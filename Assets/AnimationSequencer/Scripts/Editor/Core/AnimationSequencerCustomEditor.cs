#if DOTWEEN_ENABLED
using DG.DOTweenEditor;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Modified by Pablo Huaxteco
    [CustomEditor(typeof(AnimationSequencer), true)]
    public class AnimationSequencerCustomEditor : Editor {
        #region Variables
        // Static variables and properties 
        private static AnimationStepAdvancedDropdown cachedAnimationStepsDropdown;
        private static AnimationStepAdvancedDropdown AnimationStepAdvancedDropdown {
            get {
                cachedAnimationStepsDropdown ??= new AnimationStepAdvancedDropdown(new AdvancedDropdownState());

                return cachedAnimationStepsDropdown;
            }
        }

        // Private variables
        private AnimationSequencer sequencerController;
        private ReorderableList reorderableList;
        private GUIStyle topRightTextStyle;
        private StepAnimationData[] stepsAnimationData;
        private Dictionary<int, float> collapsedStepValues = new Dictionary<int, float>();
        private Dictionary<int, List<bool>> expandedActionStates = new Dictionary<int, List<bool>>();
        private SerializedProperty playbackSpeedProperty;
        private SerializedProperty autoPlayModeSerializedProperty;
        private SerializedProperty autoKillSerializedProperty;
        private MethodInfo reorderableListClearCacheMethod = null;
        private bool cantFindReorderableListClearCacheMethod;
        private bool showPreviewPanel = true;
        private bool showSettingsPanel;
        private bool showCallbacksPanel;
        private bool showStepsPanel;
        private float tweenTimeScale = 1f;
        private bool wasShowingStepsPanel;
        private bool justStartPreviewing;
        private float mainSequenceDuration = 0;
        private bool actionsValuesTaken = false;
        private int lastExpandedStepIndex = -1;
        private bool lastExpandedStepChanged;
        private bool onlyOneActionExpandedEnable;
        private bool isPlayDirectionSet;
        private bool isPlayDirectionForward;
        #endregion

        #region OnEnable/OnDisable settings
        private void OnEnable() {
            InitializeReferences();
            InitializeReorderableList();
            SubscribeToEditorEvents();
            Repaint();
        }

        private void OnDisable() {
            UnsubscribeFromEditorEvents();
            ResetEditorState();
        }

        private void InitializeReferences() {
            sequencerController = target as AnimationSequencer;
            playbackSpeedProperty = serializedObject.FindProperty("playbackSpeed");
            autoPlayModeSerializedProperty = serializedObject.FindProperty("autoplayMode");
            autoKillSerializedProperty = serializedObject.FindProperty("autoKill");
        }

        private void InitializeReorderableList() {
            reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("animationSteps"), true, false, true, true);
            reorderableList.drawElementBackgroundCallback += OnDrawAnimationStepBackground;
            reorderableList.drawElementCallback += OnDrawAnimationStep;
            reorderableList.elementHeightCallback += GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback += OnClickToAddNewAnimationStep;
            reorderableList.onRemoveCallback += OnClickToRemoveAnimationStep;
            reorderableList.onReorderCallbackWithDetails += OnAnimationStepListOrderChanged;
            reorderableList.onMouseDragCallback += OnMouseDragAnimationStep;
            reorderableList.onMouseUpCallback += OnMouseUpAnimationStep;
        }

        private void SubscribeToEditorEvents() {
            EditorApplication.playModeStateChanged += OnEditorPlayModeChanged;

#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving += PrefabSaving;
#endif
        }

        private void UnsubscribeFromEditorEvents() {
            //Animation step reorderableList events.
            reorderableList.drawElementBackgroundCallback -= OnDrawAnimationStepBackground;
            reorderableList.drawElementCallback -= OnDrawAnimationStep;
            reorderableList.elementHeightCallback -= GetAnimationStepHeight;
            reorderableList.onAddDropdownCallback -= OnClickToAddNewAnimationStep;
            reorderableList.onRemoveCallback -= OnClickToRemoveAnimationStep;
            reorderableList.onReorderCallbackWithDetails -= OnAnimationStepListOrderChanged;
            reorderableList.onMouseDragCallback -= OnMouseDragAnimationStep;
            reorderableList.onMouseUpCallback -= OnMouseUpAnimationStep;

            //Other events.
            EditorApplication.playModeStateChanged -= OnEditorPlayModeChanged;

#if UNITY_2021_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage.prefabSaving -= PrefabSaving;
#endif
        }

        private void ResetEditorState() {
            if (!Application.isPlaying) {
                StopSequence();
            }

            tweenTimeScale = 1f;
            SerializedPropertyExtensions.ClearPropertyCache();
        }
        #endregion

        #region CustomEditor methods
        // Used to update the progress bar in the editor.
        public override bool RequiresConstantRepaint() {
            return Application.isPlaying || DOTweenEditorPreview.isPreviewing;
        }

        public override void OnInspectorGUI() {
            //Defaults.
            if (sequencerController.IsResetRequired()) {
                SetDefaults();
            }

            //Styles.
            InitializeStyles();

            //Foldout areas.
            DrawFoldoutArea("Preview", ref showPreviewPanel, DrawPreviewControls, DrawExtraPreviewHeader);
            DrawFoldoutArea("Settings", ref showSettingsPanel, DrawSettings, DrawExtraSettingsHeader);
            DrawFoldoutArea("Callbacks", ref showCallbacksPanel, DrawCallbacks);
            SerializedProperty animationStepsProperty = null;
            if (!DOTweenEditorPreview.isPreviewing) {
                animationStepsProperty = reorderableList.serializedProperty;
                showStepsPanel = animationStepsProperty.isExpanded;
            }
            DrawFoldoutArea("Steps", ref showStepsPanel, DrawAnimationSteps);
            if (animationStepsProperty != null && !DOTweenEditorPreview.isPreviewing) {
                animationStepsProperty.isExpanded = showStepsPanel;
            }

            // Verify only one step is expanded.
            if (AnimationSequencerPreferences.GetInstance().OnlyOneStepExpandedWhileEditing && showStepsPanel && !DOTweenEditorPreview.isPreviewing) {
                CheckOnlyOneStepExpanded();
            }

            // Verify only one action is expanded when "OnlyOneActionExpandedWhileEditing" is enabled.
            CollapseAllActions();
        }

        private void InitializeStyles() {
            topRightTextStyle ??= new GUIStyle(GUI.skin.label) {
                fontSize = 11,
                normal = { textColor = new Color(0.1f, 0.1f, 0.1f) }
            };
        }
        #endregion

        #region Sequencer defaults
        private void SetDefaults() {
            sequencerController = target as AnimationSequencer;

            if (sequencerController != null) {
                sequencerController.AutoplayMode = AnimationSequencerDefaults.Instance.AutoplayMode;
                sequencerController.StartPaused = AnimationSequencerDefaults.Instance.StartPaused;
                sequencerController.TimeScaleIndependent = AnimationSequencerDefaults.Instance.TimeScaleIndependent;
                sequencerController.PlayType = AnimationSequencerDefaults.Instance.PlayType;
                sequencerController.UpdateType = AnimationSequencerDefaults.Instance.UpdateType;
                sequencerController.Autokill = AnimationSequencerDefaults.Instance.AutoKill;
                sequencerController.Loops = AnimationSequencerDefaults.Instance.Loops;
                sequencerController.LoopType = AnimationSequencerDefaults.Instance.LoopType;
                sequencerController.DynamicStartValues = AnimationSequencerDefaults.Instance.DynamicStartValues;
                sequencerController.ResetComplete();
            }
        }
        #endregion

        #region Preview panel
        private void DrawExtraPreviewHeader(Rect rect) {
            //Draw sequence duration.
            if (sequencerController.PlayingSequence != null) {
                var duration = (sequencerController.PlayingSequence.Duration() - sequencerController.ExtraIntervalAdded) * (1 / playbackSpeedProperty.floatValue);
                DrawTopRightText(rect, $"Duration: {NumberFormatter.FormatDecimalPlaces(duration)}s", new Color(0f, 1f, 0f, 0.5f));
            }
        }

        private void DrawPreviewControls() {
            DrawMediaPlayerControlButtons();
            DrawTimeScaleSlider();
            DrawProgressSlider();
        }

        private void DrawMediaPlayerControlButtons() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var guiEnabled = GUI.enabled;

            var previewButtonStyle = new GUIStyle(GUI.skin.button);
            previewButtonStyle.fixedWidth = previewButtonStyle.fixedHeight = 32;

            if (GUILayout.Button(AnimationSequencerEditorGUIUtility.RewindButtonGUIContent, previewButtonStyle)) {
                Rewind();
            }

            if (GUILayout.Button(AnimationSequencerEditorGUIUtility.StepBackGUIContent, previewButtonStyle)) {
                StepBack();
            }

            // Play forward button
            if (sequencerController.IsPlaying && !sequencerController.PlayingSequence.IsBackwards()) {
                if (GUILayout.Button(AnimationSequencerEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle)) {
                    sequencerController.PlayingSequence.Pause();
                }
            } else {
                if (GUILayout.Button(AnimationSequencerEditorGUIUtility.PlayForwardButtonGUIContent, previewButtonStyle)) {
                    PlaySequenceForward();
                }
            }

            // Play backward button
            if (sequencerController.IsPlaying && sequencerController.PlayingSequence.IsBackwards()) {
                if (GUILayout.Button(AnimationSequencerEditorGUIUtility.PauseButtonGUIContent, previewButtonStyle)) {
                    sequencerController.PlayingSequence.Pause();
                }
            } else {
                if (GUILayout.Button(AnimationSequencerEditorGUIUtility.PlayBackwardsButtonGUIContent, previewButtonStyle)) {
                    PlaySequenceBackwards();
                }
            }

            if (GUILayout.Button(AnimationSequencerEditorGUIUtility.StepNextGUIContent, previewButtonStyle)) {
                StepNext();
            }

            if (GUILayout.Button(AnimationSequencerEditorGUIUtility.ForwardButtonGUIContent, previewButtonStyle)) {
                CompleteForward();
            }

            if (!Application.isPlaying) {
                GUI.enabled = DOTweenEditorPreview.isPreviewing;
                if (GUILayout.Button(AnimationSequencerEditorGUIUtility.StopButtonGUIContent, previewButtonStyle)) {
                    StopSequence();
                }
            }

            GUI.enabled = guiEnabled;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimeScaleSlider() {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            var normalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenTimeScale = EditorGUILayout.Slider("TimeScale", tweenTimeScale, 0, 2);
            EditorGUIUtility.labelWidth = normalLabelWidth;

            UpdateSequenceTimeScale();

            GUILayout.FlexibleSpace();
        }

        private void DrawProgressSlider() {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            var tweenProgress = GetProgress();

            var normalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            tweenProgress = EditorGUILayout.Slider("Progress", tweenProgress, 0, 1);
            EditorGUIUtility.labelWidth = normalLabelWidth;

            if (EditorGUI.EndChangeCheck()) {
                SetProgress(tweenProgress);
            }

            GUILayout.FlexibleSpace();
        }
        #endregion

        #region Settings panel
        private void DrawExtraSettingsHeader(Rect rect) {
            //Draw auto kill value.
            rect = DrawTopRightText(rect, $"Auto kill: {autoKillSerializedProperty.boolValue}",
                EditorGUIUtility.isProSkin ? new Color(1f, 0.2f, 0f, 0.5f) : new Color(1f, 0.2f, 0f, 0.7f));

            //Draw auto play mode.
            var autoplayMode = (AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
            var label = "";
            switch (autoplayMode) {
                case AutoplayType.Nothing:
                    label = "Off";
                    break;
                case AutoplayType.Start:
                    label = "on Start";
                    break;
                case AutoplayType.OnEnable:
                    label = "on Enable";
                    break;
            }

            DrawTopRightText(rect, $"Autoplay {label}", EditorGUIUtility.isProSkin ? new Color(1f, 0.7f, 0f, 0.5f) : new Color(1f, 0.7f, 0f, 0.7f));
        }

        private void DrawSettings() {
            var wasEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing) {
                GUI.enabled = false;
            }

            var pauseOnStartSerializedProperty = serializedObject.FindProperty("startPaused");
            var timeScaleIndependentSerializedProperty = serializedObject.FindProperty("timeScaleIndependent");
            var sequenceDirectionSerializedProperty = serializedObject.FindProperty("playType");
            var updateTypeSerializedProperty = serializedObject.FindProperty("updateType");
            var dynamicStartValuesSerializedProperty = serializedObject.FindProperty("dynamicStartValues");
            var loopsSerializedProperty = serializedObject.FindProperty("loops");
            var loopTypeSerializedProperty = serializedObject.FindProperty("loopType");

            using (var changedCheck = new EditorGUI.ChangeCheckScope()) {
                var autoplayMode = (AutoplayType)autoPlayModeSerializedProperty.enumValueIndex;
                EditorGUILayout.PropertyField(autoPlayModeSerializedProperty);
                if (autoplayMode != AutoplayType.Nothing) {
                    EditorGUILayout.PropertyField(pauseOnStartSerializedProperty);
                }

                EditorGUILayout.PropertyField(sequenceDirectionSerializedProperty);
                EditorGUILayout.PropertyField(updateTypeSerializedProperty);
                DrawPlaybackSpeedSlider();
                EditorGUILayout.PropertyField(timeScaleIndependentSerializedProperty);
                EditorGUILayout.PropertyField(dynamicStartValuesSerializedProperty);
                EditorGUILayout.PropertyField(loopsSerializedProperty);
                if (loopsSerializedProperty.intValue != 0) {
                    EditorGUILayout.PropertyField(loopTypeSerializedProperty);
                }

                EditorGUILayout.PropertyField(autoKillSerializedProperty);

                if (changedCheck.changed) {
                    loopsSerializedProperty.intValue = Mathf.Clamp(loopsSerializedProperty.intValue, -1, int.MaxValue);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUI.enabled = wasEnabled;
        }

        private void DrawPlaybackSpeedSlider() {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            var playbackSpeedLabel = new GUIContent("Playback Speed", "Speed of the animation playback.");
            playbackSpeedProperty.floatValue = EditorGUILayout.Slider(playbackSpeedLabel, playbackSpeedProperty.floatValue, 0, 2);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                UpdateSequenceTimeScale();
            }

            GUILayout.FlexibleSpace();
        }

        private void UpdateSequenceTimeScale() {
            if (sequencerController.PlayingSequence == null) {
                return;
            }

            sequencerController.PlayingSequence.timeScale = sequencerController.PlaybackSpeed * tweenTimeScale;
        }
        #endregion

        #region Callbacks panel
        protected virtual void DrawCallbacks() {
            var wasGUIEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing) {
                GUI.enabled = false;
            }

            var onAnimationStartSerializedProperty = serializedObject.FindProperty("onAnimationStart");
            var onAnimationProgressSerializedProperty = serializedObject.FindProperty("onAnimationProgress");
            var onAnimationFinishSerializedProperty = serializedObject.FindProperty("onAnimationFinish");

            using (var changedCheck = new EditorGUI.ChangeCheckScope()) {
                EditorGUILayout.PropertyField(onAnimationStartSerializedProperty);
                EditorGUILayout.PropertyField(onAnimationProgressSerializedProperty);
                EditorGUILayout.PropertyField(onAnimationFinishSerializedProperty);

                if (changedCheck.changed) {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUI.enabled = wasGUIEnabled;
        }
        #endregion

        #region Steps panel
        private void DrawAnimationSteps() {
            var wasGUIEnabled = GUI.enabled;
            if (DOTweenEditorPreview.isPreviewing) {
                GUI.enabled = false;
            }

            reorderableList.DoLayoutList();

            GUI.enabled = wasGUIEnabled;
        }

        private void OnDrawAnimationStepBackground(Rect rect, int index, bool isActive, bool isFocused) {
            if (index == -1) {
                return;
            }

            if (Event.current.type != EventType.Repaint) {
                return;
            }

            //Title rect.
            var titleRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };
            if (isActive) {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, isFocused, false);
            } else {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(0.1f, 0.1f, 0.1f));
                GUI.skin.box.Draw(titleRect, false, false, false, false);
            }

            //Step progress preview.
            if (AnimationSequencerPreferences.GetInstance().VisualizeStepsProgressWhenPreviewing && DOTweenEditorPreview.isPreviewing) {
                rect.y += 1;
                var animationData = stepsAnimationData[index];
                var barColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.38f, 0.1f, 1f) : new Color(0.7f, 0.58f, 0.3f, 1f);
                var progressColor = EditorGUIUtility.isProSkin ? new Color(0.1f, 0.4f, 0.1f, 1f) : new Color(0.4f, 0.6f, 0.3f, 1f);
                var barRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };

                if (animationData == null) {
                    barColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.1f, 0.1f, 1f) : new Color(0.7f, 0.3f, 0.3f, 1f);
                    EditorGUI.DrawRect(barRect, barColor);
                } else {
                    var startTime = animationData.startTime / mainSequenceDuration;
                    var endTime = animationData.endTime / mainSequenceDuration;
                    float extraWidth = (endTime - startTime < 0.01f) ? 1 : 0;

                    barRect.xMin = Mathf.Lerp(rect.xMin, rect.xMax, startTime) - extraWidth;
                    barRect.xMax = Mathf.Lerp(rect.xMin, rect.xMax, endTime) + extraWidth;

                    var isForward = !sequencerController.PlayingSequence.IsBackwards();
                    var loops = sequencerController.Loops;
                    var progress = GetProgress();
                    if (progress < 1 && loops > 1) {
                        progress = progress * loops % 1;
                    }

                    var showProgress = isForward ? progress >= startTime : progress <= endTime;
                    var progressRect = new Rect(rect);
                    if (showProgress) {
                        var interpolation_xMin = isForward ? startTime : Mathf.Clamp(progress, startTime, endTime);
                        var interpolation_xMax = isForward ? Mathf.Clamp(progress, startTime, endTime) : endTime;

                        progressRect.xMin = Mathf.Lerp(rect.xMin, rect.xMax, interpolation_xMin) - extraWidth;
                        progressRect.xMax = Mathf.Lerp(rect.xMin, rect.xMax, interpolation_xMax) + extraWidth;
                        progressRect.height = EditorGUIUtility.singleLineHeight;
                    }

                    EditorGUI.DrawRect(barRect, barColor);
                    if (showProgress) {
                        EditorGUI.DrawRect(progressRect, progressColor);
                    }
                }
            }
        }

        private void OnDrawAnimationStep(Rect rect, int index, bool isActive, bool isFocused) {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var flowTypeSerializedProperty = element.FindPropertyRelative("flowType");

            var flowType = (FlowType)flowTypeSerializedProperty.enumValueIndex;

            var baseIdentLevel = EditorGUI.indentLevel;

            var guiContent = new GUIContent(element.displayName);
            AnimationStepBase animationStepBase = null;
            try { animationStepBase = sequencerController.AnimationSteps[index]; } catch (Exception) { }
            if (animationStepBase != null) {
                var animationInfo = "";
                if (AnimationSequencerPreferences.GetInstance().VisualizeStepsProgressWhenPreviewing && DOTweenEditorPreview.isPreviewing) {
                    var stepAnimation = stepsAnimationData[index];
                    animationInfo = stepAnimation == null ? "Unused Step" : stepAnimation.info;
                }

                guiContent = new GUIContent(animationStepBase.GetDisplayNameForEditor(index + 1), animationInfo);
            }

            if (flowType == FlowType.Join) {
                EditorGUI.indentLevel = baseIdentLevel + 1;
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 10;
            rect.width -= 10;

            EditorGUI.LabelField(rect, guiContent);
            EditorGUI.PropertyField(rect, element, new GUIContent(""), false);

            EditorGUI.indentLevel = baseIdentLevel;
            // DrawContextInputOnItem(element, index, rect);

            // Verify if the last expanded step changed.
            if (AnimationSequencerPreferences.GetInstance().OnlyOneStepExpandedWhileEditing && showStepsPanel && !DOTweenEditorPreview.isPreviewing) {
                if (element.isExpanded && index != lastExpandedStepIndex) {
                    lastExpandedStepChanged = true;
                }
            }
        }

        private float GetAnimationStepHeight(int index) {
            if (index > reorderableList.serializedProperty.arraySize - 1) {
                return EditorGUIUtility.singleLineHeight;
            }

            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            return element.GetPropertyDrawerHeight();
        }

        private void OnClickToAddNewAnimationStep(Rect buttonRect, ReorderableList list) {
            AnimationStepAdvancedDropdown.Show(buttonRect, OnNewAnimationStepTypeSelected);
        }

        private void OnNewAnimationStepTypeSelected(AnimationStepAdvancedDropdownItem animationStepAdvancedDropdownItem) {
            AddNewAnimationStepOfType(animationStepAdvancedDropdownItem.AnimationStepType);
        }

        private void AddNewAnimationStepOfType(Type targetAnimationType) {
            var animationStepsProperty = reorderableList.serializedProperty;
            var targetIndex = animationStepsProperty.arraySize;
            animationStepsProperty.InsertArrayElementAtIndex(targetIndex);
            var arrayElementAtIndex = animationStepsProperty.GetArrayElementAtIndex(targetIndex);
            var managedReferenceValue = Activator.CreateInstance(targetAnimationType);
            arrayElementAtIndex.managedReferenceValue = managedReferenceValue;
            arrayElementAtIndex.isExpanded = true;

            //TODO copy from last step would be better here.
            if (targetAnimationType != typeof(SetActiveStep) && targetAnimationType != typeof(PlaySequenceStep)) {
                var targetSerializedProperty = arrayElementAtIndex.FindPropertyRelative("target");
                if (targetSerializedProperty != null) {
                    targetSerializedProperty.objectReferenceValue = (serializedObject.targetObject as AnimationSequencer)?.gameObject;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnClickToRemoveAnimationStep(ReorderableList list) {
            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(list.index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
        }

        private void OnAnimationStepListOrderChanged(ReorderableList list, int oldIndex, int newIndex) {
            var isCyclicRotationRight = true;
            var greatestIndex = oldIndex;
            var smallestIndex = newIndex;
            if (newIndex > oldIndex) {
                isCyclicRotationRight = false;
                greatestIndex = newIndex;
                smallestIndex = oldIndex;
            }

            var startIndex = isCyclicRotationRight ? greatestIndex : smallestIndex;
            var count = greatestIndex - smallestIndex + 1;
            var firstHeight = reorderableList.serializedProperty.GetArrayElementAtIndex(startIndex).GetPropertyDrawerHeight();
            var isFirstExpanded = firstHeight > 18;
            var currentIndex = startIndex;
            int actionsIndex;

            for (var i = 0; i < count; i++) {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(currentIndex);

                if (i == count - 1) {
                    element.SetPropertyDrawerHeight(firstHeight);
                    element.isExpanded = isFirstExpanded;
                    actionsIndex = startIndex;
                } else {
                    var nextIndex = isCyclicRotationRight ? currentIndex - 1 : currentIndex + 1;
                    var nextHeight = reorderableList.serializedProperty.GetArrayElementAtIndex(nextIndex).GetPropertyDrawerHeight();
                    element.SetPropertyDrawerHeight(nextHeight);
                    element.isExpanded = nextHeight > 18;
                    actionsIndex = nextIndex;
                }

                if (TryGetActionsExpandedStates(actionsIndex, out var actionsExpandedStates)) {
                    var actionsSerializedProperty = element.FindPropertyRelative("actions");
                    var isTweenStepExpanded = actionsExpandedStates[0];
                    actionsSerializedProperty.isExpanded = isTweenStepExpanded;

                    if (isTweenStepExpanded) {
                        for (var actionIndex = 0; actionIndex < actionsSerializedProperty.arraySize; actionIndex++) {
                            actionsSerializedProperty.GetArrayElementAtIndex(actionIndex).isExpanded = actionsExpandedStates[actionIndex + 1];
                        }
                    }
                }

                if (isCyclicRotationRight) {
                    currentIndex--;
                } else {
                    currentIndex++;
                }
            }

            actionsValuesTaken = false;
            expandedActionStates.Clear();

            SerializedPropertyExtensions.ClearPropertyCache(list.serializedProperty.propertyPath);
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetActionsExpandedStates(int index, out List<bool> actionsExpandedStates) {
            if (expandedActionStates.ContainsKey(index)) {
                actionsExpandedStates = expandedActionStates[index];
                return true;
            }

            actionsExpandedStates = null;
            return false;
        }

        private void OnMouseDragAnimationStep(ReorderableList list) {
            SaveActionsExpandedStates();
        }

        private void SaveActionsExpandedStates() {
            if (actionsValuesTaken) {
                return;
            }

            expandedActionStates.Clear();
            var reorderableListCount = reorderableList.serializedProperty.arraySize;
            for (var i = 0; i < reorderableListCount; i++) {
                var isTweenStep = sequencerController.AnimationSteps[i].GetType() == typeof(TweenStep);
                if (isTweenStep) {
                    var actionsExpandedStates = new List<bool>();
                    var actionsSerializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("actions");
                    var isTweenStepExpanded = actionsSerializedProperty.isExpanded;
                    actionsExpandedStates.Add(isTweenStepExpanded);

                    if (isTweenStepExpanded) {
                        var actionsCount = actionsSerializedProperty.arraySize;
                        for (var actionIndex = 0; actionIndex < actionsCount; actionIndex++) {
                            actionsExpandedStates.Add(actionsSerializedProperty.GetArrayElementAtIndex(actionIndex).isExpanded);
                        }
                    }

                    expandedActionStates.Add(i, actionsExpandedStates);
                }
            }

            actionsValuesTaken = true;
        }

        private void OnMouseUpAnimationStep(ReorderableList list) {
            actionsValuesTaken = false;
        }

        private void DrawContextInputOnItem(SerializedProperty element, int index, Rect rect1) {
            rect1.x -= 24;
            rect1.width += 24;
            var current = Event.current;

            if (rect1.Contains(current.mousePosition) && current.type == EventType.ContextClick) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Values"), false, () => ContextClickUtils.SetSource(element));
                if (ContextClickUtils.CanPasteToTarget(element)) {
                    menu.AddItem(new GUIContent("Paste Values"), false, () => ContextClickUtils.ApplySourceToTarget(element));
                } else {
                    menu.AddDisabledItem(new GUIContent("Paste Values"));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Duplicate Item"), false, () => DuplicateItem(index));
                menu.AddItem(new GUIContent("Delete Item"), false, () => RemoveItemAtIndex(index));
                menu.ShowAsContext();
                current.Use();
            }
        }

        private void RemoveItemAtIndex(int index) {
            reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);
            reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DuplicateItem(int index) {
            var sourceSerializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            reorderableList.serializedProperty.InsertArrayElementAtIndex(index + 1);
            var source = reorderableList.serializedProperty.GetArrayElementAtIndex(index + 1);
            ContextClickUtils.CopyPropertyValue(sourceSerializedProperty, source);
            source.serializedObject.ApplyModifiedProperties();
        }
        #endregion

        #region Generic foldout
        private void DrawFoldoutArea(string title, ref bool foldout, Action additionalInspectorGUI, Action<Rect> additionalHeaderGUI = null) {
            using (new EditorGUILayout.VerticalScope("FrameBox")) {
                var rect = EditorGUILayout.GetControlRect();
                rect.x += 10;
                rect.width -= 10;
                rect.y -= 4;

                foldout = EditorGUI.Foldout(rect, foldout, title);

                additionalHeaderGUI?.Invoke(rect);

                if (foldout) {
                    additionalInspectorGUI.Invoke();
                }
            }
        }
        #endregion

        #region Top right text header
        private Rect DrawTopRightText(Rect rect, string text, Color color) {
            if (!string.IsNullOrEmpty(text)) {
                var textSize = topRightTextStyle.CalcSize(new GUIContent(text));
                var backgroundRect = new Rect(rect.x + rect.width - textSize.x - 4,
                    rect.y + 2,
                    textSize.x + 4,
                    textSize.y);
                EditorGUI.DrawRect(backgroundRect, color);
                GUI.Label(new Rect(backgroundRect.x + 2, backgroundRect.y, textSize.x, textSize.y), text, topRightTextStyle);

                rect.width -= textSize.x + 6;
            }

            return rect;
        }
        #endregion

        #region Sequencer control
        private void Rewind() {
            EnsureSequencePlaying();
            sequencerController.PlayingSequence.Rewind();
        }

        private void StepBack() {
            AdjustSequenceProgress(-0.01f);
        }

        private void StepNext() {
            AdjustSequenceProgress(0.01f);
        }

        private void AdjustSequenceProgress(float adjustment) {
            EnsureSequencePlaying();

            var progress = Mathf.Clamp01(sequencerController.PlayingSequence.ElapsedPercentage() + adjustment);
            var time = progress * sequencerController.PlayingSequence.Duration();
            sequencerController.PlayingSequence.GotoWithCallbacks(time);
        }

        private void SetProgress(float tweenProgress) {
            EnsureSequencePlaying();
            sequencerController.PlayingSequence.GotoWithCallbacks(tweenProgress * sequencerController.PlayingSequence.Duration());
        }

        private float GetProgress() {
            return sequencerController.PlayingSequence != null ? sequencerController.PlayingSequence.ElapsedPercentage() : 0;
        }

        private void CompleteForward() {
            EnsureSequencePlaying();
            sequencerController.PlayingSequence.Complete(true);
        }

        private void EnsureSequencePlaying() {
            if (sequencerController.PlayingSequence == null) {
                PlaySequence();
            }
        }

        private void PlaySequenceForward() {
            PlaySequenceWithDirection(true);
        }

        private void PlaySequenceBackwards() {
            PlaySequenceWithDirection(false);
        }

        private void PlaySequenceWithDirection(bool forward) {
            if (sequencerController.PlayingSequence == null) {
                isPlayDirectionSet = true;
                isPlayDirectionForward = forward;

                PlaySequence();
            } else {
                if (forward) {
                    sequencerController.PlayForward();
                } else {
                    sequencerController.PlayBackwards();
                }
            }
        }

        private void PlaySequence() {
            justStartPreviewing = false;

            // Handle sequence.
            if (!Application.isPlaying) {
                HandleEditorPreviewSequence();
            } else {
                HandleRuntimeSequence();
            }

            // Update steps panel visibility.
            if (justStartPreviewing) {
                wasShowingStepsPanel = showStepsPanel;
            }

            if (AnimationSequencerPreferences.GetInstance().HideStepsWhenPreviewing) {
                showStepsPanel = false;
            }
        }

        private void HandleEditorPreviewSequence() {
            if (DOTweenEditorPreview.isPreviewing) {
                return;
            }

            // Start preview.
            justStartPreviewing = true;
            DOTweenEditorPreview.Start();
            PlaySequenceWithDirectionSettings();

            var settings = AnimationSequencerPreferences.GetInstance();

            if (settings.VisualizeStepsProgressWhenPreviewing) {
                CalculateStepsAnimationData();
            }

            if (settings.CollapseStepsWhenPreviewing) {
                CollapseSteps();
            }

            DOTweenEditorPreview.PrepareTweenForPreview(sequencerController.PlayingSequence);
        }

        private void HandleRuntimeSequence() {
            PlaySequenceWithDirectionSettings();
        }

        private void PlaySequenceWithDirectionSettings() {
            if (!isPlayDirectionSet) {
                sequencerController.Play();
            } else {
                if (isPlayDirectionForward) {
                    sequencerController.PlayForward();
                } else {
                    sequencerController.PlayBackwards();
                }
            }

            isPlayDirectionSet = false;
            isPlayDirectionForward = false;
        }

        private void StopSequence() {
            if (!DOTweenEditorPreview.isPreviewing) {
                return;
            }

            // Reset sequencer state.
            sequencerController.ResetToInitialState();
            sequencerController.ClearPlayingSequence();
            DOTweenEditorPreview.Stop();

            // Reset steps state.
            if (AnimationSequencerPreferences.GetInstance().HideStepsWhenPreviewing) {
                showStepsPanel = wasShowingStepsPanel;
            }

            if (AnimationSequencerPreferences.GetInstance().CollapseStepsWhenPreviewing) {
                ExpandCollapsedSteps();
            }
        }
        #endregion

        #region Steps animation data
        /// <summary>
        /// Calculate the main sequence duration and "StartTime" of each step relative to the main sequence.
        /// </summary>
        private void CalculateStepsAnimationData() {
            //Calculate the main sequence duration and "StartTime" of each step.
            mainSequenceDuration = 0;
            var startTimeSteps = new float[sequencerController.AnimationSteps.Length];
            float longestStepDuration = 0;

            for (var i = 0; i < sequencerController.AnimationSteps.Length; i++) {
                var step = sequencerController.AnimationSteps[i];
                var stepDuration = step.GetDuration();
                if (stepDuration == -1) {
                    startTimeSteps[i] = stepDuration;
                    continue;
                }

                startTimeSteps[i] = mainSequenceDuration;

                if (i == 0 || step.FlowType == FlowType.Append || stepDuration > longestStepDuration) {
                    longestStepDuration = step.GetDuration();
                }

                var nextStepIndex = i + 1;
                if (nextStepIndex >= sequencerController.AnimationSteps.Length || sequencerController.AnimationSteps[nextStepIndex].FlowType == FlowType.Append) {
                    mainSequenceDuration += longestStepDuration;
                }
            }

            stepsAnimationData = new StepAnimationData[sequencerController.AnimationSteps.Length];

            //Assign the main sequence duration and "StartTime" to each step. 
            for (var i = 0; i < sequencerController.AnimationSteps.Length; i++) {
                var startTime = startTimeSteps[i];
                if (startTime == -1) {
                    continue;
                }

                var tweenDuration = sequencerController.AnimationSteps[i].GetDuration();
                stepsAnimationData[i] = new StepAnimationData(tweenDuration / mainSequenceDuration * 100, startTime, startTime + tweenDuration);
            }
        }
        #endregion

        #region Collapse steps (Previewing)
        private void CollapseSteps() {
            // Load ReorderableList "ClearCache" Method.
            FindReorderableListClearCacheMethod("Collapse Steps");
            if (cantFindReorderableListClearCacheMethod) {
                return;
            }

            // Collapse expanded steps.
            collapsedStepValues.Clear();
            for (var i = 0; i < reorderableList.count; i++) {
                var stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i);
                if (!stepProperty.isExpanded) {
                    continue;
                }

                collapsedStepValues.Add(i, stepProperty.GetPropertyDrawerHeight());
                stepProperty.isExpanded = false;
                stepProperty.SetPropertyDrawerHeight(EditorGUIUtility.singleLineHeight);
            }

            // Repaint ReorderableList.
            CallReorderableListClearCacheMethod();
        }

        private void ExpandCollapsedSteps() {
            if (collapsedStepValues.Count == 0) {
                return;
            }

            if (cantFindReorderableListClearCacheMethod) {
                return;
            }

            // Expand collapsed steps.
            foreach (var collapsedStepHeight in collapsedStepValues) {
                var stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(collapsedStepHeight.Key);
                stepProperty.isExpanded = true;
                stepProperty.SetPropertyDrawerHeight(collapsedStepHeight.Value);
            }

            collapsedStepValues.Clear();

            // Repaint ReorderableList.
            CallReorderableListClearCacheMethod();
        }
        #endregion

        #region Only one step expanded
        private void CheckOnlyOneStepExpanded() {
            // Load ReorderableList "ClearCache" Method.
            //FindReorderableListClearCacheMethod("One Step Expanded");
            //if (cantFindReorderableListClearCacheMethod)
            //    return;

            if (!lastExpandedStepChanged) {
                return;
            }

            // Verify only one step is expanded.
            var reorderableListCount = reorderableList.count;
            for (var i = 0; i < reorderableListCount; i++) {
                var stepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i);

                if (stepProperty.isExpanded) {
                    if (lastExpandedStepIndex == i) {
                        continue;
                    }

                    // Collapse the last expanded step.
                    if (lastExpandedStepIndex != -1 && lastExpandedStepIndex < reorderableListCount) {
                        var lastExpandedStepProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(lastExpandedStepIndex);
                        lastExpandedStepProperty.isExpanded = false;
                        lastExpandedStepProperty.SetPropertyDrawerHeight(EditorGUIUtility.singleLineHeight);
                    }

                    // Update "lastExpandedStepIndex" value.
                    lastExpandedStepIndex = i;
                    lastExpandedStepChanged = false;

                    // Repaint ReorderableList.
                    //CallReorderableListClearCacheMethod();
                    return;
                } else {
                    // Reset "lastExpandedStepIndex" if the last expanded step is collapsed.
                    if (lastExpandedStepIndex == i) {
                        lastExpandedStepIndex = -1;
                    }
                }
            }
        }
        #endregion

        #region Only one action expanded
        private void CollapseAllActions() {
            if (onlyOneActionExpandedEnable == AnimationSequencerPreferences.GetInstance().OnlyOneActionExpandedWhileEditing) {
                return;
            }

            onlyOneActionExpandedEnable = AnimationSequencerPreferences.GetInstance().OnlyOneActionExpandedWhileEditing;

            if (!onlyOneActionExpandedEnable) {
                return;
            }

            var reorderableListCount = reorderableList.serializedProperty.arraySize;
            for (var i = 0; i < reorderableListCount; i++) {
                var isTweenStep = sequencerController.AnimationSteps[i].GetType() == typeof(TweenStep);
                if (isTweenStep) {
                    var actionsSerializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("actions");
                    var actionsCount = actionsSerializedProperty.arraySize;
                    var actionExpandedFounded = false;
                    for (var actionIndex = 0; actionIndex < actionsCount; actionIndex++) {
                        var actionProperty = actionsSerializedProperty.GetArrayElementAtIndex(actionIndex);
                        if (!actionExpandedFounded && actionProperty.isExpanded) {
                            actionExpandedFounded = true;
                            continue;
                        }

                        actionProperty.isExpanded = false;
                    }
                }
            }
        }
        #endregion

        #region ReorderableList "ClearCache" Method
        private void FindReorderableListClearCacheMethod(string featureCallFrom) {
            if (cantFindReorderableListClearCacheMethod) {
                return;
            }

            if (reorderableListClearCacheMethod != null) {
                return;
            }

            var listType = typeof(ReorderableList);
            reorderableListClearCacheMethod = listType.GetMethod("InvalidateCache", BindingFlags.NonPublic | BindingFlags.Instance);
            if (reorderableListClearCacheMethod == null) {
                reorderableListClearCacheMethod = listType.GetMethod("ClearCache", BindingFlags.NonPublic | BindingFlags.Instance);
                if (reorderableListClearCacheMethod == null) {
                    Debug.LogWarning($"The <b>\"{featureCallFrom}\"</b> feature is not available in this editor version. " +
                        "To avoid seeing this warning, please deselect the option in Preferences > Animation Sequencer.");
                    cantFindReorderableListClearCacheMethod = true;
                }
            }
        }

        private void CallReorderableListClearCacheMethod() {
            if (reorderableListClearCacheMethod == null) {
                return;
            }

            reorderableListClearCacheMethod.Invoke(reorderableList, null);
        }
        #endregion

        #region Other callbacks
        private void OnEditorPlayModeChanged(PlayModeStateChange playModeState) {
            if (playModeState == PlayModeStateChange.ExitingEditMode) {
                StopSequence();
            }
        }

        private void PrefabSaving(GameObject gameObject) {
            StopSequence();
        }
        #endregion
    }

    #region Step animation data
    /// <summary>
    /// Class used to show visual animation data for each step.
    /// </summary>
    public class StepAnimationData {
        /// <summary>
        /// Percentage duration of this step relative to the main sequence.
        /// </summary>
        public float percentageDuration;
        /// <summary>
        /// The time this step starts relative to the main sequence.
        /// </summary>
        public float startTime;
        /// <summary>
        /// The time this step ends relative to the main sequence.
        /// </summary>
        public float endTime;
        /// <summary>
        /// Data summary.
        /// </summary>
        public string info;

        public StepAnimationData(float percentageDuration, float startTime, float endTime) {
            this.percentageDuration = percentageDuration;
            this.startTime = startTime;
            this.endTime = endTime;

            var duration = endTime - startTime;
            info = $"Duration: {NumberFormatter.FormatDecimalPlaces(duration)}s ({NumberFormatter.FormatDecimalPlaces(percentageDuration)}%)\n" +
                $"Start time: {NumberFormatter.FormatDecimalPlaces(startTime)}s\n" +
                $"End time: {NumberFormatter.FormatDecimalPlaces(endTime)}s";
        }
    }
    #endregion
}
#endif