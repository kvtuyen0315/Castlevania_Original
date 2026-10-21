# kb4_hashdiff.py (CVM3 04) - so hash TUNG kich ban giua 2 lan chay RegressionRunner (dong "== <ten>: N tick, hash X, FAIL k" trong suites/<bo>.txt)
# Chay: python -P -E Tools/cvm3/kb4_hashdiff.py <thu muc cu>[,<thu muc cu 2>...] <thu muc moi> [bo,...]
#   thu muc = .../suites ; nhieu thu muc cu: file sau ghi de file truoc (vd _cache/03/arena + arena3 cho b46/b56)
import sys, os, re, glob

olds = sys.argv[1].split(','); new = sys.argv[2]
only = sys.argv[3].split(',') if len(sys.argv) > 3 else None
PAT = re.compile(r'^== ([^:]+): (\d+) tick, hash ([0-9a-f]{8})')
PAT2 = re.compile(r'^\[(?:ĐẠT|FAIL|LỖI)\] (\S+) — (\d+) tick, hash ([0-9a-f]{8})')
PAT3 = re.compile(r'^\[([A-Z]\d+ \S+)\] (\d+) tick, .*?hash ([0-9a-f]{8})')


def load(d):
    res = {}
    for p in glob.glob(os.path.join(d, '*.txt')):
        b = os.path.basename(p)[:-4]
        if b in ('SUMMARY', 'progress'):
            continue
        sc = {}
        for l in open(p, encoding='utf-8'):
            m = PAT.match(l) or PAT2.match(l) or PAT3.match(l)
            if m:
                sc[m.group(1)] = (int(m.group(2)), m.group(3))
        if sc:
            res[b] = sc
    return res


old = {}
for d in olds:
    old.update(load(d))
nw = load(new)
for b in sorted(nw):
    if only and b not in only:
        continue
    o = old.get(b, {})
    ch = [k for k in nw[b] if k in o and o[k][1] != nw[b][k][1]]
    same = [k for k in nw[b] if k in o and o[k][1] == nw[b][k][1]]
    newk = [k for k in nw[b] if k not in o]
    print('%s: %d kich ban, doi %d, giu %d, moi %d' % (b, len(nw[b]), len(ch), len(same), len(newk)))
    print('   DOI : ' + ', '.join(ch))
    print('   GIU : ' + ', '.join(same))
    if newk:
        print('   MOI : ' + ', '.join(newk))
