using System;

namespace Expriverse {

    public readonly struct UIStateChangedMessage {
        public readonly string Name;
        public readonly UILayer Layer;
        public readonly bool Active;

        public UIStateChangedMessage(string name, UILayer layer, bool active) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Layer = layer;
            Active = active;
        }
    }
}
