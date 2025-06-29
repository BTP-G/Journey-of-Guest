using Cysharp.Threading.Tasks;
using EditorAttributes;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#nullable enable

namespace JoG {

    [Serializable]
    public struct Optional<T> : ISerializationCallbackReceiver where T : UnityEngine.Object {
        private bool _hasValue;

        [SerializeField, IndentProperty] private T? _value;

        /// <summary>hasValue = value is not null</summary>
        public Optional(T value) => _hasValue = _value = value;

        public Optional(bool hasValue, T value) {
            _hasValue = hasValue;
            _value = value;
        }

        public readonly bool HasValue => _hasValue;

        public readonly T? Value => _value;

        public static implicit operator T?(in Optional<T> optional) => optional._value;

        public static implicit operator Optional<T>(T value) => new(value);

        public readonly bool TryGet([NotNullWhen(true)] out T? value) {
            value = _value;
            return _hasValue;
        }

        public override readonly string ToString() => _hasValue ? _value!.ToString() : string.Empty;

        public void Validate() => _hasValue = _value;

        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
#if UNITY_EDITOR
            if (PlayerLoopHelper.IsMainThread)
#endif
                _hasValue = _value;
        }
    }
}