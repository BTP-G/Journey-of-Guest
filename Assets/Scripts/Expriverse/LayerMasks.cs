using UnityEngine;

namespace Expriverse {

    public static class LayerMasks {
        public static readonly LayerMask Character = LayerMask.GetMask(nameof(Character));
        public static readonly LayerMask CharacterPart = LayerMask.GetMask(nameof(CharacterPart));
        public static readonly LayerMask Default = LayerMask.GetMask(nameof(Default));
        public static readonly LayerMask IgnoreRaycast = LayerMask.GetMask("Ignore Raycast");
        public static readonly LayerMask Pickup = LayerMask.GetMask(nameof(Pickup));
        public static readonly LayerMask Projectile = LayerMask.GetMask(nameof(Projectile));
        public static readonly LayerMask Prop = LayerMask.GetMask(nameof(Prop));
        public static readonly LayerMask TransparentFX = LayerMask.GetMask(nameof(TransparentFX));
        public static readonly LayerMask Trigger = LayerMask.GetMask(nameof(Trigger));
        public static readonly LayerMask UI = LayerMask.GetMask(nameof(UI));
        public static readonly LayerMask Water = LayerMask.GetMask(nameof(Water));
        public static readonly LayerMask Invisible = LayerMask.GetMask(nameof(Invisible));
    }
}
