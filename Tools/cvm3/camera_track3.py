# camera_track3.py (CVM3 02) - vùng bản đồ + camera x (toạ độ pixel NES trong vùng) cho TỪNG frame Map_3 / Boss_Map_3
# Chạy: python -P -E Tools/cvm3/camera_track3.py map3     -> _cache/02/work/map3_camera.csv (frame,t,kind,section,cx,score,cxw)
# kind: game | black | other (HUD có nhưng không khớp vùng nào: bản đồ lâu đài / nháy) ; section có thể là 'HallA>TEB' hoặc 'HallC>HallB'
#   = đoạn camera đang cuộn qua cửa TRÁI (vùng ghép: bản đồ trái ‖ bản đồ phải, cxw = camera trong toạ độ ghép).
# Cách: playfield NES y 48..223 (xám) ZNCC với từng vùng sheet; mỗi 15 frame tìm toàn bộ, frame giữa tìm cục bộ cx±12; score<0.6 → tìm toàn bộ.
import sys, os, csv
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import *
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
video = sys.argv[1]
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
N = mm.shape[0]
Y0 = 8
sheet = load_sheet()
S = {k: gray(padded_section(sheet, k))[16:192] for k in SECTIONS}   # WalkwayB có PAD cột trời bên trái: chỉ số = cx + PAD
S0 = {k: gray(section(sheet, k))[16:192] for k in SECTIONS}
# vùng ghép cho 2 cửa PHẢI (bản đồ trái ‖ bản đồ phải); cxw ∈ [512, 768] (CVM3: cửa phải #1 WEB>WA, cửa phải #2 WB>MM)
JOINT = {'WEB>WA': np.concatenate([S0['WalkwayEntranceB'], S0['WalkwayA']], axis=1),
         'WB>MM': np.concatenate([S0['WalkwayB'], S0['MummyMan']], axis=1)}
JOINT_END = {'WEB>WA': ('WalkwayEntranceB', 'WalkwayA'), 'WB>MM': ('WalkwayB', 'MummyMan')}
ALL = dict(S)
ALL.update(JOINT)
if video == 'boss3': ALL = {'MummyMan': S['MummyMan']}

def zn_many(pf, sg, cxs, sub=1):
    P = pf[::sub]; a = P - P.mean(); na = np.sqrt((a * a).sum()) + 1e-6
    res = []
    for cx in cxs:
        b = sg[::sub, cx:cx + 256]; b = b - b.mean()
        res.append(float((a * b).sum() / (na * (np.sqrt((b * b).sum()) + 1e-6))))
    return np.array(res)

def rng(k):
    W = ALL[k].shape[1]
    if k in JOINT: return range(512, 768 + 1)
    return range(0, W - 256 + 1)

def full(pf):
    best = (-2, None, None)
    for k, sg in ALL.items():
        cxs = list(rng(k))
        c4 = cxs[::4]
        sc = zn_many(pf, sg, c4, 2)
        c0 = c4[int(np.argmax(sc))]
        fine = [c for c in range(c0 - 4, c0 + 5) if c in rng(k)]
        sf = zn_many(pf, sg, fine, 1); j = int(np.argmax(sf))
        if sf[j] > best[0]: best = (float(sf[j]), k, fine[j])
    return best

def local(pf, k, cx0, span=12):
    cxs = [c for c in range(cx0 - span, cx0 + span + 1) if c in rng(k)]
    if not cxs: return (-2, k, cx0)
    sf = zn_many(pf, ALL[k], cxs, 1); j = int(np.argmax(sf))
    return (float(sf[j]), k, cxs[j])

out = []
cur = None
for i in range(N):
    fr = np.asarray(mm[i]).astype(float)
    pf = gray(fr[48 - Y0:224 - Y0])
    lum = float(fr.mean())
    if lum < 2.0 or pf.std() < 3.0 and fr[8:40].std() < 3:   # đen hoàn toàn
        out.append((i, 'black', '-', -1, 0.0, -1)); cur = None; continue
    if cur is None or i % 15 == 0:
        sc, k, cx = full(pf)
    else:
        sc, k, cx = local(pf, *cur)
        if sc < 0.6: sc, k, cx = full(pf)
    if sc < 0.45:
        out.append((i, 'other', '-', -1, sc, -1)); cur = None; continue
    cur = (k, cx)
    cxw = cx
    sec = k
    if k in JOINT:
        if cx == 768: sec, cx = JOINT_END[k][1], 0
        elif cx == 512: sec, cx = JOINT_END[k][0], 512
        else: sec = k
    else:
        cx = cx - PAD.get(k, 0); cxw = cx
    out.append((i, 'game', sec, cx, sc, cxw))
    if i % 500 == 0: print(out[-1], flush=True)
with open(os.path.join(WORK, video + '_camera.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'kind', 'section', 'cx', 'score', 'cxw'])
    for i, kind, sec, cx, sc, cxw in out:
        w.writerow([i, '%.3f' % (i / 30), kind, sec, cx, '%.3f' % sc, cxw])
print('done')
