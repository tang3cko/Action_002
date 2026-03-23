using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Action002.Accessory.SonicWave.Data;

namespace Action002.Accessory.SonicWave.Systems
{
    [BurstCompile]
    public struct WaveExpandJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<WaveState> Src;
        [WriteOnly] public NativeArray<WaveState> Dst;
        public float DeltaTime;

        public void Execute(int index)
        {
            var w = Src[index];
            w.CurrentRadius += w.ExpandSpeed * DeltaTime;
            Dst[index] = w;
        }
    }
}
