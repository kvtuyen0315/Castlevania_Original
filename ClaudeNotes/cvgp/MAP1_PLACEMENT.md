# MAP1_PLACEMENT — vị trí vật thể + chuyển cảnh Map 1 (Stage 01–03) đo từ video (CVGP 15, 14/10/2026)

> Nguồn sự thật cho dữ liệu: `Assets/Data/Config/Level/Block1/Map1Objects.json` (sinh bởi `Tools/cvgp/gen_map1_objects.py`) → menu **`CVGP/Build Map1 Objects`** → `Assets/Data/Prefabs/Level/Block1/Objects/<Section>_Objects.prefab`; nội dung nến/chậu = `Assets/Data/Config/Csv/ConfigDrop.csv` (theo id) → `CVGP/Import Config`; thời lượng chuyển cảnh = hằng `Map1FlowTiming` (`Assets/Data/Script/Stage/Map1FlowTiming.cs`). Sửa bảng → chạy lại menu (chạy lại không đổi = "ghi 0").
> Toạ độ = px section (gốc trên-trái vùng 192 dòng của sheet Level 1; **dòng r ↔ NES y 32 + r**; x tâm, Y = mặt đáy pivot như `Entity`). Video Map_1: hiệu chuẩn `MAP1_TIMELINE §1` (sx 4.8585, sy 4.1605, ox 8.60, oy −22.55), 1 frame video = 2.003 tick, mốc đơn ±1 frame.

## §0 Cách đo
| Việc | Công cụ (từ gốc repo, `python -P -E`) | Kết quả |
|---|---|---|
| Vị trí nến/chậu | `Tools/cvgp/map1_props.py 3 0.12` — khớp mẫu `Object/candle_0/1`, `torch_0/1` (thước đo `enemy_fit` 13) mọi 3 frame MỚI ở CE f67–590, EH f600–2545, CA f2756–3232 + f4441–4914, CB f3503–4430, PB f5126–6300; x section = cx planner (`work/map1_camera.csv`) + x màn | 112 cụm → **57 vật chứa trên lưới đều**: chậu CE x = 96 + 128k; nến x = 32 + 64k (CB: 32 + 64k và 496); y đáy theo 4 tầng (160/128/64/32 — CB 48). Lệch cụm ±1–4 px = sai số cx planner khi camera đang cuộn (cụm lúc camera đứng: 0 px). **Trùng 57/57 với 57 vật chứa wiki §6** (toạ độ wiki tầng trên = section; tầng dưới Stage 2 = wiki − (16, 160)) |
| Vật bị phá + rơi gì | `Tools/cvgp/map1_drops.py` (khớp mẫu 17 sprite Item quanh nến 8–22 frame sau lần thấy cuối) + đối chiếu sự kiện HUD (`work/map1_events.txt`: tim +1/+5, điểm +100/+400/+700) + dải ảnh tay (`_cache/15/drop_strip.py`, `drops_a..d.png`, `ca_bag.png`, `cb_start.png`, `rosary1.png` — đã xem) | bảng §1 (cột nguồn); 4 ô "KHẢ NĂNG" (vỡ bị che) |
| Simon ở cổng/cửa/đổi tầng/bản đồ/đầu section | `Tools/cvgp/simon_fit.py fit map1 m15 7040 7166 470 600 2540 2760 4900 5130 4370 4480` (+ `m15b` f3200–3240, 3431–3470; `m15c` f60–80, 600–620; `m15d` f4186–4200) | §3 |
| Cánh cửa | `enemy_fit.py map1 Object/door_0,Object/door_1 2600 2700 90 48 180 128` (+ 4970–5070) | §3 |
| Bản đồ lâu đài | `_cache/15/cmap_fit.py` (khớp `castlemap_bg` từng frame — dx, dy) + `enemy_fit.py … CastleMap/castlemap_bat_0..3,castlemap_marker 6990 7163` | §3 |
| Nhạc | `_cache/15/songpos.py` (vị trí bài Vampire Killer / Poison Mind trong âm thanh video theo từng 2 s, tương quan phổ) | §4 |
| So Unity ↔ video | `Map1RouteProbe.Replay(dir, 40)` chụp 27 điểm (camera x = cx frame video) → `Tools/cvgp/map1_pairs.py <dir>` | §6 |

## §1 Vật chứa (57) — nến tường / chậu lửa
SpawnPoint prefab `Candle`/`Torch` (`dataKey` = id ⇒ `CandleProp.objectId` ⇒ ConfigDrop), **phá rồi không sinh lại trong lượt vào section** (`respawn = false`), chỉ ra khỏi màn thì sinh lại (`keepIfOffscreen`); nạp lại section (đổi tầng, hồi sinh) ⇒ nến trở lại — video: CA_C4 vỡ ở lượt 1 (f2968) vẫn thấy ở lượt 2 (f4700, cx 214).

#### CastleEntrance (5 vật chứa)

| id | loại | x tâm | Y đáy | nội dung (ConfigDrop) | video thấy (khớp mẫu, mỗi 3 frame mới) | nguồn nội dung |
|---|---|---|---|---|---|---|
| CE_T1 | chậu lửa | 96 | 160 | LargeHeart | 3 lần f70–92 (x 96-96) | video:Map_1 0:03.2 vỡ → tim lớn, +5 0:03.93; wiki #B1 Large Heart |
| CE_T2 | chậu lửa | 224 | 160 | MorningStar | 16 lần f70–168 (x 224-224) | video:Map_1 0:05.8 → roi, nhặt 0:06.5; wiki #B2 Morning Star |
| CE_T3 | chậu lửa | 352 | 160 | MorningStar | 10 lần f227–269 (x 351-351) | video:Map_1 0:08.9 → roi, nhặt 0:09.8; wiki #B3 Morning Star |
| CE_T4 | chậu lửa | 480 | 160 | LargeHeart | 13 lần f323–367 (x 479-479) | video:Map_1 0:12.1 → tim lớn, +5 0:12.97; wiki #B4 Large Heart |
| CE_T5 | chậu lửa | 608 | 160 | Dagger | 10 lần f397–440 (x 607-607) | video:Map_1 0:14.5 → dao, nhặt 0:15.50; wiki #B5 Dagger |

#### EntranceHall (24 vật chứa)

| id | loại | x tâm | Y đáy | nội dung (ConfigDrop) | video thấy (khớp mẫu, mỗi 3 frame mới) | nguồn nội dung |
|---|---|---|---|---|---|---|
| EH_C01 | nến | 32 | 160 | PurpleMoneyBag | 6 lần f606–631 (x 32-32) | video:Map_1 vỡ f631 → túi tím, +400 f655 (0:21.83); wiki §6.1 nến #1 |
| EH_C02 | nến | 96 | 128 | SmallHeart | 27 lần f606–724 (x 96-96) | video:Map_1 vỡ f724 → tim nhỏ đung đưa, tim 15→16 f762 — wiki ghi Stopwatch (H1: theo video); wiki §6.1 nến #2 |
| EH_C03 | nến | 160 | 160 | SmallHeart | 30 lần f606–761 (x 160-160) | video:Map_1 vỡ f761 → tim nhỏ, 16→17 f788; wiki §6.1 nến #3 |
| EH_C04 | nến | 224 | 128 | RedMoneyBag | 45 lần f606–821 (x 224-226) | video:Map_1 vỡ f821 → túi đỏ, +100 f847 (CVM3 08: túi còn trên sàn tới f883, nhặt **f885** (chữ 100); +100 f847 là Zombie bị roi hạ); wiki §6.1 nến #4 |
| EH_C05 | nến | 288 | 160 | SmallHeart | 23 lần f809–930 (x 288-292) | video:Map_1 vỡ f930 → nhặt ngay, tim 18→19 f950; wiki §6.1 nến #5 |
| EH_C06 | nến | 352 | 128 | RedMoneyBag | 9 lần f901–963 (x 352-354) | video:Map_1 vỡ f979 → túi đỏ, +100 f1001; wiki §6.1 nến #6 |
| EH_C07 | nến | 416 | 160 | SmallHeart | 11 lần f960–1008 (x 415-417) | video:Map_1 vỡ f1008 → tim 19→20 f1071; wiki §6.1 nến #7 |
| EH_C08 | nến | 480 | 128 | SmallHeart | 20 lần f1022–1089 (x 479-482) | video:Map_1 vỡ f1089 → tim 20→21 f1125; wiki §6.1 nến #8 |
| EH_C09 | nến | 544 | 160 | SmallHeart | 15 lần f1077–1166 (x 543-546) | video:Map_1 vỡ f1166 → tim 21→22 f1190; wiki §6.1 nến #9 |
| EH_C10 | nến | 608 | 128 | SmallHeart | 15 lần f1148–1210 (x 607-611) | video:Map_1 vỡ f1210 → tim 22→23 f1248; wiki §6.1 nến #10 |
| EH_C11 | nến | 672 | 64 | SmallHeart | 51 lần f1193–1493 (x 670-674) | video:Map_1 vỡ f1493 → tim 23→24 f1564; wiki §6.1 nến #11 |
| EH_C12 | nến | 736 | 160 | WhiteMoneyBag | 7 lần f1259–1313 (x 737-740) | video:Map_1 vỡ f1319 → túi trắng, +700 f1340 (0:44.67); wiki §6.1 nến #12 |
| EH_C13 | nến | 800 | 32 | HolyWater | 34 lần f1319–1698 (x 797-799) | video:Map_1 vỡ f1698 → nước thánh rơi xuống lối đi (0:57.3, không nhặt) — wiki gán Holy Water cho #14 (H1: theo video); wiki §6.1 nến #13 |
| EH_C14 | nến | 864 | 64 | LargeHeart | 6 lần f1606–1641 (x 863-866) | video:Map_1 vỡ f1641 → tim lớn, +5 f1676 (0:55.87) — wiki ghi Holy Water; wiki §6.1 nến #14 |
| EH_C15 | nến | 928 | 160 | WhiteMoneyBag | 48 lần f1641–1914 (x 925-929) | video:Map_1 vỡ f1914 → túi trắng, +700 f1935 (1:04.50); wiki §6.1 nến #15 |
| EH_C16 | nến | 992 | 64 | SmallHeart | 10 lần f1767–1809 (x 991-994) | video:Map_1 vỡ f1809 → tim 29→30 f1845; wiki §6.1 nến #16 |
| EH_C17 | nến | 1056 | 160 | SmallHeart | 8 lần f1815–1955 (x 1056-1060) | video:Map_1 vỡ f1991 → tim 30→31 f2016; wiki §6.1 nến #17 |
| EH_C18 | nến | 1120 | 160 | SmallHeart | 10 lần f1839–2030 (x 1119-1121) | video:Map_1 vỡ f2030 → tim 31→32 f2058; wiki §6.1 nến #18 |
| EH_C19 | nến | 1184 | 160 | SmallHeart | 14 lần f2019–2069 (x 1183-1185) | video:Map_1 vỡ f2069 → tim nhỏ (dải ảnh), 32→33 f2135; wiki §6.1 nến #19 |
| EH_C20 | nến | 1248 | 160 | SmallHeart | 20 lần f2063–2150 (x 1247-1250) | video:Map_1 vỡ f2150 → tim 33→34 f2178; wiki §6.1 nến #20 |
| EH_C21 | nến | 1312 | 160 | SmallHeart | 15 lần f2139–2190 (x 1311-1314) | video:Map_1 vỡ f2190 → tim 34→35 f2232; wiki §6.1 nến #21 |
| EH_C22 | nến | 1376 | 128 | SmallHeart | 14 lần f2183–2249 (x 1375-1378) | video:Map_1 KHẢ NĂNG: vỡ sau f2268 (bị rosary nháy che), tim 35→36 f2311 — rosary 1:15.03 là Zombie chết rơi ra; wiki §6.1 nến #22 |
| EH_C23 | nến | 1440 | 64 | SmallHeart | 30 lần f2237–2463 (x 1439-1443) | video:Map_1 vỡ f2463 → tim 36→37 f2488 — wiki gán Rosary cho #23; wiki §6.1 nến #23 |
| EH_C24 | nến | 1504 | 32 | Rosary | 11 lần f2459–2505 (x 1503-1503) | video:Map_1 vỡ f2505 (1:23.5) → rosary, nhặt 1:24.73 — wiki: Rosary ở #23 (H1: theo video); wiki §6.1 nến #24 |

#### CryptA (8 vật chứa)

| id | loại | x tâm | Y đáy | nội dung (ConfigDrop) | video thấy (khớp mẫu, mỗi 3 frame mới) | nguồn nội dung |
|---|---|---|---|---|---|---|
| CA_C1 | nến | 32 | 160 | WhiteMoneyBag | 30 lần f2761–3152 (x 30-33) | video:Map_1 vỡ f3152 → +700 f3177 (1:45.90); wiki §6.2 tầng trên #1 |
| CA_C2 | nến | 96 | 64 | LargeHeart | 6 lần f2761–2778 (x 96-96) | video:Map_1 vỡ f2778 → +5 f2827 (1:34.23); wiki §6.2 tầng trên #3 |
| CA_C3 | nến | 160 | 48 | SmallHeart | 24 lần f2761–2856 (x 160-160) | video:Map_1 vỡ f2856 → tim 41→42 f2895; wiki §6.2 tầng trên #5 |
| CA_C4 | nến | 224 | 128 | RedMoneyBag | 62 lần f2761–4748 (x 224-226) | video:Map_1 vỡ f2968 (roi trên thang, dải ảnh _cache/15/ca_bag.png) → túi đỏ, +100 f3004 (1:40.13); wiki §6.2 tầng trên #7 |
| CA_C5 | nến | 304 | 48 | LargeHeart | 76 lần f2933–4691 (x 304-308) | video:Map_1 vỡ f4691 → +5 f4728 (2:37.60); wiki §6.2 tầng trên #9 |
| CA_C6 | nến | 352 | 160 | RedMoneyBag | 7 lần f3025–4467 (x 352-355) | video:Map_1 vỡ f4467 → +100 f4503 (2:30.10); wiki §6.2 tầng trên #11 |
| CA_C7 | nến | 416 | 32 | Rosary | 91 lần f4446–4830 (x 416-416) | video:Map_1 vỡ f4830 → rosary trên lối đi, nhặt 2:42.7 — wiki #13 Rosary (khớp); wiki §6.2 tầng trên #13 |
| CA_C8 | nến | 480 | 64 | RedMoneyBag | 53 lần f4446–4798 (x 480-480) | video:Map_1 vỡ f4798 → túi đỏ, +100 f4859 (2:41.97); wiki §6.2 tầng trên #15 |

#### CryptB (8 vật chứa)

| id | loại | x tâm | Y đáy | nội dung (ConfigDrop) | video thấy (khớp mẫu, mỗi 3 frame mới) | nguồn nội dung |
|---|---|---|---|---|---|---|
| CB_C1 | nến | 32 | 48 | WhiteMoneyBag | 9 lần f3505–3624 (x 32-32) | video:Map_1 KHẢ NĂNG: vỡ sau f3666 (dải ảnh), +700 f3690 (2:03.0); wiki §6.2 tầng dưới #2 (toạ độ wiki − (16,160)) |
| CB_C2 | nến | 96 | 48 | RedMoneyBag | không thấy (vỡ trước đoạn quét) | video:Map_1 vỡ f3485–3500 (ảnh _cache/15/cb_start.png) → túi đỏ, +100 f3502 (1:56.73); wiki §6.2 tầng dưới #4 (toạ độ wiki − (16,160)) |
| CB_C3 | nến | 160 | 48 | RedMoneyBag | 26 lần f3505–3739 (x 160-160) | video:Map_1 vỡ f3739 → +100 f3768; wiki §6.2 tầng dưới #6 (toạ độ wiki − (16,160)) |
| CB_C4 | nến | 224 | 48 | WhiteMoneyBag | 30 lần f3505–3849 (x 223-226) | video:Map_1 vỡ f3849 → +700 f3868 (2:08.93); wiki §6.2 tầng dưới #8 (toạ độ wiki − (16,160)) |
| CB_C5 | nến | 288 | 48 | Stopwatch | 10 lần f3777–3917 (x 288-292) | video:Map_1 vỡ f3917 → đồng hồ (không nhặt) — wiki #10 Stopwatch (khớp); wiki §6.2 tầng dưới #10 (toạ độ wiki − (16,160)) |
| CB_C6 | nến | 352 | 48 | SmallHeart | 4 lần f3881–3910 (x 351-355) | video:Map_1 KHẢ NĂNG: tim 47→48 f4007 (vỡ không bắt được — dải ảnh); wiki §6.2 tầng dưới #12 (toạ độ wiki − (16,160)) |
| CB_C7 | nến | 416 | 48 | SmallHeart | 10 lần f3974–4032 (x 416-419) | video:Map_1 vỡ f4032 → tim 48→49 f4069; wiki §6.2 tầng dưới #14 (toạ độ wiki − (16,160)) |
| CB_C8 | nến | 496 | 48 | LargeHeart | 19 lần f4032–4114 (x 495-497) | video:Map_1 vỡ f4114 → +5 f4155 (2:18.50); wiki §6.2 tầng dưới #16 (toạ độ wiki − (16,160)) |

#### PhantomBat (12 vật chứa)

| id | loại | x tâm | Y đáy | nội dung (ConfigDrop) | video thấy (khớp mẫu, mỗi 3 frame mới) | nguồn nội dung |
|---|---|---|---|---|---|---|
| PB_C01 | nến | 32 | 128 | LargeHeart | 26 lần f5128–5434 (x 30-34) | video:Map_1 vỡ f5434 → +5 f5456 (3:01.87); wiki §6.3 nến #1 |
| PB_C02 | nến | 96 | 64 | SmallHeart | 4 lần f5128–5137 (x 96-96) | video:Map_1 vỡ f5137 → tim 60→61 f5170; wiki §6.3 nến #2 |
| PB_C03 | nến | 160 | 96 | RedMoneyBag | 26 lần f5128–5249 (x 158-162) | video:Map_1 vỡ f5249 → +100 f5274; wiki §6.3 nến #3 |
| PB_C04 | nến | 224 | 64 | InvisibilityPotion | 39 lần f5128–5315 (x 222-226) | video:Map_1 vỡ f5315 (2:57.2) → bình, nhặt 2:58.2 — wiki: bình "giữa cầu thang", ứng viên #3/#4; wiki §6.3 nến #4 |
| PB_C05 | nến | 288 | 160 | SmallHeart | 20 lần f5285–5594 (x 288-290) | video:Map_1 vỡ f5594 → tim 66→67 f5624; wiki §6.3 nến #5 |
| PB_C06 | nến | 352 | 128 | Axe | 11 lần f5586–5663 (x 351-353) | video:Map_1 vỡ f5663 → rìu, nhặt 3:10.67 — wiki #6 Axe (khớp); wiki §6.3 nến #6 |
| PB_C07 | nến | 416 | 160 | SmallHeart | 28 lần f5626–5739 (x 415-418) | video:Map_1 vỡ f5739 → tim 67→68 f5769; wiki §6.3 nến #7 |
| PB_C08 | nến | 480 | 160 | SmallHeart | 9 lần f5731–5782 (x 479-481) | video:Map_1 vỡ f5782 → tim 68→69 f5813; wiki §6.3 nến #8 |
| PB_C09 | nến | 544 | 128 | LargeHeart | 10 lần f5774–5829 (x 543-546) | video:Map_1 vỡ f5829 → +5 f5858 (3:15.27); wiki §6.3 nến #9 |
| PB_C10 | nến | 608 | 160 | SmallHeart | 7 lần f5822–5880 (x 607-609) | video:Map_1 vỡ f5880 → tim 74→75 f5907; wiki §6.3 nến #10 |
| PB_C11 | nến | 672 | 128 | SmallHeart | 20 lần f5868–5943 (x 671-675) | video:Map_1 vỡ f5943 → tim 75→76 f5973; wiki §6.3 nến #11 |
| PB_C12 | nến | 736 | 128 | LargeHeart | 20 lần f5910–6000 (x 735-739) | video:Map_1 vỡ f6000 → +5 f6040 (3:21.33); wiki §6.3 nến #12 |

## §2 Enemy, bộ sinh, bí mật, boss (lớp `<Section>_Objects`)
| Section | Vật | Thông số | Nguồn |
|---|---|---|---|
| EntranceHall | `Panther_1..3` (SpawnPoint Panther, quay trái, hồi sinh kiểu NES) | (699,112) (891,80) (955,112) | CVGP 13 §4b (video 0:42.0 / 0:54.0 báo nằm chờ) |
| EntranceHall | `Ghoul_Hall` EdgeSpawner Zombie | mép PHẢI, sàn y 176, vùng Simon x **[0, 383]** | `trk_z_eh` (13): 12 Zombie sinh khi Simon 42–364 (cx 0–236); Simon 365–730 (f1021–1340, > nghỉ tối đa 288 tick) không sinh ⇒ biên = màn đầu (cx ≤ 255) — XẤP XỈ |
| EntranceHall | `Ghoul_Stairs` EdgeSpawner Zombie | mép phải, y 176, vùng **[1130, 1536]** | nhóm đầu f2062 lúc Simon 1132 (f2060: 1128); Simon 899–1128 (f1900–2060, 320 tick) không sinh; sinh tới hết thang dài (Simon 1352 f2270, sau đó đủ 3 con) |
| CryptA | `Bat_Crypt` EdgeSpawner dơi | trước mặt Simon, y theo chân Simon, cả section | CVGP 13 (video 1:32.0–1:47.4, 2:28.5–2:42.7) |
| CryptA | `Bat_PotRoastWall` | chỉ sinh khi khối `CryptA_X16_10` vỡ | wiki Secrets (video không thấy) |
| CryptB | `Fishman_Water` WaterSpawner | mặt nước y 160, cả section | CVGP 13 (video 1:57.2–2:26.7) |
| CryptB | `Secret_LowBlocks` (SecretTrigger Crouch) | vùng pivot [448,144,32,17] (khối thấp (28–29,10)), túi 1000 TRỒI ở **(352, 96)** | video 2:19.7–2:24.7: ngồi ở x 471 Y 160 (f4196, khớp mẫu jumpduck_0); túi khớp mẫu pivot màn (96,130) @ cx 256 (f4226) — 11 ghi (352,176) là sai y |
| CastleEntrance | `Secret_GateBag` (SecretTrigger Enter) | vùng pivot x [704,768) (sau mép FG vòm cổng), túi 1000 trồi ở (632,160) | CHỈ wiki (Secrets/Stages 1-3 note 1, ảnh wiki túi bên trái cửa x≈632); điều kiện "nhảy VƯỢT cổng" = vào vùng mà không bị cổng kéo (cổng chỉ kéo khi đứng đất trong [678, 704)) — SUY LUẬN |
| PhantomBat | `Ghoul_Corridor` EdgeSpawner | mép phải, gờ y 112 + sàn y 176, vùng **[0, 320]** | `trk_z_pb` (13): sinh khi Simon 49–310 (cx 0–185); không sinh khi Simon > 344 (3:10.67 →) tới phòng boss — XẤP XỈ |
| PhantomBat | `BossArena_PhantomBat` | boss `PhantomBat.prefab` ở ConfigActorParam sleepX/Y (623,41), cả section, tường màn khi camera khoá cx 512 | CVGP 14 §4b "Boss" |
| (5 section) | Khối phá được | tự sinh từ lưới 06 (`BreakableBlock`) — nội dung ConfigDrop `<Section>_X<c>_<r>` | **Sửa lỗi 11**: CSV ghi `EH_X63_7/CA_X16_10/CA_X16_9/CB_X29_6/PB_X43_9` trong khi `BreakableBlock.ObjectId` = `<tên asset section>_X…` ⇒ thịt nướng / II / túi tím không bao giờ rơi ở Map 1 thật ⇒ đổi thành `EntranceHall_X63_7, CryptA_X16_10, CryptA_X16_9, CryptB_X29_6, PhantomBat_X43_9` |
| Stage 1 ngoài trời | (không enemy) | — | wiki §6.1 + video CE f67–599 0 phát hiện (13) |

## §3 Chuyển cảnh (đo khung-khớp-khung — số trong `Map1FlowTiming`)
| Đoạn | Đo video | Unity |
|---|---|---|
| Đầu stage / điểm vào | CE (50,160) f68–80 (CVTI 42,161 góc sprite); EH sau cổng **(48,176)** f600–620; CA đầu Stage 02 **(48,80)** f2756–2760; PB đầu Stage 03 **(48,80)** f5126–5130 — khớp mẫu idle_0 (06 ghi 49 = x_med lệch +1 khi quay phải — giữ nguyên dữ liệu 06, màn chơi dùng StageStarts riêng) | StageRules.stageStarts: St1 = simonStart CE; St2/3 = (48,80) |
| **Cổng lâu đài** (CE→EH) | camera khoá cx 512 f485 (Simon 640, đi 1 px/tick do người chơi); hồi quy gãy khúc f485–546: gãy ở **x 678.1**, sau đó **0.305 px/tick** (rms 0.80 px) với nhịp khung đi bình thường (7 tick) ⇒ tự đi; TIME đứng từ đó (đv kế lẽ ra f513 không giảm); khuất sau FG từ x ~704 (err khớp mẫu tăng f538–546); đen **f591–599** (18 tick); EH f600; TIME giảm lại f631 (= pha mới 62 tick); SFX 25 (khả năng) 0:17.82 ⇐ Simon x 696 | trên đất & x ∈ [678, 704) ⇒ giữ TIME, tự đi 78/256 px/tick tới 730 (= 678 + 0.305 × 171.8 tick gãy→đen), đen 18, EH (48,176), pha TIME mới. Lộ trình: gate tick 969, đen 1140, EH 1158 |
| **Cửa cuối stage** (EH→CA, CA→PB — 2 lần y hệt) | Simon tới x **1516 / 492** (= cột cửa − 12) ⇒ camera cuộn **128 px @ 1.00 px/tick** (hồi quy: 0.999, 1.002 — planner ghi 0.90–0.91 là SAI) ⇒ dừng **152 tick** (152.8 / 151.9): door_0 (16×48, x cột + 8, y 33) ở +2 (+0..4), door_1 (24×48, x cột) + SFX 29 ở **+12**, Simon tự đi 68 px @ 1 px/tick từ **+26** (27 / 26), door_0 ở **+97**, cột cửa trở lại + SFX 29 ở **+106** (104–108), cuộn 2 từ +152 ⇒ 128 px @ 1.00 ⇒ STAGE +1 cùng frame cuộn xong (f2755.5 / f2756.1); Simon vẽ SAU cánh cửa (bị che f2633–2644); cột cửa (tile) biến mất khi cánh cửa hiện (f2621); TIME đứng 6.8 s (408 tick) | StageFlow: giữ TIME, gỡ enemy/đạn, dừng bộ sinh, section đích hiện ở targetOrigin, camera ScrollTo; xoá tile cột cửa [x,32,8,48] khi door_0, trả lại khi đóng; Simon `StartAutoWalk(x+68, noClip)` rồi đứng (AutoWalk giữ khi noClip); cuộn 2 xong ⇒ `EnterStage(N+1)` + nạp section đích (48,80) + điểm sinh ngay. Lộ trình: door 2718 → door_stop +128 → open +12 → closed +106 → stage2 +280 |
| Ô cửa | Simon đứng x 1516 (thân 1508–1523) lấn ô cửa 16×16 (1520–1535) | `TileCollision`: Door KHÔNG đặc (trước: đặc ⇒ Simon chỉ tới 1512, không bao giờ kích hoạt) |
| **Đổi tầng CB→CA** | CB: Simon leo ↖ tới Y logic 32 (f4430–4431, khung walk_1 vẽ thấp 2) ⇒ đen **f4432–4440** (18 tick) ⇒ CA: Y **188** (f4442 thấy 187 sau ~2 tick leo), x theo đường thang (CA f4442 x 383 ±1; Unity 384); TIME pha mới (giảm f4471) | Y ≤ 31 trên thang cắt đỉnh ⇒ đen 18 ⇒ thang đích (`targetStair`) tại Y 188 (`PlaceOnStair`, giữ hướng). Lộ trình: CB (387,31) → CA (384,188) |
| **Đổi tầng CA→CB** | CA: xuống ↖ tới Y 191 (f3233) ⇒ đen (3 frame game) ⇒ **non-game che** ⇒ CB f3431 Simon (67–70, 35) đang xuống | Y ≥ 192 trên thang cắt đáy ⇒ đen 18 ⇒ CB Y **35** — SUY LUẬN đối xứng (31↔188 lệch 157 ⇒ 192↔35). Lộ trình: CA (68,192) → CB (71,35) |
| Boss → tally | (14 + 12) | không đổi |
| **Tally → bản đồ lâu đài** | tally xong f6956 (đen) ⇒ bản đồ bắt đầu trượt từ x 256 ở f6967.5 (hồi quy dx(f): **1.515 px/tick**, rms 1.35) ⇒ **đen 23 tick** | `GoTo(CastleMap, 23)` |
| **Bản đồ lâu đài** | castlemap_bg dy **48**, dx 256 → 0 @ **1.5 px/tick** (dừng f7051.8 = 171 tick); dơi tím (84,115) bat_0↔bat_1 mỗi **8 tick** từ lúc dừng, chấm tím (148,141) cùng pha bat_1; Simon hiện lúc dừng ở **x 16** (f7053 x 18), đi **1.0 px/tick**, khung walk_0,1,2,1 × **7 tick**, Y 192 NES, tới **x 240** (f7163.6) ⇒ đen 27 tick (Stage 04 f7177); HUD: STAGE 04, tim 05, TIME 0000, ENEMY rỗng từ frame đầu (f6974); SFX MapStep (07) mỗi 16 tick từ ~+14; không nhạc | `CastleMapScreen` (tính lại từ localFrame mỗi tick). Lộ trình: tally xong 8003 → (đen 23) CastleMap 8027 → Stage4Stub 8449 (= 171 trượt + 224 đi + 27 đen) |
| **STAGE 04** | f7177: TIME 0400 (giờ Block 2), ENEMY đầy | `Stage4Stub`: `EnterStage(4, true)`, nền đen (chờ cụm Map 2) |

## §4 Nhạc (chưa import — `PlayMusic(null)` = im lặng + log; mốc để gán clip sau)
| Mốc | Bài | Bằng chứng | Gọi ở |
|---|---|---|---|
| Đầu Stage 01 | 03 Vampire Killer (loop) | xcorr 1.2 s đầu bài: **2.205 s** (NCC 0.78) = Stage 01 hiện 0:02.2 | `StageScreen.OnEnter` → `StageFlow.PlayStageMusic` |
| Cổng, cửa 1, Stage 02, đổi tầng, cửa 2, Stage 03 | (vẫn Vampire Killer — KHÔNG phát lại) | `songpos.py`: vị trí bài = (t + 27.62) mod 29.82 s liên tục t 2–106 s; sau đoạn quảng cáo offset 21.36 liên tục t 120–198 s (Stage 03 2:50.87 không đặt lại) | — |
| Boss | 10 Poison Mind | VK còn tới ~198.5 s; Poison Mind 200.0 s (CVGP 14) | `BossBase.StartBossMusic` (14) |
| Boss chết → nhặt cầu | Poison Mind vẫn chạy (songpos liền 200–212 s) ⇒ 12 Stage Clear lúc nhặt cầu 213.05 s (CVGP 12) | | `StageFlow.OnCrystal` |
| Bản đồ lâu đài | im (chỉ SFX MapStep) | CVGP 02 | `CastleMapScreen.OnEnter` → `StopMusic` |
| STAGE 04 | 04 Stalker | (ngoài video) | `Stage4Stub.OnEnter` |
| Mất mạng / hồi sinh | 15 Death → Vampire Killer | (không có trong video — quy ước) | `StageFlow.OnLifeLost` / `OnRespawned` |

## §5 Video ↔ wiki (H1: theo video)
| Mục | Wiki §6 | Video | Chọn |
|---|---|---|---|
| EH nến #2 (x 96) | Stopwatch | tim nhỏ (f724, tim 15→16) | video — đồng hồ 0:29.5 là Zombie chết rơi ra (13) |
| EH nến #13 / #14 (800 / 864) | #14 Holy Water | #13 nước thánh (f1698, 0:57.3), #14 tim lớn (+5 f1676) | video |
| EH nến #23 / #24 (1440 / 1504) | #23 Rosary | #24 rosary (1:23.5), #23 tim nhỏ | video |
| EH rosary 1:15.03 | — | Zombie chết rơi rosary (f2238, dải ảnh `rosary1.png`) — không phải nến | không mô phỏng (luật rơi của enemy chưa có — §5 #27) |
| (CVM3 08) enemy rơi vật Map_1 | — | xem lại MỌI lần hạ (dải ảnh lửa chết `ClaudeNotes/cvm3/_cache/08/drops/z1_*`): **7 lần** — Zombie f762 ⇒ tim nhỏ (+1 f800), Zombie f865 ⇒ đồng hồ, Zombie f1032 ⇒ túi đỏ (+100 f1042), Zombie f2222 ⇒ rosary, dơi f2992 ⇒ tim nhỏ (+1 f3010), Fish Man f3549 ⇒ tim lớn (+5 f3597), dơi f4491 ⇒ tim nhỏ (+1 f4509) | vẫn không mô phỏng — không luật tất định nào giải thích (`CVM3_MEMORY §2h`, §5 #11) |
| Stage 2 tầng dưới | toạ độ ảnh wiki | = wiki − (16, 160) | video |
| PB bình tàng hình | "giữa cầu thang", ứng viên #3/#4 | #4 (x 224) | video |
| Khối II phòng boss | khối chân thang | khối PHẢI (43,9) (11) | — |
| Còn lại | "wiki không nêu" | đọc từ video (tim nhỏ / túi đỏ / túi trắng / tim lớn) | video; 4 ô KHẢ NĂNG (EH_C22, CB_C1, CB_C6 — vỡ bị che; CB_C2 vỡ trước đoạn quét) |

## §6 Lộ trình + kiểm (Map1RouteProbe)
- Replay `Assets/Data/Tests/Replays/map1_route.txt` (**8509 tick**, hash input trong dòng 1): ghi bằng autopilot (phá 5 chậu CE như video, đi sàn EH → thang dài → cửa 1 → CA rơi 2 gờ → thang trái-dưới → CB nhảy 2 khe nước → thang phải → CA thang dài → nhảy lên lối đi → cửa 2 → PB rơi 2 gờ → nhảy lên bệ chân thang (dơi thức ở x 693) → đánh trên sàn bằng roi cấp 3 → nhặt cầu → tally → bản đồ → STAGE 04). **Cheat BẤT TỬ bật suốt lộ trình** (đề bài cho phép) ⇒ Zombie/báo/dơi/Fish Man/boss chạm không mất máu; nước vẫn chết (lộ trình không rơi).
- Mốc (tick lộ trình): gate 969 · đen 1140 · EH 1158 · door 2718 · door_open 2858 · stage2 (CA) 3126 · stair_down 3447 → CB 3465 · stair_up 3992 → CA 4010 · door 4340 · stage3 (PB) 4748 · boss thức 5412 · boss hạ 7075 (+3000) · cầu hiện 7227 (640,104) · nhặt 7246 · tally xong 8003 · bản đồ 8027 · STAGE 04 8449 · kết thúc 8509.
- Tất định: ghi / phát lại 40 tick/vòng (có chụp 27 ảnh) / phát lại 7 tick/vòng ⇒ **hash trạng thái e0b2d54c**, 18 checkpoint (mỗi 500 tick) trùng cả 3; GameObject sau mỗi lần **379 / 379 / 379** (không rò).
- 27 cặp ảnh (`_cache/shots/15/pairs/pair_*.png`, đã mở xem): vật chứa ghép được ở 19 cặp — **lệch 0 px (CE/CA/CB cx đứng) hoặc +1 px (camera đang cuộn: sai số cx planner)**; cửa 1/2 (cánh cửa đúng chỗ, Simon bị che), cổng (Simon x 696 sau mép tháp), dơi ngủ (cùng chỗ trần), bản đồ lâu đài (bản đồ, dơi tím, Simon x 113 trùng video f7100), tally, STAGE 04 (HUD 0400/04 — nền đen vì stub). Khác biệt còn lại: Simon/enemy/vật bị phá theo lộ trình riêng; màu nền (bảng màu video ≠ sheet — chênh trung vị ~20/255 trong lâu đài, ~5 ngoài trời).
