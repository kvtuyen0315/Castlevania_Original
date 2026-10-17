"""CVM2 11 — ghép cặp ảnh chuyển cảnh Map 2: video Map_2 (bộ đệm lưới NES của CVM2 02, map2_nes.npy, hàng NES 8..231) ↔ ảnh Unity của
Map2RouteProbe.Replay(shotDir) (Game view 1920x1080: NES 256x240 phóng 4 ở (448, 60)).

Chạy (từ gốc repo):  python -P -E Tools/cvm2/pairs11.py [thư mục ảnh Unity] [thư mục ra]
  mặc định ClaudeNotes/cvm2/_cache/shots/11/unity → ClaudeNotes/cvm2/_cache/shots/11/pairs_<n>.png + pairs.txt
Ánh xạ tên ảnh Unity → frame video theo CÙNG mốc chuyển cảnh (tick = frame × 2.0033; mốc đo ở CVM2_MEMORY §2m / MAP2_TIMELINE §7):
  cửa #1: cuộn 1 bắt đầu f1553.0, xong f1616.5, cuộn 2 bắt đầu f1696.5; cửa #2: f4269.0 / f4332.0 / f4414.8
  đổi tầng: khung đầu TEB f866, Hall B f2801, Medusa f5924; vào STAGE 04 f187, Hall A f1761, Hall C f4479
  khoá camera f6735, Medusa thức f6826, nhặt cầu f7164.9, tally đếm TIME f7277, bản đồ B2 bật f7594.7, STAGE 07 f7806.
Mỗi cặp: trái = video, phải = Unity, cùng lưới NES 256x224 (y 8..231), phóng 2. pairs.txt: tên, frame video, sai khác TB vùng HUD
(y 8..47) + vùng chơi (đo thô — bảng màu video khác sheet ⇒ chỉ để so tương đối; kết luận bằng mắt).
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
NES = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work', 'map2_nes.npy')
TPF = 2.0033
Y0 = 8

BASE = {
    'door1_scroll1': 1553.0, 'door1_stop': 1616.5, 'door1_scroll2': 1696.5,
    'door2_scroll1': 4269.0, 'door2_stop': 4332.0, 'door2_scroll2': 4414.8,
    'teb': 866 - 1 / TPF, 'hallb': 2801 - 1 / TPF, 'medusa': 5924 - 1 / TPF,   # r001 = khung đầu
    'st04': 187 - 1 / TPF, 'halla': 1761, 'hallc': 4479,
    'lock': 6735, 'wake': 6826 - 1, 'crystal': 7164.9, 'tally': 7277, 'map2': 7594.7, 'st07': 7806 - 1,
}
SCROLL2 = {'door1': 160, 'door2': 166}   # mốc cuộn 2 (tick từ door_stop) — ảnh scroll2_r064 tính từ đây


def video_frame(name):
    # tên: <mốc>_r<tick> hoặc door<k>_<pha>_r<tick>
    stem, r = name.rsplit('_r', 1)
    r = int(r)
    if stem.endswith('_scroll2'):
        return BASE[stem] + r / TPF
    return BASE[stem] + r / TPF


def unity_nes(path):
    im = Image.open(path).convert('RGB')
    im = im.crop((448, 60, 448 + 1024, 60 + 960)).resize((256, 240), Image.NEAREST)
    return np.asarray(im)[Y0:232]


def main():
    shots = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '11', 'unity')
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '11')
    mm = np.load(NES, mmap_mode='r')
    names = sorted(os.path.splitext(f)[0] for f in os.listdir(shots) if f.endswith('.png'))
    order = ['st04', 'teb', 'door1_scroll1', 'door1_stop', 'door1_scroll2', 'halla', 'hallb', 'door2_scroll1', 'door2_stop', 'door2_scroll2',
             'hallc', 'medusa', 'lock', 'wake', 'crystal', 'tally', 'map2', 'st07']
    names.sort(key=lambda n: (order.index(n.rsplit('_r', 1)[0]) if n.rsplit('_r', 1)[0] in order else 99, n))
    lines, tiles = [], []
    for n in names:
        try:
            fv = video_frame(n)
        except KeyError:
            continue
        f = int(round(fv))
        v = np.asarray(mm[f])
        u = unity_nes(os.path.join(shots, n + '.png'))
        hud = np.abs(v[:40].astype(int) - u[:40].astype(int)).mean()
        play = np.abs(v[40:].astype(int) - u[40:].astype(int)).mean()
        lines.append('%s\tf%d (%.1f)\tHUD %.1f\tchơi %.1f' % (n, f, fv, hud, play))
        pair = Image.new('RGB', (2 * 256 * 2 + 8, 224 * 2 + 16), (40, 40, 40))
        pair.paste(Image.fromarray(v).resize((512, 448), Image.NEAREST), (0, 16))
        pair.paste(Image.fromarray(u).resize((512, 448), Image.NEAREST), (520, 16))
        d = ImageDraw.Draw(pair)
        d.text((4, 2), 'video f%d' % f, fill=(255, 255, 0))
        d.text((524, 2), 'Unity ' + n, fill=(0, 255, 255))
        tiles.append(pair)
    per = 4
    for k in range(0, len(tiles), per):
        grp = tiles[k:k + per]
        sheet = Image.new('RGB', (grp[0].width, sum(t.height for t in grp)), (0, 0, 0))
        y = 0
        for t in grp:
            sheet.paste(t, (0, y))
            y += t.height
        sheet.save(os.path.join(out, 'pairs_%d.png' % (k // per)))
    with open(os.path.join(out, 'pairs.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(lines) + '\n')
    print(len(lines), 'cap ->', out)


if __name__ == '__main__':
    main()
