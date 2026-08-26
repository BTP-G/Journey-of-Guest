using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Logging;
using Xoderony.Numerics;

namespace Expriverse.GameplayEffects.Data {

    [Serializable]
    public sealed class StatEffectData : GameplayEffectData {

        // 玩家同一种效果/道具的叠加数量按业务约束不超过 1000；
        // 单槽最大倍率约为 1 + 9.99 * 1000 = 9991，Q16 原始值约 6.5e8，处于 Q16 与 Stat.Recalculate 的 long 中间值安全范围内。
        public static readonly Q16 MinimumMultiplierBonus = new(-999, 1000);

        public static readonly Q16 MaximumMultiplierBonus = new(999, 100);

        [SerializeField]
        private string _statKey;

        [SerializeField]
        private Q16 _multiplierBonus;

        public string StatKey {
            get => _statKey;
            set {
                if (ReadOnly) {
                    this.LogError($"{nameof(StatKey)} is readonly now!");
                    return;
                }
                _statKey = value;
            }
        }

        public Q16 MultiplierBonus {
            get => _multiplierBonus;
            set {
                if (ReadOnly) {
                    this.LogError($"{nameof(MultiplierBonus)} is readonly now!");
                    return;
                }
                _multiplierBonus = ClampMultiplierBonus(value);
            }
        }

        protected override void OnValidate() {
            base.OnValidate();
            _multiplierBonus = ClampMultiplierBonus(_multiplierBonus);
        }

        private static Q16 ClampMultiplierBonus(Q16 multiplierBonus) {
            if (multiplierBonus < MinimumMultiplierBonus) {
                return MinimumMultiplierBonus;
            }
            if (multiplierBonus > MaximumMultiplierBonus) {
                return MaximumMultiplierBonus;
            }
            return multiplierBonus;
        }
    }
}
