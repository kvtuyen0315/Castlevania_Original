"""CVM3 08 — Medusa Head Map_3: khớp từng vết (trk_<tag>.csv của enemy8b_track.py) bằng sóng CVM2 08 (P 100, A 33.75 CỐ ĐỊNH — khuôn
Tools/cvm2/enemy8_mh2.py) ⇒ rms (kiểm số dùng chung), thời điểm ĐỈNH sóng đầu (= lúc sinh, CVM2 08), x màn lúc đó, Simon lúc đó (mép trước
mặt / sau lưng, đỉnh − chân), khoảng cách 2 lần sinh liên tiếp trong cùng section ⇒ bảng vùng sinh cho routine 10.
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_mh.py [tag=mh_all] [P=100] [A=33.75] [minlen=10]
  → ClaudeNotes/cvm3/ref/data/enemy8b/mh_spawns.csv + in bảng.
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy8b')
TPF = 2.0033


def simon():
    out = {}
    for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_simon.csv'))):
        if r['found'] == '1' and r['px']:
            out[int(r['frame'])] = (int(float(r['px'])), int(float(r['Y'])), 1 if r['flip'] == '1' else -1, int(float(r['world_x'])), r['sprite'])
    return out


def near(sim, f, span=8):
    for d in range(span + 1):
        for g in (f - d, f + d):
            if g in sim:
                return sim[g]
    return None


def main():
    tag = sys.argv[1] if len(sys.argv) > 1 else 'mh_all'
    P = float(sys.argv[2]) if len(sys.argv) > 2 else 100.0
    A = float(sys.argv[3]) if len(sys.argv) > 3 else 33.75
    minlen = int(sys.argv[4]) if len(sys.argv) > 4 else 10
    sim = simon()
    tr = {}
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        tr.setdefault(int(r['track']), []).append(r)
    rows, prev = [], {}
    for k, t in sorted(tr.items(), key=lambda kv: int(kv[1][0]['frame'])):
        if len(t) < minlen:
            continue
        f = np.array([int(r['frame']) for r in t], float)
        tick = f * TPF
        wx = np.array([int(r['worldx']) for r in t], float)
        sx = np.array([int(r['pivx']) for r in t], float)
        y = np.array([int(r['pivy']) for r in t], float)
        vx, bx = np.polyfit(tick, wx, 1)
        rx = float(np.sqrt(((wx - (vx * tick + bx)) ** 2).mean()))
        best = None
        for t0 in np.arange(0.0, P, 0.1):
            g = np.cos(2 * np.pi * (tick - t0) / P)        # đỉnh (y nhỏ nhất) ở t0 + kP
            top = float(np.mean(y - A * (1 - g)))
            e = float(np.sqrt(((y - (top + A * (1 - g))) ** 2).mean()))
            if best is None or e < best[0]:
                best = (e, t0, top)
        e, t0, top = best
        ttop = t0 + np.floor((tick[0] + TPF - t0) / P) * P   # đỉnh cuối cùng <= frame đầu + 1 frame
        m = min(15, len(t))
        a, b = np.polyfit(tick[:m], sx[:m], 1)
        sx_top = a * ttop + b
        ftop = int(round(ttop / TPF))
        s = near(sim, ftop)
        sec = t[0]['section']
        gap = ttop - prev[sec] if sec in prev else None
        prev[sec] = ttop
        front = ''
        if s:
            d = 1 if vx > 0 else -1       # bay sang phải ⇒ sinh ở mép trái
            front = 'truoc' if (-d) == s[2] else 'sau'
        row = dict(track=k, section=sec, f_first=int(f[0]), f_last=int(f[-1]), n=len(t), dir=1 if vx > 0 else -1,
                   vx='%.3f' % vx, rms_x='%.2f' % rx, rms_y='%.2f' % e, top_pivy='%.1f' % top, top_frame='%.1f' % (ttop / TPF),
                   lead='%.1f' % (tick[0] - ttop), sx_top='%.1f' % sx_top, wx_top='%.1f' % (vx * ttop + bx),
                   simon_sx=s[0] if s else '', simon_Y=s[1] if s else '', simon_face=s[2] if s else '', simon_wx=s[3] if s else '',
                   top_minus_feet='%.1f' % (top - s[1]) if s else '', edge=front, gap_ticks='%.1f' % gap if gap is not None else '')
        rows.append(row)
        print(' '.join('%s=%s' % kv for kv in row.items()))
    os.makedirs(OUT, exist_ok=True)
    p = os.path.join(OUT, 'mh_spawns.csv')
    with open(p, 'w', newline='') as fo:
        w = csv.DictWriter(fo, fieldnames=list(rows[0].keys()), lineterminator='\n')
        w.writeheader()
        w.writerows(rows)
    print('ghi', p, len(rows))


if __name__ == '__main__':
    main()
