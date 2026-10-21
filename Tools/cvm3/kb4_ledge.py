# kb4_ledge.py (CVM3 04) - Simon di khoi mep buc (Map_1 / Map_2): x bat dau roi so voi mep o dac (luoi va cham Unity Block 1/2).
# Moi su kien: bam Simon tung frame quanh luc roi (kb4_common.fit), x the gioi = cx camera (csv video) + px man; tim frame dung cuoi
# + frame roi dau (Y tang); x0 roi = x frame roi dau - huong*(so tick roi - 1) (roi 8 px/tick, giu 1 px/tick ngang - CVGP 08).
# Mep E = bien o dac cua hang san duoi chan (TYPE_CHARS '#','=','X' dac). Unity hien tai: roi khi than 16 px (X-8..X+7) het o dac
#   => di PHAI khoi mep phai: x0 = E + 8 ; di TRAI khoi mep trai: x0 = E - 9 (X+7 < E).
# Chay: python -P -E Tools/cvm3/kb4_ledge.py  -> ClaudeNotes/cvm3/_cache/04/ledge/ledge.txt + strip_*.png
import sys, os, csv, re, math
import numpy as np
from PIL import Image, ImageDraw
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kb4_common as K

REPO = K.REPO
CAM = {'map1': os.path.join(REPO, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work', 'map1_camera.csv'),
       'map2': os.path.join(REPO, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')}
LVL = {'map1': 'Block1', 'map2': 'Block2'}
# (video, frame truoc luc roi ~8 frame, goi y px man, Y man)
EVENTS = [('map1', 1874, 128, 144), ('map1', 3748, 128, 128),
          ('map2', 1071, 190, 112), ('map2', 2401, 128, 80), ('map2', 2499, 128, 80), ('map2', 3378, 128, 144),
          ('map2', 3816, 128, 80), ('map2', 4244, 64, 112), ('map2', 4495, 190, 144), ('map2', 4588, 160, 160),
          ('map2', 6039, 188, 160), ('map2', 6680, 128, 160)]


def load_cam(video):
    d = {}
    for r in csv.DictReader(open(CAM[video])):
        sec = r.get('section') or r.get('sec')
        d[int(r['frame'])] = (sec, int(float(r['cx'])))
    return d


def load_grid(video, sec):
    p = os.path.join(REPO, 'Assets', 'Data', 'Config', 'Level', LVL[video], sec + '.asset')
    txt = open(p, encoding='utf-8').read().splitlines()
    rows = []; on = False
    for l in txt:
        if l.strip() == 'collision:':
            on = True; continue
        if on:
            m = re.match(r"\s*- '?([^']*)'?\s*$", l)     # YAML: hang bat dau '.' khong co nhay
            if not m or not l.strip().startswith('-'):
                break
            rows.append(m.group(1))
    return rows


def solid(c):
    return c in '#=X'


def main():
    out_dir = os.path.join(K.CACHE, 'ledge'); os.makedirs(out_dir, exist_ok=True)
    sprites = K.load_sprites(['idle_0', 'walk_0', 'walk_1', 'walk_2', 'jumpduck_0'])
    lines = []
    for video, f0, gpx, gY in EVENTS:
        cam = load_cam(video)
        trk = []; prev = None; px, Y = gpx, gY; frames = []
        for f, img, nes in K.read_frames(video, f0, f0 + 16):
            dup = K.is_dup(nes, prev); prev = nes
            b = K.fit(nes, sprites, K.SHEET_COLORS, px, Y, rx=8, ry=40)
            sec, cx = cam.get(f, ('?', 0))
            if b['err'] < 0.12:
                px, Y = b['px'], b['Y']
            trk.append(dict(f=f, dup=dup, px=b['px'], Y=b['Y'], err=b['err'], spr=b['sprite'], sec=sec, cx=cx, X=cx + b['px']))
            frames.append((f, nes, b))
        Ys = trk[0]['Y']
        s = None; ff = None
        for i, t in enumerate(trk):
            if t['dup'] or t['err'] > 0.12:
                continue
            if t['Y'] == Ys and ff is None:
                s = t
            elif t['Y'] > Ys and ff is None:
                ff = t
        if s is None or ff is None:
            lines.append('%s f%d: khong thay roi' % (video, f0)); print(lines[-1]); continue
        dirn = 1 if ff['X'] > trk[0]['X'] else -1
        dy = ff['Y'] - Ys; k = int(math.ceil(dy / 8.0))
        x0_hi = ff['X'] - dirn * (k - 1)           # x tick roi dau (neu roi du 8 px moi tick tu tick dau)
        sec = s['sec']; grid = load_grid(video, sec)
        r = (Ys - 32) // 16
        row = grid[r] if 0 <= r < len(grid) else ''
        # mep gan X nhat theo huong di: bien o dac -> o trong (di phai) / o trong -> o dac (di trai), trong [X-24, X+24]
        E = None
        for b in sorted(range((s['X'] - 24) // 16 * 16, s['X'] + 25, 16), key=lambda b: abs(b - s['X'])):
            cl, cr = b // 16 - 1, b // 16
            okl = 0 <= cl < len(row) and solid(row[cl]); okr = 0 <= cr < len(row) and solid(row[cr])
            if (dirn > 0 and okl and not okr) or (dirn < 0 and okr and not okl):
                E = b; break
        if E is None:
            lines.append('%s f%d: khong tim thay mep' % (video, f0)); print(lines[-1]); continue
        unity_x0 = E + 8 if dirn > 0 else E - 9
        # d = khoang tu x0 toi o dac cuoi ma than con cham: di phai d = x0 - E (Unity 8), di trai d = (E - 1) - x0 (Unity 8)
        d_s = (s['X'] - E) if dirn > 0 else (E - 1 - s['X'])
        d_f = (x0_hi - E) if dirn > 0 else (E - 1 - x0_hi)
        lines.append('%s f%d %s huong %+d san Y%d hang %d mep E=%d | dung cuoi f%d X%d (d %+d) | roi dau f%d X%d dy%d (%d tick) => x0 ~%d (d %+d) | Unity x0 = %d (d %+d)'
                     % (video, f0, sec, dirn, Ys, r, E, s['f'], s['X'], d_s, ff['f'], ff['X'], dy, k, x0_hi, d_f, unity_x0, 8 if dirn > 0 else 8))
        print(lines[-1], flush=True)
        # dai khung
        S = 3; cw, ch = 64, 72; ncol = len(frames)
        im = Image.new('RGB', (ncol * cw * S, ch * S + 12), (40, 40, 40)); dr = ImageDraw.Draw(im)
        cx0 = gpx - 32; cy0 = gY - 48
        for i, (f, nes, b) in enumerate(frames):
            x = i * cw * S
            crop = np.zeros((ch, cw, 3)); xa, xb = max(0, cx0), min(256, cx0 + cw); ya, yb = max(0, cy0), min(240, cy0 + ch)
            crop[ya - cy0:yb - cy0, xa - cx0:xb - cx0] = nes[ya:yb, xa:xb]
            im.paste(Image.fromarray(np.clip(crop, 0, 255).astype(np.uint8)).resize((cw * S, ch * S), Image.NEAREST), (x, 12))
            t = trk[i]
            mx = x + (b['px'] - cx0) * S; my = 12 + (b['Y'] - cy0) * S
            dr.rectangle([mx - 2, my - 2, mx + 2, my + 2], outline=(255, 0, 0))
            dr.text((x + 2, 0), '%d%s X%d Y%d' % (f, '*' if t['dup'] else '', t['X'], t['Y']), fill=(255, 255, 0))
        im.save(os.path.join(out_dir, 'strip_%s_%d.png' % (video, f0)))
    open(os.path.join(out_dir, 'ledge.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')


if __name__ == '__main__':
    main()
