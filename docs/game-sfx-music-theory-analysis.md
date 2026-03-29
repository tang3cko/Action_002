# ゲームSFXの音楽理論的分析 調査レポート

## 1. ピッチベンド / グリッサンド / ポルタメント

### マリオのジャンプ音の上行ピッチスイープ

マリオのジャンプ音はNESの**三角波チャネル**（矩形波ではない）を使用し、短時間で低音域から高音域へ急速にピッチをスイープさせている。NESの三角波チャネルは4ビット解像度（16段階）で27.3Hz〜55.9kHzの範囲を出力可能。

音楽理論的には、この上行グリッサンドは以下のように説明できる：

- **上行音形 = 上昇・飛翔のメタファー**: 音が上がる方向は、物理的に「上に行く」動作と直感的に対応する。これは音楽における「音画」(tone painting/word painting) の原理そのもの
- **加速感の知覚**: ピッチが急速に上昇すると、聴覚的に「加速」「離陸」の印象を与える。ジャンプという動作のフィードバックとして理にかなっている
- **三角波の選択理由**: 三角波はサイン波に近い柔らかい音色で、矩形波より耳に優しい。短時間のスイープでも不快感を与えにくく、繰り返し聞いても疲れにくい

### ピッチスイープの方向と感情効果

| 方向 | 感情効果 | ゲームでの用途例 |
|------|----------|-----------------|
| 上行（ascending） | 上昇、成功、期待、緊張の高まり | ジャンプ、パワーアップ、コンボ連鎖 |
| 下行（descending） | 下降、失敗、弛緩、消失 | 落下、ミス、エネルギー消耗 |
| 上行→下行 | 完結感、放物線的運動 | 投擲、バウンド |
| 連続上行（コンボ） | エスカレーション、達成感の累積 | コンボカウンター、連続ヒット |

コンボにおいて連鎖アクションの効果音のピッチを段階的に上げることで、連鎖の長さを強調し、プレイヤーの満足感を増幅させる効果がある。ピッチステップとしては**半音（セミトーン）が自然なステップ幅**とされる。

### シェパードトーン

永遠に上昇（または下降）し続けるように聞こえる聴覚的イリュージョン。ボリュームオートメーションによって作り出される。ゲームでは「常に加速・エスカレーションしている」感覚を演出するのに使用される。


## 2. 音程関係（インターバル）

### ゼルダの謎解き成功音の音程分析

ゼルダシリーズの「謎解き成功」ジングルは8音で構成される：

- **前半4音（G, F#, D#, A）**: D#半全ディミニッシュスケール（D# Half/Whole Diminished Scale）に属する。D#ディミニッシュトライアド（D#, F#, A）にGのメロディノートを加えた構成
- **後半**: E augmented triad（E, G#, B#/C）

**音楽理論的に「なぜ気持ちいいか」の説明**:
- ディミニッシュ（不安定・緊張）→ オーグメンテッド（拡張・開放感）という進行は、**「謎」→「解決」**のナラティブを音で表現している
- ディミニッシュの不安定な響きが短いフレーズ内で解決に向かうことで、プレイヤーに「困難を乗り越えた」感覚を与える
- トライトーン（増4度/減5度）を含むディミニッシュコードは中世では「悪魔の音程」と呼ばれるほど不安定だが、それが解決されることで強い満足感が生まれる

### 協和音程 vs 不協和音程のゲームフィードバック

**協和音程**（Consonant intervals）:
- ユニゾン、オクターブ、完全5度、完全4度、長3度、短3度
- 周波数比が単純（例: 完全5度 = 3:2、オクターブ = 2:1）
- ニューロンの同期発火を促進し、「心地よい」と知覚される
- **用途**: 成功音、アイテム取得音、回復音

**不協和音程**（Dissonant intervals）:
- 短2度、長2度、短7度、長7度、トライトーン
- 周波数比が複雑で、聴覚的に「ぶつかる」
- 不安定感・緊張感を生む
- **用途**: ダメージ音、警告音、失敗音、敵出現音

### 成功音/失敗音/警告音の典型パターン

| 種類 | 典型的音程 | 理由 |
|------|-----------|------|
| 成功音 | 長3度上行、完全5度上行、オクターブ上行 | 協和音程の上行 = ポジティブ + 上昇感 |
| 失敗音 | 短2度下行、トライトーン | 不協和 + 下降 = ネガティブ |
| 警告音 | 短2度の反復、トライトーン | 不安定な繰り返し = 注意喚起 |
| 回復/癒し | 長3度、完全5度の穏やかなアルペジオ | 協和音程の安定感 |


## 3. 倍音構成（ハーモニクス / ティンバー）

### 各波形と感情喚起

| 波形 | 倍音構成 | 音色の印象 | 感情効果 | ゲームでの典型用途 |
|------|----------|-----------|----------|-------------------|
| サイン波 | 基音のみ（倍音なし） | 純粋、クリア、滑らか | 瞑想的、穏やか、治療的 | UI確認音、穏やかな通知 |
| 三角波 | 奇数次倍音（急速減衰） | サイン波に近いが少し明るい、フルート的 | 柔らかい、優しい | ジャンプ音（マリオ）、穏やかなSFX |
| 矩形波 | 奇数次倍音（豊富） | ブザー的、中空、ロボット的 | 荒々しい、エネルギッシュ、レトロ | レトロゲーム全般、アラーム、攻撃音 |
| 鋸歯状波 | 全倍音（奇数+偶数） | 最も明るく豊か | 攻撃的、力強い、目立つ | リード音、派手なエフェクト |

**学術的根拠**: 高次倍音にエネルギーが多く、アタックが速い音色は「怒り」や「喜び」と関連し、低次倍音にエネルギーが集中しアタックが遅い音色は「悲しみ」と関連する（PMC研究）。

### レゾナンスフィルタースイープ（「ちゅいん」「ビュイン」の正体）

フィルタースイープとは、ローパスフィルタまたはハイパスフィルタのカットオフ周波数を時間経過とともに変化させるテクニック。

- **レゾナンス**はカットオフ周波数直前の帯域をブーストし、鋭いピークを作る
- フィルタ周波数がスイープすると、オシレーターの周波数と**音楽的な音程関係**（オクターブ、完全5度等）に近づいた時に音量と倍音が急増する
- この「共鳴ピーク」が移動することで、「ちゅいん」「ビュイン」という特徴的な音色変化が生じる
- **心理音響的効果**: フィルタースイープは音のスペクトルの「形」を劇的に変えるため、注意を強く引きつける。脳は「スペクトルの急激な変化」を新しい音源として検知する傾向がある

### 倍音が多い音 vs 少ない音

| 特性 | 倍音が多い音（矩形波、鋸歯状波） | 倍音が少ない音（サイン波、三角波） |
|------|--------------------------------|----------------------------------|
| 知覚される明るさ | 明るい（bright） | 暗い/柔らかい（dark/soft） |
| 注目度 | 高い（目立つ） | 低い（溶け込む） |
| 攻撃性 | 高い | 低い |
| 聴取疲労 | 起きやすい | 起きにくい |
| 適した用途 | メイン攻撃音、警告、UI主要操作 | 環境音、補助的通知、BGM内SFX |


## 4. エンベロープ（ADSR）

### アタックの速さとゲームフィール

- **短いアタック（0-10ms）**: 音が瞬時に最大音量に達する。**打撃感、切れ味、即時性**を演出。ヒット音、パンチ音、銃声に必須
- **中程度のアタック（10-50ms）**: やや柔らかい立ち上がり。スラッシュ（斬撃）、スワイプ音に適する
- **長いアタック（50ms+）**: ゆっくり音が膨らむ。パッド的。魔法のチャージ、シールド展開等

**ゲームフィール直結の原理**: プレイヤーの入力からフィードバックまでの遅延が短いほど「レスポンシブ」と感じる。アタックタイムが長いとフィードバックの「遅延」として知覚され、操作の軽快さが損なわれる。

### ディケイ/リリースと「重さ」「軽さ」

| パラメータ | 短い場合 | 長い場合 |
|-----------|---------|---------|
| ディケイ | 軽い、シャープ、パーカッシブ | 重い、響く、持続的 |
| リリース | ドライ、タイト、即座に消える | 残響感、空間の広さ、重量感 |

### パーカッシブ（打撃感）のレシピ

**短アタック + 急速ディケイ + 低サステイン = パーカッシブ**

これが打撃感を生む理論的根拠：
1. 現実世界の打撃音（手を叩く、物がぶつかる）は全てこのエンベロープ
2. 人間の聴覚はこのパターンを「物理的衝突」として学習している
3. サステインがないことで「エネルギーが瞬時に放出された」と知覚される
4. スマブラのヒット音はこの原理に加え、**複数レイヤー**（低音の打撃基音 + 高音のアタックトランジェント + ノイズバースト）を重ねて重量感と明瞭さを両立している


## 5. スケール / モード / キー

### ペンタトニックスケールがゲームSFXに多用される理由

ペンタトニックスケール（5音音階: 例 C D E G A）がゲームSFXで多用される理由：

1. **半音を含まない**: 全ての音の間隔が全音または短3度のため、どの組み合わせでも不協和にならない
2. **文化横断的な普遍性**: レナード・バーンスタインが指摘したように、スコットランド、中国、アフリカ、アメリカ先住民文化等、世界中で見られるスケール。あらゆる文化圏のプレイヤーに直感的に受け入れられる
3. **「何を弾いても良い音になる」性質**: 報酬音やポジティブフィードバックに最適。「5つの音をどの順番で弾いても"うまくいってるよ"というメッセージを普遍的に伝えるモチーフになる」
4. **技術的制約との相性**: チップチューン時代の限られた発音数でも効果的にメロディを構成できた

### BGMのキーとSFXの関係

- SFXをBGMのキーに合わせると、全体の「和声的一貫性」が保たれ、没入感が増す
- 逆に意図的にキー外の音を使うことで、「異質さ」「警告」を表現できる
- 実践的には、BGMのキーが動的に変化するゲームでは完全にマッチさせるのは困難
- **推奨アプローチ**: SFXの主要ピッチをペンタトニックスケールの音にすると、多くのキーと衝突しにくい

### 全音階 vs 半音階のSFXでの使い分け

| スケール | 特徴 | SFXでの効果 |
|---------|------|------------|
| 全音階（ダイアトニック） | 安定、調性感あり | 報酬音、メニュー操作音 |
| 半音階（クロマティック） | 不安定、混沌、不気味 | 緊張の高まり、異常事態 |
| ホールトーンスケール | 夢幻的、方向性が曖昧 | 異世界感、ワープ、ミステリー |
| ディミニッシュスケール | 不安定だが対称的 | ボス出現、危機、謎解き |


## 6. 心理音響学（Psychoacoustics）

### 高周波音と注意喚起

- 人間の耳は**2,500〜5,000Hz**の帯域に最も敏感（耳道の共鳴と中耳の伝達関数による）
- 高い基本周波数の音ほど**反応時間が短くなり、知覚される緊急性が高まる**（学術研究で実証）
- 火災警報の研究: 3100Hzの警報音は高い覚醒度と緊急性を誘発
- **進化的理由**: 高周波の鳴き声や叫び声は、捕食者の接近や仲間の危険信号として機能してきた。人間の聴覚系はこれらの周波数帯域に敏感に反応するよう進化した

### 低周波音と「重さ」「脅威」

- 低周波音（100Hz以下）は大きな物体や強い力と関連づけられる（大きな物体ほど低い共振周波数を持つ物理法則）
- 進化的に、低い唸り声は大型捕食者を連想させ、脅威として知覚される
- ゲームでは: ボスの足音、大型敵の出現、爆発の衝撃波に低周波成分が不可欠
- **学習された嫌悪**: 低周波ノイズに対する嫌悪は進化的プログラミングに加え、「脅威との関連付け」で強化される。ゲームのコンテキストでも同様

### マスキング効果

- BGMの特定帯域がSFXと重なると、SFXが聞こえにくくなる
- **対策**: ゲーム音楽は**1〜5kHzの中域をコントロール**する必要がある。この帯域はダイアログ、SFX、アンビエンスと競合する
- SFXが確実に聞こえるためには、BGMとは異なる周波数帯域に主要成分を配置するか、SFX再生時にBGMにダッキング（音量自動減衰）を適用する

### 等ラウドネス曲線（Fletcher-Munson曲線）

- 同じ音圧レベルでも、周波数によって知覚される音量が異なる
- 例: 40フォンにおいて、100Hzの音を3500Hzと同じ音量に聞こえさせるには約63dB SPL必要（3500Hzは34dBで済む）
- **ゲームSFX設計への示唆**:
  - 低音のSFX（爆発、足音）は物理的に大きな音圧が必要
  - 高音域のSFX（コイン取得、UI音）は比較的小さな音圧でも十分聞こえる
  - 小音量でプレイするプレイヤーほど低音が聞こえにくくなる（曲線の偏差が大きくなる）
  - 理想的なミキシング音量は80〜85dB SPL（曲線がフラットに近づく）


## 7. 具体的なゲームの音の音楽理論分析

### マリオシリーズ

**ジャンプ音**:
- 三角波による上行ピッチスイープ
- 短いエンベロープ（急速アタック→急速ディケイ）
- 繰り返し聞いても疲れない三角波の柔らかさが設計上重要

**コイン音**:
- **BとEの2音**で構成。音程は**完全4度**上行
- Cメジャー（ゲーム全体の調性）における長7度と長3度
- 装飾音（appoggiatura）として機能：Bが装飾音でEが本音。この「前打音→解決」が「ブリン！」という輝きを生む
- 高速のオクターブまたは4度上行を短時間で演奏することで再現可能

**1UP音**:
- **6音のファンファーレ: E, G, E(高), C, D, G**
- 近藤浩治作曲
- 上行→下行→上行のメロディックな輪郭がミニファンファーレとして機能
- Cメジャーの調性内で構成され、ゲーム全体の調性と一貫性がある

**ゲーム全体の調性**: Cメジャー。「赤ちゃんでも好む明るい調性」とされ、マリオの楽観的な世界観と合致。

### ゼルダシリーズ

**謎解き成功音**（前述の詳細分析に加えて）:
- 8-bit Music Theoryの分析で「シンプルだが強力」と評価
- 40年近く使われ続けている普遍性
- **ディミニッシュ→オーグメンテッド**の進行が「困惑→発見」の心理的旅路を音で表現

**アイテム取得音 / 秘密発見音**:
- 上行アルペジオ形式
- 協和音程を基盤としたポジティブなフィードバック

### ポケモンの技エフェクト音

- ゲームボーイの限られたサウンドチャネル（矩形波×2、波形メモリ×1、ノイズ×1）で、技ごとに異なるキャラクターを実現
- 攻撃技: ノイズチャネル（ホワイトノイズ）+ ピッチスイープによる衝撃感
- 特殊技: 矩形波のアルペジオやトリルで神秘的・エネルギー的な印象
- ポケモンの鳴き声: 一部にスカイウォーカーサウンドの恐竜鳴き声素材が使用されている（リザードン、ギャラドス等にカルノタウルスの鳴き声）

### スマブラのヒット音

- 桜井政博がシリーズで最も完成させるのが難しかった音としてリュウ/ケンのヒット音を挙げている
- アーケード筐体から出ているような音を目指し、長期間のトライ&エラーを経た
- **レイヤリング技法**: メタリックスマック音等、複数のピッチバリエーションを重ね合わせ
- アシストトロフィー使用時等で複数の打撃音が同時に鳴り、マスキングが発生するケースも確認されている

### 斑鳩の極性切替音・弾吸収音

- パルス的エレクトロニックサウンドトラックが画面上のアクションと完璧に同期
- BGMがレベルの各局面に適応的に変化し、ボス戦ではメランコリックな緊張感を演出
- 「Energy Max!」（エネルギーゲージ満タン時）のボイスサンプル、ボス出現時のWarningサイレン等、聴覚的フィードバックがゲームプレイメカニクスと直結
- **極性切替の音楽理論的考察**: 白/黒の極性切替は、音楽的にもメジャー/マイナー、協和/不協和の切替に対応させることで、視覚的極性と聴覚的極性の一致を実現できる設計

### Geometry Warsの敵撃破音

- 100%シンセサイズドSFXのアプローチ（録音素材を一切使わない）
- レトロシューティングの伝統に則り、8ビット的爆発音を基調とする
- **8ビット爆発音の作り方**: ノイズオシレーターを持つシンセで、エンベロープのディケイを短くし、ビットクラッシャーを適用
- ノイズバースト + 急速ディケイ + ピッチダウンスイープの組み合わせで「破裂→消散」を表現


## まとめ: ゲームSFXを「気持ちよく」する音楽理論的原則

1. **ピッチの方向 = 感情の方向**: 上行 = ポジティブ、下行 = ネガティブ。これは言語・文化を超えた普遍的知覚
2. **協和音程 = 報酬、不協和音程 = 警告**: 周波数比の単純さがニューロンの同期を促進し、快/不快を決定する
3. **ペンタトニックは最強の安全策**: 半音を含まないため、どの音の組み合わせでも不快にならない
4. **波形の倍音構成 = キャラクター**: サイン波（穏やか）→ 三角波（柔らかい）→ 矩形波（荒い）→ 鋸歯状波（攻撃的）のスペクトル
5. **アタック速度 = レスポンス感**: 短いアタックは「反応が良い」と感じさせ、ゲームフィールに直結
6. **等ラウドネス曲線を考慮した帯域配置**: 2.5-5kHzが最も敏感な帯域であり、重要SFXはここに成分を持つべき
7. **マスキング回避**: SFXとBGMの周波数帯域を分離するか、ダッキングで共存させる
8. **フィルタースイープ = 注意の誘導**: スペクトルの急激な変化は脳の「新しい音源検知」を発火させる


## Sources

- [Analysis Video: Psychology of Zelda Puzzle Solution Sound](https://www.zeldadungeon.net/analysis-video-explores-the-psychology-of-the-zelda-series-puzzle-solution-sound-effect/)
- [Zelda Secret Sound - Steel Lemon](https://steellemon.com/2020/11/19/how-to-play-the-legend-of-zelda-secret-sound/)
- [Psychoacoustics in Gaming - Audio Exotica](https://audioexotica.com/psychoacoustics-in-gaming/)
- [The Power of Pitch Shifting - Gamedeveloper.com](https://www.gamedeveloper.com/audio/the-power-of-pitch-shifting)
- [Basic Music Theory For Sound Designers - Game Audio Learning](https://www.gameaudiolearning.com/knowledgebase/basic-music-theory-for-sound-designers)
- [Mario Coin Sound Effect - David Dumais Audio](https://www.daviddumaisaudio.com/how-to-make-the-super-mario-coin-sound-effect/)
- [Super Mario Melodies - Los Doggies](https://www.losdoggies.com/archives/1302)
- [1-Up Sound Effect - Super Mario Wiki](https://www.mariowiki.com/1-Up_(sound_effect))
- [Super Mario Sound Effects Reproduction - Pure Data](https://forum.pdpatchrepo.info/topic/10396/super-mario-sound-effects-reproduction)
- [Timbre Affects Perception of Emotion in Music - PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC2683716/)
- [Waveforms: Sine, Square, Triangle, Saw - Perfect Circuit](https://www.perfectcircuit.com/signal/difference-between-waveforms)
- [Sinewaves to Physiologically-Adaptive Soundscapes - Springer](https://link.springer.com/chapter/10.1007/978-3-319-41316-7_12)
- [ADSR Envelopes Explained - MasterClass](https://www.masterclass.com/articles/adsr-envelope-explained)
- [Understanding ADSR in Sound Design - Point Blank](https://www.pointblankmusicschool.com/blog/understanding-the-role-of-adsr-in-sound-design/)
- [Psychoacoustics: Psychology of Sound - LA Recording School](https://www.larecordingschool.com/psychoacoustics-the-psychology-of-sound/)
- [Perception of Musical Consonance - PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC2607353/)
- [Equal Loudness Contour - Wikipedia](https://en.wikipedia.org/wiki/Equal-loudness_contour)
- [Fletcher Munson Curve - iZotope](https://www.izotope.com/en/learn/what-is-fletcher-munson-curve-equal-loudness-curves)
- [Composing Music For Video Games: Key & Tempo - Gamedeveloper.com](https://www.gamedeveloper.com/audio/composing-music-for-video-games---key-tempo)
- [Modular Smoothness in Video Game Music - MTO](https://www.mtosmt.org/issues/mto.19.25.3/mto.19.25.3.medina.gray.html)
- [Filter Sweeps - Perfect Circuit](https://www.perfectcircuit.com/signal/filter-sweeps)
- [Filter Sweeps 101 - Unison Audio](https://unison.audio/filter-sweeps/)
- [Sound Design Filtering - Get That Pro Sound](https://getthatprosound.com/sound-design-techniques-tools-series-part-9-filtering/)
- [Whole Tone Scales Theory - MusePrep](https://museprep.com/whole-tone-scales/)
- [Pentatonic Scale - MasterClass](https://www.masterclass.com/articles/what-is-the-pentatonic-scale-learn-music-theory)
- [Psychology of Music and Sounds in Games - Eon Music](https://www.eonmusic.co.uk/features/the-psychology-of-music-and-sounds-in-games)
- [Psychoacoustic Principles for Music Producers - Mystic Alankar](https://mysticalankar.com/blogs/blog/8-psychoacoustic-principles-for-musicians-and-sound-designers)
- [Fire Alarm Psychoacoustic Recognition - PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC7827080/)
- [Auditory Warning Design - PMC](https://ncbi.nlm.nih.gov/pmc/articles/PMC4347839)
- [Melee Sound Effects Developer Roundtable - Source Gaming](https://sourcegaming.info/2016/05/19/soundeffects/)
- [GDC 2023: Chaos Theory in Game Music - Winifred Phillips](https://winifredphillips.wpcomstaging.com/2023/08/14/quartal-chords-and-chromatics-the-game-music-of-jurassic-world-primal-ops-gdc-2023/)
- [100% Synthesized SFX - Designing Sound](https://designingsound.org/2014/10/02/100-synthesized-sfx-for-stylized-realism-in-games/)
- [Pokemon Black Sound Design Analysis](https://whimsicallytheoretical.com/2017/01/03/pokemon-black-sound-design-music-an-in-depth-analysis/)
- [NES Sound Channels - FCEUX](https://fceux.com/web/help/NESSound.html)
- [ゲーム効果音制作 - JBG音楽院](https://jbg-ongakuin.com/staff-blog/20250628/)
- [効果音を作ろう - 東京科学大学traP](https://trap.jp/post/22/)
