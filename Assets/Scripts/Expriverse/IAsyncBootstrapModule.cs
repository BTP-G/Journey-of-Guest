using Cysharp.Threading.Tasks;
using System.Threading;

namespace Expriverse {

    public interface IAsyncBootstrapModule {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
