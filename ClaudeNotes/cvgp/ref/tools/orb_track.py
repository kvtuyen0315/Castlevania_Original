# orb_track.py - tim qua cau do (magic crystal) va cum lua chet cua boss sau khi boss chet
# Chay: python -P -E tools/orb_track.py <video> <f0> <f1>
# Qua cau: cum pixel do (r>150, g<80, b<80) gan tron 10..16 px; in bbox moi frame co thay doi.
import sys, os
import numpy as np
from collections import deque
video, f0, f1 = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
Y0 = 8; YA = 48
def comps(m):
    H, W = m.shape; lab = np.zeros(m.shape, np.int32); res = []
    for y, x in zip(*np.nonzero(m)):
        if lab[y, x]: continue
        k = len(res) + 1; q = deque([(y, x)]); lab[y, x] = k; pts = []
        while q:
            cy, cx = q.popleft(); pts.append((cy, cx))
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < H and 0 <= nx < W and m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = k; q.append((ny, nx))
        res.append(np.array(pts))
    return res
prev = None
for i in range(f0, f1 + 1):
    f = np.asarray(mm[i][YA - Y0:226 - Y0]).astype(int)
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    red = (r > 150) & (g < 80) & (b < 90)
    out = []
    for c in comps(red):
        h = c[:, 0].max() - c[:, 0].min() + 1; w = c[:, 1].max() - c[:, 1].min() + 1
        if 8 <= h <= 18 and 8 <= w <= 18 and len(c) >= 0.5 * h * w:
            out.append((int(c[:, 1].min()), int(c[:, 1].max()), int(c[:, 0].min() + YA), int(c[:, 0].max() + YA), len(c)))
    s = ' '.join(f'x{a}-{b_} y{c_}-{d}' for a, b_, c_, d, n in out)
    if s != prev:
        print(i, f'{i/30:.2f}', s if s else '-')
        prev = s
