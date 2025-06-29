using System.Text;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace JoG.DebugExtensions {

    public static class DebugExtensions {
        private static readonly StringBuilder _sharedStringBuilder = new();

        public static void Log<T>(this UObject context, in T message) {
            Debug.Log(message?.ToString(), context);
        }

        public static void Log<T1>(this UObject context, in string format, in T1 arg0) {
            Debug.Log(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString())
                , context);
        }

        public static void Log<T1, T2>(this UObject context, in string format, in T1 arg0, in T2 arg1) {
            Debug.Log(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString())
                , context);
        }

        public static void Log<T1, T2, T3>(this UObject context, in string format, in T1 arg0, in T2 arg1, in T3 arg2) {
            Debug.Log(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString(), arg2?.ToString())
                , context);
        }

        public static void LogWarning<T>(this UObject context, in T message) {
            Debug.LogWarning(message?.ToString(), context);
        }

        public static void LogWarning<T1>(this UObject context, string format, in T1 arg0) {
            Debug.LogWarning(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString())
                , context);
        }

        public static void LogWarning<T1, T2>(this UObject context, string format, in T1 arg0, in T2 arg1) {
            Debug.LogWarning(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString())
                , context);
        }

        public static void LogWarning<T1, T2, T3>(this UObject context, string format, in T1 arg0, in T2 arg1, in T3 arg2) {
            Debug.LogWarning(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString(), arg2?.ToString())
                , context);
        }

        public static void LogError<T>(this UObject context, in T message) {
            Debug.LogError(message?.ToString(), context);
        }

        public static void LogError<T1>(this UObject context, string format, in T1 arg0) {
            Debug.LogError(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString())
                , context);
        }

        public static void LogError<T1, T2>(this UObject context, string format, in T1 arg0, in T2 arg1) {
            Debug.LogError(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString())
                , context);
        }

        public static void LogError<T1, T2, T3>(this UObject context, string format, in T1 arg0, in T2 arg1, in T3 arg2) {
            Debug.LogError(
                _sharedStringBuilder.Clear()
                .AppendFormat(format, arg0?.ToString(), arg1?.ToString(), arg2?.ToString())
                , context);
        }

        public static void DrawLine(this in Vector3 start, in Vector3 line, in Color color) {
            Debug.DrawLine(start, start + line, color);
        }
    }
}