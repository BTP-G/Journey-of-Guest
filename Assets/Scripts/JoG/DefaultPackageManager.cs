using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;
using Xoderony.Logging;

namespace JoG {

    internal class DefaultPackageManager : IAsyncBootstrapModule {
        private const string DefaultPackageName = "DefaultPackage";
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        [Inject] internal IResourcePackageLoader _resourcePackageLoader;

        async UniTask IAsyncBootstrapModule.InitializeAsync(CancellationToken cancellationToken) {
            while (true) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    await _resourcePackageLoader.LoadPackageAsync(DefaultPackageName, cancellationToken: cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return;
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception exception) {
                    this.LogException(exception);
                    await UniTask.Delay(RetryDelay, cancellationToken: cancellationToken);
                }
            }
        }
    }
}
