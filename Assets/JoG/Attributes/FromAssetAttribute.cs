using System;

namespace JoG.Attributes {

    /// <summary>用于自动注入 YooAsset 资源，仅适用于静态字段。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class FromAssetAttribute : Attribute {

        /// <summary>资源在包内的路径</summary>
        public string AssetLocation { get; }

        /// <summary>资源包名，默认为 "DefaultPackage"</summary>
        public string PackageName { get; set; } = "DefaultPackage";

        public FromAssetAttribute(string assetLocation) {
            if (string.IsNullOrWhiteSpace(assetLocation))
                throw new ArgumentException("AssetLocation cannot be null or whitespace.", nameof(assetLocation));
            AssetLocation = assetLocation;
        }
    }
}