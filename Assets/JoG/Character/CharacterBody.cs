using JoG.Character.InputBanks;
using JoG.DebugExtensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Character {

    [DisallowMultipleComponent]
    public partial class CharacterBody : NetworkBehaviour {
        private static readonly List<CharacterBody> _characters = new();
        private Dictionary<string, InputBank> _nameToInputBank = new();
        [SerializeField] private CharacterModel _model;
        [SerializeField] private Transform _aimOriginTransform;
        [SerializeField] private Transform _aimTargetTransform;
        private CharacterMaster _master;
        public static ReadOnlySpan<CharacterBody> Characters => _characters.AsSpan();
        public Animator Animator => _model.Animator;

        public Transform AimOriginTransform => _aimOriginTransform;

        public Transform AimTargetTransform => _aimTargetTransform;

        public Vector3 Center => _model.Center.position;

        public Vector3 Top => _model.Top.position;

        public Vector3 Bottom => _model.Bottom.position;

        public Vector3 AimOrigin => _aimOriginTransform.position;

        public Vector3 AimTarget { get => _aimTargetTransform.position; set => _aimTargetTransform.position = value; }

        public Vector3 AimVector => _aimTargetTransform.position - _aimOriginTransform.position;

        public float Radius => _model.Radius;

        public float Height => _model.Height;

        public CharacterModel Model => _model;

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                MaxHP = (uint)Math.Round(_maxHPBase * _maxHPCoefficient);
                HP = MaxHP;
            }
            InitializeBuff();
        }

        public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject) {
            this.Log("角色身体的父级发生了变化！[Parent: {0}]", parentNetworkObject);
            if (parentNetworkObject is null) {
                _master = null;
                return;
            }
            parentNetworkObject.TryGetComponent(out _master);
        }

        public override void OnNetworkDespawn() {
            DeinitializeBuff();
        }

        public T GetInputBank<T>(string name) where T : InputBank, new() {
            
            if (_nameToInputBank.TryGetValue(name, out var result)) {
                return result as T;
            } else {
                var inputBank = new T();
                _nameToInputBank[name] = inputBank;
                return inputBank;
            }
        }
    }
}