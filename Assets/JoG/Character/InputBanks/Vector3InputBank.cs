using UnityEngine;

namespace JoG.Character.InputBanks {

    public class Vector3InputBank : InputBank {
        public Vector3 vector3;

        public bool IsDetected => vector3 != Vector3.zero;

        public void UpdateVector3(in Vector3 newVector3) {
            vector3 = newVector3;
        }

        public override void Reset() {
            vector3 = Vector3.zero;
        }
    }
}