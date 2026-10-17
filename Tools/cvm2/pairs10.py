"""CVM2 10 — cặp ảnh vật thể video Map_2 ↔ Unity cùng cx (lớp <Section>_Objects, Map2ObjectsProbe chụp Game view 1920×1080).

  python -P -E Tools/cvm2/pairs10.py select [n_per_section=4]
      chọn frame video camera ĐỨNG (cx f−1 = f = f+1, không lặp) phủ nhiều nến nhất (props_det.csv của props10.py bước 1) + vài frame
      có dơi treo / Bone Tower / Spear Knight ⇒ in chuỗi shotSpec "Section:cx:p_<Section>_f<frame>;…" cho Map2ObjectsProbe.Start
      + ghi ClaudeNotes/cvm2/_cache/10/shots.csv
  python -P -E Tools/cvm2/pairs10.py compare <thư mục ảnh Unity> [ảnh_ghép_ra=pairs]
      mỗi ảnh Unity p_<Section>_f<frame>.png (+ crown_rest/chest_rest ↔ frame cố định): lấy lưới NES (Game view 1920×1080: x = 448 + 4·nx,
      y = 60 + 4·ny, lấy tâm ô 4×4), khớp mẫu sprite cả 2 phía (Unity: màu sheet; video: bảng màu video ước lượng), ghép cặp phát hiện
      cùng loại gần nhất ⇒ dx, dy (px NES); ghi <dir>/pairs.txt + ảnh ghép <dir>/<ra>_<k>.png (trái video, phải Unity, ×2, khung đỏ = vật).
"""
import csv, json, os, sys
import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import enemy8_fit as ef  # noqa: E402

C10 = os.path.join(ef.ROOT, 'ClaudeNotes', 'cvm2', '_cache', '10')
OBJ = {  # loại: (sprite, errmax video, errmax unity)
    'candle': (['Object/candle_0', 'Object/candle_1'], 0.09, 0.05),
    'bat': (['Enemy/VampireBat/bat_purple_0'], 0.12, 0.05),
    'tower': (['Enemy/DragonSkullCannon/dragonskull_purple_top', 'Enemy/DragonSkullCannon/dragonskull_red_top'], 0.14, 0.05),
    'knight': (['Enemy/BlackKnight/blackknight_0', 'Enemy/BlackKnight/blackknight_1', 'Enemy/BlackKnight/blackknight_2'], 0.12, 0.05),
    'crown': (['Item/crown_0', 'Item/crown_1', 'Item/crown_2', 'Item/crown_3'], 0.12, 0.05),
    'chest': (['Item/chest_0', 'Item/chest_1', 'Item/chest_2', 'Item/chest_3'], 0.12, 0.05),
}
FIXED = {'crown_rest': ('TowerEntranceA', 560, ['crown']), 'chest_rest': ('TowerHallA', 2420, ['chest'])}


def cam():
    return {int(r['frame']): (r['kind'], r['section'], int(float(r['cx'])))
            for r in csv.DictReader(open(os.path.join(ef.ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')))}


def select(nper):
    c = cam()
    dup = np.load(os.path.join(ef.WORK, 'map2_dup.npy'))
    det = {}
    for r in csv.DictReader(open(os.path.join(C10, 'props_det.csv'))):
        det.setdefault(int(r['frame']), []).append((r['section'], int(r['x'])))
    rows = []
    for sec in ['TowerEntranceA', 'TowerEntranceB', 'TowerHallA', 'TowerHallB', 'TowerHallC', 'Medusa']:
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
    extra = [('TowerEntranceA', 187), ('TowerHallC', 4985), ('TowerHallC', 4945), ('TowerEntranceB', 900)]
    for sec, f in extra:
        rows.append((sec, c[f][2], f, -1))
    with open(os.path.join(C10, 'shots.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'cx', 'frame', 'n_candles'])
        w.writerows(rows)
    spec = ';'.join('%s:%d:p_%s_f%d' % (s, cx, s, f) for s, cx, f, n in rows)
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


def detect(nes, kind, errmax, pal_mode, cache):
    names = OBJ[kind][0]
    if kind not in cache:
        sp, cols = ef.load(names)
        cache[kind] = (sp, cols)
    sp, cols = cache[kind]
    if pal_mode == 'sheet':
        pal = [tuple(cc) for cc in cols]
    else:
        pal = pal_mode[kind]
    box = [0, 48, 256, 224]
    sep = (max(s['w'] for s in sp) * 3 // 4, max(s['h'] for s in sp) * 3 // 4)
    out = []
    for e, s, x, y, px, py in ef.detect(ef.err_maps(nes, sp, pal, box), box, errmax, sep):
        out.append((kind, px, py, e, x, y, s['w'], s['h']))
    return out


def video_pals(mm, cache):
    """bảng màu video mỗi loại: ước lượng trên các frame có vật đó (CVM2 08/10 đã chạy) — fallback màu sheet ×0.85."""
    pals = {}
    known = {'candle': 'pal_props.json', 'crown': 'pal_crown10.json', 'chest': 'pal_chest10.json'}
    for kind in OBJ:
        sp, cols = ef.load(OBJ[kind][0])
        cache[kind] = (sp, cols)
        p = known.get(kind)
        d = json.load(open(os.path.join(C10, p))) if p and os.path.exists(os.path.join(C10, p)) else {}
        if kind == 'bat' and os.path.exists(os.path.join(ef.OUT, 'pal_bat_tea.json')):
            d = json.load(open(os.path.join(ef.OUT, 'pal_bat_tea.json')))
        if kind == 'tower' and os.path.exists(os.path.join(ef.OUT, 'pal_bt_tw.json')):
            d = json.load(open(os.path.join(ef.OUT, 'pal_bt_tw.json')))
        if kind == 'knight' and os.path.exists(os.path.join(ef.OUT, 'pal_sk_tea.json')):
            d = json.load(open(os.path.join(ef.OUT, 'pal_sk_tea.json')))
        pals[kind] = [tuple(d[','.join(map(str, cc))]) if ','.join(map(str, cc)) in d else
                      (tuple(cc) if sum(cc) == 0 else tuple(int(v * 0.85) for v in cc)) for cc in cols]
    return pals


def compare(udir, outname):
    c = cam()
    mm = np.load(os.path.join(ef.WORK, 'map2_nes.npy'), mmap_mode='r')
    cache = {}
    pals = video_pals(mm, cache)
    lines, tiles = [], []
    allres = []
    files = sorted(f for f in os.listdir(udir) if f.endswith('.png') and (f.startswith('p_') or f[:-4] in FIXED))
    for fn in files:
        stem = fn[:-4]
        if stem in FIXED:
            sec, f, kinds = FIXED[stem]
        else:
            parts = stem.split('_')
            sec, f = '_'.join(parts[1:-1]), int(parts[-1][1:])
            kinds = list(OBJ.keys())
            kinds.remove('crown'); kinds.remove('chest')
        cx = c[f][2]
        vn = np.asarray(mm[f])
        un = unity_grid(os.path.join(udir, fn))
        res = []
        boxes_v, boxes_u, pv, pu = [], [], [], []
        for kind in kinds:
            dv = detect(vn, kind, OBJ[kind][1], pals, cache)
            du = detect(un, kind, OBJ[kind][2], 'sheet', cache)
            boxes_v += dv
            boxes_u += du
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
                    res.append((kind, cx + v[1], v[2] - 32, u[1] - v[1], u[2] - v[2], v[3], u[3]))
                    pv.append(v)
                    pu.append(u)
        allres += [(fn,) + r for r in res]
        lines.append('%s  section %s frame f%d cx %d: %d cặp — %s' % (
            fn, sec, f, cx, len(res), '; '.join('%s x%d y%d dx %+d dy %+d' % (r[0], r[1], r[2], r[3], r[4]) for r in res)))
        # ảnh ghép
        a = Image.fromarray(vn.astype(np.uint8)).resize((512, 448), Image.NEAREST)
        b = Image.fromarray(un.astype(np.uint8)).resize((512, 448), Image.NEAREST)
        for img, bx in ((a, pv), (b, pu)):   # chỉ vẽ phát hiện đã ghép cặp (xanh lá)
            d = ImageDraw.Draw(img)
            for k, px, py, e, x, y, w, h in bx:
                d.rectangle([x * 2, (y - 8) * 2, (x + w) * 2 - 1, (y - 8 + h) * 2 - 1], outline=(0, 255, 0))
        t = Image.new('RGB', (1030, 470), (30, 30, 30))
        t.paste(a, (0, 22)); t.paste(b, (518, 22))
        ImageDraw.Draw(t).text((4, 4), 'video f%d (%s cx %d)  |  Unity %s — %d cap' % (f, sec, cx, stem, len(res)), fill=(255, 255, 0))
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
