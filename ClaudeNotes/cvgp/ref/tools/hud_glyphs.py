# hud_glyphs.py - gom cac mau o chu so HUD (8x8 nhi phan) tu memmap, in thong ke + montage de dan nhan
# Chay: python -P -E tools/hud_glyphs.py map1 frames/glyphs_map1.png
import sys, os
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from hud_cells import *
video, out = sys.argv[1], sys.argv[2]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
N = int(np.load(os.path.join(base, f'{video}_meta.npz'))['n'])
y0 = 8 if video == 'map1' else 8
from collections import Counter, defaultdict
cnt = Counter(); first = {}
step = 3
for i in range(0, N, step):
    fr = np.asarray(mm[i])
    for name, (x, y) in CELLS.items():
        b = binar(cell(fr, x, y, y0))
        k = b.tobytes()
        cnt[k] += 1
        if k not in first: first[k] = (i, name)
keys = [k for k, c in cnt.most_common() if c >= 5]
print('distinct keys', len(cnt), 'with >=5 occurrences', len(keys))
S = 6
cols = 16
rows = (len(keys) + cols - 1) // cols
img = Image.new('RGB', (cols * (8 * S + 10), rows * (8 * S + 22)), (60, 0, 60))
d = ImageDraw.Draw(img)
for j, k in enumerate(keys):
    b = np.frombuffer(k, dtype=bool).reshape(8, 8)
    t = Image.fromarray((b * 255).astype(np.uint8)).resize((8 * S, 8 * S), Image.NEAREST).convert('RGB')
    x = (j % cols) * (8 * S + 10); y = (j // cols) * (8 * S + 22)
    img.paste(t, (x + 5, y + 18))
    d.text((x + 2, y + 2), f'{j}:{cnt[k]}', fill=(255, 255, 0))
img.save(out)
np.save(os.path.join(base, f'{video}_glyphkeys.npy'), np.array([np.frombuffer(k, dtype=bool) for k in keys]))
for j, k in enumerate(keys):
    print(j, cnt[k], first[k])
