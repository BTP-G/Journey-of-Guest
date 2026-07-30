using UnityEngine;

namespace JoG {

    public static class LayerMasks {
        public static LayerMask Character = LayerMask.GetMask(nameof(Character));
        public static LayerMask CharacterPart = LayerMask.GetMask(nameof(CharacterPart));
        public static LayerMask Default = LayerMask.GetMask(nameof(Default));
        public static LayerMask IgnoreRaycast = LayerMask.GetMask("Ignore Raycast");
        public static LayerMask Pickup = LayerMask.GetMask(nameof(Pickup));
        public static LayerMask Projectile = LayerMask.GetMask(nameof(Projectile));
        public static LayerMask Prop = LayerMask.GetMask(nameof(Prop));
        public static LayerMask TransparentFX = LayerMask.GetMask(nameof(TransparentFX));
        public static LayerMask Trigger = LayerMask.GetMask(nameof(Trigger));
        public static LayerMask UI = LayerMask.GetMask(nameof(UI));
        public static LayerMask Water = LayerMask.GetMask(nameof(Water));
        public static LayerMask Invisible = LayerMask.GetMask(nameof(Invisible));
    }
}
