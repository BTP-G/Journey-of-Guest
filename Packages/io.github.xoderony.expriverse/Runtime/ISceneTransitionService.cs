using Cysharp.Threading.Tasks;
using System.Threading;

namespace Expriverse {

    public interface ISceneTransitionService {
        UniTask TransitionToSceneAsync(string packageName, string location, CancellationToken cancellationToken = default);
    }

    public static class SceneTransitionServiceExtensions {
        private const string DefaultPackageName = "DefaultPackage";
        private const string MainMenuSceneLocation = "MainMenuScene";

        public static UniTask TransitionToMainMenuSceneAsync(this ISceneTransitionService service, CancellationToken cancellationToken = default) {
            return service.TransitionToSceneAsync(DefaultPackageName, MainMenuSceneLocation, cancellationToken);
        }
    }
}
