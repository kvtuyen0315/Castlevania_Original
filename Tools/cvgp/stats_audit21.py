# -*- coding: utf-8 -*-
"""CVGP 21 — doi chieu bang chi so Config (CSV) voi nguon.

Doc truc tiep ban van ban wiki luu o ClaudeNotes/cvgp/_cache/wiki_raw/text_*.txt (KHONG qua CV_WIKI_DATA) cho moi so ma
cot nguon ghi 'wiki:'; dem moi o so trong 7 bang (Enemy, Boss, Item, SubWeapon, Whip, Simon, Block) theo the nguon; liet ke
o 'cho do' / 'khong ro' con sot o CA 12 bang.

    python -P -E Tools/cvgp/stats_audit21.py [out.md]

In ra console chi ASCII (cp1252); bao cao day du (UTF-8) ghi vao out.md (mac dinh ClaudeNotes/cvgp/_cache/21/stats_audit.md).
"""
import csv, io, os, re, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
CSV_DIR = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv')
RAW = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'wiki_raw')
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '21', 'stats_audit.md')


def load(name):
    with io.open(os.path.join(CSV_DIR, name + '.csv'), encoding='utf-8') as f:
        return list(csv.DictReader(f))


def raw(name):
    with io.open(os.path.join(RAW, 'text_' + name + '.txt'), encoding='utf-8') as f:
        return f.read()


# ------------------------------------------------------------------ wiki goc (bang may doc duoc)
def wiki_enemies():
    t = raw('Enemies')
    res = {}
    for m in re.finditer(r'## ([^\n]+)\n.*?TABLE:\n((?:\|[^\n]*\n)+)', t, re.S):
        sec, tab = m.group(1).strip(), m.group(2)
        d = {}
        for row in tab.splitlines():
            c = [x.strip() for x in row.strip('|').split('|')]
            if len(c) >= 2:
                d[c[0]] = c[1]
        if 'Hits (Weak)' in d:
            res[sec] = d
    comp = {}
    m = re.search(r'## Comparison.*?TABLE:\n((?:\|[^\n]*\n)+)', t, re.S)
    for row in m.group(1).splitlines()[2:]:
        c = [x.strip() for x in row.strip('|').split('|')]
        comp[c[1].lstrip('#')] = (int(c[2]), int(c[3]), int(c[4]))
    return res, comp


def wiki_bosses():
    t = raw('Bosses')
    res = {}
    for m in re.finditer(r'## ([^\n]+)\n.*?TABLE:\n((?:\|[^\n]*\n)+)', t, re.S):
        sec, tab = m.group(1).strip(), m.group(2)
        d = {}
        for row in tab.splitlines():
            c = [x.strip() for x in row.strip('|').split('|')]
            if len(c) >= 2:
                d[c[0]] = c[1]
        if 'Hits (Weak/Strong)' in d:
            res[sec] = d
    return res


def wiki_damage():
    t = raw('Gameplay')
    m = re.search(r'\| Stages \| Damage \| Hits per Life \|\n\|---\|---\|---\|\n((?:\|[^\n]*\n)+)', t)
    out = {}
    for row in m.group(1).splitlines():
        c = [x.strip() for x in row.strip('|').split('|')]
        a, b = [int(x) for x in c[0].split('-')]
        for s in range(a, b + 1):
            out[s] = int(c[1])
    return out


ENEMY_WIKI = {  # id CSV -> muc wiki
    'Ghoul': 'Ghouls', 'Bat': 'Bats', 'Panther': 'Panthers', 'Raven': 'Ravens', 'Eagle': 'Eagles', 'Fishman': 'Fishmen',
    'MedusaHead': 'Medusa Heads', 'Skeleton': 'Skeletons', 'Ghost': 'Ghosts', 'RedSkeleton': 'Red Skeletons',
    'SpearKnight': 'Spear Knights', 'BoneTower': 'Bone Towers', 'Fleaman': 'Fleamen', 'BoneDragon': 'Bone Dragons',
    'AxeKnight': 'Axe Knights',
}
BOSS_WIKI = {'PhantomBat': 'Phantom Bat', 'QueenMedusa': 'Queen Medusa', 'MummyMan': 'Mummy Men', 'Frankenstein': 'Frankenstein & Igor',
             'Death': 'Death', 'Dracula': 'The Count'}

# item: (cot, gia tri, trang, cau trich phai co trong van ban goc)
ITEM_WIKI = [
    ('SmallHeart', 'hearts', '1', 'Items', 'Small hearts are worth one heart'),
    ('LargeHeart', 'hearts', '5', 'Items', 'Large hearts are worth five hearts'),
    ('RedMoneyBag', 'score', '100', 'Gameplay', '| Moneybags | 100, 400, or 700 points |'),
    ('PurpleMoneyBag', 'score', '400', 'Gameplay', '| Moneybags | 100, 400, or 700 points |'),
    ('WhiteMoneyBag', 'score', '700', 'Gameplay', '| Moneybags | 100, 400, or 700 points |'),
    ('FlashingMoneyBag', 'score', '1000', 'Secrets', 'money bags (1000 points)'),
    ('Crown', 'score', '2000', 'Secrets', 'crowns/treasures (2000 points)'),
    ('TreasureChest', 'score', '2000', 'Secrets', 'crowns/treasures (2000 points)'),
    ('Moai', 'score', '4000', 'Secrets', 'Moai figures (4000 points)'),
    ('PotRoast', 'healBars', '6', 'Items', 'restore 6 units of health'),
    ('SmallHeart', 'score', '0', 'Gameplay', 'Picking up hearts during a stage does not earn you points'),
    ('LargeHeart', 'score', '0', 'Gameplay', 'Picking up hearts during a stage does not earn you points'),
    ('MagicCrystal', 'healFull', '1', 'Gameplay', 'once you touch the Crystal Ball'),
]
SUB_WIKI = [
    ('Dagger', 'heartCost', '1', 'Items', 'Most secondary weapons consume one heart per use'),
    ('Axe', 'heartCost', '1', 'Items', 'Most secondary weapons consume one heart per use'),
    ('HolyWater', 'heartCost', '1', 'Items', 'Most secondary weapons consume one heart per use'),
    ('Cross', 'heartCost', '1', 'Items', 'Most secondary weapons consume one heart per use'),
    ('Stopwatch', 'heartCost', '5', 'Items', 'requires five hearts for each use'),
    ('Dagger', 'strength', 'Weak', 'Enemies', 'Holy Water, and Dagger are'),
    ('HolyWater', 'strength', 'Weak', 'Enemies', 'Holy Water, and Dagger are'),
    ('Axe', 'strength', 'Strong', 'Enemies', 'Boomerang, and Axe are'),
    ('Cross', 'strength', 'Strong', 'Enemies', 'Boomerang, and Axe are'),
    ('Dagger', 'pierces', '0', 'Items', 'cannot hit more than one t'),
]
WHIP_WIKI = [
    ('1', 'strength', 'Weak', 'Enemies', 'The Leather Whip, Holy Water, and D'),
    ('2', 'strength', 'Strong', 'Enemies', 'Chain Whip'),
    ('3', 'strength', 'Strong', 'Enemies', 'Chain Whip'),
    ('2', 'damageVsLeather', '1.5', 'Items', 'deals 1.5 times as much damage'),
]
SIMON_WIKI = [
    ('MaxHp', '16', 'Gameplay', 'You have sixteen bars of health'),
]
SCORE_WIKI = [
    ('OneUpFirst', '30000', 'Gameplay', 'reaches 30,000'), ('OneUpSecond', '80000', 'Gameplay', 'again at 80,000'),
    ('OneUpEvery', '50000', 'Gameplay', 'every 50,000 points'), ('TallyPointsPerTimeUnit', '10', 'Gameplay', '| Time | 10 points/second |'),
    ('TallyPointsPerHeart', '100', 'Gameplay', '| Hearts | 100 points/heart |'), ('MultiKill2', '1000', 'Gameplay', 'Destroying two enemies awards 1000 points'),
    ('MultiKill3', '2000', 'Gameplay', 'three awards 2000 point'), ('MultiKill4', '4000', 'Gameplay', '| Special | 1000, 2000, or 4000 |'),
    ('MultiplierPickupBonus', '700', 'Gameplay', '| Double Shot | 700 points |'), ('EnemyProjectilePoints', '100', 'Gameplay', 'some projectiles are worth 100 points'),
]

TAGS = ['wiki:', 'video:', 'đo:', 'đo :', 'suy luận', 'quy ước', 'CVTI', 'không rõ', 'chờ đo', 'n/a']


def tag_of(src):
    s = (src or '').strip()
    if not s:
        return '(trống)'
    low = s.lower()
    for t in ['chờ đo', 'không rõ', 'n/a']:
        if low.startswith(t):
            return t
    found = [t for t in ['wiki:', 'video:', 'đo:', 'suy luận', 'quy ước', 'CVTI'] if t.lower() in low]
    if not found:
        return 'khác'
    # the dau tien theo vi tri
    found.sort(key=lambda t: low.find(t.lower()))
    return found[0].rstrip(':')


def num(v):
    try:
        return float(v)
    except Exception:
        return None


def main():
    lines = []
    bad = []
    ok = 0

    def chk(what, got, want, src_note):
        nonlocal ok
        g, w = str(got).strip(), str(want).strip()
        same = g == w or (num(g) is not None and num(w) is not None and abs(num(g) - num(w)) < 1e-9)
        if same:
            ok += 1
            lines.append('| %s | %s | %s | ĐẠT | %s |' % (what, g, w, src_note))
        else:
            bad.append(what)
            lines.append('| %s | %s | %s | **SAI** | %s |' % (what, g, w, src_note))

    # ---- enemy
    ewiki, comp = wiki_enemies()
    enemies = load('ConfigEnemy')
    lines.append('## 1. ConfigEnemy ↔ wiki Enemies (bảng từng loại + bảng Comparison)\n')
    lines.append('| Ô | Config | Wiki gốc | Kết quả | Nguồn wiki |')
    lines.append('|---|---|---|---|---|')
    wiki_count = len(ewiki)
    normal = [r for r in enemies if r['kind'] == 'Normal']
    for r in enemies:
        sec = ENEMY_WIKI.get(r['id'])
        if not sec:
            continue
        d = ewiki[sec]
        cw = comp[sec]
        chk(r['id'] + '.hitsWeak', r['hitsWeak'], d['Hits (Weak)'], 'Enemies#' + sec)
        chk(r['id'] + '.hitsStrong', r['hitsStrong'], d['Hits (Strong)'], 'Enemies#' + sec)
        chk(r['id'] + '.score', r['score'], d['Points'], 'Enemies#' + sec)
        chk(r['id'] + '.Comparison(Pts/HP/HP2)', '%s/%s/%s' % (r['score'], r['hitsWeak'], r['hitsStrong']), '%d/%d/%d' % cw, 'Enemies#Comparison')
        st = []
        for x in d['Stages'].replace(' ', '').replace('*', '').split(','):   # '7-9' = khoang, '11*' = do Eagle mang toi
            if '-' in x:
                lo, hi = x.split('-')
                st += [str(k) for k in range(int(lo), int(hi) + 1)]
            else:
                st.append(x)
        st = ';'.join(st)
        chk(r['id'] + '.stages', r['stages'], st, 'Enemies#' + sec)
    lines.append('\nĐếm: wiki %d loại thường (+ Comparison %d dòng) — Config %d dòng Normal + %d dòng phụ (%s).\n'
                 % (wiki_count, len(comp), len(normal), len(enemies) - len(normal), ', '.join(r['id'] for r in enemies if r['kind'] != 'Normal')))
    missing = [s for s in ewiki if s not in ENEMY_WIKI.values()]
    if missing:
        bad.append('enemy thiếu: ' + ', '.join(missing))
        lines.append('**THIẾU trong Config:** ' + ', '.join(missing) + '\n')

    # ---- boss
    bwiki = wiki_bosses()
    bosses = load('ConfigBoss')
    lines.append('## 2. ConfigBoss ↔ wiki Bosses\n')
    lines.append('| Ô | Config | Wiki gốc | Kết quả | Nguồn wiki |')
    lines.append('|---|---|---|---|---|')
    for r in bosses:
        sec = BOSS_WIKI[r['id']]
        d = bwiki[sec]
        w, s = d['Hits (Weak/Strong)'].split('/')
        if r['id'] == 'Dracula' and r['phase'] == '2':
            # wiki Stages 16-18: dang 2 "32 hits" — dang 2 khong co dong rieng trong bang Bosses
            t = raw('Stages_16_-_18')
            m = re.search(r'[^.]*32[^.]*\.', t)
            chk('Dracula.2.hitsStrong', r['hitsStrong'], '32', 'Stages 16-18: "' + (m.group(0).strip()[:90] if m else '?') + '"')
            lines.append('| Dracula.2.hitsWeak | %s | (wiki không nêu) | suy luận 3:2 | %s |' % (r['hitsWeak'], r['src_hits'][:80]))
            pts = d['Points'].replace(',', '')
            chk('Dracula.2.score', r['score'], pts, 'Bosses#The Count (mâu thuẫn Gameplay "All bosses except Dracula earn you points" — §4)')
            continue
        chk(r['id'] + '.' + r['phase'] + '.hitsWeak', r['hitsWeak'], w, 'Bosses#' + sec)
        chk(r['id'] + '.' + r['phase'] + '.hitsStrong', r['hitsStrong'], s, 'Bosses#' + sec)
        pts = d['Points'].replace(',', '')
        if r['id'] == 'Dracula':
            lines.append('| Dracula.1.score | %s | %s (gán dạng 2) | quyết định §4 | Bosses#The Count |' % (r['score'] or '(trống)', pts))
        else:
            chk(r['id'] + '.' + r['phase'] + '.score', r['score'], pts, 'Bosses#' + sec)
    sw_ok = {'PhantomBat': '1', 'QueenMedusa': '1'}
    sw_text = raw('Bosses')
    for r in bosses:
        want = sw_ok.get(r['id'], '0')
        chk(r['id'] + '.' + r['phase'] + '.stopwatchAffects', r['stopwatchAffects'], want, 'Bosses ("Queen Medusa is the last boss that the Stopwatch will work against")')
    lines.append('\nĐếm: wiki %d boss — Config %d boss (%d dòng pha).\n' % (len(bwiki), len(set(r['id'] for r in bosses)), len(bosses)))

    # ---- damage / block
    dmg = wiki_damage()
    blocks = load('ConfigBlock')
    lines.append('## 3. ConfigBlock.damageToSimon ↔ wiki Gameplay#Damage\n')
    lines.append('| Ô | Config | Wiki gốc | Kết quả | Nguồn wiki |')
    lines.append('|---|---|---|---|---|')
    for r in blocks:
        for st in r['stages'].split(';'):
            chk('Block%s(stage %s).damageToSimon' % (r['block'], st), r['damageToSimon'], dmg[int(st)], 'Gameplay#Damage')

    # ---- item / subweapon / whip / simon / score (cau trich)
    def quote_rows(title, table, keycol, specs):
        rows = {r[keycol]: r for r in load(table)}
        lines.append('## %s\n' % title)
        lines.append('| Ô | Config | Wiki gốc | Kết quả | Câu trích (phải có trong văn bản gốc) |')
        lines.append('|---|---|---|---|---|')
        for spec in specs:
            rid, col, want, page, quote = spec
            text = raw(page)
            if quote not in text:
                bad.append('trích không thấy: ' + quote)
                lines.append('| %s.%s | %s | %s | **TRÍCH SAI** | %s: "%s" |' % (rid, col, rows[rid][col], want, page, quote))
                continue
            chk(rid + '.' + col, rows[rid][col], want, page + ': "' + quote + '"')

    quote_rows('4. ConfigItem ↔ wiki Items/Gameplay/Secrets', 'ConfigItem', 'id', ITEM_WIKI)
    quote_rows('5. ConfigSubWeapon ↔ wiki Items/Enemies', 'ConfigSubWeapon', 'id', SUB_WIKI)
    quote_rows('6. ConfigWhip ↔ wiki Items/Enemies', 'ConfigWhip', 'level', WHIP_WIKI)
    simon = {r['key']: r for r in load('ConfigSimon')}
    score = {r['key']: r for r in load('ConfigScore')}
    lines.append('## 7. ConfigSimon / ConfigScore ↔ wiki Gameplay\n')
    lines.append('| Ô | Config | Wiki gốc | Kết quả | Câu trích |')
    lines.append('|---|---|---|---|---|')
    for k, want, page, q in SIMON_WIKI:
        assert q in raw(page), q
        chk('Simon.' + k, simon[k]['value'], want, page + ': "' + q + '"')
    for k, want, page, q in SCORE_WIKI:
        if q not in raw(page):
            bad.append('trích không thấy: ' + q)
        chk('Score.' + k, score[k]['value'], want, page + ': "' + q + '"')

    # ---- moi o so cua 7 bang theo the nguon
    lines.append('\n## 8. Mọi ô của 7 bảng theo thẻ nguồn (100 % dòng)\n')
    lines.append('| Bảng | Dòng | Ô số có giá trị | wiki | video | đo | suy luận | quy ước | CVTI | khác | Ô trống/“-” có lý do |')
    lines.append('|---|---|---|---|---|---|---|---|---|---|---|')
    totals = {}
    for table, keycol in [('ConfigEnemy', 'id'), ('ConfigBoss', 'id'), ('ConfigItem', 'id'), ('ConfigSubWeapon', 'id'), ('ConfigWhip', 'level'),
                          ('ConfigSimon', 'key'), ('ConfigBlock', 'block')]:
        rows = load(table)
        cnt = {}
        cells = empty = 0
        for r in rows:
            for col, v in r.items():
                if col in ('source', keycol) or col.startswith('src_') or col in ('min', 'max'):
                    continue
                if num(v) is None and v.strip() not in ('', '-'):
                    continue
                src = r.get('src_' + col, '') or r.get('source', '')
                if v.strip() in ('', '-'):
                    empty += 1
                    continue
                cells += 1
                t = tag_of(src)
                cnt[t] = cnt.get(t, 0) + 1
        totals[table] = (len(rows), cells, cnt, empty)
        lines.append('| %s | %d | %d | %d | %d | %d | %d | %d | %d | %d | %d |' % (
            table, len(rows), cells, cnt.get('wiki', 0), cnt.get('video', 0), cnt.get('đo', 0), cnt.get('suy luận', 0), cnt.get('quy ước', 0),
            cnt.get('CVTI', 0), sum(v for k, v in cnt.items() if k not in ('wiki', 'video', 'đo', 'suy luận', 'quy ước', 'CVTI')), empty))

    # ---- o cho do / khong ro con sot (ca 12 bang)
    lines.append('\n## 9. Ô "chờ đo" / "không rõ" / "chưa" còn sót (12 bảng)\n')
    pend = []
    for fn in sorted(os.listdir(CSV_DIR)):
        if not fn.endswith('.csv'):
            continue
        rows = load(fn[:-4])
        for r in rows:
            if 'actor' in r:
                rid = r['actor'] + '.' + r.get('key', '')
            else:
                rid = r.get('id') or r.get('key') or r.get('level') or r.get('objectId') or (r.get('stage') if 'stage' in r and 'areaShort' in r else r.get('block'))
            for col, v in r.items():
                if v is None:
                    continue
                low = v.lower()
                if 'chờ đo' in low or low.startswith('không rõ') or ('chưa' in low and col.startswith('src_')):
                    pend.append((fn[:-4], str(rid), col, v.strip()[:110]))
    lines.append('| Bảng | Dòng | Cột | Nội dung |')
    lines.append('|---|---|---|---|')
    for p in pend:
        lines.append('| %s | %s | %s | %s |' % p)
    waits = [p for p in pend if 'chờ đo' in p[3].lower()]

    head = ['# CVGP 21 — đối chiếu bảng chỉ số Config ↔ nguồn (sinh bởi `Tools/cvgp/stats_audit21.py`)\n',
            '**Tóm tắt:** %d ô đối chiếu với văn bản wiki gốc: ĐẠT %d, SAI %d. Ô "chờ đo" còn sót: %d; ô "không rõ"/"chưa" (có lý do): %d.\n'
            % (ok + len(bad), ok, len(bad), len(waits), len(pend) - len(waits))]
    if bad:
        head.append('**SAI:** ' + '; '.join(bad) + '\n')
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with io.open(OUT, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(head + lines) + '\n')
    print('checked %d, ok %d, bad %d, pending(cho do) %d, unknown %d -> %s' % (ok + len(bad), ok, len(bad), len(waits), len(pend) - len(waits), OUT))
    for b in bad:
        print('BAD:', b.encode('ascii', 'replace').decode())
    for t, (n, c, cnt, e) in totals.items():
        print(t, 'rows', n, 'cells', c, 'empty', e, {k.encode('ascii', 'replace').decode(): v for k, v in cnt.items()})


if __name__ == '__main__':
    main()
