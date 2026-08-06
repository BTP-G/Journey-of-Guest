using Unity.Netcode;
using Xoderony.Numerics;

namespace JoG {

    public static class Q16Serializer {

        /// <summary>
        /// 将 Q16 按底层 RawValue 写入网络缓冲区，以完整保留定点数表示。
        /// </summary>
        public static void WriteValueSafe(this FastBufferWriter writer, in Q16 value) {
            var rawValue = value.RawValue;
            writer.WriteValueSafe(rawValue);
        }

        /// <summary>
        /// 从网络缓冲区读取 Q16 的底层 RawValue，并据此还原定点数。
        /// </summary>
        public static void ReadValueSafe(this FastBufferReader reader, out Q16 value) {
            reader.ReadValueSafe(out int rawValue);
            value = new Q16(rawValue);
        }
    }
}
