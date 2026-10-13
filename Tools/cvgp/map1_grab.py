"""CVGP 15 — trích frame video Map_1/boss về lưới NES 256×240 (hiệu chuẩn MAP1_TIMELINE §1) để đo/so ảnh.

Chạy (từ gốc repo):
  python -P -E Tools/cvgp/map1_grab.py <map1|boss> <f0-f1[:bước] | f,f,f…> [outdir=ClaudeNotes/cvgp/_cache/15/v] [montage.png] [scale=2]
  → ghi <outdir>/<video>_f<frame>.png (256×240, hàng ngoài vùng thấy = đen); có montage.png thì ghép thêm 1 ảnh (ghi số frame).
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
VID_DIR = os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video')
sys.path.insert(0, os.path.join(VID_DIR, 'tools'))
import nes_common as nc  # noqa: E402

Y0 = {'map1': 8, 'boss': 10}


def parse(spec):
    out = []
    for part in spec.split(','):
        if '-' in part:
            a, b = part.split('-')
            step = 1
            if ':' in b:
                b, s = b.split(':')
                step = int(s)
            out += list(range(int(a), int(b) + 1, step))
        else:
            out.append(int(part))
    return sorted(set(out))


def grab(video, frames_wanted):
    """Trả {frame: ảnh NES 240×256×3 uint8} (đọc 1 lượt liên tục từ frame nhỏ nhất tới lớn nhất)."""
    f0, f1 = frames_wanted[0], frames_wanted[-1]
    want = set(frames_wanted)
    y0 = Y0[video]
    out = {}
    ss = max(0.0, (f0 - 0.5) / 30.0)
    f = f0
    for img in nc.read_frames(video, ss=ss, t=(f1 - f0 + 1) / 30.0):
        if f > f1:
            break
        if f in want:
            nes = nc.nes_sample(img, nc.CALIB[video], y0=y0, y1=240 - y0)
            full = np.zeros((240, 256, 3), np.uint8)
            full[y0:240 - y0] = np.clip(nes, 0, 255).astype(np.uint8)
            out[f] = full
        f += 1
    return out


def main():
    video = sys.argv[1]
    frames_wanted = parse(sys.argv[2])
    outdir = sys.argv[3] if len(sys.argv) > 3 else os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', '15', 'v')
    montage = sys.argv[4] if len(sys.argv) > 4 else None
    scale = int(sys.argv[5]) if len(sys.argv) > 5 else 2
    os.makedirs(outdir, exist_ok=True)
    imgs = grab(video, frames_wanted)
    for f, a in imgs.items():
        Image.fromarray(a).save(os.path.join(outdir, '%s_f%d.png' % (video, f)))
    if montage:
        keys = sorted(imgs)
        cols = min(4, len(keys))
        rows = (len(keys) + cols - 1) // cols
        m = Image.new('RGB', (cols * (256 * scale + 4), rows * (240 * scale + 4)), (40, 40, 40))
        d = ImageDraw.Draw(m)
        for i, f in enumerate(keys):
            x, y = (i % cols) * (256 * scale + 4), (i // cols) * (240 * scale + 4)
            m.paste(Image.fromarray(imgs[f]).resize((256 * scale, 240 * scale), Image.NEAREST), (x, y))
            d.text((x + 4, y + 2), 'f%d' % f, fill=(0, 255, 0))
        m.save(montage)
    print('ghi', len(imgs), 'frame vao', outdir)


if __name__ == '__main__':
    main()
