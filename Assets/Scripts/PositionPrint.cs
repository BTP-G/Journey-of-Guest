using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace JoG {

    public class PositionPrint : MonoBehaviour {
        public NetworkTransform networkTransform;
        private Vector3 lastPosition;
        // Update is called once per frame
        private void Update() {
            var currentPosition = transform.position;
            if(networkTransform.HasAuthority || lastPosition == (currentPosition)) return;
            print(currentPosition.ToString("F4"));
            lastPosition = currentPosition;
             
        }
    }
}