using System.Collections.Generic;
using UnityEngine;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>值实际变化时置脏并通知；编码由项目选择的 Serializer/Deserializer 提供。</summary>
    public sealed class NetworkVariable<T> : NetworkVariableBase where T : unmanaged {
        public delegate void ValueChangedDelegate(T previousValue, T value);

        private T _value;

        public event ValueChangedDelegate ValueChanged;

        public T Value {
            get => _value;
            set {
                if (EqualityComparer<T>.Default.Equals(_value, value)) {
                    return;
                }

                var previousValue = _value;
                _value = value;
                IsDirty = true;
                ValueChanged?.Invoke(previousValue, value);
            }
        }

        public NetworkVariable(T value = default) {
            _value = value;
        }

        protected sealed override void OnSerialize(ref BufferWriter writer) {
            Debug.Assert(Serializer<T>.Serialize != null, "Network serializer is not assigned.");
            Serializer<T>.Serialize(ref writer, _value);
        }

        protected sealed override void OnDeserialize(ref BufferReader reader) {
            Debug.Assert(Deserializer<T>.Deserialize != null, "Network deserializer is not assigned.");
            var value = Deserializer<T>.Deserialize(ref reader);
            if (EqualityComparer<T>.Default.Equals(_value, value)) {
                return;
            }

            var previousValue = _value;
            _value = value;
            ValueChanged?.Invoke(previousValue, value);
        }
    }
}
