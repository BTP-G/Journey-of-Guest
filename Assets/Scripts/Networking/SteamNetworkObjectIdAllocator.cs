using Steamworks;
using System;
using System.Globalization;
using UnityEngine;
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

        private readonly SteamNetworkSession _session;

        private byte _rangeId;
        private uint _nextSequence;

        public SteamNetworkObjectIdAllocator(SteamNetworkSession session) {
            _session = session;
        }

        public uint Allocate() {
            Debug.Assert(_rangeId != 0, "Network object id allocation requires a RangeId.");
            Debug.Assert(_nextSequence < SequenceLimit, "The 24-bit network object Sequence range is exhausted.");

            var sequence = _nextSequence++;
            return ((uint)_rangeId << RangeShift) | sequence;
        }

        void IInitializable.Initialize() {
            _session.Started += OnSessionStarted;
            _session.Stopped += ClearLocalState;
            _session.OwnerChanged += OnOwnerChanged;
            _session.MemberJoined += OnMemberJoined;
            _session.MemberLeft += OnMemberLeft;
            _session.LobbyDataChanged += TryBindLocalRangeId;
        }

        public void Dispose() {
            _session.Started -= OnSessionStarted;
            _session.Stopped -= ClearLocalState;
            _session.OwnerChanged -= OnOwnerChanged;
            _session.MemberJoined -= OnMemberJoined;
            _session.MemberLeft -= OnMemberLeft;
            _session.LobbyDataChanged -= TryBindLocalRangeId;

            ClearLocalState();
        }

        private void OnSessionStarted() {
            ClearLocalState();

            if (_session.IsOwner) {
                EnsureMissingMemberRangeIds();
            }

            TryBindLocalRangeId();
        }

        private void OnOwnerChanged(ulong previousOwnerPeerId, ulong ownerPeerId) {
            if (_session.IsOwner) {
                EnsureMissingMemberRangeIds();
            }

            TryBindLocalRangeId();
        }

        private void OnMemberJoined(ulong peerId) {
            if (!_session.IsOwner) {
                return;
            }

            AssignPeerRangeId(peerId);
        }

        private void OnMemberLeft(ulong peerId) {
            if (!_session.IsOwner) {
                return;
            }

            // 清除映射，避免重进短暂读到旧 RangeId 后从 Sequence 1 与持久对象冲突。
            _session.Lobby.DeleteData(NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId));
        }

        private void TryBindLocalRangeId() {
            if (!TryGetPeerRangeId(SteamClient.SteamId, out var rangeId) || _rangeId == rangeId) {
                return;
            }

            Debug.Assert(_rangeId == 0, "Lobby RangeId changed for the local peer after allocation started.");

            _rangeId = rangeId;
            _nextSequence = 1;
            _session.Lobby.SetMemberData(NetworkObjectIdLobbyKeys.IdReadyKey, NetworkObjectIdLobbyKeys.IdReadyValue);
        }

        private void EnsureMissingMemberRangeIds() {
            foreach (var member in _session.Lobby.Members) {
                if (!TryGetPeerRangeId(member.Id, out _)) {
                    AssignPeerRangeId(member.Id);
                }
            }
        }

        private void AssignPeerRangeId(ulong peerId) {
            var rangeId = AllocateRangeId();
            var wrote = _session.Lobby.SetData(
                NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId),
                rangeId.ToString(CultureInfo.InvariantCulture));
            Debug.Assert(wrote, "Failed to write the network object id RangeId.");
        }

        private byte AllocateRangeId() {
            var nextRangeId = ReadNextRangeId();
            Debug.Assert(nextRangeId <= byte.MaxValue, "The 8-bit network object RangeId space is exhausted.");

            var wrote = _session.Lobby.SetData(
                NetworkObjectIdLobbyKeys.NextRangeIdCounterKey,
                (nextRangeId + 1).ToString(CultureInfo.InvariantCulture));
            Debug.Assert(wrote, "Failed to write the next network object RangeId counter.");

            return (byte)nextRangeId;
        }

        private uint ReadNextRangeId() {
            var value = _session.Lobby.GetData(NetworkObjectIdLobbyKeys.NextRangeIdCounterKey);
            if (value.Length != 0) {
                return uint.Parse(value, CultureInfo.InvariantCulture);
            }

            uint nextRangeId = 1;
            foreach (var pair in _session.Lobby.Data) {
                if (!NetworkObjectIdLobbyKeys.IsPeerRangeIdKey(pair.Key)) {
                    continue;
                }

                var assignedRangeId = byte.Parse(pair.Value, CultureInfo.InvariantCulture);
                nextRangeId = Math.Max(nextRangeId, (uint)assignedRangeId + 1);
            }

            return nextRangeId;
        }

        private bool TryGetPeerRangeId(ulong peerId, out byte rangeId) {
            var value = _session.Lobby.GetData(NetworkObjectIdLobbyKeys.PeerRangeIdKey(peerId));
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
