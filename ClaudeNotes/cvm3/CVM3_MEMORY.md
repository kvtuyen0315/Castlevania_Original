# CVM3_MEMORY — sổ bàn giao cụm CVM3 (Map 3 = Block 3 = Stage 07–09 chơi được như video)

*Mọi routine CVM3: đọc file này + `CVM3_PLAN.md` ĐẦU phiên, cập nhật CUỐI phiên (§1 + mục liên quan + §7), commit cùng routine.*
*Hiến pháp H1–H14 ở `CVM3_PLAN §8`; kiến trúc mở rộng §5; danh sách routine §7. Hạ tầng cũ: `ClaudeNotes/cvm2/CVM2_MEMORY.md` §3/§4/§2, `ClaudeNotes/cvgp/CVGP_MEMORY.md` §3/§4/§4b/§7.*

**Yêu cầu gốc (17/10/2026, nguyên văn):**
> Tiếp tục dựng map 2 và play game theo như theo như video trên. Xem qua Walkthrough Stages 7 - 9 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_7_-_9
> Nếu như thiếu file sprite thì vào link ( https://www.spriters-resource.com/nes/cv/ ) này xem để download
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

Video kèm: `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_3.mp4` (4:11) và `...\Boss_Map_3_Behaviour.mp4` (42.7 s, Mummy Men; bảng màu khác + watermark). Sheet mới: `C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 3_ Crumbling Tower.png`. **Người dùng đã chốt từ CVGP:** bàn phím Z = Roi · X = Nhảy · C = Vũ khí phụ · V = Select · B = Start; gamepad A = Nhảy · X = Roi · B/Y = Vũ khí phụ · Menu = Start · View = Select.

**Quyết định planner (17/10/2026 — `CVM3_PLAN §0`, routine không hỏi lại):**
- Người dùng viết "map 2" nhưng mọi tài liệu kèm là Block 3 ⇒ hiểu là **Map 3**, nối tiếp CVM2 (chơi được tới `Stage7Stub`). Sai ⇒ dừng cụm, hỏi (§5 #6).
- Cụm gộp 2 nợ chặn Map 3: **đồng hồ theo video** (`cvm2-fix-01` → routine 03) và **knockback + nước thánh + mép rơi** (`cvgp-fix-03` → routine 04) — CỐ Ý đổi hành vi Map 1/2 (H14).
- Luật enemy rơi vật (`cvm2-fix-02`): routine 08 đếm mọi lần hạ ở 3 video; chỉ dựng nếu MỘT luật tất định giải thích mọi mẫu.
- `cvm2-fix-03`: phần builder màn hình không idempotent byte → routine 11; pha TIME/chờ tally: Block 3 dùng hằng riêng đo từ Map_3. `cvgp-fix-04` (Phantom Bat) KHÔNG gộp.
- Sprite: không cần tải thêm (sprite Block 3 đã cắt từ CVGP 05 + sheet Level 3 mới). spriters-resource chặn Cloudflare ⇒ thiếu thật thì ghi §5, nhờ người dùng tải tay.
- Bàn phím/gamepad, kiến trúc lõi, hiến pháp: giữ nguyên CVGP/CVM2.

## §1 Trạng thái cụm

| # | taskId | Trạng thái | Commit | Ngày | Ghi chú |
|---|---|---|---|---|---|
| 00 | cvm3-00-ha-tang-so-ghi-nho-va-baseline | **XONG** | (commit "CVM3 00") | 21/10/2026 | sổ này + commit đồ planner + kiểm mẫu số (§7) + baseline 3 phiên Play: **mọi hash = Bảng hash chuẩn CVM2 12** (Arena `full` 14/14, core, Main 14 bộ), H8 5/5 ×3 fps |
| 01 | cvm3-01-nhap-sheet-level3-va-kiem-ke | **XONG** | (commit "CVM3 01") | 21/10/2026 | sheet Level 3 chép nguyên byte (sha256 khớp) + SOURCES; kiểm kê chạy lại y hệt planner (JSON + ảnh trùng byte, 125 tile 8×8 / 83 metatile); đối chiếu Tilesets (§3b); sprite Block 3 đủ 32 ô, verify 0 lỗi; không đổi runtime ⇒ hash chuẩn giữ nguyên |
| 02 | cvm3-02-hieu-chuan-video-timeline-va-su-kien | **XONG** (Simon phủ 87 % < 90 % — §6) | (commit "CVM3 02") | 21/10/2026 | hiệu chuẩn tinh 15+8 frame (ZNCC 0.973 / grad 0.943), HUD + camera (cx −112 WalkwayB) + Simon từng frame, chuyển cảnh theo tick, 161 sự kiện, **Map_3 0 lần trúng đòn / boss 3 lần −3 vạch**, âm thanh; `ref/MAP3_TIMELINE.md` viết lại + `ref/data/*`; không đổi runtime ⇒ hash chuẩn giữ nguyên |
| 03 | cvm3-03-dong-ho-stopwatch-theo-video | **XONG** | (commit "CVM3 03") | 21/10/2026 | đo 4 lần dùng đồng hồ Map_2 (Map_3/Boss_Map_3 0 lần) ⇒ mô hình **đóng băng hằng 186 tick từ TICK BẤM + roi vẫn vung + dùng lại khi còn hiệu lực (tính lại) + TIME bỏ mốc rơi trong lúc băng + bẫy nối lại ở mốc lưới** (§2d); Config 300 → 186 (`đo:cvm3-03`); probe mới `rules stopwatch_time/chain`, `mech crusher_resume_video` (bẫy +192 = video); hash đổi 9 bộ (chỉ kịch bản đồng hồ — §4); luồng thật `caf21ae2`/PRE4/H8 y nguyên; nhạc-dừng KHÔNG dựng |
| 04 | cvm3-04-simon-knockback-nuoc-thanh-mep-roi | **XONG** | (commit "CVM3 04") | 21/10/2026 | đo **6 lần trúng / 3 video** (Boss_Map_1 ×1, Boss_Map_2 ×3, Boss_Map_3 ×2; Map_1/2/3 0 lần) ⇒ knockback = **bảng theo tick** (lên 3,3,2,1,1,1,1,1,0,1 = 14 px sau tick 10, giữ đỉnh 14 tick, xuống gương, chạm sàn tick 34) + **lùi 1 px/tick SUỐT cung = 34 px** (12 px cũ = bị mép màn khoá x 239 chặn); lọ nước thánh: 10 lọ Map_3 **dư 0 px** (vx 1.5, vy0 −1.5, g 33/256, vy += g trước, sinh Y chân − 19, dò sàn ở tâm lọ) + 4 lọ Boss_Map_1 rms 0.45–1.44; lửa fire_0→4→0→3 ×9 tick sống 76; **đi khỏi mép 9 lần ⇒ dò sàn X±6** (rơi sớm 2 px); mép phải màn khoá **cx + 239**; Unity dựng lại 5 knockback (dư 0 px theo đồng hồ x), 46 điểm lọ (0 px), 7 mép (0 px); 14 bộ Arena đổi hash — từng kịch bản đổi có nguyên nhân (§4), core giữ; `map1_route`/`map2_route` GHI LẠI (lệch do dò sàn — chứng minh tick 3196) ⇒ flow/flow30/flow144/map2 `4532ace7`, PRE4 `59cafbaa`; H8 5/5 ×3; spam1–7 0 lỗi; Map2AcceptProbe `damage_halla` + `knock_pit_hallb` ĐẠT; xác nhận 9 bộ Arena @144 = EXPECTED mới |
| 05 | cvm3-05-tilemap-va-luoi-va-cham-block3 | **XONG** | (commit "CVM3 05") | 21/10/2026 | tileset **125 tile** + 5 Tilemap Block 3 (`CVM3/Build Map3 Tilemaps`), **WalkwayB nới trái 112 px** (cx video −112 ⇔ 0; 17 ô đá + trời chép ô có sẵn, đo nền trung vị video 176/176 kiểm chứng); lưới 83 metatile + 4 override (khối vỡ WEA (18,8) video 0:26.63, phòng Mummy (83,9) Boss_Map_3 7.6 s; 2 StairFloor) + 43 check; 9 thang (2 zig-zag), 2 thang ↖ cắt đỉnh, 2 cửa PHẢI (X ≥ cột − 12, entry 48), 9 hố (3 khe cầu hỏng St9), khoá cx 1280; vết Simon video **3 220 frame 0 lệch**; Unity **25 ảnh 0 px** so sheet + **24 cặp video** đã xem; probe **32/32**; Block 1 + 2 **574 file md5 y hệt**; mọi hash bảng chuẩn y nguyên (3 phiên), H8 5/5 ×3 |
| 06 | cvm3-06-quet-hoi-quy-giua-cum | chưa chạy | | | |
| 07 | cvm3-07-skeleton-va-fleaman-do-theo-video | chưa chạy | | | |
| 08 | cvm3-08-raven-enemy-dung-chung-va-roi-vat | chưa chạy | | | |
| 09 | cvm3-09-mummy-men-va-phong-boss-that | chưa chạy | | | |
| 10 | cvm3-10-dat-vat-the-nen-bi-mat-va-spawn | chưa chạy | | | |
| 11 | cvm3-11-luong-stage-7-9-va-stage10 | chưa chạy | | | |
| 12 | cvm3-12-nghiem-thu-map3-so-voi-video | chưa chạy | | | |

## §2 Số đo

### 2a. Hiệu chuẩn video → lưới NES (routine 02, 21/10 — `ref/MAP3_TIMELINE §1`; `Tools/cvm3/nes3.py` CALIB)
| | Map_3 (1488×760, 7548 f) | Boss_Map_3 (1370×772, 1282 f) |
|---|---|---|
| `x_NES = (x_v − ox)/sx − ½`, `y_NES = (y_v − oy)/sy − ½` | **sx 3.862, sy 3.310, ox 240.044, oy −21.92** (15 frame, **ZNCC TB 0.9728**; sai số sx ±0.003, ox ±0.3 px video) | **sx 3.845, sy 3.372, ox 195.76, oy −22.392** (8 frame, **ZNCC gradient TB 0.9431**) |
| Khung game mới/s | **22.7** (24 % lặp) ⇒ mốc ±2 tick | **19.6** |
(Planner 17/10: sx 3.859 / ox 240.93, boss sy 3.363 / oy −21.77 — lệch < 1 px NES.) Watermark: Map_3 nút nhỏ góc trên-phải; boss chữ mờ giữa HUD (y 16–30) + ko-fi dưới-trái (y 214–222) — không ảnh hưởng đọc số.

### 2c. Số đo Map 3 chính (routine 02; chi tiết `ClaudeNotes/cvm3/ref/MAP3_TIMELINE.md` + `ref/data/`)
- **Trúng đòn**: Map_3 **0 lần** (PLAYER 16/16, P-04 suốt). Boss_Map_3 **3 lần, mỗi lần −3 vạch** (10→7→4→1; 1 vạch/≈ 7–8 tick); knockback #1 **+26 px ngược hướng mặt, đỉnh −13 giữ ~18 tick, ≈ 32 tick**; #2/#3 sát tường phải (x 239): **không di chuyển ngang nhưng vẫn nhảy −12, ≈ 30 tick**. Hồi: thịt nướng +6 vạch, quả cầu ~7.6 tick/vạch. (`ref/data/map3_hits.csv`.)
- **Chuyển cảnh**: bản đồ B2 trượt −1.503 px/tick dừng f81, Simon nhỏ +1.004 px/tick, **đen 26 tick**, STAGE 07 TIME 0500 f207; thang lên-trái đen **22 / 24 tick**; cửa phải: cuộn 128 px ×2 ở **~1.0 px/tick** (≈ 124–126 tick mỗi lần) + dừng **~158–162 tick**, STAGE +1 cuối cuộn 2, tổng **408 / 414 tick**, TIME đứng ~522 / 508 tick; cx kẹp 1280 (Simon wx ≥ 1408); Mummy +3000 **mỗi con**, ENEMY 16 vạch dùng chung; quả cầu ≈ 150 tick sau Mummy 2 (boss ≈ 120), nhặt → chờ **226 tick** → TIME **1.01 tick/đv** (+10 điểm/đv) → nghỉ 54 → tim **7.78 tick/tim** → 122 → đen 22 → STAGE 10 f7464.
- **WalkwayB cx min −112** (camera bám Simon, không kẹp trái); cột trái = trời thuần, mặt cầu bắt đầu x = 0, vách đá thò tới x ≈ −32. Icon vũ khí cuối = **II** (planner ghi III sai).
- Nhạc: Wicked Child t0 6.862 s (= lúc hiện STAGE 07), lặp 45.27 s; Poison Mind ≈ 198.9 s (~75 tick sau khoá); Stage Clear = nhặt cầu; tally/bản đồ B3 im 4:06.9 →.

### 2d. Đồng hồ (Stopwatch) — routine 03 (21/10; `Tools/cvm3/stopwatch3.py` ⇒ `ref/data/stopwatch3_uses.txt`; tick = frame × 2.0033, Map_2 23.7 khung mới/s ⇒ mốc ±2–3 tick)
Map_3 + Boss_Map_3: **0 lần dùng** (Map_3 cầm đồng hồ tới 0:16 rồi đổi nước thánh, tim không −5). Map_1/Boss_Map_1/Boss_Map_2: không dùng. ⇒ 4 mẫu, đều ở Map_2:

| # | Bấm (tim −5) | Simon lúc bấm | Đóng băng bắt đầu | Chạy lại (đo) | TIME |
|---|---|---|---|---|---|
| 1 | f4829 (2:41.0) STAGE 06 Hall C, 4.0 tick sau mốc TIME f4827 (271→270) | khung chuẩn bị ROI (roi sau lưng f4829–31, duỗi trái f4835) | cùng frame (bẫy đứng từ f4829) | bẫy 1 k1→k0 + bẫy 2 k0→k1 **f4923** (cùng frame TIME 270→269; Bone Tower đứng f4829–4922) | 0270 giữ **192.3 tick** (= 3 mốc, bỏ 2) |
| 2 | f6837 (3:47.9) trận Medusa, 42.1 tick sau mốc f6816 (212→211) | chuẩn bị roi f6837–42, duỗi f6843 | Medusa (bay 0.875) đứng tại (78,139) TỪ f6837 | — (lần 3 bấm khi còn đứng) | |
| 3 | f6928 (3:50.9) = 176–184 tick sau #2 (f6926–27 lặp khung) | roi ra f6929 | Medusa vẫn đứng | Medusa chạy lại **f7021** ⇒ **186.3 tick** sau bấm (khoảng 181–192 tính cả khung lặp + ≤ 1.1 tick tới px đầu) | |
| 4 | f7031 (3:54.4), 20 tick sau khi Medusa chạy lại | (Simon không đo được) | Medusa đứng từ f7030/31 | Medusa chết f7069 trong lúc băng | 0211 giữ **643.1 tick** f6816→f7137 (bỏ 9 mốc 64…576, giảm lại ở mốc 640 ±3) |

**Mô hình chọn** (giải thích cả 4 mẫu): (a) đồng hồ có hiệu lực NGAY tick bấm (cùng tick trừ tim) và Simon **vẫn vung roi** (3/3 mẫu thấy roi); (b) đóng băng **hằng D = 186 tick** (Medusa trực tiếp 186.3; ràng buộc #1: kết thúc trước mốc TIME f4923 ⇒ D < 186.3–190.3 ⇒ chọn 186, sai số ±3); (c) **dùng lại khi còn hiệu lực được** (−5 tim, tính lại D từ lần bấm mới — #3 bấm 176–184 tick sau #2 khi Medusa vẫn đứng); (d) nhịp TIME (64 tick) **vẫn chạy** trong lúc băng, mốc rơi vào lúc băng **bị bỏ** (không trừ, không dồn) ⇒ TIME giảm lại ở mốc kế tiếp sau khi hết băng — #1: 192 (video 192.3), chuỗi #2–4: 640 (video 643.1); (e) bẫy ép: lưới 16 tick vẫn chạy, mốc lưới rơi vào lúc băng bị bỏ ⇒ nấc kế ở mốc lưới đầu tiên sau băng — Unity +192 = video (f4923).
**Loại bỏ**: "hết băng ở mốc TIME thứ 3" (Medusa chạy lại f7021 lệch mốc 26.7 tick; chuỗi #2–4 sẽ cho TIME giảm 3 lần); "nhịp TIME dừng hẳn khi băng" (#1 ⇒ TIME giảm ở 250, chuỗi ⇒ 619 ≠ 643); "D = 183–184 + không dùng lại được" (cần #3 rơi đúng mép 184–184.3 của khoảng 176–184 — kém hợp lý hơn); "3 đv = 192" (Medusa 186.3 + #1 cần < 190).
**Nhạc dừng 2.40 s** khi dùng đồng hồ: 1 mẫu dương (#1 Stalker đứng), #2 dùng 0.24 s TRƯỚC khi Poison Mind bắt đầu và PM vẫn đúng giờ, #3/#4 PM không đứng (±0.4 s) ⇒ "chỉ dừng nhạc stage" khớp cả 4 nhưng chỉ 1 mẫu dương ⇒ **KHÔNG dựng** (giữ câu hỏi `CVM2 §5 #7`; Map_3 không có mẫu mới).
Unity sau sửa (probe): rules `stopwatch_time` băng 5–190, TIME giảm ở +192 ✓; `stopwatch_chain` bấm 42/224/431 ⇒ băng liền 43–617, TIME giảm ở +640 ✓ (video 643.1 ±3); mech `crusher_resume_video` bấm 4 tick sau mốc lưới ⇒ 3 bẫy đổi nấc lại CÙNG tick ở mốc +192 (video +192.3) ✓; Medusa/Phantom Bat giữ 186 (b24 `medusa_stopwatch`, boss `stopwatch`); Mummy Men + băng gạc KHÔNG bị giữ (b24 `mummy_stopwatch` — wiki "vô dụng", đã có từ CVGP 19).
Ảnh (đã xem): `ref/nes/cvm3_03_use1_whip.png` (#1 roi + bẫy), `cvm3_03_medusa_freeze_start.png` (#2), `cvm3_03_medusa_resume.png` (f7016–7033), `cvm3_03_unity_crusher_resume.png` (Unity rel 28 / 214 / 216); thêm `_cache/shots/03/`.

### 2e. Knockback / lọ nước thánh / mép rơi — routine 04 (21/10; `Tools/cvm3/kb4_*.py`, dữ liệu `ref/data/kb4_*.csv|txt`, ảnh `ref/nes/cvm3_04_pairs_*.png`)
**Công cụ**: `kb4_common.py` (6 video, hiệu chuẩn CVGP/CVM2/CVM3 02, khớp mẫu sprite Simon) — **đọc frame bằng `-ss … -copyts` + lọc `select=gte(t,…)`**: `-ss` thường của các script cũ lệch 1 frame tuỳ keyframe (Map_3 f6088, Boss_Map_2 hit #3 — csv CVM2 02 đúng chỉ số). `kb4_track.py` (bám Simon từng frame + dải ảnh), `kb4_visible.py` (nháy bất tử), `kb4_knock.py` ⇒ `ref/data/kb4_knock.csv`, `kb4_holy_all.py`/`kb4_holy_boss1.py`/`kb4_holy_fit.py`/`kb4_holy_table.py` ⇒ `ref/data/kb4_holy.csv`, `kb4_flame.py`, `kb4_ledge.py` ⇒ `ref/data/kb4_ledge.txt`, `kb4_compare.py` (Unity ↔ video), `kb4_pairs.py` (cặp ảnh), `kb4_hashdiff.py` (hash từng kịch bản 2 lần chạy).

**Knockback** (6 lần trúng đo được ở 3 video; Map_1/2/3 = 0 lần; Boss_Map_3 #3 bị Mummy che). **Đồng hồ x**: Simon lùi đúng 1 px/tick nên độ dời ngang = số tick (frame-clock của bản quay lệch ±2 tick vì frame lặp/khựng):
| Video | Lần | Nguồn / tư thế | Lùi | Tường | Mẫu đồng hồ x (cao theo t) |
|---|---|---|---|---|---|
| Boss_Map_1 | 1 | Phantom Bat chạm thân, đứng quay trái quất | 12 px → **dừng x màn 239** (không có ô tường — mép màn khoá) | phải (mép màn) | t3 8, t9 13 (lệch 1 tick pivot use_stand/hurt) |
| Boss_Map_2 | 1 | Medusa, đi phải | 10 px → tường/cửa trái x 23 | trái | t2 6, t4 9, t6 11, t8 13, t10 14; sau tường (frame) t24 14 → t26 13, t28 11, t30 9, t32 6 |
| Boss_Map_2 | 2 | Medusa, đứng sát tường trái quất | 0 | trái | chỉ frame-clock (giữ đỉnh 14, xuống 13/11/9) |
| Boss_Map_2 | 3 | rắn từ phía trước, quất phải | 32 px tới t32 (chưa đáp) | không | t2 6, t6 11, t10…t24 14, t26 13, t28 11, t30 9, t32 6 — **14 mẫu dư 0** |
| Boss_Map_3 | 1 | băng gạc Mummy trái, quất trái | 32 px tới t32 | không | t2 6, t6 11, t10…t24 14, t26 13, t28 11, t32 6 — **14 mẫu dư 0** |
| Boss_Map_3 | 2 | băng gạc, NGỒI quay trái sát mép phải | 10–11 px → x màn 239 | phải (mép màn) | t1 3, t7 12 |
**Mô hình chọn** (khớp mọi mẫu đồng hồ x dư 0 px; tick lẻ từ Boss_Map_1 + Boss_Map_3 #2): cao(t) = 0,3,6,8,9,10,11,12,13,13,**14** (t10) … giữ 14 tới t24 … 13,13,12,11,10,9,8,6,3,**0 (t34)** = bảng `KnockbackRise01..10` 3,3,2,1,1,1,1,1,0,1 + `KnockbackApexTicks` 14 + xuống gương (đối xứng quanh t17); ngang `KnockbackSpeedX` 1 px/tick **suốt cung** ⇒ 34 px trên sàn phẳng. Cao 14 ở cả 3 video (CVGP 08: 15–16 parabol; CVM2 09: "36 tick / giữ ~16" = frame-clock). Chưa chạm đất sau tick 34 (bị hất khỏi mép) ⇒ rơi FallSpeed, vẫn lùi (suy luận như hết bảng nhảy — video không có mẫu). Ngồi khi trúng ⇒ cùng cung, khung hurt_0 (Boss_Map_3 #2). Tường chặn ngang, cung dọc chạy tiếp (4 mẫu). **Hướng**: 6/6 lùi ngược hướng mặt NHƯNG cả 6 đòn đều tới từ phía trước ⇒ video KHÔNG phân biệt "ngược hướng mặt" với "ra xa nguồn đòn" — giữ `−Facing` (CVGP 08), §5 #9.
**Bất tử nháy** sau khi đáp: hiện liên tục lại sau 45–51 frame (90–102 tick, trung vị ≈ 96) ở 6 lần ⇒ giữ `InvincibleTicks` 94 (`ref/data/kb4_invinc.txt`).
**Mép màn khi camera khoá** (phòng boss): phải **x màn 239** (Boss_Map_1 knockback dừng, Boss_Map_3 ngồi rồi knockback kẹt — 3 mẫu 2 video; Unity cũ cx + 248); trái: Map_3 phòng Mummy x màn tới 11 ⇒ giữ cx + 8. Boss_Map_2 trái dừng x 23 = cột cửa `D` (x 0–15) — Unity cửa không đặc ⇒ §6.

**Lọ nước thánh** (`ref/data/kb4_holy.csv`): Map_3 trận Mummy 9 lọ từ khối cao (Simon (22,144) quay phải, sàn 208) + 1 lọ nhảy-ném (f5198): x = X Simon + floor(1.5·n) **mọi điểm**, y theo **vy += g rồi y += vy** với **vy0 −1.5, g 33/256 = 0.12890625**, tâm lọ lúc sinh = **Y chân − 19** ⇒ **46 điểm quỹ đạo (+37 của lọ nhảy-ném) dư 0 px**; sinh n = 0 ở tick 15–19 sau frame tim −1 (trung vị 17; Unity: tick 16 = duỗi + 3); lọ còn VẼ ở n48 (Y 204 — đáy lấn sàn 208 3 px), vỡ n49, lửa ở X lọ n49 (95) ⇒ **dò sàn ở tâm lọ** (khoảng hợp −1..3; Unity cũ dò hàng dưới đáy +8 ⇒ vỡ sớm 1 tick). Boss_Map_1 (bảng màu riêng, sàn phẳng, quay trái) 4 lọ: rms 0.45 / 0.67 / 0.50 / 1.44 px với cùng hằng số (X0 − Simon −1.6..+0.5, y0 − chân −19.1..−20.1 trong sai số pivot), lửa_0 hiện rel 24 sau tim −1 ⇔ n ≈ 32 = mô hình trên sàn phẳng (CVGP 16 "26–28 tick / 40–44 px" là ước lượng thô sai). Lọ không lật khi ném phải, lật khi ném trái (Unity đúng sẵn). **Lửa** (4 chuỗi trọn Map_3 + Boss_Map_1): **fire_0 → 1 → 2 → 3 → 4 → 0 → 1 → 2 → 3 rồi tắt**, mỗi khung **9 tick** (8 khoảng = 70–72 tick), không lặp; vỡ → tắt **75–78 tick** ⇒ `fireTicks` 76 (CVGP 09: lặp fire_1..4 9/6/12/7, 80 — không thấy fire_0). Hàng đáy lửa = mặt sàn − 1 (Boss_Map_1 f483–500) = Unity sẵn.

**Mép rơi** (`ref/data/kb4_ledge.txt`; mép = cạnh ô đặc hàng sàn trong lưới va chạm Unity Block 1/2): 9 lần (Map_1 EH ←, CB →; Map_2 Hall A → ←, Hall B → ← ←, Medusa → ←): **còn đứng khi thân qua mép 5 px, rơi ở 6 px** cả 2 hướng ⇒ đầu dò sàn = cột **X−6..X+6** (`FootProbeHalfWidth` 6) cho cả ĐỨNG lẫn ĐÁP (Unity cũ: cả thân X−8..X+7 ⇒ rơi trễ 2 px). Xác nhận thêm: SimonProbe `jumpwall` nay đáp tường thấp ở tick nhảy **30 = video 2:03.7** (cũ 29).

**Unity ↔ video** (`Kb4Probe` scene Arena → `_cache/04/unity3/compare.txt`; cặp ảnh đã xem `ref/nes/cvm3_04_pairs_kb_left.png`, `…kb_right.png`, `…holy.png`, `…ledge_eh.png`):
| Dựng lại | So với | Kết quả |
|---|---|---|
| `kb_free_l` (quay phải, lùi trái) | Boss_Map_2 #3 | 14 mẫu đồng hồ x dư 0; frame-clock rms x 1.00 / y 1.08 |
| `kb_free_r` (quay trái, lùi phải) | Boss_Map_3 #1 | 14 mẫu dư 0; rms 1.00 / 1.00 |
| `kb_wall_l` (tường trái sau 10 px) | Boss_Map_2 #1 | 5 mẫu dư 0; frame-clock rms 0 / 0 (13 mẫu) |
| `kb_wall_r` (mép màn khoá phải sau 12 px) | Boss_Map_1 | 2 mẫu dư −1 (pivot); rms 0.47 / 0.47 |
| `kb_duck_wall_r` (ngồi sát mép phải) | Boss_Map_3 #2 | 2 mẫu dư 0; rms 0.95 / 1.76 (frame-clock trôi — 5 frame lặp) |
| `holy_drop64_r` (tâm lọ cao hơn sàn 83 px) | Map_3 9 lọ | 46 điểm dư 0; vỡ n49, lửa x +73 = video |
| `holy_flat_l` (sàn phẳng, quay trái) | Boss_Map_1 | sinh (300,157) tick 18, vỡ n32 x −48, lửa 76 tick |
| `ledge_*` ×7 | Map_1 ×2, Map_2 ×5 | x tick rơi đầu = video x0 (921, 134, 326, 281, 601, 710, 217) — 7/7 lệch 0 |
Cặp ảnh mép (`…ledge_eh.png`) ghép theo frame video ±2 tick (frame rơi đầu video đã rơi 3 tick) — vị trí x đúng, dọc lệch pha.

### 2f. Địa hình Block 3 — routine 05 (21/10; dữ liệu `Tools/cvm3/level3_sections.json` + `level3_collision_overrides.json` → `Assets/Data/Config/Level/Block3/<Section>.asset`; khuôn CVM2 `§2h`)
**Toạ độ section** = px trong vùng 192 dòng của sheet (gốc trên-trái, y xuống), dòng r ↔ NES y 32 + r (25 ảnh Unity 0 px so sheet, 24 cặp video). **WalkwayB NỚI TRÁI 112 px** ⇒ x_section = x_sheet + 112 = x_video(world_x CSV) + 112, cx_section = cx_video + 112 (video cx −112 ⇔ 0). Camera cx ∈ [0, rộng − 256], không cuộn dọc. Toạ độ Simon video = `ref/data/map3_simon.csv` (world_x = tâm thân theo gốc sheet, y_sec = mặt sàn; flip 1 = phải).

| Section | Stage | Rộng | cx max | Điểm vào stage (x, y mặt sàn, hướng) | Nguồn |
|---|---|---|---|---|---|
| WalkwayEntranceA | 7 | 768 | 512 | **(720, 160) quay TRÁI**, cx 512 | video 0:06.90 f207–226 (idle_0 flip 0 đứng 20 frame) |
| WalkwayEntranceB | 7 | 768 | 512 | — (vào bằng thang ↖ từ WEA) | |
| WalkwayA | 8 | 768 | 512 | **(48, 128) quay PHẢI**, cx 0 | video 1:28.17 f2645 (sau cửa phải #1) |
| WalkwayB | 8 | **880** (768 + nới 112) | **624** (= video 512; cx 0 = video −112) | — (vào bằng thang ↖ từ WalkwayA) | camera video bám Simon tới cx −112 f3483 (Simon x 16 sheet = 128 sau nới) |
| MummyMan | 9 | **1536** | 1280 (**khoá cx 1280** = mép PHẢI như Phantom Bat) | **(48, 96) quay PHẢI**, cx 0 | video 2:31.17 f4535 (x ghép 816 = 48); khoá 3:17.70 f5931 |

| Nối | Kiểu | Trigger (px section nguồn) | Đích · targetOrigin · điểm vào | Bằng chứng video |
|---|---|---|---|---|
| WEA `stair_up_to_WalkwayEntranceB` | Stair | `WalkwayEntranceA_stair0` **↖ 12 bậc** (152,88)→(64,0) cắt đỉnh | WEB `stair1` · **(−32, −192)** — WEB(x,y) = WEA(x+32, y+192) | 0:36–0:39.53 leo; đen 22 tick; WEB 0:39.93 Simon (96,189) = WEA (64,−3) ngay trên đỉnh |
| WEB `stair_down_to_WalkwayEntranceA` | Stair | `WalkwayEntranceB_stair1` ↖ 4 bậc (88,184)→(64,160) cắt đáy | WEA `stair0` · (32, 192) | nghịch đảo (video chỉ lên) |
| WEB `door_to_WalkwayA` | **Door PHẢI (dir +1)** | cột cửa [752,80,8,48]; **X ≥ 740** trên đất Y 128 | WalkwayA · **(768, 0)** (ghép LIỀN PHẢI) · entry **(48, 128)** | 1:21.33 f2440 Simon đáp x 742 → cuộn; cửa #2 cho ngưỡng 740 = cột 752 − 12 (y hệt Map 1 1528 → 1516); Simon tự đi ghép 742 → 816 = WA 48 |
| WalkwayA `stair_up_to_WalkwayB` | Stair | `WalkwayA_stair1` **↖ 8 bậc** (632,56)→(576,0) cắt đỉnh (sau ↗ `stair0` (576,120)→(632,64) + chiếu nghỉ khối (40-43,4) y 64) | WalkwayB `stair0` · **(368, −192)** — WB(x,y) = WA(x−368, y+192) | 1:44–1:48.73 leo ↗ rồi ↖; đen 24 tick; WB 1:49.17 cx −33 (= 79), Simon x ≈ 95 sheet (= 207) |
| WalkwayB `stair_down_to_WalkwayA` | Stair | `WalkwayB_stair0` ↖ 8 bậc (200,184)→(144,128) cắt đáy | WalkwayA `stair1` · (−368, 192) | nghịch đảo |
| WalkwayB `door_to_MummyMan` | **Door PHẢI** | cột cửa [864,48,8,48]; **X ≥ 852** (= 740 sheet) trên đất Y 96 | MummyMan · **(880, 0)** · entry **(48, 96)** | 2:24.60 f4333–4338 Simon đi x 735, 736, 739, 740 sheet → f4338 cuộn; tự đi ghép 740 → 816 = MM 48 |
| WEA `door_entry` | Door +1, KHÔNG nối | [752,112,8,48] | — | cửa VÀO stage (trang trí) — Simon bắt đầu x 720 quay trái, không tới mép phải |

**Thứ tự đi trong video (khớp đồ thị):** WEA 0:06.90 (STAGE 07) → (thang ↖ cắt đỉnh) WEB 0:39.93 → (cửa PHẢI #1) WalkwayA 1:28.17 / STAGE 08 → (thang ↗ + ↖ cắt đỉnh) WalkwayB 1:49.17 (cx −33 … −112) → (cửa PHẢI #2) MummyMan 2:31.53 / STAGE 09 → cx 1280 khoá 3:17.70. Cả 2 lần đổi tầng = thang **↖ (lên-TRÁI) cắt mép trên** (Map 2: ↗); cả 2 cửa = **cửa PHẢI** (Map 2: trái).

**9 cầu thang (tự dò bậc 8×8 cam (252,152,56) 28 px):** WEA `stair0` ↖ 12 (152,88)→(64,0) cắt đỉnh · WEB `stair0` ↗ 8 (64,152)→(120,96) · `stair1` ↖ 4 (88,184)→(64,160) cắt đáy · `stair2` ↗ 4 (384,152)→(408,128) · `stair3` ↗ 4 (448,88)→(472,64) (**không chân/đỉnh đứng được** — gạch xanh nền 2 đầu; video Simon nhảy xuyên, không dùng) · `stair4` ↖ 8 (632,152)→(576,96) · WalkwayA `stair0` ↗ 8 (576,120)→(632,64) · `stair1` ↖ 8 (632,56)→(576,0) cắt đỉnh · WalkwayB `stair0` ↖ 8 (200,184)→(144,128) cắt đáy. **Zig-zag** (WEB stair1→stair0, WalkwayA stair0→stair1): đỉnh thang này cách chân thang kia 8 px ngang trên cùng mặt sàn ⇒ `TryEnterStair` kẹp vào thang kế ở t 5 khi giữ Lên (video WEB f1228→1233 x 66→71 liền mạch) — Unity `web_arrive_zigzag`/`wa_stairs_zigzag` ĐẠT.

**Lưới va chạm (đếm ô 16×16; phân loại tự động: L1 đỏ cửa (216,40,0) Door 3 · L2 bậc Stair 5 · L3 hồng-cam (252,116,96) ≥ 40 % Solid 1 · Empty 74 metatile = 83 kiểm kê):**
| Section | Empty | Solid | Stair | StairFloor | Door | Breakable |
|---|---|---|---|---|---|---|
| WalkwayEntranceA | 449 | 117 | 6 | 0 | 3 | 1 |
| WalkwayEntranceB | 474 | 85 | 13 | 1 | 3 | 0 |
| WalkwayA | 492 | 76 | 8 | 0 | 0 | 0 |
| WalkwayB | 592 | 61 | 3 | 1 | 3 | 0 |
| MummyMan | 1036 | 115 | 0 | 0 | 0 | 1 |
Water / Pit / Hazard: **0** (hố = rơi ra đáy section; gai đen đáy/trên cầu = bóng nền — Simon đi xuyên bóng gai WalkwayA (8-9,7) f2729–2803).

**Breakable (2) — `Tools/cvm3/terrain3.py broken` (ô thành ĐEN và giữ đen):** WEA **(18,8)** 0:26.63 f799 (98/98 mẫu; khối TRÁI bộ khối nhô thứ 2 đi từ x 720 — wiki St7 Pot Roast; thịt nướng sàn 0:27.3) · MummyMan **(83,9)** Boss_Map_3 7.60 s f228 (509/527; bậc khối thấp nhất phòng Mummy — wiki St9 Pot Roast; Map_3 không phá). Ảnh `ref/nes/cvm3_05_khoi_vo_wea_mummy.png`. Không khối nào khác đen ở 2 video. **"2 khối không với tới" St8** = WalkwayB (15-16,10) sau nới (sheet (8-9,10)) DƯỚI mặt cầu — wiki: QUỲ trên cầu phía trên chúng ⇒ túi tiền (không phải phá) ⇒ giữ Solid, bí mật là việc routine 10 (video lấy +1000 ở mút trái cầu, không quỳ — CVM3 02).
**StairFloor (2):** WEB (4,10) miệng `stair1` (đi xuống WEA) giữa 2 khối sàn y 160 — **suy luận** như CryptA Block 1 (video không đi ngang ô này); WalkwayB (9,8) miệng `stair0` giữa mặt cầu y 128 — video f3336–3346 / f3465–3479 Simon đi ngang không rơi.
**Hố (marker `pit`, tự tính 9):** WEB [96,160,64,32] · [320,160,32,32] (Simon nhảy qua 0:53.4–0:54.4) · [384,160,64,32] · [544,160,64,32] · WalkwayB [496,128,32,64] (khe cầu sheet x 384–415, nhảy qua f3870–3894) · [624,128,32,64] · MummyMan **cầu hỏng St9** [768,96,32,96] · [832,…] · [896,…] (3 khe 32 px giữa 2 bục 32 px; video 2:55.0–2:57.6 Simon nhảy qua cả 3). Ngoài marker còn các "giếng" sát tường (WEA cột 2-3 dưới bờ (2-3,8); WEB cột 44-45 dưới khối cửa; WalkwayB sheet cột 42-45 dưới bậc cửa; trời trái WalkwayB phần nới) — rơi vào cũng ra đáy = Pit (find_pits bỏ vì một bên là tường).
**Đối chiếu video**: **3 220 frame** Simon đứng/đi (idle/walk, err < 0.1, Y giữ ±2 frame) — ô dưới chân X±6 (đầu dò sàn CVM3 04) đặc/thang: **0 lệch** (MummyMan 1523, WalkwayA 287, WalkwayB 516, WEA 426, WEB 468).
**Phần nới WalkwayB** (`Tools/cvm3/wb_pad3.py`): nền trung vị 424 frame cx < 128 so mọi tile sheet (LUT video→sheet): x −112..−33 trời thuần (36,24,140); x −32..−1 hàng 136..183 = **17 ô đá rêu** chép từ ô CÓ SẴN của chính WalkwayB (gạch rêu (96..104, 0..8), mép đá (64..80, 64..72), (448/472, 72), (128,0)), rms 8–31 vs ô nhì ≥ 86; kiểm chứng x 0..63: **176/176** ô đúng; hàng 0–15 sau HUD ⇒ trời. Không thêm tile 8×8 (125) hay metatile (83) mới. Ảnh `ref/nes/cvm3_05_wb_pad_compare.png` (trên video, dưới dựng lại). **4 section còn lại không vượt mép sheet**: camera video WEA/WEB/WalkwayA cx ∈ [0, 512], MummyMan [0, 1280] (CVM3 02 `MAP3_TIMELINE §2`) ⇒ không nới. **MummyMan 1536 px** dựng bình thường (Block 1 EntranceHall cũng 1536): 6 ảnh Unity tới cx 1263/1280 0 px, probe đi tới x 1460.

**Số đo phụ (cho 10/11):** cửa phải: kích hoạt X ≥ cột − 12 khi đứng trên gờ cửa (2 cửa), Simon tự đi **74 / 76 px** tới x đích **48** (WEB 742 → 816, WB 740 → 816 — như Map 2 "tự đi tới entry.x"); đổi tầng thang ↖ đen 22 / 24 tick; WalkwayB camera bám Simon (cx = X − 128) không kẹp trái tới −112 trong video — Unity kẹp ở cx 0 (= −112); nếu Simon đi trái hơn x 128 sau nới thì video không có mẫu (cầu hết ở x 112, rơi xuống trời).

### 2b. Mốc chính (planner; `ref/MAP3_TIMELINE.md §3`) — đã trích lại ở routine 00 (ảnh `_cache/shots/00/sheet.png`)
| Mốc | Sự kiện | Kiểm 00 |
|---|---|---|
| 0:06.8–7.6 | STAGE 07, TIME 0500, đi trái (WEA cx 512) | 6.8 s còn đen (fade-in); 7.6 s đúng STAGE 07 TIME 0500 ✔ |
| 0:40.0 | WEB cx 0 sau thang lên-trái | TIME 0470 STAGE 07, Simon ở bậc thang ✔ |
| 1:24 | cửa phải #1 mở | TIME 0432 STAGE 07, cửa đỏ ✔ |
| 1:28.2 | STAGE 08 (WalkwayA) | TIME 0432 STAGE 08 ✔ |
| 1:49.4 | WalkwayB cx âm | TIME 0413 STAGE 08, trời trái ✔ |
| 2:31.6 | STAGE 09 (MummyMan) | TIME 0380 STAGE 09, cầu hỏng ✔ |
| 3:18 | khoá phòng Mummy, 2 Mummy tím | TIME 0337 STAGE 09 ✔ |
| 4:09.8 | bản đồ B3, STAGE 10 | TIME 0000 STAGE 10, bản đồ cam ✔ |

## §3 Tài nguyên (đường dẫn sinh ra)
- Đồ planner: `ClaudeNotes/cvm3/CVM3_PLAN.md`, `ref/MAP3_TIMELINE.md`, `ref/nes/*.png`, `ref/tools/*.py`, `wiki/MAP3_WIKI_DATA.md`, `inventory/level3_walkway.json` + `_annot.png`. `_cache/` (video, CSV, lưới ảnh, tools chạy được, bộ sinh routine, `00/` kết quả baseline, `shots/00/`) KHÔNG commit.
- Kết quả baseline routine 00: `ClaudeNotes/cvm3/_cache/00/{arena,core,main}/suites/SUMMARY.txt`.
- **Lớp/menu Map 2 mà cụm sẽ tổng quát** (đọc code thật 21/10):
  - Editor: `Assets/Data/Editor/Cvm2LevelMenu.cs` (`CVM2/Build Map2 Tilemaps`, `CVM2/Build Map2 Objects`; mục "Build Map2 Screens" — builder màn hình, cần idempotent byte, routine 11), `Cvm2MechBuilder.cs` (`CVM2/Build Mech Prefabs`, `CVM2/Spawn/Mech …`), `Cvm2DevMenu.cs` (`CVM2/Start Stage 4/5/6`).
  - Runtime: `Script/Stage/Map2FlowTiming.cs` (hằng thời gian luồng Block 2), `Script/Stage/Stage7Stub.cs` (+ prefab `Prefabs/Screens/Stage7Stub.prefab` — điểm cuối luồng Map 2, hash `caf21ae2`), `Script/Stage/StageScreen.cs` (stage 1–6), `Script/CastleMap/CastleMapScreen.cs` (+ prefab; biến thể B2 → thêm B3), `Script/Object/Map2Mechanisms.cs` (bệ/bẫy), `Script/System/GameRules.cs` (`BlockOfStage(int stage)` L89, `Cfg.GetBlockOfStage`).
  - Probe: `Script/Debug/Map2RouteProbe.cs` (+ lộ trình `Assets/Data/Tests/Replays/map2_route.txt`), `Map2AcceptProbe.cs`, `Map2ObjectsProbe.cs`, `Map2EnemyProbe.cs`, `Map2BossProbe.cs`/`Map2BossRoom.cs`, `Map2MechProbe.cs`, `Cvm2TerrainProbe.cs`, `Cvm2SweepProbe.cs`.
  - Dữ liệu: `Assets/Data/Config/Level/Block2/Map2Objects.json`; công cụ python `Tools/cvm2/*.py`.
  - Hạ tầng hồi quy: `Assets/Data/Script/Debug/RegressionRunner.cs` (`EXPECTED` + `PRE4`).

### 3b. Routine 01 (21/10/2026) — nhập sheet Level 3, đối chiếu Tilesets, sprite Block 3
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Sheet Level 3 | `Art_Source/spriters-resource/level3_walkway.png` | nguyên byte, 3596×618, 96 910 B, sha256 `c8bfc7ef…476c44eee` (khớp); `Art_Source/SOURCES.md` +1 dòng |
| Kiểm kê | `Tools/cvm3/inv_level3_tiles.py` (chạy từ gốc repo, đọc sheet trong repo) | ghi `inventory/level3_walkway.json` + `_annot.png` — **trùng byte** bản planner (kể cả trường `sheet`); 5 vùng, 125 tile 8×8 / 83 metatile 16×16, 0 px khoá xanh |
| Đối chiếu Tilesets | `Tools/cvm3/cmp_tilesets3.py` → `_cache/01/cmp_tilesets3.json`; ảnh `ref/level3/tilesets_xcheck.png` (đã xem) | cột "LEVEL3: CRUMBLING TOWER": lưới ô 32×32 bước 33, gốc (808,92), 5×8 |

**Đối chiếu Tilesets** (so theo HÌNH — bảng màu Tilesets khác bản đồ: 10 màu mỗi bên, chỉ đen chung; vd trời bản đồ (36,24,140) ↔ Tilesets (0,16,132)):
| So | Bản đồ (hình) | Tilesets | Chung | Chỉ bản đồ | Chỉ Tilesets |
|---|---|---|---|---|---|
| 16×16 | 80 | 77 | **74** | 6: WEA(15,1), WEA(14,3), WEA(15,3), WEB(4,10), WEB(5,11), WA(18,7) | 3 |
| 8×8 | 112 | 109 | 105 | 7 (cùng vị trí trên) | 4 |
- (a) **Trời xanh đậm là tile đặc có thật cả hai bên**: bản đồ có 32/161/183/177/430 ô 16×16 đặc (36,24,140) ở WEA/WEB/WA/WB/MM; Tilesets có ô đặc xanh tương ứng ⇒ nới `WalkwayB` sang trái (camera tới cx −109) dùng được tile trời + tile mép cầu CÓ SẴN (routine 05 ghi ô nguồn). 74/80 metatile bản đồ có trong Tilesets ⇒ đủ để routine 05 kiểm dựng Tilemap.
- (b) 6 metatile chỉ-bản-đồ là ô có chi tiết vẽ đè sẵn (cột nến/đài cam-vàng ở WEA(15,1)/(14,3)/(15,3) vùng 3 ô liên tiếp; bục cột trụ xanh ở WEB(4,10)/(5,11); cột đá rêu WA(18,7)) — không thấy dấu sửa tay; routine 05 kiểm lại trên ảnh khi dựng.

**Sprite Block 3** — đủ 32 ô, `verify_sprites.py --meta` **0 lỗi** (246 PNG, 371/371 ô); ảnh gộp `_cache/shots/01/sprites_b3.png` đã xem:
- `Enemy/Fleaman/fleaman_0..1` (16×16, co/duỗi) · `Raven/raven_0..3` (16×16: 2 đậu/cánh gập + 2 vỗ cánh) · `WhiteSkeleton/skeleton_0..1` (16×32) + `bone_0..2` (đứng 8×16 + 2 xoay 16×16) · `Boss/Mummy/mummy_purple_0..2`, `mummy_red_0..2` (16×48), `bandage_0..1` · `Item/moneybag_{red,purple,white}`, `axe`, `dagger`, `holywater`, `roast`, `double`, `triple`, `crystal_0..1` · `Object/door_0` (16×48), `door_1` (24×48).
- **Mummy đỏ**: video Map_3 3:25–3:44 (contact sheet `_cache/shots/01/mm/mm_map3.png`, đã xem) — thân Mummy ĐỔI MÀU tím → đỏ vài khung rồi về tím khi bị trúng (nước thánh/roi); chỉ đổi bảng màu, hình giữ nguyên. `mummy_red_*` có cùng số điểm ảnh từng màu với `mummy_purple_*` (đúng bảng màu đỏ (216,40,0)/(252,116,180) so tím (204,136,252)/(252,196,252)) ⇒ **dùng `mummy_red_*` làm khung nháy trúng đòn** (routine 09 đo nhịp nháy; không cần sprite mới). Mummy tím còn đứng thân trần khi kẹp hai bên.
- Khung có đủ cho 07/08/09? Chỉ đối chiếu số khung (video ≈ 18 khung thật/s): Raven 4 (đậu/bay đủ), Fleaman 2 (co/duỗi — nhảy trên không dùng khung duỗi, routine 07 xác nhận), Skeleton 2 (đi/ném dùng chung khung ném — routine 07 kiểm), Mummy 3 khung đi/giơ tay + 2 băng gạc (routine 09 kiểm khung ném). **Không thiếu sprite**, không cần tải thêm (§5 không thêm câu hỏi).

### 3c. Routine 04 (21/10/2026) — knockback / nước thánh / mép rơi
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Harness đo (Editor, Arena) | `Assets/Data/Script/Debug/Kb4Probe.cs` | `Kb4Probe.Run(Kb4Probe.CaseNames(), dir)` (+ `Kb4Probe.Shots = true` chụp Game view) ⇒ `<case>.csv` vết từng tick + SUMMARY; không trong RegressionRunner |
| Công cụ đo | `Tools/cvm3/kb4_*.py` (chạy từ gốc repo, `python -P -E`) | `kb4_common` (6 video), `kb4_track`, `kb4_visible`, `kb4_knock`, `kb4_holy*`, `kb4_flame`, `kb4_ledge`, `kb4_compare <dir Unity>`, `kb4_pairs <dir Unity>`, `kb4_hashdiff <dir cũ> <dir mới>`; cache `_cache/04/` |
| Dữ liệu | `ClaudeNotes/cvm3/ref/data/kb4_{knock.csv,holy.csv,ledge.txt,invinc.txt,compare.txt}` | bảng mẫu + so Unity ↔ video |
| Ảnh đã xem | `ClaudeNotes/cvm3/ref/nes/cvm3_04_pairs_{kb_left,kb_right,holy,ledge_eh}.png` | hàng trên video, hàng dưới Unity |
| Lộ trình | `Assets/Data/Tests/Replays/map1_route.txt`, `map2_route.txt` | GHI LẠI bằng autopilot (§4) |

### 3d. Routine 05 (21/10/2026) — địa hình Map 3 (Block 3) — routine 06+ đọc mục này + §2f, KHÔNG mở lại sheet
**Dựng lại từ đầu (2 lệnh + 1 menu, từ gốc repo):** `python -P -E Tools/cvm3/build_tileset3.py` → `python -P -E Tools/cvm3/build_collision3.py` → menu **`CVM3/Build Map3 Tilemaps`** (`Assets/Data/Editor/Cvm3LevelMenu.cs` = `CvgpLevelBuilder.BuildMap3()`). Chạy lại ⇒ "same" / "ghi 0". Sửa va chạm = `Tools/cvm3/level3_collision_overrides.json` (expectAuto + reason + bằng chứng); sửa nối/điểm vào/marker = `Tools/cvm3/level3_sections.json`; sửa phần nới WalkwayB = `PAD_LEFT` trong `build_tileset3.py` (đo lại `Tools/cvm3/wb_pad3.py`). KHÔNG sửa tay asset/prefab sinh ra.
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Tileset | `Assets/Data/Art/Level/Block3/level3_tiles.png` (128×64, 16 tile/hàng) | **125 tile 8×8** (= kiểm kê; không gộp lật); sprite `l3t_000..124` (Multiple, pivot tâm, spriteID MD5 tên), import y hệt `level2_tiles.png` (chỉ khác guid/số sprite) |
| Tile asset | `Assets/Data/Art/Level/Block3/Tiles/l3t_NNN.asset` (125) | `Tile` gốc, colliderType None, LockColor |
| Ma trận tile | `Assets/Data/Config/Level/Block3/<Section>_tiles.csv` | 24 hàng × (rộng/8) cột (WalkwayB 110 cột = 14 nới + 96) |
| Mô tả | `Level3Tiles.json` (+ khoá `pads`: WalkwayB trái 112 px, 17 ô nguồn + `padLeft` mỗi section), `Level3Map.json` (lưới, 9 thang, 7 nối + `dir`, `simonStartDir`, 9 marker `pit`, 83 metatile + luật L1–L3) | sinh bởi python |
| Dữ liệu section | `Assets/Data/Config/Level/Block3/<Section>.asset` (`LevelSectionData`) | **không thêm trường** — cửa phải dùng `Connection.dir` +1 (đã có từ CVM2 05), thang ↖ dùng `eStairDir.UpLeft` (đã có); WalkwayB `width` 880 / `cameraMaxX` 624 |
| Prefab | `Assets/Data/Prefabs/Level/Block3/{WalkwayEntranceA,WalkwayEntranceB,WalkwayA,WalkwayB,MummyMan}.prefab` | cùng cấu trúc Block 1/2 (Grid ô 0.5 + `LevelSection` + Tilemap `BG` layer Background); **không FG** (video: Simon vẽ đè tường/cột) |
| Công cụ | `Tools/cvm3/{build_tileset3,build_collision3,wb_pad3,terrain3,accept3}.py`, `level3_sections.json`, `level3_collision_overrides.json` | `terrain3.py broken [map3\|boss3]` / `cell` / `shot` (video ↔ sheet theo ô, cần bộ đệm `_cache/02/work/*_nes.npy`); `accept3.py terrain\|pairs` (Unity ↔ sheet đã nới / video; WalkwayB cx video = Unity − 112) |
| Phần chung (đầu ra Block 1/2 KHÔNG đổi) | `Tools/cvgp/build_tileset.py` (`dedupe_regions`, `tileset_png`, `rebuild_check`, `write_if_changed`), `Tools/cvgp/build_collision.py` (`check_graph`, `section_json`, `render`), `Tools/cvm2/build_collision2.py` (`find_pits`, `solid_like`), `Assets/Data/Editor/CvgpLevelBuilder.cs` (+ `MAP3` Spec, `BuildMap3`) | không sửa file python Block 1/2; C# chỉ thêm Spec/hàm |
| Ảnh overlay va chạm | `ClaudeNotes/cvm3/ref/level3/collision_<Section>.png` (2×, + marker hố tím nét đứt) | đã xem cả 5 |
| Probe (chỉ Editor) | `Assets/Data/Script/Debug/Cvm3TerrainProbe.cs` — `Setup()` (5 prefab Block 3 làm con StageWorld lúc chạy, KHÔNG đổi Arena.unity), `StartShots(dir, "Section:cx,…;…", hideHud)`, `RunMoves("all")` (32 kịch bản), `Report()/SaveReport`, `Cleanup()` (trả camera bám Simon) | không thuộc bộ hồi quy; mọi kịch bản di chuyển phải X cuối ≠ X đầu |
| Ảnh ref (commit) | `ref/nes/cvm3_05_{wb_pad_compare,khoi_vo_wea_mummy,pairs_walkwayb,unity_terrain_montage,play_wb_mut_trai_cx0}.png` | đã xem |
| Bộ đệm (KHÔNG commit) | `ClaudeNotes/cvm3/_cache/05/` (`view/` sheet phóng to + vết Simon, `work/` md5 trước/sau + nền WalkwayB, `moves_*.txt`, `arena/ core/ main/` hồi quy), ảnh `_cache/shots/05/` (`terrain/` 25 ảnh + `terrain.txt` + `terrain_montage.png`, `pairs/` 6 tấm + `pairs.txt`, `video/` khung khối vỡ) | |

## §4 Quyết định kỹ thuật + Bảng hash chuẩn

### Bảng hash chuẩn (H11 — baseline = Bảng hash chuẩn sau CVM2 12; routine 00 đo lại 21/10/2026, code = HEAD `d1bf216`)
*(Sau CVM3 03 / 04: các dòng ghi "→ **CVM3 03**" / "→ **CVM3 04**" là hash mới — cũ ở "Lịch sử đổi hash". CVM3 04 đổi MỌI bộ Arena-Simon + luồng thật; core, H8 giữ.)* Hash đổi chỉ khi routine CỐ Ý đổi hành vi: thêm 1 dòng vào "Lịch sử đổi hash" (cũ → mới, lý do, routine) **và** sửa `RegressionRunner.EXPECTED`. Cờ `EXPECTED` hiện tại (đọc, không sửa): y hệt bảng dưới (long…rules, core, gallery, flow/flow30/flow144/map2, holy, mech, h8 `352/432/448/890/922`) + `PRE4 = 59cafbaa` (CVM3 04; trước `165b9aef`).

| Bộ | Lệnh | Scene | Hash chuẩn (đo 21/10) | Đạt/tổng | Thời gian | Routine đặt | Ngày |
|---|---|---|---|---|---|---|---|
| long | `RegressionRunner.Start("long", fps, dir)` | Arena (Simon) | `b2cc1424` | 1/1 | 21 s | CVGP 11/21 → CVM2 00 → **CVM3 04** | 21/10 |
| simon | `…Start("simon",…)` | Arena (Simon) | `5c1c46ce` | 20/20 | 38 s | CVGP 08/21 → CVM2 00 → **CVM3 04** | 21/10 |
| attack | `…Start("attack",…)` | Arena (Simon) | `62c14a1f` | 21/21 | 26 s | CVGP 09/21 → CVM2 00 → **CVM3 03** → **CVM3 04** | 21/10 |
| item | `…Start("item",…)` | Arena (Simon) | `c8db5123` | 14/14 | 33 s | CVGP 11/21 → CVM2 00 → **CVM3 04** | 21/10 |
| enemy | `…Start("enemy",…)` | Arena (Simon) | `20aa5652` | 20/20 | 104 s | CVGP 13/21 → CVM2 00 → **CVM3 03** → **CVM3 04** | 21/10 |
| boss | `…Start("boss",…)` | Arena (Simon) | `4d7bd652` | 12/12 | 109 s | CVGP 14/21 → CVM2 00 → **CVM3 03** → **CVM3 04** | 21/10 |
| b23 | `…Start("b23",…)` | Arena (Simon) | `46dcc544` | 35/35 | 182 s | CVGP 17/21 → CVM2 08 → **CVM3 03** → **CVM3 04** | 21/10 |
| b46 | `…Start("b46",…)` | Arena (Simon) | `a4cfdcaf` | 26/26 | 135 s | CVGP 18/21 → CVM2 00 → **CVM3 03** → **CVM3 04** | 21/10 |
| b24 | `…Start("b24",…)` | Arena (Simon) | `4066b4db` | 21/21 | 249 s | CVGP 19/21 → CVM2 09 → **CVM3 03** → **CVM3 04** | 21/10 |
| b56 | `…Start("b56",…)` | Arena (Simon) | `9b9b7440` | 14/14 | 221 s | CVGP 20/21 → CVM2 00 → **CVM3 03** → **CVM3 04** | 21/10 |
| holy | `…Start("holy",…)` | Arena (Simon) | `95e3ba78` | 5/5 | 13 s | CVM2 04 → CVM2 09 → **CVM3 04** | 21/10 |
| mech | `…Start("mech",…)` | Arena (Simon) | `d2b0212f` | 15/15 | 59 s | CVM2 06 → **CVM3 03** (+`crusher_resume_video`) → **CVM3 04** | 21/10 |
| rules | `…Start("rules",…)` | Arena (Simon) | `72c7e7bf` | 78/78 | 0 s | CVGP 12/21 → CVM2 00 → **CVM3 03** (+`stopwatch_time`, `stopwatch_chain`) → **CVM3 04** | 21/10 |
| gallery | `…Start("gallery",…)` (240 fps, không chụp; nằm trong `full`) | Arena (Simon) | `ab874ae4` | 75/75 | 486 s | CVGP 21 → CVM2 11 → **CVM3 04** | 21/10 |
| core | `…Start("core",…)` | Arena, `CVGP.ArenaUseDummy` = true | `76516076` | 21/21 | 92 s | CVGP 07/21 → CVM2 00 | 21/10 |
| flow (Title → STAGE 07 + trước STAGE 04 = `59cafbaa`) | `…Start("flow",…)` | Main | `4532ace7` | 2/2 | 294 s | CVM2 11 → **CVM3 04** (lộ trình ghi lại) | 21/10 |
| flow30 / flow144 | `…Start("flow30,flow144",…)` | Main | `4532ace7` (cả hai) | 2/2 ×2 | 294 s mỗi bộ | CVM2 11 → **CVM3 04** | 21/10 |
| h8 (5 mốc CVTI, kèm mọi `flow*`) | tự chạy | Main | Start **L352** · đen **L432** · Intro **L448** · đen **L890** · Stage **L922** | 5/5 ×3 | 0 s | CVTI → CVM2 00 | 21/10 |
| map2 (`Map2RouteProbe.Replay` tua nhanh) | `…Start("map2",…)` (trong `main`) | Main | `4532ace7` | 2/2 | 1 s | CVM2 11 → **CVM3 04** | 21/10 |
| spam1..4 (Map 1) | `…Start("spam",…)` (trong `main`) | Main | *(không hash)* 0 lỗi console | 4/4 | 60–80 s mỗi lượt | CVGP 16/21 | 21/10 |
| spam5..7 (Block 2) | `…Start("spamb2",…)` (trong `main`) | Main | *(không hash)* 0 lỗi console | 3/3 | 60–80 s mỗi lượt | CVM2 11 | 21/10 |

Thời gian đo 21/10 (@60 fps, harness cùng máy): Arena `full` **1672 s ≈ 27.9 phút** (nhiều hơn CVM2 00: 1527 s); core 92 s; Main `main,flow30,flow144` **1383 s ≈ 23 phút** (y hệt CVM2 11). Cả ba `ALL.done` = `OK` (bỏ qua 0, lệch 0, lỗi 0).

**Lịch sử đổi hash:** kế thừa toàn bộ `CVM2_MEMORY §4 "Lịch sử đổi hash"`; cụm CVM3 ghi tiếp dưới đây (cũ → mới, lý do, routine).
- (00, 21/10) **không hash nào đổi** — baseline. Mọi hash của bảng KHỚP.
- (01, 02) không đổi runtime ⇒ không hash nào đổi.
- (05, 21/10) dữ liệu Block 3 mới (chỉ dựng trong probe) + builder/probe Editor ⇒ **không hash nào đổi** — đo lại 3 phiên: Arena `full` 14/14 KHỚP, core `76516076`, Main 14/14 KHỚP (`4532ace7` ×4, PRE4 `59cafbaa`, H8 5/5 ×3, spam1–7 0 lỗi).
- (03, 21/10) **Đồng hồ theo video** (300 → 186 tick, hiệu lực ngay tick bấm + roi vẫn vung, dùng lại khi còn hiệu lực, TIME bỏ mốc trong lúc băng, bẫy nối lại ở mốc lưới — §2d). So từng kịch bản với baseline `_cache/00/arena`: **chỉ kịch bản dùng đồng hồ đổi hash**, mọi kịch bản khác y hệt:
  - attack `eee2cdfb` → `b880de83` (`stopwatch`: kỳ vọng 300 → 186; lần 2 khi còn hiệu lực nay DÙNG ĐƯỢC ⇒ băng 41 + 186 = 227, tim 10, roi vẫn vung)
  - enemy `f288b0ce` → `0a731b36` (`freeze` Zombie: 186)
  - boss `31e6d711` → `c9813c11` (`stopwatch` Phantom Bat: 186)
  - b23 `5eb00c5b` → `50a6b4bf` (`freeze_b23`, `freeze_whip`)
  - b46 `58a8edf9` → `6c8e9a74` (`freeze_b46` kỳ vọng 299–301 → 185–187; `freeze_whip_axe` 10 roi > 186 tick ⇒ dùng lại đồng hồ giữa chừng; `dragon_kill_l1/l2` dùng đồng hồ — vẫn 8 đòn ĐẠT)
  - b24 `9b218696` → `397842ee` (`medusa_stopwatch`, `mummy_stopwatch`, `frank_stopwatch` — đồng hồ bắt đầu ngay tick bấm 201/101/131)
  - b56 `6cb8a1ad` → `c140225d` (`death_stopwatch` dùng 2 lần 101 + 252 để giữ cửa sổ quan sát lưỡi hái như cũ; `drac2_stopwatch`)
  - mech `91215ebd` → `6409b5fe` (`crusher_stopwatch` bấm rel 2 → 14 vì đồng hồ nay có hiệu lực ngay — bẫy 3 vẫn đứng khung 6 & giết; + kịch bản mới `crusher_resume_video`)
  - rules `03fafb24` → `00144e42` (+2 kịch bản `stopwatch_time`, `stopwatch_chain`; 68 → 78 kiểm)
  - **Y nguyên**: long, simon, item, holy, gallery, core; Main: flow/flow30/flow144/map2 `caf21ae2`, PRE4 `165b9aef`, H8 5/5 ×3, spam1–7 0 lỗi — lộ trình `map1_route`/`map2_route` không bấm vũ khí phụ (không có `Sub`/`U+B`) ⇒ không tick nào khác.
  - Replay đổi: `enemy_b46_freeze_whip_axe.txt`, `boss_b56_death_stopwatch.txt` (kịch bản); 6 file khác chỉ đổi dòng mô tả `#` (300 → 186).

- (04, 21/10) **Knockback bảng theo tick 34 px / 34 tick + dò sàn Simon X±6 + mép phải màn khoá cx + 239 + lọ nước thánh/lửa theo video** (§2e). So TỪNG kịch bản với `_cache/03/{arena,arena2,arena3}` (`Tools/cvm3/kb4_hashdiff.py`) — mỗi kịch bản đổi đều có nguyên nhân, kịch bản không dính 4 luật giữ nguyên hash:
  - long `5963c4ba` → `b2cc1424` (lộ trình RegressionProbe rơi khỏi mép/xuống nước — **dò sàn**; chạy lại với `FootProbeHalfWidth` tạm 0 ⇒ ra lại đúng `5963c4ba`)
  - simon `b2954cbe` → `5c1c46ce` (8/20 đổi: hurtL/hurtR/death = **knockback**; ledge32/ledge64/water/pit = **dò sàn** rơi sớm 2 tick; jumpwall = **dò sàn khi đáp** — đáp tick nhảy 29 → **30 = video 2:03.7**; 12 kịch bản khác giữ)
  - attack `b880de83` → `62c14a1f` (chỉ `holy` — **lọ**: sinh Y−19, quỹ đạo video, vỡ n32, lửa 76; 20 giữ)
  - item `7ca5bf56` → `c8db5123` (chỉ `potion` — hết tàng hình chạm bia ⇒ **knockback**)
  - enemy `0a731b36` → `20aa5652` (`touch`, `fireball` — **knockback**; 18 giữ)
  - boss `c9813c11` → `4d7bd652` (`touch` — **knockback**; `holy_hold` — **lọ/lửa**; 10 giữ, gồm `walls` mép trái cx + 8)
  - b23 `50a6b4bf` → `46dcc544` (7 `*_touch`/`*_hit` — **knockback**; 28 giữ)
  - b46 `6c8e9a74` → `a4cfdcaf` (5 `*_touch`/`*_hit` — **knockback**; 21 giữ)
  - b24 `397842ee` → `4066b4db` (medusa_touch, medusa_room_touch, frank_touch, mummy_stand, mummy_kill_l1, frank_kill_l1 — Simon trúng đòn ⇒ **knockback**; 15 giữ)
  - b56 `c140225d` → `9b9b7440` (death_touch, drac1_touch — **knockback**; drac2_holy — **lọ**; 11 giữ)
  - holy `b1033a3f` → `95e3ba78` (5/5 — **lọ**: đối chứng vỡ rel 50 lửa (348,176), xuyên bia/Phantom Bat/Medusa như cũ)
  - mech `6409b5fe` → `d2b0212f` (`plat_pit` — **dò sàn** rời bệ ở x + 6 (kiểm sửa: trước "x + 7 < mép bệ" theo thân); `pit_knockback` — **knockback 34 px bay QUA khe 32 px** ⇒ kịch bản dời Simon 194 → 214, bia 205 → 225 (cuối cung x 180 giữa khe); 13 giữ)
  - rules `00144e42` → `72c7e7bf` (water/pit FallDeath sớm 2 tick — **dò sàn**; damage* — **knockback**)
  - gallery `ffd5b71e` → `ab874ae4` (39/69 replay đổi: 34 có đòn vào Simon ⇒ **knockback** (0 replay có đòn mà giữ hash); 5 replay `*_attack` (bất tử, không đòn): QueenMedusa ⇐ **mép màn cx + 239** (tắt tạm `LockedScreenMaxX` ⇒ ra lại `64d590d7` cũ), Dracula1/Dracula2 ⇐ **dò sàn** (tắt tạm ⇒ `8e91724a`/`b053a40d` cũ), MummyMen/FrankensteinIgor ⇐ cả hai (tắt cả hai ⇒ `3e190686`/`702db6c9` cũ))
  - **Giữ**: core `76516076` (ArenaDummy — không dùng luật Simon); H8 5/5 ×3.
  - **Luồng thật** (cheat bất tử ⇒ không knockback; lộ trình không ném vũ khí phụ): `map1_route` cũ LỆCH — tick khác đầu tiên = **Crypt A tick 3196**: Simon đi phải khỏi mép x 128 rơi ở **x 134** (cũ 136 — dò sàn), đáp sớm 2 tick; sau đó 4 đoạn lệch nữa đều là đi khỏi mép (Crypt A x 182, **Crypt B x 134** (= mẫu video Map_1 2:05.3), Phantom Bat x 118) ⇒ vào lại Crypt A sớm 4 tick, input ghi sẵn lệch nhịp ⇒ không lên được bục x 693 ⇒ dơi không thức ⇒ hết lộ trình ở Phantom Bat (flow/flow30/flow144 `bbdcd542`, map2 `1927acf3` — tất định ×3 fps). **Chứng minh**: phát lại lộ trình CŨ với `FootProbeHalfWidth` = 0 + `LockedScreenMaxX` = 0 (tạm, trong bộ nhớ) ⇒ ra lại ĐÚNG `PRE4 165b9aef` + `caf21ae2` STAGE 07 tick 16648 (vết từng tick `_cache/04/route/`). ⇒ **ghi lại** bằng autopilot (`Map1RouteProbe.Record(40)`, `Map2RouteProbe.Record(40)`): mọi mốc giữ (cổng, cửa, thang, dơi, tally, bản đồ, STAGE 04/07), input chỉ dời 1–2 tick vài đoạn (+18 tick đoạn đi phải phòng Phantom Bat); STAGE 04 tick 8449 → **8464**, SCORE cuối Map 1 6640 → **6650**, STAGE 07 tick 16648 → **16663**; `PRE4` `165b9aef` → **`59cafbaa`**; flow/flow30/flow144/map2 `caf21ae2` → **`4532ace7`**.

### Bộ chạy hồi quy (kế thừa `CVM2_MEMORY §4 "Bộ chạy hồi quy"`)
Chỉ Editor, đang Play, `runInBackground = true`. Gọi bằng `execute_code` (codedom): `RegressionRunner.Start("<bộ>", 60, "D:/Unity/Castlevania_Original-main/ClaudeNotes/cvm3/_cache/<NN>/<tên phiên>")`; kết quả ở `<dir>/suites/{SUMMARY.txt,progress.txt,ALL.done,<bộ>.txt}`; chờ bằng vòng lặp bash `[ -f <dir>/suites/ALL.done ]`. **3 phiên Play**:
1. Arena, Player = Simon (`CVGP.ArenaUseDummy` = false): `Start("full", 60, …)` ≈ 28 phút.
2. Arena với EditorPrefs `CVGP.ArenaUseDummy` = true (đặt TRƯỚC khi Play, scene Arena mở): `Start("core", 60, …)` ≈ 1.5 phút.
3. Main.unity: `Start("main,flow30,flow144", 60, …)` ≈ 23 phút.
Đọc `SUMMARY.txt` + `ALL.done` — `OK (bỏ qua n)` ≠ đạt. Mỗi phiên: đặt `EditorSettings.enterPlayModeOptionsEnabled = true` + `DisableDomainReload`, AppActivate cửa sổ Unity + `runInBackground = true` sau khi vào Play. Cuối phiên: `ArenaUseDummy` = false, tắt EnterPlayModeOptions, ghi lại `ProjectSettings/EditorSettings.asset` từ HEAD (CRLF).

### Quyết định routine 03 (đồng hồ)
- **(03) Mô hình đồng hồ** = §2d (a)–(e). Code: `SimonController.StartAttack` (đồng hồ: trừ tim + `SpawnSubWeapon()` NGAY, `atkThrow = false` ⇒ đòn là roi), `CanThrow` (đồng hồ không giới hạn số — dùng lại được), `Stopwatch.OnLaunch` (gỡ đồng hồ cũ), `StageWorld.FreezeEnemies` (đang băng ⇒ chỉ kéo dài `to`, không đặt lại `from` — tránh hở 1 tick lúc dùng lại: nhóm Enemy chạy sau Player trong cùng tick), `StageRules.PostTick` (mốc TIME rơi vào `IsEnemyFrozen` ⇒ bỏ), `Crusher.OnTick` (pha lưới `ph` chạy cả lúc băng; đang chờ mốc mà `u mod 16 ≠ ph` ⇒ nhảy tới trong cùng ô hoặc giữ; đang hạ ⇒ chạy tiếp — suy luận, video không có mẫu băng lúc đang hạ). Không băng ⇒ `ph ≡ u mod 16` ⇒ y hệt cũ (mọi kịch bản bẫy khác giữ hash).
- **(03) Lưới bẫy ↔ nhịp TIME trong Unity vẫn là 2 bộ đếm riêng** (bẫy: từ lúc sinh + startTick; TIME: `ResetClockPhase` khi section hiện) — trên luồng thật chúng có trùng pha không là việc của routine căn pha (cvm2-fix-03 / `CVM2 §5 #16`); routine 03 chỉ bảo đảm "nấc kế ở mốc lưới của chính bẫy" (= video khi hai lưới trùng).
- **(03) Nhạc dừng khi dùng đồng hồ: KHÔNG dựng** (1 mẫu dương — §2d).

### Quyết định routine 04 (knockback / nước thánh / mép rơi)
- **(04) Knockback = bảng độ cao theo tick** (như bảng nhảy CVGP 08) thay parabol: `ConfigSimon.KnockbackRise01..10` + `KnockbackApexTicks` + xuống gương, `KnockbackSpeedX` 1 px/tick suốt cung (`SimonHurtState`); hết bảng chưa đáp ⇒ rơi `FallSpeed`, vẫn lùi (suy luận). Bỏ khoá `KnockbackVy0/Gravity/DxTicks`; `KnockbackDx/Dy/AirTicks` giữ làm số THÔNG TIN (34/14/34). Va trần khi bay knockback: **giữ chặn trần** (như CVGP 08 — không có mẫu video; nhảy không chặn trần CVM2 11 ⇒ §6).
- **(04) Hướng lùi** giữ ngược hướng mặt (6/6 mẫu, nhưng mọi đòn tới từ phía trước — không phân biệt được ⇒ §5 #9). Trên thang: không văng (wiki) giữ.
- **(04) Dò sàn Simon X±6** (`Entity.FootSpan` ảo, mặc định = bề rộng body; `SimonController` thu hẹp theo `FootProbeHalfWidth`) dùng cho `TileCollision.IsOnGround` + đáp trong `MoveY` (+ mặt bệ di động). Enemy giữ body. Va tường ngang vẫn thân 16 px.
- **(04) Mép phải màn khi camera khoá** = `TileCollision.PlayerWallMaxX` (chỉ nhóm Player) đặt bởi `BossArena` = cx + `LockedScreenMaxX` + 8 ⇒ Simon X ≤ cx + 239 (video). Enemy/boss giữ `WallMaxX` cx + 256; mép trái giữ cx + 8.
- **(04) Lọ nước thánh**: `HolyWater.Move` đổi thứ tự **vy += g trước rồi dời** (ConfigSubWeapon `speedY0` −1.5, `gravity` 0.12890625); `ConfigActorParam` `spawnHeightAboveFeet` 23 → **19**, thêm `floorProbePx` **0** (điểm dò sàn = tâm lọ; trường prefab `bottomOffset` chỉ còn dùng cho `OnSpent` khi pierces 0); `fireTicks` 80 → **76**; anim `holywater_fire` (manifest `Tools/cvgp/slices_gameplay.json` + `CVGP/Build AnimSets` — chỉ SubWeaponAnimSet ghi lại) fire_0→4→0→3 ×9 tick không lặp. Hộp đánh lửa giữ hợp bbox fire_1..4 (video không đo được hộp). Lửa vẽ: đáy ở mặt sàn − 1 = Unity sẵn (thử đổi viewOffset rồi BỎ — Boss_Map_1 f483–500 xác nhận Unity đúng).
- **(04) Probe**: SimonProbe kỳ vọng `VIDEO_KB_H` (35 giá trị) / `VIDEO_KB_DX` 34 / `VIDEO_KB_AIR` 34 / `VIDEO_FOOT` 6 + kiểm x bắt đầu rơi `ledge32` (438) / `ledge64` (921 = video); `ledge64` bỏ "1 tick bị chặn" (đầu kẹt bục y 80 chỉ vì rơi trễ ở x 919); AttackProbe `holy`: quỹ đạo từng tick = mô hình video, vỡ n32, lửa ở x lọ, `VIDEO_FIRE_TICKS` 76; Map2MechProbe `plat_pit` kiểm theo dò sàn, `pit_knockback` dời x 214; Map2AcceptProbe `knock_pit_hallb` dời x 164 → 176 (knockback 34 px nay bay qua khe 32 px). Harness đo mới `Kb4Probe` (không trong RegressionRunner).

### Quyết định routine 05 (địa hình Block 3)
- **(05) WalkwayB nới trái = DỜI GỐC section +112 px** (không cx âm): section 880 px, `cameraMinX` 0 / `cameraMaxX` 624, x_section = x_sheet + 112 ⇒ **không đổi API** (`CvgpLevelBuilder.ValidateSection` + `check_graph` vẫn đòi biên [0, rộng − 256]; `NesCameraController`/`TileCollision` không cần cx âm). Bề rộng nới = bội 16 nhỏ nhất ≥ |cx min 112| = 112 (video không thấy gì trái hơn x −112 — Simon tự quay lại ở x 16 sheet). **Routine 10/11 đặt vật/điểm WalkwayB phải cộng 112** vào x sheet/video (CSV CVM3 02 `world_x` = gốc sheet).
- **(05) Nội dung phần nới = chép ô có sẵn của CHÍNH WalkwayB** (H2): trời + 17 ô đá rêu đo bằng nền trung vị video (không tự vẽ, không lấy pixel video).
- **(05) StairFloor** chỉ khi miệng thang đi XUỐNG section khác nằm GIỮA 2 ô sàn đặc (như CryptA Block 1): WEB (4,10) (suy luận — không có mẫu đi ngang), WalkwayB (9,8) (video đi ngang). Miệng thang ở rìa sàn (WEB (36,6), WalkwayA (39,4)) giữ Stair như tự động.
- **(05) Gai đen = bóng nền (Empty), không Hazard**: video Simon đi xuyên bóng gai trên cầu WalkwayA; Map_3 không có lần rơi hố; hố = rơi ra đáy (khuôn CVM2 05/06). Giếng sát tường không có marker `pit` (find_pits CVM2 bỏ khe có tường) nhưng rơi vào vẫn chết Pit.
- **(05) Cửa phải Block 3** = khuôn Map 1: trigger cột đỏ 8×48 (x 752–759 sheet), `triggerSimonX` = cột − 12 (đo cửa #2: 740; cửa #1 đáp 742 ≥ 740), đích ghép liền phải `targetOrigin` = (rộng nguồn, 0), entry = x 48 trên sàn đích (= `simonStart` stage mới — kiểm trong `build_collision3.py`). Dữ liệu ghi `dir` 1 tường minh.
- **(05) Thang zig-zag** = 2 thang riêng (tự dò), không thêm kiểu dữ liệu — `TryEnterStair` hiện có chuyển thang ở chiếu nghỉ khi giữ Lên (probe ĐẠT). WEB `stair3` không chân/đỉnh đứng được — giữ dữ liệu như sheet (§6).
- **(05) Khoá boss MummyMan** `bossLockCx` 1280 = `cameraMaxX` ⇒ `NesCameraController.autoBossLock` khoá khi cx ≥ 1280 (nhánh mép PHẢI như Phantom Bat — `bossLockLeft` false vì bossLockCx ≠ cameraMinX); không đổi code camera.
- **(05) Đối chiếu video dùng X±6** (đầu dò sàn Simon CVM3 04) thay ±4 của CVM2 05 — f1859 WEB x 452 đứng mép (27,8) chỉ đúng với X±6.
- **(05) Hash**: không đổi runtime (chỉ dữ liệu Block 3 mới + builder Editor + probe Editor) ⇒ bảng chuẩn phải y nguyên — đo lại 3 phiên (§7).

### Việc gộp + hash DỰ KIẾN đổi (lập ở routine 00 — dự đoán, routine thực hiện xác nhận)
| Routine | Đổi | Hash dự kiến đổi |
|---|---|---|
| 03 đồng hồ | thời lượng/đóng băng Stopwatch + TIME đứng (số dùng chung) | item, rules, b23 (freeze), b24, gallery; flow/flow30/flow144/map2 nếu lộ trình dùng đồng hồ (map2_route có đồng hồ ở Map_2 — nhiều khả năng đổi) |
| 04 knockback/nước thánh/mép rơi | knockback Simon 31–32 px / 36 tick, mép rơi, quỹ đạo lọ | simon, long, rules, attack?, enemy, boss, b23, b46, b24, b56, holy, gallery, core?; flow* nếu lộ trình có trúng đòn (đổi lộ trình cần chứng minh — H14) |
| 07/08 enemy Block 3 | Skeleton, Fleaman, Raven (+ luật rơi vật) | b23, b46 (Fleaman/Skeleton ở b46?), gallery; enemy (Block 1) nếu dùng chung |
| 09 Mummy Men | Mummy + phòng boss thật | b56 (Mummy/Frankenstein), gallery |
| 11 luồng | luồng Map 2 → Map 3, `Stage7Stub` → màn Stage 07 thật, lộ trình `map3_route` | flow/flow30/flow144/map2 (`caf21ae2` → hash tới Stage 10); thêm `map3`, spam8..10; `PRE4` & hash trước STAGE 07 phải giữ |

## §5 Câu hỏi cho người dùng (chép từ `CVM3_PLAN §9` — đừng tự quyết thay)
1. **Nhạc Wicked Child (Block 3)**: bản rip chỉ 1 vòng rồi fade ⇒ lặp ở đầu fade (gain 0.49 — nghe tụt âm mỗi ~45 s; Map 3 ~4 phút ⇒ ~5 lần). Có bản rip 2 vòng không? Không ⇒ bù âm lượng theo đoạn (không sửa file) hay giữ nguyên?
2. Còn chờ từ CVM2: `cvgp-fix-04` (báo #3, Phantom Bat — chỉ Map 1); `CVM2 §5 #16` (pha TIME sau khi vào section ~58–60 cho Map 1/2?), `#17` (chờ tally 224 cho mọi block?); Block 3 dùng số đo riêng Map_3.
3. (vẫn mở) sheet HUD bản US (chữ số TẠM), bấm thử gamepad Flydigi.
4. Cụm sau: Map 4 (Block 4, Stage 10–12, Frankenstein & Igor) cần sheet `NES - Castlevania - Stages - Level 4_ …` (tải tay) + `Map_4.mp4` + `Boss_Map_4_Behaviour.mp4`.
5. Mummy Men: video cho **mỗi con +3000** (wiki không nói rõ) — routine 09 theo video (H1).
6. Người dùng viết "map 2" — planner hiểu là **Map 3**. Sai thì dừng cụm.
7. Kết thúc game (credits/ending/Second Quest) — vẫn chờ.
8. (02 CVM3, mới) **Sát thương Block 3 = 3 vạch/đòn** (đo Boss_Map_3; Block 1–2 = 2): dùng chung cho Skeleton/Raven/… hay chỉ băng gạc Mummy? Map_3 không có đòn nào để đo ⇒ routine 04/07/08 theo wiki (sát thương theo stage), ghi §4 — nếu người dùng có clip Skeleton/Fleaman trúng Simon thì đối chiếu.

9. (04 CVM3, mới) **Hướng knockback khi bị đánh từ PHÍA SAU**: cả 6 lần trúng ở 3 video đều do đòn tới từ phía trước ⇒ không phân biệt "lùi ngược hướng mặt" (Unity giữ, CVGP 08) với "lùi ra xa nguồn đòn". Người dùng nhớ bản gốc (vd Medusa Head đánh sau lưng ở cầu Block 3 hất Simon về phía trước xuống hố?) hoặc có clip thì báo.

## §6 Nợ & việc ngoài phạm vi (chép mục còn mở liên quan Block 3 từ `CVM2_MEMORY §6`)
- (02 CVM3) **Simon từng frame Map_3 phủ 81.4 % khớp / 87.0 % sau nội suy (< 90 %)**; hố lớn f341–496 (nền lệch LUT), cửa #1, sau đổi tầng WalkwayB f3275–3320; boss 47 %. Đủ cho bảng sự kiện/trúng đòn (khớp 100 % hit #1/#2; #3 bị Mummy che) — nếu routine sau cần quỹ đạo Simon dày hơn: chạy lại `simon3.py` với LUT riêng đoạn WEA hoặc đo tay bằng `strip3.py`.
- (02 CVM3) **Hit #3 boss** Simon bị Mummy che f806–834 (không đo knockback); +100 trong trận boss (5 lần) chưa phân biệt được "đòn trúng Mummy" hay "phá băng gạc" — routine 09 xác nhận bằng ảnh phóng to (khung f503/507, 636/640, 725/729, 833/837, 899/903).
- (02 CVM3) Nhạc Poison Mind chuyển ≈ 1.2 s sau khóa: ±0.3 s (cửa sổ 1 s) — routine 09/11 nếu cần chính xác hơn dùng `music3.py pos`.
- (02 CVM3) Loại enemy ở bảng hạ (§4 timeline) suy theo giá trị điểm wiki (ảnh xác nhận ~12/29) — 07/08 xác minh từng loại khi dựng.
- (CVM2 ACCEPT12) ~~Đồng hồ 300 vs video 186–196 tick → routine 03~~ **(03) xong** — 186 tick theo video (§2d); knockback 12 vs 31–32 px → **(04) xong** (34 px / 34 tick bảng video — §2e); enemy rơi vật → routine 08.
- (CVM2 03) **Đồng hồ dừng nhạc 2.40 s** (Map_2 2:40.97, 1 lần đo) — chưa dựng. **(03)** Map_3/Boss_Map_3 không dùng đồng hồ lần nào ⇒ không thêm mẫu; mô hình "chỉ dừng nhạc stage" khớp 4/4 nhưng 1 mẫu dương ⇒ vẫn chưa dựng (`CVM2 §5 #7` mở).
- (03) Đồng hồ băng lúc bẫy ép ĐANG HẠ (nhịp 4 tick riêng) — video không có mẫu; Unity chạy tiếp nhịp hạ rồi mới khớp lưới (suy luận).
- (03) Unity: lưới bẫy và nhịp TIME là 2 bộ đếm riêng; video cho thấy trùng nhau ⇒ routine căn pha section (cvm2-fix-03 / §5 #16 CVM2) nên đặt `startTick` bẫy theo pha TIME để "bẫy + TIME chạy lại cùng tick" đúng trên luồng thật Hall C.
- (03) Đòn roi khi dùng đồng hồ dùng chung nhánh roi của `SimonAttackState` (đứng/ngồi/thang/trên không như roi thường) — chỉ kiểm tư thế đứng trong probe.
- (CVM2 03) Wicked Child lặp tụt âm (§5 #1).
- (CVM2 03) Prefab màn hình còn trường YAML cũ `music`/`stageMusic`/… `{fileID: 0}` — tự mất khi prefab lưu lại.
- (CVM2 05) `StageFlow` cửa trái / `autoBossLock` theo phía / `simonStartDir` đã xử lý ở CVM2 11 — Block 3 có **cửa PHẢI #1/#2** và **thang lên-trái**, khoá phòng Mummy cx 1280 (routine 05/09/11).
- (CVM2 06) Cơ chế `Map2Mechanisms.SpawnFromMarkers` chỉ gắn probe/luồng Block 2 — cầu hỏng Block 3 làm mới (05).
- (CVM2 11) Builder `Build Map2 Screens` không idempotent byte → routine 11.
- (00 CVM3) Prompt routine 00 ghi baseline Arena `full` ≈ 26 phút; đo thực 28 phút (máy bận) — không ảnh hưởng.
- (00 CVM3) `RegressionRunner.EXPECTED` chưa có `map3`/spam8..10 — thêm ở routine 11.
- (04 CVM3) **Phòng Medusa (Map 2) mép trái**: video Boss_Map_2 Simon dừng ở x 23 (cột cửa `D` x 0–15 chặn như tường khi cửa đóng); Unity cửa `D` không đặc ⇒ Simon/knockback tới cx + 8 = 8 (lệch 15 px) — việc của lưới va chạm/luồng cửa trái (cvm3-fix hoặc routine màn sau), không sửa ở 04.
- (04 CVM3) Knockback **dưới trần** (Unity chặn trần như CVGP 08) và **bị hất khỏi mép xuống hố / tầng dưới** (hết bảng ⇒ rơi 8 px/tick vẫn lùi) — chưa có mẫu video; Block 3 (cầu hỏng) là nơi đầu tiên gặp — routine 12 so khi có mẫu.
- (04 CVM3) Lọ nước thánh **ném khi ngồi** (sinh Y + 8 − 19 theo tư thế) và **ném trái** (X0 − Simon đo −1.6..+0.5 ở Boss_Map_1, trong sai số pivot) chưa đo trực tiếp; nhịp lửa đánh lại 8 tick (quy ước) chưa đo được.
- (04 CVM3) Mép TRÁI màn khi khoá giữ cx + 8 (video chỉ thấy Simon tới x màn 11 ở phòng Mummy) — suy luận CVGP 14.
- (05 CVM3) **Luồng chưa dùng dữ liệu Block 3** (routine 11): `StageFlow` phải đổi tầng thang **↖ cắt đỉnh/đáy** (Map 2 chỉ ↗ — hằng `STAIR_UP_ARRIVE_Y`, đen 22/24 tick Map_3), **cửa PHẢI** Block 3 (cuộn 128 ×2 ~1 px/tick, dừng ~158–162 tick, Simon tự đi tới entry.x 48 — 74/76 px), STAGE 07/08/09 + điểm vào `simonStart`/`simonStartDir`; `StageScreen` nhận stage 7–9. Khoá cx 1280 MummyMan đã đúng nhánh camera (kiểm code), BossArena/tường phòng Mummy = routine 09.
- (05 CVM3) **WalkwayB gốc dời +112** — mọi toạ độ đặt vật/enemy/bí mật St08 tầng trên (routine 10) và vết Simon (`map3_simon.csv` world_x) phải cộng 112; camera Unity kẹp cx 0 (= video −112): video không có mẫu Simon trái hơn x 16 sheet.
- (05 CVM3) WEB `stair3` ↗ (448,88)→(472,64) không có chỗ đứng ở chân (y 96, cột 27) lẫn đỉnh (y 64, cột 30) — 2 đầu là gạch xanh nền (video Simon nhảy XUYÊN gạch đó f2104–2114) ⇒ thang trang trí/không dùng được trong Unity; NES gốc có cách lên không? (không có mẫu video — giữ như sheet).
- (05 CVM3) WEB (4,10) StairFloor là **suy luận** theo cấu hình CryptA Block 1 (video không đi ngang ô này) — routine 12 kiểm nếu có mẫu.
- (05 CVM3) Wiki St8 bí mật "quỳ trên 2 khối không với tới bên phải cầu thang" — 2 khối = WalkwayB (15-16,10) sau nới (dưới mặt cầu); video lấy +1000 ở mút trái cầu KHÔNG quỳ (CVM3 02) ⇒ routine 10 quyết định trigger.
- (05 CVM3) Phòng Mummy không có cột cửa cuối (sheet) ⇒ không `door_end`; mép phải phòng = mép section (TileCollision ngoài lưới = Solid) + `PlayerWallMaxX` khi khoá (CVM3 04).
- (04 CVM3) Script đo cũ (`Tools/cvm2/*`, `Tools/cvm3/*3.py` dùng `-ss` không `-copyts`) có thể lệch chỉ số frame 1 ở vài chỗ — csv CVM2/CVM3 02 (giải mã tuần tự) không bị; dùng `kb4_common.read_frames` khi cần chính xác frame.

## §7 Nhật ký nghiệm thu

### Routine 00 — 21/10/2026 — Hạ tầng: sổ, commit đồ planner, kiểm mẫu, baseline
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `__pycache__`, `UserSettings/`, submodule ` m`); `.gitignore` đã có dòng `ClaudeNotes/cvm3/_cache/`; cổng CVM2 12 = XONG (commit `d1bf216`). MCP chọn `Castlevania_Original-main@5669a9bbfcade130` (3 instance mở: 2 `hero_def_2021` khác), `dataPath` = `D:/Unity/Castlevania_Original-main/Assets`.
- **Kiểm mẫu số planner**: `calib_map3.py` ⇒ §2a (lệch sx 0.001 / ox 0.14 px NES, ZNCC 0.9786). 8 mốc `MAP3_TIMELINE §3` trích lại bằng ffmpeg và ĐÃ MỞ XEM (`ClaudeNotes/cvm3/_cache/shots/00/sheet.png`, `m1b.png` cho 7.6 s) — §2b; chỉ khác: 0:06.8 còn đen (fade-in), STAGE 07 TIME 0500 hiện ở 7.6 s.
- **Baseline đo (không đổi code)**: Arena-Simon `full` @60: 14/14 KHỚP (1672 s). Arena-Dummy `core`: `76516076` 21/21 (92 s). Main `main,flow30,flow144`: 14 bộ KHỚP (1383 s) — flow/flow30/flow144/map2 `caf21ae2`, trước STAGE 04 `165b9aef`, H8 5/5 ×3 (352/432/448/890/922), spam1–7 0 lỗi. Cả ba `ALL.done` = `OK` (bỏ qua 0). Kết quả: `_cache/00/{arena,core,main}/suites/SUMMARY.txt`.
- **Dọn**: thoát Play, `ArenaUseDummy` = false, EnterPlayModeOptions tắt, `EditorSettings.asset` = HEAD (CRLF; `git status` không hiện), scene mở = Main không dirty.
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 00**: [x] đồ planner commit (`_cache/` không vào git) · [x] sổ đủ §1–§7, bảng hash 21 dòng có số đo hôm nay · [x] mọi hash = bảng CVM2 12; H8 5/5 ×3; console không lỗi mới (spam 0 lỗi).

### Routine 01 — 21/10/2026 — Nhập sheet Level 3, kiểm kê, đối chiếu Tilesets, kiểm sprite Block 3
- **Bước 0**: `main`; nhiễu nền giữ nguyên; cổng 00 = XONG. Không đụng Unity (không vào Play, không ghi `.cs`/asset Unity; chỉ python + file) ⇒ MCP không cần; mọi hash giữ nguyên Bảng hash chuẩn (không chạy lại bộ).
- Sheet chép nguyên byte, sha256 khớp, `SOURCES.md` +1 dòng. Kiểm kê chạy lại ⇒ JSON + ảnh chú thích trùng byte planner (`cmp`).
- Đối chiếu Tilesets: `cmp_tilesets3.py` (mới) — §3b; ảnh `ref/level3/tilesets_xcheck.png` đã xem.
- Sprite: 32 ô có đủ, verify 0 lỗi; Mummy đỏ = nháy bảng màu dùng `mummy_red_*` (§3b).
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 01**: [x] sheet nguyên byte + SOURCES · [x] kiểm kê y hệt + bảng Tilesets + ảnh · [x] bảng sprite §3b, verify 0 lỗi · [x] không đổi runtime ⇒ hash chuẩn y nguyên (không chạy lại bộ; chưa đo lại — chỉ suy ra do không đụng Assets).

### Routine 03 — 21/10/2026 — Đồng hồ (Stopwatch) theo video (= cvm2-fix-01)
- **Bước 0**: `main`; nhiễu nền giữ nguyên; cổng 00 = XONG. MCP `Castlevania_Original-main@5669a9bbfcade130` (1 instance), `dataPath` đúng.
- **Đo** (`Tools/cvm3/stopwatch3.py` + `stopwatch3_sheet.py` — lưới khung video quanh từng lần dùng): 4 lần dùng Map_2, 0 lần Map_3/Boss_Map_3 ⇒ §2d (bảng + mô hình + giả thuyết loại bỏ). Phát hiện ngoài dự kiến: đồng hồ có hiệu lực ngay tick bấm và Simon vung roi (Unity cũ: tư thế ném, sinh ở khung duỗi ~13 tick sau); dùng lại khi còn hiệu lực được.
- **Sửa**: Config `ConfigSubWeapon.Stopwatch.freezeTicks` 300 → **186** (`đo:cvm3-03 video:Map_2 …`) + `CVGP/Import Config`; code §4 "Quyết định routine 03"; probe kỳ vọng `WIKI_STOPWATCH 300` → `VIDEO_STOPWATCH 186` (Attack/Enemy/EnemyB23/B46/Boss/B24/B56); kịch bản mới rules `stopwatch_time`, `stopwatch_chain`, mech `crusher_resume_video`; `SaveReplays` (2 replay đổi kịch bản, 6 chỉ đổi mô tả); `RegressionRunner.EXPECTED` 9 hash mới. `CHARACTER_STATS.md` xuất lại (Stopwatch 186; kèm dòng Block 3 TIME 500 từ CSV CVM2 11 chưa xuất trước đó).
- **Lần chạy đầu lộ lỗi thật** (đã sửa): dùng lại đồng hồ làm hở 1 tick băng (bia chạy 1 tick — `FreezeEnemies` đặt lại `from`); `crusher_stopwatch` hỏng vì băng sớm 12 tick (bẫy 3 ở khung 3) ⇒ dời bấm rel 2 → 14; b46 `freeze_whip_axe` / b56 `death_stopwatch` hỏng vì 186 < chuỗi roi/cửa sổ quan sát ⇒ dùng lại đồng hồ giữa chừng (đúng mô hình mới).
- **Hồi quy 3 phiên** (`_cache/03/{arena,arena3,core,main,confirm,confirm144}`): Arena `full` 14 bộ — 9 đổi hash có chủ đích (chỉ kịch bản đồng hồ, so từng kịch bản), long/simon/item/holy/gallery KHỚP; b46/b56 chạy lại sau sửa kịch bản 26/26, 14/14; core `76516076` KHỚP; Main 14 bộ KHỚP (`caf21ae2` ×4, PRE4, H8 5/5 ×3, spam1–7 0 lỗi); xác nhận `EXPECTED` mới: long/attack/mech/rules KHỚP @60, mech/attack KHỚP @144 (tất định).
- **Lưu ý quy trình**: lỡ ghi `.cs` 1 lần khi đang Play (sửa RulesProbe/Map2MechProbe/StageWorld giữa 2 lần chạy nhanh) — đã thoát Play ngay, biên dịch lại, mọi kết quả ghi ở đây đều chạy SAU lần biên dịch đó.
- **Dọn**: thoát Play, `ArenaUseDummy` = false, EnterPlayModeOptions về như HEAD, `EditorSettings.asset` = HEAD (CRLF), scene Main không dirty.
- **Câu hỏi §5 mới**: không (nhạc-dừng vẫn là `CVM2 §5 #7`).
- **Nghiệm thu routine 03**: [x] bảng 4 lần dùng + mô hình + sai số §2d; Config nguồn video · [x] probe ĐẠT kỳ vọng mới (rules 78/78, attack 21/21, mech 15/15, b23 35/35, b46 26/26, b24 21/21, b56 14/14, enemy 20/20, boss 12/12); bẫy so video: Unity +192 = video +192.3, Medusa chạy lại 186 tick sau bấm (video 186.3) · [x] mỗi hash đổi 1 dòng lý do (§4), hash không liên quan y nguyên, H8 5/5 ×3, CHARACTER_STATS cập nhật. **Xấp xỉ**: D = 186 ±3 (4 mẫu, 1 đo trực tiếp); băng lúc bẫy đang hạ = suy luận; nhạc dừng chưa dựng.

### Routine 04 — 21/10/2026 — Simon knockback + lọ nước thánh + mép rơi theo video (= cvgp-fix-03)
- **Bước 0**: `main`; nhiễu nền giữ nguyên (thêm `Tools/cvm3/__pycache__/` — không commit); cổng 00/02 = XONG. MCP `Castlevania_Original-main@5669a9bbfcade130` (1 instance), `dataPath` đúng.
- **Đo** (§2e, `Tools/cvm3/kb4_*.py`): knockback 6 lần / 3 video (Map_1/2/3 0 lần) ⇒ bảng theo tick; bất tử sau đáp 90–102 tick (giữ 94); mép phải màn khoá x 239; lọ nước thánh 10 lọ Map_3 (dư 0 px) + 4 lọ Boss_Map_1 (rms ≤ 1.44); lửa 4 chuỗi (0→4→0→3 ×9, sống 76); đi khỏi mép 9 lần (rơi ở 6 px).
- **Phát hiện ngoài dự kiến**: (1) "lùi 12 px" của CVGP 08 = Simon bị **mép màn khoá** chặn ở x 239, không phải quãng knockback; (2) "36 tick, giữ đỉnh ~16" (CVM2 09) là frame-clock — đồng hồ x cho 34 tick, giữ 14; (3) đọc frame bằng `-ss` không `-copyts` lệch 1 frame tuỳ keyframe (đã sửa trong `kb4_common`); (4) dò sàn X±6 làm SimonProbe `jumpwall` đáp đúng tick 30 của video (trước 29); (5) knockback 34 px **bay qua khe 32 px** ⇒ 2 kịch bản hất-xuống-hố phải dời chỗ đứng; (6) thử hạ lửa 1 px (viewOffset) rồi BỎ — Boss_Map_1 xác nhận Unity vẽ đúng.
- **Sửa**: `ConfigSimon` (+`KnockbackSpeedX/ApexTicks/Rise01..10`, `FootProbeHalfWidth`, `LockedScreenMaxX`; bỏ `KnockbackVy0/Gravity/DxTicks`; Dx/Dy/AirTicks = 34/14/34 thông tin), `ConfigSubWeapon.HolyWater` (vy0 −1.5, g 33/256), `ConfigActorParam` (spawnHeightAboveFeet 19, floorProbePx 0, fireTicks 76) + `CVGP/Import Config`; anim lửa (manifest + `CVGP/Build AnimSets` — chỉ SubWeaponAnimSet ghi lại); code `SimonHurtState`, `SimonController` (bảng kb, `FootSpan`), `Entity.FootSpan`, `TileCollision` (dò sàn + `PlayerWallMaxX`), `BossArena`, `HolyWater` (thứ tự vy += g, dò tâm), `HolyFire`; probe SimonProbe/AttackProbe/Map2MechProbe/Map2AcceptProbe (kỳ vọng video, §4), harness đo `Kb4Probe`; `RegressionRunner.EXPECTED` 14 hash + flow* + `PRE4`; `map1_route`/`map2_route` ghi lại (autopilot); `CHARACTER_STATS.md` xuất lại; ghi chú "(CVM3 04)" ở 4 dòng `CVGP_MEMORY` (knockback, ngưỡng mép, nước thánh, quyết định knockback) vốn diễn giải sai.
- **Hồi quy** (`_cache/04/{arena,core,main,main2,confirm144}`): Arena `full` 14/14 bộ ĐẠT mọi kiểm, 14 hash đổi — so từng kịch bản (`kb4_hashdiff.py`) + chứng minh bằng tắt tạm luật trong bộ nhớ (long, gallery QueenMedusa/Dracula/Mummy/Frankenstein ra lại đúng hash cũ) ⇒ §4 "Lịch sử đổi hash"; core `76516076` giữ; Main lần 1: luồng cũ lệch (tick 3196 Crypt A — chứng minh, §4) ⇒ ghi lại lộ trình ⇒ Main lần 2: 14/14 KHỚP (flow ×3 fps `4532ace7`, PRE4 `59cafbaa`, H8 5/5 ×3, spam1–7 0 lỗi); Map2AcceptProbe `damage_halla` (lùi 33–34 rồi kẹt tường x 760) + `knock_pit_hallb` (rơi khe ⇒ hồi sinh đầu STAGE 05) ĐẠT; xác nhận @144: long/simon/attack/item/holy/mech/rules/boss/enemy = EXPECTED mới (tất định).
- **CVM2 ACCEPT12 mặt 3** ("Simon lùi 12 px vs video 31–32"): nay **34 px / 34 tick theo bảng video** (Unity ↔ Boss_Map_2/3 dư 0 px theo đồng hồ x) ⇒ phần knockback **ĐẠT**.
- **Lưu ý quy trình**: lỡ ghi `Kb4Probe.cs` 1 lần khi đang Play (dời bia) — thoát Play ngay, biên dịch lại, mọi kết quả ghi ở đây chạy SAU lần biên dịch đó. Config chỉ sửa TẠM trong bộ nhớ khi Play (để chứng minh nguyên nhân hash) rồi trả; `CVGP/Import Config` cuối phiên "ghi lại 0".
- **Dọn**: thoát Play, `ArenaUseDummy`/`CheatInvincible` = false, fps/vSync mặc định (-1 / 1), EnterPlayModeOptions tắt, `EditorSettings.asset` = HEAD (CRLF), scene Main không dirty.
- **Câu hỏi §5 mới**: #9 (hướng knockback khi bị đánh từ phía sau).
- **Nghiệm thu routine 04**: [x] bảng knockback 6 lần / 3 video + mô hình; nước thánh 14 lọ (10 Map_3 + 4 Boss_Map_1) + lửa 4 chuỗi; mép rơi 9 lần — có frame nguồn (`ref/data/kb4_*`) · [x] Unity ↔ video: knockback dư 0 px theo đồng hồ x (5 dựng lại; frame-clock rms ≤ 1.1, riêng ngồi-sát-mép y 1.76 do frame lặp), lọ 46 điểm 0 px + vỡ n49 đúng, mép 7/7 lệch 0; 4 cặp ảnh đã xem (`ref/nes/cvm3_04_pairs_*`) · [x] mọi hash đổi có lý do + chứng minh; lộ trình ghi lại có tick khác đầu + nguyên nhân; Map 1 + 2 chơi trọn tới STAGE 07 ×3 fps; H8; CHARACTER_STATS. **Xấp xỉ/còn lại**: tick lẻ pha lên (2 mẫu) và đoạn hết bảng bị hất khỏi mép là suy luận; hướng lùi chưa phân biệt được; mép trái phòng Medusa (cửa) lệch 15 px — §6.

### Routine 05 — 21/10/2026 — Tileset + 5 Tilemap Block 3 (WalkwayB nới theo video), lưới va chạm, đồ thị section (Block 1 + 2 md5 y hệt)
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `Tools/cvgp|cvm2|cvm3/__pycache__/`, `UserSettings/`, submodule ` m`); cổng 01 = XONG (00–04 đều XONG). MCP `Castlevania_Original-main@5669a9bbfcade130` (1 instance), `dataPath` đúng.
- **Đầu vào**: 5 vùng sheet phóng to + lưới (`_cache/05/view/*.png`, đã xem 12 tấm) + vết chân Simon video vẽ lên từng section (`view/trail_*.png`, đã xem WEA/WEB/WA/WB/MM) + bảng đoạn đứng theo sàn (frame/x) ⇒ checks; `Tools/cvm3/terrain3.py broken` (Map_3 + Boss_Map_3) ⇒ 2 khối vỡ (ảnh `ref/nes/cvm3_05_khoi_vo_wea_mummy.png`, đã xem); `Tools/cvm3/wb_pad3.py` ⇒ nội dung trái WalkwayB (§2f).
- **Tileset** (`build_tileset3.py`, phần chung CVGP): **125 tile** = kiểm kê, WalkwayB nới trái 112 px chỉ bằng ô có sẵn (17 ô đá + trời), dựng lại 0 px so vùng đã nới, phần sheet 0 px so sheet gốc; 2 lần chạy "same".
- **Va chạm** (`build_collision3.py`): 83 metatile (L1 Door 3 · L2 Stair 5 · L3 Solid 1 · Empty 74); 4 override (2 Breakable video, 2 StairFloor) + 43 check; 9 thang (2 zig-zag), 7 nối (2 thang ↖ ×2 chiều, 2 cửa PHẢI, 1 cửa trang trí), 9 hố tự tính; kiểm Level 3 (cửa phải, entry = simonStart, khoá boss = cameraMaxX) đạt; **đối chiếu 3 220 frame Simon đứng ở video: 0 lệch**; 2 lần chạy "same"; 5 ảnh overlay `ref/level3/` (đã xem).
- **Unity**: `CvgpLevelBuilder` + Spec `MAP3` + `BuildMap3()`; menu `CVM3/Build Map3 Tilemaps` (`Cvm3LevelMenu.cs`); `LevelSectionData` không đổi. Lần 1: "tileset cắt lại; tile 125 (ghi 125); section 5 (data ghi 5, prefab ghi 5)"; lần 2 (menu thật): "ghi 0". **H12**: `CVGP/Build Map1 Tilemaps` + `Build Arena Section` + `Build Arena Boss Rooms` + `CVM2/Build Map2 Tilemaps` ⇒ "ghi 0" cả 4; python Block 1 (7 + 1 file) + Block 2 (8 + 1) "same"; **574 file** `Assets/Data/{Art,Config,Prefabs}/Level/**` (trừ Block3) + overlay `cvgp/ref/level1` + `cvm2/ref/level2` **md5 y hệt** trước/sau (`_cache/05/work/md5_{before,after}.txt`). Compile sạch (console chỉ lỗi nền Odin path1 / Unity.Mathematics / GUIClips / MCP WebSocket + warning framework).
- **Ảnh Play pixel** (`Cvm3TerrainProbe.StartShots`, Arena Play, Game view 1920×1080, sprite + HUD + HudBackdrop ẩn): **25 ảnh** (WEA cx 0/169/277/405/512; WEB 0/52/296/421/512; WalkwayA 0/256/512; WalkwayB 0/49/117/297/551/624 = video −112/−63/5/185/439/512; MummyMan 0/228/636/993/1263/1280) so vùng sheet đã nới (`Tools/cvm3/accept3.py terrain`): **0 / 49 152 px lệch mỗi ảnh**, NES y 0–31 / 224–239 đen. Đã xem `terrain_montage.png` (bản lưu `ref/nes/cvm3_05_unity_terrain_montage.png`).
- **So video cùng cx** (`accept3.py pairs`, **24 cặp**, 6 tấm ghép `_cache/shots/05/pairs/pairs_0..5.png` — **đã xem cả 6**): TB |hiệu| 3.1–6.3, pixel lệch > 48: **1.84–3.93 %** — phần đỏ chỉ là Simon, quái (Skeleton, Raven, Fleaman, Bone Tower, Mummy), nến, vật phẩm, roi, chữ điểm, khối vỡ WEA (18,8) (f892); bố cục nền trùng, kể cả **WalkwayB cx −112 ⇔ Unity 0** (f3484, `ref/nes/cvm3_05_pairs_walkwayb.png`).
- **Simon trong Block 3** (`Cvm3TerrainProbe.RunMoves("all")`, Arena-Simon, 5 section dựng lúc chạy): **32/32 ĐẠT** (`_cache/05/moves_final.txt`; mọi kịch bản di chuyển X cuối ≠ X đầu): đi sàn mọi section + điểm vào St07/08/09; nhảy bậc/bờ WEA (y 176→160→128→96); leo WEA ↖ 12 bậc cắt đỉnh (Y ≤ 31); **zig-zag** WEB (đến từ thang cắt đáy t 4 → ↖ → chiếu nghỉ y 160 → ↗ tới (127,96), 244 tick) và WalkwayA (↗ → chiếu nghỉ y 64 → ↖ cắt đỉnh); WEB stair2 lên / stair4 xuống; đi ngang miệng thang StairFloor WEB (4,10) + WalkwayB (9,8); WalkwayB đến từ thang → mút trái cầu **X 128 ⇒ camera cx 0 (= video −112)**; nhảy khe cầu WalkwayB + 3 khe **cầu hỏng St9**; rơi hố WEB / khe WalkwayB / trời trái WalkwayB / khe St9 ⇒ FallDeath Pit; bậc khối vào phòng Mummy 96→112→128→144→176; roi vỡ WEA (18,8) + MummyMan (83,9); cột cửa phải không đặc (tới x 760 / 872). Lần chạy đầu 31/32: `web_arrive_zigzag` hết tick ở Y 106 (đã sang ↗ — chỉ thiếu thời lượng: 0.5 px/tick) ⇒ nới U*150 → U*240, không sửa lưới. Ảnh Play có Simon: `ref/nes/cvm3_05_play_wb_mut_trai_cx0.png` (đã xem: Simon x 128 trên mút trái cầu, cx 0 — trùng khung video f3484).
- **Hồi quy H11 (3 phiên Play, DisableDomainReload, không crash)**: Arena-Simon `full` @60 (1672 s): **14/14 bộ KHỚP** bảng chuẩn (long `b2cc1424` … gallery `ab874ae4`), 0 lệch/lỗi/bỏ qua; Arena-Dummy `core` `76516076` 21/21 KHỚP (92 s); Main `main,flow30,flow144` @60 (1384 s): **14/14 KHỚP** — flow/flow30/flow144/map2 `4532ace7`, trước STAGE 04 `59cafbaa`, **H8 5/5 ×3** (352/432/448/890/922), spam1–7 0 lỗi console. Cả ba `ALL.done` = `OK` (bỏ qua 0). Kết quả `_cache/05/{arena,core,main}/suites/SUMMARY.txt`. **Không hash nào đổi.**
- **Cuối phiên**: `Cvm3TerrainProbe.Cleanup()` (gỡ 5 section, camera bám Simon) trước khi thoát Play; thoát Play; `CVGP.ArenaUseDummy` = false, `CvgpDebug.CheatInvincible` = false, `CvtiDebug.LogFlow` = false, fps −1 / vSync 1; EnterPlayModeOptions trả như cũ + `EditorSettings.asset` = HEAD (CRLF, `git status` sạch); Main.unity mở không dirty, Arena.unity không đổi. Sau phiên: ghi lại `Level3Tiles.json` (chỉ đổi chuỗi nguồn PAD → `Tools/cvm3/wb_pad3.py`), builder Map3/Map2/Map1 "ghi 0", 574 file Block 1/2 md5 vẫn y hệt.
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 05**: [x] 5 section 0 px so sheet (25 ảnh), 24 cặp video đã xem; WalkwayB nới đúng bề rộng video (112 = |cx min|, 17 ô nguồn ghi rõ, kiểm chứng 176/176); tileset/Tile/CSV/JSON tái tạo "same"/"ghi 0" · [x] lưới va chạm 4 override (expectAuto + reason + video m:ss / suy luận ghi rõ) + 43 check; 5 overlay đã xem; vết Simon video 3 220 frame 0 lệch; đồ thị khớp thứ tự video (§2f) · [x] Block 1 + Block 2 md5 y hệt (574 file); mọi hash bảng chuẩn y nguyên; H8 5/5 ×3. **Xấp xỉ/còn lại**: WEB (4,10) StairFloor suy luận; WEB stair3 không dùng được; camera WalkwayB trái hơn −112 không có mẫu; luồng (thang ↖, cửa phải, STAGE 07–09) chưa nối — routine 11 (§6).

### Routine 02 — 21/10/2026 — Hiệu chuẩn tinh 2 video, HUD/camera/Simon từng frame, chuyển cảnh, sự kiện, trúng đòn, âm thanh
- **Bước 0**: `main`; nhiễu nền giữ nguyên; cổng 00/01 = XONG. Không đụng Unity (python + ffmpeg + file; không ghi `.cs`/asset; MCP không cần) ⇒ **không đổi runtime, mọi hash giữ Bảng hash chuẩn (không chạy lại bộ)**.
- **Công cụ mới** `Tools/cvm3/*3.py` (copy từ `Tools/cvm2` + sửa: vùng ghép cửa phải `WEB>WA`/`WB>MM`, đệm trời cx âm WalkwayB, LUT theo video, icon vũ khí nước thánh/đồng hồ/dao găm) + `strip3.py`, `trans3.py`, `events3.py`, `music3.py`.
- **Kết quả**: §2a + §2c (số liệu chính); `ref/MAP3_TIMELINE.md` viết lại §1–§8 (189 dòng); `ref/data/` (HUD/camera/Simon CSV 7548 + 1282 dòng, 161 sự kiện, hits, transitions, audio, calib, dup); 15 ảnh mới `ref/nes/cvm3_02_*.png` (đã xem) + lưới 40 ảnh planner (đã xem, đổi tên 2).
- **Đính chính planner**: Map_3 0 lần trúng đòn; vật = II; khung/s 22.7 & 19.6; ENEMY 16 vạch chung; túi St08 ở mút trái cầu; không có nhạc Wicked Child "sớm" ở STAGE 07 (0 tick).
- **Câu hỏi §5 mới**: #8 (sát thương Block 3 = 3 vạch dùng chung?).
- **Nghiệm thu routine 02**: [x] hiệu chuẩn Map_3 15 frame ZNCC 0.973 (≥ 0.93) + boss 8 frame grad 0.943; hàng nhìn thấy + watermark ghi rõ · [x] HUD từng frame + 161 sự kiện, **20 frame kiểm mắt 0 sai**, bảng trúng đòn đủ 2 video · [~] Simon/camera từng frame: camera 100 % game, cx âm đo được (−112); **Simon 81.4 % khớp / 87.0 % nội suy — chưa đạt 90 %** (độ phủ ghi rõ) · [x] chuyển cảnh có tick + sai số · [x] `MAP3_TIMELINE` viết lại + `CVM3_MEMORY §2`, ảnh mốc đã xem.
