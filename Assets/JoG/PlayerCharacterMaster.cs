using GuestUnion.ObjectPool.Generic;
using JoG.Character;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace JoG {

    [DisallowMultipleComponent]
    public class PlayerCharacterMaster : CharacterMaster {
        public GameObject topUIPrefab;
        private static readonly List<PlayerCharacterMaster> _players = new();
        private readonly NetworkVariable<FixedString32Bytes> _playerName = new(writePerm: NetworkVariableWritePermission.Owner);
        public static ReadOnlySpan<PlayerCharacterMaster> Players => _players.AsSpan();

        /// <summary>Write: Owner Only.</summary>
        public string PlayerName {
            get => _playerName.Value.ToString();
            set => _playerName.Value = value;
        }

        public static bool FindPlayer(string playerName, out PlayerCharacterMaster result) {
            foreach (var player in _players.AsSpan()) {
                if (player._playerName.Value.Equals(playerName)) {
                    result = player;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            _players.Add(this);
            if (!IsLocalPlayer) {
                var go = Instantiate(topUIPrefab, transform);
                GetComponent<LifetimeScope>().Container.InjectGameObject(go);
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            _players.Remove(this);
        }

        protected override void OnBodyChanged(CharacterBody previous, CharacterBody next) {
            using (ListPool<Collider>.Rent(out var buffer)) {
                if (previous != null) {
                    previous.GetComponentsInChildren(buffer);
                    foreach (var c in buffer.AsSpan()) {
                        c.tag = "HitCollider";
                    }
                }
                if (next is not null) {
                    next.GetComponentsInChildren(buffer);
                    foreach (var c in buffer.AsSpan()) {
                        c.tag = "LocalPlayerCharacterCollider";
                    }
                }
            }
        }
    }
}