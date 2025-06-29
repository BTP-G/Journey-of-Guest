using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper {

    /// <summary>
    /// Serializes a UnityEngine.Object with the given interface. Adds a nice
    /// decorator in the inspector as well and a custom object selector.
    /// </summary>
    /// <typeparam name="TInterface">The interface.</typeparam>
    /// <typeparam name="TObject">The UnityEngine.Object.</typeparam>
    [System.Serializable]
    public struct InterfaceReference<TInterface, TObject> where TInterface : class where TObject : Object {

        [SerializeField, HideInInspector]
        private TObject _underlyingValue;

        public InterfaceReference(TObject target) => _underlyingValue = target;

        public InterfaceReference(TInterface @interface) => _underlyingValue = @interface as TObject;

        /// <summary>
        /// Get the interface, if the UnderlyingValue is not null and implements
        /// the given interface.
        /// </summary>
        public TInterface Value {
            get => _underlyingValue as TInterface;
            set {
                if (value == null) {
                    _underlyingValue = null;
                } else {
                    var newValue = value as TObject;
                    Debug.Assert(newValue != null, $"{value} needs to be of type {typeof(TObject)}.");
                    _underlyingValue = newValue;
                }
            }
        }

        /// <summary>Get the actual UnityEngine.Object that gets serialized.</summary>
        public TObject UnderlyingValue {
            get => _underlyingValue;
            set => _underlyingValue = value;
        }

        public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.Value;

        public static implicit operator InterfaceReference<TInterface, TObject>(TInterface obj) => new(obj);
    }

    /// <summary>
    /// Serializes a UnityEngine.Object with the given interface. Adds a nice
    /// decorator in the inspector as well and a custom object selector.
    /// </summary>
    /// <typeparam name="TInterface">The interface.</typeparam>
    [System.Serializable]
    public struct InterfaceReference<TInterface> where TInterface : class {

        [SerializeField, HideInInspector]
        private Object _underlyingValue;

        public InterfaceReference(Object target) => _underlyingValue = target;

        public InterfaceReference(TInterface @interface) => _underlyingValue = @interface as Object;

        /// <summary>
        /// Get the interface, if the UnderlyingValue is not null and implements
        /// the given interface.
        /// </summary>
        public TInterface Value {
            get => _underlyingValue as TInterface;
            set {
                if (value == null) {
                    _underlyingValue = null;
                } else {
                    var newValue = value as Object;
                    Debug.Assert(newValue != null, $"{value} needs to be of type {typeof(Object)}.");
                    _underlyingValue = newValue;
                }
            }
        }

        /// <summary>Get the actual UnityEngine.Object that gets serialized.</summary>
        public Object UnderlyingValue {
            get => _underlyingValue;
            set => _underlyingValue = value;
        }
    }
}