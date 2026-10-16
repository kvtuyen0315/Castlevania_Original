# mech6_fit.py (CVM2 06) - khop mo hinh chuyen dong be di dong tu platform_<ten>.csv (mech6_platform.py)
#   t = frame * TPF tick (CVM2 02: 1 frame video = 2.0033 tick), bo frame lap (dup) + frame khop kem (margin < M) + sprite khuat (vis < 32)
#   Mo hinh: sin (dao dong dieu hoa), para (luc hang huong tam: parabol tung 1/4 chu ky), tri (toc do deu, doi chieu tuc thi)
# Chay: python -P -E Tools/cvm2/mech6_fit.py <ten> [<ten2> ...]  (ten nhieu doan cung 1 be: ghep chung 1 truc thoi gian)
import sys, os, csv
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06')
TPF = 2.0033
M = 0.6
def load(names):
    f, x = [], []
    for n in names:
        for r in csv.DictReader(open(os.path.join(D, 'platform_%s.csv' % n))):
            if int(r['dup']) or float(r['margin']) < M or int(r['vis']) < 32: continue
            f.append(int(r['frame'])); x.append(int(r['x']))
    return np.array(f, float), np.array(x, float)
def shape(model, u):
    u = np.mod(u, 1.0)
    if model == 'sin': return np.sin(2 * np.pi * u)
    if model == 'para':
        return np.where(u < 0.5, 1 - (4 * u - 1) ** 2, -(1 - (4 * u - 3) ** 2))
    if model == 'tri':
        return np.where(u < 0.25, 4 * u, np.where(u < 0.75, 2 - 4 * u, 4 * u - 4))
def fit(model, t, x, Tlo, Thi):
    best = None
    for T in np.arange(Tlo, Thi, 0.25):
        for ph in np.arange(0, 1, 1.0 / 400):
            g = shape(model, t / T + ph)
            A = np.vstack([g, np.ones_like(g)]).T
            sol, res, _, _ = np.linalg.lstsq(A, x, rcond=None)
            r = x - A.dot(sol)
            e = float(np.sqrt((r * r).mean()))
            if best is None or e < best[0]: best = (e, T, ph, sol[0], sol[1], r)
    return best
names = sys.argv[1:]
f, x = load(names)
t = f * TPF
print('%s: %d diem, x %d..%d, frame %d..%d' % ('+'.join(names), len(x), x.min(), x.max(), f.min(), f.max()))
for m in ('sin', 'para', 'tri'):
    e, T, ph, A, c, r = fit(m, t, x, 120, 460)
    # vi tri tich luy: dinh/day
    print('  %-4s rms %.3f px  T %.2f tick  bien do %.2f  tam %.2f  -> [%.2f, %.2f]  |res|max %.1f  vmax %.3f px/tick' %
          (m, e, T, abs(A), c, c - abs(A), c + abs(A), np.abs(r).max(), abs(A) * (2 * np.pi / T if m == 'sin' else (8.0 / T if m == 'para' else 4.0 / T))))
