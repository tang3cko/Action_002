# セーブシステム設計方針

## スコープ

Layer 1（最低限）+ Layer 3（統計）を一括実装する。
Layer 2（装飾品アンロック等）は装飾品システム実装時に構造を拡張する。

## セーブデータ構造

```csharp
[Serializable]
public struct SaveData
{
    // Layer 1
    public int HighScore;
    public bool TutorialCompleted;

    // Layer 3
    public int BestCombo;
    public int TotalKills;
    public int TotalAbsorptions;

    // バージョン管理（将来の構造変更・マイグレーション用）
    // 正常値は Version=1 のみ。保存時は常に Version=1 を書き込む（新規・復旧・マイグレーション後すべて）。
    // Version != 1（0含む、>1含む）は異常として self-heal: デフォルト値 Version=1 で新規生成して上書き保存（データ消失前提の設計）
    // ただし旧キーあり・新キーなし（Version=0に相当）のみ例外: one-shot migration で旧データを引き継ぐ
    public int Version;
}
```

フィールド命名は PascalCase（既存コードベースに準拠）。

## 永続化方式

- JsonUtility でシリアライズし、PlayerPrefs に単一キーで保存
- PlayerPrefs.Save() はチェックポイント単位で呼び出す
  - ラン終了時（結果確定）: 1回だけ
  - チュートリアル完了時: 1回だけ
- データ消失前提の設計（なくても遊べる、デフォルト値で動作する）

## キーマイグレーション

- 旧キー: `HasCompletedAwakeningTutorial`（FirstPlayFlagRepository）
- 新 JSON キーが空で旧キーが存在する場合: one-shot migration を行い、**新 JSON を PlayerPrefs に書き込んで PlayerPrefs.Save() を呼んでから**旧キーを削除する（保存成功後に旧キー削除）
- 壊れた JSON / 空文字の場合: デフォルト値（Version=1）で新規生成
- Version フィールドで将来の追加フィールドに対応

## アーキテクチャ

### SaveData（struct、Pure C#）

セーブデータ構造体。JsonUtility でシリアライズ可能な形式。

### ISaveDataRepository（interface、Pure C#）

```csharp
public interface ISaveDataRepository
{
    SaveData Load();
    void Save(SaveData data);
}
```

PlayerPrefs への依存を1箇所に集約し、テスト時にフェイク差し替え可能にする。

### SaveDataRepository（ScriptableObject）

- ISaveDataRepository の PlayerPrefs 実装
- ScriptableObject にすることで Inspector 注入・Asset-based DI に対応（既存の FirstPlayFlagRepository と同じ流儀）
- Load():
  1. 旧キーあり・新キーなし → one-shot migration（旧データを引き継いで Version=1 で新JSON保存 → PlayerPrefs.Save() → 旧キー削除）
  2. 旧キーなし・新キーなし（初回起動）→ デフォルト値（Version=1）を返す（保存はしない）
  3. 新キーあり・JSON 破損/空文字 → デフォルト値（Version=1）で新規生成して即 Save（self-heal）
  4. Version != 1（上記以外の異常値）→ デフォルト値（Version=1）で新規生成して即 Save（self-heal）
  5. 正常（Version=1）: デシリアライズして返す
- Save(SaveData): シリアライズ → PlayerPrefs 書き込み → PlayerPrefs.Save()

### SaveDataService（Pure C#、Humble Object ロジック）

- ISaveDataRepository を受け取り、まとめた操作を提供
- UI 層には持たせない。ロジック層のみが呼び出す

```csharp
// ラン終了時に一括で更新・保存（内部で1回のSave）
void ApplyRunResult(int finalScore, int maxCombo, int killCount, int absorptionCount);

// チュートリアル完了（1回のSave）
void MarkTutorialCompleted();

// 読み取り（Save不要）
bool IsTutorialCompleted();
SaveData GetCurrentData();
```

### ラン中統計の追跡（Variable SO で保持）

ランをまたぐシーン境界の問題を避けるため、ラン中統計は既存の `IntVariableSO` パターンで管理する。
GameFlowController が各 VariableSO を SerializeField で持ち、結果確定時に読む。

| VariableSO | 更新元 | 更新タイミング |
|------------|--------|----------------|
| `maxComboVar` | onComboIncremented（BulletCollision が発火）| 受信時に comboCountVar の現在値を読んで最大値を更新 |
| `runKillCountVar` | onEnemyKilled（BulletCollision が発火）| 敵撃破のたびにインクリメント |
| `runAbsorptionCountVar` | onComboIncremented（BulletCollision が発火）| 吸収のたびにインクリメント |

- GameplaySceneLifetime.Awake() でこれら3つも ResetToInitial（既存の scoreVar, comboCountVar と同じ扱い）
- 各 VariableSO を更新する薄い MonoBehaviour（`RunSessionStatsCollector`）は Gameplay シーン内に配置し、シーン常駐の GameFlowController には非依存

### 統合ポイント（IGameFlowActions 経由）

`IGameFlowActions` に以下を追加する:

```csharp
void CommitRunResult();   // 引数なし: GameFlowController が VariableSO から全値を読んで SaveDataService を呼ぶ
```

- `CommitRunResult()` は GameFlowController.CommitRunResult() で実装
  - scoreVar / maxComboVar / runKillCountVar / runAbsorptionCountVar を読んで SaveDataService.ApplyRunResult(...) を呼ぶ
- `HandleGameOver()` / `HandleBossDefeated()` 内で `actions.CommitRunResult()` を呼ぶ（1回のみ）
- **二重保存の防止**: `GameFlowLogic` に `bool _resultCommitted` フラグを持ち、すでに `CommitRunResult()` 済みなら再呼び出しをスキップする。リセット時（`HandleResultRetrySelected` / `HandleResultBackToTitleSelected`）にフラグをクリアする

### チュートリアルフラグの読み取り

Tutorial は Gameplay シーン内の `TutorialSequenceController` で動作し、完了時に `onTutorialCompleted` を発火する。`GameFlowLogic.HandleTutorialCompleted()` がそれを受けて `PendingPhase = Title` に遷移する（既存実装通り）。設計書での `GameFlowLogic` の変更は不要。

チュートリアルスキップ判定は**本タスクのスコープ外**とする。本タスクでは `SaveDataRepository.Load().TutorialCompleted` でスキップ判定に必要なデータが提供できる状態にすることのみを目標とする。判定責務・統合地点・遷移分岐の設計は別途対応する。

### ResultScreenController

保存処理を持たない（表示専任）。

### 既存コードの変更

- `FirstPlayFlagRepository` → `SaveDataRepository` に置き換え
- `GameFlowController` に `SaveDataRepository` を SerializeField で Inspector 注入、SaveDataService を生成
- `GameFlowController` に `maxComboVar` / `runKillCountVar` / `runAbsorptionCountVar` の SerializeField を追加
- `IGameFlowActions` に `CommitRunResult()` を追加
- `GameplaySceneLifetime` にリセット対象 VariableSO を3つ追加
- TutorialSequenceController の統合はスコープ外（別途対応）

## ファイル配置

```
Assets/_Project/Scripts/Core/Save/
  SaveData.cs                  # データ構造体
  ISaveDataRepository.cs       # インターフェース
  SaveDataRepository.cs        # ScriptableObject / PlayerPrefs実装
  SaveDataService.cs           # ビジネスロジック（Pure C#）
  RunSessionStatsCollector.cs  # onEnemyKilled/onComboIncrementedを購読し runKillCountVar/runAbsorptionCountVar/maxComboVar を更新する薄いMonoBehaviourラッパー（Gameplayシーン内に配置）。maxComboVar はonComboIncremented受信時にcomboCountVarを読んで最大値を更新
```

名前空間: `Action002.Core.Save`（既存 FirstPlayFlagRepository に準拠）

## テスト方針

- `SaveDataService` のロジック（ハイスコア更新判定・統計加算・ApplyRunResult）を EditMode テストで検証
- `ISaveDataRepository` をフェイクで差し替えてテスト（PlayerPrefs 非依存）
- `SaveDataRepository.Load()` の分岐を EditMode テストで検証
  - キーなし → デフォルト値（Version=1）で新規生成
  - 旧キーあり・新キーなし → one-shot migration して Version=1 で保存
  - 壊れた JSON / 空文字 → デフォルト値（Version=1）で self-heal 保存
  - Version != 1（未知・異常フォーマット）→ デフォルト値（Version=1）で新規生成して self-heal 保存
- `GameFlowLogic` の `HandleGameOver()` / `HandleBossDefeated()` で `CommitRunResult()` が呼ばれることを Spy で検証
- 二重保存防止: 連続して `HandleGameOver()` を呼んでも `CommitRunResult()` は1回だけ呼ばれること
- フラグリセット: `HandleResultRetrySelected()` / `HandleResultBackToTitleSelected()` 後に再度 `HandleGameOver()` を呼ぶと再び `CommitRunResult()` が呼ばれること

## 制約

- PlayerPrefs 上限 1MB（WebGL）— スカラー値のみなので問題なし
- Bundle Identifier を変更しない限りデータ継続
- ブラウザキャッシュクリアで消える前提
