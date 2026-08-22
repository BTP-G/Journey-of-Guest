using System.Runtime.CompilerServices;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>项目网络对象上的同步状态项。</summary>
    public abstract class NetworkVariableBase {
        private JoGNetworkObject _networkObject;
        private byte _index;
        private bool _isDirty;

        internal byte Index => _index;

        public bool IsDirty => _isDirty;

        internal void Bind(JoGNetworkObject networkObject, byte index) {
            _networkObject = networkObject;
            _index = index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Serialize(ref BufferWriter writer) {
            OnSerialize(ref writer);
            MarkClean();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Deserialize(ref BufferReader reader) {
            OnDeserialize(ref reader);
            MarkClean();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MarkDirty() {
            _isDirty = true;
            _networkObject.MarkNetworkVariableDirty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MarkClean() {
            _isDirty = false;
        }

        protected abstract void OnSerialize(ref BufferWriter writer);

        protected abstract void OnDeserialize(ref BufferReader reader);
    }
}
