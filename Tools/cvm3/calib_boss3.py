# calib_boss.py (CVM3 02) - hiệu chuẩn video boss Map 3 (bảng màu KHÁC sheet): ZNCC trên ĐỘ LỚN GRADIENT (cạnh) thay vì độ sáng.
# Chạy (từ gốc repo): python -P -E Tools/cvm3/calib_boss3.py <t1> <t2> ...   (phòng Mummy, MummyMan cx 1280 khoá)
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import *
import numpy as np
video = 'boss3'
sheet = load_sheet()
sec = gray(section(sheet, 'MummyMan'))[16:192, 1280:1536]   # NES y 48..223, cx 1280

def grad(a):
    gx = np.abs(np.diff(a, axis=1))[:-1]
    gy = np.abs(np.diff(a, axis=0))[:, :-1]
    return gx + gy

G0 = grad(sec)
imgs = [(float(t), next(read_frames(video, ss=float(t)))) for t in sys.argv[1:]]

def score(cal, ret=False):
    out = []
    for t, img in imgs:
        pf = gray(nes_sample(img, cal, 48, 224))
        # che HUD/Simon/boss: chỉ dùng cột nền (bỏ 24 hàng giữa-dưới là chỗ Simon/Medusa/rắn) -> dùng nửa trên (y 48..130, NES) + cột phải
        g = grad(pf)
        m = np.zeros(g.shape, bool); m[0:80, :] = True   # CVM3: chi nua tren (tranh Simon/Mummy/ko-fi); watermark HUD o NES y<48 nam ngoai vung so
        out.append(zncc(g[m], G0[m]))
    return out if ret else sum(out) / len(out)

cal = dict(CALIB[video])
for it_ in range(4):
    step = [0.02, 0.01, 0.003, 0.001][it_]; ostep = [2.0, 1.0, 0.3, 0.1][it_]
    for axis in ('x', 'y'):
        best = None
        if axis == 'x':
            c0 = cal['ox'] + cal['sx'] * 128
            for sx in np.arange(cal['sx'] - 8 * step, cal['sx'] + 8.01 * step, step):
                for cv in np.arange(c0 - 6 * ostep, c0 + 6.01 * ostep, ostep):
                    c = dict(cal, sx=sx, ox=cv - sx * 128)
                    s = score(c)
                    if best is None or s > best[0]: best = (s, c)
        else:
            c0 = cal['oy'] + cal['sy'] * 136
            for sy in np.arange(cal['sy'] - 8 * step, cal['sy'] + 8.01 * step, step):
                for cv in np.arange(c0 - 6 * ostep, c0 + 6.01 * ostep, ostep):
                    c = dict(cal, sy=sy, oy=cv - sy * 136)
                    s = score(c)
                    if best is None or s > best[0]: best = (s, c)
        cal = best[1]
    print('iter', it_, 'ZNCC grad TB %.4f' % best[0], {k: round(float(v), 4) for k, v in cal.items()}, 'sx/sy %.4f' % (cal['sx'] / cal['sy'])); sys.stdout.flush()
for (t, _), s in zip(imgs, score(cal, True)):
    print('t=%5.2f ZNCC grad %.4f' % (t, s))
s0 = score(cal)
print('--- sensitivity ---')
for name, vals in (('sx', (-0.006, -0.003, 0.003, 0.006)), ('sy', (-0.006, -0.003, 0.003, 0.006)),
                   ('cxv', (-0.6, -0.3, 0.3, 0.6)), ('cyv', (-0.6, -0.3, 0.3, 0.6))):
    out = []
    for d in vals:
        c = dict(cal)
        if name == 'sx': c['sx'] += d; c['ox'] = cal['ox'] + cal['sx'] * 128 - c['sx'] * 128
        elif name == 'sy': c['sy'] += d; c['oy'] = cal['oy'] + cal['sy'] * 136 - c['sy'] * 136
        elif name == 'cxv': c['ox'] += d
        else: c['oy'] += d
        out.append('%+.3f:%+.4f' % (d, score(c) - s0))
    print(name, ' '.join(out))
print('final', {k: round(float(v), 4) for k, v in cal.items()}, 'ZNCC grad TB %.4f' % s0)
