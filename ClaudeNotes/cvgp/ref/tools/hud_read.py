# hud_read.py - doc HUD tung frame tu memmap NES -> CSV
# Chay: python -P -E tools/hud_read.py map1     (can work/map1_nes.npy, work/map1_glyphkeys.npy)
# Cot: frame,t,hud_ok,score,time,stage,hearts,p,pbar,ebar,pempty,eempty,sub,mult,diff
#  - chu so: khop mau nhi phan 8x8 (Hamming) voi 10 mau so (glyph key 0..9 da dan nhan bang mat)
#  - pbar/ebar: so vach DAY (tam vach co mau do/cam), pempty/eempty: so vach rong (vien trang)
#  - sub: chu ky mau trong khung vu khi phu (x130..157,y26..43): none/dagger/axe/holy/cross/watch/?
#  - mult: icon II/III tai (208..223, 24..39)
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from hud_cells import *
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
meta = np.load(os.path.join(base, f'{video}_meta.npz'))
N = int(meta['n']); diff = meta['diff_full']
keys = np.load(os.path.join(base, 'map1_glyphkeys.npy'))   # mau so lay tu map1 (cung font)
# nhan dan bang mat tu frames/glyphs_map1.png: key j -> chu so
LABEL = {0: '0', 1: '1', 2: '3', 3: '2', 4: '4', 5: '7', 6: '6', 7: '9', 8: '5', 9: '8'}
T = np.array([keys[j] for j in range(10)]).reshape(10, 64)
TL = [LABEL[j] for j in range(10)]
Y0 = 8
ref = np.asarray(mm[90]) if video == 'map1' else None

def digit(fr, x, y):
    b = binar(cell(fr, x, y, Y0)).reshape(64)
    d = (T != b).sum(axis=1)
    j = int(np.argmin(d))
    return TL[j] if d[j] <= 6 else '?'

def read(fr):
    s = ''.join(digit(fr, *CELLS[f'score{i}']) for i in range(6))
    tm = ''.join(digit(fr, *CELLS[f'time{i}']) for i in range(4))
    st = ''.join(digit(fr, *CELLS[f'stage{i}']) for i in range(2))
    he = ''.join(digit(fr, *CELLS[f'heart{i}']) for i in range(2))
    pp = ''.join(digit(fr, *CELLS[f'p{i}']) for i in range(2))
    f = fr.astype(int)
    out = []
    for yy, kind in ((27, 'P'), (35, 'E')):
        full = 0; empty = 0
        for i in range(16):
            c = f[yy - Y0, 57 + 4 * i]
            e = f[yy - Y0 - 2, 56 + 4 * i]   # vien tren-trai cua vach rong
            if c[0] > 110 and c[0] - c[2] > 60:
                full += 1
            elif c.max() < 90 and e.min() > 120:
                empty += 1
        out += [full, empty]
    box = f[26 - Y0:44 - Y0, 130:158]
    nb = box.max(axis=2) > 70
    n = int(nb.sum())
    if n < 8:
        sub = 'none'
    else:
        px = box[nb]
        purple = ((px[:, 0] > 110) & (px[:, 0] < 255) & (px[:, 2] > 170) & (px[:, 1] < 150)).sum()
        blue = ((px[:, 2] > 140) & (px[:, 0] < 70)).sum()
        cream = ((px[:, 0] > 180) & (px[:, 1] > 160) & (px[:, 2] < 200)).sum()
        if purple > 0.4 * n: sub = 'axe'
        elif n <= 45 and blue >= 0.25 * n and cream >= 0.2 * n: sub = 'dagger'
        elif n > 45 and (blue > 0.3 * n or (blue >= 0.1 * n and cream >= 0.1 * n)): sub = 'holy'   # binh nuoc thanh (xanh + nap vang)
        else: sub = f'?{n}'
    ic = f[24 - Y0:40 - Y0, 208:224]
    red = ((ic[..., 0] > 150) & (ic[..., 1] < 90)).sum()
    if red < 20:
        mult = ''
    else:
        wh = (ic.min(axis=2) > 170)
        # so cot dung trang trong icon (gom ca 2 canh khung): II = 4, III = 5
        colw = wh[4:12].sum(axis=0) >= 5
        bars_ = 0; prev = False
        for v in colw:
            if v and not prev: bars_ += 1
            prev = v
        mult = {4: 'II', 5: 'III'}.get(bars_, f'x{bars_}')   # khung trang 2 cot + 2 hoac 3 thanh
    return s, tm, st, he, pp, out, sub, mult

def hud_ok(fr):
    if ref is None: return True
    # chu SCORE / PLAYER / ENEMY phai giong frame tham chieu
    ok = 0
    for (x, y) in ((0, 16), (8, 16), (16, 16), (0, 24), (8, 24), (0, 32), (8, 32)):
        a = binar(cell(fr, x, y, Y0)); b = binar(cell(ref, x, y, Y0))
        ok += int((a != b).sum() <= 6)
    return ok == 7

rows = []
for i in range(N):
    fr = np.asarray(mm[i])
    s, tm, st, he, pp, bars_, sub, mult = read(fr)
    rows.append([i, f'{i/30:.3f}', int(hud_ok(fr)), s, tm, st, he, pp, *bars_, sub, mult, f'{diff[i]:.2f}'])
with open(os.path.join(base, f'{video}_hud.csv'), 'w', newline='') as fh:
    w = csv.writer(fh)
    w.writerow(['frame', 't', 'hud_ok', 'score', 'time', 'stage', 'hearts', 'p', 'pbar', 'pempty', 'ebar', 'eempty', 'sub', 'mult', 'diff'])
    w.writerows(rows)
print('wrote', len(rows))
