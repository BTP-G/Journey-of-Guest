using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.Services.Authentication;
using VContainer;
using Xoderony.Logging;

namespace JoG.Networking {

    internal class AuthenticationController : IAsyncBootstrapModule {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        [Inject] internal IAuthenticationService _authenticationService;

        async UniTask IAsyncBootstrapModule.InitializeAsync(CancellationToken cancellationToken) {
            while (!_authenticationService.IsSignedIn) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    await _authenticationService.SignInAnonymouslyAsync();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception exception) {
                    this.LogException(exception);
                    await UniTask.Delay(RetryDelay, cancellationToken: cancellationToken);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
