using JoG.InteractionSystem;
using QuickOutline;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace JoG {

    [RequireComponent(typeof(Outline))]
    public class Teleporter : InteractableObject {
        public string nextSceneName = string.Empty;
        [Inject] private NetworkManager _networkManager;

        public override string GetString(string key) {
            if (key is "name") {
                return "´«ËÍÆ÷";
            }
            if (key is "description") {
                return "µØÍ¼´«ËÍÆ÷";
            }
            return string.Empty;
        }

        public override Interactability GetInteractability(Interactor interactor) {
            return enabled
                ? interactor.CompareTag("Player") ? Interactability.Available : Interactability.ConditionsNotMet
                : Interactability.Disabled;
        }

        public void Activate() {
            if (_networkManager.LocalClient.IsSessionOwner) {
                _networkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
        }

        public override void PreformInteraction(Interactor interactor) {
            Activate();
        }
    }
}