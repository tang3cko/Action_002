using Unity.Mathematics;
using Action002.Accessory.SonicWave.Data;
using Action002.Accessory.SonicWave.Logic;
using Action002.Audio.Systems;
using Tang3cko.ReactiveSO;

namespace Action002.Accessory.SonicWave.Systems
{
    public class SonicWave : IAccessory
    {
        private readonly IRhythmClock rhythmClock;
        private readonly WaveStateSetSO waveSet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly IntVariableSO playerPolarityVar;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextWaveId = 400000;

        /// <summary>
        /// 通常レベル → 装飾品レベルのマッピングテーブル。
        /// インデックスが通常レベル、値が装飾品レベル。
        /// 例: levelUpTable[3]=1 → 通常Lv3で解禁（WaveLevel=1）
        /// </summary>
        private static readonly int[] levelUpTable = { 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 5 };
        //                                           Lv0 Lv1 Lv2 Lv3 Lv4 Lv5 Lv6 Lv7 Lv8 Lv9 Lv10

        // --- IAccessory ---
        public int Level { get; set; }
        public bool IsUnlocked => Level > 0;

        public int GetLevelForPlayerLevel(int playerLevel)
        {
            if (playerLevel < 0) return 0;
            if (playerLevel >= levelUpTable.Length) return levelUpTable[levelUpTable.Length - 1];
            return levelUpTable[playerLevel];
        }

        private readonly float baseMaxRadius;
        private readonly float baseExpandSpeed;

        public SonicWave(
            IRhythmClock rhythmClock,
            WaveStateSetSO waveSet,
            Vector2VariableSO playerPositionVar,
            IntVariableSO playerPolarityVar,
            float baseMaxRadius,
            float baseExpandSpeed)
        {
            this.rhythmClock = rhythmClock;
            this.waveSet = waveSet;
            this.playerPositionVar = playerPositionVar;
            this.playerPolarityVar = playerPolarityVar;
            this.baseMaxRadius = baseMaxRadius;
            this.baseExpandSpeed = baseExpandSpeed;
        }

        public void ProcessAttacks()
        {
            if (!IsUnlocked) return;
            if (rhythmClock == null || waveSet == null) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            int currentHalfBeat = rhythmClock.CurrentHalfBeatIndex;
            if (currentHalfBeat == lastConsumedHalfBeatIndex) return;
            lastConsumedHalfBeatIndex = currentHalfBeat;

            var beat = BeatPatternCalculator.GetCurrentBeat(currentHalfBeat);
            if (BeatPatternCalculator.IsRestBeat(beat)) return;

            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            byte polarity = (byte)playerPolarityVar.Value;
            var waveParams = SonicWaveParameterCalculator.Calculate(
                Level, baseMaxRadius, baseExpandSpeed, beat);

            WaveState wave = SonicWaveAttackCalculator.CreatePulse(
                playerPos, waveParams.MaxRadius, waveParams.Duration,
                polarity, waveParams.Damage);

            waveSet.Register(nextWaveId++, wave);
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextWaveId = 400000;
            Level = 0;
        }
    }
}
