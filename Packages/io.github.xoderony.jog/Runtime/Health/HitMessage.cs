using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Health {

    public struct HitMessage : INetworkSerializable {

        public static readonly int PreCheckedSize = Unsafe.SizeOf<HitMessage>();

        public Vector3 point;

        public Vector3 impulse;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.PreCheck(PreCheckedSize)) {
                serializer.SerializeValuePreChecked(ref point);
                serializer.SerializeValuePreChecked(ref impulse);
            }
        }
    }
}
