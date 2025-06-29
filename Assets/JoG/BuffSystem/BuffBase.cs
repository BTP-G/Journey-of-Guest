using JoG.Character;
using Unity.Netcode;
using UnityEngine;

namespace JoG.BuffSystem {

    /// <summary>Custom buff must inherit from this type.</summary>
    public abstract class BuffBase<T> : IBuff where T : BuffBase<T>, new() {
        internal static ushort index = ushort.MaxValue;
        private CharacterBody _owner;

        ushort IBuff.Index => index;
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract ushort Count { get; }
        public abstract Sprite IconSprite { get; }
        public abstract EBuffType Type { get; }

        public CharacterBody Owner => _owner;

        void IBuff.AddToOwner(CharacterBody owner) {
            _owner = owner;
            OnAddedToOwner();
        }

        void IBuff.Serialize(FastBufferWriter writer) => OnSerialize(writer);

        void IBuff.Deserialize(FastBufferReader reader) => OnDeserialize(reader);

        void IBuff.MergeWith(IBuff buff) => MergeWith(buff as T);

        void IBuff.RemoveFromOwner() {
            OnRemoveFromOwner();
            _owner = null;
        }

        protected void RemoveSelfOnLocal() => _owner.RemoveBuffOnLocal(index);

        protected virtual void OnSerialize(FastBufferWriter writer) {
        }

        protected virtual void OnDeserialize(FastBufferReader reader) {
        }

        protected virtual void OnAddedToOwner() {
        }

        protected virtual void MergeWith(T buff) {
        }

        protected virtual void OnRemoveFromOwner() {
        }
    }
}