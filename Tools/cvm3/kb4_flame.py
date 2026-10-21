# kb4_flame.py (CVM3 04) - chuoi khung ngon lua nuoc thanh (holywater_fire_0..4) tai cho vo: khung nao khop tot nhat moi frame, tu luc vo toi khi tat.
# Chay: python -P -E Tools/cvm3/kb4_flame.py <video> <f0> <f1> <X> <san_Y>   (X = cot giua lua, san_Y = hang mat san NES)
# In: frame, dup, khung (0..4 / '-'), err ; chuoi khung theo frame
import sys, os
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K
import kb4_holy as H

video = sys.argv[1]; f0, f1, X, FY = map(int, sys.argv[2:6])
fires = [H.load('holywater_fire_%d' % i) for i in range(5)]
prev = None; seq = []
for f, img, nes in K.read_frames(video, f0, f1):
    dup = K.is_dup(nes, prev); prev = nes
    best = (9, '-', 0, 0)
    for i, t in enumerate(fires):
        # slot day-giua tai (X, FY): goc trai = X - w/2, dinh = FY - h (+-2 px)
        e = H.errmap(nes, t, X - t['w'] // 2 - 3, X - t['w'] // 2 + 4, FY - t['h'] - 2, FY - t['h'] + 3)
        iy, ix = np.unravel_index(np.argmin(e), e.shape)
        if e[iy, ix] < best[0]:
            best = (float(e[iy, ix]), str(i), ix - 3, iy - 2)
    k = best[1] if best[0] < 0.12 else '-'
    seq.append('=' if dup else k)
    print(f, int(dup), k, '%.3f' % best[0], 'dx %+d dy %+d' % (best[2], best[3]))
print(''.join(seq))
