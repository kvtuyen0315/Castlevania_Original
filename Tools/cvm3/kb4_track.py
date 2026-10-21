# kb4_track.py (CVM3 04) - bam Simon tung frame video bang khop mau sprite (luoi NES) + anh dai khung de xem tay.
# Chay: python -P -E Tools/cvm3/kb4_track.py <video> <ten> <f0> <f1> <px0> <Y0> [sprites=hurt_0,idle_0,...] [r=10]
#   video = map1|boss1|map2|boss2|map3|boss3 ; px0/Y0 = pivot man NES doan dau (frame f0)
# Ra: ClaudeNotes/cvm3/_cache/04/track/<ten>.csv (frame,t,dup,sprite,flip,px,Y,err,sharp) + <ten>.png (dai khung x3)
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K


def track(video, f0, f1, px0, Y0, names, r, palette=None):
    sprites = K.load_sprites(names)
    pal = palette or K.SHEET_COLORS
    out = []
    prev_nes = None
    px, Y = px0, Y0
    frames = []
    for f, img, nes in K.read_frames(video, f0, f1):
        dup = K.is_dup(nes, prev_nes)
        prev_nes = nes
        b = K.fit(nes, sprites, pal, px, Y, rx=r, ry=r)
        frames.append((f, nes))
        if b is None:
            out.append(dict(frame=f, dup=int(dup), err=None))
            continue
        b['frame'] = f; b['dup'] = int(dup)
        out.append(b)
        if b['err'] < 0.12:
            px, Y = b['px'], b['Y']
    return out, frames, sprites


def main():
    video, name = sys.argv[1], sys.argv[2]
    f0, f1, px0, Y0 = map(int, sys.argv[3:7])
    names = None; r = 10
    for a in sys.argv[7:]:
        if a.startswith('sprites='):
            names = a[8:].split(',')
        elif a.startswith('r='):
            r = int(a[2:])
    out, frames, sprites = track(video, f0, f1, px0, Y0, names, r)
    # hieu chuan bang mau Simon tren video tu cac khop tot
    for it in range(2):
        acc = [[], [], []]
        good = sorted([b for b in out if b.get('err') is not None], key=lambda b: b['err'])[:max(3, len(out) // 3)]
        for b in good:
            s = [s for s in sprites if s['name'] == b['sprite'] and s['flip'] == b['flip']][0]
            nes = dict(frames)[b['frame']]
            for k, c in enumerate(K.calib_palette(nes, s, b['px'], b['Y'])):
                acc[k].append(c)
        pal = [tuple(np.median(np.array(a), axis=0)) if a else K.SHEET_COLORS[k] for k, a in enumerate(acc)]
        out, frames, sprites = track(video, f0, f1, px0, Y0, names, r, pal)
    os.makedirs(os.path.join(K.CACHE, 'track'), exist_ok=True)
    p = os.path.join(K.CACHE, 'track', name + '.csv')
    with open(p, 'w', newline='') as fh:
        w = csv.writer(fh)
        w.writerow(['frame', 't', 'dup', 'sprite', 'flip', 'px', 'Y', 'err', 'sharp'])
        for b in out:
            if b.get('err') is None:
                w.writerow([b['frame'], '%.3f' % (b['frame'] / 30.0), b['dup'], '', '', '', '', '', '']); continue
            sharp = min(b['nb'].values()) - b['err'] if b['nb'] else 0
            w.writerow([b['frame'], '%.3f' % (b['frame'] / 30.0), b['dup'], b['sprite'], b['flip'], b['px'], b['Y'], '%.4f' % b['err'], '%.4f' % sharp])
    # dai khung: o 64x64 NES quanh (px0, Y0-24) co dinh (thay chuyen dong), x3, cham do = pivot khop
    S = 3; cw, ch = 64, 64; ncol = 10
    rows = (len(frames) + ncol - 1) // ncol
    im = Image.new('RGB', (ncol * cw * S, rows * (ch * S + 12)), (40, 40, 40))
    dr = ImageDraw.Draw(im)
    cx0 = px0 - 32; cy0 = Y0 - 48
    byf = {b['frame']: b for b in out}
    for i, (f, nes) in enumerate(frames):
        x = (i % ncol) * cw * S; y = (i // ncol) * (ch * S + 12)
        crop = np.zeros((ch, cw, 3), np.uint8)
        ya, xa = max(0, cy0), max(0, cx0)
        yb, xb = min(240, cy0 + ch), min(256, cx0 + cw)
        crop[ya - cy0:yb - cy0, xa - cx0:xb - cx0] = np.clip(nes[ya:yb, xa:xb], 0, 255).astype(np.uint8)
        tile = Image.fromarray(crop).resize((cw * S, ch * S), Image.NEAREST)
        im.paste(tile, (x, y + 12))
        b = byf.get(f)
        lab = '%d%s' % (f, '*' if b and b['dup'] else '')
        if b and b.get('err') is not None:
            lab += ' %d,%d %s' % (b['px'], b['Y'], b['sprite'][:6])
            mx = x + (b['px'] - cx0) * S; my = y + 12 + (b['Y'] - cy0) * S
            dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(255, 0, 0))
        dr.text((x + 2, y), lab, fill=(255, 255, 0))
    im.save(os.path.join(K.CACHE, 'track', name + '.png'))
    print('ok', p, 'bang mau', [tuple(int(v) for v in c) for c in pal])
    for b in out:
        if b.get('err') is None:
            print(b['frame'], b['dup'], '-'); continue
        print(b['frame'], b['dup'], b['sprite'], b['flip'], b['px'], b['Y'], '%.3f' % b['err'], '%.3f' % (min(b['nb'].values()) - b['err'] if b['nb'] else 0))


if __name__ == '__main__':
    main()
