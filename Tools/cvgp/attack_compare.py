"""CVGP 09 — so quỹ đạo vũ khí phụ Unity (AttackProbe.TrajectoryCsv) với vết video (proj_fit.py + simon_fit.py).

Chạy (từ gốc repo) sau khi có:
  ClaudeNotes/cvgp/_cache/09/unity_traj_{dagger,axe,holy}.csv   (AttackProbe: Simon (300,176) quay phải, bấm ở rel 2 ⇒ khung duỗi từ rel 15)
  ClaudeNotes/cvgp/_cache/09/proj_map1_dagger_2780.csv, proj_map1_axe_6225.csv, proj_boss_holy_386.csv
  ClaudeNotes/cvgp/_cache/08/fit_w09_throw.csv, fit_w09_axe.csv  (Simon cùng điểm seek với vết vũ khí)
  python -P -E Tools/cvgp/attack_compare.py
Toạ độ so = TÂM BBOX PIXEL ĐỤC của sprite (proj_fit đo vậy) tương đối pivot Simon (x, Y mặt sàn); Unity đổi từ pivot thực thể
sang tâm bbox của đúng khung đang vẽ. Thời gian: video t = 2·(frame − frame duỗi đầu tiên) tick, thử lệch δ ∈ [−4, 4] tick (mốc
khung duỗi trên video sai ±1 frame) — chọn δ có RMS nhỏ nhất. Nước thánh: video boss không thấy Simon (nháy bất tử) ⇒ chỉ so
HÌNH DẠNG (bù thêm tịnh tiến trung bình). Chỉ xét 30 tick đầu sau khi vũ khí xuất hiện.
"""
import csv, math, os
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
C9 = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '09')
C8 = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '08')
SPR = os.path.join(ROOT, 'Assets', 'Data', 'Art', 'SubWeapon')
EXT_REL = 15          # AttackProbe: bấm rel 2 + chuẩn bị 6 + vắt 7
SIMON = (300, 176)
WINDOW = 30


def bbox_center(name):
    im = Image.open(os.path.join(SPR, name + '.png')).convert('RGBA')
    w, h = im.size
    px = im.load()
    xs = [x for x in range(w) for y in range(h) if px[x, y][3] > 0]
    ys = [y for x in range(w) for y in range(h) if px[x, y][3] > 0]
    # tâm bbox so với pivot tâm (w/2, h/2) — sprite vẽ hướng PHẢI khi ném phải
    return ((min(xs) + max(xs)) / 2.0 - w / 2.0, (min(ys) + max(ys)) / 2.0 - h / 2.0)   # chỉ số pixel tâm (như proj_fit) − pivot


def unity(kind, frames, frame_ticks):
    rows = [r for r in csv.DictReader(open(os.path.join(C9, 'unity_traj_%s.csv' % kind)))]
    first = None
    pts = {}
    cls = {'dagger': 'Dagger', 'axe': 'Axe', 'holy': 'HolyWater'}[kind]
    centers = [bbox_center(f) for f in frames]
    for r in rows:
        if r['kind'] != cls:
            continue
        rel = int(r['rel'])
        if first is None:
            first = rel
        k = ((rel - first) // frame_ticks) % len(frames)
        cx, cy = centers[k]
        pts[rel - EXT_REL] = (int(r['x']) + cx - SIMON[0], int(r['y']) + cy - SIMON[1])
    return pts, first - EXT_REL


def video(path, f0, f1, errmax, mirror=False, simon=None):
    pts = []
    last = None
    for r in csv.DictReader(open(path)):
        f = int(r['frame'])
        if f < f0 or f > f1 or float(r['err']) > errmax:
            continue
        p = (float(r['cx']), float(r['cy']))
        if p == last:
            continue
        last = p
        x, y = p
        if simon:
            x, y = x - simon[0], y - simon[1]
        if mirror:
            x = -x
        pts.append((f, x, y))
    return pts


def compare(name, upts, vpts, ext_frame, shape=False):
    best = None
    for d in range(-4, 5):
        res = []
        for f, x, y in vpts:
            t = 2 * (f - ext_frame) + d
            if t in upts:
                ux, uy = upts[t]
                res.append((x - ux, y - uy, t))
        if len(res) < 3:
            continue
        if shape:
            mx = sum(r[0] for r in res) / len(res)
            my = sum(r[1] for r in res) / len(res)
            res = [(r[0] - mx, r[1] - my, r[2]) for r in res]
        t0 = min(r[2] for r in res)
        res = [r for r in res if r[2] - t0 <= WINDOW]
        rms = math.sqrt(sum(r[0] ** 2 + r[1] ** 2 for r in res) / len(res))
        if best is None or rms < best[0]:
            best = (rms, d, res)
    rms, d, res = best
    mx = max(abs(r[0]) for r in res)
    my = max(abs(r[1]) for r in res)
    within1 = sum(1 for r in res if abs(r[0]) <= 1.0 and abs(r[1]) <= 1.0)
    print('%-28s n=%2d  lech thoi gian %+d tick  RMS %.2f px  max |dx| %.1f |dy| %.1f  (<=1 px: %d/%d)%s' % (
        name, len(res), d, rms, mx, my, within1, len(res), '  [chi hinh dang]' if shape else ''))
    print('    ' + ' '.join('t%d:(%+.1f,%+.1f)' % (r[2], r[0], r[1]) for r in res))
    return rms, mx, my


def main():
    ud, s_d = unity('dagger', ['dagger_0'], 8)
    ua, s_a = unity('axe', ['axe_0', 'axe_1', 'axe_2', 'axe_3'], 4)
    uh, s_h = unity('holy', ['holywater_bottle'], 8)
    print('Unity: vu khi xuat hien %d / %d / %d tick sau khi khung duoi bat dau (dao / riu / lo)' % (s_d, s_a, s_h))
    # dao 1 — Map_1 1:33.0: Simon (48,112) quay phải; khung duỗi đầu f2793 (fit_w09_throw cùng điểm seek 2780)
    compare('dao Map_1 1:33.0', ud, video(os.path.join(C9, 'proj_map1_dagger_2780.csv'), 2795, 2806, 0.12, simon=(48, 112)), 2793)
    # dao 2 — trên thang 1:47.1: Simon (57,216); khung duỗi đầu f3211 (seek 3195 cho cả hai)
    compare('dao Map_1 1:47.1 (thang)', ud, video(os.path.join(C9, 'proj_map1_dagger_3195.csv'), 3212, 3220, 0.12, simon=(57, 216)), 3211)
    # rìu — Map_1 3:27.9 / 3:29.3: Simon (143,208); khung duỗi đầu f6237 / f6278 (seek 6225)
    compare('riu Map_1 3:27.9', ua, video(os.path.join(C9, 'proj_map1_axe_6225.csv'), 6239, 6252, 0.12, simon=(143, 208)), 6237)
    compare('riu Map_1 3:29.3', ua, video(os.path.join(C9, 'proj_map1_axe_6225.csv'), 6281, 6294, 0.12, simon=(143, 208)), 6278)
    # nước thánh — video Boss 13.0–13.9 s (ném TRÁI ⇒ lật x), Simon không thấy ⇒ so hình dạng; mốc duỗi ước lượng = frame đầu − 2
    compare('nuoc thanh Boss 13.0 s', uh, video(os.path.join(C9, 'proj_boss_holy_386.csv'), 390, 403, 0.2, mirror=True), 390 - 2, shape=True)
    compare('nuoc thanh Boss 13.5 s', uh, video(os.path.join(C9, 'proj_boss_holy_386.csv'), 404, 418, 0.2, mirror=True), 404 - 2, shape=True)


if __name__ == '__main__':
    main()
