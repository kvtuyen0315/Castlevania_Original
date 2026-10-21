# kb4_knock.py (CVM3 04) - bang knockback moi lan trung don (6 lan do duoc / 3 video) tu track/*.csv cua kb4_track.py
#   t = dong ho x (lui 1 px/tick) khi chua cham tuong; sau tuong: t = t_tuong + 2*(f - f_tuong) (dong ho frame, +-2 tick)
#   so voi bang mo hinh (ConfigSimon KnockbackRise01..10 = 3,3,2,1,1,1,1,1,0,1; giu dinh 14; xuong guong; 34 tick)
# Chay: python -P -E Tools/cvm3/kb4_knock.py  -> ClaudeNotes/cvm3/ref/data/kb4_knock.csv + in tom tat
import sys, os, csv
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

RISE = [3, 3, 2, 1, 1, 1, 1, 1, 0, 1]
H = [0]
for r in RISE:
    H.append(H[-1] + r)
H += [14] * 14
for r in reversed(RISE):
    H.append(H[-1] - r)
assert len(H) == 35 and H[-1] == 0

# (video, ten track, so lan, x0 truoc trung (pivot hurt), san Y, huong lui, x tuong (None), ghi chu)
HITS = [('Boss_Map_1', 'b1_hit', 1, 228, 208, +1, 239, 'Phantom Bat cham than; mep man khoa phai (x man 239); Simon nhay-khoa nhieu frame lap'),
        ('Boss_Map_2', 'b2_hit1', 1, 33, 176, -1, 23, 'Medusa; tuong/cua trai x 23 sau 10 px'),
        ('Boss_Map_2', 'b2_hit2', 2, 23, 176, -1, 23, 'Medusa; da sat tuong trai (khong lui ngang)'),
        ('Boss_Map_2', 'b2_hit3', 3, 104, 176, -1, None, 'ran tu phia truoc; khong tuong'),
        ('Boss_Map_3', 'b3_hit1', 1, 196, 208, +1, None, 'bang gac Mummy trai; khong tuong'),
        ('Boss_Map_3', 'b3_hit2', 2, 229, 208, +1, 239, 'bang gac; Simon NGOI sat mep phai (x man 239) sau 10 px')]
rows = []
summ = []
for video, tr, n, x0, gy, d, wall, note in HITS:
    pts = []
    for r in csv.DictReader(open(os.path.join(K.CACHE, 'track', tr + '.csv'))):
        if r['sprite'] == 'hurt_0' and r['dup'] == '0' and float(r['err']) < 0.12:
            pts.append((int(r['frame']), int(r['px']), int(r['Y'])))
    tw = None; fw = None
    for f, x, y in pts:
        dx = (x - x0) * d
        h = gy - y
        if wall is not None and x == wall and (wall - x0) * d >= 0:
            if tw is None:
                tw, fw = max(dx, 0), f
            t = tw + 2 * (f - fw); clock = 'frame'
        else:
            t = dx; clock = 'x'
        mh = H[t] if 0 <= t < len(H) else None
        rows.append([video, n, f, x, y, dx, h, t, clock, mh if mh is not None else '', (h - mh) if mh is not None else '', note])
    xs = [r for r in rows if r[0] == video and r[1] == n and r[8] == 'x' and r[9] != '']
    fr = [r for r in rows if r[0] == video and r[1] == n and r[8] == 'frame' and r[9] != '']
    summ.append('%s #%d: %d mau dong ho x (max |du| %s), %d mau dong ho frame (max |du| %s) - %s' % (
        video, n, len(xs), max([abs(r[10]) for r in xs]) if xs else '-', len(fr), max([abs(r[10]) for r in fr]) if fr else '-', note))
p = os.path.join(K.REPO, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'kb4_knock.csv')
with open(p, 'w', newline='', encoding='utf-8') as fh:
    w = csv.writer(fh)
    w.writerow(['video', 'hit', 'frame', 'px', 'Y', 'dx_lui', 'cao', 't', 'dong_ho', 'cao_mo_hinh', 'du', 'ghi_chu'])
    w.writerows(rows)
print('bang mo hinh cao theo tick:', H)
print('\n'.join(summ))
print('ok', p, len(rows), 'dong')
