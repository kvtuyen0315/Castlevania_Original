# -*- coding: utf-8 -*-
"""accept3.py - CVM3 05: so ảnh địa hình Map 3 trong Unity với sheet (WalkwayB đã nới trái 112 px) và với video Map_3 cùng cx.
Khuôn Tools/cvm2/accept5.py (CVM2 05).

Chạy từ gốc repo:  python -P -E Tools/cvm3/accept3.py <lệnh> …
  terrain <thư mục ảnh terrain3_*>     ảnh nền Unity (sprite + HUD ẩn — Cvm3TerrainProbe.StartShots) ↔ vùng sheet Level 3 (build_tileset3.padded_regions)
                                         cùng cx: NES y 32–223 = sheet hàng 0–191; NES y 0–31 / 224–239 phải đen
  pairs <thư mục ảnh terrain3_*> <ra>  cặp video Map_3 (bộ đệm lưới NES CVM3 02) ↔ Unity cùng section + cx (WalkwayB: cx video = cx Unity − 112):
                                         đổi màu sheet→video (fg3 LUT) rồi đo |hiệu| ở NES y 48–223 (dưới HUD) ⇒ % pixel lệch > 48 + ảnh ghép xem
Unity: Game view 1920×1080 ⇒ vùng game 1024×960 tại (448,60), 4 px màn / px NES ⇒ lấy mẫu tâm ô (như Tools/cvm2/accept5.py).
"""
import os
import re
import sys

import numpy as np
from PIL import Image, ImageDraw

TOOLS = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(TOOLS))
sys.path.insert(0, TOOLS)
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
sys.stdout.reconfigure(encoding='utf-8')
import build_tileset3 as T3  # noqa: E402

# cặp video ↔ Unity (frame Map_3, section, cx UNITY) — camera video đứng yên ±3 frame (ref/data/map3_camera.csv), trải đều thời gian từng section.
# WalkwayB: cx Unity = cx video + 112 (f3484 cx −112 chỉ đứng 3 frame — mút trái cầu)
PAIRS = [
    (289, 'WalkwayEntranceA', 512), (525, 'WalkwayEntranceA', 405), (657, 'WalkwayEntranceA', 277), (892, 'WalkwayEntranceA', 169),
    (1075, 'WalkwayEntranceA', 0),
    (1244, 'WalkwayEntranceB', 0), (1406, 'WalkwayEntranceB', 52), (1699, 'WalkwayEntranceB', 296), (2199, 'WalkwayEntranceB', 421),
    (2378, 'WalkwayEntranceB', 512),
    (2704, 'WalkwayA', 0), (3097, 'WalkwayA', 512),
    (3484, 'WalkwayB', 0), (3363, 'WalkwayB', 49), (3565, 'WalkwayB', 117), (3798, 'WalkwayB', 297), (4069, 'WalkwayB', 551), (4253, 'WalkwayB', 624),
    (4601, 'MummyMan', 0), (4822, 'MummyMan', 228), (5216, 'MummyMan', 636), (5492, 'MummyMan', 993), (5905, 'MummyMan', 1263),
    (6691, 'MummyMan', 1280),
]


def unity_nes(path):
    im = np.asarray(Image.open(path).convert('RGB'))
    if im.shape[:2] != (1080, 1920):
        raise SystemExit('anh Unity phai 1920x1080: %s %s' % (path, im.shape))
    ys = 60 + 4 * np.arange(240) + 2
    xs = 448 + 4 * np.arange(256) + 2
    return im[ys][:, xs].astype(np.int16)


def shots(udir):
    out = []
    for f in sorted(os.listdir(udir)):
        m = re.match(r'terrain3_(\w+?)_cx(\d+)\.png$', f)
        if m:
            out.append((f, m.group(1), int(m.group(2))))
    return out


def cmd_terrain(udir):
    regs = T3.padded_regions()
    lines = []
    total = 0
    for f, sec, cx in shots(udir):
        got = unity_nes(os.path.join(udir, f))
        exp = regs[sec][:, cx:cx + 256].astype(np.int16)
        d = np.abs(got[32:224] - exp).sum(2) > 0
        n = int(d.sum())
        outside = int((got[:32].sum(2) > 0).sum() + (got[224:].sum(2) > 0).sum())
        total += n + outside
        extra = ''
        if n:
            ys, xs = np.nonzero(d)
            extra = ' bbox NES (%d,%d)-(%d,%d)' % (xs.min(), ys.min() + 32, xs.max(), ys.max() + 32)
            Image.fromarray((d * 255).astype(np.uint8)).resize((512, 384), Image.NEAREST).save(os.path.join(udir, 'diff_' + f))
        lines.append('%-36s %6d / %d pixel lech (NES y 32-223); ngoai vung khong den: %d%s' % (f, n, d.size, outside, extra))
    lines.append('TONG pixel lech: %d tren %d anh' % (total, len(lines)))
    with open(os.path.join(udir, 'terrain.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))
    # ghép xem nhanh: mỗi ảnh Unity thu 1/2 (NES 256x240) có nhãn
    tiles = []
    for f, sec, cx in shots(udir):
        u = unity_nes(os.path.join(udir, f)).astype(np.uint8)
        t = Image.new('RGB', (256, 252), (40, 40, 40))
        t.paste(Image.fromarray(u), (0, 12))
        ImageDraw.Draw(t).text((2, 0), '%s cx %d' % (sec[:12], cx), fill=(255, 255, 0))
        tiles.append(t)
    cols = 5
    rows = (len(tiles) + cols - 1) // cols
    mont = Image.new('RGB', (cols * 260, rows * 256), (0, 0, 0))
    for i, t in enumerate(tiles):
        mont.paste(t, ((i % cols) * 260, (i // cols) * 256))
    mont.save(os.path.join(udir, 'terrain_montage.png'))


def cmd_pairs(udir, out):
    import fg3
    os.makedirs(out, exist_ok=True)
    mm = np.load(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work', 'map3_nes.npy'), mmap_mode='r')
    have = {(sec, cx): f for f, sec, cx in shots(udir)}
    lines = []
    tiles = []
    for fr, sec, cx in PAIRS:
        uf = have.get((sec, cx))
        if uf is None:
            lines.append('f%d %s cx %d: THIEU anh Unity' % (fr, sec, cx))
            continue
        u = unity_nes(os.path.join(udir, uf))[48:224].astype(float)        # NES y 48..223
        ulut = fg3.apply_lut(u, 'map3')                                       # màu sheet ⇒ màu video
        v = np.asarray(mm[fr])[40:216].astype(float)                          # bộ đệm hàng 0 = NES y 8
        d = np.abs(v - ulut).max(axis=2)
        bad = d > 48
        pct = 100.0 * bad.mean()
        mean = float(np.abs(v - ulut).mean())
        cxv = cx - T3.pad_of(sec)
        lines.append('f%-5d %6.2fs %-17s cx Unity %4d (video %4d)  TB|hieu| %5.1f  pixel lech>48: %5.2f %%  (Unity %s)' % (
            fr, fr / 30.0, sec, cx, cxv, mean, pct, uf))
        vi = Image.fromarray(np.clip(v, 0, 255).astype(np.uint8)).resize((512, 352), Image.NEAREST)
        ui = Image.fromarray(np.clip(ulut, 0, 255).astype(np.uint8)).resize((512, 352), Image.NEAREST)
        dm = np.clip(v * 0.35, 0, 255).astype(np.uint8)
        dm[bad] = (255, 0, 0)
        di = Image.fromarray(dm).resize((512, 352), Image.NEAREST)
        tile = Image.new('RGB', (512 * 3 + 8, 352 + 16), (40, 40, 40))
        tile.paste(vi, (0, 16)); tile.paste(ui, (516, 16)); tile.paste(di, (1032, 16))
        ImageDraw.Draw(tile).text((4, 2), 'video f%d %.2fs | Unity %s cx %d (video cx %d) | lech>48 %.2f%% (do = sprite/khoi vo/lech)' % (
            fr, fr / 30.0, sec, cx, cxv, pct), fill=(255, 255, 0))
        tiles.append(tile)
    for k in range(0, len(tiles), 4):
        part = tiles[k:k + 4]
        sheet = Image.new('RGB', (part[0].width, sum(t.height + 4 for t in part)), (0, 0, 0))
        y = 0
        for t in part:
            sheet.paste(t, (0, y))
            y += t.height + 4
        sheet.save(os.path.join(out, 'pairs_%d.png' % (k // 4)))
    with open(os.path.join(out, 'pairs.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    c = sys.argv[1]
    if c == 'terrain':
        cmd_terrain(sys.argv[2])
    elif c == 'pairs':
        cmd_pairs(sys.argv[2], sys.argv[3])
