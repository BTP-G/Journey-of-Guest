using Xoderony.Networking.Messaging;

namespace JoG.Networking.P2P {
    /// <summary>JoG 项目拥有的对象扩展协议。</summary>
    internal static class NetworkObjectMessageType {
        public const byte State = NetworkMessageType.User;
        public const byte Rpc = NetworkMessageType.User + 1;
    }

    /// <summary>Steam Lobby 中的网络对象 id 分配状态字段。</summary>
    internal static class NetworkObjectIdLobbyData {
        public const string PeerRangeIdKeyPrefix = "network.id.peer.range";
        public const string PeerReservedEndKeyPrefix = "network.id.peer.end";
        public const string RangeRequestKey = "network.id.request";
        public const string ReadyKey = "network.id.ready";
        public const string ReadyValue = "1";
    }
}
