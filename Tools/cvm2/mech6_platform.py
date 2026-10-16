# mech6_platform.py (CVM2 06) - do vi tri be di dong (Object/platform.png, phan duc 32x8) TUNG FRAME video Map_2 bang ghep nen:
#   voi moi frame game (camera CVM2 02), dung nen sheet (fg2.bg, doi mau sheet->video) roi ve sprite be o moi vi tri ung vien trong lan,
#   chon vi tri co tong |sai khac| nho nhat tren dai hang NES y0..y0+7. Mau sprite: lan 1 doan, lan 2 = trung vi tren frame da khop tot.
# Chay (tu goc repo): python -P -E Tools/cvm2/mech6_platform.py <ten> <section> <f0> <f1> <lanX0> <lanX1> [y0_nes=113]
#   -> ClaudeNotes/cvm2/_cache/06/platform_<ten>.csv (frame,t,dup,cx,x,sad,sad2,margin,vis)  x = mep TRAI sprite (px section)
import sys, os, csv
import numpy as np
from PIL import Image
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK, ROOT, bg, load_camera
name, sec = sys.argv[1], sys.argv[2]
f0, f1, L0, L1 = (int(v) for v in sys.argv[3:7])
Y0 = int(sys.argv[7]) if len(sys.argv) > 7 else 113
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06'); os.makedirs(OUT, exist_ok=True)
spr = np.asarray(Image.open(os.path.join(ROOT, 'Assets', 'Data', 'Art', 'Object', 'platform.png')).convert('RGBA'))[:8]
alpha = spr[..., 3] > 0
key = {(252, 216, 168): 0, (124, 8, 0): 1, (252, 152, 56): 2}
idx = np.full(alpha.shape, -1)
for y in range(8):
    for x in range(32):
        if alpha[y, x]: idx[y, x] = key[tuple(int(v) for v in spr[y, x, :3])]
mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(WORK, 'map2_dup.npy'))
cam = load_camera('map2')
R0 = Y0 - 48  # hang trong bg (NES y 48..223)

def run(colors):
    rows = []; samples = [[], [], []]
    for f in range(f0, f1 + 1):
        c = cam.get(f)
        if c is None or c['kind'] != 'game' or c['section'] != sec: continue
        cx = int(c['cx'])
        b = bg(sec, cx)[R0 - 2:R0 + 10]           # 12 hang: 2 tren + 8 sprite + 2 duoi
        fr = np.asarray(mm[f])[Y0 - 2 - 8:Y0 + 10 - 8].astype(float)
        wx0 = max(0, L0 - cx - 8); wx1 = min(256, L1 - cx + 8)
        if wx1 - wx0 < 8: continue
        base = np.abs(fr - b).sum(axis=2)
        best = []
        for p in range(L0 - 8, L1 - 32 + 9):
            sx = p - cx
            comp = b.copy()
            for k in range(3):
                m = np.zeros((12, 256), bool)
                ys, xs = np.where(idx == k)
                xx = xs + sx; ok = (xx >= 0) & (xx < 256)
                m[ys[ok] + 2, xx[ok]] = True
                comp[m] = colors[k]
            d = np.abs(fr - comp).sum(axis=2)[:, wx0:wx1].sum()
            vis = int(((np.arange(32) + sx >= 0) & (np.arange(32) + sx < 256)).sum())
            best.append((d, p, vis))
        best.sort()
        d0 = base[:, wx0:wx1].sum()
        (d, p, vis) = best[0]
        d2 = best[1][0] if len(best) > 1 else d
        rows.append([f, '%.3f' % (f / 30.0), int(dup[f]), cx, p, int(d), int(d2), '%.3f' % ((d0 - d) / max(1.0, d0)), vis])
        if vis == 32 and (d0 - d) / max(1.0, d0) > 0.5:
            sx = p - cx
            for k in range(3):
                ys, xs = np.where(idx == k)
                for y, x in zip(ys, xs): samples[k].append(fr[y + 2, x + sx])
    return rows, samples

colors = [np.array([228, 206, 186.]), np.array([90, 0, 0.]), np.array([237, 139, 51.])]
rows, samples = run(colors)
colors2 = [np.median(np.array(s), axis=0) if len(s) > 20 else colors[k] for k, s in enumerate(samples)]
print('mau uoc luong', [list(np.round(c)) for c in colors2])
rows, _ = run(colors2)
with open(os.path.join(OUT, 'platform_%s.csv' % name), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'dup', 'cx', 'x', 'sad', 'sad2', 'margin', 'vis']); w.writerows(rows)
print(name, 'frames', len(rows))
