using JoG.Character;
using JoG.Magic;
using JoG.Projectiles;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Spells {

    [CreateAssetMenu(fileName = nameof(FireBall), menuName = nameof(Spells) + "/" + nameof(FireBall))]
    public class FireBall : Spell {
        public NetworkObject projectilePrefab;

        public override void Cast(CharacterBody caster, in Vector3 position, in Quaternion rotation) {
            var _networkManager = caster.NetworkManager;
            var nob = _networkManager.SpawnManager.InstantiateAndSpawn(projectilePrefab,
                 caster.OwnerClientId,
                 true,
                 false,
                 false,
                 position,
                 rotation);
            var projectile = nob.GetComponent<ProjectileData>();
            projectile.ownerReference = caster.NetworkObject;
        }
    }
}