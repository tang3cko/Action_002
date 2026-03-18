using Unity.Mathematics;
using Action002.Player.Data;

namespace Action002.Player.Logic
{
    public static class ScoreCalculator
    {
        public static PlayerState AddKillScore(PlayerState state, int baseScore, float killGaugeRate)
        {
            state.Score += baseScore;
            state.SpinGauge = math.min(1f, state.SpinGauge + killGaugeRate);
            return state;
        }
    }
}
