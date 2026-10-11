# nes_common.py - ham dung chung: doc frame video, doi toa do video <-> NES, ban do level 1
# Dung: import tu cac script khac trong tools/ (python -P -E tools/<script>.py ...)
import subprocess
import numpy as np
from PIL import Image

FFMPEG = r'C:\ffmpeg\bin\ffmpeg.exe'
VID = {
    'map1': (r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Map_1.mp4', 1266, 950),
    'boss': (r'C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original\Boss_Map_1_Behaviour.mp4', 1264, 834),
}
# He so hieu chuan: x_v = ox + sx * x_nes ; y_v = oy + sy * y_nes  (toa do pixel video, goc tren-trai)
# (gia tri cuoi cung do bang calib_hud.py + refine_calib.py; xem MAP1_TIMELINE.md §1)
CALIB = {
    'map1': dict(sx=4.8585, sy=4.1605, ox=8.60, oy=-22.55),   # refine_calib2.py (HUD-only: sx 4.8605 sy 4.1775 ox 8.46 oy -23.05)
    'boss': dict(sx=3.6295, sy=3.1765, ox=169.68, oy=40.13),  # refine_calib2.py (HUD-only: sx 3.627 sy 3.164 ox 169.5 oy 40.83)
}
MAP_PATH = r'C:\Users\kvtuy\Downloads\NES - Castlevania - Stages - Level 1_ Entrance.png'
# Cac vung trong sprite sheet (toa do pixel sheet). Moi vung cao 192 dong;
# dong r cua vung <-> NES y = 32 + r (16 dong dau bi HUD che), playfield thay duoc NES y 48..223.
SECTIONS = {
    'CastleEntrance': dict(x0=1, y0=11, w=768),
    'EntranceHall':   dict(x0=774, y0=11, w=1536),
    'CryptA':         dict(x0=2315, y0=11, w=512),
    'CryptB':         dict(x0=2315, y0=218, w=512),
    'PhantomBat':     dict(x0=2832, y0=11, w=768),
}
SEC_ROW_TO_NES = 32


def read_frames(video, ss=None, t=None, fps=None, vf_extra=None):
    """Generator: yields HxWx3 uint8 frames (full res) from ffmpeg pipe.
    ss: start seconds (accurate seek), t: duration seconds, fps: resample fps (None = native)."""
    path, W, H = VID[video]
    cmd = [FFMPEG, '-hide_banner', '-loglevel', 'error']
    if ss is not None:
        cmd += ['-ss', f'{ss:.3f}']
    cmd += ['-i', path]
    if t is not None:
        cmd += ['-t', f'{t:.3f}']
    vf = []
    if fps:
        vf.append(f'fps={fps}')
    if vf_extra:
        vf.append(vf_extra)
    if vf:
        cmd += ['-vf', ','.join(vf)]
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
        p.stdout.close()
        p.wait()


def nes_sample(img, cal, y0=8, y1=232, x0=0, x1=256, box=1):
    """Lay mau frame video tai tam moi pixel NES -> mang (y1-y0, x1-x0, 3) float.
    box=1: trung binh 3x3 pixel video quanh tam (giam nhieu nen video)."""
    H, W, _ = img.shape
    xs = cal['ox'] + cal['sx'] * (np.arange(x0, x1) + 0.5)
    ys = cal['oy'] + cal['sy'] * (np.arange(y0, y1) + 0.5)
    xi = np.clip(np.round(xs - 0.5).astype(int), 0, W - 1)
    yi = np.clip(np.round(ys - 0.5).astype(int), 0, H - 1)
    if box:
        acc = np.zeros((len(yi), len(xi), 3), float)
        k = 0
        for dy in (-box, 0, box):
            for dx in (-box, 0, box):
                acc += img[np.clip(yi + dy, 0, H - 1)][:, np.clip(xi + dx, 0, W - 1)]
                k += 1
        return acc / k
    return img[yi][:, xi].astype(float)


def load_map():
    return np.asarray(Image.open(MAP_PATH).convert('RGB')).astype(float)


def section_view(mapimg, sec, nes_y0=48, nes_y1=224):
    """Mang (nes_y1-nes_y0, w, 3) cua 1 vung, theo toa do NES y."""
    s = SECTIONS[sec]
    r0 = nes_y0 - SEC_ROW_TO_NES
    r1 = nes_y1 - SEC_ROW_TO_NES
    return mapimg[s['y0'] + r0: s['y0'] + r1, s['x0']: s['x0'] + s['w']]


def gray(a):
    return a[..., 0] * 0.299 + a[..., 1] * 0.587 + a[..., 2] * 0.114


def match_camera(play_gray, sec_gray, step=1):
    """ZNCC giua playfield NES (h x 256) va moi cua so 256 cot cua vung (h x w).
    Tra ve (best_cx, best_score, scores)."""
    h, w = sec_gray.shape
    P = play_gray - play_gray.mean()
    Pn = np.sqrt((P ** 2).sum()) + 1e-6
    n = w - 256 + 1
    scores = np.full(n, -1.0)
    # cumulative sums over columns for fast window mean/var
    S = sec_gray
    colsum = S.sum(axis=0)
    colsq = (S ** 2).sum(axis=0)
    cs = np.concatenate([[0], np.cumsum(colsum)])
    cq = np.concatenate([[0], np.cumsum(colsq)])
    N = h * 256
    for cx in range(0, n, step):
        win = S[:, cx:cx + 256]
        m = (cs[cx + 256] - cs[cx]) / N
        var = (cq[cx + 256] - cq[cx]) - N * m * m
        num = (P * win).sum()  # P has zero mean so sum(P*(win-m)) = sum(P*win)
        scores[cx] = num / (Pn * np.sqrt(max(var, 1e-6)))
    best = int(np.argmax(scores))
    return best, float(scores[best]), scores
