"""CVGP 07 — đo luật camera bám Simon từ dữ liệu theo dõi của planner (Map_1.mp4).
Đầu vào: ClaudeNotes/cvgp/_cache/video/work/map1_simon.csv + map1_camera.csv (planner sinh — mỗi frame video:
  cx camera dò ZNCC theo section, x_med/x0/x1 Simon trên màn NES). Chạy: python -P -E Tools/cvgp/measure_camera.py
In ra: phân bố x màn của Simon khi camera đang cuộn phải/trái, x khi camera đứng giữa section, phản ứng khi đổi hướng,
và các đoạn camera ĐỨNG giữa biên (dò vùng chết: Simon tịnh tiến mà camera không theo).
Diễn giải (CVGP 07): x_med = trung vị cột pixel cam/kem của Simon; trên sprite idle/walk x_med = pivot − 2 khi quay trái,
pivot + 1 khi quay phải (đo trên PNG) ⇒ cuộn phải x_med 129 / cuộn trái 126 đều là pivot 128.
"""
import csv, os, sys
from collections import Counter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work')
NONGAME = (3226, 3502)  # frame video non-game + thanh YouTube (MAP1_TIMELINE §3)
WIDTH = {'CastleEntrance': 768, 'EntranceHall': 1536, 'CryptA': 512, 'CryptB': 512, 'PhantomBat': 768}

def load():
    rows = list(csv.DictReader(open(os.path.join(WORK, 'map1_simon.csv'))))
    out = []
    for r in rows:
        f = int(r['frame'])
        if NONGAME[0] <= f <= NONGAME[1] or not r['section']:
            out.append(None); continue
        out.append(dict(f=f, t=float(r['t']), found=r['found'] == '1', xm=float(r['x_med']) if r['x_med'] else None,
                        x0=int(r['x0']) if r['x0'] else None, x1=int(r['x1']) if r['x1'] else None,
                        sec=r['section'], cx=int(r['cx'])))
    return out

def main():
    rows = load()
    # Gom các frame video DUY NHẤT (cx hoặc Simon đổi) để bỏ frame lặp của capture.
    right, left, still_mid = Counter(), Counter(), Counter()
    right_c, left_c = Counter(), Counter()
    samples = []
    prev = None
    for r in rows:
        if r is None or not r['found']:
            prev = None; continue
        if prev is not None and prev['sec'] == r['sec']:
            d = r['cx'] - prev['cx']
            w = r['x1'] - r['x0'] + 1
            narrow = w <= 17   # không roi/không vật chồng: tâm hộp = tâm thân
            cmid = (r['x0'] + r['x1'] + 1) / 2.0 if narrow else None
            maxcx = WIDTH[r['sec']] - 256
            if 0 < d <= 6:
                right[round(r['xm'] * 2) / 2] += 1
                if narrow: right_c[cmid] += 1
                samples.append((r['f'], 'R', r['cx'], r['xm'], cmid))
            elif -6 <= d < 0:
                left[round(r['xm'] * 2) / 2] += 1
                if narrow: left_c[cmid] += 1
                samples.append((r['f'], 'L', r['cx'], r['xm'], cmid))
            elif d == 0 and 0 < r['cx'] < maxcx and r['cx'] != 512:
                still_mid[round(r['xm'])] += 1
        prev = r
    def show(name, c):
        n = sum(c.values())
        print(name, 'n=%d' % n, sorted(c.items(), key=lambda kv: -kv[1])[:10])
    show('cuon PHAI x_med', right)
    show('cuon PHAI tam hop (rong<=17)', right_c)
    show('cuon TRAI x_med', left)
    show('cuon TRAI tam hop (rong<=17)', left_c)
    show('camera DUNG giua bien x_med (top)', still_mid)
    # Đổi hướng cuộn: tìm chỗ cx đổi chiều trong cùng section, in x Simon quanh đó.
    print('--- doi chieu cuon (f, cx, xm) ---')
    lastdir, lastf = 0, None
    seq = [r for r in rows if r is not None and r['found']]
    for i in range(1, len(seq)):
        a, b = seq[i - 1], seq[i]
        if a['sec'] != b['sec']:
            lastdir = 0; continue
        d = b['cx'] - a['cx']
        if d == 0 or abs(d) > 6:
            continue
        s = 1 if d > 0 else -1
        if lastdir and s != lastdir:
            ctx = [(x['f'], x['cx'], x['xm']) for x in seq[max(0, i - 6):i + 4]]
            print('f%d dir %+d -> %+d :' % (b['f'], lastdir, s), ctx)
        lastdir = s

def dead_zone():
    """Đoạn cx không đổi (giữa biên, ≥ 4 frame video): x_med cuối − đầu. Vùng chết thật ⇒ nhiều đoạn trôi đều 1 hướng."""
    rows = load()
    runs, cur = [], None
    for r in rows:
        if r is None or not r['found']:
            if cur:
                runs.append(cur)
            cur = None
            continue
        if cur and cur['sec'] == r['sec'] and cur['cx'] == r['cx']:
            cur['xs'].append(r['xm'])
            cur['f1'] = r['f']
        else:
            if cur:
                runs.append(cur)
            cur = dict(sec=r['sec'], cx=r['cx'], xs=[r['xm']], f0=r['f'], f1=r['f'])
    if cur:
        runs.append(cur)
    mid = [r for r in runs if 0 < r['cx'] < WIDTH[r['sec']] - 256 and r['cx'] != 512 and len(r['xs']) >= 4]
    drift = Counter(round(r['xs'][-1] - r['xs'][0]) for r in mid)
    print('--- doan camera dung giua bien (>=4 frame video):', len(mid))
    print('x_med cuoi-dau:', sorted(drift.items()))
    for r in mid:
        d = r['xs'][-1] - r['xs'][0]
        if abs(d) >= 5:
            print('  troi', d, 'f%d-%d' % (r['f0'], r['f1']), r['sec'], 'cx', r['cx'], r['xs'][0], '->', r['xs'][-1])


if __name__ == '__main__':
    main()
    dead_zone()
