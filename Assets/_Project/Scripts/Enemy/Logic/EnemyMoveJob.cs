using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Action002.Enemy.Data;

namespace Action002.Enemy.Logic
{
    [BurstCompile]
    public struct EnemyMoveJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<EnemyState> Src;
        [WriteOnly] public NativeArray<EnemyState> Dst;
        [ReadOnly] public float2 PlayerPos;
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public NativeArray<MovementSpec> TypeSpecs;

        public void Execute(int index)
        {
            EnemyState state = Src[index];
            int typeIndex = (int)state.TypeId;
            MovementSpec spec = TypeSpecs[typeIndex];

            switch (spec.Pattern)
            {
                case MovementPattern.Chase:
                    ExecuteChase(ref state);
                    break;
                case MovementPattern.KeepDistance:
                    ExecuteKeepDistance(ref state, spec.KeepDistance);
                    break;
                case MovementPattern.Anchor:
                    ExecuteAnchor(ref state, spec.ArrivalThreshold);
                    break;
            }

            if (spec.RotationSpeed > 0f)
            {
                float direction = state.Velocity.x >= 0f ? -1f : 1f;
                state.RotationAccumulator += direction * spec.RotationSpeed * DeltaTime;

                if (spec.StepAngle > 0f)
                {
                    // Stepped "gakon" rotation: hold → OutQuart snap → hold
                    float absAccum = math.abs(state.RotationAccumulator);
                    float sign = state.RotationAccumulator >= 0f ? 1f : -1f;
                    float rawSteps = absAccum / spec.StepAngle;
                    float stepIndex = math.floor(rawSteps);
                    float progress = rawSteps - stepIndex;

                    float t;
                    if (progress < spec.HoldRatio)
                    {
                        t = 0f;
                    }
                    else
                    {
                        float snapProgress = (progress - spec.HoldRatio) / (1f - spec.HoldRatio);
                        float inv = 1f - snapProgress;
                        t = 1f - inv * inv * inv * inv; // OutQuart
                    }
                    state.RotationAngle = sign * (stepIndex + t) * spec.StepAngle;
                }
                else
                {
                    state.RotationAngle = state.RotationAccumulator;
                }
            }

            Dst[index] = state;
        }

        private void ExecuteChase(ref EnemyState state)
        {
            float2 direction = PlayerPos - state.Position;
            float dist = math.length(direction);

            if (dist > 0.01f)
            {
                state.Velocity = (direction / dist) * state.Speed;
                state.Position += state.Velocity * DeltaTime;
            }
        }

        private void ExecuteKeepDistance(ref EnemyState state, float keepDistance)
        {
            float2 toPlayer = PlayerPos - state.Position;
            float dist = math.length(toPlayer);

            if (dist < 0.01f) return;

            float2 dirToPlayer = toPlayer / dist;

            if (dist > keepDistance + 0.5f)
            {
                // まだ遠い: プレイヤーに向かって移動
                state.Velocity = dirToPlayer * state.Speed;
            }
            else if (dist < keepDistance - 0.5f)
            {
                // 近すぎ: 後退
                state.Velocity = -dirToPlayer * state.Speed;
            }
            else
            {
                // 距離維持中: 横移動（strafeSign で方向固定）
                float2 perpendicular = new float2(-dirToPlayer.y, dirToPlayer.x);
                state.Velocity = perpendicular * state.StrafeSign * state.Speed * 0.5f;
            }

            state.Position += state.Velocity * DeltaTime;
        }

        private void ExecuteAnchor(ref EnemyState state, float arrivalThreshold)
        {
            float2 toTarget = state.TargetPosition - state.Position;
            float dist = math.length(toTarget);

            if (dist > arrivalThreshold)
            {
                float moveAmount = state.Speed * DeltaTime;
                if (moveAmount >= dist)
                {
                    // 今回の移動量で到達 → 目標位置に吸着
                    state.Position = state.TargetPosition;
                    state.Velocity = float2.zero;
                }
                else
                {
                    float2 dir = toTarget / dist;
                    state.Velocity = dir * state.Speed;
                    state.Position += state.Velocity * DeltaTime;
                }
            }
            else
            {
                // 到着: 停止
                state.Velocity = float2.zero;
            }
        }
    }
}
