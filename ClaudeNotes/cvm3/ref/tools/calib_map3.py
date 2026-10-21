# calib_map3.py - tim (sx, sy, ox, oy) khop frame video voi vung sheet Level 3 (luoi tim kiem 2 truc doc lap)
# Chay: python -P -E calib_map3.py <video> <t1:section:cx> [t2:section:cx ...]
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import *
import numpy as np
video = sys.argv[1]
sheet = load_sheet()
items = []
for arg in sys.argv[2:]:
    t, k, cx = arg.split(':')
    img = next(read_frames(video, ss=float(t)))
    sec = gray(section(sheet, k))
    items.append((img, sec, int(cx)))

def score(cal, coarse=False):
    tot = 0
    for img, sec, cx in items:
        pf = gray(nes_sample(img, cal, 48, 224))
        tot += zncc(pf, sec[16:192, cx:cx + 256])
    return tot / len(items)

cal = dict(CALIB[video])
for it in range(3):
    step = [0.01, 0.003, 0.001][it]; ostep = [1.0, 0.3, 0.1][it]
    best = None
    cxv0 = cal['ox'] + cal['sx'] * 128
    for sx in np.arange(cal['sx'] - 10 * step, cal['sx'] + 10.01 * step, step):
        for cxv in np.arange(cxv0 - 6 * ostep, cxv0 + 6.01 * ostep, ostep):
            c = dict(cal, sx=sx, ox=cxv - sx * 128)
            s = score(c)
            if best is None or s > best[0]:
                best = (s, c)
    cal = best[1]
    best = None
    cyv0 = cal['oy'] + cal['sy'] * 136
    for sy in np.arange(cal['sy'] - 10 * step, cal['sy'] + 10.01 * step, step):
        for cyv in np.arange(cyv0 - 6 * ostep, cyv0 + 6.01 * ostep, ostep):
            c = dict(cal, sy=sy, oy=cyv - sy * 136)
            s = score(c)
            if best is None or s > best[0]:
                best = (s, c)
    cal = best[1]
    print('iter', it, 'score %.4f' % best[0], {k: round(v, 4) for k, v in cal.items()}, 'sx/sy %.4f' % (cal['sx'] / cal['sy']))
