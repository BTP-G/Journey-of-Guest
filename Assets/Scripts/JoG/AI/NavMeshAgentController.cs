using EditorAttributes;
using Xoderony.Extensions;
using JoG.Character;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class NavMeshAgentController : MonoBehaviour, IComponent {
        public float warpDistanceThreshold = 0.5f;
        public float updateDestinationDistanceThreshold = 1;
        [Required] public NavMeshAgent agent;
        [Required] public AITarget target;
        [Inject] internal Rigidbody body;
        [Inject, Key(Constants.Stats.MaxMoveSpeed)] internal Stat maxMoveSpeedStat;
        [Inject, Key(Constants.Stats.MoveAcceleration)] internal Stat accelerationStat;

        private void Start() {
            agent.updatePosition = false;
            agent.updateRotation = false;
            warpDistanceThreshold *= warpDistanceThreshold;
            updateDestinationDistanceThreshold *= updateDestinationDistanceThreshold;
        }

        private void Update() {
            agent.speed = maxMoveSpeedStat.Value;
            agent.acceleration = accelerationStat.Value;
            if (!agent.isOnOffMeshLink && agent.nextPosition.SqrDistanceTo(body.position) > warpDistanceThreshold) {
                agent.nextPosition = body.position;
            }
            if (target.target == null) {
                return;
            }
            if (agent.destination.SqrDistanceTo(target.target.position) > updateDestinationDistanceThreshold) {
                agent.SetDestination(target.target.position);
            }
        }
    }
}
