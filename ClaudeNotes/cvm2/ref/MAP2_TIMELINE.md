# Chỉ mục timeline + hiệu chuẩn video tham chiếu — Castlevania NES Map 2 (Block 2: Stage 04–06)

> Planner CVM2 đo 15/10/2026 (Opus 5.5). **Mức độ: THÔ** — đủ để chia việc và tìm đoạn video; routine `cvm2-02` đo lại tới từng tick và
> mở rộng file này (HUD đọc từng frame, vị trí Simon, sự kiện). Mọi số dưới đây: **đo lại trước khi dựa vào** nếu cần độ chính xác cao.
>
> - Ảnh **full frame gốc** (36 PNG, 18 MB): `ClaudeNotes/cvm2/_cache/video/ref/` (chỉ máy này, KHÔNG commit).
> - Ảnh **lấy mẫu về lưới NES 256×240** (tâm từng pixel NES theo §1; hàng ngoài khung video = đen): `ClaudeNotes/cvm2/ref/nes/<tên>_nes.png` (commit) — so trực tiếp với ảnh chụp Unity.
> - Script planner: bản lưu `ClaudeNotes/cvm2/ref/tools/` (commit); bản chạy được + dữ liệu trung gian: `ClaudeNotes/cvm2/_cache/tools/` + `_cache/work/`
>   (chạy từ `ClaudeNotes/cvm2/_cache/`: `python -P -E tools/<tên>.py …`; `tools/nes2.py` trỏ đường dẫn tuyệt đối tới 2 video + sheet Level 2 trong `Downloads`).
> - Lưới ảnh 2 s/ô toàn video (đã xem hết): `_cache/work/sheet_map2_all_00..05.png`; cửa trái 0.33 s/ô: `sheet_map2_door1_00.png`; bẫy ép: `sheet_map2_crusher_00.png`; video boss 0.5 s/ô: `sheet_boss2_00..01.png`.

Video nguồn (chỉ đọc), thư mục `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\`:
- `Map_2.mp4` — **1486×712**, 30 fps, **4:20.47** (7814 frame), AAC 48 kHz stereo. Quay màn hình **tiếp đúng playthrough của `Map_1.mp4`** (mở đầu SCORE 023370, P-03, bản đồ lâu đài STAGE 04 — trùng cuối Map_1). Có watermark đỏ góc phải-dưới (như Map_1). Không thấy đoạn non-game (planner chưa quét kỹ — 02 kiểm).
- `Boss_Map_2_Behaviour.mp4` — **1490×718**, 30 fps, **23.30 s** (699 frame). Quay trình duyệt YouTube (kênh "Overtime Gamer"): game ở giữa, 2 bên viền mờ, chữ "ko-fi.com/overtimegamer" + watermark mờ "OVERTIME GAMER" đè vùng HUD; **bảng màu khác Map_2** (gạch tím/magenta — emulator/palette khác). Playthrough KHÁC (SCORE 005800, P-01, Simon cầm nước thánh + III).

Quy ước: thời điểm `m:ss.ss` (Map_2) hoặc `ss.s s` (boss). Frame video `f` = round(t × 30). **tick** = 1 frame NES (60.0988 Hz) ≈ 2.003 tick/frame video. Toạ độ NES x 0–255, y 0–239 (HUD y 0–47, sân chơi 48–223). `cx` = camera x trong vùng sheet; `world_x = cx + x_màn`.

## §1. Hiệu chuẩn video → pixel NES

Công thức: `x_NES = (x_v − ox) / sx`, `y_NES = (y_v − oy) / sy`; tâm pixel NES (x, y) ở video `(ox + sx·(x+0.5), oy + sy·(y+0.5))`.
Đo bằng `tools/calib_map2.py` (lưới tìm kiếm 2 trục độc lập, ZNCC xám vùng sân chơi NES y 48–223 với vùng sheet Level 2, 3 vòng mịn dần).

| | **Map_2** (1486×712) | **Boss_Map_2_Behaviour** (1490×718) |
|---|---|---|
| sx / sy | **3.623 / 3.104** | **3.609 / 3.157** |
| ox / oy | **268.93 / −20.24** | **274.33 / −22.95** |
| sx/sy | **1.167 ≈ 7/6** (như Map_1 — KHÔNG phải 8/7) | **1.143 ≈ 8/7** (như video boss Map 1) |
| Khớp | ZNCC 0.934 (1 frame TEA 0:12, cx 0) | ZNCC 0.843 (3 frame phòng Medusa cx 0 — thấp vì bảng màu khác + watermark) |
| Vùng game | x ≈ 269–1196 | x ≈ 274–1198 |

**Việc của 02:** tinh chỉnh bằng nhiều frame ở nhiều vùng (≥ 1 frame/section, cx khác nhau), kiểm hàng NES hiển thị (dự đoán y 7–235 — ngoài là viền đen của trình phát), kiểm HUD glyph (như Map_1 `MAP1_TIMELINE §1`). Video boss: bảng màu khác ⇒ khớp theo cạnh/độ sáng tương đối, KHÔNG so màu tuyệt đối với sheet.

**Bản đồ ↔ màn hình** (giả thuyết như Level 1, khớp tốt ở cả 6 vùng): dòng r của vùng sheet ↔ **NES y = 32 + r**; `world_x = cx + x_màn`; cx ∈ [0, rộng − 256].

## §2. Vùng sheet Level 2 (`ClaudeNotes/cvm2/inventory/level2_main_hall.json`, ảnh chú thích `level2_main_hall_annot.png`)

| Vùng (tên planner) | Stage | rect sheet [x,y,w,h] | Màn | Vào từ (video) | Ra | 8×8 / 16×16 duy nhất |
|---|---|---|---|---|---|---|
| **TowerEntranceA** (TEA) | 04 | [1803, 632, 256, 192] | 1 | bản đồ lâu đài → cx 0 (0:06.23), cửa đỏ ở mép TRÁI (cửa vào, không dùng) | thang phải-lên cắt mép trên → TEB (0:28.50) | 32 / 22 |
| **TowerEntranceB** (TEB) | 04 | [1547, 425, 512, 192] | 2 | thang từ TEA, hiện ở **cx 256** (0:28.87) | **cửa TRÁI** tầng trên (0:51.80) → Hall A | 42 / 27 |
| **TowerHallA** | 05 | [774, 425, 768, 192] | 3 | sau cửa: **cx 512** (0:57.70), đi sang TRÁI | thang trái-lên cắt mép trên → Hall B (1:33.00) | 43 / 26 |
| **TowerHallB** | 05 | [774, 218, 768, 192] | 3 | thang: cx 0 (1:33.37), đi PHẢI tới cx 512 (1:54) rồi quay TRÁI tầng trên | **cửa TRÁI** tầng trên (2:22.33) → Hall C | 56 / 37 |
| **TowerHallC** | 06 | [1, 218, 768, 192] | 3 | sau cửa: cx 512 (2:28.33), đi TRÁI tầng dưới tới cx 0 (2:52) → thang trái lên tầng trên → đi PHẢI | thang phải-lên cắt mép trên → sảnh Medusa (3:17.13) | 44 / 32 · **6144 px màu teal (0,128,128) = 3 ô 32×64 chỗ bẫy ép gai** |
| **Medusa** | 06 | [1, 11, 768, 192] | 3 | thang: cx 512 (3:17.47), đi TRÁI | phòng boss: camera khoá **cx 0** (~3:46); tượng Medusa VẼ SẴN trên nền sheet (cột ~6–7) | 64 / 44 |
| Sprite rời | — | Crusher 8 khung [1+33k, 425, 32, 64] k=0..7 (dài dần), Moving Platform [269, 425, 32, 16] | | | | |

Tổng: 82 tile 8×8 / 69 metatile 16×16 duy nhất (lưới 0,0 — số ít ⇒ lưới đúng). Màu nền khoá: xanh (0,128,0). **Xám (116,116,116) là màu THẬT** (tường đá Hall C, bậc phòng Medusa) — đừng key như sheet nhân vật. Credit logo [1576,792,224,32] — bỏ.
Sheet phụ: `NES - Castlevania - Miscellaneous - Tilesets.png` (1473×773) — bảng tile 16×16 của 6 level (cột "LEVEL2: MAIN HALL" có 2 bảng màu Mesen/Nestopia) — dùng để **kiểm** tile, không thay sheet bản đồ.

## §3. Timeline Map_2 (phân đoạn tự động từng frame — `tools/locate_map2.py map2 30` ⇒ `_cache/work/map2_locate_30.csv`; ĐEN = độ sáng TB < 12)

| Mốc | f | Sự kiện (planner nhìn lưới 2 s + đoạn dày) |
|---|---|---|
| 0:00.00–0:05.73 | 0–172 | **bản đồ lâu đài** (đuôi Map 1): HUD STAGE 04 TIME 0000, tim 05; Simon nhỏ đi dọc đáy |
| 0:05.77–0:06.20 | 173–186 | đen (14 frame ≈ 28 tick — Map 1 đo 27) |
| **0:06.23** | 187 | **STAGE 04 bắt đầu — TEA cx 0**, TIME **0400** (khớp `ConfigBlock` Block 2 = 400 — nguồn cũ `video:Map_1 3:59.2`, nay thêm bằng chứng Map_2); Simon trái-dưới cạnh cửa đỏ; Spear Knight đứng tầng trên trái |
| 0:08 | 240 | nến đầu tiên ⇒ HUD đổi rìu → **thập tự (Cross/Boomerang)** (wiki: Boomerang trong nến đầu) |
| 0:18–0:24 | | leo thang phải; điểm 23770 → 25770 (**+2000 ≈ vương miện** — wiki: phá tường bên phải ⇒ vương miện dưới thang); "1000"? lúc 0:24 (02 kiểm) |
| 0:28.50–0:28.83 | 855–865 | đen (đổi tầng TEA → TEB, 11 frame) |
| **0:28.87** | 866 | **TEB cx 256** (mép phải); Spear Knight ×2, dơi; đi TRÁI |
| 0:38–0:48 | | camera 231 → 0; **bệ di động "oooo" (Moving Platform)** ở tầng trên trái 0:44–0:51 (Simon đứng lên đi qua khe) |
| **0:51.80–0:57.67** | 1554–1730 | **CỬA TRÁI cuối Stage 04**: camera cuộn sang TRÁI (cửa ở mép trái TEB tầng trên), cửa mở 0:53.8–0:55.5, Simon tự đi sang TRÁI xuyên cửa, cửa đóng, cuộn tiếp; TIME đứng 0359 (5.9 s — Map 1 6.8 s: 02 đo lại) |
| **0:57.70** | 1731 | **STAGE 05 — Tower Hall A cx 443→512**, đi TRÁI; Medusa Head (xanh) bay sóng; dao găm rơi từ nến ~1:08 |
| 1:20 | ~2400 | **rương báu (Treasure Box) tím** trồi lên (wiki: đứng trên 2 khối cao ⇒ +2000) — điểm 28370 → 30370 lúc 1:22 |
| 1:28–1:32 | | thang trái-lên ở cx 0 |
| 1:33.00–1:33.33 | 2790–2800 | đen (Hall A → Hall B) |
| **1:33.37** | 2801 | **Tower Hall B cx 0**, đi PHẢI; 2 bệ di động (1:40–1:46), Spear Knight tím ×2–3, Medusa Head |
| 1:54–2:04 | | cx 512 (mép phải) — **thịt nướng (Pot Roast)** hiện 1:56 (wiki: bậc cao nhất góc trên-phải); 2:06 HUD đổi → **nước thánh**; quay TRÁI tầng trên |
| **2:22.33–2:28.30** | 4270–4449 | **CỬA TRÁI cuối Stage 05** (Hall B tầng trên trái, cột cửa đỏ 2:20); TIME đứng 0281 (6.0 s) |
| **2:28.33** | 4450 | **STAGE 06 — Tower Hall C cx 507→512**, đi TRÁI tầng dưới; thịt nướng ở khối đầu stage (2:30 — wiki #1) |
| 2:33–2:41 | | **3 bẫy ép gai (Crusher)** hạ/nâng từ trần, lệch pha nhau (`sheet_map2_crusher_00.png`); Simon chờ rồi chạy qua |
| 2:42–2:58 | | **Bone Tower** (tháp sọ tím) trên bậc, cầu lửa; **Ghost** (2:46); **III** rơi (2:50); thang trái-lên lên tầng trên (cx 0 lúc 2:52), đi PHẢI |
| 3:09.2–3:09.6 | 5676–5689 | 2 khoảnh "other" ngắn (nhiễu khớp — 02 xem: nháy màn? trúng đòn?) |
| 3:17.13–3:17.43 | 5914–5923 | đen (Hall C → sảnh Medusa) |
| **3:17.47** | 5924 | **sảnh Medusa cx 512** (vào từ thang phải-dưới), đi TRÁI; Medusa Head nhiều |
| ~3:42–3:46 | | thấy phòng boss + **tượng Medusa** (cx 0 từ ~3:46); TIME 0216 |
| **3:46–3:56** | | **trận Queen Medusa** (Simon nước thánh + roi, P-04): ENEMY tụt nhanh, **+3000** lúc ~3:56 (36570 → 39570) |
| 3:58 | | **quả cầu đỏ** nhặt; TIME dừng **0210** |
| 4:00–4:12 | | **tally**: TIME 0210 → 0000 (+10/đv), tim 30 → 00 (+100/tim) ⇒ SCORE 44670 (= 39570 + 2100 + 3000 ✓ luật Map 1) |
| 4:12.80–4:13.80 | 7584–7614 | đen (31 frame ≈ 62 tick — Map 1 đo 23 ⇒ 02 đo lại; có thể là chờ sau tally + đen) |
| 4:13.83–4:19.70 | 7615–7791 | **bản đồ lâu đài Block 2**: HUD **STAGE 07**, tim 05; bản đồ trượt vào từ phải (cảnh khác Block 1 — dơi tím ô kế), Simon nhỏ đi dọc đáy |
| 4:19.73–4:20.17 | 7792–7805 | đen → (hết video ~ frame đầu Stage 07) |

**Quỹ đạo camera (mỗi 2 s, `map2_locate.csv`):** TEA 0 suốt · TEB 256 → 231 (0:38) → 0 (0:48) · Hall A 512 (1:00–1:04) → 0 (1:30) · Hall B 0 → 512 (1:54) → 0 (2:22) · Hall C 512 → 0 (2:52) → 512 (3:16) · Medusa 512 → 0 (3:46) rồi khoá 0.
**Hướng đi Map 2 ngược Map 1**: hai cửa cuối stage nằm **mép TRÁI**, camera cuộn sang trái; nhiều section vào từ mép phải ⇒ lõi (`StageFlow` cửa, `StageRules.GetStageStart`, camera) phải hỗ trợ hướng trái (Map 1 chỉ có cửa phải).

## §4. Video boss (`Boss_Map_2_Behaviour.mp4` — lưới 0.5 s `sheet_boss2_00..01.png`)
| t | Sự kiện |
|---|---|
| 0.0–2.0 | phòng Medusa cx 0 (khoá), TIME 0083→0081; Simon trái đi vào; **tượng Medusa (đầu + thân đỏ-trắng trên bệ) giữa phòng** |
| ~2.5 | **Medusa thoát khỏi tượng** (đầu tóc rắn bay ra; thân tượng đỏ-trắng VẪN nằm ở bệ suốt trận); ENEMY 16 vạch |
| 3.0–16.0 | Medusa bay sóng thấp sát sàn, qua lại về phía Simon; **đổi bảng màu liên tục xanh đậm ↔ tím nhạt** (nháy — 02/09 đo chu kỳ); **rắn** (xanh) rơi xuống sàn trườn (3.5, 12.0–13.0 s); Simon ném **nước thánh** (lửa hồng trên sàn 5–8 s), quất roi; **PLAYER tụt 16 → 4** (Simon trúng đòn nhiều lần — nguồn đo knockback bổ sung) |
| ~16.5 | Medusa hết máu ⇒ **lửa chết boss** tại chỗ (16.5–17.0) |
| ~19.0 | **quả cầu đỏ** hiện + rơi cạnh tượng; Simon nhặt ~20.5; SCORE 5800 → 8800 (+3000) |
| 20.5–23.3 | TIME dừng 0064, Simon giơ cầu (tư thế nhặt) |

## §5. Ảnh tham chiếu (`ref/nes/` + `_cache/video/ref/`)
Map_2 (28): `m2_00m02_0s_ban_do_lau_dai_vao_stage04` · `m2_00m06_0s_stage04_bat_dau_tower_entrance_a` · `m2_00m08_0s_nen_dau_tien_thap_tu` · `m2_00m20_0s_leo_thang_tea_vuong_mien` · `m2_00m30_0s_tower_entrance_b_cx256` · `m2_00m46_0s_be_di_dong_teb` · `m2_00m52_2s_cua_trai_bat_dau_cuon` · `m2_00m54_2s_cua_trai_mo` · `m2_00m58_0s_stage05_tower_hall_a_cx512` · `m2_01m02_0s_medusa_head_hall_a` · `m2_01m20_0s_ruong_bau_vat_bi_mat` · `m2_01m32_0s_thang_len_hall_a_sang_b` · `m2_01m44_0s_hall_b_be_di_dong` · `m2_01m56_0s_hall_b_thit_nuong` · `m2_02m24_2s_cua_trai_stage05_sang_06` · `m2_02m30_0s_stage06_tower_hall_c_cx512` · `m2_02m38_0s_bay_ep_gai_hall_c` · `m2_02m44_0s_bone_tower_hall_c` · `m2_02m46_0s_ghost_hall_c` · `m2_02m50_0s_vat_pham_iii_hall_c` · `m2_03m16_0s_thang_len_sanh_medusa` · `m2_03m18_0s_sanh_medusa_cx512` · `m2_03m42_0s_phong_boss_tuong_medusa` · `m2_03m48_0s_medusa_tran_dau` · `m2_03m56_0s_medusa_chet_3000` · `m2_03m58_0s_qua_cau_do` · `m2_04m02_0s_tally_cong_diem` · `m2_04m16_0s_ban_do_lau_dai_stage07`.
Boss (8): `b2_00_0s_tuong_medusa_simon_vao` · `b2_02_5s_medusa_thoat_tuong` · `b2_03_5s_ran_tren_san` · `b2_06_0s_lua_nuoc_thanh` · `b2_12_5s_ran_va_medusa_bay` · `b2_16_5s_medusa_chet_lua` · `b2_19_0s_qua_cau_roi` · `b2_20_5s_nhat_qua_cau`.
(Tên mô tả là planner đoán theo lưới 2 s — 02 kiểm và đổi tên nếu sai.)

## §6. Việc đo còn lại cho routine (planner CHƯA làm)
- HUD từng frame (SCORE/TIME/tim/P/máu/vũ khí) ⇒ bảng sự kiện điểm/nhặt/trúng đòn (như `map1_events.txt`, `score_events.txt` của Map 1).
- Vị trí Simon từng frame (khớp mẫu sprite — `Tools/cvgp/simon_fit.py`), enemy/vật thể (khớp mẫu sprite sheet Enemies/Items).
- Mọi thời lượng chuyển cảnh tính bằng tick (cửa trái, đổi tầng, khoá camera boss, tally, bản đồ Block 2).
- Âm thanh: nhạc bắt đầu/dừng (Stalker? Poison Mind, Stage Clear) — tương quan chéo với `Art_Source/music/*.mp3`.
- Bệ di động (biên, tốc độ, chu kỳ), bẫy ép (chu kỳ, pha, hitbox, sát thương/chết), hố (Stage 5–6), Medusa Head (sinh, sóng), Spear Knight/Bone Tower/Ghost/dơi, Queen Medusa + rắn.
