# Trich frame luoi NES lien tiep quanh moi lan nem nuoc thanh -> anh ghep (cvm2-04). Chay tu goc repo: python -P -E Tools/cvm2/holy4_sheets.py <boss1|boss2|map2> <f0> <n> <x0> <y0> <x1> <y1> <scale> <cols> <out.png>
import sys, os
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, 'Tools/cvm2')
import nes2
FF = r'C:\ffmpeg\bin\ffmpeg.exe'
VIDS = {
 'boss1': (r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Boss_Map_1_Behaviour.mp4', 1264, 834, dict(sx=3.6295, sy=3.1765, ox=169.68, oy=40.13), 0.0),
 'boss2': (nes2.VID['boss2'][0], 1490, 718, nes2.CALIB['boss2'], 0.5),
 'map2': (nes2.VID['map2'][0], 1486, 712, nes2.CALIB['map2'], 0.5),
}
def frames(v, f0, n):
    import subprocess
    path, W, H, cal, half = VIDS[v]
    cmd = [FF, '-hide_banner', '-loglevel', 'error', '-i', path, '-vf', r'select=between(n\,%d\,%d)' % (f0, f0 + n - 1), '-vsync', '0', '-f', 'rawvideo', '-pix_fmt', 'rgb24', '-']
    p = subprocess.run(cmd, capture_output=True)
    buf = p.stdout; sz = W * H * 3
    out = []
    for i in range(len(buf) // sz):
        out.append(np.frombuffer(buf[i * sz:(i + 1) * sz], np.uint8).reshape(H, W, 3))
    return out
def nes(img, v):
    path, W, H, cal, half = VIDS[v]
    xs = np.round(cal['ox'] + cal['sx'] * (np.arange(256) + half)).astype(int)
    ys = np.round(cal['oy'] + cal['sy'] * (np.arange(240) + half)).astype(int)
    xs = np.clip(xs, 0, W - 1); ys = np.clip(ys, 0, H - 1)
    return img[ys][:, xs]
def sheet(v, f0, n, x0, y0, x1, y1, scale, cols, out):
    fr = frames(v, f0, n)
    tiles = []
    for i, im in enumerate(fr):
        a = nes(im, v)[y0:y1, x0:x1]
        t = Image.fromarray(a).resize(((x1 - x0) * scale, (y1 - y0) * scale), Image.NEAREST)
        d = ImageDraw.Draw(t); d.rectangle([0, 0, 44, 11], fill=(0, 0, 0)); d.text((2, 0), 'f%d' % (f0 + i), fill=(255, 255, 0))
        tiles.append(t)
    w, h = tiles[0].size; rows = (len(tiles) + cols - 1) // cols
    S = Image.new('RGB', (cols * (w + 2), rows * (h + 2)), (60, 0, 60))
    for i, t in enumerate(tiles):
        S.paste(t, ((i % cols) * (w + 2), (i // cols) * (h + 2)))
    S.save(out); print(out, len(tiles))
if __name__ == '__main__':
    v, f0, n, x0, y0, x1, y1, sc, cols, out = sys.argv[1:]
    sheet(v, int(f0), int(n), int(x0), int(y0), int(x1), int(y1), int(sc), int(cols), out)
