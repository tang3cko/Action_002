# 装飾品（アクセサリー）システム設計方針

## 1. 概要

プレイヤーの攻撃を「基本攻撃」と「装飾品（アクセサリー）」の2層に分ける。

- **基本攻撃（PlayerAttack）**: 直線弾、毎表拍。常に動作。一切変更しない。
- **装飾品**: レベルアップで解禁される追加攻撃。各装飾品が独自のリズムパターンで攻撃し、同時に動くことで音楽になっていく。

装飾品は最大2-3個を想定。今回実装するのは第一弾「ソニックウェーブ」のみ。

### ソニックウェーブのリズムパターン（4拍1セット、表拍）
- Beat 0（とぅん）: 右方向ソニックウェーブ（扇状）
- Beat 1（て）: 左方向ソニックウェーブ（扇状）
- Beat 2（てぇん）: 全方位パルス（円形）
- Beat 3: 休符（攻撃なし）

### 各ビートのSE
- Beat 0（右ウェーブ）: 固有AudioClip
- Beat 1（左ウェーブ）: 固有AudioClip
- Beat 2（パルス）: 固有AudioClip

### 通常攻撃との関係
- **PlayerAttack.cs は一切変更しない。** 既存の直線弾はそのまま動作し続ける。
- **PlayerAttackSystem.cs は一切変更しない。** DI構成もそのまま。
- **BulletStateSetSO, BulletState 等の弾関連は一切変更しない。**
- **PlayerGrowthState の BulletCount, BulletSpeedMultiplier は残す。** 通常攻撃の強化は既存通り動作する。

## 2. 装飾品システムの共通基盤

### 2.1 IAccessory（新規 Pure C# interface）

```csharp
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
        /// 戻り値は「何が起きたか」を示す任意の値。
        /// SE 再生等の副作用は呼び出し側（MonoBehaviour）が担当する。
        /// </summary>
        void ProcessAttacks();

        /// <summary>ラン開始時のリセット。</summary>
        void ResetForNewRun();
    }
}
```

**設計根拠:**
- `ProcessAttacks()` の戻り値を void にする。SE再生など副作用のトリガーは、各装飾品の MonoBehaviour ラッパー側が具体的な Coordinator のメソッドを直接呼んで処理する。IAccessory はポリモーフィックな管理（一括更新、一括リセット）のためのインターフェース。
- Level の set を公開し、AccessoryManager がレベルアップ時に外部から設定する。
- Pure C# interface のため MonoBehaviour に依存しない。テスタブル。

### 2.2 AccessoryManager（新規 Pure C# class）

```csharp
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
```

**設計根拠:**
- AccessoryManager は攻撃処理のディスパッチを行わない。各装飾品の MonoBehaviour ラッパー（例: SonicWaveSystem）が ProcessAttacks + SE再生を一体で行うため、GameLoopManager から各 System を直接呼ぶ。
- AccessoryManager は装飾品レベルの適用を担当する。ApplyLevels(playerLevel) で各装飾品のレベルアップテーブルに基づいた装飾品レベルをセットする。
- 各装飾品が GetLevelForPlayerLevel(int playerLevel) を持ち、「通常レベル → 装飾品レベル」のマッピングを定義する。装飾品レベルが変化しない通常レベルでは前回と同じ値が返るため、Level のセットは冪等。
- 通常攻撃のレベルアップテーブル（PlayerGrowthCalculator）と装飾品のレベルアップは完全に独立。PlayerGrowthCalculator は通常攻撃パラメータのみを扱い、装飾品レベルは AccessoryManager が管理する。
- List の初期容量 4 は想定最大数（2-3）に余裕を持たせた値。

### 2.3 装飾品と GameLoopManager の関係

各装飾品の MonoBehaviour ラッパー（SonicWaveSystem 等）は GameLoopManager の SerializeField として登録され、LateUpdate から直接呼ばれる。

```
GameLoopManager.LateUpdate:
    ...
    5. RhythmClock.ProcessClock()
    6. PlayerAttack.ProcessAttacks()       // 既存（通常攻撃、変更なし）
    7. SonicWave.ProcessAttacks()          // 新規（ソニックウェーブ生成 + SE再生）
    8. BulletCollision.ProcessCollisions()  // 既存（変更なし）
    ...
```

各装飾品固有の処理（Wave の衝突判定、削除等）も、装飾品ごとの System が個別に GameLoopManager から呼ばれる。

### 2.4 装飾品のレベル管理フロー

装飾品のレベルは通常攻撃とは独立したツリーで管理される。
装飾品ごとにレベルアップテーブル（通常レベル → 装飾品レベルのマッピング）を持ち、特定の通常レベルでのみ装飾品レベルが変化する。

```
SpinGauge が 1.0 に達する
  → PlayerGrowthCoordinator.CheckAndApplyGrowth()
    → PlayerGrowthCalculator.ApplyLevelUp(growthState)  // 通常攻撃のみ（BulletCount等）
    → actions.ApplyGrowth(growthState)                   // 通常攻撃パラメータを反映
    → accessoryManager.ApplyLevels(growthState.Level)    // 通常レベルに応じた装飾品レベルを適用
    → actions.RaiseLevelUp(level)
```

各装飾品は GetLevelForPlayerLevel(int playerLevel) で「通常レベル → 装飾品レベル」を返す。
未解禁の通常レベルでは 0 を返し、レベルアップ対象でない通常レベルでは前回と同じ装飾品レベルを返す。

```
ソニックウェーブのレベルアップテーブル例:
  通常Lv1: WaveLevel=0（未解禁）
  通常Lv2: WaveLevel=0（未解禁）
  通常Lv3: WaveLevel=1（解禁）
  通常Lv4: WaveLevel=1（変化なし）
  通常Lv5: WaveLevel=2
  通常Lv6: WaveLevel=2（変化なし）
  通常Lv7: WaveLevel=3
  通常Lv8: WaveLevel=3（変化なし）
  通常Lv9: WaveLevel=4
  通常Lv10: WaveLevel=5
```

**将来の装飾品が増えた場合:**
AccessoryManager.Register() で新しい装飾品を登録し、その装飾品に固有のレベルアップテーブルを持たせるだけ。PlayerGrowthCalculator と PlayerGrowthState の変更は不要。

## 3. リズムパターンの管理

### 3.1 BeatPatternCalculator（新規 Pure C# static class）

```csharp
namespace Action002.Accessory.SonicWave.Logic
{
    public enum SonicWaveBeat : byte
    {
        RightWave = 0,
        LeftWave  = 1,
        Pulse     = 2,
        Rest      = 3,
    }

    public static class BeatPatternCalculator
    {
        public const int PATTERN_LENGTH = 4;

        /// <summary>
        /// currentHalfBeatIndex から表拍の通算インデックスを算出し、
        /// 4拍パターン内のビート種別を返す。
        /// フレーム落ちで表拍を飛ばしても音楽上の拍位置と常に同期する。
        /// </summary>
        public static SonicWaveBeat GetCurrentBeat(int currentHalfBeatIndex)
        {
            int downbeatIndex = currentHalfBeatIndex / 2;
            int beatInPattern = downbeatIndex % PATTERN_LENGTH;
            return (SonicWaveBeat)beatInPattern;
        }

        public static bool IsRestBeat(SonicWaveBeat beat)
        {
            return beat == SonicWaveBeat.Rest;
        }
    }
}
```

**設計根拠:**
- `currentHalfBeatIndex / 2` で表拍の通算インデックスを直接算出する。
  ローカルカウンタ (`downbeatCount++`) を使わないため、フレーム落ちで表拍を
  飛ばしても音楽上の拍位置と攻撃パターンが常に同期する。
- Pure C# static class。SOは不要（パターンは固定値）。
- 名前空間は `Action002.Accessory.SonicWave.Logic`。ソニックウェーブ固有のロジック。
- enum 名は `SonicWaveBeat`（旧 `WaveAttackBeat`）。装飾品固有であることを明示。

### 3.2 IRhythmClock の拡張

`IRhythmClock` に `CurrentHalfBeatIndex` プロパティを追加する。

```csharp
public interface IRhythmClock
{
    int CurrentHalfBeatIndex { get; }  // 追加
    bool StartClock();
    void StopClock();
    void ProcessClock();
    bool ShouldFireOnDownbeat(ref int lastConsumedIndex);
    bool ShouldFireOnOffbeat(ref int lastConsumedIndex);
    void ResetForNewRun();
}
```

RhythmClock は既に `CurrentHalfBeatIndex` プロパティを持っている。
RhythmClockSystem も既に `CurrentHalfBeatIndex` を公開している。
インターフェースに追加するだけで実装変更は不要。

## 4. 波動（Wave）のデータ構造

### 4.1 WaveState（新規 struct）

```csharp
namespace Action002.Accessory.SonicWave.Data
{
    public enum WaveShape : byte
    {
        Arc = 0,
        Circle = 1,
    }

    public struct WaveState
    {
        public float2 Origin;
        public float CurrentRadius;
        public float MaxRadius;
        public float ExpandSpeed;
        public float ArcCenterAngle;
        public float ArcHalfSpread;
        public WaveShape Shape;
        public byte Polarity;
        public int Damage;
    }
}
```

**設計根拠:**
- BulletState と別の struct にする。波動は「原点からの半径が拡大する」という
  全く異なる更新モデル。BulletState に押し込むと Velocity の意味が崩壊する。
- WaveShape で扇/円を判別。共通の拡大ロジックで処理できる。

### 4.2 WaveStateSetSO（新規 ReactiveEntitySetSO<WaveState>）

```csharp
namespace Action002.Accessory.SonicWave.Data
{
    [CreateAssetMenu(fileName = "WaveStateSet", menuName = "Action002/Sets/Wave State Set")]
    public class WaveStateSetSO : ReactiveEntitySetSO<WaveState> { }
}
```

**SOが必要な理由:** 複数システム（生成・更新・衝突判定・描画）が共有するデータストア。
SerializeField で注入するために SO が必須。BulletStateSetSO と同じパターン。

## 5. ソニックウェーブの生成

### 5.1 SonicWaveAttackCalculator（新規 Pure C# static class）

```csharp
namespace Action002.Accessory.SonicWave.Logic
{
    public static class SonicWaveAttackCalculator
    {
        public const float RIGHT_WAVE_CENTER_ANGLE = 0f;
        public const float LEFT_WAVE_CENTER_ANGLE = math.PI;

        public static WaveState CreateArcWave(
            float2 origin, float centerAngle, float halfSpread,
            float maxRadius, float expandSpeed, byte polarity, int damage)
        {
            return new WaveState
            {
                Origin = origin,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                ExpandSpeed = expandSpeed,
                ArcCenterAngle = centerAngle,
                ArcHalfSpread = halfSpread,
                Shape = WaveShape.Arc,
                Polarity = polarity,
                Damage = damage,
            };
        }

        public static WaveState CreatePulse(
            float2 origin, float maxRadius, float expandSpeed,
            byte polarity, int damage)
        {
            return new WaveState
            {
                Origin = origin,
                CurrentRadius = 0f,
                MaxRadius = maxRadius,
                ExpandSpeed = expandSpeed,
                ArcCenterAngle = 0f,
                ArcHalfSpread = math.PI,
                Shape = WaveShape.Circle,
                Polarity = polarity,
                Damage = damage,
            };
        }
    }
}
```

### 5.2 SonicWave（新規 Pure C# class / IAccessory 実装）

ソニックウェーブ装飾品の Coordinator。IAccessory を実装する。

```csharp
namespace Action002.Accessory.SonicWave
{
    public class SonicWave : IAccessory
    {
        private readonly IRhythmClock rhythmClock;
        private readonly WaveStateSetSO waveSet;
        private readonly Vector2VariableSO playerPositionVar;
        private readonly IntVariableSO playerPolarityVar;

        private int lastConsumedHalfBeatIndex = -1;
        private int nextWaveId = 400000;

        /// <summary>
        /// 通常レベル → 装飾品レベルのマッピングテーブル。
        /// インデックスが通常レベル、値が装飾品レベル。
        /// 例: levelUpTable[3]=1 → 通常Lv3で解禁（WaveLevel=1）
        /// </summary>
        private static readonly int[] levelUpTable = { 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 5 };
        //                                           Lv0 Lv1 Lv2 Lv3 Lv4 Lv5 Lv6 Lv7 Lv8 Lv9 Lv10

        // --- IAccessory ---
        public int Level { get; set; }
        public bool IsUnlocked => Level > 0;

        public int GetLevelForPlayerLevel(int playerLevel)
        {
            if (playerLevel < 0) return 0;
            if (playerLevel >= levelUpTable.Length) return levelUpTable[levelUpTable.Length - 1];
            return levelUpTable[playerLevel];
        }

        /// <summary>最後の ProcessAttacks で発生したビート種別。nullなら何も起きなかった。</summary>
        public SonicWaveBeat? LastFiredBeat { get; private set; }

        public SonicWave(
            IRhythmClock rhythmClock,
            WaveStateSetSO waveSet,
            Vector2VariableSO playerPositionVar,
            IntVariableSO playerPolarityVar,
            float baseMaxRadius,
            float baseExpandSpeed)
        {
            this.rhythmClock = rhythmClock;
            this.waveSet = waveSet;
            this.playerPositionVar = playerPositionVar;
            this.playerPolarityVar = playerPolarityVar;
            this.baseMaxRadius = baseMaxRadius;
            this.baseExpandSpeed = baseExpandSpeed;
        }

        private readonly float baseMaxRadius;
        private readonly float baseExpandSpeed;

        public void ProcessAttacks()
        {
            LastFiredBeat = null;
            if (!IsUnlocked) return;
            if (rhythmClock == null || waveSet == null) return;
            if (playerPositionVar == null || playerPolarityVar == null) return;

            if (!rhythmClock.ShouldFireOnDownbeat(ref lastConsumedHalfBeatIndex))
                return;

            var beat = BeatPatternCalculator.GetCurrentBeat(rhythmClock.CurrentHalfBeatIndex);
            if (BeatPatternCalculator.IsRestBeat(beat)) return;

            float2 playerPos = new float2(playerPositionVar.Value.x, playerPositionVar.Value.y);
            byte polarity = (byte)playerPolarityVar.Value;
            var waveParams = SonicWaveParameterCalculator.Calculate(
                Level, baseMaxRadius, baseExpandSpeed);

            WaveState wave;
            switch (beat)
            {
                case SonicWaveBeat.RightWave:
                    wave = SonicWaveAttackCalculator.CreateArcWave(
                        playerPos, SonicWaveAttackCalculator.RIGHT_WAVE_CENTER_ANGLE,
                        waveParams.ArcHalfSpread,
                        waveParams.MaxRadius, waveParams.ExpandSpeed, polarity, waveParams.Damage);
                    break;
                case SonicWaveBeat.LeftWave:
                    wave = SonicWaveAttackCalculator.CreateArcWave(
                        playerPos, SonicWaveAttackCalculator.LEFT_WAVE_CENTER_ANGLE,
                        waveParams.ArcHalfSpread,
                        waveParams.MaxRadius, waveParams.ExpandSpeed, polarity, waveParams.Damage);
                    break;
                default: // Pulse
                    wave = SonicWaveAttackCalculator.CreatePulse(
                        playerPos, waveParams.MaxRadius, waveParams.ExpandSpeed,
                        polarity, waveParams.Damage);
                    break;
            }

            waveSet.Register(nextWaveId++, wave);
            LastFiredBeat = beat;
        }

        public void ResetForNewRun()
        {
            lastConsumedHalfBeatIndex = -1;
            nextWaveId = 400000;
            LastFiredBeat = null;
            Level = 0;
        }
    }
}
```

**重要な設計判断:**
- `Level <= 0`（IsUnlocked == false）の場合は波動を一切生成しない。これが解禁制御。
- SonicWave は IAccessory を実装する Pure C# class。AudioSource を持たない。
  SE 再生は SonicWaveSystem（MonoBehaviour）側で、`LastFiredBeat` プロパティに基づいて行う。
- SonicWave は自身の `lastConsumedHalfBeatIndex` を持つ。
  PlayerAttack とは独立した表拍消費トラッキング。

### 5.3 表拍消費の競合問題

**確認:** `ShouldFireOnDownbeat(ref int lastConsumedIndex)` の `ref int lastConsumedIndex` は呼び出し側が保持するローカル変数の参照であり、`RhythmClock` 内部の `currentHalfBeatIndex` は変更されない。
つまり複数の呼び出し元がそれぞれ独立した `lastConsumedIndex` を持てば、
同じ表拍を複数システムが検出できる。**競合は発生しない。**

### 5.4 SonicWaveSystem（新規 MonoBehaviour / Humble Object ラッパー）

```csharp
namespace Action002.Accessory.SonicWave.Systems
{
    public class SonicWaveSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Systems")]
        [SerializeField] private RhythmClockSystem rhythmClock;

        [Header("Sets")]
        [SerializeField] private WaveStateSetSO waveSet;

        [Header("Variables (read)")]
        [SerializeField] private Vector2VariableSO playerPositionVar;
        [SerializeField] private IntVariableSO playerPolarityVar;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip rightWaveClip;
        [SerializeField] private AudioClip leftWaveClip;
        [SerializeField] private AudioClip pulseClip;

        private SonicWave sonicWave;

        /// <summary>IAccessory として AccessoryManager に登録するための参照。</summary>
        public IAccessory Accessory => sonicWave;

        private void Awake()
        {
            sonicWave = new SonicWave(
                rhythmClock, waveSet,
                playerPositionVar, playerPolarityVar,
                gameConfig.WaveBaseMaxRadius, gameConfig.WaveBaseExpandSpeed);
        }

        /// <summary>
        /// 毎フレーム GameLoopManager から呼ばれる。
        /// SonicWave の攻撃処理を実行し、SE を再生する。
        /// Level は AccessoryManager が直接管理するため、SO変数の読み取りは不要。
        /// </summary>
        public void ProcessAttacks()
        {
            if (sonicWave == null) return;

            sonicWave.ProcessAttacks();

            if (sonicWave.LastFiredBeat.HasValue)
                PlaySfx(sonicWave.LastFiredBeat.Value);
        }

        public void ResetForNewRun()
        {
            sonicWave?.ResetForNewRun();
        }

        private void PlaySfx(SonicWaveBeat beat)
        {
            if (sfxSource == null) return;
            AudioClip clip = beat switch
            {
                SonicWaveBeat.RightWave => rightWaveClip,
                SonicWaveBeat.LeftWave => leftWaveClip,
                SonicWaveBeat.Pulse => pulseClip,
                _ => null,
            };
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }
    }
}
```

**SE再生の責務分離:**
SonicWave（Pure C# class）は波動の生成ロジックのみを担当し、AudioSource を参照しない。
SE 再生は SonicWaveSystem（MonoBehaviour）が `LastFiredBeat` プロパティに基づいて行う。
これにより SonicWave のユニットテストで AudioSource のモックが不要になる。

**AccessoryManager との関係:**
SonicWaveSystem.ProcessAttacks() は GameLoopManager から直接呼ばれる。
SE 再生は MonoBehaviour 側の責務であり、IAccessory.ProcessAttacks() だけでは SE が鳴らないため、
攻撃処理のディスパッチは各 System が担当する。
AccessoryManager はレベル管理と一括リセットを担当する（セクション 2.2 参照）。
Level は AccessoryManager.ApplyLevels(playerLevel) で通常レベルに応じた値がセットされるため、
SO変数（playerWaveLevelVar）を経由する間接層は不要。

## 6. 波動の更新（移動）

### 6.1 WaveExpandJob（新規 IJobParallelFor）

```csharp
namespace Action002.Accessory.SonicWave.Systems
{
    [BurstCompile]
    public struct WaveExpandJob : IJobParallelFor
    {
        [ReadOnly] public NativeSlice<WaveState> Src;
        [WriteOnly] public NativeArray<WaveState> Dst;
        public float DeltaTime;

        public void Execute(int index)
        {
            var w = Src[index];
            w.CurrentRadius += w.ExpandSpeed * DeltaTime;
            Dst[index] = w;
        }
    }
}
```

### 6.2 最大半径到達時の削除

WaveBoundsCalculator（新規 static class）:
```csharp
namespace Action002.Accessory.SonicWave.Logic
{
    public static class WaveBoundsCalculator
    {
        /// <summary>
        /// currentRadius が maxRadius を超えたら期限切れ。
        /// </summary>
        public static bool IsExpired(float currentRadius, float maxRadius)
        {
            return currentRadius > maxRadius;
        }
    }
}
```

**MaxRadius の定義と各システムの整合:**
- `MaxRadius` は「リング中心線の最大到達半径」。
- `WaveExpandJob`: `currentRadius += expandSpeed * deltaTime`。クランプしない。
  MaxRadius を超えた値もそのまま保持される。
- `WaveBoundsCalculator.IsExpired`: `currentRadius > maxRadius` で判定。
  `>` (strict greater) なので、ちょうど MaxRadius に到達したフレームは期限切れにならない。
- 衝突判定: `currentRadius > maxRadius` のフレームでも WaveCollision が先に走るため、
  そのフレームの衝突判定は行われてから RemoveExpiredWaves で削除される。
- 描画: メッシュスケールを `(maxRadius + ringThickness * 0.5f) * 2` にして、
  リング外縁が MaxRadius を超える分も描画可能にする。

## 7. 波動の衝突判定

### 7.1 WaveCollisionCalculator（新規 Pure C# static class）

```csharp
namespace Action002.Accessory.SonicWave.Logic
{
    public static class WaveCollisionCalculator
    {
        public static bool IsInWaveRing(
            float2 waveOrigin, float currentRadius, float ringThickness,
            float2 targetPos, float targetRadius)
        {
            float dist = math.distance(waveOrigin, targetPos);
            float innerEdge = currentRadius - ringThickness * 0.5f - targetRadius;
            float outerEdge = currentRadius + ringThickness * 0.5f + targetRadius;
            return dist >= math.max(0f, innerEdge) && dist <= outerEdge;
        }

        public static bool IsInArc(
            float2 waveOrigin, float arcCenterAngle, float arcHalfSpread,
            float2 targetPos, float targetRadius)
        {
            float2 diff = targetPos - waveOrigin;
            float dist = math.length(diff);
            if (dist < 0.0001f) return true;

            float angle = math.atan2(diff.y, diff.x);
            float delta = AngleDelta(angle, arcCenterAngle);
            float angleMargin = math.atan2(targetRadius, dist);
            return math.abs(delta) <= arcHalfSpread + angleMargin;
        }

        public static bool IsHit(WaveState wave, float ringThickness,
            float2 targetPos, float targetRadius)
        {
            if (!IsInWaveRing(wave.Origin, wave.CurrentRadius, ringThickness,
                targetPos, targetRadius))
                return false;

            if (wave.Shape == WaveShape.Circle)
                return true;

            return IsInArc(wave.Origin, wave.ArcCenterAngle,
                wave.ArcHalfSpread, targetPos, targetRadius);
        }

        private static float AngleDelta(float a, float b)
        {
            float d = a - b;
            d = d - 2f * math.PI * math.floor((d + math.PI) / (2f * math.PI));
            return d;
        }
    }
}
```

### 7.2 EnemyDamageCalculator（新規 Pure C# static class）

BulletCollision と WaveCollision の両方から使われるダメージ計算の共通化。

```csharp
namespace Action002.Enemy.Logic
{
    public static class EnemyDamageCalculator
    {
        public struct DamageResult
        {
            public int RemainingHp;
            public bool IsKilled;
        }

        public static DamageResult ApplyDamage(int currentHp, int damage)
        {
            int remaining = currentHp - damage;
            return new DamageResult
            {
                RemainingHp = remaining,
                IsKilled = remaining <= 0,
            };
        }
    }
}
```

### 7.3 WaveCollision（新規 Pure C# class / Coordinator）

**依存:**
- `WaveStateSetSO waveSet`
- `EnemyStateSetSO enemySet`
- `IBossHitTarget bossHitTarget`（ボス戦対応）
- `EnemyDeathBufferSO deathBuffer`
- `IntEventChannelSO onEnemyKilled`
- `IntEventChannelSO onKillScoreAdded`
- `GameConfigSO gameConfig`（waveRingThickness 取得）
- `int killScore`（コンストラクタ引数。BulletCollisionSystem の killScore = 50 と同値をデフォルト）

**処理フロー:**
1. `ProcessCollisions` 冒頭で waveHitHistory のキーと現存 Wave の EntityIds を照合し、
   存在しない waveId のエントリを削除（寿命管理）
2. `enemyKillSet` (HashSet<int>) をクリア — 同フレーム内の多重キル防止用
3. 全 WaveState を走査
4. 各 Wave に対して:
   a. ボス判定（7.5 参照）
   b. 全敵を走査し `WaveCollisionCalculator.IsHit` で判定
   c. `hitSet` で `hitSet.Contains(enemyId)` チェック。含まれていればスキップ
   d. `enemyKillSet.Contains(enemyId)` ならスキップ（同フレームで別 Wave がキル済み）
   e. `hitSet.Add(enemyId)` でヒット履歴に登録（再ヒット防止）
   f. `EnemyDamageCalculator.ApplyDamage` でHP計算
   g. 非致死: `enemySet.SetData(enemyId, updatedEnemy)` でHP反映
   h. 致死: `enemyKillSet.Add(enemyId)`, `enemyDespawnQueue.Add(enemyId)`,
      `deathBuffer.Add(...)`, `onKillScoreAdded.RaiseEvent(...)`, `onEnemyKilled.RaiseEvent(...)`
5. フレーム末尾で `enemyDespawnQueue` 内の enemyId を `enemySet.Unregister(enemyId)` で削除

**重複ヒット防止の設計:**

`Dictionary<int, HashSet<int>> waveHitHistory` を保持。
key = waveId（WaveStateSetSO の EntityId）、value = ヒット済みターゲット ID のセット。

**ターゲット ID は以下のように統一する:**
- 敵: enemyId そのまま（正の整数）
- ボス entity: `-(bossEntityIndex + 1)` で負の整数にマッピング（例: Guardian0 = -1, Guardian1 = -2, Magatama = -3）

**寿命管理:**
- `ProcessCollisions` 冒頭で、`waveHitHistory` のキーのうち `waveSet.EntityIds` に
  存在しないものを削除。
- `ResetForNewRun()`: `waveHitHistory.Clear()`

### 7.4 BulletCollision との関係

**BulletCollision は一切変更しない。**
BulletCollision は引き続き:
- 敵弾 vs プレイヤーの衝突判定（吸収・ダメージ）
- プレイヤー弾 vs 敵の衝突判定（キル）
- プレイヤー弾 vs ボスの衝突判定

を担当する。波動攻撃とは完全に独立した系統で動作する。

### 7.5 ボス戦での波動ヒット判定

**現行ボス構造の分析:**
- `BossController.TryHitAny` は 3 つの BossEntity（Guardian0, Guardian1, Magatama）を
  個別の位置・個別の衝突半径で走査し、点対点の距離判定でヒットを決める。
- 波動は点ではなくリング状のため、既存 `TryHitAny` に渡す形では正しく判定できない。

**方針: `IBossHitTarget` を拡張し、WaveCollision が各 entity を個別に判定する。**

```csharp
public interface IBossHitTarget
{
    bool IsActive { get; }
    int EntityCount { get; }                                          // 新規
    bool GetEntityInfo(int index, out float2 position,
        out float collisionRadius, out bool isActive);                // 新規
    bool TryHitAny(float bulletX, float bulletY,
        float bulletRadius, int damage);                              // 既存（BulletCollision用、変更なし）
    bool TryApplyDamageToEntity(int entityIndex, int damage);         // 新規
}
```

**既存の `TryHitAny` はそのまま残す。** BulletCollision が引き続き使用する。

### 7.6 WaveCollisionSystem（新規 MonoBehaviour / Humble Object ラッパー）

`BulletCollisionSystem` と同じパターン:
- `onBossHitTargetChanged` (GameObjectEventChannelSO) を Subscribe
- `HandleBossHitTargetChanged(GameObject go)` で `go.GetComponent<IBossHitTarget>()` を取得
- `WaveCollision.SetBossHitTarget(target)` に渡す

## 8. レベルアップとの連携

### 8.1 PlayerGrowthState の変更

**PlayerGrowthState は変更しない。** 通常攻撃のパラメータのみを保持する。

```csharp
public struct PlayerGrowthState
{
    public int Level;
    public int BulletCount;               // 既存維持
    public float MoveSpeedMultiplier;     // 既存維持
    public float BulletSpeedMultiplier;   // 既存維持
    // WaveLevel は持たない。装飾品レベルは IAccessory.Level で管理する。
}
```

装飾品のレベルは PlayerGrowthState には含めない。各装飾品が IAccessory.Level として
自身のレベルを保持し、AccessoryManager が管理する。

### 8.2 装飾品レベルの初期値契約

- 各装飾品の `ResetForNewRun()` で `Level = 0` にリセットする。
  **0 = 未解禁。** SonicWave は `Level <= 0`（IsUnlocked == false）で波動を生成しない。
- `SonicWaveParameterCalculator.Calculate(waveLevel, ...)` は `waveLevel >= 1` を前提とする。
  `waveLevel <= 0` の場合は `waveLevel = 1` にクランプして安全にフォールバック。

### 8.3 playerWaveLevelVar の廃止

SO変数（playerWaveLevelVar）を経由したレベル伝搬は行わない。
SonicWave（Pure C# class）が直接 Level プロパティを保持し、
AccessoryManager.ApplyLevels(playerLevel) で通常レベルに応じた値がセットされる。

SonicWaveSystem（MonoBehaviour）は、sonicWave.Level を直接参照する。
PlayerController、GameplaySceneLifetime に playerWaveLevelVar を追加する必要はない。

### 8.4 レベルアップテーブル（通常攻撃のみ）

**PlayerGrowthCalculator.ApplyLevelUp は通常攻撃のパラメータのみを扱う。**
現行コードと完全一致。装飾品のレベルアップは含めない。

```csharp
public static PlayerGrowthState ApplyLevelUp(PlayerGrowthState current)
{
    current.Level++;
    switch (current.Level)
    {
        case 1:  current.BulletCount = 2; break;
        case 2:  current.MoveSpeedMultiplier += 0.10f; break;
        case 3:  current.BulletCount = 3; break;
        case 4:  current.BulletSpeedMultiplier += 0.15f; break;
        case 5:  current.BulletCount = 4; break;
        case 6:  current.BulletCount = 5; break;
        case 7:  current.BulletSpeedMultiplier += 0.15f; break;
        case 8:  current.BulletCount = 6; break;
        case 9:  current.BulletCount = 7; break;
        case 10: current.BulletCount = MAX_BULLET_COUNT; break;
        default: current.BulletCount = MAX_BULLET_COUNT; break; // Level 11+: capped
    }
    return current;
}
```

### 8.4.1 装飾品のレベルアップ（PlayerGrowthCoordinator の変更）

装飾品のレベル適用は PlayerGrowthCoordinator が AccessoryManager を通じて行う。
各装飾品が固有のレベルアップテーブルを持ち、通常レベルに応じた装飾品レベルを返す。

```csharp
namespace Action002.Player.Logic
{
    public class PlayerGrowthCoordinator
    {
        private PlayerGrowthState growthState;
        private readonly IPlayerGrowthActions actions;
        private readonly AccessoryManager accessoryManager;

        public PlayerGrowthCoordinator(
            IPlayerGrowthActions actions,
            AccessoryManager accessoryManager)
        {
            this.actions = actions;
            this.accessoryManager = accessoryManager;
            growthState = PlayerGrowthCalculator.CreateDefault();
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
            accessoryManager.ApplyLevels(growthState.Level);

            actions.RaiseLevelUp(growthState.Level);
        }

        public void Reset()
        {
            growthState = PlayerGrowthCalculator.CreateDefault();
            accessoryManager.ResetForNewRun();
        }
    }
}
```

**設計根拠:**
- 装飾品の解禁・レベルアップは各装飾品のレベルアップテーブルで決まる。PlayerGrowthCoordinator は通常レベルを渡すだけで、解禁レベルや個別のレベルアップタイミングを知らない。
- 通常攻撃のテーブル（PlayerGrowthCalculator）は一切変更しない。現行コードと完全一致。
- ApplyLevels() は冪等。同じ通常レベルで複数回呼んでも結果は同じ。
- MAX_BULLET_COUNT = 8 のキャップは既存維持。
- 今はソニックウェーブのみだが、装飾品が増えた場合も Register() でテーブル付きの装飾品を追加するだけ。

### 8.5 WaveLevel ごとのパラメータ

MaxRadius と ExpandSpeed は GameConfigSO の基底値に対する倍率で定義する。
Damage と ArcHalfSpread はゲームデザイン上の固定値。

| WaveLevel | MaxRadius 倍率 | ExpandSpeed 倍率 | Damage | ArcHalfSpread |
|-----------|---------------|-----------------|--------|---------------|
| 1（解禁） | 1.0x | 1.0x | 1 | π/4 (90度扇) |
| 2 | 1.0x | 1.0x | 2 | π/4 |
| 3 | 1.4x | 1.0x | 2 | π/3 (120度扇) |
| 4 | 1.4x | 1.25x | 2 | π/3 |
| 5 | 1.8x | 1.25x | 3 | π/3 |
| 6 | 1.8x | 1.25x | 3 | π/2 (180度=半円) |

### 8.6 SonicWaveParameterCalculator（新規 Pure C# static class）

```csharp
namespace Action002.Accessory.SonicWave.Logic
{
    public struct SonicWaveParameters
    {
        public float MaxRadius;
        public float ExpandSpeed;
        public int Damage;
        public float ArcHalfSpread;
    }

    public static class SonicWaveParameterCalculator
    {
        public static SonicWaveParameters Calculate(int waveLevel,
            float baseMaxRadius, float baseExpandSpeed)
        {
            if (waveLevel < 1) waveLevel = 1;
            var p = new SonicWaveParameters
            {
                MaxRadius = baseMaxRadius,
                ExpandSpeed = baseExpandSpeed,
                Damage = 1,
                ArcHalfSpread = math.PI / 4f,  // 90度
            };

            // Damage
            if (waveLevel >= 2) p.Damage = 2;
            if (waveLevel >= 5) p.Damage = 3;

            // MaxRadius
            if (waveLevel >= 3) p.MaxRadius = baseMaxRadius * 1.4f;
            if (waveLevel >= 5) p.MaxRadius = baseMaxRadius * 1.8f;

            // ExpandSpeed
            if (waveLevel >= 4) p.ExpandSpeed = baseExpandSpeed * 1.25f;

            // ArcHalfSpread
            if (waveLevel >= 3) p.ArcHalfSpread = math.PI / 3f;  // 120度
            if (waveLevel >= 6) p.ArcHalfSpread = math.PI / 2f;  // 180度

            return p;
        }
    }
}
```

### 8.7 パラメータのソースオブトゥルース

**方針: GameConfigSO は基底値（ベースライン）、SonicWaveParameterCalculator はレベル強化を適用する。**

GameConfigSO の波動パラメータ:
- `waveBaseMaxRadius = 5f` — レベル1のベース値
- `waveBaseExpandSpeed = 8f` — レベル1のベース値
- `waveRingThickness = 0.5f` — レベルに依存しない固定値（衝突判定と描画の両方で使用）

**ringThickness は GameConfigSO のみが保持。** SonicWaveParameterCalculator には含めない。
WaveCollision と WaveRenderer がそれぞれ GameConfigSO から取得する。

## 9. 視覚表現

### 9.1 方針: Graphics.DrawMesh（per-wave 個別描画）+ カスタムシェーダー

**BulletRenderer との差異と理由:**
BulletRenderer は DrawMeshInstanced で大量の弾をバッチ描画する。
波動は同時存在数が少なく（最大 3-6 個）、かつインスタンスごとに異なるパラメータ
（半径、角度、扇幅）が必要なため、per-instance パラメータが必要。
`DrawMeshInstanced` は per-instance パラメータを直接渡せない。
よって、1 Wave = 1 `Graphics.DrawMesh` 呼び出しで描画する。

### 9.2 WaveRenderer（新規 MonoBehaviour）

- `WaveStateSetSO` を参照
- `GameConfigSO` から `waveRingThickness` を取得
- **LateUpdate** で全 WaveState を走査
- 各 Wave ごとに MaterialPropertyBlock を設定し `Graphics.DrawMesh` で描画

```
foreach wave in waveSet:
    float meshHalfSize = wave.MaxRadius + ringThickness * 0.5f;
    propertyBlock.SetFloat("_RingRadius", wave.CurrentRadius / meshHalfSize)
    propertyBlock.SetFloat("_RingThickness", ringThickness / meshHalfSize)
    propertyBlock.SetFloat("_ArcCenter", wave.ArcCenterAngle)
    propertyBlock.SetFloat("_ArcSpread", wave.ArcHalfSpread)
    propertyBlock.SetColor("_BaseColor", PolarityColors.GetForeground(wave.Polarity))
    var matrix = Matrix4x4.TRS(
        new Vector3(wave.Origin.x, wave.Origin.y, waveZ),
        Quaternion.identity,
        Vector3.one * meshHalfSize * 2f)
    Graphics.DrawMesh(quadMesh, matrix, waveMaterial, layer, null, 0, propertyBlock)
```

### 9.3 シェーダー（WaveRing.shader）

URP Unlit ベースのカスタムシェーダー。
- Fragment で UV 中心 (0.5, 0.5) からの距離を算出（0-0.5 を 0-1 に正規化）
- `_RingRadius` 付近（± `_RingThickness * 0.5`）のみ描画、それ以外は discard
- Arc の場合は `_ArcCenter` / `_ArcSpread` で角度判定し、範囲外を discard
- 半透明 + Additive ブレンドでグロー感を出す

## 10. 極性との関連

波動攻撃はプレイヤーの現在の極性（`playerPolarityVar`）を引き継ぐ。
WaveState.Polarity にプレイヤーの極性を設定。
衝突判定時に敵の極性との関係は**使用しない**（プレイヤー弾と同じく全ての敵にダメージ）。
視覚的にはプレイヤーの極性カラーで波動を描画する。

## 11. GameConfigSO の変更

以下のフィールドを追加:
```csharp
[Header("Wave Attack")]
[SerializeField] private float waveBaseMaxRadius = 5f;
[SerializeField] private float waveBaseExpandSpeed = 8f;
[SerializeField] private float waveRingThickness = 0.5f;

public float WaveBaseMaxRadius => waveBaseMaxRadius;
public float WaveBaseExpandSpeed => waveBaseExpandSpeed;
public float WaveRingThickness => waveRingThickness;
```

既存の `PlayerBulletSpeed` 等は一切変更しない。

## 12. GameLoopManager の変更

### 12.1 フィールド追加

```csharp
[Header("Sets")]
[SerializeField] private WaveStateSetSO waveSet;

[Header("Systems")]
[SerializeField] private SonicWaveSystem sonicWave;
[SerializeField] private WaveCollisionSystem waveCollision;
```

`ReactiveEntitySetOrchestrator<WaveState> waveOrchestrator` をプライベートフィールドとして追加。
`bool hasPendingWaveJob` を追加。
`List<int> waveDespawnQueue` を追加。

### 12.2 Start() の変更

```csharp
if (waveSet != null)
    waveOrchestrator = new ReactiveEntitySetOrchestrator<WaveState>(waveSet);
```

### 12.3 Update() — Job スケジュール

```
Update:
    ScheduleEnemyJob()        // 既存
    ScheduleBulletJob()       // 既存
    ScheduleWaveExpandJob()   // 新規
```

### 12.4 LateUpdate() — 更新順序

```
LateUpdate:
    1. Complete EnemyJob               // 既存
    2. Complete BulletJob              // 既存
    3. Complete WaveExpandJob          // 新規
    4. RemoveOffscreenBullets()        // 既存
    5. RhythmClock.ProcessClock()      // 既存
    6. PlayerAttack.ProcessAttacks()   // 既存（通常攻撃、変更なし）
    7. SonicWave.ProcessAttacks()      // 新規（ソニックウェーブ生成 + SE再生）
    8. BulletCollision.ProcessCollisions()  // 既存（変更なし）
    9. WaveCollision.ProcessCollisions()   // 新規
   10. RemoveExpiredWaves()            // 新規（衝突判定の後に削除）
   11. ProcessEnemyContacts()          // 既存
   12. EnemySpawn/EnemyShoot           // 既存
```

**更新順序の根拠:**
- PlayerAttack (6) で通常弾が生成される（既存通り）。
- SonicWave (7) で波動が生成される（新規）。
- BulletCollision (8) で通常弾 vs 敵の衝突判定（既存通り）。
- WaveCollision (9) で波動 vs 敵の衝突判定（新規）。
- RemoveExpiredWaves (10) で期限切れ Wave を Unregister。

### 12.5 RemoveExpiredWaves() の実装

```csharp
private void RemoveExpiredWaves()
{
    if (waveSet == null || waveSet.Count == 0) return;
    waveDespawnQueue.Clear();

    var data = waveSet.Data;
    var ids = waveSet.EntityIds;
    for (int i = 0; i < data.Length; i++)
    {
        if (WaveBoundsCalculator.IsExpired(data[i].CurrentRadius, data[i].MaxRadius))
            waveDespawnQueue.Add(ids[i]);
    }

    foreach (var id in waveDespawnQueue)
        waveSet.Unregister(id);
}
```

### 12.6 ライフサイクル管理

**GameplaySceneLifetime の変更:**

フィールド追加:
- `[SerializeField] private SonicWaveSystem sonicWaveSystem`
- `[SerializeField] private WaveCollisionSystem waveCollisionSystem`
- `[SerializeField] private WaveStateSetSO waveStateSet`

Awake() に追加:
```csharp
if (waveStateSet != null) waveStateSet.Clear();
```

ResetForNewRun() に追加:
```csharp
if (sonicWaveSystem != null) sonicWaveSystem.ResetForNewRun();
if (waveCollisionSystem != null) waveCollisionSystem.ResetForNewRun();
if (waveStateSet != null) waveStateSet.Clear();
```

装飾品レベルのリセットは AccessoryManager.ResetForNewRun() が担当する
（PlayerGrowthCoordinator.Reset() から呼ばれる）。

OnDestroy() に追加:
```csharp
if (waveStateSet != null) waveStateSet.Clear();
```

**PlayerController.ApplyGrowth() は変更なし:**
```csharp
private void ApplyGrowth(PlayerGrowthState growthState)
{
    moveSpeedMultiplier = growthState.MoveSpeedMultiplier;
    if (playerBulletCountVar != null)
        playerBulletCountVar.Value = growthState.BulletCount;
    if (bulletSpeedMultiplierVar != null)
        bulletSpeedMultiplierVar.Value = growthState.BulletSpeedMultiplier;
    // 装飾品レベルは AccessoryManager が直接管理するため、ここでは扱わない
}
```

**StopAndCleanup() に追加:**
```csharp
if (hasPendingWaveJob)
{
    waveOrchestrator?.CompleteAndApply();
    hasPendingWaveJob = false;
}
```

**OnDestroy() に追加:**
```csharp
if (hasPendingWaveJob)
{
    waveOrchestrator?.CompleteAndApply();
    hasPendingWaveJob = false;
}
waveOrchestrator?.Dispose();
waveOrchestrator = null;
```

## 13. 名前空間とフォルダ構造

装飾品システム全体を `Action002.Accessory` 名前空間に配置する。

```
Assets/_Project/Scripts/
  Accessory/
    IAccessory.cs                          # 共通インターフェース
    AccessoryManager.cs                    # 装飾品マネージャー
    SonicWave/
      Data/
        WaveState.cs                       # struct (WaveShape enum 含む)
        WaveStateSetSO.cs                  # SO
      Logic/
        BeatPatternCalculator.cs           # static class (SonicWaveBeat enum 含む)
        SonicWaveAttackCalculator.cs       # static class
        SonicWaveParameterCalculator.cs    # static class (SonicWaveParameters struct 含む)
        WaveCollisionCalculator.cs         # static class
        WaveBoundsCalculator.cs            # static class
      Systems/
        SonicWave.cs                       # Pure C# class (IAccessory 実装)
        SonicWaveSystem.cs                 # MonoBehaviour (Humble Object)
        WaveCollision.cs                   # Pure C# class (Coordinator)
        WaveCollisionSystem.cs             # MonoBehaviour (Humble Object)
        WaveExpandJob.cs                   # IJobParallelFor
      Rendering/
        WaveRenderer.cs                    # MonoBehaviour
        WaveRing.shader                    # Shader
```

## 14. 将来の拡張パターン

2つ目の装飾品（例: ホーミングミサイル）を追加する場合:

1. `Accessory/HomingMissile/` フォルダを作成
2. `HomingMissile : IAccessory` を実装
3. `HomingMissileSystem : MonoBehaviour` を作成
4. `PlayerGrowthState` に `MissileLevel` フィールドを追加
5. `PlayerGrowthCalculator.ApplyLevelUp` に MissileLevel の変更を追加
6. `PlayerController` に `playerMissileLevelVar` を追加
7. `GameLoopManager` に HomingMissileSystem の参照と呼び出しを追加

IAccessory / AccessoryManager のコードは変更不要。

## 15. 新規ファイル一覧

| ファイルパス | 種類 | 説明 |
|-------------|------|------|
| `Accessory/IAccessory.cs` | Pure C# interface | 装飾品の共通インターフェース |
| `Accessory/AccessoryManager.cs` | Pure C# class | 装飾品マネージャー（一括リセット等） |
| `Accessory/SonicWave/Logic/BeatPatternCalculator.cs` | Pure C# static | 4拍パターンのビート判定（SonicWaveBeat enum 含む） |
| `Accessory/SonicWave/Logic/SonicWaveAttackCalculator.cs` | Pure C# static | WaveState 生成ロジック |
| `Accessory/SonicWave/Logic/SonicWaveParameterCalculator.cs` | Pure C# static | レベル→波動パラメータ変換（SonicWaveParameters struct 含む） |
| `Accessory/SonicWave/Logic/WaveCollisionCalculator.cs` | Pure C# static | 波動の当たり判定ロジック |
| `Accessory/SonicWave/Logic/WaveBoundsCalculator.cs` | Pure C# static | 最大半径到達判定 |
| `Accessory/SonicWave/Data/WaveState.cs` | struct | 波動データ（WaveShape enum 含む） |
| `Accessory/SonicWave/Data/WaveStateSetSO.cs` | SO | 波動の ReactiveEntitySet |
| `Accessory/SonicWave/Systems/SonicWave.cs` | Pure C# class (IAccessory) | ソニックウェーブ生成 Coordinator |
| `Accessory/SonicWave/Systems/SonicWaveSystem.cs` | MonoBehaviour | ソニックウェーブの Humble Object ラッパー |
| `Accessory/SonicWave/Systems/WaveCollision.cs` | Pure C# class | 波動衝突処理の Coordinator |
| `Accessory/SonicWave/Systems/WaveCollisionSystem.cs` | MonoBehaviour | 波動衝突の Humble Object ラッパー |
| `Accessory/SonicWave/Systems/WaveExpandJob.cs` | IJobParallelFor | 波動の半径拡大 Job |
| `Accessory/SonicWave/Rendering/WaveRenderer.cs` | MonoBehaviour | DrawMesh 描画 |
| `Accessory/SonicWave/Rendering/WaveRing.shader` | Shader | SDF リング描画シェーダー |
| `Enemy/Logic/EnemyDamageCalculator.cs` | Pure C# static | ダメージ/キル判定の共通化 |

## 16. 変更ファイル一覧

| ファイルパス | 変更内容 |
|-------------|---------|
| `Player/Logic/PlayerGrowthCoordinator.cs` | AccessoryManager / IAccessory 依存追加、CheckAndApplyGrowth に装飾品解禁・レベルアップ処理追加、Reset に accessoryManager.ResetForNewRun() 追加 |
| `Core/GameConfigSO.cs` | WaveBaseMaxRadius, WaveBaseExpandSpeed, WaveRingThickness 追加 |
| `Core/GameLoopManager.cs` | WaveStateSetSO / Orchestrator 追加、WaveExpandJob スケジュール、RemoveExpiredWaves、SonicWave/WaveCollision 呼び出し追加 |
| `Core/IBossHitTarget.cs` | EntityCount, GetEntityInfo, TryApplyDamageToEntity 追加 |
| `Core/Flow/GameplaySceneLifetime.cs` | waveStateSet / sonicWaveSystem / waveCollisionSystem 追加、Awake/ResetForNewRun/OnDestroy に波動関連初期化追加（既存コードは変更なし） |
| `Boss/Systems/BossController.cs` | IBossHitTarget 新メソッド実装（EntityCount, GetEntityInfo, TryApplyDamageToEntity） |
| `Audio/Systems/IRhythmClock.cs` | CurrentHalfBeatIndex プロパティ追加 |

## 17. 変更しないファイル（明示）

以下のファイルは一切変更しない:

| ファイルパス | 理由 |
|-------------|------|
| `Player/Data/PlayerGrowthState.cs` | 通常攻撃パラメータのみ保持。装飾品レベルは IAccessory.Level で管理 |
| `Player/Logic/PlayerGrowthCalculator.cs` | 通常攻撃のレベルアップテーブルのみ。現行コードと完全一致を維持 |
| `Player/Systems/PlayerController.cs` | ApplyGrowth は通常攻撃パラメータのみ。装飾品レベルは AccessoryManager 経由 |
| `Player/Systems/PlayerAttack.cs` | 通常攻撃の生成ロジック。そのまま維持 |
| `Player/Systems/PlayerAttackSystem.cs` | 通常攻撃のHumble Object。DI構成もそのまま |
| `Bullet/Data/BulletState.cs` | 通常弾のデータ構造 |
| `Bullet/Data/BulletStateSetSO.cs` | 通常弾のデータストア |
| `Bullet/Systems/BulletCollision.cs` | 通常弾の衝突判定。リファクタは将来課題 |
| `Bullet/Systems/BulletCollisionSystem.cs` | 通常弾衝突のHumble Object |
| `Bullet/Logic/BulletCollisionCalculator.cs` | 通常弾の衝突判定ロジック |

## 18. テスト方針

### 18.1 新規テスト（EditMode）

- `BeatPatternCalculatorTests`: パターン循環、休符判定、halfBeatIndex からの正しい変換
- `SonicWaveAttackCalculatorTests`: Arc/Pulse 生成の正しさ
- `WaveCollisionCalculatorTests`:
  - リング判定: 内側・リング上・外側のケース
  - 角度判定: 扇内・扇外・境界のケース
  - targetRadius（敵半径）の効果: 大型敵がリングエッジで当たるか
  - Circle は角度判定なしで全方位ヒットするか
  - AngleDelta の正規化が正しいか
- `SonicWaveParameterCalculatorTests`:
  - レベルごとのパラメータ正しさ
  - waveLevel <= 0 のクランプ
  - 基底値の反映
- `EnemyDamageCalculatorTests`: ダメージ計算、キル判定
- `WaveBoundsCalculatorTests`: 期限切れ判定
- `SonicWaveTests` (IAccessory 統合テスト):
  - Level 0 の場合に波動が生成されないこと
  - Level 1 以上で表拍に波動が生成されること
  - リズムパターンに従ってビートが循環すること

### 18.2 既存テストの改修

- `PlayerGrowthCalculatorTests`: 変更なし。ApplyLevelUp は通常攻撃のみを扱うため、既存アサーション（BulletCount / BulletSpeedMultiplier）はそのまま維持。
- `PlayerGrowthCoordinatorTests`（新規）: AccessoryManager との連携テスト。通常レベルに応じて ApplyLevels が呼ばれ、各装飾品のレベルアップテーブルに基づいた装飾品レベルが適用されること。
