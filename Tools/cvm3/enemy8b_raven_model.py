"""CVM3 08 — mô phỏng Raven ĐÚNG logic Unity (fixed-point 1/256, thứ tự tick của StateMachine: Execute rồi Check, state mới chạy nối
cùng tick) trên Simon "rối" (ref/data/enemy8b/puppet_<đoạn>.csv) ⇒ so vết video (trk_rv_all.csv, _cache/08) ⇒ rms; chọn vy0 từng
chặng lướt trong lưới {min + k·step} (phần NGẪU NHIÊN — Map3EnemyProbe ép theo đây). Dùng để chốt tham số trước khi sửa C#.
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_raven_model.py [decel256=9] [dashAcc256=8] [dashV0_256=32] [brake256=64] [hover=25] [turnHover=14] [perch=60]
Mô hình (CVM3 08 §2h): Perch tới ageTicks ≥ perch ⇒ Glide: vx = ±glideVx về Simon, vy = ±vy0 về TÂM THÂN Simon (chân − 16), mỗi tick
dời rồi |vy| −= decel (không đổi dấu) ⇒ vy = 0 ⇒ Hover (đứng hover tick) ⇒ cao hơn tâm Simon ⇒ Dash, thấp hơn ⇒ Glide lại; Dash: vx = ±v0
về Simon, mỗi tick |vx| += acc (trần), y giữ; vượt qua X Simon lần đầu ⇒ Brake (|vx| −= brake tới 0) ⇒ Hover turnHover ⇒ Dash lại; lần
vượt thứ 2 ⇒ bay tiếp ra khỏi màn. Hướng mặt luôn về Simon.
"""
import csv, itertools, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C8 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')
PUP = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy8b')
TPF = 2.0033
XOFF = {'WalkwayB': 112}
# đoạn: (tên puppet, section, frame đầu, [(track id video, frame hiện, x, y)])  — x, y = vết video lúc hiện (gốc sheet)
SEGS = [
    ('rv_web0', 'WalkwayEntranceB', 1338, [(0, 1340, 280, 41)]),
    ('rv_web1', 'WalkwayEntranceB', 1572, [(1, 1574, 423, 89), (2, 1634, 487, 57), (3, 1654, 527, 169)]),
    ('rv_wb0', 'WalkwayB', 3592, [(4, 3594, 271, 57)]),
    ('rv_wb1', 'WalkwayB', 3863, [(5, 3865, 487, 121), (6, 3898, 551, 121)]),
    ('rv_mm', 'MummyMan', 5084, [(11, 5086, 747, 41)]),
]


def puppet(name):
    return [(int(r['x']), int(r['y'])) for r in csv.DictReader(open(os.path.join(PUP, 'puppet_%s.csv' % name)))]


def sgn(v):
    return (v > 0) - (v < 0)


def simulate(P, sim, spawn_rel, x0, y0, vy0s, T):
    fx, fy = x0 << 8, y0 << 8
    st, sf, age = 'Perch', 0, 0
    vx = vy = 0
    gl = 0
    turns = P['turns']
    after_brake = False
    hover_len = P['hover']
    out = {}
    d = 0
    for t in range(spawn_rel, T):
        sx, sy = sim[min(t, len(sim) - 1)]
        age += 1
        chain = 0
        sf += 1
        while True:
            X, Y = fx >> 8, fy >> 8
            changed = None
            # Execute
            if st == 'Glide':
                fx += vx
                fy += vy
                if vy > 0:
                    vy = max(0, vy - P['decel'])
                elif vy < 0:
                    vy = min(0, vy + P['decel'])
                if vy == 0:
                    changed = 'Hover'
                    hover_len = P['hover']
                    after_brake = False
            elif st == 'Dash':
                v = min(abs(vx) + P['acc'], P['vmax'])
                vx = d * v
                fx += vx
                X = fx >> 8
                if turns > 0 and (sx - X) * d < 0:
                    turns -= 1
                    changed = 'Brake'
            elif st == 'Brake':
                v = max(0, abs(vx) - P['brake'])
                vx = sgn(vx) * v
                fx += vx
                if v == 0:
                    changed = 'Hover'
                    hover_len = P['turnHover']
                    after_brake = True
            # Check
            X, Y = fx >> 8, fy >> 8
            if st == 'Perch' and age >= P['perch']:
                changed = 'Glide'
            elif st == 'Hover' and sf > hover_len:
                changed = 'Dash' if (after_brake or Y < sy - 16) else 'Glide'
            if changed is None or chain >= 4:
                break
            chain += 1
            st, sf = changed, 1
            if st == 'Glide':
                d = sgn(sx - X) or -1
                vx = d * P['glideVx']
                v0 = vy0s[gl] if gl < len(vy0s) else vy0s[-1]
                gl += 1
                vy = (sgn(sy - 16 - Y) or -1) * v0
            elif st == 'Dash':
                d = sgn(sx - X) or -1
                vx = d * P['v0']
            # state mới chạy nối cùng tick
        out[t] = (fx >> 8, fy >> 8, st)
    return out


def video_track(tid, sec):
    pts = []
    for r in csv.DictReader(open(os.path.join(C8, 'trk_rv_all.csv'))):
        if int(r['track']) == tid:
            pts.append((int(r['frame']), int(r['worldx']) + XOFF.get(sec, 0), int(r['ysec']), r['sprite']))
    return pts


def main():
    a = [int(v) for v in sys.argv[1:]]
    P = dict(decel=a[0] if len(a) > 0 else 9, acc=a[1] if len(a) > 1 else 8, v0=a[2] if len(a) > 2 else 32,
             brake=a[3] if len(a) > 3 else 64, hover=a[4] if len(a) > 4 else 25, turnHover=a[5] if len(a) > 5 else 14,
             perch=a[6] if len(a) > 6 else 60, glideVx=192, vmax=768, turns=1)
    grid = [320, 384, 448, 512]          # 1.25, 1.5, 1.75, 2.0 (× 256)
    tot_n = tot_e = 0
    lines = ['tham số %s' % P]
    for name, sec, f0, ravens in SEGS:
        sim = puppet(name)
        T = len(sim)
        for tid, fa, x, y, in ravens:
            vt = video_track(tid, sec)
            rel = int(round((fa - f0) * TPF))
            best = None
            for k in (1, 2):
                for combo in itertools.product(grid, repeat=k):
                    out = simulate(P, sim, rel, x + XOFF.get(sec, 0), y, list(combo), T)
                    for off in (-2, -1, 0, 1, 2):
                        d = []
                        for f, vx_, vy_, sp in vt:
                            t = int(round((f - f0) * TPF)) + off
                            if t in out:
                                d.append((out[t][0] - vx_, out[t][1] - vy_))
                        if len(d) < 5:
                            continue
                        e = np.array(d, float)
                        r = float(np.sqrt((e ** 2).sum(1).mean()))
                        if best is None or r < best[0]:
                            best = (r, combo, off, len(d), float(np.abs(e).max()))
            r, combo, off, n, mx = best
            tot_n += n
            tot_e += r * r * n
            lines.append('%s raven #%d hiện f%d rel %d: vy0 %s lệch %+d tick ⇒ rms %.2f px (max %.0f, %d điểm)' % (
                name, tid, fa, rel, '/'.join('%.2f' % (c / 256.0) for c in combo), off, r, mx, n))
    lines.append('TỔNG rms %.2f px trên %d điểm' % (np.sqrt(tot_e / tot_n), tot_n))
    sys.stdout.buffer.write(('\n'.join(lines) + '\n').encode('utf-8'))


if __name__ == '__main__':
    main()
