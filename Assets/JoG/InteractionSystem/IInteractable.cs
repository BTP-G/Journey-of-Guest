namespace JoG.InteractionSystem {

    public interface IInteractable {

        Interactability GetInteractability(Interactor interactor);

        void PreformInteraction(Interactor interactor);
    }
}