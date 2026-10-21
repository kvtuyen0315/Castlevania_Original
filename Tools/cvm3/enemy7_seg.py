"""CVM3 07 — tách vết Skeleton/Fleaman (trk_<tag>.csv) thành ĐOẠN: đứng (x giữ ±1), đi (x đơn điệu, y giữ), bay (y đổi) ⇒ mỗi đoạn:
frame đầu–cuối, tick, x/y đầu–cuối, vận tốc hồi quy px/tick; kèm vị trí Simon (ref/data/map3_simon.csv) ở đầu đoạn và nhịp đổi khung.
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy7_seg.py <tag> [track=-1] [video=map3]
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '07')
TPF = 2.0033


def simon(video):
    p = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', '%s_simon.csv' % video)
    out = {}
    for r in csv.DictReader(open(p)):
        if r.get('world_x'):
            out[int(r['frame'])] = (int(float(r['world_x'])), int(float(r['y_sec'])), r['sprite'], r['flip'])
    return out


def near(sim, f):
    for d in range(0, 6):
        for g in (f - d, f + d):
            if g in sim:
                return sim[g]
    return None


def main():
    tag = sys.argv[1]
    want = int(sys.argv[2]) if len(sys.argv) > 2 else -1
    video = sys.argv[3] if len(sys.argv) > 3 else 'map3'
    sim = simon(video)
    tr = {}
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        tr.setdefault(int(r['track']), []).append(r)
    for k, t in sorted(tr.items()):
        if want >= 0 and k != want:
            continue
        f = [int(r['frame']) for r in t]; x = [int(r['worldx']) for r in t]; y = [int(r['pivy']) for r in t]
        spr = [r['sprite'] + ('R' if r['flip'] == '1' else 'L') for r in t]
        print('== vet #%d %s f%d-%d' % (k, t[0]['section'], f[0], f[-1]))
        # đoạn: phân loại từng bước (đứng / đi / bay)
        segs = []
        cur = None
        for i in range(1, len(t)):
            dx, dy = x[i] - x[i - 1], y[i] - y[i - 1]
            kind = 'bay' if dy != 0 else ('di' if dx != 0 else 'dung')
            if kind == 'di':
                kind = 'di+' if dx > 0 else 'di-'
            if cur is None or cur[0] != kind:
                cur = [kind, i - 1, i]
                segs.append(cur)
            else:
                cur[2] = i
        # gộp đoạn đứng 1 bước nằm giữa 2 đoạn đi cùng chiều (frame lặp / nhiễu ±1)
        for kind, a, b in segs:
            fa, fb = f[a], f[b]
            ticks = (fb - fa) * TPF
            v = (x[b] - x[a]) / ticks if ticks else 0
            s = near(sim, fa)
            sm = ('Simon %d,%d %s%s' % s) if s else 'Simon ?'
            print('  %-5s f%d-%d (%5.1f tick) x %d->%d y %d->%d  v %+.3f  [%s]  %s' % (kind, fa, fb, ticks, x[a], x[b], y[a], y[b], v, ' '.join(spr[a:b + 1][:12]), sm))
        ch = [f[i] for i in range(1, len(t)) if spr[i][:-1] != spr[i - 1][:-1]]
        if len(ch) > 1:
            d = np.diff(ch)
            print('  doi khung (frame):', ch, ' khoang TB %.2f frame = %.1f tick' % (d.mean(), d.mean() * TPF))


if __name__ == '__main__':
    main()
