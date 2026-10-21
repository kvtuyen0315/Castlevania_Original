"""CVM3 07 — khớp mô hình Skeleton / xương / Fleaman từ vết video Map_3 (trk_<tag>.csv của enemy7_track.py) ⇒ bảng số đo
ClaudeNotes/cvm3/ref/data/enemy7/models.txt (+ tracks.csv: mọi điểm vết đã dùng). tick = frame × 2.0033 (đồng hồ frame — lệch ±15 %
trong cửa sổ ngắn; tốc độ đi kiểm thêm bằng "đồng hồ Simon" 1 px/tick, xem §2g).

Dựng lại từ đầu (từ gốc repo, cần bộ đệm CVM3 02 _cache/02/work/map3_nes.npy):
  P=Tools/cvm3; S=Enemy/WhiteSkeleton; F=Enemy/Fleaman
  python -P -E $P/enemy7_fit.py fl_wea $F/fleaman_0,$F/fleaman_1 207 1186 0 48 256 224 0.14 $P/enemy7_pal_flea_map3.json
  python -P -E $P/enemy7_track.py fl_wea 0.10 8 10 4
  for w in "sw_wea 990 1080" "sw_web1 1480 1556" "sw_web2 2090 2240" "sw_wa 2940 3075" "sw_mm 5080 5300"; do set -- $w
    python -P -E $P/enemy7_fit.py $1 $S/skeleton_0,$S/skeleton_1 $2 $3 0 48 256 224 0.16 $P/enemy7_pal_skel.json
    python -P -E $P/enemy7_track.py $1 0.15 8 14 4; done
  for w in "bw_wea 1000 1110" "bw_web1 1500 1570" "bw_web2 2100 2240" "bw_wa 2960 3100" "bw_mm 5100 5240"; do set -- $w
    python -P -E $P/enemy7_fit.py $1 $S/bone_0,$S/bone_1,$S/bone_2 $2 $3 0 48 256 224 0.15 $P/enemy7_pal_skel.json
    python -P -E $P/enemy7_track.py $1 0.15 6 14 4; done
  python -P -E $P/enemy7_models.py
Rồi: enemy7_puppet.py ⇒ Unity Map3EnemyProbe ⇒ enemy7_compare.py / enemy7_pairs.py.
"""
import csv, itertools, os
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '07')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy7')
TPF = 2.0033
LINES = []


def out(s=''):
    LINES.append(s)
    print(s)


def trk(tag, k=None, fa=None, fb=None):
    rows = []
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        if k is not None and int(r['track']) != k:
            continue
        f = int(r['frame'])
        if (fa is not None and f < fa) or (fb is not None and f > fb):
            continue
        rows.append((f, int(r['worldx']), int(r['pivy'])))
    return rows


# ---------------------------------------------------------------- xương
BONES = [('bw_wea', 1, 119, 129, 'S1 WEA 0:35.7'), ('bw_web1', 0, 352, 193, 'S2 WEB 0:50.8'), ('bw_web2', 0, 625, 193, 'S3a WEB 1:11.5'),
         ('bw_web2', 1, 625, 193, 'S3b WEB 1:11.8'), ('bw_web2', 2, 625, 193, 'S3c WEB 1:12.8'), ('bw_wa', 2, 522, 161, 'S4 WalkwayA 1:41.8')]


def bones():
    out('== XƯƠNG (x tuyến tính; y bậc 2 tự do và g 0.125 cố định; sinh = lúc x xương = X Skeleton; đỉnh so pivot VẼ Skeleton)')
    for tag, k, sx, sy, d in BONES:
        t = trk(tag, k)
        f = np.array([p[0] for p in t], float); x = np.array([p[1] for p in t], float); y = np.array([p[2] for p in t], float)
        tk = (f - f[0]) * TPF
        a, b = np.polyfit(tk, x, 1)
        c2, c1, c0 = np.polyfit(tk, y, 2)
        rfree = np.sqrt(((y - np.polyval([c2, c1, c0], tk)) ** 2).mean())
        g = 0.125
        A = np.vstack([tk, np.ones_like(tk)]).T
        (vy, y0), _, _, _ = np.linalg.lstsq(A, y - 0.5 * g * tk * tk, rcond=None)
        rms = np.sqrt(((y - (0.5 * g * tk * tk + vy * tk + y0)) ** 2).mean())
        ts = (sx - b) / a
        ys = 0.5 * g * ts * ts + vy * ts + y0
        apex = y0 - vy * vy / (2 * g)
        out('  %-20s f%d–%d n%-3d vx %+.3f | g tự do %.4f (rms %.2f) | g 0.125: sinh f%.1f y %.1f (trên pivot %.1f) vy0 %.3f, ĐỈNH trên pivot %.1f (rms %.2f)' % (
            d, f[0], f[-1], len(t), a, 2 * c2, rfree, f[0] + ts / TPF, ys, sy - ys, vy + g * ts, sy - apex, rms))
    t = trk('bw_mm')
    f = np.array([p[0] for p in t], float); x = np.array([p[1] for p in t], float); y = np.array([p[2] for p in t], float)
    tk = (f - f[0]) * TPF
    A = np.vstack([tk, np.ones_like(tk)]).T
    (vy, y0), _, _, _ = np.linalg.lstsq(A, y - 0.0625 * tk * tk, rcond=None)
    a, b = np.polyfit(tk, x, 1)
    out('  %-20s f%d–%d n%-3d vx %+.3f | g 0.125: ĐỈNH trên pivot %.1f (rms %.2f) — 2 vết (lên f5127–5134 + xuống f5151–5168) cùng 1 xương' % (
        'S5 MummyMan 2:51.0', f[0], f[-1], len(t), a, 129 - (y0 - vy * vy / 0.25), np.sqrt(((y - (0.0625 * tk * tk + vy * tk + y0)) ** 2).mean())))
    out('  ⇒ 2 độ cao: thấp 61.5–62.6 (S1, S3a, S3c, S4), cao 90.8–93.0 (S2, S3b, S5); Unity: sinh Y − 13 (pivot vẽ + 1 − 13), vy0 −3.5 / −4.5 ⇒ 61.25 / 92.75')


# ---------------------------------------------------------------- nhảy Skeleton
JUMPS = [('J2 WEB 0:50.9 (qua hố 320–351)', 'sw_web1', 0, 1524, 1549, 193), ('J6 MummyMan 2:53.0 (khe 832–863)', 'sw_mm', 1, 5188, 5214, 129),
         ('J3 WEB 1:13.2 (rơi hố 544–607)', 'sw_web2', 0, 2193, 2216, 193), ('J5 MummyMan 2:51.2 (khe 768–799)', 'sw_mm', 0, 5134, 5148, 129)]


def h_model(t, v0, g):
    out_ = []
    up = int(round(v0 / g))
    for tt in t:
        n = int(np.floor(tt)); fr = tt - n

        def H(n):
            if n <= 0:
                return 0.0
            if n <= up:
                return v0 * n - g * n * (n + 1) / 2
            top = v0 * up - g * up * (up + 1) / 2
            m = n - up
            return top - g * m * (m + 1) / 2
        out_.append(H(n) * (1 - fr) + H(n + 1) * fr)
    return np.array(out_)


def jumps():
    out('== NHẢY SKELETON (độ cao theo tick, mô hình "vy += g rồi dời", giờ bật tự do từng cú)')
    res = []
    for v0, g in itertools.product([2.25, 2.375, 2.5, 2.625, 2.75], [0.09375, 0.1015625, 0.109375, 0.1171875, 0.125, 0.1328125]):
        tot = n = 0
        for _, tag, k, fa, fb, y0 in JUMPS:
            t = trk(tag, k, fa, fb)
            f = np.array([p[0] for p in t], float); h = y0 - np.array([p[2] for p in t], float)
            bb = None
            for to in np.arange(f[0] - 3, f[0] + 6, 0.1):
                tk = (f - to) * TPF
                m = h_model(np.maximum(tk, 0), v0, g)
                mask = (tk >= 0) & ((h > 0) | (tk < 3))
                e = ((m - h)[mask] ** 2).sum()
                if bb is None or e < bb[0]:
                    bb = (e, mask.sum())
            tot += bb[0]; n += bb[1]
        res.append((np.sqrt(tot / n), v0, g))
    res.sort()
    for r in res[:5]:
        out('  rms %.2f px  vy0 −%.3f  g %.4f (đỉnh %.2f px, %d tick)' % (r[0], r[1], r[2], r[1] * r[1] / (2 * r[2]) - r[1] / 2, int(round(2 * r[1] / r[2]))))
    for name, tag, k, fa, fb, y0 in JUMPS:
        t = trk(tag, k, fa, fb)
        f = np.array([p[0] for p in t], float); x = np.array([p[1] for p in t], float)
        a, _ = np.polyfit(f * TPF, x, 1)
        out('  %-34s vx hồi quy %+.3f px/tick, dời %d px' % (name, a, int(x[-1] - x[0])))


# ---------------------------------------------------------------- đi Skeleton
LEGS = [('sw_wea', 0, 1034, 1046, 97, 119), ('sw_wea', 0, 1045, 1056, 119, 142), ('sw_wea', 0, 1055, 1066, 142, 119),
        ('sw_web2', 0, 2123, 2134, 671, 648), ('sw_web2', 0, 2133, 2143, 648, 625), ('sw_web2', 0, 2159, 2171, 625, 648),
        ('sw_web2', 0, 2170, 2182, 648, 625), ('sw_wa', 0, 2986, 2996, 671, 648), ('sw_wa', 1, 3029, 3040, 567, 545),
        ('sw_wa', 1, 3039, 3050, 545, 522), ('sw_wa', 1, 3058, 3069, 522, 545)]


def legs():
    out('== CHẶNG ĐI SKELETON (mô hình dốc: đứng xa → đi đều v → đứng xb; đồng hồ frame)')
    vs = []
    for tag, k, fa, fb, xa, xb in LEGS:
        t = trk(tag, k, fa, fb)
        f = np.array([p[0] for p in t], float) * TPF; x = np.array([p[1] for p in t], float)
        best = None
        for v in np.arange(1.2, 1.71, 0.01):
            Ld = abs(xb - xa) / v
            for ts in np.arange(f[0] - 4, f[-1], 0.1):
                m = xa + np.sign(xb - xa) * v * np.clip(f - ts, 0, Ld)
                e = ((m - x) ** 2).sum()
                if best is None or e < best[0]:
                    best = (e, v, Ld)
        vs.append(best[1])
        out('  %-8s f%d–%d %d→%d (%d px): v %.2f, chặng %.1f tick (rms %.2f)' % (tag, fa, fb, xa, xb, abs(xb - xa), best[1], best[2], np.sqrt(best[0] / len(x))))
    out('  TB v %.3f (trung vị %.3f) theo đồng hồ frame; đồng hồ Simon (Simon đi 1 px/tick cùng lúc): f2125–2131 Simon 14 px / Skeleton 20 px = 1.43,'
        ' f2174–2178 9 / 13 = 1.44, f5132–5136 7 / 10 = 1.43 ⇒ chọn 1.4375 (23 px / 16 tick)' % (np.mean(vs), np.median(vs)))


# ---------------------------------------------------------------- Fleaman
FLEA = [('F0 bục (433,128) f520–543', 0, 520, 543, 433, 161), ('F1 bục (305,128) f650–673', 1, 650, 673, 305, 161),
        ('F2 bục (177,96) f889–916', 2, 889, 916, 177, 129)]


def sim(n, vx, vy0, g):
    xs = [0.0]; ys = [0.0]; vy = vy0; x = y = 0.0
    for _ in range(n):
        vy += g; x += vx; y += vy; xs.append(x); ys.append(y)
    return np.array(xs), np.array(ys)


def flea():
    out('== FLEAMAN — cú nhảy đầu (từ bục xuống sàn; lưới vx/vy0/g, giờ bật tự do)')
    res = []
    for vx, vy0, g in itertools.product([2.0, 2.125, 2.25, 2.375, 2.5], [-0.5, -0.75, -1.0, -1.25, -1.5, -2.0], [0.09375, 0.109375, 0.125, 0.140625, 0.15625, 0.1875]):
        xs, ys = sim(80, vx, vy0, g)
        tot = n = 0; tos = []
        for _, k, fa, fb, x0, y0 in FLEA:
            t = trk('fl_wea', k, fa, fb)
            f = np.array([p[0] for p in t], float); x = np.array([p[1] for p in t], float); y = np.array([p[2] for p in t], float)
            bb = None
            for to in np.arange(f[0] - 2, f[-1], 0.1):
                tk = (f - to) * TPF
                m = tk >= 0
                if m.sum() < 5:
                    continue
                mx = x0 + np.interp(tk[m], np.arange(81), xs); my = y0 + np.interp(tk[m], np.arange(81), ys)
                e = ((mx - x[m]) ** 2 + (my - y[m]) ** 2).sum() + ((x[~m] - x0) ** 2 + (y[~m] - y0) ** 2).sum()
                if bb is None or e < bb[0]:
                    bb = (e, to)
            tot += bb[0]; n += len(t); tos.append(bb[1])
        res.append((np.sqrt(tot / n / 2), vx, vy0, g, tos))
    res.sort(key=lambda r: r[0])
    for r in res[:5]:
        out('  rms %.2f px  vx %.3f vy0 %.3f g %.4f  bật f%s' % (r[0], r[1], r[2], r[3], ' / '.join('%.1f' % v for v in r[4])))
    out('  khung chờ 0↔1: đổi mỗi 4.4–4.5 frame (≈ 9 tick, 17 lần); trước khi bật giữ fleaman_0 12.4 / 14.2 / 12.4 tick (f518→524.4, f647→654.1, f889→895.2)')
    out('  vào tầm |dx| < 128 (vết Simon từng tick ref/data/enemy7/puppet_fl_wea*.csv): tick 14 / 33 / 32 ⇒ bật 88.9 / 108.4 / 180.7 ⇒ đếm 61.9 / 62.4 / 135.7 tick (+ ngồi thấp 13)')
    out('  Simon đứng yên ở |dx| 100 / 100 / 120 khi bật; f838 |dx| 95 không bật ⇒ không phải ngưỡng tức thì')


def dump_tracks():
    p = os.path.join(OUT, 'tracks.csv')
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo, lineterminator='\n')
        w.writerow(['tag', 'track', 'frame', 'sprite', 'flip', 'worldx', 'pivy', 'err'])
        for tag in ('fl_wea', 'sw_wea', 'sw_web1', 'sw_web2', 'sw_wa', 'sw_mm', 'bw_wea', 'bw_web1', 'bw_web2', 'bw_wa', 'bw_mm'):
            for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
                w.writerow([tag, r['track'], r['frame'], r['sprite'], r['flip'], r['worldx'], r['pivy'], r['err']])
    out('ghi ' + p)


def main():
    os.makedirs(OUT, exist_ok=True)
    bones(); out(); jumps(); out(); legs(); out(); flea(); out(); dump_tracks()
    open(os.path.join(OUT, 'models.txt'), 'w', encoding='utf-8').write('\n'.join(LINES) + '\n')


if __name__ == '__main__':
    main()
