using UnityEngine;

namespace JoG {

    public class AutoMove : MonoBehaviour {
        public Vector3 moveVector = Vector3.forward;
        public Space space = Space.Self;

        private void Update() {
            transform.Translate(moveVector * Time.deltaTime, space);
        }
    }
}
