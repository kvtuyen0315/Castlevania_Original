"""CVM2 08 — dò NHIỀU enemy mỗi frame video Map_2 bằng khớp mẫu sprite trên bộ đệm lưới NES (routine 02: map2_nes.npy).
Bản Map_2 của Tools/cvgp/enemy_fit.py (CVGP 13): cùng thước đo (sai khác màu cắt CLIP rồi lấy trung bình trên pixel đục của sprite)
nhưng (a) đọc bộ đệm NES đã lấy mẫu (không giải mã lại video), (b) bảng màu VIDEO ước lượng tự động (video Map_2 tối/khác sheet —
H1: chỉ dùng để dò vị trí, không lấy màu làm sprite), (c) gồm cả pixel ĐEN đục của sprite (viền/thân đen NES vẽ đè nền đỏ).

Chạy (từ gốc repo):
  python -P -E Tools/cvm2/enemy8_fit.py <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax=0.16] [pal=auto|file.json]
    sprites: đường dẫn tương đối Assets/Data/Art không đuôi, cách ',' (vd Enemy/BlackKnight/blackknight_0,...)
    vùng tìm = px màn NES (y NES 8..231; mặc định sân chơi 0 48 256 224)
  → ClaudeNotes/cvm2/_cache/08/det_<tag>.csv (frame, dup, sprite, flip, góc trên-trái x y, pivot màn pivx pivy, cx, section, worldx,
    ysec = pivy − 32, err) + pal_<tag>.json (bảng màu video đã ước lượng).
Bảng màu auto: lượt 1 dùng màu sheet ×0.85 (đen giữ đen) trên ≤ 60 frame rải đều, lấy 40 phát hiện tốt nhất ⇒ trung vị màu video
từng lớp màu ⇒ lượt 2 (chính) dùng bảng đó.
"""
import csv, json, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')
ART = os.path.join(ROOT, 'Assets', 'Data', 'Art')
Y0 = 8          # hàng NES đầu của bộ đệm
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
        a = im[..., 3] > 0
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
    return out, cols_all


def err_maps(nes, sprites, pal, box):
    x0, y0, x1, y1 = box
    sub = nes[y0 - Y0:y1 - Y0, x0:x1].astype(float)
    dist = [np.minimum(np.sqrt(((sub - np.array(pal[c], float)) ** 2).sum(2)) / 441.7, CLIP) for c in range(len(pal))]
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
            pivx = x + ((s['w'] - s['px']) if s['flip'] else s['px'])
            cand.append((float(e), s, x, y, pivx, y + s['py']))
    cand.sort(key=lambda c: c[0])
    keep = []
    for c in cand:
        if all(abs(c[4] - k[4]) >= sep[0] or abs(c[5] - k[5]) >= sep[1] for k in keep):
            keep.append(c)
    return keep


def camera():
    cam = {}
    for r in csv.DictReader(open(os.path.join(WORK, 'map2_camera.csv'))):
        cam[int(r['frame'])] = (r['kind'], r['section'], int(float(r['cx'])), int(float(r['cxw'])))
    return cam


def estimate_pal(mm, dup, frames, sprites, cols, box, errmax, sep):
    pal0 = [tuple(c) if sum(c) == 0 else tuple(int(v * 0.85) for v in c) for c in cols]
    fr = [f for f in frames if not dup[f]]
    if len(fr) > 60:
        fr = [fr[int(i * (len(fr) - 1) / 59)] for i in range(60)]
    hits = []
    for f in fr:
        nes = np.asarray(mm[f])
        for e, s, x, y, px, py in detect(err_maps(nes, sprites, pal0, box), box, errmax * 1.5, sep)[:3]:
            hits.append((e, f, s, x, y))
    hits.sort(key=lambda h: h[0])
    acc = [[] for _ in cols]
    for e, f, s, x, y in hits[:40]:
        nes = np.asarray(mm[f]).astype(int)
        for k in range(len(s['ys'])):
            acc[s['cls'][k]].append(nes[y + s['ys'][k] - Y0, x + s['xs'][k]])
    pal = []
    for c, a in zip(cols, acc):
        pal.append(tuple(int(v) for v in np.median(np.array(a), axis=0)) if len(a) >= 10 else pal0[len(pal)])
    return pal, len(hits[:40]), (hits[0][0] if hits else -1)


def main():
    tag, names = sys.argv[1], sys.argv[2].split(',')
    f0, f1 = int(sys.argv[3]), int(sys.argv[4])
    box = [int(v) for v in sys.argv[5:9]] if len(sys.argv) > 8 else [0, 48, 256, 224]
    errmax = float(sys.argv[9]) if len(sys.argv) > 9 else 0.16
    palarg = sys.argv[10] if len(sys.argv) > 10 else 'auto'
    sprites, cols = load(names)
    sep = (max(s['w'] for s in sprites) * 3 // 4, max(s['h'] for s in sprites) * 3 // 4)
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(WORK, 'map2_dup.npy'))
    cam = camera()
    os.makedirs(OUT, exist_ok=True)
    frames = [f for f in range(f0, f1 + 1) if cam.get(f, ('',))[0] == 'game']
    if palarg == 'auto':
        pal, nh, best = estimate_pal(mm, dup, frames, sprites, cols, box, errmax, sep)
        print('bang mau video (sheet -> video):', [(tuple(c), p) for c, p in zip(cols, pal)], 'mau', nh, 'tot nhat', round(best, 4))
    else:
        d = json.load(open(palarg))
        pal = [tuple(d[','.join(map(str, c))]) for c in cols]
    json.dump({','.join(map(str, c)): list(p) for c, p in zip(cols, pal)}, open(os.path.join(OUT, 'pal_%s.json' % tag), 'w'))
    p = os.path.join(OUT, 'det_%s.csv' % tag)
    n = 0
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'section', 'worldx', 'ysec', 'err'])
        for f in frames:
            d = int(dup[f])
            if d:
                continue
            kind, sec, cx, cxw = cam[f]
            nes = np.asarray(mm[f])
            for e, s, x, y, px, py in detect(err_maps(nes, sprites, pal, box), box, errmax, sep):
                w.writerow([f, d, s['name'].split('/')[-1], s['flip'], x, y, px, py, cx, sec, cx + px, py - 32, '%.4f' % e])
                n += 1
    print('ghi', p, n, 'phat hien /', len(frames), 'frame')


if __name__ == '__main__':
    main()
