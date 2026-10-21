"""CVM3 08 — dải ảnh kiểm "enemy rơi vật": mỗi lần HUD cộng điểm (frame k) ⇒ 1 hàng khung lưới NES quanh k (mặc định k−4, k, k+6,
k+10, k+14, k+22 — vật rơi từ enemy hiện 20 tick ≈ 10 frame sau lúc chết, đúng nhịp nến CVGP 11) để xem bằng mắt: lửa chết ở đâu, có vật
hiện TẠI CHỖ lửa không (khác vật từ nến/khối gần đó).
Bộ đệm lưới NES (y NES 8..231): map1 = ClaudeNotes/cvgp/_cache/video/work/map1_nes.npy (tạo lại: cd ClaudeNotes/cvgp/_cache/video &&
python -P -E tools/extract_nes.py map1), map2 = ClaudeNotes/cvm2/_cache/02/work/map2_nes.npy, map3 = ClaudeNotes/cvm3/_cache/02/work/map3_nes.npy.
Chạy (từ gốc repo): python -P -E Tools/cvm3/drops8b_strip.py <map1|map2|map3> <out.png> <k1[:nhãn],k2,...> [offsets=-4,0,6,10,14,22] [scale=1.5]
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
NPY = {
    'map1': os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work', 'map1_nes.npy'),
    'map2': os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work', 'map2_nes.npy'),
    'map3': os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work', 'map3_nes.npy'),
}
Y0, YA, YB = 8, 48, 224      # bộ đệm bắt đầu y NES 8; chỉ lấy sân chơi y 48..223


def main():
    video, out, spec = sys.argv[1], sys.argv[2], sys.argv[3]
    offs = [int(v) for v in (sys.argv[4] if len(sys.argv) > 4 else '-4,0,6,10,14,22').split(',')]
    S = float(sys.argv[5]) if len(sys.argv) > 5 else 1.5
    mm = np.load(NPY[video], mmap_mode='r')
    kills = []
    for part in spec.split(','):
        k, _, lab = part.partition(':')
        kills.append((int(k), lab))
    w, h = int(256 * S), int((YB - YA) * S)
    img = Image.new('RGB', (len(offs) * (w + 3), len(kills) * (h + 14)), (30, 30, 30))
    d = ImageDraw.Draw(img)
    for r, (k, lab) in enumerate(kills):
        for c, o in enumerate(offs):
            f = k + o
            if f < 0 or f >= mm.shape[0]:
                continue
            crop = np.asarray(mm[f])[YA - Y0:YB - Y0]
            X, Y = c * (w + 3), r * (h + 14)
            img.paste(Image.fromarray(crop).resize((w, h), Image.NEAREST), (X, Y + 14))
            d.text((X + 2, Y + 1), '%s f%d (k%+d) %s' % (video, f, o, lab if c == 0 else ''), fill=(255, 255, 0))
    img.save(out)
    print('saved', out, img.size)


if __name__ == '__main__':
    main()
