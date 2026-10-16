"""CVM2 08 — ghép phát hiện của enemy8_fit.py thành VẾT (ràng buộc liên tục: vị trí dự đoán theo vận tốc vết) + tóm tắt từng vết.
Chạy (từ gốc repo):
  python -P -E Tools/cvm2/enemy8_track.py <tag> [errmax=0.12] [maxgap=6] [maxd=8] [minlen=4] [key=worldx|pivx] [dump=0]
  → ClaudeNotes/cvm2/_cache/08/trk_<tag>.csv (mỗi phát hiện + id vết) và in mỗi vết: frame đầu–cuối, số mẫu, tick, x/y đầu–cuối,
    vận tốc hồi quy (px/tick; tick = frame × 2.0033), rms dư, các đoạn đổi chiều (x giảm/tăng), mốc đổi khung.
dump=1 in từng điểm (frame, khung, lật, x, y, err).
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')
TPF = 2.0033   # tick NES / frame video (CVM2 02)


def load(tag, errmax):
    rows = []
    for r in csv.DictReader(open(os.path.join(D, 'det_%s.csv' % tag))):
        for k in ('frame', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'worldx', 'ysec'):
            r[k] = int(r[k])
        r['err'] = float(r['err'])
        if r['err'] <= errmax:
            rows.append(r)
    return rows


def link(rows, maxgap, maxd, key):
    tracks = []
    byf = {}
    for r in rows:
        byf.setdefault(r['frame'], []).append(r)
    for f in sorted(byf):
        used = set()
        for r in sorted(byf[f], key=lambda r: r['err']):
            best, bd = None, 1e9
            for t in tracks:
                last = t[-1]
                gap = f - last['frame']
                if gap <= 0 or gap > maxgap or id(t) in used:
                    continue
                px, py = last[key], last['pivy']
                if len(t) >= 2:
                    a = t[-2] if len(t) < 4 else t[-4]
                    df = float(last['frame'] - a['frame'])
                    px += (last[key] - a[key]) / df * gap
                    py += (last['pivy'] - a['pivy']) / df * gap
                d = abs(r[key] - px) + abs(r['pivy'] - py)
                if d <= maxd + 2 * gap and d < bd:
                    best, bd = t, d
            if best is None:
                tracks.append([r])
                used.add(id(tracks[-1]))
            else:
                best.append(r)
                used.add(id(best))
    return tracks


def runs(t, key):
    """đoạn đơn điệu của x (bỏ dao động ±1): trả list (frame đầu, frame cuối, dấu)"""
    out = []
    cur = None
    for i in range(1, len(t)):
        dx = t[i][key] - t[i - 1][key]
        s = (dx > 0) - (dx < 0)
        if s == 0:
            continue
        if cur is None or cur[2] != s:
            if cur is not None:
                out.append(cur)
            cur = [t[i - 1]['frame'], t[i]['frame'], s]
        else:
            cur[1] = t[i]['frame']
    if cur is not None:
        out.append(cur)
    return out


def summarize(t, key):
    f = np.array([r['frame'] for r in t], float)
    x = np.array([r[key] for r in t], float)
    y = np.array([r['pivy'] for r in t], float)
    tick = TPF * f
    if len(t) >= 2 and tick[-1] > tick[0]:
        a, b = np.polyfit(tick, x, 1)
        res = x - (a * tick + b)
    else:
        a, res = 0.0, np.zeros(1)
    flips = [t[i]['frame'] for i in range(1, len(t)) if t[i]['sprite'] != t[i - 1]['sprite']]
    return dict(f0=int(f[0]), f1=int(f[-1]), n=len(t), v=float(a), rms=float(np.sqrt((res ** 2).mean())),
                x0=int(x[0]), x1=int(x[-1]), y0=int(y[0]), y1=int(y[-1]), ymin=int(y.min()), ymax=int(y.max()),
                sec=t[0]['section'], flips=flips, face=t[0]['flip'], err=float(np.mean([r['err'] for r in t])))


def main():
    tag = sys.argv[1]
    errmax = float(sys.argv[2]) if len(sys.argv) > 2 else 0.12
    maxgap = int(sys.argv[3]) if len(sys.argv) > 3 else 6
    maxd = int(sys.argv[4]) if len(sys.argv) > 4 else 8
    minlen = int(sys.argv[5]) if len(sys.argv) > 5 else 4
    key = sys.argv[6] if len(sys.argv) > 6 else 'worldx'
    dump = int(sys.argv[7]) if len(sys.argv) > 7 else 0
    tracks = [t for t in link(load(tag, errmax), maxgap, maxd, key) if len(t) >= minlen]
    tracks.sort(key=lambda t: t[0]['frame'])
    with open(os.path.join(D, 'trk_%s.csv' % tag), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['track', 'frame', 'sprite', 'flip', 'pivx', 'pivy', 'cx', 'section', 'worldx', 'ysec', 'err'])
        for i, t in enumerate(tracks):
            for r in t:
                w.writerow([i, r['frame'], r['sprite'], r['flip'], r['pivx'], r['pivy'], r['cx'], r['section'], r['worldx'], r['ysec'], r['err']])
    for i, t in enumerate(tracks):
        s = summarize(t, key)
        print('#%d %s f%d-%d n%d  %s %d->%d  y %d->%d [%d..%d]  v %.3f px/tick rms %.2f  lat%d err %.3f' % (
            i, s['sec'], s['f0'], s['f1'], s['n'], key, s['x0'], s['x1'], s['y0'], s['y1'], s['ymin'], s['ymax'], s['v'], s['rms'], s['face'], s['err']))
        print('   doan x:', ' '.join('%d-%d%s' % (a, b, '+' if s_ > 0 else '-') for a, b, s_ in runs(t, key)))
        print('   doi khung:', s['flips'][:40])
        if dump:
            print('   ', ' '.join('%d:%s%s:%d,%d:%.2f' % (r['frame'], r['sprite'][-1], 'R' if r['flip'] else 'L', r[key], r['pivy'], r['err'] * 100) for r in t))


if __name__ == '__main__':
    main()
