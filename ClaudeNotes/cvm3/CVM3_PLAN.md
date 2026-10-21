# CVM3 — Map 3 Castlevania (NES): Block 3 = Stage 07–09 chơi được như video (Walkway Entrance → Walkway → cầu hỏng → Mummy Men → STAGE 10)

> Soạn 17/10/2026 bởi session planner (Opus 5.5). **Mọi routine `cvm3-NN` đọc file này ĐẦU phiên** (cùng `CVM3_MEMORY.md`); cần chi tiết
> hạ tầng/gameplay cũ thì đọc `ClaudeNotes/cvm2/CVM2_MEMORY.md` (§3 tài nguyên + đường dẫn Map 2, §4 Bảng hash + `RegressionRunner` + quyết định),
> `ClaudeNotes/cvm2/ACCEPT12.md` (kết luận Map 2 + routine sửa đề xuất) và `ClaudeNotes/cvgp/CVGP_MEMORY.md` (§3 tài nguyên, §4/§4b API lõi, §7 cách chạy probe).
> Số liệu planner = THÔ — **đo lại trước khi dựa vào**.

## 0. Yêu cầu gốc (nguyên văn, 17/10/2026)

> Tiếp tục dựng map 2 và play game theo như theo như video trên. Xem qua Walkthrough Stages 7 - 9 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_7_-_9
> Nếu như thiếu file sprite thì vào link ( https://www.spriters-resource.com/nes/cv/ ) này xem để download
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

Kèm: 2 video `Map_3.mp4`, `Boss_Map_3_Behaviour.mp4` + ảnh thư mục Downloads có sheet mới tải 17/10 `NES - Castlevania - Stages - Level 3_ Crumbling Tower.png`.
Đây là câu trả lời cho câu hỏi mở `CVM2_MEMORY §5 #4/#19` (sheet Level 3 + video Map 3 cho cụm CVM3).

**Quyết định planner (ghi để routine không hỏi lại):**
- Người dùng viết "map 2" nhưng đính kèm `Map_3.mp4` + Walkthrough **Stages 7–9** + sheet **Level 3** ⇒ hiểu là **Map 3 (Block 3)**, nối tiếp cụm CVM2 (Map 2 đã chơi được tới `Stage7Stub`). Nếu người dùng nói khác ⇒ dừng cụm, hỏi lại (§9 #6).
- Cụm CVM3 **gộp 2 việc nợ của CVM2 chặn Map 3** (như CVM2 đã gộp nợ CVGP): **đồng hồ (Stopwatch) theo video** (`cvm2-fix-01` → **`cvm3-03`** — số DÙNG CHUNG, Stage 8 có nến đồng hồ) và **Simon knockback + lọ nước thánh + mép rơi theo video** (`cvgp-fix-03` → **`cvm3-04`** — Block 3 toàn hố/cầu hỏng, Raven/Medusa Head/Fleaman hất Simon; video boss Map 3 có nhiều lần Simon trúng đòn). Hai routine này **CỐ Ý đổi hành vi Map 1/Map 2** ⇒ hash đổi có lý do (H14).
- **Luật enemy rơi vật** (`cvm2-fix-02`, chờ `CVM2 §5 #14`): nay có video thứ 3 ⇒ `cvm3-08` **đếm mọi lần hạ enemy ở 3 video** (Map_1/2/3); chỉ dựng nếu MỘT luật tất định giải thích MỌI mẫu (kể cả lần không rơi), không thì giữ `dropItem` None + cập nhật câu hỏi (đúng tinh thần lựa chọn (a) "chờ thêm video").
- `cvm2-fix-03` (lệch nhỏ Map 2): phần **builder `Build Map2 Screens` không idempotent byte** gộp vào `cvm3-11` (builder màn hình phải mở rộng cho Block 3 — sửa luôn); **pha TIME (#16) / chờ tally (#17)** là quyết định người dùng cho Map 1/2 ⇒ Block 3 dùng **hằng riêng đo từ Map_3** (không đổi Map 1/2), câu hỏi giữ nguyên. `cvgp-fix-04` (báo #3, Phantom Bat — chỉ Map 1) **KHÔNG gộp**, vẫn chờ duyệt.
- Sprite: **không cần tải thêm** — mọi sprite Block 3 đã cắt từ CVGP 05 (`Enemy/Fleaman` 2 khung, `Enemy/Raven` 4, `Enemy/WhiteSkeleton` + `bone_0..2`, `Boss/Mummy` tím/đỏ ×3 + `bandage_0/1`, `Item/moneybag_*`, `axe`, `dagger`, `roast`, `triple`) + sheet Level 3 mới. spriters-resource bị Cloudflare chặn ⇒ thiếu thật thì ghi `§5` nhờ người dùng tải tay, KHÔNG tự vượt bot-check.
- Bàn phím/gamepad, kiến trúc lõi, hiến pháp: **giữ nguyên CVGP/CVM2**.

## 1. Nguồn

| Loại | Đường dẫn / ghi chú |
|---|---|
| Video Map 3 (chỉ đọc) | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_3.mp4` — 4:11.61, **1488×760**, 30 fps (≈ 18 khung game thật/s), AAC 48 kHz. **Tiếp đúng playthrough Map_2** (SCORE 044670, P-04, tim 05, đồng hồ): đuôi bản đồ B2 → **STAGE 07 → 08 → 09** → Mummy Men → tally (SCORE **073630**) → bản đồ lâu đài B3 (HUD **STAGE 10**) — hết giữa lúc bản đồ trượt vào. |
| Video boss (chỉ đọc) | `…\Boss_Map_3_Behaviour.mp4` — 42.73 s, **1370×772**, 30 fps (≈ 21.7 khung thật/s). Playthrough khác (SCORE 016900, **P-01**, dao găm), **bảng màu khác** (gạch hồng-cam) + watermark giữa HUD + "Support me on ko-fi" góc dưới-trái. Trận Mummy Men đầy đủ: vào phòng → phá khối ⇒ thịt nướng → 2 Mummy + băng gạc → **Simon trúng đòn nhiều lần** → 2 Mummy chết → quả cầu → nhặt. |
| Chỉ mục timeline + hiệu chuẩn | **`ClaudeNotes/cvm3/ref/MAP3_TIMELINE.md`** + ảnh `ref/nes/*_nes.png` (40) — planner đo THÔ, routine 02 đo kỹ |
| Wiki | **`ClaudeNotes/cvm3/wiki/MAP3_WIKI_DATA.md`** (chỉ mục vào `CV_WIKI_DATA.md` + chi tiết Stage 7–9 + đối chiếu video) — văn bản thô `ClaudeNotes/cvgp/_cache/wiki_raw/text_Stages_7_-_9.txt`, ảnh bản đồ `ClaudeNotes/cvm3/_cache/wiki_img/` |
| Kiểm kê sheet | **`ClaudeNotes/cvm3/inventory/level3_walkway.json`** + `level3_walkway_annot.png` |
| Hạ tầng sẵn có (CVTI + CVGP + CVM2) | Title/Intro, `NesPad` + replay, Config CSV → SO, `StageWorld` tick/fixed-point, Simon + roi + 5 vũ khí phụ, nến/item/bí mật, HUD/luật/tally, **mọi enemy/boss (Block 3 = Fleaman/Skeleton/Raven/Mummy theo WIKI — ước lượng CVGP 17/19)**; Map 1 + **Map 2 chơi trọn** (`StageScreen` stage 1–6, `StageFlow` cửa trái/phải + đổi tầng, `CastleMapScreen` B1/B2, `Stage7Stub`), bệ/bẫy/hố (CVM2 06), nhạc 16 bài (`MusicLibrary`), `RegressionRunner` + Bảng hash chuẩn (`CVM2_MEMORY §4`), harness nghiệm thu (`Map1AcceptProbe`, `Map2AcceptProbe`, `Map2RouteProbe`). |
| Nhạc Block 3 | `Assets/Data/Audio/Music/m05_wicked_child.mp3` (`eMusic.WickedChild`, đã phát ở `Stage7Stub`) — **bản rip chỉ 1 vòng rồi fade ⇒ điểm lặp 45.26 s ở đầu fade (gain 0.49 — nghe tụt âm)** (`CVM2 §6 03`); boss Poison Mind, Stage Clear có sẵn. |

## 2. Tài nguyên

### 2a. Sheet mới (routine 01 chép nguyên byte vào `Art_Source/spriters-resource/`)
| File ở `C:\Users\kvtuy\Downloads\` | Đích | Kích thước | sha256 | Dùng cho |
|---|---|---|---|---|
| `NES - Castlevania - Stages - Level 3_ Crumbling Tower.png` | `level3_walkway.png` | 3596×618 RGBA, 96 910 B | `c8bfc7ef3da18110c4f226276e7ae82e6611d2c4f3246517f5578bd476c44eee` | địa hình 5 section (05) |
Sheet `Tilesets.png` (CVM2 01 đã nhập) có bảng tile Level 3 ⇒ đối chiếu chéo (01).

### 2b. Kiểm kê Level 3 (planner — `inventory/level3_walkway.json`, tool `ref/tools/inv_level3_tiles.py`)
| Section | Stage | rect [x,y,w,h] | Màn | tile 8×8 / 16×16 | Ghi chú |
|---|---|---|---|---|---|
| `WalkwayEntranceA` | 07 | [1, 425, 768, 192] | 3 | 68 / 50 | vào cx 512 (cửa đỏ trang trí mép PHẢI), đi TRÁI; thang lên-trái góc trên-trái; cột lưới xanh cao (trang trí) |
| `WalkwayEntranceB` | 07 | [1, 218, 768, 192] | 3 | 47 / 43 | chân thang trái-dưới → đi PHẢI; bục/khe/hố; **CỬA PHẢI** cuối stage |
| `WalkwayA` | 08 | [774, 218, 768, 192] | 3 | 60 / 44 | cầu + lan can + tượng; thang lên-trái ở phần phải |
| `WalkwayB` | 08 | [1286, 11, 768, 192] | 3 | 60 / 43 | **camera tới cx −109** (trời bên trái NGOÀI vùng sheet); Bone Tower trên cầu; **CỬA PHẢI** |
| `MummyMan` | 09 | [2059, 11, **1536**, 192] | **6** | 100 / 59 | cầu dài + **đoạn hỏng (hố)**, tháp vỡ, bậc xuống phòng; **phòng Mummy 256 px cuối, khoá cx 1280** |
Tổng **125 / 83**. Nền khoá xanh (0,128,0); không có ô lạ trong vùng; logo credit [3371,585,224,32]. Dòng r ↔ **NES y = 32 + r**.

### 2c. Sprite Block 3 đã có (CVGP 05 — `CVGP_MEMORY §3 "Sprite gameplay đã cắt"`; KHÔNG cắt lại)
Fleaman = `Enemy/Fleaman/fleaman_0..1` · Raven = `Enemy/Raven/raven_0..3` · Skeleton trắng = `Enemy/WhiteSkeleton/skeleton_0..1` + `bone_0..2` · Ghost, Medusa Head, Bone Tower (`DragonSkullCannon`) có từ Block 2 (CVM2 08 đo) · Mummy = `Boss/Mummy/mummy_purple_0..2`, `mummy_red_0..2`, `bandage_0..1` · item `moneybag_{red,purple,white}`, `axe`, `dagger`, `holywater`, `roast`, `double`, `triple`, `heart_*`, `crystal_*` · `Object/door_0/1`. Thiếu thật (vd khung "Mummy đỏ" là sprite riêng hay bảng màu nháy?) ⇒ ghi `§5`.

## 3. Video (chi tiết + bảng ở `ref/MAP3_TIMELINE.md`)

### 3a. Hiệu chuẩn (planner, `ref/tools/calib_map3.py`, `calib_boss3.py`)
| | Map_3 (1488×760) | Boss_Map_3 (1370×772) |
|---|---|---|
| `x_NES = (x_v − ox)/sx − ½`, `y_NES = (y_v − oy)/sy − ½` | **sx 3.859, sy 3.310, ox 240.93, oy −21.92** (ZNCC 0.977 một frame / 0.894 TB 5 frame) | **sx 3.845, sy 3.363, ox 195.76, oy −21.77** (ZNCC gradient 0.94, phòng boss cx 1280) |
| Tỉ lệ pixel | 7/6 | 8/7 |

### 3b. Timeline rút gọn (phân đoạn tự động 5 fps — `_cache/work/map3_locate_5.csv`)
| Mốc | Sự kiện |
|---|---|
| 0:00–0:06.2 | **bản đồ lâu đài B2** (trượt vào 0.4–2.4, Simon nhỏ đi 2.8–6.0) — mẫu thứ 2 của bản đồ CVM2 11 đã dựng |
| **0:06.8** | **STAGE 07 — WEA cx 512**, TIME **0500**, đi TRÁI; Ghost?, Fleaman, Raven, nước thánh (0:14–0:16 đổi vũ khí), Skeleton góc dưới-trái (0:26–0:36) |
| 0:39.5 | đen (thang lên-trái) → **WEB cx 0** (0:40.0), đi PHẢI; Raven, Skeleton ném xương, **túi tiền bí mật +1000** (1:06–1:08) |
| **1:21.4–1:28.0** | **CỬA PHẢI #1** → **STAGE 08 — WalkwayA cx 0** (TIME 0432, không đặt lại); Medusa Head, Skeleton |
| 1:48.8 | đen (thang lên-trái) → **WalkwayB cx −43** … tới **cx −109**; **túi tiền bí mật +1000** (1:53.5–1:56); Bone Tower ×2 (2:04–2:14), Raven |
| **2:24.6–2:31.5** | **CỬA PHẢI #2** → **STAGE 09 — MummyMan cx 0** (TIME 0380); Bone Tower, Raven, **cầu hỏng + Skeleton nhảy qua** (2:50–2:58), tháp vỡ |
| ~3:06–3:18 | bậc xuống phòng, **2 Mummy đứng chờ** thấy trước khi khoá → **khoá cx 1280 ≈ 3:18.0** |
| 3:18–3:46 | Simon đứng khối cao trái ném **nước thánh** (Mummy đỏ khi trong lửa), băng gạc; **Mummy 1 chết +3000 (~3:43), Mummy 2 +3000 (~3:46)** |
| ~3:48–3:54 | **quả cầu** + **vật III rơi**; TIME đứng 0306; tally 3:54–4:07.4 (SCORE **073630**) |
| 4:08.6 | đen, HUD **STAGE 10** → 4:09.2 **bản đồ lâu đài B3** trượt vào → hết video 4:11.6 |
Simon **không mất mạng** (P-04 suốt), có trúng đòn vài lần.

### 3c. Video boss (chi tiết `MAP3_TIMELINE §5`)
Vào phòng (cuộn, cx ≈ 1200 → 1280) → phá khối thấp nhất cầu thang trái ⇒ thịt nướng (0:06) → khoá → 2 Mummy kẹp hai phía, ném băng gạc ngang thấp; Simon (dao + roi, P-01) trúng đòn nhiều lần → Mummy 1 chết ~0:28 (+3200?), Mummy 2 ~0:33 (+3000) → quả cầu hiện cao giữa phòng 0:35 rơi xuống → Simon tới nhặt ~0:40.

## 4. Wiki (tóm tắt — đầy đủ ở `wiki/MAP3_WIKI_DATA.md`)
- **Sát thương 3 vạch/đòn** từ Stage 7 (Unity đã có `ConfigBlock` block 3 = 3).
- St7: Ghost, Fleaman, Skeleton, Raven; Holy Water nến đầu; Pot Roast khối trái bộ khối nhô thứ 2; Skeleton góc dưới-trái; **ngồi mép phải bục thấp nhất ⇒ money bag bục cao bên trái (biến mất nếu chậm)**; Skeleton rơi hố vì gờ cao; **Axe nến thấp nhất (rơi mất nếu không nhặt ngay)**.
- St8: Medusa Head + Skeleton (dưới), Bone Tower + Raven (trên); Boomerang nến dưới cầu thang; **quỳ trên 2 khối không với tới bên phải cầu thang ⇒ money bag bên trái (có thể ngoài màn)**; Stop Watch nến phải cầu thang.
- St9: Raven cất cánh khi lại gần; **Skeleton nhảy qua cầu hỏng**; Pot Roast khối thấp nhất phòng boss; Dagger nến phòng boss; (bí mật) money bag trong phòng boss trước khi khoá.
- **Mummy Men**: 2 con, 48/32 đòn (tổng?), 3000 đ (**video: mỗi con +3000**), lang thang + ném băng gạc, kẹp 2 phía; Stopwatch vô dụng; Holy Water "hoá đá".

## 5. Kiến trúc — phần MỞ RỘNG cho Map 3 (giữ nguyên lõi CVGP + mở rộng CVM2; đổi ⇒ ghi lý do `CVM3_MEMORY §4`)

### 5.1 Dữ liệu theo Block (không phá Block 1 + Block 2)
- Python ở `Tools/cvm3/` (dùng lại `Tools/cvgp/*`, `Tools/cvm2/*` bằng import/tham số — **không đổi đầu ra Block 1/2**): `level3_sections.json`, `level3_collision_overrides.json`, `gen_map3_objects.py`, `map3_boss.json`.
- Asset: `Assets/Data/Art/Level/Block3/`, `Assets/Data/Config/Level/Block3/<Section>.asset`, `Assets/Data/Prefabs/Level/Block3/<Section>.prefab` + `Objects/<Section>_Objects.prefab`, replay `Assets/Data/Tests/Replays/map3_route.txt`.
- Builder: tổng quát builder CVM2 theo block **hoặc** `Cvm3*` gọi phần chung; menu `CVM3/*`. Điều kiện: chạy lại builder Block 1 + Block 2 ⇒ **md5 y hệt** + "ghi 0".
- Tên section (thống nhất): `WalkwayEntranceA`, `WalkwayEntranceB`, `WalkwayA`, `WalkwayB`, `MummyMan`.

### 5.2 Điểm mới so với Map 2
- **Cửa PHẢI** trở lại (như Map 1) nhưng mốc đo riêng TỪNG cửa ở Map_3 (Map_2: 2 cửa lệch 7–9 tick) ⇒ `Map3FlowTiming` (khuôn `Map2FlowTiming`, `AppliesTo(stage)` theo block). Map 1/2 không đổi.
- **Đổi tầng thang LÊN-TRÁI cắt mép trên** (WEA → WEB, WalkwayA → WalkwayB) — Map 2 chỉ có lên-phải ⇒ tổng quát hướng; phép dời giữa 2 section + cx đích đo video (WalkwayB hiện ở **cx âm**).
- **Camera ngoài vùng sheet** (WalkwayB cx −109): section NES rộng hơn sheet ⇒ thêm cột trời bên trái (chép tile trời CÓ SẴN trong sheet — không vẽ mới; H2) + gốc section/`cameraMinX` âm hoặc dời gốc; ghi `§4`.
- **Section 1536 px** (MummyMan) + **phòng boss ở mép PHẢI** của section dài (khoá cx 1280 — như Phantom Bat khoá phía phải, KHÁC Medusa cx 0); **2 Mummy đứng chờ trước khi khoá** (thấy ở cx ~1100).
- **Hố / cầu hỏng** (St7, St9): dùng hố CVM2 (rơi khỏi đáy ⇒ chết); Skeleton nhảy qua khe; **vật phẩm rơi xuống hố mất** (Axe St7 — wiki).
- **Bí mật kích hoạt bằng NGỒI** (St7, St8 — túi tiền hiện chỗ khác, có thể ngoài màn) + **túi tiền tự biến mất** sau một lúc (St7 — wiki; đo video); kiểm kiểu `SecretTrigger` có sẵn (CVGP 11 / CVM2 10) trước khi thêm.
- **Mummy Men = 2 thực thể BossBase chung 1 phòng** (CVGP 19 đã có `MummyMan` + `MummyBandage`): thanh ENEMY chung?, mỗi con +3000 (video), Holy Water làm Mummy đỏ/đứng, quả cầu khi CẢ HAI chết, **vật III rơi** sau trận (nguồn? — 09/10 xác định).
- **Sát thương 3 vạch** + knockback (04) ⇒ lộ trình tự động phải tính chuyện bị hất xuống hố.

### 5.3 Màn hình & luồng (routine 11)
`StageScreen` nhận stage 1–9; hết Block 2 ⇒ bản đồ B2 ⇒ **Stage 07 thật** (bỏ `Stage7Stub` khỏi luồng); hết Block 3 ⇒ tally ⇒ đen ⇒ **bản đồ lâu đài B3** (trượt vào — video chỉ có 2.4 s đầu; phần Simon đi ⇒ suy luận theo bản đồ B1/B2 + ghi rõ) ⇒ **`Stage10Stub`** (HUD STAGE 10, chờ cụm Map 4). Cheat dev `CVM3/Start Stage 7|8|9`. Nhạc Wicked Child liền 3 stage (03 CVM2 + đo lại Map_3), Poison Mind khi Mummy thức, Stage Clear khi nhặt cầu.

### 5.4 Kỷ luật hồi quy (dùng `RegressionRunner` của CVM2 00 — thêm bộ CVM3)
Bảng hash chuẩn khởi đầu = `CVM2_MEMORY §4` sau CVM2 12 (chép vào `CVM3_MEMORY §4` ở routine 00). Hash chỉ đổi khi routine CỐ Ý đổi hành vi (03, 04 chắc chắn; 07, 08, 09 đổi b23/b24/gallery; 11 đổi flow) — 1 dòng/hash có lý do. Mốc mới **`PRE7`** (hash luồng ngay trước tick STAGE 07 bật, đo ở routine 11 trên code TRƯỚC khi nối Block 3) chứng minh Map 1 + Map 2 không đổi khi nối Map 3.

## 6. Input
Không đổi (`CVGP_PLAN §6`). Gamepad thật vẫn chờ người dùng bấm thử.

## 7. Danh sách routine (chạy TUẦN TỰ — một Unity Editor + một cầu MCP)

Model · effort theo skill `skill-commond`: **Sonnet** cho việc cơ học (nhập file, đo theo công thức, chạy bộ hồi quy có sẵn), **Opus** cho mọi việc phải thiết kế/suy luận; effort `medium` (hạ tầng/nhập), `high` (mặc định — phạm vi khép kín), `xhigh` (tạo mới asset từ dữ liệu thô, đổi dữ liệu/lõi DÙNG CHUNG nhiều stage, AI nhiều ứng viên dễ kết luận sai, lắp ghép, nghiệm thu). Ghi ở frontmatter `model:`/`effort:` từng `SKILL.md` — `run-routines.ps1` tự đọc.

| # | taskId | Model · effort | Cổng | Việc chính |
|---|---|---|---|---|
| 00 | `cvm3-00-ha-tang-so-ghi-nho-va-baseline` | Sonnet 5.5 · medium | CVM2 12 | Sổ `CVM3_MEMORY`, commit đồ planner, `.gitignore`, kiểm mẫu số planner, **baseline = Bảng hash chuẩn CVM2 12** (3 phiên Play) + H8 — không đổi gameplay |
| 01 | `cvm3-01-nhap-sheet-level3-va-kiem-ke` | Sonnet 5.5 · medium | 00 | Chép nguyên byte sheet Level 3 + sha256 + `SOURCES.md`, chạy lại kiểm kê y hệt, đối chiếu sheet Tilesets, **kiểm đủ sprite Block 3** (manifest + pivot), thiếu ⇒ `§5` |
| 02 | `cvm3-02-hieu-chuan-video-timeline-va-su-kien` | Sonnet 5.5 · high | 00 | Hiệu chuẩn tinh 2 video, HUD từng frame, camera (cả **cx âm**) + Simon từng frame, chuyển cảnh theo tick, bảng sự kiện, **mọi lần Simon trúng đòn** (cho 04), âm thanh, bản đồ B2/B3 — viết lại `MAP3_TIMELINE.md` |
| 03 | `cvm3-03-dong-ho-stopwatch-theo-video` | Opus 5.5 · high | 00 | (= `cvm2-fix-01`) Stopwatch đóng băng theo video (Map_2 186–196 tick, giả thuyết 3 đv TIME = 192), TIME đứng cùng, nối lại theo nhịp TIME; số dùng chung ⇒ đổi hash Block 1/2 có lý do |
| 04 | `cvm3-04-simon-knockback-nuoc-thanh-mep-roi` | Opus 5.5 · **xhigh** | 00, 02 | (= `cvgp-fix-03`) Knockback Simon theo 4 video (31–32 px / 36 tick, giữ đỉnh; đo lại Map_1 + Block 3), rơi hố khi bị hất, quỹ đạo lọ nước thánh (Map_3 trận Mummy ném ~20 lọ), mép rơi; lõi Simon dùng chung ⇒ nhiều hash đổi có lý do |
| 05 | `cvm3-05-tilemap-va-luoi-va-cham-block3` | Opus 5.5 · **xhigh** | 01 (02 nên xong) | Tileset + 5 Tilemap Block 3 khớp từng pixel (WalkwayB thêm trời trái theo video), lưới va chạm (hố, cầu hỏng, thang lên-trái, khối phá, cửa phải, phòng boss), đồ thị section + điểm vào; Block 1 + 2 md5 y hệt |
| 06 | `cvm3-06-quet-hoi-quy-giua-cum` | Sonnet 5.5 · high | 03, 04, 05 | Quét hồi quy sau 2 sửa lõi + địa hình: mọi hash = bảng (đã gồm đổi có lý do), H8, luồng Map 1+2 đa fps, tất định, builder Block 1/2/3 idempotent, pixel-perfect Block 3, hiệu năng, repo |
| 07 | `cvm3-07-skeleton-va-fleaman-do-theo-video` | Opus 5.5 · **xhigh** | 05, 06 | Skeleton trắng (nhảy tới lui, ném xương 2–3 đường, rơi hố, nhảy qua cầu hỏng) + Fleaman (nhảy ngắn/vọt) đo Map_3, thay số "ước lượng CVGP 17"; dữ liệu dùng chung Block 4–6 |
| 08 | `cvm3-08-raven-enemy-dung-chung-va-roi-vat` | Opus 5.5 · **xhigh** | 07 | Raven (đậu → cất cánh → treo → lao → quay) đo Map_3; kiểm Ghost/Medusa Head/Bone Tower (số CVM2 08) trên Map_3 — chỉ đổi khi lệch có bằng chứng; **luật rơi vật enemy từ 3 video** (dựng chỉ khi luật giải thích mọi mẫu) |
| 09 | `cvm3-09-mummy-men-va-phong-boss-that` | Opus 5.5 · **xhigh** | 05, 06 | Mummy Men ×2 + băng gạc đo 2 video, phòng boss thật cuối MummyMan (khoá cx 1280, Mummy chờ trước khoá, khối thịt nướng, nến dao găm), nước thánh làm Mummy đỏ/đứng, điểm mỗi con, quả cầu + vật III |
| 10 | `cvm3-10-dat-vat-the-nen-bi-mat-va-spawn` | Opus 5.5 · **xhigh** | 05, 07, 08, 09 | `MAP3_PLACEMENT.md` từ video: nến + nội dung, enemy/bộ sinh (Medusa Head, Raven đậu, Skeleton, Fleaman, Ghost, Bone Tower), khối phá, **bí mật ngồi ⇒ túi tiền** (St7 biến mất, St8 ngoài màn), Axe rơi hố ⇒ `Map3Objects.json` + builder |
| 11 | `cvm3-11-luong-stage-7-9-va-stage10` | Opus 5.5 · **xhigh** | 10 | Luồng: bản đồ B2 → Stage 07 thật, thang lên-trái ×2, cửa phải ×2, khoá phòng Mummy, tally Block 3 → bản đồ B3 → `Stage10Stub`; nhạc Block 3; builder màn hình idempotent byte; lộ trình Title → STAGE 10 tất định + `PRE7` |
| 12 | `cvm3-12-nghiem-thu-map3-so-voi-video` | Opus 5.5 · **xhigh** | 11 | **Milestone**: nghiệm thu Map 3 so video (≥ 11 mặt) + hồi quy Map 1 + Map 2; ĐẠT/CHƯA + `cvm3-fix-NN` đề xuất + cụm Map 4 |

- **Mốc dừng tự nhiên**: sau **06** (2 nợ lõi đã trả + địa hình Block 3 chắc chắn — kết luận 06 quyết định chạy tiếp) và sau **12** (Map 3 chơi được — milestone người dùng yêu cầu).
- Chạy cả dãy: `C:\Users\kvtuy\Downloads\run-routines.ps1 -Prefix "cvm3" -From 0 -To 12 -ProjectDir "D:\Unity\Castlevania_Original-main"` (thêm `-DryRun` để xem trước). Script tự đọc model/effort ở frontmatter, dừng dãy nếu routine không tạo commit. Nên chạy thử **một** routine trước (`-From 0 -To 0`).
- Routine dừng ở cổng phụ thuộc vẫn commit phần ghi sổ `(dở dang)`; đọc `CVM3_MEMORY §1` để biết chạy lại cái nào.
- Sửa phát sinh: `cvm3-fix-NN-…` (routine nghiệm thu đề xuất, người dùng duyệt rồi mới tạo).

## 8. Hiến pháp cụm CVM3 (H1–H13 như `CVM2_PLAN §8`, mở rộng cho Block 3; thêm H14; muốn lệch ghi lý do `CVM3_MEMORY §4`)
- **H1** Video = chuẩn THỜI GIAN & CHUYỂN ĐỘNG; sheet = chuẩn PIXEL & BỐ CỤC; wiki = chuẩn CHỈ SỐ khi video không đo được. Mâu thuẫn ⇒ theo video, ghi `§4`. Không lấy pixel từ video làm sprite. **Video boss Map 3 khác bảng màu + watermark** ⇒ chỉ dùng cho chuyển động/thời gian.
- **H2** Không bịa art/âm thanh (không AI sinh, không tự vẽ, không cắt âm từ video). Thiếu ⇒ `§5`. Chép tile CÓ SẴN trong sheet để nới section theo bằng chứng video (vd trời trái WalkwayB) KHÔNG phải tự vẽ — ghi rõ ô nguồn.
- **H3** Không sửa submodule `Assets/TuyenAframework`.
- **H4** Gameplay theo tick `NesClock` 60 Hz, fixed-point; KHÔNG `Update`/`FixedUpdate`/`Time.deltaTime`/Rigidbody2D/Physics2D.
- **H5** Pixel-perfect (`NesCoord`, preset `NesSpriteImporter`).
- **H6** Số gameplay trong Config/hằng có tên, mỗi số có thẻ nguồn (`video:Map_3 m:ss`, `video:Boss_Map_3 ss.s`, `đo:cvm3-NN`, `wiki:…`, `suy luận: …` — importer chỉ nhận thẻ có sẵn: xem `CVGP_MEMORY §4`; thêm thẻ `video:Map_3`/`video:Boss_Map_3` nếu importer chưa nhận).
- **H7** Tất định (seed, thứ tự `StageWorld`).
- **H8** Không phá luồng CVTI (Start L352, đen L432, Intro L448, đen L890, Stage L922).
- **H9** Đúng phạm vi routine; việc khác ghi `§6`.
- **H10** Báo cáo trung thực (log + ảnh Play đã tự mở xem); chưa đạt ghi CHƯA ĐẠT.
- **H11 Không hồi quy Map 1 + Map 2**: sau mọi thay đổi gameplay/lõi, chạy bộ chạy hồi quy (`RegressionRunner`) — mọi hash phải trùng **Bảng hash chuẩn** `CVM3_MEMORY §4`, trừ khi routine CỐ Ý đổi hành vi (một dòng/hash: cũ → mới, lý do, routine). Sau routine 11: luồng thật Title → STAGE 10 hash mới + **`PRE7`** = hash trước STAGE 07 của code trước khi nối Block 3.
- **H12 Block 1 + Block 2 bất khả xâm phạm (dữ liệu)**: không sửa tay asset/prefab Block 1/2 sinh ra; builder tổng quát hoá phải tái tạo Block 1 + 2 **md5 y hệt**; dữ liệu Block 3 ở thư mục/khoá riêng (`Tools/cvm3/`, `…/Level/Block3/`, menu `CVM3/*`).
- **H13 Dữ liệu dùng chung**: Fleaman (St 7/11/13/14/17), Skeleton (St 7–9/13/17), Medusa Head (St 5/6/8/9/15), Bone Tower (St 6/8/9/15), Ghost (St 6/7/15), Stopwatch, knockback Simon — đổi số phải ghi nguồn mới, chạy lại probe enemy/boss + phòng trưng bày, xuất lại `ClaudeNotes/cvgp/CHARACTER_STATS.md` (`CVGP/Export Stats Report`).
- **H14 (mới) Sửa gộp đổi Map 1/2 có chủ đích (03, 04)**: mỗi thay đổi hành vi phải có khung video chứng minh (m:ss/frame), chạy lại TOÀN BỘ bộ hồi quy (Arena `full` + `core` + Main `main,flow30,flow144`), lộ trình replay (`map1_route`, `map2_route`) nếu lệch thì **chứng minh lệch do sửa** (tick đầu tiên khác + nguyên nhân) rồi mới ghi lại lộ trình/hash — không sửa kỳ vọng mù; ghi lại cả mốc `PRE4`/`caf21ae2` mới.

## 9. Câu hỏi mở (routine 00 chép vào `CVM3_MEMORY §5`; đừng tự quyết thay người dùng)
1. **Nhạc Wicked Child (Block 3)**: bản rip chỉ 1 vòng rồi fade ⇒ lặp ở đầu fade (gain 0.49 — nghe tụt âm mỗi ~45 s; Map 3 dài ~4 phút ⇒ ~5 lần). Người dùng có bản rip 2 vòng không? Không có ⇒ chấp nhận bù âm lượng theo đoạn (không sửa file) hay giữ nguyên?
2. **Còn chờ từ CVM2**: `cvgp-fix-04` (báo #3, Phantom Bat — chỉ Map 1); `CVM2 §5 #16` (pha TIME sau khi vào section — thống nhất ~58–60 cho Map 1/2?), `#17` (chờ tally 224 cho mọi block?); Block 3 dùng số đo riêng của Map_3, không đổi Map 1/2.
3. (vẫn mở) sheet HUD bản US (chữ số TẠM), bấm thử gamepad Flydigi.
4. **Cụm sau: Map 4 (Block 4, Stage 10–12, boss Frankenstein & Igor)** cần sheet `NES - Castlevania - Stages - Level 4_ …` (tải tay) + video `Map_4.mp4` (tiếp playthrough từ STAGE 10) + `Boss_Map_4_Behaviour.mp4`.
5. Mummy Men: video cho **mỗi con +3000** (wiki 3000 không nói rõ) — routine 09 theo video (H1); người dùng nhớ khác thì báo.
6. Người dùng viết "map 2" — planner hiểu là **Map 3** (video + walkthrough + sheet đều là Block 3). Sai thì dừng cụm.
7. Kết thúc game (credits/ending/Second Quest) — vẫn chờ.
