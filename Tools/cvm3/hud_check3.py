# hud_check3.py (CVM3 02) - kiểm bằng mắt: ghép N crop HUD (NES y 8..47, x 0..255, phóng 2x) kèm giá trị bộ đọc ghi trên ảnh
# Chạy: python -P -E Tools/cvm3/hud_check3.py map3 <out.png> [N=20] [seed=7]
import sys, os, csv, random
import numpy as np
from PIL import Image, ImageDraw
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '02', 'work')
video, out = sys.argv[1], sys.argv[2]
N = int(sys.argv[3]) if len(sys.argv) > 3 else 20
random.seed(int(sys.argv[4]) if len(sys.argv) > 4 else 7)
mm = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
R = [r for r in csv.DictReader(open(os.path.join(WORK, video + '_hud.csv'))) if r['hud_ok'] == '1']
pick = sorted(random.sample(R, N), key=lambda r: int(r['frame']))
S = 2; H = 40
img = Image.new('RGB', (2 * 256 * S + 4, ((N + 1) // 2) * (H * S + 14)), (40, 40, 40))
d = ImageDraw.Draw(img)
for k, r in enumerate(pick):
    f = int(r['frame'])
    crop = Image.fromarray(np.asarray(mm[f])[0:H]).resize((256 * S, H * S), Image.NEAREST)
    x = (k % 2) * (256 * S + 4); y = (k // 2) * (H * S + 14)
    img.paste(crop, (x, y + 12))
    d.text((x + 2, y), 'f%d SC %s TM %s ST %s HE %s P %s pb %s eb %s sub %s %s' % (f, r['score'], r['time'], r['stage'], r['hearts'], r['p'], r['pbar'], r['ebar'], r['sub'], r['mult']), fill=(255, 255, 0))
img.save(out)
print('frames', [int(r['frame']) for r in pick])
