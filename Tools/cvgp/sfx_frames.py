"""sfx_frames.py - trích khung video quanh một thời điểm để xác nhận sự kiện bằng mắt.

Chạy (từ gốc repo):
    python -P -E Tools/cvgp/sfx_frames.py map1|boss out.png t1 [t2 ...] [--dt 0.1] [--n 4]
Mỗi hàng = một thời điểm t: n khung ở t-dt, t, t+dt, ... (thu nhỏ 1/3, chỉ vùng game). Cần ffmpeg ở C:\ffmpeg\bin.
"""
import os
import subprocess
import sys
import tempfile

from PIL import Image, ImageDraw

VID = r"C:\Users\kvtuy\Videos\Screen Recordings\Castlevania_Original"
FILES = {"map1": "Map_1.mp4", "boss": "Boss_Map_1_Behaviour.mp4"}
CROP = {"map1": (9, 11, 1244, 932), "boss": (170, 72, 929, 698)}  # x, y, w, h (vùng game trong khung video)
FF = r"C:\ffmpeg\bin\ffmpeg.exe"


def frame(name, t):
    x, y, w, h = CROP[name]
    tmp = os.path.join(tempfile.gettempdir(), "cvgp_frame.png")
    subprocess.run([FF, "-y", "-loglevel", "error", "-ss", "%.3f" % max(0, t), "-i", os.path.join(VID, FILES[name]),
                    "-frames:v", "1", "-vf", "crop=%d:%d:%d:%d,scale=%d:-1" % (w, h, x, y, w // 4), tmp], check=True)
    return Image.open(tmp).convert("RGB").copy()


if __name__ == "__main__":
    a = sys.argv[1:]
    name, out = a[0], a[1]
    dt, n = 0.1, 4
    ts = []
    i = 2
    while i < len(a):
        if a[i] == "--dt":
            dt = float(a[i + 1]); i += 2
        elif a[i] == "--n":
            n = int(a[i + 1]); i += 2
        else:
            ts.append(float(a[i])); i += 1
    rows = []
    for t in ts:
        ims = [frame(name, t + (k - 1) * dt) for k in range(n)]
        row = Image.new("RGB", (sum(im.width for im in ims) + 2 * n, ims[0].height + 12), (30, 30, 60))
        x = 0
        d = ImageDraw.Draw(row)
        for k, im in enumerate(ims):
            row.paste(im, (x, 12))
            d.text((x + 2, 0), "t=%.2f" % (t + (k - 1) * dt), fill=(255, 255, 0))
            x += im.width + 2
        rows.append(row)
    W = max(r.width for r in rows)
    canvas = Image.new("RGB", (W, sum(r.height for r in rows)), (0, 0, 0))
    y = 0
    for r in rows:
        canvas.paste(r, (0, y)); y += r.height
    canvas.save(out)
    print("ghi", out, canvas.size)
