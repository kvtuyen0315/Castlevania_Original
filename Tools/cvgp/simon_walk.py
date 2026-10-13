"""CVGP 08 — đo đi: tốc độ (hồi quy X theo thời gian video trên 11 đoạn) + nhịp khung (chu kỳ walk_0/1/2/1, thử 4–12 tick).
Chạy: python -P -E Tools/cvgp/simon_walk.py"""
import os
# chạy từ gốc repo; dữ liệu khớp mẫu ở ClaudeNotes/cvgp/_cache/08/fit_*.csv (sinh bằng Tools/cvgp/simon_fit.py — lệnh ở CVGP_MEMORY §2 "Simon")
os.chdir(os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'ClaudeNotes', 'cvgp', '_cache', '08'))

# ---- đồng hồ video + tốc độ
# Kiểm đồng hồ video: trong đoạn đi thẳng, X (cx+px) theo thời gian video (frame) — hồi quy + phần dư trên frame DUY NHẤT
import csv, sys
import numpy as np
rows = list(csv.DictReader(open('fit_walk.csv')))
wins = [(108,168),(214,259),(314,359),(369,429),(445,505),(1349,1399),(1733,1778),(1923,1978),(3077,3147),(4510,4565),(5508,5583)]
allres = []
for a, b in wins:
    pts = []
    prev = None
    for r in rows:
        f = int(r['frame'])
        if f < a or f > b or not r['px'] or r['cx'] == '':
            continue
        X = int(r['cx']) + int(r['px'])
        key = (X, r['sprite'], r['Y'])
        if key == prev:
            continue
        prev = key
        pts.append((f, X, float(r['err'])))
    P = np.array(pts, float)
    k, c = np.polyfit(P[:, 0], P[:, 1], 1)
    res = P[:, 1] - (k * P[:, 0] + c)
    allres += list(res)
    print('f%d-%d n=%d  toc do %.4f px/frame video = %.4f px/tick (x 30/60.0988)  du rms %.2f max %.2f' % (a, b, len(P), k, k * 30 / 60.0988, res.std(), abs(res).max()))
r = np.array(allres)
print('tong: rms %.2f px; |du|<=1: %.0f%%, <=2: %.0f%%, <=3: %.0f%%' % (r.std(), 100*np.mean(abs(r)<=1), 100*np.mean(abs(r)<=2), 100*np.mean(abs(r)<=3)))

# ---- nhịp khung
rows = list(csv.DictReader(open('fit_walk.csv')))
wins = [(108,168),(214,259),(314,359),(369,429),(445,505),(1349,1399),(1733,1778),(1923,1978),(3077,3147),(4510,4565),(5508,5583)]
K = {'idle_0': 0, 'walk_1': 1, 'walk_2': 2}
segs = []
for a, b in wins:
    pts = []; prev = None
    for r in rows:
        f = int(r['frame'])
        if f < a or f > b or not r['px'] or r['sprite'] not in K or float(r['err']) > 0.05: continue
        X = int(r['cx'] or 0) + int(r['px'])
        key = (X, r['sprite'])
        if key == prev: continue
        prev = key
        pts.append((X, K[r['sprite']]))
    segs.append(pts)
ORDER = [0, 1, 2, 1]
for N in range(4, 13):
    tot = 0; ok = 0
    for pts in segs:
        best = 0
        sgn = 1 if pts[-1][0] > pts[0][0] else -1
        for ph in range(4 * N):
            m = sum(1 for X, k in pts if ORDER[((sgn * X + ph) // N) % 4] == k)
            best = max(best, m)
        ok += best; tot += len(pts)
    print('N=%2d: khop %d/%d = %.1f%%' % (N, ok, tot, 100.0 * ok / tot))
