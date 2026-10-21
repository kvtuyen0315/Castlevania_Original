"""CVM3 08 — định vị LỬA CHẾT của từng lần hạ (frame HUD cộng điểm k) rồi cắt ảnh PHÓNG TO quanh đó qua nhiều frame ⇒ xem bằng mắt
có vật rơi TẠI CHỖ enemy chết không (vật từ enemy hiện ~20 tick ≈ 10 frame sau lúc chết ở tâm lửa — nhịp nến CVGP 11, EnemyBase.Die).
Định vị: khớp mẫu FX/death_1, death_2 (+ hit_0) trên lưới NES (thước đo Tools/cvm2/enemy8_fit.py, bảng màu đỏ/hồng video Map_3
Tools/cvm3/enemy8b_pal_fireball_map3.json — cùng màu NES ở cả 3 video) ở frame k..k+8; chỉ nhận chỗ MỚI (frame k−8 ở đó không khớp) —
loại gạch/nến đỏ nền. Có thể ép toạ độ màn bằng "k@x,y".
Chạy (từ gốc repo): python -P -E Tools/cvm3/drops8b_flame.py <map1|map2|map3> <out.png> <k1[@x,y][:nhãn],...> [offsets=-2,2,6,10,12,16,22,30] [S=3] [half=40]
  → ảnh + dòng "k x y err" cho mỗi lần hạ (toạ độ màn NES của tâm lửa).
"""
import json, os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy8_fit as E8  # noqa: E402
from drops8b_strip import NPY, Y0  # noqa: E402


def locate(mm, k, sprites, pal):
    box = [0, 48, 256, 224]
    best = None
    old = E8.err_maps(np.asarray(mm[max(0, k - 8)]), sprites, pal, box)
    for f in range(k, min(k + 9, mm.shape[0])):
        cur = E8.err_maps(np.asarray(mm[f]), sprites, pal, box)
        for (s, acc), (_, acc0) in zip(cur, old):
            sc = acc + np.maximum(0.0, 0.12 - acc0) * 2.0     # phạt chỗ đã đỏ từ trước (gạch / nến)
            i = np.unravel_index(np.argmin(sc), sc.shape)
            e = float(sc[i])
            if best is None or e < best[0]:
                x, y = box[0] + i[1], box[1] + i[0]
                best = (e, x + s['px'], y + s['py'], f)
    return best


def main():
    video, out, spec = sys.argv[1], sys.argv[2], sys.argv[3]
    offs = [int(v) for v in (sys.argv[4] if len(sys.argv) > 4 else '-2,2,6,10,12,16,22,30').split(',')]
    S = int(sys.argv[5]) if len(sys.argv) > 5 else 3
    H = int(sys.argv[6]) if len(sys.argv) > 6 else 40
    mm = np.load(NPY[video], mmap_mode='r')
    sprites, cols = E8.load(['FX/death_1', 'FX/death_2', 'FX/hit_0'])
    d = json.load(open(os.path.join(ROOT, 'Tools', 'cvm3', 'enemy8b_pal_fireball_map3.json')))
    pal = [tuple(d[','.join(map(str, c))]) for c in cols]
    kills = []
    for part in spec.split(','):
        head, _, lab = part.partition(':')
        k, _, xy = head.partition('@')
        kills.append((int(k), [int(v) for v in xy.split(';')] if xy else None, lab))
    w = 2 * H * S
    img = Image.new('RGB', (len(offs) * (w + 3), len(kills) * (w + 14)), (30, 30, 30))
    dr = ImageDraw.Draw(img)
    for r, (k, xy, lab) in enumerate(kills):
        if xy:
            cx, cy, e, ff = xy[0], xy[1], -1.0, k
        else:
            e, cx, cy, ff = locate(mm, k, sprites, pal)
        print('k %d x %d y %d err %.3f f %d %s' % (k, cx, cy, e, ff, lab))
        x0 = int(min(max(cx - H, 0), 256 - 2 * H))
        y0 = int(min(max(cy - H, 48), 224 - 2 * H))
        for c, o in enumerate(offs):
            f = k + o
            if f < 0 or f >= mm.shape[0]:
                continue
            crop = np.asarray(mm[f])[y0 - Y0:y0 - Y0 + 2 * H, x0:x0 + 2 * H]
            X, Y = c * (w + 3), r * (w + 14)
            img.paste(Image.fromarray(crop).resize((w, w), Image.NEAREST), (X, Y + 14))
            dr.text((X + 2, Y + 1), 'f%d k%+d %s' % (f, o, ('%s (%d,%d)' % (lab, cx, cy)) if c == 0 else ''), fill=(255, 255, 0))
            # dấu tâm lửa
            mx, my = (cx - x0) * S + X, (cy - y0) * S + Y + 14
            dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(0, 255, 0))
    img.save(out)
    print('saved', out, img.size)


if __name__ == '__main__':
    main()
