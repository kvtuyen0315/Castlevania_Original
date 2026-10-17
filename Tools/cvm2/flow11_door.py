# CVM2 11 - trang thai canh cua trai tung frame (door_0/door_1 LAT NGANG tai x man 120, y = dinh cot + 1) + camera + Simon.
# Chay tu goc repo: python -P -E Tools/cvm2/flow11_door.py <f0> <f1> <y dinh canh NES: 65 cua #1 | 97 cua #2>  (can _cache/02/work/map2_nes.npy)
import os, sys, csv
import numpy as np
ROOT = os.getcwd()
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
import enemy8_fit as ef
mm = np.load(os.path.join(ef.WORK, 'map2_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(ef.WORK, 'map2_dup.npy'))
cam = ef.camera()
sim = {}
for r in csv.DictReader(open('ClaudeNotes/cvm2/ref/data/map2_simon.csv')):
    sim[int(r['frame'])] = (r['sprite'], r['world_x'], r['y_sec'], r['px'])
sprites, cols = ef.load(['Object/door_0', 'Object/door_1'])
pal = [(0, 0, 0), (190, 13, 4)]
def err_at(nes, s, x, y):
    acc = 0.0
    for k in range(len(s['ys'])):
        v = nes[y + s['ys'][k] - ef.Y0, x + s['xs'][k]].astype(float)
        acc += min(np.sqrt(((v - np.array(pal[s['cls'][k]], float)) ** 2).sum()) / 441.7, ef.CLIP)
    return acc / len(s['ys'])
d0 = [s for s in sprites if s['name'].endswith('door_0') and s['flip']][0]
d1 = [s for s in sprites if s['name'].endswith('door_1') and s['flip']][0]
f0, f1, ytop = int(sys.argv[1]), int(sys.argv[2]), int(sys.argv[3])
for f in range(f0, f1 + 1):
    nes = np.asarray(mm[f])
    e0, e1 = err_at(nes, d0, 120, ytop), err_at(nes, d1, 120, ytop)
    st = 'door_1' if e1 < 0.06 else 'door_0' if e0 < 0.05 else '-'
    k = cam.get(f, ('?', '?', -1, -1))
    print(f, 'dup' if dup[f] else '   ', k[3], st, '%.3f %.3f' % (e0, e1), sim.get(f, ('',))[0], sim.get(f, ('', '', ''))[1])
