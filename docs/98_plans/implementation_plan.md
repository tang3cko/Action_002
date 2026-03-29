# Implementation plan

## Purpose

This document describes the implementation plan for Action_002 (Polarity Survivors), a unity1week game jam project.

Covers: architecture, data design, milestones, and task breakdown for everything except the slot system.
Does not cover: slot system implementation, visual polish details, or audio.

---

## Architecture overview

```
Assets/_Project/
├── Scripts/
│   ├── Core/           # Polarity enum, GameManager, GameConfigSO
│   ├── Player/
│   │   ├── Data/       # PlayerState
│   │   ├── Logic/      # PolarityCalculator, DamageCalculator
│   │   └── Systems/    # PlayerController, PlayerAttackSystem
│   ├── Enemy/
│   │   ├── Data/       # EnemyState, EnemyStateSetSO
│   │   ├── Logic/      # SpawnCalculator, EnemyMoveJob
│   │   ├── Systems/    # EnemySpawnSystem, EnemyMovementSystem
│   │   └── Rendering/  # EnemyRenderer
│   ├── Bullet/
│   │   ├── Data/       # BulletState, BulletStateSetSO
│   │   ├── Logic/      # BulletSpawnCalculator, BulletMoveJob, AbsorptionCalculator
│   │   ├── Systems/    # BulletMovementSystem, BulletCollisionSystem
│   │   └── Rendering/  # BulletRenderer
│   ├── Input/          # InputReaderSO
│   └── UI/             # HUD, GameOverScreen
├── ScriptableObjects/
│   ├── Events/
│   ├── Variables/
│   └── Sets/
├── Tests/
│   └── EditMode/
│       ├── Player/
│       ├── Enemy/
│       ├── Bullet/
│       └── Integration/
├── Scenes/
├── Prefabs/
└── Sprites/
```

Namespace: `Action002.Player`, `Action002.Enemy`, `Action002.Bullet`, `Action002.Core`, `Action002.Input`, `Action002.UI`

---

## State ownership

| Subject | Truth source | UI connection |
|---------|-------------|--------------|
| Enemies | `EnemyStateSetSO<EnemyState>` | None (renderer reads directly) |
| Bullets | `BulletStateSetSO<BulletState>` | None |
| Player | `PlayerController` owns `PlayerState` struct | One-way push to VariableSOs for HUD |

---

## Data definitions

### EnemyState

```csharp
public struct EnemyState  // unmanaged
{
    public float2 Position;
    public float2 Velocity;
    public float Speed;
    public int Hp;
    public byte Polarity;   // 0 = White, 1 = Black
    public int EnemyTypeId;
}
```

### EnemyTrait

Trait bitmask for immutable tags only. Polarity is in data, not trait.

```csharp
[Flags]
public enum EnemyTrait : byte
{
    None    = 0,
    Shooter = 1 << 0,
    Elite   = 1 << 1,
}
```

### BulletState

```csharp
public struct BulletState  // unmanaged
{
    public float2 Position;
    public float2 Velocity;
    public float Lifetime;
    public float ScoreValue;
    public byte Polarity;   // 0 = White, 1 = Black
}
```

### Polarity

```csharp
public enum Polarity : byte { White = 0, Black = 1 }
```

### PlayerState

Single entity. Not managed by RES.

```csharp
public struct PlayerState
{
    public float2 Position;
    public Polarity CurrentPolarity;
    public int Hp;
    public int MaxHp;
    public float InvincibleTimer;
    public int ComboCount;
    public float ComboTimer;
    public float ComboMultiplier;
    public float SpinGauge;
    public int Score;
}
```

### GameConfigSO

All tuning values in a single ScriptableObject.

```csharp
[CreateAssetMenu(menuName = "Action002/Game Config")]
public class GameConfigSO : ScriptableObject
{
    [Header("Player")]
    public float moveSpeed;
    public int maxHp;
    public float attackRange;
    public float attackInterval;
    public float invincibleDuration;

    [Header("Combo")]
    public float comboMultiplierStep;
    public float comboTimeout;

    [Header("Spawn")]
    public float baseSpawnInterval;
    public float minSpawnInterval;
    public int maxEnemies;
    public float spawnRadius;

    [Header("Gauge")]
    public float absorbGaugeRate;
    public float killGaugeRate;
}
```

---

## RES update order (per frame)

Structural changes (Unregister) must never happen during Job execution or before CompleteAndApply.

```
Update:
  Schedule EnemyMoveJob  →  orchestrator.ScheduleUpdate(handle, count)
  Schedule BulletMoveJob →  orchestrator.ScheduleUpdate(handle, count)

LateUpdate:
  enemyOrchestrator.CompleteAndApply()
  bulletOrchestrator.CompleteAndApply()
  Run collision detection (write to despawnQueue only, no Unregister here)
  Flush despawnQueue → Unregister all pending entities
```

---

## Rendering strategy

- `Graphics.DrawMeshInstanced` with an unlit shader
- Two draw calls per entity type: one white batch, one black batch
- Polarity read from `state.Polarity` field
- No per-entity GameObjects

---

## SO communication

### Variables

| Asset | Type | Owner |
|-------|------|-------|
| PlayerPolarity | IntVariableSO | PlayerController |
| PlayerHp | IntVariableSO | PlayerController |
| Score | IntVariableSO | PlayerController |
| ComboCount | IntVariableSO | PlayerController |
| SpinGauge | FloatVariableSO | PlayerController |

### Event channels

| Asset | Type | Publisher |
|-------|------|-----------|
| OnPolarityChanged | IntEventChannelSO | PlayerController |
| OnPlayerDamaged | VoidEventChannelSO | BulletCollisionSystem |
| OnComboChanged | IntEventChannelSO | PlayerController |
| OnGameOver | VoidEventChannelSO | PlayerController |

---

## Milestones

### M1: Player foundation

Goal: Player moves and switches polarity. Camera follows.

- Directory structure and asmdef setup
- `Polarity` enum, `PlayerState` struct
- `InputReaderSO` (Move + PolaritySwitch)
- `PolarityCalculator` + Edit Mode tests
- `PlayerController` (movement, polarity toggle, color change)
- Scene + Cinemachine camera

### M2: Enemy spawning + RES + movement job

Goal: Enemies spawn from edges and swarm toward the player.

- `EnemyState` struct, `EnemyStateSetSO`
- `SpawnCalculator` + Edit Mode tests
- `EnemySpawnSystem` (Register + initial state)
- `EnemyMoveJob` (Burst, IJobParallelFor)
- `EnemyMovementSystem` (Orchestrator)
- `EnemyRenderer` (DrawMeshInstanced)

### M3: Player auto-attack + enemy death

Goal: Player automatically kills nearby enemies. Score increments.

- `AttackCalculator` + Edit Mode tests
- `PlayerAttackSystem` (range scan → despawnQueue → Unregister)
- Score update (PlayerState.Score → Score VariableSO)

### M4: Bullet system + polarity absorption

Goal: Enemies shoot bullets. Same-polarity bullets absorbed, opposite-polarity bullets damage.

- `BulletState` struct, `BulletStateSetSO`
- `BulletMoveJob` (Burst) + `BulletMovementSystem` (Orchestrator)
- `EnemyShootSystem` (Register bullets at interval)
- `AbsorptionCalculator` + Edit Mode tests
- `BulletCollisionSystem` (absorb/damage → despawnQueue → Unregister)
- `BulletRenderer` (DrawMeshInstanced)
- Integration test: Schedule → CompleteAndApply → despawnQueue flush order

### M4.5: Enemy-player contact collision

Goal: Walking into enemies has polarity-dependent results.

- `EnemyContactCalculator` + Edit Mode tests
- Same-polarity contact: pass through, small score bonus
- Different-polarity contact: damage player, destroy enemy (despawnQueue)

### M5: HP, invincibility, combo, game over

Goal: Full damage loop. HP reaches 0 → game over.

- `DamageCalculator` + Edit Mode tests
- Invincibility timer in PlayerState
- Combo counter (consecutive absorbs → multiplier, timeout → reset)
- HP 0 → `OnGameOver` → `GameManager`
- unityroom scoreboard submission

### M6: HUD + game flow

Goal: HUD shows game state. Title and game over screens work.

- HUD: HP, score, combo, gauge, polarity indicator (placeholder styling)
- VariableSO / EventChannel bindings to HUD
- Game over panel + retry
- Title screen (name + start button)

### M7: Balance + WebGL

Goal: Playable 3–5 minute session. WebGL build ready for submission.

- Difficulty scaling: spawn rate, enemy speed, bullet density over time
- Tuning pass on all GameConfigSO values
- WebGL build (960x540, Gzip, <50 MB)
- Mobile touch controls
- unityroom submission

---

## Out of scope

- Slot system (planned separately)
- Elite trait (future)
- BGM sync
- Visual/audio polish

---

## References

- [Reactive SO library source](../Assets/Event%20Channels/Runtime/ReactiveEntitySets/)
- [unityroom scoreboard docs](https://help.unityroom.com/how-to-implement-scoreboard)
