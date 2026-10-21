# nes3.py - ham dung chung cho planner CVM3 (Map_3 / Boss_Map_3): doc frame, lay mau luoi NES, vung sheet Level 3
import subprocess
import numpy as np
from PIL import Image

FFMPEG = r'C:\ffmpeg\bin\ffmpeg.exe'
VDIR = r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original'
VID = {
    'map3': (VDIR + r'\Map_3.mp4', 1488, 760),
    'boss3': (VDIR + r'\Boss_Map_3_Behaviour.mp4', 1370, 772),
}
# x_v = ox + sx * x_nes ; y_v = oy + sy * y_nes (pixel video, goc tren-trai). Gia tri cuoi = calib_map3.py
CALIB = {
    'map3': dict(sx=3.859, sy=3.31, ox=240.928, oy=-21.92),
    'boss3': dict(sx=3.845, sy=3.363, ox=195.76, oy=-21.768),
}
SHEET = r'C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 3_ Crumbling Tower.png'
# vung [x0, y0, w] trong sheet, cao 192; dong r <-> NES y = 32 + r (nhu Level 1/2 - calib kiem lai)
SECTIONS = {
    'WalkwayEntranceA': (1, 425, 768),
    'WalkwayEntranceB': (1, 218, 768),
    'WalkwayA': (774, 218, 768),
    'WalkwayB': (1286, 11, 768),
    'MummyMan': (2059, 11, 1536),
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
    """lay mau tam tung pixel NES (nearest) -> mang (y1-y0, x1-x0, 3); ngoai khung = 0"""
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
