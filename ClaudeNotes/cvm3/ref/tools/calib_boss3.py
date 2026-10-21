# calib_boss3.py - hieu chuan video boss Map 3 (bang mau KHAC sheet): ZNCC tren do lon GRADIENT; tim cx truoc roi tinh chinh
# Chay (tu _cache): python -P -E tools/calib_boss3.py <cx|auto> <t1> [t2 ...]
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import *
import numpy as np
video = 'boss3'
sheet = load_sheet()
full = gray(section(sheet, 'MummyMan'))

def grad(a):
    gx = np.abs(np.diff(a, axis=1))[:-1]
    gy = np.abs(np.diff(a, axis=0))[:, :-1]
    return gx + gy

imgs = [(float(t), next(read_frames(video, ss=float(t)))) for t in sys.argv[2:]]
cal = dict(CALIB[video])
M = np.zeros((175, 255), bool); M[0:100, :] = True  # nua tren (tranh Simon/Mummy)

def score(cal, cx, ret=False):
    G0 = grad(full[16:192, cx:cx + 256])
    out = []
    for t, img in imgs:
        g = grad(gray(nes_sample(img, cal, 48, 224)))
        out.append(zncc(g[M], G0[M]))
    return out if ret else sum(out) / len(out)

if sys.argv[1] == 'auto':
    best = max((score(cal, cx), cx) for cx in range(0, full.shape[1] - 256 + 1, 2))
    cx = best[1]
    print('cx auto', cx, 'score %.4f' % best[0])
else:
    cx = int(sys.argv[1])
for it_ in range(4):
    step = [0.02, 0.01, 0.003, 0.001][it_]; ostep = [2.0, 1.0, 0.3, 0.1][it_]
    for axis in ('x', 'y'):
        best = None
        if axis == 'x':
            c0 = cal['ox'] + cal['sx'] * 128
            for sx in np.arange(cal['sx'] - 8 * step, cal['sx'] + 8.01 * step, step):
                for cv in np.arange(c0 - 6 * ostep, c0 + 6.01 * ostep, ostep):
                    c = dict(cal, sx=sx, ox=cv - sx * 128)
                    s = score(c, cx)
                    if best is None or s > best[0]: best = (s, c)
        else:
            c0 = cal['oy'] + cal['sy'] * 136
            for sy in np.arange(cal['sy'] - 8 * step, cal['sy'] + 8.01 * step, step):
                for cv in np.arange(c0 - 6 * ostep, c0 + 6.01 * ostep, ostep):
                    c = dict(cal, sy=sy, oy=cv - sy * 136)
                    s = score(c, cx)
                    if best is None or s > best[0]: best = (s, c)
        cal = best[1]
    if it_ == 1 and sys.argv[1] == 'auto':
        b2 = max((score(cal, c), c) for c in range(max(0, cx - 8), min(full.shape[1] - 256, cx + 8) + 1))
        cx = b2[1]
    print('iter', it_, 'cx', cx, 'ZNCC grad TB %.4f' % best[0], {k: round(float(v), 4) for k, v in cal.items()}, 'sx/sy %.4f' % (cal['sx'] / cal['sy'])); sys.stdout.flush()
for (t, _), s in zip(imgs, score(cal, cx, True)):
    print('t=%5.2f ZNCC grad %.4f' % (t, s))
