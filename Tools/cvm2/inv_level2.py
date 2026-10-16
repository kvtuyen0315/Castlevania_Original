# inv_level2.py - tim cac vung (khoi khong phai nen xanh 0,128,0) trong sheet Level 2 + kiem cao/rong
# Chay: python -P -E ClaudeNotes/cvm2/_cache/tools/inv_level2.py <sheet.png>
import sys
import numpy as np
from PIL import Image
a = np.asarray(Image.open(sys.argv[1]).convert('RGB')).astype(int)
H, W, _ = a.shape
bg = (a[..., 0] == 0) & (a[..., 1] == 128) & (a[..., 2] == 0)
m = ~bg
# connected components (4-neighbour) by BFS on a downsampled-free mask
lab = np.zeros((H, W), np.int32)
comps = []
from collections import deque
cur = 0
for y in range(H):
    row = m[y]
    for x in np.nonzero(row & (lab[y] == 0))[0]:
        if lab[y, x]:
            continue
        cur += 1
        q = deque([(y, x)]); lab[y, x] = cur
        x0 = x1 = x; y0 = y1 = y; n = 0
        while q:
            cy, cx = q.popleft(); n += 1
            if cx < x0: x0 = cx
            if cx > x1: x1 = cx
            if cy < y0: y0 = cy
            if cy > y1: y1 = cy
            for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                ny, nx = cy + dy, cx + dx
                if 0 <= ny < H and 0 <= nx < W and m[ny, nx] and not lab[ny, nx]:
                    lab[ny, nx] = cur; q.append((ny, nx))
        comps.append((x0, y0, x1 - x0 + 1, y1 - y0 + 1, n))
comps.sort(key=lambda c: -c[4])
for c in comps:
    if c[4] >= 50:
        print('rect [x,y,w,h] = [%d, %d, %d, %d]  px %d  fill %.3f' % (c[0], c[1], c[2], c[3], c[4], c[4] / (c[2] * c[3])))
print('components >=50px:', sum(1 for c in comps if c[4] >= 50), ' total', len(comps))
