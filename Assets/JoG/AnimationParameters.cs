using UnityEngine;

namespace JoG {

    public static class AnimationParameters {
        public static readonly int forwardSpeed = Animator.StringToHash("forwardSpeed");
        public static readonly int isAimming = Animator.StringToHash("isAimming");
        public static readonly int isCrouching = Animator.StringToHash("isCrouching");
        public static readonly int isGrounded = Animator.StringToHash("isGrounded");
        public static readonly int isMoving = Animator.StringToHash("isMoving");
        public static readonly int isSprinting = Animator.StringToHash("isSprinting");
        public static readonly int rightSpeed = Animator.StringToHash("rightSpeed");
        public static readonly int upSpeed = Animator.StringToHash("upSpeed");
        public static readonly int maxMoveSpeed = Animator.StringToHash("maxMoveSpeed"); 
    }
}