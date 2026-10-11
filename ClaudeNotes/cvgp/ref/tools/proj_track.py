# proj_track.py - theo vet vu khi phu (dao/riu/binh nuoc thanh) bang mau, trong 1 doan frame
# Chay: python -P -E tools/proj_track.py <video> <kind> <f0> <f1>
#   kind: dagger (trang+xanh, dai 12..18 x 3..8), axe (tim), holy (xanh duong + trang), orb (do tron)
# In: frame, tam (x,y) NES man hinh, kich thuoc; chi frame khong lap (dup) ; + hoi quy van toc/gia toc
import sys, os
import numpy as np
from collections import deque
video, kind, f0, f1 = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
Y0, YA = 8, 48
def mask(f):
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    if kind == 'axe':
        return (r > 120) & (r < 200) & (g > 60) & (g < 130) & (b > 200)   # tim riu (176,88,240)
    if kind == 'dagger':
        return ((r > 220) & (g > 200) & (b > 140) & (b < 215)) | ((b > 180) & (r < 60) & (g > 40) & (g < 110))   # luoi kem + chuoi xanh
    if kind == 'holy':
        return (b > 150) & (r < 90) & (g > 50) & (g < 120)   # loi xanh cua binh nuoc thanh (video boss: ~(30,90,190))
    return np.zeros(r.shape, bool)
RAD = 2 if kind == 'axe' else 1
def comps(m):
    H, W = m.shape; lab = np.zeros(m.shape, np.int32); res = []
    for y, x in zip(*np.nonzero(m)):
        if lab[y, x]: continue
        k = len(res) + 1; q = deque([(y, x)]); lab[y, x] = k; pts = []
        while q:
            cy, cx = q.popleft(); pts.append((cy, cx))
            for dy in range(-RAD, RAD + 1):
                for dx in range(-RAD, RAD + 1):
                    ny, nx = cy + dy, cx + dx
                    if 0 <= ny < H and 0 <= nx < W and m[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = k; q.append((ny, nx))
        res.append(np.array(pts))
    return res
pts = []
for i in range(f0, f1 + 1):
    if dup[i]: continue
    f = np.asarray(mm[i][YA - Y0:224 - Y0]).astype(int)
    m = mask(f)
    for p in comps(m):
        h = p[:, 0].max() - p[:, 0].min() + 1; w = p[:, 1].max() - p[:, 1].min() + 1
        ok = {'axe': 6 <= h <= 18 and 6 <= w <= 18 and len(p) >= 10,
              'dagger': 2 <= h <= 9 and 10 <= w <= 20 and len(p) >= 8,
              'holy': 1 <= h <= 6 and 2 <= w <= 7 and len(p) >= 3}[kind]
        if ok:
            pts.append((i, (p[:, 1].min() + p[:, 1].max()) / 2, (p[:, 0].min() + p[:, 0].max()) / 2 + YA, w, h))
for q in pts:
    print(q[0], f'{q[0]/30:.2f}', f'x{q[1]:.1f} y{q[2]:.1f} {q[3]}x{q[4]}')
if len(sys.argv) > 5 and sys.argv[5] == 'fit' and len(pts) >= 4:
    P = np.array([(q[0] / 30.0 * 60.0988, q[1], q[2]) for q in pts])   # t theo tick NES
    t = P[:, 0] - P[0, 0]
    kx, cx = np.polyfit(t, P[:, 1], 1)
    a2, a1, a0 = np.polyfit(t, P[:, 2], 2)
    print(f'FIT: vx = {kx:+.3f} px/tick ; y(t) = {a0:.1f} + ({a1:+.3f}) t + ({a2:+.4f}) t^2  -> vy0 {a1:+.3f} px/tick, g = {2*a2:.4f} px/tick^2, '
          f'dinh tai t={-a1/(2*a2):.1f} tick, cao {a1*a1/(4*a2):.1f} px tren diem dau')
