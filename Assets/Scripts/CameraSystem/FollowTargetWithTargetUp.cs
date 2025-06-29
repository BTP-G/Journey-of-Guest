using Unity.Cinemachine;
using UnityEngine;

namespace JoG.CameraSystem {

    public class FollowTargetWithTargetUp : MonoBehaviour {

        [SerializeField]
        private CinemachineVirtualCameraBase _camera;

        [SerializeField]
        private readonly GameObject _camerafollowOverridePrefab;

        private Transform _cameraFollow;
        private Transform _cameraFollowOverride;

        private void Awake() {
            if (!_camera) {
                _camera = GetComponent<CinemachineVirtualCameraBase>();
            }
        }

        private void Update() {
            if (_camera.Follow) {
                if (_camera.Follow != _cameraFollowOverride) {
                    _cameraFollow = _camera.Follow;
                    _cameraFollowOverride = Instantiate(_camerafollowOverridePrefab).transform;
                    _camera.Follow = _cameraFollowOverride;
                }
                _cameraFollowOverride.SetPositionAndRotation(_cameraFollow.position,
                    Quaternion.FromToRotation(Vector3.up, _cameraFollow.up));
            }
        }
    }
}