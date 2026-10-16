# enemy8_view.py (CVM2 08) - ghép nhiều frame lưới NES (bộ đệm map2_nes.npy của routine 02) thành 1 ảnh phóng to để XEM bằng mắt
# Chạy (từ gốc repo): python -P -E Tools/cvm2/enemy8_view.py <out.png> <f0:f1[:bước]> [x0 y0 x1 y1] [phóng=4] [cột=8] [video=map2]
#   vùng = px màn NES (y NES 8..231, mặc định cả khung); bỏ frame video lặp (map2_dup.npy); nhãn = frame + section + cx
import os, sys, csv
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
Y0 = 8


def main():
    out = sys.argv[1]
    rng = [int(v) for v in sys.argv[2].split(':')]
    f0, f1 = rng[0], rng[1]
    step = rng[2] if len(rng) > 2 else 1
    box = [int(v) for v in sys.argv[3:7]] if len(sys.argv) > 6 else [0, 8, 256, 232]
    zoom = int(sys.argv[7]) if len(sys.argv) > 7 else 4
    cols = int(sys.argv[8]) if len(sys.argv) > 8 else 8
    video = sys.argv[9] if len(sys.argv) > 9 else 'map2'
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(WORK, video + '_dup.npy'))
    cam = {}
    p = os.path.join(WORK, video + '_camera.csv')
    if os.path.exists(p):
        for r in csv.DictReader(open(p)):
            cam[int(r['frame'])] = (r['section'], r['cx'])
    x0, y0, x1, y1 = box
    fr = [f for f in range(f0, f1 + 1, step) if f < len(dup) and not dup[f]]
    w, h = (x1 - x0) * zoom, (y1 - y0) * zoom
    rows = (len(fr) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * (w + 4), rows * (h + 16)), (40, 40, 40))
    d = ImageDraw.Draw(sheet)
    for i, f in enumerate(fr):
        a = np.asarray(mm[f])[y0 - Y0:y1 - Y0, x0:x1]
        im = Image.fromarray(a.astype(np.uint8)).resize((w, h), Image.NEAREST)
        dr = ImageDraw.Draw(im)
        for gx in range(0, x1 - x0, 16):   # lưới 16 px NES (theo px màn)
            dr.line([((gx + (16 - x0 % 16) % 16) * zoom, 0), ((gx + (16 - x0 % 16) % 16) * zoom, h)], fill=(70, 70, 70))
        for gy in range(0, y1 - y0, 16):
            yy = (gy + (16 - y0 % 16) % 16) * zoom
            dr.line([(0, yy), (w, yy)], fill=(70, 70, 70))
        cx_, cy_ = (i % cols) * (w + 4), (i // cols) * (h + 16)
        sheet.paste(im, (cx_, cy_ + 14))
        s, c = cam.get(f, ('?', '?'))
        d.text((cx_ + 2, cy_ + 1), 'f%d %s cx%s' % (f, s[:10], c), fill=(255, 255, 0))
    sheet.save(out)
    print('ghi', out, len(fr), 'frame')


if __name__ == '__main__':
    main()
