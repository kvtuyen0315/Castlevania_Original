"""Kiểm độc lập sprite gameplay CVGP (routine 05) — không dùng lại code cắt.

  python -P -E Tools/cvgp/verify_sprites.py [--meta]

1. Pixel: mọi PNG trong manifest so lại sheet: pixel sheet = key (keyColor, hoặc nền sheet nếu bgIsKey) ⇔ PNG trong
   suốt (0,0,0,0); còn lại PNG đục đúng màu sheet; 0 pixel đục màu nền sheet (0,128,0); đúng kích thước.
2. Ô tách/lặp: 2 nửa Dragon Skull ghép lại = ô 16x32 gốc; brick_0 = cả 4 khối của obj_brick_set.
3. Roi: với mọi anim có overlays, ghép PNG thân (đặt theo pivot manifest) + PNG roi (pivot + overlayOffset, thứ tự
   theo overlayOnTop) rồi so với ô ghép sẵn `simon_whip*` của sheet, căn theo pivot_goi_y CỦA KIỂM KÊ (nguồn độc lập).
   Khung _3 (roi đè thân) kiểm bằng cùng dữ liệu khung 2 nhưng đảo thứ tự. Phần mảnh roi thò ra ngoài ô ghép sẵn được
   đếm riêng (sheet cụt), không tính lệch.
4. Đếm: mọi ô kiểm kê (371) có mặt đúng 1 lần ở `src` hoặc `skipped`.
5. --meta: đọc .meta của mọi PNG: PPU 16, filter Point (0), không nén, không mip, Single, pivot = manifest.
"""
import json
import os
import re
import sys

import numpy as np
from PIL import Image

MAN = "Tools/cvgp/slices_gameplay.json"
INV = "ClaudeNotes/cvgp/inventory/{}.json"
ART = "Assets/Data/Art/"
INV_SHEETS = {"simon_belmont": "simon_belmont", "enemies": "enemies", "bosses": "bosses", "items_objects": "items_objects"}


def rgba(path):
    return np.array(Image.open(path).convert("RGBA")).astype(np.int64)


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    doc = json.load(open(MAN, encoding="utf-8"))
    bg = tuple(doc["sheetBg"])
    sheets, errs = {}, []
    inv = {}
    for s in INV_SHEETS:
        for c in json.load(open(INV.format(s), encoding="utf-8"))["cells"]:
            inv[c["id"]] = c
    img = {}

    # 1. pixel
    for e in doc["slices"]:
        sp = e["sheet"]
        if sp not in sheets:
            sheets[sp] = np.array(Image.open(sp).convert("RGB")).astype(np.int64)
        S = sheets[sp]
        x, y, w, h = e["rect"]
        src = S[y:y + h, x:x + w]
        out = f"{e['outDir']}/{e['name']}.png"
        P = rgba(out)
        img[out[len(ART):-4]] = P
        if P.shape[:2] != (h, w):
            errs.append(f"{out}: kích thước {P.shape[1]}x{P.shape[0]} ≠ {w}x{h}")
            continue
        key = np.zeros((h, w), bool)
        if e.get("keyColor"):
            key |= np.all(src == e["keyColor"], axis=2)
        if e.get("bgIsKey"):
            key |= np.all(src == bg, axis=2)
        bad_t = int((key & ~np.all(P == 0, axis=2)).sum())
        bad_o = int((~key & ((P[:, :, 3] != 255) | np.any(P[:, :, :3] != src, axis=2))).sum())
        green = int(((P[:, :, 3] == 255) & np.all(P[:, :, :3] == bg, axis=2)).sum())
        if bad_t or bad_o or green:
            errs.append(f"{out}: lệch trong suốt {bad_t}, lệch màu đục {bad_o}, xanh nền đục {green}")
    n_px = len(doc["slices"])

    # 2. ô tách / lặp
    for col, cid in (("purple", "enemy_dragonskull_0"), ("red", "enemy_dragonskull_1")):
        x, y, w, h = inv[cid]["rect"]
        S = sheets["Art_Source/spriters-resource/enemies.png"][y:y + h, x:x + w]
        both = np.concatenate([img[f"Enemy/DragonSkullCannon/dragonskull_{col}_top"],
                               img[f"Enemy/DragonSkullCannon/dragonskull_{col}_bottom"]], axis=0)
        k = np.all(S == 116, axis=2)
        if not (np.all(both[~k][:, :3] == S[~k]) and np.all(both[k] == 0)):
            errs.append(f"{cid}: 2 nửa ghép lại ≠ ô gốc")
    x, y, w, h = inv["obj_brick_set"]["rect"]
    S = sheets["Art_Source/spriters-resource/items_objects.png"][y:y + h, x:x + w]
    B = img["Object/brick_0"]
    for i in range(4):
        part = S[:, 8 * i:8 * i + 8]
        k = np.all(part == 116, axis=2)
        if not (np.all(B[~k][:, :3] == part[~k]) and np.all(B[k] == 0)):
            errs.append(f"obj_brick_set khối {i} ≠ brick_0")

    # 3. roi
    pv = {e["outDir"][len(ART):] + "/" + e["name"]: e["pivot"] for e in doc["slices"]}
    pv.update({"Character/Simon/" + n: [8, 32] for n in ("idle_0", "walk_0", "walk_1", "walk_2", "back_0")})
    LV = {"l1": "whip1", "l2": "whip2", "l3c1": "whip3c1", "l3c2": "whip3c2", "l3c3": "whip3c3", "l3c4": "whip3c4"}
    S = sheets["Art_Source/spriters-resource/simon_belmont.png"]
    n_whip, clipped_total = 0, 0
    for a in doc["anims"]:
        if not a.get("overlays") or not a["name"].startswith("whip_"):
            continue  # CVGP 20: lớp phủ không phải roi (Dragon Skull 17) — chỉ kiểm roi Simon
        _, tag, post = a["name"].split("_", 2)
        for i in range(4):
            j = min(i, 2)
            on_top = a["overlayOnTop"][j] if i < 3 else not a["overlayOnTop"][2]
            cid = f"simon_{LV[tag]}_{post}_{i}"
            cx, cy, cw, ch = inv[cid]["rect"]
            ref = S[cy:cy + ch, cx:cx + cw]
            refk = np.all(ref == 116, axis=2) | np.all(ref == bg, axis=2)
            cp = inv[cid]["pivot_goi_y"]  # pivot thân theo kiểm kê (độc lập)
            body, ov = img[a["frames"][j]], img[a["overlays"][j]]
            bp = pv[a["frames"][j]]
            ox, oy = a["overlayOffsets"][2 * j], a["overlayOffsets"][2 * j + 1]
            # canvas lớn, gốc = pivot thân tại (64,64)
            C = np.zeros((160, 200, 4), np.int64)
            O = 64

            def put(dst, P, px, py):
                m = P[:, :, 3] == 255
                sub = dst[py:py + P.shape[0], px:px + P.shape[1]]
                sub[m] = P[m]

            layers = [(body, O - bp[0], O - bp[1]), (ov, O + ox, O + oy)]
            for P, px, py in (layers if on_top else layers[::-1]):
                put(C, P, px, py)
            win = C[O - cp[1]:O - cp[1] + ch, O - cp[0]:O - cp[0] + cw]
            bad = int((~refk & ((win[:, :, 3] != 255) | np.any(win[:, :, :3] != ref, axis=2))).sum()
                      + (refk & (win[:, :, 3] == 255)).sum())
            outside = int((C[:, :, 3] == 255).sum() - (win[:, :, 3] == 255).sum())
            clipped_total += outside
            n_whip += 1
            if bad:
                errs.append(f"{cid}: ghép thân+roi lệch {bad} pixel")
            if outside:
                print(f"  ghi chú: {cid} — {outside} pixel mảnh roi nằm ngoài ô ghép sẵn (sheet cụt)")

    # 4. đếm
    seen = {}
    for e in doc["slices"]:
        seen.setdefault(e["src"], 0)
        seen[e["src"]] += 1
    for s in doc["skipped"]:
        if s["src"] in seen:
            errs.append(f"{s['src']}: vừa cắt vừa bỏ")
        seen[s["src"]] = seen.get(s["src"], 0) + 1
    missing = [k for k in inv if k not in seen]
    extra = [k for k in seen if k not in inv and not k.startswith("cutscene_")]
    if missing or extra:
        errs.append(f"đếm: thiếu {missing}, lạ {extra}")
    cut_cells = len({e['src'] for e in doc['slices'] if e['src'] in inv})

    # 5. meta
    n_meta = 0
    if "--meta" in sys.argv:
        for e in doc["slices"]:
            mp = f"{e['outDir']}/{e['name']}.png.meta"
            if not os.path.exists(mp):
                errs.append(f"{mp}: chưa có (chưa import?)")
                continue
            t = open(mp, encoding="utf-8").read()
            w, h = e["rect"][2], e["rect"][3]
            want = (e["pivot"][0] / w, 1 - e["pivot"][1] / h)

            def g(k):
                m = re.search(r"\n\s*" + k + r": (.*)", t)
                return m.group(1).strip() if m else None
            pm = re.search(r"spritePivot: \{x: ([-\d.e]+), y: ([-\d.e]+)\}", t)
            got = (float(pm.group(1)), float(pm.group(2))) if pm else None
            checks = {"spritePixelsToUnits": "16", "filterMode": "0", "textureCompression": "0", "enableMipMap": "0",
                      "spriteMode": "1", "alignment": "9", "spriteMeshType": "0", "textureType": "8"}
            bad = [f"{k}={g(k)}" for k, v in checks.items() if g(k) != v]
            if got is None or abs(got[0] - want[0]) > 1e-6 or abs(got[1] - want[1]) > 1e-6:
                bad.append(f"pivot {got} ≠ {want}")
            if bad:
                errs.append(f"{mp}: " + ", ".join(bad))
            n_meta += 1

    print(f"pixel: {n_px} PNG; roi: {n_whip} khung ghép; pixel roi ngoài ô sheet: {clipped_total}; "
          f"đếm: {cut_cells} ô cắt + {len(doc['skipped'])} ô bỏ = {cut_cells + len(doc['skipped'])}/{len(inv)} ô kiểm kê"
          + (f"; meta: {n_meta}" if n_meta else ""))
    if errs:
        print("LỖI (" + str(len(errs)) + "):\n  " + "\n  ".join(errs[:60]))
        sys.exit(1)
    print("0 lỗi.")


if __name__ == "__main__":
    main()
