using UnityEngine;

namespace Expriverse {

    public static class AnimatorHashs {
        public static readonly int forwardSpeed = Animator.StringToHash(nameof(forwardSpeed));
        public static readonly int isAimming = Animator.StringToHash(nameof(isAimming));
        public static readonly int isCrouching = Animator.StringToHash(nameof(isCrouching));
        public static readonly int isDead = Animator.StringToHash(nameof(isDead));
        public static readonly int isGrounded = Animator.StringToHash(nameof(isGrounded));
        public static readonly int isMoving = Animator.StringToHash(nameof(isMoving));
        public static readonly int SpawnState = Animator.StringToHash(nameof(SpawnState));
        public static readonly int isAttacking = Animator.StringToHash(nameof(isAttacking));
        public static readonly int isAttackingR = Animator.StringToHash(nameof(isAttackingR));
        public static readonly int isAttackingL = Animator.StringToHash(nameof(isAttackingL));
        public static readonly int SpeedCap = Animator.StringToHash(nameof(SpeedCap));
        public static readonly int rightSpeed = Animator.StringToHash(nameof(rightSpeed));
        public static readonly int upSpeed = Animator.StringToHash(nameof(upSpeed));
        public static readonly int isChargingL = Animator.StringToHash(nameof(isChargingL));
        public static readonly int isChargingR = Animator.StringToHash(nameof(isChargingR));
        public static readonly int Idle = Animator.StringToHash(nameof(Idle));
        public static readonly int Ground = Animator.StringToHash(nameof(Ground));
        public static readonly int Air = Animator.StringToHash(nameof(Air));
        public static readonly int Cast = Animator.StringToHash(nameof(Cast));
        public static readonly int maxMoveSpeed = Animator.StringToHash(nameof(maxMoveSpeed));
    }
}
