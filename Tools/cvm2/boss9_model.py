"""CVM2 09 — khớp mô hình bay của Queen Medusa + rắn + nháy màu trên các đoạn SẠCH của 2 video (det_*.csv của boss9_fit.py).

Chạy (từ gốc repo): python -P -E Tools/cvm2/boss9_model.py  → in bảng + ghi ClaudeNotes/cvm2/ref/data/boss9/model_fit.txt
Đoạn bay (frame video, Medusa không bị khựng/đóng băng; mốc chia theo ref/data/boss9/medusa_*.csv):
  A  = Boss_Map_2 f68–f99   (thức → dừng 1)      W = Map_2 f6826–f6837 (thức → đồng hồ)
  D  = Boss_Map_2 f322–f330 (dừng 2 → dừng 3)    E = Boss_Map_2 f362–f387 (dừng 3 → lửa nước thánh)
  C  = Boss_Map_2 f243–f292 (sau khi va Simon f196 + 2 lần khựng — đoạn "nảy", mô hình riêng)
Mô hình nhào: x = x0 + dir·v·τ ; y = y0 + P·(1 − cos(2πτ/T))/2 (đi XUỐNG trước) — τ tick từ đầu đoạn (lệch gốc τ0 tự do ±3 tick).
tick = frame × 2.0033 (CVM2 02).
"""
import csv, math, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C9 = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '09')
OUTD = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'boss9')
TPF = 2.0033


def best(tag, emax=0.15, kinds=None):
    d = {}
    for r in csv.DictReader(open(os.path.join(C9, 'det_%s.csv' % tag))):
        f = int(r['frame'])
        e = float(r['err'])
        if e >= emax or (kinds and not any(k in r['sprite'] for k in kinds)):
            continue
        if f not in d or e < d[f][3]:
            d[f] = (int(r['pivx']), int(r['pivy']), r['sprite'], e)
    return d


def seg(d, f0, f1):
    return [(f, d[f][0], d[f][1]) for f in sorted(d) if f0 <= f <= f1]


def model(tau, x0, y0, dirx, v, P, T):
    return x0 + dirx * v * tau, y0 + P * (1 - np.cos(2 * np.pi * tau / T)) / 2


def fit_segments(segs, v, P, T):
    """mỗi đoạn: x0,y0 = điểm đầu (frame đầu), τ0 tối ưu trong [-3,3] tick; trả rms tổng (px) và rms từng đoạn"""
    tot, n, per = 0.0, 0, []
    for name, pts, dirx in segs:
        f0 = pts[0][0]
        bestr = None
        for t0 in np.arange(-3, 3.01, 0.25):
            tau = np.array([(f - f0) * TPF + t0 for f, x, y in pts])
            xs = np.array([x for f, x, y in pts], float)
            ys = np.array([y for f, x, y in pts], float)
            # x0, y0 = giá trị mô hình tại τ = 0 khớp điểm đầu (bình phương tối thiểu theo hằng)
            mx, my = model(tau, 0, 0, dirx, v, P, T)
            x0 = np.mean(xs - mx)
            y0 = np.mean(ys - my) if True else ys[0]
            err = np.concatenate([xs - (mx + x0), ys - (my + y0)])
            r = float(np.sqrt(np.mean(err ** 2)))
            if bestr is None or r < bestr[0]:
                bestr = (r, t0, x0, y0, float(np.sqrt(np.mean((xs - mx - x0) ** 2))), float(np.sqrt(np.mean((ys - my - y0) ** 2))))
        per.append((name, len(pts)) + bestr)
        tot += bestr[0] ** 2 * len(pts) * 2
        n += len(pts) * 2
    return math.sqrt(tot / n), per


def linfit(pts, idx):
    t = np.array([p[0] * TPF for p in pts])
    v = np.array([p[idx] for p in pts], float)
    A = np.vstack([t, np.ones_like(t)]).T
    k, b = np.linalg.lstsq(A, v, rcond=None)[0]
    return k, b, float(np.sqrt(np.mean((A @ [k, b] - v) ** 2)))


def runs(d, f0, f1):
    """độ dài chuỗi khung head/stone liên tiếp (tick) — bỏ chuỗi chạm mép đoạn"""
    fr = [f for f in sorted(d) if f0 <= f <= f1]
    out, cur, start = [], None, None
    prev = None
    for f in fr:
        k = 'st' if 'stone' in d[f][2] else 'he'
        if k != cur:
            if cur is not None:
                out.append((cur, start, prev))
            cur, start = k, f
        prev = f
    out.append((cur, start, prev))
    # mốc đổi = trung điểm giữa frame cuối khung cũ và frame đầu khung mới
    ch = []
    for i in range(1, len(out)):
        ch.append(((out[i - 1][2] + out[i][1]) / 2.0, out[i][0]))
    return out, ch


def main():
    os.makedirs(OUTD, exist_ok=True)
    lines = []
    P = lambda *a: lines.append(' '.join(str(x) for x in a))
    b2 = best('md_b2')
    m2 = best('md_m2')
    A = seg(b2, 68, 99)
    W = seg(m2, 6826, 6837)
    D = seg(b2, 322, 330)
    E = seg(b2, 362, 387)
    C = seg(b2, 243, 292)
    # tốc độ ngang (hồi quy x theo tick)
    P('== Toc do ngang (hoi quy, px/tick) ==')
    for name, pts in (('A', A), ('W', W), ('D', D), ('E', E), ('C', C)):
        k, b, r = linfit(pts, 1)
        P(name, 'n', len(pts), 'vx %.4f' % k, 'rms %.2f' % r)
    # quét mô hình nhào
    segs = [('A', A, -1), ('W', W, -1), ('D', D, -1), ('E', E, -1)]
    P('== Quet mo hinh nhao y = y0 + P(1-cos(2pi t/T))/2 ==')
    res = []
    for v in (0.8125, 0.84375, 0.875, 0.90625, 0.9375, 1.0):
        for Pp in (11, 12, 13, 14, 15):
            for T in (56, 60, 64, 68, 72, 80):
                r, per = fit_segments(segs, v, Pp, T)
                res.append((r, v, Pp, T, per))
    res.sort(key=lambda z: z[0])
    for r, v, Pp, T, per in res[:8]:
        P('rms %.3f v %.5f P %d T %d' % (r, v, Pp, T), ' | '.join('%s n%d rms %.2f (x %.2f y %.2f) t0 %.2f' % (p[0], p[1], p[2], p[6], p[7], p[3]) for p in per))
    for v, Pp, T in ((0.875, 13, 64),):
        r, per = fit_segments(segs, v, Pp, T)
        P('CHON v %.3f P %d T %d: rms %.3f' % (v, Pp, T, r))
        for p in per:
            P('   ', p[0], 'n', p[1], 'rms %.2f' % p[2], 'x %.2f y %.2f' % (p[6], p[7]), 't0 %.2f' % p[3], 'x0 %.1f y0 %.1f' % (p[4], p[5]))
    # đoạn nảy C (sau va chạm): y = y0 + P(1 - cos)/2 nhưng ĐI LÊN trước, quét P,T
    P('== Doan nay C (f243-f292, dang di len), mo hinh y = yb - P(1-cos(2pi(t+ph)/T))/2 ==')
    bestc = None
    for Pp in (14, 16, 18, 20):
        for T in (80, 88, 96, 104, 112):
            for ph in range(0, 112, 2):
                tau = np.array([(f - 243) * TPF + ph for f, x, y in C])
                ys = np.array([y for f, x, y in C], float)
                my = -Pp * (1 - np.cos(2 * np.pi * tau / T)) / 2
                y0 = np.mean(ys - my)
                r = float(np.sqrt(np.mean((ys - my - y0) ** 2)))
                if bestc is None or r < bestc[0]:
                    bestc = (r, Pp, T, ph, y0)
    P('C tot nhat: rms y %.2f P %d T %d pha %d yb %.1f' % bestc)
    # nháy màu
    P('== Nhay mau head/stone (doan khong khung/khong dong bang) ==')
    allch = []
    for name, d, f0, f1 in (('b2 A+dung1', b2, 68, 129), ('b2 C', b2, 243, 330), ('b2 dung3+E', b2, 330, 387), ('m2 W', m2, 6826, 6837)):
        out, ch = runs(d, f0, f1)
        P(name, ' '.join('%s%d-%d' % (k, a, b) for k, a, b in out))
        ts = [c[0] for c in ch]
        if len(ts) > 2:
            dt = np.diff(ts) * TPF
            P('   khoang doi (tick):', ' '.join('%.1f' % x for x in dt), 'TB %.2f' % np.mean(dt))
            allch.append(ts)
    # rắn: rơi + trườn + anim
    P('== Ran ==')
    sn = best('sn_b2', 0.13)
    tracks = {'s1': (69, 116), 's2': (132, 158), 's3': (324, 387), 's4': (388, 418)}
    for name, (f0, f1) in tracks.items():
        pts = [(f, sn[f][0], sn[f][1], sn[f][2]) for f in sorted(sn) if f0 <= f <= f1]
        if name == 's3':
            pts = [p for p in pts if p[1] > 0]
        fall = [p for p in pts if p[2] < 175]
        crawl = [p for p in pts if p[2] >= 175]
        if len(crawl) > 3:
            k, b, r = linfit([(p[0], p[1]) for p in crawl], 1)
            P(name, 'truon n', len(crawl), 'vx %.4f px/tick rms %.2f' % (k, r), 'x %d..%d f%d..%d' % (crawl[0][1], crawl[-1][1], crawl[0][0], crawl[-1][0]))
        if len(fall) >= 3:
            t = np.array([(p[0] - fall[0][0]) * TPF for p in fall]); y = np.array([p[2] for p in fall], float)
            A2 = np.vstack([t * t / 2, t, np.ones_like(t)]).T
            g, v0, y0 = np.linalg.lstsq(A2, y, rcond=None)[0]
            P(name, 'roi n', len(fall), 'g %.3f v0 %.3f y0 %.1f' % (g, v0, y0), 'cham san f', crawl[0][0] if crawl else '-')
        # anim snake_0/1
        out, cur, st = [], None, None
        for p in crawl:
            k = p[3][-1]
            if k != cur:
                if cur is not None:
                    out.append((cur, st, prevf))
                cur, st = k, p[0]
            prevf = p[0]
        if crawl:
            out.append((cur, st, prevf))
        mids = [(out[i - 1][2] + out[i][1]) / 2.0 for i in range(1, len(out))]
        if len(mids) > 2:
            P('   anim', ' '.join('%s%d-%d' % o for o in out), '| doi moi (tick):', ' '.join('%.1f' % x for x in np.diff(mids) * TPF), 'TB %.2f' % (np.mean(np.diff(mids)) * TPF))
    open(os.path.join(OUTD, 'model_fit.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    main()
