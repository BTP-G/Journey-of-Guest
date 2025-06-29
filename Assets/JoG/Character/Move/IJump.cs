using UnityEngine;

namespace JoG.Character.Move {

    public interface IJump {

        public bool TryJump(in Vector3? directionOverride = null);
    }
}