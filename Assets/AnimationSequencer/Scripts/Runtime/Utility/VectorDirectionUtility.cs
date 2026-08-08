using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Created by Pablo Huaxteco
    public static class VectorDirectionUtility {
        public enum VectorDirection {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Backward
        }

        /// <summary>
        /// Gets the Vector3 representation of a given direction.
        /// </summary>
        /// <param name="direction">The direction to convert.</param>
        /// <returns>A Vector3 representing the specified direction.</returns>
        public static Vector3 GetDirectionVector(VectorDirection direction) {
            return direction switch {
                VectorDirection.Up => Vector3.up,
                VectorDirection.Down => Vector3.down,
                VectorDirection.Left => Vector3.left,
                VectorDirection.Right => Vector3.right,
                VectorDirection.Forward => Vector3.forward,
                VectorDirection.Backward => Vector3.back,
                _ => Vector3.zero,// Default in case an undefined direction is used
            };
        }
    }
}
