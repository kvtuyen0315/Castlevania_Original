# kb4_holy.py (CVM3 04) - quy dao lo nuoc thanh tung frame: khop mau sprite holywater_bottle (vung duc 8x8) + ngon lua holywater_fire_1..4.
# Chay: python -P -E Tools/cvm3/kb4_holy.py <video> <ten> <f_nem> <simon_px> <simon_Y> <huong +1/-1> [n=45] [x0= x1= y0= y1=]
#   f_nem = frame tim -1 (bam nut); simon_px/Y = pivot Simon man NES; vung tim mac dinh: truoc mat Simon 0..150 px, Y-60..Y+80
# Ra: ClaudeNotes/cvm3/_cache/04/holy/<ten>.csv (frame,rel,dup,kind,X,Y,err) - toa do = quy uoc Unity: lo X = cot trai + 4,
#     Y = hang tren cung vung duc (tam o 8x16); lua X = cot trai + 4 (8 rong) / + 8 (16 rong), Y = hang day + 1 (mat san) ; + anh dai khung
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K


def load(name, flip=0):
    im = np.asarray(Image.open(os.path.join(K.SUB_DIR, name + '.png')).convert('RGBA')).astype(float)
    if flip:
        im = im[:, ::-1]
    a = im[..., 3] > 0
    ys, xs = np.nonzero(a)
    return dict(name=name, w=im.shape[1], h=im.shape[0], ys=ys, xs=xs, col=im[ys, xs, :3])


def errmap(nes, t, x0, x1, y0, y1, col=None):
    """sai khac TB tren pixel duc cua mau t khi goc tren-trai slot o (x, y), x0<=x<x1, y0<=y<y1"""
    H, W, _ = nes.shape
    pad = 64
    big = np.zeros((H + 2 * pad, W + 2 * pad, 3)); big[pad:pad + H, pad:pad + W] = nes
    cols = t['col'] if col is None else col
    acc = np.zeros((y1 - y0, x1 - x0))
    for (yy, xx, c) in zip(t['ys'], t['xs'], cols):
        sub = big[pad + y0 + yy:pad + y1 + yy, pad + x0 + xx:pad + x1 + xx]
        d = np.sqrt(((sub - c) ** 2).sum(2)) / 441.7
        acc += np.minimum(d, K.CLIP)
    return acc / len(t['ys'])


def main():
    video, name = sys.argv[1], sys.argv[2]
    fthrow, spx, sY, face = map(int, sys.argv[3:7])
    opt = dict(n=45)
    for a in sys.argv[7:]:
        k, v = a.split('=')
        opt[k] = int(v)
    n = opt['n']
    bx0 = opt.get('x0', spx - 10 if face > 0 else spx - 150)
    bx1 = opt.get('x1', spx + 150 if face > 0 else spx + 10)
    by0 = opt.get('y0', sY - 60); by1 = opt.get('y1', sY + 80)
    bx0 = max(-4, bx0); bx1 = min(252, bx1); by0 = max(0, by0); by1 = min(232, by1)
    bottles = [load('holywater_bottle', 0), load('holywater_bottle', 1)]   # video Map_3: lo KHONG lat khi nem sang phai (co chai tren-phai)
    fires = [load('holywater_fire_%d' % i) for i in (1, 2, 3, 4)]
    out = []; frames = []; prev = None
    for f, img, nes in K.read_frames(video, fthrow - 2, fthrow + n):
        dup = K.is_dup(nes, prev); prev = nes
        frames.append((f, nes))
        best = ('bottle', 9.0, 0, 0)
        for bi, bt in enumerate(bottles):
            e = errmap(nes, bt, bx0, bx1, by0, by1)
            iy, ix = np.unravel_index(np.argmin(e), e.shape)
            if e[iy, ix] < best[1]:
                best = ('bottle' if bi == 0 else 'bottleF', float(e[iy, ix]), bx0 + ix + 4, by0 + iy + 8)
        for t in fires:
            ef = errmap(nes, t, bx0, bx1, by0, by1)
            jy, jx = np.unravel_index(np.argmin(ef), ef.shape)
            if ef[jy, jx] < best[1] - 0.01:
                best = (t['name'], float(ef[jy, jx]), bx0 + jx + t['w'] // 2, by0 + jy + t['h'])
        out.append(dict(frame=f, rel=f - fthrow, dup=int(dup), kind=best[0], err=best[1], X=best[2], Y=best[3]))
    os.makedirs(os.path.join(K.CACHE, 'holy'), exist_ok=True)
    with open(os.path.join(K.CACHE, 'holy', name + '.csv'), 'w', newline='') as fh:
        w = csv.writer(fh); w.writerow(['frame', 'rel', 'dup', 'kind', 'X', 'Y', 'err'])
        for o in out:
            w.writerow([o['frame'], o['rel'], o['dup'], o['kind'], o['X'], o['Y'], '%.3f' % o['err']])
    # dai khung: vung tim x2
    S = 2; cw, ch = bx1 - bx0, by1 - by0; ncol = 8
    rows = (len(frames) + ncol - 1) // ncol
    im = Image.new('RGB', (ncol * cw * S, rows * (ch * S + 12)), (40, 40, 40)); dr = ImageDraw.Draw(im)
    for i, (f, nes) in enumerate(frames):
        x = (i % ncol) * cw * S; y = (i // ncol) * (ch * S + 12)
        crop = np.clip(nes[by0:by1, max(0, bx0):bx1], 0, 255).astype(np.uint8)
        im.paste(Image.fromarray(crop).resize((crop.shape[1] * S, crop.shape[0] * S), Image.NEAREST), (x, y + 12))
        o = out[i]
        if o['err'] < 0.12:
            mx = x + (o['X'] - bx0) * S; my = y + 12 + (o['Y'] - by0) * S
            dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(255, 0, 0) if o['kind'] == 'bottle' else (0, 255, 0))
        dr.text((x + 2, y), '%d%s %s %d,%d %.2f' % (o['rel'], '*' if o['dup'] else '', o['kind'][:6].replace('holywa', 'f'), o['X'], o['Y'], o['err']), fill=(255, 255, 0))
    im.save(os.path.join(K.CACHE, 'holy', name + '.png'))
    for o in out:
        print(o['frame'], o['rel'], o['dup'], o['kind'], o['X'], o['Y'], '%.3f' % o['err'])


if __name__ == '__main__':
    main()
