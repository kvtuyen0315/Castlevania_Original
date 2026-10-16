# tracks2.py (CVM2 02) - nối blob tiền cảnh (objects2.py) thành "vết" (track) theo toạ độ thế giới -> work/map2_tracks.csv + map2_track_pts.csv
# Chạy: python -P -E Tools/cvm2/tracks2.py map2
# Loại blob của Simon (cam/kem) và blob chạm bbox Simon (roi/quái dính). Một vết = chuỗi blob cùng vật thể; static = đứng yên ≥ 30 frame (nến/đồ nền).
# tracks.csv: id,section,f0,f1,nfr,wx0,wy0,wx1,wy1,w,h,n_mean,static,or,cr,red,pur,blu,wht  (wx = toạ độ vùng, tâm bbox; wy = y màn NES tâm bbox)
import sys, os, csv
import numpy as np
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from fg2 import WORK, load_camera
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'
cam = load_camera(video)
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_simon.csv')))}
B = {}
for r in csv.DictReader(open(os.path.join(WORK, video + '_blobs.csv'))):
    B.setdefault(int(r['frame']), []).append(r)
JOINT = ('HallA>TEB', 'HallC>HallB')
def base(f):
    c = cam[f]
    return int(c['cxw']) if c['section'] in JOINT else int(c['cx'])
def is_simon_blob(r):
    return int(r['or']) >= 4 and int(r['cr']) >= 3
tracks = []; active = []
nid = 0
MAXGAP = 6
for f in sorted(B):
    c = cam[f]
    if c['kind'] != 'game': continue
    sec = c['section']; bs = base(f)
    s = S.get(f); sb = None
    if s and s['found'] == '1':
        px = int(s['px']); Y = int(s['Y']); sb = (px - 14, Y - 34, px + 14, Y + 2)
    obs = []
    for r in B[f]:
        n = int(r['n'])
        if n < 5 or is_simon_blob(r): continue
        x0, y0, x1, y1 = int(r['x0']), int(r['y0']), int(r['x1']), int(r['y1'])
        if sb and x0 < sb[2] and x1 > sb[0] and y0 < sb[3] and y1 > sb[1] and (x1 - x0 > 20 or y1 - y0 > 40): pass
        obs.append(dict(f=f, sec=sec, wx=bs + (x0 + x1) / 2.0, wy=(y0 + y1) / 2.0, w=x1 - x0 + 1, h=y1 - y0 + 1, n=n, x0=x0, y0=y0, x1=x1, y1=y1,
                        c=[int(r[k]) for k in ('or', 'cr', 'red', 'pur', 'blu', 'wht')]))
    # drop tracks gone / section changed
    for t in active[:]:
        if t['sec'] != sec or f - t['last']['f'] > MAXGAP:
            active.remove(t)
    pairs = []
    for ti, t in enumerate(active):
        l = t['last']; gap = f - l['f']
        for oi, o in enumerate(obs):
            d = np.hypot(o['wx'] - l['wx'], o['wy'] - l['wy'])
            lim = 8 + 5 * gap + 0.15 * max(l['w'], o['w'])
            if d <= lim:
                sz = abs(np.log((o['n'] + 3.0) / (l['n'] + 3.0)))
                pairs.append((d + 3 * sz, ti, oi))
    pairs.sort()
    ut, uo = set(), set()
    for _, ti, oi in pairs:
        if ti in ut or oi in uo: continue
        ut.add(ti); uo.add(oi)
        active[ti]['pts'].append(obs[oi]); active[ti]['last'] = obs[oi]
    for oi, o in enumerate(obs):
        if oi in uo: continue
        nid += 1
        t = dict(id=nid, sec=sec, pts=[o], last=o)
        active.append(t); tracks.append(t)
rows = []; prow = []
for t in tracks:
    p = t['pts']
    if len(p) < 2: continue
    xs = np.array([q['wx'] for q in p]); ys = np.array([q['wy'] for q in p])
    static = int(len(p) >= 30 and np.ptp(xs) < 3 and np.ptp(ys) < 3)
    cs = np.mean([q['c'] for q in p], axis=0)
    rows.append([t['id'], t['sec'], p[0]['f'], p[-1]['f'], len(p), '%.1f' % xs[0], '%.1f' % ys[0], '%.1f' % xs[-1], '%.1f' % ys[-1],
                 int(np.median([q['w'] for q in p])), int(np.median([q['h'] for q in p])), '%.0f' % np.mean([q['n'] for q in p]), static] + ['%.0f' % v for v in cs])
    for q in p:
        prow.append([t['id'], q['f'], '%.1f' % q['wx'], '%.1f' % q['wy'], q['w'], q['h'], q['n'], q['x0'], q['y0'], q['x1'], q['y1']])
with open(os.path.join(WORK, video + '_tracks.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['id', 'section', 'f0', 'f1', 'nfr', 'wx0', 'wy0', 'wx1', 'wy1', 'w', 'h', 'n_mean', 'static', 'or', 'cr', 'red', 'pur', 'blu', 'wht']); w.writerows(rows)
with open(os.path.join(WORK, video + '_track_pts.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['id', 'f', 'wx', 'wy', 'w', 'h', 'n', 'x0', 'y0', 'x1', 'y1']); w.writerows(prow)
print('tracks', len(rows), 'static', sum(r[12] for r in rows))
