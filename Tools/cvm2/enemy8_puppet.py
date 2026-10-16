"""CVM2 08 — xuất đường đi Simon từ video Map_2 (ref/data/map2_simon.csv) thành từng TICK cho Map2EnemyProbe ("Simon rối" — Unity đặt
Simon đúng vị trí video mỗi tick để enemy phản ứng y như trong video). Chỉ dùng frame KHÔNG lặp (dup 0) có Simon; giữa 2 frame nội suy
tuyến tính x/y, hướng lấy frame gần nhất. tick t ↔ frame f0 + t / 2.0033.
Chạy (từ gốc repo): python -P -E Tools/cvm2/enemy8_puppet.py  ⇒ ClaudeNotes/cvm2/ref/data/enemy8/puppet_<đoạn>.csv (tick,x,y,face,frame)
"""
import csv, os
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'enemy8')
TPF = 2.0033
# đoạn so sánh (CVM2_MEMORY §2j): tên, section, frame đầu, frame cuối
SEGS = [
    ('mh_a', 'TowerHallA', 1762, 2060),
    ('mh_b', 'TowerHallB', 2801, 3160),
    ('mh_m', 'Medusa', 6180, 6580),
    ('sk_teb', 'TowerEntranceB', 863, 1008),
    ('sk_hb', 'TowerHallB', 3209, 3420),
    ('gh_1', 'TowerHallC', 4976, 5007),
    ('gh_2', 'TowerHallC', 5810, 5851),
    ('bat_c', 'TowerHallC', 4974, 5101),
    ('bat_tea', 'TowerEntranceA', 187, 776),
    ('bt_c', 'TowerHallC', 4922, 5530),
]


def main():
    rows = [r for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_simon.csv')))
            if r['found'] == '1' and r['dup'] == '0']
    os.makedirs(OUT, exist_ok=True)
    for name, sec, f0, f1 in SEGS:
        pts = [(int(r['frame']), int(r['world_x']), int(r['Y']) - 32, 1 if r['flip'] == '1' else -1) for r in rows
               if r['section'] == sec and f0 - 6 <= int(r['frame']) <= f1 + 6]
        fr = np.array([p[0] for p in pts], float)
        xs = np.array([p[1] for p in pts], float)
        ys = np.array([p[2] for p in pts], float)
        T = int(round((f1 - f0) * TPF))
        with open(os.path.join(OUT, 'puppet_%s.csv' % name), 'w', newline='') as fo:
            w = csv.writer(fo, lineterminator='\n')
            w.writerow(['tick', 'x', 'y', 'face', 'frame'])
            for t in range(T + 1):
                f = f0 + t / TPF
                x = int(round(np.interp(f, fr, xs)))
                y = int(round(np.interp(f, fr, ys)))
                k = int(np.argmin(np.abs(fr - f)))
                w.writerow([t, x, y, pts[k][3], '%.2f' % f])
        print(name, sec, 'f%d-%d' % (f0, f1), T + 1, 'tick,', len(pts), 'frame Simon')


if __name__ == '__main__':
    main()
