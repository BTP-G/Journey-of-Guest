using System;
using System.Buffers;
using Unity.Cinemachine;
using UnityEngine;
using Xoderony.Extensions;

namespace Expriverse.Cinemachine {

    [DisallowMultipleComponent]
    public class CinemachineOrbitalFollowAim : CinemachineExtension, IComponent {
        public Rigidbody selfBody;

        /// <summary>Objects on these layers will be detected.</summary>
        [Tooltip("Objects on these layers will be detected")]
        public LayerMask aimMask;

        /// <summary>How far to project the object detection ray.</summary>
        [Tooltip("How far to project the object detection ray")]
        [Delayed]
        public float aimDistance;

        /// <summary>
        /// World space _position of where the player would hit if a projectile were to be fired from
        /// the player origin. This may be different from _lifeState.ReferenceLookAt due to camera offset
        /// from player origin.
        /// </summary>
        public Collider AimTarget { get; private set; }

        public Vector3 AimPoint { get; private set; }

        public Quaternion AimRotation { get; private set; }

        /// <summary>
        /// Sets the ReferenceLookAt to be the result of a raycast in the direction of camera
        /// forward. If an object is hit, point is placed there, else it is placed at aimDistance
        /// along the ray.
        /// </summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera _lifeState</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            switch (stage) {
                case CinemachineCore.Stage.Body: {
                    //if (NoiseCancellation) {
                    //    // Raycast to establish what we're actually aiming at
                    //    var player = vcam.LookAt;
                    //    if (player != null) {
                    //        _lifeState.ReferenceLookAt = UpateCameraLookAt(_lifeState.GetCorrectedPosition(), player, player.forward);
                    //        AimPoint = UpdatePlayerAim(_lifeState.ReferenceLookAt, player);
                    //    }
                    //}
                    break;
                }
                case CinemachineCore.Stage.Finalize: {
                    // Raycast to establish what we're actually aiming at. In this case we do it
                    // without cancelling the noise.
                    var player = vcam.LookAt;
                    UpateCameraLookAt(ref state, player);
                    UpdatePlayerAim(state.ReferenceLookAt, player);
                    break;
                }
            }
            AimRotation = state.GetCorrectedOrientation();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(AimPoint, 0.1f);
        }

        private void OnValidate() {
            aimDistance = Mathf.Max(1, aimDistance);
        }

        private void Reset() {
            aimMask = 1;
            aimDistance = 200.0f;
        }

        private void UpateCameraLookAt(ref CameraState state, Transform player) {
            // We don't want to hit targets behind the player
            var cameraPosition = state.GetCorrectedPosition();
            var cameraRotation = state.GetCorrectedOrientation();
            var forward = cameraRotation * Vector3.forward;
            var playerLocalPosition = Quaternion.Inverse(cameraRotation) * (player.position - cameraPosition);
            cameraPosition += forward * playerLocalPosition.z;
            var aimDistance = Mathf.Max(1, this.aimDistance - playerLocalPosition.z);
            if (Raycast(cameraPosition, forward, aimDistance, out var result)) {
                AimTarget = result.collider;
                state.ReferenceLookAt = result.point;
            } else {
                AimTarget = null;
                state.ReferenceLookAt = cameraPosition + (forward * aimDistance);
            }
        }

        private void UpdatePlayerAim(in Vector3 cameraLookAt, Transform player) {
            // Adjust for actual player aim target (may be different due to offset)
            var playerPosition = player.position;
            var direction = cameraLookAt - playerPosition;
            if (Raycast(playerPosition, direction, direction.magnitude, out var result)) {
                AimTarget = result.collider;
                AimPoint = result.point;
            } else {
                AimPoint = cameraLookAt;
            }
        }

        private bool Raycast(in Vector3 origin, in Vector3 direction, float distance, out RaycastHit result) {
            result = default;
            var hasHit = false;
            var buffer = ArrayPool<RaycastHit>.Shared.Rent(256);
            var hitCount = Physics.RaycastNonAlloc(origin, direction, buffer, distance, aimMask, QueryTriggerInteraction.Ignore);
            var closestSqrDistance = float.MaxValue;
            foreach (ref var hit in buffer.AsSpan(0, hitCount)) {
                var collider = hit.collider;
                if (collider.attachedRigidbody == selfBody) {
                    continue;
                }

                var sqrDistance = origin.SqrDistanceTo(hit.point);
                if (sqrDistance < closestSqrDistance) {
                    closestSqrDistance = sqrDistance;
                    hasHit = true;
                    result = hit;
                }
            }
            ArrayPool<RaycastHit>.Shared.Return(buffer);
            return hasHit;
        }
    }
}
