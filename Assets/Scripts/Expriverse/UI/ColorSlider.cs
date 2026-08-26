using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expriverse.UI {

    [DefaultExecutionOrder(-1)]
    public class ColorSlider : MonoBehaviour {
        [SerializeField, ReadOnly] private Color _color;

        public Color Color {
            get => _color;
            set {
                _color = value;
                RSlider.value = value.r;
                GSlider.value = value.g;
                BSlider.value = value.b;
            }
        }

        [field: SerializeField, Required] public Slider RSlider { get; private set; }
        [field: SerializeField, Required] public Slider GSlider { get; private set; }
        [field: SerializeField, Required] public Slider BSlider { get; private set; }
        [field: SerializeField] public Graphic ColorPreview { get; private set; }
        [field: SerializeField] public UnityEvent<Color> OnColorChanged { get; private set; }

        private void Awake() {
            RSlider.onValueChanged.AddListener(r => {
                UpdateColor();
                RSlider.handleRect.GetComponent<Graphic>().color = new Color(r, 0, 0);
            });
            GSlider.onValueChanged.AddListener(g => {
                UpdateColor();
                GSlider.handleRect.GetComponent<Graphic>().color = new Color(0, g, 0);
            });
            BSlider.onValueChanged.AddListener(b => {
                UpdateColor();
                BSlider.handleRect.GetComponent<Graphic>().color = new Color(0, 0, b);
            });
        }

        [Button]
        private void UpdateColor() {
            _color = new Color(RSlider.value, GSlider.value, BSlider.value);
            OnColorChanged.Invoke(_color);
            if (ColorPreview == null) {
                return;
            }
            ColorPreview.color = _color;
        }
    }
}
