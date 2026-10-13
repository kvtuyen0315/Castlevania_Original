"""CVGP 15 — cặp ảnh video Map_1 ↔ Unity (Map1RouteProbe.Replay chụp Game view 1920×1080) cùng camera x, đo lệch vật đặt sẵn.

Chạy (từ gốc repo):  python -P -E Tools/cvgp/map1_pairs.py <thư mục ảnh Unity> [out_dir]
  - Ảnh Unity tên <nhóm>_f<frame video>_<…>.png ⇒ frame video so sánh = số sau 'f'.
  - Unity: vùng game 1024×960 tại (448,60) = NES ×4 ⇒ lấy mẫu tâm mỗi ô 4×4 ⇒ 256×240.
  - Video: lưới NES theo hiệu chuẩn MAP1_TIMELINE §1 (map1_grab.py).
  - Mỗi cặp: ảnh ghép [video | Unity | |hiệu|] ×2 → <out>/pair_<tên>.png; dò nến/chậu (khớp mẫu sprite như map1_props) ở cả 2 ảnh,
    ghép cặp gần nhất ⇒ lệch (dx, dy) px NES; độ lệch nền = trung vị |hiệu| vùng chơi (y 48–223) ngoài khung có sprite động.
  → <out>/pairs.txt
"""
import os, re, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import map1_grab as mg  # noqa: E402
import enemy_fit as ef  # noqa: E402

PROPS = ['Object/candle_0', 'Object/candle_1', 'Object/torch_0', 'Object/torch_1']


def unity_nes(path):
    im = np.asarray(Image.open(path).convert('RGB'))
    ys = 60 + 4 * np.arange(240) + 2
    xs = 448 + 4 * np.arange(256) + 2
    return im[ys][:, xs].astype(np.uint8)


def props(nes, sprites, pal):
    sub = nes[8:232].astype(float)
    det = ef.detect(ef.err_maps(sub, sprites, pal, (0, 48, 256, 224)), (0, 48), 0.12, (6, 12))
    return [(px, py, s['name'].split('/')[-1].split('_')[0], e) for e, s, x, y, px, py in det]


def main():
    udir = sys.argv[1]
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'shots', '15', 'pairs')
    os.makedirs(out, exist_ok=True)
    files = sorted(f for f in os.listdir(udir) if f.endswith('.png') and re.search(r'_f(\d+)', f))
    want = sorted(set(int(re.search(r'_f(\d+)', f).group(1)) for f in files))
    vids = mg.grab('map1', want)
    sprites, pal = ef.load(PROPS)
    lines = []
    for f in files:
        fr = int(re.search(r'_f(\d+)', f).group(1))
        u = unity_nes(os.path.join(udir, f))
        v = vids[fr]
        d = np.abs(u.astype(int) - v.astype(int)).sum(2)
        play = d[48:224]
        bg = float(np.median(play)) / 3
        frac = float((play > 90).mean())
        pu, pv = props(u, sprites, pal), props(v, sprites, pal)
        matches = []
        for (x, y, k, e) in pv:
            best = None
            for (x2, y2, k2, e2) in pu:
                if k2 != k:
                    continue
                dd = abs(x2 - x) + abs(y2 - y)
                if best is None or dd < best[0]:
                    best = (dd, x2 - x, y2 - y)
            if best is not None and best[0] <= 8:
                matches.append((x, y, best[1], best[2]))
        miss_u = len(pv) - len(matches)
        name = f[:-4]
        desc = ' '.join('(%d,%d)%+d%+d' % m for m in matches)
        lines.append('%-24s f%-5d nen: |hieu| trung vi %.1f, %.1f%% px khac >30  | vat video %d, Unity %d, ghep %d (lech max %d px): %s%s' % (
            name, fr, bg, 100 * frac, len(pv), len(pu), len(matches),
            max([abs(m[2]) + abs(m[3]) for m in matches] or [0]), desc, '' if miss_u == 0 else '  [video co %d vat khong ghep]' % miss_u))
        W = 256 * 2
        canvas = Image.new('RGB', (W * 3 + 8, 240 * 2 + 14), (40, 40, 40))
        canvas.paste(Image.fromarray(v).resize((W, 480), Image.NEAREST), (0, 14))
        canvas.paste(Image.fromarray(u).resize((W, 480), Image.NEAREST), (W + 4, 14))
        dimg = np.clip(d / 3, 0, 255).astype(np.uint8)
        canvas.paste(Image.fromarray(dimg).convert('RGB').resize((W, 480), Image.NEAREST), (2 * W + 8, 14))
        dr = ImageDraw.Draw(canvas)
        dr.text((2, 1), 'video f%d' % fr, fill=(0, 255, 0))
        dr.text((W + 6, 1), 'Unity ' + name, fill=(0, 255, 0))
        dr.text((2 * W + 10, 1), '|hieu|', fill=(0, 255, 0))
        canvas.save(os.path.join(out, 'pair_%s.png' % name))
    with open(os.path.join(out, 'pairs.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    main()
