using Steamworks;
using System;
using UnityEngine.Assertions;
using VContainer;
using VContainer.Unity;
using Xoderony.Networking;

namespace Expriverse.Networking.P2P {
    /// <summary>
    /// 本端 Id = (Steam AccountId &lt;&lt; 32) | Sequence；不经主机分配。
    /// 订阅 <see cref="INetworkObjectManager.Spawned"/>，对本端 Account 前缀对象抬高 Sequence（重连恢复）。
    /// 会话停止后 Sequence 重置为 1；重连后须先收到相关对象快照再 Spawn。
    /// </summary>
    /// <remarks>
    /// 待优化：水位恢复与持久化（例如 PlayerPrefs）、循环依赖解法、会话停止是否重置 Sequence 等，后续再处理。
    /// </remarks>
    public sealed class SteamNetworkObjectIdAllocator : INetworkObjectIdAllocator, IInitializable, IDisposable {
        private readonly INetworkSession _session;
        private readonly IObjectResolver _resolver;
        private readonly uint _accountId;

        private INetworkObjectManager _objectManager;
        private uint _nextSequence = 1;

        public SteamNetworkObjectIdAllocator(INetworkSession session, IObjectResolver resolver) {
            _session = session;
            _resolver = resolver;
            _accountId = SteamClient.SteamId.AccountId;
            Assert.AreNotEqual(0u, _accountId, "Steam AccountId is required for network object id allocation.");
        }

        public ulong Allocate() {
            Assert.AreNotEqual(0u, _nextSequence, "The 32-bit network object Sequence range is exhausted.");

            var sequence = _nextSequence++;
            return ((ulong)_accountId << 32) | sequence;
        }

        void IInitializable.Initialize() {
            // ObjectManager 构造依赖本 Allocator，推迟到 Initialize 再订阅，避免循环依赖。
            _objectManager = _resolver.Resolve<INetworkObjectManager>();
            _objectManager.Spawned += OnSpawned;
            _session.Stopped += OnSessionStopped;
        }

        public void Dispose() {
            if (_objectManager != null) {
                _objectManager.Spawned -= OnSpawned;
                _objectManager = null;
            }

            _session.Stopped -= OnSessionStopped;
            OnSessionStopped();
        }

        private void OnSpawned(NetworkObject networkObject) {
            var id = networkObject.Id;
            if ((uint)(id >> 32) != _accountId) {
                return;
            }

            var sequence = (uint)id;
            if (sequence >= _nextSequence) {
                _nextSequence = sequence + 1;
            }
        }

        private void OnSessionStopped() {
            _nextSequence = 1;
        }
    }
}
