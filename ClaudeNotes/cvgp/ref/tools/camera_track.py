# camera_track.py - xac dinh vung ban do + camera x (toa do pixel NES trong vung) cho TUNG frame
# Chay: python -P -E tools/camera_track.py map1      -> work/map1_camera.csv (frame,t,section,cx,score)
# Cach lam: playfield NES y 48..223 (xam) so ZNCC voi sprite sheet ban do (nes_common.SECTIONS).
#  - moi 15 frame: tim toan bo (5 vung x moi cx)
#  - frame giua: tim cuc bo cx +-10 trong vung hien tai; neu score < 0.55 -> tim toan bo
#  - frame khong co HUD (hud_ok=0 trong work/map1_hud.csv) hoac man den: section='-'
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
N = int(np.load(os.path.join(base, f'{video}_meta.npz'))['n'])
hud = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_hud.csv')))}
Y0 = 8
mp = load_map()
secs = {k: gray(section_view(mp, k)) for k in SECTIONS}

def zncc_local(pf, sg, cands):
    P = pf - pf.mean(); Pn = np.sqrt((P * P).sum()) + 1e-6
    best = (-2, None)
    for cx in cands:
        if cx < 0 or cx + 256 > sg.shape[1]: continue
        W = sg[:, cx:cx + 256]; Wm = W - W.mean()
        s = (P * Wm).sum() / (Pn * (np.sqrt((Wm * Wm).sum()) + 1e-6))
        if s > best[0]: best = (s, cx)
    return best

def full(pf):
    best = (-2, None, None)
    for k, sg in secs.items():
        cx, sc, _ = match_camera(pf, sg)
        if sc > best[0]: best = (sc, k, cx)
    return best

out = []
cur = None
for i in range(N):
    h = hud.get(i)
    fr = np.asarray(mm[i]).astype(float)
    pf = gray(fr[48 - Y0:224 - Y0])
    if h is None or h['hud_ok'] != '1' or pf.std() < 8:
        out.append((i, '-', -1, 0.0)); cur = None; continue
    if cur is None or i % 15 == 0:
        sc, k, cx = full(pf)
        cur = (k, cx)
    else:
        k, cx0 = cur
        sc, cx = zncc_local(pf, secs[k], range(cx0 - 10, cx0 + 11))
        if sc < 0.55:
            sc, k, cx = full(pf)
        cur = (k, cx)
    out.append((i, cur[0], cur[1], sc))
    if i % 900 == 0: print(i, out[-1], flush=True)
with open(os.path.join(base, f'{video}_camera.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'section', 'cx', 'score'])
    for i, k, cx, sc in out:
        w.writerow([i, f'{i/30:.3f}', k, cx, f'{sc:.3f}'])
print('done')
