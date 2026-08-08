using JoG.Localization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Xoderony.Localization;
using Xoderony.Unity;

namespace JoG.Gameplay.Objectives {

    public class TimerObjective : ObjectiveController {
        [Min(0)] public float requiredTime = 60f;

        private float _targetTime;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            PostUpdateLoop<Update.ScriptRunBehaviourUpdate>.Register(OnPostUpdate);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            PostUpdateLoop<Update.ScriptRunBehaviourUpdate>.Unregister(OnPostUpdate);
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            _targetTime = NetworkManager.ServerTime.TimeAsFloat + requiredTime;
            serializer.SerializeValue(ref _targetTime);
        }

        private void OnPostUpdate() {
            var remainingTime = Mathf.Max(0, _targetTime - NetworkManager.ServerTime.TimeAsFloat);
            IsComplete = remainingTime == 0;
            labelText.text = Localizer.GetString(L10nKeys.Objective.TimerObjective, Mathf.RoundToInt(remainingTime));
        }
    }
}
