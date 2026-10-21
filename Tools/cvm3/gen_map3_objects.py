"""CVM3 10 — sinh bảng vật thể Map 3 (Assets/Data/Config/Level/Block3/Map3Objects.json) + các dòng Block 3 của ConfigDrop.csv từ số đo
video Map_3 (Tools/cvm3/fit10.py + candles10.py + rise10.py + dải ảnh strip3.py; đối chiếu ảnh bản đồ wiki — ClaudeNotes/cvm3/MAP3_PLACEMENT.md).
Bản Map 3 của Tools/cvm2/gen_map2_objects.py (CVM2 10). JSON là nguồn sự thật cho menu CVM3/Build Map3 Objects (sửa ở đây rồi chạy lại cả hai).
Phòng boss: mục bossArenas + VỊ TRÍ 4 nến phòng đọc NGUYÊN từ Tools/cvm3/map3_boss.json (CVM3 09 — nguồn duy nhất; nội dung 2 nến giữa
phòng đo ở đây — map3_boss.json để trống vì harness Map3BossRoom chỉ dựng nến có content, đổi file đó sẽ đổi kịch bản b24).

Toạ độ = px section Unity (x tâm, Y = mặt đáy pivot như Entity). WalkwayB: x Unity = x SHEET/video + 112 (CVM3 05 nới trái).
Nến: Y = đáy đo − 1 (prefab Candle vẽ viewOffset +1 — như Map 1/2). Enemy: điểm sinh = chỗ video thấy lần đầu ở mép khung hiện vật.

Chạy (từ gốc repo):  python -P -E Tools/cvm3/gen_map3_objects.py
ConfigDrop: xoá mọi dòng do script này sinh (id nến WEA_C/WEB_C/WA_C/WB_C/MM_C + khối BLOCKS) rồi ghi lại ĐÚNG CHỖ dòng đầu tiên bị xoá
(chưa có ⇒ CUỐI file). Dòng Block 1/2 + khối phòng Mummy MummyMan_X83_9 (Tools/cvm3/boss9m_config.py) giữ nguyên thứ tự/byte;
gen_map1_objects.py / gen_map2_objects.py giữ dòng stage ≥ 7 khi chạy lại.
"""
import csv, io, json, os, re

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_JSON = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block3', 'Map3Objects.json')
DROP_CSV = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigDrop.csv')
BOSS_JSON = os.path.join(ROOT, 'Tools', 'cvm3', 'map3_boss.json')

PREF = 'Assets/Data/Prefabs/'
VID = ('video:Map_3 khớp mẫu candle_0/1 mọi frame không lặp, camera ref/data/map3_camera.csv (Tools/cvm3/fit10.py cd_all + candles10.py '
       'cluster; x = mốt khung camera đứng, Y = đáy đo − 1 vì Candle viewOffset +1); wiki: ảnh bản đồ nửa tỉ lệ cùng vị trí (±2 px)')
STAGE = {'WalkwayEntranceA': 7, 'WalkwayEntranceB': 7, 'WalkwayA': 8, 'WalkwayB': 8, 'MummyMan': 9}
PAD = {'WalkwayB': 112}
ORDER = ['WalkwayEntranceA', 'WalkwayEntranceB', 'WalkwayA', 'WalkwayB', 'MummyMan']

# (section, tiền tố id, [(x SHEET, Y, nội dung, số lần thấy, nguồn nội dung)]) — thứ tự theo x tăng; nguồn nội dung = frame vỡ (lần thấy
# cuối + 1), item khớp mẫu quanh nến 45 frame sau (candles10.py drops, err < 0.1 = chắc) + sự kiện HUD (ref/data/map3_events.txt) / dải ảnh
# _cache/10/view/*.png. Nến MummyMan 1311/1375/1439/1503: vị trí = map3_boss.json (khớp đo 0 px).
CANDLES = [
    ('WalkwayEntranceA', 'WEA_C', [
        (97, 144, 'SmallHeart', '37 lần f973–f1012 (camera cuộn — không có khung đứng); đáy đo 145',
         'video:Map_3 vỡ f1013 (0:33.77) → tim nhỏ f1026 (err 0.063), +1 f1032'),
        (161, 80, 'LargeHeart', '153 lần f834–f1103 (đứng 74/74); đáy đo 81',
         'video:Map_3 vỡ f1104 (0:36.80) → tim lớn f1116 (0.075), +5 f1120; wiki ảnh bản đồ St7: icon tim lớn góc trái tầng dưới'),
        (225, 160, 'SmallHeart', '156 lần f714–f942 (đứng 62/62); đáy đo 161', 'video:Map_3 vỡ f943 → tim nhỏ f953 (0.059), +1 f960'),
        (289, 160, 'SmallHeart', '74 lần f628–f725 (đứng 38/38); đáy đo 161', 'video:Map_3 vỡ f726 → tim nhỏ f742 (0.064), +1 f745'),
        (353, 160, 'SmallHeart', '69 lần f580–f672 (đứng 38/38); đáy đo 161', 'video:Map_3 vỡ f673 → tim nhỏ f690 (0.057), +1 f697'),
        (417, 160, 'RedMoneyBag', '75 lần f498–f592 (đứng 41/41); đáy đo 161', 'video:Map_3 vỡ f593 → túi đỏ f606 (0.062), +100 f619'),
        (480, 160, 'SmallHeart', '89 lần f392–f508 (đứng 39/39); đáy đo 161',
         'video:Map_3 vỡ f509 (0:16.97) → tim nhỏ f525 (0.047), +1 f566 (tim LỚN +5 f555 là vật Fleaman hạ f546 rơi — dải ảnh wea_480.png)'),
        (544, 160, 'HolyWater', '137 lần f207–f398 (đứng 118/118); đáy đo 161',
         'video:Map_3 vỡ f399 (0:13.30) → nước thánh f412 (0.093) nằm sàn, nhặt f491 0:16.37 (HUD đồng hồ → nước thánh); wiki Stage 7 #1 '
         '"Holy Water trong một trong các nến đầu" (icon ảnh bản đồ ≈ x 670 — H1: theo video)'),
        (608, 160, 'SmallHeart', '101 lần f207–f355 (đứng 100/100); đáy đo 161', 'video:Map_3 vỡ f356 → tim nhỏ f373 (0.057), +1 f377'),
        (672, 160, 'RedMoneyBag', '63 lần f207–f304 (đứng 62/62); đáy đo 161', 'video:Map_3 vỡ f305 → túi đỏ f331 (0.060), +100 f333'),
        (736, 144, 'WhiteMoneyBag', '22 lần f207–f247 (đứng 21/21); đáy đo 145',
         'video:Map_3 vỡ f248 (0:08.27) → túi trắng f278 (0.081), +700 f279 (0:09.30)')]),
    ('WalkwayEntranceB', 'WEB_C', [
        (160, 80, 'LargeHeart', '87 lần f1198–f1302 (đứng 80/80); đáy đo 81',
         'video:Map_3 vỡ f1303 → tim lớn f1334 (0.058) rơi xuống sàn y 160, +5 f1358; wiki ảnh bản đồ St7: icon tim lớn góc trái tầng trên'),
        (224, 144, 'RedMoneyBag', '145 lần f1198–f1380 (đứng 108/110); đáy đo 145', 'video:Map_3 vỡ f1381 → túi đỏ f1424 (0.061), +100 f1458'),
        (288, 144, 'RedMoneyBag', '137 lần f1346–f1531 (đứng 65/65); đáy đo 145', 'video:Map_3 vỡ f1532 → túi đỏ f1555 (0.060), +100 f1563'),
        (351, 144, 'SmallHeart', '45 lần f1524–f1583 (đứng 14/14); đáy đo 145', 'video:Map_3 vỡ f1584 → tim nhỏ f1596 (0.063), +1 f1613'),
        (423, 112, 'SmallHeart', '156 lần f1576–f1824 (đứng 70/70); đáy đo 113', 'video:Map_3 vỡ f1825 → tim nhỏ f1836 (0.066), +1 f1842'),
        (479, 160, 'RedMoneyBag', '216 lần f1614–f1878 (đứng 129/129); đáy đo 161', 'video:Map_3 vỡ f1879 → túi đỏ f1895 (0.056), +100 f1897'),
        (543, 160, 'SmallHeart', '215 lần f1662–f1924 (đứng 136/136); đáy đo 161', 'video:Map_3 vỡ f1925 → tim nhỏ f1936 (0.061), +1 f1960'),
        (607, 144, 'Axe', '242 lần f1906–f2281 (đứng 109/109); đáy đo 145',
         'video:Map_3 vỡ f2282 (1:16.07) → RÌU f2292–2301 (0.060) rơi qua khe x 592–623 xuống đáy section, mất f2308 — không nhặt (dải ảnh '
         'web607_axe.png); wiki Stage 7 #3 "Axe ở nến thấp nhất (rơi mất nếu không nhặt ngay)" + icon rìu ảnh bản đồ'),
        (671, 144, 'RedMoneyBag', '118 lần f2127–f2324 (đứng 58/58); đáy đo 145', 'video:Map_3 vỡ f2325 → túi đỏ f2357 (0.059), +100 f2358'),
        (735, 96, 'PurpleMoneyBag', '122 lần f2261–f2420 (đứng 104/104); đáy đo 97',
         'video:Map_3 vỡ f2421 (1:20.70) → vật trùng thân Simon (đứng sát cột cửa) nhặt ngay: +400 f2433 + chữ "400" (dải ảnh web735.png) = '
         'túi tím (ConfigItem 400; CVM3 08 "nhặt TÚI TÍM ở cửa")')]),
    ('WalkwayA', 'WA_C', [
        (32, 80, 'LargeHeart', '21 lần f2647–f2676 (đứng 22/22); đáy đo 81',
         'video:Map_3 vỡ f2677 → tim lớn f2688 (0.061), +5 f2696; wiki ảnh bản đồ St8: icon tim lớn'),
        (104, 112, 'SmallHeart', '59 lần f2647–f2723 (đứng 59/59); đáy đo 113', 'video:Map_3 vỡ f2724 → tim nhỏ f2738 (0.055), +1 f2754'),
        (551, 48, 'Cross', '266 lần f2926–f3262 (đứng 79/79); đáy đo 49',
         'wiki:Stages 7-9#Stage 8 (1) "Boomerang trong nến DƯỚI cầu thang lên tầng trên" + icon thập tự/boomerang ảnh bản đồ wiki x ≈ 548 '
         '(eItemId Cross = Boomerang, như TEA_C1 Block 2) — CHỈ WIKI: video KHÔNG phá (Simon leo thang ↗ ngay dưới nến rồi đổi tầng f3263)'),
        (615, 112, 'RedMoneyBag', '33 lần f2959–f2997 (đứng 5/5); đáy đo 113', 'video:Map_3 vỡ f2998 → túi đỏ f3019 (0.069), +100 f3024'),
        (679, 112, 'SmallHeart', '56 lần f2990–f3067 (đứng 23/23); đáy đo 113', 'video:Map_3 vỡ f3068 → tim nhỏ f3082 (0.060), +1 f3089')]),
    ('WalkwayB', 'WB_C', [
        (104, 112, 'SmallHeart', '74 lần f3275–f3359 (đứng 6/6); đáy đo 113', 'video:Map_3 vỡ f3360 → tim nhỏ f3378 (0.056), +1 f3395'),
        (160, 80, 'Stopwatch', '207 lần f3275–f3559 (đứng 37/37); đáy đo 81',
         'video:Map_3 vỡ f3560 (1:58.67) → đồng hồ f3570 (0.082) — không nhặt (HUD giữ nước thánh); wiki Stage 8 #2 "Stop Watch trong nến '
         'bên phải cầu thang" + icon ảnh bản đồ'),
        (287, 80, 'LargeHeart', '57 lần f3604–f3669 (đứng 28/28); đáy đo 81', 'video:Map_3 vỡ f3670 → tim lớn f3680 (0.058), +5 f3710'),
        (671, 80, 'SmallHeart', '106 lần f4013–f4144 (đứng 75/75); đáy đo 81', 'video:Map_3 vỡ f4145 → tim nhỏ f4187 (0.051), +1 f4220'),
        (735, 80, 'SmallHeart', '139 lần f4128–f4297 (đứng 127/127); đáy đo 81', 'video:Map_3 vỡ f4298 → tim nhỏ f4310 (0.045), +1 f4335')]),
    ('MummyMan', 'MM_C', [
        (32, 80, 'SmallHeart', '20 lần f4546–f4585 (đứng 20/20); đáy đo 81', 'video:Map_3 vỡ f4586 → tim nhỏ f4601 (0.049), +1 f4606'),
        (1247, 80, 'SmallHeart', '44 lần f5550–f5606 (đứng 7/7); đáy đo 81', 'video:Map_3 vỡ f5607 → tim nhỏ f5637 (0.043), +1 f5684'),
        (1311, 80, 'Dagger', '143 lần f5582–f5774 (đứng 59/59); đáy đo 81',
         'video:Map_3 vỡ f5775 (3:12.50) → dao găm rơi xuống đỉnh khối f5785–5835, không nhặt (CVM3 09); wiki Stage 9 "Dagger trong nến phòng boss"'),
        (1375, 128, 'SmallHeart', '88 lần f5686–f5845 (đứng 43/43); đáy đo 129', 'video:Map_3 vỡ f5846 (trước khoá) → tim nhỏ f5867 (0.049), +1 f5880'),
        (1439, 128, 'SmallHeart', '63 lần f5820–f5901 (đứng 31/31); đáy đo 129', 'video:Map_3 vỡ f5902 (trước khoá) → tim nhỏ f5921 (0.048), +1 f5940'),
        (1503, 128, 'SmallHeart', '759 lần f5885–f6838 (đứng 747/747); đáy đo 129',
         'video:Map_3 vỡ f6839 (sau trận) → II f6857 (0.069), HUD II f6864: tim nhỏ đổi Double Shot theo luật ItemDrops.Resolve (vũ khí phụ '
         'trúng ≥ 10 lần — CVM3 09)')]),
]

# khối phá được (ô lưới CVM3 05) — id = BreakableBlock.ObjectId "<Section>_X<c>_<r>". MummyMan_X83_9 (Pot Roast phòng Mummy) do
# Tools/cvm3/boss9m_config.py ghi (CVM3 09) — KHÔNG ghi lại ở đây (đọc map3_boss.json để ghi chú placement).
BLOCKS = [
    ('WalkwayEntranceA', 18, 8, 'PotRoast', 'khối TRÁI bộ khối nhô thứ 2 (đi từ x 720)',
     'video:Map_3 0:26.5–0:26.8 roi vỡ (CVM3 05: ô thành đen f799) → thịt nướng rơi khỏi khối f795–803 (khớp mẫu roast x 296) xuống sàn y 176, '
     'không nhặt (Simon đầy máu); wiki Stage 7 #1 / Secrets "khối trái của bộ khối nhô thứ 2 chứa Pot Roast"'),
]


def candle(cid, x, y, content, seen, src):
    return dict(id=cid, kind='candle', x=x, y=y, content=content,
                src='%s — x %d, y đáy %d; %s; nội dung: %s' % (VID, x, y, seen, src))


def sp(name, prefab, x, y, facing, src, param=0, respawn=True):
    d = dict(name=name, prefab=PREF + prefab, x=x, y=y, facing=facing, respawn=respawn, src=src)
    if param:
        d['param'] = param
    return d


def edge(name, prefab, side, zmin, zmax, src, extra_above=0, max_spawns=0):
    d = dict(name=name, prefab=PREF + prefab, side=side, rows=[], playerRelativeY=True, zoneMinX=zmin, zoneMaxX=zmax,
             releaseOnBlock='', src=src)
    if max_spawns:
        d['maxSpawns'] = max_spawns
    if extra_above:
        d['extraAboveFeetPx'] = extra_above
    return d


# khung hiện vật thể (nến + SpawnPoint enemy): video:Map_3 37 nến — thấy ở 143/147 frame x màn 8–15, 0/34 ở 0–7, 7/16 ở 248, 0/224 ở
# 249–255 (Tools/cvm3/candles10.py window) = Map_2 ⇒ NES chỉ vẽ/giữ vật trong [cx + 8, cx + 248]; Skeleton/Fleaman/Raven hiện lần đầu ở
# x màn 9–10 / 246–248 (CVM3 07/08). Unity SpawnPoint activate/despawnMargin −8 ⇒ [cx + 8, cx + 248) (phải lệch 1 px như Block 2)
WINDOW_INSET = 8

RISE = 0.25
RISE_PX = 14
SECRET_SRC = ('trồi 0.25 px/tick = video:Map_3 1 px / 2 frame (St7 f1963–1987: 12 px / 24 frame; St8 f3417–3440: 11 px / 23 frame — '
              'Tools/cvm3/rise10.py); quãng trồi 14 px (risePx): hàng túi đầu tiên hiện 3 frame ≈ 6 tick sau khung NGỒI đầu (St7 f1958 → '
              'f1961; St8 f3411 → ≈ f3413) — với SecretCrouchTicks 4 (ConfigRule, đo Map_1) chỉ quãng 14 px khớp (16 px ⇒ 15 tick); '
              'nghỉ ở mặt sàn + 1 (đáy đo 130 = Y 129 — như Block 2); túi nhấp nháy đỏ→tím→trắng (moneybag_flash); keepOffscreen: wiki '
              'Secrets St8 "có thể hiện NGOÀI màn — đi lùi lại sau 1–2 s" (St7 cũng hiện bên trái ngoài màn nếu ngồi sát mép phải); '
              'lifetimeFromRest: tuổi thọ 188 (ConfigItem) tính từ lúc trồi xong — video St7 nhặt f2054 ≈ 197 tick sau khi sinh (> 188 tính từ '
              'lúc sinh), 134 sau khi nghỉ; Map_1 CB nhặt 176 tick sau khi nghỉ; wiki St7 "chạy tới nhặt trước khi nó biến mất" (thời lượng '
              'thật chưa đo được — video nhặt kịp)')


def main():
    boss = json.load(io.open(BOSS_JSON, encoding='utf-8'))
    bsec = {s['id']: s for s in boss['sections']}
    # nến phòng boss: vị trí phải trùng map3_boss.json (nguồn duy nhất CVM3 09)
    want = {(c['x'], c['y']) for c in bsec['MummyMan']['candles']}
    have = {(x, y) for x, y, *_ in CANDLES[4][2]}
    assert want <= have, 'nến map3_boss.json không có trong CANDLES: %s' % (want - have)
    for c in bsec['MummyMan']['candles']:
        mine = [r for r in CANDLES[4][2] if (r[0], r[1]) == (c['x'], c['y'])][0]
        assert not c['content'] or c['content'] == mine[2], 'nội dung nến %s lệch map3_boss.json' % c
    secs = {}
    for sec, pre, rows in CANDLES:
        wd = 2 if len(rows) >= 10 else 1
        secs[sec] = dict(id=sec, candles=[candle('%s%0*d' % (pre, wd, i + 1), x + PAD.get(sec, 0), y, c, seen, s)
                                          for i, (x, y, c, seen, s) in enumerate(rows)],
                         spawnPoints=[], edgeSpawners=[], waterSpawners=[], secrets=[], bossArenas=[], mechSpawners=[])
    S = secs
    E7 = 'CVM3 07 §2g (ref/data/enemy7/tracks.csv): '
    TW = 'x = mốt pivot sprite đầu tháp (dragonskull_purple_top) khớp mẫu CVM3 08 _cache/08/det_bt_all.csv (cả 2 đầu, 140–180 lần); CVM3 08 §2h ghi x − 3 (= gốc cầu lửa ngoại suy) — cặp ảnh pairs10.py: sprite Unity ở x − 3 lệch −3 px ⇒ đặt theo sprite (như Block 2 CVM2 10); '
    E8 = 'CVM3 08 §2h (ref/data/enemy8b/): '
    # ---- Stage 7: WalkwayEntranceA (vào cx 512, đi TRÁI)
    S['WalkwayEntranceA']['spawnPoints'] = [
        sp('Fleaman_WEA_1', 'Enemy/Fleaman.prefab', 433, 128, 0,
           E7 + 'video:Map_3 f491 (0:16.37) Fleaman F0 thấy lần đầu ở x màn 9 (cx 424, camera cuộn trái) ngồi trên bục (433, 128); +500 f545'),
        sp('Fleaman_WEA_2', 'Enemy/Fleaman.prefab', 305, 128, 0,
           E7 + 'video:Map_3 f621 F1 x màn 10 (cx 295) trên bục (305, 128); +500 f675'),
        sp('Fleaman_WEA_3', 'Enemy/Fleaman.prefab', 177, 96, 0,
           E7 + 'video:Map_3 f825 F2 x màn 9 (cx 168) trên bục (177, 96); +500 f918'),
        sp('Skeleton_WEA', 'Enemy/Skeleton.prefab', 33, 128, 0,
           E7 + 'video:Map_3 f1006 (0:33.53) Skeleton S1 hiện ở x màn 10 (cx 23) trên bờ (33, 128) góc dưới-trái rồi đi ngay; +300 f1078; '
           'wiki Stage 7 #2 "Skeleton ở góc dưới-trái"')]
    S['WalkwayEntranceA']['edgeSpawners'] = [edge('Ghost_WEA', 'Enemy/Ghost.prefab', 'BehindPlayer', 0, 576,
        E8 + 'video:Map_3 Ghost hiện f416 (0:13.87) ở (671, 145) = 96 px SAU LƯNG Simon đi trái (x 577 f415 → 575 f416; Config Ghost không '
        'có firstDelay ⇒ sinh ngay khi vào vùng) ⇒ biên phải 576; Simon đi tiếp tới x 94 không có Ghost nữa ⇒ 1 con / lần nạp (maxSpawns 1); '
        'độ cao lúc hiện = chân Simon (176) − 31 (Map_2 Hall C = chân — CVM3 08 §6) ⇒ extraAboveFeetPx 31 riêng bộ sinh này (1 mẫu; giả '
        'thuyết khác khớp 3/3: tâm = min(chân, y 144)) — XẤP XỈ biên (không khoá Y: video chỉ có Simon trên sàn y 176)',
        extra_above=31, max_spawns=1)]
    # ---- Stage 7: WalkwayEntranceB (vào bằng thang ↖ từ WEA, đi PHẢI)
    S['WalkwayEntranceB']['spawnPoints'] = [
        sp('Raven_WEB_1', 'Enemy/Raven.prefab', 280, 41, 0, E8 + 'video:Map_3 Raven #0 đậu (280, 41) hiện f1340 ở x màn 246–248 (tâm sprite); bay ra màn f1490 (không hạ)'),
        sp('Skeleton_WEB_1', 'Enemy/Skeleton.prefab', 352, 160, 0,
           E7 + 'video:Map_3 f1517 (0:50.57) Skeleton S2 hiện ở x màn 246 (cx 105) trên bục (352, 160); +300 f1553'),
        sp('Raven_WEB_2', 'Enemy/Raven.prefab', 423, 89, 0, E8 + 'video:Map_3 Raven #1 đậu (423, 89) hiện f1574 cùng lúc nến (423, 112) ở x màn 247; +200'),
        sp('Raven_WEB_3', 'Enemy/Raven.prefab', 487, 57, 0, E8 + 'video:Map_3 Raven #2 đậu (487, 57) hiện f1634; +200'),
        sp('Raven_WEB_4', 'Enemy/Raven.prefab', 527, 169, 0, E8 + 'video:Map_3 Raven #3 đậu (527, 169) hiện f1654; +200 f1732 (rơi tim nhỏ — luật rơi chưa dựng)'),
        sp('Skeleton_WEB_2', 'Enemy/Skeleton.prefab', 671, 160, 0,
           E7 + 'video:Map_3 f2123 (1:10.77) Skeleton S3 hiện ở x màn 246 (cx 425) trên bục (671, 160); bật mép 608 rơi hố 544–607 '
           '(wiki Stage 7 #3 "gờ quá cao — nó rơi xuống hố")')]
    S['WalkwayEntranceB']['secrets'] = [dict(
        name='Secret_St7Bag', x=536, y=176, area=[528, 160, 22, 17], condition='Crouch', holdTicks=0, item='FlashingMoneyBag',
        spawnAt=[424, 129], rise=True, riseSpeed=RISE, risePx=RISE_PX, keepOffscreen=True, lifetimeFromRest=True,
        src='video:Map_3 1:05.2–1:08.5: Simon đứng x 537 trên bục THẤP NHẤT (cột 30–33, x 480–543, mặt y 176 — mép phải) f1951–1957, NGỒI '
            'f1958–1972 (khớp mẫu jumpduck_0) ⇒ túi 1000 trồi trên bục CAO bên trái (cột 26–27, mặt y 128) ở x 424 (fit10.py bag7: đáy đo '
            '130 ⇒ Y 129), hàng đầu hiện f1961, nghỉ f1987; Simon đi trái, nhảy lên bục nhặt f2054 +1000 (1:08.47). Vùng = ô phải nhất '
            'của bục (x 528–543) + phần đứng chìa mép (đầu dò sàn X±6 ⇒ X ≤ 549), Y 160–176 — video ngồi ở 537 (1 mẫu; wiki "mép phải '
            'bục thấp nhất" / Secrets "ngồi ở tận cùng bục cụt") — XẤP XỈ biên trái vùng; holdTicks 0 = ConfigRule.SecretCrouchTicks 4; ' + SECRET_SRC)]
    # ---- Stage 8: WalkwayA (sau cửa phải #1, đi PHẢI)
    S['WalkwayA']['spawnPoints'] = [
        sp('Skeleton_WA', 'Enemy/Skeleton.prefab', 671, 64, 0,
           E7 + 'video:Map_3 f2986 (1:39.53) Skeleton S4 hiện ở x màn 247 (cx 424) trên khối (671, 64); bật mép 640 rơi xuống cầu; +300 f3073 '
           '(rơi tim nhỏ); wiki Stage 8 "Skeleton tầng dưới"')]
    S['WalkwayA']['edgeSpawners'] = [edge('MedusaHead_WA', 'Enemy/MedusaHead.prefab', 'FrontOfPlayer', 144, 4096,
        E8 + 'ref/data/enemy8b/mh_spawns.csv: con đầu (đỉnh sóng) f2790.6 khi Simon x 183 (đi phải); firstDelay Config 17–56 tick ⇒ vào '
        'vùng f2762.6–2782.1 = Simon x 125–164 ⇒ biên trái 144 (giữa — XẤP XỈ); sinh tới f3230 (Simon trên thang ↗ x 639) ⇒ biên phải = hết '
        'section; 7 con, khoảng 128 (4 khoảng lệch — CVM3 08 §6); wiki Stage 8 "Medusa Head tầng dưới"')]
    # ---- Stage 8: WalkwayB (x Unity = sheet + 112)
    S['WalkwayB']['spawnPoints'] = [
        sp('Raven_WB_1', 'Enemy/Raven.prefab', 271 + 112, 57, 0, E8 + 'video:Map_3 Raven #4 đậu (271, 57) sheet (Unity 383) hiện f3594; +200 f3644'),
        sp('BoneTower_WB_1', 'Enemy/BoneTower.prefab', 367 + 112, 128, 0,
           E8 + TW + 'video:Map_3 Bone Tower trên cầu (367, 128) sheet (Unity 479) f3695–3835, +400 f3837; wiki Stage 8 "Bone Tower tầng trên"'),
        sp('Raven_WB_2', 'Enemy/Raven.prefab', 487 + 112, 121, 0, E8 + 'video:Map_3 Raven #5 đậu (487, 121) sheet (Unity 599) hiện f3865; +200 f3917'),
        sp('Raven_WB_3', 'Enemy/Raven.prefab', 551 + 112, 121, 0, E8 + 'video:Map_3 Raven #6 đậu (551, 121) sheet (Unity 663) hiện f3898; +200 f3957'),
        sp('BoneTower_WB_2', 'Enemy/BoneTower.prefab', 623 + 112, 128, 0,
           E8 + TW + 'video:Map_3 Bone Tower trên cầu (623, 128) sheet (Unity 735) f3991–4094, +400 f4096')]
    S['WalkwayB']['secrets'] = [dict(
        name='Secret_St8Bag', x=256, y=128, area=[240, 112, 32, 17], condition='Crouch', holdTicks=0, item='FlashingMoneyBag',
        spawnAt=[16 + 112, 129], rise=True, riseSpeed=RISE, risePx=RISE_PX, keepOffscreen=True, lifetimeFromRest=True,
        src='video:Map_3 1:53.7–1:55.9: Simon NGỒI f3411–3423 ở x 134 sheet (Unity 246) trên cầu y 128, ngay trên 2 khối lơ lửng không '
            'với tới (Unity cột 15–16, x 240–271, y 160 — sheet 128–159; CVM3 02 ghi "không ngồi" là sót: khớp mẫu nhầm hurt_0 err 0.20, y '
            'thấp 5 px) ⇒ túi 1000 trồi ở MÚT TRÁI cầu x 16 sheet = Unity 128 (fit10.py bag8: đáy đo 130 ⇒ Y 129; x màn 10 khi cx 6), hàng '
            'đầu ≈ f3413, nghỉ f3440; Simon đi trái nhặt f3478 +1000 (1:55.93). Vùng = phía trên 2 khối x 240–271 (wiki Secrets "quỳ NGAY '
            'TRÊN 2 khối lơ lửng — dưới nến Stop Watch"), Y 112–128 — XẤP XỈ (1 mẫu x 246); holdTicks 0 = ConfigRule.SecretCrouchTicks 4; '
            + SECRET_SRC)]
    # ---- Stage 9: MummyMan (sau cửa phải #2, đi PHẢI; phòng Mummy khoá cx 1280)
    S['MummyMan']['spawnPoints'] = [
        sp('Raven_MM_1', 'Enemy/Raven.prefab', 268, 41, 0, E8 + 'video:Map_3 Raven #7 đậu (268, 41) hiện f4668; +200 f4700; wiki Stage 9 #1 "Raven cất cánh khi lại gần"'),
        sp('Raven_MM_2', 'Enemy/Raven.prefab', 363, 41, 0, E8 + 'video:Map_3 Raven #8 đậu (363, 41) hiện f4715; +200 f4745'),
        sp('BoneTower_MM_1', 'Enemy/BoneTower.prefab', 415, 96, 0, E8 + TW + 'video:Map_3 Bone Tower trên cầu (415, 96) f4741–4851, +400 f4854'),
        sp('Raven_MM_3', 'Enemy/Raven.prefab', 495, 33, 0, E8 + 'video:Map_3 Raven #9 đậu (495, 33) hiện f4872; +200 f4904'),
        sp('Raven_MM_4', 'Enemy/Raven.prefab', 559, 33, 0, E8 + 'video:Map_3 Raven #10 đậu (559, 33) hiện f4904; +200 f4933'),
        sp('BoneTower_MM_2', 'Enemy/BoneTower.prefab', 671, 96, 0, E8 + TW + 'video:Map_3 Bone Tower trên cầu (671, 96) f4959–5075, +400 f5077 (rơi tim lớn — luật rơi chưa dựng)'),
        sp('Raven_MM_5', 'Enemy/Raven.prefab', 747, 41, 0, E8 + 'video:Map_3 Raven #11 đậu (747, 41) hiện f5086; +200 f5123'),
        sp('Skeleton_MM_1', 'Enemy/Skeleton.prefab', 815, 96, 0,
           E7 + 'video:Map_3 f5121 (2:50.70) Skeleton S5 hiện ở x màn 247 (cx 568) trên bục cầu hỏng (815, 96), ném xương cao rồi nhảy khe '
           '768–799; +300 f5151; wiki Stage 9 #2 "Skeleton nhảy qua cầu hỏng"'),
        sp('Skeleton_MM_2', 'Enemy/Skeleton.prefab', 879, 96, 0,
           E7 + 'video:Map_3 f5184 Skeleton S6 hiện ở x màn 247 (cx 632) trên bục (879, 96), nhảy khe 832–863 đáp bục 800–831'),
        sp('BoneTower_MM_3', 'Enemy/BoneTower.prefab', 1183, 96, 0, E8 + TW + 'video:Map_3 Bone Tower (1183, 96) f5394–5509, +400 f5511 (+100 cầu lửa cùng nhát)')]
    S['MummyMan']['edgeSpawners'] = [edge('MedusaHead_MM', 'Enemy/MedusaHead.prefab', 'FrontOfPlayer', 976, 1259,
        E8 + 'ref/data/enemy8b/mh_spawns.csv: con đầu f5370.7 khi Simon x 1018 (đi phải) ⇒ vào vùng 17–56 tick trước = f5342.7–5362.2 = '
        'Simon x 959–998 ⇒ biên trái 976 (XẤP XỈ); con cuối f5716.9 Simon 1245, lượt kế (≈ f5781) KHÔNG sinh khi Simon 1273 (chỉ 1 con '
        'sống — không vướng maxAlive) ⇒ biên phải ∈ [1245, 1272] ⇒ 1259 (XẤP XỈ); 7 con; cầu hỏng St9 (x 768–927) KHÔNG có Medusa Head '
        'trong video (wiki có — H1 theo video); phòng Mummy không có')]
    S['MummyMan']['secrets'] = [dict(
        name='Secret_St9Bag', x=1320, y=128, area=[1306, 112, 28, 17], condition='WalkLeft', holdTicks=120, item='FlashingMoneyBag',
        spawnAt=[1264, 97], rise=True, riseSpeed=RISE, risePx=RISE_PX, keepOffscreen=True, lifetimeFromRest=True, beforeBossLock=True,
        src='CHỈ WIKI — wiki Secrets Stage 9 "trong phòng boss, TRƯỚC khi khoá màn: đứng trên khối nằm trên khối chứa thịt nướng và đi '
            'sang trái vài giây ⇒ túi tiền hiện nhưng thường không với tới"; video không làm. Khối = (82,8) x 1312–1327 mặt y 128 (ngay trên-trái '
            'khối thịt nướng (83,9)); đi trái ⇒ đẩy vào tường (81,7) (Simon dừng X 1320) ⇒ vùng X 1306–1333 (đầu dò sàn X±6), Y 112–128, điều '
            'kiện WalkLeft (giữ ←, trạng thái Walk), chỉ khi camera CHƯA khoá (beforeBossLock — khoá ở cx 1280 khi Simon x ≥ 1408). '
            'holdTicks 120 = SUY LUẬN ("vài giây" như rương Stage 5 đo video Map_2 121 tick); chỗ túi (1264, 97) = SUY LUẬN (wiki không nói '
            'vị trí — đặt trên gờ (78–79,6) bên trái-trên sau tường, giống St7/St8 túi hiện bên TRÁI; §5 hỏi người dùng); trồi/tuổi thọ theo '
            'St7/St8 đo video: ' + SECRET_SRC)]
    for k in S:
        S[k]['objectWindowInsetPx'] = WINDOW_INSET
    for bid, bs in bsec.items():
        S[bid]['bossArenas'] = bs['bossArenas']
    doc = dict(
        note='CVM3 10 — vật thể Map 3 (Block 3: Stage 07–09) đặt theo video Map_3 (ClaudeNotes/cvm3/MAP3_PLACEMENT.md). Sinh bởi '
             'Tools/cvm3/gen_map3_objects.py; menu CVM3/Build Map3 Objects dựng Assets/Data/Prefabs/Level/Block3/Objects/<Section>_Objects.prefab. '
             'Toạ độ px section Unity (x tâm, y = mặt đáy pivot; WalkwayB x = sheet + 112). Nội dung nến/khối = ConfigDrop.csv theo id. '
             'bossArenas chép từ Tools/cvm3/map3_boss.json (CVM3 09). objectWindowInsetPx 8 = nến/enemy SpawnPoint chỉ hiện ở x màn 8–247 '
             '(video:Map_3 8–248).',
        sections=[S[k] for k in ORDER])
    with io.open(OUT_JSON, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write('\n')
    # ConfigDrop: bỏ dòng do script này sinh (đúng chỗ dòng đầu bị bỏ — chưa có ⇒ cuối file)
    out = []
    for k in ORDER:
        for c in S[k]['candles']:
            out.append([c['id'], str(STAGE[k]), 'WallCandle', c['content'], 'nến %s x %d y %d' % (k, c['x'], c['y']),
                        c['src'].split('nội dung: ')[1]])
    for sec, col, row, item, note, src in BLOCKS:
        out.append(['%s_X%d_%d' % (sec, col, row), str(STAGE[sec]), 'BreakableBlock', item, '%s (ô (%d,%d))' % (note, col, row), src])
    mine = {r[0] for r in out}
    pat = re.compile(r'^(WEA|WEB|WA|WB|MM)_C\d+$')
    rows = list(csv.reader(io.open(DROP_CSV, encoding='utf-8')))
    head, body = rows[0], rows[1:]
    ours = [i for i, r in enumerate(body) if r[0] in mine or (7 <= int(r[1]) <= 9 and pat.match(r[0]))]
    keep = [r for i, r in enumerate(body) if i not in set(ours)]
    at = ours[0] if ours else len(keep)
    with io.open(DROP_CSV, 'w', encoding='utf-8', newline='') as f:
        w = csv.writer(f, lineterminator='\n')
        w.writerow(head)
        w.writerows(keep[:at])
        w.writerows(out)
        w.writerows(keep[at:])
    n = sum(len(S[k]['candles']) for k in ORDER)
    print('ghi', OUT_JSON, n, 'nen;', DROP_CSV, len(keep) + len(out), 'dong (Block 3:', len(out), ')')


if __name__ == '__main__':
    main()
