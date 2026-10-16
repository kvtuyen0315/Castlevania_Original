# CVM2 — Map 2 Castlevania (NES): Block 2 = Stage 04–06 chơi được như video (Tower Entrance → Tower Hall → Queen Medusa → STAGE 07)

> Soạn 15/10/2026 bởi session planner (Opus 5.5). **Mọi routine `cvm2-NN` đọc file này ĐẦU phiên** (cùng `CVM2_MEMORY.md`); cần chi tiết
> hạ tầng/gameplay cũ thì đọc `ClaudeNotes/cvgp/CVGP_MEMORY.md` (§3 tài nguyên + đường dẫn, §4/§4b API lõi, §7 cách chạy probe) và
> `ClaudeNotes/cvgp/CVGP_PLAN.md` (§5 kiến trúc, §6 input, §8 hiến pháp gốc). Số liệu planner = THÔ — **đo lại trước khi dựa vào**.

## 0. Yêu cầu gốc (nguyên văn, 15/10/2026)

> Tiếp tục dựng map 2 và play game theo như theo như video trên. Xem qua Walkthrough Stages 4 - 6 https://strategywiki.org/wiki/Castlevania/Walkthrough , https://strategywiki.org/wiki/Castlevania/Stages_4_-_6
> Nếu như thiếu file sprite thì vào link ( https://www.spriters-resource.com/nes/cv/ ) này xem để download
> => Hãy setup kỹ lưỡng . Nhớ kỹ là mỗi task hãy phân loại model và effort của AI sao cho hợp lý để đạt được chất lượng tốt giúp tôi

Kèm: 2 video `Map_2.mp4`, `Boss_Map_2_Behaviour.mp4` + ảnh thư mục Downloads có 2 sheet mới tải 15/10 (`NES - Castlevania - Stages - Level 2_ Main Hall.png`,
`NES - Castlevania - Miscellaneous - Tilesets.png`). Đây là câu trả lời cho câu hỏi mở `CVGP_MEMORY §5 #59` (duyệt cụm CVM2 + cung cấp sheet Level 2 + video Stage 4–6/Medusa).

**Quyết định planner (ghi để routine không hỏi lại):**
- Cụm CVM2 **gộp 3 việc nợ của CVGP chặn Map 2**: nhạc (`cvgp-fix-02` → `cvm2-03`), lọ nước thánh xuyên địch (`cvgp-fix-01` → `cvm2-04` — trận Medusa trong cả 2 video dùng nước thánh), dọn harness + bộ chạy hồi quy 1 lệnh (`cvgp-fix-05` → `cvm2-00`). `cvgp-fix-03` (Simon knockback/nước thánh/mép rơi) và `cvgp-fix-04` (báo #3, Phantom Bat) **chỉ liên quan Map 1 ⇒ KHÔNG gộp**, vẫn chờ người dùng duyệt (§9 #1) — routine 09 ghi thêm mẫu knockback từ video boss Map 2 cho fix-03 dùng sau.
- Sprite: **không cần tải thêm** — mọi thứ Block 2 đã có (sheet Enemies/Bosses/Items đã nhập từ CVGP + 2 sheet mới). spriters-resource bị Cloudflare chặn (memory) ⇒ nếu routine nào phát hiện thiếu thật, ghi `§5` cho người dùng tải tay, KHÔNG tự vượt bot-check.
- Bàn phím/gamepad, kiến trúc lõi, hiến pháp: **giữ nguyên CVGP** (người dùng đã chốt 11/10).

## 1. Nguồn

| Loại | Đường dẫn / ghi chú |
|---|---|
| Video Map 2 (chỉ đọc) | `C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_2.mp4` — 4:20.47, **1486×712**, 30 fps, AAC 48 kHz. **Tiếp đúng playthrough của Map_1** (SCORE 023370, P-03): bản đồ lâu đài → STAGE 04 → 05 → 06 → Queen Medusa → tally → bản đồ → **STAGE 07**. |
| Video boss (chỉ đọc) | `...\Boss_Map_2_Behaviour.mp4` — 23.30 s, **1490×718**, 30 fps. Playthrough khác (SCORE 005800), **bảng màu khác** (gạch tím) + watermark. Trận Queen Medusa đầy đủ: tượng → Medusa thoát → bay sóng + rắn → Simon trúng đòn nhiều lần → chết → quả cầu → nhặt. |
| Chỉ mục timeline + hiệu chuẩn | **`ClaudeNotes/cvm2/ref/MAP2_TIMELINE.md`** + ảnh `ref/nes/*_nes.png` (36) — planner đo THÔ, routine 02 đo kỹ |
| Wiki | **`ClaudeNotes/cvm2/wiki/MAP2_WIKI_DATA.md`** (chỉ mục vào `CV_WIKI_DATA.md` + chi tiết Stage 4–6) — văn bản thô `ClaudeNotes/cvgp/_cache/wiki_raw/text_Stages_4_-_6.txt`, ảnh bản đồ `ClaudeNotes/cvm2/_cache/wiki_img/` |
| Kiểm kê sheet | **`ClaudeNotes/cvm2/inventory/level2_main_hall.json`** + `level2_main_hall_annot.png` |
| Hạ tầng sẵn có (CVTI + CVGP) | Title/Intro (CVTI), `NesPad` + replay, 12 bảng Config (CSV → SO), `StageWorld` tick/fixed-point/`TileCollision`/`StairPath`/`StateMachine`/`CombatResolver`, Simon + roi + 5 vũ khí phụ, item/nến/bí mật, HUD + luật chơi + tally, **mọi enemy/boss (Block 2 theo WIKI — gần đúng)**, Map 1 trọn (`StageScreen`/`StageFlow`/`CastleMapScreen`/`Stage4Stub`), harness probe (hash ở §7 dưới). API: `CVGP_MEMORY §3–§4b`. |
| Nhạc | `Art_Source/music/` 16 MP3 (CHƯA import Unity — `cvm2-03`): 03 Vampire Killer, **04 Stalker**, 05 Wicked Child, …, **10 Poison Mind**, 12 Stage Clear, 15 Death, 16 Game Over. Block 2 theo bản NES gốc = Stalker (03 xác nhận bằng âm thanh video). |

## 2. Tài nguyên

### 2a. Sheet mới (routine 01 chép nguyên byte vào `Art_Source/spriters-resource/`)
| File ở `C:\Users\kvtuy\Downloads\` | Đích | Kích thước | sha256 | Dùng cho |
|---|---|---|---|---|
| `NES - Castlevania - Stages - Level 2_ Main Hall.png` | `level2_main_hall.png` | 2060×825 RGBA, 95 606 B | `6d09c60ef9b25ad19fe7138779030a0505d5c7d3b15a5599ea001d060b9313fd` | địa hình 6 section + bẫy ép 8 khung + bệ di động (05/06) |
| `NES - Castlevania - Miscellaneous - Tilesets.png` | `tilesets.png` | 1473×773 RGB, 180 462 B | `dd679ecd2f60f19534295d8a7c9ffdfb9c9a2f0dca5ec341016c31594e378491` | kiểm chéo tile 16×16 Level 2 (2 bảng màu Mesen/Nestopia); tham khảo Level 3–6 cho cụm sau |

### 2b. Kiểm kê Level 2 (planner — `inventory/level2_main_hall.json`, tool `ref/tools/inv_level2_tiles.py`)
| Vùng | Stage | rect [x,y,w,h] | Màn | tile 8×8 / 16×16 duy nhất | Ghi chú |
|---|---|---|---|---|---|
| TowerEntranceA | 04 | [1803, 632, 256, 192] | 1 | 32 / 22 | cửa đỏ mép trái (cửa vào), thang phải-lên |
| TowerEntranceB | 04 | [1547, 425, 512, 192] | 2 | 42 / 27 | khe có bệ di động, cửa TRÁI cuối stage |
| TowerHallA | 05 | [774, 425, 768, 192] | 3 | 43 / 26 | 2 tầng sàn + vòm; thang trái-lên |
| TowerHallB | 05 | [774, 218, 768, 192] | 3 | 56 / 37 | hố, bệ di động, cửa TRÁI tầng trên |
| TowerHallC | 06 | [1, 218, 768, 192] | 3 | 44 / 32 | tường đá xám; **3 ô teal (0,128,128) 32×64 = chỗ bẫy ép gai** (sheet để trống) |
| Medusa | 06 | [1, 11, 768, 192] | 3 | 64 / 44 | sảnh vòm + phòng boss trái (cửa sổ màu, **tượng Medusa vẽ sẵn trên nền**) |
| Crusher (sprite) | 06 | 8 × [1+33k, 425, 32, 64] | | | bẫy ép: 8 khung dài dần |
| Moving Platform (sprite) | 04/05 | [269, 425, 32, 16] | | | **= 4 × `Assets/Data/Art/Object/brick_0.png` khớp 0 px** (planner kiểm) — CVGP 05 ghi `brick_0` "chưa rõ công dụng": đó là khối bệ di động; xám (116,116,116) trong ô = khoá trong suốt |
Tổng **82 tile 8×8 / 69 metatile 16×16** (Level 1: 118/164). Nền khoá xanh (0,128,0); **xám (116,116,116) là màu THẬT trên bản đồ** (khác sheet nhân vật). Ghép stage: Stage 04 = TEA + TEB, 05 = Hall A + Hall B, 06 = Hall C + Medusa (khớp ảnh bản đồ wiki).

### 2c. Sprite Block 2 đã có (CVGP 05 — `CVGP_MEMORY §3 "Sprite gameplay đã cắt"`; KHÔNG cắt lại)
Spear Knight = `Enemy/BlackKnight` · Medusa Head = `Enemy/MedusaHead` · Bone Tower = `Enemy/DragonSkullCannon` + cầu lửa · Ghost = `Enemy/Ghost` · Bat = `Enemy/VampireBat` (đỏ + tím) · Queen Medusa = `Boss/Medusa` (`medusa_bust_0/1` nền đen thật, `medusa_head`, `medusa_stone`, `snake_0/1`) · item `cross`, `crown_0..3`, `chest_0..3`, `roast`, `double`, `triple`, `stopwatch` · `Object/door_0/1`, `brick_0` · bản đồ lâu đài `CastleMap/*`. Thiếu thật ⇒ ghi `§5`.

## 3. Video (chi tiết + bảng đầy đủ ở `ref/MAP2_TIMELINE.md`)

### 3a. Hiệu chuẩn (planner, `ref/tools/calib_map2.py`)
| | Map_2 (1486×712) | Boss_Map_2 (1490×718) |
|---|---|---|
| `x_NES = (x_v − ox)/sx`, `y_NES = (y_v − oy)/sy` | **sx 3.623, sy 3.104, ox 268.93, oy −20.24** (ZNCC 0.934) | **sx 3.609, sy 3.157, ox 274.33, oy −22.95** (0.843) |
| Tỉ lệ pixel | 7/6 (như Map_1) | 8/7 (như boss Map 1) |
Dòng r của vùng sheet ↔ **NES y = 32 + r** (khớp cả 6 vùng). Đo ảnh: so trên lưới NES (`ref/nes/`) — đừng scale đều 2 chiều.

### 3b. Timeline rút gọn (phân đoạn tự động từng frame — `_cache/work/map2_locate_30.csv`)
| Mốc | Sự kiện |
|---|---|
| 0:00–0:05.7 | bản đồ lâu đài (đuôi Map 1), STAGE 04 TIME 0000 → đen 0:05.77–0:06.20 |
| **0:06.23** | **STAGE 04 — TEA cx 0**, TIME 0400; nến đầu ⇒ thập tự (0:08); leo thang; +2000 vương miện (~0:19) |
| 0:28.50 | đen 11 frame → **TEB cx 256**, đi TRÁI; Spear Knight, dơi; bệ di động 0:44–0:51 |
| **0:51.80–0:57.67** | **CỬA TRÁI** (camera cuộn trái, cửa mở, Simon tự đi trái, đóng, cuộn) |
| **0:57.70** | **STAGE 05 — Hall A cx 512**, đi TRÁI; Medusa Head; rương báu +2000 (1:20–1:22); thang trái lên |
| 1:33.00 | đen → **Hall B cx 0**, đi PHẢI tới 512 (1:54; thịt nướng 1:56), quay TRÁI tầng trên; bệ di động 1:40–1:46 |
| **2:22.33–2:28.30** | **CỬA TRÁI** → **STAGE 06 — Hall C cx 512** (2:28.33), đi TRÁI; thịt nướng; **3 bẫy ép gai** 2:33–2:41; Bone Tower, Ghost, III (2:50); thang trái lên tầng trên (cx 0 2:52) → đi PHẢI |
| 3:17.13 | đen → **sảnh Medusa cx 512**, đi TRÁI (Medusa Head) → phòng boss khoá **cx 0** (~3:46) |
| 3:46–3:56 | **Queen Medusa** (+3000 ~3:56) → quả cầu 3:58 (TIME 0210) → tally 4:00–4:12 (SCORE 44670 = luật Map 1 ✓) |
| 4:12.80 | đen 31 frame → **bản đồ lâu đài Block 2**, HUD **STAGE 07** tim 05 → đen 4:19.73 → hết video |
**Map 2 đi NGƯỢC Map 1**: 2 cửa cuối stage ở **mép TRÁI**, nhiều section vào từ mép phải, phòng boss ở trái (khoá cx 0) ⇒ lõi phải hỗ trợ hướng trái.

### 3c. Video boss (chi tiết `MAP2_TIMELINE §4`)
Tượng Medusa giữa phòng → ~2.5 s đầu Medusa bay ra (thân tượng đỏ-trắng **còn lại** ở bệ) → bay sóng thấp, đổi màu xanh↔tím liên tục, rắn rơi + trườn, Simon trúng đòn 16 → 4 vạch → ~16.5 s lửa chết → ~19 s quả cầu → nhặt +3000.

## 4. Wiki (tóm tắt — đầy đủ ở `wiki/MAP2_WIKI_DATA.md`)
- Stage 4: chỉ Spear Knight + Bat; Boomerang nến đầu; phá tường phải trên thang đầu ⇒ vương miện 2000; khối góc bục ⇒ Shot Multiplier; Spear Knight chắn cửa cuối.
- Stage 5: Medusa Head suốt stage từ phía xa, **hố rải rác** (Head hất xuống hố); dao găm nến giữa tầng dưới; đứng trên 2 khối cao ⇒ Treasure Box 2000; Pot Roast bậc cao nhất góc trên-phải + Stop Watch nến gần đó.
- Stage 6: **bẫy gai** (3 cái; cái thứ 2 hạ ngắn hơn nhiều; Stopwatch đóng băng được), Bone Tower, Ghost; Pot Roast khối đầu stage; Shot Multiplier khối chân cầu thang; (bí mật) money bag đứng yên ở vòm thứ 4 sảnh có Medusa Head.
- Queen Medusa: 12/8 đòn, 3000 đ, vỡ ra từ tượng, lao vào Simon bay thấp, thả rắn (1 đòn roi), Stopwatch có tác dụng. Sát thương Simon 2 vạch.

## 5. Kiến trúc — phần MỞ RỘNG cho Map 2 (giữ nguyên lõi CVGP `CVGP_PLAN §5` + `CVGP_MEMORY §4b`; đổi ⇒ ghi lý do `CVM2_MEMORY §4`)

### 5.1 Dữ liệu theo Block (không phá Block 1)
- Công cụ python mới ở `Tools/cvm2/` (dùng lại `Tools/cvgp/*` bằng import/tham số — **không sửa hành vi Block 1**). Dữ liệu: `Tools/cvm2/level2_sections.json`, `level2_collision_overrides.json`, `gen_map2_objects.py`.
- Asset: `Assets/Data/Art/Level/Block2/` (tileset + Tile), `Assets/Data/Config/Level/Block2/<Section>.asset` (`LevelSectionData` — dùng lại lớp), `Assets/Data/Prefabs/Level/Block2/<Section>.prefab` + `Objects/<Section>_Objects.prefab`, replay `Assets/Data/Tests/Replays/map2_route.txt`.
- Builder Unity: tổng quát hoá `CvgpLevelBuilder`/`CvgpMap1Builder` theo block (tham số) **hoặc** builder `Cvm2*` mới gọi lại phần chung — điều kiện bắt buộc: chạy lại builder Block 1 ⇒ **md5 284 file Block 1 y hệt** + "ghi 0". Menu mới `CVM2/*`.
- Tên section (dùng thống nhất): `TowerEntranceA`, `TowerEntranceB`, `TowerHallA`, `TowerHallB`, `TowerHallC`, `Medusa`.

### 5.2 Hướng trái (Map 2 đi ngược Map 1)
- **Cửa**: `LevelSectionData.Connection` (kind Door) cần hướng (đề xuất `dir` −1/+1, mặc định +1 cho dữ liệu Block 1 cũ); `StageFlow` cuộn camera theo dấu, cánh cửa/cột vẽ ở mép đúng, Simon tự đi theo dấu, section đích đặt LIỀN TRÁI (gốc âm). Thời lượng đo lại ở video Map_2 (planner: 5.9–6.0 s, Map 1 6.8 s) — khác thì tách hằng theo block, giống thì dùng chung (`Map1FlowTiming` → tên trung tính hoặc thêm `Map2FlowTiming`).
- **Điểm vào stage/section**: Simon có thể bắt đầu ở mép phải, quay TRÁI; camera đặt cx tối đa (`StageRules.GetStageStart` thêm hướng/cx).
- **Đổi tầng cắt mép trên** (TEA→TEB, Hall A→B, Hall C→Medusa): như CA↔CB nhưng hướng LÊN sang section khác chiều rộng/cx khác — dữ liệu đích = (section, phép dời, cx camera) đo video.
- **Khoá camera phòng boss ở cx 0** (Map 1 khoá 512 — `BossArena`/`NesCameraController` tổng quát).

### 5.3 Cơ chế mới (routine 06)
- **Bệ di động** (`MovingPlatform` — Entity, KHÔNG Update): AABB đặc mặt trên (one-way?) chở Simon/enemy; biên + tốc độ + chu kỳ đo video (TEB 0:44–0:51, Hall B 1:40–1:46); thứ tự tick trong `StageWorld` phải chốt (bệ đi trước, kéo người đứng trên — ghi `§4`).
- **Bẫy ép gai** (`Crusher`): 8 khung sheet, chu kỳ hạ/nâng + lệch pha 3 cái + độ hạ (cái 2 ngắn) đo video 2:33–2:41; chạm ⇒ sát thương 2 vạch + knockback **hay chết ngay** — ĐO video/ghi nguồn (wiki chỉ nói "spike traps"); Stopwatch đóng băng (wiki).
- **Hố chết** (`Pit`): rơi khỏi đáy section ⇒ mất mạng (Block 1 chỉ có nước); Medusa Head/knockback hất xuống hố là hệ quả tự nhiên.
- **Tượng Medusa** là tile nền ⇒ lúc thức: `LevelSection.ClearTileRect` (+ sprite tượng/thân còn lại theo video boss) — routine 09.

### 5.4 Màn hình & luồng (routine 11)
`StageScreen` nhận stage 1–6 (bảng start theo stage), `StageFlow` chạy Block 2; hết Block 1 ⇒ `CastleMap` ⇒ **Stage 04 thật** (bỏ `Stage4Stub` khỏi luồng — giữ file nếu còn tham chiếu, ghi `§4`); hết Block 2 ⇒ tally ⇒ `CastleMap` (biến thể Block 2: khung nhìn/marker/đường Simon đo video 4:13–4:20) ⇒ **`Stage7Stub`** (HUD STAGE 07, nền đen, chờ cụm Map 3). Menu/cheat dev bắt đầu thẳng Stage 4/5/6.

### 5.5 Kỷ luật hồi quy (routine 00 dựng — mọi routine dùng)
Bộ chạy 1 lệnh (`CvgpRegressionRunner` hoặc tên tương tự, chỉ Editor) chạy nối tiếp **mọi probe CVGP + probe CVM2 mới** qua `EditorApplication.update`, ghi `ClaudeNotes/cvm2/_cache/<NN>/suites/*.txt` + bảng hash, so với **bảng hash chuẩn** `CVM2_MEMORY §4 "Bảng hash"`. Hash chỉ được đổi khi routine CỐ Ý đổi hành vi — ghi lý do + hash mới vào bảng (routine 04, 08, 09 chắc chắn đổi một số hash).

## 6. Input
Không đổi (`CVGP_PLAN §6`). Gamepad thật vẫn chờ người dùng bấm thử (`CVGP_MEMORY §5 #39b`).

## 7. Danh sách routine (chạy TUẦN TỰ — một Unity Editor + một cầu MCP)

Model · effort theo skill `skill-commond`: **Sonnet** cho việc cơ học (nhập file, đo theo công thức, chạy bộ hồi quy), **Opus** cho mọi việc phải thiết kế/suy luận; effort `medium` (nhập/kiểm kê) / `high` (mặc định) / `xhigh` (tạo mới asset từ dữ liệu thô, cơ chế lõi mới xuyên hệ, đổi dữ liệu DÙNG CHUNG nhiều stage, lắp ghép, nghiệm thu). Ghi ở frontmatter `model:`/`effort:` từng `SKILL.md` — `run-routines.ps1` tự đọc.

| # | taskId | Model · effort | Cổng | Việc chính |
|---|---|---|---|---|
| 00 | `cvm2-00-ha-tang-so-ghi-nho-bo-chay-hoi-quy` | Sonnet 5.5 · high | — | Sổ `CVM2_MEMORY`, commit đồ planner, **bộ chạy hồi quy 1 lệnh** (gộp `cvgp-fix-05`: RulesProbe trả StageRules/AutoRespawn, `BossB24Probe.Cleanup` → `DestroyImmediate`, tham số spam) + bảng hash chuẩn + H8 |
| 01 | `cvm2-01-nhap-sheet-level2-va-sprite-moi` | Sonnet 5.5 · medium | 00 | Chép nguyên byte 2 sheet + sha256 + `SOURCES.md`, chạy lại kiểm kê y hệt, cắt sprite bẫy ép 8 khung + bệ di động, đối chiếu tile ↔ sheet Tilesets |
| 02 | `cvm2-02-hieu-chuan-video-timeline-va-su-kien` | Sonnet 5.5 · high | 00 | Hiệu chuẩn tinh 2 video, đọc HUD từng frame, vị trí Simon, mọi chuyển cảnh theo tick, danh sách sự kiện/enemy/vật phẩm — mở rộng `MAP2_TIMELINE.md` |
| 03 | `cvm2-03-nhac-16-bai-va-moc-nhac-map1-map2` | Opus 5.5 · high | 00, 02 | (= `cvgp-fix-02` mở rộng) nhập 16 MP3, `eMusic`, xác nhận bài từng block bằng tương quan chéo âm thanh 2 video, gắn mốc nhạc Map 1 + Map 2 |
| 04 | `cvm2-04-lo-nuoc-thanh-xuyen-dich` | Opus 5.5 · high | 00 | (= `cvgp-fix-01`) lọ nước thánh bay XUYÊN enemy/boss, chỉ vỡ khi chạm sàn (bằng chứng 2 video boss); cập nhật kỳ vọng probe + bảng hash |
| 05 | `cvm2-05-tilemap-va-luoi-va-cham-block2` | Opus 5.5 · **xhigh** | 01 (02 nên xong) | Tileset dedupe + 6 Tilemap khớp từng pixel + lưới va chạm (hố, thang, khối phá, cửa trái, vùng bẫy, tượng) + đồ thị section; builder tổng quát, Block 1 md5 y hệt |
| 06 | `cvm2-06-be-di-dong-bay-ep-gai-va-ho-chet` | Opus 5.5 · **xhigh** | 05 | Cơ chế mới đo video: bệ di động, bẫy ép gai, hố chết; phòng thử Arena + probe |
| 07 | `cvm2-07-quet-hoi-quy-giua-cum` | Sonnet 5.5 · high | 03–06 | Quét hồi quy: bộ chạy 1 lệnh, H8, luồng Map 1, tất định đa fps, hiệu năng, builder idempotent, repo |
| 08 | `cvm2-08-enemy-block2-do-lai-theo-video` | Opus 5.5 · **xhigh** | 06 (+ đọc kết luận 07) | Spear Knight, Medusa Head, dơi, Bone Tower, Ghost: thay số "ước lượng routine 17" bằng số ĐO Map_2 (dữ liệu dùng chung tới Block 3–5) |
| 09 | `cvm2-09-queen-medusa-va-phong-boss-that` | Opus 5.5 · **xhigh** | 05, 08 | Queen Medusa + rắn đo 2 video, phòng boss thật (tượng trên nền, khoá cx 0), quả cầu |
| 10 | `cvm2-10-dat-vat-the-nen-bi-mat-va-spawn` | Opus 5.5 · **xhigh** | 05, 06, 08, 09 | `MAP2_PLACEMENT.md` từ video: nến + nội dung, enemy/bộ sinh, bệ, bẫy, khối phá, 5 bí mật ⇒ `Map2Objects.json` + builder |
| 11 | `cvm2-11-luong-stage-4-6-cua-trai-va-stage07` | Opus 5.5 · **xhigh** | 03, 10 | Luồng: bản đồ → Stage 04 thật, cửa trái, đổi tầng, khoá boss, tally → bản đồ Block 2 → `Stage7Stub`; lộ trình Title → STAGE 07 tất định |
| 12 | `cvm2-12-nghiem-thu-map2-so-voi-video` | Opus 5.5 · **xhigh** | 11 | **Milestone**: nghiệm thu Map 2 so video (≥ 10 mặt) + hồi quy Map 1 = 0; ĐẠT/CHƯA + routine `cvm2-fix-NN` đề xuất |

- **Mốc dừng tự nhiên**: sau **07** (nợ CVGP đã trả + địa hình + cơ chế mới chắc chắn) và sau **12** (Map 2 chơi được — milestone người dùng yêu cầu).
- Chạy cả dãy: `C:\Users\kvtuy\Downloads\run-routines.ps1 -Prefix "cvm2" -From 0 -To 12 -ProjectDir "D:\Unity\Castlevania_Original-main"` (thêm `-DryRun` để xem trước). Script tự đọc model/effort ở frontmatter, dừng dãy nếu routine không tạo commit. Nên chạy thử **một** routine trước (`-From 0 -To 0`).
- Routine dừng ở cổng phụ thuộc vẫn commit phần ghi sổ `(dở dang)`; đọc `CVM2_MEMORY §1` để biết chạy lại cái nào.
- Sửa phát sinh: `cvm2-fix-NN-…` (routine nghiệm thu đề xuất, người dùng duyệt rồi mới tạo).

## 8. Hiến pháp cụm CVM2 (H1–H10 giữ nguyên `CVGP_PLAN §8`; thêm H11–H13; muốn lệch ghi lý do `CVM2_MEMORY §4`)
- **H1** Video = chuẩn THỜI GIAN & CHUYỂN ĐỘNG; sheet = chuẩn PIXEL & BỐ CỤC; wiki = chuẩn CHỈ SỐ khi video không đo được. Mâu thuẫn ⇒ theo video, ghi `§4`. Không lấy pixel từ video làm sprite. **Video boss Map 2 khác bảng màu** ⇒ chỉ dùng cho chuyển động/thời gian, không cho màu.
- **H2** Không bịa art/âm thanh (không AI sinh, không tự vẽ, không cắt âm từ video). Thiếu ⇒ `§5`.
- **H3** Không sửa submodule `Assets/TuyenAframework`.
- **H4** Gameplay theo tick `NesClock` 60 Hz, fixed-point; KHÔNG `Update`/`FixedUpdate`/`Time.deltaTime`/Rigidbody2D/Physics2D.
- **H5** Pixel-perfect (`NesCoord`, preset `NesSpriteImporter`).
- **H6** Số gameplay trong Config/hằng có tên, mỗi số có thẻ nguồn (`video:Map_2 m:ss`, `video:Boss_Map_2 ss.s`, `đo:cvm2-NN`, `wiki:…`, `suy luận: …`).
- **H7** Tất định (seed, thứ tự `StageWorld`).
- **H8** Không phá luồng CVTI (Title L0, Start L352, đen L432, Intro L448, đen L890, Stage L922 — `CVTI_MEMORY §7`).
- **H9** Đúng phạm vi routine; việc khác ghi `§6`.
- **H10** Báo cáo trung thực (log + ảnh Play đã tự mở xem); chưa đạt ghi CHƯA ĐẠT.
- **H11 (mới) Không hồi quy Map 1**: sau mọi thay đổi gameplay/lõi, chạy bộ chạy hồi quy 1 lệnh (00) — mọi hash CVGP + luồng thật Title→STAGE 04 (`e0b2d54c`; sau routine 11: Title→STAGE 07 hash mới) phải trùng **bảng hash chuẩn**, trừ khi routine CỐ Ý đổi hành vi (ghi lý do + hash mới vào bảng — một dòng/hash).
- **H12 (mới) Block 1 bất khả xâm phạm**: không sửa tay asset/prefab Block 1 sinh ra; builder tổng quát hoá phải tái tạo Block 1 **md5 y hệt**; dữ liệu Block 2 ở thư mục/khoá riêng.
- **H13 (mới) Dữ liệu dùng chung**: số của enemy/boss Block 2 cũng dùng ở Block 3–5 (Medusa Head St 8–9/15, Bone Tower St 8–9/15, Ghost St 7/15, Spear Knight…) — đổi số phải ghi nguồn mới, chạy lại `EnemyB23Probe`/`EnemyB46Probe`/`BossB24Probe`/gallery `CharacterAcceptProbe`, cập nhật `CHARACTER_STATS.md` (exporter `CVGP/Export Stats Report`).

## 9. Câu hỏi mở (routine 00 chép vào `CVM2_MEMORY §5`; đừng tự quyết thay người dùng)
1. **`cvgp-fix-03` (Simon knockback/nước thánh/mép rơi) + `cvgp-fix-04` (báo #3, Phantom Bat)** — chỉ Map 1, chưa tạo; người dùng duyệt thì tạo sau (routine 09/12 ghi thêm mẫu đo từ Map 2 nếu có).
2. (vẫn mở từ CVGP) sheet HUD bản US (chữ số TẠM) — `CVGP_MEMORY §5 #39a`; bấm thử gamepad Flydigi — `#39b`.
3. Bẫy ép gai: chạm = mất 2 vạch + knockback hay chết ngay? Video có thể không cho thấy (Simon né được) ⇒ routine 06 chọn theo bằng chứng có được, ghi `suy luận`, hỏi lại ở đây.
4. Cụm sau: **Map 3 (Block 3, Stage 7–9, boss Mummy Men)** cần sheet `NES - Castlevania - Stages - Level 3_ ...` (spriters-resource — tải tay) + video `Map_3.mp4` / `Boss_Map_3_Behaviour.mp4`. Sheet Tilesets đã có bảng tile Level 3–6 nhưng KHÔNG thay được sheet bản đồ.
5. Kết thúc game (credits/ending/Second Quest) — vẫn chờ (`CVGP_MEMORY §5 #60`).
