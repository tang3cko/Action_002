---
title: 画像生成プロンプト集
game: Action_002 (Polarity Survivors)
author: Hoshino Ayane (Illustrator, Labee LLC)
version: v1.4
date: 2026-03-22
target_tool: Gemini CLI nanobanana extension
reference_doc: character-setting.md
---

# 画像生成プロンプト集 — Polarity Survivors v1.4

## プロンプト設計方針

### スプライトテクスチャの技術仕様

このゲームのレンダリングシステムは DrawMeshInstanced + URP Unlit を使用しており、テクスチャにシェーダーが極性カラーを乗算する。
そのため、**スプライト用プロンプト（2〜5）はすべて「白いシルエット on 透過背景」を指定する**。実際のゲーム内カラーはシェーダーが付与するため、生成画像自体に白黒の色分けは不要。

ただし**ボスのみSpriteRenderer使用**のため、白/黒バージョンを個別カラーで指定する。
背景プロンプト（7）はフルカラーで指定する。

### スプライトシート形式について

すべてのスプライト（背景を除く）をスプライトシート形式で出力する。
スプライトシートとは1枚の画像内に複数のフレームやバリエーションをグリッド配置した形式であり、Unity の Sprite Editor でのスライスを前提とする。

- グリッド配置：各フレームを同一サイズのセルに揃えて配置する
- 余白：セル間の境界を薄いガイド線で明示する（Unityスライス作業の目安用）
- 背景：シート全体は透過背景（PNG with alpha）
- カメラ距離・スケールは全フレーム完全一致
- 視点：トップダウン（真上から）

### ネガティブプロンプト（全スプライト共通）

```
NEGATIVE: colored background, gradient background, shadow behind subject, drop shadow, photorealistic, 3D render, anime face, human face, text, watermark, existing IP characters, copyright characters, signature, colored border frame
```

---

## 1. コンセプトアート（世界観を伝える1枚絵）

**用途：** ゲーム紹介ページ、SNSサムネイル
**出力サイズ：** 1920×1080 または 960×540（16:9）
**ファイル形式：** JPEGまたは透過不要のPNG
**対象：** ゲームジャム参加者、アクションゲームファン

```
Concept art for a 2D top-down action shooter game with yin-yang duality theme.
The scene depicts a cosmic battlefield split vertically down the center: the left half is deep dark navy (#1A1A2E) and the right half is warm off-white (#F0EEE6).
At the center, a single circular player character based on the taijitu symbol (yin-yang circle) hovers at the boundary between the two worlds, glowing with a subtle aura mixing both colors.
On the navy side, geometric white enemies (triangles, hexagonal rings, three-pointed rotating forms) emerge from darkness.
On the off-white side, identical geometric dark enemies mirror them.
Small teardrop-shaped magatama projectiles streak through the air in both white and dark navy tones.
At the top of the composition, two colossal slime-like guardian figures face each other: one off-white fluid tower with a navy eye circle floating in its head region, one dark navy fluid tower with an off-white eye circle. They are tall, amorphous, and awe-inspiring — cosmic sentinels made of living polarity energy.
The background shows flowing wave patterns in yin-yang style, suggesting energy currents and a fragile cosmic order about to shatter.
Style: flat graphic illustration, bold clean vector-influenced linework, geometric and symbolic, inspired by traditional East Asian cosmological diagrams, high contrast, minimal gradients, dramatic atmosphere.
Aspect ratio 16:9.
```

---

## 2. プレイヤーキャラクター スプライトシート

**用途：** DrawMeshInstanced テクスチャ（グレースケール乗算）
**出力サイズ：** 512×128 px（4列×1行）
**各コマサイズ：** 128×128 px
**ファイル形式：** PNG with transparent background
**対象：** ゲームプレイ中のプレイヤー表示
**Unityスライス：** 128×128 で4分割

```
2D game sprite sheet, top-down view, canvas size 512x128 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 128x128 pixels.
Draw a thin visible guide line at each cell boundary (x=128, x=256, x=384) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
Subject: a taijitu (yin-yang) circle symbol used as a top-down player character.

Frame 1 (leftmost, x 0-127px): The taijitu at 0 degrees rotation.
Frame 2 (x 128-255px): The taijitu at 90 degrees clockwise rotation.
Frame 3 (x 256-383px): The taijitu at 180 degrees rotation.
Frame 4 (rightmost, x 384-511px): The taijitu at 270 degrees clockwise rotation.

Each frame:
Pure white silhouette design on fully transparent background.
The circle is divided by a smooth flowing S-curve through the center.
One half has a small filled circle (the "eye") near its edge; the opposite half also has a small filled circle.
Clean flat vector shape, no gradients, no shading, no outlines beyond the shape edge.
Crisp alpha edges suitable for alpha-clip threshold rendering.
Subject centered within each cell with uniform small padding on all sides.
No background, no shadow, no glow, no border, no frame label.
Style: minimal flat game sprite, symbolic, geometric precision.
All 4 frames must be pixel-aligned and uniformly sized.
```

---

## 3-A. Shooter スプライトシート

**用途：** EnemyVisualConfigSO → DrawMeshInstanced テクスチャ
**出力サイズ：** 256×64 px（4列×1行）
**各コマサイズ：** 64×64 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 64×64 で4分割

```
2D game sprite sheet, top-down view, canvas size 256x64 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 64x64 pixels.
Draw thin visible guide lines at cell boundaries (x=64, x=128, x=192) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
All frames: pure white silhouette on fully transparent background, crisp flat vector shapes, no gradients, no shading.
Subject: Shooter enemy for a top-down bullet-hell shooter.
Design: a sharp equilateral triangle. Slightly inward-curved sides suggesting aerodynamic tension. Small notch or angular cut at the base center. Bold and aggressive silhouette.

Frame 1 (x 0-63px): triangle pointing upward (0°)
Frame 2 (x 64-127px): triangle pointing right (90°)
Frame 3 (x 128-191px): triangle pointing downward (180°)
Frame 4 (x 192-255px): triangle pointing left (270°)

Centered within each cell with small padding. No background, no shadow, no gradient.
Style: minimal flat game sprite, geometric, instantly readable as a directional threat.
```

---

## 3-B. NWay スプライトシート

**用途：** EnemyVisualConfigSO → DrawMeshInstanced テクスチャ
**出力サイズ：** 256×64 px（4列×1行）
**各コマサイズ：** 64×64 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 64×64 で4分割

```
2D game sprite sheet, top-down view, canvas size 256x64 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 64x64 pixels.
Draw thin visible guide lines at cell boundaries (x=64, x=128, x=192) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
All frames: pure white silhouette on fully transparent background, crisp flat vector shapes, no gradients, no shading.
Subject: NWay enemy for a top-down bullet-hell shooter.
Design: a three-way rotational symmetry symbol. Three curved blade-like arms extending outward from a central hub, each arm swept in the same rotational direction creating a pinwheel or triple-comma (tomoe) crest configuration from Japanese heraldry.

Frame 1 (x 0-63px): rotation at 0°
Frame 2 (x 64-127px): rotation at 30°
Frame 3 (x 128-191px): rotation at 60°
Frame 4 (x 192-255px): rotation at 90°

Centered within each cell. No background, no shadow, no gradient.
Style: minimal flat game sprite, symbolic, Japanese heraldic influence.
```

---

## 3-C. Ring スプライトシート

**用途：** EnemyVisualConfigSO → DrawMeshInstanced テクスチャ
**出力サイズ：** 256×64 px（4列×1行）
**各コマサイズ：** 64×64 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 64×64 で4分割

```
2D game sprite sheet, top-down view, canvas size 256x64 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 64x64 pixels.
Draw thin visible guide lines at cell boundaries (x=64, x=128, x=192) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
All frames: pure white silhouette on fully transparent background, crisp flat vector shapes, no gradients, no shading.
Subject: Ring enemy for a top-down bullet-hell shooter.
Design: a concentric ring shape — thick outer ring with fully transparent hollow center. 8 thin radial slits dividing the ring into 8 equal segments like a dharma wheel. Bold and chunky ring, mandala-influenced radial symmetry.

Frame 1 (x 0-63px): ring at 0° rotation
Frame 2 (x 64-127px): ring pulsing slightly larger (expanded state)
Frame 3 (x 128-191px): ring at 22.5° rotation (slits offset)
Frame 4 (x 192-255px): ring pulsing slightly smaller (contracted state)

Centered within each cell. No background, no shadow, no gradient.
Style: minimal flat game sprite, geometric ring structure.
```

---

## 3-D. Anchor スプライトシート

**用途：** EnemyVisualConfigSO → DrawMeshInstanced テクスチャ
**出力サイズ：** 384×96 px（4列×1行）
**各コマサイズ：** 96×96 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 96×96 で4分割

```
2D game sprite sheet, top-down view, canvas size 384x96 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 96x96 pixels.
Draw thin visible guide lines at cell boundaries (x=96, x=192, x=288) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
All frames: pure white silhouette on fully transparent background, crisp flat vector shapes, no gradients, no shading.
Subject: Anchor enemy (boss-tier mob) for a top-down bullet-hell shooter.
Design: a single yin-yang fish silhouette — the shape of one of the two interlocking fish that form a taijitu symbol, isolated on its own. Large rounded head bulging at the top, transitioning into a body that gradually tapers and curves in a smooth S-arc toward a thin pointed tail at the bottom. Tall and vertically oriented, organic and elongated. Inside the rounded head, a single small filled circle represents the eye.

Frame 1 (x 0-95px): fish facing upward (0°)
Frame 2 (x 96-191px): fish rotated 90° clockwise
Frame 3 (x 192-287px): fish facing downward (180°)
Frame 4 (x 288-383px): fish rotated 270° clockwise

Centered within each cell with small padding. No background, no shadow, no gradient.
Style: minimal flat game sprite, yin-yang symbolic, heavier and more complex than other enemy types.
```

---

## 4. 弾丸スプライトシート（勾玉形状）

**用途：** BulletRenderer → DrawMeshInstanced テクスチャ
**出力サイズ：** 128×64 px（2列×1行）
**各コマサイズ：** 64×64 px（勾玉形状は32×32相当を64×64枠に配置）
**ファイル形式：** PNG with transparent background
**Unityスライス：** 64×64 で2分割

```
2D game sprite sheet, top-down view, canvas size 128x64 pixels, PNG with transparent background.
Grid layout: 2 columns x 1 row. Each cell is exactly 64x64 pixels.
Draw a thin visible guide line at the cell boundary (x=64) so the grid division is clearly visible.
Camera distance and scale must be completely identical across both frames.
Subject: magatama (Japanese comma-shaped jewel) bullet projectile for a top-down bullet-hell shooter.
Design: teardrop or comma shape — one end is a full rounded bulge, tapering to a thin curved tail that curls slightly. The actual magatama shape occupies approximately 32x32 within the 64x64 cell, centered with padding on all sides.

Frame 1 (leftmost, x 0-63px) — upward-pointing magatama:
The bulge (heavy end) faces downward, the thin curling tail points upward. This represents a bullet moving upward.

Frame 2 (rightmost, x 64-127px) — downward-pointing magatama:
The bulge (heavy end) faces upward, the thin curling tail points downward. This represents a bullet moving downward.

Each frame: pure white silhouette on fully transparent background. Smooth curved silhouette, no jagged edges. Crisp flat vector shape with clean alpha edge. Centered within each cell.
No background, no shadow, no gradient, no outline beyond shape edge, no labels.
Style: minimal flat game sprite, ancient symbolic shape adapted for game projectile.
Both frames pixel-aligned within their respective cells.
```

---

## 5-A. 白の守護者（WhiteGuardian）スプライトシート

**用途：** BossRenderer → SpriteRenderer（フルカラー）
**出力サイズ：** 1024×256 px（4列×1行）
**各コマサイズ：** 256×256 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 256×256 で4分割

```
2D game boss sprite sheet, top-down view, canvas size 1024x256 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 256x256 pixels.
Draw thin visible guide lines at cell boundaries (x=256, x=512, x=768) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
Subject: White Guardian boss character for a yin-yang themed bullet-hell shooter.
Design: a tall, elongated slime-like entity. The silhouette is amorphous and fluid — not humanoid. The body rises upward in a stretched teardrop or pillar shape, widest at the base and narrowing as it ascends, with an irregular undulating surface suggesting slow, ponderous motion. Think of living liquid restrained into a vertical column.
Body color: warm off-white (#F0EEE6) flat fill.
Outline color: deep dark navy (#1A1A2E), bold stroke weight approximately 4-5px.
Head region (topmost body portion): a large dark navy circle (#1A1A2E) floating visibly inside the body, representing the "eye" — the opposite polarity within.
Mouth/expression: open configuration — the lower front of the body has a wide open cavity suggesting the "ah" (Agyo) form.

Frame 1 (x 0-255px) — Idle:
Upright, slightly rounded, composed stance. Surface undulations minimal. Calm and watchful.

Frame 2 (x 256-511px) — Attack windup:
Body leans slightly forward. The eye circle brightens/enlarges slightly. Surface ripples intensify near the mouth cavity, suggesting energy gathering.

Frame 3 (x 512-767px) — Attack release:
Body stretches upward momentarily. The mouth cavity widens. Surface ripples radiate outward from the mouth region, suggesting bullets being expelled.

Frame 4 (x 768-1023px) — Damaged:
Body compresses slightly, surface becomes more jagged and irregular. The eye circle dims or shrinks. The undulating surface shows disruption — cracks or fragmentation in the smooth form.

Each frame: subject centered within cell with small padding.
No background, no shadow, no text, no labels.
Clean flat graphic style, symbolic and geometric, no gradients on body.
NEGATIVE: photorealistic, humanoid body, armor plating, human face, weapon, colored background.
```

---

## 5-B. 黒の守護者（BlackGuardian）スプライトシート

**用途：** BossRenderer → SpriteRenderer（フルカラー）
**出力サイズ：** 1024×256 px（4列×1行）
**各コマサイズ：** 256×256 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 256×256 で4分割

```
2D game boss sprite sheet, top-down view, canvas size 1024x256 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 256x256 pixels.
Draw thin visible guide lines at cell boundaries (x=256, x=512, x=768) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
Subject: Black Guardian boss character for a yin-yang themed bullet-hell shooter.
Design: identical silhouette structure to the White Guardian — a tall, elongated slime-like entity. Amorphous and fluid, not humanoid. Stretched teardrop or pillar shape, widest at the base, narrowing upward, with irregular undulating surface.
Body color: deep dark navy (#1A1A2E) flat fill.
Outline color: warm off-white (#F0EEE6), bold stroke weight approximately 4-5px.
Head region (topmost body portion): a large warm off-white circle (#F0EEE6) floating visibly inside the dark body, representing the "eye" — the opposite polarity within.
Mouth/expression: closed or compressed — the lower front of the body has a tight sealed cavity suggesting the "un" (Ungyo) form.

Frame 1 (x 0-255px) — Idle:
Upright, same proportions as White Guardian. Surface undulations minimal. Stern and sealed.

Frame 2 (x 256-511px) — Attack windup:
Body leans slightly forward. The eye circle brightens/enlarges slightly. Surface ripples intensify near the sealed mouth, suggesting pressure building.

Frame 3 (x 512-767px) — Attack release:
Body stretches upward. The sealed mouth cracks open momentarily. Surface ripples radiate outward, suggesting burst fire.

Frame 4 (x 768-1023px) — Damaged:
Body compresses, surface becomes jagged and irregular. The eye circle dims or shrinks. Surface disruption visible.

Each frame: subject centered within cell with small padding.
No background, no shadow, no text, no labels.
Clean flat graphic style, symbolic and geometric, no gradients on body.
NEGATIVE: photorealistic, humanoid body, armor plating, human face, weapon, colored background.
```

---

## 6. ボス第2形態（勾玉合体：Magatama）スプライトシート

**用途：** BossRenderer → SpriteRenderer（フルカラー）
**出力サイズ：** 2048×512 px（4列×1行）
**各コマサイズ：** 512×512 px
**ファイル形式：** PNG with transparent background
**Unityスライス：** 512×512 で4分割

```
2D game boss sprite sheet, top-down view, canvas size 2048x512 pixels, PNG with transparent background.
Grid layout: 4 columns x 1 row. Each cell is exactly 512x512 pixels.
Draw thin visible guide lines at cell boundaries (x=512, x=1024, x=1536) so the grid division is clearly visible.
Camera distance and scale must be completely identical across all 4 frames.
Subject: merged final boss entity for a yin-yang themed bullet-hell shooter.
Design: an enormous magatama (comma-shaped jewel) form filling most of each cell. Smooth magatama/comma silhouette: rounded large bulge on one end, tapering curved tail on the other. Body surface covered in a swirling yin-yang pattern: two interlocked flowing regions, one warm off-white (#F0EEE6) and one deep dark navy (#1A1A2E), spiraling into each other. At the center, two small circles remain visible — one white and one dark — representing the absorbed "eyes" of the two guardian bosses. Outline: dual-tone — off-white regions bordered by navy, navy regions bordered by off-white.

Frame 1 (x 0-511px) — Idle:
Swirl pattern in a calm, steady configuration. Heavy bulge oriented lower-left, tail curling upper-right. Eye circles at roughly 10 o'clock and 4 o'clock. Smooth and continuous flow.

Frame 2 (x 512-1023px) — Spinning:
Internal swirl pattern intensified to suggest rapid spinning. Tighter spiral curves, more dynamic directional lines. Eye circles slightly stretched along the rotation arc. Outer silhouette identical to Frame 1.

Frame 3 (x 1024-1535px) — Attack burst:
Swirl pattern explosively expanding outward from the center. The eye circles glow brighter. Surface pattern suggests energy radiating outward in all directions. Outer silhouette slightly bulging at the edges.

Frame 4 (x 1536-2047px) — Damaged:
Swirl pattern fragmenting and destabilizing. The two color regions begin to separate rather than flow together. Eye circles drift apart. Cracks or discontinuities appear in the swirl boundaries. The cosmic fusion is breaking down.

Each frame: entity centered within cell with small padding.
No background, no text, no labels.
Clean flat graphic style with bold graphic pattern work. No photorealism.
NEGATIVE: photorealistic, human face, text, watermark, colored rectangular background, lens flare.
```

---

## 7. 背景スプライト

**用途：** ステージ背景。ポストプロセス不使用のためスプライトまたは背景描画
**出力サイズ：** 960×540 px（ゲーム解像度に合わせる）
**ファイル形式：** PNG（透過不要）
**注意：** タイリングを想定する場合は継ぎ目のないシームレステクスチャで指定すること

### 7-A. White極性ステージ背景

```
2D game background for a top-down bullet-hell shooter, 960x540 pixels.
Dark cosmic background representing the "white polarity" world.
Base color: deep dark navy (#1A1A2E) filling the entire canvas.
Subtle visual texture: flowing wave-like patterns in slightly lighter navy or very dim off-white, suggesting yin-yang energy currents moving through the space.
The wave patterns are organic, curved, and reminiscent of the flowing S-curves in a taijitu symbol — but abstract and ambient, not literal yin-yang symbols.
Overall impression: vast, ordered, the darkness of a perfectly balanced cosmos.
No characters, no enemies, no UI elements.
Style: flat minimal game background, atmospheric dark, subtle surface patterning. Low contrast between pattern and base to avoid distracting from gameplay.
Seamless tileable texture structure preferred.
```

### 7-B. Black極性ステージ背景

```
2D game background for a top-down bullet-hell shooter, 960x540 pixels.
Light cosmic background representing the "black polarity" world.
Base color: warm off-white (#F0EEE6) filling the entire canvas.
Subtle visual texture: flowing wave-like patterns in slightly darker off-white or very dim navy, suggesting yin-yang energy currents converging inward.
The wave patterns are organic, curved, reminiscent of taijitu S-curves — abstract and ambient.
Overall impression: vast, serene, the luminosity of perfect order.
No characters, no enemies, no UI elements.
Style: flat minimal game background, atmospheric bright, subtle surface patterning. Low contrast between pattern and base.
Seamless tileable texture structure preferred.
```

### 7-C. 境界ステージ背景（終盤：秩序の崩壊）

```
2D game background for a top-down bullet-hell shooter, 960x540 pixels.
A background representing the destabilization of yin-yang cosmic order.
The canvas is divided but not cleanly: the left region trends toward dark navy (#1A1A2E) and the right toward warm off-white (#F0EEE6), but the boundary between them is NOT a sharp line.
Instead, the boundary zone (roughly the central third of the canvas) shows the two colors bleeding and eroding into each other — dark tendrils reaching into the light, bright wisps dissolving into the dark.
The erosion pattern follows organic flowing curves, suggesting the S-curve boundary of a yin-yang symbol destabilizing and breaking apart.
Overall impression: the cosmic order is failing. The separation that maintained stability is dissolving.
No characters, no enemies, no UI elements.
Style: flat graphic, dramatic, the flowing color intrusion is intentional and bold without being gradient-blurry. More like ink bleeding into paper or oil on water — patterned dissolution, not a soft blend.
```

---

## バージョン管理

| バージョン | 日付 | 変更内容 |
|-----------|------|---------|
| v1.0 | 2026-03-22 | 初版作成。全7カテゴリのプロンプトを確立 |
| v1.1 | 2026-03-22 | ボス第1形態を人型甲冑からスライム型不定形に変更。全スプライトをスプライトシート形式に変更。ボス第2形態に被弾フェーズシートを追加 |
| v1.2 | 2026-03-22 | Anchor敵プロンプト（3-AのCell 4）を「楕円＋S字分割」から「陰陽魚の片身シルエット」に変更。弾丸・プレイヤー・他モブとの混同を回避 |
| v1.3 | 2026-03-22 | スプライト2〜6を指定スプライトシート仕様に全面改訂。モブ敵を256×256（2列×2行）・各コマ128×128に変更。弾丸を128×64（2列×1行）・各コマ64×64・上下2コマに変更。ボス守護者を512×256（2列×1行）・White/Blackを1シートに統合（3フレーム→2コマ）。ボス勾玉を1024×512（2列×1行）・通常/渦強調の2コマに統合（被弾フェーズシート削除）。各プロンプトに薄いガイド線・スケール統一・トップダウン視点を明示 ※採用されなかった変更。v1.2の構成が現在の正式仕様として継続 |
| v1.4 | 2026-03-22 | 5-A・5-Bの口の位置指定を「front face」から「lower front of the body」に修正（character-setting.md「正面下部」との整合）。バージョン履歴v1.3に未採用注記を追加 |

## 次回改訂時の確認事項

- nanobananaで実際に生成したあと、シルエットの可読性を各解像度でチェック
- ボス守護者2体のシルエット一致度を確認（異なりすぎたら片方を再生成）
- スプライトシートのグリッド境界がUnity Sprite Editorで正しくスライスできるか確認
- 弾丸（64×64セル内32×32相当）がゲーム内で視認できるサイズかPlayモードで確認
- Anchor敵のシルエットがプレイヤー（太極図）と混同されないか確認
- ボス守護者のスライム型シルエットが既存IPの類似キャラと重ならないか確認
- モブ敵シートのラベルテキストがUnityスライス作業の邪魔にならないか確認
