"""CVM3 09 — đường đi Simon từng TICK cho Map3BossProbe ("Simon rối" — khuôn Tools/cvm3/enemy7_puppet.py) ở 2 đoạn mở trận Mummy Men:
  m3_open  video Map_3 f5931 (camera vừa khoá cx 1280) → f6160: ref/data/map3_simon.csv (world_x, y_sec, flip — section MummyMan)
  b3_open  video Boss_Map_3 f374 (khoá) → f540 (trước lần trúng đòn đầu f542): ref/data/boss3_simon.csv (px + 1280, Y − 32, flip)
  m3_death video Map_3 f6680 → f6930: Mummy 1 chết f6686, Mummy 2 f6771, quả cầu f6829.5 → nhặt f6920
Frame không lặp có Simon (found 1); giữa 2 mẫu nội suy x/y tuyến tính, hướng = mẫu gần nhất. tick t ↔ frame f0 + t / 2.0033.
Chạy (từ gốc repo): python -P -E Tools/cvm3/boss9m_puppet.py ⇒ ClaudeNotes/cvm3/ref/data/boss9m/puppet_<đoạn>.csv (tick,x,y,face,frame)
"""
import csv, os
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
DATA = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
OUT = os.path.join(DATA, 'boss9m')
TPF = 2.0033
SEGS = [('m3_open', 'map3', 5931, 6160), ('b3_open', 'boss3', 374, 540), ('m3_death', 'map3', 6680, 6930)]


def samples(vid, f0, f1):
    pts = []
    for r in csv.DictReader(open(os.path.join(DATA, '%s_simon.csv' % vid))):
        f = int(r['frame'])
        if f < f0 - 6 or f > f1 + 6 or r['dup'] != '0' or r['found'] != '1':
            continue
        if vid == 'map3':
            if r['section'] != 'MummyMan' or not r['world_x']:
                continue
            x, y = int(float(r['world_x'])), int(float(r['y_sec']))
        else:
            x, y = 1280 + int(float(r['px'])), int(float(r['Y'])) - 32
        pts.append((f, x, y, 1 if r['flip'] == '1' else -1))
    return pts


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, vid, f0, f1 in SEGS:
        pts = samples(vid, f0, f1)
        fr = np.array([p[0] for p in pts], float)
        xs = np.array([p[1] for p in pts], float)
        ys = np.array([p[2] for p in pts], float)
        T = int(round((f1 - f0) * TPF))
        with open(os.path.join(OUT, 'puppet_%s.csv' % name), 'w', newline='') as fo:
            w = csv.writer(fo, lineterminator='\n')
            w.writerow(['tick', 'x', 'y', 'face', 'frame'])
            for t in range(T + 1):
                f = f0 + t / TPF
                k = int(np.argmin(np.abs(fr - f)))
                w.writerow([t, int(round(np.interp(f, fr, xs))), int(round(np.interp(f, fr, ys))), pts[k][3], '%.2f' % f])
        print(name, vid, 'f%d-%d' % (f0, f1), T + 1, 'tick,', len(pts), 'frame Simon')


if __name__ == '__main__':
    main()
