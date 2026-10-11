# strip.py - ghep day frame (full-res video, thu nho) de xem 1 doan ngan
# Chay: python -P -E tools/strip.py <video> <frame_start> <count> <cols> <scale> <out.png> [step]
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *
from PIL import ImageDraw
video, f0, cnt, cols, scale, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), float(sys.argv[5]), sys.argv[6]
step = int(sys.argv[7]) if len(sys.argv) > 7 else 1
path, W, H = VID[video]
ss = f0 / 30.0
frames = []
for k, fr in enumerate(read_frames(video, ss=ss, t=(cnt * step + 2) / 30.0)):
    if k % step == 0:
        frames.append(fr)
    if len(frames) >= cnt: break
tw, th = int(W * scale), int(H * scale)
rows = (len(frames) + cols - 1) // cols
img = Image.new('RGB', (cols * tw, rows * (th + 14)), (50, 50, 50))
d = ImageDraw.Draw(img)
for k, fr in enumerate(frames):
    im = Image.fromarray(fr).resize((tw, th), Image.BILINEAR)
    x = (k % cols) * tw; y = (k // cols) * (th + 14)
    img.paste(im, (x, y + 14))
    fi = f0 + k * step; t = fi / 30
    d.text((x + 2, y + 1), f'f{fi} {int(t//60)}:{t%60:05.2f}', fill=(255, 255, 0))
img.save(out); print('saved', out, img.size, len(frames), 'frames')
