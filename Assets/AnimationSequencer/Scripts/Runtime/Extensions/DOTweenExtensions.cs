#if DOTWEEN_ENABLED
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using TMPro;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Modified by Pablo Huaxteco
    public static class DOTweenExtensions {
        public static TweenerCore<string, string, StringOptions> DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null) {
            if (endValue == null) {
                if (Debugger.logPriority > 0) {
                    Debugger.LogWarning("You can't pass a NULL string to DOText: an empty string will be used instead to avoid errors");
                }

                endValue = "";
            }
            var t = DOTween.To(() => target.text, x => target.text = x, endValue, duration);
            t.SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetTarget(target);
            return t;
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMove(this Transform target, Vector3 endValue, float duration,
            AxisConstraint axisConstraint, bool snapping = false) {
            return target.DoTransformInternal(endValue, duration, axisConstraint, snapping, () => target.position, value => target.position = value);
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOLocalMove(this Transform target, Vector3 endValue, float duration,
            AxisConstraint axisConstraint, bool snapping = false) {
            return target.DoTransformInternal(endValue, duration, axisConstraint, snapping, () => target.localPosition, value => target.localPosition = value);
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOScale(this Transform target, Vector3 endValue, float duration,
            AxisConstraint axisConstraint, bool snapping = false) {
            return target.DoTransformInternal(endValue, duration, axisConstraint, snapping, () => target.localScale, value => target.localScale = value);
        }

        private static TweenerCore<Vector3, Vector3, VectorOptions> DoTransformInternal(this Transform target, Vector3 endValue, float duration,
            AxisConstraint axisConstraint, bool snapping, Func<Vector3> getCurrentValue, Action<Vector3> setNewValue) {
            var useX = (axisConstraint & AxisConstraint.X) == AxisConstraint.X;
            var useY = (axisConstraint & AxisConstraint.Y) == AxisConstraint.Y;
            var useZ = (axisConstraint & AxisConstraint.Z) == AxisConstraint.Z;

            // Check if no specific axis is selected; apply movement on all axes. (None, Everything or "W" selected).
            var notValueSelected = !useX && !useY && !useZ;
            if (notValueSelected) {
                useX = useY = useZ = true;
            }

            if (!useX) {
                endValue.x = 0;
            }

            if (!useY) {
                endValue.y = 0;
            }

            if (!useZ) {
                endValue.z = 0;
            }

            var tempValue = getCurrentValue();
            var tweenerCore = DOTween.To(() => getCurrentValue(), delegate (Vector3 x) {
                tempValue = getCurrentValue();

                if (useX) {
                    tempValue.x = x.x;
                }

                if (useY) {
                    tempValue.y = x.y;
                }

                if (useZ) {
                    tempValue.z = x.z;
                }

                setNewValue(tempValue);
            }, endValue, duration);
            tweenerCore.SetOptions(snapping).SetTarget(target);
            return tweenerCore;
        }
    }
}
#endif