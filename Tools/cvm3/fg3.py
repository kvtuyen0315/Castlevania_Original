# fg3.py (CVM3 02) - nền từ sheet theo (vùng, cx) + mặt nạ tiền cảnh (sprite) cho frame lưới NES. Dùng chung: simon2.py, objects2.py
import os, sys, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes3 import SECTIONS, load_sheet, section, padded_section, PAD
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
_S = None; _J = None
def _init():
    global _S, _J
    if _S is None:
        sh = load_sheet()
        _S = {k: section(sh, k)[16:192] for k in SECTIONS}   # (176, W, 3) float = NES y 48..223 ; WalkwayB: cx am -> bg() dem troi ben trai
        _J = {'WEB>WA': np.concatenate([_S['WalkwayEntranceB'], _S['WalkwayA']], axis=1),
              'WB>MM': np.concatenate([_S['WalkwayB'], _S['MummyMan']], axis=1)}
def _padded(k, cx, pad=160):
    full = _S[k]
    if cx >= 0 and cx + 256 <= full.shape[1]: return full[:, cx:cx + 256]
    # cx am (WalkwayB toi -109): cot troi (36,24,140) ben trai
    out = np.zeros((full.shape[0], 256, 3), float); out[:] = (36, 24, 140)
    x0 = max(0, cx); x1 = min(full.shape[1], cx + 256)
    out[:, x0 - cx:x1 - cx] = full[:, x0:x1]
    return out
def bg(section_name, cx, cxw=None, lut=True, video='map3'):
    """nền (176,256,3) float cho NES y 48..223; section 'A>B' dùng cxw (toạ độ ghép). lut=True: đổi màu sheet -> màu video (bảng màu video khác sheet)"""
    _init()
    src = _J if section_name in _J else _S
    if section_name in _J: a = _J[section_name][:, cxw:cxw + 256]
    else: a = _padded(section_name, cx)
    return apply_lut(a, video) if lut else a
_LUT = {}
def lut(video='map3'):
    """màu sheet (3 byte) -> màu video: trung vị trên ~190 frame ngẫu nhiên (cache work/map3_lut.npz)"""
    if video in _LUT: return _LUT[video]
    p = os.path.join(WORK, video + '_lut.npz')
    if os.path.exists(p):
        z = np.load(p); _LUT[video] = dict(zip([int(k) for k in z['k']], [tuple(v) for v in z['v']])); return _LUT[video]
    _init()
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    cam = load_camera(video); acc = {}; rng = np.random.RandomState(1)
    for f in range(190 if video == 'map3' else 60, mm.shape[0], 37 if video == 'map3' else 12):
        c = cam[f]
        if c['kind'] != 'game': continue
        b = bg(c['section'], int(c['cx']), int(c['cxw']), lut=False, video=video)
        fr = np.asarray(mm[f])[40:216].astype(int)
        key = (b[..., 0].astype(int) << 16) | (b[..., 1].astype(int) << 8) | b[..., 2].astype(int)
        idx = rng.choice(key.size, 4000, replace=False)
        for k, v in zip(key.ravel()[idx], fr.reshape(-1, 3)[idx]): acc.setdefault(int(k), []).append(v)
    keys = [k for k, v in acc.items() if len(v) >= 200]
    vals = [np.median(np.array(acc[k]), axis=0) for k in keys]
    np.savez(p, k=np.array(keys), v=np.array(vals))
    _LUT[video] = dict(zip(keys, [tuple(v) for v in vals]))
    return _LUT[video]
def apply_lut(a, video='map3'):
    L = lut(video)
    key = (a[..., 0].astype(int) << 16) | (a[..., 1].astype(int) << 8) | a[..., 2].astype(int)
    out = np.zeros(a.shape, float)
    for k, v in L.items():
        out[key == k] = v
    miss = ~np.isin(key, list(L.keys()))
    out[miss] = a[miss]
    return out
def fg_mask(frame_nes, section_name, cx, cxw=None, thr=60, y0=8, tol=1, video='map3'):
    """frame_nes (224,256,3) NES y 8..231 -> mask (176,256) cho y 48..223. Sai khác = min trên dịch ±tol pixel của nền (khử viền do lệch lấy mẫu)"""
    _init()
    if section_name in _J: full = _J[section_name]; c0 = cxw
    else:
        # dem troi ben trai PAD de cx am cung chay duoc
        full = np.concatenate([np.full((176, PAD.get(section_name, 0), 3), (36, 24, 140.0)), _S[section_name]], axis=1); c0 = cx + PAD.get(section_name, 0)
    pad = tol
    lo = max(0, c0 - pad); hi = min(full.shape[1], c0 + 256 + pad)
    a = apply_lut(full[:, lo:hi], video)
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
def load_camera(video='map3'):
    cam = {}
    for r in csv.DictReader(open(os.path.join(WORK, video + '_camera.csv'))):
        cam[int(r['frame'])] = r
    return cam
