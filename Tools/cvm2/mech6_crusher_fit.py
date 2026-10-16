# mech6_crusher_fit.py (CVM2 06) - khop mo hinh bay ep (luoi chung 4/16 tick, kmax rieng) vao MOI frame do duoc (crusher_low/up.csv)
# Mo hinh (bo dem c tick chung, dung khi Dong ho): o s = c//16; bay i bat dau ha o cac o (s - r_i) % L_i == 0;
#   u = tick tu luc bat dau ha: k = 1 + u//4 (u < 4*kmax) -> k = kmax-1 tu u = 4*kmax toi moc 16 ke tiep b1 -> giam 1 moi moc 16 -> k0 giu toi het chu ky
#   L_i = b1/16 + kmax - 1 (kmax 6/4/7 => 7/5/8 o)
# Tham so tim: pha luoi phi (tick), r_i, thoi luong dong bang N (tick), TPF. In ket qua + trang thai bay tai cac moc.
# Chay: python -P -E Tools/cvm2/mech6_crusher_fit.py
import os, csv, sys
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06')
KMAX = {1: 6, 2: 4, 3: 7}
def cyc(kmax):
    b1 = 16 * (4 * kmax // 16 + 1)
    return b1, b1 // 16 + kmax - 1
def kat(i, u):
    kmax = KMAX[i]; b1, L = cyc(kmax)
    if u < 4 * kmax: return 1 + u // 4
    if u < b1: return kmax - 1
    return max(0, kmax - 2 - (u - b1) // 16)
obs = []
for n in ('low', 'up'):
    for r in csv.DictReader(open(os.path.join(D, 'crusher_%s.csv' % n))):
        if int(r['dup']): continue
        f = int(r['frame'])
        for i in (1, 2, 3):
            k = int(r['k%d' % i]); m = float(r['m%d' % i])
            if k >= 0 and m >= 1.5: obs.append((f, i, k))
F = np.array([o[0] for o in obs], float); I = np.array([o[1] for o in obs]); K = np.array([o[2] for o in obs])
TS = 4829   # f: dung dong ho (HUD tim 30->25, MAP2_TIMELINE 3 #37)
def predict(phi, r, N, tpf):
    t = F * tpf
    ts = TS * tpf
    c = np.where(t < ts, t, np.where(t < ts + N, ts, t - N)) - phi
    c = np.floor(c).astype(int)
    s = c // 16; sub = c % 16
    out = np.zeros(len(c), int)
    for i in (1, 2, 3):
        b1, L = cyc(KMAX[i])
        m = I == i
        u = ((s[m] - r[i - 1]) % L) * 16 + sub[m]
        out[m] = [kat(i, int(x)) for x in u]
    return out
best = None
for tpf in (2.0013, 2.0023, 2.0033, 2.0043, 2.0053):
    for phi in np.arange(0, 16, 0.5):
        for N in range(160, 224, 2):
            # r_i doc lap: chon tung bay
            r = [0, 0, 0]; tot = 0
            for i in (1, 2, 3):
                b1, L = cyc(KMAX[i]); bi = None
                for ri in range(L):
                    rr = list(r); rr[i - 1] = ri
                    p = predict(phi, rr, N, tpf); m = I == i
                    sc = int((p[m] == K[m]).sum())
                    if bi is None or sc > bi[0]: bi = (sc, ri)
                r[i - 1] = bi[1]; tot += bi[0]
            if best is None or tot > best[0]: best = (tot, tpf, phi, N, tuple(r))
tot, tpf, phi, N, r = best
print('khop %d / %d frame-bay (%.1f%%)  TPF %.4f  pha luoi %.1f tick  dong bang N %d tick  r %s' % (tot, len(K), 100.0 * tot / len(K), tpf, phi, N, r))
p = predict(phi, list(r), N, tpf)
for i in (1, 2, 3):
    m = I == i
    d = p[m] - K[m]
    print('  bay %d: %d frame, dung %d, |lech| 1 nac: %d, >1: %d' % (i, m.sum(), (d == 0).sum(), (abs(d) == 1).sum(), (abs(d) > 1).sum()))
# luu tham so
with open(os.path.join(D, 'crusher_fit.txt'), 'w') as fh:
    fh.write('tpf %.4f phi %.2f N %d r %d %d %d\n' % (tpf, phi, N, r[0], r[1], r[2]))
