using System;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>绑定到 JoG 网络对象且固定发送给所有 Peer 的强类型 RPC 端点。</summary>
    public sealed class NetworkAllRpc<T> : NetworkRpcBase<T> where T : unmanaged {
        public void Send(in T value) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            Serialize(ref writer, value);
            NetworkObject.SendRpcToAll(Index, writer.Written);
        }
    }
}
