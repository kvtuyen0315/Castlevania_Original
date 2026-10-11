# overlay_scan.py - phat hien lop phu YouTube (thanh dieu khien, nut play, tieu de) chong len khung game
# Y tuong: NES y 8..15 va 224..231 luon DEN trong Castlevania (tren HUD / duoi san) -> neu co pixel sang o
# 2 dai nay (video y 12..42 va 908..941 voi map1) thi la lop phu YouTube. Them: tam man (nut play) do rieng.
# Chay: python -P -E tools/overlay_scan.py map1   -> work/map1_overlay.csv (frame,top_bright,bot_bright)
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import read_frames, CALIB, VID
video = sys.argv[1]
cal = CALIB[video]; path, W, H = VID[video]
def vy(n): return int(round(cal['oy'] + cal['sy'] * n))
def vx(n): return int(round(cal['ox'] + cal['sx'] * n))
top = (vy(8) + 2, vy(16) - 2); bot = (vy(224) + 2, vy(232) - 2)
xl, xr = vx(0) + 2, vx(256) - 2
xr_bot = vx(236)   # bo goc phai duoi: logo do (watermark kenh) luon hien o NES x~240..252, y~224..231
print('dai tren video y', top, 'dai duoi', bot, 'x', xl, xr)
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
rows = []
for i, fr in enumerate(read_frames(video)):
    t = fr[top[0]:top[1], xl:xr].max(axis=2); b = fr[bot[0]:bot[1], xl:xr_bot].max(axis=2)
    rows.append((i, int((t > 60).sum()), int((b > 60).sum())))
with open(os.path.join(base, f'{video}_overlay.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 'top_bright', 'bot_bright']); w.writerows(rows)
# tom tat cac doan co lop phu
def ts(i): t = i / 30; return '%d:%05.2f' % (t // 60, t % 60)
ntop = (top[1] - top[0]) * (xr - xl)
# dai tren sang > 60% = man nhay trang (Rosary/flash) chu khong phai lop phu
on = [(30 < r[1] < 0.6 * ntop) or r[2] > 30 for r in rows]
start = None
for i, v in enumerate(on + [False]):
    if v and start is None: start = i
    if not v and start is not None:
        print(f'lop phu: f{start}-f{i-1}  {ts(start)}-{ts(i-1)}  ({i-start} frame)  top max {max(r[1] for r in rows[start:i])} bot max {max(r[2] for r in rows[start:i])}')
        start = None
