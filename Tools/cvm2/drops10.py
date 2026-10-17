"""CVM2 10 — nến Block 2 nào bị phá trong video Map_2, lúc nào, và vật gì rơi ra (khớp mẫu item quanh nến sau lúc vỡ + sự kiện HUD
ref/data/map2_events.txt). Đầu vào: ClaudeNotes/cvm2/_cache/10/props_det.csv (props10.py bước 1) — vị trí nến lấy từ bảng cụm cố định
CANDLES (đã chốt, MAP2_PLACEMENT §1).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/drops10.py
  → ClaudeNotes/cvm2/_cache/10/drops.txt: mỗi nến: frame thấy cuối, frame mất khi còn trong khung (= vỡ), item khớp mẫu tốt nhất trong
    45 frame sau (sprite, frame, vị trí, err), sự kiện HUD trong 150 frame sau.
"""
import csv, json, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import enemy8_fit as ef  # noqa: E402

OUT = os.path.join(ef.ROOT, 'ClaudeNotes', 'cvm2', '_cache', '10')
REF = os.path.join(ef.ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data')
ITEMS = ['Item/heart_small', 'Item/heart_large', 'Item/moneybag_red', 'Item/moneybag_purple', 'Item/moneybag_white', 'Item/dagger',
         'Item/stopwatch', 'Item/cross', 'Item/axe', 'Item/holywater', 'Item/roast', 'Item/double', 'Item/triple', 'Item/rosary',
         'Item/whip', 'Item/jar', 'Item/1up']


def candles():
    out = []
    for r in csv.DictReader(open(os.path.join(OUT, 'candles_final.csv'))):
        out.append((r['section'], int(r['x']), int(r['ymeas'])))
    return out


def main():
    cam = ef.camera()
    det = {}
    for r in csv.DictReader(open(os.path.join(OUT, 'props_det.csv'))):
        det.setdefault(r['section'], []).append((int(r['frame']), int(r['x']), int(r['y'])))
    dup = np.load(os.path.join(ef.WORK, 'map2_dup.npy'))
    mm = np.load(os.path.join(ef.WORK, 'map2_nes.npy'), mmap_mode='r')
    events = [l.rstrip('\n') for l in open(os.path.join(REF, 'map2_events.txt'), encoding='utf-8') if l.startswith('f')]
    ev = [(int(l.split()[0][1:]), l) for l in events]
    pal = {}
    sprites = {}
    for n in ITEMS:
        s, cols = ef.load([n])
        sprites[n] = (s, cols)
    lines = []
    for sec, x, y in candles():
        fs = sorted(f for f, xx, yy in det.get(sec, []) if abs(xx - x) <= 2 and abs(yy - y) <= 2)
        if not fs:
            lines.append('%s x %d y %d: KHÔNG thấy' % (sec, x, y))
            continue
        f_last = fs[-1]
        # còn trong khung sau lần thấy cuối? (≥ 3 frame game không lặp liên tiếp cùng section, x màn 6..250)
        gone = None
        k = 0
        for f in range(f_last + 1, f_last + 40):
            c = cam.get(f)
            if c is None or c[0] != 'game' or c[1] != sec or dup[f]:
                continue
            sx = x - c[2]
            if 6 <= sx <= 250:
                k += 1
                if k >= 3:
                    gone = f
                    break
            else:
                break
        s = '%s x %d y %d: thấy f%d–f%d (%d lần)' % (sec, x, y, fs[0], f_last, len(fs))
        if gone is None:
            lines.append(s + ' — rời khung (không phá trong video)')
            continue
        s += ' — VỠ ≈ f%d (%s)' % (f_last + 1, '%d:%05.2f' % ((f_last + 1) / 30 // 60, (f_last + 1) / 30 % 60))
        # item quanh nến sau lúc vỡ
        best = []
        for n in ITEMS:
            sp, cols = sprites[n]
            palv = [tuple(c) if sum(c) == 0 else tuple(int(v * 0.85) for v in c) for c in cols]
            for f in range(f_last + 1, f_last + 46):
                c = cam.get(f)
                if c is None or c[0] != 'game' or c[1] != sec or dup[f]:
                    continue
                sx = x - c[2]
                box = [max(0, sx - 20), max(48, y + 32 - 24), min(256, sx + 20), 224]
                if box[2] - box[0] < 18:
                    continue
                for e, spr, xx, yy, px, py in ef.detect(ef.err_maps(np.asarray(mm[f]), sp, palv, box), box, 0.2, (8, 8))[:1]:
                    best.append((e, f, n.split('/')[-1], c[2] + px, py - 32))
        best.sort()
        tops = []
        for b in best:
            if all(b[2] != t[2] for t in tops):
                tops.append(b)
            if len(tops) >= 3:
                break
        s += '\n    item: ' + '; '.join('%s f%d (%d,%d) err %.3f' % (t[2], t[1], t[3], t[4], t[0]) for t in tops)
        near = [l for f, l in ev if f_last <= f <= f_last + 150]
        s += '\n    HUD: ' + (' || '.join(l.split('|')[1].strip() + ' @f' + l.split()[0][1:] + ' (' + l.split('|')[2].strip() + ')' for l in near) if near else '-')
        lines.append(s)
    open(os.path.join(OUT, 'drops.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    print('ghi drops.txt', len(lines))


if __name__ == '__main__':
    main()
