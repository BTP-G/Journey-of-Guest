using Cysharp.Threading.Tasks;
using Xoderony.YooAsset;
using JoG.Buff;
using JoG.Character;
using JoG.Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class EnemySpawner : CharacterSpawner {

        public YooAssetReference<GameObject> characterPrefab;

        public YooAssetReference<GameObject> maxHealthBuffPrefab;

        public YooAssetReference<GameObject> attackPowerBuffPrefab;

        [Min(0)]
        public int buffCountOnSpawn;

        [Min(0)] public int buffCountIncrementPerSpawn = 1;

        [Min(0)] public float respawnDelay = 10;

        [Inject] internal DifficultyManager difficultyManager;

        private NetworkObject _characterObject;



        public void Awake() {
            characterPrefab.Load();
            maxHealthBuffPrefab.Load();
            attackPowerBuffPrefab.Load();

        }

        public override void OnBodySpawn(CharacterEntity entity) {
            base.OnBodySpawn(entity);
            ApplyBuff(entity);
        }

        public override void OnBodyLifeStop(CharacterEntity entity) {
            base.OnBodyLifeStop(entity);
            buffCountOnSpawn += buffCountIncrementPerSpawn;
            entity.Buffs.Clear();
            ApplyBuff(entity);
            Invoke(nameof(Respawn), respawnDelay);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            characterPrefab.Unload();
            maxHealthBuffPrefab.Unload();
            attackPowerBuffPrefab.Unload();
        }

        protected override async void OnInSceneObjectsSpawned() {
            base.OnInSceneObjectsSpawned();
            await UniTask.WaitForSeconds(3);
            if (!HasAuthority) {
                return;
            }

            transform.GetPositionAndRotation(out var position, out var rotation);
            if (NavMesh.SamplePosition(position, out var hit, 100f, NavMesh.AllAreas)) {
                position = hit.position;
            }
            var prefab = characterPrefab.AssetObject.GetComponent<NetworkObject>();
            SpawnBody(prefab, position, rotation);
        }

        private void ApplyBuff(CharacterEntity entity) {
            if (buffCountOnSpawn > 0) {
                //var maxHealthBuff = maxHealthBuffData.Get();
                //var attackPowerBuff = attackPowerBuffData.Get();
                //foreach (var component in maxHealthBuff.ComponentSpan) {
                //    if (component is Counter counter) {
                //        var baseCount = buffCountOnSpawn;
                //        counter.count = Mathf.RoundToInt(baseCount * difficultyManager.CurrentHealthMultiplier);
                //        break;
                //    }
                //}
                //foreach (var component in attackPowerBuff.ComponentSpan) {
                //    if (component is Counter counter) {
                //        var baseCount = buffCountOnSpawn;
                //        counter.count = Mathf.RoundToInt(baseCount * difficultyManager.CurrentAttackMultiplier);
                //        break;
                //    }
                //}
                //entity.Buffs.AddBuff(maxHealthBuff);
                //entity.Buffs.AddBuff(attackPowerBuff);
            }
        }

        private void Respawn() {
            if (HasAuthority && Body != null) {
                Body.Health.Current = Body.Health.Max;
                Body.Motor.Position = transform.position;
            }
        }
    }
}
