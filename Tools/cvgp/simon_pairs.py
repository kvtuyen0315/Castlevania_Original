"""CVGP 08 — ghép ảnh so sánh Simon: khung video Map_1/Boss (lấy mẫu lưới NES theo hiệu chuẩn MAP1_TIMELINE §1) đặt cạnh ảnh
Play Unity cùng pha (SimonProbe.RunPhoto ⇒ ClaudeNotes/cvgp/_cache/shots/08/photo_<tên>_<tick>.png, Game view 1920×1080 ⇒ vùng NES
1024×960 tại (448,60), 4 px/pixel NES). Cắt quanh Simon (cùng toạ độ màn NES), phóng ×4, ghép từng cặp: [video | Unity].
Chạy (từ gốc repo): python -P -E Tools/cvgp/simon_pairs.py  ⇒ ClaudeNotes/cvgp/_cache/shots/08/pair_*.png + jump_strip.png
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '08')
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import simon_fit as sf  # noqa: E402

GX, GY, S = 448, 60, 4   # vùng NES trong ảnh Game view

# (tên cặp, video, frame, ảnh Unity, tâm crop x màn NES, y màn NES (pivot chân), ghi chú)
PAIRS = [
    ('walk', 'map1', 407, 'photo_walk_003.png', 128, 192, 'đi: CE x 509 walk_2 (video 0:13.57 / Unity tick 3)'),
    ('jump_apex', 'map1', 1135, 'photo_jump_021.png', 128, 200, 'đỉnh nhảy co chân (video 0:37.83 / Unity t 20)'),
    ('stairs', 'map1', 1415, 'photo_stairs_029.png', 128, 190, 'giữa thang ↗ t 18 stairs_up_0 (video 0:47.17 / Unity tick 29)'),
    ('knockback', 'boss', 366, 'photo_knockback_010.png', 237, 208, 'knockback +10 px, cao 13 (video boss 12.2 s / Unity tick 10)'),
    ('crouch', 'map1', 1890, 'photo_crouch_005.png', 128, 208, 'ngồi (khựng sau rơi) x 915 (video 1:03.0 / Unity giữ ↓)'),
    ('fall', 'map1', 1884, 'photo_fall_021.png', 128, 168, 'đang rơi khỏi Bục C (video 1:02.8 / Unity tick 21)'),
]
# chuỗi nhảy: (frame video, tick Unity) cùng pha t = 3/8/10/12/14/16/27/34 (độ cao video đo bằng khớp mẫu)
JUMP = [(1126, 4), (1129, 9), (1130, 11), (1131, 13), (1132, 15), (1133, 17), (1138, 28), (1141, 35)]
W, H = 48, 72   # crop px NES


def video_nes(video, f):
    for _, nes in sf.frame_iter(video, f, f):
        full = np.zeros((240, 256, 3), float)
        full[sf.NES_Y0:sf.NES_Y0 + nes.shape[0]] = nes
        return np.clip(full, 0, 255).astype(np.uint8)


def unity_nes(path):
    im = np.asarray(Image.open(path).convert('RGB'))
    ys = GY + S * np.arange(240) + S // 2
    xs = GX + S * np.arange(256) + S // 2
    return im[ys][:, xs]


def crop(nes, cx, cy):
    x0, y0 = cx - W // 2, cy - H + 8
    out = np.zeros((H, W, 3), np.uint8)
    for yy in range(H):
        for xx in range(W):
            y, x = y0 + yy, x0 + xx
            if 0 <= y < 240 and 0 <= x < 256:
                out[yy, xx] = nes[y, x]
    return out


def big(a, k=4):
    return Image.fromarray(a).resize((a.shape[1] * k, a.shape[0] * k), Image.NEAREST)


def main():
    made = []
    for name, video, f, shot, cx, cy, note in PAIRS:
        v = crop(video_nes(video, f), cx, cy)
        u = crop(unity_nes(os.path.join(SHOTS, shot)), cx, cy)
        canvas = Image.new('RGB', (W * 4 * 2 + 12, H * 4 + 18), (40, 40, 40))
        canvas.paste(big(v), (0, 18))
        canvas.paste(big(u), (W * 4 + 12, 18))
        d = ImageDraw.Draw(canvas)
        d.text((2, 2), 'video f%d' % f, fill=(255, 255, 0))
        d.text((W * 4 + 14, 2), 'Unity ' + shot[6:-4], fill=(0, 255, 255))
        out = os.path.join(SHOTS, 'pair_%s.png' % name)
        canvas.save(out)
        made.append(out)
    # chuỗi nhảy: hàng trên video, hàng dưới Unity
    cols = len(JUMP)
    strip = Image.new('RGB', (cols * (W * 3 + 4), 2 * (H * 3 + 16)), (40, 40, 40))
    for i, (f, t) in enumerate(JUMP):
        v = crop(video_nes('map1', f), 128, 208)
        u = crop(unity_nes(os.path.join(SHOTS, 'photo_jump_%03d.png' % t)), 128, 208)
        x = i * (W * 3 + 4)
        strip.paste(big(v, 3), (x, 16))
        strip.paste(big(u, 3), (x, H * 3 + 32))
        d = ImageDraw.Draw(strip)
        d.text((x + 2, 2), 'f%d' % f, fill=(255, 255, 0))
        d.text((x + 2, H * 3 + 18), 't%d' % (t - 1), fill=(0, 255, 255))
    out = os.path.join(SHOTS, 'jump_strip.png')
    strip.save(out)
    made.append(out)
    for m in made:
        print(os.path.basename(m))


if __name__ == '__main__':
    main()
