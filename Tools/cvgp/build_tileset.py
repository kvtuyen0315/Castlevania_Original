# -*- coding: utf-8 -*-
"""
build_tileset.py - CVGP 06: cắt địa hình Level 1 (Map 1) thành tileset 8x8 dedupe + CSV chỉ số tile từng section.

Chạy từ gốc repo:  python -P -E Tools/cvgp/build_tileset.py [--check]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

Đầu vào (chỉ đọc): Art_Source/spriters-resource/level1_entrance.png (sha256 CVGP_MEMORY §3)
                    + rect 5 vùng = ClaudeNotes/cvgp/inventory/level1_entrance.json (kiểm khớp bảng SECTIONS dưới đây).
Đầu ra:
  Assets/Data/Art/Level/Block1/level1_tiles.png        tileset 16 tile/hàng (128 px), ô trống trong suốt (0,0,0,0)
  Assets/Data/Config/Level/Block1/<Section>_tiles.csv  ma trận chỉ số tile (hàng = 8 px từ TRÊN xuống, cột = 8 px từ trái)
  Assets/Data/Config/Level/Block1/Level1Tiles.json     mô tả tileset + section (CvgpLevelBuilder đọc)
Thứ tự tile: quét section theo thứ tự sheet (CE, EH, CA, CB, PB), mỗi section hàng trên→dưới, cột trái→phải; tile chưa gặp
⇒ chỉ số kế tiếp. KHÔNG gộp lật (giữ đúng pixel, 118 tile).
Tự kiểm: dựng lại từng vùng từ tileset + CSV ⇒ số pixel lệch phải = 0; số tile = 118 (kiểm kê planner); --check chỉ kiểm, không ghi.
Chạy 2 lần ⇒ md5 đầu ra y hệt (PNG compress_level 9, không metadata; JSON/CSV '\n').
"""
import hashlib
import io
import json
import os
import sys

import numpy as np
from PIL import Image

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(TOOLS_DIR))
SHEET = os.path.join(REPO, "Art_Source", "spriters-resource", "level1_entrance.png")
SHEET_SHA256 = "f9f8e348837db0b64f3606dfb9b6cb80daf571791fce3771e13fd50997d90a2f"
INVENTORY = os.path.join(REPO, "ClaudeNotes", "cvgp", "inventory", "level1_entrance.json")
ART_DIR = "Assets/Data/Art/Level/Block1"
CFG_DIR = "Assets/Data/Config/Level/Block1"
PNG_REL = ART_DIR + "/level1_tiles.png"
JSON_REL = CFG_DIR + "/Level1Tiles.json"

TILE = 8
COLUMNS = 16          # tile mỗi hàng của tileset
EXPECTED_UNIQUE = 118  # kiểm kê planner (SHEETS_INVENTORY §5.2)

# id Unity (PascalCase) - id kiểm kê - rect sheet [x, y, w, h] - stage
SECTIONS = [
    ("CastleEntrance", "castle_entrance", [1, 11, 768, 192], 1),
    ("EntranceHall", "entrance_hall", [774, 11, 1536, 192], 1),
    ("CryptA", "crypt_a", [2315, 11, 512, 192], 2),
    ("CryptB", "crypt_b", [2315, 218, 512, 192], 2),
    ("PhantomBat", "phantom_bat", [2832, 11, 768, 192], 3),
]


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        h.update(f.read())
    return h.hexdigest()


def png_bytes(img):
    buf = io.BytesIO()
    img.save(buf, format="PNG", compress_level=9, optimize=False)
    return buf.getvalue()


def write_if_changed(rel, data, check, report):
    path = os.path.join(REPO, rel)
    old = None
    if os.path.exists(path):
        with open(path, "rb") as f:
            old = f.read()
    state = "same" if old == data else ("new" if old is None else "changed")
    report.append("%-7s %s (%d B, md5 %s)" % (state, rel, len(data), hashlib.md5(data).hexdigest()))
    if check or state == "same":
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)


def dedupe_regions(regions, tile=TILE):
    """regions: list (id, ndarray HxWx3) theo thứ tự quét ⇒ (tiles, maps): tile 8x8 duy nhất (KHÔNG gộp lật) + ma trận chỉ số từng vùng.
    Dùng chung CVGP 06 (Level 1) và CVM2 05 (Tools/cvm2/build_tileset2.py, Level 2)."""
    tiles = []          # list ndarray 8x8x3
    index = {}          # bytes -> id
    maps = {}
    for sid, reg in regions:
        h, w = reg.shape[:2]
        rows, cols = h // tile, w // tile
        m = np.zeros((rows, cols), np.int32)
        for j in range(rows):
            for i in range(cols):
                t = reg[j * tile:(j + 1) * tile, i * tile:(i + 1) * tile]
                k = t.tobytes()
                if k not in index:
                    index[k] = len(tiles)
                    tiles.append(t.copy())
                m[j, i] = index[k]
        maps[sid] = m
    return tiles, maps


def tileset_png(tiles, columns=COLUMNS, tile=TILE):
    """PNG tileset `columns` tile/hàng, ô thừa trong suốt (0,0,0,0)."""
    n = len(tiles)
    trows = (n + columns - 1) // columns
    ts = np.zeros((trows * tile, columns * tile, 4), np.uint8)
    for k, t in enumerate(tiles):
        r, c = divmod(k, columns)
        ts[r * tile:(r + 1) * tile, c * tile:(c + 1) * tile, :3] = t
        ts[r * tile:(r + 1) * tile, c * tile:(c + 1) * tile, 3] = 255
    return png_bytes(Image.fromarray(ts, "RGBA"))


def rebuild_check(png, regions, maps, columns=COLUMNS, tile=TILE):
    """Tự kiểm: dựng lại từng vùng từ tileset (đọc lại từ PNG đã mã hoá) + ma trận ⇒ {id: (pixel lệch, số tile duy nhất)}, tổng lệch."""
    ts2 = np.asarray(Image.open(io.BytesIO(png)).convert("RGBA"))
    diff_total = 0
    per = {}
    for sid, reg in regions:
        h, w = reg.shape[:2]
        m = maps[sid]
        out = np.zeros((h, w, 3), np.uint8)
        for j in range(m.shape[0]):
            for i in range(m.shape[1]):
                r, c = divmod(int(m[j, i]), columns)
                out[j * tile:(j + 1) * tile, i * tile:(i + 1) * tile] = ts2[r * tile:(r + 1) * tile, c * tile:(c + 1) * tile, :3]
        d = int((out != reg).any(axis=2).sum())
        per[sid] = (d, len(set(m.flatten().tolist())))
        diff_total += d
    return per, diff_total


def build():
    if sha256(SHEET) != SHEET_SHA256:
        sys.exit("sheet level1_entrance.png sai sha256")
    inv = json.load(open(INVENTORY, encoding="utf-8"))
    inv_rects = {r["id"]: r["rect"] for r in inv["regions"]}
    inv_unique = {r["id"]: r["tiles"]["unique8"] for r in inv["regions"]}
    a = np.asarray(Image.open(SHEET).convert("RGBA"))
    if (a[..., 3] != 255).any():
        sys.exit("sheet có pixel không đục")
    rgb = a[..., :3]

    regions = []
    for sid, iid, (x, y, w, h), _stage in SECTIONS:
        if inv_rects.get(iid) != [x, y, w, h]:
            sys.exit("rect %s lech kiem ke: %s" % (sid, inv_rects.get(iid)))
        reg = rgb[y:y + h, x:x + w]
        # vùng phải đặc (không có nền xanh sheet) và viền 1 px quanh vùng = nền xanh
        if ((reg == (0, 128, 0)).all(axis=2)).any():
            sys.exit("%s co pixel nen xanh ben trong" % sid)
        regions.append((sid, reg))
    tiles, maps = dedupe_regions(regions)
    # tự kiểm: dựng lại vùng từ tileset (đọc lại từ mảng PNG đã mã hoá) + ma trận
    png = tileset_png(tiles)
    per, diff_total = rebuild_check(png, regions, maps)
    for sid, iid, _rect, _stage in SECTIONS:
        if per[sid][1] != inv_unique[iid]:
            sys.exit("%s: so tile duy nhat %d khac kiem ke %d" % (sid, per[sid][1], inv_unique[iid]))
    return tiles, maps, png, per, diff_total


def main():
    check = "--check" in sys.argv
    tiles, maps, png, per, diff_total = build()
    n = len(tiles)
    print("tiles 8x8 duy nhat:", n, "(ky vong %d)" % EXPECTED_UNIQUE)
    for sid, (d, u) in per.items():
        print("  %-15s unique %3d  pixel lech khi dung lai: %d" % (sid, u, d))
    print("TONG pixel lech:", diff_total)
    if n != EXPECTED_UNIQUE or diff_total != 0:
        sys.exit("LOI: so tile hoac pixel lech khong dat - khong ghi file")

    report = []
    write_if_changed(PNG_REL, png, check, report)
    meta = {
        "note": "CVGP 06 - sinh boi Tools/cvgp/build_tileset.py; KHONG sua tay",
        "sheet": "Art_Source/spriters-resource/level1_entrance.png",
        "sheetSha256": SHEET_SHA256,
        "tileSize": TILE,
        "columns": COLUMNS,
        "count": n,
        "png": PNG_REL,
        "levelRowToNesY": 32,
        "sections": [],
    }
    for sid, iid, rect, stage in SECTIONS:
        m = maps[sid]
        csv_rel = CFG_DIR + "/" + sid + "_tiles.csv"
        txt = "\n".join(",".join(str(int(v)) for v in row) for row in m) + "\n"
        write_if_changed(csv_rel, txt.encode("ascii"), check, report)
        meta["sections"].append({"id": sid, "inventoryId": iid, "stage": stage, "sheetRect": rect,
                                 "width": rect[2], "height": rect[3], "cols8": int(m.shape[1]), "rows8": int(m.shape[0]),
                                 "uniqueTiles": per[sid][1], "csv": csv_rel})
    js = json.dumps(meta, ensure_ascii=True, indent=1) + "\n"
    write_if_changed(JSON_REL, js.encode("ascii"), check, report)
    for line in report:
        print(line)
    print("CHECK (khong ghi)" if check else "OK")


if __name__ == "__main__":
    main()
