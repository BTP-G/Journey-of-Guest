using EditorAttributes;
using JoG.CameraSystem;
using JoG.Messages;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace JoG.LifetimeScopes {

    public class GameplaySceneScope : LifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<FocusOnUIMessage>(options);
        }
    }
}