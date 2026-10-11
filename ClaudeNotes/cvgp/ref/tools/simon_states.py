# simon_states.py - phan loai trang thai Simon tung frame (Map_1) va liet ke cua so cho moi hanh vi
# Chay: python -P -E tools/simon_states.py map1
# Dau vao: work/map1_simon.csv, map1_whip.csv, map1_camera.csv
# Trang thai: STAND/WALK (y chan = nen, cao >= 24), CROUCH (y chan = nen, cao 17..22), AIR (y chan < nen-3, ngoai cau thang),
#             STAIR (y chan doi deu +-0.5 px/tick cung world_x), WHIP (ext roi >= 14 px) ket hop voi trang thai tren.
# Nen (ground) = y chan cua frame dung gan nhat (cao >= 24) trong +-20 frame co x_world on dinh.
import sys, os, csv
import numpy as np
video = sys.argv[1]
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
S = list(csv.DictReader(open(os.path.join(base, f'{video}_simon.csv'))))
W = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_whip.csv')))}
N = len(S)
found = np.array([r['found'] == '1' for r in S])
y0 = np.array([int(r['y0']) if r['found'] == '1' else -1 for r in S])
y1 = np.array([int(r['y1']) if r['found'] == '1' else -1 for r in S])
h = y1 - y0
wx = np.array([float(r['world_x']) if r['found'] == '1' and r['world_x'] else np.nan for r in S])
sec = [r['section'] for r in S]
ext = np.array([max(int(W[i]['ext_l']), int(W[i]['ext_r'])) if W[i]['ext_l'] != '' else 0 for i in range(N)])
# ground estimate: y1 of standing frames (h>=24) ; propagate nearest within 20 frames
stand = found & (h >= 24)
ground = np.full(N, -1)
last = -1; lastf = -999
for i in range(N):
    if stand[i]: last = y1[i]; lastf = i
    if i - lastf <= 20: ground[i] = last
nxt = -1; nxtf = 10 ** 9
g2 = np.full(N, -1)
for i in range(N - 1, -1, -1):
    if stand[i]: nxt = y1[i]; nxtf = i
    if nxtf - i <= 20: g2[i] = nxt
# stairs: windows found by fit (see measure_stairs) -> here simple: |dy1| over 16 frames ~ 8 px and |dwx| ~ 8 px
stair = np.zeros(N, bool)
for i in range(8, N - 8):
    if found[i - 8] and found[i + 8] and not np.isnan(wx[i - 8]) and not np.isnan(wx[i + 8]):
        dy = y1[i + 8] - y1[i - 8]; dx = wx[i + 8] - wx[i - 8]
        if 5 <= abs(dy) <= 12 and 5 <= abs(dx) <= 12 and abs(abs(dy) - abs(dx)) <= 4 and h[i] >= 18:
            stair[i] = True
state = []
for i in range(N):
    if not found[i]: state.append('-'); continue
    g = ground[i] if ground[i] > 0 else g2[i]
    if stair[i]: s = 'STAIR'
    elif g > 0 and y1[i] < min(g, g2[i] if g2[i] > 0 else g) - 3 and not stair[i]: s = 'AIR'
    elif 17 <= h[i] <= 22 and g > 0 and abs(y1[i] - g) <= 1: s = 'CROUCH'
    else: s = 'GROUND'
    if ext[i] >= 14: s += '+WHIP'
    state.append(s)
with open(os.path.join(base, f'{video}_states.csv'), 'w', newline='') as fh:
    w = csv.writer(fh); w.writerow(['frame', 't', 'state', 'y0', 'y1', 'h', 'ext'])
    for i in range(N): w.writerow([i, f'{i/30:.3f}', state[i], y0[i], y1[i], h[i], ext[i]])
def ts(i): t = i / 30; return '%d:%04.1f' % (t // 60, t % 60)
def windows(pred, minlen, maxgap=2, label=''):
    out = []; i = 0
    while i < N:
        if pred(i):
            j = i; gap = 0; k = i
            while k < N and gap <= maxgap:
                if pred(k): j = k; gap = 0
                else: gap += 1
                k += 1
            if j - i + 1 >= minlen: out.append((i, j))
            i = j + 1
        else: i += 1
    print(f'{label}: {len(out)} cua so')
    print('   ' + ', '.join(f'{ts(a)}-{ts(b)} (f{a}-{b})' for a, b in out[:40]))
    return out
windows(lambda i: state[i] == 'CROUCH', 5, 2, 'NGOI (khong roi)')
windows(lambda i: state[i] == 'CROUCH+WHIP', 2, 1, 'ROI KHI NGOI')
windows(lambda i: state[i] == 'AIR+WHIP', 2, 1, 'ROI TREN KHONG')
windows(lambda i: state[i] == 'STAIR+WHIP', 2, 1, 'ROI TREN CAU THANG')
windows(lambda i: state[i] == 'GROUND+WHIP', 2, 1, 'ROI DUNG')
