"""CVGP 15 — nến/chậu nào bị phá trong Map_1 và rơi ra gì (khớp mẫu sprite item ngay tại chỗ nến sau khi vỡ).

Chạy (từ gốc repo, sau map1_props.py):  python -P -E Tools/cvgp/map1_drops.py
  Đầu vào: _cache/15/props_det.csv (phát hiện nến/chậu mỗi 3 frame mới) + work/map1_camera.csv (cx planner).
  Lưới nến (video + wiki §6 trùng nhau 57/57): CANDLES bên dưới (section, x tâm, y đáy pivot = Y CandleProp + 1).
  Với mỗi nến: frame cuối còn thấy khi vẫn trong màn (x màn 8..247) mà các frame sau KHÔNG thấy dù vẫn trong màn ⇒ vỡ ở đó;
  rồi khớp mẫu mọi sprite Item quanh tâm nến 8–22 frame sau (item hiện 20 tick = 10 frame sau khi vỡ — routine 11) ⇒ item.
  → _cache/15/drops.csv + in bảng.
"""
import csv, os, sys
from collections import Counter, defaultdict
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import item_fit as itf  # noqa: E402
import map1_props as mp  # noqa: E402

OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '15')
ITEMS = ['heart_large', 'heart_small', 'whip', 'dagger', 'axe', 'holywater', 'cross', 'stopwatch', 'rosary', 'double', 'triple',
         'moneybag_red', 'moneybag_purple', 'moneybag_white', 'roast', 'jar', '1up']


def grid():
    """(section, id, kind, x, y_pivot_det) — x = 32 + 64k (nến) / 96 + 128k (chậu); y = hàng đáy + 1 (khớp mẫu)."""
    out = [('CastleEntrance', 'CE_T%d' % (k + 1), 'torch', 96 + 128 * k, 161) for k in range(5)]
    eh = [161, 129, 161, 129, 161, 129, 161, 129, 161, 129, 65, 161, 33, 65, 161, 65, 161, 161, 161, 161, 161, 129, 65, 33]
    out += [('EntranceHall', 'EH_C%02d' % (k + 1), 'candle', 32 + 64 * k, y) for k, y in enumerate(eh)]
    ca = [161, 65, 49, 129, 49, 161, 33, 65]
    cax = [32, 96, 160, 224, 304, 352, 416, 480]
    out += [('CryptA', 'CA_C%d' % (k + 1), 'candle', x, y) for k, (x, y) in enumerate(zip(cax, ca))]
    cbx = [32, 96, 160, 224, 288, 352, 416, 496]
    out += [('CryptB', 'CB_C%d' % (k + 1), 'candle', x, 49) for k, x in enumerate(cbx)]
    pb = [129, 65, 97, 65, 161, 129, 161, 161, 129, 161, 129, 129]
    out += [('PhantomBat', 'PB_C%02d' % (k + 1), 'candle', 32 + 64 * k, y) for k, y in enumerate(pb)]
    return out


def main():
    cam = {}
    for r in csv.DictReader(open(os.path.join(itf.VID_DIR, 'work', 'map1_camera.csv'))):
        try:
            cam[int(r['frame'])] = (int(float(r['cx'])), r['section'])
        except ValueError:
            pass
    det = defaultdict(list)
    frames_seen = defaultdict(set)
    for r in csv.DictReader(open(os.path.join(OUT, 'props_det.csv'))):
        det[r['section']].append((int(r['frame']), int(r['x']), int(r['y'])))
    for r in csv.DictReader(open(os.path.join(OUT, 'props_scanned.csv'))):
        frames_seen[r['section']].add(int(r['frame']))
    segs = mp.SEGMENTS
    names = ['Item/' + n for n in ITEMS]
    sprites, pal = itf.load(names)
    res = []
    for sec, cid, kind, x, y in grid():
        scanned = sorted(f for s, f0, f1 in segs if s == sec for f in frames_seen[sec] if f0 <= f <= f1)
        # frame đã quét (có ít nhất 1 phát hiện trong section) mà nến nằm trong màn
        on = [f for f in scanned if f in cam and cam[f][1] == sec and 8 <= x - cam[f][0] <= 247]
        hit = {f for f, dx, dy in det[sec] if abs(dx - x) <= 6 and abs(dy - y) <= 3}
        seen_on = [f for f in on if f in hit]
        broken = None
        if seen_on:
            last = max(seen_on)
            after = [f for f in on if f > last]
            # vỡ nếu sau lần thấy cuối còn ≥ 3 lần quét trong màn mà không thấy, và lần quét kế ngay sau đó (< 12 frame)
            if len(after) >= 3 and after[0] - last < 12:
                broken = last
        item, votes, err = '', '', ''
        if broken is not None:
            c = Counter()
            errs = defaultdict(list)
            top = y - (32 if kind == 'torch' else 16) + 32   # hàng đỉnh vật (NES y)
            for f, nes in itf.frames('map1', broken + 8, broken + 22):
                if f not in cam:
                    continue
                sx = x - cam[f][0]
                box = (max(0, sx - 14), max(48, top - 10), min(256, sx + 14), min(224, top + 40))
                b = itf.match(nes, sprites, pal, box)
                if b and b['err'] <= 0.13:
                    n = b['s']['name'].split('/')[-1]
                    c[n] += 1
                    errs[n].append(b['err'])
            if c:
                item = c.most_common(1)[0][0]
                votes = ' '.join('%s:%d' % kv for kv in c.most_common(3))
                err = '%.3f' % np.mean(errs[item])
        res.append((sec, cid, kind, x, y - 1, len(on), len(seen_on), broken if broken is not None else '', item, votes, err))
        print(cid, sec, x, y - 1, 'on', len(on), 'seen', len(seen_on), 'vo' if broken else '--', broken or '', item, votes, err)
    with open(os.path.join(OUT, 'drops.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'id', 'kind', 'x', 'Y', 'scan_on', 'scan_seen', 'break_frame', 'item', 'votes', 'err'])
        w.writerows(res)


if __name__ == '__main__':
    main()
