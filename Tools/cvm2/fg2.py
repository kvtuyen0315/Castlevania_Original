# fg2.py (CVM2 02) - nền từ sheet theo (vùng, cx) + mặt nạ tiền cảnh (sprite) cho frame lưới NES. Dùng chung: simon2.py, objects2.py
import os, sys, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes2 import SECTIONS, load_sheet, section
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
_S = None; _J = None
def _init():
    global _S, _J
    if _S is None:
        sh = load_sheet()
        _S = {k: section(sh, k)[16:192] for k in SECTIONS}   # (176, W, 3) float = NES y 48..223
        _J = {'HallA>TEB': np.concatenate([_S['TowerHallA'], _S['TowerEntranceB']], axis=1),
              'HallC>HallB': np.concatenate([_S['TowerHallC'], _S['TowerHallB']], axis=1)}
def bg(section_name, cx, cxw=None, lut=True):
    """nền (176,256,3) float cho NES y 48..223; section 'A>B' dùng cxw (toạ độ ghép). lut=True: đổi màu sheet -> màu video (bảng màu video khác sheet)"""
    _init()
    src = _J if section_name in _J else _S
    if section_name in _J: a = _J[section_name][:, cxw:cxw + 256]
    else: a = _S[section_name][:, cx:cx + 256]
    return apply_lut(a) if lut else a
_LUT = None
def lut():
    """màu sheet (3 byte) -> màu video: trung vị trên ~190 frame ngẫu nhiên (cache work/map2_lut.npz)"""
    global _LUT
    if _LUT is not None: return _LUT
    p = os.path.join(WORK, 'map2_lut.npz')
    if os.path.exists(p):
        z = np.load(p); _LUT = dict(zip([int(k) for k in z['k']], [tuple(v) for v in z['v']])); return _LUT
    _init()
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    cam = load_camera(); acc = {}; rng = np.random.RandomState(1)
    for f in range(190, 7500, 37):
        c = cam[f]
        if c['kind'] != 'game': continue
        b = bg(c['section'], int(c['cx']), int(c['cxw']), lut=False)
        fr = np.asarray(mm[f])[40:216].astype(int)
        key = (b[..., 0].astype(int) << 16) | (b[..., 1].astype(int) << 8) | b[..., 2].astype(int)
        idx = rng.choice(key.size, 4000, replace=False)
        for k, v in zip(key.ravel()[idx], fr.reshape(-1, 3)[idx]): acc.setdefault(int(k), []).append(v)
    keys = [k for k, v in acc.items() if len(v) >= 200]
    vals = [np.median(np.array(acc[k]), axis=0) for k in keys]
    np.savez(p, k=np.array(keys), v=np.array(vals))
    _LUT = dict(zip(keys, [tuple(v) for v in vals]))
    return _LUT
def apply_lut(a):
    L = lut()
    key = (a[..., 0].astype(int) << 16) | (a[..., 1].astype(int) << 8) | a[..., 2].astype(int)
    out = np.zeros(a.shape, float)
    for k, v in L.items():
        out[key == k] = v
    miss = ~np.isin(key, list(L.keys()))
    out[miss] = a[miss]
    return out
def fg_mask(frame_nes, section_name, cx, cxw=None, thr=60, y0=8, tol=1):
    """frame_nes (224,256,3) NES y 8..231 -> mask (176,256) cho y 48..223. Sai khác = min trên dịch ±tol pixel của nền (khử viền do lệch lấy mẫu)"""
    _init()
    if section_name in _J: full = _J[section_name]; c0 = cxw
    else: full = _S[section_name]; c0 = cx
    pad = tol
    lo = max(0, c0 - pad); hi = min(full.shape[1], c0 + 256 + pad)
    a = apply_lut(full[:, lo:hi])
    f = frame_nes[48 - y0:224 - y0].astype(float)
    best = None
    for dy in range(-tol, tol + 1):
        for dx in range(-tol, tol + 1):
            x0 = c0 - lo + dx
            if x0 < 0 or x0 + 256 > a.shape[1]: continue
            b = np.roll(a, -dy, axis=0)[:, x0:x0 + 256]
            d = np.abs(f - b).max(axis=2)
            best = d if best is None else np.minimum(best, d)
    return best > thr
def load_camera(video='map2'):
    cam = {}
    for r in csv.DictReader(open(os.path.join(WORK, video + '_camera.csv'))):
        cam[int(r['frame'])] = r
    return cam
