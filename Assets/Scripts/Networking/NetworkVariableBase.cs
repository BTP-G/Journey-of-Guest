using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>项目网络对象上的同步状态项。</summary>
    public abstract class NetworkVariableBase {
        public bool IsDirty { get; protected set; }

        internal void Serialize(ref BufferWriter writer) {
            OnSerialize(ref writer);
            IsDirty = false;
        }

        internal void Deserialize(ref BufferReader reader) {
            OnDeserialize(ref reader);
            IsDirty = false;
        }

        protected abstract void OnSerialize(ref BufferWriter writer);

        protected abstract void OnDeserialize(ref BufferReader reader);
    }
}
