# small_movers.py - tim vat the nho (3..10 px) di chuyen ngang deu (dan lua Fish Man, dao nem, ...) bang tru nen ban do
# Chay: python -P -E tools/small_movers.py map1 <f0> <f1>
# In cac "vet" (track) dai >= 6 frame moi, toc do ngang on dinh, khong trung vi tri Simon.
import sys, os, csv
import numpy as np
from collections import deque
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fg_map import fg_mask
video, f0, f1 = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_camera.csv')))}
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
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
dets = []
for i in range(f0, f1 + 1):
    if dup[i]: continue
    c = cam[i]
    if c['section'] == '-' or float(c['score']) < 0.8: continue
    fr = np.asarray(mm[i])
    m = fg_mask(fr, c['section'], int(c['cx']))
    s = S[i]
    if s['found'] == '1':
        m[max(0, int(s['y0']) - 52):int(s['y1']) - 44, max(0, int(s['x0']) - 4):int(s['x1']) + 5] = False
    f = fr[40:216].astype(int)
    for p in comps(m):
        h = p[:, 0].max() - p[:, 0].min() + 1; w = p[:, 1].max() - p[:, 1].min() + 1
        if 3 <= h <= 10 and 3 <= w <= 10 and len(p) >= 6:
            col = f[p[:, 0], p[:, 1]].mean(axis=0)
            dets.append((i, int(c['cx']) + p[:, 1].mean(), p[:, 0].mean() + 48, w, h, tuple(int(v) for v in col)))
# lien ket thanh vet: cung y +-2, x tang/giam deu
dets.sort()
tracks = []
for d in dets:
    placed = False
    for t in tracks:
        last = t[-1]
        if 0 < d[0] - last[0] <= 4 and abs(d[2] - last[2]) <= 2 and 0 < abs(d[1] - last[1]) <= 4 * (d[0] - last[0]) + 2:
            if len(t) >= 2:
                v_old = (last[1] - t[-2][1]) / (last[0] - t[-2][0]); v_new = (d[1] - last[1]) / (d[0] - last[0])
                if np.sign(v_old) != np.sign(v_new): continue
            t.append(d); placed = True; break
    if not placed: tracks.append([d])
for t in tracks:
    if len(t) >= 6:
        a, b = t[0], t[-1]
        v = (b[1] - a[1]) / (b[0] - a[0])
        print(f'vet f{a[0]}-{b[0]} ({a[0]/30//60:.0f}:{a[0]/30%60:05.2f}) world x {a[1]:.0f}->{b[1]:.0f} y~{np.mean([q[2] for q in t]):.0f}  v={v:+.2f} px/frame video = {v*30/60.0988:+.2f} px/tick  n={len(t)}  co {t[0][3]}x{t[0][4]} mau {t[len(t)//2][5]}')
