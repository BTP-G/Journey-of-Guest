using JoG.Health;
using System;
using System.Runtime.CompilerServices;
using Xoderony.Numerics;

namespace JoG.GameplayEffects.Data {

    public static class DamageEffectUtility {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MatchesFlags(HealthChangeFlag flags, HealthChangeFlag requiredFlags, HealthChangeFlag excludedFlags) {
            return (requiredFlags == HealthChangeFlag.None || (flags & requiredFlags) == requiredFlags)
                && (excludedFlags == HealthChangeFlag.None || (flags & excludedFlags) == HealthChangeFlag.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateDamage(int fixedDamage, Q16 actualDamageMultiplier, int count, in HealthChangeReport report) {
            var actualDamage = Math.Max(0L, -(long)report.deltaValue);
            var scaledActualDamage = actualDamageMultiplier.Multiply(actualDamage * count);
            var damage = ((long)fixedDamage * count) + scaledActualDamage;
            return -(int)Math.Clamp(damage, 0L, HealthChangeMessage.MaxValueLong);
        }
    }
}
