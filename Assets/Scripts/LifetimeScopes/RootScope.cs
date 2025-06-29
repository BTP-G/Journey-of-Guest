using Cysharp.Threading.Tasks;
using JoG.Networking;
using JoG.UI;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace JoG.LifetimeScopes {

    public class RootScope : LifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            var unityServices = UnityServices.Instance;
            builder.RegisterInstance(unityServices);
            builder.RegisterInstance(unityServices.GetAuthenticationService());
            builder.RegisterInstance(unityServices.GetLobbyService());
            builder.RegisterInstance(unityServices.GetMatchmakerService());
            builder.RegisterInstance(unityServices.GetMultiplayerService());
            builder.RegisterInstance(unityServices.GetPlayerAccountService());
            builder.RegisterInstance(unityServices.GetQosService());
            builder.RegisterInstance(unityServices.GetRelayService());
            builder.RegisterInstance(InputSystem.actions);
            builder.RegisterComponent(NetworkManager.Singleton);
            builder.RegisterEntryPoint<SessionManager>();
        }
    }
}