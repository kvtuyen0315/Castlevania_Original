# locate_map2.py - moi frame (lay mau fps) -> vung sheet + cx tot nhat + diem ZNCC  => work/<video>_locate.csv
# Chay: python -P -E locate_map2.py <video> <fps> [ox oy sx sy]
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes2 import *
import numpy as np
video = sys.argv[1]; fps = sys.argv[2]
cal = dict(CALIB[video])
if len(sys.argv) > 6:
    cal = dict(ox=float(sys.argv[3]), oy=float(sys.argv[4]), sx=float(sys.argv[5]), sy=float(sys.argv[6]))
sheet = load_sheet()
secs = {k: gray(section(sheet, k))[16:192:2] for k in SECTIONS}

def zn_many(pf, sec, cxs):
    a = pf - pf.mean(); na = np.sqrt((a * a).sum())
    out = []
    for cx in cxs:
        b = sec[:, cx:cx + 256]; b = b - b.mean(); nb = np.sqrt((b * b).sum())
        out.append((a * b).sum() / (na * nb) if na * nb > 0 else 0)
    return np.array(out)

os.makedirs('../work', exist_ok=True)
outp = open('../work/%s_locate%s.csv' % (video, '' if fps == '2' else '_' + fps.replace('/', 'd')), 'w')
outp.write('t,section,cx,score,second,second_score,mean_lum\n')
step = 1.0 / float(eval(fps))
for i, img in enumerate(read_frames(video, fps=fps)):
    t = i * step
    pf = gray(nes_sample(img, cal, 48, 224))[::2]
    res = []
    for k, sec in secs.items():
        W = sec.shape[1]
        cxs = list(range(0, W - 256 + 1, 4))
        sc = zn_many(pf, sec, cxs)
        j = int(np.argmax(sc)); c0 = cxs[j]
        fine = list(range(max(0, c0 - 3), min(W - 256, c0 + 3) + 1))
        sf = zn_many(pf, sec, fine); jj = int(np.argmax(sf))
        res.append((float(sf[jj]), k, fine[jj]))
    res.sort(reverse=True)
    outp.write('%.2f,%s,%d,%.3f,%s,%.3f,%.1f\n' % (t, res[0][1], res[0][2], res[0][0], res[1][1], res[1][0], pf.mean()))
    if i % 50 == 0:
        print(t, res[0]); sys.stdout.flush()
outp.close()
