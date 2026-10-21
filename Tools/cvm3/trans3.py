# trans3.py (CVM3 02) - chuyển cảnh Map_3 theo tick từ map3_camera.csv / map3_hud.csv / map3_simon.csv / lưới NES
# Chạy (từ gốc repo): python -P -E Tools/cvm3/trans3.py   -> ClaudeNotes/cvm3/ref/data/map3_transitions.txt
# 1 frame video = 2 tick NES (30 -> 60 Hz) NHƯNG video ~22.7 khung mới/s (dup_frames3) => mốc đơn lẻ ±2 tick; thời lượng = hồi quy / TIME.
import os, csv
import numpy as np
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
W = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_transitions.txt')
mm = np.load(os.path.join(W, 'map3_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(W, 'map3_dup.npy'))
cam = list(csv.DictReader(open(os.path.join(W, 'map3_camera.csv'))))
hud = list(csv.DictReader(open(os.path.join(W, 'map3_hud.csv'))))
sim = list(csv.DictReader(open(os.path.join(W, 'map3_simon.csv'))))
out = []
def P(s=''): out.append(s)
def ts(f): return '%d:%05.2f' % (f / 30 // 60, f / 30 % 60)
def fit(xs, ys):
    xs = np.array(xs, float); ys = np.array(ys, float)
    A = np.vstack([xs, np.ones_like(xs)]).T
    m, c = np.linalg.lstsq(A, ys, rcond=None)[0]
    r = ys - (m * xs + c)
    return m, c, float(np.abs(r).max())

# 1. bản đồ lâu đài B2 (frame 0..193)
P('== 1. Bản đồ B2 -> đen -> STAGE 07 (frame video @30 fps; tick = 2 x frame, sai số +-2 tick)')
edge = []; simx = []
for f in range(0, 194):
    fr = np.asarray(mm[f]).astype(int)
    reg = fr[60 - 8:200 - 8]
    red = (reg[..., 0] > 130) & (reg[..., 1] < 90) & (reg[..., 2] < 40)
    cols = np.nonzero(red.sum(axis=0) > 8)[0]
    edge.append(int(cols.min()) if len(cols) else -1)
    r2 = fr[190 - 8:222 - 8]
    o = (r2[..., 0] > 200) & (r2[..., 1] > 120) & (r2[..., 1] < 180) & (r2[..., 2] < 70)
    ys, xs = np.nonzero(o)
    simx.append(float(xs.mean()) if o.sum() > 6 else None)
sl = [(f, edge[f]) for f in range(10, 78) if not dup[f]]
m, c, e = fit([a for a, b in sl], [b for a, b in sl])
P('map trượt VÀO từ phải sang trái (mép trái bản đồ x=231 ở f4 -> x≈6 ở f80): %.3f px/frame video = %.3f px/tick (dư tối đa %.1f px); dừng ≈ f%d (%s)' % (m, m / 2, e, int(round((6 - c) / m)), ts(int(round((6 - c) / m)))))
first_sim = next(f for f in range(190) if simx[f] is not None)
sl = [(f, simx[f]) for f in range(first_sim + 4, 186) if simx[f] is not None and not dup[f]]
m2, c2, e2 = fit([a for a, b in sl], [b for a, b in sl])
P('Simon nhỏ hiện f%d (%s) tại x≈%.0f, đi phải %.3f px/frame = %.3f px/tick (dư tối đa %.1f px); x(f182)=%.0f' % (first_sim, ts(first_sim), simx[first_sim], m2, m2 / 2, e2, m2 * 182 + c2))
black = [r for r in cam if r['kind'] == 'black']
runs = []
for r in black:
    f = int(r['frame'])
    if runs and f == runs[-1][1] + 1: runs[-1][1] = f
    else: runs.append([f, f])
P('đen: ' + ', '.join('f%d-f%d (%d frame = %d tick, %s-%s)' % (a, b, b - a + 1, 2 * (b - a + 1), ts(a), ts(b)) for a, b in runs))

# 2. thời gian TIME / STAGE
P(); P('== 2. TIME / STAGE')
st = None
for r in hud:
    if r['hud_ok'] != '1' or '?' in r['stage']: continue
    if r['stage'] != st:
        P('STAGE %s -> %s tại f%s (%s) TIME %s' % (st, r['stage'], r['frame'], ts(int(r['frame'])), r['time'])); st = r['stage']
# TIME đứng
tm = [(int(r['frame']), r['time']) for r in hud if r['hud_ok'] == '1' and '?' not in r['time']]
runs = []
for f, t in tm:
    if runs and runs[-1][2] == t and f - runs[-1][1] < 6: runs[-1][1] = f
    else: runs.append([f, f, t])
P('TIME đứng yên > 80 frame video (>160 tick; bình thường 1 đơn vị TIME / ~64 tick ≈ 32 frame): ' + '; '.join('TIME %s f%d-f%d (%s-%s, %d tick)' % (t, a, b, ts(a), ts(b), 2 * (b - a)) for a, b, t in runs if b - a > 80))

# 3. cửa PHẢI
P(); P('== 3. Cửa PHẢI (vùng ghép cxw 512..768; cuộn tới 768 = cx 0 của vùng sau)')
cc = [(int(r['frame']), r['section'], int(r['cx']), int(r['cxw'])) for r in cam]
for J in ('WEB>WA', 'WB>MM'):
    fs = [x for x in cc if x[1] == J]
    a, b = fs[0][0], fs[-1][0]
    P('-- %s: f%d-f%d (%s-%s) = %d frame = %d tick; cxw %d -> %d' % (J, a, b, ts(a), ts(b), b - a + 1, 2 * (b - a + 1), fs[0][3], fs[-1][3]))
    # đoạn cuộn
    seg = []; cur = None
    last = None
    for f, s, cx, cxw in fs:
        if dup[f]: continue
        if last is not None:
            d = cxw - last[1]
            mv = d != 0
            if cur and cur[0] == mv: cur[2] = f; cur[4] = cxw
            else:
                if cur: seg.append(cur)
                cur = [mv, f, f, last[1], cxw]
        last = (f, cxw)
    seg.append(cur)
    for mv, f0, f1, c0, c1 in seg:
        if f1 - f0 < 3: continue
        if mv:
            xs = [(f, cxw) for f, s, cx, cxw in fs if f0 <= f <= f1 and not dup[f]]
            m, c, e = fit([x for x, y in xs], [y for x, y in xs])
            P('   cuộn f%d-f%d (%s-%s, %d tick) cxw %d -> %d: %.3f px/frame = %.3f px/tick (dư %.1f)' % (f0, f1, ts(f0), ts(f1), 2 * (f1 - f0), c0, c1, m, m / 2, e))
        else:
            P('   đứng f%d-f%d (%s-%s, %d tick) cxw=%d' % (f0, f1, ts(f0), ts(f1), 2 * (f1 - f0), c0))
    # Simon trong cửa
    sx = [(int(r['frame']), r['px'], r['Y'], r['sprite'], r['world_x']) for r in sim if r['found'] == '1' and a <= int(r['frame']) <= b]
    P('   Simon tìm thấy %d/%d frame; mẫu: %s' % (len(sx), b - a + 1, ' '.join('f%d:wx%s,Y%s,%s' % (f, w, y, s[:6]) for f, px, y, s, w in sx[::max(1, len(sx) // 14)])))

# 4. khoá phòng Mummy
P(); P('== 4. Khoá camera phòng Mummy (cx -> 1280)')
mmf = [x for x in cc if x[1] == 'MummyMan']
lock = next(x for x in mmf if x[2] >= 1280)
P('cx đạt 1280 lần đầu f%d (%s); cx trước đó: %s' % (lock[0], ts(lock[0]), ' '.join('f%d:%d' % (f, cx) for f, s, cx, cxw in mmf if lock[0] - 120 <= f <= lock[0] and f % 10 == 0)))
fs = [(f, cx) for f, s, cx, cxw in mmf if lock[0] - 70 <= f <= lock[0] and not dup[f]]
m, c, e = fit([a for a, b in fs], [b for a, b in fs])
P('tốc độ cx ~ %.3f px/frame = %.3f px/tick (70 frame cuối; dư %.1f)' % (m, m / 2, e))
sx = {int(r['frame']): r for r in sim if r['found'] == '1'}
P('Simon quanh khoá: ' + ' '.join('f%d:wx%s,Y%s,%s' % (f, sx[f]['world_x'], sx[f]['Y'], sx[f]['sprite'][:6]) for f in range(lock[0] - 60, lock[0] + 40, 6) if f in sx))

# 5. đổi tầng (thang)
P(); P('== 5. Đổi tầng bằng thang lên-trái (frame đen)')
for a, b in [(1187, 1197), (3263, 3274)]:
    pre = [f for f in range(a - 40, a) if f in sx]; post = [f for f in range(b + 1, b + 40) if f in sx]
    P('-- đen f%d-f%d (%s-%s, %d tick): trước %s | sau %s' % (a, b, ts(a), ts(b), 2 * (b - a + 1),
      ' '.join('f%d:cx%s,px%s,Y%s,%s' % (f, sx[f]['cx'], sx[f]['px'], sx[f]['Y'], sx[f]['sprite'][:9]) for f in pre[-8:]),
      ' '.join('f%d:cx%s,px%s,Y%s,%s' % (f, sx[f]['cx'], sx[f]['px'], sx[f]['Y'], sx[f]['sprite'][:9]) for f in post[:10])))
    cb = [r for r in cam if a - 3 <= int(r['frame']) <= b + 3]
    P('   camera: ' + ' '.join('f%s:%s/%s' % (r['frame'], r['section'][:6], r['cx']) for r in cb[::3]))

# 6. tally
P(); P('== 6. Tally Block 3')
H = {int(r['frame']): r for r in hud if r['hud_ok'] == '1'}
def first(pred, a, b):
    for f in range(a, b):
        if f in H and pred(H[f]): return f
P('Mummy1 chết (+3000): f%s ; Mummy2 chết (+3000): f%s' % (first(lambda r: r['score'] == '062970', 6000, 7000), first(lambda r: r['score'] == '065970', 6000, 7000)))
t0 = first(lambda r: r['time'] == '0304', 6900, 7100); t1 = first(lambda r: r['time'] == '0000', 6900, 7300)
h0 = first(lambda r: r['hearts'] == '45', 7100, 7300); h1 = first(lambda r: r['hearts'] == '00', 7300, 7500)
P('TIME 0304 (đầu đếm TIME) f%d (%s); TIME 0000 f%d (%s): %d frame = %d tick cho 304 đơn vị => %.2f tick/đơn vị' % (t0, ts(t0), t1, ts(t1), t1 - t0, 2 * (t1 - t0), 2 * (t1 - t0) / 304))
P('tim 46 -> 45 f%d (%s), tim 00 f%d (%s): %d frame = %d tick cho 46 tim => %.2f tick/tim' % (h0, ts(h0), h1, ts(h1), h1 - h0, 2 * (h1 - h0), 2 * (h1 - h0) / 46))
P('chờ sau TIME 0000 -> tim đầu: %d tick ; sau tim cuối -> đen f7453: %d tick ; đen %d tick' % (2 * (h0 - t1), 2 * (7453 - h1), 2 * 11))
open(OUT, 'w', encoding='utf-8').write('\n'.join(out))
