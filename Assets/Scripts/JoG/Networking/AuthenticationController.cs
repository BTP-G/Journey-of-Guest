using Cysharp.Threading.Tasks;
using JoG.UI.Popup;
using Unity.Services.Authentication;
using VContainer;

namespace JoG.Networking {

    internal class AuthenticationController : IAsyncBootstrapModule {
        [Inject] internal PopupManager _popupManager;
        [Inject] internal IAuthenticationService _authenticationService;

        async UniTask IAsyncBootstrapModule.InitializeAsync() {
            if (_authenticationService.IsSignedIn) {
                return;
            }

            using (_popupManager.PopupLoader()) {
                await _authenticationService.SignInAnonymouslyAsync();
            }
        }
    }
}
