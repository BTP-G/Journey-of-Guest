using Cysharp.Threading.Tasks;
using Expriverse.Character;
using Expriverse.Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using Xoderony;
using Xoderony.GameplayEffects;
using Xoderony.YooAsset;

namespace Expriverse.AI {

    public class EnemySpawner : CharacterSpawner {

        public YooAssetReference<GameObject> characterPrefab;

        public YooAssetReference<GameplayEffectDefinition> maxHealthEffectDefinition;

        public YooAssetReference<GameplayEffectDefinition> attackPowerEffectDefinition;

        [Min(0)]
        public int effectCountOnSpawn;

        [Min(0)] public int effectCountIncrementPerSpawn = 1;

        [Min(0)] public float respawnDelay = 10;

        [Inject] internal DifficultyManager difficultyManager;

        private readonly NetworkVariable<int> _respawnCount = new(writePerm: NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<double> _respawnAt = new(-1, writePerm: NetworkVariableWritePermission.Owner);

        private IDelegateSubscriber<CharacterLifeStopHandler> _lifeStoppedSubscriber;

        private bool _isReadyToSpawn;

        private int _appliedMaxHealthEffectCount;

        private int _appliedAttackPowerEffectCount;

        public void Awake() {
            characterPrefab.Load();
            maxHealthEffectDefinition.Load();
            attackPowerEffectDefinition.Load();

        }

        protected override void OnBodyAssigned(CharacterEntity entity) {
            base.OnBodyAssigned(entity);
            _appliedMaxHealthEffectCount = 0;
            _appliedAttackPowerEffectCount = 0;
            _lifeStoppedSubscriber = entity.GetComponent<IDelegateSubscriber<CharacterLifeStopHandler>>();
            _lifeStoppedSubscriber.Subscribe(OnBodyLifeStopped);
            if (CanControlBody) {
                ApplyEffects(entity);
            }
        }

        protected override void OnBodyReleased(CharacterEntity entity) {
            base.OnBodyReleased(entity);
            _lifeStoppedSubscriber.Unsubscribe(OnBodyLifeStopped);
            _lifeStoppedSubscriber = null;
            _appliedMaxHealthEffectCount = 0;
            _appliedAttackPowerEffectCount = 0;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            characterPrefab.Unload();
            maxHealthEffectDefinition.Unload();
            attackPowerEffectDefinition.Unload();
        }

        protected override async void OnInSceneObjectsSpawned() {
            base.OnInSceneObjectsSpawned();
            await UniTask.WaitForSeconds(3);
            _isReadyToSpawn = true;
            TrySpawnEnemy();
        }

        protected override void Update() {
            base.Update();
            TrySpawnEnemy();

            if (!HasAuthority || _respawnAt.Value < 0 || !CanControlBody) {
                return;
            }
            if (NetworkManager.ServerTime.Time < _respawnAt.Value) {
                return;
            }

            Respawn();
        }

        private void TrySpawnEnemy() {
            if (!_isReadyToSpawn || !HasAuthority || HasBodyReference) {
                return;
            }

            transform.GetPositionAndRotation(out var position, out var rotation);
            if (NavMesh.SamplePosition(position, out var hit, 100f, NavMesh.AllAreas)) {
                position = hit.position;
            }
            var prefab = characterPrefab.AssetObject.GetComponent<NetworkObject>();
            if (TrySpawnBody(prefab, position, rotation, out _)) {
                _respawnAt.Value = -1;
            }
        }

        private void OnBodyLifeStopped(CharacterEntity entity) {
            if (!HasAuthority || entity != Body || _respawnAt.Value >= 0) {
                return;
            }

            _respawnCount.Value++;
            _respawnAt.Value = NetworkManager.ServerTime.Time + respawnDelay;
        }

        private void ApplyEffects(CharacterEntity entity) {
            var effectCount = effectCountOnSpawn + (_respawnCount.Value * effectCountIncrementPerSpawn);
            var maxHealthEffect = maxHealthEffectDefinition.AssetObject;
            var maxHealthEffectCount = Mathf.RoundToInt(effectCount * difficultyManager.CurrentHealthMultiplier);
            ApplyEffectCountDelta(entity.Effects, maxHealthEffect, ref _appliedMaxHealthEffectCount, maxHealthEffectCount);

            var attackPowerEffect = attackPowerEffectDefinition.AssetObject;
            var attackPowerEffectCount = Mathf.RoundToInt(effectCount * difficultyManager.CurrentAttackMultiplier);
            ApplyEffectCountDelta(entity.Effects, attackPowerEffect, ref _appliedAttackPowerEffectCount, attackPowerEffectCount);
        }

        private void Respawn() {
            ApplyEffects(Body);
            Body.Health.Current = Body.Health.Max;
            Body.Motor.Position = transform.position;
            _respawnAt.Value = -1;
        }

        private static void ApplyEffectCountDelta(CharacterEffects effects, GameplayEffectDefinition definition, ref int appliedCount, int targetCount) {
            var countDelta = targetCount - appliedCount;
            if (countDelta > 0) {
                effects.AddEffectRpc(definition.Id, countDelta);
            } else if (countDelta < 0) {
                effects.RemoveEffectRpc(definition.Id, -countDelta);
            }
            appliedCount = targetCount;
        }
    }
}
