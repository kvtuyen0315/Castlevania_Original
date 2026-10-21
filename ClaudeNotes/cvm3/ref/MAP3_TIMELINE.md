# Chỉ mục timeline + hiệu chuẩn video tham chiếu — Castlevania NES Map 3 (Block 3: Stage 07–09)

> **Bản PLANNER (thô, 17/10/2026)** — routine `cvm3-02` đo lại tới từng frame/tick và VIẾT LẠI file này (khuôn `ClaudeNotes/cvm2/ref/MAP2_TIMELINE.md`).
> Số ở đây đọc bằng mắt trên lưới NES mỗi 0.25–2 s + phân đoạn tự động 5 fps ⇒ **sai số mốc ±0.2–0.5 s**, toạ độ ±4 px. Đừng dựng game từ số planner.
> Công cụ planner (chạy được): `ClaudeNotes/cvm3/_cache/tools/` (bản lưu `ref/tools/`): `nes3.py` (hiệu chuẩn + đọc frame + lấy mẫu lưới NES),
> `calib_map3.py`, `calib_boss3.py` (gradient — video boss khác bảng màu), `locate_map3.py` (section + cx mỗi frame), `nes_sheets3.py` (lưới ảnh có nhãn giây),
> `save_refs3.py` (40 ảnh mốc), `inv_level3_tiles.py` (kiểm kê sheet). Chạy từ `ClaudeNotes/cvm3/_cache/`: `python -P -E tools/<script>.py …`.

## §1. Hai video

| | `Map_3.mp4` | `Boss_Map_3_Behaviour.mp4` |
|---|---|---|
| Đường dẫn | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_3.mp4` | `…\Boss_Map_3_Behaviour.mp4` |
| Thời lượng / khung | **4:11.61**, **1488×760**, 30 fps, AAC 48 kHz stereo | **42.73 s**, **1370×772**, 30 fps, AAC 48 kHz |
| Nội dung | **Tiếp ĐÚNG playthrough Map_2** (SCORE 044670, P-04, tim 05, **đồng hồ**): đuôi bản đồ lâu đài B2 → STAGE 07 → 08 → 09 → Mummy Men → tally → bản đồ lâu đài B3 (HUD **STAGE 10**) → hết giữa lúc bản đồ trượt vào | Playthrough KHÁC (SCORE 016900, **P-01**, tim 07, **dao găm**), **bảng màu khác** (gạch hồng-cam), watermark "OVERTIME GAMER"/"by AND_…" giữa HUD + "Support me on ko-fi…" góc dưới-trái (NES y ≈ 214–222, x ≈ 0–90) + 2 dải mờ hai bên. 0.0 s = 1 khung bản đồ lâu đài (cắt ghép, HUD STAGE 07 SCORE 009840 — bỏ qua) |
| Khung game thật/giây (planner, 10 s mẫu) | **≈ 18** (≈ 40 % khung lặp) ⇒ mốc đơn ±3 tick | **≈ 21.7** |
| Tỉ lệ pixel | **7/6** (như Map_1/Map_2) | **8/7** (như video boss Map 1/2) |

### §1a. Hiệu chuẩn planner (`x_v = ox + sx·(x_NES + ½)`, `y_v = oy + sy·(y_NES + ½)`, quy ước lấy mẫu như `cvm2 nes2.py`)
| Video | sx | sy | ox | oy | Cách đo | Điểm |
|---|---|---|---|---|---|---|
| Map_3 | **3.859** | **3.310** | **240.93** | **−21.92** | ZNCC xám, 5 frame (12 s WEA cx 512, 56 s WEB cx 296, 92 s WalkwayA cx 0, 140 s WalkwayB cx 512, 176 s MummyMan cx 711) — 1 frame (12 s) cho 0.977 | TB 0.894 (cx planner có thể lệch ±1) |
| Boss_Map_3 | **3.845** | **3.363** | **195.76** | **−21.77** | ZNCC **gradient** (bảng màu khác), 5 frame 14/20/26/32/38 s, phòng boss **MummyMan cx 1280** (nửa trên khung, tránh Simon/Mummy) | 0.942–0.944 mỗi frame |
Dòng r của vùng sheet ↔ **NES y = 32 + r** (như Level 1/2 — 5/5 section khớp). Routine 02: tinh lại ≥ 8 frame (mỗi section ≥ 1, cx tinh ±2), kiểm bằng glyph HUD, ghi hàng NES nhìn thấy + vùng watermark.

## §2. Vùng sheet Level 3 (`ClaudeNotes/cvm3/inventory/level3_walkway.json`, ảnh `level3_walkway_annot.png`)
Sheet `NES - Castlevania - Stages - Level 3_ Crumbling Tower.png` **3596×618 RGBA**, 96 910 B, sha256 `c8bfc7ef3da18110c4f226276e7ae82e6611d2c4f3246517f5578bd476c44eee`. Nền khoá xanh (0,128,0); nhãn chữ trắng trên mỗi vùng; logo credit [3371,585,224,32].
| Section (tên = nhãn sheet) | Stage | rect [x,y,w,h] | Màn | tile 8×8 / 16×16 duy nhất | Đi trong video |
|---|---|---|---|---|---|
| `WalkwayEntranceA` | 07 | [1, 425, 768, 192] | 3 | 68 / 50 | vào cx 512 (cửa trang trí mép PHẢI), đi **TRÁI** tới cx 0, thang lên-trái góc trên-trái |
| `WalkwayEntranceB` | 07 | [1, 218, 768, 192] | 3 | 47 / 43 | vào cx 0 (chân thang trái-dưới), đi **PHẢI** tới cx 512, **CỬA PHẢI** cuối stage |
| `WalkwayA` | 08 | [774, 218, 768, 192] | 3 | 60 / 44 | vào cx 0, đi **PHẢI**; cầu + lan can + tượng; thang lên-trái ở phần phải (cx ~484–512) |
| `WalkwayB` | 08 | [1286, 11, 768, 192] | 3 | 60 / 43 | vào sau đen ở **cx ≈ −43** (!), lùi tới **cx −109**, rồi đi PHẢI tới cx 512, **CỬA PHẢI** |
| `MummyMan` | 09 | [2059, 11, **1536**, 192] | **6** | 100 / 59 | vào cx 0, đi **PHẢI**; cầu dài có **đoạn hỏng (hố)**, tháp vỡ; **phòng Mummy = 256 px cuối, khoá cx 1280** |
Tổng **125 tile 8×8 / 83 metatile 16×16** (Level 1: 118/164, Level 2: 82/69). Không có ô lạ (teal/khoá) trong vùng — khác Level 2.
**Phát hiện cho routine 05:** ở `WalkwayB` camera đi tới **cx −109** (planner: ZNCC 0.94–0.96 khi đệm trái bằng màu trời (36,24,140)) ⇒ section NES rộng hơn vùng sheet ≥ 109 px về bên trái (toàn trời xanh đậm + có thể đầu cầu) — sheet cắt mất. Ảnh wiki Stage 8 (nửa tỉ lệ) cũng vẽ trời ở trái tầng trên. Đo kỹ: cx nhỏ nhất, nội dung cột trái ở cx nhỏ nhất (trời thuần? đầu cầu?), Simon đứng ở đâu khi nhặt túi tiền (≈ 1:55).

## §3. Phân đoạn tự động (`_cache/work/map3_locate_5.csv`, 5 fps, ZNCC xám mọi cx bước 4 rồi tinh ±3; `other` = điểm < 0.6)
| Giây video | Nhãn | cx | Ghi chú planner |
|---|---|---|---|
| 0.00–6.20 | bản đồ lâu đài **B2** | — | 0.0–0.4 nút ▶ YouTube đè; 0.4–2.4 bản đồ trượt vào từ phải; 2.8–6.0 Simon nhỏ đi từ x ≈ 30 tới ≈ 235 dưới đáy; tim nhấp nháy ở tháp phải; HUD STAGE 07 TIME 0000 SCORE 044670 P-04 tim 05 — **mẫu thứ 2 của bản đồ B2 (CVM2 11 đã dựng từ cuối Map_2)** |
| 6.40–6.80 | đen | — | |
| **7.00**–39.40 | `WalkwayEntranceA` | 512 → 0 | **STAGE 07** hiện ≈ 6.8 s, **TIME 0500**, Simon x màn ≈ 205 (x section ≈ 717) cạnh cửa đỏ trang trí mép phải |
| 39.60–39.80 | đen (đổi tầng) | — | Simon leo thang lên-trái ở WEA cx 0 (36–39.2) |
| **40.00**–81.20 | `WalkwayEntranceB` | 0 → 512 | Simon hiện ở chân thang trái-dưới (x màn ≈ 110, y ≈ 200), leo tiếp |
| 81.40–88.00 | `other` = **CỬA PHẢI #1** | — | 81.5–83 cuộn 1; 83.5–84.5 cửa mở, Simon đi qua; 85–88 cuộn 2; **STAGE 08 đổi ≈ 88.0** (cuối cuộn 2 như Map_2); TIME 0432 đứng suốt cửa |
| **88.20**–108.60 | `WalkwayA` | 0 → 512 | |
| 108.80–109.00 | đen (đổi tầng) | — | Simon leo thang lên-trái ở WalkwayA phần phải (106–108.5) |
| 109.20–117.80 | `WalkwayB` (planner gắn `other` vì cx < 0) | **−43 → −91 → −2 → +6 → −109 → +5** | Simon lên thang trái-dưới, đi trái, ngồi (≈ 113.5) ⇒ **túi tiền bí mật** hiện bên trái (114.5), nhặt **+1000** (116.0) |
| 118.00–144.40 | `WalkwayB` | 0 → 512 | Bone Tower trên cầu lan can (124–134) |
| 144.60–150.20 | `other` = **CỬA PHẢI #2** | — | **STAGE 09 đổi ≈ 151.0–151.5**; TIME 0380 đứng suốt cửa |
| **150.40**–248.20 | `MummyMan` | 0 → **1280** | cầu dài, Skeleton nhảy qua đoạn hỏng (170–172), tháp vỡ (176–180), Mummy đứng chờ thấy từ ≈ 186 (cx ≈ 1100), **khoá cx 1280 ≈ 198.0**; trận 198–226; quả cầu 228; tally 234–247.4 |
| 248.40–249.00 | đen | — | HUD **STAGE 10**, tim 05 |
| 249.20–251.61 | bản đồ lâu đài **B3** trượt vào | — | video hết giữa lúc trượt (không có Simon nhỏ đi) |

## §4. Timeline sự kiện Map_3 (planner — đọc mắt lưới NES mỗi 2 s `_cache/work/sheet_map3_all_00..05.png` + 0.25–1 s quanh chuyển cảnh)
| Giây | Section | Sự kiện (thô — routine 02 đo lại theo tick + HUD từng frame) |
|---|---|---|
| 6.8 | WEA cx 512 | STAGE 07 TIME 0500, SCORE 044670, tim 05, vũ khí **đồng hồ**, P-04, roi cấp 3 (?) |
| 8–12 | WEA | +700 (045370 @10 — túi tiền?), +100 (045470), tim 05→06 |
| 14.0 | WEA | **lọ nước thánh rơi trên sàn** (x màn ≈ 110) + **enemy hồng nhạt** bay gần Simon (Ghost? — wiki St7 có Ghost) |
| 16.0 | WEA | vũ khí đổi **nước thánh** (giữ tới hết video), +300 |
| 18–28 | WEA cx ~400→170 | Raven (20.0, 28.0 — đậu ở bậc trái), tim lớn/nhỏ, cột lưới xanh cao (cx ~170 — trang trí), +100/+500 |
| 26–36 | WEA cx 0 | **Skeleton góc dưới-trái** (wiki ②) + trên thang (34.0), xương bay (36.0), tim 16→21 (+5 tim lớn ≈ 37.2) |
| 36–39.2 | WEA cx 0 | leo thang lên-trái → đen 39.5 |
| 40–52 | WEB | leo thang trái-dưới lên; Raven (46–52), Skeleton trắng (50.0), +100/+400 (48170 @52) |
| 54–66 | WEB cx ~100–300 | Raven, tim; **66.0 túi tiền trắng ở bục cao bên trái** (wiki ③: ngồi ở mép phải bục thấp nhất) ⇒ **+1000** (049870 @68) |
| 70–74 | WEB cx ~400 | **Skeleton ném xương** (70–72) |
| 78–81 | WEB cx 512 | +100, +400 (050370) — Simon quất gì đó ngay trước cửa |
| 81.4–88.0 | cửa #1 | xem §3 |
| 88–100 | WalkwayA cx 0–300 | tim, **Medusa Head** (94–104, tầng dưới — wiki ①), Skeleton (100) |
| 102–108.5 | WalkwayA phải | leo thang lên-trái (Medusa Head quanh thang) |
| 109–117.8 | WalkwayB cx < 0 | bí mật túi tiền St08 (wiki ②: ngồi ngay trên 2 khối không với tới, bên phải thang) |
| 118–136 | WalkwayB cx 0–400 | **Bone Tower** ×≥2 trên cầu lan can (124–134; tháp tím-trắng), Raven (130), tim |
| 138–150 | WalkwayB cx 512 → cửa #2 | |
| 151.5–166 | MummyMan cx 0–400 | tượng trên cầu; Bone Tower (158–168), Raven (162 — sà từ trên) |
| 170–178 | MummyMan cx ~500–700 | **cầu hỏng — Simon nhảy qua các bục**, **Skeleton nhảy qua đoạn hỏng** (wiki ②), tim |
| 176–184 | MummyMan cx ~700–1000 | **tháp vỡ** (bóng đen nhọn), Bone Tower (180–182), Raven |
| 186–196 | MummyMan cx ~1000–1253 | bậc cam xuống phòng; **2 Mummy đứng chờ ở sàn phòng** trước khi khoá; Simon xuống bậc |
| ≈ 198.0 | MummyMan **cx 1280** | **khoá camera**; Simon nhảy lên các khối trái (cầu thang khối) đứng khối cao nhất |
| 198–226 | phòng Mummy | Simon đứng khối trái cao **ném nước thánh** xuống; Mummy đỏ khi trúng lửa ("hoá đá" wiki); Mummy ném **băng gạc**; ENEMY giảm dần; tim 55→46 |
| ≈ 223 | | **Mummy 1 chết +3000** (059970 → 062970) |
| ≈ 226 | | **Mummy 2 chết +3000** (→ 065970) — **mỗi con +3000** (wiki 3000 chung — kiểm) |
| ≈ 228 | | **quả cầu đỏ** rơi giữa phòng + **vật III (shot multiplier) rơi** ở bên phải — HUD có III từ ≈ 229–230 |
| ≈ 229.5–233.5 | | TIME đứng 0306 (nhặt cầu → chờ tally) |
| 234–247.4 | | **tally**: TIME ×10 (306 → 0 ⇒ 069030 @239.6), tim ×100 (46 → 0 ⇒ **073630**) |
| 248.6 | | đen, HUD **STAGE 10** tim 05 → 249.2 bản đồ B3 trượt vào → hết video 251.6 |
Simon **không mất mạng** trong Map_3 (P-04 suốt); có trúng đòn (thanh PLAYER giảm vài lần — routine 02 liệt kê theo tick: mẫu knockback Block 3 cho routine 04).

## §5. Video boss (`Boss_Map_3_Behaviour.mp4`) — planner, mỗi 1 s (`_cache/work/sheet_boss3_all_00..01.png`)
| Giây | Sự kiện |
|---|---|
| 0.0 | 1 khung bản đồ lâu đài (cắt ghép — bỏ) |
| 1.0–6.0 | STAGE 09 TIME 0240, Simon ở bục cam phía trên, xuống bậc vào phòng; 1 Mummy tím đứng sàn; camera đang cuộn (cx ≈ 1200 → 1280) |
| 6.0–8.0 | Simon **phá khối thấp nhất cầu thang trái ⇒ thịt nướng** (wiki ③) |
| ≈ 11–12 | camera khoá cx 1280; 2 Mummy hai phía Simon (wiki: kẹp hai bên) |
| 12–33 | Simon (dao găm + roi) đánh; **Mummy ném băng gạc** (nhiều lần, bay ngang thấp — 14, 17, 18, 19, 23, 27, 32); **Simon trúng đòn nhiều lần** (PLAYER giảm — mẫu knockback/sát thương 3 vạch); SCORE +100 vài lần trong trận (17000→17300 — trúng gì được +100? kiểm) |
| ≈ 28 | Mummy 1 chết (017300 → 020500 = +3200?) |
| ≈ 33 | Mummy 2 chết (020600 → **023600** = +3000) |
| 35–36 | **quả cầu** hiện trên cao giữa phòng (≈ x 128) rồi rơi xuống sàn |
| 36–42 | Simon đi tới cầu; TIME đứng 0203 từ ≈ 40 (nhặt?) — video hết 42.7 |

## §6. Chuyển cảnh (planner — routine 02 đo theo tick, khuôn `MAP2_TIMELINE §7`)
| Mốc | Planner (giây video) | Cần đo |
|---|---|---|
| Bản đồ B2 → đen → STAGE 07 | bản đồ tới 6.2, đen 6.4–6.8, St07 6.8 | so với Unity CVM2 11 (đen 27 tick, Simon (x,y) trên bản đồ, dơi (164,99), chấm (76,111)) — **mẫu thứ 2** |
| Đổi tầng WEA → WEB (thang lên-trái cắt mép trên) | 39.2 → đen 39.5–39.8 → 40.0 | đen N tick, Y rời/hiện, x Simon trước/sau ⇒ phép dời WEA→WEB; cx đích |
| Cửa PHẢI #1 (WEB → WalkwayA, STAGE 07→08) | 81.4–88.0 | cuộn 1 (px/tick, độ dài), cánh cửa door_0/door_1, Simon tự đi, đóng, cuộn 2, STAGE +1 lúc nào — so `Map1FlowTiming` (cửa phải Map 1) |
| Đổi tầng WalkwayA → WalkwayB (thang lên-trái) | 108.5 → đen 108.8–109.0 → 109.2 (cx ≈ −43) | như trên + camera đích ÂM |
| Cửa PHẢI #2 (WalkwayB → MummyMan, STAGE 08→09) | 144.6–151.5 | như cửa #1 (Map_2: 2 cửa lệch nhau 7–9 tick — §5 #15 CVM2) |
| Khoá camera phòng Mummy | ≈ 198.0 cx 1280 | Simon ở x nào; Mummy thức lúc nào (đứng yên trước khoá?) |
| Mummy chết → quả cầu → nhặt → tally | 223 / 226 / 228 / ≈ 229.5 / 234 | khoảng chờ tally (Map_2 224 tick, Map_1 260 — §5 #17 CVM2) |
| Tally → đen → STAGE 10 → bản đồ B3 | 247.4 / 248.6 / 249.2 | đen N tick, HUD STAGE 10 lúc nào, bản đồ trượt (tốc độ) |
TIME: STAGE 07 = **0500**; **không đặt lại** qua STAGE 08 (0432 → 0432) và 09 (0380 → 0380) — như Block 1/2.

## §7. Ảnh tham chiếu (`ref/nes/*_nes.png` = lưới NES 256×240 hiệu chuẩn planner; full frame gốc `_cache/video/ref/` — không commit)
40 ảnh: Map_3 33 (`m3_<m>m<ss_s>s_<mô tả>`) + boss 7 (`b3_<ss_s>s_<mô tả>`), danh sách trong `ref/tools/save_refs3.py`. Mô tả là của planner — routine 02 kiểm, đổi tên nếu sai.
Lưới ảnh nhanh (không commit): `_cache/work/sheet_map3_{all,start,stair1,door1,stair2,door2,boss,end}_*.png`, `sheet_boss3_all_*.png`; sheet Level 3 cắt đôi `_cache/level3_left.png` / `level3_right.png`; bản đồ wiki ghép `_cache/stages789_x2.png`, `_cache/screens789.png`.

## §8. Việc routine 02 phải làm với file này
Viết lại §1–§7 theo số đo từng frame (khuôn MAP2_TIMELINE của CVM2 02): hiệu chuẩn tinh; HUD từng frame (SCORE/TIME/STAGE/tim/P/PLAYER/ENEMY/vũ khí/II-III); camera + section từng frame (**kể cả cx âm ở WalkwayB**); Simon từng frame; bảng sự kiện ≥ 60 dòng; **mọi lần Simon trúng đòn ở cả 2 video** (tick, nguồn, vạch mất, quỹ đạo lùi — cho routine 04); mọi chuyển cảnh theo tick; âm thanh (Wicked Child bắt đầu/lặp, Poison Mind, Stage Clear); mẫu bản đồ B2 thứ 2 + bản đồ B3 trượt.
