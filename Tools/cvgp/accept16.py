"""CVGP 16 — công cụ so ảnh nghiệm thu Map 1 (Unity ↔ baseline / sheet / video).

Chạy từ gốc repo:  python -P -E Tools/cvgp/accept16.py <lệnh> …
  h8 <thư mục Unity> <thư mục baseline>          so h8_L*.png với base_L*.png (pixel đầy đủ 1920×1080) ⇒ số pixel khác + bbox vùng khác
  terrain <thư mục ảnh terrain_*>                 ảnh nền Unity (sprite ẩn) ↔ sheet Level 1 cùng cx: NES y 48–223 = sheet hàng 16–191
  bgpairs <thư mục ảnh route> [out]               cặp video ↔ Unity cùng cx (như map1_pairs 15) + ánh xạ bảng màu Unity→video (trung vị
                                                  theo từng màu sheet) rồi đo |hiệu| còn lại ⇒ % pixel nền khác > 48 ngoài vùng sprite động
Unity: Game view 1920×1080 ⇒ vùng game 1024×960 tại (448,60), 4 px màn / px NES ⇒ lấy mẫu tâm ô.
"""
import os
import re
import sys

import numpy as np
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
sys.stdout.reconfigure(encoding='utf-8')

SHEET = os.path.join(ROOT, 'Art_Source', 'spriters-resource', 'level1_entrance.png')
SECTIONS = {  # rect trên sheet (CVGP_PLAN §2c)
    'CastleEntrance': (1, 11, 768, 192),
    'EntranceHall': (774, 11, 1536, 192),
    'CryptA': (2315, 11, 512, 192),
    'CryptB': (2315, 218, 512, 192),
    'PhantomBat': (2832, 11, 768, 192),
}


def unity_nes(path):
    im = np.asarray(Image.open(path).convert('RGB'))
    if im.shape[:2] != (1080, 1920):
        raise SystemExit('anh Unity phai 1920x1080: %s %s' % (path, im.shape))
    ys = 60 + 4 * np.arange(240) + 2
    xs = 448 + 4 * np.arange(256) + 2
    return im[ys][:, xs].astype(np.int16)


def cmd_h8(udir, bdir):
    for L in (10, 600, 930):
        u = os.path.join(udir, 'h8_L%d.png' % L)
        b = os.path.join(bdir, 'base_L%d.png' % L)
        if not (os.path.exists(u) and os.path.exists(b)):
            print('L%d: thieu anh' % L)
            continue
        a = np.asarray(Image.open(u).convert('RGB')).astype(int)
        c = np.asarray(Image.open(b).convert('RGB')).astype(int)
        if a.shape != c.shape:
            print('L%d: khac kich thuoc %s %s' % (L, a.shape, c.shape))
            continue
        d = np.abs(a - c).sum(2) > 0
        n = int(d.sum())
        if n:
            ys, xs = np.nonzero(d)
            print('L%d: %d pixel khac, bbox man (%d,%d)-(%d,%d) = NES (%d,%d)-(%d,%d)' % (
                L, n, xs.min(), ys.min(), xs.max(), ys.max(), (xs.min() - 448) // 4, (ys.min() - 60) // 4, (xs.max() - 448) // 4, (ys.max() - 60) // 4))
        else:
            print('L%d: 0 pixel khac' % L)


def cmd_terrain(udir):
    sheet = np.asarray(Image.open(SHEET).convert('RGB')).astype(np.int16)
    files = sorted(f for f in os.listdir(udir) if f.startswith('terrain_') and f.endswith('.png'))
    total_bad = 0
    lines = []
    for f in files:
        m = re.match(r'terrain_(\w+?)_cx(\d+)\.png', f)
        sec, cx = m.group(1), int(m.group(2))
        x0, y0, w, h = SECTIONS[sec]
        exp = sheet[y0 + 16:y0 + 192, x0 + cx:x0 + cx + 256]
        got = unity_nes(os.path.join(udir, f))[48:224]
        d = np.abs(got - exp).sum(2) > 0
        n = int(d.sum())
        total_bad += n
        extra = ''
        if n:
            ys, xs = np.nonzero(d)
            extra = ' bbox NES (%d,%d)-(%d,%d)' % (xs.min(), ys.min() + 48, xs.max(), ys.max() + 48)
            Image.fromarray((d * 255).astype(np.uint8)).resize((512, 352), Image.NEAREST).save(os.path.join(udir, 'diff_' + f))
        lines.append('%-36s %6d / %d pixel lech%s' % (f, n, d.size, extra))
    lines.append('TONG pixel lech: %d tren %d anh' % (total_bad, len(files)))
    out = os.path.join(udir, 'terrain.txt')
    with open(out, 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


def cmd_bgpairs(udir, out):
    import map1_grab as mg
    os.makedirs(out, exist_ok=True)
    files = sorted(f for f in os.listdir(udir) if f.endswith('.png') and re.search(r'_f(\d+)', f))
    want = sorted(set(int(re.search(r'_f(\d+)', f).group(1)) for f in files))
    vids = mg.grab('map1', want)
    lines = []
    for f in files:
        fr = int(re.search(r'_f(\d+)', f).group(1))
        u = unity_nes(os.path.join(udir, f))[48:224]
        v = vids[fr][48:224].astype(np.int16)
        # ánh xạ bảng màu: mỗi màu Unity ⇒ trung vị màu video ở cùng chỗ (bảng màu video ≠ sheet)
        key = (u[..., 0].astype(np.int32) << 16) | (u[..., 1].astype(np.int32) << 8) | u[..., 2]
        mapped = np.zeros_like(u)
        for k in np.unique(key):
            msk = key == k
            mapped[msk] = np.median(v[msk], axis=0)
        d = np.abs(mapped - v).max(2)
        bad = d > 48
        # bỏ vùng sprite động: ô 8×8 có > 30 % pixel lệch (Simon/enemy/item/vật đã phá khác lộ trình) — chỉ còn lệch rải rác = nén/viền
        bh, bw = bad.shape[0] // 8, bad.shape[1] // 8
        blk = bad[:bh * 8, :bw * 8].reshape(bh, 8, bw, 8).mean((1, 3))
        dyn = np.repeat(np.repeat(blk > 0.30, 8, 0), 8, 1)
        rest = bad[:bh * 8, :bw * 8] & ~dyn
        lines.append('%-24s f%-5d lech>48 sau anh xa mau: %5.2f%% tong | o 8x8 "dong" %3d/%d | con lai ngoai o dong %5.2f%%' % (
            f[:-4], fr, 100 * bad.mean(), int((blk > 0.30).sum()), bh * bw, 100 * rest.mean()))
        canvas = Image.new('RGB', (256 * 2 * 3 + 8, 176 * 2), (40, 40, 40))
        canvas.paste(Image.fromarray(v.astype(np.uint8)).resize((512, 352), Image.NEAREST), (0, 0))
        canvas.paste(Image.fromarray(mapped.astype(np.uint8)).resize((512, 352), Image.NEAREST), (516, 0))
        dimg = np.zeros(bad.shape + (3,), np.uint8)
        dimg[bad] = (255, 0, 0)
        dimg[dyn[:bad.shape[0], :bad.shape[1]] if dyn.shape == bad.shape else np.zeros_like(bad)] //= 3
        canvas.paste(Image.fromarray(dimg).resize((512, 352), Image.NEAREST), (1032, 0))
        canvas.save(os.path.join(out, 'bg_%s.png' % f[:-4]))
    with open(os.path.join(out, 'bgpairs.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    c = sys.argv[1]
    if c == 'h8':
        cmd_h8(sys.argv[2], sys.argv[3])
    elif c == 'terrain':
        cmd_terrain(sys.argv[2])
    elif c == 'bgpairs':
        cmd_bgpairs(sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '16', 'bg'))
    else:
        raise SystemExit(__doc__)
