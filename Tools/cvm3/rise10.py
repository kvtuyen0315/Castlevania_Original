"""CVM3 10 — đo túi tiền bí mật TRỒI sau sàn (phần dưới bị gạch che) trên video Map_3: mỗi frame thử mọi độ trồi d (đáy túi ở y_rest + d,
chỉ chấm các hàng sprite NẰM TRÊN mặt sàn y_floor) × 3 khung túi (đỏ/tím/trắng — FlashingMoneyBag nháy), lấy d có sai khác nhỏ nhất.
Khuôn Tools/cvm2/rise10.py (CVM2 10: vương miện/rương). Bảng màu = pal_<tag>.json của fit10.py (ước lượng tự động, chỉ để dò — H1).

Chạy (từ gốc repo):  python -P -E Tools/cvm3/rise10.py <section> <x_sheet> <y_rest> <y_floor> <f0> <f1> <pal.json> [errmax=0.09]
  vd St7:  python -P -E Tools/cvm3/rise10.py WalkwayEntranceB 424 130 128 1940 1990 ClaudeNotes/cvm3/_cache/10/pal_bag7.json
  → in từng frame: cx, x màn, độ trồi còn lại d (px dưới vị trí nghỉ), số hàng thấy, sai khác, khung khớp.
y_rest = đáy ĐO của túi lúc nghỉ (= pivy − 32, Unity Y = y_rest − 1); y_floor = mặt sàn chỗ túi (hàng y_floor trở xuống bị che).
"""
import json, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy8_fit as E8  # noqa: E402
import enemy7_fit as E7  # noqa: E402

WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
Y0 = 8   # bộ đệm map3_nes.npy bắt đầu ở NES y 8


def main():
    a = sys.argv
    sec, xw, yrest, yfloor, f0, f1, palp = a[1], int(a[2]), int(a[3]), int(a[4]), int(a[5]), int(a[6]), a[7]
    errmax = float(a[8]) if len(a) > 8 else 0.09
    names = ['Item/moneybag_red', 'Item/moneybag_purple', 'Item/moneybag_white']
    sprites, cols = E8.load(names)
    d = json.load(open(palp))
    pal = [np.array(d[','.join(map(str, c))], float) for c in cols]
    sprites = [s for s in sprites if s['flip'] == 0]
    cam = E7.camera('map3')
    dup = np.load(os.path.join(WORK, 'map3_dup.npy'))
    mm = np.load(os.path.join(WORK, 'map3_nes.npy'), mmap_mode='r')
    for f in range(f0, f1 + 1):
        c = cam.get(f)
        if c is None or c[1] != sec:
            continue
        cx = c[3] if '>' in c[1] else c[2]
        nes = np.asarray(mm[f]).astype(float)
        best = None
        for s in sprites:
            for dd in range(0, 17):
                # đáy sprite (pivot) ở y section yrest + dd ⇒ hàng sprite r ở NES y = 32 + yrest + dd − py + r
                ys = 32 + yrest + dd - s['py'] + s['ys']
                xs = (xw - cx) - s['px'] + s['xs']
                vis = (ys < 32 + yfloor) & (ys >= 48) & (xs >= 0) & (xs < 256)
                n = int(vis.sum())
                if n < 6:
                    continue
                px = nes[ys[vis] - Y0, xs[vis]]
                e = np.minimum(np.sqrt(((px - np.array([pal[k] for k in s['cls'][vis]])) ** 2).sum(1)) / 441.7, E8.CLIP).mean()
                if best is None or e < best[0]:
                    best = (e, dd, n, s['name'].split('_')[-1])
        if best is None:
            print('f%d cx %d sx %d: (ngoai khung)' % (f, cx, xw - cx))
            continue
        tag = '' if best[0] <= errmax else '  (khong khop)'
        print('f%d%s cx %d sx %d  d %2d  hang %3d  err %.3f %s%s' % (f, '*' if dup[f] else ' ', cx, xw - cx, best[1], best[2], best[0], best[3], tag))


if __name__ == '__main__':
    main()
