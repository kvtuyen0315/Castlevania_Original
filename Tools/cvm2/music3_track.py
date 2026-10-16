# -*- coding: utf-8 -*-
"""cvm2-03 - kiem nhanh: do khop cuc bo (cua so ngan) giua am thanh video va 1 bai o cac 'goc' (offset) cho truoc.
python -P -E Tools/cvm2/music3_track.py <video> NN t0 t1 goc1 [goc2 ...] [--win 0.25]
In moi cua so: t, rms video, ncc theo tung goc (songpos = t - goc). Dung de tim mep bat dau/dung/tiep tuc cua nhac."""
import sys, os
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
import music3 as M
a = sys.argv[1:]
win = 0.25
if '--win' in a:
    i = a.index('--win'); win = float(a[i + 1]); del a[i:i + 2]
v, nn, t0, t1 = a[0], a[1], float(a[2]), float(a[3])
gocs = [float(g) for g in a[4:]]
F, e = M.vid_feat(v)
B = M.song_feat(nn)[0]
fs = M.SR / M.HOP
W = max(2, int(win * fs))
t = t0
while t < t1:
    i = int(t * fs)
    A = F[i:i + W]
    out = []
    for g in gocs:
        k = int(round((t - g) * fs))
        if 0 <= k and k + W <= len(B):
            out.append('%.2f' % float((A * B[k:k + W]).sum(1).mean()))
        else:
            out.append(' -- ')
    print('%7.2f  lvl %5.2f  ' % (t, float(e[i:i + W].mean())) + '  '.join(out))
    t += win
