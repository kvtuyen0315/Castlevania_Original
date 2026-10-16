# cmp_tilesets.py - CVM2 01: doi chieu tile ban do Level 2 (level2_main_hall.png) voi cot "LEVEL2: MAIN HALL" cua tilesets.png THEO HINH
# (anh xa mau 1-1: gan nhan theo thu tu xuat hien dau tien; khong so mau tuyet doi vi bang mau emulator khac ban do).
# Chay tu goc repo: python -P -E Tools/cvm2/cmp_tilesets.py [thu_muc_ra]  -> cmp_tilesets.json + cmp_tilesets_sheet.png
import json, os, sys
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes2 import REPO, SHEET, SECTIONS
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(REPO, 'ClaudeNotes', 'cvm2', '_cache', '01')
os.makedirs(OUT, exist_ok=True)
TS = os.path.join(REPO, 'Art_Source', 'spriters-resource', 'tilesets.png')
# Luoi cot Level 2 trong tilesets.png: o 32x32, buoc 33 (duong luoi 1 px mau nen 127,178,187), goc (408, 92), 5 cot x 8 hang
GX, GY, STEP, CELL, NC, NR = 408, 92, 33, 32, 5, 8

def canon(t):  # nhan mau theo thu tu xuat hien dau -> bytes
    flat = t.reshape(-1, 3); m = {}; lab = []
    for c in map(tuple, flat):
        if c not in m: m[c] = len(m)
        lab.append(m[c])
    return bytes(lab)

ts = np.asarray(Image.open(TS).convert('RGB'))
mp = np.asarray(Image.open(SHEET).convert('RGB'))
cells = []
for j in range(NR):
    for i in range(NC):
        cells.append(ts[GY + STEP * j:GY + STEP * j + CELL, GX + STEP * i:GX + STEP * i + CELL])
# o trong (1 mau) bo qua
cell_used = [len(set(map(tuple, c.reshape(-1, 3)))) > 1 for c in cells]

res = {}
for size in (8, 16):
    # tile ban do duy nhat (canon) + noi xuat hien
    mt = {}
    for name, (x0, y0, w) in SECTIONS.items():
        if name == 'TowerEntranceA': pass
        reg = mp[y0:y0 + 192, x0:x0 + w]
        for ty in range(0, 192, size):
            for tx in range(0, w, size):
                k = canon(reg[ty:ty + size, tx:tx + size])
                mt.setdefault(k, []).append((name, tx // size, ty // size))
    ts_t = {}
    for n, c in enumerate(cells):
        if not cell_used[n]: continue
        for qy in range(0, CELL, size):
            for qx in range(0, CELL, size):
                ts_t.setdefault(canon(c[qy:qy + size, qx:qx + size]), []).append((n, qx // size, qy // size))
    both = [k for k in mt if k in ts_t]
    res[size] = dict(map_unique=len(mt), ts_unique=len(ts_t), both=len(both), map_only=len(mt) - len(both), ts_only=len(ts_t) - len(both),
                     map_only_where=[mt[k][0] for k in mt if k not in ts_t],
                     ts_only_where=[ts_t[k][0] for k in ts_t if k not in mt])
    print(size, {k: v for k, v in res[size].items() if not k.endswith('where')})
json.dump(res, open(os.path.join(OUT, 'cmp_tilesets.json'), 'w'), indent=1)
