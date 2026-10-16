# CVM2_MEMORY — sổ bàn giao cụm CVM2 (Map 2 = Block 2 = Stage 04–06 chơi được như video)

*Mọi routine CVM2: đọc file này + `CVM2_PLAN.md` ĐẦU phiên, cập nhật CUỐI phiên (§1 + mục liên quan + §7), commit cùng routine.*
*Hiến pháp H1–H13 ở `CVM2_PLAN §8`; kiến trúc mở rộng §5; danh sách routine §7. Hạ tầng + API lõi CVGP: `ClaudeNotes/cvgp/CVGP_MEMORY.md` §3/§4/§4b/§7.*

**Yêu cầu gốc (15/10/2026, nguyên văn):**
> Tiếp tục dựng map 2 và play game theo như theo như video trên. Xem qua Walkthrough Stages 4 - 6 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_4_-_6
> Nếu như thiếu file sprite thì vào link ( https://www.spriters-resource.com/nes/cv/ ) này xem để download
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

Video kèm: `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_2.mp4` (4:20, tiếp đúng playthrough Map_1) và `...\Boss_Map_2_Behaviour.mp4` (23 s, Queen Medusa, playthrough + bảng màu khác). Sheet mới người dùng tải 15/10: `C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 2_ Main Hall.png`, `...\NES - Castlevania - Miscellaneous - Tilesets.png`.

**Người dùng đã chốt từ CVGP (giữ nguyên):** bàn phím Z = Roi · X = Nhảy · C = Vũ khí phụ · V = Select · B = Start; gamepad A = Nhảy · X = Roi · B/Y = Vũ khí phụ · Menu = Start · View = Select.

**Quyết định planner (15/10/2026 — `CVM2_PLAN §0`, routine không hỏi lại):**
- Cụm CVM2 gộp 3 việc nợ CVGP chặn Map 2: nhạc (`cvgp-fix-02` → `cvm2-03`), lọ nước thánh xuyên địch (`cvgp-fix-01` → `cvm2-04`), dọn harness + bộ chạy hồi quy 1 lệnh (`cvgp-fix-05` → **`cvm2-00`, XONG**). `cvgp-fix-03` (Simon knockback/nước thánh/mép rơi) và `cvgp-fix-04` (báo #3, Phantom Bat) chỉ liên quan Map 1 ⇒ KHÔNG gộp, chờ người dùng duyệt (§5 #1).
- Sprite: không cần tải thêm (Block 2 đã có sprite từ CVGP + 2 sheet mới). spriters-resource bị Cloudflare chặn ⇒ thiếu thật thì ghi §5 cho người dùng tải tay.
- Bàn phím/gamepad, kiến trúc lõi, hiến pháp: giữ nguyên CVGP.

## §1 Trạng thái cụm

| # | taskId | Trạng thái | Commit | Ngày | Ghi chú |
|---|---|---|---|---|---|
| 00 | cvm2-00-ha-tang-so-ghi-nho-bo-chay-hoi-quy | **XONG** | (commit "CVM2 00") | 16/10/2026 | sổ này + commit đồ planner (48 file) + `RegressionRunner` (bộ chạy hồi quy, gộp `cvgp-fix-05`) + **Bảng hash chuẩn** (17 dòng, mọi hash = CVGP 21, H8 khớp) |
| 01 | cvm2-01-nhap-sheet-level2-va-sprite-moi | **XONG** | (commit "CVM2 01") | 16/10/2026 | 2 sheet sha256 khớp + SOURCES; kiểm kê chạy lại y hệt planner; đối chiếu Tilesets (§3b); 9 sprite mới (crusher_0..7 + platform) md5 lặp y hệt, verify 0 lỗi; importer đọc pivot manifest Map 2 |
| 02 | cvm2-02-hieu-chuan-video-timeline-va-su-kien | **XONG** | (commit "CVM2 02") | 16/10/2026 | hiệu chuẩn tinh (Map_2 ZNCC 0.9468/10 frame, boss gradient 0.955/7 frame; **hàng NES nhìn thấy 8–231**); HUD 7814 frame (20/20 frame kiểm mắt đúng); camera 100 %, Simon 92.9 %; 78 sự kiện HUD có nghĩa; 12 chuyển cảnh đo tick; **MAP2_TIMELINE.md viết lại (57 dòng §3 + §3b–d + §4 boss + §7)**; bảng số ở `ref/data/`; **phát hiện: vũ khí "nước thánh" của Map_2 là ĐỒNG HỒ (stopwatch)**, STAGE đổi ngay cuối cuộn 2 của cửa, đồng hồ đứng 186–196 tick (wiki 5 s) — không sửa game (H9) |
| 03 | cvm2-03-nhac-16-bai-va-moc-nhac-map1-map2 | **XONG** | (commit "CVM2 03") | 16/10/2026 | 16 MP3 nhập (sha256 §3d) + `eMusic`/`MusicLibrary` + lặp theo mẫu (10 bài; ±1 mẫu ở đầu ra thật); bài ↔ mốc bằng xcorr 5 video (§2d: Block 2 = **Stalker**, boss = Poison Mind, Stage Clear = nhặt cầu, bản đồ im); mốc Map 1 trong Unity §2e; **bảng mốc Map 2 §2f cho routine 11**; 24 ô Config → `eMusic` + nguồn; Death lúc bắt đầu chết; mọi hash = bảng chuẩn |
| 04 | cvm2-04-lo-nuoc-thanh-xuyen-dich | **XONG** | (commit "CVM2 04") | 17/10/2026 | bằng chứng 7 lần ném (2 video boss, §2g): lọ **xuyên** địch, chỉ vỡ ở sàn, **gây 1 đòn** khi bay qua (boss1 f471, boss2 f393); **Unity ĐÃ đúng từ CVGP 04/09** (`ConfigSubWeapon.HolyWater.pierces = 1` ⇒ không vỡ khi trúng) — "Unity vỡ ngay" của CVGP 14/16 chỉ dựa vào comment sai, chưa từng đo; sửa comment `HolyWater`/`BossBase`, nguồn `src_pierces` → video; probe mới `HolyPierceProbe` (bộ `holy` 5/5 `52c59864`); **mọi hash cũ giữ nguyên** |
| 05 | cvm2-05-tilemap-va-luoi-va-cham-block2 | chưa chạy | | | |
| 06 | cvm2-06-be-di-dong-bay-ep-gai-va-ho-chet | chưa chạy | | | |
| 07 | cvm2-07-quet-hoi-quy-giua-cum | chưa chạy | | | |
| 08 | cvm2-08-enemy-block2-do-lai-theo-video | chưa chạy | | | |
| 09 | cvm2-09-queen-medusa-va-phong-boss-that | chưa chạy | | | |
| 10 | cvm2-10-dat-vat-the-nen-bi-mat-va-spawn | chưa chạy | | | |
| 11 | cvm2-11-luong-stage-4-6-cua-trai-va-stage07 | chưa chạy | | | |
| 12 | cvm2-12-nghiem-thu-map2-so-voi-video | chưa chạy | | | |

## §2 Timeline & số đo

### 2a. Hiệu chuẩn video → lưới NES (planner 15/10 — `CVM2_PLAN §3a`; routine 02 đo kỹ)
| | Map_2 (1486×712) | Boss_Map_2 (1490×718) |
|---|---|---|
| `x_NES = (x_v − ox)/sx`, `y_NES = (y_v − oy)/sy` | **sx 3.623, sy 3.104, ox 268.93, oy −20.24** (ZNCC 0.934) | **sx 3.609, sy 3.157, ox 274.33, oy −22.95** (0.843) |
| Tỉ lệ pixel | 7/6 (như Map_1) | 8/7 (như boss Map 1) |

Dòng r của vùng sheet ↔ **NES y = 32 + r**. Đo ảnh: so trên lưới NES (`ref/nes/`) — đừng scale đều 2 chiều. **Kiểm mẫu số routine 00 (16/10):** chạy lại `calib_map2.py map2 12:TowerEntranceA:0 100:TowerHallB:47 150:TowerHallC:512 225:Medusa:0` ⇒ sx **3.620** (planner 3.623, lệch 0.003 ≤ 0.005), sy **3.100** (3.104, lệch 0.004), ox 269.12 (lệch 0.19 px video = 0.05 px NES), oy −19.6 (lệch 0.64 px video = 0.21 px NES ≤ 1 px), ZNCC 0.9386 (planner 0.934) ⇒ **mẫu số planner đúng trong dung sai**.

### 2b. Mốc chính của video Map_2 (planner — `CVM2_PLAN §3b`, bảng đầy đủ `ref/MAP2_TIMELINE.md`)
| Mốc | Sự kiện |
|---|---|
| 0:00–0:05.7 | bản đồ lâu đài (đuôi Map 1), STAGE 04 TIME 0000 → đen 0:05.77–0:06.20 |
| **0:06.23** | **STAGE 04 — TEA cx 0**, TIME 0400; nến đầu ⇒ thập tự (0:08); leo thang; +2000 vương miện (~0:19) |
| 0:28.50 | đen 11 frame → **TEB cx 256**, đi TRÁI; Spear Knight, dơi; bệ di động 0:44–0:51 |
| **0:51.80–0:57.67** | **CỬA TRÁI** (camera cuộn trái, cửa mở, Simon tự đi trái, đóng, cuộn) |
| **0:57.70** | **Hall A cx 512**, đi TRÁI; Medusa Head; rương báu +2000 (1:20–1:22); thang trái lên. **HUD đổi "STAGE 04" → "STAGE 05" ở ~58.75 s** (routine 00 đo: 58.7 s còn 04, 58.8 s đã 05 — chậm ~1 s sau khi section Hall A hiện; Map 1 đổi ngay khi vào section ⇒ routine 02/11 xác nhận luật đổi số stage) |
| 1:33.00 | đen → **Hall B cx 0**, đi PHẢI tới 512 (1:54; thịt nướng 1:56), quay TRÁI tầng trên; bệ di động 1:40–1:46 |
| **2:22.33–2:28.30** | CỬA TRÁI → **STAGE 06 — Hall C cx 512** (2:28.33; **HUD ở 2:28.4 còn "STAGE 05" TIME 0281**, đã "STAGE 06" ở 2:38 — cùng luật HUD đổi chậm), đi TRÁI; thịt nướng; **3 bẫy ép gai** 2:33–2:41; Bone Tower, Ghost, III (2:50); thang trái lên (cx 0 2:52) → đi PHẢI |
| 3:17.13 | đen → **sảnh Medusa cx 512**, đi TRÁI → phòng boss khoá **cx 0** (~3:46) |
| 3:46–3:56 | **Queen Medusa** (+3000 ~3:56) → quả cầu 3:58 (TIME 0210) → tally 4:00–4:12 (SCORE 44670 = luật Map 1 ✓) |
| 4:12.80 | đen 31 frame → **bản đồ lâu đài Block 2**, HUD **STAGE 07** tim 05 → đen 4:19.73 → hết video |

Video boss (23 s): tượng Medusa giữa phòng → ~2.5 s đầu Medusa bay ra (thân tượng còn ở bệ) → bay sóng thấp, đổi màu xanh↔tím, rắn rơi + trườn, Simon trúng đòn 16 → 4 vạch → ~16.5 s lửa chết → ~19 s quả cầu → nhặt +3000 (`MAP2_TIMELINE §4`).

### 2c. Routine 02 (16/10/2026) — hiệu chuẩn tinh + timeline (chi tiết đầy đủ `ref/MAP2_TIMELINE.md`; bảng theo frame `ref/data/`)
**Hiệu chuẩn mới (thay §2a)**: Map_2 **sx 3.621 · sy 3.101 · ox 268.888 · oy −19.736** (ZNCC 0.9468, 10 frame sạch; sai số sx ±0.003, tâm y ±0.2 px video); Boss_Map_2 **sx 3.608 · sy 3.156 · ox 274.456 · oy −23.016** (ZNCC gradient 0.955, 7 frame). Đã ghi vào `Tools/cvm2/nes2.py CALIB`. **Hàng NES nhìn thấy Map_2 = y 8–231 (224 hàng, đủ 256 cột)** (planner 7–235 sai); boss ≈ 10–229. Watermark Map_2 ngoài vùng game; video boss: ▶ YouTube f0–22, ko-fi NES x 0–85 y 216–227, OVERTIME GAMER NES x 92–166 y 21–38.
**Nhịp quay**: 23.7 frame game mới/s (20.9 % lặp); TIME **64.0 tick/đv**; 1 frame video = 2.0033 tick. **Camera giữ Simon ở px 128 mọi hướng, không vùng chết.**
| Mốc (số đo, tick) | Giá trị |
|---|---|
| Đen đổi tầng / vào stage | 22 / 22 / 20 tick (11/11/10 frame); bản đồ→Stage 04 & bản đồ B2→Stage 07: **28 tick**; tally→bản đồ B2: 22 tick |
| Cửa TRÁI (2 cửa, cùng cấu trúc) | cuộn 1 = **128 tick** (128 px, **1.00 px/tick**) → dừng **≈158 / 166 tick** (cửa hiện lúc dừng; Simon đi TRÁI **66 px** 1 px/tick từ 26–36 tick sau; cửa tắt 50 tick trước cuộn 2) → cuộn 2 = 128 tick; **STAGE HUD đổi ≈ 4 tick sau camera dừng** (cuối cuộn 2); TIME đứng suốt cửa (giảm lại +26 frame sau STAGE) |
| Đổi tầng bằng thang | cả 3 lần = thang lên-phải cắt mép trên: Simon rời Y 63 → hiện **Y 221 mép dưới, x +3 (+259 cho TEB)**, vẫn leo; TIME giảm lần kế **40–41 frame (80 tick) sau đầu màn đen** |
| Stage 04 bắt đầu | TEA cx 0, Simon px 64 Y 208 quay PHẢI đứng 13 frame; TIME 0400, giảm lần đầu +58 tick; ENEMY 0→16 lúc f187 |
| Khoá camera boss | cx 0 từ **3:44.50** (f6735); Medusa thức 3:47.37 (+2.87 s) |
| Tally | chờ ≈272–280 tick sau chạm cầu → TIME **1 đv/tick** (+10) → nghỉ ≈56 → tim **1/8 tick** (+100) → chờ ≈118 → đen 22; SCORE 039570→044670 ✓ luật Map 1 |
| Bản đồ lâu đài B2 | trượt vào từ phải **1.50 px/tick, 162 tick**; Simon nhỏ hiện +0.2 s, đi **1.004 px/tick** x 18→241; **Stage 07 TIME 0500** (8 frame cuối video; Block 3 nền xanh-xám rêu) |
| Điểm (video = wiki) | Bat treo 200 · Medusa Head 300 · Ghost 300 · Spear Knight 400 · Bone Tower 400 · rắn Medusa 100 · Medusa 3000; túi 100/400/700; crown/rương 2000 |
| Quái | Medusa Head **|vx| 1.25 px/tick** (14 vết); Spear Knight đi ≈0.38 px/tick (thô); Ghost 0.31–0.58 (thô); Bone Tower (248,96) & (312,64) Hall C; sinh MH phía Simon quay mặt |
| Bẫy ép | wx 368/432/496, thu 119 … hạ 175, chu kỳ **128 tick (496), 80 tick (432)** (thô — 06) |
| **Đồng hồ (stopwatch)** | **−5 tim; quái/bẫy/Medusa đứng ≈ 186–196 tick** (wiki 5 s = 300 tick ⇒ khác; §6); dùng ×4 trong video; TIME không giảm lúc đứng |
| Boss video | Medusa thức f68 (2.27 s), bay sóng thấp ≈1 px/tick, nháy xanh↔tím mỗi ≈9 tick, E 16→0 khi bị lửa nước thánh (1 vạch/30 tick), **hit −2 vạch ×3**; **knock-back hit #3 = 31 px/36 tick, đỉnh −14 px** (cho `cvgp-fix-03`); quả cầu +150 tick sau chết, rơi 26 tick; PLAYER hồi +1/7.4 tick |
| PLAYER Map_2 | **16/16 suốt video (không bị trúng, không rơi hố)**; P 03→04 do SCORE ≥ 30000 (1:21.37) |

### 2d. Routine 03 (16/10/2026) — nhạc: bài nào ở đâu (tương quan chéo âm thanh 5 video) + điểm lặp
**Cách đo** (`Tools/cvm2/music3.py`, python `-P -E`, từ gốc repo; wav giải mã ffmpeg ở `ClaudeNotes/cvm2/_cache/03/audio/`): phổ log 64 băng (90 Hz–6 kHz, hop 11.6 ms, chuẩn hoá từng khung) — cửa sổ video 3 s bước 0.5 s tương quan chéo (FFT) với CẢ 16 bài ⇒ bài + songpos + NCC (`ident`), gộp đoạn phát liền theo "gốc bài" = t − songpos (`seg`, songpos đưa về vòng 1); mép bắt đầu/dừng đo bằng cửa sổ 0.1–0.2 s (`Tools/cvm2/music3_track.py`). Bảng thô `ref/data/music/ident_<video>.csv`. Bài có lặp nội bộ (VK 3.73 s / 33.55 s, Stalker 4.26 s) cho "gốc bài" giả cách nhau đúng độ lặp đó — dòng chính chọn theo đoạn dài nhất + mốc hình ảnh.

| Video | Gốc bài (s) | Bài | NCC | Mốc hình / sự kiện | Hành vi |
|---|---|---|---|---|---|
| Title_Intro | 0–8.43 | — | — | Title | im lặng (CVTI) |
| Title_Intro | **8.429** | Prologue | 0.87 | Intro hiện L448 = 8.497 s ⇒ nhạc sớm **4 tick** (CVTI silencedetect 8.436) | 1 lần (không lặp), hết 14.86 |
| Title_Intro | **16.40** | Vampire Killer | 0.93 | Stage 01 L922 (CVTI 16.414) — lệch 0.2 tick | lặp |
| Map_1 | **2.215** | Vampire Killer | 0.94 | STAGE 01 (0:02.2) | lặp; gốc 2.24→32.02→61.88→91.71‖97.96→127.73→157.56→187.42 (vòng 29.78–29.86 s; ‖ = video cắt ≈6.25 s quanh 1:48 trang YouTube) ⇒ **chạy liền qua cổng 0:16–0:20, cửa 1:28, STAGE 02 1:31.9, đổi tầng, cửa 2:47, STAGE 03 2:50.9 — KHÔNG phát lại** |
| Map_1 | **198.80** | Poison Mind | 0.96 | VK tắt 198.7 (cùng lúc); camera khoá cx 512 **197.40** (+1.40 s); dơi đổi sprite thức **199.47** (−0.67 s); lao đầu ≈200.0 (−1.2 s) | lặp tới nhặt cầu (CVGP ghi ≈200.17 NCC 0.69 — số mới chính xác hơn) |
| Map_1 | **213.06** | Stage Clear | 0.93 | nhặt cầu f6391 = 213.03 | 1 lần (kêu 4.10 s) |
| Map_1 | 217–239.8 | — | < 0.5 | tally → đen → bản đồ B1 | **không có bài nhạc** (chỉ SFX đếm + SFX bước bản đồ `MapStep`) |
| Boss_Map_1 | 0–2.6 | Vampire Killer | 0.6–0.86 | Simon vào phòng | — |
| Boss_Map_1 | **2.81** | Poison Mind | 0.93 | dơi thức f66 = 2.20 (+0.61 s) ≈ **lao đầu 2.79** (CVGP: thức + 33 tick) | lặp |
| Boss_Map_1 | **20.76** | Stage Clear | 0.92 | nhặt cầu / hồi máu ≈ 20.9 | 1 lần |
| Map_2 | 0–6.0 | — | 0.43–0.50 (bài 09, songpos đứng yên = không phải nhạc) | bản đồ B1 + đen 5.77–6.20 | **không nhạc** (âm nhỏ = SFX bước bản đồ) |
| Map_2 | **6.177** | **Stalker** | 0.94 | STAGE 04 hiện 6.23 (đen hết 6.20); tiếng đầu 6.195 | lặp; gốc 6.177→36.00→65.82→95.66→125.48 (vòng 29.82–29.84) ⇒ **chạy liền qua đổi tầng 0:28.5 / 1:33 / 3:17.1, cửa trái #1 0:51.8–57.7 (STAGE 05), cửa trái #2 2:22.3–2:28.3 (STAGE 06) — KHÔNG phát lại** |
| Map_2 | 161.0 → 163.4 | Stalker | — | **2:40.97 dùng đồng hồ**: nhạc tắt 161.0, SFX đồng hồ 161.1–162.8, lặng 162.9–163.3, nhạc **chạy tiếp đúng chỗ** 163.4 | gốc 125.48 → 157.71 (= +29.826 + **2.40 s**) ⇒ nhạc **đứng 2.40 s (144 tick)**, không phát lại |
| Map_2 | 157.71→187.53 | Stalker | 0.7–0.9 | tới 3:47.8 (qua khoá camera cx 0 3:44.50) | — |
| Map_2 | **≈228.14** (≡ 198.3 mod 7.4615) | Poison Mind | 0.45–0.49 (SFX đồng hồ che) → **0.95–0.98** ở 236.8–238.7 | Medusa thức **227.37** (+0.77 s); đồng hồ 227.90 / 230.93 / 234.37 (không thấy đứng 2.4 s như Stalker — ±0.4 s); Medusa chết 235.63, lặng 236.2–236.6 | lặp tới nhặt cầu |
| Map_2 | **238.83** | Stage Clear | 0.93 | nhặt cầu (cầu hiện = chết + 150 tick = 238.13, rơi 26 tick → 238.56, Simon tới px 113) | 1 lần |
| Map_2 | 241–259.8 | — | — | tally SFX 4:02.6–4:10.9 → lặng 4:11.3–4:16.25 → bản đồ B2 (SFX bước) → đen 4:19.80 | không nhạc |
| Map_2 | **259.80** | **Wicked Child** | 0.48 (bài kế 0.14; chỉ 0.3 s) | STAGE 07 (4:20.15) | bằng chứng yếu cho Block 3 |
| Boss_Map_2 | 0–3.0 | Stalker | 0.6–0.93 | trước khi Medusa thức | — |
| Boss_Map_2 | **3.22** | Poison Mind | 0.94 | Medusa thức 2.27 (**+0.95 s**); không dùng đồng hồ ⇒ gốc không lệch suốt trận | lặp |
| Boss_Map_2 | **20.70** | Stage Clear | 0.92 | nhặt cầu 20.6–20.7 | 1 lần |

**Kết luận**: Block 1 = Vampire Killer, **Block 2 = Stalker (xác nhận)**, boss Block 1–2 = Poison Mind, Stage Clear = lúc nhặt cầu (4/4 video, ±0.05 s), bản đồ lâu đài + tally = không nhạc, Intro = Prologue, STAGE 07 = Wicked Child (yếu). Không video: Death, Game Over, bài Block 4–6, Dracula, ending ⇒ suy luận. Tempo bài = tempo game (vòng trong video 29.78–29.86 s ≈ vòng MP3 29.82 s).
**Phantom Bat — 2 video lệch nhau**: PM bắt đầu ở Map_1 0.67 s TRƯỚC khi dơi đổi sprite thức, ở video boss 0.61 s SAU (= lúc lao đầu). Không quy tắc vị trí/khoá camera nào khớp cả hai (Map_1 Simon px 141 lúc PM; video boss Simon đứng px 184) ⇒ giữ quy tắc CVGP (PM lúc lao đầu = thức + 33 tick — khớp video boss ±0.03 s), hỏi §5 #6. Giả thuyết: ở Map_1 Simon ném rìu 197.43/197.97 có thể đánh thức dơi sớm hơn lúc sprite đổi.
**Medusa**: PM = Medusa hiện/thức + 0.77–0.95 s (≈ 46–57 tick) ở cả 2 video — `QueenMedusa` hiện gọi `StartBossMusic` lúc hết Emerge (32 tick, số CVGP 19) ⇒ routine 09 đặt lại theo số này.

**Điểm lặp** (`music3.py loop2` + `export` ⇒ `Tools/cvm2/music_loops.json` ⇒ `MusicLibrary.asset`): bản rip KHÔNG lặp đúng từng mẫu (độ trễ giữa 2 vòng dao động ±12 ms — jitter khung ghi âm; NCC dạng sóng 1 s chỉ 0.3–0.7) ⇒ điểm lặp chọn theo **khớp dạng sóng cục bộ 80 ms hai phía điểm nhảy** (quét 0.05 s trong 10 s đầu vùng lặp, lag ±50 ms quanh độ dài vòng thô lấy từ ma trận tự tương đồng). Mẫu = clip Unity 44.1 kHz (= ffmpeg + **2257** — Unity giữ độ trễ bộ mã MP3 1105 + 1 khung 1152; đo 5/5 jingle NCC ≥ 0.999); phát từ mẫu 2257.

| # | Bài | Dài (s) | Lặp | loopStart → loopEnd (mẫu Unity) | vòng (s) | NCC trước/sau | gain ở loopEnd | ghi chú |
|---|---|---|---|---|---|---|---|---|
| 01 | Underground (FDS) | 28.98 | có | 244 806 → 669 180 | 9.623 | 0.993 / 1.000 | 0.99 | không dùng |
| 02 | Prologue | 7.06 | **không** | — | — | — | — | Intro |
| 03 | Vampire Killer | 67.00 | có | 33 127 → 1 350 059 | 29.862 | 0.977 / 0.990 | 1.19 | vòng từ 0.70 s (gần đầu bài) |
| 04 | Stalker | 72.01 | có | 579 967 → 1 895 019 | 29.820 | 0.922 / 0.949 | 0.99 | intro ≈ 4 s |
| 05 | Wicked Child | 76.02 | có | 1 009 942 → 3 006 096 | 45.264 | 0.921 / 0.944 | **0.49** | **xấp xỉ**: rip chỉ 1 vòng rồi fade — điểm nhảy rơi vào đầu fade |
| 06 | Walking on the Edge | 82.01 | có | 394 747 → 1 945 585 | 35.166 | 0.998 / 1.000 | 0.94 | |
| 07 | Heart of Fire | 71.01 | có | 90 457 → 2 556 382 | 55.917 | 0.961 / 0.937 | 0.95 | |
| 08 | Out of Time | 70.01 | có | 121 326 → 1 530 338 | 31.950 | 0.917 / 0.972 | 0.95 | |
| 09 | Nothing to Lose | 39.98 | có | 8 872 → 1 556 726 | 35.099 | 0.984 / 0.975 | **0.71** | **xấp xỉ**: 1 vòng + fade (như 05) |
| 10 | Poison Mind | 38.00 | có | 410 181 → 739 234 | 7.462 | 0.977 / 0.996 | 0.99 | cả bài tuần hoàn 7.46 s |
| 11 | Black Night | 41.98 | có | 368 286 → 1 072 798 | 15.975 | 0.970 / 0.979 | 1.00 | |
| 12 | Stage Clear | 5.06 | **không** | — | — | — | — | kêu 4.10 s |
| 13 | Game Complete | 5.06 | không | — | — | — | — | chưa dùng |
| 14 | Voyager (Ending) | 52.06 | không | — | — | — | — | không thấy vòng; chưa dùng |
| 15 | Death | 2.07 | **không** | — | — | — | — | |
| 16 | Game Over | 5.06 | **không** | — | — | — | — | |

**Kiểm trong Unity (âm thanh đầu ra thật — `MusicProbeTap` ghi WAV 48 kHz, `Tools/cvm2/music3_tapcheck.py`)**: đặt timeSamples = loopEnd − 1 s rồi ghi 2.5 s: VK / Stalker / Poison Mind — vị trí bài sau cú nhảy tiếp đúng **±1 mẫu** (VK 0, Stalker −1, PM −1), NCC từng cửa sổ 50 ms với bài gốc **min 0.985 / 0.965 / 0.992** (trung vị 0.998 / 0.994 / 0.999), RMS 10 ms quanh cú nhảy bám đúng đường RMS bài gốc (không ô ~0 ⇒ không khoảng lặng). Log `AudioManager` (MusicProbe): VK 1 350 423 → 33 491 (vượt loopEnd 364 mẫu = 8 ms trước khi Update lùi — đoạn sau loopEnd là vòng 2 cùng nội dung nên không nghe ra).

### 2e. Mốc nhạc luồng Map 1 trong Unity (luồng THẬT Title → STAGE 04, `MusicProbe` + `RegressionRunner "flow"`, L = frame − L0)
| Mốc | Unity | Video | Lệch | Ghi chú |
|---|---|---|---|---|
| Title | StopMusic L0 | im | — | |
| Prologue | **L448** (tick bật Intro) | L443.9 (8.429 s) | **+4.1 tick = +0.07 s** | ≤ 0.1 s; giữ phát ở OnEnter (CVTI §4.3) ⇒ H8 không đổi |
| Prologue hết | L876 (clip 7.13 s) | 14.86 s | — | cuối clip im (kêu tới 6.42 s) |
| Vampire Killer | **L922** (StageScreen.OnEnter) | L922.2 | −0.2 tick | lặp mỗi **1792 tick** (29.87 s: L2756, L4548, L6340) đúng vòng đo; KHÔNG phát lại ở cổng/cửa/STAGE 02/03 |
| Poison Mind | L6366 = boss thức L6334 + **32** tick (lao đầu) | thức + 0.61 s (video boss) / −0.67 s (Map_1) | quy tắc CVGP | lộ trình autopilot nhanh hơn video ⇒ so theo sự kiện, không theo giây tuyệt đối |
| Stage Clear | **L8168** = tick nhặt cầu L8168 | = nhặt cầu (4 video ±0.05 s) | 0 | 1 lần, hết L8473 |
| Bản đồ lâu đài | StopMusic L8948 | im | 0 | |
| STAGE 04 (Stage4Stub) | Stalker **L9370** | Map_2 0:06.18 = STAGE 04 hiện | 0 | Stage4Stub là điểm cuối luồng Map 1 (vốn có trường nhạc Stalker) |
| Chết (spam4) | Death lúc Simon BẮT ĐẦU chết, kêu đủ 126 tick → GAME OVER (+30) → Game Over; CONTINUE → VK từ đầu | không video | suy luận | trước sửa: Death phát SAU 124 tick chết và bị màn GAME OVER cắt sau 32 tick |
Hai lần chạy (trước/sau sửa Death) cho mốc luồng y hệt (tất định). 0 lần "đứng" (timeSamples không tăng khi đang phát).

### 2f. BẢNG MỐC NHẠC MAP 2 — cho routine 11 (routine 03 KHÔNG gắn vào luồng Map 2 — luồng chưa có)
| # | Sự kiện luồng Map 2 | Video Map_2 (boss video) | Bài | Hành vi | Cách dựng (API có sẵn) |
|---|---|---|---|---|---|
| 1 | bản đồ B1 → đen 28 tick | 0:00–6.20 | — | im | `CastleMapScreen` đã `StopMusic` |
| 2 | STAGE 04 hiện (TEA cx 0) | **0:06.18** (STAGE 04 hiện 0:06.23) | Stalker | phát từ đầu ở tick bật màn | `StageScreen.OnEnter → flow.PlayStageMusic()` (đọc `ConfigStage[4].music` = Stalker) |
| 3 | đổi tầng bằng thang (TEA→TEB 0:28.5; Hall A→B 1:33.0; Hall C→Medusa 3:17.1) | — | Stalker | **TIẾP TỤC** | không gọi `PlayMusic` |
| 4 | cửa trái #1 → STAGE 05 (0:51.8–57.7); #2 → STAGE 06 (2:22.3–2:28.3) | — | Stalker | **TIẾP TỤC** (không phát lại) | `GameRules.EnterStage` không gọi nhạc; đừng gọi `PlayStageMusic` khi đổi stage |
| 5 | dùng đồng hồ | 2:40.97 | Stalker | **đứng 2.40 s (144 tick)** rồi chạy tiếp đúng chỗ (SFX đồng hồ) | CHƯA dựng (H9 — chính sách SFX↔nhạc); 3 lần dùng trong trận boss không thấy đứng (±0.4 s) ⇒ chưa chắc — §6 |
| 6 | khoá camera phòng boss cx 0 | 3:44.50 | Stalker | TIẾP TỤC | — |
| 7 | Medusa thức (tượng vỡ) | 3:47.37 (boss 2.27) | **Poison Mind** | phát **+0.77–0.95 s (≈ 46–57 tick)** sau khi Medusa hiện | `BossBase.StartBossMusic()` (đọc `ConfigBlock[2].bossMusic`); routine 09 dời lời gọi trong `QueenMedusa` (hiện = hết Emerge 32 tick) |
| 8 | Medusa chết | 3:55.63 | Poison Mind | TIẾP TỤC tới nhặt cầu | (lặng 0.4 s ở 3:56.2 — có lẽ SFX) |
| 9 | nhặt quả cầu | **3:58.83** (boss 20.70) | Stage Clear | phát 1 lần cùng tick | `StageFlow.OnCrystal` (có sẵn) |
| 10 | tally → đen 22 → bản đồ B2 | 4:02.6–4:19.8 | — | im | bài SC tự hết; `CastleMapScreen.StopMusic` |
| 11 | STAGE 07 (Stage7Stub) | **4:19.80** | Wicked Child | phát từ đầu | `AudioManager.PlayMusic(ConfigStage[7].music)` |
| 12 | Simon chết / hồi sinh | (không có trong video) | Death → bài stage từ đầu | đã dựng | `SimonController.DyingStarted` → Death; `OnRespawned → PlayStageMusic` |
| 13 | Pause (Start) | (không có) | — | tạm dừng / chạy tiếp | `StageRules` → `AudioManager.PauseMusic` (CVGP 12) |

### 2g. Routine 04 (17/10/2026) — lọ nước thánh khi bay chạm enemy/boss (đo video, lưới NES; `Tools/cvm2/holy4_*.py`)
Cách đo: tim −1 (HUD) = lúc bấm ném (lọ rời tay ≈ 8 frame sau); ENEMY −1 = trúng (`holy4_hud.py` đọc `boss_hud.csv` CVGP + `ref/data/boss2_hud.csv`); frame lưới NES liên tiếp phóng 3× (`holy4_sheets.py`, hiệu chuẩn boss1 = `nes_common` CVGP, boss2 = `nes2` CVM2 02) — **đã mở xem từng tấm** (`_cache/04/b1_f334_zoom.png`, `b1_f458_zoom.png`, `b2_f128_zoom.png`, `b2_f160_zoom.png`, `b2_f378_zoom.png`).

| # | Video · frame | Mô tả (toạ độ NES) | ENEMY | Kết luận |
|---|---|---|---|---|
| 1 | Boss_Map_1 f326→**f334–350** (11.1–11.7 s) | lọ ném sang TRÁI, rơi tới sàn; dơi lơ lửng (≈167,172), lọ (199,187)→(190,193) — cách mép thân dơi ≈ 10 px, **không chồng** | 16 giữ | KHÔNG chạm. **CVGP 14 ghi "f342–352 lọ xuyên dơi" là nhầm** (dơi đứng = đang lơ lửng) |
| 2 | Boss_Map_1 f459→**f467–483** (15.57–16.10 s) | lọ rời tay f467, chạm cánh phải dơi **f471**, bị thân dơi che f474–479 (sprite dơi vẽ đè), **ló ra dưới-trái vẫn bay f480–482**, lửa trên sàn **f483**; dơi đứng yên f468–485 | **16→15 f471** | XUYÊN + **1 đòn khi bay** (không có lửa trên màn — lửa trước tắt f459; Simon tư thế ném, không roi) |
| 3 | Boss_Map_1 f471→f479–485 | lọ #2 rời tay ≈f479, vào cánh phải dơi f480–485 rồi qua thân | **15→14 f480** | (khả năng) lọ #2 cũng 1 đòn — cách đòn trước 9 frame ≈ 18 tick > bất tử 16 |
| 4 | Boss_Map_2 f126→**f134–149** (4.47–4.97 s) | lọ từ Simon (x≈25) qua rìa trái-dưới thân Medusa (cằm/tóc) f137–143, ló ra phải f144–147, vỡ sàn x≈70 **f148–149**; Medusa đứng yên | 16 giữ lúc chồng; **16→15 f149** = lúc lửa hiện | XUYÊN; lần này **không** mất máu khi lọ chồng rìa thân (hộp nhận đòn NES nhỏ hơn sprite tóc? / NES không dò va chạm mỗi frame) |
| 5 | Boss_Map_2 f141/f156→f160–163, **f164–179** | lọ qua rìa phải-dưới (f160–162) rồi vỡ sàn f163; lọ kế qua MẶT Medusa f168–176, vỡ sàn phải ≈f179 | 15→14 f165 (lửa sàn), 14→13 f177 + 13→12 f181 (tia roi Simon f173–182) | XUYÊN; lọ trong mặt f168–172 không trừ máu — Medusa vừa trúng f165 (bất tử) |
| 6 | Boss_Map_2 f375→**f383–402** (12.77–13.40 s) | lọ từ Simon (x≈85) vào tóc trái f387–389, **trong mặt f390–393**, ló ra dưới-phải f394, vỡ sàn x≈116 **f402** | **10→9 f393** | XUYÊN + **1 đòn khi bay** (không lửa nào trên màn; Simon đang ném lọ #2 f388–396 — không roi) |
| 7 | Boss_Map_2 f388→f396–408 | lọ #2 qua tóc/mặt f401–407 | 9→8 f409 | trùng lúc lửa lọ #1 ở mép phải-dưới Medusa — không tách được |
| — | Map_2 3:46–3:56 | Simon dùng **đồng hồ** (không phải nước thánh — CVM2 02) | — | không phải bằng chứng nước thánh |

**Kết luận (H1 — video)**: (a) lọ **không vỡ khi chạm** enemy/boss — 6/6 lần lọ đi qua thân đều bay tiếp và chỉ vỡ khi tới sàn; (b) lọ **có gây đòn** khi bay qua (2 lần rõ: boss1 f471, boss2 f393; 1 lần khả năng: boss1 f480) — 1 đòn/lọ/mục tiêu; 2 lần chồng không trừ máu đều giải thích được (rìa thân / đang bất tử); (c) boss **đứng yên** suốt lúc lọ trong thân (dơi f468–485, Medusa f137–147) = cơ chế `BossBase.TouchingHolyWater` có sẵn; (d) lửa giữ chân + đốt (dơi f483 → chết f542 trong lửa, Medusa 1 vạch/≈30 tick) = cơ chế có sẵn.
**Unity (đo, `HolyPierceProbe`, Arena)**: đối chứng không mục tiêu: lọ sinh rel 18 (300,153) → vỡ rel 48 lửa (345,176). Bia (Enemy, hộp 314–329 × 144–175) đứng x 322: lọ trúng rel 26 tại (312,146), chồng hộp 15 tick, vỡ **rel 48 lửa (345,176) = y hệt đối chứng**, bia trúng đúng 1 lần. Phantom Bat (732,160): trúng rel 27 (E 16→15), chồng 21 tick, dơi khựng + đứng yên suốt, vỡ rel 48 lửa (745,176) dưới dơi ⇒ giữ chân 80 tick + đốt 5 lần. Queen Medusa (Fly, 64,157): trúng rel 155 (E 16→15), chồng 21 tick đứng yên, vỡ rel 176 lửa (77,176). Shot III: 3 lọ qua bia A = 3 đòn (1/lọ), bia B trong lửa 12 đòn, `subWeaponHits` 15 = A + B (luật 10 lần ⇒ multiplier đếm đúng).
**7 cặp ảnh video ↔ Unity đã xem** (`_cache/shots/04/pairs_0..3.png`): lọ chạm thân dơi + ENEMY 15 (f471 ↔ bat r027) · lọ trong thân, dơi đứng yên (f476 ↔ r033) · lọ ló ra vẫn bay (f481 ↔ enemy r041) · lửa sàn phía sau (f484 ↔ bat r049) · lọ vào thân Medusa (f390 ↔ r155) · lọ trong mặt, ENEMY −1 (f393 ↔ r161) · lửa sàn phía sau Medusa (f402 ↔ r177). Phòng Medusa Unity = phòng thử Arena (phòng thật: routine 09).

## §3 Tài nguyên (đường dẫn sinh ra)

| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Kế hoạch / chỉ mục video / wiki / kiểm kê (planner, **commit ở routine 00**) | `ClaudeNotes/cvm2/CVM2_PLAN.md`, `ref/MAP2_TIMELINE.md`, `ref/nes/*.png` (36 ảnh lưới NES), `ref/tools/*.py` (7 script đo), `wiki/MAP2_WIKI_DATA.md`, `inventory/level2_main_hall.json` + `level2_main_hall_annot.png` | `_cache/` KHÔNG commit (video ref 18 MB, CSV từng frame, ảnh wiki, tools chạy được `_cache/tools/`, bộ sinh SKILL `_cache/routines/`) |
| **Bộ chạy hồi quy** (routine 00) | `Assets/Data/Script/Debug/RegressionRunner.cs` (+ `.meta`; chỉ Editor) | gọi qua MCP `execute_code` khi Play — xem §4 "Bộ chạy hồi quy" |
| Harness sửa ở routine 00 (không đổi hash) | `RulesProbe.cs` (trả StageRules/AutoRespawn/sự kiện cuối bộ; bia `touchDamage = true`), `BossB24Probe.cs` (`DestroyImmediate`), `Map1AcceptProbe.cs` (`StartSpam` thêm tham số warp/mạng/máu) | `§7 Routine 00` |
| Kết quả baseline routine 00 | `ClaudeNotes/cvm2/_cache/00/{arena,arena60,core,main,order,alias}/suites/*.txt` + `SUMMARY.txt`, ảnh `_cache/shots/00/` | không commit |
| Công cụ video Map_2 | `ClaudeNotes/cvm2/_cache/tools/` (chạy từ `ClaudeNotes/cvm2/_cache/`; bản lưu `ref/tools/`), `nes2.py` (hệ số Map_2) | python `-P -E`; ffmpeg `C:\ffmpeg\bin\ffmpeg.exe` |

### 3b. Routine 01 (16/10/2026) — nhập sheet + sprite mới
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Sheet Level 2 | `Art_Source/spriters-resource/level2_main_hall.png` | nguyên byte, sha256 `6d09c60e…9313fd`, 95 606 B (khớp lúc chép) |
| Sheet Tilesets | `Art_Source/spriters-resource/tilesets.png` | nguyên byte, sha256 `dd679ecd…378491`, 180 462 B; `SOURCES.md` +2 dòng |
| Công cụ | `Tools/cvm2/{nes2,inv_level2_tiles,inv_level2,calib_map2,locate_map2}.py`, `cmp_tilesets.py` (mới), `slices_map2.json` | `nes2.SHEET`/`inv_level2_tiles` trỏ sheet trong repo (theo `__file__`), video giữ đường dẫn tuyệt đối; chạy từ gốc repo, `python -P -E` |
| Sprite mới | `Assets/Data/Art/Object/crusher_0..7.png` (32×64), `platform.png` (32×16) + `.meta` | cắt bằng `slice_sprites.py --manifest Tools/cvm2/slices_map2.json`; kiểm `verify_sprites.py --manifest Tools/cvm2/slices_map2.json [--meta]` (tham số mới, lệnh cũ không đổi) |
| Importer | `CvgpSpriteManifest.FindSlice` đọc thêm `Tools/cvm2/slices_map2.json` | để pivot Custom từ manifest Map 2 (hàm mới `FindMap2Slice`, không đổi hành vi với PNG CVGP) |
| Ảnh đã xem | `ClaudeNotes/cvm2/_cache/shots/01/{new_sprites_2x,crusher_sheet4x,vid_156.3,vid_159.0,tiles_diff}.png` | không commit |

**Kiểm kê chạy lại**: `inv_level2_tiles.py` ⇒ JSON và ảnh chú thích **trùng byte** bản planner (6 vùng, 82 tile 8×8 / 69 metatile 16×16, 6144 px teal ở TowerHallC = 3 ô 32×64 tại cột 352/416/480, hàng 80–143 của vùng ⇒ NES y 112–175).

**Sprite mới — quyết định**
- Bẫy ép: 8 khung, mỗi khung đục thêm đúng 30 px (xích + khối dài ra, khối hạ 8 px/khung). **Đen quanh = vòm hành lang** (video Map_2 154.0–159.0: bẫy treo trong vòm đen; 3 ô teal có viền trái toàn đen) ⇒ key đen trong suốt (nhìn y hệt trên nền đen). Xám (116,116,116) 20 px = đầu gai thật ⇒ GIỮ đục (khác bệ). pivot **[16,0]** (giữa-trên = điểm treo). Nguồn `đo:cvm2-01`.
- Bệ di động `platform` 32×16 = 4 × `brick_0` (kiểm 0 lệch trong `verify_sprites`); xám = khoá trong suốt chỉ trong ô sprite; pivot [16,0] (giữa mép trên = mặt đứng). **`brick_0` = 1 khối bệ di động** (CVGP 05 ghi "chưa rõ công dụng" — đã rõ).
- Chu kỳ/độ hạ từng bẫy, tốc độ bệ: để routine 02/06 đo video.

**Đối chiếu sheet Tilesets** (cột "LEVEL2: MAIN HALL": lưới ô **32×32** bước 33, gốc (408,92), 5 cột × 8 hàng; ô cuối trống; so theo HÌNH — nhãn màu theo thứ tự xuất hiện, bỏ qua màu tuyệt đối; mỗi ô cắt thành 4 phần 16×16 / 16 phần 8×8):

| So | Bản đồ (duy nhất theo hình) | Tilesets | Chung | Chỉ bản đồ | Chỉ Tilesets |
|---|---|---|---|---|---|
| 16×16 | 68 (69 theo màu: 1 cặp chỉ khác màu) | 71 | **64** | 4: TEA(10,1), TEA(1,10), TEB(16,11), HallB(7,11) | 7: ô 1 q(1,0), ô 11 q(0,0)/(1,0), ô 25 q(0,0), ô 27 q(1,1), ô 29 q(0,1)/(1,1) |
| 8×8 | 78 (82 theo màu) | 80 | 75 | 3: TEA(21,3), TEB(32,22), HallB(15,23) | 5 |

Đọc: 64/68 metatile bản đồ có trong Tilesets ⇒ đủ để routine 05 kiểm dựng Tilemap. 4 metatile chỉ-bản-đồ (xem `shots/01/tiles_diff.png`) là biến thể có chi tiết vẽ đè (khối cam lớn viền xanh ở TEB(16,11); mảng đỏ-xám ở TEA/HallB) — **chưa rõ nguồn** (có thể vật thể ghép sẵn trên sheet bản đồ) ⇒ routine 05 kiểm trên ảnh. 7 metatile chỉ-Tilesets (viền/đất, mảng đỏ đậm, phần tượng) có thể là khối chưa xuất hiện trên bản đồ. Script `Tools/cvm2/cmp_tilesets.py` (ghi `_cache/01/cmp_tilesets.json`).

**Kiểm kê sprite Block 2 đã có** (`Assets/Data/Art/…`; AnimSet ở `Assets/Data/Config/Anim/`)

| Nhân vật | Thư mục / file | AnimSet |
|---|---|---|
| Spear Knight (BlackKnight) | `Enemy/BlackKnight/blackknight_0..2` | `BlackKnightAnimSet` |
| Medusa Head | `Enemy/MedusaHead/medusahead_0,1` | `MedusaHeadAnimSet` |
| Bone Tower (DragonSkullCannon) | `Enemy/DragonSkullCannon/dragonskull_{purple,red}_{top,bottom}`, `dragonskull_fireball` | `DragonSkullCannonAnimSet` |
| Ghost | `Enemy/Ghost/ghost_0,1` | `GhostAnimSet` |
| Bat | `Enemy/VampireBat/bat_purple_0..3`, `bat_red_0..2` | `VampireBatAnimSet` |
| Queen Medusa | `Boss/Medusa/medusa_bust_0,1`, `medusa_head`, `medusa_stone`, `snake_0,1` | `MedusaAnimSet` |
| Item | `Item/{cross,cross_spin_0,1,crown_0..3,chest_0..3,roast,double,triple,stopwatch,…}` | `ItemAnimSet` |
| Object | `Object/{door_0,1,brick_0,candle_*,torch_*}` + **mới** `crusher_0..7`, `platform` | `ObjectAnimSet` (chưa có anim crusher/platform — routine 06 thêm) |
| Bản đồ lâu đài | `CastleMap/castlemap_{bg,marker,bat_0..3}` | `CastleMapAnimSet` |

Không thấy thiếu sprite thật ⇒ không có câu hỏi §5 mới.

**Tượng Medusa (cho routine 09)**: `medusa_bust_0` (32×48) **khớp 0 px** tại (96,96) của vùng Medusa (rect [1,11,768,192]) ⇒ tượng vẽ sẵn trên nền ở x 96–127, NES y 128–175, cùng pixel với sprite CVGP. `medusa_bust_1`, `medusa_head`, `medusa_stone` KHÔNG có trên nền (lệch tốt nhất 232/302/332 px) ⇒ là khung sprite riêng. Routine 09: xoá ô tượng trên nền khi thức + vẽ sprite theo video.

### 3c. Routine 02 (16/10/2026) — dữ liệu video Map 2
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Chỉ mục + timeline (viết lại) | `ClaudeNotes/cvm2/ref/MAP2_TIMELINE.md` | §1 hiệu chuẩn, §3 57 dòng + §3b quái + §3c vật phẩm + §3d cơ chế, §4 boss, §5 ảnh, §6 số đo, §7 chuyển cảnh theo tick |
| Bảng theo frame (commit) | `ClaudeNotes/cvm2/ref/data/` | `map2_hud.csv` (frame,t,hud_ok,score,time,stage,hearts,p,pbar,pempty,ebar,eempty,sub,mult), `map2_camera.csv` (kind,section,cx,score,cxw), `map2_simon.csv` (sprite,flip,px,Y,err,world_x,y_sec), `map2_events.txt` (78 sự kiện + nghĩa) + `_raw`, `map2_pickups.txt`, `map2_tracks.csv` + `_class`, `map2_candles.txt`, `boss2_{hud,simon,medusa}.csv`, `*_audio_cues.txt`, `calib_*.txt`, `dup_map2.txt` |
| Bộ đệm lớn (KHÔNG commit) | `ClaudeNotes/cvm2/_cache/02/work/` | `map2_nes.npy` 1.3 GB, `map2_blobs.csv`, `map2_track_pts.csv`, `map2_lut.npz` (bảng màu sheet→video), `boss2_*`; ảnh xem `_cache/02/shots/` |
| Công cụ (commit) | `Tools/cvm2/` | `calib_fine`, `calib_boss`, `extract_nes2`, `dup_frames2`, `hud_read2`, `hud_events2`, `hud_check2`, `camera_track2`, `fg2`, `objects2`, `simon2`, `tracks2`, `classify2`, `track_sheet2`, `event_triplets2`, `pickups2`, `candles2`, `events_annot2`, `platform2`, `boss2_blobs`, `simon_boss2`, `audio_cues2`, `frames_sheet2`, `ref_images2` (+ `nes2.py` thêm `box`, `stderr` im, CALIB mới); **cách chạy: MAP2_TIMELINE §6** |

### 3d. Routine 03 (16/10/2026) — nhạc
**16 bài** (chép nguyên byte từ `Art_Source/music/` — sha256 đọc lại sau khi chép trùng nguồn, KHÔNG sửa nội dung âm thanh):

| # | Tên gốc (`Art_Source/music/`) | Tên mới (`Assets/Data/Audio/Music/`) | sha256 | eMusic |
|---|---|---|---|---|
| 01 | `01. Underground (Name Entry) [Famicom Disk System].mp3` | `m01_underground.mp3` | `1ed97f88d0dc8777686722f16fde9a873da15b99686839b13c9f469b79489314` | Underground |
| 02 | `02. Prologue (Castle Gate).mp3` | `m02_prologue.mp3` | `961f10d99266a9b472bfb0bafe3de3a816ce1a9889aa79f74730a5e90abbf3fa` | Prologue |
| 03 | `03. Vampire Killer (Courtyard).mp3` | `m03_vampire_killer.mp3` | `0f4453be75adc4db0c79f2452bea033a2ee08ee2c51ad93d9b1307a4f2434608` | VampireKiller |
| 04 | `04. Stalker (Armory).mp3` | `m04_stalker.mp3` | `5b3902b0ffdadcde17a6a58589bb850a3f628c99222ae4f1004f2294a9d5b2b5` | Stalker |
| 05 | `05. Wicked Child (Upper Yard).mp3` | `m05_wicked_child.mp3` | `d2548e3ab2b56ae255e5453a8c17349a865f194f8201f9ebf6911b70be8f31a0` | WickedChild |
| 06 | `06. Walking on the Edge (Castle Swamp).mp3` | `m06_walking_on_the_edge.mp3` | `cfbd1cd4d902d93c418b31f0572cb804f8a2991df9a18ca81a48eb4c511ef97e` | WalkingOnTheEdge |
| 07 | `07. Heart of Fire (Prisonhold).mp3` | `m07_heart_of_fire.mp3` | `26c7d15ba6bbe2b828a6ad362de7c75f21900fc6dd8e12573c43a0bd891ea935` | HeartOfFire |
| 08 | `08. Out of Time (Upper Terrace).mp3` | `m08_out_of_time.mp3` | `846f6e6308724cebaa44d6fe1515e55015a9faaff0944f5a2a759d61dc38237b` | OutOfTime |
| 09 | `09. Nothing to Lose (Dracula's Quarters).mp3` | `m09_nothing_to_lose.mp3` | `d71e7a56c1628ef9fe374e335fba7de1293b479c45c52bb77b89d0eddc0e40a1` | NothingToLose |
| 10 | `10. Poison Mind (Boss Theme).mp3` | `m10_poison_mind.mp3` | `a2f1b1d89c8e2ff9c6c43df01688a75118e11ae50710fa0bb99e2c2aa8707cd0` | PoisonMind |
| 11 | `11. Black Night (Final Battle).mp3` | `m11_black_night.mp3` | `d40255c0066f3c144bd97bcd4b6d0f0516bb8509580055a4a0ed6c3d49b32b78` | BlackNight |
| 12 | `12. Stage Clear.mp3` | `m12_stage_clear.mp3` | `c328c0d000d352154b4df7fe2f84b150c68fc34ec0e4bc1d5bafb09337695816` | StageClear |
| 13 | `13. Game Complete.mp3` | `m13_game_complete.mp3` | `a8dbc4c3d7ad745c8089724dbae2b121819efc7e3212f5bde1367e5f8b03f644` | GameComplete |
| 14 | `14. Voyager (Ending).mp3` | `m14_voyager.mp3` | `a1f9898ff6b5c5671ed18b25cd0c871b530db568b66f809d506afbecdc1e3288` | Voyager |
| 15 | `15. Death.mp3` | `m15_death.mp3` | `1c38509058c5bec2d9a8af10e072697d879c7e6944fdb3af926c4b49b5cd0677` | Death |
| 16 | `16. Game Over.mp3` | `m16_game_over.mp3` | `d0fc421596c640caa42467fc0db880b0bb702e94984e47d63bfb467da2ec644e` | GameOver |

| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Preset import | `Assets/Data/Editor/MusicImporter.cs` | `Assets/Data/Audio/Music/**`: stereo, giữ 44.1 kHz, Vorbis 70 %, preload, không nạp nền; bài dài Compressed In Memory (đặt timeSamples chính xác — không Streaming), jingle (m02/m12/m13/m15/m16) Decompress On Load; chỉ đặt lần đầu import |
| Tên bài | `Assets/Data/Script/Audio/eMusic.cs` | giá trị = số file NN |
| Thư viện | `Assets/Data/Script/Audio/MusicLibrary.cs` + asset `Assets/Data/Audio/MusicLibrary.asset` | Entry: id, clip, startSample (2257), loop, loopStart, loopEnd, source |
| Builder | `Assets/Data/Editor/MusicLibraryBuilder.cs` — menu **`CVGP/Build MusicLibrary`** | đọc `Tools/cvm2/music_loops.json`; kiểm id/số/điểm lặp; gán `AudioManager.musicLibrary` trên `GameManager.prefab` nếu trống; chạy lại ⇒ "không đổi" |
| Đo nhạc | `Tools/cvm2/music3.py` (`loop`, `loop2`, `export`, `ident`, `seg`, `pos`), `music3_track.py` (mép cục bộ), `music3_tapcheck.py` (kiểm WAV đầu ra Unity) | chạy từ gốc repo `python -P -E`; cần wav giải mã ở `ClaudeNotes/cvm2/_cache/03/audio/` (lệnh ffmpeg ở §7 routine 03) |
| Dữ liệu (commit) | `Tools/cvm2/music_loops.json` (đầu vào builder), `ClaudeNotes/cvm2/ref/data/music/` (`ident_<video>.csv`, `loops2.json` mẫu ffmpeg, `check_m03/04/10.txt` kiểm lặp, `unity_music_flow_spam.txt` log Unity) | |
| Probe (chỉ Editor) | `Assets/Data/Script/Debug/MusicProbe.cs` (`Start()` / `Stop(path)` — log `AudioManager.MusicEvent` + mẫu mỗi 60 tick + cảnh báo "đứng"), `MusicProbeTap.cs` (`Begin(giây, wav)` — ghi đầu ra thật qua OnAudioFilterRead) | chỉ đọc, không đổi hash |
| Bộ đệm (KHÔNG commit) | `ClaudeNotes/cvm2/_cache/03/` (`audio/` wav, `unity/` mẫu Unity, `loop/` WAV tap, `main*/` `arena*/` kết quả bộ hồi quy, `doc/`), ảnh `_cache/shots/03/` | |

### 3e. Routine 04 (17/10/2026) — lọ nước thánh
| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Probe (chỉ Editor) | `Assets/Data/Script/Debug/HolyPierceProbe.cs` — `RunSuite(ScenarioNames())`, `SuiteReport()`, `Trace(a,b)`, `ShotDir` (chụp 4 khung: lọ vào thân / +6 tick / ra khỏi thân / lửa) | 5 kịch bản `control, enemy, bat, medusa, count3`; input giải mã RLE trong code (không file replay); bộ `holy` của `RegressionRunner` (nằm trong `all`, trước `rules`); trả `world.Stage` cuối bộ |
| Công cụ video | `Tools/cvm2/holy4_sheets.py` (frame lưới NES liên tiếp, boss1/boss2/map2), `holy4_hud.py` (tim/ENEMY đổi), `holy4_pairs.py` (ghép cặp video ↔ ảnh Unity) | chạy từ gốc repo, `python -P -E` |
| Bộ đệm (KHÔNG commit) | `ClaudeNotes/cvm2/_cache/04/` (ảnh zoom, `arena/` `core/` `main/` `run_holy1/` kết quả bộ hồi quy, `quick_*.txt` thử nhanh), ảnh `_cache/shots/04/` (Unity + `pairs_0..3.png`) | |

## §4 Quyết định kỹ thuật đã chốt

### Bảng hash chuẩn (H11 — baseline = CVGP 21, đo lại ở routine 00 ngày 16/10/2026)
Hash đổi chỉ khi routine CỐ Ý đổi hành vi: thêm 1 dòng vào "Lịch sử đổi hash" dưới bảng (cũ → mới, lý do, routine) **và** sửa `RegressionRunner.EXPECTED` cho trùng.

| Bộ | Lệnh (Play đúng scene) | Scene | Hash chuẩn | Đạt/tổng | Routine đặt | Ngày |
|---|---|---|---|---|---|---|
| long (`RegressionProbe.RunLong(2,true)`, 1260 tick) | `RegressionRunner.Start("long", fps, dir)` | Arena (Simon) | `5963c4ba` | 1/1 (chết 1 = rơi hố, baseline) | CVGP 11/21 → CVM2 00 | 16/10 |
| simon (`SimonProbe`) | `…Start("simon",…)` | Arena (Simon) | `b2954cbe` | 20/20 | CVGP 08/21 → 00 | 16/10 |
| attack (`AttackProbe`) | `…Start("attack",…)` | Arena (Simon) | `eee2cdfb` | 21/21 | CVGP 09/21 → 00 | 16/10 |
| item (`ItemProbe`) | `…Start("item",…)` | Arena (Simon) | `7ca5bf56` | 14/14 | CVGP 11/21 → 00 | 16/10 |
| enemy (`EnemyProbe`, Block 1) | `…Start("enemy",…)` | Arena (Simon) | `f288b0ce` | 20/20 | CVGP 13/21 → 00 | 16/10 |
| boss (`BossProbe`, Phantom Bat) | `…Start("boss",…)` | Arena (Simon) | `31e6d711` | 12/12 | CVGP 14/21 → 00 | 16/10 |
| b23 (`EnemyB23Probe`) | `…Start("b23",…)` | Arena (Simon) | `171be0c1` | 31/31 | CVGP 17/21 → 00 | 16/10 |
| b46 (`EnemyB46Probe`) | `…Start("b46",…)` | Arena (Simon) | `58a8edf9` | 26/26 | CVGP 18/21 → 00 | 16/10 |
| b24 (`BossB24Probe`) | `…Start("b24",…)` | Arena (Simon) | `7adfbc54` | 16/16 | CVGP 19/21 → 00 | 16/10 |
| b56 (`BossB56Probe`) | `…Start("b56",…)` | Arena (Simon) | `6cb8a1ad` | 14/14 | CVGP 20/21 → 00 | 16/10 |
| rules (`RulesProbe`, ScenarioNames có `tally` 2 lần) | `…Start("rules",…)` | Arena (Simon) | `03fafb24` | 68/68 | CVGP 12/21 → 00 | 16/10 |
| gallery (`CharacterAcceptProbe.RunGallery`, không chụp) | `…Start("gallery",…)` (chạy 240 fps) | Arena (Simon) | `517442ed` | 75/75 | CVGP 21 → 00 | 16/10 |
| core (`CvgpCoreProbe`, 21 kịch bản) | `…Start("core",…)` | Arena, `CVGP.ArenaUseDummy` = true (Player = ArenaDummy) | `76516076` | 21/21 | CVGP 07/21 → 00 | 16/10 |
| flow (luồng THẬT Title → STAGE 04, 60 fps) | `…Start("flow",…)` | Main | `e0b2d54c` | 1/1 + 0 lỗi console | CVGP 15/21 → 00 | 16/10 |
| flow30 / flow144 (tất định đa fps) | `…Start("flow30,flow144",…)` | Main | `e0b2d54c` (cả hai) | 1/1 | CVGP 21 → 00 | 16/10 |
| h8 (5 mốc luồng CVTI, đọc từ flow) | tự chạy kèm mọi `flow*` | Main | Start **L352** · đen **L432** · Intro **L448** · đen **L890** · Stage **L922** | 5/5 | CVTI → 00 | 16/10 |
| spam1..4 (spam phím 4 lượt CE/CA/PB/CB) | `…Start("spam",…)` | Main | *(không hash)* — đạt = 0 lỗi console; CB: GAME OVER tick 171 → CONTINUE 204 | 4/4 | CVGP 16/21 → 00 | 16/10 |
| holy (`HolyPierceProbe`, 5 kịch bản — **bộ MỚI**) | `…Start("holy",…)` (nằm trong `all`) | Arena (Simon) | `52c59864` | 5/5 | CVM2 04 | 17/10 |

Mốc routine 11 (luồng Title → STAGE 07) sẽ thêm dòng hash mới; **ảnh H8** L10/L600 0 px so `ClaudeNotes/cvgp/_cache/shots/00/base_*` (đo lại 16/10: **0 px** ở cả 60/30/144 fps); L930 so `base_L930` KHÔNG so được (baseline chụp lúc Simon chưa chạy lộ trình) — so với ảnh routine 21 `cvgp/_cache/shots/21/flow60/h8_L930.png` ⇒ **0 px**.

**Lịch sử đổi hash:** *(ghi 1 dòng/hash: cũ → mới, lý do, routine)*
- (04, 17/10) **không hash cũ nào đổi** — lọ nước thánh vốn đã xuyên trong Unity (`pierces = 1` từ CVGP 04); routine chỉ sửa comment + chuỗi nguồn Config. **Thêm** dòng `holy` = `52c59864` (bộ mới, lặp 2 lần y hệt — có/không chụp ảnh).

### Bộ chạy hồi quy (`RegressionRunner`, routine 00)
Chỉ Editor, đang Play. Chạy nối tiếp các probe qua `EditorApplication.update`, ghi `<dir>/suites/<bộ>.txt`, `SUMMARY.txt` (bộ | đạt/tổng | hash | hash chuẩn | khớp? | giây), `progress.txt`, `ALL.done` (`OK` / `OK (bỏ qua n)` / `LỆCH n` / `ABORT …`). Dùng `execute_code` (codedom — `Start` trả chuỗi, không cần chờ trong lệnh); chờ bằng vòng lặp `[ -f <dir>/suites/ALL.done ]`.

| Lệnh | Scene / điều kiện | Bộ chạy | Thời gian đo (16/10) |
|---|---|---|---|
| `RegressionRunner.Start("all", 60, "<abs dir>")` | Arena, Player = Simon (`CVGP.ArenaUseDummy` = false), Play, `runInBackground = true` | long, simon, attack, item, enemy, boss, b23, b46, b24, b56, **holy** (CVM2 04), rules | **1047 s ≈ 17.5 phút** (mọi fps cho cùng thời gian: ~60 tick/s; CVM2 04: 1058 s — holy 11 s) |
| `RegressionRunner.Start("full", 60, "<dir>")` | như trên | all + gallery (240 fps tự đặt, không chụp) | 1527 s ≈ 25.5 phút |
| `RegressionRunner.Start("core", 60, "<dir>")` | Arena với **EditorPrefs `CVGP.ArenaUseDummy` = true** (đặt TRƯỚC khi vào Play, mở lại scene Arena) — Play mới | core | 92 s |
| `RegressionRunner.Start("flow,spam", 60, "<dir>")` hoặc `"main"` (flow + spam1..4); thêm `flow30,flow144` | Main.unity, Play | flow (+ h8), spam1..4 | flow 157 s ×3, spam 60–80 s ×4 ⇒ flow+spam ≈ 7.5 phút; cả bốn flow/spam + flow30/144 = 752 s |
| `RegressionRunner.Status()` / `Abort()` | | tiến độ / dừng | |
Quy ước: **3 lệnh / 3 phiên Play** cho đủ bộ (Arena-Simon · Arena-Dummy · Main) — đổi người chơi Arena và đổi scene bắt buộc Play mới (`ArenaPlayerSelect.Awake`). Tổng ≈ 17.5 + 1.5 + 7.5 ≈ **27 phút** không gallery. Bộ nào không hợp scene hiện tại ⇒ dòng `SKIP` (không tính lệch, nhưng `ALL.done` ghi `OK (bỏ qua n)` — **đọc kỹ trước khi báo "đạt"**). Thêm probe CVM2 mới = thêm 1 `case` trong `Make()` + 1 dòng `EXPECTED` + 1 dòng bảng trên. Cuối phiên: thoát Play, `CVGP.ArenaUseDummy` = false, EditorSettings = HEAD (CRLF).
- **fps**: tham số `fps` đặt `targetFrameRate` cho cả bộ (vSync 0, `runInBackground` true; trả lại khi xong). Hash tất định theo fps (đã kiểm `all` ở 240 và 60 fps ⇒ y hệt; flow ở 30/60/144 ⇒ `e0b2d54c`).
- **gallery** mặc định KHÔNG nằm trong `all` (8 phút); không chụp ảnh (hash vẫn so được).

### Sửa harness gộp `cvgp-fix-05` (routine 00) — KHÔNG đổi gameplay, mọi hash giữ nguyên
(a) `RulesProbe.RunSuite` giờ **trả lại** trạng thái cuối bộ: gỡ `StageRules` do chính nó thêm (`DestroyImmediate`), trả `Simon.AutoRespawn`, gỡ sự kiện `LifeLost/Respawned/TallyDone/OneUp` ⇒ chạy cùng phiên với bộ khác được. (b) `BossB24Probe.Cleanup`: `Destroy(rules)` → `DestroyImmediate` (như B56). (c) `Map1AcceptProbe.StartSpam(stage, ticks, seed, withStart, warpSection = null, warpX = 0, warpY = 0, lives = 0, hp = 0)` — lượt PB = `lives 1, hp 2`, CB = `Warp CryptB (240,96) + lives 1` đưa vào tham số (mặc định = cách gọi cũ). (d) **Nguồn "RunLong sau RulesProbe ra hash khác"** = đúng cái (a): `StageRules` còn gắn vào `StageWorld` + `Simon.AutoRespawn = false` + sự kiện còn đăng ký ⇒ sau khi sửa (a), chạy `rules,long,rules,long` cùng phiên: long `5963c4ba` ×2, rules `03fafb24` ×2 (không phụ thuộc thứ tự). Bộ chạy vẫn đặt **long ĐẦU, rules CUỐI** theo khuôn routine 21 (ghi lý do: rò chỗ khác trong tương lai).
(e) **Rò thứ hai tìm thấy khi chạy `long` → `rules` cùng phiên**: `RulesProbe.Damage` sinh `ArenaTarget` từ pool nhưng pool giữ `touchDamage = false` do `RegressionProbe`/`AttackProbe` đặt ⇒ "chạm bia mất 2 vạch" FAIL 7 ô (hash `a7b09b08`). Sửa 1 dòng: bia vừa sinh `touchDamage = true` (probe chuẩn đã kỳ vọng mặc định prefab). Gốc ở `ArenaTarget.OnSpawned` không reset `touchDamage` — để nguyên (H9), ghi §6.
(f) `RegressionProbe` pass = đủ tick, **không** đòi 0 lần chết (replay dài CÓ 1 lần rơi hố — baseline).

### Khác
- **(02) Hiệu chuẩn Map_2/boss**: dùng bộ số mới của routine 02 (§2c), không dùng số planner; frame hiệu chuẩn = 10 frame sạch (mỗi vùng ≥ 1), cx tinh ±2 mỗi frame; boss so gradient (bảng màu khác). **Quy ước lấy mẫu**: chỉ số pixel video = round(ox + sx·(x+½)) — khác `nes_common` Map 1 (round(c−½)): ox của 2 bộ chênh +0.5.
- **(02) Trừ nền sheet** cần đổi màu sheet→video (`fg2.lut()`, 12 màu; video Map_2 tối/khác sheet) + min trên dịch ±1 px; sau đó mới thấy sprite sạch. Nhãn quái bằng IoU sprite chỉ ĐÁNG TIN khi xem thumbnail (bat treo bị nhãn candle…) — dùng làm gợi ý, quyết bằng mắt.
- **(02) Đọc HUD**: dùng lại mẫu số Map 1 (cùng font); nhãn vũ khí phụ bằng mẫu pixel từ frame sạch (rìu f100, thập tự f1000, **đồng hồ f7000**); nhãn `occ` = sprite khác (thập tự đang bay) đè ô — bỏ qua.
- **(02) Nghĩa số liệu video ↔ wiki**: điểm quái khớp wiki hết (Bat 200, MH 300, Ghost 300, SK 400, BT 400); **stopwatch: video 186–196 tick vs wiki 5 s ⇒ theo video (H1) — chưa áp vào game** (§6); "nước thánh" của planner ở Map_2 là đồng hồ.
- **H8**: chạy kèm mọi `flow*` — đọc 5 mốc từ `Map1AcceptProbe.Events` (`Title: Start`, `Title→Black`, `Black→Intro`, `Intro→Black`, `Black→Stage1`) ⇒ `suites/h8_<flow>.txt`.
- Play Mode an toàn (Odin MemoryStream crash): `EditorSettings.enterPlayModeOptionsEnabled = true` + `DisableDomainReload`; routine 00 chạy 5 phiên Play + 4 lần biên dịch không crash.

- **(03) Nhạc theo tên**: `AudioManager.PlayMusic(eMusic)` (thư viện `MusicLibrary` gán trên `GameManager.prefab`); API cũ `PlayMusic(AudioClip, bool)` giữ nguyên. Màn hình giữ **id** (trường mới `musicId` / `stageClearId` / `deathId` — tên MỚI để không đọc nhầm giá trị `{fileID: 0}` cũ của trường AudioClip trong prefab; trường cũ còn trong YAML prefab, Unity bỏ qua). Nhạc stage đọc `ConfigStage.music` theo `GameSession.stage`, nhạc boss đọc `ConfigBlock.bossMusic` (stage → block) ⇒ Map 2 + các block sau chỉ cần dữ liệu.
- **(03) Lặp theo mẫu bằng `AudioManager.Update`** — NGOẠI LỆ hợp lệ của H4: chỉ âm thanh (đọc/đặt `timeSamples`), không đụng trạng thái gameplay hay hash. Vượt loopEnd ⇒ lùi đúng (loopEnd − loopStart): đoạn sau loopEnd trong file là vòng 2 (cùng nội dung) nên trễ ≤ 1 khung không nghe ra (đo: vượt 8 ms, vị trí sau nhảy ±1 mẫu). Không dùng PlayScheduled 2 nguồn (Pause của StageRules sẽ phá lịch DSP). Bài lặp có loopEnd ⇒ `AudioSource.loop = false`; bài không lặp phát 1 lần, `MusicEvent("end")` khi hết.
- **(03) Phát từ mẫu 2257** (bỏ độ trễ bộ mã MP3 mà Unity giữ) ⇒ tiếng đầu trùng mốc video (≤ 1 ms); điểm lặp lưu theo mẫu Unity = mẫu ffmpeg + 2257.
- **(03) Hash luồng không chứa âm thanh**: `Map1AcceptProbe`/`Map1RouteProbe` băm trạng thái game (màn, session, thực thể, FSM) — không băm log/âm thanh ⇒ không cần tách; mọi hash giữ nguyên (§7 routine 03).
- **(03) Mốc nhạc chốt**: Intro — Prologue ở tick bật Intro (video sớm 4 tick = 0.07 s; không dời để H8/CVTI §4.3 nguyên vẹn). Stage — `PlayStageMusic` ở `StageScreen.OnEnter` + hồi sinh, KHÔNG phát lại ở cổng/cửa/đổi stage (video Map_1 + Map_2). Boss — Phantom Bat giữ quy tắc CVGP (lao đầu); Medusa: số đo cho routine 09 (§2f #7). Nhặt cầu — Stage Clear cùng tick (4 video). Bản đồ lâu đài/tally — im (video: không bài nào). Stage4Stub (điểm cuối luồng Map 1) — Stalker (video Map_2 0:06.18 = STAGE 04 hiện; trường có sẵn từ CVGP 15 — KHÔNG phải luồng Map 2).
- **(03) Death phát lúc Simon BẮT ĐẦU chết** (sự kiện tĩnh mới `SimonController.DyingStarted`, bắn ở `OnEnterState` của Dead/FallDeath — chỉ báo, không đổi trạng thái): suy luận — DeathTicks 124 vốn lấy theo độ dài bài Death 2.07 s ⇒ bài phủ cả cảnh chết; trước đây phát ở `LifeLost` (SAU cảnh chết) và bị màn GAME OVER cắt sau 32 tick (spam4). Không video Simon chết ⇒ ghi suy luận.
- **(03) Chính sách SFX ↔ nhạc (NES dùng chung kênh — KHÔNG giả lập kênh)**: nhặt cầu ⇒ thay bằng Stage Clear; chết ⇒ thay bằng Death; Pause ⇒ tạm dừng (CVGP 12); **dùng đồng hồ ⇒ video Map_2 cho thấy Stalker đứng 2.40 s rồi chạy tiếp** (chưa dựng — §6); các SFX khác (roi, tim, rìu…) chỉ chồng lên nhạc (video: RMS tụt ngắn 0.1–0.3 s ở 9 chỗ — SFX ăn kênh, nhạc không dừng/không lệch gốc bài).
- **(04) Lọ nước thánh = XUYÊN + 1 đòn/mục tiêu, vỡ chỉ ở sàn** (video §2g; trước đây ghi "suy luận" nhưng code đã làm vậy): `ConfigSubWeapon.HolyWater.pierces = 1` (nguồn đổi thành video) ⇒ `SubWeaponProjectile.OnHitLanded` không gọi `OnSpent`; `Hitbox.singleHit` ⇒ mỗi lọ đánh 1 mục tiêu tối đa 1 lần; boss khựng suốt lúc lọ chạm (`BossBase.TouchingHolyWater`, hộp lọ 8×8 ∩ hộp nhận đòn). **Không sửa logic** (H9 — tối thiểu): `HolyWater.OnSpent` (vỡ ngay dưới chỗ trúng) giữ làm nhánh cho `pierces = 0`, ghi rõ "không dùng". Hộp nhận đòn Medusa (32×32 Unity) có thể lớn hơn NES (boss2 f137–143 lọ chồng rìa không trừ máu) — routine 09 đo lại hộp.
- **(04) Bộ chạy**: `holy` thêm vào `all` NGAY TRƯỚC `rules` (probe tự trả `world.Stage` + gỡ phòng boss/BossArena bằng `DestroyImmediate`); `rules` vẫn `03fafb24` sau `holy`.
- **(03) Ô nguồn nhạc Config**: 24 ô (6 `ConfigBlock.bossMusic` + 18 `ConfigStage.music`) đổi kiểu **chuỗi → `eMusic`** (importer validate tên enum); nguồn: video (Block 1–2 boss, Stage 1–7) hoặc "suy luận … chưa có video" (Block 3–6 boss, Stage 8–18) — lý do giữ ghi trong ô.

## §5 Câu hỏi cho người dùng (chép từ `CVM2_PLAN §9` — đừng tự quyết thay)
1. **`cvgp-fix-03` (Simon knockback/nước thánh/mép rơi) + `cvgp-fix-04` (báo #3, Phantom Bat)** — chỉ Map 1, chưa tạo; người dùng duyệt thì tạo sau (routine 09/12 ghi thêm mẫu đo từ Map 2 nếu có).
2. (vẫn mở từ CVGP) sheet HUD bản US (chữ số TẠM) — `CVGP_MEMORY §5 #39a`; bấm thử gamepad Flydigi — `#39b`.
3. Bẫy ép gai: chạm = mất 2 vạch + knockback hay chết ngay? Video có thể không cho thấy (Simon né được) ⇒ routine 06 chọn theo bằng chứng có được, ghi `suy luận`, hỏi lại ở đây.
4. Cụm sau: **Map 3 (Block 3, Stage 7–9, boss Mummy Men)** cần sheet `NES - Castlevania - Stages - Level 3_ ...` (spriters-resource — tải tay) + video `Map_3.mp4` / `Boss_Map_3_Behaviour.mp4`. Sheet Tilesets đã có bảng tile Level 3–6 nhưng KHÔNG thay được sheet bản đồ.
5. Kết thúc game (credits/ending/Second Quest) — vẫn chờ (`CVGP_MEMORY §5 #60`).
6. (03) **Nhạc boss Phantom Bat bắt đầu lúc nào?** Video Map_1: Poison Mind 3:18.80 — 0.67 s TRƯỚC khi dơi đổi sprite thức (1.40 s sau khi camera khoá); video boss: 0.61 s SAU khi thức (= lúc lao đầu). Đang giữ quy tắc CVGP (lúc lao đầu). Nếu người dùng có thêm 1 clip vào phòng Phantom Bat (không ném vũ khí trước khi dơi thức) thì đo lại được.
7. (03) **Dùng đồng hồ có làm nhạc dừng không?** Map_2 2:40.97: Stalker đứng 2.40 s rồi chạy tiếp; nhưng 3 lần dùng trong trận Medusa không thấy Poison Mind đứng. Chưa dựng — cần thêm 1 clip dùng đồng hồ (ngoài trận boss) để chốt.

## §6 Nợ & việc ngoài phạm vi
- (00) `ArenaTarget.OnSpawned` không đặt lại `touchDamage` (bia từ pool mang cờ của probe trước); đã vá phía `RulesProbe`. Probe mới dùng `ArenaTarget` phải tự đặt `touchDamage` sau khi `Spawn`.
- (00) `BossProbe.Cleanup` vẫn `Destroy(rules)` (không `DestroyImmediate`) — chưa gây lệch hash (kịch bản kế chạy khung sau); nếu thêm kịch bản gắn `StageRules` liền kề thì đổi như B24/B56.
- (00) Bộ chạy `core`/`flow` cần phiên Play riêng (đổi `CVGP.ArenaUseDummy` / scene) ⇒ tối thiểu 3 lệnh; chưa tự động hoá đổi Play (rủi ro mất focus MCP + Odin crash). Có thể làm ở routine 07 nếu cần.
- (00) `Map1AcceptProbe` chỉ hiểu stage 1–3 (`Clamp(stage,1,3)` trong `StartSpam`) — luồng Stage 04–06 (routine 11) cần probe Map 2 riêng.
- (00) HUD "STAGE 05" đổi ~1 s SAU khi Hall A hiện (xem §2b) — routine 02 xác nhận, routine 11 dựng.
- (00) Ảnh H8 L930 không có baseline đúng (xem §4) — dùng ảnh routine 21.

- (02) **Đồng hồ (stopwatch) trong game có thể dùng 5 s (wiki) — video đo 186–196 tick (~3.1–3.3 s)**: routine 08/12 kiểm `ConfigItem`/Stopwatch của CVGP và ghi nguồn video nếu khác; H13 (số dùng chung) ⇒ chạy lại probe enemy/boss + `Export Stats Report`.
- (02) **Cuộn cửa Map_2 = 1.00 px/tick × 128 tick/đoạn; CVGP Map 1 dùng 0.90–0.91 px/tick (140 tick)**: routine 11 chọn theo video Map_2 cho cửa trái (nếu lõi `StageFlow` cửa dùng chung ⇒ tham số hoá theo stage); có thể chính số Map 1 là sai do nhịp capture 21 fps (hồi quy 128 px nhưng chưa đo bằng khớp mẫu) — nên đo lại Map_1 bằng `camera_track` mới nếu muốn thống nhất.
- (02) **STAGE HUD đổi cuối cuộn 2** (≈ 4 tick sau camera dừng) — routine 11 dựng đúng; ghi chú routine 00 ("đổi ~1 s sau khi section hiện") đã giải thích (§2c, MAP2_TIMELINE §7).
- (02) **TIME**: đứng suốt cửa; sau thang đổi tầng giảm lại **40–41 frame (80 tick) sau đầu màn đen** (3/3); sau STAGE đổi ở cửa +26 frame — routine 11 chọn cách dựng (đặt lại pha như CVGP 12 "pha đặt lại khi vào section" hay giữ nguyên).
- (02) **Bệ di động / bẫy ép chỉ đo thô** (`platform2.py` chỉ bắt 321 frame vì viền kem; bẫy ép đo bằng blob đỏ 3 chu kỳ) — routine 06 phải khớp mẫu từng frame (viền kem bệ 32×8; thân bẫy 32 rộng).
- (02) **Thịt nướng Hall B (1:57)** và **nguồn của khối gạch/nến cụ thể** chưa rõ nguồn rơi (xuất hiện trước khi vào khung) — routine 10 xem lại bằng ảnh `ev/*.png` + `candles2.py`.
- (02) **Nhạc**: stage 04–06 phát liên tục 6.0 s → 3:56.0, không thấy đổi nhạc khi vào phòng boss (RMS/phổ) — routine 03 phải tương quan chéo (mốc im lặng ở `ref/data/map2_audio_cues.txt`: 0:05.85–6.15 đen, 2:42.80–2:43.45 lặng 0.65 s lạ, 3:56.10–3:56.80 → Stage Clear, 4:11.30–4:16.25 lặng 4.95 s).
  → **(03) đã giải**: Stalker liền 0:06.18 → 3:47.8; Poison Mind ≈ 3:48.1 (Medusa thức + 0.77 s, bị SFX đồng hồ che nên phổ trung bình không thấy); lặng 2:42.80–2:43.45 = nhạc đứng khi dùng đồng hồ; "3:56.8 Stage Clear" thực ra là Poison Mind tiếp tục, Stage Clear bắt đầu 3:58.83 = nhặt cầu; bản đồ B1/B2 KHÔNG có nhạc (âm nhỏ = SFX bước) — §2d.
- (03) **Đồng hồ dừng nhạc 2.40 s** (Map_2 2:40.97, 1 lần đo; trong trận boss 3 lần không thấy) — chưa dựng (chính sách SFX↔nhạc, H9). Routine 08/11 (hoặc fix sau) nếu muốn: `ItemEffects` stopwatch ⇒ `AudioManager.PauseMusic(true)` 144 tick rồi `false` (tick-driven) — cần thêm bằng chứng (§5 #7).
- (03) **Medusa: `QueenMedusa` gọi `StartBossMusic` lúc hết Emerge (32 tick)**; video: Poison Mind = Medusa hiện + 46–57 tick ⇒ routine 09 dời (đọc §2f #7).
- (03) **Phantom Bat**: video Map_1 vs video boss lệch nhau ~1.3 s về mốc nhạc boss (§2d, §5 #6) — giữ quy tắc CVGP.
- (03) Bài 05 Wicked Child / 09 Nothing to Lose: bản rip chỉ có 1 vòng rồi fade ⇒ điểm nhảy nằm ở đầu fade (gain 0.49 / 0.71 — nghe tụt âm lượng khi lặp). Cụm Map 3 / Block 6 cần: (a) bản rip 2 vòng (người dùng cung cấp), hoặc (b) chấp nhận + bù gain (không sửa file — H2; có thể nhân `volume` theo đoạn — chưa làm).
- (03) Prefab màn hình (`IntroScreen`, `GameOverScreen`, `Stage4Stub`, `Stage1Stub`, `StageScreen`) còn trường YAML cũ `music`/`stageMusic`/`stageClearMusic`/`deathMusic: {fileID: 0}` (Unity bỏ qua) — sẽ tự mất khi prefab được lưu lại lần sau; không lưu lại ở routine 03 để diff gọn.
- (03) Video Map_1 bị cắt ≈ 6.25 s quanh 1:48 (trang YouTube chen vào — gốc bài Vampire Killer nhảy 91.71 → 97.96 không đúng 1 vòng): mốc thời gian Map_1 sau 1:48 so với game thật lệch hằng số đó (đã biết từ CVGP — ghi lại bằng chứng âm thanh).
- (02) **Simon Medusa 71 %**: thiếu 4:01.7–4:12.8 (tư thế thắng không có trong bộ sprite `simon_fit`) — không ảnh hưởng dựng.
- (04) **Hộp nhận đòn Queen Medusa** (Unity 32×32 quanh tâm) vs video boss2: lọ chồng rìa thân f137–143 không trừ máu (có thể hộp NES ≈ mặt 16×16, hoặc NES dò va chạm vũ khí–địch cách frame) — routine 09 khớp hộp theo video (H1), chạy lại `holy`/`b24`.
- (04) Ghi chú sai của CVGP về nước thánh đã hiệu đính "(CVM2 04)" trong `CVGP_MEMORY` (số đo Phantom Bat, luật 09, quyết định 14, §5 #15d/#31, §6); `ACCEPT21.md` (❌ "lọ nước thánh chưa xuyên") giữ nguyên là lịch sử nghiệm thu CVGP — routine 12 ghi kết quả mới.
- (02) Ảnh `ref/nes` 36 ảnh planner dùng hiệu chuẩn cũ (lệch ≤ 0.3 px NES); nếu cần so chính xác dùng `Tools/cvm2/ref_images2.py` để xuất lại.

## §7 Nhật ký nghiệm thu

### Routine 00 — 16/10/2026 — Hạ tầng: sổ, commit đồ planner, bộ chạy hồi quy 1 lệnh, baseline
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `Tools/cvgp/__pycache__/`, `UserSettings/`, submodule ` m`); `.gitignore` đã có dòng `ClaudeNotes/cvm2/_cache/`; cổng CVGP 21 = XONG (commit `*add task "CVGP 21`). MCP `Castlevania_Original-main@5669a9bbfcade130` (dataPath đúng).
- **Kiểm mẫu số planner**: `calib_map2.py` ⇒ sx 3.620 / sy 3.100 / ox 269.12 / oy −19.6, ZNCC 0.9386 — lệch planner ≤ 0.005 / ≤ 0.2 px NES (§2a). Trích lại 8 mốc `MAP2_TIMELINE §3` bằng ffmpeg `-ss` (ảnh ghép `ClaudeNotes/cvm2/_cache/shots/00/milestones.png` — **đã mở xem**): 0:06.3 STAGE 04 TIME 0400 TEA, rìu + II ✓ · 0:28.9 TEB TIME 0380, Spear Knight tím + thang ✓ · 0:52.2 cửa đỏ mép trái TEB, Simon đứng cạnh ✓ · 0:57.8 Hall A (vòm, Simon giữa) ✓ nhưng **HUD còn "STAGE 04" TIME 0359** (đổi 05 ở ~58.75 s — `hud_switch.png`, §2b) · 2:28.4 STAGE 05 TIME 0281 sảnh Hall B/C (planner ghi 2:28.4 = STAGE 06 — thực tế khung này HUD "STAGE 05" cạnh cửa; STAGE 06 xuất hiện ở khung 2:38) · 2:38 STAGE 06 TIME 0273 Hall C, 2 bẫy ép gai treo trên trần ✓ · 3:48 STAGE 06 TIME 0211 sảnh Medusa, cửa sổ màu, tượng Medusa + Simon + Medusa Head ✓ · 4:16 STAGE 07 bản đồ lâu đài Block 2, tim 05, ENEMY rỗng ✓. ⇒ 7/8 đúng; mốc "2:28.4 STAGE 06" của planner lệch (HUD đổi 06 muộn hơn ~vài giây, giống quy luật "HUD đổi sau") — routine 02 đo lại.
- **Công cụ**: `RegressionRunner.cs` (~480 dòng) + 3 sửa harness (§4 gộp fix-05, (a)–(f)). Biên dịch sạch (console chỉ lỗi nền Odin/Unity.Mathematics/GUIClips).
- **Baseline đo (mọi hash = CVGP 21)**: Arena-Simon `full` @240 fps: long `5963c4ba` 1/1 · simon `b2954cbe` 20/20 · attack `eee2cdfb` 21/21 · item `7ca5bf56` 14/14 · enemy `f288b0ce` 20/20 · boss `31e6d711` 12/12 · b23 `171be0c1` 31/31 · b46 `58a8edf9` 26/26 · b24 `7adfbc54` 16/16 · b56 `6cb8a1ad` 14/14 · rules `03fafb24` 68/68 · gallery `517442ed` 75/75 — **1527 s, 0 lệch**; lặp `all` @60 fps: **y hệt** 1047 s; thứ tự `rules,long,rules,long` y hệt. `core` (ArenaDummy) `76516076` 21/21 (92 s). Main: `flow` `e0b2d54c` ở 60/30/144 fps (checkpoint 18 giá trị trùng routine 21, GameObject 379), **h8 5/5** (352/432/448/890/922) ×3, spam1..4 0 lỗi console (CB: GAME OVER 171 → CONTINUE 204 = routine 21). Ảnh H8 L10/L600 **0 px** so baseline ở cả 3 fps (`h8_montage.png` — đã xem: Title, Intro, Stage 01); L930 = ảnh routine 21 (0 px).
- **Lệch / xấp xỉ**: không có hash lệch. `base_L930` không so được (đã ghi). Số `all` 17.5 phút + core 1.5 + main 7.5 ≈ 27 phút (≤ 30). gallery 8 phút (routine 21 đo 3.5 phút — máy bận hơn).
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 00**: [x] đồ planner commit đủ (48 file; `_cache/` không vào git; kiểm mẫu planner ghi §2a/§7) · [x] `CVM2_MEMORY.md` đủ §1–§7, Bảng hash chuẩn 17 dòng · [x] bộ chạy chạy hết không thao tác giữa chừng trong từng phiên (3 phiên Play: Arena-Simon / Arena-Dummy / Main — ghi rõ), SUMMARY đã đọc · [x] mọi hash = CVGP 21, H8 khớp, console sạch (chỉ lỗi nền).

### Routine 01 — 16/10/2026 — Nhập sheet Level 2 + Tilesets, kiểm kê, sprite bẫy ép + bệ
- **Bước 0**: cổng 00 = XONG; `main`, nhiễu nền giữ nguyên. MCP `Castlevania_Original-main@5669a9bbfcade130` (set bằng Name@hash — chỉ truyền tên thì lỗi), dataPath đúng. Không vào Play (không đụng gameplay ⇒ không chạy bộ hồi quy; hash không đổi).
- Sheet: sha256 trong Downloads khớp planner; chép nguyên byte, đọc lại sha khớp; `SOURCES.md` +2 dòng.
- Kiểm kê: JSON + ảnh chú thích trùng byte planner. Tilesets: bảng ở §3b.
- Sprite: cắt 2 lần ⇒ md5 y hệt từng file (crusher_0 `b192b0d3…`, platform `5a35e692…`); `verify_sprites.py` CVGP vẫn 0 lỗi (246 PNG, 371/371 ô); chế độ `--manifest` mới 0 lỗi + `--meta` 0 lỗi sau nhập lại (lần nhập đầu dùng importer chưa biên dịch ⇒ pivot sai; đã xoá 9 `.meta` mới và nhập lại). Console chỉ lỗi nền (Odin path1, Unity.Mathematics, GUIClips). Ảnh `new_sprites_2x.png` đã xem.
- Sửa ngoài sprite (H9, nhỏ): `CvgpSpriteManifest.FindSlice` (+~25 dòng) để importer dùng pivot manifest Map 2; `verify_sprites.py` thêm `--manifest` (lệnh CVGP không đổi). Không đổi file Block 1/gameplay.
- **Nghiệm thu routine 01**: [x] 2 sheet sha256 khớp + SOURCES · [x] kiểm kê khớp, bảng Tilesets §3b · [x] 9 sprite md5 lặp y hệt, verify 0 lỗi, ảnh đã xem, quyết định đen/pivot có bằng chứng video (frame 156.3/159.0) · [x] git diff chỉ thêm (trừ 2 sửa công cụ/importer nêu trên).

### Routine 02 — 16/10/2026 — Hiệu chuẩn tinh 2 video, HUD từng frame, Simon + camera, timeline + sự kiện theo tick
- **Bước 0**: `main`; nhiễu nền giữ nguyên; cổng 00 + 01 = XONG (commit `*add task "CVM2 01`). Không đụng Unity (routine chỉ đo video bằng ffmpeg + python ⇒ không vào Play, không chạy bộ hồi quy; mọi hash giữ nguyên Bảng hash chuẩn; không ghi `.cs`). Không cần MCP.
- **Hiệu chuẩn** (`calib_fine.py`, `calib_boss.py`): Map_2 ZNCC 0.9468 (10 frame; cùng 12 frame có quái 0.915 — ghi cả hai), boss gradient 0.955 (7 frame); độ nhạy + hàng nhìn thấy + watermark (§1 MAP2_TIMELINE). Cập nhật `Tools/cvm2/nes2.py CALIB`.
- **Bộ đệm + HUD**: `extract_nes2` (7814 frame 55 s; boss 5 s), `hud_read2`: 7705/7814 frame có HUD (109 = đen/nháy/ sprite đè), 3 frame có `?`; **20 frame ngẫu nhiên (seed 7) kiểm mắt: 0 sai** (`_cache/02/shots/hud_check_20.png`); sự kiện 78 + nghĩa (đã xem 7 ảnh bộ ba `ev_00..06.png`).
- **Phát hiện chính**: (1) vũ khí HUD từ 2:09.23 là **đồng hồ** không phải nước thánh (khớp mẫu icon với Item/stopwatch + hearts −5 ×4); (2) số STAGE đổi ngay sau cuộn 2 của cửa; (3) 2 lần rosary (3:09.2, 3:30.1) giải "other ngắn"; (4) dao găm 1:08 = item không nhặt; (5) III Hall C không nhặt; (6) vương miện mọc từ sàn 0:17.0 sau khi quất tường phải TEA; (7) 3 lần chuyển tầng đều là thang lên-phải wrap mép trên→mép dưới; (8) video kết thúc bằng 8 frame **STAGE 07 TIME 0500** (Block 3); (9) PLAYER 16/16 cả Map_2.
- **Camera/Simon**: camera 100 %; Simon khớp mẫu 6829/7347 (92.9 %), err trung vị 0.038 (`simon_fit` palette tự hiệu chuẩn (84,30,5)/(219,157,56)/(242,215,163)); 12 overlay đã xem (`simon_fit_check.png`).
- **Đồng hồ + cơ chế**: stopwatch đứng 186–196 tick (đo bằng bẫy ép/Medusa), bẫy ép chu kỳ 128/80 tick, bệ di động thô; MH 1.25 px/tick; v.v. (MAP2_TIMELINE §3b–d, §6).
- **Chuyển cảnh** §7: 12 mốc có frame/tick/sai số; 2 cửa trái 3 pha (128 · 158/166 · 128 tick).
- **Boss**: Medusa phase table, nháy màu 9 tick, 3 cú trúng (−2 vạch mỗi lần), knock-back 31 px/36 tick (hit #3, cho `cvgp-fix-03`), quả cầu +150 tick / rơi 26 tick.
- **Âm thanh**: `audio_cues2.py` + file cues (khoảng lặng/mốc nhạc); nhận dạng bài chưa kết luận (cho routine 03).
- **Ảnh**: 36 ảnh planner đã xem (2 bảng `refs_a/b.png`): 32 đúng, **4 đã `git mv` đổi tên**; thêm 10 ảnh Map_2 + 2 ảnh boss (hiệu chuẩn mới).
- **Nghiệm thu routine 02**: [x] hiệu chuẩn ≥6 frame Map_2 (ZNCC TB 0.9468 ≥ 0.93, 10 frame) + ≥4 frame boss (7) + hàng nhìn thấy + watermark · [x] HUD từng frame + bảng sự kiện, 20 frame mắt 0 sai · [x] Simon 92.9 % + camera 100 % + 12 chuyển cảnh có tick · [x] `MAP2_TIMELINE.md` §1/§3/§4/§7 mở rộng + `CVM2_MEMORY §2c` · [x] ảnh mốc đã xem. **Chưa đạt/ xấp xỉ**: bệ di động + bẫy ép + Spear Knight/Ghost chỉ đo thô (chuyển 06/08); nguồn một số vật phẩm rơi (thịt nướng Hall B, III Hall C) chưa rõ.

### Routine 03 — 16/10/2026 — Nhạc 16 bài, xác nhận bài + mốc bằng âm thanh video, gắn nhạc Map 1, bảng mốc Map 2
- **Bước 0**: `main`; nhiễu nền giữ nguyên; cổng 00 + 02 = XONG. MCP `Castlevania_Original-main@5669a9bbfcade130`, dataPath đúng. Play an toàn (DisableDomainReload) 5 phiên, không crash; cuối phiên EditorSettings = HEAD (CRLF), `CVGP.ArenaUseDummy` = false, fps/vSync mặc định, Main.unity mở không dirty. **Sai sót quy trình (đã xử lý)**: ghi 4 file .cs (sửa Death) khi Editor còn Play sau bộ spam — thoát Play ngay, biên dịch lại sạch, chạy LẠI toàn bộ hồi quy sau đó (kết quả dưới là của bản cuối).
- **Giải mã** (ffmpeg): `cd Art_Source/music; ffmpeg -i "<NN…>.mp3" -vn -ac 1 -ar 22050 -c:a pcm_s16le _cache/03/audio/mNN.wav` (+ `sNN.wav` stereo 44.1 kHz); video: `-vn -ac 1 -ar 22050` ⇒ `v_<video>.wav` (5 video).
- **Nhập**: 16 MP3 chép nguyên byte ⇒ `Assets/Data/Audio/Music/mNN_*.mp3` (sha256 = nguồn, §3d); preset `MusicImporter`; Unity: 44.1 kHz stereo, độ trễ +2257 mẫu so ffmpeg (5/5 jingle NCC ≥ 0.999).
- **Nhận dạng** (§2d): 5 video × 16 bài; Block 1 = VK, Block 2 = Stalker (xác nhận), boss = Poison Mind, Stage Clear = nhặt cầu (4/4), bản đồ/tally không nhạc, Prologue ở Intro, STAGE 07 = Wicked Child (yếu). Phát hiện: dùng đồng hồ làm Stalker đứng 2.40 s; Phantom Bat 2 video lệch nhau (§5 #6); Medusa PM = thức + 0.77–0.95 s.
- **Điểm lặp** (§2d): 10 bài lặp + 6 không lặp; khớp cục bộ NCC 0.92–1.00; 05/09 xấp xỉ (rip 1 vòng + fade). Kiểm đầu ra thật VK/Stalker/PM: ±1 mẫu, NCC min 0.965, không khoảng lặng (`ref/data/music/check_m*.txt`).
- **Code**: `eMusic`, `MusicLibrary` (+ asset, builder `CVGP/Build MusicLibrary` — chạy 2 lần: ghi rồi "không đổi"), `AudioManager.PlayMusic(eMusic)` + lặp theo mẫu + `MusicEvent`; `StageFlow` (ConfigStage.music, StageClear, Death lúc bắt đầu chết qua `SimonController.DyingStarted`), `BossBase.StartBossMusic` (ConfigBlock.bossMusic), `IntroScreen`/`GameOverScreen`/`Stage4Stub`/`Stage1Stub` (id nhạc); `ConfigRows` music/bossMusic → `eMusic`; 2 CSV 24 ô nguồn mới ⇒ `CVGP/Import Config` OK (12 bảng 673 dòng, ghi 2); `CVGP/Export Stats Report` (CHARACTER_STATS cột nhạc). Probe Editor `MusicProbe`, `MusicProbeTap`. Hiệu đính 3 dòng `CVGP_MEMORY` (ghi "(CVM2 03)").
- **Mốc Unity ↔ video** (§2e): Prologue L448 (video L443.9, +0.07 s), VK L922 (−0.2 tick), lặp mỗi 1792 tick, PM lúc lao đầu L6366, Stage Clear cùng tick nhặt cầu L8168, bản đồ im L8948, Stalker L9370; Death 126 tick → GAME OVER → Game Over → CONTINUE → VK. 2 lần chạy y hệt.
- **Hồi quy H11 (bản cuối, 3 phiên Play)**: Main `flow` `e0b2d54c` + **h8 5/5** (352/432/448/890/922), `spam1..4` 0 lỗi console (437 s) — trước đó (trước sửa Death) `flow30`/`flow144` `e0b2d54c` + h8 5/5 ×2; Arena-Simon `all` @60: long `5963c4ba` · simon `b2954cbe` 20/20 · attack `eee2cdfb` 21/21 · item `7ca5bf56` 14/14 · enemy `f288b0ce` 20/20 · boss `31e6d711` 12/12 · b23 `171be0c1` 31/31 · b46 `58a8edf9` 26/26 · b24 `7adfbc54` 16/16 · b56 `6cb8a1ad` 14/14 · rules `03fafb24` 68/68 (1047 s, 0 lệch); Arena-Dummy `core` `76516076` 21/21. **Không hash nào đổi** (nhạc không chạm trạng thái game). gallery không chạy (không đổi số nhân vật — H13 không kích hoạt). flow30/144 không chạy lại sau sửa Death (sửa chỉ thêm sự kiện; flow 60 + spam đã chạy lại).
- **Ảnh đã xem**: `_cache/shots/03/flow_L930_vk.png` (Stage 01 L930 lúc VK đang phát — HUD STAGE 01 TIME 0300, Simon cạnh tượng) ; `_cache/shots/03/ssm_05_07_09.png` (ma trận tự tương đồng — xác định vòng 07/09).
- **Câu hỏi §5 mới**: #6 (mốc nhạc Phantom Bat), #7 (đồng hồ dừng nhạc).
- **Nghiệm thu routine 03**: [x] 16 bài nhập, tên/sha256 §3d; điểm lặp đo cho mọi bài lặp (10; 05/09 xấp xỉ có lý do) · [x] bảng bài ↔ mốc Map_1 + Map_2 + Intro + 2 video boss có NCC (§2d); 24 ô Config có nguồn video hoặc lý do giữ suy luận · [x] nhạc Map 1 phát đúng mốc (log §2e, lệch ≤ 0.07 s ở mọi mốc so được; Phantom Bat theo quy tắc có ghi lệch) ; bảng mốc Map 2 §2f sẵn cho 11 · [x] mọi hash bảng chuẩn y nguyên, H8 5/5, console chỉ lỗi nền. **Xấp xỉ/thiếu**: Death/Game Over/bài Block 3–6 không có video (suy luận); mốc nhạc Phantom Bat 2 video mâu thuẫn; đồng hồ-dừng-nhạc chưa dựng; không nghe trực tiếp (kiểm bằng log + WAV đầu ra).

### Routine 04 — 17/10/2026 — Lọ nước thánh xuyên địch (bằng chứng video + kiểm Unity + probe mới)
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `Tools/cvgp|cvm2/__pycache__/`, `UserSettings/`, submodule ` m`); cổng 00 = XONG (00–03 đều XONG). MCP `Castlevania_Original-main@5669a9bbfcade130`, dataPath đúng.
- **Video** (§2g): 7 lần ném trong 2 video boss (Map_2 3:46–3:56 dùng đồng hồ, không phải nước thánh). Kết luận: lọ xuyên, vỡ chỉ ở sàn, 1 đòn khi bay qua (boss1 f471, boss2 f393), boss đứng yên lúc lọ chạm. Phát hiện **CVGP 14 nhầm frame**: f342–352 lọ KHÔNG chạm dơi; đoạn xuyên thật là f467–483.
- **Unity trước sửa**: thử nhanh (execute_code, Arena) — lọ trúng bia tick 8, bay tiếp, vỡ sàn x 343 tick 29; Phantom Bat: trúng (E 16→15), xuyên 21 tick, vỡ sàn ⇒ **hành vi ĐÃ đúng**; "Unity vỡ ngay" (CVGP 14/16, ACCEPT21) chỉ suy từ comment `HolyWater.cs` ("Trúng trực tiếp ⇒ vỡ") trong khi Config `pierces = 1` từ CVGP 04 — chưa từng đo.
- **Sửa (tối thiểu, không đổi logic)**: comment `HolyWater.cs` (+ ghi chú `OnSpent`), `BossBase.cs` (f342–352 → f467–482); `ConfigSubWeapon.csv` ô `src_pierces` HolyWater: "suy luận wiki" → nguồn video + `đo:cvm2-04` ⇒ `CVGP/Import Config` (ghi lại 1 bảng, GameConfig không đổi; asset chỉ đổi chuỗi). `CHARACTER_STATS.md` không chứa nguồn `pierces` ⇒ không xuất lại (H13 không kích hoạt — không đổi số).
- **Probe mới** `HolyPierceProbe` (5 kịch bản) + bộ `holy` trong `RegressionRunner` (`EXPECTED` thêm `52c59864`). Chạy: 5/5 ĐẠT, hash `52c59864` ×2.
- **Sai sót quy trình (đã xử lý)**: ghi `HolyPierceProbe.cs` khi Editor còn Play (sau thử nhanh) — thoát Play ngay, biên dịch lại sạch; mọi kết quả bên dưới chạy SAU biên dịch. EditorSettings bị trả về `enabled = false` sau lần biên dịch đầu ⇒ phiên Play `run_holy1` + `all` vào Play CÓ domain reload (không crash); 2 phiên sau đặt lại DisableDomainReload.
- **Hồi quy H11 (3 phiên Play)**: Arena-Simon `all` @60 (12 bộ, 1058 s): long `5963c4ba` · simon `b2954cbe` 20/20 · attack `eee2cdfb` 21/21 · item `7ca5bf56` 14/14 · enemy `f288b0ce` 20/20 · boss `31e6d711` 12/12 · b23 `171be0c1` 31/31 · b46 `58a8edf9` 26/26 · b24 `7adfbc54` 16/16 · b56 `6cb8a1ad` 14/14 · **holy `52c59864` 5/5** · rules `03fafb24` 68/68 — 0 lệch; Arena-Dummy `core` `76516076` 21/21; Main `main` @60: flow `e0b2d54c` + **h8 5/5** (352/432/448/890/922), spam1..4 0 lỗi console (437 s). **Không hash nào đổi** (chỉ thêm `holy`). gallery không chạy (không đổi số nhân vật — H13 không kích hoạt).
- **Ảnh đã xem**: 5 tấm zoom video (§2g) + `_cache/shots/04/bat_a_vao_r027.png` + 4 tấm ghép `pairs_0..3.png` (7 cặp video ↔ Unity).
- **Cuối phiên**: thoát Play; EditorSettings = HEAD (CRLF), `CVGP.ArenaUseDummy` = false, `CheatInvincible` = false, fps −1 / vSync 1, Main.unity mở không dirty.
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 04**: [x] bảng bằng chứng 7 lần ném + kết luận CÓ gây 1 đòn khi bay · [x] lọ xuyên địch trong Unity (bia / Phantom Bat / Medusa / III), các bộ probe ĐẠT, không hash cũ nào đổi, `holy` mới ghi bảng chuẩn · [x] 7 cặp ảnh đã xem; H8 5/5; console chỉ lỗi nền. **Xấp xỉ**: 2 lần lọ chồng thân Medusa không trừ máu ở video (rìa thân / bất tử) — Unity trừ khi hộp chồng; hộp Medusa để routine 09.
