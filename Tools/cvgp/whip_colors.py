"""CVGP 09 — đo chu kỳ đổi màu roi cấp 3 trên video Map_1.

Với mọi frame khớp mẫu khung duỗi (use_*_2) trong ClaudeNotes/cvgp/_cache/08/fit_w09_all.csv (sau 0:10.5 — roi cấp 3),
lấy mẫu dải hàng của mảnh roi duỗi (Y−22..Y−17, cách pivot 18..52 px về hướng mặt), đếm pixel gần màu đặc trưng
của 4 bảng màu roi (sheet: c1 cam (252,152,56) · c2 tím (204,136,252)/(252,196,252) · c3 xanh (0,112,236) ·
c4 đỏ (216,40,0)/(252,116,180)) ⇒ màu trội. Rồi xét cặp frame video MỚI liên tiếp trong cùng cú roi: hiệu chỉ số màu
(mod 4) theo khoảng tick ⇒ suy ra màu đổi mỗi mấy tick và theo chiều nào.

Chạy: python -P -E Tools/cvgp/whip_colors.py   (đọc lại video bằng ffmpeg, ~vài phút)
"""
import csv, os, sys
from collections import Counter, defaultdict
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

FIT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '08', 'fit_w09_all.csv')
Y0 = 8
F_L3 = 315   # 0:10.5 — roi cấp 3


def classify(nes, px, Y, flip):
    rows = np.arange(Y - 22, Y - 16) - Y0
    if flip:   # quay phải
        cols = np.arange(px + 18, px + 53)
    else:
        cols = np.arange(px - 52, px - 17)
    cols = cols[(cols >= 0) & (cols < 256)]
    reg = nes[rows][:, cols].reshape(-1, 3)
    r, g, b = reg[:, 0], reg[:, 1], reg[:, 2]
    # luật màu theo màu VIDEO (video tối/lệch so với sheet: đỏ (216,40,0) → ~(160–192,0,0), hồng → ~(224,64,128),
    # tím (204,136,252) → ~(160,96,224), tím nhạt → ~(224,192,224), cam → ~(224,128,32))
    cnt = {
        1: int(((r > 190) & (g > 100) & (g < 175) & (b < 80)).sum()),
        2: int(((b > 170) & (r > 120) & (g < b - 20)).sum()),
        3: int(((b > 150) & (r < 100)).sum()),
        4: int(((r > 130) & (g < 100) & (b < 160)).sum()),
    }
    best = max(cnt, key=cnt.get)
    return best if cnt[best] >= 6 else 0, cnt


def main():
    rows = [r for r in csv.DictReader(open(FIT))]
    want = {}
    for r in rows:
        f = int(r['frame'])
        if f < F_L3 or r['dup'] != '0' or not r['sprite'].endswith('_2') or not r['sprite'].startswith('use_'):
            continue
        if float(r['err']) > 0.12:
            continue
        want[f] = (int(r['px']), int(r['Y']), int(r['flip']))
    fs = sorted(want)
    # gom thành đoạn liền để đọc video
    segs = []
    for f in fs:
        if segs and f - segs[-1][1] <= 3:
            segs[-1][1] = f
        else:
            segs.append([f, f])
    res = {}
    for a, b in segs:
        ss = max(0.0, (a - 0.5) / 30.0)
        f = a
        for img in nc.read_frames('map1', ss=ss, t=(b - a + 1) / 30.0):
            if f > b:
                break
            if f in want:
                nes = nc.nes_sample(img, nc.CALIB['map1'], y0=Y0, y1=240 - Y0)
                res[f] = classify(nes, *want[f])
            f += 1
    print('frame duoi L3:', len(res), ' phan bo mau:', sorted(Counter(v[0] for v in res.values()).items()))
    pairs = defaultdict(Counter)
    lines = []
    for f in sorted(res):
        g = f + 1
        while g <= f + 3 and g not in res:
            g += 1
        if g in res and res[f][0] and res[g][0]:
            d = (res[g][0] - res[f][0]) % 4
            pairs[2 * (g - f)][d] += 1
            lines.append('f%d c%d -> f%d c%d  (%d tick, hieu %d)' % (f, res[f][0], g, res[g][0], 2 * (g - f), d))
    print('\n'.join(lines))
    print('\nhieu chi so mau (mod 4) theo khoang tick giua 2 frame moi:')
    for t in sorted(pairs):
        print('  %d tick: %s' % (t, sorted(pairs[t].items())))
    # chuyển màu theo thứ tự: c -> màu kế tiếp (đếm mọi cặp 2 tick)
    trans = Counter((res[f][0], res[f + 1][0]) for f in res if f + 1 in res and res[f][0] and res[f + 1][0])
    print('chuyen mau giua 2 frame MOI cach 1 frame video:', sorted(trans.items()))


if __name__ == '__main__':
    main()
