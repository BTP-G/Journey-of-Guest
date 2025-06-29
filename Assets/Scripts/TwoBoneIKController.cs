using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace JoG.Character {

    [RequireComponent(typeof(TwoBoneIKConstraint))]
    public class TwoBoneIKController : MonoBehaviour {
        public Optional<Transform> targetOverride;
        private Vector3 defaultLocalPosition;
        private Quaternion defaultLocalRotation;
        private Transform _origin;
        private TwoBoneIKConstraint _iKConstraint;
        public TwoBoneIKConstraint IKConstraint => _iKConstraint;

        private void Awake() {
            _iKConstraint = GetComponent<TwoBoneIKConstraint>();
            _origin = _iKConstraint.data.target;
            _origin.GetLocalPositionAndRotation(out defaultLocalPosition, out defaultLocalRotation);
        }

        private void Update() {
            if (targetOverride.TryGet(out var target)) {
                target.GetPositionAndRotation(out var targetPosition, out var targetRotation);
                _origin.SetPositionAndRotation(targetPosition, targetRotation);
            } else {
                _origin.SetLocalPositionAndRotation(defaultLocalPosition, defaultLocalRotation);
            }
        }
    }
}