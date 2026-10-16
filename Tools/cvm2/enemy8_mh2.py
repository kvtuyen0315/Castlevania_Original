"""CVM2 08 — Medusa Head lượt 2: với P, A cố định (đo ở enemy8_mh.py) khớp từng vết (trk_<tag>.csv, ghép theo pivx) chỉ 2 tham số
(tâm c, pha) ⇒ thời điểm ĐỈNH sóng đầu tiên (giả thuyết: sinh ở đỉnh) + x màn ngoại suy lúc đó + Simon lúc đó ⇒ luật sinh
(mép, x, y theo Simon) và nhịp sinh (khoảng cách giữa các lần sinh trong cùng cửa sổ).
Chạy: python -P -E Tools/cvm2/enemy8_mh2.py <tag,...> [P=100.5] [A=33.75] [minlen=10] [out=ClaudeNotes/cvm2/ref/data/enemy8/mh_spawns.csv]
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')
TPF = 2.0033
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from enemy8_mh import simon, near  # noqa: E402


def main():
    tags = sys.argv[1].split(',')
    P = float(sys.argv[2]) if len(sys.argv) > 2 else 100.5
    A = float(sys.argv[3]) if len(sys.argv) > 3 else 33.75
    minlen = int(sys.argv[4]) if len(sys.argv) > 4 else 10
    out = sys.argv[5] if len(sys.argv) > 5 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'enemy8', 'mh_spawns.csv')
    sim = simon()
    rows = []
    for tag in tags:
        tr = {}
        for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
            tr.setdefault(int(r['track']), []).append(r)
        prev = None
        for k, t in sorted(tr.items(), key=lambda kv: int(kv[1][0]['frame'])):
            if len(t) < minlen:
                continue
            f = np.array([int(r['frame']) for r in t], float)
            tick = f * TPF
            wx = np.array([int(r['worldx']) for r in t], float)
            sx = np.array([int(r['pivx']) for r in t], float)
            y = np.array([int(r['pivy']) for r in t], float)
            vx = np.polyfit(tick, wx, 1)[0]
            best = None
            for t0 in np.arange(0.0, P, 0.1):
                g = np.sin(2 * np.pi * (tick - t0) / P)
                c = float(np.mean(y + A * g))
                e = float(np.sqrt(((y - (c - A * g)) ** 2).mean()))
                if best is None or e < best[0]:
                    best = (e, t0, c)
            e, t0, c = best
            # đỉnh (y nhỏ nhất) khi sin = 1 ⇒ t = t0 + P/4 + kP; lấy đỉnh cuối cùng <= frame đầu + 1 frame
            ttop = t0 + P / 4.0
            ttop += np.floor((tick[0] + TPF - ttop) / P) * P
            # x màn lúc ttop: hồi quy pivx theo tick 15 điểm đầu (camera có thể cuộn)
            m = min(15, len(t))
            a, b = np.polyfit(tick[:m], sx[:m], 1)
            sx_top = a * ttop + b
            ftop = int(round(ttop / TPF))
            s = near(sim, ftop)
            row = dict(tag=tag, track=k, section=t[0]['section'], f_first=int(f[0]), f_last=int(f[-1]), n=len(t),
                       dir=1 if vx > 0 else -1, vx='%.3f' % vx, rms='%.2f' % e, c='%.1f' % c, top_tick='%.1f' % ttop,
                       top_frame='%.1f' % (ttop / TPF), lead_ticks='%.1f' % (tick[0] - ttop), sx_top='%.1f' % sx_top,
                       sx_first=int(sx[0]), y_first=int(y[0]), simon_sx=s[0] if s else '', simon_Y=s[1] if s else '',
                       simon_face=s[2] if s else '', c_minus_Y='%.1f' % (c - s[1]) if s else '',
                       gap_ticks='%.1f' % (ttop - prev) if prev is not None else '')
            prev = ttop
            rows.append(row)
            print(' '.join('%s=%s' % (kk, row[kk]) for kk in ('tag', 'track', 'f_first', 'n', 'dir', 'vx', 'rms', 'c', 'top_frame', 'lead_ticks', 'sx_top', 'sx_first', 'simon_sx', 'simon_Y', 'simon_face', 'c_minus_Y', 'gap_ticks')))
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, 'w', newline='') as fo:
        w = csv.DictWriter(fo, fieldnames=list(rows[0].keys()))
        w.writeheader()
        w.writerows(rows)
    print('ghi', out, len(rows))


if __name__ == '__main__':
    main()
