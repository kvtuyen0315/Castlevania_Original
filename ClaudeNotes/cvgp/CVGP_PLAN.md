# CVGP — Gameplay Castlevania (NES): input · bảng chỉ số · character controller + state machine mọi nhân vật · dựng & chơi Map 1 (Stage 1–3)

> Soạn 11/10/2026 bởi session planner (Opus 5.5, effort max). **Mọi routine `cvgp-NN` đọc file này ĐẦU phiên** (cùng `CVGP_MEMORY.md`; cần chi tiết hạ tầng cũ thì đọc `ClaudeNotes/cvti/CVTI_MEMORY.md §4`).
> Số liệu ở đây đo lúc planner chạy — **đo lại trước khi dựa vào** nếu routine cần độ chính xác cao hơn.

## 0. Yêu cầu gốc (nguyên văn, 11/10/2026)

> Tiếp tục tạo danh sách routines về việc làm tất cả character controller và State machine. Với tạo các info chỉ số tất cả character
> Xem qua wiki của boss ở đây https://strategywiki.org/wiki/Castlevania/Bosses
> Và wiki của enemy ở đây https://strategywiki.org/wiki/Castlevania/Enemies
> Wiki của item ở đây https://strategywiki.org/wiki/Castlevania/Items
> Còn đây là wiki của gameplay ( có character chính ) https://strategywiki.org/wiki/Castlevania/Gameplay
> Về phần làm các nút controller thì ở bàn phím thì di chuyển thì các nút hướng Arrow, còn lại thì phân bổ từ phím Z -> M theo hàng ngang của phím
> Về phần controller thì thì cứ theo như gamepad ( tôi đang dùng chiếc Flydigi Vader 4 Pro hệ xbox )
> Dựng map 1 và play game theo như theo như video trên. Xem qua Walkthrough Stages 1 -3 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_1_-_3
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

Kèm: 2 video (`Map_1.mp4`, `Boss_Map_1_Behaviour.mp4`) + ảnh thư mục Downloads có các sheet.

**Người dùng trả lời câu hỏi của planner (11/10/2026):**
- Bàn phím: **Z = Roi (nút B) · X = Nhảy (nút A) · C = Vũ khí phụ · V = Select · B = Start/Pause**; N, M để trống; mũi tên = di chuyển; Enter/Space vẫn là Start (như màn Title); Up + Roi vẫn ném vũ khí phụ như bản gốc.
- Gamepad Flydigi Vader 4 Pro (chế độ Xbox/XInput): **A = Nhảy · X = Roi · B và Y = Vũ khí phụ · Menu (≡) = Start · View (⧉) = Select**; D-pad và cần trái đều di chuyển.
- Âm thanh: người dùng chỉ thư mục `C:\Users\kvtuy\Downloads\NES - Castlevania - Sounds`. Planner đã so: **37 file trùng từng byte với pack `393996`** đã nhập ở CVTI 01 (`Art_Source/sounds-resource/393996/`, toàn SFX 0.10–2.40 s) ⇒ **chưa có bản nhạc nào** (§9 #1).

## 1. Nguồn

| Loại | Đường dẫn / ghi chú |
|---|---|
| Video Map 1 (chỉ đọc) | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_1.mp4` — 3:59.83, 1266×950, 30 fps, AAC 48 kHz stereo. Quay màn hình 1 playthrough YouTube: Stage 01 → 02 → 03 + boss Phantom Bat → bảng cộng điểm → bản đồ lâu đài → STAGE 04. Đoạn **không phải game** 1:47.77–1:54.33 (trang YouTube + quảng cáo) + thanh YouTube che đáy tới 1:56.73 — loại khỏi đo. |
| Video boss (chỉ đọc) | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Boss_Map_1_Behaviour.mp4` — 25.37 s, 1264×834, 30 fps. Quay trình duyệt YouTube ("Castlevania NES All Bosses"): game ở giữa, 2 bên viền mờ, có UI YouTube. Phantom Bat tới khi chết → quả cầu đỏ → nhặt. |
| Video cũ | `...\Title_Intro.mp4` (cụm CVTI — đã xong). |
| Chỉ mục timeline + hiệu chuẩn video | **`ClaudeNotes/cvgp/ref/MAP1_TIMELINE.md`** + ảnh `ClaudeNotes/cvgp/ref/*.png` (planner đo — §3) |
| Wiki đã tóm tắt | **`ClaudeNotes/cvgp/wiki/CV_WIKI_DATA.md`** (6 trang StrategyWiki ở §0; §4 tóm tắt) |
| Kiểm kê sheet | **`ClaudeNotes/cvgp/inventory/`** — JSON rect từng ô sprite + ảnh đánh số + `SHEETS_INVENTORY.md` (§2) |
| Code cũ đã xoá (chỉ tham khảo PHONG CÁCH) | `git show 2bfd90b^:Assets/Data/Script/Character/State/BaseState.cs` (+ `BaseCharacter.cs`, `JumpState.cs`, `PhysicController.cs`, `System/InputManager.cs`): state pattern 1 class/state với `OnEnterState / OnExecuteState / CheckNextState / OnExitState`, enum `eStateType`, `eDirectionType`. **Không chép** phần vật lý `Update + Time.deltaTime + Physics2D.BoxCast` (trái H4). |
| Hạ tầng CVTI đang chạy | `NesCoord` (PPU 16, NES y xuống, `ToWorld`), `NesClock` (60 Hz, `OnSystemTick`/`OnTick`), `NesTiming`, `GameManager` (`eGameScreen`, `GoTo(next, blackFrames)`), `GameScreen`, `InputController` (Start), `AudioManager` (`PlayMusic`/`PlaySfx`), `FrameAnimator`, `NesText`/`NesFont`, `GameSession`, `Hud` (Canvas World Space + TMP), `IntroScreen`, `Stage1Stub`, `CvtiDebug`, menu `CVTI/*`. Chi tiết API: `CVTI_MEMORY §4.1–§4.6`. |

## 2. Tài nguyên

### 2a. Sheet (routine 01 chép nguyên byte vào `Art_Source/spriters-resource/`)
| File ở `C:\Users\kvtuy\Downloads\` | Đích | Kích thước | sha256 | Dùng cho |
|---|---|---|---|---|
| `NES - Castlevania - Enemies & Bosses - Enemies.png` | `enemies.png` | 474×190 RGBA, 18 354 B | `fce5814984bf31a2ee0f689ec9d7a4dde22bde32037e1d8b49dda75bcea3044a` | 16 nhóm enemy (05) |
| `NES - Castlevania - Enemies & Bosses - Bosses.png` | `bosses.png` | 415×292 RGBA, 19 791 B | `1532ff90baef5e7f62e78bcd7d10a8f045498da7bcef29782ea7793a19092235` | 6 boss + hiệu ứng (05) |
| `NES - Castlevania - Miscellaneous - Items & Objects.png` | `items_objects.png` | 251×135 RGB, 8 841 B | `74704601cc14460ab2242c5f71be4662555d33dd9a01bcd2f96265d6cc37f61e` | item, nến, đuốc, cửa, số điểm (05) |
| `NES - Castlevania - Stages - Level 1_ Entrance.png` | `level1_entrance.png` | 3601×411 RGBA, 83 940 B | `f9f8e348837db0b64f3606dfb9b6cb80daf571791fce3771e13fd50997d90a2f` | địa hình Map 1 (06) |
| (đã có từ CVTI) `simon_belmont.png` | — | 799×529 | `8151622015ab8d322b0ded1f29b63ee07faa538bc773008e28b098b2c1cdf7e4` | CVTI mới cắt 6 khung; cụm này cắt toàn bộ (05) |
| (đã có từ CVTI) `cutscenes_credits.png` | — | 1039×1128 | `542d55e5…6edf008` | màn bản đồ lâu đài sau Block 1 (05/15) |
| (đã có từ CVTI) `sounds-resource/393996/01..37.wav` | — | 37 SFX | md5 ở `CVTI_MEMORY §3` | định danh ở 02 (thư mục `Downloads\NES - Castlevania - Sounds` trùng từng byte) |

Mọi sheet ghi credit "Sprites ripped by Mister Mike. Give credit if used." (logo 224×32 trong ảnh — bỏ khi cắt).

### 2b. Kiểm kê sheet (planner, `ClaudeNotes/cvgp/inventory/` — `SHEETS_INVENTORY.md` + JSON + ảnh đánh số + `tools/` chạy lại ra y hệt)
| Sheet | Ô | Nhóm | Ghi chú cắt |
|---|---|---|---|
| Simon | 181 | 43 | Thân + roi ghép sẵn (roi duỗi rộng 44/60, thân 16 px bên phải ô) + **16 mảnh roi rời** (L1, L2 chỉ mảnh duỗi, L3 × 4 màu) ⇒ tách lớp thân/roi được; pivot chân = baseline hàng (đáy slot 32 px); Simon quay TRÁI |
| Enemies | 71 | 16 | 2 ô dính liền (`enemy_dragonskull_0/1` — tách 2 khung 16×16); tầng 16×16 của Zombie/Fish Man/Black Knight có thể là nửa thân |
| Bosses | 54 | 9 | 2 khung Medusa **nền đen thật** (cắt nguyên, không key); Dracula P2 cao tới 80 px; hiệu ứng chết boss 3 cỡ |
| Items & Objects | 65 | 10 | Sheet không có nhãn (tên do planner gán theo hình); 16 ô `obj_block_*` không có xám (có thể là tượng Moai — Second Quest); dải 4 khối gạch dính liền |
| Level 1 | 5 vùng | — | xem §2c |

Key chung: xám (116,116,116) + xanh (0,128,0) ⇒ trong suốt, **giữ đen**; vùng xám khép kín nhỏ là lỗ trong sprite.

### 2c. Bản đồ Level 1 (`inventory/level1_entrance.json`)
| Vùng | rect sheet [x,y,w,h] | Màn 256 px | Hàng sàn chính | Ghi chú |
|---|---|---|---|---|
| Castle Entrance (Stage 01 ngoài trời) | [1, 11, 768, 192] | 3 | y 160 | không có enemy (wiki); 5 đuốc; vòm cổng tháp cột 43 |
| Entrance Hall (Stage 01 trong) | [774, 11, 1536, 192] | 6 | y 176 | cột cửa đỏ-trắng mép phải (cột 95) → Crypt A |
| Crypt A (Stage 02 trên) | [2315, 11, 512, 192] | 2 | y 176 | 2 giếng cầu thang xuống Crypt B (lệch ngang −32 px); cửa mép phải (cột 31) → Phantom Bat |
| Crypt B (Stage 02 dưới) | [2315, 218, 512, 192] | 2 | y 96 | nước hàng 10–11 (chết) |
| Phantom Bat (Stage 03 + phòng boss) | [2832, 11, 768, 192] | 3 | y 176 | cửa cuối mép phải (cột 47) |

Cao 192 px = 12 khối 16; lưới 16×16 & 8×8 căn offset (0,0); toàn map **118 tile 8×8 / 164 metatile 16×16 duy nhất** ⇒ dựng Tilemap gọn (06). Nối giữa vùng là suy luận từ bố cục — 06/15 xác nhận bằng video.

## 3. Video: hiệu chuẩn, timeline, số đo sơ bộ (đầy đủ ở `ClaudeNotes/cvgp/ref/MAP1_TIMELINE.md`)

### 3a. Hiệu chuẩn (planner đo bằng HUD + ZNCC với sheet bản đồ; kiểm glyph HUD nét 100 %)
| | Map_1.mp4 (1266×950) | Boss_Map_1_Behaviour.mp4 (1264×834) |
|---|---|---|
| `x_NES = (x_v − ox)/sx`, `y_NES = (y_v − oy)/sy` | **sx 4.8585, sy 4.1605, ox 8.60, oy −22.55** | **sx 3.6295, sy 3.1765, ox 169.68, oy 40.13** |
| Tỉ lệ pixel | **7/6** (256×224 kéo về 4:3) — **KHÔNG phải 8:7** như video Title_Intro | 8/7 |
| Dòng NES nhìn thấy | y 8–231 (cắt 8 trên + 8 dưới) | y 10–229 |
| Capture thực | ≈ 21 frame game/giây (35 % frame lặp, jitter) ⇒ mốc đơn lẻ ±2–3 tick; đo thời lượng bằng hồi quy hoặc đồng hồ TIME | ≈ 20–22 fps khi có chuyển động |
| Ngoài game | watermark đỏ góc phải-dưới (NES x 240–252, y 224–231); **đoạn non-game 1:47.77–1:54.33** (YouTube + quảng cáo) + thanh YouTube che đáy tới 1:56.73 | viền mờ 2 bên, UI YouTube |

So ảnh: dùng ảnh đã lấy mẫu lưới NES `ref/nes/*_nes.png` (so trực tiếp với ảnh chụp Unity ở lưới NES) hoặc áp công thức trên lên ảnh full frame `_cache/video/ref/`. Đừng scale đều 2 chiều.

**Bản đồ ↔ màn hình**: vùng 192 dòng của sheet Level 1 có **dòng r ↔ NES y = 32 + r** (16 dòng đầu nằm dưới HUD); vùng chơi nhìn thấy = NES y 48–223 (176 dòng — khớp wiki); NES y 224–239 đen. `world_x = cx + x_màn`, cx tối đa = rộng vùng − 256 (đều đạt trong video).

### 3b. Timeline Map_1 (rút gọn — 50 dòng chi tiết ở `MAP1_TIMELINE §2`)
| Mốc | Sự kiện |
|---|---|
| 0:00–0:01.7 | đuôi cảnh Intro (cụm CVTI), 0:01.7–0:02.2 đen |
| **0:02.2** | **Stage 01 Castle Entrance** cx 0, Simon world x 51 (y chân 190); 5 chậu lửa: tim lớn → roi (đóng băng 0.77 s, cấp 2) → roi (cấp 3) → tim lớn → dao găm |
| 0:16.2–0:20.0 | camera khoá cx 512, Simon tự đi vào cổng (khuất ở x màn 191), TIME đứng; đen 0:19.7–0:20.0 |
| **0:20.0** | **Entrance Hall** cx 0, Simon world x 49 (y chân 206): Zombie từng nhóm 2–3 từ mép phải, 3 Black Leopard trên gờ, nến → tim/túi tiền/stopwatch/holy water/rosary, cầu thang lên tầng trên |
| 1:24.8–1:31.8 | **cửa cuối Stage 01**: camera cuộn 128 px → dừng (cửa mở, Simon tự đi 68 px, cửa đóng) → cuộn 128 px; TIME đứng 6.8 s; **STAGE 02 lúc 1:31.87** |
| 1:31.9–1:47.7 | **Crypt A** tầng trên/dưới: Vampire Bat, roi khi ngồi phá tường gạch lộ Pot Roast, ném dao; xuống cầu thang góc trái-dưới |
| 1:47.8–1:54.3 | **NON-GAME** (chuyển tầng CA→CB bị che) |
| 1:54.4–2:27.7 | **Crypt B** (nước): Fish Man nhảy khỏi nước liên tục, túi 1000 (2:23.8), lên cầu thang trái-lên → đè HUD → đen 0.33 s → về Crypt A phải |
| 2:28.0–2:43.2 | Crypt A phải: cầu thang dài, dơi, rosary |
| 2:43.8–2:50.9 | **cửa cuối Stage 02** (cùng chuỗi như trên); **STAGE 03 lúc 2:50.87** |
| 2:50.9–3:17.4 | **hành lang Phantom Bat**: Zombie, bình vô hình (~2.6 s trong suốt), nến → rìu (3:10.67), thấy dơi boss ngủ từ ~3:15 |
| 3:17.4 | **camera khoá cx 512** (phòng boss); nến chân cầu thang → **II** |
| 3:19.43–3:30.40 | **boss thức** (Simon trên cầu thang, x màn ≈ 183) → bị rìu + II hạ (E 16→0), +3000, chùm lửa 0.75 s |
| 3:32.67 | **quả cầu đỏ** hiện ở (128,128) và rơi; Simon nhặt 3:33.0; TIME dừng 0127 |
| 3:37.4–3:49.9 | **tally**: TIME 1 đv/tick (+10), nghỉ 0.9 s, tim 1/8 tick (+100) → 23 370 điểm |
| 3:52.3–3:58.7 | **bản đồ lâu đài** trượt vào từ phải (2.7 s), Simon nhỏ đi 1 px/tick; HUD đã STAGE 04, tim reset 05 |
| 3:59.2 | **Stage 04** (TIME 0400) — ngoài phạm vi Map 1 |

Suốt video: P = 03, máu PLAYER luôn 16 (Simon không trúng đòn lần nào); roi cấp 3 từ 0:10.5.

### 3c. Số đo sơ bộ (planner — routine đo lại; chi tiết + cửa sổ ở `MAP1_TIMELINE §3–§5`)
| Đại lượng | Giá trị |
|---|---|
| Simon đi | **1.00 px/tick**; camera giữ Simon ở x màn ≈ 128–129 khi cuộn |
| Nhảy | bay **38–40 tick**, chân lên **36–38 px**, KHÔNG parabol: lên ~2.3 px/tick ~16 tick → **đỉnh phẳng ~10 tick** → rơi 2.5–3 px/tick; nhảy kèm hướng xa 39–43 px; không đổi hướng giữa không trung; tư thế co chân (cao 20 px) |
| Cầu thang | **0.50 px/tick ngang + 0.50 px/tick dọc** |
| Knockback (chỉ video boss) | lùi ~11–13 px, nảy lên ~20 px, chạm đất ~0.4–0.5 s; −2 vạch; nháy bất tử ≈ 1.5 s (~92 tick) |
| Roi | tổng **18–22 tick** (trung vị ~20): vung sau 4–6 / giơ cao 4–8 / duỗi 6–10 tick; tầm tay→đầu roi: cấp 1 **≈ 22 px**, cấp 2 **≈ 24–25** (≈ cấp 1, chỉ đổi sang xích), cấp 3 **≈ 43** |
| Nhặt roi | đóng băng **46–48 tick**, Simon nháy màu, TIME vẫn chạy |
| Dao / rìu / nước thánh | dao **4.0 px/tick** thẳng; rìu vx 2.0–2.2, vy0 −5.1, g 0.24–0.25 px/tick²; nước thánh vx 1.4–1.6, g 0.12–0.13, chạm đất ~40–44 px sau ~26–28 tick |
| Zombie / Leopard / Bat / Fishman | **0.875** px/tick; báo nhảy ngang ~1.9–2.0 + chạy **2.0**; dơi ~1.0–1.12 về phía Simon, dao động dọc ±6–9 px chu kỳ ~1.3–2 s (thô); Fish Man nhảy vy −5.6, g 0.124, đỉnh ~129 px trên mặt nước (~46 tick); Fish Man bắn lửa **không thấy trong video** |
| HUD | 1 đơn vị TIME = **64.0 tick**; vạch máu giảm/hồi hiển thị dần (~1 vạch/7–8 tick); chữ điểm túi tiền ~0.8 s; rosary nháy trắng 0.40–0.44 s |
| Cửa cuối stage | cuộn 128 px @ 0.90–0.91 px/tick → dừng 2.30–2.33 s (Simon tự đi 68 px @ 1 px/tick) → cuộn 128 px; TIME đứng |
| Phantom Bat | ngủ ở (110.5, 64.5) màn (world PB x ≈ 622); thức khi Simon tới x màn ≈ 181–184 (trên cầu thang); lượn 0.5 s → lao đầu +31° @ ~1.5 px/tick → bay chậm 0.75–1.0 tới điểm → lơ lửng 0.5–1.6 s → bổ nhào 1.5–1.6 tới tầm đầu Simon rồi bay ngang → vọt lên 2.0–2.3 → lặp (chu kỳ bổ nhào ≈ 4.0–4.3 s); không bắn đạn; rìu −2 vạch/lần; nước thánh giữ chân; chết → lửa 0.7–0.8 s → **quả cầu 2.5 s sau** ở (127.5,128.5), g ≈ 0.24 |
| Không có trong video | Simon trúng đòn/chết ở Map_1, roi trên cầu thang, Fish Man bắn lửa, nhặt Holy Water/Stopwatch ⇒ theo wiki, ghi rõ |

## 4. Dữ liệu wiki (tóm tắt — đầy đủ ở `ClaudeNotes/cvgp/wiki/CV_WIKI_DATA.md`)

StrategyWiki trả 403 + màn chắn Cloudflare cho mọi truy cập tự động ⇒ planner lấy **bản lưu Wayback Machine** của đúng các URL (snapshot 2026, ghi ở `CV_WIKI_DATA §0`), chuyển thành văn bản (kiểm từng từ, không rơi chữ). Đọc 13 trang: Gameplay, Items, Enemies, Bosses, Walkthrough, Stages 1–3 (+ trang con Secrets, Stages 4–18). HTML/văn bản thô để ở `ClaudeNotes/cvgp/_cache/wiki_raw/` (KHÔNG commit — bản quyền); file dữ liệu chỉ chứa số liệu + mô tả diễn đạt lại.

**12 hằng số quan trọng nhất** (nguồn trong file):
1. Simon 16 vạch máu. Mỗi đòn trúng mất **2 vạch ở Stage 1–6, 3 ở 7–12, 4 ở 13–18** — mọi loại đòn trong 1 stage như nhau (kể cả boss, đạn). Rơi hố/nước/gai = chết ngay.
2. 1UP ở **30 000, 80 000, rồi mỗi 50 000** điểm. Khởi đầu 3 mạng, 5 tim (suy luận từ ảnh HUD).
3. Tim nhỏ +1 (rơi đung đưa), tim lớn +5 (rơi nhanh). Vũ khí phụ 1 tim/lần, **Stopwatch 5 tim**.
4. Roi: da (yếu) → xích ngắn (×1.5, mạnh) → xích dài (mạnh); chết thì về roi da. Morning Star nằm trong nến "vốn chứa tim nhỏ" khi đã thu đủ tim (4 / 8 — cơ chế wiki không giải thích); 2 đuốc trước cổng Stage 1 chứa Morning Star cố định.
5. Vũ khí yếu = roi da, Holy Water, Dagger; mạnh = roi xích, Boomerang (Cross), Axe. Máu enemy/boss cho dưới dạng **số đòn yếu / mạnh**.
6. Stopwatch đóng băng 5 s (boss miễn nhiễm trừ Phantom Bat, Queen Medusa); bình tàng hình ~5 s; Rosary diệt mọi enemy trên màn trừ boss (không điểm); Pot Roast +6 vạch (chỉ trong khối gạch phá bằng roi).
7. II/III cho 2/3 vũ khí phụ trên màn, rơi khi trúng enemy 10 lần bằng cùng vũ khí phụ, mất khi đổi vũ khí; nhặt lúc đang cầm Stopwatch/không vũ khí = 700 đ. Giết 2/3/4 enemy bằng 1 lần ném = 1000/2000/4000 đ.
8. Túi tiền 100/400/700, túi bí mật 1000, Crown/Treasure 2000.
9. Sau boss: Magic Crystal hồi đầy máu + **giờ còn lại × 10 + tim × 100**.
10. Không checkpoint giữa stage: hết mạng ⇒ Continue về **đầu stage đang chơi**.
11. Boss (đòn yếu/mạnh, điểm): Phantom Bat 12/8 (3000), Queen Medusa 12/8 (3000), Mummy Men 48/32 (3000), Frankenstein 24/16 (5000, Igor bất tử), Death 24/16 (7000), Dracula 24/16 (dạng 2 "32 đòn" — mâu thuẫn; điểm 50 000 vs "không điểm" — mâu thuẫn).
12. Wiki KHÔNG nêu: giờ đầu mỗi stage + tốc độ đếm, giới hạn tim, sát thương số từng vũ khí, mọi tốc độ/độ cao/knockback/bất tử, bảng rơi ngẫu nhiên ⇒ **đo video** (H1) hoặc ghi "chưa rõ".

**15 enemy thường theo block xuất hiện đầu tiên** (tên wiki — tên nhóm trên sheet · đòn yếu/mạnh · điểm):
| Block | Enemy | Routine |
|---|---|---|
| 1 (St 1–3) | Ghoul — Zombie · 1/1 · 100; Panther — Black Leopard · 1/1 · 200; Bat (Black/Red) — Vampire Bat · 1/1 · 200; Fishman — Fish Man · 1/1 · 300 (cầu lửa) | 13 |
| 2–3 (St 4–9) | Spear Knight — Black Knight · 3/2 · 400; Medusa Head · 1/1 · 300; Bone Tower — Dragon Skull Cannon · 8/6 · 400; Ghost · 3/2 · 300; Raven · 1/1 · 200; Skeleton — White Skeleton · 1/1 · 300; Fleaman — Hunchback · 1/1 · 500 | 17 |
| 4–6 (St 10–18) | Eagle (mang Fleaman) · 1/1 · 300; Bone Dragon — Skele-Dragon · 8/7 · 1000; Red Skeleton (không giết được, hồi sinh) · 400; Axe Knight — Axe-Man · 10/9 · 1400; Phantom Bat bản thường (St 16, cầu lửa) | 18 |

Thực thể phụ của boss: Snake (Medusa), Igor (Frankenstein). Boss: 14 (Phantom Bat), 19 (Medusa, Mummy Men, Frankenstein & Igor), 20 (Death, Dracula).

**Map 1 theo wiki** (`§6`, đo từ ảnh bản đồ nửa tỉ lệ ⇒ xấp xỉ): Stage 1 ngoài trời 768 px (3 màn), **không có enemy**, 5 đuốc tâm x ≈ 95 + 128k: Large Heart, Morning Star, Morning Star, Large Heart, Dagger; bí mật: nhảy vượt cửa lâu đài ⇒ túi 1000. Sảnh trong 1536 px (6 màn), 24 nến (#2 Stopwatch, #14 Holy Water, #23 Rosary), Ghoul + ≥ 3 Panther, khối phải bục Panther thứ 3 ⇒ túi tím 400, hết stage khi qua cầu thang cuối. Stage 2: 16 nến (#10 Stopwatch, #13 Rosary), Fishman + Bat, tường khối có Pot Roast (+1 Bat), 2 khối bí mật ⇒ túi 1000, nước chết người. Stage 3: 12 nến (#6 Axe, #7 Holy Water ngẫu nhiên, Invisibility Potion giữa cầu thang), Ghoul sinh từ phía trên, phòng boss có khối chân cầu thang ⇒ Double Shot, Phantom Bat treo trần.

## 5. Kiến trúc đề xuất (routine 07 CHỐT — muốn đổi thì ghi lý do vào `CVGP_MEMORY §4`)

### 5.1 Nhịp & tất định
- Mọi logic gameplay chạy theo tick `NesClock` (60 Hz). **Không** `Update`/`FixedUpdate`/`Time.deltaTime`/`Rigidbody2D`/`Physics2D` cho chuyển động hay va chạm gameplay.
- Một bộ điều phối duy nhất (đề xuất `StageWorld`, con của màn gameplay) gọi theo **thứ tự cố định** trong mỗi tick: (1) đọc `NesPad` (đã chốt ở đầu tick) → (2) Simon (state machine) → (3) đạn/vũ khí phụ → (4) enemy/boss → (5) item → (6) giải va chạm chiến đấu (hitbox↔hurtbox) → (7) camera → (8) spawn/despawn theo camera → (9) báo HUD. Không để từng thực thể tự đăng ký `NesClock.OnTick` (thứ tự đăng ký của Unity không tất định).
- Đóng băng: nhặt roi (cả thế giới dừng N frame), đồng hồ (enemy dừng), boss chết, pause — `StageWorld` bỏ qua đúng nhóm.
- RNG: `NesRandom` có seed, reset ở đầu stage ⇒ replay input ra đúng từng frame.

### 5.2 Toạ độ & đơn vị
- Đơn vị logic = pixel NES; vị trí thực thể lưu **fixed-point** (vd `int` 1/256 px) để tốc độ lẻ tích luỹ đúng; vị trí vẽ = phần nguyên ⇒ luôn nguyên pixel (H5).
- Toạ độ level: px trong 1 *section* (= 1 vùng của sheet Level 1), gốc trên-trái vùng 192 dòng. Planner đo (§3a): **dòng r của vùng ↔ NES y = 32 + r** ⇒ 16 dòng đầu nằm dưới HUD (HUD NES y 0–47 vẽ đè), vùng chơi nhìn thấy NES y 48–223, y 224–239 đen. World = `NesCoord.ToWorld(levelX − cameraX, levelY + 32)` khi camera ở gốc (hoặc tương đương: section đặt ở world y = −32/16 và camera dời theo x) — routine 06/07 chốt, ghi `§4`.
- Camera: chỉ cuộn ngang trong section, x nguyên px, kẹp trong biên section; luật bám Simon (giữ ở giữa? lệch?) **đo từ video** (routine 07). Đổi section (lên/xuống tầng qua cầu thang, qua cửa) = cắt/cuộn theo đúng video. **HUD bám camera** (con của camera hoặc dời theo camera mỗi tick) — HUD hiện là con của prefab màn (CVTI §4.5), routine 07/12 xử lý.

### 5.3 Va chạm địa hình
- Lưới va chạm sinh từ tilemap (routine 06): `Solid`, `Stair` (hướng ↗/↖, chân/đỉnh), `Water`/`Pit` (chết), `Door`, `Breakable` (tường có gà nướng), `Hazard`. Kích thước ô 16×16 (hoặc 8×8 nếu map có nửa khối — routine 06 chốt sau khi kiểm kê).
- Thực thể có hộp va chạm AABB (px) theo state (đứng/ngồi); kiểm sàn, tường; cầu thang = chế độ di chuyển riêng theo đường chéo 45°.
- Có xuyên platform từ dưới lên khi nhảy không, có va trần không, rơi khỏi mép thì rơi thẳng hay giữ quán tính — **đo từ video** (routine 08), ghi `§4`.

### 5.4 State machine (giữ phong cách code cũ của người dùng)
- `BaseState<TOwner>`: `OnEnterState()`, `OnExecuteState()`, `CheckNextState()`, `OnExitState()`; `StateMachine<TOwner, TState>` giữ `curState`, `SetState(TState)`, log chuyển state khi bật cờ debug. Enum tiền tố `e` (`eSimonState`, `eZombieState`, `ePhantomBatState`…). Mỗi tick: `OnExecuteState()` rồi `CheckNextState()` (y như code cũ).
- Simon: 1 class/state. Enemy đơn giản: state nhỏ (class lồng) nhưng vẫn qua cùng `StateMachine`. Boss: state + pha.

### 5.5 Chiến đấu
- `Hitbox` (gây sát thương: roi, vũ khí phụ, thân enemy, đạn) / `Hurtbox` (nhận) = AABB px theo **khung animation** (dữ liệu trong AnimSet/Config). Nhóm: `PlayerAttack`, `EnemyAttack`, `Player`, `Enemy`, `Breakable` (nến/đuốc/tường), `Item`.
- Giải va chạm tập trung 1 chỗ/tick. Sát thương lấy từ Config (theo Block nếu wiki có bảng), Simon bất tử N frame sau khi trúng (nháy), knockback, điểm khi diệt, hiệu ứng chết, rơi item.

### 5.6 Thực thể & spawn
- `Entity` (MonoBehaviour nhẹ, KHÔNG Update) đăng ký vào `StageWorld`; prefab từng loại; pool.
- Dữ liệu spawn đặt trong prefab level: `SpawnPoint` (loại + tham số), `SpawnZone` (sinh liên tục: chu kỳ, giới hạn số, mép trái/phải). Kích hoạt/huỷ theo khung camera (+ lề) như NES. Luật thật (vd zombie sinh liên tục ở vùng nào, mấy con, cách nhau bao nhiêu frame) **đo từ video** (routine 13/15).

### 5.7 Animation
- `AnimSet` (ScriptableObject/nhân vật): tên anim → `Sprite[]`, `durations[]` (frame), loop, (tuỳ) offset + hitbox từng khung. Chạy bằng `FrameAnimator` sẵn có (tick-based) hoặc lớp mở rộng.
- Simon: **thân + roi tách lớp** (roi là sprite con, offset theo khung, cấp 3 đổi 4 màu theo chu kỳ) — offset lấy từ các khung ghép sẵn trên sheet; routine 05 chốt.

### 5.8 Bảng chỉ số (Config)
- Nguồn sự thật: CSV `Assets/Data/Config/Csv/*.csv` (diff được, sửa tay/Sheets) → menu `CVGP/Import Config` → ScriptableObject `Assets/Data/Config/*.asset`. Được DÙNG `afsdk.GSheetScriptableObject<T>.ParseCSV` (không sửa framework).
- Mỗi dòng có cột `source` (`wiki:<trang>#<mục>`, `video:Map_1 mm:ss`, `đo:routine NN`, `chờ đo`).
- Bảng tối thiểu: `ConfigSimon` (máu, tốc độ, nhảy, cầu thang, knockback, bất tử…), `ConfigEnemy` (mọi enemy), `ConfigBoss` (mọi boss/pha), `ConfigItem`, `ConfigSubWeapon`, `ConfigWhip`, `ConfigBlock` (stage→block, giờ, nhạc, bậc sát thương), `ConfigDrop` (nến/đuốc → item), `ConfigScore` (1UP, tally…). Code đọc qua `GameConfig` (giữ tham chiếu các bảng) — không hard-code số.
- Kèm báo cáo đọc được cho người: `ClaudeNotes/cvgp/CHARACTER_STATS.md` sinh từ CSV.

### 5.9 Màn hình & luồng
- Giữ kiến trúc CVTI: 1 scene `Main.unity`, mỗi màn 1 `GameScreen`. `Stage1Stub` được **thay** bằng màn gameplay thật (giữ luồng Intro → đen 32 f → Stage 01); thêm màn `CastleMap` (bản đồ sau Block 1) và `GameOver` nếu cần. Level mỗi stage là prefab riêng (`Assets/Data/Prefabs/Level/Block1/Stage0N.prefab`) do màn gameplay bật/tắt.
- Scene thử: `Assets/Scenes/Test/Arena.unity` (sàn, bục, cầu thang lên/xuống, nước, tường) + menu spawn enemy/boss — để thử nhân vật ngoài Map 1.

### 5.10 Âm thanh
- `AudioManager.PlaySfx` có sẵn; thêm `eSfx` + bảng `ConfigSfx` (id → clip) sau routine 02. NES chỉ có ít kênh — SFX mới đè SFX cũ hay không, nhạc có tắt khi SFX ưu tiên không: routine 02 ghi đặc điểm đo được.
- Nhạc chưa có file ⇒ gọi `PlayMusic(null)` (im lặng + log), KHÔNG thay bằng âm khác (H2).

### 5.11 Kiểm thử & nghiệm thu
- Harness qua MCP như CVTI (`execute_code` đăng ký delegate `NesClock.OnSystemTick` để tiêm input/đổi màn đúng frame; `ScreenCapture` đúng frame). **Không** chuyển code game sang asmdef chỉ để chạy Unity Test Framework (Assembly-CSharp không tham chiếu được từ asmdef test) — nếu cần test tự động thì làm harness/menu chạy trong Play Mode và ghi báo cáo.
- **Input replay**: chuỗi bitmask nút theo frame (file text/JSON) phát lại qua `NesPad` ⇒ đo vị trí/khung theo tick, so số đo video (đi px/f, nhảy cao/thời lượng, cầu thang, knockback…), và tái hiện đoạn video để so ảnh (lệnh ghép 8:7 ở §3).
- Debug overlay (hitbox, toạ độ, state) + menu cheat `CVGP/*` (bất tử, cấp roi, vũ khí phụ, II/III, warp section, spawn enemy/boss) — chỉ Editor/dev build, tắt mặc định.

### 5.12 Thư mục
- Code: `Assets/Data/Script/{Core,Input,Config,Combat,Level,Camera,Character/Simon,Enemy,Boss,Item,Stage,Debug}/` (các thư mục `Script/Character`, `Script/Config` đang rỗng — dùng luôn).
- Art: `Assets/Data/Art/{Character/Simon,Enemy,Boss,Item,Object,FX,Level/Block1,CastleMap}/` (preset `NesSpriteImporter`: `Character/**` pivot Bottom-Center, còn lại Top-Left — enemy/boss cần pivot chân thì routine 05 mở rộng quy tắc, ghi `§4`).
- Config: `Assets/Data/Config/` (+ `Csv/`). Prefab: `Assets/Data/Prefabs/{Character,Enemy,Boss,Item,Object,Level,Screens}/`. Audio: `Assets/Data/Audio/{SFX,Music}/`.
- Công cụ python: `Tools/cvgp/` (dùng lại `Tools/cvti/slice_sprites.py` — thêm tham số `--manifest`, giữ tương thích manifest CVTI).
- Ghi chú: `ClaudeNotes/cvgp/` (`CVGP_PLAN.md`, `CVGP_MEMORY.md`, `ref/`, `wiki/`, `inventory/`, `_cache/` — `_cache` không commit).

## 6. Input (người dùng đã chốt — routine 03 làm)

| Nút NES / hành động | Bàn phím | Gamepad Xbox (Flydigi Vader 4 Pro, XInput) |
|---|---|---|
| Trái / Phải / Lên / Xuống | ← → ↑ ↓ | D-pad **và** cần trái (vùng chết ~0.5, chọn 8 hướng) |
| **B — Roi** | **Z** | **X** |
| **A — Nhảy** | **X** | **A** |
| Vũ khí phụ (phím tắt = Up + B) | **C** | **B** và **Y** |
| Select | **V** | View (⧉) = JoystickButton6 |
| Start / Pause | **B**, Enter, KeypadEnter, Space | Menu (≡) = JoystickButton7 (giữ thêm JoystickButton9 như CVTI) |
| (trống) | N, M | LB, RB, LT, RT, cần phải |

- Lớp trừu tượng `NesPad`: 8 nút NES + `SubWeapon` (ảo). Mỗi tick: `Held`, `Pressed` (cạnh lên, chốt trong `Update` như `InputController` để không lọt khi fps cao/thấp), `Released`. Gameplay chỉ đọc `NesPad`, không đọc `Input.*` trực tiếp. `InputController` (Start của Title) chuyển sang đọc `NesPad` hoặc dùng chung bảng phím — **không làm đổi hành vi Title/Intro** (H8).
- Input Manager cũ (`activeInputHandler: 0`) giữ nguyên: nút gamepad đọc bằng `KeyCode.JoystickButtonN`; **D-pad XInput trên Windows là trục 6/7** (YAML `axis: 5`/`axis: 6`), cần trái là trục X/Y (đã có trong `Horizontal`/`Vertical` mặc định) ⇒ routine 03 thêm 2–4 trục tên riêng (vd `CV_DPadX`, `CV_DPadY`, `CV_StickX`, `CV_StickY`) vào `ProjectSettings/InputManager.asset` bằng SerializedObject — **không** cài Input System mới (đổi `activeInputHandler` bắt khởi động lại Editor, routine chạy vắng mặt không làm được). Muốn đổi hướng thì ghi lý do `§4`.
- Phím tắt vũ khí phụ (C / B / Y) = **cờ riêng** `Pressed(SubWeapon)` trong đúng 1 tick, Simon (09) hiểu như Up + B. **Không tiêm giả Up** (giữ Up có thể làm Simon leo cầu thang ngoài ý muốn).
- Bảng phím là dữ liệu (ScriptableObject `InputBindings`), đổi được không cần sửa code. Replay/tiêm input đi qua cùng lớp.

## 7. Danh sách routine (chạy TUẦN TỰ — mọi routine đụng Unity dùng chung 1 Editor + 1 cầu MCP)

Model · effort theo luật skill `skill-commond`: **Sonnet** cho việc cơ học (đo theo công thức, nhập file, quét hồi quy), **Opus** cho mọi việc phải thiết kế/suy luận; effort `medium` (hạ tầng) / `high` (mặc định) / `xhigh` (tạo mới asset từ dữ liệu thô, kiến trúc lõi, nhân quả xuyên hệ, nghiệm thu). Ghi ở frontmatter `model:`/`effort:` của từng `SKILL.md` — `run-routines.ps1` tự đọc.

| # | taskId | Model · effort | Cổng phụ thuộc | Việc chính |
|---|---|---|---|---|
| 00 | `cvgp-00-ha-tang-so-ghi-nho-va-baseline` | Sonnet 5.5 · medium | — | Sổ `CVGP_MEMORY`, kiểm mẫu chỉ mục video, baseline + mốc luồng CVTI, commit đồ planner |
| 01 | `cvgp-01-nhap-sheet-gameplay-va-ban-do-level1` | Sonnet 5.5 · medium | 00 | Chép nguyên byte 4 sheet mới vào `Art_Source/` + sha256 + `SOURCES.md`, chạy lại kiểm kê, tool vào `Tools/cvgp/` |
| 02 | `cvgp-02-dinh-danh-37-sfx-qua-am-thanh-video` | Sonnet 5.5 · high | 01 | Gán tên 37 SFX bằng tương quan chéo với âm thanh video + xem frame; nhập `Assets/Data/Audio/SFX`, `eSfx` + `SfxLibrary` |
| 03 | `cvgp-03-input-ban-phim-gamepad-xbox-va-replay` | Opus 5.5 · high | 00 | `NesPad` (bàn phím §6 + gamepad XInput), chốt cạnh theo tick, 4 trục InputManager, replay ghi/phát, Title dùng chung |
| 04 | `cvgp-04-bang-chi-so-config-moi-nhan-vat-va-item` | Opus 5.5 · high | 00 | **Info chỉ số mọi character**: CSV có nguồn → ScriptableObject (Simon, enemy, boss, item, vũ khí, block, drop, điểm) + `CHARACTER_STATS.md` |
| 05 | `cvgp-05-cat-sprite-simon-enemy-boss-item-animset` | Opus 5.5 · high | 01 | Cắt ~400 sprite (Simon đủ anim + roi tách lớp có offset, enemy, boss, item, bản đồ lâu đài) + AnimSet |
| 06 | `cvgp-06-tilemap-va-luoi-va-cham-map1` | Opus 5.5 · **xhigh** | 01 | Tileset dedupe + Tilemap 5 vùng khớp từng pixel + lưới va chạm + đồ thị section |
| 07 | `cvgp-07-loi-gameplay-vat-ly-state-machine-hitbox-camera` | Opus 5.5 · **xhigh** | 03, 04, 05, 06 | Lõi: `StageWorld` theo tick, fixed-point, va chạm tile/cầu thang, `StateMachine` kiểu `BaseState`, hitbox/hurtbox, camera, `NesRandom`, debug/cheat, scene Arena |
| 08 | `cvgp-08-simon-di-nhay-ngoi-cau-thang-trung-don-chet` | Opus 5.5 · **xhigh** | 07 | Simon di chuyển đo từ video: đi/nhảy/ngồi/rơi/cầu thang/knockback/bất tử/chết |
| 09 | `cvgp-09-simon-roi-3-cap-va-5-vu-khi-phu` | Opus 5.5 · high | 08 (02 nên xong) | Roi 3 cấp × 4 tư thế + hitbox theo khung, 5 vũ khí phụ + giá tim + II/III |
| 10 | `cvgp-10-quet-hoi-quy-giua-cum` | Sonnet 5.5 · high | 07–09 | Quét hồi quy: luồng CVTI, replay, tất định đa fps, pixel-perfect khi di chuyển, hiệu năng, repo |
| 11 | `cvgp-11-nen-duoc-item-roi-va-kho-bau-bi-mat` | Opus 5.5 · high | 09 (+ đọc kết luận 10) | Nến/brazier, bảng rơi, item rơi/nháy/nhặt + hiệu ứng, kho báu bí mật, tường phá |
| 12 | `cvgp-12-hud-song-luat-choi-chet-hoi-sinh-tally` | Opus 5.5 · high | 11 | HUD sống, đồng hồ theo block, chết/hồi sinh, Game Over/Continue, 1UP, tally, pause |
| 13 | `cvgp-13-enemy-block1-zombie-bao-doi-nguoi-ca` | Opus 5.5 · high | 12 | Zombie, Black Leopard, Vampire Bat, Fish Man + spawner NES, đo video; khuôn "thêm enemy mới" |
| 14 | `cvgp-14-boss-phantom-bat` | Opus 5.5 · **xhigh** | 13 | Phantom Bat theo video boss + khung boss dùng lại (`BossBase`, phòng boss, thanh ENEMY, quả cầu) |
| 15 | `cvgp-15-lap-ghep-map1-stage-1-3-va-chuyen-canh` | Opus 5.5 · **xhigh** | 06, 11–14 | Đặt vật thể theo video (`MAP1_PLACEMENT.md`), cửa/đổi tầng, `StageScreen` thay `Stage1Stub`, tally → bản đồ lâu đài → STAGE 04 stub |
| 16 | `cvgp-16-nghiem-thu-map1-so-voi-video` | Opus 5.5 · **xhigh** | 15 | **Milestone**: nghiệm thu Map 1 so video trên 10 mặt, ĐẠT/CHƯA + routine fix đề xuất |
| 17 | `cvgp-17-enemy-block-2-3` | Opus 5.5 · high | 13 (+ đọc kết luận 16) | Enemy xuất hiện lần đầu ở Block 2–3 theo wiki (gần đúng), thử ở Arena |
| 18 | `cvgp-18-enemy-block-4-6` | Opus 5.5 · high | 17 | Enemy còn lại (Block 4–6) theo wiki, thử ở Arena |
| 19 | `cvgp-19-boss-medusa-mummy-frankenstein` | Opus 5.5 · high | 14, 18 | Medusa, Mummy Man ×2, Frankenstein & Igor theo wiki, phòng boss Arena |
| 20 | `cvgp-20-boss-death-va-dracula-2-pha` | Opus 5.5 · **xhigh** | 19 | Death (+ liềm) và Dracula 2 pha theo wiki |
| 21 | `cvgp-21-nghiem-thu-toan-bo-nhan-vat` | Opus 5.5 · **xhigh** | 16–20 | Nghiệm thu cuối: chỉ số mọi nhân vật ↔ nguồn, hành vi ở Arena, hồi quy Map 1, chất lượng — đề xuất cụm sau |

- **Mốc dừng tự nhiên**: sau **10** (lõi + Simon đã chắc) và sau **16** (Map 1 chơi được — milestone người dùng yêu cầu). 17–21 mở rộng "mọi character" + chốt cụm.
- Chạy cả dãy bằng script của người dùng (`C:\Users\kvtuy\Downloads\run-routines.ps1`): `.\run-routines.ps1 -Prefix "cvgp" -From 0 -To 21 -ProjectDir "D:\Unity\Castlevania_Original-main"` (thêm `-DryRun` để xem trước). Script tự lấy model/effort ở frontmatter, dừng cả dãy nếu một routine không tạo commit. Nên chạy thử **một** routine trước (`-From 0 -To 0`) để chắc phiên CLI thấy Unity MCP.
- Routine dừng ở cổng phụ thuộc vẫn commit phần ghi sổ `(dở dang)` ⇒ dãy chạy tiếp, routine sau tự dừng ở cổng của nó; đọc `CVGP_MEMORY §1` để biết chạy lại cái nào.
- Sửa phát sinh: đặt tên `cvgp-fix-NN-…` (routine nghiệm thu đề xuất, người dùng duyệt rồi mới tạo).

## 8. Hiến pháp cụm CVGP (muốn lệch phải ghi lý do vào `CVGP_MEMORY §4`)

- **H1** Video gốc là chuẩn về THỜI GIAN & CHUYỂN ĐỘNG (tốc độ, quỹ đạo, nhịp khung, thứ tự sự kiện); sprite sheet là chuẩn về PIXEL & BỐ CỤC; wiki là chuẩn về CHỈ SỐ (HP, sát thương, điểm, giá tim…) khi video không đo được. Video ↔ wiki mâu thuẫn ⇒ theo video, ghi `§4`. Không lấy pixel từ video làm sprite (video kéo pixel — Map_1 7/6, boss 8/7 — và nén h264).
- **H2** Không bịa art/âm thanh: không AI sinh, không tự vẽ, không lấy âm từ video. Thiếu ⇒ ghi `§5`, dựng phần còn lại.
- **H3** Không sửa submodule `Assets/TuyenAframework` (framework dùng chung) — cần mở rộng thì viết lớp trong `Assets/Data/Script/`.
- **H4** Logic chạy theo tick `NesClock` 60 Hz; thời lượng tính bằng frame; tốc độ bằng px/frame (fixed-point). KHÔNG `Update`/`FixedUpdate`/`Time.deltaTime`/Rigidbody2D/Physics2D cho gameplay (Update chỉ để chốt input).
- **H5** Pixel-perfect: vị trí vẽ nguyên pixel NES; sprite Point/không nén/không mip (preset `NesSpriteImporter`).
- **H6** Số liệu gameplay nằm trong Config (CSV → ScriptableObject) hoặc lớp hằng có tên (`NesTiming`…); mỗi số có nguồn (wiki/video/đo). Không số ma thuật rải trong code.
- **H7** Tất định: cùng chuỗi input + seed ⇒ cùng kết quả từng frame. Không phụ thuộc thứ tự `Update` của Unity, không `Random` không seed.
- **H8** Không phá luồng CVTI: Title → Intro → Stage vẫn đúng frame như `CVTI_MEMORY §7` (Title L0, Start L352, đen L432, Intro L448, đen L890, Stage L922). Routine đụng `GameManager`/input/flow phải chạy lại harness này.
- **H9** Chỉ làm đúng phạm vi routine; việc khác ghi `§6`, không tiện tay làm.
- **H10** Báo cáo trung thực: chỉ ghi "đã chạy/đã nhìn thấy" khi có log + ảnh chụp Play Mode đã tự mở xem. Chưa đạt thì ghi CHƯA ĐẠT, kèm việc còn thiếu.

## 9. Câu hỏi mở (routine 00 chép vào `CVGP_MEMORY §5`; đừng tự quyết thay người dùng)

1. **Nhạc vẫn chưa có** (thư mục `Downloads\NES - Castlevania - Sounds` = 37 SFX đã có). Cần: Vampire Killer (Stage 1–3, lặp), Poison Mind (boss), Stage Clear, Life Lost (chết), Game Over, Prologue (Intro — từ cụm CVTI). Người dùng bỏ file vào `Art_Source/_inbox/music/` ⇒ routine 01/02 (hoặc `cvgp-fix`) nhập. Chưa có ⇒ `PlayMusic(null)` im lặng + log, mốc bật/tắt nhạc vẫn ghi sẵn.
2. **Enemy/boss Block 2–6 không có video** ⇒ routine 17–20 dựng theo wiki, mức gần đúng (có ghi chú từng chỗ ước lượng). Có video gameplay các block đó thì bỏ vào `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\` — routine tự dùng để đo.
3. **Kiểm tay gamepad**: routine không bấm nút thật được ⇒ sau routine 03, người dùng bật menu `CVGP/Input/Debug Overlay` trong Play, bấm lần lượt D-pad/cần trái/A/X/B/Y/Menu/View để xác nhận (đặc biệt D-pad Flydigi ở chế độ XInput).
4. **HUD còn sprite TẠM** (thanh máu/tim/khung + chữ số Famicom — `CVTI_MEMORY §5 #16–17`): routine 05 kiểm sheet Items & Objects có bản gốc không; không có thì vẫn TẠM — có muốn tìm thêm sheet HUD bản US không?
5. Video `Map_1.mp4` là bản quay lại playthrough YouTube (có frame non-game ~1:55, nén) — đủ để đo; nếu có capture trực tiếp 60 fps thì số đo nhịp khung sẽ chính xác hơn.
6. (từ CVTI, vẫn mở) pixel vuông hay 8:7; Start có bỏ qua Intro không; attract/demo mode — cụm này giữ mặc định CVTI (vuông, không bỏ qua, không demo).
