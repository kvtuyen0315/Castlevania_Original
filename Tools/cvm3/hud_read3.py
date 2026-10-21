# hud_read3.py (CVM3 02) - đọc HUD từng frame từ memmap NES (Map_3 / Boss_Map_3) -> CSV. Khuôn hud_read.py của Map 1.
# Chạy (từ gốc repo): python -P -E Tools/cvm3/hud_read3.py map3   (cần _cache/02/work/<video>_nes.npy + cvgp/_cache/video/work/map1_glyphkeys.npy)
# Cột: frame,t,hud_ok,score,time,stage,hearts,p,pbar,pempty,ebar,eempty,sub,mult,diff
#  - chữ số: khớp mẫu nhị phân 8x8 (Hamming ≤ dung sai) với 10 mẫu số lấy từ Map 1 (cùng font HUD)
#  - pbar/ebar: số vạch ĐẦY; pempty/eempty: số vạch RỖNG (viền trắng)
#  - sub: chữ ký vũ khí phụ (khung x130..157,y26..43) ; mult: icon II/III
import sys, os, csv
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
KEYS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work', 'map1_glyphkeys.npy')
Y0 = 8
CELLS = {}
for i in range(6): CELLS['score%d' % i] = (48 + 8 * i, 16)
for i in range(4): CELLS['time%d' % i] = (144 + 8 * i, 16)
for i in range(2):
    CELLS['stage%d' % i] = (232 + 8 * i, 16)
    CELLS['heart%d' % i] = (184 + 8 * i, 24)
    CELLS['p%d' % i] = (184 + 8 * i, 32)
LABEL = {0: '0', 1: '1', 2: '3', 3: '2', 4: '4', 5: '7', 6: '6', 7: '9', 8: '5', 9: '8'}   # hud_read.py (nhãn bằng mắt)
keys = np.load(KEYS)
T = np.array([keys[j] for j in range(10)]).reshape(10, 64)
TL = [LABEL[j] for j in range(10)]

def cell(fr, x, y): return fr[y - Y0: y - Y0 + 8, x: x + 8]
def binar(c, thr=128): return c.min(axis=2) > thr

def digit(fr, x, y, tol=8):
    b = binar(cell(fr, x, y)).reshape(64)
    d = (T != b).sum(axis=1)
    j = int(np.argmin(d))
    return TL[j] if d[j] <= tol else '?'

# mẫu vũ khí phụ: lấy từ frame video sạch (đã xem bằng mắt: shots/sub_reps.png) — Map_3: f100 = rìu, f1000 = thập tự (boomerang), f7000 = đồng hồ (stopwatch; so Item/stopwatch.png - KHONG phai nuoc thanh)
SUB_REF_FRAMES = {'map3': {'watch': 250, 'holy': 700}, 'boss3': {'dagger': 300}}   # CVM3: map3 f250 = dong ho, f700 = nuoc thanh; boss3 f300 = dao gam
SUB_REF = {}
def _init_sub(mm, video):
    for k, i in SUB_REF_FRAMES[video].items():
        SUB_REF[k] = (np.asarray(mm[i]).astype(int)[26 - Y0:44 - Y0, 130:158].max(axis=2) > 70)
def sub_label(nb, tol=14):
    if not SUB_REF: return 'n%d' % int(nb.sum())
    d = {k: int((v != nb).sum()) for k, v in SUB_REF.items()}
    k = min(d, key=d.get)
    return k if d[k] <= tol else 'occ'            # occ = bi sprite khac (dao/thap tu dang bay) de len o vu khi phu

def read(fr):
    s = ''.join(digit(fr, *CELLS['score%d' % i]) for i in range(6))
    tm = ''.join(digit(fr, *CELLS['time%d' % i]) for i in range(4))
    st = ''.join(digit(fr, *CELLS['stage%d' % i]) for i in range(2))
    he = ''.join(digit(fr, *CELLS['heart%d' % i]) for i in range(2))
    pp = ''.join(digit(fr, *CELLS['p%d' % i]) for i in range(2))
    f = fr.astype(int)
    out = []
    for yy in (27, 35):
        full = 0; empty = 0
        for i in range(16):
            c = f[yy - Y0, 57 + 4 * i]
            e = f[yy - Y0 - 2, 56 + 4 * i]
            if c[0] > 110 and c[0] - c[2] > 60: full += 1
            elif c.max() < 90 and e.min() > 120: empty += 1
        out += [full, empty]
    box = f[26 - Y0:44 - Y0, 130:158]
    nb = box.max(axis=2) > 70
    n = int(nb.sum())
    sub = 'none' if n < 8 else sub_label(nb)
    ic = f[24 - Y0:40 - Y0, 208:224]
    red = ((ic[..., 0] > 150) & (ic[..., 1] < 90)).sum()
    if red < 20:
        mult = ''
    else:
        wh = (ic.min(axis=2) > 170)
        colw = wh[4:12].sum(axis=0) >= 5
        bars_ = 0; prev = False
        for v in colw:
            if v and not prev: bars_ += 1
            prev = v
        mult = {4: 'II', 5: 'III'}.get(bars_, 'x%d' % bars_)
    return s, tm, st, he, pp, out, sub, mult

if __name__ == '__main__':
    video = sys.argv[1]
    ref_idx = int(sys.argv[2]) if len(sys.argv) > 2 else (300 if video == 'map3' else 300)
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    meta = np.load(os.path.join(WORK, video + '_meta.npz'))
    N = int(meta['n']); diff = meta['diff_full']
    ref = np.asarray(mm[ref_idx])
    _init_sub(mm, video)
    def hud_ok(fr):
        ok = 0
        for (x, y) in ((0, 16), (8, 16), (16, 16), (0, 24), (8, 24), (0, 32), (8, 32)):
            a = binar(cell(fr, x, y)); b = binar(cell(ref, x, y))
            ok += int((a != b).sum() <= 6)
        return ok == 7
    rows = []
    for i in range(N):
        fr = np.asarray(mm[i])
        s, tm, st, he, pp, bars_, sub, mult = read(fr)
        rows.append([i, '%.3f' % (i / 30), int(hud_ok(fr)), s, tm, st, he, pp, *bars_, sub, mult, '%.2f' % diff[i]])
    with open(os.path.join(WORK, video + '_hud.csv'), 'w', newline='') as fh:
        w = csv.writer(fh)
        w.writerow(['frame', 't', 'hud_ok', 'score', 'time', 'stage', 'hearts', 'p', 'pbar', 'pempty', 'ebar', 'eempty', 'sub', 'mult', 'diff'])
        w.writerows(rows)
    print('wrote', len(rows))
