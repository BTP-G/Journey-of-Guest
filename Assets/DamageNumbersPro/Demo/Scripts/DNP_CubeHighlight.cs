using UnityEngine;

namespace DamageNumbersPro.Demo {
    public class DNP_CubeHighlight : MonoBehaviour {
        public string propertyName = "_Color";
        public AnimationCurve propertyCurve;
        public float destructionDelay = 0.2f;

        private Material mat;

        private int propertyID;
        private float startTime;

        private void Start() {
            startTime = Time.time;
            propertyID = Shader.PropertyToID(propertyName);

            var mr = GetComponent<MeshRenderer>();
            mat = mr.material;

            Destroy(gameObject, destructionDelay);
        }

        private void FixedUpdate() {
            mat.SetColor(propertyID, new Color(1, 0, 0, propertyCurve.Evaluate(Time.time - startTime)));
        }
    }
}
