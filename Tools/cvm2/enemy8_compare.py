"""CVM2 08 — so quỹ đạo enemy Block 2: video Map_2 (vết trk_<tag>.csv của enemy8_track.py) ↔ Unity (Map2EnemyProbe ⇒ <dir>/<đoạn>.csv).
Toạ độ so = vị trí VẼ trong section: video (pivot x, pivot y − 32); Unity (X, Y + viewOffset.y của prefab: SpearKnight/BoneTower 1, còn lại 0).
Thời gian: video frame f ↔ tick (f − f0)·2.0033; Unity rel r (thử lệch −1/0/+1 tick, báo lệch tốt nhất). Mỗi vết video ghép với thực thể Unity
cùng loại có rms nhỏ nhất (≥ 5 điểm chung). In: rms x/y/tổng, lệch lớn nhất, tick xuất hiện video ↔ Unity; + mốc sự kiện từng đoạn.
Chạy (từ gốc repo): python -P -E Tools/cvm2/enemy8_compare.py <unity_dir> [out=ClaudeNotes/cvm2/ref/data/enemy8/compare.txt]
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08')
TPF = 2.0033
VOY = {'SpearKnight': 1, 'BoneTower': 1}
OUTLIER = 16   # px — điểm video lệch > 16 px so Unity = ghép nhầm vật khác (bộ dò vết), loại khi còn ≥ 80 % điểm
# đoạn: f0, f1, [(tag, loại Unity, errmax, lọc)]
SEGS = {
    'mh_a': (1762, 2060, [('mhz_a', 'MedusaHead', 0.09, None)]),
    'mh_b': (2801, 3160, [('mhz_b', 'MedusaHead', 0.09, None)]),
    'mh_m': (6180, 6580, [('mhz_m', 'MedusaHead', 0.09, None)]),
    'sk_teb': (863, 1008, [('sk_teb', 'SpearKnight', 0.08, None)]),
    'sk_hb': (3209, 3420, [('sk_hb', 'SpearKnight', 0.08, lambda r: int(r['pivy']) < 150)]),
    'gh_1': (4976, 5007, [('gh_c', 'Ghost', 0.09, None)]),
    'gh_2': (5810, 5851, [('gh_c2', 'Ghost', 0.09, None)]),
    'bat_c': (4974, 5101, [('bat_c', 'Bat', 0.08, lambda r: int(r['pivy']) > 40)]),
    'bat_tea': (187, 776, [('bat_tea', 'Bat', 0.05, None)]),
    'bt_c': (4922, 5530, [('bt_fb', 'BoneTowerFireball', 0.08, None)]),
}


def video_tracks(tag, f0, f1, errmax, filt):
    p = os.path.join(D, 'trk_%s.csv' % tag)
    rows = []
    if os.path.exists(p):
        rows = list(csv.DictReader(open(p)))
        key = 'track'
    else:   # chỉ có det (vd dơi treo tĩnh) ⇒ 1 vết
        rows = list(csv.DictReader(open(os.path.join(D, 'det_%s.csv' % tag))))
        for r in rows:
            r['track'] = '0'
    tr = {}
    for r in rows:
        f = int(r['frame'])
        if f < f0 or f > f1 or float(r['err']) > errmax:
            continue
        if filt and not filt(r):
            continue
        tr.setdefault(int(r['track']), []).append(((f - f0) * TPF, int(r['worldx']), int(r['pivy']) - 32, int(r['flip']), r['sprite']))
    return {k: v for k, v in tr.items() if len(v) >= 3}


def unity(path):
    ent = {}
    for r in csv.DictReader(open(path)):
        k = int(r['id'])
        e = ent.setdefault(k, dict(type=r['type'], pts={}))
        e['pts'][int(r['rel'])] = (int(r['x']), int(r['y']) + VOY.get(r['type'], 0), int(r['facing']), r['state'], r['anim'], int(r['frame']))
    return ent


def match(vt, ue, off):
    d = []
    for (t, x, y, fl, sp) in vt:
        r = int(round(t)) + off
        if r in ue['pts']:
            ux, uy = ue['pts'][r][0], ue['pts'][r][1]
            d.append((ux - x, uy - y))
    if len(d) < 5 or len(d) < 0.5 * len(vt):
        return None
    a = np.array(d, float)
    keep = np.abs(a).max(1) <= OUTLIER   # điểm ghép nhầm của bộ dò vết (khác vật) — loại, báo số điểm loại
    if keep.sum() < 0.8 * len(a):
        keep[:] = True
    drop = int((~keep).sum())
    a = a[keep]
    return dict(n=len(a), drop=drop, rx=float(np.sqrt((a[:, 0] ** 2).mean())), ry=float(np.sqrt((a[:, 1] ** 2).mean())),
                r=float(np.sqrt((a ** 2).sum(1).mean())), mx=float(np.abs(a).max()), bx=float(a[:, 0].mean()), by=float(a[:, 1].mean()))


def main():
    udir = sys.argv[1]
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'enemy8', 'compare.txt')
    lines = []
    allr = {}
    for seg, (f0, f1, specs) in SEGS.items():
        p = os.path.join(udir, seg + '.csv')
        if not os.path.exists(p):
            lines.append('== %s: thiếu vết Unity' % seg)
            continue
        ue = unity(p)
        lines.append('== %s (video f%d–%d = %d tick)' % (seg, f0, f1, int((f1 - f0) * TPF)))
        for tag, typ, errmax, filt in specs:
            vts = video_tracks(tag, f0, f1, errmax, filt)
            cands = {k: e for k, e in ue.items() if e['type'] == typ}
            for k, vt in sorted(vts.items(), key=lambda kv: kv[1][0][0]):
                best = None
                for off in (-1, 0, 1):
                    for uid, e in cands.items():
                        m = match(vt, e, off)
                        if m and (best is None or m['r'] < best[0]['r']):
                            best = (m, uid, off)
                if best is None:
                    lines.append('  video #%d %s n%d t %.0f–%.0f: KHÔNG có thực thể Unity trùng thời gian' % (k, typ, len(vt), vt[0][0], vt[-1][0]))
                    continue
                m, uid, off = best
                ut = sorted(cands[uid]['pts'])
                al = None   # rms sau căn mốc thời gian (lệch ±20 tick) — tách hình dạng đường đi khỏi lệch thời điểm
                for sh in range(-20, 21):
                    mm = match(vt, cands[uid], sh)
                    if mm and (al is None or mm['r'] < al[0]['r']):
                        al = (mm, sh)
                lines.append('  video #%d %s n%d t %.0f–%.0f ↔ Unity #%d (rel %d–%d, lệch %+d tick): rms x %.2f y %.2f tổng %.2f px, max %.0f, TB lệch (%+.2f, %+.2f)%s%s' % (
                    k, typ, len(vt), vt[0][0], vt[-1][0], uid, ut[0], ut[-1], off, m['rx'], m['ry'], m['r'], m['mx'], m['bx'], m['by'],
                    (' [loại %d điểm ghép nhầm > 16 px]' % m['drop']) if m['drop'] else '',
                    (' | căn mốc %+d tick: rms %.2f' % (al[1], al[0]['r'])) if al and al[1] not in (-1, 0, 1) else ''))
                allr.setdefault(seg, []).append((m['n'], m['r']))
        # mốc sự kiện Unity
        ev = []
        for uid, e in sorted(ue.items()):
            ts = sorted(e['pts'])
            st = [e['pts'][t][3] for t in ts]
            fc = [e['pts'][t][2] for t in ts]
            chg = [ts[i] for i in range(1, len(ts)) if st[i] != st[i - 1]]
            turns = [ts[i] for i in range(1, len(ts)) if fc[i] != fc[i - 1]]
            ev.append('#%d %s rel %d–%d xuất hiện (%d,%d)%s%s' % (uid, e['type'], ts[0], ts[-1], e['pts'][ts[0]][0], e['pts'][ts[0]][1],
                                                              (' đổi state ' + ' '.join('%d:%s' % (t, e['pts'][t][3]) for t in chg[:8])) if chg else '',
                                                              (' quay ' + ' '.join(map(str, turns[:12]))) if turns and e['type'] != 'BoneTower' else ''))
        lines.append('  Unity: ' + ' | '.join(ev[:14]) + (' …(%d thực thể)' % len(ev) if len(ev) > 14 else ''))
    lines.append('== TỔNG (trung bình rms theo số điểm)')
    for seg, l in allr.items():
        n = sum(a for a, b in l)
        lines.append('  %s: %d vết, %d điểm, rms %.2f px (vết xấu nhất %.2f)' % (seg, len(l), n, sum(a * b for a, b in l) / n, max(b for a, b in l)))
    txt = '\n'.join(lines) + '\n'
    open(out, 'w', encoding='utf-8').write(txt)
    sys.stdout.buffer.write(txt.encode('utf-8'))


if __name__ == '__main__':
    main()
