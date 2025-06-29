using EditorAttributes;
using UnityEngine;

namespace JoG.Character {

    public class CharacterModel : MonoBehaviour {
        [field: SerializeField] public Renderer MainRenderer { get; private set; }
        [field: SerializeField] public Collider MainCollider { get; private set; }
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public Transform Top { get; private set; }
        [field: SerializeField] public Transform Center { get; private set; }
        [field: SerializeField] public Transform Bottom { get; private set; }
        public float Radius { get; private set; }
        public float Height { get; private set; }
        public bool IsMainRendererVisible => MainRenderer.isVisible;

        protected void Awake() {
            Height = Vector3.Distance(Top.position, Bottom.position);
            Radius = Height * 0.5f;
        }
    }
}