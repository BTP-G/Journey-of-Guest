using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>绑定到 JoG 网络对象的 RPC 端点基类。</summary>
    public abstract class NetworkRpcBase {
        private JoGNetworkObject _networkObject;
        private byte _index;

        protected JoGNetworkObject NetworkObject => _networkObject;

        protected byte Index => _index;

        internal void Bind(JoGNetworkObject networkObject, byte index) {
            _networkObject = networkObject;
            _index = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Deserialize(ulong senderPeerId, ref BufferReader reader) {
            OnDeserialize(senderPeerId, ref reader);
        }

        protected abstract void OnDeserialize(ulong senderPeerId, ref BufferReader reader);
    }

    public abstract class NetworkRpcBase<T> : NetworkRpcBase where T : unmanaged {
        public delegate void ReceivedHandler(ulong senderPeerId, in T value);

        public ReceivedHandler Received;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Serialize(ref BufferWriter writer, in T value) {
            Assert.IsNotNull(Serializer<T>.Serialize, $"Serializer<{typeof(T).Name}> is not registered.");
            Serializer<T>.Serialize(ref writer, value);
        }

        protected sealed override void OnDeserialize(ulong senderPeerId, ref BufferReader reader) {
            Assert.IsNotNull(Deserializer<T>.Deserialize, $"Deserializer<{typeof(T).Name}> is not registered.");
            var value = Deserializer<T>.Deserialize(ref reader);
            Received?.Invoke(senderPeerId, in value);
        }
    }
}
