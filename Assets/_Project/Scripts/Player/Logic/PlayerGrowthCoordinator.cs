using Action002.Player.Data;

namespace Action002.Player.Logic
{
    public interface IPlayerGrowthActions
    {
        void ResetSpinGauge();
        void ApplyGrowth(PlayerGrowthState state);
        void RaiseLevelUp(int level);
    }

    public class PlayerGrowthCoordinator
    {
        private PlayerGrowthState growthState;
        private readonly IPlayerGrowthActions actions;

        public PlayerGrowthCoordinator(IPlayerGrowthActions actions)
        {
            this.actions = actions;
            growthState = PlayerGrowthCalculator.CreateDefault();
        }

        public PlayerGrowthState CurrentState => growthState;

        public void CheckAndApplyGrowth(float spinGauge)
        {
            if (!PlayerGrowthCalculator.ShouldLevelUp(spinGauge))
                return;

            actions.ResetSpinGauge();
            growthState = PlayerGrowthCalculator.ApplyLevelUp(growthState);
            actions.ApplyGrowth(growthState);
            actions.RaiseLevelUp(growthState.Level);
        }

        public void Reset()
        {
            growthState = PlayerGrowthCalculator.CreateDefault();
        }
    }
}
