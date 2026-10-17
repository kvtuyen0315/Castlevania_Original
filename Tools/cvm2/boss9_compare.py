"""CVM2 09 — so vết Queen Medusa / rắn / quả cầu Unity (Map2BossProbe) ↔ video (boss9_fit.py det_*.csv), rms px trên lưới NES.

Chạy (từ gốc repo): python -P -E Tools/cvm2/boss9_compare.py <thư mục CSV Unity> [out.txt]
  → in bảng + ghi ClaudeNotes/cvm2/ref/data/boss9/compare.txt (mặc định).
Ánh xạ thời gian: rel Unity = r0 + (frame − f0) × 2.0033; r0 tìm trong ±4 tick quanh mốc danh nghĩa (căn mốc — ghi ra).
Toạ độ: Unity X = x NES; y NES = Y + 32 + viewOffset.y (Medusa 0 — tâm đầu; rắn −1 — chân sprite, so với pivy video; quả cầu +1).
"""
import csv, math, os, sys
from collections import defaultdict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C9 = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '09')
TPF = 2.0033
VIEW_DY = {'QueenMedusa': 0, 'Snake': -1, 'ItemPickup': 1}


def video(tag, f0, f1, emax=0.15, near=None, kind=None):
    """điểm tốt nhất mỗi frame (gần 'near(f)' nếu có) — [(f, x, y)]"""
    d = defaultdict(list)
    for r in csv.DictReader(open(os.path.join(C9, 'det_%s.csv' % tag))):
        f = int(r['frame'])
        if f0 <= f <= f1 and float(r['err']) < emax and (kind is None or kind in r['sprite']):
            d[f].append((float(r['err']), int(r['pivx']), int(r['pivy'])))
    out = []
    for f in sorted(d):
        c = d[f]
        if near:
            nx = near(f)
            if nx is None:
                continue
            c = [z for z in c if abs(z[1] - nx[0]) <= nx[2] and abs(z[2] - nx[1]) <= nx[2]]
            if not c:
                continue
        e, x, y = min(c)
        out.append((f, x, y))
    return out


def unity(path):
    tr = defaultdict(dict)   # (kind, id) → rel → (x, yNES, state)
    for r in csv.DictReader(open(path)):
        k = r['kind']
        tr[(k, int(r['id']))][int(r['rel'])] = (int(r['x']), int(r['y']) + 32 + VIEW_DY.get(k, 0), r['state'])
    return tr


def match(vpts, upts, f0, r0nom, span=4):
    best = None
    for r0 in [r0nom + i * 0.25 for i in range(-span * 4, span * 4 + 1)]:
        err, n = 0.0, 0
        for f, x, y in vpts:
            rel = int(round(r0 + (f - f0) * TPF))
            if rel in upts:
                ux, uy = upts[rel][0], upts[rel][1]
                err += (ux - x) ** 2 + (uy - y) ** 2
                n += 1
        if n >= max(3, len(vpts) // 2):
            rms = math.sqrt(err / (2 * n))
            if best is None or rms < best[0]:
                best = (rms, r0, n)
    return best


def axis_rms(vpts, upts, f0, r0):
    ex, ey, n = 0.0, 0.0, 0
    for f, x, y in vpts:
        rel = int(round(r0 + (f - f0) * TPF))
        if rel in upts:
            ex += (upts[rel][0] - x) ** 2
            ey += (upts[rel][1] - y) ** 2
            n += 1
    return (math.sqrt(ex / n), math.sqrt(ey / n)) if n else (-1, -1)


def first_track(tr, kind, cond=lambda t: True):
    c = [(min(t.keys()), key) for key, t in tr.items() if key[0] == kind and cond(t)]
    c.sort()
    return [tr[k] for _, k in c]


def main():
    udir = sys.argv[1]
    outp = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'boss9', 'compare.txt')
    lines = []
    P = lambda *a: lines.append(' '.join(str(x) for x in a))
    P('CVM2 09 — so vết Unity (Map2BossProbe, %s) ↔ video (det_*.csv boss9_fit.py); rms px lưới NES; r0 = tick Unity ứng frame đầu (căn mốc ±4)' % os.path.basename(udir.rstrip('/\\')))
    P('đoạn | vết | video frame | điểm | rms (x / y) | r0 (danh nghĩa → căn)')
    rows = []

    def add(name, what, vpts, upts, f0, r0nom):
        if not vpts or not upts:
            P(name, '|', what, '| thiếu dữ liệu (video %d điểm, Unity %d)' % (len(vpts), len(upts)))
            return
        b = match(vpts, upts, f0, r0nom)
        if b is None:
            P(name, '|', what, '| không khớp được')
            return
        ax, ay = axis_rms(vpts, upts, f0, b[1])
        P('%s | %s | f%d–f%d | %d | %.2f (%.2f / %.2f) | %.2f → %.2f' % (name, what, vpts[0][0], vpts[-1][0], b[2], b[0], ax, ay, r0nom, b[1]))
        rows.append((name, what, b[0]))

    # ---------------------------------------------------------------- video boss: thức → A → dừng 1 → B (tới khi chạm lọ f135)
    tr = unity(os.path.join(udir, 'wake_b2.csv'))
    med = first_track(tr, 'QueenMedusa')[0]
    medfly = {r: v for r, v in med.items() if v[2] != 'Statue'}
    add('wake_b2', 'Medusa A (thức→dừng 1) Boss_Map_2', video('md_b2', 68, 99), medfly, 68, 1.25)
    add('wake_b2', 'Medusa dừng 1 Boss_Map_2', video('md_b2', 99, 129), medfly, 68, 1.25)
    add('wake_b2', 'Medusa B (rời dừng 1 → chạm lọ) Boss_Map_2', video('md_b2', 130, 135), medfly, 68, 1.25)
    sn = first_track(tr, 'Snake')
    if len(sn) >= 1:
        add('wake_b2', 'rắn 1 (thức) Boss_Map_2', video('sn_b2', 69, 116, 0.2, lambda f: (113 - (0 if f < 78 else int((f - 78) * 2.75)), 165, 14)), sn[0], 68, 1.25)
    if len(sn) >= 2:
        add('wake_b2', 'rắn 2 (rời dừng 1) Boss_Map_2', video('sn_b2', 132, 158, 0.2, lambda f: (57 - (0 if f < 141 else int((f - 141) * 2.75)), 165, 14)), sn[1], 68, 1.25)
    # ---------------------------------------------------------------- video Map_2: thức (bảng màu chuẩn) tới đồng hồ f6837
    tr = unity(os.path.join(udir, 'wake_m2.csv'))
    med = first_track(tr, 'QueenMedusa')[0]
    medfly = {r: v for r, v in med.items() if v[2] != 'Statue'}
    add('wake_m2', 'Medusa W (thức→đồng hồ) Map_2', video('md_m2', 6826, 6837), medfly, 6826, 3.5)
    sn = first_track(tr, 'Snake')
    if sn:
        add('wake_m2', 'rắn 1 rơi Map_2', video('sn_m2', 6836, 6837, 0.2), sn[0], 6826, 3.5)
    # ---------------------------------------------------------------- video boss: D (rời dừng 2) → dừng 3 → E (tới lửa f388) + rắn 3, 4
    tr = unity(os.path.join(udir, 'seg_de.csv'))
    med = first_track(tr, 'QueenMedusa')[0]
    add('seg_de', 'Medusa D (rời dừng 2) Boss_Map_2', video('md_b2', 323, 330), med, 323, 0.0)
    add('seg_de', 'Medusa dừng 3 Boss_Map_2', video('md_b2', 330, 361), med, 323, 0.0)
    add('seg_de', 'Medusa E (rời dừng 3 → lửa) Boss_Map_2', video('md_b2', 362, 387), med, 323, 0.0)
    sn = first_track(tr, 'Snake')
    if len(sn) >= 1:
        add('seg_de', 'rắn 3 (rời dừng 2) Boss_Map_2', video('sn_b2', 324, 387, 0.2, lambda f: (162 - (0 if f < 332 else int((f - 332) * 2.75)), 165, 16)), sn[0], 323, 0.0)
    if len(sn) >= 2:
        add('seg_de', 'rắn 4 (rời dừng 3) Boss_Map_2', video('sn_b2', 363, 418, 0.2, lambda f: (148 - (0 if f < 370 else int((f - 370) * 2.75)), 165, 16)), sn[1], 323, 0.0)
    # ---------------------------------------------------------------- video Map_2: hạ → quả cầu
    tr = unity(os.path.join(udir, 'death.csv'))
    cr = first_track(tr, 'ItemPickup')
    if cr:
        add('death', 'quả cầu hiện + rơi Map_2 (hạ f7069 = rel 1)', video('cr_m2', 7146, 7162, 0.12, lambda f: (128, 157, 30)), cr[0], 7069, 1.0)
        add('death', 'quả cầu hiện + rơi Boss_Map_2 (hạ f491 = rel 1)', video('cr_b2', 566, 590, 0.12), cr[0], 491, 1.0)
    P('')
    P('tổng: %d vết; rms trung vị %.2f px, lớn nhất %.2f px' % (len(rows), sorted(r[2] for r in rows)[len(rows) // 2] if rows else -1, max(r[2] for r in rows) if rows else -1))
    os.makedirs(os.path.dirname(outp), exist_ok=True)
    open(outp, 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    sys.stdout.buffer.write(('\n'.join(lines) + '\n').encode('utf-8'))


if __name__ == '__main__':
    main()
