"""CVM2 10 — khung hiện vật thể NES ở video Map_2: với mỗi nến (ClaudeNotes/cvm2/_cache/10/candles_final.csv) và mỗi frame game không
lặp TRƯỚC lần thấy cuối (nến chưa vỡ), x màn = x − cx; đếm frame có / không phát hiện nến (props_det.csv của props10.py bước 1) theo x màn
gần mép trái/phải ⇒ cửa sổ hiện vật (kết quả: thấy 100 % ở x màn 8–248, 0 % ở 0–6 và 249–255 — MAP2_PLACEMENT §0).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/window10.py
"""
import csv, os
from collections import defaultdict
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C10 = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '10')


def main():
    rows = list(csv.DictReader(open(os.path.join(C10, 'props_det.csv'))))
    cam = {int(r['frame']): (r['kind'], r['section'], int(float(r['cx'])))
           for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')))}
    dup = np.load(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work', 'map2_dup.npy'))
    cand = list(csv.DictReader(open(os.path.join(C10, 'candles_final.csv'))))
    det = defaultdict(set)
    last = {}
    for r in rows:
        det[(r['section'], int(r['frame']))].add(int(r['x']))
        k = (r['section'], int(r['x']))
        last[k] = max(last.get(k, 0), int(r['frame']))
    hist = defaultdict(lambda: [0, 0])
    for c in cand:
        sec, x = c['section'], int(c['x'])
        lf = max(v for (s, xx), v in last.items() if s == sec and abs(xx - x) <= 1)
        for f, (kind, s, cx) in cam.items():
            if s != sec or kind != 'game' or dup[f] or f > lf:
                continue
            sx = x - cx
            if -4 <= sx <= 259:
                seen = any(abs(xx - x) <= 1 for xx in det.get((sec, f), ()))
                hist[sx][0] += 1
                hist[sx][1] += int(seen)
    for sx in list(range(0, 14)) + list(range(242, 256)):
        if sx in hist:
            print('x man %3d: %3d frame, thay %3d' % (sx, hist[sx][0], hist[sx][1]))


if __name__ == '__main__':
    main()
