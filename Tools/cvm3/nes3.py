# nes3.py (CVM3 02) - ham dung chung (Map_3 / Boss_Map_3): doc frame, lay mau luoi NES, vung sheet Level 3. Khuon Tools/cvm2/nes2.py
# WalkwayB: camera co the cx < 0 (toi -109) => PAD[k] = so cot troi (36,24,140) dem ben trai (padded_section): chi so cot j <-> cx = j - PAD[k]
import os, subprocess
import numpy as np
from PIL import Image

FFMPEG = r'C:\ffmpeg\bin\ffmpeg.exe'
VDIR = r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original'
VID = {
    'map3': (VDIR + r'\Map_3.mp4', 1488, 760),
    'boss3': (VDIR + r'\Boss_Map_3_Behaviour.mp4', 1370, 772),
}
# x_v = ox + sx * x_nes ; y_v = oy + sy * y_nes (pixel video, goc tren-trai). Gia tri cuoi = calib_fine3.py / calib_boss3.py
CALIB = {
    # planner 17/10 (5 frame); CVM3 02 tinh lai - xem ClaudeNotes/cvm3/ref/data/calib_*.txt
    'map3': dict(sx=3.862, sy=3.31, ox=240.044, oy=-21.92),   # calib_fine3.py 15 frame, ZNCC TB 0.9728
    'boss3': dict(sx=3.845, sy=3.372, ox=195.76, oy=-22.392),   # calib_boss3.py 8 frame, ZNCC gradient TB 0.9431
}
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
SHEET = os.path.join(REPO, 'Art_Source', 'spriters-resource', 'level3_walkway.png')  # CVM3 01: chep nguyen byte tu Downloads
# vung [x0, y0, w] trong sheet, cao 192; dong r <-> NES y = 32 + r (planner: 5/5 section khop)
SECTIONS = {
    'WalkwayEntranceA': (1, 425, 768),
    'WalkwayEntranceB': (1, 218, 768),
    'WalkwayA': (774, 218, 768),
    'WalkwayB': (1286, 11, 768),
    'MummyMan': (2059, 11, 1536),
}
SKY = (36, 24, 140)
PAD = {'WalkwayB': 160}          # cot troi dem trai (camera cx am toi -109)
ROW0 = 32


def load_sheet():
    return np.asarray(Image.open(SHEET).convert('RGB')).astype(float)


def section(sheet, k):
    x0, y0, w = SECTIONS[k]
    return sheet[y0:y0 + 192, x0:x0 + w]


def padded_section(sheet, k):
    s = section(sheet, k)
    p = PAD.get(k, 0)
    if not p:
        return s
    pad = np.zeros((s.shape[0], p, 3), float)
    pad[:] = SKY
    return np.concatenate([pad, s], axis=1)


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
    p = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, bufsize=W * H * 3 * 4)
    n = W * H * 3
    try:
        while True:
            buf = p.stdout.read(n)
            if len(buf) < n:
                break
            yield np.frombuffer(buf, np.uint8).reshape(H, W, 3)
    finally:
        p.stdout.close(); p.wait()


def nes_sample(img, cal, y0=0, y1=240, x0=0, x1=256, box=0):
    """lay mau tam tung pixel NES (nearest) -> mang (y1-y0, x1-x0, 3). box=1: TB 3x3 pixel video quanh tam (giam nhieu nen h264)"""
    H, W = img.shape[:2]
    xs = np.round(cal['ox'] + cal['sx'] * (np.arange(x0, x1) + 0.5)).astype(int)
    ys = np.round(cal['oy'] + cal['sy'] * (np.arange(y0, y1) + 0.5)).astype(int)
    if box:
        acc = np.zeros((y1 - y0, x1 - x0, 3), float)
        n = 0
        for dy in range(-box, box + 1):
            for dx in range(-box, box + 1):
                yy = np.clip(ys + dy, 0, H - 1)
                acc += img[yy][:, np.clip(xs + dx, 0, W - 1)]
                n += 1
        acc /= n
        acc[(ys < 0) | (ys >= H)] = 0
        return acc
    xs = np.clip(xs, 0, W - 1)
    out = np.zeros((y1 - y0, x1 - x0, 3), float)
    ok = (ys >= 0) & (ys < H)
    out[ok] = img[ys[ok]][:, xs].astype(float)
    return out


def zncc(a, b):
    a = a - a.mean(); b = b - b.mean()
    d = np.sqrt((a * a).sum() * (b * b).sum())
    return float((a * b).sum() / d) if d > 0 else 0.0
