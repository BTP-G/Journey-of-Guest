using JoG.Character;
using Unity.Netcode;
using UnityEngine;

namespace JoG.BuffSystem {

    /// <summary>Custom buff must inherit from Buff<T>.</summary>
    public interface IBuff {
        Sprite IconSprite { get; }
        ushort Index { get; }
        ushort Count { get; }
        string Name { get; }
        string Description { get; }
        EBuffType Type { get; }
        CharacterBody Owner { get; }

        internal void AddToOwner(CharacterBody owner);

        internal void Serialize(FastBufferWriter writer);

        internal void Deserialize(FastBufferReader reader);

        /// <summary>合并另一个Buff</summary>
        /// <param name="buff">另一个Buff</param>
        internal void MergeWith(IBuff buff);

        internal void RemoveFromOwner();
    }
}