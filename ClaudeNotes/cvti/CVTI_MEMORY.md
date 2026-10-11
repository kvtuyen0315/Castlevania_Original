# CVTI_MEMORY — sổ bàn giao cụm Title + Intro Castlevania (NES)

*Mọi routine CVTI: đọc file này + CVTI_PLAN.md ĐẦU phiên, cập nhật CUỐI phiên, commit cùng routine.*
*Yêu cầu gốc của người dùng (11/10/2026), nguyên văn:* "Hãy lên danh sách routines về làm task làm Title và đoạn intro game như video trên" — video `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Title_Intro.mp4`; Sprite Resource: https://www.spriters-resource.com/nes/cv/ ; Sound Resource: https://sounds.spriters-resource.com/nes/cv/

## §1 Trạng thái cụm

| # | Routine | Trạng thái | Commit | Ngày | Ghi chú |
|---|---|---|---|---|---|
| 00 | hạ tầng timeline + sổ | **XONG** (ảnh Play Mode baseline chụp lại 11/10 nhưng ra TRẮNG, xem §7) | (commit "CVTI 00") | 11/10/2026 | đo bằng ffmpeg + python; MCP mất kết nối khi vào Play Mode |
| 01 | thu thập sprite/sound nguồn | **XONG** | (commit "CVTI 01") | 11/10/2026 | nhập 3 PNG + zip + 37 WAV, sha256/md5 khớp; 3 thứ còn thiếu → §5 |
| 02 | nền render pixel-perfect NES | **XONG** | (commit "CVTI 02") | 11/10/2026 | 256×240, PPU 16, scale 4× trên 1920×1080 = `1024:960:448:60`; ảnh lưới khớp từng pixel ở 3×/4×/6× |
| 03 | cắt sprite Title/Intro/HUD/font | **XONG phần Title/Intro/Simon/Font; HUD + Stage 1 chờ sheet** | (commit "CVTI 03") | 11/10/2026 | 77 sprite từ manifest `Tools/cvti/slices.json`, chạy 2 lần md5 y hệt; ảnh lưới 0 pixel lệch; `NesFont.asset` 26 glyph |
| 04 | khung game flow/input/audio/NesText | **XONG** | (commit "CVTI 04") | 11/10/2026 | 1 scene + 3 prefab màn placeholder; NesClock 60 Hz; log frame đúng ±0 (80/16/442/32) ở 20 fps và ~378 fps; 3 ảnh đã xem |
| 05 | màn Title + dơi + nháy PUSH START | **XONG** | (commit "CVTI 05") | 11/10/2026 | prefab Title thật; 19 ảnh Play khớp 0 pixel lệch với ảnh ghép từ PNG; dơi 37 khung bay 3/4 f xen kẽ (129 f, xấp xỉ) + lơ lửng 37-38-39-38 ×4 f; log nháy 80 / đen 16 ±0 |
| 06 | HUD dùng chung | **CHƯA — DỪNG (thiếu sheet HUD)** | (không có commit code) | 11/10/2026 | `Text Font.gif` mới chỉ là bảng chữ, vẫn thiếu thanh máu/tim/khung vũ khí phụ; xem §5 #13, §7 |
| 07 | Intro Simon + nối Stage 1 stub | **CHƯA — DỪNG ở Bước 0 (cổng 06 chưa đạt)** | (chỉ commit ghi sổ) | 11/10/2026 | 06 chưa XONG, vẫn chưa có sheet HUD; xem §7 |
| 08 | nghiệm thu so với video gốc | CHƯA | | | |

## §2 Timeline & số đo (video 524×476, 30 fps container, capture thực ~7.5 fps ⇒ sai số mốc ±0.13 s; frame NES = giây × 60)

Đổi toạ độ video→NES (đo lại 11/10/2026): `x_NES = (x_v − 11)/1.969`, `y_NES = (y_v + 7.4)/1.712`.

### 2.1 Mốc đã xác minh

| Mốc | Giây video | Frame NES ≈ | Nguồn đo |
|---|---|---|---|
| Đen (UI emu kết thúc) | 0.93 | 56 | YAVG toàn khung = 16 |
| **Title hiện** (cắt cứng) | **1.03** | 62 | YAVG 16→47.5 |
| Dơi bắt đầu cử động (nhỏ, ở ngọn tháp) | **1.07** | 64 | diff frame vùng tháp (xem 2.2) |
| Dơi bay ra sau tháp / lên trái | 2.83 | 170 | diff frame |
| Dơi bắt đầu lơ lửng vỗ cánh | ~3.2–3.4 | ~195–205 | autocorr pose |
| PUSH START KEY tắt #1 | **6.90** | 414 | YAVG vùng chữ 61→16 |
| tắt #2 / #3 / #4 / #5 | 7.17 / 7.43 / 7.70 / 7.97 | 430/446/462/478 | idem; chu kỳ 0.267 s ≈ **16 f** (tắt 8 f + bật 8 f) |
| bật lại sau #5 | 8.10 | 486 | idem |
| **Cắt đen** | **8.23** | 494 | YAVG = 16 |
| Nhạc Prologue bắt đầu | 8.436 | 506 | silencedetect (-45 dB) |
| **Intro hiện** (HUD+cảnh) | **8.50** | 510 | YAVG 16→46.7 |
| Simon dừng, đổi quay lưng | **12.70** (khung đầu tiên ≡ khung cuối là 12.70; chuyển động cuối thấy ở 12.67) | 762 | diff vùng Simon |
| Nhạc Prologue hết | 14.855 | 891 | silencedetect |
| **Cắt đen** | **15.87** | 952 | YAVG = 16 |
| **Stage 1 hiện** | **16.40** | 984 | YAVG 16→49.4 |
| Âm Vampire Killer | bắt đầu 16.414; có quãng lặng 16.946–17.25 | | silencedetect |

Thời lượng: Title im lặng 1.03→6.90 (5.87 s ≈ 352 f) · nháy 6.90→8.23 (1.33 s = **80 f**) · đen 8.23→8.50 (0.27 s ≈ **16 f**) · Intro 8.50→15.87 (7.37 s ≈ 442 f) · đen 15.87→16.40 (0.53 s ≈ **32 f**). Mọi chuyển cảnh là cắt cứng. Không có âm thanh trước 8.436 s (Title im lặng, bấm Start không SFX) ✓.

### 2.2 Số đo bổ sung

**Dơi Title** (crop video x373,y171; toạ độ NES quy đổi):
- Planner ghi "xuất hiện ~1.8 s" → **LỆCH**: dơi đã cử động từ **1.07 s** ngay sau khi Title hiện (nhỏ, bbox ~13×16 video px ở ngọn tháp ≈ NES (215–222, 118–127)); capture thấp + nhỏ nên khó thấy.
- Đường bay: 1.07 s ngọn tháp → bay sang phải & to dần: 2.2 s bbox NES x≈235–248, y≈119–133 (xa nhất bên phải) → 2.7 s lệch xuống/ trước tháp NES x≈222–239, y≈134–150 (thấp nhất) → 2.83 s quặt lên-trái NES x≈211–229 → 3.2 s đã ở trên-trái tháp, mép trái NES x≈186, mép trên chạm đỉnh ô (y≈104). Khớp "40 khung sheet" ⇒ chạy tuần tự, mỗi khung ≈ (3.2−1.03)/… không đo được từng khung (capture 7.5 fps); routine 05 chia đều 40 khung và nghiệm thu bằng ảnh.
- **Lơ lửng**: 4 tư thế lặp, chu kỳ **0.274 s ≈ 16 f** (autocorr lag 4 mẫu; cùng tư thế lặp ở 3.40/3.67/3.93/4.23/4.50/4.77 s) ⇒ mỗi tư thế ≈ 4 f NES. Không dừng/đổi pha cho tới 6.90 s và tiếp tục khi PUSH START nháy.

**Simon intro**:
- x đầu: khung đầu (8.50 s) mép trái sprite video x=489 ⇒ **NES x ≈ 243** (sprite 16 px, đang chui vào từ mép phải 256).
- x dừng: mép trái video 248 ⇒ **NES x ≈ 120.6**, tâm ≈ **128.6** ≈ giữa cổng (tâm cổng video 262 ⇒ NES 128) ✓.
- Tốc độ: hồi quy tuyến tính mép trái 8.6–11.5 s = −58.0 video px/s = **−29.5 NES px/s = 0.49 px/f** (planner 0.5 ✓); đi 122 px trong 4.2 s (≈252 f) từ 8.50 đến 12.70.
- Chân Simon ≈ NES y 209 (video y≈350), cao 32 ⇒ đỉnh y≈177.
- Nhịp walk cycle: độ rộng bbox dao động chu kỳ **7 mẫu @30 fps ≈ 0.233 s ≈ 14 f NES** (autocorr đỉnh ở lag 7/14/21/28; chuyển hẹp↔rộng cách nhau 0.10–0.17 s). **Không phân giải được từng tư thế** (capture 7.5 fps, sheet có 3 sprite đi) ⇒ routine 07 làm xấp xỉ: ping-pong 3 khung, tổng chu kỳ ≈ 14 f, ghi quyết định vào §4.
- Đổi quay lưng: **12.70 s** (planner 12.6; lệch +0.1, trong sai số), đứng yên tới hết cảnh (diff = 0 từ 12.70 đến 13.6).

**Mây intro**: sprite 32×16, **vẽ SAU/TRÊN mặt trăng** (che trăng, tối). Mép trái NES x ≈ 222 (8.6 s) → 189 (14.0 s); tốc độ **≈ 7.1 NES px/s ≈ 0.118 px/f** (≈1 px mỗi 8.5 f) trôi sang TRÁI; y NES ≈ 72–88. Đến 15.5–15.87 s mây vẫn còn đè lên mép trái mặt trăng (chưa trôi qua hẳn). Mặt trăng NES x≈189–216, y≈66–89 (cố định).

**2 dơi nhỏ intro** (đều vỗ cánh: luân phiên dạng "v" và "—", chu kỳ < 0.5 s, không đo chính xác):
- Dơi A: có sẵn từ đầu Intro ở ngọn tháp lâu đài (NES ≈ (146,69) lúc 9.0 s), trôi chậm sang trái-xuống ≈ (121,72) lúc 13.0 s.
- Dơi B: **xuất hiện 11.0 s** từ vùng trái (NES ≈ (48,105)), bay sang phải & lên ≈ 13 px/s (0.22 px/f), tới ≈ (101,67) lúc 15.0 s. Planner "rõ từ ~11.5 s" ≈ đúng (B hiện 11.0).
- Không đo được: quỹ đạo dao động chi tiết ⇒ routine 07 làm xấp xỉ.

**Hệ số 8:7 (đo lại)**: ô vuông ngang bắt đầu x=11, 12 ô cách 189 px ⇒ ×1.969 (8 px NES ≈ 15.75); y: dải 1 video 20–32 (NES 16–23), dải 2 335–348 (NES 200–207) ⇒ ×**1.712**, gốc y −7.4; thanh xám NES 216 → video 363 ✓. **Xác nhận** planner (1.96/1.71, gốc 11/−7.4), tinh chỉnh x thành 1.969.
**Lệnh ghép §3a** chạy thử OK (panel A phóng 4×, đặt (448,60), W:H:X:Y=1024:960:448:60): logo/ô vuông/thanh xám khớp (ảnh hiệu chỉ còn viền 1px do nén h264); sáng ở đúng: dòng copyright, PUSH START KEY (sheet có thanh chấm tím/xanh), vùng tháp ô-liu, chữ "1987" vs "1988". Ảnh: `_cache/shots/00/cmp_side.png`, `cmp_diff.png` (đã xem).

### 2.3 Dơi Title — khung sheet ↔ thời lượng (routine 05 đo 11/10/2026)

**Cách đo**: crop video vùng tháp (x368 y162 150×110) ở **30 fps đầy đủ** (`ffmpeg -ss 0.9 -t 6.2 … -f rawvideo`), mỗi frame video so (MAE) với 40 khung `title_bat_NN` đã chiếu sang toạ độ video theo 8:7 (dịch tối ưu (−2,−1) px video tìm trên khung 00) ⇒ khung khớp nhất. Script + kết quả: `_cache/05/match.py`, `match.txt`, `fit.py`, `sim.py` (không commit).
- **Sửa số cũ**: ở vùng tháp video đổi hình **mỗi 2 frame video (≈15 fps)**, không phải 7.5 fps — mỗi khung sheet bắt được 1–3 mẫu. Khung 00, 08, 17 không bắt được (lọt giữa 2 mẫu).
- **Đường bay = khung 00 → 36 chạy tuần tự đúng thứ tự sheet, mỗi khung 1 lần**, bắt đầu ngay khi Title hiện (khung 01 đã thấy ở mẫu Title đầu tiên 1.033 s). Hồi quy thời điểm xuất hiện: **0.0598 s/khung**; quy về đồng hồ NES của video (chu kỳ lơ lửng 0.2701 s = 16 f ⇒ 1 f = 16.9 ms, trình giả lập chạy ~59.2 Hz) ⇒ **≈ 3.54 f/khung**. Khung 37 xuất hiện 3.200 s ⇒ **≈ 129–130 f** sau khi Title hiện.
- Từng khung là 3 hay 4 f **không phân giải được** (mẫu cách 2 f + jitter capture ±1 mẫu; mô phỏng mọi mẫu lặp 3/4 chỉ khớp tối đa 48/68 mẫu, các mẫu [3,4], [4,3], [3,4,4] ngang nhau) ⇒ **chọn xen kẽ 3,4,3,4… (khung chẵn 3 f, lẻ 4 f) = 129 f — XẤP XỈ**; tổng thời lượng và thứ tự là số đo chắc.
- **Lơ lửng = 37 → 38 → 39 → 38 lặp**, mỗi tư thế 4 f (chu kỳ 16 f, đo 14 chu kỳ 3.2–7.0 s: 16.21 f, sai lệch tối đa 0.028 s). Khung 36 KHÔNG thuộc vòng lặp. Không dừng/đổi pha tới 7.07 s (hết đoạn crop, đang nháy).

| khung sheet | frame NES bắt đầu (local Title) | giữ (f) | video thấy lần đầu (s, ≈ local f) |
|---|---|---|---|
| 00 | 0 | 3 | không bắt được |
| 01 / 02 / 03 | 3 / 7 / 10 | 4 / 3 / 4 | 1.033 (L1) / 1.067 (L3) / 1.133 (L7) |
| 04 / 05 / 06 / 07 | 14 / 17 / 21 / 24 | 3 / 4 / 3 / 4 | 1.200 / 1.300 / 1.367 / 1.433 (L11/17/21/25) |
| 08 / 09 / 10 / 11 | 28 / 31 / 35 / 38 | 3 / 4 / 3 / 4 | — / 1.500 / 1.567 / 1.633 (–/29/33/36) |
| 12 / 13 / 14 / 15 | 42 / 45 / 49 / 52 | 3 / 4 / 3 / 4 | 1.700 / 1.767 / 1.833 / 1.900 (40/44/48/52) |
| 16 / 17 / 18 / 19 | 56 / 59 / 63 / 66 | 3 / 4 / 3 / 4 | 1.967 / — / 2.033 / 2.100 (56/–/60/64) |
| 20 / 21 / 22 / 23 | 70 / 73 / 77 / 80 | 3 / 4 / 3 / 4 | 2.133 / 2.233 / 2.267 / 2.333 (66/72/74/78) |
| 24 / 25 / 26 / 27 | 84 / 87 / 91 / 94 | 3 / 4 / 3 / 4 | 2.400 / 2.467 / 2.500 / 2.600 (82/86/88/94) |
| 28 / 29 / 30 / 31 | 98 / 101 / 105 / 108 | 3 / 4 / 3 / 4 | 2.667 / 2.733 / 2.800 / 2.833 (98/102/106/108) |
| 32 / 33 / 34 / 35 / 36 | 112 / 115 / 119 / 122 / 126 | 3 / 4 / 3 / 4 / 3 | 2.867 / 2.933 / 3.033 / 3.067 / 3.133 (110/114/119/121/125) |
| **37 → 38 → 39 → 38** (lặp) | **129** | 4 mỗi tư thế | 3.200 (L≈130) |

(Cột cuối: L ≈ (t − 1.017)·59.24, Title hiện giữa 1.000 và 1.033 s.) Mọi khung lệch dự kiến ≤ 4 f — trong sai số mẫu.

**Nháy PUSH START (đo lại `ref/brightness_series.tsv` cột 3)**: tắt (16.0) ở 6.900 / 7.167 / 7.433 / 7.700 / 7.967 s, bật (61.0) ở 7.033 / 7.300 / 7.533 / 7.833 / 8.100 s, đen 8.233 s, Intro 8.500 s ⇒ chu kỳ 0.267 s = 16 f (8 tắt + 8 bật), 5 lần tắt, 80 f, đen 16 f — **khớp hằng số cũ, không đổi**.

## §3 Tài nguyên

Nguồn + sha256 + credit: `Art_Source/SOURCES.md` (và `CVTI_PLAN.md §2`). Đã nhập 11/10/2026, **sha256 cả 4 file khớp planner**; 37 WAV giải nén lại từ zip, md5 từng file khớp `Downloads\393996\`. Sprite đã cắt: xem **§3.1** (routine 03).

### 3.1 Bảng sprite đã cắt (routine 03 — 11/10/2026) — routine 05–07 đọc bảng này, KHÔNG mở lại sheet

Sinh bằng `python -P -E Tools/cvti/slice_sprites.py` từ `Tools/cvti/slices.json` (rect, key, ghi chú từng mục ở manifest). Mọi sprite: import Single, PPU 16, Point, Uncompressed, no mip, mesh FullRect. Màu nền sheet đo được = `(0,128,0)` (cả 3 sheet); ô sprite nhỏ Intro/Simon nền `(116,116,116)`. "đục" = không key (giữ cả nền đen).

| tên | file | sheet | rect (x,y,w,h trong sheet) | pivot | key | ghi chú |
|---|---|---|---|---|---|---|
| `title_base` | `Assets/Data/Art/Title/title_base.png` | title_screen | (1,1,256,240) | Top-Left | đục | Panel A = toàn màn Title, đặt NES (0,0). **Đã tô đen**: ô ô-liu NES (184,104,72,56), thanh chấm tím/teal NES (72,136,112,8), chữ copyright US NES (32,160,192,32). Kiểm: trước khi tô vùng đó chỉ chứa ô-liu / tím+teal / đen+trắng chữ (không ăn vào C-dao găm, logo); sau khi tô 0 pixel (128,128,0)/(128,0,128)/(0,128,128). 8 màu. |
| `title_bat_00` … `title_bat_39` | `Assets/Data/Art/Title/title_bat_NN.png` | title_screen | (1+73·c, 256+57·r, 72, 56), NN = 10·r + c | Top-Left | đục | "Bat Animation" khung NN (tháp + dơi ghép sẵn, nền đen) — đè tại NES **(184,104)**. 00 = chỉ tháp (dơi chưa thấy rõ) → dơi bay phải/to dần → sà trước tháp → lên trên-trái → các khung cuối vỗ cánh lơ lửng. Viền 1 px quanh mỗi ô trên sheet là nền xanh ✓. |
| `push_start_key` | `Assets/Data/Art/Title/push_start_key.png` | title_screen | (558,1,112,8) | Top-Left | đục | Đặt NES **(72,136)**; trạng thái tắt khi nháy = ẩn renderer (nền dưới đã đen). 2 màu (trắng 252 + đen). |
| `glyph_<k>` (26 cái) | `Assets/Data/Art/Font/glyph_<k>.png` | title_screen | ô 8×8, xem manifest (`char`, `alsoAt`) | Top-Left | **key đen** | Ký tự: `1 7 8 9 A B C D E F H I K L M N O P R S T U Y , . ©` — tên file `<k>` = chính ký tự, riêng `copyright`=©, `period`=., `comma`=,. Mọi lần ký tự lặp lại trên sheet đều trùng từng pixel (kiểm `alsoAt`). Glyph chỉ có 2 màu (trắng + đen nền tile) ⇒ key đen là đúng ngoại lệ H (nền tile đen, glyph không chứa đen). `TM` = 2 glyph `T`+`M` thường; `©` là glyph riêng; `,` và `.` là glyph riêng (`,` = chấm + 1 px đuôi ở dòng 7). P, H lấy từ `PUSH START KEY` (558+8i, 1). |
| `intro_bg` | `Assets/Data/Art/Intro/intro_bg.png` | cutscenes_credits | (1,1,256,192) | Top-Left | đục | Nền cảnh Intro, đặt NES **(0,48)** (dưới HUD). 11 màu. |
| `intro_bat_0` / `intro_bat_1` | `Assets/Data/Art/Intro/intro_bat_{0,1}.png` | cutscenes_credits | (1,194,8,16) / (10,194,8,16) | Top-Left | xám 116 | Dơi nhỏ 2 khung (0 = cánh chữ "v", 1 = cánh ngang); hình chỉ chiếm 8×7 trên cùng, phần dưới trong suốt. Đen thật giữ nguyên (30 / 28 px). |
| `intro_cloud` | `Assets/Data/Art/Intro/intro_cloud.png` | cutscenes_credits | (19,194,32,16) | Top-Left | xám 116 | Mây (đen + tím (204,136,252)), vẽ đè trước mặt trăng. |
| `idle_0` | `Assets/Data/Art/Character/Simon/idle_0.png` | simon_belmont | (1,21,16,32) | **Bottom-Center** | xám 116 | Simon đứng. |
| `walk_0` / `walk_1` / `walk_2` | `…/Simon/walk_{0,1,2}.png` | simon_belmont | (29,21) / (46,21) / (63,21), 16×32 | Bottom-Center | xám 116 | 3 khung đi theo thứ tự sheet. **`walk_0` trùng từng byte với `idle_0`** (md5 821c2363…) — đúng như game gốc (khung đứng là 1 pha của chu kỳ đi). |
| `back_0` | `…/Simon/back_0.png` | simon_belmont | (362,21,16,32) | Bottom-Center | xám 116 | Khung "Intro" quay lưng. |

**Hướng mặt Simon trong sheet: quay TRÁI** (mặt/tay ở bên trái hình). Intro Simon đi sang trái ⇒ routine 07 dùng nguyên, **không flipX**; khi đi phải (gameplay) mới `flipX = true`. Simon dùng 3 màu (124,8,0)/(252,152,56)/(252,216,168) — không có đen.

**Dòng copyright theo video** (ghép bằng glyph, đã thử ghép + so `ref/01` bằng lệnh §3a: khớp, căn lệch tối ưu (1,1) video px giống hệt logo ⇒ vị trí đúng): dòng 1 `TM AND © 1988` tại NES **(72,160)**; dòng 2 `KONAMI INDUSTRY CO.,LTD.` tại **(32,168)**; dòng 3 `LICENSED BY NINTENDO` tại **(48,176)** (thử x 40/44/48/52/56 — 48 là duy nhất khớp). Ô 8 px, không khoảng cách thêm. Ảnh: `_cache/03/cmp_side.png`, `zoom_copy.png`.

**Font (`Assets/Data/Art/Font/NesFont.asset`, class `NesFont` ở `Assets/Data/Script/UI/NesFont.cs`)**: 26 glyph; API `TryGetSprite(char, out Sprite)`, `CanRender(string, out char missing)`, hằng `NesFont.CELL = 8`; dấu cách = tiến 1 ô, không sprite. Dựng lại: menu **CVTI/Build NesFont** (`Assets/Data/Editor/NesFontBuilder.cs`, quét `glyph_*.png`; tên đặc biệt đã khai sẵn `hyphen`→`-`, `heart`→`♥` cho sheet HUD sau này). Kiểm `CanRender`: 3 dòng copyright video ✓, `PUSH START KEY` ✓, `PLAYER` ✓, `ENEMY` ✓; **HUD thiếu `G`, `-`, `0`, `2`–`6` (và icon ♥)** → §5.

### Bản đồ sheet đã xác nhận (đo bằng ffmpeg rawvideo + python, 11/10/2026)

| Sheet | Thành phần | Rect (x,y,w,h trong sheet) | Màu cần key | Kiểm |
|---|---|---|---|---|
| title_screen | Panel A Title đầy đủ | (1,1,256,240) | KHÔNG key (đen là đen thật; nền sheet (0,128,0) nằm ngoài panel) | góc (1,1)=(188,188,188), (256,240)=đen, x=257 xanh lá ✓ |
| title_screen | Ô ô-liu (128,128,0) = chỗ tháp+dơi | NES (184,104,72,56) = sheet (185,105,72,56) | — (khung bat đè lên) | bbox đo = (185,105,72,56) ✓ |
| title_screen | Thanh chấm tím/xanh = chỗ PUSH START | NES (72,136,112,8) = sheet (73,137,112,8) | — | tím từ x73 + teal từ x81 ghép = 112×8 tại y137 ✓ |
| title_screen | Thanh xám (188,188,188) trên/dưới | NES y 0–7, 216–223 | — | sheet y1–8 / 217–224 ✓ |
| title_screen | `PUSH START KEY` bật | (558,1,112,8) | không (chữ (252,252,252) trên đen) | 2 màu ✓ |
| title_screen | `PUSH START KEY` tắt (đen) | (558,10,112,8) | — | toàn đen ✓ |
| title_screen | 8 dơi lẻ (tham khảo) | y127: x528/545/562/579/596 (16×16), 613 (24×32), 638, 671 (32×32) | không | 5 ô đầu không dính nền xanh ✓ |
| title_screen | 40 khung "Bat Animation" 72×56 | x=1+73·c, y=256+57·r (c 0..9, r 0..3) | KHÔNG key | cả 40 ô: có đen, không lẫn xanh lá ✓; đã xem hình |
| title_screen | Hộp credit (loại trừ) | (506,486,224,32) | — | thấy trên hình |
| cutscenes_credits | Nền Intro | (1,1,256,192) | KHÔNG key (11 màu; không có cây/mây/dơi) | x=257 và y=193 ra ngoài nền ✓ |
| cutscenes_credits | Dơi nhỏ khung 1 | (1,194,8,16) | xám (116,116,116) | xám 98 px, 2 màu ✓ |
| cutscenes_credits | Dơi nhỏ khung 2 | (10,194,8,16) | xám (116,116,116) | xám 100 px ✓ |
| cutscenes_credits | Mây | (19,194,32,16) | xám (116,116,116) | xám 272 px, 3 màu ✓; ô kế (x51) là xanh lá |
| simon_belmont | Idle | (1,21,16,32) | xám (116,116,116) | 4 màu, xám 223 px, viền xanh lá ✓ |
| simon_belmont | Walk 1/2/3 | (29,21), (46,21), (63,21) (16×32) | xám | xám 223/268/227 px ✓ |
| simon_belmont | Intro/quay lưng | (362,21,16,32) | xám | xám 178 px ✓ |

**Không lệch so với planner** (mọi rect khớp). Lưu ý: panel A bắt đầu ở (1,1) nên toạ độ sheet = toạ độ NES + 1.

### Danh mục 37 SFX (`Art_Source/sounds-resource/393996/`, để dành cho cụm gameplay; không đoán tên)

| file | thời lượng | md5 |
|---|---|---|
| 01.wav | 0.56 s | 89286c306a599b201892e808e6193ace |
| 02.wav | 0.10 s | 606648a8fa95f5dd2a9414713b645b0c |
| 03.wav | 0.19 s | 9a75d5ee09ed23c47371b20a5062f9ab |
| 04.wav | 0.37 s | 6790b0bb857deb44ea858e32ce03ef73 |
| 05.wav | 0.55 s | 83afb2e26bbefba904655004b4bf0f8b |
| 06.wav | 0.54 s | a04b8464a334b390587e4d203e9ef3b9 |
| 07.wav | 0.11 s | ddffd586c2a03c25829497325655eaba |
| 08.wav | 0.18 s | 0d14463468a6a60aa33bd32a5270af53 |
| 09.wav | 0.18 s | a46679ec13b868ba2b78a908699e52fc |
| 10.wav | 0.25 s | d50a2e822557e3284fade599e9e1adea |
| 11.wav | 2.05 s | 7acc49bf4e2ab9543c1d887f2b284193 |
| 12.wav | 1.64 s | 5776c93a5b2f1b254fcf896415223778 |
| 13.wav | 0.13 s | 7a3654f098818626fea0ce36c9ac60ae |
| 14.wav | 0.73 s | b9dba53b274a45c81955a70c874796d2 |
| 15.wav | 1.01 s | 99fffc9ce0eb5c009166629b36afe411 |
| 16.wav | 0.40 s | 1abc4e93946dd7cf1dc106625ae456c6 |
| 17.wav | 0.40 s | 4a194ce5b6f24676ad70bf1597b43094 |
| 18.wav | 0.79 s | eef154aadec729a1f050499c5bc3ca42 |
| 19.wav | 0.45 s | 40de178a75ffc588ebd4390850bb8c3a |
| 20.wav | 0.36 s | 8c21631e10b3433efd50e363d2018ed0 |
| 21.wav | 0.60 s | 6b49d959cd96dad37df1b9716af90687 |
| 22.wav | 0.50 s | 95d7ab928bd31a379eeead4de40cc1a1 |
| 23.wav | 1.04 s | a32b044451da616aa3c9b7953718a2eb |
| 24.wav | 0.75 s | 64873fb6c4200ddfd5fd538aebf5c30a |
| 25.wav | 1.22 s | 045b425faf36752acb75452c17a42740 |
| 26.wav | 2.40 s | c7496a7187593fa952dc2cd10468e384 |
| 27.wav | 0.83 s | 82aebea8d7f85927fae9d182c2877c13 |
| 28.wav | 0.83 s | 200a739475b17ea59683b6e00ce5d6fe |
| 29.wav | 0.81 s | b28d71a8d634148dd5c265154aac303b |
| 30.wav | 1.16 s | 4bca08eb62ebb476646b9481cbf6ec2d |
| 31.wav | 1.15 s | df7851a075c7a3b930e1a9d76625ae85 |
| 32.wav | 2.14 s | 43a5ed9ae847f29d460d2beecb37f391 |
| 33.wav | 0.50 s | 805f2b56df493e169e3e599fe4b01d53 |
| 34.wav | 1.54 s | 1f98ad45ac7abaf6121fed1519108522 |
| 35.wav | 1.62 s | d369a58fb3b1827cc21e129f535b3dcb |
| 36.wav | 0.66 s | 58a5d963ca7d08d9fc6d61d517b839d6 |
| 37.wav | 0.11 s | f002d9b8c4499b0117612cd865e1a2b0 |

## §4 Quyết định kỹ thuật đã chốt

| Quyết định | Lý do | Routine chốt |
|---|---|---|
| Hệ số video→NES: x ÷1.969, y (+7.4)÷1.712 | đo lại trên 2 dải ô vuông, §2.2 | 00 |
| Thời lượng tính bằng frame NES 60 fps, chuẩn: nháy 8f tắt + 8f bật ×5 (80 f), đen 16 f trước Intro, đen 32 f trước Stage 1, Simon 0.5 px/f ≈ 252 f, mây 0.118 px/f | §2 | 00 (hằng số `NesTiming` thuộc routine 04) |
| Hiến pháp H1–H7 giữ nguyên như đề bài | | 00 |

### 4.1 Nền render (routine 02 chốt — 11/10/2026)

| Quyết định | Giá trị | Lý do |
|---|---|---|
| Độ phân giải logic | **256×240** (đủ màn NES, không cắt 8 dòng như TV/trình giả lập) | Intro dùng hết y 0–239 (HUD 0–47 + nền 48–239); Title có thanh xám y 0–7 trong sheet. Cắt 224 dòng sẽ mất 8 dòng đáy cảnh Intro. Video cắt 8 dòng trên là do trình giả lập — lệnh ghép §3a đã tính (overlay y −7). |
| Tỷ lệ pixel | **Vuông** (1:1) | Scale nguyên lần ⇒ pixel đều. 8:7 (ngang ×1.143) không nguyên lần ⇒ cột pixel rộng hẹp không đều. Câu hỏi để ở §5 #2, KHÔNG tự làm 8:7. |
| PPU | **16** (`NesCoord.PPU`) | 1 metatile NES 16×16 = 1 unit, tile 8×8 = 0.5 unit; màn = 16×15 unit — số tròn cho vật lý/tilemap Stage sau. PPU 8 cũng được nhưng khối 16 px (đơn vị thiết kế màn của Castlevania) sẽ là 2 unit. |
| Camera | `MainCamera` (tag **MainCamera** — sửa nợ §6 cũ), ortho size 7.5, pos `NesCoord.CameraHome` = (8, −7.5, −10), SolidColor **#000000 alpha 1**, depth 0 + `UnityEngine.U2D.PixelPerfectCamera` (package 2d.pixel-perfect 5.1.1): assetsPPU 16, ref 256×240, upscaleRT ✓, pixelSnapping ✓, cropFrameX ✓, cropFrameY ✓, stretchFill ✗ | upscaleRT: vẽ vào RT 256×240 rồi phóng Point ⇒ vật ở vị trí lẻ pixel (Simon 0.5 px/f) vẫn ra đúng lưới pixel NES. |
| **Camera xoá viền** | con `MainCamera/LetterboxClear`: Camera depth −100, SolidColor đen, cullingMask Nothing, rect full | Built-in: PixelPerfectCamera thu `pixelRect` về vùng game nên vùng viền KHÔNG được ai xoá ⇒ ra rác trắng/đen (ảnh `grid_1920x1080_noclear.png`; đây cũng là lý do ảnh baseline routine 00 ra trắng). Mọi scene có camera game phải mang theo con này. |
| Bảng scale (Game view cố định) | **1280×720 → 3× = 768×720 @ (256,0)** · **1920×1080 → 4× = 1024×960 @ (448,60)** · **2560×1440 → 6× = 1536×1440 @ (512,0)** | đo bằng `cam.pixelRect` trong Play + ảnh chụp so từng pixel với PNG gốc: 0 pixel lệch, 0 pixel không đen ngoài vùng game ở cả 3 mức. zoom = min(⌊W/256⌋, ⌊H/240⌋). |
| **Vùng game trong ảnh chụp 1920×1080** | **`1024:960:448:60`** (W:H:X:Y, gốc trên-trái ảnh) | dùng cho lệnh ghép `CVTI_PLAN §3a`. |
| Hệ toạ độ | NES (x phải, **y xuống**, gốc trên-trái màn) → world `(x/16, −y/16)`; góc trên-trái màn = world (0,0), góc dưới-phải = (16, −15). API `NesCoord`: `SCREEN_W/H`, `PPU`, `UNIT_PER_PIXEL`, `CameraHome`, `ToWorld(int,int)`, `ToWorld(float,float)` (vị trí lẻ pixel), `ToNes(Vector3)` (làm tròn). | Đặt object bằng toạ độ đo trên sheet/video là so pixel trực tiếp. Màn cuộn (Stage) sau này: toạ độ NES = toạ độ pixel trong level, camera dời theo x — không đổi quy ước. |
| Pivot | **Top-Left** cho mọi sprite, trừ `Assets/Data/Art/Character/**` = **Bottom-Center** | Top-Left: transform = `ToWorld(góc trên-trái đo trên sheet)`. Nhân vật: transform = `ToWorld(tâm x, mép dưới)` — vd Simon 16×32 góc (120,177) ⇒ `ToWorld(128, 209)`; tiện cho chân chạm sàn/lật hướng. Sheet cắt Multiple (routine 03) tự đặt pivot từng sprite theo đúng quy ước này. |
| Preset import | `Assets/Data/Editor/NesSpriteImporter.cs` (`OnPreprocessTexture`), chỉ `Assets/Data/Art/**`, chỉ khi `importSettingsMissing` (lần đầu): Sprite, PPU 16, **Point**, **Uncompressed**, no mipmap, alphaIsTransparency, Clamp, mesh **FullRect**, pivot như trên | Đã thử: import mới ra đúng hết; chỉnh tay PPU 32 rồi force reimport → giữ 32 (không đè). Lưu ý: file PNG chép vào CÙNG đợt với lúc script importer mới tạo sẽ import trước khi compile ⇒ phải xoá `.meta` và import lại (đã gặp ở routine 02). |
| Sorting layers (dưới → trên) | `Default`, `Background`, `BackgroundFX`, **`Character`**, `Foreground`, `HUD`, `Overlay` | Thêm bằng SerializedObject trên TagManager. **Dùng lại layer `Character` có sẵn** (uniqueID 733023783, không asset nào dùng) thay vì tạo `Characters` trùng nghĩa. `Default` để dưới cùng: object quên đặt layer sẽ bị che ⇒ lộ lỗi ngay. `Overlay` = màn đen cắt cảnh. |
| Chữ & UI | **SpriteRenderer world-space** (Title, HUD, NesText) — KHÔNG dùng uGUI Canvas Screen Space Overlay/TextMeshPro | Canvas Overlay không đi qua PixelPerfectCamera ⇒ không nguyên pixel, lệch scale. Nếu buộc phải Canvas thì Screen Space - Camera trỏ `MainCamera` + sprite Point, ghi lý do ở đây. |
| Scene test | `Assets/Scenes/Test/PixelGrid.unity` (MainCamera + LetterboxClear giống Main + sprite `Assets/Data/Art/_Test/PixelGrid.png` ở NES (0,0), layer Background); ảnh lưới sinh bằng `python -I Tools/cvti/make_pixel_grid.py <out.png>` | Giữ lại để routine sau kiểm nhanh pixel-perfect sau khi đổi camera. Camera trong scene test là BẢN SAO — nếu routine 04 đổi camera Main thì sửa cả bản này. |
| Kiểm từng pixel ảnh chụp | `python -I Tools/cvti/check_pixel_exact.py <shot> <src.png> <S> <X> <Y> <shotW> <shotH> [srcW srcH]` — so RGBA từng khối S×S với PNG gốc + ngoài vùng game phải đen alpha 255 | 0 lệch ⇒ không nhoè, không lệch nửa pixel. Phải so cả alpha: vùng không ai xoá ra (0,0,0,**0**) — trình xem ảnh hiện TRẮNG, so RGB sẽ lọt. |

### 4.2 Cắt sprite (routine 03 chốt — 11/10/2026)

| Quyết định | Giá trị | Lý do |
|---|---|---|
| Công cụ | **python** `Tools/cvti/slice_sprites.py` + manifest `Tools/cvti/slices.json` (nguồn sự thật, 1 mục/dòng). `--check` = chỉ kiểm. Script sinh manifest lần đầu chỉ ở `_cache/03/gen_manifest.py` (không commit) — từ nay **sửa thẳng `slices.json`**. | Kiểm pixel (nền sót, viền rect, glyph lặp) dễ viết bằng Pillow, chạy ngoài Unity, không cần Editor mở. |
| Pillow | **Đã có sẵn** (12.2.0, user site) — KHÔNG cần pip. Phải chạy `python -P -E` (an toàn đường dẫn), **không `-I`** vì `-I` bỏ user site ⇒ `No module named PIL`. Tool cũ của 02 (ffmpeg) vẫn chạy `-I` được. | |
| Lặp lại byte | PNG RGBA, `compress_level=9`, không metadata; pixel trong suốt ghi (0,0,0,0). Chạy 2 lần ⇒ md5 77 file y hệt; Unity import không đổi PNG. | |
| Đầu ra | **mỗi frame một PNG** (Single), tên phẳng trong thư mục nhóm (`title_bat_NN.png`, `walk_N.png`, `glyph_X.png`) — KHÔNG atlas Multiple | Preset `NesSpriteImporter` (02) áp thẳng (PPU/pivot theo thư mục), diff git rõ từng frame, giống quy ước code cũ. 02 viết "Sheet cắt Multiple tự đặt pivot" — không dùng Multiple nên không cần. |
| Key nền | Chỉ key màu nền ô: xám (116,116,116) cho Intro nhỏ + Simon; **glyph font key đen** (ngoại lệ có kiểm: glyph chỉ 2 màu); Title/push_start/intro_bg **đục**, giữ đen. | H5/§3 đề bài. |
| Kiểm tự động trong tool | 0 pixel màu nền sheet (0,128,0) trong sprite; viền 1 px ngoài rect = nền sheet (trừ glyph); `fill` chỉ được chứa màu `allow`; `forbidColors`; `allowColors`; `alsoAt` trùng từng pixel; key phải xoá ≥1 và < hết pixel. Lỗi ⇒ không ghi file nào. | |
| Sprite dơi lẻ hàng y127 (8 cái) | **Không cắt** | 05 dùng 40 khung ghép sẵn (đề bài mặc định). Cần thì thêm dòng vào manifest. |
| Sprite Simon cũ trong git `2bfd90b^` | **Không dùng** | Đã có sheet gốc, cắt trực tiếp khớp từng pixel; không cần so bản cũ (bản cũ import Bilinear/PPU 100). |

### 4.3 Khung game flow (routine 04 chốt — 11/10/2026)

| Quyết định | Giá trị | Lý do |
|---|---|---|
| Kiến trúc màn hình | **1 scene `Main.unity`**: root `GameManager` (prefab) + root `ScreenRoot` chứa 3 prefab màn `Assets/Data/Prefabs/Screens/{TitleScreen,IntroScreen,Stage1Stub}.prefab` (để **inactive** trong scene). Mỗi màn = 1 component kế thừa `GameScreen` (`Id`, `OnEnter()`, `OnTick(localFrame)`, `OnExit()`). `GameManager.Awake` tự gom mọi `GameScreen` trong scene (`FindObjectsOfType(true)`, trùng `Id` ⇒ error) và tắt hết. | Cắt cứng NES = bật/tắt GameObject trong 1 tick, không load scene (load scene không đảm bảo đúng frame). Gom tự động ⇒ routine 05/07 chỉ sửa nội dung prefab/class, không phải nối tham chiếu. |
| Class màn | `TitleScreen` (`Script/Title/`), `IntroScreen` (`Script/Intro/`), `Stage1Stub` (`Script/Stage1/`) — hiện là **placeholder**; routine 05/07 **sửa thẳng class + prefab này** (giữ tên, giữ `Id`). | Không phải đổi script trên prefab/scene. |
| State machine | `eGameScreen { Boot, Title, Intro, Stage1 }`; `GameManager.I.State`, `IsTransitioning`; **`GoTo(eGameScreen next, int blackFrames)`** (trả false nếu đang chuyển). Boot → Title ở tick 1 (cắt thẳng). | |
| Thứ tự trong 1 tick | `NesClock` tăng `Frame` → `OnSystemTick` (chỉ GameManager: `InputController.Tick()` → đếm đen / `current.OnTick(frame − enterFrame)`) → `OnTick` (FrameAnimator, object trong màn). Màn mới: `OnEnter` ở tick bật (localFrame 0), `OnTick` từ localFrame 1. | Input đọc 1 lần/tick như NES; logic luồng chạy trước animation nên màn bật ở tick T vẽ ngay khung đầu ở frame T. |
| Cắt đen | **Tắt màn cũ, không màn nào bật** ⇒ camera xoá đen (#000, §4.1). `GoTo` ở tick T ⇒ màn đen hiện ở các frame T…T+N−1, màn mới bật ở tick T+N (đúng N frame đen). `blackFrames ≤ 0` = cắt thẳng trong cùng tick. Đang đen: không màn nào nhận tick ⇒ Start bị bỏ qua. | Không cần sprite đen (H2), không phụ thuộc sorting. Layer `Overlay` vẫn để dành nếu sau này cần phủ đen mà giữ object chạy. **HUD dùng chung (06) phải là con của prefab màn** (hoặc tự ẩn theo `GameManager.State`) để tắt cùng lúc khi đen. |
| Đồng hồ 60 Hz | `NesClock` (`ManualSingletonMono`, `[DefaultExecutionOrder(-100)]`, con `NesClock` của prefab GameManager): accumulator `Time.unscaledDeltaTime` trong `Update`, bước `NesTiming.FRAME_SECONDS = 1/60`, **tối đa 4 tick/Update** (thừa thì bỏ — game chậm lại, không nhảy cóc). Event **static** `NesClock.OnSystemTick` / `NesClock.OnTick`, `NesClock.Frame` static (reset 0 ở Awake). **Không** đổi `ProjectSettings/TimeManager.asset`, không đặt `targetFrameRate`/vSync. | Không đụng setting toàn project; FixedUpdate 0.02 giữ nguyên cho vật lý cụm sau. Đã thử "snap" dt về 1/60 khi lệch < 2 ms ⇒ đoạn nháy 80 f đo được 1.302 s thay vì 1.333 s (đồng hồ nhanh hơn thời gian thực ⇒ lệch nhạc) ⇒ **bỏ snap**. Sau khi bỏ: 59.99–60.11 tick/s, nháy 1.334 s. |
| Hằng thời lượng | `Assets/Data/Script/System/NesTiming.cs`: `FPS`, `FRAME_SECONDS`, `TITLE_BLINK_HALF 8`, `TITLE_BLINK_COUNT 5`, `TITLE_BLINK_TOTAL 80`, `TITLE_BAT_HOVER_POSE 4`, `BLACK_AFTER_TITLE 16`, `BLACK_AFTER_INTRO 32`, `INTRO_TOTAL 442`, `INTRO_SIMON_WALK 252`, `ToSeconds(int)`. Routine sau thêm hằng mới vào đây (H4). | Số lấy từ §2. |
| Phím Start | Input Manager cũ, `KeyCode`: **Return, KeypadEnter, Space, JoystickButton7, JoystickButton9**. `InputController` (`[DefaultExecutionOrder(-200)]`, con của prefab GameManager): `Update` chốt `GetKeyDown`; `Tick()` (GameManager gọi đầu tick) tính `StartPressedThisFrame` = chốt ∨ (giữ ∧ tick trước chưa giữ) — đúng 1 tick, giữ không lặp. `StartHeld`. **`InjectStart()`** giả lập 1 lần nhấn (test MCP). | Không dùng axis/Input Manager asset ⇒ không đổi ProjectSettings. Chốt để fps cao (nhiều Update/tick) hay thấp (nhiều tick/Update) đều không lọt/không lặp. |
| Nháy PUSH START (placeholder) | Tick bấm Start = blinkFrame 0 ⇒ ẩn; hiện khi `(blinkFrame / 8) % 2 == 1`; blinkFrame 80 ⇒ `GoTo(Intro, 16)`. Khớp video: tắt #1 = tick bấm, bật lại sau #5 = +72, đen = +80. | 05 giữ đúng logic này khi dựng thật. |
| Audio | `AudioManager` (`ManualSingletonMono`, con của prefab GameManager, 2 `AudioSource` gán sẵn: `musicSource`, `sfxSource`, playOnAwake tắt): `PlayMusic(AudioClip, bool loop)` (phát từ đầu, `null` ⇒ chỉ dừng + log debug "chưa có clip"), `StopMusic()`, `PlaySfx(AudioClip)` (PlayOneShot), `IsMusicPlaying`. **Clip serialized trên prefab màn** (`IntroScreen.music`, `Stage1Stub.music`), không Resources.Load. Gọi trong `OnEnter` ⇒ cùng tick bật màn (log ghi frame). `TitleScreen.OnEnter` gọi `StopMusic` (Title im lặng). | Video: nhạc Prologue bắt đầu 8.436 s, Intro hiện 8.50 s (lệch ~4 f, trong sai số capture ±0.13 s) ⇒ mặc định phát cùng tick bật màn; 07/08 chỉnh nếu đo được chắc chắn. |
| `NesText` | `Assets/Data/Script/UI/NesText.cs`. Serialized: `font` (NesFont.asset), `text`, `color`, `sortingLayer` (mặc định **`HUD`**), `sortingOrder`. Mỗi ký tự 1 SpriteRenderer con `c<i>` (pool, tạo ở runtime trong `Awake`/`SetText`), cách 8 px; `'\n'` xuống 8 px; dấu cách/ký tự thiếu tiến 1 ô. API: `SetText(string)`, `SetVisible(bool)` (bật/tắt renderer, không dựng lại), `SetColor(Color)` (tint — glyph trắng), `SetNesPosition(x, y)` (localPosition = `NesCoord.ToWorld`, giả định cha ở gốc), `Text`, `Visible`. Thiếu glyph ⇒ `LogWarning` **1 lần mỗi ký tự** (static), không crash. Transform = góc trên-trái ô đầu. | Không ExecuteAlways: trong Edit mode chữ không hiện (chỉ thấy khi Play) — chấp nhận để khỏi sinh object con vào prefab. |
| `FrameAnimator` | `Assets/Data/Script/Anim/FrameAnimator.cs`. Serialized: `target` (SpriteRenderer), `frames`, `durations` (tick/khung; rỗng ⇒ `frameDuration`, mặc định 8), `loop`, `playOnEnable`. API: `Play()`, `Play(Sprite[], int[] durations, bool loop, Action onComplete)`, `Play(Sprite[], int each, bool loop, Action onComplete)`, `Stop()`, `SetFrame(i)`, `IsPlaying`, `CurrentIndex`, event `OnComplete`. Chạy theo `NesClock.OnTick`; tick cùng frame với `Play` bị bỏ qua ⇒ khung 0 giữ đúng `durations[0]` frame. One-shot dừng ở khung cuối sau đủ thời lượng rồi gọi `OnComplete`. | Đã kiểm: durations {2,3,4} one-shot ⇒ đổi khung ở +2, +5, COMPLETE ở +9; loop 4 ⇒ đổi ở +4, +8, +12… |
| Log mốc frame | `CvtiDebug.Log(msg)` ⇒ `[CVTI] frame N (t=…s): msg` **chỉ khi** `CvtiDebug.LogFlow`. Bật bằng `GameManager.logFlow` (prefab, mặc định tắt) **hoặc menu `CVTI/Log Flow Frames`** (EditorPrefs `CVTI.LogFlow`, không đổi asset; có hiệu lực từ lần vào Play sau). Mốc đang log: `Boot→Title`, `Title: Start`, `X→Black (Nf)`, `Black→Y`, `X→Y` (cắt thẳng), `PlayMusic …`, `StopMusic`. Đo: thời lượng đoạn = hiệu số frame giữa 2 mốc. | Routine 08 bật menu rồi đọc console. Hiện EditorPrefs để **tắt** (đã trả về sau khi test). |
| Test qua MCP | `execute_code` (codedom) đăng ký delegate vào `NesClock.OnSystemTick` để `InputController.I.InjectStart()` đúng frame, `GameManager.I.GoTo(eGameScreen.Title, 0)` để chạy lại từ Title trong cùng phiên Play, `Application.targetFrameRate` (runtime, không lưu) để đổi fps. Không đổi `QualitySettings` khi Play (dễ ghi vào asset). | |
| GameManager.prefab | Đã **xoá 2 con `AnimationManager`, `InputManager` mang script MISSING** (nợ §6 từ 02); thêm con `NesClock`, `InputController`, `AudioManager`. Còn `GlobalInfo` (rỗng, giữ nguyên). | |

### 4.4 Màn Title (routine 05 chốt — 11/10/2026)

| Quyết định | Giá trị | Lý do |
|---|---|---|
| Prefab `TitleScreen` | Con (đều localPosition = `NesCoord.ToWorld`): `Base` SpriteRenderer `title_base` NES (0,0) layer **Background**; `TowerBat` SpriteRenderer + `FrameAnimator` (loop ✗, playOnEnable ✗) NES **(184,104)** layer **BackgroundFX**; `PushStartKey` SpriteRenderer `push_start_key` NES **(72,136)** layer **HUD**; `Copyright1..3` `NesText` (layer HUD) tại **(72,160) `TM AND © 1988`**, **(32,168) `KONAMI INDUSTRY CO.,LTD.`**, **(48,176) `LICENSED BY NINTENDO`**. Placeholder `TitleText` của 04 đã xoá. | x/y copyright theo §3.1 (routine 03 đã chốt bằng lệnh ghép); kiểm lại ở 05: lệch tương đối < 1 px NES (§7). |
| PUSH START KEY | **Sprite** cắt từ sheet (không dựng bằng NesText) — tắt khi nháy = `SpriteRenderer.enabled = false` (nền dưới đã đen). | Pixel đúng sheet (H1); cùng hình với glyph nhưng 1 renderer gọn hơn. |
| `TitleScreen` | Serialized `pushStart` (SpriteRenderer), `bat` (FrameAnimator), `batFrames[40]` (`title_bat_00..39`). `Awake` dựng mảng bay (khung 0..36, thời lượng xen kẽ `TITLE_BAT_FLIGHT_SHORT/LONG`) + mảng lơ lửng theo `HOVER_ORDER = {37,38,39,38}`. `OnEnter`: hiện PUSH START, `bat.Play(bay, durations, loop ✗, StartHover)`, `StopMusic`. `StartHover` (gọi trong tick khung 36 hết hạn) ⇒ `bat.Play(hover, TITLE_BAT_HOVER_POSE, loop ✓)` ⇒ khung 37 hiện ngay tick đó (local 129). Log `CvtiDebug` "Title: dơi lơ lửng". Logic nháy giữ nguyên 04. | Dùng API có sẵn của FrameAnimator (one-shot + onComplete), **không sửa FrameAnimator/GameManager/NesText**. Vào lại Title (GoTo) ⇒ dơi chạy lại từ khung 00. |
| Hằng `NesTiming` mới | `TITLE_BAT_FLIGHT_FRAMES 37`, `TITLE_BAT_FLIGHT_SHORT 3`, `TITLE_BAT_FLIGHT_LONG 4` (khung chẵn 3 f, lẻ 4 f ⇒ 129 f); giữ `TITLE_BAT_HOVER_POSE 4`, `TITLE_BLINK_*`, `BLACK_AFTER_TITLE` (đo lại khớp). | §2.3 (xấp xỉ từng khung, tổng đo được). |
| Start trước khi dơi bay xong | **Hợp lệ** — nhận Start từ local frame 1; dơi tiếp tục bay/lơ lửng trong lúc nháy. | Video không cho thấy cấm (§5 #11). |
| Âm thanh | Không phát gì ở Title, kể cả khi bấm Start. | Video im lặng. Không làm demo/attract mode (§5 #1). |
| Test chụp ảnh đúng frame | `execute_code` đăng ký delegate `NesClock.OnTick` gọi `ScreenCapture.CaptureScreenshot(<đường dẫn tuyệt đối>)` ở local frame mong muốn (chạy sau FrameAnimator vì đăng ký sau) — chụp đúng khung đã log. Nhận diện khung dơi trong ảnh: `_cache/05/whichbat.py` (so với ảnh ghép `expected.py`). | `manage_camera screenshot` qua MCP không chọn được frame. Lưu ý: Unity mất focus ⇒ NesClock ngừng (delegate treo tới khi `AppActivate`). |

## §5 Câu hỏi cho người dùng (chép từ CVTI_PLAN §8 + phát sinh)

1. Để yên ở Title lâu thì có cần attract/demo mode không? (video không thể hiện ⇒ mặc định KHÔNG làm)
2. Pixel vuông (mặc định, nét) hay giả lập tỷ lệ 8:7 như video?
3. Nhạc Prologue / Vampire Killer lấy từ đâu? (pack 393996 chỉ có SFX; video có audio nhưng rất nhỏ ~-52 dB và lẫn UI emu — không nên cắt từ video)
4. Sheet HUD/font và sheet Stage 1 — người dùng tải thêm khi nào? (chặn routine 06 và phần Stage 1 của 07)
5. Stage 1 sau intro: chỉ dựng **stub** (màn đầu + HUD + nhạc) để nối luồng; gameplay Stage 1 là cụm khác — xác nhận.
6. (Mới) Dòng copyright: làm theo video "1988"/3 dòng (mặc định đã chọn theo yêu cầu "như video") — xác nhận được.
7. (02) Tỷ lệ pixel (= câu 2, chi tiết): routine 02 đã làm **pixel vuông** 256×240 scale nguyên. Nếu muốn giống video 8:7 thì có thể thêm *tuỳ chọn* kéo ngang ×8/7 bằng RT Point (pixel ngang sẽ rộng 4 hoặc 5 px màn hình xen kẽ ở 4×) — có muốn không? Mặc định KHÔNG.

8. (03) **Thiếu cho HUD/font** (sheet Title không có): chữ `G`; dấu `-`; chữ số `0`, `2`, `3`, `4`, `5`, `6` (có `1 7 8 9`); icon tim `♥`; thanh máu PLAYER/ENEMY; khung vũ khí phụ. Cần sheet HUD/font (§5 #4). Khi có: thêm dòng `glyph_<k>` vào `Tools/cvti/slices.json` (tên `hyphen`/`heart` đã khai sẵn trong `NesFontBuilder`), chạy slicer + menu CVTI/Build NesFont. Lưu ý: chữ `O` hiện có KHÔNG được dùng thay số `0` (H2 — không bịa).
9. (03) **Stage 1**: chưa có sheet ⇒ `Assets/Data/Art/Stage1/` chưa tạo. `Assets/Data/Art/HUD/` cũng chưa tạo.
10. (04) Bấm Start trong Intro có bỏ qua cảnh không? Video không thể hiện ⇒ mặc định KHÔNG (Start bị bỏ qua khi đen/Intro).
11. (05) Bấm Start khi dơi còn đang bay: hiện **cho phép** (dơi bay tiếp trong lúc nháy, rồi đen như thường). Game gốc có chặn/bỏ qua không? Video chỉ bấm khi dơi đã lơ lửng ⇒ không biết.
12. (05) Thời lượng từng khung bay 3 hay 4 f là xấp xỉ (§2.3). Nếu có nguồn chính xác hơn (capture 60 fps của bản gốc, hoặc dữ liệu ROM) thì sửa `NesTiming.TITLE_BAT_FLIGHT_*`.

13. (06) **Sheet HUD vẫn THIẾU — routine 06 dừng ở Bước 0.** 11/10/2026 12:43 người dùng tải thêm `Downloads/NES - Castlevania - Miscellaneous - Text Font.gif` (104×192, GIF87a, 1 khung, 4 màu: đen / trắng (255,255,254) / xám 176 / xám 120; sha256 `bbf0ddcacd8ddab7ea1b3ace2873d68d69d7dec5cf23cafab905170c20fbed0f`), đã chép nguyên byte vào `Art_Source/_inbox/` (chưa vào `spriters-resource/`, chưa đưa vào `slices.json`). Nội dung (đã xem): font bản **Famicom/Nhật** — hiragana, katakana, kanji, `0–9`, `A–Z` (kiểu cao-đậm), dấu `! ? " - . = :` và `a–z`; **KHÔNG có**: thanh máu PLAYER/ENEMY (ô đầy/rỗng), icon tim, khung vũ khí phụ, icon vũ khí. ⇒ chưa đủ dựng HUD (H2: không vẽ/suy pixel từ video). Cần người dùng tải thêm sheet có HUD từ https://www.spriters-resource.com/nes/cv/ (sheet chứa dải trạng thái `SCORE-000000 TIME 0300 STAGE 01`, thanh máu, tim, `P-03`). Lưu ý khi có: font gif là bản Nhật, kiểu chữ có thể khác glyph US đã cắt từ sheet Title (`1 7 8 9 A B…`) — so từng glyph trước khi trộn nguồn; ưu tiên glyph từ chính sheet HUD US.

**Trả lời cho §5 (routine 01, chạy vắng mặt — không hỏi được người dùng):** CHƯA TRẢ LỜI. Cần người dùng cung cấp đúng 3 thứ (chi tiết `Art_Source/_inbox/README.md`): (1) sheet HUD+font — chặn routine 06; (2) sheet Stage 1 — 07 dùng nền đen nếu thiếu; (3) nhạc Prologue + Vampire Killer — 07 không phát nhạc nếu thiếu. Tạm áp dụng phương án Recommended: người dùng tự tải vào `Art_Source/_inbox/`, routine 02–05 làm tiếp phần đã có; 06 tự kiểm sheet HUD.

## §6 Nợ & việc ngoài phạm vi

- Ảnh baseline Play Mode ra trắng toàn khung (xem §7) — routine 02 xem lại khi dựng camera. `manage_camera screenshot` mặc định lưu vào `Assets/Screenshots/` (tạo file game + .meta): phải chuyển sang `_cache` và xoá thư mục đó.
- Nhịp từng tư thế walk của Simon và từng khung dơi Title không đo được ở capture 7.5 fps.
- Nhiễu nền git (không thuộc cụm): `Assets/TuyenAframework.meta`, `Castlevania_Original-main.slnx`, `UserSettings/`, submodule `Assets/TuyenAframework` hiện cờ ` m`.
- (01) 393996 = SFX không tên, cụm gameplay sẽ định danh; không import WAV nào vào `Assets/` ở cụm CVTI.
- ~~Camera `MainCamera` Untagged ⇒ `Camera.main` null~~ — **đã sửa ở routine 02** (tag MainCamera).
- ~~Ảnh baseline Play Mode ra trắng~~ — **nguyên nhân (02)**: vùng không camera nào xoá ra alpha 0 (trình xem hiện trắng). Đã sửa bằng `LetterboxClear` + nền camera alpha 1.
- (02) **`Assets/Data/Prefabs/GameManager.prefab` có 2 con `AnimationManager`, `InputManager` mang script MISSING** (sót từ code cũ đã xoá) ⇒ vào Play Main báo 6 warning "The referenced script … is missing!". Không thuộc phạm vi 02 — **routine 04** dọn khi dựng input/flow.
- (02) **MCP rơi kết nối khi vào Play Mode** nếu cửa sổ Unity không được focus (Editor bị throttle, ping không trả lời; `runInBackground: 0`). Cách xử lý đã thử OK: sau `manage_editor play` chạy PowerShell `(New-Object -ComObject WScript.Shell).AppActivate(<PID Unity>)` rồi chờ ~4 s; PID lấy bằng `Get-Process Unity` (cái có MainWindowTitle). Không đổi `runInBackground` (ProjectSettings ngoài phạm vi) — routine 04 cân nhắc.
- (02) Build target hiện là **Android** (nhóm Game view = Android; size 1920×1080 Landscape = index 6, 1280×720 = 4, 2560×1440 = 10). `manage_camera screenshot` chụp theo size Game view. Không đổi build target.
- (02) Ngoài TagManager KHÔNG đổi ProjectSettings. Gợi ý cho **routine 04**: `Time.fixedDeltaTime`/vSync/`Application.targetFrameRate = 60` cho nhịp frame NES (H4).
- (02) Lỗi nền console không thuộc cụm (có từ trước): Odin `PersistentContextCache` ArgumentNullException `path1`, `Unity.Mathematics module data`, GUIClips — giữ nguyên.
- (03) `execute_code` của MCP chỉ có **codedom (C# 6)** — Roslyn không cài: không local function, `Object` mơ hồ ⇒ viết `UnityEngine.Object`. `AssetDatabase.DeleteAsset` bị safety check chặn ⇒ `safety_checks=false` (chỉ dùng cho file tạm của chính routine).
- (03) `manage_camera screenshot` nhận `output_folder` (vd `ClaudeNotes/cvti/_cache/shots/03`) ⇒ không còn rác `Assets/Screenshots/`.
- ~~(02) `GameManager.prefab` có 2 con script MISSING~~ — **đã dọn ở routine 04** (Play không còn 6 warning).
- (04) Thiếu glyph `G` ⇒ placeholder Stage 1 ghi **`SCENE 1`** thay vì "STAGE 1" (để console sạch). Routine 07 thay bằng cảnh thật; khi có sheet HUD (glyph G) thì "STAGE" vẽ được.
- (04) `runInBackground: 0` vẫn giữ: Editor mất focus ⇒ `Update` ngừng ⇒ NesClock ngừng (game "đóng băng", số frame không sai, chỉ thời gian thực dài ra). Test Play qua MCP phải `AppActivate` Unity (§6 mục 02) — đã làm vậy ở 04, MCP không rớt.
- (04) `NesText` chỉ dựng chữ khi Play (không ExecuteAlways) ⇒ mở prefab màn trong Edit mode không thấy chữ. Nếu 05–07 cần xem trước trong Editor thì thêm `[ExecuteAlways]` cẩn thận (đừng để glyph con bị lưu vào prefab).
- (04) Câu hỏi mới cho §5 (không chặn): có cần cho Start **bỏ qua Intro** không? Video không thể hiện ⇒ mặc định KHÔNG (Start bị bỏ qua khi đen/Intro).
- (05) `CVTI_PLAN.md` ghi capture video "~7.5 fps" — ở vùng tháp Title thực tế đổi hình **mỗi 2 frame video (~15 fps)** (§2.3). Routine 07/08 đo chuyển động nên crop đủ 30 fps rồi khớp mẫu thay vì giả định 7.5 fps.
- (05) Trình giả lập trong video chạy ~**59.2 Hz** (chu kỳ lơ lửng 16 f = 0.270 s; nháy 16 f = 0.267 s) — sai khác ~1 % so với 60 Hz, nhỏ hơn sai số mốc; KHÔNG chỉnh NesClock.
- (05) `read_console` của MCP trả mục **cũ nhất trước** và cắt theo `count` ⇒ console nhiều log thì phải `clear` trước khi test hoặc lọc `filter_text`.
- (05) Lệnh ghép §3a cho lệch chung (+1, 0…+2) px video (tăng dần theo y vì 410/240 = 1.708 < 1.712) ⇒ ảnh hiệu luôn còn viền mảnh ở MỌI thành phần (cả dải ô vuông của `title_base`). Muốn ảnh hiệu sạch hơn: overlay `12:-6`. Không phải lệch bố cục.

## §7 Nhật ký nghiệm thu

### Routine 00 — 11/10/2026
- Ảnh tham chiếu `ref/` đủ 9 PNG + tsv; đã mở xem cả 9 (01 qua `cmp_side`).
- Số đo lệch so với planner: dơi Title bắt đầu 1.07 s (không phải 1.8 s); Simon quay lưng 12.70 s (không 12.6 s); chi tiết hệ số x 1.969. Mọi mốc còn lại khớp (±1 frame video): Title 1.03, nháy 6.90–8.23, đen 8.23–8.50, nhạc 8.436, nhạc hết 14.855, đen 15.87–16.40.
- **BASELINE project**: Unity 2022.3.62f3, MCP kết nối đúng (`dataPath` = `D:/Unity/Castlevania_Original-main/Assets`). Git: HEAD `38c219c`. `Assets/Data`: `Prefabs/GameManager.prefab`, `Script/System/{GameManager,GlobalInfo}.cs`; `Assets/Scenes/Main.unity` có root `GameManager` (Transform+GameManager) và `MainCamera` (ortho, size 1.3, bg RGBA(0.192,0.302,0.475,0), pos (0,0,-10), Untagged). ProjectSettings: `defaultScreenWidth/Height` 1920×1080, `activeInputHandler: 0` (Input Manager cũ), `m_ActiveColorSpace: 0` (Gamma), Quality hiện tại = Ultra (index 5), không có SRP (Built-in). `Packages/manifest.json` có `2d.pixel-perfect 5.1.1`, `2d.sprite 1.0.0`, `2d.spriteshape 9.1.1`, `textmeshpro 3.0.9`, `timeline 1.7.7`, `ugui 1.0.0`, UniTask, unity-mcp (git). Console (Error/Warning) trước Play: **0 mục**.
- **DỞ**: ảnh Play Mode màn trống KHÔNG chụp được — sau `manage_editor play` kết nối MCP rơi (`no_unity_session`, instances=0) trong >5 phút (Editor.log có "The file 'MemoryStream' is corrupted"/Odin). Không ghi "đã nhìn thấy" (H7).

### Chạy lại routine 00 — 11/10/2026 (phiên 2, bổ sung baseline)
- MCP kết nối lại bình thường, vào Play Mode không rớt kết nối lần này. Chụp 2 ảnh `manage_camera screenshot` (game view 640×360): **cả 2 ảnh TRẮNG hoàn toàn** dù MainCamera là SolidColor bg RGBA(0.192,0.302,0.475,**0**) — nghi alpha=0 làm game view ra trắng/không render; chưa kết luận nguyên nhân. Ảnh lưu `_cache/shots/00/screenshot-20261011-11495{5,7}.png` (đã xem). Console có lỗi nền không thuộc cụm: Odin `Sirenix.OdinInspector.Editor.dll` broken assembly, `Unity.Mathematics module data`, `ArgumentNullException path1`. Screenshot tự ghi vào `Assets/Screenshots/` — đã chuyển sang `_cache` và xoá thư mục.
- Baseline hoàn tất, 0 file game thay đổi.

### Routine 01 — 11/10/2026
- Nhập 3 PNG + zip + giải nén 37 WAV vào `Art_Source/`; sha256 4 file khớp planner, md5 37 WAV khớp `Downloads\393996\`. `SOURCES.md` + `_inbox/README.md` đã tạo. Bản đồ sheet đo lại (§3): không lệch. Đã mở xem cả 3 sheet. 0 file mới trong `Assets/`.

### Routine 02 — 11/10/2026
- Tạo: `Assets/Data/Script/System/NesCoord.cs`, `Assets/Data/Editor/NesSpriteImporter.cs`, `Assets/Scenes/Test/PixelGrid.unity`, `Assets/Data/Art/_Test/PixelGrid.png`, `Tools/cvti/make_pixel_grid.py`, `Tools/cvti/check_pixel_exact.py`. Sửa: `Assets/Scenes/Main.unity` (MainCamera + PixelPerfectCamera + con LetterboxClear), `ProjectSettings/TagManager.asset` (5 sorting layer mới). Không đụng `Assets/TuyenAframework`.
- Ảnh nghiệm thu (thư mục `ClaudeNotes/cvti/_cache/shots/02/`, đều đã mở xem):
  - `grid_1920x1080.png` — lưới 256×240 ở NES (0,0): scale 4×, vùng game (448,60,1024,960), viền đen; `check_pixel_exact` **0/983 040 lệch, 0 pixel ngoài vùng không đen**. Xem phóng to `zoom_topleft.png` (viền đỏ 1 px = 4 px màn, ô xanh 8×8 ở (1,1)), `zoom_center.png` (caro 1 px đen/trắng nét, không nhoè).
  - `grid_1280x720.png` (3×, 0/552 960 lệch) · `grid_2560x1440.png` (6×, 0/2 211 840 lệch) — đều 0 pixel ngoài vùng không đen.
  - `grid_1920x1080_noclear.png` — TRƯỚC khi thêm LetterboxClear: vùng viền là rác (alpha 0 hiện trắng + mảng đen cũ), checker báo 737 676 pixel ⇒ bằng chứng cần camera xoá viền.
  - `title_panelA_1920x1080.png` — panel A sheet Title (crop (1,1,256,240), sprite tạm) ở NES (0,0): khớp từng pixel với PNG (0 lệch). Lệnh ghép §3a với `ref/01` → `cmp_side.png`, `cmp_diff.png`: ảnh hiệu **tối** ở logo, 2 dải ô vuông, thanh xám dưới (chỉ viền mảnh do h264); **sáng** ở dòng copyright (1987/4 dòng vs 1988/3 dòng), PUSH START KEY (thanh chấm tím/xanh), ô tháp ô-liu, và UI trình giả lập dưới đáy — đúng như dự đoán. Có thêm dải sáng mảnh ở mép trên (video y 0–6, chỗ thanh xám NES y 0–7 bị trình giả lập cắt) — chưa xác minh nguyên nhân; logo/ô vuông bên dưới khớp nên không phải lệch toạ độ chung.
  - `main_play_1920x1080.png` — Play `Main.unity`: toàn khung đen đặc (0 pixel khác (0,0,0,255)), `Camera.main` có, pixelRect (448,60,1024,960), zoom 4.
- Sprite tạm `TitlePanelA_tmp.png` + object trong scene test **đã xoá** trước commit (03 cắt bản chính thức).
- Console sau `refresh_unity`: không có error/warning nào từ file của routine 02; còn lỗi nền §6 (Odin, Mathematics, GUIClips) và 6 warning missing script của `GameManager.prefab` (§6, routine 04).
- `Main.unity` + `PixelGrid.unity` đã lưu (scene active cuối phiên: Main, không dirty). Game view để lại 1920×1080 Landscape.

### Routine 03 — 11/10/2026
- Tạo: `Tools/cvti/slices.json` (77 mục), `Tools/cvti/slice_sprites.py`, 77 PNG + `.meta` trong `Assets/Data/Art/{Title,Font,Intro,Character/Simon}/` (Title 42 = base + 40 bat + push_start_key; Font 26 glyph; Intro 4; Simon 5), `Assets/Data/Script/UI/NesFont.cs`, `Assets/Data/Editor/NesFontBuilder.cs`, `Assets/Data/Art/Font/NesFont.asset` (26 glyph). Không đụng `Assets/TuyenAframework`, không đổi scene Main.
- **Lặp lại**: chạy tool 2 lần ⇒ md5 77 file y hệt (`_cache/03/md5_1.txt` = `md5_2.txt`); sau khi Unity import vẫn y hệt.
- **Kiểm pixel độc lập** (`_cache/03/verify.py`, so lại với sheet): 0 pixel lệch ở cả 77 sprite (pixel đục = đúng màu sheet; trong suốt ⇔ đúng màu key; vùng `fill` đen). Tool tự kiểm: 0 pixel nền sheet, viền ngoài rect toàn nền sheet (mọi sprite trừ glyph), glyph lặp trùng từng pixel.
- **Import**: cả 77 Sprite Single, PPU 16, Point, Uncompressed, no mip, FullRect; pivot Top-Left (alignment 1) / Simon Bottom-Center (alignment 7) — kiểm bằng API + `.meta` (`filterMode: 0`, `textureCompression: 0` ở DefaultTexturePlatform, các nền tảng khác `overridden: 0`, `spritePixelsToUnits: 16`, `enableMipMap: 0`).
- **Ảnh lưới**: scene tạm `Assets/Scenes/Test/SpriteGrid_tmp.unity` (bản sao PixelGrid, refResolution 768×480 ⇒ zoom 2 trên 1920×1080 = vùng (192,60,1536,960), nền xám (160,160,160)), Play, chụp `ClaudeNotes/cvti/_cache/shots/03/sprite_grid_1920x1080.png` (đã xem) — `check_pixel_exact` so với ảnh kỳ vọng ghép từ PNG (`_cache/03/expected_grid.png`): **0/1 474 560 lệch, 0 pixel ngoài vùng không đen**. Xem phóng to `zoom_small_sprites.png` (glyph, dơi nhỏ, mây, 5 Simon — không sót xám 116, đen giữ nguyên, nét) và `zoom_bats.png` (khung bat, nét). Scene tạm **đã xoá**; scene active cuối phiên: Main, không dirty.
- **So video**: glyph ghép 3 dòng copyright theo video + panel A + bat_00 + push_start → lệnh §3a với `ref/01` (`_cache/03/cmp_side.png`, `cmp_diff.png`, `zoom_copy.png`, đã xem): chữ khớp; độ lệch tối ưu (1,1) video px giống hệt vùng logo ⇒ vị trí dòng đúng (§3.1).
- Console sau `refresh_unity`: 0 error/warning từ file của routine 03; chỉ còn lỗi nền §6 (warning framework `AnimatorState.cs`/`AFCacheWindow.cs`, Odin path1, Mathematics, GUIClips).
- **Thiếu** (§5 #8–9): glyph HUD `G - 0 2 3 4 5 6 ♥`, toàn bộ sprite HUD và Stage 1 — chờ sheet.

### Routine 04 — 11/10/2026
- Tạo: `Assets/Data/Script/System/{NesTiming,NesClock,CvtiDebug,InputController,GameScreen}.cs`, `Script/Audio/AudioManager.cs`, `Script/UI/NesText.cs`, `Script/Anim/FrameAnimator.cs`, `Script/Title/TitleScreen.cs`, `Script/Intro/IntroScreen.cs`, `Script/Stage1/Stage1Stub.cs`, `Assets/Data/Editor/CvtiDebugMenu.cs`, `Assets/Data/Prefabs/Screens/{TitleScreen,IntroScreen,Stage1Stub}.prefab`. Sửa: `GameManager.cs` (state machine), `GameManager.prefab` (xoá 2 con MISSING, thêm NesClock/InputController/AudioManager), `Main.unity` (thêm `ScreenRoot` + 3 instance màn, inactive). Không đụng `Assets/TuyenAframework`, không đổi ProjectSettings.
- Console sau `refresh_unity`: 0 error/warning từ `Assets/Data/**`; chỉ lỗi nền §6 (Odin path1, Mathematics, GUIClips, MCP WebSocket). Play: không còn warning missing script.
- **Log frame** (Play `Main.unity`, menu log bật tạm qua EditorPrefs, Start giả lập bằng `InjectStart` từ delegate trên `NesClock.OnSystemTick`):
  - Lượt 1 (fps Editor mặc định, NesClock còn bản có snap):
    ```
    [CVTI] frame 1 (t=2.536s): Boot→Title
    [TEST] frame 1086: InjectStart (Title)
    [CVTI] frame 1087 (t=22.293s): Title: Start
    [TEST] frame 1106: InjectStart (Title đang nháy)
    [CVTI] frame 1167 (t=23.595s): Title→Black (16f)
    [TEST] frame 1172: InjectStart (đen sau Title)
    [CVTI] frame 1183 (t=23.861s): Black→Intro
    [CVTI] frame 1183 (t=23.861s): PlayMusic: chưa có clip — im lặng
    [TEST] frame 1283: InjectStart (Intro)
    [TEST] frame 1624: InjectStart (Intro)
    [CVTI] frame 1625 (t=31.227s): Intro→Black (32f)
    [TEST] frame 1633: InjectStart (đen sau Intro)
    [CVTI] frame 1657 (t=31.760s): Black→Stage1
    ```
    ⇒ nháy 1167−1087 = **80**, đen 1183−1167 = **16**, Intro 1625−1183 = **442**, đen 1657−1625 = **32** (±0). Start lúc nháy / đen / Intro / đen bị bỏ qua, luồng không vỡ. Thời gian thực nháy 1.302 s (< 1.333) ⇒ phát hiện snap làm đồng hồ nhanh ⇒ bỏ snap (§4.3).
  - Lượt 2+3 (sau khi bỏ snap; cùng phiên Play, chạy lại bằng `GoTo(Title, 0)`):
    ```
    [TEST] run 0 targetFrameRate=20 frame 669: InjectStart
    [CVTI] frame 670 (t=14.518s): Title: Start
    [CVTI] frame 750 (t=15.867s): Title→Black (16f)
    [CVTI] frame 766 (t=16.117s): Black→Intro
    [CVTI] frame 1208 (t=23.517s): Intro→Black (32f)
    [CVTI] frame 1240 (t=24.017s): Black→Stage1
    [TEST] run 0 done: ticks=571 real=9.500s Update/s=20.0 ticks/s=60.11
    [CVTI] frame 1240 (t=24.017s): Stage1→Title
    [TEST] run 1 targetFrameRate=-1 frame 1270: InjectStart
    [CVTI] frame 1271 (t=24.531s): Title: Start
    [CVTI] frame 1351 (t=25.865s): Title→Black (16f)
    [CVTI] frame 1367 (t=26.130s): Black→Intro
    [CVTI] frame 1809 (t=33.496s): Intro→Black (32f)
    [CVTI] frame 1841 (t=34.030s): Black→Stage1
    [TEST] run 1 done: ticks=571 real=9.518s Update/s=377.7 ticks/s=59.99
    ```
    ⇒ ở **20 Update/s** (3 tick/Update) và **~378 Update/s** đều 80 / 16 / 442 / 32 frame, 60 tick/s; Intro 7.37–7.40 s thực (video 7.37 s).
- **FrameAnimator** (object tạm, đã xoá): `[TEST] FrameAnimator: f0:title_bat_00 f2:title_bat_01 f5:title_bat_02 COMPLETE@9 | loop each=4: f0:title_bat_00 f4:title_bat_01 f8:title_bat_02 f12:title_bat_00 f16:… f24:title_bat_00` ✓.
- **NesText**: `SetText("STAGE 1")` rồi `"STAGE 1 G"` ⇒ đúng **1** warning `[NesText] Không có glyph 'G' …`, không crash; trả lại `SCENE 1`; `SetVisible(false/true)` ✓.
- **Ảnh** (`ClaudeNotes/cvti/_cache/shots/04/`, 1920×1080, đều đã mở xem): `placeholder_title_1920x1080.png` (TITLE + PUSH START KEY), `placeholder_intro_1920x1080.png` (INTRO), `placeholder_stage1_1920x1080.png` (SCENE 1). Đo bằng python: chữ trắng (252,252,252) duy nhất, bbox NES Title (73,104)–(184,143) = TITLE ô (104,104) + PUSH START KEY ô (72,136) đúng vị trí đặt; 0 pixel khác đen ngoài vùng game (448,60,1024,960).
- Cuối phiên: thoát Play, `Application.targetFrameRate` = −1, EditorPrefs `CVTI.LogFlow` = false, scene Main không dirty.

### Routine 05 — 11/10/2026
- Sửa: `Assets/Data/Script/Title/TitleScreen.cs` (dựng thật), `Assets/Data/Script/System/NesTiming.cs` (+3 hằng dơi), `Assets/Data/Prefabs/Screens/TitleScreen.prefab` (Base/TowerBat/PushStartKey/Copyright1..3, xoá placeholder). **Không** đổi `Main.unity`, FrameAnimator, NesText, GameManager, `Assets/TuyenAframework`, ProjectSettings.
- Console sau `refresh_unity` + suốt Play: 0 error/warning mới (chỉ lỗi nền §6: warning framework, Odin path1, Mathematics, MCP WebSocket). `NesFont.CanRender` 3 dòng copyright = true (không warning thiếu glyph).
- **Log frame** (Play `Main.unity`, Game view 1920×1080, EditorPrefs LogFlow bật tạm; Start giả lập bằng `InjectStart` từ delegate `NesClock.OnTick`):
  ```
  [CVTI] frame 1 (t=2.654s): Boot→Title
  [CVTI] frame 130 (t=5.183s): Title: dơi lơ lửng            ← +129 f
  [CVTI] frame 1404 (t=26.426s): Title→Title                  (chạy lại từ Title, t0=1404)
  [CVTI] frame 1533 (t=29.255s): Title: dơi lơ lửng           ← +129
  [CVTI] frame 1756 (t=32.950s): Title: Start                 ← local 352 (= video 6.90−1.03 s)
  [CVTI] frame 1836 (t=34.285s): Title→Black (16f)            ← nháy 80 f, 1.335 s thực
  [CVTI] frame 1852 (t=34.550s): Black→Intro                  ← đen 16 f
  ```
  Đổi khung dơi (local, log từ delegate): `01@3 02@7 03@10 04@14 05@17 06@21 07@24 08@28 09@31 10@35 … 35@122 36@126 37@129 38@133 39@137 38@141 37@145 …` (lặp 16 f tới hết nháy). PUSH START: tắt 352, bật 360, tắt 368, 376 bật, 384, 392, 400, 408, tắt 416, bật 424, đen 432 ⇒ **5 × (8 tắt + 8 bật)** ✓. Start giả lập thêm ở 361/381/411 (đang nháy) và 436/441 (đen) ⇒ bị bỏ qua, không nhảy cóc.
  - Bấm sớm: Start ở local 5 (dơi khung 01) ⇒ `Title: Start` 8642 → `Black` 8722 (+80) → `Intro` 8738 (+16); dơi vẫn bay trong lúc nháy (khung 11 ở L40, 23 ở L84); Start thêm ở L6/30/84/90 bị bỏ qua ✓.
  - So §2: Title→lơ lửng 129 f (video ≈ 130 f, ±2), nháy 80 f (video 80), đen 16 f (video 16) ⇒ trong ±1 f với mốc video, trừ thời lượng từng khung bay (xấp xỉ, §2.3).
- **Ảnh** (`ClaudeNotes/cvti/_cache/shots/05/`, 1920×1080, chụp đúng frame bằng `ScreenCapture` từ delegate): `a_title_L000`, `a_title_L010`, `b_fly_L008…L128` (10 ảnh), `c_hover_L129/133/137/141/300`, `d_blink_off_L354`, `d_blink_on_L362`, `e_black_L436`, `f_intro_L449`. Đã mở xem: `a_title_L000`, các ảnh ghép dưới.
  - **Kiểm từng pixel**: 19 ảnh Title (mọi ảnh a/b/c/d) so với ảnh ghép kỳ vọng từ PNG nguồn (`_cache/05/expected.py`: title_base + title_bat_NN @ (184,104) + push_start_key @ (72,136) + glyph 3 dòng) ⇒ **0 pixel lệch** ở cả 19 ảnh (khung dơi nhận ra đúng như log, `d_blink_off` = không PUSH START). `check_pixel_exact` cho `a_title_L000`: 0 pixel ngoài vùng game không đen. `e_black_L436`: 0 pixel khác (0,0,0,255). Ghi chú: ảnh `a_title_L000` ra khung 01 (log nói khung 00 giữ L0–L2) — chụp ở lượt Title được bật từ trong `OnSystemTick`; các ảnh khác đều đúng khung log. Không ảnh hưởng game.
  - **So video** (`_cache/05/cmp_side.png`, `cmp_diff.png`, `cmp_diff_aligned.png`, đã xem; shot L010 vs `ref/01` t1.2 s — cùng khung dơi 03): mọi thành phần trùng; dịch tối ưu theo vùng (px video → NES): logo (+1,0)→(+0.51,0) · ô vuông trên (+1,0) · ô vuông dưới (+1,+2)→(+0.51,+1.17) · PUSH START (+1,+1)→(+0.51,+0.58) · copyright 1/2 (+1,+1) · copyright 3 (+1,+2) · tháp (+1,+1). Lệch giống hệt các phần cố định của `title_base` ở cùng độ cao ⇒ do lệnh ghép (§6), **lệch bố cục tương đối = 0 px NES** (≤ 1 px). Màu mẫu (game/video): tháp (199,75,10)/(206,76,9), cỏ (0,146,0)/(0,149,0), thanh xám 187/189, tím logo (68,0,154)/(65,0,153) ✓.
  - **Đường bay** (`_cache/05/bat_vs_video.png`, đã xem): 15 cặp game | video cùng thời điểm (t = 1.0 + L/59.24): hình dạng + vị trí dơi trùng ở mọi chặng (ngọn tháp → phải, to dần → sà trước tháp → vọt lên trên-trái → lơ lửng), lệch ≤ 1 khung. Lơ lửng L300 vs 6.06 s khác pha vỗ cánh (trong 4 f).
  - `def_strip.png` (đã xem): giữa nháy chữ tắt / chữ bật, dơi đang vỗ cánh; Intro placeholder sau đen.
- Cuối phiên: thoát Play, EditorPrefs `CVTI.LogFlow` = false, scene Main không dirty, không có `Assets/Screenshots/`.

### Routine 06 — 11/10/2026 — DỪNG ở Bước 0 (không làm HUD)
- Điều kiện chặn: `Art_Source/` không có sheet HUD (§3; `Assets/Data/Art/HUD/` chưa tồn tại). Routine 04, 05 đã commit (`5fed9e6`, `01c70ab`); git sạch ngoài nhiễu nền §6.
- Đã tìm thêm: `Downloads/NES - Castlevania - Miscellaneous - Text Font.gif` (mới, 12:43) — chỉ là bảng chữ Famicom, không có thanh máu/tim/khung vũ khí phụ (chi tiết §5 #13). Đã chép vào `Art_Source/_inbox/`.
- Không đụng Unity, không tạo script/prefab/sprite, không dựng HUD từ pixel video (H1/H2). Việc tiếp: người dùng tải sheet HUD đầy đủ → nhập vào `Art_Source/spriters-resource/` + `SOURCES.md` → chạy lại routine 06.

### Routine 07 — 11/10/2026 — DỪNG ở Bước 0 (cổng phụ thuộc chưa đạt)
- Bước 0.2 yêu cầu 05 **và 06** đã commit `*add task "CVTI 0x` và §1 = XONG. Thực tế: 05 ✓ (`01c70ab`); 06 **chưa** — commit mới nhất `292239a` chỉ là "ghi sổ CVTI 06: dừng vì thiếu sheet HUD (dở dang)", §1 dòng 06 = "CHƯA — DỪNG".
- Kiểm lại đầu vào: `Downloads/` không có file mới sau `Text Font.gif` (12:43); `Art_Source/spriters-resource/` vẫn chỉ 3 sheet (title/cutscenes/simon); không có sheet HUD, sheet Stage 1 hay nhạc Prologue/Vampire Killer ⇒ chưa có gì để chạy lại 06.
- Không đụng Unity, không tạo script/prefab/sprite. Ghi chú: `git status` thấy submodule `Assets/TuyenAframework` báo " m" (nội dung trong submodule thay đổi, con trỏ vẫn `4685d33`) — không thuộc cụm, không đụng (H3).
- Việc tiếp: người dùng tải sheet có HUD (dải `SCORE-000000 TIME 0300 STAGE 01`, thanh máu, tim, `P-03`) → chạy lại 06 → chạy lại 07. (Sheet Stage 1 + nhạc không chặn 07: thiếu thì stub nền đen + không phát nhạc như routine đã dặn.)
