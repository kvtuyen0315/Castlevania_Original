# -*- coding: utf-8 -*-
"""
build_arena.py - CVGP 07: section thử "Arena" (scene Assets/Scenes/Test/Arena.unity) GHÉP từ ô có sẵn của Map 1.
KHÔNG vẽ mới (H2): chỉ chép ô tile 8x8 (chỉ số tileset level1_tiles.png) + ký tự va chạm + cầu thang của Level1Map.json.

Chạy từ gốc repo (SAU build_tileset.py + build_collision.py):  python -P -E Tools/cvgp/build_arena.py [--check]
  rồi menu Unity CVGP/Build Arena Section (CvgpLevelBuilder) -> Assets/Data/Config/Level/Arena/Arena.asset + Assets/Data/Prefabs/Level/Test/Arena.prefab
Ra:  Assets/Data/Config/Level/Arena/{ArenaTiles.json, Arena_tiles.csv, ArenaMap.json}
     ClaudeNotes/cvgp/ref/arena_preview.png (2x, overlay va chạm + bậc thang + chú thích)

Bố cục (ô 16x16; 65 cột = 1040 px = 4 màn + 16; 12 hàng = 192 px như Map 1):
  nền  : EntranceHall cột 0-64 nguyên trạng — sàn hàng 11 (mặt sàn y 176); stair0 ↗ 8 bậc (sàn -> bục y112, cột 43-45),
         stair1 ↗ 4 bậc (bục y112 -> bục y80, cột 47-56), stair2 ↖ 4 bậc (bục y80 xuống bục y112, cột 58-63); khối phá (63,7)
  sửa  : tường thấp 32 px  cột 26 hàng 9-10   (x 416-431)  <- gạch bục EH (44,7)       [kiểm đụng tường / xuyên tường]
         hố                cột 30-31 hàng 11   (x 480-511)  <- nền EH cùng cột hàng 10   [rơi khỏi đáy section = chết rơi]
         nước              cột 34-35 hàng 11   (x 544-575)  <- mặt nước Crypt B (0-1,10) [chạm nước = chết]
  sàn trống x 0-415 đủ cho kiểm "đi phải 300 tick" từ x 48; camera cuộn 0..784.
"""
import json
import os
import sys

from PIL import Image, ImageDraw

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC_DIR = "Assets/Data/Config/Level/Block1"
OUT_DIR = "Assets/Data/Config/Level/Arena"
PREVIEW = os.path.join(REPO, "ClaudeNotes", "cvgp", "ref", "arena_preview.png")
CELL = 16
COLS = 65
ROWS = 12
START = [48, 176]

# (ô đích (cột, hàng), section nguồn, ô nguồn (cột, hàng), ký tự va chạm đích)
PATCHES = [
    ((26, 9), "EntranceHall", (44, 7), "#"),
    ((26, 10), "EntranceHall", (44, 7), "#"),
    ((30, 11), "EntranceHall", (30, 10), "."),
    ((31, 11), "EntranceHall", (31, 10), "."),
    ((34, 11), "CryptB", (0, 10), "~"),
    ((35, 11), "CryptB", (1, 10), "~"),
]
COLORS = {"#": (255, 60, 60), "S": (255, 230, 0), "=": (255, 140, 0), "~": (0, 170, 255), "P": (150, 0, 255),
          "D": (255, 0, 255), "X": (0, 255, 80), "!": (255, 255, 255)}


def rp(rel):
    return os.path.join(REPO, rel.replace("/", os.sep))


def read_csv(rel):
    with open(rp(rel), encoding="utf-8") as f:
        return [[int(v) for v in line.strip().split(",")] for line in f if line.strip()]


def write_if_changed(rel, text):
    path = rp(rel)
    old = None
    if os.path.exists(path):
        with open(path, "rb") as f:
            old = f.read()
    data = text.encode("utf-8")
    if old == data:
        return "same"
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)
    return "written"


def main():
    check = "--check" in sys.argv
    tiles_doc = json.load(open(rp(SRC_DIR + "/Level1Tiles.json"), encoding="utf-8"))
    map_doc = json.load(open(rp(SRC_DIR + "/Level1Map.json"), encoding="utf-8"))
    tsec = {s["id"]: s for s in tiles_doc["sections"]}
    msec = {s["id"]: s for s in map_doc["sections"]}
    grids = {k: read_csv(tsec[k]["csv"]) for k in ("EntranceHall", "CryptB")}

    eh = msec["EntranceHall"]
    tiles = [row[:COLS * 2] for row in grids["EntranceHall"]]
    coll = [list(row[:COLS]) for row in eh["collision"]]
    assert len(tiles) == ROWS * 2 and all(len(r) == COLS * 2 for r in tiles)

    for (dc, dr), src, (sc, sr), ch in PATCHES:
        g = grids[src]
        for y in range(2):
            for x in range(2):
                tiles[dr * 2 + y][dc * 2 + x] = g[sr * 2 + y][sc * 2 + x]
        want_src = msec[src]["collision"][sr][sc]
        # ô nguồn phải cùng loại (chép đúng nghĩa), trừ hố: nền EH (.) đặt vào chỗ sàn
        if want_src != ch:
            sys.exit("PATCH %s<-%s%s: ky tu nguon '%s' khac '%s'" % ((dc, dr), src, (sc, sr), want_src, ch))
        coll[dr][dc] = ch

    stairs = []
    for s in eh["stairs"]:
        k_last = s["steps"] - 1
        dx = 8 if s["dir"] == "UpRight" else -8
        xs = [s["foot"][0] + dx * k for k in range(s["steps"])]
        if max(xs) + 8 <= COLS * CELL and min(xs) >= 0:
            t = dict(s)
            t["id"] = s["id"].replace("EntranceHall", "Arena")
            stairs.append(t)
            # mọi bậc phải nằm trong ô Stair (không bị patch đè)
            for k in range(s["steps"]):
                cx, cy = (s["foot"][0] + dx * k) // CELL, (s["foot"][1] - 8 * k) // CELL
                if coll[cy][cx] not in "S=":
                    sys.exit("bac %s/%d nam o o '%s'" % (t["id"], k, coll[cy][cx]))
            assert [s["foot"][0] + dx * k_last, s["foot"][1] - 8 * k_last] == s["top"]
    rows_str = ["".join(r) for r in coll]

    section_t = {
        "id": "Arena", "inventoryId": "-", "stage": 0, "sheetRect": [0, 0, 0, 0],
        "width": COLS * CELL, "height": ROWS * CELL, "cols8": COLS * 2, "rows8": ROWS * 2,
        "uniqueTiles": len({v for r in tiles for v in r}), "csv": OUT_DIR + "/Arena_tiles.csv",
    }
    tdoc = {
        "note": "CVGP 07 - sinh boi Tools/cvgp/build_arena.py (ghep o Map 1); KHONG sua tay",
        "tileSize": tiles_doc["tileSize"], "columns": tiles_doc["columns"], "count": tiles_doc["count"],
        "png": tiles_doc["png"], "levelRowToNesY": tiles_doc["levelRowToNesY"], "sections": [section_t],
    }
    section_m = {
        "id": "Arena", "stage": 0, "width": COLS * CELL, "height": ROWS * CELL, "cols": COLS, "rows": ROWS,
        "cameraMinX": 0, "cameraMaxX": COLS * CELL - 256, "bossLockCx": -1,
        "hasSimonStart": True, "simonStart": START,
        "simonStartSrc": "quy uoc: Arena (CVGP 07) - san trong x 0-415, mat san y 176",
        "collision": rows_str, "foreground": [], "stairs": stairs, "connections": [],
    }
    mdoc = {
        "note": "CVGP 07 - sinh boi Tools/cvgp/build_arena.py (EntranceHall cot 0-64 + tuong/ho/nuoc chep o Map 1); KHONG sua tay",
        "cellSize": CELL, "levelRowToNesY": map_doc["levelRowToNesY"], "types": map_doc["types"],
        "order": ["Arena"], "sections": [section_m],
    }
    csv_text = "\n".join(",".join(str(v) for v in r) for r in tiles) + "\n"
    outs = [
        (OUT_DIR + "/Arena_tiles.csv", csv_text),
        (OUT_DIR + "/ArenaTiles.json", json.dumps(tdoc, indent=1, ensure_ascii=True) + "\n"),
        (OUT_DIR + "/ArenaMap.json", json.dumps(mdoc, indent=1, ensure_ascii=True) + "\n"),
    ]
    for r in rows_str:
        print("   " + r)
    print("stairs:", [(s["id"], s["dir"], s["steps"], s["foot"], s["top"]) for s in stairs])
    if check:
        for rel, text in outs:
            ok = os.path.exists(rp(rel)) and open(rp(rel), "rb").read() == text.encode("utf-8")
            print(rel, "OK" if ok else "KHAC")
        return
    for rel, text in outs:
        print(rel, write_if_changed(rel, text))
    render(tiles_doc, tiles, rows_str, stairs)


def render(tiles_doc, tiles, rows_str, stairs):
    ts, cols_t = tiles_doc["tileSize"], tiles_doc["columns"]
    sheet = Image.open(rp(tiles_doc["png"])).convert("RGBA")
    w, h = COLS * CELL, ROWS * CELL
    img = Image.new("RGBA", (w, h), (0, 0, 0, 255))
    for r, row in enumerate(tiles):
        for c, v in enumerate(row):
            sx, sy = (v % cols_t) * ts, (v // cols_t) * ts
            img.paste(sheet.crop((sx, sy, sx + ts, sy + ts)), (c * ts, r * ts))
    img = img.resize((w * 2, h * 2), Image.NEAREST)
    over = Image.new("RGBA", img.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(over)
    for r, row in enumerate(rows_str):
        for c, ch in enumerate(row):
            if ch in COLORS:
                col = COLORS[ch]
                d.rectangle([c * 32, r * 32, c * 32 + 31, r * 32 + 31], fill=col + (70,), outline=col + (255,))
    for s in stairs:
        dx = 8 if s["dir"] == "UpRight" else -8
        for k in range(s["steps"]):
            x, y = s["foot"][0] + dx * k, s["foot"][1] - 8 * k
            d.rectangle([x * 2, y * 2, x * 2 + 15, y * 2 + 3], outline=(255, 255, 255, 255))
    d.rectangle([START[0] * 2 - 2, START[1] * 2 - 2, START[0] * 2 + 2, START[1] * 2 + 2], fill=(0, 255, 0, 255))
    out = Image.alpha_composite(img, over).convert("RGB")
    # chia 2 dải 520 px cho dễ xem
    half = out.width // 2
    sheet2 = Image.new("RGB", (half, out.height * 2 + 8), (40, 40, 40))
    sheet2.paste(out.crop((0, 0, half, out.height)), (0, 0))
    sheet2.paste(out.crop((half, 0, out.width, out.height)), (0, out.height + 8))
    os.makedirs(os.path.dirname(PREVIEW), exist_ok=True)
    sheet2.save(PREVIEW, optimize=True)
    print("preview", os.path.relpath(PREVIEW, REPO))


if __name__ == "__main__":
    main()
