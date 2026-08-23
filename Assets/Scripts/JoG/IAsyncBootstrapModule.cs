using Cysharp.Threading.Tasks;
using System.Threading;

namespace JoG {

    public interface IAsyncBootstrapModule {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
