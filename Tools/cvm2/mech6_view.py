# mech6_view.py (CVM2 06) - ghep frame luoi NES (bo dem map2_nes.npy cua CVM2 02) thanh 1 tam de xem bang mat
# Chay (tu goc repo): python -P -E Tools/cvm2/mech6_view.py <out.png> <f0> <f1> <buoc> [x0 x1 y0 y1 (NES)] [scale]
import sys, os
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
out = sys.argv[1]; f0, f1, st = int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4])
x0, x1, y0, y1 = (int(v) for v in sys.argv[5:9]) if len(sys.argv) > 8 else (0, 256, 8, 232)
sc = int(sys.argv[9]) if len(sys.argv) > 9 else 2
mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
fr = list(range(f0, f1 + 1, st))
w, h = (x1 - x0) * sc, (y1 - y0) * sc
cols = max(1, min(len(fr), 1600 // (w + 4)))
rows = (len(fr) + cols - 1) // cols
img = Image.new('RGB', (cols * (w + 4), rows * (h + 14)), (40, 40, 40))
d = ImageDraw.Draw(img)
for k, f in enumerate(fr):
    a = np.asarray(mm[f])[y0 - 8:y1 - 8, x0:x1]
    t = Image.fromarray(a).resize((w, h), Image.NEAREST)
    cx, cy = (k % cols) * (w + 4), (k // cols) * (h + 14)
    img.paste(t, (cx, cy + 12))
    d.text((cx + 2, cy), 'f%d %.2fs' % (f, f / 30.0), fill=(255, 255, 0))
img.save(out)
print(out, len(fr), 'frames', img.size)
