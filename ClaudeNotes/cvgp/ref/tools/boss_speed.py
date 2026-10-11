# boss_speed.py - hoi quy toc do doi (x,y theo t) trong cac doan chon tay; chi dung frame co vi tri moi
# Chay: python -P -E tools/boss_speed.py <bat.csv> t0 t1 [t0 t1 ...]
import sys, csv
import numpy as np
R = [r for r in csv.DictReader(open(sys.argv[1])) if r['found'] == '1']
args = list(map(float, sys.argv[2:]))
for a, b in zip(args[::2], args[1::2]):
    pts = [(float(r['t']), float(r['xc']), float(r['yc'])) for r in R if a <= float(r['t']) <= b]
    # bo frame lap (vi tri trung frame truoc)
    P = [pts[0]] + [p for q, p in zip(pts, pts[1:]) if (p[1], p[2]) != (q[1], q[2])]
    P = np.array(P)
    kx, cx = np.polyfit(P[:, 0], P[:, 1], 1); ky, cy = np.polyfit(P[:, 0], P[:, 2], 1)
    v = np.hypot(kx, ky)
    rx = np.abs(P[:, 1] - (kx * P[:, 0] + cx)).max(); ry = np.abs(P[:, 2] - (ky * P[:, 0] + cy)).max()
    print(f'{a:7.2f}-{b:7.2f}s: tu ({P[0,1]:.0f},{P[0,2]:.0f}) den ({P[-1,1]:.0f},{P[-1,2]:.0f}); vx {kx:+6.1f} vy {ky:+6.1f} px/s; |v| {v:5.1f} px/s = {v/60.0988:.2f} px/tick; huong {np.degrees(np.arctan2(ky,kx)):+.0f} do; lech max {rx:.1f}/{ry:.1f}px; n={len(P)}')
