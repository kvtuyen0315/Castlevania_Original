# -*- coding: utf-8 -*-
"""accept12_ledger.py (CVM3 12) - sinh "so diem" video Map_3 cho Map3AcceptProbe.Ledger.

Nguon: ClaudeNotes/cvm3/ref/data/map3_events.txt (CVM3 02, su kien HUD tung frame), dinh chinh nghia theo CVM3 07/08/09/10
(ref/data/drops3.tsv = loai enemy tung lan ha + 11 lan pha cau lua Bone Tower; MAP3_PLACEMENT = noi dung nen), map3_hud.csv (TIME luc nhat cau,
tach f5221 / f5223). Moi su kien HUD co nghia => 1 dong: frame \t op \t arg \t score \t hearts \t lives \t sub \t mult (HUD video SAU su kien;
-1 / "-" = khong so). op xem Map3AcceptProbe.Ledger. Luat CHUA DUNG (Skeleton chet trong lua nuoc thanh => chu "1000", CVM3 §5 #12) => dong
op "known" ghi rieng (khong tinh la khop).

Chay (tu goc repo): python -P -E Tools/cvm3/accept12_ledger.py [ra]   (mac dinh ClaudeNotes/cvm3/_cache/12/ledger.tsv)
"""
import csv, os, re, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
REF = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
EV = os.path.join(REF, 'map3_events.txt')
HUD = os.path.join(REF, 'map3_hud.csv')
DROPS = os.path.join(REF, 'drops3.tsv')
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '12', 'ledger.tsv')

SUB = {'axe': 'Axe', 'cross': 'Cross', 'watch': 'Stopwatch', 'dagger': 'Dagger', 'holy': 'HolyWater', '': 'None', 'none': 'None'}
MULT = {'II': 2, 'III': 3, '': 1}
ENEMY = {'Ghost': 'Ghost', 'Fleaman': 'Fleaman', 'Skeleton': 'Skeleton', 'Raven': 'Raven', 'BoneTower': 'BoneTower', 'MedusaHead': 'MedusaHead'}

# loai enemy tung lan ha + lan pha cau lua (CVM3 08)
kills, shots, holykill = {}, set(), set()
for r in csv.DictReader((l for l in open(DROPS, encoding='utf-8') if not l.startswith('#')), delimiter='\t'):
    if r['video'] != 'Map_3':
        continue
    f = int(r['frame'])
    if r['kind'] == 'shot':
        shots.add(f)
    elif r['kind'] == 'kill':
        kills[f] = r['enemy']
        if r['weapon'] == 'nước thánh':
            holykill.add(f)

hud_rows = {}
for r in csv.DictReader(open(HUD, encoding='utf-8')):
    if r['hud_ok'] == '1':
        hud_rows[int(r['frame'])] = r

hud = {'score': 44670, 'hearts': 5, 'p': 4, 'sub': 'Stopwatch', 'mult': 1, 'stage': 7}
rows = []
unknown = []


def emit(frame, op, arg, score=None, hearts=None, lives=None, sub=None, mult=None, note=''):
    rows.append((frame, op, arg,
                 hud['score'] if score is None else score, hud['hearts'] if hearts is None else hearts,
                 hud['p'] if lives is None else lives, hud['sub'] if sub is None else sub, hud['mult'] if mult is None else mult, note))


emit('f207', 'start', '3')   # roi cap 3 (cuoi Map_2), dong ho, tim 05, P-04 (map3_hud.csv f207)

for line in open(EV, encoding='utf-8'):
    if not line.startswith('f') or '|' not in line:
        continue
    parts = [p.strip() for p in line.split('|')]
    frame, change, mean = parts[0], parts[5], parts[6] if len(parts) > 6 else ''
    fr = int(frame[1:])
    ch = {}
    for m in re.finditer(r'(score|hearts|stage|sub|mult|p|ebar) (\S*)->(\S*)', change):
        ch[m.group(1)] = (m.group(2), m.group(3))
    used = [k for k in ch if k != 'ebar']
    if not used:
        continue
    if fr in (6917, 6920):
        continue   # icon II nhap nhay ngay sau khi nhat (f6903 da ghi II)
    old = dict(hud)
    if 'score' in ch: hud['score'] = int(ch['score'][1])
    if 'hearts' in ch: hud['hearts'] = int(ch['hearts'][1])
    if 'sub' in ch: hud['sub'] = SUB.get(ch['sub'][1], ch['sub'][1])
    if 'mult' in ch: hud['mult'] = MULT.get(ch['mult'][1], 1)
    if 'stage' in ch: hud['stage'] = int(ch['stage'][1])
    ds = hud['score'] - old['score']
    dh = hud['hearts'] - old['hearts']
    if fr == 7186:
        # tally: TIME luc nhat cau (f6920 HUD 0306) roi dem toi xong (video SCORE 073630 f7393, tim 00)
        t_at = int(hud_rows[6920]['time'])
        emit('f6920', 'time', str(t_at), score=old['score'], hearts=old['hearts'])
        emit('f7393', 'tally', '', score=73630, hearts=0)
        hud['score'], hud['hearts'] = 73630, 0
        continue
    if 7186 < fr < 7400:
        continue   # cac buoc dem tim cua tally (da gop vao dong tally)
    if 'stage' in ch:
        if hud['stage'] == 10:
            emit(frame, 'nextblock', '')
        else:
            emit(frame, 'stage', str(hud['stage']))
        continue
    if 'sub' in ch and 'score' not in ch:
        emit(frame, 'item', SUB.get(ch['sub'][1], ch['sub'][1]))
        continue
    if 'mult' in ch:
        emit(frame, 'item', 'DoubleShot' if hud['mult'] == 2 else 'TripleShot')
        continue
    if 'hearts' in ch and 'score' not in ch:
        if dh == 1: emit(frame, 'item', 'SmallHeart')
        elif dh == 5: emit(frame, 'item', 'LargeHeart')
        elif dh == -1: emit(frame, 'use', old['sub'])
        else: unknown.append(frame + ' ' + change)
        continue
    # diem
    if fr == 5223:
        # f5221 +100 (nguon chua ro - cung luc lua nuoc thanh ha Skeleton S5; Unity xuong KHONG co hop nhan don) + f5223 Skeleton +300 + "1000"
        s5221 = int(hud_rows[5221]['score'])
        emit('f5221', 'known', '100', score=s5221, note='+100 f5221 chua ro nguon (luc lua nuoc thanh: co the xuong bi lua pha - Unity xuong khong pha duoc, CVM3 §6 07)')
        emit(frame, 'kill', 'Skeleton', score=-1)
        emit(frame, 'known', str(hud['score'] - s5221 - 300), note='Skeleton chet trong lua nuoc thanh => chu "1000" (CVM3 §5 #12)')
        continue
    if fr in holykill:
        emit(frame, 'kill', kills[fr], score=-1)
        emit(frame, 'known', str(ds - 300), note='Skeleton chet trong lua nuoc thanh => chu "1000" (CVM3 §5 #12)')
        continue
    if fr == 5511:
        emit(frame, 'kill', 'BoneTower', score=-1)   # +500 = thap 400 + cau lua 100 cung nhat roi (CVM3 08)
        emit(frame, 'shot', '')
        continue
    if fr in kills:
        emit(frame, 'kill', ENEMY[kills[fr]])
        continue
    if fr in shots:
        emit(frame, 'shot', '')
        continue
    if 'Mummy' in mean and ds == 3000:
        emit(frame, 'boss', 'MummyMan')
        continue
    if ds == 100: emit(frame, 'item', 'RedMoneyBag')
    elif ds == 400: emit(frame, 'item', 'PurpleMoneyBag')
    elif ds == 700: emit(frame, 'item', 'WhiteMoneyBag')
    elif ds == 1000: emit(frame, 'item', 'FlashingMoneyBag')
    else:
        unknown.append(frame + ' ' + change + ' ' + mean)

os.makedirs(os.path.dirname(OUT), exist_ok=True)
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('# frame\top\targ\tscore\thearts\tlives\tsub\tmult\tghi chu (HUD video SAU su kien) - Tools/cvm3/accept12_ledger.py\n')
    for r in rows:
        f.write('\t'.join(str(x) for x in r) + '\n')
print('ledger: %d dong -> %s' % (len(rows), OUT))
print('khong phan loai: %d' % len(unknown))
for u in unknown:
    print('  ' + u.encode('ascii', 'replace').decode())
