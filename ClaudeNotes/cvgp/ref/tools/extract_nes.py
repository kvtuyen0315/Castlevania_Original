# extract_nes.py - giai ma TOAN BO video (30 fps goc), lay mau ve luoi NES, ghi memmap + diff tung frame
# Chay: python -P -E tools/extract_nes.py map1   (hoac boss)
# Ra:   work/<video>_nes.npy  uint8 (N, 224, 256, 3) (np.load(..., mmap_mode="r"))  = NES y 8..231, x 0..255 (moi pixel = TB 3x3 px video quanh tam)
#       work/<video>_meta.npz  diff_full (mean |f_i - f_(i-1)| tren anh video thu nho 1/2), diff_nes, n, fps
import sys, os, time
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import *

video = sys.argv[1]
y0, y1 = (8, 232)
cal = CALIB[video]
path, W, H = VID[video]
maxn = 30 * 260
out = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work', f'{video}_nes.u8')
mm = np.lib.format.open_memmap(out.replace('.u8', '.npy'), mode='w+', dtype=np.uint8, shape=(maxn, y1 - y0, 256, 3))
diff_full = []; diff_nes = []; maxd = []
prev = None; prevn = None
t0 = time.time()
i = 0
for fr in read_frames(video):
    n = nes_sample(fr, cal, y0=y0, y1=y1)
    nu = np.clip(np.round(n), 0, 255).astype(np.uint8)
    mm[i] = nu
    small = fr[::2, ::2].astype(np.int16)
    if prev is not None:
        d = np.abs(small - prev)
        diff_full.append(float(d.mean())); maxd.append(int(d.max()))
        diff_nes.append(float(np.abs(nu.astype(np.int16) - prevn).mean()))
    else:
        diff_full.append(-1.0); maxd.append(-1); diff_nes.append(-1.0)
    prev = small; prevn = nu.astype(np.int16)
    i += 1
    if i % 600 == 0:
        print(i, 'frames', round(time.time() - t0, 1), 's', flush=True)
mm.flush()
del mm
# cat bo phan cap phat du (maxn) -> file chi con i frame
_src = out.replace('.u8', '.npy'); _tmp = _src.replace('.npy', '_tmp.npy')
_m = np.load(_src, mmap_mode='r')
_o = np.lib.format.open_memmap(_tmp, mode='w+', dtype=np.uint8, shape=(i,) + _m.shape[1:])
for k in range(0, i, 500):
    _o[k:k + 500] = _m[k:min(i, k + 500)]
_o.flush(); del _o; del _m
os.replace(_tmp, _src)
np.savez(out.replace('_nes.u8', '_meta.npz'), diff_full=np.array(diff_full), diff_nes=np.array(diff_nes), maxd=np.array(maxd), n=i, fps=30.0)
print('done', i, 'frames', round(time.time() - t0, 1), 's')
