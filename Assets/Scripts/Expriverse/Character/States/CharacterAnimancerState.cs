using Animancer;
using Expriverse.States;
using VContainer;

namespace Expriverse.Character.States {

    public abstract class CharacterAnimancerState : State {
        [Inject]
        internal AnimancerComponent animancer;

        protected AnimancerState Play(ITransition transition, int layerIndex = 0) {
            return animancer.Layers[layerIndex].Play(transition);
        }
    }
}
