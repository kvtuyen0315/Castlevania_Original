# kb4_holy_boss1.py (CVM3 04) - lo nuoc thanh video Boss_Map_1 (nen xam, bang mau rieng): bam lo bang mau hieu chuan (f470),
# lua bang mau hong/do tren nen xam (cot pixel lua trong 12 hang tren mat san 208). So voi mo hinh kb4_holy_fit (vy0 -1.5, g 33/256, sinh Y-19).
# Chay: python -P -E Tools/cvm3/kb4_holy_boss1.py  -> ClaudeNotes/cvm3/_cache/04/holy/boss1_fit.txt
import sys, os, math
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K
import kb4_holy as H

PAL = {(252, 216, 168): (223, 220, 194), (0, 112, 236): (82, 96, 160), (0, 0, 0): (27, 17, 19)}   # mau video tai f470 (lo nem trai)
THROWS = [182, 238, 250, 326, 356, 380, 398, 459, 471, 530, 545]
FLOOR = 208
G = 33 / 256.0


def model(X0, Y0, face, n):
    x = math.floor(X0 + face * 1.5 * n + 1e-9)
    y = math.floor(Y0 - 1.5 * n + G * n * (n + 1) / 2.0 + 1e-9)
    return x, y


def main():
    sprites = K.load_sprites()
    out = []
    for f0 in THROWS:
        # Simon luc nem (khop mau quanh vung phai man)
        best = None
        for f, img, nes in K.read_frames('boss1', f0, f0 + 6):
            b = K.fit(nes, sprites, K.SHEET_COLORS, 215, 208, rx=40, ry=4)
            if b is not None and (best is None or b['err'] < best['err']):
                best = b
        spx, sY, face = best['px'], best['Y'], (1 if best['flip'] else -1)
        bt = H.load('holywater_bottle', 1 if face < 0 else 0)
        col = np.array([PAL[tuple(int(v) for v in c)] for c in bt['col']], float)
        rows = []
        prev = None
        for f, img, nes in K.read_frames('boss1', f0 + 6, f0 + 34):
            dup = K.is_dup(nes, prev); prev = nes
            # lo: tim quanh quy dao du doan (n ~ 2*rel - 16.6), +-6 px
            rel = f - f0
            n_g = int(round(2 * rel - 16.6))
            gx, gy = model(spx, sY - 19, face, max(0, n_g))
            x0, x1 = max(-40, min(250, gx - 10)), max(-30, min(260, gx + 10)); y0, y1 = max(10, min(220, gy - 10)), max(20, min(230, gy + 10))
            if x1 <= x0 + 1:
                x0, x1 = 0, 20
            e = H.errmap(nes, bt, x0 - 4, x1 - 4, y0 - 8, y1 - 8, col=col)
            iy, ix = np.unravel_index(np.argmin(e), e.shape)
            bx, by, be = x0 + ix, y0 + iy, float(e[iy, ix])
            # lua: cot co pixel hong/do trong hang FLOOR-12..FLOOR-1, x trong +-24 quanh du doan diem roi
            reg = nes[FLOOR - 12:FLOOR, :]
            r, g, b2 = reg[..., 0], reg[..., 1], reg[..., 2]
            m = ((r > 190) & (g < 150) & (b2 > 110)) | ((r > 150) & (g < 80) & (b2 < 80))
            xs = np.nonzero(m.any(0))[0]
            lx, ln = model(spx, sY - 19, face, 32)
            xs = xs[(xs > lx - 24) & (xs < lx + 24)]
            rows.append((f, rel, int(dup), bx, by, be, (int(xs.min()), int(xs.max())) if len(xs) else None))
        out.append((f0, spx, sY, face, best['sprite'], rows))
    lines = []
    for f0, spx, sY, face, spr, rows in out:
        lines.append('== nem f%d Simon (%d,%d) huong %+d %s' % (f0, spx, sY, face, spr))
        for f, rel, dup, bx, by, be, fire in rows:
            # n theo x mo hinh
            ns = [n for n in range(0, 60) if model(spx, sY - 19, face, n)[0] == bx]
            fits = ['n%d y%d' % (n, model(spx, sY - 19, face, n)[1]) for n in ns]
            lines.append('  f%d rel %d%s lo (%d,%d) err %.3f %s | lua cot %s' % (f, rel, '*' if dup else '', bx, by, be, ' '.join(fits) if be < 0.11 else '', fire))
    p = os.path.join(K.CACHE, 'holy', 'boss1_fit.txt')
    open(p, 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    main()
