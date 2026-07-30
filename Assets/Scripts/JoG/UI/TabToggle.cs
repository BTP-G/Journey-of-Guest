using EditorAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace JoG.UI {

    [DisallowMultipleComponent]
    public class TabToggle : MonoBehaviour {
        [Required] public TabGroup group;
        public UnityEvent2 onToggleOn = new();
        public UnityEvent2 onToggleOff = new();
        public ToggleEvent onToggleChanged = new();

        [SerializeField, PropertyOrder(-1)]
        private bool _isOn;

        public bool IsOn => _isOn;
        public bool IsOff => !_isOn;

        [Button(conditionName: nameof(IsOff), conditionResult: ConditionResult.ShowHide, buttonLabel: "开启", buttonHeight: 24)]
        public void SetOn() {
            group.SwitchTo(this);
        }

        internal void Set(bool isOn) {
            if (isOn == _isOn) {
                return;
            }
            _isOn = isOn;
            if (isOn) {
                OnToggleOn();
            } else {
                OnToggleOff();
            }
        }

        private void OnToggleOn() {
            onToggleOn.Invoke();
            onToggleChanged.Invoke(true);
        }

        private void OnToggleOff() {
            onToggleOff.Invoke();
            onToggleChanged.Invoke(false);
        }

        private void Reset() {
            group = GetComponentInParent<TabGroup>();
        }

        [Serializable]
        public class ToggleEvent : UnityEvent2<bool> { }
    }
}
