"""CVGP 13 — dò NHIỀU enemy mỗi frame video bằng khớp mẫu sprite (Assets/Data/Art/<Nhóm>/<tên>.png) trên lưới NES.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/enemy_fit.py <map1|boss> <sprites> f0 f1 x0 y0 x1 y1 [errmax=0.14] [tag] [newonly=1]
    sprites: danh sách cách ',' — đường dẫn tương đối Assets/Data/Art không đuôi (vd Enemy/Zombie/zombie_0)
    vùng tìm (x0,y0)-(x1,y1) = px màn NES (y NES, gồm HUD); newonly=1 bỏ frame video lặp (work/<video>_dup.npy)
  → ghi ClaudeNotes/cvgp/_cache/13/det_<tag>.csv: mỗi dòng 1 phát hiện (frame, sprite, flip, góc trên-trái, pivot màn,
    cx camera, world x pivot, err). Nhiều phát hiện/frame: cực tiểu cục bộ của bản đồ sai khác, loại chồng (NMS theo pivot).
Cùng thước đo với item_fit.py (CVGP 11): chỉ so pixel CÓ MÀU của sprite (bỏ viền đen), bảng màu = màu sheet, sai khác
màu cắt ở 0.35 rồi lấy trung bình. Bản đồ khoảng cách màu tính 1 lần/frame cho từng màu ⇒ nhanh hơn item_fit.
"""
import csv, json, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '13')
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


def err_maps(nes, sprites, pal, box):
    """Trả list (sprite, err map H×W, gốc x0, y0) — err[y, x] = sai khác khi góc trên-trái sprite ở (x0 + x, y0 + y)."""
    x0, y0, x1, y1 = box
    sub = nes[y0 - Y0:y1 - Y0, x0:x1].astype(float)
    dist = [np.minimum(np.sqrt(((sub - pal[c]) ** 2).sum(2)) / 441.7, CLIP) for c in range(len(pal))]
    out = []
    for s in sprites:
        H, W = (y1 - y0) - s['h'] + 1, (x1 - x0) - s['w'] + 1
        if H <= 0 or W <= 0:
            continue
        acc = np.zeros((H, W))
        for k in range(len(s['ys'])):
            acc += dist[s['cls'][k]][s['ys'][k]:s['ys'][k] + H, s['xs'][k]:s['xs'][k] + W]
        acc /= len(s['ys'])
        out.append((s, acc))
    return out


def detect(maps, box, errmax, sep):
    """Cực tiểu cục bộ < errmax của mọi bản đồ, loại chồng theo pivot (cách < sep px ⇒ giữ cái sai khác nhỏ hơn)."""
    x0, y0 = box[0], box[1]
    cand = []
    for s, acc in maps:
        H, W = acc.shape
        ys, xs = np.nonzero(acc <= errmax)
        for yy, xx in zip(ys, xs):
            e = acc[yy, xx]
            ya, yb, xa, xb = max(0, yy - 2), min(H, yy + 3), max(0, xx - 2), min(W, xx + 3)
            if e > acc[ya:yb, xa:xb].min():
                continue
            x, y = x0 + xx, y0 + yy
            pivx = x + (s['w'] - s['px'] if s['flip'] else s['px'])
            cand.append((float(e), s, x, y, pivx, y + s['py']))
    cand.sort(key=lambda c: c[0])
    keep = []
    for c in cand:
        if all(abs(c[4] - k[4]) >= sep[0] or abs(c[5] - k[5]) >= sep[1] for k in keep):
            keep.append(c)
    return keep


def camera(video):
    p = os.path.join(VID_DIR, 'work', video + '_camera.csv')
    cx = {}
    if os.path.exists(p):
        for r in csv.DictReader(open(p)):
            try:
                cx[int(r['frame'])] = (int(float(r['cx'])), r['section'])
            except ValueError:
                pass
    return cx


def main():
    video, names = sys.argv[1], sys.argv[2].split(',')
    f0, f1, x0, y0, x1, y1 = map(int, sys.argv[3:9])
    errmax = float(sys.argv[9]) if len(sys.argv) > 9 else 0.14
    tag = sys.argv[10] if len(sys.argv) > 10 else '%s_%d' % (video, f0)
    newonly = int(sys.argv[11]) if len(sys.argv) > 11 else 1
    sprites, pal = load(names)
    sep = (max(s['w'] for s in sprites) * 3 // 4, max(s['h'] for s in sprites) * 3 // 4)
    dup = np.load(os.path.join(VID_DIR, 'work', video + '_dup.npy'))
    cam = camera(video)
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, 'det_%s.csv' % tag)
    n = 0
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'section', 'worldx', 'err'])
        for f, nes in frames(video, f0, f1):
            d = int(dup[f]) if f < len(dup) else 0
            if newonly and d:
                continue
            cx, sec = cam.get(f, (0, ''))
            for e, s, x, y, px, py in detect(err_maps(nes, sprites, pal, (x0, y0, x1, y1)), (x0, y0), errmax, sep):
                w.writerow([f, d, s['name'].split('/')[-1], s['flip'], x, y, px, py, cx, sec, cx + px, '%.4f' % e])
                n += 1
    print('ghi', p, n, 'phat hien')


if __name__ == '__main__':
    main()
