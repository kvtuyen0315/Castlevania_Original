"""CVM2 08 — vẽ phát hiện (det_<tag>.csv) theo thời gian để XEM vết bằng mắt: 2 khung (trên: x màn theo frame; dưới: y NES
theo frame). Điểm phát hiện màu theo err (xanh tốt → đỏ kém), Simon (ref/data/map2_simon.csv) màu trắng, nhãn frame mỗi 50.
Chạy: python -P -E Tools/cvm2/enemy8_plot.py <tag> <out.png> [errmax=0.10] [px/frame=4] [key=pivx|worldx]
"""
import csv, os, sys
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')


def main():
    tag, out = sys.argv[1], sys.argv[2]
    errmax = float(sys.argv[3]) if len(sys.argv) > 3 else 0.10
    k = int(sys.argv[4]) if len(sys.argv) > 4 else 4
    key = sys.argv[5] if len(sys.argv) > 5 else 'pivx'
    det = [r for r in csv.DictReader(open(os.path.join(D, 'det_%s.csv' % tag))) if float(r['err']) <= errmax]
    fr = [int(r['frame']) for r in det]
    f0, f1 = min(fr) - 5, max(fr) + 5
    xs = [int(r[key]) for r in det]
    xmax = max(256, max(xs) + 8) if key == 'worldx' else 256
    xmin = min(0, min(xs) - 8) if key == 'worldx' else 0
    W = (f1 - f0) * k + 60
    H1, H2 = (xmax - xmin) * 2, 2 * 200
    im = Image.new('RGB', (W, H1 + H2 + 30), (20, 20, 20))
    d = ImageDraw.Draw(im)
    for f in range(f0 - f0 % 50 + 50, f1, 50):
        X = 50 + (f - f0) * k
        d.line([(X, 0), (X, H1 + H2 + 30)], fill=(60, 60, 60))
        d.text((X + 2, H1 + 2), str(f), fill=(200, 200, 0))
    for v in range(xmin - xmin % 32, xmax, 32):
        d.line([(50, (v - xmin) * 2), (W, (v - xmin) * 2)], fill=(45, 45, 45))
        d.text((2, (v - xmin) * 2), str(v), fill=(150, 150, 150))
    for v in range(32, 232, 32):
        Y = H1 + 30 + (v - 32) * 2
        d.line([(50, Y), (W, Y)], fill=(45, 45, 45))
        d.text((2, Y), str(v), fill=(150, 150, 150))
    for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_simon.csv'))):
        f = int(r['frame'])
        if r['found'] != '1' or f < f0 or f > f1:
            continue
        X = 50 + (f - f0) * k
        sx = int(r['world_x']) if key == 'worldx' else int(r['px'])
        d.rectangle([X, (sx - xmin) * 2, X + 1, (sx - xmin) * 2 + 1], fill=(255, 255, 255))
        Y = H1 + 30 + (int(r['Y']) - 32) * 2
        d.rectangle([X, Y, X + 1, Y + 1], fill=(255, 255, 255))
    for r in det:
        f = int(r['frame'])
        e = float(r['err']) / errmax
        col = (int(255 * e), int(255 * (1 - e)), 80)
        X = 50 + (f - f0) * k
        x = (int(r[key]) - xmin) * 2
        d.rectangle([X - 1, x - 1, X + 1, x + 1], fill=col)
        Y = H1 + 30 + (int(r['pivy']) - 32) * 2
        d.rectangle([X - 1, Y - 1, X + 1, Y + 1], fill=col)
    im.save(out)
    print('ghi', out, len(det), 'diem')


if __name__ == '__main__':
    main()
