# enemy_speed.py - do toc do quai: phat hien theo mau -> lien ket thanh vet (theo world x) -> hoi quy x(t), y(t)
# Chay: python -P -E tools/enemy_speed.py map1 <kind> <f0> <f1>
#   kind: zombie (hong-trang), fishman (do, cao 28-36), bat (vien hong ~(228,68,132), 8-16 px)
# Bo frame lap (dup). In moi vet >= 8 diem: v_x (px/tick), bien do dao dong y, chu ky (neu co).
import sys, os, csv
import numpy as np
from collections import deque
video, kind, f0, f1 = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_camera.csv')))}
Y0, YA = 8, 48
TICK = 60.0988 / 30.0
def mask(f):
    r, g, b = f[..., 0], f[..., 1], f[..., 2]
    if kind == 'zombie': return (r > 200) & (b > 190) & (g > 120) & (g < 215)
    if kind == 'fishman': return (r > 140) & (g < 110) & (b < 130) & (r - g > 70)
    if kind == 'bat': return (r > 190) & (g < 120) & (b > 95) & (b < 180)
def sizeok(h, w, n):
    if kind == 'zombie': return 14 <= h <= 34 and 6 <= w <= 20 and n >= 30
    if kind == 'fishman': return 20 <= h <= 36 and 10 <= w <= 24 and n >= 40
    if kind == 'bat': return 4 <= h <= 16 and 6 <= w <= 18 and n >= 8
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
dets = []
for i in range(f0, f1 + 1):
    if dup[i] or cam[i]['section'] == '-': continue
    f = np.asarray(mm[i][YA - Y0:224 - Y0]).astype(int)
    for p in comps(mask(f)):
        h = p[:, 0].max() - p[:, 0].min() + 1; w = p[:, 1].max() - p[:, 1].min() + 1
        if sizeok(h, w, len(p)):
            xc = (p[:, 1].min() + p[:, 1].max()) / 2
            if xc < 6 or xc > 250: continue      # bo mep man (sprite bi cat)
            dets.append((i, int(cam[i]['cx']) + xc, (p[:, 0].min() + p[:, 0].max()) / 2 + YA, h, w))
tracks = []
for d in dets:
    best = None
    for t in tracks:
        l = t[-1]; df = d[0] - l[0]
        if 0 < df <= 6 and abs(d[1] - l[1]) <= 5 * df + 3 and abs(d[2] - l[2]) <= 6 * df + 4:
            if best is None or abs(d[1] - l[1]) < abs(d[1] - best[-1][1]): best = t
    if best is not None: best.append(d)
    else: tracks.append([d])
for t in tracks:
    if len(t) < 8: continue
    T = np.array(t)
    tt = (T[:, 0] - T[0, 0]) * TICK
    kx, cx = np.polyfit(tt, T[:, 1], 1); ky, cy = np.polyfit(tt, T[:, 2], 1)
    rx = np.abs(T[:, 1] - (kx * tt + cx)).max()
    a = int(T[0, 0]); b = int(T[-1, 0])
    print(f'vet f{a}-{b} ({a/30//60:.0f}:{a/30%60:05.2f}-{b/30//60:.0f}:{b/30%60:05.2f}) n={len(t)}: world x {T[0,1]:.0f}->{T[-1,1]:.0f}, vx={kx:+.3f} px/tick (lech max {rx:.1f}), y {T[:,2].min():.0f}..{T[:,2].max():.0f} (vy {ky:+.3f}), co {int(np.median(T[:,4]))}x{int(np.median(T[:,3]))}')
