using System;


namespace JoG.Core {

    public abstract class PropertyValue {

        public abstract void Reset();
    }

    [Serializable]
    public class PropertyValue<T> : PropertyValue {
        public T value;

        public static implicit operator T(PropertyValue<T> property) => property.value;

        public override void Reset() {
            value = default;
        }
    }
}
