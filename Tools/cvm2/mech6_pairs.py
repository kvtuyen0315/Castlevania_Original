# mech6_pairs.py (CVM2 06) - ghep cap anh video Map_2 <-> Unity (Map2MechProbe ShotDir) CUNG trang thai co che:
#   be: cung mep trai be (px section) + cung chieu chay; bay ep: cung bo khung (k1,k2,k3) cua cac bay thay duoc ca 2 ben
#   cat cung vung px section (camera moi ben: Unity cx = kep(Simon x - 128); video cx = map2_camera.csv) tren luoi NES, phong 3x
# Chay (tu goc repo): python -P -E Tools/cvm2/mech6_pairs.py  -> ClaudeNotes/cvm2/_cache/shots/06/pairs_<n>.png + pairs.txt
import os, csv, sys
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
C6 = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '06')
SHOTS = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '06')
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
mm = np.load(os.path.join(WORK, 'map2_nes.npy'), mmap_mode='r')
dup = np.load(os.path.join(WORK, 'map2_dup.npy'))
cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_camera.csv')))}
CXMAX = {'TowerEntranceB': 256, 'TowerHallB': 512, 'TowerHallC': 512}
SC = 3

def unity_crop(png, cx, x0, x1, y0, y1):
    im = np.asarray(Image.open(png).convert('RGB'))
    xs = 448 + 4 * (np.arange(x0, x1) - cx) + 2
    ys = 60 + 4 * (np.arange(y0, y1) + 32) + 2
    ok = (xs >= 448) & (xs < 448 + 1024)
    out = np.zeros((y1 - y0, x1 - x0, 3), np.uint8)
    out[:, ok] = im[ys][:, xs[ok]]
    return out

def video_crop(f, cx, x0, x1, y0, y1):
    a = np.asarray(mm[f])
    xs = np.arange(x0, x1) - cx
    ys = np.arange(y0, y1) + 32 - 8
    ok = (xs >= 0) & (xs < 256)
    out = np.zeros((y1 - y0, x1 - x0, 3), np.uint8)
    out[:, ok] = a[ys][:, xs[ok]]
    return out

def trace(name):
    return list(csv.DictReader(open(os.path.join(C6, 'traces', name + '.csv'))))

def vplat(n):
    rows = [r for r in csv.DictReader(open(os.path.join(C6, 'platform_%s.csv' % n)))
            if not int(r['dup']) and int(r['vis']) == 32 and float(r['margin']) > 0.6]
    out = []
    for a, b in zip(rows, rows[1:]):
        if int(b['frame']) - int(a['frame']) <= 2 and int(b['x']) != int(a['x']):
            out.append((int(a['frame']), int(a['x']), 1 if int(b['x']) > int(a['x']) else -1))
    return out

pairs = []   # (nhan, anh video, anh unity)
# --- be
for scn, sec, col, vids, win in (('plat_ride_teb', 'TowerEntranceB', 'p0x', ['teb'], (16, 128, 40, 120)),
                                 ('plat_ride_hallb', 'TowerHallB', 'p0x', ['hallb1', 'hallb1r'], (240, 384, 40, 120)),
                                 ('plat_ride_hallb', 'TowerHallB', 'p1x', ['hallb2', 'hallb2r'], (376, 496, 40, 120)),
                                 ('plat_jump_off', 'TowerEntranceB', 'p0x', ['teb'], (16, 128, 40, 120))):
    tr = trace(scn)
    cand = []
    for v in vids: cand += vplat(v)
    for png in sorted(os.listdir(os.path.join(SHOTS, 'unity'))):
        if not png.startswith(scn + '_r'): continue
        rel = int(png[len(scn) + 2:-4])
        if rel + 1 >= len(tr): continue
        x = int(tr[rel][col]); d = 1 if int(tr[rel + 1][col]) > x else (-1 if int(tr[rel + 1][col]) < x else 0)
        if d == 0 and rel > 0: d = 1 if x > int(tr[rel - 1][col]) else -1
        m = [c for c in cand if c[1] == x and c[2] == d]
        if not m: m = [c for c in cand if abs(c[1] - x) <= 1 and c[2] == d]
        if not m: continue
        f = m[len(m) // 2][0]
        ucx = min(max(int(tr[rel]['sx']) - 128, 0), CXMAX[sec]); vcx = int(cam[f]['cx'])
        x0, x1, y0, y1 = win
        pairs.append(('%s bệ %s mép trái %d chiều %+d | video f%d (%.2fs) cx %d | Unity rel %d cx %d' % (sec, col, x, d, f, f / 30.0, vcx, rel, ucx),
                      video_crop(f, vcx, x0, x1, y0, y1), unity_crop(os.path.join(SHOTS, 'unity', png), ucx, x0, x1, y0, y1), (scn, rel, f)))
# --- bay ep
vrows = []
for n in ('low', 'up'):
    for r in csv.DictReader(open(os.path.join(C6, 'crusher_%s.csv' % n))):
        if int(r['dup']): continue
        ks = tuple(int(r['k%d' % i]) if float(r['m%d' % i]) >= 1.5 else -2 for i in (1, 2, 3))
        vrows.append((int(r['frame']), int(r['cx']), ks))
for scn in ('crusher_cycle', 'crusher_pass', 'crusher_stopwatch'):
    tr = trace(scn)
    for png in sorted(os.listdir(os.path.join(SHOTS, 'unity'))):
        if not png.startswith(scn + '_r'): continue
        rel = int(png[len(scn) + 2:-4])
        if rel >= len(tr): continue
        uk = tuple(int(tr[rel]['c%d' % i]) for i in (1, 2, 3))
        ucx = min(max(int(tr[rel]['sx']) - 128, 0), 512)
        uvis = tuple(368 + 64 * i - 16 >= ucx and 368 + 64 * i + 16 <= ucx + 256 for i in range(3))
        best = None
        for f, vcx, vk in vrows:
            common = [i for i in range(3) if uvis[i] and vk[i] >= 0]
            if len(common) < 2: continue
            if all(vk[i] == uk[i] for i in common):
                score = len(common)
                if best is None or score > best[0]: best = (score, f, vcx, common)
        if best is None: continue
        score, f, vcx, common = best
        pairs.append(('Hall C bẫy %s (k1,k2,k3 Unity %s) | video f%d (%.2fs) cx %d | Unity %s rel %d cx %d' % ('+'.join(str(i + 1) for i in common), uk, f, f / 30.0, vcx, scn, rel, ucx),
                      video_crop(f, vcx, 336, 528, 72, 160), unity_crop(os.path.join(SHOTS, 'unity', png), ucx, 336, 528, 72, 160), (scn, rel, f)))
# --- ghep
lines = []
per = 3
for k in range(0, len(pairs), per):
    grp = pairs[k:k + per]
    W = max(p[1].shape[1] for p in grp) * SC * 2 + 30
    H = sum(p[1].shape[0] * SC + 30 for p in grp)
    img = Image.new('RGB', (W, H), (40, 40, 40)); d = ImageDraw.Draw(img); y = 0
    for lab, v, u, meta in grp:
        d.text((4, y + 2), lab[:150], fill=(255, 255, 0))
        vi = Image.fromarray(v).resize((v.shape[1] * SC, v.shape[0] * SC), Image.NEAREST)
        ui = Image.fromarray(u).resize((u.shape[1] * SC, u.shape[0] * SC), Image.NEAREST)
        img.paste(vi, (0, y + 16)); img.paste(ui, (vi.size[0] + 30, y + 16))
        d.text((4, y + 18), 'VIDEO', fill=(0, 255, 255)); d.text((vi.size[0] + 34, y + 18), 'UNITY', fill=(0, 255, 255))
        y += vi.size[1] + 30
        lines.append('pairs_%d: %s' % (k // per, lab))
    img.save(os.path.join(SHOTS, 'pairs_%d.png' % (k // per)))
open(os.path.join(SHOTS, 'pairs.txt'), 'w', encoding='utf-8').write('\n'.join(lines) + '\n')
print(len(pairs), 'cap ->', (len(pairs) + per - 1) // per, 'tam')
EOF_MARK = None
