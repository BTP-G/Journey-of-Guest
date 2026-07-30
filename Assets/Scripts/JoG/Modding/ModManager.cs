using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Xoderony.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace JoG.Modding {

    internal class ModManager : IModManager, IAsyncBootstrapModule {
        private readonly Dictionary<string, Mod> _idToMod = new();

        private Mod[] _mods = Array.Empty<Mod>();
        public int ModCount => _idToMod.Count;
        public ReadOnlySpan<Mod> ModSpan => _mods;

        async UniTask IAsyncBootstrapModule.InitializeAsync() {
            var modsDirectory = Path.Combine(Application.dataPath, "Mods");
            var modCandidates = await LoadModCandidates(modsDirectory);
            if (modCandidates.Count == 0) {
                this.Log("No valid mods found to load.");
                return;
            }
            var uniqueCandidates = DeduplicateMods(modCandidates, out var duplicates);
            if (duplicates.Count > 0) {
                using var sb = ZString.CreateStringBuilder(true);
                sb.AppendLine("Duplicate mod IDs found. Only one instance of each will be loaded:");
                sb.AppendJoin('\n', duplicates);
                this.LogWarning(sb.ToString());
            }
            var sortedList = TopologicalSort(uniqueCandidates, out var failedCollection);
            if (failedCollection.Count > 0) {
                using var sb = ZString.CreateStringBuilder(true);
                sb.AppendLine("Some mods could not be loaded due to missing dependencies or circular dependencies:");
                sb.AppendJoin('\n', failedCollection);
                this.LogWarning(sb.ToString());
            }
            if (sortedList.Count == 0) {
                this.LogWarning("No valid mods to load.");
                return;
            }
            foreach (var candidate in sortedList) {
                try {
                    foreach (var depId in candidate.manifest.Dependencies.Keys) {
                        if (!_idToMod.ContainsKey(depId)) {
                            throw new Exception(ZString.Concat("Dependency '", depId, "' is not loaded."));
                        }
                    }
                    var mainAssembly = LoadModMainAssembly(candidate);
                    var entryType = FindModEntryType(mainAssembly);
                    var modInstance = CreateModInstance(entryType, candidate);
                    _idToMod.Add(modInstance.Id, modInstance);
                } catch (Exception ex) {
                    this.LogError(ZString.Concat("Failed to load mod ", candidate.manifest.Id, " from ", candidate.directory, ": ", ex));
                }
            }
            _mods = _idToMod.Values.ToArray();
            this.Log(ZString.Concat("Successfully loaded ", _idToMod.Count, " mod(s)."));
            foreach (var mod in _mods) {
                if (await ReadModEnabled(mod.RootDirectory)) {
                    await EnableModAsync(mod.Id);
                }
            }
        }

        public bool IsModLoaded(string modId) {
            return _idToMod.ContainsKey(modId);
        }

        public bool TryGetMod(string modId, out Mod mod) {
            return _idToMod.TryGetValue(modId, out mod);
        }

        public async UniTask EnableModAsync(string modId) {
            if (_idToMod.TryGetValue(modId, out var mod) && !mod.Enabled) {
                foreach (var id in mod.Dependencies.Keys) {
                    await EnableModAsync(id);
                }
                await mod.EnableAsync();
                await WriteModEnabled(mod.RootDirectory, true);
            }
        }

        public async UniTask DisableModAsync(string modId) {
            if (_idToMod.TryGetValue(modId, out var mod) && mod.Enabled) {
                await mod.DisableAsync();
                await WriteModEnabled(mod.RootDirectory, false);
                foreach (var mod1 in _mods) {
                    if (mod1.Enabled && mod1.Dependencies.ContainsKey(modId)) {
                        await DisableModAsync(mod1.Id);
                    }
                }
            }
        }

        private async UniTask<IReadOnlyList<ModCandidate>> LoadModCandidates(string modsDirectory) {
            var candidates = new List<ModCandidate>();
            if (!Directory.Exists(modsDirectory)) {
                this.LogWarning($"Mods directory not found at {modsDirectory}. No mods will be loaded.");
                return candidates;
            }
            foreach (var modDirectory in Directory.GetDirectories(modsDirectory)) {
                try {
                    var manifestFilePath = Path.Combine(modDirectory, "mod.json");
                    if (!File.Exists(manifestFilePath)) {
                        throw new FileNotFoundException($"No mod.json found.");
                    }
                    var jsonText = await File.ReadAllTextAsync(manifestFilePath);
                    var manifest = JsonConvert.DeserializeObject<ModManifest>(jsonText) ?? throw new InvalidDataException("Mod manifest is null. Check modinfo.json content.");
                    candidates.Add(new ModCandidate(modDirectory, manifest));
                } catch (Exception ex) {
                    this.LogWarning($"Failed to load mod manifest in {modDirectory}: {ex}");
                }
            }
            return candidates;
        }

        private async UniTask<bool> ReadModEnabled(string modDirectory) {
            var filePath = Path.Combine(modDirectory, "enabled.txt");
            if (!File.Exists(filePath)) {
                await WriteModEnabled(modDirectory, true);
                return true;
            }
            try {
                var content = await File.ReadAllTextAsync(filePath);
                return string.Equals(content?.Trim(), "y", StringComparison.OrdinalIgnoreCase);
            } catch (Exception ex) {
                this.LogError($"Failed to read enabled status from {filePath}: {ex}");
            }
            return false;
        }

        private async UniTask WriteModEnabled(string modDirectory, bool enabled) {
            var filePath = Path.Combine(modDirectory, "enabled.txt");
            var content = enabled ? "y" : "n";
            try {
                await File.WriteAllTextAsync(filePath, content);
            } catch (Exception ex) {
                this.LogError($"Failed to write enabled status ({content}) to {filePath}: {ex}");
            }
        }

        private Assembly LoadModMainAssembly(ModCandidate candidate) {
            var assemblyFilePath = Path.Combine(candidate.directory, "Assemblies", $"{candidate.manifest.Id}.dll");
            if (!File.Exists(assemblyFilePath)) {
                return null;
            }
            return Assembly.LoadFrom(assemblyFilePath);
        }

        private Type FindModEntryType(Assembly mainAssembly) {
            if (mainAssembly == null) return null;
            foreach (var type in mainAssembly.GetTypes()) {
                if (type == null || type.IsAbstract || type.IsGenericType) {
                    continue;
                }
                if (type.IsSubclassOf(typeof(Mod))) {
                    return type;
                }
            }
            return null;
        }

        private Mod CreateModInstance(Type entryType, ModCandidate candidate) {
            Mod mod;
            if (entryType == null) {
                mod = new StandardMod();
            } else {
                mod = Activator.CreateInstance(entryType) as Mod;
            }
            mod.Construct(
                candidate.directory,
                candidate.manifest.Id,
                candidate.manifest.Name,
                candidate.manifest.Author,
                candidate.manifest.Version,
                candidate.manifest.Description,
                candidate.manifest.Dependencies);
            return mod;
        }

        private IReadOnlyList<ModCandidate> DeduplicateMods(IEnumerable<ModCandidate> candidates, out IReadOnlyCollection<ModCandidate> duplicates) {
            var seenIds = new HashSet<string>();
            var uniqueList = new List<ModCandidate>();
            var duplicateList = new List<ModCandidate>();
            foreach (var candidate in candidates) {
                if (seenIds.Add(candidate.manifest.Id)) {
                    uniqueList.Add(candidate);
                } else {
                    duplicateList.Add(candidate);
                }
            }
            duplicates = duplicateList;
            return uniqueList;
        }

        private IReadOnlyList<ModCandidate> TopologicalSort(IEnumerable<ModCandidate> collection, out IReadOnlyCollection<ModCandidate> failedCollection) {
            var sorted = new List<ModCandidate>();
            var unsorted = new List<ModCandidate>(collection);
            var sortedIdToVersion = new Dictionary<string, Version>();
            int sortCountThisRound;
            do {
                sortCountThisRound = 0;
                for (var i = unsorted.Count - 1; i >= 0; i--) {
                    var candidate = unsorted[i];
                    if (IsDependenciesSatisfied(candidate.manifest, sortedIdToVersion)) {
                        sorted.Add(candidate);
                        sortedIdToVersion[candidate.manifest.Id] = candidate.manifest.Version;
                        unsorted.RemoveAt(i);
                        sortCountThisRound++;
                    }
                }
            } while (sortCountThisRound > 0);
            failedCollection = unsorted;
            return sorted;
            /// <summary>检查该 Mod 的依赖是否被已排序的 Mod 集合满足。</summary>
            static bool IsDependenciesSatisfied(ModManifest manifest, IReadOnlyDictionary<string, Version> idToVersion) {
                foreach (var dep in manifest.Dependencies) {
                    if (!idToVersion.TryGetValue(dep.Key, out var actualVersion)) {
                        return false; // 依赖未找到
                    }
                    if (actualVersion < dep.Value) {
                        return false; // 版本不兼容
                    }
                }
                return true;
            }
        }

        private readonly struct ModCandidate {
            public readonly string directory;
            public readonly ModManifest manifest;

            public ModCandidate(string directory, ModManifest manifest) {
                this.directory = directory;
                this.manifest = manifest;
            }

            public override string ToString() {
                return $"ModCandidate(Id: {manifest.Id}, directory: {directory})";
            }
        }
    }
}
