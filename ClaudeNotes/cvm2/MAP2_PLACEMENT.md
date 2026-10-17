# MAP2_PLACEMENT — vị trí vật thể Map 2 (Block 2 = Stage 04–06) đo từ video Map_2 (CVM2 10, 17/10/2026)

> Nguồn sự thật cho dữ liệu: `Assets/Data/Config/Level/Block2/Map2Objects.json` (sinh bởi `python -P -E Tools/cvm2/gen_map2_objects.py`) → menu
> **`CVM2/Build Map2 Objects`** → `Assets/Data/Prefabs/Level/Block2/Objects/<Section>_Objects.prefab` (lớp vật thể RIÊNG — không lồng vào prefab
> section; luồng thật gắn ở routine 11 như Map 1 gắn trong `StageScreen`); nội dung nến/khối = `Assets/Data/Config/Csv/ConfigDrop.csv` (id
> `TEA_C*`, `TEB_C*`, `HA_C*`, `HB_C*`, `HC_C*`, `MD_C*`, `<Section>_X<c>_<r>` — stage 4–6, ghi CUỐI file) → `CVGP/Import Config`.
> Phòng boss: `Tools/cvm2/map2_boss.json` (CVM2 09 — nguồn duy nhất, generator chép nguyên). Sửa bảng → chạy lại generator + menu (lần 2 = "ghi 0").
> Toạ độ = px section (gốc trên-trái vùng 192 dòng của sheet Level 2; **dòng r ↔ NES y 32 + r**; x tâm, Y = mặt đáy pivot như `Entity`).
> Video Map_2: hiệu chuẩn CVM2 02 (`Tools/cvm2/nes2.py`; sx 3.621, sy 3.101, ox 268.888, oy −19.736), camera từng frame `ref/data/map2_camera.csv`
> (khớp nền sheet từng frame — kiểm lại ở khung camera đứng: lệch ±1 px làm sai khác nền tăng 3× ⇒ cx chính xác 0 px), 1 frame video = 2.0033 tick.

## §0 Cách đo
| Việc | Công cụ (từ gốc repo, `python -P -E`) | Kết quả |
|---|---|---|
| Vị trí nến | `Tools/cvm2/props10.py 1 0.09` — khớp mẫu `Object/candle_0/1` (thước đo `enemy8_fit` CVM2 08, bảng màu video tự ước lượng `_cache/10/pal_props.json`) MỌI frame không lặp của 6 section (bỏ đoạn cửa ghép), x section = cx + x màn; gom cụm ±3 px; x = **mốt ở khung camera đứng** (cx f−1 = f = f+1) | **51 nến** (TEA 3 · TEB 7 · Hall A 11 · Hall B 11 · Hall C 10 · Medusa 9), mỗi cụm 23–524 lần thấy, x mốt khung đứng = mốt tất cả (lệch ±1 ở 6 cụm khi camera cuộn). **Không cùng một lưới**: x ≡ 15/16/17 (mod 64) tuỳ đoạn (vd Hall A 81, 145, …, 401 rồi 464, …, 720; Hall B 335 … 719) — đã kiểm cx ở khung camera kẹp biên (cx 0 / 512, sai khác nền ±1 px tăng 3×) và lỗi khớp mẫu ±1 px tăng 3× ⇒ lệch ±1 là THẬT, dùng đúng số đo. Y = đáy đo − 1 (prefab Candle vẽ viewOffset +1 — như Map 1: đo 161 ⇒ 160) |
| Đối chiếu wiki | `Tools/cvm2/wiki10.py` — ảnh bản đồ wiki NỬA tỉ lệ `_cache/wiki_img/Castlevania_Stage_{4,5,6}.png` (khác bảng màu + mờ ⇒ dò dấu thân nến 2 cột hồng cách 2 px) ⇒ `_cache/10/wiki_candles.csv` | **51/51 nến trùng** (đủ số từng section; x lệch ≤ 2 = độ phân giải ảnh, cả mẫu lệch +1 Hall A/C/Medusa); y wiki xấp xỉ (+4 hằng) |
| Khung hiện vật | `Tools/cvm2/window10.py` (histogram nến theo x màn, chỉ frame nến chưa vỡ) | nến thấy ở **100 % frame có x màn 8–248** (≈ 700 frame), **0/223 frame x màn 0–6**, 0/29 frame x màn 249–255; Bone Tower (313, 48) chưa hiện ở x màn 7 (f4841, cx 306) — hiện ở x màn 9 (f4842) ⇒ NES chỉ vẽ/giữ vật trong **[cx + 8, cx + 248]** ⇒ Unity SpawnPoint của nến + enemy Block 2 `activate/despawnMargin −8` (khoá `objectWindowInsetPx` 8 — [cx+8, cx+248), phải lệch 1 px). Block 1 giữ mặc định 16 (H12) |
| Vật bị phá + rơi gì | `Tools/cvm2/drops10.py` — nến mất khỏi khung còn trong tầm (≥ 3 frame) = vỡ; khớp mẫu 17 sprite Item quanh nến 45 frame sau (err < 0.1 = chắc) + sự kiện HUD `ref/data/map2_events.txt` (tim +1/+5, điểm +100/+400/+700) ⇒ `_cache/10/drops.txt`; dải ảnh `Tools/cvm2/strip10.py` (`_cache/10/view/*.png` — đã xem) | **51/51 nến bị phá trong video**, nội dung đọc được hết (§1); 4 vật không nhặt (rơi xuống hố/khe hoặc bỏ qua) xác nhận bằng dải ảnh |
| Bí mật | khớp mẫu `crown_0..3` / `chest_0..3` từng frame (`enemy8_fit.py` + `Tools/cvm2/rise10.py` chỉ chấm hàng trên mặt sàn) + Simon `ref/data/map2_simon.csv` | §3 |
| Enemy | vết CVM2 08 (`_cache/08/trk_*.csv`, `ref/data/enemy8/mh_spawns.csv`) + Simon từng frame | §4 |
| So Unity ↔ video | `Assets/Data/Script/Debug/Map2ObjectsProbe.cs` (Arena Play: kiểm + chụp cùng cx) → `Tools/cvm2/pairs10.py select|compare` | §6 |

## §1 Nến (51) — `SpawnPoint` prefab `Candle` (`dataKey` = id ⇒ `CandleProp.objectId` ⇒ ConfigDrop), không sinh lại khi đã phá, sinh lại khi chỉ ra khỏi màn (`keepIfOffscreen`) — như Map 1
Nến tường Block 2 đều là `candle` (không có chậu lửa). Video: Simon phá **cả 51 nến** (roi / thập tự) ⇒ mọi nội dung là số đo video (cột nguồn); wiki chỉ nêu Boomerang (TEA nến đầu), dao găm (Hall A), Stop Watch (Hall B) — khớp video (dao: icon wiki ≈ x 526, video nến x 464 — H1 theo video). `ItemDrops.Resolve` (tim nhỏ ⇒ roi khi roi < 3 / ⇒ II sau 10 đòn) giữ nguyên luật CVGP 11: video Simon roi cấp 3 + đồng hồ/thập tự, không có lần đổi.

#### TowerEntranceA (3 nến, stage 4)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| TEA_C1 | 80 | 128 | Cross | 23 lần f187–f214; x 80×23 (đứng 22/22); đáy đo 129 | (80, 132) | video:Map_2 vỡ f215 (0:07.17) → thập tự rơi f225–237 (dải ảnh tea80.png), nhặt f274 0:09.13 (HUD rìu → thập tự); wiki Stage 4 #1 "Boomerang nến đầu tiên" |
| TEA_C2 | 144 | 32 | RedMoneyBag | 443 lần f187–f714; x 144×443 (đứng 442/442); đáy đo 33 | (144, 36) | video:Map_2 vỡ f715 → túi đỏ f726 (err 0.066), +100 f734 (0:24.47) |
| TEA_C3 | 208 | 128 | LargeHeart | 98 lần f187–f313; x 208×98 (đứng 97/97); đáy đo 129 | (208, 132) | video:Map_2 vỡ f314 → tim lớn f335 (0.062), +5 f341 (0:11.37) |

#### TowerEntranceB (7 nến, stage 4)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| TEB_C1 | 81 | 32 | RedMoneyBag | 64 lần f1344–f1425; x 81×64 (đứng 35/35); đáy đo 33 | (80, 36) | video:Map_2 vỡ f1426 → túi đỏ f1438 (0.064), +100 f1506 (0:50.20, cạnh bệ) |
| TEB_C2 | 145 | 32 | WhiteMoneyBag | 97 lần f1243–f1356; x 145×97 (đứng 57/57); đáy đo 33 | (144, 36) | video:Map_2 vỡ f1357 → túi trắng f1373 (0.092), +700 f1375 (0:45.83) |
| TEB_C3 | 208 | 32 | SmallHeart | 92 lần f1163–f1274; x 208×92 (đứng 47/47); đáy đo 33 | (206, 36) | video:Map_2 vỡ f1275 → tim nhỏ f1289 (0.056), +1 f1311 |
| TEB_C4 | 272 | 32 | SmallHeart | 271 lần f866–f1201; x 272×271 (đứng 226/226); đáy đo 33 | (270, 36) | video:Map_2 vỡ f1202 → tim nhỏ f1217 (0.049), +1 f1236 |
| TEB_C5 | 336 | 32 | SmallHeart | 140 lần f866–f1031; x 336×140 (đứng 139/139); đáy đo 33 | (334, 36) | video:Map_2 vỡ f1032 → tim nhỏ f1073 (0.056) rơi xuống khe — không nhặt |
| TEB_C6 | 400 | 32 | SmallHeart | 128 lần f866–f1015; x 400×128 (đứng 127/127); đáy đo 33 | (398, 36) | video:Map_2 vỡ f1016 → tim nhỏ f1052 (0.054), +1 f1125 |
| TEB_C7 | 464 | 32 | RedMoneyBag | 145 lần f866–f1048; x 464×145 (đứng 144/144); đáy đo 33 | (462, 36) | video:Map_2 vỡ f1049 → túi đỏ f1079 (0.065), +100 f1082 (0:36.07) |

#### TowerHallA (11 nến, stage 5)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| HA_C01 | 81 | 32 | SmallHeart | 52 lần f2639–f2707; x 81×52 (đứng 22/22); đáy đo 33 | (82, 36) | video:Map_2 vỡ f2708 → tim nhỏ f2734 (0.062), +1 f2741 |
| HA_C02 | 145 | 64 | LargeHeart | 106 lần f2517–f2655; x 145×106 (đứng 50/50); đáy đo 65 | (146, 68) | video:Map_2 vỡ f2656 → tim lớn f2672 (0.073), +5 f2673 (1:29.10) |
| HA_C03 | 209 | 32 | SmallHeart | 178 lần f2291–f2594; x 209×178 (đứng 95/95); đáy đo 33 | (210, 36) | video:Map_2 vỡ f2595 → tim nhỏ f2611 (0.060), +1 f2629 |
| HA_C04 | 273 | 64 | LargeHeart | 251 lần f2210–f2540; x 273×251 (đứng 91/91); đáy đo 65 | (274, 68) | video:Map_2 vỡ f2541 → tim lớn f2558 (0.066), +5 f2564 (1:25.47) |
| HA_C05 | 337 | 32 | RedMoneyBag | 90 lần f2155–f2260; x 337×90 (đứng 28/28); đáy đo 33 | (338, 36) | video:Map_2 vỡ f2261 → túi đỏ f2277 (0.073), +100 f2283 |
| HA_C06 | 401 | 32 | SmallHeart | 61 lần f2089–f2177; x 401×61 (đứng 25/25); đáy đo 33 | (402, 36) | video:Map_2 vỡ f2178 → tim nhỏ f2220 (0.059), +1 f2237 |
| HA_C07 | 464 | 32 | Dagger | 68 lần f2014–f2102; x 464×68 (đứng 37/37); đáy đo 33 | (464, 36) | video:Map_2 vỡ f2103 (1:10.10) → dao găm rơi xuống sàn f2116–2124, nằm tới f2160 — không nhặt (dải ảnh ha464.png); wiki Stage 5 #1 "dao găm trong nến gần giữa tầng dưới" (icon wiki ≈ x 526 — H1: theo video) |
| HA_C08 | 528 | 32 | RedMoneyBag | 212 lần f1762–f2027; x 528×212 (đứng 184/184); đáy đo 33 | (528, 36) | video:Map_2 vỡ f2028 → túi đỏ f2056 (0.066) trên sàn, +100 f2082 (tim +1 f2073 là từ Medusa Head bị hạ f2056 — ha528.png) |
| HA_C09 | 592 | 32 | SmallHeart | 159 lần f1762–f1960; x 592×159 (đứng 148/148); đáy đo 33 | (592, 36) | video:Map_2 vỡ f1961 → tim nhỏ f1981 (0.057), +1 f2012 |
| HA_C10 | 656 | 32 | SmallHeart | 101 lần f1762–f1890; x 656×101 (đứng 101/101); đáy đo 33 | (656, 36) | video:Map_2 vỡ f1891 → tim nhỏ f1912 (0.060), +1 f1930 |
| HA_C11 | 720 | 32 | SmallHeart | 38 lần f1762–f1810; x 720×38 (đứng 38/38); đáy đo 33 | (720, 36) | video:Map_2 vỡ f1811 → tim nhỏ f1823 (0.059), +1 f1845 |

#### TowerHallB (11 nến, stage 5)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| HB_C01 | 80 | 128 | WhiteMoneyBag | 53 lần f2801–f2858; x 80×53 (đứng 52/52); đáy đo 129 | (80, 132) | video:Map_2 vỡ f2859 → túi trắng f2871–2879 rơi xuống hố cạnh chân thang — không nhặt (dải ảnh hb80.png, err 0.094) |
| HB_C02 | 144 | 32 | SmallHeart | 260 lần f2801–f4156; x 144×233/145×27 (đứng 173/173); đáy đo 33 | (144, 36) | video:Map_2 vỡ f4157 → tim nhỏ f4197 (0.057), +1 f4208 |
| HB_C03 | 208 | 120 | RedMoneyBag | 170 lần f2801–f3001; x 208×170 (đứng 147/147); đáy đo 121 | (208, 124) | video:Map_2 vỡ f3002 → túi đỏ f3014–3022 rơi xuống khe — không nhặt (dải ảnh hb208.png, err 0.062) |
| HB_C04 | 272 | 32 | SmallHeart | 280 lần f2960–f4059; x 272×209/273×71 (đứng 97/110); đáy đo 33 | (272, 36) | video:Map_2 vỡ f4060 → tim nhỏ f4072 (0.057), +1 f4095 |
| HB_C05 | 335 | 120 | SmallHeart | 103 lần f3031–f3154; x 335×103 (đứng 39/39); đáy đo 121 | (336, 124) | video:Map_2 vỡ f3155 → tim nhỏ f3165 (0.064), +1 f3171 |
| HB_C06 | 399 | 32 | SmallHeart | 272 lần f3084–f3936; x 399×229/401×43 (đứng 109/109); đáy đo 33 | (400, 36) | video:Map_2 vỡ f3937 → tim nhỏ f3965 (0.058), +1 f3972 |
| HB_C07 | 463 | 120 | RedMoneyBag | 143 lần f3121–f3281; x 463×143 (đứng 94/94); đáy đo 121 | (464, 124) | video:Map_2 vỡ f3282 → túi đỏ f3294 (0.060), +100 f3303 |
| HB_C08 | 527 | 32 | Stopwatch | 524 lần f3209–f3805; x 527×523/528×1 (đứng 303/303); đáy đo 33 | (528, 36) | video:Map_2 vỡ f3806 → đồng hồ f3840 (0.091), nhặt f3877 2:09.23 (HUD thập tự → đồng hồ); wiki Stage 5 #4 "Stop Watch trong nến gần Pot Roast" |
| HB_C09 | 591 | 32 | RedMoneyBag | 414 lần f3311–f3789; x 591×413/592×1 (đứng 226/226); đáy đo 33 | (592, 36) | video:Map_2 vỡ f3790 → túi đỏ f3813 (0.065), +100 f3830 |
| HB_C10 | 655 | 128 | WhiteMoneyBag | 50 lần f3343–f3397; x 655×49/656×1 (đứng 8/8); đáy đo 129 | (656, 132) | video:Map_2 vỡ f3398 → túi trắng f3408 (0.092), +700 f3420 (1:54.00) |
| HB_C11 | 719 | 32 | RedMoneyBag | 188 lần f3378–f3689; x 719×188 (đứng 108/108); đáy đo 33 | (720, 36) | video:Map_2 vỡ f3690 → túi đỏ f3716 (0.059), +100 f3728 |

#### TowerHallC (10 nến, stage 6)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| HC_C01 | 81 | 96 | LargeHeart | 168 lần f5099–f5319; x 81×168 (đứng 94/94); đáy đo 97 | (82, 100) | video:Map_2 vỡ f5320 → tim lớn f5348 (0.068), +5 f5349 (2:58.30) |
| HC_C02 | 145 | 120 | SmallHeart | 167 lần f4974–f5200; x 145×166/144×1 (đứng 96/96); đáy đo 121 | (146, 124) | video:Map_2 vỡ f5201 → tim nhỏ f5219 (0.049), +1 f5269 |
| HC_C03 | 209 | 120 | SmallHeart | 102 lần f4930–f5057; x 209×101/208×1 (đứng 47/47); đáy đo 121 | (210, 124) | video:Map_2 vỡ f5058 → tim nhỏ f5068 (0.066), +1 f5083 |
| HC_C04 | 273 | 128 | RedMoneyBag | 51 lần f4888–f4947; x 273×51 (đứng 14/14); đáy đo 129 | (274, 132) | video:Map_2 vỡ f4948 → túi đỏ f4959 (0.066), +100 f4965 |
| HC_C05 | 337 | 128 | RedMoneyBag | 76 lần f4819–f4908; x 337×76 (đứng 20/20); đáy đo 129 | (338, 132) | video:Map_2 vỡ f4909 → túi đỏ f4919 (0.073), +100 f4922 |
| HC_C06 | 401 | 128 | PurpleMoneyBag | 63 lần f4757–f4836; x 401×63 (đứng 24/24); đáy đo 129 | (402, 132) | video:Map_2 vỡ f4837 → túi tím f4847 (0.067), +400 f4853 (2:41.77) |
| HC_C07 | 464 | 128 | HolyWater | 138 lần f4678–f4870; x 464×138 (đứng 53/53); đáy đo 129 | (464, 132) | video:Map_2 vỡ f4871 → nước thánh f4909 (0.074) nằm sàn — không nhặt (HUD giữ đồng hồ); wiki ảnh bản đồ Stage 6 icon nước thánh giữa tầng dưới |
| HC_C08 | 528 | 128 | SmallHeart | 152 lần f4481–f4689; x 528×152 (đứng 119/119); đáy đo 129 | (528, 132) | video:Map_2 vỡ f4690 → tim nhỏ f4719 (0.052), +1 f4723 |
| HC_C09 | 592 | 128 | SmallHeart | 101 lần f4481–f4620; x 592×101 (đứng 92/92); đáy đo 129 | (592, 132) | video:Map_2 vỡ f4621 → tim nhỏ f4649 (0.051), +1 f4652 |
| HC_C10 | 656 | 128 | SmallHeart | 61 lần f4481–f4567; x 656×61 (đứng 61/61); đáy đo 129 | (656, 132) | video:Map_2 vỡ f4568 → tim nhỏ f4589 (0.058), +1 f4590 |

#### Medusa (9 nến, stage 6)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp; x: mốt tất cả / số lần ở khung camera đứng) | wiki ảnh bản đồ (x, y ±4) | nguồn nội dung |
|---|---|---|---|---|---|---|
| MD_C1 | 209 | 96 | LargeHeart | 78 lần f6576–f6670; x 209×78 (đứng 40/40); đáy đo 97 | (210, 100) | video:Map_2 vỡ f6671 → tim lớn f6684 (0.072), +5 f6690 (3:43.00) |
| MD_C2 | 273 | 88 | SmallHeart | 65 lần f6530–f6603; x 273×65 (đứng 24/24); đáy đo 89 | (274, 92) | video:Map_2 vỡ f6604 → tim nhỏ f6623 (0.060), +1 f6629 |
| MD_C3 | 337 | 88 | SmallHeart | 77 lần f6391–f6538; x 337×77 (đứng 20/20); đáy đo 89 | (338, 92) | video:Map_2 vỡ f6539 → tim nhỏ f6558 (0.050), +1 f6565 |
| MD_C4 | 401 | 88 | SmallHeart | 135 lần f6300–f6466; x 401×131/400×4 (đứng 58/58); đáy đo 89 | (402, 92) | video:Map_2 vỡ f6467 → tim nhỏ f6493 (0.054), +1 f6526 |
| MD_C5 | 464 | 88 | SmallHeart | 76 lần f6232–f6331; x 464×76 (đứng 51/51); đáy đo 89 | (464, 92) | video:Map_2 vỡ f6332 → tim nhỏ f6351 (0.054), +1 f6389 |
| MD_C6 | 528 | 88 | Rosary | 282 lần f5924–f6254; x 528×282 (đứng 242/242); đáy đo 89 | (528, 92) | video:Map_2 vỡ f6255 → rosary f6292 (0.068), nhặt ⇒ nháy màn f6304 3:30.13 |
| MD_C7 | 592 | 88 | SmallHeart | 193 lần f5924–f6159; x 592×193 (đứng 187/187); đáy đo 89 | (592, 92) | video:Map_2 vỡ f6160 → tim nhỏ f6178 (0.050), +1 f6194 (tim +1 f6242 là từ Medusa Head bị hạ f6227 — md_heart.png) |
| MD_C8 | 656 | 88 | SmallHeart | 149 lần f5924–f6108; x 656×149 (đứng 148/148); đáy đo 89 | (656, 92) | video:Map_2 vỡ f6109 → tim nhỏ f6125 (0.051), +1 f6134 |
| MD_C9 | 720 | 88 | SmallHeart | 80 lần f5924–f6019; x 720×80 (đứng 79/79); đáy đo 89 | (720, 92) | video:Map_2 vỡ f6020 → tim nhỏ f6048 (0.056), +1 f6062 |

## §2 Khối phá được (6 — ô Breakable lưới CVM2 05, `BreakableBlock` tự sinh; nội dung ConfigDrop `<Section>_X<c>_<r>`)
| ObjectId | Ô (px) | Nội dung | Video | Wiki |
|---|---|---|---|---|
| `TowerEntranceA_X14_5` | x 224–239, y 80–95 | None | roi vỡ f469 (0:15.63) — không rơi gì (`view/tea_wall.png`) | Stage 4 #1 "phá các khối tường bên phải" |
| `TowerEntranceA_X14_6` | x 224–239, y 96–111 | None | roi vỡ f486 (0:16.20) — không rơi gì | (như trên) — 2 ô mở khe cho bí mật vương miện §3 |
| `TowerEntranceB_X22_5` | x 352–367, y 80–95 | **DoubleShot** | **video không phá** (Simon không tới góc bục) | Stage 4 #2 "phá khối góc bục bên phải ⇒ Shot Multiplier" + icon **II** ảnh bản đồ wiki ⇒ **CHỈ WIKI** |
| `TowerHallB_X39_3` | x 624–639, y 48–63 | PotRoast | roi vỡ f3474–3478 (1:55.9) → thịt nướng rơi xuống bậc dưới f3486 (`view/hb_roast.png`), không nhặt (đầy máu) | Stage 5 #4 Pot Roast góc trên-phải bậc cao nhất |
| `TowerHallC_X44_7` | x 704–719, y 112–127 | PotRoast | Simon ngồi quất f4524–4528 (2:30.8) → thịt nướng lộ ngay, nhặt khi đầy máu (`view/hc_roast.png`) | Stage 6 #1 Pot Roast khối đầu stage |
| `TowerHallC_X8_8` | x 128–143, y 128–143 | **DoubleShot** | roi vỡ f5110–5116 (2:50.4) → **II** (Double Shot: icon xanh-kem = `Item/double.png`; KHÔNG phải III đỏ — CVM2 02 ghi nhầm "III nằm sàn tới ≥ 3:08.6": ảnh `view/hc_iii_zoom.png`, `view/hc_full.png` cho thấy item bị nhặt f5148) → nhặt khi đang cầm **đồng hồ ⇒ +700** (HUD 2:51.60 — CVM2 02 ghi "+700 túi trắng" cũng nhầm; khớp luật `ItemEffects` có sẵn) | Stage 6 #3 Shot Multiplier khối chân cầu thang |

**Shot Multiplier "nâng cấp theo trạng thái"?** (yêu cầu routine) — video: Simon mất II lúc nhặt thập tự 0:09.13 và KHÔNG có hệ số nào suốt Map_2 ⇒ khối Hall C rơi **II** đúng với cả 2 cách hiểu (cố định / "II nếu chưa có"). Không có bằng chứng khối đổi thành III khi đã có II ⇒ giữ nội dung CỐ ĐỊNH DoubleShot (như `PhantomBat_X43_9` Block 1); `ItemDrops.Resolve` chỉ đổi ô tim nhỏ (luật CVGP 11) — không sửa.

## §3 Bí mật (5 mục wiki `MAP2_WIKI_DATA §2` + `CV_WIKI_DATA §2.2`)
| # | Section | Điều kiện (SecretTrigger) | Vật · nơi hiện | Video (số đo) | Wiki | Unity (Map2ObjectsProbe) |
|---|---|---|---|---|---|---|
| 1 | TowerEntranceA | **Enter** vùng pivot [217, 96, 23, 17] (x 217–239, Y 112) — chỉ tới được khi đã phá cả 2 ô (14,5)/(14,6) (thân 16 px chồng ô khe khi x ≥ 217; chưa phá tường chặn ở x ≤ 216) | **Crown 2000** trồi tại **(136, 177)** dưới cầu thang, 0.25 px/tick | Simon x 220 f502 → 225 f504 → 232 f508; vương miện khớp mẫu x 136 (lật 135/136), đáy đo 178 (⇒ Y 177, 1 px dưới mặt sàn 176 — như rương), đi lên **1 px / 2 frame** f512–f531 (188→178) ⇒ 0.25 px/tick, ngoại suy bắt đầu trồi 16 px ≈ f499–500 ≈ lúc thân Simon chồng khe; nhặt f593 (0:19.77) +2000 | #1 "bước vào khe ⇒ vương miện nhấp nháy hiện dưới cầu thang" | chưa phá: dừng x 216, không hiện ✓; đã phá + đi phải: kích hoạt ở **x 217**, trồi **64 tick**, nghỉ (136, 177), nhặt **+2000** ✓; ảnh `crown_rest` ↔ video f560: **dx +1 (lật) dy 0** |
| 2 | TowerEntranceB | phá khối `TowerEntranceB_X22_5` (§2) | **DoubleShot** rơi tại ô | **không có trong video** | #2 "phá khối góc bục ⇒ Shot Multiplier", icon II | khối ⇒ DoubleShot; nhặt khi cầm đồng hồ ⇒ +700 ✓ — **CHỈ WIKI** |
| 3 | TowerHallA | **StandStill** (Simon trạng thái Idle) trong vùng [288, 32, 32, 17] = trên 2 khối nhô (cột 18–19, mặt y 48) ≥ **120 tick** | **TreasureChest 2000** trồi tại **(400, 81)** (khối sàn bên phải), 0.25 px/tick | nhảy lên khối f2288–2297, bước cuối f2312 (x 307), đứng yên x 306 từ f2314; rương khớp mẫu x 400, đáy đo 82 (⇒ Y 81), lên 1 px / 2 frame tới đỉnh f2406 ⇒ bắt đầu ≈ f2374 ⇒ **đứng yên ≈ 121 tick** (±3) ⇒ 120; nhặt f2441 +2000 (P 03→04) | #2 "đứng trên 2 khối nhô cao một lúc ⇒ Treasure Box trồi từ khối bên phải" | kích hoạt sau **121 tick** (giữ ≥ 120 + tick trồi), trồi 64 tick, nghỉ (400, 81), nhặt **+2000** ✓; đứng yên ngoài 2 khối (x 360) 160 tick ⇒ không ✓; ảnh `chest_rest` ↔ f2420: **dx 0 dy 0** |
| 4 | TowerHallB / TowerHallC | khối `TowerHallB_X39_3` (Pot Roast), `TowerHallC_X44_7` (Pot Roast), `TowerHallC_X8_8` (Shot Multiplier) — §2 | PotRoast / PotRoast / DoubleShot | §2 (cả 3 có video) | Stage 5 #4, Stage 6 #1, #3 | 3 khối ⇒ đúng item; II nhặt +700 ✓ |
| 5 | Medusa | **StandStill** vùng [471, 112, 43, 17] = **vòm thứ 4** sảnh (khoảng tối x 471–513 trên sheet hàng y 100/120 — đếm từ trái 288·343·407·**471** hay từ phải 663·599·535·**471** đều là vòm giữa), sàn y 128, ≥ 120 tick | **FlashingMoneyBag 1000** trồi giữa vòm **(492, 129)** | **không có trong video** (Simon không đứng yên ≥ 24 tick ở x 471–513: f6343–6351, f6417–6427) | Secrets Bonuses Stage 6 "đứng yên vài giây ở vòm thứ 4 sảnh có Medusa Head ⇒ money bag" | đứng mép trái vòm (x 474): kích hoạt 121 tick, trồi 64, nghỉ (492, 129), nhặt **+1000** ✓ — **CHỈ WIKI**; holdTicks = suy luận theo #3 (cùng luật "đứng yên"), điểm trồi = suy luận (wiki không nói); Simon đứng GIỮA vòm ⇒ thân chồng túi ⇒ nhặt ngay lúc trồi (chạy thử lần 1) |

**Tốc độ trồi Block 2 = 0.25 px/tick** (`SecretTriggerSpec.riseSpeed`, cả 3 bí mật) — video Map_2: vương miện + rương đều lên đúng 1 px mỗi 2 frame video (= 4.0 tick). Block 1 giữ `ConfigRule.SecretRiseSpeed` 0.3125 (CVGP 11 "16 px trong ~26 frame" — đo thô; có thể cũng là 0.25 — §6, H12 không đổi Block 1). Vị trí nghỉ của vật bí mật = mặt sàn + 1 (2/2 video) — vật rơi thường nghỉ đúng mặt sàn (Y = đáy đo − 1 = sàn).

## §4 Enemy, bộ sinh, cơ chế, boss (lớp `<Section>_Objects`)
SpawnPoint enemy: `respawn = true` (như Map 1 — sinh lại khi điểm sinh ra khỏi khung rồi vào lại), khung hiện **[cx + 8, cx + 248)** (§0). Toạ độ = điểm đặt; "lần đầu thấy" khi đã đi = XẤP XỈ (ghi rõ).
| Section | Vật | Thông số | Nguồn |
|---|---|---|---|
| TowerEntranceA | `SpearKnight_TEA` | (142, 80) quay TRÁI | video f187 (0:06.23, section hiện) đúng chỗ này rồi đi trái 0.5 px/tick (CVM2 08 `trk_sk_tea`) — ảnh f187/f188 ↔ Unity dx 0/+1 |
| TowerEntranceA | `Bat_TEA` | (48, 33) treo (`param 1` ⇒ `Bat.Perch`) | video f187–776 (CVM2 08 §2j) — ảnh dx 0 dy 0 |
| TowerEntranceB | `SpearKnight_TEB_gap` | (336, 77) quay PHẢI | video f866 đang rơi vào khe x 320–351 (sàn y 144); điểm đặt + hướng theo CVM2 08 `Map2EnemyProbe sk_teb` (g 0.125 ⇒ đáy đo 81 sau 6 tick). Wiki #2 "Spear Knight đứng bên dưới" chỗ khối II ✓ |
| TowerEntranceB | `SpearKnight_TEB_door` | (145, 80) quay PHẢI | video f1244 lần đầu thấy đủ hình x màn 10 (cx 135, cuộn trái) — XẤP XỈ (chưa thấy lúc sinh; khung hiện x màn 8 ⇒ sai ≤ 2 px); wiki #3 "Spear Knight trước cửa cuối" |
| TowerEntranceB | `Bat_TEB` | (304, 33) treo | video f866–1040 (CVM2 08) — ảnh dx 0 dy 0 |
| TowerEntranceB | `Mech_TEB` (MechSpawner) | marker `platform_lane_TEB` ⇒ 1 bệ | CVM2 06 §2i (Config `Object.MovingPlatform` TEB) |
| TowerHallA | `SpearKnight_HA` | (211, 80) quay PHẢI | video f2293 lần đầu thấy x màn 15 (cx 196) rồi quay trái 211→186 — XẤP XỈ |
| TowerHallA | `MedusaHead_HA` EdgeSpawner FrontOfPlayer, playerRelativeY | vùng Simon x **[550, 4096]** | video: 5 con khi Simon x 720 → 581 (đỉnh sóng f1770–2026 cách 128 tick), không có lượt kế f2090 (Simon 518) ⇒ biên ∈ (518, 581] — **550 XẤP XỈ**; con đầu 17 tick (firstDelay Config CVM2 08) |
| TowerHallA | `Secret_TreasureChest` | §3 #3 | |
| TowerHallB | `SpearKnight_HB_low` | (494, 144) quay TRÁI | video f3193 lần đầu thấy x màn 245 (cuộn phải) — XẤP XỈ |
| TowerHallB | `SpearKnight_HB_up1` / `_up2` | (526, 80) / (718, 80) quay TRÁI | video f3209 / f3380 x màn 246 / 245 (CVM2 08 `Map2EnemyProbe sk_hb`) |
| TowerHallB | `MedusaHead_HB` | vùng x **[0, 288]** (chung 2 tầng) | lượt dưới (đi phải) 5 con Simon 94 → 279, không ở 352; lượt trên (đi trái) con đầu f4092.6 lúc Simon 277 ⇒ vào vùng 17–56 tick trước = Simon 281–295; sinh tới 120 ⇒ biên phải ∈ [281, 295] — **288 XẤP XỈ** |
| TowerHallB | `Mech_HB` | marker `platform_lane_HallB_1`, `_2` ⇒ 2 bệ | CVM2 06 §2i |
| TowerHallC | `Bat_HC` | (145, 57) treo | video f4974–5101 (rời chỗ f5043 khi Simon nhảy — CVM2 08) — ảnh f4985 dx 0 dy 0 |
| TowerHallC | `BoneTower_HC_low` / `_up` | (249, 80) / (313, 48) | video 2:41.4–3:08.43 (CVM2 08 `bt_c`) — ảnh f4941/f4945/f4985/f5112 dx 0 dy 0 (đầu trên + đầu dưới) |
| TowerHallC | `Ghost_HC_low` EdgeSpawner **BehindPlayer** | vùng x **[0, 256]**, **y [129, 192]** (tầng dưới), **maxSpawns 1** | Ghost #1 hiện f4979 sau lưng Simon đi trái tầng dưới (x 257 f4978 → 254 f4980) ⇒ biên 256 (Ghost không có firstDelay ⇒ sinh ngay); Simon quay lại x 262 rồi đi trái tới 132 + leo thang ≈ 480 tick không có Ghost nữa ⇒ 1 con / lần nạp; tầng trên cùng khoảng x (3:00–3:08) không có ⇒ khoá Y — XẤP XỈ |
| TowerHallC | `Ghost_HC_up` | vùng x **[576, 4096]**, **y [0, 64]** (tầng trên), maxSpawns 1 | Ghost #2 hiện f5813 sau lưng Simon đi phải tầng trên (x 572 f5810 → 576 f5812) ⇒ biên 576; lúc vào Hall C Simon ở x 720 → 530 tầng dưới/bậc y 112–176 không có Ghost ⇒ khoá Y — XẤP XỈ |
| TowerHallC | `Mech_HC` | marker `crusher_1..3` ⇒ 3 bẫy ép | CVM2 06 §2i (pha lượt 2 chưa giải — CVM2_MEMORY §5 #8) |
| Medusa | `MedusaHead_MD` | vùng x **[384, 612]** | 6 con Simon 586 → 434 (f6190–6509), không ở f6573 (Simon 334) ⇒ biên trái ∈ (334, 434] — **384**; con đầu f6189.9 ⇒ vào vùng Simon 602–624 ⇒ biên phải **612** (XẤP XỈ); không vào phòng boss (x < 256) |
| Medusa | `Secret_ArchBag` | §3 #5 | chỉ wiki |
| Medusa | `BossArena_QueenMedusa` | prefab QueenMedusa, zone [0, 4096], bossPosition (112, 144) = chân tượng, screenWalls, useConfigPosition false | **chép nguyên `Tools/cvm2/map2_boss.json`** (CVM2 09) — Unity: Warp sảnh ⇒ QueenMedusa (112, 144) ✓ |

**Bệ / bẫy trong luồng section — `MechSpawner`** (mới, `Assets/Data/Script/Object/MechSpawner.cs`): là `EnemySpawner` ⇒ `StageWorld.LoadSection` tự gom + `ResetSpawner`; tick đầu bộ sinh chạy sau khi nạp ⇒ `Map2Mechanisms.SpawnFromMarkers` 1 lần (vị trí marker 05, biên/chu kỳ/pha Config 06). Unity: Warp ⇒ 0 bệ/bẫy, tick đầu ⇒ TEB 1 bệ, Hall B 2 bệ, Hall C 3 bẫy ✓. Bước 8 dừng khi bộ sinh tạm dừng (cửa/đen) / đồng hồ ⇒ tick sinh = tick đầu section chạy — **startTick 06 đo từ frame đầu section hiện ⇒ routine 11 căn** (§6).

**Rơi vật khi enemy chết (phát hiện mới — CVM2 08 ghi "17 lần hạ không thấy vật rơi" là SÓT)**: video Map_2 có **3 lần**: Medusa Head bị roi hạ f2056 (Hall A) ⇒ tim nhỏ rơi f2068 → nhặt f2073 (`view/ha528.png`); Medusa Head hạ f6227 (sảnh Medusa) ⇒ tim nhỏ f6236 → nhặt f6242 (`view/md_heart.png`); **Bone Tower #2 hạ f5653 ⇒ ROSARY** rơi f5662–5674 → nhặt ⇒ nháy màn 3:09.2 (`view/hc_rosary.png` — CVM2 02 "rosary nhặt 3:09.2" không rõ nguồn). Luật rơi (xác suất/loại) không suy được từ 3 mẫu ⇒ **không dựng** (H9) — §6.

## §5 Video ↔ wiki / ghi chép cũ (H1: theo video)
| Mục | Wiki / ghi cũ | Video | Chọn |
|---|---|---|---|
| Dao găm Stage 5 | icon ảnh bản đồ ≈ x 526 | nến **x 464** (f2103) rơi dao; nến 528 rơi túi đỏ | video |
| Shot Multiplier Stage 6 | wiki "Shot Multiplier", ảnh icon II; CVM2 02 "III" | **II** (Double Shot), nhặt +700 khi cầm đồng hồ | video (II) |
| "+700 túi trắng 2:51.60" (CVM2 02) | — | +700 = nhặt II khi cầm đồng hồ | video — sửa `MAP2_TIMELINE` "(CVM2 10)" |
| "rosary 3:09.2" nguồn | — | Bone Tower #2 chết rơi rosary | video (luật rơi enemy chưa dựng) |
| Stop Watch Stage 5 "nến gần Pot Roast" | nến gần | nến **x 527** (Pot Roast ở ô (39,3) x 624) | khớp |
| Nến Block 2 cùng lưới? | — | không: x ≡ 15/16/17 mod 64 tuỳ đoạn | đo từng nến (0 px) |
| Spear Knight / Bat / Bone Tower / Ghost / Medusa Head theo stage | St4 SK + Bat; St5 MH (+SK); St6 BT + Ghost + MH | khớp (St6 có thêm 1 dơi treo Hall C) | video |

## §6 Kiểm Unity
- `Map2ObjectsProbe.Start("candles,blocks,secrets,spawns,mech,boss", shotDir, spec)` (Arena-Simon Play, 6 section Block 2 + lớp Objects dựng lúc chạy): **101/101 ĐẠT** — 51 nến (điểm sinh + vị trí + id ⇒ phá ⇒ item = video), 4 nến kiểm khung hiện (x màn 7 chưa sinh / 8 sinh), 6 khối + 2 lần nhặt II (+700), 3 bí mật (tick kích hoạt, x Simon, 64 tick trồi, vị trí nghỉ, điểm 2000/2000/1000) + 1 kiểm âm (đứng ngoài 2 khối), 12 SpawnPoint enemy (loại/chỗ/hướng, dơi ở Perch), 6 vùng Medusa Head (trong có / ngoài không), Ghost (sinh 1, hạ rồi 240 tick không sinh lại, khoá Y), 3 MechSpawner, BossArena. Báo cáo `_cache/shots/10/unity2/probe_report.txt`.
- **Cặp ảnh cùng cx** (`Tools/cvm2/pairs10.py`, 26 ảnh Unity 1920×1080 ẩn Simon ↔ frame video camera đứng; khớp mẫu cả 2 phía): **89 cặp vật thể, 87 cặp lệch 0–1 px** (nến 0 px ×63 + −1 px ×1 (Hall B f3991 x 273 — video x 272/273 ±1 khi camera cuộn), dơi treo 0 px ×5, Bone Tower 0 px ×14 (đầu trên + đầu dưới), rương 0, vương miện dx +1 (lật sprite đối xứng 135/136), Spear Knight TEA f187 0 / f188 +1 (đã đi 1 frame)); 2 cặp lệch là Spear Knight **đang đi/rơi ở video** (TEB f1265 dx −2: đã đi 21 frame sau lần thấy đầu; TEB f867 dy −4: đang rơi) — Unity chụp ở điểm sinh. 7 tấm ghép `_cache/shots/10/unity2/pairs_0..6.png` (**đã mở xem cả 7**), bảng `pairs.txt`.
