using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Xoderony.Logging;

namespace Xoderony.GameplayEffects {

    [CreateAssetMenu(menuName = "Xoderony/Gameplay Effect Definition", fileName = nameof(GameplayEffectDefinition))]
    public class GameplayEffectDefinition : ScriptableObject {

        [SerializeReferenceDropdown]
        private GameplayEffectData[] _dataArray = Array.Empty<GameplayEffectData>();

        private int _id;

        private bool _readOnly;

        public int Id {
            get => _id;
            internal set {
                _id = value;
                _readOnly = value != 0;
                foreach (var data in _dataArray) {
                    if (data is not null) {
                        data.ReadOnly = _readOnly;
                    }
                }
            }
        }

        public bool ReadOnly => _readOnly;

        public ReadOnlySpan<GameplayEffectData> DataSpan {
            get => _dataArray;
            set {
                if (ReadOnly) {
                    this.LogError($"{nameof(DataSpan)} is readonly now!");
                    return;
                }
                _dataArray = value.ToArray();
                ValidateDataArray();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetData<T>() where T : GameplayEffectData {
            foreach (var data in DataSpan) {
                if (data is T typedData) {
                    return typedData;
                }
            }
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetData<T>(out T data) where T : GameplayEffectData {
            foreach (var currentData in DataSpan) {
                if (currentData is T typedData) {
                    data = typedData;
                    return true;
                }
            }
            data = default;
            return false;
        }

        protected virtual void OnValidate() {
            ValidateDataArray();
        }

        private void ValidateDataArray() {
            foreach (var data in _dataArray) {
                data?.Validate();
            }
        }
    }
}
