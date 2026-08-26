using Cysharp.Threading.Tasks;
using System.Threading;
using YooAsset;

namespace JoG {

    internal interface IResourcePackageLoader {
        UniTask<ResourcePackage> LoadPackageAsync(string packageName, string packageRoot = null, CancellationToken cancellationToken = default);

        UniTask UnloadPackageAsync(ResourcePackage package, CancellationToken cancellationToken = default);
    }
}
