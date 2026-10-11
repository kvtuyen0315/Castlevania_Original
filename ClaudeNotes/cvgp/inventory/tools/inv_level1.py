# -*- coding: utf-8 -*-
"""
inv_level1.py - KIỂM KÊ BẢN ĐỒ LEVEL 1 (sheet "Stages - Level 1_ Entrance", 3601 x 411).

Cách chạy (Git Bash / PowerShell), KHÔNG dùng -I:
    python -P -E inv_level1.py

Đầu vào : C:\\Users\\kvtuy\\Downloads\\NES - Castlevania - Stages - Level 1_ Entrance.png (chỉ đọc)
Đầu ra  : ..\\level1_entrance.json
          ..\\level1_entrance_annot.png            (toàn bản đồ 1x, tô loại khối)
          ..\\level1_entrance_annot_1..4.png        (4 ảnh phóng 2x có lưới 16 px + toạ độ + ký tự loại khối)

Các bước
  1. Vùng bản đồ = thành phần liên thông (8) của pixel khác nền (0,128,0) mà là CHỮ NHẬT ĐẶC (diện tích = w*h) và
     đủ lớn. Nhãn chữ trắng (252,252,252) nằm NGOÀI vùng được gộp (khoảng cách <= 5 px) và gán cho vùng ngay bên dưới.
  2. Lưới: với mỗi vùng thử mọi offset (0..15, 0..15) cho khối 16x16 và (0..7, 0..7) cho tile 8x8, đếm số khối
     duy nhất; offset đúng là offset cho số khối duy nhất ÍT NHẤT (nhiều trùng lặp nhất).
  3. Thống kê tile: số tile 8x8 / metatile 16x16 duy nhất cho từng vùng và toàn Level 1 (khớp từng pixel; kèm số
     sau khi gộp lật ngang/dọc).
  4. Phân loại khối (kiểm kê cho planner, không phải va chạm tuyệt đối):
       B = khối gạch cam 16x16 (đứng được)     F = sàn sân (Castle Entrance, hàng 10)
       U = ô có bậc thang đi lên sang PHẢI      L = ô có bậc thang đi lên sang TRÁI
       W = nước (Crypt B, chết)                 K = tường gạch cam dạng lưới (có thể là tường phá được)
       G = song/lưới sắt (trang trí)            D = cột cửa đỏ-trắng
       R = lan can/rào (trang trí)              . = nền / trang trí khác
     Bậc thang nhận ra bằng vệt cam 8x2 (diện tích 15 px) lặp mỗi 8 px theo đường chéo.
"""
import os
import sys
from collections import Counter, defaultdict

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sheetlib as S  # noqa: E402

KEY = "level1_entrance"
ORANGE = (200, 76, 12)
BLUE = (36, 24, 140)
WHITE = (252, 252, 252)
BLACK = (0, 0, 0)

# Tên/nhãn của 5 vùng theo vị trí nhãn chữ (đã nhìn ảnh): text -> id
LABEL_TEXT = {  # (x,y) góc trên-trái của nhãn đo được -> (text, id)
    (1, 1): ("Castle Entrance", "castle_entrance"),
    (774, 1): ("Entrance Hall", "entrance_hall"),
    (2315, 1): ("Crypt A", "crypt_a"),
    (2832, 1): ("Phantom Bat", "phantom_bat"),
    (2315, 208): ("Crypt B", "crypt_b"),
}
# Chú thích tay cho các khoảng trống trên hàng sàn chính (vùng, cột đầu) - đã nhìn ảnh
PIT_NOTES = {
    ("crypt_b", 14): "vực: khe trống 32 px giữa 2 đoạn sàn hàng 6, bên dưới là nước (rơi xuống là chết)",
    ("crypt_b", 18): "vực: khe trống 32 px (có bục nổi 2 khối ở cột 16-17 bên trái), bên dưới là nước (rơi xuống là chết)",
    ("crypt_b", 30): "KHÔNG phải vực tự do: cột 30-31 là trụ đá + dải khung cửa (mép phải vùng), không có sàn ở hàng 6",
}
# Hàng sàn chính của từng vùng (đã nhìn ảnh)
GROUND_ROW = {"castle_entrance": 10, "entrance_hall": 11, "crypt_a": 11, "crypt_b": 6, "phantom_bat": 11}
# Khối mẫu để nhận ra lan can/rào/song sắt bằng so khớp hash (vùng, cột, hàng) - chọn bằng mắt
RAIL_PROBES = [("entrance_hall", 52, 4), ("phantom_bat", 3, 4), ("phantom_bat", 10, 6), ("castle_entrance", 4, 9)]


def find_regions(a, bgm):
    lab, n = S.label_components(~bgm, 8)
    st = S.comp_stats(lab, n)
    regs = [s for s in st if s["area"] == s["w"] * s["h"] and s["w"] >= 64 and s["h"] >= 64]
    regs.sort(key=lambda s: (s["x"], s["y"]))
    return regs, st


def block_hash(reg, c, r, size=16):
    return reg[r * size:(r + 1) * size, c * size:(c + 1) * size].tobytes()


def canon_flips(blk):
    """Dạng chuẩn tắc dưới lật ngang/dọc (để đếm tile 'duy nhất sau khi gộp lật')."""
    cands = [blk, blk[:, ::-1], blk[::-1, :], blk[::-1, ::-1]]
    return min(c.tobytes() for c in cands)


def uniq_count(reg, size, ox, oy):
    H, W = reg.shape[:2]
    nx, ny = (W - ox) // size, (H - oy) // size
    seen = set()
    for j in range(ny):
        for i in range(nx):
            seen.add(reg[oy + j * size:oy + (j + 1) * size, ox + i * size:ox + (i + 1) * size].tobytes())
    return len(seen), nx * ny


def runs_of(bits):
    out, start = [], None
    for i, v in enumerate(list(bits) + [False]):
        if v and start is None:
            start = i
        if (not v) and start is not None:
            out.append((start, i - 1))
            start = None
    return out


def analyze():
    a = S.load_rgb(S.sheet_path(KEY))
    H, W = a.shape[:2]
    bgm = S.color_mask(a, S.BG)
    hist = S.color_hist(a)

    # ---------------------------------------------------------------- vùng + logo + nhãn
    regs_all, st_all = find_regions(a, bgm)
    # logo credit: khung 224x32 chứa pixel (0,64,0)
    cd = S.color_mask(a, S.CREDIT_DARK)
    ys, xs = np.nonzero(cd)
    cdb = (int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1)
    logo = max([s for s in st_all if s["x"] <= cdb[0] and s["y"] <= cdb[1] and s["x"] + s["w"] >= cdb[2] and s["y"] + s["h"] >= cdb[3]],
               key=lambda s: s["w"] * s["h"])
    regs = [s for s in regs_all if not (s["x"] == logo["x"] and s["y"] == logo["y"])]
    assert len(regs) == 5, "can 5 vung, thay %d" % len(regs)
    inreg = np.zeros((H, W), bool)
    for s in regs:
        inreg[s["y"]:s["y"] + s["h"], s["x"]:s["x"] + s["w"]] = True
    txtm = S.color_mask(a, WHITE) & ~inreg
    gb = S.glyph_boxes(txtm)
    lab_boxes = S.merge_boxes(gb, 5)
    labels = []
    for b in lab_boxes:
        key = (b[0], b[1])
        if key not in LABEL_TEXT:
            raise SystemExit("LOI: nhan tai %r chua co trong LABEL_TEXT" % (key,))
        labels.append(dict(text=LABEL_TEXT[key][0], id=LABEL_TEXT[key][1], bbox=S.bbox_xywh(b)))
    assert len(labels) == len(LABEL_TEXT)
    # gán vùng: nhãn ngay phía trên (đáy nhãn <= đỉnh vùng) và x0 nhãn nằm trong dải x của vùng; chọn gần nhất
    regions = []
    for s in regs:
        cands = [L for L in labels if L["bbox"][1] + L["bbox"][3] <= s["y"] and s["x"] <= L["bbox"][0] < s["x"] + s["w"]]
        L = max(cands, key=lambda L: L["bbox"][1])
        regions.append(dict(id=L["id"], label=L["text"], x=s["x"], y=s["y"], w=s["w"], h=s["h"], label_bbox=L["bbox"]))
    order = ["castle_entrance", "entrance_hall", "crypt_a", "crypt_b", "phantom_bat"]
    regions.sort(key=lambda r: order.index(r["id"]))
    R = {r["id"]: r for r in regions}
    img = {r["id"]: a[r["y"]:r["y"] + r["h"], r["x"]:r["x"] + r["w"]] for r in regions}

    # ---------------------------------------------------------------- lưới
    grid_info = {}
    for r in regions:
        reg = img[r["id"]]
        res16 = {(ox, oy): uniq_count(reg, 16, ox, oy) for oy in range(16) for ox in range(16)}
        best16 = min(res16, key=lambda k: (res16[k][0] / res16[k][1], k))
        res8 = {(ox, oy): uniq_count(reg, 8, ox, oy) for oy in range(8) for ox in range(8)}
        best8 = min(res8, key=lambda k: (res8[k][0] / res8[k][1], k))
        others16 = sorted(v[0] for k, v in res16.items() if k != (0, 0))
        others8 = sorted(v[0] for k, v in res8.items() if k != (0, 0))
        grid_info[r["id"]] = dict(
            offset16=list(best16), unique16_at_best=res16[best16][0], total16=res16[best16][1],
            unique16_at_0=res16[(0, 0)][0], min_unique16_other_offsets=others16[0],
            offset8=list(best8), unique8_at_best=res8[best8][0], total8=res8[best8][1],
            unique8_at_0=res8[(0, 0)][0], min_unique8_other_offsets=others8[0],
        )
        assert best16 == (0, 0) and best8 == (0, 0), "luoi khong can o offset 0 o vung %s" % r["id"]

    # thống kê tile duy nhất (offset 0)
    def tiles(reg, size):
        H_, W_ = reg.shape[:2]
        out = []
        for j in range(H_ // size):
            for i in range(W_ // size):
                out.append(reg[j * size:(j + 1) * size, i * size:(i + 1) * size])
        return out
    tstats = {}
    glob8, glob16, glob8f, glob16f = set(), set(), set(), set()
    seen8_before, seen16_before = set(), set()
    for r in regions:
        reg = img[r["id"]]
        t8 = tiles(reg, 8)
        t16 = tiles(reg, 16)
        h8 = [t.tobytes() for t in t8]
        h16 = [t.tobytes() for t in t16]
        u8, u16 = set(h8), set(h16)
        u8f = set(canon_flips(t) for t in t8)
        u16f = set(canon_flips(t) for t in t16)
        # ô đồng nhất 1 màu
        uni16 = sum(1 for t in t16 if (t == t[0, 0]).all())
        tstats[r["id"]] = dict(
            tiles8_total=len(h8), unique8=len(u8), unique8_flip_merged=len(u8f),
            tiles16_total=len(h16), unique16=len(u16), unique16_flip_merged=len(u16f),
            blocks16_uniform_color=uni16,
            new8_vs_previous_regions=len(u8 - seen8_before), new16_vs_previous_regions=len(u16 - seen16_before),
        )
        seen8_before |= u8
        seen16_before |= u16
        glob8 |= u8
        glob16 |= u16
        glob8f |= u8f
        glob16f |= u16f
    total_unique = dict(
        unique8=len(glob8), unique8_flip_merged=len(glob8f), unique16=len(glob16), unique16_flip_merged=len(glob16f),
        tiles8_total=sum(v["tiles8_total"] for v in tstats.values()), tiles16_total=sum(v["tiles16_total"] for v in tstats.values()),
    )

    # ---------------------------------------------------------------- bậc thang (vệt cam 8x2 area 15)
    stairs = []
    step_blocks = {}   # (region, col, row) -> 'U'/'L'
    for r in regions:
        reg = img[r["id"]]
        om = S.color_mask(reg, ORANGE)
        lab, n = S.label_components(om, 8)
        st = S.comp_stats(lab, n)
        bars = sorted([(s["x"], s["y"]) for s in st if s["w"] == 8 and s["h"] == 2 and s["area"] == 15])
        used = set()
        chains = []
        bset = set(bars)
        for (x, y) in bars:
            if (x, y) in used:
                continue
            # bắt đầu chuỗi tại bậc ngoài cùng bên trái (không có bậc tiền nhiệm)
            if (x - 8, y + 8) in bset or (x - 8, y - 8) in bset:
                continue
            for dy in (-8, 8):
                if (x + 8, y + dy) in bset:
                    ch = [(x, y)]
                    while (ch[-1][0] + 8, ch[-1][1] + dy) in bset:
                        ch.append((ch[-1][0] + 8, ch[-1][1] + dy))
                    chains.append((dy, ch))
                    used.update(ch)
                    break
            else:
                chains.append((0, [(x, y)]))  # bậc lẻ loi
                used.add((x, y))
        # bậc không thuộc chuỗi nào (an toàn)
        for b in bars:
            if b not in used:
                chains.append((0, [b]))
        for k, (dy, ch) in enumerate(sorted(chains, key=lambda c: c[1][0])):
            xs_ = [p[0] for p in ch]
            ys_ = [p[1] for p in ch]
            x0, x1 = min(xs_), max(xs_) + 8
            y0, y1 = min(ys_), max(ys_) + 8
            if dy < 0:
                direction = "up_right"        # ↗ đi lên sang phải: chân ở trái-dưới, đỉnh ở phải-trên
                foot, top = ch[0], ch[-1]
            elif dy > 0:
                direction = "up_left"         # ↖ đi lên sang trái: chân ở phải-dưới, đỉnh ở trái-trên
                foot, top = ch[-1], ch[0]
            else:
                direction, foot, top = "single", ch[0], ch[0]
            rr = R[r["id"]]
            st_rec = dict(
                id="%s_stair_%d" % (r["id"], k), region=r["id"], direction=direction,
                arrow="↗" if direction == "up_right" else ("↖" if direction == "up_left" else "?"),
                n_steps=len(ch), step_size=[8, 8],
                foot_tread_local=list(foot), top_tread_local=list(top),
                foot_tread_sheet=[foot[0] + rr["x"], foot[1] + rr["y"]], top_tread_sheet=[top[0] + rr["x"], top[1] + rr["y"]],
                rect_local=[x0, y0, x1 - x0, y1 - y0], rect_sheet=[x0 + rr["x"], y0 + rr["y"], x1 - x0, y1 - y0],
                rise_tread_to_tread=abs(foot[1] - top[1]),
                height_px=len(ch) * 8,   # từ mặt sàn/bục chân thang tới mặt bục đích (n bậc x 8 px)
                steps_local=[list(p) for p in ch],
            )
            flags = []
            if y0 <= 0:
                flags.append("đỉnh chạm mép TRÊN vùng (nối lên vùng phía trên)")
            if y1 >= rr["h"]:
                flags.append("chân chạm mép DƯỚI vùng (bậc thang bị cắt, nối xuống vùng phía dưới)")
            st_rec["ghi_chu"] = "; ".join(flags)
            stairs.append(st_rec)
            for (x, y) in ch:
                step_blocks[(r["id"], x // 16, y // 16)] = "U" if dy < 0 else "L"

    # ---------------------------------------------------------------- phân loại khối 16x16
    rail_hashes = set()
    for (rid, c, rw) in RAIL_PROBES:
        rail_hashes.add(block_hash(img[rid], c, rw))
    # cột cửa: pixel trắng nằm trong vùng
    doors = []
    for r in regions:
        reg = img[r["id"]]
        wm = S.color_mask(reg, WHITE)
        if wm.any():
            ys_, xs_ = np.nonzero(wm)
            doors.append(dict(region=r["id"], rect_local=[int(xs_.min()), int(ys_.min()), int(xs_.max() - xs_.min() + 1), int(ys_.max() - ys_.min() + 1)],
                              rect_sheet=[int(xs_.min()) + r["x"], int(ys_.min()) + r["y"], int(xs_.max() - xs_.min() + 1), int(ys_.max() - ys_.min() + 1)],
                              n_white_px=int(wm.sum())))
    maps = {}
    counts = defaultdict(Counter)
    for r in regions:
        reg = img[r["id"]]
        cols, rows = r["w"] // 16, r["h"] // 16
        om = S.color_mask(reg, ORANGE)
        bm = S.color_mask(reg, BLUE)
        kk = S.color_mask(reg, BLACK)
        # thành phần cam 6x5 (gạch nhỏ) cho tường K
        lab6, n6 = S.label_components(om, 8)
        st6 = S.comp_stats(lab6, n6)
        small6 = [(s["x"], s["y"]) for s in st6 if s["w"] == 6 and s["h"] == 5 and s["area"] == 28]
        cnt6 = Counter((x // 16, y // 16) for (x, y) in small6)
        # song sắt: thanh cam dọc 3x32 (diện tích 39) -> mọi khối giao với bbox của thanh
        grate_blocks = set()
        for s_ in st6:
            if s_["w"] == 3 and s_["h"] == 32 and s_["area"] == 39:
                for cc_ in range(s_["x"] // 16, (s_["x"] + s_["w"] - 1) // 16 + 1):
                    for rr2 in range(s_["y"] // 16, (s_["y"] + s_["h"] - 1) // 16 + 1):
                        grate_blocks.add((cc_, rr2))
        rowsmap = []
        for rr_ in range(rows):
            line = []
            for cc in range(cols):
                blk = reg[rr_ * 16:(rr_ + 1) * 16, cc * 16:(cc + 1) * 16]
                fo = float(om[rr_ * 16:(rr_ + 1) * 16, cc * 16:(cc + 1) * 16].mean())
                fk = float(kk[rr_ * 16:(rr_ + 1) * 16, cc * 16:(cc + 1) * 16].mean())
                fb = float(bm[rr_ * 16:(rr_ + 1) * 16, cc * 16:(cc + 1) * 16].mean())
                h_ = blk.tobytes()
                ch = "."
                if fo >= 0.45 and fk < 0.2:
                    ch = "B"
                elif (r["id"], cc, rr_) in step_blocks:
                    ch = step_blocks[(r["id"], cc, rr_)]
                elif r["id"] == "castle_entrance" and rr_ == GROUND_ROW["castle_entrance"]:
                    ch = "F"      # sàn sân (gạch lát kiểu khác, cam 2-45%); chiếm hàng 10 (hàng 11 là nền đen)
                elif cnt6[(cc, rr_)] >= 2 and 0 < cc < cols - 1:
                    ch = "K"      # >=2 viên gạch nhỏ 6x5 trong khối, không nằm ở cột mép (cột mép là dải khung cửa)
                elif (cc, rr_) in grate_blocks:
                    ch = "G"
                elif r["id"] == "crypt_b" and rr_ >= 10 and fb >= 0.4:
                    ch = "W"
                elif h_ in rail_hashes:
                    ch = "R"
                line.append(ch)
            rowsmap.append(line)
        # sàn sân (F): hàng 10 của Castle Entrance (nhìn ảnh): gạch lát kiểu khác (cam 2-45%)
        if r["id"] == "castle_entrance":
            for cc in range(cols):
                if rowsmap[10][cc] == ".":
                    rowsmap[10][cc] = "F"
        # cột cửa D
        for d in doors:
            if d["region"] == r["id"]:
                x0, y0, w0, h0 = d["rect_local"]
                for cc in range(x0 // 16, (x0 + w0 - 1) // 16 + 1):
                    for rr_ in range(y0 // 16, (y0 + h0 - 1) // 16 + 1):
                        rowsmap[rr_][cc] = "D"
        maps[r["id"]] = ["".join(l) for l in rowsmap]
        for l in rowsmap:
            counts[r["id"]].update(l)

    # ---------------------------------------------------------------- danh sách chạy ngang theo loại
    def runs_by_type(rid, types, min_len=1):
        out = []
        m = maps[rid]
        for rr_, line in enumerate(m):
            for (c0, c1) in runs_of([ch in types for ch in line]):
                if c1 - c0 + 1 >= min_len:
                    out.append((rr_, c0, c1))
        return out

    platforms = []
    for r in regions:
        gr = GROUND_ROW[r["id"]]
        for (rr_, c0, c1) in runs_by_type(r["id"], {"B", "F"}):
            role = "san_chinh" if rr_ == gr else "bục"
            platforms.append(dict(id="%s_r%d_c%d" % (r["id"], rr_, c0), region=r["id"], role=role, row=rr_, col0=c0, col1=c1,
                                  n_blocks=c1 - c0 + 1,
                                  rect_local=[c0 * 16, rr_ * 16, (c1 - c0 + 1) * 16, 16],
                                  rect_sheet=[c0 * 16 + r["x"], rr_ * 16 + r["y"], (c1 - c0 + 1) * 16, 16],
                                  top_y_local=rr_ * 16 if r["id"] != "castle_entrance" else rr_ * 16))
    # nước
    water = []   # (điền sau khi có rect_groups)
    # vực (khoảng trống trên hàng sàn chính)
    pits = []
    for r in regions:
        gr = GROUND_ROW[r["id"]]
        line = maps[r["id"]][gr]
        for (c0, c1) in runs_of([ch == "." for ch in line]):
            water_below = any(maps[r["id"]][rr_][cc] == "W" for rr_ in range(gr + 1, len(maps[r["id"]])) for cc in range(c0, c1 + 1))
            note = PIT_NOTES.get((r["id"], c0))
            if note is None:
                note = ("khoảng trống trên hàng sàn chính (hàng %d)%s" % (gr, "; bên dưới có nước (rơi xuống là chết)" if water_below else ""))
            pits.append(dict(region=r["id"], row=gr, col0=c0, col1=c1, width_px=(c1 - c0 + 1) * 16,
                             left_edge_x_local=c0 * 16, right_edge_x_local=(c1 + 1) * 16,
                             rect_sheet=[c0 * 16 + r["x"], gr * 16 + r["y"], (c1 - c0 + 1) * 16, 16],
                             water_below=water_below, la_vuc_that=(note.startswith("vực")), ghi_chu=note))
    # tường K / song sắt G / lan can R (gom thành khối chữ nhật theo thành phần liên thông của ký tự)
    def rect_groups(rid, ch):
        m = maps[rid]
        rows_, cols_ = len(m), len(m[0])
        mask = np.array([[c == ch for c in line] for line in m])
        if not mask.any():
            return []
        lab, n = S.label_components(mask, 8)
        out = []
        for s in S.comp_stats(lab, n):
            out.append(dict(region=rid, col0=s["x"], row0=s["y"], cols=s["w"], rows=s["h"], n_blocks=s["area"],
                            rect_local=[s["x"] * 16, s["y"] * 16, s["w"] * 16, s["h"] * 16],
                            rect_sheet=[s["x"] * 16 + R[rid]["x"], s["y"] * 16 + R[rid]["y"], s["w"] * 16, s["h"] * 16]))
        return out
    water = [dict(g, ghi_chu="nước: chạm là chết (theo hình vẽ); mặt nước bắt đầu ở y_local=%d" % g["rect_local"][1])
             for r in regions for g in rect_groups(r["id"], "W")]
    walls_k = [g for r in regions for g in rect_groups(r["id"], "K")]
    grates = [g for r in regions for g in rect_groups(r["id"], "G")]
    rails = [g for r in regions for g in rect_groups(r["id"], "R")]

    # dải khung cửa: chuỗi gạch nhỏ 6x5 xếp dọc cách 8 px (cột x giống nhau)
    strips = []
    for r in regions:
        reg = img[r["id"]]
        om = S.color_mask(reg, ORANGE)
        lab, n = S.label_components(om, 8)
        small = [(s["x"], s["y"]) for s in S.comp_stats(lab, n) if s["w"] == 6 and s["h"] == 5 and s["area"] == 28]
        byx = defaultdict(list)
        for (x, y) in small:
            byx[x].append(y)
        for x, yl in sorted(byx.items()):
            yl = sorted(yl)
            # chuỗi liên tiếp cách 8
            chain = [yl[0]]
            allc = []
            for y in yl[1:]:
                if y - chain[-1] == 8:
                    chain.append(y)
                else:
                    allc.append(chain)
                    chain = [y]
            allc.append(chain)
            for chn in allc:
                if len(chn) >= 4 and (x < 8 or x > r["w"] - 16):
                    strips.append(dict(region=r["id"], side="left" if x < 8 else "right", x_local=x, y_local=[chn[0], chn[-1] + 5],
                                       n_bricks=len(chn),
                                       rect_sheet=[x + r["x"], chn[0] + r["y"], 6, chn[-1] + 5 - chn[0]]))

    # ---------------------------------------------------------------- liên kết giữa các vùng (theo bố cục sheet + hình vẽ)
    def rect(r):
        return (r["x"], r["y"], r["x"] + r["w"], r["y"] + r["h"])
    adj = []
    for i in range(len(regions)):
        for j in range(len(regions)):
            if i == j:
                continue
            A, B = rect(regions[i]), rect(regions[j])
            # B nằm bên phải A
            if B[0] >= A[2] and B[0] - A[2] < 20 and min(A[3], B[3]) - max(A[1], B[1]) > 0:
                adj.append(dict(from_region=regions[i]["id"], edge="right", to_region=regions[j]["id"], to_edge="left",
                                gap_px=B[0] - A[2], overlap_y=[max(A[1], B[1]), min(A[3], B[3])]))
            if B[1] >= A[3] and B[1] - A[3] < 20 and min(A[2], B[2]) - max(A[0], B[0]) > 0:
                adj.append(dict(from_region=regions[i]["id"], edge="bottom", to_region=regions[j]["id"], to_edge="top",
                                gap_px=B[1] - A[3], overlap_x=[max(A[0], B[0]), min(A[2], B[2])]))
    # ---- cổng/ô trống ở mép (đo bằng pixel) ------------------------------------------------------
    gate_blk = img["castle_entrance"][80:160, 688:704]
    gate_black = float(S.color_mask(gate_blk, BLACK).mean())
    assert gate_black > 0.95, "vom cong khong den"
    doorway_blk = img["crypt_a"][32:80, 0:16]
    doorway_black = float(S.color_mask(doorway_blk, BLACK).mean())
    assert doorway_black == 1.0
    castle_gate = dict(region="castle_entrance", rect_local=[688, 64, 16, 96],
                       rect_sheet=[688 + R["castle_entrance"]["x"], 64 + R["castle_entrance"]["y"], 16, 96],
                       black_fraction_rows5_9=round(gate_black, 3),
                       ghi_chu="vòm cổng lâu đài: ô đen rộng 16 px trong tháp (cột 43, hàng 4-9, đỉnh vòm ở hàng 4); nhìn ảnh, nhiều khả năng trang trí")
    doorway = dict(region="crypt_a", rect_local=[0, 32, 16, 48], rect_sheet=[R["crypt_a"]["x"], 32 + R["crypt_a"]["y"], 16, 48],
                   black_fraction=doorway_black,
                   ghi_chu="khoảng đen 16x48 ở mép trái Crypt A (cột 0, hàng 2-4), cùng độ cao với cột cửa ở mép phải Entrance Hall")
    for dpole in doors:
        rid = dpole["region"]
        x0_, y0_, w0_, h0_ = dpole["rect_local"]
        row_below = (y0_ + h0_) // 16     # hàng ngay dưới chân cột
        col = x0_ // 16
        run = [(rr_, c0, c1) for (rr_, c0, c1) in runs_by_type(rid, {"B", "F"}) if rr_ == row_below and c0 <= col <= c1]
        dpole["stands_on_platform"] = dict(row=row_below, col0=run[0][1], col1=run[0][2]) if run else None
        dpole["cols_rows_blocks"] = dict(col=col, rows=[y0_ // 16, (y0_ + h0_ - 1) // 16])

    # liên kết giữa các vùng (theo bố cục sheet + hình vẽ); từng mục có bằng chứng số liệu
    by_id = {st_["id"]: st_ for st_ in stairs}

    def stair_shift(a_id, b_id):
        sa, sb = by_id[a_id], by_id[b_id]
        xa_last = sa["foot_tread_local"][0]          # bậc cuối (chân thang bị cắt ở đáy vùng A)
        xa_virtual = xa_last + 8                      # bậc kế tiếp sẽ nằm ở y_local = 192 -> y=0 của vùng B
        return sb["top_tread_local"][0] - xa_virtual
    link_stairs = [("crypt_a_stair_0", "crypt_b_stair_0"), ("crypt_a_stair_2", "crypt_b_stair_1")]
    shifts = {"%s->%s" % (x, y): stair_shift(x, y) for x, y in link_stairs}
    connections = [
        dict(id="c1", from_region="castle_entrance", from_edge="right", to_region="entrance_hall", to_edge="left",
             sheet_gap_px=5, via="vòm cổng trong tháp ở cuối sân (cột 43, hàng 4-9) rồi tới mép phải sân (x=768)",
             do_tin_cay="trung bình (suy luận từ bố cục + hình vẽ)",
             ghi_chu="2 vùng đặt kề nhau trên sheet (khe xanh 5 px); không có cột cửa ở mép phải sân."),
        dict(id="c2", from_region="entrance_hall", from_edge="right", to_region="crypt_a", to_edge="left",
             sheet_gap_px=5, via="cột cửa đỏ-trắng ở mép phải Hall (cột 95, hàng 2-4) đứng trên bục hàng 5 (cột 87-95, y_local 80) "
                                  "<-> khoảng đen 16x48 ở mép trái Crypt A (cột 0, hàng 2-4) trên bục hàng 5 (cột 0-7)",
             do_tin_cay="cao (cùng độ cao y sheet 43..90 ở 2 bên, đo bằng pixel)", ghi_chu="Hall: bục hàng 5 cuối bản đồ dẫn tới cửa."),
        dict(id="c3", from_region="crypt_a", from_edge="right", to_region="phantom_bat", to_edge="left",
             sheet_gap_px=5, via="cột cửa đỏ-trắng ở mép phải Crypt A (cột 31, hàng 2-4) đứng trên bục hàng 5 (cột 26-31) "
                                  "-> bục hàng 5 (cột 0-6) ở mép trái Phantom Bat (cùng y sheet 91)",
             do_tin_cay="trung bình (mép trái Phantom Bat không có khoảng đen, chỉ khớp độ cao bục)", ghi_chu=""),
        dict(id="c4", from_region="crypt_a", from_edge="bottom", to_region="crypt_b", to_edge="top",
             sheet_gap_px=15, via="2 giếng cầu thang ở sàn Crypt A (cột 3 và 23, hàng 11) <-> 2 cầu thang có đỉnh sát mép trên Crypt B (cột 2 và 22)",
             do_tin_cay="cao (bậc thang khớp sau khi dịch ngang)", x_shift_px_B_minus_A=shifts,
             ghi_chu="bậc thang kéo dài từ A xuống B với độ lệch ngang x_B = x_A + shift (cùng giá trị cho cả 2 cầu thang)."),
        dict(id="c5", from_region="phantom_bat", from_edge="right", to_region=None, to_edge=None,
             sheet_gap_px=None, via="cột cửa đỏ-trắng ở mép phải (cột 47, hàng 4-6) đứng trên bục hàng 7 (cột 45-47)",
             do_tin_cay="trung bình", ghi_chu="cuối Level 1 (phòng boss Phantom Bat); chưa thấy vùng nào nối tiếp trên sheet."),
        dict(id="c6", from_region="crypt_b", from_edge="right", to_region=None, to_edge=None, sheet_gap_px=None,
             via="dải khung cửa (gạch nhỏ cam) ở mép phải, KHÔNG có cột đỏ-trắng", do_tin_cay="thấp",
             ghi_chu="Crypt B chỉ thấy lối vào bằng 2 cầu thang từ Crypt A; mép phải không có vùng nối tiếp trên sheet."),
    ]

    # ---------------------------------------------------------------- bảng màu
    mask_in = inreg
    logo_mask = np.zeros((H, W), bool)
    logo_mask[logo["y"]:logo["y"] + logo["h"], logo["x"]:logo["x"] + logo["w"]] = True
    ROLE = {
        (0, 128, 0): "nền sheet (ngoài vùng bản đồ)",
        (0, 0, 0): "đen: nền tối/ngoài trời, hàng đáy sân, bóng, vân tường",
        (188, 188, 188): "xám sáng: tường đá Entrance Hall/Phantom Bat, thân trụ",
        (116, 116, 116): "xám giữa: gạch tường Crypt, vân đá, bóng trụ",
        (200, 76, 12): "cam gạch: khối đứng được, bậc thang, tường gạch cam",
        (36, 24, 140): "xanh dương đậm: trời sân, kính cửa sổ, nước Crypt B",
        (0, 68, 0): "xanh lá đậm: tán cây, khung cửa sổ, rêu",
        (0, 148, 0): "xanh lá nhạt: tán cây, khung cửa sổ, rêu",
        (216, 40, 0): "đỏ: rèm, cột cửa, logo",
        (252, 116, 180): "hồng: bóng sáng rèm, logo",
        (252, 252, 252): "trắng: chữ nhãn (ngoài vùng) và viền cột cửa (trong vùng)",
        (0, 64, 0): "bóng chữ logo credit",
    }
    bang_mau = []
    for col, cnt in hist.most_common():
        cm = S.color_mask(a, col)
        bang_mau.append(dict(rgb=list(col), hex=S.hex_color(col), so_pixel=int(cnt), trong_vung=int((cm & mask_in).sum()),
                             trong_logo=int((cm & logo_mask).sum()), vai_tro=ROLE.get(col, "?")))

    # ---------------------------------------------------------------- kích thước / bội số
    for r in regions:
        r["cols16"] = r["w"] // 16
        r["rows16"] = r["h"] // 16
        r["w_multiple_of_16"] = (r["w"] % 16 == 0)
        r["h_multiple_of_16"] = (r["h"] % 16 == 0)
        r["screens_256"] = r["w"] / 256.0
        r["filled_rectangle"] = True
        r["grid"] = grid_info[r["id"]]
        r["tiles"] = tstats[r["id"]]
        r["block_counts"] = dict(counts[r["id"]])
        r["rect"] = [r["x"], r["y"], r["w"], r["h"]]
        r["ground_row"] = GROUND_ROW[r["id"]]
    result = dict(
        sheet_file=S.SHEET_FILES[KEY], sheet_key=KEY, size=[W, H], bg_color=list(S.BG),
        logo_credit=dict(rect=[logo["x"], logo["y"], logo["w"], logo["h"]], ghi_chu="khung credit Mister Mike: BỎ QUA khi cắt"),
        bang_mau=bang_mau,
        labels=labels,
        regions=[{k: v for k, v in r.items() if k not in ("x", "y", "w", "h")} for r in regions],
        adjacency_by_sheet_layout=adj,
        connections=connections,
        castle_gate_arch=castle_gate,
        crypt_a_left_doorway=doorway,
        door_poles=doors,
        door_frame_strips=strips,
        grid=dict(metatile=16, tile=8, offset=[0, 0], origin="góc trên-trái của từng vùng",
                  ghi_chu="cả 5 vùng căn đúng lưới 16x16 và 8x8 ở offset (0,0) so với góc trên-trái vùng; mọi offset khác cho nhiều khối duy nhất hơn rõ rệt"),
        tile_stats_total=total_unique,
        tile_stats_per_region=tstats,
        legend={
            "B": "gạch cam 16x16 (đứng được)", "F": "sàn sân Castle Entrance (hàng 10)", "U": "ô có bậc thang đi lên sang phải (↗)",
            "L": "ô có bậc thang đi lên sang trái (↖)", "W": "nước (Crypt B, chết)", "K": "tường gạch cam dạng lưới (có thể phá)",
            "G": "song/lưới sắt (trang trí)", "D": "cột cửa đỏ-trắng", "R": "lan can/rào (trang trí)", ".": "nền/trang trí khác",
        },
        block_maps=maps,
        stairs=stairs,
        platforms=platforms,
        water=water,
        pits_on_ground_row=pits,
        brick_walls_K=walls_k,
        iron_grates_G=grates,
        rails_R=rails,
    )
    return result, a, regions, img


# --------------------------------------------------------------------------------------------------
def render(result, a, regions):
    H, W = a.shape[:2]
    Rg = {r["id"]: r for r in regions}
    colors = {"B": (0, 255, 0), "F": (0, 200, 120), "U": (255, 0, 255), "L": (255, 120, 255), "W": (0, 120, 255),
              "K": (255, 255, 0), "G": (200, 200, 200), "D": (255, 0, 0), "R": (150, 150, 255)}

    def overlay(scale, x0, x1, grid, y1=None):
        crop = a[:y1, x0:x1] if y1 else a[:, x0:x1]
        im = Image.fromarray(crop).resize((crop.shape[1] * scale, crop.shape[0] * scale), Image.NEAREST).convert("RGBA")
        ov = Image.new("RGBA", im.size, (0, 0, 0, 0))
        d = ImageDraw.Draw(ov)
        font = S.get_font(12 if scale >= 2 else 10)
        bigfont = S.get_font(16 if scale >= 2 else 12)
        for r in regions:
            m = result["block_maps"][r["id"]]
            for ri, line in enumerate(m):
                for ci, ch in enumerate(line):
                    if ch == ".":
                        continue
                    bx0 = (r["x"] + ci * 16 - x0) * scale
                    by0 = (r["y"] + ri * 16) * scale
                    bx1, by1 = bx0 + 16 * scale, by0 + 16 * scale
                    if bx1 < 0 or bx0 > im.size[0]:
                        continue
                    col = colors[ch]
                    d.rectangle([bx0, by0, bx1 - 1, by1 - 1], fill=col + (70,), outline=col + (200,))
                    if scale >= 2:
                        S.draw_text_outlined(d, (bx0 + 3, by0 + 2), ch, (255, 255, 255, 255), font)
            # khung vùng
            rx0, ry0 = (r["x"] - x0) * scale, r["y"] * scale
            d.rectangle([rx0 - 1, ry0 - 1, rx0 + r["w"] * scale, ry0 + r["h"] * scale], outline=(0, 255, 255, 255))
            S.draw_text_outlined(d, (rx0 + 4, ry0 + 4), "%s [%d,%d,%d,%d]" % (r["label"], r["x"], r["y"], r["w"], r["h"]),
                                 (0, 255, 255, 255), bigfont)
        # bậc thang: nối các bậc
        for s in result["stairs"]:
            rr = Rg[s["region"]]
            pts = [((rr["x"] + p[0] - x0) * scale + 4 * scale, (rr["y"] + p[1]) * scale + scale) for p in s["steps_local"]]
            if len(pts) >= 2:
                d.line(pts, fill=(255, 0, 255, 255), width=max(2, scale))
            for p in pts:
                d.ellipse([p[0] - 2, p[1] - 2, p[0] + 2, p[1] + 2], fill=(255, 255, 0, 255))
        # nhãn chữ
        for L in result["labels"]:
            lx, ly, lw, lh = L["bbox"]
            d.rectangle([(lx - x0) * scale - 1, ly * scale - 1, (lx + lw - x0) * scale, (ly + lh) * scale], outline=(255, 255, 0, 255))
        # logo
        lg = result["logo_credit"]["rect"]
        d.rectangle([(lg[0] - x0) * scale, lg[1] * scale, (lg[0] + lg[2] - x0) * scale, (lg[1] + lg[3]) * scale], outline=(255, 128, 0, 255))
        if grid:
            for r in regions:
                for ci in range(r["w"] // 16 + 1):
                    X = (r["x"] + ci * 16 - x0) * scale
                    if 0 <= X <= im.size[0]:
                        d.line([(X, r["y"] * scale), (X, (r["y"] + r["h"]) * scale)], fill=(255, 255, 255, 60 if ci % 16 else 140), width=1)
                        if ci % 4 == 0:
                            S.draw_text_outlined(d, (X + 2, (r["y"] + r["h"]) * scale - 14), str(ci), (255, 255, 0, 255), S.get_font(11))
                for ri in range(r["h"] // 16 + 1):
                    Y = (r["y"] + ri * 16) * scale
                    d.line([((r["x"] - x0) * scale, Y), ((r["x"] + r["w"] - x0) * scale, Y)], fill=(255, 255, 255, 60), width=1)
                    if r["x"] - x0 >= 0 and r["x"] - x0 < im.size[0] // scale:
                        S.draw_text_outlined(d, ((r["x"] - x0) * scale + 2, Y + 2), str(ri), (0, 255, 255, 255), S.get_font(11))
        return Image.alpha_composite(im, ov).convert("RGB")

    overlay(1, 0, W, False).save(os.path.join(S.OUT_DIR, "level1_entrance_annot.png"))
    cuts = [(0, 900), (900, 1800), (1800, 2700), (2700, W)]
    for i, (x0, x1) in enumerate(cuts, 1):
        # phần chỉ có hàng vùng trên (y<=203) thì cắt bớt phần xanh trống; phần có Crypt B/logo giữ đủ chiều cao
        has_lower = any(r["y"] > 100 and r["x"] < x1 and r["x"] + r["w"] > x0 for r in regions) or x1 >= W
        overlay(2, x0, x1, True, None if has_lower else 215).save(os.path.join(S.OUT_DIR, "level1_entrance_annot_%d.png" % i))


def main():
    S.ensure_dir(S.OUT_DIR)
    result, a, regions, img = analyze()
    S.dump_json_compact(os.path.join(S.OUT_DIR, "level1_entrance.json"), result,
                        one_line_keys=("bang_mau", "labels", "regions", "adjacency_by_sheet_layout", "connections", "door_poles", "door_frame_strips",
                                       "stairs", "platforms", "water", "pits_on_ground_row", "brick_walls_K", "iron_grates_G", "rails_R"))
    render(result, a, regions)
    # tóm tắt ASCII
    print("regions:", [(r["id"], r["rect"]) for r in result["regions"]])
    print("tile totals:", result["tile_stats_total"])
    for r in result["regions"]:
        print(" ", r["id"], "unique8=%d unique16=%d" % (r["tiles"]["unique8"], r["tiles"]["unique16"]), "blocks:", r["block_counts"])
    print("stairs:", len(result["stairs"]), "platform runs:", len(result["platforms"]), "water runs:", len(result["water"]),
          "pits:", len(result["pits_on_ground_row"]), "K:", len(result["brick_walls_K"]), "G:", len(result["iron_grates_G"]),
          "R:", len(result["rails_R"]), "door poles:", len(result["door_poles"]), "strips:", len(result["door_frame_strips"]))


if __name__ == "__main__":
    main()
