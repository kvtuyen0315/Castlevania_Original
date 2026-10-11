# pf_sheets.py - contact sheet playfield (NES y 48..223) moi 0.5 s, nhan: thoi diem, frame, vung+cx, TIME, so tim
# Chay: python -P -E tools/pf_sheets.py map1 <t0> <t1> <step_s> <out_prefix>
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
video, t0, t1, step, pre = sys.argv[1], float(sys.argv[2]), float(sys.argv[3]), float(sys.argv[4]), sys.argv[5]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
mm = np.load(os.path.join(base, f'{video}_nes.npy'), mmap_mode='r')
cp = os.path.join(base, f'{video}_camera.csv')
cam = {int(r['frame']): r for r in csv.DictReader(open(cp))} if os.path.exists(cp) else {}
hud = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_hud.csv')))}
AB = {'CastleEntrance': 'CE', 'EntranceHall': 'EH', 'CryptA': 'CA', 'CryptB': 'CB', 'PhantomBat': 'PB', '-': '--'}
Y0 = 8; ya, yb = 48, 224
if video == 'boss': ya, yb = 16, 226   # gom ca HUD (vach mau) cho video boss
cols, rows = 8, 6
ts = np.arange(t0, t1 + 1e-6, step)
per = cols * rows
for s in range(0, len(ts), per):
    chunk = ts[s:s + per]
    th = yb - ya
    img = Image.new('RGB', (cols * 258, rows * (th + 12)), (60, 60, 60))
    d = ImageDraw.Draw(img)
    for k, t in enumerate(chunk):
        i = int(round(t * 30))
        if i >= mm.shape[0]: break
        a = np.asarray(mm[i][ya - Y0:yb - Y0])
        x = (k % cols) * 258; y = (k // cols) * (th + 12)
        img.paste(Image.fromarray(a), (x, y + 12))
        c = cam.get(i, {}); h = hud.get(i, {})
        lab = f"{int(t//60)}:{t%60:04.1f} f{i} {AB.get(c.get('section','-'),'?')}{c.get('cx','')} T{h.get('time','')} h{h.get('hearts','')}"
        d.text((x + 1, y), lab, fill=(255, 255, 0))
    out = f'{pre}_{int(chunk[0]):03d}.png'
    img.save(out); print('saved', out)
