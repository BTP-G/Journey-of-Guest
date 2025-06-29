using JoG.Messages;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character {

    [DisallowMultipleComponent]
    public class CharacterMaster : NetworkBehaviour {
        [Inject] private IBufferedPublisher<CharacterBodyChangedMessage> _bodyChangedMessagePublisher;

        public CharacterBody Body { get; private set; }

        protected virtual void OnBodyChanged(CharacterBody previous, CharacterBody next) {
            if (previous != null && next == null) {
                NetworkObject.Despawn();
            }
        }

        protected virtual void OnTransformChildrenChanged() {
            var nextBody = GetComponentInChildren<CharacterBody>();
            if (nextBody == Body) return;
            var message = new CharacterBodyChangedMessage() { previous = Body, next = nextBody };
            Body = nextBody;
            _bodyChangedMessagePublisher.Publish(message);
            OnBodyChanged(message.previous, message.next);
        }
    }
}