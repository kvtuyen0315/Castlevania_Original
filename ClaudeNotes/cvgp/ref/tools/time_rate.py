# time_rate.py - toc do dem nguoc cua dong ho TIME tren HUD: hoi quy thoi diem doi so (frame video) theo gia tri TIME
# Chay: python -P -E tools/time_rate.py map1 f0 f1 [f0 f1 ...]
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
H = [r for r in csv.DictReader(open(os.path.join(base, f'{video}_hud.csv'))) if r['hud_ok'] == '1' and '?' not in r['time']]
args = list(map(int, sys.argv[2:]))
allk = []
for a, b in zip(args[::2], args[1::2]):
    ch = []; prev = None
    for r in H:
        f = int(r['frame'])
        if f < a or f > b: continue
        if r['time'] != prev:
            if prev is not None: ch.append((f, int(r['time'])))
            prev = r['time']
    ch = np.array(ch, float)
    k, c = np.polyfit(ch[:, 1], ch[:, 0], 1)     # frame theo gia tri TIME (k am)
    res = ch[:, 0] - (k * ch[:, 1] + c)
    print(f'f{a}-{b}: {len(ch)} lan doi, {-k:.3f} frame video / don vi TIME = {-k/30:.4f} s = {-k/30*60.0988:.2f} tick NES ; lech max {abs(res).max():.1f} frame')
    allk.append((-k, len(ch)))
