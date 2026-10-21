# -*- coding: utf-8 -*-
"""CVM3 03 - luoi anh cac frame video quanh moi lan dung dong ho (doc pose Simon + vat bi dong bang).

Chay tu goc repo:  python -P -E Tools/cvm3/stopwatch3_sheet.py <video> <out.png> <f0>-<f1>[,<f0>-<f1>...] [x0 y0 x1 y1 crop video px] [scale]
vd: python -P -E Tools/cvm3/stopwatch3_sheet.py "C:/.../Map_2.mp4" out.png 4826-4838 0 0 1488 760 0.5
Moi frame ghi so frame + tick (frame*2.0033) o goc.
"""
import sys, subprocess, numpy as np
from PIL import Image, ImageDraw

FF = r'C:\ffmpeg\bin\ffmpeg.exe'


def grab(video, f, W, H):
    t = f / 30.0
    cmd = [FF, '-v', 'error', '-ss', '%.4f' % max(0, t - 1.0), '-i', video, '-ss', '1.0' if t >= 1.0 else '%.4f' % t,
           '-frames:v', '1', '-f', 'rawvideo', '-pix_fmt', 'rgb24', '-']
    raw = subprocess.run(cmd, capture_output=True).stdout
    return np.frombuffer(raw[:W * H * 3], np.uint8).reshape(H, W, 3)


def read_range(video, f0, f1, W, H):
    t = f0 / 30.0
    n = f1 - f0 + 1
    cmd = [FF, '-v', 'error', '-ss', '%.4f' % max(0, t - 2.0), '-i', video, '-ss', '%.4f' % (min(t, 2.0)),
           '-frames:v', str(n), '-f', 'rawvideo', '-pix_fmt', 'rgb24', '-']
    raw = subprocess.run(cmd, capture_output=True).stdout
    k = len(raw) // (W * H * 3)
    return np.frombuffer(raw[:k * W * H * 3], np.uint8).reshape(k, H, W, 3)


def main():
    video, out, ranges = sys.argv[1], sys.argv[2], sys.argv[3]
    W, H = 1488, 760
    info = subprocess.run([FF, '-i', video], capture_output=True, text=True).stderr
    import re
    m = re.search(r', (\d{3,4})x(\d{3,4})', info)
    if m:
        W, H = int(m.group(1)), int(m.group(2))
    crop = [int(v) for v in sys.argv[4:8]] if len(sys.argv) >= 8 else [0, 0, W, H]
    scale = float(sys.argv[8]) if len(sys.argv) >= 9 else 0.5
    tiles = []
    for rg in ranges.split(','):
        a, b = [int(v) for v in rg.split('-')]
        fr = read_range(video, a, b, W, H)
        for i in range(len(fr)):
            im = Image.fromarray(fr[i][crop[1]:crop[3], crop[0]:crop[2]])
            im = im.resize((int(im.width * scale), int(im.height * scale)), Image.NEAREST)
            d = ImageDraw.Draw(im)
            d.rectangle([0, 0, 120, 14], fill=(0, 0, 0))
            d.text((2, 2), 'f%d t%.1f' % (a + i, (a + i) * 60.0988 / 30.0), fill=(255, 255, 0))
            tiles.append(im)
    cols = 6
    tw, th = tiles[0].width, tiles[0].height
    rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * tw, rows * th), (40, 40, 40))
    for i, t in enumerate(tiles):
        sheet.paste(t, ((i % cols) * tw, (i // cols) * th))
    sheet.save(out)
    print('saved', out, len(tiles), 'tiles', sheet.size)


if __name__ == '__main__':
    main()
