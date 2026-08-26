using EditorAttributes;
using Expriverse.Health;
using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using URandom = UnityEngine.Random;

namespace Expriverse.UI {

    [Serializable]
    [DisallowMultipleComponent]
    public class FloatingTextController : MonoBehaviour, IMessageHandler<HealthChangeReport> {
        [Required] public FloatingText floatingTextTemplate;
        [Inject] internal ISubscriber<HealthChangeReport> damageReportSubscriber;
        private readonly Stack<FloatingText> _pool = new();
        private IDisposable _subscription1;

        public Vector3 GetRandomFloatDisplacement() {
            return new Vector3(
                URandom.Range(-1, 1),
                URandom.Range(2, 4),
                URandom.Range(-1, 1)
            );
        }

        //void IMessageHandler<HealthChangeMessage>.Handle(HealthChangeMessage report) {
        //    if (report.target.Entity is CharacterEntity entity) {
        //        if (!_pool.TryPop(out var floatingText)) {
        //            floatingText = Instantiate(floatingTextTemplate);
        //        }
        //        floatingText.Position(entity.Model.Center)
        //            .Text(report.deltaHeal.ToString())
        //            .Color(Color.green)
        //            .Fire(GetRandomFloatDisplacement(), 3, OnFloatingTextStop);
        //    }
        //}

        void IMessageHandler<HealthChangeReport>.Handle(HealthChangeReport report) {
            if (!_pool.TryPop(out var floatingText)) {
                floatingText = Instantiate(floatingTextTemplate);
            }
            floatingText.transform.position = report.position;
            var color = report.color;

            floatingText.Position(report.position)
                .Text(report.deltaValue.ToString())
                .Color(color)
                .Fire(GetRandomFloatDisplacement(), 3, OnFloatingTextStop);
        }

        private void OnEnable() {
            _subscription1 = damageReportSubscriber.Subscribe(this);
        }

        private void OnDisable() {
            _subscription1.Dispose();
        }

        private void OnFloatingTextStop(FloatingText text) {
            _pool.Push(text);
        }
    }
}
