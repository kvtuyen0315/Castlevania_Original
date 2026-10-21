# kb4_common.py (CVM3 04) - ham dung chung do knockback / nuoc thanh / mep roi tren 6 video (Map_1..3 + Boss_Map_1..3).
# Hieu chuan (x_v = ox + sx*(x_nes+0.5)): map1/boss1 = CVGP 02 refine_calib2 (ClaudeNotes/cvgp/_cache/video/tools/nes_common.py),
# map2/boss2 = CVM2 02 (Tools/cvm2/nes2.py), map3/boss3 = CVM3 02 (Tools/cvm3/nes3.py). Toa do tra ve = man NES (0..255, 0..239).
# Khop mau sprite Simon (khuon Tools/cvgp/simon_fit.py): px = pivot x (tam than), Y = hang MAT SAN duoi chan (hang sprite r <-> y = Y - 31 + r).
# Chay tu goc repo: python -P -E Tools/cvm3/<script>.py
import os, subprocess
import numpy as np
from PIL import Image

FFMPEG = r'C:\ffmpeg\bin\ffmpeg.exe'
VDIR = r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original'
VID = {
    'map1': (VDIR + r'\Map_1.mp4', 1266, 950),
    'boss1': (VDIR + r'\Boss_Map_1_Behaviour.mp4', 1264, 834),
    'map2': (VDIR + r'\Map_2.mp4', 1486, 712),
    'boss2': (VDIR + r'\Boss_Map_2_Behaviour.mp4', 1490, 718),
    'map3': (VDIR + r'\Map_3.mp4', 1488, 760),
    'boss3': (VDIR + r'\Boss_Map_3_Behaviour.mp4', 1370, 772),
}
CALIB = {
    'map1': dict(sx=4.8585, sy=4.1605, ox=8.60, oy=-22.55),
    'boss1': dict(sx=3.6295, sy=3.1765, ox=169.68, oy=40.13),
    'map2': dict(sx=3.621, sy=3.101, ox=268.888, oy=-19.736),
    'boss2': dict(sx=3.608, sy=3.156, ox=274.456, oy=-23.016),
    'map3': dict(sx=3.862, sy=3.31, ox=240.044, oy=-21.92),
    'boss3': dict(sx=3.845, sy=3.372, ox=195.76, oy=-22.392),
}
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
CACHE = os.path.join(REPO, 'ClaudeNotes', 'cvm3', '_cache', '04')
SPR_DIR = os.path.join(REPO, 'Assets', 'Data', 'Art', 'Character', 'Simon')
SUB_DIR = os.path.join(REPO, 'Assets', 'Data', 'Art', 'SubWeapon')
SHEET_COLORS = [(124, 8, 0), (252, 152, 56), (252, 216, 168)]
SPRITES = [('idle_0', 8), ('walk_0', 8), ('walk_1', 8), ('walk_2', 8), ('jumpduck_0', 8),
           ('stairs_up_0', 8), ('stairs_up_1', 8), ('stairs_down_0', 8), ('stairs_down_1', 8),
           ('hurt_0', 8), ('death_0', 8),
           ('use_stand_0', 8), ('use_stand_1', 8), ('use_stand_2', 16),
           ('use_duck_0', 8), ('use_duck_1', 8), ('use_duck_2', 16),
           ('use_stairs_up_0', 8), ('use_stairs_up_1', 8), ('use_stairs_up_2', 16),
           ('use_stairs_down_0', 8), ('use_stairs_down_1', 8), ('use_stairs_down_2', 16)]
CLIP = 0.35


def read_frames(video, f0, f1, cal=None, y0=0, y1=240):
    """(frame, anh NES float (y1-y0)x256x3) cho frame video f0..f1 (gom 2 dau). Doc bang ffmpeg -ss chinh xac."""
    path, W, H = VID[video]
    cal = cal or CALIB[video]
    # chi so frame = pts*30 (giu pts goc bang -copyts + loc select theo t): -ss thuong lech 1 frame tuy vi tri keyframe (Map_3 f6088)
    ts = (f0 - 0.5) / 30.0
    ss = max(0.0, ts - 1.0)
    cmd = [FFMPEG, '-hide_banner', '-loglevel', 'error', '-ss', '%.4f' % ss, '-copyts', '-i', path,
           '-vf', 'select=gte(t\\,%.5f)' % ts, '-vsync', '0',
           '-frames:v', str(f1 - f0 + 1), '-f', 'rawvideo', '-pix_fmt', 'rgb24', '-']
    p = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, bufsize=W * H * 3 * 4)
    n = W * H * 3
    f = f0
    try:
        while f <= f1:
            buf = p.stdout.read(n)
            if len(buf) < n:
                break
            img = np.frombuffer(buf, np.uint8).reshape(H, W, 3)
            yield f, img, nes_sample(img, cal, y0, y1)
            f += 1
    finally:
        p.stdout.close(); p.wait()


def nes_sample(img, cal, y0=0, y1=240, x0=0, x1=256, dx=0.0, dy=0.0):
    H, W = img.shape[:2]
    xs = np.round(cal['ox'] + cal['sx'] * (np.arange(x0, x1) + 0.5 + dx)).astype(int)
    ys = np.round(cal['oy'] + cal['sy'] * (np.arange(y0, y1) + 0.5 + dy)).astype(int)
    xs = np.clip(xs, 0, W - 1)
    out = np.zeros((y1 - y0, x1 - x0, 3), float)
    ok = (ys >= 0) & (ys < H)
    out[ok] = img[ys[ok]][:, xs].astype(float)
    return out


def load_sprites(names=None):
    out = []
    for name, px in SPRITES:
        if names and name not in names:
            continue
        im = np.asarray(Image.open(os.path.join(SPR_DIR, name + '.png')).convert('RGBA')).astype(int)
        a = im[..., 3] > 0
        idx = np.full(a.shape, -1, int)
        for k, c in enumerate(SHEET_COLORS):
            idx[(im[..., 0] == c[0]) & (im[..., 1] == c[1]) & (im[..., 2] == c[2]) & a] = k
        h, w = a.shape
        for flip in (0, 1):
            ii = idx[:, ::-1] if flip else idx
            pxx = (w - px) if flip else px
            ys, xs = np.nonzero(ii >= 0)
            out.append(dict(name=name, flip=flip, w=w, h=h, pivx=pxx, ys=ys, xs=xs, cls=ii[ys, xs]))
    return out


def fit(nes, sprites, palette, px0, Y0, rx=12, ry=12, y_off=0):
    """quet pivot (px, Y) trong [px0-rx, px0+rx] x [Y0-ry, Y0+ry]; nes = anh NES bat dau o hang y_off. Tra (best, 2nd khac vi tri)."""
    H, W, _ = nes.shape
    pal = np.asarray(palette, float)
    cand = []
    for s in sprites:
        cols = pal[s['cls']]
        for Y in range(Y0 - ry, Y0 + ry + 1):
            r = Y - 31 + s['ys'] - y_off
            if r.min() < 0 or r.max() >= H:
                continue
            for px in range(px0 - rx, px0 + rx + 1):
                c = px - s['pivx'] + s['xs']
                if c.min() < 0 or c.max() >= W:
                    continue
                v = nes[r, c]
                d = np.sqrt(((v - cols) ** 2).sum(1)) / 441.7
                e = float(np.minimum(d, CLIP).mean())
                cand.append((e, s['name'], s['flip'], px, Y, s))
    if not cand:
        return None
    cand.sort(key=lambda c: c[0])
    b = cand[0]
    best = dict(err=b[0], sprite=b[1], flip=b[2], px=b[3], Y=b[4])
    # sai khac tot nhat o vi tri lech 1 px (do do "sac" cua cuc tieu)
    nb = {}
    for c in cand:
        if c[1] == b[1] and c[2] == b[2] and abs(c[3] - b[3]) + abs(c[4] - b[4]) == 1:
            nb[(c[3] - b[3], c[4] - b[4])] = c[0]
    best['nb'] = nb
    return best


def calib_palette(nes, sprite, px, Y, y_off=0):
    """mau video trung vi cua 3 lop mau sprite tai vi tri da biet"""
    pal = []
    for k in range(3):
        m = sprite['cls'] == k
        r = Y - 31 + sprite['ys'][m] - y_off
        c = px - sprite['pivx'] + sprite['xs'][m]
        pal.append(tuple(np.median(nes[r, c], axis=0)))
    return pal


def is_dup(a, b, thr=8):
    """frame lap (bo ghi lap lai anh cu): < thr pixel NES doi > 40 (tong 3 kenh) so frame truoc"""
    return b is not None and int((np.abs(a - b).sum(2) > 40).sum()) < thr
