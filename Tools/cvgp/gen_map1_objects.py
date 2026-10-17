"""CVGP 15 — sinh bảng vật thể Map 1 (Assets/Data/Config/Level/Block1/Map1Objects.json) + các dòng nến/chậu của ConfigDrop.csv
từ số đo video (map1_props.py / map1_drops.py + kiểm tay — MAP1_PLACEMENT.md). Chạy 1 lần khi đổi số đo; JSON là nguồn sự thật cho
menu CVGP/Build Map1 Objects (sửa JSON rồi chạy lại menu).

Chạy (từ gốc repo):  python -P -E Tools/cvgp/gen_map1_objects.py
"""
import csv, io, json, os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_JSON = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block1', 'Map1Objects.json')
DROP_CSV = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigDrop.csv')

PREF = 'Assets/Data/Prefabs/'
VID = 'video:Map_1 khớp mẫu candle_*/torch_* (Tools/cvgp/map1_props.py, camera planner)'

# (id, x tâm, Y = mặt đáy pivot CandleProp, nội dung, nguồn nội dung, ghi chú wiki)
CE = [('CE_T1', 96, 160, 'LargeHeart', 'video:Map_1 0:03.2 vỡ → tim lớn, +5 0:03.93', 'wiki #B1 Large Heart'),
      ('CE_T2', 224, 160, 'MorningStar', 'video:Map_1 0:05.8 → roi, nhặt 0:06.5', 'wiki #B2 Morning Star'),
      ('CE_T3', 352, 160, 'MorningStar', 'video:Map_1 0:08.9 → roi, nhặt 0:09.8', 'wiki #B3 Morning Star'),
      ('CE_T4', 480, 160, 'LargeHeart', 'video:Map_1 0:12.1 → tim lớn, +5 0:12.97', 'wiki #B4 Large Heart'),
      ('CE_T5', 608, 160, 'Dagger', 'video:Map_1 0:14.5 → dao, nhặt 0:15.50', 'wiki #B5 Dagger')]

EH_Y = [160, 128, 160, 128, 160, 128, 160, 128, 160, 128, 64, 160, 32, 64, 160, 64, 160, 160, 160, 160, 160, 128, 64, 32]
EH_C = {  # nến #n: (nội dung, nguồn video)
    1: ('PurpleMoneyBag', 'vỡ f631 → túi tím, +400 f655 (0:21.83)'),
    2: ('SmallHeart', 'vỡ f724 → tim nhỏ đung đưa, tim 15→16 f762 — wiki ghi Stopwatch (H1: theo video)'),
    3: ('SmallHeart', 'vỡ f761 → tim nhỏ, 16→17 f788'),
    4: ('RedMoneyBag', 'vỡ f821 → túi đỏ, +100 f847'),
    5: ('SmallHeart', 'vỡ f930 → nhặt ngay, tim 18→19 f950'),
    6: ('RedMoneyBag', 'vỡ f979 → túi đỏ, +100 f1001'),
    7: ('SmallHeart', 'vỡ f1008 → tim 19→20 f1071'),
    8: ('SmallHeart', 'vỡ f1089 → tim 20→21 f1125'),
    9: ('SmallHeart', 'vỡ f1166 → tim 21→22 f1190'),
    10: ('SmallHeart', 'vỡ f1210 → tim 22→23 f1248'),
    11: ('SmallHeart', 'vỡ f1493 → tim 23→24 f1564'),
    12: ('WhiteMoneyBag', 'vỡ f1319 → túi trắng, +700 f1340 (0:44.67)'),
    13: ('HolyWater', 'vỡ f1698 → nước thánh rơi xuống lối đi (0:57.3, không nhặt) — wiki gán Holy Water cho #14 (H1: theo video)'),
    14: ('LargeHeart', 'vỡ f1641 → tim lớn, +5 f1676 (0:55.87) — wiki ghi Holy Water'),
    15: ('WhiteMoneyBag', 'vỡ f1914 → túi trắng, +700 f1935 (1:04.50)'),
    16: ('SmallHeart', 'vỡ f1809 → tim 29→30 f1845'),
    17: ('SmallHeart', 'vỡ f1991 → tim 30→31 f2016'),
    18: ('SmallHeart', 'vỡ f2030 → tim 31→32 f2058'),
    19: ('SmallHeart', 'vỡ f2069 → tim nhỏ (dải ảnh), 32→33 f2135'),
    20: ('SmallHeart', 'vỡ f2150 → tim 33→34 f2178'),
    21: ('SmallHeart', 'vỡ f2190 → tim 34→35 f2232'),
    22: ('SmallHeart', 'KHẢ NĂNG: vỡ sau f2268 (bị rosary nháy che), tim 35→36 f2311 — rosary 1:15.03 là Zombie chết rơi ra'),
    23: ('SmallHeart', 'vỡ f2463 → tim 36→37 f2488 — wiki gán Rosary cho #23'),
    24: ('Rosary', 'vỡ f2505 (1:23.5) → rosary, nhặt 1:24.73 — wiki: Rosary ở #23 (H1: theo video)'),
}
CA_X = [32, 96, 160, 224, 304, 352, 416, 480]
CA_Y = [160, 64, 48, 128, 48, 160, 32, 64]
CA_C = {1: ('WhiteMoneyBag', 'vỡ f3152 → +700 f3177 (1:45.90)'),
        2: ('LargeHeart', 'vỡ f2778 → +5 f2827 (1:34.23)'),
        3: ('SmallHeart', 'vỡ f2856 → tim 41→42 f2895'),
        4: ('RedMoneyBag', 'vỡ f2968 (roi trên thang, dải ảnh _cache/15/ca_bag.png) → túi đỏ, +100 f3004 (1:40.13)'),
        5: ('LargeHeart', 'vỡ f4691 → +5 f4728 (2:37.60)'),
        6: ('RedMoneyBag', 'vỡ f4467 → +100 f4503 (2:30.10)'),
        7: ('Rosary', 'vỡ f4830 → rosary trên lối đi, nhặt 2:42.7 — wiki #13 Rosary (khớp)'),
        8: ('RedMoneyBag', 'vỡ f4798 → túi đỏ, +100 f4859 (2:41.97)')}
CB_X = [32, 96, 160, 224, 288, 352, 416, 496]
CB_C = {1: ('WhiteMoneyBag', 'KHẢ NĂNG: vỡ sau f3666 (dải ảnh), +700 f3690 (2:03.0)'),
        2: ('RedMoneyBag', 'vỡ f3485–3500 (ảnh _cache/15/cb_start.png) → túi đỏ, +100 f3502 (1:56.73)'),
        3: ('RedMoneyBag', 'vỡ f3739 → +100 f3768'),
        4: ('WhiteMoneyBag', 'vỡ f3849 → +700 f3868 (2:08.93)'),
        5: ('Stopwatch', 'vỡ f3917 → đồng hồ (không nhặt) — wiki #10 Stopwatch (khớp)'),
        6: ('SmallHeart', 'KHẢ NĂNG: tim 47→48 f4007 (vỡ không bắt được — dải ảnh)'),
        7: ('SmallHeart', 'vỡ f4032 → tim 48→49 f4069'),
        8: ('LargeHeart', 'vỡ f4114 → +5 f4155 (2:18.50)')}
PB_Y = [128, 64, 96, 64, 160, 128, 160, 160, 128, 160, 128, 128]
PB_C = {1: ('LargeHeart', 'vỡ f5434 → +5 f5456 (3:01.87)'),
        2: ('SmallHeart', 'vỡ f5137 → tim 60→61 f5170'),
        3: ('RedMoneyBag', 'vỡ f5249 → +100 f5274'),
        4: ('InvisibilityPotion', 'vỡ f5315 (2:57.2) → bình, nhặt 2:58.2 — wiki: bình "giữa cầu thang", ứng viên #3/#4'),
        5: ('SmallHeart', 'vỡ f5594 → tim 66→67 f5624'),
        6: ('Axe', 'vỡ f5663 → rìu, nhặt 3:10.67 — wiki #6 Axe (khớp)'),
        7: ('SmallHeart', 'vỡ f5739 → tim 67→68 f5769'),
        8: ('SmallHeart', 'vỡ f5782 → tim 68→69 f5813'),
        9: ('LargeHeart', 'vỡ f5829 → +5 f5858 (3:15.27)'),
        10: ('SmallHeart', 'vỡ f5880 → tim 74→75 f5907'),
        11: ('SmallHeart', 'vỡ f5943 → tim 75→76 f5973'),
        12: ('LargeHeart', 'vỡ f6000 → +5 f6040 (3:21.33)')}


def candle(cid, kind, x, y, content, src_item, wiki):
    return dict(id=cid, kind=kind, x=x, y=y, content=content,
                src='%s — x %d (lưới %s, ±1), y đáy %d; nội dung: %s; %s' % (
                    VID, x, '96+128k' if kind == 'torch' else '32+64k', y, src_item, wiki))


def main():
    secs = []
    ce = [candle(c[0], 'torch', c[1], c[2], c[3], c[4], c[5]) for c in CE]
    secs.append(dict(
        id='CastleEntrance',
        candles=ce,
        spawnPoints=[],
        edgeSpawners=[],
        waterSpawners=[],
        secrets=[dict(name='Secret_GateBag', x=704, y=160, area=[704, 0, 64, 161], condition='Enter', holdTicks=0,
                      item='FlashingMoneyBag', spawnAt=[632, 160], rise=True,
                      src='wiki Secrets/Stages 1-3 note 1: nhảy VƯỢT cửa lâu đài ⇒ túi 1000 nhấp nháy hiện phía sau (ảnh wiki: bên trái cửa x≈632); '
                          'video không có. Vùng = qua mép FG vòm cổng (x ≥ 704 — CVGP 06) mà không bị cổng kéo vào (Map1FlowTiming.GATE_WINDOW_MAX_X): SUY LUẬN')],
        bossArenas=[]))
    eh = []
    for n in range(1, 25):
        it, s = EH_C[n]
        eh.append(candle('EH_C%02d' % n, 'candle', 32 + 64 * (n - 1), EH_Y[n - 1], it, 'video:Map_1 ' + s, 'wiki §6.1 nến #%d' % n))
    secs.append(dict(
        id='EntranceHall',
        candles=eh,
        spawnPoints=[dict(name='Panther_%d' % (i + 1), prefab=PREF + 'Enemy/Panther.prefab', x=x, y=y, facing=-1, respawn=True,
                          src='CVGP 13 §4b "Enemy" — báo nằm chờ trên gờ (video 0:42.0 / 0:54.0)')
                     for i, (x, y) in enumerate([(699, 112), (891, 80), (955, 112)])],
        edgeSpawners=[
            dict(name='Ghoul_Hall', prefab=PREF + 'Enemy/Ghoul.prefab', side='Right', rows=[176], playerRelativeY=False,
                 zoneMinX=0, zoneMaxX=383, releaseOnBlock='',
                 src='video:Map_1 trk_z_eh (CVGP 13): 12 Zombie sinh khi Simon x 42–364 (cx 0–236), không sinh khi Simon 365–730 (f1021–1340, '
                     '> nghỉ tối đa 288 tick) ⇒ vùng = màn đầu (Simon ≤ cx 255 + 128 = 383) — biên XẤP XỈ'),
            dict(name='Ghoul_Stairs', prefab=PREF + 'Enemy/Ghoul.prefab', side='Right', rows=[176], playerRelativeY=False,
                 zoneMinX=1130, zoneMaxX=1536, releaseOnBlock='',
                 src='video:Map_1 trk_z_eh: Zombie đầu nhóm sau f2062 lúc Simon x 1132 (f2060 x 1128, firstDelay 0) — không sinh khi Simon 899–1128 '
                     '(f1900–2060, 320 tick); sinh tới hết cầu thang dài (Simon 1352, f2270; sau đó đủ 3 con) ⇒ [1130, hết section]')],
        waterSpawners=[],
        secrets=[],
        bossArenas=[]))
    ca = []
    for n in range(1, 9):
        it, s = CA_C[n]
        ca.append(candle('CA_C%d' % n, 'candle', CA_X[n - 1], CA_Y[n - 1], it, 'video:Map_1 ' + s, 'wiki §6.2 tầng trên #%d' % (2 * n - 1)))
    secs.append(dict(
        id='CryptA',
        candles=ca,
        spawnPoints=[],
        edgeSpawners=[
            dict(name='Bat_Crypt', prefab=PREF + 'Enemy/Bat.prefab', side='FrontOfPlayer', rows=[], playerRelativeY=True,
                 zoneMinX=0, zoneMaxX=512, releaseOnBlock='',
                 src='CVGP 13: dơi CA cả section (video 1:32.0–1:47.4 lần 1, 2:28.5–2:42.7 lần 2), trước mặt Simon'),
            dict(name='Bat_PotRoastWall', prefab=PREF + 'Enemy/Bat.prefab', side='FrontOfPlayer', rows=[], playerRelativeY=True,
                 zoneMinX=99999, zoneMaxX=99999, releaseOnBlock='CryptA_X16_10',
                 src='wiki Secrets: phá tường Pot Roast thả 1 dơi (CVGP 13 releaseOnBlock) — video không thấy (dơi lúc đó đã có)')],
        waterSpawners=[],
        secrets=[],
        bossArenas=[]))
    cb = []
    for n in range(1, 9):
        it, s = CB_C[n]
        cb.append(candle('CB_C%d' % n, 'candle', CB_X[n - 1], 48, it, 'video:Map_1 ' + s,
                         'wiki §6.2 tầng dưới #%d (toạ độ wiki − (16,160))' % (2 * n)))
    secs.append(dict(
        id='CryptB',
        candles=cb,
        spawnPoints=[],
        edgeSpawners=[],
        waterSpawners=[dict(name='Fishman_Water', prefab=PREF + 'Enemy/Fishman.prefab', surfaceY=160, zoneMinX=0, zoneMaxX=512,
                            src='CVGP 13 §4b "Enemy": Fish Man cả Crypt B (video 1:57.2–2:26.7), mặt nước y 160')],
        secrets=[dict(name='Secret_LowBlocks', x=464, y=160, area=[448, 144, 32, 17], condition='Crouch', holdTicks=0,
                      item='FlashingMoneyBag', spawnAt=[352, 96], rise=True,
                      src='video:Map_1 2:19.7–2:24.7: Simon ngồi trên khối thấp (28–29,10) ở x 471 Y 160 (f4196, khớp mẫu jumpduck_0) ⇒ túi 1000 '
                          'trồi ở pivot (352,96) (f4226 khớp mẫu moneybag, đứng lối đi y 96); wiki Secrets Bonuses Stage 2')],
        bossArenas=[]))
    pb = []
    for n in range(1, 13):
        it, s = PB_C[n]
        pb.append(candle('PB_C%02d' % n, 'candle', 32 + 64 * (n - 1), PB_Y[n - 1], it, 'video:Map_1 ' + s, 'wiki §6.3 nến #%d' % n))
    secs.append(dict(
        id='PhantomBat',
        candles=pb,
        spawnPoints=[],
        edgeSpawners=[
            dict(name='Ghoul_Corridor', prefab=PREF + 'Enemy/Ghoul.prefab', side='Right', rows=[112, 176], playerRelativeY=False,
                 zoneMinX=0, zoneMaxX=320, releaseOnBlock='',
                 src='video:Map_1 trk_z_pb (CVGP 13): Zombie gờ y 112 + sàn y 176 khi Simon x 49–310 (cx 0–185); không sinh khi Simon > 344 '
                     '(3:10.67 →) tới phòng boss ⇒ [0, 320] — biên XẤP XỈ')],
        waterSpawners=[],
        secrets=[],
        bossArenas=[dict(name='BossArena_PhantomBat', prefab=PREF + 'Boss/PhantomBat.prefab', zoneMinX=0, zoneMaxX=4096,
                         useConfigPosition=True, bossPosition=[623, 41], screenWalls=True,
                         src='CVGP 14 §4b "Boss": dơi treo trần (623,41) sinh khi vào section (video 3:15 thấy từ hành lang), camera khoá cx 512')]))
    doc = dict(
        note='CVGP 15 — vật thể Map 1 đặt theo video (MAP1_PLACEMENT.md). Sinh bởi Tools/cvgp/gen_map1_objects.py; menu CVGP/Build Map1 Objects '
             'dựng Assets/Data/Prefabs/Level/Block1/Objects/<Section>_Objects.prefab. Toạ độ px section (x tâm, y = mặt đáy pivot). '
             'Nội dung nến = ConfigDrop.csv theo id.',
        sections=secs)
    with io.open(OUT_JSON, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write('\n')
    # ConfigDrop: giữ dòng khối gạch (sửa id theo BreakableBlock.ObjectId = <tên asset section>_X<c>_<r>), thay dòng nến/chậu
    rows = list(csv.reader(io.open(DROP_CSV, encoding='utf-8')))
    head = rows[0]
    keep = []
    rename = {'EH_X63_7': 'EntranceHall_X63_7', 'CA_X16_10': 'CryptA_X16_10', 'CA_X16_9': 'CryptA_X16_9',
              'CB_X29_6': 'CryptB_X29_6', 'PB_X43_9': 'PhantomBat_X43_9'}
    for r in rows[1:]:
        # CVM2 10: giữ cả dòng Block 2+ (stage ≥ 4 — Tools/cvm2/gen_map2_objects.py ghi cuối file) ⇒ chạy lại script này không xoá chúng
        if r[2] == 'BreakableBlock' or int(r[1]) >= 4:
            r[0] = rename.get(r[0], r[0])
            keep.append(r)
    out = []
    stage = {'CastleEntrance': 1, 'EntranceHall': 1, 'CryptA': 2, 'CryptB': 2, 'PhantomBat': 3}
    for s in secs:
        for c in s['candles']:
            out.append([c['id'], str(stage[s['id']]), 'Torch' if c['kind'] == 'torch' else 'WallCandle', c['content'],
                        '%s %s x %d y %d' % ('chậu lửa' if c['kind'] == 'torch' else 'nến', s['id'], c['x'], c['y']),
                        c['src'].split('nội dung: ')[1]])
    with io.open(DROP_CSV, 'w', encoding='utf-8', newline='') as f:
        w = csv.writer(f, lineterminator='\n')
        w.writerow(head)
        w.writerows(out)
        w.writerows(keep)
    print('ghi', OUT_JSON, sum(len(s['candles']) for s in secs), 'nen;', DROP_CSV, len(out) + len(keep), 'dong')


if __name__ == '__main__':
    main()
