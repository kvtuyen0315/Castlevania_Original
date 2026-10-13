"""CVGP 12 — đo luật HUD từ CSV đọc HUD từng frame của planner (ClaudeNotes/cvgp/_cache/video/work/{map1,boss}_hud.csv).

Chạy từ gốc repo:  python -P -E Tools/cvgp/hud_rules.py
Video 30 fps container ⇒ 1 frame video = 2 tick NES (60 Hz); capture thật ~21 khung/s ⇒ mốc đơn lẻ ±1 frame (±2 tick).
Đo: (1) nhịp TIME (hồi quy mốc đổi giá trị trên các đoạn chạy liền), (2) các đoạn TIME đứng, (3) pha sau khi TIME chạy lại,
(4) tally: tốc độ đếm TIME/tim, khoảng nghỉ, chờ trước/sau, (5) nhịp thanh máu hiển thị (E giảm, P hồi ở video boss),
(6) icon vũ khí phụ / multiplier theo thời gian.  In ASCII (console cp1252).
"""
import csv
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work')
TPF = 2  # tick / frame video
NONGAME = (3226, 3502)  # f non-game Map_1 (CVGP_MEMORY §2a) + thanh YouTube che đáy


def load(name):
    rows = []
    with open(os.path.join(WORK, name), newline='') as f:
        for r in csv.DictReader(f):
            r['frame'] = int(r['frame'])
            r['ok'] = r['hud_ok'] == '1' and r['time'].isdigit()
            rows.append(r)
    return rows


def changes(rows, key, lo=0, hi=10 ** 9):
    """Danh sách (frame, giá trị mới) mỗi lần cột key đổi (bỏ frame HUD không đọc được)."""
    out, prev = [], None
    for r in rows:
        if not r['ok'] or not (lo <= r['frame'] <= hi):
            continue
        v = r[key]
        if v != prev:
            out.append((r['frame'], v))
            prev = v
    return out


def linfit(xs, ys):
    n = len(xs)
    mx, my = sum(xs) / n, sum(ys) / n
    sxx = sum((x - mx) ** 2 for x in xs)
    sxy = sum((x - mx) * (y - my) for x, y in zip(xs, ys))
    b = sxy / sxx
    a = my - b * mx
    res = [y - (a + b * x) for x, y in zip(xs, ys)]
    return a, b, max(abs(e) for e in res)


def time_rate(rows):
    print('== (1)(2) TIME: nhip dem + doan dung (Map_1) ==')
    ch = [(f, int(v)) for f, v in changes(rows, 'time') if not (NONGAME[0] <= f <= NONGAME[1])]
    # tách đoạn: TIME giảm đúng 1 mỗi mốc, khoảng cách < 50 frame
    segs, cur = [], [ch[0]]
    holds = []
    for a, b in zip(ch, ch[1:]):
        gap = b[0] - a[0]
        if b[1] == a[1] - 1 and gap <= 40:
            cur.append(b)
        else:
            segs.append(cur)
            if b[1] == a[1] - 1:
                holds.append((a[0], b[0], a[1]))
            cur = [b]
    segs.append(cur)
    tot_units, tot_ticks = 0, 0
    allx, ally = [], []
    for s in segs:
        if len(s) < 6:
            continue
        xs = [f * TPF for f, _ in s]
        ys = [v for _, v in s]
        a, b, mx = linfit(ys, xs)  # tick theo giá trị TIME
        units = ys[0] - ys[-1]
        print('  doan f%d-%d TIME %04d->%04d: %d don vi, %.2f tick/dv (du max %.1f tick)' % (s[0][0], s[-1][0], ys[0], ys[-1], units, -b, mx))
        tot_units += units
        tot_ticks += xs[-1] - xs[0]
    print('  TONG: %d don vi / %d tick = %.3f tick/dv' % (tot_units, tot_ticks, tot_ticks / float(tot_units)))
    print('  TIME dung (khoang giua 2 moc giam > 40 frame):')
    for a, b, v in holds:
        print('    TIME %04d tu f%d (%.2fs) toi f%d (%.2fs) = %d tick' % (v, a, a / 30.0, b, b / 30.0, (b - a) * TPF))
    return ch


def phase_after(rows, ch, period):
    print('== (3) pha: tick tu moc bat dau doan toi lan giam dau ==')
    # mốc stage hiện (HUD đọc lại sau màn đen) và lần giảm đầu
    for lo, hi, label in [(60, 200, 'Stage 01 bat dau (HUD hien f67 sau den)'),
                          (590, 700, 'Entrance Hall sau cong (den f591-599)'),
                          (4425, 4520, 'CB->CA sau den 0.33 s'),
                          (2740, 2900, 'sau cua Stage 01->02 (STAGE 02 f2756)'),
                          (5110, 5260, 'sau cua Stage 02->03 (STAGE 03 f5126)')]:
        sub = [(f, v) for f, v in ch if lo <= f <= hi]
        st = changes(rows, 'stage', lo, hi)
        okf = [r['frame'] for r in rows if r['ok'] and lo <= r['frame'] <= hi]
        if not sub or not okf:
            continue
        print('  %s: HUD doc duoc tu f%d, cac moc TIME %s' % (label, okf[0], sub[:3]))


def tally(rows):
    print('== (4) tally (Map_1 3:33-3:52) ==')
    tch = changes(rows, 'time', 6370, 6600)
    hch = changes(rows, 'hearts', 6370, 6960)
    sch = changes(rows, 'score', 6370, 6960)
    print('  TIME doi:', tch[:3], '...', tch[-2:])
    t0 = [f for f, v in tch if v == '0127'][0]
    tend = [f for f, v in tch if v == '0000'][0]
    first = [f for f, v in tch if int(v) < 127][0]
    units = 127
    print('  TIME 0127 tu f%d; giam dau f%d; ve 0000 f%d => %d dv trong %d tick (%.2f tick/dv)' % (
        t0, first, tend, units, (tend - first) * TPF + TPF, ((tend - first) * TPF + TPF) / float(units)))
    hs = [(f, int(v)) for f, v in hch if v.isdigit()]
    h_first = [f for f, v in hs if v < 72][0]
    h_end = [f for f, v in hs if v == 0][0]
    xs = [f * TPF for f, v in hs if 0 <= v < 72]
    ys = [v for f, v in hs if 0 <= v < 72]
    a, b, mx = linfit(ys, xs)
    print('  tim: 72 -> 0, giam dau f%d, ve 00 f%d; hoi quy %.3f tick/tim (du max %.1f tick)' % (h_first, h_end, -b, mx))
    print('  nghi giua TIME=0 (f%d) va tim giam dau (f%d) = %d tick' % (tend, h_first, (h_first - tend) * TPF))
    sc = [(f, v) for f, v in sch]
    print('  SCORE:', sc[0], '... sau dem TIME', [x for x in sc if x[0] <= tend + 2][-1], '... cuoi', sc[-1])
    dark = [r['frame'] for r in rows if not r['ok'] and 6890 <= r['frame'] <= 6975]
    print('  den (HUD khong doc) tu f%d toi f%d; cho sau tim=0: %d tick; den %d tick' % (dark[0], dark[-1], (dark[0] - h_end) * TPF, (dark[-1] - dark[0] + 1) * TPF))
    return t0, first


def bars(rows, boss):
    print('== (5) thanh mau hien thi ==')
    ech = [(f, int(v)) for f, v in changes(rows, 'ebar', 6100, 6330)]
    steps = [(a, b) for a, b in zip(ech, ech[1:]) if b[1] == a[1] - 1]
    pairs = [b[0] - a[0] for a, b in steps if b[0] - a[0] <= 8]
    print('  Map_1 ENEMY: moc', ech)
    print('  khoang cach 2 buoc trong cung 1 don (frame):', pairs, '=> TB %.2f frame = %.1f tick' % (sum(pairs) / float(len(pairs)), TPF * sum(pairs) / float(len(pairs))))
    pch = [(f, int(v)) for f, v in changes(boss, 'pbar') if v.isdigit()]
    print('  Boss PLAYER: moc', pch)
    heal = [(f, v) for f, v in pch if f > 500]
    if len(heal) > 3:
        xs = [f * TPF for f, v in heal]
        ys = [v for f, v in heal]
        a, b, mx = linfit(ys, xs)
        print('  hoi mau sau qua cau: %.2f tick/vach (du max %.1f tick), %s' % (b, mx, heal))
    tch = changes(boss, 'time', 600, 10 ** 9)
    print('  Boss TIME sau qua cau:', tch)


def subweapon(rows):
    print('== (6) vu khi phu / multiplier ==')
    print('  sub:', changes(rows, 'sub'))
    mc = changes(rows, 'mult')
    print('  mult (%d moc):' % len(mc), mc[:40])


def main():
    m = load('map1_hud.csv')
    b = load('boss_hud.csv')
    ch = time_rate(m)
    phase_after(m, ch, 64)
    tally(m)
    bars(m, b)
    subweapon(m)
    print('== video boss: moc HUD ==')
    print('  hearts:', changes(b, 'hearts')[:5], ' score:', changes(b, 'score'))


if __name__ == '__main__':
    main()
