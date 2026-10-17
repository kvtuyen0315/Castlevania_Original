"""CVM2 10 — y từng frame của vật bí mật đang TRỒI (chỉ chấm hàng sprite phía trên mặt sàn — phần dưới bị sàn che). Chạy (từ gốc repo):
  python -P -E Tools/cvm2/rise10.py <sprite (vd Item/crown_0)> <pal.json> <x thế giới> <hàng mặt sàn NES> <f0> <f1>
  ⇒ in frame, pivot y section (đáy + 1), số hàng thấy, sai khác. Vương miện: Item/crown_0 _cache/10/pal_crown10.json 136 208 496 534."""
# per-frame best y of rising item (partial-visibility aware: only score sprite rows ABOVE floor line)
import os, sys, json, numpy as np, csv
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import enemy8_fit as ef
name, palf, wx, floor_nes, f0, f1 = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5]), int(sys.argv[6])
sp, cols = ef.load([name]); s = [q for q in sp if q['flip'] == 0][0]
d = json.load(open(palf)); pal = [np.array(d[','.join(map(str, c))], float) for c in cols]
mm = np.load(os.path.join(ef.WORK, 'map2_nes.npy'), mmap_mode='r'); dup = np.load(os.path.join(ef.WORK, 'map2_dup.npy'))
cam = ef.camera()
for f in range(f0, f1 + 1):
    if dup[f]: continue
    cx = cam[f][2]; sx = wx - cx
    nes = np.asarray(mm[f]).astype(float)
    best = None
    for top in range(floor_nes - 18, floor_nes + 1):     # hàng trên cùng sprite (NES)
        acc = 0; n = 0
        for k in range(len(s['ys'])):
            yy = top + s['ys'][k]
            if yy >= floor_nes: continue          # phần dưới sàn bị che
            xx = sx - s['px'] + s['xs'][k]
            v = nes[yy - 8, xx]
            acc += min(np.sqrt(((v - pal[s['cls'][k]]) ** 2).sum()) / 441.7, 0.35); n += 1
        if n >= 6:
            e = acc / n
            if best is None or e < best[0]: best = (e, top, n)
    print(f, 'pivy_sec', best[1] + 16 - 32 if best else None, 'rows', best[2] if best else 0, 'err %.3f' % best[0] if best else '')
