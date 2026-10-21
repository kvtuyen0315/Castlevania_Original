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
| 01 | cvm3-01-nhap-sheet-level3-va-kiem-ke | chưa chạy | | | |
| 02 | cvm3-02-hieu-chuan-video-timeline-va-su-kien | chưa chạy | | | |
| 03 | cvm3-03-dong-ho-stopwatch-theo-video | chưa chạy | | | |
| 04 | cvm3-04-simon-knockback-nuoc-thanh-mep-roi | chưa chạy | | | |
| 05 | cvm3-05-tilemap-va-luoi-va-cham-block3 | chưa chạy | | | |
| 06 | cvm3-06-quet-hoi-quy-giua-cum | chưa chạy | | | |
| 07 | cvm3-07-skeleton-va-fleaman-do-theo-video | chưa chạy | | | |
| 08 | cvm3-08-raven-enemy-dung-chung-va-roi-vat | chưa chạy | | | |
| 09 | cvm3-09-mummy-men-va-phong-boss-that | chưa chạy | | | |
| 10 | cvm3-10-dat-vat-the-nen-bi-mat-va-spawn | chưa chạy | | | |
| 11 | cvm3-11-luong-stage-7-9-va-stage10 | chưa chạy | | | |
| 12 | cvm3-12-nghiem-thu-map3-so-voi-video | chưa chạy | | | |

## §2 Số đo

### 2a. Hiệu chuẩn video → lưới NES (planner 17/10 — `CVM3_PLAN §3a`; routine 02 đo kỹ)
| | Map_3 (1488×760) | Boss_Map_3 (1370×772) |
|---|---|---|
| `x_NES = (x_v − ox)/sx − ½`, `y_NES = (y_v − oy)/sy − ½` | **sx 3.859, sy 3.310, ox 240.93, oy −21.92** (ZNCC 0.977 / 0.894 TB 5 frame) | **sx 3.845, sy 3.363, ox 195.76, oy −21.77** (ZNCC gradient 0.94) |
| Tỉ lệ pixel | 7/6 | 8/7 |

Kiểm lại routine 00 (21/10): `calib_map3.py map3 12:WalkwayEntranceA:512 92:WalkwayA:0 140:WalkwayB:512` ⇒ sx 3.860 / sy 3.310 / ox 240.40 / oy −21.92, ZNCC 0.9786 — lệch planner sx 0.001, sy 0, ox 0.53 px video (≈ 0.14 px NES), oy 0 ⇒ đạt (≤ 0.005 / ≤ 1 px).

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

## §4 Quyết định kỹ thuật + Bảng hash chuẩn

### Bảng hash chuẩn (H11 — baseline = Bảng hash chuẩn sau CVM2 12; routine 00 đo lại 21/10/2026, code = HEAD `d1bf216`)
Hash đổi chỉ khi routine CỐ Ý đổi hành vi: thêm 1 dòng vào "Lịch sử đổi hash" (cũ → mới, lý do, routine) **và** sửa `RegressionRunner.EXPECTED`. Cờ `EXPECTED` hiện tại (đọc, không sửa): y hệt bảng dưới (long…rules, core, gallery, flow/flow30/flow144/map2, holy, mech, h8 `352/432/448/890/922`) + `PRE4 = 165b9aef`.

| Bộ | Lệnh | Scene | Hash chuẩn (đo 21/10) | Đạt/tổng | Thời gian | Routine đặt | Ngày |
|---|---|---|---|---|---|---|---|
| long | `RegressionRunner.Start("long", fps, dir)` | Arena (Simon) | `5963c4ba` | 1/1 | 21 s | CVGP 11/21 → CVM2 00 | 21/10 |
| simon | `…Start("simon",…)` | Arena (Simon) | `b2954cbe` | 20/20 | 38 s | CVGP 08/21 → CVM2 00 | 21/10 |
| attack | `…Start("attack",…)` | Arena (Simon) | `eee2cdfb` | 21/21 | 26 s | CVGP 09/21 → CVM2 00 | 21/10 |
| item | `…Start("item",…)` | Arena (Simon) | `7ca5bf56` | 14/14 | 33 s | CVGP 11/21 → CVM2 00 | 21/10 |
| enemy | `…Start("enemy",…)` | Arena (Simon) | `f288b0ce` | 20/20 | 104 s | CVGP 13/21 → CVM2 00 | 21/10 |
| boss | `…Start("boss",…)` | Arena (Simon) | `31e6d711` | 12/12 | 109 s | CVGP 14/21 → CVM2 00 | 21/10 |
| b23 | `…Start("b23",…)` | Arena (Simon) | `5eb00c5b` | 35/35 | 182 s | CVGP 17/21 → CVM2 08 | 21/10 |
| b46 | `…Start("b46",…)` | Arena (Simon) | `58a8edf9` | 26/26 | 135 s | CVGP 18/21 → CVM2 00 | 21/10 |
| b24 | `…Start("b24",…)` | Arena (Simon) | `9b218696` | 21/21 | 249 s | CVGP 19/21 → CVM2 09 | 21/10 |
| b56 | `…Start("b56",…)` | Arena (Simon) | `6cb8a1ad` | 14/14 | 221 s | CVGP 20/21 → CVM2 00 | 21/10 |
| holy | `…Start("holy",…)` | Arena (Simon) | `b1033a3f` | 5/5 | 13 s | CVM2 04 → CVM2 09 | 21/10 |
| mech | `…Start("mech",…)` | Arena (Simon) | `91215ebd` | 14/14 | 54 s | CVM2 06 | 21/10 |
| rules | `…Start("rules",…)` | Arena (Simon) | `03fafb24` | 68/68 | 0 s | CVGP 12/21 → CVM2 00 | 21/10 |
| gallery | `…Start("gallery",…)` (240 fps, không chụp; nằm trong `full`) | Arena (Simon) | `ffd5b71e` | 75/75 | 486 s | CVGP 21 → CVM2 11 | 21/10 |
| core | `…Start("core",…)` | Arena, `CVGP.ArenaUseDummy` = true | `76516076` | 21/21 | 92 s | CVGP 07/21 → CVM2 00 | 21/10 |
| flow (Title → STAGE 07 + trước STAGE 04 = `165b9aef`) | `…Start("flow",…)` | Main | `caf21ae2` | 2/2 | 294 s | CVM2 11 | 21/10 |
| flow30 / flow144 | `…Start("flow30,flow144",…)` | Main | `caf21ae2` (cả hai) | 2/2 ×2 | 294 s mỗi bộ | CVM2 11 | 21/10 |
| h8 (5 mốc CVTI, kèm mọi `flow*`) | tự chạy | Main | Start **L352** · đen **L432** · Intro **L448** · đen **L890** · Stage **L922** | 5/5 ×3 | 0 s | CVTI → CVM2 00 | 21/10 |
| map2 (`Map2RouteProbe.Replay` tua nhanh) | `…Start("map2",…)` (trong `main`) | Main | `caf21ae2` | 2/2 | 1 s | CVM2 11 | 21/10 |
| spam1..4 (Map 1) | `…Start("spam",…)` (trong `main`) | Main | *(không hash)* 0 lỗi console | 4/4 | 60–80 s mỗi lượt | CVGP 16/21 | 21/10 |
| spam5..7 (Block 2) | `…Start("spamb2",…)` (trong `main`) | Main | *(không hash)* 0 lỗi console | 3/3 | 60–80 s mỗi lượt | CVM2 11 | 21/10 |

Thời gian đo 21/10 (@60 fps, harness cùng máy): Arena `full` **1672 s ≈ 27.9 phút** (nhiều hơn CVM2 00: 1527 s); core 92 s; Main `main,flow30,flow144` **1383 s ≈ 23 phút** (y hệt CVM2 11). Cả ba `ALL.done` = `OK` (bỏ qua 0, lệch 0, lỗi 0).

**Lịch sử đổi hash:** kế thừa toàn bộ `CVM2_MEMORY §4 "Lịch sử đổi hash"`; cụm CVM3 ghi tiếp dưới đây (cũ → mới, lý do, routine).
- (00, 21/10) **không hash nào đổi** — baseline. Mọi hash của bảng KHỚP.

### Bộ chạy hồi quy (kế thừa `CVM2_MEMORY §4 "Bộ chạy hồi quy"`)
Chỉ Editor, đang Play, `runInBackground = true`. Gọi bằng `execute_code` (codedom): `RegressionRunner.Start("<bộ>", 60, "D:/Unity/Castlevania_Original-main/ClaudeNotes/cvm3/_cache/<NN>/<tên phiên>")`; kết quả ở `<dir>/suites/{SUMMARY.txt,progress.txt,ALL.done,<bộ>.txt}`; chờ bằng vòng lặp bash `[ -f <dir>/suites/ALL.done ]`. **3 phiên Play**:
1. Arena, Player = Simon (`CVGP.ArenaUseDummy` = false): `Start("full", 60, …)` ≈ 28 phút.
2. Arena với EditorPrefs `CVGP.ArenaUseDummy` = true (đặt TRƯỚC khi Play, scene Arena mở): `Start("core", 60, …)` ≈ 1.5 phút.
3. Main.unity: `Start("main,flow30,flow144", 60, …)` ≈ 23 phút.
Đọc `SUMMARY.txt` + `ALL.done` — `OK (bỏ qua n)` ≠ đạt. Mỗi phiên: đặt `EditorSettings.enterPlayModeOptionsEnabled = true` + `DisableDomainReload`, AppActivate cửa sổ Unity + `runInBackground = true` sau khi vào Play. Cuối phiên: `ArenaUseDummy` = false, tắt EnterPlayModeOptions, ghi lại `ProjectSettings/EditorSettings.asset` từ HEAD (CRLF).

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

## §6 Nợ & việc ngoài phạm vi (chép mục còn mở liên quan Block 3 từ `CVM2_MEMORY §6`)
- (CVM2 ACCEPT12) Đồng hồ 300 vs video 186–196 tick → routine 03; knockback 12 vs 31–32 px → routine 04; enemy rơi vật → routine 08.
- (CVM2 03) **Đồng hồ dừng nhạc 2.40 s** (Map_2 2:40.97, 1 lần đo) — chưa dựng; Stage 8 có nến đồng hồ ⇒ routine 03 kiểm bằng Map_3 (`CVM2 §5 #7`).
- (CVM2 03) Wicked Child lặp tụt âm (§5 #1).
- (CVM2 03) Prefab màn hình còn trường YAML cũ `music`/`stageMusic`/… `{fileID: 0}` — tự mất khi prefab lưu lại.
- (CVM2 05) `StageFlow` cửa trái / `autoBossLock` theo phía / `simonStartDir` đã xử lý ở CVM2 11 — Block 3 có **cửa PHẢI #1/#2** và **thang lên-trái**, khoá phòng Mummy cx 1280 (routine 05/09/11).
- (CVM2 06) Cơ chế `Map2Mechanisms.SpawnFromMarkers` chỉ gắn probe/luồng Block 2 — cầu hỏng Block 3 làm mới (05).
- (CVM2 11) Builder `Build Map2 Screens` không idempotent byte → routine 11.
- (00 CVM3) Prompt routine 00 ghi baseline Arena `full` ≈ 26 phút; đo thực 28 phút (máy bận) — không ảnh hưởng.
- (00 CVM3) `RegressionRunner.EXPECTED` chưa có `map3`/spam8..10 — thêm ở routine 11.

## §7 Nhật ký nghiệm thu

### Routine 00 — 21/10/2026 — Hạ tầng: sổ, commit đồ planner, kiểm mẫu, baseline
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `__pycache__`, `UserSettings/`, submodule ` m`); `.gitignore` đã có dòng `ClaudeNotes/cvm3/_cache/`; cổng CVM2 12 = XONG (commit `d1bf216`). MCP chọn `Castlevania_Original-main@5669a9bbfcade130` (3 instance mở: 2 `hero_def_2021` khác), `dataPath` = `D:/Unity/Castlevania_Original-main/Assets`.
- **Kiểm mẫu số planner**: `calib_map3.py` ⇒ §2a (lệch sx 0.001 / ox 0.14 px NES, ZNCC 0.9786). 8 mốc `MAP3_TIMELINE §3` trích lại bằng ffmpeg và ĐÃ MỞ XEM (`ClaudeNotes/cvm3/_cache/shots/00/sheet.png`, `m1b.png` cho 7.6 s) — §2b; chỉ khác: 0:06.8 còn đen (fade-in), STAGE 07 TIME 0500 hiện ở 7.6 s.
- **Baseline đo (không đổi code)**: Arena-Simon `full` @60: 14/14 KHỚP (1672 s). Arena-Dummy `core`: `76516076` 21/21 (92 s). Main `main,flow30,flow144`: 14 bộ KHỚP (1383 s) — flow/flow30/flow144/map2 `caf21ae2`, trước STAGE 04 `165b9aef`, H8 5/5 ×3 (352/432/448/890/922), spam1–7 0 lỗi. Cả ba `ALL.done` = `OK` (bỏ qua 0). Kết quả: `_cache/00/{arena,core,main}/suites/SUMMARY.txt`.
- **Dọn**: thoát Play, `ArenaUseDummy` = false, EnterPlayModeOptions tắt, `EditorSettings.asset` = HEAD (CRLF; `git status` không hiện), scene mở = Main không dirty.
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 00**: [x] đồ planner commit (`_cache/` không vào git) · [x] sổ đủ §1–§7, bảng hash 21 dòng có số đo hôm nay · [x] mọi hash = bảng CVM2 12; H8 5/5 ×3; console không lỗi mới (spam 0 lỗi).
