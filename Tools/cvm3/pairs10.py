"""CVM3 10 — cặp ảnh vật thể video Map_3 ↔ Unity cùng cx (lớp <Section>_Objects, Map3ObjectsProbe chụp Game view 1920×1080).
Bản Map_3 của Tools/cvm2/pairs10.py (CVM2 10).

  python -P -E Tools/cvm3/pairs10.py select [n_per_section=4]
      chọn frame video camera ĐỨNG (cx f−1 = f = f+1, không lặp) phủ nhiều nến nhất (_cache/10/det_cd_all.csv của fit10.py) + frame
      enemy ở điểm sinh (Raven đậu, Fleaman chờ, Skeleton vừa hiện, Bone Tower, 2 Mummy chờ) ⇒ in chuỗi shotSpec "Section:cxUnity:p_<Section>_f<frame>;…"
      cho Map3ObjectsProbe.Start + ghi ClaudeNotes/cvm3/_cache/10/shots.csv (cx Unity = cx video + 112 ở WalkwayB)
  python -P -E Tools/cvm3/pairs10.py compare <thư mục ảnh Unity> [ảnh_ghép_ra=pairs]
      mỗi ảnh Unity p_<Section>_f<frame>.png (+ bag7_rest / bag8_rest ↔ frame cố định): lấy lưới NES (Game view 1920×1080: x = 448 + 4·nx,
      y = 60 + 4·ny, tâm ô 4×4), khớp mẫu sprite cả 2 phía (Unity: màu sheet; video: bảng màu video Tools/cvm3/*_pal_map3.json — chỉ để dò),
      ghép cặp phát hiện cùng loại gần nhất ⇒ dx, dy (px NES); ghi <dir>/pairs.txt + ảnh ghép <dir>/<ra>_<k>.png (trái video, phải Unity, ×2).
"""
import csv, json, os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
import enemy8_fit as ef  # noqa: E402

C10 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '10')
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
T3 = os.path.join(ROOT, 'Tools', 'cvm3')
PAD = {'WalkwayB': 112}
SECTIONS = ['WalkwayEntranceA', 'WalkwayEntranceB', 'WalkwayA', 'WalkwayB', 'MummyMan']
OBJ = {  # loại: (sprite, errmax video, errmax unity, bảng màu video)
    'candle': (['Object/candle_0', 'Object/candle_1'], 0.09, 0.05, 'candle10_pal_map3.json'),
    'raven': (['Enemy/Raven/raven_0'], 0.10, 0.05, 'enemy8b_pal_raven_map3.json'),
    'tower': (['Enemy/DragonSkullCannon/dragonskull_purple_top', 'Enemy/DragonSkullCannon/dragonskull_red_top'], 0.12, 0.05, 'enemy8b_pal_tower_map3.json'),
    'skeleton': (['Enemy/WhiteSkeleton/skeleton_0', 'Enemy/WhiteSkeleton/skeleton_1'], 0.12, 0.05, 'enemy7_pal_skel.json'),
    'fleaman': (['Enemy/Fleaman/fleaman_0', 'Enemy/Fleaman/fleaman_1'], 0.12, 0.05, 'enemy7_pal_flea_map3.json'),
    'mummy': (['Boss/Mummy/mummy_purple_0'], 0.12, 0.05, 'mummy10_pal_map3.json'),
    'bag': (['Item/moneybag_red', 'Item/moneybag_purple', 'Item/moneybag_white'], 0.10, 0.05, 'bag10_pal_map3.json'),
}
# ảnh Unity cố định ↔ frame video (túi bí mật đã nghỉ): St7 f1990 cx 380, St8 f3445 cx −35 (Unity 77)
FIXED = {'bag7_rest': ('WalkwayEntranceB', 1990, ['bag', 'candle']), 'bag8_rest': ('WalkwayB', 3445, ['bag', 'candle'])}
# frame enemy ở điểm sinh (CVM3 07/08/09 vết): Fleaman chờ, Skeleton vừa hiện ở mép, Raven đậu (+8 frame sau khi hiện), Bone Tower, Mummy chờ
EXTRA = [('WalkwayEntranceA', 491), ('WalkwayEntranceA', 621), ('WalkwayEntranceA', 825), ('WalkwayEntranceA', 1006),
         ('WalkwayEntranceB', 1348), ('WalkwayEntranceB', 1517), ('WalkwayEntranceB', 1582), ('WalkwayEntranceB', 1642), ('WalkwayEntranceB', 1662),
         ('WalkwayEntranceB', 2123), ('WalkwayA', 2986), ('WalkwayB', 3602), ('WalkwayB', 3700), ('WalkwayB', 3873), ('WalkwayB', 3906),
         ('WalkwayB', 3995), ('MummyMan', 4676), ('MummyMan', 4723), ('MummyMan', 4760), ('MummyMan', 4880), ('MummyMan', 4912),
         ('MummyMan', 4975), ('MummyMan', 5094), ('MummyMan', 5121), ('MummyMan', 5184), ('MummyMan', 5400), ('MummyMan', 5900)]


def cam():
    return {int(r['frame']): (r['kind'], r['section'], int(float(r['cx'])))
            for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv')))}


def select(nper):
    c = cam()
    dup = np.load(os.path.join(WORK, 'map3_dup.npy'))
    det = {}
    for r in csv.DictReader(open(os.path.join(C10, 'det_cd_all.csv'))):
        if float(r['err']) <= 0.06:
            det.setdefault(int(r['frame']), []).append((r['section'], int(r['worldx'])))
    rows = []
    for sec in SECTIONS:
        cand = []
        for f, d in det.items():
            k = c.get(f)
            if not k or k[1] != sec or dup[f] or c.get(f - 1, (0, 0, -9))[2] != k[2] or c.get(f + 1, (0, 0, -9))[2] != k[2]:
                continue
            cand.append((f, k[2], set(x for s, x in d if s == sec)))
        covered = set()
        for _ in range(nper):
            best = max(cand, key=lambda t: (len(t[2] - covered), len(t[2]), -t[0]), default=None)
            if best is None or not (best[2] - covered):
                break
            covered |= best[2]
            rows.append((sec, best[1], best[0], len(best[2])))
            cand = [t for t in cand if abs(t[0] - best[0]) > 30]
    for sec, f in EXTRA:
        assert c[f][1] == sec, (sec, f, c[f])
        rows.append((sec, c[f][2], f, -1))
    with open(os.path.join(C10, 'shots.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'cx_video', 'cx_unity', 'frame', 'n_candles'])
        for s, cx, f, n in rows:
            w.writerow([s, cx, cx + PAD.get(s, 0), f, n])
    spec = ';'.join('%s:%d:p_%s_f%d' % (s, cx + PAD.get(s, 0), s, f) for s, cx, f, n in rows)
    print(len(rows), 'anh')
    print(spec)


def unity_grid(path):
    im = np.asarray(Image.open(path).convert('RGB')).astype(np.uint8)
    if im.shape[1] != 1920 or im.shape[0] != 1080:
        raise SystemExit('anh Unity khong phai 1920x1080: %s %s' % (path, im.shape))
    ys = 60 + 4 * np.arange(240) + 2
    xs = 448 + 4 * np.arange(256) + 2
    g = im[ys][:, xs]
    return g[8:232]          # cùng khung bộ đệm video (NES y 8..231)


def detect(nes, kind, errmax, pal, cache):
    sp, cols = cache[kind]
    box = [0, 48, 256, 224]
    sep = (max(s['w'] for s in sp) * 3 // 4, max(s['h'] for s in sp) * 3 // 4)
    out = []
    for e, s, x, y, px, py in ef.detect(ef.err_maps(nes, sp, pal, box), box, errmax, sep):
        out.append((kind, px, py, e, x, y, s['w'], s['h']))
    return out


def compare(udir, outname):
    c = cam()
    mm = np.load(os.path.join(WORK, 'map3_nes.npy'), mmap_mode='r')
    cache, pals = {}, {}
    for kind, (names, ev, eu, pf) in OBJ.items():
        sp, cols = ef.load(names)
        cache[kind] = (sp, cols)
        d = json.load(open(os.path.join(T3, pf)))
        pals[kind] = [tuple(d[','.join(map(str, cc))]) if ','.join(map(str, cc)) in d else
                      (tuple(cc) if sum(cc) == 0 else tuple(int(v * 0.85) for v in cc)) for cc in cols]
    lines, tiles, allres = [], [], []
    files = sorted(f for f in os.listdir(udir) if f.endswith('.png') and (f.startswith('p_') or f[:-4] in FIXED))
    for fn in files:
        stem = fn[:-4]
        if stem in FIXED:
            sec, f, kinds = FIXED[stem]
        else:
            parts = stem.split('_')
            sec, f = '_'.join(parts[1:-1]), int(parts[-1][1:])
            kinds = [k for k in OBJ if k != 'bag']
        cx = c[f][2]
        cxu = cx + PAD.get(sec, 0)
        vn = np.asarray(mm[f])
        un = unity_grid(os.path.join(udir, fn))
        res, pv, pu = [], [], []
        for kind in kinds:
            dv = detect(vn, kind, OBJ[kind][1], pals[kind], cache)
            du = detect(un, kind, OBJ[kind][2], [tuple(cc) for cc in cache[kind][1]], cache)
            used = set()
            for v in dv:
                best = None
                for j, u in enumerate(du):
                    if j in used:
                        continue
                    d = abs(u[1] - v[1]) + abs(u[2] - v[2])
                    if d <= 8 and (best is None or d < best[0]):
                        best = (d, j, u)
                if best:
                    used.add(best[1])
                    u = best[2]
                    res.append((kind, cxu + v[1], v[2] - 32, u[1] - v[1], u[2] - v[2], v[3], u[3]))
                    pv.append(v)
                    pu.append(u)
        allres += [(fn,) + r for r in res]
        lines.append('%s  section %s frame f%d cx video %d (Unity %d): %d cặp — %s' % (
            fn, sec, f, cx, cxu, len(res), '; '.join('%s x%d y%d dx %+d dy %+d' % (r[0], r[1], r[2], r[3], r[4]) for r in res)))
        a = Image.fromarray(vn.astype(np.uint8)).resize((512, 448), Image.NEAREST)
        b = Image.fromarray(un.astype(np.uint8)).resize((512, 448), Image.NEAREST)
        for img, bx in ((a, pv), (b, pu)):   # chỉ vẽ phát hiện đã ghép cặp (xanh lá)
            d = ImageDraw.Draw(img)
            for k, px, py, e, x, y, w, h in bx:
                d.rectangle([x * 2, (y - 8) * 2, (x + w) * 2 - 1, (y - 8 + h) * 2 - 1], outline=(0, 255, 0))
        t = Image.new('RGB', (1030, 470), (30, 30, 30))
        t.paste(a, (0, 22)); t.paste(b, (518, 22))
        ImageDraw.Draw(t).text((4, 4), 'video f%d (%s cx %d)  |  Unity %s cx %d — %d cap' % (f, sec, cx, stem, cxu, len(res)), fill=(255, 255, 0))
        tiles.append(t)
    with open(os.path.join(udir, 'pairs.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
        dx = [abs(r[4]) for r in allres]
        dy = [abs(r[5]) for r in allres]
        n01 = sum(1 for r in allres if abs(r[4]) <= 1 and abs(r[5]) <= 1)
        fo.write('TONG %d cap vat the / %d anh; |dx| max %d, |dy| max %d; trong 0-1 px: %d\n' % (
            len(allres), len(files), max(dx) if dx else -1, max(dy) if dy else -1, n01))
        for r in allres:
            fo.write('%s %s x %d y %d dx %+d dy %+d errV %.3f errU %.3f\n' % r)
    for k in range(0, len(tiles), 4):
        grp = tiles[k:k + 4]
        sheet = Image.new('RGB', (1030 * 2, 470 * ((len(grp) + 1) // 2)), (0, 0, 0))
        for i, t in enumerate(grp):
            sheet.paste(t, ((i % 2) * 1030, (i // 2) * 470))
        sheet.save(os.path.join(udir, '%s_%d.png' % (outname, k // 4)))
    print(open(os.path.join(udir, 'pairs.txt'), encoding='utf-8').read().splitlines()[-len(allres) - 1])


if __name__ == '__main__':
    if sys.argv[1] == 'select':
        select(int(sys.argv[2]) if len(sys.argv) > 2 else 4)
    else:
        compare(sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else 'pairs')
