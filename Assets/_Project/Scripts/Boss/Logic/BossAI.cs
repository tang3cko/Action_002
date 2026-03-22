using Action002.Audio.Systems;
using Action002.Boss.Data;
using Unity.Mathematics;

namespace Action002.Boss.Logic
{
    public class BossAI
    {
        private readonly IBossAIActions actions;
        private readonly IRhythmClock rhythmClock;

        private readonly int phase1HpPerGuardian;
        private readonly int phase2HpMagatama;
        private readonly float phase1ShootCooldown;
        private readonly float phase2ShootCooldown;
        private readonly float phase1SimultaneousThreshold;
        private readonly float phase1ForcedSwitchInterval;
        private readonly float phase2ForcedSwitchInterval;
        private readonly float forcedSwitchWarningDuration;
        private readonly float introDuration;
        private readonly float mergeDuration;
        private readonly int phase1KillScore;
        private readonly int phase2KillScore;
        private readonly float guardianCollisionRadius;
        private readonly float magatamaCollisionRadius;
        private readonly float2 whiteGuardianOffset;
        private readonly float2 blackGuardianOffset;
        private readonly float magatamaRotationSpeed;

        private BossEntityState[] entities;
        private float phaseTimer;
        private float lastFireTime;
        private int lastConsumedHalfBeatIndex;
        private float forcedSwitchTimer;
        private float warningTimer;
        private bool isWarningActive;
        private bool isDefeatedNotified;
        private float magatamaAngle;
        private float2 arenaCenter;
        private float magatamaOrbitRadius;

        public BossPhaseId CurrentPhase { get; private set; }
        public bool IsActive { get; private set; }

        public BossAI(
            IBossAIActions actions,
            IRhythmClock rhythmClock,
            int phase1HpPerGuardian, int phase2HpMagatama,
            float phase1ShootCooldown, float phase2ShootCooldown,
            float phase1SimultaneousThreshold,
            float phase1ForcedSwitchInterval, float phase2ForcedSwitchInterval,
            float forcedSwitchWarningDuration,
            float introDuration, float mergeDuration,
            int phase1KillScore, int phase2KillScore,
            float guardianCollisionRadius, float magatamaCollisionRadius,
            float2 whiteGuardianOffset, float2 blackGuardianOffset,
            float magatamaRotationSpeed)
        {
            this.actions = actions;
            this.rhythmClock = rhythmClock;
            this.phase1HpPerGuardian = phase1HpPerGuardian;
            this.phase2HpMagatama = phase2HpMagatama;
            this.phase1ShootCooldown = phase1ShootCooldown;
            this.phase2ShootCooldown = phase2ShootCooldown;
            this.phase1SimultaneousThreshold = phase1SimultaneousThreshold;
            this.phase1ForcedSwitchInterval = phase1ForcedSwitchInterval;
            this.phase2ForcedSwitchInterval = phase2ForcedSwitchInterval;
            this.forcedSwitchWarningDuration = forcedSwitchWarningDuration;
            this.introDuration = introDuration;
            this.mergeDuration = mergeDuration;
            this.phase1KillScore = phase1KillScore;
            this.phase2KillScore = phase2KillScore;
            this.guardianCollisionRadius = guardianCollisionRadius;
            this.magatamaCollisionRadius = magatamaCollisionRadius;
            this.whiteGuardianOffset = whiteGuardianOffset;
            this.blackGuardianOffset = blackGuardianOffset;
            this.magatamaRotationSpeed = magatamaRotationSpeed;

            entities = new BossEntityState[3];
        }

        public BossEntityState GetEntity(BossEntityId id) => entities[(int)id];

        public void Begin(float2 arenaCenter)
        {
            this.arenaCenter = arenaCenter;
            IsActive = true;
            isDefeatedNotified = false;
            isWarningActive = false;
            magatamaAngle = 0f;
            phaseTimer = 0f;
            lastFireTime = -9999f;
            lastConsumedHalfBeatIndex = 0;
            forcedSwitchTimer = 0f;
            warningTimer = 0f;
            entities[(int)BossEntityId.Magatama] = default;

            entities[(int)BossEntityId.WhiteGuardian] = new BossEntityState
            {
                Position = arenaCenter + whiteGuardianOffset,
                Hp = phase1HpPerGuardian,
                MaxHp = phase1HpPerGuardian,
                Polarity = 0,
                CollisionRadius = guardianCollisionRadius,
                IsActive = true,
            };
            entities[(int)BossEntityId.BlackGuardian] = new BossEntityState
            {
                Position = arenaCenter + blackGuardianOffset,
                Hp = phase1HpPerGuardian,
                MaxHp = phase1HpPerGuardian,
                Polarity = 1,
                CollisionRadius = guardianCollisionRadius,
                IsActive = true,
            };

            var wg = entities[(int)BossEntityId.WhiteGuardian];
            var bg = entities[(int)BossEntityId.BlackGuardian];
            actions.SpawnEntity(BossEntityId.WhiteGuardian, wg.Position.x, wg.Position.y, wg.Hp, wg.Polarity, wg.CollisionRadius);
            actions.SpawnEntity(BossEntityId.BlackGuardian, bg.Position.x, bg.Position.y, bg.Hp, bg.Polarity, bg.CollisionRadius);

            EnterPhase(BossPhaseId.Intro);
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;

            phaseTimer += deltaTime;

            switch (CurrentPhase)
            {
                case BossPhaseId.Intro:
                    TickIntro();
                    break;
                case BossPhaseId.Phase1:
                    TickPhase1(deltaTime);
                    break;
                case BossPhaseId.Merge:
                    TickMerge();
                    break;
                case BossPhaseId.Phase2:
                    TickPhase2(deltaTime);
                    break;
                case BossPhaseId.Defeated:
                    break;
            }
        }

        public void ApplyDamage(BossEntityId id, int damage)
        {
            int idx = (int)id;
            if (!entities[idx].IsActive) return;

            if (CurrentPhase == BossPhaseId.Phase1 &&
                (id == BossEntityId.WhiteGuardian || id == BossEntityId.BlackGuardian))
            {
                entities[idx].Hp = BossCollisionCalculator.CalculateRemainingHp(entities[idx].Hp, damage);
                if (BossCollisionCalculator.IsEntityKilled(entities[idx].Hp))
                {
                    entities[idx].IsActive = false;
                    actions.DespawnEntity(id);
                    actions.AddScore(phase1KillScore);
                }
            }
            else if (CurrentPhase == BossPhaseId.Phase2 && id == BossEntityId.Magatama)
            {
                entities[idx].Hp = BossCollisionCalculator.CalculateRemainingHp(entities[idx].Hp, damage);
            }
        }

        private void TickIntro()
        {
            if (phaseTimer >= introDuration)
                EnterPhase(BossPhaseId.Phase1);
        }

        private void TickPhase1(float deltaTime)
        {
            TickPhase1Attack(deltaTime);
            TickForcedSwitch(deltaTime, phase1ForcedSwitchInterval);

            var wg = entities[(int)BossEntityId.WhiteGuardian];
            var bg = entities[(int)BossEntityId.BlackGuardian];

            if (!wg.IsActive && !bg.IsActive)
            {
                CancelWarning();
                actions.PlayMergeAnimation();
                EnterPhase(BossPhaseId.Merge);
            }
        }

        private void TickPhase1Attack(float deltaTime)
        {
            if (!rhythmClock.ShouldFireOnDownbeat(ref lastConsumedHalfBeatIndex))
                return;

            if (phaseTimer - lastFireTime < phase1ShootCooldown)
                return;

            lastFireTime = phaseTimer;
            bool simultaneous = phaseTimer >= phase1SimultaneousThreshold;

            var wg = entities[(int)BossEntityId.WhiteGuardian];
            var bg = entities[(int)BossEntityId.BlackGuardian];

            if (simultaneous)
            {
                if (wg.IsActive)
                    actions.FireBullets(BossEntityId.WhiteGuardian, wg.Position.x, wg.Position.y, wg.Polarity, 0);
                if (bg.IsActive)
                    actions.FireBullets(BossEntityId.BlackGuardian, bg.Position.x, bg.Position.y, bg.Polarity, 0);
            }
            else
            {
                bool whiteFirst = ((int)(phaseTimer / phase1ShootCooldown)) % 2 == 0;
                if (whiteFirst)
                {
                    if (wg.IsActive)
                        actions.FireBullets(BossEntityId.WhiteGuardian, wg.Position.x, wg.Position.y, wg.Polarity, 0);
                    else if (bg.IsActive)
                        actions.FireBullets(BossEntityId.BlackGuardian, bg.Position.x, bg.Position.y, bg.Polarity, 0);
                }
                else
                {
                    if (bg.IsActive)
                        actions.FireBullets(BossEntityId.BlackGuardian, bg.Position.x, bg.Position.y, bg.Polarity, 0);
                    else if (wg.IsActive)
                        actions.FireBullets(BossEntityId.WhiteGuardian, wg.Position.x, wg.Position.y, wg.Polarity, 0);
                }
            }
        }

        private void TickMerge()
        {
            if (phaseTimer >= mergeDuration)
            {
                var wgPos = entities[(int)BossEntityId.WhiteGuardian].Position;
                var bgPos = entities[(int)BossEntityId.BlackGuardian].Position;
                float2 magatamaPos = (wgPos + bgPos) * 0.5f;

                entities[(int)BossEntityId.Magatama] = new BossEntityState
                {
                    Position = magatamaPos,
                    Hp = phase2HpMagatama,
                    MaxHp = phase2HpMagatama,
                    Polarity = 0,
                    CollisionRadius = magatamaCollisionRadius,
                    IsActive = true,
                };

                magatamaOrbitRadius = math.length(magatamaPos - arenaCenter);
                magatamaAngle = math.atan2(magatamaPos.y - arenaCenter.y, magatamaPos.x - arenaCenter.x);

                actions.SpawnEntity(BossEntityId.Magatama, magatamaPos.x, magatamaPos.y,
                    phase2HpMagatama, 0, magatamaCollisionRadius);

                EnterPhase(BossPhaseId.Phase2);
            }
        }

        private void TickPhase2(float deltaTime)
        {
            magatamaAngle += magatamaRotationSpeed * deltaTime;

            float2 newPos = arenaCenter + new float2(
                math.cos(magatamaAngle) * magatamaOrbitRadius,
                math.sin(magatamaAngle) * magatamaOrbitRadius);
            entities[(int)BossEntityId.Magatama].Position = newPos;
            actions.SetEntityPosition(BossEntityId.Magatama, newPos.x, newPos.y);

            TickPhase2Attack(deltaTime);
            TickForcedSwitch(deltaTime, phase2ForcedSwitchInterval);

            if (BossCollisionCalculator.IsEntityKilled(entities[(int)BossEntityId.Magatama].Hp))
            {
                entities[(int)BossEntityId.Magatama].IsActive = false;
                actions.DespawnEntity(BossEntityId.Magatama);
                actions.AddScore(phase2KillScore);

                if (!isDefeatedNotified)
                {
                    isDefeatedNotified = true;
                    EnterPhase(BossPhaseId.Defeated);
                    IsActive = false;
                    actions.NotifyBossDefeated();
                }
            }
        }

        private void TickPhase2Attack(float deltaTime)
        {
            if (!rhythmClock.ShouldFireOnOffbeat(ref lastConsumedHalfBeatIndex))
                return;

            if (phaseTimer - lastFireTime < phase2ShootCooldown)
                return;

            lastFireTime = phaseTimer;

            var mag = entities[(int)BossEntityId.Magatama];
            actions.FireBullets(BossEntityId.Magatama, mag.Position.x, mag.Position.y, 0, 1);
        }

        private void TickForcedSwitch(float deltaTime, float interval)
        {
            if (isWarningActive)
            {
                warningTimer -= deltaTime;
                if (warningTimer <= 0f)
                {
                    isWarningActive = false;
                    actions.RequestForcedPolaritySwitch();
                    forcedSwitchTimer = interval;
                }
            }
            else
            {
                forcedSwitchTimer -= deltaTime;
                if (forcedSwitchTimer <= 0f)
                {
                    isWarningActive = true;
                    warningTimer = forcedSwitchWarningDuration;
                }
            }
        }

        private void CancelWarning()
        {
            isWarningActive = false;
        }

        private void EnterPhase(BossPhaseId phase)
        {
            CurrentPhase = phase;
            phaseTimer = 0f;
            lastFireTime = -9999f;

            if (phase == BossPhaseId.Phase1)
                forcedSwitchTimer = phase1ForcedSwitchInterval;
            else if (phase == BossPhaseId.Phase2)
                forcedSwitchTimer = phase2ForcedSwitchInterval;
        }
    }
}
