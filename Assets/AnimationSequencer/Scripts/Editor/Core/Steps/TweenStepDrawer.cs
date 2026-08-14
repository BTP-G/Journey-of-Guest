#if DOTWEEN_ENABLED
using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Modified by Pablo Huaxteco
    [CustomPropertyDrawer(typeof(TweenStep))]
    public class TweenStepDrawer : AnimationStepBaseDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            DrawBaseGUI(position, property, label, "actions", "loopCount", "loopType");

            var originHeight = position.y;
            if (property.isExpanded) {
                if (EditorGUI.indentLevel > 0) {
                    position = EditorGUI.IndentedRect(position);
                }

                var flowTypeSerializedProperty = property.FindPropertyRelative("flowType");
                var flowType = (FlowType)flowTypeSerializedProperty.enumValueIndex;
                if (flowType == FlowType.Join) {
                    EditorGUI.indentLevel++;
                    position = EditorGUI.IndentedRect(position);
                    EditorGUI.indentLevel--;
                }

                position.y += base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginChangeCheck();
                var actionsSerializedProperty = property.FindPropertyRelative("actions");
                var targetSerializedProperty = property.FindPropertyRelative("target");
                var loopCountSerializedProperty = property.FindPropertyRelative("loopCount");
                EditorGUI.PropertyField(position, loopCountSerializedProperty);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                loopCountSerializedProperty.intValue = Mathf.Clamp(loopCountSerializedProperty.intValue, -1, int.MaxValue);
                if (loopCountSerializedProperty.intValue != 0) {
                    if (loopCountSerializedProperty.intValue == -1) {
                        Debug.LogWarning("Infinity Loops doesn't work well with sequence, the best way of doing " +
                                         "that is setting to the int.MaxValue, will end eventually, but will take a really " +
                                         "long time, more info here: https://github.com/Demigiant/dotween/issues/92");
                        loopCountSerializedProperty.intValue = int.MaxValue;
                    }
                    var loopTypeSerializedProperty = property.FindPropertyRelative("loopType");
                    EditorGUI.PropertyField(position, loopTypeSerializedProperty);
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUIUtility.singleLineHeight * 1.15f;
                var originalWidth = position.width;
                var actionsFoldoutPosition = position;
                actionsFoldoutPosition.x += 10;
                actionsFoldoutPosition.width = EditorGUIUtility.labelWidth - 10;
                actionsSerializedProperty.isExpanded = EditorGUI.Foldout(actionsFoldoutPosition, actionsSerializedProperty.isExpanded, "Actions", true, EditorStyles.foldout);

                position.x += EditorGUIUtility.labelWidth;
                position.width = originalWidth - EditorGUIUtility.labelWidth;
                if (GUI.Button(position, "+")) {
                    try {
                        AnimationSequencerEditorGUIUtility.TweenActionsDropdown.Show(position, actionsSerializedProperty, targetSerializedProperty.objectReferenceValue,
                        item => {
                            if (AnimationSequencerEditorGUIUtility.TweenActionsDropdown.IsTypeAlreadyInUse(actionsSerializedProperty, item.BaseTweenActionType)) {
                                Debug.Log($"The '{item.name}' action already exists in this step.");
                            } else {
                                AddNewActionOfType(actionsSerializedProperty, item.BaseTweenActionType);
                            }
                        });
                    } catch (Exception ex) {
                        Debug.Log($"Unexpected error: {ex}");
                    }
                }
                position.x -= EditorGUIUtility.labelWidth;
                position.width = originalWidth;
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                var normalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 112;
                if (actionsSerializedProperty.isExpanded) {
                    var arraySize = actionsSerializedProperty.arraySize;
                    for (var i = 0; i < arraySize; i++) {
                        if (DrawDeleteActionButton(position, property, i)) {
                            var actionSerializedProperty = actionsSerializedProperty.GetArrayElementAtIndex(i);

                            var guiEnabled = GUI.enabled;

                            if (GUI.enabled) {
                                var isValidTargetForRequiredComponent = IsValidTargetForRequiredComponent(targetSerializedProperty, actionSerializedProperty);
                                GUI.enabled = isValidTargetForRequiredComponent;
                            }

                            var wasExpanded = actionSerializedProperty.isExpanded;
                            float heightToRest = 0;
                            EditorGUI.PropertyField(position, actionSerializedProperty);

                            // Verify only one action is expanded.
                            if (AnimationSequencerPreferences.GetInstance().OnlyOneActionExpandedWhileEditing) {
                                if (actionSerializedProperty.isExpanded && !wasExpanded) {
                                    for (var actionIndex = 0; actionIndex < arraySize; actionIndex++) {
                                        if (actionIndex != i) {
                                            var actionProperty = actionsSerializedProperty.GetArrayElementAtIndex(actionIndex);
                                            if (actionProperty.isExpanded) {
                                                if (i > actionIndex) {
                                                    heightToRest = actionProperty.GetPropertyDrawerHeight() - 26;
                                                }

                                                actionProperty.isExpanded = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            position.y += actionSerializedProperty.GetPropertyDrawerHeight() - heightToRest;

                            if (i < arraySize - 1) {
                                position.y += EditorGUIUtility.standardVerticalSpacing;
                            }

                            GUI.enabled = guiEnabled;
                        } else {
                            i--;
                            arraySize--;
                        }
                    }
                }
                EditorGUIUtility.labelWidth = normalLabelWidth;

                if (EditorGUI.EndChangeCheck()) {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            property.SetPropertyDrawerHeight(position.y - originHeight + (property.isExpanded ? 0 : EditorGUIUtility.singleLineHeight));
        }

        private void AddNewActionOfType(SerializedProperty actionsSerializedProperty, Type targetType) {
            actionsSerializedProperty.arraySize++;
            var newElement = actionsSerializedProperty.GetArrayElementAtIndex(actionsSerializedProperty.arraySize - 1);
            newElement.managedReferenceValue = Activator.CreateInstance(targetType);

            SerializedProperty SetDirection(SerializedProperty element, SerializedProperty previousElement = null) {
                var direction = element.FindPropertyRelative("direction");
                if (direction == null) {
                    return null;
                }

                direction.enumValueIndex = previousElement != null && AnimationSequencerDefaults.Instance.UsePreviousDirection
                    ? previousElement.FindPropertyRelative("direction").enumValueIndex
                    : (int)AnimationSequencerDefaults.Instance.Direction;

                return direction;
            }

            SerializedProperty SetEase(SerializedProperty element, SerializedProperty previousElement = null) {
                var ease = element.FindPropertyRelative("ease").FindPropertyRelative("ease");
                if (ease == null) {
                    return null;
                }

                if (previousElement != null && AnimationSequencerDefaults.Instance.UsePreviousEase) {
                    var previousEase = previousElement.FindPropertyRelative("ease").FindPropertyRelative("ease");
                    ease.enumValueIndex = previousEase.enumValueIndex;

                    if (ease.enumValueIndex == (int)Ease.INTERNAL_Custom) {
                        var previousCurve = previousElement.FindPropertyRelative("ease").FindPropertyRelative("curve");
                        element.FindPropertyRelative("ease").FindPropertyRelative("curve").animationCurveValue = previousCurve.animationCurveValue;
                    }
                } else {
                    ease.enumValueIndex = (int)AnimationSequencerDefaults.Instance.Ease.Ease;
                }

                return ease;
            }

            if (actionsSerializedProperty.arraySize > 1) {
                var previousElement = actionsSerializedProperty.GetArrayElementAtIndex(actionsSerializedProperty.arraySize - 2);
                SetDirection(newElement, previousElement);
                SetEase(newElement, previousElement);
            } else {
                SetDirection(newElement);
                SetEase(newElement);
            }

            actionsSerializedProperty.isExpanded = true;
            if (AnimationSequencerPreferences.GetInstance().OnlyOneActionExpandedWhileEditing) {
                var actionsCount = actionsSerializedProperty.arraySize;
                for (var i = 0; i < actionsCount - 1; i++) {
                    actionsSerializedProperty.GetArrayElementAtIndex(i).isExpanded = false;
                }
            }
            newElement.isExpanded = true;
            actionsSerializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private static bool IsValidTargetForRequiredComponent(SerializedProperty targetSerializedProperty, SerializedProperty actionSerializedProperty) {
            if (targetSerializedProperty.objectReferenceValue == null) {
                return false;
            }

            var type = actionSerializedProperty.GetTypeFromManagedFullTypeName();
            return AnimationSequencerEditorGUIUtility.CanActionBeAppliedToTarget(type, targetSerializedProperty.objectReferenceValue as GameObject);
        }

        private bool DrawDeleteActionButton(Rect position, SerializedProperty property, int targetIndex) {
            var buttonPosition = position;
            buttonPosition.width = 24;
            buttonPosition.x += position.width - 34;
            buttonPosition.y += 4;

            if (GUI.Button(buttonPosition, "X", EditorStyles.miniButton)) {
                DeleteElementAtIndex(property, targetIndex);
                return false;
            }

            return true;
        }

        private void DeleteElementAtIndex(SerializedProperty serializedProperty, int targetIndex) {
            var actionsPropertyPath = serializedProperty.FindPropertyRelative("actions");
            actionsPropertyPath.DeleteArrayElementAtIndex(targetIndex);
            SerializedPropertyExtensions.ClearPropertyCache(actionsPropertyPath.propertyPath);
            //actionsPropertyPath.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return property.GetPropertyDrawerHeight();
        }
    }
}
#endif
