using JoG.Character;
using JoG.States;
using System;
using UnityEngine;
using VContainer;
using Xoderony;

namespace JoG.StateMachines {

    [DisallowMultipleComponent]
    public class CharacterRootStateMachine : MonoBehaviour, IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        private readonly StateMachine _stateMachine = new();

        [SerializeField]
        private MonoBehaviour _lifeState;

        [SerializeField]
        private MonoBehaviour _deathState;

        private IState _life;

        private IState _death;

        [Inject] internal IDelegateSubscriber<CharacterLifeStartHandler> lifeStarted;

        [Inject] internal IDelegateSubscriber<CharacterLifeStopHandler> lifeStopped;

        public MonoBehaviour LifeState {
            get => _lifeState;
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value is not IState state) {
                    throw new ArgumentException("The value must be IState!");
                }
                _lifeState = value;
                _life = state;
            }
        }

        public MonoBehaviour DeathState {
            get => _deathState;
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value is not IState state) {
                    throw new ArgumentException("The value must be IState!");
                }
                _deathState = value;
                _death = state;
            }
        }

        private void Awake() {
            _life = _lifeState as IState;
            _death = _deathState as IState;
        }

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            lifeStarted.Subscribe(OnLifeStart);
            lifeStopped.Subscribe(OnLifeStop);
            _stateMachine.TransitionTo(null);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            lifeStarted.Unsubscribe(OnLifeStart);
            lifeStopped.Unsubscribe(OnLifeStop);
        }

        private void OnLifeStart(CharacterEntity entity) {
            _stateMachine.TransitionTo(_life);
        }

        private void OnLifeStop(CharacterEntity entity) {
            _stateMachine.TransitionTo(_death);
        }
    }
}
