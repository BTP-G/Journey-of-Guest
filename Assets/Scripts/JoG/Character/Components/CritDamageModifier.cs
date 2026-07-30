using UnityEngine;
using VContainer;

namespace JoG.Character.Components {

    //public class CritDamageModifier : MonoBehaviour, IComponent {
    //    [Inject, Name(Constants.Stats.CritRate)] internal Stat critRate;
    //    [Inject, Name(Constants.Stats.CritDamage)] internal Stat critDamage;

    //    [Inject]
    //    internal void Inject(DelegateHub delegateHub) {
    //        delegateHub.AddDelegate<OutgoingDamageMessageModifier>(OnAttack);
    //    }

    //    private void OnAttack(ref HealthChangeMessage message, in Victim target) {
    //        if (message.HasFlag((ulong)HealthChangeFlag.Direct) && Random.Range(0, 100) < critRate.Value) {
    //            message._flags |= (ulong)HealthChangeFlag.Critical;
    //            message.Value = (int)(message.Value * critDamage.Value);
    //        }
    //    }
    //}
}
