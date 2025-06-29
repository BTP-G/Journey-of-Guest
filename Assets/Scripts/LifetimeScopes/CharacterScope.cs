using JoG.Character;
using JoG.Messages;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace JoG.LifetimeScopes {

    public class CharacterScope : LifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            var options = builder.RegisterMessagePipe();
            options.InstanceLifetime = InstanceLifetime.Scoped;
            builder.RegisterMessageBroker<CharacterBodyChangedMessage>(options);
            builder.RegisterComponent(GetComponent<CharacterMaster>());
        }
    }
}