"""CVM2 10 — dò nến tường trên ảnh bản đồ wiki NỬA tỉ lệ (StrategyWiki Castlevania_Stage_{4,5,6}.png: 1 px ảnh = 2 px NES) để đối
chiếu số lượng/vị trí nến đo từ video (props10.py). Ảnh wiki khác bảng màu + mờ ⇒ không khớp mẫu màu được; dò dấu hiệu
thân nến thu nhỏ 2×: 2 cột hồng (wiki ≈ (128,50,94)) cách 2 px, cao ≥ 2 hàng.
Ghép ảnh → section (planner MAP2_WIKI_DATA §2): St4 = TEB trên (x 0..255, y 0..87) + TEA dưới-phải (x 127..254, y 88..175);
St5 = TowerHallB trên + TowerHallA dưới (384 rộng); St6 = Medusa trên + TowerHallC dưới. Hàng ảnh h ↔ hàng vùng sheet r = 2h + 16
(sân chơi NES y 48..223 = r 16..191).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/wiki10.py
  → in danh sách nến wiki theo section (x tâm, y đáy section — XẤP XỈ ±4 px: ảnh wiki thu nhỏ + bảng màu khác) + ClaudeNotes/cvm2/_cache/10/wiki_candles.csv
Ảnh wiki ở ClaudeNotes/cvm2/_cache/wiki_img/ (KHÔNG commit — bản quyền).
"""
import csv, os, sys
import numpy as np
from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
IMG = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'wiki_img')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '10')
ART = os.path.join(ROOT, 'Assets', 'Data', 'Art', 'Object')
# (ảnh, section, x0 ảnh, y0 ảnh, rộng ảnh, cao ảnh)
PARTS = [(4, 'TowerEntranceB', 0, 0, 255, 88), (4, 'TowerEntranceA', 127, 88, 128, 88),
         (5, 'TowerHallB', 0, 0, 384, 88), (5, 'TowerHallA', 0, 88, 384, 88),
         (6, 'Medusa', 0, 0, 384, 88), (6, 'TowerHallC', 0, 88, 384, 88)]


def pink(px):
    r, g, b = px[..., 0], px[..., 1], px[..., 2]
    return (r >= 100) & (b >= 70) & (g <= 80) & (b <= r)


def main():
    rows = []
    for st, sec, x0, y0, W, H in PARTS:
        im = np.asarray(Image.open(os.path.join(IMG, 'Castlevania_Stage_%d.png' % st)).convert('RGB')).astype(int)
        sub = im[y0:y0 + H, x0:x0 + W]
        m = pink(sub)
        # thân nến = 2 cột hồng cách 2 px, cao >= 2 hàng (candle_0/1 thu nhỏ 2x: cột 1 và 5 của sprite 8 px)
        hits = []
        for yy in range(H - 1):
            for xx in range(W - 2):
                if m[yy, xx] and m[yy + 1, xx] and m[yy, xx + 2] and m[yy + 1, xx + 2]:
                    if not any(abs(yy - a) <= 4 and abs(xx - b) <= 3 for a, b in hits):
                        hits.append((yy, xx))
        for yy, xx in sorted(hits, key=lambda p: p[1]):
            # cột hồng trái = cột 1 sprite (x tâm − 3 NES) ⇒ x tâm NES = 2*(x0 + xx) + 2 + 3 − 1; đáy sprite = hàng thân trên + 6 (ảnh) ⇒ NES
            gx = 2 * (x0 + xx) + 4
            if sec == 'TowerEntranceA':
                gx -= 2 * 127
            ybot = 2 * (yy + 6) + 16          # hàng vùng sheet đáy sprite (= y section)
            rows.append((st, sec, gx, ybot))
    with open(os.path.join(OUT, 'wiki_candles.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['stage', 'section', 'x_approx', 'y_approx'])
        w.writerows(rows)
    from collections import Counter
    print(Counter(r[1] for r in rows))
    for r in rows:
        print(r)


if __name__ == '__main__':
    main()
