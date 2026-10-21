"""CVM3 07 — dò NHIỀU Skeleton / xương / Fleaman mỗi frame video Map_3 (hoặc Boss_Map_3) bằng khớp mẫu sprite trên bộ đệm lưới NES
(CVM3 02: _cache/02/work/<video>_nes.npy, y NES 8..231). Dùng lại nguyên thước đo của Tools/cvm2/enemy8_fit.py (CVM2 08: sai khác màu cắt
CLIP, gồm pixel ĐEN đục, bảng màu VIDEO tự ước lượng — H1: chỉ để dò vị trí, không lấy màu làm sprite); chỉ đổi đường dẫn + camera Map_3.

Chạy (từ gốc repo):
  python -P -E Tools/cvm3/enemy7_fit.py <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax=0.16] [pal=auto|file.json] [video=map3] [step=1]
    sprites: đường dẫn tương đối Assets/Data/Art không đuôi, cách ',' (vd Enemy/Fleaman/fleaman_0,Enemy/Fleaman/fleaman_1)
    vùng tìm = px màn NES (mặc định sân chơi 0 48 256 224); step > 1: chỉ lấy 1/step frame không lặp (dò thô)
  → ClaudeNotes/cvm3/_cache/07/det_<tag>.csv (frame, dup, sprite, flip, góc trên-trái x y, pivot màn pivx pivy, cx, section, worldx,
    ysec = pivy − 32, err) + pal_<tag>.json.
worldx = cx_video + pivx (gốc SHEET; WalkwayB: Unity x = worldx + 112 — CVM3 05). Cửa phải (WEB>WA, WB>MM) dùng cxw.
"""
import csv, json, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
import enemy8_fit as E8  # noqa: E402  (load / err_maps / detect / estimate_pal)

WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '07')


def camera(video):
    cam = {}
    for r in csv.DictReader(open(os.path.join(WORK, '%s_camera.csv' % video))):
        cam[int(r['frame'])] = (r['kind'], r['section'], int(float(r['cx'])), int(float(r['cxw'])))
    return cam


def main():
    a = sys.argv
    tag, names = a[1], a[2].split(',')
    f0, f1 = int(a[3]), int(a[4])
    box = [int(v) for v in a[5:9]] if len(a) > 8 else [0, 48, 256, 224]
    errmax = float(a[9]) if len(a) > 9 else 0.16
    palarg = a[10] if len(a) > 10 else 'auto'
    video = a[11] if len(a) > 11 else 'map3'
    step = int(a[12]) if len(a) > 12 else 1
    sprites, cols = E8.load(names)
    sep = (max(s['w'] for s in sprites) * 3 // 4, max(s['h'] for s in sprites) * 3 // 4)
    mm = np.load(os.path.join(WORK, '%s_nes.npy' % video), mmap_mode='r')
    dup = np.load(os.path.join(WORK, '%s_dup.npy' % video))
    cam = camera(video)
    os.makedirs(OUT, exist_ok=True)
    frames = [f for f in range(f0, min(f1, len(dup) - 1) + 1) if cam.get(f, ('',))[0] == 'game']
    if palarg == 'auto':
        pal, nh, best = E8.estimate_pal(mm, dup, frames, sprites, cols, box, errmax, sep)
        print('bang mau video (sheet -> video):', [(tuple(c), p) for c, p in zip(cols, pal)], 'mau', nh, 'tot nhat', round(best, 4))
    else:
        d = json.load(open(palarg))
        pal = [tuple(d[','.join(map(str, c))]) for c in cols]
    json.dump({','.join(map(str, c)): list(p) for c, p in zip(cols, pal)}, open(os.path.join(OUT, 'pal_%s.json' % tag), 'w'))
    p = os.path.join(OUT, 'det_%s.csv' % tag)
    n = k = 0
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'dup', 'sprite', 'flip', 'x', 'y', 'pivx', 'pivy', 'cx', 'section', 'worldx', 'ysec', 'err'])
        for f in frames:
            if dup[f]:
                continue
            k += 1
            if step > 1 and k % step:
                continue
            kind, sec, cx, cxw = cam[f]
            base = cxw if '>' in sec else cx
            nes = np.asarray(mm[f])
            for e, s, x, y, px, py in E8.detect(E8.err_maps(nes, sprites, pal, box), box, errmax, sep):
                w.writerow([f, 0, s['name'].split('/')[-1], s['flip'], x, y, px, py, base, sec, base + px, py - 32, '%.4f' % e])
                n += 1
    print('ghi', p, n, 'phat hien /', len(frames), 'frame')


if __name__ == '__main__':
    main()
