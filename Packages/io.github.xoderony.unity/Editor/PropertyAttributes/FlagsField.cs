using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

internal class FlagsField : BaseField<Enum> {
    private static readonly StringBuilder _sb = new();
    private readonly List<(string name, Enum value)> _options = new();
    private VisualElement _visualInput;
    private TextElement _textElement;
    private VisualElement _arrowElement;
    private Type _enumType;
    private Type _underlyingType;
    private SerializedProperty _property;

    public FlagsField(SerializedProperty property, Type enumType) : base(property.displayName, null) {
        _property = property;
        _enumType = enumType;
        _underlyingType = Enum.GetUnderlyingType(enumType);
        rawValue = Enum.ToObject(_enumType, property.boxedValue) as Enum;
        tooltip = property.tooltip;
        AddToClassList(EnumFlagsField.ussClassName);
        AddToClassList(BasePopupField<int, int>.ussClassName);

        _visualInput = this.Q<VisualElement>(className: inputUssClassName);
        _visualInput.AddToClassList(EnumFlagsField.inputUssClassName);
        _visualInput.AddToClassList(BasePopupField<int, int>.inputUssClassName);

        labelElement.AddToClassList(EnumFlagsField.labelUssClassName);
        labelElement.AddToClassList(BasePopupField<int, int>.labelUssClassName);

        _textElement = new TextElement { pickingMode = PickingMode.Ignore };
        _textElement.AddToClassList(BasePopupField<int, int>.textUssClassName);
        _textElement.AddToClassList(EnumFlagsField.textUssClassName);
        _visualInput.Add(_textElement);

        _arrowElement = new VisualElement { pickingMode = PickingMode.Ignore };
        _arrowElement.AddToClassList(EnumFlagsField.arrowUssClassName);
        _arrowElement.AddToClassList(BasePopupField<int, int>.arrowUssClassName);
        _visualInput.Add(_arrowElement);

        BuildOptions();
        UpdateText();

        RegisterCallback<ClickEvent>(OnClicked);
        RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        this.TrackPropertyValue(property, property => {
            value = Enum.ToObject(_enumType, property.boxedValue) as Enum;
        });
    }

    public override void SetValueWithoutNotify(Enum newValue) {
        base.SetValueWithoutNotify(newValue);
        UpdateText();
    }

    public bool HasFlags(Enum flag) {
        return rawValue.HasFlag(flag);
    }

    protected override void UpdateMixedValueContent() {
        UpdateText();
    }

    private void BuildOptions() {
        var names = Enum.GetNames(_enumType);
        var values = Enum.GetValues(_enumType);
        var everythingFlag = 0UL;
        for (int i = 0; i < names.Length; i++) {
            var enumValue = (Enum)values.GetValue(i);
            var enumValueAsUInt64 = ToUInt64(enumValue);
            if (enumValueAsUInt64 == 0) continue; // 跳过0值选项
            everythingFlag |= enumValueAsUInt64;
            var name = ObjectNames.NicifyVariableName(names[i]);
            _options.Add((name, enumValue));
        }
        _options.Insert(0, ("Everything", ToEnum(everythingFlag)));
    }

    private void OnClicked(ClickEvent evt) {
        ShowDropdown();
        evt.StopPropagation();
    }

    private void OnNavigationSubmit(NavigationSubmitEvent evt) {
        ShowDropdown();
        evt.StopPropagation();
    }

    private void ShowDropdown() {
        var menu = new GenericDropdownMenu();
        foreach (var (name, flag) in _options) {
            var hasFlag = HasFlags(flag);
            var data = hasFlag ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
            menu.AddItem(name, hasFlag, OnClickItem, data);
            void OnClickItem(object _) {
                var valueAsUInt64 = ToUInt64(rawValue);
                if (hasFlag) {
                    valueAsUInt64 &= ~ToUInt64(flag);
                } else {
                    valueAsUInt64 |= ToUInt64(flag);
                }
                _property.boxedValue = Convert.ChangeType(valueAsUInt64, _underlyingType);
                _property.serializedObject.ApplyModifiedProperties();
            }
        }
        menu.DropDown(_visualInput.worldBound, this, true);
    }

    private void UpdateText() {
        if (HasFlags(_options[0].value)) {
            _textElement.text = "Everything";
            return;
        }
        _sb.Clear();
        foreach (var (name, flag) in _options) {
            if (HasFlags(flag)) {
                _sb.Append(name).Append(", ");
            }
        }
        if (_sb.Length > 2) {
            _sb.Length -= 2;
        } else {
            _sb.Append("Nothing");
        }
        _textElement.text = _sb.ToString();
    }

    private ulong ToUInt64(Enum value) {
        switch (value.GetTypeCode()) {
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return (ulong)Convert.ToInt64(value);

            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return Convert.ToUInt64(value);

            default:
                return 0;
        }
    }

    private Enum ToEnum(ulong value) {
        return (Enum)Enum.ToObject(_enumType, value);
    }
}