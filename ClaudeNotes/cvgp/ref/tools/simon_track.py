# simon_track.py - dinh vi sprite Simon tung frame bang mau (cam f39610 + kem fbdca0) tren luoi NES
# Chay: python -P -E tools/simon_track.py map1   -> work/map1_simon.csv
# Cot: frame,t,found,x_med,x0,x1,y0,y1,npx,n_or,n_cr,section,cx,world_x
#   x_med = trung vi cot cac pixel cam/kem cua cum lon nhat (than Simon, it bi roi lam lech)
#   x0..x1,y0..y1 = bbox cum (khong gom vien den) ; toa do NES (y tinh tu dinh man NES)
#   world_x = cx (camera, tu camera_track.py) + x_med  (toa do pixel trong vung ban do)
import sys, os, csv
import numpy as np
from collections import deque
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
N = int(np.load(os.path.join(base, f'{video}_meta.npz'))['n'])
Y0 = 8
YTOP = 48   # chi xet playfield (duoi HUD)
cam = {}
p = os.path.join(base, f'{video}_camera.csv')
if os.path.exists(p):
    cam = {int(r['frame']): r for r in csv.DictReader(open(p))}

def masks(f):
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    orange = (r > 200) & (g > 110) & (g < 185) & (b < 90)
    cream = (r > 215) & (g > 185) & (b > 110) & (b < 205) & ((r - b) > 40)
    return orange, cream

def components(m):
    H, W = m.shape
    lab = np.zeros(m.shape, int); comps = []
    ys, xs = np.nonzero(m)
    for y, x in zip(ys, xs):
        if lab[y, x]: continue
        k = len(comps) + 1; q = deque([(y, x)]); lab[y, x] = k; pts = []
        while q:
            cy, cx_ = q.popleft(); pts.append((cy, cx_))
            for dy in (-2, -1, 0, 1, 2):          # lan can 5x5 (noi qua vien den 1-2 px)
                for dx in (-2, -1, 0, 1, 2):
                    ny, nx = cy + dy, cx_ + dx
                    if 0 <= ny < H and 0 <= nx < W and m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = k; q.append((ny, nx))
        comps.append(np.array(pts))
    return comps

rows = []
for i in range(N):
    f = np.asarray(mm[i][YTOP - Y0:]).astype(int)
    o, c = masks(f)
    m = o | c
    rec = [i, f'{i/30:.3f}', 0, '', '', '', '', '', 0, 0, 0, '', '', '']
    if m.sum() >= 15 and m.sum() < 2500:
        comps = components(m)
        best = None
        for pts in comps:
            h = pts[:, 0].max() - pts[:, 0].min() + 1
            w = pts[:, 1].max() - pts[:, 1].min() + 1
            no = int(o[pts[:, 0], pts[:, 1]].sum()); nc = int(c[pts[:, 0], pts[:, 1]].sum())
            # Simon: cao 12..34, rong <= 40, co ca cam va kem
            if 10 <= h <= 34 and w <= 48 and no >= 4 and nc >= 4:
                sc = len(pts)
                if best is None or sc > best[0]:
                    best = (sc, pts, no, nc)
        if best is not None:
            sc, pts, no, nc = best
            ys, xs = pts[:, 0] + YTOP, pts[:, 1]
            rec[2] = 1
            rec[3] = f'{np.median(xs):.1f}'
            rec[4], rec[5], rec[6], rec[7] = int(xs.min()), int(xs.max()), int(ys.min()), int(ys.max())
            rec[8], rec[9], rec[10] = int(sc), no, nc
    cr = cam.get(i)
    if cr is not None and cr['section'] != '-':
        rec[11] = cr['section']; rec[12] = cr['cx']
        if rec[2]:
            rec[13] = f"{int(cr['cx']) + float(rec[3]):.1f}"
    rows.append(rec)
    if i % 1000 == 0: print(i, rec, flush=True)
with open(os.path.join(base, f'{video}_simon.csv'), 'w', newline='') as fh:
    w = csv.writer(fh)
    w.writerow(['frame', 't', 'found', 'x_med', 'x0', 'x1', 'y0', 'y1', 'npx', 'n_or', 'n_cr', 'section', 'cx', 'world_x'])
    w.writerows(rows)
print('done', len(rows))
