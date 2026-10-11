# door_camera.py - camera trong doan qua cua cuoi stage: ghep 2 vung ban do lien nhau (vd EntranceHall|CryptA)
# roi khop ZNCC playfield tung frame -> cx trong toa do ghep (0 = mep trai vung 1).
# Chay: python -P -E tools/door_camera.py map1 <secA> <secB> <f0> <f1> [step]
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *
video, sa, sb, f0, f1 = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4]), int(sys.argv[5])
step = int(sys.argv[6]) if len(sys.argv) > 6 else 6
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
mp = load_map()
A = gray(section_view(mp, sa)); B = gray(section_view(mp, sb))
cat = np.concatenate([A, B], axis=1)
wa = A.shape[1]
for i in range(f0, f1 + 1, step):
    pf = gray(np.asarray(mm[i][40:216]).astype(float))
    if pf.std() < 8:
        print(i, f'{i/30//60:.0f}:{i/30%60:05.2f}', 'den'); continue
    cx, sc, _ = match_camera(pf, cat)
    where = f'{sa}+{cx}' if cx <= wa - 256 else (f'qua cua: {sa} {cx} (= {sb} {cx - wa})')
    print(i, f'{i/30//60:.0f}:{i/30%60:05.2f}', 'cx_ghep', cx, f'score {sc:.3f}', where)
