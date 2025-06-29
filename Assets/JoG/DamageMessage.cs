using System;
using Unity.Netcode;
using UnityEngine;

namespace JoG {

    [Serializable]
    public struct DamageMessage : INetworkSerializable {
        public static readonly int Size = sizeof(uint) + sizeof(float) + sizeof(ulong) + (sizeof(float) * 3) * 2;
        public uint value;
        public float cofficient;
        public Vector3 position;
        public Vector3 impulse;
        public ulong flags;
        public NetworkObjectReference attacker;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.PreCheck(Size)) {
                serializer.SerializeValuePreChecked(ref value);
                serializer.SerializeValuePreChecked(ref cofficient);
                serializer.SerializeValuePreChecked(ref flags);
                serializer.SerializeValuePreChecked(ref position);
                serializer.SerializeValuePreChecked(ref impulse);
            }
            serializer.SerializeNetworkSerializable(ref attacker);
        }
    }
}