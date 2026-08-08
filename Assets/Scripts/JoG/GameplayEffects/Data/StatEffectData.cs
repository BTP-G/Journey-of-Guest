using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Logging;
using Xoderony.Numerics;

namespace JoG.GameplayEffects.Data {

    [Serializable]
    public sealed class StatEffectData : GameplayEffectData {

        public static readonly Q16 MinimumMultiplierBonus = new(-999, 1000);

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
            return multiplierBonus < MinimumMultiplierBonus ? MinimumMultiplierBonus : multiplierBonus;
        }
    }
}
