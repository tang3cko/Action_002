using Unity.Mathematics;
using UnityEngine;

namespace Action002.Boss.Systems
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Action002/Boss Config")]
    public class BossConfigSO : ScriptableObject
    {
        [Header("Phase 1 - Guardians")]
        [SerializeField] private int phase1HpPerGuardian = 50;
        [SerializeField] private float phase1ShootCooldown = 1.5f;
        [SerializeField] private float phase1SimultaneousThreshold = 15f;
        [SerializeField] private float phase1ForcedSwitchInterval = 8f;
        [SerializeField] private int phase1KillScore = 500;
        [SerializeField] private float guardianCollisionRadius = 1.0f;
        [SerializeField] private float2 whiteGuardianOffset = new(-3f, 3f);
        [SerializeField] private float2 blackGuardianOffset = new(3f, 3f);

        [Header("Phase 2 - Magatama")]
        [SerializeField] private int phase2HpMagatama = 80;
        [SerializeField] private float phase2ShootCooldown = 0.8f;
        [SerializeField] private float phase2ForcedSwitchInterval = 4f;
        [SerializeField] private int phase2KillScore = 1000;
        [SerializeField] private float magatamaCollisionRadius = 1.5f;
        [SerializeField] private float magatamaRotationSpeed = 0.5f;

        [Header("Timing")]
        [SerializeField] private float introDuration = 2f;
        [SerializeField] private float mergeDuration = 1.5f;
        [SerializeField] private float forcedSwitchWarningDuration = 2f;

        public int Phase1HpPerGuardian => phase1HpPerGuardian;
        public float Phase1ShootCooldown => phase1ShootCooldown;
        public float Phase1SimultaneousThreshold => phase1SimultaneousThreshold;
        public float Phase1ForcedSwitchInterval => phase1ForcedSwitchInterval;
        public int Phase1KillScore => phase1KillScore;
        public float GuardianCollisionRadius => guardianCollisionRadius;
        public float2 WhiteGuardianOffset => whiteGuardianOffset;
        public float2 BlackGuardianOffset => blackGuardianOffset;
        public int Phase2HpMagatama => phase2HpMagatama;
        public float Phase2ShootCooldown => phase2ShootCooldown;
        public float Phase2ForcedSwitchInterval => phase2ForcedSwitchInterval;
        public int Phase2KillScore => phase2KillScore;
        public float MagatamaCollisionRadius => magatamaCollisionRadius;
        public float MagatamaRotationSpeed => magatamaRotationSpeed;
        public float IntroDuration => introDuration;
        public float MergeDuration => mergeDuration;
        public float ForcedSwitchWarningDuration => forcedSwitchWarningDuration;
    }
}
