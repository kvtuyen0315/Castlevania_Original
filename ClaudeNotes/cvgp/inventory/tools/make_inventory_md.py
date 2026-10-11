# -*- coding: utf-8 -*-
"""
make_inventory_md.py - SINH BÁO CÁO TỔNG HỢP  ..\\SHEETS_INVENTORY.md  từ các file JSON đã xuất.

Cách chạy (sau khi đã chạy inv_sprites.py và inv_level1.py), KHÔNG dùng -I:
    python -P -E make_inventory_md.py
Mọi số liệu trong báo cáo được đọc từ JSON nên chạy lại ra y hệt; phần văn bản nhận xét là hằng số trong script này.
"""
import json
import os
import re
import sys
from collections import Counter, OrderedDict

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sheetlib as S  # noqa: E402

OUT = S.OUT_DIR
NGAY = "2026-10-11"


def load(key):
    with open(os.path.join(OUT, key + ".json"), encoding="utf-8") as f:
        return json.load(f)


def esc(s):
    return str(s).replace("|", "/").replace("\n", " ").strip()


def rgb(c):
    return "(%d,%d,%d)" % tuple(c)


def size_summary(sizes, top=4):
    items = sorted(sizes.items(), key=lambda kv: (-kv[1], kv[0]))
    txt = ", ".join("%s x%d" % (k, v) if v > 1 else k for k, v in items[:top])
    if len(items) > top:
        txt += ", ..."
    return txt


# ------------------------------------------------------------------------------------------------
# bảng nhóm (có gộp các nhóm Simon lặp lại)
# ------------------------------------------------------------------------------------------------
COLLAPSE = [
    (re.compile(r"^simon_use_(stand|duck|stairs_up|stairs_down)$"), "simon_use_*",
     "Whip/Use Subweapon (4 nhóm con: Standing, Ducking, Ascending Stairs, Descending Stairs)",
     "mỗi nhóm con 6 ô: [0] chuẩn bị, [1] biến thể thu hẹp (16 rộng), [2..5] tay duỗi với 4 màu vật cầm (không/tím/xanh/hồng-đỏ); "
     "ô là hợp tile 8x16 (fill 0.62-0.83); nhóm Ducking: 8 px cuối ô trống"),
    (re.compile(r"^simon_whip(1|2|3c[1-4])_(stand|duck|stairs_up|stairs_down)$"), None, None, None),
]


def whip_collapse_meta(band):
    names = {"1": "Whip (Level 1)", "2": "Whip (Level 2)", "3c1": "Whip (Level 3, Color 1)", "3c2": "Whip (Level 3, Color 2)",
             "3c3": "Whip (Level 3, Color 3)", "3c4": "Whip (Level 3, Color 4)"}
    colors = {"1": "hồng/trắng viền đen", "2": "tím (chuỗi)", "3c1": "cam", "3c2": "tím", "3c3": "xanh dương", "3c4": "đỏ"}
    if band == "1":
        note = ("4 nhóm con: Standing/Jumping (5 ô), Ducking (5 ô), Ascending Stairs (4 ô), Descending Stairs (4 ô). Ô: [0] chuẩn bị (roi rủ), "
                "[1] roi vắt qua đầu, [2][3] roi duỗi (rộng 44 px, thân Simon ở 16 px bên PHẢI, roi bên trái), [4] thân Simon 16x32 lẻ "
                "(chỉ ở Standing và Ducking, loại manh). Ducking: 8 px cuối ô trống. Simon quay mặt TRÁI. Roi %s." % colors[band])
    elif band == "2":
        note = "Như `simon_whip1_*` (ô roi duỗi rộng 44 px). Roi %s." % colors[band]
    else:
        note = "Như `simon_whip1_*` nhưng roi dài hơn: ô roi duỗi rộng 60 px. Roi %s." % colors[band]
    return names[band] + " (4 nhóm con)", note


def group_rows(d, collapse):
    rows = []
    seen = OrderedDict()
    for g in d["groups"]:
        key = None
        if collapse:
            for rx, label, text, note in COLLAPSE:
                m = rx.match(g["id"])
                if m:
                    if label is None:   # roi
                        band = m.group(1)
                        key = "simon_whip%s_*" % band
                        t, n = whip_collapse_meta(band)
                        seen.setdefault(key, dict(id=key, nhan=t, ghi_chu=n, groups=[]))["groups"].append(g)
                    else:
                        key = label
                        seen.setdefault(key, dict(id=key, nhan=text, ghi_chu=note, groups=[]))["groups"].append(g)
                    break
        if key is None:
            seen["g:" + g["id"]] = dict(id=g["id"], nhan=g["nhan"], ghi_chu=g["ghi_chu"], groups=[g])
    for k, v in seen.items():
        gs = v["groups"]
        sizes = Counter()
        for g in gs:
            sizes.update(g["kich_thuoc"])
        n = sum(g["so_o"] for g in gs)
        lo, hi = min(g["stt"][0] for g in gs), max(g["stt"][1] for g in gs)
        if len(gs) > 1:
            cnt = "+".join(str(g["so_o"]) for g in gs) + "=" + str(n)
        else:
            cnt = str(n)
        rows.append((v["id"], v["nhan"], "%d-%d" % (lo, hi), cnt, size_summary(sizes), v["ghi_chu"]))
    return rows


def md_group_table(d, collapse=False):
    L = ["| Nhóm (id) | Nhãn chữ | stt | Số khung | Kích thước điển hình | Ghi chú |", "|---|---|---|---|---|---|"]
    for (gid, nhan, stt, cnt, size, note) in group_rows(d, collapse):
        if nhan.startswith("(không có nhãn chữ"):
            nhan = "(gán tay theo toạ độ)"
        L.append("| `%s` | %s | %s | %s | %s | %s |" % (gid, esc(nhan), stt, cnt, esc(size), esc(note)))
    return L


def kind_counts(d):
    c = Counter(x["loai"] for x in d["cells"])
    names = {"frame": "khung", "manh": "mảnh", "dan": "đạn/vũ khí ném", "hieu_ung": "hiệu ứng", "item": "vật phẩm", "vat_the": "vật thể"}
    return ", ".join("%s %d" % (names.get(k, k), v) for k, v in sorted(c.items(), key=lambda kv: -kv[1]))


def colors_line(d):
    parts = []
    for c in d["bang_mau"]:
        parts.append("%s %s: %d px (%s)" % (c["hex"], rgb(c["rgb"]), c["so_pixel"], c["vai_tro"]))
    return parts


def sheet_header(d, title, extra=None):
    t = d["tong_ket"]
    L = ["## %s — `%s` (%d×%d)" % (title, d["sheet_key"], d["size"][0], d["size"][1]), ""]
    L.append("File nguồn: `%s`" % d["sheet_file"])
    L.append("")
    L.append("| Mục | Giá trị |")
    L.append("|---|---|")
    L.append("| Màu nền sheet | %s (xanh lá) |" % rgb(d["bg_color"]))
    L.append("| Màu key (nền ô) | %s (xám) |" % rgb(d["key_color"]))
    L.append("| Màu chữ nhãn | %s |" % rgb(d["text_color"]))
    L.append("| Số màu khác nhau | %d |" % len(d["bang_mau"]))
    L.append("| Số nhãn chữ | %d (%d nhãn nhóm + %d nhãn tiêu đề) |" % (t["so_nhan"], t["so_nhan_nhom"], t["so_nhan"] - t["so_nhan_nhom"]))
    L.append("| Số nhóm | %d |" % t["so_nhom"])
    L.append("| **Số ô** | **%d** (loại: %s) |" % (t["so_o"], kind_counts(d)))
    L.append("| Ô không đặc (hợp tile, fill<1) | %d |" % t["so_o_khong_dac"])
    L.append("| Ô không có pixel xám | %d |" % t["so_o_khong_xam"])
    L.append("| Ô dính liền (nhiều khung 1 ô) | %d |" % t["so_o_dinh_lien"])
    lg = d["logo_credit"]["rect"]
    L.append("| Logo credit (bỏ qua) | rect [%d,%d,%d,%d] + %d mảnh chữ bên trong |" % (lg[0], lg[1], lg[2], lg[3], d["logo_credit"]["so_manh_chu_trong_logo"]))
    if extra:
        L.extend(extra)
    L.append("")
    L.append("**Histogram màu** (số pixel toàn sheet; vai trò): " + "; ".join(
        "`%s` %s %d = %s" % (c["hex"], rgb(c["rgb"]), c["so_pixel"], c["vai_tro"]) for c in d["bang_mau"]) + ".")
    L.append("")
    return L


EDGE_VI = {"right": "phải", "left": "trái", "top": "trên", "bottom": "dưới"}


def reduce_group(k):
    m = re.match(r"^simon_whip(1|2|3c[1-4])_(stand|duck|stairs_up|stairs_down)$", k)
    if m:
        return "simon_whip%s_*" % m.group(1)
    if k.startswith("simon_use_"):
        return "simon_use_*"
    return k


def anomalies_md(d, manual):
    L = ["**Ô / hiện tượng bất thường**", ""]
    nogray = [b for b in d["bat_thuong"] if b["loai"] == "o_khong_xam"]
    if nogray:
        L.append("- %d ô KHÔNG có pixel xám: %s. Toàn bộ chữ nhật là pixel đặc: cắt nguyên chữ nhật, không key-out." %
                 (len(nogray), "; ".join("`%s` %s viền %s" % (b["id"], b["rect"], b["ghi_chu"].split("viền ô ")[-1]) for b in nogray)))
    for b in d["bat_thuong"]:
        if b["loai"] == "mau_la":
            L.append("- Màu lạ %s: %d px — %s" % (rgb(b["rgb"]), b["so_pixel"], esc(b["ghi_chu"])))
        elif b["loai"] == "o_dinh_lien":
            L.append("- Ô DÍNH LIỀN: `%s` rect %s; gợi ý tách (toạ độ tương đối trong ô): %s" % (b["id"], b["rect"], b["goi_y_tach"]))
        elif b["loai"] == "o_khong_dac":
            red = Counter()
            for k, v in b["theo_nhom"].items():
                red[reduce_group(k)] += v
            L.append("- %d ô không đặc (fill<1) trong các nhóm: %s. Ô là hợp tile 8x16: bbox đúng nhưng có góc trống màu xanh; "
                     "khi cắt giữ nguyên bbox, coi pixel xanh/xám là trong suốt." % (sum(red.values()), ", ".join("`%s` %d" % kv for kv in red.items())))
        elif b["loai"] == "o_co_xam_khep_kin":
            if b["so_o"]:
                L.append("- %d ô có vùng xám key KHÉP KÍN nhỏ (tổng %d px, ví dụ `%s`): thường là lỗ trong sprite (vòng roi, quai bình...); "
                         "key-out theo màu vẫn đúng. Chi tiết ở trường `key_khep_kin` của từng ô." % (b["so_o"], b["tong_px"], "`, `".join(b["vi_du"][:3])))
        elif b["loai"] == "kiem_tra":
            L.append("- Kiểm tra: %s" % esc(b["ghi_chu"]))
    cao = [b for b in d["bat_thuong"] if b["loai"] == "o_cao_hon_slot"]
    if cao:
        L.append("- %d ô cao hơn slot 32 px (roi thò ra): %s" % (len(cao), ", ".join("`%s` %s" % (b["id"], "x".join(str(v) for v in b["rect"][2:])) for b in cao)))
    for m in manual:
        L.append("- " + m)
    L.append("")
    return L


# ------------------------------------------------------------------------------------------------
def build():
    L = []
    simon = load("simon_belmont")
    ene = load("enemies")
    bos = load("bosses")
    itm = load("items_objects")
    lv1 = load("level1_entrance")
    n_cells = sum(x["tong_ket"]["so_o"] for x in (simon, ene, bos, itm))

    L += ["# Kiểm kê 5 sprite sheet Castlevania (NES) — rip bởi Mister Mike", ""]
    L += ["Tạo ngày %s bằng `tools\\run_all.py` (chạy lại ra y hệt). Không sửa file nào trong dự án Unity; mọi thứ nằm trong thư mục này." % NGAY, ""]
    L += ["Nguồn (chỉ đọc): `C:\\Users\\kvtuy\\Downloads\\NES - Castlevania - ....png` (5 file; hai sheet Title Screen và Cutscenes & Credits đã cắt trước đó, không làm lại).", ""]
    L += ["| File | Nội dung |", "|---|---|"]
    for k, t in [("simon_belmont", "Simon Belmont: %d ô / %d nhóm" % (simon["tong_ket"]["so_o"], simon["tong_ket"]["so_nhom"])),
                 ("enemies", "Enemies: %d ô / %d nhóm" % (ene["tong_ket"]["so_o"], ene["tong_ket"]["so_nhom"])),
                 ("bosses", "Bosses: %d ô / %d nhóm" % (bos["tong_ket"]["so_o"], bos["tong_ket"]["so_nhom"])),
                 ("items_objects", "Items & Objects: %d ô / %d nhóm" % (itm["tong_ket"]["so_o"], itm["tong_ket"]["so_nhom"]))]:
        L.append("| `%s.json`, `%s_annot.png` | %s (JSON: nhãn, nhóm, hàng, ô, bất thường; ảnh: khung + số thứ tự `stt`) |" % (k, k, t))
    L.append("| `level1_entrance.json`, `level1_entrance_annot.png`, `level1_entrance_annot_1..4.png` | Bản đồ Level 1: vùng, lưới, thống kê tile, loại khối, cầu thang, nước, cửa, bản đồ ký tự |")
    L.append("| `tools\\` | `sheetlib.py`, `sheet_specs.py` (gán nhãn thủ công), `inv_sprites.py`, `inv_level1.py`, `make_inventory_md.py`, `run_all.py` |")
    L += ["", "Tổng cộng sheet 1-4: **%d ô** sprite (+ 5 vùng bản đồ ở sheet 5)." % n_cells, ""]

    # ---------------------------------------------------------------- 0. quy ước
    L += ["## 0. Quy ước chung (đã đo trên cả 5 sheet)", ""]
    L += [
        "- Cả 5 sheet đều đục hoàn toàn (alpha=255), chỉ 11-14 màu NES. Nền sheet = **(0,128,0)**; mỗi sprite nằm trên ô nền xám **(116,116,116)** (màu key). "
        "Chữ nhãn = **(255,255,255)** ở sheet 1-4 (font 8 px, cao 7-8 px) và (252,252,252) ở sheet 5.",
        "- **Ô sprite** = thành phần liên thông (8 láng giềng) của pixel khác nền sheet và khác chữ trắng. Đã kiểm liên thông 4 và 8 cho cùng kết quả "
        "(ngoài logo) => không có ô nào chỉ dính nhau ở góc chéo; các ô cùng nhóm thường cách nhau đúng 1 px xanh (bước 17 px với ô 16 px; riêng hai bảng màu Vampire Bat cách 3 px).",
        "- `rect` = [x, y, w, h] theo pixel gốc, gốc ở góc trên-trái sheet. Ô Simon/boss là **hợp các tile 8x16** nên nhiều ô không phải chữ nhật đặc (`fill` < 1): "
        "bbox vẫn chính xác, phần trống trong bbox là nền xanh.",
        "- Quy ước khi cắt: pixel xám (116,116,116) và pixel xanh nền (0,128,0) trong bbox = trong suốt; **đen (0,0,0) là màu thật**. "
        "`co_den_that` = ô có pixel đen thật. Ô không có xám (Medusa nền đen, 8 khối 8x16 ở sheet items) là chữ nhật đặc, không key-out.",
        "- Logo credit \"Sprites ripped by Mister Mike\" (khung 224x32 + bóng chữ (0,64,0) = 516 px) có ở mọi sheet: bỏ qua khi cắt.",
        "- Id: `<nhóm>_<chỉ số>` (chỉ số theo thứ tự trái->phải, trên->dưới trong nhóm) hoặc tên riêng khi đã nhận diện được (vd `item_heart_small`). "
        "Id duy nhất trên cả 4 sheet (đã kiểm).",
        "- `loai`: frame = khung hoạt hình, manh = mảnh/biến thể cần ghép, dan = đạn/vũ khí ném, hieu_ung, item, vat_the. Phân loại chỉ dựa vào nhìn ảnh nên mục \"manh\" và vài tên item có ghi chú \"chưa chắc\".",
        "",
    ]

    # ---------------------------------------------------------------- 1. Simon
    L += sheet_header(simon, "1. Simon Belmont")
    L += ["**Bố cục**: 9 hàng ô. Hàng 0 (y 21-52): Idle, Walk, Jump/Duck, Climb Stairs, Hurt, Death, Intro, Colors. Hàng 1 (y 78-109): Whip/Use Subweapon. "
          "Hàng 2-7: Whip Level 1, Level 2, Level 3 màu 1-4 (mỗi hàng 4 nhóm con Standing/Jumping, Ducking, Ascending Stairs, Descending Stairs). "
          "Hàng 8 (y 477+): roi rời, Subweapons, Hit Effect.", ""]
    L += ["**Bảng nhóm** (nhóm roi và Use Subweapon được gộp dòng; JSON có đủ %d nhóm):" % simon["tong_ket"]["so_nhom"], ""]
    L += md_group_table(simon, collapse=True)
    L += ["", "**Hàng và baseline y** (slot Simon cao 32 px; baseline_y = đáy slot, dùng làm pivot chân):", ""]
    L += ["| Hàng | y0..y1 | Số ô | baseline_y | Đỉnh slot | Ô lệch đáy |", "|---|---|---|---|---|---|"]
    for r in simon["rows"]:
        if "baseline_y" in r:
            L.append("| %d | %d..%d | %d | **%d** | %d | %s |" % (r["id"], r["y0"], r["y1"], r["so_o"], r["baseline_y"], r["slot_top_y"],
                                                              ", ".join("`%s`" % x for x in r["o_lech_day"]) or "-"))
        else:
            L.append("| %d | %d..%d | %d | (không áp dụng: roi rời + vũ khí phụ) | - | - |" % (r["id"], r["y0"], r["y1"], r["so_o"]))
    L += ["",
          "- Mọi ô Simon trong hàng 0-7 có đáy slot trùng baseline_y, trừ các ô thò roi thấp hơn chân (+1..2 px, nhóm Ascending Stairs [0]) và ô nằm ngang `simon_death_1` (đáy ở y+16).",
          "- Chân (pixel sprite thấp nhất) cách đáy ô: **0-1 px** khi đứng/đi/leo cầu thang; **8 px** khi cúi (Jump/Duck, Ducking, Death[0]) vì tile dưới trống. "
          "=> pivot nên lấy theo baseline_y của hàng (đáy slot 32 px), không lấy theo pixel thấp nhất, để các khung khớp nhau như trong game.",
          "- Trường `chan_x` = [x0, x0+16] (tương đối trong ô) là **cột chân**: đoạn cột rộng đúng 16 px mà ô phủ đủ 16 hàng cuối của slot (2 tile chân 8x16 của Simon), "
          "và `pivot_goi_y` = [x0+8, baseline_rel] (baseline_rel = baseline_y - y của ô). Quy tắc này cho đáp án DUY NHẤT với 149/150 ô Simon hàng 0-7 "
          "(trừ ô nằm ngang `simon_death_1`). Ví dụ pivot X: ô 16 rộng = 8; chuẩn bị/vắt roi 32 rộng = 8; roi duỗi 44 rộng = 36, 60 rộng = 52; nhóm Use Subweapon 24 rộng = 8 (khung [0]) hoặc 16 (khung [2..5]).",
          "- Với ô rộng hơn 16 px, trường `phan` gợi ý tách thành `phu_trai` / `cot_chan` / `phu_phai` (phần phụ = roi, tay, vũ khí); chúng dính liền không có biên xanh nên vẫn giữ 1 ô.",
          "- Simon quay mặt TRÁI trong sheet (roi duỗi sang trái); cần lật ngang khi quay phải.", ""]
    L += anomalies_md(simon, [
        "Không có ô dính liền thật: mọi ô cách nhau >= 1 px xanh. Các ô roi dài (44/60 rộng) là 1 ô ghép thân + roi, không phải 2 khung.",
        "Ô 16x32 lẻ ở cuối nhóm Standing/Ducking (trùng từng pixel với thân trong ô roi duỗi, đã kiểm) là mảnh thân Simon (loại `manh`), không phải khung riêng.",
        "Hàng 8: mảnh roi rời, vũ khí phụ, hit effect không nằm trên slot 32 px (không có baseline).",
    ])

    # ---------------------------------------------------------------- 2. Enemies
    L += sheet_header(ene, "2. Enemies")
    L += ["**Bố cục**: 4 hàng nhãn/ô; mỗi nhãn nằm ngay trên nhóm của nó. Zombie, Fish Man, Black Knight xếp 2 tầng (tầng trên 16x16, tầng dưới 16x32).", ""]
    L += ["**Bảng nhóm**:", ""]
    L += md_group_table(ene)
    L += [""]
    h31 = [c["id"] for c in ene["cells"] if c["rect"][3] == 31]
    L += anomalies_md(ene, [
        "Các ô 16x31 (%s) có đỉnh thấp hơn 1 px nhưng đáy y+h = 60 thẳng hàng với ô 16x32 cùng hàng (không phải ô khác loại)." % ", ".join("`%s`" % x for x in h31),
        "Mọi ô là chữ nhật đặc (fill=1) — không có ô hợp tile như Simon.",
        "Nhóm có mảnh 16x16 ở tầng trên (Zombie, Fish Man, Black Knight): nhìn ảnh có vẻ là nửa thân/biến thể ghép thành khung 16x32 tầng dưới — ghi `loai=manh`, chưa chắc.",
    ])

    # ---------------------------------------------------------------- 3. Bosses
    L += sheet_header(bos, "3. Bosses")
    L += ["**Bố cục**: 5 hàng ô (Phantom Bat/Medusa/Mummy Man; Frankenstein & Igor/Grim Reaper/Dracula P1; Dracula P2 hai tầng; Death Effect + Unused).", ""]
    L += ["**Bảng nhóm**:", ""]
    L += md_group_table(bos)
    L += [""]
    L += anomalies_md(bos, [
        "2 ô Medusa `boss_medusa_0/1` (32x48) có NỀN ĐEN thật (đen là pixel đặc, không phải key).",
        "Dracula (Phase 2) và Phantom Bat là ô hợp nhiều tile (fill 0.67-0.89, cao tới 80 px): giữ nguyên bbox.",
        "Death Effect có 3 cỡ khối ghép sẵn (8x16, 24x32, 24x48): mỗi ô là 1 khung của cỡ đó, không phải nhiều khung.",
    ])

    # ---------------------------------------------------------------- 4. Items
    L += sheet_header(itm, "4. Items & Objects")
    L += ["**Bố cục**: lưới 17 px (ô 16 px + 1 px khe), 6 hàng ô: hàng y=1 (item cơ bản, nến, đuốc, 2 cửa), y=18 (vũ khí phụ + II/III), y=35 (đồ ăn, vương miện, rương), "
          "y=52 (8 cặp khối), y=69 (mảnh số điểm + gạch), y=83 (Unused). Sheet **chỉ có một nhãn chữ** (\"Unused\"): nhóm/tên của các ô còn lại do nhìn ảnh gán thủ công trong `sheet_specs.py`.", ""]
    L += ["**Bảng nhóm**:", ""]
    L += md_group_table(itm)
    L += [""]
    L += anomalies_md(itm, [
        "8 ô không xám là các khối 8x16 `obj_block_u_*` / `obj_block_bang_*` (nhóm `obj_block`): sprite phủ kín ô, viền đỏ sẫm hoặc đen.",
        "`obj_brick_set` (64,69,32,16): 4 khối gạch 8x16 dính liền không có biên xanh; giữ 1 ô, gợi ý tách 4 ô 8x16 theo x+0/8/16/24.",
        "Tên chưa chắc: `item_stopwatch` (hình giống đồng hồ, đoán), 16 ô `obj_block_*`/`obj_lean_*` (khối 'U'/'!' + hình nghiêng, chưa rõ vật thể), `obj_purple_orb`, "
        "`item_unused_0`, các `obj_score_*` (chữ số điểm chỉ là mảnh: 10/20/40/70 ghép với 0 hoặc 00).",
        "`obj_door_arch` (16x48) và `obj_door_wide` (24x48) là sprite cửa/cổng cao 48 px; có xám ở góc trên nên vẫn là ô thường.",
    ])

    # ---------------------------------------------------------------- 5. Level 1
    L += level1_md(lv1)

    # ---------------------------------------------------------------- 6. cần lưu ý
    L += ["## 6. Điểm cần lưu ý khi cắt (tóm tắt)", ""]
    L += [
        "1. Cắt theo `rect` trong JSON; xoá màu key xám (116,116,116) và xanh nền (0,128,0) trong bbox thành trong suốt; **giữ pixel đen**. "
        "Chỉ 2 ô Medusa (nền đen) và 8 khối 8x16 ở sheet items không có xám.",
        "2. Bỏ logo credit (224x32) và mọi chữ nhãn; không ô nào chạm chữ nhãn (đã kiểm 0 pixel chạm).",
        "3. Simon: ô roi ghép thân + roi (rộng 44/60 cho roi duỗi); pivot chân = [`pivot_goi_y`] (x = giữa cột chân, y = đáy slot 32 px = `baseline_y` của hàng), không phải pixel thấp nhất; "
        "Simon quay TRÁI; các ô thò roi 33-34 px cao hơn slot.",
        "4. Hai ô `enemy_dragonskull_0/1` và `obj_brick_set` là ô DÍNH LIỀN nhiều khung (có `goi_y_tach`); mọi ô khác đã tách đúng bằng khe 1 px.",
        "5. Nhiều ô Simon/boss không phải chữ nhật đặc (hợp tile 8x16): đừng cắt theo lưới đều; dùng đúng bbox từng ô.",
        "6. Một số ô có vùng xám khép kín nhỏ (<= 37 px): là lỗ trong sprite (vòng roi, quai bình...), key-out bằng màu vẫn đúng.",
        "7. Level 1: 5 vùng chữ nhật đặc, cao 192 (12 khối 16 px), rộng bội của 256; lưới 16x16/8x8 căn đúng offset (0,0) từ góc trên-trái từng vùng; "
        "khe xanh giữa vùng 5 px (ngang) / 15 px (dọc); cắt theo `rect` của `regions`.",
        "",
    ]

    L += ["## 7. Nhóm / tên không chắc (cần người xem lại nếu quan trọng)", ""]
    L += [
        "| Nội dung | Mức chắc | Lý do |", "|---|---|---|",
        "| `enemy_zombie_0..2`, `enemy_fishman_0..4`, `enemy_blackknight_0..3` (tầng trên 16x16) | trung bình | nhìn giống nửa thân/biến thể; có thể là khung độc lập |",
        "| `item_stopwatch` | thấp | hình khác đồng hồ NES quen thuộc; là ô duy nhất còn lại trong hàng vũ khí |",
        "| `obj_block_*` / `obj_lean_*` (16 ô hàng y=52) | thấp | không có nhãn, chưa rõ vật thể (4 bảng màu x 2 kiểu) |",
        "| `obj_purple_orb`, `item_unused_0`, `boss_unused_*`, `boss_dracula2_7` | thấp | hình lạ, không có nhãn rõ |",
        "| `obj_score_*` | trung bình | mảnh chữ số; cách ghép thành 100/200/.../2000 là suy luận |",
        "| `simon_use_*` ô [2..5] (4 màu vật cầm) | trung bình | chưa đối chiếu với từng cấp roi trong game |",
        "| `simon_whippart_*`: gán L2 / L3 màu | cao | đã khớp pixel với các ô roi (mảnh treo/vắt); mảnh duỗi gán theo màu và độ dài |",
        "| Liên kết vùng Level 1 (c1, c3, c5, c6) | trung bình/thấp | suy luận từ bố cục và hình vẽ; xem `connections` |",
        "",
    ]
    return L


# ------------------------------------------------------------------------------------------------
def level1_md(d):
    L = ["## 5. Level 1 — `level1_entrance` (%d×%d)" % tuple(d["size"]), ""]
    L += ["File nguồn: `%s`" % d["sheet_file"], ""]
    L += ["### 5.1 Vùng bản đồ", ""]
    L += ["**Histogram màu** (số pixel toàn sheet; vai trò): " + "; ".join(
        "`%s` %s %d = %s" % (c["hex"], rgb(c["rgb"]), c["so_pixel"], c["vai_tro"]) for c in d["bang_mau"]) + ".", ""]
    L += ["Sheet gồm **5 vùng chữ nhật ĐẶC** (diện tích = w×h, không có pixel nền xanh bên trong) + logo credit %s. "
          "Không có mảnh rời nào khác: hai \"mảnh nhỏ\" nhìn thấy ở góc dưới-trái của ảnh crop vùng Phantom Bat chính là mép phải của Crypt A và Crypt B." % d["logo_credit"]["rect"], ""]
    L += ["| Vùng (id) | Nhãn chữ | rect [x,y,w,h] | Khối 16 (cột×hàng) | Màn 256 px | Hàng sàn chính | Tile 8×8 duy nhất | Metatile 16×16 duy nhất | Ô 16×16 đồng màu |",
          "|---|---|---|---|---|---|---|---|---|"]
    for r in d["regions"]:
        L.append("| `%s` | %s | %s | %d×%d | %g | %d (y_local %d) | %d | %d | %d |" % (
            r["id"], r["label"], r["rect"], r["cols16"], r["rows16"], r["screens_256"], r["ground_row"], r["ground_row"] * 16,
            r["tiles"]["unique8"], r["tiles"]["unique16"], r["tiles"]["blocks16_uniform_color"]))
    L += ["",
          "- Chiều cao mọi vùng = **192 px = 12 khối 16 px** (không phải 176); chiều rộng 768/1536/512/512/768 đều là **bội của 16 (và của 256)**: tổng 256 khối = 16 màn.",
          "- Hàng sàn chính: hàng 11 (y_local 176) ở Entrance Hall, Crypt A, Phantom Bat; hàng 10 (y_local 160) ở Castle Entrance (hàng 11 là nền đen); hàng 6 (y_local 96) ở Crypt B (nước ở hàng 10-11).",
          "- Nhãn chữ: " + "; ".join("\"%s\" %s" % (x["text"], x["bbox"]) for x in d["labels"]) + ".",
          ""]

    L += ["### 5.2 Lưới và thống kê tile", ""]
    L += ["Thử mọi offset (0..15, 0..15) cho khối 16×16 và (0..7, 0..7) cho tile 8×8, đếm số khối duy nhất; offset đúng là offset cho ÍT khối duy nhất nhất.", ""]
    L += ["| Vùng | Offset đúng 16×16 | Duy nhất ở offset đó | Min ở offset khác | Offset đúng 8×8 | Duy nhất ở offset đó | Min ở offset khác |", "|---|---|---|---|---|---|---|"]
    for r in d["regions"]:
        g = r["grid"]
        L.append("| `%s` | %s | %d / %d | %d | %s | %d / %d | %d |" % (r["id"], tuple(g["offset16"]), g["unique16_at_best"], g["total16"], g["min_unique16_other_offsets"],
                                                                      tuple(g["offset8"]), g["unique8_at_best"], g["total8"], g["min_unique8_other_offsets"]))
    t = d["tile_stats_total"]
    L += ["",
          "**Kết luận: cả 5 vùng căn đúng lưới 16×16 và 8×8 ở offset (0,0) so với góc trên-trái vùng** (offset khác cho nhiều khối duy nhất hơn rõ rệt).", "",
          "**Toàn Level 1** (chỉ 5 vùng, đã bỏ nền xanh; khớp từng pixel, gồm cả màu):",
          "",
          "| Đại lượng | Giá trị |", "|---|---|",
          "| Tổng tile 8×8 / metatile 16×16 | %d / %d |" % (t["tiles8_total"], t["tiles16_total"]),
          "| **Tile 8×8 duy nhất** | **%d** (còn %d sau khi gộp lật ngang/dọc) |" % (t["unique8"], t["unique8_flip_merged"]),
          "| **Metatile 16×16 duy nhất** | **%d** (gộp lật vẫn %d) |" % (t["unique16"], t["unique16_flip_merged"]),
          ""]
    L += ["Theo vùng (cột \"mới\" = chưa xuất hiện ở các vùng đứng trước theo thứ tự sheet):", "",
          "| Vùng | Tổng 8×8 | Duy nhất 8×8 | Mới 8×8 | Tổng 16×16 | Duy nhất 16×16 | Mới 16×16 |", "|---|---|---|---|---|---|---|"]
    for r in d["regions"]:
        ts = r["tiles"]
        L.append("| `%s` | %d | %d | %d | %d | %d | %d |" % (r["id"], ts["tiles8_total"], ts["unique8"], ts["new8_vs_previous_regions"],
                                                         ts["tiles16_total"], ts["unique16"], ts["new16_vs_previous_regions"]))
    L += [""]

    L += ["### 5.3 Liên kết giữa các vùng (theo bố cục sheet + hình vẽ)", ""]
    L += ["Bố cục sheet: Castle Entrance → Entrance Hall → Crypt A → Phantom Bat đặt kề nhau theo hàng ngang (khe xanh 5 px), Crypt B nằm ngay dưới Crypt A (cùng x, khe 15 px).", ""]
    L += ["| Mã | Từ | Tới | Nối qua | Độ tin cậy | Ghi chú |", "|---|---|---|---|---|---|"]
    for c in d["connections"]:
        to = "`%s` (mép %s)" % (c["to_region"], EDGE_VI[c["to_edge"]]) if c["to_region"] else "(không có vùng nối tiếp)"
        extra = esc(c["ghi_chu"])
        if "x_shift_px_B_minus_A" in c:
            extra += " Dịch ngang đo được: " + ", ".join("%s = %+d px" % kv for kv in c["x_shift_px_B_minus_A"].items())
        L.append("| %s | `%s` (mép %s) | %s | %s | %s | %s |" % (c["id"], c["from_region"], EDGE_VI[c["from_edge"]], to, esc(c["via"]), esc(c["do_tin_cay"]), extra))
    L += [""]
    L += ["**Cột cửa đỏ-trắng** (viền trắng (252,252,252), cùng một hình ở 3 vùng; đứng trên một bục gạch cam):", "",
          "| Vùng | rect sheet [x,y,w,h] | Khối (cột; hàng) | Đứng trên bục |", "|---|---|---|---|"]
    for p in d["door_poles"]:
        s = p["stands_on_platform"]
        L.append("| `%s` | %s | %d; %d-%d | hàng %d, cột %d-%d |" % (p["region"], p["rect_sheet"], p["cols_rows_blocks"]["col"], p["cols_rows_blocks"]["rows"][0],
                                                                    p["cols_rows_blocks"]["rows"][1], s["row"], s["col0"], s["col1"]))
    g = d["castle_gate_arch"]
    w = d["crypt_a_left_doorway"]
    L += ["",
          "- Vòm cổng lâu đài trong tháp (Castle Entrance): rect sheet %s (cột 43, hàng 4-9) — %s." % (g["rect_sheet"], esc(g["ghi_chu"])),
          "- Khoảng đen ở mép trái Crypt A: rect sheet %s — %s." % (w["rect_sheet"], esc(w["ghi_chu"])),
          "- Dải khung cửa (chuỗi gạch cam nhỏ 6×5 xếp dọc ở sát mép): " + "; ".join("`%s` mép %s x_local %d y_local %s" % (s["region"], EDGE_VI[s["side"]], s["x_local"], s["y_local"]) for s in d["door_frame_strips"]) + ".",
          ""]

    L += ["### 5.4 Loại khối (cho planner làm va chạm; không tuyệt đối)", "",
          "Bản đồ ký tự theo metatile 16×16 (hàng = y/16, cột = x/16 trong vùng):", "",
          "| Ký hiệu | Ý nghĩa | " + " | ".join("`%s`" % r["id"] for r in d["regions"]) + " |", "|---|---|" + "---|" * len(d["regions"])]
    for ch in ["B", "F", "U", "L", "W", "K", "G", "D", "R", "."]:
        L.append("| `%s` | %s | " % (ch, esc(d["legend"][ch])) + " | ".join(str(r["block_counts"].get(ch, 0)) for r in d["regions"]) + " |")
    L += ["", "**Gạch cam (B/F, đứng được)** — khối 16×16 màu cam (200,76,12); các đoạn liền nhau theo hàng:", ""]
    byreg = OrderedDict()
    for p in d["platforms"]:
        byreg.setdefault(p["region"], []).append(p)
    for rid, ps in byreg.items():
        L.append("- `%s`: " % rid + "; ".join("hàng %d cột %d-%d (%d khối)%s" % (p["row"], p["col0"], p["col1"], p["n_blocks"], " [sàn chính]" if p["role"] == "san_chinh" else "") for p in ps))
    L += ["", "**Cầu thang** — nhận ra bằng vệt cam 8×2 lặp mỗi 8 px theo đường chéo (mỗi bậc 8×8 px; `chân`/`đỉnh` là toạ độ góc trên-trái vạch cam của bậc thấp nhất/cao nhất, theo toạ độ SHEET). "
          "↗ = đi lên sang phải (chân ở trái-dưới); ↖ = đi lên sang trái (chân ở phải-dưới):", "",
          "| Id | Hướng | Số bậc | Chân (x,y) | Đỉnh (x,y) | rect sheet [x,y,w,h] | Cao (px) | Ghi chú |", "|---|---|---|---|---|---|---|---|"]
    for s in d["stairs"]:
        L.append("| `%s` | %s | %d | (%d,%d) | (%d,%d) | %s | %d | %s |" % (s["id"], s["arrow"], s["n_steps"], s["foot_tread_sheet"][0], s["foot_tread_sheet"][1],
                                                                          s["top_tread_sheet"][0], s["top_tread_sheet"][1], s["rect_sheet"], s["height_px"], esc(s["ghi_chu"])))
    L += ["", "**Nước (chết)** — Crypt B hàng 10-11 (mặt nước từ y_local 160): " + "; ".join("rect sheet %s (%d khối)" % (w["rect_sheet"], w["n_blocks"]) for w in d["water"]) + ".", ""]
    L += ["**Mép vực** (khoảng trống trên hàng sàn chính):", ""]
    for p in d["pits_on_ground_row"]:
        L.append("- `%s` hàng %d cột %d-%d (rộng %d px, mép trái x_local %d, mép phải x_local %d): %s" % (p["region"], p["row"], p["col0"], p["col1"], p["width_px"],
                                                                                                         p["left_edge_x_local"], p["right_edge_x_local"], esc(p["ghi_chu"])))
    L += ["", "**Trang trí / khác**:", ""]
    L += ["- Tường gạch cam dạng lưới (K, có thể là tường phá được; sheet không có nhãn): " + "; ".join("`%s` rect sheet %s (%d khối, có 3 khe đen)" % (k["region"], k["rect_sheet"], k["n_blocks"]) for k in d["brick_walls_K"]) + ".",
          "- Song/lưới sắt (G, trang trí, ở sau nước): " + "; ".join("rect sheet %s" % k["rect_sheet"] for k in d["iron_grates_G"]) + ".",
          "- Lan can/rào lưới (R, trang trí): %d cụm, nhận bằng so khớp khối mẫu (rào sân ở hàng 9 Castle Entrance; lan can trên bục/cầu thang ở Hall, Crypt A, Phantom Bat). "
          "Lan can chạy dọc cầu thang nằm trong các khối U/L." % len(d["rails_R"]),
          "- Không thấy gai/hố lửa; rêu/cỏ gai xanh ở Crypt B (hàng 7 và 9) là trang trí; trụ đá, rèm đỏ, cửa sổ xanh, mảng gạch xám là nền.", ""]

    L += ["**Bản đồ ký tự** (mỗi dòng = 1 hàng khối; B=gạch cam, F=sàn sân, U/L=bậc thang, W=nước, K=tường gạch, G=song sắt, D=cột cửa, R=lan can, .=nền):", ""]
    for r in d["regions"]:
        L.append("`%s` (%d cột):" % (r["id"], r["cols16"]))
        L.append("")
        L.append("```")
        for i, line in enumerate(d["block_maps"][r["id"]]):
            L.append("%2d %s" % (i, line))
        L.append("```")
        L.append("")
    L += ["Ảnh `level1_entrance_annot.png` (1×) tô loại khối; `level1_entrance_annot_1..4.png` (2×) có lưới 16 px, chỉ số cột/hàng, ký tự loại khối, đường nối các bậc thang và khung nhãn.", ""]
    return L


def main():
    L = build()
    path = os.path.join(OUT, "SHEETS_INVENTORY.md")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(L) + "\n")
    print("wrote", os.path.basename(path), "lines:", len(L))


if __name__ == "__main__":
    main()
