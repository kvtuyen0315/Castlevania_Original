# extract_nes2.py (CVM2 02) - giải mã TOÀN BỘ video Map 2 (30 fps gốc), lấy mẫu về lưới NES, ghi memmap + diff từng frame
# Chạy (từ gốc repo): python -P -E Tools/cvm2/extract_nes2.py map2   (hoặc boss2)
# Ra: ClaudeNotes/cvm2/_cache/02/work/<video>_nes.npy  uint8 (N, 224, 256, 3) = NES y 8..231, x 0..255 (nearest, hiệu chuẩn nes2.CALIB)
#     <video>_meta.npz: diff_full (TB |f_i - f_(i-1)| ảnh video thu 1/2), diff_nes, n, fps
import sys, os, time
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes2 import *
video = sys.argv[1]
Y0, Y1 = 8, 232
cal = CALIB[video]
path, W, H = VID[video]
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
os.makedirs(WORK, exist_ok=True)
maxn = 30 * 270
out = os.path.join(WORK, video + '_nes.npy')
mm = np.lib.format.open_memmap(out, mode='w+', dtype=np.uint8, shape=(maxn, Y1 - Y0, 256, 3))
diff_full = []; diff_nes = []
prev = None; prevn = None
t0 = time.time(); i = 0
for fr in read_frames(video):
    nu = np.clip(np.round(nes_sample(fr, cal, y0=Y0, y1=Y1)), 0, 255).astype(np.uint8)
    mm[i] = nu
    small = fr[::2, ::2].astype(np.int16)
    if prev is not None:
        diff_full.append(float(np.abs(small - prev).mean())); diff_nes.append(float(np.abs(nu.astype(np.int16) - prevn).mean()))
    else:
        diff_full.append(-1.0); diff_nes.append(-1.0)
    prev = small; prevn = nu.astype(np.int16)
    i += 1
    if i % 1000 == 0:
        print(i, 'frames', round(time.time() - t0, 1), 's', flush=True)
mm.flush(); del mm
tmp = out.replace('.npy', '_tmp.npy')
m = np.load(out, mmap_mode='r')
o = np.lib.format.open_memmap(tmp, mode='w+', dtype=np.uint8, shape=(i,) + m.shape[1:])
for k in range(0, i, 500):
    o[k:k + 500] = m[k:min(i, k + 500)]
o.flush(); del o; del m
os.replace(tmp, out)
np.savez(os.path.join(WORK, video + '_meta.npz'), diff_full=np.array(diff_full), diff_nes=np.array(diff_nes), n=i, fps=30.0)
print('done', i, 'frames', round(time.time() - t0, 1), 's')
