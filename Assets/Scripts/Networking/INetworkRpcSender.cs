using System;

namespace JoG.Networking.P2P {
    /// <summary>JoG 网络对象的 RPC 发送服务。</summary>
    public interface INetworkRpcSender {
        void SendToOthers(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload);

        void SendToAll(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload);

        void SendToOwner(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload);

        void SendToPeer(JoGNetworkObject networkObject, ulong peerId, byte index, ReadOnlySpan<byte> payload);
    }
}
