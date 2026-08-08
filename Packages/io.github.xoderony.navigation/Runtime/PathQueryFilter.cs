using System;
using UnityEngine.AI;

namespace Xoderony.AI {

    [Serializable]
    public struct PathQueryFilter {

        public int agentTypeID;

        public int areaMask;

        public static implicit operator NavMeshQueryFilter(in PathQueryFilter filter) {
            return new() {
                agentTypeID = filter.agentTypeID,
                areaMask = filter.areaMask
            };
        }
    }
}
