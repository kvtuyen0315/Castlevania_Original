"""Tách lớp roi Simon: mỗi khung roi ghép sẵn trên sheet = thân (ô "Whip/Use Subweapon") + mảnh roi rời.

Tìm cho từng khung `simon_whip<cấp>_<tư thế>_<i>` (i = 0..3; i = 4 là mảnh thân lẻ, bỏ):
  1) thân: thử 6 ô `simon_use_<tư thế>_*` ở mọi vị trí trong ô, lấy chỗ mọi pixel đục của thân trùng khung
     (bỏ qua pixel bị roi đè) — ít lệch nhất;
  2) mảnh roi: thử mảnh cùng cấp/màu (L2 treo/vắt dùng chung của L3 màu 2) ở mọi vị trí (cho phép thò ra ngoài ô),
     2 thứ tự vẽ (thân trên / roi trên);
  3) ghép lại = khung sheet: 0 pixel lệch (trong suốt = xám 116 hoặc xanh nền), ngược lại báo lỗi.
Ra: JSON (stdout hoặc --out) gồm offset roi so với PIVOT CHÂN của thân (toạ độ sheet, Simon quay TRÁI, y xuống):
  whipOffset = góc trên-trái mảnh roi − pivot thân; pivot thân = (x giữa cột chân 16 px, đáy slot 32 px).
Chạy từ gốc repo: python -P -E Tools/cvgp/whip_layers.py [--out <json>]
"""
import json
import sys

import numpy as np
from PIL import Image

SHEET = "Art_Source/spriters-resource/simon_belmont.png"
INV = "ClaudeNotes/cvgp/inventory/simon_belmont.json"
KEYS = (116 * 65536 + 116 * 256 + 116, 128 * 256)
LEVELS = {"whip1": "l1", "whip2": "l2", "whip3c1": "l3c1", "whip3c2": "l3c2", "whip3c3": "l3c3", "whip3c4": "l3c4"}
POSTURES = ["stand", "duck", "stairs_up", "stairs_down"]
PART_KIND = {0: "hang", 1: "over", 2: "ext", 3: "ext"}


def load():
    s = np.array(Image.open(SHEET).convert("RGB")).astype(np.int64)
    code = s[:, :, 0] * 65536 + s[:, :, 1] * 256 + s[:, :, 2]
    cells = {c["id"]: c for c in json.load(open(INV, encoding="utf-8"))["cells"]}
    return code, cells


def cell(code, cells, cid):
    x, y, w, h = cells[cid]["rect"]
    a = code[y:y + h, x:x + w].copy()
    for k in KEYS:
        a[a == k] = -1
    return a


def place(H, W, P, px, py):
    """Đặt P vào khung H×W tại (px,py); trả (canvas, số pixel đục bị cắt ở mép)."""
    can = np.full((H, W), -1, dtype=np.int64)
    ph, pw = P.shape
    y0, y1, x0, x1 = max(0, py), min(H, py + ph), max(0, px), min(W, px + pw)
    if y0 >= y1 or x0 >= x1:
        return None, 0
    sub = P[y0 - py:y1 - py, x0 - px:x1 - px]
    can[y0:y1, x0:x1] = sub
    return can, int((P != -1).sum() - (sub != -1).sum())


def over(top, bot):
    o = bot.copy()
    o[top != -1] = top[top != -1]
    return o


def part_name(level, kind):
    tag = LEVELS[level]
    if tag == "l2" and kind != "ext":
        tag = "l3c2"  # L2 không có mảnh treo/vắt riêng (planner + kiểm dưới đây)
    return f"simon_whippart_{tag}_{kind}"


def solve(code, cells, fid, level, post, i):
    F = cell(code, cells, fid)
    H, W = F.shape
    part = part_name(level, PART_KIND[i])
    P = cell(code, cells, part)
    best = None
    for b in [f"simon_use_{post}_{k}" for k in range(6)]:
        B = cell(code, cells, b)
        for by in range(-2, H - B.shape[0] + 3):
            for bx in range(0, W - B.shape[1] + 1):
                cb, clip = place(H, W, B, bx, by)
                if cb is None or clip:
                    continue
                m = cb != -1
                bad = int((m & (cb != F)).sum())
                if best is None or bad < best[0]:
                    best = (bad, [(b, bx, by, cb)])
                elif bad == best[0]:
                    best[1].append((b, bx, by, cb))
    sols = []
    for b, bx, by, cb in best[1]:
        for py in range(-P.shape[0] + 1, H):
            for px in range(-P.shape[1] + 1, W):
                cp, clip = place(H, W, P, px, py)
                if cp is None:
                    continue
                for on_top in (False, True):
                    comp = over(cp, cb) if on_top else over(cb, cp)
                    if np.array_equal(comp, F):
                        sols.append(dict(frame=fid, body=b, bodyAt=[bx, by], part=part, partAt=[px, py],
                                         whipOnTop=on_top, clippedPx=clip))
    # cùng kết quả ghép với cả 2 thứ tự (không chồng pixel) ⇒ giữ thứ tự thân trên (mặc định)
    uniq = {}
    for s in sols:
        k = (s["body"], tuple(s["bodyAt"]), tuple(s["partAt"]))
        if k not in uniq or not s["whipOnTop"]:
            uniq[k] = s
    return list(uniq.values())


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    code, cells = load()
    out, errs = [], []
    for level in LEVELS:
        for post in POSTURES:
            for i in range(4):
                fid = f"simon_{level}_{post}_{i}"
                sols = solve(code, cells, fid, level, post, i)
                if len(sols) != 1:
                    errs.append(f"{fid}: {len(sols)} nghiệm")
                    continue
                s = sols[0]
                bw = cells[s["body"]]["rect"][2]
                # pivot thân: cột chân 16 px (inventory pivot_goi_y) + đáy slot 32
                piv = cells[s["body"]]["pivot_goi_y"]
                s["bodyPivot"] = piv
                s["whipOffset"] = [s["partAt"][0] - (s["bodyAt"][0] + piv[0]), s["partAt"][1] - (s["bodyAt"][1] + piv[1])]
                s["bodyWidth"] = bw
                out.append(s)
                print(f"{fid:34s} thân {s['body']:28s}@{s['bodyAt']}  roi {s['part']:26s}@{s['partAt']}  "
                      f"offset {s['whipOffset']}  roiTrên={s['whipOnTop']}  cụt={s['clippedPx']}")
    if errs:
        print("LỖI:\n  " + "\n  ".join(errs))
        sys.exit(1)
    print(f"{len(out)} khung roi ghép = thân + roi: 0 pixel lệch.")
    if "--out" in sys.argv:
        json.dump(out, open(sys.argv[sys.argv.index("--out") + 1], "w", encoding="utf-8"), indent=1, ensure_ascii=False)


if __name__ == "__main__":
    main()
