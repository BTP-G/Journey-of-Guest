using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Character {

    public partial class CharacterBody : IHealth {
        private readonly NetworkVariable<uint> _hp = new(0, writePerm: NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<uint> _maxHP = new(1, writePerm: NetworkVariableWritePermission.Owner);
        private List<DamageMessageModifier> _damageMessageModifiers = new();
        private List<HealingMessageModifier> _healingMessageModifiers = new();
        private List<DamageReportHandler> _damageReportHandlers = new();
        private List<HealingReportHandler> _healingReportHandlers = new();
        [SerializeField] private uint _maxHPBase = 1000;
        private float _maxHPCoefficient = 1;

        /// <summary>NetworkVariable. Write: OwnerOnly</summary>
        public uint HP {
            get => _hp.Value;
            set => _hp.Value = value > _maxHP.Value ? _maxHP.Value : value;
        }

        /// <summary>NetworkVariable. Write: OwnerOnly</summary>
        public uint MaxHP {
            get => _maxHP.Value;
            private set {
                var maxHP = value is 0 ? 1 : value;
                if (_hp.Value > maxHP) {
                    _hp.Value = maxHP;
                }
                _maxHP.Value = maxHP;
            }
        }

        public uint MaxHPBase {
            get => _maxHPBase;
            set {
                _maxHPBase = value is 0 ? 1 : value;
                if (IsOwner) {
                    MaxHP = (uint)Math.Round(_maxHPBase * _maxHPCoefficient);
                }
            }
        }

        public float MaxHPCoefficient {
            get => _maxHPCoefficient;
            set {
                _maxHPCoefficient = value > float.Epsilon ? value : float.Epsilon;
                if (IsOwner) {
                    MaxHP = (uint)Math.Round(_maxHPBase * _maxHPCoefficient);
                }
            }
        }

        public bool IsAlive => _hp.Value is not 0;

        public float PercentHp => (float)_hp.Value / _maxHP.Value;

        public event DamageMessageModifier OnModifyDamageMessage {
            add => _damageMessageModifiers.Add(value);
            remove => _damageMessageModifiers.Remove(value);
        }

        public event DamageReportHandler OnHandleDamageReport {
            add => _damageReportHandlers.Add(value);
            remove => _damageReportHandlers.Remove(value);
        }

        public event HealingMessageModifier OnModifyHealingMessage {
            add => _healingMessageModifiers.Add(value);
            remove => _healingMessageModifiers.Remove(value);
        }

        public event HealingReportHandler OnHandleHealingReport {
            add => _healingReportHandlers.Add(value);
            remove => _healingReportHandlers.Remove(value);
        }

        public void Handle(in DamageMessage message) => HandleRpc(message);

        public void Handle(in HealingMessage message) => HandleRpc(message);

        [Rpc(SendTo.Owner)]
        private void HandleRpc(DamageMessage message) {
            foreach (var modefier in _damageMessageModifiers.AsSpan()) {
                modefier(ref message);
            }
            var damageReport = new DamageReport {
                deltaDamage = (uint)(message.value * message.cofficient),
                flag = message.flags,
                position = message.position,
                impulse = message.impulse,
            };
            if (damageReport.deltaDamage >= HP) {
                damageReport.deltaDamage = HP;
                HP = 0;
            } else {
                HP -= damageReport.deltaDamage;
            }
            foreach (var handler in _damageReportHandlers.AsSpan()) {
                handler(damageReport);
            }
        }

        [Rpc(SendTo.Owner)]
        private void HandleRpc(HealingMessage message) {
            foreach (var modify in _healingMessageModifiers.AsSpan()) {
                modify(ref message);
            }
            var healingReport = new HealingReport {
                deltaHealing = (uint)(message.value * message.cofficient),
                flag = message.flags,
            };
            HP += healingReport.deltaHealing;
            foreach (var handler in _healingReportHandlers.AsSpan()) {
                handler(healingReport);
            }
        }

        public delegate void DamageMessageModifier(ref DamageMessage message);

        public delegate void DamageReportHandler(in DamageReport report);

        public delegate void HealingMessageModifier(ref HealingMessage message);

        public delegate void HealingReportHandler(in HealingReport report);
    }
}