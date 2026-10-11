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
| 02 | cvgp-02-dinh-danh-37-sfx-qua-am-thanh-video | **XONG** | (commit "CVGP 02") | 11/10/2026 | 37 SFX: 13 chắc + 7 khả năng + 17 chưa định danh; nhập `Assets/Data/Audio/SFX` (PCM mono) + `eSfx` + `SfxLibrary` + `AudioManager.PlaySfx(eSfx)`; harness CVTI không đổi |
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

Sprite đã cắt (CVTI): `Assets/Data/Art/{Title,Intro,Font,Character/Simon}`. Chưa có: tilemap.

### SFX đã định danh (routine 02, 11/10/2026)
**Cách làm:** âm thanh 2 video (`ffmpeg -vn -ac 1 -ar 22050` → `_cache/02/map1.wav`, `boss.wav`) đối chiếu 37 SFX bằng `Tools/cvgp/sfx_match.py` (NCC phổ STFT 500–8000 Hz + NCC dạng sóng; loại đoạn non-game 1:47.7–1:54.5; chạy lại ra y hệt từng byte), `sfx_events.py` (làm giàu theo loại sự kiện HUD/roi lấy từ `_cache/video/work`), `sfx_view.py` (vẽ phổ), `sfx_frames.py` (trích khung quanh mốc), `sfx_describe.py` (md5/thời lượng/âm học). Mọi tên "chắc" và "khả năng" đã được xem khung hình quanh mốc (`_cache/02/fr_*.png`, phổ `seg_*.png`). **Tổng: 13 chắc + 7 khả năng + 17 chưa định danh = 37.**
*Thang tin cậy:* chắc = khớp ≥ 2 sự kiện cùng loại + đã xem khung; khả năng = 1 sự kiện hoặc gián tiếp; chưa định danh = không đỉnh nổi (không đoán tên).
*Giới hạn:* SFX noise ngắn (vung roi, bị thương, nhảy/đáp, pause…) bị trống/nhạc nền noise của game che — NCC cho đỉnh ngang nhau cả nhóm `02/03/08/10/13/16/17/19/37`; không gán tên. `20` khớp cả "roi trúng chậu lửa" lẫn "hạ quái" nên có thể là âm "trúng đòn" chung. Điểm: spec = NCC phổ (0–1), z = (điểm − median)/MAD, wave = NCC dạng sóng (0.25–0.5 trên video nén AAC đã là khớp).

| # | File gốc → file Unity | Thời lượng | md5 | Tên | `eSfx` | Độ tin cậy | Bằng chứng (thời điểm video, mô tả khung, điểm) | Đặc điểm phát / âm học |
|---|---|---|---|---|---|---|---|---|
| 01 | `01.wav` → `sfx_01_unknown.wav` | 0.56 s | `89286c306a599b201892e808e6193ace` | `eSfx.Unknown01` | chưa định danh | không có đỉnh nổi (spec ≤0.30, z≤4; wave ≤0.18) | pulse 3 bước cao độ rồi giữ ~1 kHz; có thể jingle (1UP/boss chết?) — chưa kiểm chứng |
| 02 | `02.wav` → `sfx_02_unknown.wav` | 0.10 s | `606648a8fa95f5dd2a9414713b645b0c` | `eSfx.Unknown02` | chưa định danh | click ngắn nhiễu; chỉ wave 0.62 tại 0:02.46 (khung đen/Simon vừa hiện) và 0.52 tại 1:35.65, không thấy sự kiện | noise 0.10 s |
| 03 | `03.wav` → `sfx_03_unknown.wav` | 0.19 s | `9a75d5ee09ed23c47371b20a5062f9ab` | `eSfx.Unknown03` | chưa định danh | wave 0.75 tại 1:01.15 / 0.63 tại 1:26.09 nhưng khung hình chỉ Simon đi bộ (trùng trống nhạc nền) | noise 0.19 s, ~1.5 kHz |
| 04 | `04.wav` → `sfx_04_tally_time.wav` | 0.37 s | `6790b0bb857deb44ea858e32ce03ef73` | `eSfx.TallyTime` | chắc | 3:39.49 spec 0.92 z8 wave 0.81 (TIME 0001→0000, khung 219.49 đã xem) + video boss 24.78 s (đếm TIME, spec 0.51 wave 0.33) | phát lại mỗi ~8 tick (0.135 s) suốt lúc đếm; bản cuối phát trọn 0.37 s ngay sau khi TIME về 0 (đo trên phổ 217.4–219.5 s) |
| 05 | `05.wav` → `sfx_05_tally_heart.wav` | 0.55 s | `83afb2e26bbefba904655004b4bf0f8b` | `eSfx.TallyHeart` | chắc | 3:49.94 spec 0.95 z12 wave 0.86 (tim 01→00, khung 229.94 đã xem); 67/68 mốc đếm tim p>0.95 | lặp mỗi ~8 tick (0.132 s) trong 3:40.5–3:49.9; bản cuối phát trọn (đuôi giữ 1 cao độ ~0.4 s) |
| 06 | `06.wav` → `sfx_06_tally_heart_alt.wav` | 0.54 s | `a04b8464a334b390587e4d203e9ef3b9` | `eSfx.TallyHeartAlt` | khả năng | 3:49.95 spec 0.82 wave 0.70 — cùng thời điểm với 05 (không tách được 05/06 trong 1 sự kiện) | tonal, đầu giống 05; có thể bản dự phòng/kênh khác của đếm tim |
| 07 | `07.wav` → `sfx_07_map_step.wav` | 0.11 s | `ddffd586c2a03c25829497325655eaba` | `eSfx.MapStep` | chắc | bản đồ lâu đài 3:55.3–3:58.8: 14/14 nhịp tick (cách 0.267 s = 16 tick) wave 0.26–0.45; khung 235.85 đã xem (chấm Simon nhấp nháy trên bản đồ) | click noise 0.11 s lặp mỗi 16 tick; không có nhạc chồng |
| 08 | `08.wav` → `sfx_08_unknown.wav` | 0.18 s | `0d14463468a6a60aa33bd32a5270af53` | `eSfx.Unknown08` | chưa định danh | spec ≤0.59 z3, không gắn sự kiện | noise 0.18 s thấp |
| 09 | `09.wav` → `sfx_09_throw_dagger.wav` | 0.18 s | `a46679ec13b868ba2b78a908699e52fc` | `eSfx.ThrowDagger` | chắc | 1:32.83 +0.26 s (spec 0.69, wave 0.13) và 1:46.83 +0.18 s (spec 0.70, wave 0.17) — hai đỉnh cao nhất toàn video; khung 92.83/106.83 đã xem (tim −1, dao bay ra) | noise 0.18 s; trễ ~0.2 s sau lúc HUD trừ tim; ném rìu (3:22–3:30) không tách được vì chồng boss+nhạc |
| 10 | `10.wav` → `sfx_10_unknown.wav` | 0.25 s | `d50a2e822557e3284fade599e9e1adea` | `eSfx.Unknown10` | chưa định danh | spec ≤0.64 z5, không gắn sự kiện | noise 0.25 s |
| 11 | `11.wav` → `sfx_11_unknown.wav` | 2.05 s | `7acc49bf4e2ab9543c1d887f2b284193` | `eSfx.Unknown11` | chưa định danh | không có đỉnh (z≤3) — không xuất hiện trong 2 video | tonal trượt giảm chậm 2.05 s — có thể mất mạng/game over; chưa kiểm chứng |
| 12 | `12.wav` → `sfx_12_unknown.wav` | 1.64 s | `5776c93a5b2f1b254fcf896415223778` | `eSfx.Unknown12` | chưa định danh | không có đỉnh (z≤4) — không xuất hiện trong 2 video | tonal trượt giảm 1.64 s, tương tự 11 |
| 13 | `13.wav` → `sfx_13_unknown.wav` | 0.13 s | `7a3654f098818626fea0ce36c9ac60ae` | `eSfx.Unknown13` | chưa định danh | spec ≤0.73 z4, wave ≤0.31 rải rác (trùng trống nhạc); có xu hướng lặp sau cú roi trên không (+0.2 s) nhưng không đủ chắc | noise 0.13 s |
| 14 | `14.wav` → `sfx_14_fish_jump.wav` | 0.73 s | `b9dba53b274a45c81955a70c874796d2` | `eSfx.FishJump` | chắc | 1:57.17 spec 0.61 (2 Fish Man trồi khỏi nước, bọt tím), 2:06.45, 2:25.67 — khung đã xem cả 3 | noise 0.73 s, 1 lần mỗi Fish Man |
| 15 | `15.wav` → `sfx_15_fish_splash.wav` | 1.01 s | `99fffc9ce0eb5c009166629b36afe411` | `eSfx.FishSplash` | chắc | 2:11.37 spec 0.58, 2:27.12 spec 0.60 — Fish Man rơi lại mặt nước, bọt tím (khung 131.37/147.12 đã xem) | noise 1.01 s, khớp trong vùng nước Crypt B |
| 16 | `16.wav` → `sfx_16_unknown.wav` | 0.40 s | `1abc4e93946dd7cf1dc106625ae456c6` | `eSfx.Unknown16` | chưa định danh | z≤3 | noise 0.40 s |
| 17 | `17.wav` → `sfx_17_unknown.wav` | 0.40 s | `4a194ce5b6f24676ad70bf1597b43094` | `eSfx.Unknown17` | chưa định danh | z≤3 (thấp ở mọi sự kiện đánh trúng nhưng không nổi) | noise 0.40 s |
| 18 | `18.wav` → `sfx_18_door_alt.wav` | 0.79 s | `eef154aadec729a1f050499c5bc3ca42` | `eSfx.DoorAlt` | khả năng | 1:27.41, 1:29.02, 2:46.35, 2:47.98 (spec 0.45–0.55) trùng 4 mốc cửa như 29 | noise có tiếng trầm, biến thể của 29 |
| 19 | `19.wav` → `sfx_19_unknown.wav` | 0.45 s | `40de178a75ffc588ebd4390850bb8c3a` | `eSfx.Unknown19` | chưa định danh | z≤4 | noise 0.45 s |
| 20 | `20.wav` → `sfx_20_hit_flame.wav` | 0.36 s | `8c21631e10b3433efd50e363d2018ed0` | `eSfx.HitFlame` | chắc | roi trúng chậu lửa 0:03.35 và 0:09.05 (spec 0.66/0.64; khung đã xem: tia lửa đỏ bung) + các lần hạ quái (+200/+300…: spec 0.55–0.67, p>0.99 ở 9/13 lần) | noise 0.36 s; đến ~0.03–0.2 s sau khung roi duỗi |
| 21 | `21.wav` → `sfx_21_break_wall.wav` | 0.60 s | `6b49d959cd96dad37df1b9716af90687` | `eSfx.BreakWall` | chắc | 1:39.61 spec 0.62 (đập tường gạch, mảnh tím), 3:17.62 spec 0.67 (roi vỡ nến → item II), 2:19.05 spec 0.67 (vỡ gạch, mảnh tím) — khung đã xem cả 3 | noise 0.60 s |
| 22 | `22.wav` → `sfx_22_pickup_heart.wav` | 0.50 s | `95d7ab928bd31a379eeead4de40cc1a1` | `eSfx.PickupHeart` | chắc | 33/38 lần nhặt tim nhỏ/lớn (p≥0.995, spec 0.56–0.75) + video boss 1.45 s (z12); khung 3.93 đã xem (tim 05→10) | vòng cung hài ba 0.50 s; trong lúc phát, vạch nhạc nền biến mất khỏi phổ — xem `_cache/02/seg_a.png` |
| 23 | `23.wav` → `sfx_23_pickup_bag.wav` | 1.04 s | `a32b044451da616aa3c9b7953718a2eb` | `eSfx.PickupBag` | chắc | túi 700 (5/5 mốc), 400 (0:21.83), 1000 (2:23.80): spec 0.52–0.65; khung 21.83 đã xem (túi hồng biến mất, SCORE +400) | vòng cung hài ba 1.04 s |
| 24 | `24.wav` → `sfx_24_pickup_weapon.wav` | 0.75 s | `64873fb6c4200ddfd5fd538aebf5c30a` | `eSfx.PickupWeapon` | chắc | nhặt roi #1 0:06.50, roi #2 0:09.78, dao 0:15.55 (khung 6.50 Simon nháy tím, 15.50 dao hiện ở HUD), rìu 3:10.67, nhặt II video boss 7.11 s — spec 0.52–0.61 | vòng cung hài ba 0.75 s; đi kèm Simon đóng băng 0.75 s khi nhặt roi |
| 25 | `25.wav` → `sfx_25_enter_gate.wav` | 1.22 s | `045b425faf36752acb75452c17a42740` | `eSfx.EnterGate` | khả năng | 0:17.82 spec 0.68 z7 wave 0.17 (Simon bước vào cổng lâu đài, khung 17.35–18.35 đã xem) — chỉ 1 sự kiện | 1.22 s, nhiều bước cao độ |
| 26 | `26.wav` → `sfx_26_unknown.wav` | 2.40 s | `c7496a7187593fa952dc2cd10468e384` | `eSfx.Unknown26` | chưa định danh | không có đỉnh nổi (chỉ nhận nhầm tick bản đồ); 3 xung rời nhau | tonal thấp ~107 Hz 2.40 s, 3 xung — chưa kiểm chứng |
| 27 | `27.wav` → `sfx_27_potion_invisible.wav` | 0.83 s | `82aebea8d7f85927fae9d182c2877c13` | `eSfx.PotionInvisible` | khả năng | 2:58.20 spec 0.71 z8 (Simon nhặt bình, khung 177.9–178.2 đã xem) — 1 sự kiện | 0.83 s dạng xung đều |
| 28 | `28.wav` → `sfx_28_invisible_end.wav` | 0.83 s | `200a739475b17ea59683b6e00ce5d6fe` | `eSfx.InvisibleEnd` | khả năng | 3:02.11 spec 0.49 z7 (Simon hiện lại, khung 181.95–182.25 đã xem) — 1 sự kiện | 0.83 s dạng xung đều, giống 27 |
| 29 | `29.wav` → `sfx_29_door.wav` | 0.81 s | `b28d71a8d634148dd5c265154aac303b` | `eSfx.Door` | chắc | cửa cuối màn: 1:27.36, 1:28.94, 2:46.38, 2:47.96 (spec 0.57–0.64; khung 87.21–89.4 đã xem: cửa đỏ mở rồi đóng) | 0.81 s; 1 lần lúc mở, 1 lần lúc đóng (cách 1.6 s) |
| 30 | `30.wav` → `sfx_30_holy_water.wav` | 1.16 s | `4bca08eb62ebb476646b9481cbf6ec2d` | `eSfx.HolyWater` | khả năng | chỉ video boss: 6.31, 8.61, 11.60, 13.97, 16.47, 18.93 s (spec 0.66, wave 0.30–0.38), nhịp ≈ 2.4 s trùng chu kỳ ném nước thánh (khung đã xem); chưa phân biệt ném hay vỡ/cháy | 1.16 s, bước cao độ rồi nhiễu |
| 31 | `31.wav` → `sfx_31_holy_water_alt.wav` | 1.15 s | `df7851a075c7a3b930e1a9d76625ae85` | `eSfx.HolyWaterAlt` | khả năng | trùng thời điểm 30 trong video boss (spec 0.5, wave thấp) | 1.15 s; mở đầu bằng nhiễu |
| 32 | `32.wav` → `sfx_32_unknown.wav` | 2.14 s | `43a5ed9ae847f29d460d2beecb37f391` | `eSfx.Unknown32` | chưa định danh | không có đỉnh nổi | noise 2.14 s thấp, phức hợp (có thể boss chết/Dracula) — chưa kiểm chứng |
| 33 | `33.wav` → `sfx_33_unknown.wav` | 0.50 s | `805f2b56df493e169e3e599fe4b01d53` | `eSfx.Unknown33` | chưa định danh | không có đỉnh | 4 nhịp bíp đều 0.50 s ~2 kHz — có thể pause; video không có pause |
| 34 | `34.wav` → `sfx_34_unknown.wav` | 1.54 s | `1f98ad45ac7abaf6121fed1519108522` | `eSfx.Unknown34` | chưa định danh | spec ≤0.51 z4 | 1.54 s, nhiều pha |
| 35 | `35.wav` → `sfx_35_rosary.wav` | 1.62 s | `d369a58fb3b1827cc21e129f535b3dcb` | `eSfx.Rosary` | chắc | 1:15.02 (z9), 1:24.69 (z10), 2:42.76 (z7): spec 0.47–0.64; khung 75.02/84.69/84.79/162.76 đã xem: màn nháy trắng | 1.62 s; kèm nháy trắng 0.40–0.44 s |
| 36 | `36.wav` → `sfx_36_unknown.wav` | 0.66 s | `58a5d963ca7d08d9fc6d61d517b839d6` | `eSfx.Unknown36` | chưa định danh | chỉ khớp trong khoảng đếm TIME (trùng 04), z≤4 | pulse bước cao độ 0.66 s |
| 37 | `37.wav` → `sfx_37_unknown.wav` | 0.11 s | `f002d9b8c4499b0117612cd865e1a2b0` | `eSfx.Unknown37` | chưa định danh | spec ≤0.69 z3, wave 0.2–0.33 rải rác | tonal ngắn 0.11 s ~236 Hz |

**Đặc điểm phát đo được (NES):**
- *Lặp theo nhịp:* đếm TIME (`04`) và đếm tim (`05`) **kích lại mỗi ~8 tick** (0.132–0.135 s) dù giá trị đổi mỗi tick — chỉ đoạn đầu mỗi lần nghe được (lần sau cắt lần trước); lần kích cuối phát trọn (`04` 0.37 s sau TIME=0, `05` đuôi giữ ~0.4 s sau tim=0). Tick bản đồ (`07`) cách 16 tick (0.267 s), không chồng. Cửa (`29`): 1 lần lúc mở, 1 lần lúc đóng, cách 1.6 s.
- *SFX mới cắt SFX cũ (kênh đơn):* đúng ở các ca lặp trên. **Unity `PlayOneShot` KHÔNG cắt** (chồng lên) ⇒ SFX lặp phải tự `Stop`-rồi-phát hoặc thêm API riêng — chưa làm (ngoài phạm vi, §6).
- *Nhạc bị tắt kênh khi SFX phát:* trên phổ (`seg_a.png`) lúc nhặt tim (`22`, 3.98–4.4 s) và nhặt vũ khí (`24`, 6.5–7.3 s) vạch nhạc nền biến mất tới khi SFX kết thúc ⇒ game gốc có vẻ **tạm tắt kênh nhạc** khi SFX nhặt đồ phát. Nhạc chưa có ở Unity; ghi cho routine nhạc.
- *Độ trễ so với sự kiện hình:* `09` ném dao trễ ~0.2 s sau lúc HUD trừ tim; `20` ~0.03–0.2 s sau khung roi duỗi; nhặt đồ (`22/23/24`) +0.0–0.07 s sau HUD đổi.
- *Không có SFX ở Title* (CVTI) — vẫn đúng: log luồng không có `PlaySfx` trước Stage.

**Nhập Unity:** 37 file `Assets/Data/Audio/SFX/sfx_<NN>_<ten>.wav` (byte y hệt `Art_Source/sounds-resource/393996/NN.wav`; `cmp` 1 mẫu + md5 bảng trên), import PCM · Force To Mono · Decompress On Load · Preserve Sample Rate · preload (preset `Assets/Data/Editor/SfxImporter.cs` — chỉ khi `importSettingsMissing`; kiểm bằng API: 37/37 đúng). `Assets/Data/Script/Audio/eSfx.cs` (giá trị = số file gốc, `None = 0`), `SfxLibrary.cs` (ScriptableObject `Assets/Data/Audio/SfxLibrary.asset`, 37 mục, dựng bằng menu `CVGP/Build SfxLibrary` = `SfxLibraryBuilder.cs`, chạy lại được), `AudioManager.PlaySfx(eSfx)` + `IsSfxPlaying` (giữ nguyên `PlaySfx(AudioClip)`); tham chiếu `sfxLibrary` serialized trên `Assets/Data/Prefabs/GameManager.prefab` (diff 1 dòng).


## §4 Quyết định kỹ thuật đã chốt
| Quyết định | Giá trị | Lý do | Routine |
|---|---|---|---|
| Hiệu chuẩn Map_1 | sx 4.8585, sy 4.1605, ox 8.60, oy −22.55 | khớp glyph HUD 100 % | 00 (kiểm) |
| .gitignore | `ClaudeNotes/cvgp/_cache/` | ảnh full frame 28 MB + wiki thô (bản quyền) | 00 |
| Công cụ kiểm kê | `Tools/cvgp/` (git mv từ `inventory/tools/`); `sheetlib.SRC_DIR` = `Art_Source/spriters-resource`, `ORIGINAL_NAMES` giữ tên file tải gốc để JSON (`sheet_file`) y hệt bản planner; `CVGP_INV_OUT` đổi thư mục ra | truy vết + tái lập byte | 01 |
| Nhạc MP3 | nhập nguyên tên vào `Art_Source/music/` (16 file ~16 MB, commit vào git) | người dùng đã để chung thư mục Sounds; H2 không cần tải thêm | 01 |
| Định danh SFX | tương quan phổ + dạng sóng 2 video, bằng chứng theo sự kiện HUD, xác nhận bằng khung hình; ca noise ngắn bị nhạc che ⇒ `UnknownNN` (không đoán) | H2/H10: không bịa tên | 02 |
| `eSfx` | giá trị enum = số file gốc (`None = 0`) | truy vết 1-1 về `NN.wav` | 02 |
| Tên "khả năng" giữ trong enum (`DoorAlt`, `TallyHeartAlt`, `HolyWaterAlt`) | nhóm biến thể không tách được trong cùng sự kiện | routine sau thử cả hai khi nghe thử | 02 |
| `PlaySfx(eSfx)` | `PlayOneShot` trên 1 `AudioSource` (chồng, không cắt) | giữ API cũ; SFX lặp/cắt đưa vào nợ §6 | 02 |

## §5 Câu hỏi cho người dùng (chép từ CVGP_PLAN §9)
1. ~~Nhạc chưa có~~ — **đã có 16 MP3** ở `Art_Source/music/` (routine 01 tìm thấy trong `Downloads\NES - Castlevania - Sounds`). Còn lại: bài nào cho Stage 2/3 và boss (routine 02 đối chiếu video); không có bài "Life Lost" riêng (có `15. Death`).
2. Enemy/boss Block 2–6 không có video ⇒ routine 17–20 dựng theo wiki (gần đúng). Có video thì bỏ vào `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\`.
3. Kiểm tay gamepad sau routine 03: bật `CVGP/Input/Debug Overlay` trong Play, bấm lần lượt D-pad/cần trái/A/X/B/Y/Menu/View (đặc biệt D-pad Flydigi XInput).
4. HUD còn sprite TẠM (thanh máu/tim/khung + chữ số Famicom — CVTI_MEMORY §5 #16–17): routine 05 kiểm sheet Items & Objects; muốn tìm thêm sheet HUD bản US không?
5. `Map_1.mp4` là bản quay lại playthrough YouTube (có non-game ~1:55, nén); nếu có capture 60 fps thì đo nhịp khung chính xác hơn.
6. (từ CVTI) pixel vuông hay 8:7; Start có bỏ qua Intro không; attract/demo — cụm này giữ mặc định CVTI (vuông, không bỏ qua, không demo).
8. (mới, routine 02) 17 SFX chưa định danh (`01 02 03 08 10 11 12 13 16 17 19 26 32 33 34 36 37`) — gồm vung roi, bị thương, nhảy/đáp, pause, mất mạng, game over, 1UP, boss chết… Muốn gán tên chắc: người dùng nghe thử từng file (hoặc quay clip có các sự kiện đó: bị thương, chết, pause, 1UP) rồi cho biết; hoặc `cvgp-fix` gắn tên khi các routine gameplay có sự kiện tương ứng.
7. (mới, routine 00) Console ghi "Joystick disconnected (Controller (Flydigi VADER3))" ⇒ tay cầm báo tên VADER3 — routine 03 nên map theo JoystickButtonN/trục chứ không theo tên.

## §6 Nợ & việc ngoài phạm vi
- (CVTI) cần chạy lại CVTI 08 nghiệm thu khi Intro/Stage 1 đã dựng (CVTI_MEMORY §1) — ngoài phạm vi CVGP.
- Console có lỗi nền từ trước (Odin `PersistentContextCache` path1, GUIClips, Mathematics) — không thuộc cụm.
- `InputManager.asset` có nhiều mục tên "Debug" (lấy bằng grep `m_Name`) — routine 03 kiểm trước khi thêm trục.
- `_cache/00/f17_225.png` chưa mở xem riêng.
- (02) **16 file nhạc MP3 chưa import Unity** — §3 (routine 01) ghi "routine 02 làm" nhưng đề routine 02 chỉ cho kiểm `Art_Source/music/`, không xử lý nhạc (H9) ⇒ chưa làm; cần routine/việc riêng (import `Assets/Data/Audio/Music/`, preset, `eMusic` + bảng, gắn Intro/Stage; Title im lặng). Tên file có khoảng trắng + dấu nháy.
- (02) `AudioManager.PlaySfx(eSfx)` dùng `PlayOneShot` (chồng); SFX lặp (đếm tally mỗi 8 tick) cần "SFX mới cắt SFX cũ" — routine 12 (tally) nên thêm.
- (02) `Tools/cvgp/__pycache__/` mới sinh — không commit. `_cache/02/scores*.npz` ~1.5 GB (xoá tay được).


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

### Routine 02 — 11/10/2026
- **Kết quả:** 37 SFX → 13 chắc (`TallyTime TallyHeart MapStep ThrowDagger FishJump FishSplash HitFlame BreakWall PickupHeart PickupBag PickupWeapon Door Rosary`) + 7 khả năng (`TallyHeartAlt DoorAlt EnterGate PotionInvisible InvisibleEnd HolyWater HolyWaterAlt`) + 17 chưa định danh (bảng §3). Mọi dòng chắc/khả năng có thời điểm video + khung đã xem.
- **Ảnh/phổ đã mở xem** (`ClaudeNotes/cvgp/_cache/02/`): `sfx_montage.png` (phổ 37 SFX), `seg_tally_time.png`, `seg_tally_heart.png`, `seg_a.png`, `seg_map.png`, `seg_boss1.png`; khung `fr_03.png` (đối chứng nhiễu: Simon đi bộ), `fr_09.png`, `fr_14.png`, `fr_15.png`, `fr_21.png`, `fr_25.png`, `fr_2728.png`, `fr_door.png`, `fr_35.png`, `fr_pick.png`, `fr_misc.png`, `fr_30.png`; `shots/02/stage1_after_sfx.png` (Stage1Stub sau test, giống baseline).
- **Chạy lại:** `python -P -E Tools/cvgp/sfx_match.py` (ra `_cache/02/matches.json|txt`; chạy 2 lần `diff` y hệt); cần `_cache/02/{map1,boss}.wav` từ ffmpeg (lệnh ở đầu `sfx_match.py`).
- **Unity:** 37 clip import đúng thiết lập (API: mono, DecompressOnLoad, PCM, PreserveSampleRate — 37/37); `SfxLibrary` 37 mục, 0 id thiếu clip. Lần compile đầu lỗi (`AudioImporter.preloadAudioData` obsolete) nên 37 WAV import trước khi postprocessor có ⇒ đã xoá `.meta` mới sinh của 37 WAV và nhập lại để preset thực sự chạy; sửa dùng `AudioImporter.defaultSampleSettings.preloadAudioData`.
- **Play (Main.unity):** `PlaySfx(PickupHeart/Rosary/Unknown37/None)` — không lỗi, `IsSfxPlaying = true`. **Harness H8** (`GoTo(Title,0)` ở abs 852, `InjectStart` L351): Start L352 · đen L432 · Intro L448 · Simon dừng L696 · đen L890 · Stage1 L922 — **khớp tuyệt đối**; không `PlaySfx` nào ở Title. Console chỉ có lỗi nền (Odin path1, GUIClips). Cuối: thoát Play, `CVTI.LogFlow` = false, scene Main không dirty.
