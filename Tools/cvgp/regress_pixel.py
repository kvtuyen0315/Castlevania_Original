"""Chấm ảnh Play của RegressionProbe.StepShots (CVGP 10): pixel-perfect khi Simon đi + camera cuộn.

Dùng:  python -P -E regress_pixel.py <thư_mục_ảnh> <tag> <S> <X> <Y> <W> <H>
  vd 1920×1080 scale 4: ... shots/10/px4x s4 4 448 60 1920 1080
Thư mục có <tag>_NNNN.png (NNNN = tick tương đối) và log.csv (StepLog: rel,x,y,cam,simonWorldX*16,simonWorldY*16,camWorldX*16).
Kiểm cho mỗi ảnh: (1) đúng cỡ W×H; (2) mọi khối S×S trong vùng game đồng nhất (không nhoè); (3) ngoài vùng game đen ĐẶC (alpha 255);
cho mỗi cặp tick liền nhau: (4) nền dịch ĐÚNG số px NES nguyên bằng cam[t+1]−cam[t] (so mọi pixel hàng 56–231, bỏ cột quanh Simon 100–156),
và mức dịch tốt nhất (0..4) trùng mức theo log; từ log: (5) vị trí transform Simon/camera là số nguyên px NES, (6) Simon − camera = hằng (không rung).
Thoát mã 1 nếu có lỗi.
"""
import glob
import os
import re
import sys

from PIL import Image, ImageChops


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
    for i in range(len(files) - 1):
        if rels[i + 1] != rels[i] + 1 or nes[i] is None or nes[i + 1] is None:
            continue
        pairs += 1
        dcam = int(log[rels[i + 1]][3]) - int(log[rels[i]][3])
        a, b = nes[i], nes[i + 1]
        best, bestm = None, None
        for sh in range(0, 5):
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
    print(f"{tag}: {pairs} cặp liền nhau, nền dịch đúng số px nguyên theo log: {shift_ok}/{pairs}, tổng pixel lệch {mism_total}, mức dịch tốt nhất khớp log: {best_ok}/{pairs}")
    errors += (pairs - shift_ok)

    nonint = 0
    rel_set = set()
    for r, f in log.items():
        sx, sy, cx = float(f[4]), float(f[5]), float(f[6])
        for v in (sx, sy, cx):
            if abs(v - round(v)) > 1e-3:
                nonint += 1
        rel_set.add(round(sx - cx, 3))
    print(f"{tag}: log {len(log)} tick; transform không nguyên px: {nonint}; (Simon − camera) khác nhau: {sorted(rel_set)}")
    errors += nonint + (len(rel_set) - 1)
    print("KẾT QUẢ", "ĐẠT" if errors == 0 else f"LỖI {errors}")
    sys.exit(1 if errors else 0)


def mismatch(a, b, d):
    """số pixel khác nhau giữa b và a dịch trái d (b[x] == a[x+d]) ở hàng 56–231, cột 0..255−d, bỏ cột 100–156."""
    w = 256 - d
    ca = a.crop((d, 56, 256, 232))
    cb = b.crop((0, 56, w, 232))
    diff = ImageChops.difference(ca, cb).convert("L").point(lambda v: 255 if v else 0)
    # bỏ cột quanh Simon (cột ảnh b)
    diff.paste(0, (100, 0, min(157, w), diff.size[1]))
    return diff.histogram()[255]


if __name__ == "__main__":
    main()
