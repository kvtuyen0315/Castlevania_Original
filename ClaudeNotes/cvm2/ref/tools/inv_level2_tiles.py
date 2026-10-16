# inv_level2_tiles.py - kiem ke sheet Level 2: vung, tile 8x8 / metatile 16x16 duy nhat, mau, o teal (vung bay ep) => inventory/level2_main_hall.json + anh chu thich
# Chay tu ClaudeNotes/cvm2: python -P -E _cache/tools/inv_level2_tiles.py
import json, hashlib
import numpy as np
from PIL import Image, ImageDraw
SHEET = r'C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 2_ Main Hall.png'
raw = open(SHEET, 'rb').read()
im = Image.open(SHEET).convert('RGB'); a = np.asarray(im)
REG = [  # ten, stage, x, y, w, h, ghi chu
    ('TowerEntranceA', 4, 1803, 632, 256, 192, 'Stage 04 dau (video 0:06) - cua vao ben trai, thang len o phai'),
    ('TowerEntranceB', 4, 1547, 425, 512, 192, 'Stage 04 tang tren (video 0:29) - be di dong + cua TRAI cuoi stage (0:52)'),
    ('TowerHallA', 5, 774, 425, 768, 192, 'Stage 05 tang duoi (video 0:58, vao tu phai cx 512, di trai) - thang len trai'),
    ('TowerHallB', 5, 774, 218, 768, 192, 'Stage 05 tang tren (video 1:33, cx 0 -> 512 -> 0) - be di dong, cua TRAI (2:22)'),
    ('TowerHallC', 6, 1, 218, 768, 192, 'Stage 06 (video 2:28, vao tu phai) - 3 o teal = vung bay ep gai, thang trai len tang tren, thang phai len sanh Medusa'),
    ('Medusa', 6, 1, 11, 768, 192, 'Stage 06 sanh + phong boss (video 3:17 vao tu phai-duoi, khoa cx 0 o phong boss); tuong Medusa ve SAN tren nen'),
]
SPR = [('Crusher', [[1 + 33 * k, 425, 32, 64] for k in range(8)], '8 khung bay ep gai (keo dai dan), nen den'),
       ('MovingPlatform', [[269, 425, 32, 16]], 'be di dong 4 khoi')]
out = dict(sheet='NES - Castlevania - Stages - Level 2_ Main Hall.png', size=list(im.size), bytes=len(raw),
           sha256=hashlib.sha256(raw).hexdigest(), background_key=[0, 128, 0], credit='Sprites ripped by Mister Mike. Give credit if used. (logo [1576,792,224,32])',
           row_to_nes_y='dong r cua vung <-> NES y = 32 + r (kiem bang ZNCC video Map_2: TEA 0:12 0.934 voi cal planner)', regions=[], sprites=[])
all8 = {}; all16 = {}
for name, st, x, y, w, h, note in REG:
    r = a[y:y + h, x:x + w]
    t8 = {}; t16 = {}
    for ty in range(0, h, 8):
        for tx in range(0, w, 8):
            k = r[ty:ty + 8, tx:tx + 8].tobytes(); t8[k] = 1; all8[k] = 1
    for ty in range(0, h, 16):
        for tx in range(0, w, 16):
            k = r[ty:ty + 16, tx:tx + 16].tobytes(); t16[k] = 1; all16[k] = 1
    cols = {}
    for c in map(tuple, r.reshape(-1, 3)):
        cols[c] = cols.get(c, 0) + 1
    teal = int(((r[..., 0] == 0) & (r[..., 1] == 128) & (r[..., 2] == 128)).sum())
    out['regions'].append(dict(name=name, stage=st, rect=[x, y, w, h], screens=w // 256, cx_max=w - 256, unique8=len(t8), unique16=len(t16),
                               colors=len(cols), teal_px=teal, note=note))
out['total_unique8'] = len(all8); out['total_unique16'] = len(all16)
for name, rects, note in SPR:
    out['sprites'].append(dict(name=name, rects=rects, note=note))
json.dump(out, open('inventory/level2_main_hall.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
ann = im.copy(); d = ImageDraw.Draw(ann)
for rg in out['regions']:
    x, y, w, h = rg['rect']; d.rectangle((x - 1, y - 1, x + w, y + h), outline=(255, 255, 0))
    d.text((x + 2, y + 2), '%s St%d %dx%d' % (rg['name'], rg['stage'], w, h), fill=(255, 255, 0))
    for gx in range(x, x + w, 256):
        d.line((gx, y, gx, y + h), fill=(0, 255, 255))
for sp in out['sprites']:
    for (x, y, w, h) in sp['rects']:
        d.rectangle((x - 1, y - 1, x + w, y + h), outline=(255, 0, 255))
ann.save('inventory/level2_main_hall_annot.png')
for rg in out['regions']:
    print(rg['name'], rg['rect'], 'u8', rg['unique8'], 'u16', rg['unique16'], 'colors', rg['colors'], 'teal', rg['teal_px'])
print('total unique8', out['total_unique8'], 'unique16', out['total_unique16'])
