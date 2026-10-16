# Chỉ mục timeline + hiệu chuẩn video tham chiếu — Castlevania NES Map 2 (Block 2: Stage 04–06)

> **Routine `cvm2-02` (16/10/2026, Sonnet 5.5) đã đo lại tới từng frame/tick**; bản planner (thô, 15/10) bị thay bằng file này. Mọi số có thể kiểm lại bằng script ở `Tools/cvm2/` (xem §6 "Cách chạy").
> - Ảnh **full frame gốc** (36 PNG, 18 MB): `ClaudeNotes/cvm2/_cache/video/ref/` (chỉ máy này, KHÔNG commit).
> - Ảnh **lấy mẫu về lưới NES 256×240** (hàng ngoài khung video = đen): `ClaudeNotes/cvm2/ref/nes/<tên>_nes.png` (commit). 36 ảnh planner dùng hiệu chuẩn cũ (sx 3.623…; lệch ≤ 0.3 px NES so với §1 mới, không ảnh hưởng so sánh bằng mắt); **10 ảnh Map_2 + 2 ảnh boss mới** (hiệu chuẩn mới) thêm bởi 02. 4 ảnh planner đã đổi tên cho đúng nội dung (§5).
> - **Bảng số theo frame (commit)**: `ClaudeNotes/cvm2/ref/data/` — `map2_hud.csv`, `map2_camera.csv`, `map2_simon.csv`, `map2_events.txt`, `map2_pickups.txt`, `map2_tracks.csv` (+ `map2_track_class.csv`), `map2_candles.txt`, `boss2_hud.csv`, `boss2_simon.csv`, `boss2_medusa.csv`, `map2_audio_cues.txt`, `boss2_audio_cues.txt`, `calib_*.txt`. Bộ đệm lớn (`map2_nes.npy` 1.3 GB, `map2_blobs.csv`, `map2_track_pts.csv`) ở `ClaudeNotes/cvm2/_cache/02/work/` — tạo lại bằng `Tools/cvm2/extract_nes2.py` (~1 phút) + `objects2.py` (~2 phút) + `tracks2.py`.

Video nguồn (chỉ đọc), thư mục `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\`:
- `Map_2.mp4` — **1486×712**, 30 fps, **4:20.47** (7814 frame), AAC 48 kHz. Quay màn hình **tiếp đúng playthrough của `Map_1.mp4`** (mở đầu SCORE 023370, P-03, bản đồ lâu đài STAGE 04). Watermark đỏ góc phải-dưới **nằm ngoài vùng game** (video x 1420–1457, y 638–675). Không có đoạn non-game / lớp phủ YouTube trong video này (quét `hud_ok` 7705/7814; 109 frame không có HUD = **71 frame đen** (6 màn đen §7) + **18 frame nháy rosary** (3:09.2, 3:30.1) + **20 frame sprite đè chữ PLAYER/ENEMY** (thập tự đang bay: f1899–1903, 1963–1968, 2027–2032, 4158–4160) — không có lớp phủ/đoạn non-game).
- `Boss_Map_2_Behaviour.mp4` — **1490×718**, 30 fps, **23.30 s** (699 frame). Trình duyệt YouTube ("Overtime Gamer"): **bảng màu khác Map_2** (gạch tím/magenta); playthrough KHÁC (SCORE 005800, P-01, **Holy Water + III**, hearts 17). Lớp phủ: nút ▶ trắng giữa màn **f0–~f22**; chữ "ko-fi.com/overtimegamer" đè sàn NES x 0–85, y ≈216–227; watermark mờ "OVERTIME GAMER / powered by AND…RPG" đè HUD NES x ≈92–166, y ≈21–38 (không che chữ số).

Quy ước: thời điểm `m:ss.ss` (Map_2) hoặc `ss.s s` (boss). Frame video `f` = round(t × 30). **tick** = 1 frame NES (60.0988 Hz) ⇒ **1 frame video = 2.0033 tick**. Toạ độ NES x 0–255, y 0–239 (HUD y 16–47, sân chơi 48–223). `cx` = camera x trong vùng sheet; `wx = cx + x_màn` (toạ độ thế giới trong vùng); đoạn cuộn qua cửa dùng toạ độ ghép (cột "cxw" của `map2_camera.csv`). **Y** của Simon = hàng MẶT SÀN dưới chân (quy ước CVGP 07 §4b); `y_sec = Y − 32` (hàng trong vùng sheet).

## §1. Hiệu chuẩn video → pixel NES (routine 02 — đo tinh)

Công thức: `x_NES = (x_v − ox) / sx`, `y_NES = (y_v − oy) / sy`; **chỉ số pixel video của tâm pixel NES (x,y) = round(ox + sx·(x+½))** (quy ước `Tools/cvm2/nes2.py`).

| | **Map_2** (1486×712) | **Boss_Map_2_Behaviour** (1490×718) |
|---|---|---|
| sx / sy | **3.621 / 3.101** (± 0.003) | **3.608 / 3.156** (± 0.003) |
| ox / oy | **268.89 / −19.74** (± 0.3 / ± 0.2 px video) | **274.46 / −23.02** (± 0.3 / ± 0.3) |
| sx/sy | **1.1677 ≈ 7/6** (1.1667) | **1.1432 ≈ 8/7** (1.1429) |
| Khớp | **ZNCC xám TB 0.9468** trên **10 frame sạch**, mỗi vùng ≥ 1 (TEA·TEB·HallA×2·HallB×2·HallC×2·Medusa×2), từng frame 0.918–0.968 (cx tinh ±2 mỗi frame); 12 frame kể cả frame có quái/cuộn: 0.915 | **ZNCC gradient TB 0.9550** (7 frame phòng Medusa cx 0; bảng màu khác ⇒ so ĐỘ LỚN CẠNH, không so màu), từng frame 0.921–0.968 |
| Vùng game trong video | x **269–1195** (927 px = 256 cột) · y **6–700** (695 hàng) | x **275–1198** (923 px) · y **9–703** (695 hàng) |
| **Hàng NES nhìn thấy** | **y 8–231 (224 hàng)** — cắt 8 trên + 8 dưới (như Map_1; **planner dự đoán 7–235 là sai**); HUD hàng chữ y 16–47 thấy đủ | **y ≈10–229 (220 hàng)**; đủ 256 cột |
| Ngoài vùng game | viền xám phẳng (luma 15) trên 0–5, dưới 701–711 | viền mờ 2 bên (x < 275, x > 1198), trên/dưới UI |
| Lệch ZNCC khi đổi tham số | sx ±0.003: −0.0004; sy ±0.003: −0.002/−0.0007; tâm x ±0.3 px: −0.0003; tâm y ±0.3 px: −0.003 | sx ±0.003: −0.0015; sy ±0.003: −0.0006/−0.0023; tâm x ±0.3: −0.003; tâm y ±0.3: −0.009/−0.001 |

**Kiểm chéo bằng HUD** (`ref/data/calib_*.txt`, `hud_check2.py`): (a) vạch máu ENEMY 16 vạch ⇒ sx 3.628, ox 268.20; sy 3.0835, oy −18.46 từ 3 hàng chữ (độ rộng chữ blur ±0.5 px ⇒ dọc kém tin; cùng quy luật Map_1 "HUD-only lệch 0.4 %") — tại giữa màn x=128 hai cách khác nhau 0.1 px video; (b) **20 frame ngẫu nhiên (seed 7) đọc HUD: 0 sai SCORE/TIME/STAGE/tim/P/vạch** (`_cache/02/shots/hud_check_20.png`, đã mở xem); (c) 7705/7705 frame có HUD khớp 10 mẫu số Map 1 (Hamming ≤ 8), chỉ 3 frame có `?` do sprite đè chữ số (f2012 TIME `03?2` …).

**Nhịp quay thực** (`dup_frames2.py`; frame lặp = không pixel NES nào đổi > 40): Map_2 **20.9 % frame video là bản lặp ⇒ 23.7 frame game mới/s** (từng 10 s: 16.3–26.6; thấp nhất 4:00–4:10 vì màn tĩnh/tally); chuỗi lặp độ dài 1: 4979, 2: 1081, 3: 67, ≥4: ~70. Video boss 33 % lặp ⇒ 20.0/s. Hệ quả: một mốc đơn lẻ ±1–2 frame video = ±2–4 tick; đo thời lượng bằng hồi quy nhiều frame hoặc đồng hồ TIME. **TIME = 64.0 tick/đv** (6 đoạn: 63.98, 64.06, 64.08, 64.02, 64.01 tick/đv; lệch max 2.4 frame) ⇒ video phát đúng thời gian thực.

**Bảng màu video ↔ sheet** (`fg2.lut()`): video Map_2 khác sheet Level 2 (sheet (168,0,16) ↔ video (108,0,9); (200,76,12) ↔ (167,49,3); (216,40,0) ↔ (172,20,18); (116,116,116) ↔ (85,81,82); (36,24,140) ↔ (6,13,115) …) ⇒ trừ nền cần đổi màu trước (đã làm trong `fg2.py`; không lấy màu tuyệt đối video làm sprite — H1).

**Camera bám Simon** (`camera_track2` + `simon2`, 754 + 1451 frame đang cuộn phải/trái): **Simon luôn ở px = 128 khi camera cuộn (p10 = p90 = 128), mọi hướng, không vùng chết**; camera đứng yên khi Simon ở khoảng giữa mép: đến mép vùng thì dừng (cx 0 / tối đa).

## §2. Vùng sheet Level 2 (`ClaudeNotes/cvm2/inventory/level2_main_hall.json`, ảnh chú thích `level2_main_hall_annot.png`)

| Vùng (tên planner) | Stage (HUD) | rect sheet [x,y,w,h] | Màn | Vào từ (video) | Ra | 8×8 / 16×16 duy nhất |
|---|---|---|---|---|---|---|
| **TowerEntranceA** (TEA) | 04 | [1803, 632, 256, 192] | 1 | bản đồ lâu đài → đen → cx 0 (0:06.23, Simon px 64 Y 208 quay phải) | thang phải-lên cắt mép trên → TEB (đen 0:28.50) | 32 / 22 |
| **TowerEntranceB** (TEB) | 04 | [1547, 425, 512, 192] | 2 | thang: **cx 256** (0:28.87, Simon wx 386 Y 221 đang leo) | **cửa TRÁI** (0:51.80–0:58.73) → Hall A | 42 / 27 |
| **TowerHallA** | 05 | [774, 425, 768, 192] | 3 | sau cửa: **cx 512**, HUD đổi 05 lúc 0:58.73 | thang trái-lên cắt mép trên → Hall B (đen 1:33.00) | 43 / 26 |
| **TowerHallB** | 05 | [774, 218, 768, 192] | 3 | thang: cx 0 (1:33.37, Simon wx 66 Y 221) | **cửa TRÁI** (2:22.33–2:29.40) → Hall C | 56 / 37 |
| **TowerHallC** | 06 | [1, 218, 768, 192] | 3 | sau cửa: cx 512, HUD đổi 06 lúc 2:29.33 | thang phải-lên cắt mép trên → sảnh Medusa (đen 3:17.13) | 44 / 32 · 6144 px teal = 3 ô 32×64 bẫy ép (wx 352/416/480) |
| **Medusa** | 06 | [1, 11, 768, 192] | 3 | thang: cx 512 (3:17.47, Simon wx 674 Y 221) | phòng boss: camera khoá **cx 0 ở 3:44.50** (f6735); tượng Medusa VẼ SẴN (wx 96–127, y 128–175) | 64 / 44 |
| Sprite rời | — | Crusher 8 khung [1+33k, 425, 32, 64] k=0..7, Moving Platform [269, 425, 32, 16] | | | | |

Tổng: 82 tile 8×8 / 69 metatile 16×16 duy nhất. Màu nền khoá: xanh (0,128,0). **Xám (116,116,116) là màu THẬT.** Credit logo [1576,792,224,32] — bỏ. **Hướng đi Map 2 ngược Map 1:** hai cửa cuối stage nằm **mép TRÁI**, camera cuộn sang TRÁI ⇒ lõi (`StageFlow` cửa, `StageRules.GetStageStart`, camera) phải hỗ trợ hướng trái (Map 1 chỉ có cửa phải). **Cả 3 lần đổi tầng (TEA→TEB, A→B, C→Medusa) là thang LÊN-PHẢI cắt mép TRÊN màn: Simon biến mất ở Y≈63 và hiện ở mép DƯỚI (Y 221) của section kế, cùng x ±3 (+256 với TEB vì TEB rộng 512), vẫn đang leo (§7).**

**Vật thể KHÔNG vẽ trên nền sheet** (trừ nền ra là tiền cảnh): nến tường (candle_0/1, 8×14, đôi ngọn lửa đỏ), dơi treo (bat_purple_0: oval viền hồng 12×14), tượng cổ vẫn vẽ sẵn, **khối gạch cam đơn lẻ phá được**, bệ di động, bẫy ép, cánh cửa, item/quái. Danh sách nến: `map2_candles.txt` (đo thô — routine 10 lập chính thức).

## §3. Timeline Map_2 (57 dòng — HUD từng frame + camera + Simon + tiền cảnh; mốc theo frame video, tick = frame × 2.0033)

Suốt Map_2 (trước boss): **PLAYER = 16/16 vạch (Simon KHÔNG hề bị trúng đòn), không rơi hố**; P 03→04 chỉ nhờ SCORE qua 30000 (1:21.37). Nguồn: `ref/data/map2_hud.csv` (đọc từng frame), `map2_events.txt` (78 sự kiện HUD + nghĩa), `map2_camera.csv`, `map2_simon.csv` (phủ **92.9 % frame gameplay**, 6829/7347), `map2_tracks.csv`. Vũ khí phụ trong HUD: rìu + II (tới 0:09.13) → thập tự (tới 2:09.23) → **đồng hồ (stopwatch — KHÔNG phải nước thánh, planner đoán nhầm; khớp mẫu `Item/stopwatch.png`)**. Mã vết `#n` = id trong `map2_tracks.csv`.

| # | Thời gian (frame) | ST | TIME | SCORE | Tim | VK | Khu vực · camera x | Sự kiện |
|---|---|---|---|---|---|---|---|---|
| 1 | 0:00.00–0:05.73 (f0–172) | 04 | 0000 | 023370 | 05 | rìu + II | bản đồ / khác | **Bản đồ lâu đài Block 1** (đuôi Map_1): nền cam; bản đồ đã trượt vào xong (mép trái x=8 từ f≈62); Simon nhỏ hiện f61, đi PHẢI **1.005 px/tick** x 17→241 (f61→172); HUD STAGE 04, TIME 0000, tim 05, P 03, rìu + **II**. |
| 2 | 0:05.77–0:06.20 (f173–186) | – | – | – | – | – | đen | **Màn đen 14 frame ≈ 28 tick** (Map_1 đo 27). |
| 3 | 0:06.23–0:06.87 (f187–206) | 04 | 0400 | 023370 | 05 | rìu + II | TEA · cx 0..0 | **STAGE 04 bắt đầu** — TEA cx 0; Simon đứng yên quay PHẢI tại px 64, mặt sàn Y 208 (13 frame); ENEMY 0→16 vạch; TIME 0400 (giảm lần đầu f216 = 29 frame ≈ 58 tick sau). |
| 4 | 0:06.90–0:09.10 (f207–273) | 04 | 0400 | 023370 | 04 | rìu + II | TEA · cx 0..0 | **Spear Knight #3** (tầng trên trái, wx 141→84, đi trái ≈0.38 px/tick). Simon ném **rìu ×2** (f207 0:06.90, f239 0:07.97; tim 05→04→03). Nến tường wx 80 (y 154) vỡ ≈0:07.7 → **thập tự** rơi xuống sàn (79,200). **0:08.37 +400** Spear Knight chết vì rìu. |
| 5 | 0:09.13–0:11.33 (f274–340) | 04 | 0398 | 023770 | 03 | thập tự | TEA · cx 0..0 | **0:09.13 nhặt thập tự** (boomerang): HUD rìu→thập tự, **mất II**. Nến wx 208 (y 154) vỡ 0:11.07 → tim lớn. |
| 6 | 0:11.37–0:16.67 (f341–500) | 04 | 0395 | 023770 | 08 | thập tự | TEA · cx 0..0 | **0:11.37 tim lớn +5** (03→08, wx 199). Simon đi phải, leo thang phải-lên tới tầng trên TEA. |
| 7 | 0:16.70–0:17.33 (f501–520) | 04 | 0390 | 023770 | 08 | thập tự | TEA · cx 0..0 | 0:15.6–0:16.6 Simon (tầng trên, phải) roi vào **tường phải TEA** (wx 228–252, y 99–133): mảnh gạch tím bay (FX brick_debris). **0:17.0 vương miện mọc từ sàn** tại wx 135, y 206 (cạnh chân thang), nháy màu. |
| 8 | 0:17.37–0:20.00 (f521–600) | 04 | 0390 | 023770 | 08 | thập tự | TEA · cx 0..0 | Simon quay xuống thang trái-xuống → **0:19.77 nhặt vương miện +2000** (wx 124, Y 196). |
| 9 | 0:20.03–0:25.67 (f601–770) | 04 | 0387 | 025770 | 08 | thập tự | TEA · cx 0..0 | **0:24.47 +100** túi đỏ (rơi từ nến trên cao, wx 120→144). **0:24.93 ném thập tự** (tim 08→07), thập tự bay trái trúng **dơi treo** (oval viền hồng, wx 48 y 64) **0:25.93 +200**; thập tự quay về (đường bay y≈49–54, vx −1.7/+0.8 px/tick). |
| 10 | 0:25.70–0:28.47 (f771–854) | 04 | 0382 | 025870 | 07 | thập tự | TEA · cx 0..0 | Simon leo thang PHẢI-lên cuối TEA (0:27.7–0:28.47): wx 103→127, Y 87→63 (0.5 px/tick chéo). |
| 11 | 0:28.50–0:28.83 (f855–865) | – | – | – | – | – | đen | **Màn đen 11 frame ≈ 22 tick** — đổi tầng TEA→TEB (Simon rời mép TRÊN ở wx 127, Y 63; hiện ở mép DƯỚI wx 386 = +259, Y 221; vẫn đang leo cùng chiều thang). |
| 12 | 0:28.87–0:33.33 (f866–1000) | 04 | 0380 | 026070 | 07 | thập tự | TEB · cx 256..256 | **TEB cx 256** (mép phải). Simon tiếp tục leo thang lên-phải (wx 386→438 trong 0:28.9–0:30.6) rồi đi **TRÁI** trên lối trên. **Spear Knight #44** đứng wx 336 y 97 (0:28.87–0:42.9); dơi treo #43 wx 304 y 64. **0:33.40 ném thập tự** (07→06). |
| 13 | 0:33.37–0:36.67 (f1001–1100) | 04 | 0376 | 026070 | 06 | thập tự | TEB · cx 256..256 | **0:34.67 +200** thập tự trúng dơi treo #43 (wx ≈ 304). **0:36.07 +100** túi đỏ. 0:37.50 tim +1. |
| 14 | 0:36.70–0:44.33 (f1101–1330) | 04 | 0373 | 026370 | 06 | thập tự | TEB · cx 76..256 | Camera cuộn TRÁI theo Simon (px khoá 128): cx 256→231 (0:38)→…→0. 0:41.20, 0:43.70 tim nhỏ +1 (nến wx 208/144, trên cao). |
| 15 | 0:44.37–0:46.50 (f1331–1395) | 04 | 0366 | 026770 | 09 | thập tự | TEB · cx 0..76 | **0:44.37 +400** Spear Knight #60 (wx 144–148, y 97) chết vì **roi**. **0:45.83 +700** túi trắng (nến wx 144). |
| 16 | 0:46.53–0:51.33 (f1396–1540) | 04 | 0364 | 027470 | 09 | thập tự | TEB · cx 0..0 | **Bệ di động** (Object platform, 32×8 viền kem): thấy y=113 (mặt trên), wx 32–44 trong 0:46.5–0:48.0 và 0:49.9–0:51.3 (chỉ đo thô — biên/chu kỳ để routine 06). **0:50.20 +100** túi đỏ cạnh bệ. 0:50.4–0:51.7 Simon tới mép TRÁI TEB (px 86→23). |
| 17 | 0:51.80–0:53.90 (f1554–1617) | 04 | 0359 | 027570 | 09 | thập tự | cửa-trái(HallA‖TEB) · cx 641..766 | **CỬA TRÁI #1 — cuộn 1**: camera 768→640 (vùng ghép HallA‖TEB), **1.00 px/tick × 128 tick** (f1554–1617, 64 frame); Simon đứng yên (world x 791), TIME đứng. |
| 18 | 0:53.93–0:56.53 (f1618–1696) | 04 | 0359 | 027570 | 09 | thập tự | cửa-trái(HallA‖TEB) · cx 640..640 | **CỬA TRÁI #1 — dừng ≈158 tick** (79 frame): cánh cửa (door sprite 38×50, x 120–158 màn) hiện f1618, Simon tự đi TRÁI **66 px** 1.00 px/tick (px 148→82, f1630–1663), khuất sau cửa f1637–1649; cửa biến mất f1672; Simon đứng px 80 tới f1696. |
| 19 | 0:56.57–0:58.73 (f1697–1762) | 04 | 0359 | 027570 | 09 | thập tự | cửa-trái(HallA‖TEB) → HallA · cx 512..639 | **CỬA TRÁI #1 — cuộn 2**: 640→512, **1.006 px/tick × 128 tick** (f1697–1760); **HUD STAGE 04→05 ở f1762** (≈1.5 frame sau khi camera dừng); Hall A cx 512 (Simon px 208, wx 720, tầng trên Y 112); TIME giảm lại f1788. |
| 20 | 0:58.77–1:06.33 (f1763–1990) | 05 | 0359 | 027570 | 09 | thập tự | HallA · cx 480..512 | **Hall A** (cx 512→…): Simon đi TRÁI. **Medusa Head** ×5 (#94 #95 #102 #103 #106, 1:01.2–1:08.1) bay ngang **±1.25 px/tick** (sóng dọc), từ cả hai phía. **1:00.53 ném thập tự** (09→08) → **1:02.40 +300** MH chết; 1:01.50, 1:04.33 tim +1. |
| 21 | 1:06.37–1:09.67 (f1991–2090) | 05 | 0352 | 027870 | 10 | thập tự | HallA · cx 390..480 | 1:06.8–1:07.1 thập tự bay đè HUD (nhiễu ô vũ khí). **1:07.07 tim +1**. Nến wx 528/592 vỡ: **dao găm** (item) nằm sàn từ ≈1:10.4 (không nhặt), túi đỏ rơi. **1:08.53 +300** MH chết vì roi; **1:09.40 +100** túi đỏ. |
| 22 | 1:09.70–1:16.67 (f2091–2300) | 05 | 0349 | 028270 | 12 | thập tự | HallA · cx 184..388 | 1:09.10 tim +1, 1:14.57 tim +1; **1:16.10 +100** túi đỏ; Spear Knight #137 đi trái (210→186, 1:16.4–1:17.8). |
| 23 | 1:16.70–1:22.33 (f2301–2470) | 05 | 0342 | 028370 | 13 | thập tự | HallA · cx 178..272 | **1:21.37 +2000 rương báu** (chest mọc lên từ sàn wx ≈ 390–416 từ ≈1:20.0, đỏ→xanh khi mở); đồng thời **P 03→04** (SCORE qua 30000 = +1 mạng). |
| 24 | 1:22.37–1:32.97 (f2471–2789) | 05 | 0337 | 030370 | 13 | thập tự | HallA · cx 0..227 | 1:24–1:31 nến tường wx 272 (y 87) → **tim lớn +5 (1:25.47)**, nến wx 208 → tim +1 (1:27.63), nến wx 144 (y 88) → **tim lớn +5 (1:29.10)**, nến wx 80 → tim +1 (1:31.37). Simon leo thang TRÁI-lên cuối Hall A (wx 79→63, Y 63). |
| 25 | 1:33.00–1:33.33 (f2790–2800) | – | – | – | – | – | đen | **Màn đen 11 frame ≈ 22 tick** — Hall A→Hall B (Simon rời mép trên wx 63 Y 63; hiện mép dưới wx 66 Y 221). |
| 26 | 1:33.37–1:36.67 (f2801–2900) | 05 | 0327 | 030370 | 25 | thập tự | HallB · cx 0..0 | **Hall B cx 0**: Simon leo thang rồi **đi PHẢI** (tầng dưới). **1:36.40 ném thập tự** (25→24). |
| 27 | 1:36.70–1:47.83 (f2901–3235) | 05 | 0324 | 030370 | 24 | thập tự | HallB · cx 0..292 | **Medusa Head ×6** (#164–#195, 1:34.4–1:45.1) bay TRÁI **1.25 px/tick** (sóng dọc). **1:37.80, 1:39.43 +300** (MH chết: roi/thập tự). Nến wx 334 (y 144) vỡ → tim +1 (1:45.70). **Bệ di động**: y=113, wx 256→301 (1:41.2–1:44.3, 88 frame) — đo thô. Camera cx 0→512 (tới 1:54). |
| 28 | 1:47.87–1:54.33 (f3236–3430) | 05 | 0314 | 031370 | 25 | thập tự | HallB · cx 292..512 | **1:47.83 +400** Spear Knight (tầng dưới) chết vì roi. 1:50.10 +100 túi đỏ. **1:54.00 +700** túi trắng. Camera tới cx 512 (mép phải) ≈1:54. |
| 29 | 1:54.37–2:03.33 (f3431–3700) | 05 | 0308 | 032170 | 25 | thập tự | HallB · cx 437..512 | Hall B tầng trên-phải: Spear Knight #211 đi trái (717→652, 1:52.7–1:57.2) chết **1:56.90 +400**; #200 đứng (526–546, y 97, tới 2:00.8) chết **2:00.50 +400**. **Thịt nướng** thấy ở bậc (wx ≈ 632, y≈176) 1:57.3 (HP đầy). 2:02.4–2:02.9 thập tự bay đè HUD. |
| 30 | 2:03.37–2:09.33 (f3701–3880) | 05 | 0299 | 032970 | 25 | thập tự | HallB · cx 402..512 | **2:04.27, 2:07.67 +100** túi đỏ. **2:05.90 ném thập tự** (25→24). 2:07.4 **đồng hồ** (stopwatch) xuất hiện quanh wx 527 (y 82→104, rơi từ nến trên cao) → **2:09.23 nhặt đồng hồ** (HUD thập tự→đồng hồ; mất thập tự). |
| 31 | 2:09.37–2:22.30 (f3881–4269) | 05 | 0294 | 033170 | 24 | đồng hồ | HallB · cx 0..400 | Hall B quay TRÁI tầng trên: nến → tim +1 (2:12.40, 2:16.50, 2:20.27); **MH #295, #307** (2:16.4–2:22.3, +1.26 px/tick); **2:19.60 +300** MH chết vì roi; bệ di động y=113 wx 259–301 (2:11.7–2:18.4, thô). **2:21.0–2:22.3 Simon tới mép TRÁI Hall B** (px 99→30, Y 144). |
| 32 | 2:22.33–2:24.40 (f4270–4332) | 05 | 0281 | 033470 | 27 | đồng hồ | cửa-trái(HallC‖HallB) · cx 642..766 | **CỬA TRÁI #2 — cuộn 1**: 768→640 (HallC‖HallB), **1.003 px/tick, 128 px ≈ 128 tick** (f4270–4332 = 63 frame ≈ 126 tick đếm frame, ±2); Simon đứng yên; TIME đứng. |
| 33 | 2:24.43–2:27.17 (f4333–4415) | 05 | 0281 | 033470 | 27 | đồng hồ | cửa-trái(HallC‖HallB) · cx 640..640 | **CỬA TRÁI #2 — dừng ≈166 tick** (83 frame): cánh cửa hiện f4334–4391; Simon tự đi TRÁI **66 px** 1.01 px/tick (px 149→83, f4351–4383), khuất f4354–4369; đứng px 80 tới f4415. |
| 34 | 2:27.20–2:29.40 (f4416–4482) | 05 | 0281 | 033470 | 27 | đồng hồ | cửa-trái(HallC‖HallB) → HallC · cx 512..638 | **CỬA TRÁI #2 — cuộn 2**: 640→512, **1.002 px/tick, 128 px ≈ 128 tick** (f4416–4478 = 63 frame); **HUD STAGE 05→06 ở f4480** (2:29.33, 1.5 frame sau camera dừng); Hall C cx 512; TIME giảm lại f4507. |
| 35 | 2:29.43–2:33.33 (f4483–4600) | 06 | 0281 | 033470 | 27 | đồng hồ | HallC · cx 512..512 | **STAGE 06 — Hall C cx 512**: Simon px 208 wx 720 Y 144 (đầu bậc), đi TRÁI. **2:30.73 Simon ngồi quất khối gạch** (bậc trên cùng): mảnh gạch tím bay, **thịt nướng** lộ ra (2:31.0, Simon bước qua nhặt khi HP đầy ⇒ HUD không đổi); khối bị phá để lại lỗ. 2:33.00, 2:35.07, 2:37.43 tim +1 (nến). |
| 36 | 2:33.37–2:40.93 (f4601–4828) | 06 | 0277 | 033470 | 28 | đồng hồ | HallC · cx 311..512 | **Bẫy ép gai ×3** (Hall C tầng dưới wx ≈ 368, 432, 496; điểm treo y 113, đáy 119→175; lệch pha — xem §7): thấy 2:33.9–2:49.6 (lượt trái) và 3:04.3–3:16.2 (lượt phải, tầng trên). Simon chờ rồi chạy qua, KHÔNG bị trúng. |
| 37 | 2:40.97–2:44.33 (f4829–4930) | 06 | 0270 | 033470 | 25 | đồng hồ | HallC · cx 200..310 | **2:40.97 dùng đồng hồ** (tim 30→25): bẫy ép + địch **đứng yên ≈ 93 frame ≈ 186 tick**; TIME giữ 0270 tới f4923. |
| 38 | 2:44.37–2:51.67 (f4931–5150) | 06 | 0269 | 033970 | 25 | đồng hồ | HallC · cx 0..200 | Bone Tower ×2 (wx 248 y 96 và wx 312 y 64, 16×32, đứng yên, tím) từ 2:41.4. **2:41.77 +400** túi tím; 2:44.07, 2:45.50 +100 túi đỏ; **Ghost #371** (2:45.8–2:49.1, wx 144→230, 0.31 px/tick) **2:47.40 +300** chết vì roi; **2:51.60 +700** túi trắng. **Vật phẩm III** (triple, khối viền xanh) nằm sàn wx 136 y 170 từ 2:51.8 tới ≥3:08.6 — KHÔNG nhặt (HUD không đổi). |
| 39 | 2:51.70–3:09.17 (f5151–5675) | 06 | 0262 | 035070 | 26 | đồng hồ | HallC · cx 0..174 | Thang TRÁI-lên (cx 0 ≈ 2:52–2:56) rồi tầng trên đi PHẢI: **2:55.63 tim +1; 2:58.30 tim lớn +5**. **3:03.37 +400**, **3:08.43 +400**: Bone Tower chết vì roi (wx 248 và 312). |
| 40 | 3:09.20–3:09.63 (f5676–5689) | – | – | – | – | – | HallC · cx 180..197 | **3:09.2–3:09.63: màn nháy trắng/xám xen kẽ (13 frame ≈ 27 tick) = ROSARY** (SCORE không đổi); chỉ 1 frame/2 sáng. |
| 41 | 3:09.67–3:17.10 (f5690–5913) | 06 | 0245 | 035870 | 32 | đồng hồ | HallC · cx 204..512 | Hall C tầng trên → phải: Ghost #461 (wx 480→546, 3:13.8–3:15.8) **3:15.50 +300** chết vì roi; lên thang phải-lên (wx 671, Y 63). |
| 42 | 3:17.13–3:17.43 (f5914–5923) | – | – | – | – | – | đen | **Màn đen 10 frame ≈ 20 tick** — Hall C→sảnh Medusa (wx 671 Y 63 → hiện wx 674 Y 221, cx 512). |
| 43 | 3:17.47–3:30.10 (f5924–6303) | 06 | 0239 | 036170 | 32 | đồng hồ | Medusa · cx 344..512 | **Sảnh Medusa cx 512**: Simon leo thang rồi đi TRÁI. **Medusa Head ×8** (#471–#503, 3:26.4–3:39.0, +1.0–1.26 px/tick). Camera bắt đầu cuộn trái ≈3:25 (Simon px khoá 128). 3:22.07, 3:24.47, 3:26.47 tim +1. |
| 44 | 3:30.13–3:30.53 (f6304–6316) | 06 | 0227 | 036470 | 36 | đồng hồ | Medusa · cx 389..404 | **3:30.13–3:30.53: ROSARY lần 2** (nháy màn 13 frame); trước đó **3:27.57 +300** MH. |
| 45 | 3:30.57–3:44.67 (f6317–6740) | 06 | 0227 | 036470 | 36 | đồng hồ | Medusa · cx 0..387 | Camera cuộn TRÁI cx 512→0, đạt **cx 0 ở f6735 (3:44.50)** — KHOÁ phòng boss. Tim +1 (3:28.07, 3:32.97, 3:37.53, 3:38.83, 3:40.97), **tim lớn +5 (3:43.00)**; MH #498–#503 tới 3:39. |
| 46 | 3:44.70–3:47.87 (f6741–6836) | 06 | 0214 | 036470 | 45 | đồng hồ | Medusa · cx 0..0 | Simon (px 100→49→61, wx 61, Y 176) đi vào phòng boss; tượng Medusa (wx 96–127, y 128–175) trên nền; **Medusa đầu xanh hiện f6821 (3:47.37)**. |
| 47 | 3:47.90–3:55.77 (f6837–7073) | 06 | 0211 | 036470 | 40 | đồng hồ | Medusa · cx 0..0 | **TRẬN QUEEN MEDUSA** (xem §4): **3:47.90 dùng đồng hồ (−5 tim) ×3** (f6837, f6928, f7031): Medusa + rắn đứng yên ≈186–196 tick/lần; Simon (wx 56–61, Y 176) quất liên tục (1 vạch ENEMY/≈15 frame = 30 tick): E 16→0 từ 3:48.30 tới 3:55.77; **3:48.20 +100** rắn chết; **3:55.63 +3000** (f7069) rồi E=0 (f7073). |
| 48 | 3:55.80–3:57.90 (f7074–7137) | 06 | 0211 | 039570 | 30 | đồng hồ | Medusa · cx 0..0 | Quả cầu + đi nhặt: Simon px 61→113 (3:57.17–3:58.0); **TIME 0211→0210 ở f7137 (3:57.9)** = TIME hết đứng. |
| 49 | 3:57.93–4:02.53 (f7138–7276) | 06 | 0210 | 039570 | 30 | đồng hồ | Medusa · cx 0..0 | Chờ nhạc Stage Clear ≈ **136 frame ≈ 272 tick** (f7141–7276) — TIME giữ 0210. |
| 50 | 4:02.57–4:06.00 (f7277–7380) | 06 | 0202 | 039650 | 30 | đồng hồ | Medusa · cx 0..0 | **TALLY TIME**: 0210→0000 **1 đv/tick** (f7277–7380, 104 frame ≈ 208 tick), SCORE +10/đv → **+2100** (039570→041670). |
| 51 | 4:06.03–4:06.90 (f7381–7407) | 06 | 0000 | 041670 | 30 | đồng hồ | Medusa · cx 0..0 | Nghỉ ≈ 28 frame ≈ 56 tick. |
| 52 | 4:06.93–4:10.87 (f7408–7526) | 06 | 0000 | 041770 | 29 | đồng hồ | Medusa · cx 0..0 | **TALLY TIM**: 30 tim → 0, **1 tim/≈8 tick** (f7408→7526), +100/tim → +3000 ⇒ **SCORE 044670** (= 39570 + 2100 + 3000). |
| 53 | 4:10.90–4:12.80 (f7527–7584) | 06 | 0000 | 044670 | 00 | đồng hồ | Medusa · cx 0..0 | Chờ ≈ 59 frame ≈ 118 tick. |
| 54 | 4:12.83–4:13.17 (f7585–7595) | – | – | – | – | – | đen | **Màn đen 11 frame ≈ 22 tick** (4:12.83–4:13.17). |
| 55 | 4:13.20–4:19.70 (f7596–7791) | 07 | 0000 | 044670 | 05 | đồng hồ | bản đồ / khác | **Bản đồ lâu đài BLOCK 2**: HUD **STAGE 07**, tim 05, TIME 0000, P 04, ENEMY rỗng. Bản đồ **trượt vào từ phải** (mép trái 251→8 px) **1.50 px/tick, f7599–7680 (162 tick)**; Simon nhỏ xuất hiện f7681 (x 18), đi PHẢI **1.004 px/tick** tới x 241 (f7791); marker hồng ở ô kế (≈ x 175, y 115). |
| 56 | 4:19.73–4:20.17 (f7792–7805) | – | – | – | – | – | đen | **Màn đen 14 frame ≈ 28 tick** (4:19.73–4:20.17). |
| 57 | 4:20.20–4:20.43 (f7806–7813) | 07 | 0500 | 044670 | 05 | đồng hồ | bản đồ / khác | **STAGE 07 bắt đầu** (Block 3: nền xanh xám + rêu, tượng) — **TIME 0500**, tim 05, P 04; ENEMY 0→16. Video hết ở 4:20.47 (8 frame). |

Điểm số quan sát (video ↔ wiki **khớp hết**, H1 không mâu thuẫn): **Vampire Bat (treo) +200 · Medusa Head +300 · Ghost +300 · Spear Knight +400 · Bone Tower +400 · rắn Medusa +100 · Queen Medusa +3000**; túi tiền đỏ +100, tím +400, trắng +700; vương miện +2000, rương +2000. Tim nhỏ +1, tim lớn +5; thập tự/rìu −1 tim; **đồng hồ −5 tim** (HUD trừ ngay tick bấm). Tally: TIME 1 đv/tick ×10 điểm; tim 1/8 tick ×100.

### §3b. Kẻ địch theo vết (chưa phải số đo cuối — routine 08 đo lại; dưới đây là nguồn + sơ bộ)

| Loại (khớp sprite + màu + điểm) | Section · vị trí (wx, y_màn) | Xuất hiện (thời điểm / số lượng) | Hành vi quan sát | Chết lúc nào / vì gì |
|---|---|---|---|---|
| **Spear Knight** (tím, 16×31–32) | TEA: tầng trên-trái (141,97)→(84,100) · TEB: (336,97) đứng 14 s, (144,97) · Hall A: (210,97)→(186,97) · Hall B: (526–546,97) đứng, (717,97)→(652,96) đi TRÁI | 0:06.2 (#3), 0:28.9 (#44), 0:41.5 (#60), 1:16.4 (#137), 1:47.0 (#200), 1:52.7 (#211); + 1 con tầng dưới Hall B (+400 1:47.83) | đi chậm ≈ **0.38 px/tick** (2 vết), phần lớn **đứng yên** tới khi Simon lại gần | **0:08.37 rìu**; **0:44.37, 1:47.83, 1:56.90, 2:00.50 roi** (+400) — 2 nhát roi cấp 3 không thấy (1 chu kỳ roi); 2 con (#44, #137) không chết trong video (rời khung hình) |
| **Dơi treo** (bat_purple_0, oval hồng 12×14, đứng yên) | TEA (48,64) · TEB (304,64) | đã treo sẵn từ đầu section (0:06.2; 0:28.9) | đứng treo (không thấy lao xuống vì bị hạ trước) | **0:25.93, 0:34.67 thập tự** (+200) |
| **Medusa Head** (xanh, 14–15×15, 80–100 px) | Hall A ×5 (1:01.2–1:08.1, #94 #95 #102 #103 #106) · Hall B ×6 (1:34.4–1:45.1, #164 #169 #174 #179 #189 #195) + ×2 (2:16.4 #295, 2:20.7 #307) · sảnh Medusa ×8 (3:26.4–3:39.0, #471 #473 #476 #495 #498 #501 #503…) | ≈ mỗi 1.0–2.5 s trong 3 cửa sổ dày | bay ngang **|vx| = 1.24–1.27 px/tick** (14 vết; 0.92–1.1 ở vết ngắn), vy dao động (sóng dọc ±30 px, vy trung bình −0.3…+1.9); hướng: Hall A bay PHẢI (+1.26) khi Simon đi TRÁI; Hall B bay TRÁI (−1.25) khi Simon đi PHẢI ⇒ **sinh phía Simon đang QUAY MẶT hướng tới** (wiki ✓) | **+300 ×6** (1:02.40, 1:08.53, 1:37.80, 1:39.43, 2:19.60, 3:27.57): 3 roi + 2 thập tự + 1 chưa phân biệt |
| **Bone Tower** (tím 16×32, đứng yên) | Hall C: **(248,96)** và **(312,64)** (2 tháp) | thấy từ 2:41.4 (khi camera tới) | đứng yên; vết con nhỏ (cầu lửa 6×7, `skeledragon_fireball` 0.4–0.7) quanh tháp (2:50–2:56) | **3:03.37, 3:08.43 roi** (+400 mỗi tháp) |
| **Ghost** (tím nhạt/trắng 12–16, 55–159 px) | Hall C: #371 (144,88)→(230,120) · #461 (480,80)→(546,70) | 2:45.8 và 3:13.8 | bay chậm **0.31–0.58 px/tick**, đường chéo; bám Simon | **2:47.40, 3:15.50 roi** (+300) |
| **Rắn Medusa** (xanh, 16×8) | phòng boss (§4) | 3:47.4 (cùng lúc Medusa) | trườn trên sàn | **3:48.20 roi** (+100) |
| **Queen Medusa** | phòng boss cx 0 | 3:47.37 | §4 | **3:55.63 +3000** |

Dơi bay (Vampire Bat thật) và các loại Block 3+ **không xuất hiện** trong Map_2.

### §3c. Vật phẩm / nến / bí mật (đối chiếu planner)

| Mục | Quan sát từ video | Ghi chú |
|---|---|---|
| Nến tường đôi (candle_0/1 8×14) | tường TEA/TEB/Hall A/B/C, **2 độ cao: y 56–59 (trên) và y 87–88/144–154 (thấp)**, cách đều 64 px (wx 80,144,208,272,336,400,464,528,592,656,720) | nhiều nến chỉ là nền (không phá); bị roi/thập tự ⇒ flame hồng + vật rơi (`map2_candles.txt` thô — routine 10 lập chính thức) |
| Vương miện (+2000) | **mọc từ sàn TEA wx 135, y 206 ở 0:17.0** sau khi Simon quất tường phải (wx 228–252, y 99–133, brick_debris 0:15.6–0:16.6); nháy màu; nhặt 0:19.77 (wx 124) | trả lời "phá khối nào trước": **khối tường phải TEA (tầng trên)** |
| Rương báu (+2000) | Hall A, mọc từ sàn wx ≈ 390–416 từ ≈ 1:20.0 (đỏ → xanh khi mở); nhặt 1:21.37 | Simon đứng ở wx 388 lúc nhặt; điều kiện "2 khối cao" của wiki chưa kiểm — routine 10 |
| Thịt nướng #1 | **Hall C đầu stage**: Simon ngồi quất khối gạch bậc trên cùng 2:30.73 → khối vỡ → thịt nướng lộ 2:31.0; Simon nhặt (HP đầy ⇒ HUD không đổi) | khớp wiki #1; khối để lại lỗ |
| Thịt nướng #2 | Hall B thấy ở bậc thấp (wx ≈ 632, y≈176) 1:57.3 (đã có sẵn khi Simon tới; planner ghi 1:56) | không đo được nguồn |
| **Dao găm** 1:08–1:12 | **item rơi từ nến Hall A (nến trên cao bị phá ≈1:10.3), nằm sàn 1:11.3+; Simon KHÔNG nhặt** (vũ khí vẫn thập tự) — không phải dao ném | planner nghi "Simon ném hay item?" → item |
| Đồng hồ (stopwatch) | rơi từ nến trên cao Hall B ≈ wx 527 (2:07.4 →), nhặt 2:09.23 | thay thập tự; dùng **×4** (2:40.97 Hall C; 3:47.90, 3:50.93, 3:54.37 phòng boss) |
| **III (triple)** Hall C | khối viền xanh nằm sàn wx 136 y 170 từ 2:51.8 (khi Simon tới) tới ≥ 3:08.6; **không nhặt**; không thấy nguồn rơi (xuất hiện trước khi vào khung) | planner "III rơi 2:50" = vật phẩm có sẵn |
| Rosary | **2 lần nhặt**: 3:09.2–3:09.63 (Hall C tầng trên) và 3:30.13–3:30.53 (sảnh Medusa): nháy màn xám/trắng xen kẽ 13 frame ≈ 27 tick; SCORE không đổi | đây là "other ngắn 3:09.2–3:09.6" của planner |
| Tim | +1 ×27, **+5 ×5** (0:11.37, 1:25.47, 1:29.10, 2:58.30, 3:43.00) — xem `map2_events.txt` | nến tường rơi tim; tim nhỏ rơi lắc lư |
| Túi tiền | đỏ +100 ×10, tím +400 ×1 (2:41.77), trắng +700 ×3 (0:45.83, 1:54.00, 2:51.60) | rơi từ nến trên cao |
| 1UP | không có item 1UP; P 03→04 bằng SCORE ≥ 30000 | ✓ luật CVGP |

### §3d. Cơ chế quan sát được (không phải sự kiện đơn)

- **Bệ di động** (Object platform 32×8 viền kem; bbox 32×8, y mặt trên **113**): TEB wx 32–44 (0:46.5–0:48.0, 0:49.9–0:51.3); Hall B wx 256→301 trong 1:41.2–1:44.3 (88 frame), 393–397 (1:48.2), 284–300 (2:11.7–2:12.2), 274–291 (2:15.0–2:15.6), 259–287 (2:17.2–2:18.4). **Chỉ đo thô** — tốc độ, biên và chu kỳ để routine 06 (cần khớp mẫu mọi frame).
- **Bẫy ép gai ×3** (Hall C): wx 368 / 432 / 496, điểm treo y 113, đáy block **119 (thu hết) … 175 (hạ hết)** (63 px, 8 nấc 8 px); **chu kỳ KHÁC nhau**: wx 496 thu hết mỗi **64 frame ≈ 128 tick** (f4652, 4716, 4780), wx 432 mỗi **40 frame ≈ 80 tick** (f4699, 4738, 4779, 4819); wx 368 chưa đủ vòng (f4782). Simon qua bằng cách chờ khi trần thu — **không trúng lần nào**. Hai lượt: tầng dưới 2:33.9–2:49.6 (trái), tầng trên 3:04.3–3:16.2 (phải).
- **Đồng hồ**: bẫy + quái + Medusa **đứng yên ≈ 93–98 frame ≈ 186–196 tick** mỗi lần (Hall C 2:40.97 → bẫy đứng tới f4922; phòng boss 3 lần liên tiếp mỗi lần ≈ 91–103 frame giữa các lần bấm) — **wiki ghi 5 s (300 tick) ⇒ video ngắn hơn (≈ 3.1–3.3 s); ghi `CVM2_MEMORY §6`**. TIME: trong lúc đứng KHÔNG giảm, tới khi hết đứng mới giảm tiếp (Hall C: 0270 giữ 96 frame; phòng boss: 0211 giữ f6816→7137 qua 3 lần bấm).
- **Cuộn camera sát Simon**: Simon px = 128 (mọi hướng). Hall B (2:22): camera cx 0 + Simon tới mép trái (px 30) ⇒ cửa trái.
- **Sau chuyển tầng bằng thang, TIME giảm lần kế cách ĐẦU màn đen 40–41 frame (≈ 80 tick) — 3/3 lần (855→895, 2790→2831, 5914→5955), bất kể pha đồng hồ.**

## §4. Video boss (`Boss_Map_2_Behaviour.mp4`, đo từng frame — `ref/data/boss2_hud.csv`, `boss2_simon.csv`, `boss2_medusa.csv`)

HUD đầu video: SCORE 005800, TIME 0083, STAGE 06, **hearts 18→17 (f30)**, P-01, PLAYER **10** vạch (đã bị đánh trước), ENEMY 16, **Holy Water + III** (icon "III" đỏ). Simon quay trái/phải đứng bên TRÁI phòng (px 33–82); **hearts 17→4**: mỗi ném nước thánh −1 tim (III không tăng chi phí): f126 (4.20 s), 141, 156, 191 (6.37), 219 (7.30), 230, 243, 388 (12.93), 400, 449, 465 (15.5), 477 (15.9) …; lửa nước thánh giữ chân/gây sát thương.

| t (f) | Sự kiện (Medusa tâm bbox x,y; Simon px,Y) |
|---|---|
| 0–0.7 (f0–22) | lớp phủ ▶ YouTube; tượng Medusa (đầu trắng + thân đỏ-trắng, wx 101–121 × y 136–158 phần thân) giữa phòng — Simon (74→82,176) đứng/đi nhẹ quay phải. (blob 114–141 × 105–131 trong f0–~22 = nút ▶ YouTube, không phải tượng) |
| **2.13 (f64)** | tia hồng ở chân tượng (f64–66) |
| **2.27 (f68)** | **Medusa thức: đầu xanh + tóc rắn (42×47) bay ra khỏi tượng**, tâm (116,152); thân tượng (101–121 × 136–158) ở lại bệ SUỐT trận |
| 2.3–3.1 | bay **sang TRÁI sát sàn, đường sóng thấp** (x 116→68, y 148–158) ≈ **1.0 px/tick** về phía Simon; Simon (82→39) nhảy lên tránh (jumpduck, đỉnh Y 144) rồi đáp (35,176) |
| **3.53 (f106) hit #1** | Medusa đè Simon ở sát tường trái: Simon `hurt_0` (33,176)→(23,162), **PLAYER 10→9 (f107)→8 (f111)** (2 vạch, cách nhau ~4 frame = 8 tick); knock-back chỉ 10 px vì tường; Y 176→162 (−14) trong 0.2 s rồi rơi (f122 Y 170) |
| 4.5–6.4 | Medusa lơ lửng thấp x 47–55, y 144–155, **màu nhấp nháy** (§ dưới); Simon ném nước thánh (lửa trên sàn), E 16→**15 (f149, 4.97 s)**, 14 (5.5), 13 (5.9), 12 (6.03), 11 (6.33), 10 (7.07) |
| **6.67 (f200) hit #2** | Simon (23,176) sát tường: PLAYER 8→7 (f200)→6 (f206); `death_0`/`hurt_0` Y 172→162 (−10…−14) |
| 7.3–10.4 | Medusa bay PHẢI: x 49→68 (7.8)→88 (8.3)→114 (8.8)→141 (9.3)→161.5 (9.8) **đứng 10.0–10.4 s** tại (161.5,147) rồi quay lại; Simon (27→63) tiến theo, E giữ 10 |
| 10.8–11.7 | Medusa x 158→144→147 (y 148–156); rắn xanh hiện/trườn trên sàn (nhiều lần 3.5, 11–12.8 s) |
| **11.73 (f352) hit #3** | **rắn** chạm chân Simon (102,176) khi Simon ở px 105 quay phải: `hurt_0`: **PLAYER 6→5 (f352)→4 (f358)**; **knock-back ngược hướng mặt (sang TRÁI): x 102→71 trong f352–370 (18 frame ≈ 36 tick) ≈ 31 px**, **Y 176→170→165→162 (đỉnh −14 px ở f358–362, giữ 3 frame) rồi 163,167,166 (hạ), chạm đất ≈ f372**; sau đó nháy bất tử (Simon gần như không thấy 12.4–13.0 s) — **số cho `cvgp-fix-03`** (khác Map 1: 12 px/cao 15/36 tick: ở đây không bị tường chặn) |
| 12.9–16.4 | Medusa (95–107, 162–165) lơ lửng thấp; tim 9→4; E 10→9 (13.10)→8 (13.63)→7 (14.07)→6 (14.20)→5 (14.53)→4 (14.73)→3 (14.97)→2 (15.27)→1 (15.80) — **1 vạch ≈ 15 frame (30 tick) khi ở trong lửa** |
| **16.37 (f491)** | **E=0; SCORE 005800→008800 (+3000)**; Medusa biến mất, **lửa chết** (blob đỏ 69–120 px tại (111–126,150–178), 16.3–17.9 s; ở Map_2 không thấy 6 ngọn "bossdeath" rõ — blob lửa) |
| **18.87 (f566)** | **quả cầu hiện ở (127,128)** (122–132 × 122–134), **sau chết +75 frame = 2.5 s = 150 tick** (như Map 1) |
| 19.0–19.27 | rơi: y_trên 122 → 132 (19.00) → 157 (19.13) → **162 (19.27, chạm sàn)** ≈ 13 frame ≈ 26 tick, gia tốc |
| 19.27–20.6 | quả cầu nằm (122–133 × 162–174, đổi màu); Simon đi tới (px 47→111); **nhặt ≈ 20.6–20.7 s (f620–622)** |
| 20.83–22.30 | **PLAYER 4→16** (+1 vạch/≈4 frame ≈ 7.4 tick; 12 vạch trong 1.47 s); TIME giữ 0064 (đã dừng từ khi nhặt/chết); video hết 23.30 s |

**Đổi màu Medusa (nháy)**: bảng màu đầu xanh ↔ tím đổi **mỗi ≈ 4.6 frame video ≈ 9 tick** (chu kỳ 18 tick ≈ 3.3 Hz; 51 pha trong 22 s) khi bay bình thường; khi bị đánh/nằm trong lửa giữ **tím liên tục** (f135–196 = 61 frame = 2.0 s; f200–225). Nguồn: `boss2_medusa.csv` cột `state` (B/P).

**Phòng boss**: camera cx 0 khoá; sàn Y 176 (mặt gạch); Simon trong Map_2 đứng px 49–61 (wx 61) suốt trận (dùng đồng hồ + quất liên tục); Medusa ở Map_2 **xuất hiện lúc 3:47.37 (f6821) — 2.87 s sau khi camera khoá (3:44.50)** và Simon đứng ở px ≈ 61–100.

## §5. Ảnh tham chiếu (`ref/nes/` + `_cache/video/ref/`)

Routine 02 đã mở xem (2 bảng ghép `_cache/02/shots/refs_a.png`, `refs_b.png`) + đối chiếu HUD/camera cho cả 36 ảnh planner: **32 đúng mô tả**, **4 đã đổi tên (`git mv`)**: `m2_00m06_0s_stage04_bat_dau_tower_entrance_a` → **`m2_00m06_0s_man_den_truoc_stage04`** (6.0 s rơi vào màn đen f173–186; STAGE 04 bắt đầu 6.23 s) · `m2_00m58_0s_stage05_tower_hall_a_cx512` → **`m2_00m58_0s_cua_trai_cuon_2_sap_vao_hall_a_stage04`** (0:58.0: HUD vẫn STAGE 04, camera 553→512; STAGE 05 hiện 0:58.73) · `m2_02m50_0s_vat_pham_iii_hall_c` → **`m2_02m50_0s_ghost_bone_tower_hall_c_chua_co_iii`** (item III xuất hiện 2:51.8, lúc 2:50.0 chưa có). · `m2_04m02_0s_tally_cong_diem` → **`m2_04m02_0s_sap_tally_time_0210_cho_dem`** (4:02.0 TIME còn 0210; đếm TIME bắt đầu 4:02.57).

Map_2 (28 cũ, tên mới theo trên) · **mới (hiệu chuẩn §1)**: `m2_00m06_4s_stage04_bat_dau_tea_cx0` · `m2_00m17_3s_vuong_mien_moc_tu_san_tea` · `m2_00m54_0s_cua_trai_cua_hien_ra_simon_di_vao` · `m2_01m21_1s_ruong_bau_moc_len_hall_a` · `m2_02m53_3s_vat_pham_iii_hall_c` · `m2_03m09_2s_rosary_nhay_man_hall_c` · `m2_03m46_0s_phong_boss_khoa_cx0` · `m2_03m48_5s_medusa_dong_bang_dong_ho` · `m2_04m14_7s_ban_do_lau_dai_block2_truot_vao` · `m2_04m20_2s_stage07_time0500_frame_dau`.
Boss (8 cũ + 2 mới): `b2_00_0s_tuong_medusa_simon_vao` · `b2_02_5s_medusa_thoat_tuong` · `b2_03_5s_ran_tren_san` · `b2_06_0s_lua_nuoc_thanh` · `b2_12_5s_ran_va_medusa_bay` · `b2_16_5s_medusa_chet_lua` · `b2_19_0s_qua_cau_roi` · `b2_20_5s_nhat_qua_cau` · mới: `b2_02_3s_medusa_thuc_dau_xanh_tren_tuong` · `b2_04_0s_simon_trung_don_hurt`.
Ảnh xem thêm (chỉ máy này, `_cache/02/shots/`): `hud_check_20.png`, `simon_fit_check.png`, `fg_test.png`, `ev/ev_00..06.png` (78 bộ ba frame quanh mỗi sự kiện HUD), `tr/tr_00..08.png` (thumbnail 340 vết), `door1.png`, `crown.png`, `flash1.png`, `hallc_start.png`, `castle1/2.png`, `boss_wake.png`, `boss_hit3.png`, `sub_icons.png`, `newrefs.png`.

## §6. Số đo gộp + cách chạy lại

| Đại lượng | Giá trị | Cách đo / sai số |
|---|---|---|
| Đi (Simon) | **1.00 px/tick** (cả trên bản đồ lâu đài: 1.005 / 1.004) | hồi quy world_x(t) |
| Cuộn qua cửa trái | **1.00 px/tick** (0.994–1.006) × 128 px ≈ 128 tick, 2 đoạn/cửa, 2 cửa | `camera_track2` ghép vùng; Map 1 ghi 0.90–0.91 px/tick (khác — mỗi đoạn Map_1 140 tick; **Map_2 ngắn hơn ~12 tick**) |
| Dừng giữa cửa | cửa 1 **≈158 tick**, cửa 2 **≈166 tick** (±4) | giao đường hồi quy 2 đoạn cuộn với cx 640 |
| Simon tự đi qua cửa | **66 px** (148→82) 1.00 px/tick ≈ 66 tick, bắt đầu 13–18 frame (26–36 tick) sau khi camera dừng | `simon_fit` |
| Màn đen đổi tầng / vào stage | **22 / 22 / 20 tick** (11/11/10 frame) đổi tầng; **28 tick** (14 frame) bản đồ→Stage 04 và bản đồ B2→Stage 07; **22 tick** (11 frame) tally→bản đồ B2 | đếm frame đen ±2 |
| Camera bản đồ lâu đài B2 | trượt vào **1.50 px/tick**, 243 px = **162 tick**; Simon nhỏ xuất hiện 0.2 s sau, đi 1.004 px/tick, x 18→241 | f7598–7680; Simon cream-blob |
| TIME | **64.0 tick/đv** | 6 đoạn (63.98–64.08) |
| Tally | chờ **≈272–280 tick** sau chạm cầu → TIME **1 đv/tick** (+10) → nghỉ **≈56 tick** → tim **1/≈8 tick** (+100) → chờ **≈118 tick** → đen **22 tick** | f7141→7584 |
| Medusa Head | **|vx| 1.25 px/tick** (14 vết), điểm 300 | `tracks2` |
| Spear Knight | đi ≈ **0.38 px/tick**, điểm 400 | 2 vết, thô |
| Ghost | **0.31–0.58 px/tick**, điểm 300 | 2 vết, thô |
| Thập tự bay | **|vx| 1.7–2.0 px/tick** (đi), 0.8–1.9 (về) ở y≈49–54 | 4 vết, thô |
| Đồng hồ | đứng **186–196 tick** | quái/bẫy |
| Bẫy ép gai | thu→hạ 8 nấc × 8 px; chu kỳ **128 tick (wx 496), 80 tick (wx 432)** | f4625–4828 |
| Knock-back (boss, hit #3) | **31 px ngược hướng mặt trong ≈36 tick; đỉnh −14 px; nháy sau đó** | `simon_boss2` (không có tường chặn) |
| Medusa nháy màu | đổi mỗi **≈9 tick** (xanh↔tím) | `boss2_medusa.csv` |
| Quả cầu | xuất hiện **+150 tick** sau khi boss chết, y 122→162 trong ≈26 tick | video boss |
| PLAYER hồi sau cầu | **+1 vạch/≈7.4 tick** | video boss |

**Cách chạy lại** (từ gốc repo; `python -P -E`, Pillow/numpy; ffmpeg `C:\ffmpeg\bin\ffmpeg.exe`; chữ Việt có dấu in ra console cp1252 lỗi ⇒ dùng `PYTHONIOENCODING=utf-8` hoặc `-X utf8`):
`calib_fine.py map2 <t:section:cx> …` (hiệu chuẩn tinh; `BOX=0|1`) · `calib_boss.py <t…>` (boss, gradient) · `extract_nes2.py map2|boss2` (bộ đệm lưới NES) · `dup_frames2.py` · `hud_read2.py map2|boss2 [ref]` → `*_hud.csv` · `hud_events2.py map2` (sự kiện thô) · `hud_check2.py map2 out.png 20 7` (kiểm mắt) · `camera_track2.py map2` → `*_camera.csv` · `objects2.py map2` (blob tiền cảnh; `fg2.py` = trừ nền sheet + LUT màu) · `simon2.py map2` (khớp mẫu Simon, 10 tiến trình, ~3.5 phút) · `tracks2.py` → `classify2.py` (nhãn sprite IoU) · `track_sheet2.py` · `event_triplets2.py` · `pickups2.py` · `candles2.py` · `events_annot2.py` (nghĩa sự kiện) · `platform2.py` (bệ di động — thô) · `boss2_blobs.py` + `simon_boss2.py` (boss) · `audio_cues2.py` · `frames_sheet2.py` (ghép frame để xem) · `ref_images2.py` (xuất ảnh lưới NES).
**Độ phủ**: Simon khớp 6829/7347 frame gameplay (**92.9 %**; TEA/TEB/Hall A/B/C = **100 %**, cửa ghép 92–94 %, Medusa 71 %: thiếu 4:01.7–4:12.8 tư thế thắng sau cầu + vài đoạn Simon bị lửa/Medusa che); camera 100 % frame (7347 game + 8 vùng đen/nháy + bản đồ).

## §7. Chuyển cảnh theo tick (mốc đo trên frame video; tick = frame × 2.0033; sai số đơn ±2–3 tick, thời lượng ≥ 20 frame ±1 %)

| # | Mốc | Frame đầu–cuối | Thời điểm | Độ dài | Trước → Sau |
|---|---|---|---|---|---|
| 1 | Bản đồ lâu đài B1 → đen → **Stage 04** | đen f173–186; Stage f187 | 0:05.77–0:06.20; Stage 0:06.23 | đen **14 frame = 28.0 tick** (Map 1: 27) | Simon bản đồ x 241 (phải) → TEA cx 0, Simon **px 64, Y 208, quay phải, đứng yên 13 frame**; TIME 0400, **giảm lần đầu f216 (+29 frame = 58 tick)**; ENEMY 0→16 lúc f187 |
| 2 | **TEA → TEB** (thang lên-phải cắt mép trên) | đen f855–865; TEB f866 | 0:28.50–0:28.83 | **11 frame = 22.0 tick** | TEA cx 0, Simon wx 127, Y 63 `stairs_up` → TEB **cx 256**, Simon px 130 (wx **386** = +259), Y **221** (mép dưới), quay PHẢI, **tiếp tục leo** (cùng nhịp walk/stairs) tới wx ≈ 438 (Y 167) rồi lên lối trên đi TRÁI; TIME giảm lần kế cách đầu đen **40 frame** (f895) |
| 3 | **CỬA TRÁI #1** (TEB tầng trên trái → Hall A) | cuộn 1 f1554–1617 · dừng f1618–1696 · cuộn 2 f1697–1760 | 0:51.80 → 0:58.67 (tổng **207 frame ≈ 415 tick = 6.9 s**) | cuộn 1 **128 tick** · dừng **≈158 tick** · cuộn 2 **128 tick** | Simon tới mép trái TEB (px 23, wx 23, tầng trên Y 112). Cuộn 1: camera 768→640, Simon **đứng yên** (wx 791 trong toạ độ ghép). Dừng: **cánh cửa hiện f1618–1671** (38×50, x 120–158), Simon đi TRÁI 66 px (px 148→82) f1630–1663, **khuất sau cửa f1637–1649**, đứng px 80; cửa biến mất f1672 (**25 frame = 50 tick trước cuộn 2**). Cuộn 2: 640→512, Simon đứng (wx 720). **STAGE 04→05 ở f1762** (2 frame sau camera dừng); **TIME 0359 đứng từ f1533 tới f1788** (giảm lại +26 frame sau STAGE) |
| 4 | **Hall A → Hall B** | đen f2790–2800; Hall B f2801 | 1:33.00–1:33.33 | **11 frame = 22.0 tick** | Hall A cx 0, Simon wx 63 Y 63 → Hall B **cx 0**, wx 66 Y 221 (+3), leo rồi đi PHẢI |
| 5 | **CỬA TRÁI #2** (Hall B → Hall C) | cuộn 1 f4270–4332 · dừng f4333–4415 · cuộn 2 f4416–4478 | 2:22.33 → 2:29.27 (**209 frame ≈ 419 tick**) | 128 · **≈166** · 128 tick | Hall B cx 0, Simon px 30 (Y 144, tầng thấp hơn cửa 1) → như #3: cánh cửa f4334–4391 (58 frame), Simon bắt đầu đi f4351 (18 frame = 36 tick sau dừng), khuất f4354–4369, đứng px 80; **STAGE 05→06 ở f4480 (2:29.33)**; TIME 0281 đứng f4267→f4507 |
| 6 | **Hall C → sảnh Medusa** | đen f5914–5923; Medusa f5924 | 3:17.13–3:17.43 | **10 frame = 20.0 tick** | Hall C cx 512, Simon wx 671 Y 63 → Medusa **cx 512**, wx 674 Y 221 (+3), leo thang rồi đi TRÁI |
| 7 | **Khoá camera phòng boss** | f6735 | 3:44.50 | camera cuộn trái 512→0 từ ≈3:25 (Simon px 128 mọi lúc) | cx **0 từ 3:44.50 và giữ** tới hết video; Simon px 100→49 (≈ 3:45–3:46) rồi đứng ≈ 61 |
| 8 | Medusa thức (video Map_2) | f6821 | 3:47.37 | +**2.87 s sau khi camera khoá** | đầu xanh trên tượng (111,147), Simon wx 61; **+0.53 s sau đó Simon bấm đồng hồ** |
| 9 | **Medusa chết → quả cầu → tally** | +3000 f7069 · E=0 f7073 · TIME 0211→0210 f7137 · đếm TIME f7277–7380 · tim f7408–7526 | 3:55.63 · 3:55.77 · 3:57.9 · 4:02.57–4:06.0 · 4:06.93–4:10.87 | +3000 **4 frame TRƯỚC khi vạch ENEMY về 0**; chờ **≈272 tick** → TIME 208 tick → nghỉ 56 → tim 236 tick → chờ 118 | SCORE 039570 →(+2100) 041670 →(+3000) 044670 ✓ = luật Map 1; tim 30→0; TIME 0210→0000 |
| 10 | Tally → **bản đồ lâu đài Block 2** | đen f7585–7595; bản đồ f7596 | 4:12.83–4:13.17 | **11 frame = 22.0 tick** | HUD **STAGE 07**, tim **05**, TIME 0000 (hiện ngay f7596) |
| 11 | Bản đồ B2 (xem §3 dòng 55) | f7596–7791 | 4:13.20–4:19.70 | trượt vào 162 tick (f7599–7680) + đi 1.004 px/tick f7681–7791 | không có Simon f7596–7680 |
| 12 | Bản đồ B2 → đen → **Stage 07** | đen f7792–7805; Stage f7806–7813 | 4:19.73–4:20.17; Stage 4:20.20–4:20.43 | đen **14 frame = 28.0 tick**; Stage 07 hiện 8 frame | TIME **0500** (Block 3 — thêm bằng chứng cho `ConfigBlock`), tim 05, P 04, ENEMY rỗng→16; nền xanh-xám + rêu |

**Luật đổi số STAGE trên HUD (xác nhận, giải quyết nghi vấn routine 00)**: **số stage đổi ở cuối đoạn cuộn thứ 2 của cửa (khi camera vào section mới, ≈ 2 frame = 4 tick sau camera dừng)** — giống Map 1 (đổi ngay khi section hiện). Planner/routine 00 thấy "đổi ~1 s sau khi section hiện" vì coi section hiện từ lúc BẮT ĐẦU cuộn 2 (0:57.70) — thật ra camera còn 128 px mới tới; **không có độ trễ riêng** (0:58.73 = hết cuộn). Khung "2:28.4 STAGE 05 / 2:38 STAGE 06" của planner cũng là hệ quả (cuộn 2 đang chạy, kết thúc 2:29.27, đổi 2:29.33).
