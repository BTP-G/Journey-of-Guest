using System;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>JoG 网络对象的 RPC 发送服务。</summary>
    public interface INetworkRpcSender {
        void SendToOthers(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery);

        void SendToAll(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery);

        void SendToPeer(JoGNetworkObject networkObject, ulong peerId, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery);
    }
}
