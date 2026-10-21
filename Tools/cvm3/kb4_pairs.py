# kb4_pairs.py (CVM3 04) - cap anh dai khung: hang tren = video (luoi NES), hang duoi = Unity (Game view 1920x1080, NES x4 tai (448,60))
# cung tick t (knockback: t tu luc trung; lo: n tick bay; mep: tick quanh luc roi). Unity: anh Kb4Probe <case>_<rel>.png.
# Simon trong anh Unity dinh vi bang khop mau sprite (kb4_common.fit) => cat cung khung NES quanh Simon nhu video.
# Chay: python -P -E Tools/cvm3/kb4_pairs.py <thu muc Unity co anh>  -> ClaudeNotes/cvm3/ref/nes/cvm3_04_pairs_*.png
import sys, os
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

U = sys.argv[1]
OUT = os.path.join(K.REPO, 'ClaudeNotes', 'cvm3', 'ref', 'nes')
S = 3
CW, CH = 80, 64


def unity_nes(path):
    im = np.asarray(Image.open(path).convert('RGB')).astype(float)
    # NES (x,y) -> pixel (448 + 4x + 2, 60 + 4y + 2)
    xs = 448 + 4 * np.arange(256) + 2; ys = 60 + 4 * np.arange(240) + 2
    return im[ys][:, xs]


def vid_nes(video, f):
    for ff, img, nes in K.read_frames(video, f, f):
        return nes


def crop(nes, cx, cy):
    out = np.zeros((CH, CW, 3))
    x0, y0 = cx - CW // 2, cy - CH // 2
    xa, xb = max(0, x0), min(256, x0 + CW); ya, yb = max(0, y0), min(240, y0 + CH)
    out[ya - y0:yb - y0, xa - x0:xb - x0] = nes[ya:yb, xa:xb]
    return out


def find_simon(nes, names, gx=128, gy=150):
    sp = K.load_sprites(names)
    b = K.fit(nes, sp, K.SHEET_COLORS, gx, gy, rx=120, ry=60)
    return b


def strip(title, pairs, out):
    """pairs = [(nhan, anh video NES crop, anh Unity NES crop)]"""
    n = len(pairs)
    im = Image.new('RGB', (n * CW * S, 2 * CH * S + 40), (30, 30, 30))
    dr = ImageDraw.Draw(im)
    dr.text((4, 2), title, fill=(255, 255, 255))
    for i, (lab, a, b) in enumerate(pairs):
        x = i * CW * S
        im.paste(Image.fromarray(np.clip(a, 0, 255).astype(np.uint8)).resize((CW * S, CH * S), Image.NEAREST), (x, 14))
        im.paste(Image.fromarray(np.clip(b, 0, 255).astype(np.uint8)).resize((CW * S, CH * S), Image.NEAREST), (x, 14 + CH * S + 12))
        dr.text((x + 3, 14 + CH * S), 'video ' + lab, fill=(255, 255, 0))
        dr.text((x + 3, 14 + 2 * CH * S + 12 - 12), 'Unity', fill=(0, 255, 255))
    im.save(out)
    print('ok', out)


def kb_pairs(case, video, vf_by_t, vx0, vgy, ux_rel0):
    """vf_by_t: {t: frame video}; crop cung goc toa do so voi diem trung (video x0/san, Unity x0/san)."""
    pairs = []
    for t, f in sorted(vf_by_t.items()):
        p = os.path.join(U, '%s_%03d.png' % (case, t))
        if not os.path.exists(p):
            continue
        vn = vid_nes(video, f)
        un = unity_nes(p)
        b = find_simon(un, ['hurt_0', 'idle_0', 'jumpduck_0'])
        # Unity: dat khung theo vi tri Simon tim duoc truoc luc trung: x man Simon luc t = Simon(t); x0 man = Simon(t) - dx(t)
        dx = -ux_rel0 * t   # huong lui * t (khong tuong)
        ux0 = b['px'] - dx; ugy = b['Y'] + uh(t)
        pairs.append(('t%d f%d' % (t, f), crop(vn, vx0 + dx // 2, vgy - 24), crop(un, ux0 + dx // 2, ugy - 24)))
    return pairs


H = [0, 3, 6, 8, 9, 10, 11, 12, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 13, 13, 12, 11, 10, 9, 8, 6, 3, 0]


def uh(t):
    return H[t] if 0 <= t < len(H) else 0


if __name__ == '__main__':
    os.makedirs(OUT, exist_ok=True)
    # knockback tu do: lui trai (Boss_Map_2 #3) / lui phai (Boss_Map_3 #1)
    strip('CVM3 04 knockback lui TRAI: video Boss_Map_2 #3 (x0 104, san 176) vs Unity kb_free_l (t = tick tu luc trung, dong ho x)',
          kb_pairs('kb_free_l', 'boss2', {6: 355, 12: 359, 20: 363, 26: 366, 30: 368}, 104, 176, 1),
          os.path.join(OUT, 'cvm3_04_pairs_kb_left.png'))
    strip('CVM3 04 knockback lui PHAI: video Boss_Map_3 #1 (x0 196, san 208) vs Unity kb_free_r',
          kb_pairs('kb_free_r', 'boss3', {6: 543, 12: 546, 20: 550, 26: 554}, 196, 208, -1),
          os.path.join(OUT, 'cvm3_04_pairs_kb_right.png'))
    # lo nuoc thanh: Map_3 (lo f6230/f6717) vs Unity holy_drop64_r; cat quanh vi tri lo theo mo hinh (x0 22 / 100, tam sinh 125 / 93)
    import math
    def hp(x0, y0, n):
        return x0 + math.floor(1.5 * n), math.floor(y0 - 1.5 * n + 33 / 256.0 * n * (n + 1) / 2)
    pairs = []
    for n, vf in [(8, 6717 + 13), (16, 6230 + 16), (24, 6230 + 20), (32, 6230 + 24), (48, 6230 + 32), (51, 6230 + 34), (55, 6230 + 36)]:
        p = os.path.join(U, 'holy_drop64_r_%03d.png' % (n + 2))
        if not os.path.exists(p):
            continue
        vn = vid_nes('map3', vf); un = unity_nes(p)
        # Unity camera: tim Simon (dung yen x 48 section) => cx
        b = find_simon(un, ['idle_0'], 48, 176)
        cx = 48 - b['px']
        nn = min(n, 49)
        vx, vy = hp(22, 125, nn); ux, uy = hp(100, 93, nn)
        pairs.append(('n%d f%d' % (n, vf), crop(vn, vx, vy + 8), crop(un, ux - cx, uy + 8 + 32)))
    strip('CVM3 04 lo nuoc thanh: video Map_3 tran Mummy (Simon (22,144), san 208) vs Unity holy_drop64_r (tam lo cao hon san 83 px); n = tick bay, lua tu n49',
          pairs, os.path.join(OUT, 'cvm3_04_pairs_holy.png'))
    # mep roi: Map_1 EH Buc C mep trai 928 (video f1880..1886) vs Unity ledge_eh_l rel 16..25 (roi o rel 18)
    pairs = []
    for rel, vf in [(16, 1880), (17, 1881), (18, 1883), (19, 1884), (21, 1885), (25, 1887)]:
        p = os.path.join(U, 'ledge_eh_l_%03d.png' % rel)
        if not os.path.exists(p):
            continue
        vn = vid_nes('map1', vf); un = unity_nes(p)
        bv = find_simon(vn, ['idle_0', 'walk_0', 'walk_1', 'walk_2', 'jumpduck_0'], 128, 150)
        bu = find_simon(un, ['idle_0', 'walk_0', 'walk_1', 'walk_2', 'jumpduck_0'], 128, 150)
        pairs.append(('rel%d f%d x%d,%d' % (rel, vf, bv['px'], bv['Y']), crop(vn, bv['px'], 150), crop(un, bu['px'], 150)))
    strip('CVM3 04 di khoi mep TRAI Buc C (EntranceHall, mep 928): video Map_1 1:02.7 vs Unity ledge_eh_l (roi o x 921 ca hai)',
          pairs, os.path.join(OUT, 'cvm3_04_pairs_ledge_eh.png'))
