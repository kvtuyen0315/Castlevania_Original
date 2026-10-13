"""CVGP 11 — theo vết item / nến / FX trên video bằng KHỚP MẪU sprite (Assets/Data/Art/<Nhóm>/<tên>.png), lưới NES.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/item_fit.py <map1|boss> <sprites> f0 f1 x0 y0 x1 y1 [errmax=0.16] [tag]
    sprites: danh sách cách ',' — đường dẫn tương đối Assets/Data/Art không đuôi (vd Item/heart_large,FX/hit_0)
    vùng tìm (x0,y0)-(x1,y1) = px màn NES (y NES, gồm HUD)
  → in từng frame: sprite khớp nhất, góc trên-trái (x,y) NES màn, đáy pivot (x+px, y+py) theo pivot manifest, cx camera,
    world x = cx + x; ghi ClaudeNotes/cvgp/_cache/11/fit_<tag>.csv
Giống proj_fit.py (CVGP 09): chỉ so pixel CÓ MÀU (bỏ viền đen), bảng màu = màu sheet.
"""
import csv, json, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '11')
ART = os.path.join(ROOT, 'Assets', 'Data', 'Art')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

Y0 = 8
CLIP = 0.35


def pivots():
    m = json.load(open(os.path.join(ROOT, 'Tools', 'cvgp', 'slices_gameplay.json'), encoding='utf-8'))
    out = {}
    for s in m['slices']:
        d = s.get('outDir', '').replace('\\', '/').rstrip('/')
        key = (d.split('Art/')[-1] + '/' + s['name']) if d else s['name']
        out[key] = s.get('pivot')
    return out


def load(names):
    piv = pivots()
    out, cols_all = [], []
    for n in names:
        im = np.asarray(Image.open(os.path.join(ART, n + '.png')).convert('RGBA')).astype(int)
        a = (im[..., 3] > 0) & (im[..., :3].sum(2) > 0)
        ys, xs = np.nonzero(a)
        rgb = im[ys, xs, :3]
        for c in map(tuple, rgb):
            if c not in cols_all:
                cols_all.append(c)
        cls = np.array([cols_all.index(tuple(c)) for c in rgb])
        h, w = a.shape
        p = piv.get(n) or [w // 2, h]
        for flip in (0, 1):
            xx = (w - 1 - xs) if flip else xs
            out.append(dict(name=n, flip=flip, w=w, h=h, ys=ys, xs=xx, cls=cls, px=p[0], py=p[1]))
    return out, np.array(cols_all, float)


def frames(video, f0, f1):
    ss = max(0.0, (f0 - 0.5) / 30.0)
    f = f0
    for img in nc.read_frames(video, ss=ss, t=(f1 - f0 + 1) / 30.0):
        if f > f1:
            break
        yield f, nc.nes_sample(img, nc.CALIB[video], y0=Y0, y1=240 - Y0)
        f += 1


def match(nes, sprites, pal, box):
    x0, y0, x1, y1 = box
    best = None
    for s in sprites:
        H, W = y1 - y0 - s['h'] + 1, x1 - x0 - s['w'] + 1
        if H <= 0 or W <= 0:
            continue
        acc = np.zeros((H, W))
        for k in range(len(s['ys'])):
            sl = nes[y0 - Y0 + s['ys'][k]:y0 - Y0 + s['ys'][k] + H, x0 + s['xs'][k]:x0 + s['xs'][k] + W]
            acc += np.minimum(np.sqrt(((sl - pal[s['cls'][k]]) ** 2).sum(2)) / 441.7, CLIP)
        acc /= len(s['ys'])
        i = int(np.argmin(acc))
        yy, xx = divmod(i, W)
        e = float(acc[yy, xx])
        if best is None or e < best['err']:
            best = dict(err=e, s=s, x=x0 + xx, y=y0 + yy)
    return best


def camera(video):
    p = os.path.join(VID_DIR, 'work', video + '_camera.csv')
    cx = {}
    if os.path.exists(p):
        for r in csv.DictReader(open(p)):
            try:
                cx[int(r['frame'])] = int(float(r['cx']))
            except ValueError:
                pass
    return cx


def main():
    video, names = sys.argv[1], sys.argv[2].split(',')
    f0, f1, x0, y0, x1, y1 = map(int, sys.argv[3:9])
    errmax = float(sys.argv[9]) if len(sys.argv) > 9 else 0.16
    tag = sys.argv[10] if len(sys.argv) > 10 else '%s_%d' % (video, f0)
    sprites, pal = load(names)
    dup = np.load(os.path.join(VID_DIR, 'work', video + '_dup.npy'))
    cam = camera(video)
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, 'fit_%s.csv' % tag)
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'worldx', 'err', 'ok'])
        for f, nes in frames(video, f0, f1):
            b = match(nes, sprites, pal, (x0, y0, x1, y1))
            s = b['s']
            pivx = b['x'] + (s['w'] - s['px'] if s['flip'] else s['px'])
            pivy = b['y'] + s['py']
            cx = cam.get(f, 0)
            d = int(dup[f]) if f < len(dup) else 0
            ok = b['err'] <= errmax
            w.writerow([f, d, s['name'], s['flip'], b['x'], b['y'], pivx, pivy, cx, cx + pivx, '%.4f' % b['err'], int(ok)])
            print('f%d %s %s%s xy(%d,%d) piv(%d,%d) cx%d wx%d err %.3f%s' % (
                f, 'D' if d else ' ', s['name'], 'F' if s['flip'] else '', b['x'], b['y'], pivx, pivy, cx, cx + pivx,
                b['err'], '' if ok else '  -'))
    print('ghi', p)


if __name__ == '__main__':
    main()
