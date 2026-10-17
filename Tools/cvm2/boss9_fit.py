"""CVM2 09 — dò Queen Medusa / rắn / quả cầu trên bộ đệm lưới NES của 2 video (routine 02: map2_nes.npy, boss2_nes.npy).
Dùng lại thước đo của enemy8_fit.py (CVM2 08): sai khác màu cắt CLIP, trung bình trên pixel đục của sprite (gồm cả pixel đen), bảng màu
VIDEO tự ước lượng (lượt 1 = màu sheet x0.85) — H1: video boss khác bảng màu ⇒ chỉ dùng để dò vị trí/khung, KHÔNG lấy màu làm sprite.

Chạy (từ gốc repo):
  python -P -E Tools/cvm2/boss9_fit.py <map2|boss2> <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax=0.16] [pal=auto|file.json] [maxdet=4]
    sprites: tương đối Assets/Data/Art, không đuôi, cách ',' (vd Boss/Medusa/medusa_head,Boss/Medusa/medusa_stone)
  → ClaudeNotes/cvm2/_cache/09/det_<tag>.csv (frame, dup, sprite, flip, x, y (góc trên-trái NES), pivx, pivy (pivot NES), cx, wx = cx + pivx,
    ysec = pivy − 32, err) + pal_<tag>.json.
Camera: map2 đọc ref/data/map2_camera.csv (chỉ frame 'game'); boss2 = phòng Medusa cx 0 mọi frame (CVM2 02: camera khoá suốt video).
"""
import csv, json, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import enemy8_fit as ef  # noqa: E402

ROOT = ef.ROOT
WORK = ef.WORK
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '09')


def camera(video, n):
    if video == 'boss2':
        return {f: ('game', 'Medusa', 0, 0) for f in range(n)}
    return ef.camera()


def main():
    video, tag, names = sys.argv[1], sys.argv[2], sys.argv[3].split(',')
    f0, f1 = int(sys.argv[4]), int(sys.argv[5])
    box = [int(v) for v in sys.argv[6:10]] if len(sys.argv) > 9 else [0, 48, 256, 224]
    errmax = float(sys.argv[10]) if len(sys.argv) > 10 else 0.16
    palarg = sys.argv[11] if len(sys.argv) > 11 else 'auto'
    maxdet = int(sys.argv[12]) if len(sys.argv) > 12 else 4
    sprites, cols = ef.load(names)
    sep = (max(s['w'] for s in sprites) * 3 // 4, max(s['h'] for s in sprites) * 3 // 4)
    mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(WORK, video + '_dup.npy'))
    cam = camera(video, mm.shape[0])
    os.makedirs(OUT, exist_ok=True)
    frames = [f for f in range(f0, min(f1, mm.shape[0] - 1) + 1) if cam.get(f, ('',))[0] == 'game']
    if palarg == 'auto':
        pal, nh, best = ef.estimate_pal(mm, dup, frames, sprites, cols, box, errmax, sep)
        print('bang mau video (sheet -> video):', [(tuple(c), p) for c, p in zip(cols, pal)], 'mau', nh, 'tot nhat', round(best, 4))
    else:
        d = json.load(open(palarg))
        pal = [tuple(d[','.join(map(str, c))]) for c in cols]
    json.dump({','.join(map(str, c)): list(p) for c, p in zip(cols, pal)}, open(os.path.join(OUT, 'pal_%s.json' % tag), 'w'))
    p = os.path.join(OUT, 'det_%s.csv' % tag)
    n = 0
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'wx', 'ysec', 'err'])
        for f in frames:
            d = int(dup[f])
            if d:
                continue
            kind, sec, cx, cxw = cam[f]
            nes = np.asarray(mm[f])
            for e, s, x, y, px, py in ef.detect(ef.err_maps(nes, sprites, pal, box), box, errmax, sep)[:maxdet]:
                w.writerow([f, d, s['name'].split('/')[-1], s['flip'], x, y, px, py, cx, cx + px, py - 32, '%.4f' % e])
                n += 1
    print('ghi', p, n, 'phat hien /', len(frames), 'frame')


if __name__ == '__main__':
    main()
