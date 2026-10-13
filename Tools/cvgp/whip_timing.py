"""CVGP 09 — đo nhịp khung roi từ kết quả khớp mẫu sprite (simon_fit.py) quanh các cú roi của video Map_1.

Chạy (từ gốc repo), sau khi đã có ClaudeNotes/cvgp/_cache/08/fit_w09_all.csv:
  python -P -E Tools/cvgp/simon_fit.py fit map1 w09_all <cửa sổ…>      (≈ 30 phút — cửa sổ in ở whip_windows())
  python -P -E Tools/cvgp/whip_timing.py
Mỗi cú roi = chuỗi khung thân use_<tư thế>_0 → _1 → _2 rồi khung khác. Mốc của 1 pha = frame video MỚI (dup = 0) đầu tiên
thấy khung đó (mỗi frame video mới ≈ thời điểm game ±1 tick — CVGP_MEMORY §2g); thời lượng pha = hiệu 2 mốc × 2 tick.
Chỉ tính cú roi đủ 3 pha có mốc trước/sau rõ (khung trước và sau cú roi là frame mới). In thống kê + từng cú roi.
"""
import csv, os, re, statistics
from collections import Counter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
FIT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '08', 'fit_w09_all.csv')
ATT = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work', 'whip_attacks.txt')
ERR_MAX = 0.10


def whip_windows():
    fs = []
    for l in open(ATT, encoding='utf-8', errors='replace'):
        m = re.match(r'\s+f(\d+)-(\d+)', l)
        if m:
            fs.append((int(m.group(1)), int(m.group(2))))
    win = []
    for a, b in fs:
        if 3226 <= a <= 3502:        # non-game + thanh YouTube
            continue
        a0, b0 = a - 10, b + 12
        if win and a0 <= win[-1][1]:
            win[-1][1] = max(win[-1][1], b0)
        else:
            win.append([a0, b0])
    return win


def pose_of(s):
    m = re.match(r'use_(stand|duck|stairs_up|stairs_down)_(\d)', s or '')
    return (m.group(1), int(m.group(2))) if m else (None, None)


def main():
    rows = [r for r in csv.DictReader(open(FIT))]
    for r in rows:
        r['frame'] = int(r['frame'])
        r['ok'] = r['err'] not in ('', None) and float(r['err']) <= ERR_MAX
    by = {r['frame']: r for r in rows}
    # chuỗi frame MỚI (bỏ frame lặp) có khớp tốt
    seq = [r for r in rows if r['ok'] and r['dup'] == '0']
    whips = []
    i = 0
    while i < len(seq):
        pose, k = pose_of(seq[i]['sprite'])
        if pose is None or k != 0:
            i += 1
            continue
        # khung trước phải là frame mới liền trước (không phải use_*) để mốc bắt đầu rõ
        prev = seq[i - 1] if i > 0 else None
        if prev is None or pose_of(prev['sprite'])[0] is not None or seq[i]['frame'] - prev['frame'] > 3:
            i += 1
            continue
        marks = {0: seq[i]['frame']}
        ys = [int(seq[i]['Y'])]
        j = i + 1
        ok = True
        cur = 0
        while j < len(seq):
            p2, k2 = pose_of(seq[j]['sprite'])
            if seq[j]['frame'] - seq[j - 1]['frame'] > 3:
                ok = False
                break
            if p2 is None:
                marks[3] = seq[j]['frame']
                break
            if k2 < cur:
                ok = False
                break
            if k2 > cur:
                if k2 != cur + 1:
                    ok = False
                    break
                marks[k2] = seq[j]['frame']
                cur = k2
            ys.append(int(seq[j]['Y']))
            j += 1
        if ok and len(marks) == 4:
            d = [2 * (marks[k + 1] - marks[k]) for k in range(3)]
            air = max(ys) - min(ys) > 2
            whips.append(dict(f=marks[0], pose=pose, d=d, air=air, nxt=seq[j]['sprite'], y0=ys[0], ys=(min(ys), max(ys))))
        i = j if j > i else i + 1
    print('so cu roi du 3 pha:', len(whips))
    for w in whips:
        print('  f%-5d %5.2fs %-12s %s d=%s tong=%d  sau=%s Y %d..%d' % (
            w['f'], w['f'] / 30.0, w['pose'], 'KHONG' if w['air'] else 'dat', w['d'], sum(w['d']), w['nxt'], w['ys'][0], w['ys'][1]))
    for name, sel in (('dat', lambda w: not w['air']), ('khong', lambda w: w['air']), ('tat ca', lambda w: True)):
        ws = [w for w in whips if sel(w)]
        if not ws:
            continue
        print('\n== %s (n=%d) ==' % (name, len(ws)))
        for k in range(3):
            v = [w['d'][k] for w in ws]
            print('  pha %d: TB %.2f  trung vi %.1f  min %d max %d  phan bo %s' % (
                k, statistics.mean(v), statistics.median(v), min(v), max(v), sorted(Counter(v).items())))
        t = [sum(w['d']) for w in ws]
        print('  tong: TB %.2f trung vi %.1f min %d max %d' % (statistics.mean(t), statistics.median(t), min(t), max(t)))
        p01 = [w['d'][0] + w['d'][1] for w in ws]
        print('  pha0+1: TB %.2f trung vi %.1f' % (statistics.mean(p01), statistics.median(p01)))
    # khoảng cách giữa 2 cú roi liền nhau (bắt đầu → bắt đầu)
    st = sorted(w['f'] for w in whips)
    gaps = [2 * (b - a) for a, b in zip(st, st[1:]) if b - a < 20]
    print('\nkhoang bat dau->bat dau < 40 tick:', sorted(gaps))


if __name__ == '__main__':
    main()
