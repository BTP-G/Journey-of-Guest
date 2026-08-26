using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour {
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = clickSound;

        // 找到场景里所有按钮
        var allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (var btn in allButtons) {
            btn.onClick.AddListener(() => PlayClickSound());
        }
    }

    private void PlayClickSound() {
        audioSource.PlayOneShot(clickSound);
    }
}
