# 弾幕パターン・アルゴリズム 調査レポート

## Summary

- 弾幕パターンは **基本3グループ（Ring / Spread / Stack）** の組み合わせで構成される
- 弾の挙動は **速度・角度・加速度・角速度** の4パラメータで大半を表現可能
- 極性システム（斑鳩型）では同属性吸収 + 逆属性2倍ダメージが基本設計
- ボス弾幕は **フェーズ制**（体力区間ごとにパターン切替）が標準
- BulletML等の弾幕記述言語が存在し、パターンをデータドリブンで管理可能

---

## 1. 基本弾幕パターン

### 1.1 Ring（全方位弾 / n-ring）

360度を均等分割し、同時に全方向へ弾を発射する。

```
angleStep = 360° / n
for i in 0..n:
    angle = seedAngle + angleStep * i
    vx = speed * cos(angle)
    vy = speed * sin(angle)
    spawn(emitter.pos, vx, vy)
```

**バリエーション:**
- **Fixed Ring**: seedAngle固定。毎回同じ方向に発射
- **Random Ring**: seedAngleをランダム化。予測不能
- **Aimed Ring**: seedAngleを自機方向に設定。1発が必ず自機を狙う
- **Rotating Ring**: seedAngleをフレームごとに増加 → 渦巻きへ発展
- **Offset Ring**: 発射位置を中心からrだけオフセット（`emitter.pos + r * (cos(angle), sin(angle))`）
- **Concentric Ring（同心円リング）**: 同角度・異速度のリングを同時発射。時間経過でリング間が広がる

**設計指針:**
- 最低3発でリング感が出る
- 奇数リングは中央弾が自機を狙い、偶数リングは自機の隙間を通る

**参照元:** Sparen's Danmaku Design Studio Guide A3, Qiita弾幕プログラミング入門

---

### 1.2 Spread（扇状弾 / n-way）

指定した角度範囲に弾を扇形に展開する。

```
arcAngle = totalArc  // 例: 60°
angleStep = arcAngle / (n - 1)
startAngle = aimAngle - arcAngle / 2
for i in 0..n:
    angle = startAngle + angleStep * i
    vx = speed * cos(angle)
    vy = speed * sin(angle)
    spawn(emitter.pos, vx, vy)
```

**奇数弾 vs 偶数弾:**
- **奇数way（3way, 5way, 7way）**: 中央弾が自機直撃 → プレイヤーは横移動必須（チョン避け）
- **偶数way（2way, 4way, 6way）**: 中央が空く → 動かなければ当たらないが行動が制限される（自機外し弾）

**参照元:** Sparen Guide A3, 東方Project弾幕基本講座

---

### 1.3 Stack（重ね撃ち / n-stack）

同じ角度・発射点から速度違いの弾を同時発射する。

```
for i in 0..n:
    s = minSpeed + (maxSpeed - minSpeed) * i / (n - 1)
    vx = s * cos(angle)
    vy = s * sin(angle)
    spawn(emitter.pos, vx, vy)
```

**特性:**
- 時間経過で弾間隔が広がる
- 後発の高速弾グループが先発の低速弾を追い越し、複雑な密度変化を生む

**組み合わせ:**
- **Stack of Rings**: リング × 速度段階 → 同心円状の弾幕
- **Stack of Spreads**: 扇形 × 速度段階 → 逃げ道のある壁

**参照元:** Sparen Guide A3

---

### 1.4 自機狙い弾（Aimed）

自機の現在位置に向かって発射する。

```
dx = player.x - emitter.x
dy = player.y - emitter.y
angle = atan2(dy, dx)
vx = speed * cos(angle)
vy = speed * sin(angle)
```

**距離ベース方式（正規化）:**
```
dist = sqrt(dx*dx + dy*dy)
vx = dx / dist * speed
vy = dy / dist * speed
```

**バリエーション:**
- **単発自機狙い**: 1発のみ自機方向
- **自機狙い連射**: 一定間隔で連続発射（切り返しで誘導可能）
- **自機狙いn-way**: 自機方向を中心に扇状展開
- **自機外し弾**: 自機方向から意図的にずらした偶数way

**参照元:** Qiita弾幕の初歩, ニコニコ大百科「自機狙い」, 東方弾幕基本講座

---

### 1.5 固定弾（Fixed / Static）

自機位置に無関係に、毎回同じ方向・タイミングで発射される弾。

**バリエーション:**
- **完全固定弾**: 発射位置・角度・タイミングすべて固定。パターン暗記で対応。安全地帯が存在しうる
- **敵依存弾**: 敵の位置によって発射点が変わるが、敵の動きが固定なら結果的に固定パターン
- **自機依存固定弾**: 自機位置で弾の初期条件が決まるが、その後の軌道は固定

**参照元:** 東方弾幕基本講座, 東方原作弾幕研究

---

### 1.6 ランダム弾（ばらまき）

角度・速度にランダム性を持たせた弾。

```
angle = random(0, 360°)
speed = random(minSpeed, maxSpeed)
vx = speed * cos(angle)
vy = speed * sin(angle)
```

**設計注意:**
- 完全ランダムは理不尽になりやすい。速度を遅くするか、密度を下げて回避可能性を担保
- 角度のみランダムで弾配列自体は固定の場合もある → パターン化した動きで対応可能
- 弾幕の粗密を見極め、密度の薄い箇所を狙う「気合避け」が必要

**参照元:** 東方弾幕基本講座

---

### 1.7 連射弾（Rapid Fire / Stream）

一定間隔で同方向に連続発射する弾列。

```
if frameCount % fireInterval == 0:
    spawn(emitter.pos, angle, speed)
```

**バリエーション:**
- **固定方向連射**: 常に同じ角度。弾の壁を形成
- **自機狙い連射**: 自機方向に追従しながら連射。プレイヤーの動きで弾列の形が変わる
- **バースト連射**: N発を高速連射した後に休止期間

**参照元:** Qiita弾幕プログラミング入門

---

## 2. レーザー系パターン

### 2.1 固定レーザー（Straight Laser）

一定時間、固定角度で照射するレーザー。発射前に予告線（Delay Laser）を表示するのが一般的。

```
// 矩形当たり判定（回転座標系）
localX = (player.x - laser.x) * cos(-angle) - (player.y - laser.y) * sin(-angle)
localY = (player.x - laser.x) * sin(-angle) + (player.y - laser.y) * cos(-angle)
hit = |localX| < width/2 && 0 < localY < length
```

**実装パラメータ:**
- length: 512（画面を貫通する長さ）
- width: 最低20（予告線の視認性確保）
- delay: 発射前の予告フレーム数
- deleteTime: 照射持続フレーム数

**参照元:** Sparen Danmakufu Tutorial Lesson 9

---

### 2.2 回転レーザー（Rotating Laser）

固定レーザーの角度を毎フレーム変化させ、掃射するレーザー。

```
laserAngle += rotationSpeed * deltaTime
// 複数本を等間隔で配置
for i in 0..laserCount:
    angle = laserAngle + 360° / laserCount * i
    drawLaser(emitter.pos, angle, length, width)
```

**バリエーション:**
- **単方向回転**: 一方向に連続回転
- **往復回転**: 一定角度で折り返し
- **加減速回転**: 回転速度にイージングを適用

**参照元:** Sparen Danmakufu Tutorial

---

### 2.3 曲がるレーザー（Curving Laser / Sinuate Laser）

曲線を描くレーザー。セグメント分割で当たり判定を持つ。

```
theta = vangle - atan2(target.y - y, target.x - x)
if shouldTurnRight(theta):
    vangle += HOMING_ANGLE  // 例: 0.2 rad/frame
else:
    vangle -= HOMING_ANGLE
vx = HOMING_V * cos(vangle)
vy = HOMING_V * sin(vangle)
```

近距離でのオービット防止:
```
if distance < threshold:
    multiplier *= 1.1  // 角速度増加で収束
```

**当たり判定:** レーザー長をセグメント分割し、各セグメントに個別の矩形判定を配置（長さ96なら96個の判定）

**参照元:** MIS.W曲がるレーザー実装, Sparen Danmakufu Tutorial

---

### 2.4 予告レーザー（Delay / Telegraph Laser）

当たり判定を持たない視覚的な予告線。弾やレーザーの軌道を事前に示す。

```
// 判定なし + 視覚のみのレーザーを表示
spawnDelayLaser(pos, angle, length, width, duration)
wait(duration)
// 予告終了後に本体を発射
spawnActualAttack(pos, angle)
```

**用途:** 高速弾の軌道予告、ボス攻撃の合図

**参照元:** Sparen Danmakufu Tutorial Lesson 9

---

### 2.5 拡散レーザー（Spread Laser）

複数本のレーザーを扇状に同時照射。

```
for i in 0..n:
    angle = centerAngle - arcAngle/2 + arcAngle/(n-1) * i
    spawnLaser(emitter.pos, angle, length, width, duration)
```

---

### 2.6 反射レーザー

壁で反射するレーザー。グラディウスIIIのビッグコアMk-IIIが代表例。

```
// レーザーの先端が壁に到達したら反射角を計算
if hitWall(laserTip):
    reflectAngle = calculateReflection(laserAngle, wallNormal)
    spawnReflectedSegment(hitPoint, reflectAngle, remainingLength)
```

**参照元:** グラディウスシリーズ ビッグコア解説

---

### 2.7 マスタースパーク型（Beam）

画面の大部分を覆う極太レーザー。東方のマリサが代表例。

**特徴:**
- 発射前に長い溜めモーション
- 照射中は画面の大部分が判定範囲
- 回避手段が限られる（画面端に逃げる等）

---

## 3. 複合パターン

### 3.1 渦巻き弾（Spiral）

Ringの発射角度を毎フレーム回転させることで生成。

```
baseAngle += rotationSpeed  // 例: 8°/frame
for i in 0..n:
    angle = baseAngle + 360° / n * i
    vx = speed * cos(angle)
    vy = speed * sin(angle)
    spawn(emitter.pos, vx, vy)
```

**バリエーション:**
- **単渦巻き**: 1系統のアームが回転
- **二重渦巻き（永夜叉型）**: 2つの渦巻きを逆回転で同時発射
- **三重渦巻き**: 120°間隔の3アーム
- **加速渦巻き**: 回転速度を時間で変化
- **渦巻き + 隙間**: 一定間隔で発射を止め、通り抜け可能な切れ目を作る

```
// 二重渦巻き
blueAngle += rotationSpeed
redAngle -= rotationSpeed
```

**参照元:** Qiita弾幕プログラミング入門, Re:ゼロから始める弾幕アルゴリズム

---

### 3.2 ワインダー（Winder）

sine波で発射角度を揺らし、檻のような弾幕を生成。

```
dir = baseDirection + sin(count * Deg2Rad) * amplitude
// amplitude = 30° 程度
```

自機狙いのbaseDirectionと組み合わせると、追従する檻になる。

**参照元:** Re:ゼロから始める弾幕アルゴリズム

---

### 3.3 交差弾（Cross Pattern）

複数の発射源から異なるパターンを同時展開し、弾道が交差する配置。

```
// 左右2箇所から斜め方向にSpreadを発射
leftEmitter:  aimAngle = -30° (右下方向)
rightEmitter: aimAngle = 210° (左下方向)
// 画面中央付近で弾道が交錯
```

**バリエーション:**
- **X字交差**: 対角線上の2発射源から中央へ
- **十字交差**: 上下左右の4発射源
- **回転十字**: 十字パターンを回転させながら発射

---

### 3.4 停止→再発射（Pause and Redirect）

弾が一定時間後に停止し、再度自機方向へ発射される。

```
// Phase 1: 通常飛行
if frameCount < pauseFrame:
    pos += velocity
// Phase 2: 停止
elif frameCount < pauseFrame + waitFrames:
    // 静止
// Phase 3: 再狙い
else:
    angle = atan2(player.y - pos.y, player.x - pos.x)
    velocity = (cos(angle), sin(angle)) * newSpeed
    pos += velocity
```

**参照元:** DeepWiki touhou RumiaPatternD

---

### 3.5 子弾生成（Option Shot / 弾を撃つ弾）

弾が一定時間後に消滅し、その位置から複数の子弾を生成。

```
if lifetime >= maxLifetime:
    for i in 0..childCount:
        childAngle = 360° / childCount * i
        spawnChild(pos, childAngle, childSpeed)
    destroy(self)
```

**バリエーション:**
- **時限式**: 一定フレーム後に分裂
- **位置式**: 特定座標到達で分裂
- **多段分裂**: 子弾がさらに孫弾を生成

**参照元:** Qiita弾幕プログラミング入門

---

### 3.6 壁弾幕（Wall Pattern）

Spreadの密度を極端に上げ、通り抜け不可能な壁を形成。壁の隙間でルートを制限する。

```
// 360度のうち特定区間を除外して弾を配置
for i in 0..n:
    angle = 360° / n * i
    if angle > gapStart && angle < gapEnd:
        continue  // 隙間
    spawn(emitter.pos, angle, speed)
```

**参照元:** Sparen Guide A4 (Macrododge)

---

### 3.7 花弁弾幕（Flower / Petal Pattern）

速度のsin波変調で花弁状の軌道を描くパターン。

```
angle += angularVelocity * deltaTime
speed = baseSpeed + amplitude * sin(time * frequency)
vx = speed * cos(angle)
vy = speed * sin(angle)
```

speedが0付近まで落ちると弾が中心近くに戻り、再度外へ → 花弁の形状を生む。

**参照元:** Sparen Guide A6

---

### 3.8 弾幕+レーザー混合

弾とレーザーを同時展開し、回避の自由度を制限するパターン。

**例:**
- 回転レーザー（通り抜け不可） + ばらまき弾（隙間を縫う）
- 固定レーザーで画面分割 + 各区画内に自機狙い弾

---

### 3.9 ストリーミング（Streaming / 誘導回避）

自機狙い連射を意図的に誘導し、弾列を1箇所にまとめてから切り返す技術。パターンというよりプレイヤー側テクニックだが、設計上これを前提にした弾幕が多い。

```
// 自機狙い弾を一定方向に撃たせ続け
// 切り返しで弾列をずらし、生まれた隙間を通る
```

**参照元:** 東方弾幕基本講座, Boghog's shmup 101

---

### 3.10 BoWaP（Border of Wave and Particle）

大量の弾を波のように一斉発射し、個々の弾が粒子として分散するパターン。wave-like（密集した流れ）とparticle-like（個別の弾）の境界を行き来する設計。

**参照元:** Sparen Guide A5

---

## 4. 弾の挙動アルゴリズム

### 4.1 直進弾（Linear）

```
pos.x += vx * deltaTime
pos.y += vy * deltaTime
```

### 4.2 加速弾（Accelerating）

```
speed += acceleration * deltaTime
speed = clamp(speed, minSpeed, maxSpeed)
vx = speed * cos(angle)
vy = speed * sin(angle)
```

### 4.3 減速弾（Decelerating）

加速弾の逆。高速で発射され徐々に減速。停止後に再発射パターンと組み合わせることが多い。

```
speed -= deceleration * deltaTime
if speed <= 0:
    speed = 0
    // → 停止状態。再発射トリガーを待つ
```

### 4.4 加減速弾（花弁弾 / Petal）

speedにsin波を適用し、加減速を周期的に繰り返す。

```
speed = baseSpeed + amplitude * sin(time * frequency)
```

**参照元:** Sparen Guide A6

---

### 4.5 角速度弾（Curving）

進行方向を毎フレーム回転させる。

```
angle += angularVelocity * deltaTime
vx = speed * cos(angle)
vy = speed * sin(angle)
```

- 角速度大 → 渦巻き状の軌道
- 角速度小 → 緩やかなカーブ
- 角速度を時間で変化 → S字カーブ等

**参照元:** Qiita弾幕プログラミング入門, Sparen Guide A6

---

### 4.6 ホーミング弾（Homing）

毎フレーム目標方向へ速度ベクトルを補正。

**ベクトル補間方式:**
```
desiredDir = normalize(target.pos - bullet.pos)
velocity += desiredDir * homingStrength
velocity = normalize(velocity) * speed
```

**角度補正方式:**
```
targetAngle = atan2(target.y - pos.y, target.x - pos.x)
angleDiff = targetAngle - currentAngle
currentAngle += sign(angleDiff) * min(|angleDiff|, maxTurnRate)
```

**バリエーション:**
- **完全追尾**: maxTurnRateが大きく、ほぼ確実に追尾
- **緩追尾**: maxTurnRateが小さく、大回りして追尾
- **時限追尾**: 一定フレーム後に追尾を停止し直進に切替

**参照元:** Qiita弾幕プログラミング入門

---

### 4.7 反射弾（Bouncing）

壁（画面端）で跳ね返る弾。東方の一部スペルカードで使用。

```
if pos.x < minX || pos.x > maxX:
    vx = -vx
if pos.y < minY || pos.y > maxY:
    vy = -vy
```

**バリエーション:**
- **回数制限反射**: N回反射後に消滅
- **無限反射**: 画面内を跳ね返り続ける
- **減衰反射**: 反射ごとに速度が減少

**参照元:** 東方スペルカード解析

---

### 4.8 分裂弾（Splitting）

一定条件で複数の子弾に分裂する。

```
if shouldSplit(bullet):  // 時間、距離、衝突等
    for i in 0..splitCount:
        childAngle = bullet.angle + spreadAngle * (i - splitCount/2)
        spawnChild(bullet.pos, childAngle, childSpeed)
    destroy(bullet)
```

**トリガー:**
- 時間経過
- 特定距離到達
- 壁衝突
- プレイヤー近接

---

### 4.9 重力弾（Gravity-Affected）

弾に重力加速度を適用。放物線軌道を描く。

```
vy += gravity * deltaTime
pos.x += vx * deltaTime
pos.y += vy * deltaTime
```

Undertaleでは青SOULモードで重力がプレイヤーに適用される（弾ではなくプレイヤー側の変化）。

---

### 4.10 ワープ弾（Teleporting）

一定条件で瞬間移動する弾。

```
if frameCount == warpFrame:
    pos = warpTarget  // 瞬間移動
    // 移動後に新しい方向・速度を設定
    angle = newAngle
    speed = newSpeed
```

---

### 4.11 属性変化弾（Color-Switching）

飛行中に極性/属性が変化する弾。斑鳩型システムで特に有効。

```
if frameCount % switchInterval == 0:
    polarity = 1 - polarity  // 白⇔黒を切替
```

プレイヤーに吸収/回避の判断を強制する。

---

### 4.12 設置弾（Stationary / Mine）

発射後に特定位置で停止し、一定時間後に爆発または弾幕を放射する。

```
// Phase 1: 移動して配置
if frameCount < placeFrame:
    pos += velocity
// Phase 2: 静止
elif frameCount < placeFrame + waitFrames:
    // 待機
// Phase 3: 爆発（Ringを放射）
else:
    for i in 0..n:
        spawnBullet(pos, 360°/n * i, explosionSpeed)
    destroy(self)
```

---

### 4.13 蛇行弾（Sine Wave）

進行方向に対して垂直にsin波で揺れながら移動する弾。

```
t += deltaTime
// 基本移動方向
baseX = speed * cos(angle) * deltaTime
baseY = speed * sin(angle) * deltaTime
// 垂直方向のsin波オフセット
perpAngle = angle + PI/2
offsetX = amplitude * sin(t * frequency) * cos(perpAngle) * deltaTime
offsetY = amplitude * sin(t * frequency) * sin(perpAngle) * deltaTime
pos.x += baseX + offsetX
pos.y += baseY + offsetY
```

---

### 4.14 螺旋弾（Helical / Orbital）

発射源の周囲を公転しながら外側へ拡散する弾。

```
orbitalAngle += orbitalSpeed * deltaTime
radius += expansionRate * deltaTime
pos.x = emitter.x + radius * cos(orbitalAngle)
pos.y = emitter.y + radius * sin(orbitalAngle)
```

---

## 5. 敵の移動パターン

### 5.1 直線降下

```
pos.y += descendSpeed * deltaTime
```

### 5.2 斜め降下

```
pos.x += horizontalSpeed * deltaTime
pos.y += descendSpeed * deltaTime
```

### 5.3 サイン波移動

```
pos.x = baseX + amplitude * sin(time * frequency)
pos.y += descendSpeed * deltaTime
```

### 5.4 円運動

```
orbitalAngle += orbitalSpeed * deltaTime
pos.x = centerX + radius * cos(orbitalAngle)
pos.y = centerY + radius * sin(orbitalAngle)
```

### 5.5 停止→射撃→離脱

```
// Phase 1: 進入
if phase == ENTER: moveToTarget()
// Phase 2: 停止して攻撃
elif phase == ATTACK: fireBullets()
// Phase 3: 離脱
elif phase == EXIT: moveOffscreen()
```

### 5.6 往復移動（ジグザグ）

```
pos.x += direction * speed * deltaTime
if pos.x > maxX || pos.x < minX:
    direction = -direction
```

### 5.7 編隊飛行（Formation）

複数の敵が相対位置を保ったまま移動。リーダーの動きに追従。

```
pos = leader.pos + formationOffset
```

### 5.8 Toaplanパターン（左右交互スポーン）

画面の左右交互に敵を出現させ、プレイヤーの横移動を強制する古典的パターン。

**参照元:** Boghog's shmup 101, bituse敵の移動パターン

---

## 6. パターン生成の数学

### 6.1 極座標 → 直交座標変換

```
x = r * cos(θ)
y = r * sin(θ)
```

弾幕のほぼ全パターンはこの変換が基盤。

### 6.2 自機方向の角度計算

```
angle = atan2(player.y - emitter.y, player.x - emitter.x)
```

### 6.3 距離計算（平方根回避）

```
distSq = dx*dx + dy*dy
hit = distSq <= (r1 + r2) * (r1 + r2)
```

### 6.4 リサージュ曲線

```
x = A * sin(a * t + δ)
y = B * sin(b * t)
```

- a:b = 1:1 → 楕円
- a:b = 1:2 → 8の字
- a:b = 3:2 → 複雑な閉曲線
- a:b が無理数 → 回転する開曲線

### 6.5 薔薇曲線（Rose Curve）

```
r = a * cos(k * θ)
x = r * cos(θ)
y = r * sin(θ)
```

- k=2 → 4弁の花
- k=3 → 3弁の花
- k=5 → 5弁の花
- kが有理数 → 複雑な花弁パターン

弾の発射位置や弾道にこの曲線を適用すると花弁状の弾幕が生成できる。

### 6.6 カージオイド（Cardioid / ハート型）

```
r = a * (1 + cos(θ))
x = r * cos(θ) = a * (1 + cos(θ)) * cos(θ)
y = r * sin(θ) = a * (1 + cos(θ)) * sin(θ)
```

ハート型の弾幕配置に使用。

### 6.7 アルキメデスの螺旋（Archimedean Spiral）

```
r = a + b * θ
x = r * cos(θ)
y = r * sin(θ)
```

θを時間で増加させると外側に広がる渦巻き。

### 6.8 対数螺旋（Logarithmic Spiral）

```
r = a * e^(b * θ)
```

自然界の渦巻き（オウムガイ等）に対応。アルキメデスより急速に広がる。

### 6.9 サイクロイド（Cycloid）

```
x = r * (t - sin(t))
y = r * (1 - cos(t))
```

転がる円の軌跡。弾の弾道に適用すると波打つような軌道を生成。

### 6.10 インボリュート（Involute）

```
x = r * (cos(t) + t * sin(t))
y = r * (sin(t) - t * cos(t))
```

糸巻きの糸を解くような軌道。渦巻きの変種として使用可能。

### 6.11 イージング関数

```
// EaseInQuad（加速）
t = t * t

// EaseOutQuad（減速）
t = 1 - (1 - t) * (1 - t)

// EaseInOutQuad
t < 0.5 ? 2*t*t : 1 - pow(-2*t + 2, 2) / 2

// EaseInOutSine（滑らかな加減速）
t = -(cos(PI * t) - 1) / 2
```

弾速、レーザー角速度、ボスの移動等あらゆる場面で使用。

---

## 7. 弾の属性パラメータ

| パラメータ | 型 | 説明 |
|---|---|---|
| Position | float2 | 現在座標 |
| Velocity | float2 | 速度ベクトル |
| Speed | float | 速さスカラー |
| Angle | float | 進行方向（rad） |
| Acceleration | float | 速度変化率 |
| AngularVelocity | float | 角速度（rad/frame） |
| MinSpeed / MaxSpeed | float | 速度制限 |
| ScoreValue | int | 吸収/破壊時のスコア |
| Polarity | byte | 極性（白/黒） |
| Faction | byte | 陣営（Player/Enemy） |
| Damage | int | ダメージ量 |
| Lifetime | int | 残存フレーム数 |
| Size / Radius | float | 描画サイズ / 当たり判定半径 |
| GraphicType | enum | Non-directional / Directional / Vector / Static |
| CanAbsorb | bool | 吸収可能か |
| Piercing | bool | 貫通するか |
| BehaviorFlags | flags | Homing / Bouncing / Splitting / GravityAffected 等 |
| HomingStrength | float | 追尾強度（0=直進, 1=即追尾） |
| BounceCount | int | 残り反射回数 |
| SplitTrigger | enum | Time / Distance / WallHit / None |
| ParentId | int | 分裂元の弾ID（子弾追跡用） |

**弾のグラフィック分類（Sparen Guide A1）:**
1. **Non-directional**: 円形。どの方向にも見える（点対称）
2. **Directional**: 楕円形。移動軸が視覚的に示される
3. **Vector**: 矢印型。正確な移動方向を示す
4. **Static-angled**: 回転しないグラフィック

---

## 8. ボス弾幕の設計哲学

### 8.1 スペルカード制（東方Project）

- ボスが複数の「スペルカード」（名前付き弾幕パターン）を持つ
- 各カードは制限時間付きで、撃破 or 時間切れで次へ移行
- 通常攻撃（ノンスペル）とスペルカードが交互に出現
- スペル取得（被弾なしクリア）でボーナススコア
- 紅魔郷64個、妖々夢87個、永夜抄114個のスペルカード

### 8.2 フェーズ制（CAVE系）

- ボスHPを区間で分割し、区間ごとにパターンが変化
- HP低下に伴い弾密度・速度が増加（怒り状態）
- 怒首領蜂: 最大245発同時表示、低速弾 + 極小当たり判定

### 8.3 ボスパターン設計原則

| 原則 | 説明 |
|---|---|
| **Light/Heavy交互** | 軽い弾幕と重い弾幕を交互に配置し、緩急をつける |
| **休息区間** | パターン間に短い安全時間を設ける |
| **弾消し報酬** | フェーズ移行時に画面上の弾をアイテム化 → 待機の動機 |
| **視覚的明瞭さ** | 弾の色とコアを明確に。背景は中間色 |
| **パターン多様性** | 同じボスでもAimed/Static/Randomを混ぜる |
| **学習曲線** | 初見殺しは避け、パターン認識で上達可能にする |
| **敵の役割分担** | Pressure（圧力）, Area Denial（範囲封鎖）, Direct Challenge（直接攻撃） |
| **ランク制考慮** | プレイヤーの腕前に応じて弾速・弾量を動的変化（バトルガレッガ） |

### 8.4 ボス攻撃の構成例

| フェーズ | パターン構成 | 意図 |
|---|---|---|
| Phase 1 | 自機狙い3way + 固定リング | パターン学習。基本避けの確認 |
| Phase 2 | 渦巻き + 交差弾 | 密度増加。移動範囲の制限 |
| Phase 3 | 回転レーザー + ばらまき | 回避経路の制限 + 瞬時判断の要求 |
| Phase 4（怒り）| 高速自機狙い + 高密度リング + 設置弾 | 総力戦。パターン切替の速度も増加 |

**参照元:** Boghog's bullet hell shmup 101, Sparen Danmaku Design Studio

---

## 9. 極性・属性システム

### 9.1 斑鳩のメカニクス

| 要素 | 仕様 |
|---|---|
| 属性数 | 2（白 / 黒） |
| 切替 | ボタン1つで即時切替 |
| 同属性弾 | 吸収（ゲージ+0.1本、スコア+100） |
| 逆属性弾 | ミス（即死） |
| 同属性攻撃 | 通常ダメージ |
| 逆属性攻撃 | 2倍ダメージ |
| チェーン | 同属性3連続撃破でボーナス（100→200→...→25600、9チェーン上限） |
| 力の解放 | ゲージ消費で誘導レーザー。近距離優先割り当て |

### 9.2 エスプガルーダの覚聖システム

| 要素 | 仕様 |
|---|---|
| 覚聖 | ボタンで変身。ショット性能変化・強化 |
| 弾速変化 | 覚聖中は敵弾が紫に変色し低速化 |
| 弾消し | 覚聖中に敵を倒すとその敵の弾が金塊に変化 |
| ガードバリア | ゲージ消費で無敵+弾消し。解除時にバリアアタック |
| カウンター0 | 覚聖カウンター0で弾速が逆に上昇。リスク/リワード |

### 9.3 UndertaleのSOULモードシステム

| モード | 色 | メカニクス |
|---|---|---|
| Normal | 赤 | 自由移動。全方向に一定速度 |
| Jump | 青 | 重力適用。ジャンプで回避。プラットフォーム要素 |
| Shield | 緑 | 移動不可。代わりに盾で4方向からの弾を防御 |
| Trap | 紫 | 水平線上のみ移動可。線の切替で上下移動 |
| Shooter | 黄 | 上下反転。プレイヤーが弾を撃てる |

**特殊攻撃タイプ:**
- **青弾**: 静止していれば無害。動くとダメージ
- **橙弾**: 動いていれば無害。静止するとダメージ
- **白弾**: 通常の回避必須弾
- **灰弾**: 演出用。ダメージなし
- **KARMA**: Sansの毒ダメージ。被弾後もHPが徐々に減少（最大40）

### 9.4 虫姫さまのモード別弾幕差異

| モード | 弾速 | 弾量 | 特徴 |
|---|---|---|---|
| オリジナル | 高速 | 少量 | 従来型STGに近い。弾避けより敵撃破重視 |
| マニアック | 低速 | 大量 | 弾幕系。弾の隙間を縫う操作 |
| ウルトラ | 低速 | 超大量 | 超高難度弾幕。最大2000発同時表示 |

### 9.5 ケツイのロックオンシステム

| 要素 | 仕様 |
|---|---|
| ショット | 正面への通常射撃 |
| ロックショット | 敵をロックし追尾する攻撃。ビットが全方向に追尾攻撃 |
| チップ | 敵を近距離で倒すほど高倍率のチップが出現 |
| 倍率タイマー | ロックショット使用時に倍率チップが爆風から出現 |

### 9.6 バトルガレッガのランクシステム

| ランク上昇条件 | 影響 |
|---|---|
| ショット発射数（フレーム単位） | 弾速・弾量増加 |
| アイテム取得 | ランク微増 |
| パワーアップ状態 | ランク上昇 |
| 残機数 | ランク上昇 |
| **自機破壊** | **ランク大幅減少** → 意図的自殺が戦略に |

**参照元:** 斑鳩システム解説, エスプガルーダ攻略, Undertale SOUL Modes, Wikipedia各作品

---

## 10. 密度設計（Density Design）

### 10.1 Macrododge vs Micrododge

| 種別 | 特徴 | 例 |
|---|---|---|
| **Macrododge** | 密集した弾群を大きく避ける。画面全体を使う移動 | 壁弾幕、同角度リング |
| **Micrododge** | 小さな隙間を精密に抜ける。狭い範囲に集中 | 低速ばらまき、多層パターン |

設計上は両方を交互に配置し、緩急をつけるのが理想。

### 10.2 Spatial vs Temporal Density

- **Spatial（空間的）**: ある瞬間の画面上の弾数・密集度
- **Temporal（時間的）**: 弾の発射タイミングの間隔・頻度

弾速が上がると空間密度は下がるが、プレイヤーの注意範囲が広がる。弾速が下がると空間密度は上がるが、精密回避が可能。

### 10.3 Negative Space（安全地帯の設計）

発射源を移動させながら時間差で弾群を発射することで、弾のない空間（通り道）を意図的に作る。

```
for t in 0..duration:
    emitterPos = center + radius * (cos(t * moveSpeed), sin(t * moveSpeed))
    spawnBulletGroup(emitterPos, aimAngle, speed)
    wait(interval)
```

### 10.4 弾数と難易度の非比例

- **弾数多 ≠ 高難度**: 画面端に集中して即座に離脱する弾が多い場合、通り道は広い
- **弾数少 ≠ 低難度**: 角速度・加速度・ランダム性で少数でも高難度化可能

### 10.5 視覚的明瞭さの原則

- **高コントラスト**: 弾のコア（明）+ 外縁（暗）でサンドイッチ
- **背景は中間色**: 極端な明暗を避け、弾の視認性を確保
- **弾色の統一**: 赤/ピンク/紫を主体とし、爆発エフェクトやアイテムと混同しない配色

**参照元:** Sparen Guide A4, Boghog's shmup 101

---

## 11. Sansの攻撃パターン一覧（Undertale）

弾幕ゲームデザインの参考として、Sans戦の24種類の攻撃パターン:

### 第1フェーズ（13攻撃）
1. 骨の回廊（横から骨が流れる + ジャンプ回避）
2. Gaster Blaster 4連（画面四方から交差ビーム）
3. Gaster Blaster X字（対角線からのビーム）
4. 骨のプラットフォーム（重力下でプラットフォームを渡る）
5. 青/白骨の交互（青は静止、白は回避）
6. 重力スラム（SOULを壁に叩きつける）
7. 骨の迷路（上下の骨が交互に迫る）
8. 高速Gaster Blaster（大型2連ビーム）
9. 骨 + Blaster混合
10. 重力方向切替（上下左右に重力を切替えながら骨回避）
11. プラットフォーム + Blaster
12. 骨の波（下から骨が波状に出現）
13. 大量Blaster掃射

### 第2フェーズ（11攻撃）
14-24. より高速・高密度の変種。パターン組み合わせが複雑化。

### 設計上の特徴
- **SOULモード切替**: 赤（自由移動）↔ 青（重力）を攻撃中に切替
- **KARMA**: 被弾後もHPが削れ続ける持続毒ダメージ
- **メタ攻撃**: ターン順序を無視して先制攻撃、メニュー欄への干渉

**参照元:** Undertale Wiki Sans/In Battle

---

## 12. 代表作品の弾幕特徴（詳細）

### 12.1 東方Project（ZUN）
- **スペルカード制**: 名前付き弾幕。制限時間+ノーミスボーナス
- **低速弾重視**: 弾の美しさを楽しむ設計。高速弾は少数
- **幾何学パターン**: 円、螺旋、花弁、対称性のある美しい配置
- **難易度4段階**: Easy/Normal/Hard/Lunatic で弾量・弾速・パターン変化
- **グレイズ**: 弾の近くを通るとスコアボーナス
- **弾消し**: ボム使用で画面上の弾を消去+無敵

### 12.2 怒首領蜂シリーズ（CAVE）
- **最大245発同時表示**（初代）
- **極小当たり判定**: プレイヤーの見た目より遥かに小さい判定
- **ショット/レーザー切替**: 広範囲ショットと高威力レーザーの切替
- **ハイパー**: 弾が加速するがスコアが大幅に増える
- **真ボス（火蜂/緋蜂）**: 2周目クリアで出現する超高難度ボス

### 12.3 斑鳩（トレジャー）
- **極性2色システム**: 詳細は9.1参照
- **パズル的構成**: 弾避けよりも「正しい極性選択」の思考が中心
- **パターン完全固定**: ランダム要素なし。完全暗記が前提
- **芸術的弾幕配置**: 白黒の弾が交互に織りなすビジュアル

### 12.4 虫姫さま（CAVE）
- **最大2000発同時表示**（ウルトラ）
- **モード別弾幕差異**: 詳細は9.4参照
- **中大型敵破壊で弾消し**: 戦略的に大きい敵を残して弾消しタイミングを調整

### 12.5 エスプガルーダ（CAVE）
- **覚聖システム**: 詳細は9.2参照
- **弾速操作**: プレイヤーが敵弾速度を制御できるユニークなシステム

### 12.6 ケツイ（CAVE）
- **ロックオンシステム**: 詳細は9.5参照
- **接近戦リスク/リワード**: 近距離ほど高得点だがミスのリスク大

### 12.7 グラディウスシリーズ（コナミ）
- **パワーアップカプセル制**: スピード→ミサイル→ダブル→レーザー→オプション→バリア
- **オプション（分身）**: 自機の動きを追従する分身が最大4つ
- **ビッグコア**: 遮蔽板を破壊してコアを露出させるボス設計
- **地形連動**: 弾幕と地形の組み合わせで回避経路を制限
- **ダブル vs レーザー**: 広範囲低火力 vs 狭範囲高火力の選択

### 12.8 R-TYPE（アイレム）
- **波動砲**: チャージショット。溜め時間で威力変化
- **フォース**: 着脱可能な無敵ユニット。前方/後方装着で攻撃方向変化
- **フォースシュート**: フォースを射出して独立攻撃
- **生体的弾幕**: バイド（敵勢力）の有機的・生物的な弾道

### 12.9 ダライアスシリーズ（タイトー）
- **巨大魚型ボス**: 水棲生物モチーフのボスデザイン
- **多段関節攻撃**: ボスの各パーツが独立して攻撃
- **3画面筐体**（初代）: 横に広い画面でスケール感を演出
- **ルート分岐**: ステージクリア後に次ステージを選択
- **パワーアップ3色**: 赤（対空）, 緑（対地）, 青（防御）

### 12.10 バトルガレッガ（ライジング）
- **ランクシステム**: 詳細は9.6参照
- **意図的自殺**: ランク調整のため戦略的にミスする文化
- **弾速動的変化**: ランクに応じて弾速がリアルタイムに変化

### 12.11 Undertale / Deltarune（Toby Fox）
- **RPG+弾幕回避**: 戦闘がリアルタイム弾幕回避
- **SOULモードシステム**: 詳細は9.3参照
- **物語と弾幕の融合**: キャラクターの感情・個性が弾幕パターンに反映
- **骨・Gaster Blaster**: 独自のビジュアル言語
- **メタ要素**: Sans戦のターン順序破壊等

### 12.12 式神の城（アルファ・システム）
- **式神システム**: 通常ショットと式神攻撃の切替
- **式神展開中**: 移動速度低下、式神の攻撃範囲内の弾を消去
- **キャラクター選択**: 使用キャラで式神の性能が変化

---

## 13. 実装パターン（パフォーマンス）

### 13.1 データ構造

弾をGameObjectにせず、struct配列で管理する。

```csharp
public struct BulletState {
    public float2 Position;
    public float2 Velocity;
    public float Speed;
    public float Angle;
    public float Acceleration;
    public float AngularVelocity;
    public byte Polarity;
    public byte Faction;
    public int Damage;
    public float ScoreValue;
    public int Lifetime;
    public byte BehaviorFlags;
}
```

### 13.2 描画

**Graphics.DrawMeshInstanced:**
- 1バッチ最大1023個
- MaterialPropertyBlockで色・サイズを個別設定
- 1ドローコール/弾種で数千発を処理可能

**Graphics.DrawMeshInstancedIndirect:**
- ComputeBufferで弾データをGPUに送信
- 1ドローコール/弾種で2万発以上対応

### 13.3 移動処理

**Unity Job System + Burst:**
```csharp
[BurstCompile]
public struct BulletMoveJob : IJobParallelFor {
    public NativeArray<BulletState> bullets;
    public float deltaTime;

    public void Execute(int i) {
        var b = bullets[i];
        b.Speed += b.Acceleration * deltaTime;
        b.Angle += b.AngularVelocity * deltaTime;
        b.Velocity = new float2(
            b.Speed * math.cos(b.Angle),
            b.Speed * math.sin(b.Angle)
        );
        b.Position += b.Velocity * deltaTime;
        bullets[i] = b;
    }
}
```

### 13.4 当たり判定

円形判定が標準。距離の2乗比較でsqrt回避。

```csharp
float distSq = math.distancesq(bullet.Position, player.Position);
bool hit = distSq <= (bulletRadius + playerRadius) * (bulletRadius + playerRadius);
```

### 13.5 オブジェクトプール

リンクリスト方式のフリーリスト:
```
[Active] → bullet1 → bullet2 → ...
[Free]   → bullet3 → bullet4 → ...

Spawn: Free.Pop() → Active.Add()
Despawn: Active.Remove() → Free.Push()
```

### 13.6 パフォーマンス注意事項

- Rigidbodyは使わない（重い）
- LINQを避ける（GCアロケーション → 3倍遅化の報告あり）
- NativeArray + Job Systemで100fps以上を維持（4096発）
- Update() + LateUpdate()でジョブのスケジュールと完了を分離

**参照元:** Little Polygon Tech Breakdown, Unity-Bullet-Hell GitHub, 20k bullets itch.io

---

## 14. BulletML（弾幕記述言語）

XMLベースの弾幕パターン記述フォーマット。ABA Games開発。

**主要要素:**
- `<bullet>`: 弾の定義（direction, speed）
- `<fire>`: 発射アクション
- `<action>`: 一連の動作シーケンス
- `<changeDirection>`: 方向変更
- `<changeSpeed>`: 速度変更
- `<repeat>`: 繰り返し
- `<wait>`: 待機フレーム
- `<vanish>`: 弾の消滅

**利点:** パターンをデータとしてアセット管理でき、コード変更なしにパターン追加可能。

**実装:**
- C++: libbulletml
- F#: FsBulletML（内部DSL + XML/SXML外部DSL）
- Go: go-bulletml

**Bulletsmorph:** 遺伝的プログラミングでBulletMLパターンを自動生成するエンジンも存在。

**参照元:** BulletML公式サイト(ABA Games), GitHub FsBulletML, GitHub go-bulletml

---

## 15. Action_002への応用ポイント

### 極性システムとの組み合わせ

- 白黒混合弾幕で極性切替判断を強制
- 属性変化弾（飛行中に白⇔黒）でタイミング判断を追加
- 同属性吸収をスコア/ゲージに変換する既存システムと連携

### リズムシステムとの組み合わせ

- ビートに合わせた弾の発射タイミング
- 表拍（プレイヤー攻撃）と裏拍（敵攻撃）の交互構造を活かしたパターン設計
- テンポ変化で弾幕の密度を動的に変化

### 推奨パターン優先度

| 優先度 | パターン | 理由 |
|---|---|---|
| 高 | Ring, Aimed, Spiral | 基本中の基本。最小実装で最大効果 |
| 高 | 停止→再発射 | 極性切替の判断タイミングを作りやすい |
| 中 | Spread(n-way), Stack | バリエーション追加 |
| 中 | 回転レーザー | ボス戦の緊張感 |
| 中 | 属性変化弾 | 極性システムとの相乗効果 |
| 低 | 反射弾, 分裂弾 | 実装コスト高。余裕があれば |
| 低 | ホーミング, 蛇行弾 | ボス後半フェーズ用 |

---

## Sources

### Tier A（公式・技術文献）
- [BulletML公式](https://www.asahi-net.or.jp/~cs8k-cyu/bulletml/index_e.html) - ABA Games弾幕記述言語仕様
- [斑鳩 Wikipedia](https://ja.wikipedia.org/wiki/%E6%96%91%E9%B3%A9_(%E3%82%B7%E3%83%A5%E3%83%BC%E3%83%86%E3%82%A3%E3%83%B3%E3%82%B0%E3%82%B2%E3%83%BC%E3%83%A0)) - 極性システム仕様
- [Undertale SOUL Modes (Cyberpedia)](https://cyberpedia.miraheze.org/wiki/UNDERTALE_SOUL_Modes) - SOULモードシステム
- [Undertale Attack Types (Cyberpedia)](https://cyberpedia.miraheze.org/wiki/Attack_Types) - 攻撃属性分類

### Tier B（専門解説）
- [Sparen's Danmaku Design Studio](https://sparen.github.io/ph3tutorials/danmakudesign.html) - 弾幕設計理論の包括的ガイド（A1〜A6）
- [Sparen's Danmakufu Tutorial Lesson 9](https://sparen.github.io/ph3tutorials/ph3u1l9.html) - レーザー実装詳細
- [Boghog's bullet hell shmup 101](https://shmups.wiki/library/Boghog's_bullet_hell_shmup_101) - STG設計の包括的ガイド
- [DeepWiki touhou Bullet Patterns](https://deepwiki.com/shoraaa/touhou/6.1-bullet-patterns) - 東方風パターン実装例
- [Unity-Bullet-Hell (GitHub)](https://github.com/jongallant/Unity-Bullet-Hell) - DrawMeshInstancedIndirect実装
- [東方弾幕基本講座 (東方Wiki)](https://wikiwiki.jp/thk/Lesson) - 弾種分類と回避理論
- [東方原作弾幕研究 (RANDOM)](https://ch-random.net/post/303/) - 弾幕パターン分類体系

### Tier C（技術ブログ）
- [弾幕シューティングに現れる数学 (Qiita)](https://qiita.com/Snowman-s/items/56bbf19304330be4e258) - 数学基礎
- [弾幕の初歩 距離系と角度系 (Qiita)](https://qiita.com/hp0me/items/1164bf9669a825d76ffa) - 2つの弾道計算方式
- [p5.js弾幕プログラミング入門 (Qiita)](https://qiita.com/WGG_SH/items/e4c12fb6ff62d2502fbd) - 7パターンの実装例
- [Re:ゼロから始める弾幕アルゴリズム](https://noranuk0.hatenablog.com/entry/2016/10/29/235004) - Unity実装5パターン
- [斑鳩システム解説 (E.G.I.ブログ)](https://egimal.hatenablog.com/entry/2021/02/12/023704) - 極性・チェーン・解放の詳細
- [曲がるレーザー実装 (MIS.W)](https://blog.misw.jp/entry/archives/674) - ホーミングレーザーアルゴリズム
- [Bullet Hell Tech Breakdown (Little Polygon)](https://blog.littlepolygon.com/posts/bullets/) - Job System + プール実装
- [シューティングゲームでよく使う数式 (作っちゃうおじさん)](https://hothukurou.com/blog/post-951) - STG数式まとめ
- [敵の移動パターン (bituse)](https://bituse.info/game/shot/12) - 敵移動アルゴリズム
- [ビッグコア (Pixiv百科事典)](https://dic.pixiv.net/a/%E3%83%93%E3%83%83%E3%82%B0%E3%82%B3%E3%82%A2) - グラディウスボス設計
- [エスプガルーダ攻略](http://t-yosi.main.jp/memo/espgaruda.htm) - 覚聖システム詳細
- [バトルガレッガ ランク (Wiki)](https://wikiwiki.jp/garegga/Battle%20Garegga/%E3%83%A9%E3%83%B3%E3%82%AF) - ランクシステム詳細
- [ケツイ システム (CAVE公式)](https://www.cave.co.jp/gameonline/ketsui/system.html) - ロックオンシステム
