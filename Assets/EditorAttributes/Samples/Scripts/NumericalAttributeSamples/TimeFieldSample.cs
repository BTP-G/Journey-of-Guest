using EditorAttributes;
using UnityEngine;

namespace EditorAttributesSamples {
    [HelpURL("https://editorattributesdocs.readthedocs.io/en/latest/Attributes/NumericalAttributes/timefield.html")]
    public class TimeFieldSample : MonoBehaviour {
        [Header("TimeField Attribute:")]
        [Rename(nameof(ConversionResultDays), stringInputMode: StringInputMode.Dynamic)]
        [SerializeField, TimeField(TimeFormat.YearMonthWeek, Unit.Day)][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete] private int intField;

        [Rename(nameof(ConversionResultSeconds), stringInputMode: StringInputMode.Dynamic)]
        [SerializeField, TimeField(TimeFormat.DayHourMinute, Unit.Second)][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete][System.Obsolete] private float floatField;

        [System.Obsolete]
        private string ConversionResultDays() {
            return $"{intField} Days";
        }

        [System.Obsolete]
        private string ConversionResultSeconds() {
            return $"{floatField} Seconds";
        }
    }
}
