using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
            w.ElapsedTime += DeltaTime;
            float t = math.saturate(w.ElapsedTime / w.Duration);
            // OutQuart: 1 - (1-t)^4 — fast burst, gradual settle
            float inv = 1f - t;
            float eased = 1f - inv * inv * inv * inv;
            w.CurrentRadius = w.MaxRadius * eased;
            Dst[index] = w;
        }
    }
}
