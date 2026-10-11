# CVTI — Title screen + Intro (Castlevania NES) — bản kế hoạch của session planner

> Soạn 11/10/2026 bởi session planner, cập nhật cùng ngày sau khi người dùng tự tải tài nguyên về máy.
> Mọi routine `cvti-NN` đọc file này ĐẦU phiên (cùng `CVTI_MEMORY.md`).
> Số liệu ở đây đo lúc planner chạy — **đo lại trước khi dựa vào** nếu routine cần độ chính xác cao hơn.

## 1. Nguồn

| Loại | Đường dẫn |
|---|---|
| Video gốc (chỉ đọc) | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Title_Intro.mp4` — 18.17 s, 524×476, 30 fps (capture thực tế ~7.5 fps ⇒ sai số mốc thời gian ±0.13 s), có audio AAC (rất nhỏ, ~-52 dB) |
| Khung hình tham chiếu đã trích | `ClaudeNotes/cvti/ref/` (9 PNG + `brightness_series.tsv` = độ sáng từng frame: toàn khung / vùng chữ PUSH START KEY) |
| Trang sprite | https://www.spriters-resource.com/nes/cv/ |
| Trang sound | https://sounds.spriters-resource.com/nes/cv/ |
| Code/sprite CŨ đã xoá (chỉ tham khảo) | commit `2bfd90b` "*delete old code" — `git show 2bfd90b^:<path>`; có sprite Simon `Assets/Data/Resources/Character/Simond/...`, `AnimationFBF.cs`, `ConfigAnimation.cs`, `InputController.cs`, `InputManager.cs`. Code cũ import sprite **Bilinear + nén + PPU 100** — KHÔNG chép thiết lập đó. |

Hai trang resource chặn truy cập tự động bằng Cloudflare (curl/WebFetch → 403; Browser pane kẹt "Performing security verification"). Claude **không được** tự vượt bước xác minh ⇒ tài nguyên còn thiếu thì nhờ người dùng tự tải.

Video: 0.00–0.93 s là UI của trình giả lập web (nút "START GAME", màn xám) và thanh điều khiển dưới đáy (y > ~385 px) — **không thuộc game, không dựng lại**.

## 2. Tài nguyên người dùng ĐÃ TẢI TAY (11/10/2026) — ở `C:\Users\kvtuy\Downloads\`

Routine 01 chép nguyên byte vào `Art_Source/` và kiểm sha256.

| File | Kích thước | sha256 | Dùng cho |
|---|---|---|---|
| `NES - Castlevania - Miscellaneous - Title Screen.png` | 731×519 | `84b9ecbf46f90236840a1c6c121e5794322cc95df752f02c487f9e2e2038b2ba` | Màn Title |
| `NES - Castlevania - Miscellaneous - Cutscenes & Credits.png` | 1039×1128 | `542d55e565372dca8106029546a484dd6b5beebd8746aae824f83db4b0edf008` | Nền Intro, dơi nhỏ, mây |
| `NES - Castlevania - Playable Characters - Simon Belmont.png` | 799×529 | `8151622015ab8d322b0ded1f29b63ee07faa538bc773008e28b098b2c1cdf7e4` | Simon (đi, quay lưng, đứng) |
| `393996.zip` (đã giải nén ra `393996\`) | 150 049 B · 37 WAV | `86fb3d3141ea2adfda17d21c50f142a04feb96c10a4fb00339b7c71aa6d17b55` | Pack SFX của trang sound: `01.wav`…`37.wav`, PCM 8-bit mono 44.1 kHz, dài 0.10–2.40 s, **không có tên** |

Cả 3 sheet ghi credit "Sprites ripped by Mister Mike. Give credit if used."

### 2a. Bản đồ sheet (toạ độ pixel TRONG SHEET: x, y, w, h — planner đo bằng ffmpeg rawvideo)

**Title Screen** — nền sheet xanh lá `(0,128,0)`; nội dung màn hình nền **đen thật** ⇒ không key màu đen.
- **Panel A = màn Title đầy đủ (bản US)**: `(1,1,256,240)`. Trong panel (= toạ độ NES 0..255 × 0..239):
  - thanh xám `(188,188,188)` y 0–7 và y 216–223; dải ô vuông y 16–23 và y 200–207; băng logo đỏ y 58–102; y 224–239 đen.
  - **ô vàng ô-liu `(128,128,0)` 72×56 tại NES (184,104)** = chỗ đặt khung animation tháp + dơi.
  - **thanh chấm tím/xanh `(128,0,128)`/`(0,128,128)` 112×8 tại NES (72,136)** = chỗ đặt `PUSH START KEY`.
  - chữ copyright 4 dòng, hàng y 160 / 168 / 176 / 184 (glyph 8×8 theo lưới, nét chữ màu `(252,252,252)`).
- Panel B `(258,1,256,240)`: như A nhưng logo thiếu chữ C (C-dao găm là sprite riêng `(519,1,34,121)`, nền teal) — chỉ tham khảo, dựng bằng panel A.
- `PUSH START KEY` 112×8 tại `(558,1)`; bản đen hoàn toàn `(558,10)` (trạng thái tắt khi nháy).
- 8 sprite dơi lẻ (nền đen) hàng y 127: `(528,127,16,16)`, `(545,127,16,16)`, `(562,127,16,16)`, `(579,127,16,16)`, `(596,127,16,16)`, `(613,127,24,32)`, `(638,127,32,32)`, `(671,127,32,32)`.
- **"Bat Animation": 40 khung 72×56 (tháp + dơi đã ghép sẵn, nền đen)** tại `x = 1 + 73·c`, `y = 256 + 57·r` (c = 0..9, r = 0..3), đọc theo hàng. Khung 1 chỉ có tháp → dơi nhỏ xíu từ ngọn tháp → bay sang phải, to dần → sà xuống trước tháp → bay lên trên-trái → các khung cuối vỗ cánh lơ lửng. Ghép đè đúng vào ô ô-liu (184,104).
- Hộp credit `(506,486,224,32)` — loại trừ khi cắt.

**Cutscenes & Credits** — nền sheet `(0,128,0)`.
- **Nền Intro `(1,1,256,192)`**: trời, lâu đài, trăng, tường, cổng, đường; KHÔNG có mây và dơi (là sprite).
- Sprite trên ô nền **xám `(116,116,116)`** — **key màu xám, KHÔNG key đen (sprite dùng đen thật)**: dơi nhỏ khung 1 `(1,194,8,16)`, khung 2 `(10,194,8,16)`, mây `(19,194,32,16)`.
- Phần còn lại (ending, credits, bản đồ lâu đài) không thuộc cụm này.

**Simon Belmont** — nền sheet `(0,128,0)`, mỗi sprite trên ô xám `(116,116,116)` 16×32 (key màu xám); nhãn chữ trắng phía trên, né khi cắt.
- Idle `(1,21)`, Walk `(29,21)`, `(46,21)`, `(63,21)`, **Intro/quay lưng `(362,21)`** (tất cả 16×32).

### 2b. CÒN THIẾU — cần người dùng tải thêm (trang bị Cloudflare chặn)
1. **HUD + font**: chữ số 0–9, chữ cái, `-`, thanh máu PLAYER/ENEMY, khung vũ khí phụ, icon tim. Sheet Title chỉ có ký tự của dòng copyright + `PUSH START KEY` (thiếu `G`, `0`, `2`–`6`…). Chặn routine **06**.
2. **Stage 1** (màn đầu: sân trước lâu đài) cho stub nối luồng. Chặn một phần routine **07**.
3. **Nhạc Prologue + Vampire Killer**: pack `393996` chỉ có SFX. Chặn phần âm thanh của **07**.

Video **không có SFX nào** ở Title/Intro (Title im lặng, bấm Start không có tiếng) ⇒ cụm này **không dùng file nào trong 393996**; chỉ lưu vào `Art_Source/` để dành cho cụm gameplay.

## 3. Đối chiếu video ↔ sheet (planner đo)

- **Tỷ lệ pixel 8:7 trong video.** Trình giả lập kéo ngang ⇒ video **×1.96 theo x, ×1.71 theo y** (đo trên 2 dải ô vuông màn Title: NES y16 → video y20, NES y200 → video y335; ô cách nhau 32 px video / 16 px NES). Đổi toạ độ: `x_NES ≈ (x_video − 11) / 1.96`, `y_NES ≈ (y_video + 7.4) / 1.71`. **Không dùng "÷2.05"** như bản kế hoạch đầu.
- **Video là bản khác với sheet**: dòng copyright trong video là 3 dòng `TM AND © 1988` / `KONAMI INDUSTRY CO.,LTD.` / `LICENSED BY NINTENDO` (y 160/168/176, dòng 3 bắt đầu x ≈ 48); sheet là bản US 4 dòng `TM AND © 1987` / `KONAMI INDUSTRY CO.,LTD.` / `LICENSED BY` / `NINTENDO OF AMERICA INC.`. Mọi phần khác trùng sheet (logo, ô vuông, thanh xám, tháp, vị trí PUSH START KEY). **Quyết định planner: làm theo video** (yêu cầu là "như video"), ghép chữ bằng glyph cắt từ chính dòng copyright trên sheet — đủ ký tự, số `8` lấy từ "1987".
- Thanh xám ngang dưới Title **thuộc game** (có trong sheet) — câu hỏi cũ đã đóng.
- Màn hình NES 256×240. Title dùng y 0–223. Intro = HUD (y 0–47) + nền 256×192 (y 48–239) — khớp video (cảnh bắt đầu ở video y≈73 → NES ≈47).
- Tốc độ Simon tính lại theo hệ số mới: ≈235 px video / 1.96 ≈ 120 px NES trong ≈240 f ⇒ **0.5 px/frame** (không đổi).
- Trình giả lập **cắt 8 dòng trên** (thanh xám NES y 0–7 không thấy trong video), dòng dưới vẫn thấy thanh xám y 216–223.

### 3a. Lệnh ghép so sánh CHUẨN (planner đã chạy thử: đè panel A lên `ref/01` khớp logo/ô vuông/thanh xám)
Đưa ảnh chụp game về đúng hệ toạ độ của video (256×240 NES → 502×410, đặt tại (11,−7) trên nền 524×476) rồi ghép cạnh nhau + ảnh hiệu:
```
ffmpeg -y -i shot.png -i ref.png -filter_complex "[0]crop=<W>:<H>:<X>:<Y>,scale=502:410:flags=neighbor[g];color=c=black:s=524x476:d=1[bg];[bg][g]overlay=11:-7:shortest=1,format=rgb24,split[a1][a2];[1]format=rgb24,split[r1][r2];[a1][r1]hstack[s];[a2][r2]blend=all_mode=difference[d]" -map "[s]" -frames:v 1 cmp_side.png -map "[d]" -frames:v 1 cmp_diff.png
```
`<W>:<H>:<X>:<Y>` = vùng game 256×240 trong ảnh chụp (vd scale 4× trên 1920×1080 → `1024:960:448:60`; lấy số thật ở `CVTI_MEMORY §4`). Ảnh hiệu: chỗ trùng ra đen (viền mảnh do nén h264 là bình thường), chỗ khác sáng lên. Chạy trong `ClaudeNotes/cvti/_cache/` (đường dẫn có `C:` trong filter dễ lỗi).

## 4. Timeline chuẩn (đo từ video)

| Mốc video | Sự kiện | Ghi chú dựng lại |
|---|---|---|
| 0.93–1.03 s | đen | |
| **1.03 s** | **Title hiện** — cắt cứng, KHÔNG fade-in | Title **im lặng** (không nhạc, không SFX) |
| 1.03 s → | Animation tháp + dơi (40 khung sheet) chạy; dơi nhỏ thấy rõ trong video từ ~1.8 s | khung đầu dơi rất nhỏ, capture thấp nên khó thấy |
| ~1.8–3.3 s | Dơi bay vòng xuống **trước** tháp, to dần, rồi lên trên-trái tháp | `ref/03_title_bat_path…png` |
| ~3.3 s → hết Title | Dơi **lơ lửng vỗ cánh** ở trên-trái tháp (không lặp đường bay) | các khung cuối của 40 khung |
| 1.03–6.90 s | "PUSH START KEY" hiện **đứng yên** (không nháy trước khi bấm) | |
| **6.90 s** | Người chơi bấm Start | không có SFX |
| 6.90–8.23 s | "PUSH START KEY" **nháy**: tắt ~0.13 s / bật ~0.13 s (≈ 8 frame NES mỗi pha), 5 lần tắt, tổng ≈ 1.33 s (≈ 80 frame) | dơi vẫn vỗ cánh |
| **8.23 s** | Cắt cứng sang **đen** | |
| 8.23–8.50 s | đen ≈ 0.27 s (≈ 16 frame) | |
| 8.44 s | Nhạc **Prologue** bắt đầu | |
| **8.50 s** | **Intro hiện** (cắt cứng): HUD + cảnh cổng lâu đài đêm trăng | |
| 8.50–12.5 s | **Simon đi bộ** từ mép phải vào **giữa cổng sắt** ≈ 0.5 px/frame | `ref/06_intro_simon_walk…png` |
| ~12.6 s | Simon đổi sang **tư thế quay lưng**, đứng yên | `ref/07_intro_back_pose_t12.8.png` |
| 8.5–15.87 s | **Mây trôi sang trái** qua mặt trăng; **2 dơi nhỏ** bay lượn (rõ từ ~11.5 s) | `ref/08_intro_sequence…png` |
| 14.85 s | Nhạc Prologue hết | |
| **15.87 s** | Cắt cứng sang **đen** | |
| 15.87–16.40 s | đen ≈ 0.53 s (≈ 32 frame) | |
| **16.40 s** | **Stage 1** hiện + nhạc **Vampire Killer** bắt đầu | `ref/09_stage1_start_t17.9.png`; TIME 0300→0299 lúc ~17.5 s |

Mọi chuyển cảnh trong video đều là **cắt cứng** (không fade/crossfade).

## 5. Bố cục HUD (video, `ref/05_intro_start_t8.6.png`)
```
SCORE-000000 TIME 0300 STAGE 01
PLAYER ████████████████  [khung đỏ]  ♥-05
ENEMY  ████████████████  [subweap ]  P-03
```
PLAYER/ENEMY = 16 vạch đầy; khung vũ khí phụ viền đỏ, trống; tim đỏ `-05`; `P-03`. HUD nằm trong y 0–47.

## 6. Quy ước kỹ thuật đề xuất (routine 02/04 CHỐT — muốn đổi ghi lý do vào `CVTI_MEMORY §4`)

- Unity **2022.3.62f3**, Built-in 2D, package có sẵn: `2d.pixel-perfect`, `2d.sprite`, `timeline`, `textmeshpro`, UniTask, DOTween (+Pro), Odin. Input: **Input Manager cũ** (`activeInputHandler: 0`).
- Độ phân giải logic NES **256×240**, Pixel Perfect Camera, upscale số nguyên + viền đen trên Game view 1920×1080. PPU đề xuất **16**. Texture: **Point, Compression None, không mipmap**.
- **Pixel vuông** (mặc định, nét) — video dùng 8:7 nhưng kéo ngang không nguyên lần sẽ làm pixel rộng hẹp không đều. Ghi `§5` hỏi người dùng có muốn tuỳ chọn 8:7 không.
- Logic khung hình theo **60 fps NES**: thời lượng tính bằng *số frame* (vd nháy 8f, đen 16f/32f).
- Thư mục: nguồn thô `Art_Source/` (ngoài Assets, có `SOURCES.md`); sprite đã cắt `Assets/Data/Art/{Title,Intro,HUD,Font,Character/Simon,Stage1}/`; âm thanh `Assets/Data/Audio/{Music,SFX}/`; code `Assets/Data/Script/{System,Title,Intro,UI,Anim,Audio}/`; editor tool `Assets/Data/Editor/`; công cụ python `Tools/cvti/`.
- **Không sửa submodule `Assets/TuyenAframework`**. Được *dùng*: `afsdk.ManualSingletonMono<T>`, `afsdk.ui.CanvasManager/BaseUIMenu`, `HandleAnim`, UniTask, DOTween.
- Python không có Pillow — `python -m pip install --user pillow`, hoặc đọc pixel bằng `ffmpeg -f rawvideo -pix_fmt rgba -` (planner đã dùng cách này), hoặc Editor script C#.

## 7. Danh sách routine (chạy TUẦN TỰ — mọi routine đụng Unity Editor dùng chung 1 instance)

| # | taskId | Model · effort | Phụ thuộc | Việc chính |
|---|---|---|---|---|
| 00 | `cvti-00-ha-tang-timeline-video-va-so-ghi-nho` | Sonnet 5.5 · medium | — | Sổ `CVTI_MEMORY`, xác minh timeline, baseline project, commit kế hoạch + ảnh tham chiếu |
| 01 | `cvti-01-thu-thap-sprite-va-sound-nguon` | Sonnet 5.5 · medium | 00 | Nhập tài nguyên đã tải tay vào `Art_Source/`, kiểm sha256, lập danh mục, xin phần còn thiếu |
| 02 | `cvti-02-nen-render-pixel-perfect-nes` | Opus 5.5 · high | 01 | Camera 256×240 pixel-perfect, PPU, preset import, quy ước toạ độ NES→Unity |
| 03 | `cvti-03-cat-sprite-title-intro-hud-font` | Opus 5.5 · high | 02 | Cắt sprite Title/Intro/Simon (+ HUD/Stage1 nếu đã có sheet) + bảng glyph font |
| 04 | `cvti-04-khung-game-flow-input-audio-nestext` | Opus 5.5 · high | 03 | State machine màn hình, input Start, cắt-đen theo frame, AudioManager, `NesText`, `FrameAnimator` |
| 05 | `cvti-05-man-title-dang-va-nhay-push-start` | Opus 5.5 · high | 04 | Màn Title đúng pixel + 40 khung tháp/dơi + nháy PUSH START KEY |
| 06 | `cvti-06-hud-dung-chung-score-time-stage` | Sonnet 5.5 · high | 04 + **sheet HUD** | HUD prefab dùng chung, bind dữ liệu |
| 07 | `cvti-07-intro-simon-di-vao-cong-va-noi-stage1` | Opus 5.5 · high | 05, 06 | Cảnh Intro (Simon, mây, dơi nhỏ, nhạc) + nối Stage 1 stub |
| 08 | `cvti-08-nghiem-thu-so-khop-video-goc` | Opus 5.5 · xhigh | 00–07 | Chụp Play Mode so với video gốc theo mốc, đo timing, báo đạt/chưa đạt |

## 8. Câu hỏi mở (ghi vào `CVTI_MEMORY §5`, đừng tự quyết thay người dùng)
- Để yên ở Title lâu thì có cần làm demo/attract mode không? (video không thể hiện ⇒ mặc định KHÔNG làm)
- Pixel vuông (mặc định) hay giả lập tỷ lệ 8:7 như video?
- Nhạc Prologue / Vampire Killer lấy từ đâu (pack SFX không có)?
- Sheet HUD/font và sheet Stage 1 — người dùng tải thêm khi nào?
- Stage 1 sau intro: cụm này chỉ dựng **stub** (màn đầu + HUD + nhạc) để nối luồng; gameplay Stage 1 là cụm khác.
