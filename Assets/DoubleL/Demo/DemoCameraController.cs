using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DoubleL
{
    public class DemoCameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        public float boost = 3.5f;
                
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));
                
        public float rotationLerpTime = 0.01f;

        public bool invertY = false;

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed)
                {
                    direction += Vector3.forward;
                }
                if (kb.sKey.isPressed)
                {
                    direction += Vector3.back;
                }
                if (kb.aKey.isPressed)
                {
                    direction += Vector3.left;
                }
                if (kb.dKey.isPressed)
                {
                    direction += Vector3.right;
                }
                if (kb.qKey.isPressed)
                {
                    direction += Vector3.down;
                }
                if (kb.eKey.isPressed)
                {
                    direction += Vector3.up;
                }
            }
            return direction;
        }

        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
            if (mouse != null && mouse.rightButton.wasReleasedThisFrame)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
            if (mouse != null && mouse.rightButton.isPressed)
            {
                // mouse.delta 是像素偏移，乘以一个小系数以靠近旧 Input.GetAxis 行为
                var rawDelta = mouse.delta.ReadValue();
                var mouseMovement = new Vector2(rawDelta.x, rawDelta.y * (invertY ? 1 : -1)) * 0.1f;

                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            
            var translation = GetInputTranslationDirection() * Time.deltaTime;
                        
            if (kb != null && kb.leftShiftKey.isPressed)
            {
                translation *= 10.0f;
            }

            if (mouse != null)
            {
                boost += mouse.scroll.ReadValue().y * 0.2f;
            }
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }
}
