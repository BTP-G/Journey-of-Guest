using UnityEngine;

namespace JoG {

    public class ActiveWhenVisible : MonoBehaviour {

        protected void OnBecameVisible() {
            gameObject.SetActive(true);
        }

        protected void OnBecameInvisible() {
            gameObject.SetActive(false);
        }
    }
}