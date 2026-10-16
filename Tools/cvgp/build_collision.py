# -*- coding: utf-8 -*-
"""
build_collision.py - CVGP 06: lưới va chạm (ô 16x16) + cầu thang + đồ thị section của Map 1 (Level 1, Stage 1-3).

Chạy từ gốc repo (SAU build_tileset.py):  python -P -E Tools/cvgp/build_collision.py [--check] [--no-images]
  (KHÔNG -I: Pillow ở user site; không in tiếng Việt có dấu ra console - cp1252)

1. Phân loại TỰ ĐỘNG theo metatile id (khối 16x16 duy nhất toàn Level 1 - cùng id ⇒ cùng loại):
     R1 có pixel trắng (252,252,252)                         ⇒ Door   (cột cửa đỏ-trắng)
     R2 có vạch bậc thang cam 8x2 (diện tích 15)             ⇒ Stair  (ô chứa bậc 8x8)
     R3 cam (200,76,12) ≥ 45 % và đen < 20 %                 ⇒ Solid  (gạch cam 16x16)
     R4 ≥ 3 viên gạch nhỏ cam 6x5 (diện tích 28)             ⇒ Solid  (tường gạch nhỏ Crypt A)
     R5 ≥ 2 viên gạch nhỏ 6x5 và cam ≥ 40 %                  ⇒ Solid  (sàn lát sân Castle Entrance)
     còn lại                                                 ⇒ Empty  (nền, lan can, song sắt, trụ, rèm, cây…)
2. Áp `Tools/cvgp/level1_collision_overrides.json` (overrides có expectAuto + lý do; checks = chỗ nghi ngờ đã xem video).
3. Kiểm chéo với bản đồ ký tự kiểm kê planner (inventory/level1_entrance.json block_maps): khác biệt phải ĐÚNG bằng các ô override.
4. Cầu thang = kiểm kê `stairs` (13 thang, bậc 8x8) - kiểm mọi bậc nằm trong ô Stair/StairFloor và ngược lại;
   nối section `Tools/cvgp/level1_sections.json` - kiểm đích tồn tại, thang nối liền (bậc kế tiếp qua mép = bậc đầu thang đích).
Đầu ra:  Assets/Data/Config/Level/Block1/Level1Map.json   (CvgpLevelBuilder đọc)
         ClaudeNotes/cvgp/ref/level1/collision_<Section>.png  (ảnh overlay 2x; --no-images để bỏ)
Ký tự lưới (1 ký tự / ô 16x16, hàng 0 = trên): . Empty  # Solid  S Stair  = StairFloor  ~ Water  P Pit  D Door  X Breakable  ! Hazard
CVM2 05: check_graph / section_json / render tách hàm để Tools/cvm2/build_collision2.py (Level 2) dùng lại — đầu ra Level 1 KHÔNG đổi (H12).
"""
import hashlib
import json
import os
import sys
from collections import Counter, OrderedDict

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sheetlib as S  # noqa: E402
from build_tileset import SECTIONS, SHEET, SHEET_SHA256, REPO, CFG_DIR, sha256  # noqa: E402

OVERRIDES = os.path.join(REPO, "Tools", "cvgp", "level1_collision_overrides.json")
GRAPH = os.path.join(REPO, "Tools", "cvgp", "level1_sections.json")
INVENTORY = os.path.join(REPO, "ClaudeNotes", "cvgp", "inventory", "level1_entrance.json")
OUT_REL = CFG_DIR + "/Level1Map.json"
IMG_DIR = os.path.join(REPO, "ClaudeNotes", "cvgp", "ref", "level1")

ORANGE = (200, 76, 12)
WHITE = (252, 252, 252)
BLACK = (0, 0, 0)
CELL = 16

# tên loại (= tên enum eTileCollision trong Unity) -> ký tự lưới; thứ tự = giá trị enum
TYPES = OrderedDict([("Empty", "."), ("Solid", "#"), ("Stair", "S"), ("StairFloor", "="), ("Water", "~"),
                     ("Pit", "P"), ("Door", "D"), ("Breakable", "X"), ("Hazard", "!")])
CHAR2TYPE = {v: k for k, v in TYPES.items()}
# ký tự kiểm kê planner -> loại tự động mong đợi (để kiểm chéo)
INV2TYPE = {"B": "Solid", "F": "Solid", "K": "Solid", "U": "Stair", "L": "Stair", "W": "Water", "D": "Door",
            "R": "Empty", "G": "Empty", ".": "Empty"}
COLORS = {"Solid": (255, 60, 60), "Stair": (255, 230, 0), "StairFloor": (255, 140, 0), "Water": (0, 170, 255),
          "Pit": (150, 0, 255), "Door": (255, 0, 255), "Breakable": (0, 255, 80), "Hazard": (255, 255, 255)}


def classify_metatile(b):
    """b: 16x16x3. Trả (loại, mã luật)."""
    om = S.color_mask(b, ORANGE)
    if S.color_mask(b, WHITE).any():
        return "Door", "R1"
    lab, n = S.label_components(om, 8)
    st = S.comp_stats(lab, n)
    if any(s["w"] == 8 and s["h"] == 2 and s["area"] == 15 for s in st):
        return "Stair", "R2"
    fo = float(om.mean())
    fk = float(S.color_mask(b, BLACK).mean())
    if fo >= 0.45 and fk < 0.2:
        return "Solid", "R3"
    small = sum(1 for s in st if s["w"] == 6 and s["h"] == 5 and s["area"] == 28)
    if small >= 3:
        return "Solid", "R4"
    if small >= 2 and fo >= 0.40:
        return "Solid", "R5"
    return "Empty", "-"


def fail(msg):
    sys.exit("LOI: " + msg)


def check_graph(graph, grids, stairs, sections):
    """Kiểm đồ thị section (dùng chung CVGP 06 / CVM2 05): biên camera, simonStart đứng trên ô đặc, trigger cửa trùng ô Door,
    thang nối liền qua phép dời targetOrigin, mọi thang chạm mép đều có nối. sections = [(id, idKiểmKê, rect, stage)]. Trả {id: section}."""
    gsec = {s["id"]: s for s in graph["sections"]}
    if sorted(gsec) != sorted(sid for sid, _i, _r, _s in sections):
        fail("do thi section thieu/thua section")
    for sid, sec in gsec.items():
        w = [r[2] for (a, _i, r, _st) in sections if a == sid][0]
        if sec["cameraMinX"] != 0 or sec["cameraMaxX"] != w - 256:
            fail(sid + ": bien camera phai la [0, rong-256]")
        if sec["simonStart"] is not None:
            x, yy = sec["simonStart"]
            c, r = x // CELL, yy // CELL
            if grids[sid][r][c] not in ("Solid", "StairFloor"):
                fail(sid + ": simonStart khong dung tren o dac")
            if grids[sid][r - 1][c] != "Empty" or grids[sid][r - 2][c] != "Empty":
                fail(sid + ": simonStart bi chan tren dau")
        for fg in sec["foreground"]:
            if not fg.get("src"):
                fail(sid + ": foreground thieu src")
        for cn in sec["connections"]:
            if not cn.get("src"):
                fail(sid + "/" + cn["id"] + ": thieu src")
            if cn["target"] and cn["target"] not in gsec:
                fail(sid + "/" + cn["id"] + ": dich la")
            if cn["kind"] == "Door":
                tx, ty, tw, th = cn["trigger"]
                cs = {grids[sid][r][c] for r in range(ty // CELL, (ty + th - 1) // CELL + 1) for c in range(tx // CELL, (tx + tw - 1) // CELL + 1)}
                if cs != {"Door"}:
                    fail(sid + "/" + cn["id"] + ": trigger khong trung o Door " + str(cs))
                if cn["target"] and "triggerSimonX" not in cn:
                    fail(sid + "/" + cn["id"] + ": thieu triggerSimonX")
            if cn["kind"] == "Stair":
                a = stairs[cn["stair"]]
                b = stairs[cn["targetStair"]]
                ox, oy = cn["targetOrigin"]
                dx_down = 8 if a["dir"] == "UpLeft" else -8   # đi xuống: ↖ sang phải, ↗ sang trái
                if a["cutBottom"]:
                    nxt = (a["foot"][0] + dx_down, a["foot"][1] + 8)       # bậc kế tiếp phía dưới mép
                    want = b["top"]
                    if not b["cutTop"] or b["dir"] != a["dir"] or [nxt[0] - ox, nxt[1] - oy] != want:
                        fail("%s: thang %s -> %s khong noi lien (%s -> %s)" % (cn["id"], a["id"], b["id"], nxt, want))
                elif a["cutTop"]:
                    prv = (a["top"][0] - dx_down, a["top"][1] - 8)        # bậc kế tiếp phía trên mép
                    want = b["foot"]
                    if not b["cutBottom"] or b["dir"] != a["dir"] or [prv[0] - ox, prv[1] - oy] != want:
                        fail("%s: thang %s -> %s khong noi lien (%s -> %s)" % (cn["id"], a["id"], b["id"], prv, want))
                else:
                    fail(cn["id"] + ": thang khong cham mep section")
    linked = {cn["stair"] for s in gsec.values() for cn in s["connections"] if cn["kind"] == "Stair"}
    for st in stairs.values():
        if (st["cutBottom"] or st["cutTop"]) != (st["id"] in linked):
            fail(st["id"] + ": thang cham mep nhung khong co noi (hoac nguoc lai)")
    print("do thi section: dat (", sum(len(s["connections"]) for s in gsec.values()), "noi )")
    return gsec


def section_json(sec, sid, stage, rect, g, stairs):
    """1 section ra JSON (schema Level1Map.json — CvgpLevelBuilder đọc). Khoá CVM2 05 (dir của nối, simonStartDir, markers) CHỈ ghi khi
    có trong dữ liệu nguồn ⇒ Level 1 (không có các khoá này) ra byte y hệt."""
    cnt = Counter(t for row in g for t in row)
    o = OrderedDict()
    o["id"] = sid
    o["stage"] = stage
    if sec["stage"] != stage:
        fail(sid + ": stage lech")
    o["width"], o["height"] = rect[2], rect[3]
    o["cols"], o["rows"] = rect[2] // CELL, rect[3] // CELL
    o["cameraMinX"], o["cameraMaxX"], o["bossLockCx"] = sec["cameraMinX"], sec["cameraMaxX"], sec["bossLockCx"]
    o["hasSimonStart"] = sec["simonStart"] is not None
    o["simonStart"] = sec["simonStart"] or [0, 0]
    if "simonStartDir" in sec:
        o["simonStartDir"] = sec["simonStartDir"]
    o["simonStartSrc"] = sec["simonStartSrc"] + ((" | camera: " + sec["bossLockSrc"]) if sec.get("bossLockSrc") else "")
    o["collision"] = ["".join(TYPES[t] for t in row) for row in g]
    o["counts"] = OrderedDict((k, cnt.get(k, 0)) for k in TYPES)
    o["foreground"] = [{"cols": f["cols"], "rows": f["rows"], "src": f["src"]} for f in sec["foreground"]]
    o["stairs"] = [{k: st[k] for k in ("id", "dir", "steps", "foot", "top", "cutBottom", "cutTop")}
                   for st in sorted(stairs.values(), key=lambda s: s["id"]) if st["section"] == sid]
    if "markers" in sec:
        o["markers"] = [OrderedDict([("id", m["id"]), ("kind", m["kind"]), ("rect", m["rect"]), ("src", m["src"])]) for m in sec["markers"]]
    o["connections"] = []
    for cn in sec["connections"]:
        c2 = OrderedDict()
        c2["id"], c2["kind"], c2["target"], c2["targetStage"] = cn["id"], cn["kind"], cn["target"], cn["targetStage"]
        c2["trigger"] = cn.get("trigger", [0, 0, 0, 0])
        if cn["kind"] == "Stair":
            st = stairs[cn["stair"]]
            tx = min(st["foot"][0], st["top"][0])
            ty = min(st["foot"][1], st["top"][1])
            c2["trigger"] = [tx, ty, abs(st["foot"][0] - st["top"][0]) + 8, abs(st["foot"][1] - st["top"][1]) + 8]
        c2["triggerSimonX"] = cn.get("triggerSimonX", -1)
        c2["stair"], c2["targetStair"] = cn.get("stair", ""), cn.get("targetStair", "")
        c2["targetOrigin"] = cn["targetOrigin"]
        c2["entry"] = cn.get("entry", [0, 0])
        if "dir" in cn:
            c2["dir"] = cn["dir"]
        c2["src"] = cn["src"]
        o["connections"].append(c2)
    return o


def main():
    check = "--check" in sys.argv
    if sha256(SHEET) != SHEET_SHA256:
        fail("sheet sai sha256")
    rgb = np.asarray(Image.open(SHEET).convert("RGB"))
    inv = json.load(open(INVENTORY, encoding="utf-8"))
    ov = json.load(open(OVERRIDES, encoding="utf-8"))
    graph = json.load(open(GRAPH, encoding="utf-8"))
    sid_of_inv = {iid: sid for sid, iid, _r, _s in SECTIONS}

    # ---------------------------------------------------------------- 1. metatile id + loại tự động
    meta_id = {}
    meta_info = []          # [{id, auto, rule, count, firstAt}]
    grids = {}              # sid -> list[list[type]]
    autos = {}
    mids = {}
    regs = {}
    for sid, iid, (x, y, w, h), _stage in SECTIONS:
        reg = rgb[y:y + h, x:x + w]
        regs[sid] = reg
        cols, rows = w // CELL, h // CELL
        g = [[None] * cols for _ in range(rows)]
        m = [[0] * cols for _ in range(rows)]
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
                m[r][c] = mi["id"]
                g[r][c] = mi["auto"]
        grids[sid] = g
        autos[sid] = [row[:] for row in g]
        mids[sid] = m
    print("metatile 16x16 duy nhat:", len(meta_info), "(kiem ke 164)")
    if len(meta_info) != inv["tile_stats_total"]["unique16"]:
        fail("so metatile khac kiem ke")
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
        if not o.get("reason"):
            fail("override thieu reason: %s" % o)
        for (c, r) in cells(o):
            a = autos[o["section"]][r][c]
            if o.get("expectAuto") and a != o["expectAuto"]:
                fail("%s (%d,%d): tu dong = %s, override mong %s" % (o["section"], c, r, a, o["expectAuto"]))
            grids[o["section"]][r][c] = o["type"]
            overridden[(o["section"], c, r)] = o["type"]
    for ck in ov["checks"]:
        for (c, r) in cells(ck):
            t = grids[ck["section"]][r][c]
            if t != ck["type"]:
                fail("check %s (%d,%d): loai %s != %s (%s)" % (ck["section"], c, r, t, ck["type"], ck["evidence"][:40]))
    print("overrides:", len(ov["overrides"]), "dong /", len(overridden), "o; checks:", len(ov["checks"]), "dong - dat")

    # ---------------------------------------------------------------- 3. kiểm chéo kiểm kê planner
    diffs = []
    for sid, iid, _rect, _stage in SECTIONS:
        bm = inv["block_maps"][iid]
        for r, line in enumerate(bm):
            for c, ch in enumerate(line):
                want = INV2TYPE[ch]
                got = grids[sid][r][c]
                if want != got:
                    diffs.append((sid, c, r, ch, got))
    unexplained = [d for d in diffs if (d[0], d[1], d[2]) not in overridden or overridden[(d[0], d[1], d[2])] != d[4]]
    print("kiem cheo kiem ke: %d o khac, %d o khong giai thich bang override" % (len(diffs), len(unexplained)))
    for d in unexplained[:20]:
        print("   ", d)
    if unexplained:
        fail("luoi lech kiem ke ngoai override")

    # ---------------------------------------------------------------- 4. cầu thang
    stairs = {}
    for s in inv["stairs"]:
        sid = sid_of_inv[s["region"]]
        same = [x for x in inv["stairs"] if x["region"] == s["region"]]
        k = same.index(s)
        nid = "%s_stair%d" % (sid, k)
        d = "UpRight" if s["direction"] == "up_right" else "UpLeft"
        h = [r[3] for (a, _i, r, _st) in SECTIONS if a == sid][0]
        stairs[nid] = {"id": nid, "section": sid, "dir": d, "steps": s["n_steps"],
                       "foot": s["foot_tread_local"], "top": s["top_tread_local"],
                       "cutBottom": s["foot_tread_local"][1] + 8 >= h, "cutTop": s["top_tread_local"][1] <= 0,
                       "treads": s["steps_local"]}
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
    print("cau thang:", len(stairs), "- moi bac nam trong o Stair/StairFloor va nguoc lai")

    # ---------------------------------------------------------------- 5. đồ thị section
    gsec = check_graph(graph, grids, stairs, SECTIONS)

    # ---------------------------------------------------------------- 6. ghi JSON
    out = OrderedDict()
    out["note"] = "CVGP 06 - sinh boi Tools/cvgp/build_collision.py tu sheet + level1_collision_overrides.json + level1_sections.json; KHONG sua tay"
    out["cellSize"] = CELL
    out["levelRowToNesY"] = graph["conventions"]["levelRowToNesY"]
    out["types"] = [{"name": k, "char": v} for k, v in TYPES.items()]
    out["order"] = graph["order"]
    out["sections"] = []
    for sid, iid, rect, stage in SECTIONS:
        out["sections"].append(section_json(gsec[sid], sid, stage, rect, grids[sid], stairs))
    out["metatiles"] = meta_info
    js = (json.dumps(out, ensure_ascii=True, indent=1) + "\n").encode("ascii")
    path = os.path.join(REPO, OUT_REL)
    old = open(path, "rb").read() if os.path.exists(path) else None
    state = "same" if old == js else ("new" if old is None else "changed")
    print("%s %s (%d B, md5 %s)" % (state, OUT_REL, len(js), hashlib.md5(js).hexdigest()))
    if not check and state != "same":
        with open(path, "wb") as f:
            f.write(js)

    # bảng đếm
    print("dem o theo loai:")
    print("  %-15s" % "section" + "".join("%11s" % k for k in TYPES))
    for o in out["sections"]:
        print("  %-15s" % o["id"] + "".join("%11d" % o["counts"][k] for k in TYPES))

    if "--no-images" not in sys.argv and not check:
        render(out, regs)
    print("CHECK (khong ghi)" if check else "OK")


def render(out, regs, img_dir=IMG_DIR):
    os.makedirs(img_dir, exist_ok=True)
    Z = 2
    CH = 768   # cắt section dài thành đoạn 768 px xếp dọc
    for o in out["sections"]:
        reg = regs[o["id"]]
        w, h = o["width"], o["height"]
        base = Image.fromarray(reg).convert("RGBA").resize((w * Z, h * Z), Image.NEAREST)
        ov = Image.new("RGBA", base.size, (0, 0, 0, 0))
        d = ImageDraw.Draw(ov)
        for r, line in enumerate(o["collision"]):
            for c, ch in enumerate(line):
                t = CHAR2TYPE[ch]
                if t == "Empty":
                    continue
                col = COLORS[t]
                d.rectangle([c * CELL * Z, r * CELL * Z, (c + 1) * CELL * Z - 1, (r + 1) * CELL * Z - 1], fill=col + (110,), outline=col + (255,))
        for f in o["foreground"]:
            x0, x1 = f["cols"][0] * CELL * Z, (f["cols"][1] + 1) * CELL * Z - 1
            y0, y1 = f["rows"][0] * CELL * Z, (f["rows"][1] + 1) * CELL * Z - 1
            for k in range(x0 - (y1 - y0), x1, 12):
                d.line([max(k, x0), y0 + max(0, x0 - k), min(k + (y1 - y0), x1), y0 + min(y1 - y0, x1 - k)], fill=(0, 255, 255, 200), width=1)
            d.rectangle([x0, y0, x1, y1], outline=(0, 255, 255, 255), width=2)
        for st in o["stairs"]:
            n = st["steps"]
            dx = 8 if st["dir"] == "UpRight" else -8
            for k in range(n):
                tx, ty = st["foot"][0] + dx * k, st["foot"][1] - 8 * k
                d.line([tx * Z, ty * Z, (tx + 8) * Z - 1, ty * Z], fill=(0, 0, 0, 255), width=2)
            fx, fy = st["foot"]
            d.ellipse([fx * Z - 4, (fy + 8) * Z - 4, fx * Z + 4, (fy + 8) * Z + 4], outline=(0, 0, 0, 255), width=2)
        for cn in o["connections"]:
            x, y, tw, th = cn["trigger"]
            d.rectangle([x * Z, y * Z, (x + tw) * Z - 1, (y + th) * Z - 1], outline=(255, 255, 255, 255), width=2)
            d.text((x * Z + 2, max(0, y * Z - 12)), cn["id"], fill=(255, 255, 255, 255))
            if cn["triggerSimonX"] >= 0:
                d.line([cn["triggerSimonX"] * Z, 0, cn["triggerSimonX"] * Z, h * Z], fill=(255, 255, 255, 200), width=1)
        if o["hasSimonStart"]:
            sx, sy = o["simonStart"]
            d.rectangle([(sx - 8) * Z, (sy - 32) * Z, (sx + 8) * Z - 1, sy * Z - 1], outline=(0, 255, 0, 255), width=2)
            d.text(((sx - 8) * Z, (sy - 32) * Z - 12), "start", fill=(0, 255, 0, 255))
        img = Image.alpha_composite(base, ov)
        # lưới 16 px mờ
        g = ImageDraw.Draw(img)
        for c in range(0, w + 1, CELL):
            g.line([c * Z, 0, c * Z, h * Z], fill=(255, 255, 255, 40))
        for r in range(0, h + 1, CELL):
            g.line([0, r * Z, w * Z, r * Z], fill=(255, 255, 255, 40))
        parts = [img.crop((x * Z, 0, min(w, x + CH) * Z, h * Z)) for x in range(0, w, CH)]
        legend_h = 28
        H = sum(p.height + 6 for p in parts) + legend_h
        out_img = Image.new("RGBA", (CH * Z, H), (40, 40, 40, 255))
        yy = 0
        for i, p in enumerate(parts):
            out_img.paste(p, (0, yy))
            ImageDraw.Draw(out_img).text((4, yy + 2), "%s x %d..%d" % (o["id"], i * CH, min(w, (i + 1) * CH) - 1), fill=(255, 255, 255, 255))
            yy += p.height + 6
        ld = ImageDraw.Draw(out_img)
        lx = 4
        for t, col in COLORS.items():
            ld.rectangle([lx, yy + 6, lx + 14, yy + 20], fill=col + (255,))
            ld.text((lx + 18, yy + 8), t, fill=(255, 255, 255, 255))
            lx += 26 + 7 * len(t) + 8
        ld.rectangle([lx, yy + 6, lx + 14, yy + 20], outline=(0, 255, 255, 255), width=2)
        ld.text((lx + 18, yy + 8), "FG", fill=(255, 255, 255, 255))
        out_img.convert("RGB").save(os.path.join(img_dir, "collision_%s.png" % o["id"]), optimize=True)


if __name__ == "__main__":
    main()
