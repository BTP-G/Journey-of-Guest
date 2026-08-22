using System;
using System.Globalization;
using Xoderony.Networking.Messaging;

namespace JoG.Networking.P2P {
    /// <summary>JoG 项目拥有的对象扩展协议。</summary>
    internal static class NetworkObjectMessageType {
        public const byte NetworkVariable = NetworkMessageType.User;
        public const byte Rpc = NetworkMessageType.User + 1;
    }

    /// <summary>Steam Lobby 中的网络对象 id 分配键。</summary>
    internal static class NetworkObjectIdLobbyKeys {
        private const string KeyPrefix = "network.id.";
        private const string PeerRangeIdKeyPrefix = KeyPrefix + "peer.range";

        public const string NextRangeIdCounterKey = KeyPrefix + "range.next";
        public const string IdReadyKey = KeyPrefix + "ready";
        public const string IdReadyValue = "1";

        public static string PeerRangeIdKey(ulong peerId) {
            return PeerRangeIdKeyPrefix + peerId.ToString(CultureInfo.InvariantCulture);
        }

        public static bool IsPeerRangeIdKey(string key) {
            return key.StartsWith(PeerRangeIdKeyPrefix, StringComparison.Ordinal);
        }

        public static bool IsIdReady(string memberData) {
            return memberData == IdReadyValue;
        }
    }
}
