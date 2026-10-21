"""CVM3 08 — phân tích vết Raven Map_3 (trk_rv_all.csv của enemy8b_track.py) + vị trí Simon (ref/data/map3_simon.csv).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_raven.py [tag=rv_all] [chi_tiet=0]
In mỗi vết: chỗ đậu, frame cất cánh (khung đầu ≠ raven_0), Simon lúc cất cánh (x, chân, |dx|, dy), các CHẶNG bay (tách bởi đoạn đứng yên
≥ 3 frame liền): frame đầu–cuối, tick, dx/dy, vx hồi quy (px/tick), y cuối chặng (đích dọc), Simon lúc bắt đầu chặng.
Toạ độ: x thế giới = pivot (tâm sprite 16×16, gốc SHEET — WalkwayB Unity = +112), ysec = pivot NES − 32 (Unity Y của pivot tâm).
tick = frame × 2.0033.
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')
SIMON = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_simon.csv')
TPF = 2.0033


def simon():
    out = {}
    for r in csv.DictReader(open(SIMON)):
        if r['found'] == '1' and r['world_x']:
            out[int(r['frame'])] = (int(float(r['world_x'])), int(float(r['y_sec'])), r['sprite'], r['flip'], r['conf'])
    return out


def near(sm, f, span=6):
    for d in range(span + 1):
        for g in (f - d, f + d):
            if g in sm:
                return g, sm[g]
    return None, None


def tracks(tag):
    tr = {}
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        tr.setdefault(int(r['track']), []).append(dict(f=int(r['frame']), s=r['sprite'], fl=int(r['flip']), x=int(r['worldx']),
                                                         y=int(r['ysec']), sec=r['section'], cx=int(r['cx'])))
    return tr


def legs(pts):
    """tách chặng bay: bỏ đoạn đậu (raven_0), rồi cắt ở đoạn đứng yên (x, y không đổi) ≥ 3 điểm liền."""
    fly = [p for p in pts if p['s'] != 'raven_0']
    out, cur, still = [], [], []
    for p in fly:
        if cur and p['x'] == cur[-1]['x'] and p['y'] == cur[-1]['y']:
            still.append(p)
            continue
        if len(still) >= 2:
            out.append(('move', cur))
            out.append(('hold', [cur[-1]] + still))
            cur = [still[-1], p]
        else:
            cur += still + [p]
        still = []
    if cur:
        out.append(('move', cur))
    if len(still) >= 2:
        out.append(('hold', [cur[-1]] + still))
    return out


def main():
    tag = sys.argv[1] if len(sys.argv) > 1 else 'rv_all'
    det = int(sys.argv[2]) if len(sys.argv) > 2 else 0
    sm = simon()
    for k, pts in sorted(tracks(tag).items()):
        perch = [p for p in pts if p['s'] == 'raven_0']
        to = next((p for p in pts if p['s'] != 'raven_0'), None)
        print('#%d %s f%d-%d' % (k, pts[0]['sec'], pts[0]['f'], pts[-1]['f']))
        if perch:
            print('   dau (%d,%d) f%d-%d cx %d..%d' % (perch[0]['x'], perch[0]['y'], perch[0]['f'], perch[-1]['f'], perch[0]['cx'], perch[-1]['cx']))
        if to:
            g, s = near(sm, to['f'])
            if s:
                print('   cat canh f%d (%.1f s) Simon f%d x %d chan %d %s%s |dx| %d  dy(chan-raven) %d' % (
                    to['f'], to['f'] / 30.0, g, s[0], s[1], s[2], 'R' if s[3] == '1' else 'L', abs(s[0] - to['x']), s[1] - to['y']))
            for kind, L in legs(pts):
                f0, f1 = L[0]['f'], L[-1]['f']
                t = np.array([p['f'] for p in L], float) * TPF
                x = np.array([p['x'] for p in L], float)
                y = np.array([p['y'] for p in L], float)
                vx = np.polyfit(t, x, 1)[0] if len(L) > 2 else 0.0
                g, s = near(sm, f0)
                ss = (' Simon x %d chan %d' % (s[0], s[1])) if s else ''
                print('   %s f%d-%d (%.0f tick) (%d,%d)->(%d,%d) vx %.3f%s' % (kind, f0, f1, (f1 - f0) * TPF, x[0], y[0], x[-1], y[-1], vx, ss))
                if det and kind == 'move':
                    print('      ', ' '.join('%d:%s%s%d,%d' % (p['f'], p['s'][-1], 'R' if p['fl'] else 'L', p['x'], p['y']) for p in L))


if __name__ == '__main__':
    main()
