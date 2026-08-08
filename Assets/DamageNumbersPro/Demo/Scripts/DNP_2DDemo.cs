using UnityEngine;

#if ENABLE_INPUT_SYSTEM && DNP_NewInputSystem
using UnityEngine.InputSystem;
#endif

namespace DamageNumbersPro.Demo {
    public class DNP_2DDemo : MonoBehaviour {
        private float nextShotTime;

        private void Start() {
            nextShotTime = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update() {
            HandleShooting();
        }

        private void HandleShooting() {
            if (DNP_InputHandler.GetLeftClick()) {
                Shoot();
                nextShotTime = Time.time + 0.3f;
            } else if (DNP_InputHandler.GetRightHeld() && Time.time > nextShotTime) {
                Shoot();
                nextShotTime = Time.time + 0.06f;
            }
        }

        private void Shoot() {
            var mousePosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM && DNP_NewInputSystem
            if (Mouse.current != null) {
                mousePosition = Mouse.current.position.ReadValue();
            }
#else
            mousePosition = Input.mousePosition;
#endif

            // Raycast
            var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = -5;
            Physics.Raycast(worldPosition, Vector3.forward, out var hit, 10f);

            // Select Damage Number
            var settings = DNP_DemoManager.instance.GetSettings();
            var prefab = DNP_DemoManager.instance.GetCurrent();

            // Number
            var number = 1 + (Mathf.Pow(Random.value, 2.2f) * settings.numberRange);
            if (prefab.digitSettings.decimals == 0) {
                number = Mathf.Floor(number);
            }

            // Create Damage Number
            var newDamageNumber = prefab.Spawn(worldPosition, number);

            if (hit.collider != null) {
                var dnpTarget = hit.collider.GetComponent<DNP_Target>();
                if (dnpTarget != null) {
                    dnpTarget.Hit();
                }

                newDamageNumber.SetFollowedTarget(hit.collider.transform);
            }

            // Apply Demo Settings
            settings.Apply(newDamageNumber);
        }
    }
}
