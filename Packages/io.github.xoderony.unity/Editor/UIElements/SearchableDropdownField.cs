using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xoderony.UIElements {

    public class SearchableDropdownField : VisualElement {
        private readonly TextField _textField;
        private readonly TextField _searchField;
        private readonly ListView _listView;
        private readonly IEnumerable<string> _options;
        private readonly List<string> _filteredOptions;
        private readonly VisualElement _dropdownContainer;

        public string Value {
            get => _textField.value;
            set {
                _textField.value = value;
                var index = _filteredOptions.IndexOf(value);
                _listView.AddToSelection(index);
            }
        }

        public event Action<string> OnValueChanged;

        public SearchableDropdownField(string label, IEnumerable<string> options) {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _filteredOptions = new List<string>(_options);

            _textField = new TextField(label) {
                isReadOnly = true,
                style = { flexGrow = 1 }
            };
            var arrow = new Button(ToggleDropdown) {
                text = "▼",
                style = {
                    width = 20,
                    height = 20,
                    marginRight = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            var inputRow = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                }
            };
            inputRow.Add(_textField);
            inputRow.Add(arrow);
            Add(inputRow);

            _searchField = new TextField {
                multiline = false,
                style = {
                    marginTop = 2,
                    marginBottom = 2,
                    marginLeft = 4,
                    marginRight = 4,
                }
            };
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            _listView = new ListView(_filteredOptions, -1, MakeItem, BindItem) {
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                selectionType = SelectionType.Single,
            };
            _listView.itemsChosen += OnItemsChosen;

            _dropdownContainer = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Column,
                    maxHeight = 300,
                    display = DisplayStyle.None,
                }
            };
            _dropdownContainer.AddToClassList("unity-popup-window");
            _dropdownContainer.Add(_searchField);
            _dropdownContainer.Add(_listView);
            Add(_dropdownContainer);
        }

        private void ToggleDropdown() {
            if (_dropdownContainer.style.display == DisplayStyle.Flex) {
                CloseDropdown();
            } else {
                OpenDropdown();
            }
        }

        private void OpenDropdown() {
            _dropdownContainer.style.display = DisplayStyle.Flex;
            _searchField.value = string.Empty;
            _searchField.Focus();
        }

        private void CloseDropdown() {
            _dropdownContainer.style.display = DisplayStyle.None;
        }

        private void OnSearchChanged(ChangeEvent<string> evt) {
            _filteredOptions.Clear();
            if (string.IsNullOrWhiteSpace(evt.newValue)) {
                _filteredOptions.AddRange(_options);
            } else {
                foreach (var key in _options) {
                    if (key.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase)) {
                        _filteredOptions.Add(key);
                    }
                }
            }
            _listView.RefreshItems();
        }

        private VisualElement MakeItem() {
            return new Label {
                style = {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 4,
                }
            };
        }

        private void BindItem(VisualElement element, int index) {
            if (element is Label label && (uint)index < (uint)_filteredOptions.Count) {
                label.text = _filteredOptions[index];
            }
        }

        private void OnItemsChosen(IEnumerable<object> selections) {
            if (selections.FirstOrDefault() is string selected) {
                _textField.value = selected;
                OnValueChanged?.Invoke(selected);
                CloseDropdown();
            }
        }
    }
}