# whip_reach.py - in mask tien canh (ASCII) quanh Simon cho 1 frame va do doan roi nam ngang dai nhat
# Chay: python -P -E tools/whip_reach.py map1 <frame> [frame...]
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fg_map import fg_mask
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_camera.csv')))}
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
for fs in sys.argv[2:]:
    i = int(fs); c = cam[i]; s = S[i]
    m = fg_mask(np.asarray(mm[i]), c['section'], int(c['cx']))
    x0, x1, y0, y1 = int(s['x0']), int(s['x1']), int(s['y0']), int(s['y1'])
    xa, xb = max(0, x0 - 60), min(256, x1 + 60)
    print(f'frame {i}: Simon bbox x{x0}-{x1} y{y0}-{y1} (x_med {s["x_med"]})')
    best = (0, None)
    for y in range(y0 - 6, y1 + 1):
        r = y - 48
        if r < 0 or r >= 176: continue
        row = m[r, xa:xb]
        print(f'  y{y:3d} ' + ''.join('#' if v else ('|' if xa + k in (x0, x1) else '.') for k, v in enumerate(row)))
        # doan lien tuc dai nhat (cho phep ho 1)
        run = 0; gap = 0; st = None; bestr = (0, 0, 0)
        for k, v in enumerate(row):
            if v:
                if st is None: st = k
                run = k - st + 1; gap = 0
                if run > bestr[0]: bestr = (run, xa + st, xa + k)
            else:
                gap += 1
                if gap > 1: st = None
        if bestr[0] > best[0]: best = (bestr[0], (y,) + bestr[1:])
    print(f'  doan ngang dai nhat: {best[0]} px tai y{best[1][0]} x{best[1][1]}..{best[1][2]}')
