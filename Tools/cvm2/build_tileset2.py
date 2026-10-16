# -*- coding: utf-8 -*-
"""
build_tileset2.py - CVM2 05: cắt địa hình Level 2 (Map 2 = Block 2, Stage 4-6) thành tileset 8x8 dedupe + CSV chỉ số tile từng section.
Dùng lại phần chung Tools/cvgp/build_tileset.py (dedupe_regions, tileset_png, rebuild_check, write_if_changed) — Level 1 không đổi.

Chạy từ gốc repo:  python -P -E Tools/cvm2/build_tileset2.py [--check]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

Đầu vào (chỉ đọc): Art_Source/spriters-resource/level2_main_hall.png (sha256 CVM2_MEMORY §3b) + rect 6 vùng =
                    ClaudeNotes/cvm2/inventory/level2_main_hall.json (kiểm khớp bảng SECTIONS dưới đây).
Vá trước khi cắt (H2 - không vẽ mới, chỉ CHÉP ô có sẵn): 3 ô teal (0,128,128) 32x64 ở TowerHallC = chỗ sheet chừa cho bẫy ép
  (không phải tile thật) ⇒ thay bằng tile ĐEN có sẵn (vòm đen quanh bẫy). Bằng chứng video Map_2 2:33-2:41 (Tools/cvm2/terrain5.py teal):
  mọi pixel teal đều ĐEN ở video khi bẫy không che (trừ 11-12 px/bẫy ở y 80-87 luôn bị xích bẫy che - cùng vòm đen).
Đầu ra:
  Assets/Data/Art/Level/Block2/level2_tiles.png        tileset 16 tile/hàng (128 px), ô thừa trong suốt
  Assets/Data/Config/Level/Block2/<Section>_tiles.csv  ma trận chỉ số tile (hàng = 8 px từ TRÊN xuống)
  Assets/Data/Config/Level/Block2/Level2Tiles.json     mô tả tileset + section (CvgpLevelBuilder đọc, menu CVM2/Build Map2 Tilemaps)
Tự kiểm: dựng lại từ tileset + CSV ⇒ 0 pixel lệch so vùng đã vá; so sheet gốc lệch ĐÚNG bằng số pixel teal (6144);
số tile = kiểm kê 82 − 1 (tile teal thuần bỏ đi; tile đen thay vào đã có) = 81. Chạy 2 lần ⇒ "same".
"""
import json
import os
import sys

import numpy as np
from PIL import Image

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(TOOLS_DIR))
sys.path.insert(0, os.path.join(REPO, "Tools", "cvgp"))
from build_tileset import (TILE, COLUMNS, sha256, dedupe_regions, tileset_png, rebuild_check,  # noqa: E402
                           write_if_changed)

SHEET = os.path.join(REPO, "Art_Source", "spriters-resource", "level2_main_hall.png")
SHEET_SHA256 = "6d09c60ef9b25ad19fe7138779030a0505d5c7d3b15a5599ea001d060b9313fd"
INVENTORY = os.path.join(REPO, "ClaudeNotes", "cvm2", "inventory", "level2_main_hall.json")
ART_DIR = "Assets/Data/Art/Level/Block2"
CFG_DIR = "Assets/Data/Config/Level/Block2"
PNG_REL = ART_DIR + "/level2_tiles.png"
JSON_REL = CFG_DIR + "/Level2Tiles.json"
INV_UNIQUE_TOTAL = 82   # kiểm kê planner/CVM2 01 (gồm 1 tile teal thuần)
EXPECTED_UNIQUE = 81    # sau khi vá teal ⇒ đen (tile đen đã có)

# id Unity - rect sheet [x, y, w, h] - stage (thứ tự quét = thứ tự đi trong video Map_2)
SECTIONS = [
    ("TowerEntranceA", [1803, 632, 256, 192], 4),
    ("TowerEntranceB", [1547, 425, 512, 192], 4),
    ("TowerHallA", [774, 425, 768, 192], 5),
    ("TowerHallB", [774, 218, 768, 192], 5),
    ("TowerHallC", [1, 218, 768, 192], 6),
    ("Medusa", [1, 11, 768, 192], 6),
]
TEAL = (0, 128, 128)
BLACK = (0, 0, 0)
# vùng bẫy ép (px section) - teal ⇒ đen; kiểm: đúng 3 ô, mỗi ô 32x64 toàn teal, ngoài 3 ô không còn teal
PATCHES = {"TowerHallC": [[352, 80, 32, 64], [416, 80, 32, 64], [480, 80, 32, 64]]}
PATCH_SRC = ("CVM2 05: 3 o teal 32x64 = cho sheet chua cho bay ep gai (Object/crusher_0..7) -> den (tile den co san, vom den quanh bay). "
             "Bang chung video Map_2 2:33-2:41 + 3:04-3:16: Tools/cvm2/terrain5.py teal - moi pixel teal deu den o video khi bay khong che "
             "(11-12 px/bay o y 80-87 luon bi xich bay che); anh _cache/work/sheet_map2_crusher_00.png")


def load_regions():
    if sha256(SHEET) != SHEET_SHA256:
        sys.exit("sheet level2_main_hall.png sai sha256")
    inv = json.load(open(INVENTORY, encoding="utf-8"))
    inv_rects = {r["name"]: r["rect"] for r in inv["regions"]}
    inv_unique = {r["name"]: r["unique8"] for r in inv["regions"]}
    if inv["total_unique8"] != INV_UNIQUE_TOTAL:
        sys.exit("kiem ke doi: total_unique8 %d" % inv["total_unique8"])
    a = np.asarray(Image.open(SHEET).convert("RGBA"))
    rgb = a[..., :3]
    raw = {}
    regions = []
    teal_total = 0
    for sid, (x, y, w, h), _stage in SECTIONS:
        if inv_rects.get(sid) != [x, y, w, h]:
            sys.exit("rect %s lech kiem ke: %s" % (sid, inv_rects.get(sid)))
        if (a[y:y + h, x:x + w, 3] != 255).any():
            sys.exit("%s co pixel khong duc" % sid)
        reg = rgb[y:y + h, x:x + w]
        if ((reg == (0, 128, 0)).all(axis=2)).any():
            sys.exit("%s co pixel nen xanh ben trong" % sid)
        raw[sid] = reg
        fixed = reg.copy()
        teal = (reg == TEAL).all(axis=2)
        for (px, py, pw, ph) in PATCHES.get(sid, []):
            box = teal[py:py + ph, px:px + pw]
            if not box.all():
                sys.exit("%s: o va %s khong toan teal" % (sid, [px, py, pw, ph]))
            fixed[py:py + ph, px:px + pw] = BLACK
            teal_total += int(box.sum())
        if (fixed == TEAL).all(axis=2).any():
            sys.exit("%s: con teal ngoai o va" % sid)
        regions.append((sid, fixed))
    return raw, regions, inv_unique, teal_total


def patched_regions():
    """{id: vùng RGB đã vá} — build_collision2.py / accept5.py dùng (cùng ảnh mà tileset dựng lại 0 lệch)."""
    _raw, regions, _u, _t = load_regions()
    return dict(regions)


def build():
    raw, regions, inv_unique, teal_total = load_regions()
    tiles, maps = dedupe_regions(regions)
    png = tileset_png(tiles)
    per, diff_total = rebuild_check(png, regions, maps)
    # so với sheet GỐC: lệch đúng bằng số pixel teal đã vá
    _per_raw, diff_raw = rebuild_check(png, [(sid, raw[sid]) for sid, _r in regions], maps)
    black_id = None
    for k, t in enumerate(tiles):
        if not t.any():
            black_id = k
            break
    for sid, _rect, _st in SECTIONS:
        want = inv_unique[sid] - (1 if sid in PATCHES else 0)   # Hall C: bỏ tile teal, tile đen đã có trong vùng
        if per[sid][1] != want:
            sys.exit("%s: so tile duy nhat %d khac kiem ke (%d)" % (sid, per[sid][1], want))
    return tiles, maps, png, per, diff_total, diff_raw, teal_total, black_id


def main():
    check = "--check" in sys.argv
    tiles, maps, png, per, diff_total, diff_raw, teal_total, black_id = build()
    n = len(tiles)
    print("tiles 8x8 duy nhat:", n, "(ky vong %d = kiem ke %d - 1 tile teal)" % (EXPECTED_UNIQUE, INV_UNIQUE_TOTAL))
    for sid, (d, u) in per.items():
        print("  %-15s unique %3d  pixel lech khi dung lai: %d" % (sid, u, d))
    print("TONG pixel lech (so vung da va):", diff_total, "| so sheet goc:", diff_raw, "(pixel teal da va: %d) | tile den = %s" % (teal_total, black_id))
    if n != EXPECTED_UNIQUE or diff_total != 0 or diff_raw != teal_total or teal_total != 6144 or black_id is None:
        sys.exit("LOI: so tile / pixel lech khong dat - khong ghi file")
    report = []
    write_if_changed(PNG_REL, png, check, report)
    meta = {
        "note": "CVM2 05 - sinh boi Tools/cvm2/build_tileset2.py; KHONG sua tay",
        "sheet": "Art_Source/spriters-resource/level2_main_hall.png",
        "sheetSha256": SHEET_SHA256,
        "tileSize": TILE,
        "columns": COLUMNS,
        "count": n,
        "png": PNG_REL,
        "levelRowToNesY": 32,
        "patches": [{"section": sid, "rect": r, "from": list(TEAL), "to": list(BLACK), "tile": black_id, "src": PATCH_SRC}
                    for sid, rs in PATCHES.items() for r in rs],
        "sections": [],
    }
    for sid, rect, stage in SECTIONS:
        m = maps[sid]
        csv_rel = CFG_DIR + "/" + sid + "_tiles.csv"
        txt = "\n".join(",".join(str(int(v)) for v in row) for row in m) + "\n"
        write_if_changed(csv_rel, txt.encode("ascii"), check, report)
        meta["sections"].append({"id": sid, "inventoryId": sid, "stage": stage, "sheetRect": rect,
                                 "width": rect[2], "height": rect[3], "cols8": int(m.shape[1]), "rows8": int(m.shape[0]),
                                 "uniqueTiles": per[sid][1], "csv": csv_rel})
    js = json.dumps(meta, ensure_ascii=True, indent=1) + "\n"
    write_if_changed(JSON_REL, js.encode("ascii"), check, report)
    for line in report:
        print(line)
    print("CHECK (khong ghi)" if check else "OK")


if __name__ == "__main__":
    main()
