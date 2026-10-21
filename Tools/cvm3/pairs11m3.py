"""CVM3 11 — ghép cặp ảnh chuyển cảnh Map 3: video Map_3 (bộ đệm lưới NES của CVM3 02, map3_nes.npy, hàng NES 8..231) ↔ ảnh Unity của
Map3RouteProbe.Replay(shotDir) (Game view 1920x1080: NES 256x240 phóng 4 ở (448, 60)). Khuôn Tools/cvm2/pairs11.py.

Chạy (từ gốc repo):  python -P -E Tools/cvm3/pairs11m3.py [thư mục ảnh Unity] [thư mục ra]
  mặc định ClaudeNotes/cvm3/_cache/shots/11/unity → ClaudeNotes/cvm3/_cache/shots/11/pairs_<n>.png + pairs.txt
Ánh xạ tên ảnh Unity "<mốc>_r<tick>" → frame video theo CÙNG mốc (tick = frame × 2.0033; mốc đo CVM3 11 — CVM3_MEMORY §2k):
  bản đồ B2 dừng f81 (Unity dừng ở tick 171 của màn bản đồ ⇒ map2_rN ↔ f81 + (N − 171)/2.0033); STAGE 07 khung đầu f207;
  thang: khung đầu WEB f1198, WalkwayB f3275; cửa #1: cuộn 1 bắt đầu f2438.1, xong f2501.9, cuộn 2 bắt đầu f2581.1; cửa #2: f4336.5 / f4400.4
  / f4480.6; STAGE 08 khung đầu f2645, STAGE 09 f4546; khoá cx 1280 f5931, Mummy thức +120 tick; quả cầu hiện f6829.5; nhặt cầu f6920;
  tally đếm TIME f7033; bản đồ B3 dx 256 (hồi quy) f7462.1. Ảnh không có video (map3_r300 — sau f7547, st10) bỏ qua.
Mỗi cặp: trái = video, phải = Unity, lưới NES 256x224 (y 8..231), phóng 2. pairs.txt: tên, frame video, sai khác TB HUD (y 8..47) + vùng
chơi (đo thô — bảng màu video khác sheet ⇒ chỉ so tương đối; kết luận bằng mắt).
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
NES = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work', 'map3_nes.npy')
TPF = 2.0033
Y0 = 8
LAST_FRAME = 7547

# mốc: r = 0 ứng với frame video BASE (r001 = khung đầu ⇒ BASE = khung đầu − 1 tick)
BASE = {
    'map2': 81 - 171 / TPF,
    'st07': 207 - 1 / TPF,
    'web': 1198 - 1 / TPF, 'wb': 3275 - 1 / TPF,
    'door1_scroll1': 2438.1, 'door1_stop': 2501.9, 'door1_scroll2': 2581.1,
    'door2_scroll1': 4336.5, 'door2_stop': 4400.4, 'door2_scroll2': 4480.6,
    'wa': 2645 - 1 / TPF, 'mm': 4546 - 1 / TPF,
    'lock': 5931, 'wake': 5931 + 120 / TPF, 'crystal': 6829.5, 'pickup': 6920, 'tally': 7033, 'map3': 7462.1,
}
ORDER = ['map2', 'st07', 'web', 'door1_scroll1', 'door1_stop', 'door1_scroll2', 'wa', 'wb', 'door2_scroll1', 'door2_stop', 'door2_scroll2',
         'mm', 'lock', 'wake', 'crystal', 'pickup', 'tally', 'map3']


def video_frame(name):
    stem, r = name.rsplit('_r', 1)
    r = int(r)
    return BASE[stem] + r / TPF   # scroll2_rN: N tính từ lúc cuộn 2 bắt đầu (Unity door_stop + 158 / 161 — Map3FlowTiming)


def unity_nes(path):
    im = Image.open(path).convert('RGB')
    im = im.crop((448, 60, 448 + 1024, 60 + 960)).resize((256, 240), Image.NEAREST)
    return np.asarray(im)[Y0:232]


def main():
    shots = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', 'shots', '11', 'unity')
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', 'shots', '11')
    mm = np.load(NES, mmap_mode='r')
    names = sorted(os.path.splitext(f)[0] for f in os.listdir(shots) if f.endswith('.png'))
    names.sort(key=lambda n: (ORDER.index(n.rsplit('_r', 1)[0]) if n.rsplit('_r', 1)[0] in ORDER else 99, n))
    lines, tiles = [], []
    for n in names:
        try:
            fv = video_frame(n)
        except (KeyError, ValueError):
            continue
        f = int(round(fv))
        if f < 0 or f > LAST_FRAME:
            continue
        v = np.asarray(mm[f])
        u = unity_nes(os.path.join(shots, n + '.png'))
        hud = np.abs(v[:40].astype(int) - u[:40].astype(int)).mean()
        play = np.abs(v[40:].astype(int) - u[40:].astype(int)).mean()
        lines.append('%s\tf%d (%.1f)\tHUD %.1f\tchoi %.1f' % (n, f, fv, hud, play))
        pair = Image.new('RGB', (2 * 256 * 2 + 8, 224 * 2 + 16), (40, 40, 40))
        pair.paste(Image.fromarray(v).resize((512, 448), Image.NEAREST), (0, 16))
        pair.paste(Image.fromarray(u).resize((512, 448), Image.NEAREST), (520, 16))
        d = ImageDraw.Draw(pair)
        d.text((4, 2), 'video Map_3 f%d' % f, fill=(255, 255, 0))
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
    print('%d cap -> %s/pairs_0..%d.png' % (len(tiles), out, (len(tiles) - 1) // per))


if __name__ == '__main__':
    main()
