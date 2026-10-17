"""CVM2 10 — dò vị trí nến tường Block 2 trên video Map_2 (khớp mẫu Object/candle_0/1 trên bộ đệm lưới NES của CVM2 02 — thước đo
enemy8_fit CVM2 08, bảng màu VIDEO tự ước lượng), quy về toạ độ section bằng camera (ref/data/map2_camera.csv), gom cụm ⇒ bảng vị trí
nến (MAP2_PLACEMENT). Bản Map_2 của Tools/cvgp/map1_props.py (CVGP 15).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/props10.py [bước=3] [errmax=0.14]
  → ClaudeNotes/cvm2/_cache/10/props_det.csv (mọi phát hiện), props_clusters.csv (cụm: section, x tâm = pivot, y đáy section, số lần
    thấy, frame đầu/cuối, sai khác TB), pal_props.json. Pivot = đáy-giữa (manifest [4,16]) ⇒ x tâm, y đáy NES ⇒ y section = NES − 32.
Chỉ frame 'game' của đúng 1 section (bỏ đoạn cửa ghép 'HallA>TEB' / 'HallC>HallB') và frame KHÔNG lặp.
"""
import csv, json, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import enemy8_fit as ef  # noqa: E402

OUT = os.path.join(ef.ROOT, 'ClaudeNotes', 'cvm2', '_cache', '10')
SECTIONS = ['TowerEntranceA', 'TowerEntranceB', 'TowerHallA', 'TowerHallB', 'TowerHallC', 'Medusa']
NAMES = ['Object/candle_0', 'Object/candle_1']
BOX = [0, 48, 256, 224]


def main():
    step = int(sys.argv[1]) if len(sys.argv) > 1 else 3
    errmax = float(sys.argv[2]) if len(sys.argv) > 2 else 0.14
    sprites, cols = ef.load(NAMES)
    sprites = [s for s in sprites if s['flip'] == 0]   # nến đối xứng — lật không thêm thông tin
    sep = (6, 12)
    mm = np.load(os.path.join(ef.WORK, 'map2_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(ef.WORK, 'map2_dup.npy'))
    cam = ef.camera()
    os.makedirs(OUT, exist_ok=True)
    frames = [f for f in sorted(cam) if cam[f][0] == 'game' and cam[f][1] in SECTIONS and not dup[f]]
    pal, nh, best = ef.estimate_pal(mm, dup, frames, sprites, cols, BOX, errmax, sep)
    print('bang mau video:', [(tuple(c), p) for c, p in zip(cols, pal)], 'mau', nh, 'tot nhat', round(best, 4))
    json.dump({','.join(map(str, c)): list(p) for c, p in zip(cols, pal)}, open(os.path.join(OUT, 'pal_props.json'), 'w'))
    rows = []
    k = 0
    for f in frames:
        k += 1
        if k % step:
            continue
        kind, sec, cx, cxw = cam[f]
        nes = np.asarray(mm[f])
        for e, s, x, y, px, py in ef.detect(ef.err_maps(nes, sprites, pal, BOX), BOX, errmax, sep):
            rows.append((sec, f, s['name'].split('/')[-1], cx, px, py, cx + px, py - 32, e))
    with open(os.path.join(OUT, 'props_det.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'frame', 'sprite', 'cx', 'pivx', 'pivy', 'x', 'y', 'err'])
        for r in rows:
            w.writerow(list(r[:8]) + ['%.4f' % r[8]])
    cl = []
    for sec, f, name, cx, px, py, x, y, e in rows:
        for c in cl:
            if c['sec'] == sec and abs(c['x'] - x) <= 3 and abs(c['y'] - y) <= 3:
                c['xs'].append(x); c['ys'].append(y); c['f'].append(f); c['e'].append(e); c['px'].append(px)
                break
        else:
            cl.append(dict(sec=sec, x=x, y=y, xs=[x], ys=[y], f=[f], e=[e], px=[px]))
    with open(os.path.join(OUT, 'props_clusters.csv'), 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['section', 'x', 'y', 'n', 'x_spread', 'y_spread', 'frame_first', 'frame_last', 'err_mean', 'screen_x_range'])
        for c in sorted(cl, key=lambda c: (SECTIONS.index(c['sec']), c['x'])):
            xs = np.array(c['xs']); ys = np.array(c['ys'])
            w.writerow([c['sec'], int(np.round(np.median(xs))), int(np.round(np.median(ys))), len(xs), '%d-%d' % (xs.min(), xs.max()),
                        '%d-%d' % (ys.min(), ys.max()), min(c['f']), max(c['f']), '%.4f' % np.mean(c['e']),
                        '%d-%d' % (min(c['px']), max(c['px']))])
    print('phat hien', len(rows), 'cum', len(cl), '/', len(frames) // step, 'frame quet')


if __name__ == '__main__':
    main()
