# boss_segments.py - chia quy dao doi (x,y theo t) thanh cac doan thang (Douglas-Peucker, dung sai 4 px),
# in diem dau/cuoi, thoi luong, van toc (px/s va px/tick 60Hz); doan dung yen (<15 px/s) = "luon/dung".
# Chay: python -P -E tools/boss_segments.py <bat.csv> [t_offset_label]
import sys, csv
import numpy as np
f = sys.argv[1]
R = [r for r in csv.DictReader(open(f)) if r['found'] == '1']
T = np.array([float(r['t']) for r in R]); X = np.array([float(r['xc']) for r in R]); Y = np.array([float(r['yc']) for r in R])
Wg = np.array([int(r['wing']) for r in R])
# loai diem nhay vot (glitch): khac trung vi 5 diem lan can > 25 px
keep = np.ones(len(T), bool)
for i in range(len(T)):
    a, b = max(0, i - 3), min(len(T), i + 4)
    if abs(X[i] - np.median(X[a:b])) > 25 or abs(Y[i] - np.median(Y[a:b])) > 25: keep[i] = False
T, X, Y, Wg = T[keep], X[keep], Y[keep], Wg[keep]
def dp(i0, i1, tol):
    if i1 <= i0 + 1: return [i0, i1]
    t0, t1 = T[i0], T[i1]
    if t1 - t0 > 0.6:   # tach theo khoang trong du lieu
        pass
    u = (T[i0:i1 + 1] - t0) / max(t1 - t0, 1e-6)
    px = X[i0] + u * (X[i1] - X[i0]); py = Y[i0] + u * (Y[i1] - Y[i0])
    d = np.hypot(X[i0:i1 + 1] - px, Y[i0:i1 + 1] - py)
    k = int(np.argmax(d))
    if d[k] > tol:
        L = dp(i0, i0 + k, tol); Rr = dp(i0 + k, i1, tol)
        return L[:-1] + Rr
    return [i0, i1]
# chia theo khoang trong > 0.5 s
breaks = [0] + [i for i in range(1, len(T)) if T[i] - T[i - 1] > 0.5] + [len(T)]
print(' t0     t1    dur   (x0,y0) -> (x1,y1)      dx    dy   v(px/s)  v(px/tick)  kieu')
for a, b in zip(breaks[:-1], breaks[1:]):
    idx = dp(a, b - 1, 4.0)
    for i, j in zip(idx[:-1], idx[1:]):
        dur = T[j] - T[i]
        if dur <= 0: continue
        dx, dy = X[j] - X[i], Y[j] - Y[i]
        v = np.hypot(dx, dy) / dur
        kind = 'dung/luon' if v < 15 else ('lao nhanh' if v > 100 else 'bay')
        print(f'{T[i]:6.2f} {T[j]:6.2f} {dur:5.2f}  ({X[i]:5.1f},{Y[i]:5.1f}) -> ({X[j]:5.1f},{Y[j]:5.1f})  {dx:+6.1f} {dy:+6.1f}  {v:6.1f}   {v/60.0988:5.2f}     {kind}')
# vo canh: so lan doi do rong canh (48<->32)
ch = np.sum(np.abs(np.diff(Wg)) > 8)
print(f'doi trang thai canh: {ch} lan trong {T[-1]-T[0]:.1f}s -> chu ky vo canh ~{2*(T[-1]-T[0])/max(ch,1):.2f}s (bi anh huong frame lap)')
