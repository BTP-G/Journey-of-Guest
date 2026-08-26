using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;
using Xoderony.Logging;
using Xoderony.Networking;

namespace Expriverse.Networking.P2P {
    /// <summary>Owner 转移到本端后延迟销毁，验证离开玩家对象的生命周期策略。</summary>
    public sealed class P2PValidationPlayerObject : P2PValidationNetworkObject {
        [Inject] internal INetworkObjectManager _objectManager;
        [Inject] internal INetworkSession _session;

        protected override void OnOwnerChanged(ulong previousOwnerPeerId, ulong newOwnerPeerId) {
            if (previousOwnerPeerId == 0 || newOwnerPeerId == 0 || previousOwnerPeerId == newOwnerPeerId || newOwnerPeerId != _session.LocalPeerId) {
                return;
            }

            DestroyAfterOwnerTransferAsync(destroyCancellationToken).Forget(HandleDelayedDestroyException);
        }

        private async UniTask DestroyAfterOwnerTransferAsync(CancellationToken cancellationToken) {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
            if (!IsSpawned || OwnerPeerId != _session.LocalPeerId) {
                return;
            }

            _objectManager.Despawn(this);
        }

        private void HandleDelayedDestroyException(Exception exception) {
            if (exception is not OperationCanceledException) {
                this.LogException(exception);
            }
        }
    }
}
