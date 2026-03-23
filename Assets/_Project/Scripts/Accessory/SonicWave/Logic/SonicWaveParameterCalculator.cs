using Unity.Mathematics;

namespace Action002.Accessory.SonicWave.Logic
{
    public struct SonicWaveParameters
    {
        public float MaxRadius;
        public float ExpandSpeed;
        public int Damage;
        public float ArcHalfSpread;
    }

    public static class SonicWaveParameterCalculator
    {
        /// <summary>
        /// ビート種別に応じたパラメータを返す。
        /// SmallPulse: MaxRadius を小さめ (×0.6)、通常の ExpandSpeed
        /// LargePulse: MaxRadius を大きめ (×1.2)、ExpandSpeed を遅く (×0.5) して余韻を表現
        /// </summary>
        public static SonicWaveParameters Calculate(int waveLevel,
            float baseMaxRadius, float baseExpandSpeed,
            SonicWaveBeat beat)
        {
            if (waveLevel < 1) waveLevel = 1;
            var p = new SonicWaveParameters
            {
                MaxRadius = baseMaxRadius,
                ExpandSpeed = baseExpandSpeed,
                Damage = 1,
                ArcHalfSpread = math.PI,  // 全方位（円パルス）
            };

            // Damage
            if (waveLevel >= 2) p.Damage = 2;
            if (waveLevel >= 5) p.Damage = 3;

            // MaxRadius（レベルによるスケーリング）
            if (waveLevel >= 3) p.MaxRadius = baseMaxRadius * 1.4f;
            if (waveLevel >= 5) p.MaxRadius = baseMaxRadius * 1.8f;

            // ExpandSpeed（レベルによるスケーリング）
            if (waveLevel >= 4) p.ExpandSpeed = baseExpandSpeed * 1.25f;

            // ビート種別によるサイズ差
            switch (beat)
            {
                case SonicWaveBeat.SmallPulse:
                    p.MaxRadius *= 0.6f;
                    break;
                case SonicWaveBeat.LargePulse:
                    p.MaxRadius *= 1.2f;
                    p.ExpandSpeed *= 0.5f;  // 遅くして「てぇぇぇん」の余韻を表現
                    break;
            }

            return p;
        }
    }
}
