"""CVM3 09 — so vết Unity (Map3BossProbe ⇒ <outDir>/<đoạn>.csv) với vết video (ref/data/boss9m/trk_*.csv + _cache/09/det_cr_m3.csv) ⇒ rms px.
Chạy (từ gốc repo): python -P -E Tools/cvm3/boss9m_compare.py <outDir> [ra=ClaudeNotes/cvm3/ref/data/boss9m/compare.txt]
Ghép thời gian: tick Unity rel ↔ frame video f0 + rel / 2.0033 (f0 = frame đầu đoạn: m3_open 5931, b3_open 374, m3_death 6680) — so ở frame
video KHÔNG lặp có phát hiện (ok). Mummy: video L/R ↔ Unity MummyL (con 2, x 1312) / MummyR (con đầu, 1503); Unity Y = video ysec − 1
(viewOffset +1). Băng: ghép băng video ↔ băng Unity sinh gần nhất (thời điểm, x sinh), so (x, y) tâm (Unity Y = ysec — viewOffset 0).
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
DATA = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'boss9m')
C09 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '09')
T = 2.0033
# đoạn: f0, video, [(nhãn, ai, f_a, f_b)], [(băng video id, f_a, f_b)]
SEGS = {
    'm3_open': (5931, 'm3', [('Mummy phải (R) thức → đi trái → ném B0 → đi', 'R', 5931, 6100),
                             ('Mummy trái (L) thức → quay → mép → tới → ném B1 → B2', 'L', 5931, 6150)],
                [0, 1, 2]),
    'b3_open': (374, 'b3', [('Mummy trái (L) thức → đi phải → ném B1 (phải)', 'L', 374, 540),
                            ('Mummy phải (R) thức → đi trái → quay → ném B0', 'R', 374, 500)],
                [0, 1]),
}
lines = []


def P(s):
    lines.append(s)
    print(s.encode('ascii', 'replace').decode())


def unity(outdir, seg):
    rows = list(csv.DictReader(open(os.path.join(outdir, seg + '.csv'))))
    mm = {'L': {}, 'R': {}}
    bands = {}
    for r in rows:
        rel = int(r['rel'])
        if r['kind'] == 'MummyL':
            mm['L'][rel] = (int(r['x']), int(r['y']), r['state'])
        elif r['kind'] == 'MummyR':
            mm['R'][rel] = (int(r['x']), int(r['y']), r['state'])
        elif r['kind'] == 'MummyBandage':
            bands.setdefault(int(r['id']), {})[rel] = (int(r['x']), int(r['y']))
    return mm, bands, rows


def video_mm(v):
    return [r for r in csv.DictReader(open(os.path.join(DATA, 'trk_mm_%s.csv' % v)))]


def video_bd(v):
    by = {}
    for r in csv.DictReader(open(os.path.join(DATA, 'trk_bd_%s.csv' % v))):
        by.setdefault(int(r['id']), []).append((int(r['frame']), int(r['x']), int(r['y'])))
    return by


def main():
    outdir = sys.argv[1]
    dst = sys.argv[2] if len(sys.argv) > 2 else os.path.join(DATA, 'compare.txt')
    allr = []
    for seg, (f0, v, mlist, blist) in SEGS.items():
        if not os.path.exists(os.path.join(outdir, seg + '.csv')):
            continue
        um, ub, _ = unity(outdir, seg)
        vm = video_mm(v)
        P('## %s (f0 %d)' % (seg, f0))
        for lab, who, fa, fb in mlist:
            dx = []
            for r in vm:
                f = int(r['frame'])
                if f < fa or f > fb or r['ok' + who] != '1':
                    continue
                rel = int(round((f - f0) * T))
                u = um[who].get(rel)
                if u is None:
                    continue
                dx.append(u[0] - int(r['x' + who]))
            dx = np.array(dx, float)
            rms = np.sqrt(np.mean(dx ** 2)) if len(dx) else -1
            allr.append(rms)
            P('  %-48s %3d điểm  rms x %.2f px  (lệch TB %+.2f, max |%d|)' % (lab, len(dx), rms, dx.mean() if len(dx) else 0, np.abs(dx).max() if len(dx) else 0))
        vb = video_bd(v)
        # băng Unity: (rel sinh, x sinh, hướng)
        us = []
        for bid, pts in ub.items():
            r0 = min(pts)
            us.append((r0, bid))
        us.sort()
        for vid in blist:
            pts = vb.get(vid)
            if not pts:
                continue
            fs = pts[0][0]
            rel0 = (fs - f0) * T
            best = min(us, key=lambda u: abs(u[0] - rel0) + 0.5 * abs(ub[u[1]][u[0]][0] - pts[0][1])) if us else None
            if best is None:
                continue
            ex, ey = [], []
            for f, x, y in pts:
                rel = int(round((f - f0) * T))
                u = ub[best[1]].get(rel)
                if u is None:
                    continue
                ex.append(u[0] - x)
                ey.append(u[1] - y)
            ex, ey = np.array(ex, float), np.array(ey, float)
            if len(ex) == 0:
                P('  băng video B%d: không có điểm chung (Unity băng sinh rel %d)' % (vid, best[0]))
                continue
            r = np.sqrt(np.mean(ex ** 2 + ey ** 2))
            allr.append(r)
            P('  băng video B%-2d (thấy f%d)  ↔ Unity băng sinh rel %d (video ≈ %.0f): %2d điểm rms x %.2f y %.2f (gộp %.2f px)'
              % (vid, fs, best[0], rel0, len(ex), np.sqrt(np.mean(ex ** 2)), np.sqrt(np.mean(ey ** 2)), r))
    # chết + quả cầu
    if os.path.exists(os.path.join(outdir, 'm3_death.csv')):
        _, _, rows = unity(outdir, 'm3_death')
        f0 = 6680
        P('## m3_death (f0 %d)' % f0)
        fx = [(int(r['rel']), int(r['x']), int(r['y'])) for r in rows if r['kind'] == 'FxSprite' and r['frame'].startswith('bossdeath')]
        firsts = {}
        for rel, x, y in fx:
            firsts.setdefault((x, y), rel)
        for (x, y), rel in sorted(firsts.items(), key=lambda k: k[1]):
            P('  lửa chết Unity rel %d tâm (%d, %d)  — video f%s tâm (%s, 153)' % (rel, x, y, '6686' if x == 1360 else '6770', x))
        it = [(int(r['rel']), int(r['x']), int(r['y'])) for r in rows if r['kind'] == 'ItemPickup']
        cr = [r for r in csv.DictReader(open(os.path.join(C09, 'det_cr_m3.csv'))) if float(r['err']) < 0.12]
        ey = []
        ud = {rel: (x, y) for rel, x, y in it}
        for r in cr:
            rel = int(round((int(r['frame']) - f0) * T))
            if rel in ud:
                ey.append((ud[rel][0] - int(r['worldx']), ud[rel][1] - (int(r['ysec']) - 1)))
        ey = np.array(ey, float)
        if it:
            P('  quả cầu Unity hiện rel %d (%d, %d) — video f6829.5 ⇒ rel %.1f (1408, 104)' % (it[0][0], it[0][1], it[0][2], (6829.5 - f0) * T))
            fall = next((rel for rel, x, y in it if y != it[0][2]), -1)
            land = next((rel for rel, x, y in it if y == 176), -1)
            P('  bắt đầu rơi Unity rel %d — video ≈ f6847 ⇒ rel %.1f; chạm sàn Unity rel %d — video f6859 ⇒ rel %.1f' % (fall, (6847 - f0) * T, land, (6859 - f0) * T))
        if len(ey):
            r = np.sqrt(np.mean((ey ** 2).sum(1)))
            allr.append(r)
            P('  quả cầu %d điểm: rms x %.2f y %.2f (gộp %.2f px)' % (len(ey), np.sqrt(np.mean(ey[:, 0] ** 2)), np.sqrt(np.mean(ey[:, 1] ** 2)), r))
    P('## tổng: %d vết, rms trung vị %.2f px, lớn nhất %.2f px' % (len(allr), float(np.median(allr)) if allr else -1, max(allr) if allr else -1))
    with open(dst, 'w', encoding='utf-8') as fo:
        fo.write('# CVM3 09 — Tools/cvm3/boss9m_compare.py ' + outdir.replace('\\', '/') + '\n' + '\n'.join(lines) + '\n')
    print('ghi', dst)


if __name__ == '__main__':
    main()
