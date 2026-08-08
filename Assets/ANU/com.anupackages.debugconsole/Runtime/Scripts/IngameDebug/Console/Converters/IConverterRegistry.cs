using System;

namespace ANU.IngameDebug.Console.Converters {
    public interface IConverter {
        int Priority => 0;
        Type TargetType { get; }
        bool CanConvert<TFrom>() {
            return CanConvert(typeof(TFrom));
        }

        bool CanConvert(Type type) {
            return TargetType.IsAssignableFrom(type);
        }

        string ConvertToString(object obj, Type targetType) {
            return obj?.ToString();
        }

        object ConvertFromString(string option, Type targetType);
    }

    public interface IConverter<T> : IConverter {
        Type IConverter.TargetType => typeof(T);
        object IConverter.ConvertFromString(string option, Type targetType) {
            return ConvertFromString(option);
        }

        string ConvertToString(T value) {
            return ConvertToString(value as object, typeof(T));
        }

        T ConvertFromString(string option);
    }

    public interface IReadOnlyConverterRegistry {
        string ConvertToString<T>(T value);
        string ConvertToString(Type type, object value);

        T ConvertFromString<T>(string option);
        object ConvertFromString(Type type, string option);
    }

    public interface IConverterRegistry : IReadOnlyConverterRegistry {
        void Register<T>(Func<string, T> converter);
        void Register<T>(IConverter<T> converter);
        void Register(IConverter converter);
    }
}