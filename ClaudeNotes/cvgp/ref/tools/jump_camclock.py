# jump_camclock.py - do cu nhay KEM DI CHUYEN khi camera dang cuon theo Simon (camera cx tang 1 px/tick 60 Hz
# = dong ho NES chinh xac, khong phu thuoc jitter cua ban quay). Khop parabol y_chan(cx) -> thoi gian bay + do cao.
# Chay: python -P -E tools/jump_camclock.py map1 f_start f_end [f_start f_end ...]
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
args = list(map(int, sys.argv[2:]))
for a, b in zip(args[::2], args[1::2]):
    pts = []
    for f in range(a - 8, b + 9):
        r = S[f]
        if r['found'] != '1' or dup[f] or r['cx'] in ('', '-1'): continue
        pts.append((int(r['cx']) + float(r['x_med']) - 128.0 * 0, int(r['cx']), float(r['x_med']), int(r['y1']), int(r['y0']), f))
    P = np.array(pts)
    cxs, y1 = P[:, 1], P[:, 3]
    base_y = np.median(y1[:4])
    air = y1 < base_y - 1
    if air.sum() < 5:
        print(a, b, 'khong du diem'); continue
    c2, c1, c0 = np.polyfit(cxs[air], y1[air], 2)
    disc = c1 * c1 - 4 * c2 * (c0 - base_y)
    r1 = (-c1 - np.sqrt(disc)) / (2 * c2); r2 = (-c1 + np.sqrt(disc)) / (2 * c2)
    apex = base_y - (c0 - c1 * c1 / (4 * c2))
    # dem theo cx: o nen cuoi cung truoc khi bay va dau tien sau khi dap
    idx = np.where(air)[0]
    pre = cxs[idx[0] - 1] if idx[0] > 0 else np.nan; post = cxs[idx[-1] + 1] if idx[-1] + 1 < len(cxs) else np.nan
    first, last = cxs[idx[0]], cxs[idx[-1]]
    print(f'f{a}-{b}: nen y={base_y:.0f}; diem tren khong={air.sum()}; cx dau/cuoi tren khong {first:.0f}..{last:.0f} (khoang {last-first:.0f}),'
          f' bao ngoai {pre:.0f}..{post:.0f} ({post-pre:.0f}); parabol: bay {r2-r1:.1f} tick, dinh {apex:.1f} px; y_min quan sat {base_y - y1.min():.0f}; x man hinh {np.median(P[:,2]):.0f}')
    print('     cx,y1:', ' '.join(f'{int(c)}:{int(y)}' for c, y in zip(cxs, y1)))
