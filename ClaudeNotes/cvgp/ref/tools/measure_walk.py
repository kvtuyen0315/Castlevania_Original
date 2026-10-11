# measure_walk.py - toc do di bo: hoi quy tuyen tinh world_x (= camera cx + x man hinh) theo thoi gian
# trong cac doan di thang lien tuc >= 1 s (tu dong tim) + mot so doan chon tay.
# Chay: python -P -E tools/measure_walk.py map1
# NES = 60.0988 Hz. Ket qua px/frame NES = (px/s) / 60.0988
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = list(csv.DictReader(open(os.path.join(base, f'{video}_simon.csv'))))
NES_HZ = 60.0988
rows = []
for r in S:
    ok = r['found'] == '1' and r['world_x'] not in ('',)
    rows.append((int(r['frame']), float(r['world_x']) if ok else np.nan, float(r['y1']) if ok else np.nan, r['section'], float(r['x_med']) if ok else np.nan, int(r['cx']) if r['cx'] not in ('', '-1') else -1))
F = np.array([x[0] for x in rows]); WX = np.array([x[1] for x in rows]); Y1 = np.array([x[2] for x in rows])
SEC = [x[3] for x in rows]
def fit(a, b):
    m = (F >= a) & (F <= b) & ~np.isnan(WX)
    t = F[m] / 30.0; x = WX[m]
    A = np.vstack([t, np.ones(len(t))]).T
    (k, c), res, _, _ = np.linalg.lstsq(A, x, rcond=None)
    resid = x - (k * t + c)
    return k, np.abs(resid).max(), m.sum(), (np.nanmax(Y1[m]) - np.nanmin(Y1[m]))
# tu dong: cua so 45 frame (1.5 s) truot, chon doan |toc do| on dinh, y chan khong doi (khong nhay/cau thang)
cands = []
i = 0
while i + 45 < len(F):
    a, b = F[i], F[i + 45]
    if len(set(SEC[i:i + 46])) == 1 and SEC[i] not in ('', '-'):
        k, rmax, n, dy = fit(a, b)
        if n >= 40 and abs(k) > 40 and rmax < 3.5 and dy <= 2:
            # mo rong cang dai cang tot
            j = i + 45
            while j + 5 < len(F) and len(set(SEC[i:j + 6])) == 1:
                k2, r2, n2, dy2 = fit(a, F[j + 5])
                if r2 < 3.5 and dy2 <= 2 and abs(k2 - k) < 4: j += 5; k = k2
                else: break
            k, rmax, n, dy = fit(a, F[j])
            cands.append((a, F[j], k, rmax, n))
            i = j + 1; continue
    i += 3
print('Doan di thang tu dong (world_x tuyen tinh, y chan khong doi):')
tot = []
for a, b, k, rmax, n in cands:
    print(f'  f{a}-f{b}  {a/30//60:.0f}:{a/30%60:05.2f}-{b/30//60:.0f}:{b/30%60:05.2f}  dur {(b-a)/30:.2f}s  v={k:7.2f} px/s = {k/NES_HZ:.3f} px/NESframe  resid max {rmax:.1f}px  n={n}  sec={SEC[list(F).index(a)]}')
    tot.append((b - a, abs(k)))
w = np.array(tot)
if len(w):
    print(f'Trung binh co trong so thoi luong: {np.average(w[:,1], weights=w[:,0]):.2f} px/s = {np.average(w[:,1], weights=w[:,0])/NES_HZ:.4f} px/frame NES  (n={len(w)} doan, tong {w[:,0].sum()/30:.1f}s)')
