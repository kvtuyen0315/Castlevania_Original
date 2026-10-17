"""CVM2 10 — dải ảnh lưới NES video Map_2 quanh 1 điểm thế giới (xem bằng mắt vật rơi / bí mật). Mỗi ô = vùng 64×64 NES quanh
(wx, ysec) ở frame f (camera ref/data/map2_camera.csv), phóng ×3, nhãn frame.

Chạy (từ gốc repo):  python -P -E Tools/cvm2/strip10.py <out.png> <wx> <ysec> <f0> <f1> [bước=2] [w=64] [h=64]
"""
import csv, os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')


def main():
    out, wx, ys, f0, f1 = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5])
    step = int(sys.argv[6]) if len(sys.argv) > 6 else 2
    W = int(sys.argv[7]) if len(sys.argv) > 7 else 64
    H = int(sys.argv[8]) if len(sys.argv) > 8 else 64
    cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')))}
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    tiles = []
    for f in range(f0, f1 + 1, step):
        c = cam.get(f)
        cx = int(float(c['cx'])) if c and c['kind'] == 'game' else 0
        sx, sy = wx - cx, ys + 32 - 8          # NES y = ysec + 32; bộ đệm bắt đầu ở hàng 8
        img = np.zeros((H, W, 3), np.uint8)
        fr = np.asarray(mm[f])
        x0, y0 = sx - W // 2, sy - H // 2
        for yy in range(H):
            for xx in range(W):
                a, b = y0 + yy, x0 + xx
                if 0 <= a < fr.shape[0] and 0 <= b < 256:
                    img[yy, xx] = fr[a, b]
        im = Image.fromarray(img).resize((W * 3, H * 3), Image.NEAREST)
        d = ImageDraw.Draw(im)
        d.text((2, 2), 'f%d cx%d' % (f, cx), fill=(255, 255, 0))
        d.line([(W * 3 // 2, 0), (W * 3 // 2, 6)], fill=(0, 255, 0))
        tiles.append(im)
    n = len(tiles)
    cols = min(8, n)
    rows = (n + cols - 1) // cols
    sheet = Image.new('RGB', (cols * W * 3 + (cols - 1) * 2, rows * H * 3 + (rows - 1) * 2), (40, 40, 40))
    for i, t in enumerate(tiles):
        sheet.paste(t, ((i % cols) * (W * 3 + 2), (i // cols) * (H * 3 + 2)))
    sheet.save(out)
    print('ghi', out, n, 'o')


if __name__ == '__main__':
    main()
