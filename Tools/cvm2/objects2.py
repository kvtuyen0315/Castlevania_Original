# objects2.py (CVM2 02) - tách mọi blob tiền cảnh (sprite) từng frame Map_2 bằng trừ nền sheet (fg2.py) -> work/map2_blobs.csv
# Chạy: python -P -E Tools/cvm2/objects2.py map2
# Cột: frame,kind,section,cx,x0,y0,x1,y1,n,cx_s,cy_s,or,cr,red,pur,blu,wht  (toạ độ màn NES; y gốc trên màn; or/cr = pixel cam/kem kiểu Simon)
import sys, os, csv
from collections import deque
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fg2 import *
video = sys.argv[1]
f0 = int(sys.argv[2]) if len(sys.argv) > 2 else 0
f1 = int(sys.argv[3]) if len(sys.argv) > 3 else None
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
N = mm.shape[0]
cam = load_camera(video)
lut()

def label(m, grow=2):
    H, W = m.shape
    d = m.copy()
    for dy in range(-grow, grow + 1):
        for dx in range(-grow, grow + 1):
            d |= np.roll(np.roll(m, dy, 0), dx, 1)
    lab = np.zeros(m.shape, np.int32); comps = []
    ys, xs = np.nonzero(d)
    for y, x in zip(ys, xs):
        if lab[y, x]: continue
        k = len(comps) + 1; q = deque([(y, x)]); lab[y, x] = k
        while q:
            cy, cx_ = q.popleft()
            for ny in (cy - 1, cy, cy + 1):
                for nx in (cx_ - 1, cx_, cx_ + 1):
                    if 0 <= ny < H and 0 <= nx < W and d[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = k; q.append((ny, nx))
        comps.append(k)
    return lab, len(comps)

rows = []
for i in range(f0, N if f1 is None else f1 + 1):
    c = cam[i]
    if c['kind'] != 'game': continue
    fr = np.asarray(mm[i])
    m = fg_mask(fr, c['section'], int(c['cx']), int(c['cxw']))
    m[:0] = False
    if m.sum() == 0: continue
    lab, n = label(m)
    px = fr[40:216].astype(int)
    r, g, b = px[..., 0], px[..., 1], px[..., 2]
    orange = (r > 200) & (g > 100) & (g < 175) & (b < 100)
    cream = (r > 220) & (g > 190) & (b > 90) & (b < 215) & (r - b > 35)
    red = (r > 120) & (g < 70) & (b < 70); pur = (b > 150) & (r > 70) & (g < 120); blu = (b > 100) & (r < 60) & (g < 90); wht = (px.min(axis=2) > 170)
    for k in range(1, n + 1):
        sel = (lab == k) & m
        cnt = int(sel.sum())
        if cnt < 2: continue
        ys, xs = np.nonzero(sel)
        rows.append([i, c['kind'], c['section'], c['cx'], int(xs.min()), int(ys.min()) + 48, int(xs.max()), int(ys.max()) + 48, cnt,
                     '%.1f' % xs.mean(), '%.1f' % (ys.mean() + 48), int((sel & orange).sum()), int((sel & cream).sum()), int((sel & red).sum()),
                     int((sel & pur).sum()), int((sel & blu).sum()), int((sel & wht).sum())])
    if i % 500 == 0: print(i, len(rows), flush=True)
out = os.path.join(WORK, video + '_blobs.csv')
with open(out, 'w', newline='') as fh:
    w = csv.writer(fh)
    w.writerow(['frame', 'kind', 'section', 'cx', 'x0', 'y0', 'x1', 'y1', 'n', 'cx_s', 'cy_s', 'or', 'cr', 'red', 'pur', 'blu', 'wht'])
    w.writerows(rows)
print('done', len(rows))
