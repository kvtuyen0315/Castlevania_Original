# events3.py (CVM3 02) - bảng sự kiện Map_3 từ HUD từng frame + camera + Simon: mỗi lần SCORE/tim/vũ khí/ENEMY/STAGE đổi (lọc nhiễu: giá trị ổn định 3 frame hud_ok)
# Chạy (từ gốc repo): python -P -E Tools/cvm3/events3.py   -> ClaudeNotes/cvm3/ref/data/map3_events.txt + map3_hud.csv + map3_camera.csv + map3_simon.csv (bản commit, rút gọn cột)
# Nghĩa của SCORE suy từ GIÁ TRỊ theo wiki (Raven 200, Skeleton/Ghost/Medusa 300, Bone Tower 400, Fleaman 500, Mummy 3000) + ảnh đã xem (đánh dấu v)
import os, csv, shutil
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
W = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
hud = list(csv.DictReader(open(os.path.join(W, 'map3_hud.csv'))))
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(W, 'map3_camera.csv')))}
sim = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(W, 'map3_simon.csv'))) if r['found'] == '1' and float(r['err']) <= 0.12}
MEAN = {100: 'túi đỏ nhặt (+100) / vật 100', 200: 'Raven (wiki 200)', 300: 'Skeleton / Ghost / Medusa Head (wiki 300)', 400: 'Bone Tower (wiki 400) / túi tím 400',
        500: 'Fleaman (wiki 500)', 700: 'túi trắng nhặt (+700)', 1000: 'túi tiền 1000 (bí mật)', 3000: 'Mummy chết (+3000)'}
def ts(f): return '%d:%05.2f' % (f / 30 // 60, f / 30 % 60)
def near(f, win=8):
    for d in range(win + 1):
        for g in (f - d, f + d):
            if g in sim: return sim[g]
    return None
rows = [r for r in hud if r['hud_ok'] == '1']
F = ['score', 'stage', 'hearts', 'sub', 'mult', 'ebar']
out = ['# map3_events.txt (CVM3 02) - sự kiện HUD từng frame Map_3. f = frame video @30 fps (tick NES ≈ 2f, ±2); wx = x thế giới Simon (cx + x màn) tại frame gần nhất tìm thấy (±8 f); nghĩa suy theo giá trị.',
       '# cột: f | m:ss.ss | TIME | vùng/cx | wx,Y Simon | thay đổi | nghĩa']
prev = None
for k in range(len(rows) - 2):
    r = rows[k]
    cur = tuple(r[f] for f in F)
    if any('?' in x for x in cur) or cur[3] == 'occ': continue
    if tuple(rows[k + 1][f] for f in F) != cur or tuple(rows[k + 2][f] for f in F) != cur: continue
    if cur != prev:
        if prev is not None:
            fr = int(r['frame'])
            ch = []; mean = []
            for f, a, b in zip(F, prev, cur):
                if a == b: continue
                ch.append('%s %s->%s' % (f, a, b))
                if f == 'score' and int(b) > int(a):
                    d = int(b) - int(a); mean.append(MEAN.get(d, '?') + ('' if d in MEAN else ' (+%d)' % d))
                elif f == 'hearts': mean.append('tim %+d' % (int(b) - int(a)) + (' (tim lớn +5)' if int(b) - int(a) == 5 else ' (dùng vũ khí phụ -1)' if int(b) < int(a) else ''))
                elif f == 'ebar': mean.append('ENEMY %d vạch' % int(b))
                elif f == 'stage': mean.append('STAGE ' + b)
                elif f == 'sub': mean.append('vũ khí phụ ' + a + '->' + b)
                elif f == 'mult': mean.append('icon nhân vũ khí: ' + (b or 'mất'))
            c = cam[fr]; s = near(fr)
            out.append('f%d | %s | %s | %s/%s | %s | %s | %s' % (fr, ts(fr), r['time'], c['section'], c['cx'], ('wx%s,Y%s' % (s['world_x'], s['Y'])) if s else '-', ' '.join(ch), '; '.join(mean)))
        prev = cur
open(os.path.join(D, 'map3_events.txt'), 'w', encoding='utf-8').write('\n'.join(out))
print(len(out) - 2, 'su kien')
# CSV rút gọn để commit
def rdr(n): return list(csv.DictReader(open(os.path.join(W, n))))
shutil.copy(os.path.join(W, 'map3_hud.csv'), os.path.join(D, 'map3_hud.csv'))
shutil.copy(os.path.join(W, 'boss3_hud.csv'), os.path.join(D, 'boss3_hud.csv'))
shutil.copy(os.path.join(W, 'map3_camera.csv'), os.path.join(D, 'map3_camera.csv'))
shutil.copy(os.path.join(W, 'boss3_camera.csv'), os.path.join(D, 'boss3_camera.csv'))
# Simon: thêm cột tin cậy (err <= 0.12 cao; <= 0.22 thấp; hurt_0/death_0 với err > 0.12 là KHỚP SAI — PLAYER không đổi trong Map_3)
for v in ('map3', 'boss3'):
    R = rdr(v + '_simon.csv')
    with open(os.path.join(D, v + '_simon.csv'), 'w', newline='') as fh:
        h = list(R[0].keys()) + ['conf']
        w = csv.DictWriter(fh, fieldnames=h); w.writeheader()
        for r in R:
            r['conf'] = '' if r['found'] != '1' else ('hi' if float(r['err']) <= 0.12 else 'lo')
            w.writerow(r)
