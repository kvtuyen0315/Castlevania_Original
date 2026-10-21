# -*- coding: utf-8 -*-
"""
build_collision3.py - CVM3 05: lưới va chạm (ô 16x16) + cầu thang + đồ thị section của Map 3 (Level 3 = Block 3, Stage 7-9).
Cùng khuôn Tools/cvm2/build_collision2.py (CVM2 05) — dùng lại check_graph / section_json / render (Tools/cvgp/build_collision.py) và
find_pits (Tools/cvm2/build_collision2.py); Level 1/2 không đổi.

Chạy từ gốc repo (SAU build_tileset3.py):  python -P -E Tools/cvm3/build_collision3.py [--check] [--no-images] [--no-video] [--dump]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

1. Phân loại TỰ ĐỘNG theo metatile id (khối 16x16 duy nhất của vùng — WalkwayB đã nới trái 112 px, Tools/cvm3/build_tileset3.py):
     L1 có màu cột cửa đỏ (216,40,0)                                  ⇒ Door   (cột cửa đỏ 8x48 x 752-759 — 3 cửa mép PHẢI)
     L2 có ô 8x8 bậc thang (≥ 25 px cam (252,152,56) hàng 0-4)          ⇒ Stair
     L3 hồng-cam (252,116,96) ≥ 40 %                                   ⇒ Solid  (khối gạch hồng-cam 16x16 viền rêu)
     còn lại                                                          ⇒ Empty  (gạch xanh nền, rêu, vòm, trời, tượng, lan can, cột phòng Mummy…)
2. Áp Tools/cvm3/level3_collision_overrides.json (overrides có expectAuto + reason + bằng chứng video; checks = chỗ đã xem video).
3. Cầu thang = TỰ DÒ từ sheet (bậc 8x8 nối chéo ↗ (x+8, y−8) / ↖ (x−8, y−8)); thang zig-zag (WEB, WalkwayA) = 2 thang riêng.
4. Đồ thị Tools/cvm3/level3_sections.json — check_graph Level 1 (biên camera, simonStart, cửa trùng ô Door, thang nối liền qua
   targetOrigin) + kiểm Level 3: cửa PHẢI (dir +1: trigger cột cuối, đích ghép LIỀN PHẢI targetOrigin = (rộng nguồn, 0), entry đứng trên ô
   đặc, triggerSimonX = trigger.x − 12 như Map 1), simonStartDir, markers, khoá boss MummyMan = cameraMaxX.
5. Hố (pit) TỰ TÍNH (find_pits CVM2): khe trên hàng sàn mà cột dưới trống tới đáy ⇒ marker "pit" (rơi ra đáy = chết Pit).
6. --video (mặc định bật): mọi frame Simon ĐỨNG/ĐI trên đất ở video Map_3 (ClaudeNotes/cvm3/ref/data/map3_simon.csv: idle/walk, err < 0.1,
   Y giữ ±2 frame) ⇒ ô dưới chân (X±6 = đầu dò sàn Simon CVM3 04) đặc/thang. WalkwayB: x_section = world_x (CSV, gốc sheet) + 112.
Đầu ra:  Assets/Data/Config/Level/Block3/Level3Map.json (CvgpLevelBuilder đọc — menu CVM3/Build Map3 Tilemaps)
         ClaudeNotes/cvm3/ref/level3/collision_<Section>.png (ảnh overlay 2x; --no-images để bỏ)
"""
import csv
import hashlib
import json
import os
import sys
from collections import Counter, OrderedDict

import numpy as np

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(TOOLS_DIR))
sys.path.insert(0, os.path.join(REPO, "Tools", "cvgp"))
sys.path.insert(0, os.path.join(REPO, "Tools", "cvm2"))
sys.path.insert(0, TOOLS_DIR)
import sheetlib as S  # noqa: E402
from build_collision import TYPES, CELL, check_graph, section_json, render  # noqa: E402
from build_collision2 import find_pits, solid_like  # noqa: E402
import build_tileset3 as T3  # noqa: E402

OVERRIDES = os.path.join(REPO, "Tools", "cvm3", "level3_collision_overrides.json")
GRAPH = os.path.join(REPO, "Tools", "cvm3", "level3_sections.json")
OUT_REL = T3.CFG_DIR + "/Level3Map.json"
IMG_DIR = os.path.join(REPO, "ClaudeNotes", "cvm3", "ref", "level3")
SIMON_CSV = os.path.join(REPO, "ClaudeNotes", "cvm3", "ref", "data", "map3_simon.csv")

SALMON = (252, 116, 96)
TREAD = (252, 152, 56)
DOORCOL = (216, 40, 0)
EXPECTED_META = 83        # kiểm kê 83 metatile (phần nới WalkwayB chỉ tạo khối 16x16 đã có trong WalkwayB)
MARKER_KINDS = ("pit",)
FOOT = 6                  # CVM3 04: đầu dò sàn Simon X±6 (ConfigSimon.FootProbeHalfWidth — video: còn đứng khi thân qua mép 5 px, rơi ở 6)
DOOR_SIMON_DX = 12        # Map 1 (CVGP 15): triggerSimonX = cột cửa − 12 (EH 1528 → 1516); Map_3 2 cửa: cột 752, Simon 740 (f4337) / đáp 742 (f2440)


def fail(msg):
    sys.exit("LOI: " + msg)


def is_tread(t8):
    """ô 8x8 bậc thang Level 3: mặt bậc cam (252,152,56) 28 px ở hàng 0-4 (3 hàng đặc + vạch hàng 4)."""
    return int(S.color_mask(t8[0:5], TREAD).sum()) >= 25


def classify_metatile(b):
    if S.color_mask(b, DOORCOL).any():
        return "Door", "L1"
    for ty in (0, 8):
        for tx in (0, 8):
            if is_tread(b[ty:ty + 8, tx:tx + 8]):
                return "Stair", "L2"
    if float(S.color_mask(b, SALMON).mean()) >= 0.40:
        return "Solid", "L3"
    return "Empty", "-"


def find_stairs(sid, reg):
    """Dò bậc 8x8 + nối thành thang thẳng (↗: (x+8, y−8), ↖: (x−8, y−8)) — như build_collision2 (Level 2) với is_tread Level 3."""
    h, w = reg.shape[:2]
    treads = sorted((x, y) for y in range(0, h, 8) for x in range(0, w, 8) if is_tread(reg[y:y + 8, x:x + 8]))
    tset = set(treads)
    used = set()
    out = []
    for (x, y) in sorted(treads, key=lambda p: (-p[1], p[0])):     # từ bậc thấp nhất
        if (x, y) in used:
            continue
        for d, dx in (("UpRight", 8), ("UpLeft", -8)):
            if (x + dx, y - 8) in tset and (x - dx, y + 8) not in tset:
                chain = [(x, y)]
                while (chain[-1][0] + dx, chain[-1][1] - 8) in tset and (chain[-1][0] + dx, chain[-1][1] - 8) not in used:
                    chain.append((chain[-1][0] + dx, chain[-1][1] - 8))
                for p in chain:
                    if p in used:
                        fail("%s: bac %s thuoc 2 thang" % (sid, p))
                    used.add(p)
                out.append({"section": sid, "dir": d, "steps": len(chain), "foot": list(chain[0]), "top": list(chain[-1]),
                            "cutBottom": chain[0][1] + 8 >= h, "cutTop": chain[-1][1] <= 0, "treads": [list(p) for p in chain]})
                break
    if used != tset:
        fail("%s: bac le khong thuoc thang nao: %s" % (sid, sorted(tset - used)))
    out.sort(key=lambda s: (s["foot"][0], s["foot"][1]))
    for k, s in enumerate(out):
        s["id"] = "%s_stair%d" % (sid, k)
    return out


def video_ground_check(grids, markers):
    """Mọi frame Simon đứng yên/đi trên đất ở video ⇒ ô dưới chân phải đặc/thang. Trả (số frame kiểm theo section, list lỗi)."""
    rows = list(csv.DictReader(open(SIMON_CSV)))
    n = Counter()
    bad = []
    for i in range(2, len(rows) - 2):
        r = rows[i]
        if r["found"] != "1" or r["section"] not in grids or r["sprite"] not in ("idle_0", "walk_0", "walk_1", "walk_2"):
            continue
        if float(r["err"]) >= 0.1:
            continue                                                  # khớp mẫu kém (sprite khác đè Simon)
        win = rows[i - 2:i + 3]
        if any(w["found"] != "1" or w["section"] != r["section"] or w["y_sec"] != r["y_sec"] for w in win):
            continue
        sid = r["section"]
        x, y = int(r["world_x"]) + T3.pad_of(sid), int(r["y_sec"])
        g = grids[sid]
        n[sid] += 1
        if y % CELL != 0 or y // CELL >= len(g):
            bad.append((r["frame"], sid, x, y, "y khong o mat o"))
            continue
        rr = y // CELL
        cs = range(max(0, (x - FOOT) // CELL), min(len(g[0]) - 1, (x + FOOT) // CELL) + 1)
        if any(solid_like(g[rr][c]) or g[rr][c] == "Stair" for c in cs):
            continue
        bad.append((r["frame"], sid, x, y, "".join(TYPES[g[rr][c]] for c in cs)))
    return n, bad


def main():
    check = "--check" in sys.argv
    regs = T3.padded_regions()
    ov = json.load(open(OVERRIDES, encoding="utf-8"))
    graph = json.load(open(GRAPH, encoding="utf-8"))
    sections = [(sid, sid, [rect[0], rect[1], rect[2] + T3.pad_of(sid), rect[3]], stage) for sid, rect, stage in T3.SECTIONS]

    # ---------------------------------------------------------------- 1. metatile id + loại tự động
    meta_id = {}
    meta_info = []
    grids, autos = {}, {}
    for sid, _i, (x, y, w, h), _st in sections:
        reg = regs[sid]
        if reg.shape[1] != w:
            fail(sid + ": be rong vung lech")
        cols, rows = w // CELL, h // CELL
        g = [[None] * cols for _ in range(rows)]
        for r in range(rows):
            for c in range(cols):
                b = reg[r * CELL:(r + 1) * CELL, c * CELL:(c + 1) * CELL]
                k = b.tobytes()
                if k not in meta_id:
                    t, rule = classify_metatile(b)
                    meta_id[k] = len(meta_info)
                    meta_info.append({"id": len(meta_info), "auto": t, "rule": rule, "count": 0, "firstSection": sid, "firstCol": c, "firstRow": r})
                mi = meta_info[meta_id[k]]
                mi["count"] += 1
                g[r][c] = mi["auto"]
        grids[sid] = g
        autos[sid] = [row[:] for row in g]
    print("metatile 16x16 duy nhat:", len(meta_info), "(ky vong %d = kiem ke; phan noi WalkwayB khong them)" % EXPECTED_META)
    if len(meta_info) != EXPECTED_META:
        fail("so metatile khac ky vong")
    rc = Counter((mi["auto"], mi["rule"]) for mi in meta_info)
    for (t, rule), n in sorted(rc.items()):
        print("  luat %-3s %-7s %3d metatile" % (rule, t, n))
    if "--dump" in sys.argv:
        for sid in grids:
            print("== auto", sid)
            for r, row in enumerate(grids[sid]):
                print("  %2d %s" % (r, "".join(TYPES[t] for t in row)))

    # ---------------------------------------------------------------- 2. overrides + checks
    def cells(o):
        g = grids.get(o["section"])
        if g is None:
            fail("override section la: " + o["section"])
        c0, c1 = o["cols"]
        r0, r1 = o["rows"]
        if not (0 <= c0 <= c1 < len(g[0]) and 0 <= r0 <= r1 < len(g)):
            fail("override ngoai luoi: %s" % o)
        return [(c, r) for r in range(r0, r1 + 1) for c in range(c0, c1 + 1)]

    overridden = {}
    for o in ov["overrides"]:
        if o["type"] not in TYPES:
            fail("loai la: " + o["type"])
        if not o.get("reason") or not o.get("expectAuto"):
            fail("override thieu reason/expectAuto: %s" % o)
        for (c, r) in cells(o):
            a = autos[o["section"]][r][c]
            if a != o["expectAuto"]:
                fail("%s (%d,%d): tu dong = %s, override mong %s" % (o["section"], c, r, a, o["expectAuto"]))
            grids[o["section"]][r][c] = o["type"]
            overridden[(o["section"], c, r)] = o["type"]
    for ck in ov["checks"]:
        for (c, r) in cells(ck):
            t = grids[ck["section"]][r][c]
            if t != ck["type"] and not (ck["type"] == "Solid" and t in ("Breakable", "StairFloor")):
                fail("check %s (%d,%d): loai %s != %s (%s)" % (ck["section"], c, r, t, ck["type"], ck["evidence"][:40]))
    print("overrides:", len(ov["overrides"]), "dong /", len(overridden), "o; checks:", len(ov["checks"]), "dong - dat")

    # ---------------------------------------------------------------- 3. cầu thang (tự dò)
    stairs = {}
    for sid, _i, _rect, _st in sections:
        for s in find_stairs(sid, regs[sid]):
            stairs[s["id"]] = s
    step_cells = set()
    for st in stairs.values():
        for (tx, ty) in st["treads"]:
            c, r = tx // CELL, ty // CELL
            step_cells.add((st["section"], c, r))
            if grids[st["section"]][r][c] not in ("Stair", "StairFloor"):
                fail("bac %s (%d,%d) nam o o %s" % (st["id"], tx, ty, grids[st["section"]][r][c]))
    for sid, g in grids.items():
        for r, row in enumerate(g):
            for c, t in enumerate(row):
                if t in ("Stair", "StairFloor") and (sid, c, r) not in step_cells:
                    fail("o Stair %s (%d,%d) khong co bac" % (sid, c, r))
    for st in sorted(stairs.values(), key=lambda s: s["id"]):
        print("  %-26s %-7s %2d bac chan %-10s dinh %-10s%s%s" % (st["id"], st["dir"], st["steps"], st["foot"], st["top"],
                                                               " cat-day" if st["cutBottom"] else "", " cat-dinh" if st["cutTop"] else ""))
    print("cau thang:", len(stairs), "- moi bac nam trong o Stair/StairFloor va nguoc lai")

    # ---------------------------------------------------------------- 4. đồ thị section (Level 1 + kiểm Level 3)
    gsec = check_graph(graph, grids, stairs, sections)
    width = {sid: rect[2] for sid, _i, rect, _st in sections}
    for sid, sec in gsec.items():
        g = grids[sid]
        if sec["simonStart"] is not None and sec.get("simonStartDir") not in (-1, 1):
            fail(sid + ": simonStartDir phai -1/1")
        if sec["bossLockCx"] != -1 and sec["bossLockCx"] != sec["cameraMaxX"]:
            fail(sid + ": Block 3 khoa boss o mep PHAI (bossLockCx = cameraMaxX)")
        for m in sec.get("markers", []):
            x, y, w, h = m["rect"]
            if m["kind"] not in MARKER_KINDS or w <= 0 or h <= 0 or x < 0 or y < 0 or x + w > width[sid] or y + h > 192 or not m.get("src"):
                fail(sid + "/" + m["id"] + ": marker sai")
        for cn in sec["connections"]:
            d = cn.get("dir", 1)
            if d not in (-1, 1):
                fail(cn["id"] + ": dir phai -1/1")
            if cn["kind"] == "Door":
                if d != 1:
                    fail(cn["id"] + ": Block 3 chi co cua PHAI")
                tx, ty, tw, th = cn["trigger"]
                if tx < width[sid] - CELL:
                    fail(cn["id"] + ": cua phai phai o cot cuoi")
                fy = ty + th
                if not solid_like(g[fy // CELL][tx // CELL]):
                    fail(cn["id"] + ": duoi cot cua khong co go dung")
                if cn["target"]:
                    if cn["targetOrigin"] != [width[sid], 0]:
                        fail(cn["id"] + ": section dich phai ghep LIEN PHAI (targetOrigin = [rong nguon, 0])")
                    ex, ey = cn["entry"]
                    tg = grids[cn["target"]]
                    if not solid_like(tg[ey // CELL][ex // CELL]) or tg[ey // CELL - 1][ex // CELL] != "Empty" or tg[ey // CELL - 2][ex // CELL] != "Empty":
                        fail(cn["id"] + ": entry khong dung tren o dac")
                    if cn["triggerSimonX"] != tx - DOOR_SIMON_DX:
                        fail(cn["id"] + ": triggerSimonX phai = cot cua - %d" % DOOR_SIMON_DX)
                    if gsec[cn["target"]].get("simonStart") and gsec[cn["target"]]["simonStart"] != cn["entry"]:
                        fail(cn["id"] + ": entry khac simonStart cua stage dich")
    print("kiem Level 3 (cua phai / markers / huong / khoa boss): dat")

    # ---------------------------------------------------------------- 5. hố tự tính ⇒ markers
    markers = {}
    for sid, sec in gsec.items():
        pits = find_pits(sid, grids[sid])
        for p in pits:
            p["src"] = p["src"].replace("build_collision2", "build_collision3")
        sec["markers"] = [m for m in sec.get("markers", []) if m["kind"] != "pit"] + pits
        markers[sid] = sec["markers"]
        for p in pits:
            print("  ho %-17s %s" % (sid, p["rect"]))

    # ---------------------------------------------------------------- 6. đối chiếu video (Simon đứng trên đất)
    if "--no-video" not in sys.argv:
        n, bad = video_ground_check(grids, markers)
        print("doi chieu video: %d frame Simon dung tren dat (%s), %d frame khong co o dac duoi chan" % (
            sum(n.values()), ", ".join("%s %d" % kv for kv in sorted(n.items())), len(bad)))
        for b in bad[:40]:
            print("   ", b)
        if bad:
            fail("luoi lech video (Simon dung o cho khong dac)")

    # ---------------------------------------------------------------- 7. ghi JSON
    out = OrderedDict()
    out["note"] = "CVM3 05 - sinh boi Tools/cvm3/build_collision3.py tu sheet (WalkwayB noi trai 112 px) + level3_collision_overrides.json + level3_sections.json; KHONG sua tay"
    out["cellSize"] = CELL
    out["levelRowToNesY"] = graph["conventions"]["levelRowToNesY"]
    out["types"] = [{"name": k, "char": v} for k, v in TYPES.items()]
    out["order"] = graph["order"]
    out["sections"] = []
    for sid, _i, rect, stage in sections:
        o = section_json(gsec[sid], sid, stage, rect, grids[sid], stairs)
        out["sections"].append(o)
    out["metatiles"] = meta_info
    js = (json.dumps(out, ensure_ascii=True, indent=1) + "\n").encode("ascii")
    path = os.path.join(REPO, OUT_REL)
    old = open(path, "rb").read() if os.path.exists(path) else None
    state = "same" if old == js else ("new" if old is None else "changed")
    print("%s %s (%d B, md5 %s)" % (state, OUT_REL, len(js), hashlib.md5(js).hexdigest()))
    if not check and state != "same":
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "wb") as f:
            f.write(js)

    print("dem o theo loai:")
    print("  %-17s" % "section" + "".join("%11s" % k for k in TYPES))
    for o in out["sections"]:
        print("  %-17s" % o["id"] + "".join("%11d" % o["counts"][k] for k in TYPES))
    if "--no-images" not in sys.argv and not check:
        render(out, regs, IMG_DIR)
        render_markers(out, regs)
    print("CHECK (khong ghi)" if check else "OK")


def render_markers(out, regs):
    """Vẽ thêm marker hố (nét đứt tím) lên ảnh overlay đã có."""
    from PIL import Image, ImageDraw
    Z, CH = 2, 768
    col = {"pit": (150, 0, 255)}
    for o in out["sections"]:
        ms = o.get("markers", [])
        if not ms:
            continue
        p = os.path.join(IMG_DIR, "collision_%s.png" % o["id"])
        img = Image.open(p).convert("RGB")
        d = ImageDraw.Draw(img)
        h = o["height"]
        for m in ms:
            x, y, mw, mh = m["rect"]
            yy0 = (x // CH) * (h * Z + 6)
            x0 = (x % CH) * Z
            for k in range(0, mw * Z, 6):
                d.line([x0 + k, yy0 + y * Z, min(x0 + k + 3, x0 + mw * Z - 1), yy0 + y * Z], fill=col[m["kind"]], width=2)
                d.line([x0 + k, yy0 + (y + mh) * Z - 1, min(x0 + k + 3, x0 + mw * Z - 1), yy0 + (y + mh) * Z - 1], fill=col[m["kind"]], width=2)
            for k in range(0, mh * Z, 6):
                d.line([x0, yy0 + y * Z + k, x0, yy0 + min(y * Z + k + 3, (y + mh) * Z - 1)], fill=col[m["kind"]], width=2)
                d.line([x0 + mw * Z - 1, yy0 + y * Z + k, x0 + mw * Z - 1, yy0 + min(y * Z + k + 3, (y + mh) * Z - 1)], fill=col[m["kind"]], width=2)
            d.text((x0 + 3, yy0 + y * Z + 3), m["kind"], fill=col[m["kind"]])
        img.save(p, optimize=True)


if __name__ == "__main__":
    main()
