using UnityEngine;

namespace Expriverse.Gameplay {

    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Expriverse/Difficulty/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject {

        [Tooltip("X轴: TimeDL, Y轴: 血量倍率")]
        public AnimationCurve hpMultiplierCurve = new AnimationCurve(
            new Keyframe(0, 1.0f),
            new Keyframe(5, 1.5f),
            new Keyframe(10, 2.0f),
            new Keyframe(15, 2.6f),
            new Keyframe(20, 3.4f),
            new Keyframe(25, 4.8f),
            new Keyframe(30, 7.2f)
        );

        [Tooltip("X轴: TimeDL, Y轴: 攻击倍率")]
        public AnimationCurve atkMultiplierCurve = new AnimationCurve(
            new Keyframe(0, 1.0f),
            new Keyframe(5, 1.3f),
            new Keyframe(10, 1.6f),
            new Keyframe(15, 1.9f),
            new Keyframe(20, 2.3f),
            new Keyframe(25, 3.0f),
            new Keyframe(30, 4.2f)
        );

        [Tooltip("X轴: 玩家数量(1-16), Y轴: PartyDL值")]
        public AnimationCurve playerCountToMultiplierCurve = new(new Keyframe[] {
            new Keyframe(1f, 1.00f),
            new Keyframe(2f, 1.15f),
            new Keyframe(4f, 1.40f),
            new Keyframe(8f, 1.80f),
            new Keyframe(16f, 2.28f)
        });

        [Header("Boss Health Multiplier Curve")]
        [Tooltip("X轴: TimeDL, Y轴: Boss血量倍率")]
        public AnimationCurve bossHealthMultiplierCurve = new AnimationCurve(
            new Keyframe(1.0f, 5f),   // DL 1.0 → ×15
            new Keyframe(2.0f, 8f),   // DL 2.0 → ×22
            new Keyframe(3.0f, 12f),   // DL 3.0 → ×30
            new Keyframe(4.0f, 18f),   // DL 4.0 → ×40
            new Keyframe(5.5f, 25f)    // DL 5.5 → ×50
        );

        [Header("Boss Attack Multiplier Curve")]
        [Tooltip("X轴: TimeDL, Y轴: Boss攻击倍率")]
        public AnimationCurve bossAttackMultiplierCurve = new AnimationCurve(
            new Keyframe(1.0f, 3f),    // DL 1.0 → ×5
            new Keyframe(2.0f, 5f),    // DL 2.0 → ×8
            new Keyframe(3.0f, 7f),   // DL 3.0 → ×12
            new Keyframe(4.0f, 10f),   // DL 4.0 → ×16
            new Keyframe(5.5f, 14f)    // DL 5.5 → ×22
        );

        [Header("Boss PartyDL Multiplier")]
        [Tooltip("X轴: 玩家数量(1-16), Y轴: Boss人数加成")]
        public AnimationCurve bossPartyMultiplierCurve = new AnimationCurve(
            new Keyframe(1f, 1.00f),
            new Keyframe(2f, 1.30f),
            new Keyframe(4f, 1.80f),
            new Keyframe(8f, 2.55f),
            new Keyframe(16f, 3.47f)
        );

        [Tooltip("X轴: Time, Y轴: 掉落数量倍率")]
        public AnimationCurve timeToDropChanceCurve = AnimationCurve.Linear(1f, 1f, 5.5f, 2.0f);
    }
}
