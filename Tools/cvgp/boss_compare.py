"""CVGP 14 — so quỹ đạo Phantom Bat Unity (BossProbe.SaveTraces ⇒ _cache/14/traces/trace_wake_video.csv) với vết video
(boss_fit.py fly ⇒ _cache/14/fly_{boss,map1}.csv) theo thời gian TƯƠNG ĐỐI kể từ lúc thức.
Mốc: Unity tick thức 17 ↔ video boss f65.5 / Map_1 f5983.5 (khung cánh đầu f66 / f5984). Toạ độ video = tâm sprite màn + cx 512,
y section = y màn − 32. Mỗi frame video MỚI (đã khớp mẫu, err < 0.125) ↔ tick Unity gần nhất. In RMS / max |dx|, |dy| theo pha:
lượn tại chỗ (0–34), lao đầu (34–90), lượn chậm (90–180: trước khi Unity tới đích NGẪU NHIÊN).
In thêm dx/dy từng frame pha lao đầu (_cache/14/compare_dive.txt).
Chạy (từ gốc repo): python -P -E Tools/cvgp/boss_compare.py
"""
import csv, os, math

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
C14 = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '14')
TPF = 60.0988 / 30
WAKE_U = 17
VIDEOS = {'boss': (65.5, 157), 'map1': (5983.5, 6128)}   # mốc thức, frame video hết lượn chậm đầu
PHASES = [('lượn tại chỗ', 0, 34), ('lao đầu', 34, 90), ('lượn chậm', 90, 180)]   # Unity tới đích lượn (ngẫu nhiên) sau ~183


def main():
    tr = {}
    for r in csv.DictReader(open(os.path.join(C14, 'traces', 'trace_wake_video.csv'), encoding='utf-8')):
        tr[int(r['rel'])] = (int(r['bx']), int(r['by']))
    out, dive = [], []
    for video, (f0, fend) in VIDEOS.items():
        rows = [r for r in csv.DictReader(open(os.path.join(C14, 'fly_%s.csv' % video))) if r['found'] == '1' and float(r['err']) < 0.125]
        for name, a, b in PHASES:
            ex, ey, n = [], [], 0
            for r in rows:
                f = int(r['frame'])
                if f > fend:
                    continue
                rel = (f - f0) * TPF
                if not a <= rel < b:
                    continue
                u = tr.get(int(round(rel)) + WAKE_U)
                if u is None:
                    continue
                vx, vy = 512 + int(r['pivx']), int(r['pivy']) - 32
                ex.append(u[0] - vx)
                ey.append(u[1] - vy)
                if name == 'lao đầu':
                    dive.append('%s f%d rel %.1f video (%d,%d) Unity (%d,%d) d (%d,%d)' % (video, f, rel, vx, vy, u[0], u[1], u[0] - vx, u[1] - vy))
            if not ex:
                continue
            rms = lambda v: math.sqrt(sum(x * x for x in v) / len(v))
            line = '%-5s %-14s n %3d  dx rms %5.2f max %3d | dy rms %5.2f max %3d' % (video, name, len(ex), rms(ex), max(abs(x) for x in ex), rms(ey), max(abs(y) for y in ey))
            out.append(line)
            print(line.encode('ascii', 'replace').decode())
    with open(os.path.join(C14, 'compare.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(out) + '\n')
    with open(os.path.join(C14, 'compare_dive.txt'), 'w', encoding='utf-8') as fo:
        fo.write('\n'.join(dive) + '\n')


if __name__ == '__main__':
    main()
