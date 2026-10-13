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
| WalkSpeed | 1.0 | 0.978–1.02 | px/tick | đo:routine 08 khớp mẫu sprite 11 đoạn đi video:Map_1 0:03.6–3:06.1 (TB 1.006; dư rms 1 px) | không gia tốc; dừng ngay khi thả |
| JumpAirTicks | 40 |  | tick | đo:routine 08 đồng hồ x video:Map_1 2:14.0 (f4021 chạm đất tick 40-41) + bảng nhảy đối xứng | trên sàn phẳng |
| JumpHeight | 36 |  | px | đo:routine 08 (pivot chân; khung roi trên không ở đỉnh = mặt sàn − 36 ở 9 cú nhảy video:Map_1) | khung co chân vẽ thấp 4 px nên trông như 32 |
| JumpRiseSpeedAvg | 2.25 |  | px/tick | đo:routine 08 (36 px / 16 tick) | thông tin — code dùng JumpRise01..16 |
| JumpRiseTicks | 16 |  | tick | đo:routine 08 bảng JumpRise (tick 16 lên tới 36 px) |  |
| JumpApexTicks | 8 |  | tick | đo:routine 08 video:Map_1 2:03.7/2:14.0/3:23.6 (cao 36 từ tick 16 tới tick 24) | số tick vận tốc 0 ở đỉnh (tick 17–24); rơi = gương của pha lên |
| JumpFallSpeed | 5 |  | px/tick | đo:routine 08 (2 tick cuối bảng nhảy = gương JumpRise02/01) | sau tick 40 chưa chạm đất ⇒ rơi FallSpeed |
| JumpDistance | 40 |  | px | đo:routine 08 (1 px/tick × 40 tick; video:Map_1 2:03.7 x 68→ tiếp đất bục ở tick 30) |  |
| JumpAirControl | 0 |  | bool | video:planner (không đổi hướng giữa không trung ở mọi mẫu); wiki:Gameplay (hướng chốt lúc bấm A); đo:routine 08 (x tăng đều 1 px/tick suốt cú nhảy 2:03.7) | 0 = không điều khiển trên không |
| StairSpeedX | 0.5 |  | px/tick | đo:routine 08 khớp mẫu 4 cửa sổ lên thang video:Map_1 0:46.7/1:18.6/2:25.7/2:32.3 (+1 px mỗi frame video) | liên tục — không nhảy bậc |
| StairSpeedY | 0.5 |  | px/tick | đo:routine 08 (như StairSpeedX; chéo 45°) |  |
| StairKnockOff | 0 |  | bool | wiki:Stages 16-18#Stage 17 (trên cầu thang không bao giờ bị đánh văng khỏi bậc) | trúng đòn trên thang: chỉ mất máu + nháy |
| KnockbackDx | 12 | 11–13 | px | đo:routine 08 khớp mẫu video:Boss_Map_1 11.9–12.6 s (f363–380: x +4/+10/+12 rồi đứng) | lùi NGƯỢC hướng mặt |
| KnockbackDy | 15 | 14–15 | px | đo:routine 08 video:Boss_Map_1 f363–380 (h 8/13/14/13/8; khung hurt_0) | 1 mẫu |
| KnockbackAirTicks | 35 | 32–38 | tick | đo:routine 08 video:Boss_Map_1 (trúng f362.5 → biến mất nháy từ f381 ≈ 36 tick) | = 2·\|KnockbackVy0\|/KnockbackGravity |
| InvincibleTicks | 94 | 92–96 | tick | đo:routine 08 video:Boss_Map_1 f381–427 (47 frame không thấy Simon sau khi đáp) | đếm từ lúc ĐÁP sau knockback (trong lúc bay hiện hurt_0 không nháy) |
| WhipTotalTicks | 22 | 18–26 | tick | đo:routine 09 Tools/cvgp/whip_timing.py 52 cú roi video:Map_1 (TB 21.6; trung vị 22; khung video mới ±1 tick) | = WhipWindupTicks + WhipRaiseTicks + WhipExtendTicks; ném vũ khí phụ dùng cùng nhịp (video 1:32.9 và 3:27.9) |
| WhipWindupTicks | 6 | 2–10 | tick | đo:routine 09 whip_timing.py 52 cú roi video:Map_1 (khung use_*_0: TB 5.85; mốt 6) | khung 0 — roi treo sau lưng |
| WhipRaiseTicks | 7 | 2–14 | tick | đo:routine 09 whip_timing.py 52 cú roi video:Map_1 (khung use_*_1: TB 7.0; trung vị 6) | khung 1 — roi vắt qua vai |
| WhipExtendTicks | 9 | 4–12 | tick | đo:routine 09 whip_timing.py 52 cú roi video:Map_1 (khung use_*_2: TB 8.73; trung vị 9) | khung 2 — roi duỗi; hitbox bật suốt khung này (video 0:03.27 chậu lửa vỡ ngay frame duỗi đầu tiên f98) |
| WhipHeightAboveFeet | 20 | 17–25 | px | đo:routine 09 sheet (hàng đục mảnh duỗi: L1 Y−25..−18; L2/L3 Y−22..−17 khi đứng) | thông tin — code dùng ConfigWhip.hit* + overlayOffsets |
| WhipPickupFreezeTicks | 46 | 44–48 | tick | đo:routine 09 khớp mẫu video:Map_1 f195–217 và f293–315 (23 frame video Simon nháy màu — không khớp sprite gốc) | routine 11 dùng khi nhặt Morning Star; camera + Simon đứng; TIME vẫn chạy |
| HurtboxWidth | 16 |  | px | đo:routine 07 bbox pixel đục sprite idle_0 (cột 0–15) | XẤP XỈ = khung sprite; routine 08: video boss f362 Simon trúng khi chạm thân dơi — hộp ROM vẫn không đo được |
| HurtboxHeight | 30 |  | px | đo:routine 07 bbox pixel đục sprite idle_0 (hàng 1–30 của khung 32) | XẤP XỈ như HurtboxWidth; chân ở hàng Y−1 |
| CrouchHurtboxHeight | 23 |  | px | đo:routine 07 bbox pixel đục sprite jumpduck_0 (hàng 1–23); đo:routine 08 khung ngồi vẽ lệch +8 (video:Map_1 1:02.8) | XẤP XỈ như HurtboxWidth; dùng cả cho tư thế co chân khi nhảy (vẽ +4) |
| JumpRise01 | 5 |  | px | đo:routine 08 đồng hồ x video:Map_1 2:03.7 (f3712) và 3:23.6 (f6111) + 14 cú nhảy ghép theo thời gian video | độ cao tick 1 = 5 (khung đứng) |
| JumpRise02 | 5 |  | px | đo:routine 08 (như JumpRise01) | cao 10 (khung đứng) |
| JumpRise03 | 3 |  | px | đo:routine 08 (như JumpRise01) | cao 13 (khung co chân đầu tiên) |
| JumpRise04 | 3 |  | px | đo:routine 08 (như JumpRise01) | cao 16 |
| JumpRise05 | 3 |  | px | đo:routine 08 (như JumpRise01) | cao 19 |
| JumpRise06 | 3 |  | px | đo:routine 08 (như JumpRise01) | cao 22 |
| JumpRise07 | 3 |  | px | đo:routine 08 (như JumpRise01) | cao 25 |
| JumpRise08 | 2 |  | px | đo:routine 08 (như JumpRise01) | cao 27 |
| JumpRise09 | 2 |  | px | đo:routine 08 (như JumpRise01) | cao 29 |
| JumpRise10 | 2 |  | px | đo:routine 08 (như JumpRise01) | cao 31 |
| JumpRise11 | 1 |  | px | đo:routine 08 (như JumpRise01) | cao 32 |
| JumpRise12 | 1 |  | px | đo:routine 08 (như JumpRise01) | cao 33 |
| JumpRise13 | 1 |  | px | đo:routine 08 (như JumpRise01) | cao 34 |
| JumpRise14 | 1 |  | px | đo:routine 08 (như JumpRise01) | cao 35 |
| JumpRise15 | 0 |  | px | đo:routine 08 (như JumpRise01) | cao 35 |
| JumpRise16 | 1 |  | px | đo:routine 08 (như JumpRise01) | cao 36 = đỉnh; rơi tick 25–40 = gương (−JumpRise16 … −JumpRise01) |
| JumpTuckStartTick | 3 |  | tick | đo:routine 08 (khung đứng ở cao 5/10 = tick 1–2; co chân từ cao 13 = tick 3; 14 cú nhảy video:Map_1) |  |
| JumpUntuckTick | 30 | 29–31 | tick | đo:routine 08 (co chân cuối cùng cao 32–33 = tick 28–29; khung đứng từ cao 29–25; video:Map_1 0:30.5/0:37.6/2:54.0/3:21.2) | sau roi trên không quay về khung co chân (video:Map_1 2:14.6/2:36.8) — routine 09 |
| JumpTuckDrawY | 4 | 3–5 | px | đo:routine 08 (ghép 14 cú nhảy: tổng dư nhỏ nhất ở +4 = 161 px so với +3/+5 = 193/192) | khung co chân vẽ thấp hơn pivot 4 px |
| CrouchDrawY | 8 |  | px | đo:routine 08 khớp mẫu video:Map_1 1:02.8 (f1886–1896 jumpduck_0 tại mặt sàn + 8; err 0.019–0.022) | khung ngồi = jumpduck_0 vẽ thấp 8 px (chân chạm sàn) |
| FallSpeed | 8 |  | px/tick | đo:routine 08 video:Map_1 2:05.3 (f3757–3760 rơi 32 px trong 4 tick) và 3:24.1 (f6129–6132: hết bảng nhảy rơi 32 px trong 4 tick) | hằng — không gia tốc; giữ vận tốc ngang lúc rời mép |
| LandCrouchMinFallTicks | 5 | 5–8 | tick | đo:routine 08 video:Map_1 1:02.8 (rơi 64 px = 8 tick ⇒ ngồi khựng) và 2:05.3 (rơi 32 px = 4 tick ⇒ không) | ngưỡng thật trong 5–8 |
| LandCrouchTicks | 20 | 18–22 | tick | đo:routine 08 video:Map_1 1:02.8 (f1886–1896 ngồi 10–11 frame video) | không điều khiển trong lúc ngồi khựng |
| WalkFrameTicks | 7 |  | tick | đo:routine 08 (11 đoạn đi: thứ tự walk_0/1/2/1 mỗi 7 tick khớp 84.5 %; 6 tick 48 % / 8 tick 63 %) | AnimSet walk bắt đầu ở walk_2 (3/4 lần bắt đầu đi trong video) |
| StairFrameTicks | 8 |  | tick | đo:routine 08 (4 cửa sổ lên thang: mỗi khung 4 px = 8 tick; khung theo VỊ TRÍ trên thang) | lên: stairs_up_0 khi (t−1) mod 8 < 4; xuống: stairs_down_0 khi t mod 8 < 4; còn lại walk_1 |
| StairStepPx | 8 |  | px | suy luận: bậc 8×8 (CVGP 06) — video không có cảnh dừng giữa thang | thả nút ⇒ đi nốt tới bội 8 px rồi đứng |
| StairEnterRange | 8 |  | px | quy ước — video:Map_1 0:46.7/1:18.6/2:25.7/2:32.3 bấm Lên khi cách chân thang 3–4 px | phía dưới gầm thang: kẹp lên đường thang tại x hiện tại; phía kia: tự đi tới chân rồi leo |
| StairLineOffsetUpRight | 3 |  | px | đo:routine 08 (thang ↗ EH stair0/stair3: x + y của pivot = đường tâm bậc + 3; ra đỉnh x = đỉnh + 3 ở 0:48.9) | pivot Simon trên thang ↗ lệch +3 px theo x so với StairPath |
| StairLineOffsetUpLeft | 0 |  | px | đo:routine 08 (thang ↖ CB stair1 + CA stair3: x − y đúng đường tâm bậc; ra đỉnh 2:34.3) |  |
| StairWalkPoseDrawY | 2 |  | px | đo:routine 08 (khi lên thang khung walk_1 vẽ thấp hơn stairs_up_0 2 px ở cả 4 cửa sổ) | chỉ khi lên thang |
| StairDownDrawBack | 3 | 2–4 | px | đo:routine 08 video:Map_1 1:47.5 (CA stair0 −2.5) và 1:54.4 (CB stair0 −3.6) khi xuống thang ↖ | vẽ lùi 3 px ngược hướng di chuyển ngang khi xuống — thang ↗ lấy đối xứng (chưa có video) |
| KnockbackVy0 | -1.75 |  | px/tick | đo:routine 08 khớp parabol video:Boss_Map_1 f363–380 (h 8/13/14/13/8) | cùng KnockbackGravity ⇒ cao ≈ 15 px; bay ≈ 35 tick |
| KnockbackGravity | 0.1 |  | px/tick² | đo:routine 08 (như KnockbackVy0) |  |
| KnockbackDxTicks | 12 |  | tick | đo:routine 08 video:Boss_Map_1 (x +12 rồi đứng yên dù còn trên không — f369–380) | lùi 1 px/tick trong 12 tick đầu |
| InvincibleBlinkPeriod | 2 |  | tick | suy luận: video:Boss_Map_1 f381–427 KHÔNG thấy Simon ở cả 47 frame ⇒ nháy 1 tick hiện / 1 tick ẩn (bản quay 30 fps bắt trúng pha ẩn) |  |
| DeathKneelTicks | 16 |  | tick | quy ước — video/wiki không có cảnh chết (routine 12 xem lại) | khung death_0 (quỳ) trước death_1 (nằm) |
| DeathTicks | 124 |  | tick | suy luận: độ dài nhạc Art_Source/music/15. Death.mp3 = 2.07 s; video không có cảnh chết | từ lúc ngã (hoặc rơi hố) tới khi báo sự kiện Died (routine 12 hồi sinh) |
| WhipColorTicks | 1 |  | tick | đo:routine 09 Tools/cvgp/whip_colors.py video:Map_1 (286 frame duỗi roi cấp 3: 2 frame mới cách 2 tick mà khác màu thì lệch chỉ số 2 ở 37/53 lần ⇒ đổi màu MỖI tick qua 4 bảng màu) | thứ tự c1→c2→c3→c4 theo sheet là suy luận (video 30 fps không phân biệt chiều); chu kỳ đếm từ tick đầu của đòn (suy luận) |
| SubWeaponSpawnTick | 3 | 2–5 | tick | đo:routine 09 proj_fit.py: dao video:Map_1 1:33.0 (f2795–2805) + 1:47.1 trên thang (f3214–3220); rìu 3:27.9/3:29.3 (khớp parabol g 0.25) ⇒ vũ khí xuất hiện tại pivot 2.5–5 tick sau khi khung duỗi bắt đầu | tick thứ N (0 = tick đầu) của khung duỗi thì sinh vũ khí phụ; tim trừ ngay lúc bấm (HUD trừ tim trước SFX ném ~0.2 s — routine 02) |

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
| SubWeapon.Dagger | spawnHeightAboveFeet | 23 |  |  | px | pivot (tâm sprite) dao; x sinh = pivot Simon | đo:routine 09 proj_fit.py video:Map_1 f2797 (góc trên sprite dao = Y−31 khi Simon Y 112) và f3214 trên thang (Y 216 ⇒ Y−31) ⇒ pivot tâm sprite 16×16 = Y−23; Tools/cvgp/attack_compare.py dy 0 |
| SubWeapon.Axe | apexPx | 50 |  |  | px | thông tin | đo:routine 09 (= vy0²/2g với vy0 −5.0 g 0.25; video:Map_1 đỉnh tâm rìu Y−80.5 ≈ 28 + 50 + 2) |
| SubWeapon.Axe | apexTicks | 20 |  |  | tick | thông tin | đo:routine 09 (= \|vy0\|/g) |
| SubWeapon.Axe | spawnHeightAboveFeet | 26 |  |  | px | pivot (tâm sprite) rìu | đo:routine 09 khớp parabol 2 quỹ đạo (g cố định 0.25): tâm bbox rìu lúc sinh Y−27.5 / Y−27.8 ⇒ pivot tâm sprite = Y−26 (bbox axe_0 lệch −1.5 so với pivot) |
| SubWeapon.HolyWater | flameHoldTicks | *chờ đo routine 14* | 138 | 144 | tick | boss bị lửa giữ chân | video:planner (≈ 2.3–2.4 s = khoảng giữa 2 lần ném trong video boss — KHÔNG phải thời gian cháy; xem fireTicks) — chờ đo routine 14 |
| Item.MagicCrystal | spawnDelayTicks | 150 |  |  | tick |  | video:planner (2.5 s sau khi boss chết; 2 video) — chờ đo routine 14 |
| Item.MagicCrystal | spawnScreenX | 127.5 |  |  | px màn |  | video:planner (2 video) — chờ đo routine 14 |
| Item.MagicCrystal | spawnScreenY | 128.5 |  |  | px màn |  | video:planner (2 video) — chờ đo routine 14 |
| Item.MagicCrystal | gravity | *chờ đo routine 14* | 0.23 | 0.26 | px/tick² |  | video:planner — chờ đo routine 14 |
| SubWeapon.HolyWater | spawnHeightAboveFeet | 23 |  |  | px | tâm lọ | suy luận: = dao (video boss Simon đang nháy bất tử — không thấy điểm sinh) |
| SubWeapon.HolyWater | fireTicks | 80 | 80 | 94 | tick | sau đó lửa biến mất; anim holywater_fire lặp | đo:routine 09 proj_fit.py video:Boss: lửa vỡ ~f348 tắt sau f389; lọ cuối chạm sàn ~f570 lửa tắt sau f609 (≈ 40 frame video) |
| SubWeapon.HolyWater | fireRehitTicks | 8 |  |  | tick | Hitbox.Activate lại ⇒ mục tiêu trúng lần nữa nếu hết bất tử | quy ước routine 09 (lửa đánh lại mỗi 8 tick) — video boss: thanh ENEMY tụt đều khi trong lửa; chờ đo routine 14 |
| SubWeapon.Cross | spawnHeightAboveFeet | 23 |  |  | px | tâm thập tự | suy luận: = dao (Cross không có trong video) |
| SubWeapon.Cross | turnDistancePx | 112 |  |  | px | quay đầu khi đi đủ xa hoặc gần mép màn (turnScreenMarginPx) | quy ước routine 09 — wiki Items#Cross chỉ nói bay ra rồi quay về; không có video |
| SubWeapon.Cross | turnScreenMarginPx | 8 |  |  | px | tâm cách mép màn ≤ N px thì quay đầu | quy ước routine 09 (không có video) |
| SubWeapon.Dagger | offscreenMarginPx | 8 |  |  | px |  | quy ước routine 09 (vũ khí biến mất khi tâm ra khỏi màn quá nửa sprite 16 px) |
| SubWeapon.Axe | offscreenMarginPx | 8 |  |  | px |  | quy ước routine 09 (vũ khí biến mất khi tâm ra khỏi màn quá nửa sprite 16 px) |
| SubWeapon.HolyWater | offscreenMarginPx | 8 |  |  | px |  | quy ước routine 09 (vũ khí biến mất khi tâm ra khỏi màn quá nửa sprite 16 px) |
| SubWeapon.Cross | offscreenMarginPx | 8 |  |  | px |  | quy ước routine 09 (vũ khí biến mất khi tâm ra khỏi màn quá nửa sprite 16 px) |
| Item.SmallHeart | fallSpeed | 0.5 |  |  | px/tick | rơi đều, không gia tốc | đo:routine 11 video Map_1 f4046–4069 (CB): y tăng đều 0.477 px/tick (22 px / 46 tick) |
| Item.SmallHeart | swayVx0 | 1.125 | 1.0 | 1.2 | px/tick | lần đầu sang PHẢI (1 mẫu — suy luận) | đo:routine 11 cùng vết: mô hình vận tốc ngang đổi đều khớp rms 0.44 px (v0 1.10, a 0.045) — làm tròn 288/256 |
| Item.SmallHeart | swayAccel | 0.046875 | 0.04 | 0.05 | px/tick² | vận tốc ngang đổi chiều quanh điểm sinh (chu kỳ 4·v0/a = 96 tick) | đo:routine 11 cùng vết (a 0.045) — làm tròn 12/256; biên độ v0²/2a = 13.5 px (video lệch tối đa +14) |
| Item.InvisibilityPotion | visibleTicks | 84 | 80 | 86 | tick | pha 1: vẽ thường nhưng đã xuyên enemy (suy luận) | đo:routine 11 video Map_1 f5347 (nhặt) → f5389 (Simon hết vẽ màu thường; trước đó vẫn hiện, chỉ mất vài frame do nhấp nháy sprite) |
| Item.InvisibilityPotion | glintFromTick | 160 |  |  | tick | pha 2: ẩn; từ tick này lóe white | đo:routine 11 video Map_1: lóe bảng màu white thưa ở t 106/142, dày từ t 168 (168–178, 184, 196, 206, 212) — xấp xỉ |
| Item.InvisibilityPotion | glintPeriodTicks | 10 |  |  | tick |  | đo:routine 11 cùng cửa sổ (lóe cách ~10 tick) — xấp xỉ |
| Item.InvisibilityPotion | glintOnTicks | 3 |  |  | tick |  | đo:routine 11 cùng cửa sổ (mỗi lần lóe 1–2 frame video) — xấp xỉ |
| Item.BreakableBlock | debrisVxTop | 0.75 |  |  | px/tick | 2 mảnh trên: ±vx | đo:routine 11 video Map_1 1:39.6 f2989–3004 (mảnh tím bay ngang ≈ 0.76 px/tick) — xấp xỉ ±0.3 |
| Item.BreakableBlock | debrisVyTop | -1.5 |  |  | px/tick |  | đo:routine 11 cùng cửa sổ (mảnh lên rồi rơi, đỉnh ~10 tick) — xấp xỉ |
| Item.BreakableBlock | debrisVxBottom | 0.375 |  |  | px/tick | 2 mảnh dưới | đo:routine 11 cùng cửa sổ (mảnh dưới ≈ ±0.3 px/tick) — xấp xỉ |
| Item.BreakableBlock | debrisVyBottom | -0.75 |  |  | px/tick |  | đo:routine 11 cùng cửa sổ — xấp xỉ |
| Item.BreakableBlock | debrisGravity | 0.1875 | 0.15 | 0.25 | px/tick² | mảnh xuyên địa hình, biến mất khi ra khỏi màn | đo:routine 11 cùng cửa sổ (khớp parabol 1 mảnh ≈ 0.19) — xấp xỉ |

## 5. Item & vật chứa

| Item | Tên khác | Loại | +Tim | Điểm | +Vạch máu | Hồi đầy | Hiệu lực (tick) | Tồn tại (tick) | Kiểu rơi | Hiệu ứng | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|
| SmallHeart | Heart | Heart | 1 | 0 | — | không | — | 188 | rơi 0.5 px/tick + đung đưa ngang (ConfigActorParam Item.SmallHeart — đo:routine 11 f4046–4069) | +1 tim (đạn vũ khí phụ) | wiki:Items#Small Heart · *score:* wiki:Gameplay#Points (nhặt tim giữa stage không cho điểm) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| LargeHeart | Heart (lớn) | Heart | 5 | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | +5 tim | wiki:Items#Large Heart; video:Map_1 0:03.93 (tim 05→10) · *score:* wiki:Gameplay#Points (nhặt tim giữa stage không cho điểm) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| RedMoneyBag | Moneybag (đỏ) | Score | — | 100 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | +100 điểm | wiki:Items#Red Money Bag · *score:* wiki:Items#Red Money Bag; video:Map_1 (+100 nhiều lần khớp) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| PurpleMoneyBag | Moneybag (tím) | Score | — | 400 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | +400 điểm | wiki:Items#Purple Money Bag · *score:* wiki:Items#Purple Money Bag; video:Map_1 0:21.83 (+400 khớp) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| WhiteMoneyBag | Moneybag (trắng) | Score | — | 700 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | +700 điểm | wiki:Items#White Money Bag · *score:* wiki:Items#White Money Bag; video:Map_1 (5 lần +700 khớp; CVGP_MEMORY §3 SFX 23) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| FlashingMoneyBag | Bonus / hidden money bag | Score | — | 1000 | — | không | — | 188 | trồi lên tại chỗ bí mật (ConfigRule.SecretRiseSpeed/SecretRisePx — đo:routine 11 f4200–4226); ra từ vật chứa thì rơi thường | +1000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *score:* wiki:Items#Bonuses; video:Map_1 2:23.80 (+1000 khớp) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu); wiki Stage 7/25: biến mất sau chốc lát |
| Crown | Bonus | Score | — | 2000 | — | không | — | 188 | trồi lên tại chỗ bí mật (ConfigRule.SecretRiseSpeed/SecretRisePx — đo:routine 11 f4200–4226); ra từ vật chứa thì rơi thường | +2000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *score:* wiki:Items#Bonuses · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| TreasureChest | Treasure Box | Score | — | 2000 | — | không | — | 188 | trồi lên tại chỗ bí mật (ConfigRule.SecretRiseSpeed/SecretRisePx — đo:routine 11 f4200–4226); ra từ vật chứa thì rơi thường | +2000 điểm | wiki:Items#Bonuses; wiki:Secrets#Bonuses · *score:* wiki:Items#Bonuses · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Moai |  | Score | — | 4000 | — | không | — | 188 | trồi lên tại chỗ bí mật (ConfigRule.SecretRiseSpeed/SecretRisePx — đo:routine 11 f4200–4226); ra từ vật chứa thì rơi thường | +4000 điểm | wiki:Secrets#Bonuses · *score:* wiki:Items#Bonuses · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| MorningStar | Whip upgrade | Whip | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Roi lên 1 cấp (tối đa cấp 3); chết ⇒ về cấp 1; nhặt ⇒ cả thế giới đóng băng (ConfigSimon.WhipPickupFreezeTicks) | wiki:Items#Morning Star · *score:* wiki:Stages 1-3 ảnh HUD (SCORE 000000 sau 2 Morning Star + Dagger) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Dagger |  | SubWeapon | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Vũ khí phụ Dagger (ConfigSubWeapon) | wiki:Items#Dagger · *score:* wiki:Stages 1-3 ảnh HUD (SCORE 000000 khi đã có Dagger) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Axe | Throwing Axe | SubWeapon | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Vũ khí phụ Axe (ConfigSubWeapon) | wiki:Items#Axe · *score:* video:Map_1 (nhặt Axe — SCORE không đổi; đo:routine 11 HUD csv) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| HolyWater | Fire Bomb | SubWeapon | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Vũ khí phụ Holy Water (ConfigSubWeapon) | wiki:Items#Holy Water · *score:* suy luận: wiki chỉ nêu điểm cho túi/bonus/multiplier (CV_WIKI_DATA §2.1) — routine 11 · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Cross | Boomerang | SubWeapon | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Vũ khí phụ Cross (ConfigSubWeapon) | wiki:Items#Cross · *score:* suy luận: wiki chỉ nêu điểm cho túi/bonus/multiplier (CV_WIKI_DATA §2.1) — routine 11 · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Stopwatch | Stop Watch | SubWeapon | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Vũ khí phụ Stopwatch (ConfigSubWeapon) | wiki:Items#Stopwatch · *score:* suy luận: wiki chỉ nêu điểm cho túi/bonus/multiplier (CV_WIKI_DATA §2.1) — routine 11 · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| DoubleShot | Shot Multiplier ×2 | Multiplier | — | *n/a* | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | 2 vũ khí phụ cùng lúc trên màn; mất khi nhặt vũ khí phụ khác; rơi khi trúng enemy 10 lần bằng cùng vũ khí phụ | wiki:Items#Double Shot · *score:* n/a: điểm có điều kiện ⇒ ConfigScore.MultiplierPickupBonus · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| TripleShot | Shot Multiplier ×3 | Multiplier | — | *n/a* | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | 3 vũ khí phụ cùng lúc trên màn; mất khi nhặt vũ khí phụ khác | wiki:Items#Triple Shot · *score:* n/a: điểm có điều kiện ⇒ ConfigScore.MultiplierPickupBonus · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| Rosary | Cross (bảng Items) | Special | — | 0 | — | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Diệt mọi enemy trên màn (trừ boss; không cho điểm); phá đạn của boss; màn nháy trắng (ConfigRule.RosaryFlashTicks) | wiki:Items#Rosary · *score:* video:Map_1 (nhặt Rosary — SCORE không đổi; đo:routine 11 HUD csv) · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| InvisibilityPotion |  | Special | — | 0 | — | không | 232 | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Tàng hình (lấp lánh): đi xuyên enemy không bị thương; vẫn chết khi rơi hố | wiki:Items#Invisibility Potion · *score:* video:Map_1 (nhặt InvisibilityPotion — SCORE không đổi; đo:routine 11 HUD csv) · *effectTicks:* đo:routine 11 video Map_1 nhặt f5347 → Simon hiện lại thường f5463 (116 frame; SFX 27→28 cách 3.9 s) — wiki ≈ 5 s nhưng theo video (H1); pha xem ConfigActorParam Item.InvisibilityPotion · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| PotRoast | Pork Chop | Heal | — | 0 | 6 | không | — | 188 | rơi trọng lực ConfigRule.ItemGravity (đo:routine 11) | Hồi 6 vạch máu; chỉ có trong khối gạch phá bằng roi | wiki:Items#Pork Chop · *score:* suy luận: wiki chỉ nêu điểm cho túi/bonus/multiplier (CV_WIKI_DATA §2.1) — routine 11 · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu) |
| OneUp |  | Life | — | 0 | — | không | — | 188 | trồi lên tại chỗ bí mật (ConfigRule.SecretRiseSpeed/SecretRisePx — đo:routine 11 f4200–4226); ra từ vật chứa thì rơi thường | +1 mạng; chỉ hiện khi kích hoạt bí mật | wiki:Items#1up; wiki:Secrets · *score:* suy luận: wiki chỉ nêu điểm cho túi/bonus/multiplier (CV_WIKI_DATA §2.1) — routine 11 · *lifetimeTicks:* đo:routine 11 (ConfigRule.ItemLifetimeTicks — video Map_1 f874–968; 1 mẫu); wiki Stage 7/25: biến mất sau chốc lát |
| MagicCrystal | Crystal Ball | Crystal | — | — | — | có | — | — | rơi theo trọng lực (ConfigActorParam Item.MagicCrystal) | Hồi đầy máu + tính tally (ConfigScore); kết thúc block | wiki:Items#Magic Crystal; wiki:Walkthrough; video:Boss_Map_1 20.33–21.80 s |
| Torch | chậu lửa (đuốc) | Container | — | — | — | không | — | — | — | Vật chứa phá bằng roi (Stage 1 ngoài trời: 5 cái); nội dung ở ConfigDrop | wiki:Stages 1-3#Stage 1 (ảnh bản đồ); video:Map_1 0:02–0:16 |
| WallCandle | nến tường | Container | — | — | — | không | — | — | — | Vật chứa phá bằng roi/vũ khí phụ; nội dung ở ConfigDrop | wiki:Items; wiki:Stages 1-3 |
| BreakableBlock | khối gạch phá được | Container | — | — | — | không | — | — | — | Khối/tường gạch phá bằng roi giấu Pot Roast / multiplier / túi tiền; nội dung ở ConfigDrop | wiki:Items#Pork Chop; wiki:Secrets; video:Map_1 1:39.61 |

## 6. Vũ khí phụ

| Vũ khí | Giá tim | Nhóm | Xuyên mục tiêu | Quay về | Đóng băng (tick) | vx (px/tick) | vy0 (px/tick) | g (px/tick²) | Đặc tính | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|
| Dagger | 1 | Weak | 0 | không | — | 4.0 | — | — | Bay nhanh thẳng về phía trước; sức ≈ roi da | wiki:Items#Dagger; wiki:Enemies (nhóm yếu) · *pierces:* wiki:Items#Dagger (chỉ trúng 1 mục tiêu) · *motion:* đo:routine 09 Tools/cvgp/proj_fit.py video:Map_1 1:33.0 (f2797–2805 +8 px mỗi frame video = 4 px/tick) và 1:47.1 trên thang (f3214–3220); bay thẳng ngang |
| Axe | 1 | Strong | 1 | không | — | 2.0 | -5.0 | 0.25 | Bay vòng cung lên cao rồi rơi xuống sàn; tốt với mục tiêu trên cao/bay | wiki:Items#Axe; wiki:Enemies (nhóm mạnh) · *pierces:* đo:routine 09 video:Map_1 3:28.9 (f6268–6280: rìu trúng Phantom Bat rồi vẫn bay tiếp xuống sàn) · *motion:* đo:routine 09 proj_fit.py khớp 2 quỹ đạo video:Map_1 f6239–6252 và f6281–6294 (vx 1.99 / 2.16; với g 0.25: vy0 −5.04 / −5.00 tại điểm sinh (pivot; Y−28)); làm tròn |
| HolyWater | 1 | Weak | 1 | không | — | 1.5 | -1.3 | 0.125 | Lọ vỡ khi chạm sàn thành ngọn lửa nhỏ giữ chân/đốt enemy | wiki:Items#Holy Water; wiki:Enemies (nhóm yếu) · *pierces:* suy luận: wiki Items#Holy Water (lửa trên sàn nhốt enemy — trúng mọi mục tiêu chạm lửa) · *motion:* đo:routine 09 proj_fit.py khớp 2 quỹ đạo lọ video:Boss 13.0–13.9 s (f390–403 / f404–418: vx 1.57 / 1.43; g 0.116 / 0.133; với g 0.125 vy0 −1.13 / −1.53) |
| Cross | 1 | Strong | 1 | có | — | 3.0 | — | — | Bay ra rồi quay về người ném; trúng nhiều mục tiêu cả lượt đi và về | wiki:Items#Cross; wiki:Enemies (nhóm mạnh) · *pierces:* wiki:Items#Cross (trúng nhiều mục tiêu; trúng cả lượt về) · *motion:* quy ước routine 09 — Cross không có trong video và wiki không nêu tốc độ (chờ video) |
| Stopwatch | 5 | None | — | không | 300 | — | — | — | Đóng băng enemy và gai di động; hầu hết boss miễn nhiễm (trừ Phantom Bat và Queen Medusa); không bị multiplier ảnh hưởng | wiki:Items#Stopwatch; wiki:Gameplay · *freezeTicks:* wiki:Items#Stopwatch (5 giây × 60) |

## 7. Roi (Vampire Killer)

| Cấp | Tên | Nhóm | Sát thương so roi da | Dài mảnh roi sheet (px) | Tầm tay→đầu roi (px) | Tim cần (wiki) | Số màu | Nguồn |
|---|---|---|---|---|---|---|---|---|
| 1 | Leather Whip | Weak | 1 | 24 | 24 | — | 1 | wiki:Items#Morning Star; wiki:Enemies (nhóm yếu) · *damage:* quy ước: mốc so sánh 1× · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L1 24×16) — chờ 05 chốt hitbox · *reachPx:* đo:routine 09 sheet (mảnh duỗi đục rộng 24 px = chuôi→đầu roi; đầu roi cách pivot 36 px); video:planner ≈ 22/24–25 (±2 — chậu lửa che đầu roi) · *hit:* đo:routine 09 bbox pixel đục mảnh roi duỗi sheet whip_l1_ext (cột 0–23 · hàng 8–15) — góc trên-trái mảnh đặt theo overlayOffsets khung 2 (routine 05) |
| 2 | Chain Whip (ngắn) | Strong | 1.5 | 24 | 24 | 4 | 1 | wiki:Items#Morning Star; wiki:Enemies (nhóm mạnh) · *damage:* wiki:Items#Morning Star (gấp 1.5 lần roi da) · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L2 24×16) — chờ 05 chốt hitbox · *reachPx:* đo:routine 09 sheet (mảnh duỗi đục rộng 24 px = chuôi→đầu roi; đầu roi cách pivot 36 px); video:planner ≈ 22/24–25 (±2 — chậu lửa che đầu roi) · *upgradeHearts:* wiki:Items#Morning Star (đủ 4 tim cho lần nâng 1 — cơ chế không giải thích; §7.1 #5) · *hit:* đo:routine 09 bbox pixel đục whip_l2_ext (cột 0–23 · hàng 10–15) |
| 3 | Chain Whip (dài) | Strong | *không rõ* | 40 | 40 | 8 | 4 | wiki:Items#Morning Star; đo: kiểm kê sheet routine 01 (L3 có 4 bảng màu) · *damage:* không rõ — wiki không nêu riêng (cùng nhóm mạnh) · *spriteLenPx:* đo: kiểm kê sheet routine 01 (simon_whippart mảnh duỗi L3 40×16) — chờ 05 chốt hitbox · *reachPx:* đo:routine 09 sheet (mảnh duỗi đục rộng 40 px; đầu roi cách pivot 52 px); video:planner ≈ 43 (57 đòn ±3) — H1 pixel theo sheet · *upgradeHearts:* wiki:Items#Morning Star (đủ 8 tim cho lần nâng 2) · *hit:* đo:routine 09 bbox pixel đục whip_l3c1..4_ext (cột 0–39 · hàng 10–15; 4 màu cùng hình) |

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
| RosaryFlashTicks | 26 | 22–28 | tick | đo:routine 11 video Map_1 3 lần nhặt Rosary: màu đen nền của CẢ MÀN (kể cả HUD) đổi xám f2252–2264 (13 frame), f2543–2553 (11), f4883–4896 (14) ⇒ 22–28 tick; chọn 26 (planner 0.40–0.44 s) | xám/thường xen kẽ — RosaryFlashPeriodTicks |
| CameraFollowScreenX | 128 |  | px màn | đo:routine 07 Tools/cvgp/measure_camera.py (Map_1: pivot Simon = x_med màu − lệch sprite (trái −2/phải +1); cuộn phải n=1452 trung vị x_med 129 ⇒ pivot 128; cuộn trái n=293 x_med 126 ⇒ pivot 128; cả cầu thang) | camera X = kẹp(Simon.X − 128); không vùng chết; không trễ; cùng luật 2 hướng |
| DoorScrollPx | 128 |  | px | video:planner (cửa cuối stage: cuộn 128 px hai lần) — chờ đo routine 15 |  |
| DoorScrollSpeed | *chờ đo routine 15* | 0.9–0.91 | px/tick | video:planner — chờ đo routine 15 |  |
| DoorPauseTicks | *chờ đo routine 15* | 138–140 | tick | video:planner (dừng 2.30–2.33 s) — chờ đo routine 15 |  |
| DoorAutoWalkPx | 68 |  | px | video:planner (Simon tự đi 68 px @ 1 px/tick) — chờ đo routine 15 |  |
| StageClearToTallyTicks | *chờ đo routine 12* | 228–264 | tick | video:planner (3.8–4.4 s sau khi nhặt quả cầu; 2 lần khác nhau) — chờ đo routine 12 |  |
| ItemGravity | 0.25 |  | px/tick² | đo:routine 11 Tools/cvgp/item_fit.py — 4 lần item rơi khỏi chậu lửa CE (f110–115, f190–193, f378–384, f452–456) + nước thánh rơi 48 px ở EH (f1713–1723): y = g·t(t−1)/2 dư ≤ 1 px; không thấy vận tốc trần tới 4.75 px/tick | mọi item trừ tim nhỏ (đung đưa); vận tốc đầu 0 |
| ItemLifetimeTicks | 188 | 182–190 | tick | đo:routine 11 video Map_1 f874→f968: đồng hồ (rơi từ Zombie) nằm sàn rồi biến mất giữa màn, KHÔNG nháy trước (f940–967 hiện đủ) — 1 mẫu (91–95 frame) | tính từ lúc item hiện; wiki chỉ nói biến mất sau thời gian ngắn |
| ItemDespawnInsetPx | 8 |  | px | đo:routine 11 video Map_1 f1796→1797: nước thánh trôi theo camera và biến mất khi mép trái sprite chạm mép màn (tâm cách mép 8 px) | gỡ item khi tâm cách mép màn < 8 px (phía phải: suy luận đối xứng) |
| BreakItemDelayTicks | 20 | 18–24 | tick | đo:routine 11 video Map_1 5 chậu lửa CE: frame lửa vỡ đầu tiên → item hiện = 10/12/11/9/9 frame (TB 10.2 ⇒ ~20 tick) | nến/chậu: hết hiệu ứng lửa mới hiện item; khối gạch: item hiện NGAY (1:39.6) |
| HitSparkTicks | 16 | 10–18 | tick | đo:routine 11 Tools/cvgp/item_fit.py chỉ mẫu hit_0: tia lửa hồng thấy cùng lúc lửa khung 0–1 ở 4 chậu CE (f104–108, f271–279, f371–377, f443–449 = 5–9 frame) rồi tắt khi lửa khung 2 hiện | vẽ cùng ô với lửa vỡ (tâm vật) |
| ScorePopupTicks | 36 | 32–36 | tick | đo:routine 11 video Map_1 chữ 100 f886–903, chữ 700 f1339–1355, chữ 100 f3005–3020 (16–18 frame) | chữ điểm đứng yên tại chỗ item |
| ScorePopupFlashTicks | 48 |  | tick | đo:routine 11 video Map_1 chữ 1000 f4315–4338 (24 frame, đổi màu) — 1 mẫu | túi nhấp nháy 1000 |
| RosaryFlashPeriodTicks | 4 |  | tick | đo:routine 11 video Map_1 f4883–4896: xám/đen xen kẽ từng frame video (2 tick) ⇒ chu kỳ 4 tick | 2 tick xám + 2 tick thường |
| WhipPickupPaletteTicks | 2 |  | tick | đo:routine 11 video Map_1 f193–217: đổi màu liền kề (đỏ→tím→xanh→thường) chiếm 8/14 lần đổi giữa 2 frame video liên tiếp ⇒ đổi mỗi 2 tick (thứ tự SUY LUẬN) | Simon nháy 4 bảng: thường→đỏ→tím→xanh |
| SecretCrouchTicks | 4 |  | tick | đo:routine 11 video Map_1 f4196 (Simon ngồi trên khối CB) → f4200 túi bắt đầu trồi ⇒ ~8 tick trừ ~3 tick trồi pixel đầu (xấp xỉ ±3) | ngồi giữ ≥ N tick trong vùng bí mật |
| SecretRiseSpeed | 0.3125 | 0.25–0.38 | px/tick | đo:routine 11 video Map_1 f4200–4226: túi 1000 trồi từ sau sàn 16 px trong ~26 frame | item bí mật trồi lên (sàn che phần dưới) |
| SecretRisePx | 16 |  | px | đo:routine 11 video Map_1 f4200–4226 (trồi hết chiều cao túi) |  |
| MorningStarNeedsHearts | 1 |  | bool | suy luận: wiki Items#Morning Star (nến vốn chứa tim nhỏ đổi thành Morning Star khi đã thu đủ tim 4 / 8) — chọn so với SỐ TIM ĐANG CÓ (ConfigWhip.upgradeHearts) — CVGP_MEMORY §5 | 0 = luôn đổi khi roi < 3 |

## 11. Nội dung vật chứa (ConfigDrop)

| Vật | Stage | Vật chứa | Item | Ghi chú | Nguồn |
|---|---|---|---|---|---|
| CE_T1 | 1 | Torch | LargeHeart | chậu lửa 1 (x≈95) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:03.2 (tim lớn) |
| CE_T2 | 1 | Torch | MorningStar | chậu lửa 2 (x≈223) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:05.8 |
| CE_T3 | 1 | Torch | MorningStar | chậu lửa 3 (x≈351) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:08.9 |
| CE_T4 | 1 | Torch | LargeHeart | chậu lửa 4 (x≈479) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:12.1 |
| CE_T5 | 1 | Torch | Dagger | chậu lửa 5 (x≈607) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:14.5 |
| EH_C02 | 1 | WallCandle | Stopwatch | nến #2 (wiki x 96) — video 0:29.5 đồng hồ rơi ra ở x≈254 khi Zombie chết (15 đối chiếu) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6) |
| EH_C14 | 1 | WallCandle | HolyWater | nến #14 đỉnh cầu thang Bục B (wiki x 864; video rơi ở x≈799) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 0:57.3 |
| EH_C23 | 1 | WallCandle | Rosary | nến #23 Bục D (wiki x 1440) | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 1:23.5 (rosary rơi, nhặt 1:24.73) |
| EH_X63_7 | 1 | BreakableBlock | PurpleMoneyBag | khối phải nhất bục Panther 3 (ô lưới 06 (63,7)) | wiki:Secrets#Miscellaneous (Purple money bag 400) — video không phá |
| CA_X16_10 | 2 | BreakableBlock | PotRoast | tường khối nhỏ — khối thấp nhất (ô (16,10)) | video:Map_1 1:39.6 (roi khi ngồi, lộ thịt nướng NGAY); wiki:Secrets Pot Roast Stage 2 |
| CA_X16_9 | 2 | BreakableBlock | None | khối trên (ô (16,9)) — vỡ không ra gì | video:Map_1 1:40.55 (đập vỡ, không item) |
| CB_X29_6 | 2 | BreakableBlock | None | khối trên-phải (ô (29,6)) — phá để thả xuống, bí mật = ngồi trên khối thấp (SecretTrigger) | video:Map_1 2:19.05; wiki:Secrets Bonuses Stage 2 |
| ST2_C10 | 2 | WallCandle | Stopwatch | nến #10 tầng dưới | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6) |
| ST2_C13 | 2 | WallCandle | Rosary | nến #13 tầng trên-phải | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); video:Map_1 2:42.7 (rosary trên lối đi) |
| PB_C06 | 3 | WallCandle | Axe | nến #6 giữa cầu thang cuối | wiki:Stages 1-3 ảnh bản đồ (CV_WIKI_DATA §6); wiki:Stages 1-3 note 2; video:Map_1 3:09.5 |
| PB_POTION | 3 | WallCandle | InvisibilityPotion | nến giấu bình giữa cầu thang (wiki: ứng viên #3/#4) | wiki:Stages 1-3 note 1; video:Map_1 2:57.3 |
| PB_X43_9 | 3 | BreakableBlock | DoubleShot | khối PHẢI bệ chân thang phòng boss (ô (43,9)) | video:Map_1 3:17.62–3:18.0 (II rơi ra); wiki:Secrets Shot Multiplier Stage 3 |


