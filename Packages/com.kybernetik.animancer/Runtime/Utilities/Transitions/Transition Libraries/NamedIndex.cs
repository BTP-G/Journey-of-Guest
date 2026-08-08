// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2026 Kybernetik //

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animancer.TransitionLibraries {
    /// <summary>[<see cref="SerializableAttribute"/>] A <see cref="StringAsset"/> and <see cref="int"/> pair.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions/libraries">
    /// Transition Libraries</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.TransitionLibraries/NamedIndex
    [Serializable]
    public struct NamedIndex :
        IComparable<NamedIndex>,
        IEquatable<NamedIndex> {
        /************************************************************************************************************************/

        [SerializeField]
        private StringAsset _Name;

        /// <summary>The name.</summary>
        public readonly StringAsset Name
            => _Name;

        /************************************************************************************************************************/

        [SerializeField]
        private int _Index;

        /// <summary>The index.</summary>
        public readonly int Index
            => _Index;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="NamedIndex"/>.</summary>
        public NamedIndex(StringAsset name, int index) {
            _Name = name;
            _Index = index;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="NamedIndex"/>.</summary>
        public readonly NamedIndex With(StringAsset name) {
            return new(name, _Index);
        }

        /// <summary>Creates a new <see cref="NamedIndex"/>.</summary>
        public readonly NamedIndex With(int index) {
            return new(_Name, index);
        }

        /************************************************************************************************************************/

        /// <summary>Describes this value.</summary>
        public override readonly string ToString() {
            return $"[{_Index}]{_Name}";
        }

        /************************************************************************************************************************/
        #region Equality
        /************************************************************************************************************************/

        /// <summary>Compares the <see cref="Index"/> then <see cref="Name"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(NamedIndex other) {
            var result = _Index.CompareTo(other._Index);
            if (result != 0) {
                return result;
            } else {
                return StringAsset.Compare(_Name, other._Name);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Are all fields in this object equal to the equivalent in `obj`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj) {
            return obj is NamedIndex value
                                                                     && Equals(value);
        }

        /// <summary>Are all fields in this object equal to the equivalent fields in `other`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(NamedIndex other) {
            return _Index == other._Index
                                                                  && _Name == other._Name;
        }

        /// <summary>Are all fields in `a` equal to the equivalent fields in `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NamedIndex a, NamedIndex b) {
            return a.Equals(b);
        }

        /// <summary>Are any fields in `a` not equal to the equivalent fields in `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NamedIndex a, NamedIndex b) {
            return !(a == b);
        }

        /************************************************************************************************************************/

        /// <summary>Returns a hash code based on the values of this object's fields.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() {
            return AnimancerUtilities.Hash(-871379578,
                                                                   _Index.SafeGetHashCode(),
                                                                   _Name.SafeGetHashCode());
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

