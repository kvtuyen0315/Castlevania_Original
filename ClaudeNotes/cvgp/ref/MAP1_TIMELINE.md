# Chỉ mục timeline + hiệu chuẩn video tham chiếu — Castlevania NES (Stage 01–03)

> **Vị trí file trong repo (planner chép 11/10/2026):** bản này ở `ClaudeNotes/cvgp/ref/MAP1_TIMELINE.md`.
> - Ảnh **full frame gốc** (44 PNG, 28 MB — dùng thẳng công thức §1): `ClaudeNotes/cvgp/_cache/video/ref/` (chỉ ở máy này, KHÔNG commit).
> - Ảnh **đã lấy mẫu về lưới NES 256×240** (tâm từng pixel NES theo §1; hàng ngoài vùng hiển thị = đen): `ClaudeNotes/cvgp/ref/nes/<tên>_nes.png` (commit) — so trực tiếp với ảnh chụp Unity ở lưới NES.
> - Script: bản lưu `ClaudeNotes/cvgp/ref/tools/` (commit); bản chạy được kèm dữ liệu trung gian `work/*.csv`: `ClaudeNotes/cvgp/_cache/video/` (chạy `python -P -E tools/<tên>.py` từ thư mục đó; `tools/nes_common.py` trỏ đường dẫn tuyệt đối tới 2 video). `work/*_nes.npy` (bộ đệm 1.3 GB) không chép — tạo lại bằng `tools/extract_nes.py` (~90 s).
> - Các đường dẫn `ref/`, `tools/`, `work/`, `frames/` nhắc bên dưới là tương đối so với thư mục làm việc gốc của planner ⇒ đọc là `_cache/video/…`.

Video nguồn (chỉ đọc):
- `Map_1.mp4` — 1266×950, 30 fps, 3:59.83 (7195 frame). Quay màn hình một playthrough trên YouTube.
- `Boss_Map_1_Behaviour.mp4` — 1264×834, 30 fps, 25.37 s (761 frame). Quay trình duyệt YouTube ("Castlevania NES All Bosses").

Quy ước trong file này:
- Thời điểm ghi `m:ss.s` (Map_1) hoặc `ss.s s` (video boss). Số frame video `f` = round(t × 30); `f` đếm từ 0.
- **tick** = 1 frame NES (60.0988 Hz). 1 frame video ≈ 2.003 tick. Tốc độ ghi theo **px NES / tick**.
- Toạ độ NES: x 0–255, y 0–239 (gốc trên-trái màn NES; HUD ở y 0–47, sân chơi y 48–223).
- `cx` = camera x = toạ độ x (px NES) của mép trái màn hình **trong vùng map** của sprite sheet
  `NES - Castlevania - Stages - Level 1_ Entrance.png`. Toạ độ thế giới `world_x = cx + x_màn_hình`.
  Vùng (gốc trong sheet, đo bằng tách nền xanh (0,128,0)): Castle Entrance **CE** x0=1 (rộng 768), Entrance Hall **EH** x0=774 (1536),
  Crypt A **CA** x0=2315 y0=11 (512), Crypt B **CB** x0=2315 y0=218 (512), Phantom Bat **PB** x0=2832 (768); các vùng cao 192 dòng,
  **dòng r của vùng ↔ NES y = 32 + r** (16 dòng đầu nằm dưới HUD). Pixel sheet = (x0 + world_x, y0 + y_NES − 32).
  cx tối đa = rộng − 256 (CE 512, EH 1280, CA/CB 256, PB 512) — đều đạt được trong video.
- "y chân" của Simon = hàng thấp nhất có màu cam/kem của sprite = **mặt sàn − 2** (vd. sàn EH: mặt gạch y 208 → y chân 206; sàn CE 192 → 190).

---------------------------------------------------------------------------------------------------

## §1. Hiệu chuẩn video → pixel NES

Công thức chung: `x_NES = (x_v − ox) / sx`, `y_NES = (y_v − oy) / sy` (x_v, y_v = pixel video, gốc trên-trái).
Tâm pixel NES (x, y) nằm ở video `(ox + sx·(x+0.5), oy + sy·(y+0.5))`.

| | **Map_1** (1266×950) | **Boss_Map_1_Behaviour** (1264×834) |
|---|---|---|
| sx (px video / px NES ngang) | **4.8585** ± 0.002 | **3.6295** ± 0.003 |
| sy (dọc) | **4.1605** ± 0.003 | **3.1765** ± 0.003 |
| ox / oy | **8.60 / −22.55** | **169.68 / 40.13** |
| sx/sy | **1.1678 ≈ 7/6** (màn 256×224 kéo về 4:3) — *không phải 8/7* | **1.1426 ≈ 8/7** (pixel aspect 8:7) |
| Vùng game trong khung video | x 8.6–1252.4, y 10.7–942.7 (≈ x 9..1252, y 11..942) | x 169.7–1098.8, y 72–770 (phần nhìn thấy) |
| Dòng NES hiển thị | y 8–231 (224 dòng): **cắt 8 dòng trên + 8 dòng dưới**; đủ 256 cột | y 10–229 (220 dòng): **cắt 10 trên + 10 dưới**; đủ 256 cột |
| Phần ngoài game | trên 0–10: thanh trình duyệt; dưới 943–949; phải 1254–1262: thanh cuộn; logo đỏ (watermark) góc phải-dưới NES x≈240–252, y≈224–231 | 2 bên viền mờ (x<170, x>1099), trên/dưới UI YouTube; chữ watermark "ko-fi.com/overtimegamer" chồng lên khung game ở NES x≈0–90, y≈219–223 (mép dưới sàn, bên trái) |
| Sai số chuyển đổi ước lượng | ≤ ±0.25 px NES (≈ ±1.2 px video ngang, ±1 px dọc) | ≤ ±0.3 px NES |

**Cách đo** (script `tools/calib_hud.py`, `tools/refine_calib2.py`, `tools/verify_calib.py`):
1. *HUD (ước lượng đầu)*: 16 vạch máu PLAYER (đỏ) + 16 vạch ENEMY (cam) → tâm từng vạch trong video; tâm NES biết trước = 56 + 4i + 1.5
   → hồi quy tuyến tính 32 điểm: Map_1 sx 4.8605, ox 8.46, **dư max 0.65 px video (rms 0.29)**; boss sx 3.627, ox 169.5 (dư max 1.35, rms 0.41).
   Dọc: mép trên/dưới 3 hàng chữ SCORE/PLAYER/ENEMY (glyph 7 dòng, đỉnh NES y 16/24/32) → Map_1 sy 4.1775, oy −23.05; boss sy 3.164, oy 40.83.
   Khung vũ khí phụ đo được đúng 32×22 tại (128,24); ô số tim/P ở x 184–199 (dấu "-" ở x 176) — khớp bố cục HUD đã biết.
2. *Tinh chỉnh bằng bản đồ*: lấy mẫu sân chơi (NES y 48–223) theo lưới NES, so ZNCC với từng vùng của sprite sheet tại mọi camera x
   (Map_1: 6 frame ở EH/CB/CA; boss: 4 frame ở PB cx 512) rồi quét lưới (sx, cx_tâm) và (sy, cy_tâm). ZNCC sau tinh chỉnh 0.95–0.98.
   Sy theo HUD lệch 0.4% vì chỉ có 24 dòng NES làm cơ sở; giá trị cuối dùng sy theo bản đồ (khớp đúng biên dải đen:
   Map_1 932 dòng video = 224 dòng NES → 4.1607; boss 699 dòng = 220 dòng → 3.177).
3. *Kiểm*: lấy mẫu đúng tâm từng pixel NES ô "SCORE" (x 0–39, y 16–23) và in ASCII → glyph S, C, O, R, E nét đều, đúng 7 dòng, không dòng/cột kép
   ở cả 2 video. Thêm chỉ số "độ nét" (tỉ lệ mẫu rơi vào nền tối <60 hoặc nét sáng >170 trong vùng chữ HUD x 0–55, y 16–39):
   Map_1 = **1.000** ở lưới cuối (dịch ±0.5 px NES ngang → 0.93–0.95; dịch ±0.5 dọc → 0.80–0.86); boss = 0.995 (dịch ±0.25 ngang → 0.87).
   Cột ngang Map_1 cho độ nét 1.000 trong khoảng dịch 0…+0.25 px → nếu cần cực chính xác có thể cộng ox thêm ~+0.5 px video.

**Nhịp quay thực (frame lặp)** — `tools/dup_frames.py` (frame "lặp" = không pixel NES nào đổi > 40/255 so với frame trước):
- Map_1: 35.1% frame video là bản lặp. Trong các đoạn camera đang cuộn đều (2042 frame): 29.5% lặp → **≈ 21 frame game duy nhất/giây**.
  Dùng camera (cuộn đúng 1 px/tick khi bám Simon) làm đồng hồ: khoảng cách giữa 2 frame duy nhất liên tiếp = 2 tick (54%), 3 (17%), 4–6 (23%), 1 (6%),
  trung bình 2.8 tick → **capture thực ≈ 21 fps** của game 60 Hz, có jitter. Đoạn 3:30–3:50 chỉ 9–11 frame mới/s vì màn tĩnh (bảng cộng điểm).
- Boss: 45% lặp; khi có chuyển động ≈ 20–22 fps (0–10 s: 22.1; 10–20 s: 16.4; 20–25 s: 6.1 — màn tĩnh).
- Hệ quả: một mốc đơn lẻ sai ±1 frame video (±2–3 tick). Đo thời lượng nên dùng hồi quy nhiều frame, hoặc **đồng hồ TIME trên HUD**:
  1 đơn vị TIME = **64.0 tick** (đo 5 đoạn, 31.93–31.95 frame video/đơn vị, lệch ≤ 2 frame) → tốc độ phát video đúng thời gian thực.

---------------------------------------------------------------------------------------------------

## §2. Timeline Map_1

Suốt Map_1: **P = 03**, **máu PLAYER = 16/16 vạch** (Simon không hề bị trúng đòn), ENEMY = 16 vạch cho tới trận boss.
Roi: cấp 1 tới 0:06.47, cấp 2 từ 0:07.23, **cấp 3 (xích dài) từ 0:10.50** tới hết. Cột "Vũ khí" = vũ khí phụ (+ hệ số II).
Nguồn: `work/map1_hud.csv` (đọc HUD từng frame), `work/map1_events.txt` (mọi lần HUD đổi), `work/map1_camera.csv` (vùng + cx từng frame),
`work/map1_simon.csv` (vị trí Simon), cộng xem ảnh contact 0.5 s/khung (`frames/pf_*.png`) và các dải zoom.

| # | Thời gian (frame) | ST | TIME | SCORE | Tim | Vũ khí | Khu vực · camera x | Sự kiện |
|---|---|---|---|---|---|---|---|---|
| 1 | 0:00.0–0:01.7 (f0–50) | 01 | 0300 | 000000 | 05 | – | **Intro** (ngoài cổng, đêm; không có trong sheet) | Cuối đoạn Intro: Simon đứng trước cổng, trăng, dơi nhỏ bay. Lớp phủ YouTube: nút ▶ giữa màn f0–19, thanh điều khiển f0–9. |
| 2 | 0:01.7–0:02.2 (f51–66) | – | – | – | – | – | màn đen | chuyển cảnh |
| 3 | 0:02.2–0:06.4 (f67–193) | 01 | 0300→0297 | 0 | 05→10 | – | **CE** cx 0→87 | Simon xuất hiện world x 51, y chân 190; đi phải. 0:03.2 roi (cấp 1) chậu lửa #1 → tim lớn, nhặt 0:03.93 (+5). 0:05.8 roi chậu #2 → item roi rơi. |
| 4 | 0:06.5–0:07.2 (f194–217) | 01 | 0296 | 0 | 10 | – | CE cx 87 (đứng) | **Nhặt roi #1 → đóng băng 0.77 s**, Simon nháy màu; roi lên cấp 2. TIME vẫn chạy. |
| 5 | 0:07.3–0:09.7 (f218–292) | 01 | 0296→0293 | 0 | 10 | – | CE 91→213 | 0:08.9 roi (cấp 2) chậu #3 → item roi. |
| 6 | 0:09.8–0:10.5 (f293–315) | 01 | 0293 | 0 | 10 | – | CE 214 | **Nhặt roi #2 → đóng băng 0.73 s**; roi cấp 3. |
| 7 | 0:10.5–0:16.2 (f316–485) | 01 | 0293→0287 | 0 | 10→15 | –→**dao** | CE 219→512 | 0:12.1 chậu → tim lớn (0:12.97, +5). 0:14.5 chậu → dao găm rơi trên sàn, nhặt 0:15.50. Từ 0:15.0 tháp cổng lâu đài hiện bên phải. 0:16.2 **camera khoá cx 512** (hết vùng CE). |
| 8 | 0:16.2–0:19.7 (f486–590) | 01 | 0287 (dừng) | 0 | 15 | dao | CE 512 | Simon đi vào cổng: x màn 122→160 với 1 px/tick rồi **chậm ~0.3 px/tick** (f504–534), khuất sau mép cổng x≈191 lúc 0:18.3. TIME đứng từ ~0:16.2 tới 0:20.0. |
| 9 | 0:19.7–0:20.0 (f591–599) | – | – | – | – | – | màn đen | vào lâu đài |
| 10 | 0:20.0–0:42.0 (f600–1261) | 01 | 0287→0267 | 0→1800 | 15→23 | dao | **EH** cx 0→497 | Simon world x 49, y chân 206. **Zombie** ra từng nhóm 2–3 từ mép phải. 0:21.83 túi tím +400 (chữ "400"); 0:23.0 một roi hạ 2 Zombie (+200), 0:23.6 +100; nến tường → tim nhỏ. 0:28.2–0:29.6 Zombie + túi đỏ rơi từ nến (+100/+200/+200; nhặt túi hiện chữ "100" lúc 0:29.6). 0:29.5–0:31.7 **đồng hồ (stopwatch)** nằm trên sàn — Simon nhảy qua, không nhặt. Nhảy tại chỗ 0:23.8, 0:27.1, 0:32.2, 0:36.1 (kèm roi), 0:37.5, 0:40.1. 0:40–0:42 chân cầu thang đi lên phải. |
| 11 | 0:42.1–0:43.0 (f1262–1290) | 01 | 0267→0266 | 1800 | 23 | dao | EH 500→538 | **Black Leopard** nhảy từ gờ xuống trái (ngang ≈1.9 px/tick, cung rơi), bị roi 0:43.17 (+200). 2 con báo khác nằm chờ trên gờ (x≈217,y≈112 và x≈155,y≈144 màn). |
| 12 | 0:43.0–0:53.0 (f1291–1589) | 01 | 0266→0257 | 1800→2700 | 23→24 | dao | EH 498→606 | 0:44.67 túi trắng +700. 0:45.0–0:46.6 đi trái. **0:46.4–0:50.4 lên cầu thang** (phải-lên) lên tầng giữa; 0:49.4 nhảy. Camera đứng cx 563 khi Simon ở tầng trên (0:49.5–0:52.5). |
| 13 | 0:53.0–0:59.6 (f1590–1787) | 01 | 0256→0250 | 2700→2900 | 24→29 | dao | EH 594→778 | Lối đi tầng trên (y chân 110). Báo nằm chờ trên gờ thấp bên phải; 0:54.90 roi trúng (+200). 0:55.87 tim lớn. 0:56.3 nhảy. 0:57.3–0:58.9 **bình nước thánh** nằm trên lối đi (không nhặt). |
| 14 | 0:59.6–1:01.9 (f1788–1856) | 01 | 0250→0248 | 2900 | 29→30 | dao | EH 778→872 | **Báo nhảy** từ gờ xuống trái (0:59.60–1:00.10, ngang 2.0 px/tick), chạm sàn rồi **chạy sang phải 2.0 px/tick** (1:00.5–1:01.9) ra khỏi màn. 1:00.27 Simon nhảy + roi trên không. |
| 15 | 1:01.9–1:13.5 (f1857–2204) | 01 | 0248→0237 | 2900→3900 | 30→34 | dao | EH 767→1152 | 1:02–1:04 xuống cầu thang về sàn dưới; 1:04.50 túi trắng +700. Sảnh cửa sổ cao: Zombie liên tục (+100 ×3 lúc 1:09.7–1:10.7), nến → tim. |
| 16 | 1:13.5–1:15.0 (f2205–2250) | 01 | 0237→0236 | 3900→4100 | 34→35 | dao | EH 1158→1203 | Zombie (+100 ×2); nến → **rosary** (thánh giá xanh). |
| 17 | 1:15.0–1:15.4 (f2251–2263) | 01 | 0236 | 4100 | 35 | dao | EH 1214→1227 | **Nhặt rosary → màn nháy trắng 0.40 s** (trắng/thường xen kẽ 1–2 frame video), xoá quái trên màn. |
| 18 | 1:15.5–1:18.3 (f2264–2348) | 01 | 0235→0233 | 4100→4200 | 35→36 | dao | EH 1185→1244 | Zombie lại kéo tới; 1:15.5 nhảy + roi; chân cầu thang dài. |
| 19 | 1:18.3–1:24.1 (f2349–2523) | 01 | 0233→0227 | 4200 | 36→37 | dao | EH 1170→**1280** | **Lên cầu thang dài** (phải-lên) tới lối đi trên cùng (y chân 110) trong khi Zombie đi bên dưới. 1:22.0 camera chạm cuối vùng. 1:23.5 roi nến → rosary rơi. |
| 20 | 1:24.1–1:25.1 (f2524–2554) | 01 | 0227→0226 | 4200 | 37 | dao | EH 1280 | Đi tới **cửa cuối stage** (mép phải, cột đỏ-trắng); nhặt rosary → nháy trắng 1:24.73–1:25.13. |
| 21 | 1:24.8–1:31.8 (f2545–2755) | 01→02 | 0226 (đứng 6.8 s) | 4200 | 37 | dao | EH 1280 → (ghép EH\|CA) → **CA** 0 | **Qua cửa**: camera cuộn 128 px (1:24.83–1:27.17, 0.91 px/tick) → camera đứng, cửa mở, Simon tự đi x màn 109→177 (1 px/tick), cửa đóng (1:27.17–1:29.47) → cuộn tiếp 128 px (1:29.47–1:31.83, 0.90 px/tick). HUD đổi **STAGE 02 lúc 1:31.87**. |
| 22 | 1:31.9–1:38.1 (f2756–2944) | 02 | 0226→0221 | 4200→4400 | 37→36→41→42 | dao | **CA** 0→76 | Simon world x 49, y chân 110 (tầng trên). **Vampire Bat** bay tới từ phải; 1:32.83 **ném dao** (−1 tim) → trúng dơi 1:33.53 (+200). 1:34.23 tim lớn. 1:35.1 roi khi ngồi + roi đứng. 1:36.0–1:37.5 xuống cầu thang bên phải. |
| 23 | 1:38.2–1:47.7 (f2945–3232) | 02 | 0221→0212 | 4400→5800 | 42→43→42 | dao | CA 76→132→0 | 1:39.3 túi đỏ rơi từ nến; 1:39.6 **roi khi ngồi**: hạ dơi bay sát sàn (+200, 1:39.73) và **đập vỡ tường gạch đỏ** — mảnh tím bay, lộ **thịt nướng** (không nhặt). 1:40.13 nhặt túi (+100, chữ "100" ~0.8 s). 1:41.1–1:42.5 dơi (+200). Đi trái về cx 0; 1:45.90 túi trắng +700. 1:46.83 ném dao → trúng dơi 1:47.40 (+200). 1:47.5 **xuống cầu thang** góc trái-dưới (world x 61→66). |
| 24 | **1:47.8–1:54.3 (f3233–3430)** | – | – | – | – | – | **NON-GAME — loại khỏi đo đạc** | f3233–3235 (1:47.77–1:47.83) màn đen chuyển tầng (của game); **f3236–3245** trang YouTube (player đen + tiêu đề "Castlevania (NES) Playthrough"); **f3246–3409 (1:48.20–1:53.63) quảng cáo** (cảnh phòng làm việc); f3410–3430 player đen + thanh điều khiển. Game trở lại ở **f3431 (1:54.37)**. Phần chuyển tầng CA→CB bị che. |
| 25 | 1:54.4–1:56.7 (f3431–3502) | 02 | 0212→0210 | 5800→5900 | 42 | dao | **CB** 0 | Nước xanh đáy màn, lối đi gạch. Simon đang **xuống cầu thang** từ góc trái-trên (world x 68 — đúng ngay dưới chỗ rời CA). **Thanh điều khiển YouTube còn chồng đáy khung (NES y≈218–232) tới f3502** — đừng đo vùng đó. 1:56.1 roi khi ngồi; 1:56.73 túi đỏ +100. |
| 26 | 1:56.8–2:03.3 (f3503–3699) | 02 | 0210→0204 | 5900→7500 | 42→47 | dao | CB 0 | 1:57.2 **2 Fish Man nhảy khỏi nước** (x 30 và 160) lên lối đi; đi trên lối đi; roi +300 (1:58.30, 1:59.20, 2:01.07). 1:59.90 tim lớn. Nhảy 2:00.7, 2:01.7. 2:03.0 túi trắng +700. |
| 27 | 2:03.3–2:16.6 (f3700–4099) | 02 | 0204→0192 | 7500→8900 | 47→49 | dao | CB 0→256 | 2:03.7 nhảy lên bục cao hơn 32 px (y chân 126→94), đi trên bục rồi bước xuống lại 2:05.3. 2:05.6 túi đỏ +100. Fish Man nhảy lên liên tục (2:06.7, 2:09.3, 2:10.3…); +300 (2:07.57, 2:12.47); 2:08.93 túi +700. Nhiều cú nhảy + roi trên không; 2:10.7 ngồi. Cuối vùng (cx 256) ~2:14.5. |
| 28 | 2:16.7–2:23.3 (f4100–4299) | 02 | 0192→0185 | 8900→9500 | 49→54 | dao | CB 256 | Fish Man từ bên trái (+300 2:18.00, 2:22.73); 2:18.50 tim lớn. |
| 29 | 2:23.3–2:27.7 (f4300–4430) | 02 | 0185→0181 | 9500→10500 | 54 | dao | CB 226→256 | 2:23.80 **túi 1000** nhấp nháy (+1000, chữ "1000" ~0.8 s). 2:25.4–2:27.6 **lên cầu thang** (trái-lên) ra mép trên → sprite Simon **đè lên HUD** (2:27.40–2:27.67, ô vũ khí phụ bị che — bộ đọc tự động báo nhầm "holy"). |
| 30 | 2:27.7–2:28.0 (f4431–4440) | – | – | – | – | – | màn đen 0.33 s | chuyển tầng CB→CA |
| 31 | 2:28.0–2:31.8 (f4441–4554) | 02 | 0181→0178 | 10500→10800 | 54→55 | dao | **CA** 255→220…256 (tầng dưới phải) | Simon trồi lên từ đáy màn (world x 381 ≈ chỗ rời CB 387). Dơi (+200 2:29.70); túi đỏ +100 (2:30.10); đi phải 2:30.3–2:32.2. |
| 32 | 2:31.8–2:42.6 (f4555–4879) | 02 | 0178→0168 | 10800→11100 | 55→60 | dao | CA 256→180→256 | **2:32.0–2:37.0 lên cầu thang dài** (trái-lên); dơi (+200 2:35.80); 2:37.60 tim lớn; leo tiếp lên lối đi trên; +100 2:41.97; rosary trên lối đi. |
| 33 | 2:42.7–2:43.2 (f4880–4895) | 02 | 0168→0167 | 11100 | 60 | dao | CA 256 | Nhặt rosary → nháy trắng 0.44 s. |
| 34 | 2:43.8–2:50.9 (f4915–5125) | 02→03 | 0166 (đứng 6.9 s) | 11100 | 60 | dao | CA 256 → (ghép CA\|PB) → **PB** 0 | **Qua cửa stage 2**: cuộn 128 px (0.90 px/tick) → dừng 2.33 s (cửa mở, Simon tự đi x 109→177, cửa đóng) → cuộn 128 px (0.91) → **STAGE 03 lúc 2:50.87**. |
| 35 | 2:50.9–2:56.6 (f5126–5299) | 03 | 0166→0161 | 11100→11300 | 60→61 | dao | **PB** 0→56 | Hành lang Phantom Bat (nền xám như EH). Simon world x 49 tầng trên (y chân 110); Zombie ở sàn dưới. 2:53.6 nhảy xuống bậc giữa (y chân 142); túi đỏ +100 ×2 (2:55.80, 2:56.40); roi trên không 2:56.3. |
| 36 | 2:56.7–3:02.6 (f5300–5479) | 03 | 0161→0155 | 11300 | 61→66 | dao | PB 0→92 | 2:57.3 roi nến → **bình vô hình** (cam) → nhặt ~2:58.2 → Simon **vô hình (chỉ còn bóng nhạt) ~2:59.4–3:02.0**, rồi hiện lại; đi xuống qua đám Zombie không mất máu. 3:01.87 tim lớn. |
| 37 | 3:02.7–3:10.6 (f5480–5719) | 03 | 0155→0148 | 11300→11900 | 66→67 | dao | PB 0→214 | Sàn dưới: Zombie (+200 3:03.07, +100 3:03.63; roi khi ngồi 3:03.6). 3:07.47 tim; +100 3:08.10; nhảy 3:08.3; 3:09.53 +200 và nến rơi **rìu**. |
| 38 | 3:10.7–3:17.4 (f5720–5921) | 03 | 0148→0142 | 11900 | 67→75 | dao→**rìu** (3:10.67) | PB 216→510 | Đi phải qua sảnh vòm; nến → tim (3:12.30, 3:13.77, +5 3:15.27, 3:16.90). Từ ~3:15 thấy **dơi boss ngủ** treo ở trần phòng boss. |
| 39 | 3:17.4–3:19.4 (f5922–5982) | 03 | 0142→0140 | 11900 | 75→76 | rìu | **PB 512 (khoá — phòng boss)** | 3:17.9 nhảy; 3:18.0 roi nến chân cầu thang → item **"II"** rơi xuống sàn; 3:18.9 Simon lên cầu thang. |
| 40 | 3:19.4–3:24.9 (f5983–6148) | 03 | 0140→0135 | 11900 | 76→81→78 | rìu + **II** (3:19.67; icon nhấp nháy tới 3:21.57) | PB 512 | **BOSS THỨC 3:19.43** (Simon x màn 183, đang trên cầu thang). Lượn tại chỗ 0.5 s → lao xuống-phải tới (177,108) → bay chậm sang trái tới (27,120) (3:20.9–3:24.2). Simon xuống cầu thang; 3:21.33 tim lớn; **ném rìu** (−1 tim/rìu: 3:22.23, 3:24.00, 3:24.67, …). |
| 41 | 3:25.0–3:30.4 (f6149–6311) | 03 | 0134→0129 | 11900→**14900** | 78→72 | rìu II | PB 512 | Rìu trúng: E 16→8 (3:24.97–3:26.63) khi dơi lơ lửng bên trái rồi bổ nhào về phía Simon; 3:27.0–3:27.7 dơi vọt lên-phải tới (190,115), rồi lơ lửng tại (210,147); E 8→0 (3:28.33–3:30.40). **3:30.17 +3000** (boss chết); thanh E tụt nốt 2 vạch tới 3:30.40. Dơi → **chùm lửa** 3:30.4–3:31.2. |
| 42 | 3:30.4–3:33.0 (f6312–6390) | 03 | 0129→0127 | 14900 | 72 | rìu II | PB 512 | 3:32.67 **quả cầu đỏ** hiện ở (128,128) và rơi; 3:33.03 Simon nhảy chụp cầu (máu đã đầy). TIME dừng ở 0127. |
| 43 | 3:33.0–3:37.4 (f6391–6522) | 03 | 0127 | 14900 | 72 | rìu II | PB 512 | Đứng chờ (nhạc hết stage), không có gì chuyển động. |
| 44 | 3:37.4–3:39.5 (f6523–6586) | 03 | 0127→**0000** | 14900→16170 | 72 | rìu II | PB 512 | **Bảng cộng điểm — đếm TIME**: 1 đơn vị/tick, +10 điểm/đơn vị (127 đơn vị trong 63 frame video). |
| 45 | 3:39.6–3:40.4 (f6587–6612) | 03 | 0000 | 16170 | 72 | rìu II | PB 512 | nghỉ 0.9 s |
| 46 | 3:40.4–3:49.9 (f6613–6897) | 03 | 0000 | 16170→**23370** | 72→**00** | rìu II | PB 512 | **Đếm tim**: 1 tim / 8 tick (≈7.5 tim/s), +100 điểm/tim (72 tim trong 9.47 s). |
| 47 | 3:49.9–3:52.3 (f6898–6969) | – | – | 23370 | 00 | – | PB 512 → đen | chờ 2.0 s (f6898–6955) rồi đen 0.43 s (f6956–6969) |
| 48 | 3:52.3–3:58.7 (f6970–7162) | **04** | 0000 | 23370 | **05** (reset) | rìu II | **Màn bản đồ lâu đài** | Bản đồ (nền cam-nâu) trượt vào từ phải 3:52.3–3:55.0; icon dơi tím (boss đã hạ) trong ô tối bên trái; Simon (sprite thường) đi dọc đáy từ trái sang phải **1.0 px/tick** (3:55.0–3:58.7); chấm tím nhấp nháy ở ô kế. |
| 49 | 3:58.8–3:59.2 (f7163–7176) | – | – | – | – | – | màn đen 0.47 s | |
| 50 | 3:59.2–3:59.8 (f7177–7194) | 04 | **0400** | 23370 | 05 | rìu II | Stage 04 (ngoài sheet Level 1) | Gạch đỏ, Simon góc trái-dưới; ENEMY lại 16 vạch. |

Điểm số quan sát: Zombie +100, Vampire Bat +200, Black Leopard +200, Fish Man +300, Phantom Bat +3000;
túi tiền đỏ +100, tím +400, trắng +700, nhấp nháy +1000. Tim nhỏ +1, tim lớn +5; mỗi lần ném dao/rìu −1 tim.

---------------------------------------------------------------------------------------------------

## §3. Cửa sổ đo (hành vi hiện rõ, không bị che)

Tất cả nằm ngoài đoạn non-game f3233–3430 và ngoài đoạn thanh YouTube che đáy f3431–3502 (trừ khi ghi chú).

| Hành vi | Cửa sổ (m:ss.s, frame) | Ghi chú |
|---|---|---|
| **Simon đi thẳng** (camera cuộn theo, Simon giữa màn x≈128) | 0:03.6–0:05.6 (f108–168); 0:07.1–0:08.6 (f214–259); 0:10.5–0:12.0 (f314–359); 0:12.3–0:14.3 (f369–429); 0:14.8–0:16.8 (f445–505); 0:45.0–0:46.6 (f1349–1399, đi trái); 0:57.8–0:59.3 (f1733–1778); 1:04.1–1:05.9 (f1923–1978); 1:42.6–1:44.9 (f3077–3147, trái); 2:30.3–2:32.2 (f4510–4565); 3:03.6–3:06.1 (f5508–5583) | world x tuyến tính, y chân không đổi |
| Simon đi khi camera **đứng yên** (đo trên toạ độ màn) | 1:27.3–1:29.4 (f2620–2683) và 2:46.3–2:48.5 (f4990–5055) — tự đi qua cửa; 3:55.0–3:58.7 (f7050–7162) — bản đồ lâu đài | |
| **Nhảy tại chỗ** | 0:23.8–0:24.5 (f715–735); 0:27.1–0:27.8 (f814–835); 0:32.2–0:33.0 (f967–989); 0:37.5–0:38.1 (f1124–1143); 0:49.4–0:50.2 (f1483–1505); 0:56.3–0:57.0 (f1690–1711); 2:00.7–2:01.4 (f3622–3641); 2:16.8–2:17.5 (f4104–4125); 2:36.1–2:36.8 (f4683–4703); 2:56.9–2:57.6 (f5307–5328) | dx ≈ 0 |
| **Nhảy kèm di chuyển** | **0:30.0–0:30.7 (f899–921)** và **2:09.2–2:09.9 (f3876–3898)** — camera đang cuộn → dùng cx làm đồng hồ tick; 2:12.0–2:12.6 (f3959–3978); 2:14.0–2:14.7 (f4020–4040, camera chạm biên giữa chừng); 2:53.6–2:54.2 (f5207–5227); 3:18.8–3:19.5 (f5963–5984) | dx 39–43 px |
| Nhảy lên bục / nhảy xuống tầng thấp hơn | lên bục +32 px: 2:03.7–2:04.3 (f3711–3726), bước xuống f3756–3760; phòng boss: nhảy lên khối gạch chân cầu thang rồi nhảy xuống sàn 3:23.1–3:24.4 (f6094–6132); 2:53.6–2:54.2 (f5207–5227, lối đi trên y110 → bậc giữa y142) | |
| **Ngồi** | 2:10.7–2:10.9 (f3922–3927) — duy nhất và ngắn; xem thêm tư thế ngồi trong các cú roi khi ngồi bên dưới | ít dữ liệu |
| **Roi đứng — cấp 1** | 0:03.07–0:03.37 (f92–101); 0:05.70–0:06.03 (f171–181) | đánh chậu lửa |
| Roi đứng — cấp 2 | 0:08.80–0:09.13 (f264–274) — chỉ 1 lần | đánh chậu lửa |
| Roi đứng — cấp 3 | 0:29.03–0:29.33 (f871–880); 1:00.13–1:00.50 (f1804–1815); 1:13.77–1:14.10 (f2213–2224); 1:32.47–1:32.73 (f2770–2782); 0:31.0–0:31.2 (f930–935) | 57 lần tự động phát hiện (`work/whip_attacks.txt`) |
| Roi khi ngồi (cấp 3) | 1:35.10–1:35.23 (f2853–2858); 1:39.57–1:39.73 (f2987–2991, đập tường); 1:40.60–1:40.70 (f3018–3020); 1:56.07–1:56.13 (f3482–3484, *có thanh YouTube ở đáy*); 3:03.53–3:03.63 (f5505–5508) | sprite cao 23 thay vì 27 |
| Roi trên không (giữa cú nhảy, cấp 3) | 0:24.10 (f720–730); 0:27.27 (f815–825); 0:32.43 (f970–980); 0:36.30 (f1086–1094, nhảy tại chỗ); 0:40.30 (f1206–1216); 1:15.87 (f2273–2280); 1:38.77 (f2960–2966); 2:01.90 (f3655–3660); 2:08.17; 2:09.63; 2:10.53; 2:12.30; 2:14.33; 3:08.60; 3:14.17; 3:18.13; 3:20.00 | dùng khung roi đứng ở đỉnh nhảy |
| Roi trên cầu thang | **không tìm thấy** trong Map_1 | |
| **Ném dao găm** | 1:32.83–1:33.53 (f2785–2806; dao bay f2795–2804, trúng dơi); 1:46.83–1:47.40 (f3205–3222; bay f3213–3219) | camera đứng (cx 0) — sạch |
| **Ném rìu** (có II) | 3:22.2–3:30.3 (9 rìu); quỹ đạo sạch: f6237–6254 (3:27.9–3:28.5) và f6279–6292 (3:29.3–3:29.7) | camera khoá |
| Ném nước thánh | chỉ có trong video boss: 12.9–13.9 s (f389–417), 15.3–18.1 s (cháy giữ boss) | |
| **Lên cầu thang** | 0:46.7–0:48.9 (f1400–1466, phải-lên); **1:18.6–1:21.3 (f2357–2438, dài nhất)**; 2:25.7–2:27.6 (f4372–4428, trái-lên); 2:32.3–2:34.3 (f4569–4629, trái-lên) | |
| **Xuống cầu thang** | 1:54.4–1:55.3 (f3431–3459, phải-xuống, *thanh YouTube ở đáy*); 1:47.5–1:47.7 (f3226–3232, rất ngắn); 3:21.1–3:24.2 (f6032–6156, phòng boss, xen ném rìu) | |
| **Bị đánh lùi (knockback)** | **không có trong Map_1** (máu luôn 16). Chỉ có ở video boss 11.87–12.63 s (f356–379) | |
| Nháy bất tử sau khi trúng | chỉ video boss: 12.67–14.20 s (f380–426) | Simon gần như không thấy trên bản quay (nhịp nháy lệch pha với nhịp capture) |
| Vô hình (bình thuốc) — khác cơ chế | 2:59.4–3:02.0 (f5382–5461) | |
| **Nhặt roi (đóng băng + nháy)** | 0:06.47–0:07.23 (f194–217); 0:09.77–0:10.50 (f293–315) | camera đứng, Simon đổi màu |
| **Zombie đi** | 0:20.0–0:23.5 (f600–705, 3 con); 0:23.3–0:25.3 (f700–759); 1:06–1:13 (EH sảnh cửa sổ); 1:16–1:24 (dưới cầu thang dài); 2:51–3:09 (PB) | |
| **Black Leopard** nằm chờ | 0:42.0–0:43.2 (2 con trên gờ); 0:54.0–0:59.5 (gờ phải) | |
| Leopard nhảy | 0:42.10–0:42.90 (f1263–1287); 0:59.60–1:00.10 (f1788–1803) | |
| Leopard chạy | 1:00.50–1:01.87 (f1815–1856) | |
| **Vampire Bat bay** | 1:32.0–1:33.4 (f2760–2803); 1:41.0–1:42.4 (f3030–3073); 1:45.8–1:47.3 (f3173–3219); 2:28.5–2:29.9 (f4455–4497); 2:41.3–2:42.7 (f4839–4880) | |
| **Fish Man nhảy khỏi nước** | 1:57.17–1:58.2 (f3515–3547, 2 con, quỹ đạo sạch); 2:06.7–2:07.5 (f3800–3824); 2:09.3–2:09.7 (f3879–3891); 2:17.4–2:18.3 (f4122–4148); 2:25.9–2:26.7 (f4377–4400) | |
| Fish Man đi trên lối đi | 1:58.2–2:00.0 (f3548–3600); 2:21–2:23 | |
| Fish Man bắn lửa | **không quan sát thấy** | |
| **Nến/chậu lửa vỡ + item rơi** | chậu CE: 0:03.0–0:03.9 (tim lớn), 0:05.7–0:06.4 (roi), 0:08.8–0:09.7 (roi), 0:12.0–0:13.0 (tim lớn), 0:14.4–0:15.5 (dao); nến tường: 1:23.4–1:24.6 (rosary), 2:57.2–2:58.2 (bình vô hình), 3:09.4–3:10.7 (rìu), 3:17.9–3:19.7 ("II"); tim nhỏ rơi lắc lư 2:14.7–2:15.3 (f4038–4058) | |
| **Tiền nổ số điểm** | 1:40.0–1:40.9 (túi đỏ, chữ "100"); 2:23.6–2:24.7 (túi 1000, chữ "1000", f4300–4340); 0:21.8 (+400); 0:44.7 (+700); 1:04.5 (+700) | chữ điểm hiện ~0.8 s |
| Rosary (nháy trắng) | 1:15.03–1:15.43; 1:24.73–1:25.13; 2:42.73–2:43.17 | |
| **Mở cửa cuối stage + camera cuộn** | 1:24.83–1:31.83 (f2545–2755); 2:43.83–2:50.87 (f4915–5126) | TIME đứng yên |
| Vào cổng lâu đài (cuối CE) | 0:16.2–0:18.3 (f486–549) | camera khoá cx 512 |
| **Chuyển tầng qua cầu thang** | CB→CA: 2:25.4–2:28.2 (Simon lên mép trên, đè HUD, đen 2:27.70–2:28.00, hiện từ đáy CA); CA→CB: chỉ thấy phần đầu 1:47.5–1:47.83 (phần còn lại bị non-game che) | |
| **Boss Phantom Bat** | Map_1 3:15–3:31.2 (ngủ → thức 3:19.43 → chết 3:30.40); video boss 0–19 s (xem §5) | |
| **Tally cuối stage** | đếm TIME 3:37.43–3:39.53; đếm tim 3:40.43–3:49.90; video boss: hồi máu 20.90–21.80 s, đếm TIME 24.63–25.37 s | |
| **Màn bản đồ lâu đài** | 3:52.33–3:58.73 (f6970–7162) | |

---------------------------------------------------------------------------------------------------

## §4. Số đo sơ bộ (planner-level)

| Đại lượng | Giá trị | Cách đo / cửa sổ / sai số |
|---|---|---|
| **Tốc độ đi** | **1.00 px/tick** (60.16 px/s) | `measure_walk.py`: hồi quy world_x(t) trên 11 đoạn đi thẳng ≥ 1.5 s (tổng 20.7 s, §3); từng đoạn 0.987–1.028; dư max ≤ 3.5 px. Sai số ±0.01. Bản đồ lâu đài cũng đo 1.000. |
| Camera bám Simon | giữ Simon ở x màn ≈ 128–129 (x_med) khi cuộn | camera_track + simon_track |
| **Nhảy — thời gian trên không** | **≈ 38–40 tick** | Đồng hồ camera (nhảy kèm di chuyển f899, f3876): cất cánh giữa cx 106→108, chạm đất giữa cx 140→146 → 32–40 tick; đếm frame video trên 25 cú nhảy sạch: 18–21 frame (36–42 tick), trung vị 20 frame = 40 tick. |
| **Nhảy — độ cao** | chân lên **36–38 px** trên mặt đất; đỉnh đầu lên 31–36 px | `measure_jump.py`, `jump_camclock.py`. Trong phần lớn cú nhảy sprite là tư thế **co chân** (cao 20 px thay vì 26–27) — trừ khi đang roi. |
| Nhảy — dạng quỹ đạo | không phải parabol thuần: lên trung bình ~2.3 px/tick trong ~16 tick đầu, **đỉnh phẳng ±1 px ~10 tick**, rơi ~2.5–3 px/tick ở cuối | f899–921 theo cx; khớp parabol cho thời gian bay 44–50 tick (vượt giới hạn đồng hồ camera) ⇒ dùng bảng vận tốc kiểu NES chứ không phải trọng lực hằng. |
| Nhảy kèm di chuyển — tầm xa | **39–43 px** (trung vị 42) | = 1 px/tick × thời gian bay; không đổi hướng giữa không trung trong mọi mẫu |
| **Cầu thang lên/xuống** | **0.50 px/tick ngang và 0.50 px/tick dọc** (chéo 45°) | `measure_stairs.py` 5 cửa sổ: x 0.497–0.503, y 0.48–0.504 px/tick (f1400–1466, f2357–2438, f4372–4428, f4569–4629, f3431–3459). Sai số ±0.01. |
| **Knockback** (chỉ video boss) | lùi **≈ 11–13 px** ngược hướng mặt (Simon quay trái → bật sang phải) và nảy lên **≈ 20 px** (chân 207→187), tiếp đất ~0.4–0.5 s sau; sát thương −2 vạch | video boss f356–379; `simon_boss.py`. Thời điểm chạm đất không chắc (Simon nhấp nháy ngay sau). |
| Nhấp nháy bất tử sau trúng đòn | ≈ 1.5 s (12.67→14.20 s, ~92 tick) | video boss: Simon hầu như không hiện trong ảnh quay suốt khoảng này |
| **Đòn roi — thời lượng** | **≈ 18–22 tick** tổng (trung vị ~20): khung 1 (roi vung ra sau ~7 px) ≈ 4–6 tick; khung 2 (giơ cao ~15 px sau lưng) ≈ 4–8 tick (điển hình 6); khung 3 (duỗi thẳng) ≈ 6–10 tick | `whip_track.py` (trừ nền bằng sheet → độ dài phần tiền cảnh nằm ngang ngoài thân); 6 đòn ở §3; mỗi pha sai ±2 tick (nhịp capture). |
| **Tầm roi** (từ tay tới đầu roi, khung duỗi thẳng) | cấp 1 ≈ **22 px**; cấp 2 (xích ngắn) ≈ **24–25 px** (≈ cấp 1); cấp 3 (xích dài) ≈ **43 px**. Tính từ tâm thân Simon (x_med): ≈ 29 / 34 / 52 px. Roi nằm ngang ở **đỉnh đầu + 9–10 px** (≈ 16 px trên y chân) | đọc từng pixel hàng roi (f178–180 cấp 1, f270–272 cấp 2, f877–878 cấp 3) + `whip_track.py`/`whip_reach.py` (trừ nền bằng sheet); cấp 3 đối chiếu 57 đòn (ext điển hình 43). Cấp 1–2 chỉ có đòn đánh chậu lửa → đầu roi có thể lẫn tia lửa trúng đòn ±2 px. |
| Đóng băng khi nhặt roi | 0.73–0.80 s (≈ 46–48 tick) | f194–217, f293–315; camera + Simon đứng, TIME vẫn chạy |
| Rosary | màn nháy trắng 0.40–0.44 s | 3 lần |
| **Dao găm** | **4.0 px/tick** ngang, bay thẳng (y không đổi ≈ đỉnh đầu + 4–5 px, tức ~21 px trên y chân; xuất hiện ngay trước tay) | f2797–2804, f3214–3219: 8 px/2 tick hoặc 12 px/3 tick giữa các frame duy nhất |
| **Rìu** | vx ≈ **2.0–2.2 px/tick**, vy0 ≈ **−5.1 px/tick**, g ≈ **0.24–0.25 px/tick²**, đỉnh cao ~55 px sau ~21 tick | `proj_track.py axe` khớp parabol 2 quỹ đạo sạch (dư max 2.6–4.4 px) |
| Nước thánh (video boss) | vx ≈ 1.4–1.6 px/tick, cung thấp (lên 5–10 px), g ≈ 0.12–0.13 px/tick², chạm đất cách ~40–44 px sau ~26–28 tick; lửa giữ chân boss | f389–402, f403–417 |
| **Zombie** | **0.875 px/tick** đều | `enemy_speed.py zombie` 4 vết (f600–759), dư max ≤ 3.3 px; sprite ~12×25 |
| **Black Leopard** | nhảy: ngang ≈ 1.9–2.0 px/tick + cung rơi; **chạy 2.0 px/tick** | f1266–1287 (78 px/42 tick), f1788–1798, f1815–1856 (166 px/82 tick); sprite 32×13 |
| **Vampire Bat** | ngang ≈ **1.0–1.12 px/tick** về phía Simon, dao động dọc ±6–9 px, chu kỳ ~1.3–2 s (thô) | 4 vết (f2760–2803, f3030–3073, f3173–3219, f4839–4880) |
| **Fish Man nhảy khỏi nước** | vy ≈ −5.6 px/tick khi vượt mặt nước, g ≈ **0.124 px/tick²**, đỉnh ~129 px trên mặt nước (chân y≈85) sau ~46 tick, rồi rơi xuống lối đi | f3515–3547 khớp parabol (dư max 1.6 px). Tốc độ đi: không đo tin cậy (~0.8 px/tick, nhiễu) — để routine |
| Đồng hồ TIME | 1 đơn vị = 64.0 tick (1.065 s) | `time_rate.py` 5 đoạn |
| Tally | TIME→điểm 1 đơn vị/tick, +10/đơn vị; nghỉ 0.9 s; tim→điểm **1 tim/8 tick**, +100/tim; chờ 2.0 s rồi đen 0.43 s | HUD từng frame |
| Hồi máu sau quả cầu (video boss) | 1 vạch / ~7 tick (8→16 trong 0.9 s) | f627–654 |
| Thanh máu HUD giảm dần | ~1 vạch / 8 tick (mỗi rìu −2 vạch hiện thành 2 bước cách 3–6 frame video) | E-bar Map_1; cũng thấy P-bar 10→9→8 cách 6 frame |
| **Qua cửa cuối stage** | cuộn 128 px ở **0.90–0.91 px/tick** (~2.35 s) → dừng **2.30–2.33 s** (Simon tự đi 68 px, 1 px/tick) → cuộn 128 px; tổng ≈ 7.0 s; TIME đứng | `door_phases.py` (ghép 2 vùng sheet làm nền) |
| Vào cổng (cuối CE) | camera khoá cx 512; Simon chậm lại ~0.3 px/tick; khuất sau cổng ở x màn 191 | f486–549 |
| Quả cầu đỏ | xuất hiện **2.5 s sau khi boss chết** (2.27 s sau khi E=0) tại tâm (127.5,128.5), ~10×11 px, rơi g ≈ 0.23–0.26 px/tick² | cả 2 video (`orb_track.py`) |

---------------------------------------------------------------------------------------------------

## §5. Boss Phantom Bat

### 5.1 Hiệu chuẩn video boss
Xem bảng §1: **sx 3.6295, sy 3.1765, ox 169.68, oy 40.13** (sx/sy = 8/7), hiển thị NES y 10–229, đủ 256 cột.
Camera phòng boss cố định ở **PB cx 512** (= Map_1) → x màn hình + 512 = world x trong vùng PB.
Nhịp quay ≈ 20–22 fps khi có chuyển động. Định vị dơi bằng **trừ nền trung vị** (phòng tĩnh) + lọc cụm có ≥25 px đỏ, rộng ≥24 (`boss_track.py`);
Simon (bảng màu nâu/kem riêng của video này) bằng `simon_boss.py`. Toạ độ dơi = tâm bbox; bbox cánh mở 48×22, cánh khép ~31×23, khi ngủ 16×16.

### 5.2 Timeline video boss (0.5 s)

| t | Dơi (x,y tâm) | Tư thế | Simon (x, y chân) | P | E | Tim | Ghi chú |
|---|---|---|---|---|---|---|---|
| 0.0–2.0 s | (110.5, 64.5) | **ngủ** treo trần (sprite tối 16×16, bbox x103–118 y57–72) | (127,175)→(152,207)→(184,166) | 10 | 16 | 42→43 | Simon nhảy xuống sàn (0.6 s), nhảy lên khối gạch chân cầu thang (1.2 s), nhặt tim 1.5 s, lên cầu thang |
| 2.2 | (110,68) | **thức** (cánh mở) | (181,166) trên cầu thang | 10 | 16 | 43 | kích hoạt khi Simon x≈181–184 (world ≈ 693–696) |
| 2.5 | (110,68) | lượn tại chỗ | (184,166) | 10 | 16 | 43 | lượn 0.5 s |
| 3.0 | (134,91) | **lao xuống-phải** 1.5 px/tick, hướng +31° | (177,172) | 10 | 16 | 43 | |
| 3.5 | (174,108) | cuối cú lao | (175,175) | 10 | 16 | 43 | |
| 4.0 | (164,110) | **bay chậm sang trái** 0.77 px/tick | (191,161) | 10 | 16 | 43 | |
| 4.5 | (140,115) | bay chậm | (206,146) | 10 | 16 | 43 | |
| 5.0 | (118,118) | bay chậm | (204,148) | 10 | 16 | 43 | |
| 5.5–7.0 | (110,118) | **lơ lửng** 1.6 s (vỗ cánh tại chỗ) | (198,175)→(198,207) | 10 | 16 | 43→42 | Simon xuống sàn; 6.0 s ném nước thánh đầu; 6.8–7.0 s nhặt "II" (icon II từ 7.23 s) |
| 7.5 | (134,137) | **bổ nhào** về phía Simon 1.6 px/tick, ~34° | (194,207) | 10 | 16 | 42 | |
| 8.0 | (184,158) | **bay ngang** ~1.55 px/tick ngang tầm đầu Simon | (185,207) | 10 | 16 | 41 | |
| 8.5 | (228,156) | cuối đoạn ngang, quay lên | (186,207) | 10 | 16 | 40 | vượt qua Simon ~40 px |
| 9.0 | (193,121) | **vọt lên-trái** 2.0 px/tick | (187,207) | 10 | 16 | 40 | |
| 9.5 | (168,126) | **lượn chậm xuống-trái** 0.88 px/tick | (217,207) | 10 | 16 | 40 | |
| 10.0 | (148,138) | lượn chậm | (238,207) | 10 | 16 | 40 | |
| 10.5–11.0 | (124,152) | lơ lửng 0.5 s | (227,207) | 10 | 16 | 40→39 | |
| 11.5 | (169,175) | bổ nhào 1.55 px/tick (~27°) rồi bay ngang | (227,207) | 10 | 16 | 39 | |
| 12.0 | (214,190) | **chạm Simon 11.87 s** | (229,207) | 10→8 | 16 | 38 | **Simon trúng đòn −2 vạch**, knockback sang phải ~12 px |
| 12.5 | (219,180) | quay lên | (238,195) đang bật lùi | 8 | 16 | 38 | |
| 13.0 | (154,144) | **vọt lên-trái** 2.27 px/tick | (không thấy — nhấp nháy) | 8 | 16 | 37 | |
| 13.5 | (126,121) | đỉnh, đổi hướng | – | 8 | 16 | 36 | ném nước thánh 12.9–13.9 s (2 bình) |
| 14.0 | (148,140) | lượn chậm xuống-phải 0.98 px/tick | – (hiện lại 14.23 s ở (239,207)) | 8 | 16 | 36 | |
| 14.5–15.0 | (162,151) | lơ lửng 1.0 s | (239,207) | 8 | 16 | 36→35 | |
| 15.5 | (188,174) | **bổ nhào** 2.46 px/tick (+40°) vào vùng lửa | (231,207) | 8 | 16 | 35 | |
| 16.0 | (200,184) | **bị lửa nước thánh giữ chân** | (231,207) | 8 | 14 | 34 | E giảm từ 15.70 s |
| 16.5 | (200,184) | đứng yên trong lửa | (231,207) | 8 | 12 | 34 | |
| 17.0 | (204,187) | đứng yên | (233,207) | 8 | 8 | 34 | |
| 17.5 | (204,187) | đứng yên | (231,207) | 8 | 5 | 34 | |
| 18.0 | (215,190) | **chết 17.80 s** (+3000) | (232,207) | 8 | 1→0 (18.07) | 33 | thanh E tụt nốt |
| 18.5 | ~(190–215,165–190) | **chùm lửa chết** (6 ngọn, 2×3) tới ~18.77 s | (231,207) | 8 | 0 | 32 | |
| 19.0–20.0 | – | trống | (231,207)→(211,207) đi trái, 20.0 s bắt đầu nhảy (183,182) | 8 | 0 | 32 | |
| 20.5 | quả cầu | **quả cầu đỏ** hiện 20.33 s tại (127.5,128.5), rơi, chạm sàn 20.73 s | (153,175) đang nhảy về phía cầu | 8 | 0 | 32 | |
| 21.0–21.8 | – | – | nhặt cầu 20.83 s → đứng (138,207) | 8→16 | 0 | 32 | hồi máu 20.90–21.80 s; TIME đứng 0107 |
| 22.0–24.5 | – | – | (138,207) | 16 | 0 | 32 | chờ |
| 24.63→25.37 | – | – | (138,207) | 16 | 0 | 32 | đếm TIME 0107→0065 (1 đv/tick, +10 điểm) — video hết |

### 5.3 Đặc tả hành vi (rút từ cả 2 lần đánh)
1. **Ngủ**: treo ở trần, tâm (110.5, 64.5) màn hình (world PB x ≈ 622), sprite tối 16×16, không cử động.
2. **Kích hoạt**: khi Simon tới x màn ≈ 181–184 (world ≈ 693–696) — cả hai lần Simon đang ở trên cầu thang bên phải. Thức dậy → mở cánh, **lượn tại chỗ 0.5 s (~30 tick)**.
3. **Cú lao đầu tiên** giống hệt nhau ở 2 video: từ chỗ ngủ lao xuống-phải hướng ~+31° với **≈1.45–1.5 px/tick** tới ≈ (177,108) trong ~0.9 s.
4. **Bay chậm tới một điểm** với **0.75–1.0 px/tick** (gần như đường thẳng): sang trái tới (110,118) [boss] hoặc tới tận (27,120) [Map_1]; sau các cú bổ nhào là lượn chéo xuống tới (124,152), (158,147)… → điểm đến thay đổi (ngẫu nhiên hoặc phụ thuộc vị trí Simon).
5. **Lơ lửng** tại điểm đó **0.5–1.6 s** (vỗ cánh tại chỗ; bbox đổi giữa 48 và 32 px rộng, chu kỳ vỗ ~0.3 s quan sát được).
6. **Bổ nhào**: lao chéo xuống về phía Simon (~27–40°) với **1.5–1.6 px/tick** (có lần 2.46), tới **ngang tầm đầu Simon** (y tâm ≈ 152–190) thì **bay ngang** tiếp ~1.5 px/tick, vượt qua Simon 40–70 px.
7. **Rút lui**: vọt chéo lên-ngược lại **2.0–2.3 px/tick** (~30–45° so với phương ngang) lên cao (y ≈ 115–125), rồi quay về bước 4.
8. Chu kỳ bổ nhào ≈ **4.0–4.3 s** (bổ nhào ở 7.05, 11.05, 15.3 s trong video boss). **Không bắn đạn** (cả 2 video). Sát thương khi chạm: −2 vạch.
9. **Máu 16 vạch**. Rìu: mỗi lần trúng −2 vạch (8 lần trúng). Nước thánh: lửa **giữ chân** boss tại chỗ (2.4 s) và trừ liên tục (~1 vạch / 8 tick hiển thị).
   Map_1: khi trúng rìu liên tục ở (210,147) nó vẫn lơ lửng tại chỗ ~2 s (chỉ dao động do vỗ cánh).
10. **Chết**: điểm +3000 ngay lúc hết máu, thanh E tụt nốt; dơi thay bằng **chùm lửa 0.7–0.8 s**; **2.5 s sau khi chết** quả cầu đỏ hiện ở tâm (127.5,128.5) và rơi theo trọng lực (~0.24 px/tick²).
    Chạm cầu → TIME dừng; máu hồi đầy (1 vạch/~7 tick); sau 3.8–4.4 s bắt đầu tally.

### 5.4 Đối chiếu với trận boss trong Map_1 (3:17.4–3:33.0)
| | Map_1 | Video boss |
|---|---|---|
| Thức dậy | 3:19.43, Simon x 183 trên cầu thang | 2.2 s, Simon x 181–184 trên cầu thang |
| Vũ khí | **rìu + II**, máu 16 (không bị trúng) | **nước thánh + II**, máu 10 → trúng 1 lần còn 8 |
| Diễn biến | lao đầu (1.44 px/tick +31°) → bay chậm trái 3.2 s tới (27,120) → lơ lửng 1.3 s (ăn rìu) → bổ nhào về Simon → vọt lên-phải (1.76 px/tick) → lơ lửng (210,147) ăn rìu tới chết | lao đầu (1.50 px/tick +31°) → bay chậm trái 1.55 s → lơ lửng 1.6 s → 3 chu kỳ bổ nhào/rút → bổ nhào vào lửa nước thánh, bị giữ chân tới chết |
| Thời gian thức → chết | 10.97 s | 15.6 s |
| Giống | vị trí ngủ, điều kiện thức, cú lao đầu, tốc độ bay chậm (~0.77), +3000, chùm lửa ~0.75 s, quả cầu 2.5 s sau chết tại cùng điểm, TIME dừng khi nhặt cầu | |
| Khác | Map_1 nhặt cầu giữa không trung (máu đầy, không hồi); tally bắt đầu 4.4 s sau khi nhặt | video boss: cầu chạm sàn rồi mới nhặt, hồi máu 0.9 s, tally bắt đầu 3.8 s sau khi nhặt |

Bảng 0.5 s tự động cho Map_1: `work/map1_boss_table.md`; cho video boss: `work/boss_table.md`
(dòng 7.0 s của video boss bị nhận nhầm lửa/hộp "II" là dơi — giá trị đúng là (110,119) lơ lửng).

---------------------------------------------------------------------------------------------------

## §6. Điều không chắc / không đo được

- **Non-game Map_1**: f3233–3430 (1:47.77–1:54.33) — gồm 3 frame đen của game rồi trang YouTube + quảng cáo. Ngoài ra lớp phủ YouTube:
  nút ▶ f0–19, thanh điều khiển f0–9 và **f3431–3502** (che NES y ≈ 218–232), vài frame lẻ ở đáy f3531–4430 (thanh tiến trình hiện khi rê chuột — chỉ vài chục pixel ở đáy).
  Logo đỏ (watermark) luôn ở góc phải-dưới (NES x≈240–252, y≈224–231). Chuyển tầng CA→CB không quan sát được.
- **Map_1 có tỉ lệ 7/6 (4:3), video boss 8/7** — nếu routine giả định 8/7 cho Map_1 sẽ sai ~2% theo phương ngang.
- Thời điểm đơn lẻ lệch ±1 frame video (±2–3 tick) do capture ~21 fps có jitter; thời lượng từng pha roi/knockback chỉ ở mức ±2 tick.
- **Không có trong video**: Simon bị trúng đòn ở Map_1 (máu luôn 16) → knockback/nháy chỉ 1 mẫu (video boss, Simon gần như không thấy lúc nháy);
  roi trên cầu thang; Fish Man bắn lửa; Simon chết / mất mạng; nhặt nước thánh/đồng hồ (bỏ qua cả hai).
- Roi cấp 1 chỉ có 2 đòn và cấp 2 chỉ 1 đòn, đều đánh trúng chậu lửa (tia lửa trúng đòn có thể lẫn vào đầu roi, ±2 px). Tầm roi là tầm hình ảnh (sprite) — hitbox thật cần routine đối chiếu ROM/tài liệu.
- Quỹ đạo nhảy: đỉnh phẳng & thời gian bay 38–40 tick là từ 2 cú nhảy có đồng hồ camera; cú nhảy tại chỗ chỉ có đếm frame (36–42 tick).
- Tốc độ đi của Fish Man, chu kỳ dao động của Vampire Bat (cửa sổ ngắn, ~1.4 s) — chỉ thô.
- Đặc tả boss: điểm đến của pha "bay chậm" khác nhau giữa các lần — chưa rõ ngẫu nhiên hay theo vị trí Simon (chỉ 2 lần đánh). Pha "bay ngang" sau bổ nhào có thể là đường cong liên tục.
- Độ trễ từ lúc nhặt cầu tới tally khác nhau (4.4 s vs 3.8 s) — có thể theo nhạc; chưa kết luận.
- Đoạn Simon vào cổng CE: phần chậm ~0.3 px/tick có thể là "đi tự động" của game — chưa phân biệt được với người chơi nhả nút.
- Bộ đọc HUD tự động nhầm ô vũ khí phụ khi sprite Simon đè lên HUD (2:27.40–2:27.67) — đã ghi chú, giá trị đúng vẫn là dao.

---------------------------------------------------------------------------------------------------

## §7. Tệp đi kèm

**`ref/`** — 44 ảnh PNG **full frame gốc, không scale** (28 MB): áp công thức §1 trực tiếp lên toạ độ ảnh.
- Map_1 (34): `m1_00m01s_intro_cong_lau_dai`, `m1_00m02s_stage01_castle_entrance_bat_dau`, `m1_00m03s_roi_cap1_danh_chau_lua`, `m1_00m06s_nhat_roi_nhay_mau_dong_bang`,
  `m1_00m09s_roi_cap2_duoi`, `m1_00m15s_dao_gam_roi_tren_san`, `m1_00m16s_cong_lau_dai_camera_khoa_cx512`, `m1_00m20s_entrance_hall_zombie`, `m1_00m29s_roi_cap3_duoi_zombie`,
  `m1_00m30s_nhay_kem_di_chuyen_qua_dong_ho`, `m1_00m42s_black_leopard_nhay`, `m1_00m47s_len_cau_thang`, `m1_01m01s_black_leopard_chay_san`, `m1_01m15s_rosary_man_nhay_trang`,
  `m1_01m28s_cua_cuoi_stage1_mo_simon_di_qua`, `m1_01m32s_stage02_crypt_a_vampire_bat`, `m1_01m40s_tuong_vo_thit_nuong_diem100`, `m1_01m47s_nem_dao_gam_bay`,
  `m1_01m48s_NONGAME_trang_youtube`, `m1_01m54s_crypt_b_tro_lai_con_thanh_youtube`, `m1_01m57s_fishman_nhay_khoi_nuoc`, `m1_01m58s_fishman_di_tren_loi_di`,
  `m1_02m27s_simon_de_len_hud_cuoi_cau_thang`, `m1_02m52s_stage03_hanh_lang_phantom_bat`, `m1_03m00s_simon_vo_hinh_binh_thuoc`, `m1_03m17s_phong_boss_doi_dang_ngu`,
  `m1_03m19s_doi_thuc_day_nhat_II`, `m1_03m25s_riu_trung_doi_E_giam`, `m1_03m30s_doi_chet_khoi_lua`, `m1_03m32s_qua_cau_do_roi`, `m1_03m38s_tally_dem_time`,
  `m1_03m45s_tally_dem_tim`, `m1_03m53s_ban_do_lau_dai_truot_vao`, `m1_03m59s_stage04_bat_dau` (.png).
- Boss (10): `boss_01.0s_simon_vao_phong_doi_ngu`, `boss_02.2s_doi_thuc_day`, `boss_04.0s_doi_bay_cham_sang_trai`, `boss_07.5s_doi_lao_xuong_simon`, `boss_09.0s_doi_bay_len_lui`,
  `boss_12.2s_simon_trung_don_knockback`, `boss_16.5s_nuoc_thanh_chay_doi_trung`, `boss_18.2s_doi_chet_luoi_lua`, `boss_20.5s_qua_cau_do_roi`, `boss_21.5s_nhat_cau_hoi_mau` (.png).

**`tools/`** — script Python (chạy từ thư mục `video/` bằng `python -P -E tools/<tên>.py …`; mỗi file có dòng "Chay:" ở đầu). Chạy lại toàn bộ: `sh tools/run_all.sh`.
- Nền tảng: `nes_common.py` (hệ số CALIB, đọc frame qua ffmpeg pipe, lấy mẫu lưới NES, vùng sheet, ZNCC), `extract_nes.py` (giải mã cả video → `work/<video>_nes.npy` 256×224/frame).
- Hiệu chuẩn: `calib_hud.py`, `refine_calib2.py`, `verify_calib.py`.
- HUD: `hud_cells.py`, `hud_glyphs.py`, `hud_read.py` (→ `work/*_hud.csv`), `hud_events.py`, `time_rate.py`, `overlay_scan.py` (lớp phủ YouTube), `dup_frames.py` (frame lặp).
- Camera/Simon: `camera_track.py` (vùng + cx), `simon_track.py`, `simon_states.py`, `measure_walk.py`, `measure_jump.py`, `jump_camclock.py`, `measure_stairs.py`,
  `fg_map.py` (trừ nền bằng sheet), `whip_track.py`, `whip_reach.py`, `whip_levels.py`, `door_camera.py`, `door_phases.py`, `segment_summary.py`.
- Quái/vật thể: `enemy_speed.py`, `redenemy_track.py`, `small_movers.py`, `proj_track.py` (dao/rìu/nước thánh), `orb_track.py`.
- Boss: `boss_track.py`, `simon_boss.py`, `boss_segments.py`, `boss_speed.py`, `boss_table.py`, `plot_boss.py`.
- Xem nhanh: `pf_sheets.py`, `nstrip.py`, `strip.py`, `attack_strip.py`, `event_triplets.py`, `plot_track.py`, `save_refs.py`.

**`work/`** — dữ liệu trung gian (CSV mỗi frame: `map1_hud.csv`, `map1_camera.csv`, `map1_simon.csv`, `map1_whip.csv`, `map1_states.csv`, `boss_hud.csv`, `boss_bat.csv`,
`map1_bat.csv`, `boss_simon2.csv`, `*_dup.npy`, `notes.txt` (ghi chép quan sát)). `map1_nes.npy` (1.2 GB) và `boss_nes.npy` (124 MB) là bộ đệm frame NES —
có thể xoá, tạo lại bằng `extract_nes.py` (~90 s). `frames/` = ảnh nháp (contact sheet, dải zoom), có thể xoá.
