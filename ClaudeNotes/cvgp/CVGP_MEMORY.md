# CVGP_MEMORY — sổ bàn giao cụm CVGP (gameplay, input, chỉ số, Map 1)

*Mọi routine CVGP: đọc file này + CVGP_PLAN.md ĐẦU phiên, cập nhật CUỐI phiên, commit cùng routine.*
*Hiến pháp H1–H10 ở CVGP_PLAN §8; kiến trúc đề xuất §5; danh sách routine §7.*

**Yêu cầu gốc (11/10/2026, nguyên văn):**
> Tiếp tục tạo danh sách routines về việc làm tất cả character controller và State machine. Với tạo các info chỉ số tất cả character
> Xem qua wiki của boss ở đây https://strategywiki.org/wiki/Castlevania/Bosses
> Và wiki của enemy ở đây https://strategywiki.org/wiki/Castlevania/Enemies
> Wiki của item ở đây https://strategywiki.org/wiki/Castlevania/Items
> Còn đây là wiki của gameplay ( có character chính ) https://strategywiki.org/wiki/Castlevania/Gameplay
> Về phần làm các nút controller thì ở bàn phím thì di chuyển thì các nút hướng Arrow, còn lại thì phân bổ từ phím Z -> M theo hàng ngang của phím
> Về phần controller thì thì cứ theo như gamepad ( tôi đang dùng chiếc Flydigi Vader 4 Pro hệ xbox )
> Dựng map 1 và play game theo như theo như video trên. Xem qua Walkthrough Stages 1 -3 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_1_-_3
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

**Người dùng đã chốt (11/10/2026):**
- Bàn phím: Z = Roi (B) · X = Nhảy (A) · C = Vũ khí phụ · V = Select · B = Start/Pause; N, M trống; mũi tên = di chuyển; Enter/Space vẫn Start; Up + Roi vẫn ném vũ khí phụ.
- Gamepad Flydigi Vader 4 Pro (XInput): A = Nhảy · X = Roi · B và Y = Vũ khí phụ · Menu = Start · View = Select; D-pad và cần trái đều di chuyển.
- Âm thanh: `Downloads\NES - Castlevania - Sounds` = 37 SFX trùng byte với pack 393996 đã nhập ⇒ **chưa có nhạc**.

## §1 Trạng thái cụm

| # | taskId | Trạng thái | Commit | Ngày | Ghi chú |
|---|---|---|---|---|---|
| 00 | cvgp-00-ha-tang-so-ghi-nho-va-baseline | **XONG** | (commit "CVGP 00") | 11/10/2026 | sổ + kiểm mẫu 17 mốc video (0 lệch) + baseline luồng CVTI (5 mốc H8 khớp) |
| 01 | cvgp-01-nhap-sheet-gameplay-va-ban-do-level1 | **XONG** | (commit "CVGP 01") | 11/10/2026 | 4 sheet nhập nguyên byte (sha256 khớp) + kiểm kê chạy lại y hệt từ `Tools/cvgp/` + **16 bản nhạc MP3 mới** nhập `Art_Source/music/` |
| 02 | cvgp-02-dinh-danh-37-sfx-qua-am-thanh-video | CHƯA | — | — | |
| 03 | cvgp-03-input-ban-phim-gamepad-xbox-va-replay | CHƯA | — | — | |
| 04 | cvgp-04-bang-chi-so-config-moi-nhan-vat-va-item | CHƯA | — | — | |
| 05 | cvgp-05-cat-sprite-simon-enemy-boss-item-animset | CHƯA | — | — | |
| 06 | cvgp-06-tilemap-va-luoi-va-cham-map1 | CHƯA | — | — | |
| 07 | cvgp-07-loi-gameplay-vat-ly-state-machine-hitbox-camera | CHƯA | — | — | |
| 08 | cvgp-08-simon-di-nhay-ngoi-cau-thang-trung-don-chet | CHƯA | — | — | |
| 09 | cvgp-09-simon-roi-3-cap-va-5-vu-khi-phu | CHƯA | — | — | |
| 10 | cvgp-10-quet-hoi-quy-giua-cum | CHƯA | — | — | |
| 11 | cvgp-11-nen-duoc-item-roi-va-kho-bau-bi-mat | CHƯA | — | — | |
| 12 | cvgp-12-hud-song-luat-choi-chet-hoi-sinh-tally | CHƯA | — | — | |
| 13 | cvgp-13-enemy-block1-zombie-bao-doi-nguoi-ca | CHƯA | — | — | |
| 14 | cvgp-14-boss-phantom-bat | CHƯA | — | — | |
| 15 | cvgp-15-lap-ghep-map1-stage-1-3-va-chuyen-canh | CHƯA | — | — | |
| 16 | cvgp-16-nghiem-thu-map1-so-voi-video | CHƯA | — | — | |
| 17 | cvgp-17-enemy-block-2-3 | CHƯA | — | — | |
| 18 | cvgp-18-enemy-block-4-6 | CHƯA | — | — | |
| 19 | cvgp-19-boss-medusa-mummy-frankenstein | CHƯA | — | — | |
| 20 | cvgp-20-boss-death-va-dracula-2-pha | CHƯA | — | — | |
| 21 | cvgp-21-nghiem-thu-toan-bo-nhan-vat | CHƯA | — | — | |

## §2 Timeline & số đo

*(planner đo — routine 00 kiểm mẫu; đầy đủ ở `ref/MAP1_TIMELINE.md`, tóm tắt ở CVGP_PLAN §3)*

### 2a. Hiệu chuẩn video → NES (routine 00 kiểm lại: ĐÚNG, không đổi)
`x_NES = (x_v − ox)/sx`, `y_NES = (y_v − oy)/sy`; tâm pixel NES (x,y) ở video `(ox + sx·(x+0.5), oy + sy·(y+0.5))`.

| | Map_1.mp4 (1266×950, 30 fps, 3:59.83) | Boss_Map_1_Behaviour.mp4 (1264×834, 30 fps, 25.37 s) |
|---|---|---|
| sx, sy | **4.8585, 4.1605** (tỉ lệ 7/6, KHÔNG phải 8:7) | **3.6295, 3.1765** (8/7) |
| ox, oy | **8.60, −22.55** | **169.68, 40.13** |
| Dòng NES nhìn thấy | y 8–231 | y 10–229 |
| Nhịp capture | ≈ 21 frame game/s (35 % frame lặp) ⇒ mốc đơn lẻ ±2–3 tick | ≈ 20–22 fps khi chuyển động |
| Ngoài game | watermark đỏ NES x 240–252, y 224–231; non-game 1:47.77–1:54.33 + thanh YouTube đáy tới 1:56.73 | viền mờ 2 bên, UI YouTube |

**Kiểm hiệu chuẩn (routine 00, frame Map_1 t=3.93 s):** lấy mẫu lưới NES theo công thức, vùng chữ HUD x 0–55, y 16–39: độ nét (mẫu <60 hoặc >170) = **1.000**; so mặt nạ 5 glyph `Assets/Data/Art/Font/glyph_{S,C,O,R,E}.png` với ô (0,16),(8,16),(16,16),(24,16),(32,16): **khớp 100 % pixel cả 5 glyph, dịch tối ưu (0,0)** ⇒ hệ số Map_1 đúng, không cần đo lại. `ref/nes/*_nes.png` đủ 44 file 256×240; đã mở montage 12 ảnh (Stage 01 đầu, Entrance Hall, cầu thang, cửa cuối, Crypt A, Crypt B, hành lang PB, phòng boss, boss thức, boss chết, bản đồ lâu đài, Stage 04): HUD/sân chơi căn đúng ô lưới. Hệ số video boss không kiểm riêng bằng glyph (chỉ xem 2 ảnh `boss_*_nes`, căn đúng).

### 2b. Mốc chính Map_1 (planner đo)
| Mốc | Sự kiện |
|---|---|
| 0:02.2 | Stage 01 Castle Entrance, Simon world x 51 |
| 0:06.5–0:07.2 / 0:09.8–0:10.5 | nhặt roi #1/#2 (đóng băng ~0.75 s), roi cấp 3 từ 0:10.5 |
| 0:16.2–0:20.0 | camera khoá cx 512, Simon vào cổng, đen 0:19.7–0:20.0 |
| 0:20.0 | Entrance Hall (Zombie, Black Leopard) |
| 1:24.8–1:31.8 | cửa cuối Stage 01 (cuộn 128 px → mở cửa → cuộn 128 px), STAGE 02 lúc 1:31.87 |
| 1:47.8–1:54.3 | NON-GAME (chuyển tầng CA→CB bị che) |
| 2:27.7–2:28.0 | chuyển tầng CB→CA, đen 0.33 s |
| 2:43.8–2:50.9 | cửa cuối Stage 02, STAGE 03 lúc 2:50.87 |
| 3:17.4 | camera khoá cx 512 (phòng boss); 3:19.43 boss thức; 3:30.17 +3000 boss chết |
| 3:32.67 | quả cầu đỏ hiện (128,128); 3:37.4–3:49.9 tally; 3:52.3–3:58.7 bản đồ lâu đài; 3:59.2 Stage 04 |

### 2c. Kiểm mẫu chỉ mục video (routine 00: 16 frame trích mới bằng ffmpeg `-ss t`, ảnh ở `_cache/00/`, đã mở xem)
| # | t (s) | Mốc | Quan sát | Kết quả |
|---|---|---|---|---|
| 1 | 3.93 | HUD tim lần 1 | TIME 0299, tim 10, 1 chậu lửa còn nguyên phía phải | ĐÚNG (tim đã 05→10; lệch ≤ 1 frame video) |
| 2 | 10.7 | roi cấp 3 | TIME 0293, tim 10, ô vũ khí trống | ĐÚNG |
| 3 | 15.6 | nhặt dao | TIME 0288, ô vũ khí có dao, tim 15 | ĐÚNG |
| 4 | 91.9 | Stage 01→02 | STAGE 02, TIME 0226, Crypt A cx 0 | ĐÚNG (1:31.87) |
| 5 | 93.0 | Crypt A, Vampire Bat | TIME 0225, tim 36 (đã ném dao), dơi bay | ĐÚNG |
| 6 | 111 | non-game | trang YouTube + quảng cáo | ĐÚNG (1:47.77–1:54.33) |
| 7 | 147.9 | chuyển tầng Crypt B→A | toàn đen | ĐÚNG (2:27.7–2:28.0) |
| 8 | 148.1 | Crypt A tầng dưới phải | TIME 0181, tim 54, Simon trồi từ đáy | ĐÚNG |
| 9 | 170.9 | Stage 02→03 | STAGE 03, TIME 0166, tim 60 | ĐÚNG (2:50.87) |
| 10 | 197.5 | phòng boss, dơi ngủ | TIME 0141, rìu, tim 75, dơi 16×16 treo trần | ĐÚNG |
| 11 | 199.7 | boss thức + II | TIME 0139, tim 76, icon II, dơi mở cánh | ĐÚNG (3:19.43) |
| 12 | 210.5 | boss chết | SCORE 014900, E rỗng, chùm lửa 2×3, TIME 0129 | ĐÚNG (+3000) |
| 13 | 212.7 | quả cầu đỏ | cầu giữa màn, TIME 0127 | ĐÚNG |
| 14 | 218 | tally TIME | TIME 0091, SCORE 015260 (+10/đv) | ĐÚNG |
| 15 | 233 | bản đồ lâu đài trượt vào | STAGE 04, SCORE 023370, tim 05 | ĐÚNG (3:52.3) |
| 16 | 237 | bản đồ lâu đài | dơi tím, Simon nhỏ ở đáy | ĐÚNG |

Chuyển tầng Crypt A↔B (#7/#8), 3 lần HUD đổi do nhặt (tim #1, dao #3, II #11), roi lên cấp (#2): đều khớp. **0 số lệch cần sửa.** Ảnh `f17_225.png` (tally tim) có trích nhưng không mở riêng — kết quả cuối tally (023370) đã thấy ở #15.

### 2d. Số đo chuyển động (planner, chưa đo lại)
Chép từ CVGP_PLAN §3c / `ref/MAP1_TIMELINE.md §4`: Simon đi 1.00 px/tick; nhảy 38–40 tick, cao 36–38 px, đỉnh phẳng; cầu thang 0.5+0.5 px/tick; roi ~20 tick, tầm 22/24–25/43 px; Zombie 0.875, Leopard chạy 2.0, dơi ~1.0–1.12; TIME 1 đv = 64 tick; tally TIME 1 đv/tick +10, tim 1/8 tick +100; Phantom Bat xem MAP1_TIMELINE §5.

## §3 Tài nguyên

### Sheet nguồn (routine 01 đã chép nguyên byte vào `Art_Source/spriters-resource/`, sha256 khớp 100 %)
| File ở Downloads | Đích | sha256 |
|---|---|---|
| NES - Castlevania - Enemies & Bosses - Enemies.png | enemies.png | fce5814984bf31a2ee0f689ec9d7a4dde22bde32037e1d8b49dda75bcea3044a |
| NES - Castlevania - Enemies & Bosses - Bosses.png | bosses.png | 1532ff90baef5e7f62e78bcd7d10a8f045498da7bcef29782ea7793a19092235 |
| NES - Castlevania - Miscellaneous - Items & Objects.png | items_objects.png | 74704601cc14460ab2242c5f71be4662555d33dd9a01bcd2f96265d6cc37f61e |
| NES - Castlevania - Stages - Level 1_ Entrance.png | level1_entrance.png | f9f8e348837db0b64f3606dfb9b6cb80daf571791fce3771e13fd50997d90a2f |

`simon_belmont.png`, `cutscenes_credits.png` (+ `title_screen.png`) còn khớp sha256 trong `Art_Source/SOURCES.md`. Thư mục `Downloads\NES - Castlevania - Sounds\01..37.wav` trùng md5 từng file `sounds-resource/393996/` (routine 01 so lại: 0 file khác) — không nhập lại.

### Nhạc (routine 01 tìm thấy — thay đổi so với kế hoạch "chưa có nhạc")
Thư mục `Downloads\NES - Castlevania - Sounds\` ngoài 37 WAV còn **16 file MP3** (stereo 44.1 kHz ~165 kbps, tên kiểu `NN. Tên (Địa điểm).mp3`) — đã chép nguyên byte vào `Art_Source/music/` (sha256 khớp nguồn, ghi `SOURCES.md`), **chưa import Unity** (routine 02 làm). `Downloads\Music\` rỗng. Bảng (số = số thứ tự trong tên file):
| # | Bài | Gợi ý dùng (suy từ bản NES gốc, CHƯA đối chiếu video) |
|---|---|---|
| 01 | Underground (Name Entry) [Famicom Disk System] | nhập tên (không dùng) |
| 02 | Prologue (Castle Gate) | Intro |
| 03 | Vampire Killer (Courtyard) | Stage 1 (dài 67.0 s, đã đo bằng ffmpeg) |
| 04 | Stalker (Armory) | Stage 2 |
| 05 | Wicked Child (Upper Yard) | Stage 3 |
| 06–09 | Walking on the Edge · Heart of Fire · Out of Time · Nothing to Lose (Dracula's Quarters) | các màn sau |
| 10 | Poison Mind (Boss Theme) | boss thường |
| 11 | Black Night (Final Battle) | Dracula |
| 12 | Stage Clear | quả cầu đỏ |
| 13–14 | Game Complete · Voyager (Ending) | kết game |
| 15 | Death | mất mạng |
| 16 | Game Over | game over |
Tên file có khoảng trắng, ngoặc và dấu nháy (`Dracula's`) — cẩn thận khi import.

### Tóm tắt kiểm kê 4 sheet + Simon (đã mở xem `enemies_annot.png`, `bosses_annot.png`, `items_objects_annot.png`, `level1_entrance_annot_1.png`; JSON/ảnh chạy lại từ `Tools/cvgp/` y hệt byte bản planner)
| Sheet | Kích thước | Ô | Nhóm | Nền sheet / key ô | Bất thường |
|---|---|---|---|---|---|
| simon_belmont | 799×529 | 181 | 43 | (0,128,0) / (116,116,116) | 125 ô là hợp tile 8×16 (fill<1, chủ yếu roi); 12 ô cao >32 px (roi thò); 16 mảnh roi rời; 32 ô có lỗ key khép kín nhỏ |
| enemies | 474×190 | 71 | 16 | idem | 2 ô DÍNH LIỀN `enemy_dragonskull_0/1` (16×32 = 2 ô 16×16 chồng, gợi ý tách); zombie/fishman/blackknight tầng trên 16×16 có thể là nửa thân (chưa chắc); 3 ô 16×31 (đỉnh thấp 1 px) |
| bosses | 415×292 | 54 | 9 | idem; riêng **Medusa 2 ô nền đen thật** (không key-out) | 12 ô không đặc (Phantom Bat 2, Reaper 1, Dracula P1 2, Dracula P2 7); Dracula P2 cao tới 80 px; Death Effect 3 cỡ khối ghép sẵn; `boss_unused_*`, `boss_dracula2_7` hình lạ |
| items_objects | 251×135 | 65 | 10 | idem; **8 khối `obj_block_*` 8×16 không có xám** (đặc) | 1 ô DÍNH LIỀN `obj_brick_set` 32×16 = 4 khối 8×16; tên chưa chắc: `item_stopwatch`, 16 ô `obj_block_*/obj_lean_*`, `obj_purple_orb`, `obj_score_*` (mảnh chữ số) |
| level1_entrance | 3601×411 | 5 vùng đặc (castle_entrance 768×192, entrance_hall 1536×192, crypt_a 512×192, crypt_b 512×192, phantom_bat 768×192) + logo credit | — | (0,128,0) ngoài vùng; vùng là cảnh thật (không key) | 118 tile 8×8 duy nhất (113 sau gộp lật); 13 cầu thang, 3 hố, 2 đoạn nước; liên kết giữa vùng (c1,c3,c5,c6) suy từ bố cục — chắc trung bình/thấp |
Chung: cả 5 sheet đục hoàn toàn (alpha 255), 11–14 màu NES; chữ nhãn (255,255,255); `#004000` 516 px chỉ ở logo credit; **đen (0,0,0) là màu thật**. Không thấy chỗ sai trong `SHEETS_INVENTORY.md`; chỉ bỏ dòng ghi chú planner và sửa tên công cụ.
Công cụ kiểm kê dùng lại: `Tools/cvgp/` (`run_all.py` = `inv_sprites.py` + `inv_level1.py` + `make_inventory_md.py`; `sheetlib.py`, `sheet_specs.py`) — từ gốc repo: `python -P -E Tools/cvgp/run_all.py`; đầu vào `Art_Source/spriters-resource/*.png`, đầu ra `ClaudeNotes/cvgp/inventory/` (đổi bằng biến môi trường `CVGP_INV_OUT`). Thư mục `inventory/tools/` đã chuyển (git mv) sang đó.

Sprite đã cắt (CVTI): `Assets/Data/Art/{Title,Intro,Font,Character/Simon}`. Chưa có: tilemap, SFX định danh (routine 02).

## §4 Quyết định kỹ thuật đã chốt
| Quyết định | Giá trị | Lý do | Routine |
|---|---|---|---|
| Hiệu chuẩn Map_1 | sx 4.8585, sy 4.1605, ox 8.60, oy −22.55 | khớp glyph HUD 100 % | 00 (kiểm) |
| .gitignore | `ClaudeNotes/cvgp/_cache/` | ảnh full frame 28 MB + wiki thô (bản quyền) | 00 |
| Công cụ kiểm kê | `Tools/cvgp/` (git mv từ `inventory/tools/`); `sheetlib.SRC_DIR` = `Art_Source/spriters-resource`, `ORIGINAL_NAMES` giữ tên file tải gốc để JSON (`sheet_file`) y hệt bản planner; `CVGP_INV_OUT` đổi thư mục ra | truy vết + tái lập byte | 01 |
| Nhạc MP3 | nhập nguyên tên vào `Art_Source/music/` (16 file ~16 MB, commit vào git) | người dùng đã để chung thư mục Sounds; H2 không cần tải thêm | 01 |

## §5 Câu hỏi cho người dùng (chép từ CVGP_PLAN §9)
1. ~~Nhạc chưa có~~ — **đã có 16 MP3** ở `Art_Source/music/` (routine 01 tìm thấy trong `Downloads\NES - Castlevania - Sounds`). Còn lại: bài nào cho Stage 2/3 và boss (routine 02 đối chiếu video); không có bài "Life Lost" riêng (có `15. Death`).
2. Enemy/boss Block 2–6 không có video ⇒ routine 17–20 dựng theo wiki (gần đúng). Có video thì bỏ vào `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\`.
3. Kiểm tay gamepad sau routine 03: bật `CVGP/Input/Debug Overlay` trong Play, bấm lần lượt D-pad/cần trái/A/X/B/Y/Menu/View (đặc biệt D-pad Flydigi XInput).
4. HUD còn sprite TẠM (thanh máu/tim/khung + chữ số Famicom — CVTI_MEMORY §5 #16–17): routine 05 kiểm sheet Items & Objects; muốn tìm thêm sheet HUD bản US không?
5. `Map_1.mp4` là bản quay lại playthrough YouTube (có non-game ~1:55, nén); nếu có capture 60 fps thì đo nhịp khung chính xác hơn.
6. (từ CVTI) pixel vuông hay 8:7; Start có bỏ qua Intro không; attract/demo — cụm này giữ mặc định CVTI (vuông, không bỏ qua, không demo).
7. (mới, routine 00) Console ghi "Joystick disconnected (Controller (Flydigi VADER3))" ⇒ tay cầm báo tên VADER3 — routine 03 nên map theo JoystickButtonN/trục chứ không theo tên.

## §6 Nợ & việc ngoài phạm vi
- (CVTI) cần chạy lại CVTI 08 nghiệm thu khi Intro/Stage 1 đã dựng (CVTI_MEMORY §1) — ngoài phạm vi CVGP.
- Console có lỗi nền từ trước (Odin `PersistentContextCache` path1, GUIClips, Mathematics) — không thuộc cụm.
- `InputManager.asset` có nhiều mục tên "Debug" (lấy bằng grep `m_Name`) — routine 03 kiểm trước khi thêm trục.
- `_cache/00/f17_225.png` chưa mở xem riêng.

## §7 Nhật ký nghiệm thu

### Routine 00 — 11/10/2026
- **Đồ planner**: `CVGP_PLAN.md`, `ref/MAP1_TIMELINE.md` + `ref/nes/` (44 ảnh) + `ref/tools/`, `wiki/CV_WIKI_DATA.md`, `inventory/` đủ; `_cache/video/ref` (44 ảnh full frame) có ở máy. Đã mở xem: 3 montage Map_1 (16 frame trích mới), montage 12 ảnh `ref/nes/`, `enemies_annot.png`, `bosses_annot.png`, 1 frame full (boss chết).
- **Kiểm mẫu & hiệu chuẩn**: §2a/§2c — 0 lệch.
- **BASELINE project**: Unity 2022.3.62f3; HEAD trước routine `585d8ca`; `activeInputHandler: 0`; `Packages/manifest.json` 52 dòng; `Assets/Data/{Art,Editor,Prefabs/{Screens,UI},Resources,Script/{Anim,Audio,Character,Config,Intro,Stage1,System,Title,UI}}` — chưa có `Assets/Data/Audio`; scene `Assets/Scenes/Main.unity` (+ `Test/`). GameObject trong scene Main: **107 (Edit) / 157 (Play)**. Console trước Play: chỉ lỗi nền (Odin path1, GUIClips) + "Joystick disconnected"; trong Play không có error/warning mới.
- **Mốc luồng CVTI (H8)** — harness `execute_code` đăng ký `NesClock.OnSystemTick` (L0 = tick gọi `GoTo(Title,0)`, abs 737), `InjectStart` ở L351; log `CVTI.LogFlow`:
  ```
  [H] t0=737            (Title L0)
  [H] inject L351 abs=1088
  frame 1089 Title: Start                     = L352 ✔
  frame 1169 Title→Black (16f)                = L432 ✔
  frame 1185 Black→Intro (PlayMusic im lặng)  = L448 ✔
  frame 1433 Intro: Simon dừng, quay lưng     = L696 ✔
  frame 1627 Intro→Black (32f)                = L890 ✔
  frame 1659 Black→Stage1                     = L922 ✔
  ```
  Cả 5 mốc H8 khớp tuyệt đối. Ảnh đã mở xem (1920×1080): `_cache/shots/00/base_L10.png` (Title: logo + PUSH START KEY + copyright), `base_L600.png` (Intro: cổng, trăng, Simon, HUD), `base_L930.png` (Stage1Stub: nền đen + HUD + Simon idle).
- Cuối: `CVTI.LogFlow` = false, thoát Play, scene Main không dirty. 0 file game thay đổi.

### Routine 01 — 11/10/2026
- Chép 4 sheet nguyên byte vào `Art_Source/spriters-resource/` (`enemies.png` 18 354 B, `bosses.png` 19 791 B, `items_objects.png` 8 841 B, `level1_entrance.png` 83 940 B): sha256 **khớp cả 4** bảng planner; Simon/Cutscenes/Title còn khớp `SOURCES.md`. `SOURCES.md` thêm 4 dòng + ghi chú thư mục Sounds trùng byte + 16 MP3 nhạc.
- Kiểm kê: chạy `Tools/cvgp/run_all.py` (đầu ra tạm qua `CVGP_INV_OUT`) rồi chạy lại in-place: 14 JSON/PNG **y hệt md5** bản planner (git không báo thay đổi); `SHEETS_INVENTORY.md` chỉ khác dòng ghi chú planner (bỏ) + tên công cụ. Đã mở xem 4 ảnh annot + `level1_entrance_annot_1.png`: khung/đánh số đúng, khớp bảng §3.
- Nhạc: **phát hiện 16 MP3** (§3) — chép + sha256 khớp nguồn; không import Unity (routine 02).
- 0 file trong `Assets/`; không dùng Unity/MCP (việc thuần file). Thư mục tạm `D:\tmp_inv` (đầu ra so sánh) ngoài repo — có thể xoá tay.
