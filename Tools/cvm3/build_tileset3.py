# -*- coding: utf-8 -*-
"""
build_tileset3.py - CVM3 05: cắt địa hình Level 3 (Map 3 = Block 3, Stage 7-9) thành tileset 8x8 dedupe + CSV chỉ số tile từng section.
Dùng lại phần chung Tools/cvgp/build_tileset.py (dedupe_regions, tileset_png, rebuild_check, write_if_changed) — Level 1/2 không đổi.

Chạy từ gốc repo:  python -P -E Tools/cvm3/build_tileset3.py [--check]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

Đầu vào (chỉ đọc): Art_Source/spriters-resource/level3_walkway.png (sha256 CVM3_MEMORY §3b) + rect 5 vùng =
                    ClaudeNotes/cvm3/inventory/level3_walkway.json (kiểm khớp bảng SECTIONS dưới đây).
NỚI WalkwayB sang TRÁI 112 px (H2 - không vẽ mới, chỉ CHÉP ô 8x8 CÓ SẴN trong sheet): video Map_3 camera WalkwayB tới cx −112
  (CVM3 02, f3483 1:56.10) ⇒ bội 16 nhỏ nhất ≥ 112 = 112 (7 metatile). Nội dung cột trái đo bằng nền trung vị video (424 frame cx < 128,
  đổi màu video→sheet LUT CVM3 02) so với MỌI tile 8x8 của 5 vùng: x −112..−33 trời thuần; x −32..−1 hàng 136..183 = 17 ô đá rêu mép cầu
  (tile CHÍNH WalkwayB có sẵn — gạch rêu cột 6 hàng 0 + mép đá dưới tấm xanh (64..80, 64..72); rms 8–31 so ô nhì ≥ 86); kiểm chứng cùng phép đo ở x 0..63: 176/176 ô đúng tile sheet (đo: Tools/cvm3/wb_pad3.py). Hàng 0..15
  (sau HUD) không thấy ở video ⇒ trời (liền với cột 0 sheet). Gốc section dời +112: x_section = x_sheet + 112 (cx video −112 ⇔ cx 0).
Đầu ra:
  Assets/Data/Art/Level/Block3/level3_tiles.png        tileset 16 tile/hàng (128 px), ô thừa trong suốt
  Assets/Data/Config/Level/Block3/<Section>_tiles.csv  ma trận chỉ số tile (hàng = 8 px từ TRÊN xuống)
  Assets/Data/Config/Level/Block3/Level3Tiles.json     mô tả tileset + section (CvgpLevelBuilder đọc, menu CVM3/Build Map3 Tilemaps)
Tự kiểm: dựng lại từ tileset + CSV ⇒ 0 pixel lệch so vùng (đã nới); phần sheet của mỗi vùng 0 lệch so sheet gốc; số tile = kiểm kê 125
(phần nới chỉ dùng tile đã có). Chạy 2 lần ⇒ "same".
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

SHEET = os.path.join(REPO, "Art_Source", "spriters-resource", "level3_walkway.png")
SHEET_SHA256 = "c8bfc7ef3da18110c4f226276e7ae82e6611d2c4f3246517f5578bd476c44eee"
INVENTORY = os.path.join(REPO, "ClaudeNotes", "cvm3", "inventory", "level3_walkway.json")
ART_DIR = "Assets/Data/Art/Level/Block3"
CFG_DIR = "Assets/Data/Config/Level/Block3"
PNG_REL = ART_DIR + "/level3_tiles.png"
JSON_REL = CFG_DIR + "/Level3Tiles.json"
INV_UNIQUE_TOTAL = 125   # kiểm kê planner / CVM3 01
EXPECTED_UNIQUE = 125    # phần nới WalkwayB chỉ chép tile có sẵn ⇒ không thêm tile

# id Unity - rect sheet [x, y, w, h] - stage (thứ tự quét = thứ tự đi trong video Map_3)
SECTIONS = [
    ("WalkwayEntranceA", [1, 425, 768, 192], 7),
    ("WalkwayEntranceB", [1, 218, 768, 192], 7),
    ("WalkwayA", [774, 218, 768, 192], 8),
    ("WalkwayB", [1286, 11, 768, 192], 8),
    ("MummyMan", [2059, 11, 1536, 192], 9),
]
SKY = (36, 24, 140)
# Nới trái: section -> (bề rộng px, ô trời nguồn (section, x, y), [(x video, y, section nguồn, x nguồn, y nguồn) — ô 8x8 KHÁC trời])
PAD_LEFT = {
    "WalkwayB": (112, ("WalkwayB", 0, 0), [
        (-32, 136, "WalkwayB", 448, 72), (-8, 136, "WalkwayB", 472, 72),
        (-32, 144, "WalkwayB", 96, 0), (-24, 144, "WalkwayB", 448, 72),
        (-16, 144, "WalkwayB", 472, 72), (-8, 144, "WalkwayB", 104, 0),
        (-32, 152, "WalkwayB", 96, 8), (-24, 152, "WalkwayB", 104, 8),
        (-16, 152, "WalkwayB", 96, 8), (-8, 152, "WalkwayB", 104, 0),
        (-32, 160, "WalkwayB", 64, 64), (-24, 160, "WalkwayB", 72, 64),
        (-16, 160, "WalkwayB", 80, 64), (-8, 160, "WalkwayB", 128, 0),
        (-16, 168, "WalkwayB", 80, 72), (-8, 168, "WalkwayB", 80, 64),
        (-8, 176, "WalkwayB", 80, 72),
    ]),
}
PAD_SRC = ("CVM3 05: camera WalkwayB toi cx -112 (video Map_3 f3483 1:56.10, CVM3 02 MAP3_TIMELINE 2a) => noi trai 112 px = bo 16 nho nhat >= 112. "
           "Noi dung do bang nen trung vi video (424 frame cx < 128, LUT video->sheet CVM3 02) so moi tile 8x8 cua sheet: x -112..-33 troi thuan, "
           "x -32..-1 hang 136-183 = 17 o da reu mep cau chep tu o co san CUA CHINH WalkwayB (gach reu x 96-111 y 0-15, mep da x 64-87 y 64-79, x 448/472 y 72; rms 8-31, o nhi >= 86); kiem chung x 0..63: 176/176 o dung. "
           "Hang 0-15 sau HUD khong thay => troi. Script do: Tools/cvm3/wb_pad3.py (anh _cache/05/work/wb_pad_compare.png; ban luu ref/nes/cvm3_05_wb_pad_compare.png)")


def load_regions():
    if sha256(SHEET) != SHEET_SHA256:
        sys.exit("sheet level3_walkway.png sai sha256")
    inv = json.load(open(INVENTORY, encoding="utf-8"))
    inv_rects = {r["name"]: r["rect"] for r in inv["regions"]}
    inv_unique = {r["name"]: r["unique8"] for r in inv["regions"]}
    if inv["total_unique8"] != INV_UNIQUE_TOTAL:
        sys.exit("kiem ke doi: total_unique8 %d" % inv["total_unique8"])
    a = np.asarray(Image.open(SHEET).convert("RGBA"))
    rgb = a[..., :3]
    raw = {}
    for sid, (x, y, w, h), _stage in SECTIONS:
        if inv_rects.get(sid) != [x, y, w, h]:
            sys.exit("rect %s lech kiem ke: %s" % (sid, inv_rects.get(sid)))
        if (a[y:y + h, x:x + w, 3] != 255).any():
            sys.exit("%s co pixel khong duc" % sid)
        reg = rgb[y:y + h, x:x + w]
        if ((reg == (0, 128, 0)).all(axis=2)).any():
            sys.exit("%s co pixel nen xanh ben trong" % sid)
        raw[sid] = reg
    regions = []
    for sid, (x, y, w, h), _stage in SECTIONS:
        reg = raw[sid]
        if sid in PAD_LEFT:
            pw, (ss, sx, sy), cells = PAD_LEFT[sid]
            if pw % 16 or pw <= 0:
                sys.exit("%s: be rong noi phai boi 16" % sid)
            sky = raw[ss][sy:sy + 8, sx:sx + 8]
            if not (sky == SKY).all():
                sys.exit("%s: o troi nguon %s khong thuan troi" % (sid, (ss, sx, sy)))
            pad = np.zeros((h, pw, 3), np.uint8)
            pad[:] = SKY
            seen = set()
            for (vx, vy, s2, tx, ty) in cells:
                if not (-pw <= vx < 0 and vx % 8 == 0 and 0 <= vy < h and vy % 8 == 0) or (vx, vy) in seen:
                    sys.exit("%s: o noi sai %s" % (sid, (vx, vy)))
                seen.add((vx, vy))
                src = raw[s2][ty:ty + 8, tx:tx + 8]
                if src.shape != (8, 8, 3) or tx % 8 or ty % 8:
                    sys.exit("%s: o nguon sai %s" % (sid, (s2, tx, ty)))
                pad[vy:vy + 8, pw + vx:pw + vx + 8] = src
            reg = np.concatenate([pad, reg], axis=1)
        regions.append((sid, reg))
    return raw, regions, inv_unique


def pad_of(sid):
    return PAD_LEFT[sid][0] if sid in PAD_LEFT else 0


def padded_regions():
    """{id: vùng RGB (WalkwayB đã nới trái)} — build_collision3.py / accept3.py dùng (cùng ảnh mà tileset dựng lại 0 lệch)."""
    _raw, regions, _u = load_regions()
    return dict(regions)


def build():
    raw, regions, inv_unique = load_regions()
    tiles, maps = dedupe_regions(regions)
    png = tileset_png(tiles)
    per, diff_total = rebuild_check(png, regions, maps)
    # phần sheet của mỗi vùng (bỏ cột nới) so sheet GỐC
    sheet_part = [(sid, reg[:, pad_of(sid):]) for sid, reg in regions]
    maps_part = {sid: maps[sid][:, pad_of(sid) // TILE:] for sid, _r in regions}
    per_raw, diff_raw = rebuild_check(png, [(sid, raw[sid]) for sid, _r in sheet_part], maps_part)
    for sid, _rect, _st in SECTIONS:
        if per_raw[sid][1] != inv_unique[sid]:
            sys.exit("%s: so tile duy nhat (phan sheet) %d khac kiem ke (%d)" % (sid, per_raw[sid][1], inv_unique[sid]))
    return tiles, maps, png, per, diff_total, diff_raw


def main():
    check = "--check" in sys.argv
    tiles, maps, png, per, diff_total, diff_raw = build()
    n = len(tiles)
    print("tiles 8x8 duy nhat:", n, "(ky vong %d = kiem ke; phan noi WalkwayB chi chep tile co san)" % EXPECTED_UNIQUE)
    for sid, (d, u) in per.items():
        print("  %-17s unique %3d  pixel lech khi dung lai: %d%s" % (sid, u, d, ("  (noi trai %d px)" % pad_of(sid)) if pad_of(sid) else ""))
    print("TONG pixel lech (so vung da noi):", diff_total, "| phan sheet so sheet goc:", diff_raw)
    if n != EXPECTED_UNIQUE or diff_total != 0 or diff_raw != 0:
        sys.exit("LOI: so tile / pixel lech khong dat - khong ghi file")
    report = []
    write_if_changed(PNG_REL, png, check, report)
    meta = {
        "note": "CVM3 05 - sinh boi Tools/cvm3/build_tileset3.py; KHONG sua tay",
        "sheet": "Art_Source/spriters-resource/level3_walkway.png",
        "sheetSha256": SHEET_SHA256,
        "tileSize": TILE,
        "columns": COLUMNS,
        "count": n,
        "png": PNG_REL,
        "levelRowToNesY": 32,
        "pads": [{"section": sid, "left": pw, "sky": list(sky), "cells": [list(c) for c in cells], "src": PAD_SRC}
                 for sid, (pw, sky, cells) in PAD_LEFT.items()],
        "sections": [],
    }
    for sid, rect, stage in SECTIONS:
        m = maps[sid]
        csv_rel = CFG_DIR + "/" + sid + "_tiles.csv"
        txt = "\n".join(",".join(str(int(v)) for v in row) for row in m) + "\n"
        write_if_changed(csv_rel, txt.encode("ascii"), check, report)
        meta["sections"].append({"id": sid, "inventoryId": sid, "stage": stage, "sheetRect": rect, "padLeft": pad_of(sid),
                                 "width": rect[2] + pad_of(sid), "height": rect[3], "cols8": int(m.shape[1]), "rows8": int(m.shape[0]),
                                 "uniqueTiles": per[sid][1], "csv": csv_rel})
    js = json.dumps(meta, ensure_ascii=True, indent=1) + "\n"
    write_if_changed(JSON_REL, js.encode("ascii"), check, report)
    for line in report:
        print(line)
    print("CHECK (khong ghi)" if check else "OK")


if __name__ == "__main__":
    main()
