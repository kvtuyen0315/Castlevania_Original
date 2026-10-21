# calib_fine.py (CVM3 02) - hiệu chuẩn TINH video Map_3: khớp ZNCC nhiều frame, cx mỗi frame tự tinh ±2, vòng mịn tới ±0.001 / ±0.1 px.
# Chạy (từ gốc repo): python -P -E Tools/cvm3/calib_fine.py map3 12:12:WalkwayEntranceA:512 92:WalkwayA:0 ...
# Khác calib_map3.py: cx không cố định (mỗi frame lấy max trên cx0-2..cx0+2) ⇒ dùng được cả frame camera đang cuộn chậm.
# Quy ước nes3: chỉ số pixel video = round(ox + sx*(x+0.5)); (ox,oy) ở toạ độ chỉ số (tâm pixel = chỉ số).
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import *
import numpy as np
video = sys.argv[1]
BOX = int(os.environ.get('BOX', '0'))
sheet = load_sheet()
items = []
for arg in sys.argv[2:]:
    t, k, cx = arg.split(':')
    img = next(read_frames(video, ss=float(t)))
    items.append((float(t), k, img, gray(padded_section(sheet, k)), int(cx) + PAD.get(k, 0)))

def zn(pf, w):
    a = pf - pf.mean(); b = w - w.mean()
    d = np.sqrt((a * a).sum() * (b * b).sum())
    return float((a * b).sum() / d) if d > 0 else 0.0

def frame_score(cal, it, span=2):
    t, k, img, sec, cx0 = it
    pf = gray(nes_sample(img, cal, 48, 224, box=BOX))
    best = (-2, cx0)
    for cx in range(max(0, cx0 - span), min(sec.shape[1] - 256, cx0 + span) + 1):
        s = zn(pf, sec[16:192, cx:cx + 256])
        if s > best[0]:
            best = (s, cx)
    return best

def score(cal):
    return sum(frame_score(cal, it)[0] for it in items) / len(items)

cal = dict(CALIB[video])
for it_ in range(3):
    step = [0.01, 0.003, 0.001][it_]; ostep = [1.0, 0.3, 0.1][it_]
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
    print('iter', it_, 'ZNCC TB %.4f' % best[0], {k: round(v, 4) for k, v in cal.items()}, 'sx/sy %.4f' % (cal['sx'] / cal['sy'])); sys.stdout.flush()
print('--- per frame (cx tinh) ---')
for it in items:
    s, cx = frame_score(cal, it)
    print('t=%7.2f %-15s cx %4d (guess %4d) ZNCC %.4f' % (it[0], it[1], cx - PAD.get(it[1], 0), it[4] - PAD.get(it[1], 0), s))
# độ nhạy: đổi từng tham số quanh cực đại (tâm giữ nguyên), in mức giảm ZNCC TB ⇒ sai số ≈ biên độ làm giảm < 0.001
print('--- sensitivity (delta ZNCC TB vs optimum) ---')
s0 = score(cal)
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
print('final', {k: round(float(v), 4) for k, v in cal.items()}, 'ZNCC TB %.4f' % s0)
