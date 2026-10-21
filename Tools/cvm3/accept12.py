# -*- coding: utf-8 -*-
"""accept12.py - CVM3 12: nghiem thu Map 3 - phan tich vet THU DONG phan Map 3 cua luong that
(Map2AcceptProbe.MinStage = 7 + StartTelemetry, Map1AcceptProbe.StartFlow) so voi so do video Map_3 / Boss_Map_3 (CVM3 02-11).
Khuon Tools/cvm2/accept12.py (CVM2 12) - doc vet bang lop Tele cua ban do.

Chay tu goc repo:  python -P -E Tools/cvm3/accept12.py <lenh> <telemetry.tsv | thu muc anh> [thu muc ra]
  objects  (mat 2) nen/khoi/bi mat/SpawnPoint hien trong luong that (dong A/E) vs Map3Objects.json + 3 nguon (MAP3_PLACEMENT, JSON, ConfigDrop)
  enemy    (mat 4) Raven / Skeleton (+ xuong) / Fleaman / Medusa Head / Bone Tower (+ cau lua) / Ghost do lai tren MAP THAT vs so do video (CVM3 07/08)
  boss     (mat 5) Mummy Men trong luong that: khoa, thuc, nhac, di, nem, bang gac, 2 x 3000, qua cau
  hud      (mat 6) TIME 500 lien 3 stage, nhip 64, pha TIME sau khi vao section, diem tung su kien, tally
  music    (mat 7) moc nhac luong that vs video (CVM3_MEMORY 2c / 2k)
  trans    (mat 8) thoi luong chuyen canh luong that vs video (CVM3_MEMORY 2k) +-2 tick
  all      chay het, ghi <ra>/accept12_<lenh>.txt
  terrain <thu muc anh terrain_*>   (mat 1) anh nen Block 3 tu StageScreen THAT (sprite an, HUD con) vs sheet Level 3 da noi:
                                     NES y 48-223 = sheet hang 16-191 (+ cap video cung cx: accept3.cmd_pairs)
Khong in tieng Viet co dau ra console (cp1252) - ghi file UTF-8.
"""
import csv, importlib.util, json, math, os, re, sys
from collections import defaultdict, OrderedDict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
REF = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
try:
    sys.stdout.reconfigure(encoding='utf-8')
except Exception:
    pass

_spec = importlib.util.spec_from_file_location('acc2', os.path.join(ROOT, 'Tools', 'cvm2', 'accept12.py'))
acc2 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(acc2)
Tele, rms, lin_slope, quad_g, black_runs = acc2.Tele, acc2.rms, acc2.lin_slope, acc2.quad_g, acc2.black_runs

B3 = ('WalkwayEntranceA', 'WalkwayEntranceB', 'WalkwayA', 'WalkwayB', 'MummyMan')
OUT = []


def P(*a):
    s = ' '.join(str(x) for x in a)
    OUT.append(s)
    try:
        print(s)
    except Exception:
        print(s.encode('ascii', 'replace').decode())


def states(tr, key=lambda d: d['state'].split()[0] if d['state'] else '-'):
    """[(state, wt dau, wt cuoi)]"""
    seg = []
    for d in tr:
        st = key(d)
        if not seg or seg[-1][0] != st:
            seg.append([st, d['wt'], d['wt']])
        seg[-1][2] = d['wt']
    return seg


def wsimon(T):
    return {d['wt']: d for d in T.W}


# ----------------------------------------------------------------------------------------------------------- mat 2: vat the
def cmd_objects(T):
    P('== MAT 2 - vat the Block 3: 3 nguon + LUONG THAT (dong A/E) vs Map3Objects.json')
    d = json.load(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Level', 'Block3', 'Map3Objects.json'), encoding='utf-8'))
    # 3 nguon: MAP3_PLACEMENT (bang §1) <-> JSON <-> ConfigDrop
    plc = {}
    for line in open(os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'MAP3_PLACEMENT.md'), encoding='utf-8'):
        m = re.match(r'\| ((?:WEA|WEB|WA|WB|MM)_C\d+) \| (\d+) \| (\d+) \| (\w+) \|', line)
        if m:
            plc[m.group(1)] = (int(m.group(2)), int(m.group(3)), m.group(4))
    drop = {}
    for r in csv.DictReader(open(os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigDrop.csv'), encoding='utf-8')):
        if r['objectId'][:3] in ('WEA', 'WEB', 'WA_', 'WB_', 'MM_', 'Wal', 'Mum'):
            drop[r['objectId']] = (int(r['stage']), r['container'], r['item'])
    want = {}
    for s in d['sections']:
        for c in s.get('candles', []):
            want[c['id']] = (s['id'], c['x'], c['y'], c.get('content', drop.get(c['id'], ('', '', '?'))[2]))
    ok3 = 0
    for cid, (sec, x, y, cont) in sorted(want.items()):
        p = plc.get(cid)
        dr = drop.get(cid)
        same = p is not None and dr is not None and (p[0], p[1], p[2]) == (x, y, dr[2])
        ok3 += same
        if not same:
            P('  LECH 3 nguon %s: PLACEMENT %s | JSON %s (%d,%d) | ConfigDrop %s' % (cid, p, sec, x, y, dr))
    P('nen: %d/%d trung 3 nguon (id, x, Y, noi dung); PLACEMENT %d dong, ConfigDrop Block 3 %d dong (gom khoi)' % (ok3, len(want), len(plc), len(drop)))
    blocks_cfg = sorted(k for k in drop if '_X' in k)
    P('khoi pha ConfigDrop: %s' % ['%s=%s' % (k, drop[k][2]) for k in blocks_cfg])
    # luong that
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
        if seen[cid] == (sec, x, y):
            ok += 1
        else:
            bad += 1
            P('  LECH %s: json %s (%d,%d) vs luong that %s' % (cid, sec, x, y, seen[cid]))
    P('luong that: nen %d/%d hien dung (section, x, Y) | lech %d | khong hien %d %s' % (ok, len(want), bad, len(miss), miss))
    brk = [e for e in T.ev if e['kind'] == 'X']
    blocks = [a for a in T.A if a['type'] == 'BreakableBlock']
    P('khoi pha hien: %s | vo trong luong: %s' % ([(b['sec'], b['pid'], b['x'], b['y']) for b in blocks], [(e['sec'], e['msg']) for e in brk]))
    secs = [a for a in T.A if a['type'] == 'SecretTrigger']
    P('bi mat (SecretTrigger) hien: %d %s' % (len(secs), [(b['sec'], b['x'], b['y']) for b in secs]))
    # SpawnPoint enemy: vi tri lan dau thay (E) vs json
    sp = {}
    for s in d['sections']:
        for p in s.get('spawnPoints', []):
            pref = os.path.basename(p.get('prefab', '')).replace('.prefab', '')
            if pref and pref != 'Candle':
                sp[p.get('name', '%s_%d' % (pref, p['x']))] = (s['id'], p['x'], p['y'], pref)
    wmap = wsimon(T)
    firsts = []
    for k, tr in T.E.items():
        t = T.Etype[k]
        if t in ('Skeleton', 'Fleaman', 'Raven', 'BoneTower'):
            w = wmap.get(tr[0]['wt'])
            firsts.append((w['sec'] if w else '?', t, tr[0]['x'], tr[0]['y'], tr[0]['wt']))
    m = 0
    for name, (sec, x, y, pref) in sorted(sp.items(), key=lambda kv: (B3.index(kv[1][0]) if kv[1][0] in B3 else 9, kv[1][1])):
        hit = [f for f in firsts if f[0] == sec and abs(f[2] - x) <= 1 and abs(f[3] - y) <= 1]
        m += 1 if hit else 0
        P('  spawn %-28s %s (%d,%d) %s: %s' % (name, sec, x, y, pref, ('sinh trong luong that wt%d (%d,%d)' % (hit[0][4], hit[0][2], hit[0][3])) if hit else 'KHONG thay sinh dung cho'))
    P('SpawnPoint enemy: %d/%d sinh dung (x, Y +-1) trong luong that' % (m, len(sp)))
    edge = [e for e in T.ev if e['kind'] == 'N']
    P('sinh tu bo sinh (N): %d lan - %s' % (len(edge), defaultdict(int, {k: sum(1 for e in edge if k in e['msg']) for k in ('MedusaHead', 'Ghost')})))
    return dict(three=(ok3, len(want)), flow=(ok, len(want), bad, miss), spawn=(m, len(sp)))


# ----------------------------------------------------------------------------------------------------------- mat 4: enemy
def cmd_enemy(T):
    P('== MAT 4 - enemy Block 3 tren MAP THAT (luong that, vet thu dong) vs so do video (CVM3 07 2g / 08 2h)')
    wmap = wsimon(T)
    res = {}
    # ---- Raven
    P('-- Raven (video: dau 60 tick sau khi hien (58-62), luot vx 0.75 (0.716-0.792), vy0 1.25/1.75/2.0 giam 9/256, treo 25 (24-26), lao ngang +1/32, phanh -> treo 14)')
    rv = []
    for k, tr in sorted(T.tracks('Raven').items(), key=lambda kv: kv[1][0]['wt']):
        seg = states(tr)
        s0 = wmap.get(tr[0]['wt'])
        perch = seg[0][2] - seg[0][1] + 1 if seg[0][0] == 'Perch' and len(seg) > 1 else None
        gl = [s for s in seg if s[0] == 'Glide']
        vxs = []
        for st, a, b in gl:
            pts = [d for d in tr if a <= d['wt'] <= b]
            if len(pts) > 4:
                vxs.append(abs(lin_slope([d['wt'] for d in pts], [d['fx'] / 256.0 for d in pts])))
        hov = [b - a + 1 for st, a, b in seg if st == 'Hover']
        rv.append((perch, vxs, hov))
        P('Raven %s (%d,%d) hien wt%d (cx %s) | dau %s tick | luot vx %s | treo %s | chuoi %s' % (
            s0['sec'] if s0 else '?', tr[0]['x'], tr[0]['y'], tr[0]['wt'], s0['cx'] if s0 else '?', perch,
            ' '.join('%.3f' % v for v in vxs), hov, ' '.join('%s%d' % (s[0][:2], s[2] - s[1] + 1) for s in seg[:9])))
    res['raven'] = rv
    # ---- Skeleton
    P('-- Skeleton (video: chang 22-23 px, 1.43-1.44 px/tick, nghi 4.6-5, chu ky luot 20-21; nem dung 18-23 tick, 1 xuong vx 0.725-0.778, 2 do cao dinh 61.5-62.6 / 90.8-93.0 tren pivot ve; nhay chi o mep vy0 -2.5 g 0.125 vx 1.17-1.35)')
    for k, tr in sorted(T.tracks('Skeleton').items(), key=lambda kv: kv[1][0]['wt']):
        seg = states(tr, key=lambda d: d['state'] or '-')
        s0 = wmap.get(tr[0]['wt'])
        legs = []
        for st, a, b in seg:
            if st in ('ActL', 'ActR'):
                pts = [d for d in tr if a <= d['wt'] <= b]
                legs.append((b - a + 1, abs(pts[-1]['fx'] - pts[0]['fx']) / 256.0 if len(pts) > 1 else 0, abs(lin_slope([d['wt'] for d in pts], [d['fx'] / 256.0 for d in pts])) if len(pts) > 2 else float('nan')))
        rests = [b - a + 1 for st, a, b in seg if st == 'Act']
        thr = [b - a + 1 for st, a, b in seg if st == 'ActT']
        jumps = []
        for st, a, b in seg:
            if st == 'Jump':
                pts = [d for d in tr if a - 1 <= d['wt'] <= b]
                if len(pts) > 6:
                    ts = [d['wt'] - pts[0]['wt'] for d in pts]
                    g = quad_g(ts, [d['fy'] / 256.0 for d in pts])
                    vx = abs(lin_slope(ts, [d['fx'] / 256.0 for d in pts]))
                    jumps.append((b - a + 1, round(vx, 3), round(g, 4) if g else None, min(d['y'] for d in pts) - pts[0]['y']))
        P('Skeleton %s (%d,%d) wt%d-%d | chang (tick, px, px/tick): %s | nghi %s | nem %s | nhay (tick, vx, g, dinh) %s' % (
            s0['sec'] if s0 else '?', tr[0]['x'], tr[0]['y'], tr[0]['wt'], tr[-1]['wt'],
            ['%d/%.1f/%.3f' % l for l in legs[:6]], rests[:6], thr[:4], jumps[:3]))
    bones = sorted(T.tracks('SkeletonBone').items(), key=lambda kv: kv[1][0]['wt'])
    for k, tr in bones:
        ts = [d['wt'] - tr[0]['wt'] for d in tr]
        vx = abs(lin_slope(ts, [d['fx'] / 256.0 for d in tr])) if len(tr) > 2 else float('nan')
        g = quad_g(ts, [d['fy'] / 256.0 for d in tr]) if len(tr) > 5 else None
        sk = [t for t in T.tracks('Skeleton').values() if any(d['wt'] == tr[0]['wt'] - 1 for d in t)]
        sky = None
        if sk:
            near = min(sk, key=lambda t: abs(next(d['x'] for d in t if d['wt'] == tr[0]['wt'] - 1) - tr[0]['x']))
            sky = next(d['y'] for d in near if d['wt'] == tr[0]['wt'] - 1)
        P('  xuong sinh wt%d (%d,%d) Skeleton Y %s => sinh Y - %s | vx %.3f | g %s | dinh %d px tren Skeleton Y' % (
            tr[0]['wt'], tr[0]['x'], tr[0]['y'], sky, (sky - tr[0]['y']) if sky is not None else '?', vx, ('%.4f' % g) if g else '-',
            (sky - min(d['y'] for d in tr)) if sky is not None else -1))
    # ---- Fleaman
    P('-- Fleaman (video: cho khung 0<->1 x 9; vao tam |dx| < 128 => dem 62/62/136; ngoi thap 12.4-14.2; cu ngan vx 2.25 vy0 -1 g 0.125)')
    for k, tr in sorted(T.tracks('Fleaman').items(), key=lambda kv: kv[1][0]['wt']):
        seg = states(tr, key=lambda d: d['state'] or '-')
        s0 = wmap.get(tr[0]['wt'])
        enter = next((d['wt'] for d in tr if wmap.get(d['wt']) and wmap[d['wt']]['sx'] is not None and abs(wmap[d['wt']]['sx'] - d['x']) < 128), None)
        leave_wait = next((a for st, a, b in seg if st != 'Wait'), None)
        ground = [b - a + 1 for st, a, b in seg if st == 'Ground']
        hops = []
        for st, a, b in seg:
            if st.startswith('Hop'):
                pts = [d for d in tr if a <= d['wt'] <= b]
                if len(pts) > 3:
                    ts = [d['wt'] - pts[0]['wt'] for d in pts]
                    hops.append((st, len(pts), round(abs(lin_slope(ts, [d['fx'] / 256.0 for d in pts])), 3), round(quad_g(ts, [d['fy'] / 256.0 for d in pts]) or 0, 4)))
        P('Fleaman %s (%d,%d) wt%d | vao tam wt%s => roi Wait sau %s tick | Ground %s | cu nhay %s | chet wt%d' % (
            s0['sec'] if s0 else '?', tr[0]['x'], tr[0]['y'], tr[0]['wt'], enter, (leave_wait - enter) if enter and leave_wait else '-', ground[:4], hops[:4], tr[-1]['wt']))
    # ---- Medusa Head
    P('-- Medusa Head (video: vx 1.217-1.272, song P 100 A 33.75 (1-cos), dinh = chan - 48.2..-48.8 o mep TRUOC mat, nhip 126.9-128.8 (8/12); vung WalkwayA tu Simon x 125-170, MummyMan 959-998 .. 1245-1272)')
    rows, spawns = [], []
    for k, tr in sorted(T.tracks('MedusaHead').items(), key=lambda kv: kv[1][0]['wt']):
        t0 = tr[0]['wt']
        ts = [d['wt'] - t0 for d in tr]
        vx = lin_slope(ts, [d['fx'] / 256.0 for d in tr])
        y0 = tr[0]['y']
        r = rms([a - (y0 + 33.75 * (1 - math.cos(2 * math.pi * t / 100.0))) for a, t in zip([d['y'] for d in tr], ts)])
        s = wmap.get(t0)
        spawns.append((s['sec'] if s else '?', t0, s['sx'] if s else None))
        rows.append((s['sec'] if s else '?', t0, len(tr), vx, max(d['y'] for d in tr) - min(d['y'] for d in tr), r, (y0 - s['sy']) if s else None,
                     tr[0]['x'] - (s['cx'] if s else 0), s['sx'] if s else None, tr[0]['facing'], s['facing'] if s else 0))
    P('section | tick sinh | so tick | vx | dinh-day | rms vs song | y sinh - chan | x man sinh | Simon x | huong MH | huong Simon')
    for r in rows:
        P('%s | %d | %d | %.3f | %d | %.2f | %s | %d | %s | %+d | %+d' % r)
    gaps = [(s1, t2 - t1) for (s1, t1, x1), (s2, t2, x2) in zip(spawns, spawns[1:]) if s1 == s2]
    P('khoang 2 lan sinh lien tiep cung section: %s' % gaps)
    res['mh'] = rows
    # ---- Bone Tower
    P('-- Bone Tower (video: cau lua 1.85-2.32 (TB 2.0), y = chan - 15 (5/5), sinh o x thap, cap cach 39-40; phat dau 48-51 tick sau khi thap HIEN; Unity 40 sau khi sinh)')
    towers = T.tracks('BoneTower')
    fbs = sorted(T.tracks('BoneTowerFireball').items(), key=lambda kv: kv[1][0]['wt'])
    speeds, rel = [], []
    for k, tr in fbs:
        if len(tr) > 3:
            speeds.append(abs(lin_slope([d['wt'] for d in tr], [d['fx'] / 256.0 for d in tr])))
        live = [t for t in towers.values() if t[0]['wt'] <= tr[0]['wt'] <= t[-1]['wt'] + 1]
        tw = min(live, key=lambda t: abs(t[0]['x'] - tr[0]['x'])) if live else None
        if tw:
            rel.append((tr[0]['wt'], tw[0]['x'], tr[0]['x'] - tw[0]['x'], tr[0]['y'] - tw[0]['y'], tw[0]['wt']))
    P('%d cau lua: toc do %s' % (len(fbs), ' '.join('%.3f' % s for s in speeds)))
    for k, tw in sorted(towers.items(), key=lambda kv: kv[1][0]['wt']):
        s0 = wmap.get(tw[0]['wt'])
        a, b = tw[0]['wt'], tw[-1]['wt']
        ts = [r for r in rel if r[4] == a]
        P('thap %s (%d,%d) song wt%d-%d: cau (tick tu luc sinh, dx, dy) %s' % (s0['sec'] if s0 else '?', tw[0]['x'], tw[0]['y'], a, b, [(r[0] - a, r[2], r[3]) for r in ts]))
    res['fb'] = speeds
    # ---- Ghost
    P('-- Ghost (video Map_3 WEA f416: hien 96 px SAU LUNG, tam = chan Simon - 31, ngang 0.76, doc 0.12)')
    for k, tr in sorted(T.tracks('Ghost').items(), key=lambda kv: kv[1][0]['wt']):
        s = wmap.get(tr[0]['wt'])
        tr40 = tr[:40]
        P('Ghost %s sinh wt%d (%d,%d) | Simon (%s,%s) huong %+d => x - Simon %+d, y - chan %+d | vx 40 tick dau %.3f | song %d tick' % (
            s['sec'] if s else '?', tr[0]['wt'], tr[0]['x'], tr[0]['y'], s['sx'], s['sy'], s['facing'], tr[0]['x'] - s['sx'], tr[0]['y'] - s['sy'],
            abs(lin_slope([d['wt'] for d in tr40], [d['fx'] / 256.0 for d in tr40])) if len(tr40) > 2 else float('nan'), len(tr)))
    # ---- roi vat
    kills = [e for e in T.ev if e['kind'] == 'K']
    P('-- Ha enemy trong luong that: %d lan; vat roi tai cho enemy: Unity khong co luat (CVM3 08 KHONG dung - §5 #11)' % len(kills))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 5: Mummy Men
def cmd_boss(T):
    P('== MAT 5 - Mummy Men trong LUONG THAT (vet thu dong) vs video (CVM3 09 2i)')
    res = {}
    mm = sorted(T.tracks('MummyMan').values(), key=lambda t: -t[0]['x'])
    if not mm:
        P('KHONG co MummyMan trong vet')
        return res
    lock = next((d for d in T.W if d['sec'] == 'MummyMan' and d['locked']), None)
    P('khoa camera: wt%d cx %d Simon x %s (video f5931: cx 1280 khi Simon x 1409; Unity khoa o Simon 1408)' % (lock['wt'], lock['cx'], lock['sx']))
    res['lock'] = (lock['cx'], lock['sx'])
    for tr in mm:
        seen = tr[0]
        wake = next((d for d in tr if 'dormant=0' in d['extra']), None)
        P('Mummy dung cho (%d,%d) tu wt%d (truoc khoa %d tick) | thuc wt%s = khoa + %s (video 119.5-120.9)' % (
            seen['x'], seen['y'], seen['wt'], lock['wt'] - seen['wt'], wake['wt'] if wake else '-', (wake['wt'] - lock['wt']) if wake else '-'))
    pm = next((e for e in T.events('M', 'play PoisonMind')), None)
    if pm:
        P('Poison Mind: wt%d = khoa + %d (video ~ khoa + 1.2 s; Unity Config 60)' % (pm['wt'], pm['wt'] - lock['wt']))
        res['pm'] = pm['wt'] - lock['wt']
    for tr in mm:
        seg = states(tr)
        steps = []
        for a_, b_ in zip(tr, tr[1:]):
            if b_['wt'] == a_['wt'] + 1 and a_['state'].startswith('Walk') and b_['state'].startswith('Walk') and b_['fx'] != a_['fx']:
                steps.append(abs(b_['fx'] - a_['fx']) / 256.0)
        walks = ['buoc TB %.4f px/tick (%d tick di that)' % (sum(steps) / len(steps), len(steps))] if steps else []
        edge = sum(1 for a_, b_ in zip(tr, tr[1:]) if b_['wt'] == a_['wt'] + 1 and a_['state'].startswith('Walk') and b_['state'].startswith('Walk') and b_['fx'] == a_['fx'])
        walks.append('%d tick Walk dung tai cho (mep man / quay)' % edge)
        throws = [b - a + 1 for st, a, b in seg if st == 'Throw']
        P('Mummy (%d,%d): di %s px/tick (video 0.741-0.767) | Throw (tick) %s (video dung 46-51, Config 48) | chuoi %s' % (
            tr[0]['x'], tr[0]['y'], walks[:8], throws, ' '.join('%s%d' % (s[0][:2], s[2] - s[1] + 1) for s in seg[:14])))
    bd = sorted(T.tracks('MummyBandage').items(), key=lambda kv: kv[1][0]['wt'])
    for k, tr in bd:
        ts = [d['wt'] - tr[0]['wt'] for d in tr]
        vx = lin_slope(ts, [d['fx'] / 256.0 for d in tr]) if len(tr) > 2 else float('nan')
        y0 = tr[0]['fy'] / 256.0
        r = rms([d['fy'] / 256.0 - (y0 + 24 * math.sin(2 * math.pi * t / 102.0)) for d, t in zip(tr, ts)])
        thrower = min(mm, key=lambda t: abs(next((d['x'] for d in t if d['wt'] == tr[0]['wt'] - 1), 9999) - tr[0]['x']))
        tf = next((d for d in thrower if d['wt'] == tr[0]['wt'] - 1), None)
        P('  bang sinh wt%d (%d,%d) Mummy (%s,%s) => y - chan %s | vx %+.3f | rms song 24 sin(2pi t/102) %.2f | song %d tick' % (
            tr[0]['wt'], tr[0]['x'], tr[0]['y'], tf['x'] if tf else '?', tf['y'] if tf else '?', (tr[0]['y'] - tf['y']) if tf else '?', vx, r, len(tr)))
    hp = [d['ehp'] for d in T.W if d['sec'] == 'MummyMan' and d['wt'] >= lock['wt']]
    drops = sum(1 for a, b in zip(hp, hp[1:]) if b < a)
    P('thanh ENEMY dung chung: %d lan giam, 16 -> %d (video 16 lan -1)' % (drops, hp[-1] if hp else -1))
    bk = [e for e in T.ev if e['kind'] == 'B' or (e['kind'] == 'K' and 'Mummy' in e['msg'])]
    P('ha: %s' % [(e['kind'], e['wt'], e['msg'], e['score']) for e in bk])
    sc = [d['score'] for d in T.W if d['sec'] == 'MummyMan' and d['wt'] >= lock['wt']]
    jumps = [(b - a) for a, b in zip(sc, sc[1:]) if b - a >= 1000]
    P('buoc SCORE >= 1000 trong phong: %s (video +3000 MOI con)' % jumps)
    res['scores'] = jumps
    cr = next((e for e in T.events('C')), None)
    bdead = next((e for e in T.events('B')), None)
    if cr and bdead:
        P('con cuoi bi ha wt%d -> qua cau hien wt%d (+%d; video +117: 118.2 / 116.2) tai %s (video (cx+128, 104) = (1408,104))' % (bdead['wt'], cr['wt'], cr['wt'] - bdead['wt'], cr['msg']))
    items = [t for t in T.tracks('ItemPickup').values() if 'MagicCrystal' in t[0]['extra']]
    if items:
        it = items[0]
        first_move = next((d for d in it if d['y'] != it[0]['y']), None)
        land = next((d for d in it if d['y'] == it[-1]['y']), it[-1])
        P('qua cau: dung tu wt%d, roi dau wt%s (+%s; video 35-37 sau khi hien, Config hover 34), y cuoi %d o wt%d (+%d) - %s' % (
            it[0]['wt'], first_move['wt'] if first_move else '-', (first_move['wt'] - it[0]['wt']) if first_move else '-', it[-1]['y'], it[-1]['wt'], it[-1]['wt'] - it[0]['wt'],
            'cham san y 176 (video +57)' if it[-1]['y'] == 176 else 'Simon nhat khi qua cau con dang roi (lo trinh tu dong nhay len)'))
    return res


# ----------------------------------------------------------------------------------------------------------- mat 6: HUD + luat
def cmd_hud(T):
    P('== MAT 6 - HUD & luat trong LUONG THAT (vet thu dong)')
    res = {}
    st7 = next(d for d in T.W if d['stage'] == 7 and d['sec'] == 'WalkwayEntranceA')
    P('vao STAGE 07 wt%d: TIME %d, tim %d, P %d, SCORE %d, ENEMY %d (video 0500, tim 05, ENEMY 16)' % (st7['wt'], st7['time'], st7['hearts'], st7['lives'], st7['score'], st7['ehp']))
    ch, prev = [], None
    for d in T.W:
        if d['sec'] not in B3:
            continue
        if prev is not None and d['time'] != prev['time']:
            ch.append((d['wt'], prev['time'], d['time'], d['stage'], d['sec']))
        prev = d
    resets = [c for c in ch if c[2] > c[1]]
    P('TIME tang (dat lai) trong Block 3: %d lan %s (video: KHONG dat lai qua STAGE 08/09 - 0432 / 0380 giu)' % (len(resets), resets[:5]))
    stage_ch, prev = [], None
    for d in T.W:
        if prev and d['stage'] != prev['stage'] and 7 <= d['stage'] <= 9:
            stage_ch.append((d['wt'], prev['stage'], d['stage'], prev['time'], d['time']))
        prev = d
    P('doi STAGE (wt, cu, moi, TIME truoc, TIME sau): %s' % stage_ch)
    pick = next((e for e in T.events('F', 'crystal')), None)
    dec = [c for c in ch if c[2] == c[1] - 1 and (pick is None or c[0] < pick['wt'])]
    gaps = defaultdict(int)
    for a, b in zip(dec, dec[1:]):
        gaps[b[0] - a[0]] += 1
    P('khoang giua 2 lan TIME giam (truoc tally): %s (video 64 tick/dv; khoang dai = TIME dung o cua / den)' % dict(sorted(gaps.items())))
    res['time_resets'] = len(resets)
    # pha TIME sau khi vao section (mot F stage_*/stair/door) - Unity hang 60/59/58
    # pha TIME theo FRAME (the gioi dung luc den => wt khong chay): video STAGE 07 hien -> 60, dau den thang -> 82, STAGE 08/09 hien -> 58/57
    chf, prev = [], None
    for d in T.W:
        if d['sec'] in B3 and prev is not None and d['time'] == prev['time'] - 1:
            chf.append(d['frame'])
        prev = d
    first7 = st7['frame']
    nxt = next((f for f in chf if f > first7), None)
    P('  STAGE 07 hien frame %d => TIME giam dau sau %s tick (video 60)' % (first7, (nxt - first7) if nxt else '-'))
    F = T.events('F')
    for e in F:
        if e['msg'].startswith('stage') or e['msg'] == 'stair_up':
            nxt = next((f for f in chf if f > e['frame']), None)
            ref = 'video 58 / 57 sau khi STAGE moi hien' if e['msg'].startswith('stage') else 'video 82 sau DAU den thang'
            P('  moc %-10s frame %d %s => TIME giam dau sau %s tick (%s)' % (e['msg'], e['frame'], e['sec'], (nxt - e['frame']) if nxt else '-', ref))
    P('-- diem tung su kien (K/B/P/U) = buoc SCORE HUD')
    for e in T.ev:
        if e['kind'] in ('K', 'B', 'P', 'U', 'X', 'L', 'R'):
            P('  %s wt%d %s %s | SCORE sau %d' % (e['kind'], e['wt'], e['sec'], e['msg'], e['score']))
    done = next((e for e in T.events('F', 'tally_done')), None)
    if pick and done:
        before = max((d for d in T.W if d['wt'] <= pick['wt']), key=lambda d: d['wt'])
        exp = before['score'] + before['time'] * 10 + before['hearts'] * 100
        P('tally: luc nhat cau SCORE %d TIME %d tim %d => ky vong %d | Unity luc tally_done %d => %s' % (
            before['score'], before['time'], before['hearts'], exp, done['score'], 'KHOP' if exp == done['score'] else 'LECH'))
        res['tally'] = exp == done['score']
    hp = [(d['wt'], d['hp']) for d in T.W if d['sec'] in B3]
    hd = [(b[0], a[1] - b[1]) for a, b in zip(hp, hp[1:]) if b[1] < a[1]]
    P('Simon mat mau trong luong that (wt, vach): %s (lo trinh bat tu theo doan - chi WEA chiu don)' % hd)
    return res


# ----------------------------------------------------------------------------------------------------------- mat 7: nhac
def cmd_music(T):
    P('== MAT 7 - nhac luong that (AudioManager.MusicEvent) vs video Map_3 (CVM3_MEMORY 2c/2k)')
    ms = [e for e in T.ev if e['kind'] == 'M']
    for e in ms:
        P('  frame %d wt %d %s | %s' % (e['frame'], e['wt'], e['sec'] or '-', e['msg']))
    def first_frame(pred):
        for d in sorted(T.S + T.W, key=lambda d: d['frame']):
            if pred(d):
                return d['frame']
        return None
    st7 = first_frame(lambda d: d.get('screen') == 'Stage1' and not d.get('trans') and d.get('stage') == 7 and 'wt' in d)
    wc = next((e for e in ms if 'play WickedChild' in e['msg']), None)
    plays = [e for e in ms if e['msg'].startswith('play')]
    P('so lan play trong Map 3: %d (%s) - video: Wicked Child 1 lan (KHONG phat lai o thang/cua/STAGE), Poison Mind, Stage Clear' % (len(plays), ', '.join(e['msg'] for e in plays)))
    if wc and st7:
        P('Wicked Child: frame %d, khung STAGE 07 dau tien %d => %+d tick (video t0 6.862 s = luc hien STAGE 07)' % (wc['frame'], st7, wc['frame'] - st7))
    loops = [e['frame'] for e in ms if 'loop WickedChild' in e['msg']]
    if loops:
        P('Wicked Child lap o frame %s, khoang %s (video vong 45.27 s = %d tick o 60 fps)' % (loops, [b - a for a, b in zip(loops, loops[1:])], round(45.27 * 60)))
    lock = next((d for d in T.W if d['sec'] == 'MummyMan' and d['locked']), None)
    pm = next((e for e in ms if 'play PoisonMind' in e['msg']), None)
    if lock and pm:
        P('Poison Mind: khoa + %d tick (video ~ +1.2 s = ~72 +-18; Config 60)' % (pm['wt'] - lock['wt']))
    pick = next((e for e in T.events('P', 'MagicCrystal')), None)
    sc = next((e for e in ms if 'play StageClear' in e['msg']), None)
    if pick and sc:
        P('Stage Clear: frame %d, nhat cau frame %d => %+d (video cung luc)' % (sc['frame'], pick['frame'], sc['frame'] - pick['frame']))
    stop = [(e['frame'], e['msg']) for e in ms if 'stop' in e['msg'] or 'end' in e['msg']]
    P('dung/het nhac: %s (video: tally + ban do B3 im)' % stop)
    return {}


# ----------------------------------------------------------------------------------------------------------- mat 8: chuyen canh
def cmd_trans(T):
    P('== MAT 8 - chuyen canh luong that vs video Map_3 (CVM3_MEMORY 2k; tick = frame video x 2.0033, moc don +-2)')
    res = []
    def chk(name, u, v, tol=2):
        ok = v is None or (u is not None and abs(u - v) <= tol)
        res.append((name, u, v, ok))
        P('  %-58s Unity %6s | video %6s | %s' % (name, u, '-' if v is None else v, 'DAT' if ok else 'LECH'))
    br = black_runs(T)
    for r in br:
        P('  den: frame %d-%d (%d khung) %s -> %s' % r)
    b0 = next((r for r in br if r[3] == 'CastleMap' and r[4] == 'Stage1'), None)
    if b0:
        chk('ban do B2 -> STAGE 07: den (khung)', b0[2], 26)
    F = T.events('F')
    fs = [(e['frame'], e['wt'], e['msg'], e['sec'], e['simon'], e['cx']) for e in F]
    P('  moc StageFlow: %s' % [(f, m, s, p, c) for f, w, m, s, p, c in fs])
    st = [(i, x) for i, x in enumerate(fs) if x[2] == 'stair_up']
    vid_black = {'WalkwayEntranceB': 22, 'WalkwayB': 24}
    vid_arr = {'WalkwayEntranceB': ('96,189', 0), 'WalkwayB': ('207,191', 79)}
    for i, x in st:
        nx = fs[i + 1] if i + 1 < len(fs) else None
        if not nx:
            continue
        chk('thang len-trai -> %s: den (tick)' % nx[3], nx[0] - x[0], vid_black.get(nx[3]), tol=2)
        P('    hien %s cx %d (video %s cx %s)' % (nx[4], nx[5], vid_arr.get(nx[3], ('?', '?'))[0], vid_arr.get(nx[3], ('?', '?'))[1]))
    VD = [dict(stage=414.5, name='cua PHAI #1 (WEB -> WalkwayA)', d1='(+4.1,+8.1]', d1m=6.1, d1t=2, cl='(+104.3,+110.3]', clm=107.3),
          dict(stage=416.6, name='cua PHAI #2 (WalkwayB -> MummyMan)', d1='(+3.2,+11.2]', d1m=7.2, d1t=4, cl='(+107.3,+113.3]', clm=110.3)]
    di = [i for i, x in enumerate(fs) if x[2] == 'door']
    for n, i in enumerate(di[:2]):
        seq = {}
        for j in range(i, min(i + 8, len(fs))):
            seq.setdefault(fs[j][2], fs[j][0])
        st_ = next((fs[j] for j in range(i, len(fs)) if fs[j][2].startswith('stage')), None)
        v = VD[n]
        if 'door_stop' in seq:
            chk('%s: cuon 1 (door -> door_stop)' % v['name'], seq['door_stop'] - seq['door'], 128)
            if 'door_open' in seq:
                chk('%s: door_stop -> canh door_1 (video %s)' % (v['name'], v['d1']), seq['door_open'] - seq['door_stop'], v['d1m'], tol=v['d1t'])
            if 'door_closed' in seq:
                chk('%s: door_stop -> cot cua tro lai (video %s)' % (v['name'], v['cl']), seq['door_closed'] - seq['door_stop'], v['clm'], tol=3)
        if st_:
            chk('%s: bat dau cuon -> STAGE moi' % v['name'], st_[0] - seq['door'], v['stage'])
            P('    sau cua: %s %s cx %d (video (48,%s) quay phai cx 0)' % (st_[3], st_[4], st_[5], '128' if n == 0 else '96'))
    lock = next((d for d in T.W if d['sec'] == 'MummyMan' and d['locked']), None)
    mm = sorted(T.tracks('MummyMan').values(), key=lambda t: -t[0]['x'])
    if lock and mm:
        wake = next((d for d in mm[0] if 'dormant=0' in d['extra']), None)
        if wake:
            chk('khoa phong Mummy -> thuc (tick)', wake['wt'] - lock['wt'], 120)
        bdead = next((e for e in T.events('B')), None)
        cr = next((e for e in T.events('C')), None)
        if cr and bdead:
            chk('Mummy cuoi bi ha -> qua cau (tick)', cr['wt'] - bdead['wt'], 117)
    pick = next((e for e in T.events('F', 'crystal')), None)
    if pick:
        tw = [d for d in T.W if d['wt'] > pick['wt']]
        t_at = next(x for x in T.W if x['wt'] == pick['wt'])['time']
        t0 = next((d for d in tw if d['time'] < t_at), None)
        if t0:
            chk('nhat cau -> TIME bat dau dem (tick)', t0['wt'] - pick['wt'], 226.4)
            tz = next((d for d in tw if d['time'] == 0), None)
            if tz:
                chk('TIME dem 1 tick/dv: so tick TIME %d -> 0' % t_at, tz['wt'] - t0['wt'] + 1, t_at, tol=2)
                h0 = next((d for d in tw if d['wt'] > tz['wt'] and d['hearts'] < tz['hearts']), None)
                if h0:
                    chk('TIME 0000 -> tim dau tien (tick)', h0['wt'] - tz['wt'], 54.1)
                    hs = [d for d in tw if d['wt'] >= h0['wt']]
                    hd = [b['wt'] for a, b in zip(hs, hs[1:]) if b['hearts'] < a['hearts']]
                    if len(hd) > 2:
                        chk('nhip tim (tick, TB)', round((hd[-1] - hd[0]) / (len(hd) - 1), 2), 7.97, tol=0.1)
    b2 = next((r for r in br if r[3] == 'Stage1' and r[4] == 'CastleMap' and pick and r[0] > pick['frame']), None)
    if b2:
        chk('tally xong -> ban do B3: den (khung)', b2[2], 20.4)
    b3 = next((r for r in br if r[3] == 'CastleMap' and r[4] == 'Stage10Stub'), None)
    if b3:
        mp = [d['frame'] for d in T.S if d['screen'] == 'CastleMap' and not d['trans'] and (not pick or d['frame'] > pick['frame'])]
        P('  ban do B3: hien frame %s -> den %d (%d tick; video het 4:11.6 khi ban do con truot - SUY LUAN §5 #16) -> Stage10Stub (den %d khung)' % (
            mp[0] if mp else '-', b3[0], (b3[0] - mp[0]) if mp else -1, b3[2]))
    s10 = next((d for d in T.S if d['screen'] == 'Stage10Stub' and not d['trans']), None)
    if s10:
        P('  STAGE 10: TIME %d, tim %d, STAGE %d, SCORE %d (video ban do B3: STAGE 10 tim 05 TIME 0000)' % (s10['time'], s10['hearts'], s10['stage'], s10['score']))
    n_ok = sum(1 for r in res if r[3])
    P('  => %d/%d moc trong +-2 tick (hoac khop)' % (n_ok, len(res)))
    return res


CMDS = OrderedDict([('objects', cmd_objects), ('enemy', cmd_enemy), ('boss', cmd_boss), ('hud', cmd_hud), ('music', cmd_music), ('trans', cmd_trans)])


def cmd_terrain(udir):
    """(mat 1) anh nen Block 3 chup tu StageScreen THAT (Map1AcceptProbe.StartTerrain - sprite an, HUD con) vs vung sheet Level 3 da noi:
    NES y 48-223 (duoi dai HUD) = sheet hang 16-191; cung cach lay mau tam o 4x4 nhu accept3.py."""
    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
    sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvgp'))
    import accept3 as A3
    import numpy as np
    regs = A3.T3.padded_regions()
    total = n_img = 0
    shots = []
    for f in sorted(os.listdir(udir)):
        m = re.match(r'terrain_(\w+?)_cx(\d+)\.png$', f)
        if not m or m.group(1) not in B3:
            continue
        sec, cx = m.group(1), int(m.group(2))
        shots.append((f, sec, cx))
        got = A3.unity_nes(os.path.join(udir, f))
        exp = regs[sec][:, cx:cx + 256].astype(np.int16)
        d = np.abs(got[48:224] - exp[16:192]).sum(2) > 0
        n = int(d.sum())
        total += n
        n_img += 1
        P('%-34s %6d / %d pixel lech (NES y 48-223)' % (f, n, d.size))
    P('TONG pixel lech: %d tren %d anh' % (total, n_img))
    return shots


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        return
    cmd, path = sys.argv[1], sys.argv[2]
    if cmd == 'terrain':
        shots = cmd_terrain(path)
        with open(os.path.join(path, 'terrain.txt'), 'w', encoding='utf-8') as f:
            f.write('\n'.join(OUT) + '\n')
        if len(sys.argv) > 3:   # cap video cung cx (accept3.cmd_pairs voi ten anh terrain_*)
            import accept3 as A3
            A3.shots = lambda udir: shots
            A3.cmd_pairs(path, sys.argv[3])
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
