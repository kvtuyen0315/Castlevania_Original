# verify_calib.py - kiem tra he so hieu chuan bang luoi NES tren HUD:
#  (1) in ASCII 8x8 cac o chu 'SCORE' (NES x 0..39, y 16..23) lay mau dung tam pixel NES -> glyph phai net, dung hinh
#  (2) "do net": % mau tai tam pixel NES roi vao vung toi (<60) hoac sang (>170) tren vung chu HUD (y 16..39, x 0..55)
#      va tren dong gach san; so sanh khi dich luoi +-0.25/0.5 px NES -> cuc dai phai o (0,0)
# Chay: python -P -E tools/verify_calib.py <map1|boss> <frame.png>
import sys, os
import numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import CALIB
video, path = sys.argv[1], sys.argv[2]
cal = CALIB[video]
im = np.asarray(Image.open(path).convert('RGB')).astype(int)
H, W, _ = im.shape
def sample(x0, x1, y0, y1, dx=0.0, dy=0.0):
    xs = cal['ox'] + cal['sx'] * (np.arange(x0, x1) + 0.5 + dx)
    ys = cal['oy'] + cal['sy'] * (np.arange(y0, y1) + 0.5 + dy)
    xi = np.clip(np.floor(xs).astype(int), 0, W - 1); yi = np.clip(np.floor(ys).astype(int), 0, H - 1)
    return im[yi][:, xi]
s = sample(0, 40, 16, 24)
print('ASCII glyph "SCORE" (NES x0..39, y16..23), # = trang:')
for row in s:
    print('   ' + ''.join('#' if p.min() > 128 else '.' for p in row))
def crisp(a):
    m = a.max(axis=2)
    return float(((m < 60) | (m > 170)).mean())
print('do net vung chu HUD (x0..55,y16..39) theo do dich luoi (px NES):')
best = None
for dy in (-0.5, -0.25, 0, 0.25, 0.5):
    line = []
    for dx in (-0.5, -0.25, 0, 0.25, 0.5):
        c = crisp(sample(0, 56, 16, 40, dx, dy))
        line.append(f'{c:5.3f}')
        if best is None or c > best[0]: best = (c, dx, dy)
    print(f'  dy={dy:+.2f}: ' + ' '.join(line) + '   (cot dx = -0.5 -0.25 0 +0.25 +0.5)')
print(f'  cuc dai: {best[0]:.3f} tai dx={best[1]:+.2f}, dy={best[2]:+.2f}')
