using System;
using Unity.Netcode;

namespace JoG.Buffs {

    [Serializable]
    public abstract class TimedBuff<T> : TickableBuff<T> where T : TimedBuff<T>, new() {
        public ushort durationCount;
        public override sealed float TickInterval => 1f;
        public override ushort Count => durationCount;
        public virtual EStackMode StackMode => EStackMode.StackDuration;

        protected override void OnSerialize(FastBufferWriter writer) {
            BytePacker.WriteValueBitPacked(writer, durationCount);
        }

        protected override void OnDeserialize(FastBufferReader reader) {
            ByteUnpacker.ReadValueBitPacked(reader, out durationCount);
        }

        protected override void MergeWith(T buff) {
            switch (StackMode) {
                case EStackMode.StackDuration:
                    durationCount += buff.durationCount;
                    break;

                case EStackMode.LongerDuration:
                    durationCount = durationCount > buff.durationCount ? durationCount : buff.durationCount;
                    break;

                case EStackMode.OverrideDuration:
                    durationCount = buff.durationCount;
                    break;
            }
        }

        protected override void OnTick() {
            if (durationCount is 0) {
                RemoveSelfOnLocal();
                return;
            }
            --durationCount;
        }

        public enum EStackMode : byte {
            StackDuration,
            LongerDuration,
            OverrideDuration,
        }
    }
}