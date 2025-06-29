using JoG.BuffSystem;
using JoG.DebugExtensions;
using System;
using System.Buffers;
using Unity.Netcode;

namespace JoG.Character {

    public partial class CharacterBody {
        private IBuff[] _buffs;
        private ushort _activeBuffCount;

        public ReadOnlySpan<IBuff> Buffs => _buffs;
        public ushort ActiveBuffCount => _activeBuffCount;

        public void AddBuffOnLocal(IBuff buff) {
            if (buff.Index < _buffs.Length) {
                var buff0 = _buffs[buff.Index];
                if (buff0 is not null) {
                    buff0.MergeWith(buff);
                    BuffPool.Return(buff);
                } else {
                    _buffs[buff.Index] = buff;
                    ++_activeBuffCount;
                    buff.AddToOwner(this);
                }
            } else {
                this.LogWarning("Add buff failed! [Buff: {0}] index is out of range!", buff);
            }
        }

        public void RemoveBuffOnLocal(ushort index) {
            if (index < _buffs.Length) {
                var buff0 = _buffs[index];
                if (buff0 is not null) {
                    buff0.RemoveFromOwner();
                    _buffs[index] = null;
                    --_activeBuffCount;
                    BuffPool.Return(buff0);
                }
            } else {
                this.LogWarning("Remove buff failed! Buff [index: {0}] is out of range!", index);
            }
        }

        public void RemoveBuffOnLocal<T>() where T : BuffBase<T>, new() => RemoveBuffOnLocal(BuffBase<T>.index);

        public void AddBuffOnEveryone(IBuff buff) => AddBuffRpc(buff);

        public void RemoveBuffOnEveryone<T>() where T : BuffBase<T>, new() => RemoveBuffRpc(BuffBase<T>.index);

        public bool TryGetbuff<T>(out T buff) where T : BuffBase<T>, new() {
            if (BuffBase<T>.index < _buffs.Length) {
                var buff0 = _buffs[BuffBase<T>.index];
                if (buff0 is T) {
                    buff = buff0 as T;
                    return true;
                }
            }
            buff = null;
            return false;
        }

        [Rpc(SendTo.Everyone)]
        private void AddBuffRpc(IBuff buff) => AddBuffOnLocal(buff);

        [Rpc(SendTo.Everyone)]
        private void RemoveBuffRpc(ushort buffIndex) => RemoveBuffOnLocal(buffIndex);

        private void InitializeBuff() {
            _buffs = ArrayPool<IBuff>.Shared.Rent(BuffPool.Count);
            _activeBuffCount = 0;
        }

        private void DeinitializeBuff() {
            foreach (var buff in new ReadOnlySpan<IBuff>(_buffs)) {
                if (buff is null) continue;
                buff.RemoveFromOwner();
                BuffPool.Return(buff);
            }
            ArrayPool<IBuff>.Shared.Return(_buffs, true);
            _activeBuffCount = 0;
        }
    }
}