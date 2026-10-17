# accept12_ledger.py (CVM2 12) - sinh "so diem" video Map_2 cho Map2AcceptProbe.Ledger tu ref/data/map2_events.txt (+ map2_hud.csv cho tally).
#   Moi su kien HUD co nghia (khong tinh ENEMY bar, TIME, nhieu "occ") => 1 dong: frame \t op \t arg \t score \t hearts \t lives \t sub \t mult
#   (gia tri HUD video SAU su kien; -1 / "-" = khong so). op: start, kill, boss, item, use, stage, time, tally, nextblock.
#   Nghia su kien = cot "nghia" cua map2_events (CVM2 02, hieu dinh CVM2 10: +700 f5148 = nhat II khi cam dong ho, khong phai tui trang).
# Chay (tu goc repo): python -P -E Tools/cvm2/accept12_ledger.py [ra]  (mac dinh ClaudeNotes/cvm2/_cache/12/ledger.tsv)
import os, re, sys, csv
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
EV = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_events.txt')
HUD = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_hud.csv')
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '12', 'ledger.tsv')

SUB = {'axe': 'Axe', 'cross': 'Cross', 'watch': 'Stopwatch', 'dagger': 'Dagger', 'holy': 'HolyWater', '': 'None'}
MULT = {'II': 2, 'III': 3, '': 1}

hud = {'score': 23370, 'hearts': 5, 'p': 3, 'sub': 'Axe', 'mult': 2, 'stage': 4}
rows = []
def emit(frame, op, arg, score=None, hearts=None, lives=None, sub=None, mult=None):
    rows.append((frame, op, arg,
                 hud['score'] if score is None else score, hud['hearts'] if hearts is None else hearts,
                 hud['p'] if lives is None else lives, hud['sub'] if sub is None else sub, hud['mult'] if mult is None else mult))

emit('f0', 'start', '3')   # roi cap 3 (cuoi Map_1)
unknown = []
for line in open(EV, encoding='utf-8'):
    if not line.startswith('f') or '|' not in line:
        continue
    parts = [p.strip() for p in line.split('|')]
    head, change, _where, mean = parts[0], parts[1], parts[2], parts[3] if len(parts) > 3 else ''
    frame = head.split()[0]
    fr = int(frame[1:])
    if change == 'start':
        continue
    ch = {}
    for kv in change.split():
        m = re.match(r'(\w+):(\S*)->(\S*)', kv)
        if m:
            ch[m.group(1)] = (m.group(2), m.group(3))
    used = [k for k in ch if k not in ('ebar', 'eempty')]
    if not used:
        continue
    low = mean.lower()
    if '(nhiễu)' in low:
        continue
    # cap nhat HUD sau su kien
    if 'score' in ch: hud['score'] = int(ch['score'][1])
    if 'hearts' in ch: hud['hearts'] = int(ch['hearts'][1])
    if 'p' in ch: hud['p'] = int(ch['p'][1])
    if 'sub' in ch and ch['sub'][1] != 'occ': hud['sub'] = SUB.get(ch['sub'][1], ch['sub'][1])
    if 'mult' in ch: hud['mult'] = MULT.get(ch['mult'][1], 1)
    if 'stage' in ch: hud['stage'] = int(ch['stage'][1])
    op = arg = None
    if 'stage' in ch and 'nextblock' not in low and hud['stage'] <= 6 and 'hearts' not in ch:
        op, arg = 'stage', str(hud['stage'])
    elif 'stage' in ch:
        op, arg = 'nextblock', ''
    elif 'spear knight' in low: op, arg = 'kill', 'SpearKnight'
    elif 'dơi treo' in low: op, arg = 'kill', 'Bat'
    elif 'medusa head' in low: op, arg = 'kill', 'MedusaHead'
    elif 'ghost' in low: op, arg = 'kill', 'Ghost'
    elif 'bone tower' in low: op, arg = 'kill', 'BoneTower'
    elif 'rắn của medusa' in low: op, arg = 'kill', 'Snake'
    elif 'queen medusa chết' in low: op, arg = 'boss', 'QueenMedusa'
    elif 'túi tiền đỏ' in low: op, arg = 'item', 'RedMoneyBag'
    elif 'túi tiền tím' in low: op, arg = 'item', 'PurpleMoneyBag'
    elif 'túi tiền trắng' in low: op, arg = 'item', 'DoubleShot' if fr == 5148 else 'WhiteMoneyBag'
    elif 'vương miện' in low: op, arg = 'item', 'Crown'
    elif 'rương báu' in low: op, arg = 'item', 'TreasureChest'
    elif 'tim nhỏ' in low: op, arg = 'item', 'SmallHeart'
    elif 'tim lớn' in low: op, arg = 'item', 'LargeHeart'
    elif 'dùng đồng hồ' in low: op, arg = 'use', 'Stopwatch'
    elif 'dùng vũ khí phụ' in low: op, arg = 'use', None
    elif 'nhặt thập tự' in low: op, arg = 'item', 'Cross'
    elif 'nhặt đồng hồ' in low: op, arg = 'item', 'Stopwatch'
    elif fr == 7380:
        op = 'tally'
    if op is None:
        unknown.append(frame + ' ' + change)
        continue
    if op == 'use' and arg is None:
        # vu khi dang cam TRUOC su kien (HUD sub khong doi khi nem)
        arg = hud['sub']
    if op == 'tally':
        # TIME luc nhat cau (map2_hud.csv f7140-7260 = 0210) roi tally toi xong (video SCORE 044670 f7550)
        t_at = None
        for r in csv.DictReader(open(HUD)):
            f = int(r['frame'])
            if 7140 <= f <= 7260 and r['hud_ok'] == '1':
                t_at = int(r['time'])
        # HUD da cap nhat score cua dong f7380 (giua tally) o tren => dong 'time' ky vong SCORE truoc tally
        emit('f7146', 'time', str(t_at), score=int(ch['score'][0]))
        emit('f7550', 'tally', '', score=44670, hearts=0)
        hud['score'], hud['hearts'] = 44670, 0
        continue
    emit(frame, op, arg or '')

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# frame\top\targ\tscore\thearts\tlives\tsub\tmult (HUD video SAU su kien) - accept12_ledger.py\n')
    for r in rows:
        f.write('\t'.join(str(x) for x in r) + '\n')
print('ledger: %d dong -> %s' % (len(rows), OUT))
print('khong phan loai: %d' % len(unknown))
for u in unknown:
    print('  ' + u)
