using Unity.Mathematics;

namespace Action002.Accessory.SonicWave.Logic
{
    public struct SonicWaveParameters
    {
        public float MaxRadius;
        public float Duration;
        public int Damage;
        public float ArcHalfSpread;
    }

    public static class SonicWaveParameterCalculator
    {
        /// <summary>
        /// ビート種別に応じたパラメータを返す。
        /// Duration = MaxRadius / ExpandSpeed で算出。OutQuartイージングにより
        /// 最初の25%の時間で約68%まで到達し、残りは余韻として減速する。
        /// SmallPulse: 短いDuration（スナップ感）
        /// LargePulse: 長いDuration（「てぇぇぇん」の余韻）
        /// </summary>
        public static SonicWaveParameters Calculate(int waveLevel,
            float baseMaxRadius, float baseExpandSpeed,
            SonicWaveBeat beat)
        {
            if (waveLevel < 1) waveLevel = 1;
            float maxRadius = baseMaxRadius;
            float expandSpeed = baseExpandSpeed;

            // Damage
            int damage = 1;
            if (waveLevel >= 2) damage = 2;
            if (waveLevel >= 5) damage = 3;

            // MaxRadius（レベルによるスケーリング）
            if (waveLevel >= 3) maxRadius = baseMaxRadius * 1.4f;
            if (waveLevel >= 5) maxRadius = baseMaxRadius * 1.8f;

            // ExpandSpeed（レベルによるスケーリング）
            if (waveLevel >= 4) expandSpeed = baseExpandSpeed * 1.25f;

            // ビート種別によるサイズ差
            switch (beat)
            {
                case SonicWaveBeat.SmallPulse:
                    maxRadius *= 0.6f;
                    break;
                case SonicWaveBeat.LargePulse:
                    maxRadius *= 1.2f;
                    expandSpeed *= 0.5f;
                    break;
            }

            return new SonicWaveParameters
            {
                MaxRadius = maxRadius,
                Duration = maxRadius / expandSpeed,
                Damage = damage,
                ArcHalfSpread = math.PI,
            };
        }
    }
}
