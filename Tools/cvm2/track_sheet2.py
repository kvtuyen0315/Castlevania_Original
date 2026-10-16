# track_sheet2.py (CVM2 02) - ghép thumbnail từng vết (frame giữa, crop 48x48 NES phóng 2x) kèm id + nhãn đoán + thời gian, để xác nhận bằng mắt
# Chạy: python -P -E Tools/cvm2/track_sheet2.py map2 <out_prefix> [min_nfr=6] [per_sheet=40]
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK
video, pre = sys.argv[1], sys.argv[2]
mn = int(sys.argv[3]) if len(sys.argv) > 3 else 6
per = int(sys.argv[4]) if len(sys.argv) > 4 else 40
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
T = [t for t in csv.DictReader(open(os.path.join(WORK, video + '_tracks.csv'))) if int(t['nfr']) >= mn]
cl = {int(r['id']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_track_class.csv')))}
pts = {}
for r in csv.DictReader(open(os.path.join(WORK, video + '_track_pts.csv'))): pts.setdefault(int(r['id']), []).append(r)
S = 2; C = 48; CW = C * S
cols = 8
for sh in range(0, len(T), per):
    chunk = T[sh:sh + per]
    rows = (len(chunk) + cols - 1) // cols
    img = Image.new('RGB', (cols * (CW + 2), rows * (CW + 26)), (40, 40, 40)); d = ImageDraw.Draw(img)
    for k, t in enumerate(chunk):
        tid = int(t['id']); P = pts[tid]; q = P[len(P) // 2]; f = int(q['f'])
        cx = (int(q['x0']) + int(q['x1'])) // 2; cy = (int(q['y0']) + int(q['y1'])) // 2
        x0 = min(max(cx - C // 2, 0), 256 - C); y0 = min(max(cy - C // 2, 40), 232 - C)
        a = np.asarray(mm[f])[y0 - 8:y0 - 8 + C, x0:x0 + C]
        im = Image.fromarray(a).resize((CW, CW), Image.NEAREST)
        x = (k % cols) * (CW + 2); y = (k // cols) * (CW + 26)
        img.paste(im, (x, y + 24))
        c = cl.get(tid)
        lab = '%s %s' % (c['l1'], c['s1']) if c else '?'
        d.text((x + 1, y), '#%d f%d %d:%04.1f n%s%s' % (tid, f, f / 30 // 60, f / 30 % 60, t['nfr'], ' ST' if t['static'] == '1' else ''), fill=(255, 255, 0))
        d.text((x + 1, y + 11), lab[:26], fill=(120, 255, 120))
    img.save('%s_%02d.png' % (pre, sh // per))
print(len(T), 'tracks ->', (len(T) + per - 1) // per, 'sheets')
