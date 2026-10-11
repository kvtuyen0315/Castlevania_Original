# refine_calib2.py - tinh chinh hieu chuan bang luoi tim kiem (pivot o tam playfield) khop voi sprite sheet
# Chay: python -P -E tools/refine_calib2.py <map1|boss> <frame.png> [frame2.png ...]
# Tham so hoa: x_v = cxv + sx*(x-128) ; y_v = cyv + sy*(y-136)  (128,136 = tam playfield NES)
# Quet doc lap truc ngang (sx, cxv) va truc doc (sy, cyv); in bo tot nhat va doi lai (ox, oy).
import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *

video = sys.argv[1]; files = sys.argv[2:]
cal0 = dict(CALIB[video])
mp = load_map()
secs = {k: gray(section_view(mp, k)) for k in SECTIONS}
imgs = [np.asarray(Image.open(f).convert('RGB')).astype(float) for f in files]

def locate(img, cal):
    pf = gray(nes_sample(img, cal, y0=48, y1=224))
    best = None
    for k, sg in secs.items():
        cx, sc, _ = match_camera(pf, sg)
        if best is None or sc > best[2]:
            best = (k, cx, sc)
    return best
locs = [locate(im, cal0) for im in imgs]
print('locations:', locs)
good = [i for i, l in enumerate(locs) if l[2] > 0.6]

def score(cal):
    tot = 0
    for i in good:
        k, cx, _ = locs[i]
        pf = gray(nes_sample(imgs[i], cal, y0=48, y1=224))
        win = secs[k][:, cx:cx + 256]
        a = pf - pf.mean(); b = win - win.mean()
        tot += (a * b).sum() / np.sqrt((a * a).sum() * (b * b).sum())
    return tot / len(good)

cxv0 = cal0['ox'] + cal0['sx'] * 128
cyv0 = cal0['oy'] + cal0['sy'] * 136
best = None
for sx in np.arange(cal0['sx'] - 0.02, cal0['sx'] + 0.0201, 0.0025):
    for cxv in np.arange(cxv0 - 3, cxv0 + 3.01, 0.5):
        c = dict(cal0, sx=sx, ox=cxv - sx * 128)
        s = score(c)
        if best is None or s > best[0]:
            best = (s, sx, cxv)
print('best horiz: score %.4f sx %.4f cxv %.2f -> ox %.2f' % (best[0], best[1], best[2], best[2] - best[1] * 128))
sxb, oxb = best[1], best[2] - best[1] * 128
best = None
for sy in np.arange(cal0['sy'] - 0.03, cal0['sy'] + 0.0301, 0.0025):
    for cyv in np.arange(cyv0 - 3, cyv0 + 3.01, 0.5):
        c = dict(cal0, sx=sxb, ox=oxb, sy=sy, oy=cyv - sy * 136)
        s = score(c)
        if best is None or s > best[0]:
            best = (s, sy, cyv)
syb, oyb = best[1], best[2] - best[1] * 136
print('best vert : score %.4f sy %.4f cyv %.2f -> oy %.2f' % (best[0], syb, best[2], oyb))
calb = dict(sx=round(sxb, 4), sy=round(syb, 4), ox=round(oxb, 2), oy=round(oyb, 2))
print('FINAL', calb, 'ratio sx/sy %.4f' % (sxb / syb))
for f, im in zip(files, imgs):
    print(f, '->', locate(im, calb))
