namespace Action002.Accessory
{
    /// <summary>
    /// 装飾品の共通インターフェース。
    /// 各装飾品はこのインターフェースを実装する Pure C# class。
    /// MonoBehaviour ではない。
    /// </summary>
    public interface IAccessory
    {
        /// <summary>装飾品レベル。0 = 未解禁。1以上で動作する。</summary>
        int Level { get; set; }

        /// <summary>装飾品が解禁されているか。</summary>
        bool IsUnlocked { get; }

        /// <summary>
        /// 通常レベルに対応する装飾品レベルを返す。
        /// レベルアップテーブルに基づき、未解禁なら 0、解禁後は対応するレベルを返す。
        /// </summary>
        int GetLevelForPlayerLevel(int playerLevel);

        /// <summary>
        /// 毎フレーム呼ばれる攻撃処理。
        /// IRhythmClock 経由でビートを判定し、攻撃を生成する。
        /// SE 再生等の副作用は呼び出し側（MonoBehaviour）が担当する。
        /// </summary>
        void ProcessAttacks();

        /// <summary>ラン開始時のリセット。</summary>
        void ResetForNewRun();
    }
}
