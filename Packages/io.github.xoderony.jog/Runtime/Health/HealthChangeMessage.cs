using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Health {

    [Serializable]
    public struct HealthChangeMessage : INetworkSerializable {

        public const int MaxValueInt = 999_999_999;

        public const long MaxValueLong = 999_999_999L;

        public const int MinValueInt = -999_999_999;

        public const long MinValueLong = -999_999_999L;

        public static readonly int PreCheckedSize = Unsafe.SizeOf<HealthChangeMessage>();

        public int Value;

        public HealthChangeFlag Flags;

        public Color32 Color;

        public Vector3 Position;

        /// <summary>Scales Value by a percentage factor (e.g 150 = 150% = ×1.5).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleByPercent(int percent) {
            Value = (int)Math.Clamp((long)Value * percent / 100L, MinValueLong, MaxValueLong);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasFlag(HealthChangeFlag flag) {
            return (Flags & flag) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasFlags(HealthChangeFlag flags) {
            return (this.Flags & flags) == flags;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.PreCheck(PreCheckedSize)) {
                serializer.SerializeValuePreChecked(ref Value);
                serializer.SerializeValuePreChecked(ref Flags);
                serializer.SerializeValuePreChecked(ref Color);
                serializer.SerializeValuePreChecked(ref Position);
            }
        }
    }
}
