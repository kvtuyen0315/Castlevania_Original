# simon_boss2.py (CVM2 02) - Simon trong video boss Map 2 (bảng màu khác): khớp mẫu sprite (Tools/cvgp/simon_fit.py) quanh blob cam/kem của boss2_blobs.csv
# Chạy: python -P -E Tools/cvm2/simon_boss2.py  -> work/boss2_simon.csv (frame,t,dup,sprite,flip,px,Y,err,n,found)
import sys, os, csv
import numpy as np
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE); sys.path.insert(0, os.path.join(HERE, '..', 'cvgp'))
import simon_fit as sf
from fg2 import WORK
mm = np.load(os.path.join(WORK, 'boss2_nes.npy'), mmap_mode='r'); N = mm.shape[0]
dup = np.load(os.path.join(WORK, 'boss2_dup.npy'))
B = {}
for r in csv.DictReader(open(os.path.join(WORK, 'boss2_blobs.csv'))): B.setdefault(int(r['frame']), []).append(r)
sprites = sf.load_sprites()
def blob(f):
    best = None
    for r in B.get(f, []):
        o, c = int(r['or']), int(r['cr']); h = int(r['y1']) - int(r['y0']) + 1; w = int(r['x1']) - int(r['x0']) + 1
        if o >= 8 and c >= 4 and 8 <= h <= 40 and w <= 60:
            if best is None or o + c > best[0]: best = (o + c, r)
    return best[1] if best else None
def run(pal, step, search):
    out = []; prev = None
    for f in range(0, N, step):
        bl = blob(f); nes = mm[f].astype(float)
        if prev is not None and step == 1 and f - prev['frame'] <= 3: g = (prev['lx'], prev['lx'] + 16, prev['ty'], prev['ty'] + 31)
        elif bl is not None: g = (int(bl['x0']) - 2, int(bl['x1']), int(bl['y0']) - 2, int(bl['y1']))
        else: prev = None; continue
        b = sf.fit_frame(nes, sprites, pal, g, search=search)
        if b is not None and b['err'] > 0.12 and bl is not None:
            g2 = (int(bl['x0']) - 2, int(bl['x1']), int(bl['y0']) - 2, int(bl['y1'])); b2 = sf.fit_frame(nes, sprites, pal, g2, search=search)
            if b2 is not None and b2['err'] < b['err']: b = b2
        if b is None: prev = None; continue
        b['frame'] = f; out.append(b); prev = b if b['err'] < 0.12 else None
    return out
pal = sf.SHEET_COLORS
res = run(pal, 6, 14)
for it in range(2):
    pal = sf.calibrate_palette(res); print('bang mau video boss:', [tuple(int(round(x)) for x in c) for c in pal], flush=True)
    res = run(pal, 6, 14)
full = run(pal, 1, 8)
got = {b['frame']: b for b in full}
with open(os.path.join(WORK, 'boss2_simon.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'dup', 'sprite', 'flip', 'px', 'Y', 'err', 'n', 'found'])
    for f in range(N):
        b = got.get(f)
        if b is None or b['err'] > 0.25: w.writerow([f, '%.3f' % (f / 30), int(dup[f]), '', '', '', '', '', '', 0]); continue
        w.writerow([f, '%.3f' % (f / 30), int(dup[f]), b['sprite'], b['flip'], b['px'], b['Y'], '%.4f' % b['err'], b['n'], 1])
print('found', len(full), 'of', N, 'median err %.3f' % float(np.median([b['err'] for b in full])))
