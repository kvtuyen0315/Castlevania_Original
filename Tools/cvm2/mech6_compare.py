# mech6_compare.py (CVM2 06) - so co che Unity (Map2MechProbe) voi video Map_2:
#   (1) quy dao be: video platform_<ten>.csv (mech6_platform.py) vs mo hinh Unity x = floor(tam + bien*cos(2 pi t/T)) (probe da kiem
#       Unity == cong thuc <= 0.004 px moi tick) voi t = (frame - frame nap section)*2.0033 (tau = 0) va tau khop tot nhat
#   (2) Simon tren be: do lech (x Simon - x be) quanh trung binh khi Simon dung yen tren be — video vs vet Unity (traces/plat_ride_*.csv)
#   (3) pha bay ep: khung tung frame video (crusher_low/up.csv) vs mo hinh Unity (startTick tai f4479) — ti le khop + lech tick moc doi khung
# Chay (tu goc repo): python -P -E Tools/cvm2/mech6_compare.py  -> in bang + ghi ClaudeNotes/cvm2/_cache/06/compare.txt
import os, csv, math
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06')
SIMON = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_simon.csv')
TPF = 2.0033
out = []
def P(*a):
    s = ' '.join(str(x) for x in a); print(s); out.append(s)

def load_plat(names):
    f, x = [], []
    for n in names:
        for r in csv.DictReader(open(os.path.join(D, 'platform_%s.csv' % n))):
            if int(r['dup']) or float(r['margin']) < 0.6 or int(r['vis']) < 32: continue
            f.append(int(r['frame'])); x.append(int(r['x']))
    return np.array(f, float), np.array(x, float)

# (1) be: tam pivot, bien, chu ky (ConfigActorParam), frame nap section (camera CVM2 02: frame game dau tien cua section)
PLATS = [('TEB', ['teb'], 64, 16, 201.4, 866, 4), ('HallB_1', ['hallb1', 'hallb1r'], 304, 32, 402.4, 2801, 6), ('HallB_2', ['hallb2', 'hallb2r'], 432, 24, 201.8, 2801, 6)]
def model_left(t, c, a, T):
    return np.floor(c + a * np.cos(2 * np.pi * t / T)) - 16
P('== (1) quy dao be: video vs mo hinh Unity (be sinh o mep phai luc nap section)')
P('be | diem | rms tau=0 | rms Unity (tau=startTick) | |lech| max Unity | tau tot nhat (tick) | rms tau tot | ghi chu')
for name, files, c, a, T, f0, st in PLATS:
    f, x = load_plat(files)
    t = (f - f0) * TPF
    r0 = x - model_left(t, c, a, T)
    best = None
    for tau in np.arange(-20, 20.01, 0.25):
        r = x - model_left(t + tau, c, a, T)
        e = math.sqrt((r * r).mean())
        if best is None or e < best[0]: best = (e, tau)
    ru = x - model_left(t + st, c, a, T)
    P('%s | %d | %.2f px | %.2f px (startTick %d) | %d | %+.2f | %.2f px | t0 = f%d' % (name, len(x), math.sqrt((r0 * r0).mean()), math.sqrt((ru * ru).mean()), st, int(np.abs(ru).max()), best[1], best[0], f0))

# (2) Simon tren be (dung yen, chan y 80): video
P('')
P('== (2) Simon dung yen tren be: x Simon - x be (mep trai)')
plat = {}
for n in ('teb', 'hallb1r', 'hallb2r'):
    for r in csv.DictReader(open(os.path.join(D, 'platform_%s.csv' % n))):
        if int(r['vis']) == 32 and float(r['margin']) > 0.5: plat.setdefault(int(r['frame']), {})[n] = int(r['x'])
WIN = [('TEB f1466-1484', 'teb', 1466, 1484), ('HallB_2 f3953-3970', 'hallb2r', 3953, 3970), ('HallB_1 f4009-4032', 'hallb1r', 4009, 4032)]
sim = {int(r['frame']): r for r in csv.DictReader(open(SIMON)) if r['found'] == '1'}
for lab, n, a, b in WIN:
    d = []
    for fr in range(a, b + 1):
        s = sim.get(fr)
        if s is None or int(s['dup']) or n not in plat.get(fr, {}) or int(s['y_sec']) != 80: continue
        d.append(int(s['world_x']) - plat[fr][n])
    d = np.array(d, float)
    P('video %s: %d frame, lech TB %.2f, rms quanh TB %.2f px, khoang %d..%d' % (lab, len(d), d.mean(), d.std(), d.min(), d.max()))
for sc, col in (('plat_ride_teb', 'p0x'), ('plat_ride_hallb', None)):
    p = os.path.join(D, 'traces', sc + '.csv')
    if not os.path.exists(p): continue
    rows = list(csv.DictReader(open(p)))
    # be Simon dung: TEB = p0; Hall B = be co mep trai gan Simon nhat luc dau
    k = 'p0x'
    if sc == 'plat_ride_hallb':
        s0 = int(rows[0]['sx']); k = 'p0x' if abs(int(rows[0]['p0x']) + 16 - s0) < abs(int(rows[0]['p1x']) + 16 - s0) else 'p1x'
    d = np.array([int(r['sx']) - int(r[k]) for r in rows], float)
    P('Unity %s: %d tick, lech TB %.2f, rms quanh TB %.2f px, khoang %d..%d (fixed Simon - be khong doi — probe)' % (sc, len(d), d.mean(), d.std(), d.min(), d.max()))

# (3) bay ep
P('')
P('== (3) bay ep: khung video vs mo hinh Unity (startTick 55/7/7 tai f4479, chu ky 112/80/128, kmax 6/4/7)')
KMAX = {1: 6, 2: 4, 3: 7}; U0 = {1: 55, 2: 7, 3: 7}
def cyc(k):
    b1 = 16 * (4 * k // 16 + 1); return b1, b1 + 16 * (k - 1)
def fr_at(i, u):
    k = KMAX[i]; b1, L = cyc(k); u %= L
    if u < 4 * k: return 1 + u // 4
    if u < b1: return k - 1
    return max(0, k - 2 - (u - b1) // 16)
F0 = 4479
for name, lo, hi in (('luot duoi truoc dong ho', 4600, 4828), ('luot tren (sau dong ho + camera roi di)', 5480, 5900)):
    rows = list(csv.DictReader(open(os.path.join(D, 'crusher_%s.csv' % ('low' if lo < 5000 else 'up')))))
    for i in (1, 2, 3):
        ok = n = 0; seqv = []
        for r in rows:
            f = int(r['frame'])
            if f < lo or f > hi or int(r['dup']): continue
            k = int(r['k%d' % i]); m = float(r['m%d' % i])
            if k < 0 or m < 1.5: continue
            u = int(math.floor(U0[i] + (f - F0) * TPF))
            ok += fr_at(i, u) == k; n += 1
            seqv.append((f, k))
        # lech tick moc doi khung: moi lan video doi khung, tim moc doi khung Unity gan nhat cung (truoc, sau)
        offs = []
        for (fa, ka), (fb, kb) in zip(seqv, seqv[1:]):
            if ka == kb or fb - fa > 2: continue
            tv = (fb - F0) * TPF   # video doi trong (fa, fb]
            best = None
            for dt in range(-24, 25):
                u1 = int(math.floor(U0[i] + tv + dt - 1)); u2 = int(math.floor(U0[i] + tv + dt))
                if fr_at(i, u1) == ka and fr_at(i, u2) == kb:
                    if best is None or abs(dt) < abs(best): best = dt
            if best is not None: offs.append(best)
        offs = np.array(offs, float)
        P('%s, bay %d: %d/%d frame dung khung (%.0f%%); %d moc doi khung, lech TB %+.1f tick, rms %.1f tick' %
          (name, i, ok, n, 100.0 * ok / max(1, n), len(offs), offs.mean() if len(offs) else 0, math.sqrt((offs ** 2).mean()) if len(offs) else 0))
open(os.path.join(D, 'compare.txt'), 'w').write('\n'.join(out) + '\n')
