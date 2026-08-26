using Expriverse.UI.Popup;
using VContainer;
using VContainer.Unity;

namespace Expriverse.LifetimeScopes {

    public class MainMenuScope : LifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            builder.RegisterComponentInHierarchy<LoaderPopup>().AsSelf();
            builder.RegisterComponentInHierarchy<ConfirmPopup>().AsSelf();
            builder.RegisterComponentInHierarchy<ToastPopupController>().AsSelf();
        }
    }
}
