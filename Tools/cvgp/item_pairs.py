"""CVGP 11 — ghép ảnh so sánh item / vật chứa: khung video Map_1 (lấy mẫu lưới NES) đặt cạnh ảnh Play Unity cùng pha
(ItemProbe.Shots ⇒ ClaudeNotes/cvgp/_cache/shots/11/<cảnh>_r<tick>.png, Game view 1920×1080 ⇒ vùng NES 1024×960 tại (448,60)).
Mỗi cặp cắt 1 vùng NES riêng cho video và Unity (vật thử ở Arena khác chỗ video), phóng ×k, xếp [video | Unity].
Chạy (từ gốc repo): python -P -E Tools/cvgp/item_pairs.py  ⇒ ClaudeNotes/cvgp/_cache/shots/11/pair_*.png + pairs_<nhóm>.png
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '11')
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import simon_pairs as sp  # noqa: E402

# nhóm: [(frame video, ảnh Unity, (x0,y0) video, (x0,y0) Unity, (w,h), phóng, ghi chú)]
TORCH = ((68, 136), (136, 152), (56, 64), 4)
WHIP = ((100, 146), (100, 162), (56, 50), 4)
HEART = ((143, 55), (152, 175), (48, 48), 4)
POP = ((112, 168), (122, 176), (48, 40), 4)
FULL = ((0, 40), (0, 40), (256, 184), 2)
BLOCK = ((156, 150), (200, 102), (56, 72), 4)
SECRET = ((72, 100), (144, 179), (48, 40), 4)
GROUPS = {
    'torch': [(100, 'torch_r16', TORCH, 'vỡ: tia lửa + lửa khung 0 (+1 tick)'),
              (106, 'torch_r28', TORCH, 'lửa khung 1 (+12)'),
              (109, 'torch_r34', TORCH, 'lửa khung 2 (+18)'),
              (110, 'torch_r35', TORCH, 'item hiện (+20)'),
              (112, 'torch_r39', TORCH, 'item rơi (+24)'),
              (113, 'torch_r41', TORCH, 'item rơi (+26)'),
              (116, 'torch_r47', TORCH, 'item nằm sàn (+32)')],
    'whip': [(194, 'whip_r66', WHIP, 'nhặt roi: đỏ'),
             (195, 'whip_r68', WHIP, 'nhặt roi: tím'),
             (198, 'whip_r70', WHIP, 'nhặt roi: xanh'),
             (200, 'whip_r72', WHIP, 'nhặt roi: thường')],
    'heart': [(4046, 'heart_r35', HEART, 'tim nhỏ hiện'),
              (4051, 'heart_r45', HEART, 'tim nhỏ +10 tick'),
              (4057, 'heart_r57', HEART, 'tim nhỏ +22 tick (lệch phải max)'),
              (4063, 'heart_r69', HEART, 'tim nhỏ +34 tick (quay lại)')],
    'popup': [(888, 'bags_r6', POP, 'chữ 100 (+2 tick sau nhặt)')],
    'rosary': [(2252, 'rosary_r5', FULL, 'Rosary: nền đen ⇒ xám'),
               (2255, 'rosary_r7', FULL, 'Rosary: pha thường')],
    'block': [(2988, 'block_r26', BLOCK, 'khối vỡ: mảnh + item ngay (+1)'),
                            (2992, 'block_r34', BLOCK, '+9'),
              (2995, 'block_r40', BLOCK, '+15')],
    'secret': [(4213, 'secret_r49', SECRET, 'túi 1000 trồi giữa chừng (+26)'),
               (4226, 'secret_r75', SECRET, 'túi trồi xong (+52)')],
    'potion': [(5376, 'potion_r60', ((95, 96), (96, 150), (64, 64), 4), 'tàng hình pha 1: vẽ thường'),
               (5410, 'potion_r130', ((60, 130), (96, 150), (64, 64), 4), 'pha 2: ẩn'),
               (5445, 'potion_r164', ((18, 150), (96, 150), (64, 64), 4), 'pha 2: lóe white')],
}


def crop(nes, x0, y0, w, h):
    out = np.zeros((h, w, 3), np.uint8)
    for yy in range(h):
        y = y0 + yy
        if not 0 <= y < 240:
            continue
        for xx in range(w):
            x = x0 + xx
            if 0 <= x < 256:
                out[yy, xx] = nes[y, x]
    return out


def main():
    for g, pairs in GROUPS.items():
        rows = []
        for f, shot, spec, note in pairs:
            path = os.path.join(SHOTS, shot + '.png')
            if not os.path.exists(path):
                print('thieu', shot)
                continue
            (vx, vy), (ux, uy), (w, h), k = spec
            v = crop(sp.video_nes('map1', f), vx, vy, w, h)
            u = crop(sp.unity_nes(path), ux, uy, w, h)
            canvas = Image.new('RGB', (w * k * 2 + 12, h * k + 18), (40, 40, 40))
            canvas.paste(sp.big(v, k), (0, 18))
            canvas.paste(sp.big(u, k), (w * k + 12, 18))
            d = ImageDraw.Draw(canvas)
            d.text((2, 2), 'video f%d' % f, fill=(255, 255, 0))
            d.text((w * k + 14, 2), 'Unity ' + shot, fill=(0, 255, 255))
            canvas.save(os.path.join(SHOTS, 'pair_%s.png' % shot))
            rows.append(canvas)
        if rows:
            wmax = max(r.width for r in rows)
            sheet = Image.new('RGB', (wmax, sum(r.height + 4 for r in rows)), (0, 0, 0))
            y = 0
            for r in rows:
                sheet.paste(r, (0, y))
                y += r.height + 4
            sheet.save(os.path.join(SHOTS, 'pairs_%s.png' % g))
            print('pairs_%s.png' % g, len(rows))


if __name__ == '__main__':
    main()
