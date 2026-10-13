"""CVGP 12 — so dải HUD (NES y 16–47) ảnh chụp Unity ↔ ảnh lưới NES của video (ClaudeNotes/cvgp/ref/nes/*_nes.png).

Chạy từ gốc repo:  python -P -E Tools/cvgp/hud_pairs.py
Ảnh Unity: ClaudeNotes/cvgp/_cache/shots/12/hud_<k>.png (Game view 1920x1080 ⇒ vùng game (448,60,1024,960), 4 px màn / px NES).
So mặt nạ "sáng" (max kênh > 60) từng pixel NES theo vùng; in số pixel lệch + ghép ảnh (trên = video, dưới = Unity, giữa = lệch đỏ).
"""
import os
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '12')
REF = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', 'ref', 'nes')
PAIRS = [
    (1, 'm1_00m02s_stage01_castle_entrance_bat_dau', 'SCORE 0 TIME 0300 ST01 tim 05, chua vu khi'),
    (2, 'm1_00m20s_entrance_hall_zombie', 'TIME 0287 tim 15 dao'),
    (3, 'm1_01m32s_stage02_crypt_a_vampire_bat', 'SCORE 4200 TIME 0226 ST02 tim 37 dao'),
    (4, 'm1_02m52s_stage03_hanh_lang_phantom_bat', 'SCORE 11100 TIME 0165 ST03 tim 60 dao'),
    (5, 'm1_03m19s_doi_thuc_day_nhat_II', 'SCORE 11900 TIME 0139 tim 76 riu + II'),
    (6, 'm1_03m25s_riu_trung_doi_E_giam', 'TIME 0134 tim 78 ENEMY 15'),
    (7, 'm1_03m30s_doi_chet_khoi_lua', 'SCORE 14900 TIME 0129 tim 72 ENEMY 0 (vach rong)'),
    (8, 'm1_03m45s_tally_dem_tim', 'SCORE 19670 TIME 0000 tim 37'),
]
REGIONS = [
    ('dong trang thai', 0, 16, 256, 24),
    ('nhan PLAYER/ENEMY', 0, 24, 56, 40),
    ('thanh mau', 56, 24, 124, 40),
    ('khung + icon vu khi', 124, 24, 164, 48),
    ('tim + P', 164, 24, 204, 40),
    ('icon II/III', 204, 24, 232, 40),
]


def unity_nes(path):
    im = Image.open(path).convert('RGB')
    w, h = im.size
    s = 4 if (w, h) == (1920, 1080) else None
    if s is None:
        raise SystemExit('anh Unity phai 1920x1080: %s %s' % (path, im.size))
    ox, oy = 448, 60
    out = Image.new('RGB', (256, 240))
    px = out.load()
    for y in range(240):
        for x in range(256):
            px[x, y] = im.getpixel((ox + s * x + s // 2, oy + s * y + s // 2))
    return out


def lit(p):
    return max(p) > 60


def main():
    total = {}
    rows = []
    for k, ref, label in PAIRS:
        up = os.path.join(SHOTS, 'hud_%d.png' % k)
        if not os.path.exists(up):
            print('thieu', up)
            continue
        u = unity_nes(up)
        v = Image.open(os.path.join(REF, ref + '_nes.png')).convert('RGB')
        line = []
        diff = Image.new('RGB', (256, 32))
        dp = diff.load()
        for name, x0, y0, x1, y1 in REGIONS:
            n = 0
            for y in range(y0, y1):
                for x in range(x0, x1):
                    a, b = lit(u.getpixel((x, y))), lit(v.getpixel((x, y)))
                    if a != b:
                        n += 1
                        dp[x, y - 16] = (255, 0, 0)
            line.append(n)
            total[name] = total.get(name, 0) + n
        print('#%d %-48s %s' % (k, label, '  '.join('%s=%d' % (r[0], n) for r, n in zip(REGIONS, line))))
        strip = Image.new('RGB', (256, 32 * 3 + 4), (40, 40, 40))
        strip.paste(v.crop((0, 16, 256, 48)), (0, 0))
        strip.paste(diff, (0, 34))
        strip.paste(u.crop((0, 16, 256, 48)), (0, 68))
        rows.append(strip)
    if rows:
        sheet = Image.new('RGB', (256, len(rows) * 104), (0, 0, 0))
        for i, r in enumerate(rows):
            sheet.paste(r, (0, i * 104))
        sheet = sheet.resize((sheet.width * 3, sheet.height * 3), Image.NEAREST)
        out = os.path.join(SHOTS, 'hud_pairs.png')
        sheet.save(out)
        print('TONG lech theo vung:', total)
        print('anh ghep:', out)


if __name__ == '__main__':
    main()
