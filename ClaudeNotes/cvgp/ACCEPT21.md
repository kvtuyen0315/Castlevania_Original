# ACCEPT21 — Nghiệm thu cuối cụm CVGP (routine 21, 14/10/2026)

> Chi tiết của `CVGP_MEMORY §7 "NGHIỆM THU CỤM"`. Chỉ ĐO + phán xét; không đổi gameplay. Công cụ thêm (chỉ Editor/công cụ):
> `Assets/Data/Script/Debug/CharacterAcceptProbe.cs` (audit sát thương runtime ↔ Config, phòng trưng bày 3 replay/nhân vật, Arena đông),
> `Tools/cvgp/stats_audit21.py` (Config ↔ văn bản wiki gốc), `Tools/cvgp/gallery21.py` (ghép ảnh), `ClaudeNotes/cvgp/behavior_status.csv`
> (cột "hành vi đã dựng / mức gần đúng" của `CHARACTER_STATS.md` — exporter `CVGP/Export Stats Report` đọc file này).
> Số liệu thô + ảnh: `ClaudeNotes/cvgp/_cache/21/` (`stats_audit.md`, `suites/*.txt`, `gallery240/`, `gallery30/`, `sheets/gallery_*.png`,
> `enemy_compare21.txt`, `c14/compare.txt`, `pairs13_all.png`, `spawnpoints21.tsv`) + `_cache/shots/21/` (không commit).

## 0. Bảng tổng + kết luận

| Mặt | Kết quả | Tóm tắt bằng chứng |
|---|---|---|
| (a) Chỉ số mọi nhân vật ↔ nguồn | **ĐẠT** | 157/157 ô số có nguồn wiki khớp văn bản wiki gốc; 15/15 enemy + 6/6 boss (7 pha) có trong Config; 0 ô "chờ đo"; runtime 350 đòn vào 25 loại + 101 đòn vào Simon từ 30 nguồn **0 sai** (240 fps) + 70 + 60 đòn **0 sai** (30 fps). 33 ô "không rõ" có lý do (giờ Block 3–6, nguồn nhạc, điểm Snake/Dracula 1…) |
| (b) Hành vi từng enemy/boss | **ĐẠT có điều kiện** | 25/25 nhân vật chạy đủ pha mô tả, 300 ảnh đã xem + chuỗi state; Block 1 so video lại = routine 13/14/16 (y hệt số); Block 2–6 **chỉ theo wiki, gần đúng** (không video). Còn ❌: báo #3 lệch 7.7 px, Phantom Bat (lao đầu 4–6 px, lọ nước thánh chưa xuyên), credits sau Dracula |
| (c) Hồi quy Map 1 (routine 16 mục 1–6, 9) | **ĐẠT — 0 hồi quy** | địa hình 28/28 ảnh 0 px so sheet; 71/71 vật thể trùng; Simon/Attack/Item/Enemy/Boss/Rules/Core/long ra đúng hash 16; quỹ đạo enemy + Phantom Bat so video y hệt; HUD 8 ảnh trùng pixel; luồng thật 30/60/144 fps `e0b2d54c` + 18 checkpoint y hệt; spam 4 lượt 0 lỗi, trạng thái cuối trùng 16. (Các mục CHƯA ĐẠT của 16 vẫn CHƯA — chưa có routine sửa) |
| (d) Chất lượng tổng thể | **ĐẠT** | tất định: replay dài `5963c4ba` ở 60/30/144/240 fps; 75 hash phòng trưng bày 240 fps ≡ 30 fps (`517442ed`); Arena đông 23 thực thể Step TB 0.08 ms max 0.36 ms, 0 B GC/Step, GameObject 199 → 199 (lần 2); luồng thật 379 ×3; console chỉ lỗi nền; repo sạch, submodule `4685d33` không đổi (H3), SOURCES 62/62 file khớp hash, sprite 246/246 khớp sheet, builder chạy lại 0 thay đổi |

**KẾT LUẬN CHUNG: CHƯA ĐẠT.** Cụm CVGP đã có đủ *mọi* nhân vật (Simon + 15 enemy wiki + 3 thực thể phụ + 6 boss / Dracula 2 dạng) với state machine + chỉ số đúng nguồn + tất định, và Map 1 chơi trọn từ Title tới STAGE 04 không hồi quy. Nhưng cụm **chưa đạt mục tiêu "giống bản gốc"** ở: (1) **không có nhạc** (16 MP3 có sẵn, chưa nhập — việc của cụm, `cvgp-fix-02`); (2) các lệch nhỏ đã biết từ routine 16 chưa được sửa (lọ nước thánh xuyên địch, knockback/nước thánh/mép rơi Simon, báo #3, Phantom Bat; enemy rơi item chưa có luật); (3) Block 2–6 chỉ "theo wiki, gần đúng" — không có video/map để đối chiếu, chưa đặt vào màn chơi nào; (4) phần chỉ người dùng gỡ được: sheet HUD bản US (chữ số TẠM), bấm thử gamepad Flydigi; (5) kết thúc game sau Dracula (credits/ending/Second Quest) chưa làm.

## 1. Chỉ số — Config ↔ nguồn

### 1a. Đối chiếu tĩnh (`python -P -E Tools/cvgp/stats_audit21.py` — đọc THẲNG văn bản wiki lưu `_cache/wiki_raw/text_*.txt`, không qua CV_WIKI_DATA)
| Nhóm | Ô đối chiếu | ĐẠT | SAI | Cách |
|---|---|---|---|---|
| ConfigEnemy ↔ Enemies (bảng từng loại + bảng Comparison) | 75 (15 loại × đòn yếu/mạnh/điểm/Comparison/stage) | 75 | 0 | bảng máy đọc được của trang Enemies |
| ConfigBoss ↔ Bosses | 26 (đòn yếu/mạnh/điểm 6 boss + stopwatchAffects 7 dòng + Dracula dạng 2 "32") | 26 | 0 | bảng Bosses + câu "Queen Medusa is the last boss that the Stopwatch will work against" |
| ConfigBlock.damageToSimon ↔ Gameplay#Damage | 18 stage | 18 | 0 | bảng Stages/Damage |
| ConfigItem ↔ Items/Gameplay/Secrets | 13 | 13 | 0 | câu trích phải có trong văn bản gốc |
| ConfigSubWeapon ↔ Items/Enemies | 10 | 10 | 0 | idem |
| ConfigWhip ↔ Items/Enemies | 4 | 4 | 0 | idem |
| ConfigSimon / ConfigScore ↔ Gameplay | 11 | 11 | 0 | idem |
| **Tổng** | **157** | **157** | **0** | |

**Đếm nhân vật:** wiki 15 enemy thường (bảng Comparison 15 dòng) ↔ ConfigEnemy 15 dòng `Normal` + 3 dòng `Sub` (PhantomBatMinor, Igor, Snake) — **0 thiếu**; wiki 6 boss ↔ ConfigBoss 6 boss / 7 dòng pha (Dracula 2 dạng) — **0 thiếu**; 22 item + 3 vật chứa, 5 vũ khí phụ, 3 cấp roi, 6 block / 18 stage.

**100 % dòng của 7 bảng theo thẻ nguồn** (mọi ô số có thẻ — importer từ chối ô số không thẻ):
| Bảng | Dòng | Ô số | wiki | video | đo | suy luận | quy ước | Ô "–"/trống (có lý do) |
|---|---|---|---|---|---|---|---|---|
| ConfigEnemy | 18 | 86 | 71 | 0 | 4 | 11 | 0 | 24 |
| ConfigBoss | 7 | 53 | 48 | 0 | 1 | 4 | 0 | 4 |
| ConfigItem | 25 | 69 | 39 | 3 | 22 | 5 | 0 | 88 |
| ConfigSubWeapon | 5 | 23 | 21 | 0 | 1 | 1 | 0 | 12 |
| ConfigWhip | 3 | 25 | 19 | 0 | 6 | 0 | 0 | 2 |
| ConfigSimon | 69 | 68 | 2 | 3 | 57 | 4 | 2 | 9 |
| ConfigBlock | 6 | 8 | 6 | 2 | 0 | 0 | 0 | 4 |
Số "đo/video" (Simon, roi, vũ khí phụ, enemy Block 1, Phantom Bat) được kiểm GIÁN TIẾP: các probe 08/09/11/12/13/14 chấm bằng hằng `VIDEO_*` độc lập Config và **đều ĐẠT** ở lần chạy này (mục 3). Số "suy luận" của Block 2–6 kiểm bằng hằng `EST_*` của probe 17–20 (ĐẠT).

**Ô "chờ đo" còn sót: 0** (routine 16 còn 1 — đã hết). **Ô "không rõ"/"chưa đối chiếu" (có lý do, 33):** giờ giới hạn Block 3–6 (4); điểm Dracula dạng 1, điểm Snake, moveSpeed Igor, sát thương roi cấp 3 so roi da, trần tim (MaxHearts ≥ 76) (5); nguồn nhạc ConfigBlock/ConfigStage "suy luận theo tên file — CHƯA đối chiếu video" (24 — nhạc chưa nhập, `cvgp-fix-02`).

### 1b. Sát thương RUNTIME (`CharacterAcceptProbe.StartAudit` — mọi `StageWorld.Hit` trong phòng trưng bày 240 fps + 9 bộ probe 08–20, 90 334 tick)
- **350 đòn vào 25 loại enemy/boss — sai 0**: mỗi đòn máu giảm đúng `CombatHealth.DamageFor(strength, W, S)` với W/S đọc LẠI từ ConfigEnemy/ConfigBoss (Mummy chia `entityCount` vì `hitsScope TotalAllEntities`), máu runtime (Health.HitsWeak/Strong) = Config ở cả 25 loại. Mỗi loại có ≥ 1 cặp (đa số cả đòn yếu lẫn mạnh): roi L1/L2/L3, Dagger, Axe, HolyWater/HolyFire. Igor: 12 đòn khi bất tử, máu không đổi (Config "–" = bất tử). Red Skeleton: 4 lần đổ (hết máu ⇒ ráp lại, không chết).
- **101 đòn vào Simon từ 30 nguồn — sai 0**: `HitInfo.damage` = `ConfigBlock.damageToSimon` của stage đang chơi và PLAYER giảm đúng số đó: stage 1–6 = 2, 7–12 = 3, 13–18 = 4 (thân + mọi đạn: cầu lửa Fish Man/tháp/rồng/Igor/Dracula, xương, rìu, băng vải, lưỡi hái).
- Đạn bị phá: BoneTowerFireball ×12, AxeKnightAxe ×6, DeathScythe ×17, DraculaFireball ×13.
- **Lặp lại ở phiên Play mới, 30 fps** (phòng trưng bày không chụp): 70 đòn vào 19 loại + 60 đòn vào Simon từ 24 nguồn — **sai 0** (`_cache/21/suites/audit30.txt`).

## 2. Hành vi — "phòng trưng bày" (`CharacterAcceptProbe.RunGallery`, scene Arena, Game view 1920×1080, 240 fps)
Mỗi nhân vật 3 replay sinh từ mẫu: **đứng yên** (Simon không bất tử — đo sát thương), **tiến lại** (đi về phía nhân vật), **tấn công** (đi 8 tick rồi quất; nhảy quất với mục tiêu cao; ngồi quất với rắn; ném rìu với Death — Simon bất tử). 4 ảnh/replay ⇒ **300 ảnh**, ghép 25 tấm `_cache/21/sheets/gallery_<tên>.png` — **đã mở xem cả 25 tấm**; chuỗi state từng thực thể + đòn + hash từng replay ở `_cache/21/gallery240/<tên>_<chế độ>.txt`. Enemy đặt ở Arena (Block 1 dùng section Map 1 như probe 13), boss ở phòng thử 14/19/20. Simon máu ≤ 8 ⇒ harness hồi 16 (không chết giữa replay).

Ký hiệu: ✅ ĐẠT · ❌ CHƯA · ❔ không xác định được (thiếu map/video) · (V) đo video Map_1/boss · (W) theo wiki, ước lượng (không video).

| Nhân vật | Checklist hành vi (`CV_WIKI_DATA §3.4/§4.3`) — bằng chứng | Mức |
|---|---|---|
| E01 Ghoul | ✅ đi thẳng 1 hướng qua màn rồi đi (Walk suốt, x 240→30) · ✅ nhóm 3 cách 32 tick (EnemyProbe) · ⚠ sinh **chỉ mép phải** theo video 31/31 ≠ wiki "2 phía" (H1 — §4) · ✅ St 3 rơi từ gờ trên (ghoul_pb) · ✅ chạm −2 St 1 · ✅ 1 đòn +100 | (V) |
| E02 Bat | ✅ dơi đỏ bay sóng về phía Simon (A 8.75 P 100, dx/dy 0.92/0.63 px) · ✅ dơi đen treo, sà khi \|dx\| < 64 (Perch→Swoop@56→Fly) · ❔ hất khỏi bệ nổi St 10 (chưa có map) · ⚠ dơi đen dùng sprite dơi đỏ (bộ tím chưa rõ — §5 #12) | đỏ (V), đen (W) |
| E03 Panther | ✅ nằm chờ (idle 240 tick Sit) · ✅ vào tầm ⇒ chạy 2 rồi nhảy khỏi gờ (Sit→Run@69→Jump@74) · ✅ đáp rồi quay về Simon 1 lần, chạy thẳng ra khỏi màn (video) · ✅ ≥ 3 con St 1 (Map 1) · ❌ báo #3 lệch quỹ đạo 7.7 px (fix-04) | (V) |
| E04 Raven | ✅ đậu chờ (idle) · ✅ cất cánh \|dx\| < 96 · ✅ khựng rồi bổ nhào · ✅ trượt thì quay lại lao tiếp (6 cú/240 tick) · ❔ "Holy Water không hiệu quả" (lời khuyên chiến thuật — không có luật riêng) | (W) |
| E05 Eagle | ✅ bay ngang mang Fleaman · ✅ thả gần Simon (\|dx\| ≤ 28) rồi bay đi · ✅ liên tục (EdgeSpawner) · ✅ hạ khi mang ⇒ cả hai chết (B46 eagle_whip) | (W) |
| E06 Fishman | ✅ nhảy khỏi nước, đáp, đi 0.75 (video dx 0–0.55) · ✅ bắn cầu lửa trên đất (Shoot@164) — **QUY ƯỚC** (video không có) · ❔ hất khỏi bệ nổi | (V) + quy ước |
| E07 Medusa Head | ✅ ra từ phía Simon nhìn; quay lưng ⇒ phía kia (B23 medusa_back) · ✅ bay ngang sóng xuyên địa hình · ❔ húc rơi hố (Arena không có hố trên đường bay) | (W) |
| E08 Skeleton | ✅ nhanh, nhảy nhiều · ✅ ném 2–3 xương vòng cung · ✅ giữ khoảng cách (Simon tiến ⇒ lùi tới x 364) · ❔ đứng gờ cao (chưa có map) | (W) |
| E09 Ghost | ✅ bám theo, vượt quá rồi vòng lại, xuyên địa hình · ✅ 3/2 đòn (roi L2 2 đòn) | (W) |
| E10 Red Skeleton | ✅ đi chậm đuổi · ✅ trúng ⇒ đổ 8 → đống xương 90 → ráp 8 → đi (không chết) · ⚠ +400 mỗi lần đổ (suy luận — §5 #45) | (W) |
| E11 Spear Knight | ✅ tuần tra tới lui, quay bất kỳ lúc nào · ✅ chỉ va chạm · ✅ 3 đòn roi da +400 | (W) |
| E12 Bone Tower | ✅ bất động, nháy tím↔đỏ rồi 2 cầu lửa/lượt · ✅ cầu lửa quất vỡ (+100 — suy luận) · ✅ 8/6 đòn | (W) |
| E13 Fleaman | ✅ chờ ⇒ ngắn-ngắn-vọt (HopS HopS HopL) · ✅ Eagle thả | (W) |
| E14 Bone Dragon | ✅ chân cổ bất động, cổ uốn · ✅ 2 cầu lửa nhắm Simon · ✅ chỉ hại khi lại gần/cầu lửa · ✅ rơi 5–8 vật, nước thánh ⇒ multiplier (B46) · ❔ gắn tường thật (Arena không có tường cao — rồng đặt lơ lửng) | (W) |
| E15 Axe Knight | ✅ giữ khoảng cách 64–112, lùi khi Simon tiến · ✅ rìu cao/thấp bay về như boomerang, quất vỡ · ✅ đẩy khỏi mép màn không quay lại (B46 axe_push) | (W) |
| Phantom Bat thường | ✅ ngủ → thức \|dx\| < 96 → bổ nhào/lượn như boss · ✅ cầu lửa khi lơ lửng (B46 pbat_wake — gallery 240 tick chưa tới pha bắn) · ✅ 1 đòn khựng 32 · ⚠ máu 3/2 + điểm 200 ƯỚC LƯỢNG (§5 #46), sprite cầu lửa TẠM (§5 #47) | (W) |
| Snake | ✅ Medusa thả, rơi rồi trườn 0.5 về phía Simon · ✅ roi ngồi 1 đòn · ⚠ 0 đ (§5 #51) | (W) |
| Igor | ✅ bất tử (12 đòn, máu không đổi) · ✅ ngồi vai → nhảy tiến giữa màn / nhảy cao ở bục góc qua lại · ✅ cầu lửa · ✅ quất ⇒ đứng hình 120 (B24) · ✅ Monster chết ⇒ biến mất | (W) |
| B1 Phantom Bat | ✅ treo trần → thức → lượn → bổ nhào → bay tới điểm ngẫu nhiên (video: bổ nhào 4/4) · ✅ 12/8, 3000 · ✅ Stopwatch tác dụng · ✅ khối chân cầu thang ⇒ Double Shot (gallery nhặt II +700) · ❌ lọ nước thánh chưa xuyên dơi (fix-01) · ❌ lao đầu lệch 4–6 px, thức trễ ở video boss (fix-04) | (V) |
| B2 Queen Medusa | ✅ vỡ ra từ tượng giữa phòng không báo trước (Statue→Emerge@59) · ✅ lao vào Simon bay thấp · ✅ thả rắn trườn về Simon · ✅ Stopwatch tác dụng · ✅ 8 đòn mạnh +3000 → lửa → cầu | (W) |
| B3 Mummy Men | ✅ 2 con lang thang kẹp 2 phía · ✅ ném băng (ngồi tránh) · ✅ Holy Water giữ chân · ✅ Pot Roast khối thấp nhất thang trái · ✅ Stopwatch vô dụng · ⚠ 48/32 = tổng 2 con (§5 #50) | (W) |
| B4 Frankenstein & Igor | ✅ Monster lang thang nửa phải · ✅ Igor như trên · ✅ 24/16 chỉ Monster, 5000 | (W) |
| B5 Death | ✅ bay ngẫu nhiên ưu tiên góc · ✅ ≤ 3 lưỡi hái đứng ~1 s rồi đuổi nhanh dần · ✅ lưỡi hái còn sau khi chết · ✅ rìu trúng cả Death lẫn lưỡi hái · ✅ 24/16, 7000 | (W) |
| B6 Dracula dạng 1 | ✅ dịch chuyển, hiện dần, mở áo bắn 3 cầu lửa, biến mất · ✅ chỉ đầu nhận đòn (gallery nhảy quất 0 trúng — đúng "khó"; B56 16 đòn) · ✅ cầu lửa phá đôi khi rơi tim · ✅ hết máu ⇒ đầu bay ra, thân thành quỷ | (W) |
| B6 Dracula dạng 2 | ✅ nhảy cao/thấp, thỉnh thoảng phun lửa · ✅ đánh mặt, 32 đòn mạnh · ✅ Holy Water choáng, không mất máu, đi xuyên · ✅ quả cầu cuối ⇒ `OnGameCleared` · ❌ credits/ending/Second Quest chưa làm (ngoài phạm vi cụm — §6) | (W) |

**Tổng hành vi:** 25/25 nhân vật (15 enemy wiki + dơi đen + Phantom Bat thường + Snake + Igor + 6 boss / Dracula 2 dạng) có state machine chạy đủ các pha trong mô tả wiki/video, ảnh đã xem, chuỗi state khớp mô tả. Block 1 (Ghoul, Bat đỏ, Panther, Fishman, Phantom Bat) **so video lại ra số y hệt routine 13/14/16** (mục 3). Block 2–6 = **theo wiki, gần đúng** (mọi tốc độ/nhịp ước lượng — không có video) — không phải lỗi, nhưng chưa thể gọi là "khớp bản gốc". Còn ❌: báo #3, Phantom Bat (lao đầu/lọ nước thánh), credits sau Dracula.

**Tất định của mọi nhân vật:** phòng trưng bày chạy lại ở **phiên Play mới, 30 fps, không chụp** ⇒ **75/75 hash từng replay trùng** lần 240 fps (chạy sau 4 lượt replay dài, khác thứ tự phiên) — hash bộ `517442ed` (`_cache/21/suites/gallery{,30}.txt`).

## 3. Hồi quy Map 1 — chạy lại routine 16 (mục 1–6, 9)
| # | Mục (kết luận 16) | Routine 21 | So 16 |
|---|---|---|---|
| 1 | Địa hình & pixel (ĐẠT) | `Map1AcceptProbe.StartTerrain` 28 ảnh (5 vùng × 4–9 cx) ↔ sheet `level1_entrance.png`: **0 / 45 056 px lệch mỗi ảnh** (`accept16.py terrain`); so ảnh 16: sân chơi trùng pixel, chỉ khác 4 chữ số TIME (thời điểm chụp) | **ĐẠT, không hồi quy** |
| 2 | Vật thể đặt đúng chỗ (ĐẠT) | `Map1Objects.json`/prefab Objects không đổi từ commit 15; dump prefab `_cache/21/spawnpoints21.tsv`: **71/71 dòng** (62 SpawnPoint + 7 bộ sinh/BossArena + 2 bí mật) trùng `_cache/16/spawnpoints.tsv` (vị trí, prefab, hướng, nội dung ConfigDrop); `CVGP/Build Map1 Objects` "ghi 0" | **ĐẠT, không hồi quy** |
| 3 | Simon chuyển động (CHƯA — lệch nhỏ) | SimonProbe 20/20 `b2954cbe`, AttackProbe 21/21 `eee2cdfb` (kỳ vọng VIDEO_*) — y hệt 16 ⇒ các lệch knockback 16 vs 14–15, nước thánh +2..4 tick, mép rơi ±3 px, rìu RMS 2.5 VẪN CÒN | **= 16 (CHƯA)** |
| 4 | Enemy & spawn (CHƯA — báo #3, rơi item) | EnemyProbe 20/20 `f288b0ce`; `enemy_compare` trên vết mới: Zombie 1.35/2.29/1.13, PB 3.16/2.01/1.84, báo #1 1.77/3.26, **#3 7.73/3.91**, dơi 0.92/0.63, Fish Man 0.55/3.21 · 0/4.23 — **từng số y hệt 16**; 17 cặp ảnh video ↔ Unity (`_cache/21/pairs13_all.png` — đã xem) | **= 16 (CHƯA)** |
| 5 | Boss Phantom Bat (CHƯA — nhỏ) | BossProbe 12/12 `31e6d711`; `boss_compare` `compare.txt` **trùng từng byte** routine 14 (lao đầu dx 5.68/3.98); 20 cặp ảnh (`shots/21/boss14/pairs_death_map1.png` — đã xem: lửa chết + quả cầu cùng chỗ) | **= 16 (CHƯA)** |
| 6 | HUD & luật (CHƯA — chỉ thiếu sheet) | RulesProbe 68/68 `03fafb24` (phiên mới); 8 ảnh HUD dải NES y 0–47 **trùng pixel** ảnh 16; `hud_pairs` vs video: nhãn/thanh 0, icon dao 8, chữ số TẠM 23–38 / 18–21 — như 16 (`shots/21/hud/hud_pairs.png` — đã xem); TIME 64 tick/đv ×90 trong luồng thật | **= 16** |
| 9 | Luồng, tất định, độ bền (ĐẠT) | Luồng THẬT Title → STAGE 04 ở **60 / 30 / 144 fps: `e0b2d54c` ×3**, 18 checkpoint trùng 16, mọi mốc L0/L352/L432/L448/L890/L922/…/L9370 trùng 16 (thêm 1 dòng log SFX tally lấy mẫu theo đồng hồ audio — không phải trạng thái game); H8 ảnh L10/L600 **0 px** so `shots/00/base_*` (L930 lệch 1 px Simon đang đi — chụp lệch 1 tick, đã biết §6 16); GameObject **379 ×3**; spam 4 lượt (CE 3600 · CA 3600 có Start/Select · PB 4800 · CB 4800 đặt tay 1 mạng như 16 ⇒ rơi nước → GAME OVER (171) → CONTINUE (204)) **0 lỗi console**, trạng thái cuối trùng 16; Step TB 0.028 ms, 0 B GC | **ĐẠT, không hồi quy** |

Hồi quy bộ khác cùng phiên: EnemyB23Probe 31/31 `171be0c1`, EnemyB46Probe 26/26 `58a8edf9`, BossB24Probe 16/16 `7adfbc54`, BossB56Probe 14/14 `6cb8a1ad`, ItemProbe 14/14 `7ca5bf56`, RegressionProbe long `5963c4ba`, CvgpCoreProbe (ArenaDummy) 21/21 `76516076` — **mọi hash = routine 20**.

## 4. Chất lượng tổng thể
| Kiểm | Kết quả |
|---|---|
| Tất định | replay dài 1260 tick `5963c4ba` ở **60 / 30 / 144 / 240 fps** (FirstDiff "trùng"); phòng trưng bày 75 replay 240 fps ≡ 30 fps; luồng thật 30/60/144 `e0b2d54c` |
| Hiệu năng (Arena đông nhất) | `RunCrowd(1800)`: phòng Death + 11 enemy (Spear Knight, Skeleton, Bone Tower, Axe Knight, Ghost, Raven, Fleaman, Red Skeleton, Bone Dragon, Phantom Bat thường, bộ sinh Medusa Head) + lưỡi hái + Simon ném nước thánh ×3 mỗi 10 tick: tối đa **23 thực thể sống** (10 vũ khí/đạn), **Step TB 0.080 ms, max 0.355 ms**; lặp seed khác 0.079 / 0.263 ms |
| GC / tick | **0 B** ở mọi lần đo (crowd 2×1800, long 4×1260, spam 4 lượt 24 012 Step) |
| Rò object | crowd lần 1: GameObject 195 → 199 (pool tạo thêm lúc đầu); lần 2: **199 → 199**, pool không tạo thêm (108); luồng thật 379 ×3; spam 379–380 (pool) |
| Console | 0 lỗi gameplay; chỉ lỗi nền đã biết (Odin PersistentContext path1, Unity.Mathematics, GUIClips, MCP WebSocket, warning framework) |
| Repo / H3 | submodule `Assets/TuyenAframework` con trỏ `4685d33` không đổi (nội dung ` m` từ trước); không file tạm trong `Assets/`; TODO/FIXME 0; log gameplay đều qua cờ debug; Update/Time.deltaTime chỉ ở NesClock/NesPad (chốt input) |
| SOURCES.md | 62/62 file `Art_Source/` khớp hash (25 sha256 trong SOURCES + 37 WAV md5 theo danh mục CVTI_MEMORY §3; zip 393996 sha256 khớp) |
| Sprite / địa hình | `verify_sprites.py --meta`: 246 PNG, 96 khung roi, 371/371 ô kiểm kê, **0 lỗi**; `build_tileset/build_collision/build_arena/build_boss_rooms.py` "same"; terrain 0 px; builder AnimSet/prefab/Map1 Objects chạy lại 0 thay đổi; `CVGP/Import Config` 12 bảng / 673 dòng "ghi lại 0" |

## 5. Xấp xỉ / quy ước còn lại (không phải lỗi, nhưng chưa là "bản gốc")
- **Block 2–6 (11 enemy + 5 boss + Snake + Igor + Phantom Bat thường): mọi tốc độ/nhịp/tầm kích hoạt là ước lượng** theo mô tả wiki (`ConfigActorParam` nguồn "suy luận: ước lượng (không video) — routine 17–20"); phòng boss thử ghép ô Block 1 (không phải phòng gốc); enemy bay đặt lơ lửng ở Arena (không có trần/tường thật cho dơi treo, quạ đậu, rồng gắn tường).
- Block 1 quy ước: Fish Man bắn lửa, bất tử boss 16 tick, đích lượn Phantom Bat ngẫu nhiên, biên vùng sinh Zombie, 4 nến "khả năng", Zombie chỉ sinh mép phải (video ≠ wiki).
- Chỉ số ước lượng: máu/điểm Phantom Bat thường (3/2, 200), điểm Snake 0, Red Skeleton +400 mỗi lần đổ, Dracula dạng 2 đòn yếu 48, Mummy 48/32 = tổng 2 con, +100 khi phá đạn.
- Sprite tạm/chưa rõ: dơi đen dùng sprite dơi đỏ, cầu lửa Phantom Bat thường mượn của Bone Dragon, `dracula2_spark` chưa dùng, HUD chữ số Famicom TẠM.
- Không có trong video ⇒ theo wiki/quy ước: Simon chết/trúng đòn ở Map 1, roi trên cầu thang, thập tự, đồng hồ.

## 6. Routine sửa đề xuất (KHÔNG tạo — người dùng duyệt; thứ tự khuyến nghị 02 → 01 → 03 → 04 → 05)
| Routine | Model · effort | Việc |
|---|---|---|
| `cvgp-fix-01-lo-nuoc-thanh-xuyen-dich` | Opus 5.5 · high | (giữ từ 16) lọ nước thánh bay XUYÊN enemy/boss, chỉ vỡ khi chạm sàn (video boss f342–352, f469–480); sửa `HolyWater` + AttackProbe/BossProbe, ghi lại hash |
| `cvgp-fix-02-nhap-nhac-va-gan-moc-map1` | Opus 5.5 · high | (giữ từ 16) nhập 16 MP3 → `Assets/Data/Audio/Music`, `eMusic` thay khoá chuỗi ConfigStage/ConfigBlock (24 ô nguồn nhạc "CHƯA đối chiếu"), gắn mốc đã đo (Prologue, Vampire Killer, Poison Mind, Stage Clear, Death, Game Over, Stalker), tắt kênh nhạc khi SFX nhặt; H8 + luồng thật |
| `cvgp-fix-03-simon-knockback-nuoc-thanh-mep-roi` | Opus 5.5 · high | (giữ từ 16) đo lại khớp mẫu: knockback cao 14–15, quỹ đạo lọ nước thánh, đầu dò mép rơi 2 phía; sửa Config + kỳ vọng probe; chạy lại mọi bộ |
| `cvgp-fix-04-quy-dao-bao-va-phantom-bat` | Opus 5.5 · xhigh | (giữ từ 16) báo #3 (cất cánh muộn, khựng khi đáp) + Phantom Bat (lao đầu lệch, thức trễ, cú lao 4) |
| `cvgp-fix-05-don-no-harness-probe` | Sonnet 5.5 · high | (mới) RulesProbe ↔ RegressionProbe phụ thuộc thứ tự phiên (StageRules để lại, `Simon.AutoRespawn` = false), `BossB24Probe.Cleanup` dùng `Destroy` (→ `DestroyImmediate`), gộp mọi probe vào 1 bộ chạy nối tiếp ghi file (như chuỗi `_cache/21/suites` của routine 21) để lần nghiệm thu sau chỉ cần 1 lệnh — không đổi gameplay, mọi hash giữ nguyên |

## 7. Đề xuất cụm tiếp theo (chỉ đề xuất — không tạo)
**Cụm CVM2 — "Map 2 = Block 2 (Stage 4–6) chơi được như video"**, cùng khuôn CVGP (sổ + plan + video + timeline + hiến pháp H1–H10):
| # | Routine đề xuất | Model · effort | Việc |
|---|---|---|---|
| 00 | `cvm2-00-ha-tang-va-baseline` | Sonnet 5.5 · medium | sổ CVM2_MEMORY, baseline hash CVGP (bộ chuỗi probe), kiểm đầu vào |
| 01 | `cvm2-01-nhap-sheet-level2-va-video` | Sonnet 5.5 · medium | nhập sheet "NES - Castlevania - Stages - Level 2" (người dùng tải tay — spriters-resource chặn Cloudflare) + video Stage 4–6 + boss Medusa; sha256, kiểm kê |
| 02 | `cvm2-02-hieu-chuan-video-timeline` | Opus 5.5 · high | hiệu chuẩn video → NES, timeline Stage 4–6 |
| 03 | `cvm2-03-tilemap-va-va-cham-block2` | Opus 5.5 · xhigh | tileset + Tilemap + lưới va chạm (hố, gai Stage 6, cầu thang, khối phá Crown/Treasure) |
| 04 | `cvm2-04-do-lai-enemy-block2-bang-video` | Opus 5.5 · xhigh | đổi số ƯỚC LƯỢNG 17 (Spear Knight, Medusa Head, Bone Tower, Ghost, dơi đen) sang số ĐO video; ghi lại kỳ vọng EnemyB23Probe |
| 05 | `cvm2-05-queen-medusa-theo-video` | Opus 5.5 · xhigh | đo Medusa + rắn + phòng thật (tượng giữa sảnh) thay phòng thử |
| 06 | `cvm2-06-lap-ghep-stage-4-6-va-chuyen-canh` | Opus 5.5 · xhigh | đặt vật thể/enemy theo video, cửa/đổi tầng, thay `Stage4Stub`, tally → bản đồ lâu đài → STAGE 07 stub |
| 07 | `cvm2-07-nghiem-thu-map2` | Opus 5.5 · xhigh | nghiệm thu như routine 16 + hồi quy Map 1 |

Điều kiện trước: người dùng duyệt `cvgp-fix-01..05` (ít nhất fix-02 nhạc), cung cấp sheet Level 2 + video Stage 4–6 (+ sheet HUD US nếu có). Cụm sau nữa theo cùng khuôn: Map 3–6 (Block 3–6) + **kết thúc game** (credits, ending, Second Quest Stage 19+ sát thương 4 vạch).
