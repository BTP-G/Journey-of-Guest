using Steamworks;
using System;
using System.Globalization;
using UnityEngine.Assertions;
using VContainer.Unity;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>
    /// Session Owner 在成员每次进房时发放新的、会话内不回收的 RangeId；
    /// Peer 取得后本地从 Sequence 1 递增。重进房换 RangeId，无需同步 Sequence。
    /// </summary>
    public sealed class SteamNetworkObjectIdAllocator : INetworkObjectIdAllocator, IInitializable, IDisposable {
        private const int RangeShift = 24;
        private const uint SequenceLimit = 1u << RangeShift;

        private readonly SteamNetworkLobby _lobby;

        private byte _rangeId;
        private uint _nextSequence;

        public SteamNetworkObjectIdAllocator(SteamNetworkLobby lobby) {
            _lobby = lobby;
        }

        public uint Allocate() {
            Assert.AreNotEqual((byte)0, _rangeId, "Network object id allocation requires a RangeId.");
            Assert.IsTrue(_nextSequence < SequenceLimit, "The 24-bit network object Sequence range is exhausted.");

            var sequence = _nextSequence++;
            return ((uint)_rangeId << RangeShift) | sequence;
        }

        void IInitializable.Initialize() {
            _lobby.Started += OnSessionStarted;
            _lobby.Stopped += ClearLocalState;
            _lobby.OwnerChanged += OnOwnerChanged;
            _lobby.MemberJoined += OnLobbyMemberJoined;
            _lobby.MemberLeft += OnLobbyMemberLeft;
            _lobby.LobbyDataChanged += TryBindLocalRangeId;
        }

        public void Dispose() {
            _lobby.Started -= OnSessionStarted;
            _lobby.Stopped -= ClearLocalState;
            _lobby.OwnerChanged -= OnOwnerChanged;
            _lobby.MemberJoined -= OnLobbyMemberJoined;
            _lobby.MemberLeft -= OnLobbyMemberLeft;
            _lobby.LobbyDataChanged -= TryBindLocalRangeId;

            ClearLocalState();
        }

        private void OnSessionStarted() {
            ClearLocalState();

            if (_lobby.IsOwner) {
                EnsureMissingMemberRangeIds();
            }

            TryBindLocalRangeId();
        }

        private void OnOwnerChanged(ulong previousOwnerPeerId, ulong ownerPeerId) {
            if (_lobby.IsOwner) {
                EnsureMissingMemberRangeIds();
            }

            TryBindLocalRangeId();
        }

        private void OnLobbyMemberJoined(ulong peerId) {
            if (!_lobby.IsOwner) {
                return;
            }

            AssignPeerRangeId(peerId);
        }

        private void OnLobbyMemberLeft(ulong peerId) {
            if (!_lobby.IsOwner) {
                return;
            }

            // 清除映射，避免重进短暂读到旧 RangeId 后从 Sequence 1 与持久对象冲突。
            _lobby.Lobby.DeleteData(NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId));
        }

        private void TryBindLocalRangeId() {
            if (!TryGetPeerRangeId(SteamClient.SteamId, out var rangeId) || _rangeId == rangeId) {
                return;
            }

            Assert.AreEqual((byte)0, _rangeId, "Lobby RangeId changed for the local peer after allocation started.");

            _rangeId = rangeId;
            _nextSequence = 1;
            _lobby.Lobby.SetMemberData(NetworkObjectIdLobbyKeys.IdReadyKey, NetworkObjectIdLobbyKeys.IdReadyValue);
        }

        private void EnsureMissingMemberRangeIds() {
            foreach (var member in _lobby.Lobby.Members) {
                if (!TryGetPeerRangeId(member.Id, out _)) {
                    AssignPeerRangeId(member.Id);
                }
            }
        }

        private void AssignPeerRangeId(ulong peerId) {
            var rangeId = AllocateRangeId();
            var wrote = _lobby.Lobby.SetData(
                NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId),
                rangeId.ToString(CultureInfo.InvariantCulture));
            Assert.IsTrue(wrote, "Failed to write the network object id RangeId.");
        }

        private byte AllocateRangeId() {
            var nextRangeId = ReadNextRangeId();
            Assert.IsTrue(nextRangeId <= byte.MaxValue, "The 8-bit network object RangeId space is exhausted.");

            var wrote = _lobby.Lobby.SetData(
                NetworkObjectIdLobbyKeys.NextRangeIdCounterKey,
                (nextRangeId + 1).ToString(CultureInfo.InvariantCulture));
            Assert.IsTrue(wrote, "Failed to write the next network object RangeId counter.");

            return (byte)nextRangeId;
        }

        private uint ReadNextRangeId() {
            var value = _lobby.Lobby.GetData(NetworkObjectIdLobbyKeys.NextRangeIdCounterKey);
            if (value.Length != 0) {
                return uint.Parse(value, CultureInfo.InvariantCulture);
            }

            uint nextRangeId = 1;
            foreach (var pair in _lobby.Lobby.Data) {
                if (!NetworkObjectIdLobbyKeys.IsPeerRangeIdKey(pair.Key)) {
                    continue;
                }

                var assignedRangeId = byte.Parse(pair.Value, CultureInfo.InvariantCulture);
                nextRangeId = Math.Max(nextRangeId, (uint)assignedRangeId + 1);
            }

            return nextRangeId;
        }

        private bool TryGetPeerRangeId(ulong peerId, out byte rangeId) {
            var value = _lobby.Lobby.GetData(NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId));
            if (value.Length == 0) {
                rangeId = 0;
                return false;
            }

            rangeId = byte.Parse(value, CultureInfo.InvariantCulture);
            return true;
        }

        private void ClearLocalState() {
            _rangeId = 0;
            _nextSequence = 0;
        }
    }
}
