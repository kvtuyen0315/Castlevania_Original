"""CVM3 09 — bảng số đo Mummy Men từ vết (boss9m_track.py) 2 video ⇒ ClaudeNotes/cvm3/ref/data/boss9m/model.txt (+ chép vết commit).

Chạy (từ gốc repo, sau boss9m_fit.py + boss9m_track.py map3 / boss3):  python -P -E Tools/cvm3/boss9m_model.py
Mục:
  1. Khoá camera → thức (tick đầu Mummy dời/quay)            — Map_3 f5931 (cx 1280), Boss f374
  2. Tốc độ đi (hồi quy x theo tick trên đoạn đi sạch)        — đồng hồ frame × 2.0033
  3. Khung anim (chuỗi khung 0/1/2 + số frame mỗi khung)
  4. Tư thế ném: dừng → nháy đỏ/tím (từng frame, màu tại thân) → băng sinh → đi lại
  5. Băng gạc: vx hồi quy + sóng y = c + A·sin(2π(t − t0)/P) (P, A quét chung mọi băng; c, t0 từng băng) ⇒ cao/thấp, x sinh so Mummy
  6. Quay đầu (đổi lật) theo |dx| Simon — Map_3 Simon đứng yên trên khối (1302, 112) f6023–6763
  7. Lửa chết + điểm, quả cầu (hiện / đứng / rơi)
Toạ độ: x = gốc section MummyMan (= cx + x màn), y = ysec = pivot NES − 32 (Mummy/băng vẽ: Unity Y = ysec − viewOffset).
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C09 = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '09')
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'boss9m')
T = 2.0033
LOCK = {'m3': 5931, 'b3': 374}
WAKE_SEARCH = {'m3': (5935, 6005), 'b3': (378, 445)}
WALK = [('b3', 'L', 435, 494), ('m3', 'R', 5992, 6023), ('m3', 'L', 6027, 6051), ('m3', 'R', 6047, 6100)]
THROWS = [('m3', 'R', 1454, 6018, 6050), ('m3', 'L', 1326, 6048, 6080), ('m3', 'L', 1332, 6105, 6135),
          ('b3', 'R', 1516, 472, 504), ('b3', 'L', 1402, 490, 522)]
BANDAGES = {'m3': [0, 1, 2, 3, 4, 6, 7, 8, 9, 10], 'b3': [1, 2, 3, 4, 6, 7, 8]}
lines = []


def P(*a):
    s = ' '.join(str(x) for x in a)
    lines.append(s)
    print(s.encode('ascii', 'replace').decode())


def trk(v):
    return [r for r in csv.DictReader(open(os.path.join(C09, 'trk_mm_%s.csv' % v)))]


def bands(v):
    by = {}
    for r in csv.DictReader(open(os.path.join(C09, 'trk_bd_%s.csv' % v))):
        by.setdefault(int(r['id']), []).append((int(r['frame']), int(r['x']), int(r['y']), int(r['sprite'])))
    return by


def wake():
    P('## 1. Khoá camera → thức')
    for v in ('m3', 'b3'):
        rows = trk(v)
        a, b = WAKE_SEARCH[v]
        first = {}
        for who in 'LR':
            x0 = f0 = None
            for r in rows:
                f = int(r['frame'])
                if f < a or f > b or r['ok' + who] != '1':
                    continue
                if x0 is None:
                    x0, fl0 = int(r['x' + who]), r['flip' + who]
                    continue
                if int(r['x' + who]) != x0 or r['flip' + who] != fl0:
                    first[who] = (f, int(r['x' + who]) - x0)
                    break
        for who, (f, dx) in first.items():
            # lùi về tick bắt đầu dời: |dx| px ở 0.75 px/tick
            t = (f - LOCK[v]) * T - abs(dx) / 0.75
            P('  %s %s: khoá f%d, dời/quay đầu tiên f%d (dx %+d) ⇒ thức ≈ khoá + %.1f tick' % (v, who, LOCK[v], f, dx, t))


def speed():
    P('## 2. Tốc độ đi (hồi quy)')
    allv = []
    for v, who, a, b in WALK:
        pts = [(int(r['frame']), int(r['x' + who])) for r in trk(v) if a <= int(r['frame']) <= b and r['ok' + who] == '1']
        f = np.array([p[0] for p in pts]) * T
        x = np.array([p[1] for p in pts], float)
        k, c = np.polyfit(f, x, 1)
        rms = np.sqrt(np.mean((x - (k * f + c)) ** 2))
        allv.append(abs(k))
        P('  %s %s f%d-%d: %+.3f px/tick (rms %.2f, %d điểm)' % (v, who, a, b, k, rms, len(pts)))
    P('  TB %.3f px/tick' % np.mean(allv))


def anim():
    P('## 3. Khung anim (khung:frame đầu-cuối) trên đoạn đi sạch')
    for v, who, a, b in WALK:
        seq = [(int(r['frame']), int(r['i' + who])) for r in trk(v) if a <= int(r['frame']) <= b and r['ok' + who] == '1']
        out, cur = [], None
        for f, i in seq:
            if cur and cur[0] == i:
                cur[2] = f
            else:
                if cur:
                    out.append(cur)
                cur = [i, f, f]
        out.append(cur)
        P('  %s %s: %s' % (v, who, ' '.join('%d:%d-%d' % tuple(o) for o in out)))


def flash():
    P('## 4. Tư thế ném — màu thân từng frame (R đỏ / p tím / d frame lặp)')
    for v, who, x, a, b in THROWS:
        vid = 'map3' if v == 'm3' else 'boss3'
        mm = np.load(os.path.join(WORK, vid + '_nes.npy'), mmap_mode='r')
        dup = np.load(os.path.join(WORK, vid + '_dup.npy'))
        sx = x - 1280
        out = []
        for f in range(a, b + 1):
            reg = np.asarray(mm[f]).astype(int)[176 + 32 - 46 - 8:176 + 32 - 2 - 8, max(0, sx - 6):sx + 6]
            r, g, bb = reg[..., 0], reg[..., 1], reg[..., 2]
            if v == 'm3':
                red = ((r > 170) & (g < 40) & (bb < 40)).sum()
            else:
                red = ((r > 140) & (g < 60) & (bb > 20) & (bb < 130)).sum()
            pur = ((bb > 180) & (r > 150) & (g > 70)).sum()
            c = 'R' if red > pur and red > 15 else ('p' if pur > 15 else '?')
            out.append('%d%s%s' % (f, c, 'd' if dup[f] else ''))
        P('  %s %s x%d: %s' % (v, who, x, ' '.join(out)))


def bandage_fit():
    P('## 5. Băng gạc')
    for Pp, A in ((100, 24), (101, 24), (102, 24), (103, 24), (104, 24), (102, 23), (102, 25)):
        tot = []
        for v, ids in BANDAGES.items():
            by = bands(v)
            for k in ids:
                tot.append(fit_one(by[k], Pp, A)[0])
        P('  quét chung P %d A %d: rms TB %.3f' % (Pp, A, np.mean(tot)))
    for v, ids in BANDAGES.items():
        by = bands(v)
        rows = {int(r['frame']): r for r in trk(v)}
        for k in ids:
            r, t0, c, vx, x0, fs = fit_one(by[k], 102, 24)
            m = rows.get(int(round(fs))) or rows.get(int(round(fs)) - 1) or rows.get(int(round(fs)) + 1)
            thr = min((abs(float(m['x' + w]) - x0), w, float(m['x' + w])) for w in 'LR') if m else None
            kind = 'CAO' if c < 153 else 'THẤP'
            P('  %s B%-2d %2d điểm rms %.2f | sinh f%.1f x %.1f tâm y %.1f (%s) vx %+.3f | Mummy gần nhất %s x%.0f (x sinh − Mummy %+.1f)'
              % (v, k, len(by[k]), r, fs, x0, c, kind, vx, thr[1] if thr else '?', thr[2] if thr else -1, x0 - thr[2] if thr else 0))
        for k, pts in sorted(by.items()):
            s = [p[3] for p in pts]
            ch = [pts[i][0] for i in range(1, len(pts)) if s[i] != s[i - 1]]
            if len(ch) >= 4:
                P('  %s B%d đổi khung băng ở f %s ⇒ TB %.2f frame' % (v, k, ch, (ch[-1] - ch[0]) / (len(ch) - 1)))


def fit_one(pts, Pp, A):
    f = np.array([p[0] for p in pts])
    x = np.array([p[1] for p in pts], float)
    y = np.array([p[2] for p in pts], float)
    tt = (f - f[0]) * T
    best = None
    for t0 in np.arange(-40, 3, 0.25):
        s = np.sin(2 * np.pi * (tt - t0) / Pp) * A
        c = np.mean(y - s)
        r = np.sqrt(np.mean((s + c - y) ** 2))
        if best is None or r < best[0]:
            best = (r, t0, c)
    r, t0, c = best
    vx = np.polyfit(tt, x, 1)[0]
    return r, t0, c, vx, x[0] + vx * t0, f[0] + t0 / T


def turns():
    P('## 6. Quay đầu Map_3 (Simon đứng yên x 1302 trên khối, f6023–6763): |dx| lúc đổi lật')
    rows = [r for r in trk('m3') if 6023 <= int(r['frame']) <= 6763]
    for who in 'LR':
        last, ev = None, []
        for r in rows:
            if r['ok' + who] != '1':
                continue
            fl, x = r['flip' + who], int(r['x' + who])
            if last is not None and fl != last:
                ev.append('f%s %s %d' % (r['frame'], 'xa' if (fl == '1') == (x > 1302) else 'tới', abs(x - 1302)))
            last = fl
        P('  %s: %s' % (who, ', '.join(ev)))


def death_crystal():
    P('## 7. Lửa chết + quả cầu')
    for tag, lab in (('fx_m3', 'lửa chết Map_3'), ('cr_m3', 'quả cầu Map_3'), ('cr_b3', 'quả cầu Boss_Map_3')):
        p = os.path.join(C09, 'det_%s.csv' % tag)
        if not os.path.exists(p):
            continue
        rows = [r for r in csv.DictReader(open(p)) if float(r['err']) < (0.16 if tag.startswith('fx') else 0.12)]
        P('  %s: %s' % (lab, ' '.join('%s:%s,%s,%s' % (r['frame'], r['sprite'].split('_')[-1] if tag.startswith('cr') else r['sprite'][-3:],
                                                       r['worldx'], r['ysec']) for r in rows)))


def main():
    os.makedirs(OUT, exist_ok=True)
    wake()
    speed()
    anim()
    flash()
    bandage_fit()
    turns()
    death_crystal()
    with open(os.path.join(OUT, 'model.txt'), 'w', encoding='utf-8') as fo:
        fo.write('# CVM3 09 — Tools/cvm3/boss9m_model.py (vết _cache/09/trk_*). tick = frame × 2.0033\n')
        fo.write('\n'.join(lines) + '\n')
    for v in ('m3', 'b3'):
        for k in ('mm', 'bd'):
            src = os.path.join(C09, 'trk_%s_%s.csv' % (k, v))
            if os.path.exists(src):
                open(os.path.join(OUT, 'trk_%s_%s.csv' % (k, v)), 'w', newline='').write(open(src).read())
    print('ghi', OUT)


if __name__ == '__main__':
    main()
