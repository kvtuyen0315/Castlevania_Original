# -*- coding: utf-8 -*-
"""
sheetlib.py - thư viện dùng chung cho các script kiểm kê sprite sheet Castlevania (NES), rip bởi Mister Mike.

KHÔNG chạy trực tiếp; được import bởi inv_sprites.py và inv_level1.py.
Chạy các script khác bằng:  python -P -E <script>.py   (KHÔNG dùng -I).
Không in tiếng Việt có dấu ra console (cp1252) - chỉ ghi file UTF-8.

Hàm chính
  load_rgb(path)                 -> ndarray HxWx3 uint8
  color_hist(arr)                -> Counter {(r,g,b): số_pixel}
  label_components(mask, conn)   -> (labels int32 HxW [0 = nền], n)  thành phần liên thông (4 hoặc 8), không cần scipy
  comp_stats(labels, n)          -> list dict {id, x, y, w, h, area}
  glyph_boxes / merge_boxes / words_from_text_mask -> gộp ký tự chữ trắng thành từ
  cluster_rows(items, tol)       -> gom ô thành hàng theo y0
  annotate(...)                  -> vẽ khung + số thứ tự lên ảnh phóng to
  dump_json_compact(path, obj)   -> ghi JSON UTF-8, mỗi phần tử danh sách trên 1 dòng (dễ đọc/diff)
"""
import json
import os
from collections import Counter

import numpy as np
from PIL import Image, ImageDraw, ImageFont

# --------------------------------------------------------------------------------------
# Hằng số màu (đã đo trên cả 5 sheet)
# --------------------------------------------------------------------------------------
BG = (0, 128, 0)          # nền sheet (xanh lá)
KEY = (116, 116, 116)     # ô xám = nền của từng sprite (màu key)
TEXT = (255, 255, 255)    # chữ nhãn (trắng tinh) ở sheet 1-4
BLACK = (0, 0, 0)
CREDIT_DARK = (0, 64, 0)  # bóng chữ logo credit "Sprites ripped by Mister Mike" (516 px mỗi sheet)

SRC_DIR = r"C:\Users\kvtuy\Downloads"
SHEET_FILES = {
    "simon_belmont": "NES - Castlevania - Playable Characters - Simon Belmont.png",
    "enemies": "NES - Castlevania - Enemies & Bosses - Enemies.png",
    "bosses": "NES - Castlevania - Enemies & Bosses - Bosses.png",
    "items_objects": "NES - Castlevania - Miscellaneous - Items & Objects.png",
    "level1_entrance": "NES - Castlevania - Stages - Level 1_ Entrance.png",
}

# Thư mục đầu ra = thư mục cha của tools/ (inventory/)
TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.dirname(TOOLS_DIR)


def sheet_path(key):
    return os.path.join(SRC_DIR, SHEET_FILES[key])


def load_rgb(path):
    return np.array(Image.open(path).convert("RGB"))


def color_mask(arr, rgb):
    return np.all(arr == np.array(rgb, dtype=arr.dtype), axis=2)


def hex_color(rgb):
    return "#%02x%02x%02x" % tuple(int(c) for c in rgb)


def color_hist(arr):
    flat = arr.reshape(-1, 3)
    packed = (flat[:, 0].astype(np.uint32) << 16) | (flat[:, 1].astype(np.uint32) << 8) | flat[:, 2].astype(np.uint32)
    vals, counts = np.unique(packed, return_counts=True)
    out = Counter()
    for v, c in zip(vals.tolist(), counts.tolist()):
        out[((v >> 16) & 255, (v >> 8) & 255, v & 255)] = c
    return out


# --------------------------------------------------------------------------------------
# Thành phần liên thông (CCL) theo run-length + union-find
# --------------------------------------------------------------------------------------
def label_components(mask, conn=8):
    """mask: bool HxW; conn: 4 hoặc 8. Trả về (labels, n); labels 0 = nền, 1..n = thành phần.
    Thứ tự đánh số theo run đầu tiên (trên->dưới, trái->phải) nên ổn định, chạy lại ra y hệt."""
    H, W = mask.shape
    parent = []

    def find(a):
        while parent[a] != a:
            parent[a] = parent[parent[a]]
            a = parent[a]
        return a

    def union(a, b):
        ra, rb = find(a), find(b)
        if ra != rb:
            if ra < rb:
                parent[rb] = ra
            else:
                parent[ra] = rb

    runs_per_row = []
    run_xy = []  # (y, x0, x1)
    for y in range(H):
        row = mask[y]
        if not row.any():
            runs_per_row.append([])
            continue
        d = np.diff(np.concatenate(([0], row.view(np.uint8), [0])))
        starts = np.nonzero(d == 1)[0]
        ends = np.nonzero(d == -1)[0]
        lst = []
        for s, e in zip(starts.tolist(), ends.tolist()):
            rid = len(parent)
            parent.append(rid)
            run_xy.append((y, s, e))
            lst.append((s, e, rid))
        runs_per_row.append(lst)
    pad = 1 if conn == 8 else 0
    for y in range(1, H):
        prev = runs_per_row[y - 1]
        cur = runs_per_row[y]
        if not prev or not cur:
            continue
        j = 0
        for (s, e, rid) in cur:
            lo, hi = s - pad, e + pad
            while j < len(prev) and prev[j][1] <= lo:
                j += 1
            k = j
            while k < len(prev) and prev[k][0] < hi:
                union(rid, prev[k][2])
                k += 1
    root_to_label = {}
    labels = np.zeros((H, W), dtype=np.int32)
    n = 0
    for rid, (y, s, e) in enumerate(run_xy):
        r = find(rid)
        lab = root_to_label.get(r)
        if lab is None:
            n += 1
            lab = n
            root_to_label[r] = lab
        labels[y, s:e] = lab
    return labels, n


def comp_stats(labels, n):
    """bbox + diện tích cho từng thành phần 1..n"""
    out = []
    ys, xs = np.nonzero(labels)
    lab = labels[ys, xs]
    order = np.argsort(lab, kind="stable")
    lab_s, ys_s, xs_s = lab[order], ys[order], xs[order]
    bounds = np.searchsorted(lab_s, np.arange(1, n + 2))
    for i in range(1, n + 1):
        a, b = bounds[i - 1], bounds[i]
        if a == b:
            continue
        yy, xx = ys_s[a:b], xs_s[a:b]
        x0, y0 = int(xx.min()), int(yy.min())
        x1, y1 = int(xx.max()) + 1, int(yy.max()) + 1
        out.append({"id": i, "x": x0, "y": y0, "w": x1 - x0, "h": y1 - y0, "area": int(b - a)})
    return out


# --------------------------------------------------------------------------------------
# Chữ nhãn: ký tự -> từ
# --------------------------------------------------------------------------------------
def glyph_boxes(text_mask):
    """Glyph = thành phần liên thông 8 của chữ trắng. Trả list (x0,y0,x1,y1), x1/y1 exclusive."""
    labels, n = label_components(text_mask, conn=8)
    st = comp_stats(labels, n)
    return [(s["x"], s["y"], s["x"] + s["w"], s["y"] + s["h"]) for s in st]


def merge_boxes(boxes, gap_x):
    """Gộp các bbox cùng dòng (chồng lấn theo y) nếu khoảng cách ngang <= gap_x.
    Trả list bbox đã gộp, sắp theo (y, x)."""
    cur = [list(b) for b in sorted(boxes, key=lambda b: (b[1], b[0]))]
    changed = True
    while changed:
        changed = False
        out = []
        used = [False] * len(cur)
        for i in range(len(cur)):
            if used[i]:
                continue
            a = cur[i]
            for j in range(i + 1, len(cur)):
                if used[j]:
                    continue
                b = cur[j]
                if min(a[3], b[3]) - max(a[1], b[1]) <= 0:
                    continue
                gap = max(b[0] - a[2], a[0] - b[2])
                if gap <= gap_x:
                    a = [min(a[0], b[0]), min(a[1], b[1]), max(a[2], b[2]), max(a[3], b[3])]
                    used[j] = True
                    changed = True
            used[i] = True
            out.append(a)
        cur = out
    cur.sort(key=lambda b: (b[1], b[0]))
    return [tuple(b) for b in cur]


def words_from_text_mask(text_mask, gap_x=3):
    """Gộp ký tự thành TỪ (khoảng cách ngang <= gap_x px; khoảng trắng giữa từ là >= 5 px)."""
    return merge_boxes(glyph_boxes(text_mask), gap_x)


def bbox_xywh(b):
    return [int(b[0]), int(b[1]), int(b[2] - b[0]), int(b[3] - b[1])]


def union_boxes(boxes):
    return (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))


def cluster_rows(items, tol=3):
    """Gom phần tử (có khoá 'y') thành hàng theo y0: bắt đầu hàng mới khi y - y_đầu_hàng > tol.
    Trả list các list (đã sắp theo x trong từng hàng)."""
    its = sorted(items, key=lambda c: (c["y"], c["x"]))
    rows = []
    for c in its:
        if rows and c["y"] - rows[-1][0]["y"] <= tol:
            rows[-1].append(c)
        else:
            rows.append([c])
    for r in rows:
        r.sort(key=lambda c: (c["x"], c["y"]))
    return rows


# --------------------------------------------------------------------------------------
# Vẽ chú thích
# --------------------------------------------------------------------------------------
def get_font(size):
    try:
        return ImageFont.load_default(size=size)
    except TypeError:  # Pillow cũ
        return ImageFont.load_default()


def hsv_color(i, n):
    import colorsys
    h = (i * 0.61803398875) % 1.0
    r, g, b = colorsys.hsv_to_rgb(h, 0.85, 1.0)
    return (int(r * 255), int(g * 255), int(b * 255))


def draw_text_outlined(draw, xy, text, fill, font, outline=(0, 0, 0)):
    x, y = xy
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            if dx or dy:
                draw.text((x + dx, y + dy), text, font=font, fill=outline)
    draw.text((x, y), text, font=font, fill=fill)


def dump_json_compact(path, obj, one_line_keys=()):
    """Ghi JSON UTF-8. Các khoá trong one_line_keys (giá trị là list) được ghi mỗi phần tử 1 dòng."""
    lines = ["{"]
    keys = list(obj.keys())
    for ki, k in enumerate(keys):
        v = obj[k]
        comma = "," if ki < len(keys) - 1 else ""
        if isinstance(v, list) and k in one_line_keys:
            lines.append("  %s: [" % json.dumps(k, ensure_ascii=False))
            for i, it in enumerate(v):
                lines.append("    " + json.dumps(it, ensure_ascii=False) + ("," if i < len(v) - 1 else ""))
            lines.append("  ]" + comma)
        else:
            lines.append("  %s: %s%s" % (json.dumps(k, ensure_ascii=False), json.dumps(v, ensure_ascii=False), comma))
    lines.append("}")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")


def ensure_dir(p):
    os.makedirs(p, exist_ok=True)
    return p
