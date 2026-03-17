using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Action002.Bullet.Data;

namespace Action002.Bullet.Logic
{
    [BurstCompile]
    public struct BulletMoveJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<BulletState> Src;
        [WriteOnly] public NativeArray<BulletState> Dst;
        [ReadOnly] public float DeltaTime;

        public void Execute(int index)
        {
            BulletState state = Src[index];
            state.Position += state.Velocity * DeltaTime;
            state.Lifetime -= DeltaTime;
            Dst[index] = state;
        }
    }
}
