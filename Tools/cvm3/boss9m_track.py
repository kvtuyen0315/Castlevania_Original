"""CVM3 09 — ghép vết 2 Mummy (danh tính L/R theo liên tục) + băng gạc từ phát hiện của boss9m_fit.py, video Map_3 / Boss_Map_3.

Chạy (từ gốc repo):  python -P -E Tools/cvm3/boss9m_track.py <map3|boss3>
  vào : ClaudeNotes/cvm3/_cache/09/det_mm_<v>.csv (tím), det_mm_<v>r.csv (đỏ), det_bd_<v>.csv (băng) — v = m3 | b3
  ra  : ClaudeNotes/cvm3/_cache/09/trk_mm_<v>.csv (mỗi frame không lặp: x/khung/lật/đỏ/ok của L và R, Simon, ENEMY),
        trk_bd_<v>.csv (mỗi điểm băng: id, frame, x, y, khung, lật, err).
Danh tính: L = con đứng TRÁI lúc bắt đầu (Map_3 x 1312, video boss 1311), R = con phải (1503). Mỗi frame gán ≤ 2 phát hiện (tím hoặc đỏ)
cho L/R theo tổng |dx| nhỏ nhất so với vị trí cuối đã biết (2 Mummy đi ≤ 1 px/tick ⇒ ≤ 3 px/frame video, nhảy > MAXJ px thì bỏ).
Khi 2 con chồng nhau (|dx| < 12 — chỉ 1 phát hiện), phát hiện gán cho con gần hơn, con kia giữ vị trí cũ với ok = 0 (kiểm mắt).
Trình tự đo đầy đủ routine 09 (dựng lại từ đầu):
  1. boss9m_fit.py mm_m3  Boss/Mummy/mummy_purple_0..2 5560 6800 0 136 256 216 0.16 auto map3
     boss9m_fit.py mm_m3r Boss/Mummy/mummy_red_0..2    5960 6800 0 136 256 216 0.16 Tools/cvm3/boss9m_pal_red_map3.json map3
     boss9m_fit.py bd_m3  Boss/Mummy/bandage_0,bandage_1 5960 6800 0 48 256 224 0.16 auto map3
     (boss3: mm_b3 34 1100 auto · mm_b3r 374 1100 Tools/cvm3/boss9m_pal_red_boss3.json · bd_b3 374 1100 Tools/cvm3/boss9m_pal_bd_boss3.json)
  2. boss9m_track.py map3 / boss3            (vết)
  3. boss9m_model.py                          (bảng pha + hồi quy ⇒ ClaudeNotes/cvm3/ref/data/boss9m/*.txt)
"""
import csv, os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C09 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '09')
DATA = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
TPF = 2.0033            # tick / frame video (NES 60.0988 Hz, video 30 fps)
MAXJ = 8                # px nhảy tối đa giữa 2 frame có phát hiện (cùng con)
ERR = {'m3': (0.09, 0.10, 0.12), 'b3': (0.10, 0.115, 0.14)}   # tím, đỏ, băng
START = {'m3': (1312, 1503), 'b3': (1311, 1503)}


def load(path, emax):
    out = {}
    if not os.path.exists(path):
        return out
    for r in csv.DictReader(open(path)):
        if float(r['err']) > emax:
            continue
        out.setdefault(int(r['frame']), []).append(r)
    return out


def side_tables(v):
    vid = 'map3' if v == 'm3' else 'boss3'
    simon, hud = {}, {}
    for r in csv.DictReader(open(os.path.join(DATA, '%s_simon.csv' % vid))):
        if r.get('found') == '1':
            x = r.get('world_x') or ''
            if not x:
                try:
                    x = str(1280 + int(float(r['px'])))   # boss3: camera khoá cx 1280 (sau f374) — trước đó không dùng
                except ValueError:
                    x = ''
            simon[int(r['frame'])] = (x, r.get('Y', ''), r.get('sprite', ''), r.get('flip', ''))
    for r in csv.DictReader(open(os.path.join(DATA, '%s_hud.csv' % vid))):
        hud[int(r['frame'])] = (r['ebar'], r['pbar'], r['score'], r['hearts'])
    return simon, hud


def track_mummies(v):
    ep, er, eb = ERR[v]
    pur = load(os.path.join(C09, 'det_mm_%s.csv' % v), ep)
    red = load(os.path.join(C09, 'det_mm_%sr.csv' % v), er)
    frames = sorted(set(pur) | set(red))
    last = {'L': START[v][0], 'R': START[v][1]}
    seenR = False
    rows = []
    for f in frames:
        cands = []
        for r in pur.get(f, []):
            cands.append((int(r['worldx']), int(r['sprite'][-1]), int(r['flip']), 0, float(r['err'])))
        for r in red.get(f, []):
            cands.append((int(r['worldx']), int(r['sprite'][-1]), int(r['flip']), 1, float(r['err'])))
        # gộp trùng (tím + đỏ cùng chỗ): giữ err nhỏ
        cands.sort(key=lambda c: c[4])
        uniq = []
        for c in cands:
            if all(abs(c[0] - u[0]) >= 6 for u in uniq):
                uniq.append(c)
        cands = uniq[:2]
        asg = {'L': None, 'R': None}
        if len(cands) == 2:
            a, b = cands
            d1 = abs(a[0] - last['L']) + abs(b[0] - last['R'])
            d2 = abs(b[0] - last['L']) + abs(a[0] - last['R'])
            asg['L'], asg['R'] = (a, b) if d1 <= d2 else (b, a)
        elif len(cands) == 1:
            c = cands[0]
            k = 'L' if abs(c[0] - last['L']) <= abs(c[0] - last['R']) else 'R'
            asg[k] = c
        row = [f, '%.1f' % (f * TPF)]
        for k in ('L', 'R'):
            c = asg[k]
            if c is not None and abs(c[0] - last[k]) > MAXJ * 3:
                c = None
            if c is None:
                row += [last[k], '', '', '', 0, '']
            else:
                last[k] = c[0]
                row += [c[0], c[1], c[2], c[3], 1, '%.4f' % c[4]]
                if k == 'R':
                    seenR = True
        rows.append(row)
    return rows


def track_bandages(v):
    eb = ERR[v][2]
    det = load(os.path.join(C09, 'det_bd_%s.csv' % v), eb)
    tracks = []   # [id, [(f, x, y, s, flip, err)], dir]
    for f in sorted(det):
        for r in sorted(det[f], key=lambda r: float(r['err'])):
            x, y, s, fl, e = int(r['worldx']), int(r['ysec']), int(r['sprite'][-1]), int(r['flip']), float(r['err'])
            best, bd = None, 99
            for t in tracks:
                lf, lx, ly = t[1][-1][0], t[1][-1][1], t[1][-1][2]
                gap = f - lf
                if gap <= 0 or gap > 6:
                    continue
                d = 1 if fl else -1
                ex = lx + d * 3 * gap
                dd = abs(x - ex) + abs(y - ly) * 0.5
                if abs(x - ex) <= 4 + gap and abs(y - ly) <= 6 * gap and dd < bd and t[2] == d:
                    best, bd = t, dd
            if best is None:
                tracks.append([len(tracks), [(f, x, y, s, fl, e)], 1 if fl else -1])
            else:
                best[1].append((f, x, y, s, fl, e))
    return [t for t in tracks if len(t[1]) >= 4]


def main():
    vid = sys.argv[1]
    v = 'm3' if vid == 'map3' else 'b3'
    simon, hud = side_tables(v)
    rows = track_mummies(v)
    p = os.path.join(C09, 'trk_mm_%s.csv' % v)
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['frame', 'tick', 'xL', 'iL', 'flipL', 'redL', 'okL', 'errL', 'xR', 'iR', 'flipR', 'redR', 'okR', 'errR',
                    'simon_x', 'simon_Y', 'simon_spr', 'simon_flip', 'ebar', 'pbar', 'score', 'hearts'])
        for r in rows:
            s = simon.get(r[0], ('', '', '', ''))
            h = hud.get(r[0], ('', '', '', ''))
            w.writerow(r + list(s) + list(h))
    print('ghi', p, len(rows))
    tr = track_bandages(v)
    p = os.path.join(C09, 'trk_bd_%s.csv' % v)
    with open(p, 'w', newline='') as fo:
        w = csv.writer(fo)
        w.writerow(['id', 'frame', 'x', 'y', 'sprite', 'flip', 'err'])
        for k, t in enumerate(tr):
            for pt in t[1]:
                w.writerow([k] + list(pt))
    print('ghi', p, len(tr), 'bang')
    for k, t in enumerate(tr):
        a, b = t[1][0], t[1][-1]
        print('bang %d dir %+d f%d..f%d x %d->%d y %d->%d n %d' % (k, t[2], a[0], b[0], a[1], b[1], a[2], b[2], len(t[1])))


if __name__ == '__main__':
    main()
