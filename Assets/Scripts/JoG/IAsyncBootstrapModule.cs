using Cysharp.Threading.Tasks;

namespace JoG {

    public interface IAsyncBootstrapModule {
        UniTask InitializeAsync();
    }
}
