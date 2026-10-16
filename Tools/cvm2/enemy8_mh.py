"""CVM2 08 — Medusa Head: phân tích vết (trk_<tag>.csv của enemy8_track.py) ↔ Simon (ref/data/map2_simon.csv).
Chạy (từ gốc repo): python -P -E Tools/cvm2/enemy8_mh.py <tag,tag,...> [minlen=12] [out=ClaudeNotes/cvm2/ref/data/enemy8/mh_tracks.csv]
Mỗi vết: frame đầu/cuối, x màn đầu (mép sinh), hướng bay, vx hồi quy (px/tick, tick = frame × 2.0033), Simon lúc sinh (x màn, Y, hướng),
khớp sóng dọc 3 mô hình trên t = tick tương đối:
  sin  : y = c + A·sin(2π(t − t0)/P)
  para : cung parabol nối nhau (gia tốc hằng ±a hướng về đường tâm — chuyển động "nảy" quanh tâm), cùng c, A, P, t0
  tri  : sóng tam giác (vận tốc dọc hằng, đổi chiều ở đỉnh/đáy)
⇒ tham số tốt nhất + rms (px). Tóm tắt toàn bộ: trung vị P, A, tâm − Y Simon, pha lúc sinh.
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')
TPF = 2.0033


def shape(kind, ph):
    """ph = pha (chu kỳ 1) — giá trị chuẩn hoá [−1, 1], đi LÊN (y giảm) trước: trả g sao cho y = c − A·g"""
    ph = np.mod(ph, 1.0)
    if kind == 'sin':
        return np.sin(2 * np.pi * ph)
    if kind == 'para':
        u = np.mod(2 * ph, 1.0)
        g = 4 * u * (1 - u)
        return np.where(ph < 0.5, g, -g)
    if kind == 'tri':
        return np.where(ph < 0.25, 4 * ph, np.where(ph < 0.75, 2 - 4 * ph, 4 * ph - 4))
    raise ValueError(kind)


def fit(t, y, kind):
    best = None
    for P in np.arange(60.0, 160.0, 0.5):
        for t0 in np.arange(0.0, P, P / 96.0):
            g = shape(kind, (t - t0) / P)
            M = np.vstack([np.ones_like(g), -g]).T
            sol, res, *_ = np.linalg.lstsq(M, y, rcond=None)
            r = y - M @ sol
            e = float(np.sqrt((r ** 2).mean()))
            if best is None or e < best[0]:
                best = (e, P, t0, sol[0], sol[1])
    # tinh chỉnh quanh tốt nhất
    e0, P0, t00, _, _ = best
    for P in np.arange(P0 - 0.5, P0 + 0.5, 0.05):
        for t0 in np.arange(t00 - P0 / 96.0, t00 + P0 / 96.0, 0.1):
            g = shape(kind, (t - t0) / P)
            M = np.vstack([np.ones_like(g), -g]).T
            sol, *_ = np.linalg.lstsq(M, y, rcond=None)
            r = y - M @ sol
            e = float(np.sqrt((r ** 2).mean()))
            if e < best[0]:
                best = (e, P, t0, sol[0], sol[1])
    return best


def simon():
    s = {}
    for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_simon.csv'))):
        if r['found'] == '1':
            s[int(r['frame'])] = (int(r['px']), int(r['Y']), 1 if r['flip'] == '1' else -1, int(r['world_x']))
    return s


def near(sim, f):
    for d in range(0, 6):
        for g in (f - d, f + d):
            if g in sim:
                return sim[g]
    return None


def main():
    tags = sys.argv[1].split(',')
    minlen = int(sys.argv[2]) if len(sys.argv) > 2 else 12
    out = sys.argv[3] if len(sys.argv) > 3 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'enemy8', 'mh_tracks.csv')
    os.makedirs(os.path.dirname(out), exist_ok=True)
    sim = simon()
    rows = []
    for tag in tags:
        tr = {}
        for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
            tr.setdefault(int(r['track']), []).append(r)
        for k, t in sorted(tr.items()):
            if len(t) < minlen:
                continue
            f = np.array([int(r['frame']) for r in t], float)
            x = np.array([int(r['worldx']) for r in t], float)
            y = np.array([int(r['pivy']) for r in t], float)
            tick = (f - f[0]) * TPF
            vx = np.polyfit(tick, x, 1)[0]
            fits = {kd: fit(tick, y, kd) for kd in ('sin', 'para', 'tri')}
            s = near(sim, int(f[0]))
            e, P, t0, c, A = fits['sin']
            ph0 = np.mod(-t0 / P, 1.0)   # pha lúc vết bắt đầu (0 = ở tâm đi lên, 0.25 = đỉnh, 0.5 = tâm đi xuống, 0.75 = đáy)
            row = dict(tag=tag, track=k, section=t[0]['section'], f0=int(f[0]), f1=int(f[-1]), n=len(t), sx0=int(t[0]['pivx']),
                       cx0=int(t[0]['cx']), dir=1 if vx > 0 else -1, vx='%.3f' % vx, y0=int(y[0]), ymin=int(y.min()), ymax=int(y.max()),
                       simon_sx=s[0] if s else '', simon_Y=s[1] if s else '', simon_face=s[2] if s else '',
                       sin_rms='%.2f' % e, sin_P='%.1f' % P, sin_A='%.1f' % A, sin_c='%.1f' % c, sin_ph0='%.3f' % ph0,
                       c_minus_simonY='%.1f' % (c - s[1]) if s else '',
                       para_rms='%.2f' % fits['para'][0], para_P='%.1f' % fits['para'][1], para_A='%.1f' % fits['para'][4],
                       tri_rms='%.2f' % fits['tri'][0], tri_P='%.1f' % fits['tri'][1], tri_A='%.1f' % fits['tri'][4])
            rows.append(row)
            print('%s#%d %s f%d-%d n%d sx0 %d dir %+d vx %s y %d..%d | simon sx %s Y %s face %s | sin rms %s P %s A %s c %s ph0 %s | para rms %s P %s A %s | tri rms %s P %s' % (
                tag, k, row['section'], row['f0'], row['f1'], row['n'], row['sx0'], row['dir'], row['vx'], row['ymin'], row['ymax'],
                row['simon_sx'], row['simon_Y'], row['simon_face'], row['sin_rms'], row['sin_P'], row['sin_A'], row['sin_c'], row['sin_ph0'],
                row['para_rms'], row['para_P'], row['para_A'], row['tri_rms'], row['tri_P']))
    with open(out, 'w', newline='') as fo:
        w = csv.DictWriter(fo, fieldnames=list(rows[0].keys()))
        w.writeheader()
        w.writerows(rows)
    long_ = [r for r in rows if r['n'] >= 40]
    for kd in ('sin', 'para', 'tri'):
        print('%s: rms trung vi (vet >= 40) %.2f, P trung vi %.1f, A trung vi %.1f' % (
            kd, np.median([float(r[kd + '_rms']) for r in long_]), np.median([float(r[kd + '_P']) for r in long_]),
            np.median([float(r[kd + '_A']) for r in long_])))
    print('ghi', out, len(rows), 'vet')


if __name__ == '__main__':
    main()
