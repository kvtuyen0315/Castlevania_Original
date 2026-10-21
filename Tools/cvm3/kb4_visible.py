# kb4_visible.py (CVM3 04) - Simon co hien tren frame khong (nhay bat tu sau knockback): khop mau moi sprite quanh vi tri cuoi.
# Chay: python -P -E Tools/cvm3/kb4_visible.py <video> <f0> <f1> <px0> <Y0> [r=40]
# In: frame, dup, err tot nhat, sprite, px, Y ; chuoi hien/an ('#' hien, '.' an, '=' frame lap)
import sys, os
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

video = sys.argv[1]; f0, f1, px0, Y0 = map(int, sys.argv[2:6])
r = 40
for a in sys.argv[6:]:
    if a.startswith('r='):
        r = int(a[2:])
sprites = K.load_sprites()
pal = None
prev = None
px, Y = px0, Y0
line = []
for f, img, nes in K.read_frames(video, f0, f1):
    dup = K.is_dup(nes, prev); prev = nes
    if pal is None:
        b0 = K.fit(nes, sprites, K.SHEET_COLORS, px, Y, rx=r, ry=16)
        s = [s for s in sprites if s['name'] == b0['sprite'] and s['flip'] == b0['flip']][0]
        pal = K.calib_palette(nes, s, b0['px'], b0['Y'])
    b = K.fit(nes, sprites, pal, px, Y, rx=r, ry=16)
    vis = b is not None and b['err'] < 0.10
    if vis:
        px, Y = b['px'], b['Y']
    line.append('=' if dup else ('#' if vis else '.'))
    print(f, int(dup), '%.3f' % b['err'], b['sprite'], b['px'], b['Y'], 'HIEN' if vis else '')
print(''.join(line))
