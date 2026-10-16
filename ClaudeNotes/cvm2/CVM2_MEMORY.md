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
| 01 | cvm2-01-nhap-sheet-level2-va-sprite-moi | chưa chạy | | | |
| 02 | cvm2-02-hieu-chuan-video-timeline-va-su-kien | chưa chạy | | | |
| 03 | cvm2-03-nhac-16-bai-va-moc-nhac-map1-map2 | chưa chạy | | | |
| 04 | cvm2-04-lo-nuoc-thanh-xuyen-dich | chưa chạy | | | |
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

*(Các routine sau thêm 2c… — số đo của mình.)*

## §3 Tài nguyên (đường dẫn sinh ra)

| Thứ | Đường dẫn | Ghi chú |
|---|---|---|
| Kế hoạch / chỉ mục video / wiki / kiểm kê (planner, **commit ở routine 00**) | `ClaudeNotes/cvm2/CVM2_PLAN.md`, `ref/MAP2_TIMELINE.md`, `ref/nes/*.png` (36 ảnh lưới NES), `ref/tools/*.py` (7 script đo), `wiki/MAP2_WIKI_DATA.md`, `inventory/level2_main_hall.json` + `level2_main_hall_annot.png` | `_cache/` KHÔNG commit (video ref 18 MB, CSV từng frame, ảnh wiki, tools chạy được `_cache/tools/`, bộ sinh SKILL `_cache/routines/`) |
| **Bộ chạy hồi quy** (routine 00) | `Assets/Data/Script/Debug/RegressionRunner.cs` (+ `.meta`; chỉ Editor) | gọi qua MCP `execute_code` khi Play — xem §4 "Bộ chạy hồi quy" |
| Harness sửa ở routine 00 (không đổi hash) | `RulesProbe.cs` (trả StageRules/AutoRespawn/sự kiện cuối bộ; bia `touchDamage = true`), `BossB24Probe.cs` (`DestroyImmediate`), `Map1AcceptProbe.cs` (`StartSpam` thêm tham số warp/mạng/máu) | `§7 Routine 00` |
| Kết quả baseline routine 00 | `ClaudeNotes/cvm2/_cache/00/{arena,arena60,core,main,order,alias}/suites/*.txt` + `SUMMARY.txt`, ảnh `_cache/shots/00/` | không commit |
| Công cụ video Map_2 | `ClaudeNotes/cvm2/_cache/tools/` (chạy từ `ClaudeNotes/cvm2/_cache/`; bản lưu `ref/tools/`), `nes2.py` (hệ số Map_2) | python `-P -E`; ffmpeg `C:\ffmpeg\bin\ffmpeg.exe` |

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

Mốc routine 11 (luồng Title → STAGE 07) sẽ thêm dòng hash mới; **ảnh H8** L10/L600 0 px so `ClaudeNotes/cvgp/_cache/shots/00/base_*` (đo lại 16/10: **0 px** ở cả 60/30/144 fps); L930 so `base_L930` KHÔNG so được (baseline chụp lúc Simon chưa chạy lộ trình) — so với ảnh routine 21 `cvgp/_cache/shots/21/flow60/h8_L930.png` ⇒ **0 px**.

**Lịch sử đổi hash:** *(chưa có — routine 04, 08, 09 chắc chắn đổi một số hash; ghi 1 dòng/hash: cũ → mới, lý do, routine)*

### Bộ chạy hồi quy (`RegressionRunner`, routine 00)
Chỉ Editor, đang Play. Chạy nối tiếp các probe qua `EditorApplication.update`, ghi `<dir>/suites/<bộ>.txt`, `SUMMARY.txt` (bộ | đạt/tổng | hash | hash chuẩn | khớp? | giây), `progress.txt`, `ALL.done` (`OK` / `OK (bỏ qua n)` / `LỆCH n` / `ABORT …`). Dùng `execute_code` (codedom — `Start` trả chuỗi, không cần chờ trong lệnh); chờ bằng vòng lặp `[ -f <dir>/suites/ALL.done ]`.

| Lệnh | Scene / điều kiện | Bộ chạy | Thời gian đo (16/10) |
|---|---|---|---|
| `RegressionRunner.Start("all", 60, "<abs dir>")` | Arena, Player = Simon (`CVGP.ArenaUseDummy` = false), Play, `runInBackground = true` | long, simon, attack, item, enemy, boss, b23, b46, b24, b56, rules | **1047 s ≈ 17.5 phút** (mọi fps cho cùng thời gian: ~60 tick/s) |
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
- **H8**: chạy kèm mọi `flow*` — đọc 5 mốc từ `Map1AcceptProbe.Events` (`Title: Start`, `Title→Black`, `Black→Intro`, `Intro→Black`, `Black→Stage1`) ⇒ `suites/h8_<flow>.txt`.
- Play Mode an toàn (Odin MemoryStream crash): `EditorSettings.enterPlayModeOptionsEnabled = true` + `DisableDomainReload`; routine 00 chạy 5 phiên Play + 4 lần biên dịch không crash.

## §5 Câu hỏi cho người dùng (chép từ `CVM2_PLAN §9` — đừng tự quyết thay)
1. **`cvgp-fix-03` (Simon knockback/nước thánh/mép rơi) + `cvgp-fix-04` (báo #3, Phantom Bat)** — chỉ Map 1, chưa tạo; người dùng duyệt thì tạo sau (routine 09/12 ghi thêm mẫu đo từ Map 2 nếu có).
2. (vẫn mở từ CVGP) sheet HUD bản US (chữ số TẠM) — `CVGP_MEMORY §5 #39a`; bấm thử gamepad Flydigi — `#39b`.
3. Bẫy ép gai: chạm = mất 2 vạch + knockback hay chết ngay? Video có thể không cho thấy (Simon né được) ⇒ routine 06 chọn theo bằng chứng có được, ghi `suy luận`, hỏi lại ở đây.
4. Cụm sau: **Map 3 (Block 3, Stage 7–9, boss Mummy Men)** cần sheet `NES - Castlevania - Stages - Level 3_ ...` (spriters-resource — tải tay) + video `Map_3.mp4` / `Boss_Map_3_Behaviour.mp4`. Sheet Tilesets đã có bảng tile Level 3–6 nhưng KHÔNG thay được sheet bản đồ.
5. Kết thúc game (credits/ending/Second Quest) — vẫn chờ (`CVGP_MEMORY §5 #60`).

## §6 Nợ & việc ngoài phạm vi
- (00) `ArenaTarget.OnSpawned` không đặt lại `touchDamage` (bia từ pool mang cờ của probe trước); đã vá phía `RulesProbe`. Probe mới dùng `ArenaTarget` phải tự đặt `touchDamage` sau khi `Spawn`.
- (00) `BossProbe.Cleanup` vẫn `Destroy(rules)` (không `DestroyImmediate`) — chưa gây lệch hash (kịch bản kế chạy khung sau); nếu thêm kịch bản gắn `StageRules` liền kề thì đổi như B24/B56.
- (00) Bộ chạy `core`/`flow` cần phiên Play riêng (đổi `CVGP.ArenaUseDummy` / scene) ⇒ tối thiểu 3 lệnh; chưa tự động hoá đổi Play (rủi ro mất focus MCP + Odin crash). Có thể làm ở routine 07 nếu cần.
- (00) `Map1AcceptProbe` chỉ hiểu stage 1–3 (`Clamp(stage,1,3)` trong `StartSpam`) — luồng Stage 04–06 (routine 11) cần probe Map 2 riêng.
- (00) HUD "STAGE 05" đổi ~1 s SAU khi Hall A hiện (xem §2b) — routine 02 xác nhận, routine 11 dựng.
- (00) Ảnh H8 L930 không có baseline đúng (xem §4) — dùng ảnh routine 21.

## §7 Nhật ký nghiệm thu

### Routine 00 — 16/10/2026 — Hạ tầng: sổ, commit đồ planner, bộ chạy hồi quy 1 lệnh, baseline
- **Bước 0**: `main`; nhiễu nền giữ nguyên (`.codex/`, `Assets/TuyenAframework.meta`, `.slnx`, `Tools/cvgp/__pycache__/`, `UserSettings/`, submodule ` m`); `.gitignore` đã có dòng `ClaudeNotes/cvm2/_cache/`; cổng CVGP 21 = XONG (commit `*add task "CVGP 21`). MCP `Castlevania_Original-main@5669a9bbfcade130` (dataPath đúng).
- **Kiểm mẫu số planner**: `calib_map2.py` ⇒ sx 3.620 / sy 3.100 / ox 269.12 / oy −19.6, ZNCC 0.9386 — lệch planner ≤ 0.005 / ≤ 0.2 px NES (§2a). Trích lại 8 mốc `MAP2_TIMELINE §3` bằng ffmpeg `-ss` (ảnh ghép `ClaudeNotes/cvm2/_cache/shots/00/milestones.png` — **đã mở xem**): 0:06.3 STAGE 04 TIME 0400 TEA, rìu + II ✓ · 0:28.9 TEB TIME 0380, Spear Knight tím + thang ✓ · 0:52.2 cửa đỏ mép trái TEB, Simon đứng cạnh ✓ · 0:57.8 Hall A (vòm, Simon giữa) ✓ nhưng **HUD còn "STAGE 04" TIME 0359** (đổi 05 ở ~58.75 s — `hud_switch.png`, §2b) · 2:28.4 STAGE 05 TIME 0281 sảnh Hall B/C (planner ghi 2:28.4 = STAGE 06 — thực tế khung này HUD "STAGE 05" cạnh cửa; STAGE 06 xuất hiện ở khung 2:38) · 2:38 STAGE 06 TIME 0273 Hall C, 2 bẫy ép gai treo trên trần ✓ · 3:48 STAGE 06 TIME 0211 sảnh Medusa, cửa sổ màu, tượng Medusa + Simon + Medusa Head ✓ · 4:16 STAGE 07 bản đồ lâu đài Block 2, tim 05, ENEMY rỗng ✓. ⇒ 7/8 đúng; mốc "2:28.4 STAGE 06" của planner lệch (HUD đổi 06 muộn hơn ~vài giây, giống quy luật "HUD đổi sau") — routine 02 đo lại.
- **Công cụ**: `RegressionRunner.cs` (~480 dòng) + 3 sửa harness (§4 gộp fix-05, (a)–(f)). Biên dịch sạch (console chỉ lỗi nền Odin/Unity.Mathematics/GUIClips).
- **Baseline đo (mọi hash = CVGP 21)**: Arena-Simon `full` @240 fps: long `5963c4ba` 1/1 · simon `b2954cbe` 20/20 · attack `eee2cdfb` 21/21 · item `7ca5bf56` 14/14 · enemy `f288b0ce` 20/20 · boss `31e6d711` 12/12 · b23 `171be0c1` 31/31 · b46 `58a8edf9` 26/26 · b24 `7adfbc54` 16/16 · b56 `6cb8a1ad` 14/14 · rules `03fafb24` 68/68 · gallery `517442ed` 75/75 — **1527 s, 0 lệch**; lặp `all` @60 fps: **y hệt** 1047 s; thứ tự `rules,long,rules,long` y hệt. `core` (ArenaDummy) `76516076` 21/21 (92 s). Main: `flow` `e0b2d54c` ở 60/30/144 fps (checkpoint 18 giá trị trùng routine 21, GameObject 379), **h8 5/5** (352/432/448/890/922) ×3, spam1..4 0 lỗi console (CB: GAME OVER 171 → CONTINUE 204 = routine 21). Ảnh H8 L10/L600 **0 px** so baseline ở cả 3 fps (`h8_montage.png` — đã xem: Title, Intro, Stage 01); L930 = ảnh routine 21 (0 px).
- **Lệch / xấp xỉ**: không có hash lệch. `base_L930` không so được (đã ghi). Số `all` 17.5 phút + core 1.5 + main 7.5 ≈ 27 phút (≤ 30). gallery 8 phút (routine 21 đo 3.5 phút — máy bận hơn).
- **Câu hỏi §5 mới**: không.
- **Nghiệm thu routine 00**: [x] đồ planner commit đủ (48 file; `_cache/` không vào git; kiểm mẫu planner ghi §2a/§7) · [x] `CVM2_MEMORY.md` đủ §1–§7, Bảng hash chuẩn 17 dòng · [x] bộ chạy chạy hết không thao tác giữa chừng trong từng phiên (3 phiên Play: Arena-Simon / Arena-Dummy / Main — ghi rõ), SUMMARY đã đọc · [x] mọi hash = CVGP 21, H8 khớp, console sạch (chỉ lỗi nền).
