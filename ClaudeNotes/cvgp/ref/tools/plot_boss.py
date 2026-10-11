# plot_boss.py - ve quy dao Phantom Bat (x,y NES, do rong canh) + Simon theo thoi gian trong phong boss
# Chay: python -P -E tools/plot_boss.py <bat.csv> <simon.csv> <t0> <t1> <out.png> [px_per_s]
import sys, csv
from PIL import Image, ImageDraw
batf, simf, t0, t1, out = sys.argv[1], sys.argv[2], float(sys.argv[3]), float(sys.argv[4]), sys.argv[5]
pps = float(sys.argv[6]) if len(sys.argv) > 6 else 70
B = [r for r in csv.DictReader(open(batf)) if t0 <= float(r['t']) <= t1]
S = [r for r in csv.DictReader(open(simf)) if t0 <= float(r['t']) <= t1]
W = int((t1 - t0) * pps) + 70; PH = 260
img = Image.new('RGB', (W, PH * 2 + 40), (18, 18, 18)); d = ImageDraw.Draw(img)
def X(t): return 60 + (t - t0) * pps
def YX(v): return 10 + (256 - v) * (PH - 20) / 256          # panel x (0..256, 0 o duoi)
def YY(v): return PH + 20 + (v - 48) * (PH - 20) / 184      # panel y (48..232, xuong duoi)
for gx in range(0, 257, 32):
    d.line([(60, YX(gx)), (W, YX(gx))], fill=(45, 45, 45)); d.text((2, YX(gx) - 5), f'x{gx}', fill=(130, 130, 130))
for gy in range(48, 233, 16):
    d.line([(60, YY(gy)), (W, YY(gy))], fill=(45, 45, 45)); d.text((2, YY(gy) - 5), f'y{gy}', fill=(130, 130, 130))
t = int(t0)
while t <= t1:
    for yy in (10, PH + 20):
        d.line([(X(t), yy), (X(t), yy + PH - 20)], fill=(55, 55, 55))
    d.text((X(t) + 2, 2 * PH + 22), f'{t//60}:{t%60:02d}' if t >= 60 else f'{t}s', fill=(170, 170, 170))
    t += 1
d.text((62, 0), 'X: bat (red, thickness=wing), Simon (orange)', fill=(220, 220, 220))
d.text((62, PH + 8), 'Y: bat center (red), bat top/bottom (dark red), Simon head/feet (orange)', fill=(220, 220, 220))
pv = {}
def plot(key, tt, p, col):
    if key in pv and tt - pv[key][2] < 0.25: d.line([pv[key][:2], p], fill=col, width=2 if 'bat' in key else 1)
    else: d.point(p, fill=col)
    pv[key] = (p[0], p[1], tt)
for r in B:
    if r['found'] != '1': continue
    tt = float(r['t'])
    plot('batx', tt, (X(tt), YX(float(r['xc']))), (255, 40, 40))
    plot('baty', tt, (X(tt), YY(float(r['yc']))), (255, 40, 40))
    plot('batt', tt, (X(tt), YY(float(r['y0']))), (130, 20, 20))
    plot('batb', tt, (X(tt), YY(float(r['y1']))), (130, 20, 20))
    plot('batw', tt, (X(tt), YX(float(r['x0']))), (110, 30, 30)); plot('batw2', tt, (X(tt), YX(float(r['x1']))), (110, 30, 30))
for r in S:
    if r['found'] != '1': continue
    tt = float(r['t'])
    plot('sx', tt, (X(tt), YX(float(r['x_med']))), (255, 150, 0))
    plot('sy0', tt, (X(tt), YY(float(r['y0']))), (255, 150, 0))
    plot('sy1', tt, (X(tt), YY(float(r['y1']))), (200, 110, 0))
img.save(out); print('saved', out, img.size)
