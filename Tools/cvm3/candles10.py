"""CVM3 10 — gom cụm phát hiện nến (fit10.py cd_all) thành bảng vị trí nến Block 3 (MAP3_PLACEMENT §1), khung hiện vật theo x màn,
frame vỡ + vật rơi quanh nến (khớp mẫu item) + sự kiện HUD. Khuôn Tools/cvm2/props10.py + window10.py + drops10.py (CVM2 10).

Chạy (từ gốc repo):
  python -P -E Tools/cvm3/candles10.py cluster [errmax=0.06]   → _cache/10/candles_clusters.csv (section, x = mốt khung camera đứng,
        y đáy section, số lần thấy, frame đầu/cuối, err TB, khoảng x màn) — x theo gốc SHEET; WalkwayB Unity = +112 (cột xU)
  python -P -E Tools/cvm3/candles10.py window                  → histogram x màn của nến chưa vỡ (khung hiện vật NES)
  python -P -E Tools/cvm3/candles10.py drops                   → _cache/10/drops.txt: frame vỡ, item khớp mẫu quanh nến 45 frame sau, HUD
Đầu vào: _cache/10/det_cd_all.csv; camera _cache/02/work/map3_camera.csv; dup map3_dup.npy; sự kiện ref/data/map3_events.txt.
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy8_fit as E8  # noqa: E402
import enemy7_fit as E7  # noqa: E402

OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '10')
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
REF = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
SECTIONS = ['WalkwayEntranceA', 'WalkwayEntranceB', 'WalkwayA', 'WalkwayB', 'MummyMan']
PADX = {'WalkwayB': 112}   # CVM3 05: WalkwayB nới trái 112 px ⇒ x Unity = x sheet + 112
ITEMS = ['Item/heart_small', 'Item/heart_large', 'Item/moneybag_red', 'Item/moneybag_purple', 'Item/moneybag_white', 'Item/dagger',
         'Item/stopwatch', 'Item/cross', 'Item/axe', 'Item/holywater', 'Item/roast', 'Item/double', 'Item/triple', 'Item/rosary',
         'Item/whip', 'Item/1up']


def det(errmax):
    rows = []
    for r in csv.DictReader(open(os.path.join(OUT, 'det_cd_all.csv'))):
        if r['section'] not in SECTIONS or float(r['err']) > errmax:
            continue
        rows.append((r['section'], int(r['frame']), int(r['cx']), int(r['pivx']), int(r['worldx']), int(r['ysec']), float(r['err'])))
    return rows


def clusters(errmax):
    cam = E7.camera('map3')
    cl = []
    for sec, f, cx, px, x, y, e in det(errmax):
        for c in cl:
            if c['sec'] == sec and abs(c['x0'] - x) <= 3 and abs(c['y0'] - y) <= 3:
                c['r'].append((f, cx, px, x, y, e))
                break
        else:
            cl.append(dict(sec=sec, x0=x, y0=y, r=[(f, cx, px, x, y, e)]))
    out = []
    for c in cl:
        r = c['r']
        xs = np.array([v[3] for v in r])
        ys = np.array([v[4] for v in r])
        # khung camera đứng: cx(f−1) = cx(f) = cx(f+1)
        st = [v[3] for v in r if cam.get(v[0] - 1, (0, 0, -9))[2] == v[1] == cam.get(v[0] + 1, (0, 0, -9))[2]]
        vals, cnt = np.unique(xs, return_counts=True)
        mode_all = int(vals[np.argmax(cnt)])
        if st:
            v2, c2 = np.unique(st, return_counts=True)
            mode_st = int(v2[np.argmax(c2)])
        else:
            mode_st = mode_all
        hist = ' '.join('%d×%d' % (v, n) for v, n in zip(vals, cnt))
        out.append(dict(sec=c['sec'], x=mode_st, xall=mode_all, y=int(np.round(np.median(ys))), n=len(r), nst=len(st),
                        st_ok=sum(1 for v in st if v == mode_st), hist=hist, f0=min(v[0] for v in r), f1=max(v[0] for v in r),
                        err=float(np.mean([v[5] for v in r])), sx='%d-%d' % (min(v[2] for v in r), max(v[2] for v in r))))
    out.sort(key=lambda d: (SECTIONS.index(d['sec']), d['x'], d['y']))
    return out


def cmd_cluster():
    errmax = float(sys.argv[2]) if len(sys.argv) > 2 else 0.06
    out = clusters(errmax)
    with open(os.path.join(OUT, 'candles_clusters.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'x', 'xU', 'y', 'n', 'n_stand', 'stand_at_mode', 'x_hist', 'frame_first', 'frame_last', 'err_mean', 'screen_x'])
        for d in out:
            w.writerow([d['sec'], d['x'], d['x'] + PADX.get(d['sec'], 0), d['y'], d['n'], d['nst'], d['st_ok'], d['hist'], d['f0'], d['f1'],
                        '%.4f' % d['err'], d['sx']])
    for d in out:
        print('%-17s x %4d (U %4d) y %3d n %4d stand %3d/%3d  f%d-f%d err %.3f sx %s  [%s]' % (
            d['sec'], d['x'], d['x'] + PADX.get(d['sec'], 0), d['y'], d['n'], d['st_ok'], d['nst'], d['f0'], d['f1'], d['err'], d['sx'], d['hist']))
    print('cum', len(out))


def final():
    rows = []
    p = os.path.join(OUT, 'candles_final.csv')
    for r in csv.DictReader(open(p)):
        rows.append((r['section'], int(r['x']), int(r['y'])))
    return rows


def cmd_window():
    cam = E7.camera('map3')
    dup = np.load(os.path.join(WORK, 'map3_dup.npy'))
    dets = {}
    for sec, f, cx, px, x, y, e in det(0.06):
        dets.setdefault((sec, f), []).append((x, y))
    seen = np.zeros(256, int)
    tot = np.zeros(256, int)
    for sec, x, y in final():
        fs = sorted(f for (s, f), v in dets.items() if s == sec and any(abs(a - x) <= 2 and abs(b - y) <= 2 for a, b in v))
        if not fs:
            continue
        flast = fs[-1]
        for f in range(207, flast + 1):
            c = cam.get(f)
            if c is None or c[0] != 'game' or c[1] != sec or dup[f]:
                continue
            sx = x - c[2]
            if 0 <= sx < 256:
                tot[sx] += 1
                if any(abs(a - x) <= 2 and abs(b - y) <= 2 for a, b in dets.get((sec, f), [])):
                    seen[sx] += 1
    for a, b in [(0, 8), (8, 16), (16, 240), (240, 248), (248, 256)]:
        print('x man %3d-%3d: thay %5d / %5d' % (a, b - 1, seen[a:b].sum(), tot[a:b].sum()))
    print('tung cot:', ' '.join('%d:%d/%d' % (i, seen[i], tot[i]) for i in list(range(0, 12)) + list(range(244, 256))))


def cmd_drops():
    cam = E7.camera('map3')
    dup = np.load(os.path.join(WORK, 'map3_dup.npy'))
    mm = np.load(os.path.join(WORK, 'map3_nes.npy'), mmap_mode='r')
    dets = {}
    for sec, f, cx, px, x, y, e in det(0.06):
        dets.setdefault(sec, []).append((f, x, y))
    ev = [(int(l.split()[0][1:]), l.rstrip('\n')) for l in open(os.path.join(REF, 'map3_events.txt'), encoding='utf-8') if l.startswith('f')]
    sprites = {}
    for n in ITEMS:
        sprites[n] = E8.load([n])
    lines = []
    for sec, x, y in final():
        fs = sorted(f for f, xx, yy in dets.get(sec, []) if abs(xx - x) <= 2 and abs(yy - y) <= 2)
        if not fs:
            lines.append('%s x %d y %d: KHÔNG thấy' % (sec, x, y))
            continue
        f_last = fs[-1]
        gone = None
        k = 0
        for f in range(f_last + 1, f_last + 40):
            c = cam.get(f)
            if c is None or c[0] != 'game' or c[1] != sec or dup[f]:
                continue
            sx = x - c[2]
            if 8 <= sx <= 247:
                k += 1
                if k >= 3:
                    gone = f
                    break
            else:
                break
        s = '%s x %d y %d: thấy f%d–f%d (%d lần)' % (sec, x, y, fs[0], f_last, len(fs))
        if gone is None:
            lines.append(s + ' — rời khung (không phá trong video)')
            continue
        s += ' — VỠ ≈ f%d (%d:%05.2f)' % (f_last + 1, (f_last + 1) // 1800, (f_last + 1) / 30 % 60)
        best = []
        for n in ITEMS:
            sp, cols = sprites[n]
            palv = [tuple(c) if sum(c) == 0 else tuple(int(v * 0.85) for v in c) for c in cols]
            for f in range(f_last + 1, f_last + 46):
                c = cam.get(f)
                if c is None or c[0] != 'game' or c[1] != sec or dup[f]:
                    continue
                sx = x - c[2]
                box = [max(0, sx - 24), max(48, y + 32 - 24), min(256, sx + 24), 224]
                if box[2] - box[0] < 18:
                    continue
                for e, spr, xx, yy, px, py in E8.detect(E8.err_maps(np.asarray(mm[f]), sp, palv, box), box, 0.2, (8, 8))[:1]:
                    best.append((e, f, n.split('/')[-1], c[2] + px, py - 32))
        best.sort()
        tops = []
        for b in best:
            if all(b[2] != t[2] for t in tops):
                tops.append(b)
            if len(tops) >= 3:
                break
        s += '\n    item: ' + '; '.join('%s f%d (%d,%d) err %.3f' % (t[2], t[1], t[3], t[4], t[0]) for t in tops)
        near = [l for f, l in ev if f_last <= f <= f_last + 150]
        s += '\n    HUD: ' + (' || '.join(' '.join(l.split('|')[i].strip() for i in (0, 5, 6)) for l in near) if near else '-')
        lines.append(s)
    open(os.path.join(OUT, 'drops.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    print('ghi drops.txt', len(lines))


if __name__ == '__main__':
    {'cluster': cmd_cluster, 'window': cmd_window, 'drops': cmd_drops}[sys.argv[1]]()
