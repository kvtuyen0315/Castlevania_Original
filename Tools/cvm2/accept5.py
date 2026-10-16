# -*- coding: utf-8 -*-
"""accept5.py - CVM2 05: so ảnh địa hình Map 2 trong Unity với sheet (đã vá) và với video Map_2 cùng cx.

Chạy từ gốc repo:  python -P -E Tools/cvm2/accept5.py <lệnh> …
  terrain <thư mục ảnh terrain2_*>     ảnh nền Unity (sprite + HUD ẩn — Cvm2TerrainProbe.StartShots) ↔ vùng sheet Level 2 ĐÃ VÁ teal
                                         (Tools/cvm2/build_tileset2.py) cùng cx: NES y 32–223 = sheet hàng 0–191; NES y 0–31 / 224–239 phải đen
  tilesets                             đối chiếu 81 tile 8×8 của level2_tiles.png với cột LEVEL2 của sheet Tilesets THEO HÌNH (như
                                         Tools/cvm2/cmp_tilesets.py CVM2 01) ⇒ số tile có / không có
  pairs <thư mục ảnh terrain2_*> <ra>  cặp video Map_2 (bộ đệm lưới NES CVM2 02) ↔ Unity cùng section + cx: đổi màu sheet→video (fg2 LUT)
                                         rồi đo |hiệu| ở NES y 48–223 (dưới HUD) ⇒ % pixel lệch > 48 (gồm sprite động video) + ảnh ghép xem
Unity: Game view 1920×1080 ⇒ vùng game 1024×960 tại (448,60), 4 px màn / px NES ⇒ lấy mẫu tâm ô (như Tools/cvgp/accept16.py).
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
import build_tileset2 as T2  # noqa: E402

# cặp video ↔ Unity (frame Map_2, section, cx) — camera đứng yên ±2 frame (ref/data/map2_camera.csv), trải đều thời gian từng section
PAIRS = [
    (299, 'TowerEntranceA', 0), (521, 'TowerEntranceA', 0),
    (957, 'TowerEntranceB', 256), (1193, 'TowerEntranceB', 153), (1328, 'TowerEntranceB', 76), (1462, 'TowerEntranceB', 0),
    (1763, 'TowerHallA', 512), (1964, 'TowerHallA', 492), (2169, 'TowerHallA', 312), (2693, 'TowerHallA', 0),
    (2803, 'TowerHallB', 0), (3167, 'TowerHallB', 224), (3441, 'TowerHallB', 512), (3807, 'TowerHallB', 497),
    (4481, 'TowerHallC', 512), (4907, 'TowerHallC', 239), (5188, 'TowerHallC', 0), (5559, 'TowerHallC', 120),
    (5926, 'Medusa', 512), (6661, 'Medusa', 114), (6937, 'Medusa', 0),
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
        m = re.match(r'terrain2_(\w+?)_cx(\d+)\.png$', f)
        if m:
            out.append((f, m.group(1), int(m.group(2))))
    return out


def cmd_terrain(udir):
    regs = T2.patched_regions()
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


def cmd_pairs(udir, out):
    import fg2
    os.makedirs(out, exist_ok=True)
    mm = np.load(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work', 'map2_nes.npy'), mmap_mode='r')
    have = {(sec, cx): f for f, sec, cx in shots(udir)}
    lines = []
    tiles = []
    for fr, sec, cx in PAIRS:
        uf = have.get((sec, cx))
        if uf is None:
            lines.append('f%d %s cx %d: THIEU anh Unity' % (fr, sec, cx))
            continue
        u = unity_nes(os.path.join(udir, uf))[48:224].astype(float)        # NES y 48..223
        ulut = fg2.apply_lut(u)                                               # màu sheet ⇒ màu video
        v = np.asarray(mm[fr])[40:216].astype(float)                          # bộ đệm hàng 0 = NES y 8
        d = np.abs(v - ulut).max(axis=2)
        bad = d > 48
        pct = 100.0 * bad.mean()
        mean = float(np.abs(v - ulut).mean())
        lines.append('f%-5d %.2fs %-15s cx %3d  TB|hieu| %5.1f  pixel lech>48: %5.2f %%  (Unity %s)' % (fr, fr / 30.0, sec, cx, mean, pct, uf))
        # ảnh ghép: video | Unity (LUT) | lệch đỏ
        z = 2
        vi = Image.fromarray(np.clip(v, 0, 255).astype(np.uint8)).resize((512, 352), Image.NEAREST)
        ui = Image.fromarray(np.clip(ulut, 0, 255).astype(np.uint8)).resize((512, 352), Image.NEAREST)
        dm = np.clip(v * 0.35, 0, 255).astype(np.uint8)
        dm[bad] = (255, 0, 0)
        di = Image.fromarray(dm).resize((512, 352), Image.NEAREST)
        tile = Image.new('RGB', (512 * 3 + 8, 352 + 16), (40, 40, 40))
        tile.paste(vi, (0, 16)); tile.paste(ui, (516, 16)); tile.paste(di, (1032, 16))
        ImageDraw.Draw(tile).text((4, 2), 'video f%d %.2fs | Unity %s cx %d | lech>48 %.2f%% (do = sprite/khoi vo/lech)' % (fr, fr / 30.0, sec, cx, pct), fill=(255, 255, 0))
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


def cmd_tilesets():
    ts = np.asarray(Image.open(os.path.join(ROOT, 'Art_Source', 'spriters-resource', 'tilesets.png')).convert('RGB'))
    tl = np.asarray(Image.open(os.path.join(ROOT, T2.PNG_REL)).convert('RGBA'))

    def canon(t):
        m = {}
        lab = []
        for c in map(tuple, t.reshape(-1, 3)):
            if c not in m:
                m[c] = len(m)
            lab.append(m[c])
        return bytes(lab)
    GX, GY, STEP, CELL, NC, NR = 408, 92, 33, 32, 5, 8      # lưới cột LEVEL2 (CVM2 01)
    keys = set()
    for j in range(NR):
        for i in range(NC):
            c = ts[GY + STEP * j:GY + STEP * j + CELL, GX + STEP * i:GX + STEP * i + CELL]
            if len(set(map(tuple, c.reshape(-1, 3)))) <= 1:
                continue
            for qy in range(0, 32, 8):
                for qx in range(0, 32, 8):
                    keys.add(canon(c[qy:qy + 8, qx:qx + 8]))
    n = (tl.shape[0] // 8) * 16
    hit, miss = [], []
    for k in range(n):
        r, c = divmod(k, 16)
        t = tl[r * 8:(r + 1) * 8, c * 8:(c + 1) * 8]
        if t[..., 3].max() == 0:
            continue
        (hit if canon(t[..., :3]) in keys else miss).append(k)
    print('tile Level 2 co trong Tilesets (theo hinh): %d / %d; khong co: %s' % (len(hit), len(hit) + len(miss), miss))


if __name__ == '__main__':
    c = sys.argv[1]
    if c == 'tilesets':
        cmd_tilesets()
    elif c == 'terrain':
        cmd_terrain(sys.argv[2])
    elif c == 'pairs':
        cmd_pairs(sys.argv[2], sys.argv[3])
