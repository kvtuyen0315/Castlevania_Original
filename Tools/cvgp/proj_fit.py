"""CVGP 09 — theo vết vũ khí phụ trên video bằng KHỚP MẪU sprite (Assets/Data/Art/SubWeapon), lấy mẫu lưới NES.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/proj_fit.py <map1|boss> <kind> f0 f1 x0 y0 x1 y1 [errmax=0.16]
    kind: dagger | axe | holy | fire        vùng tìm (x0,y0)-(x1,y1) = px màn NES (y NES, gồm HUD)
  → ClaudeNotes/cvgp/_cache/09/proj_<video>_<kind>_<f0>.csv: frame,dup,sprite,flip,x,y (góc trên-trái sprite, NES màn),
    cx,cy (tâm bbox pixel đục),err
Bảng màu = màu sheet (không hiệu chuẩn — xem main()).
"""
import csv, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '09')
SPR = os.path.join(ROOT, 'Assets', 'Data', 'Art', 'SubWeapon')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

KINDS = {
    'dagger': ['dagger_0'],
    'axe': ['axe_0', 'axe_1', 'axe_2', 'axe_3'],
    'holy': ['holywater_bottle'],
    'fire': ['holywater_fire_0', 'holywater_fire_1', 'holywater_fire_2', 'holywater_fire_3', 'holywater_fire_4'],
}
Y0 = 8
CLIP = 0.35


def load(kind):
    out = []
    cols_all = []
    for n in KINDS[kind]:
        im = np.asarray(Image.open(os.path.join(SPR, n + '.png')).convert('RGBA')).astype(int)
        a = im[..., 3] > 0
        # chỉ so pixel CÓ MÀU (bỏ viền đen): viền đen khớp mọi nền tối ⇒ kéo vết về chỗ tối
        a &= im[..., :3].sum(2) > 0
        ys, xs = np.nonzero(a)
        rgb = im[ys, xs, :3]
        for c in map(tuple, rgb):
            if c not in cols_all:
                cols_all.append(c)
        cls = np.array([cols_all.index(tuple(c)) for c in rgb])
        h, w = a.shape
        for flip in (0, 1):
            xx = (w - 1 - xs) if flip else xs
            out.append(dict(name=n, flip=flip, w=w, h=h, ys=ys, xs=xx, cls=cls,
                            bx=(xx.min() + xx.max()) / 2.0, by=(ys.min() + ys.max()) / 2.0))
    return out, cols_all


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
        H = y1 - y0 - s['h'] + 1
        W = x1 - x0 - s['w'] + 1
        if H <= 0 or W <= 0:
            continue
        acc = np.zeros((H, W))
        for k in range(len(s['ys'])):
            ry = y0 - Y0 + s['ys'][k]
            rx = x0 + s['xs'][k]
            sl = nes[ry:ry + H, rx:rx + W]
            d = np.sqrt(((sl - pal[s['cls'][k]]) ** 2).sum(2)) / 441.7
            acc += np.minimum(d, CLIP)
        acc /= len(s['ys'])
        i = int(np.argmin(acc))
        yy, xx = divmod(i, W)
        e = float(acc[yy, xx])
        if best is None or e < best['err']:
            best = dict(err=e, sprite=s['name'], flip=s['flip'], x=x0 + xx, y=y0 + yy, s=s)
    return best


def run(video, kind, f0, f1, box, pal, keep=None):
    sprites, _ = load(kind)
    dup = np.load(os.path.join(VID_DIR, 'work', video + '_dup.npy'))
    res = []
    for f, nes in frames(video, f0, f1):
        b = match(nes, sprites, pal, box)
        b['frame'] = f
        b['dup'] = int(dup[f]) if f < len(dup) else 0
        s = b['s']
        r = (b['y'] - Y0 + s['ys']).clip(0, nes.shape[0] - 1)
        b['v'] = nes[r, b['x'] + s['xs']]
        res.append(b)
    return res


def calib(res, ncol, pal, errmax):
    acc = [[] for _ in range(ncol)]
    for b in res:
        if b['err'] > errmax:
            continue
        for k in range(ncol):
            m = b['s']['cls'] == k
            if m.any():
                acc[k].append(b['v'][m])
    # màu đen (viền) giữ nguyên — hiệu chuẩn đen theo nền tối sẽ làm mọi chỗ tối "khớp"
    return np.array([pal[k] if (not acc[k] or not pal[k].any()) else np.median(np.concatenate(acc[k]), axis=0)
                     for k in range(ncol)])


def main():
    video, kind = sys.argv[1], sys.argv[2]
    f0, f1, x0, y0, x1, y1 = map(int, sys.argv[3:9])
    errmax = float(sys.argv[9]) if len(sys.argv) > 9 else 0.16
    _, cols = load(kind)
    pal = np.array(cols, float)
    # bảng màu = màu sheet (KHÔNG tự hiệu chuẩn: vật nhỏ trên nền tối ⇒ trung vị kéo màu về nền và khớp sai mọi chỗ)
    res = run(video, kind, f0, f1, (x0, y0, x1, y1), pal)
    print('bang mau video:', [tuple(int(v) for v in c) for c in pal])
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, 'proj_%s_%s_%d.csv' % (video, kind, f0))
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'cx', 'cy', 'err'])
        for b in res:
            s = b['s']
            w.writerow([b['frame'], b['dup'], b['sprite'], b['flip'], b['x'], b['y'],
                        '%.1f' % (b['x'] + s['bx']), '%.1f' % (b['y'] + s['by']), '%.4f' % b['err']])
            print('f%d dup%d %s%s x%d y%d c(%.1f,%.1f) err %.3f%s' % (
                b['frame'], b['dup'], s['name'], 'F' if s['flip'] else '', b['x'], b['y'], b['x'] + s['bx'],
                b['y'] + s['by'], b['err'], '' if b['err'] <= errmax else '  (khong tin)'))
    print('ghi', p)


if __name__ == '__main__':
    main()
