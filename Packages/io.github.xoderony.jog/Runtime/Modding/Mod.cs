using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace JoG.Modding {

    public abstract class Mod {
        private string _id;
        private string _name;
        private string _author;
        private Version _version;
        private string _description;
        private IReadOnlyDictionary<string, Version> _dependencies;
        private string _rootDirectory;
        public string RootDirectory => _rootDirectory;
        public string Id => _id;
        public string Name => _name;
        public string Author => _author;
        public Version Version => _version;
        public string Description => _description;
        public IReadOnlyDictionary<string, Version> Dependencies => _dependencies;
        public bool Enabled { get; private set; }

        internal void Construct(
            string directory,
            string id,
            string name,
            string author,
            Version version,
            string description,
            IReadOnlyDictionary<string, Version> dependencies) {
            _rootDirectory = directory;
            _id = id;
            _name = name;
            _author = author;
            _version = version;
            _description = description;
            _dependencies = dependencies;
        }

        internal async UniTask EnableAsync() {
            await OnEnableAsync();
            Enabled = true;
        }

        internal async UniTask DisableAsync() {
            await OnDisableAsync();
            Enabled = false;
        }

        protected abstract UniTask OnEnableAsync();

        protected abstract UniTask OnDisableAsync();
    }
}
