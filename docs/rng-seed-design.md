# RNG Seed分離 + SeedHelper + 固定Seed 設計方針

## 目的
1. spawnRng/polarityRng を分離し、敵タイプ追加・削除で極性列が変動しないようにする
2. SeedHelper で seed=0 保護と派生ロジックを一元化する
3. 当面は固定Seed でランキング公平性を担保する

## 変更対象

### 新規作成

#### `Assets/_Project/Scripts/Core/SeedHelper.cs`
- Pure C# static class（SO不要）
- `Normalize(uint seed)` → `seed == 0 ? 1u : seed`
- `DeriveSpawnSeed(uint runSeed)` → `Normalize(runSeed ^ 0xA5A5A5A5u)`
- `DerivePolaritySeed(uint runSeed)` → `Normalize(runSeed ^ 0x5A5A5A5Au)`
- `ResolveRunSeed(uint fixedRunSeed, uint fallbackTicks)` → `fixedRunSeed != 0 ? fixedRunSeed : Normalize(fallbackTicks)`
  - seed解決ロジックをPure C#に集約し、テスト可能にする
- namespace: `Action002.Core`

#### `Assets/_Project/Tests/EditMode/Core/SeedHelperTests.cs`
- Normalize: 0→1, 非0→そのまま
- DeriveSpawnSeed/DerivePolaritySeed: 同一runSeedから異なるseedが出ること
- Derive結果が0にならないこと（XOR結果が0になるケースでNormalizeが効くこと）
- 同一入力→同一出力の決定論性
- ResolveRunSeed: fixedRunSeed != 0 → fixedRunSeedをそのまま返す
- ResolveRunSeed: fixedRunSeed == 0 → fallbackTicksを返す（0の場合は1に正規化）

### 修正

#### `Assets/_Project/Scripts/Core/GameConfigSO.cs`
- `[Header("Seed")]` セクション追加
- `[SerializeField] private uint fixedRunSeed = 12345u;`
- `public uint FixedRunSeed => fixedRunSeed;`
- 0 = ランダム（DateTime.Now.Ticks）、非0 = 固定

#### `Assets/_Project/Scripts/Enemy/Systems/EnemySpawn.cs`
- `private Unity.Mathematics.Random rng;` → `spawnRng` + `polarityRng` に分離
- コンストラクタ: `rngSeed` パラメータ名を `runSeed` に変更
  - `spawnRng = new Random(SeedHelper.DeriveSpawnSeed(runSeed));`
  - `polarityRng = new Random(SeedHelper.DerivePolaritySeed(runSeed));`
- `ResetForNewRun(uint newSeed)` → `ResetForNewRun(uint runSeed)` に変更、同様に2本派生
- `SpawnEnemy()`: 極性決定のみ `polarityRng.NextFloat()` を使用
  - L78: `SpawnCalculator.GetRandomPolarity(polarityRng.NextFloat())`
- その他（角度、タイプ選択、速度、ストレイフ、アンカー座標）は全て `spawnRng` を使用
- seed=0 の重複ガードを削除（SeedHelper.Normalize に一元化済み）

#### `Assets/_Project/Scripts/Enemy/Systems/EnemySpawnSystem.cs`
- `logic` を遅延生成に変更。`Start()` での即時生成を廃止
  - `Awake()`: `mainCamera = Camera.main;` のみ（seed非依存の依存解決）
  - `logic` は `ResetForNewRun(runSeed)` で初回生成される
- `ResetForNewRun()` → `ResetForNewRun(uint runSeed)` にシグネチャ変更
  - `logic` が null なら新規生成、既存なら `logic.ResetForNewRun(runSeed)` を呼ぶ
  - `DateTime.Now.Ticks` 自力生成を廃止、受け取ったrunSeedをそのまま渡す
  - seed解決の責務は `GameplaySceneLifetime` に一元化
  - seed=0 ガードを削除（SeedHelper.Normalize に委譲）
- `ProcessSpawning()`: `logic == null` の場合は early return（既存の null チェックで対応済み）
  - `ResetForNewRun` が呼ばれるまでスポーンは一切発生しない

#### `Assets/_Project/Scripts/Core/Flow/GameplaySceneLifetime.cs`
- `[Header("Config")]` セクション追加
  - `[SerializeField] private GameConfigSO gameConfig;`
- `[Header("Systems")]` に `EnemySpawnSystem` 追加
  - `[SerializeField] private EnemySpawnSystem enemySpawnSystem;`（既存のrhythmClockSystemと並列）
- `ResetForNewRun()` 実装を拡張:
  - `uint runSeed = SeedHelper.ResolveRunSeed(gameConfig.FixedRunSeed, (uint)System.DateTime.Now.Ticks);`
  - `enemySpawnSystem?.ResetForNewRun(runSeed);`
  - `rhythmClockSystem?.ResetForNewRun();` は変更なし（RNG不使用）
- `OnValidate()` に gameConfig / enemySpawnSystem の null チェック追加

#### `Assets/_Project/Tests/EditMode/Enemy/EnemySpawnTests.cs`
- コンストラクタ呼び出しの `rngSeed:` → `runSeed:` に変更（全箇所）
- 決定論性テスト: 同一runSeedで同一結果になることを維持
- RNG分離テスト追加（極性列スナップショットテスト）:
  - 既知seed（例: runSeed=42）で10体スポーンし、極性列のハードコード期待値と一致することを確認
  - spawnRng側の消費パターンが変わっても極性列が不変であることを保証する回帰テスト
  - 将来polarityRngをspawnRngに戻した場合に確実にテストが失敗する設計
- ResetForNewRun再初期化契約テスト追加:
  - 数体スポーンして内部RNG状態を進めた後、同じrunSeedで `ResetForNewRun(runSeed)` を呼び、その後のspawn結果列（位置+極性）が初回実行と完全一致することを確認
  - spawnRng/polarityRng の両方が正しく再初期化されることの回帰検知

#### `Assets/_Project/Tests/EditMode/Core/GameplayStartupLogicTests.cs`
- `StubGameplayStartupActions.ResetForNewRun()` は変更不要
  （IGameplayStartupActions のシグネチャは変更しない — seedの生成・配布はGameplaySceneLifetime内部の実装詳細）

### 変更しないファイル
- `IGameplayStartupActions.cs` — ResetForNewRun() のシグネチャは変更しない。seed管理はGameplaySceneLifetimeの実装詳細であり、インターフェース境界に漏らさない
- `GameplayStartupLogic.cs` — pure C#ロジックに変更なし
- `RhythmClockSystem/RhythmClock` — RNG不使用、変更不要
- `PlayerAttack/PlayerAttackSystem` — RNG不使用、変更不要
- `EnemyShoot/EnemyShootSystem` — 独立したper-enemy RNG、今回のスコープ外

## テスト方針
1. SeedHelperTests: Normalize/Derive系の境界値テスト + ResolveRunSeed（fixedRunSeed有無の分岐）
2. EnemySpawnTests: 既存テストが全てパスすること + 極性列スナップショットテスト（既知seedに対するハードコード期待値で回帰検出）
3. GameplayStartupLogicTests: 変更なしで全パス（インターフェース不変）
4. seed配布経路: `SeedHelper.ResolveRunSeed` のPure C#テストでseed解決ロジックを検証。`EnemySpawnTests` の `ResetForNewRun` 再初期化契約テストでseed注入後のRNG再初期化を検証。この2層で「seed解決→RNG初期化」のPure C#チェーンを網羅する。`GameplaySceneLifetime` → `EnemySpawnSystem` のMonoBehaviour間配線はPlay Modeテストの領域であり、今回のEdit Modeスコープ外とする

## 設計判断の根拠
- **IGameplayStartupActionsを変更しない理由**: seed管理はMonoBehaviour実装の詳細。Pure C#のGameplayStartupLogicがseedを知る必要はなく、テストスタブも影響を受けない
- **XOR派生の理由**: 単純で決定論的。同一runSeedから2つの十分に異なるseedを生成できる。暗号的強度は不要（ゲーム用途）
- **GameConfigSOにfixedRunSeedを置く理由**: 既存のSO。新SOを作る必要がない。Inspector上で設定変更可能
- **MonoBehaviour間配線をEdit Modeテスト対象外とする理由**: seed解決(`ResolveRunSeed`)とRNG再初期化(`EnemySpawn.ResetForNewRun`)の両方がPure C#でテスト済み。MonoBehaviour間の配線ミス（呼び忘れ等）はPlay Modeテストの責務であり、Edit Modeスコープに含めない
