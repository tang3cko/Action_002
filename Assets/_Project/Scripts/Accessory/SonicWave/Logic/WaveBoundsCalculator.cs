namespace Action002.Accessory.SonicWave.Logic
{
    public static class WaveBoundsCalculator
    {
        /// <summary>
        /// elapsedTime が duration 以上になったら期限切れ。
        /// </summary>
        public static bool IsExpired(float elapsedTime, float duration)
        {
            return elapsedTime >= duration;
        }
    }
}
