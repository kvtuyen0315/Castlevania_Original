# simon2.py (CVM2 02) - định vị Simon từng frame Map_2 bằng KHỚP MẪU sprite (Tools/cvgp/simon_fit.py) quanh blob màu cam/kem (objects2.py)
# Chạy: python -P -E Tools/cvm2/simon2.py map2   -> _cache/02/work/map2_simon.csv
# Cột: frame,t,dup,section,cx,cxw,sprite,flip,px,Y,err,n,world_x,y_sec,found
#   px = x tâm thân trên màn NES; Y = hàng MẶT SÀN dưới chân (quy ước CVGP 07 §4b); world_x = cx + px (trong vùng); y_sec = Y - 32 (hàng trong vùng sheet)
#   đoạn cuộn qua cửa (section 'A>B'): world_x tính theo toạ độ ghép (cxw + px).
# 2 lượt: (1) mỗi 20 frame, bảng màu sheet -> hiệu chuẩn bảng màu video (trung vị) ; (2) mọi frame với bảng màu video; chạy song song theo khoảng frame.
import sys, os, csv
import numpy as np
from concurrent.futures import ProcessPoolExecutor
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, '..', 'cvgp'))
import simon_fit as sf
from fg2 import WORK, load_camera
video = sys.argv[1] if len(sys.argv) > 1 else 'map2'

_g = {}
def _init():
    _g['mm'] = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    _g['sprites'] = sf.load_sprites()
    _g['cam'] = load_camera(video)
    rows = {}
    for r in csv.DictReader(open(os.path.join(WORK, video + '_blobs.csv'))):
        rows.setdefault(int(r['frame']), []).append(r)
    _g['blobs'] = rows
    d = np.load(os.path.join(WORK, video + '_dup.npy')); _g['dup'] = d

def simon_blob(f):
    best = None
    for r in _g['blobs'].get(f, []):
        o, c = int(r['or']), int(r['cr'])
        h = int(r['y1']) - int(r['y0']) + 1; w = int(r['x1']) - int(r['x0']) + 1
        if o >= 4 and c >= 3 and 8 <= h <= 40 and w <= 90:
            sc = o + c
            if best is None or sc > best[0]: best = (sc, r)
    return best[1] if best else None

def fit_range(args):
    a, b, step, pal, search = args
    if not _g: _init()
    out = []
    prev = None
    for f in range(a, b + 1, step):
        c = _g['cam'][f]
        if c['kind'] != 'game': prev = None; continue
        bl = simon_blob(f)
        nes = _g['mm'][f].astype(float)
        if prev is not None and step == 1 and f - prev['frame'] <= 3:
            g = (prev['lx'], prev['lx'] + 16, prev['ty'], prev['ty'] + 31)
        elif bl is not None:
            g = (int(bl['x0']) - 2, int(bl['x1']), int(bl['y0']) - 2, int(bl['y1']))
        else:
            prev = None; continue
        best = sf.fit_frame(nes, _g['sprites'], pal, g, search=search)
        if best is not None and best['err'] > 0.12 and bl is not None and g != (int(bl['x0']) - 2, int(bl['x1']), int(bl['y0']) - 2, int(bl['y1'])):
            g2 = (int(bl['x0']) - 2, int(bl['x1']), int(bl['y0']) - 2, int(bl['y1']))
            b2 = sf.fit_frame(nes, _g['sprites'], pal, g2, search=search)
            if b2 is not None and b2['err'] < best['err']: best = b2
        if best is None: prev = None; continue
        best['frame'] = f
        out.append(best)
        prev = best if best['err'] < 0.12 else None
    return out

def split(a, b, parts, step, pal, search):
    n = b - a + 1; k = (n + parts - 1) // parts
    return [(a + i * k, min(b, a + (i + 1) * k - 1), step, pal, search) for i in range(parts) if a + i * k <= b]

if __name__ == '__main__':
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r'); N = mm.shape[0]
    pal = sf.SHEET_COLORS
    with ProcessPoolExecutor(10) as ex:
        res = [b for part in ex.map(fit_range, split(0, N - 1, 40, 20, pal, 14)) for b in part]
        for it in range(2):
            pal = sf.calibrate_palette(res)
            print('bang mau video:', [tuple(int(round(x)) for x in c) for c in pal], flush=True)
            res = [b for part in ex.map(fit_range, split(0, N - 1, 40, 20, pal, 14)) for b in part]
        print('luot 1 (step 20):', len(res), 'khop', flush=True)
        full = [b for part in ex.map(fit_range, split(0, N - 1, 40, 1, pal, 8)) for b in part]
    full.sort(key=lambda b: b['frame'])
    cam = load_camera(video); dup = np.load(os.path.join(WORK, video + '_dup.npy'))
    with open(os.path.join(WORK, video + '_simon.csv'), 'w', newline='') as fh:
        w = csv.writer(fh)
        w.writerow(['frame', 't', 'dup', 'section', 'cx', 'cxw', 'sprite', 'flip', 'px', 'Y', 'err', 'n', 'world_x', 'y_sec', 'found'])
        got = {b['frame']: b for b in full}
        for f in range(N):
            c = cam[f]; b = got.get(f)
            if b is None or b['err'] > 0.22:
                w.writerow([f, '%.3f' % (f / 30), int(dup[f]), c['section'], c['cx'], c['cxw'], '', '', '', '', '', '', '', '', 0]); continue
            base = int(c['cxw']) if c['section'] in ('HallA>TEB', 'HallC>HallB') else int(c['cx'])
            w.writerow([f, '%.3f' % (f / 30), int(dup[f]), c['section'], c['cx'], c['cxw'], b['sprite'], b['flip'], b['px'], b['Y'], '%.4f' % b['err'], b['n'], base + b['px'], b['Y'] - 32, 1])
    errs = [b['err'] for b in full]
    print('xong: %d frame khop, err trung vi %.4f, p90 %.4f' % (len(full), float(np.median(errs)), float(np.percentile(errs, 90))))
