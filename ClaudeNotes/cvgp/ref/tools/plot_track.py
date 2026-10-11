# plot_track.py - ve do thi (PIL) quy dao Simon: y dinh dau (y0), y chan (y1), x man hinh, camera cx theo thoi gian
# Chay: python -P -E tools/plot_track.py map1 <t0> <t1> <out.png> [px_per_s]
import sys, os, csv
from PIL import Image, ImageDraw
video, t0, t1, out = sys.argv[1], float(sys.argv[2]), float(sys.argv[3]), sys.argv[4]
pps = float(sys.argv[5]) if len(sys.argv) > 5 else 40
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = [r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv'))) if t0 <= float(r['t']) <= t1]
W = int((t1 - t0) * pps) + 60; PH = 230
img = Image.new('RGB', (W, PH * 3 + 30), (20, 20, 20)); d = ImageDraw.Draw(img)
def X(t): return 50 + (t - t0) * pps
# panel 1: y (NES 48..232, inverted so up is up)
def Y1(y): return 10 + (y - 40) * 1.1
d.text((2, 2), 'y0 head (cyan) y1 feet (yellow)', fill=(200, 200, 200))
for gy in range(48, 232, 16):
    d.line([(50, Y1(gy)), (W, Y1(gy))], fill=(50, 50, 50)); d.text((2, Y1(gy) - 5), str(gy), fill=(120, 120, 120))
# panel 2: screen x 0..256
def Y2(x): return PH + 10 + (256 - x) * 0.82
for gx in range(0, 257, 32):
    d.line([(50, Y2(gx)), (W, Y2(gx))], fill=(50, 50, 50)); d.text((2, Y2(gx) - 5), str(gx), fill=(120, 120, 120))
d.text((2, PH + 2), 'screen x_med (orange)', fill=(200, 200, 200))
# panel 3: camera cx (scaled to min..max)
cxs = [int(r['cx']) for r in S if r['cx'] not in ('', '-1')]
lo, hi = (min(cxs), max(cxs)) if cxs else (0, 1)
hi = max(hi, lo + 1)
def Y3(c): return 2 * PH + 20 + (hi - c) * (PH - 30) / (hi - lo)
d.text((2, 2 * PH + 2), f'camera cx {lo}..{hi} (green), world x (white, same scale offset)', fill=(200, 200, 200))
t = t0
while t <= t1:
    for yy in (0, PH, 2 * PH):
        d.line([(X(t), yy + 10), (X(t), yy + PH)], fill=(45, 45, 45))
    d.text((X(t) + 2, 3 * PH + 12), f'{int(t//60)}:{t%60:04.1f}', fill=(160, 160, 160))
    t += 1.0
prev = {}
for r in S:
    tt = float(r['t'])
    if r['found'] == '1':
        for key, fy, col in (('y0', Y1, (0, 220, 220)), ('y1', Y1, (240, 220, 0)), ('x_med', Y2, (255, 140, 0))):
            v = float(r[key]); p = (X(tt), fy(v))
            if key in prev and tt - prev[key][2] < 0.2: d.line([prev[key][:2], p], fill=col)
            else: d.point(p, fill=col)
            prev[key] = (p[0], p[1], tt)
    if r['cx'] not in ('', '-1'):
        p = (X(tt), Y3(int(r['cx'])))
        if 'cx' in prev and tt - prev['cx'][2] < 0.2: d.line([prev['cx'][:2], p], fill=(0, 200, 0))
        prev['cx'] = (p[0], p[1], tt)
img.save(out); print('saved', out, img.size)
