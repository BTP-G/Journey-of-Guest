using Xoderony.Networking.Messaging;

namespace JoG.Networking.P2P {
    /// <summary>JoG 项目拥有的对象扩展协议。</summary>
    internal static class NetworkObjectMessageType {
        public const byte State = NetworkMessageType.User;
        public const byte Rpc = NetworkMessageType.User + 1;
    }
}
