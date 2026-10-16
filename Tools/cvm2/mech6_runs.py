# mech6_runs.py (CVM2 06) - doi chuoi khung bay ep tung frame (crusher_<ten>.csv) thanh cac doan (k, frame dau, so tick)
#   tick = frame * 2.0033; bo frame lap (dup); frame khop kem (margin < 1.5 — Simon/vat khac de len) bo qua (giu doan cu)
# Chay: python -P -E Tools/cvm2/mech6_runs.py <ten> [<ten2> ...]
import sys, os, csv
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06')
TPF = 2.0033
for n in sys.argv[1:]:
    rows = list(csv.DictReader(open(os.path.join(D, 'crusher_%s.csv' % n))))
    for i in (1, 2, 3):
        seq = []
        for r in rows:
            if int(r['dup']): continue
            k = int(r['k%d' % i]); m = float(r['m%d' % i])
            if k < 0 or m < 1.5: continue
            seq.append((int(r['frame']), k))
        if not seq: continue
        runs = []
        for f, k in seq:
            if runs and runs[-1][0] == k: runs[-1][2] = f
            else: runs.append([k, f, f])
        print('== %s bay %d (%d frame)' % (n, i, len(seq)))
        out = []
        for j, (k, a, b) in enumerate(runs):
            nxt = runs[j + 1][1] if j + 1 < len(runs) else None
            dur = '%5.1f' % ((nxt - a) * TPF) if nxt is not None else '    ?'
            out.append('k%d@%d:%s' % (k, a, dur.strip()))
        print('  ' + '  '.join(out))
