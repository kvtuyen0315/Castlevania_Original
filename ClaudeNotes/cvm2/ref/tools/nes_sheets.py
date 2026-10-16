# nes_sheets.py - lay mau luoi NES moi <step> giay, ghep luoi 6x4 co nhan thoi gian => work/sheets_<video>_<n>.png
# Chay: python -P -E tools/nes_sheets.py <video> <t0> <t1> <step> <tag>
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes2 import *
from PIL import ImageDraw
video, t0, t1, step, tag = sys.argv[1], float(sys.argv[2]), float(sys.argv[3]), float(sys.argv[4]), sys.argv[5]
cal = dict(CALIB[video])
if video == 'map2':
    cal = dict(ox=268.932, oy=-20.244, sx=3.623, sy=3.104)
tiles = []
for i, img in enumerate(read_frames(video, ss=t0, t=t1 - t0, fps='1/%g' % step if step >= 1 else str(1 / step))):
    t = t0 + i * step
    a = nes_sample(img, cal, 8, 232).clip(0, 255).astype('uint8')
    im = Image.fromarray(a)
    d = ImageDraw.Draw(im); d.rectangle((0, 214, 60, 223), fill=(0, 0, 0)); d.text((1, 213), '%.1f' % t, fill=(255, 255, 0))
    tiles.append(im)
cols, rows = 6, 4
for s in range(0, len(tiles), cols * rows):
    sh = Image.new('RGB', (cols * 258, rows * 226), (60, 60, 60))
    for j, im in enumerate(tiles[s:s + cols * rows]):
        sh.paste(im, ((j % cols) * 258, (j // cols) * 226))
    sh.save('work/sheet_%s_%s_%02d.png' % (video, tag, s // (cols * rows)))
print(len(tiles), 'frames')
