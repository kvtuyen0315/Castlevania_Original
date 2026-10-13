"""CVGP 14 — dò Phantom Bat (boss Block 1) từng frame video bằng khớp mẫu sprite trên lưới NES (cùng thước đo enemy_fit 13).

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/boss_fit.py fit <map1|boss> f0 f1 <tag> [errmax=0.20] [newonly=1]
    → ClaudeNotes/cvgp/_cache/14/bat_<tag>.csv: mỗi frame video MỚI 1 dòng = cực tiểu toàn cục của 3 sprite
      phantombat_0 (ngủ 16×16) / _1 (dang cánh 48×32) / _2 (cánh nửa 32×32): tâm sprite (= pivot manifest) màn NES,
      world x (cx camera), sai khác, sai khác sprite thứ 2 (để biết khung cánh chắc không).
  python -P -E Tools/cvgp/boss_fit.py fly …  (như fit nhưng chỉ 2 khung bay _1/_2 — sau khi thức: khung ngủ nhiều đen dễ khớp nhầm cầu thang)
  python -P -E Tools/cvgp/boss_fit.py fx <map1|boss> f0 f1 <tag> [errmax=0.20]
    → dò chùm lửa chết (FX/bossdeath_{s,m,l}_{0..2}) ⇒ _cache/14/fx_<tag>.csv
  python -P -E Tools/cvgp/boss_fit.py orb <map1|boss> f0 f1 <tag>
    → dò quả cầu (Item/crystal_0/1) ⇒ _cache/14/orb_<tag>.csv (pivot đáy-giữa)
Vùng tìm = sân chơi NES y 48–223 (cả 256 cột). Frame lặp (work/<video>_dup.npy) bỏ khi newonly=1.
"""
import csv, os, sys
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import enemy_fit as ef  # noqa: E402

OUT = os.path.join(ef.ROOT, 'ClaudeNotes', 'cvgp', '_cache', '14')
BAT = ['Boss/PhantomBat/phantombat_0', 'Boss/PhantomBat/phantombat_1', 'Boss/PhantomBat/phantombat_2']
FX = ['FX/bossdeath_%s_%d' % (s, k) for s in 'sml' for k in range(3)]
ORB = ['Item/crystal_0', 'Item/crystal_1']
BOX = (0, 48, 256, 224)
# Bảng màu video ≠ sheet (đo routine 14: cụm pixel đỏ quanh dơi — boss f100, Map_1 f5990): đỏ (216,40,0), hồng (252,116,180)
PALMAP = {
    'boss': {(216, 40, 0): (174, 22, 22), (252, 116, 180): (228, 70, 196)},
    'map1': {(216, 40, 0): (202, 10, 0), (252, 116, 180): (240, 76, 144)},
}


def load(names, black):
    """Như enemy_fit.load nhưng (black=True) so CẢ pixel đen của sprite — dơi phần lớn đen (ngủ 16×16 chỉ có vài pixel đỏ),
    nền phòng boss xám ⇒ viền đen là đặc trưng chính."""
    if not black:
        return ef.load(names)
    from PIL import Image
    piv = ef.pivots()
    out, cols_all = [], []
    for n in names:
        im = np.asarray(Image.open(os.path.join(ef.ART, n + '.png')).convert('RGBA')).astype(int)
        a = im[..., 3] > 0
        ys, xs = np.nonzero(a)
        rgb = im[ys, xs, :3]
        for c in map(tuple, rgb):
            if c not in cols_all:
                cols_all.append(c)
        cls = np.array([cols_all.index(tuple(c)) for c in rgb])
        h, w = a.shape
        p = piv.get(n) or [w // 2, h]
        out.append(dict(name=n, flip=0, w=w, h=h, ys=ys, xs=xs, cls=cls, px=p[0], py=p[1]))
    return out, np.array(cols_all, float)


def best(maps, box):
    """Mỗi sprite (không phân biệt lật): cực tiểu toàn cục. Trả list (err, sprite, x, y) tăng dần."""
    by = {}
    for s, acc in maps:
        i = int(np.argmin(acc))
        yy, xx = divmod(i, acc.shape[1])
        e = float(acc[yy, xx])
        k = s['name']
        if k not in by or e < by[k][0]:
            by[k] = (e, s, box[0] + xx, box[1] + yy)
    return sorted(by.values(), key=lambda t: t[0])


def run(mode, video, f0, f1, tag, errmax, newonly):
    names = BAT if mode == 'fit' else BAT[1:] if mode == 'fly' else FX if mode == 'fx' else ORB
    sprites, pal = load(names, mode in ('fit', 'fly'))
    for i in range(len(pal)):
        pal[i] = PALMAP[video].get(tuple(int(v) for v in pal[i]), tuple(pal[i]))
    dup = np.load(os.path.join(ef.VID_DIR, 'work', video + '_dup.npy'))
    cam = ef.camera(video)
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, '%s_%s.csv' % ({'fit': 'bat', 'fly': 'fly', 'fx': 'fx', 'orb': 'orb'}[mode], tag))
    n = 0
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 't', 'dup', 'found', 'sprite', 'x', 'y', 'pivx', 'pivy', 'cx', 'worldx', 'err', 'sprite2', 'err2'])
        for f, nes in ef.frames(video, f0, f1):
            d = int(dup[f]) if f < len(dup) else 0
            if newonly and d:
                continue
            cx = cam.get(f, (0, ''))[0]
            if video == 'boss':
                cx = 512   # video boss: camera khoá PB cx 512 suốt (MAP1_TIMELINE §5.1)
            r = best(ef.err_maps(nes, sprites, pal, BOX), BOX)
            e, s, x, y = r[0]
            px = x + (s['w'] - s['px'] if s['flip'] else s['px'])
            py = y + s['py']
            e2 = r[1][0] if len(r) > 1 else 9
            n2 = r[1][1]['name'].split('/')[-1] if len(r) > 1 else ''
            ok = int(e <= errmax)
            w.writerow([f, '%.3f' % (f / 30.0), d, ok, s['name'].split('/')[-1], x, y, px, py, cx, cx + px, '%.4f' % e, n2, '%.4f' % e2])
            n += ok
    print('ghi', p, n, 'frame thay')


def main():
    mode, video = sys.argv[1], sys.argv[2]
    f0, f1 = int(sys.argv[3]), int(sys.argv[4])
    tag = sys.argv[5]
    errmax = float(sys.argv[6]) if len(sys.argv) > 6 else 0.20
    newonly = int(sys.argv[7]) if len(sys.argv) > 7 else 1
    run(mode, video, f0, f1, tag, errmax, newonly)


if __name__ == '__main__':
    main()
