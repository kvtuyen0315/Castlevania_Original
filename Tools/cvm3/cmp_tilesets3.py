# cmp_tilesets3.py - CVM3 01: doi chieu tile ban do Level 3 (level3_walkway.png, 5 vung) voi cot "LEVEL3: CRUMBLING TOWER" cua tilesets.png
# THEO HINH (nhan mau theo thu tu xuat hien dau) va THEO MAU TUYET DOI (tile giong het ca mau). Khuon = Tools/cvm2/cmp_tilesets.py.
# Chay tu goc repo: python -P -E Tools/cvm3/cmp_tilesets3.py [thu_muc_ra]  -> cmp_tilesets3.json + tilesets_xcheck.png
import json, os, sys
import numpy as np
from PIL import Image, ImageDraw
REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(REPO, 'ClaudeNotes', 'cvm3', '_cache', '01')
os.makedirs(OUT, exist_ok=True)
TS = os.path.join(REPO, 'Art_Source', 'spriters-resource', 'tilesets.png')
MP = os.path.join(REPO, 'Art_Source', 'spriters-resource', 'level3_walkway.png')
# Luoi cot Level 3 trong tilesets.png: o 32x32, buoc 33, goc (808, 92), 5 cot x 8 hang
GX, GY, STEP, CELL, NC, NR = 808, 92, 33, 32, 5, 8
REG = [('WalkwayEntranceA', 1, 425, 768), ('WalkwayEntranceB', 1, 218, 768), ('WalkwayA', 774, 218, 768),
       ('WalkwayB', 1286, 11, 768), ('MummyMan', 2059, 11, 1536)]
SKY = (36, 24, 140)

def canon(t):
    m = {}; lab = []
    for c in map(tuple, t.reshape(-1, 3)):
        if c not in m: m[c] = len(m)
        lab.append(m[c])
    return bytes(lab)

ts = np.asarray(Image.open(TS).convert('RGB'))
mp = np.asarray(Image.open(MP).convert('RGB'))
cells = [ts[GY + STEP * j:GY + STEP * j + CELL, GX + STEP * i:GX + STEP * i + CELL] for j in range(NR) for i in range(NC)]
used = [len(set(map(tuple, c.reshape(-1, 3)))) > 1 for c in cells]
tsc = {}  # mau tuyet doi cua Tilesets (de biet ban Mesen hay khac)
for c in cells:
    for col in map(tuple, c.reshape(-1, 3)): tsc[col] = tsc.get(col, 0) + 1
mpc = {}
for _, x0, y0, w in REG:
    for col in map(tuple, mp[y0:y0 + 192, x0:x0 + w].reshape(-1, 3)): mpc[col] = mpc.get(col, 0) + 1
res = dict(grid=dict(origin=[GX, GY], step=STEP, cell=CELL, cols=NC, rows=NR, used_cells=sum(used)),
           colors_map=len(mpc), colors_tilesets_col=len(tsc), colors_shared=len([c for c in mpc if c in tsc]),
           colors_map_only=[list(map(int, c)) for c in mpc if c not in tsc], sky_in_map=SKY in mpc, sky_in_tilesets=SKY in tsc)
print('colors', {k: v for k, v in res.items() if k.startswith('colors') or k.startswith('sky')})
keep = {}
for size in (8, 16):
    mt = {}; mt_abs = {}
    for name, x0, y0, w in REG:
        reg = mp[y0:y0 + 192, x0:x0 + w]
        for ty in range(0, 192, size):
            for tx in range(0, w, size):
                t = reg[ty:ty + size, tx:tx + size]
                mt.setdefault(canon(t), []).append((name, tx // size, ty // size)); mt_abs.setdefault(t.tobytes(), []).append((name, tx // size, ty // size))
    tt = {}; tt_abs = {}
    for n, c in enumerate(cells):
        if not used[n]: continue
        for qy in range(0, CELL, size):
            for qx in range(0, CELL, size):
                t = c[qy:qy + size, qx:qx + size]
                tt.setdefault(canon(t), []).append((n, qx // size, qy // size)); tt_abs.setdefault(t.tobytes(), []).append((n, qx // size, qy // size))
    both = [k for k in mt if k in tt]; both_abs = [k for k in mt_abs if k in tt_abs]
    res[size] = dict(map_unique_shape=len(mt), map_unique_color=len(mt_abs), ts_unique_shape=len(tt), ts_unique_color=len(tt_abs),
                     shared_shape=len(both), shared_color=len(both_abs), map_only_shape=len(mt) - len(both), ts_only_shape=len(tt) - len(both),
                     map_only_where=[mt[k][0] for k in mt if k not in tt], ts_only_where=[tt[k][0] for k in tt if k not in mt])
    # tile mau lech: cung hinh nhung khac mau tuyet doi
    shape_ok_color_bad = 0
    for k in both:
        if not any(a in tt_abs for a, locs in mt_abs.items() if canon(np.frombuffer(a, np.uint8).reshape(size, size, 3)) == k): shape_ok_color_bad += 1
    res[size]['shared_shape_but_color_differs'] = shape_ok_color_bad
    keep[size] = (mt, tt)
    print(size, {k: v for k, v in res[size].items() if not k.endswith('where')})
    print('  map_only_where', res[size]['map_only_where'])
json.dump(res, open(os.path.join(OUT, 'cmp_tilesets3.json'), 'w'), indent=1)

# Anh: tren = cot Tilesets Level 3 x3; duoi = o chi-ban-do 16x16 (x6) canh o tuong tu nhat trong Tilesets neu co
mt, tt = keep[16]
only = [(k, mt[k][0]) for k in mt if k not in tt]
Z = 3
W = CELL * NC * Z + 8
img = Image.new('RGB', (max(W, 40 * 6 * 3), CELL * NR * Z + 30 + 40 * Z * ((len(only) + 5) // 6) + 20), (40, 40, 40))
col = Image.fromarray(np.vstack([np.hstack(cells[j * NC:(j + 1) * NC]) for j in range(NR)]))
img.paste(col.resize((col.width * Z, col.height * Z), Image.NEAREST), (0, 0))
d = ImageDraw.Draw(img); y0 = CELL * NR * Z + 6
d.text((0, y0), 'Level3 map-only 16x16 (by shape) n=%d' % len(only), fill=(255, 255, 0)); y0 += 14
for i, (k, loc) in enumerate(only):
    name, tx, ty = loc; reg = [r for r in REG if r[0] == name][0]
    t = mp[reg[2] + ty * 16:reg[2] + ty * 16 + 16, reg[1] + tx * 16:reg[1] + tx * 16 + 16]
    im = Image.fromarray(t).resize((16 * Z * 2, 16 * Z * 2), Image.NEAREST)
    img.paste(im, ((i % 6) * 110, y0 + (i // 6) * 100))
    d.text(((i % 6) * 110, y0 + (i // 6) * 100 + 98 - 12), '%s %d,%d' % (name[:9], tx, ty), fill=(255, 255, 255))
img.save(os.path.join(OUT, 'tilesets_xcheck.png'))
