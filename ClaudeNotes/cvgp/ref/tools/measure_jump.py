# measure_jump.py - tim cac cu nhay cua Simon (y chan y1 nho hon nen roi tro lai), do do cao + thoi luong
# Chay: python -P -E tools/measure_jump.py map1 [f_from f_to]
# Thoi luong: (a) dem frame video tu frame cuoi o nen -> frame dau tro lai nen (sai so +-1 frame moi dau = +-2 tick 60Hz)
#             (b) neu nhay kem di chuyen: quang duong ngang world_x / 1.0 px/frame NES  (Simon di 1 px/frame)
#             (c) khop parabol y1(t) cho cac frame tren khong -> 2 nghiem = thoi gian bay
import sys, os, csv
import numpy as np
video = sys.argv[1]
fa, fb = (int(sys.argv[2]), int(sys.argv[3])) if len(sys.argv) > 3 else (0, 10 ** 9)
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = [r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv'))) if fa <= int(r['frame']) <= fb]
NES = 60.0988
F = np.array([int(r['frame']) for r in S])
ok = np.array([r['found'] == '1' for r in S])
Y1 = np.array([float(r['y1']) if r['found'] == '1' else np.nan for r in S])
Y0 = np.array([float(r['y0']) if r['found'] == '1' else np.nan for r in S])
WX = np.array([float(r['world_x']) if r['found'] == '1' and r['world_x'] else np.nan for r in S])
SX = np.array([float(r['x_med']) if r['found'] == '1' else np.nan for r in S])
jumps = []
i = 6
n = len(S)
while i < n - 5:
    # nen = trung vi y1 cua 6 frame truoc (phai on dinh)
    prev = Y1[i - 6:i]
    if np.isnan(prev).sum() > 1 or np.isnan(prev).all() or np.nanmax(prev) - np.nanmin(prev) > 1 or np.isnan(Y1[i]):
        i += 1; continue
    basey = np.nanmedian(prev)
    if Y1[i] <= basey - 3:
        # cat canh: tim ket thuc (tro ve nen +-1 hoac thap hon)
        j = i
        while j < n and (np.isnan(Y1[j]) or Y1[j] < basey - 1) and j - i < 90:
            j += 1
        if j >= n or j - i >= 90:
            i += 1; continue
        seg = slice(i, j)
        pk = np.nanmin(Y1[seg])
        rise = basey - pk
        landed = Y1[j]
        # loai cau thang: o cau thang y giam dan deu nhieu frame, khong tro lai nen
        if rise >= 8 and abs(landed - basey) <= 1:
            t0 = (F[i - 1] + F[i]) / 2.0; t1 = (F[j - 1] + F[j]) / 2.0
            dur_v = (t1 - t0)                 # frame video (30 fps)
            dx = WX[j] - WX[i - 1] if not np.isnan(WX[j]) and not np.isnan(WX[i - 1]) else np.nan
            # parabol
            m = ~np.isnan(Y1[seg])
            tt = F[seg][m] / 30.0; yy = Y1[seg][m]
            par = None
            if m.sum() >= 5:
                c2, c1, c0 = np.polyfit(tt, yy, 2)
                disc = c1 * c1 - 4 * c2 * (c0 - basey)
                if c2 > 0 and disc > 0:
                    r1 = (-c1 - np.sqrt(disc)) / (2 * c2); r2 = (-c1 + np.sqrt(disc)) / (2 * c2)
                    tpk = -c1 / (2 * c2); ypk = c0 - c1 * c1 / (4 * c2)
                    par = (r2 - r1, basey - ypk, c2)
            jumps.append(dict(f0=int(F[i - 1]), f1=int(F[j]), base=basey, rise=rise, dur_v=dur_v, dx=dx,
                              head_rise=np.nanmedian(Y0[i - 6:i]) - np.nanmin(Y0[seg]), par=par, sx0=SX[i - 1], sx1=SX[j]))
        i = j + 1; continue
    i += 1
print(f'{len(jumps)} cu nhay tim duoc')
print(' f_start  f_end   t_start   base  rise_feet  head_rise  dur(video f)  dur(NES f ~x2.003)  dx_world  par_dur(NESf) par_rise')
for J in jumps:
    p = J['par']
    pd = f"{p[0]*NES:6.1f}" if p else '   -  '
    pr = f"{p[1]:5.1f}" if p else '  -  '
    t = J['f0'] / 30
    print(f" {J['f0']:6d} {J['f1']:6d}  {int(t//60)}:{t%60:05.2f}  {J['base']:5.0f}  {J['rise']:6.0f}    {J['head_rise']:6.0f}     {J['dur_v']:5.1f}        {J['dur_v']*NES/30:6.1f}        {J['dx']:6.1f}     {pd}     {pr}")
r = np.array([J['rise'] for J in jumps]); d = np.array([J['dur_v'] * NES / 30 for J in jumps])
dx = np.array([abs(J['dx']) for J in jumps if not np.isnan(J['dx'])])
print(f"rise feet: median {np.median(r):.1f} (min {r.min():.0f} max {r.max():.0f}); dur NES frames: median {np.median(d):.1f} (p25 {np.percentile(d,25):.1f} p75 {np.percentile(d,75):.1f})")
mv = [J for J in jumps if not np.isnan(J['dx']) and abs(J['dx']) > 20]
if mv:
    print('Nhay kem di chuyen (|dx|>20): dx =', [round(abs(J['dx'])) for J in mv], ' median', np.median([abs(J['dx']) for J in mv]))
pp = [J['par'] for J in jumps if J['par']]
if pp:
    print(f"Parabol: thoi gian bay median {np.median([p[0] for p in pp])*NES:.1f} NES f, do cao median {np.median([p[1] for p in pp]):.1f} px")
