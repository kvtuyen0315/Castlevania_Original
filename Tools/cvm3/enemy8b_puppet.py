"""CVM3 08 — xuất đường đi Simon video Map_3 từng TICK cho Map3EnemyProbe (đoạn Raven / Ghost / Medusa Head / Bone Tower) — y hệt
Tools/cvm3/enemy7_puppet.py (dùng lại hàm), chỉ khác danh sách đoạn + thư mục ra ref/data/enemy8b/ + WalkwayB cộng 112 (gốc section
Unity sau nới trái — CVM3 05).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_puppet.py  ⇒ ClaudeNotes/cvm3/ref/data/enemy8b/puppet_<đoạn>.csv (tick,x,y,face,frame)
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy8b')
TPF = 2.0033
CXMAX = {'WalkwayEntranceA': 512, 'WalkwayEntranceB': 512, 'WalkwayA': 512, 'WalkwayB': 512, 'MummyMan': 1280}
XOFF = {'WalkwayB': 112}
DEFAULT_Y = {'gh_wea': 176}   # đoạn không có frame Simon khớp mẫu: chân Simon theo ảnh
# đoạn so sánh: tên, section, frame đầu, frame cuối
SEGS = [
    ('rv_web0', 'WalkwayEntranceB', 1338, 1492),
    ('rv_web1', 'WalkwayEntranceB', 1572, 1732),
    ('rv_wb0', 'WalkwayB', 3592, 3644),
    ('rv_wb1', 'WalkwayB', 3863, 3957),
    ('rv_mm', 'MummyMan', 5084, 5122),
    ('gh_wea', 'WalkwayEntranceA', 414, 452),     # Simon không khớp mẫu f341–496 (CVM3 02 §6) ⇒ x = cx + 128, chân 176 (sàn WEA, ảnh gh_wea_z)
    ('mh_wa', 'WalkwayA', 2788, 3060),
    ('mh_mm', 'MummyMan', 5368, 5590),
    ('bt_mm', 'MummyMan', 5392, 5512),
]


def main():
    sim = list(csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_simon.csv'))))
    cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv')))}
    os.makedirs(OUT, exist_ok=True)
    for name, sec, f0, f1 in SEGS:
        off = XOFF.get(sec, 0)
        pts, fill = [], 0
        for r in sim:
            f = int(r['frame'])
            if f < f0 - 8 or f > f1 + 8 or r['section'] != sec or r['dup'] != '0':
                continue
            if r['found'] == '1' and r['world_x']:
                pts.append((f, int(float(r['world_x'])) + off, int(float(r['y_sec'])), 1 if r['flip'] == '1' else -1))
            else:
                c = cam.get(f)
                cx = int(float(c['cx'])) if c and c['section'] == sec else -9999
                if -112 < cx < CXMAX[sec]:
                    pts.append((f, cx + 128 + off, None, None))
                    fill += 1
        real = [p for p in pts if p[2] is not None]
        if not real:
            real = [(f0, 0, DEFAULT_Y[name], -1)]
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
