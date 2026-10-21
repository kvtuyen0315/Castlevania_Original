# kb4_holy_fit.py (CVM3 04) - khop mo hinh lo nuoc thanh cho tung lan nem (all*.csv cua kb4_holy_all.py):
#   x_n = X0 + huong*1.5*n (hien = floor), vy += g roi y += vy (vy0 -1.5, g 33/256 - tim o Map_3 tran Mummy, 46 diem du 0),
#   tim X0 (phan nguyen + le), y0 cho moi lan; in du (px), n luc thay lo cuoi + vi tri lua.
# Chay: python -P -E Tools/cvm3/kb4_holy_fit.py <all.csv> [vy0=-1.5] [g=0.12890625]
import sys, csv, math
from collections import defaultdict

p = sys.argv[1]
VY0 = -1.5; G = 33 / 256.0; VX = 1.5
for a in sys.argv[2:]:
    k, v = a.split('=')
    if k == 'vy0': VY0 = float(v)
    if k == 'g': G = float(v)


def ypos(y0, n):
    # vy += g; y += vy  (n tick)
    return y0 + VY0 * n + G * n * (n + 1) / 2.0


rows = list(csv.DictReader(open(p)))
by = defaultdict(list)
for r in rows:
    by[(r['video'], r['throw'])].append(r)
for (video, thr), rs in by.items():
    face = int(rs[0]['face']); spx = int(rs[0]['spx']); sY = int(rs[0]['sY'])
    pts = [(int(r['rel']), int(r['bX']), int(r['bY'])) for r in rs if float(r['berr']) < 0.11 and r['dup'] == '0']
    fire = [(int(r['rel']), r['fk'], int(r['fX']), int(r['fY'])) for r in rs if float(r['ferr']) < 0.11]
    if len(pts) < 4:
        print(video, thr, 'it diem lo', len(pts)); continue
    best = None
    for x0q in range(-40 * 8, 40 * 8):     # X0 = spx + x0q/8
        X0 = spx + x0q / 8.0
        ns = []
        ok = True
        for rel, X, Y in pts:
            cand = [n for n in range(0, 90) if math.floor(X0 + face * VX * n + 1e-9) == X]
            if not cand:
                ok = False; break
            # chon n gan 2*rel - 16 nhat
            ns.append(min(cand, key=lambda n: abs(n - (2 * rel - 16))))
        if not ok:
            continue
        for y0q in range(-60 * 8, 30 * 8):
            y0 = sY + y0q / 8.0
            res = [math.floor(ypos(y0, n) + 1e-9) - Y for n, (rel, X, Y) in zip(ns, pts)]
            sse = sum(d * d for d in res)
            if best is None or sse < best[0]:
                best = (sse, X0, y0, ns, res)
    if best is None:
        print(video, thr, 'khong khop x'); continue
    sse, X0, y0, ns, res = best
    sp = [2 * rel - n for n, (rel, X, Y) in zip(ns, pts)]
    last = max(zip(ns, pts), key=lambda t: t[0])
    print('%s %s Simon (%d,%d) huong %+d %s | X0 %.3f (Simon%+.3f) y0 %.3f (chan%+.3f) | du max %d sse %d n=%d..%d | sinh tick~%s | lo cuoi n%d (%d,%d) | lua %s'
          % (video, thr, spx, sY, face, rs[0]['spose'], X0, X0 - spx, y0, y0 - sY, max(abs(d) for d in res), sse, min(ns), max(ns),
             sorted(sp)[len(sp) // 2], last[0], last[1][1], last[1][2], fire[:1] + fire[-1:] if fire else '-'))
