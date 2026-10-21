"""CVM3 07 — xuất đường đi Simon từ video Map_3 (ref/data/map3_simon.csv) thành từng TICK cho Map3EnemyProbe ("Simon rối" — khuôn
Tools/cvm2/enemy8_puppet.py). Dùng frame KHÔNG lặp có Simon (found 1); frame thiếu Simon mà camera đang bám (0 < cx < cx max của section)
⇒ x = cx + 128 (camera bám tâm — CVM3 02), y giữ mặt sàn gần nhất. Giữa 2 mẫu nội suy tuyến tính, hướng lấy mẫu gần nhất.
tick t ↔ frame f0 + t / 2.0033. Toạ độ section Unity = video (4 section dùng ở đây không nới; WalkwayB sẽ phải + 112 — CVM3 05).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy7_puppet.py  ⇒ ClaudeNotes/cvm3/ref/data/enemy7/puppet_<đoạn>.csv (tick,x,y,face,frame)
"""
import csv, os
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy7')
TPF = 2.0033
CXMAX = {'WalkwayEntranceA': 512, 'WalkwayEntranceB': 512, 'WalkwayA': 512, 'MummyMan': 1280}
# đoạn so sánh (CVM3_MEMORY §2g): tên, section, frame đầu, frame cuối
SEGS = [
    ('sk_wea', 'WalkwayEntranceA', 1004, 1077),
    ('sk_web1', 'WalkwayEntranceB', 1516, 1553),
    ('sk_web2', 'WalkwayEntranceB', 2122, 2222),
    ('sk_wa', 'WalkwayA', 2984, 3072),
    ('sk_mm', 'MummyMan', 5120, 5222),
    ('fl_wea0', 'WalkwayEntranceA', 480, 546),
    ('fl_wea1', 'WalkwayEntranceA', 600, 676),
    ('fl_wea2', 'WalkwayEntranceA', 805, 918),
]


def main():
    sim = list(csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_simon.csv'))))
    cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv')))}
    os.makedirs(OUT, exist_ok=True)
    for name, sec, f0, f1 in SEGS:
        pts, fill = [], 0
        lasty, lastf = 176, -1
        for r in sim:
            f = int(r['frame'])
            if f < f0 - 6 or f > f1 + 6 or r['section'] != sec or r['dup'] != '0':
                continue
            if r['found'] == '1' and r['world_x']:
                lasty, lastf = int(float(r['y_sec'])), 1 if r['flip'] == '1' else -1
                pts.append((f, int(float(r['world_x'])), lasty, lastf))
            else:
                c = cam.get(f)
                cx = int(float(c['cx'])) if c and c['section'] == sec else -1
                if 0 < cx < CXMAX[sec]:
                    pts.append((f, cx + 128, None, None))
                    fill += 1
        # điền y / hướng cho mẫu camera bằng mẫu Simon gần nhất
        real = [p for p in pts if p[2] is not None]
        pts2 = []
        for p in pts:
            if p[2] is None:
                q = min(real, key=lambda q: abs(q[0] - p[0]))
                pts2.append((p[0], p[1], q[2], q[3]))
            else:
                pts2.append(p)
        fr = np.array([p[0] for p in pts2], float)
        xs = np.array([p[1] for p in pts2], float)
        ys = np.array([p[2] for p in pts2], float)
        T = int(round((f1 - f0) * TPF))
        with open(os.path.join(OUT, 'puppet_%s.csv' % name), 'w', newline='') as fo:
            w = csv.writer(fo, lineterminator='\n')
            w.writerow(['tick', 'x', 'y', 'face', 'frame'])
            for t in range(T + 1):
                f = f0 + t / TPF
                x = int(round(np.interp(f, fr, xs)))
                y = int(round(np.interp(f, fr, ys)))
                k = int(np.argmin(np.abs(fr - f)))
                w.writerow([t, x, y, pts2[k][3], '%.2f' % f])
        print(name, sec, 'f%d-%d' % (f0, f1), T + 1, 'tick,', len(real), 'frame Simon +', fill, 'frame camera')


if __name__ == '__main__':
    main()
