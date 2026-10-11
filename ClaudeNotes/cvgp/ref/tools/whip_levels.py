# whip_levels.py - tam voi roi theo cap: dau roi (tip) so voi tam Simon (x_med), lay max moi don, trung vi theo cap
# Cap roi theo thoi gian Map_1: cap1 < f194 (truoc lan nang 1), cap2 f217..f292, cap3 > f316
# Chay: python -P -E tools/whip_levels.py map1
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
W = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_whip.csv')))}
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
N = len(S)
def level(f): return 1 if f < 194 else (2 if f < 293 else 3)
atk = []; cur = None
for f in range(N):
    w = W[f]; s = S[f]
    if w['ext_l'] == '' or s['found'] != '1': 
        if cur: atk.append(cur); cur = None
        continue
    el, er = int(w['ext_l']), int(w['ext_r'])
    x0, x1, xm = int(s['x0']), int(s['x1']), float(s['x_med'])
    h = int(s['y1']) - int(s['y0']) + 1
    if max(el, er) >= 18 and h >= 20:
        tip = (x1 + 2 + er - 1 - xm) if er >= el else (xm - (x0 - 2 - el + 1))
        if cur and f - cur['f1'] <= 2: cur['tips'].append(tip); cur['f1'] = f
        else:
            if cur: atk.append(cur)
            cur = dict(f0=f, f1=f, tips=[tip])
    else:
        if cur and f - cur['f1'] > 2: atk.append(cur); cur = None
if cur: atk.append(cur)
res = {1: [], 2: [], 3: []}
for a in atk:
    res[level(a['f0'])].append(max(a['tips']))
for L in (1, 2, 3):
    v = np.array(res[L])
    if len(v): print(f'cap {L}: {len(v)} don; dau roi cach tam Simon: trung vi {np.median(v):.0f} px, p25 {np.percentile(v,25):.0f}, p75 {np.percentile(v,75):.0f}, max {v.max():.0f}')
