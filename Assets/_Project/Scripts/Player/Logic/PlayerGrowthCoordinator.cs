using Action002.Accessory;
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
        private AccessoryManager accessoryManager;

        public PlayerGrowthCoordinator(
            IPlayerGrowthActions actions,
            AccessoryManager accessoryManager = null)
        {
            this.actions = actions;
            this.accessoryManager = accessoryManager;
            growthState = PlayerGrowthCalculator.CreateDefault();
        }

        /// <summary>
        /// AccessoryManager を後から設定する。
        /// PlayerController が生成後、外部から注入するために使用。
        /// </summary>
        public void SetAccessoryManager(AccessoryManager manager)
        {
            accessoryManager = manager;
        }

        public PlayerGrowthState CurrentState => growthState;

        public void CheckAndApplyGrowth(float spinGauge)
        {
            if (!PlayerGrowthCalculator.ShouldLevelUp(spinGauge))
                return;

            actions.ResetSpinGauge();

            // 1. 通常攻撃のレベルアップ（独立テーブル）
            growthState = PlayerGrowthCalculator.ApplyLevelUp(growthState);
            actions.ApplyGrowth(growthState);

            // 2. 通常レベルに応じた装飾品レベルを適用
            //    各装飾品の GetLevelForPlayerLevel() が通常レベルに対応する装飾品レベルを返す。
            //    レベルアップ対象でない通常レベルでは前回と同じ値が返るため冪等。
            accessoryManager?.ApplyLevels(growthState.Level);

            actions.RaiseLevelUp(growthState.Level);
        }

        public void Reset()
        {
            growthState = PlayerGrowthCalculator.CreateDefault();
            accessoryManager?.ResetForNewRun();
        }
    }
}
