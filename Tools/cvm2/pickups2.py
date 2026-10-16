# pickups2.py (CVM2 02) - mỗi sự kiện HUD nhặt/giết (SCORE/tim/vũ khí) -> vết vật thể kết thúc ngay quanh Simon: nơi/thời điểm xuất hiện, vị trí lúc nhặt
# Chạy: python -P -E Tools/cvm2/pickups2.py map2 -> work/map2_pickups.txt   (dùng map2_events_raw.txt, tracks2, classify2, simon2)
import sys, os, re, csv
import numpy as np
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK, load_camera
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'
T = list(csv.DictReader(open(os.path.join(WORK, video + '_tracks.csv'))))
C = {int(r['id']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_track_class.csv')))}
P = {}
for r in csv.DictReader(open(os.path.join(WORK, video + '_track_pts.csv'))): P.setdefault(int(r['id']), []).append(r)
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(WORK, video + '_simon.csv')))}
cam = load_camera(video)
JOINT = ('HallA>TEB', 'HallC>HallB')
def ts(f): t = f / 30; return '%d:%05.2f' % (t // 60, t % 60)
def simon_w(f):
    for g in (f, f - 2, f + 2, f - 5, f + 5, f - 9, f + 9):
        s = S.get(g)
        if s and s['found'] == '1':
            c = cam[g]; b = int(c['cxw']) if c['section'] in JOINT else int(c['cx'])
            return b + int(s['px']), int(s['Y']), c['section']
    return None
out = []
for l in open(os.path.join(WORK, video + '_events_raw.txt'), encoding='utf-8'):
    m = re.match(r'f\s*(\d+) (\S+) TIME (\S+) \| (.*)', l)
    if not m: continue
    f = int(m.group(1)); ch = m.group(4)
    if f > 7100: continue
    sp = simon_w(f)
    best = []
    for t in T:
        f1 = int(t['f1']); f0 = int(t['f0'])
        if not (f - 6 <= f1 <= f + 3) or int(t['nfr']) < 3 or t['static'] == '1': continue
        if sp is None or t['section'] != sp[2]: continue
        wx, wy = float(t['wx1']), float(t['wy1'])
        d = np.hypot(wx - sp[0], (wy - (sp[1] - 14)) * 0.6)
        if d <= 28: best.append((d, t))
    best.sort(key=lambda x: x[0])
    s = 'f%d %s TIME %s | %s | Simon %s' % (f, ts(f), m.group(3), ch, ('%s wx %d Y %d' % (sp[2], sp[0], sp[1])) if sp else '-')
    for d, t in best[:2]:
        c = C.get(int(t['id']), {})
        s += ' || track #%s %s f%s-%s (%s-%s) (%.0f,%.0f)->(%.0f,%.0f) %sx%s %s %s' % (t['id'], t['section'][:6], t['f0'], t['f1'], ts(int(t['f0'])), ts(int(t['f1'])), float(t['wx0']), float(t['wy0']), float(t['wx1']), float(t['wy1']), t['w'], t['h'], c.get('l1', '?'), c.get('s1', ''))
    out.append(s)
open(os.path.join(WORK, video + '_pickups.txt'), 'w', encoding='utf-8').write('\n'.join(out))
print('\n'.join(out))
