# CHARACTER_STATS — Bảng chỉ số Castlevania (NES) của project

> **Tự sinh** bởi menu `CVGP/Export Stats Report` từ `Assets/Data/Config/Csv/*.csv` (CVGP 04) — đừng sửa tay: sửa CSV rồi chạy `CVGP/Import Config` + `CVGP/Export Stats Report`.
> Ô *nghiêng* = chưa có số: *chờ đo routine NN* (routine đó đo từ video), *không rõ* (wiki không nêu / không có video), *n/a* (có điều kiện). `—` = không áp dụng.
> Nhãn nguồn: `wiki:<Trang>#<Mục>` (StrategyWiki, bản Wayback — `ClaudeNotes/cvgp/wiki/CV_WIKI_DATA.md`), `video:Map_1 mm:ss` / `video:Boss_Map_1`, `video:planner` (số đo sơ bộ `ref/MAP1_TIMELINE.md` — routine ghi kèm sẽ đo lại), `đo:` (routine đo), `suy luận`, `quy ước`.

**Đếm:** 15 enemy thường + 3 thực thể phụ · 6 boss (7 dòng pha) · 22 item + 3 vật chứa · 5 vũ khí phụ · 3 cấp roi · 6 block / 18 stage.

## 1. Simon Belmont

| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |
|---|---|---|---|---|---|
| MaxHp | 16 |  | vạch | wiki:Gameplay#Damage (16 vạch); video:Map_1 (PLAYER 16 suốt video) |  |
| StartLives | 3 |  | mạng | suy luận: wiki ảnh HUD P-03; video:Map_1 P=03 suốt video; CVTI GameSession | wiki không nêu số mạng khởi đầu |
| StartHearts | 5 |  | tim | video:Map_1 0:02.2 (tim 05 → 10 sau Large Heart lúc 0:03.93); CVTI GameSession | khớp suy luận wiki (♥15 = 5 + 2×5) |
| HeartsOnNewBlock | 5 |  | tim | video:Map_1 3:52.3 (HUD STAGE 04 tim về 05 sau tally) — chờ đo routine 12 | tim đặt lại khi sang block mới |
| MaxHearts | *không rõ* | 76–? | tim | không rõ — wiki chỉ cho thấy ≥ 70 (ảnh Stage 3); video:Map_1 3:19 đạt 76; trần thật chưa biết | min = giá trị lớn nhất đã thấy (không phải trần) |
| WalkSpeed | 1.0 |  | px/tick | video:planner MAP1_TIMELINE §4 (11 đoạn; 0.987–1.028) — chờ đo routine 08 |  |
| JumpAirTicks | *chờ đo routine 08* | 38–40 | tick | video:planner (đồng hồ camera + 25 cú nhảy; trung vị 40) — chờ đo routine 08 |  |
| JumpHeight | *chờ đo routine 08* | 36–38 | px | video:planner (chân lên khỏi đất) — chờ đo routine 08 |  |
| JumpRiseSpeedAvg | 2.3 |  | px/tick | video:planner (≈ trung bình ~16 tick đầu) — chờ đo routine 08 | không phải parabol: lên ~16 tick → đỉnh phẳng ~10 tick → rơi |
| JumpRiseTicks | 16 |  | tick | video:planner (≈) — chờ đo routine 08 |  |
| JumpApexTicks | 10 |  | tick | video:planner (≈ đỉnh phẳng ±1 px) — chờ đo routine 08 |  |
| JumpFallSpeed | *chờ đo routine 08* | 2.5–3 | px/tick | video:planner — chờ đo routine 08 |  |
| JumpDistance | *chờ đo routine 08* | 39–43 | px | video:planner (trung vị 42; = 1 px/tick × thời gian bay) — chờ đo routine 08 |  |
| JumpAirControl | 0 |  | bool | video:planner (không đổi hướng giữa không trung ở mọi mẫu); wiki:Gameplay (hướng chốt lúc bấm A) | 0 = không điều khiển trên không |
| StairSpeedX | 0.5 |  | px/tick | video:planner (5 cửa sổ; 0.497–0.503) — chờ đo routine 08 |  |
| StairSpeedY | 0.5 |  | px/tick | video:planner (5 cửa sổ; 0.48–0.504) — chờ đo routine 08 |  |
| StairKnockOff | 0 |  | bool | wiki:Stages 16-18#Stage 17 (trên cầu thang không bao giờ bị đánh văng khỏi bậc) |  |
| KnockbackDx | *chờ đo routine 08* | 11–13 | px | video:planner (video boss f356–379; 1 mẫu) — chờ đo routine 08 | lùi ngược hướng mặt |
| KnockbackDy | 20 |  | px | video:planner (≈ nảy lên 20 px; 1 mẫu) — chờ đo routine 08 |  |
| KnockbackAirTicks | *chờ đo routine 08* | 24–30 | tick | video:planner (≈ 0.4–0.5 s; không chắc) — chờ đo routine 08 |  |
| InvincibleTicks | 92 |  | tick | video:planner (≈ 1.5 s nhấp nháy; video boss 12.67→14.20 s) — chờ đo routine 08 |  |
| WhipTotalTicks | *chờ đo routine 09* | 18–22 | tick | video:planner (trung vị ~20) — chờ đo routine 09 |  |
| WhipWindupTicks | *chờ đo routine 09* | 4–6 | tick | video:planner (khung 1 vung ra sau) — chờ đo routine 09 |  |
| WhipRaiseTicks | *chờ đo routine 09* | 4–8 | tick | video:planner (khung 2 giơ cao; điển hình 6) — chờ đo routine 09 |  |
| WhipExtendTicks | *chờ đo routine 09* | 6–10 | tick | video:planner (khung 3 duỗi thẳng) — chờ đo routine 09 |  |
| WhipHeightAboveFeet | 16 |  | px | video:planner (≈ đỉnh đầu + 9–10 px) — chờ đo routine 09 |  |
| WhipPickupFreezeTicks | *chờ đo routine 09* | 46–48 | tick | video:planner (0.73–0.80 s; f194–217 và f293–315) — chờ đo routine 09 | camera + Simon đứng; TIME vẫn chạy |
| HurtboxWidth | *chờ đo routine 07* |  | px | chờ đo routine 07 |  |
| HurtboxHeight | *chờ đo routine 07* |  | px | chờ đo routine 07 |  |
| CrouchHurtboxHeight | *chờ đo routine 07* |  | px | chờ đo routine 07 |  |

### Sát thương Simon nhận mỗi lần trúng đòn (theo STAGE, mọi loại đòn như nhau — wiki Gameplay#Damage)

| Block | Stage | Vạch mất / đòn | Số đòn chết (16 vạch) | Giờ giới hạn | Boss | Nguồn |
|---|---|---|---|---|---|---|
| 1 | 1, 2, 3 | 2 | 8 | 300 | PhantomBat | wiki:Gameplay#Damage (Stage 1–6 = 2 vạch); giờ: video:Map_1 0:02.2 TIME 0300 (= CVTI GameSession); đồng hồ KHÔNG đặt lại giữa Stage 1→2→3 (1:31.87 STAGE 02 TIME 0226; 2:50.87 STAGE 03 TIME 0166) |
| 2 | 4, 5, 6 | 2 | 8 | 400 | QueenMedusa | wiki:Gameplay#Damage (Stage 1–6 = 2 vạch); giờ: video:Map_1 3:59.2 STAGE 04 TIME 0400 |
| 3 | 7, 8, 9 | 3 | 6 | *không rõ* | MummyMan | wiki:Gameplay#Damage (Stage 7–12 = 3 vạch); wiki:Stages 7-9; giờ: không rõ — không có video (wiki không nêu giờ) |
| 4 | 10, 11, 12 | 3 | 6 | *không rõ* | Frankenstein | wiki:Gameplay#Damage (Stage 7–12 = 3 vạch); giờ: không rõ — không có video (wiki không nêu giờ) |
| 5 | 13, 14, 15 | 4 | 4 | *không rõ* | Death | wiki:Gameplay#Damage (Stage 13–18 = 4 vạch); wiki:Stages 13-15; giờ: không rõ — không có video (wiki không nêu giờ) |
| 6 | 16, 17, 18 | 4 | 4 | *không rõ* | Dracula | wiki:Gameplay#Damage (Stage 13–18 = 4 vạch); giờ: không rõ — không có video (wiki không nêu giờ) |

Rơi hố/nước/gai = chết ngay; Second Quest (Stage 19+) mọi đòn 4 vạch (ConfigRule).

## 2. Enemy (15 loại thường + thực thể phụ)

Máu = **số đòn** để hạ: *yếu* (roi da, Holy Water, Dagger) / *mạnh* (roi xích ngắn/dài, Cross, Axe). Sát thương lên Simon: bảng §1 (theo stage).

| Id (wiki) | Tên khác | Nhóm sheet | Loại | Stage | Đòn yếu | Đòn mạnh | Điểm | Chết được | Đạn | Tốc độ (px/tick) | Hành vi | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Ghoul | Zombie (lost souls) | enemy_zombie | Normal | 1, 3 | 1 | 1 | 100 | có | — | 0.875 | Đi thẳng một hướng qua màn rồi rời đi; nhóm 2–3 con từ cả hai phía (Stage 3 rơi từ trên xuống) | wiki:Enemies#Ghouls · *moveSpeed:* video:planner MAP1_TIMELINE §4 (4 vết f600–759; dư ≤ 3.3 px) — chờ đo routine 13 |
| Bat | Black Bat / Red Bat | enemy_bat (Vampire Bat) | Normal | 2, 4, 10 | 1 | 1 | 200 | có | — | *chờ đo routine 13* | Dơi đen treo trần rồi lao xuống khi Simon lại gần; dơi đỏ bay sóng lên xuống | wiki:Enemies#Bats · *moveSpeed:* video:planner 1.0–1.12 px/tick về phía Simon (thô) — chờ đo routine 13 |
| Panther | Black Leopard | enemy_leopard (Black Leopard) | Normal | 1 | 1 | 1 | 200 | có | — | 2.0 | Nằm chờ tới khi Simon vào tầm rồi nhảy vọt; rơi xuống tầng thấp và luôn quay về phía Simon | wiki:Enemies#Panthers · *moveSpeed:* video:planner chạy 2.0 px/tick (f1266–1856) — chờ đo routine 13 |
| Raven |  | enemy_raven | Normal | 7, 8, 9 | 1 | 1 | 200 | có | — | *không rõ* | Đậu chờ; cất cánh khi Simon lại gần; khựng một chút rồi bổ nhào; trượt thì quay lại đánh từ sau | wiki:Enemies#Ravens · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| Eagle | giant Eagle | enemy_eagle | Normal | 11, 17 | 1 | 1 | 300 | có | — | *không rõ* | Bay ngang mang Fleaman và thả xuống gần Simon; giết khi còn mang thì cả hai cùng chết | wiki:Enemies#Eagles · *moveSpeed:* không rõ — không có video (routine 18 ước lượng) |
| Fishman | Fish Man | enemy_fishman (Fish Man) | Normal | 2, 10 | 1 | 1 | 300 | có | Cầu lửa (chỉ khi đã lên đất; video Map_1 không thấy bắn) | *chờ đo routine 13* | Nhảy lên từ mặt nước; chạm đất thì đi bộ và bắn cầu lửa | wiki:Enemies#Fishmen · *moveSpeed:* video:planner đi ~0.8 px/tick (nhiễu) — chờ đo routine 13 |
| MedusaHead | gorgon heads | enemy_medusahead | Normal | 5, 6, 8, 9, 15 | 1 | 1 | 300 | có | — | *không rõ* | Xuất hiện từ phía Simon đang nhìn; bay ngang theo đường sóng; quay lưng thì tới từ cả hai phía | wiki:Enemies#Medusa Heads · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| Skeleton | White Skeleton | enemy_skeleton (White Skeleton) | Normal | 7, 8, 9, 13, 17 | 1 | 1 | 300 | có | Xương ném vòng cung (2–3 cái cùng lúc) | *không rõ* | Nhanh và nhảy nhiều; ném xương theo vòng cung với các quỹ đạo khác nhau | wiki:Enemies#Skeletons · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| Ghost | souls of lost men | enemy_ghost | Normal | 6, 7, 15 | 3 | 2 | 300 | có | — | *không rõ* | Xuất hiện ở điểm cố định; chậm nhưng bám theo vị trí Simon từ góc lạ | wiki:Enemies#Ghosts · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| RedSkeleton | enchanted skeleton | enemy_redskeleton | Normal | 13, 14, 15 | 1 | 1 | 400 | không | — | *không rõ* | Đi chậm đuổi theo; bị đánh thì đổ thành đống xương rồi đứng dậy sau 1–2 s (không giết được) | wiki:Enemies#Red Skeletons · *moveSpeed:* không rõ — không có video (routine 18 ước lượng) |
| SpearKnight | Black Knight | enemy_blackknight (Black Knight) | Normal | 4, 5 | 3 | 2 | 400 | có | — | *không rõ* | Đi tới lui chậm; có thể quay đầu bất cứ lúc nào; chỉ gây hại khi va chạm | wiki:Enemies#Spear Knights · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| BoneTower | Dragon Skull Cannon | enemy_dragonskull (Dragon Skull Cannon) | Normal | 6, 8, 9, 15 | 8 | 6 | 400 | có | 2 cầu lửa mỗi lượt (quất vỡ được) | — | Bất động; nhấp nháy rồi phun 2 cầu lửa theo chu kỳ | wiki:Enemies#Bone Towers |
| Fleaman | Hunchback | enemy_fleaman (Hunchback/Fleaman) | Normal | 7, 11, 13, 14, 17 | 1 | 1 | 500 | có | — | *không rõ* | Nhảy như ngẫu nhiên: thường 2 bước nhảy ngắn rồi 1 cú nhảy vọt; Stage 11/17 do Eagle thả | wiki:Enemies#Fleamen · *moveSpeed:* không rõ — không có video (routine 17 ước lượng) |
| BoneDragon | Skele-Dragon | enemy_skeledragon (Skele-Dragon) | Normal | 11, 12 | 8 | 7 | 1000 | có | Cầu lửa từ miệng theo nhiều góc | — | Gắn vào tường; thân bất động; cổ ngọ nguậy; phun cầu lửa (cả game chỉ 3 con) | wiki:Enemies#Bone Dragons · *hits:* wiki:Enemies#Bone Dragons (8/7 không đúng tỉ lệ 1.5× — giữ nguyên số wiki; CV_WIKI_DATA §7.1 #6) |
| AxeKnight | Axe-Man | enemy_axeman (Axe-Man) | Normal | 14, 15 | 10 | 9 | 1400 | có | Rìu ném cao hoặc thấp rồi bay về như Boomerang (quất vỡ được) | *không rõ* | Giữ khoảng cách; lùi khi Simon tiến; bị đẩy khỏi mép màn thì không quay lại | wiki:Enemies#Axe Knights · *hits:* wiki:Enemies#Axe Knights (10/9 không đúng tỉ lệ 1.5× — giữ nguyên số wiki; CV_WIKI_DATA §7.1 #6) · *moveSpeed:* không rõ — không có video (routine 18 ước lượng) |
| PhantomBatMinor |  | boss_phantombat | Sub | 16 | *không rõ* | *không rõ* | *không rõ* | có | Cầu lửa | *không rõ* | 5 con ở Stage 16; bay tới theo vị trí khởi đầu và phun cầu lửa; nên tránh hơn là đánh | wiki:Bosses#Phantom Bat; wiki:Stages 16-18#Stage 16 · *hits:* không rõ — wiki không nêu số đòn bản thường · *score:* không rõ — wiki không nêu điểm · *moveSpeed:* không rõ — không có video (routine 18 ước lượng) |
| Igor |  | boss_frankenstein (ô Igor) | Sub | 12 | — | — | — | không | Cầu lửa | *không rõ* | Bất tử; nhảy qua lại hai bên phòng và ném cầu lửa; quất 1 lần ⇒ choáng | wiki:Bosses#Frankenstein & Igor; wiki:Stages 10-12#Stage 12 · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |
| Snake |  | boss_medusa (ô rắn) | Sub | 6 | 1 | 1 | *không rõ* | có | — | *không rõ* | Do Queen Medusa thả xuống; bò trên sàn về phía Simon | wiki:Bosses#Queen Medusa · *hits:* wiki:Bosses#Queen Medusa (chết bằng 1 đòn roi; đòn mạnh = 1 là suy luận) · *score:* không rõ — wiki không nêu điểm · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |

## 3. Boss

| Boss | Pha | Stage | Số thực thể | Đòn yếu | Đòn mạnh | Đòn tính cho | Điểm | Stopwatch tác dụng | Điểm yếu | Đòn tấn công | Item trong phòng | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| PhantomBat | 1 | 3 | 1 | 12 | 8 | Single | 3000 | có | toàn thân | Bay tới điểm ngẫu nhiên; thỉnh thoảng bổ nhào về phía Simon; video: không bắn đạn | Double Shot (khối phải nhất ở chân cầu thang) | wiki:Bosses#Phantom Bat · *hits:* wiki:Bosses#Phantom Bat; video:Map_1 3:19–3:30 rìu −2 vạch/lần ⇒ 8 đòn mạnh hết 16 vạch (khớp) · *score:* wiki:Bosses#Phantom Bat; video:Map_1 3:30.17 +3000 (khớp) · *moveSpeed:* chờ đo routine 14 (nhiều pha tốc độ — xem ConfigActorParam Boss.PhantomBat) |
| QueenMedusa | 1 | 6 | 1 | 12 | 8 | Single | 3000 | có | toàn thân | Lao thẳng vào Simon (bay thấp); thả rắn bò trên sàn về phía Simon | Không | wiki:Bosses#Queen Medusa · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |
| MummyMan | 1 | 9 | 2 | 48 | 32 | TotalAllEntities | 3000 | không | toàn thân | Lang thang trong phòng; thỉnh thoảng ném băng gạc; kẹp Simon từ hai phía | Pot Roast (khối thấp nhất cầu thang trái) | wiki:Bosses#Mummy Men · *hits:* wiki:Bosses#Mummy Men (48/32 không nói tổng hay mỗi con — chọn TỔNG 2 con theo suy luận 48 = 2 × 24; §4) · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |
| Frankenstein | 1 | 12 | 2 | 24 | 16 | MainEntityOnly | 5000 | không | toàn thân (Monster) | Monster đi lang thang (thường nửa phải); Igor nhảy qua lại và ném cầu lửa | Không (Dagger ở ngoài phòng) | wiki:Bosses#Frankenstein & Igor · *hits:* wiki:Bosses#Frankenstein & Igor (chỉ Monster; Igor bất tử ⇒ ConfigEnemy.Igor) · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |
| Death | 1 | 15 | 1 | 24 | 16 | Single | 7000 | không | toàn thân | Lơ lửng ngẫu nhiên (ưa các góc); tạo tối đa 3 lưỡi hái đứng ~1 s rồi đuổi Simon và nhanh dần | Không | wiki:Bosses#Death · *moveSpeed:* không rõ — không có video (routine 20 ước lượng) |
| Dracula | 1 | 18 | 1 | 24 | 16 | Single | *không rõ* | không | chỉ đầu | Dịch chuyển tới chỗ ngẫu nhiên; mở áo choàng bắn 3 cầu lửa rồi biến mất | Holy Water (nến trái cùng) | wiki:Bosses#The Count · *hits:* wiki:Bosses#The Count (bảng không nói dạng nào — gán dạng 1; §4) · *score:* không rõ — wiki không nêu điểm riêng dạng 1 · *moveSpeed:* không rõ — không có video (routine 20 ước lượng) |
| Dracula | 2 | 18 | 1 | *không rõ* | 32 | Single | 50000 | không | chỉ đầu (chân không nhận sát thương) | Nhảy quanh phòng (cao/thấp); thỉnh thoảng phun lửa; Holy Water làm choáng nhưng không gây sát thương | — | wiki:Bosses#The Count; wiki:Stages 16-18#Stage 18 · *hits:* wiki:Stages 16-18#Stage 18 ('32 lần' vào đầu; vũ khí không nêu — gán đòn mạnh là suy luận); đòn yếu không rõ (gợi ý tỉ lệ 3:2 ⇒ 48 — routine 20 chốt) · *score:* wiki:Bosses#The Count — mâu thuẫn wiki:Gameplay#Points (Dracula không cho điểm); chọn Bosses (§4) · *moveSpeed:* không rõ — không có video (routine 20 ước lượng) |

## 4. Tham số hành vi đo được (enemy / boss / vũ khí / item)

| Thực thể | Tham số | Giá trị | Min | Max | Đơn vị | Ghi chú | Nguồn |
|---|---|---|---|---|---|---|---|
| Enemy.Ghoul | groupSize | *chờ đo routine 13* | 2 | 3 | con |  | wiki:Enemies#Ghouls (nhóm 2–3) — chờ đo routine 13 |
| Enemy.Bat | bobAmplitudePx | *chờ đo routine 13* | 6 | 9 | px |  | video:planner (dao động dọc; thô) — chờ đo routine 13 |
| Enemy.Bat | bobPeriodTicks | *chờ đo routine 13* | 78 | 120 | tick |  | video:planner (~1.3–2 s; thô) — chờ đo routine 13 |
| Enemy.Panther | jumpVx | *chờ đo routine 13* | 1.9 | 2.0 | px/tick |  | video:planner (nhảy ngang + cung rơi) — chờ đo routine 13 |
| Enemy.Fishman | jumpVy0 | -5.6 |  |  | px/tick | âm = đi lên (NES y xuống) | video:planner (f3515–3547 khớp parabol; dư ≤ 1.6 px) — chờ đo routine 13 |
| Enemy.Fishman | gravity | 0.124 |  |  | px/tick² |  | video:planner — chờ đo routine 13 |
| Enemy.Fishman | jumpApexPx | 129 |  |  | px |  | video:planner (đỉnh trên mặt nước) — chờ đo routine 13 |
| Enemy.Fishman | jumpApexTicks | 46 |  |  | tick |  | video:planner — chờ đo routine 13 |
| Enemy.RedSkeleton | reviveTicks | *chờ đo routine 18* | 60 | 120 | tick |  | wiki:Enemies#Red Skeletons; wiki:Stages 13-15 (đứng dậy sau 1–2 s) — chờ đo routine 18 |
| Boss.PhantomBat | sleepScreenX | 110.5 |  |  | px màn |  | video:planner MAP1_TIMELINE §5 (tâm sprite khi ngủ; world PB x ≈ 622) — chờ đo routine 14 |
| Boss.PhantomBat | sleepScreenY | 64.5 |  |  | px màn |  | video:planner — chờ đo routine 14 |
| Boss.PhantomBat | wakeSimonScreenX | *chờ đo routine 14* | 181 | 184 | px màn |  | video:planner (2 lần; Simon trên cầu thang phải) — chờ đo routine 14 |
| Boss.PhantomBat | wakeHoverTicks | 30 |  |  | tick |  | video:planner (≈ 0.5 s lượn tại chỗ) — chờ đo routine 14 |
| Boss.PhantomBat | firstDiveSpeed | *chờ đo routine 14* | 1.44 | 1.5 | px/tick | hướng ≈ +31° | video:planner (2 video) — chờ đo routine 14 |
| Boss.PhantomBat | firstDiveAngleDeg | 31 |  |  | độ |  | video:planner (2 video giống nhau) — chờ đo routine 14 |
| Boss.PhantomBat | slowFlySpeed | *chờ đo routine 14* | 0.75 | 1.0 | px/tick |  | video:planner — chờ đo routine 14 |
| Boss.PhantomBat | hoverTicks | *chờ đo routine 14* | 30 | 96 | tick |  | video:planner (0.5–1.6 s) — chờ đo routine 14 |
| Boss.PhantomBat | diveSpeed | *chờ đo routine 14* | 1.5 | 1.6 | px/tick | góc 27–40° | video:planner (có lần 2.46) — chờ đo routine 14 |
| Boss.PhantomBat | retreatSpeed | *chờ đo routine 14* | 2.0 | 2.3 | px/tick |  | video:planner — chờ đo routine 14 |
| Boss.PhantomBat | diveCycleTicks | *chờ đo routine 14* | 240 | 258 | tick |  | video:planner (4.0–4.3 s) — chờ đo routine 14 |
| Boss.PhantomBat | deathFlameTicks | *chờ đo routine 14* | 42 | 48 | tick |  | video:planner (0.7–0.8 s) — chờ đo routine 14 |
| Boss.Death | maxScythes | 3 |  |  | cái |  | wiki:Bosses#Death |
| Boss.Death | scytheIdleTicks | 60 |  |  | tick |  | wiki:Bosses#Death (đứng yên khoảng 1 giây × 60) |
| Boss.Dracula | fireballsPerVolley | 3 |  |  | cái |  | wiki:Bosses#The Count |
| SubWeapon.Dagger | spawnHeightAboveFeet | 21 |  |  | px |  | video:planner (≈ đỉnh đầu + 4–5 px) — chờ đo routine 09 |
| SubWeapon.Axe | apexPx | 55 |  |  | px |  | video:planner (≈ sau ~21 tick) — chờ đo routine 09 |
| SubWeapon.Axe | apexTicks | 21 |  |  | tick |  | video:planner (≈) — chờ đo routine 09 |
| SubWeapon.HolyWater | flameHoldTicks | 144 |  |  | tick |  | video:planner (≈ 2.4 s lửa giữ chân boss) — chờ đo routine 09 |
| Item.MagicCrystal | spawnDelayTicks | 150 |  |  | tick |  | video:planner (2.5 s sau khi boss chết; 2 video) — chờ đo routine 14 |
| Item.MagicCrystal | spawnScreenX | 127.5 |  |  | px màn |  | video:planner (2 video) — chờ đo routine 14 |
| Item.MagicCrystal | spawnScreenY | 128.5 |  |  | px màn |  | video:planner (2 video) — chờ đo routine 14 |
| Item.MagicCrystal | gravity | *chờ đo routine 14* | 0.23 | 0.26 | px/tick² |  | video:planner — chờ đo routine 14 |

## 5. Item & vật chứa

| Item | Tên khác | Loại | +Tim | Điểm | +Vạch máu | Hồi đầy | Hiệu lực (tick) | Tồn tại (tick) | Kiểu rơi | Hiệu ứng | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|
| SmallHeart | Heart | Heart | 1 | 0 | — | không | — | *chờ đo routine 11* | rơi chậm đung đưa | +1 tim (đạn vũ khí phụ) | wiki:Items#Small Heart · *score:* wiki:Gameplay#Points (nhặt tim giữa stage không cho điểm) · *lifetimeTicks:* chờ đo routine 11 |
| LargeHeart | Heart (lớn) | Heart | 5 | 0 | — | không | — | *chờ đo routine 11* | rơi nhanh thẳng xuống sàn | +5 tim | wiki:Items#Large Heart; video:Map_1 0:03.93 (tim 05→10) · *score:* wiki:Gameplay#Points (nhặt tim giữa stage không cho điểm) · *lifetimeTicks:* chờ đo routine 11 |
| RedMoneyBag | Moneybag (đỏ) | Score | — | 100 | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | +100 điểm | wiki:Items#Red Money Bag · *lifetimeTicks:* chờ đo routine 11 |
| PurpleMoneyBag | Moneybag (tím) | Score | — | 400 | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | +400 điểm | wiki:Items#Purple Money Bag · *score:* wiki:Items#Purple Money Bag; video:Map_1 0:21.83 (+400 khớp) · *lifetimeTicks:* chờ đo routine 11 |
| WhiteMoneyBag | Moneybag (trắng) | Score | — | 700 | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | +700 điểm | wiki:Items#White Money Bag · *score:* wiki:Items#White Money Bag; video:Map_1 (5 lần +700 khớp; CVGP_MEMORY §3 SFX 23) · *lifetimeTicks:* chờ đo routine 11 |
| FlashingMoneyBag | Bonus / hidden money bag | Score | — | 1000 | — | không | — | *chờ đo routine 11* | hiện tại chỗ bí mật | +1000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *score:* wiki:Items#Bonuses; video:Map_1 2:23.80 (+1000 khớp) · *lifetimeTicks:* chờ đo routine 11 (wiki Stage 7: có thể biến mất sau thời gian ngắn) |
| Crown | Bonus | Score | — | 2000 | — | không | — | *không rõ* | hiện tại chỗ bí mật (Stage 4) | +2000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *lifetimeTicks:* không rõ — không có video |
| TreasureChest | Treasure Box | Score | — | 2000 | — | không | — | *không rõ* | hiện tại chỗ bí mật (Stage 5) | +2000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *lifetimeTicks:* không rõ — không có video |
| Moai |  | Score | — | 4000 | — | không | — | *không rõ* | chỉ Second Quest (Stage 24/26/27) | +4000 điểm | wiki:Secrets#Bonuses · *lifetimeTicks:* không rõ — không có video |
| MorningStar | Whip upgrade | Whip | — | 0 | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Roi lên 1 cấp (tối đa cấp 3); chết ⇒ về cấp 1; nhặt ⇒ cả thế giới đóng băng (ConfigSimon.WhipPickupFreezeTicks) | wiki:Items#Morning Star · *score:* wiki:Stages 1-3 ảnh HUD (SCORE 000000 sau 2 Morning Star + Dagger) · *lifetimeTicks:* chờ đo routine 11 |
| Dagger |  | SubWeapon | — | 0 | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Vũ khí phụ Dagger (ConfigSubWeapon) | wiki:Items#Dagger · *score:* wiki:Stages 1-3 ảnh HUD (SCORE 000000 khi đã có Dagger) · *lifetimeTicks:* chờ đo routine 11 |
| Axe | Throwing Axe | SubWeapon | — | *chờ đo routine 11* | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Vũ khí phụ Axe (ConfigSubWeapon) | wiki:Items#Axe · *score:* chờ đo routine 11 (video Map_1 3:10.67 nhặt rìu) · *lifetimeTicks:* chờ đo routine 11 (wiki Stage 7: rơi mất nếu không nhặt ngay) |
| HolyWater | Fire Bomb | SubWeapon | — | *chờ đo routine 11* | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Vũ khí phụ Holy Water (ConfigSubWeapon) | wiki:Items#Holy Water · *score:* chờ đo routine 11 (không nhặt trong video) · *lifetimeTicks:* chờ đo routine 11 |
| Cross | Boomerang | SubWeapon | — | *không rõ* | — | không | — | *không rõ* | chờ đo routine 11 | Vũ khí phụ Cross (ConfigSubWeapon) | wiki:Items#Cross · *score:* không rõ — không có trong video · *lifetimeTicks:* không rõ — không có trong video |
| Stopwatch | Stop Watch | SubWeapon | — | *chờ đo routine 11* | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Vũ khí phụ Stopwatch (ConfigSubWeapon) | wiki:Items#Stopwatch · *score:* chờ đo routine 11 (không nhặt trong video) · *lifetimeTicks:* chờ đo routine 11 |
| DoubleShot | Shot Multiplier ×2 | Multiplier | — | *n/a* | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | 2 vũ khí phụ cùng lúc trên màn; mất khi nhặt vũ khí phụ khác; rơi khi trúng enemy 10 lần bằng cùng vũ khí phụ | wiki:Items#Double Shot · *score:* n/a: điểm có điều kiện ⇒ ConfigScore.MultiplierPickupBonus · *lifetimeTicks:* chờ đo routine 11 |
| TripleShot | Shot Multiplier ×3 | Multiplier | — | *n/a* | — | không | — | *không rõ* | chờ đo routine 11 | 3 vũ khí phụ cùng lúc trên màn; mất khi nhặt vũ khí phụ khác | wiki:Items#Triple Shot · *score:* n/a: điểm có điều kiện ⇒ ConfigScore.MultiplierPickupBonus · *lifetimeTicks:* không rõ — không có trong video |
| Rosary | Cross (bảng Items) | Special | — | *chờ đo routine 11* | — | không | — | *chờ đo routine 11* | chờ đo routine 11 | Diệt mọi enemy trên màn (trừ boss; không cho điểm); phá đạn của boss; màn nháy trắng (ConfigRule.RosaryFlashTicks) | wiki:Items#Rosary · *score:* chờ đo routine 11 (video Map_1 1:15.02/1:24.69/2:42.76) · *lifetimeTicks:* chờ đo routine 11 |
| InvisibilityPotion |  | Special | — | *chờ đo routine 11* | — | không | *chờ đo routine 11* | *chờ đo routine 11* | chờ đo routine 11 | Tàng hình (lấp lánh): đi xuyên enemy không bị thương; vẫn chết khi rơi hố | wiki:Items#Invisibility Potion · *score:* chờ đo routine 11 (video Map_1 2:58.20) · *effectTicks:* chờ đo routine 11 — wiki ≈ 5 s (300 tick) nhưng video Map_1: Simon trong suốt ≈ 2.6 s (planner) và SFX 27→28 cách 3.9 s (2:58.20→3:02.11) — ngắn hơn wiki (H1: theo video) · *lifetimeTicks:* chờ đo routine 11 |
| PotRoast | Pork Chop | Heal | — | *chờ đo routine 11* | 6 | không | — | *chờ đo routine 11* | chờ đo routine 11 | Hồi 6 vạch máu; chỉ có trong khối gạch phá bằng roi | wiki:Items#Pork Chop · *score:* chờ đo routine 11 (video Map_1 1:39 nhặt sau khi đập tường) · *lifetimeTicks:* chờ đo routine 11 |
| OneUp |  | Life | — | *không rõ* | — | không | — | *không rõ* | chờ đo routine 11 | +1 mạng; chỉ hiện khi kích hoạt bí mật | wiki:Items#1up; wiki:Secrets · *score:* không rõ — không có trong video · *lifetimeTicks:* không rõ (wiki Stage 25: biến mất sau chốc lát) |
| MagicCrystal | Crystal Ball | Crystal | — | — | — | có | — | — | rơi theo trọng lực (ConfigActorParam Item.MagicCrystal) | Hồi đầy máu + tính tally (ConfigScore); kết thúc block | wiki:Items#Magic Crystal; wiki:Walkthrough; video:Boss_Map_1 20.33–21.80 s |
| Torch | chậu lửa (đuốc) | Container | — | — | — | không | — | — | — | Vật chứa phá bằng roi (Stage 1 ngoài trời: 5 cái); nội dung ở ConfigDrop | wiki:Stages 1-3#Stage 1 (ảnh bản đồ); video:Map_1 0:02–0:16 |
| WallCandle | nến tường | Container | — | — | — | không | — | — | — | Vật chứa phá bằng roi/vũ khí phụ; nội dung ở ConfigDrop | wiki:Items; wiki:Stages 1-3 |
| BreakableBlock | khối gạch phá được | Container | — | — | — | không | — | — | — | Khối/tường gạch phá bằng roi giấu Pot Roast / multiplier / túi tiền; nội dung ở ConfigDrop | wiki:Items#Pork Chop; wiki:Secrets; video:Map_1 1:39.61 |

## 6. Vũ khí phụ

| Vũ khí | Giá tim | Nhóm | Xuyên mục tiêu | Quay về | Đóng băng (tick) | vx (px/tick) | vy0 (px/tick) | g (px/tick²) | Đặc tính | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|
| Dagger | 1 | Weak | 0 | không | — | 4.0 | — | — | Bay nhanh thẳng về phía trước; sức ≈ roi da | wiki:Items#Dagger; wiki:Enemies (nhóm yếu) · *pierces:* wiki:Items#Dagger (chỉ trúng 1 mục tiêu) · *motion:* video:planner 4.0 px/tick bay thẳng (f2797–2804; f3214–3219) — chờ đo routine 09 |
| Axe | 1 | Strong | *chờ đo routine 09* | không | — | *chờ đo routine 09* | -5.1 | *chờ đo routine 09* | Bay vòng cung lên cao rồi rơi xuống sàn; tốt với mục tiêu trên cao/bay | wiki:Items#Axe; wiki:Enemies (nhóm mạnh) · *pierces:* chờ đo routine 09 (wiki không nêu xuyên mục tiêu) · *motion:* video:planner vx 2.0–2.2; vy0 −5.1; g 0.24–0.25 px/tick² (2 quỹ đạo) — chờ đo routine 09 |
| HolyWater | 1 | Weak | *chờ đo routine 09* | không | — | *chờ đo routine 09* | *chờ đo routine 09* | *chờ đo routine 09* | Lọ vỡ khi chạm sàn thành ngọn lửa nhỏ giữ chân/đốt enemy | wiki:Items#Holy Water; wiki:Enemies (nhóm yếu) · *pierces:* chờ đo routine 09 (lửa trên sàn — số mục tiêu wiki không nêu) · *motion:* video:planner (video boss) vx 1.4–1.6; cung thấp lên 5–10 px; g 0.12–0.13; chạm đất cách 40–44 px sau 26–28 tick — chờ đo routine 09 |
| Cross | 1 | Strong | 1 | có | — | *không rõ* | — | *không rõ* | Bay ra rồi quay về người ném; trúng nhiều mục tiêu cả lượt đi và về | wiki:Items#Cross; wiki:Enemies (nhóm mạnh) · *pierces:* wiki:Items#Cross (trúng nhiều mục tiêu; trúng cả lượt về) · *motion:* không rõ — Cross không có trong video (routine 09 ước lượng) |
| Stopwatch | 5 | None | — | không | 300 | — | — | — | Đóng băng enemy và gai di động; hầu hết boss miễn nhiễm (trừ Phantom Bat và Queen Medusa); không bị multiplier ảnh hưởng | wiki:Items#Stopwatch; wiki:Gameplay · *freezeTicks:* wiki:Items#Stopwatch (5 giây × 60) |

## 7. Roi (Vampire Killer)

| Cấp | Tên | Nhóm | Sát thương so roi da | Dài mảnh roi sheet (px) | Tầm tay→đầu roi (px) | Tim cần (wiki) | Số màu | Nguồn |
|---|---|---|---|---|---|---|---|---|
| 1 | Leather Whip | Weak | 1 | 24 | 22 | — | 1 | wiki:Items#Morning Star; wiki:Enemies (nhóm yếu) · *damage:* quy ước: mốc so sánh 1× · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L1 24×16) — chờ 05 chốt hitbox · *reachPx:* video:planner tay→đầu roi ≈ 22 px (f178–180; ±2 px) — chờ đo routine 09 |
| 2 | Chain Whip (ngắn) | Strong | 1.5 | 24 | *chờ đo routine 09* | 4 | 1 | wiki:Items#Morning Star; wiki:Enemies (nhóm mạnh) · *damage:* wiki:Items#Morning Star (gấp 1.5 lần roi da) · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L2 24×16) — chờ 05 chốt hitbox · *reachPx:* video:planner 24–25 px (f270–272) — chờ đo routine 09 · *upgradeHearts:* wiki:Items#Morning Star (đủ 4 tim cho lần nâng 1 — cơ chế không giải thích; §7.1 #5) |
| 3 | Chain Whip (dài) | Strong | *không rõ* | 40 | 43 | 8 | 4 | wiki:Items#Morning Star; đo: kiểm kê sheet routine 01 (L3 có 4 bảng màu) · *damage:* không rõ — wiki không nêu riêng (cùng nhóm mạnh) · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L3 40×16) — chờ 05 chốt hitbox · *reachPx:* video:planner ≈ 43 px (57 đòn) — chờ đo routine 09 · *upgradeHearts:* wiki:Items#Morning Star (đủ 8 tim cho lần nâng 2) |

## 8. Block & Stage

| Block | Stage | Boss | Giờ | Vạch/đòn | Nhạc boss | Mô tả | Nguồn |
|---|---|---|---|---|---|---|---|
| 1 | 1, 2, 3 | PhantomBat | 300 | 2 | M10_PoisonMind | Các stage luyện tập: sân và sảnh lâu đài; hầm có nước; hành lang tới phòng boss | wiki:Walkthrough; wiki:Stages 1-3 · *timeLimit:* video:Map_1 0:02.2 TIME 0300 (= CVTI GameSession); đồng hồ KHÔNG đặt lại giữa Stage 1→2→3 (1:31.87 STAGE 02 TIME 0226; 2:50.87 STAGE 03 TIME 0166) · *damage:* wiki:Gameplay#Damage (Stage 1–6 = 2 vạch) · *music:* suy luận: tên file Art_Source/music (Boss Theme) — CHƯA đối chiếu video |
| 2 | 4, 5, 6 | QueenMedusa | 400 | 2 | M10_PoisonMind | Nhiều enemy và hố hơn; bẫy gai Stage 6 | wiki:Walkthrough; wiki:Stages 4-6 · *timeLimit:* video:Map_1 3:59.2 STAGE 04 TIME 0400 · *damage:* wiki:Gameplay#Damage (Stage 1–6 = 2 vạch) · *music:* suy luận: tên file Art_Source/music (Boss Theme) — CHƯA đối chiếu video |
| 3 | 7, 8, 9 | MummyMan | *không rõ* | 3 | M10_PoisonMind | Enemy chuyển động phức tạp hơn; cầu hỏng; phòng Mummy | wiki:Walkthrough; wiki:Stages 7-9 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 7–12 = 3 vạch); wiki:Stages 7-9 · *music:* suy luận: tên file Art_Source/music (Boss Theme) — CHƯA đối chiếu video |
| 4 | 10, 11, 12 | Frankenstein | *không rõ* | 3 | M10_PoisonMind | Hầm nước có bệ nổi; mặt đất có Eagle; cống ngầm có Bone Dragon | wiki:Walkthrough; wiki:Stages 10-12 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 7–12 = 3 vạch) · *music:* suy luận: tên file Art_Source/music (Boss Theme) — CHƯA đối chiếu video |
| 5 | 13, 14, 15 | Death | *không rõ* | 4 | M10_PoisonMind | Nguy hiểm nhất: Axe Knight; Red Skeleton; chỉ chịu 4 đòn | wiki:Walkthrough; wiki:Stages 13-15 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 13–18 = 4 vạch); wiki:Stages 13-15 · *music:* suy luận: tên file Art_Source/music (Boss Theme) — CHƯA đối chiếu video |
| 6 | 16, 17, 18 | Dracula | *không rõ* | 4 | M11_BlackNight | Cầu dài; tháp đồng hồ; phòng Dracula | wiki:Walkthrough; wiki:Stages 16-18 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 13–18 = 4 vạch) · *music:* suy luận: tên file Art_Source/music (Final Battle) — CHƯA đối chiếu video |

| Stage | Block | Khu vực | Enemy | Boss | Nhạc | Nguồn |
|---|---|---|---|---|---|---|
| 1 | 1 | Sân ngoài lâu đài (không enemy) + sảnh vào lâu đài | Ghoul, Panther | None | M03_VampireKiller | wiki:Stages 1-3#Stage 1 · *music:* suy luận: tên file Art_Source/music (Courtyard) — CHƯA đối chiếu video |
| 2 | 1 | Hầm ngầm có nước (rơi nước chết) | Fishman, Bat | None | M03_VampireKiller | wiki:Stages 1-3#Stage 2 · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 3 | 1 | Hành lang cầu thang xuống phòng boss | Ghoul | PhantomBat | M03_VampireKiller | wiki:Stages 1-3#Stage 3 · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 4 | 2 | Ngắn; Spear Knight canh cửa cuối | SpearKnight, Bat | None | M04_Stalker | wiki:Stages 4-6#Stage 4; wiki:Enemies · *music:* suy luận: tên file Art_Source/music (Armory) — CHƯA đối chiếu video |
| 5 | 2 | Nhiều hố; Medusa Head ngay từ đầu | MedusaHead, SpearKnight | None | M04_Stalker | wiki:Stages 4-6#Stage 5; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 6 | 2 | Bẫy gai và sảnh vòm | BoneTower, Ghost, MedusaHead | QueenMedusa | M04_Stalker | wiki:Stages 4-6#Stage 6; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 7 | 3 | Bục và khối nhô; sát thương 3 vạch | Ghost, Fleaman, Skeleton, Raven | None | M05_WickedChild | wiki:Stages 7-9#Stage 7; wiki:Enemies · *music:* suy luận: tên file Art_Source/music (Upper Yard) — CHƯA đối chiếu video |
| 8 | 3 | Tầng dưới + tầng trên | MedusaHead, Skeleton, BoneTower, Raven | None | M05_WickedChild | wiki:Stages 7-9#Stage 8 · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 9 | 3 | Cầu hỏng; phòng Mummy | Raven, Skeleton, BoneTower, MedusaHead | MummyMan | M05_WickedChild | wiki:Stages 7-9#Stage 9; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 10 | 4 | Hầm nước có bệ nổi; trần thấp | Fishman, Bat | None | M06_WalkingOnTheEdge | wiki:Stages 10-12#Stage 10 · *music:* suy luận: tên file Art_Source/music (Castle Swamp) — CHƯA đối chiếu video |
| 11 | 4 | Trên mặt đất; Eagle liên tục; 1 Bone Dragon | Eagle, Fleaman, BoneDragon | None | M06_WalkingOnTheEdge | wiki:Stages 10-12#Stage 11; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 12 | 4 | Cống ngầm; 2 Bone Dragon | BoneDragon | Frankenstein | M06_WalkingOnTheEdge | wiki:Stages 10-12#Stage 12 · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 13 | 5 | Sảnh không có hố khó; sát thương 4 vạch | Fleaman, Skeleton, RedSkeleton | None | M07_HeartOfFire | wiki:Stages 13-15#Stage 13; wiki:Enemies · *music:* suy luận: tên file Art_Source/music (Prisonhold) — CHƯA đối chiếu video |
| 14 | 5 | Nhiều Axe Knight | AxeKnight, RedSkeleton, Fleaman | None | M07_HeartOfFire | wiki:Stages 13-15#Stage 14; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 15 | 5 | Bậc thang qua Bone Tower rồi hành lang cuối | BoneTower, AxeKnight, MedusaHead, Ghost, RedSkeleton | Death | M07_HeartOfFire | wiki:Stages 13-15#Stage 15; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 16 | 6 | Cầu dài tới tháp chuông | PhantomBatMinor | None | M08_OutOfTime | wiki:Stages 16-18#Stage 16 · *music:* suy luận: tên file Art_Source/music (Upper Terrace) — CHƯA đối chiếu video |
| 17 | 6 | Tháp đồng hồ (bánh răng) | Skeleton, Eagle, Fleaman | None | M08_OutOfTime | wiki:Stages 16-18#Stage 17 · *music:* suy luận: tên file Art_Source/music — CHƯA đối chiếu video |
| 18 | 6 | Phòng Dracula (đỉnh lâu đài) |  | Dracula | M09_NothingToLose | wiki:Stages 16-18#Stage 18 · *music:* suy luận: tên file Art_Source/music (Dracula's Quarters) — CHƯA đối chiếu video |

## 9. Điểm, 1UP, tally

| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |
|---|---|---|---|---|---|
| OneUpFirst | 30000 |  | điểm | wiki:Gameplay#Simon Belmont |  |
| OneUpSecond | 80000 |  | điểm | wiki:Gameplay#Simon Belmont |  |
| OneUpEvery | 50000 |  | điểm | wiki:Gameplay#Simon Belmont | sau 80000: 130000; 180000 … (suy luận) |
| TallyPointsPerTimeUnit | 10 |  | điểm/đv TIME | wiki:Gameplay#Points; video:Map_1 3:37.4 (+10 mỗi đơn vị) |  |
| TallyPointsPerHeart | 100 |  | điểm/tim | wiki:Gameplay#Points; video:Map_1 3:40.5–3:49.9 (+100 mỗi tim) |  |
| TallyTicksPerTimeUnit | 1 |  | tick | video:planner (HUD từng frame) — chờ đo routine 12 |  |
| TallyTicksPerHeart | 8 |  | tick | video:planner (HUD từng frame) — chờ đo routine 12 |  |
| TallyGapTicks | *chờ đo routine 12* |  | tick | video:planner ≈ 0.9 s giữa đếm TIME và đếm tim — chờ đo routine 12 |  |
| MultiKill2 | 1000 |  | điểm | wiki:Gameplay#Points (Special — 1 lần ném vũ khí phụ hạ 2 enemy) | không tính roi |
| MultiKill3 | 2000 |  | điểm | wiki:Gameplay#Points |  |
| MultiKill4 | 4000 |  | điểm | wiki:Gameplay#Points |  |
| MultiplierPickupBonus | 700 |  | điểm | wiki:Items#Double Shot (khi đang cầm Stopwatch); wiki:Gameplay#Points (hoặc không có vũ khí phụ) | 2 trang nói khác điều kiện — CV_WIKI_DATA §7.1 #3 |
| RosaryKillPoints | 0 |  | điểm | wiki:Items#Rosary (enemy bị Rosary diệt không cho điểm) |  |
| EnemyProjectilePoints | 100 |  | điểm | wiki:Gameplay#Points (một số đạn đáng 100 điểm) | đạn nào: không rõ |

## 10. Luật chung, HUD, chuyển cảnh

| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |
|---|---|---|---|---|---|
| TimeUnitTicks | 64 |  | tick | video:planner MAP1_TIMELINE §4 (time_rate 5 đoạn; 1 đv = 1.065 s) — chờ đo routine 12 |  |
| PlayerBarSegments | 16 |  | vạch | wiki:Gameplay#Damage; video:Map_1 HUD |  |
| EnemyBarSegments | 16 |  | vạch | suy luận: wiki ảnh HUD (16 đoạn); video:Map_1 HUD |  |
| DamageDisplayTicksPerBar | 8 |  | tick | video:planner (≈ thanh máu tụt dần 1 vạch / 8 tick) — chờ đo routine 12 |  |
| HealDisplayTicksPerBar | 7 |  | tick | video:planner (≈ hồi 8→16 trong 0.9 s sau quả cầu) — chờ đo routine 12 |  |
| DamageSecondQuest | 4 |  | vạch | wiki:Secrets; wiki:Stages 16-18 (Second Quest mọi đòn 4 vạch) |  |
| SubWeaponOnScreenBase | 1 |  | cái | wiki:Gameplay; wiki:Items#Double Shot |  |
| DoubleShotOnScreen | 2 |  | cái | wiki:Items#Double Shot |  |
| TripleShotOnScreen | 3 |  | cái | wiki:Items#Triple Shot |  |
| MultiplierHitsToDrop | 10 |  | lần trúng | wiki:Items#Double Shot (trúng enemy 10 lần bằng cùng vũ khí phụ) |  |
| WhipLevelOnDeath | 1 |  | cấp | wiki:Items#Morning Star (chết ⇒ roi về dạng ban đầu) |  |
| LoseSubWeaponOnDeath | 1 |  | bool | suy luận: CV_WIKI_DATA §1.4 (wiki Stages 13-15 và Bosses) | wiki không nói thẳng |
| ContinueAtStageStart | 1 |  | bool | wiki:Gameplay#Simon Belmont (hết mạng ⇒ Continue về đầu stage đang chơi) |  |
| PitWaterSpikeInstantKill | 1 |  | bool | wiki:Gameplay#Damage |  |
| StopwatchItemsStillExpire | 1 |  | bool | wiki:Items#Stopwatch |  |
| FrozenEnemiesStillHurt | 1 |  | bool | wiki:Items#Stopwatch (chạm enemy/gai đóng băng vẫn mất máu) |  |
| RosaryFlashTicks | *chờ đo routine 11* | 24–26 | tick | video:planner (màn nháy trắng 0.40–0.44 s; 3 lần) — chờ đo routine 11 |  |
| CameraFollowScreenX | *chờ đo routine 07* | 128–129 | px màn | video:planner (Simon giữ ở x màn ≈ 128–129 khi cuộn) — chờ đo routine 07 |  |
| DoorScrollPx | 128 |  | px | video:planner (cửa cuối stage: cuộn 128 px hai lần) — chờ đo routine 15 |  |
| DoorScrollSpeed | *chờ đo routine 15* | 0.9–0.91 | px/tick | video:planner — chờ đo routine 15 |  |
| DoorPauseTicks | *chờ đo routine 15* | 138–140 | tick | video:planner (dừng 2.30–2.33 s) — chờ đo routine 15 |  |
| DoorAutoWalkPx | 68 |  | px | video:planner (Simon tự đi 68 px @ 1 px/tick) — chờ đo routine 15 |  |
| StageClearToTallyTicks | *chờ đo routine 12* | 228–264 | tick | video:planner (3.8–4.4 s sau khi nhặt quả cầu; 2 lần khác nhau) — chờ đo routine 12 |  |

## 11. Nội dung vật chứa (ConfigDrop)

*Chưa có dòng — routine 11/15 điền vị trí cụ thể (wiki chỉ nêu một phần: `CV_WIKI_DATA §6`).*

