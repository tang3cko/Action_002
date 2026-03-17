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

        public void Execute(int index)
        {
            EnemyState state = Src[index];
            float2 direction = PlayerPos - state.Position;
            float dist = math.length(direction);

            if (dist > 0.01f)
            {
                state.Velocity = (direction / dist) * state.Speed;
                state.Position += state.Velocity * DeltaTime;
            }

            Dst[index] = state;
        }
    }
}
