# -*- coding: utf-8 -*-
"""accept12.py - CVM2 12: nghiem thu Map 2 - phan tich vet THU DONG cua luong that (Map2AcceptProbe.StartTelemetry + Map1AcceptProbe.StartFlow)
so voi so do video Map_2 / Boss_Map_2 (CVM2 02-11).

Chay tu goc repo:  python -P -E Tools/cvm2/accept12.py <lenh> <telemetry.tsv> [thu muc ra]
  mech     (mat 3) be di dong + bay ep trong luong that vs video (cung truc thoi gian = tick tu khung dau section), Simon tren be
  enemy    (mat 4) tham so enemy Block 2 do lai tren map that (MH, Spear Knight, Bone Tower, Ghost, doi treo, ran) vs so do video (CVM2 08/09)
  boss     (mat 5) Queen Medusa trong luong that: khoa -> thuc, doan bay/dung, song, ran, 16 don, chet -> qua cau, nhac boss
  hud      (mat 6) TIME 400 lien 3 stage + nhip 64, diem tung su kien = Config, tally = diem + TIME*10 + tim*100
  music    (mat 7) moc nhac luong that vs video (bang CVM2_MEMORY 2f)
  trans    (mat 8) thoi luong chuyen canh luong that vs video (CVM2_MEMORY 2m) +-2 tick
  objects  (mat 2) vat tinh hien trong luong that (A) vs Map2Objects.json
  all      chay het, ghi <ra>/accept12_<lenh>.txt
Khong in tieng Viet co dau ra console khi chay tren cp1252 - ghi file UTF-8.
"""
import csv, json, math, os, re, sys
from collections import defaultdict, OrderedDict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
REF = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data')
TPF = 2.0033
try:
    sys.stdout.reconfigure(encoding='utf-8')
except Exception:
    pass

OUT = []
def P(*a):
    s = ' '.join(str(x) for x in a)
    OUT.append(s)
    try:
        print(s)
    except Exception:
        print(s.encode('ascii', 'replace').decode())


# ----------------------------------------------------------------------------------------------------------- doc vet
class Tele:
    def __init__(self, path):
        self.W = []          # dict moi tick the gioi
        self.E = defaultdict(list)   # key -> list of dict
        self.Etype = {}
        self.A = []
        self.S = []
        self.ev = []         # (kind, frame, wt, sec, cx, simon, score, msg)
        for line in open(path, encoding='utf-8'):
            line = line.rstrip('\n')
            if not line or line[0] == '#':
                continue
            c = line.split('\t')
            k = c[0]
            if k == 'W':
                d = dict(frame=int(c[1]), wt=int(c[2]), sec=c[3], cx=int(c[4]), locked=int(c[5]))
                if c[6] != '-':
                    d.update(sx=int(c[6]), sy=int(c[7]), sfx=int(c[8]), sfy=int(c[9]), state=c[10], facing=int(c[11]), inv=int(c[12]))
                else:
                    d.update(sx=None, sy=None, state='-', facing=0, inv=0)
                d.update(score=int(c[13]), time=int(c[14]), hearts=int(c[15]), stage=int(c[16]), lives=int(c[17]), hp=int(c[18]), ehp=int(c[19]),
                         sub=c[20], mult=int(c[21]), screen=c[22], trans=int(c[23]))
                self.W.append(d)
            elif k == 'E':
                d = dict(frame=int(c[1]), wt=int(c[2]), key=c[3], type=c[4], x=int(c[5]), y=int(c[6]), fx=int(c[7]), fy=int(c[8]),
                         facing=int(c[9]), state=c[10], extra=c[11] if len(c) > 11 else '')
                self.E[d['key']].append(d)
                self.Etype[d['key']] = d['type']
            elif k == 'A':
                self.A.append(dict(frame=int(c[1]), wt=int(c[2]), key=c[3], type=c[4], x=int(c[5]), y=int(c[6]), sec=c[7], cx=int(c[8]), pid=c[9]))
            elif k == 'S':
                self.S.append(dict(frame=int(c[1]), score=int(c[2]), time=int(c[3]), hearts=int(c[4]), stage=int(c[5]), lives=int(c[6]),
                                   hp=int(c[7]), ehp=int(c[8]), sub=c[9], mult=int(c[10]), screen=c[11], trans=int(c[12])))
            else:
                self.ev.append(dict(kind=k, frame=int(c[1]), wt=int(c[2]), sec=c[3], cx=int(c[4]), simon=c[5], score=int(c[6]), msg='\t'.join(c[7:])))
        self.byframe = {d['frame']: d for d in self.W}

    def events(self, kind, contains=None):
        return [e for e in self.ev if e['kind'] == kind and (contains is None or contains in e['msg'])]

    def first_w(self, sec):
        for d in self.W:
            if d['sec'] == sec:
                return d
        return None

    def section_spans(self):
        spans = []
        cur = None
        for d in self.W:
            if cur is None or d['sec'] != cur[0]:
                cur = [d['sec'], d['frame'], d['frame'], d['wt'], d['wt']]
                spans.append(cur)
            cur[2] = d['frame']
            cur[4] = d['wt']
        return spans

    def tracks(self, typ):
        return {k: v for k, v in self.E.items() if self.Etype[k] == typ}


def rms(a):
    return math.sqrt(sum(x * x for x in a) / len(a)) if a else float('nan')


def load_csv(name):
    return list(csv.DictReader(open(os.path.join(REF, name), encoding='utf-8')))


def fit_shift(u, v, tshift=(0,)):
    """u: dict tau->(x,y) Unity; v: list (tau, x, y) video. Tim tinh tien (dx,dy) + lech thoi gian (trong tshift) cho rms nho nhat."""
    best = None
    for ts in tshift:
        pairs = []
        for tv, xv, yv in v:
            t = int(round(tv + ts))
            if t in u:
                pairs.append((u[t][0], u[t][1], xv, yv))
        if len(pairs) < 3:
            continue
        dx = sum(p[2] - p[0] for p in pairs) / len(pairs)
        dy = sum(p[3] - p[1] for p in pairs) / len(pairs)
        r = rms([math.hypot(p[2] - p[0] - dx, p[3] - p[1] - dy) for p in pairs])
        if best is None or r < best[0]:
            best = (r, ts, dx, dy, len(pairs))
    return best


def lin_slope(ts, xs):
    n = len(ts)
    if n < 2:
        return float('nan')
    mt = sum(ts) / n
    mx = sum(xs) / n
    den = sum((t - mt) ** 2 for t in ts)
    return sum((t - mt) * (x - mx) for t, x in zip(ts, xs)) / den if den else float('nan')


def quad_g(ts, ys):
    """hoi quy y = a + b t + (g/2) t^2 => g."""
    n = len(ts)
    if n < 4:
        return None
    S = [sum(t ** k for t in ts) for k in range(5)]
    Yv = [sum((t ** k) * y for t, y in zip(ts, ys)) for k in range(3)]
    M = [[S[0], S[1], S[2]], [S[1], S[2], S[3]], [S[2], S[3], S[4]]]

    def det(m):
        return (m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1]) - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
                + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]))
    D = det(M)
    if not D:
        return None
    M3 = [row[:] for row in M]
    for i in range(3):
        M3[i][2] = Yv[i]
    return 2 * det(M3) / D


def vid_tracks(name):
    p = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '08', name)
    out = defaultdict(list)
    if not os.path.exists(p):
        return out
    for r in csv.DictReader(open(p, encoding='utf-8')):
        out[int(r['track'])].append((int(r['frame']), int(r['worldx']), int(r['ysec']), r['section']))
    return out


# ----------------------------------------------------------------------------------------------------------- mat 3: co che
def cmd_mech(T):
    P('== MAT 3 - be di dong + bay ep trong LUONG THAT (vet thu dong) vs video Map_2')
    first = OrderedDict()
    for d in T.W:
        first.setdefault(d['sec'], d)
    P('khung dau section (Unity frame/wt): ' + ', '.join('%s f%d wt%d' % (s, d['frame'], d['wt']) for s, d in first.items()))
    VID = [('platform_lane_TEB', ['platform_teb.csv'], 'TowerEntranceB', 866),
           ('platform_lane_HallB_1', ['platform_hallb1.csv', 'platform_hallb1r.csv'], 'TowerHallB', 2801),
           ('platform_lane_HallB_2', ['platform_hallb2.csv', 'platform_hallb2r.csv'], 'TowerHallB', 2801)]
    plats = defaultdict(dict)
    for k, tr in T.tracks('MovingPlatform').items():
        for d in tr:
            m = re.search(r'left=(-?\d+) lane=(\S+)', d['extra'])
            plats[m.group(2)][d['wt']] = int(m.group(1))
    P('be | diem so | rms (px) | |lech| max | tick Unity co be | khoang t so (tick tu khung dau section) | ghi chu')
    res = {}
    for lane, files, sec, f0 in VID:
        u = plats.get(lane, {})
        if not u or sec not in first:
            P('%s | KHONG CO trong vet' % lane)
            continue
        wt0 = first[sec]['wt']
        errs, ts = [], []
        for fn in files:
            for r in load_csv('mech6/' + fn):
                if int(r['dup']) or float(r['margin']) < 0.6 or int(r['vis']) < 32:
                    continue
                t = (int(r['frame']) - f0) * TPF
                wt = wt0 + int(round(t))
                if wt in u:
                    errs.append(int(r['x']) - u[wt])
                    ts.append(t)
        if errs:
            res[lane] = (rms(errs), len(errs))
            P('%s | %d | %.2f | %d | %d | %.0f..%.0f | bat dau tu khung dau section (startTick), chay lien tuc' % (
                lane, len(errs), rms(errs), max(abs(e) for e in errs), len(u), min(ts), max(ts)))
        else:
            P('%s | 0 diem chung (Unity roi section truoc luot video)' % lane)
    # mo hinh Config (ConfigActorParam Object.MovingPlatform): Unity luong that == cong thuc? video (moi mau, ke ca ngoai khoang Unity) vs cong thuc
    P('')
    P('-- mo hinh Config left = floor(tam + bien*cos(2pi(t + startTick)/chu ky)) - 16, t = tick tu khung dau section')
    cfgp = {}
    for r in csv.reader(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigActorParam.csv'), encoding='utf-8')):
        if r and r[0] == 'Object.MovingPlatform':
            cfgp[r[1]] = float(r[2])
    for lane, files, sec, f0 in VID:
        suf = lane.replace('platform_lane_', '').replace('_', '')
        c, a, T_, st = cfgp['centerX' + suf], cfgp['amplitude' + suf], cfgp['periodTicks' + suf], cfgp['startTick' + suf]
        def model(t):
            return math.floor(c + a * math.cos(2 * math.pi * (t + st) / T_)) - 16
        u = plats.get(lane, {})
        wt0 = first[sec]['wt'] if sec in first else 0
        du = [u[w] - model(w - wt0) for w in u]
        ev = []
        for fn in files:
            for r in load_csv('mech6/' + fn):
                if int(r['dup']) or float(r['margin']) < 0.6 or int(r['vis']) < 32:
                    continue
                ev.append(int(r['x']) - model((int(r['frame']) - f0) * TPF))
        res['model_' + lane] = (max(abs(x) for x in du) if du else None, rms(ev), len(ev))
        P('%s (tam %g bien %g chu ky %g startTick %g): Unity luong that - cong thuc: %d tick, |lech| max %s px | video - cong thuc: %d diem rms %.2f px, max %d' % (
            lane, c, a, T_, st, len(du), max(abs(x) for x in du) if du else '-', len(ev), rms(ev), max(abs(x) for x in ev) if ev else 0))
    P('')
    P('-- Simon dung tren be (Unity luong that): x Simon - mep trai be qua cac tick dung tren mat be (video: hang so +-0.56 / +-0.25 / +-0.00)')
    for lane, u in plats.items():
        runs, cur = [], []
        for d in T.W:
            if d['sx'] is None or d['wt'] not in u:
                if cur:
                    runs.append(cur)
                    cur = []
                continue
            left = u[d['wt']]
            on = d['sy'] == 80 and left - 4 <= d['sx'] <= left + 36 and d['state'] in ('Idle', 'Attack', 'Crouch', 'Walk')
            if on and d['state'] != 'Walk':
                cur.append((d['wt'], d['sx'] - left, left))
            elif cur:
                runs.append(cur)
                cur = []
        if cur:
            runs.append(cur)
        for rn in runs:
            if len(rn) < 10:
                continue
            offs = [o for _, o, _ in rn]
            moved = max(l for _, _, l in rn) - min(l for _, _, l in rn)
            m = sum(offs) / len(offs)
            P('%s wt%d-%d (%d tick): be troi %d px, x Simon - mep be %d..%d (rms quanh TB %.2f px)' % (lane, rn[0][0], rn[-1][0], len(rn), moved, min(offs), max(offs), rms([o - m for o in offs])))
            res.setdefault('ride', []).append((lane, len(rn), moved, max(offs) - min(offs)))
    P('')
    P('-- bay ep Hall C: khung video (crusher_low.csv f4479-4828, truoc dong ho) vs Unity luong that (cung tick tu khung dau Hall C)')
    cr = defaultdict(dict)
    for k, tr in T.tracks('Crusher').items():
        for d in tr:
            m = re.search(r'frame=(\d+) u=(\d+) id=crusher_(\d)', d['extra'])
            cr[int(m.group(3))][d['wt']] = int(m.group(1))
    if 'TowerHallC' in first:
        wt0 = first['TowerHallC']['wt']
        rows = [r for r in load_csv('mech6/crusher_low.csv') if 4479 <= int(r['frame']) < 4829 and not int(r['dup'])]
        for b in (1, 2, 3):
            n = ok = near = 0
            for r in rows:
                k = int(r['k%d' % b])
                if k < 0:
                    continue
                wt = wt0 + int(round((int(r['frame']) - 4479) * TPF))
                if wt not in cr[b]:
                    continue
                n += 1
                if cr[b][wt] == k:
                    ok += 1
                if any(cr[b].get(wt + dd) == k for dd in (-2, -1, 0, 1, 2)):
                    near += 1
            res['crusher%d' % b] = (ok, near, n)
            P('bay %d: %d frame video co khung | trung khung CUNG tick %d (%.0f %%) | trung trong +-2 tick %d (%.0f %%)' % (b, n, ok, 100.0 * ok / max(1, n), near, 100.0 * near / max(1, n)))
        # chu ky Unity
        for b in (1, 2, 3):
            seq = [cr[b][w] for w in sorted(cr[b])]
            maxk = max(seq) if seq else -1
            P('bay %d Unity: khung toi da %d, %d tick quan sat' % (b, maxk, len(seq)))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 4: enemy
def cmd_enemy(T):
    P('== MAT 4 - enemy Block 2 tren MAP THAT (luong that, vet thu dong) vs so do video (CVM2 08 2j / 09 2k)')
    wmap = {d['wt']: d for d in T.W}
    res = {}
    P('-- Medusa Head')
    rows, spawns = [], []
    for k, tr in sorted(T.tracks('MedusaHead').items(), key=lambda kv: kv[1][0]['wt']):
        t0 = tr[0]['wt']
        ts = [d['wt'] - t0 for d in tr]
        xs = [d['fx'] / 256.0 for d in tr]
        ys = [d['y'] for d in tr]
        vx = lin_slope(ts, xs)
        y0 = tr[0]['y']
        r = rms([a - (y0 + 33.75 * (1 - math.cos(2 * math.pi * t / 100.0))) for a, t in zip(ys, ts)])
        s = wmap.get(t0)
        spawns.append((s['sec'] if s else '?', t0))
        rows.append((s['sec'] if s else '?', t0, len(tr), vx, max(ys) - min(ys), r, (y0 - s['sy']) if s else None, tr[0]['x'] - (s['cx'] if s else 0), tr[0]['facing'], s['facing'] if s else 0))
    P('section | tick sinh | so tick | vx (px/tick) | dinh-day | rms vs y0+33.75(1-cos 2pi t/100) | y sinh - chan Simon | x man luc sinh | huong MH | huong Simon')
    for r in rows:
        P('%s | %d | %d | %.3f | %d | %.2f | %s | %d | %+d | %+d' % r)
    gaps = [t2 - t1 for (s1, t1), (s2, t2) in zip(spawns, spawns[1:]) if s1 == s2]
    P('khoang cach 2 lan sinh lien tiep cung section: %s (video deu 128: 126.8-129.0)' % gaps)
    res['mh'] = rows
    res['mh_gaps'] = gaps
    P('so hinh dang quy dao MH Unity (tu luc sinh = dinh song) vs vet video _cache/08/trk_mh_*.csv (canh top_frame mh_spawns.csv; tinh tien + lech t +-3):')
    sp = load_csv('enemy8/mh_spawns.csv')
    tagfile = {'mh_a': 'trk_mh_a.csv', 'mh_b1': 'trk_mh_b1.csv', 'mh_b2': 'trk_mh_b2.csv', 'mh_m': 'trk_mh_m.csv'}
    utr = defaultdict(list)
    for k, tr in T.tracks('MedusaHead').items():
        s = wmap.get(tr[0]['wt'])
        utr[(s['sec'] if s else '?', tr[0]['facing'])].append({d['wt'] - tr[0]['wt']: (d['fx'] / 256.0, d['y']) for d in tr})
    allr = []
    cache = {}
    for r in sp:
        fn = tagfile.get(r['tag'])
        if fn is None:
            continue
        if fn not in cache:
            cache[fn] = vid_tracks(fn)
        pts = cache[fn].get(int(r['track']))
        if not pts:
            continue
        top = float(r['top_frame'])
        v = [((f - top) * TPF, x, y) for f, x, y, _ in pts]
        # loai diem ghep nham cua bo do video (nhay x > 12 px so voi duong thang toc do |vx| 1.25 qua diem dau) - CVM2 08 cung loai ngoai lai
        if v:
            x0, t0_ = v[0][1], v[0][0]
            keep = [p for p in v if abs((p[1] - x0) - int(r['dir']) * 1.25 * (p[0] - t0_)) <= 12]
            dropped = len(v) - len(keep)
            v = keep
        best = None
        for u in utr.get((r['section'], int(r['dir'])), []):
            b = fit_shift(u, v, tshift=range(-3, 4))
            if b and (best is None or b[0] < best[0]):
                best = b
        if best:
            allr.append(best[0])
            P('  video %s #%s %s dir %+d (%d diem%s) vs Unity cung section/huong: rms %.2f px (lech t %+d tick, %d diem chung)' % (r['tag'], r['track'], r['section'], int(r['dir']), len(v), (', bo %d diem ghep nham' % dropped) if dropped else '', best[0], best[1], best[4]))
        else:
            P('  video %s #%s %s dir %+d: Unity khong co MH cung section/huong' % (r['tag'], r['track'], r['section'], int(r['dir'])))
    if allr:
        s_ = sorted(allr)
        P('  => %d vet: rms trung vi %.2f, lon nhat %.2f px' % (len(allr), s_[len(s_) // 2], max(allr)))
    res['mh_shape'] = allr
    P('-- Spear Knight')
    sk = []
    for k, tr in sorted(T.tracks('SpearKnight').items(), key=lambda kv: kv[1][0]['wt']):
        s = wmap.get(tr[0]['wt'])
        steps = []
        for a, b in zip(tr, tr[1:]):
            if b['wt'] == a['wt'] + 1 and b['facing'] == a['facing'] and b['y'] == a['y'] and b['state'] == a['state'] and 'Walk' in b['state']:
                steps.append(abs(b['fx'] - a['fx']) / 256.0)
        turns = [b['x'] for a, b in zip(tr, tr[1:]) if b['facing'] != a['facing']]
        v = sum(steps) / len(steps) if steps else float('nan')
        sk.append(v)
        P('%s sinh wt%d (%d,%d) f%+d | %d tick | toc do TB khi Walk %.3f px/tick | %d lan quay, x: %s' % (s['sec'] if s else '?', tr[0]['wt'], tr[0]['x'], tr[0]['y'], tr[0]['facing'], len(tr), v, len(turns), turns[:14]))
    res['sk'] = sk
    P('-- Bone Tower + cau lua (video: 2 cau cung mieng tren cach 40, 2.0 px/tick, sinh o x thap, cao chan - 15; luot cach 185-314)')
    towers = T.tracks('BoneTower')
    for k, tr in towers.items():
        P('thap (%d,%d) %d tick, huong doi %d lan' % (tr[0]['x'], tr[0]['y'], len(tr), sum(1 for a, b in zip(tr, tr[1:]) if a['facing'] != b['facing'])))
    fbs = sorted(T.tracks('BoneTowerFireball').items(), key=lambda kv: kv[1][0]['wt'])
    speeds, rel = [], []
    for k, tr in fbs:
        speeds.append(abs(lin_slope([d['wt'] for d in tr], [d['fx'] / 256.0 for d in tr])))
        tw = min(towers.values(), key=lambda t: abs(t[0]['x'] - tr[0]['x'])) if towers else None
        if tw:
            rel.append((tr[0]['wt'], tw[0]['x'], tr[0]['x'] - tw[0]['x'], tr[0]['y'] - tw[0]['y']))
    P('%d cau lua: toc do %s' % (len(fbs), ' '.join('%.3f' % s for s in speeds)))
    P('sinh (tick, x thap, x cau - x thap, y cau - y thap): %s' % rel)
    for k, tw in sorted(towers.items(), key=lambda kv: kv[1][0]['wt']):
        a, b = tw[0]['wt'], tw[-1]['wt']
        ts = [r[0] for r in rel if r[1] == tw[0]['x'] and a <= r[0] <= b + 1]
        firsts = [t for j, t in enumerate(ts) if j % 2 == 0]
        P('thap (%d,%d) song wt%d-%d: cau o tick (tu luc sinh) %s | cap cach %s | luot cach %s' % (tw[0]['x'], tw[0]['y'], a, b, [t - a for t in ts],
          [ts[j + 1] - ts[j] for j in range(0, len(ts) - 1, 2)], [y - x for x, y in zip(firsts, firsts[1:])]))
    res['fb'] = (speeds, rel)
    P('-- Ghost (video: hien 96 px SAU LUNG Simon, tam = mat san Simon, ngang 0.74-0.75, doc 0.10-0.12)')
    for k, tr in sorted(T.tracks('Ghost').items(), key=lambda kv: kv[1][0]['wt']):
        s = wmap.get(tr[0]['wt'])
        tr40 = tr[:40]
        ts = [d['wt'] for d in tr40]
        vys = [abs(b['fy'] - a['fy']) / 256.0 for a, b in zip(tr, tr[1:])]
        P('sinh wt%d (%d,%d) | Simon (%s,%s) huong %+d => x ghost - x Simon %+d, y ghost - chan Simon %+d | vx 40 tick dau %.3f | |vy| max %.3f | song %d tick' % (
            tr[0]['wt'], tr[0]['x'], tr[0]['y'], s['sx'], s['sy'], s['facing'], tr[0]['x'] - s['sx'], tr[0]['y'] - s['sy'],
            abs(lin_slope(ts, [d['fx'] / 256.0 for d in tr40])), max(vys) if vys else 0, len(tr)))
    P('-- Doi treo (video: treo bat_purple_0; roi cho khi |dx| < 96 va dy < 80; sa 1.5 px/tick, ha 31 px / 42 tick roi bay ngang)')
    for k, tr in sorted(T.tracks('Bat').items(), key=lambda kv: kv[1][0]['wt']):
        states = []
        for d in tr:
            st = d['state'].split()[0] if d['state'] else '-'
            if not states or states[-1][1] != st:
                states.append((d['wt'], st))
        line = 'doi (%d,%d) %d tick, state: %s' % (tr[0]['x'], tr[0]['y'], len(tr), states[:5])
        if len(states) > 1:
            t1 = states[1][0]
            i = next(j for j, d in enumerate(tr) if d['wt'] == t1)
            s0 = wmap.get(t1 - 1)
            s = wmap.get(t1)
            if s and s0:
                line += ' | roi cho wt%d: Simon tick truoc |dx| %d dy %d, cung tick |dx| %d dy %d' % (t1, abs(s0['sx'] - tr[i - 1]['x']), s0['sy'] - tr[i - 1]['y'], abs(s['sx'] - tr[i - 1]['x']), s['sy'] - tr[i - 1]['y'])
            sw = tr[i - 1:i + 42]
            if len(sw) > 2:
                line += ' | %d tick dau: vx %.3f, ha %d px' % (len(sw) - 1, abs(sw[-1]['fx'] - sw[0]['fx']) / 256.0 / (len(sw) - 1), sw[-1]['y'] - sw[0]['y'])
        P(line)
    P('-- Ran (video: roi g 0.10-0.135, truon 1.376-1.396 px/tick ve phia Simon)')
    for k, tr in sorted(T.tracks('Snake').items(), key=lambda kv: kv[1][0]['wt']):
        fl = [d for d in tr if d['y'] == 144]
        fall = [d for d in tr if d['y'] < 144]
        g = quad_g([d['wt'] - tr[0]['wt'] for d in fall], [d['fy'] / 256.0 for d in fall]) if len(fall) >= 6 else None
        sp_ = abs(lin_slope([d['wt'] for d in fl], [d['fx'] / 256.0 for d in fl])) if len(fl) > 3 else float('nan')
        P('ran sinh wt%d (%d,%d): roi %d tick g %s | truon tren san %d tick %.3f px/tick' % (tr[0]['wt'], tr[0]['x'], tr[0]['y'], len(fall), ('%.3f' % g) if g else '-', len(fl), sp_))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 5: Queen Medusa
def best_det(name, f0, f1, sprites, errmax):
    p = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '09', name)
    best = {}
    for r in csv.DictReader(open(p, encoding='utf-8')):
        f = int(r['frame'])
        if f0 <= f <= f1 and r['sprite'] in sprites and int(r['dup']) == 0 and float(r['err']) < errmax:
            if f not in best or float(r['err']) < best[f][2]:
                best[f] = (int(r['wx']), int(r['ysec']), float(r['err']))
    return best


def cmd_boss(T):
    P('== MAT 5 - Queen Medusa trong LUONG THAT (vet thu dong) vs video Map_2 / Boss_Map_2 (CVM2 09 2k)')
    res = {}
    med = list(T.tracks('QueenMedusa').values())
    if not med:
        P('KHONG co Queen Medusa trong vet')
        return res
    tr = med[0]
    lock = next((d for d in T.W if d['sec'] == 'Medusa' and d['locked']), None)
    wake = next((d for d in tr if 'dormant=0' in d['extra']), None)
    dead = next((d for d in tr if 'defeated=1' in d['extra']), None)
    P('khoa camera: wt%d cx %d Simon x %s (video f6735: cx 0 khi Simon x 128)' % (lock['wt'], lock['cx'], lock['sx']))
    P('thuc: wt%d = khoa + %d tick (video 180: f6735 -> f6825) | vi tri buoc dau (%d,%d) (video tam dau f6826 (112,113))' % (wake['wt'], wake['wt'] - lock['wt'], wake['x'], wake['y']))
    res['lock_wake'] = wake['wt'] - lock['wt']
    pm = next((e for e in T.events('M', 'play PoisonMind')), None)
    if pm:
        P('nhac Poison Mind: wt%d = thuc + %d tick (video thuc + 46-57; Unity Config 57)' % (pm['wt'], pm['wt'] - wake['wt']))
        res['pm'] = pm['wt'] - wake['wt']
    # doan dau sau khi thuc vs video Map_2 f6826-6836 (Simon dung x 61 ca hai; truoc dong ho f6837)
    stun0 = next((d['wt'] for d in tr if d['wt'] > wake['wt'] and d['state'].startswith('Stun')), wake['wt'] + 999)
    u = {d['wt'] - wake['wt']: (d['x'], d['y']) for d in tr if wake['wt'] <= d['wt'] < stun0}
    P('Unity bay tu do tu luc thuc toi lan khung dau (roi Simon) wt%d = %d tick' % (stun0, stun0 - wake['wt']))
    vd = best_det('det_md_m2.csv', 6826, 6836, ('medusa_head', 'medusa_stone'), 0.08)
    v = [((f - 6825) * TPF, x, y) for f, (x, y, e) in sorted(vd.items())]
    raw = []
    for ts in range(-2, 3):
        pr = [(u[int(round(t + ts))], (x, y)) for t, x, y in v if int(round(t + ts)) in u]
        if pr:
            raw.append((rms([math.hypot(a[0] - b[0], a[1] - b[1]) for a, b in pr]), ts, len(pr)))
    raw.sort()
    if raw:
        ts = raw[0][1]
        P('  cap (tau, Unity, video): %s' % [(round(t, 1), u.get(int(round(t + ts))), (x, y)) for t, x, y in v])
        P('quy dao sau khi thuc vs video Map_2 f6826-6836 (%d frame): rms THO (khong tinh tien) %.2f px (lech t %+d tick, %d diem)' % (len(v), raw[0][0], raw[0][1], raw[0][2]))
        res['wake_rms'] = raw[0][0]
    # ran 1 (luc thuc) vs video Map_2 f6830-6845
    sn = sorted(T.tracks('Snake').values(), key=lambda t: t[0]['wt'])
    if sn:
        s1 = sn[0]
        us = {d['wt'] - wake['wt']: (d['x'], d['y']) for d in s1}
        vs = best_det('det_sn_m2.csv', 6830, 6845, ('snake_0', 'snake_1'), 0.2)
        vv = [((f - 6825) * TPF, x, y) for f, (x, y, e) in sorted(vs.items())]
        rr = []
        for ts in range(-2, 3):
            pr = [(us[int(round(t + ts))], (x, y)) for t, x, y in vv if int(round(t + ts)) in us]
            if pr:
                rr.append((rms([math.hypot(a[0] - b[0], a[1] - 1 - b[1]) for a, b in pr]), ts, len(pr)))
        rr.sort()
        if rr:
            P('ran 1 (sinh luc thuc wt%d (%d,%d)) vs video Map_2 f6830-6845: rms %.2f px (lech t %+d, %d diem; Unity ve cao 1 px - viewOffset -1)' % (s1[0]['wt'], s1[0]['x'], s1[0]['y'], rr[0][0], rr[0][1], rr[0][2]))
            res['snake_rms'] = rr[0][0]
        P('ran sinh luc (tick tu luc thuc): %s (Unity: luc thuc + moi lan roi cho dung)' % [t[0]['wt'] - wake['wt'] for t in sn])
    # pha: Fly / Hover / Stun theo DebugState
    seg = []
    for d in tr:
        st = d['state'].split()[0] if d['state'] else '-'
        if not seg or seg[-1][0] != st:
            seg.append([st, d['wt'], d['wt']])
        seg[-1][2] = d['wt']
    P('chuoi pha (state, tu wt, toi wt): %s' % ['%s %d-%d' % (s, a, b) for s, a, b in seg][:40])
    flys = [(a, b) for s, a, b in seg if s == 'Fly' and b - a >= 6]
    sp = []
    for a, b in flys:
        pts = [d for d in tr if a <= d['wt'] <= b]
        sp.append(abs(lin_slope([d['wt'] for d in pts], [d['fx'] / 256.0 for d in pts])))
    if sp:
        P('toc do ngang cac doan Fly >= 6 tick: %s (video 0.80-0.98, Config 0.875)' % ' '.join('%.3f' % s for s in sp))
    hov = [b - a + 1 for s, a, b in seg if s == 'Hover']
    P('doan Hover (tick): %s (video dung 62-63)' % hov)
    # mau ENEMY: so lan giam
    hp = [d['ehp'] for d in T.W if d['sec'] == 'Medusa']
    drops = sum(1 for a, b in zip(hp, hp[1:]) if b < a)
    P('thanh ENEMY: %d lan giam, 16 -> %d (video 16 nhat roi cap 3, moi nhat -1 vach)' % (drops, hp[-1] if hp else -1))
    res['ehp_drops'] = drops
    cr = next((e for e in T.events('C')), None)
    if dead and cr:
        P('ha boss wt%d (+3000) -> qua cau hien wt%d (+%d tick, video +150 boss / +154 Map_2) tai %s (video NES (128,137) = section (128,105))' % (dead['wt'], cr['wt'], cr['wt'] - dead['wt'], cr['msg']))
        res['crystal'] = cr['wt'] - dead['wt']
    items = [t for t in T.tracks('ItemPickup').values() if 'MagicCrystal' in t[0]['extra']]
    if items:
        it = items[0]
        land = next((d for d in it if d['y'] == it[-1]['y']), it[-1])
        P('qua cau roi: y %d -> %d, cham san sau %d tick (video 18-20 tick)' % (it[0]['y'], it[-1]['y'], land['wt'] - it[0]['wt']))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 6: HUD + luat
def cmd_hud(T, cfg_scores=None):
    P('== MAT 6 - HUD & luat trong LUONG THAT (vet thu dong)')
    res = {}
    # TIME
    st4 = next(d for d in T.W if d['stage'] == 4 and d['sec'] == 'TowerEntranceA')
    P('vao STAGE 04 wt%d: TIME %d, tim %d, P %d, SCORE %d (video 0400, tim 05 sau ban do)' % (st4['wt'], st4['time'], st4['hearts'], st4['lives'], st4['score']))
    ch = []
    prev = None
    for d in T.W:
        if d['stage'] > 6 or d['sec'] not in ('TowerEntranceA', 'TowerEntranceB', 'TowerHallA', 'TowerHallB', 'TowerHallC', 'Medusa'):
            continue
        if prev is not None and d['time'] != prev['time']:
            ch.append((d['wt'], prev['time'], d['time'], d['stage'], d['sec']))
        prev = d
    resets = [c for c in ch if c[2] > c[1]]
    P('TIME tang (dat lai) trong Block 2: %d lan %s (video: KHONG dat lai qua STAGE 05/06)' % (len(resets), resets[:5]))
    stage_ch = []
    prev = None
    for d in T.W:
        if prev and d['stage'] != prev['stage'] and d['stage'] <= 6:
            stage_ch.append((d['wt'], prev['stage'], d['stage'], prev['time'], d['time']))
        prev = d
    P('doi STAGE (wt, cu, moi, TIME truoc, TIME sau): %s' % stage_ch)
    dec = [c for c in ch if c[2] == c[1] - 1 and c[0] < 7000]
    gaps = defaultdict(int)
    for a, b in zip(dec, dec[1:]):
        gaps[b[0] - a[0]] += 1
    P('khoang giua 2 lan TIME giam (truoc tally): %s (video 64 tick/dv; khoang dai = giu TIME o cua / den / dong ho)' % dict(sorted(gaps.items())))
    res['time_resets'] = len(resets)
    # diem tung su kien
    P('-- diem tung su kien (K/B/P) = do lech SCORE HUD')
    for e in T.ev:
        if e['kind'] in ('K', 'B', 'P', 'U'):
            P('  %s wt%d %s | SCORE sau %d' % (e['kind'], e['wt'], e['msg'], e['score']))
    # tally
    pick = next((e for e in T.events('F', 'crystal')), None)
    done = next((e for e in T.events('F', 'tally_done')), None)
    if pick and done:
        before = max((d for d in T.W if d['wt'] <= pick['wt']), key=lambda d: d['wt'])
        after_score = done['score']
        exp = before['score'] + before['time'] * 10 + before['hearts'] * 100
        P('tally: luc nhat cau SCORE %d TIME %d tim %d => ky vong %d + %d*10 + %d*100 = %d | Unity luc tally_done %d => %s' % (
            before['score'], before['time'], before['hearts'], before['score'], before['time'], before['hearts'], exp, after_score, 'KHOP' if exp == after_score else 'LECH'))
        res['tally'] = exp == after_score
    return res


# ----------------------------------------------------------------------------------------------------------- mat 7: nhac
def cmd_music(T):
    P('== MAT 7 - nhac luong that (AudioManager.MusicEvent) vs video (CVM2_MEMORY 2f)')
    res = {}
    ms = [e for e in T.ev if e['kind'] == 'M']
    for e in ms:
        P('  frame %d wt %d %s | %s' % (e['frame'], e['wt'], e['sec'] or '-', e['msg']))
    # moc man hinh tu S/W
    def first_frame(pred):
        for d in sorted(T.S + T.W, key=lambda d: d['frame']):
            if pred(d):
                return d['frame']
        return None
    st4 = first_frame(lambda d: d.get('screen') == 'Stage1' and not d.get('trans') and d.get('stage') == 4 and 'wt' in d)
    st7 = first_frame(lambda d: d.get('screen') == 'Stage7Stub' and not d.get('trans'))
    stalk = next((e for e in ms if 'play Stalker' in e['msg']), None)
    wc = next((e for e in ms if 'play WickedChild' in e['msg']), None)
    loops = [e['frame'] for e in ms if 'loop Stalker' in e['msg']]
    plays = [e for e in ms if e['msg'].startswith('play')]
    P('so lan play trong Map 2: %d (%s) - video: Stalker 1 lan (KHONG phat lai o doi tang/cua), PM, SC, WC' % (len(plays), ', '.join(e['msg'] for e in plays)))
    if stalk and st4:
        P('Stalker: frame %d, khung STAGE 04 dau tien %d => %+d tick (video Stalker 6.177 s vs STAGE 04 hien 6.23 s => -3 tick)' % (stalk['frame'], st4, stalk['frame'] - st4))
    if loops:
        P('Stalker lap o frame %s, khoang %s tick (video vong 29.82-29.84 s = 1789-1790 tick)' % (loops, [b - a for a, b in zip(loops, loops[1:])]))
    pick = next((e for e in T.events('P', 'MagicCrystal')), None)
    sc = next((e for e in ms if 'play StageClear' in e['msg']), None)
    if pick and sc:
        P('Stage Clear: frame %d, nhat cau frame %d => %+d (video cung luc +-0.05 s)' % (sc['frame'], pick['frame'], sc['frame'] - pick['frame']))
    stop = [e for e in ms if 'stop' in e['msg']]
    P('dung nhac: %s (video: ban do + tally im)' % [(e['frame'], e['msg']) for e in stop])
    if wc and st7:
        P('Wicked Child: frame %d, khung STAGE 07 dau tien %d => %+d tick (video 4:19.80 vs STAGE 07 hien 4:20.20 => ~ -24 tick; bang chung yeu NCC 0.48)' % (wc['frame'], st7, wc['frame'] - st7))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 8: chuyen canh
def black_runs(T):
    """cac doan man den (S trans=1) lien tiep: (frame dau, frame cuoi, so khung, man truoc, man sau)."""
    seq = sorted([(d['frame'], d.get('screen'), d.get('trans', 0)) for d in T.S] + [(d['frame'], 'Stage1', 0) for d in T.W])
    runs = []
    cur = None
    prev_screen = None
    for f, scr, tr in seq:
        if tr:
            if cur is None:
                cur = [f, f, prev_screen]
            cur[1] = f
        else:
            if cur is not None:
                runs.append((cur[0], cur[1], cur[1] - cur[0] + 1, cur[2], scr))
                cur = None
            prev_screen = scr
    return runs


def cmd_trans(T):
    P('== MAT 8 - chuyen canh luong that vs video Map_2 (CVM2_MEMORY 2m; tick = frame video x 2.0033)')
    res = []
    def chk(name, u, v, tol=2):
        ok = v is None or abs(u - v) <= tol
        res.append((name, u, v, ok))
        P('  %-48s Unity %6s | video %6s | %s' % (name, u, '-' if v is None else v, 'DAT' if ok else 'LECH'))
    for r in black_runs(T):
        P('  den: frame %d-%d (%d khung) %s -> %s' % r)
    br = black_runs(T)
    b1 = next((r for r in br if r[3] == 'CastleMap' and r[4] == 'Stage1'), None)
    if b1:
        chk('ban do B1 -> STAGE 04: den (khung)', b1[2], 28)
    F = T.events('F')
    fs = [(e['frame'], e['wt'], e['msg'], e['sec'], e['simon'], e['cx']) for e in F]
    P('  moc StageFlow: %s' % [(f, m, s, p) for f, w, m, s, p, c in fs])
    # doi tang
    st = [(i, x) for i, x in enumerate(fs) if x[2] == 'stair_up']
    vid_black = {'TowerEntranceB': 22, 'TowerHallB': 22, 'Medusa': 20}
    vid_arr = {'TowerEntranceB': ('386,189', 256), 'TowerHallB': ('66,189', 0), 'Medusa': ('674,189', 512)}
    for i, x in st:
        nx = fs[i + 1]
        chk('doi tang -> %s: den (tick)' % nx[3], nx[0] - x[0], vid_black.get(nx[3]))
        P('    hien %s cx %d (video %s cx %s)' % (nx[4], nx[5], vid_arr.get(nx[3], ('?', '?'))[0], vid_arr.get(nx[3], ('?', '?'))[1]))
        res.append(('arrive ' + nx[3], nx[4], vid_arr.get(nx[3], ('?',))[0], nx[4] == vid_arr.get(nx[3], ('?',))[0]))
    # cua trai
    VD = [dict(scroll1=128, open=8, closed=110, scroll2_end=160 + 128), dict(scroll1=128, open=15, closed=119, scroll2_end=166 + 128)]
    di = [i for i, x in enumerate(fs) if x[2] == 'door']
    for n, i in enumerate(di[:2]):
        seq = {fs[j][2]: fs[j][0] for j in range(i, min(i + 6, len(fs)))}
        st_ = next((fs[j] for j in range(i, len(fs)) if fs[j][2].startswith('stage')), None)
        v = VD[n]
        chk('cua trai #%d: cuon 1 (door -> door_stop)' % (n + 1), seq['door_stop'] - seq['door'], v['scroll1'])
        chk('cua trai #%d: door_stop -> canh mo' % (n + 1), seq['door_open'] - seq['door_stop'], v['open'])
        chk('cua trai #%d: door_stop -> cot tro lai' % (n + 1), seq['door_closed'] - seq['door_stop'], v['closed'])
        if st_:
            chk('cua trai #%d: door_stop -> het cuon 2 (STAGE doi)' % (n + 1), st_[0] - seq['door_stop'], v['scroll2_end'])
            P('    sau cua: %s %s cx %d (video (720,80/112) quay trai cx 512)' % (st_[3], st_[4], st_[5]))
    # boss / tally / ban do
    lock = next((d for d in T.W if d['sec'] == 'Medusa' and d['locked']), None)
    med = list(T.tracks('QueenMedusa').values())
    if lock and med:
        wake = next(d for d in med[0] if 'dormant=0' in d['extra'])
        chk('khoa camera -> Medusa thuc (tick)', wake['wt'] - lock['wt'], 180)
        dead = next(d for d in med[0] if 'defeated=1' in d['extra'])
        cr = T.events('C')[0]
        chk('ha boss -> qua cau (tick; video 150/154)', cr['wt'] - dead['wt'], 152, tol=2)
    pick = next((e for e in T.events('F', 'crystal')), None)
    if pick:
        tw = [d for d in T.W if d['wt'] > pick['wt']]
        t0 = next((d for d in tw if d['time'] < next(x for x in T.W if x['wt'] == pick['wt'])['time']), None)
        if t0:
            chk('nhat cau -> TIME bat dau dem (tick)', t0['wt'] - pick['wt'], 224.6)
            tz = next((d for d in tw if d['time'] == 0), None)
            h0 = next((d for d in tw if d['wt'] > tz['wt'] and d['hearts'] < tz['hearts']), None)
            if tz and h0:
                chk('TIME 0000 -> tim dau tien (tick)', h0['wt'] - tz['wt'], 58.1)
                hs = [d for d in tw if d['wt'] >= h0['wt']]
                hd = [b['wt'] for a, b in zip(hs, hs[1:]) if b['hearts'] < a['hearts']]
                if len(hd) > 1:
                    chk('nhip tim (tick)', hd[1] - hd[0], 8.0)
                done = next(e for e in T.events('F', 'tally_done'))
                hz = next(d for d in tw if d['hearts'] == 0)
                chk('tim cuoi -> tally xong (tick; video tim cuoi -> den 118.2)', done['wt'] - hz['wt'], 118.2)
    b2 = next((r for r in br if r[3] == 'Stage1' and r[4] == 'CastleMap' and r[0] > (pick['frame'] if pick else 0)), None)
    if b2:
        chk('tally xong -> ban do B2: den (khung)', b2[2], 21)
    b3 = next((r for r in br if r[3] == 'CastleMap' and r[4] == 'Stage7Stub'), None)
    if b3:
        chk('ban do B2 -> STAGE 07: den (khung)', b3[2], 28)
        mp = [d['frame'] for d in T.S if d['screen'] == 'CastleMap' and not d['trans'] and d['frame'] > (pick['frame'] if pick else 0)]
        if mp:
            chk('ban do B2 hien -> bat dau den (khung; video f7596 -> f7792 = 196 frame = 393 tick)', b3[0] - mp[0], 393, tol=4)
    st7 = next((d for d in T.S if d['screen'] == 'Stage7Stub' and not d['trans']), None)
    if st7:
        P('  STAGE 07: TIME %d, tim %d, ENEMY %d, STAGE %d (video TIME 0500, tim 05, ENEMY 16)' % (st7['time'], st7['hearts'], st7['ehp'], st7['stage']))
    n_ok = sum(1 for r in res if r[3])
    P('  => %d/%d moc trong +-2 tick (hoac khop)' % (n_ok, len(res)))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 2: vat the
def cmd_objects(T):
    P('== MAT 2 - vat tinh hien trong LUONG THAT (dong A) vs Map2Objects.json')
    d = json.load(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block2', 'Map2Objects.json'), encoding='utf-8'))
    want = {}
    for s in d['sections']:
        for c in s.get('candles', []):
            want[c['id']] = (s['id'], c['x'], c['y'], c['content'])
    seen = {}
    for a in T.A:
        if a['type'] == 'CandleProp':
            seen.setdefault(a['pid'], (a['sec'], a['x'], a['y']))
    ok = bad = 0
    miss = []
    for cid, (sec, x, y, cont) in sorted(want.items()):
        if cid not in seen:
            miss.append(cid)
            continue
        s2, x2, y2 = seen[cid]
        if (s2, x2, y2) == (sec, x, y):
            ok += 1
        else:
            bad += 1
            P('  LECH %s: json %s (%d,%d) vs luong that %s (%d,%d)' % (cid, sec, x, y, s2, x2, y2))
    P('nen: %d/%d trung vi tri (section, x, Y) | lech %d | khong hien trong luong that %d %s' % (ok, len(want), bad, len(miss), miss))
    blocks = [a for a in T.A if a['type'] == 'BreakableBlock']
    P('khoi pha hien: %d %s' % (len(blocks), [(b['pid'], b['x'], b['y']) for b in blocks]))
    sec_ = [a for a in T.A if a['type'] == 'SecretTrigger']
    P('bi mat (SecretTrigger) hien: %d %s' % (len(sec_), [(b['sec'], b['x'], b['y']) for b in sec_]))
    # enemy SpawnPoint: vi tri lan dau thay (E) vs json spawnPoints
    sp = {}
    for s in d['sections']:
        for p in s.get('spawnPoints', []):
            sp[p['name']] = (s['id'], p['x'], p['y'], os.path.basename(p['prefab']).replace('.prefab', ''))
    firsts = []
    for k, tr in T.E.items():
        t = T.Etype[k]
        if t in ('SpearKnight', 'Bat', 'BoneTower'):
            w = next((x for x in T.W if x['wt'] == tr[0]['wt']), None)
            firsts.append((w['sec'] if w else '?', t, tr[0]['x'], tr[0]['y'], tr[0]['wt']))
    m = 0
    for name, (sec, x, y, pref) in sorted(sp.items()):
        hit = [f for f in firsts if f[0] == sec and f[2] == x and f[3] == y]
        m += 1 if hit else 0
        P('  spawn %-26s %s (%d,%d) %s: %s' % (name, sec, x, y, pref, ('sinh trong luong that wt%d' % hit[0][4]) if hit else 'khong thay sinh dung cho (co the chua vao khung/sinh lai noi khac)'))
    P('SpawnPoint enemy: %d/%d sinh dung (x, Y) trong luong that' % (m, len(sp)))
    return dict(candles=(ok, len(want), bad, miss), spawn=(m, len(sp)))


CMDS = OrderedDict([('objects', cmd_objects), ('mech', cmd_mech), ('enemy', cmd_enemy), ('boss', cmd_boss), ('hud', cmd_hud), ('music', cmd_music), ('trans', cmd_trans)])


def cmd_terrain2(udir):
    """(mat 1) anh nen Block 2 chup tu StageScreen THAT (Map1AcceptProbe.StartTerrain - sprite an, HUD con) vs vung sheet Level 2 da va:
    NES y 48-223 (duoi dai HUD) = sheet hang 16-191; cung cach lay mau tam o 4x4 nhu accept5.py."""
    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
    sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
    import build_tileset2 as T2
    from PIL import Image
    import numpy as np
    regs = T2.patched_regions()
    total = 0
    n_img = 0
    for f in sorted(os.listdir(udir)):
        m = re.match(r'terrain_(\w+?)_cx(\d+)\.png$', f)
        if not m:
            continue
        sec, cx = m.group(1), int(m.group(2))
        im = np.asarray(Image.open(os.path.join(udir, f)).convert('RGB'))
        ys = 60 + 4 * np.arange(240) + 2
        xs = 448 + 4 * np.arange(256) + 2
        got = im[ys][:, xs].astype(np.int16)
        exp = regs[sec][:, cx:cx + 256].astype(np.int16)
        d = np.abs(got[48:224] - exp[16:192]).sum(2) > 0
        n = int(d.sum())
        total += n
        n_img += 1
        P('%-34s %6d / %d pixel lech (NES y 48-223)' % (f, n, d.size))
    P('TONG pixel lech: %d tren %d anh' % (total, n_img))


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        return
    cmd, path = sys.argv[1], sys.argv[2]
    if cmd == 'terrain2':
        cmd_terrain2(path)
        with open(os.path.join(path, 'terrain.txt'), 'w', encoding='utf-8') as f:
            f.write('\n'.join(OUT) + '\n')
        return
    out = sys.argv[3] if len(sys.argv) > 3 else os.path.dirname(os.path.abspath(path))
    T = Tele(path)
    names = list(CMDS) if cmd == 'all' else [cmd]
    for n in names:
        OUT.clear()
        CMDS[n](T)
        with open(os.path.join(out, 'accept12_%s.txt' % n), 'w', encoding='utf-8') as f:
            f.write('\n'.join(OUT) + '\n')
        print('')


if __name__ == '__main__':
    main()
