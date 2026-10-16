# event_triplets2.py (CVM2 02) - mỗi sự kiện HUD: 3 frame (f-10, f-3, f+3) crop 144x96 quanh Simon, phóng 2x, kèm nhãn. Khuôn event_triplets.py của Map 1.
# Chạy: python -P -E Tools/cvm2/event_triplets2.py map2 <out_prefix> <label:frame> ...
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK
video, pre = sys.argv[1], sys.argv[2]
items = [a.rsplit(':', 1) for a in sys.argv[3:]]
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
sim = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_simon.csv')))}
CW, CH, S = 144, 96, 2
OFF = (-10, -3, 3)
per_img = 12
def simpos(f):
    for g in (f - 3, f - 6, f - 10, f, f + 3, f - 15, f + 8):
        s = sim.get(g)
        if s and s['found'] == '1': return int(s['px']), int(s['Y']) - 16
    return 128, 150
for p in range(0, len(items), per_img):
    chunk = items[p:p + per_img]
    cols = 2; rows = (len(chunk) + 1) // 2
    ew = len(OFF) * (CW * S + 2) + 10
    img = Image.new('RGB', (cols * ew, rows * (CH * S + 14)), (40, 40, 40)); d = ImageDraw.Draw(img)
    for k, (lab, fs) in enumerate(chunk):
        f0 = int(fs)
        xc, yc = simpos(f0)
        x0 = int(min(max(xc - CW / 2, 0), 256 - CW)); y0 = int(min(max(yc - CH / 2 - 6, 48), 232 - CH))
        ex = (k % cols) * ew; ey = (k // cols) * (CH * S + 14)
        d.text((ex + 2, ey + 1), '%s @f%d %d:%05.2f (f%d,f%d,f%d)' % (lab, f0, f0 / 30 // 60, f0 / 30 % 60, f0 + OFF[0], f0 + OFF[1], f0 + OFF[2]), fill=(255, 255, 0))
        for j, o in enumerate(OFF):
            a = np.asarray(mm[min(max(f0 + o, 0), mm.shape[0] - 1)])[y0 - 8:y0 - 8 + CH, x0:x0 + CW]
            img.paste(Image.fromarray(a).resize((CW * S, CH * S), Image.NEAREST), (ex + j * (CW * S + 2), ey + 14))
    img.save('%s_%02d.png' % (pre, p // per_img))
print(len(items), 'events', (len(items) + per_img - 1) // per_img, 'images')
