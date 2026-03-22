namespace Action002.Core
{
    public static class SeedHelper
    {
        public static uint Normalize(uint seed)
        {
            return seed == 0 ? 1u : seed;
        }

        public static uint DeriveSpawnSeed(uint runSeed)
        {
            return Normalize(runSeed ^ 0xA5A5A5A5u);
        }

        public static uint DerivePolaritySeed(uint runSeed)
        {
            return Normalize(runSeed ^ 0x5A5A5A5Au);
        }

        public static uint ResolveRunSeed(uint fixedRunSeed, uint fallbackTicks)
        {
            return fixedRunSeed != 0 ? fixedRunSeed : Normalize(fallbackTicks);
        }
    }
}
