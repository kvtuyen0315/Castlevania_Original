# platform2.py (CVM2 02) - dò sprite bệ di động (Object/platform.png, 32x16) trong mặt nạ tiền cảnh từng frame bằng tương quan FFT + IoU cửa sổ
# Chạy: python -P -E Tools/cvm2/platform2.py map2  -> work/map2_platform.csv (frame,section,cx,sx,sy,iou,wx)   sx,sy = góc trên-trái trên màn NES; wx = toạ độ vùng (cx + sx; vùng ghép dùng cxw)
import sys, os, csv
import numpy as np
from PIL import Image
HERE = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, HERE)
from fg2 import WORK, ROOT, fg_mask, load_camera, lut
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'
f0 = int(sys.argv[2]) if len(sys.argv) > 2 else 0
f1 = int(sys.argv[3]) if len(sys.argv) > 3 else None
K = np.asarray(Image.open(os.path.join(ROOT, 'Assets', 'Data', 'Art', 'Object', 'platform.png')).convert('RGBA'))[..., 3] > 0
kh, kw = K.shape
nk = K.sum()
H, W = 176, 256
Fk = np.fft.rfft2(K.astype(float), s=(H, W))
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
cam = load_camera(video); lut()
rows = []
for i in range(f0, (mm.shape[0] if f1 is None else f1 + 1)):
    c = cam[i]
    if c['kind'] != 'game': continue
    m = fg_mask(np.asarray(mm[i]), c['section'], int(c['cx']), int(c['cxw']))
    A = m.astype(float)
    corr = np.fft.irfft2(np.fft.rfft2(A) * np.conj(Fk), s=(H, W))   # corr[y,x] = overlap khi sprite đặt góc trên-trái (x,y)
    ii = np.pad(A, ((1, 0), (1, 0))).cumsum(0).cumsum(1)
    wsum = ii[kh:, kw:] - ii[:-kh, kw:] - ii[kh:, :-kw] + ii[:-kh, :-kw]   # (H-kh+1, W-kw+1)
    ov = corr[:H - kh + 1, :W - kw + 1]
    iou = ov / (nk + wsum - ov + 1e-9)
    y, x = np.unravel_index(np.argmax(iou), iou.shape)
    if iou[y, x] >= 0.5 and ov[y, x] >= 0.8 * nk:
        base = int(c['cxw']) if c['section'] in ('HallA>TEB', 'HallC>HallB') else int(c['cx'])
        rows.append([i, c['section'], c['cx'], x, y + 48, '%.2f' % iou[y, x], base + x])
with open(os.path.join(WORK, video + '_platform.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 'section', 'cx', 'sx', 'sy', 'iou', 'wx']); w.writerows(rows)
print('frames with platform', len(rows))
