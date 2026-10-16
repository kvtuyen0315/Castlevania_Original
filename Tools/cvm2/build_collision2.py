# -*- coding: utf-8 -*-
"""
build_collision2.py - CVM2 05: lưới va chạm (ô 16x16) + cầu thang + đồ thị section của Map 2 (Level 2 = Block 2, Stage 4-6).
Cùng khuôn Tools/cvgp/build_collision.py (CVGP 06) — dùng lại check_graph / section_json / render của nó (Level 1 không đổi).

Chạy từ gốc repo (SAU build_tileset2.py):  python -P -E Tools/cvm2/build_collision2.py [--check] [--no-images] [--no-video]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

1. Phân loại TỰ ĐỘNG theo metatile id (khối 16x16 duy nhất của vùng ĐÃ VÁ teal - Tools/cvm2/build_tileset2.py):
     L1 có màu cột cửa (255,136,51)                           ⇒ Door   (cột cửa đỏ-trắng 8x48, x 8-15)
     L2 có ô 8x8 bậc thang (≥ 25 px (216,40,0) hàng 1-5 cột 0-6) ⇒ Stair
     L3 cam (200,76,12) ≥ 45 %                                ⇒ Solid  (khối cam viền xanh 16x16)
     còn lại                                                  ⇒ Empty  (gạch nền, trụ, vòm, cửa sổ, đá xám, tượng Medusa…)
2. Áp Tools/cvm2/level2_collision_overrides.json (overrides có expectAuto + reason + bằng chứng video; checks = chỗ đã xem video).
3. Cầu thang = TỰ DÒ từ sheet (bậc 8x8 nối chéo ±8/−8) — mọi bậc trong ô Stair và ngược lại; tên <Section>_stair<k> theo (chân x, y).
4. Đồ thị Tools/cvm2/level2_sections.json — check_graph Level 1 + kiểm thêm Level 2: cửa TRÁI (dir −1: trigger mép trái,
   section đích ghép liền trái, entry đứng trên ô đặc), simonStartDir, markers trong section, bossLockCx trong biên.
5. Hố (pit) TỰ TÍNH: khe trên một hàng sàn (2 bên đặc) mà cột bên dưới không còn ô đặc/thang tới đáy ⇒ marker kind "pit"
   (lưới KHÔNG đổi: rơi ra đáy section ⇒ TileCollision.CheckDeadly trả Pit — như hố Arena CVGP 07).
6. --video (mặc định bật): đối chiếu mọi frame Simon ĐỨNG trên đất ở video Map_2 (ClaudeNotes/cvm2/ref/data/map2_simon.csv:
   idle/walk, err khớp mẫu < 0.1, Y giữ nguyên ±2 frame) ⇒ ô dưới chân (bề rộng thân ±4) phải đặc/thang, trừ khi ở làn bệ di động.
Đầu ra:  Assets/Data/Config/Level/Block2/Level2Map.json (CvgpLevelBuilder đọc — menu CVM2/Build Map2 Tilemaps)
         ClaudeNotes/cvm2/ref/level2/collision_<Section>.png (ảnh overlay 2x; --no-images để bỏ)
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
sys.path.insert(0, TOOLS_DIR)
import sheetlib as S  # noqa: E402
from build_collision import TYPES, CELL, check_graph, section_json, render  # noqa: E402
import build_tileset2 as T2  # noqa: E402

OVERRIDES = os.path.join(REPO, "Tools", "cvm2", "level2_collision_overrides.json")
GRAPH = os.path.join(REPO, "Tools", "cvm2", "level2_sections.json")
OUT_REL = T2.CFG_DIR + "/Level2Map.json"
IMG_DIR = os.path.join(REPO, "ClaudeNotes", "cvm2", "ref", "level2")
SIMON_CSV = os.path.join(REPO, "ClaudeNotes", "cvm2", "ref", "data", "map2_simon.csv")

ORANGE = (200, 76, 12)
TREAD = (216, 40, 0)
DOORCOL = (255, 136, 51)
EXPECTED_META = 68        # kiểm kê 69 metatile − 1 (teal thuần ⇒ đen, đen đã có)
MARKER_KINDS = ("crusher", "statue", "statueHead", "platformLane", "pit")


def fail(msg):
    sys.exit("LOI: " + msg)


def is_tread(t8):
    """ô 8x8 bậc thang Level 2: mặt bậc (216,40,0) đặc ở hàng 1-5 cột 0-6 (hàng 0 đen, viền xám phải-dưới)."""
    return int(S.color_mask(t8[1:6, 0:7], TREAD).sum()) >= 25


def classify_metatile(b):
    if S.color_mask(b, DOORCOL).any():
        return "Door", "L1"
    for ty in (0, 8):
        for tx in (0, 8):
            if is_tread(b[ty:ty + 8, tx:tx + 8]):
                return "Stair", "L2"
    if float(S.color_mask(b, ORANGE).mean()) >= 0.45:
        return "Solid", "L3"
    return "Empty", "-"


def find_stairs(sid, reg):
    """Dò bậc 8x8 + nối thành thang thẳng (↗: (x+8, y−8), ↖: (x−8, y−8)). Trả list dict như Level 1 (treads = mọi bậc)."""
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
                while (chain[-1][0] + dx, chain[-1][1] - 8) in tset:
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


def solid_like(t):
    return t in ("Solid", "StairFloor", "Breakable")


def find_pits(sid, g):
    """Khe trên hàng sàn: hàng r có ô đặc 2 bên, các ô giữa không đặc và ô trên 2 bên trống (mặt đi được); cột khe không còn ô
    đặc/thang từ r xuống hàng cuối ⇒ hố. Trả list marker pit (rect = cột khe × từ mặt sàn tới đáy)."""
    rows, cols = len(g), len(g[0])
    pits = []
    for r in range(1, rows):
        c = 0
        while c < cols:
            if solid_like(g[r][c]) or g[r][c] == "Stair":
                c += 1
                continue
            c0 = c
            while c < cols and not (solid_like(g[r][c]) or g[r][c] == "Stair"):
                c += 1
            c1 = c - 1
            if c0 == 0 or c >= cols or c1 - c0 + 1 > 8:
                continue
            if not (solid_like(g[r][c0 - 1]) and solid_like(g[r][c1 + 1])):
                continue
            if solid_like(g[r - 1][c0 - 1]) or solid_like(g[r - 1][c1 + 1]):
                continue                                              # không phải mặt sàn (tường)
            open_cols = [cc for cc in range(c0, c1 + 1)
                         if all(not (solid_like(g[rr][cc]) or g[rr][cc] == "Stair") for rr in range(r, rows))]
            if not open_cols:
                continue
            # gộp cột hở liền nhau thành rect
            runs = []
            for cc in open_cols:
                if runs and runs[-1][1] == cc - 1:
                    runs[-1][1] = cc
                else:
                    runs.append([cc, cc])
            for a, b in runs:
                pits.append({"id": "pit_%d_%d" % (r, a), "kind": "pit", "rect": [a * CELL, r * CELL, (b - a + 1) * CELL, rows * CELL - r * CELL],
                             "src": "tu dong (build_collision2): khe san hang %d cot %d-%d (2 ben dac), cot %d-%d trong toi day section => roi ra day = chet Pit (TileCollision.CheckDeadly)" % (r, c0, c1, a, b)})
    return pits


def video_ground_check(grids, markers):
    """Mọi frame Simon đứng yên/đi trên đất ở video ⇒ ô dưới chân phải đặc/thang. Trả (số frame kiểm, list lỗi)."""
    rows = list(csv.DictReader(open(SIMON_CSV)))
    lanes = {sid: [m["rect"] for m in ms if m["kind"] == "platformLane"] for sid, ms in markers.items()}
    n = 0
    bad = []
    for i in range(2, len(rows) - 2):
        r = rows[i]
        if r["found"] != "1" or r["section"] not in grids or r["sprite"] not in ("idle_0", "walk_0", "walk_1", "walk_2"):
            continue
        if float(r["err"]) >= 0.1:
            continue                                                  # khớp mẫu kém (sprite khác đè Simon — vd f6891-6895 Medusa)
        win = rows[i - 2:i + 3]
        if any(w["found"] != "1" or w["section"] != r["section"] or w["y_sec"] != r["y_sec"] for w in win):
            continue
        x, y = int(r["world_x"]), int(r["y_sec"])
        g = grids[r["section"]]
        n += 1
        if y % CELL != 0 or y // CELL >= len(g):
            bad.append((r["frame"], r["section"], x, y, "y khong o mat o"))
            continue
        rr = y // CELL
        cs = range(max(0, (x - 4) // CELL), min(len(g[0]) - 1, (x + 4) // CELL) + 1)
        if any(solid_like(g[rr][c]) or g[rr][c] == "Stair" for c in cs):
            continue
        if any(lx <= x <= lx + lw and y == ly for (lx, ly, lw, _lh) in lanes.get(r["section"], [])):
            continue                                                  # đứng trên bệ di động
        bad.append((r["frame"], r["section"], x, y, "".join(TYPES[g[rr][c]] for c in cs)))
    return n, bad


def main():
    check = "--check" in sys.argv
    regs = T2.patched_regions()
    ov = json.load(open(OVERRIDES, encoding="utf-8"))
    graph = json.load(open(GRAPH, encoding="utf-8"))
    sections = [(sid, sid, rect, stage) for sid, rect, stage in T2.SECTIONS]

    # ---------------------------------------------------------------- 1. metatile id + loại tự động
    meta_id = {}
    meta_info = []
    grids, autos = {}, {}
    for sid, _i, (x, y, w, h), _st in sections:
        reg = regs[sid]
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
    print("metatile 16x16 duy nhat:", len(meta_info), "(kiem ke 69 - 1 teal = %d)" % EXPECTED_META)
    if len(meta_info) != EXPECTED_META:
        fail("so metatile khac ky vong")
    rc = Counter((mi["auto"], mi["rule"]) for mi in meta_info)
    for (t, rule), n in sorted(rc.items()):
        print("  luat %-3s %-7s %3d metatile" % (rule, t, n))

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
            if t != ck["type"] and not (ck["type"] == "Solid" and t == "Breakable") and not (ck["type"] == "Stair" and t == "Solid" and c == 29):
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
        print("  %-22s %-7s %2d bac chan %-10s dinh %-10s%s%s" % (st["id"], st["dir"], st["steps"], st["foot"], st["top"],
                                                               " cat-day" if st["cutBottom"] else "", " cat-dinh" if st["cutTop"] else ""))
    print("cau thang:", len(stairs), "- moi bac nam trong o Stair va nguoc lai")

    # ---------------------------------------------------------------- 4. đồ thị section (Level 1 + kiểm Level 2)
    gsec = check_graph(graph, grids, stairs, sections)
    width = {sid: rect[2] for sid, _i, rect, _st in sections}
    for sid, sec in gsec.items():
        g = grids[sid]
        if sec["simonStart"] is not None and sec.get("simonStartDir") not in (-1, 1):
            fail(sid + ": simonStartDir phai -1/1")
        if sec["bossLockCx"] != -1 and not (sec["cameraMinX"] <= sec["bossLockCx"] <= sec["cameraMaxX"]):
            fail(sid + ": bossLockCx ngoai bien camera")
        for m in sec.get("markers", []):
            x, y, w, h = m["rect"]
            if m["kind"] not in MARKER_KINDS or w <= 0 or h <= 0 or x < 0 or y < 0 or x + w > width[sid] or y + h > 192 or not m.get("src"):
                fail(sid + "/" + m["id"] + ": marker sai")
        for cn in sec["connections"]:
            d = cn.get("dir", 1)
            if d not in (-1, 1):
                fail(cn["id"] + ": dir phai -1/1")
            if cn["kind"] == "Door" and d == -1:
                tx, ty, tw, th = cn["trigger"]
                if tx >= CELL:
                    fail(cn["id"] + ": cua trai phai o cot 0")
                fy = ty + th
                if not solid_like(g[fy // CELL][0]):
                    fail(cn["id"] + ": duoi cot cua khong co go dung")
                if cn["target"]:
                    if cn["targetOrigin"] != [-width[cn["target"]], 0]:
                        fail(cn["id"] + ": section dich phai ghep LIEN TRAI (targetOrigin = [-rong dich, 0])")
                    ex, ey = cn["entry"]
                    tg = grids[cn["target"]]
                    if ey != fy or not solid_like(tg[ey // CELL][ex // CELL]) or tg[ey // CELL - 1][ex // CELL] != "Empty":
                        fail(cn["id"] + ": entry khong dung tren o dac cung do cao go cua")
                    if not (tx + tw <= cn["triggerSimonX"] < 2 * CELL):
                        fail(cn["id"] + ": triggerSimonX la")
    print("kiem Level 2 (cua trai / markers / huong): dat")

    # ---------------------------------------------------------------- 5. hố tự tính ⇒ markers
    markers = {}
    for sid, sec in gsec.items():
        pits = find_pits(sid, grids[sid])
        sec["markers"] = [m for m in sec.get("markers", []) if m["kind"] != "pit"] + pits
        markers[sid] = sec["markers"]
        for p in pits:
            print("  ho %-15s %s" % (sid, p["rect"]))

    # ---------------------------------------------------------------- 6. đối chiếu video (Simon đứng trên đất)
    if "--no-video" not in sys.argv:
        n, bad = video_ground_check(grids, markers)
        print("doi chieu video: %d frame Simon dung tren dat, %d frame khong co o dac duoi chan" % (n, len(bad)))
        for b in bad[:30]:
            print("   ", b)
        if bad:
            fail("luoi lech video (Simon dung o cho khong dac)")

    # ---------------------------------------------------------------- 7. ghi JSON
    out = OrderedDict()
    out["note"] = "CVM2 05 - sinh boi Tools/cvm2/build_collision2.py tu sheet (da va teal) + level2_collision_overrides.json + level2_sections.json; KHONG sua tay"
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
    print("  %-15s" % "section" + "".join("%11s" % k for k in TYPES))
    for o in out["sections"]:
        print("  %-15s" % o["id"] + "".join("%11d" % o["counts"][k] for k in TYPES))
    if "--no-images" not in sys.argv and not check:
        render(out, regs, IMG_DIR)
        render_markers(out, regs)
    print("CHECK (khong ghi)" if check else "OK")


def render_markers(out, regs):
    """Vẽ thêm markers (bẫy ép, tượng, làn bệ, hố) lên ảnh overlay đã có (đè viền nét đứt)."""
    from PIL import Image, ImageDraw
    Z, CH = 2, 768
    col = {"crusher": (255, 255, 255), "statue": (0, 255, 255), "statueHead": (0, 128, 255), "platformLane": (255, 128, 255), "pit": (150, 0, 255)}
    for o in out["sections"]:
        ms = o.get("markers", [])
        if not ms:
            continue
        p = os.path.join(IMG_DIR, "collision_%s.png" % o["id"])
        img = Image.open(p).convert("RGB")
        d = ImageDraw.Draw(img)
        w, h = o["width"], o["height"]
        for m in ms:
            x, y, mw, mh = m["rect"]
            # đoạn 768 px xếp dọc: section 768 ⇒ 1 đoạn
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
