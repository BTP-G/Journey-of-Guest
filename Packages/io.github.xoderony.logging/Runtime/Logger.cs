using Cysharp.Text;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xoderony.Logging {

    public static class Logger {
        public static readonly ILogger UnityLogger = Debug.unityLogger;

        public static readonly ILogHandler UnityLogHandler = Debug.unityLogger.logHandler;

        /// <summary>UnityLogger.logEnabled</summary>
        public static bool LogEnabled {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityLogger.logEnabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnityLogger.logEnabled = value;
        }

        /// <summary>UnityLogger.filterLogType</summary>
        public static LogType FilterLogType {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnityLogger.filterLogType;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => UnityLogger.filterLogType = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string message, LogType logType = LogType.Log, Object context = null) {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            UnityLogHandler.LogFormat(logType, context, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log<TContext>(this TContext context, string message) where TContext : class {
            InternalLog(message, LogType.Log, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning<TContext>(this TContext context, string message) where TContext : class {
            InternalLog(message, LogType.Warning, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogAssertion<TContext>(this TContext context, string message) where TContext : class {
            InternalLog(message, LogType.Assert, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError<TContext>(this TContext context, string message) where TContext : class {
            InternalLog(message, LogType.Error, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException<TContext>(this TContext context, System.Exception exception) where TContext : class {
            if (UnityLogger.logEnabled) {
                UnityLogHandler.LogException(exception, context as Object);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat<TContext, T0>(this TContext context, string format, T0 arg0) where TContext : class {
            InternalLogFormat(LogType.Log, context, format, arg0);
        }

        /// <summary>格式化日志（2 个参数）</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat<TContext, T0, T1>(this TContext context, string format, T0 arg0, T1 arg1) where TContext : class {
            InternalLogFormat(LogType.Log, context, format, arg0, arg1);
        }

        /// <summary>格式化日志（3 个参数）</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat<TContext, T0, T1, T2>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2) where TContext : class {
            InternalLogFormat(LogType.Log, context, format, arg0, arg1, arg2);
        }

        /// <summary>格式化日志（4 个参数）</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat<TContext, T0, T1, T2, T3>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where TContext : class {
            InternalLogFormat(LogType.Log, context, format, arg0, arg1, arg2, arg3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarningFormat<TContext, T0>(this TContext context, string format, T0 arg0) where TContext : class {
            InternalLogFormat(LogType.Warning, context, format, arg0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarningFormat<TContext, T0, T1>(this TContext context, string format, T0 arg0, T1 arg1) where TContext : class {
            InternalLogFormat(LogType.Warning, context, format, arg0, arg1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarningFormat<TContext, T0, T1, T2>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2) where TContext : class {
            InternalLogFormat(LogType.Warning, context, format, arg0, arg1, arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarningFormat<TContext, T0, T1, T2, T3>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where TContext : class {
            InternalLogFormat(LogType.Warning, context, format, arg0, arg1, arg2, arg3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogErrorFormat<TContext, T0>(this TContext context, string format, T0 arg0) where TContext : class {
            InternalLogFormat(LogType.Error, context, format, arg0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogErrorFormat<TContext, T0, T1>(this TContext context, string format, T0 arg0, T1 arg1) where TContext : class {
            InternalLogFormat(LogType.Error, context, format, arg0, arg1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogErrorFormat<TContext, T0, T1, T2>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2) where TContext : class {
            InternalLogFormat(LogType.Error, context, format, arg0, arg1, arg2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogErrorFormat<TContext, T0, T1, T2, T3>(this TContext context, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where TContext : class {
            InternalLogFormat(LogType.Error, context, format, arg0, arg1, arg2, arg3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void InternalLog<TContext>(string message, LogType logType, TContext context = null) where TContext : class {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            UnityLogHandler.LogFormat(logType, context as Object, TagCache<TContext>.Tag + message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalLogFormat<TContext, T0>(LogType logType, TContext context, string format, T0 arg0) where TContext : class {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append(TagCache<TContext>.Tag);
            sb.AppendFormat(format, arg0);
            UnityLogHandler.LogFormat(logType, context as Object, sb.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalLogFormat<TContext, T0, T1>(LogType logType, TContext context, string format, T0 arg0, T1 arg1) where TContext : class {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append(TagCache<TContext>.Tag);
            sb.AppendFormat(format, arg0, arg1);
            UnityLogHandler.LogFormat(logType, context as Object, sb.ToString() );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalLogFormat<TContext, T0, T1, T2>(LogType logType, TContext context, string format, T0 arg0, T1 arg1, T2 arg2) where TContext : class {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append(TagCache<TContext>.Tag);
            sb.AppendFormat(format, arg0, arg1, arg2);
            UnityLogHandler.LogFormat(logType, context as Object, sb.ToString() );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalLogFormat<TContext, T0, T1, T2, T3>(LogType logType, TContext context, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3) where TContext : class {
            if (!UnityLogger.IsLogTypeAllowed(logType)) return;
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append(TagCache<TContext>.Tag);
            sb.AppendFormat(format, arg0, arg1, arg2, arg3);
            UnityLogHandler.LogFormat(logType, context as Object, sb.ToString());
        }

        private static class TagCache<T> {
            public static readonly string Tag = $"[{typeof(T).Name}] ";
        }
    }
}