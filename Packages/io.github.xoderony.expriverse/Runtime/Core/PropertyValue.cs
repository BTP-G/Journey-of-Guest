using System;

namespace Expriverse.Core {

    public abstract class PropertyValue {

        public abstract void Reset();
    }

    [Serializable]
    public class PropertyValue<T> : PropertyValue {
        public T value;

        public static implicit operator T(PropertyValue<T> property) {
            return property.value;
        }

        public override void Reset() {
            value = default;
        }
    }
}
