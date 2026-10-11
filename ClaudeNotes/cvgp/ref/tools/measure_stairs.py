# measure_stairs.py - toc do len/xuong cau thang: hoi quy y_chan (y1), y_dau (y0) va world_x theo thoi gian
# trong cac doan chon tay (Simon dang di lien tuc tren cau thang). Chay: python -P -E tools/measure_stairs.py map1 f0 f1 [f0 f1 ...]
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
NES = 60.0988
args = list(map(int, sys.argv[2:]))
for a, b in zip(args[::2], args[1::2]):
    t, y1, y0, wx = [], [], [], []
    for f in range(a, b + 1):
        r = S[f]
        if r['found'] != '1' or not r['world_x']: continue
        t.append(f / 30); y1.append(int(r['y1'])); y0.append(int(r['y0'])); wx.append(float(r['world_x']))
    t = np.array(t)
    out = []
    for name, v in (('y1', y1), ('y0', y0), ('wx', wx)):
        k, c = np.polyfit(t, np.array(v, float), 1)
        out.append(f'{name}: {k:7.2f} px/s = {k/NES:+.3f} px/tick')
    print(f'f{a}-{b} ({a/30//60:.0f}:{a/30%60:05.2f}-{b/30//60:.0f}:{b/30%60:05.2f}, {(b-a)/30:.2f}s): ' + ' | '.join(out) +
          f' | dy tong {y1[-1]-y1[0]} px, dx tong {wx[-1]-wx[0]:.0f} px')
