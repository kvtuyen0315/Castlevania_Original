# fg_map.py - tach tien canh (sprite) bang cach tru nen ban do (sprite sheet) theo camera cx da biet
# Chay: python -P -E tools/fg_map.py map1 <f0> <count> <step> <out.png>   (anh: tren = frame, duoi = mask tien canh)
# Dung lai trong whip_track.py (ham fg_mask)
import sys, os, csv
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import load_map, section_view, SECTIONS
_base = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'work')
_secs = None
def secs():
    global _secs
    if _secs is None:
        mp = load_map(); _secs = {k: section_view(mp, k) for k in SECTIONS}
    return _secs
def fg_mask(frame_nes, section, cx, y0=8, thr=70):
    """frame_nes: (224,256,3) NES y 8..231. Tra ve mask (176,256) cho NES y 48..223."""
    bg = secs()[section][:, cx:cx + 256]
    f = frame_nes[48 - y0:224 - y0].astype(float)
    return np.abs(f - bg).max(axis=2) > thr
if __name__ == '__main__':
    from PIL import Image, ImageDraw
    video, f0, cnt, step, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), sys.argv[5]
    mm = np.load(os.path.join(_base, f'{video}_nes.npy'), mmap_mode='r')
    cam = {int(r['frame']): r for r in csv.DictReader(open(os.path.join(_base, f'{video}_camera.csv')))}
    img = Image.new('RGB', (cnt * 258, 2 * 176 + 12), (40, 40, 40)); d = ImageDraw.Draw(img)
    for k in range(cnt):
        i = f0 + k * step; c = cam[i]
        fr = np.asarray(mm[i])
        img.paste(Image.fromarray(fr[40:216]), (k * 258, 12))
        if c['section'] != '-':
            m = fg_mask(fr, c['section'], int(c['cx']))
            img.paste(Image.fromarray((m * 255).astype(np.uint8)).convert('RGB'), (k * 258, 188))
        d.text((k * 258 + 2, 0), f'f{i} {c["section"]} {c["cx"]}', fill=(255, 255, 0))
    img.save(out); print('saved', out)
