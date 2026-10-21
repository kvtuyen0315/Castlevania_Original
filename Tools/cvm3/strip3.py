# strip3.py (CVM3 02) - ghép dải khung NES (lưới hiệu chuẩn) có nhãn frame/giây -> PNG để xem bằng mắt.
# Chạy (từ gốc repo): python -P -E Tools/cvm3/strip3.py <map3|boss3> <out.png> <a-b:step | f1,f2,...> [x0,y0,x1,y1 = 0,40,256,224] [scale=2] [cols=4]
import sys, os
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
video, out, spec = sys.argv[1], sys.argv[2], sys.argv[3]
x0, y0, x1, y1 = (int(v) for v in (sys.argv[4] if len(sys.argv) > 4 else '0,40,256,224').split(','))
S = int(sys.argv[5]) if len(sys.argv) > 5 else 2
cols = int(sys.argv[6]) if len(sys.argv) > 6 else 4
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
if ':' in spec:
    r, st = spec.split(':'); a, b = (int(v) for v in r.split('-')); fs = list(range(a, b + 1, int(st)))
else:
    fs = [int(v) for v in spec.split(',')]
w, h = (x1 - x0) * S, (y1 - y0) * S
rows = (len(fs) + cols - 1) // cols
img = Image.new('RGB', (cols * (w + 3), rows * (h + 12)), (30, 30, 30))
d = ImageDraw.Draw(img)
for k, f in enumerate(fs):
    fr = np.asarray(mm[f])                      # NES y 8..231
    crop = fr[y0 - 8:y1 - 8, x0:x1]
    X = (k % cols) * (w + 3); Y = (k // cols) * (h + 12)
    img.paste(Image.fromarray(crop).resize((w, h), Image.NEAREST), (X, Y + 12))
    d.text((X + 2, Y), 'f%d %.2fs' % (f, f / 30), fill=(255, 255, 0))
img.save(out)
print('saved', out, img.size)
