"""CVGP 13 — ghép phát hiện của enemy_fit.py thành VẾT (từng con) và tóm tắt: tốc độ (hồi quy world x theo tick = 2 × frame
video), chỗ/phía xuất hiện, chỗ biến mất, nhịp đổi khung, khoảng cách giữa các lần sinh.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/enemy_track.py <tag> [maxgap=8] [maxdx=10] [minlen=4] [key=worldx]
    đọc ClaudeNotes/cvgp/_cache/13/det_<tag>.csv → ghi trk_<tag>.csv (mỗi dòng = 1 phát hiện + id vết) và in tóm tắt
    key = worldx (đi trên sàn, camera cuộn) hoặc pivx (toạ độ màn)
"""
import csv, os, sys
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '13')


def load(tag):
    rows = list(csv.DictReader(open(os.path.join(D, 'det_%s.csv' % tag))))
    for r in rows:
        for k in ('frame', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'worldx'):
            r[k] = int(r[k])
        r['err'] = float(r['err'])
    return rows


def link(rows, maxgap, maxdx, key):
    tracks = []
    byf = {}
    for r in rows:
        byf.setdefault(r['frame'], []).append(r)
    for f in sorted(byf):
        dets = sorted(byf[f], key=lambda r: r['err'])
        used = set()
        for r in dets:
            best, bd = None, 1e9
            for t in tracks:
                last = t[-1]
                gap = f - last['frame']
                if gap <= 0 or gap > maxgap or id(t) in used:
                    continue
                pred = last[key]
                if len(t) >= 2 and t[-1]['frame'] != t[-2]['frame']:
                    v = (t[-1][key] - t[-2][key]) / float(t[-1]['frame'] - t[-2]['frame'])
                    pred = last[key] + v * gap
                d = abs(r[key] - pred) + abs(r['pivy'] - last['pivy'])
                if d <= maxdx + gap and d < bd:
                    best, bd = t, d
            if best is None:
                tracks.append([r])
                used.add(id(tracks[-1]))
            else:
                best.append(r)
                used.add(id(best))
    return tracks


def summarize(t, key):
    f = np.array([r['frame'] for r in t], float)
    x = np.array([r[key] for r in t], float)
    tick = 2 * f
    if len(t) >= 2 and tick[-1] > tick[0]:
        a, b = np.polyfit(tick, x, 1)
        res = x - (a * tick + b)
    else:
        a, res = 0.0, np.zeros(1)
    flips = []
    for i in range(1, len(t)):
        if t[i]['sprite'] != t[i - 1]['sprite']:
            flips.append(t[i]['frame'])
    return dict(f0=t[0]['frame'], f1=t[-1]['frame'], n=len(t), v=a, rms=float(np.sqrt((res ** 2).mean())),
                rmax=float(np.abs(res).max()), sx0=t[0]['pivx'], sx1=t[-1]['pivx'], wx0=t[0]['worldx'], wx1=t[-1]['worldx'],
                y0=t[0]['pivy'], y1=t[-1]['pivy'], cx0=t[0]['cx'], cx1=t[-1]['cx'], sec=t[0]['section'], flips=flips,
                face=t[0]['flip'], err=float(np.mean([r['err'] for r in t])))


def main():
    tag = sys.argv[1]
    maxgap = int(sys.argv[2]) if len(sys.argv) > 2 else 8
    maxdx = int(sys.argv[3]) if len(sys.argv) > 3 else 10
    minlen = int(sys.argv[4]) if len(sys.argv) > 4 else 4
    key = sys.argv[5] if len(sys.argv) > 5 else 'worldx'
    rows = load(tag)
    tracks = [t for t in link(rows, maxgap, maxdx, key) if len(t) >= minlen]
    tracks.sort(key=lambda t: t[0]['frame'])
    with open(os.path.join(D, 'trk_%s.csv' % tag), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['track', 'frame', 'sprite', 'flip', 'pivx', 'pivy', 'cx', 'worldx', 'err'])
        for i, t in enumerate(tracks):
            for r in t:
                w.writerow([i, r['frame'], r['sprite'], r['flip'], r['pivx'], r['pivy'], r['cx'], r['worldx'], r['err']])
    print('track f0-f1 n | v px/tick rms max | screen x0->x1 world x0->x1 | y0->y1 | cx0->cx1 sec | face err | frames doi khung')
    for i, t in enumerate(tracks):
        s = summarize(t, key)
        print('%3d f%d-%d n%d | v %+.3f rms %.2f max %.1f | sx %d->%d wx %d->%d | y %d->%d | cx %d->%d %s | flip%d e%.3f | %s' % (
            i, s['f0'], s['f1'], s['n'], s['v'], s['rms'], s['rmax'], s['sx0'], s['sx1'], s['wx0'], s['wx1'], s['y0'], s['y1'],
            s['cx0'], s['cx1'], s['sec'], s['face'], s['err'], ' '.join(map(str, s['flips']))))


if __name__ == '__main__':
    main()
