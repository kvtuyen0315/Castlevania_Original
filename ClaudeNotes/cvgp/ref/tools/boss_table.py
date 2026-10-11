# boss_table.py - bang moc 0.5 s cho video boss (hoac doan boss Map_1): vi tri doi, tu the canh, Simon, HUD
# Chay: python -P -E tools/boss_table.py boss 0 25.0      |  python -P -E tools/boss_table.py map1 197.5 211
# Cot: t | doi (x,y tam; rong bbox -> 'dang'=48 / 'khep'=32) | Simon (x, y chan) | tim | mau P | mau E | diem
import sys, os, csv
import numpy as np
video, t0, t1 = sys.argv[1], float(sys.argv[2]), float(sys.argv[3])
step = float(sys.argv[4]) if len(sys.argv) > 4 else 0.5
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
B = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_bat.csv')))}
# loc nhieu: bo diem lech > 25 px so voi trung vi 7 diem tim duoc lan can (lua nuoc thanh / item bi nham la doi)
_fs = sorted(f for f, r in B.items() if r['found'] == '1')
for k, f in enumerate(_fs):
    nb = _fs[max(0, k - 3):k + 4]
    mx = np.median([float(B[g]['xc']) for g in nb]); my = np.median([float(B[g]['yc']) for g in nb])
    if abs(float(B[f]['xc']) - mx) > 25 or abs(float(B[f]['yc']) - my) > 25:
        B[f] = dict(B[f], found='0')
sim_csv = 'boss_simon2.csv' if video == 'boss' else 'map1_simon.csv'
S = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, sim_csv)))}
H = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(base, f'{video}_hud.csv')))}
def near(D, f, key='found'):
    for d in (0, 1, -1, 2, -2, 3, -3):
        r = D.get(f + d)
        if r and r.get(key) == '1': return r
    return None
print('| t | doi (x,y) | canh | Simon (x, y chan) | tim | P | E | diem |')
print('|---|---|---|---|---|---|---|---|')
t = t0
while t <= t1 + 1e-6:
    f = int(round(t * 30))
    b = near(B, f); s = near(S, f); h = H.get(f, {})
    bs = f"({float(b['xc']):.0f},{float(b['yc']):.0f})" if b else '-'
    wing = ('dang' if int(b['wing']) >= 40 else 'khep') if b else ''
    ss = f"({float(s['x_med']):.0f},{s['y1']})" if s else '-'
    tl = f'{int(t//60)}:{t%60:04.1f}' if video == 'map1' else f'{t:4.1f}s'
    print(f"| {tl} | {bs} | {wing} | {ss} | {h.get('hearts','')} | {h.get('pbar','')} | {h.get('ebar','')} | {h.get('score','')} |")
    t += step
