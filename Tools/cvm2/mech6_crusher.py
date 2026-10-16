# mech6_crusher.py (CVM2 06) - do khung bay ep gai (Object/crusher_0..7.png, 32x(8k+8), treo tu tren) TUNG FRAME video Map_2 (Hall C)
#   voi moi frame game o TowerHallC: nen sheet (fg2.bg, doi mau) + ve crusher_k tai (tam - 16, Y0) cho k = 0..7, chon k co tong |sai khac|
#   nho nhat tren vung 32x64 (hang NES Y0..Y0+63). Bay chi do khi nam tron trong man.
# Chay (tu goc repo): python -P -E Tools/cvm2/mech6_crusher.py <f0> <f1> [Y0_nes=113] [ten=hallc]
#   -> ClaudeNotes/cvm2/_cache/06/crusher_<ten>.csv (frame,t,dup,cx, k1,m1, k2,m2, k3,m3)  k = khung (-1 = ngoai man), m = (sad2-sad1)/sad1
import sys, os, csv
import numpy as np
from PIL import Image
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK, ROOT, bg, load_camera, apply_lut
f0, f1 = int(sys.argv[1]), int(sys.argv[2])
Y0 = int(sys.argv[3]) if len(sys.argv) > 3 else 113
name = sys.argv[4] if len(sys.argv) > 4 else 'hallc'
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06'); os.makedirs(OUT, exist_ok=True)
SPR = []
for k in range(8):
    a = np.asarray(Image.open(os.path.join(ROOT, 'Assets', 'Data', 'Art', 'Object', 'crusher_%d.png' % k)).convert('RGBA')).astype(float)
    SPR.append((a[..., 3] > 0, apply_lut(a[..., :3])))
CENTERS = (368, 432, 496)
mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(WORK, 'map2_dup.npy'))
cam = load_camera('map2')
R0 = Y0 - 48
rows = []
for f in range(f0, f1 + 1):
    c = cam.get(f)
    if c is None or c['kind'] != 'game' or c['section'] != 'TowerHallC': continue
    cx = int(c['cx'])
    b = bg('TowerHallC', cx)[R0:R0 + 64]
    fr = np.asarray(mm[f])[Y0 - 8:Y0 - 8 + 64].astype(float)
    out = [f, '%.3f' % (f / 30.0), int(dup[f]), cx]
    for xc in CENTERS:
        sx = xc - 16 - cx
        if sx < 0 or sx + 32 > 256:
            out += [-1, 0]; continue
        win = fr[:, sx:sx + 32]; bb = b[:, sx:sx + 32]
        sads = []
        for k in range(8):
            al, col = SPR[k]
            comp = bb.copy(); h = al.shape[0]
            comp[:h][al] = col[al]
            sads.append(np.abs(win - comp).sum())
        o = np.argsort(sads)
        out += [int(o[0]), '%.3f' % ((sads[o[1]] - sads[o[0]]) / max(1.0, sads[o[0]]))]
    rows.append(out)
with open(os.path.join(OUT, 'crusher_%s.csv' % name), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'dup', 'cx', 'k1', 'm1', 'k2', 'm2', 'k3', 'm3']); w.writerows(rows)
print(name, len(rows), 'frames')
