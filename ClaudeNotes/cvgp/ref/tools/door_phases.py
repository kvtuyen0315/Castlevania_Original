# door_phases.py - phan pha doan qua cua: camera (ghep 2 vung) tung frame + vi tri Simon -> cac moc
# Chay: python -P -E tools/door_phases.py map1 <secA> <secB> <f0> <f1>
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *
video, sa, sb, f0, f1 = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4]), int(sys.argv[5])
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_simon.csv')))}
dup = np.load(os.path.join(base, f'{video}_dup.npy'))
mp = load_map()
cat = np.concatenate([gray(section_view(mp, sa)), gray(section_view(mp, sb))], axis=1)
P = None
rows = []
prev = None
for i in range(f0, f1 + 1):
    pf = gray(np.asarray(mm[i][40:216]).astype(float))
    if prev is None:
        cx, sc, _ = match_camera(pf, cat)
    else:
        # cuc bo
        best = (-2, prev)
        Pm = pf - pf.mean(); Pn = np.sqrt((Pm * Pm).sum())
        for c in range(max(0, prev - 8), min(cat.shape[1] - 256, prev + 9)):
            W = cat[:, c:c + 256]; Wm = W - W.mean()
            s = (Pm * Wm).sum() / (Pn * np.sqrt((Wm * Wm).sum()) + 1e-6)
            if s > best[0]: best = (s, c)
        sc, cx = best
    prev = cx
    s = S[i]
    rows.append((i, cx, sc, float(s['x_med']) if s['found'] == '1' else None, int(s['y1']) if s['found'] == '1' else None))
# in moc: doi trang thai camera dung/chay; Simon xuat hien/bien mat
def ts(i): t = i / 30; return '%d:%05.2f' % (t // 60, t % 60)
state = None; seg_start = rows[0]
for k in range(1, len(rows)):
    moving = rows[k][1] != rows[k - 1][1]
    # lam muot: coi la dung neu 8 frame toi khong doi
    window = [r[1] for r in rows[k:k + 8]]
    mv = len(set(window)) > 1
    if state is None: state = mv
    if mv != state:
        a = seg_start; b = rows[k]
        dur = (b[0] - a[0]) / 30
        print(f"{'CUON' if state else 'DUNG'}: {ts(a[0])}-{ts(b[0])} ({dur:.2f}s) cx {a[1]}->{b[1]}" + (f"  v={(b[1]-a[1])/dur/60.0988:.3f} px/tick" if state and dur > 0 else ''))
        seg_start = b; state = mv
a = seg_start; b = rows[-1]
print(f"{'CUON' if state else 'DUNG'}: {ts(a[0])}-{ts(b[0])} cx {a[1]}->{b[1]}")
# Simon: in vi tri moi 6 frame
print('Simon (frame: x man hinh / y chan / cx):')
print(' '.join(f"{r[0]}:{'-' if r[3] is None else int(r[3])}/{'' if r[4] is None else r[4]}/{r[1]}" for r in rows[::6]))
