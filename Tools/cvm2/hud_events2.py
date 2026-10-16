# hud_events2.py (CVM2 02, ban Map 1 doi duong dan; sub 'occ' = bi che -> bo qua)
# hud_events.py - liet ke moi lan HUD doi (tru TIME), loc nhieu 1-2 frame (chi tinh frame hud_ok=1)
# Chay: python -P -E Tools/cvm2/hud_events2.py map2 > ClaudeNotes/cvm2/_cache/02/work/map2_events_raw.txt
import sys, os, csv
video = sys.argv[1]
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
base = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
rows = [r for r in csv.DictReader(open(os.path.join(base, f'{video}_hud.csv'))) if r['hud_ok'] == '1']
F = ['score', 'stage', 'hearts', 'p', 'pbar', 'pempty', 'ebar', 'eempty', 'sub', 'mult']
def ts(i):
    t = int(i) / 30
    return '%d:%05.2f' % (t // 60, t % 60)
# stable value: require same value in 3 consecutive hud_ok frames
prev = None
for k in range(len(rows) - 2):
    r = rows[k]
    cur = tuple(r[f] for f in F)
    if any('?' in x for x in cur):
        continue
    if rows[k + 1][F[0]] != r[F[0]] or tuple(rows[k + 2][f] for f in F) != cur:
        continue
    if cur != prev:
        if prev is None:
            ch = 'start'
        else:
            ch = ' '.join(f'{f}:{a}->{b}' for f, a, b in zip(F, prev, cur) if a != b)
        print(f"f{r['frame']:>5} {ts(r['frame'])} TIME {r['time']} | {ch}")
        prev = cur
