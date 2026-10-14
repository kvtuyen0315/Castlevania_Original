# -*- coding: utf-8 -*-
"""CVGP 21 — ghep anh "phong trung bay" (CharacterAcceptProbe.RunGallery) thanh 1 tam/nhan vat de xem.

    python -P -E Tools/cvgp/gallery21.py <thu muc anh> <thu muc ra>

Anh Game view 1920x1080 (Android idx 6) => vung game 1024x960 @ (448,60) = 4x NES 256x240. Moi nhan vat: 3 hang
(idle / approach / attack) x 4 anh, thu nho ve 256x240 (lay mau 1 diem/o 4x4 => dung pixel NES), nhan ten + tick.
"""
import os, re, sys
from PIL import Image, ImageDraw

SRC, DST = sys.argv[1], sys.argv[2]
GX, GY, GW, GH = 448, 60, 1024, 960
MODES = ['idle', 'approach', 'attack']
os.makedirs(DST, exist_ok=True)

shots = {}
for fn in os.listdir(SRC):
    m = re.match(r'(.+)_(idle|approach|attack)_r(\d+)\.png$', fn)
    if m:
        shots.setdefault(m.group(1), {}).setdefault(m.group(2), []).append((int(m.group(3)), fn))

made = 0
for name, modes in sorted(shots.items()):
    W, H, LAB = 256, 240, 12
    sheet = Image.new('RGB', (4 * W + 3 * 4, 3 * (H + LAB) + 2 * 4), (40, 40, 40))
    d = ImageDraw.Draw(sheet)
    for mi, mode in enumerate(MODES):
        for k, (rel, fn) in enumerate(sorted(modes.get(mode, []))[:4]):
            im = Image.open(os.path.join(SRC, fn)).convert('RGB')
            if im.size != (1920, 1080):
                print('size?', fn, im.size)
            g = im.crop((GX, GY, GX + GW, GY + GH)).resize((W, H), Image.NEAREST)
            x, y = k * (W + 4), mi * (H + LAB + 4)
            sheet.paste(g, (x, y + LAB))
            d.text((x + 2, y), '%s %s t%d' % (name, mode, rel), fill=(255, 255, 0))
    sheet.save(os.path.join(DST, 'gallery_%s.png' % name))
    made += 1
print('sheets', made)
