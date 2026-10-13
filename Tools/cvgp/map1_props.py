"""CVGP 15 — dò vị trí nến tường / chậu lửa trên Map_1 (khớp mẫu sprite Object/candle_*, torch_* — cùng thước đo enemy_fit 13),
quy về toạ độ section bằng camera planner (work/map1_camera.csv), gom cụm theo (section, x, y) ⇒ bảng vị trí đặt vật (MAP1_PLACEMENT).

Chạy (từ gốc repo):  python -P -E Tools/cvgp/map1_props.py [bước=3] [errmax=0.12]
  → ClaudeNotes/cvgp/_cache/15/props_det.csv (mọi phát hiện), props_scanned.csv (frame đã quét) + props_clusters.csv (cụm: section, x tâm = pivot, y đáy, số lần thấy,
    frame đầu/cuối, sai khác TB). Pivot sprite = đáy-giữa (manifest) ⇒ x tâm, y mặt đáy (= hàng dưới cùng + 1) theo NES ⇒ y section = y − 32.
Đoạn đo (ngoài non-game, ngoài lúc chuyển cảnh): CE f67–590, EH f600–2545, CA f2756–3232 + f4441–4914, CB f3503–4430, PB f5126–6300.
"""
import csv, os, sys
from collections import defaultdict
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
import enemy_fit as ef  # noqa: E402

OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '15')
SEGMENTS = [('CastleEntrance', 67, 590), ('EntranceHall', 600, 2545), ('CryptA', 2756, 3232), ('CryptB', 3503, 4430),
            ('CryptA', 4441, 4914), ('PhantomBat', 5126, 6300)]
NAMES = ['Object/candle_0', 'Object/candle_1', 'Object/torch_0', 'Object/torch_1']


def main():
    step = int(sys.argv[1]) if len(sys.argv) > 1 else 3
    errmax = float(sys.argv[2]) if len(sys.argv) > 2 else 0.12
    sprites, pal = ef.load(NAMES)
    sep = (6, 12)
    dup = np.load(os.path.join(ef.VID_DIR, 'work', 'map1_dup.npy'))
    cam = ef.camera('map1')
    os.makedirs(OUT, exist_ok=True)
    rows = []
    scanned = []
    for sec, f0, f1 in SEGMENTS:
        k = 0
        for f, nes in ef.frames('map1', f0, f1):
            if dup[f]:
                continue
            k += 1
            if k % step:
                continue
            cx, csec = cam.get(f, (None, ''))
            if cx is None or csec != sec:
                continue
            scanned.append((sec, f))
            for e, s, x, y, px, py in ef.detect(ef.err_maps(nes, sprites, pal, (0, 48, 256, 224)), (0, 48), errmax, sep):
                rows.append((sec, f, s['name'].split('/')[-1], cx, px, py, cx + px, py - 32, e))
        print(sec, f0, f1, 'xong', len(rows))
    with open(os.path.join(OUT, 'props_det.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'frame', 'sprite', 'cx', 'pivx', 'pivy', 'x', 'y', 'err'])
        for r in rows:
            w.writerow(list(r[:8]) + ['%.4f' % r[8]])
    with open(os.path.join(OUT, 'props_scanned.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'frame'])
        w.writerows(scanned)
    # gom cụm: cùng section, |dx| <= 2, |dy| <= 2, cùng loại (candle/torch)
    cl = []
    for sec, f, name, cx, px, py, x, y, e in rows:
        kind = name.split('_')[0]
        for c in cl:
            if c['sec'] == sec and c['kind'] == kind and abs(c['x'] - x) <= 2 and abs(c['y'] - y) <= 2:
                c['xs'].append(x); c['ys'].append(y); c['f'].append(f); c['e'].append(e)
                break
        else:
            cl.append(dict(sec=sec, kind=kind, x=x, y=y, xs=[x], ys=[y], f=[f], e=[e]))
    with open(os.path.join(OUT, 'props_clusters.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'kind', 'x', 'y', 'n', 'x_spread', 'frame_first', 'frame_last', 'err_mean'])
        for c in sorted(cl, key=lambda c: (c['sec'], c['x'])):
            xs = np.array(c['xs']); ys = np.array(c['ys'])
            xm = int(np.round(np.median(xs))); ym = int(np.round(np.median(ys)))
            w.writerow([c['sec'], c['kind'], xm, ym, len(xs), '%d-%d' % (xs.min(), xs.max()), min(c['f']), max(c['f']),
                        '%.4f' % np.mean(c['e'])])
    print('cum', len(cl))


if __name__ == '__main__':
    main()
