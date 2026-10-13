"""CVGP 08 — đo nhảy: (1) cú nhảy có đồng hồ x (camera khoá/đứng) so bảng độ cao; (2) ghép 14 cú nhảy theo thời gian video
để dựng bảng độ cao + chọn độ lệch vẽ khung co chân. Chạy: python -P -E Tools/cvgp/simon_jump.py"""
import os
# chạy từ gốc repo; dữ liệu khớp mẫu ở ClaudeNotes/cvgp/_cache/08/fit_*.csv (sinh bằng Tools/cvgp/simon_fit.py — lệnh ở CVGP_MEMORY §2 "Simon")
os.chdir(os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'ClaudeNotes', 'cvgp', '_cache', '08'))

# ---- (2) ghép theo thời gian video
# Ghép nhiều cú nhảy để dựng bảng độ cao theo tick (đồng hồ = thời gian video, ±1 tick).
import csv, sys
import numpy as np
TPF = 60.0988 / 30.0
def load(fn):
    return {int(r['frame']): r for r in csv.DictReader(open(fn)) if r['px']}
J = load('fit_jumpin.csv'); M = load('fit_jumpmove.csv')
# (bang, frame dau tien tren khong, mat san, frame cuoi cung xet)
JUMPS = [(J,716,208,740),(J,815,208,836),(J,968,208,992),(J,1126,208,1147),(J,1691,112,1714),(J,3623,128,3645),
         (J,4106,128,4126),(J,4685,128,4706),(J,5309,144,5332),(J,1484,144,1508),
         (M,901,208,924),(M,3878,128,3900),(M,4021,128,4042),(M,5209,112,5226)]
POSE = {'jumpduck_0': 'tuck'}
def samples(dtuck):
    out = []
    for tab, fa, g, fe in JUMPS:
        prev = None; pts = []
        for f in range(fa - 4, fe + 1):
            r = tab.get(f)
            if r is None: continue
            key = (r['sprite'], r['Y'], r['px'], r['cx'])
            if key == prev: continue
            prev = key
            if float(r['err']) > 0.06: continue
            y = int(r['Y']) - (dtuck if r['sprite'] == 'jumpduck_0' else 0)
            pts.append((f, g - y, r['sprite']))
        out.append((fa, pts))
    return out
def build(S, f0s, T=48):
    bins = [[] for _ in range(T + 1)]
    for (fa, pts), f0 in zip(S, f0s):
        for f, h, sp in pts:
            t = (f - f0) * TPF
            if t < -3 or t > T: continue
            i = int(round(t))
            if 0 <= i <= T: bins[i].append(h)
    H = np.array([np.median(b) if b else np.nan for b in bins])
    # tick truoc khi bat dau = 0
    return H, bins
def fillnan(H):
    H = H.copy(); idx = np.arange(len(H)); ok = ~np.isnan(H)
    H[~ok] = np.interp(idx[~ok], idx[ok], H[ok])
    return H
def interp(H, t):
    if t <= 0: return 0.0
    i = int(np.floor(t)); a = t - i
    if i + 1 >= len(H): return 0.0
    h0, h1 = H[i], H[i + 1]
    if np.isnan(h0) or np.isnan(h1): return np.nan
    return h0 * (1 - a) + h1 * a
def fit(dtuck, iters=12, verbose=False):
    S = samples(dtuck)
    f0s = [fa - 1.0 + 0.137 * (k % 4) for k, (fa, _) in enumerate(S)]
    for it in range(iters):
        H, bins = build(S, f0s)
        H = fillnan(H)
        newf = []
        tot = 0
        for (fa, pts), f0 in zip(S, f0s):
            best = None
            for d in np.arange(-1.5, 1.51, 0.05):
                c = f0 + d
                e = 0
                for f, h, sp in pts:
                    m = interp(H, (f - c) * TPF)
                    if np.isnan(m): m = 0
                    e += abs(h - m)
                if best is None or e < best[0]: best = (e, c)
            newf.append(best[1]); tot += best[0]
        # neo: trung binh f0 - fa giu nguyen (bo bac tu do tinh tien chung)
        sh = np.mean([n - (fa - 1.0) for n, (fa, _) in zip(newf, S)])
        f0s = [n - sh for n in newf]
    H, bins = build(S, f0s)
    return tot, H, bins, f0s, S
def main_fit():
    for dt in [0, 2, 3, 4, 5, 6, 8]:
        tot, H, bins, f0s, S = fit(dt, iters=6)
        print('dtuck', dt, 'tong |du|', round(tot, 1))
    dt = int(sys.argv[1]) if len(sys.argv) > 1 else 4
    tot, H, bins, f0s, S = fit(dt)
    print('dtuck', dt, 'tong', round(tot, 1))
    for i, (h, b) in enumerate(zip(H, bins)):
        print('t%2d h=%5.1f n=%2d  %s' % (i, h if not np.isnan(h) else -1, len(b), ' '.join(str(int(x)) for x in sorted(b))))


# ---- (1) đồng hồ x
# Nhảy có đồng hồ x (camera khoá hoặc đứng ⇒ x Simon = đồng hồ 1 px/tick): t = |x − x_cất_cánh|, h = mặt sàn − Y logic
RISE = [0, 5, 10, 13, 16, 19, 22, 25, 27, 29, 31, 32, 33, 34, 35, 35, 36]
def load(fn):
    return {int(r['frame']): r for r in csv.DictReader(open(fn)) if r['px']}
L = load('fit_ledge.csv'); M = load('fit_jumpmove.csv')
CLOCKED = [('3712 len buc +32', L, 3709, 3727, 128, 1), ('6095 len khoi +32', L, 6092, 6110, 208, -1),
         ('6111 nhay tu khoi xuong san', L, 6108, 6134, 176, -1), ('5965 boss len khoi', M, 5962, 5980, 208, 1),
         ('4021 (cx256 tu 4032)', M, 4018, 4042, 128, 1), ('5209 nhay xuong bac', M, 5206, 5232, 112, 1)]
DT = 4
rows_all = []
for name, tab, fa, fe, g, d in CLOCKED:
    pts = []
    prev = None
    for f in range(fa, fe + 1):
        r = tab.get(f)
        if r is None: continue
        X = int(r['cx'] or 0) + int(r['px'])
        key = (X, r['Y'], r['sprite'])
        if key == prev: continue
        prev = key
        y = int(r['Y']) - (DT if r['sprite'] == 'jumpduck_0' else 0)
        pts.append((f, X, g - y, r['sprite'], float(r['err'])))
    # chọn x0: khớp pha lên (h>0, t<=16)
    best = None
    for x0 in np.arange(pts[0][1] - 6 * d, pts[0][1] + 12 * d, 0.5 * d):
        e = 0; n = 0
        for f, X, h, sp, er in pts:
            t = (X - x0) * d
            if 0 < t <= 16 and h > 0:
                i = int(np.floor(t)); a = t - i
                m = RISE[i] * (1 - a) + RISE[min(i + 1, 16)] * a
                e += abs(h - m); n += 1
        if n >= 3 and (best is None or e / n < best[0]):
            best = (e / n, x0)
    x0 = best[1]
    print('== %s: x0 %.1f (du TB pha len %.2f)' % (name, x0, best[0]))
    print('   ' + '  '.join('t%.0f:h%d%s%s' % ((X - x0) * d, h, 'T' if sp == 'jumpduck_0' else ('W' if sp.startswith('use') else 'S'), '?' if er > 0.06 else '') for f, X, h, sp, er in pts))

main_fit()
