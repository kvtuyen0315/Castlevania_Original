"""CVM2 08 — ghép cặp ảnh video Map_2 ↔ Unity (Map2EnemyProbe ShotDir: <đoạn>_r<rel>.png) CÙNG mốc thời gian của đoạn so:
frame video = f0 + rel / 2.0033 (frame không lặp gần nhất). Cả hai cắt về lưới NES y 40..223 (sân chơi + mép HUD), phóng 2×, mỗi tấm
4 cặp (trái video, phải Unity), nhãn đoạn/rel/frame/cx. Unity 1920×1080: NES (x, y) ↔ ảnh (448 + 4x + 2, 60 + 4y + 2).
Chạy (từ gốc repo): python -P -E Tools/cvm2/enemy8_pairs.py [shotdir] ⇒ ClaudeNotes/cvm2/_cache/shots/08/pairs_<n>.png + pairs.txt
"""
import csv, os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '08')
TPF = 2.0033
F0 = {'mh_a': 1762, 'mh_b': 2801, 'mh_m': 6180, 'sk_teb': 863, 'sk_hb': 3209, 'gh_1': 4976, 'gh_2': 5810, 'bat_c': 4974, 'bat_tea': 187, 'bt_c': 4922}
Y0, Y1, Z = 40, 224, 2


def main():
    sd = sys.argv[1] if len(sys.argv) > 1 else os.path.join(OUT, 'unity')
    mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(WORK, 'map2_dup.npy'))
    cam = {int(r['frame']): r['cx'] for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')))}
    pairs = []
    for fn in sorted(os.listdir(sd)):
        if not fn.endswith('.png') or '_r' not in fn:
            continue
        seg, rel = fn[:-4].rsplit('_r', 1)
        if seg not in F0:
            continue
        rel = int(rel)
        f = int(round(F0[seg] + rel / TPF))
        while dup[f] and f > 0:
            f -= 1
        v = np.asarray(mm[f])[Y0 - 8:Y1 - 8]
        u = np.asarray(Image.open(os.path.join(sd, fn)).convert('RGB'))
        xs = 448 + 4 * np.arange(256) + 2
        ys = 60 + 4 * np.arange(Y0, Y1) + 2
        u = u[ys][:, xs]
        pairs.append(('%s r%d ↔ f%d (cx video %s)' % (seg, rel, f, cam.get(f, '?')), v, u))
    os.makedirs(OUT, exist_ok=True)
    H, W = (Y1 - Y0) * Z, 256 * Z
    lines = []
    for n in range(0, len(pairs), 4):
        grp = pairs[n:n + 4]
        im = Image.new('RGB', (2 * W + 12, len(grp) * (H + 16)), (30, 30, 30))
        d = ImageDraw.Draw(im)
        for i, (lab, v, u) in enumerate(grp):
            y = i * (H + 16)
            im.paste(Image.fromarray(v.astype(np.uint8)).resize((W, H), Image.NEAREST), (0, y + 14))
            im.paste(Image.fromarray(u.astype(np.uint8)).resize((W, H), Image.NEAREST), (W + 12, y + 14))
            d.text((2, y + 1), 'VIDEO ' + lab.split(' ↔ ')[1], fill=(255, 255, 0))
            d.text((W + 14, y + 1), 'UNITY ' + lab.split(' ↔ ')[0], fill=(120, 255, 120))
            lines.append('pairs_%d.png #%d: %s' % (n // 4, i, lab))
        im.save(os.path.join(OUT, 'pairs_%d.png' % (n // 4)))
    open(os.path.join(OUT, 'pairs.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    print('ghi', (len(pairs) + 3) // 4, 'tam,', len(pairs), 'cap')


if __name__ == '__main__':
    main()
