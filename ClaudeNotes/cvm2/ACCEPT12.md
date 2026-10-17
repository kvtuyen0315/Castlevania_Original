# ACCEPT12 — Nghiệm thu Map 2 (Block 2 = Stage 04–06) so với video + hồi quy Map 1 (routine cvm2-12, 17/10/2026)

> Chi tiết của `CVM2_MEMORY §7 "Routine 12"`. Chỉ ĐO + phán xét; KHÔNG đổi gameplay/Config/scene/prefab (không hash nào đổi). Công cụ thêm (chỉ Editor / python):
> `Assets/Data/Script/Debug/Map2AcceptProbe.cs` — (1) **vết THỤ ĐỘNG** mọi tick phần Map 2 của luồng thật (W/E/A/S + sự kiện, không tiêm input — hash luồng vẫn `caf21ae2`),
> (2) **kịch bản KHÔNG bất tử trên map thật** (StageScreen thật: hố, gai, Game Over → Continue, sát thương, knockback xuống hố), (3) **sổ điểm video**
> (áp từng sự kiện HUD của video qua đúng đường code game). Python: `Tools/cvm2/accept12.py` (mech · enemy · boss · hud · music · trans · objects · terrain2),
> `Tools/cvm2/accept12_ledger.py` (sổ điểm từ `ref/data/map2_events.txt`), `Tools/cvm2/accept12_objects.py` (MAP2_PLACEMENT ↔ JSON ↔ ConfigDrop).
> Số liệu gọn (commit): **`ClaudeNotes/cvm2/ref/data/accept12/`**. Thô + ảnh (không commit): `ClaudeNotes/cvm2/_cache/12/` (vết `flow60/telemetry.tsv` 26 155 dòng,
> `main/ arena/ core/` bộ hồi quy, `boss9/`) + `_cache/shots/12/` (`flow60/` H8, `route/` 36 ảnh chuyển cảnh, `route_pairs/`, `terrain1/` 28, `terrain2/` 25,
> `terrain2v/`, `terrain_pairs/`, `objects/` 26 + `pairs_0..6`).
> **"Luồng thật"** = `Map1AcceptProbe.StartFlow(60)`: Title → Intro → Map 1 → bản đồ → **STAGE 04 → 05 → 06 → Queen Medusa → tally → bản đồ B2 → STAGE 07**, thời gian
> thực 60 fps, phát `map1_route` + `map2_route` (autopilot CVM2 11 — **bất tử suốt**, như Map 1). Mặt 3–9 đo trên luồng này (không phải Arena); Arena chỉ dùng
> để chạy lại probe phòng thật của 09/10 và bộ hồi quy.

## 0. Bảng tổng + kết luận

| # | Mặt | Kết quả | Tóm tắt bằng chứng |
|---|---|---|---|
| 1 | Địa hình & pixel | **ĐẠT** | 25 ảnh nền chụp từ **StageScreen thật** (6 section × 1–5 cx) ↔ sheet Level 2 đã vá: **0 / 45 056 px lệch mỗi ảnh** (NES y 48–223); 21 cặp video cùng cx: lệch > 48 chỉ 1.25–3.77 % (= sprite động/khối vỡ), bố cục trùng (ảnh đã xem) |
| 2 | Vật thể | **ĐẠT** | 51/51 nến trùng 3 nguồn (MAP2_PLACEMENT ↔ JSON ↔ ConfigDrop: id, x, Y, nội dung), 6/6 khối; **luồng thật**: 51/51 nến hiện đúng (section, x, Y), 6 khối, 3 bí mật, **12/12 SpawnPoint sinh đúng chỗ**; probe phòng thật 101/101; **89 cặp ảnh video ↔ Unity — 87 cặp 0–1 px** (2 cặp = Spear Knight đang đi/rơi ở video). Còn: 2 bí mật **chỉ wiki** (khối II TEB, túi vòm 4) |
| 3 | Simon trên cơ chế mới | **CHƯA ĐẠT** (1 lệch) | bệ: luồng thật ↔ video **rms 0.73 / 0.78 px** (Hall B, cùng trục tick từ khung đầu section), Unity = công thức Config mọi tick (lệch 0–1 px) ⇒ TEB 0.75 px; Simon đứng bệ: x Simon − mép bệ dao động ±1 px (rms 0.37–0.45; video ±0.56/0.25/0.00); bẫy ép **97 / 95 / 83 % khung cùng tick, 100 % trong ±2 tick**; chết hố TEB/Hall B, gai Hall C, knockback Medusa Head **hất xuống hố** ⇒ hồi sinh đúng đầu stage (6/6 kịch bản). **LỆCH**: Simon trúng đòn lùi **12 px** (Config Map 1) — video Boss_Map_2 lùi **31–32 px** + giữ đỉnh ~16 tick (CVM2 09 §2k) ⇒ quyết định hất xuống hố Block 2 sai quãng (nợ `cvgp-fix-03`, chưa duyệt) |
| 4 | Enemy Block 2 | **CHƯA ĐẠT** (2 lệch) | trên **map thật**: Medusa Head vx **1.250**, đỉnh–đáy 67, sinh ở **chân − 49** mép trước mặt (x màn 8 / 248), nhịp **128** (6/6 khoảng); **19 vết video ↔ Unity cùng section/hướng: rms trung vị 1.41, max 1.85 px**; Spear Knight **0.500** px/tick, quay ở tường 335/324 & mép 484/488 (video 335–337 / 483–484); Bone Tower cầu **2.000**, cặp cách **40**, cao chân − 15, x tháp; Ghost **±96 sau lưng**, tâm = sàn, **0.750 / 0.125**; dơi treo rời chỗ ở \|dx\| 95 dy 47 / \|dx\| 29 dy 77 (luật < 96, < 80), sà **1.5 px/tick, hạ 31 px / 42 tick**; rắn g **0.125**, trườn **1.375**. **LỆCH**: (a) **đồng hồ (Stopwatch) đóng băng 300 tick (wiki) — video Map_2 186–196 tick** (4 lần dùng; bẫy + TIME đứng f4829–4922 ≈ 188–192 tick) — treo từ CVM2 02, chưa routine nào sửa; (b) **enemy rơi vật**: video 3 lần (2 Medusa Head ⇒ tim nhỏ, Bone Tower ⇒ rosary), Unity không có luật (chờ §5 #14) |
| 5 | Queen Medusa | **ĐẠT** | luồng thật: khoá cx 0 khi Simon x 128 → **thức +180** (video 180) → bước đầu (113,113) (video 112,113) → 4 điểm bay tự do đầu tiên ↔ video Map_2 f6826–6833 **rms thô 1.12 px**; rắn 1 ↔ video 1.00 px; **Poison Mind = thức + 57**; bay 0.875 mọi đoạn; ENEMY **16 lần −1** (roi cấp 3); **+3000**; quả cầu **+152** tick (video 150/154) tại (128,104), chạm sàn sau **19** tick (video 18–20). Phòng thật (Map2BossProbe chạy lại): **13 vết rms trung vị 1.12, max 2.78 px — trùng từng số routine 09**. (Trận trong luồng thật bị roi khựng liên tục ⇒ không so được độ dài đoạn bay/dừng với video — đã so ở phòng thật.) Ảnh hưởng chéo: thời lượng đồng hồ (mặt 4) + knockback (mặt 3) |
| 6 | HUD & luật | **ĐẠT** (1 lệch nhỏ có lý do) | **sổ điểm video 86/86 sự kiện khớp HUD** từ trạng thái đầu Map_2 (SCORE 23370, P-03, tim 05, rìu II, roi 3) qua đúng code game ⇒ **1UP đúng lúc rương (30370 ⇒ P-04)**, **tally ⇒ SCORE 44670 = video**; luồng thật: TIME 0400 vào STAGE 04, **không đặt lại qua STAGE 05/06** (383→383, 347→347), nhịp **64 tick/đv** (88 lần), tally = điểm + TIME×10 + tim×100 (9940 + 3060 + 500 = **13500** khớp); sát thương **8 đòn × 2 vạch** (Medusa Head, Hall A) ⇒ chết ⇒ hồi sinh. Lệch nhỏ: TIME giảm lần đầu sau khi vào section 64 tick (video 56–62) — giữ luật Block 1 theo yêu cầu routine 11 (§5 #16) |
| 7 | Nhạc | **ĐẠT** (2 điểm bằng chứng yếu) | luồng thật: **Stalker 1 lần** ở khung đầu STAGE 04 (video −3 tick), **không phát lại** qua 3 đổi tầng + 2 cửa; lặp mỗi **1790 / 1789 tick** (video 29.82–29.84 s); Poison Mind thức + 57; **Stage Clear cùng tick nhặt cầu**; im ở tally + bản đồ; Wicked Child ở STAGE 07; 0 lần "đứng". Chưa dựng: đồng hồ dừng nhạc 2.40 s (1 mẫu, 3 mẫu trận boss ngược lại — §5 #7); Wicked Child video bắt đầu ~24 tick trước STAGE 07 hiện (NCC 0.48 — yếu) |
| 8 | Chuyển cảnh | **ĐẠT** | **24/24 mốc trong ±2 tick**: đen bản đồ → St04 27 (28), đổi tầng 21/21/21 (22/22/20) hiện (386,189)/(66,189)/(674,189) cx 256/0/512 = video, **cửa trái #1 128/+8/+110/+288, #2 128/+15/+119/+294 = video**, khoá → thức 180, hạ → cầu 152, nhặt → TIME 224 (224.6), TIME 0 → tim 58 (58.1), nhịp tim 8, tim cuối → xong 120 (118.2), tally → bản đồ B2 20 (21), bản đồ B2 395 (393), → STAGE 07 27 (28); STAGE 07 TIME 0500 tim 05 ENEMY 16; **36 cặp ảnh** video ↔ Unity (đã xem 5/9 tấm) |
| 9 | Luồng, tất định, độ bền | **ĐẠT** | luồng thật Title → STAGE 07 **`caf21ae2` ở 60 / 30 / 144 fps** + `map2` + lần chạy có vết thụ động; trước STAGE 04 **`165b9aef`** ×4; H8 **352/432/448/890/922** ×3; GameObject 495 (luồng) → 517 (sau spam, pool) và **không tăng** qua flow30/flow144; **spam5–7 (St04/05/06) 0 lỗi console**; Game Over ở St06 ⇒ CONTINUE ⇒ đầu STAGE 06 (Hall C (720,112) quay trái, P-03, điểm 0, TIME 0400) |
| 10 | Hồi quy Map 1 (H11) | **ĐẠT — 0 hồi quy** | Arena `full` 14/14 bộ = bảng chuẩn (long, simon, attack, item, enemy, boss, b23, b46, b24, b56, holy, mech, rules, gallery `ffd5b71e`); core `76516076`; Main 14 bộ lệch 0; địa hình Map 1 **28/28 ảnh 0 px** (StageScreen thật); mọi file Level Block 1 (prefab + Objects + Config + Art) **không đổi byte nào từ commit CVGP 21**, ConfigDrop chỉ thêm 57 dòng Block 2; builder Map 1 "ghi 0" (69 vật) |
| 11 | Chất lượng | **ĐẠT** (1 nợ công cụ) | cảnh đông Block 2: Hall B 13 thực thể **Step TB 0.052 ms / max 0.24**, Hall C 19 thực thể **0.058 / 0.43 ms**, **0 B GC**, GameObject 213 → 213; luồng thật Step TB 0.037 ms, 0 B; console 0 lỗi gameplay; sprite 246 + 9 PNG 0 lỗi; SOURCES **27/27** sha256 + 16/16 MP3 trong Assets; submodule `4685d33` (H3); builder python + 8 menu Unity "ghi 0"/byte y hệt; **`CVM2/Build Map2 Screens` KHÔNG idempotent ở mức byte** (sinh lại fileID ngẫu nhiên cho prefab lồng — nội dung chuẩn hoá ID giống hệt; đã trả 3 prefab về HEAD) |

**KẾT LUẬN CHUNG: CHƯA ĐẠT.** Map 2 **chơi được trọn** Title → Map 1 → STAGE 04–06 → Queen Medusa → STAGE 07 (tất định 30/60/144 fps, 0 hồi quy Map 1)
và khớp video ở địa hình (0 px), vật thể (51/51), cơ chế bệ/bẫy, quỹ đạo enemy trên map thật (rms ≤ 1.85 px), Medusa, mọi chuyển cảnh (24/24 ±2 tick), nhạc
và chuỗi điểm (86/86 ⇒ 44670). Nhưng **2 mặt cốt lõi còn lệch video**: (1) **đồng hồ đóng băng 300 tick, video 186–196** (Simon dùng 4 lần trong Map_2 —
thấy rõ ở bẫy ép, TIME, trận Medusa); (2) **Simon trúng đòn lùi 12 px, video Boss_Map_2 31–32 px** (quyết định có bị hất xuống hố hay không); kèm (3) enemy
rơi vật (3 lần trong video) chưa có luật. Cần người dùng: duyệt routine sửa (§6 dưới), trả lời §5 #14 (luật rơi vật), **nghe thử nhạc Map 2**, **bấm thử gamepad**,
sheet HUD bản US (chữ số TẠM — §5 #2).

## 1. Địa hình & pixel
- **Phương pháp**: Main.unity Play, `GameRules.NewGame(); GoTo(Stage1,0)` → `Map1AcceptProbe.StartTerrain(dir, "TowerEntranceA:0;TowerEntranceB:0,76,128,153,256;TowerHallA:0,256,312,492,512;TowerHallB:0,224,256,497,512;TowerHallC:0,120,239,256,512;Medusa:0,114,256,512")`
  — section của **StageScreen thật** (không phải section dựng lúc chạy ở Arena như CVM2 05), ẩn sprite động, HUD còn ⇒ so NES y 48–223 với vùng sheet Level 2 đã vá
  (`python -P -E Tools/cvm2/accept12.py terrain2 <dir>`). Video: `accept5.py pairs` (21 cặp cùng cx, sau đổi màu sheet → video).
- **Số liệu**: 25/25 ảnh **0 px** (`ref/data/accept12/terrain2.txt`); cặp video: TB |hiệu| 4.2–6.9, lệch > 48 **1.25–3.77 %** (`terrain_pairs.txt`) — trùng CVM2 05.
- **Ảnh đã xem**: `_cache/shots/12/terrain_pairs/pairs_0.png` (TEA ×2, TEB ×2), `pairs_4.png` (Hall C ×2, Medusa ×2) — đỏ chỉ ở Simon, nến, quái, khối vỡ, bẫy; nền trùng.

## 2. Vật thể
- **Ba nguồn** (`accept12_objects.py`): 51/51 nến trùng (id, x, Y, nội dung) MAP2_PLACEMENT ↔ Map2Objects.json ↔ ConfigDrop; 6/6 khối; JSON 12 SpawnPoint + 5 EdgeSpawner + 3 bí mật + 3 MechSpawner + 1 BossArena; ConfigDrop stage 4–6 = 57 dòng.
- **Luồng thật** (vết dòng A/E, `accept12_objects.txt`): 51/51 nến hiện đúng (section, x, Y), 6 khối (`TowerEntranceA_X14_5/6`, `TowerEntranceB_X22_5`, `TowerHallB_X39_3`, `TowerHallC_X44_7`, `TowerHallC_X8_8`), 3 bí mật, 12/12 SpawnPoint enemy sinh đúng (x, Y).
- **Phòng thật / ảnh** (Arena-Simon, `Map2ObjectsProbe.Start("candles,blocks,secrets,spawns,mech,boss", …)` + `pairs10.py compare`): **101/101**; 26 ảnh ⇒ **89 cặp, 87 cặp 0–1 px**, |dx| max 2, |dy| max 4 (Spear Knight TEB f1265/f867 đang đi/rơi ở video — Unity chụp ở điểm sinh). Đã xem `_cache/shots/12/objects/pairs_0.png` (rương, vương miện, nến sảnh Medusa), `pairs_5.png` (nến + 2 Bone Tower Hall C).
- Nội dung nến = video 51/51 (CVM2 10 — mọi nến bị phá trong video); 2 bí mật chỉ wiki (khối II TEB, túi vòm 4) — chưa có video để đo.

## 3. Simon trên cơ chế mới (luồng thật + kịch bản không bất tử)
| Đại lượng | Video Map_2 | Unity (luồng thật / kịch bản) | |
|---|---|---|---|
| Bệ TEB (mép trái theo tick từ khung đầu section) | 143 điểm f1351–1552 | Unity luồng thật = công thức Config 709/709 tick (lệch 0); video ↔ công thức **rms 0.75 px** (max 2) — luồng Unity rời TEB trước lượt video nên so qua công thức | ĐẠT |
| Bệ Hall B 1 / Hall B 2 | 328 / 289 điểm | **so trực tiếp cùng tick: rms 0.73 / 0.78 px** (163 / 180 điểm, max 2); Unity = công thức (lệch 0 / ≤ 1) | ĐẠT |
| Simon đứng trên bệ (x Simon − mép bệ) | hằng ±0.56 / ±0.25 / ±0.00 | 17–18 / 17–18 / 16–17 (rms 0.37–0.45) khi bệ trôi 29 / 62 / 36 px | ĐẠT |
| Bẫy ép (khung từng frame f4479–4828, trước đồng hồ) | 37 / 77 / 150 frame | khung trùng **cùng tick 97 / 95 / 83 %, ±2 tick 100 %**; khung tối đa 6 / 4 / 7 | ĐẠT |
| Rơi hố ⇒ hồi sinh | (video không rơi) | `pit_teb`: rơi khe TEB ⇒ Fall ⇒ **TEA (64,176) quay phải cx 0**, P-02, TIME 0400; `pit_hallb`: ⇒ **Hall A (720,80) quay trái cx 512** | ĐẠT |
| Gai bẫy ép | (video né) | `crusher_hallc`: đứng dưới bẫy 3 ⇒ Hazard @129 ⇒ **Hall C (720,112) quay trái**; wiki "chết ngay" | ĐẠT (suy luận wiki) |
| Knockback xuống hố | wiki "Head hất xuống hố" | `knock_pit_hallb`: Medusa Head trúng (164,160) ⇒ lùi 12 px ⇒ rơi khe ⇒ Fall ⇒ đầu STAGE 05 | ĐẠT (cơ chế) |
| **Quãng lùi khi trúng đòn** | **Boss_Map_2: 31–32 px / 36 tick, đỉnh −14 giữ ~16 tick** (CVM2 09) | **12 px** (Config Map 1, `damage_halla` 12,12,12 rồi kẹt tường) | **LỆCH** — `cvgp-fix-03` |
| (thông tin) lộ trình bất tử dưới bẫy | — | ước lượng hộp 16×30: lộ trình CVM2 11 chồng vùng gai bẫy 3 ~10 tick (cheat bất tử miễn gai) — người chơi thật phải canh nhịp (CVM2 06 `crusher_pass` chứng minh qua được) | ghi chú |
Công cụ `accept12.py mech`, `Map2AcceptProbe.Run(...)` (`ref/data/accept12/accept12_mech.txt`, `scenarios.txt`). Bẫy lượt 2 (3:04–3:16 sau đồng hồ) vẫn chưa giải pha (§5 #8).

## 4. Enemy Block 2 trên MAP THẬT (`accept12.py enemy`, `accept12_enemy.txt`)
| Loại | Video (CVM2 08 §2j) | Luồng thật Unity | |
|---|---|---|---|
| Medusa Head | 1.25 px/tick; 1 − cos A 33.75 P 100 (đỉnh–đáy 67–68); sinh ở đỉnh = chân − 49, mép trước mặt inset 8; đều 128 | 8 con: vx **1.250**, đỉnh–đáy **67**, sinh **chân − 49**, x màn 8 / 248 trước mặt Simon, khoảng **128 ×4** (+ 1234 = Simon rời vùng); so hình dạng với **19 vết video** cùng section/hướng (tịnh tiến, lệch t ±3): **rms trung vị 1.41, max 1.85 px** (vết `mh_m #2` bỏ 3 điểm ghép nhầm của bộ dò — CVM2 08 cũng loại) | ĐẠT |
| Spear Knight | 0.5; quay tường 335–337, mép 483–484/572 | 7 con: **0.500** khi Walk; TEB khe quay 335/324…, Hall B 484/488 | ĐẠT |
| Bone Tower | cầu 2.0, cặp cách 40, cao chân − 15, x tháp; lượt cách 185–314 | 8 cầu **2.000**, cặp **40**, **−15**, dx **0**; lượt 303; tháp (313,48) sinh lại khi vào khung ⇒ bộ đếm khởi lại (+41) | ĐẠT |
| Ghost | 96 px sau lưng, tâm = sàn, 0.74–0.75 / 0.10–0.12 | **+96 / −96** sau lưng, y = chân, vx 40 tick đầu **0.750**, \|vy\| ≤ **0.125** | ĐẠT |
| Dơi treo | \|dx\| < 96, dy < 80; sà 1.5, hạ 31 / 42 tick, rồi bay ngang | rời chỗ ở \|dx\| 95 dy 47 (TEA, TEB), \|dx\| 29 dy 77 (Hall C); **1.500, 31 px / 42 tick** ⇒ Glide | ĐẠT (video TEA/TEB dơi bị thập tự hạ khi còn treo) |
| Rắn | g 0.10–0.135; 1.376–1.396 | g **0.125** ×3; trườn **1.375** | ĐẠT |
| **Đồng hồ đóng băng** | **186–196 tick** (bẫy + Bone Tower đứng f4829–4922; TIME 0270 giữ 192 tick; 3 lần trận Medusa) | **300 tick** (`ConfigSubWeapon.Stopwatch.freezeTicks`, wiki 5 s) | **LỆCH** |
| **Enemy rơi vật** | 3/17 lần hạ: 2 Medusa Head ⇒ tim nhỏ, Bone Tower ⇒ rosary | không có luật (`dropItem` None) | **LỆCH** (chờ §5 #14) |

## 5. Queen Medusa (`accept12.py boss`, `accept12_boss.txt`, `boss9_compare.txt`)
- Luồng thật: khoá camera wt 6160 cx 0 khi Simon x 128 (video f6735) → **thức wt 6340 = khoá + 180** (video 180) → bước đầu (113,113) (video tâm đầu f6826 (112,113)) → bay tự do 14 tick tới nhát roi đầu ⇒ **4 điểm ↔ video Map_2 rms thô 1.12 px** (không tịnh tiến, lệch t 0) → rắn 1 ↔ video 1.00 px → **Poison Mind thức + 57** → roi cấp 3: **16 lần ENEMY −1** (video 16) → +3000 wt 6754 → **quả cầu +152** (128,104) → chạm sàn sau 19 tick → nhặt ⇒ Stage Clear cùng tick.
- Trận trong luồng thật: Simon (autopilot) quất mỗi ~25 tick ⇒ Medusa khựng 10 tick liên tục (Stun) ⇒ đoạn bay/dừng bị cắt — **không so được với video** (video Map_2 bị đồng hồ đóng băng, video boss ném nước thánh). Quỹ đạo bay tự do so ở phòng thật: `Map2BossProbe.Run("wake_b2,wake_m2,seg_de,death")` + `boss9_compare.py` ⇒ **13 vết rms trung vị 1.12, max 2.78 px — trùng từng dòng `ref/data/boss9/compare.txt` (CVM2 09)**.
- Ảnh đã xem: `_cache/shots/12/route_pairs/pairs_7.png` (khoá cx 0 + tượng, Medusa thức đầu xanh, quả cầu, tally).

## 6. HUD & luật
- **Sổ điểm video** (`accept12_ledger.py` ⇒ `ref/data/accept12/ledger.tsv` 86 dòng; `Map2AcceptProbe.Ledger` ⇒ `ledger_result.txt`): đặt trạng thái đầu Map_2 (SCORE 023370, P-03, tim 05, rìu × II, roi 3, STAGE 04 TIME 0400) rồi áp từng sự kiện HUD video qua code game — giết enemy/boss = `GameRules.AddScore(điểm Config)` như `EnemyBase`/`BossBase`, nhặt = `ItemEffects.Apply`, ném = `ConfigSubWeapon.heartCost`, đổi stage = `GameRules.EnterStage`, tally = `StageClearTally` Block 2 (chờ 224 / nghỉ 58), sang block = `StartNextBlock` ⇒ **86/86 khớp HUD** (điểm, tim, P, vũ khí, hệ số): thập tự làm mất II ✓, II khi cầm đồng hồ = +700 ✓, **rương 2000 ⇒ 30370 ⇒ P-04 (1UP)** ✓, Medusa +3000 ⇒ 39570 ✓, tally TIME 210 (+2100) + tim 30 (+3000) ⇒ **044670** ✓, STAGE 07 tim 05 ✓.
- Luồng thật (`accept12_hud.txt`): TIME 0400 ở STAGE 04; **0 lần đặt lại** trong Block 2 (STAGE 04→05 383→383, 05→06 347→347); khoảng giảm 64 tick × 88; tally 9940 + 306×10 + 5×100 = 13500 = Unity.
- Sát thương: `damage_halla` 8 đòn Medusa Head **mỗi đòn −2 vạch** (ConfigBlock block 2) ⇒ hết 16 ⇒ Damage ⇒ hồi sinh đầu STAGE 05.
- Lệch nhỏ (có lý do, §5 #16): TIME giảm lần đầu sau khi vào section 64 (video 56–62).
- Ảnh HUD: chữ số TẠM (bản Famicom — chờ sheet US, §5 #2); các cặp ảnh chuyển cảnh cho thấy nhãn/thanh/ô vũ khí trùng vị trí.

## 7. Nhạc (`accept12_music.txt`, `_cache/12/flow60/music.txt`)
| Mốc | Video (§2f) | Luồng thật Unity | |
|---|---|---|---|
| Bản đồ B1 | im | stop (frame 9759) | ✓ |
| STAGE 04 | Stalker 6.177 s (STAGE 04 hiện 6.23 s, −3 tick) | Stalker ở khung đầu STAGE 04 (0 tick) | ✓ (±3) |
| Đổi tầng ×3, cửa trái ×2, STAGE 05/06 | KHÔNG phát lại | 1 lần play; lặp 12757 → 14547 → 16336 (**1790 / 1789** tick) | ✓ |
| Medusa thức | Poison Mind +46–57 tick | +57 | ✓ |
| Nhặt cầu | Stage Clear cùng lúc | cùng tick (17198) | ✓ |
| Tally / bản đồ B2 | im | end SC → stop ở bản đồ | ✓ |
| STAGE 07 | Wicked Child 4:19.80 (STAGE 07 hiện 4:20.20; NCC 0.48 — yếu) | ở khung đầu STAGE 07 (~24 tick muộn hơn) | ghi chú |
| Dùng đồng hồ | 2:40.97 Stalker đứng 2.40 s (3 lần trận boss: không thấy) | không dựng | chờ §5 #7 |
MusicProbe: 0 lần "đứng". **Cần người dùng nghe thử** (không tự đánh giá cảm nhận âm thanh được).

## 8. Chuyển cảnh (`accept12_trans.txt`, `route_pairs.txt`)
24/24 mốc ±2 tick (bảng ở §0 mục 8). `Map2RouteProbe.Replay(shotDir, 40)` (hash `caf21ae2`) ⇒ 36 ảnh ⇒ `pairs11.py` ⇒ 9 tấm. **Đã xem**: `pairs_0.png` (vào STAGE 04 + khung đầu TEB), `pairs_1.png` (cửa #1 cuộn 1 / cánh mở r009 / r015 — r002 Unity cánh hiện sớm ≤ 1 frame), `pairs_3.png` (cửa #1 cuối + cuộn 2 + Hall A + khung đầu Hall B), `pairs_6.png` (cửa #2 cuối → Hall C — nến Hall C video hiện chậm 1–2 frame (§6 11) + khung đầu sảnh Medusa), `pairs_7.png`, `pairs_8.png` (bản đồ B2 ×3: vị trí trượt, dơi/chấm, Simon; STAGE 07 = stub nền đen, video có nền Block 3 — ngoài phạm vi).

## 9. Luồng, tất định, độ bền (`regress_main.txt`, `scenarios.txt`)
- `RegressionRunner.Start("main,flow30,flow144", 60, …)` (Main, 1384 s): flow / flow30 / flow144 / map2 = **`caf21ae2`**, trước STAGE 04 **`165b9aef`**, h8 5/5 ×3, spam1–7 0 lỗi console (spam5/6/7 = đầu STAGE 04/05/06; Step TB 0.037 ms, 0 B GC). Lần chạy luồng thật CÓ vết thụ động (flow60) cũng `caf21ae2`, 0 lỗi console.
- GameObject: 495 sau luồng; 495 → 517 sau spam (pool), **517 không đổi** qua flow30/flow144.
- `gameover_st6`: P-01 + gai ⇒ GAME OVER (tick 161) ⇒ Start (CONTINUE) ⇒ **TowerHallC (720,112) quay trái cx 512, STAGE 06, P-03, SCORE 0, TIME 0400**.

## 10. Hồi quy Map 1 (`regress_arena.txt`, `regress_core.txt`, `terrain1.txt`)
- Arena-Simon `full` @60 (1667 s): long `5963c4ba` · simon `b2954cbe` 20/20 · attack `eee2cdfb` 21/21 · item `7ca5bf56` 14/14 · enemy `f288b0ce` 20/20 · boss `31e6d711` 12/12 · b23 `5eb00c5b` 35/35 · b46 `58a8edf9` 26/26 · b24 `9b218696` 21/21 · b56 `6cb8a1ad` 14/14 · holy `b1033a3f` 5/5 · mech `91215ebd` 14/14 · rules `03fafb24` 68/68 · gallery `ffd5b71e` 75/75 — **lệch 0**. Arena-Dummy core `76516076` 21/21.
- Các mục ❌ của `ClaudeNotes/cvgp/ACCEPT21.md` (lịch sử, giữ nguyên file): **nhạc** — đã nhập + gắn mốc (CVM2 03; mặt 7 ở đây ĐẠT); **lọ nước thánh xuyên địch** — đúng từ CVGP 04/09, kiểm bằng video ở CVM2 04 (bộ `holy` `b1033a3f` 5/5 lần này); **Block 2 "chỉ theo wiki"** — nay đo video (CVM2 08/09, mặt 4/5 ở đây); còn mở: knockback/mép rơi Simon (`cvgp-fix-03`), báo #3 + Phantom Bat (`cvgp-fix-04`), credits sau Dracula.
- Các mục ĐẠT của CVGP 16/21: địa hình Map 1 **28/28 ảnh 0 / 45 056 px** (`accept16.py terrain`, StageScreen thật); vật thể Map 1: `git diff 14d5c20..HEAD` trên `Assets/Data/{Prefabs,Config,Art}/Level/Block1` = **0 file**, ConfigDrop +57 dòng (chỉ thêm), `CVGP/Build Map1 Objects` "69 vật — ghi 0" ⇒ 71/71 dòng CVGP 21 không đổi; luồng Map 1 = `165b9aef` + H8.

## 11. Chất lượng
- Hiệu năng (`Cvm2SweepProbe.RunCrowd`, Arena 240 fps, 1500 tick): Hall B (2 bệ + enemy + Medusa Head + nước thánh ×3) tối đa 13 thực thể, **Step TB 0.0515 ms, max 0.238 ms**; Hall C (3 bẫy + 2 Bone Tower + …) 19 thực thể, **0.0576 / 0.428 ms**; **0 B GC**, pool 121, GameObject **213 → 213**.
- Console: 0 lỗi gameplay (chỉ lỗi nền đã biết); probe mới biên dịch sạch (đã xoá 1 CS0414 của chính nó).
- Repo: `git status` sạch ngoài file mới của routine + nhiễu nền; submodule `4685d33` = ghi nhận (H3); `SOURCES.md` 27/27 hàng sha256 khớp file + 16/16 MP3 trong `Assets/Data/Audio/Music` khớp.
- Builder: python `build_tileset/build_collision(2)` "same", `gen_map1/2_objects.py` byte y hệt; `verify_sprites.py --meta` 246 PNG + Map 2 9 PNG **0 lỗi**; menu `CVGP/Import Config` (763 dòng, ghi lại 0), `Build AnimSets` (28, ghi 0), `Build Map1 Tilemaps` / `Build Map1 Objects` / `CVM2/Build Map2 Tilemaps` / `Build Map2 Objects` (75 vật) ghi 0, `Build Mech Prefabs` md5 y hệt, `Build MusicLibrary` không đổi. **`CVM2/Build Map2 Screens`: Main.unity "không đổi" nhưng 3 prefab màn hình bị ghi lại với fileID mới** (857 dòng ±, chuẩn hoá ID ⇒ chỉ thứ tự khối YAML khác) — đã trả về HEAD; ghi nợ.
- Sổ ↔ code: đường dẫn/API của §3l/§4 (11) dùng trong routine này đều có thật (`Map2RouteProbe`, `Map1AcceptProbe.StartFlow/StartTerrain`, `RegressionRunner`, `Map2ObjectsProbe`, `Map2BossProbe`, `Cvm2SweepProbe`, `MusicProbe`, `Map2FlowTiming`).

## 12. Routine sửa đề xuất (KHÔNG tạo — người dùng duyệt; model · effort theo `skill-commond`)
| Routine | Model · effort | Việc | Bằng chứng |
|---|---|---|---|
| `cvm2-fix-01-dong-ho-theo-video` | Opus 5.5 · high | Thời lượng đóng băng Stopwatch theo video (186–196 tick; kiểm giả thuyết "3 đơn vị TIME = 192 tick, TIME đứng cùng") + nối lại theo nhịp TIME (bẫy chạy lại cùng frame TIME — §6 06) + quyết định dừng nhạc 2.40 s; số DÙNG CHUNG (H13) ⇒ đổi hash Block 1 `item`/`rules`/`b23 freeze_*`/`b24`/`gallery`, chạy lại bộ hồi quy + `CHARACTER_STATS.md` | `ref/data/map2_hud.csv` f4827→f4923, `mech6/crusher_runs.txt` f4829–4922, CVM2 09 §2k (3 lần trận Medusa), §2d nhạc |
| `cvgp-fix-03-simon-knockback-nuoc-thanh-mep-roi` (đề xuất từ CVGP 16 — **nay chặn cả Map 2**) | Opus 5.5 · high | Knockback Simon theo video: 31–32 px / 36 tick, đỉnh −14 GIỮ ~16 tick (Boss_Map_2 f351–372, chạm tường f106–123) — đo lại cả Map_1; quỹ đạo lọ nước thánh; mép rơi | CVM2 09 §2k "Mẫu knockback", `ref/data/boss2_simon.csv` |
| `cvm2-fix-02-enemy-roi-vat` | Opus 5.5 · high | Luật rơi vật khi hạ enemy SAU khi người dùng chọn §5 #14 (a/b/c) | Map_2 f2056→f2068, f6227→f6236, f5653→f5662 (`_cache/10/view/ha528.png`, `md_heart.png`, `hc_rosary.png`) |
| `cvm2-fix-03-hoan-thien-nho-map2` | Opus 5.5 · high | Gom lệch nhỏ: pha TIME sau khi vào section (§5 #16, có thể đổi hash Map 1), tally 224 vs 260 (§5 #17), nến section sau cửa #2 hiện chậm 1–2 frame, Spear Knight `TEB_gap` đã rơi 6 tick, Bone Tower nháy đỏ kép (3/7 lượt), dơi treo trễ 2–4 tick, Wicked Child bắt đầu trong màn đen, **builder `Build Map2 Screens` giữ fileID ổn định (idempotent byte)** | §6 (02/08/10/11/12) |

Thứ tự khuyến nghị: fix-01 → cvgp-fix-03 → (người dùng trả lời #14) fix-02 → fix-03. Sau đó chạy lại routine nghiệm thu này (công cụ có sẵn: §13).

## 13. Chạy lại nghiệm thu (lần sau)
(1) Main.unity Play (`DisableDomainReload`) → AppActivate → `runInBackground = true` → `GameSession.Current.ResetForNewGame(); Map2AcceptProbe.StartTelemetry("<_cache>/12/flow60/telemetry.tsv"); MusicProbe.Start(); Map1AcceptProbe.StartFlow(60, "<shots>/flow60")` (~5 phút) → `StopTelemetry()`, `MusicProbe.Stop(…)`, `SaveReport(…)` → `python -P -E Tools/cvm2/accept12.py all <telemetry.tsv>`;
(2) `Map2RouteProbe.Replay("<shots>/route", 40)` → `pairs11.py <route> <route_pairs>`; (3) `GameRules.NewGame(); GoTo(Stage1,0)` → `Map1AcceptProbe.StartTerrain` (Map 1 spec CVGP 21 + Map 2 spec §1) → `accept16.py terrain`, `accept12.py terrain2`, `accept5.py pairs`;
(4) `Map2AcceptProbe.Run(Map2AcceptProbe.ScenarioNames(), "<_cache>/12/scenarios.txt")`; (5) `python -P -E Tools/cvm2/accept12_ledger.py` → `Map2AcceptProbe.Ledger("<ledger.tsv>", "<ledger_result.txt>")`;
(6) phiên Main mới: `RegressionRunner.Start("main,flow30,flow144", 60, …)`; (7) Arena-Simon: `full` → `Map2ObjectsProbe.Start(…, spec từ ảnh _cache/shots/10/unity2)` → `pairs10.py compare`, `Map2BossProbe.Run(…)` → `boss9_compare.py <dir> <ra>`, `Cvm2SweepProbe.RunCrowd("TowerHallB"/"TowerHallC", 1500, seed)`; (8) Arena-Dummy `core`; (9) `accept12_objects.py`, builder python/menu, `verify_sprites.py`, SOURCES sha256.

## 14. Cụm tiếp theo đề xuất (KHÔNG tạo)
**CVM3 — Map 3 (Block 3, Stage 07–09, boss Mummy Men)** cùng khuôn CVM2 (00 hạ tầng + baseline · 01 nhập sheet · 02 hiệu chuẩn video · 05 tilemap Block 3 · 06 cơ chế mới (bệ nổi/nước St 8–9 nếu có) · 08 enemy Block 3 đo video (Ghost, Raven, Skeleton, Medusa Head, Bone Tower, Fleaman/Eagle…) · 09 Mummy Men · 10 vật thể · 11 luồng thay `Stage7Stub` → STAGE 10 stub · 12 nghiệm thu).
**Điều kiện trước**: (1) người dùng **tải tay** sheet `NES - Castlevania - Stages - Level 3_ …` từ spriters-resource (Cloudflare chặn tải tự động); (2) video `Map_3.mp4` (tiếp playthrough từ STAGE 07) + `Boss_Map_3_Behaviour.mp4`; (3) duyệt + chạy trước `cvm2-fix-01` (đồng hồ — dùng chung Block 3) và `cvgp-fix-03` (knockback — Block 3 có nước/hố); (4) quyết định `cvgp-fix-04` (Phantom Bat) và §5 #14/#16/#17; (5) bản rip Wicked Child 2 vòng nếu muốn lặp sạch (§6 03 — gain 0.49 ở điểm lặp).
