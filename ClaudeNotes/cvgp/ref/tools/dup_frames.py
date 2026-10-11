# dup_frames.py - phat hien frame lap (video quay man hinh) tren luoi NES: frame i la "lap" neu khong co pixel NES
# nao doi > 40 (moi kenh) so voi frame i-1 (nhieu nen video < 40). Ghi work/<video>_dup.npy (bool) + thong ke fps thuc.
# Chay: python -P -E tools/dup_frames.py map1
import sys, os
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
N = int(np.load(os.path.join(base, f'{video}_meta.npz'))['n'])
ch = np.zeros(N, int)
prev = np.asarray(mm[0]).astype(np.int16)
for i in range(1, N):
    a = np.asarray(mm[i]).astype(np.int16)
    ch[i] = int((np.abs(a - prev).max(axis=2) > 40).sum())
    prev = a
dup = ch == 0
dup[0] = False
np.save(os.path.join(base, f'{video}_dup.npy'), dup)
np.save(os.path.join(base, f'{video}_changed.npy'), ch)
from collections import Counter
runs = []; c = 1
for i in range(1, N):
    if dup[i]: c += 1
    else: runs.append(c); c = 1
print('do dai chuoi frame giong nhau:', sorted(Counter(runs).items())[:10])
print('ty le frame lap toan video: %.3f -> %.1f frame moi/s' % (dup.mean(), 30 * (1 - dup.mean())))
for s in range(0, int(N / 30), 10):
    a, b = s * 30, min(N, (s + 10) * 30)
    print(f'{s//60}:{s%60:02d}-{(s+10)//60}:{(s+10)%60:02d}  frame moi/s = {(~dup[a:b]).sum()/((b-a)/30):5.1f}')
