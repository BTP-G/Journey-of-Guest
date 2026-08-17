using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xoderony.Extensions {

    public static class Vector3Extensions {

        public const float Epsilon = 1e-5f;

        public const float SqrEpsilon = 1e-10f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddMagnitude(this in Vector3 v, float length) {
            if (length == 0) {
                return v;
            }
            var sqr = v.sqrMagnitude;
            if (sqr > SqrEpsilon) {
                return v * (1f + (length / MathF.Sqrt(sqr)));
            } else {
                return v;
            }
        }

        /// <summary>
        /// 将输入向量映射到由 planeNormal 定义且受 up 约束的切向方向。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnTangent(this in Vector3 v, in Vector3 planeNormal, in Vector3 up) {
            return planeNormal.Cross(v.Cross(up));
        }

        /// <summary>
        /// 判断向量是否接近零向量（通用场景）。
        /// </summary>
        /// <param name="v">要检测的向量</param>
        /// <param name="sqrTolerance">平方容差阈值。默认为 1e-10（对应长度容差 1e-5）。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this in Vector3 v, float sqrTolerance = SqrEpsilon) {
            return v.sqrMagnitude <= sqrTolerance;
        }

        /// <summary>
        /// 判断向量是否**不接近**零向量（通用场景）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotZero(this in Vector3 v, float sqrTolerance = SqrEpsilon) {
            return v.sqrMagnitude > sqrTolerance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ChangeMagnitude(this in Vector3 v, float magnitude) {
            var sqr = v.sqrMagnitude;
            if (sqr > SqrEpsilon) {
                return v * (magnitude / MathF.Sqrt(sqr));
            } else {
                return v;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnIntersection(this in Vector3 v, in Vector3 planeANormal, in Vector3 planeBNormal) {
            return v.Project(planeANormal.Cross(planeBNormal));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(this in Vector3 lhs, in Vector3 rhs) {
            return (lhs.x * rhs.x) + (lhs.y * rhs.y) + (lhs.z * rhs.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(this in Vector3 lhs, in Vector3 rhs) {
            return new(
                (lhs.y * rhs.z) - (lhs.z * rhs.y),
                (lhs.z * rhs.x) - (lhs.x * rhs.z),
                (lhs.x * rhs.y) - (lhs.y * rhs.x)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpTo(this in Vector3 from, in Vector3 to, float t) {
            if (t <= 0) {
                return from;
            }
            if (t >= 1) {
                return to;
            }
            return from + ((to - from) * t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpToUnclamped(this in Vector3 from, in Vector3 to, float t) {
            return from + ((to - from) * t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveTowards(this in Vector3 current, in Vector3 target, float maxDistanceDelta) {
            var toTarget = target - current;
            var sqrDistance = toTarget.sqrMagnitude;
            var maxDistanceDeltaSqr = maxDistanceDelta * maxDistanceDelta;
            if (sqrDistance <= maxDistanceDeltaSqr) {
                return target;
            }
            var factor = maxDistanceDelta / MathF.Sqrt(sqrDistance);
            return current + (toTarget * factor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Project(this in Vector3 v, in Vector3 onNormal) {
            var sqr = onNormal.sqrMagnitude;
            if (sqr > SqrEpsilon) {
                return v.Dot(onNormal) / sqr * onNormal;
            } else {
                return Vector3.zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnPlane(this in Vector3 v, in Vector3 planeNormal) {
            var sqr = planeNormal.sqrMagnitude;
            if (sqr > SqrEpsilon) {
                return v - (v.Dot(planeNormal) / sqr * planeNormal);
            } else {
                return v;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetDirectionAndMagnitude(this in Vector3 v, out Vector3 direction, out float magnitude) {
            magnitude = v.magnitude;
            if (magnitude > Mathf.Epsilon) {
                direction = v / magnitude;
            } else {
                direction = Vector3.zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnPlaneAlongDirection(this in Vector3 v, in Vector3 planeNormal, in Vector3 direction) {
            return v - (v.Dot(planeNormal) / planeNormal.Dot(direction) * direction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ApplySpread(this in Vector3 direction, in Vector3 upwards, float horizontal, float vertical) {
            if (horizontal > Mathf.Epsilon) {
                var result = Quaternion.AngleAxis(horizontal, upwards) * direction;
                if (vertical > Mathf.Epsilon) {
                    result = Quaternion.AngleAxis(vertical, direction.Cross(Vector3.up)) * result;
                }
                return result;
            }
            if (vertical > Mathf.Epsilon) {
                return Quaternion.AngleAxis(vertical, direction.Cross(upwards)) * direction;
            }
            return direction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleTo(this in Vector3 from, in Vector3 to) {
            var sqr = from.sqrMagnitude * to.sqrMagnitude;
            if (sqr <= SqrEpsilon) {
                return 0f;
            }

            var cos = from.Dot(to) / MathF.Sqrt(sqr);
            if (cos >= 1) {
                return 0f;
            }

            if (cos <= -1) {
                return 180f;
            }
            return MathF.Acos(cos) * Mathf.Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceTo(this in Vector3 from, in Vector3 to) {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var dz = to.z - from.z;
            return MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrDistanceTo(this in Vector3 from, in Vector3 to) {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var dz = to.z - from.z;
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithX(this in Vector3 v, float newX) {
            return new(newX, v.y, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithY(this in Vector3 v, float newY) {
            return new(v.x, newY, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WithZ(this in Vector3 v, float newZ) {
            return new(v.x, v.y, newZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddX(this in Vector3 v, float deltaX) {
            return new(v.x + deltaX, v.y, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddY(this in Vector3 v, float deltaY) {
            return new(v.x, v.y + deltaY, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddZ(this in Vector3 v, float deltaZ) {
            return new(v.x, v.y, v.z + deltaZ);
        }
    }
}
