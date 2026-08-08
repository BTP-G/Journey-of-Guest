using System.Collections.Generic;

namespace JoG.Core {

    public class PropertyHub {

        private readonly Dictionary<string, PropertyValue> _nameToProperty = new();

        public PropertyValue<T> GetProperty<T>(string name) {
            if (_nameToProperty.TryGetValue(name, out var propertyValue)) {
                return (PropertyValue<T>)propertyValue;
            } else {
                var property = new PropertyValue<T>();
                _nameToProperty[name] = property;
                return property;
            }
        }

        public PropertyHub SetProperty<T>(string name, T value) {
            if (_nameToProperty.TryGetValue(name, out var objectValue)) {
                ((PropertyValue<T>)objectValue).value = value;
            } else {
                _nameToProperty[name] = new PropertyValue<T> {
                    value = value
                };
            }
            return this;
        }

        public void Reset() {
            foreach (var property in _nameToProperty.Values) {
                property.Reset();
            }
        }
    }
}
