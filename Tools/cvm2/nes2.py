# nes2.py - ham dung chung cho planner CVM2 (Map_2 / Boss_Map_2): doc frame, lay mau luoi NES, vung sheet Level 2
import os, subprocess
import numpy as np
from PIL import Image

FFMPEG = r'C:\ffmpeg\bin\ffmpeg.exe'
VDIR = r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original'
VID = {
    'map2': (VDIR + r'\Map_2.mp4', 1486, 712),
    'boss2': (VDIR + r'\Boss_Map_2_Behaviour.mp4', 1490, 718),
}
# x_v = ox + sx * x_nes ; y_v = oy + sy * y_nes (pixel video, goc tren-trai). Gia tri cuoi = calib_map2.py
CALIB = {
    'map2': dict(sx=3.617, sy=3.10, ox=270.0, oy=-19.6),
    'boss2': dict(sx=3.61, sy=3.10, ox=276.0, oy=-20.0),
}
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
SHEET = os.path.join(REPO, 'Art_Source', 'spriters-resource', 'level2_main_hall.png')  # CVM2 01: chep nguyen byte tu Downloads
# vung [x0, y0, w] trong sheet, cao 192; dong r <-> NES y = 32 + r (gia thuyet nhu Level 1 - calib kiem lai)
SECTIONS = {
    'TowerEntranceA': (1803, 632, 256),
    'TowerEntranceB': (1547, 425, 512),
    'TowerHallA': (774, 425, 768),
    'TowerHallB': (774, 218, 768),
    'TowerHallC': (1, 218, 768),
    'Medusa': (1, 11, 768),
}
ROW0 = 32


def load_sheet():
    return np.asarray(Image.open(SHEET).convert('RGB')).astype(float)


def section(sheet, k):
    x0, y0, w = SECTIONS[k]
    return sheet[y0:y0 + 192, x0:x0 + w]


def gray(a):
    return a[..., 0] * 0.299 + a[..., 1] * 0.587 + a[..., 2] * 0.114


def read_frames(video, ss=None, t=None, fps=None):
    path, W, H = VID[video]
    cmd = [FFMPEG, '-hide_banner', '-loglevel', 'error']
    if ss is not None:
        cmd += ['-ss', '%.3f' % ss]
    cmd += ['-i', path]
    if t is not None:
        cmd += ['-t', '%.3f' % t]
    if fps:
        cmd += ['-vf', 'fps=%s' % fps]
    cmd += ['-f', 'rawvideo', '-pix_fmt', 'rgb24', '-']
    p = subprocess.Popen(cmd, stdout=subprocess.PIPE, bufsize=W * H * 3 * 4)
    n = W * H * 3
    try:
        while True:
            buf = p.stdout.read(n)
            if len(buf) < n:
                break
            yield np.frombuffer(buf, np.uint8).reshape(H, W, 3)
    finally:
        p.stdout.close(); p.wait()


def nes_sample(img, cal, y0=0, y1=240, x0=0, x1=256):
    """lay mau tam tung pixel NES (bilinear-free: nearest) -> mang (y1-y0, x1-x0, 3)"""
    H, W = img.shape[:2]
    xs = np.clip(np.round(cal['ox'] + cal['sx'] * (np.arange(x0, x1) + 0.5)).astype(int), 0, W - 1)
    ys = np.round(cal['oy'] + cal['sy'] * (np.arange(y0, y1) + 0.5)).astype(int)
    out = np.zeros((y1 - y0, x1 - x0, 3), float)
    ok = (ys >= 0) & (ys < H)
    out[ok] = img[ys[ok]][:, xs].astype(float)
    return out


def zncc(a, b):
    a = a - a.mean(); b = b - b.mean()
    d = np.sqrt((a * a).sum() * (b * b).sum())
    return float((a * b).sum() / d) if d > 0 else 0.0


def match_camera(pf_gray, sec_gray):
    """pf_gray: 176x256 (NES y 48..223); sec_gray: 192xW (r 0..191) -> (cx, score) tot nhat"""
    win_rows = sec_gray[16:192]
    best = (0, -2.0)
    for cx in range(0, sec_gray.shape[1] - 256 + 1):
        s = zncc(pf_gray, win_rows[:, cx:cx + 256])
        if s > best[1]:
            best = (cx, s)
    return best
