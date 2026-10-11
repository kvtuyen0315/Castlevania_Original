# whip_track.py - do roi: tru nen ban do (fg_map) roi tim phan tien canh nam ngang keo dai ra khoi than Simon
# Chay: python -P -E tools/whip_track.py map1   -> work/map1_whip.csv + in danh sach don roi
# Moi frame (co camera + Simon): ext_l / ext_r = so px tien canh noi lien (cho phep ho <=2 px) tinh tu mep bbox Simon
#   trong dai hang [y0+2, y0+13] (tay cam roi). Don roi = chuoi frame ext >= 14 px cung 1 phia.
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fg_map import fg_mask
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_camera.csv')))}
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
N = len(S)
def reach(row_any, start, direction, maxlen=64, gap=2):
    """row_any: bool array theo cot; tu cot start di theo direction, tra ve do dai lien tuc (cho phep ho <= gap)."""
    last = 0; miss = 0; k = 0
    while k < maxlen:
        c = start + direction * k
        if c < 0 or c >= len(row_any): break
        if row_any[c]: last = k + 1; miss = 0
        else:
            miss += 1
            if miss > gap: break
        k += 1
    return last
rows = []
for i in range(N):
    s = S[i]; c = cam[i]
    if s['found'] != '1' or c['section'] == '-' or float(c['score']) < 0.8:
        rows.append((i, '', '', '', '')); continue
    x0, x1, y0, y1 = int(s['x0']), int(s['x1']), int(s['y0']), int(s['y1'])
    m = fg_mask(np.asarray(mm[i]), c['section'], int(c['cx']))
    r0, r1 = max(0, y0 + 2 - 48), min(176, y0 + 14 - 48)
    band = m[r0:r1].any(axis=0)
    el = reach(band, x0 - 2, -1)        # bat dau ngay sat mep trai than (bo 1 px vien)
    er = reach(band, x1 + 2, +1)
    rows.append((i, el, er, y0, y1))
with open(os.path.join(base, f'{video}_whip.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 'ext_l', 'ext_r', 'y0', 'y1']); w.writerows(rows)
# don roi
def ts(i): t = i / 30; return '%d:%05.2f' % (t // 60, t % 60)
atk = []
i = 0
while i < N:
    r = rows[i]
    if r[1] != '' and max(r[1], r[2]) >= 14:
        side = 'L' if r[1] >= r[2] else 'R'
        j = i
        while j < N and rows[j][1] != '' and (rows[j][1] if side == 'L' else rows[j][2]) >= 14:
            j += 1
        L = [rows[k][1] if side == 'L' else rows[k][2] for k in range(i, j)]
        atk.append((i, j - 1, side, max(L), int(np.median(L)), rows[i][4] - rows[i][3]))
        i = j; continue
    i += 1
print(f'{len(atk)} lan roi duoi ra (ext>=14px):')
for a in atk:
    print(f'  f{a[0]}-{a[1]} {ts(a[0])} phia {a[2]} so frame video {a[1]-a[0]+1:2d} (~{(a[1]-a[0]+1)*2:2d} tick)  dai max {a[3]:2d}px trung vi {a[4]:2d}px  cao Simon {a[5]+1}')
