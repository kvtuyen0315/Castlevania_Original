"""CVM2 10 — sinh bảng vật thể Map 2 (Assets/Data/Config/Level/Block2/Map2Objects.json) + các dòng Block 2 của ConfigDrop.csv từ số đo
video Map_2 (props10.py / drops10.py / strip10.py + đối chiếu wiki10.py — ClaudeNotes/cvm2/MAP2_PLACEMENT.md). Bản Map 2 của
Tools/cvgp/gen_map1_objects.py (CVGP 15). JSON là nguồn sự thật cho menu CVM2/Build Map2 Objects (sửa ở đây rồi chạy lại cả hai).
Phòng boss: mục bossArenas đọc NGUYÊN từ Tools/cvm2/map2_boss.json (CVM2 09 — nguồn duy nhất).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/gen_map2_objects.py
ConfigDrop: xoá mọi dòng stage 4–6 cũ rồi ghi dòng Block 2 ở CUỐI file (Block 1 + dòng khác giữ nguyên thứ tự/byte;
gen_map1_objects.py giữ dòng stage ≥ 4 khi chạy lại).
"""
import csv, io, json, os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_JSON = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block2', 'Map2Objects.json')
DROP_CSV = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigDrop.csv')
BOSS_JSON = os.path.join(ROOT, 'Tools', 'cvm2', 'map2_boss.json')

PREF = 'Assets/Data/Prefabs/'
MECH = 'Assets/Data/Prefabs/Level/Block2/Mech/'
VID = ('video:Map_2 khớp mẫu candle_0/1 mọi frame không lặp, camera ref/data/map2_camera.csv (Tools/cvm2/props10.py; x = mốt khung '
       'camera đứng, Y = đáy đo − 1 vì Candle viewOffset +1); wiki: ảnh bản đồ nửa tỉ lệ (Tools/cvm2/wiki10.py) cùng vị trí ±2 px')
STAGE = {'TowerEntranceA': 4, 'TowerEntranceB': 4, 'TowerHallA': 5, 'TowerHallB': 5, 'TowerHallC': 6, 'Medusa': 6}

# (section, tiền tố id, [(x, Y, nội dung, nguồn nội dung)]) — thứ tự theo x tăng; nguồn = frame vỡ (lần thấy cuối + 1), item khớp mẫu
# (Tools/cvm2/drops10.py, err < 0.1 = chắc) + sự kiện HUD (ref/data/map2_events.txt) / dải ảnh _cache/10/view/*.png
CANDLES = [
    ('TowerEntranceA', 'TEA_C', [
        (80, 128, 'Cross', 'video:Map_2 vỡ f215 (0:07.17) → thập tự rơi f225–237 (dải ảnh tea80.png), nhặt f274 0:09.13 (HUD rìu → thập tự); wiki Stage 4 #1 "Boomerang nến đầu tiên"'),
        (144, 32, 'RedMoneyBag', 'video:Map_2 vỡ f715 → túi đỏ f726 (err 0.066), +100 f734 (0:24.47)'),
        (208, 128, 'LargeHeart', 'video:Map_2 vỡ f314 → tim lớn f335 (0.062), +5 f341 (0:11.37)')]),
    ('TowerEntranceB', 'TEB_C', [
        (81, 32, 'RedMoneyBag', 'video:Map_2 vỡ f1426 → túi đỏ f1438 (0.064), +100 f1506 (0:50.20, cạnh bệ)'),
        (145, 32, 'WhiteMoneyBag', 'video:Map_2 vỡ f1357 → túi trắng f1373 (0.092), +700 f1375 (0:45.83)'),
        (208, 32, 'SmallHeart', 'video:Map_2 vỡ f1275 → tim nhỏ f1289 (0.056), +1 f1311'),
        (272, 32, 'SmallHeart', 'video:Map_2 vỡ f1202 → tim nhỏ f1217 (0.049), +1 f1236'),
        (336, 32, 'SmallHeart', 'video:Map_2 vỡ f1032 → tim nhỏ f1073 (0.056) rơi xuống khe — không nhặt'),
        (400, 32, 'SmallHeart', 'video:Map_2 vỡ f1016 → tim nhỏ f1052 (0.054), +1 f1125'),
        (464, 32, 'RedMoneyBag', 'video:Map_2 vỡ f1049 → túi đỏ f1079 (0.065), +100 f1082 (0:36.07)')]),
    ('TowerHallA', 'HA_C', [
        (81, 32, 'SmallHeart', 'video:Map_2 vỡ f2708 → tim nhỏ f2734 (0.062), +1 f2741'),
        (145, 64, 'LargeHeart', 'video:Map_2 vỡ f2656 → tim lớn f2672 (0.073), +5 f2673 (1:29.10)'),
        (209, 32, 'SmallHeart', 'video:Map_2 vỡ f2595 → tim nhỏ f2611 (0.060), +1 f2629'),
        (273, 64, 'LargeHeart', 'video:Map_2 vỡ f2541 → tim lớn f2558 (0.066), +5 f2564 (1:25.47)'),
        (337, 32, 'RedMoneyBag', 'video:Map_2 vỡ f2261 → túi đỏ f2277 (0.073), +100 f2283'),
        (401, 32, 'SmallHeart', 'video:Map_2 vỡ f2178 → tim nhỏ f2220 (0.059), +1 f2237'),
        (464, 32, 'Dagger', 'video:Map_2 vỡ f2103 (1:10.10) → dao găm rơi xuống sàn f2116–2124, nằm tới f2160 — không nhặt (dải ảnh ha464.png); wiki Stage 5 #1 "dao găm trong nến gần giữa tầng dưới" (icon wiki ≈ x 526 — H1: theo video)'),
        (528, 32, 'RedMoneyBag', 'video:Map_2 vỡ f2028 → túi đỏ f2056 (0.066) trên sàn, +100 f2082 (tim +1 f2073 là từ Medusa Head bị hạ f2056 — ha528.png)'),
        (592, 32, 'SmallHeart', 'video:Map_2 vỡ f1961 → tim nhỏ f1981 (0.057), +1 f2012'),
        (656, 32, 'SmallHeart', 'video:Map_2 vỡ f1891 → tim nhỏ f1912 (0.060), +1 f1930'),
        (720, 32, 'SmallHeart', 'video:Map_2 vỡ f1811 → tim nhỏ f1823 (0.059), +1 f1845')]),
    ('TowerHallB', 'HB_C', [
        (80, 128, 'WhiteMoneyBag', 'video:Map_2 vỡ f2859 → túi trắng f2871–2879 rơi xuống hố cạnh chân thang — không nhặt (dải ảnh hb80.png, err 0.094)'),
        (144, 32, 'SmallHeart', 'video:Map_2 vỡ f4157 → tim nhỏ f4197 (0.057), +1 f4208'),
        (208, 120, 'RedMoneyBag', 'video:Map_2 vỡ f3002 → túi đỏ f3014–3022 rơi xuống khe — không nhặt (dải ảnh hb208.png, err 0.062)'),
        (272, 32, 'SmallHeart', 'video:Map_2 vỡ f4060 → tim nhỏ f4072 (0.057), +1 f4095'),
        (335, 120, 'SmallHeart', 'video:Map_2 vỡ f3155 → tim nhỏ f3165 (0.064), +1 f3171'),
        (399, 32, 'SmallHeart', 'video:Map_2 vỡ f3937 → tim nhỏ f3965 (0.058), +1 f3972'),
        (463, 120, 'RedMoneyBag', 'video:Map_2 vỡ f3282 → túi đỏ f3294 (0.060), +100 f3303'),
        (527, 32, 'Stopwatch', 'video:Map_2 vỡ f3806 → đồng hồ f3840 (0.091), nhặt f3877 2:09.23 (HUD thập tự → đồng hồ); wiki Stage 5 #4 "Stop Watch trong nến gần Pot Roast"'),
        (591, 32, 'RedMoneyBag', 'video:Map_2 vỡ f3790 → túi đỏ f3813 (0.065), +100 f3830'),
        (655, 128, 'WhiteMoneyBag', 'video:Map_2 vỡ f3398 → túi trắng f3408 (0.092), +700 f3420 (1:54.00)'),
        (719, 32, 'RedMoneyBag', 'video:Map_2 vỡ f3690 → túi đỏ f3716 (0.059), +100 f3728')]),
    ('TowerHallC', 'HC_C', [
        (81, 96, 'LargeHeart', 'video:Map_2 vỡ f5320 → tim lớn f5348 (0.068), +5 f5349 (2:58.30)'),
        (145, 120, 'SmallHeart', 'video:Map_2 vỡ f5201 → tim nhỏ f5219 (0.049), +1 f5269'),
        (209, 120, 'SmallHeart', 'video:Map_2 vỡ f5058 → tim nhỏ f5068 (0.066), +1 f5083'),
        (273, 128, 'RedMoneyBag', 'video:Map_2 vỡ f4948 → túi đỏ f4959 (0.066), +100 f4965'),
        (337, 128, 'RedMoneyBag', 'video:Map_2 vỡ f4909 → túi đỏ f4919 (0.073), +100 f4922'),
        (401, 128, 'PurpleMoneyBag', 'video:Map_2 vỡ f4837 → túi tím f4847 (0.067), +400 f4853 (2:41.77)'),
        (464, 128, 'HolyWater', 'video:Map_2 vỡ f4871 → nước thánh f4909 (0.074) nằm sàn — không nhặt (HUD giữ đồng hồ); wiki ảnh bản đồ Stage 6 icon nước thánh giữa tầng dưới'),
        (528, 128, 'SmallHeart', 'video:Map_2 vỡ f4690 → tim nhỏ f4719 (0.052), +1 f4723'),
        (592, 128, 'SmallHeart', 'video:Map_2 vỡ f4621 → tim nhỏ f4649 (0.051), +1 f4652'),
        (656, 128, 'SmallHeart', 'video:Map_2 vỡ f4568 → tim nhỏ f4589 (0.058), +1 f4590')]),
    ('Medusa', 'MD_C', [
        (209, 96, 'LargeHeart', 'video:Map_2 vỡ f6671 → tim lớn f6684 (0.072), +5 f6690 (3:43.00)'),
        (273, 88, 'SmallHeart', 'video:Map_2 vỡ f6604 → tim nhỏ f6623 (0.060), +1 f6629'),
        (337, 88, 'SmallHeart', 'video:Map_2 vỡ f6539 → tim nhỏ f6558 (0.050), +1 f6565'),
        (401, 88, 'SmallHeart', 'video:Map_2 vỡ f6467 → tim nhỏ f6493 (0.054), +1 f6526'),
        (464, 88, 'SmallHeart', 'video:Map_2 vỡ f6332 → tim nhỏ f6351 (0.054), +1 f6389'),
        (528, 88, 'Rosary', 'video:Map_2 vỡ f6255 → rosary f6292 (0.068), nhặt ⇒ nháy màn f6304 3:30.13'),
        (592, 88, 'SmallHeart', 'video:Map_2 vỡ f6160 → tim nhỏ f6178 (0.050), +1 f6194 (tim +1 f6242 là từ Medusa Head bị hạ f6227 — md_heart.png)'),
        (656, 88, 'SmallHeart', 'video:Map_2 vỡ f6109 → tim nhỏ f6125 (0.051), +1 f6134'),
        (720, 88, 'SmallHeart', 'video:Map_2 vỡ f6020 → tim nhỏ f6048 (0.056), +1 f6062')]),
]

# khối phá được (ô lưới CVM2 05) — id = BreakableBlock.ObjectId "<Section>_X<c>_<r>"
BLOCKS = [
    ('TowerEntranceA', 14, 5, 'None', 'tường phải bục trên thang đầu — ô trên', 'video:Map_2 0:15.63 roi vỡ (f469), không rơi gì (dải ảnh tea_wall.png); mở khe cho bí mật vương miện'),
    ('TowerEntranceA', 14, 6, 'None', 'tường phải bục trên thang đầu — ô dưới', 'video:Map_2 0:16.10 roi vỡ (f486), không rơi gì (tea_wall.png); wiki Stage 4 #1 "phá các khối tường bên phải, bước vào khe"'),
    ('TowerEntranceB', 22, 5, 'DoubleShot', 'khối góc phải bục (dưới là khe Spear Knight)', 'wiki:Stages 4-6 Stage 4 #2 "phá khối ở góc bục bên phải ⇒ Shot Multiplier" + icon II ảnh bản đồ wiki — video không phá (CVM2 05)'),
    ('TowerHallB', 39, 3, 'PotRoast', 'bậc cao nhất góc trên-phải', 'video:Map_2 1:55.9 roi vỡ (f3474–3478) → thịt nướng rơi xuống bậc dưới f3486 (dải ảnh hb_roast.png); wiki Stage 5 #4 Pot Roast'),
    ('TowerHallC', 44, 7, 'PotRoast', 'khối đầu stage (bậc lối vào, Simon ngồi quất)', 'video:Map_2 2:30.8 roi ngồi vỡ (f4524–4528) → thịt nướng lộ ngay, nhặt khi đầy máu (dải ảnh hc_roast.png); wiki Stage 6 #1'),
    ('TowerHallC', 8, 8, 'DoubleShot', 'khối trái bệ chân thang ngắn', 'video:Map_2 2:50.4 roi vỡ (f5110–5116) → II (Double Shot — bảng màu xanh-kem, KHÔNG phải III như CVM2 02 ghi) rơi xuống sàn, nhặt f5148 khi cầm đồng hồ ⇒ +700 (2:51.60; hc_iii_zoom.png); wiki Stage 6 #3 Shot Multiplier khối chân cầu thang'),
]


def candle(sec, cid, x, y, content, src):
    return dict(id=cid, kind='candle', x=x, y=y, content=content,
                src='%s — x %d, y đáy %d; nội dung: %s' % (VID, x, y, src))


def sp(name, prefab, x, y, facing, src, param=0, respawn=True):
    d = dict(name=name, prefab=PREF + prefab, x=x, y=y, facing=facing, respawn=respawn, src=src)
    if param:
        d['param'] = param
    return d


def edge(name, prefab, side, zmin, zmax, src, rows=None, ymin=None, ymax=None, max_spawns=0):
    d = dict(name=name, prefab=PREF + prefab, side=side, rows=rows or [], playerRelativeY=True, zoneMinX=zmin, zoneMaxX=zmax,
             releaseOnBlock='', src=src)
    if ymin is not None:
        d['zoneMinY'] = ymin
        d['zoneMaxY'] = ymax
    if max_spawns:
        d['maxSpawns'] = max_spawns
    return d


def mech(name, platform, crusher, src):
    return dict(name=name, platformPrefab=MECH + 'MovingPlatform.prefab' if platform else '',
                crusherPrefab=MECH + 'Crusher.prefab' if crusher else '', src=src)


# khung hiện vật thể (nến + SpawnPoint enemy): video:Map_2 51 nến — thấy ở 100 % frame có x màn 8–248 (≈ 700 frame), 0 / 223 frame x màn
# 0–6 và 0 / 29 frame x màn 249–255 (Tools/cvm2/props10.py, MAP2_PLACEMENT §0); Bone Tower (313, 48) chưa hiện ở x màn 7 (f4841), hiện ở 9
# (f4842) ⇒ NES chỉ vẽ/giữ vật trong [cx + 8, cx + 248]. Unity SpawnPoint activate/despawnMargin −8 ⇒ [cx + 8, cx + 248) (phải lệch 1 px)
WINDOW_INSET = 8

MH_SRC = ('video:Map_2 (CVM2 08 §2j, ref/data/enemy8/mh_spawns.csv + map2_simon.csv): ')
RISE = 0.25
RISE_SRC = ('tốc độ trồi 0.25 px/tick = video:Map_2 1 px / 2 frame (vương miện f512–531: 188→178; rương f2399–2406: 86→82) — Block 1 '
            'ConfigRule.SecretRiseSpeed 0.3125 giữ nguyên (H12, §6)')


def main():
    secs = {}
    for sec, pre, rows in CANDLES:
        secs[sec] = dict(id=sec, candles=[candle(sec, '%s%0*d' % (pre, 2 if len(rows) >= 10 else 1, i + 1), x, y, c, s)
                                          for i, (x, y, c, s) in enumerate(rows)],
                         spawnPoints=[], edgeSpawners=[], waterSpawners=[], secrets=[], bossArenas=[], mechSpawners=[])
    S = secs
    # ---- Stage 4
    S['TowerEntranceA']['spawnPoints'] = [
        sp('SpearKnight_TEA', 'Enemy/SpearKnight.prefab', 142, 80, -1,
           'video:Map_2 f187 (0:06.23, section hiện) Spear Knight tầng trên-trái (142, 80) quay TRÁI rồi đi trái 0.5 px/tick (CVM2 08 trk_sk_tea); wiki Stage 4 "chỉ Spear Knight + Bat"'),
        sp('Bat_TEA', 'Enemy/Bat.prefab', 48, 33, -1,
           'video:Map_2 f187–776 dơi treo bat_purple_0 (48, 33) (CVM2 08 §2j; Y = vị trí vẽ, viewOffset 0) — param 1 = treo (Bat.Perch); +200 khi thập tự trúng 0:25.93', param=1)]
    S['TowerEntranceA']['secrets'] = [dict(
        name='Secret_Crown', x=228, y=112, area=[217, 96, 23, 17], condition='Enter', holdTicks=0, item='Crown', spawnAt=[136, 177],
        rise=True, riseSpeed=RISE,
        src='video:Map_2 0:15.6–0:19.8: phá 2 ô tường (14,5)/(14,6) rồi Simon bước vào khe (x 220 f502 → 225 f504 → 232 f508) ⇒ vương '
            'miện trồi dưới cầu thang (khớp mẫu crown_0..3: x 136, đáy đo 178 ⇒ Y 177; lên 1 px/2 frame tới f531 ⇒ bắt đầu ≈ f499–500 ⇒ '
            'kích hoạt khi thân Simon (±8) chồng ô khe = pivot x ≥ 217 — chưa phá thì tường chặn ở x ≤ 216), nhặt f593 +2000; '
            'wiki Stage 4 #1 "step inside, a flashing crown will appear under the stairway"; ' + RISE_SRC)]
    S['TowerEntranceB']['spawnPoints'] = [
        sp('SpearKnight_TEB_gap', 'Enemy/SpearKnight.prefab', 336, 77, 1,
           'video:Map_2 f866 (0:28.87, TEB hiện) Spear Knight lơ lửng (336, 81) đang rơi vào khe x 320–351 (sàn y 144) — điểm đặt (336, 77) '
           'quay PHẢI theo CVM2 08 (Map2EnemyProbe sk_teb, g 0.125 ⇒ đáy đo 81 sau 6 tick); wiki Stage 4 #2 "Spear Knight đứng bên dưới" chỗ khối II'),
        sp('SpearKnight_TEB_door', 'Enemy/SpearKnight.prefab', 145, 80, 1,
           'video:Map_2 f1244 (0:41.47) lần đầu thấy đủ hình ở x màn 10 (cx 135, camera cuộn trái) tại (145, 80) quay PHẢI (trk_sk_teb #2), '
           'chết +400 0:44.37; wiki Stage 4 #3 "Spear Knight trước cửa cuối" — XẤP XỈ (chưa thấy lúc sinh)'),
        sp('Bat_TEB', 'Enemy/Bat.prefab', 304, 33, -1,
           'video:Map_2 f866–1040 dơi treo (304, 33) (CVM2 08 §2j), +200 thập tự 0:34.67', param=1)]
    S['TowerEntranceB']['mechSpawners'] = [mech('Mech_TEB', True, False,
        'CVM2 06 §2i: bệ di động marker platform_lane_TEB (Config Object.MovingPlatform centerX/amplitude/periodTicks/startTick TEB) — video:Map_2 0:46.5–0:51.3')]
    # ---- Stage 5
    S['TowerHallA']['spawnPoints'] = [
        sp('SpearKnight_HA', 'Enemy/SpearKnight.prefab', 211, 80, 1,
           'video:Map_2 f2293 (1:16.43) lần đầu thấy đủ hình x màn 15 (cx 196, cuộn trái) tại (211, 80) quay PHẢI rồi quay trái đi 211→186 (trk_sk_ha) — XẤP XỈ (chưa thấy lúc sinh)')]
    S['TowerHallA']['edgeSpawners'] = [edge('MedusaHead_HA', 'Enemy/MedusaHead.prefab', 'FrontOfPlayer', 550, 4096,
        MH_SRC + 'Hall A sinh 5 con khi Simon x 720 → 581 (đỉnh sóng f1770–2026, cách 128 tick), KHÔNG sinh ở lượt kế f2090 (Simon 518) ⇒ '
        'biên trái ∈ (518, 581] — chọn 550 (giữa, XẤP XỈ); con đầu 17 tick sau khi section hiện (firstDelay Config)')]
    S['TowerHallA']['secrets'] = [dict(
        name='Secret_TreasureChest', x=304, y=48, area=[288, 32, 32, 17], condition='StandStill', holdTicks=120, item='TreasureChest',
        spawnAt=[400, 81], rise=True, riseSpeed=RISE,
        src='video:Map_2 1:16.2–1:21.4: Simon nhảy lên 2 khối nhô (cột 18–19, mặt y 48) f2288–2297, bước cuối f2312 (x 307) rồi ĐỨNG YÊN '
            'x 306 từ f2314; rương trồi ở x 400 (khớp mẫu chest_0..3, đáy đo 82 ⇒ Y 81) lên 1 px/2 frame, tới đỉnh f2406 ⇒ bắt đầu ≈ f2374 '
            '(16 px @ 0.25) ⇒ đứng yên ≈ (2374 − 2313.5) × 2.0033 = 121 tick ⇒ holdTicks 120 (±3); nhặt f2441 +2000 (P 03→04); '
            'wiki Stage 5 #2 "đứng trên 2 khối nhô cao một lúc ⇒ Treasure Box trồi từ các khối bên phải"; ' + RISE_SRC)]
    S['TowerHallB']['spawnPoints'] = [
        sp('SpearKnight_HB_low', 'Enemy/SpearKnight.prefab', 494, 144, -1,
           'video:Map_2 f3193 (1:46.43) lần đầu thấy x màn 245 (cx 249, cuộn phải) tầng dưới (494, 144) quay TRÁI (trk_sk_hb #0), +400 1:47.83 — XẤP XỈ'),
        sp('SpearKnight_HB_up1', 'Enemy/SpearKnight.prefab', 526, 80, -1,
           'video:Map_2 f3209 (1:46.97) x màn 246 tầng trên (526, 80) quay TRÁI (trk_sk_hb #1; CVM2 08 Map2EnemyProbe sk_hb), +400 2:00.50'),
        sp('SpearKnight_HB_up2', 'Enemy/SpearKnight.prefab', 718, 80, -1,
           'video:Map_2 f3380 (1:52.67) x màn 245 tầng trên (718, 80) quay TRÁI (trk_sk_hb #2), +400 1:56.90')]
    S['TowerHallB']['edgeSpawners'] = [edge('MedusaHead_HB', 'Enemy/MedusaHead.prefab', 'FrontOfPlayer', 0, 288,
        MH_SRC + 'Hall B lượt dưới sinh 5 con khi Simon x 94 → 279 (f2829–3084), không sinh f3148 (Simon 352); lượt trên (đi trái) con '
        'đầu f4092.6 lúc Simon 277 ⇒ vào vùng 17–56 tick trước = Simon 281–295; sinh tới Simon 120 ⇒ biên phải ∈ [281, 295] (chung 2 tầng) '
        '— chọn 288 (XẤP XỈ), biên trái 0 (Simon vào từ chân thang x 66)')]
    S['TowerHallB']['mechSpawners'] = [mech('Mech_HB', True, False,
        'CVM2 06 §2i: 2 bệ di động marker platform_lane_HallB_1 / _2 (Config Object.MovingPlatform HallB1/HallB2) — video:Map_2 1:41–1:48, 2:11–2:18')]
    # ---- Stage 6
    S['TowerHallC']['spawnPoints'] = [
        sp('Bat_HC', 'Enemy/Bat.prefab', 145, 57, -1,
           'video:Map_2 f4974–5101 dơi treo (145, 57) rời chỗ f5043 khi Simon nhảy (CVM2 08 §2j)', param=1),
        sp('BoneTower_HC_low', 'Enemy/BoneTower.prefab', 249, 80, -1,
           'video:Map_2 2:41.4–3:03.37 Bone Tower (249, 80) (CVM2 08 §2j, Map2EnemyProbe bt_c; Y = chân, viewOffset +1) — luôn quay về Simon; +400 roi'),
        sp('BoneTower_HC_up', 'Enemy/BoneTower.prefab', 313, 48, -1,
           'video:Map_2 2:41.4–3:08.43 Bone Tower (313, 48) (CVM2 08 §2j); +400 roi (rơi rosary khi chết — §6)')]
    S['TowerHallC']['edgeSpawners'] = [
        edge('Ghost_HC_low', 'Enemy/Ghost.prefab', 'BehindPlayer', 0, 256,
             'video:Map_2 Ghost #1 hiện f4979 (2:45.97) 96 px SAU LƯNG Simon (Simon đi trái tầng dưới y 144, x 257 f4978 → 254 f4980; CVM2 08 §2j) '
             '⇒ biên phải 256 (Config Ghost không có firstDelay ⇒ sinh ngay khi vào vùng); Simon quay lại x 262 f5022 rồi đi trái tới 132 + '
             'leo thang ≈ 480 tick không thấy Ghost nữa ⇒ 1 con / lần nạp section (maxSpawns 1); khoá Y tầng dưới [129, 192] (tầng trên '
             'y 48 cùng khoảng x không sinh — Simon đi qua 3:00–3:08) — XẤP XỈ', ymin=129, ymax=192, max_spawns=1),
        edge('Ghost_HC_up', 'Enemy/Ghost.prefab', 'BehindPlayer', 576, 4096,
             'video:Map_2 Ghost #2 hiện f5813 (3:13.77) sau lưng Simon đi PHẢI tầng trên y 48 (x 572 f5810 → 576 f5812 → 580 f5814) ⇒ biên '
             'trái 576; khoá Y tầng trên [0, 64] (lúc vào Hall C Simon ở x 720 → 530 tầng dưới/bậc y 112–176 KHÔNG thấy Ghost); 1 con '
             '(maxSpawns 1 — như Ghost #1) — XẤP XỈ', ymin=0, ymax=64, max_spawns=1)]
    S['TowerHallC']['mechSpawners'] = [mech('Mech_HC', False, True,
        'CVM2 06 §2i: 3 bẫy ép gai marker crusher_1..3 (Config Object.Crusher maxFrame/startTick 1..3) — video:Map_2 2:33–2:41 (lượt 2 lệch pha: §5 #8)')]
    S['Medusa']['edgeSpawners'] = [edge('MedusaHead_MD', 'Enemy/MedusaHead.prefab', 'FrontOfPlayer', 384, 612,
        MH_SRC + 'sảnh Medusa sinh 6 con khi Simon x 586 → 434 (f6190–6509), không sinh f6573 (Simon 334) ⇒ biên trái ∈ (334, 434] — chọn 384; '
        'con đầu f6189.9 ⇒ vào vùng 17–56 tick trước = Simon 602–624 ⇒ biên phải 612 (XẤP XỈ); không vào phòng boss (x < 256)')]
    S['Medusa']['secrets'] = [dict(
        name='Secret_ArchBag', x=492, y=128, area=[471, 112, 43, 17], condition='StandStill', holdTicks=120, item='FlashingMoneyBag',
        spawnAt=[492, 129], rise=True, riseSpeed=RISE,
        src='wiki:Secrets Bonuses Stage 6 "sảnh có vòm + Medusa Head: đứng yên vài giây ở vòm thứ 4 ⇒ money bag" — CHỈ WIKI (video Simon '
            'không đứng yên ≥ 24 tick ở x 471–513: f6343–6351, f6417–6427). Vòm thứ 4 (đếm từ trái hay phải đều là vòm giữa) = khoảng tối '
            'x 471–513 của sheet (hàng y 100/120); sàn y 128. holdTicks 120 = suy luận theo rương Stage 5 (cùng luật "đứng yên", đo video); '
            'điểm trồi (492, 129) giữa vòm = suy luận (wiki không nói); FlashingMoneyBag 1000 (wiki Secrets: bonus money bag); ' + RISE_SRC)]
    for k in S:
        S[k]['objectWindowInsetPx'] = WINDOW_INSET
    boss = json.load(io.open(BOSS_JSON, encoding='utf-8'))
    for bs in boss['sections']:
        S[bs['id']]['bossArenas'] = bs['bossArenas']
    order = ['TowerEntranceA', 'TowerEntranceB', 'TowerHallA', 'TowerHallB', 'TowerHallC', 'Medusa']
    doc = dict(
        note='CVM2 10 — vật thể Map 2 (Block 2: Stage 04–06) đặt theo video Map_2 (ClaudeNotes/cvm2/MAP2_PLACEMENT.md). Sinh bởi '
             'Tools/cvm2/gen_map2_objects.py; menu CVM2/Build Map2 Objects dựng Assets/Data/Prefabs/Level/Block2/Objects/<Section>_Objects.prefab. '
             'Toạ độ px section (x tâm, y = mặt đáy pivot). Nội dung nến/khối = ConfigDrop.csv theo id. bossArenas chép từ Tools/cvm2/map2_boss.json (CVM2 09). '
             'objectWindowInsetPx 8 = nến/enemy SpawnPoint chỉ hiện ở x màn 8–247 (video:Map_2 8–248).',
        sections=[S[k] for k in order])
    with io.open(OUT_JSON, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write('\n')
    # ConfigDrop: bỏ dòng stage 4–6 cũ, ghi lại ở cuối
    rows = list(csv.reader(io.open(DROP_CSV, encoding='utf-8')))
    head, keep = rows[0], [r for r in rows[1:] if not (4 <= int(r[1]) <= 6)]
    out = []
    for k in order:
        for c in S[k]['candles']:
            out.append([c['id'], str(STAGE[k]), 'WallCandle', c['content'], 'nến %s x %d y %d' % (k, c['x'], c['y']),
                        c['src'].split('nội dung: ')[1]])
    for sec, col, row, item, note, src in BLOCKS:
        out.append(['%s_X%d_%d' % (sec, col, row), str(STAGE[sec]), 'BreakableBlock', item, '%s (ô (%d,%d))' % (note, col, row), src])
    with io.open(DROP_CSV, 'w', encoding='utf-8', newline='') as f:
        w = csv.writer(f, lineterminator='\n')
        w.writerow(head)
        w.writerows(keep)
        w.writerows(out)
    n = sum(len(S[k]['candles']) for k in order)
    print('ghi', OUT_JSON, n, 'nen;', DROP_CSV, len(keep) + len(out), 'dong (Block 2:', len(out), ')')


if __name__ == '__main__':
    main()
