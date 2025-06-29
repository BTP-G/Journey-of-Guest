namespace JoG.InteractionSystem {

    public interface IInteractionMessageHandler {

        void Handle(IInteractable interactableObject);
    }
}