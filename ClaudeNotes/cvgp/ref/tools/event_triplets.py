# event_triplets.py - moi su kien HUD: 3 frame (f-10, f-3, f+3) crop 144x96 quanh Simon, phong 2x, kem nhan
# Chay: python -P -E tools/event_triplets.py map1 <out_prefix> <label:frame> ...   (label tuy y, vd +400)
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
video, pre = sys.argv[1], sys.argv[2]
items = [a.rsplit(':', 1) for a in sys.argv[3:]]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
sim = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
Y0 = 8; CW, CH, S = 144, 96, 2
OFF = (-10, -3, 3)
per_img = 12
for p in range(0, len(items), per_img):
    chunk = items[p:p + per_img]
    cols = 2
    rows = (len(chunk) + 1) // 2
    ew = len(OFF) * (CW * S + 2) + 10
    img = Image.new('RGB', (cols * ew, rows * (CH * S + 14)), (40, 40, 40))
    d = ImageDraw.Draw(img)
    for k, (lab, fs) in enumerate(chunk):
        f0 = int(fs)
        # vi tri crop: theo Simon o frame f0-3
        s = sim.get(f0 - 3)
        xc = float(s['x_med']) if s and s['found'] == '1' else 128
        yc = (int(s['y0']) + int(s['y1'])) / 2 if s and s['found'] == '1' else 160
        x0 = int(min(max(xc - CW / 2, 0), 256 - CW)); y0 = int(min(max(yc - CH / 2 - 6, 48), 232 - CH))
        ex = (k % cols) * ew; ey = (k // cols) * (CH * S + 14)
        d.text((ex + 2, ey + 1), f'{lab} @f{f0} {f0/30//60:.0f}:{f0/30%60:05.2f}  (tiles f{f0+OFF[0]}, f{f0+OFF[1]}, f{f0+OFF[2]})', fill=(255, 255, 0))
        for j, o in enumerate(OFF):
            i = f0 + o
            a = np.asarray(mm[i][y0 - Y0:y0 - Y0 + CH, x0:x0 + CW])
            img.paste(Image.fromarray(a).resize((CW * S, CH * S), Image.NEAREST), (ex + j * (CW * S + 2), ey + 14))
    out = f'{pre}_{p // per_img}.png'
    img.save(out); print('saved', out, img.size)
