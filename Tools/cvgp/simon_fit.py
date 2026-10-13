"""CVGP 08 — định vị Simon trên video bằng KHỚP MẪU sprite (thay cho bbox màu cam/kem của planner).
Lấy mẫu mỗi frame video về lưới NES (hiệu chuẩn MAP1_TIMELINE §1, dùng nes_common của planner), rồi với mỗi sprite
thân Simon (Assets/Data/Art/Character/Simon, quay trái + lật phải) quét vị trí quanh bbox planner và chọn
(sprite, hướng, x, y) có sai khác màu nhỏ nhất trên các pixel ĐỤC của sprite.
Kết quả theo quy ước Unity (CVGP 07 §4b): px = x pivot trên màn NES (tâm thân), Y = hàng MẶT SÀN dưới chân
(sprite vẽ pivot ở Y+1 ⇒ hàng sprite r nằm ở NES y = Y − 31 + r). Y section = Y − 32.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/simon_fit.py fit map1 <tên> f0 f1 [f0 f1 ...]   → ClaudeNotes/cvgp/_cache/08/fit_<tên>.csv
  python -P -E Tools/cvgp/simon_fit.py fit boss <tên> f0 f1
Cột CSV: frame,t,dup,sec,cx,sprite,flip,px,Y,err,n — dup = frame lặp (planner *_dup.npy), err = sai khác TB (0–1),
n = số pixel đục so. Bảng màu Simon trong video tự hiệu chuẩn (lượt 1 màu sheet → trung vị màu video → lượt 2).
"""
import csv, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
WORK = os.path.join(VID_DIR, 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '08')
SPR_DIR = os.path.join(ROOT, 'Assets', 'Data', 'Art', 'Character', 'Simon')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

# (tên, pivot x) — pivot y = 32 (đáy slot) cho mọi khung thân
SPRITES = [('idle_0', 8), ('walk_0', 8), ('walk_1', 8), ('walk_2', 8), ('jumpduck_0', 8),
           ('stairs_up_0', 8), ('stairs_up_1', 8), ('stairs_down_0', 8), ('stairs_down_1', 8),
           ('hurt_0', 8), ('death_0', 8),
           ('use_stand_0', 8), ('use_stand_1', 8), ('use_stand_2', 16),
           ('use_duck_0', 8), ('use_duck_1', 8), ('use_duck_2', 16),
           ('use_stairs_up_0', 8), ('use_stairs_up_1', 8), ('use_stairs_up_2', 16),
           ('use_stairs_down_0', 8), ('use_stairs_down_1', 8), ('use_stairs_down_2', 16)]
SHEET_COLORS = [(124, 8, 0), (252, 152, 56), (252, 216, 168)]
NES_Y0 = 8          # hàng NES đầu của ảnh lấy mẫu
CLIP = 0.35          # sai khác tối đa 1 pixel (chuẩn hoá 0–1) — chống nhiễu nền/che


def load_sprites():
    out = []
    for name, px in SPRITES:
        im = np.asarray(Image.open(os.path.join(SPR_DIR, name + '.png')).convert('RGBA')).astype(int)
        a = im[..., 3] > 0
        idx = np.full(a.shape, -1, int)
        for k, c in enumerate(SHEET_COLORS):
            idx[(im[..., 0] == c[0]) & (im[..., 1] == c[1]) & (im[..., 2] == c[2]) & a] = k
        if (idx[a] < 0).any():
            # khung roi cấp 3 có màu chuôi khác — không dùng ở đây
            raise SystemExit('sprite %s có màu ngoài bảng Simon' % name)
        h, w = a.shape
        for flip in (0, 1):
            ii = idx[:, ::-1] if flip else idx
            pxx = (w - px) if flip else px
            ys, xs = np.nonzero(ii >= 0)
            out.append(dict(name=name, flip=flip, w=w, h=h, pivx=pxx, ys=ys, xs=xs, cls=ii[ys, xs]))
    return out


def frame_iter(video, f0, f1):
    """(frame, ảnh NES float h×256×3) cho frame video f0..f1 (gồm 2 đầu)."""
    cal = nc.CALIB[video]
    ss = max(0.0, (f0 - 0.5) / 30.0)
    f = f0
    for img in nc.read_frames(video, ss=ss, t=(f1 - f0 + 1) / 30.0):
        if f > f1:
            break
        yield f, nc.nes_sample(img, cal, y0=NES_Y0, y1=240 - NES_Y0)
        f += 1


def fit_frame(nes, sprites, palette, guess, search=14):
    """guess = (x0, x1, y0, y1) bbox planner (màn NES) hoặc (px, Y) lần trước. Trả dict tốt nhất."""
    H, W, _ = nes.shape
    gx0, gx1, gy0, gy1 = guess
    best = None
    pal = np.asarray(palette, float)
    for s in sprites:
        cols = pal[s['cls']]
        # vị trí góc trên-trái (lx, ty) — quét quanh bbox
        for ty in range(gy0 - search, gy0 + 6):
            r = ty - NES_Y0 + s['ys']
            if r.min() < 0 or r.max() >= H:
                continue
            for lx in range(gx0 - search, gx0 + 8):
                c = lx + s['xs']
                if c.min() < 0 or c.max() >= W:
                    continue
                v = nes[r, c]
                d = np.sqrt(((v - cols) ** 2).sum(1)) / 441.7
                e = np.minimum(d, CLIP).mean()
                if best is None or e < best['err']:
                    best = dict(err=float(e), sprite=s['name'], flip=s['flip'], lx=lx, ty=ty, n=len(d),
                                px=lx + s['pivx'], Y=ty + 31, cls=s['cls'], v=v)
    return best


def planner_rows(video):
    p = os.path.join(WORK, ('map1' if video == 'map1' else 'boss') + '_simon.csv')
    if video == 'boss':
        p = os.path.join(WORK, 'boss_simon2.csv')
    rows = {}
    for r in csv.DictReader(open(p)):
        rows[int(r['frame'])] = r
    return rows


def guess_of(r, prev):
    if prev is not None:
        return (prev['lx'], prev['lx'] + 16, prev['ty'], prev['ty'] + 31)
    def gi(k):
        v = r.get(k, '')
        return int(float(v)) if v not in ('', None) else None
    x0, x1, y0, y1 = gi('x0'), gi('x1'), gi('y0'), gi('y1')
    if x0 is None:
        return None
    return (x0 - 2, x1, y0 - 2, y1)


def run_fit(video, name, ranges, palette=None):
    sprites = load_sprites()
    P = planner_rows(video)
    dup = np.load(os.path.join(WORK, ('map1' if video == 'map1' else 'boss') + '_dup.npy'))
    cams = {}
    if video == 'map1':
        for r in csv.DictReader(open(os.path.join(WORK, 'map1_camera.csv'))):
            cams[int(r['frame'])] = (r['section'], r['cx'])
    pal = palette or SHEET_COLORS
    res = []
    for f0, f1 in ranges:
        prev = None
        for f, nes in frame_iter(video, f0, f1):
            r = P.get(f, {})
            g = guess_of(r, prev)
            if g is None:
                res.append(dict(frame=f, err=None))
                continue
            b = fit_frame(nes, sprites, pal, g)
            # lần trước bám sai (err lớn) ⇒ thử lại theo bbox planner
            if b is not None and b['err'] > 0.12 and prev is not None:
                g2 = guess_of(r, None)
                if g2 is not None:
                    b2 = fit_frame(nes, sprites, pal, g2)
                    if b2 is not None and b2['err'] < b['err']:
                        b = b2
            b['frame'] = f
            sec, cx = cams.get(f, ('', ''))
            b['sec'], b['cx'] = sec, cx
            b['dup'] = int(dup[f]) if f < len(dup) else 0
            res.append(b)
            prev = b if b['err'] < 0.12 else None
    return res


def calibrate_palette(res):
    """Trung vị màu video của 3 lớp màu Simon trên các khớp tốt (err ≤ 0.10, hoặc 30 % khớp tốt nhất nếu
    bảng màu video khác hẳn sheet — vd video boss)."""
    acc = [[], [], []]
    errs = sorted(b['err'] for b in res if b.get('err') is not None)
    lim = max(0.10, errs[int(len(errs) * 0.3)]) if errs else 0.10
    for b in res:
        if b.get('err') is None or b['err'] > lim:
            continue
        for k in range(3):
            m = b['cls'] == k
            if m.any():
                acc[k].append(b['v'][m])
    pal = []
    for k in range(3):
        if acc[k]:
            pal.append(tuple(np.median(np.concatenate(acc[k]), axis=0)))
        else:
            pal.append(SHEET_COLORS[k])
    return pal


def write_csv(path, res):
    with open(path, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 't', 'dup', 'sec', 'cx', 'sprite', 'flip', 'px', 'Y', 'err', 'n'])
        for b in res:
            if b.get('err') is None:
                w.writerow([b['frame'], '%.3f' % (b['frame'] / 30.0), '', '', '', '', '', '', '', '', ''])
                continue
            w.writerow([b['frame'], '%.3f' % (b['frame'] / 30.0), b['dup'], b['sec'], b['cx'], b['sprite'], b['flip'],
                        b['px'], b['Y'], '%.4f' % b['err'], b['n']])


def main():
    if len(sys.argv) < 5 or sys.argv[1] != 'fit':
        print(__doc__)
        return
    video, name = sys.argv[2], sys.argv[3]
    a = list(map(int, sys.argv[4:]))
    ranges = list(zip(a[::2], a[1::2]))
    os.makedirs(OUT, exist_ok=True)
    res = run_fit(video, name, ranges)
    for it in range(2):
        pal = calibrate_palette(res)
        print('bang mau video:', [tuple(int(round(x)) for x in c) for c in pal])
        res = run_fit(video, name, ranges, pal)
    write_csv(os.path.join(OUT, 'fit_%s.csv' % name), res)
    errs = [b['err'] for b in res if b.get('err') is not None]
    print('%s: %d frame, err trung vi %.4f, max %.4f' % (name, len(res), float(np.median(errs)), float(np.max(errs))))


if __name__ == '__main__':
    main()
