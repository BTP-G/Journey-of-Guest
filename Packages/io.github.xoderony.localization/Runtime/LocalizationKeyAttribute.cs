using System;
using UnityEngine;

namespace Xoderony.Localization {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LocalizationKeyAttribute : PropertyAttribute {

        /// <summary>用于过滤本地化 Key 的正则表达式。为空时显示本地化文件中的全部 Key。</summary>
        public string RegexPattern { get; }

        public LocalizationKeyAttribute() {
        }

        public LocalizationKeyAttribute(string regexPattern) {
            RegexPattern = regexPattern;
        }
    }
}