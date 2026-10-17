# CVM2 11 - khop mau doi/cham/Simon tren ban do lau dai (map2_nes.npy).
# Chay tu goc repo: python -P -E Tools/cvm2/flow11_cmap.py <f0> <f1> CastleMap/castlemap_bat_0,CastleMap/castlemap_bat_1,CastleMap/castlemap_marker [buoc]
import sys
import numpy as np
from PIL import Image
from numpy.lib.stride_tricks import sliding_window_view as swv
mm = np.load('ClaudeNotes/cvm2/_cache/02/work/map2_nes.npy', mmap_mode='r')
dup = np.load('ClaudeNotes/cvm2/_cache/02/work/map2_dup.npy')
Y0 = 8
def rgba(n):
    return np.asarray(Image.open('Assets/Data/Art/%s.png' % n).convert('RGBA')).astype(np.float32)
names = sys.argv[3].split(',')
spr = [(n, rgba(n)) for n in names]
def best(nes, s, flip=False):
    if flip:
        s = s[:, ::-1]
    h, w = s.shape[:2]
    m = s[..., 3] > 0
    win = swv(nes.astype(np.float32), (h, w, 3))[:, :, 0]   # (H-h+1, W-w+1, h, w, 3)
    d = np.abs(win - s[..., :3]).sum(-1)                     # (.., h, w)
    e = (d * m).sum((-1, -2)) / m.sum() / 3
    k = np.unravel_index(np.argmin(e), e.shape)
    return e[k], k[1], k[0] + Y0
f0, f1 = int(sys.argv[1]), int(sys.argv[2])
step = int(sys.argv[4]) if len(sys.argv) > 4 else 1
for f in range(f0, f1 + 1, step):
    nes = np.asarray(mm[f])
    out = []
    for n, s in spr:
        e, x, y = best(nes, s, flip=(n.endswith('walk_0') or n.endswith('walk_1') or n.endswith('walk_2') or n.endswith('idle_0')))
        out.append('%s %.1f (%d,%d)' % (n.split('/')[-1], e, x, y))
    print(f, 'dup' if dup[f] else '   ', ' | '.join(out))
