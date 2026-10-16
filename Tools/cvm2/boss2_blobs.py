# boss2_blobs.py (CVM2 02) - video boss Map 2: nền = trung vị thời gian (phòng tĩnh, cx 0) -> blob tiền cảnh từng frame -> work/boss2_blobs.csv
# Chạy: python -P -E Tools/cvm2/boss2_blobs.py   (cần boss2_nes.npy). Cột: frame,x0,y0,x1,y1,n,cx,cy,red,pur,blu,wht,cr,or (màu theo bảng màu video boss — chỉ để phân biệt tương đối)
import sys, os, csv
from collections import deque
import numpy as np
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK
mm = np.load(os.path.join(WORK, 'boss2_nes.npy'), mmap_mode='r')
N = mm.shape[0]
sub = np.asarray(mm[::3, 48 - 8:224 - 8]).astype(np.uint8)
bgm = np.median(sub, axis=0)
np.save(os.path.join(WORK, 'boss2_bg.npy'), bgm.astype(np.uint8))
def label(m, grow=2):
    H, W = m.shape
    d = m.copy()
    for dy in range(-grow, grow + 1):
        for dx in range(-grow, grow + 1):
            d |= np.roll(np.roll(m, dy, 0), dx, 1)
    lab = np.zeros(m.shape, np.int32); n = 0
    ys, xs = np.nonzero(d)
    for y, x in zip(ys, xs):
        if lab[y, x]: continue
        n += 1; q = deque([(y, x)]); lab[y, x] = n
        while q:
            cy, cx_ = q.popleft()
            for ny in (cy - 1, cy, cy + 1):
                for nx in (cx_ - 1, cx_, cx_ + 1):
                    if 0 <= ny < H and 0 <= nx < W and d[ny, nx] and not lab[ny, nx]:
                        lab[ny, nx] = n; q.append((ny, nx))
    return lab, n
rows = []
for i in range(N):
    fr = np.asarray(mm[i])[48 - 8:224 - 8].astype(float)
    m = np.abs(fr - bgm).max(axis=2) > 60
    m[:14, 100:176] = False     # watermark mờ "OVERTIME GAMER" không nằm trong y>=48 nên vô hại; giữ dự phòng
    if m.sum() == 0: continue
    lab, n = label(m)
    px = fr.astype(int)
    r, g, b = px[..., 0], px[..., 1], px[..., 2]
    for k in range(1, n + 1):
        sel = (lab == k) & m
        c = int(sel.sum())
        if c < 3: continue
        ys, xs = np.nonzero(sel)
        rows.append([i, int(xs.min()), int(ys.min()) + 48, int(xs.max()), int(ys.max()) + 48, c, '%.1f' % xs.mean(), '%.1f' % (ys.mean() + 48),
                     int((sel & (r > 150) & (g < 70) & (b < 90)).sum()), int((sel & (b > 150) & (r > 90) & (g < 130)).sum()), int((sel & (b > 110) & (r < 80) & (g < 100)).sum()),
                     int((sel & (px.min(axis=2) > 170)).sum()), int((sel & (r > 220) & (g > 190) & (b > 90) & (b < 215)).sum()), int((sel & (r > 200) & (g > 100) & (g < 175) & (b < 100)).sum())])
with open(os.path.join(WORK, 'boss2_blobs.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 'x0', 'y0', 'x1', 'y1', 'n', 'cx', 'cy', 'red', 'pur', 'blu', 'wht', 'cr', 'or']); w.writerows(rows)
print('done', len(rows))
