using System;
using UnityEngine;
using YooAsset;
using UObject = UnityEngine.Object;

namespace Xoderony.YooAsset {

    [Serializable]
    public class YooAssetReference<T> : IDisposable where T : UObject {

#if UNITY_EDITOR

        [SerializeField]
        private T _assetCache;

#endif

        [SerializeField]
        private string _location;

        [SerializeField]
        private string _packageName;

        private AssetHandle _assetHandleCache;

        private YooAssetReference() { }

        public YooAssetReference(string packageName, string location) {
            _packageName = packageName ?? throw new ArgumentNullException(nameof(packageName));
            _location = location ?? throw new ArgumentNullException(nameof(location));
        }

        public string Location => _location;

        public string PackageName => _packageName;

        public AssetHandle AssetHandle => _assetHandleCache;

        public T AssetObject => _assetHandleCache.GetAssetObject<T>();

        public void Load(bool async = false) {
            if (_assetHandleCache is not null) {
                Debug.LogError($"[YooAssetReference] 资源已经加载！");
                return;
            }
            if (!YooAssets.TryGetPackage(_packageName, out var package)) {
                Debug.LogError($"[YooAssetReference] 资源包不存在：{_packageName}");
                return;
            }
            if (string.IsNullOrEmpty(_location)) {
                Debug.LogError($"[YooAssetReference] 地址为空！");
                return;
            }
            if (async) {
                _assetHandleCache = package.LoadAssetAsync<T>(_location);
            } else {
                _assetHandleCache = package.LoadAssetSync<T>(_location);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Unload() {
            _assetHandleCache?.Dispose();
            _assetHandleCache = null;
        }

        void IDisposable.Dispose() {
            _assetHandleCache?.Dispose();
            _assetHandleCache = null;
        }
    }
}
