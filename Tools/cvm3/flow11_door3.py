"""CVM3 11 — cửa PHẢI Block 3 từng frame video Map_3 (khuôn Tools/cvm2/flow11_door.py của CVM2 11, nhưng cửa phải KHÔNG lật):
door_1 (rộng 24) ở x cột + dx, door_0 (hẹp 16) ở x cột + dx + 8, đỉnh = đỉnh cột + 1 (như Map 1 DOOR_SPRITE_Y 33 = 32 + 1; Map 2 DOOR_DY 1).
dx dò vị trí tự do (bảng màu cố định (2,0,0)/(204,13,6), cả 2 chiều lật — KHÔNG lật thắng): cửa #1 dx −4 (f2504 door_0 x màn 116,
f2510 door_1 108 @ cxw 640, cột 752 — cột đỏ x màn 115–116), cửa #2 dx +8 (f4402/4404 door_0 128, f4406/4410/4450 door_1 120 @ cxw 640,
cột cũng ở 115–116) ⇒ 2 cửa lệch nhau 12 px so với cột (Map 1: dx 0).
Camera = đồng hồ khi cuộn 1 px/tick (cxw ghép từng frame ref/data/map3_camera.csv) ⇒ mốc tick của 2 lần cuộn bằng hồi quy
(tick = frame × 2.0033); trạng thái cánh cửa + Simon (map3_simon.csv) từng frame ⇒ mốc từng bước tính từ lúc cuộn 1 xong.

Chạy (từ gốc repo; cần ClaudeNotes/cvm3/_cache/02/work/map3_nes.npy của CVM3 02):
  python -P -E Tools/cvm3/flow11_door3.py <f0> <f1> <x cột cửa sheet> <y đỉnh cột section> <dx door_1>   (cửa #1: 2425 2650 752 80 -4; cửa #2: 4320 4550 752 48 8)
In mỗi frame: frame, dup, cxw, trạng thái (door_1 / door_0 / -), sai số 2 mẫu, sprite + x Simon; cuối: hồi quy cuộn + mốc (tick tương đối).
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm2'))
import enemy8_fit as E8  # noqa: E402

WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
TPF = 2.0033


def main():
    f0, f1, colx, coly, dx = [int(v) for v in sys.argv[1:6]]
    mm = np.load(os.path.join(WORK, 'map3_nes.npy'), mmap_mode='r')
    dup = np.load(os.path.join(WORK, 'map3_dup.npy'))
    cam = {}
    for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_camera.csv'))):
        cam[int(r['frame'])] = (r['section'], int(float(r['cx'])), int(float(r['cxw'])))
    sim = {}
    for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_simon.csv'))):
        sim[int(r['frame'])] = (r['sprite'], r['world_x'], r['y_sec'], r['px'])
    sprites, cols = E8.load(['Object/door_0', 'Object/door_1'])
    d0 = [s for s in sprites if s['name'].endswith('door_0') and not s['flip']][0]
    d1 = [s for s in sprites if s['name'].endswith('door_1') and not s['flip']][0]
    ytop = 32 + coly + 1
    pal = None
    rows = []
    for f in range(f0, f1 + 1):
        nes = np.asarray(mm[f]).astype(float)
        c = cam.get(f, ('?', -1, -1))
        cxw = c[2]
        sx = colx - cxw      # x màn cột cửa (khung ghép: cột ở x sheet colx của section nguồn)
        if pal is None:
            # bảng màu video của cửa: lấy từ frame cột cửa đóng (đen + đỏ) — đỏ = trung vị pixel màu đậm trong ô cột
            pal = [(2.0, 0.0, 0.0), (204.0, 13.0, 6.0)]   # trung vị dò tự do f2510 (door_1)
        def err_at(s, x):
            if x < 0 or x + s['w'] > 256:
                return 9.0
            acc = 0.0
            for k in range(len(s['ys'])):
                v = nes[ytop + s['ys'][k] - E8.Y0, x + s['xs'][k]]
                acc += min(np.sqrt(((v - np.array(pal[s['cls'][k]])) ** 2).sum()) / 441.7, E8.CLIP)
            return acc / len(s['ys'])
        e0, e1 = err_at(d0, sx + dx + 8), err_at(d1, sx + dx)
        st = 'door_1' if e1 < 0.06 else 'door_0' if e0 < 0.05 else '-'
        s = sim.get(f, ('', '', '', ''))
        rows.append((f, int(dup[f]), cxw, st, e0, e1, s[0], s[1]))
        print(f, 'dup' if dup[f] else '   ', c[0], cxw, st, '%.3f %.3f' % (e0, e1), s[0], s[1], s[2])
    # hồi quy cuộn: cxw = base + (tick − T) với base 512 (cuộn 1) / 640 (cuộn 2)
    def fit(lo, hi, base):
        pts = [(f * TPF, cxw) for (f, d, cxw, *_r) in rows if not d and lo < cxw < hi]
        if len(pts) < 3:
            return None, 0, 0
        t = np.array([p[0] for p in pts]); x = np.array([p[1] for p in pts], float)
        a, b = np.polyfit(t, x, 1)
        T = np.median(t - (x - base))   # với tốc độ 1 px/tick: tick bắt đầu
        return T, a, len(pts)
    T1, v1, n1 = fit(513, 640, 512)
    T2, v2, n2 = fit(641, 768, 640)
    print('\ncuon 1: bat dau tick %.1f (f%.1f), toc do %.3f px/tick, %d diem; xong tick %.1f (f%.1f)' % (T1, T1 / TPF, v1, n1, T1 + 128, (T1 + 128) / TPF))
    print('cuon 2: bat dau tick %.1f (f%.1f), toc do %.3f px/tick, %d diem; xong tick %.1f (f%.1f)' % (T2, T2 / TPF, v2, n2, T2 + 128, (T2 + 128) / TPF))
    z = T1 + 128
    print('cuon2 - cuon1 xong = %.1f tick' % (T2 - z))
    # mốc đổi trạng thái cánh cửa (frame đầu tiên của trạng thái mới, bỏ frame lặp)
    prev = None
    for (f, d, cxw, st, e0, e1, sp, wx) in rows:
        if d:
            continue
        if st != prev:
            print('f%d  %-6s  tick tuong doi (cuon 1 xong) %+.1f' % (f, st, f * TPF - z))
            prev = st
    # Simon tự đi: frame đầu x đổi sau khi cửa mở
    lastx = None
    for (f, d, cxw, st, e0, e1, sp, wx) in rows:
        if d or not wx:
            continue
        x = float(wx)
        if lastx is not None and x != lastx and f * TPF > z:
            print('Simon doi x f%d %.0f -> %.0f  tick tuong doi %+.1f' % (f, lastx, x, f * TPF - z))
        lastx = x


if __name__ == '__main__':
    main()
