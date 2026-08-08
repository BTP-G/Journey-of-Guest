using Cysharp.Threading.Tasks;
using System;

namespace JoG.Modding {

    public interface IModManager {
        ReadOnlySpan<Mod> ModSpan { get; }
        int ModCount { get; }

        UniTask DisableModAsync(string modId);

        UniTask EnableModAsync(string modId);

        bool IsModLoaded(string modId);

        bool TryGetMod(string modId, out Mod mod);
    }
}
