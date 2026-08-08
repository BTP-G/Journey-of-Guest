using UnityEngine;

public class DNP_UIMove : MonoBehaviour {
    public Vector2 fromPosition;
    public Vector2 toPosition;
    public float frequency = 4f;

    private RectTransform rectTransform;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void FixedUpdate() {
        rectTransform.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, (Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f);
    }
}
