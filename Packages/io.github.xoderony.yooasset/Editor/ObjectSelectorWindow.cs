#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace Xoderony.YooAsset.Editor {

    public class ObjectSelectorWindow : EditorWindow {
        private List<string> _assetPaths;
        private Type _assetType;
        private ObjectField _objectField;
        private ListView _listView;
        private Image _previewImage;
        private Label _previewLabel;
        private Texture2D _previewTexture;
        private UObject _previewObject;

        public static void Show(ObjectField objectField, List<string> assetPaths) {
            var win = GetWindow<ObjectSelectorWindow>(true);
            win.minSize = new Vector2(256, 256);
            win.titleContent = new GUIContent($"{objectField.objectType} 资源选择器");
            win._objectField = objectField;
            win._assetPaths = assetPaths;
            win._assetType = objectField.objectType;
            win._listView.itemsSource = assetPaths;
            win._previewObject = null;
            win._previewTexture = null;
            win._previewLabel.text = "请选择资源";
            win.RefreshPreviewImage(null);
            win.Focus();
        }

        protected void CreateGUI() {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            _listView = new ListView(_assetPaths, 24, MakeItem, BindItem) {
                selectionType = SelectionType.Single,
                style = { flexGrow = 1 },
            };
            _listView.selectionChanged += OnSelectionChanged;
            _listView.itemsChosen += OnItemsChosen;
            var bottom = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Column,
                    paddingTop = 6,
                    paddingLeft = 6,
                    paddingRight = 6,
                    alignItems = Align.Center,
                    flexShrink = 0,
                }
            };
            _previewImage = new Image { scaleMode = ScaleMode.ScaleToFit, };
            _previewLabel = new Label("请选择资源") { style = { marginTop = 6 } };
            bottom.Add(_previewImage);
            bottom.Add(_previewLabel);
            root.Add(_listView);
            root.Add(bottom);
        }

        protected void OnInspectorUpdate() {
            if (_previewTexture == null && _previewObject != null) {
                _previewTexture = AssetPreview.GetAssetPreview(_previewObject);
                RefreshPreviewImage(_previewTexture);
            }
        }

        private VisualElement MakeItem() {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var icon = new Image { name = "icon", scaleMode = ScaleMode.ScaleToFit, style = { width = 24, height = 24 } };
            var lab = new Label { name = "text", style = { marginLeft = 4, unityTextAlign = TextAnchor.MiddleLeft } };
            row.Add(icon);
            row.Add(lab);
            return row;
        }

        private void BindItem(VisualElement ve, int index) {
            var path = _assetPaths[index];
            ve.Q<Image>("icon").image = AssetPreview.GetMiniTypeThumbnail(_assetType);
            ve.Q<Label>("text").text = Path.GetFileNameWithoutExtension(path);
            ve.tooltip = path;
        }

        private void OnSelectionChanged(IEnumerable<object> selection) {
            var path = selection.FirstOrDefault() as string;
            _previewLabel.text = path;
            _previewObject = AssetDatabase.LoadAssetAtPath(path, _assetType);
            _previewTexture = AssetPreview.GetAssetPreview(_previewObject);
            RefreshPreviewImage(_previewTexture);
        }

        private void OnItemsChosen(IEnumerable<object> selection) {
            var path = selection.FirstOrDefault() as string;
            var asset = AssetDatabase.LoadAssetAtPath(path, _assetType);
            _objectField.value = asset;
            Close();
        }

        private void RefreshPreviewImage(Texture image) {
            if (image == null) {
                _previewImage.image = null;
                _previewImage.style.width = 0;
                _previewImage.style.height = 0;
            } else {
                _previewImage.image = image;
                _previewImage.style.width = image.width;
                _previewImage.style.height = image.height;
            }
        }
    }
}
#endif