"""CVGP 13 — so quỹ đạo enemy Unity (EnemyProbe.SaveTraces ⇒ _cache/13/traces/trace_<kịch bản>.csv, px section, theo tick) với vết
video (enemy_fit.py + enemy_track.py ⇒ _cache/13/trk_<tag>.csv, toạ độ NES màn + world x; 1 frame video = 2 tick).
Quy đổi video → section: x = world x (camera video = section từ 0), Y = pivy − 33 (đi đất: pivot vẽ ở mặt sàn + 1, NES = section + 32)
hoặc pivy − 32 (dơi: pivot tâm). Căn thời gian theo mốc ghi trong CASES (sinh / bật dậy / đỉnh nhảy). In RMS + max |dx|, |dy|.
Chạy (từ gốc repo): python -P -E Tools/cvgp/enemy_compare.py
"""
import csv, os, sys, collections
import numpy as np

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '13')


def unity(scn):
    """Vết theo thứ tự trong file (mỗi vết bắt đầu ở k = 0), sắp theo tick sinh (ổn định)."""
    tr = []
    for r in csv.DictReader(open(os.path.join(D, 'traces', 'trace_%s.csv' % scn), encoding='utf-8')):
        if int(r['k']) == 0:
            tr.append([])
        tr[-1].append((int(r['rel']), int(r['x']), int(r['y']), r['state']))
    return sorted(tr, key=lambda t: t[0][0] - 0)


def video(tag, track, ydrop):
    out = []
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        if int(r['track']) == track:
            out.append((2 * int(r['frame']), int(r['worldx']), int(r['pivy']) - ydrop))
    return out


def compare(name, u, v, t0u, t0v, tmax=None, only_x=False):
    ud = {rel: (x, y) for rel, x, y, _ in u}
    dx, dy = [], []
    for tv, x, y in v:
        t = tv - t0v
        if tmax is not None and t > tmax:
            break
        rel = int(round(t0u + t))
        if rel in ud:
            dx.append(ud[rel][0] - x)
            dy.append(ud[rel][1] - y)
    if not dx:
        print('%-34s không có điểm chung' % name)
        return
    dx, dy = np.array(dx, float), np.array(dy, float)
    print('%-34s n%3d  dx rms %.2f max %.0f (TB %+.2f) | dy rms %.2f max %.0f (TB %+.2f)' % (
        name, len(dx), np.sqrt((dx ** 2).mean()), np.abs(dx).max(), dx.mean(),
        np.sqrt((dy ** 2).mean()), np.abs(dy).max(), dy.mean()))


def first_state(track, st):
    for rel, x, y, s in track:
        if s == st:
            return rel
    return None


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    # Zombie EH: video sinh (ngoại suy về pivot màn 248) tick 1200.9 / 1235.7 / 1264.9; Unity sinh rel 0 / 32 / 64
    g = unity('ghoul_spawner')
    for i, (trk, tv0) in enumerate(((0, 1200.9), (1, 1235.7), (2, 1264.9))):
        compare('Zombie EH #%d (f600–706)' % (i + 1), g[i], video('z_eh', trk, 33), g[i][0][0], tv0, tmax=170)
    # Zombie PB: video f5127 = tick 10254 ↔ Unity rel 0 (Put trước tick 0 ⇒ rel đầu = 1 tick sau sinh)
    p = unity('ghoul_pb_video')
    compare('Zombie PB gờ trên #1 (f5129–5271)', p[0], video('z_pb', 0, 33), -1, 10254)
    compare('Zombie PB sàn dưới (f5129–5257)', p[1], video('z_pb', 1, 33), -1, 10254)
    compare('Zombie PB gờ trên #2 (f5145–5232)', p[2], video('z_pb', 2, 33), 32, 10254 + 33)
    # Báo: căn mốc BẬT DẬY (video #1 f1263 = tick 2526 (nội suy 1261.8–1263); #3 f1768.4 = 3536.8)
    for scn, trk, tv in (('panther_eh1', 0, 2526.0), ('panther_eh3', 4, 3536.8)):
        u = unity(scn)[0]
        tr = first_state(u, 'Run')
        compare('Báo %s (bật→chạy→nhảy→đáp)' % scn[-3:], u, video('leo', trk, 33), tr, tv)
        compare('Báo %s sau đáp (chạy về Simon)' % scn[-3:], u, video('leo', trk, 33) if trk == 4 else video('leo', 1, 33), tr, tv)
    # Dơi CA: video sinh (ngoại suy về pivot 248 với 1.125 px/tick) f2760 − 3 frame ≈ tick 5514; Unity rel sinh
    b = unity('bat_ca')[0]
    compare('Dơi CA #1 (f2760–2804)', b, video('bat_a', 0, 32), b[0][0], 5514)
    # Fish Man CB: căn ĐỈNH (video khớp parabol: f3536.9 / f3536.7 ⇒ tick 7073.8 / 7073.4; Unity đỉnh rel 45)
    f = unity('fishman_video')
    for i, (trk, apex) in enumerate(((0, 7073.8), (1, 7073.4))):
        ymin = min(y for _, _, y, _ in f[i])
        kap = [rel for rel, _, y, _ in f[i] if y == ymin]
        ua = (kap[0] + kap[-1]) / 2.0
        compare('Fish Man x%d (f3517–3575)' % f[i][0][1], f[i], video('fish', trk, 33), ua, apex)


if __name__ == '__main__':
    main()
