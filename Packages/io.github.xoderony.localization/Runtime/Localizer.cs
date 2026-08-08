using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.Localization {

    public static class Localizer {
        private static Dictionary<string, string> _keyToTranslation = new();
        private static string _languageCode;
        private static Action _languageUpdatedHandlers;
        private static LanguageBuilder _languageBuilders;

        public static string LanguageCode {
            get => _languageCode;
            set {
                if (_languageCode == value) {
                    return;
                }

                _languageCode = value;
                Build();
                OnLanguageChanged?.Invoke();
            }
        }

        public static event Action OnLanguageChanged;

        /// <summary>添加/移除语言构建者并且重构语言表</summary>
        public static event LanguageBuilder LanguageBuilders {
            add {
                _languageBuilders += value;
                Build();
            }
            remove {
                _languageBuilders -= value;
                Build();
            }
        }

        /// <summary>添加时触发一次调用</summary>
        public static event Action OnLanguageUpdated {
            add {
                _languageUpdatedHandlers += value;
                value.Invoke();
            }
            remove {
                _languageUpdatedHandlers -= value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string key) {
            return _keyToTranslation.TryGetValue(key, out var value)
                ? value
                : key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<T>(string key, T arg0) {
            return ZString.Format(GetString(key), arg0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<T0, T1>(string key, T0 arg0, T1 arg1) {
            return ZString.Format(GetString(key), arg0, arg1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<T0, T1, T2>(string key, T0 arg0, T1 arg1, T2 arg2) {
            return ZString.Format(GetString(key), arg0, arg1, arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<T0, T1, T2, T3>(string key, T0 arg0, T1 arg1, T2 arg2, T3 arg3) {
            return ZString.Format(GetString(key), arg0, arg1, arg2, arg3);
        }

        private static void Build() {
            if (string.IsNullOrWhiteSpace(_languageCode)) {
                return;
            }

            var builder = new Dictionary<string, string>();
            _languageBuilders?.Invoke(_languageCode, builder);
            _keyToTranslation = builder;
            _languageUpdatedHandlers?.Invoke();
        }

        public delegate void LanguageBuilder(string languageCode, IDictionary<string, string> builder);
    }
}
