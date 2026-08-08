using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Xoderony.Netcode.Editor {

    [CustomEditor(typeof(NetworkBehaviour), true)]
    [CanEditMultipleObjects]
    public class NetworkBehaviourEditor : UnityEditor.Editor {

        private static readonly Dictionary<Type, BuildFieldFunc> typeToBuilder = new() {
            { typeof(sbyte), BuildSByteField },
            { typeof(short), BuildShortField },
            { typeof(int), BuildIntField },
            { typeof(byte), BuildByteField },
            { typeof(ushort), BuildUShortField },
            { typeof(uint), BuildUIntField },
            { typeof(long), BuildLongField },
            { typeof(ulong), BuildULongField },
            { typeof(float), BuildFloatField },
            { typeof(double), BuildDoubleField },
            { typeof(bool), BuildBoolField },
            { typeof(string), BuildStringField },
            { typeof(Vector2), BuildVector2Field },
            { typeof(Vector3), BuildVector3Field },
            { typeof(Vector4), BuildVector4Field },
            { typeof(Vector2Int), BuildVector2IntField },
            { typeof(Vector3Int), BuildVector3IntField },
            { typeof(Quaternion), BuildQuaternionField },
            { typeof(Color), BuildColorField },
            { typeof(Color32), BuildColor32Field },
            { typeof(Bounds), BuildBoundsField },
            { typeof(BoundsInt), BuildBoundsIntField },
            { typeof(Rect), BuildRectField },
            { typeof(RectInt), BuildRectIntField },
            { typeof(LayerMask), BuildLayerMaskField },
            { typeof(Hash128), BuildHash128Field },
        };

        private List<FieldInfo> _networkVarFields = new();
        private List<FieldInfo> _networkListFields = new();
        private NetworkBehaviour _target;

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();
            var iterator = serializedObject.GetIterator();
            var expanded = true;
            while (iterator.NextVisible(expanded)) {
                if (iterator.propertyPath == "m_Script") {
                    continue;
                }

                if (!iterator.type.StartsWith("NetworkVariable")) {
                    var child = new PropertyField(iterator) {
                        name = "PropertyField:" + iterator.propertyPath
                    };
                    root.Add(child);
                }
                expanded = false;
            }
            foreach (var field in _networkVarFields) {
                var label = "(🌐)" + field.Name;
                var tooltipAttribute = field.GetCustomAttribute<TooltipAttribute>();
                var tooltip = "([NetworkVariable]: It can not be serialized and can only be changed during runtime.)\n" + tooltipAttribute?.tooltip;
                var elementName = "NetworkVariableField:" + field.Name;
                var nv = field.GetValue(_target) as dynamic;
                if (nv == null) {
                    var info = new HelpBox($"[Field: {field.Name}] Value is null (not initialized)", HelpBoxMessageType.Warning);
                    root.Add(info);
                    continue;
                }
                var boxedValue = nv.Value;
                var boxedType = boxedValue.GetType();
                if (boxedType.IsEnum) {
                    root.Add(BuildEnumField(elementName, label, tooltip, Application.isPlaying, nv));
                    continue;
                }
                if (typeToBuilder.TryGetValue(boxedType, out BuildFieldFunc builder)) {
                    root.Add(builder(elementName, label, tooltip, Application.isPlaying, nv));
                    continue;
                }
            }
            foreach (var field in _networkListFields) {
                var container = new VisualElement();
                container.style.marginTop = 5;
                var label = new Label(ObjectNames.NicifyVariableName("(🌐)" + field.Name)) {
                    tooltip = "NetworkList(read - only): This variable is a NetworkList. It is rendered, but you can't serialize or change it.",
                    enabledSelf = false
                };
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(label);
                var fieldValue = field.GetValue(_target);
                if (fieldValue == null) {
                    var info = new HelpBox("Value is null (not initialized)", HelpBoxMessageType.Warning);
                    container.Add(info);
                    root.Add(container);
                    continue;
                }
                var list = fieldValue as dynamic;
                var items = new List<string>();
                foreach (var item in list.AsNativeArray()) {
                    items.Add(item.ToString());
                }
                var listView = new ListView {
                    itemsSource = items,
                    makeItem = () => new Label(),
                    bindItem = (ve, i) => (ve as TextElement).text = items[i],
                    selectionType = SelectionType.None,
                    fixedItemHeight = 18,
                    style = { height = Mathf.Min(items.Count * 18, 200) },
                    horizontalScrollingEnabled = true,
                    enabledSelf = false
                };
                container.Add(listView);
                root.Add(container);
            }
            return root;
        }

        #region 整数

        private static VisualElement BuildSByteField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new IntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => {
                var clampValue = Math.Clamp(evt.newValue, sbyte.MinValue, sbyte.MaxValue);
                fld.SetValueWithoutNotify(clampValue);
                nv.Value = (sbyte)clampValue;
            });
            return fld;
        }

        private static VisualElement BuildShortField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new IntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => {
                var clampValue = Math.Clamp(evt.newValue, short.MinValue, short.MaxValue);
                fld.SetValueWithoutNotify(clampValue);
                nv.Value = (short)clampValue;
            });
            return fld;
        }

        private static VisualElement BuildByteField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new UnsignedIntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => {
                var clampValue = Math.Clamp(evt.newValue, byte.MinValue, byte.MaxValue);
                fld.SetValueWithoutNotify(clampValue);
                nv.Value = (byte)clampValue;
            });
            return fld;
        }

        private static VisualElement BuildUShortField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new UnsignedIntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => {
                var clampValue = Math.Clamp(evt.newValue, ushort.MinValue, ushort.MaxValue);
                fld.SetValueWithoutNotify(clampValue);
                nv.Value = (ushort)clampValue;
            });
            return fld;
        }

        private static VisualElement BuildIntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new IntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildUIntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new UnsignedIntegerField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildLongField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new LongField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildULongField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new UnsignedLongField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        #endregion 整数

        #region 浮点 / 基本

        private static VisualElement BuildFloatField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new FloatField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildDoubleField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new DoubleField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildBoolField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Toggle(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildStringField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new TextField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        #endregion 浮点 / 基本

        #region 向量 / 四元数 / 颜色 / 矩形

        private static VisualElement BuildVector2Field(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Vector2Field(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildVector3Field(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Vector3Field(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildVector4Field(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Vector4Field(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildVector2IntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Vector2IntField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildVector3IntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Vector3IntField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildQuaternionField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var q = (Quaternion)nv.Value;
            var fld = new Vector4Field(label) { name = name, tooltip = tooltip, value = new Vector4(q.x, q.y, q.z, q.w), enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => { var v = evt.newValue; nv.Value = new Quaternion(v.x, v.y, v.z, v.w); });
            return fld;
        }

        private static VisualElement BuildColorField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new ColorField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildColor32Field(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new ColorField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = (Color32)evt.newValue);
            return fld;
        }

        private static VisualElement BuildBoundsField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new BoundsField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildBoundsIntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new BoundsIntField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildRectField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new RectField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildRectIntField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new RectIntField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        #endregion 向量 / 四元数 / 颜色 / 矩形

        #region 其它

        private static VisualElement BuildLayerMaskField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new LayerMaskField(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildHash128Field(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var fld = new Hash128Field(label) { name = name, tooltip = tooltip, value = nv.Value, enabledSelf = enabledSelf };
            fld.RegisterValueChangedCallback(evt => nv.Value = evt.newValue);
            return fld;
        }

        private static VisualElement BuildEnumField(string name, string label, string tooltip, bool enabledSelf, dynamic nv) {
            var enumValue = (Enum)nv.Value;
            if (enumValue.GetType().GetCustomAttribute<FlagsAttribute>() != null) {
                var fld = new EnumFlagsField(label, enumValue) { name = name, tooltip = tooltip, enabledSelf = enabledSelf };
                fld.RegisterValueChangedCallback(evt => nv.Value = (dynamic)evt.newValue);
                return fld;
            } else {
                var fld = new EnumField(label, enumValue) { name = name, tooltip = tooltip, enabledSelf = enabledSelf };
                fld.RegisterValueChangedCallback(evt => nv.Value = (dynamic)evt.newValue);
                return fld;
            }
        }

        #endregion 其它

        private void OnEnable() {
            if (_target == target) {
                return;
            }

            _target = target as NetworkBehaviour;
            _networkListFields.Clear();
            foreach (var field in _target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).AsSpan()) {
                var fieldType = field.FieldType;
                if (!fieldType.IsGenericType
                    || field.IsDefined(typeof(HideInInspector), true)
                    || field.IsDefined(typeof(NonSerializedAttribute), true)) {
                    continue;
                }
                var genericType = fieldType.GetGenericTypeDefinition();
                if (genericType == typeof(NetworkList<>)) {
                    _networkListFields.Add(field);
                } else if (genericType == typeof(NetworkVariable<>)) {
                    _networkVarFields.Add(field);
                }
            }
        }

        private delegate VisualElement BuildFieldFunc(string name, string label, string tooltip, bool enabledSelf, dynamic nv);
    }
}