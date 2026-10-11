# attack_strip.py - day TUNG frame video quanh 1 don danh (crop quanh Simon, phong to), danh dau frame lap (D)
# Chay: python -P -E tools/attack_strip.py <video> <f0> <f1> <out.png> [half_w=48] [scale=3]
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
video, f0, f1, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), sys.argv[4]
hw = int(sys.argv[5]) if len(sys.argv) > 5 else 48
S_ = int(sys.argv[6]) if len(sys.argv) > 6 else 3
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
sim = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
s = next((sim[i] for i in range(f0, f1 + 1) if sim[i]['found'] == '1'), None)
xc = float(s['x_med']); yc = (int(s['y0']) + int(s['y1'])) / 2
x0 = int(min(max(xc - hw, 0), 256 - 2 * hw)); y0 = int(min(max(yc - 26, 8), 232 - 48))
n = f1 - f0 + 1; cols = min(n, 12); rows = (n + cols - 1) // cols
tw, th = 2 * hw * S_, 48 * S_
img = Image.new('RGB', (cols * (tw + 2), rows * (th + 13)), (50, 50, 50)); d = ImageDraw.Draw(img)
for k in range(n):
    i = f0 + k
    a = np.asarray(mm[i][y0 - 8:y0 - 8 + 48, x0:x0 + 2 * hw])
    x = (k % cols) * (tw + 2); y = (k // cols) * (th + 13)
    img.paste(Image.fromarray(a).resize((tw, th), Image.NEAREST), (x, y + 13))
    d.text((x + 2, y + 1), f'f{i}{" D" if dup[i] else ""}', fill=(255, 80, 80) if dup[i] else (255, 255, 0))
img.save(out); print('saved', out, img.size, 'crop x', x0, 'y', y0)
