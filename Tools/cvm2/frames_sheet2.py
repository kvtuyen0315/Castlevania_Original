# frames_sheet2.py (CVM2 02) - ghép các frame (lưới NES, full 224 dòng hoặc crop) thành 1 ảnh để xem, kèm nhãn frame/thời gian
# Chạy: python -P -E Tools/cvm2/frames_sheet2.py <video> <out.png> <cols> <scale> <f1> <f2> ...   (hoặc range a:b:step)
#   crop tuỳ chọn qua biến môi trường CROP="x0,y0,x1,y1" (toạ độ NES)
import sys, os
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
video, out, cols, S = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
fr = []
for a in sys.argv[5:]:
    if ':' in a:
        p = [int(x) for x in a.split(':')]
        fr += list(range(p[0], p[1] + 1, p[2] if len(p) > 2 else 1))
    else:
        fr.append(int(a))
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
crop = os.environ.get('CROP')
x0, y0, x1, y1 = (0, 8, 256, 232) if not crop else [int(v) for v in crop.split(',')]
w, h = (x1 - x0) * S, (y1 - y0) * S
rows = (len(fr) + cols - 1) // cols
img = Image.new('RGB', (cols * (w + 3), rows * (h + 15)), (50, 50, 50))
d = ImageDraw.Draw(img)
for k, f in enumerate(fr):
    a = np.asarray(mm[f])[y0 - 8:y1 - 8, x0:x1]
    im = Image.fromarray(a).resize((w, h), Image.NEAREST)
    x = (k % cols) * (w + 3); y = (k // cols) * (h + 15)
    img.paste(im, (x, y + 13))
    t = f / 30
    d.text((x + 2, y + 1), 'f%d %d:%05.2f' % (f, t // 60, t % 60), fill=(255, 255, 0))
img.save(out)
print(out, img.size)
