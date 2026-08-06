using System;

namespace Xoderony.GameplayEffects {

    public interface IGameplayEffectController {

        Type DataType { get; }

        void SetEffectCount(int definitionId, GameplayEffectData data, int count);

        void Clear();
    }

    [Serializable]
    public abstract class GameplayEffectController<TData> : IGameplayEffectController where TData : GameplayEffectData {

        Type IGameplayEffectController.DataType => typeof(TData);

        void IGameplayEffectController.SetEffectCount(int definitionId, GameplayEffectData data, int count) {
            SetEffectCount(definitionId, (TData)data, count);
        }

        void IGameplayEffectController.Clear() {
            Clear();
        }

        protected abstract void SetEffectCount(int definitionId, TData data, int count);

        protected abstract void Clear();
    }
}
