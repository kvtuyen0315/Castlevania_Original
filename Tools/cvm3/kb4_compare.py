# kb4_compare.py (CVM3 04) - so vet Unity (Kb4Probe <case>.csv) voi mau video (kb4_track.py track/*.csv, kb4_holy_all.py, kb4_ledge.py).
# Chay: python -P -E Tools/cvm3/kb4_compare.py <thu muc Unity>  -> <thu muc>/compare.txt
#  knockback: (1) hinh dang = do cao theo quang lui (dong ho x) - du tung mau; (2) theo thoi gian frame (t = 2*(f - f0) + pha tot nhat)
#             rms x / y; mau sau khi cham tuong dung dong ho frame.
#  nuoc thanh: lo theo n (n = so tick bay) - du x/y tung diem video; vo (n, x lua).
#  mep roi: x tick dau roi Unity vs video (x0 + dung cuoi).
import sys, os, csv, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

U = sys.argv[1]
TR = os.path.join(K.CACHE, 'track')
out = []


def ucsv(name):
    return [r for r in csv.DictReader(open(os.path.join(U, name + '.csv')))]


def vtrack(name):
    rows = []
    for r in csv.DictReader(open(os.path.join(TR, name + '.csv'))):
        if r['sprite'] == 'hurt_0' and r['dup'] == '0' and float(r['err']) < 0.12:
            rows.append((int(r['frame']), int(r['px']), int(r['Y'])))
    return rows


# (case Unity, track video, video x0 (truoc trung), Y san, huong lui video, ghi chu)
KB = [('kb_free_l', 'b2_hit3', 104, 176, -1, 'Boss_Map_2 #3 f351-369 (ran, khong tuong)'),
      ('kb_free_r', 'b3_hit1', 196, 208, +1, 'Boss_Map_3 #1 f540-556 (bang gac, khong tuong)'),
      ('kb_wall_l', 'b2_hit1', 33, 176, -1, 'Boss_Map_2 #1 f106-122 (tuong trai sau 10 px)'),
      ('kb_wall_r', 'b1_hit', 227, 208, +1, 'Boss_Map_1 f362-379 (mep man khoa phai sau 12 px)'),
      ('kb_duck_wall_r', 'b3_hit2', 229, 216, +1, 'Boss_Map_3 #2 f699-713 (ngoi sat mep phai)')]
summary = []
for case, tr, vx0, vgy, vdir, note in KB:
    u = ucsv(case)
    hit = next(i for i, r in enumerate(u) if r['state'] == 'Hurt')
    land = next(i for i in range(hit, len(u)) if u[i]['state'] != 'Hurt')
    ux0, uy0 = int(u[hit]['x']), int(u[hit]['y'])
    if case == 'kb_duck_wall_r':
        uy0 = 176
    ut = [(i - hit, int(u[i]['x']) - ux0, uy0 - int(u[i]['y'])) for i in range(hit, land + 1)]   # (t, dx, h)
    udx = {t: dx for t, dx, h in ut}; uh = {t: h for t, dx, h in ut}
    tmax = max(udx)
    v = vtrack(tr)
    vgy0 = 208 if tr == 'b3_hit2' else vgy
    # (1) hinh dang theo dong ho x (chi mau chua cham tuong: |dx| tang)
    res_shape = []
    wall_dx = max(abs(udx[t]) for t in udx)
    for f, x, y in v:
        dx = (x - vx0) * vdir
        h = vgy0 - y
        if 0 <= dx < wall_dx:   # chua toi tuong
            t = dx
            if t in uh:
                res_shape.append((f, t, h, uh[t], h - uh[t]))
    # (2) theo frame: f0 = frame hurt dau; t = 2*(f-f0)+pha
    f0 = v[0][0]
    best = None
    for ph in range(-3, 4):
        rx, ry = [], []
        for f, x, y in v:
            t = 2 * (f - f0) + ph
            if 0 <= t <= tmax:
                rx.append((x - vx0) * vdir - abs(udx[t])); ry.append((vgy0 - y) - uh[t])
        if not rx:
            continue
        rmsx = math.sqrt(sum(a * a for a in rx) / len(rx)); rmsy = math.sqrt(sum(a * a for a in ry) / len(ry))
        if best is None or rmsx + rmsy < best[0] + best[1]:
            best = (rmsx, rmsy, ph, len(rx), max(abs(a) for a in rx), max(abs(a) for a in ry))
    s_shape = ('du cao theo dong ho x: %d mau, max |du| %d, rms %.2f' % (len(res_shape), max(abs(r[4]) for r in res_shape),
               math.sqrt(sum(r[4] ** 2 for r in res_shape) / len(res_shape)))) if res_shape else 'khong co mau dong ho x (dung sat tuong)'
    line = '%s <-> %s (%s): Unity lui %d px / bay %d tick, cao %d | %s | theo frame (pha %+d, %d mau): rms x %.2f y %.2f (max %d / %d)' % (
        case, tr, note, abs(udx[tmax]), land - hit, max(uh.values()), s_shape, best[2], best[3], best[0], best[1], best[4], best[5])
    out.append(line); summary.append((case, best[0], best[1]))
    for r in res_shape:
        out.append('    f%d t%d video cao %d Unity %d du %+d' % r)

# ---- nuoc thanh: Unity holy_drop64_r vs mo hinh video Map_3 (46 diem) + vo
u = ucsv('holy_drop64_r')
pos = []
for r in u:
    for p in r['proj'].split(';'):
        if p.startswith('HolyWater'):
            _, x, y = p.split(' '); pos.append((int(r['rel']), int(x), int(y)))
fire = next(((int(r['rel']), r['proj']) for r in u if 'HolyFire' in r['proj']), None)
spawn = pos[0][0]
un = {rel - spawn: (x - 100, y - 93) for rel, x, y in pos}
vrows = list(csv.DictReader(open(os.path.join(K.CACHE, 'holy', 'all_map3.csv'))))
vpts = set()
for r in vrows:
    if r['throw'] in ('5198', '3031') or float(r['berr']) >= 0.11:
        continue
    vpts.add((int(r['bX']), int(r['bY'])))
res = []
for X, Y in sorted(vpts):
    n = next((n for n in range(80) if 22 + math.floor(1.5 * n) == X), None)
    if n is not None and n in un:
        res.append((n, X - 22 - un[n][0], Y - 125 - un[n][1]))
fx = int(fire[1].split(';')[0].split(' ')[1]) - 100 if fire else None
out.append('holy_drop64_r <-> Map_3 tran Mummy 9 lo (46 diem quy dao): du x max %d, du y max %d, rms y %.2f; Unity vo o n %d, lua x +%s (video: lo con ve n48, lua x +73 => vo n49)' % (
    max(abs(a[1]) for a in res), max(abs(a[2]) for a in res), math.sqrt(sum(a[2] ** 2 for a in res) / len(res)), fire[0] - spawn if fire else -1, fx))
u = ucsv('holy_flat_l')
pos = []
for r in u:
    for p in r['proj'].split(';'):
        if p.startswith('HolyWater'):
            _, x, y = p.split(' '); pos.append((int(r['rel']), int(x), int(y)))
fire = next(((int(r['rel']), r['proj']) for r in u if 'HolyFire' in r['proj']), None)
fire_end = None
if fire:
    fire_end = next((int(r['rel']) for r in u if int(r['rel']) > fire[0] and 'HolyFire' not in r['proj']), None)
out.append('holy_flat_l (Unity nem tren san quay trai): sinh rel %d tai (%d,%d), vo rel %d (n %d) lua %s, lua song %s tick | video Boss_Map_1: lua_0 hien rel 24 sau tim -1 (n ~32), lo n31 con ve' % (
    pos[0][0], pos[0][1], pos[0][2], fire[0], fire[0] - pos[0][0], fire[1], (fire_end - fire[0]) if fire_end else '?'))
# ---- mep roi
LEDGE = [('ledge_eh_l', 'Map_1 f1882/1883 EH mep trai 928', 922, 921), ('ledge_cb_r', 'Map_1 f3757/3758 CB mep phai 128', 132, 134),
         ('ledge_tha_r', 'Map_2 f2409/2410 Hall A mep phai 320', 324, 326), ('ledge_tha_l', 'Map_2 f2507/2508 Hall A mep trai 288', 283, 281),
         ('ledge_thb_l', 'Map_2 f3824/3825 Hall B mep trai 608', 603, 601), ('ledge_med_r', 'Map_2 f6047/6048 Medusa mep phai 704', 708, 710),
         ('ledge_med_l', 'Map_2 f6688/6690 Medusa mep trai 224', 220, 217)]
for case, note, vstand, vx0 in LEDGE:
    u = ucsv(case)
    y0 = int(u[0]['y'])
    i = next(i for i, r in enumerate(u) if int(r['y']) > y0)
    ux = int(u[i]['x']); ulast = int(u[i - 1]['x'])
    out.append('%s (%s): Unity dung cuoi x %d, roi tick dau x %d | video dung cuoi %d, x0 %d => lech x0 %+d' % (case, note, ulast, ux, vstand, vx0, ux - vx0))
open(os.path.join(U, 'compare.txt'), 'w', encoding='utf-8').write('\n'.join(out) + '\n')
print('\n'.join(out))
