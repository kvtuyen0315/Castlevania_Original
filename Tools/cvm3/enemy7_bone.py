"""CVM3 07 — khớp quỹ đạo từng vết XƯƠNG (trk_<tag>.csv của enemy7_track.py): x tuyến tính, y bậc 2 theo tick
(tick = frame × 2.0033) ⇒ vx, vy lúc t0, g, đỉnh (y nhỏ nhất của parabol), rms dư; thêm ước lượng tick sinh nếu cho Y tay (yhand).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy7_bone.py <tag> [g_fixed=0] [min_n=6]
  g_fixed > 0: khớp lại với g cố định (chỉ y0, vy0) ⇒ so rms hai mô hình.
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '07')
TPF = 2.0033


def tracks(tag):
    out = {}
    for r in csv.DictReader(open(os.path.join(D, 'trk_%s.csv' % tag))):
        out.setdefault(int(r['track']), []).append((int(r['frame']), int(r['worldx']), int(r['pivy']), r['section']))
    return out


def main():
    tag = sys.argv[1]
    gfix = float(sys.argv[2]) if len(sys.argv) > 2 else 0.0
    minn = int(sys.argv[3]) if len(sys.argv) > 3 else 6
    for k, t in sorted(tracks(tag).items()):
        if len(t) < minn:
            continue
        f = np.array([p[0] for p in t], float); x = np.array([p[1] for p in t], float); y = np.array([p[2] for p in t], float)
        tk = (f - f[0]) * TPF
        ax, bx = np.polyfit(tk, x, 1)
        rx = np.sqrt(((x - (ax * tk + bx)) ** 2).mean())
        c2, c1, c0 = np.polyfit(tk, y, 2)
        ry = np.sqrt(((y - (c2 * tk * tk + c1 * tk + c0)) ** 2).mean())
        g = 2 * c2
        tap = -c1 / g if g else 0
        yap = c0 + c1 * tap + c2 * tap * tap
        s = '#%d %s f%d-%d n%d vx %.3f (rms %.2f) | g %.4f vy(t0) %.3f apex y %.1f @tick %.1f (rms %.2f) | y0 %.0f yend %.0f' % (
            k, t[0][3], f[0], f[-1], len(t), ax, rx, g, c1, yap, tap, ry, y[0], y[-1])
        if gfix > 0:
            A = np.vstack([tk, np.ones_like(tk)]).T
            sol, _, _, _ = np.linalg.lstsq(A, y - 0.5 * gfix * tk * tk, rcond=None)
            r2 = np.sqrt(((y - (0.5 * gfix * tk * tk + sol[0] * tk + sol[1])) ** 2).mean())
            tap2 = -sol[0] / gfix
            s += ' | g=%.4f: vy %.3f apex %.1f @%.1f rms %.2f' % (gfix, sol[0], sol[1] + sol[0] * tap2 + 0.5 * gfix * tap2 * tap2, tap2, r2)
        print(s)


if __name__ == '__main__':
    main()
