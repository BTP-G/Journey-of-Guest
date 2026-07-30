using Animancer;
using JoG.States;
using VContainer;

namespace JoG.Character.States {

    public abstract class CharacterAnimancerState : State {
        [Inject]
        internal AnimancerComponent animancer;

        protected AnimancerState Play(ITransition transition, int layerIndex = 0) {
            return animancer.Layers[layerIndex].Play(transition);
        }
    }

}
