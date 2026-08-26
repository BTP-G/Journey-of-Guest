using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Logging;
using Xoderony.Numerics;

namespace Expriverse.GameplayEffects.Data {

    [Serializable]
    public abstract class HealthChangeValueModifierEffectData : GameplayEffectData {

        [SerializeField]
        private Q16 _multiplierBonus = new(1, 10);

        public Q16 MultiplierBonus {
            get => _multiplierBonus;
            set {
                if (ReadOnly) {
                    this.LogError($"{nameof(MultiplierBonus)} is readonly now!");
                    return;
                }
                _multiplierBonus = value;
            }
        }
    }

    [Serializable]
    public sealed class OutgoingHealthChangeValueModifierEffectData : HealthChangeValueModifierEffectData { }

    [Serializable]
    public sealed class IncomingHealthChangeValueModifierEffectData : HealthChangeValueModifierEffectData { }
}
