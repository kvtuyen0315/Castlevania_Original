# calib_hud.py - hieu chuan toa do video -> pixel NES tu HUD Castlevania
# Chay:  python -P -E tools/calib_hud.py <frame.png> [x_lo x_hi y_lo y_hi]
#   frame.png: 1 frame full-size co HUD day du (vd ffmpeg -ss 30 -i Map_1.mp4 -frames:v 1 f.png)
#   x_lo..y_hi: (tuy chon) vung tim HUD trong khung video, mac dinh = ca anh
# Cach do:
#   - Ngang: 16 vach mau PLAYER (do) va ENEMY (cam): vach i co tam NES x = 56 + 4i + 1.5
#     -> hoi quy tuyen tinh x_v = ox + sx * x_nes  (32 diem)
#   - Doc : 3 hang chu SCORE/PLAYER/ENEMY (glyph 7 dong, dinh o NES y 16/24/32,
#     day o 23/31/39) -> hoi quy y_v = oy + sy * y_nes (6 diem: dinh + day)
# In ra sx, sy, ox, oy, residual.
import sys
import numpy as np
from PIL import Image

path = sys.argv[1]
im = np.asarray(Image.open(path).convert('RGB')).astype(int)
H, W, _ = im.shape
if len(sys.argv) >= 6:
    xl, xh, yl, yh = map(int, sys.argv[2:6])
else:
    xl, xh, yl, yh = 0, int(W * 0.49), 0, int(H * 0.17)   # nua trai dai HUD (vach mau o NES x 56..119 < 128)
sub = im[yl:yh, xl:xh]
r, g, b = sub[..., 0], sub[..., 1], sub[..., 2]
red = (r > 120) & (g < 70) & (b < 70)                 # PLAYER bar (~208,13,4)
orange = (r > 180) & (g > 70) & (g < 160) & (b < 110)  # ENEMY bar (~241,108,56)
white = sub.min(axis=2) > 128

def runs_1d(mask):
    out = []; inr = False
    for i, v in enumerate(mask):
        if v and not inr: s = i; inr = True
        if not v and inr: out.append((s, i - 1)); inr = False
    if inr: out.append((s, len(mask) - 1))
    return out

def find_bar(mask, name):
    # row with the most "on" pixels in exactly 16 runs
    best = None
    rows = np.where(mask.sum(axis=1) > 0)[0]
    if len(rows) == 0:
        print(name, 'not found'); return None
    cnt = mask.sum(axis=1)
    ymid = int(np.argmax(cnt))
    good = np.where(cnt > 0.6 * cnt[ymid])[0]
    good = good[np.abs(good - ymid) < 40]
    ylo, yhi = good.min(), good.max()
    rr = runs_1d(mask[ymid])
    rr = [q for q in rr if q[1] - q[0] >= 3]
    # subpixel edges: use column coverage over bar rows
    cov = mask[ylo:yhi + 1].mean(axis=0)
    return dict(ylo=ylo, yhi=yhi, runs=rr, cov=cov, mask=mask)

pts_x = []
for mask, name, nes_y0 in ((red, 'PLAYER', 25), (orange, 'ENEMY', 33)):
    fb = find_bar(mask, name)
    if fb is None: continue
    rr = fb['runs']
    print(f'{name} bar: rows {fb["ylo"]+yl}..{fb["yhi"]+yl}, {len(rr)} runs')
    if len(rr) != 16:
        print('  runs', [(a + xl, b_ + xl) for a, b_ in rr]); continue
    # vertical extent (50% threshold on a center column of each bar)
    for i, (a, b_) in enumerate(rr):
        cx = (a + b_) / 2.0 + xl
        pts_x.append((56 + 4 * i + 1.5, cx))
    # vertical edges via column at first bar centre
    c = (rr[0][0] + rr[0][1]) // 2
    colv = mask[:, c].astype(float)
    on = np.where(colv > 0)[0]
    print(f'  bar vertical on-rows: {on.min()+yl}..{on.max()+yl}  (height {on.max()-on.min()+1} px video)')

pts_x = np.array(pts_x)
A = np.vstack([pts_x[:, 0], np.ones(len(pts_x))]).T
(sx, ox), res, _, _ = np.linalg.lstsq(A, pts_x[:, 1], rcond=None)
resid = pts_x[:, 1] - (ox + sx * pts_x[:, 0])
print(f'HORIZ: sx={sx:.4f} ox={ox:.2f}  resid max={abs(resid).max():.2f}px video  rms={np.sqrt((resid**2).mean()):.2f}')

# text rows: profile of white pixels in left text region (letters SCORE/PLAYER/ENEMY)
# use region left of PLAYER bar: NES x 0..48
x1 = int(ox + sx * 48) - xl
prof = white[:, max(0, int(ox) - xl):x1].sum(axis=1)
rows = runs_1d(prof > 3)
rows = [q for q in rows if q[1] - q[0] > 10]
print('text row runs (video y):', [(a + yl, b_ + yl) for a, b_ in rows])
pts_y = []
for k, (a, b_) in enumerate(rows[:3]):
    top_nes = 16 + 8 * k
    pts_y.append((top_nes, a + yl - 0.0))          # top edge of first glyph row (pixel a starts the row)
    pts_y.append((top_nes + 7, b_ + yl + 1.0))     # bottom edge (after last row)
pts_y = np.array(pts_y)
A = np.vstack([pts_y[:, 0], np.ones(len(pts_y))]).T
(sy, oy), _, _, _ = np.linalg.lstsq(A, pts_y[:, 1], rcond=None)
resid = pts_y[:, 1] - (oy + sy * pts_y[:, 0])
print(f'VERT : sy={sy:.4f} oy={oy:.2f}  resid max={abs(resid).max():.2f}px video')
print(f'ratio sx/sy = {sx/sy:.4f}  (8/7={8/7:.4f}, 7/6 (4:3 of 256x224)={7/6:.4f})')
print(f'NES x at video x=0: {(0-ox)/sx:.2f} ; at video x={W}: {(W-ox)/sx:.2f}')
print(f'NES y at video y=0: {(0-oy)/sy:.2f} ; at video y={H}: {(H-oy)/sy:.2f}')
