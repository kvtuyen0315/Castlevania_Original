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
| HeartsOnNewBlock | 5 |  | tim | video:Map_1 3:52.3 (HUD STAGE 04 tim 72→00 sau tally rồi 05 khi màn bản đồ lâu đài hiện; đo:routine 12 map1_hud.csv f6970) | tim đặt lại khi sang block mới; roi/vũ khí phụ/II giữ nguyên |
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

| Id (wiki) | Tên khác | Nhóm sheet | Loại | Stage | Đòn yếu | Đòn mạnh | Điểm | Chết được | Đạn | Tốc độ (px/tick) | Hành vi | Hành vi đã dựng / mức gần đúng (nghiệm thu CVGP 21) | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Ghoul | Zombie (lost souls) | enemy_zombie | Normal | 1, 3 | 1 | 1 | 100 | có | — | 0.875 | Đi thẳng một hướng qua màn rồi rời đi; nhóm 2–3 con từ cả hai phía (Stage 3 rơi từ trên xuống) | ĐỦ: đi thẳng qua màn, nhóm 3 mép phải, rơi gờ (St 3), chạm −2 · ĐO VIDEO Map_1 (lệch ≤ capture); sinh 1 phía theo video ≠ wiki "2 phía" (§4) | wiki:Enemies#Ghouls · *moveSpeed:* đo:routine 13 video Map_1 19 vết Zombie (EH f600–2541, PB f5129–5686): hồi quy 0.82–0.94, trung vị 0.878 ⇒ 224/256 |
| Bat | Black Bat / Red Bat | enemy_bat (Vampire Bat) | Normal | 2, 4, 10 | 1 | 1 | 200 | có | — | 1.125 | Dơi đen treo trần rồi lao xuống khi Simon lại gần; dơi đỏ bay sóng lên xuống | ĐỦ: dơi đỏ bay sóng (ĐO VIDEO, ≤ 1.5 px); dơi đen treo → sà (THEO WIKI, ước lượng; dùng sprite dơi đỏ — §5 #12); hất khỏi bệ nổi St 10: chưa thử (chưa có map) | wiki:Enemies#Bats · *moveSpeed:* đo:routine 13 video Map_1 7 vết dơi: 1.085–1.157 (TB 1.12) ⇒ 288/256, hướng về phía Simon |
| Panther | Black Leopard | enemy_leopard (Black Leopard) | Normal | 1 | 1 | 1 | 200 | có | — | 2.0 | Nằm chờ tới khi Simon vào tầm rồi nhảy vọt; rơi xuống tầng thấp và luôn quay về phía Simon | ĐỦ: nằm chờ → chạy 2 → nhảy khỏi gờ → quay về Simon 1 lần · ĐO VIDEO (báo #3 lệch 7.7 px — fix-04) | wiki:Enemies#Panthers · *moveSpeed:* đo:routine 13 video Map_1 chạy f1815–1856 (166 px/82 tick = 2.02) + nhảy ngang 1.90–2.06 |
| Raven |  | enemy_raven | Normal | 7, 8, 9 | 1 | 1 | 200 | có | — | 2.0 | Đậu chờ; cất cánh khi Simon lại gần; khựng một chút rồi bổ nhào; trượt thì quay lại đánh từ sau | ĐỦ: đậu → cất cánh \|dx\| < 96 → khựng → bổ nhào → quay lại lao tiếp · THEO WIKI, ước lượng (không video) | wiki:Enemies#Ravens · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; wiki Enemies#Ravens bổ nhào nhanh — tốc độ lao (≈ báo chạy 2.0 đo 13) |
| Eagle | giant Eagle | enemy_eagle | Normal | 11, 17 | 1 | 1 | 300 | có | — | 1.5 | Bay ngang mang Fleaman và thả xuống gần Simon; giết khi còn mang thì cả hai cùng chết | ĐỦ: bay ngang mang Fleaman, thả gần Simon, hạ khi mang ⇒ cả hai chết · THEO WIKI, ước lượng | wiki:Enemies#Eagles · *moveSpeed:* suy luận: ước lượng (không video) — routine 18; wiki Enemies#Eagles "bay ngang qua" — nhanh hơn dơi 1.125 (đo 13), chậm hơn báo 2.0 |
| Fishman | Fish Man | enemy_fishman (Fish Man) | Normal | 2, 10 | 1 | 1 | 300 | có | Cầu lửa (chỉ khi đã lên đất; video Map_1 không thấy bắn) | 0.75 | Nhảy lên từ mặt nước; chạm đất thì đi bộ và bắn cầu lửa | ĐỦ: trồi khỏi nước, đứng, đi 0.75 · ĐO VIDEO; bắn cầu lửa = QUY ƯỚC (video không có — §5 #25) | wiki:Enemies#Fishmen · *moveSpeed:* đo:routine 13 video Map_1 2 vết đi trên lối CB: 0.731 / 0.746 ⇒ 192/256 |
| MedusaHead | gorgon heads | enemy_medusahead | Normal | 5, 6, 8, 9, 15 | 1 | 1 | 300 | có | — | 1.0 | Xuất hiện từ phía Simon đang nhìn; bay ngang theo đường sóng; quay lưng thì tới từ cả hai phía | ĐỦ: ra từ phía Simon nhìn (quay lưng ⇒ phía kia), bay ngang sóng xuyên địa hình · THEO WIKI, ước lượng | wiki:Enemies#Medusa Heads · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; wiki Enemies#Medusa Heads bay ngang theo sóng — chọn 1.0 (giữa dơi 1.125 và Zombie 0.875) |
| Skeleton | White Skeleton | enemy_skeleton (White Skeleton) | Normal | 7, 8, 9, 13, 17 | 1 | 1 | 300 | có | Xương ném vòng cung (2–3 cái cùng lúc) | 1.0 | Nhanh và nhảy nhiều; ném xương theo vòng cung với các quỹ đạo khác nhau | ĐỦ: giữ khoảng cách, nhảy, ném 2–3 xương vòng cung · THEO WIKI, ước lượng | wiki:Enemies#Skeletons · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; wiki Enemies#Skeletons "nhanh" ⇒ nhanh hơn Zombie 0.875 — chọn 1.0 |
| Ghost | souls of lost men | enemy_ghost | Normal | 6, 7, 15 | 3 | 2 | 300 | có | — | 0.625 | Xuất hiện ở điểm cố định; chậm nhưng bám theo vị trí Simon từ góc lạ | ĐỦ: bám theo Simon, vượt quá rồi vòng lại, xuyên địa hình, 3/2 đòn · THEO WIKI, ước lượng | wiki:Enemies#Ghosts · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; wiki Enemies#Ghosts "chậm nhưng bám theo" — tốc độ trần khi đuổi (gia tốc chaseAccel) |
| RedSkeleton | enchanted skeleton | enemy_redskeleton | Normal | 13, 14, 15 | 1 | 1 | 400 | không | — | 0.5 | Đi chậm đuổi theo; bị đánh thì đổ thành đống xương rồi đứng dậy sau 1–2 s (không giết được) | ĐỦ: đi chậm đuổi, đổ 8 → đống xương 90 → ráp 8, không chết · THEO WIKI, ước lượng (+400 mỗi lần đổ — §5 #45) | wiki:Enemies#Red Skeletons · *moveSpeed:* suy luận: ước lượng (không video) — routine 18; wiki Stages 13-15 "chậm rãi đuổi theo" ⇒ bằng Spear Knight 0.5 (routine 17), chậm hơn Zombie 0.875 (đo 13) |
| SpearKnight | Black Knight | enemy_blackknight (Black Knight) | Normal | 4, 5 | 3 | 2 | 400 | có | — | 0.5 | Đi tới lui chậm; có thể quay đầu bất cứ lúc nào; chỉ gây hại khi va chạm | ĐỦ: tuần tra tới lui 0.5, quay ngẫu nhiên, chỉ va chạm, 3/2 đòn · THEO WIKI, ước lượng | wiki:Enemies#Spear Knights · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; wiki Enemies#Spear Knights "đi tới lui, chậm" ⇒ chậm hơn Zombie 0.875 (đo 13) — chọn 0.5 |
| BoneTower | Dragon Skull Cannon | enemy_dragonskull (Dragon Skull Cannon) | Normal | 6, 8, 9, 15 | 8 | 6 | 400 | có | 2 cầu lửa mỗi lượt (quất vỡ được) | — | Bất động; nhấp nháy rồi phun 2 cầu lửa theo chu kỳ | ĐỦ: bất động, nháy tím↔đỏ, 2 cầu lửa/lượt quất vỡ được (+100 suy luận) · THEO WIKI, ước lượng | wiki:Enemies#Bone Towers · *moveSpeed:* wiki:Enemies#Bone Towers (bất động) |
| Fleaman | Hunchback | enemy_fleaman (Hunchback/Fleaman) | Normal | 7, 11, 13, 14, 17 | 1 | 1 | 500 | có | — | 1.0 | Nhảy như ngẫu nhiên: thường 2 bước nhảy ngắn rồi 1 cú nhảy vọt; Stage 11/17 do Eagle thả | ĐỦ: chờ → nhảy ngắn-ngắn-vọt, Eagle thả · THEO WIKI, ước lượng | wiki:Enemies#Fleamen · *moveSpeed:* suy luận: ước lượng (không video) — routine 17; tốc độ ngang nhảy ngắn (nhảy vọt 2.0 ở ConfigActorParam) |
| BoneDragon | Skele-Dragon | enemy_skeledragon (Skele-Dragon) | Normal | 11, 12 | 8 | 7 | 1000 | có | Cầu lửa từ miệng theo nhiều góc | — | Gắn vào tường; thân bất động; cổ ngọ nguậy; phun cầu lửa (cả game chỉ 3 con) | ĐỦ: chân cổ bất động, cổ uốn sóng, 2 cầu lửa nhắm Simon, chỉ đầu nhận đòn, rơi 5–8 vật, nước thánh ⇒ multiplier · THEO WIKI, ước lượng; chưa thử gắn tường thật | wiki:Enemies#Bone Dragons · *hits:* wiki:Enemies#Bone Dragons (8/7 không đúng tỉ lệ 1.5× — giữ nguyên số wiki; CV_WIKI_DATA §7.1 #6) · *moveSpeed:* wiki:Enemies#Bone Dragons (gắn tường, thân bất động — chỉ cổ ngọ nguậy) |
| AxeKnight | Axe-Man | enemy_axeman (Axe-Man) | Normal | 14, 15 | 10 | 9 | 1400 | có | Rìu ném cao hoặc thấp rồi bay về như Boomerang (quất vỡ được) | 0.5 | Giữ khoảng cách; lùi khi Simon tiến; bị đẩy khỏi mép màn thì không quay lại | ĐỦ: giữ khoảng 64–112, rìu cao/thấp bay về như boomerang (quất vỡ), đẩy khỏi màn không quay lại · THEO WIKI, ước lượng | wiki:Enemies#Axe Knights · *hits:* wiki:Enemies#Axe Knights (10/9 không đúng tỉ lệ 1.5× — giữ nguyên số wiki; CV_WIKI_DATA §7.1 #6) · *moveSpeed:* suy luận: ước lượng (không video) — routine 18; wiki: lùi/tiến giữ khoảng cách, Simon đẩy được khỏi màn ⇒ chậm hơn Simon 1.0 (đo 08) — bằng Spear Knight 0.5 |
| PhantomBatMinor |  | boss_phantombat | Sub | 16 | 3 | 2 | 200 | có | Cầu lửa | — | 5 con ở Stage 16; bay tới theo vị trí khởi đầu và phun cầu lửa; nên tránh hơn là đánh | ĐỦ: chuyển động boss 14 thu nhỏ + cầu lửa khi lơ lửng · THEO WIKI, ƯỚC LƯỢNG cả máu/điểm (§5 #46); cầu lửa dùng sprite TẠM (§5 #47) | wiki:Bosses#Phantom Bat; wiki:Stages 16-18#Stage 16 · *hits:* suy luận: ước lượng (không video) — routine 18; wiki KHÔNG nêu: Stage 16 "1 đòn chỉ làm chậm" ⇒ > 1 đòn, "giết được nhưng không đáng" ⇒ ít hơn boss 12/8 — chọn như enemy nhiều đòn phổ biến (Ghost/Spear Knight 3/2); hỏi §5 · *score:* suy luận: ước lượng (không video) — routine 18; wiki KHÔNG nêu điểm: chọn bằng Bat 200 (cùng loài dơi); hỏi §5 · *moveSpeed:* routine 18: chuyển động = PhantomBat (đo 14) — tốc độ ở ConfigActorParam Enemy.PhantomBatMinor.* |
| Igor |  | boss_frankenstein (ô Igor) | Sub | 12 | — | — | — | không | Cầu lửa | *không rõ* | Bất tử; nhảy qua lại hai bên phòng và ném cầu lửa; quất 1 lần ⇒ choáng | ĐỦ: bất tử, ngồi vai → nhảy tiến/nhảy cao qua lại 2 góc, cầu lửa, quất ⇒ đứng hình 120 · THEO WIKI, ước lượng | wiki:Bosses#Frankenstein & Igor; wiki:Stages 10-12#Stage 12 · *moveSpeed:* không rõ — không có video (routine 19 ước lượng) |
| Snake |  | boss_medusa (ô rắn) | Sub | 6 | 1 | 1 | *không rõ* | có | — | 0.5 | Do Queen Medusa thả xuống; bò trên sàn về phía Simon | ĐỦ: Medusa thả, rơi rồi trườn 0.5 về phía Simon, roi ngồi 1 đòn · THEO WIKI, ước lượng (0 đ — §5 #51) | wiki:Bosses#Queen Medusa · *hits:* wiki:Bosses#Queen Medusa (chết bằng 1 đòn roi; đòn mạnh = 1 là suy luận) · *score:* không rõ — wiki không nêu điểm · *moveSpeed:* suy luận: ước lượng (không video) — routine 19; rắn 'trườn về phía Simon' — chậm như Spear Knight 0.5 |

## 3. Boss

| Boss | Pha | Stage | Số thực thể | Đòn yếu | Đòn mạnh | Đòn tính cho | Điểm | Stopwatch tác dụng | Điểm yếu | Đòn tấn công | Item trong phòng | Hành vi đã dựng / mức gần đúng (nghiệm thu CVGP 21) | Nguồn |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| PhantomBat | 1 | 3 | 1 | 12 | 8 | Single | 3000 | có | toàn thân | Bay tới điểm ngẫu nhiên; thỉnh thoảng bổ nhào về phía Simon; video: không bắn đạn | Double Shot (khối phải nhất ở chân cầu thang) | ĐỦ: ngủ → thức → lượn → bổ nhào → ngang → vọt → lượn → lơ lửng; khựng/bất tử; chuỗi chết → quả cầu · ĐO 2 VIDEO (lao đầu lệch 4–6 px; lọ nước thánh chưa xuyên — fix-01/04) | wiki:Bosses#Phantom Bat · *hits:* wiki:Bosses#Phantom Bat; video:Map_1 3:19–3:30 rìu −2 vạch/lần ⇒ 8 đòn mạnh hết 16 vạch (khớp) · *score:* wiki:Bosses#Phantom Bat; video:Map_1 3:30.17 +3000 (khớp) · *moveSpeed:* đo:routine 14 — tốc độ bay lượn (trục trội 0.72–0.77; 5 lần 2 video); các pha khác ở ConfigActorParam Boss.PhantomBat |
| QueenMedusa | 1 | 6 | 1 | 12 | 8 | Single | 3000 | có | toàn thân | Lao thẳng vào Simon (bay thấp); thả rắn bò trên sàn về phía Simon | Không | ĐỦ: tượng → vỡ → bay thấp sóng lao vào Simon, thả rắn; Stopwatch tác dụng · THEO WIKI, ước lượng; phòng thử ghép ô Block 1 | wiki:Bosses#Queen Medusa · *moveSpeed:* suy luận: ước lượng (không video) — routine 19; ngang khi bay lao về Simon (wiki: 'cố va vào Simon'; như Medusa Head 17 1.0); sóng dọc ở ConfigActorParam |
| MummyMan | 1 | 9 | 2 | 48 | 32 | TotalAllEntities | 3000 | không | toàn thân | Lang thang trong phòng; thỉnh thoảng ném băng gạc; kẹp Simon từ hai phía | Pot Roast (khối thấp nhất cầu thang trái) | ĐỦ: 2 con kẹp 2 phía, ném băng ngang (ngồi tránh), Stopwatch vô dụng · THEO WIKI, ước lượng; 48/32 = TỔNG (§5 #50) | wiki:Bosses#Mummy Men · *hits:* wiki:Bosses#Mummy Men (48/32 không nói tổng hay mỗi con — chọn TỔNG 2 con theo suy luận 48 = 2 × 24; §4) · *moveSpeed:* suy luận: ước lượng (không video) — routine 19; wiki Block 3 'boss chậm nhưng khoẻ' — chậm hơn Zombie 0.875 và Spear Knight 0.5 |
| Frankenstein | 1 | 12 | 2 | 24 | 16 | MainEntityOnly | 5000 | không | toàn thân (Monster) | Monster đi lang thang (thường nửa phải); Igor nhảy qua lại và ném cầu lửa | Không (Dagger ở ngoài phòng) | ĐỦ: Monster lang thang nửa phải + Igor; Monster chết ⇒ Igor biến mất · THEO WIKI, ước lượng | wiki:Bosses#Frankenstein & Igor · *hits:* wiki:Bosses#Frankenstein & Igor (chỉ Monster; Igor bất tử ⇒ ConfigEnemy.Igor) · *moveSpeed:* suy luận: ước lượng (không video) — routine 19; Monster 'chỉ đi lang thang' — chậm nhất (sải chân lớn, nặng nề) |
| Death | 1 | 15 | 1 | 24 | 16 | Single | 7000 | không | toàn thân | Lơ lửng ngẫu nhiên (ưa các góc); tạo tối đa 3 lưỡi hái đứng ~1 s rồi đuổi Simon và nhanh dần | Không | ĐỦ: bay ưu tiên 4 góc, ≤ 3 lưỡi hái đứng 1 s rồi đuổi nhanh dần, lưỡi hái còn sau khi chết · THEO WIKI, ước lượng | wiki:Bosses#Death · *moveSpeed:* suy luận: ước lượng (không video) — routine 20; wiki Stages 13-15: 'bản thân Death rất CHẬM' ⇒ chậm hơn Phantom Bat 0.75 (đo 14); trục trội khi bay tới đích — tham số khác ở ConfigActorParam Boss.Death |
| Dracula | 1 | 18 | 1 | 24 | 16 | Single | *không rõ* | không | chỉ đầu | Dịch chuyển tới chỗ ngẫu nhiên; mở áo choàng bắn 3 cầu lửa rồi biến mất | Holy Water (nến trái cùng) | ĐỦ: ẩn → dịch chuyển → hiện → mở áo 3 cầu lửa → biến mất; chỉ đầu nhận đòn; biến hình · THEO WIKI, ước lượng | wiki:Bosses#The Count · *hits:* wiki:Bosses#The Count (bảng không nói dạng nào — gán dạng 1; §4) · *score:* không rõ — wiki không nêu điểm riêng dạng 1 · *moveSpeed:* n/a — dạng 1 không đi, chỉ DỊCH CHUYỂN (wiki Bosses#The Count); nhịp ở ConfigActorParam Boss.Dracula.p1* (routine 20) |
| Dracula | 2 | 18 | 1 | 48 | 32 | Single | 50000 | không | chỉ đầu (chân không nhận sát thương) | Nhảy quanh phòng (cao/thấp); thỉnh thoảng phun lửa; Holy Water làm choáng nhưng không gây sát thương | — | ĐỦ: quỷ nhảy cao/thấp, phun lửa, đánh mặt, nước thánh choáng + đi xuyên; quả cầu cuối ⇒ OnGameCleared (credits CHƯA) · THEO WIKI, ước lượng | wiki:Bosses#The Count; wiki:Stages 16-18#Stage 18 · *hits:* wiki:Stages 16-18#Stage 18 ('32 lần' vào đầu; vũ khí không nêu — gán đòn MẠNH: lúc đó roi thường cấp 3); đòn yếu 48 = suy luận: tỉ lệ 3:2 của MỌI boss khác (wiki Bosses 12/8, 48/32, 24/16) — routine 20 chốt (§4) · *score:* wiki:Bosses#The Count — mâu thuẫn wiki:Gameplay#Points (Dracula không cho điểm); chọn Bosses (§4) · *moveSpeed:* n/a — dạng 2 chỉ NHẢY (wiki); vx/vy cú nhảy ở ConfigActorParam Boss.Dracula.p2* (routine 20) |

## 4. Tham số hành vi đo được (enemy / boss / vũ khí / item)

| Thực thể | Tham số | Giá trị | Min | Max | Đơn vị | Ghi chú | Nguồn |
|---|---|---|---|---|---|---|---|
| Enemy.Ghoul | groupSize | 3 | 2 | 3 | con | mỗi nhóm sinh lần lượt | đo:routine 13 video Map_1 7 nhóm đủ (EH f600–636, f800–837, f2058–2090, f2358–2390; PB f5397–5443, f5500–5533, f5621–5653) đều 3 con; wiki:Enemies#Ghouls nhóm 2–3 |
| Enemy.Bat | bobAmplitudePx | 8.75 | 8.3 | 9.5 | px | y = y0 + A(1 − cos 2πt/P): sinh ở đỉnh rồi đi XUỐNG | đo:routine 13 video Map_1 khớp sin 7 vết dơi (f2760–2804, f2951–2987, f3039–3072, f3174–3220, f4455–4485, f4648–4672, f4839–4880): biên 8.3–9.5 (TB 8.8), dư rms 0.34–0.43 px |
| Enemy.Bat | bobPeriodTicks | 100 | 94 | 107 | tick |  | đo:routine 13 cùng 7 vết: chu kỳ 94–106.5 (TB 99) |
| Enemy.Panther | jumpVx | 2.0 | 1.9 | 2.06 | px/tick | giữ suốt cú nhảy | đo:routine 13 video Map_1 2 cú nhảy (f1262–1288 hồi quy 1.90; f1768–1799 1.95; tổng 699→594 trong 51 tick = 2.06) |
| Enemy.Fishman | jumpVy0 | -5.875 | -5.95 | -5.5 | px/tick | âm = đi lên (NES y xuống); nhảy thẳng đứng, xuyên lối đi từ dưới | đo:routine 13 video Map_1 8 cú nhảy (f3517, f3519, f3589, f3606, f3868, f4096, f4115, f4218): đỉnh pivot NES 85–88 cố định ⇒ chọn vy0 để từ Y sinh 192 lên đỉnh Y 54 với g 0.125 (dư khớp parabol rms 1.6–3.9 px) |
| Enemy.Fishman | gravity | 0.125 | 0.119 | 0.139 | px/tick² |  | đo:routine 13 cùng 8 cú nhảy: g 0.119–0.139 (trung vị 0.124) |
| Enemy.Fishman | jumpApexPx | 107 | 104 | 107 | px | planner ghi 129 — sửa theo đo lại | đo:routine 13 đỉnh chân pivot NES 85–88 = 104–107 px trên mặt nước CB (NES 192) |
| Enemy.Fishman | jumpApexTicks | 47 | 46 | 47 | tick |  | đo:routine 13 vy0/g = 5.875/0.125 (planner ~46) |
| Enemy.Ghoul | walkFrameTicks | 17 | 16.6 | 17.6 | tick | bắt đầu ở zombie_0 | đo:routine 13 video Map_1 19 vết Zombie: khung zombie_0/1 đổi mỗi 16.6–17.4 tick (vết dài nhất 18 lần đổi = 17.00) |
| Enemy.Ghoul | fallGravity | 0.125 | 0.096 | 0.148 | px/tick² | rơi THẲNG ĐỨNG (x giữ nguyên), vy từ 0 | đo:routine 13 video Map_1 PB 4 lần Zombie bước khỏi gờ (f5202–5214, f5224–5232, f5573–5580, f5589–5594): g 0.096–0.148, vết sạch f5573 0.125 (dư 0.3 px) |
| Enemy.Ghoul | spawnInsetPx | 8 | 8 | 10 | px | mép phải: cx + 248; mép trái: cx + 8 | đo:routine 13 video Map_1 31 lần sinh: hiện NGUYÊN hình ở pivot màn 242–247 ngay frame đầu (vd f618, f633) ⇒ pivot = mép màn − 8 (sprite sát mép) |
| Enemy.Ghoul | groupGapTicks | 32 | 28 | 34 | tick |  | đo:routine 13 video Map_1 khoảng cách sinh trong nhóm (ngoại suy về pivot màn 248): 29.2/32.2/33.3/28.6/29.8/31.2/30.6/33.1/32.1/32.3/32.6/30.9 (trung vị 31.7; Zombie cách nhau ~28 px) |
| Enemy.Ghoul | waveDelayMinTicks | 115 |  |  | tick |  | đo:routine 13 video Map_1 từ con cuối nhóm trước tới con đầu nhóm sau: 115–288 tick (8 mẫu: 115/136/176/200/212/226/234/288) — ngẫu nhiên đều trong khoảng (suy luận: luật ROM chưa rõ) |
| Enemy.Ghoul | waveDelaySpanTicks | 173 |  |  | tick |  | đo:routine 13 cùng 8 mẫu: 288 − 115 |
| Enemy.Ghoul | maxAlive | 3 |  |  | con |  | đo:routine 13 video Map_1 tối đa 3 Zombie cùng lúc trong mọi vết (EH f2390, PB f5145) |
| Enemy.Ghoul | firstDelayTicks | 0 |  |  | tick |  | đo:routine 13 video Map_1 EH f600 và PB f5126: Zombie đầu có mặt ngay frame đầu section |
| Enemy.Ghoul | despawnInsetPx | 4 | 2 | 6 | px | gỡ khi pivot ra ngoài [cx + 4, cx + 252] | đo:routine 13 video Map_1 f2436–2447: Zombie đi ra mép trái còn thấy tới pivot màn ≈ 6, mất ở ≈ 4 (±2) |
| Enemy.Panther | triggerDistancePx | 70 | 69 | 72 | px | camera bám Simon ⇒ Simon.X = cx + 128 | đo:routine 13 video Map_1 2 lần bật dậy (f1263: \|dx\| 69; f1768: 68.5) + 1 lần KHÔNG bật ở \|dx\| 72 (f1644) ⇒ bật khi \|Simon.X − X\| < 70 |
| Enemy.Panther | jumpVy0 | -1.5 | -1.5 | -1.35 | px/tick | âm = lên; cất cánh khi mất sàn dưới pivot (mép gờ x 688 / 928) | đo:routine 13 video Map_1 khớp tự do 2 cú nhảy: vy0 −1.35/−1.48, g 0.107/0.125, đỉnh 8.5–8.8 px; chọn −1.5/0.125 (rms 1.55 / 0.79 px) |
| Enemy.Panther | gravity | 0.125 | 0.107 | 0.125 | px/tick² |  | đo:routine 13 cùng 2 cú nhảy |
| Enemy.Panther | runFrameTicks | 7 |  |  | tick |  | đo:routine 13 video Map_1 f1799–1856: chu kỳ leopard_2→1→3 dài 10.6 frame (21.2 tick) ⇒ 7 tick/khung |
| Enemy.Bat | flyFrameTicks | 9 | 8.5 | 9.5 | tick |  | đo:routine 13 video Map_1 7 vết: bat_red_0→1→2→1 đổi mỗi 4.5 frame (vết f2760–2804: 9 lần đổi trong 82 tick) |
| Enemy.Bat | spawnInsetPx | 8 | 8 | 15 | px |  | đo:routine 13 video Map_1 6 lần sinh: pivot màn 241–246 (mép phải) / 9–16 (mép trái) frame đầu |
| Enemy.Bat | spawnAboveFeetPx | 14 | 3 | 34 | px | biến thiên chưa giải thích (CVGP_MEMORY §6) | đo:routine 13 video Map_1 6 lần sinh: tâm dơi = chân Simon − 14 (3 lần), −21, −26, −34 — chọn mốt 14 |
| Enemy.Bat | edgeScreenMarginPx | 64 | 49 | 187 | px | khoảng 49–187 = x màn Simon ở các lần sinh | suy luận routine 13 từ 6 lần sinh: dơi ra từ mép TRƯỚC MẶT Simon (5/6); lần f3174 Simon quay trái ở x màn 49 ⇒ dơi ra từ mép phải ⇒ Simon cách mép trước < 64 px thì ra mép kia |
| Enemy.Bat | maxAlive | 1 |  |  | con |  | đo:routine 13 video Map_1 Crypt A: mọi lúc chỉ 1 dơi trên màn (7 vết không chồng thời gian) |
| Enemy.Bat | respawnDelayMinTicks | 194 |  |  | tick |  | đo:routine 13 video Map_1 dơi chết → dơi kế: 194/288/314/330 tick (f3076→3173, f2806→2950, f4491→4648, f4674→4839) — ngẫu nhiên đều trong khoảng (suy luận) |
| Enemy.Bat | respawnDelaySpanTicks | 136 |  |  | tick |  | đo:routine 13 cùng 4 mẫu: 330 − 194 |
| Enemy.Bat | firstDelayMinTicks | 8 |  |  | tick |  | đo:routine 13 video Map_1 vào Crypt A: dơi đầu sau 8 tick (f2756→2760) / 28 tick (f4441→4455) |
| Enemy.Bat | firstDelaySpanTicks | 20 |  |  | tick |  | đo:routine 13 cùng 2 mẫu: 28 − 8 |
| Enemy.Bat | perchTriggerPx | 64 |  |  | px | treo → Swoop → Fly | quy ước routine 13; routine 17 giữ 64 — suy luận: ước lượng (không video Stage 4 — Black Bat wiki Enemies#Bats): treo tới khi \|dx\| < 64 rồi Swoop (sà xuống swoopVy tới chân Simon − swoopAboveFeetPx) ⇒ bay sóng như dơi đỏ |
| Enemy.Fishman | walkSpeed | 0.75 | 0.73 | 0.75 | px/tick | đi về phía Simon (chọn hướng lúc đáp) | đo:routine 13 video Map_1 2 vết đi trên lối CB (f3553–3575 −0.731, f4256–4281 +0.746; dư rms 0.66/0.86 px) |
| Enemy.Fishman | spawnBelowSurfacePx | 32 |  |  | px |  | suy luận routine 13: Fish Man sinh ở đáy section (CB y 192 = mặt nước 160 + 32, ẩn) — chọn để đỉnh nhảy khớp video với vy0/g đo được |
| Enemy.Fishman | hiddenBelowSurfacePx | 16 | 10 | 18 | px |  | đo:routine 13 video Map_1 f3515/3517: chưa thấy khi chân dưới mặt nước ~18 px, thấy khi −3 (f3517) / +8 (f4218) ⇒ ẩn khi chân thấp hơn mặt nước ≥ 16 |
| Enemy.Fishman | gridStepPx | 64 |  |  | px |  | đo:routine 13 video Map_1 12 lần nhảy: x thế giới ∈ {32, 96, 160, 224, 288, 352, 416} (±2) = 32 + 64k |
| Enemy.Fishman | gridOffsetPx | 32 |  |  | px |  | đo:routine 13 cùng 12 lần nhảy |
| Enemy.Fishman | minPlayerDistPx | 32 |  |  | px |  | suy luận routine 13: 12 lần nhảy \|x − Simon.X\| = 35–178 (không lần nào < 32) ⇒ bỏ cột sát Simon |
| Enemy.Fishman | maxAlive | 2 |  |  | con |  | đo:routine 13 video Map_1 tối đa 2 Fish Man cùng lúc (f3517 hai con nhảy cùng lúc; f3589–3606) |
| Enemy.Fishman | respawnDelayMinTicks | 44 |  |  | tick | mỗi suất (maxAlive) có đồng hồ riêng: f3517 2 con cùng lúc | đo:routine 13 video Map_1 Fish Man bị gỡ → con kế: 44/58/80/156/176 tick (f3576→3598, f3827→3856, f3549→3589, f4140→4218, f4282→4370) — ngẫu nhiên đều (suy luận) |
| Enemy.Fishman | respawnDelaySpanTicks | 132 |  |  | tick |  | đo:routine 13 cùng 5 mẫu: 176 − 44 |
| Enemy.Fishman | firstDelayTicks | 0 |  |  | tick |  | quy ước routine 13 — f3517 (2 con) là lần đầu thấy được sau khi vào CB nhưng 1:47.8–1:56.7 bị non-game che ⇒ không đo được |
| Enemy.Fishman | standTicks | 8 | 6 | 10 | tick |  | đo:routine 13 video Map_1 đáp lối đi rồi đứng 3–5 frame mới đi (f3550→3554, f4253→4258) |
| Enemy.Fishman | walkFrameTicks | 32 | 32 | 34 | tick |  | đo:routine 13 video Map_1 fishman_0 → fishman_1 sau 32/34 tick kể từ lúc đáp (f3550→3566, f4253→4270); khung sau giả định cùng nhịp |
| Enemy.Fishman | firstShotTicks | 96 |  |  | tick | wiki Enemies#Fishmen: bắn cầu lửa khi đã lên đất | quy ước routine 13 — video KHÔNG có cảnh bắn (mọi Fish Man bị hạ ≤ 50 tick sau khi đáp) ⇒ bắn lần đầu sau 96 tick trên đất |
| Enemy.Fishman | shotEveryTicks | 160 |  |  | tick |  | quy ước routine 13 (như trên) |
| Enemy.Fishman | shootPoseTicks | 24 |  |  | tick |  | quy ước routine 13 — tư thế fishman_2 (suy luận từ hình) |
| Enemy.Fishman | fireballSpeed | 2.0 |  |  | px/tick |  | quy ước routine 13 — không có video |
| Enemy.Fishman | fireballHeightAboveFeetPx | 22 |  |  | px |  | quy ước routine 13 — tâm cầu lửa ngang miệng sprite fishman_2 |
| Enemy.Fishman | bubbleCenterVy | -0.5 |  |  | px/tick | bọt giữa: sinh mặt nước − 27, lệch x +2; 2 bọt bên: mặt nước − 12, lệch x ∓3, vy 0 | đo:routine 13 video Map_1 1 lần trồi f3517–3539: bọt giữa lên ~1 px rồi rơi — xấp xỉ |
| Enemy.Fishman | bubbleSideVx | 0.375 | 0.3 | 0.4 | px/tick |  | đo:routine 13 cùng lần trồi: 2 bọt bên trôi ±9 px / 24 tick — xấp xỉ |
| Enemy.Fishman | bubbleCenterVx | 0.25 |  |  | px/tick |  | đo:routine 13 cùng lần trồi: bọt giữa +7 px / 34 tick — xấp xỉ |
| Enemy.Fishman | bubbleGravity | 0.125 |  |  | px/tick² |  | đo:routine 13 cùng lần trồi: 2 bọt bên rơi 33 px trong 24 tick — xấp xỉ |
| Enemy.Fishman | bubbleSideTicks | 24 |  |  | tick |  | đo:routine 13 cùng lần trồi (f3519→3531) |
| Enemy.Fishman | bubbleCenterTicks | 34 |  |  | tick |  | đo:routine 13 cùng lần trồi (f3522→3539) |
| Enemy.RedSkeleton | reviveTicks | 90 | 60 | 120 | tick | nằm thành đống xương | wiki:Enemies#Red Skeletons; wiki:Stages 13-15 ("một hai giây" ⇒ 60–120 tick) — routine 18 chọn giữa khoảng 90 (suy luận: ước lượng, không video) |
| Boss.PhantomBat | sleepX | 623 |  |  | px section | x pivot (tâm khung bay) | đo:routine 14 khớp mẫu phantombat_0: tâm màn (111,65) cả 2 video (boss f5–65, Map_1 f5942) + cx 512 |
| Boss.PhantomBat | sleepY | 41 |  |  | px section | pivot = tâm khung bay; khung ngủ vẽ cao hơn sleepDrawUpPx | đo:routine 14: khung bay lúc thức tâm màn (111,73) / (110,73) ⇒ y section 73 − 32 |
| Boss.PhantomBat | sleepDrawUpPx | 8 |  |  | px |  | đo:routine 14: tâm khung ngủ (111,65) cao hơn tâm khung bay (111,73) 8 px — đỉnh 2 khung trùng nhau (y 57) |
| Boss.PhantomBat | wakeSimonX | 693 | 693 | 696 | px section | Simon.X ≥ ngưỡng ⇒ thức | đo:routine 14 simon_fit: Map_1 dơi thức f5984 khi Simon x màn 180→182 (f5983/5984); video boss Simon tới 181 ở f57, 184 ở f60, dơi thức f66 (trễ ~12–18 tick chưa giải thích) |
| Boss.PhantomBat | wakeHoverTicks | 33 | 31 | 35 | tick | xong ⇒ nhạc boss + cú lao đầu | đo:routine 14: vỗ cánh tại chỗ từ lúc thức tới lúc bắt đầu lao — boss f66→f83.5 (35), Map_1 f5984→f5999.5 (31) |
| Boss.PhantomBat | swoopVy0 | 1.46 | 1.40 | 1.52 | px/tick | dọc giảm đều về 0 ở đáy | đo:routine 14 fitdive (5 cú lao: y = y0 + d(2s − s²), s = t/T, rms y 0.4–1.1 px): vy đầu = 2d/T = 1.52 / 1.40 / 1.52 / 1.47 / 1.41 |
| Boss.PhantomBat | swoopDepthPx | 41 | 41 | 44 | px |  | đo:routine 14 fitdive: d = 41 (boss lao đầu), 42 (Map_1 lao đầu), 41 (boss lao 2), 44 (Map_1 lao); lao 3 boss 31 = chạm swoopMaxY |
| Boss.PhantomBat | swoopMaxY | 156 |  |  | px section | đáy không thấp hơn | đo:routine 14: đáy cú lao 3 boss (từ y màn 157) dừng ở y màn 188 (= sàn 208 − 20) |
| Boss.PhantomBat | swoopVxMin | 1.22 |  |  | px/tick | vx = (Simon.X − x0)/T kẹp | đo:routine 14 fitdive: vx nhỏ nhất 5 cú lao (Map_1 lao đầu 1.222) |
| Boss.PhantomBat | swoopVxMax | 2.1 |  |  | px/tick |  | đo:routine 14 fitdive: vx lớn nhất (boss lao 3 2.10; lao 4 vào lửa 2.0) |
| Boss.PhantomBat | sweepTicks | 12 | 8 | 14 | tick |  | đo:routine 14: bay ngang sau đáy trước khi vọt lên — boss lao 2 f246→f250–253, Map_1 f6201→f6206 |
| Boss.PhantomBat | riseAccel | 0.039 | 0.033 | 0.042 | px/tick² | vx giữ nguyên | đo:routine 14: vọt lên y = y0 − a·t²/2 — Map_1 f6206–6230 0.039, boss f369–398 0.033, boss f251–274 0.042 |
| Boss.PhantomBat | riseTopY | 88 |  |  | px section | ⇒ bay lượn | đo:routine 14: hết vọt lên ở y màn 120 cả 3 lần (boss (188,120) f274, (118,120) f399; Map_1 (192,120) f6230) |
| Boss.PhantomBat | screenMinX | 18 |  |  | px màn | đổi chiều ngang | suy luận: đối xứng screenMaxX (video không có cảnh chạm mép trái) |
| Boss.PhantomBat | screenMaxX | 238 | 237 | 238 | px màn | đổi chiều ngang | đo:routine 14: video boss quay đầu ở x màn 237 (f259) và 238 (f369) |
| Boss.PhantomBat | glideSpeed | 0.75 | 0.72 | 0.77 | px/tick | trục kia tỉ lệ | đo:routine 14: bay thẳng tới điểm, TRỤC TRỘI 0.75 (5 lần: vx −0.755 / −0.756 / −0.767, vx 0.723, vy 0.740) |
| Boss.PhantomBat | glideMinScreenX | 24 |  |  | px màn |  | suy luận: điểm tới video x màn 26–211 (5 lần); biên = nửa sải cánh 24 |
| Boss.PhantomBat | glideMaxScreenX | 232 |  |  | px màn |  | suy luận: đối xứng glideMinScreenX |
| Boss.PhantomBat | glideLowY | 92 | 92 | 93 | px section |  | đo:routine 14: điểm tới sau cú lao ĐẦU y màn 124 (boss) / 125 (Map_1) |
| Boss.PhantomBat | glideHighY | 123 | 121 | 125 | px section |  | đo:routine 14: điểm tới sau khi vọt lên y màn 157 / 156 (boss), 153 (Map_1) |
| Boss.PhantomBat | glideMinSimonDx | 64 |  |  | px | x ngẫu nhiên đều phần hợp lệ | suy luận: 5/5 điểm tới cách Simon 68–180 px (video) |
| Boss.PhantomBat | hoverMinTicks | 32 |  |  | tick | ngẫu nhiên đều | đo:routine 14: lượn tại chỗ 33 (boss f316–332), 55 (Map_1 trừ 2 lần khựng), 59 (boss f430–459), 122 (boss f157–218) |
| Boss.PhantomBat | hoverMaxTicks | 124 |  |  | tick |  | đo:routine 14 (xem hoverMinTicks) |
| Boss.PhantomBat | diveChancePct | 100 |  |  | % |  | video:4/4 lần hết lượn tại chỗ đều bổ nhào (wiki: thỉnh thoảng) |
| Boss.PhantomBat | hitStunTicks | 10 | 8 | 14 | tick | khung hurt | đo:routine 14: Map_1 dơi đứng yên khi trúng rìu f6179–6184, f6191–6197, f6247–6252 (≈ 5 frame) |
| Boss.PhantomBat | invincibleTicks | 16 |  |  | tick |  | suy luận: video boss hạ dơi (16 vạch) trong 130 tick (f469–f534) bằng roi xích + lửa nước thánh — lửa đánh lại mỗi 8 tick thì cần bất tử 12–19 tick |
| Boss.PhantomBat | deathFxTicks | 62 | 60 | 62 | tick |  | đo:routine 14 boss_fit fx: cụm lửa bossdeath_m 7 khung (0,1,2,0,1,2,0) — Map_1 f6306–6336 (62), boss f534–563 (60) |
| Boss.PhantomBat | deathFxUpPx | 8 |  |  | px |  | đo:routine 14: tâm cụm lửa (211,145) / (204,184) = tâm dơi lúc chết (211,153) / (204,192) − 8 |
| Boss.Death | maxScythes | 3 |  |  | cái |  | wiki:Bosses#Death |
| Boss.Death | scytheIdleTicks | 60 |  |  | tick |  | wiki:Bosses#Death (đứng yên khoảng 1 giây × 60) |
| Boss.Death | hitStunTicks | 10 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; như Phantom Bat (đo 14) — khung chung BossBase |
| Boss.Death | invincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; như Phantom Bat (suy luận 14) |
| Boss.Death | deathFxTicks | 62 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; cụm lửa bossdeath_m như Phantom Bat (đo 14) |
| Boss.Death | deathFxUpPx | 0 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; pivot Death = tâm sprite 40×48 ⇒ lửa ở tâm |
| Boss.Death | wakeDelayTicks | 32 |  |  | tick | từ lúc sinh (vào phòng) tới khi va chạm + bay | suy luận: ước lượng (không video) — routine 20; wiki: Holy Water giết được Death 'trước khi nó kịp vào vị trí' ⇒ có khoảng hiện ra rồi mới bay vào góc; chọn ~0.5 s |
| Boss.Death | firstCorner | 1 |  |  | chỉ số | 0 trên-trái 1 trên-phải 2 dưới-trái 3 dưới-phải | suy luận: ước lượng (không video) — routine 20; wiki: ném Holy Water lên BỤC TRÊN-PHẢI giết nó trước khi nó tấn công ⇒ vị trí đầu = góc trên-phải |
| Boss.Death | cornerXMinScreen | 40 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 20; Death rộng 40 ⇒ mép trái thân 20 px trong màn |
| Boss.Death | cornerXMaxScreen | 216 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 20; đối xứng cornerXMinScreen |
| Boss.Death | cornerYHigh | 88 |  |  | px section | góc trên | suy luận: ước lượng (không video) — routine 20; đáy thân (y + 24) = mặt bục trên y 112 phòng thử ⇒ lửa nước thánh trên bục chạm được (wiki) |
| Boss.Death | cornerYLow | 152 |  |  | px section | góc dưới | suy luận: ước lượng (không video) — routine 20; đáy thân = sàn 176 ⇒ 'Death có thể bay tới chỗ bạn nấp ở góc' (wiki) |
| Boss.Death | cornerPct | 75 |  |  | % | còn lại 1 điểm ngẫu nhiên trong vùng | suy luận: ước lượng (không video) — routine 20; wiki: 'bay ngẫu nhiên, ưu tiên các góc' — 3/4 lần tới 1 góc khác góc hiện tại |
| Boss.Death | roamYMin | 64 |  |  | px section |  | suy luận: ước lượng (không video) — routine 20; vùng điểm ngẫu nhiên: giữa góc trên (88) và mép HUD |
| Boss.Death | roamYMax | 152 |  |  | px section |  | suy luận: ước lượng (không video) — routine 20; = cornerYLow |
| Boss.Death | hoverMinTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; dừng đủ lâu để gọi 3 lưỡi hái (16 + 2×32 = 80) — wiki 'ném khi nó dừng' |
| Boss.Death | hoverSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; ngẫu nhiên đều [96, 160] |
| Boss.Death | summonFirstTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; gọi lưỡi hái ngay sau khi dừng (wiki: lưỡi hái là đòn duy nhất) |
| Boss.Death | summonGapTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; 3 lưỡi hái trong ~1 s dừng |
| Boss.Death | scytheSpawnDx | 24 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; sinh ngay cạnh thân (40 rộng), luân phiên phía Simon trước — wiki 'phá lưỡi hái ngay khi vừa tạo' |
| Boss.Death | scytheSpawnDy | 0 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; ngang tâm thân |
| Boss.Death | scytheAccel | 0.015625 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 20; mỗi trục 1/64 về Simon như Ghost (17) — 'chậm rãi đuổi theo' |
| Boss.Death | scytheCapStart | 0.5 |  |  | px/tick | trần tốc độ mỗi trục khi bắt đầu đuổi | suy luận: ước lượng (không video) — routine 20; 'lúc đầu chậm' (nửa tốc độ Simon đi) |
| Boss.Death | scytheCapGrow | 0.00390625 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 20; 'rồi nhanh dần' — trần +1/256 mỗi tick (0.5 → 1.5 trong 256 tick) |
| Boss.Death | scytheCapMax | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; nhanh hơn Simon đi (1.0) ⇒ phải phá, không chạy thoát mãi |
| Boss.Death | scytheAimAbovePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; nhắm tâm thân Simon (như 18/19) |
| Boss.Dracula | fireballsPerVolley | 3 |  |  | cái |  | wiki:Bosses#The Count |
| Boss.Dracula | hitStunTicks | 10 |  |  | tick | cả 2 dạng | suy luận: ước lượng (không video) — routine 20; như Phantom Bat (đo 14) — khung chung BossBase |
| Boss.Dracula | invincibleTicks | 16 |  |  | tick | cả 2 dạng | suy luận: ước lượng (không video) — routine 20; như Phantom Bat (suy luận 14) |
| Boss.Dracula | deathFxTicks | 62 |  |  | tick | lửa biến hình + chết dạng 2 | suy luận: ước lượng (không video) — routine 20; cụm lửa bossdeath_m như Phantom Bat (đo 14) |
| Boss.Dracula | deathFxUpPx | 32 |  |  | px | dạng 2 | suy luận: ước lượng (không video) — routine 20; tâm thân quỷ 48×64 (chân − 32) |
| Boss.Dracula | aimAbovePx | 16 |  |  | px | quả giữa của loạt | suy luận: ước lượng (không video) — routine 20; nhắm tâm thân Simon (như 18/19) |
| Boss.Dracula | fireballSpeed | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; như cầu lửa Bone Dragon/Igor (18/19) |
| Boss.Dracula | fireSpreadDeg | 14 |  |  | độ | quay ±(cos/sin ×256 = 248/62) | suy luận: ước lượng (không video) — routine 20; wiki: 'nhảy quá sớm thì 3 cầu lửa bắn CHẾCH LÊN … bay qua đầu' / 'bắn quá thấp để nhảy qua' ⇒ loạt 3 quả nhắm Simon, xoè nhẹ (≈ ±10 px ở 40 px) |
| Boss.Dracula | fireballHeartPct | 25 |  |  | % | tim nhỏ | suy luận: ước lượng (không video) — routine 20; wiki Stages 16-18: cầu lửa bị phá 'đôi khi' sinh tim |
| Boss.Dracula | p1FirstDelayTicks | 60 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; wiki: 'chờ Dracula hiện ra' — 1 s sau khi vào phòng |
| Boss.Dracula | p1HiddenMinTicks | 48 |  |  | tick | ngẫu nhiên đều [48, 96] | suy luận: ước lượng (không video) — routine 20; giữa 2 lần hiện |
| Boss.Dracula | p1HiddenSpanTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; (xem p1HiddenMinTicks) |
| Boss.Dracula | p1AppearTicks | 32 |  |  | tick | nhấp nháy, không va chạm | suy luận: ước lượng (không video) — routine 20; wiki: 'đứng đúng tầm roi khi ông ta HIỆN HÌNH HOÀN TOÀN' ⇒ có pha hiện dần không đánh được |
| Boss.Dracula | p1FlickerTicks | 2 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; nhấp nháy hiện/ẩn mỗi 2 tick (như nháy bất tử) |
| Boss.Dracula | p1StandTicks | 24 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; wiki: 'mở áo choàng TRƯỚC khi bắn' — đứng khép áo ~0.4 s |
| Boss.Dracula | p1FireDelayTicks | 8 |  |  | tick | tính từ tick mở áo | suy luận: ước lượng (không video) — routine 20; bắn ngay sau khi áo mở |
| Boss.Dracula | p1CapeTicks | 40 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; áo mở giữ thêm sau loạt bắn (wiki: 'nhảy qua cầu lửa rồi quất vào đầu' ⇒ còn ở lại sau khi bắn) |
| Boss.Dracula | p1CloseTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; khép áo trước khi biến mất |
| Boss.Dracula | p1VanishTicks | 24 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; biến mất dần (nhấp nháy, không va chạm) |
| Boss.Dracula | p1OnSimonPct | 50 |  |  | % | x = Simon; còn lại ngẫu nhiên | suy luận: ước lượng (không video) — routine 20; wiki: 'nhiều khả năng hiện NGAY TRÊN ĐẦU bạn nếu đứng yên' |
| Boss.Dracula | p1MinScreenX | 32 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 20; trong màn (thân 24–32 rộng) |
| Boss.Dracula | p1MaxScreenX | 224 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 20; đối xứng p1MinScreenX |
| Boss.Dracula | p1FireAbovePx | 40 |  |  | px | điểm sinh cầu lửa | suy luận: ước lượng (không video) — routine 20; ngực — chỗ áo choàng mở (dracula1_1 hàng ~19) |
| Boss.Dracula | p1HeadFlyVy | -1.5 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 20; wiki: 'hết vạch máu cuối ⇒ ĐẦU BAY RA' — bay thẳng lên khỏi màn |
| Boss.Dracula | p1HeadFlyVx | 0.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; lùi về sau lưng khi bay lên |
| Boss.Dracula | p1TransformTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; 'thân biến thành quỷ' — thân không đầu đứng 1.6 s rồi bùng lửa ra dạng 2 |
| Boss.Dracula | p1HeadlessFlipTicks | 8 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; thân không đầu đổi áo khép/mở (2 khung sheet) |
| Boss.Dracula | p2EmergeTicks | 48 |  |  | tick | không va chạm | suy luận: ước lượng (không video) — routine 20; quỷ đứng trong lửa biến hình (62) rồi mới đánh/nhận đòn |
| Boss.Dracula | p2StandMinTicks | 32 |  |  | tick | ngẫu nhiên đều [32, 64] | suy luận: ước lượng (không video) — routine 20; đứng giữa 2 hành động |
| Boss.Dracula | p2StandSpanTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; (xem p2StandMinTicks) |
| Boss.Dracula | p2FirePct | 33 |  |  | % |  | suy luận: ước lượng (không video) — routine 20; wiki: 'THỈNH THOẢNG phun lửa' — 1/3 lần, còn lại nhảy |
| Boss.Dracula | p2FireWindupTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; há miệng rồi phun |
| Boss.Dracula | p2FireHoldTicks | 40 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; giữ tư thế phun |
| Boss.Dracula | p2FireAbovePx | 39 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; miệng: khung đứng hàng 20–30 cột 7–24 ⇒ tâm (−8, −39) |
| Boss.Dracula | p2MouthDx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 20; (xem p2FireAbovePx) — phía trước mặt |
| Boss.Dracula | p2CrouchTicks | 12 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; ngồi lấy đà (khung dracula2_5) |
| Boss.Dracula | p2LandTicks | 12 |  |  | tick |  | suy luận: ước lượng (không video) — routine 20; co chân khi đáp (khung dracula2_4) |
| Boss.Dracula | p2HighPct | 50 |  |  | % |  | suy luận: ước lượng (không video) — routine 20; wiki: 'nhảy CAO hoặc THẤP' |
| Boss.Dracula | p2HighVy0 | -5.5 |  |  | px/tick | g p2Gravity | suy luận: ước lượng (không video) — routine 20; cao ~58 px (đáy thân cao hơn đầu Simon ~30 tick ⇒ 'chạy chui dưới khi nó nhảy cao') |
| Boss.Dracula | p2HighVx | 2.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; ~88 px mỗi cú cao — vượt qua Simon |
| Boss.Dracula | p2LowVy0 | -3.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; thấp ~17 px — không chui qua được |
| Boss.Dracula | p2LowVx | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 20; ~36 px mỗi cú thấp |
| Boss.Dracula | p2Gravity | 0.25 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 20; như rìu Simon (đo 09) / Igor (19) |
| Boss.Dracula | p2MinScreenX | 32 |  |  | px màn | biên ngang khi nhảy | suy luận: ước lượng (không video) — routine 20; thân 48 rộng ⇒ mép 8 px trong màn |
| Boss.Dracula | p2MaxScreenX | 224 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 20; đối xứng p2MinScreenX |
| Boss.QueenMedusa | hitStunTicks | 10 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (đo 14) — khung chung BossBase |
| Boss.QueenMedusa | invincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (suy luận 14) |
| Boss.QueenMedusa | deathFxTicks | 62 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; cụm lửa bossdeath_m như Phantom Bat (đo 14) |
| Boss.QueenMedusa | deathFxUpPx | 0 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; pivot = tâm đầu ⇒ lửa ở tâm sprite |
| Boss.QueenMedusa | wakeDelayTicks | 60 |  |  | tick | từ lúc sinh (vào phòng) | suy luận: ước lượng (không video) — routine 19; wiki: vỡ ra từ tượng 'không báo trước' khi Simon đã vào phòng — chọn 1 s |
| Boss.QueenMedusa | statueHeadUpPx | 32 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; sheet: tượng medusa_bust 32×48 — đầu 32×32 hiện ở đỉnh tượng (tâm cao hơn chân tượng 48 − 16) |
| Boss.QueenMedusa | emergeTicks | 32 |  |  | tick | đã nhận/gây đòn | suy luận: ước lượng (không video) — routine 19; thời gian vỡ tượng (nháy đá ↔ sống) — như Bone Tower nháy 17 |
| Boss.QueenMedusa | emergeFlashTicks | 4 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; nhịp nháy medusa_stone ↔ medusa_head như Bone Tower 17 |
| Boss.QueenMedusa | flyAboveFloorPx | 32 |  |  | px | tâm sóng so với sàn phòng | suy luận: ước lượng (không video) — routine 19; wiki: 'hầu như luôn bay thấp sát đất' (Holy Water dễ trúng) — tâm sóng ngang ngực Simon |
| Boss.QueenMedusa | flyAmplitudePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; đáy sóng chạm chân (lao vào Simon), đỉnh sóng qua đầu Simon đứng |
| Boss.QueenMedusa | flyPeriodTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như sóng Medusa Head 17 (cùng 'đầu Medusa') |
| Boss.QueenMedusa | overshootPx | 32 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; wiki: 'cố va vào Simon' — vượt qua Simon 32 px rồi quay lại lao tiếp |
| Boss.QueenMedusa | screenMinX | 16 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; quay đầu khi chạm mép (nửa bề ngang đầu 16) |
| Boss.QueenMedusa | screenMaxX | 240 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; đối xứng screenMinX |
| Boss.QueenMedusa | snakeFirstTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'khi bay thả rắn' — rắn đầu sau 0.8 s bay |
| Boss.QueenMedusa | snakeMinTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; nhịp thả rắn (wiki không nêu) — như nghỉ Bone Tower 17 |
| Boss.QueenMedusa | snakeSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; ngẫu nhiên đều [96, 160] |
| Boss.QueenMedusa | maxSnakes | 2 |  |  | con |  | suy luận: ước lượng (không video) — routine 19; giới hạn rắn sống cùng lúc (wiki không nêu) |
| Boss.QueenMedusa | snakeDropBelowPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; rắn rơi ra từ dưới tâm đầu |
| Boss.MummyMan | hitStunTicks | 10 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (đo 14) |
| Boss.MummyMan | invincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (suy luận 14) |
| Boss.MummyMan | deathFxTicks | 62 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; cụm lửa bossdeath_m như Phantom Bat (đo 14) |
| Boss.MummyMan | deathFxUpPx | 20 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tâm thân sprite 16×48 (đục hàng 8–47) |
| Boss.MummyMan | wakeDistancePx | 128 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; thức (cả 2) khi Simon cách con gần nhất < nửa màn |
| Boss.MummyMan | partnerDxPx | 40 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; con thứ hai sinh cách con đầu 40 px về bên trái |
| Boss.MummyMan | decideMinTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'lang thang' — đổi ý mỗi 0.8–1.9 s |
| Boss.MummyMan | decideSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; ngẫu nhiên đều [48, 112] |
| Boss.MummyMan | keepDxPx | 40 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; wiki: 'kẹp Simon từ hai phía' — mỗi con nhắm 1 bên Simon cách 40 px |
| Boss.MummyMan | wanderPct | 25 |  |  | % |  | suy luận: ước lượng (không video) — routine 19; wiki: 'đi lang thang' — 1/4 lần chọn hướng ngẫu nhiên |
| Boss.MummyMan | standDxPx | 4 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tới đích ±4 px thì đứng |
| Boss.MummyMan | throwFirstTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'thỉnh thoảng ném băng gạc' — lần đầu sau ~1 s |
| Boss.MummyMan | throwMinTicks | 128 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; thỉnh thoảng: mỗi con 2.1–3.7 s |
| Boss.MummyMan | throwSpanTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; ngẫu nhiên đều [128, 224] |
| Boss.MummyMan | throwRangePx | 160 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; chỉ ném khi Simon trong 160 px |
| Boss.MummyMan | throwPoseTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; đứng lại trước khi ném (như tư thế ném Axe Knight 18) |
| Boss.MummyMan | bandageAbovePx | 28 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tâm băng ở chân − 28 ⇒ hộp băng hàng chân−31..chân−23: chồng hộp Simon ĐỨNG (−30..−1), KHÔNG chồng Simon NGỒI (−22..0) — wiki 'ngồi quất khi chúng áp sát' |
| Boss.MummyMan | bandageSpeed | 1 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; băng bay ngang chậm như cầu lửa (1–1.5) |
| Boss.MummyMan | bandageFrontPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; băng sinh trước thân nửa sprite |
| Boss.Frankenstein | hitStunTicks | 10 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (đo 14) |
| Boss.Frankenstein | invincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như Phantom Bat (suy luận 14) |
| Boss.Frankenstein | deathFxTicks | 62 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; cụm lửa bossdeath_m như Phantom Bat (đo 14) |
| Boss.Frankenstein | deathFxUpPx | 24 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tâm thân sprite 16×48 |
| Boss.Frankenstein | wakeDelayTicks | 16 |  |  | tick | từ lúc sinh (vào phòng) | suy luận: ước lượng (không video) — routine 19; wiki: vào phòng chạy lên bục phải đánh vài đòn trước khi Igor đi ⇒ Monster nhận đòn gần như ngay |
| Boss.Frankenstein | zoneMinScreenX | 128 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; wiki: Monster 'thường ở nửa phải màn hình' |
| Boss.Frankenstein | zoneMaxScreenX | 216 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; tới sát bục góc phải (phòng thử: bục x 224–255) |
| Boss.Frankenstein | decideMinTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'chỉ đi lang thang' — đổi ý mỗi 1.1–2.1 s |
| Boss.Frankenstein | decideSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; ngẫu nhiên đều [64, 128] |
| Boss.Frankenstein | chasePct | 75 |  |  | % |  | suy luận: ước lượng (không video) — routine 19; wiki: Monster 'có thể chắn đường' — 3/4 lần nhắm x Simon (kẹp trong vùng), còn lại điểm ngẫu nhiên |
| Boss.Frankenstein | standDxPx | 4 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tới đích ±4 px thì đứng |
| Enemy.Igor | rideAbovePx | 40 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; wiki Stages 10-12: Igor 'nhảy khỏi vai Monster' — ngồi ở vai (Monster cao 48) |
| Enemy.Igor | startDelayTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: đánh vài đòn 'trước khi Igor bắt đầu di chuyển' — 1.6 s sau khi Monster thức |
| Enemy.Igor | edgeMinScreenX | 24 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; wiki: 'nhảy hết sang một phía phòng rồi nhảy ngược lại' — đích mép trái (trên bục góc) |
| Enemy.Igor | edgeMaxScreenX | 232 |  |  | px màn |  | suy luận: ước lượng (không video) — routine 19; đích mép phải (trên bục góc) |
| Enemy.Igor | edgeReachPx | 24 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; tới đích ±24 px ⇒ đổi phía (thử 12: Igor đáp trên bục góc cách đích 20 px rồi nhảy cao thêm 1 lần ra giữa sàn — ảnh smoke CVGP 19) |
| Enemy.Igor | cornerZonePx | 48 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; wiki: 'nhảy CAO khi tới các bục ở góc' — trong 48 px cuối |
| Enemy.Igor | shortVx | 1.25 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'nhảy tiến ở giữa màn' — như Fleaman (17) hơi xa hơn |
| Enemy.Igor | shortVy0 | -3 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; đỉnh 18 px (g 0.25) |
| Enemy.Igor | longVx | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; cú nhảy cao vượt mép bục 32 px khi còn cách 40–48 px (lên 44 px lúc chạm mép) |
| Enemy.Igor | longVy0 | -5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; đỉnh 50 px (g 0.25) — cao hơn bục góc 32 px |
| Enemy.Igor | gravity | 0.25 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 19; như Fleaman (17) |
| Enemy.Igor | pauseMinTicks | 8 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'không hoàn toàn theo quy luật' — nghỉ ngẫu nhiên như Fleaman |
| Enemy.Igor | pauseSpanTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; ngẫu nhiên đều [8, 24] |
| Enemy.Igor | fireEveryHops | 2 |  |  | cú |  | suy luận: ước lượng (không video) — routine 19; wiki: 'ném cầu lửa' (tần suất không nêu) — ở đỉnh mỗi cú nhảy thứ 2 |
| Enemy.Igor | fireballSpeed | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 19; như cầu lửa Bone Dragon / Phantom Bat thường (18) |
| Enemy.Igor | aimAbovePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 19; nhắm tâm thân Simon (như 18) |
| Enemy.Igor | stunTicks | 120 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; wiki: 'quất Igor 1 lần sẽ đóng băng nó' — đủ dồn vài đòn vào Monster (2 s) |
| Enemy.Igor | hitInvincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 19; như boss (14) |
| Enemy.Snake | gravity | 0.25 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 19; rơi như item (ConfigRule.ItemGravity 0.25 — đo 11) |
| SubWeapon.Dagger | spawnHeightAboveFeet | 23 |  |  | px | pivot (tâm sprite) dao; x sinh = pivot Simon | đo:routine 09 proj_fit.py video:Map_1 f2797 (góc trên sprite dao = Y−31 khi Simon Y 112) và f3214 trên thang (Y 216 ⇒ Y−31) ⇒ pivot tâm sprite 16×16 = Y−23; Tools/cvgp/attack_compare.py dy 0 |
| SubWeapon.Axe | apexPx | 50 |  |  | px | thông tin | đo:routine 09 (= vy0²/2g với vy0 −5.0 g 0.25; video:Map_1 đỉnh tâm rìu Y−80.5 ≈ 28 + 50 + 2) |
| SubWeapon.Axe | apexTicks | 20 |  |  | tick | thông tin | đo:routine 09 (= \|vy0\|/g) |
| SubWeapon.Axe | spawnHeightAboveFeet | 26 |  |  | px | pivot (tâm sprite) rìu | đo:routine 09 khớp parabol 2 quỹ đạo (g cố định 0.25): tâm bbox rìu lúc sinh Y−27.5 / Y−27.8 ⇒ pivot tâm sprite = Y−26 (bbox axe_0 lệch −1.5 so với pivot) |
| Item.MagicCrystal | spawnDelayTicks | 152 | 150 | 154 | tick | từ tick boss hết máu | đo:routine 14: +3000 → quả cầu hiện: Map_1 f6305→f6380 (150), boss f534→f611 (154) |
| Item.MagicCrystal | spawnScreenX | 128 |  |  | px màn |  | đo:routine 14 boss_fit orb: pivot (đáy-giữa) x màn 128 cả 2 video |
| Item.MagicCrystal | spawnY | 104 |  |  | px section | pivot item = đáy ô 16 | đo:routine 14: hàng đáy sprite y màn 136 (pivot khớp 137 cả 2 video) ⇒ section 104 |
| Item.MagicCrystal | gravity | 0.25 | 0.21 | 0.25 | px/tick² | ItemPickup dùng ConfigRule.ItemGravity (cùng giá trị) | đo:routine 14: rơi Map_1 f6380–6390 +44 px trong ~20 tick khớp y = 0.25·t(t−1)/2 (±1 tick lúc hiện) |
| SubWeapon.HolyWater | spawnHeightAboveFeet | 23 |  |  | px | tâm lọ | suy luận: = dao (video boss Simon đang nháy bất tử — không thấy điểm sinh) |
| SubWeapon.HolyWater | fireTicks | 80 | 80 | 94 | tick | sau đó lửa biến mất; anim holywater_fire lặp | đo:routine 09 proj_fit.py video:Boss: lửa vỡ ~f348 tắt sau f389; lọ cuối chạm sàn ~f570 lửa tắt sau f609 (≈ 40 frame video) |
| SubWeapon.HolyWater | fireRehitTicks | 8 |  |  | tick | Hitbox.Activate lại ⇒ mục tiêu trúng lần nữa nếu hết bất tử | quy ước routine 09 (lửa đánh lại mỗi 8 tick); routine 14 không đo được nhịp thật — thanh ENEMY hiện chạy dần 8 tick/vạch che nhịp; tổng thời gian hạ 130 tick khớp bất tử boss 16 tick |
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
| Enemy.SpearKnight | turnMinTicks | 64 |  |  | tick | đoạn = Roll(min, min + span) tick | suy luận: ước lượng (không video) — routine 17; wiki "có thể quay lại bất cứ lúc nào" ⇒ đổi hướng ngẫu nhiên sau mỗi đoạn tuần tra |
| Enemy.SpearKnight | turnSpanTicks | 128 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do (64–192 tick ≈ 1–3 s, 32–96 px ở 0.5 px/tick) |
| Enemy.SpearKnight | edgeProbePx | 8 |  |  | px | đụng tường / hết sàn ⇒ quay đầu | suy luận: ước lượng (không video) — routine 17; wiki tuần tra trên bục (Stage 4: đứng dưới hố, chắn cửa) ⇒ không tự đi rơi khỏi mép: dò sàn cách pivot 8 px theo hướng đi |
| Enemy.SpearKnight | fallGravity | 0.125 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 17; rơi khi sinh lơ lửng — như Zombie (đo 13) |
| Enemy.SpearKnight | hitInvincibleTicks | 16 |  |  | tick | 1 cú roi chỉ trúng 1 lần dù hộp roi duỗi 9 tick | suy luận: ước lượng (không video) — routine 17; enemy nhiều đòn: bất tử sau mỗi đòn như boss (routine 14 suy luận 16) |
| Enemy.MedusaHead | waveAmplitudePx | 24 |  |  | px | y = y0 − A·sin(2πt/P): sinh ở tâm rồi đi LÊN | suy luận: ước lượng (không video) — routine 17; wiki "bay theo sóng; nhảy qua hoặc đi/ngồi dưới" ⇒ biên độ đủ để đỉnh sóng qua đầu Simon (cao 32) và đáy chạm chân |
| Enemy.MedusaHead | wavePeriodTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; ≈ 2.7 sóng trên 256 px ở 1.0 px/tick |
| Enemy.MedusaHead | spawnInsetPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; như dơi/Zombie (đo 13): hiện nguyên hình sát mép |
| Enemy.MedusaHead | spawnAboveFeetPx | 24 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; tâm sóng = chân Simon − 24 ⇒ đỉnh −48 (qua đầu), đáy 0 (chạm chân) |
| Enemy.MedusaHead | edgeScreenMarginPx | 64 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; wiki "từ hướng Simon đang nhìn" — luật mép như dơi (13) |
| Enemy.MedusaHead | maxAlive | 2 |  |  | con |  | suy luận: ước lượng (không video) — routine 17; Stage 5 nhiều Medusa Head liên tiếp — chọn 2 cùng lúc |
| Enemy.MedusaHead | groupSize | 1 |  |  | con |  | suy luận: ước lượng (không video) — routine 17; 1 con/lần |
| Enemy.MedusaHead | waveDelayMinTicks | 64 |  |  | tick | ngẫu nhiên đều [64, 128] | suy luận: ước lượng (không video) — routine 17; nhịp sinh liên tục ~1–2 s |
| Enemy.MedusaHead | waveDelaySpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.MedusaHead | firstDelayMinTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; wiki Stage 5 "xuất hiện ngay từ đầu" ⇒ trễ ngắn |
| Enemy.MedusaHead | firstDelaySpanTicks | 0 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.BoneTower | firstIdleTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; lượt bắn đầu sau khi hiện |
| Enemy.BoneTower | idleMinTicks | 96 |  |  | tick | ngẫu nhiên đều [96, 160] | suy luận: ước lượng (không video) — routine 17; wiki "theo chu kỳ" — nghỉ giữa 2 lượt |
| Enemy.BoneTower | idleSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.BoneTower | warnTicks | 32 |  |  | tick | đổi bảng tím ↔ đỏ (2 bộ màu có trên sheet) | suy luận: ước lượng (không video) — routine 17; wiki "nhấp nháy ngay trước khi bắn" |
| Enemy.BoneTower | warnBlinkTicks | 4 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; nhịp nháy |
| Enemy.BoneTower | fireGapTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; wiki "2 cầu lửa mỗi lượt" — đầu trên rồi đầu dưới cách 16 tick |
| Enemy.BoneTower | fireballSpeed | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; cầu lửa bay ngang về phía Simon (nhanh hơn Simon đi 1.0 để không né bằng cách đi) |
| Enemy.BoneTower | fireballFrontPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; miệng đầu lâu ở mép trước sprite 16 px |
| Enemy.BoneTower | fireballTopAbovePx | 16 |  |  | px | lần thử đầu 20 nằm trên dải roi 1 hàng — roi đứng không phá được | suy luận: ước lượng (không video) — routine 17; quả cầu (ô 8×16 pivot tâm; pixel đục hàng −7..−2) ngang miệng đầu lâu trên ⇒ hàng y−23..y−18 = trong dải roi đứng (ConfigWhip hitY: chân −22..−17) ⇒ quất vỡ được khi đứng (wiki) |
| Enemy.BoneTower | fireballBottomAbovePx | 0 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; cùng cách tính cho miệng đầu lâu dưới ⇒ quả cầu hàng y−7..y−2 (ngang ống chân — nhảy qua hoặc ngồi) |
| Enemy.BoneTower | hitInvincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; như SpearKnight |
| Enemy.Ghost | chaseAccel | 0.015625 |  |  | px/tick² | 1/64 px/tick² | suy luận: ước lượng (không video) — routine 17; wiki "chậm nhưng bám theo vị trí Simon, đến từ góc lạ" ⇒ gia tốc nhỏ từng trục ⇒ vượt quá rồi vòng lại |
| Enemy.Ghost | maxVy | 0.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; trần tốc độ dọc (ngang dùng ConfigEnemy.moveSpeed 0.625) |
| Enemy.Ghost | targetAboveFeetPx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; đuổi theo tâm thân Simon |
| Enemy.Ghost | hitInvincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; như SpearKnight |
| Enemy.Raven | triggerDistancePx | 96 |  |  | px | \|Simon.X − X\| < | suy luận: ước lượng (không video) — routine 17; wiki "cất cánh khi Simon lại gần" — xa hơn báo 70 (đo 13) để kịp lượn |
| Enemy.Raven | riseVy | -1.0 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 17; cất cánh thẳng lên |
| Enemy.Raven | riseTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; lên 16 px |
| Enemy.Raven | firstHoverTicks | 24 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; wiki "treo giây lát rồi bổ nhào" |
| Enemy.Raven | hoverTicks | 12 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; wiki "trượt thì quay đầu rất nhanh" ⇒ khựng ngắn hơn lần đầu |
| Enemy.Raven | diveOvershootPx | 32 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; lao quá điểm nhắm rồi mới khựng quay lại |
| Enemy.Raven | maxDiveTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; chặn cú lao dài |
| Enemy.Raven | targetAboveFeetPx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; nhắm tâm thân Simon lúc bắt đầu lao |
| Enemy.Raven | diveFloorAboveFeetPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; cú lao xuống dừng khi tâm Raven (sprite 16) chạm chân Simon − 8 ⇒ đáy sprite ngang mặt sàn, không lao xuyên sàn (ảnh Arena routine 17 raven_dive r160) |
| Enemy.Skeleton | keepMinPx | 48 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; wiki Stages 13-15 "chọn khoảng cách thuận lợi để tấn công", "đẩy chúng đi" ⇒ lùi khi Simon gần hơn |
| Enemy.Skeleton | keepMaxPx | 96 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; tiến khi Simon xa hơn |
| Enemy.Skeleton | decideTicks | 24 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; đổi ý mỗi 24 tick (trong dải: ngẫu nhiên tới/lui) |
| Enemy.Skeleton | jumpChancePct | 20 |  |  | % | mỗi lần đổi ý; đụng tường ⇒ nhảy luôn | suy luận: ước lượng (không video) — routine 17; wiki "nhảy nhiều" |
| Enemy.Skeleton | jumpVy0 | -3.25 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 17; đỉnh 42 px ⇒ lên được bục 32 px |
| Enemy.Skeleton | jumpVx | 1.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; như tốc độ đi |
| Enemy.Skeleton | gravity | 0.125 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 17; như enemy Block 1 (đo 13) |
| Enemy.Skeleton | firstThrowTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; ném sớm sau khi hiện |
| Enemy.Skeleton | throwMinTicks | 80 |  |  | tick | ngẫu nhiên đều [80, 144] | suy luận: ước lượng (không video) — routine 17; nghỉ giữa 2 loạt |
| Enemy.Skeleton | throwSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.Skeleton | burstMin | 2 |  |  | xương | 2 + Roll(0..burstSpan) | suy luận: ước lượng (không video) — routine 17; wiki "hay 2–3 xương cùng lúc" |
| Enemy.Skeleton | burstSpan | 1 |  |  | xương |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.Skeleton | burstGapTicks | 10 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; xương trong loạt cách nhau |
| Enemy.Skeleton | boneVx | 1.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; wiki "vòng cung theo các đường khác nhau" ⇒ xương k: vx = boneVx + k·boneVxStep |
| Enemy.Skeleton | boneVxStep | 0.375 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.Skeleton | boneVy0 | -3.5 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 17; xương k: vy0 = boneVy0 − k·boneVyStep |
| Enemy.Skeleton | boneVyStep | 0.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.Skeleton | boneGravity | 0.125 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 17; như enemy Block 1 |
| Enemy.Skeleton | boneHandAbovePx | 24 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; tay giơ cao trên sprite 32 px |
| Enemy.Fleaman | triggerDistancePx | 112 |  |  | px | \|Simon.X − X\| < | suy luận: ước lượng (không video) — routine 17; wiki Stage 13 "chờ sẵn" ⇒ bắt đầu nhảy khi Simon tới gần |
| Enemy.Fleaman | shortVx | 1.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; wiki "2 bước nhảy ngắn rồi 1 cú nhảy vọt" |
| Enemy.Fleaman | shortVy0 | -2.0 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 17; nhảy ngắn: đỉnh 8 px, 16 tick |
| Enemy.Fleaman | longVx | 2.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 17; nhảy vọt qua đầu Simon (wiki "chạy chui dưới cú nhảy cao") |
| Enemy.Fleaman | longVy0 | -4.5 |  |  | px/tick | âm = lên | suy luận: ước lượng (không video) — routine 17; nhảy vọt: đỉnh 40 px, 36 tick |
| Enemy.Fleaman | gravity | 0.25 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 17; nhảy nhanh, dứt khoát (bằng g rìu/item đo 09/11) |
| Enemy.Fleaman | longEvery | 3 |  |  | lần |  | suy luận: ước lượng (không video) — routine 17; cú thứ 3 của mỗi chu kỳ là nhảy vọt |
| Enemy.Fleaman | pauseMinTicks | 4 |  |  | tick | ngẫu nhiên đều [4, 16] | suy luận: ước lượng (không video) — routine 17; wiki "nhìn như ngẫu nhiên" ⇒ nghỉ ngẫu nhiên giữa 2 cú |
| Enemy.Fleaman | pauseSpanTicks | 12 |  |  | tick |  | suy luận: ước lượng (không video) — routine 17; cùng lý do |
| Enemy.Bat | swoopVy | 1.5 |  |  | px/tick | dương = xuống | suy luận: ước lượng (không video) — routine 17; Black Bat (wiki Enemies#Bats "treo trần rồi lao xuống khi Simon lại gần"): sà xuống chéo tới tầm Simon rồi bay sóng như dơi đỏ |
| Enemy.Bat | swoopAboveFeetPx | 14 |  |  | px |  | suy luận: ước lượng (không video) — routine 17; sà tới tâm = chân Simon − 14 (= mốt lúc dơi đỏ sinh, đo 13) |
| Enemy.Eagle | carryBelowPx | 28 |  |  | px | pivot Fleaman = tâm chim + 28 | suy luận: ước lượng (không video) — routine 18; chân Fleaman (pivot đáy 16×16) dưới tâm chim 28 ⇒ đầu Fleaman chạm móng (eagle_0 đáy ô 32 ở tâm + 16, chồng 4 px) |
| Enemy.Eagle | dropDistancePx | 28 |  |  | px | chỉ thả khi đang bay TỚI Simon | suy luận: ước lượng (không video) — routine 18; wiki Stage 11 "Fleaman hầu như luôn rơi đúng tầm roi" ⇒ thả khi \|dx\| ≤ 28 (roi da trúng 12–36 px trước pivot — ConfigWhip đo 09) |
| Enemy.Eagle | spawnAboveFeetPx | 96 |  |  | px | tâm = chân Simon − 96 | suy luận: ước lượng (không video) — routine 18; bay cao trên đầu Simon (wiki "bầu trời đầy Eagle"); Fleaman rơi 68 px ≈ 23 tick (g 0.25 routine 17) |
| Enemy.Eagle | spawnInsetPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; như dơi/Zombie (đo 13): hiện sát mép màn |
| Enemy.Eagle | edgeScreenMarginPx | 64 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; EdgeSpawner FrontOfPlayer như dơi 13 / Medusa Head 17 (wiki không nói phía) |
| Enemy.Eagle | maxAlive | 2 |  |  | con |  | suy luận: ước lượng (không video) — routine 18; wiki Stage 11 "con này nối con kia" — tối đa 2 chim trên màn |
| Enemy.Eagle | groupSize | 1 |  |  | con |  | suy luận: ước lượng (không video) — routine 18; 1 chim/lần |
| Enemy.Eagle | waveDelayMinTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; wiki "không ngừng xuất hiện tới cuối stage" — ~1.5–2.5 s giữa 2 chim |
| Enemy.Eagle | waveDelaySpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; cùng lý do |
| Enemy.Eagle | firstDelayMinTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; wiki Stage 11 "lúc đầu tưởng không có kẻ địch rồi Eagle xuất hiện" — chim đầu sau ~0.5 s khi vào vùng |
| Enemy.Eagle | firstDelaySpanTicks | 0 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; cùng lý do |
| Enemy.BoneDragon | segments | 6 |  |  | đốt | ≤ CvgpEnemyBuilder.DRAGON_NECK | suy luận: ước lượng (không video) — routine 18; wiki "cổ dài" — sheet chỉ có 1 đốt 8×16; 6 đốt ⇒ cổ 48 px + đầu (≈ 1/5 màn) |
| Enemy.BoneDragon | segmentSpacingPx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; đốt skeledragon_bone rộng 8 — xếp sát nhau |
| Enemy.BoneDragon | headGapPx | 10 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; tâm đầu 16×16 cách tâm đốt cuối 8 + 2 (đầu chồng nhẹ lên đốt) |
| Enemy.BoneDragon | headAmplitudePx | 24 |  |  | px | đốt k lệch A·k/(N+1) | suy luận: ước lượng (không video) — routine 18; wiki "đầu ngọ nguậy lên xuống trên cổ dài; chờ đầu hạ thấp rồi quất" ⇒ ±24: điểm thấp nhất vào dải roi đứng (chân −25..−17) khi chân cổ ngang đầu Simon |
| Enemy.BoneDragon | wavePeriodTicks | 128 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; ngọ nguậy chậm ~2 s/chu kỳ (đủ thời gian "chờ đầu hạ" rồi quất) |
| Enemy.BoneDragon | waveLagTicks | 6 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; sóng lan từ tường ra đầu — trễ 6 tick/đốt ⇒ cổ uốn lượn ("thân ngọ nguậy thất thường") |
| Enemy.BoneDragon | firstIdleTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; nghỉ trước lượt phun đầu (Bone Tower 48 — routine 17; wiki: shot multiplier có thể giết trước khi kịp phun) |
| Enemy.BoneDragon | idleMinTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; như Bone Tower (routine 17) |
| Enemy.BoneDragon | idleSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; như Bone Tower (routine 17) |
| Enemy.BoneDragon | warnTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; há miệng (head_1) trước khi phun — báo hiệu như Bone Tower nháy (wiki không nói) |
| Enemy.BoneDragon | burstCount | 2 |  |  | quả |  | suy luận: ước lượng (không video) — routine 18; wiki "phun các cầu lửa theo các góc" (số nhiều) — 2 quả/lượt như Bone Tower |
| Enemy.BoneDragon | burstGapTicks | 20 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; 2 quả cách nhau, mỗi quả nhắm lại ⇒ "các góc khác nhau" khi Simon di chuyển |
| Enemy.BoneDragon | fireballSpeed | 1.5 |  |  | px/tick | độ lớn vận tốc (vx, vy nguyên) | suy luận: ước lượng (không video) — routine 18; như cầu lửa Bone Tower (routine 17) |
| Enemy.BoneDragon | aimAbovePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; nhắm tâm thân Simon (chân − 16) |
| Enemy.BoneDragon | mouthFrontPx | 6 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; miệng ở mép trước đầu 16×16 |
| Enemy.BoneDragon | dropMinCount | 5 |  |  | vật | số vật = 5 + [0, dropSpanCount] | wiki:Enemies#Bone Dragons "thường để lại 5–8 túi tiền hoặc tim nhỏ" |
| Enemy.BoneDragon | dropSpanCount | 3 |  |  | vật |  | wiki:Enemies#Bone Dragons (5–8) |
| Enemy.BoneDragon | dropMoneyPct | 50 |  |  | % |  | suy luận: ước lượng (không video) — routine 18; wiki "túi tiền hoặc tim nhỏ" không nói tỉ lệ/màu túi — 50 % túi đỏ (100 đ, rẻ nhất) / 50 % tim nhỏ |
| Enemy.BoneDragon | hitInvincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; enemy nhiều đòn như Spear Knight/Bone Tower (routine 17) = bất tử boss 14 |
| Enemy.RedSkeleton | collapseTicks | 8 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; khung redskeleton_1 (đang đổ) 8 tick rồi đống xương _0 (nhịp khung mặc định 8) |
| Enemy.RedSkeleton | riseTicks | 8 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; ráp lại: khung _1 8 tick rồi đi |
| Enemy.RedSkeleton | turnDeadzonePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; "chậm rãi đuổi theo": quay lại khi Simon ở phía sau quá 16 px (tránh lắc qua lại khi trùng cột) |
| Enemy.RedSkeleton | fallGravity | 0.125 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 18; rơi khỏi gờ như Zombie (đo 13) |
| Enemy.AxeKnight | keepMinPx | 64 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; wiki "lùi khi Simon tiến, không bao giờ cố tới quá gần" ⇒ lùi khi gần hơn 64 (ngoài tầm roi xích 52 — đo 09) |
| Enemy.AxeKnight | keepMaxPx | 112 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; tiến khi xa hơn 112 (rìu bay ~85 px rồi quay về — trong dải 64–112 rìu tới Simon) |
| Enemy.AxeKnight | decideTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; chọn hướng mỗi ~0.5 s (Skeleton 24 — Axe Knight chậm hơn) |
| Enemy.AxeKnight | edgeProbePx | 8 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; không tự rơi khỏi bục (wiki Stage 14 Knight trên gờ cao đứng tại chỗ) — như Spear Knight 17 |
| Enemy.AxeKnight | fallGravity | 0.125 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 18; như Zombie (đo 13) khi sinh lơ lửng |
| Enemy.AxeKnight | firstThrowTicks | 48 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; như Bone Tower/Skeleton (routine 17) |
| Enemy.AxeKnight | throwMinTicks | 96 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; wiki "học nhịp ném" — chu kỳ đều ~1.6–2.7 s, chỉ ném khi rìu trước đã hết |
| Enemy.AxeKnight | throwSpanTicks | 64 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; cùng lý do |
| Enemy.AxeKnight | throwPoseTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; tư thế ném (khung axeman_1) — sheet không có khung ném riêng |
| Enemy.AxeKnight | axeSpeed | 2.0 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 18; rìu "y hệt Boomerang của Simon" — tốc ngang bằng rìu Simon vx 2 (đo 09) |
| Enemy.AxeKnight | axeTurnAccel | 0.0234375 |  |  | px/tick² |  | suy luận: ước lượng (không video) — routine 18; giảm 6/256 px/tick² ⇒ dừng sau 85 tick, xa ≈ 85 px rồi quay về (thập tự Simon quay ở 112 — routine 09; rìu Knight ngắn hơn) |
| Enemy.AxeKnight | axeHighAbovePx | 20 |  |  | px | tâm rìu | suy luận: ước lượng (không video) — routine 18; wiki "ném cao hoặc thấp — quất đúng độ cao": rìu cao ở dải roi ĐỨNG (chân −25..−17, ConfigWhip đo 09) |
| Enemy.AxeKnight | axeLowAbovePx | 10 |  |  | px | tâm rìu | suy luận: ước lượng (không video) — routine 18; rìu thấp ở dải roi NGỒI (khung ngồi thấp 8 — đo 08 ⇒ chân −17..−9); nhảy qua được |
| Enemy.AxeKnight | axeFrontPx | 12 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; rìu sinh ở mép trước thân (axeman rộng 24, pivot giữa) |
| Enemy.AxeKnight | hitInvincibleTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; enemy nhiều đòn như Spear Knight/Bone Tower (routine 17) |
| Enemy.PhantomBatMinor | wakeDistancePx | 96 |  |  | px | \|Simon.X − X\| < | suy luận: ước lượng (không video) — routine 18; đặt sẵn theo vị trí đánh số (wiki Stage 16) — thức khi Simon tới gần 96 (như Raven 17) |
| Enemy.PhantomBatMinor | wakeHoverTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; boss vỗ tại chỗ 33 (đo 14) — bản thường lao sớm hơn (wiki "nhắm độ cao cú nhảy") |
| Enemy.PhantomBatMinor | sleepDrawUpPx | 8 |  |  | px |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.sleepDrawUpPx |
| Enemy.PhantomBatMinor | swoopVy0 | 1.46 |  |  | px/tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.swoopVy0 |
| Enemy.PhantomBatMinor | swoopDepthPx | 41 |  |  | px |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.swoopDepthPx |
| Enemy.PhantomBatMinor | swoopVxMin | 1.22 |  |  | px/tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.swoopVxMin |
| Enemy.PhantomBatMinor | swoopVxMax | 2.1 |  |  | px/tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.swoopVxMax |
| Enemy.PhantomBatMinor | sweepTicks | 12 |  |  | tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.sweepTicks |
| Enemy.PhantomBatMinor | riseAccel | 0.039 |  |  | px/tick² |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.riseAccel |
| Enemy.PhantomBatMinor | screenMinX | 18 |  |  | px màn |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.screenMinX |
| Enemy.PhantomBatMinor | screenMaxX | 238 |  |  | px màn |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.screenMaxX |
| Enemy.PhantomBatMinor | glideSpeed | 0.75 |  |  | px/tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.glideSpeed |
| Enemy.PhantomBatMinor | glideMinScreenX | 24 |  |  | px màn |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.glideMinScreenX |
| Enemy.PhantomBatMinor | glideMaxScreenX | 232 |  |  | px màn |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.glideMaxScreenX |
| Enemy.PhantomBatMinor | glideMinSimonDx | 64 |  |  | px |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.glideMinSimonDx |
| Enemy.PhantomBatMinor | hoverMinTicks | 32 |  |  | tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.hoverMinTicks |
| Enemy.PhantomBatMinor | hoverMaxTicks | 124 |  |  | tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.hoverMaxTicks |
| Enemy.PhantomBatMinor | diveChancePct | 100 |  |  | % |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.diveChancePct |
| Enemy.PhantomBatMinor | invincibleTicks | 16 |  |  | tick |  | suy luận: bằng boss Phantom Bat (đo routine 14) — routine 18 dùng lại chuyển động; Boss.PhantomBat.invincibleTicks |
| Enemy.PhantomBatMinor | hitStunTicks | 32 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; wiki Stage 16 "quất 1 phát làm chậm vừa đủ để cuộn chúng ra khỏi màn" ⇒ khựng dài hơn boss 10 (đo 14) |
| Enemy.PhantomBatMinor | swoopMaxAboveFeetPx | 24 |  |  | px | đáy = chân Simon (lúc thức) − 24 | suy luận: ước lượng (không video) — routine 18; wiki "nhắm vào độ cao cú nhảy" ⇒ đáy bổ nhào chạm thân Simon (boss: đáy cố định y 156 phòng boss) |
| Enemy.PhantomBatMinor | riseTopAboveFeetPx | 104 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; boss vọt tới y 88 = sàn 208 − 120 (đo 14) — cầu Stage 16 thấp hơn, chọn chân − 104 |
| Enemy.PhantomBatMinor | glideLowAboveFeetPx | 96 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; boss lượn thấp y 92 = sàn − 116 (đo 14) — tỉ lệ theo riseTop |
| Enemy.PhantomBatMinor | glideHighAboveFeetPx | 72 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; boss lượn cao y 123 = sàn − 85 (đo 14) — tỉ lệ theo riseTop |
| Enemy.PhantomBatMinor | fireDelayTicks | 16 |  |  | tick |  | suy luận: ước lượng (không video) — routine 18; wiki "phun được cầu lửa" — 1 quả mỗi lần lơ lửng, sau 16 tick (lơ lửng boss ≥ 32) |
| Enemy.PhantomBatMinor | fireballSpeed | 1.5 |  |  | px/tick |  | suy luận: ước lượng (không video) — routine 18; như cầu lửa Bone Tower/Bone Dragon |
| Enemy.PhantomBatMinor | aimAbovePx | 16 |  |  | px |  | suy luận: ước lượng (không video) — routine 18; nhắm tâm thân Simon |
| Object.MovingPlatform | centerXTEB | 64 |  |  | px section | TowerEntranceB marker platform_lane_TEB; x = tâm + biên·cos(2πt/chu kỳ), lúc sinh ở mép phải | đo:cvm2-06 video:Map_2 0:45.0–0:51.7 (f1351–1552, 143 frame sạch, mech6_platform.py ghép nền + sprite): mép trái dao động sin tâm 47.99 biên 15.51 ⇒ pivot (giữa bệ) tâm 48+16; bệ chạm 2 bờ khe x 32–96 ở 2 đầu |
| Object.MovingPlatform | amplitudeTEB | 16 | 15.5 | 16 | px |  | đo:cvm2-06 video:Map_2 f1351–1552: biên khớp sin 15.51 (rms 0.41 px; mép trái đo 32–64) |
| Object.MovingPlatform | periodTicksTEB | 201.4 | 201.0 | 201.75 | tick | ≈ 2π·32 | đo:cvm2-06 video:Map_2 f1351–1552: chu kỳ khớp sin 201.40 tick (1 frame = 2.0033 tick); gia tốc hằng rms 0.59, tốc độ đều 1.34 ⇒ loại |
| Object.MovingPlatform | centerXHallB1 | 304 |  |  | px section | TowerHallB marker platform_lane_HallB_1; bệ chạy liên tục cả khi ngoài khung (pha 2 lượt nối liền) | đo:cvm2-06 video:Map_2 1:41.0–1:48.2 (f3030–3246) + 2:11.5–2:18.5 (f3945–4153) ghép 1 hàm pha liên tục: mép trái tâm 287.94 biên 31.48 ⇒ pivot tâm 288+16; chạm 2 bờ khe x 256–352 |
| Object.MovingPlatform | amplitudeHallB1 | 32 | 31.4 | 32 | px |  | đo:cvm2-06 video:Map_2 f3030–3246 + f3945–4153: biên 31.48 (rms 0.49 px; mép trái đo 256–320) |
| Object.MovingPlatform | periodTicksHallB1 | 402.4 | 401.0 | 402.5 | tick | ≈ 2π·64 | đo:cvm2-06 video:Map_2 f3030–4153 (2 lượt, ~4.6 chu kỳ): chu kỳ khớp sin 402.37 tick |
| Object.MovingPlatform | centerXHallB2 | 432 |  |  | px section | TowerHallB marker platform_lane_HallB_2 | đo:cvm2-06 video:Map_2 1:43.9–1:51.3 (f3117–3339) + 2:09.2–2:14.6 (f3876–4037) ghép: mép trái tâm 415.93 biên 23.45 ⇒ pivot tâm 416+16; KHÔNG chạm bờ (khe x 384–480, bệ 392–472) |
| Object.MovingPlatform | amplitudeHallB2 | 24 | 23.4 | 24 | px |  | đo:cvm2-06 video:Map_2 f3117–4037: biên 23.45 (rms 0.53 px; mép trái đo 392–440); chọn 24 = cùng kiểu số nguyên với 16/32 (sai ≤ 0.6 px) |
| Object.MovingPlatform | periodTicksHallB2 | 201.8 | 201.0 | 201.82 | tick |  | đo:cvm2-06 video:Map_2 f3117–4037 (2 lượt, ~8 chu kỳ): chu kỳ khớp sin 201.82 tick |
| Object.Crusher | downStepTicks | 4 | 4 | 4 | tick |  | đo:cvm2-06 video:Map_2 2:35.6–3:15.7 (f4668–5872, 3 bẫy, mech6_runs.py): mỗi nấc hạ 8 px kéo 2 frame video = 4.0 tick (±2 rung nhịp quay); nấc ngay sau khung tối đa = đổi chiều lên 1 nấc |
| Object.Crusher | gridTicks | 16 | 16 | 16 | tick | lưới chung 3 bẫy (cùng tick sinh) | đo:cvm2-06 video:Map_2 f4626–5872: mỗi nấc lên 8 frame = 16 tick; khung 0 giữ 16 tick; 3 bẫy đổi nấc CÙNG frame (f4827, f4923) trùng nhịp giảm TIME 64 tick (= 4 ô) |
| Object.Crusher | maxFrame1 | 6 | 6 | 6 | khung | TowerHallC marker crusher_1 | đo:cvm2-06 video:Map_2 6 chu kỳ bẫy 1 (wx 368) đều dừng ở khung 6 (đáy khối y 136), chu kỳ 112 tick (f5612→5667→5723) |
| Object.Crusher | maxFrame2 | 4 | 4 | 4 | khung | TowerHallC marker crusher_2 | đo:cvm2-06 video:Map_2 8 chu kỳ bẫy 2 (wx 432) dừng ở khung 4 (đáy khối y 120), chu kỳ 80 tick (f4747→4787, f5707→5746→5786); wiki:Stages 4-6 'drops a much shorter distance' ✓ |
| Object.Crusher | maxFrame3 | 7 | 7 | 7 | khung | TowerHallC marker crusher_3 | đo:cvm2-06 video:Map_2 7 chu kỳ bẫy 3 (wx 496) hạ hết khung 7 (gai chạm sàn y 144), chu kỳ 128 tick (f4660→4725→4788) |
| Object.Crusher | startTick1 | 55 | 53 | 57 | tick | 0 = bắt đầu hạ; bẫy 1 đi trước bẫy 2/3 đúng 3 ô lưới | đo:cvm2-06 video:Map_2 lượt dưới f4600–4828 khớp mô hình (88 % frame đúng khung; lệch còn lại ±1 nấc = rung nhịp quay): tick trong chu kỳ lúc Hall C hiện (f4479 cx 512, cuối cửa trái #2) = 55.3 |
| Object.Crusher | startTick2 | 7 | 5 | 9 | tick |  | đo:cvm2-06 video:Map_2 lượt dưới f4600–4828: tick trong chu kỳ lúc Hall C hiện (f4479) = 7.3 (bắt đầu hạ 7 tick trước, CÙNG lúc bẫy 3) |
| Object.Crusher | startTick3 | 7 | 5 | 9 | tick |  | đo:cvm2-06 video:Map_2 lượt dưới f4600–4828: tick trong chu kỳ lúc Hall C hiện (f4479) = 7.3 |
| Object.MovingPlatform | startTickTEB | 4 | 3 | 4 | tick | bắt đầu trong màn đen chuyển tầng 22 tick | đo:cvm2-06 video:Map_2 f1351–1552 so mô hình (mech6_compare.py): bệ ĐÃ chạy τ tick từ mép phải khi TEB hiện (f866) — τ 0 rms 1.46 px, τ 4 rms 0.75 px (tốt nhất 3.5) |
| Object.MovingPlatform | startTickHallB1 | 6 | 5 | 6 | tick |  | đo:cvm2-06 video:Map_2 f3030–4153: τ tick lúc Hall B hiện (f2801) — τ 0 rms 2.11 px, τ 6 rms 0.79 px (tốt nhất 5.75) |
| Object.MovingPlatform | startTickHallB2 | 6 | 6 | 6 | tick |  | đo:cvm2-06 video:Map_2 f3117–4037: τ lúc Hall B hiện (f2801) — τ 0 rms 3.41 px, τ 6 rms 0.79 px (tốt nhất 6.0; cùng τ bệ 1 ⇒ 2 bệ sinh cùng lúc nạp section) |

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
| HolyWater | 1 | Weak | 1 | không | — | 1.5 | -1.3 | 0.125 | Lọ vỡ khi chạm sàn thành ngọn lửa nhỏ giữ chân/đốt enemy | wiki:Items#Holy Water; wiki:Enemies (nhóm yếu) · *pierces:* video:Boss_Map_1 15.57–16.10 s (f467–483: lọ vào thân Phantom Bat; bị thân che; ló ra dưới-trái vẫn bay rồi vỡ ở sàn; ENEMY 16→15 f471 đúng lúc lọ chạm) + video:Boss_Map_2 4.5–5.0 s và 12.8–13.4 s (f134–149; f383–402: lọ xuyên Medusa vỡ ở sàn phía sau; ENEMY 10→9 f393 lúc lọ trong mặt) — đo:cvm2-04 (lọ trúng 1 lần/mục tiêu rồi bay tiếp) · *motion:* đo:routine 09 proj_fit.py khớp 2 quỹ đạo lọ video:Boss 13.0–13.9 s (f390–403 / f404–418: vx 1.57 / 1.43; g 0.116 / 0.133; với g 0.125 vy0 −1.13 / −1.53) |
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
| 1 | 1, 2, 3 | PhantomBat | 300 | 2 | PoisonMind | Các stage luyện tập: sân và sảnh lâu đài; hầm có nước; hành lang tới phòng boss | wiki:Walkthrough; wiki:Stages 1-3 · *timeLimit:* video:Map_1 0:02.2 TIME 0300 (= CVTI GameSession); đồng hồ KHÔNG đặt lại giữa Stage 1→2→3 (1:31.87 STAGE 02 TIME 0226; 2:50.87 STAGE 03 TIME 0166) · *damage:* wiki:Gameplay#Damage (Stage 1–6 = 2 vạch) · *music:* video:Map_1 3:18.80 Poison Mind thay Vampire Killer (xcorr NCC 0.96) + video:Boss_Map_1 2.81 s (NCC 0.93) — đo:cvm2-03 Tools/cvm2/music3.py |
| 2 | 4, 5, 6 | QueenMedusa | 400 | 2 | PoisonMind | Nhiều enemy và hố hơn; bẫy gai Stage 6 | wiki:Walkthrough; wiki:Stages 4-6 · *timeLimit:* video:Map_1 3:59.2 STAGE 04 TIME 0400 · *damage:* wiki:Gameplay#Damage (Stage 1–6 = 2 vạch) · *music:* video:Boss_Map_2 3.22 s Poison Mind thay Stalker ≈0.9 s sau khi Medusa thức (NCC 0.94) + video:Map_2 3:48.1–3:58.8 (NCC 0.96 ở 3:57.0) — đo:cvm2-03 Tools/cvm2/music3.py |
| 3 | 7, 8, 9 | MummyMan | *không rõ* | 3 | PoisonMind | Enemy chuyển động phức tạp hơn; cầu hỏng; phòng Mummy | wiki:Walkthrough; wiki:Stages 7-9 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 7–12 = 3 vạch); wiki:Stages 7-9 · *music:* suy luận: cùng bài boss Block 1–2 (đã xác nhận video) — Block 3 chưa có video |
| 4 | 10, 11, 12 | Frankenstein | *không rõ* | 3 | PoisonMind | Hầm nước có bệ nổi; mặt đất có Eagle; cống ngầm có Bone Dragon | wiki:Walkthrough; wiki:Stages 10-12 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 7–12 = 3 vạch) · *music:* suy luận: cùng bài boss Block 1–2 (đã xác nhận video) — Block 4 chưa có video |
| 5 | 13, 14, 15 | Death | *không rõ* | 4 | PoisonMind | Nguy hiểm nhất: Axe Knight; Red Skeleton; chỉ chịu 4 đòn | wiki:Walkthrough; wiki:Stages 13-15 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 13–18 = 4 vạch); wiki:Stages 13-15 · *music:* suy luận: cùng bài boss Block 1–2 (đã xác nhận video) — Block 5 chưa có video |
| 6 | 16, 17, 18 | Dracula | *không rõ* | 4 | BlackNight | Cầu dài; tháp đồng hồ; phòng Dracula | wiki:Walkthrough; wiki:Stages 16-18 · *timeLimit:* không rõ — không có video (wiki không nêu giờ) · *damage:* wiki:Gameplay#Damage (Stage 13–18 = 4 vạch) · *music:* suy luận: tên file Art_Source/music (Final Battle) — Block 6 chưa có video |

| Stage | Block | Khu vực | Enemy | Boss | Nhạc | Nguồn |
|---|---|---|---|---|---|---|
| 1 | 1 | Sân ngoài lâu đài (không enemy) + sảnh vào lâu đài | Ghoul, Panther | None | VampireKiller | wiki:Stages 1-3#Stage 1 · *music:* video:Map_1 0:02.22 Vampire Killer đầu bài (xcorr NCC 0.94) + video:Title_Intro 16.40 s (= L922 Stage 01) — đo:cvm2-03 Tools/cvm2/music3.py |
| 2 | 1 | Hầm ngầm có nước (rơi nước chết) | Fishman, Bat | None | VampireKiller | wiki:Stages 1-3#Stage 2 · *music:* video:Map_1 1:31–2:50 Vampire Killer chạy liền từ Stage 01 qua cửa (không phát lại; gốc bài không đổi ±0.03 s/vòng 29.82 s) — đo:cvm2-03 Tools/cvm2/music3.py |
| 3 | 1 | Hành lang cầu thang xuống phòng boss | Ghoul | PhantomBat | VampireKiller | wiki:Stages 1-3#Stage 3 · *music:* video:Map_1 2:51–3:18.7 Vampire Killer chạy liền tới khi Poison Mind (không phát lại) — đo:cvm2-03 Tools/cvm2/music3.py |
| 4 | 2 | Ngắn; Spear Knight canh cửa cuối | SpearKnight, Bat | None | Stalker | wiki:Stages 4-6#Stage 4; wiki:Enemies · *music:* video:Map_2 0:06.18 Stalker đầu bài (xcorr NCC 0.94; STAGE 04 hiện 0:06.23) — đo:cvm2-03 Tools/cvm2/music3.py |
| 5 | 2 | Nhiều hố; Medusa Head ngay từ đầu | MedusaHead, SpearKnight | None | Stalker | wiki:Stages 4-6#Stage 5; wiki:Enemies · *music:* video:Map_2 0:57.7–2:22 Stalker chạy liền qua cửa trái #1 (không phát lại) — đo:cvm2-03 Tools/cvm2/music3.py |
| 6 | 2 | Bẫy gai và sảnh vòm | BoneTower, Ghost, MedusaHead | QueenMedusa | Stalker | wiki:Stages 4-6#Stage 6; wiki:Enemies · *music:* video:Map_2 2:28–3:47.8 Stalker chạy liền qua cửa trái #2 + sảnh Medusa (dừng 2.40 s lúc dùng đồng hồ 2:40.97 rồi chạy tiếp) — đo:cvm2-03 Tools/cvm2/music3.py |
| 7 | 3 | Bục và khối nhô; sát thương 3 vạch | Ghost, Fleaman, Skeleton, Raven | None | WickedChild | wiki:Stages 7-9#Stage 7; wiki:Enemies · *music:* video:Map_2 4:20.15–4:20.40 (0.3 s đầu STAGE 07): Wicked Child songpos 0.35 NCC 0.48 (bài kế 0.14) — bằng chứng yếu; đo:cvm2-03 Tools/cvm2/music3.py |
| 8 | 3 | Tầng dưới + tầng trên | MedusaHead, Skeleton, BoneTower, Raven | None | WickedChild | wiki:Stages 7-9#Stage 8 · *music:* suy luận: cùng bài Stage 07 (Block 3 chạy liền như Block 1–2) — chưa có video |
| 9 | 3 | Cầu hỏng; phòng Mummy | Raven, Skeleton, BoneTower, MedusaHead | MummyMan | WickedChild | wiki:Stages 7-9#Stage 9; wiki:Enemies · *music:* suy luận: cùng bài Stage 07 (Block 3 chạy liền như Block 1–2) — chưa có video |
| 10 | 4 | Hầm nước có bệ nổi; trần thấp | Fishman, Bat | None | WalkingOnTheEdge | wiki:Stages 10-12#Stage 10 · *music:* suy luận: tên file Art_Source/music (Castle Swamp) — chưa có video |
| 11 | 4 | Trên mặt đất; Eagle liên tục; 1 Bone Dragon | Eagle, Fleaman, BoneDragon | None | WalkingOnTheEdge | wiki:Stages 10-12#Stage 11; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — chưa có video |
| 12 | 4 | Cống ngầm; 2 Bone Dragon | BoneDragon | Frankenstein | WalkingOnTheEdge | wiki:Stages 10-12#Stage 12 · *music:* suy luận: tên file Art_Source/music — chưa có video |
| 13 | 5 | Sảnh không có hố khó; sát thương 4 vạch | Fleaman, Skeleton, RedSkeleton | None | HeartOfFire | wiki:Stages 13-15#Stage 13; wiki:Enemies · *music:* suy luận: tên file Art_Source/music (Prisonhold) — chưa có video |
| 14 | 5 | Nhiều Axe Knight | AxeKnight, RedSkeleton, Fleaman | None | HeartOfFire | wiki:Stages 13-15#Stage 14; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — chưa có video |
| 15 | 5 | Bậc thang qua Bone Tower rồi hành lang cuối | BoneTower, AxeKnight, MedusaHead, Ghost, RedSkeleton | Death | HeartOfFire | wiki:Stages 13-15#Stage 15; wiki:Enemies · *music:* suy luận: tên file Art_Source/music — chưa có video |
| 16 | 6 | Cầu dài tới tháp chuông | PhantomBatMinor | None | OutOfTime | wiki:Stages 16-18#Stage 16 · *music:* suy luận: tên file Art_Source/music (Upper Terrace) — chưa có video |
| 17 | 6 | Tháp đồng hồ (bánh răng) | Skeleton, Eagle, Fleaman | None | OutOfTime | wiki:Stages 16-18#Stage 17 · *music:* suy luận: tên file Art_Source/music — chưa có video |
| 18 | 6 | Phòng Dracula (đỉnh lâu đài) |  | Dracula | NothingToLose | wiki:Stages 16-18#Stage 18 · *music:* suy luận: tên file Art_Source/music (Dracula's Quarters) — chưa có video |

## 9. Điểm, 1UP, tally

| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |
|---|---|---|---|---|---|
| OneUpFirst | 30000 |  | điểm | wiki:Gameplay#Simon Belmont |  |
| OneUpSecond | 80000 |  | điểm | wiki:Gameplay#Simon Belmont |  |
| OneUpEvery | 50000 |  | điểm | wiki:Gameplay#Simon Belmont | sau 80000: 130000; 180000 … (suy luận) |
| TallyPointsPerTimeUnit | 10 |  | điểm/đv TIME | wiki:Gameplay#Points; video:Map_1 3:37.4 (+10 mỗi đơn vị) |  |
| TallyPointsPerHeart | 100 |  | điểm/tim | wiki:Gameplay#Points; video:Map_1 3:40.5–3:49.9 (+100 mỗi tim) |  |
| TallyTicksPerTimeUnit | 1 |  | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1 3:37.4–3:39.5: TIME 0127→0000 trong 128 tick = 1.01 tick/đv; video boss 24.6 s cùng nhịp 2 đv/frame) |  |
| TallyTicksPerHeart | 8 |  | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1 3:40.4–3:49.9: tim 72→00 hồi quy 7.99 tick/tim, dư ≤ 2.5 tick) |  |
| TallyGapTicks | 54 | 52–56 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1: TIME = 0000 f6586 → tim giảm đầu f6613 = 27 frame) | từ tick TIME về 0 tới tick tim giảm đầu |
| MultiKill2 | 1000 |  | điểm | wiki:Gameplay#Points (Special — 1 lần ném vũ khí phụ hạ 2 enemy) | không tính roi |
| MultiKill3 | 2000 |  | điểm | wiki:Gameplay#Points |  |
| MultiKill4 | 4000 |  | điểm | wiki:Gameplay#Points |  |
| MultiplierPickupBonus | 700 |  | điểm | wiki:Items#Double Shot (khi đang cầm Stopwatch); wiki:Gameplay#Points (hoặc không có vũ khí phụ) | 2 trang nói khác điều kiện — CV_WIKI_DATA §7.1 #3 |
| RosaryKillPoints | 0 |  | điểm | wiki:Items#Rosary (enemy bị Rosary diệt không cho điểm) |  |
| EnemyProjectilePoints | 100 |  | điểm | wiki:Gameplay#Points (một số đạn đáng 100 điểm) | đạn nào: không rõ |
| TallyEndWaitTicks | 120 | 118–122 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1: tim = 00 f6896 → màn đen f6956 = 60 frame; đen 28 tick tới bản đồ lâu đài — routine 15) | từ tick tim về 0 tới khi báo xong tally |
| TallySfxEveryTicks | 8 |  | tick | đo:routine 02 (SFX 04 đếm TIME và 05 đếm tim kích lại mỗi ~8 tick, lần cuối phát trọn) | TIME: kích ở đv đầu + mỗi 8 tick + đv cuối; tim: mỗi tim |

## 10. Luật chung, HUD, chuyển cảnh

| Khoá | Giá trị | Khoảng đo (min–max) | Đơn vị | Nguồn | Ghi chú |
|---|---|---|---|---|---|
| TimeUnitTicks | 64 |  | tick | đo:routine 12 Tools/cvgp/hud_rules.py (map1_hud.csv: 6 đoạn chạy liền 165 đv / 10556 tick = 63.98 tick/đv; từng đoạn 63.85–63.93, dư ≤ 5 tick) | đếm theo tick; pha đặt lại 0 khi bắt đầu stage/section mới (5 lần: đv đầu giảm sau 60–62 tick ±2) |
| PlayerBarSegments | 16 |  | vạch | wiki:Gameplay#Damage; video:Map_1 HUD |  |
| EnemyBarSegments | 16 |  | vạch | suy luận: wiki ảnh HUD (16 đoạn); video:Map_1 HUD |  |
| DamageDisplayTicksPerBar | 8 | 6–12 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1 ENEMY 3:24.9–3:30.4: mỗi rìu −2 vạch hiện thành 2 bước cách 4/4/4/4/3/4/4 frame (1 lần 6) ⇒ TB 8.2 tick) | bước đầu ngay tick đổi máu (suy luận), sau đó 1 vạch / 8 tick |
| HealDisplayTicksPerBar | 8 | 6–8 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (video boss f627–654: PLAYER 8→16 sau quả cầu, hồi quy 7.88 tick/vạch, dư ≤ 2.4 tick) | planner ước 7 — đo lại 8 |
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
| DoorScrollPx | 128 |  | px | đo:routine 15 (MAP1_PLACEMENT §3: cuộn 128 px hai lần, 2 cửa) | code dùng hằng Map1FlowTiming.DOOR_SCROLL_PX (CVGP 16: đồng bộ số đo vào Config) |
| DoorScrollSpeed | 1.00 |  | px/tick | đo:routine 15 (hồi quy 4 đoạn cuộn 0.999/1.000/1.002/0.995 px/tick — planner 0.90–0.91 SAI) | code dùng Map1FlowTiming.DOOR_SCROLL_SPEED_FIXED 256 (CVGP 16: đồng bộ) |
| DoorPauseTicks | 152 |  | tick | đo:routine 15 (camera dừng 152.8/151.9 tick giữa 2 lần cuộn — planner 2.30–2.33 s SAI) | code dùng Map1FlowTiming.DOOR_SCROLL2_TICK (CVGP 16: đồng bộ) |
| DoorAutoWalkPx | 68 |  | px | đo:routine 15 (Simon tự đi 68 px @ 1 px/tick: 68.3/67.4 tick) | code dùng Map1FlowTiming.DOOR_WALK_PX (CVGP 16: đồng bộ) |
| StageClearToTallyTicks | 260 | 220–260 | tick | đo:routine 12 Tools/cvgp/hud_rules.py: Map_1 nhặt cầu f6391 (= lúc nhạc Stage Clear bắt đầu — tương quan âm thanh 213.05 s NCC 0.81) → TIME giảm đầu f6521 = 260 tick; video boss: hồi máu bắt đầu f627 → f737 = 220 tick | 2 video lệch 40 tick chưa giải thích (nhạc Stage Clear kêu 4.10 s = 246 tick); theo Map_1 (video chính) |
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
| RespawnBlackTicks | 32 |  | tick | suy luận: video/wiki không có cảnh chết — dùng bằng đen Intro→Stage 01 (CVTI NesTiming.BLACK_AFTER_INTRO 32) | Died (sau DeathTicks) → màn đen N tick → đầu stage |
| GameOverBlackTicks | 32 |  | tick | quy ước: video/wiki không có Game Over (= RespawnBlackTicks) | Died khi hết mạng → đen → màn GAME OVER |
| HeartsOnDeath | 5 |  | tim | suy luận: = StartHearts (wiki không nêu tim khi chết) | hồi sinh / Continue ⇒ tim về giá trị này |
| TimeResetOnDeath | 1 |  | bool | suy luận: wiki Gameplay#Simon Belmont (hồi sinh = về đầu stage) — giờ đặt lại = ConfigBlock.timeLimit | 0 = giữ giờ đang còn |
| ContinueResetScore | 1 |  | bool | quy ước: wiki không nói điểm khi Continue (CV_WIKI_DATA §1.4) | 1 = Continue đặt điểm về 0 (và mốc 1UP về OneUpFirst) |
| ShotIconBlinkTicks | 120 | 116–124 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1 icon II nháy f5987–f6047 sau khi nhặt rồi sáng hẳn = 60 frame) | nháy khi vừa có Double/Triple Shot |
| ShotIconBlinkHalfTicks | 4 | 2–6 | tick | đo:routine 12 Tools/cvgp/hud_rules.py (Map_1 f5987–6047: 15 lần hiện TB 2.1 frame, 15 lần ẩn TB 2.4 frame ⇒ ~4 tick mỗi pha) | hiện N tick / ẩn N tick |
| LivesCap | 99 |  | mạng | suy luận: HUD P-dd 2 chữ số |  |
| ScoreCap | 999999 |  | điểm | suy luận: HUD SCORE-dddddd 6 chữ số |  |

## 11. Nội dung vật chứa (ConfigDrop)

| Vật | Stage | Vật chứa | Item | Ghi chú | Nguồn |
|---|---|---|---|---|---|
| CE_T1 | 1 | Torch | LargeHeart | chậu lửa CastleEntrance x 96 y 160 | video:Map_1 0:03.2 vỡ → tim lớn, +5 0:03.93; wiki #B1 Large Heart |
| CE_T2 | 1 | Torch | MorningStar | chậu lửa CastleEntrance x 224 y 160 | video:Map_1 0:05.8 → roi, nhặt 0:06.5; wiki #B2 Morning Star |
| CE_T3 | 1 | Torch | MorningStar | chậu lửa CastleEntrance x 352 y 160 | video:Map_1 0:08.9 → roi, nhặt 0:09.8; wiki #B3 Morning Star |
| CE_T4 | 1 | Torch | LargeHeart | chậu lửa CastleEntrance x 480 y 160 | video:Map_1 0:12.1 → tim lớn, +5 0:12.97; wiki #B4 Large Heart |
| CE_T5 | 1 | Torch | Dagger | chậu lửa CastleEntrance x 608 y 160 | video:Map_1 0:14.5 → dao, nhặt 0:15.50; wiki #B5 Dagger |
| EH_C01 | 1 | WallCandle | PurpleMoneyBag | nến EntranceHall x 32 y 160 | video:Map_1 vỡ f631 → túi tím, +400 f655 (0:21.83); wiki §6.1 nến #1 |
| EH_C02 | 1 | WallCandle | SmallHeart | nến EntranceHall x 96 y 128 | video:Map_1 vỡ f724 → tim nhỏ đung đưa, tim 15→16 f762 — wiki ghi Stopwatch (H1: theo video); wiki §6.1 nến #2 |
| EH_C03 | 1 | WallCandle | SmallHeart | nến EntranceHall x 160 y 160 | video:Map_1 vỡ f761 → tim nhỏ, 16→17 f788; wiki §6.1 nến #3 |
| EH_C04 | 1 | WallCandle | RedMoneyBag | nến EntranceHall x 224 y 128 | video:Map_1 vỡ f821 → túi đỏ, +100 f847; wiki §6.1 nến #4 |
| EH_C05 | 1 | WallCandle | SmallHeart | nến EntranceHall x 288 y 160 | video:Map_1 vỡ f930 → nhặt ngay, tim 18→19 f950; wiki §6.1 nến #5 |
| EH_C06 | 1 | WallCandle | RedMoneyBag | nến EntranceHall x 352 y 128 | video:Map_1 vỡ f979 → túi đỏ, +100 f1001; wiki §6.1 nến #6 |
| EH_C07 | 1 | WallCandle | SmallHeart | nến EntranceHall x 416 y 160 | video:Map_1 vỡ f1008 → tim 19→20 f1071; wiki §6.1 nến #7 |
| EH_C08 | 1 | WallCandle | SmallHeart | nến EntranceHall x 480 y 128 | video:Map_1 vỡ f1089 → tim 20→21 f1125; wiki §6.1 nến #8 |
| EH_C09 | 1 | WallCandle | SmallHeart | nến EntranceHall x 544 y 160 | video:Map_1 vỡ f1166 → tim 21→22 f1190; wiki §6.1 nến #9 |
| EH_C10 | 1 | WallCandle | SmallHeart | nến EntranceHall x 608 y 128 | video:Map_1 vỡ f1210 → tim 22→23 f1248; wiki §6.1 nến #10 |
| EH_C11 | 1 | WallCandle | SmallHeart | nến EntranceHall x 672 y 64 | video:Map_1 vỡ f1493 → tim 23→24 f1564; wiki §6.1 nến #11 |
| EH_C12 | 1 | WallCandle | WhiteMoneyBag | nến EntranceHall x 736 y 160 | video:Map_1 vỡ f1319 → túi trắng, +700 f1340 (0:44.67); wiki §6.1 nến #12 |
| EH_C13 | 1 | WallCandle | HolyWater | nến EntranceHall x 800 y 32 | video:Map_1 vỡ f1698 → nước thánh rơi xuống lối đi (0:57.3, không nhặt) — wiki gán Holy Water cho #14 (H1: theo video); wiki §6.1 nến #13 |
| EH_C14 | 1 | WallCandle | LargeHeart | nến EntranceHall x 864 y 64 | video:Map_1 vỡ f1641 → tim lớn, +5 f1676 (0:55.87) — wiki ghi Holy Water; wiki §6.1 nến #14 |
| EH_C15 | 1 | WallCandle | WhiteMoneyBag | nến EntranceHall x 928 y 160 | video:Map_1 vỡ f1914 → túi trắng, +700 f1935 (1:04.50); wiki §6.1 nến #15 |
| EH_C16 | 1 | WallCandle | SmallHeart | nến EntranceHall x 992 y 64 | video:Map_1 vỡ f1809 → tim 29→30 f1845; wiki §6.1 nến #16 |
| EH_C17 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1056 y 160 | video:Map_1 vỡ f1991 → tim 30→31 f2016; wiki §6.1 nến #17 |
| EH_C18 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1120 y 160 | video:Map_1 vỡ f2030 → tim 31→32 f2058; wiki §6.1 nến #18 |
| EH_C19 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1184 y 160 | video:Map_1 vỡ f2069 → tim nhỏ (dải ảnh), 32→33 f2135; wiki §6.1 nến #19 |
| EH_C20 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1248 y 160 | video:Map_1 vỡ f2150 → tim 33→34 f2178; wiki §6.1 nến #20 |
| EH_C21 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1312 y 160 | video:Map_1 vỡ f2190 → tim 34→35 f2232; wiki §6.1 nến #21 |
| EH_C22 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1376 y 128 | video:Map_1 KHẢ NĂNG: vỡ sau f2268 (bị rosary nháy che), tim 35→36 f2311 — rosary 1:15.03 là Zombie chết rơi ra; wiki §6.1 nến #22 |
| EH_C23 | 1 | WallCandle | SmallHeart | nến EntranceHall x 1440 y 64 | video:Map_1 vỡ f2463 → tim 36→37 f2488 — wiki gán Rosary cho #23; wiki §6.1 nến #23 |
| EH_C24 | 1 | WallCandle | Rosary | nến EntranceHall x 1504 y 32 | video:Map_1 vỡ f2505 (1:23.5) → rosary, nhặt 1:24.73 — wiki: Rosary ở #23 (H1: theo video); wiki §6.1 nến #24 |
| CA_C1 | 2 | WallCandle | WhiteMoneyBag | nến CryptA x 32 y 160 | video:Map_1 vỡ f3152 → +700 f3177 (1:45.90); wiki §6.2 tầng trên #1 |
| CA_C2 | 2 | WallCandle | LargeHeart | nến CryptA x 96 y 64 | video:Map_1 vỡ f2778 → +5 f2827 (1:34.23); wiki §6.2 tầng trên #3 |
| CA_C3 | 2 | WallCandle | SmallHeart | nến CryptA x 160 y 48 | video:Map_1 vỡ f2856 → tim 41→42 f2895; wiki §6.2 tầng trên #5 |
| CA_C4 | 2 | WallCandle | RedMoneyBag | nến CryptA x 224 y 128 | video:Map_1 vỡ f2968 (roi trên thang, dải ảnh _cache/15/ca_bag.png) → túi đỏ, +100 f3004 (1:40.13); wiki §6.2 tầng trên #7 |
| CA_C5 | 2 | WallCandle | LargeHeart | nến CryptA x 304 y 48 | video:Map_1 vỡ f4691 → +5 f4728 (2:37.60); wiki §6.2 tầng trên #9 |
| CA_C6 | 2 | WallCandle | RedMoneyBag | nến CryptA x 352 y 160 | video:Map_1 vỡ f4467 → +100 f4503 (2:30.10); wiki §6.2 tầng trên #11 |
| CA_C7 | 2 | WallCandle | Rosary | nến CryptA x 416 y 32 | video:Map_1 vỡ f4830 → rosary trên lối đi, nhặt 2:42.7 — wiki #13 Rosary (khớp); wiki §6.2 tầng trên #13 |
| CA_C8 | 2 | WallCandle | RedMoneyBag | nến CryptA x 480 y 64 | video:Map_1 vỡ f4798 → túi đỏ, +100 f4859 (2:41.97); wiki §6.2 tầng trên #15 |
| CB_C1 | 2 | WallCandle | WhiteMoneyBag | nến CryptB x 32 y 48 | video:Map_1 KHẢ NĂNG: vỡ sau f3666 (dải ảnh), +700 f3690 (2:03.0); wiki §6.2 tầng dưới #2 (toạ độ wiki − (16,160)) |
| CB_C2 | 2 | WallCandle | RedMoneyBag | nến CryptB x 96 y 48 | video:Map_1 vỡ f3485–3500 (ảnh _cache/15/cb_start.png) → túi đỏ, +100 f3502 (1:56.73); wiki §6.2 tầng dưới #4 (toạ độ wiki − (16,160)) |
| CB_C3 | 2 | WallCandle | RedMoneyBag | nến CryptB x 160 y 48 | video:Map_1 vỡ f3739 → +100 f3768; wiki §6.2 tầng dưới #6 (toạ độ wiki − (16,160)) |
| CB_C4 | 2 | WallCandle | WhiteMoneyBag | nến CryptB x 224 y 48 | video:Map_1 vỡ f3849 → +700 f3868 (2:08.93); wiki §6.2 tầng dưới #8 (toạ độ wiki − (16,160)) |
| CB_C5 | 2 | WallCandle | Stopwatch | nến CryptB x 288 y 48 | video:Map_1 vỡ f3917 → đồng hồ (không nhặt) — wiki #10 Stopwatch (khớp); wiki §6.2 tầng dưới #10 (toạ độ wiki − (16,160)) |
| CB_C6 | 2 | WallCandle | SmallHeart | nến CryptB x 352 y 48 | video:Map_1 KHẢ NĂNG: tim 47→48 f4007 (vỡ không bắt được — dải ảnh); wiki §6.2 tầng dưới #12 (toạ độ wiki − (16,160)) |
| CB_C7 | 2 | WallCandle | SmallHeart | nến CryptB x 416 y 48 | video:Map_1 vỡ f4032 → tim 48→49 f4069; wiki §6.2 tầng dưới #14 (toạ độ wiki − (16,160)) |
| CB_C8 | 2 | WallCandle | LargeHeart | nến CryptB x 496 y 48 | video:Map_1 vỡ f4114 → +5 f4155 (2:18.50); wiki §6.2 tầng dưới #16 (toạ độ wiki − (16,160)) |
| PB_C01 | 3 | WallCandle | LargeHeart | nến PhantomBat x 32 y 128 | video:Map_1 vỡ f5434 → +5 f5456 (3:01.87); wiki §6.3 nến #1 |
| PB_C02 | 3 | WallCandle | SmallHeart | nến PhantomBat x 96 y 64 | video:Map_1 vỡ f5137 → tim 60→61 f5170; wiki §6.3 nến #2 |
| PB_C03 | 3 | WallCandle | RedMoneyBag | nến PhantomBat x 160 y 96 | video:Map_1 vỡ f5249 → +100 f5274; wiki §6.3 nến #3 |
| PB_C04 | 3 | WallCandle | InvisibilityPotion | nến PhantomBat x 224 y 64 | video:Map_1 vỡ f5315 (2:57.2) → bình, nhặt 2:58.2 — wiki: bình "giữa cầu thang", ứng viên #3/#4; wiki §6.3 nến #4 |
| PB_C05 | 3 | WallCandle | SmallHeart | nến PhantomBat x 288 y 160 | video:Map_1 vỡ f5594 → tim 66→67 f5624; wiki §6.3 nến #5 |
| PB_C06 | 3 | WallCandle | Axe | nến PhantomBat x 352 y 128 | video:Map_1 vỡ f5663 → rìu, nhặt 3:10.67 — wiki #6 Axe (khớp); wiki §6.3 nến #6 |
| PB_C07 | 3 | WallCandle | SmallHeart | nến PhantomBat x 416 y 160 | video:Map_1 vỡ f5739 → tim 67→68 f5769; wiki §6.3 nến #7 |
| PB_C08 | 3 | WallCandle | SmallHeart | nến PhantomBat x 480 y 160 | video:Map_1 vỡ f5782 → tim 68→69 f5813; wiki §6.3 nến #8 |
| PB_C09 | 3 | WallCandle | LargeHeart | nến PhantomBat x 544 y 128 | video:Map_1 vỡ f5829 → +5 f5858 (3:15.27); wiki §6.3 nến #9 |
| PB_C10 | 3 | WallCandle | SmallHeart | nến PhantomBat x 608 y 160 | video:Map_1 vỡ f5880 → tim 74→75 f5907; wiki §6.3 nến #10 |
| PB_C11 | 3 | WallCandle | SmallHeart | nến PhantomBat x 672 y 128 | video:Map_1 vỡ f5943 → tim 75→76 f5973; wiki §6.3 nến #11 |
| PB_C12 | 3 | WallCandle | LargeHeart | nến PhantomBat x 736 y 128 | video:Map_1 vỡ f6000 → +5 f6040 (3:21.33); wiki §6.3 nến #12 |
| EntranceHall_X63_7 | 1 | BreakableBlock | PurpleMoneyBag | khối phải nhất bục Panther 3 (ô lưới 06 (63,7)) | wiki:Secrets#Miscellaneous (Purple money bag 400) — video không phá |
| CryptA_X16_10 | 2 | BreakableBlock | PotRoast | tường khối nhỏ — khối thấp nhất (ô (16,10)) | video:Map_1 1:39.6 (roi khi ngồi, lộ thịt nướng NGAY); wiki:Secrets Pot Roast Stage 2 |
| CryptA_X16_9 | 2 | BreakableBlock | None | khối trên (ô (16,9)) — vỡ không ra gì | video:Map_1 1:40.55 (đập vỡ, không item) |
| CryptB_X29_6 | 2 | BreakableBlock | None | khối trên-phải (ô (29,6)) — phá để thả xuống, bí mật = ngồi trên khối thấp (SecretTrigger) | video:Map_1 2:19.05; wiki:Secrets Bonuses Stage 2 |
| PhantomBat_X43_9 | 3 | BreakableBlock | DoubleShot | khối PHẢI bệ chân thang phòng boss (ô (43,9)) | video:Map_1 3:17.62–3:18.0 (II rơi ra); wiki:Secrets Shot Multiplier Stage 3 |
| BossMummy_X6_10 | 9 | BreakableBlock | PotRoast | phòng Mummy thử (Arena, CVGP 19): khối thấp nhất cầu thang trái — ô (6,10) | wiki:Bosses#Mummy Men + Stages 7-9#Stage 9 (Pot Roast ở khối thấp nhất cầu thang trái phòng boss) |


