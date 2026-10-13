"""Cắt sprite CVTI từ sheet gốc theo manifest Tools/cvti/slices.json (nguồn sự thật).

Mỗi mục: crop `rect` → tô đen các vùng `fill` (kiểm vùng đó chỉ chứa màu `allow`) → màu `keyColor` thành
trong suốt (0,0,0,0) → kiểm → ghi `<outDir>/<name>.png` (RGBA, không metadata ⇒ chạy lại ra byte y hệt).
Kiểm (lỗi nào cũng dừng, không ghi đè nửa chừng):
  - rect nằm trong sheet; 0 pixel màu nền sheet (`sheetBg`) trong sprite ⇒ rect không ăn vào lưới/nền
    (trừ mục `bgIsKey: true` — ô hợp tile 8x16 có góc trống màu nền sheet trong bbox: nền sheet cũng thành trong suốt);
  - `edgeCheck` (mặc định bật): viền 1 px ngay ngoài rect toàn màu nền sheet ⇒ rect không hụt mép ô;
  - `forbidColors`: 0 pixel các màu này sau khi tô; `allowColors`: chỉ có các màu này (trước khi key);
  - `alsoAt`: các chỗ khác trên sheet (cùng w×h) phải trùng từng pixel (glyph font lặp lại).
  - `recolor`: [[từ, thành], …] đổi màu ngay sau `fill` (chuẩn hoá palette giữa các sheet rip khác nhau,
    vd trắng (255,255,254) của sheet Text Font → (252,252,252) như sheet Title); `allowColors` kiểm SAU khi đổi.
Cần Pillow (máy này cài ở user site ⇒ chạy `python -P -E`, KHÔNG `-I` vì `-I` bỏ user site):
  python -P -E Tools/cvti/slice_sprites.py [--check] [--manifest <path>]
    --check: chỉ kiểm, không ghi file; --manifest: manifest khác (mặc định Tools/cvti/slices.json; gameplay CVGP =
    Tools/cvgp/slices_gameplay.json). Trường lạ trong mục (pivot, src, group…) được bỏ qua.
Chạy từ thư mục gốc repo. In md5 từng file để so hai lần chạy.
"""
import hashlib
import io
import json
import os
import sys

from PIL import Image

MANIFEST = "Tools/cvti/slices.json"


def rgb(p):
    return tuple(p[:3])


def crop_pixels(sheet, rect):
    x, y, w, h = rect
    if x < 0 or y < 0 or x + w > sheet.width or y + h > sheet.height:
        raise ValueError(f"rect {rect} ra ngoài sheet {sheet.width}x{sheet.height}")
    return sheet.crop((x, y, x + w, y + h))


def edge_ring(sheet, rect):
    x, y, w, h = rect
    pts = [(i, y - 1) for i in range(x - 1, x + w + 1)] + [(i, y + h) for i in range(x - 1, x + w + 1)]
    pts += [(x - 1, j) for j in range(y, y + h)] + [(x + w, j) for j in range(y, y + h)]
    return [rgb(sheet.getpixel(p)) for p in pts if 0 <= p[0] < sheet.width and 0 <= p[1] < sheet.height]


def process(e, sheet, bg):
    errs = []
    img = crop_pixels(sheet, e["rect"])
    px = img.load()
    w, h = img.size

    for f in e.get("fill", []):
        fx, fy, fw, fh = f["rect"]
        allow = {tuple(c) for c in f["allow"]}
        bad = sum(1 for j in range(fy, fy + fh) for i in range(fx, fx + fw) if rgb(px[i, j]) not in allow)
        if bad:
            errs.append(f"fill {f['rect']}: {bad} pixel ngoài màu cho phép (sẽ ăn vào hình khác)")
        for j in range(fy, fy + fh):
            for i in range(fx, fx + fw):
                px[i, j] = (0, 0, 0, 255)

    for src, dst in e.get("recolor", []):
        src, dst = tuple(src), tuple(dst)
        for j in range(h):
            for i in range(w):
                if rgb(px[i, j]) == src:
                    px[i, j] = dst + (255,)

    colors = {}
    for j in range(h):
        for i in range(w):
            c = rgb(px[i, j])
            colors[c] = colors.get(c, 0) + 1

    if bg in colors and not e.get("bgIsKey"):
        errs.append(f"{colors[bg]} pixel màu nền sheet {bg} trong sprite (rect dính lưới/nền)")
    for c in e.get("forbidColors", []):
        if tuple(c) in colors:
            errs.append(f"còn {colors[tuple(c)]} pixel màu cấm {tuple(c)}")
    if "allowColors" in e:
        extra = set(colors) - {tuple(c) for c in e["allowColors"]}
        if extra:
            errs.append(f"màu ngoài danh sách cho phép: {sorted(extra)}")
    if e.get("edgeCheck", True):
        ring = edge_ring(sheet, e["rect"])
        off = sum(1 for c in ring if c != bg)
        if off:
            errs.append(f"viền ngoài rect có {off}/{len(ring)} pixel không phải nền sheet (rect hụt/lệch mép ô)")
    for at in e.get("alsoAt", []):
        other = crop_pixels(sheet, [at[0], at[1], w, h])
        if other.tobytes() != crop_pixels(sheet, e["rect"]).tobytes():
            errs.append(f"bản lặp tại {at} KHÁC bản gốc {e['rect'][:2]}")

    key = tuple(e["keyColor"]) if e.get("keyColor") else None
    keyed = 0
    for j in range(h):
        for i in range(w):
            if (key is not None and rgb(px[i, j]) == key) or (e.get("bgIsKey") and rgb(px[i, j]) == bg):
                px[i, j] = (0, 0, 0, 0)
                keyed += 1
            else:
                r, g, b, _ = px[i, j]
                px[i, j] = (r, g, b, 255)
    if key is not None:
        if keyed == 0:
            errs.append(f"keyColor {key} không có pixel nào (sai màu nền?)")
        if keyed == w * h:
            errs.append("key xoá sạch sprite")
    opaque = w * h - keyed
    return img, errs, {"colors": len(colors), "keyed": keyed, "opaque": opaque}


def encode(img):
    buf = io.BytesIO()
    img.save(buf, format="PNG", optimize=False, compress_level=9)
    return buf.getvalue()


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    check_only = "--check" in sys.argv
    manifest = sys.argv[sys.argv.index("--manifest") + 1] if "--manifest" in sys.argv else MANIFEST
    doc = json.load(open(manifest, encoding="utf-8"))
    bg = tuple(doc["sheetBg"])
    sheets = {}
    outputs = []
    failed = False
    for e in doc["slices"]:
        path = e["sheet"]
        if path not in sheets:
            sheets[path] = Image.open(path).convert("RGBA")
        img, errs, info = process(e, sheets[path], bg)
        out = f"{e['outDir']}/{e['name']}.png"
        if errs:
            failed = True
            for m in errs:
                print(f"LỖI {e['name']}: {m}")
        outputs.append((out, encode(img), info))
    if failed:
        print("Có lỗi — không ghi file nào.")
        sys.exit(1)
    names = [o for o, _, _ in outputs]
    if len(set(n.lower() for n in names)) != len(names):
        sys.exit("Trùng tên file đầu ra.")
    for out, data, info in outputs:
        if not check_only:
            os.makedirs(os.path.dirname(out), exist_ok=True)
            with open(out, "wb") as f:
                f.write(data)
        print(f"{hashlib.md5(data).hexdigest()}  {out}  (màu {info['colors']}, trong suốt {info['keyed']}, đục {info['opaque']})")
    print(f"{'KIỂM' if check_only else 'GHI'} {len(outputs)} sprite, 0 lỗi.")


if __name__ == "__main__":
    main()
