# redenemy_track.py - theo vet quai mau do (Fish Man / Black Leopard? / Vampire Bat) trong 1 doan: cum pixel do
#   (r>140, g<110, b<130, r-g>70) co kich thuoc cho truoc; in tam (x man hinh, y) moi frame khong lap, + world x
# Chay: python -P -E tools/redenemy_track.py map1 <f0> <f1> [hmin hmax wmin wmax]
import sys, os, csv
import numpy as np
from collections import deque
video, f0, f1 = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
hmin, hmax, wmin, wmax = (map(int, sys.argv[4:8]) if len(sys.argv) > 7 else (18, 34, 8, 22))
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_camera.csv')))}
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
for i in range(f0, f1 + 1):
    if dup[i]: continue
    f = np.asarray(mm[i][YA - Y0:224 - Y0]).astype(int)
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    m = (r > 140) & (g < 110) & (b < 130) & (r - g > 70)
    out = []
    for p in comps(m):
        h = p[:, 0].max() - p[:, 0].min() + 1; w = p[:, 1].max() - p[:, 1].min() + 1
        if hmin <= h <= hmax and wmin <= w <= wmax and len(p) >= 40:
            xc = (p[:, 1].min() + p[:, 1].max()) / 2; yb = p[:, 0].max() + YA
            out.append(f'x{xc:.0f}(w{int(cam[i]["cx"]) + xc:.0f}) ytop{p[:, 0].min() + YA} ybot{yb} {w}x{h}')
    if out: print(i, f'{i/30:.2f}', ' | '.join(out))
