# nstrip.py - day frame NES (tu memmap) de xem nhanh 1 doan; co the crop vung NES
# Chay: python -P -E tools/nstrip.py <video> <f0> <count> <step> <cols> <scale> <out.png> [x0 y0 x1 y1 (NES)]
import sys, os
import numpy as np
from PIL import Image, ImageDraw
video, f0, cnt, step, cols, scale, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5]), float(sys.argv[6]), sys.argv[7]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
Y0 = 8
if len(sys.argv) > 8:
    x0, y0, x1, y1 = map(int, sys.argv[8:12])
else:
    x0, y0, x1, y1 = 0, 8, 256, 232
w, h = int((x1 - x0) * scale), int((y1 - y0) * scale)
rows = (cnt + cols - 1) // cols
img = Image.new('RGB', (cols * (w + 2), rows * (h + 13)), (60, 60, 60))
d = ImageDraw.Draw(img)
for k in range(cnt):
    i = f0 + k * step
    if i >= mm.shape[0]: break
    a = np.asarray(mm[i][y0 - Y0:y1 - Y0, x0:x1])
    im = Image.fromarray(a).resize((w, h), Image.NEAREST)
    x = (k % cols) * (w + 2); y = (k // cols) * (h + 13)
    img.paste(im, (x, y + 13))
    t = i / 30
    d.text((x + 2, y + 1), f'f{i} {int(t//60)}:{t%60:05.2f}', fill=(255, 255, 0))
img.save(out); print('saved', out, img.size)
