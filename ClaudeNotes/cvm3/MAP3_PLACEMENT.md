# MAP3_PLACEMENT — vị trí vật thể Map 3 (Block 3 = Stage 07–09) đo từ video Map_3 (CVM3 10, 22/10/2026)

> Nguồn sự thật cho dữ liệu: `Assets/Data/Config/Level/Block3/Map3Objects.json` (sinh bởi `python -P -E Tools/cvm3/gen_map3_objects.py`) → menu
> **`CVM3/Build Map3 Objects`** → `Assets/Data/Prefabs/Level/Block3/Objects/<Section>_Objects.prefab` (lớp vật thể RIÊNG — không lồng vào prefab
> section; luồng thật gắn ở routine 11 như Map 1/2 gắn trong `StageScreen`); nội dung nến/khối = `Assets/Data/Config/Csv/ConfigDrop.csv` (id
> `WEA_C*`, `WEB_C*`, `WA_C*`, `WB_C*`, `MM_C*`, `WalkwayEntranceA_X18_8` — stage 7–9, ghi CUỐI file; `MummyMan_X83_9` do `Tools/cvm3/boss9m_config.py`
> CVM3 09) → `CVGP/Import Config`. Phòng boss + vị trí 4 nến phòng: `Tools/cvm3/map3_boss.json` (CVM3 09 — nguồn duy nhất, generator đọc + kiểm trùng).
> Sửa bảng → chạy lại generator + menu (lần 2 = "ghi 0").
> Toạ độ = px section Unity (gốc trên-trái vùng 192 dòng của sheet Level 3; **dòng r ↔ NES y 32 + r**; x tâm, Y = mặt đáy pivot như `Entity`).
> **WalkwayB: x Unity = x sheet/video + 112** (CVM3 05 nới trái 112 px). Video Map_3: hiệu chuẩn CVM3 02 (`Tools/cvm3/nes3.py`; sx 3.862, sy 3.310,
> ox 240.044, oy −21.92), camera từng frame `ref/data/map3_camera.csv` (cx gốc sheet), 1 frame video = 2.0033 tick, 22.7 khung game mới/s (mốc ±2 tick).

## §0 Cách đo
| Việc | Công cụ (từ gốc repo, `python -P -E`) | Kết quả |
|---|---|---|
| Vị trí nến | `Tools/cvm3/fit10.py cd_all Object/candle_0,Object/candle_1 207 7451 0 48 256 224 0.10 Tools/cvm3/candle10_pal_map3.json` (bọc `enemy7_fit` = thước đo `Tools/cvm2/enemy8_fit.py`; bảng màu nến video CVM3 09) ⇒ `Tools/cvm3/candles10.py cluster 0.07` — gom cụm ±3 px theo section, x = **mốt ở khung camera đứng** (cx f−1 = f = f+1) | **37 nến** (WEA 11 · WEB 10 · WalkwayA 5 · WalkwayB 5 · MummyMan 6), mỗi cụm 20–759 lần thấy; x khung đứng = mốt tất cả ở 36/37 (WEB x 224: 10 lần 223 khi cuộn); 0 cụm giả (lớp err 0.07–0.10 không có cụm ≥ 5 lần). Y = đáy đo − 1 (prefab Candle vẽ viewOffset +1 — như Map 1/2). 4 nến phòng Mummy = `map3_boss.json` (0 px) |
| Đối chiếu wiki | ảnh bản đồ nửa tỉ lệ `_cache/wiki_img/Castlevania_Stage_{7,8,9}.png` — ảnh ghép vùng quanh từng nến (`_cache/10/work/wiki_montage.py` ⇒ `_cache/10/view/wiki_candles.png`, **đã xem**): St7 = WEB trên / WEA dưới (ảnh x = x/2), St8 = WalkwayA dưới + WalkwayB trên (ảnh x = 192.5 + xUnity/2 — mảnh ghép tay lệch hằng), St9 = MummyMan (x/2) | **37/37 nến có dấu nến wiki đúng chỗ (±1 px ảnh = ±2 px NES)**, đủ số từng section; icon vật phẩm wiki: tim lớn (WEA 161, WEB 160, WalkwayA 32), thịt nướng (khối WEA x 288–303), rìu (WEB ≈ 607), thập tự/boomerang (WalkwayA ≈ 548), đồng hồ (WalkwayB ≈ 272), dao găm (MummyMan 1311), **nước thánh cạnh nến WEA 672** (video: nến 544 — H1 theo video, §5) |
| Khung hiện vật | `Tools/cvm3/candles10.py window` (histogram x màn của nến chưa vỡ) | thấy **143/147 frame ở x màn 8–15, 0/34 ở 0–7, 7/16 ở 248, 0/224 ở 249–255** ⇒ NES chỉ vẽ/giữ vật trong **[cx + 8, cx + 248]** (= Map_2) ⇒ `objectWindowInsetPx` 8 (SpawnPoint nến + enemy `activate/despawnMargin −8` ⇒ [cx+8, cx+248) — phải lệch 1 px như Block 2); Skeleton/Fleaman/Raven hiện lần đầu ở x màn 9–10 / 246–248 (CVM3 07/08) khớp |
| Vật bị phá + rơi gì | `Tools/cvm3/candles10.py drops` — nến mất khỏi khung còn trong tầm (≥ 3 frame x màn 8–247) = vỡ; khớp mẫu 16 sprite Item quanh nến 45 frame sau (bảng màu sheet × 0.85, err < 0.1 = chắc) + sự kiện HUD `ref/data/map3_events.txt` ⇒ `_cache/10/drops.txt`; dải ảnh `Tools/cvm3/strip3.py` (`_cache/10/view/*.png` — **đã xem**: `web607_axe`, `web735`, `wea_480`, `st7_crouch`) | **36/37 nến bị phá trong video** (WalkwayA 551 không phá — Simon leo thang bên dưới), nội dung đọc được cả 36 (§1) |
| Bí mật | `Tools/cvm3/fit10.py bag7|bag8 Item/moneybag_red,…purple,…white …` (vị trí túi nghỉ) + `Tools/cvm3/rise10.py <section> <x> <y_rest> <y_floor> <f0> <f1> <pal>` (mỗi frame thử mọi độ trồi d, chỉ chấm hàng sprite trên mặt sàn) + Simon `ref/data/map3_simon.csv` + dải ảnh `st7_crouch.png` | §3 |
| Enemy | vết CVM3 07 (`ref/data/enemy7/tracks.csv`), CVM3 08 (`_cache/08/det_rv_all.csv`, `det_bt_all.csv`, `ref/data/enemy8b/mh_spawns.csv`), CVM3 09 (`map3_boss.json`) + Simon từng frame | §4 |
| So Unity ↔ video | `Assets/Data/Script/Debug/Map3ObjectsProbe.cs` (Arena Play: tự chấm + chụp cùng cx) → `Tools/cvm3/pairs10.py select|compare` | §6 |

## §1 Nến (37) — `SpawnPoint` prefab `Candle` (`dataKey` = id ⇒ `CandleProp.objectId` ⇒ ConfigDrop), không sinh lại khi đã phá, sinh lại khi chỉ ra khỏi màn (`keepIfOffscreen`) — như Map 1/2
Nến tường Block 3 đều là `candle`. Nội dung = vật thấy rơi trong video (khớp mẫu + HUD) cho 36 nến; WA_C3 theo wiki. `ItemDrops.Resolve` (tim nhỏ ⇒ roi khi roi < 3 / ⇒ II sau 10 đòn vũ khí phụ) giữ nguyên luật CVGP 11: video Simon roi cấp 3 suốt Map_3; MM_C6 ra **II** nhờ luật này (CVM3 09). Cột "x màn" = khoảng x màn lúc thấy (khung hiện 8–248). Cột "wiki": dấu nến ở ảnh bản đồ nửa tỉ lệ (37/37, ±2 px — §0) + icon vật phẩm nếu có.

#### WalkwayEntranceA (11 nến, stage 7)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp) | x màn | wiki (ảnh bản đồ) | nguồn nội dung |
|---|---|---|---|---|---|---|---|
| WEA_C01 | 97 | 144 | SmallHeart | 37 lần f973–f1012 (camera cuộn — không có khung đứng); đáy đo 145 | 9-89 | nến có (±2 px) | video:Map_3 vỡ f1013 (0:33.77) → tim nhỏ f1026 (err 0.063), +1 f1032 |
| WEA_C02 | 161 | 80 | LargeHeart | 153 lần f834–f1103 (đứng 74/74); đáy đo 81 | 9-161 | nến có + icon tim lớn | video:Map_3 vỡ f1104 (0:36.80) → tim lớn f1116 (0.075), +5 f1120; wiki ảnh bản đồ St7: icon tim lớn góc trái tầng dưới |
| WEA_C03 | 225 | 160 | SmallHeart | 156 lần f714–f942 (đứng 62/62); đáy đo 161 | 9-81 | nến có (±2 px) | video:Map_3 vỡ f943 → tim nhỏ f953 (0.059), +1 f960 |
| WEA_C04 | 289 | 160 | SmallHeart | 74 lần f628–f725 (đứng 38/38); đáy đo 161 | 8-85 | nến có (±2 px) | video:Map_3 vỡ f726 → tim nhỏ f742 (0.064), +1 f745 |
| WEA_C05 | 353 | 160 | SmallHeart | 69 lần f580–f672 (đứng 38/38); đáy đo 161 | 9-76 | nến có (±2 px) | video:Map_3 vỡ f673 → tim nhỏ f690 (0.057), +1 f697 |
| WEA_C06 | 417 | 160 | RedMoneyBag | 75 lần f498–f592 (đứng 41/41); đáy đo 161 | 8-86 | nến có (±2 px) | video:Map_3 vỡ f593 → túi đỏ f606 (0.062), +100 f619 |
| WEA_C07 | 480 | 160 | SmallHeart | 89 lần f392–f508 (đứng 39/39); đáy đo 161 | 10-75 | nến có (±2 px) | video:Map_3 vỡ f509 (0:16.97) → tim nhỏ f525 (0.047), +1 f566 (tim LỚN +5 f555 là vật Fleaman hạ f546 rơi — dải ảnh wea_480.png) |
| WEA_C08 | 544 | 160 | HolyWater | 137 lần f207–f398 (đứng 118/118); đáy đo 161 | 32-74 | nến có (icon nước thánh ở nến 672 — §5) | video:Map_3 vỡ f399 (0:13.30) → nước thánh f412 (0.093) nằm sàn, nhặt f491 0:16.37 (HUD đồng hồ → nước thánh); wiki Stage 7 #1 "Holy Water trong một trong các nến đầu" (icon ảnh bản đồ ≈ x 670 — H1: theo video) |
| WEA_C09 | 608 | 160 | SmallHeart | 101 lần f207–f355 (đứng 100/100); đáy đo 161 | 96-96 | nến có (±2 px) | video:Map_3 vỡ f356 → tim nhỏ f373 (0.057), +1 f377 |
| WEA_C10 | 672 | 160 | RedMoneyBag | 63 lần f207–f304 (đứng 62/62); đáy đo 161 | 160-160 | nến có + icon nước thánh (video: túi đỏ — §5) | video:Map_3 vỡ f305 → túi đỏ f331 (0.060), +100 f333 |
| WEA_C11 | 736 | 144 | WhiteMoneyBag | 22 lần f207–f247 (đứng 21/21); đáy đo 145 | 224-224 | nến có (±2 px) | video:Map_3 vỡ f248 (0:08.27) → túi trắng f278 (0.081), +700 f279 (0:09.30) |

#### WalkwayEntranceB (10 nến, stage 7)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp) | x màn | wiki (ảnh bản đồ) | nguồn nội dung |
|---|---|---|---|---|---|---|---|
| WEB_C01 | 160 | 80 | LargeHeart | 87 lần f1198–f1302 (đứng 80/80); đáy đo 81 | 145-160 | nến có + icon tim lớn | video:Map_3 vỡ f1303 → tim lớn f1334 (0.058) rơi xuống sàn y 160, +5 f1358; wiki ảnh bản đồ St7: icon tim lớn góc trái tầng trên |
| WEB_C02 | 224 | 144 | RedMoneyBag | 145 lần f1198–f1380 (đứng 108/110); đáy đo 145 | 171-224 | nến có (±2 px) | video:Map_3 vỡ f1381 → túi đỏ f1424 (0.061), +100 f1458 |
| WEB_C03 | 288 | 144 | RedMoneyBag | 137 lần f1346–f1531 (đứng 65/65); đáy đo 145 | 169-248 | nến có (±2 px) | video:Map_3 vỡ f1532 → túi đỏ f1555 (0.060), +100 f1563 |
| WEB_C04 | 351 | 144 | SmallHeart | 45 lần f1524–f1583 (đứng 14/14); đáy đo 145 | 170-248 | nến có (±2 px) | video:Map_3 vỡ f1584 → tim nhỏ f1596 (0.063), +1 f1613 |
| WEB_C05 | 423 | 112 | SmallHeart | 156 lần f1576–f1824 (đứng 70/70); đáy đo 113 | 78-243 | nến có (±2 px) | video:Map_3 vỡ f1825 → tim nhỏ f1836 (0.066), +1 f1842 |
| WEB_C06 | 479 | 160 | RedMoneyBag | 216 lần f1614–f1878 (đứng 129/129); đáy đo 161 | 135-248 | nến có (±2 px) | video:Map_3 vỡ f1879 → túi đỏ f1895 (0.056), +100 f1897 |
| WEB_C07 | 543 | 160 | SmallHeart | 215 lần f1662–f1924 (đứng 136/136); đáy đo 161 | 162-247 | nến có (±2 px) | video:Map_3 vỡ f1925 → tim nhỏ f1936 (0.061), +1 f1960 |
| WEB_C08 | 607 | 144 | Axe | 242 lần f1906–f2281 (đứng 109/109); đáy đo 145 | 101-248 | nến có + icon rìu | video:Map_3 vỡ f2282 (1:16.07) → RÌU f2292–2301 (0.060) rơi qua khe x 592–623 xuống đáy section, mất f2308 — không nhặt (dải ảnh web607_axe.png); wiki Stage 7 #3 "Axe ở nến thấp nhất (rơi mất nếu không nhặt ngay)" + icon rìu ảnh bản đồ |
| WEB_C09 | 671 | 144 | RedMoneyBag | 118 lần f2127–f2324 (đứng 58/58); đáy đo 145 | 159-244 | nến có (±2 px) | video:Map_3 vỡ f2325 → túi đỏ f2357 (0.059), +100 f2358 |
| WEB_C10 | 735 | 96 | PurpleMoneyBag | 122 lần f2261–f2420 (đứng 104/104); đáy đo 97 | 223-246 | nến có (±2 px) | video:Map_3 vỡ f2421 (1:20.70) → vật trùng thân Simon (đứng sát cột cửa) nhặt ngay: +400 f2433 + chữ "400" (dải ảnh web735.png) = túi tím (ConfigItem 400; CVM3 08 "nhặt TÚI TÍM ở cửa") |

#### WalkwayA (5 nến, stage 8)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp) | x màn | wiki (ảnh bản đồ) | nguồn nội dung |
|---|---|---|---|---|---|---|---|
| WA_C1 | 32 | 80 | LargeHeart | 21 lần f2647–f2676 (đứng 22/22); đáy đo 81 | 32-32 | nến có + icon tim lớn | video:Map_3 vỡ f2677 → tim lớn f2688 (0.061), +5 f2696; wiki ảnh bản đồ St8: icon tim lớn |
| WA_C2 | 104 | 112 | SmallHeart | 59 lần f2647–f2723 (đứng 59/59); đáy đo 113 | 104-104 | nến có (±2 px) | video:Map_3 vỡ f2724 → tim nhỏ f2738 (0.055), +1 f2754 |
| WA_C3 | 551 | 48 | Cross | 266 lần f2926–f3262 (đứng 79/79); đáy đo 49 | 39-247 | nến có + icon thập tự/boomerang | wiki:Stages 7-9#Stage 8 (1) "Boomerang trong nến DƯỚI cầu thang lên tầng trên" + icon thập tự/boomerang ảnh bản đồ wiki x ≈ 548 (eItemId Cross = Boomerang, như TEA_C1 Block 2) — CHỈ WIKI: video KHÔNG phá (Simon leo thang ↗ ngay dưới nến rồi đổi tầng f3263) |
| WA_C4 | 615 | 112 | RedMoneyBag | 33 lần f2959–f2997 (đứng 5/5); đáy đo 113 | 179-247 | nến có (±2 px) | video:Map_3 vỡ f2998 → túi đỏ f3019 (0.069), +100 f3024 |
| WA_C5 | 679 | 112 | SmallHeart | 56 lần f2990–f3067 (đứng 23/23); đáy đo 113 | 167-243 | nến có (±2 px) | video:Map_3 vỡ f3068 → tim nhỏ f3082 (0.060), +1 f3089 |

#### WalkwayB (5 nến, stage 8)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp) | x màn | wiki (ảnh bản đồ) | nguồn nội dung |
|---|---|---|---|---|---|---|---|
| WB_C1 | 216 | 112 | SmallHeart | 74 lần f3275–f3359 (đứng 6/6); đáy đo 113; x sheet 104 | 137-198 | nến có (±2 px) | video:Map_3 vỡ f3360 → tim nhỏ f3378 (0.056), +1 f3395 |
| WB_C2 | 272 | 80 | Stopwatch | 207 lần f3275–f3559 (đứng 37/37); đáy đo 81; x sheet 160 | 154-248 | nến có + icon đồng hồ | video:Map_3 vỡ f3560 (1:58.67) → đồng hồ f3570 (0.082) — không nhặt (HUD giữ nước thánh); wiki Stage 8 #2 "Stop Watch trong nến bên phải cầu thang" + icon ảnh bản đồ |
| WB_C3 | 399 | 80 | LargeHeart | 57 lần f3604–f3669 (đứng 28/28); đáy đo 81; x sheet 287 | 181-245 | nến có (±2 px) | video:Map_3 vỡ f3670 → tim lớn f3680 (0.058), +5 f3710 |
| WB_C4 | 783 | 80 | SmallHeart | 106 lần f4013–f4144 (đứng 75/75); đáy đo 81; x sheet 671 | 171-248 | nến có (±2 px) | video:Map_3 vỡ f4145 → tim nhỏ f4187 (0.051), +1 f4220 |
| WB_C5 | 847 | 80 | SmallHeart | 139 lần f4128–f4297 (đứng 127/127); đáy đo 81; x sheet 735 | 223-246 | nến có (±2 px) | video:Map_3 vỡ f4298 → tim nhỏ f4310 (0.045), +1 f4335 |

#### MummyMan (6 nến, stage 9)

| id | x | Y | nội dung (ConfigDrop) | video thấy (khớp mẫu mọi frame không lặp) | x màn | wiki (ảnh bản đồ) | nguồn nội dung |
|---|---|---|---|---|---|---|---|
| MM_C1 | 32 | 80 | SmallHeart | 20 lần f4546–f4585 (đứng 20/20); đáy đo 81 | 32-32 | nến có (±2 px) | video:Map_3 vỡ f4586 → tim nhỏ f4601 (0.049), +1 f4606 |
| MM_C2 | 1247 | 80 | SmallHeart | 44 lần f5550–f5606 (đứng 7/7); đáy đo 81 | 155-247 | nến có (±2 px) | video:Map_3 vỡ f5607 → tim nhỏ f5637 (0.043), +1 f5684 |
| MM_C3 | 1311 | 80 | Dagger | 143 lần f5582–f5774 (đứng 59/59); đáy đo 81 | 166-247 | nến có + icon dao găm | video:Map_3 vỡ f5775 (3:12.50) → dao găm rơi xuống đỉnh khối f5785–5835, không nhặt (CVM3 09); wiki Stage 9 "Dagger trong nến phòng boss" |
| MM_C4 | 1375 | 128 | SmallHeart | 88 lần f5686–f5845 (đứng 43/43); đáy đo 129 | 156-247 | nến có (±2 px) | video:Map_3 vỡ f5846 (trước khoá) → tim nhỏ f5867 (0.049), +1 f5880 |
| MM_C5 | 1439 | 128 | SmallHeart | 63 lần f5820–f5901 (đứng 31/31); đáy đo 129 | 176-248 | nến có (±2 px) | video:Map_3 vỡ f5902 (trước khoá) → tim nhỏ f5921 (0.048), +1 f5940 |
| MM_C6 | 1503 | 128 | SmallHeart | 759 lần f5885–f6838 (đứng 747/747); đáy đo 129 | 223-245 | nến có (±2 px) | video:Map_3 vỡ f6839 (sau trận) → II f6857 (0.069), HUD II f6864: tim nhỏ đổi Double Shot theo luật ItemDrops.Resolve (vũ khí phụ trúng ≥ 10 lần — CVM3 09) |

## §2 Khối phá được (2 — ô Breakable lưới CVM3 05, `BreakableBlock` tự sinh; nội dung ConfigDrop `<Section>_X<c>_<r>`)
| ObjectId | Ô (px) | Nội dung | Video | Wiki | Unity |
|---|---|---|---|---|---|
| `WalkwayEntranceA_X18_8` | x 288–303, y 128–143 | PotRoast | roi vỡ 0:26.5–26.8 (ô thành đen f799 — CVM3 05) → thịt nướng khớp mẫu x 296 rơi f795–803 tới đáy đo 177 (= Y 176, sàn), không nhặt (đầy máu) | Stage 7 / Secrets "khối TRÁI bộ khối nhô thứ 2 chứa Pot Roast" + icon ảnh bản đồ | Break ⇒ PotRoast nghỉ (296, 176) sau 17 tick ✓ |
| `MummyMan_X83_9` | x 1328–1343, y 144–159 | PotRoast | Boss_Map_3 7.60 s f228 (CVM3 05/09) | Stage 9 (3) "khối thấp nhất phòng Mummy" | ghi bởi `boss9m_config.py` (CVM3 09) — không đổi; Break ⇒ PotRoast nghỉ (1336, 176) ✓ |
Không khối nào khác bị phá ở 2 video (CVM3 05). Wiki Block 3 không nêu khối II/III.

## §3 Bí mật (3 mục wiki `MAP3_WIKI_DATA §2` + Secrets; St7/St8 ĐO video, St9 CHỈ WIKI)
| # | Section | Điều kiện (SecretTrigger) | Vật · nơi hiện | Video (số đo) | Wiki | Unity (Map3ObjectsProbe) |
|---|---|---|---|---|---|---|
| 1 | WalkwayEntranceB (St7) | **Crouch** vùng pivot **[528, 160, 22, 17]** (X 528–549, Y 160–176) = ô phải nhất (cột 33) của bục THẤP NHẤT (cột 30–33, x 480–543, mặt y 176) + phần đứng chìa mép (đầu dò sàn X±6); holdTicks 0 = ConfigRule.SecretCrouchTicks 4 | **FlashingMoneyBag 1000** trồi trên bục CAO bên trái (cột 26–27, mặt y 128) tại **(424, 129)**, 0.25 px/tick, **quãng 14 px** (`risePx`), `keepOffscreen`, `lifetimeFromRest` | Simon đứng x 537 y 176 f1951–1957 → **ngồi f1958–1972** (jumpduck_0; ảnh `st7_crouch.png`) → hàng túi đầu tiên hiện **f1961** (d = 13 px dưới chỗ nghỉ) → 1 px / 2 frame (d 12 f1963 … 0 f1987: 12 px / 24 frame = 0.2496 px/tick) → nghỉ **f1987** (đáy đo 130 ⇒ Y 129 = sàn + 1); Simon đi trái, nhảy lên bục nhặt **f2054 +1000** (1:08.47) = ≈ 197 tick sau khi sinh / 134 sau khi nghỉ | Stage 7 (3) "ngồi ở mép phải bục thấp nhất ⇒ túi hiện trên bục cao bên trái — chạy tới nhặt trước khi nó biến mất"; Secrets "bục cụt, ngồi ở tận cùng" | ngồi x 537 ⇒ túi (424, 129) ✓; ngồi tick 1 → kích hoạt +3 → hàng đầu hiện **+4** (video +6 ±2) → nghỉ **+59** (video 59) ✓; không nhặt ⇒ mất ở tuổi 244 = trồi 56 + **188 từ lúc nghỉ** (còn ở ≥ 197 tick ✓); nhặt ⇒ **+1000** ✓; ngồi x 549 (sát mép) ⇒ túi ở x màn 3 (ngoài khung 8) vẫn còn ✓; ngồi x 520 / đứng yên 537 ⇒ không ✓ |
| 2 | WalkwayB (St8) | **Crouch** vùng **[240, 112, 32, 17]** (X 240–271, Y 112–128) = trên mặt cầu NGAY TRÊN 2 khối lơ lửng (Unity cột 15–16, y 160; sheet 128–159); holdTicks 0 (= 4) | **FlashingMoneyBag 1000** tại **mút trái cầu (128, 129)** (sheet x 16), cùng trồi/tuổi thọ St7, `keepOffscreen` | Simon NGỒI **f3411–3423** ở x 134 sheet (Unity 246) — CVM3 02 ghi "không ngồi" là SÓT (khớp mẫu nhầm `hurt_0` err 0.20, y thấp 5 px; PLAYER không giảm) → túi khớp mẫu x 16 sheet, d 11 f3417 … 0 **f3440** (11 px / 23 frame), hàng đầu ≈ f3413 → Simon đi trái nhặt **f3478 +1000** (1:55.93; CVM3 02 "mút trái cầu") | Stage 8 (2) "quỳ ngay trên 2 khối không với tới bên phải cầu thang ⇒ túi bên trái"; Secrets "dưới nến Stop Watch — có thể hiện NGOÀI màn, đi lùi lại sau 1–2 s" | ngồi x 246 ⇒ túi (128, 129) ✓, +4 / +59 ✓; ngồi x 271 ⇒ túi ở x màn **−15** (ngoài màn) vẫn còn, đi trái 128 tick nhặt **+1000** ✓; ngồi x 300 ⇒ không ✓ |
| 3 | MummyMan (St9) | **WalkLeft** (MỚI — giữ ←, trạng thái Walk quay trái, kể cả bị tường chặn) vùng **[1306, 112, 28, 17]** = trên khối (82,8) x 1312–1327 mặt y 128 (ngay trên-trái khối thịt nướng (83,9)), ≥ **120 tick** (SUY LUẬN — "vài giây" như rương Stage 5 đo Map_2 121), **chỉ khi camera chưa khoá** (`beforeBossLock`) | **FlashingMoneyBag 1000** tại **(1264, 97)** — SUY LUẬN (wiki không nói chỗ; đặt trên gờ (78–79, 6) trái-trên sau tường, như St7/St8 túi hiện bên TRÁI) | **không có trong video** (Simon không làm) | Secrets Stage 9 "trong phòng boss, TRƯỚC khi khoá màn: đứng trên khối nằm trên khối chứa gà tây và đi sang trái vài giây ⇒ túi tiền hiện nhưng thường không với tới" | đi trái trên khối 140 tick ⇒ túi sau **120** tick, Simon kẹt tường x 1320 ✓; túi nghỉ (1264, 97) ✓; camera đã khoá (cx 1280) ⇒ không ✓; đứng yên ⇒ không ✓ — **CHỈ WIKI** |

**Trồi Block 3 = 0.25 px/tick, quãng 14 px** (`SecretTriggerSpec.riseSpeed/risePx` — 2 bí mật đo video; St9 dùng cùng số). Với `ConfigRule.SecretCrouchTicks` 4 (Map_1) chỉ quãng 14 khớp "hàng đầu hiện ~6 tick sau khung ngồi đầu" (quãng 16 ⇒ 15 tick); túi nghỉ ở mặt sàn + 1 (như vương miện/rương Block 2). Block 1/2 giữ quãng ConfigRule 16 (không đổi — H12).
**Tuổi thọ túi bí mật Block 3 tính từ lúc trồi xong** (`lifetimeFromRest`, 188 = ConfigItem): video St7 nhặt ≈ 197 tick sau khi sinh (tuổi tính từ lúc sinh 188 sẽ làm túi biến mất trước khi Simon chạm); Map_1 CB cũng nhặt 176 tick sau khi nghỉ (≈ 230 sau khi sinh — §6: Block 1 Unity hiện tính từ lúc sinh). Thời lượng thật khi KHÔNG nhặt: chưa đo được (video luôn nhặt kịp — "ít nhất 134 tick sau khi nghỉ").
**Túi ngoài màn**: `keepOffscreen` — item không bị gỡ khi ra khỏi khung (wiki St8); không đổi item thường.

## §4 Enemy, bộ sinh, boss (lớp `<Section>_Objects`)
SpawnPoint enemy: `respawn = true` (như Map 1/2 — sinh lại khi điểm sinh ra khỏi khung rồi vào lại), khung hiện **[cx + 8, cx + 248)** (§0), `facing 0` (= quay về Simon lúc sinh — video: Skeleton/Fleaman/Raven/tháp luôn nhìn Simon). Toạ độ = chỗ video thấy LẦN ĐẦU ở mép khung (Skeleton/Fleaman x màn 9–10 / 246–247 — CVM3 07; Raven nguyên hình x màn 246–248 — CVM3 08) ⇒ = điểm sinh (±1–2 px do camera cuộn 2 px/frame); Raven Y = tâm sprite (như CVM3 08 probe).
| Section | Vật | Thông số | Nguồn |
|---|---|---|---|
| WalkwayEntranceA | `Fleaman_WEA_1` | (433, 128) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f491 (0:16.37) Fleaman F0 thấy lần đầu ở x màn 9 (cx 424, camera cuộn trái) ngồi trên bục (433, 128); +500 f545 |
| WalkwayEntranceA | `Fleaman_WEA_2` | (305, 128) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f621 F1 x màn 10 (cx 295) trên bục (305, 128); +500 f675 |
| WalkwayEntranceA | `Fleaman_WEA_3` | (177, 96) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f825 F2 x màn 9 (cx 168) trên bục (177, 96); +500 f918 |
| WalkwayEntranceA | `Skeleton_WEA` | (33, 128) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f1006 (0:33.53) Skeleton S1 hiện ở x màn 10 (cx 23) trên bờ (33, 128) góc dưới-trái rồi đi ngay; +300 f1078; wiki Stage 7 #2 "Skeleton ở góc dưới-trái" |
| WalkwayEntranceA | `Ghost_WEA` EdgeSpawner BehindPlayer | vùng Simon x [0, 576], maxSpawns 1, extraAboveFeetPx 31 | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Ghost hiện f416 (0:13.87) ở (671, 145) = 96 px SAU LƯNG Simon đi trái (x 577 f415 → 575 f416; Config Ghost không có firstDelay ⇒ sinh ngay khi vào vùng) ⇒ biên phải 576; Simon đi tiếp tới x 94 không có Ghost nữa ⇒ 1 con / lần nạp (maxSpawns 1); độ cao lúc hiện = chân Simon (176) − 31 (Map_2 Hall C = chân — CVM3 08 §6) ⇒ extraAboveFeetPx 31 riêng bộ sinh này (1 mẫu; giả thuyết khác khớp 3/3: tâm = min(chân, y 144)) — XẤP XỈ biên (không khoá Y: video chỉ có Simon trên sàn y 176) |
| WalkwayEntranceB | `Raven_WEB_1` | (280, 41) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #0 đậu (280, 41) hiện f1340 ở x màn 246–248 (tâm sprite); bay ra màn f1490 (không hạ) |
| WalkwayEntranceB | `Skeleton_WEB_1` | (352, 160) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f1517 (0:50.57) Skeleton S2 hiện ở x màn 246 (cx 105) trên bục (352, 160); +300 f1553 |
| WalkwayEntranceB | `Raven_WEB_2` | (423, 89) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #1 đậu (423, 89) hiện f1574 cùng lúc nến (423, 112) ở x màn 247; +200 |
| WalkwayEntranceB | `Raven_WEB_3` | (487, 57) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #2 đậu (487, 57) hiện f1634; +200 |
| WalkwayEntranceB | `Raven_WEB_4` | (527, 169) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #3 đậu (527, 169) hiện f1654; +200 f1732 (rơi tim nhỏ — luật rơi chưa dựng) |
| WalkwayEntranceB | `Skeleton_WEB_2` | (671, 160) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f2123 (1:10.77) Skeleton S3 hiện ở x màn 246 (cx 425) trên bục (671, 160); bật mép 608 rơi hố 544–607 (wiki Stage 7 #3 "gờ quá cao — nó rơi xuống hố") |
| WalkwayA | `Skeleton_WA` | (671, 64) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f2986 (1:39.53) Skeleton S4 hiện ở x màn 247 (cx 424) trên khối (671, 64); bật mép 640 rơi xuống cầu; +300 f3073 (rơi tim nhỏ); wiki Stage 8 "Skeleton tầng dưới" |
| WalkwayA | `MedusaHead_WA` EdgeSpawner FrontOfPlayer | vùng Simon x [144, 4096] | CVM3 08 §2h (ref/data/enemy8b/): ref/data/enemy8b/mh_spawns.csv: con đầu (đỉnh sóng) f2790.6 khi Simon x 183 (đi phải); firstDelay Config 17–56 tick ⇒ vào vùng f2762.6–2782.1 = Simon x 125–164 ⇒ biên trái 144 (giữa — XẤP XỈ); sinh tới f3230 (Simon trên thang ↗ x 639) ⇒ biên phải = hết section; 7 con, khoảng 128 (4 khoảng lệch — CVM3 08 §6); wiki Stage 8 "Medusa Head tầng dưới" |
| WalkwayB | `Raven_WB_1` | (383, 57) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #4 đậu (271, 57) sheet (Unity 383) hiện f3594; +200 f3644 |
| WalkwayB | `BoneTower_WB_1` | (479, 128) | CVM3 08 §2h (ref/data/enemy8b/): x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); video:Map_3 Bone Tower trên cầu (367, 128) sheet (Unity 479) f3695–3835, +400 f3837; wiki Stage 8 "Bone Tower tầng trên" |
| WalkwayB | `Raven_WB_2` | (599, 121) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #5 đậu (487, 121) sheet (Unity 599) hiện f3865; +200 f3917 |
| WalkwayB | `Raven_WB_3` | (663, 121) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #6 đậu (551, 121) sheet (Unity 663) hiện f3898; +200 f3957 |
| WalkwayB | `BoneTower_WB_2` | (735, 128) | CVM3 08 §2h (ref/data/enemy8b/): x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); video:Map_3 Bone Tower trên cầu (623, 128) sheet (Unity 735) f3991–4094, +400 f4096 |
| MummyMan | `Raven_MM_1` | (268, 41) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #7 đậu (268, 41) hiện f4668; +200 f4700; wiki Stage 9 #1 "Raven cất cánh khi lại gần" |
| MummyMan | `Raven_MM_2` | (363, 41) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #8 đậu (363, 41) hiện f4715; +200 f4745 |
| MummyMan | `BoneTower_MM_1` | (415, 96) | CVM3 08 §2h (ref/data/enemy8b/): x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); video:Map_3 Bone Tower trên cầu (415, 96) f4741–4851, +400 f4854 |
| MummyMan | `Raven_MM_3` | (495, 33) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #9 đậu (495, 33) hiện f4872; +200 f4904 |
| MummyMan | `Raven_MM_4` | (559, 33) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #10 đậu (559, 33) hiện f4904; +200 f4933 |
| MummyMan | `BoneTower_MM_2` | (671, 96) | CVM3 08 §2h (ref/data/enemy8b/): x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); video:Map_3 Bone Tower trên cầu (671, 96) f4959–5075, +400 f5077 (rơi tim lớn — luật rơi chưa dựng) |
| MummyMan | `Raven_MM_5` | (747, 41) | CVM3 08 §2h (ref/data/enemy8b/): video:Map_3 Raven #11 đậu (747, 41) hiện f5086; +200 f5123 |
| MummyMan | `Skeleton_MM_1` | (815, 96) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f5121 (2:50.70) Skeleton S5 hiện ở x màn 247 (cx 568) trên bục cầu hỏng (815, 96), ném xương cao rồi nhảy khe 768–799; +300 f5151; wiki Stage 9 #2 "Skeleton nhảy qua cầu hỏng" |
| MummyMan | `Skeleton_MM_2` | (879, 96) | CVM3 07 §2g (ref/data/enemy7/tracks.csv): video:Map_3 f5184 Skeleton S6 hiện ở x màn 247 (cx 632) trên bục (879, 96), nhảy khe 832–863 đáp bục 800–831 |
| MummyMan | `BoneTower_MM_3` | (1183, 96) | CVM3 08 §2h (ref/data/enemy8b/): x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); video:Map_3 Bone Tower (1183, 96) f5394–5509, +400 f5511 (+100 cầu lửa cùng nhát) |
| MummyMan | `MedusaHead_MM` EdgeSpawner FrontOfPlayer | vùng Simon x [976, 1259] | CVM3 08 §2h (ref/data/enemy8b/): ref/data/enemy8b/mh_spawns.csv: con đầu f5370.7 khi Simon x 1018 (đi phải) ⇒ vào vùng 17–56 tick trước = f5342.7–5362.2 = Simon x 959–998 ⇒ biên trái 976 (XẤP XỈ); con cuối f5716.9 Simon 1245, lượt kế (≈ f5781) KHÔNG sinh khi Simon 1273 (chỉ 1 con sống — không vướng maxAlive) ⇒ biên phải ∈ [1245, 1272] ⇒ 1259 (XẤP XỈ); 7 con; cầu hỏng St9 (x 768–927) KHÔNG có Medusa Head trong video (wiki có — H1 theo video); phòng Mummy không có |
| MummyMan | `BossArena_MummyMen` BossArena | MummyMan.prefab (1503, 176), zone [0, 4096], screenWalls | CVM3 09: con ĐẦU (phải) sinh ở (1503, 176), con 2 ở 1503 − partnerDxPx 191 = 1312 — cả hai đứng chờ khung 0 quay trái (video Map_3 f5583–5990 thấy từ cx ~1100, Boss_Map_3 f37–433); sinh ngay khi vào section (đứng yên, không va chạm tới khi thức). Camera khoá cx 1280 khi Simon x ≥ 1408 (CVM3 05 bossLockCx; video Map_3 3:17.70 f5931) ⇒ tường mép màn; nhạc boss khoá + 60, thức khoá + 120 (ConfigActorParam Boss.MummyMan) |
**Bone Tower x = sprite video** (415 / 671 / 1183 MummyMan; 479 / 735 WalkwayB = sheet 367 / 623 + 112): CVM3 08 §2h ghi x − 3 (412 / 668 / 1180 / 364 / 620 — khớp gốc cầu lửa ngoại suy, không phải sprite); cặp ảnh với x − 3 lệch đều **−3 px** cả 5 tháp (đầu trên + dưới) ⇒ đặt theo sprite như Block 2 (CVM2 10 tháp = sprite, 0 px). Hệ quả: gốc cầu lửa Unity (fireballFrontPx 0 — dùng chung Block 2) nằm ở x tháp, video Map_3 ngoại suy ≈ x − 3 ⇒ §6 (routine 12 so).
**Vật rơi xuống hố / khe**: `ItemPickup` tự gỡ khi Y − 16 > đáy section (có sẵn CVGP 11). Video Map_3 cho thấy item dò sàn ở **cột tâm X** (rìu WEB x 607 rơi xuyên hàng y 160 dù ô (38,10) x 608–623 đặc; tim lớn WEB_C01 x 160 và túi WEB_C09 x 671 rơi tới y 160 thay vì đáp trên gờ y 96 / 145 như dò cả thân 16 px) ⇒ **`ConfigRule.ItemFootCenterFromBlock` = 3** (`ItemPickup.FootSpan` = [X, X] cho item rơi thường ở block ≥ 3; tim nhỏ đung đưa giữ hộp 8 px — chưa có mẫu). Block 1/2 giữ cả thân (H12/H14) dù video Map_1 CryptB nến x 288 (f3917) đồng hồ cũng rơi xuyên khe xuống nước (Unity Block 1 đáp gờ y 96; CB_C4 x 224 cùng cấu hình) — §6 đề xuất `cvm3-fix`.
**Không đặt** (video Map_3 không có): Fleaman ngoài WEA, Raven ở WEA/WalkwayA, Medusa Head ở cầu hỏng St9 (wiki có — H1 theo video), Ghost ngoài WEA, Skeleton ở WalkwayB. **Enemy rơi vật**: không dựng (CVM3 08 §5 #11) — thêm 1 mẫu CVM3 08 sót: Fleaman F0 hạ f546 ⇒ **tim LỚN** rơi tại chỗ (f554, nhặt +5 f555 — dải ảnh `wea_480.png`) ⇒ bảng rơi 14/80 (§6).

## §5 Video ↔ wiki / ghi chép cũ (H1: theo video)
| Mục | Wiki / ghi cũ | Video | Chọn |
|---|---|---|---|
| Holy Water St7 | "một trong các nến đầu"; icon ảnh bản đồ cạnh nến WEA x 672 | nến **x 544** (f399) rơi nước thánh, nhặt f491; nến 672 rơi túi đỏ (f331) | video |
| Axe St7 "nến thấp nhất, rơi mất" | icon rìu ≈ WEB x 607 | nến **WEB x 607** (y 144 — không phải nến thấp nhất về toạ độ; thấp nhất là 479/543 y 160) rơi rìu xuyên khe xuống đáy, mất | video (vị trí) + wiki (hành vi) khớp |
| Boomerang St8 | nến dưới cầu thang lên tầng trên; icon ≈ x 548 | nến **WalkwayA x 551 y 48** không bị phá | wiki (CHỈ WIKI nội dung) |
| Stop Watch St8 | nến bên phải cầu thang | nến **WalkwayB x 272** (sheet 160) rơi đồng hồ f3570 | khớp |
| Dagger St9 | nến phòng boss | nến **1311** rơi dao găm (CVM3 09) | khớp |
| Bí mật St8 | quỳ trên 2 khối ⇒ túi bên trái | ngồi f3411 ⇒ túi mút trái cầu f3440, nhặt f3478 (CVM3 02 "không ngồi" — sót) | khớp — sửa `MAP3_TIMELINE` "(CVM3 10)" |
| "+400 f2433" | CVM3 02: Bone Tower? | túi tím từ nến WEB 735 (CVM3 08 đã đính chính) | video |
| Bone Tower x | CVM3 08: 412 / 668 / 1180 / 364 / 620 | sprite 415 / 671 / 1183 / 367 / 623 | video sprite (§4) |
| Ghost St7 | wiki St7 có Ghost | 1 con WEA, chân − 31 | video |
| Medusa Head St9 | wiki St9 có (cả cầu hỏng) | chỉ vùng Simon x 976–1259 (7 con) | video |

## §6 Kiểm Unity
- `Map3ObjectsProbe.Start("candles,blocks,secrets,spawns,zones,boss", shotDir, spec)` (Arena-Simon Play, 5 section Block 3 + lớp Objects dựng lúc chạy): **98/98 ĐẠT** — 37 nến (điểm sinh + vị trí + id ⇒ phá ⇒ item = video/wiki) + 3 kiểm khung hiện (x màn 7 chưa sinh / 8 sinh), rìu WEB_C08 rơi xuống hố **mất sau 24 tick** (Offscreen, Y 207 > 192; video f2292–2303 ≈ 22 tick), tim lớn WEB_C01 + túi WEB_C09 rơi tới y 160 (video), 2 khối ⇒ thịt nướng nghỉ đúng sàn, 3 bí mật (17 kiểm: chỗ ngồi/đi, nhịp kích hoạt/trồi/nghỉ, vị trí, tuổi thọ từ lúc nghỉ, nhặt +1000, ngoài màn vẫn còn, kiểm âm), 26 SpawnPoint enemy (loại/chỗ/hướng; Raven Perch, Fleaman Wait), Ghost WEA (sau lưng +96, Y 145 = chân − 31, 1 con không sinh lại, ngoài vùng không), Medusa Head WalkwayA / MummyMan (trong vùng có, 2 phía ngoài không), BossArena ⇒ 2 Mummy (1312 / 1503, 176). Báo cáo `_cache/shots/10/unity3/probe_report.txt`.
- **Cặp ảnh cùng cx** (`Tools/cvm3/pairs10.py`, 46 ảnh Unity 1920×1080 ẩn Simon ↔ frame video; khớp mẫu cả 2 phía): **94 cặp vật thể — 94/94 lệch 0–1 px** (|dx| ≤ 1, |dy| 0): nến 0 px (1 cặp +1 — WEB x 223/224 lúc cuộn), Raven đậu ×12, Bone Tower 10 (5 tháp × 2 đầu), Skeleton ×6 (1 cặp +1: S2 x 351/352 frame đầu), Fleaman ×6, túi St7 / St8 nghỉ 0 px. Lần chụp đầu (tháp theo số CVM3 08) lệch −3 px cả 5 tháp ⇒ sửa (§4). 12 tấm ghép `_cache/shots/10/unity3/pairs_0..11.png` — **đã mở xem 5 tấm** (pairs_0 túi St7/St8 + MummyMan, _1 Raven/tháp MummyMan, _5 WalkwayB, _7 WEA Skeleton/Fleaman/nến, _10 WEB Raven/Skeleton/nến), bảng `pairs.txt`; bản lưu `ref/nes/cvm3_10_pairs_*.png` + video `ref/nes/cvm3_10_video_{riu_roi_ho_web607,ngoi_st7_tui_troi}.png` (ảnh ghép wiki chỉ ở `_cache` — bản quyền).
- Builder: `CVM3/Build Map3 Objects` 70 vật / 5 section — lần đầu ghi 5, lần 2 + menu thật "ghi 0"; `CVGP/Build Map1 Objects` (69 vật) + `CVM2/Build Map2 Objects` (75 vật) "ghi 0"; md5 1 562 file `Assets/Data/{Prefabs,Config,Art}` + `Assets/Scenes` ngoài Block3 trước/sau builder y hệt; `gen_map1/2_objects.py` + `boss9m_config.py` chạy lại sau `gen_map3` ⇒ ConfigDrop byte y hệt.
