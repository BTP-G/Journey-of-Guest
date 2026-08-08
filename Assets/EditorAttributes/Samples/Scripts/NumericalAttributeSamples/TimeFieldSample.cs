using EditorAttributes;
using UnityEngine;

namespace EditorAttributesSamples {
    [HelpURL("https://editorattributesdocs.readthedocs.io/en/latest/Attributes/NumericalAttributes/unitfield.html")]
    public class TimeFieldSample : MonoBehaviour {
        [Header("UnitField Attribute:")]
        [Rename(nameof(ConversionResultDays), stringInputMode: StringInputMode.Dynamic)]
        [SerializeField, UnitField(Unit.Week, Unit.Day)] private int intField;

        [Rename(nameof(ConversionResultSeconds), stringInputMode: StringInputMode.Dynamic)]
        [SerializeField, UnitField(Unit.Minute, Unit.Second)] private float floatField;

        private string ConversionResultDays() {
            return $"{intField} Days";
        }

        private string ConversionResultSeconds() {
            return $"{floatField} Seconds";
        }
    }
}
