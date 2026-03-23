using System.Collections.Generic;

namespace Action002.Accessory
{
    /// <summary>
    /// 装飾品の管理。レベル適用・一括リセットを担当する。
    /// 攻撃処理のディスパッチは各装飾品の MonoBehaviour ラッパーが担当する。
    /// </summary>
    public class AccessoryManager
    {
        private readonly List<IAccessory> accessories = new List<IAccessory>(4);

        public void Register(IAccessory accessory)
        {
            accessories.Add(accessory);
        }

        /// <summary>
        /// 通常レベルに応じて各装飾品のレベルを適用する。
        /// 各装飾品が持つレベルアップテーブルに基づき、対応する装飾品レベルをセットする。
        /// PlayerGrowthCoordinator から通常レベルアップ後に呼ばれる。
        /// </summary>
        public void ApplyLevels(int playerLevel)
        {
            for (int i = 0; i < accessories.Count; i++)
            {
                int accessoryLevel = accessories[i].GetLevelForPlayerLevel(playerLevel);
                accessories[i].Level = accessoryLevel;
            }
        }

        /// <summary>全装飾品をリセットする。</summary>
        public void ResetForNewRun()
        {
            for (int i = 0; i < accessories.Count; i++)
            {
                accessories[i].ResetForNewRun();
            }
        }
    }
}
