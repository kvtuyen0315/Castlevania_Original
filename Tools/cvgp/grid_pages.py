"""Sinh trang bố cục + ảnh kỳ vọng cho phép kiểm "ảnh lưới" sprite gameplay trong Play Mode (routine CVGP 05).

  python -P -E Tools/cvgp/grid_pages.py <thư_mục_ra>

Mỗi trang 768x480 px NES, nền xám (160,160,160): (1) mọi PNG của manifest (quay trái, như sheet); (2) mọi khung anim
có lớp phủ (roi Simon) ghép thân + roi theo dữ liệu manifest, cả quay TRÁI và quay PHẢI (flipX).
Ra: page_NN.json (danh sách mục cho harness Unity: k="s" sprite | k="a" khung anim; x,y = vị trí PIVOT theo px NES;
f = 1 nếu lật) + page_NN.png (ảnh kỳ vọng ghép từ PNG bằng Pillow — độc lập với Unity).
Quy ước lật (khớp AnimSet.cs): sprite lật quanh pivot; lớp phủ quay phải đặt tại pivot + (−offset.x, offset.y) và lật.
"""
import json
import os
import sys

from PIL import Image

MAN = "Tools/cvgp/slices_gameplay.json"
ART = "Assets/Data/Art/"
PW, PH, GAP = 768, 480, 3
BG = (160, 160, 160, 255)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    doc = json.load(open(MAN, encoding="utf-8"))
    piv = {e["outDir"][len(ART):] + "/" + e["name"]: e["pivot"] for e in doc["slices"]}
    for n in ("idle_0", "walk_0", "walk_1", "walk_2", "back_0"):
        piv["Character/Simon/" + n] = [8, 32]
    img = {}

    def load(p):
        if p not in img:
            img[p] = Image.open(ART + p + ".png").convert("RGBA")
        return img[p]

    # mục: (w, h, ax, ay, entry, layers) — ax,ay = vị trí pivot trong hộp; layers = [(ảnh, dx, dy)] theo thứ tự vẽ
    items = []
    for e in doc["slices"]:
        p = e["outDir"][len(ART):] + "/" + e["name"]
        im = load(p)
        px, py = e["pivot"]
        items.append((im.width, im.height, px, py, {"k": "s", "p": p, "f": 0}, [(im, -px, -py)]))
    for a in doc["anims"]:
        if not a.get("overlays"):
            continue
        for i in range(len(a["frames"])):
            body, ov = load(a["frames"][i]), load(a["overlays"][i])
            bp = piv[a["frames"][i]]
            ox, oy = a["overlayOffsets"][2 * i], a["overlayOffsets"][2 * i + 1]
            for f in (0, 1):
                if f == 0:
                    lb = (body, -bp[0], -bp[1])
                    lo = (ov, ox, oy)
                else:
                    lb = (body.transpose(Image.FLIP_LEFT_RIGHT), -(body.width - bp[0]), -bp[1])
                    lo = (ov.transpose(Image.FLIP_LEFT_RIGHT), -ox - ov.width, oy)
                layers = [lb, lo] if a["overlayOnTop"][i] else [lo, lb]
                x0 = min(l[1] for l in layers)
                y0 = min(l[2] for l in layers)
                x1 = max(l[1] + l[0].width for l in layers)
                y1 = max(l[2] + l[0].height for l in layers)
                items.append((x1 - x0, y1 - y0, -x0, -y0,
                              {"k": "a", "set": a["set"], "anim": a["name"], "i": i, "f": f}, layers))

    # xếp kệ (giữ thứ tự manifest)
    pages, cur, x, y, row_h = [], [], GAP, GAP, 0
    for w, h, ax, ay, entry, layers in items:
        if x + w + GAP > PW:
            x, y, row_h = GAP, y + row_h + GAP, 0
        if y + h + GAP > PH:
            pages.append(cur)
            cur, x, y, row_h = [], GAP, GAP, 0
        cur.append((x + ax, y + ay, entry, layers))
        x += w + GAP
        row_h = max(row_h, h)
    if cur:
        pages.append(cur)

    for k, pg in enumerate(pages):
        canvas = Image.new("RGBA", (PW, PH), BG)
        entries = []
        for px, py, entry, layers in pg:
            for im, dx, dy in layers:
                canvas.alpha_composite(im, (px + dx, py + dy))
            entries.append(dict(entry, x=px, y=py))
        canvas.convert("RGB").save(f"{out}/page_{k:02d}.png")
        json.dump({"w": PW, "h": PH, "items": entries}, open(f"{out}/page_{k:02d}.json", "w"), indent=0)
    print(f"{len(items)} mục, {len(pages)} trang")


if __name__ == "__main__":
    main()
