using Steamworks;
using System;
using System.Globalization;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>
    /// Steam Lobby Owner 为每个 SteamID 分配 8 位 RangeId，并按块预留 24 位 Sequence。
    /// 本地从已授权区间同步分配对象 id，Lobby Data 不进入对象生成热路径。
    /// </summary>
    public sealed class SteamNetworkObjectIdAllocator : INetworkObjectIdAllocator, IInitializable, IDisposable {
        private const int RangeShift = 24;
        private const uint SequenceLimit = 1u << RangeShift;
        private const uint ReservationSize = 1u << 18;
        private const uint RefillThreshold = 1u << 16;

        private readonly SteamNetworkSession _session;

        private byte _rangeId;
        private uint _nextSequence;
        private uint _reservedEnd;

        // 仅 Lobby Owner 使用。
        private uint _nextRangeId;

        // null 表示当前没有等待处理的区间请求。
        // 0 表示首次请求，否则表示请求时已知的 ReservedEnd。
        private uint? _pendingRequestEnd;

        public SteamNetworkObjectIdAllocator(SteamNetworkSession session) {
            _session = session;
        }

        public uint Allocate() {
            Debug.Assert(
                _rangeId != 0 && _nextSequence < _reservedEnd,
                "Network object id allocation requires a reserved Sequence.");

            var sequence = _nextSequence++;
            var id = ((uint)_rangeId << RangeShift) | sequence;

            RequestRefillIfNeeded();

            return id;
        }

        void IInitializable.Initialize() {
            _session.Started += OnSessionStarted;
            _session.Stopped += OnSessionStopped;
            _session.OwnerChanged += OnOwnerChanged;
            _session.LobbyDataChanged += OnLobbyDataChanged;
            _session.LobbyMemberDataChanged += OnLobbyMemberDataChanged;

            if (_session.IsJoined) {
                OnSessionStarted();
            }
        }

        public void Dispose() {
            _session.Started -= OnSessionStarted;
            _session.Stopped -= OnSessionStopped;
            _session.OwnerChanged -= OnOwnerChanged;
            _session.LobbyDataChanged -= OnLobbyDataChanged;
            _session.LobbyMemberDataChanged -= OnLobbyMemberDataChanged;

            ResetSessionState();
        }

        private void OnSessionStarted() {
            ResetSessionState();
            RequestInitialRange();

            if (_session.IsOwner) {
                ProcessMemberRequests();
            }
        }

        private void OnSessionStopped() {
            ResetSessionState();
        }

        private void OnOwnerChanged(ulong previousOwnerPeerId, ulong ownerPeerId) {
            _nextRangeId = 0;

            TryApplyLocalGrant();

            if (_session.IsOwner) {
                ProcessMemberRequests();
            }
        }

        private void OnLobbyMemberDataChanged(Friend member) {
            if (_session.IsOwner) {
                ProcessMemberRequest(member);
            }
        }

        private void OnLobbyDataChanged() {
            if (_session.IsJoined) {
                TryApplyLocalGrant();
            }
        }

        private void RequestInitialRange() {
            if (TryReadPeerState(SteamClient.SteamId, out _, out var reservedEnd)) {
                RequestRange(reservedEnd);
            } else {
                RequestRange(0);
            }
        }

        private void RequestRefillIfNeeded() {
            if (_pendingRequestEnd.HasValue) {
                return;
            }

            if (_reservedEnd - _nextSequence <= RefillThreshold) {
                RequestRange(_reservedEnd);
            }
        }

        private void RequestRange(uint requestedEnd) {
            _pendingRequestEnd = requestedEnd;

            _session.Lobby.SetMemberData(
                NetworkObjectIdLobbyData.RangeRequestKey,
                requestedEnd.ToString(CultureInfo.InvariantCulture));

            if (_session.IsOwner) {
                ReserveRange(SteamClient.SteamId, requestedEnd);
            }
        }

        private void ProcessMemberRequests() {
            foreach (var member in _session.Lobby.Members) {
                ProcessMemberRequest(member);
            }
        }

        private void ProcessMemberRequest(Friend member) {
            var value = _session.Lobby.GetMemberData(
                member,
                NetworkObjectIdLobbyData.RangeRequestKey);

            if (value.Length == 0) {
                return;
            }

            var requestedEnd = uint.Parse(
                value,
                CultureInfo.InvariantCulture);

            ReserveRange(member.Id, requestedEnd);
        }

        private void ReserveRange(ulong peerId, uint requestedEnd) {
            byte rangeId;
            uint start;

            if (TryReadPeerState(peerId, out rangeId, out var reservedEnd)) {
                // 请求必须基于当前最新的 ReservedEnd。
                // 旧请求或重复请求直接忽略。
                if (reservedEnd != requestedEnd) {
                    return;
                }

                start = reservedEnd;
            } else {
                if (requestedEnd != 0) {
                    Debug.Assert(
                        false,
                        "The initial network object id range request is invalid.");

                    return;
                }

                // RangeId 与 ReservedEnd 分开写入。
                // 如果之前只成功写入了 RangeId，则直接复用，
                // 避免因为一次不完整写入而重复分配 RangeId。
                if (!TryReadPeerRangeId(peerId, out rangeId)) {
                    rangeId = AllocateRangeId();

                    if (!PublishPeerRangeId(peerId, rangeId)) {
                        return;
                    }
                }

                start = 1;
            }

            Debug.Assert(
                start <= SequenceLimit - ReservationSize,
                "The 24-bit network object Sequence range is exhausted.");

            var endExclusive = start + ReservationSize;

            if (!PublishPeerReservedEnd(peerId, endExclusive)) {
                return;
            }

            if (peerId == SteamClient.SteamId &&
                _pendingRequestEnd == requestedEnd) {
                _pendingRequestEnd = null;
                ApplyGrant(rangeId, start, endExclusive);
            }
        }

        private byte AllocateRangeId() {
            if (_nextRangeId == 0) {
                _nextRangeId = 1;

                foreach (var pair in _session.Lobby.Data) {
                    if (!pair.Key.StartsWith(
                        NetworkObjectIdLobbyData.PeerRangeIdKeyPrefix,
                        StringComparison.Ordinal)) {
                        continue;
                    }

                    var assignedRangeId = byte.Parse(
                        pair.Value,
                        CultureInfo.InvariantCulture);

                    _nextRangeId = Math.Max(
                        _nextRangeId,
                        (uint)assignedRangeId + 1);
                }
            }

            Debug.Assert(
                _nextRangeId <= byte.MaxValue,
                "The 8-bit network object RangeId space is exhausted.");

            return (byte)_nextRangeId++;
        }

        private bool TryReadPeerState(
            ulong peerId,
            out byte rangeId,
            out uint reservedEnd) {

            var reservedEndValue = _session.Lobby.GetData(
                GetPeerReservedEndKey(peerId));

            if (reservedEndValue.Length == 0) {
                rangeId = 0;
                reservedEnd = 0;
                return false;
            }

            if (!TryReadPeerRangeId(peerId, out rangeId)) {
                Debug.Assert(
                    false,
                    "Network object id peer state has ReservedEnd but no RangeId.");

                reservedEnd = 0;
                return false;
            }

            reservedEnd = uint.Parse(
                reservedEndValue,
                CultureInfo.InvariantCulture);

            return true;
        }

        private bool TryReadPeerRangeId(
            ulong peerId,
            out byte rangeId) {

            var value = _session.Lobby.GetData(
                GetPeerRangeIdKey(peerId));

            if (value.Length == 0) {
                rangeId = 0;
                return false;
            }

            rangeId = byte.Parse(
                value,
                CultureInfo.InvariantCulture);

            return true;
        }

        private void TryApplyLocalGrant() {
            if (!_pendingRequestEnd.HasValue ||
                !TryReadPeerState(
                    SteamClient.SteamId,
                    out var rangeId,
                    out var reservedEnd)) {
                return;
            }

            var requestedEnd = _pendingRequestEnd.Value;

            // 首次分配从 Sequence 1 开始。
            // 后续分配从请求时的 ReservedEnd 开始。
            var start = requestedEnd == 0
                ? 1u
                : requestedEnd;

            var expectedEnd = start + ReservationSize;

            // ReservedEnd 只有等于本次请求对应的新边界，
            // 才说明当前看到的是本次请求的授权结果。
            if (reservedEnd != expectedEnd) {
                return;
            }

            _pendingRequestEnd = null;
            ApplyGrant(rangeId, start, reservedEnd);
        }

        private void ApplyGrant(
            byte rangeId,
            uint start,
            uint endExclusive) {

            var isInitialGrant = _rangeId == 0;

            if (isInitialGrant) {
                _rangeId = rangeId;
                _nextSequence = start;
            } else {
                Debug.Assert(
                    rangeId == _rangeId &&
                    start == _reservedEnd,
                    "The network object id refill is not contiguous.");
            }

            _reservedEnd = endExclusive;

            if (isInitialGrant) {
                _session.Lobby.SetMemberData(
                    NetworkObjectIdLobbyData.ReadyKey,
                    NetworkObjectIdLobbyData.ReadyValue);
            }
        }

        private bool PublishPeerRangeId(
            ulong peerId,
            byte rangeId) {

            var published = _session.Lobby.SetData(
                GetPeerRangeIdKey(peerId),
                rangeId.ToString(CultureInfo.InvariantCulture));

            Debug.Assert(
                published,
                "Failed to publish the network object id RangeId.");

            return published;
        }

        private bool PublishPeerReservedEnd(
            ulong peerId,
            uint reservedEnd) {

            var published = _session.Lobby.SetData(
                GetPeerReservedEndKey(peerId),
                reservedEnd.ToString(CultureInfo.InvariantCulture));

            Debug.Assert(
                published,
                "Failed to publish the network object id ReservedEnd.");

            return published;
        }

        private void ResetSessionState() {
            _rangeId = 0;
            _nextSequence = 0;
            _reservedEnd = 0;
            _nextRangeId = 0;
            _pendingRequestEnd = null;
        }

        private static string GetPeerRangeIdKey(ulong peerId) {
            return NetworkObjectIdLobbyData.PeerRangeIdKeyPrefix
                + peerId.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetPeerReservedEndKey(ulong peerId) {
            return NetworkObjectIdLobbyData.PeerReservedEndKeyPrefix
                + peerId.ToString(CultureInfo.InvariantCulture);
        }
    }
}
