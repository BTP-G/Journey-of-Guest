using System;
using UnityEngine;

namespace Xoderony.PropertyAttributes {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FlagsFieldAttribute : PropertyAttribute {
        public Type EnumType { get; }

        public FlagsFieldAttribute(Type enumType) {
            if (enumType == null) {
                throw new ArgumentNullException(nameof(enumType));
            }
            if (!enumType.IsEnum || !enumType.IsDefined(typeof(FlagsAttribute), false)) {
                throw new ArgumentException("Must be an [Flags] enum.", nameof(enumType));
            }
            EnumType = enumType;
        }
    }
}