# Chỉ mục timeline + hiệu chuẩn video tham chiếu — Castlevania NES Map 3 (Block 3: Stage 07–09)

> **Bản ĐO (routine `cvm3-02`, 21/10/2026)** — viết lại từ bản planner 17/10: hiệu chuẩn tinh, HUD/camera/Simon từng frame (30 fps gốc), chuyển cảnh theo tick, sự kiện, trúng đòn, âm thanh.
> Quy ước: `f` = frame video @30 fps ⇒ **tick NES ≈ 2·f** (60 Hz), nhưng video chỉ có **~22.7 khung mới/s** (Map_3) / **19.6** (boss) ⇒ mốc đơn lẻ **±2 tick**, thời lượng ≥ 60 tick lấy bằng hồi quy/đồng hồ TIME (1 đơn vị TIME = 64 tick).
> Dữ liệu từng frame (commit): `ref/data/` — `map3_hud.csv`, `map3_camera.csv`, `map3_simon.csv`, `boss3_*.csv`, `map3_events.txt`, `map3_hits.csv`, `map3_transitions.txt`, `*_audio_cues.txt`, `*_music_seg.txt`, `calib_*.txt`, `dup_*.txt`. Công cụ: `Tools/cvm3/*3.py` (cách chạy §8). Cache nặng (không commit): `_cache/02/work/` (`map3_nes.npy`, blob, LUT…), ảnh `_cache/shots/02/`.
> **Đính chính planner (đo được):** (1) **PLAYER không giảm lần nào trong Map_3** (16/16, P-04 ở 7501/7501 frame hud_ok); (2) vật nhặt sau Mummy là **II (nhân đôi)** — không phải III; (3) video Map_3 ≈ 22.7 khung/s (planner 18), boss ≈ 19.6 (planner 21.7); (4) Mummy: ENEMY bar **dùng chung 16 vạch cho cả 2 con**; (5) túi +1000 St08 nhặt ở **mút trái cầu (x ≈ 13)**.

## §1. Hai video + hiệu chuẩn tinh

| | `Map_3.mp4` | `Boss_Map_3_Behaviour.mp4` |
|---|---|---|
| Đường dẫn | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_3.mp4` | `…\Boss_Map_3_Behaviour.mp4` |
| Thời lượng / khung | **4:11.61** = 7548 frame, **1488×760**, 30 fps, AAC 48 kHz | **42.73 s** = 1282 frame, **1370×772**, 30 fps |
| Nội dung | Tiếp đúng Map_2: bản đồ B2 → STAGE 07 → 08 → 09 → Mummy Men → tally → bản đồ B3 (STAGE 10) | Playthrough khác (SCORE 016900, P-01, tim 07, **dao găm**), **bảng màu khác**, watermark (§1b) |
| Khung game mới/s | **22.7** (24.4 % khung lặp; chuỗi lặp 1:4384, 2:1157, 3:102, 4:44…; theo 10 s: 9.8–25.0, thấp ở đoạn tally vì khung đứng) | **19.6** (34.6 % lặp; 17.4 ở 10 s đầu) |
| Tỉ lệ pixel | 7/6 | 8/7 |

### §1a. Hiệu chuẩn tinh (`x_v = ox + sx·(x_NES + ½)`, `y_v = oy + sy·(y_NES + ½)`; chỉ số pixel video = round)
| Video | sx | sy | ox | oy | Cách đo | Điểm |
|---|---|---|---|---|---|---|
| Map_3 | **3.862** | **3.310** | **240.044** | **−21.92** | `Tools/cvm3/calib_fine3.py` — **15 frame** (WEA ×3 cx 512/205/0, WEB ×3 cx 44/296/512, WalkwayA ×2 cx 0/55, WalkwayB ×3 cx 6/112/512, MummyMan ×4 cx 0/636/1062/1280), cx tinh ±2 mỗi frame, vòng mịn ±0.001 / ±0.1 px | **ZNCC TB 0.9728** (14/15 frame ≥ 0.94; mọi frame đã loại bị che nhiều bởi sprite) |
| Boss_Map_3 | **3.845** | **3.372** | **195.76** | **−22.392** | `calib_boss3.py` — **8 frame** (t = 14/16/20/24/26/30/32/38 s), ZNCC trên độ lớn **gradient** (bảng màu khác), vùng nửa trên phòng Mummy cx 1280 (NES y 48–128) | **ZNCC gradient TB 0.9431** (0.9419–0.9449) |
Sai số (từ độ nhạy ΔZNCC < 0.001): Map_3 sx ±0.003, sy ±0.006, ox ±0.3, oy ±0.3 video-px (≈ ±0.1 px NES); boss sx ±0.003, sy ±0.003, ox ±0.2, oy ±0.2. So planner: Map_3 sx +0.003, ox −0.9, oy 0; boss sy +0.009, oy −0.62 (tất cả < 1 px NES). `CALIB` đã cập nhật trong `Tools/cvm3/nes3.py` (+ bản `_cache/tools`, `ref/tools`). Dòng sheet r ↔ **NES y = 32 + r**.
**Hàng NES nhìn thấy:** y 0–239 đều nằm trong khung (không cắt) ở Map_3; HUD y 8–47, playfield y 48–223. Lưới trích xuất = NES y 8…231.
### §1b. Watermark
- **Map_3**: biểu tượng "Download this video" nhỏ **góc trên-phải ngoài/sát mép khung NES** (x ≳ 245, y ≈ 8–24 — xem HUD crop: ô xám cạnh "STAGE 07"). Không che chữ số HUD (đọc 7501/7501 frame hud_ok, 2 frame có '?' ở P).
- **Boss_Map_3**: chữ mờ **"OVERTIME GAMER"/"by AND_…" giữa HUD (NES y ≈ 16–30, x ≈ 80–180)** — đọc số vẫn được (0 '?'), vạch PLAYER/ENEMY ổn; "Support me on ko-fi" **góc dưới-trái (NES y ≈ 214–222, x ≈ 0–90)** nằm sát hàng sàn — các script boss chỉ dùng y 48–213; 2 dải mờ hai bên ngoài khung. 0.0–1.1 s = fade-in từ khung bản đồ (HUD STAGE 07 SCORE 009840 — bỏ qua); HUD ổn từ f34 (1.13 s).

## §2. Vùng sheet Level 3 + camera từng frame
Vùng sheet: như planner (`inventory/level3_walkway.json`; WEA/WEB/WalkwayA/WalkwayB rộng 768, **MummyMan 1536**; 125 tile 8×8 / 83 metatile). Chi tiết dữ liệu `map3_camera.csv` (cột `frame,t,kind,section,cx,score,cxw`; `section` = `WEB>WA`, `WB>MM` khi camera đang cuộn qua cửa PHẢI — vùng ghép, `cxw` = tọa độ ghép 512…768).

| Frame (giây) | kind/vùng | cx | Ghi chú |
|---|---|---|---|
| f0–193 (0.00–6.43) | `other` = bản đồ B2 | — | §3a |
| f194–206 (6.47–6.87) | đen | — | **26 tick** (13 f) |
| **f207**–1186 (6.90–39.53) | `WalkwayEntranceA` | 512 → 0 | STAGE 07 TIME 0500 hiện f207 |
| f1187–1197 (39.57–39.90) | đen | — | thang lên-trái WEA→WEB: **22 tick** |
| **f1198**–2439 (39.93–81.30) | `WalkwayEntranceB` | 0 → 512 | |
| f2440–2644 (81.33–88.13) | `WEB>WA` (cửa phải #1) | cxw 516 → 766 | §3d |
| **f2645**–3262 (88.17–108.73) | `WalkwayA` | 0 → 512 | STAGE 08 từ f2645 |
| f3263–3274 (108.77–109.13) | đen | — | thang lên-trái WA→WB: **24 tick** |
| **f3275**–4337 (109.17–144.57) | `WalkwayB` | **−112** … 512 | cx đầu −33 |
| f4338–4544 (144.60–151.47) | `WB>MM` (cửa phải #2) | cxw 516 → 767 | |
| **f4545**–7451 (151.50–248.37) | `MummyMan` | 0 → **1280** | STAGE 09 từ f4546 |
| f7452–7463 (248.40–248.77) | đen | — | 22 tick |
| f7464–7547 (248.80–251.57) | `other` = bản đồ B3 | — | trượt vào; hết video |
ZNCC camera: TB 0.97 (min 0.74 ở đoạn WalkwayB đầu/đệm trời; ≥ 0.93 hầu hết). Không section nào vượt mép sheet: cx ∈ [0, 512] cho WEA/WEB/WalkwayA; WalkwayB [**−112**, 512]; MummyMan [0, 1280].
### §2a. WalkwayB cx ÂM (cho routine 05) — ảnh `ref/nes/cvm3_02_walkwayB_cx_am_cot_trai.png`, `…_vao_va_cx_am.png` (đã xem)
- Camera **bám Simon không kẹp trái**: cx = x_Simon − 128 → nhỏ nhất **−112** (f3483, 116.10 s; Simon đứng mút trái cầu x ≈ 14–16 sau khi nhặt túi). Vào section ở **cx −33** (f3275), đi trái tới −94 (f3336), quay về −63, về +6 (f3408–3425), lại trái tới −112 (f3483), về +5 (f3545), rồi đi phải (cx +84 ở f3624…).
- **Nội dung cột trái (x −112…−1 thế giới): trời xanh thuần (36,24,140)** — không đầu cầu, không vật. **Mặt cầu (gạch cam 16×16) bắt đầu đúng x = 0** (cạnh trái 2 khối cam ở màn x = 112 khi cx −112); **vách đá gập ghềnh phía dưới thò ra tới x ≈ −32…0** (hàng y ≈ 160–208, bị sheet cắt — vài tile đá rêu/mép răng cưa); trời xanh hoàn toàn trái x < −32. ⇒ routine 05 nới `WalkwayB` ≥ 112 cột trời ở trái (+ 4 cột đá mép), chép tile trời/đá CÓ SẴN ở sheet.
- **Bí mật túi tiền St08**: Simon **không ngồi** — đi tới mút trái cầu; **+1000 tại f3478 (1:55.93)** khi Simon ở x ≈ 14 (cx −112, mép trái cầu x = 0 ngay dưới chân); chữ "1000" hiện sát phải Simon (ảnh `walkwayB_cx_am_cot_trai`). Quanh đó Raven/chuỗi +100 (xem §4).
## §3. Chuyển cảnh theo tick (`ref/data/map3_transitions.txt` — `Tools/cvm3/trans3.py`)
### §3a. Bản đồ lâu đài B2 → đen → STAGE 07 (mẫu thứ 2 của bản đồ B2; ảnh `cvm3_02_map3_ban_do_b2_f0_208.png`)
- Bản đồ cam **trượt vào từ PHẢI sang TRÁI**: mép trái x = 231 (f4) → **≈ 6 (dừng f81 = 2.70 s)**, tốc độ **−1.503 px/tick** (3.007 px/frame video; dư ≤ 2.4 px) — đã chạy từ trước f0 (video cắt giữa chừng). 0.0–0.9 s có nút ▶ YouTube đè giữa khung (không thuộc game).
- Simon nhỏ hiện **f82 (2.73 s)** x ≈ 17 (đáy bản đồ, y ≈ 195–215), đi phải **+1.004 px/tick** (dư ≤ 2.3 px; x(f182) ≈ 219 → ≈ 235 lúc dừng). Tim nhấp nháy ở tháp phải; chấm tím nhỏ ở lâu đài từ f117. HUD khi bản đồ: SCORE 044670, TIME 0000, STAGE 07, P-04, tim 05.
- Đen **f194–f206 = 26 tick** (6.47–6.87 s); **STAGE 07 + TIME 0500 hiện f207 (6.90 s)**, ENEMY 16 vạch ngay f207; **Simon khởi đầu WEA cx 512: x màn 208 (wx 720), mặt sàn Y 192, `idle`** (planner 717). Nhạc: im 0–2.95 s, nhạc bản đồ ~3.0–6.5 s, im 6.50–6.85 s, **Wicked Child bắt đầu t0 = 6.862 s** (= f206/f207 ±0.05 s) — **0 tick trước khi hiện** (CVM2 nghi ~24 tick: video này không có).
### §3b. Đổi tầng thang LÊN-TRÁI (WEA→WEB; WA→WB) — `trans3.py` mục 5
| | WEA → WEB | WalkwayA → WalkwayB |
|---|---|---|
| Đen | f1187–1197 (**22 tick**), 39.57–39.90 s | f3263–3274 (**24 tick**), 108.77–109.13 s |
| Trước | Simon leo thang tới đỉnh: cx 0, x màn 106→100, Y mặt sàn 70→66 (`stairs_up` → `walk` lên sàn tầng trên), camera đã cx 0 | cx 490→482, **Simon cố định giữa màn x 128**, Y 71→63, `stairs_up` |
| Sau | `WalkwayEntranceB` **cx 0**, Simon hiện ở **chân thang trái-dưới** x màn 96→87, Y mặt sàn **221→212**, đi `walk_1` rồi `stairs_up` lên-trái | `WalkwayB` **cx −33**, Simon ở giữa màn (x thế giới ≈ 95) chân thang dưới (Y ≈ 195), leo lên-trái; camera sau đó cx −34…−94 |
| Phép dời | section WEA (x 100–106 + 0, y 66–70) → WEB (x 87–96, y 212–221): Simon dời **≈ −10 px ngang, **+150 px dọc** (xuống đáy màn)** | (x ≈ 482+128 = 610, Y 63) → (x ≈ 95, Y ≈ 195): **−515 ngang** (camera nhảy cx 482 → −33), **+130 dọc** |
| HUD | TIME/SCORE không đổi, STAGE không đổi | như trái |
Không có hồi `invincible`/nhấp nháy quanh đen (HUD PLAYER 16). **Chưa đo Simon từng frame ngay sau đen WalkwayB** (khe f3275–3320 Simon không khớp mẫu — bị che bởi thang/đá, §6).
### §3c. Cửa PHẢI #1 (WEB→WalkwayA, STAGE 07→08) & #2 (WalkwayB→MummyMan, STAGE 08→09)
| Pha | Cửa #1 (WEB cx 512) | Cửa #2 (WalkwayB cx 512) |
|---|---|---|
| TIME đứng | **TIME 0432 từ f2412 → f2673 (1:20.40–1:29.10, 522 tick)**, tiếp 0431 f2680 | TIME 0380 f4326 → f4579 (2:24.20–2:32.40, ~508 tick), tiếp 0379 f4580 |
| Cuộn 1 | f2441–2502 (**≈ 122 tick**), cxw 516 → 640 (+124 px), **0.993 px/tick** | f4339–4401 (**≈ 126 tick**), cxw 516 → 640, **1.00 px/tick** |
| Dừng | f2503–2581 (**≈ 158 tick**) cxw = 640 (cánh cửa mở → Simon tự đi **+74 px** (x ghép 742 → 816 nằm trong vùng không khớp) → cửa đóng) | f4402–4482 (**≈ 162 tick**) cxw = 640 (Simon x ghép 740 → 813) |
| Cuộn 2 | f2582–2644 (**≈ 126 tick**), cxw 640 → 766, **1.014 px/tick** | f4483–4544 (**≈ 124 tick**), cxw 640 → 767, **1.010 px/tick** |
| STAGE +1 | **f2645** (cuối cuộn 2 +1 frame) — 1:28.17 | **f4546** (cuối cuộn 2 +2 frame) — 2:31.53 |
| Tổng cuộn-bắt-đầu → STAGE | **≈ 408 tick** (f2441→f2645) | **≈ 414 tick** (f4339→f4546) |
| Điểm vào stage sau | WalkwayA cx 0; **Simon x 48, mặt sàn Y 160**, `idle` quay phải, tự đi tiếp (f2645: x 48 → f2663: 74) | MummyMan cx 0; **Simon x ≈ 60, Y 128** (f4570, `walk`; trước đó trong cửa x ghép 813, Y 123) |
Mỗi lần cuộn dài **128 px** (512 → 640 → 768), 1 px/tick, dừng ở giữa (cửa mở/đóng). So `Map1FlowTiming`/`Map2FlowTiming`: routine 11 đối chiếu. **Hai cửa lệch nhau 6 tick** (408 vs 414), TIME đứng 522 vs 508 (cùng cỡ CVM2 §5 #15). Ảnh: `ref/nes/m3_1m22…cua_phai_1_cuon`, `m3_1m24…mo`, `m3_2m26…cua_phai_2_mo`.
### §3d. Khoá camera phòng Mummy
Không có "khoá" tức thời: camera bám Simon rồi **kẹp ở cx = 1280** (Simon wx ≥ 1408). cx đạt 1280 lần đầu **f5931 (3:17.70)**; trước đó cx 1191 (f5820) → 1263 (f5890–5920 đứng — Simon wx 1391) → 1279 (f5930). Sau khoá Simon đi lùi tới wx 1383 (< 1408) mà cx **không giảm** ⇒ khóa trái thật (cx min 1280). Tốc độ cuộn ≈ 0.2 px/tick (bám Simon). Mummy đứng yên ở sàn phòng từ f≈5600 (cx ~1100), 2 con thấy ở sàn phòng trước khóa; **Wicked Child → Poison Mind (bài 10) đổi ở ≈ 198.9 s ±0.3 (f≈5967, ≈ 75 tick sau cx 1280)**; boss video: khóa f374 (12.47 s) → Poison Mind ≈ 13.3 s (~50 tick sau).
### §3e. Mummy chết → quả cầu → nhặt → tally → STAGE 10 → bản đồ B3 (ảnh `cvm3_02_map3_mummy2_chet_qua_cau_nhat.png` đã xem)
| Mốc | Frame / giây | Ghi chú |
|---|---|---|
| ENEMY bar bắt đầu giảm | f6126 (3:24.20) = 16→15 | giảm đều tới 0 ở f6771; **1 vạch chung cho 2 Mummy** |
| Mummy 1 chết | **f6686 (3:42.87) +3000** (ENEMY 3→2 ở f6678) | cháy đỏ (tia) |
| Mummy 2 chết | **f6771 (3:45.70) +3000** (ENEMY 1→0) | +3000 **mỗi con** (wiki chung — theo video, H1) |
| Quả cầu hiện (cao giữa phòng x ≈ 127) | **f≈6846 ±6 (3:48.2)** ≈ **150 tick** sau Mummy 2 (boss video ≈ 120 tick: hiện f≈1062–1069, 35.4–35.6 s) | rơi ~24 frame (≈ 48 tick) tới sàn y ≈ 208 (f6876); cùng lúc vật **II** rơi bên phải (icon HUD II từ f6864) |
| Nhặt quả cầu | **f6920 (3:50.67)** (ảnh đếm pixel cầu biến mất f6919→6920); TIME đứng 0306 từ f6907; HUD icon II nhấp nháy f6864–6920 rồi giữ; **nhạc Stage Clear (bài 12) bắt đầu t0 ≈ 230.4–230.7 s** (khớp pickup ±0.1 s) | |
| Chờ trước đếm | f6920 → f7033 (TIME 0304) = **226 tick** (Map_2 224, Map_1 260) | |
| Đếm TIME | f7033 → f7186 (TIME 0000): **306 tick cho 304 đơn vị = 1.01 tick/đơn vị** (TIME −2 mỗi frame video; SCORE +20/frame = **+10 điểm/đơn vị**: 065970 → 069030 = +3060) | |
| Nghỉ | f7186 → f7213: **54 tick** (im 3:59.75–4:00.45) | |
| Đếm tim | f7213 (46→45) → f7392 (→00): **358 tick cho 46 tim = 7.78 tick/tim**, mỗi tim +100 (069030 → 073630) | |
| Sau tim cuối → đen | f7392 → f7453: **122 tick**; nhạc im từ 4:06.9 | |
| Đen | f7453–7463: **22 tick** | |
| HUD STAGE 10 | **f7464 (4:08.80)** STAGE 09→10, tim 00→05, TIME 0000 | |
| Bản đồ B3 trượt | f7464–7547 (**≈ 84 frame ≈ 168 tick**, 4:08.8–4:11.57) tới hết video (chưa dừng, chưa có Simon nhỏ) | |
## §4. Sự kiện Map_3 (≥ 160 dòng ở `ref/data/map3_events.txt` — HUD từng frame + vùng/cx + Simon wx; nghĩa suy theo GIÁ TRỊ SCORE theo wiki + ảnh đã xem)
Giá trị đã xác nhận bằng ảnh (`cvm3_02_su_kien_diem_0_55s.png`, `cvm3_02_tui_tien_st07.png`): **+100 = nhặt túi đỏ** (túi nằm sàn, thường rơi từ enemy vừa hạ/nến); **+700 = túi trắng** (f279, 9.30 s, WEA cx 512, Simon wx 729); **+300** Skeleton (f1078 trên thang, f1553) / pink "Ghost" (f453, 15.10 s); **+500 Fleaman** (f545, f675, f918 — Fleaman đỏ sẫm; **mỗi lần hạ Fleaman rơi túi đỏ** (f541 thấy túi tại chỗ chết)); **+200 Raven** (f1626 54.20 s); **+1000 túi bí mật**: St07 f2054 (1:08.47) — túi hiện ở bục cao trái WEB **trước f1980 (66.0 s)**, Simon leo thang rồi nhặt; St08 f3478 (1:55.93) §2a; +3000 Mummy ×2. Hiện không có đồng hồ/bí mật/nến khác trong video mang điểm khác.
**Vũ khí phụ**: đồng hồ (HUD `watch`) từ đầu → **nước thánh f491 (16.37 s)** (nhặt ở WEA cx 424, lọ thấy ở sàn f420) → giữ tới hết. Icon **II** (nhân đôi) chỉ từ f6864. Tim: 05 đầu video → 06 f377 → +5 (tim lớn) f555 (18.50 s) → … 55 ở khóa → dùng nước thánh −1/lần (3:22–3:44: 55 → 46) → 46 vào tally. **Không có lần nào PLAYER giảm; P-04 suốt; không rơi hố.**
**Hạ enemy (29 lần: điểm 200–500 + Mummy 3000) + túi nhặt kế tiếp ≤ 15 s** (bảng này cho routine 08; Y = mặt sàn Simon):
| t | f | vùng/cx | Simon wx,Y | TIME | điểm | túi/vật kế tiếp |
|---|---|---|---|---|---|---|
| 0:15.10 | f453 | WalkwayEntranceA/465 | - | 0493 | +300 | +100 @0:20.63 (+5.5 s) |
| 0:18.17 | f545 | WalkwayEntranceA/405 | wx533,Y208 | 0490 | +500 | +100 @0:20.63 (+2.5 s) |
| 0:22.50 | f675 | WalkwayEntranceA/277 | wx405,Y208 | 0486 | +500 | - |
| 0:30.60 | f918 | WalkwayEntranceA/169 | wx297,Y208 | 0478 | +500 | - |
| 0:35.93 | f1078 | WalkwayEntranceA/0 | wx94,Y128 | 0473 | +300 | +100 @0:48.60 (+12.7 s) |
| 0:51.77 | f1553 | WalkwayEntranceB/134 | wx263,Y192 | 0459 | +300 | +100 @0:52.10 (+0.3 s) |
| 0:54.20 | f1626 | WalkwayEntranceB/234 | wx366,Y192 | 0457 | +200 | +100 @1:03.23 (+9.0 s) |
| 0:56.07 | f1682 | WalkwayEntranceB/296 | wx424,Y124 | 0455 | +200 | +100 @1:03.23 (+7.2 s) |
| 0:57.73 | f1732 | WalkwayEntranceB/296 | - | 0454 | +200 | +100 @1:03.23 (+5.5 s) |
| 1:21.10 | f2433 | WalkwayEntranceB/512 | wx735,Y144 | 0432 | +400 | - |
| 1:42.43 | f3073 | WalkwayA/512 | wx648,Y160 | 0419 | +300 | +1000 @1:55.93 (+13.5 s) |
| 2:01.47 | f3644 | WalkwayB/84 | wx212,Y160 | 0402 | +200 | +100 @2:04.27 (+2.8 s) |
| 2:07.90 | f3837 | WalkwayB/185 | wx313,Y160 | 0396 | +400 | +100 @2:14.27 (+6.4 s) |
| 2:10.57 | f3917 | WalkwayB/303 | wx431,Y127 | 0393 | +200 | +100 @2:14.27 (+3.7 s) |
| 2:11.90 | f3957 | WalkwayB/331 | wx459,Y126 | 0392 | +200 | +100 @2:14.27 (+2.4 s) |
| 2:16.53 | f4096 | WalkwayB/439 | wx567,Y160 | 0388 | +400 | - |
| 2:36.67 | f4700 | MummyMan/87 | wx215,Y92 | 0376 | +200 | +100 @2:39.27 (+2.6 s) |
| 2:38.17 | f4745 | MummyMan/178 | wx306,Y93 | 0374 | +200 | +100 @2:39.27 (+1.1 s) |
| 2:41.80 | f4854 | MummyMan/228 | - | 0371 | +400 | +100 @2:46.47 (+4.7 s) |
| 2:43.47 | f4904 | MummyMan/315 | wx443,Y93 | 0369 | +200 | +100 @2:46.47 (+3.0 s) |
| 2:44.43 | f4933 | MummyMan/373 | wx501,Y96 | 0368 | +200 | +100 @2:46.47 (+2.0 s) |
| 2:49.23 | f5077 | MummyMan/487 | wx615,Y128 | 0364 | +400 | +100 @3:01.07 (+11.8 s) |
| 2:50.77 | f5123 | MummyMan/572 | wx700,Y92 | 0362 | +200 | +100 @3:01.07 (+10.3 s) |
| 2:51.70 | f5151 | MummyMan/609 | wx737,Y128 | 0361 | +300 | +100 @3:01.07 (+9.4 s) |
| 3:02.50 | f5475 | MummyMan/993 | wx1121,Y128 | 0351 | +300 | +100 @3:03.17 (+0.7 s) |
| 3:03.70 | f5511 | MummyMan/993 | wx1121,Y128 | 0350 | +500 | - |
| 3:10.10 | f5703 | MummyMan/1118 | - | 0344 | +300 | - |
| 3:42.87 | f6686 | MummyMan/1280 | wx1302,Y144 | 0313 | +3000 | - |
| 3:45.70 | f6771 | MummyMan/1280 | wx1319,Y160 | 0311 | +3000 | +100 @4:00.43 (+14.7 s) |
Nhận xét luật rơi vật (cho 08): sau mỗi lần hạ thường thấy +100 (túi đỏ nhặt) 0.3–15 s sau; không phải 100 % (6/29 không có +100 trong 15 s — Simon bỏ qua / rơi hố ngoài màn). **Ghi chú:** loại enemy theo điểm (wiki): +200 = Raven, +300 = Skeleton/Ghost/Medusa Head, +400 = Bone Tower, +500 = Fleaman; hình ảnh xác nhận ở các điểm đã nêu; phần còn lại là suy theo giá trị (planner mô tả: Medusa Head 94–104 s WalkwayA nhưng không thấy +300 tương ứng — **Simon không hạ Medusa Head nào** hoặc bỏ qua; Bone Tower ×≥2 WalkwayB 124–134 s ⇒ +400 ×3: f3837, f4096, f4854 (St09 2:41.8)).
**Sự kiện theo section (tóm tắt, thời điểm đo):**
| Giây | Section | Sự kiện |
|---|---|---|
| 6.90 | WEA cx 512 | STAGE 07 TIME 0500; 9.30 túi trắng +700; 11.10 túi đỏ +100 |
| 14.0–15.1 | WEA | lọ nước thánh ở sàn (x màn ≈ 25); "enemy hồng" = **Ghost** (bay) hạ +300 (15.10) |
| 16.37 | WEA | nhặt nước thánh (HUD đổi) |
| 18.17, 22.50, 30.60 | WEA cx 405/277/169 | **Fleaman ×3 +500** (Simon wx 533/405/297) |
| 35.93 | WEA cx 0 | Skeleton +300 (trên thang trái) |
| 36–39.5 | WEA | leo thang lên-trái; đen 39.57–39.90 |
| 51.77–57.73 | WEB | Skeleton +300 (51.77), Raven +200 ×3 (54.20, 56.07, 57.73) |
| 66.0–68.47 | WEB cx ~100–300 | túi bí mật St07 hiện (trước 66.0), nhặt +1000 (68.47) |
| 81.10 | WEB cx 512 | +400 (Bone Tower?) trước cửa; cửa #1 §3c |
| 88.17 | WalkwayA cx 0 | STAGE 08 (TIME 0432) |
| 100.8–103 | WalkwayA | +100/+300/+1000(cộng dồn 1100 @1:42.70) — lọ/đồng hồ trên thang |
| 109.2–118.8 | WalkwayB cx −112…+84 | túi bí mật St08 (+1000 @115.93 — **mút trái cầu**) §2a |
| 121–136 | WalkwayB | Raven +200 (2:01.47), Bone Tower +400 (2:07.90, 2:16.53), Raven +200 ×2 (2:10.57, 2:11.90) |
| 151.5 | MummyMan | STAGE 09; Raven +200 (2:36.67, 2:38.17), +400 (2:41.80) |
| 163–173 | MummyMan cx ~300–600 | Raven/Bone Tower +200/+400, Skeleton nhảy qua khe cầu hỏng (+300 @2:51.70) |
| 182–190 | MummyMan cx ~990–1120 | Skeleton +300 (3:02.50), Fleaman +500 (3:03.70), +300 (3:10.10) |
| 197.70 | MummyMan | cx đạt 1280 (§3d) |
| 204–225 | phòng Mummy | Simon trên khối trái ném nước thánh; ENEMY 16→0; tim 55→46 |
| 222.87 / 225.70 | | Mummy 1 / 2 chết +3000 ×2 |
Ghi chú: Simon trong phòng Mummy ở **bậc/khối bên TRÁI** (wx 1302–1319, Y 144–160) khi Mummy chết; Mummy hồng/đỏ khi trúng (xem boss).
## §5. Video boss (`Boss_Map_3_Behaviour.mp4`) — theo frame (HUD `boss3_hud.csv`, Simon `boss3_simon.csv`: khớp 605/1282 frame)
| Frame (giây) | Sự kiện |
|---|---|
| f0–33 (0–1.1) | khung bản đồ + fade-in; HUD ổn từ f34: **STAGE 09 TIME 0240 SCORE 016900 PLAYER 4/16 (đã trúng từ trước; hồi tới 10 ở f302–322), ENEMY 16, tim 07, dao găm** |
| f34–372 | camera cx 1163 → 1206 → 1243 → 1214 → 1275: camera **không khoá** tới f374 (12.47 s) rồi **kẹp cx 1280** (Simon wx ≥ 1408); Simon phá khối thấp nhất bậc thang trái ⇒ thịt nướng; ăn → **PLAYER 4→10 (+6 vạch) f302–322** (1 vạch/4 frame ≈ 8 tick) |
| f90 (3.0) | icon vũ khí hiện dao găm (trước đó `occ` do dao bay) |
| f196 (6.53) | **+100 = nhặt túi đỏ** (ảnh `cvm3_02_boss_plus100.png`: túi sàn f192, chữ "100" f196) |
| f405 (13.5) | tim 07→08 |
| f452–1005 | **ENEMY 16→0 (16 vạch dùng chung)**: giảm ở f452, 507, 575, 640, 664, 690, 774, 823, 840, 851, 874, 903, 931, 956, 982, 1005 |
| f507, f640, f729, f837, f903 | **+100** trùng lúc Mummy nháy đỏ/hồng khi trúng roi/dao (ảnh `…boss_plus100.png`); 3/5 trùng ENEMY −1 — **chưa phân biệt được "đòn trúng Mummy +100" vs "phá băng gạc +100"** ⇒ câu hỏi cho routine 09 (Map_3 dùng nước thánh: 0 lần +100 khi trúng) |
| f542–549, f702–706, f798–802 | **Simon trúng đòn ×3 (−3 vạch mỗi lần: 10→7, 7→4, 4→1)** — `map3_hits.csv`, §6 |
| f848 (28.27) | Mummy 1 chết: **+3100 (= +100 đòn cuối + 3000)** |
| f1005 (33.50) | Mummy 2 chết **+3000** (SCORE 020600 → 023600) |
| f1062–1069 (35.4–35.6) | quả cầu hiện (x ≈ 128, y ≈ 100) rơi tới sàn f1093; ≈ 114–128 tick sau Mummy 2 (Map_3: ≈ 150 tick) |
| **f1223 (40.77)** | nhặt quả cầu: TIME đứng **0203**; **PLAYER hồi 1→15 trong f1223–1276** (~3.8 frame/vạch ≈ 7.6 tick/vạch; video hết lúc 15/16) |
## §6. Simon trúng đòn (`ref/data/map3_hits.csv`) — bằng chứng cho routine 04
- **Map_3: 0 lần** (PLAYER 16/16 suốt 7501 frame HUD hợp lệ; P-04 không đổi; khớp `hurt_0`/`death_0` trong `map3_simon.csv` đều err ≥ 0.18 = khớp sai, `conf=lo`). Map_2: 0 lần (CVM2 02); Map_1: mẫu boss CVGP 08.
- **Boss_Map_3: 3 lần, mỗi lần −3 vạch (10→7, 7→4, 4→1)** — **không phải 2** như Block 1–2 (wiki: sát thương theo stage ⇒ Block 3 = **3**). Vạch trừ **1 vạch / ≈ 3.5 frame (≈ 7–8 tick)**.
  | # | HUD | nguồn | knockback (Simon `hurt_0`) |
  |---|---|---|---|
  | 1 | f542 (18.07 s) | băng gạc Mummy trái, Simon quay **trái** (roi giăng trái f536) | x 202 → 228 (**+26 px** về PHẢI = ngược hướng mặt), 2 px/frame = 1 px/tick; Y sàn 207 → **đỉnh 194 (−13)** giữ f545–f553 (**≈ 18 tick dạng phẳng**), 195/197/202 rồi chạm sàn ≈ f558; **≈ 32 tick** từ f542; không rơi hố (sàn phòng liền) |
  | 2 | f702 (23.40 s) | băng gạc (sao nổ f690), Simon **ngồi sát tường phải** | **x đứng yên 239 (tường phải chặn)** suốt f700–713; Y 206 → **đỉnh 194 (−12)** giữ f702–f708 (~14 tick), đáp ≈ f715 (`use_stand` f730); ≈ 30 tick |
  | 3 | f798 (26.60 s) | băng gạc (sao f790), ngồi sát tường phải | Simon bị Mummy che f806–834 (không đo được), hiện lại f838; ≈ 30–40 tick |
- So Boss_Map_2 (CVM2 09 §2k) & CVGP 08: **cùng dạng** (lùi NGƯỢC hướng mặt, đỉnh −13/−14 giữ ~16–18 tick, ~32–36 tick, **bị tường chặn thì đứng yên nhưng vẫn nhảy lên**); quãng ngang không tường **26 px (Boss_Map_3 #1) vs 32 (Boss_Map_2 #3)** — hai mẫu không cùng giá trị (đo ±2 px, dup frame): 04 lấy khoảng 26–32 px/ ≈ 30–36 tick, KHÔNG phải 12 px.
- **Hồi máu** (ghi cho 03/04): thịt nướng **+6 vạch**, quả cầu hồi tới đầy ở ~7.6 tick/vạch; **không có bất tử nháy đo được** (Simon bị che/không phủ).
- Ảnh dải khung (đã xem): `ref/nes/cvm3_02_boss_hit1_dai_khung.png`, `…hit2…`, `…hit3…`, `…hit1_zoom.png`, `…hit2_zoom.png`.
## §7. Âm thanh (`ref/data/map3_audio_cues.txt`, `*_music_seg.txt` — `Tools/cvm3/music3.py ident/seg`, xcorr với 16 MP3 của CVM2)
- **Wicked Child (bài 05) bắt đầu t0 = 6.862 s** = f206/f207 (STAGE 07 hiện) ±0.05 s; **lặp mỗi 45.27 s** (đổi offset t0: 6.86 → 52.13 @74.9 s → 97.40 @120.3 s → 142.66 @165.3 s; mỗi lần +45.27) — không phát lại ở cửa/đổi tầng/STAGE (offset không đổi qua các mốc đó).
- **Poison Mind (bài 10)** thay Wicked Child ≈ **198.9 s** (±0.3; ≈ 1.2 s sau cx 1280); chu kỳ lặp **7.45 s**. Boss video: ≈ 13.3 s.
- **Stage Clear (bài 12)** t0 ≈ 230.4–230.7 s = nhặt cầu f6920 (230.67 s) ±0.1 s. Nhạc ngừng ở 3:59.75–4:00.45 (0.7 s giữa đếm TIME và tim) và im hẳn từ 4:06.9 tới hết (bản đồ B3 không nhạc ở video này).
- Nhạc bản đồ lâu đài B2 ~3.0–6.5 s (bài 09 lần đầu t0 = −3.4 s ⇒ nhận dạng yếu, ncc 0.50). Im 0–2.95 s.
## §8. Công cụ & cách chạy (từ gốc repo, `python -P -E`)
`Tools/cvm3/`: `nes3.py` (hiệu chuẩn, `padded_section`) · `calib_fine3.py <video> <t:section:cx> …` · `calib_boss3.py <t1> …` · `extract_nes3.py <map3|boss3>` (→ `_cache/02/work/<video>_nes.npy`) · `dup_frames3.py` · `camera_track3.py` · `hud_read3.py` / `hud_events3.py` / `hud_check3.py` · `fg3.py`, `objects3.py <video>`, `simon3.py map3`, `simon_boss3.py` · `strip3.py <video> <out.png> <a-b:step|f1,f2…> [x0,y0,x1,y1] [scale] [cols]` (dải ảnh có nhãn) · `trans3.py` (chuyển cảnh) · `events3.py` (bảng sự kiện + CSV commit) · `audio_cues3.py <wav> <out>` · `music3.py ident|seg <Map_3|Boss_Map_3_Behaviour>`. Thứ tự: extract → dup → camera → hud → objects → simon → events/trans.
**Độ phủ Simon (`map3_simon.csv`, 7222 frame gameplay):** khớp mẫu **81.4 %** (tin cậy cao err ≤ 0.12: 76.2 %); **87.0 %** sau nội suy (khoảng hở ≤ 10 frame + đứng yên giữa hai vị trí y hệt) — **chưa đạt 90 %**; hố lớn còn lại: f341–496 (11.4–16.5 s, dù Simon đi bình thường — nền bị lệch LUT, nợ), f2520–2585 (cửa #1), f3150–3212, f3275–3320 (ngay sau đổi tầng WalkwayB), f4536–4565, f5603–5724 (3 cầu hỏng/thang tháp). Boss: **605/1282 (47 %)**. HUD: 0 sai trên 20 frame kiểm mắt (`ref/nes/cvm3_02_hud_kiem_mat_20_frame.png`, đã xem 20/20 khớp SCORE/TIME/STAGE/tim/ENEMY/vũ khí).
**Ảnh tham chiếu (§7 planner, 40 ảnh `ref/nes/m3_*`, `b3_*`):** đã xem lưới tất cả 40; chỉ đổi tên 2: `…0m06_8s_stage07_bat_dau_wea_cx512` → `…den_fade_in_truoc_stage07` (6.8 s còn đen; STAGE 07 hiện 6.90 s), `…3m48_0s_qua_cau_va_vat_iii` → `…vat_ii` (icon là **II**; ảnh `cvm3_02_icon_II_khong_phai_III.png`). Ảnh thêm `ref/nes/cvm3_02_*.png`.
