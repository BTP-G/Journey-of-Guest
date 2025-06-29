using UnityEngine;

namespace JoG.Character {

    public class AimOrigin : MonoBehaviour {
        [field: SerializeField] public Transform AimTarget { get; private set; }
    }
}