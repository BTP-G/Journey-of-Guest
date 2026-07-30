using Xoderony.PropertyAttributes;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(FlagsFieldAttribute))]
internal class FlagsFieldDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var root = new VisualElement();
        var attr = (FlagsFieldAttribute)attribute;
        root.Add(new FlagsField(property, attr.EnumType));
        return root;
    }
}