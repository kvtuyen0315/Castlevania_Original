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
| 03 | cvgp-03-input-ban-phim-gamepad-xbox-va-replay | **XONG** | (commit "CVGP 03") | 13/10/2026 | `NesPad` + `InputBindings.asset` + 4 trục `CV_*` + replay RLE + menu `CVGP/Input/*`; 5 kiểm đạt (§7); H8 khớp tuyệt đối; gamepad thật chưa bấm thử (không cắm tay cầm lúc chạy — §5 #3) |
| 04 | cvgp-04-bang-chi-so-config-moi-nhan-vat-va-item | **XONG** | (commit "CVGP 04") | 13/10/2026 | 12 CSV có nguồn → 12 bảng ScriptableObject + `GameConfig` (181 dòng): 15+3 enemy, 6 boss (7 pha), 22 item + 3 vật chứa, 5 vũ khí phụ, 3 roi, 6 block, 18 stage; menu `CVGP/Import Config` (validate, idempotent) + `CVGP/Export Stats Report` → `CHARACTER_STATS.md`; `GameConfig` trên prefab GameManager; H8 khớp |
| 05 | cvgp-05-cat-sprite-simon-enemy-boss-item-animset | **XONG** | (commit "CVGP 05") | 13/10/2026 | 246 PNG từ manifest `Tools/cvgp/slices_gameplay.json` (238/371 ô kiểm kê + 2 nửa tách + 6 bản đồ lâu đài; 133 ô bỏ có lý do); roi tách lớp 96/96 khung ghép = thân + roi **0 pixel lệch**; 28 AnimSet / 135 anim; ảnh lưới Play 2 trang 0 lệch; CVTI md5 91 file y hệt; H8 khớp |
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

Sprite đã cắt (CVTI): `Assets/Data/Art/{Title,Intro,Font,Character/Simon}`; gameplay (routine 05): xem mục "Sprite gameplay đã cắt" bên dưới. Chưa có: tilemap.

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

### Sprite gameplay đã cắt (routine 05, 13/10/2026) — routine 06+ đọc bảng này, KHÔNG mở lại sheet
**Nguồn sự thật:** `Tools/cvgp/slices_gameplay.json` (mục `slices` 246, `anims` 135, `palettes` 4, `skipped` 133 — mỗi mục có `src` = id kiểm kê planner + `note`). Cắt: `python -P -E Tools/cvti/slice_sprites.py --manifest Tools/cvgp/slices_gameplay.json` (chạy 2 lần md5 y hệt). Kiểm: `python -P -E Tools/cvgp/verify_sprites.py --meta`. AnimSet: menu `CVGP/Build AnimSets` → `Assets/Data/Config/Anim/<Set>AnimSet.asset`. Mọi sprite 1 PNG Single, RGBA, trong suốt (0,0,0,0), key xám (116,116,116) (+ nền xanh trong bbox nếu `bgIsKey`), **giữ đen**. Pivot `[px,py]` tính từ góc TRÊN-TRÁI sprite (đặt `ToWorld(x_pivot, y_pivot)`). Sprite vẽ **quay TRÁI** như sheet.

| Nhóm | Số | Thư mục (`Assets/Data/Art/…`) | Pivot | Ghi chú |
|---|---|---|---|---|
| Simon thân | 36 mới (+5 CVTI giữ nguyên byte: `idle_0 walk_0..2 back_0`) | `Character/Simon` | chân: `[8,32]` (16 rộng); `use_*_0/1` `[8,32]`, `use_*_2..5` `[16,32]` (24 rộng, chân ở 16 px phải); `death_1` 32×16 `[16,16]` (quy ước) | `jumpduck_0` (nhảy = ngồi), `stairs_up_0/1`, `stairs_down_0/1`, `hurt_0`, `death_0/1`, `color_1..4` (mẫu bảng màu), `use_{stand,duck,stairs_up,stairs_down}_{0..5}` (thân vung roi/ném: 0 chuẩn bị, 1 vắt, 2 duỗi; 3/4/5 = 2 có chuôi roi tím/xanh/đỏ cho L3 màu 2/3/4) |
| Mảnh roi | 16 | `Character/Simon/Whip` | `[0,0]` | `whip_l1_{hang,over,ext}`, `whip_l2_ext`, `whip_l3c{1..4}_{hang,over,ext}`; L2 treo/vắt dùng `l3c2` |
| Vũ khí phụ | 14 | `SubWeapon` | tâm; lửa nước thánh đáy-giữa | `dagger_0`, `cross_0`, `cross_spin_0/1`, `axe_0..3`, `holywater_bottle`, `holywater_fire_0..4` |
| FX | 15 | `FX` | tâm | `hit_0`, `death_0..2` (enemy), `bossdeath_{s,m,l}_{0..2}`, `water_bubble`, `brick_debris` (= `obj_purple_orb`, xác nhận video 1:39.6) |
| Enemy (15 loại) | 58 | `Enemy/<Loại>` | đứng đất: đáy-giữa (Zombie, BlackLeopard, FishMan, BlackKnight, Fleaman, WhiteSkeleton, RedSkeleton, AxeMan, DragonSkull `[8,16]`); bay + đạn: tâm | Zombie 2 · BlackLeopard 4 · VampireBat 3 đỏ + 4 tím · FishMan 4 + `fishman_piece_0` + cầu lửa · BlackKnight 3 · Ghost 2 · DragonSkullCannon 2×(trên,dưới) + cầu lửa · MedusaHead 2 · Fleaman 2 · Raven 4 · WhiteSkeleton 2 + xương 3 · Eagle 2 · SkeleDragon đầu 2 + đốt + cầu lửa · RedSkeleton 4 · AxeMan 2 + rìu 4 |
| Boss | 43 | `Boss/{PhantomBat,Medusa,Mummy,Frankenstein,Death,Dracula}` | đứng đất đáy-giữa; bay/đạn tâm | PhantomBat 3 · Medusa: `medusa_bust_0/1` (**nền đen thật, không key**), `medusa_head`, `medusa_stone`, `snake_0/1` · Mummy tím 3 + đỏ 3 + băng 2 · Frankenstein 3 + Igor 2 + cầu lửa · Death 40×48 + lưỡi hái 4 · Dracula P1 6 (thân không đầu 2, đầu rời, cầu lửa, thân có đầu 2) + P2 7 + `dracula2_spark` + cầu lửa |
| Item | 29 + 16 Moai | `Item`, `Item/Moai` | đáy-giữa | roi, tim nhỏ/lớn, túi đỏ/tím/trắng, `crystal_0/1`, dao/rìu/nước thánh/thập tự (+2 khung xoay), `stopwatch`, `double`, `triple`, `roast`, `jar`, `rosary`, `1up`, `crown_0..3`, `chest_0..3`; Moai = 8 khối 8×16 đặc + 8 đầu nghiêng (nhận diện = SUY LUẬN) |
| Object | 13 | `Object` | nến/đuốc đáy-giữa; cửa + số điểm góc trên-trái; brick tâm | `candle_0/1`, `torch_0/1`, `door_0` (16×48), `door_1` (24×48), `score_{10,20,40,70,0,00}` (ghép ngang ra 100/200/400/700/1000/2000/4000/7000), `brick_0` (4 khối giống hệt — chưa rõ công dụng) |
| Bản đồ lâu đài | 6 | `CastleMap` | góc trên-trái | `castlemap_bg` 384×144 tại sheet (262,808) **đục** · `castlemap_marker` 8×16 (262,953) · `castlemap_bat_0..3` 16×16 (271+17k, 953) — đo bằng thành phần liên thông |
| **Tổng** | **246** | | | |

**Không cắt (133, lý do trong `skipped`):** 96 khung roi ghép sẵn (dựng lại 100 % từ thân + roi) · 12 mảnh thân `simon_whip*_4` (trùng cột chân `use_*_2..5`) · 5 ô Simon đã có ở CVTI · `simon_color_0` (= `idle_0`) · 11 mảnh 16×16 Zombie/FishMan/BlackKnight (trùng từng pixel nửa khung 16×32; riêng `enemy_fishman_4` KHÔNG trùng ⇒ đã cắt `fishman_piece_0`) · 8 ô nhãn "Unused" (`boss_unused_0/1`, `item_unused_*` 6). 238 ô cắt + 133 = **371/371**.
**HUD:** không sheet nào có thanh máu/tim/khung vũ khí phụ/chữ số US bản gốc (Items & Objects, Cutscenes đã xem) ⇒ HUD vẫn dùng sprite TẠM của CVTI; `item_heart_small` là item rơi, không phải icon HUD.
**Ending:** Cutscenes còn khối lâu đài 56×64 tại (262,728) + 4 ô 128×72 (credit/ending) — không thuộc gameplay, không cắt.

**Roi tách lớp (Simon) — `Tools/cvgp/whip_layers.py`:** mỗi khung roi ghép sẵn trên sheet = thân `use_<tư thế>_k` + 1 mảnh roi, **đúng 1 nghiệm/khung, 96/96 khung 0 pixel lệch**. Offset = góc trên-trái mảnh roi − pivot thân (px NES, y xuống, quay trái) — lưu ở `overlayOffsets` của anim `whip_<cấp>_<tư thế>` (frames = 3 thân, overlays = 3 mảnh):
| Khung | Thân | Mảnh | Offset đứng/ngồi/xuống thang | Offset lên thang |
|---|---|---|---|---|
| 0 chuẩn bị | `use_*_0` | `hang` (8×32) | (16, −32) | (16, −30) |
| 1 vắt | `use_*_1` | `over` (16×24) | (8, −32) | (8, −30) |
| 2 duỗi L1 | `use_*_2` | `l1_ext` 24×16 | (−36, −33) | (−36, −31) |
| 2 duỗi L2 | `use_*_2` | `l2_ext` 24×16 | (−36, −32) | (−36, −30) |
| 2 duỗi L3 màu 1/2/3/4 | `use_*_2/3/4/5` | `l3cN_ext` 40×16 | (−52, −32) | (−52, −30) |
Khung `_3` của sheet = khung 2 với **roi vẽ ĐÈ thân** (`whipOnTop`), khung 0–2 thân đè roi (ở 0/1 không chồng pixel). `simon_whip3c4_stairs_up_0` trên sheet bị cụt 1 hàng đáy (8 px mảnh treo nằm ngoài ô) — ô `c3` cùng tư thế đủ 34 hàng. Quay phải: thân `flipX` quanh pivot; roi đặt tại pivot + (−offset.x, offset.y) và `flipX` (ảnh lưới Play kiểm cả 2 hướng, 0 lệch).
**Bảng màu Simon (`SimonAnimSet.palettes`, từ hàng Colors, ánh xạ 1-1 đã kiểm):** mặc định (124,8,0)/(252,152,56)/(252,216,168) → `purple` đen/(204,136,252)/(252,196,252) · `blue` đen/(252,216,168)/(0,112,236) · `red` đen/(216,40,0)/(252,116,180) · `white` (168,228,252)/(252,216,168)/(252,152,56). Video Map_1 0:06.5–0:07.3 (nhặt roi): Simon nháy mặc định/đỏ/tím/xanh trên khung đang đi (không chỉ tư thế đứng) ⇒ routine 09/11 đổi màu theo bảng; `white` chưa thấy trong video. Bình tàng hình (2:58) = Simon không vẽ, không đổi màu.

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
| Lớp input | `NesPad` (`Assets/Data/Script/Input/`, `ManualSingletonMono`, `[DefaultExecutionOrder(-200)]`, GameObject con `NesPad` của prefab `GameManager`) là nơi DUY NHẤT đọc `Input.*` (grep: 8 lần, chỉ `NesPad.cs`). `InputController` giữ lại làm lớp mỏng cho Title (`StartPressedThisFrame` = `NesPad.Pressed(Start)`, `InjectStart()` = `NesPad.InjectPress(Start)`) | không đổi `TitleScreen`/harness CVTI | 03 |
| Bảng phím | `Assets/Data/Config/InputBindings.asset` (ScriptableObject `InputBindings`) là nguồn duy nhất: Up/Down/Left/Right = mũi tên; A = X, JB0; B = Z, JB2; Select = V, JB6; Start = B, Return, KeypadEnter, Space, JB7, JB9; SubWeapon = C, JB1, JB3; trục `CV_DPadX/Y` (ngưỡng 0.5), `CV_StickX/Y` (ngưỡng 0.5). N, M, LB/RB/LS/cần phải/trigger trống | bảng người dùng chốt (CVGP_PLAN §6) | 03 |
| **Thứ tự tick** | `NesPad.Update` (order −200, trước `NesClock` −100) chỉ chốt cạnh: `latched \|= GetKeyDown ∨ (raw ∧ ¬raw_Update_trước)` (raw = phím ∨ trục ≥ ngưỡng). `NesClock.Step` → `OnSystemTick` → `GameManager.HandleTick` gọi **`NesPad.Tick()` đầu tiên** (chặn gọi 2 lần/tick bằng `LastTickFrame`) → luồng màn hình → `OnTick`. Trong tick: `Held = raw ∨ latched ∨ inject` → bỏ cặp ngược → `Pressed = (Held ∧ ¬Held_trước) ∨ (latched ∧ Held_trước)`, `Released = Held_trước ∧ ¬Held`; xoá latched | NES đọc tay cầm 1 lần/frame; lần nhấn nhanh giữa 2 tick vẫn ra đúng 1 tick Held+Pressed (không lọt); nhiều tick trong 1 Update không lặp Pressed | 03 |
| **Hai hướng ngược** | Trái+Phải cùng giữ ⇒ bỏ CẢ HAI (không hướng ngang); Lên+Xuống ⇒ bỏ cả hai. Áp sau khi gộp bàn phím + D-pad + cần + tiêm (vd phím ← + D-pad → cũng huỷ). `NesPad.Direction` = Vector2Int x −1/0/+1, y +1 lên / −1 xuống | tay NES không bấm được 2 hướng ngược; tránh Simon "đứng rung" | 03 |
| SubWeapon | cờ riêng (bit 8), KHÔNG tiêm giả Up; đã kiểm: nhấn C ⇒ Pressed(SubWeapon) 1 tick, Held(Up) 0 tick | Simon (09) hiểu như Up+B, tránh leo cầu thang ngoài ý muốn | 03 |
| **Định dạng replay** | RLE chữ, 1 phần tử/tick: token `TRẠNG_THÁI*n` (thiếu `*n` = 1), cách bằng khoảng trắng/xuống dòng, `#` = chú thích. TRẠNG_THÁI = `-` hoặc tên nút nối `+`: `U D L R A B Sel Start Sub` (`B` = nút B NES = Roi). Hậu tố `!` (vd `R!`) = nhấn lại trong cùng tick khi tick trước đang giữ (Pressed dù Held liên tục — hiếm, có thật ở 300 fps). Phần tử đầu = Tick đầu tiên sau `PlayReplay`. Giá trị = bitmask Held (bit = `(int)eNesButton`) \| repress << 16. Trong lúc phát: input thật + tiêm bị bỏ qua. File `Assets/Data/Tests/Replays/*.txt` (TextAsset; dòng đầu ghi số tick + hash FNV-1a). Mô tả ở đầu `NesReplay.cs` | đọc/sửa tay được; tái lập đúng từng tick kể cả ca nhấn lại | 03 |
| API tiêm | `Inject(b, n)` giữ n tick từ Tick kế tiếp; `InjectPress(b)` = 1 cạnh nhấn như Update chốt; `PlayReplay(string\|List<int>)`/`StopReplay()`; `StartRecord()`/`StopRecord()` (bắt đầu ở Tick kế tiếp); `SetTestRaw(mask)`/`ClearTestRaw()` thay "phần cứng" để kiểm chốt cạnh; `NesPadProbe` (chỉ Editor) phát lịch raw theo Update + ghi Held/Pressed/Released từng tick — routine 10 dùng lại | harness MCP không bấm phím được | 03 |
| **Số nút Xbox (Unity legacy, Windows XInput)** | A=JB0, B=JB1, X=JB2, Y=JB3, LB=4, RB=5, View/Back=6, Menu/Start=7, LS=8, RS=9; cần trái X = `axis: 0`, Y = `axis: 1` (lên = −1 thô ⇒ `CV_StickY` invert 1); D-pad X = trục thứ 6 (`axis: 5`, phải = +), D-pad Y = trục thứ 7 (`axis: 6`, **lên = + sẵn** ⇒ `CV_DPadY` invert 0). Kiểm: tài liệu Unity (`unity_docs`: KeyCode.JoystickButtonN = "button N on any joystick", Manual Input Manager) KHÔNG ghi bảng số nút Xbox ⇒ đối chiếu chính `InputManager.asset` mặc định của Unity: `Fire1..Jump` = joystick button 0..3 (A/B/X/Y), `Debug Horizontal` = type 2 axis 5, `Debug Vertical` = type 2 axis 6 **invert 0** (Unity dùng D-pad lên = +), `Vertical` joystick axis 1 invert 1 — khớp bảng trên. **Chưa kiểm bằng tay cầm thật** (lúc chạy `GetJoystickNames()` = rỗng) ⇒ §5 #3 | — | 03 |
| `InputManager.asset` | chỉ thêm 4 khối `CV_DPadX` (axis 5), `CV_DPadY` (axis 6), `CV_StickX` (axis 0), `CV_StickY` (axis 1, invert 1): type 2, joyNum 0 (mọi tay), dead 0.19, sensitivity 1, gravity 0; trục cũ không đổi. Unity tự ghi thêm 1 dòng `m_UsePhysicalKeys: 1` khi lưu (trước đó vắng mặt trong file — giá trị trong bộ nhớ vốn là 1, không đổi hành vi) | thay đổi ProjectSettings duy nhất được phép | 03 |
| Không Input System mới | giữ `activeInputHandler: 0` | đổi phải khởi động lại Editor (routine chạy vắng mặt) | 03 |
| **Hệ Config — lược đồ** | CSV UTF-8 `Assets/Data/Config/Csv/<Bảng>.csv` (dòng 1 = tên field) → asset `Assets/Data/Config/<Bảng>.asset` + gốc `GameConfig.asset`. 12 bảng: `ConfigSimon` / `ConfigScore` / `ConfigRule` (khoá–giá trị: `key,value,min,max,unit,source,note`, key = tên `eSimonParam`/`eScoreParam`/`eRuleParam` — đủ mọi giá trị enum), `ConfigEnemy` (`eEnemyId`), `ConfigBoss` (`eBossId` + `phase`), `ConfigActorParam` (`actor` = `Enemy.X`/`Boss.X`/`SubWeapon.X`/`Item.X` + `key` tự do, có min/max — tham số hành vi riêng), `ConfigItem` (`eItemId`, vật chứa id ≥ 101, kind Container), `ConfigSubWeapon` (`eSubWeapon` của CVTI — không đổi), `ConfigWhip` (cấp 1–3), `ConfigBlock` (1–6), `ConfigStage` (1–18), `ConfigDrop` (khung trống). Code: `Assets/Data/Script/Config/` (`ConfigTypes/Ids/Rows/Table.cs`, 1 file/bảng, `GameConfig.cs`), Editor `Assets/Data/Editor/CvgpConfigImporter.cs`. Truy cập: `GameManager.I.Config` hoặc `GameConfig.Current` (gán ở `GameManager.Awake` từ field serialized trên prefab — không Resources.Load) → `GetEnemy/GetBoss(id,pha)/GetItem/GetSubWeapon/GetWhip/GetStage/GetBlock/GetParam`, `simon[eSimonParam]`, `DamageToSimon(stage)`; số có thể chưa biết là `CfgInt`/`CfgFloat` (`has`, `Get(ctx)` log lỗi nếu trống, `ToFixed256` cho px/tick) | một nguồn số duy nhất (H6), diff được | 04 |
| **Quy ước ô & nguồn** | ô số = đã biết ⇒ nguồn (cột `src_<field>` / cột chỉ định bằng `[CfgSrc]` / `source` của dòng) PHẢI chứa thẻ gốc `wiki:` `video:` `đo:` `suy luận` `CVTI` `quy ước`; ô **trống** = chưa biết ⇒ nguồn PHẢI ghi `chờ đo routine NN` / `không rõ` / `n/a`; `-` = không áp dụng. Chữ "chờ đo" luôn kèm "routine NN" (importer bắt). `video:planner` = số đo sơ bộ `MAP1_TIMELINE` — giữ số nhưng kèm "chờ đo routine NN" để routine đó đo lại; khoảng đo (vd 38–40) để value trống + min/max | không bịa số; mỗi ô truy được nguồn | 04 |
| Không dùng `afsdk ParseCSV` | importer tự viết (RFC 4180, InvariantCulture) | `ParseCSV` không phân biệt ô trống với 0, `Convert.ChangeType` theo culture máy, dừng ở dòng có ô đầu trống, nuốt lỗi; không sửa framework (H3) | 04 |
| Importer: validate + idempotent | đủ/không thừa cột, số ô/dòng, kiểu/enum, khoá trùng, đủ 100 % id (enum), số âm (trừ `[CfgAllowNegative]` / khoá chứa `Vy`), hits ≥ 1 và mạnh ≤ yếu, điểm bội 100, stage 1–18, chéo Block↔Stage↔Boss; có lỗi ⇒ log `file.csv:dòng [cột] — lý do`, KHÔNG ghi asset nào. Ghi: so `EditorJsonUtility` trước/sau, chỉ `CopySerialized` khi khác (giữ GUID) | chạy lại không đổi byte | 04 |
| **Máu enemy/boss** | lưu đúng 2 số wiki `hitsWeak`/`hitsStrong`; mô hình HP (gợi ý `HP = W × S`, đòn yếu −S, đòn mạnh −W) **routine 07 chốt**. Phantom Bat 12/8 khớp video (Map_1 3:19–3:30: rìu −2 vạch/lần ⇒ 8 đòn mạnh hết 16 vạch) | wiki = chuẩn chỉ số (H1) | 04 |
| **Sát thương lên Simon** | theo STAGE qua `ConfigBlock.damageToSimon` (B1–2 = 2, B3–4 = 3, B5–6 = 4) + `ConfigRule.DamageSecondQuest` = 4; KHÔNG có cột sát thương từng enemy | wiki Gameplay#Damage | 04 |
| **Giờ theo BLOCK** | Block 1 = 300, Block 2 = 400 (video Map_1 0:02.2 và 3:59.2); đồng hồ KHÔNG đặt lại giữa Stage 1→2→3 (STAGE 02 lúc TIME 0226, STAGE 03 lúc 0166); Block 3–6 trống "không rõ" (không có video, wiki không nêu); tim về 05 khi sang block mới (video 3:52.3) | video (H1) | 04 |
| Mâu thuẫn wiki đã chọn | **Dracula điểm** = 50 000 (trang Bosses) gán cho pha 2, pha 1 trống — Gameplay nói Dracula không cho điểm (routine 20 xem lại). **Dracula số đòn**: pha 1 = 24/16 (bảng Bosses), pha 2 = 32 gán cho đòn MẠNH (Stages 16-18 "32 lần", vũ khí không nêu — suy luận vì lúc đó roi thường cấp 3), đòn yếu pha 2 trống (gợi ý 3:2 ⇒ 48, routine 20 chốt). **Mummy 48/32** = TỔNG 2 con (`hitsScope = TotalAllEntities`; suy luận 48 = 2 × 24, khớp bậc boss Block 4–5) — routine 19 kiểm. **Bone Dragon 8/7, Axe Knight 10/9** giữ nguyên số wiki dù lệch tỉ lệ 1.5×. **Multiplier 700 đ**: ghi cả 2 điều kiện (Items: đang cầm Stopwatch; Gameplay: hoặc không có vũ khí phụ). **Bình tàng hình**: wiki ≈ 5 s nhưng video Map_1 ≈ 2.6 s trong suốt (planner) / SFX 27→28 cách 3.9 s ⇒ để trống, chờ đo routine 11 (H1). **Phantom Bat bản thường** (St 16) `canDie = 1` vì wiki không nói bất tử | ghi cả hai, chọn có lý do | 04 |
| Tên wiki ↔ sheet | id enum theo tên wiki; cột `sheetName`/`nameAlt` giữ tên sheet (Ghoul↔Zombie, Panther↔Black Leopard, Bat↔Vampire Bat, Fishman↔Fish Man, SpearKnight↔Black Knight, BoneTower↔Dragon Skull Cannon, BoneDragon↔Skele-Dragon, AxeKnight↔Axe-Man, Fleaman↔Hunchback, Skeleton↔White Skeleton; Igor/Snake/Phantom Bat thường nằm trên sheet Bosses) | — | 04 |
| Nhạc trong Config | khoá chuỗi `M03_VampireKiller`… theo tên file `Art_Source/music` (suy luận, CHƯA đối chiếu video) — chưa có `eMusic` | nhạc chưa import (§6) | 04 |
| **Manifest sprite gameplay** | `Tools/cvgp/slices_gameplay.json` là nguồn sự thật (sinh 1 lần bằng `gen_slices_gameplay.py` — script tự chặn nếu manifest đã có); cắt bằng `Tools/cvti/slice_sprites.py --manifest …` (mặc định vẫn `slices.json`, CVTI ra md5 y hệt). Thêm cờ mục `bgIsKey` (ô hợp tile: nền xanh trong bbox cũng trong suốt, bỏ kiểm "nền sheet trong sprite") — mặc định tắt | giữ quy ước CVTI (1 PNG/khung, kiểm pixel ngoài Unity) | 05 |
| **Pivot từ manifest** | `NesSpriteImporter` (chỉ lần import đầu, như cũ): PNG có trong manifest ⇒ `SpriteAlignment.Custom`, pivot = `(px/w, 1 − py/h)`; thư mục mới không có trong manifest: `Enemy/ Boss/ Item/` đáy-giữa, `FX/ SubWeapon/` tâm; quy tắc cũ (`Character/**` đáy-giữa, còn lại trên-trái) giữ nguyên. Đọc manifest bằng `CvgpSpriteManifest` (Editor, JsonUtility, cache theo mtime) | 1 nguồn pivot; pivot nguyên pixel ⇒ đặt `ToWorld(x_pivot,y_pivot)` vẫn pixel-perfect | 05 |
| Quy ước pivot | thân Simon = giữa cột chân 16 px + đáy slot 32 (`pivot_goi_y` kiểm kê, không lấy pixel thấp nhất); enemy/boss đứng đất = đáy-giữa; vật bay, đạn, FX = tâm; mảnh roi, cửa, số điểm, bản đồ = góc trên-trái; item = đáy-giữa; `death_1` (nằm) = đáy-giữa ô 32×16 (QUY ƯỚC, chưa đo) | 08/09/13 dựng vị trí theo pivot | 05 |
| **Roi = lớp phủ** | anim Simon `whip_<l1\|l2\|l3c1..4>_<stand\|duck\|stairs_up\|stairs_down>`: `frames` = thân, `overlays` = mảnh roi, `overlayOffsets` (Vector2Int, px NES, y xuống, quay trái), `overlayOnTop`; KHÔNG cắt khung ghép sẵn (dựng lại 100 %). Nhảy dùng `stand` (nhãn sheet "Standing/Jumping"). Ném vũ khí phụ = `throw_<tư thế>` (3 thân, không roi) | tách lớp để roi cấp 3 đổi 4 màu + hitbox roi riêng (09) | 05 |
| `AnimSet` | ScriptableObject `Assets/Data/Script/Anim/AnimSet.cs` (`Get(name)` log lỗi nếu thiếu, `Has`, `GetPalette`, `Anim{name, frames, durations, loop, measured, overlays, overlayOffsets, overlayOnTop, note, TotalTicks}`); 1 asset/bộ ở `Assets/Data/Config/Anim/` (28 bộ: Simon, SubWeapon, 15 enemy, PhantomBat, Medusa, Mummy, Frankenstein, Igor, Death, Dracula, Item, Object, FX, CastleMap). Dựng bằng `CVGP/Build AnimSets` (`AnimSetBuilder`): validate (trùng tên, durations = số khung ≥ 1, overlays khớp, sprite tải được) ⇒ có lỗi không ghi gì; so `EditorJsonUtility` rồi `CopySerialized` (giữ GUID, chạy lại 0 ghi). **Durations mặc định 8, `measured = false`** cho mọi anim; tên anim enemy/boss (sit/run/jump, shoot…) là **suy luận từ hình** (ghi `note`) — routine 13/14/17–20 đo + sửa manifest | không bịa thời lượng (H6) | 05 |
| Walk Simon | `walk` = `walk_0,1,2,1` (ping-pong như Intro CVTI), nhịp chờ đo 08 | — | 05 |
| Dragon Skull / brick | ô dính liền: Dragon Skull tách 2 đầu lâu 16×16 (trên ≠ dưới — đã so) với `edgeCheck:false`; `obj_brick_set` 4 khối giống hệt ⇒ cắt 1 `brick_0`, `alsoAt` kiểm 3 bản còn lại | đề bài | 05 |
| Moai | 16 ô `obj_block_*`/`obj_lean_*` CẮT vào `Item/Moai` (tên `moai_*`): sprite thật của game (không thuộc nhãn Unused); nhận diện "Moai figure 4000 đ, Second Quest" là suy luận (wiki Secrets) — không cần cho Map 1 | đề bài "xác nhận rồi mới cắt": không xác nhận được bằng video ⇒ cắt nhưng ghi rõ suy luận, không dùng ở Block 1 | 05 |

## §5 Câu hỏi cho người dùng (chép từ CVGP_PLAN §9)
1. ~~Nhạc chưa có~~ — **đã có 16 MP3** ở `Art_Source/music/` (routine 01 tìm thấy trong `Downloads\NES - Castlevania - Sounds`). Còn lại: bài nào cho Stage 2/3 và boss (routine 02 đối chiếu video); không có bài "Life Lost" riêng (có `15. Death`).
2. Enemy/boss Block 2–6 không có video ⇒ routine 17–20 dựng theo wiki (gần đúng). Có video thì bỏ vào `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\`.
3. **Kiểm tay gamepad (routine 03 đã xong code, CHƯA bấm thật — lúc chạy không có tay cầm cắm):**
   1) Cắm Flydigi (chế độ Xbox/XInput), menu `CVGP/Input/Debug Overlay` (dấu ✓), bấm Play — bảng 9 nút + giá trị trục + tên tay cầm hiện ở góc trái.
   2) Bấm lần lượt D-pad ↑↓←→, cần trái 4 hướng, A (→ A), X (→ B), B và Y (→ SubWeapon), View (→ Select), Menu (→ Start, Title sẽ vào Intro); bàn phím mũi tên/Z/X/C/V/B tương tự.
   3) Ô nào không sáng đúng (vd D-pad lên ra Down) ⇒ báo lại; sửa bằng cách đổi `invert`/số trục của `CV_*` trong InputManager hoặc phím trong `Assets/Data/Config/InputBindings.asset` (không cần sửa code).
4. HUD còn sprite TẠM (thanh máu/tim/khung + chữ số Famicom — CVTI_MEMORY §5 #16–17): routine 05 kiểm sheet Items & Objects; muốn tìm thêm sheet HUD bản US không?
5. `Map_1.mp4` là bản quay lại playthrough YouTube (có non-game ~1:55, nén); nếu có capture 60 fps thì đo nhịp khung chính xác hơn.
6. (từ CVTI) pixel vuông hay 8:7; Start có bỏ qua Intro không; attract/demo — cụm này giữ mặc định CVTI (vuông, không bỏ qua, không demo).
8. (mới, routine 02) 17 SFX chưa định danh (`01 02 03 08 10 11 12 13 16 17 19 26 32 33 34 36 37`) — gồm vung roi, bị thương, nhảy/đáp, pause, mất mạng, game over, 1UP, boss chết… Muốn gán tên chắc: người dùng nghe thử từng file (hoặc quay clip có các sự kiện đó: bị thương, chết, pause, 1UP) rồi cho biết; hoặc `cvgp-fix` gắn tên khi các routine gameplay có sự kiện tương ứng.
9. (mới, routine 04) **Chỉ số chưa có nguồn** — xem các ô *không rõ* trong `ClaudeNotes/cvgp/CHARACTER_STATS.md`: giờ giới hạn Block 3–6, tốc độ mọi enemy/boss Block 2–6, số đòn Phantom Bat bản thường (Stage 16), điểm của Snake/Phantom Bat thường, giới hạn tim tối đa (đã thấy 76). Nếu người dùng có video các block sau hoặc tài liệu khác (manual/ROM map) thì cung cấp để điền; không có thì routine 17–20 ước lượng và ghi rõ.
10. (mới, routine 04) Mâu thuẫn wiki cần người dùng chọn nếu muốn khác mặc định (§4): Dracula 50 000 điểm hay 0; Dracula dạng 2 "32 đòn" tính cho vũ khí nào; Mummy 48/32 là tổng 2 con hay mỗi con.
7. (mới, routine 00) Console ghi "Joystick disconnected (Controller (Flydigi VADER3))" ⇒ tay cầm báo tên VADER3 — routine 03 nên map theo JoystickButtonN/trục chứ không theo tên.
11. (mới, routine 05) **HUD bản gốc không có trên sheet nào** (thanh máu, tim, khung vũ khí phụ, chữ số US) — HUD giữ sprite TẠM của CVTI. Nếu có sheet "HUD"/"Text" của bản NES US (spriters-resource bị Cloudflare chặn — cần tải tay) thì bỏ vào `Downloads` để routine 12 thay.
12. (mới, routine 05) Vài nhận diện chưa chắc (đã cắt, ghi `note` trong manifest): 16 sprite Moai (`Item/Moai`), `brick_0` (4 khối gạch Simon-palette — công dụng?), `fishman_piece_0`, `dracula2_spark`, bảng màu Simon `white` và dơi tím `bat_purple_*` (màn nào?). Không ảnh hưởng Map 1.

## §6 Nợ & việc ngoài phạm vi
- (CVTI) cần chạy lại CVTI 08 nghiệm thu khi Intro/Stage 1 đã dựng (CVTI_MEMORY §1) — ngoài phạm vi CVGP.
- Console có lỗi nền từ trước (Odin `PersistentContextCache` path1, GUIClips, Mathematics) — không thuộc cụm.
- `InputManager.asset` có nhiều mục tên "Debug" (lấy bằng grep `m_Name`) — routine 03 kiểm trước khi thêm trục.
- `_cache/00/f17_225.png` chưa mở xem riêng.
- (02) **16 file nhạc MP3 chưa import Unity** — §3 (routine 01) ghi "routine 02 làm" nhưng đề routine 02 chỉ cho kiểm `Art_Source/music/`, không xử lý nhạc (H9) ⇒ chưa làm; cần routine/việc riêng (import `Assets/Data/Audio/Music/`, preset, `eMusic` + bảng, gắn Intro/Stage; Title im lặng). Tên file có khoảng trắng + dấu nháy.
- (02) `AudioManager.PlaySfx(eSfx)` dùng `PlayOneShot` (chồng); SFX lặp (đếm tally mỗi 8 tick) cần "SFX mới cắt SFX cũ" — routine 12 (tally) nên thêm.
- (02) `Tools/cvgp/__pycache__/` mới sinh — không commit. `_cache/02/scores*.npz` ~1.5 GB (xoá tay được).
- (03) Gamepad thật chưa kiểm (§5 #3) — nếu người dùng báo sai hướng/nút thì sửa asset, không cần routine riêng.
- (03) Nền mờ của Debug Overlay (đen 60 %) không nhìn thấy trên nền đen lề trái — chỉ thẩm mỹ, chữ vẫn đọc rõ.
- (03) 2 replay mẫu `Assets/Data/Tests/Replays/cvgp03_edge_fps20.txt`, `cvgp03_replay_src.txt` là chuỗi tổng hợp của harness (không phải chơi thật), đuôi có nhiều tick `-` do chờ MCP — routine 08+ ghi replay thật bằng menu `CVGP/Input/Record`.
- (04) Đạn/vật phóng (cầu lửa Fishman/Bone Tower/Bone Dragon/Igor/Dracula, xương, rìu Axe Knight, băng gạc Mummy, lưỡi hái Death) mới chỉ là chữ ở cột `projectile`/`attacks` — chưa có `eProjectileId` + bảng; wiki "một số đạn đáng 100 điểm" không nói đạn nào. Routine 13/17–20 thêm khi dựng (thêm dòng CSV + enum, importer tự kiểm).
- (04) `ConfigDrop.csv` chỉ có tiêu đề — routine 11/15 điền (dữ liệu wiki một phần ở `CV_WIKI_DATA §6`). `container` phải là Torch/WallCandle/BreakableBlock (importer kiểm).
- (04) Khoá nhạc trong `ConfigStage.music`/`ConfigBlock.bossMusic` là chuỗi — khi import nhạc (nợ 02) nên đổi sang enum `eMusic` + kiểm trong importer.
- (04) `GameConfig.GetBlockOfStage` chỉ hỗ trợ Stage 1–18 (Second Quest đánh số khác: Stage 19 và 20 đều là Stage 1).
- (04) Sau khi sửa CSV phải chạy `CVGP/Import Config` (asset) và `CVGP/Export Stats Report` (MD) — chưa tự động hoá (có thể thêm AssetPostprocessor cho `Csv/*.csv` nếu thấy cần).
- (04) Bản sao CSV hỏng có chủ đích dùng để thử validate nằm ở `_cache/04/*.orig.csv` (bản gốc trước khi làm hỏng) + `GameManager.prefab.bak` — xoá tay được.
- (05) Mọi `durations` AnimSet = 8 (`measured=false`) và tên anim enemy/boss là suy luận — routine 08 (Simon), 09 (roi/vũ khí), 11 (item/nến), 13/14 (enemy Block 1, Phantom Bat), 17–20 đo theo video/wiki rồi **sửa manifest** + chạy `CVGP/Build AnimSets` (không sửa tay asset). Chưa có hitbox theo khung (07/09 thêm trường vào manifest/AnimSet).
- (05) `NesSpriteImporter` chỉ đặt pivot ở lần import đầu ⇒ đổi `pivot` trong manifest cho PNG đã import phải xoá `.meta` (mất GUID!) hoặc đặt tay qua API — nên thêm menu "áp lại pivot từ manifest" nếu routine sau cần đổi pivot.
- (05) Đổi màu Simon (nháy khi nhặt roi) cần shader/thay palette lúc chạy — mới có bảng `palettes`, chưa có cơ chế (09/11).
- (05) Khung roi `_3` (roi đè thân) chưa dùng trong anim — routine 09 xem video quyết định có xen kẽ thứ tự vẽ không.
- (05) `ClaudeNotes/cvgp/_cache/05/` (thử dò roi, ảnh kỳ vọng, `grid/page_*`, khung video) — xoá tay được.
- (03) `NesPad` chưa có khái niệm "khoá input" (vd lúc cutscene/đóng băng nhặt roi) — để `StageWorld`/Simon (07–09) tự bỏ qua; nếu cần thì thêm cờ ở 07.


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

### Routine 03 — 13/10/2026
- **Code:** `Assets/Data/Script/Input/{eNesButton,InputBindings,NesPad,NesReplay,NesPadProbe}.cs`, `Assets/Data/Editor/NesPadMenu.cs` (menu `CVGP/Input/Record`, `Stop & Save`, `Debug Overlay`); `InputController.cs` thành lớp mỏng bọc NesPad (bỏ `Update`/`Tick`/đọc `Input.*`); `GameManager.HandleTick` gọi `NesPad.I.Tick()` thay `InputController.I.Tick()`. Asset: `Assets/Data/Config/InputBindings.asset`; prefab `GameManager` thêm con `NesPad` (tham chiếu bindings; diff chỉ gồm GameObject mới + 1 dòng `m_Children`). `ProjectSettings/InputManager.asset`: +4 trục `CV_*` (diff đã xem, §4). Compile sạch (console chỉ lỗi nền Odin path1/Mathematics/GUIClips/MCP WebSocket + warning framework).
- **Kiểm 1 — Inject(Right, 60)** (Play, probe): Held(Right) **60 tick** (26–85), Pressed **1 tick @26** (tick đầu), Released **1 tick @86** (= tick 61 của chuỗi). ĐẠT.
- **Kiểm 2 — chốt cạnh theo fps** (`vSyncCount 0` + `targetFrameRate`, lịch raw theo từng Update qua `SetTestRaw` — Update của NesPad chốt như thật):
  - **20 fps**: 44 Update → 133 tick (43 Update × 3 tick, 1 × 4). Lịch: R 1 Update, R 4 Update, Sub 1, L+R 3, R 1 / thả 1 / R 1, A 2. Kết quả: Pressed(R) **4** (kỳ vọng 4), Held(R) **21** (= 3+12+3+3), Pressed(A) 1 / Held 6, Pressed(Sub) 1 / Held 3, Held(Up) **0**, Held(L)=Held(R trong L+R) **0** (luật hướng ngược), Pressed khi tick trước đang giữ: **0** (không lặp). ĐẠT.
  - **144 fps**: 216 Update → 91 tick (2.37 Update/tick). Lịch: R 1 Update (~7 ms, nhả trước khi có tick), R 50, Sub 1, L+R 10, A 3, R 1. Kết quả: Pressed(R) **3** (kỳ vọng 3), mỗi lần chạm 1 Update vẫn ra **đúng 1 tick** Held+Pressed (không lọt), R giữ 50 Update = 21 tick / 1 Pressed; Pressed(A) 1, Pressed(Sub) 1, Held(Up) 0, L+R → 0; lặp 0. ĐẠT.
  - Chuỗi tick 20 fps ghi ở `Assets/Data/Tests/Replays/cvgp03_edge_fps20.txt` (622 tick, hash `77878f95`).
- **Kiểm 3 — replay**: ghi ở 300 fps (lịch có R, A, R+A, R nhả 1 Update giữa 2 khối 100 Update ⇒ xuất hiện **`R!*1`** nhấn-lại, Sub, B, U+R, D) → `cvgp03_replay_src.txt` **1067 tick, hash `6e3fe69f`** → đọc lại file → `PlayReplay` + ghi lần 2: 1067 tick đầu **hash `6e3fe69f`**, **0 tick khác**; so Held/Pressed/Released từng tick lần 1 ↔ lần 2 trên 121 tick có hoạt động (9 tick Pressed): **0 lệch**. ĐẠT.
- **Kiểm 4 — SubWeapon**: (trong kiểm 2) phím tắt ⇒ Pressed(SubWeapon) 1 tick mỗi lần nhấn, Held(Up) = 0 suốt. ĐẠT. Overlay ảnh L10 cho thấy Inject(SubWeapon) sáng ô SubWeapon, ô Up tắt.
- **Kiểm 5 — gamepad**: `Input.GetJoystickNames()` = **rỗng** (không có tay cầm cắm lúc chạy) ⇒ 4 trục `CV_*` đọc 0.00 (không ném lỗi); không đọc được nút thật — **người dùng tự kiểm** (§5 #3). CHƯA kiểm phần cứng.
- **H8** (Main.unity, harness `OnSystemTick`: `GoTo(Title,0)` ở abs 6989 = L0, `InjectStart` L351 qua `InputController` → `NesPad.InjectPress`): Start **7341 = L352** ✔ (`StartPressedThisFrame` true), đen **7421 = L432** ✔, Intro **7437 = L448** ✔, Simon dừng 7685 = L696 ✔, đen **7879 = L890** ✔, Stage1 **7911 = L922** ✔ — khớp tuyệt đối. Không lỗi/warning mới trong Play.
- **Ảnh đã mở xem** (`ClaudeNotes/cvgp/_cache/shots/03/`, 1920×1080): `h8_L10_title_overlay.png` (Title + Debug Overlay ở lề trái: Right/A/SubWeapon sáng vàng, các ô khác xám, D-pad/Stick 0.00, "Tay cầm: (không có)"), `h8_L600_intro.png` (Intro: cổng, trăng, Simon, HUD — như baseline), `h8_L930_stage.png` (Stage1Stub: nền đen + HUD + Simon — như baseline).
- Menu: `Debug Overlay` bật/tắt EditorPrefs đúng (bật→tắt, trả về mặc định tắt); `Record` bị khoá ngoài Play. Cuối: `CvtiDebug.LogFlow` false, overlay tắt, `targetFrameRate −1`/`vSyncCount 1` (QualitySettings không đổi file), thoát Play, scene Main không dirty, EditorPrefs tạm `CVGP03.*` đã xoá.

### Routine 04 — 13/10/2026
- **Kiểm nguồn:** đối chiếu `CV_WIKI_DATA.md` với `_cache/wiki_raw/text_{Gameplay,Enemies,Bosses,Items,Secrets,Stages_*,Walkthrough}.txt` — 0 số sai (ghi ở `CV_WIKI_DATA §0`). Không cần Wayback mới.
- **Dữ liệu (CSV, `Assets/Data/Config/Csv/`, 181 dòng):** ConfigSimon 30 · ConfigEnemy 18 · ConfigBoss 7 · ConfigActorParam 32 · ConfigItem 25 · ConfigSubWeapon 5 · ConfigWhip 3 · ConfigBlock 6 · ConfigStage 18 · ConfigScore 14 · ConfigRule 23 · ConfigDrop 0 (khung).
- **Đếm so wiki:** enemy thường **15/15** (E01–E15 `CV_WIKI_DATA §3.1`) + 3/3 thực thể phụ (Phantom Bat thường, Igor, Snake); boss **6/6** (B1–B6 §4.1, Dracula 2 pha = 7 dòng); item **22/22** (I01–I22 §2.1) + vật chứa **3/3** (C01–C03); vũ khí phụ 5/5; roi 3/3; block 6/6; stage 18/18. Importer bắt buộc đủ mọi giá trị enum ⇒ thiếu dòng là lỗi.
- **Import:** lần 1 "ghi lại 12, không đổi 0; GameConfig ghi lại"; lần 2 "ghi lại 0, không đổi 12; GameConfig không đổi" — md5 14 file `.asset` y hệt giữa 2 lần. Sửa 1 ô ConfigItem ⇒ chỉ ConfigItem ghi lại (1/11), lần sau 0.
- **Validate (CSV hỏng có chủ đích, đã khôi phục):** 9 lỗi cài vào ConfigEnemy/ConfigSimon/ConfigItem ⇒ importer báo 17 dòng đúng `file.csv:dòng [cột]` (ô số không nguồn ×3, ô trống không ghi chờ đo, "chờ đo" thiếu routine, khoá trùng, số âm, hits < 1, mạnh > yếu ×2, enum sai, điểm không bội 100, thiếu id Panther + WalkSpeed, khoá lạ Bogus, cột lạ + thiếu cột) và "không ghi asset nào" — md5 asset không đổi. Phát hiện + sửa lỗi của chính importer trong lúc thử (bảng sau bỏ qua kiểm dòng khi bảng trước có lỗi — dùng bộ đếm lỗi toàn cục).
- **Play (Main.unity):** `GameConfig.Current` == `GameManager.I.Config`; đọc ra: Ghoul(Zombie) 1/1 đòn, 100 đ, 0.875 px/tick (fixed 224/256); Phantom Bat 12/8, 3000 đ, Stopwatch có tác dụng; Stopwatch 5 tim, đóng băng 300 tick; Dracula 2 pha (pha 2: mạnh 32, yếu chưa có); Simon MaxHp 16, đi 1.0, JumpAirTicks chưa có (min 38); DamageToSimon st1/7/15/19 = 2/3/4/4; 1UP 30000/80000/+50000; giờ Block 1 = 300, Block 3 = ?; Pot Roast +6; roi L3 tầm 43 — khớp CSV.
- **H8** (harness `OnSystemTick`, t0 = 95): Start 447 = **L352** ✔, đen 527 = **L432** ✔, Intro 543 = **L448** ✔, Simon dừng 791 = L696 ✔, đen 985 = **L890** ✔, Stage1 1017 = **L922** ✔ — khớp tuyệt đối; không error/warning mới (chỉ lỗi nền Odin path1/Mathematics/GUIClips/MCP WebSocket + warning framework).
- **Ảnh đã mở xem** (`_cache/shots/04/`, 1920×1080): `h8_L10_title.png` (Title: logo, PUSH START KEY, copyright), `h8_L600_intro.png` (Intro: cổng, trăng, Simon, HUD TIME 0300 ♥05 P03), `h8_L930_stage.png` (Stage1Stub: nền đen + HUD + Simon) — như baseline.
- **Báo cáo:** `ClaudeNotes/cvgp/CHARACTER_STATS.md` sinh bằng `CVGP/Export Stats Report` (11 mục: Simon + bảng sát thương theo stage, enemy, boss, tham số hành vi, item, vũ khí phụ, roi, block/stage, điểm, luật, vật chứa); đã đọc lại toàn bộ — đủ bảng, ô chưa có số in nghiêng kèm routine.
- **Prefab:** `GameManager.prefab` thêm đúng 1 dòng `gameConfig: {guid 6c1693d8…}`; `GameManager.cs` thêm field + `Config` + `GameConfig.SetCurrent` ở Awake (log lỗi nếu chưa gán). Cuối: thoát Play, `CvtiDebug.LogFlow` false, scene Main không dirty.

### Routine 05 — 13/10/2026
- **Công cụ:** `Tools/cvti/slice_sprites.py` thêm `--manifest` + cờ mục `bgIsKey` (mặc định tắt). Chạy lại manifest CVTI sau cả 2 thay đổi: **91/91 file md5 y hệt** file đang có, `git status` Art không đổi. Mới: `Tools/cvgp/{whip_layers,gen_slices_gameplay,verify_sprites,grid_pages}.py` + manifest `slices_gameplay.json`.
- **Cắt:** 246 PNG (§3) — `--check` 0 lỗi; cắt 3 lần (lần 3 sau khi xoá PNG để Unity import bằng importer mới) — **md5 246 file y hệt cả 3 lần**.
- **Roi tách lớp:** `whip_layers.py` (dò 2 bước thân → roi, 6 s) — 96/96 khung ghép = thân + roi, đúng 1 nghiệm/khung, 0 pixel lệch; `_3` = roi đè thân; `simon_whip3c4_stairs_up_0` cụt 8 px trên sheet (lần dò vét cạn đầu tiên chạy quá 1 h — bỏ, không dùng).
- **Verify độc lập** (`verify_sprites.py --meta`): 246 PNG so lại sheet (trong suốt ⇔ key, đục đúng màu, 0 xanh nền đục) · Dragon Skull 2 nửa = ô gốc · brick_0 = cả 4 khối · ghép thân + roi theo **dữ liệu manifest** căn theo `pivot_goi_y` kiểm kê (nguồn độc lập) 96 khung **0 lệch** · đếm 238 + 133 = 371/371 · 246 `.meta`: PPU 16, Point, không nén, không mip, Single, FullRect, pivot = manifest — **0 lỗi**. Thử cài lỗi (1 pixel xanh vào `phantombat_1`, lệch offset roi 1 px) ⇒ script báo đúng 3 lỗi; đã hoàn nguyên.
- **Unity:** `CvgpSpriteManifest.cs`, `AnimSetBuilder.cs` (Editor), `AnimSet.cs` (Script/Anim), `NesSpriteImporter.cs` mở rộng. Compile sạch (chỉ lỗi nền Odin path1/Mathematics/GUIClips/MCP WebSocket + warning framework). PNG gameplay được xoá trước khi compile importer mới rồi cắt lại (byte y hệt) để lần import đầu đã có pivot Custom. Kiểm API 246/246 (PPU, Point, Uncompressed, no mip, Single, kích thước, `sprite.pivot` = manifest). `.meta` mới giống hệt `.meta` CVTI trừ dòng pivot.
- **AnimSet:** `CVGP/Build AnimSets` lần 1 "28 bộ, 135 anim, 296 khung; ghi lại 28", lần 2 + chạy bằng menu thật "ghi lại 0, không đổi 28", md5 56 file asset/meta y hệt.
- **Ảnh lưới Play** (scene tạm `Assets/Scenes/Test/SpriteGrid05_tmp.unity` = bản sao PixelGrid, ref 768×480 zoom 2 ⇒ vùng (192,60,1536,960), nền xám 160; **đã xoá**): trang 0 = 240 PNG, trang 1 = bản đồ lâu đài + 144 khung roi ghép bằng **asset AnimSet** quay trái + phải (flipX). So với ảnh kỳ vọng ghép bằng Pillow (`grid_pages.py`): **0/1 474 560 lệch, 0 pixel ngoài vùng không đen ở cả 2 trang**. Ảnh đã mở xem: `_cache/shots/05/sprite_grid_p0_1920x1080.png`, `sprite_grid_p1_1920x1080.png` (+ crop phóng to trang 1). (Lần chụp đầu không ra file vì Editor mất focus — có 2 Editor Unity khác đang mở; phải AppActivate đúng cửa sổ tiêu đề "Castlevania_Original-main".)
- **Video (đối chiếu nhận diện):** 1:39.55–1:40.15 đập tường ⇒ mảnh tím = `obj_purple_orb` (→ `FX/brick_debris`); 0:06.3–0:07.5 nhặt roi ⇒ Simon nháy mặc định/đỏ/tím/xanh (bảng màu); 2:58.6–3:00.6 bình tàng hình ⇒ Simon không vẽ. Khung ở `_cache/05/` (`v_brick.png`, `pk_montage.png`, `inv_montage.png`), đã xem.
- **H8** (Main, harness `OnSystemTick`, `GoTo(Title,0)` abs 253 = L0, `InjectStart` L351): Start **L352** ✔, đen **L432** ✔, Intro **L448** ✔, đen **L890** ✔, Stage1 **L922** ✔ — khớp tuyệt đối. Ảnh đã xem (`_cache/shots/05/h8_L10_title.png`, `h8_L600_intro.png`, `h8_L930_stage.png`): như baseline.
- Cuối: thoát Play, scene Main không dirty, `CvtiDebug.LogFlow` false, không còn scene/asset tạm; không đổi scene/prefab/ProjectSettings.
