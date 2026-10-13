"""CVGP 14 — ghép ảnh so sánh Phantom Bat: khung video (lấy mẫu lưới NES) cạnh ảnh Play Unity cùng thời điểm TƯƠNG ĐỐI
(BossProbe.ShotDir ⇒ ClaudeNotes/cvgp/_cache/shots/14/<kịch bản>_r<tick>.png, Game view 1920×1080 ⇒ vùng NES 1024×960 tại (448,60)).
Mốc: wake_video — dơi thức ở Unity tick 17 ↔ video boss f65.5 (khung cánh đầu f66) và Map_1 f5983.5; death_video — boss hạ ở Unity
tick 9 ↔ Map_1 f6305.5 (+3000 f6305, lửa từ f6306). 1 frame video = 60.0988/30 tick.
Mỗi cặp: video trái / Unity phải, cùng vùng màn NES (camera cùng cx 512). Phần NGẪU NHIÊN (đích lượn) ghi rõ ở chú thích.
Chạy (từ gốc repo): python -P -E Tools/cvgp/boss_pairs.py ⇒ ClaudeNotes/cvgp/_cache/shots/14/pairs_<nhóm>.png
"""
import os, sys
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '14')
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import simon_pairs as sp  # noqa: E402
import enemy_pairs as ep  # noqa: E402

TPF = 60.0988 / 30
ROOM = (64, 48, 192, 176)        # nửa phải phòng boss (trần → sàn)
WAKE0, WAKE_BOSS, WAKE_MAP1 = 17, 65.5, 5983.5
DEATH0, DEATH_MAP1 = 9, 6305.5


def vf(base, rel, rel0):
    return int(round(base + (rel - rel0) / TPF))


GROUPS = {
    'wake_boss': [(('boss', vf(WAKE_BOSS, r, WAKE0)), 'wake_video_r%03d' % r, ROOM, note) for r, note in [
        (33, 'thuc +16: luon tai cho'), (51, '+34: bat dau lao'), (67, '+50: giua cu lao dau'), (83, '+66: lao'),
        (105, '+88: day cu lao dau'), (137, '+120: luon cham sang trai'), (177, '+160: luon cham'),
        (217, '+200: video da toi dich (111,124); Unity dich NGAU NHIEN x')]],
    'wake_map1': [(('map1', vf(WAKE_MAP1, r, WAKE0)), 'wake_video_r%03d' % r, ROOM, note) for r, note in [
        (51, '+34'), (83, '+66'), (105, '+88: day cu lao dau'), (137, '+120 (Map_1 luon xa hon toi x 26 — dich ngau nhien)')]],
    'death_map1': [(('map1', vf(DEATH_MAP1, r, DEATH0)), 'death_video_r%03d' % r, area, note) for r, area, note in [
        (11, (168, 104, 88, 96), '+2: lua khung 0'), (19, (168, 104, 88, 96), '+10: khung 1'),
        (29, (168, 104, 88, 96), '+20: khung 2'), (39, (168, 104, 88, 96), '+30: khung 0'),
        (57, (168, 104, 88, 96), '+48: khung 2 (lan 2)'), (159, (88, 104, 88, 120), '+150: qua cau sap hien'),
        (165, (88, 104, 88, 120), '+156: qua cau roi'), (175, (88, 104, 88, 120), '+166: qua cau roi')]],
}


def main():
    total = 0
    for g, pairs in GROUPS.items():
        rows = []
        for (video, f), shot, (x0, y0, w, h), note in pairs:
            path = os.path.join(SHOTS, shot + '.png')
            if not os.path.exists(path):
                print('thieu', shot)
                continue
            k = 3 if w > 128 else 4
            v = ep.crop(sp.video_nes(video, f), x0, y0, w, h)
            u = ep.crop(sp.unity_nes(path), x0, y0, w, h)
            canvas = Image.new('RGB', (w * k * 2 + 12, h * k + 18), (40, 40, 40))
            canvas.paste(sp.big(v, k), (0, 18))
            canvas.paste(sp.big(u, k), (w * k + 12, 18))
            d = ImageDraw.Draw(canvas)
            d.text((2, 2), '%s f%d' % (video, f), fill=(255, 255, 0))
            d.text((w * k + 14, 2), 'Unity ' + shot + ' - ' + note, fill=(0, 255, 255))
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
