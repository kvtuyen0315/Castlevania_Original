# CVTI_MEMORY — sổ bàn giao cụm Title + Intro Castlevania (NES)

*Mọi routine CVTI: đọc file này + CVTI_PLAN.md ĐẦU phiên, cập nhật CUỐI phiên, commit cùng routine.*
*Yêu cầu gốc của người dùng (11/10/2026), nguyên văn:* "Hãy lên danh sách routines về làm task làm Title và đoạn intro game như video trên" — video `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Title_Intro.mp4`; Sprite Resource: https://www.spriters-resource.com/nes/cv/ ; Sound Resource: https://sounds.spriters-resource.com/nes/cv/

## §1 Trạng thái cụm

| # | Routine | Trạng thái | Commit | Ngày | Ghi chú |
|---|---|---|---|---|---|
| 00 | hạ tầng timeline + sổ | **XONG** (ảnh Play Mode baseline chụp lại 11/10 nhưng ra TRẮNG, xem §7) | (commit "CVTI 00") | 11/10/2026 | đo bằng ffmpeg + python; MCP mất kết nối khi vào Play Mode |
| 01 | thu thập sprite/sound nguồn | **XONG** | (commit "CVTI 01") | 11/10/2026 | nhập 3 PNG + zip + 37 WAV, sha256/md5 khớp; 3 thứ còn thiếu → §5 |
| 02 | nền render pixel-perfect NES | CHƯA | | | |
| 03 | cắt sprite Title/Intro/HUD/font | CHƯA | | | |
| 04 | khung game flow/input/audio/NesText | CHƯA | | | |
| 05 | màn Title + dơi + nháy PUSH START | CHƯA | | | |
| 06 | HUD dùng chung | CHƯA | | | cần sheet HUD (§5) |
| 07 | Intro Simon + nối Stage 1 stub | CHƯA | | | |
| 08 | nghiệm thu so với video gốc | CHƯA | | | |

## §2 Timeline & số đo (video 524×476, 30 fps container, capture thực ~7.5 fps ⇒ sai số mốc ±0.13 s; frame NES = giây × 60)

Đổi toạ độ video→NES (đo lại 11/10/2026): `x_NES = (x_v − 11)/1.969`, `y_NES = (y_v + 7.4)/1.712`.

### 2.1 Mốc đã xác minh

| Mốc | Giây video | Frame NES ≈ | Nguồn đo |
|---|---|---|---|
| Đen (UI emu kết thúc) | 0.93 | 56 | YAVG toàn khung = 16 |
| **Title hiện** (cắt cứng) | **1.03** | 62 | YAVG 16→47.5 |
| Dơi bắt đầu cử động (nhỏ, ở ngọn tháp) | **1.07** | 64 | diff frame vùng tháp (xem 2.2) |
| Dơi bay ra sau tháp / lên trái | 2.83 | 170 | diff frame |
| Dơi bắt đầu lơ lửng vỗ cánh | ~3.2–3.4 | ~195–205 | autocorr pose |
| PUSH START KEY tắt #1 | **6.90** | 414 | YAVG vùng chữ 61→16 |
| tắt #2 / #3 / #4 / #5 | 7.17 / 7.43 / 7.70 / 7.97 | 430/446/462/478 | idem; chu kỳ 0.267 s ≈ **16 f** (tắt 8 f + bật 8 f) |
| bật lại sau #5 | 8.10 | 486 | idem |
| **Cắt đen** | **8.23** | 494 | YAVG = 16 |
| Nhạc Prologue bắt đầu | 8.436 | 506 | silencedetect (-45 dB) |
| **Intro hiện** (HUD+cảnh) | **8.50** | 510 | YAVG 16→46.7 |
| Simon dừng, đổi quay lưng | **12.70** (khung đầu tiên ≡ khung cuối là 12.70; chuyển động cuối thấy ở 12.67) | 762 | diff vùng Simon |
| Nhạc Prologue hết | 14.855 | 891 | silencedetect |
| **Cắt đen** | **15.87** | 952 | YAVG = 16 |
| **Stage 1 hiện** | **16.40** | 984 | YAVG 16→49.4 |
| Âm Vampire Killer | bắt đầu 16.414; có quãng lặng 16.946–17.25 | | silencedetect |

Thời lượng: Title im lặng 1.03→6.90 (5.87 s ≈ 352 f) · nháy 6.90→8.23 (1.33 s = **80 f**) · đen 8.23→8.50 (0.27 s ≈ **16 f**) · Intro 8.50→15.87 (7.37 s ≈ 442 f) · đen 15.87→16.40 (0.53 s ≈ **32 f**). Mọi chuyển cảnh là cắt cứng. Không có âm thanh trước 8.436 s (Title im lặng, bấm Start không SFX) ✓.

### 2.2 Số đo bổ sung

**Dơi Title** (crop video x373,y171; toạ độ NES quy đổi):
- Planner ghi "xuất hiện ~1.8 s" → **LỆCH**: dơi đã cử động từ **1.07 s** ngay sau khi Title hiện (nhỏ, bbox ~13×16 video px ở ngọn tháp ≈ NES (215–222, 118–127)); capture thấp + nhỏ nên khó thấy.
- Đường bay: 1.07 s ngọn tháp → bay sang phải & to dần: 2.2 s bbox NES x≈235–248, y≈119–133 (xa nhất bên phải) → 2.7 s lệch xuống/ trước tháp NES x≈222–239, y≈134–150 (thấp nhất) → 2.83 s quặt lên-trái NES x≈211–229 → 3.2 s đã ở trên-trái tháp, mép trái NES x≈186, mép trên chạm đỉnh ô (y≈104). Khớp "40 khung sheet" ⇒ chạy tuần tự, mỗi khung ≈ (3.2−1.03)/… không đo được từng khung (capture 7.5 fps); routine 05 chia đều 40 khung và nghiệm thu bằng ảnh.
- **Lơ lửng**: 4 tư thế lặp, chu kỳ **0.274 s ≈ 16 f** (autocorr lag 4 mẫu; cùng tư thế lặp ở 3.40/3.67/3.93/4.23/4.50/4.77 s) ⇒ mỗi tư thế ≈ 4 f NES. Không dừng/đổi pha cho tới 6.90 s và tiếp tục khi PUSH START nháy.

**Simon intro**:
- x đầu: khung đầu (8.50 s) mép trái sprite video x=489 ⇒ **NES x ≈ 243** (sprite 16 px, đang chui vào từ mép phải 256).
- x dừng: mép trái video 248 ⇒ **NES x ≈ 120.6**, tâm ≈ **128.6** ≈ giữa cổng (tâm cổng video 262 ⇒ NES 128) ✓.
- Tốc độ: hồi quy tuyến tính mép trái 8.6–11.5 s = −58.0 video px/s = **−29.5 NES px/s = 0.49 px/f** (planner 0.5 ✓); đi 122 px trong 4.2 s (≈252 f) từ 8.50 đến 12.70.
- Chân Simon ≈ NES y 209 (video y≈350), cao 32 ⇒ đỉnh y≈177.
- Nhịp walk cycle: độ rộng bbox dao động chu kỳ **7 mẫu @30 fps ≈ 0.233 s ≈ 14 f NES** (autocorr đỉnh ở lag 7/14/21/28; chuyển hẹp↔rộng cách nhau 0.10–0.17 s). **Không phân giải được từng tư thế** (capture 7.5 fps, sheet có 3 sprite đi) ⇒ routine 07 làm xấp xỉ: ping-pong 3 khung, tổng chu kỳ ≈ 14 f, ghi quyết định vào §4.
- Đổi quay lưng: **12.70 s** (planner 12.6; lệch +0.1, trong sai số), đứng yên tới hết cảnh (diff = 0 từ 12.70 đến 13.6).

**Mây intro**: sprite 32×16, **vẽ SAU/TRÊN mặt trăng** (che trăng, tối). Mép trái NES x ≈ 222 (8.6 s) → 189 (14.0 s); tốc độ **≈ 7.1 NES px/s ≈ 0.118 px/f** (≈1 px mỗi 8.5 f) trôi sang TRÁI; y NES ≈ 72–88. Đến 15.5–15.87 s mây vẫn còn đè lên mép trái mặt trăng (chưa trôi qua hẳn). Mặt trăng NES x≈189–216, y≈66–89 (cố định).

**2 dơi nhỏ intro** (đều vỗ cánh: luân phiên dạng "v" và "—", chu kỳ < 0.5 s, không đo chính xác):
- Dơi A: có sẵn từ đầu Intro ở ngọn tháp lâu đài (NES ≈ (146,69) lúc 9.0 s), trôi chậm sang trái-xuống ≈ (121,72) lúc 13.0 s.
- Dơi B: **xuất hiện 11.0 s** từ vùng trái (NES ≈ (48,105)), bay sang phải & lên ≈ 13 px/s (0.22 px/f), tới ≈ (101,67) lúc 15.0 s. Planner "rõ từ ~11.5 s" ≈ đúng (B hiện 11.0).
- Không đo được: quỹ đạo dao động chi tiết ⇒ routine 07 làm xấp xỉ.

**Hệ số 8:7 (đo lại)**: ô vuông ngang bắt đầu x=11, 12 ô cách 189 px ⇒ ×1.969 (8 px NES ≈ 15.75); y: dải 1 video 20–32 (NES 16–23), dải 2 335–348 (NES 200–207) ⇒ ×**1.712**, gốc y −7.4; thanh xám NES 216 → video 363 ✓. **Xác nhận** planner (1.96/1.71, gốc 11/−7.4), tinh chỉnh x thành 1.969.
**Lệnh ghép §3a** chạy thử OK (panel A phóng 4×, đặt (448,60), W:H:X:Y=1024:960:448:60): logo/ô vuông/thanh xám khớp (ảnh hiệu chỉ còn viền 1px do nén h264); sáng ở đúng: dòng copyright, PUSH START KEY (sheet có thanh chấm tím/xanh), vùng tháp ô-liu, chữ "1987" vs "1988". Ảnh: `_cache/shots/00/cmp_side.png`, `cmp_diff.png` (đã xem).

## §3 Tài nguyên

Nguồn + sha256 + credit: `Art_Source/SOURCES.md` (và `CVTI_PLAN.md §2`). Đã nhập 11/10/2026, **sha256 cả 4 file khớp planner**; 37 WAV giải nén lại từ zip, md5 từng file khớp `Downloads\393996\`. Sprite đã cắt: *chưa có* (routine 03).

### Bản đồ sheet đã xác nhận (đo bằng ffmpeg rawvideo + python, 11/10/2026)

| Sheet | Thành phần | Rect (x,y,w,h trong sheet) | Màu cần key | Kiểm |
|---|---|---|---|---|
| title_screen | Panel A Title đầy đủ | (1,1,256,240) | KHÔNG key (đen là đen thật; nền sheet (0,128,0) nằm ngoài panel) | góc (1,1)=(188,188,188), (256,240)=đen, x=257 xanh lá ✓ |
| title_screen | Ô ô-liu (128,128,0) = chỗ tháp+dơi | NES (184,104,72,56) = sheet (185,105,72,56) | — (khung bat đè lên) | bbox đo = (185,105,72,56) ✓ |
| title_screen | Thanh chấm tím/xanh = chỗ PUSH START | NES (72,136,112,8) = sheet (73,137,112,8) | — | tím từ x73 + teal từ x81 ghép = 112×8 tại y137 ✓ |
| title_screen | Thanh xám (188,188,188) trên/dưới | NES y 0–7, 216–223 | — | sheet y1–8 / 217–224 ✓ |
| title_screen | `PUSH START KEY` bật | (558,1,112,8) | không (chữ (252,252,252) trên đen) | 2 màu ✓ |
| title_screen | `PUSH START KEY` tắt (đen) | (558,10,112,8) | — | toàn đen ✓ |
| title_screen | 8 dơi lẻ (tham khảo) | y127: x528/545/562/579/596 (16×16), 613 (24×32), 638, 671 (32×32) | không | 5 ô đầu không dính nền xanh ✓ |
| title_screen | 40 khung "Bat Animation" 72×56 | x=1+73·c, y=256+57·r (c 0..9, r 0..3) | KHÔNG key | cả 40 ô: có đen, không lẫn xanh lá ✓; đã xem hình |
| title_screen | Hộp credit (loại trừ) | (506,486,224,32) | — | thấy trên hình |
| cutscenes_credits | Nền Intro | (1,1,256,192) | KHÔNG key (11 màu; không có cây/mây/dơi) | x=257 và y=193 ra ngoài nền ✓ |
| cutscenes_credits | Dơi nhỏ khung 1 | (1,194,8,16) | xám (116,116,116) | xám 98 px, 2 màu ✓ |
| cutscenes_credits | Dơi nhỏ khung 2 | (10,194,8,16) | xám (116,116,116) | xám 100 px ✓ |
| cutscenes_credits | Mây | (19,194,32,16) | xám (116,116,116) | xám 272 px, 3 màu ✓; ô kế (x51) là xanh lá |
| simon_belmont | Idle | (1,21,16,32) | xám (116,116,116) | 4 màu, xám 223 px, viền xanh lá ✓ |
| simon_belmont | Walk 1/2/3 | (29,21), (46,21), (63,21) (16×32) | xám | xám 223/268/227 px ✓ |
| simon_belmont | Intro/quay lưng | (362,21,16,32) | xám | xám 178 px ✓ |

**Không lệch so với planner** (mọi rect khớp). Lưu ý: panel A bắt đầu ở (1,1) nên toạ độ sheet = toạ độ NES + 1.

### Danh mục 37 SFX (`Art_Source/sounds-resource/393996/`, để dành cho cụm gameplay; không đoán tên)

| file | thời lượng | md5 |
|---|---|---|
| 01.wav | 0.56 s | 89286c306a599b201892e808e6193ace |
| 02.wav | 0.10 s | 606648a8fa95f5dd2a9414713b645b0c |
| 03.wav | 0.19 s | 9a75d5ee09ed23c47371b20a5062f9ab |
| 04.wav | 0.37 s | 6790b0bb857deb44ea858e32ce03ef73 |
| 05.wav | 0.55 s | 83afb2e26bbefba904655004b4bf0f8b |
| 06.wav | 0.54 s | a04b8464a334b390587e4d203e9ef3b9 |
| 07.wav | 0.11 s | ddffd586c2a03c25829497325655eaba |
| 08.wav | 0.18 s | 0d14463468a6a60aa33bd32a5270af53 |
| 09.wav | 0.18 s | a46679ec13b868ba2b78a908699e52fc |
| 10.wav | 0.25 s | d50a2e822557e3284fade599e9e1adea |
| 11.wav | 2.05 s | 7acc49bf4e2ab9543c1d887f2b284193 |
| 12.wav | 1.64 s | 5776c93a5b2f1b254fcf896415223778 |
| 13.wav | 0.13 s | 7a3654f098818626fea0ce36c9ac60ae |
| 14.wav | 0.73 s | b9dba53b274a45c81955a70c874796d2 |
| 15.wav | 1.01 s | 99fffc9ce0eb5c009166629b36afe411 |
| 16.wav | 0.40 s | 1abc4e93946dd7cf1dc106625ae456c6 |
| 17.wav | 0.40 s | 4a194ce5b6f24676ad70bf1597b43094 |
| 18.wav | 0.79 s | eef154aadec729a1f050499c5bc3ca42 |
| 19.wav | 0.45 s | 40de178a75ffc588ebd4390850bb8c3a |
| 20.wav | 0.36 s | 8c21631e10b3433efd50e363d2018ed0 |
| 21.wav | 0.60 s | 6b49d959cd96dad37df1b9716af90687 |
| 22.wav | 0.50 s | 95d7ab928bd31a379eeead4de40cc1a1 |
| 23.wav | 1.04 s | a32b044451da616aa3c9b7953718a2eb |
| 24.wav | 0.75 s | 64873fb6c4200ddfd5fd538aebf5c30a |
| 25.wav | 1.22 s | 045b425faf36752acb75452c17a42740 |
| 26.wav | 2.40 s | c7496a7187593fa952dc2cd10468e384 |
| 27.wav | 0.83 s | 82aebea8d7f85927fae9d182c2877c13 |
| 28.wav | 0.83 s | 200a739475b17ea59683b6e00ce5d6fe |
| 29.wav | 0.81 s | b28d71a8d634148dd5c265154aac303b |
| 30.wav | 1.16 s | 4bca08eb62ebb476646b9481cbf6ec2d |
| 31.wav | 1.15 s | df7851a075c7a3b930e1a9d76625ae85 |
| 32.wav | 2.14 s | 43a5ed9ae847f29d460d2beecb37f391 |
| 33.wav | 0.50 s | 805f2b56df493e169e3e599fe4b01d53 |
| 34.wav | 1.54 s | 1f98ad45ac7abaf6121fed1519108522 |
| 35.wav | 1.62 s | d369a58fb3b1827cc21e129f535b3dcb |
| 36.wav | 0.66 s | 58a5d963ca7d08d9fc6d61d517b839d6 |
| 37.wav | 0.11 s | f002d9b8c4499b0117612cd865e1a2b0 |

## §4 Quyết định kỹ thuật đã chốt

| Quyết định | Lý do | Routine chốt |
|---|---|---|
| Hệ số video→NES: x ÷1.969, y (+7.4)÷1.712 | đo lại trên 2 dải ô vuông, §2.2 | 00 |
| Thời lượng tính bằng frame NES 60 fps, chuẩn: nháy 8f tắt + 8f bật ×5 (80 f), đen 16 f trước Intro, đen 32 f trước Stage 1, Simon 0.5 px/f ≈ 252 f, mây 0.118 px/f | §2 | 00 (hằng số `NesTiming` thuộc routine 04) |
| Hiến pháp H1–H7 giữ nguyên như đề bài | | 00 |

(Chưa có quyết định nào khác — để routine 02/04 điền.)

## §5 Câu hỏi cho người dùng (chép từ CVTI_PLAN §8 + phát sinh)

1. Để yên ở Title lâu thì có cần attract/demo mode không? (video không thể hiện ⇒ mặc định KHÔNG làm)
2. Pixel vuông (mặc định, nét) hay giả lập tỷ lệ 8:7 như video?
3. Nhạc Prologue / Vampire Killer lấy từ đâu? (pack 393996 chỉ có SFX; video có audio nhưng rất nhỏ ~-52 dB và lẫn UI emu — không nên cắt từ video)
4. Sheet HUD/font và sheet Stage 1 — người dùng tải thêm khi nào? (chặn routine 06 và phần Stage 1 của 07)
5. Stage 1 sau intro: chỉ dựng **stub** (màn đầu + HUD + nhạc) để nối luồng; gameplay Stage 1 là cụm khác — xác nhận.
6. (Mới) Dòng copyright: làm theo video "1988"/3 dòng (mặc định đã chọn theo yêu cầu "như video") — xác nhận được.

**Trả lời cho §5 (routine 01, chạy vắng mặt — không hỏi được người dùng):** CHƯA TRẢ LỜI. Cần người dùng cung cấp đúng 3 thứ (chi tiết `Art_Source/_inbox/README.md`): (1) sheet HUD+font — chặn routine 06; (2) sheet Stage 1 — 07 dùng nền đen nếu thiếu; (3) nhạc Prologue + Vampire Killer — 07 không phát nhạc nếu thiếu. Tạm áp dụng phương án Recommended: người dùng tự tải vào `Art_Source/_inbox/`, routine 02–05 làm tiếp phần đã có; 06 tự kiểm sheet HUD.

## §6 Nợ & việc ngoài phạm vi

- Ảnh baseline Play Mode ra trắng toàn khung (xem §7) — routine 02 xem lại khi dựng camera. `manage_camera screenshot` mặc định lưu vào `Assets/Screenshots/` (tạo file game + .meta): phải chuyển sang `_cache` và xoá thư mục đó.
- Nhịp từng tư thế walk của Simon và từng khung dơi Title không đo được ở capture 7.5 fps.
- Nhiễu nền git (không thuộc cụm): `Assets/TuyenAframework.meta`, `Castlevania_Original-main.slnx`, `UserSettings/`, submodule `Assets/TuyenAframework` hiện cờ ` m`.
- (01) 393996 = SFX không tên, cụm gameplay sẽ định danh; không import WAV nào vào `Assets/` ở cụm CVTI.
- Camera `MainCamera` Untagged ⇒ `Camera.main` null (routine 02 nên tag lại hoặc không dùng `Camera.main`).

## §7 Nhật ký nghiệm thu

### Routine 00 — 11/10/2026
- Ảnh tham chiếu `ref/` đủ 9 PNG + tsv; đã mở xem cả 9 (01 qua `cmp_side`).
- Số đo lệch so với planner: dơi Title bắt đầu 1.07 s (không phải 1.8 s); Simon quay lưng 12.70 s (không 12.6 s); chi tiết hệ số x 1.969. Mọi mốc còn lại khớp (±1 frame video): Title 1.03, nháy 6.90–8.23, đen 8.23–8.50, nhạc 8.436, nhạc hết 14.855, đen 15.87–16.40.
- **BASELINE project**: Unity 2022.3.62f3, MCP kết nối đúng (`dataPath` = `D:/Unity/Castlevania_Original-main/Assets`). Git: HEAD `38c219c`. `Assets/Data`: `Prefabs/GameManager.prefab`, `Script/System/{GameManager,GlobalInfo}.cs`; `Assets/Scenes/Main.unity` có root `GameManager` (Transform+GameManager) và `MainCamera` (ortho, size 1.3, bg RGBA(0.192,0.302,0.475,0), pos (0,0,-10), Untagged). ProjectSettings: `defaultScreenWidth/Height` 1920×1080, `activeInputHandler: 0` (Input Manager cũ), `m_ActiveColorSpace: 0` (Gamma), Quality hiện tại = Ultra (index 5), không có SRP (Built-in). `Packages/manifest.json` có `2d.pixel-perfect 5.1.1`, `2d.sprite 1.0.0`, `2d.spriteshape 9.1.1`, `textmeshpro 3.0.9`, `timeline 1.7.7`, `ugui 1.0.0`, UniTask, unity-mcp (git). Console (Error/Warning) trước Play: **0 mục**.
- **DỞ**: ảnh Play Mode màn trống KHÔNG chụp được — sau `manage_editor play` kết nối MCP rơi (`no_unity_session`, instances=0) trong >5 phút (Editor.log có "The file 'MemoryStream' is corrupted"/Odin). Không ghi "đã nhìn thấy" (H7).

### Chạy lại routine 00 — 11/10/2026 (phiên 2, bổ sung baseline)
- MCP kết nối lại bình thường, vào Play Mode không rớt kết nối lần này. Chụp 2 ảnh `manage_camera screenshot` (game view 640×360): **cả 2 ảnh TRẮNG hoàn toàn** dù MainCamera là SolidColor bg RGBA(0.192,0.302,0.475,**0**) — nghi alpha=0 làm game view ra trắng/không render; chưa kết luận nguyên nhân. Ảnh lưu `_cache/shots/00/screenshot-20261011-11495{5,7}.png` (đã xem). Console có lỗi nền không thuộc cụm: Odin `Sirenix.OdinInspector.Editor.dll` broken assembly, `Unity.Mathematics module data`, `ArgumentNullException path1`. Screenshot tự ghi vào `Assets/Screenshots/` — đã chuyển sang `_cache` và xoá thư mục.
- Baseline hoàn tất, 0 file game thay đổi.

### Routine 01 — 11/10/2026
- Nhập 3 PNG + zip + giải nén 37 WAV vào `Art_Source/`; sha256 4 file khớp planner, md5 37 WAV khớp `Downloads\393996\`. `SOURCES.md` + `_inbox/README.md` đã tạo. Bản đồ sheet đo lại (§3): không lệch. Đã mở xem cả 3 sheet. 0 file mới trong `Assets/`.
