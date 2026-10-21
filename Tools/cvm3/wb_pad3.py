# -*- coding: utf-8 -*-
"""wb_pad3.py - CVM3 05: đo nội dung phần WalkwayB NGOÀI mép trái sheet (camera video Map_3 tới cx −112) để nới section bằng tile CÓ SẴN.

Chạy từ gốc repo:  python -P -E Tools/cvm3/wb_pad3.py
Cần: ClaudeNotes/cvm3/_cache/02/work/map3_nes.npy + map3_lut.npz (Tools/cvm3/extract_nes3.py + fg3.lut, CVM3 02), ref/data/map3_camera.csv.
1. Ghép nền thế giới x −128..+127 (NES y 32..223) từ mọi frame WalkwayB có cx < 128 (cx từng frame CVM3 02), TRUNG VỊ theo frame (khử sprite).
2. Mỗi ô 8x8 ở x −112..+63, y 16..191 (y 0..15 sau HUD — không thấy): so SSD với MỌI tile 8x8 của 5 vùng sheet đã đổi màu sheet→video (LUT)
   ⇒ tile tốt nhất + nhì. Kiểm chứng ở x 0..63 (vùng sheet đã biết): tile tốt nhất phải = tile thật của sheet.
3. In bảng ô KHÁC trời ở phần x < 0 (để chép vào Tools/cvm3/build_tileset3.py PAD_LEFT, nguồn = ô có sẵn trong CHÍNH WalkwayB) + ảnh
   ClaudeNotes/cvm3/_cache/05/work/wb_pad_compare.png (trên: nền trung vị video; dưới: dựng lại từ tile đã chọn — cùng LUT).
Kết quả 21/10/2026: 424 frame (cx min −112, f3483–3485); x −112..−33 trời thuần; x −32..−1 hàng 136..183 = 17 ô đá rêu (rms 8–31, nhì ≥ 86);
kiểm chứng x 0..63: 176/176 ô đúng.
"""
import csv
import os
import sys

import numpy as np
from PIL import Image

TOOLS = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(TOOLS))
WORK2 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '05', 'work')
SHEET = os.path.join(ROOT, 'Art_Source', 'spriters-resource', 'level3_walkway.png')
SEC = [("WalkwayEntranceA", [1, 425, 768, 192]), ("WalkwayEntranceB", [1, 218, 768, 192]), ("WalkwayA", [774, 218, 768, 192]),
       ("WalkwayB", [1286, 11, 768, 192]), ("MummyMan", [2059, 11, 1536, 192])]
SKY = (36, 24, 140)
X0, X1 = -128, 128


def main():
    os.makedirs(OUT, exist_ok=True)
    nes = np.load(os.path.join(WORK2, 'map3_nes.npy'), mmap_mode='r')     # (N,224,256,3) NES y 8..231
    cam = list(csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv'))))
    z = np.load(os.path.join(WORK2, 'map3_lut.npz'))
    L = {int(k): v for k, v in zip(z['k'], z['v'])}
    # 1. nền trung vị
    acc = {}
    nfr = 0
    cxmin = 999
    for c in cam:
        if c['section'] != 'WalkwayB' or int(c['cx']) >= X1:
            continue
        f, cx = int(c['frame']), int(c['cx'])
        nfr += 1
        cxmin = min(cxmin, cx)
        img = np.asarray(nes[f][24:216], float)                              # NES y 32..223
        for xs in range(256):
            wx = cx + xs
            if X0 <= wx < X1:
                acc.setdefault(wx, []).append(img[:, xs])
    med = np.zeros((192, X1 - X0, 3), float)
    for wx, cols in acc.items():
        med[:, wx - X0] = np.median(np.stack(cols, 0), 0)
    print('frame WalkwayB cx < %d: %d (cx min %d)' % (X1, nfr, cxmin))
    # 2. thư viện tile (đổi màu sheet→video)
    S = np.asarray(Image.open(SHEET).convert('RGB')).astype(int)
    lib, order = {}, []
    for sid, (x, y, w, h) in SEC:
        reg = S[y:y + h, x:x + w]
        for ty in range(0, h, 8):
            for tx in range(0, w, 8):
                t = reg[ty:ty + 8, tx:tx + 8]
                k = t.tobytes()
                if k not in lib:
                    lib[k] = len(order)
                    order.append((t, sid, tx, ty))

    def tovid(t):
        o = np.zeros(t.shape, float)
        for i in range(8):
            for j in range(8):
                r, g, b = (int(v) for v in t[i, j])
                o[i, j] = L.get((r << 16) | (g << 8) | b, np.array([r, g, b], float))
        return o
    TV = np.stack([tovid(t[0]) for t in order], 0)
    wb = S[11:203, 1286:1286 + 768]
    wb_first = {}
    for ty in range(0, 192, 8):
        for tx in range(0, 768, 8):
            wb_first.setdefault(wb[ty:ty + 8, tx:tx + 8].tobytes(), (tx, ty))
    sky_k = lib[np.full((8, 8, 3), SKY).astype(S.dtype).tobytes()]
    ok = bad = 0
    rec = np.zeros((192, X1 - X0, 3), float)
    rec[:] = TV[sky_k][0, 0]
    cells = []
    for ty in range(16, 192, 8):
        for wx in range(-112, 64, 8):
            b = med[ty:ty + 8, wx - X0:wx - X0 + 8]
            d = ((b[None] - TV) ** 2).sum((1, 2, 3)) / 64.0
            o = np.argsort(d)
            k1, k2 = int(o[0]), int(o[1])
            rec[ty:ty + 8, wx - X0:wx - X0 + 8] = TV[k1]
            if wx >= 0:
                truth = lib[wb[ty:ty + 8, wx:wx + 8].tobytes()]
                if truth == k1:
                    ok += 1
                else:
                    bad += 1
                    print('KIEM x%4d y%3d: khop %s(%d,%d), sheet that %s(%d,%d)' % ((wx, ty) + order[k1][1:] + order[truth][1:]))
            elif k1 != sky_k:
                src = wb_first.get(order[k1][0].tobytes())
                cells.append((wx, ty, src))
                print('x%4d y%3d -> %s(%d,%d) rms %5.1f | nhi %s(%d,%d) rms %5.1f | trong WalkwayB: %s' % (
                    (wx, ty) + order[k1][1:] + (d[k1] ** .5,) + order[k2][1:] + (d[k2] ** .5, src)))
    print('kiem chung x 0..63 (hang 16..191): dung %d / sai %d' % (ok, bad))
    print('PAD_LEFT cells:', [(wx, ty, 'WalkwayB') + tuple(src) for wx, ty, src in cells])
    top = np.clip(med, 0, 255).astype(np.uint8)
    bot = np.clip(rec, 0, 255).astype(np.uint8)
    both = np.concatenate([top, np.zeros((4, X1 - X0, 3), np.uint8), bot], 0)
    Image.fromarray(both).resize(((X1 - X0) * 3, both.shape[0] * 3), Image.NEAREST).save(os.path.join(OUT, 'wb_pad_compare.png'))


if __name__ == '__main__':
    main()
