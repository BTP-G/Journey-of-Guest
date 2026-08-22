using System;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>绑定到 JoG 网络对象且发送目标由调用方指定的强类型 RPC 端点。</summary>
    public sealed class NetworkPeerRpc<T> : NetworkRpcBase<T> where T : unmanaged {
        public void SendToPeer(ulong peerId, in T value) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            Serialize(ref writer, value);
            NetworkObject.SendRpcToPeer(peerId, Index, writer.Written);
        }
    }
}
