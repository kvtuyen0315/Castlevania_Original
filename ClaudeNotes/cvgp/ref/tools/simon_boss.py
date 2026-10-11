# simon_boss.py - dinh vi Simon trong video boss theo bang mau rieng cua video nay
#   (nau dam ~ (60..115, 15..60, <35) + cam/da ~ (r>175, 110..205, <175)); doi do tuoi (r>150,g<90) bi loai
# Chay: python -P -E tools/simon_boss.py <f0> <f1> [out.csv]
import sys, os, csv
import numpy as np
from collections import deque
f0, f1 = int(sys.argv[1]), int(sys.argv[2])
out = sys.argv[3] if len(sys.argv) > 3 else None
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, 'boss_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(base, 'boss_dup.npy'))
Y0, YA = 8, 48
def comps(m):
    H, W = m.shape; lab = np.zeros(m.shape, np.int32); res = []
    for y, x in zip(*np.nonzero(m)):
        if lab[y, x]: continue
        k = len(res) + 1; q = deque([(y, x)]); lab[y, x] = k; pts = []
        while q:
            cy, cx = q.popleft(); pts.append((cy, cx))
            for dy in (-2, -1, 0, 1, 2):
                for dx in (-2, -1, 0, 1, 2):
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < H and 0 <= nx < W and m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = k; q.append((ny, nx))
        res.append(np.array(pts))
    return res
rows = []
for i in range(f0, f1 + 1):
    f = np.asarray(mm[i][YA - Y0:224 - Y0]).astype(int)
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    brown = (r >= 60) & (r <= 115) & (g >= 15) & (g <= 60) & (b < 35)
    light = (r > 175) & (g >= 110) & (g <= 205) & (b < 175) & (r - b > 40)
    m = brown | light
    best = None
    for p in comps(m):
        h = p[:, 0].max() - p[:, 0].min() + 1; w = p[:, 1].max() - p[:, 1].min() + 1
        nb = int(brown[p[:, 0], p[:, 1]].sum()); nl = int(light[p[:, 0], p[:, 1]].sum())
        if 10 <= h <= 34 and w <= 48 and nb >= 10 and nl >= 3 and len(p) >= 40:
            if best is None or len(p) > best[0]: best = (len(p), p)
    if best:
        p = best[1]; ys = p[:, 0] + YA; xs = p[:, 1]
        rows.append((i, int(dup[i]), 1, f'{np.median(xs):.1f}', int(xs.min()), int(xs.max()), int(ys.min()), int(ys.max()), len(p)))
    else:
        rows.append((i, int(dup[i]), 0, '', '', '', '', '', 0))
if out:
    with open(out, 'w', newline='') as fh:
        w = csv.writer(fh); w.writerow(['frame', 'dup', 'found', 'x_med', 'x0', 'x1', 'y0', 'y1', 'npx']); w.writerows(rows)
    print('wrote', out, len(rows), 'found', sum(r_[2] for r_ in rows))
else:
    for r_ in rows:
        print(r_[0], f'{r_[0]/30:.2f}', 'D' if r_[1] else ' ', *r_[2:])
