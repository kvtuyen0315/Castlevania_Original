# -*- coding: utf-8 -*-
"""CVM3 03 - liet ke moi lan dung dong ho (Stopwatch) trong HUD video + nhip TIME quanh do.

Chay tu goc repo:  python -P -E Tools/cvm3/stopwatch3.py
Doc: ClaudeNotes/cvm2/ref/data/map2_hud.csv, boss2_hud.csv, ClaudeNotes/cvm3/ref/data/map3_hud.csv, boss3_hud.csv
     (+ ClaudeNotes/cvgp/ref/... neu co). In: moi lan tim giam 5 khi sub = dong ho, cac moc doi TIME truoc/sau.
tick = frame * TPF (video 30 fps, NES 60.0988 Hz => 2.0033 tick/frame).
"""
import csv, sys, os

TPF = 60.0988 / 30.0
SRC = [
    ('Map_2', 'ClaudeNotes/cvm2/ref/data/map2_hud.csv'),
    ('Boss_Map_2', 'ClaudeNotes/cvm2/ref/data/boss2_hud.csv'),
    ('Map_3', 'ClaudeNotes/cvm3/ref/data/map3_hud.csv'),
    ('Boss_Map_3', 'ClaudeNotes/cvm3/ref/data/boss3_hud.csv'),
]
WATCH = ('watch', 'stopwatch')


def load(p):
    rows = []
    with open(p, newline='') as f:
        for r in csv.DictReader(f):
            if r['hud_ok'] != '1':
                continue
            rows.append(r)
    return rows


def as_int(s):
    try:
        return int(s)
    except ValueError:
        return None


def main():
    for name, p in SRC:
        if not os.path.exists(p):
            print(name, 'missing', p)
            continue
        rows = load(p)
        # moc doi TIME (frame dau tien co gia tri moi, on dinh >= 2 frame)
        tchg = []
        prev = None
        for i, r in enumerate(rows):
            t = as_int(r['time'])
            if t is None:
                continue
            if prev is not None and t != prev:
                nxt = as_int(rows[i + 1]['time']) if i + 1 < len(rows) else t
                if nxt == t:
                    tchg.append((int(r['frame']), prev, t))
                    prev = t
                continue
            prev = t
        # tim giam 5
        uses = []
        prevh = None
        for i, r in enumerate(rows):
            h = as_int(r['hearts'])
            if h is None:
                continue
            if prevh is not None and h == prevh - 5:
                nxt = as_int(rows[i + 1]['hearts']) if i + 1 < len(rows) else h
                if nxt == h:
                    uses.append((int(r['frame']), r['sub'], prevh, h, r['time'], r['stage']))
            if prevh is None or h != prevh:
                nxt = as_int(rows[i + 1]['hearts']) if i + 1 < len(rows) else h
                if nxt == h:
                    prevh = h
        subs = sorted(set(r['sub'] for r in rows))
        print('==', name, 'frames', len(rows), 'subs', subs, 'TIME changes', len(tchg))
        for u in uses:
            f = u[0]
            before = [c for c in tchg if c[0] <= f][-3:]
            after = [c for c in tchg if c[0] > f][:3]
            print('  use f%d (%.1f s, tick %.1f) sub=%s hearts %d->%d TIME %s ST %s' % (f, f / 30.0, f * TPF, u[1], u[2], u[3], u[4], u[5]))
            print('     TIME before:', ['f%d %d->%d' % c for c in before])
            print('     TIME after :', ['f%d %d->%d' % c for c in after])
            if before and after:
                g = after[0][0] - before[-1][0]
                print('     gap last->next TIME change = %d f = %.1f tick (normal 64); throw-after-lastTIME = %.1f tick'
                      % (g, g * TPF, (f - before[-1][0]) * TPF))


if __name__ == '__main__':
    main()
