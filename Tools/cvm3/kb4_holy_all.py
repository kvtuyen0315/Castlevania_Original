# kb4_holy_all.py (CVM3 04) - moi lan nem nuoc thanh (tim -1) o Map_3 / Boss_Map_1 / Boss_Map_2: tu the + vi tri Simon luc nem,
# quy dao lo tung frame (kb4_holy.errmap, 2 huong lat), ngon lua sau khi vo. Ra: ClaudeNotes/cvm3/_cache/04/holy/all.csv + strip_<video>_<f>.png
# Chay: python -P -E Tools/cvm3/kb4_holy_all.py [video ...]
import sys, os, csv
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K
import kb4_holy as H

# (video, frame tim -1, goi y pivot Simon px, Y) - goi y tu ref/data/*_simon.csv (CVM2/CVM3 02) / boss_simon2.csv (CVGP)
THROWS = [('map3', f, 22, 144) for f in (6074, 6147, 6230, 6312, 6394, 6475, 6556, 6634, 6717)] + [
    ('map3', 5198, 128, 125), ('map3', 3031, 128, 176),
    ('boss1', 182, 201, 208), ('boss1', 238, 187, 208), ('boss1', 250, 187, 208), ('boss1', 326, 231, 208), ('boss1', 356, 231, 208),
    ('boss1', 380, 239, 208), ('boss1', 398, 239, 208), ('boss1', 459, 235, 208), ('boss1', 471, 235, 208), ('boss1', 530, 235, 208), ('boss1', 545, 235, 208),
    ('boss2', 30, 82, 176), ('boss2', 126, 23, 176), ('boss2', 141, 23, 176), ('boss2', 156, 23, 176), ('boss2', 191, 23, 176), ('boss2', 219, 23, 176),
    ('boss2', 230, 23, 176), ('boss2', 243, 23, 176), ('boss2', 375, 72, 176), ('boss2', 388, 72, 176), ('boss2', 400, 72, 176),
    ('boss2', 449, 71, 176), ('boss2', 465, 71, 176), ('boss2', 477, 71, 176)]

N_AFTER = 48


def simon_at(video, f, px, Y, sprites):
    best = None
    for ff, img, nes in K.read_frames(video, f, f + 10):
        b = K.fit(nes, sprites, K.SHEET_COLORS, px, Y, rx=24, ry=24)
        if b is not None and (best is None or b['err'] < best['err']):
            best = b; best['frame'] = ff
    return best


def main():
    vids = sys.argv[1:] or None
    sprites = K.load_sprites()
    bottles = [H.load('holywater_bottle', 0), H.load('holywater_bottle', 1)]
    fires = [H.load('holywater_fire_%d' % i) for i in (1, 2, 3, 4)]
    rows = []
    for video, f, gpx, gY in THROWS:
        if vids and video not in vids:
            continue
        s = simon_at(video, f, gpx, gY, sprites)
        spx, sY, face = s['px'], s['Y'], (1 if s['flip'] else -1)
        bx0 = max(-4, spx - 16 if face > 0 else spx - 150); bx1 = min(252, spx + 150 if face > 0 else spx + 16)
        by0 = max(8, sY - 64); by1 = min(226, sY + 90)
        prev = None; frames = []
        for ff, img, nes in K.read_frames(video, f, f + N_AFTER):
            dup = K.is_dup(nes, prev); prev = nes
            bb = (9.0, None, 0, 0)
            for bi, bt in enumerate(bottles):
                e = H.errmap(nes, bt, bx0, bx1, by0, by1)
                iy, ix = np.unravel_index(np.argmin(e), e.shape)
                if e[iy, ix] < bb[0]:
                    bb = (float(e[iy, ix]), bi, bx0 + ix + 4, by0 + iy + 8)
            fb = (9.0, None, 0, 0)
            for t in fires:
                e = H.errmap(nes, t, bx0, bx1, by0, by1)
                iy, ix = np.unravel_index(np.argmin(e), e.shape)
                if e[iy, ix] < fb[0]:
                    fb = (float(e[iy, ix]), t['name'][-1], bx0 + ix + t['w'] // 2, by0 + iy + t['h'])
            rows.append(dict(video=video, throw=f, frame=ff, rel=ff - f, dup=int(dup), spx=spx, sY=sY, face=face, spose=s['sprite'],
                             berr='%.3f' % bb[0], bflip=bb[1], bX=bb[2], bY=bb[3], ferr='%.3f' % fb[0], fk=fb[1], fX=fb[2], fY=fb[3]))
            frames.append((ff, nes, bb, fb))
        # dai khung
        S = 2; cw, ch = bx1 - bx0, by1 - by0; ncol = 7
        nr = (len(frames) + ncol - 1) // ncol
        im = Image.new('RGB', (ncol * cw * S, nr * (ch * S + 12)), (40, 40, 40)); dr = ImageDraw.Draw(im)
        for i, (ff, nes, bb, fb) in enumerate(frames):
            x = (i % ncol) * cw * S; y = (i // ncol) * (ch * S + 12)
            crop = np.zeros((ch, cw, 3)); xa = max(0, bx0)
            crop[:, xa - bx0:] = nes[by0:by1, xa:bx1]
            im.paste(Image.fromarray(np.clip(crop, 0, 255).astype(np.uint8)).resize((cw * S, ch * S), Image.NEAREST), (x, y + 12))
            lab = '%d' % (ff - f)
            if bb[0] < 0.11:
                mx = x + (bb[2] - bx0) * S; my = y + 12 + (bb[3] - by0) * S
                dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(255, 0, 0)); lab += ' b%d,%d' % (bb[2], bb[3])
            if fb[0] < 0.11:
                mx = x + (fb[2] - bx0) * S; my = y + 12 + (fb[3] - by0) * S
                dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(0, 255, 0)); lab += ' F%s %d,%d' % (fb[1], fb[2], fb[3])
            dr.text((x + 2, y), lab, fill=(255, 255, 0))
        im.save(os.path.join(K.CACHE, 'holy', 'strip_%s_%d.png' % (video, f)))
        print(video, f, 'Simon', spx, sY, face, s['sprite'], '%.3f' % s['err'], flush=True)
    p = os.path.join(K.CACHE, 'holy', 'all%s.csv' % ('_' + '_'.join(vids) if vids else ''))
    with open(p, 'w', newline='') as fh:
        w = csv.DictWriter(fh, fieldnames=list(rows[0].keys())); w.writeheader(); w.writerows(rows)
    print('ok', p)


if __name__ == '__main__':
    main()
