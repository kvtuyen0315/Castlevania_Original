"""CVGP 09 — ghép ảnh so sánh tấn công: khung video Map_1 (lấy mẫu lưới NES) đặt cạnh ảnh Play Unity cùng pha
(AttackProbe.RunPhoto ⇒ ClaudeNotes/cvgp/_cache/shots/09/photo_<tên>_<tick>.png, Game view 1920×1080 ⇒ vùng NES 1024×960
tại (448,60), 4 px/pixel NES). Cắt 112×56 px NES quanh Simon (cùng toạ độ màn), phóng ×4, xếp [video | Unity] từng hàng.
Chạy (từ gốc repo): python -P -E Tools/cvgp/attack_pairs.py  ⇒ ClaudeNotes/cvgp/_cache/shots/09/pair_*.png + pairs_all.png
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '09')
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import simon_pairs as sp  # noqa: E402

W, H = 112, 56
# (tên, video, frame, ảnh Unity, x màn Simon, Y màn (mặt sàn), dịch crop x, ghi chú)
PAIRS = [
    ('l1_over', 'map1', 95, 'photo_l1_007.png', 66, 192, 0, 'roi cấp 1 khung vắt (0:03.17 / tick 7)'),
    ('l1_ext', 'map1', 98, 'photo_l1_013.png', 66, 192, 0, 'roi cấp 1 duỗi đầu — chậu lửa vỡ (0:03.27 / tick 13)'),
    ('l2_ext', 'map1', 269, 'photo_l2_013.png', 128, 192, 0, 'roi cấp 2 duỗi (0:08.97 / tick 13)'),
    ('l3_ext_c3', 'map1', 933, 'photo_l3_014.png', 128, 208, 0, 'roi cấp 3 duỗi màu xanh (0:31.1 / tick 14)'),
    ('l3_ext_c1', 'map1', 936, 'photo_l3_016.png', 128, 208, 0, 'roi cấp 3 duỗi màu cam (0:31.2 / tick 16)'),
    ('l3_ca_c4', 'map1', 2777, 'photo_l3ca_015.png', 48, 112, 0, 'roi cấp 3 duỗi màu đỏ Crypt A (1:32.57 / tick 15)'),
    ('air', 'map1', 727, 'photo_air_018.png', 75, 208, 0, 'roi trên không ở đỉnh nhảy (0:24.23 / tick 18)'),
    ('dagger', 'map1', 2798, 'photo_dagger_021.png', 48, 112, 16, 'dao bay (1:33.27 / tick 21)'),
    ('axe_a', 'map1', 6242, 'photo_axe_024.png', 143, 208, 16, 'rìu đi lên (3:28.07 / tick 24)'),
    ('axe_b', 'map1', 6246, 'photo_axe_031.png', 143, 208, 16, 'rìu gần đỉnh (3:28.2 / tick 31)'),
]


def crop(nes, x0, y0):
    out = np.zeros((H, W, 3), np.uint8)
    for yy in range(H):
        for xx in range(W):
            y, x = y0 + yy, x0 + xx
            if 0 <= y < 240 and 0 <= x < 256:
                out[yy, xx] = nes[y, x]
    return out


def main():
    rows = []
    for name, video, f, shot, sx, sy, dx, note in PAIRS:
        path = os.path.join(SHOTS, shot)
        if not os.path.exists(path):
            print('thieu', shot)
            continue
        x0, y0 = sx - W // 2 + dx, sy - H + 8
        if name.startswith('axe'):
            y0 = sy - 88   # rìu bay cao: lấy phần trên
        if name == 'air':
            y0 = sy - 88   # Simon ở đỉnh nhảy (cao 36)
        v = crop(sp.video_nes(video, f), x0, y0)
        u = crop(sp.unity_nes(path), x0, y0)
        canvas = Image.new('RGB', (W * 4 * 2 + 12, H * 4 + 18), (40, 40, 40))
        canvas.paste(sp.big(v), (0, 18))
        canvas.paste(sp.big(u), (W * 4 + 12, 18))
        d = ImageDraw.Draw(canvas)
        d.text((2, 2), 'video f%d  %s' % (f, name), fill=(255, 255, 0))
        d.text((W * 4 + 14, 2), 'Unity ' + shot[6:-4], fill=(0, 255, 255))
        out = os.path.join(SHOTS, 'pair_%s.png' % name)
        canvas.save(out)
        rows.append(canvas)
        print(os.path.basename(out))
    if rows:
        allw = rows[0].width
        sheet = Image.new('RGB', (allw, sum(r.height + 4 for r in rows)), (0, 0, 0))
        y = 0
        for r in rows:
            sheet.paste(r, (0, y))
            y += r.height + 4
        sheet.save(os.path.join(SHOTS, 'pairs_all.png'))
        print('pairs_all.png')


if __name__ == '__main__':
    main()
