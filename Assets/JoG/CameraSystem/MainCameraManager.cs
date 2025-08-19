using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace JoG.CameraSystem {

    public class MainCameraManager : MonoBehaviour {
        public static Optional<Transform> main;
        private static readonly List<MainCameraManager> cameras = new();

        private void Awake() {
            cameras.Add(this);
        }

        private void OnEnable() {
            cameras.ForEach(c => {
                if (c != this) {
                    c.gameObject.SetActive(false);
                }
            });
            main = new(true, transform);
        }

        private void OnDestroy() {
            cameras.Remove(this);
            if (main.Value == transform) {
                if (cameras.Count > 0) {
                    cameras[^1].gameObject.SetActive(true);
                } else {
                    main = default;
                }
            }
        }
    }
}