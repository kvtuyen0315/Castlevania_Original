# inv_level3_tiles.py - kiem ke sheet Level 3: vung, tile 8x8 / metatile 16x16 duy nhat, mau, o la => inventory/level3_walkway.json + anh chu thich
# Chay tu ClaudeNotes/cvm3: python -P -E _cache/tools/inv_level3_tiles.py
import json, hashlib
import numpy as np
from PIL import Image, ImageDraw
SHEET = r'C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 3_ Crumbling Tower.png'
raw = open(SHEET, 'rb').read()
im = Image.open(SHEET).convert('RGB'); a = np.asarray(im)
REG = [  # ten, stage, x, y, w, h, ghi chu
    ('WalkwayEntranceA', 7, 1, 425, 768, 192,
     'Stage 07 tang duoi (video 0:06.8 vao cx 512 tu cua phai, di TRAI toi cx 0) - thang len-trai o goc tren-trai sang WalkwayEntranceB'),
    ('WalkwayEntranceB', 7, 1, 218, 768, 192,
     'Stage 07 tang tren (video 0:40.0 cx 0, di PHAI toi cx 512) - CUA PHAI cuoi stage (1:21.4)'),
    ('WalkwayA', 8, 774, 218, 768, 192,
     'Stage 08 tang duoi (video 1:28.0 cx 0, di PHAI) - cau co lan can + tuong; thang len-trai o phan phai sang WalkwayB (1:48.8)'),
    ('WalkwayB', 8, 1286, 11, 768, 192,
     'Stage 08 tang tren (video 1:49.2) - camera lui toi cx -109 (troi ben trai NGOAI vung sheet); CUA PHAI cuoi stage (2:24.6)'),
    ('MummyMan', 9, 2059, 11, 1536, 192,
     'Stage 09 (video 2:31.5 cx 0, di PHAI) - cau dai co doan hong (ho), phong boss Mummy Men o 256 px cuoi (khoa cx 1280, video 3:18)'),
]
LOGO = [3371, 585, 224, 32]
out = dict(sheet='NES - Castlevania - Stages - Level 3_ Crumbling Tower.png', size=list(im.size), bytes=len(raw),
           sha256=hashlib.sha256(raw).hexdigest(), background_key=[0, 128, 0],
           credit='Sprites ripped by Mister Mike. Give credit if used. (logo %s)' % LOGO,
           labels='nhan chu trang tren moi vung: "Walkway Entrance A/B", "Walkway A/B", "Mummy Man" (ten vung = ten section de xuat)',
           row_to_nes_y='dong r cua vung <-> NES y = 32 + r (kiem bang ZNCC video Map_3: 5 section 0.95-0.98 voi cal planner)', regions=[])
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
    key = int(((r[..., 0] == 0) & (r[..., 1] == 128) & (r[..., 2] == 0)).sum())
    top = sorted(cols.items(), key=lambda kv: -kv[1])[:8]
    out['regions'].append(dict(name=name, stage=st, rect=[x, y, w, h], screens=w // 256, cx_max=w - 256, unique8=len(t8), unique16=len(t16),
                               colors=len(cols), key_green_px=key, top_colors=[[list(map(int, c)), n] for c, n in top], note=note))
out['total_unique8'] = len(all8); out['total_unique16'] = len(all16)
json.dump(out, open('inventory/level3_walkway.json', 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
ann = im.copy(); d = ImageDraw.Draw(ann)
for rg in out['regions']:
    x, y, w, h = rg['rect']; d.rectangle((x - 1, y - 1, x + w, y + h), outline=(255, 255, 0))
    d.text((x + 120, y + 2), '%s St%d %dx%d' % (rg['name'], rg['stage'], w, h), fill=(255, 255, 0))
    for gx in range(x, x + w, 256):
        d.line((gx, y, gx, y + h), fill=(0, 255, 255))
x, y, w, h = LOGO; d.rectangle((x - 1, y - 1, x + w, y + h), outline=(255, 0, 255))
ann.save('inventory/level3_walkway_annot.png')
for rg in out['regions']:
    print(rg['name'], rg['rect'], 'u8', rg['unique8'], 'u16', rg['unique16'], 'colors', rg['colors'], 'key', rg['key_green_px'])
print('total unique8', out['total_unique8'], 'unique16', out['total_unique16'])
