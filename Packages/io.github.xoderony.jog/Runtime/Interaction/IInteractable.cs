namespace JoG.Interaction {

    public interface IInteractable {

        bool CanInteract(Entity interactor);

        void OnInteracted(Entity interactor);
    }
}
