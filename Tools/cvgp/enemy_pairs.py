"""CVGP 13 — ghép ảnh so sánh enemy Block 1: khung video Map_1 (lấy mẫu lưới NES) đặt cạnh ảnh Play Unity cùng pha
(EnemyProbe.ShotDir ⇒ ClaudeNotes/cvgp/_cache/shots/13/<kịch bản>_r<tick>.png, Game view 1920×1080 ⇒ vùng NES 1024×960 tại (448,60)).
Mỗi nhóm: (frame video, ảnh Unity, vùng NES (x0,y0,w,h) — cùng toạ độ màn ở 2 bên vì section/camera giống video, ghi chú).
Chạy (từ gốc repo): python -P -E Tools/cvgp/enemy_pairs.py ⇒ ClaudeNotes/cvgp/_cache/shots/13/pairs_<nhóm>.png
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '13')
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import simon_pairs as sp  # noqa: E402

FLOOR = (120, 152, 136, 64)    # sàn EH/PB/CA dưới: nửa phải màn
GROUPS = {
    # Zombie EH: video spawn f600.5 ↔ Unity tick 0 (1 frame video = 2 tick)
    'ghoul': [(620, 'ghoul_spawner_r040', FLOOR, 'Zombie #1 x≈213 / #2 vừa sinh'),
              (640, 'ghoul_spawner_r080', FLOOR, '3 Zombie cách ~28 px'),
              (660, 'ghoul_spawner_r120', (64, 152, 192, 64), '3 Zombie đi trái 0.875 px/tick')],
    # Zombie PB rơi khỏi gờ: video f5127 ↔ Unity tick 0
    'ghoul_pb': [(5202, 'ghoul_pb_video_r151', (96, 120, 64, 104), 'Zombie gờ trên bắt đầu rơi ở x 127'),
                 (5207, 'ghoul_pb_video_r161', (96, 120, 64, 104), 'đang rơi thẳng đứng'),
                 (5215, 'ghoul_pb_video_r177', (96, 120, 64, 104), 'đáp sàn dưới')],
    # Báo #1: video bật dậy f1263 ↔ Unity tick bật (69) — camera bám Simon cùng x
    'panther': [(1271, 'panther_eh1_r086', (96, 120, 144, 96), 'chạy/cất cánh (bật + 16 tick)'),
                (1281, 'panther_eh1_r106', (40, 120, 144, 96), 'giữa cú nhảy (bật + 36)'),
                (1287, 'panther_eh1_r118', (24, 140, 144, 80), 'sắp đáp (bật + 48)')],
    # Dơi CA: video sinh f2757 (ngoại suy về pivot 248 — như enemy_compare) ↔ Unity tick sinh (RNG 8–28); ảnh ở sinh + 11/31/51/71
    'bat': [(2763, 'bat_ca_r036', (136, 72, 120, 72), 'dơi + 10 tick'),
            (2773, 'bat_ca_r056', (136, 72, 120, 72), 'dơi + 30 tick'),
            (2783, 'bat_ca_r076', (120, 72, 136, 72), 'dơi + 50 tick (đáy sóng)'),
            (2793, 'bat_ca_r096', (100, 72, 136, 72), 'dơi + 70 tick')],
    # Fish Man CB: video trồi ~f3514 ↔ Unity tick 0
    'fishman': [(3520, 'fishman_video_r013', (0, 64, 200, 176), 'trồi + bọt (tick 13)'),
                (3534, 'fishman_video_r041', (0, 64, 200, 176), 'gần đỉnh (tick 41)'),
                (3551, 'fishman_video_r075', (0, 64, 200, 176), 'đã đáp lối đi (tick 75)'),
                (3564, 'fishman_video_r101', (0, 64, 200, 176), 'đi về phía Simon (tick 101)')],
}


def crop(nes, x0, y0, w, h):
    out = np.zeros((h, w, 3), np.uint8)
    for yy in range(h):
        y = y0 + yy
        if not 0 <= y < 240:
            continue
        xs = slice(max(0, x0), min(256, x0 + w))
        out[yy, xs.start - x0:xs.stop - x0] = nes[y, xs]
    return out


def main():
    total = 0
    for g, pairs in GROUPS.items():
        rows = []
        for f, shot, (x0, y0, w, h), note in pairs:
            path = os.path.join(SHOTS, shot + '.png')
            if not os.path.exists(path):
                print('thieu', shot)
                continue
            k = 3 if w > 128 else 4
            v = crop(sp.video_nes('map1', f), x0, y0, w, h)
            u = crop(sp.unity_nes(path), x0, y0, w, h)
            canvas = Image.new('RGB', (w * k * 2 + 12, h * k + 18), (40, 40, 40))
            canvas.paste(sp.big(v, k), (0, 18))
            canvas.paste(sp.big(u, k), (w * k + 12, 18))
            d = ImageDraw.Draw(canvas)
            d.text((2, 2), 'video f%d' % f, fill=(255, 255, 0))
            d.text((w * k + 14, 2), 'Unity ' + shot + ' - ' + note.encode('ascii', 'ignore').decode(), fill=(0, 255, 255))
            rows.append(canvas)
            total += 1
        if rows:
            wmax = max(r.width for r in rows)
            sheet = Image.new('RGB', (wmax, sum(r.height + 4 for r in rows)), (0, 0, 0))
            y = 0
            for r in rows:
                sheet.paste(r, (0, y))
                y += r.height + 4
            sheet.save(os.path.join(SHOTS, 'pairs_%s.png' % g))
            print('pairs_%s.png' % g, len(rows))
    print('tong', total, 'cap')


if __name__ == '__main__':
    main()
