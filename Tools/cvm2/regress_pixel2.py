"""Chấm ảnh Play của RegressionProbe.StepShotsAt (CVM2 07): pixel-perfect khi camera cuộn TRÁI hoặc PHẢI trong section Block 2.

Như Tools/cvgp/regress_pixel.py (CVGP 10) nhưng mức dịch có DẤU (cửa trái / đi trái ⇒ cam giảm).
Dùng:  python -P -E Tools/cvm2/regress_pixel2.py <thư_mục_ảnh> <tag> <S> <X> <Y> <W> <H>
  vd 1920×1080 scale 4: ... shots/07/px4x s4 4 448 60 1920 1080   (1280×720 S=3: 256 0 · 2560×1440 S=6: 512 0)
Thư mục có <tag>_NNNN.png (NNNN = tick tương đối) và log.csv (StepLog: rel,x,y,cam,simonWorldX*16,simonWorldY*16,camWorldX*16).
Kiểm: (1) đúng cỡ; (2) mọi khối S×S đồng nhất; (3) ngoài vùng game đen đặc; cho mỗi cặp liền nhau (4) nền dịch ĐÚNG cam[t+1]−cam[t] px NES
(so hàng 56–231, bỏ cột Simon 100–156) và mức dịch tốt nhất trùng; từ log (5) transform nguyên px NES, (6) Simon − camera = hằng — (5)(6) chỉ trong cửa sổ ảnh.
Thoát mã 1 nếu có lỗi.
"""
import glob
import os
import re
import sys

from PIL import Image, ImageChops


def mismatch(a, b, d):
    """số pixel khác nhau giữa 2 khung liền nhau khi nền dịch d px (d>0: b[x] == a[x+d] — cam tăng; d<0: b[x+|d|] == a[x] — cam giảm)."""
    k = abs(d)
    if d >= 0:
        ca = a.crop((d, 56, 256, 232))
        cb = b.crop((0, 56, 256 - d, 232))
    else:
        ca = a.crop((0, 56, 256 - k, 232))
        cb = b.crop((k, 56, 256, 232))
    diff = ImageChops.difference(ca, cb).convert("L").point(lambda v: 255 if v else 0)
    # bỏ cột quanh Simon (toạ độ cột của crop — Simon ở 100–156 trong ảnh b; d<0 crop b bắt đầu từ k nên trừ k)
    lo = 100 - (k if d < 0 else 0)
    hi = 157 - (k if d < 0 else 0)
    diff.paste(0, (max(0, lo), 0, min(hi, diff.size[0]), diff.size[1]))
    return diff.histogram()[255]


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    d, tag = sys.argv[1], sys.argv[2]
    S, X, Y, W, H = map(int, sys.argv[3:8])
    log = {}
    for line in open(os.path.join(d, "log.csv")).read().split("\n"):
        if line.strip():
            f = line.split(",")
            log[int(f[0])] = f
    files = sorted(glob.glob(os.path.join(d, tag + "_*.png")))
    rels = [int(re.search(r"_(\d+)\.png$", f).group(1)) for f in files]
    errors = 0
    nes = []
    for f, rel in zip(files, rels):
        im = Image.open(f).convert("RGBA")
        if im.size != (W, H):
            print(f"{rel}: kích thước {im.size} != {(W, H)}")
            errors += 1
            nes.append(None)
            continue
        crop = im.crop((X, Y, X + 256 * S, Y + 240 * S))
        small = crop.resize((256, 240), Image.NEAREST)
        up = small.resize(crop.size, Image.NEAREST)
        blur = ImageChops.difference(crop, up).getbbox()
        ref = Image.new("RGBA", (W, H), (0, 0, 0, 255))
        ref.paste(crop, (X, Y))
        outside = ImageChops.difference(im, ref).getbbox()
        if blur is not None or outside is not None:
            print(f"{rel}: khối không đồng nhất bbox={blur}, ngoài vùng không đen bbox={outside}")
            errors += 1
        nes.append(small.convert("RGB"))
    print(f"{tag}: {len(files)} ảnh, S={S}, vùng game ({X},{Y}), lỗi khối/ngoài vùng: {errors}")

    pairs = shift_ok = best_ok = 0
    mism_total = 0
    moving = 0
    for i in range(len(files) - 1):
        if rels[i + 1] != rels[i] + 1 or nes[i] is None or nes[i + 1] is None:
            continue
        pairs += 1
        dcam = int(log[rels[i + 1]][3]) - int(log[rels[i]][3])
        if dcam != 0:
            moving += 1
        a, b = nes[i], nes[i + 1]
        best, bestm = None, None
        m_log = None
        for sh in range(-4, 5):
            m = mismatch(a, b, sh)
            if bestm is None or m < bestm:
                best, bestm = sh, m
            if sh == dcam:
                m_log = m
        mism_total += m_log
        if m_log == 0:
            shift_ok += 1
        else:
            print(f"cặp {rels[i]}→{rels[i + 1]}: dịch theo log {dcam} lệch {m_log} pixel (tốt nhất {best}, {bestm})")
        if best == dcam or bestm == m_log:
            best_ok += 1
    print(f"{tag}: {pairs} cặp liền nhau ({moving} cặp camera đang cuộn), nền dịch đúng số px nguyên theo log: {shift_ok}/{pairs}, "
          f"tổng pixel lệch {mism_total}, mức dịch tốt nhất khớp log: {best_ok}/{pairs}")
    errors += (pairs - shift_ok)

    nonint = 0
    rel_set = set()
    win = {r: f for r, f in log.items() if rels and rels[0] <= r <= rels[-1]}   # chỉ cửa sổ ảnh (ngoài đó camera đứng/khoá nên Simon − camera đổi là đúng)
    for r, f in win.items():
        sx, sy, cx = float(f[4]), float(f[5]), float(f[6])
        for v in (sx, sy, cx):
            if abs(v - round(v)) > 1e-3:
                nonint += 1
        rel_set.add(round(sx - cx, 3))
    print(f"{tag}: log {len(log)} tick (cửa sổ {len(win)}); transform không nguyên px: {nonint}; (Simon − camera) khác nhau: {sorted(rel_set)}")
    errors += nonint + (len(rel_set) - 1)
    print("KẾT QUẢ", "ĐẠT" if errors == 0 else f"LỖI {errors}")
    sys.exit(1 if errors else 0)


if __name__ == "__main__":
    main()
