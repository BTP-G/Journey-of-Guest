using System;
using UnityEngine;

namespace JoG.Weapons.Datas {

    [CreateAssetMenu(fileName = nameof(GunData), menuName = nameof(Weapons) + "/" + nameof(Datas) + "/" + nameof(GunData))]
    public class GunData : ScriptableObject {
        [Range(0, float.MaxValue)] public float fireInterval;
        [Range(0, float.MaxValue)] public float reloadTime;
        [Range(0, float.MaxValue)] public float timeOfStabilizing;
        public Vector2 spread;
        public Vector2 RandomSpread => UnityEngine.Random.insideUnitCircle * spread;
    }
}