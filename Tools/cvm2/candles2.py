# candles2.py (CVM2 02) - nến tường (blob đỏ 8x14 gần như đứng yên theo toạ độ vùng) bị phá giữa màn + vật rơi ngay sau đó. Dùng tracks2/classify2.
# Chạy: python -P -E Tools/cvm2/candles2.py map2 -> work/map2_candles.txt
import sys, os, csv
import numpy as np
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK, load_camera
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'
T = {int(t['id']): t for t in csv.DictReader(open(os.path.join(WORK, video + '_tracks.csv')))}
C = {int(r['id']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_track_class.csv')))}
P = {}
for r in csv.DictReader(open(os.path.join(WORK, video + '_track_pts.csv'))): P.setdefault(int(r['id']), []).append(r)
cam = load_camera(video)
def ts(f): t = f / 30; return '%d:%05.2f' % (t // 60, t % 60)
cands = []
for i, t in T.items():
    if int(t['nfr']) < 12: continue
    w, h = int(t['w']), int(t['h']); nm = float(t['n_mean'])
    red = float(t['red']) / max(nm, 1)
    if not (6 <= w <= 10 and 11 <= h <= 16 and red >= 0.25 and nm < 100): continue
    p = P[i]
    xs = np.array([float(q['wx']) for q in p]); ys = np.array([float(q['wy']) for q in p])
    if np.ptp(xs) > 6 or np.ptp(ys) > 24: continue
    last = p[-1]; f1 = int(last['f'])
    scr = (int(last['x0']) + int(last['x1'])) / 2
    nxt = cam.get(f1 + 6)
    inside = 12 <= scr <= 243 and nxt is not None and nxt['kind'] == 'game' and nxt['section'] == cam[f1]['section']
    cands.append((int(t['f0']), i, t['section'], float(np.median(xs)), float(np.median(ys)), f1, inside))
cands.sort()
print('candle-like tracks', len(cands))
# drops: tracks starting shortly after candle end near its position
drops = []
for cf0, ci, sec, wx, wy, f1, inside in cands:
    hit = None
    for j, t in T.items():
        if j == ci or t['section'] != sec: continue
        f0 = int(t['f0'])
        if not (f1 - 3 <= f0 <= f1 + 30): continue
        if abs(float(t['wx0']) - wx) <= 16 and abs(float(t['wy0']) - wy) <= 26 and int(t['nfr']) >= 2:
            lab = C.get(j, {}).get('l1', '?') + ' ' + C.get(j, {}).get('s1', '')
            if hit is None or f0 < hit[1]: hit = (j, f0, int(t['f1']), lab, float(t['wx0']), float(t['wy0']), float(t['wx1']), float(t['wy1']), t['w'], t['h'], t['or'], t['cr'], t['red'], t['pur'], t['blu'], t['wht'])
    drops.append(hit)
out = []
for (cf0, ci, sec, wx, wy, f1, inside), d in zip(cands, drops):
    s = '#%d %s candle wx %.0f wy %.0f seen %s-%s %s' % (ci, sec, wx, wy, ts(cf0), ts(f1), 'BROKEN-ON-SCREEN' if inside else 'left-view')
    if d: s += ' -> drop #%d f%d-%d %s w%sh%s (%.0f,%.0f)->(%.0f,%.0f) col or%s cr%s red%s pur%s blu%s wht%s' % (d[0], d[1], d[2], d[3], d[8], d[9], d[4], d[5], d[6], d[7], d[10], d[11], d[12], d[13], d[14], d[15])
    out.append(s)
open(os.path.join(WORK, video + '_candles.txt'), 'w', encoding='utf-8').write('\n'.join(out))
print('\n'.join(out))
