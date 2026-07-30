using Xoderony;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Health {

    public class HealthComponent : NetworkBehaviour, IComponent {

        private readonly NetworkVariable<int> _networkCurrent = new(writePerm: NetworkVariableWritePermission.Owner);

        private int _localCurrent;

        private int _max = 1;

        [Inject] internal Entity entity;

        [Inject] internal IDelegateDispatcher<HealthChangedHandler> healthChangedHandlers;

        public Entity Entity => entity;

        public int Current {
            get => _localCurrent;
            set {
                if (IsOwner) {
                    _networkCurrent.Value = Mathf.Clamp(value, 0, _max);
                } else {
                    _localCurrent = Mathf.Clamp(value, 0, _max);
                }
            }
        }

        public int Max {
            get => _max;
            set {
                var oldMax = _max;
                var oldCurrent = _localCurrent;
                _max = Mathf.Max(1, value);
                var scaled = ((int)((((long)oldCurrent) * _max) + (oldMax / 2))) / oldMax;
                Current = Mathf.Clamp(scaled, 0, _max);
            }
        }

        public float Ratio => Mathf.Clamp01(((float)_localCurrent) / _max);

        public bool IsAlive => _localCurrent > 0;

        public bool IsDead => _localCurrent <= 0;

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            serializer.SerializeValue(ref _localCurrent);
        }

        private void Awake() {
            _networkCurrent.OnValueChanged = OnValueChanged;
        }

        private void OnValueChanged(int prev, int next) {
            _localCurrent = next;
            healthChangedHandlers.Handlers?.Invoke(prev, next);
        }

    }

}
