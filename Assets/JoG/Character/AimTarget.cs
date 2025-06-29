using UnityEngine;

namespace JoG.Character {

    public class AimTarget : MonoBehaviour {
        [field: SerializeField] public Transform AimOrigin { get; private set; }
    }
}