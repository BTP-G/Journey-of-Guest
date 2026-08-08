#if DOTWEEN_ENABLED
using DG.Tweening;
using System;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Created by Pablo Huaxteco
    [Serializable]
    public sealed class PunchRotationTweenAction : PunchBaseTweenAction {
        public override string DisplayName => "Punch Rotation";

        public PunchRotationTweenAction() {
            punch = new Vector3(45, 45, 45);
        }

        private Transform targetTransform;
        private Quaternion originalRotation;

        protected override Tweener GenerateTween_Internal(GameObject target, float duration) {
            targetTransform = target.transform;
            originalRotation = targetTransform.localRotation;

            var tween = targetTransform.DOPunchRotation(punch, duration, vibrato, elasticity);

            return tween;
        }

        protected override void ResetToInitialState_Internal() {
            if (targetTransform == null) {
                return;
            }

            targetTransform.localRotation = originalRotation;
        }
    }
}
#endif