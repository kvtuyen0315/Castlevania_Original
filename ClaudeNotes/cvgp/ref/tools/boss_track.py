# boss_track.py - dinh vi Phantom Bat (va Simon) trong phong boss (camera dung yen) bang tru nen median
# Chay: python -P -E tools/boss_track.py <video> <f0> <f1> <out.csv>
#   map1: f0=5935 f1=6330 (camera = 512 co dinh) ; boss: 0 760
# Nen = trung vi theo thoi gian (buoc 2 frame) cua cac frame trong khoang. Tien canh = |frame - nen| > 45 (kenh max).
# Bat = cum tien canh lon nhat co >=150 px, >=25 px do (r>150,g<90,b<90), rong >=24 px, khong tinh pixel mau Simon.
# Toa do NES: x tam, y tam, bbox. Cot 'wing' = do rong bbox (canh dang/khep).
import sys, os, csv
import numpy as np
from collections import deque
video, f0, f1, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), sys.argv[4]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
Y0 = 8; YA, YB = 48, 226
stack = np.array([np.asarray(mm[i][YA - Y0:YB - Y0]) for i in range(f0, f1 + 1, 2)])
bg = np.median(stack, axis=0).astype(np.int16)
from PIL import Image
Image.fromarray(bg.astype(np.uint8)).save(os.path.join(base, f'{video}_bossbg_{f0}.png'))

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
    f = np.asarray(mm[i][YA - Y0:YB - Y0]).astype(np.int16)
    fg = np.abs(f - bg).max(axis=2) > 45
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    simon = ((r > 200) & (g > 110) & (g < 185) & (b < 90)) | ((r > 215) & (g > 185) & (b > 110) & (b < 205))
    red = (r > 150) & (g < 90) & (b < 90) & fg
    m = fg & ~simon
    rec = dict(frame=i, t=f'{i/30:.3f}', found=0)
    if 0 < m.sum() < 6000:
        best = None
        for pts in comps(m):
            nred = int(red[pts[:, 0], pts[:, 1]].sum())
            nsim = int(simon[np.clip(pts[:, 0], 0, None), pts[:, 1]].sum())
            w_ = pts[:, 1].max() - pts[:, 1].min() + 1
            if len(pts) >= 150 and nred >= 25 and w_ >= 24:   # doi: canh dang 48x22 (~520px) hoac khep 32x23 (~330px)
                if best is None or len(pts) > best[0]:
                    best = (len(pts), pts, nred)
        if best:
            n, pts, nred = best
            ys = pts[:, 0] + YA; xs = pts[:, 1]
            rec.update(found=1, xc=f'{(xs.min()+xs.max())/2:.1f}', yc=f'{(ys.min()+ys.max())/2:.1f}', x0=int(xs.min()), x1=int(xs.max()),
                       y0=int(ys.min()), y1=int(ys.max()), npx=n, nred=nred, wing=int(xs.max() - xs.min() + 1), h=int(ys.max() - ys.min() + 1))
    rows.append(rec)
keys = ['frame', 't', 'found', 'xc', 'yc', 'x0', 'x1', 'y0', 'y1', 'npx', 'nred', 'wing', 'h']
with open(out, 'w', newline='') as fh:
    w = csv.DictWriter(fh, fieldnames=keys); w.writeheader()
    for r_ in rows: w.writerow(r_)
print('done', len(rows), 'found', sum(r_['found'] for r_ in rows))
