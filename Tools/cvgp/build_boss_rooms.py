# -*- coding: utf-8 -*-
"""
build_boss_rooms.py - CVGP 19: 3 phong boss thu (Arena) cho Queen Medusa / Mummy Men / Frankenstein & Igor.
KHONG co sheet ban do Block 2-4 => GHEP o Map 1 (H2 - khong ve moi): chep o tile 8x8 + ky tu va cham + cau thang cua Level1Map.json.
Moi phong 1 man (16 cot x 12 hang = 256 x 192 px), camera dung yen + bossLockCx 0 (khoa ngay => BossArena bat tuong mep man).

Chay tu goc repo (SAU build_tileset.py + build_collision.py):  python -P -E Tools/cvgp/build_boss_rooms.py [--check]
  roi menu Unity CVGP/Build Arena Boss Rooms (CvgpLevelBuilder) -> Assets/Data/Config/Level/Arena/Boss*.asset
  + Assets/Data/Prefabs/Level/Test/Boss*.prefab
Ra:  Assets/Data/Config/Level/Arena/{BossRoomsTiles.json, BossRoomsMap.json, Boss<Phong>_tiles.csv}
     ClaudeNotes/cvgp/ref/boss_rooms_preview.png (2x, overlay va cham + bac thang)

Bo cuc (theo mo ta wiki - CV_WIKI_DATA S4.3; GAN DUNG, khong phai phong goc):
  BossMedusa : EntranceHall cot 0-15 nguyen trang - san phang y 176 ("vo ra tu buc tuong giua phong" - tuong la sprite, khong phai o).
  BossMummy  : CryptA cot 0-15 - cau thang TRAI (UpLeft 8 bac, chan (232,168) -> dinh (176,112), go y 112 cot 8-10) + go cao y 80
               cot 0-7 ("cua phong"); o cau thang xuong CB (3,11) lap bang nen CryptA (2,11), lan can (2-3,10) <- tuong (5,10); KHOI PHA (6,10) <- khoi PB (43,9)
               = "khoi thap nhat cau thang trai chua Pot Roast" (dung len duoc, Mummy di tren san vuong khoi).
  BossFrank  : EntranceHall cot 0-15 + 2 BUC GOC thap 32 px (cot 0-1 va 14-15, hang 9-10, mat y 144) <- gach buc EH (44,7)
               (nhu tuong thap Arena 07) = "cac buc o goc" - Igor nhay cao len buc, Simon nhay len mep buc phai.
  CVGP 20 (Block 5-6, theo wiki - GAN DUNG, khong co ban do/video):
  BossDeath  : EntranceHall cot 0-15 + BUC GIUA cao 32 px (cot 6-9, hang 9-10, mat y 144) + 2 GO TREN noi (cot 0-4 / 11-15, hang 7,
               mat y 112, day 128 - di duoi duoc) <- gach buc EH (44,7) = "buc tren-phai", "buc giua", "nhay giua cac go" (wiki Stage 15).
  BossDracula: EntranceHall cot 0-15 + 2 BAC thap 16 px o 2 mep (cot 0-2 / 13-15, hang 10, mat y 160) <- gach EH (44,7) = "phong Dracula
               co buc" (de bai routine 20; phong goc khong co anh/video).
"""
import json
import os
import sys

from PIL import Image, ImageDraw

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SRC_DIR = "Assets/Data/Config/Level/Block1"
OUT_DIR = "Assets/Data/Config/Level/Arena"
PREVIEW = os.path.join(REPO, "ClaudeNotes", "cvgp", "ref", "boss_rooms_preview.png")
CELL = 16
COLS = 16
ROWS = 12

# id, section nguon, cot dau, diem bat dau Simon, [(o dich, section nguon, o nguon, ky tu dich)], cau thang giu (id nguon), ghi chu
ROOMS = [
    ("BossMedusa", "EntranceHall", 0, [32, 176], [], [],
     "quy uoc: phong Medusa thu (CVGP 19) - Simon vao tu mep trai"),
    ("BossMummy", "CryptA", 0, [144, 112],
     [((3, 11), "CryptA", (2, 11), "#"),
      ((2, 10), "CryptA", (5, 10), "."), ((3, 10), "CryptA", (5, 10), "."),
      ((6, 10), "PhantomBat", (43, 9), "X")],
     ["CryptA_stair1"],
     "quy uoc: phong Mummy thu (CVGP 19) - Simon vao o go trai y 112 (dinh cau thang trai)"),
    ("BossFrank", "EntranceHall", 0, [16, 144],
     [((0, 9), "EntranceHall", (44, 7), "#"), ((1, 9), "EntranceHall", (44, 7), "#"),
      ((0, 10), "EntranceHall", (44, 7), "#"), ((1, 10), "EntranceHall", (44, 7), "#"),
      ((14, 9), "EntranceHall", (44, 7), "#"), ((15, 9), "EntranceHall", (44, 7), "#"),
      ((14, 10), "EntranceHall", (44, 7), "#"), ((15, 10), "EntranceHall", (44, 7), "#")],
     [],
     "quy uoc: phong Frankenstein thu (CVGP 19) - Simon vao tren buc goc trai y 144"),
    ("BossDeath", "EntranceHall", 0, [24, 176],
     [((6, 9), "EntranceHall", (44, 7), "#"), ((7, 9), "EntranceHall", (44, 7), "#"), ((8, 9), "EntranceHall", (44, 7), "#"), ((9, 9), "EntranceHall", (44, 7), "#"), ((6, 10), "EntranceHall", (44, 7), "#"), ((7, 10), "EntranceHall", (44, 7), "#"), ((8, 10), "EntranceHall", (44, 7), "#"), ((9, 10), "EntranceHall", (44, 7), "#"), ((0, 7), "EntranceHall", (44, 7), "#"), ((1, 7), "EntranceHall", (44, 7), "#"), ((2, 7), "EntranceHall", (44, 7), "#"), ((3, 7), "EntranceHall", (44, 7), "#"), ((4, 7), "EntranceHall", (44, 7), "#"), ((11, 7), "EntranceHall", (44, 7), "#"), ((12, 7), "EntranceHall", (44, 7), "#"), ((13, 7), "EntranceHall", (44, 7), "#"), ((14, 7), "EntranceHall", (44, 7), "#"), ((15, 7), "EntranceHall", (44, 7), "#")],
     [],
     "quy uoc: phong Death thu (CVGP 20) - Simon vao mep trai san y 176"),
    ("BossDracula", "EntranceHall", 0, [24, 160],
     [((0, 10), "EntranceHall", (44, 7), "#"), ((1, 10), "EntranceHall", (44, 7), "#"), ((2, 10), "EntranceHall", (44, 7), "#"), ((13, 10), "EntranceHall", (44, 7), "#"), ((14, 10), "EntranceHall", (44, 7), "#"), ((15, 10), "EntranceHall", (44, 7), "#")],
     [],
     "quy uoc: phong Dracula thu (CVGP 20) - Simon vao tren bac trai y 160"),
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


def build_room(room, tsec, msec, grids):
    rid, src, c0, start, patches, keep_stairs, start_src = room
    tiles = [row[c0 * 2:(c0 + COLS) * 2] for row in grids[src]]
    coll = [list(row[c0:c0 + COLS]) for row in msec[src]["collision"]]
    assert len(tiles) == ROWS * 2 and all(len(r) == COLS * 2 for r in tiles)
    for (dc, dr), psrc, (sc, sr), ch in patches:
        g = grids[psrc]
        for y in range(2):
            for x in range(2):
                tiles[dr * 2 + y][dc * 2 + x] = g[sr * 2 + y][sc * 2 + x]
        want_src = msec[psrc]["collision"][sr][sc]
        # o nguon phai cung loai (chep dung nghia), tru o cau thang xuong (=) lap thanh nen dac (#)
        if want_src != ch:
            sys.exit("PATCH %s %s<-%s%s: ky tu nguon '%s' khac '%s'" % (rid, (dc, dr), psrc, (sc, sr), want_src, ch))
        coll[dr][dc] = ch
    stairs = []
    for s in msec[src]["stairs"]:
        if s["id"] not in keep_stairs:
            continue
        dx = 8 if s["dir"] == "UpRight" else -8
        t = dict(s)
        t["id"] = rid + "_" + s["id"].split("_")[-1]
        t["foot"] = [s["foot"][0] - c0 * CELL, s["foot"][1]]
        t["top"] = [s["top"][0] - c0 * CELL, s["top"][1]]
        for k in range(s["steps"]):
            cx, cy = (t["foot"][0] + dx * k) // CELL, (t["foot"][1] - 8 * k) // CELL
            if not (0 <= cx < COLS) or coll[cy][cx] not in "S=":
                sys.exit("bac %s/%d nam o o ngoai/khong phai thang" % (t["id"], k))
        stairs.append(t)
    rows_str = ["".join(r) for r in coll]
    # khong con ky tu cau thang mo coi (S khong thuoc bac nao) - kiem chong chep nham
    stair_cells = set()
    for t in stairs:
        dx = 8 if t["dir"] == "UpRight" else -8
        for k in range(t["steps"]):
            stair_cells.add(((t["foot"][0] + dx * k) // CELL, (t["foot"][1] - 8 * k) // CELL))
    for r, row in enumerate(rows_str):
        for c, ch in enumerate(row):
            if ch in "S=" and (c, r) not in stair_cells:
                sys.exit("%s: o thang mo coi (%d,%d)" % (rid, c, r))
    section_t = {
        "id": rid, "inventoryId": "-", "stage": 0, "sheetRect": [0, 0, 0, 0],
        "width": COLS * CELL, "height": ROWS * CELL, "cols8": COLS * 2, "rows8": ROWS * 2,
        "uniqueTiles": len({v for r in tiles for v in r}), "csv": OUT_DIR + "/" + rid + "_tiles.csv",
    }
    section_m = {
        "id": rid, "stage": 0, "width": COLS * CELL, "height": ROWS * CELL, "cols": COLS, "rows": ROWS,
        "cameraMinX": 0, "cameraMaxX": 0, "bossLockCx": 0,
        "hasSimonStart": True, "simonStart": start, "simonStartSrc": start_src,
        "collision": rows_str, "foreground": [], "stairs": stairs, "connections": [],
    }
    return tiles, rows_str, stairs, section_t, section_m


def main():
    check = "--check" in sys.argv
    tiles_doc = json.load(open(rp(SRC_DIR + "/Level1Tiles.json"), encoding="utf-8"))
    map_doc = json.load(open(rp(SRC_DIR + "/Level1Map.json"), encoding="utf-8"))
    tsec = {s["id"]: s for s in tiles_doc["sections"]}
    msec = {s["id"]: s for s in map_doc["sections"]}
    grids = {k: read_csv(tsec[k]["csv"]) for k in tsec}

    built = [build_room(r, tsec, msec, grids) for r in ROOMS]
    tdoc = {
        "note": "CVGP 19 - sinh boi Tools/cvgp/build_boss_rooms.py (ghep o Map 1); KHONG sua tay",
        "tileSize": tiles_doc["tileSize"], "columns": tiles_doc["columns"], "count": tiles_doc["count"],
        "png": tiles_doc["png"], "levelRowToNesY": tiles_doc["levelRowToNesY"], "sections": [b[3] for b in built],
    }
    mdoc = {
        "note": "CVGP 19/20 - sinh boi Tools/cvgp/build_boss_rooms.py (5 phong boss thu ghep o Map 1, theo mo ta wiki); KHONG sua tay",
        "cellSize": CELL, "levelRowToNesY": map_doc["levelRowToNesY"], "types": map_doc["types"],
        "order": [r[0] for r in ROOMS], "sections": [b[4] for b in built],
    }
    outs = []
    for (tiles, rows_str, stairs, st, sm) in built:
        outs.append((st["csv"], "\n".join(",".join(str(v) for v in r) for r in tiles) + "\n"))
        print(st["id"])
        for r in rows_str:
            print("   " + r)
        print("   stairs:", [(s["id"], s["dir"], s["steps"], s["foot"], s["top"]) for s in stairs])
    outs.append((OUT_DIR + "/BossRoomsTiles.json", json.dumps(tdoc, indent=1, ensure_ascii=True) + "\n"))
    outs.append((OUT_DIR + "/BossRoomsMap.json", json.dumps(mdoc, indent=1, ensure_ascii=True) + "\n"))
    if check:
        for rel, text in outs:
            ok = os.path.exists(rp(rel)) and open(rp(rel), "rb").read() == text.encode("utf-8")
            print(rel, "OK" if ok else "KHAC")
        return
    for rel, text in outs:
        print(rel, write_if_changed(rel, text))
    render(tiles_doc, built)


def render(tiles_doc, built):
    ts, cols_t = tiles_doc["tileSize"], tiles_doc["columns"]
    sheet = Image.open(rp(tiles_doc["png"])).convert("RGBA")
    w, h = COLS * CELL, ROWS * CELL
    out = Image.new("RGB", (w * 2 * len(built) + 8 * (len(built) - 1), h * 2), (40, 40, 40))
    for i, (tiles, rows_str, stairs, st, sm) in enumerate(built):
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
                    d.rectangle([c * 32, r * 32, c * 32 + 31, r * 32 + 31], fill=col + (60,), outline=col + (255,))
        for s in stairs:
            dx = 8 if s["dir"] == "UpRight" else -8
            for k in range(s["steps"]):
                x, y = s["foot"][0] + dx * k, s["foot"][1] - 8 * k
                d.rectangle([x * 2, y * 2, x * 2 + 15, y * 2 + 3], outline=(255, 255, 255, 255))
        sx, sy = sm["simonStart"]
        d.rectangle([sx * 2 - 2, sy * 2 - 2, sx * 2 + 2, sy * 2 + 2], fill=(0, 255, 0, 255))
        out.paste(Image.alpha_composite(img, over).convert("RGB"), (i * (w * 2 + 8), 0))
    os.makedirs(os.path.dirname(PREVIEW), exist_ok=True)
    out.save(PREVIEW, optimize=True)
    print("preview", os.path.relpath(PREVIEW, REPO))


if __name__ == "__main__":
    main()
