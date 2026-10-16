# terrain5.py (CVM2 05) - doi chieu dia hinh Level 2 voi video Map_2 tren luoi NES (bo dem extract_nes2 cua routine 02)
# Chay tu goc repo: python -P -E Tools/cvm2/terrain5.py <lenh> ...
#   broken                      : quet moi o 16x16 cua moi section qua moi frame game: o "thanh den" (khoi bi pha) va giu den toi het doan
#   cell <Section> c r [f0 f1]  : in theo frame do lech + ti le den cua o (c, r)
#   shot <frame> <out.png> [Z]  : anh frame luoi NES phong to + luoi 16 px theo cx (de xem)
# Can: ClaudeNotes/cvm2/_cache/02/work/map2_nes.npy (Tools/cvm2/extract_nes2.py map2) + ref/data/map2_camera.csv
import os, sys, csv
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fg2
from nes2 import SECTIONS, load_sheet, section
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
CAM = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')
Y8 = 8   # hang 0 cua bo dem = NES y 8; hang section r <-> NES y 32 + r <-> bo dem r + 24


def cam_rows():
    return list(csv.DictReader(open(CAM)))


def frames_of(section_name):
    out = []
    for r in cam_rows():
        if r['kind'] == 'game' and r['section'] == section_name:
            out.append((int(r['frame']), int(r['cx'])))
    return out


def cell_stats(fr, sec_lut, sec_raw, cx, c, r):
    """(lech TB, ti le pixel den o cho sheet khong den) cho o (c, r) neu nam tron trong man; None neu ngoai/che HUD"""
    x0 = c * 16 - cx
    if x0 < 0 or x0 + 16 > 256 or r * 16 < 16:
        return None
    v = fr[r * 16 + 24:r * 16 + 40, x0:x0 + 16].astype(float)
    s = sec_lut[r * 16:r * 16 + 16, c * 16:c * 16 + 16]
    raw = sec_raw[r * 16:r * 16 + 16, c * 16:c * 16 + 16]
    nb = raw.sum(axis=2) > 0
    if nb.sum() < 32:
        return None
    d = float(np.abs(v - s).mean())
    blk = float((v.max(axis=2) < 40)[nb].mean())
    return d, blk


def cmd_broken():
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    sh = load_sheet()
    for name in SECTIONS:
        raw = section(sh, name)
        lut = fg2.apply_lut(raw)
        fl = frames_of(name)
        if not fl:
            continue
        W = raw.shape[1]
        hist = {}
        for f, cx in fl[::2]:
            fr = np.asarray(mm[f])
            for r in range(1, 12):
                for c in range(cx // 16, (cx + 256) // 16 + 1):
                    st = cell_stats(fr, lut, raw, cx, c, r)
                    if st is None:
                        continue
                    hist.setdefault((c, r), []).append((f, st[1]))
        for (c, r), seq in sorted(hist.items()):
            bl = [b > 0.85 for _f, b in seq]
            # tu frame dau tien den, >= 90 % cac frame sau con den va >= 8 mau
            for i, b in enumerate(bl):
                if b:
                    rest = bl[i:]
                    if len(rest) >= 8 and sum(rest) >= 0.9 * len(rest):
                        print('%-15s o (%2d,%2d) den tu f%d (%.2f s) - %d/%d mau sau do den; mau truoc %d' % (
                            name, c, r, seq[i][0], seq[i][0] / 30, sum(rest), len(rest), i))
                    break


def cmd_cell(name, c, r, f0=None, f1=None):
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    sh = load_sheet(); raw = section(sh, name); lut = fg2.apply_lut(raw)
    for f, cx in frames_of(name):
        if f0 is not None and not (f0 <= f <= f1):
            continue
        st = cell_stats(np.asarray(mm[f]), lut, raw, cx, c, r)
        if st:
            print(f, '%.2f' % (f / 30), 'cx', cx, 'lech %.1f den %.2f' % st)


def cmd_shot(f, out, Z=3):
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    rows = cam_rows(); cr = rows[f]
    fr = np.asarray(mm[f])
    im = Image.fromarray(fr).resize((256 * Z, 224 * Z), Image.NEAREST).convert('RGBA')
    ov = Image.new('RGBA', im.size, (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    cx = int(cr['cx']) if cr['kind'] == 'game' else 0
    for x in range(-(cx % 16), 257, 16):
        d.line([x * Z, 24 * Z, x * Z, 216 * Z], fill=(255, 255, 255, 60))
        if 0 <= x < 256:
            d.text((x * Z + 2, 24 * Z + 2), str((cx + x) // 16), fill=(255, 255, 0, 255))
    for r in range(0, 13):
        y = 24 + r * 16
        d.line([0, y * Z, 256 * Z, y * Z], fill=(255, 255, 255, 60))
        d.text((2, y * Z + 2), str(r), fill=(0, 255, 255, 255))
    d.text((4, 4), 'f%d %.2fs %s cx %d' % (f, f / 30, cr['section'], cx), fill=(255, 255, 255, 255))
    Image.alpha_composite(im, ov).convert('RGB').save(out)


def cmd_teal():
    """3 o teal Hall C: moi pixel teal - ti le frame (trong khung, game) video den (<40 moi kenh); mau khong den pho bien"""
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    sh = load_sheet(); raw = section(sh, 'TowerHallC')
    teal = (raw[..., 0] == 0) & (raw[..., 1] == 128) & (raw[..., 2] == 128)
    ys, xs = np.nonzero(teal)
    acc_n = np.zeros(teal.shape); acc_b = np.zeros(teal.shape)
    for f, cx in frames_of('TowerHallC'):
        fr = np.asarray(mm[f])
        vis = (xs >= cx) & (xs < cx + 256)
        if not vis.any():
            continue
        yy, xx = ys[vis], xs[vis]
        v = fr[yy + 24, xx - cx].astype(int)
        acc_n[yy, xx] += 1
        acc_b[yy, xx] += (v.max(axis=1) < 40)
    for x0 in (352, 416, 480):
        print('bay x %d-%d:' % (x0, x0 + 31))
        for r0 in range(80, 144, 8):
            n = acc_n[r0:r0 + 8, x0:x0 + 32]; b = acc_b[r0:r0 + 8, x0:x0 + 32]
            frac = b / np.maximum(n, 1)
            print('  y %3d-%3d: so frame %d..%d, ti le den min %.2f TB %.2f max %.2f; pixel chua tung den: %d' % (
                r0, r0 + 7, n.min(), n.max(), frac.min(), frac.mean(), frac.max(), int((b == 0).sum())))


if __name__ == '__main__':
    a = sys.argv[1:]
    if a[0] == 'broken':
        cmd_broken()
    elif a[0] == 'cell':
        cmd_cell(a[1], int(a[2]), int(a[3]), *(int(x) for x in a[4:6]))
    elif a[0] == "teal":
        cmd_teal()
    elif a[0] == "shot":
        cmd_shot(int(a[1]), a[2], int(a[3]) if len(a) > 3 else 3)
