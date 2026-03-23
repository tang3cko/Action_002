namespace Action002.Accessory.SonicWave.Logic
{
    public static class WaveBoundsCalculator
    {
        /// <summary>
        /// currentRadius が maxRadius を超えたら期限切れ。
        /// </summary>
        public static bool IsExpired(float currentRadius, float maxRadius)
        {
            return currentRadius > maxRadius;
        }
    }
}
