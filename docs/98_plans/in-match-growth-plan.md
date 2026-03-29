# マッチ中成長要素（自動強化）設計方針

## スコープ

1プレイ内でプレイヤーが自動的に強化される仕組みを追加する。
レベルアップ時に選択UIは出さず、自動で強化が適用される（リズムを途切れさせない設計）。
装飾品システムは周回要素側のスコープであり、本タスクでは扱わない。

## レベルアップトリガー

既存の SpinGauge（0.0〜1.0）を経験値ゲージとして活用する。
- SpinGauge が 1.0 に達したらレベルアップ
- レベルアップ後に SpinGauge を 0 にリセットし、再蓄積開始
- 現在のゲージ蓄積源はそのまま活用（吸収: absorbGaugeRate=0.02、撃破: killGaugeRate=0.05）

### SpinGauge リセットの責務

SpinGauge の実体は PlayerController 内の PlayerState.SpinGauge。
レベルアップ判定とゲージリセットは **PlayerController 側** で行う:
- PlayerController が内部で PlayerGrowthCoordinator を使って判定
- SyncVariables() 前にゲージ判定→リセット→イベント発火

## 自動強化内容

レベルに応じた固定テーブル方式。

| レベル | 強化内容 |
|--------|----------|
| 1 | 弾数 +1（1→2） |
| 2 | 移動速度 +10% |
| 3 | 弾数 +1（2→3） |
| 4 | 弾速 +15% |
| 5 | 弾数 +1（3→4） |
| 6+ | 弾数は上限4固定、移動速度 +5% ずつ |

## データ構造

### PlayerGrowthState（Pure C# struct）

```csharp
public struct PlayerGrowthState
{
    public int Level;
    public int BulletCount;
    public float MoveSpeedMultiplier;
    public float BulletSpeedMultiplier;
}
```

### PlayerGrowthCalculator（Pure C#、純関数）

強化テーブルの計算のみ。

```csharp
public static class PlayerGrowthCalculator
{
    public static PlayerGrowthState CreateDefault();
    public static bool ShouldLevelUp(float spinGauge);
    public static PlayerGrowthState ApplyLevelUp(PlayerGrowthState current);
}
```

### PlayerGrowthCoordinator（Pure C#、ロジックコーディネータ）

SpinGauge 消費 → 状態更新 → 出力の統合フロー。
side-effect interface 経由で外部に通知する（Humble Object パターン）。

```csharp
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

    public PlayerGrowthCoordinator(IPlayerGrowthActions actions);
    public void CheckAndApplyGrowth(float spinGauge);
    public void Reset();
    public PlayerGrowthState CurrentState { get; }
}
```

## 統合ポイント

### PlayerController が IPlayerGrowthActions を実装

PlayerController は IPlayerGrowthActions を実装し、PlayerGrowthCoordinator を所有する:
- ResetSpinGauge(): state.SpinGauge = 0
- ApplyGrowth(state): 移動速度乗算を内部に保持、playerBulletCountVar / bulletSpeedMultiplierVar を更新
- RaiseLevelUp(level): playerLevelVar.Value = level, onPlayerLevelUp.RaiseEvent(level)

PlayerController.Update() の SyncVariables() 前に coordinator.CheckAndApplyGrowth(state.SpinGauge) を呼ぶ。

### 弾数増加 → PlayerAttack への反映

PlayerAttack は playerBulletCountVar（IntVariableSO）を毎 downbeat で読んで弾数を決定:
- bulletCount=1: 現行動作（最寄りの敵1体）
- bulletCount=2+: 最寄りの敵 N 体をそれぞれ狙う（ターゲットが足りなければ同じ敵に重複可）

PlayerAttack のコンストラクタに IntVariableSO playerBulletCountVar を追加。

### 移動速度・弾速 → VariableSO 経由

- PlayerController が growthState.MoveSpeedMultiplier を内部保持し、移動速度計算時に乗算
- PlayerAttack が FloatVariableSO bulletSpeedMultiplierVar を読んで弾速に乗算

### レベルアップイベント

IntEventChannelSO onPlayerLevelUp を新設。PlayerController（IPlayerGrowthActions.RaiseLevelUp 経由）が発火。

### HUD 連携

HudController に以下を追加:
- IntVariableSO playerLevelVar（OnEnable で初期値を読んで表示、既存パターンに準拠）
- IntEventChannelSO onPlayerLevelUp を購読して表示更新
- HudUI.uxml にレベル表示用の Label 要素を追加

### ラン開始時のリセット

PlayerController はシーン再読み込みで再生成される（Gameplay シーン内のコンポーネント）。
そのため growthState は PlayerController のフィールド初期化で自動的にリセットされる。
追加で:
- PlayerController.ResetForNewRun() 内で coordinator.Reset() を呼び、その後 IPlayerGrowthActions.ApplyGrowth(CreateDefault()) を呼んで VariableSO を初期値に書き戻す（playerLevelVar=0, playerBulletCountVar=1, bulletSpeedMultiplierVar=1f）
- GameplaySceneLifetime.Awake() で playerLevelVar / playerBulletCountVar / bulletSpeedMultiplierVar を ResetToInitial

## ファイル影響

新規:
- Player/Data/PlayerGrowthState.cs
- Player/Logic/PlayerGrowthCalculator.cs
- Player/Logic/PlayerGrowthCoordinator.cs（IPlayerGrowthActions 含む）

変更:
- Player/Systems/PlayerController.cs（IPlayerGrowthActions 実装、coordinator 所有、移動速度乗算）
- Player/Systems/PlayerAttack.cs（playerBulletCountVar / bulletSpeedMultiplierVar 対応、複数ターゲット発射）
- Player/Systems/PlayerAttackSystem.cs（PlayerAttack に新 VariableSO を渡す）
- Core/Flow/GameplaySceneLifetime.cs（playerLevelVar / playerBulletCountVar / bulletSpeedMultiplierVar のリセット追加）
- UI/HudController.cs（playerLevelVar 表示追加、onPlayerLevelUp 購読）
- UI/HudUI.uxml（レベル表示用 Label 追加）

アセット:
- IntVariableSO: PlayerLevel, PlayerBulletCount（新規作成）
- FloatVariableSO: BulletSpeedMultiplier（新規作成）
- IntEventChannelSO: OnPlayerLevelUp（新規作成）

テスト:
- PlayerGrowthCalculatorTests.cs（レベルアップ判定、各レベルの強化内容、上限挙動、デフォルト状態）
- PlayerGrowthCoordinatorTests.cs（SpinGauge 消費 → 状態更新 → RaiseLevelUp 発火、SpinGauge 未到達で何も起きない、リセット後の挙動）
- PlayerAttackTests.cs（複数弾発射の検証）
