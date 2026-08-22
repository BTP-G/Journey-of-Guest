using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>项目网络对象上的同步状态项。</summary>
    public abstract class NetworkVariableBase {
        private JoGNetworkObject _networkObject;

        public bool IsDirty { get; private set; }

        internal void Bind(JoGNetworkObject networkObject) {
            _networkObject = networkObject;
        }

        internal void Serialize(ref BufferWriter writer) {
            OnSerialize(ref writer);
            MarkClean();
        }

        internal void Deserialize(ref BufferReader reader) {
            OnDeserialize(ref reader);
            MarkClean();
        }

        protected void MarkDirty() {
            IsDirty = true;
            _networkObject.MarkNetworkVariableDirty();
        }

        private void MarkClean() {
            IsDirty = false;
        }

        protected abstract void OnSerialize(ref BufferWriter writer);

        protected abstract void OnDeserialize(ref BufferReader reader);
    }
}
