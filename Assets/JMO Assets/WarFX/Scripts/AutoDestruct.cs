using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestruct : MonoBehaviour {
    public bool OnlyDeactivate;

    private void OnEnable() {
        StartCoroutine("CheckIfAlive");
    }

    private IEnumerator CheckIfAlive() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if (!GetComponent<ParticleSystem>().IsAlive(true)) {
                if (OnlyDeactivate) {
#if UNITY_3_5
						this.gameObject.SetActiveRecursively(false);
#else
                    gameObject.SetActive(false);
#endif
                } else {
                    GameObject.Destroy(gameObject);
                }

                break;
            }
        }
    }
}
