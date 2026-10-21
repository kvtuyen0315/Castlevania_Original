# -*- coding: utf-8 -*-
"""accept12_montage.py (CVM3 12) - ghep anh Game view Unity (1920x1080, vung game 1024x960 tai (448,60) = NES 256x240 x4) thanh 1 tam xem nhanh.

Chay (tu goc repo): python -P -E Tools/cvm3/accept12_montage.py <thu muc anh> <anh ra> [so cot] [loc ten (chuoi con)]
"""
import os, sys
from PIL import Image, ImageDraw

src, out = sys.argv[1], sys.argv[2]
cols = int(sys.argv[3]) if len(sys.argv) > 3 else 4
filt = sys.argv[4] if len(sys.argv) > 4 else ''
files = sorted(f for f in os.listdir(src) if f.endswith('.png') and filt in f)
tiles = []
for f in files:
    im = Image.open(os.path.join(src, f)).convert('RGB')
    if im.size == (1920, 1080):
        im = im.crop((448, 60, 448 + 1024, 60 + 960))
    im = im.resize((384, 360), Image.NEAREST)
    t = Image.new('RGB', (384, 376), (40, 40, 40))
    t.paste(im, (0, 16))
    ImageDraw.Draw(t).text((3, 2), f[:-4], fill=(255, 255, 0))
    tiles.append(t)
rows = (len(tiles) + cols - 1) // cols
m = Image.new('RGB', (cols * 388, rows * 380), (0, 0, 0))
for i, t in enumerate(tiles):
    m.paste(t, ((i % cols) * 388, (i // cols) * 380))
m.save(out)
print('%d anh -> %s' % (len(tiles), out))
