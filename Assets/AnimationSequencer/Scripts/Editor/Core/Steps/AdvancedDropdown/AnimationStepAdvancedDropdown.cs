#if DOTWEEN_ENABLED
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    public sealed class AnimationStepAdvancedDropdown : AdvancedDropdown {
        private Action<AnimationStepAdvancedDropdownItem> callBack;

        public AnimationStepAdvancedDropdown(AdvancedDropdownState state) : base(state) {
            minimumSize = new Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot() {
            var root = new AdvancedDropdownItem("Animation Step");

            var availableTypesOfAnimationStep = TypeCache.GetTypesDerivedFrom(typeof(AnimationStepBase));
            foreach (var animatedItemType in availableTypesOfAnimationStep) {
                if (animatedItemType.IsAbstract) {
                    continue;
                }

                var animationStepBase = Activator.CreateInstance(animatedItemType) as AnimationStepBase;

                var displayName = animationStepBase.GetType().Name;
                if (!string.IsNullOrEmpty(animationStepBase.DisplayName)) {
                    displayName = animationStepBase.DisplayName;
                }

                root.AddChild(new AnimationStepAdvancedDropdownItem(animationStepBase, displayName));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item) {
            base.ItemSelected(item);
            callBack?.Invoke(item as AnimationStepAdvancedDropdownItem);
        }

        public void Show(Rect rect, Action<AnimationStepAdvancedDropdownItem> onItemSelectedCallback) {
            callBack = onItemSelectedCallback;
            base.Show(rect);
        }
    }
}
#endif