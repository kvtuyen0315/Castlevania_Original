# kb4_holy_table.py (CVM3 04) - bang lo nuoc thanh: moi lan nem (Map_3 tu all_map3.csv, Boss_Map_1 tu boss1_fit.txt) khop mo hinh
#   x = X0 + huong*1.5*n (floor), vy += g roi y += vy (vy0 -1.5, g 33/256); X0, y0 tu do; + chuoi lua (flame_<f>.txt cua kb4_flame.py).
# Chay (sau kb4_holy_all.py map3, kb4_holy_boss1.py, kb4_flame.py): python -P -E Tools/cvm3/kb4_holy_table.py -> ref/data/kb4_holy.csv
import sys, os, csv, math, re, glob
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

G = 33 / 256.0


def ypos(y0, n):
    return y0 - 1.5 * n + G * n * (n + 1) / 2.0


def fit(pts, spx, sY, face, prefer_x0=None):
    best = None
    for x0q in range(-24 * 8, 24 * 8):
        X0 = spx + x0q / 8.0
        ns = []
        for rel, X, Y in pts:
            c = [n for n in range(0, 90) if math.floor(X0 + face * 1.5 * n + 1e-9) == X]
            if not c:
                ns = None; break
            ns.append(min(c, key=lambda n: abs(n - (2 * rel - 16.6))))
        if ns is None:
            continue
        for y0q in range(-40 * 8, -4 * 8):
            y0 = sY + y0q / 8.0
            res = [math.floor(ypos(y0, n) + 1e-9) - Y for n, (rel, X, Y) in zip(ns, pts)]
            sse = sum(d * d for d in res)
            key = (sse, abs(X0 - spx) + abs(y0 - sY + 19))
            if best is None or key < best[0]:
                best = (key, X0, y0, ns, res)
    return best


rows = []
# ---- Map_3
allm = list(csv.DictReader(open(os.path.join(K.CACHE, 'holy', 'all_map3.csv'))))
by = {}
for r in allm:
    by.setdefault(r['throw'], []).append(r)
for thr, rs in by.items():
    if thr == '3031':
        rows.append(['Map_3', thr, rs[0]['spx'], rs[0]['sY'], rs[0]['face'], rs[0]['spose'], '', '', '', '', '', '', '', '', 'camera cuon trong luc nem (toa do man khong dung duoc) - bo'])
        continue
    face = int(rs[0]['face']); spx = int(rs[0]['spx']); sY = int(rs[0]['sY'])
    pts = [(int(r['rel']), int(r['bX']), int(r['bY'])) for r in rs if float(r['berr']) < 0.11 and r['dup'] == '0']
    b = fit(pts, spx, sY, face)
    (sse, _), X0, y0, ns, res = b
    fl = os.path.join(K.CACHE, 'holy', 'flame_%s.txt' % thr)
    fs = fe = ''
    if os.path.exists(fl):
        L = [l.split() for l in open(fl) if l[0].isdigit()]
        st = [int(l[0]) for l in L if l[2] == '0' and l[1] == '0']
        fs = st[0] if st else ''
    note = 'nhay-nem (Simon Y luc sinh ~95)' if thr == '5198' else 'tran Mummy tu khoi cao'
    rows.append(['Map_3', thr, spx, sY, face, rs[0]['spose'], '%.3f' % (X0 - spx), '%.3f' % (y0 - sY), len(pts), '%d..%d' % (min(ns), max(ns)),
                 max(abs(d) for d in res), '%.2f' % math.sqrt(sse / len(res)), max(ns), fs, note])
# ---- Boss_Map_1
txt = open(os.path.join(K.CACHE, 'holy', 'boss1_fit.txt'), encoding='utf-8').read().split('== nem ')[1:]
for blk in txt:
    m = re.match(r'f(\d+) Simon \((\d+),(\d+)\) huong ([+-]\d) (\S+)', blk.split('\n')[0])
    f0, spx, sY, face = map(int, m.groups()[:4])
    pts = []
    for l in blk.split('\n')[1:]:
        mm = re.match(r'\s+f(\d+) rel (\d+)(\*?) lo \((-?\d+),(-?\d+)\) err ([0-9.]+)', l)
        if mm and float(mm.group(6)) < 0.11 and mm.group(3) == '':
            pts.append((int(mm.group(2)), int(mm.group(4)), int(mm.group(5))))
    if f0 == 471:
        pts = [p for p in pts if p[0] >= 21]
    if len(pts) < 4:
        rows.append(['Boss_Map_1', f0, spx, sY, face, m.group(5), '', '', len(pts), '', '', '', '', '', 'lo bi doi/than Simon che hoac Simon nhay bat tu - it diem'])
        continue
    (sse, _), X0, y0, ns, res = fit(pts, spx, sY, face)
    rows.append(['Boss_Map_1', f0, spx, sY, face, m.group(5), '%.3f' % (X0 - spx), '%.3f' % (y0 - sY), len(pts), '%d..%d' % (min(ns), max(ns)),
                 max(abs(d) for d in res), '%.2f' % math.sqrt(sse / len(res)), max(ns), '', 'san phang, quay trai (lua_0 hien rel 24 sau tim -1 o f326/f459)'])
p = os.path.join(K.REPO, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'kb4_holy.csv')
with open(p, 'w', newline='', encoding='utf-8') as fh:
    w = csv.writer(fh)
    w.writerow(['video', 'frame_tim_tru', 'simon_x', 'simon_Y', 'huong', 'tu_the', 'X0_tru_simon', 'y0_tru_chan', 'so_diem', 'n', 'du_max', 'rms',
                'n_lo_cuoi', 'frame_lua_0', 'ghi_chu'])
    w.writerows(rows)
for r in rows:
    print(r)
print('ok', p)
