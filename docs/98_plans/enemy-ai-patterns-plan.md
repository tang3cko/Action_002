# 敵AI行動パターン・弾幕パターン追加 設計方針

## スコープ

既存の追尾型に加えて、距離維持型・定位置型の移動パターンと、旋回弾・全方位ランダム弾の弾幕パターンを追加する。

## 行動パターン

### 追尾型（既存・変更なし）
- 対象: Shooter
- 挙動: プレイヤーに向かって直進し続ける
- バジェットコスト: 1（既存通り）

### 距離維持型（新規）
- 対象: NWay
- 挙動: プレイヤーに向かって移動し、一定距離（keepDistance）に到達したらその距離を維持。プレイヤーが近づいたら後退、離れたら追従。距離維持中は横移動（strafeSign で方向固定）。
- バジェットコスト: 2（既存通り）
- パラメータ: keepDistance（EnemyTypeSpec に追加）

### 定位置型（新規）
- 対象: Ring
- 挙動: ワールド座標で指定された targetPosition に移動し、到着後停止して弾幕を撃ち続ける
- バジェットコスト: 3〜5（EnemyTypeSpec で指定）
- 同時出現制限: 最大2体（EnemySpawn 側でカウント管理）
- 目標位置: スポーン時に worldBounds（Pure C# に外から渡す矩形）の角付近から RNG で選択
- パラメータ: arrivalThreshold（停止判定距離、EnemyTypeSpec に追加）

## 弾幕パターン（新規2種）

### 旋回弾（Spiral）
- 発射角が毎発射ごとにオフセットを加算して回転していく
- 状態: 敵ごとの累積発射角を EnemyShoot 側の Dictionary で管理（ShotPatternCalculator は純関数のまま）
- ShotPatternCalculator への入力: baseAngle（累積角）を呼び出し元が計算して渡す
- ShotPatternSpec 拡張: ArcDegrees フィールドを Spiral では「1発射あたりの角度オフセット」として再利用
- パラメータ: Count（弾数）、ArcDegrees（角度オフセット）、BulletSpeed

### 全方位ランダム弾（RandomSpread）
- Ring 型ベースだが、各弾の角度と速度にランダムオフセットを加える
- 乱数源: Unity.Mathematics.Random の状態を EnemyShoot 側で敵ごとに管理し、Calculate 呼び出し時に seed として渡す（ShotPatternCalculator は rng を引数で受け取る純関数）
- ShotPatternSpec 拡張: ArcDegrees フィールドを RandomSpread では「角度ジッター範囲（度）」として再利用
- パラメータ: Count（弾数）、ArcDegrees（角度ジッター）、BulletSpeed

## データ構造の変更

### MovementPattern（新規 enum）
```csharp
public enum MovementPattern : byte
{
    Chase,       // 追尾型
    KeepDistance, // 距離維持型
    Anchor       // 定位置型
}
```

### EnemyState への追加
```csharp
public struct EnemyState
{
    // 既存フィールド
    public float2 Position;
    public float2 Velocity;
    public float Speed;
    public int Hp;
    public byte Polarity;
    public EnemyTypeId TypeId;

    // 新規フィールド
    public float2 TargetPosition;  // Anchor 型の目標位置（Chase/KeepDistance では未使用）
    public sbyte StrafeSign;       // KeepDistance 型の横移動方向（+1 or -1、スポーン時に決定）
}
```

- MovementPattern は EnemyState には持たせない。EnemyTypeSpec から TypeId 経由で取得する（型固定のため二重持ち不要）

### EnemyTypeSpec への追加
```csharp
public readonly struct EnemyTypeSpec
{
    // 既存フィールド（省略）

    // 新規フィールド
    public readonly MovementPattern Movement;
    public readonly float KeepDistance;      // KeepDistance 型: 維持する距離
    public readonly float ArrivalThreshold;  // Anchor 型: 停止判定距離
    public readonly int MaxConcurrent;       // 同時出現制限（0 = 制限なし）
    public readonly float BudgetCost;        // スポーンバジェットコスト
}
```

### ShotPatternKind への追加
```csharp
// 既存: Aimed, NWay, Ring
// 追加:
Spiral,     // 旋回弾
RandomSpread   // 全方位ランダム弾
```

### ShotPatternCalculator の拡張

純関数を維持する。新パターンの状態依存パラメータは呼び出し元が計算して引数で渡す。

```csharp
// Spiral 用: baseAngle を追加パラメータとして受け取る
static int CalculateSpiral(Span<BulletState> buffer, in ShotPatternSpec pattern,
    float2 origin, float baseAngle, byte polarity, float scoreValue)

// RandomSpread 用: rng を追加パラメータとして受け取る
static int CalculateRandomSpread(Span<BulletState> buffer, in ShotPatternSpec pattern,
    float2 origin, ref Unity.Mathematics.Random rng, byte polarity, float scoreValue)
```

### ShotPatternCalculator.Calculate のシグネチャ

既存の Calculate メソッドのシグネチャは変更しない。新パターンは個別の static メソッドとして追加し、EnemyShoot 側でパターン種別に応じて呼び分ける。

```csharp
// 既存（変更なし）
public static int Calculate(Span<BulletState> buffer, in ShotPatternSpec pattern,
    float2 origin, float2 playerPosition, byte polarity, float scoreValue)

// 新規追加
public static int CalculateSpiral(Span<BulletState> buffer, in ShotPatternSpec pattern,
    float2 origin, float baseAngle, byte polarity, float scoreValue)

public static int CalculateRandomSpread(Span<BulletState> buffer, in ShotPatternSpec pattern,
    float2 origin, ref Unity.Mathematics.Random rng, byte polarity, float scoreValue)
```

EnemyShoot.ProcessShooting 内で ShotPatternKind に応じて適切なメソッドを呼ぶ。

### EnemyShoot の状態管理

EnemyShoot が敵ごとの累積発射角と RNG 状態を Dictionary で管理する。

```csharp
// 既存
private readonly Dictionary<int, float> lastShotTimes;

// 新規追加
private readonly Dictionary<int, float> spiralAngles;          // Spiral 用累積角
private readonly Dictionary<int, Unity.Mathematics.Random> shotRngs;  // RandomSpread 用 RNG
```

## EnemyMoveJob の変更

MovementPattern に応じた分岐を追加。EnemyTypeSpec のデータは NativeArray で Job に渡す。

```csharp
[BurstCompile]
public struct EnemyMoveJob : IJobParallelFor
{
    [ReadOnly] public NativeSlice<EnemyState> Src;
    [WriteOnly] public NativeArray<EnemyState> Dst;
    [ReadOnly] public float2 PlayerPos;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public NativeArray<MovementSpec> TypeSpecs; // TypeId → MovementSpec のルックアップ

    public void Execute(int index)
    {
        // TypeId → MovementSpec を引いて分岐
    }
}
```

MovementSpec は Burst 互換の軽量 struct（MovementPattern, KeepDistance, ArrivalThreshold のみ）。

## EnemySpawn の変更（Pure C# 側に統一）

- 定位置型の同時出現数チェック: EnemyStateSetSO を走査して現在の Anchor 型敵数をカウント
- 定位置型スポーン時に targetPosition を決定: worldBounds（Rect）を外部から渡し、角付近のポジションを RNG で選択
- worldBounds は EnemySpawn のコンストラクタまたは setter で渡す（Camera 依存は MonoBehaviour 側に閉じる）
- 本タスクで EnemySpawnSystem を EnemySpawn への薄い委譲ラッパーに変更する。現在 GameLoopManager が EnemySpawnSystem.ProcessSpawning() を呼んでいるため、EnemySpawnSystem は内部で EnemySpawn に委譲する形にし、新ロジック（同時出現制限、targetPosition 決定）は EnemySpawn 側に集約する。GameLoopManager の呼び出し先（EnemySpawnSystem）は変更しない。

## テスト方針

- EnemyMoveJob の各 MovementPattern 分岐を検証する Pure C# テスト（MovementSpec をパラメータ化）
- ShotPatternCalculator の Spiral / RandomSpread テスト（baseAngle / rng を明示的に渡して再現性確認）
- EnemySpawn の同時出現制限テスト
- EnemySpawn の Anchor 目標位置決定テスト（worldBounds 内の角付近に収まることを検証）

## ファイル影響

新規:
- Enemy/Data/MovementPattern.cs
- Enemy/Data/MovementSpec.cs（Burst 互換の軽量 struct）

変更:
- Enemy/Data/EnemyState.cs（TargetPosition, StrafeSign 追加）
- Enemy/Data/EnemyTypeSpec.cs（Movement, KeepDistance, ArrivalThreshold, MaxConcurrent, BudgetCost 追加）
- Enemy/Logic/EnemyMoveJob.cs（MovementPattern 分岐追加）
- Enemy/Logic/EnemyTypeTable.cs（新パラメータ追加）
- Enemy/Systems/EnemySpawn.cs（同時出現制限、targetPosition 決定、worldBounds 受け取り）
- Enemy/Systems/EnemySpawnSystem.cs（EnemySpawn への薄い委譲ラッパーに変更）
- Bullet/Data/ShotPatternKind.cs（Spiral, RandomSpread 追加）
- Bullet/Logic/ShotPatternCalculator.cs（CalculateSpiral, CalculateRandomSpread 追加。既存 Calculate は変更なし）
- Bullet/Systems/EnemyShoot.cs（spiralAngles, shotRngs の状態管理、パターン種別による呼び分け）
- Core/GameLoopManager.cs（EnemyMoveJob への MovementSpec NativeArray 生成・設定・Dispose 追加）

既存テスト修正:
- Tests/EditMode/Enemy/EnemySpawnTests.cs（worldBounds パラメータ追加、同時出現制限テスト追加）
- Tests/EditMode/Bullet/EnemyShootTests.cs（新パターンのテスト追加）
- Tests/EditMode/Bullet/ShotPatternCalculatorTests.cs（Spiral, RandomSpread テスト追加）
