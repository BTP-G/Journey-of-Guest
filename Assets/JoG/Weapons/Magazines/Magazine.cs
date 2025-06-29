using System;
using System.Text;

namespace JoG.Weapons.Magazines {

    [Serializable]
    public struct Magazine {
        [NonSerialized] public int count;
        public int capacity;
        private static readonly StringBuilder stringBuilder = new();

        public Magazine(int capacity) {
            count = this.capacity = capacity;
        }

        public readonly bool IsFull => count == capacity;
        public readonly bool IsEmpty => count == 0;

        public void Reload() => count = capacity;

        public override readonly string ToString() => stringBuilder.Clear()
                                                                   .Append(count)
                                                                   .Append('/')
                                                                   .Append(capacity)
                                                                   .ToString();
    }
}