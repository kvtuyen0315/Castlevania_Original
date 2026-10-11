# segment_summary.py - tom tat HUD + camera cho cac doan (ranh gioi frame cho truoc) -> dung lap bang timeline
# Chay: python -P -E tools/segment_summary.py map1 f0 f1 f2 ...   (doan i = [f_i, f_(i+1)-1])
import sys, os, csv
video = sys.argv[1]
B = list(map(int, sys.argv[2:]))
base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
H = list(csv.DictReader(open(os.path.join(base, f'{video}_hud.csv'))))
C = list(csv.DictReader(open(os.path.join(base, f'{video}_camera.csv'))))
AB = {'CastleEntrance': 'CE', 'EntranceHall': 'EH', 'CryptA': 'CA', 'CryptB': 'CB', 'PhantomBat': 'PB', '-': '-'}
def ts(i): t = i / 30; return '%d:%04.1f' % (t // 60, t % 60)
def rng(vals):
    vals = [v for v in vals if v and '?' not in v]
    if not vals: return '-'
    a, b = vals[0], vals[-1]
    return a if a == b else f'{a}->{b}'
for a, b in zip(B[:-1], B[1:]):
    hs = [h for h in H[a:b] if h['hud_ok'] == '1']
    cs = [c for c in C[a:b] if c['section'] != '-' and float(c['score']) >= 0.8]
    secs = []
    for c in cs:
        s = AB[c['section']]
        if not secs or secs[-1][0] != s: secs.append([s, int(c['cx']), int(c['cx'])])
        else: secs[-1][1] = min(secs[-1][1], int(c['cx'])); secs[-1][2] = max(secs[-1][2], int(c['cx']))
    cam = ' '.join(f'{s}{lo}' + (f'..{hi}' if hi != lo else '') for s, lo, hi in secs) or '-'
    print(f"{ts(a)}-{ts(b-1)} (f{a}-{b-1}) | TIME {rng([h['time'] for h in hs])} | SC {rng([h['score'] for h in hs])} | tim {rng([h['hearts'] for h in hs])} | P {rng([h['p'] for h in hs])} | mau {rng([h['pbar'] for h in hs])} | E {rng([h['ebar'] for h in hs])} | sub {rng([h['sub'] for h in hs])} {rng([h['mult'] for h in hs])} | cam {cam}")
