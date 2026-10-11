"""sfx_view.py - vẽ phổ (spectrogram) để xem bằng mắt.

Chạy (từ gốc repo):
    python -P -E Tools/cvgp/sfx_view.py sfx [out.png]                 # montage 37 SFX
    python -P -E Tools/cvgp/sfx_view.py seg map1|boss t0 t1 [out.png] [NN[@t] ...]
        # phổ đoạn video [t0,t1] giây; thêm "NN@t" để chồng phổ SFX NN bắt đầu ở giây t (hộp màu) - đối chiếu bằng mắt
Mặc định ghi vào ClaudeNotes/cvgp/_cache/02/. Trục dọc 0-8 kHz (đáy = 0), ngang 200 px/giây (seg) / 250 px/giây (sfx).
"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sfx_match as M  # noqa: E402

OUT = os.path.join(M.ROOT, "ClaudeNotes", "cvgp", "_cache", "02")
FMAX = 8000.0
H = 160


def spec_img(x, pxs, gain=1.0):
    win = np.hanning(M.WIN)
    hop = int(M.SR / pxs)
    n = 1 + max(0, (len(x) - M.WIN) // hop)
    idx = np.arange(M.WIN)[None, :] + hop * np.arange(n)[:, None]
    mag = np.abs(np.fft.rfft(x[idx] * win[None, :], axis=1)) / (M.WIN / 4.0)
    f = np.fft.rfftfreq(M.WIN, 1.0 / M.SR)
    sel = f <= FMAX
    v = np.log1p(mag[:, sel] / 0.002) * gain
    v = v / 6.0
    v = np.clip(v, 0, 1)
    img = (v.T[::-1] * 255).astype(np.uint8)  # (bins, n), đáy = 0 Hz
    im = Image.fromarray(img).resize((n, H), Image.BILINEAR)
    return im


def load_sfx(num):
    s, sr = M.read_wav(os.path.join(M.SFX_DIR, "%s.wav" % num))
    return M.resample(s, sr, M.SR)


def montage(path):
    tiles = []
    for i in range(1, 38):
        num = "%02d" % i
        s = load_sfx(num)
        im = spec_img(s - s.mean(), 250)
        t = Image.new("RGB", (max(im.width, 90) + 8, H + 14), (20, 20, 40))
        t.paste(im.convert("RGB"), (4, 12))
        ImageDraw.Draw(t).text((4, 0), "%s  %.2fs" % (num, len(s) / M.SR), fill=(255, 255, 0))
        tiles.append(t)
    W = 1500
    rows, cur, curw = [], [], 0
    for t in tiles:
        if curw + t.width > W:
            rows.append(cur)
            cur, curw = [], 0
        cur.append(t)
        curw += t.width
    rows.append(cur)
    canvas = Image.new("RGB", (W, sum(max(t.height for t in r) for r in rows)), (0, 0, 0))
    y = 0
    for r in rows:
        x0 = 0
        for t in r:
            canvas.paste(t, (x0, y))
            x0 += t.width
        y += max(t.height for t in r)
    canvas.save(path)
    print("ghi", path, canvas.size)


def seg(name, t0, t1, path, overlays):
    x, sr = M.read_wav(os.path.join(OUT, name + ".wav"))
    x = M.resample(x, sr, M.SR)
    a, b = int(t0 * M.SR), int(t1 * M.SR)
    im = spec_img(x[a:b], 200, 1.0).convert("RGB")
    canvas = Image.new("RGB", (im.width, im.height + 14), (20, 20, 40))
    canvas.paste(im, (0, 14))
    d = ImageDraw.Draw(canvas)
    for k in range(int(np.ceil(t0)), int(np.floor(t1)) + 1):
        xx = (k - t0) * 200
        d.line([(xx, 14), (xx, H + 14)], fill=(80, 80, 0))
        d.text((xx + 2, 0), "%d" % k, fill=(255, 255, 0))
    for ov in overlays:
        num, _, ts = ov.partition("@")
        s = load_sfx(num)
        x0 = (float(ts) - t0) * 200
        w = len(s) / M.SR * 200
        d.rectangle([x0, 14, x0 + w, H + 14], outline=(255, 0, 0))
        d.text((x0 + 2, H), num, fill=(255, 80, 80))
    canvas.save(path)
    print("ghi", path, canvas.size)


if __name__ == "__main__":
    a = sys.argv[1:]
    if a[0] == "sfx":
        montage(a[1] if len(a) > 1 else os.path.join(OUT, "sfx_montage.png"))
    else:
        out = a[4] if len(a) > 4 else os.path.join(OUT, "seg_%s_%s.png" % (a[1], a[2]))
        seg(a[1], float(a[2]), float(a[3]), out, a[5:])
