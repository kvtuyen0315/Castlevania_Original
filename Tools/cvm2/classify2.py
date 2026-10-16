# classify2.py (CVM2 02) - gán nhãn sprite cho từng vết (tracks2.py) bằng IoU mặt nạ giữa blob tiền cảnh và alpha các sprite trong Assets/Data/Art
# Chạy: python -P -E Tools/cvm2/classify2.py map2   -> work/map2_track_class.csv (id,label1,iou1,label2,iou2,label3,iou3)
# Mỗi vết lấy ≤ 7 frame mẫu (đều theo thời gian), mỗi frame so mọi sprite có kích thước bbox sai ≤ 6 px (2 hướng, dịch ±2); điểm = IoU TB các mẫu.
import sys, os, csv, glob
import numpy as np
from PIL import Image
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from fg2 import WORK, ROOT, fg_mask, load_camera
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'
ART = os.path.join(ROOT, 'Assets', 'Data', 'Art')
DIRS = ['Enemy', 'Item', 'Object', 'SubWeapon', 'FX', os.path.join('Boss', 'Medusa'), os.path.join('Character', 'Simon')]
bank = []
for d in DIRS:
    for p in glob.glob(os.path.join(ART, d, '**', '*.png'), recursive=True):
        a = np.asarray(Image.open(p).convert('RGBA'))[..., 3] > 0
        ys, xs = np.nonzero(a)
        if len(ys) < 6: continue
        m = a[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
        name = os.path.relpath(p, ART).replace(os.sep, '/')[:-4]
        if name.startswith('Character/Simon'): name = 'Simon/' + name.split('/')[-1]
        bank.append((name, m)); bank.append((name + '~', m[:, ::-1]))
print('sprites', len(bank) // 2)
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
cam = load_camera(video)
pts = {}
for r in csv.DictReader(open(os.path.join(WORK, video + '_track_pts.csv'))):
    pts.setdefault(int(r['id']), []).append(r)
T = list(csv.DictReader(open(os.path.join(WORK, video + '_tracks.csv'))))
def iou_best(blob, bw, bh):
    res = {}
    for name, m in bank:
        h, w = m.shape
        if abs(h - bh) > 6 or abs(w - bw) > 6: continue
        best = 0.0
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                # đặt tâm sprite lệch (dx,dy) so với tâm blob trên canvas lớn
                H = max(bh, h) + 6; W = max(bw, w) + 6
                A = np.zeros((H, W), bool); Bm = np.zeros((H, W), bool)
                oy = (H - bh) // 2; ox = (W - bw) // 2
                A[oy:oy + bh, ox:ox + bw] = blob
                sy = (H - h) // 2 + dy; sx = (W - w) // 2 + dx
                if sy < 0 or sx < 0 or sy + h > H or sx + w > W: continue
                Bm[sy:sy + h, sx:sx + w] = m
                i = (A & Bm).sum(); u = (A | Bm).sum()
                if u and i / u > best: best = i / u
        base = name.rstrip('~')
        if best > res.get(base, 0): res[base] = best
    return res
out = []
for t in T:
    tid = int(t['id']); P = pts.get(tid, [])
    if len(P) < 3: continue
    idx = np.unique(np.linspace(0, len(P) - 1, 7).astype(int))
    acc = {}
    for k in idx:
        q = P[k]; f = int(q['f']); c = cam[f]
        fr = np.asarray(mm[f])
        m = fg_mask(fr, c['section'], int(c['cx']), int(c['cxw']))
        x0, y0, x1, y1 = int(q['x0']), int(q['y0']) - 48, int(q['x1']), int(q['y1']) - 48
        blob = m[y0:y1 + 1, x0:x1 + 1]
        if blob.sum() < 4: continue
        for nm, v in iou_best(blob, x1 - x0 + 1, y1 - y0 + 1).items(): acc.setdefault(nm, []).append(v)
    n = len(idx)
    sc = sorted(((np.sum(v) / n, nm) for nm, v in acc.items()), reverse=True)[:3]
    row = [tid] + sum([[nm, '%.2f' % s] for s, nm in sc], [])
    out.append(row + [''] * (7 - len(row)))
with open(os.path.join(WORK, video + '_track_class.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['id', 'l1', 's1', 'l2', 's2', 'l3', 's3']); w.writerows(out)
print('done', len(out))
