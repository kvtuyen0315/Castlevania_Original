# terrain3.py (CVM3 05) - doi chieu dia hinh Level 3 voi video Map_3 / Boss_Map_3 tren luoi NES (bo dem extract_nes3 cua routine 02).
# Khuon Tools/cvm2/terrain5.py (CVM2 05). Chay tu goc repo: python -P -E Tools/cvm3/terrain3.py <lenh> ...
#   broken [map3|boss3]           : quet moi o 16x16 moi section qua moi frame game: o "thanh den" (khoi bi pha) va giu den toi het doan
#   cell <video> <Section> c r [f0 f1] : in theo frame do lech + ti le den cua o (c, r) (toa do section SHEET, WalkwayB chua noi)
#   shot <video> <frame> <out.png> [Z]  : anh frame luoi NES phong to + luoi 16 px theo cx (de xem)
# Can: ClaudeNotes/cvm3/_cache/02/work/<video>_nes.npy (Tools/cvm3/extract_nes3.py) + ref/data/<video>_camera.csv
# Boss_Map_3 bang mau khac: so "den" (max kenh < 40) van dung duoc; lech TB so voi LUT boss3 (fg3.lut('boss3')).
import os, sys, csv
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fg3
from nes3 import SECTIONS, load_sheet, section
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
CAMS = {'map3': os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv'),
        'boss3': os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'boss3_camera.csv')}
# hang 0 cua bo dem = NES y 8; hang section r <-> NES y 32 + r <-> bo dem r + 24


def cam_rows(video):
    return list(csv.DictReader(open(CAMS[video])))


def frames_of(video, section_name):
    out = []
    for r in cam_rows(video):
        if r['section'] == section_name and r['cx'] not in ('', '-1') and r.get('kind', 'game') not in ('other', 'black'):
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


def cmd_broken(video):
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    sh = load_sheet()
    for name in SECTIONS:
        raw = section(sh, name)
        lut = fg3.apply_lut(raw, video)
        fl = frames_of(video, name)
        if not fl:
            continue
        hist = {}
        for f, cx in fl[::2]:
            if f >= mm.shape[0]:
                continue
            fr = np.asarray(mm[f])
            for r in range(1, 12):
                for c in range(max(0, cx // 16), (cx + 256) // 16 + 1):
                    st = cell_stats(fr, lut, raw, cx, c, r)
                    if st is None:
                        continue
                    hist.setdefault((c, r), []).append((f, st[1]))
        for (c, r), seq in sorted(hist.items()):
            bl = [b > 0.85 for _f, b in seq]
            for i, b in enumerate(bl):
                if b:
                    rest = bl[i:]
                    if len(rest) >= 8 and sum(rest) >= 0.9 * len(rest):
                        print('%-17s o (%2d,%2d) den tu f%d (%.2f s) - %d/%d mau sau do den; mau truoc %d' % (
                            name, c, r, seq[i][0], seq[i][0] / 30, sum(rest), len(rest), i))
                    break


def cmd_cell(video, name, c, r, f0=None, f1=None):
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    sh = load_sheet(); raw = section(sh, name); lut = fg3.apply_lut(raw, video)
    for f, cx in frames_of(video, name):
        if f0 is not None and not (f0 <= f <= f1):
            continue
        st = cell_stats(np.asarray(mm[f]), lut, raw, cx, c, r)
        if st:
            print(f, '%.2f' % (f / 30), 'cx', cx, 'lech %.1f den %.2f' % st)


def cmd_shot(video, f, out, Z=3):
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    rows = cam_rows(video); cr = rows[f]
    fr = np.asarray(mm[f])
    im = Image.fromarray(fr).resize((256 * Z, 224 * Z), Image.NEAREST).convert('RGBA')
    ov = Image.new('RGBA', im.size, (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    try:
        cx = int(cr['cx'])
    except ValueError:
        cx = 0
    if cx == -1 and cr['section'] in ('-', ''):
        cx = 0
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


if __name__ == '__main__':
    a = sys.argv[1:]
    if a[0] == 'broken':
        cmd_broken(a[1] if len(a) > 1 else 'map3')
    elif a[0] == 'cell':
        cmd_cell(a[1], a[2], int(a[3]), int(a[4]), *(int(x) for x in a[5:7]))
    elif a[0] == 'shot':
        cmd_shot(a[1], int(a[2]), a[3], int(a[4]) if len(a) > 4 else 3)
