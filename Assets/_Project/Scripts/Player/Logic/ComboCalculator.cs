using Unity.Mathematics;
using Action002.Player.Data;
using Action002.Bullet.Logic;

namespace Action002.Player.Logic
{
    public static class ComboCalculator
    {
        public static PlayerState TickComboTimer(PlayerState state, float deltaTime)
        {
            if (state.ComboCount > 0)
            {
                state.ComboTimer -= deltaTime;

                if (state.ComboTimer <= 0f)
                {
                    state.ComboCount = 0;
                    state.ComboMultiplier = 1f;
                }
            }

            return state;
        }

        public static PlayerState IncrementCombo(PlayerState state, float bulletValue, float comboMultiplierStep, float comboTimeout, float absorbGaugeRate)
        {
            state.ComboCount++;
            state.ComboMultiplier = AbsorptionCalculator.CalculateComboMultiplier(state.ComboCount, comboMultiplierStep);
            state.ComboTimer = comboTimeout;
            state.SpinGauge = math.min(1f, state.SpinGauge + absorbGaugeRate);

            var absorbScore = (int)AbsorptionCalculator.CalculateAbsorbScore(bulletValue, state.ComboMultiplier);
            state.Score += absorbScore;

            return state;
        }
    }
}
