# ACCEPT12 — Nghiệm thu Map 3 (Block 3 = Stage 07–09) so với video + hồi quy Map 1 + Map 2 (routine cvm3-12, 22/10/2026)

> Chi tiết của `CVM3_MEMORY §7 "Routine 12"`. Chỉ ĐO + phán xét; **KHÔNG đổi gameplay/Config/scene/prefab — không hash nào đổi** (sửa duy nhất: thêm
> tham số `Map2AcceptProbe.MinStage` cho vết thụ động — Editor, mặc định 4 = như cũ). Công cụ thêm (chỉ Editor / python):
> `Assets/Data/Script/Debug/Map3AcceptProbe.cs` — (1) **11 kịch bản trên MAP THẬT Block 3** (StageScreen thật, NesClock tắt — tự Step; không bất tử trừ
> kịch bản ghi rõ): hố + mép rơi, cầu hỏng St9, knockback hất xuống hố, 3 vạch/đòn ⇒ chết đòn 6, đồng hồ ở St8, quỹ đạo lọ nước thánh phòng Mummy,
> GAME OVER ⇒ CONTINUE, bí mật túi St7/St8 (+ `ShotDir` chụp ảnh đúng tick ở mốc); (2) **sổ điểm video Map_3** (áp từng sự kiện HUD qua đúng code game,
> op `known` cho luật chưa dựng). Vết thụ động phần Map 3 của luồng thật = `Map2AcceptProbe.MinStage = 7` + `StartTelemetry`.
> Python: `Tools/cvm3/accept12.py` (objects · enemy · boss · hud · music · trans · terrain), `Tools/cvm3/accept12_ledger.py` (sổ điểm từ
> `ref/data/map3_events.txt` + `drops3.tsv` + `map3_hud.csv`), `Tools/cvm3/accept12_montage.py` (ghép ảnh Game view).
> Số liệu gọn (commit): **`ClaudeNotes/cvm3/ref/data/accept12/`**. Thô + ảnh (không commit): `ClaudeNotes/cvm3/_cache/12/` (vết `flow60/telemetry.tsv`
> 25 580 dòng, `main/ arena/ core/` bộ hồi quy, `enemy_unity/ boss_unity/ cmp/`, md5) + `_cache/shots/12/` (`flow60/` H8, `route/` 38 ảnh chuyển cảnh +
> `route_pairs/`, `terrain1/` 28, `terrain2/` 25, `terrain3/` 26 + `terrain3_pairs/`, `objects/` 46 + `pairs_0..11`, `scenarios/` 16 + montage).
> **"Luồng thật"** = `Map1AcceptProbe.StartFlow(60)`: Title → Intro → Map 1 → Map 2 → bản đồ B2 → **STAGE 07 → 08 → 09 → Mummy Men → tally → bản đồ B3
> → STAGE 10 stub**, thời gian thực 60 fps, phát `map1_route` + `map2_route` + `map3_route` (autopilot CVM3 11 — bất tử theo đoạn: chỉ WEA chịu đòn thật).
> Mặt 3–9 đo trên luồng này + kịch bản map thật; Arena chỉ để chạy lại probe so video của 07/08/09/10 và bộ hồi quy.

## 0. Bảng tổng + kết luận

| # | Mặt | Kết quả | Tóm tắt bằng chứng |
|---|---|---|---|
| 1 | Địa hình & pixel | **ĐẠT** | 26 ảnh nền chụp từ **StageScreen thật** (5 section × 4–6 cx, gồm WalkwayB cx 0 = video −112, MummyMan cx 1280) ↔ sheet Level 3 đã nới: **0 / 45 056 px lệch mỗi ảnh** (NES y 48–223); 24 cặp video cùng cx: lệch > 48 chỉ **1.84–3.93 %** (= sprite/nến/khối vỡ — ảnh đã xem) |
| 2 | Vật thể | **ĐẠT** | 37/37 nến trùng 3 nguồn (MAP3_PLACEMENT ↔ Map3Objects.json ↔ ConfigDrop); **luồng thật**: 37/37 nến hiện đúng (section, x, Y), 2 khối, 3 bí mật, **26/26 SpawnPoint sinh đúng chỗ**, Medusa Head ×11 + Ghost từ bộ sinh; probe phòng thật **98/98**; **94/94 cặp ảnh video ↔ Unity lệch 0–1 px**; bí mật St7 (túi hiện +4, nghỉ +60 tại (424,129), **biến mất 188 tick sau khi nghỉ**), St8 (nhặt +1000; **ngoài màn 226 tick vẫn còn**). Còn: bí mật St9 **chỉ wiki** (§5 #14) |
| 3 | Simon trên địa hình mới + 2 sửa gộp | **ĐẠT** | **11/11 kịch bản map thật**: mép rơi x **102** (WEB) / **774** (cầu hỏng) = dò sàn X±6; Skeleton / Raven trúng đòn ⇒ **knockback hất qua khe cầu hỏng St9 / khe WalkwayB ⇒ chết hố ⇒ hồi sinh đầu stage**; Medusa Head **6 đòn 3,3,3,3,3,1 ⇒ chết ở đòn 6**; **đồng hồ St8: Medusa Head đứng đúng 186 tick, tim −5, TIME bỏ 3 mốc trong lúc băng** (= mô hình CVM3 03); **lọ nước thánh phòng Mummy 49 điểm lệch mô hình 0 px, vỡ n49, lửa X+73** (= video Map_3); GAME OVER ⇒ CONTINUE đầu STAGE 09 |
| 4 | Enemy Block 3 | **CHƯA ĐẠT** (2 lệch) | trên **map thật**: Raven đậu **60** tick (12/12), lướt **0.750**, treo **25**/14; Skeleton chặng **1.438** × 16, nghỉ 4–5, nhảy ở mép vx **1.25** g **0.125**, xương sinh Y − 13, **vx 0.750**, đỉnh **61 / 92**; Fleaman chờ → 127/96/64 tick (∈ 62–136), ngồi thấp **13**, cú ngắn **2.25** g 0.125; Medusa Head **1.250**, đỉnh–đáy 67, sinh **chân − 49** mép trước, nhịp **128**; Ghost **+96 sau lưng, chân − 31**, 0.750; cầu lửa **2.000**, cặp cách **40**, **chân − 15**; probe so video chạy lại: **mọi rms y hệt 07/08** (Raven 0.71–2.63, Skeleton 1.36–2.23, Fleaman 1.44–2.11, Ghost 0.97, Medusa Head 1.64–2.33, cầu lửa 1.00–1.51 px). **LỆCH**: (a) **Bone Tower phát đầu 41 tick sau khi HIỆN (5/5 tháp luồng thật) — video 48–51** (sớm 7–10 tick; §6 08/10); (b) **enemy rơi vật**: video Map_3 vật rơi tại chỗ enemy 4 lần (Raven, Skeleton, Bone Tower ⇒ tim lớn, Fleaman ⇒ tim lớn) + 2 cầu lửa — Unity không có luật (§5 #11) |
| 5 | Mummy Men | **CHƯA ĐẠT** (1 lệch) | luồng thật: 2 Mummy đứng chờ (1312 / 1503) từ lúc vào MummyMan, khoá cx 1280 khi Simon **x 1408** (video 1409) → **thức + 120** cả hai (video 119.5–120.9) → Poison Mind **+60**; đi **0.750** (video 0.741–0.767), ném đứng **47–48** (46–51), băng **chân − 31**, **±1.5**, sóng 24·sin(2πt/102) rms **0.00**; ENEMY 16 lần −1; **+3000 mỗi con**; quả cầu **+117** tại (1408,104), đứng rồi rơi đầu **+38** (video 35–37); Map3BossProbe chạy lại **10 vết rms trung vị 2.69 / max 8.69 px — y hệt 09**. **LỆCH**: trận **nước thánh từ khối trái** (9 lọ đúng mốc video Map_3, phòng thật b24 `mummy_room_holy`): Unity hạ 2 con ở **rel 982 / 1144**, video **1510 / 1683** — trận ngắn ~35 % (Mummy Unity đứng trong lửa nhiều hơn; luật hướng/cự ly suy luận §5 #13) |
| 6 | HUD & luật | **CHƯA ĐẠT** (1 lệch nhỏ) | **sổ điểm video Map_3 100/100 sự kiện khớp HUD** qua code game (SCORE, tim, P, vũ khí, hệ số II) ⇒ tally **073630 = video** CHỈ khi cộng tay 3 dòng luật chưa dựng (**Skeleton chết trong lửa nước thánh ⇒ chữ "1000"**: +800 f3081, +1000 f5223; +100 f5221 chưa rõ nguồn) — **Unity thuần ra 071730 (thiếu 1 900)**; luồng thật: TIME **0500** St07, **không đặt lại** qua 08/09 (469→469, 437→437), nhịp **64** (93 lần), pha TIME **60 / 82 / 58 / 82 / 58** = video, tally = điểm + TIME×10 + tim×100 (**26230** khớp), P-04 không đổi (0 1UP = video); **3 vạch/đòn** |
| 7 | Nhạc | **ĐẠT** (mốc) — còn tụt âm điểm lặp | luồng thật: **Wicked Child 1 lần ở khung đầu STAGE 07 (+0 tick = video)**, không phát lại qua 2 thang + 2 cửa; lặp lùi **45.27 s** (= vòng video) ở 68.2 s; Poison Mind **khoá + 60**; **Stage Clear cùng tick nhặt cầu**; tally + bản đồ B3 im. **Vẫn tụt âm mỗi vòng** (điểm lặp ở đầu đoạn fade gain 0.49 — §5 #1, cần bản rip 2 vòng); Walking on the Edge ở STAGE 10 stub = suy luận. **Cần người dùng nghe thử** |
| 8 | Chuyển cảnh | **ĐẠT** | **18/18 mốc ±2 tick** (bản đồ B2 → St07 đen 27 (26); thang ↖ ×2 đen 23 (22/24), hiện (97,189) cx 0 / (209,189) cx 81 (video (96,189) cx 0 / (207,191) cx 79); cửa PHẢI ×2: cuộn 128, door_1 +6 / +7, cột +107 / +110, STAGE 08/09 = cuộn + 414 / 417 (414.5 / 416.6); khoá → thức 120; Mummy cuối → cầu 117; nhặt → TIME 226 (226.4), 1 tick/đv, → tim 54 (54.1), nhịp tim 8.0 (7.97); tally → bản đồ B3 đen 19 (20.4)); **36 cặp ảnh** (9 tấm, đã xem 3). Bản đồ B3 sau dx 0 + STAGE 10 = suy luận (§5 #16) |
| 9 | Luồng, tất định, độ bền | **ĐẠT** | luồng thật Title → STAGE 10 **`7068416c` ở 60 / 30 / 144 fps** + `map3` + lần chạy có vết thụ động; trước STAGE 04 **`59cafbaa`**, trước STAGE 07 **PRE7 `8fcc6b6e`** mọi lần; H8 **352/432/448/890/922** ×3 + 12 ảnh H8 **0 px** so baseline 00; GameObject 595 → 618 rồi **ổn định** qua flow30/flow144; **spam8/9/10 (St07/08/09) 0 lỗi console**; GAME OVER ⇒ CONTINUE + hồi sinh đầu stage cả 3 stage Block 3 |
| 10 | Hồi quy Map 1 + 2 (H11) | **ĐẠT — 0 hồi quy** | Arena `full` **14/14 KHỚP**, core `76516076` 21/21, Main **18 bộ lệch 0**; địa hình Map 1 **28/28** + Map 2 **25/25 ảnh 0 px**; dữ liệu Level Block 1/2 **0 file đổi từ commit CVM2 12**; `Map2AcceptProbe.Run` **6/6**; sổ điểm Map_2 **86/86 ⇒ 44670**. Mặt CHƯA ĐẠT của ACCEPT12 CVM2 đo lại: đồng hồ **186** (= video 186–196 — đã sửa CVM3 03), knockback **34 px** (Hall A lùi 33 rồi kẹt tường; Hall B hất qua khe — đã sửa CVM3 04), **rơi vật vẫn chưa** (§5 #11) |
| 11 | Chất lượng | **ĐẠT** | cảnh đông MummyMan (2 Bone Tower + Raven + Skeleton + Medusa Head + nước thánh mỗi 10 tick): 11 thực thể, **Step TB 0.0455 / max 0.415 ms, 0 B GC**, GameObject 216 → 217 → **217** (vòng 2 không rò); luồng thật Step TB 0.035 ms, 0 B; console 0 lỗi gameplay; repo sạch; H3 submodule `4685d33`; SOURCES **12/12 sheet + 16/16 MP3** sha256; builder python ×9 "same" + 9 menu "ghi 0 / không đổi" (gồm **`CVM3/Build Map3 Screens` + Main.unity không đổi**) ⇒ **md5 1 523 file y hệt**; sprite 0 lỗi; sổ ↔ code 82/83 đường dẫn (1 = mẫu tên) |

**KẾT LUẬN CHUNG: CHƯA ĐẠT.** Map 3 **chơi được trọn** Title → Map 1 → Map 2 → STAGE 07–09 → Mummy Men → STAGE 10 stub (tất định 30/60/144 fps, 0 hồi quy
Map 1 + Map 2) và khớp video ở địa hình (0 px), vật thể (37/37 nến, 26/26 điểm sinh, 94 cặp ảnh ≤ 1 px), Simon trên cầu/hố với 2 sửa gộp (đồng hồ 186,
knockback 34 px — 11/11 kịch bản), quỹ đạo từng enemy trên map thật, mọi chuyển cảnh (18/18 ±2 tick) và mốc nhạc. Nhưng **3 mặt cốt lõi còn lệch video**:
(4) **Bone Tower bắn phát đầu sớm 7–10 tick** (5/5 tháp) và **enemy chưa rơi vật** (video Map_3 4 lần + 2 cầu lửa); (5) **trận Mummy bằng nước thánh kết thúc
sớm ~35 %** so với video (cùng 9 lọ đúng mốc); (6) **luật điểm "1000" khi Skeleton chết trong lửa nước thánh** chưa dựng (Unity thuần thiếu 1 900 điểm so
video). Cần người dùng: **duyệt routine sửa (§12)**, trả lời **§5 #11 (rơi vật), #12 (chữ "1000"), #13 (Mummy), #1 (Wicked Child)**, **nghe thử nhạc
Block 3**, **bấm thử gamepad**, sheet HUD bản US (chữ số TẠM — §5 #3).

## 1. Địa hình & pixel
- **Phương pháp**: Main.unity Play, `GameRules.NewGame()` (stage 7) → `GoTo(Stage1,0)` → `Map1AcceptProbe.StartTerrain(dir, "WalkwayEntranceA:0,169,277,405,512;
  WalkwayEntranceB:0,52,296,421,512;WalkwayA:0,128,256,512;WalkwayB:0,49,117,297,551,624;MummyMan:0,228,636,993,1263,1280")` — section của **StageScreen thật**,
  ẩn sprite động, HUD còn ⇒ so NES y 48–223 với vùng sheet Level 3 đã nới (`python -P -E Tools/cvm3/accept12.py terrain <dir> <dir_cặp>` — `build_tileset3.padded_regions`).
  Cặp video: `accept3.cmd_pairs` (24 frame camera đứng, đổi màu sheet → video LUT `fg3`).
- **Số liệu**: 26/26 ảnh **0 px** (`ref/data/accept12/terrain3.txt`); cặp video TB |hiệu| 3.1–6.3, lệch > 48 **1.84–3.93 %** (`terrain3_pairs.txt`) — trùng CVM3 05.
- **Ảnh đã xem**: `_cache/shots/12/terrain3_pairs/pairs_3.png` (WalkwayB cx 0/49/117/297 — đỏ chỉ ở Simon, chữ "1000", nến, tháp; bản lưu `ref/nes/cvm3_12_terrain_walkwayb_video.png`).

## 2. Vật thể
- **Ba nguồn** (`accept12.py objects`): 37/37 nến trùng (id, x, Y, nội dung) MAP3_PLACEMENT ↔ Map3Objects.json ↔ ConfigDrop; ConfigDrop Block 3 = 39 dòng (37 nến + 2 khối Pot Roast).
- **Luồng thật** (vết A/E): 37/37 nến hiện đúng (section, x, Y); khối `WalkwayEntranceA_X18_8` (296,144), `MummyMan_X83_9` (1336,160); 3 SecretTrigger (WEB (536,176), WalkwayB (256,128),
  MummyMan (1320,128)); **26/26 SpawnPoint** (Skeleton 6, Fleaman 3, Raven 12, Bone Tower 5) sinh đúng (x, Y ±1); bộ sinh: Medusa Head 11 (WalkwayA 9, MummyMan 2), Ghost 1.
- **Phòng thật / ảnh** (Arena-Simon `Map3ObjectsProbe.Start("candles,blocks,secrets,spawns,zones,boss", …, spec 44 ảnh từ `_cache/10/shots.csv`)` + `pairs10.py compare`):
  **98/98**; 46 ảnh ⇒ **94/94 cặp lệch 0–1 px** (|dx| ≤ 1, |dy| 0 — `objects_pairs.txt`). Đã xem `_cache/shots/12/objects/pairs_5.png` (WalkwayB: nến, Raven, 2 Bone Tower, túi St8 — bản lưu `ref/nes/cvm3_12_objects_walkwayb.png`).
- **Bí mật trên map thật** (kịch bản, bất tử để Raven #3 đậu sát chỗ ngồi không cắt tư thế): St7 ngồi ở (537,176) ⇒ túi hiện **+4** tick tại (424,143) trồi 14 px ⇒ nghỉ **+60** tại
  Y 129 (video: hàng đầu +6, nghỉ ≈ +58 — khung lặp ±2) ⇒ **mất 188 tick sau khi nghỉ** nếu không nhặt; St8 ngồi ở (246,128) ⇒ túi (128,129) ⇒ đi trái nhặt **+1000** (video 1:55.9);
  St8 đi phải ⇒ túi **ngoài màn 226 tick vẫn còn** (keepOffscreen) rồi hết hạn 188. Ảnh `scenarios/secret_*` (đã xem trong montage). St9: chỉ wiki (§5 #14).

## 3. Simon trên địa hình mới + 2 sửa gộp (kịch bản map thật — `Map3AcceptProbe.Run(ScenarioNames(), …)`, `ref/data/accept12/scenarios.txt`)
| Kịch bản | Thiết lập | Kết quả | |
|---|---|---|---|
| `pit_web` | WEB (80,160) đi phải tới khe [96,160) | bắt đầu rơi ở **x 102** (mép 95 + 6 + 1 — dò sàn X±6, video 9/9 mẫu) ⇒ Fall ⇒ **WEA (720,160) quay trái cx 512, ST7, P−1, TIME 500** | ĐẠT |
| `pit_bridge_st9` | cầu hỏng St9 (760,96) đi phải | rơi ở **x 774** ⇒ **MummyMan (48,96) quay phải cx 0, ST9** | ĐẠT |
| `knock_bridge_st9` | đứng giữa bục [800,832) x 816 quay phải | Skeleton S5 (sinh ngay chỗ) đòn **−3** tick 1 ⇒ lùi trái tiếp tục khi rơi (34 px cung + rơi) ⇒ x 766 trong khe [768,800) ⇒ **chết hố ⇒ đầu St9** | ĐẠT |
| `knock_pit_wb` | WalkwayB (680,128) quay phải, bờ phải khe [624,656) | **Raven #6 từ SAU LƯNG** đòn −3 tick 62 ⇒ lùi trái (−hướng mặt, §5 #9) ⇒ x 634 ⇒ rơi ⇒ **đầu St8 WalkwayA (48,128)** | ĐẠT (hướng: suy luận) |
| `damage_wa` | WalkwayA đi tới x 400 rồi đứng | Medusa Head trước mặt mỗi **128** tick: HP mất **3,3,3,3,3,1** ⇒ **chết đòn 6** (Damage) ⇒ hồi sinh WalkwayA; lùi **33 px/đòn** (đo tới hết Hurt, như CVM3 06) | ĐẠT |
| `stopwatch_wa` | WalkwayA, đồng hồ, tim 20, bấm ↑+B khi có Medusa Head (bất tử — chỉ đo băng) | tim 20 → 15; Medusa Head **đứng yên tick bấm+1 … bấm+186 = 186 tick**; TIME đổi ở 59,123,187,251,315 → **bỏ 379/443/507** → 571 (= §2d (d): mốc rơi vào lúc băng bị bỏ) | ĐẠT |
| `holy_mm` | phòng Mummy, khối trái (1302,112) quay phải (= video Map_3 3:22), ném 1 lọ | sinh **(X, Y chân − 19)**, **49 điểm lệch mô hình video 0 px**, lửa hiện **n49 tại (1375,176) = X + 73** (video: vỡ n49, lửa ở X lọ n49) | ĐẠT |
| `gameover_st9` | P 1 + rơi cầu hỏng | GAME OVER ⇒ Start ⇒ **MummyMan (48,96) f1 cx 0 ST9 P3 SC0 T500** | ĐẠT |
| `secret_st7`, `secret_st8`, `secret_st8_off` | xem §2 | ĐẠT ×3 | ĐẠT |
- **Lần chạy đầu 10/11**: `stopwatch_wa` báo "đứng 353→539 ≠ 352→538" — **lỗi BỘ CHẤM** (input tính ở tuổi a được xử lý trong bước tạo tuổi a+1; `StageWorld.FreezeEnemies`
  băng từ TickCount+1 như thiết kế CVM3 03 "băng 5–190 khi bấm tick 4"); sửa kỳ vọng thành tick bấm = tuổi + 1 ⇒ **11/11** (không đổi code game).
- Video Map_3 **0 lần Simon trúng đòn** ⇒ knockback trên Block 3 so theo mô hình CVM3 04 (6 lần trúng ở Boss_Map_1/2/3); hố/cầu hỏng/đồng hồ/lọ đều có mẫu video.
- **Ảnh đã xem** (`_cache/shots/12/scenarios_montage.png` — 16 ảnh đúng tick: đòn đầu + 12, bắt đầu rơi, đồng hồ + 90, lọ + 24, lửa, túi nghỉ, túi ngoài màn; bản lưu
  `ref/nes/cvm3_12_kich_ban_map_that.png`): Simon đang bị hất qua khe cầu hỏng/WalkwayB, Medusa Head đứng yên, lọ + lửa phòng Mummy, túi St7/St8 đúng chỗ.

## 4. Enemy Block 3 trên MAP THẬT (`accept12.py enemy` ⇒ `accept12_enemy.txt`; probe so video `Map3EnemyProbe` chạy lại ⇒ `_cache/12/cmp/`)
| Loại | Video (CVM3 07 §2g / 08 §2h) | Luồng thật Unity | |
|---|---|---|---|
| Raven (12) | đậu 58–62 sau khi hiện, lướt 0.716–0.792, treo 24–26, lao ngang, phanh → treo 14 | **12/12 đậu 60**, lướt **0.750**, treo **25**, quay treo **14**, chuỗi Pe60 Gl Ho25 (Gl Ho25) Da Br Ho14 Da | ĐẠT |
| Skeleton (6) + xương (7) | chặng 22–23 px / 1.43–1.44, nghỉ 4.6–5, ném đứng 18–23, xương vx 0.725–0.778 đỉnh 61.5–62.6 / 90.8–93.0, nhảy chỉ ở mép g 0.125 vx 1.17–1.35 | chặng 16 tick **1.438** (21.6 px), nghỉ 4–5, ném 18 (2 lượt liền 36), xương **Y − 13, 0.750, g 0.125, đỉnh 61 / 92**, nhảy **vx 1.25 g 0.125** đỉnh 24 | ĐẠT |
| Fleaman (3) | đếm 62 / 62 / 136 sau khi vào tầm, ngồi thấp 12–14, cú ngắn 2.25 / −1 / 0.125 | rời Wait sau **127 / 96 / 64** (∈ [62,136] — Roll), Ground **13**, HopS **2.25 g 0.125** | ĐẠT |
| Medusa Head (11) | 1.217–1.272, A 33.75 P 100, đỉnh chân − 48…−49 mép trước, nhịp 128 (8/12), vùng WalkwayA vào ở Simon 125–170, MummyMan 959–998 | **1.250**, đỉnh–đáy 67, rms sóng 0.50, **chân − 49**, x màn 248 (trước mặt), **128 ×9**; sinh đầu khi Simon x **165** (WalkwayA) / **1019** (MummyMan — video 1018) | ĐẠT (4/12 khoảng video lệch 128 chưa có luật — §6) |
| Ghost (1) | hiện 96 sau lưng, chân − 31, 0.76 | **+96, chân − 31, 0.750** | ĐẠT |
| Bone Tower (5) + cầu lửa | cầu 1.85–2.32, chân − 15, cặp 39–40; **phát đầu 48–51 tick sau khi tháp HIỆN** | cầu **2.000** ×12, **−15**, dx 0, cặp **40**; **phát đầu 41 tick sau khi sinh (= hiện ở cx + 248)** 5/5; lượt 2 ở +234/235 (video tháp đang bị quất 124.6 / ~178) | **LỆCH** (sớm 7–10 tick) |
| Rơi vật | Map_3: Raven f1732 ⇒ tim nhỏ, Skeleton f3073 ⇒ tim nhỏ, Bone Tower f5077 ⇒ tim lớn, Fleaman f546 ⇒ tim lớn (CVM3 10) + 2 cầu lửa ⇒ tim nhỏ; 3 video 14/79 lần hạ | không có luật (CVM3 08: không luật tất định 100 %) — 6 lần hạ trong luồng, 0 vật | **LỆCH** (chờ §5 #11) |
- **Probe so video chạy lại** (`Map3EnemyProbe.Run(SegNames())` 17 đoạn ⇒ `enemy7_compare.py` / `enemy8b_compare.py`): **mọi dòng rms y hệt** `ref/data/enemy7|enemy8b/compare.txt`
  (Skeleton 1.36–2.23, xương 1.04–2.65 (+ 3 vết lệch thời điểm như 07), Fleaman 1.44–2.11, Raven 0.71–2.63, Ghost 0.97, Medusa Head 1.64–2.33, cầu lửa 1.00–1.51 px);
  chỉ khác nhãn tick đổi state ±1 trong dòng mô tả (nhãn DebugState đã sửa ở 07/08 sau khi ghi file ref — vị trí từng tick y hệt) ⇒ file ref KHÔNG ghi lại.

## 5. Mummy Men (`accept12.py boss` ⇒ `accept12_boss.txt`; `Map3BossProbe` chạy lại; b24 `mummy_room_holy`)
- Luồng thật: Mummy (1312,176) + (1503,176) đứng chờ từ khi vào MummyMan (1 359 tick trước khoá — video thấy trước khoá) → khoá camera **cx 1280, Simon x 1408** → **cả hai thức + 120**
  → Poison Mind **+ 60** → đi **0.7500** px/tick (468 / 474 tick đi thật) → Throw **47–48** tick (+ 10 / 31 bị cắt do chết) → 2 băng **chân − 31, vx +1.500, rms sóng 0.00** →
  ENEMY **16 lần −1** → **+3000, +3000** → quả cầu **+117** tại **(1408,104)** → rơi đầu **+38** → Simon (autopilot nhảy) nhặt khi cầu đang rơi.
- `Map3BossProbe.Run("m3_open,b3_open,m3_death")` ⇒ `boss9m_compare.py`: **10 vết rms trung vị 2.69, max 8.69 px — trùng từng dòng `ref/data/boss9m/compare.txt`** (CVM3 09).
- **LỆCH — trận nước thánh**: b24 `mummy_room_holy` (phòng thật, Simon trên khối trái ném 9 lọ ĐÚNG mốc video Map_3): **15 đòn lửa, 2 con hạ ở rel 982 / 1144 — video 1510 / 1683**
  (§6 09 để routine 12 so: nay xác nhận trên luồng/phòng thật). Hành vi từng thành phần khớp video; khác ở chỗ Mummy Unity giữ cự ly 46–56 nên đứng trong vùng lửa (x 1368–1382)
  nhiều hơn ⇒ ăn nhiều đòn lửa hơn. Nguyên nhân khả dĩ: luật hướng/cự ly + tỉ lệ ném suy luận từ ít mẫu (§5 #13) ⇒ `cvm3-fix-02`.
- Ảnh đã xem: `route_pairs/pairs_7.png` (STAGE 09 vào, khoá cx 1280 với 2 Mummy chờ, thức, quả cầu; bản lưu `ref/nes/cvm3_12_pairs_phong_mummy.png`).

## 6. HUD & luật
- **Sổ điểm video Map_3** (`Tools/cvm3/accept12_ledger.py` ⇒ `ref/data/accept12/ledger.tsv` 103 dòng; `Map3AcceptProbe.Ledger` ⇒ `ledger_result.txt`): trạng thái đầu Map_3
  (SCORE 044670, P-04, tim 05, đồng hồ, roi 3, STAGE 07 TIME 0500) ⇒ áp từng sự kiện (kill = điểm ConfigEnemy như `EnemyBase`; boss MummyMan ×2 = ConfigBoss mỗi con;
  shot = `ConfigScore.EnemyProjectilePoints` như cầu lửa / băng gạc vỡ; item = `ItemEffects.Apply`; use = `heartCost`; stage = `EnterStage`; tally = `StageClearTally` Block 3
  (`Map3FlowTiming` 226 / 54); nextblock = `StartNextBlock`) ⇒ **100/100 khớp HUD** (nước thánh nhặt f491, 11 Raven +200, 4 Bone Tower +400, 11 cầu lửa +100, 2 túi +1000,
  túi trắng +700, túi tím +400, II f6903 (hệ số 2), Mummy +3000 ×2, tally TIME 306 (+3060) + tim 46 (+4600) ⇒ **073630**, STAGE 10 tim 05), 0 1UP (video P-04 suốt).
  **3 dòng luật CHƯA DỰNG** (op `known`): f3081 +1100 (Skeleton +300 + **800**), f5221 **+100** (nguồn chưa rõ, lúc lửa nước thánh — có thể xương bị lửa phá; Unity xương
  không phá được), f5223 +1300 (Skeleton +300 + **1000**) ⇒ **Unity thuần 071730 ≠ 073630 (−1 900)** — §5 #12.
- Luồng thật (`accept12_hud.txt`): STAGE 07 TIME 0500 tim 5 ENEMY 16; **0 lần đặt lại** (St07→08 469→469, 08→09 437→437); khoảng giảm **64 ×93** (+ các khoảng dài = TIME đứng ở cửa/đen);
  pha TIME sau khi vào section: **60** (STAGE 07 hiện), **82 sau ĐẦU đen thang** ×2, **58** sau STAGE 08/09 hiện (video 60 / 82 / 58–57); tally 21610 + 402×10 + 6×100 = **26230** = Unity.
- Sát thương **3 vạch/đòn** (ConfigBlock 3): `damage_wa` 3,3,3,3,3,1 ⇒ chết đòn 6 ✓; luồng thật WEA trúng 1 đòn −3.
- Ảnh HUD: chữ số TẠM (bản Famicom — chờ sheet US, §5 #3); nhãn/thanh/ô vũ khí/icon II trùng vị trí trong cặp ảnh (`route_pairs/pairs_8.png` — video có icon II, lộ trình không nhặt).

## 7. Nhạc (`accept12_music.txt`, `_cache/12/flow60/music.txt` — MusicProbe lấy mẫu timeSamples)
| Mốc | Video Map_3 (§2c/2k) | Luồng thật Unity | |
|---|---|---|---|
| Bản đồ B2 → STAGE 07 | Wicked Child t0 6.862 s = khung đầu STAGE 07 | `play WickedChild` frame 26181 = **khung đầu STAGE 07 (+0)** | ✓ |
| 2 thang ↖ + 2 cửa phải + STAGE 08/09 | không phát lại | 1 lần play cho cả 07→09 | ✓ |
| Lặp | vòng 45.27 s | lặp ở mẫu 3 006 231 (68.2 s) → 1 010 077: **lùi 45.27 s** | ✓ (nhưng tụt âm — dưới) |
| Khoá phòng Mummy | Poison Mind ≈ khoá + 1.2 s (±0.3) | **khoá + 60** (Config CVM3 09; Boss_Map_3 + 0.83 s) | ✓ |
| Nhặt cầu | Stage Clear cùng lúc | **cùng tick** | ✓ |
| Tally / bản đồ B3 | im | `end StageClear` → `stop` ở bản đồ | ✓ |
| STAGE 10 | (video hết khi bản đồ còn trượt) | Walking on the Edge (ConfigStage[10] — suy luận) | ghi chú |
**Vẫn tụt âm**: điểm lặp nằm ở đầu đoạn fade của bản rip 1 vòng (gain 0.49) ⇒ mỗi ~45 s tụt nhỏ (Map 3 ~4 phút ⇒ ~5 lần) — §5 #1 (cần bản rip 2 vòng hoặc quyết định bù âm lượng).
Không tự đánh giá cảm nhận âm thanh được ⇒ **cần người dùng nghe thử**.

## 8. Chuyển cảnh (`accept12_trans.txt`, `route_pairs.txt`)
18/18 mốc ±2 tick (bảng §0 mục 8). `Map3RouteProbe.Replay(shotDir, 40)` (hash `7068416c`, PRE7 `8fcc6b6e`) ⇒ 38 ảnh ⇒ `pairs11m3.py` ⇒ **36 cặp / 9 tấm**. **Đã xem**:
`pairs_0.png` (bản đồ B2 ×3 + khung đầu STAGE 07 — vị trí trượt, dơi/chấm, Simon trùng), `pairs_7.png` (STAGE 09, khoá + 2 Mummy chờ, thức, quả cầu), `pairs_8.png` (nhặt cầu, tally,
bản đồ B3 trượt ×2). Khác biệt chỉ do lộ trình (SCORE/tim/P/vũ khí phụ, nến video đã phá, icon II) + bảng màu video. Bản lưu `ref/nes/cvm3_12_pairs_{bando_b2_st07,phong_mummy,tally_bando_b3}.png`.

## 9. Luồng, tất định, độ bền (`regress_main.txt`, `flow60_report.txt`)
- `RegressionRunner.Start("main,flow30,flow144", 60, …)` (Main, 2 036 s): flow / flow30 / flow144 / map3 = **`7068416c`**, map2 **`4ec56f7a`**, PRE4 **`59cafbaa`**, PRE7 **`8fcc6b6e`**,
  h8 5/5 ×3, spam1–10 0 lỗi console (spam8/9/10 = đầu STAGE 07/08/09, Step TB 0.035 ms, 0 B GC). Lần chạy luồng thật CÓ vết thụ động + MusicProbe (flow60) cũng `7068416c`, 0 lỗi console.
- GameObject: 595 sau luồng → 615 → **618** sau spam (pool) và **không tăng** qua flow30 / flow144.
- GAME OVER ⇒ CONTINUE: `gameover_st9` ⇒ đầu STAGE 09 P-03 điểm 0 TIME 0500; hồi sinh đầu stage: St07 (`pit_web`), St08 (`knock_pit_wb`, `damage_wa`), St09 (`pit_bridge_st9`, `knock_bridge_st9`).

## 10. Hồi quy Map 1 + Map 2 (`regress_arena.txt`, `regress_core.txt`, `terrain1_map1.txt`, `terrain2_map2.txt`, `map2_scenarios.txt`, `ledger_map2_result.txt`)
- Arena-Simon `full` @60 (1 727 s): long `b2cc1424` · simon `5c1c46ce` 20/20 · attack `62c14a1f` 21/21 · item `c8db5123` 14/14 · enemy `20aa5652` 20/20 · boss `4d7bd652` 12/12 ·
  b23 `810f0d8b` 37/37 · b46 `461c6b8b` 26/26 · b24 `4f503db5` 26/26 · b56 `9b9b7440` 14/14 · holy `95e3ba78` 5/5 · mech `d2b0212f` 15/15 · rules `72c7e7bf` 78/78 · gallery `5791addd` 75/75
  — **lệch 0**. Arena-Dummy core `76516076` 21/21.
- Các mặt ĐẠT của ACCEPT12 CVM2: địa hình Map 2 **25/25 ảnh 0 px** (`Tools/cvm2/accept12.py terrain2`), Map 1 **28/28** (`Tools/cvgp/accept16.py terrain`); vật thể: `git diff d1bf216..HEAD` trên
  `Assets/Data/{Config,Prefabs,Art}/Level/Block{1,2}` = **0 file**, ConfigDrop chỉ +39 dòng Block 3, `Build Map1/Map2 Objects` "ghi 0"; kịch bản map thật Map 2 **6/6**; **sổ điểm Map_2 86/86 ⇒ 44670**;
  luồng Map 1 + 2 = PRE4 + PRE7 không đổi.
- Các mặt CHƯA ĐẠT của ACCEPT12 CVM2 đo lại: **đồng hồ** 300 → **186** (CVM3 03 — video 186–196; `stopwatch_wa` Block 3 + rules/mech Block 1/2) ⇒ **đã khớp**; **knockback** 12 → **34 px**
  (CVM3 04): `damage_halla` lùi 33 rồi kẹt tường x 760, `knock_pit_hallb` lùi 41 ⇒ hất qua khe ⇒ đầu STAGE 05 ⇒ **đã khớp**; **rơi vật**: vẫn không dựng (§5 #11) ⇒ **vẫn lệch**.

## 11. Chất lượng
- Hiệu năng (Arena, Cvm3TerrainProbe + MummyMan (400,96), 2 Bone Tower + Raven + Skeleton + bộ sinh Medusa Head trước mặt + nước thánh mỗi 10 tick, 1 500 tick; mã dán `execute_code`
  — khuôn §3e): tối đa **11 thực thể** (4 đạn/vũ khí), **Step TB 0.0455 ms, max 0.415 ms** (vòng 2: 0.0453 / 0.333), **0 B GC**, pool 125, GameObject 216 → 217 (vòng 1, tạo 1 lần) → **217** (vòng 2) — `crowd.txt`.
- Console: 0 lỗi gameplay (luồng thật, spam1–10, 11 kịch bản, probe 07/08/09/10); probe mới biên dịch sạch (chỉ lỗi nền Odin / Unity.Mathematics / MCP WebSocket).
- Repo: `git status` sạch ngoài file mới của routine + nhiễu nền; submodule `4685d33` (` m` = DLL Odin tại chỗ — nền, H3); `SOURCES.md` **12/12** sheet + **16/16 MP3** sha256 khớp.
- Builder: python `build_tileset/build_collision` ×3 block "same", `gen_map1/2/3_objects.py` byte y hệt; menu `CVGP/Import Config` (830 dòng, ghi lại 0), `Build AnimSets` (28, ghi 0), `Build Map1 Tilemaps` /
  `Objects` (69), `CVM2/Build Map2 Tilemaps` / `Objects` (75), `CVM3/Build Map3 Tilemaps` / `Objects` (70), **`CVM3/Build Map3 Screens` (StageScreen / CastleMapScreen / Stage10Stub / Main.unity
  "không đổi")** ⇒ **md5 1 523 file (Config, Prefabs, Art/Level, Tests, Scenes, Anim, overlay ref) y hệt**; `verify_sprites.py --meta` 246 PNG **0 lỗi**.
- Sổ ↔ code: 82/83 đường dẫn trong `CVM3_MEMORY` có thật (1 = mẫu `l3t_NNN`); hằng/API chính (`PRE7`, `Map3FlowTiming`, `TALLY_WAIT_TICKS`, `ItemFootCenterFromBlock`, `extraAboveFeetPx`,
  `holyStunTicks`, `crystalHoverTicks`) có trong `.cs`; 0 TODO/FIXME.

## 12. Routine sửa đề xuất (KHÔNG tạo — người dùng duyệt; model · effort theo `skill-commond`)
| Routine | Model · effort | Việc | Bằng chứng |
|---|---|---|---|
| `cvm3-fix-01-bone-tower-sinh-ve-va-nhip` | Opus 5.5 · high | Bone Tower phát đầu 48–51 tick sau khi HIỆN (giả thuyết CVM3 08: NES sinh khi cột vào ~cx + 256, chỉ vẽ trong [cx+8, cx+248) ⇒ tách khung sinh/vẽ của SpawnPoint, hoặc trễ phát đầu tính từ lúc hiện) — giữ Raven đậu 60 tick từ lúc HIỆN (đúng); xét nhịp tháp khi bị quất (124.6 / ~178); dữ liệu dùng chung Block 2/6 (H13) ⇒ chạy lại probe + hash b23/gallery/flow có chứng minh | `accept12_enemy.txt` (5/5 tháp phát ở +41), CVM3 08 §2h (48/48/49/49/51), §6 08/10 |
| `cvm3-fix-02-mummy-tran-nuoc-thanh` | Opus 5.5 · xhigh | Trận Mummy bằng nước thánh: đo lại vị trí 2 Mummy so lửa từng lọ ở Map_3 f6074–6771 (9 lọ) ⇒ luật hướng / cự ly (46/56) / tỉ lệ ném / khựng lửa sao cho cùng 9 lọ hạ ở ≈ 1510 / 1683; chỉ Block 3 (b24/gallery) | b24 `mummy_room_holy` rel 982 / 1144 vs video 1510 / 1683; §5 #13, §6 09 |
| `cvm3-fix-03-luat-diem-va-roi-vat` (SAU KHI người dùng trả lời §5 #11, #12) | Opus 5.5 · high | (a) enemy rơi vật — nếu chọn (b): ngẫu nhiên có seed theo tỉ lệ đo 14/79 + tỉ lệ loại vật; (b) luật "1000" khi Skeleton chết trong lửa nước thánh (+ nguồn +100 f5221) ⇒ sổ điểm Map_3 Unity thuần ra 073630; đổi hash Block 1/2/3 có chủ đích (H14) | `ref/data/drops3.tsv`, `ref/data/accept12/ledger_result.txt` (3 dòng BIẾT) |
| `cvm3-fix-04-item-do-san-cot-tam-block12` | Opus 5.5 · high | Đưa luật item dò sàn cột tâm (`ItemFootCenterFromBlock` 3 → 1 — Map_1 CryptB đồng hồ rơi xuyên khe), túi bí mật tuổi thọ từ lúc nghỉ + quãng trồi 14 cho Block 1/2 — đổi Map 1 có chứng minh (H14) | §6 10 (`_cache/10/m1cb/cb_stopwatch.png`) |
| `cvm3-fix-05-wicked-child-lap` (chỉ khi có bản rip 2 vòng / người dùng chọn bù âm lượng) | Sonnet 5.5 · medium | Điểm lặp Wicked Child không tụt âm | §5 #1, `flow60/music.txt` |
Còn chờ từ trước (không thuộc cụm này): `cvgp-fix-04` (báo #3, Phantom Bat — Map 1).
Thứ tự khuyến nghị: fix-01 → fix-02 → (người dùng trả lời #11/#12) fix-03 → fix-04 → (nếu có) fix-05. Sau đó chạy lại routine nghiệm thu này (công cụ §13).

## 13. Chạy lại nghiệm thu (lần sau)
(1) Main.unity Play (`DisableDomainReload`) → AppActivate (giữ focus suốt — vòng `AppActivate` mỗi 3 s) → `runInBackground = true` → `GameSession.Current.ResetForNewGame();
GameRules.NewGame(); stage 7; GoTo(Stage1,0)` → `Map3AcceptProbe.Ledger("<_cache>/12/ledger.tsv", "<_cache>/12/ledger_result.txt")` (sổ từ `python -P -E Tools/cvm3/accept12_ledger.py`);
(2) `Map3AcceptProbe.ShotDir = "<shots>/12/scenarios"; Map3AcceptProbe.Run(Map3AcceptProbe.ScenarioNames(), "<_cache>/12/scenarios.txt")` (~1 phút);
(3) `GameSession.Current.ResetForNewGame(); Map2AcceptProbe.MinStage = 7; Map2AcceptProbe.StartTelemetry("<_cache>/12/flow60/telemetry.tsv"); MusicProbe.Start();
Map1AcceptProbe.StartFlow(60, "<shots>/12/flow60")` (~7.5 phút) → `StopTelemetry()`, `MusicProbe.Stop(…)`, `SaveReport(…)`, `MinStage = 4` → `python -P -E Tools/cvm3/accept12.py all <telemetry.tsv> <ra>`;
(4) stage 7 `GoTo(Stage1)` → `Map1AcceptProbe.StartTerrain` (spec §1 + Map 2 spec ACCEPT12 CVM2 §1 + Map 1 `CastleEntrance:0,63,246,384,512;CryptA:0,29,128,253,256;CryptB:0,88,128,256;
EntranceHall:0,108,256,512,711,768,1024,1239,1280;PhantomBat:0,118,256,330,512`) → `accept12.py terrain <dir> <dir_cặp>`, `Tools/cvm2/accept12.py terrain2`, `Tools/cvgp/accept16.py terrain`;
(5) `Map3RouteProbe.Replay("<shots>/12/route", 40)` → `Tools/cvm3/pairs11m3.py <route> <route_pairs>`; `Map2AcceptProbe.Run(Map2AcceptProbe.ScenarioNames(), …)`;
(6) phiên Main mới: `RegressionRunner.Start("main,flow30,flow144", 60, …)` (~34 phút); (7) Arena-Simon: `full` (~29 phút) → `Map3EnemyProbe.Run(SegNames(), <dir>)` → `enemy7_compare.py` /
`enemy8b_compare.py <dir> <ra>`; `Map3BossProbe.Run("m3_open,b3_open,m3_death", <dir>)` → `boss9m_compare.py <dir> <ra>`; `Map3ObjectsProbe.Start("candles,blocks,secrets,spawns,zones,boss",
<shots>, spec từ `_cache/10/shots.csv`)` → `pairs10.py compare <shots>`; crowd (§11); (8) Arena-Dummy `core`; (9) builder python + 9 menu + md5, `verify_sprites.py --meta`, SOURCES sha256;
(10) Main Play: sổ điểm Map_2 (`Tools/cvm2/accept12_ledger.py` → `Map2AcceptProbe.Ledger`).

## 14. Cụm tiếp theo đề xuất (KHÔNG tạo)
**CVM4 — Map 4 (Block 4, Stage 10–12, boss Frankenstein & Igor)** cùng khuôn CVM3: 00 hạ tầng + baseline · 01 nhập sheet Level 4 + kiểm kê · 02 hiệu chuẩn video + timeline · (gộp
`cvm3-fix-01..04` nếu người dùng duyệt mà chưa chạy) · 05 tilemap + va chạm Block 4 (Stage 10 tầng hầm nước — nước/hố) · 06 quét giữa cụm · 07/08 enemy Block 4 đo video (Fish Man / dơi
tầng nước, Eagle + Fleaman do Eagle thả — Fleaman rơi g 0.125 CVM3 07 chưa có video kiểm, Axe Knight, …) · 09 Frankenstein & Igor · 10 vật thể · 11 luồng `Stage10Stub` → STAGE 10 thật
→ STAGE 13 stub · 12 nghiệm thu.
**Điều kiện trước**: (1) người dùng **tải tay** sheet `NES - Castlevania - Stages - Level 4_ …` (spriters-resource chặn Cloudflare); (2) video **`Map_4.mp4`** (tiếp playthrough từ STAGE 10 —
nên bắt đầu từ cuối tally Block 3 để đo luôn **bản đồ B3 sau dx 0** §5 #16) + **`Boss_Map_4_Behaviour.mp4`**; (3) giờ giới hạn Block 4 (`ConfigBlock[4].timeLimit` trống — đo từ video);
(4) trả lời §5 #11 / #12 / #13 / #1 và quyết định routine sửa §12; (5) vẫn mở: §5 #3 (sheet HUD US, gamepad), #7 (ending).
