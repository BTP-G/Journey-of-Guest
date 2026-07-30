using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JoG.Modding {

    [Serializable]
    internal class ModManifest {
        private readonly string id;
        private readonly Version version;
        private readonly string name;
        private readonly string author;
        private readonly string description;
        private readonly ImmutableDictionary<string, Version> dependencies;
        public string Id => id;
        public Version Version => version;
        public string Name => name;
        public string Author => author;
        public string Description => description;
        public IReadOnlyDictionary<string, Version> Dependencies => dependencies;

        [JsonConstructor]
        public ModManifest(string id, string version, string name, string author, string description, Dictionary<string, string> dependencies) {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("Mod ID cannot be null or whitespace.", nameof(id));
            }
            this.id = id.Trim();
            if (!Version.TryParse(version?.Trim(), out this.version)) {
                throw new ArgumentException($"Invalid version format '{version}'.", nameof(version));
            }
            this.name = name?.Trim();
            this.author = author?.Trim();
            this.description = description?.Trim();
            if (dependencies is null || dependencies.Count == 0) {
                this.dependencies = ImmutableDictionary<string, Version>.Empty;
            } else {
                var builder = ImmutableDictionary.CreateBuilder<string, Version>();
                foreach (var kv in dependencies) {
                    if (string.IsNullOrWhiteSpace(kv.Key)) {
                        throw new ArgumentException("Dependency mod ID cannot be null or whitespace.", nameof(dependencies));
                    }
                    if (!Version.TryParse(kv.Value?.Trim(), out var depVersion)) {
                        throw new ArgumentException($"Invalid version format for dependency '{kv.Key}'='{kv.Value}'.", nameof(dependencies));
                    }
                    builder[kv.Key.Trim()] = depVersion;
                }
                this.dependencies = builder.ToImmutable();
            }
        }
    }
}
