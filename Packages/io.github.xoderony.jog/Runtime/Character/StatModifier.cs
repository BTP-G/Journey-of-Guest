using System.Runtime.CompilerServices;
using Xoderony.Numerics;

namespace JoG.Character {

    // 单个倍率修正的实例句柄：封装设置与移除，调用方不接触 Stat 的槽位 API；
    // Remove 幂等，重复调用无操作。
    public sealed class StatModifier {

        private readonly Stat _stat;

        private int _slotIndex;

        private bool _removed;

        internal StatModifier(Stat stat, int slotIndex) {
            _stat = stat;
            _slotIndex = slotIndex;
        }

        // 密集槽位在移除后发生移动，Stat 通过该索引回写保持句柄有效。
        internal int SlotIndex {
            get => _slotIndex;
            set => _slotIndex = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(Q16 multiplier) {
            _stat.SetMultiplier(_slotIndex, multiplier);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove() {
            if (_removed) {
                return;
            }
            _removed = true;
            _stat.RemoveMultiplierSlot(_slotIndex);
        }
    }
}
