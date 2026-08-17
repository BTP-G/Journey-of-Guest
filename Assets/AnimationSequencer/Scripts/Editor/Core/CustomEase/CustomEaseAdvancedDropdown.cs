#if DOTWEEN_ENABLED
using DG.Tweening;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    public sealed class CustomEaseAdvancedDropdownItem : AdvancedDropdownItem {
        private readonly int easeEnumIndex;
        public int EaseEnumIndex => easeEnumIndex;

        public CustomEaseAdvancedDropdownItem(int enumIndex, string displayName) : base(displayName) {
            easeEnumIndex = enumIndex;
        }
    }
    public sealed class CustomEaseAdvancedDropdown : AdvancedDropdown {
        private Action<CustomEaseAdvancedDropdownItem> callBack;

        public CustomEaseAdvancedDropdown(AdvancedDropdownState state) : base(state) {
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() {
            var root = new AdvancedDropdownItem("Easing Modes");

            var names = Enum.GetNames(typeof(Ease));
            for (var i = 0; i < names.Length; i++) {
                var name = names[i];
                if (string.Equals(name, "INTERNAL_Zero", StringComparison.Ordinal)
                    || string.Equals(name, "INTERNAL_Custom", StringComparison.Ordinal)) {
                    continue;
                }

                root.AddChild(new CustomEaseAdvancedDropdownItem(i, name));
            }

            root.AddChild(new CustomEaseAdvancedDropdownItem((int)Ease.INTERNAL_Custom, "Custom"));
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            base.ItemSelected(item);
            callBack?.Invoke(item as CustomEaseAdvancedDropdownItem);
        }

        public void Show(Rect rect, Action<CustomEaseAdvancedDropdownItem> onItemSelectedCallback) {
            callBack = onItemSelectedCallback;
            base.Show(rect);
        }
    }
}
#endif
