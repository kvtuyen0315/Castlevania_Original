"""CVGP 09 — dải ảnh frame video (lấy mẫu về lưới NES — hiệu chuẩn MAP1_TIMELINE §1) để xem/so khung theo thời gian.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/nes_strip.py <map1|boss> f0 f1 x0 y0 w h <out.png> [scale=4] [all]
  - vùng (x0,y0,w,h) = px màn NES; chỉ frame MỚI (bỏ frame lặp) trừ khi thêm 'all'; mỗi ô ghi số frame video.
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

Y0 = 8   # hàng NES đầu lấy mẫu (map1 thấy y 8–231)


def frames(video, f0, f1):
    ss = max(0.0, (f0 - 0.5) / 30.0)
    f = f0
    for img in nc.read_frames(video, ss=ss, t=(f1 - f0 + 1) / 30.0):
        if f > f1:
            break
        yield f, nc.nes_sample(img, nc.CALIB[video], y0=Y0, y1=240 - Y0)
        f += 1


def main():
    video, f0, f1, x0, y0, w, h, out = sys.argv[1], *map(int, sys.argv[2:8]), sys.argv[8]
    scale = int(sys.argv[9]) if len(sys.argv) > 9 else 4
    keep_all = 'all' in sys.argv[10:]
    dup = np.load(os.path.join(VID_DIR, 'work', video + '_dup.npy'))
    tiles = []
    for f, nes in frames(video, f0, f1):
        if not keep_all and f < len(dup) and dup[f]:
            continue
        crop = np.clip(nes[y0 - Y0:y0 - Y0 + h, x0:x0 + w], 0, 255).astype(np.uint8)
        im = Image.fromarray(crop).resize((w * scale, h * scale), Image.NEAREST)
        canvas = Image.new('RGB', (w * scale, h * scale + 12), (40, 40, 40))
        canvas.paste(im, (0, 12))
        ImageDraw.Draw(canvas).text((2, 0), 'f%d' % f, fill=(255, 255, 0))
        tiles.append(canvas)
    cols = min(len(tiles), max(1, 1200 // (w * scale + 4)))
    rows = (len(tiles) + cols - 1) // cols
    tw, th = w * scale + 4, h * scale + 16
    sheet = Image.new('RGB', (cols * tw, rows * th), (0, 0, 0))
    for i, t in enumerate(tiles):
        sheet.paste(t, ((i % cols) * tw, (i // cols) * th))
    sheet.save(out)
    print('ghi', out, len(tiles), 'o')


if __name__ == '__main__':
    main()
