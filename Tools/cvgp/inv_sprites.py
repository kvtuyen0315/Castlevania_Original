# -*- coding: utf-8 -*-
"""
inv_sprites.py - KIỂM KÊ Ô SPRITE + NHÃN cho 4 sprite sheet (Simon, Enemies, Bosses, Items & Objects).

Cách chạy (Git Bash / PowerShell), KHÔNG dùng -I:
    python -P -E inv_sprites.py                 # chạy cả 4 sheet
    python -P -E inv_sprites.py enemies bosses  # chỉ chạy một số sheet (key: simon_belmont enemies bosses items_objects)

Đầu vào : Art_Source/spriters-resource/{simon_belmont,enemies,bosses,items_objects}.png  (chỉ đọc)
Đầu ra  : ..\\<sheet_key>.json   và   ..\\<sheet_key>_annot.png   (thư mục cha của tools/ = inventory/)

Thuật toán (mô tả ngắn)
  1. Đo histogram màu; nền sheet = (0,128,0); ô key = (116,116,116); chữ nhãn = (255,255,255).
  2. "Ô sprite"  = thành phần liên thông (8 láng giềng) của pixel KHÁC nền sheet và KHÁC chữ trắng. Logo credit
     (khung 224x32 chứa mọi pixel (0,64,0)) và các mảnh chữ trong logo bị loại. Đã kiểm: liên thông 4 và 8 cho
     cùng kết quả ngoài logo => không có ô nào chỉ dính nhau ở góc chéo.
  3. "Nhãn chữ" = thành phần chữ trắng gộp thành TỪ (khoảng cách <= 3 px). Việc ghép từ -> nhãn và nhãn -> nhóm
     được khai báo trong sheet_specs.py (chép tay từ ảnh, script kiểm tra mọi từ đều được dùng đúng 1 lần).
  4. Ô -> nhóm: nhãn nhóm gần nhất PHÍA TRÊN (đáy nhãn <= đỉnh ô; "gần" tính theo đỉnh dòng nhãn cuối để không
     bị lệch bởi chữ có đuôi g/p/y) và nằm bên TRÁI (x0 nhãn <= x0 ô + 2); hoà thì lấy nhãn có x0 lớn nhất. Sheet không nhãn (items) dùng bảng toạ độ thủ công trong spec.
  5. Với Simon: baseline_y của hàng = đáy ô phổ biến nhất của hàng (slot cao 32); chan_x = đoạn cột rộng đúng 16 px mà ô
     phủ đủ 16 hàng cuối của slot ("cột chân", duy nhất cho mọi ô trừ ô nằm ngang); pivot_goi_y = [chan_x[0]+8, baseline_rel].
Lưu ý: đây là script kiểm kê; không sửa file nào trong dự án Unity.
"""
import os
import sys
from collections import Counter

import numpy as np
from PIL import Image, ImageDraw

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sheetlib as S          # noqa: E402
import sheet_specs as SP      # noqa: E402

ALL_KEYS = ["simon_belmont", "enemies", "bosses", "items_objects"]
ANNOT_SCALE = {"simon_belmont": 3, "enemies": 3, "bosses": 3, "items_objects": 5}   # hệ số phóng ảnh *_annot.png
ROW_TOL = 8                    # dung sai gom hàng theo y0
BASELINE_ROWS = {"simon_belmont": list(range(0, 8))}   # các hàng Simon có slot 32 px -> tính baseline


def col_runs(v):
    """Các đoạn liên tiếp (start, end_exclusive) của giá trị True trong mảng bool 1 chiều."""
    out, start = [], None
    for i, x in enumerate(list(v) + [False]):
        if x and start is None:
            start = i
        if (not x) and start is not None:
            out.append((start, i))
            start = None
    return out


def build_labels(spec_labels, words):
    """Ghép các từ đo được thành nhãn theo khai báo; kiểm tra mọi từ được dùng đúng 1 lần."""
    used = set()
    out = []
    for L in spec_labels:
        boxes = []
        for (x, y, n) in L["parts"]:
            cand = [i for i, w in enumerate(words) if w[0] == x and w[1] == y and i not in used]
            if not cand:
                raise SystemExit("LOI spec: khong tim thay tu tai (%d,%d) cho nhan %r" % (x, y, L["text"]))
            i0 = cand[0]
            used.add(i0)
            part = [words[i0]]
            while len(part) < n:
                last = part[-1]
                nxt = [i for i, w in enumerate(words)
                       if i not in used and abs(w[1] - y) <= 1 and w[0] >= last[2]]
                if not nxt:
                    raise SystemExit("LOI spec: nhan %r thieu tu tiep theo sau (%d,%d)" % (L["text"], last[0], last[1]))
                j = min(nxt, key=lambda i: words[i][0])
                used.add(j)
                part.append(words[j])
            boxes.extend(part)
        bb = S.union_boxes(boxes)
        rec = dict(L)
        rec["bbox"] = S.bbox_xywh(bb)
        rec["_bb"] = bb
        rec["_ref_y"] = max(b[1] for b in boxes)   # đỉnh của dòng nhãn CUỐI (không phụ thuộc chữ có đuôi g/p/y)
        rec["words"] = [S.bbox_xywh(b) for b in boxes]
        out.append(rec)
    if len(used) != len(words):
        miss = [S.bbox_xywh(w) for i, w in enumerate(words) if i not in used]
        raise SystemExit("LOI spec: con tu chua duoc gan nhan: %r" % (miss,))
    return out


def full_label_text(L):
    return (L["parent"] + " " + L["text"]) if L.get("parent") else L["text"]


def nearest_label(cell, group_labels):
    """Nhãn nhóm gần nhất phía trên-trái của ô."""
    cands = []
    for L in group_labels:
        bx0, by0, bx1, by1 = L["_bb"]
        if by1 <= cell["y"] + 0 and bx0 <= cell["x"] + 2:
            cands.append(L)
    if not cands:
        return None
    # gần nhất theo đỉnh dòng nhãn cuối (lớn nhất), hoà thì x0 lớn nhất (nhãn sát bên trái nhất)
    return max(cands, key=lambda L: (L["_ref_y"], L["_bb"][0]))


def analyze(key):
    spec = SP.SPECS[key]
    a = S.load_rgb(S.sheet_path(key))
    H, W = a.shape[:2]
    bgm = S.color_mask(a, S.BG)
    keym = S.color_mask(a, S.KEY)
    txtm = S.color_mask(a, S.TEXT)
    hist = S.color_hist(a)

    # ---- thành phần liên thông của pixel sprite --------------------------------------
    spr = ~bgm & ~txtm
    lab, n = S.label_components(spr, 8)
    st = S.comp_stats(lab, n)

    # ---- logo credit -----------------------------------------------------------------
    cd = S.color_mask(a, S.CREDIT_DARK)
    ys, xs = np.nonzero(cd)
    cdb = (int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1)
    logo_c = [s for s in st if s["x"] <= cdb[0] and s["y"] <= cdb[1]
              and s["x"] + s["w"] >= cdb[2] and s["y"] + s["h"] >= cdb[3]]
    logo = max(logo_c, key=lambda s: s["w"] * s["h"])
    lx0, ly0, lx1, ly1 = logo["x"], logo["y"], logo["x"] + logo["w"], logo["y"] + logo["h"]

    def inside_logo(s):
        return s["x"] >= lx0 and s["y"] >= ly0 and s["x"] + s["w"] <= lx1 and s["y"] + s["h"] <= ly1

    cells = [s for s in st if not inside_logo(s)]
    n_logo_parts = len(st) - len(cells)

    # ---- chữ nhãn ----------------------------------------------------------------------
    words = S.words_from_text_mask(txtm, 3)
    labels = build_labels(spec["labels"], words)
    group_labels = [L for L in labels if L["kind"] == "G"]

    # ---- thông số từng ô -----------------------------------------------------------------
    recs = []
    for c in cells:
        x, y, w, h = c["x"], c["y"], c["w"], c["h"]
        m = lab[y:y + h, x:x + w] == c["id"]
        sub = a[y:y + h, x:x + w]
        key_in = keym[y:y + h, x:x + w] & m
        has_key = bool(key_in.any())
        ink = m & ~key_in
        pix = sub[ink]
        cols = S.color_hist(pix.reshape(-1, 1, 3)) if len(pix) else Counter()
        iy, ix = np.nonzero(ink)
        ink_box = [int(ix.min()), int(iy.min()), int(ix.max() - ix.min() + 1), int(iy.max() - iy.min() + 1)] if len(ix) else None
        rec = dict(
            x=x, y=y, w=w, h=h, _cid=c["id"],
            area=int(m.sum()), fill=round(float(m.sum()) / (w * h), 3),
            has_key=has_key,
            so_mau=len(cols),
            mau=[S.hex_color(k) for k, _ in cols.most_common()],
            co_den_that=bool(S.BLACK in cols),
            ink=ink_box,
        )
        if not has_key:
            ring = np.concatenate([sub[0, :], sub[-1, :], sub[:, 0], sub[:, -1]])
            rc = S.color_hist(ring.reshape(-1, 1, 3)).most_common(1)[0][0]
            rec["vien_o"] = S.hex_color(rc)
        # vùng xám KHÉP KÍN (không nối tới mép ô qua pixel xám, 4 láng giềng): lỗ trong sprite hoặc xám thật
        if has_key:
            klab, kn = S.label_components(key_in, 4)
            mp = np.pad(m, 1, constant_values=False)       # viền ngoài bbox coi như "ngoài ô"
            enclosed = 0
            for st_k in S.comp_stats(klab, kn):
                comp = klab == st_k["id"]
                cp = np.pad(comp, 1, constant_values=False)
                touch = False
                for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                    sh = np.roll(cp, (dy, dx), axis=(0, 1))     # vị trí láng giềng của comp theo 1 hướng
                    if (sh & ~cp & ~mp).any():                  # láng giềng nằm ngoài mask ô => comp chạm mép ô
                        touch = True
                        break
                if not touch:
                    enclosed += st_k["area"]
            rec["key_khep_kin"] = enclosed
        rec["_mask"] = m            # giữ mask để tính "cột chân" (Simon) ở bước baseline
        recs.append(rec)

    # ---- gán nhóm -----------------------------------------------------------------------
    for r in recs:
        k = (r["x"], r["y"])
        if k in spec["cell_group"]:
            r["group"] = spec["cell_group"][k]
        else:
            L = nearest_label(r, group_labels)
            if L is None:
                raise SystemExit("LOI: o %r khong co nhan nhom" % (k,))
            r["group"] = L["stem"]
    # kiểm: mọi key spec phải tồn tại
    for dct in ("cell_group", "cell_names", "cell_note", "cell_kind", "glued"):
        for k in spec[dct]:
            if not any((r["x"], r["y"]) == k for r in recs):
                raise SystemExit("LOI spec.%s: khong co o tai %r" % (dct, k))

    # ---- thứ tự đọc: hàng (theo y0) -> x ----------------------------------------------------
    rows = S.cluster_rows(recs, ROW_TOL)
    row_of = {}
    for ri, rr in enumerate(rows):
        for r in rr:
            row_of[id(r)] = ri
    for r in recs:
        r["hang"] = row_of[id(r)]

    # thứ tự nhóm: theo (hàng, x) nhỏ nhất của ô trong nhóm
    gmin = {}
    for r in recs:
        kk = (r["hang"], r["x"], r["y"])
        if r["group"] not in gmin or kk < gmin[r["group"]]:
            gmin[r["group"]] = kk
    group_order = sorted(gmin, key=lambda g: gmin[g])

    # id trong nhóm
    for g in group_order:
        mem = [r for r in recs if r["group"] == g]
        sub_rows = S.cluster_rows(mem, ROW_TOL)
        ordered = [r for rr in sub_rows for r in rr]
        names = spec["names"].get(g)
        if names is not None and len(names) != len(ordered):
            raise SystemExit("LOI spec.names[%s]: %d ten nhung nhom co %d o" % (g, len(names), len(ordered)))
        for i, r in enumerate(ordered):
            r["idx"] = i
            k = (r["x"], r["y"])
            if k in spec["cell_names"]:
                r["id"] = spec["cell_names"][k]
            elif names is not None:
                r["id"] = names[i]
            else:
                r["id"] = "%s_%d" % (g, i)
    ids = [r["id"] for r in recs]
    dup = [k for k, v in Counter(ids).items() if v > 1]
    if dup:
        raise SystemExit("LOI: id trung %r" % (dup,))

    # sắp xếp danh sách ô theo (thứ tự nhóm, idx) và đánh số thứ tự (stt) bắt đầu từ 1
    gi = {g: i for i, g in enumerate(group_order)}
    recs.sort(key=lambda r: (gi[r["group"]], r["idx"]))
    for i, r in enumerate(recs):
        r["stt"] = i + 1

    # ---- baseline (Simon) -----------------------------------------------------------------
    row_info = []
    base_rows = set(BASELINE_ROWS.get(key, []))
    for ri, rr in enumerate(rows):
        info = dict(id=ri, y0=min(r["y"] for r in rr), y1=max(r["y"] + r["h"] for r in rr), so_o=len(rr))
        if ri in base_rows:
            bottoms = Counter(r["y"] + r["h"] for r in rr if r["h"] >= 32)
            base, cnt = bottoms.most_common(1)[0]
            info["baseline_y"] = base
            info["slot_top_y"] = base - 32
            off = [r["id"] for r in rr if (r["y"] + r["h"]) != base]
            info["o_lech_day"] = off
            info["ghi_chu"] = ("slot Simon cao 32 px, baseline_y = đáy slot (y+32 của đỉnh slot) dùng làm pivot chân; "
                               "%d/%d ô có đáy đúng baseline" % (cnt, len(rr)))
            for r in rr:
                r["baseline_rel"] = base - r["y"]
                # "cột chân" = các cột mà ô phủ ĐỦ 16 hàng cuối của slot (16 hàng ngay trên baseline); lấy đoạn rộng đúng 16 px.
                # Hai tile chân (8x16 x2) của Simon luôn nằm ở đây; mảnh roi/tay hẹp hơn nên bị loại. Xác định duy nhất
                # cho mọi ô Simon trừ ô nằm ngang (không có tile chân).
                lo = max(0, base - 16 - r["y"])
                hi = min(base - r["y"], r["h"])
                band = r["_mask"][lo:hi]
                if band.shape[0] == 16:
                    runs16 = [q for q in col_runs(band.all(axis=0)) if q[1] - q[0] == 16]
                    if len(runs16) == 1:
                        fx0, fx1 = runs16[0]
                        r["chan_x"] = [fx0, fx1]
                        r["pivot_goi_y"] = [fx0 + 8, base - r["y"]]
                        if r["w"] > 16:
                            # ô ghép cột chân + phần phụ (roi/tay/vũ khí) dính liền, không có biên xanh: gợi ý tách theo cột
                            parts = []
                            if fx0 > 0:
                                parts.append(dict(ten="phu_trai", rect=[0, 0, fx0, r["h"]]))
                            parts.append(dict(ten="cot_chan", rect=[fx0, 0, fx1 - fx0, r["h"]]))
                            if fx1 < r["w"]:
                                parts.append(dict(ten="phu_phai", rect=[fx1, 0, r["w"] - fx1, r["h"]]))
                            r["phan"] = parts
        row_info.append(info)

    # ---- ghi chú tự động + ghi chú tay ----------------------------------------------------------
    anomalies = []
    for r in recs:
        k = (r["x"], r["y"])
        notes = []
        if not r["has_key"]:
            notes.append("ô KHÔNG có pixel key xám: toàn bộ chữ nhật là pixel đặc, không có phần trong suốt (viền ô %s%s)"
                         % (r.get("vien_o", "?"), ", nền đen thật" if r.get("vien_o") == "#000000" else ""))
            anomalies.append(dict(loai="o_khong_xam", id=r["id"], rect=[r["x"], r["y"], r["w"], r["h"]],
                                  ghi_chu="không có pixel xám key: cắt nguyên chữ nhật, KHÔNG key-out màu nào; viền ô %s" % r.get("vien_o", "?")))
        if r["fill"] < 0.999:
            notes.append("không phải chữ nhật đặc (fill=%.2f): ô là hợp các tile 8x16, vùng trống trong bbox là nền xanh" % r["fill"])
        if "baseline_rel" in r:
            base = r["y"] + r["baseline_rel"]
            top = base - 32
            if r["y"] < top:
                notes.append("thò %d px phía TRÊN slot 32" % (top - r["y"]))
            if r["y"] + r["h"] > base:
                notes.append("thò %d px phía DƯỚI baseline (roi treo thấp hơn chân)" % (r["y"] + r["h"] - base))
            if r["y"] + r["h"] < base:
                notes.append("đáy ô cao hơn baseline %d px (ô đặt ở nửa trên của hàng)" % (base - r["y"] - r["h"]))
        if "phan" in r:
            notes.append("ô ghép dính liền: cột chân 16 px tại x=%d..%d + phần phụ (roi/tay/vũ khí) không có biên xanh giữa chúng; gợi ý tách ở trường phan"
                         % (r["chan_x"][0], r["chan_x"][1]))
        if k in spec["glued"]:
            sp = spec["glued"][k]
            notes.append("DÍNH LIỀN: %d khung không có biên xanh, giữ 1 ô; gợi ý tách (toạ độ tương đối) %s" % (len(sp), sp))
            r["goi_y_tach"] = sp
            anomalies.append(dict(loai="o_dinh_lien", id=r["id"], rect=[r["x"], r["y"], r["w"], r["h"]],
                                  goi_y_tach=sp, ghi_chu="nhiều khung dính liền không có biên xanh"))
        if k in spec["cell_note"]:
            notes.append(spec["cell_note"][k])
        r["loai"] = spec["cell_kind"].get(k) or spec["group_kind"].get(r["group"], "frame")
        r["ghi_chu"] = "; ".join(notes)

    # ---- nhóm ------------------------------------------------------------------------------------
    lab_by_stem = {L["stem"]: L for L in group_labels}
    groups = []
    for g in group_order:
        mem = [r for r in recs if r["group"] == g]
        sizes = Counter("%dx%d" % (r["w"], r["h"]) for r in mem)
        L = lab_by_stem.get(g)
        groups.append(dict(
            id=g,
            nhan=full_label_text(L) if L else "(không có nhãn chữ - gán thủ công theo toạ độ)",
            nhan_bbox=L["bbox"] if L else None,
            so_o=len(mem),
            stt=[mem[0]["stt"], mem[-1]["stt"]],
            kich_thuoc_dien_hinh=sizes.most_common(1)[0][0],
            kich_thuoc=dict(sizes),
            hang=sorted(set(r["hang"] for r in mem)),
            so_o_khong_dac=sum(1 for r in mem if r["fill"] < 0.999),
            ghi_chu=spec["group_note"].get(g, ""),
        ))
    # khoảng cách giữa pixel sprite thấp nhất và đáy ô (chỉ các hàng có baseline = Simon)
    for gdict in groups:
        mem = [r for r in recs if r["group"] == gdict["id"] and "baseline_rel" in r and r["ink"]]
        if mem:
            gaps = Counter(r["h"] - (r["ink"][1] + r["ink"][3]) for r in mem)
            gdict["chan_cach_day_o"] = {str(k): v for k, v in sorted(gaps.items())}
    # nhóm trong spec nhưng không có ô?
    for L in group_labels:
        if L["stem"] not in group_order:
            raise SystemExit("LOI: nhan nhom %r khong co o nao" % L["stem"])

    # ---- bảng màu --------------------------------------------------------------------------------
    cell_mask = np.zeros((H, W), dtype=bool)
    for r in recs:
        cell_mask[r["y"]:r["y"] + r["h"], r["x"]:r["x"] + r["w"]] |= (lab[r["y"]:r["y"] + r["h"], r["x"]:r["x"] + r["w"]] == r["_cid"])
    logo_mask = np.zeros((H, W), dtype=bool)
    logo_mask[ly0:ly1, lx0:lx1] = True
    bang_mau = []
    odd = []
    for col, cnt in hist.most_common():
        cm = S.color_mask(a, col)
        in_cells = int((cm & cell_mask).sum())
        in_logo = int((cm & logo_mask & ~cell_mask).sum())
        in_text = int((cm & txtm).sum()) if col == S.TEXT else 0
        if col == S.BG:
            role = "nền sheet"
        elif col == S.KEY:
            role = "màu key (nền ô xám)"
        elif col == S.TEXT:
            role = "chữ nhãn"
        elif in_cells == 0:
            role = "chỉ có trong logo credit"
        else:
            role = "sprite"
        bang_mau.append(dict(rgb=list(col), hex=S.hex_color(col), so_pixel=int(cnt), trong_o=in_cells,
                             trong_logo=in_logo, vai_tro=role))
        if role == "chỉ có trong logo credit":
            odd.append(dict(loai="mau_la", rgb=list(col), so_pixel=int(cnt),
                            ghi_chu="chỉ nằm trong logo credit (bóng chữ), không phải màu sprite"))
        if col != S.TEXT and col != S.BG and col != S.KEY and max(abs(col[i] - S.TEXT[i]) for i in range(3)) <= 6 and in_cells:
            odd.append(dict(loai="mau_la", rgb=list(col), so_pixel=int(cnt),
                            ghi_chu="trắng của sprite (252,252,252) KHÁC trắng chữ nhãn (255,255,255): "
                                    "không bị tách nhầm vào nhãn (đã kiểm: chữ nhãn không chạm sprite)"))
    # chữ nhãn có chạm sprite không?
    sp_pad = np.pad(spr, 1)
    adj = np.zeros_like(txtm)
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            if dy or dx:
                adj |= sp_pad[1 + dy:1 + dy + H, 1 + dx:1 + dx + W]
    touch = int((txtm & adj).sum())
    anomalies.append(dict(loai="kiem_tra", ghi_chu="pixel chữ trắng chạm pixel sprite: %d (0 = nhãn không dính ô nào)" % touch))
    anomalies.extend(odd)
    anomalies.append(dict(loai="logo_credit", rect=[lx0, ly0, logo["w"], logo["h"]],
                          ghi_chu="khung credit 'Sprites ripped by Mister Mike' 224x32 + %d mảnh chữ bên trong: BỎ QUA khi cắt" % (n_logo_parts - 1)))
    # tóm tắt ô không đặc theo nhóm
    nd = {g["id"]: g["so_o_khong_dac"] for g in groups if g["so_o_khong_dac"]}
    if nd:
        anomalies.append(dict(loai="o_khong_dac", theo_nhom=nd,
                              ghi_chu="ô là hợp tile 8x16 (fill<1): bbox đúng nhưng có góc trống màu xanh; khi cắt giữ nguyên bbox, "
                                      "coi pixel xanh/xám là trong suốt"))
    # ô cao bất thường (Simon): thò khỏi slot 32
    for r in recs:
        if "baseline_rel" in r and (r["h"] > 32):
            anomalies.append(dict(loai="o_cao_hon_slot", id=r["id"], rect=[r["x"], r["y"], r["w"], r["h"]],
                                  ghi_chu=r["ghi_chu"]))

    enc = [r for r in recs if r.get("key_khep_kin")]
    if enc:
        anomalies.append(dict(loai="o_co_xam_khep_kin", so_o=len(enc), tong_px=int(sum(r["key_khep_kin"] for r in enc)),
                              vi_du=[r["id"] for r in enc[:6]],
                              ghi_chu="ô có vùng xám key KHÉP KÍN (không nối ra mép ô): lỗ trong sprite hoặc xám thật. "
                                      "Nếu key-out bằng màu thì lỗ cũng trong suốt; nếu muốn giữ xám thật phải flood-fill từ mép. "
                                      "Chi tiết ở trường key_khep_kin (số pixel) của từng ô."))
    else:
        anomalies.append(dict(loai="o_co_xam_khep_kin", so_o=0, ghi_chu="không ô nào có vùng xám key khép kín: key-out bằng màu là an toàn"))

    # ---- dựng JSON ---------------------------------------------------------------------------------
    out_cells = []
    for r in recs:
        d = dict(stt=r["stt"], id=r["id"], group=r["group"], idx=r["idx"], rect=[r["x"], r["y"], r["w"], r["h"]],
                 so_mau=r["so_mau"], co_den_that=r["co_den_that"], loai=r["loai"], hang=r["hang"],
                 fill=r["fill"], nen_o="xam" if r["has_key"] else "khong_xam", ink=r["ink"], mau=r["mau"])
        if "vien_o" in r:
            d["vien_o"] = r["vien_o"]
        for kk in ("baseline_rel", "chan_x", "pivot_goi_y", "phan", "goi_y_tach"):
            if kk in r:
                d[kk] = r[kk]
        if r.get("key_khep_kin"):
            d["key_khep_kin"] = r["key_khep_kin"]
        d["ghi_chu"] = r["ghi_chu"]
        out_cells.append(d)
    out_labels = []
    for L in labels:
        out_labels.append(dict(text=L["text"], bbox=L["bbox"], loai="tieu_de" if L["kind"] == "H" else "nhom",
                               stem=L["stem"], parent=L["parent"], words=L["words"]))
    result = dict(
        sheet_file=S.ORIGINAL_NAMES[key],
        sheet_key=key,
        size=[W, H],
        bg_color=list(S.BG),
        key_color=list(S.KEY),
        text_color=list(S.TEXT),
        ghi_chu_dinh_nghia=("ô = thành phần liên thông (8 láng giềng) của pixel khác nền sheet (0,128,0) và khác chữ trắng; "
                            "rect=[x,y,w,h] theo pixel gốc, gốc toạ độ ở góc trên-trái sheet; "
                            "so_mau = số màu khác nhau trong ô, KHÔNG tính màu key xám; co_den_that = có pixel đen (0,0,0) trong ô "
                            "(đen là màu thật, không trong suốt); fill = diện tích thành phần / (w*h) (<1: ô là hợp tile, vùng trống trong bbox là nền xanh); "
                            "ink = bbox pixel sprite (khác key) tương đối so với góc trên-trái ô; stt = số thứ tự (bắt đầu 1) vẽ trên ảnh *_annot.png; "
                            "hang = chỉ số hàng ô (gom theo y0, dung sai 8 px)"),
        quy_uoc_cat=["pixel xám (116,116,116) trong ô = màu key -> trong suốt khi cắt",
                     "pixel xanh nền sheet (0,128,0) nằm trong bbox của ô không đặc (fill<1) -> trong suốt",
                     "pixel đen (0,0,0) là màu thật của sprite -> KHÔNG trong suốt",
                     "bỏ qua logo credit (xem logo_credit) và mọi chữ nhãn trắng"],
        logo_credit=dict(rect=[lx0, ly0, logo["w"], logo["h"]], so_manh_chu_trong_logo=n_logo_parts - 1),
        bang_mau=bang_mau,
        labels=out_labels,
        groups=groups,
        rows=row_info,
        cells=out_cells,
        bat_thuong=anomalies,
        tong_ket=dict(so_nhan=len(labels), so_nhan_nhom=len(group_labels), so_nhom=len(groups), so_o=len(recs),
                      so_o_khong_xam=sum(1 for r in recs if not r["has_key"]),
                      so_o_khong_dac=sum(1 for r in recs if r["fill"] < 0.999),
                      so_o_dinh_lien=len(spec["glued"])),
    )
    S.dump_json_compact(os.path.join(S.OUT_DIR, key + ".json"), result,
                        one_line_keys=("bang_mau", "labels", "groups", "rows", "cells", "bat_thuong"))
    annotate(key, a, recs, labels, group_order, scale=ANNOT_SCALE.get(key, 3))
    return result


def annotate(key, a, recs, labels, group_order, scale=3):
    H, W = a.shape[:2]
    base = Image.fromarray(a).resize((W * scale, H * scale), Image.NEAREST).convert("RGB")
    d = ImageDraw.Draw(base)
    font = S.get_font(11 if scale <= 3 else 14)
    gi = {g: i for i, g in enumerate(group_order)}
    for L in labels:
        x0, y0, x1, y1 = L["_bb"]
        d.rectangle([x0 * scale - 1, y0 * scale - 1, x1 * scale, y1 * scale],
                    outline=(255, 255, 0) if L["kind"] == "G" else (255, 140, 0))
    for r in recs:
        col = S.hsv_color(gi[r["group"]], len(group_order))
        x0, y0 = r["x"] * scale, r["y"] * scale
        x1, y1 = (r["x"] + r["w"]) * scale - 1, (r["y"] + r["h"]) * scale - 1
        d.rectangle([x0, y0, x1, y1], outline=col)
        txt = str(r["stt"])
        tw = d.textlength(txt, font=font)
        if tw + 2 <= (x1 - x0) and (y1 - y0) >= 14:
            S.draw_text_outlined(d, (x0 + 2, y0 + 1), txt, col, font)
        else:
            S.draw_text_outlined(d, (x0, max(0, y0 - 13)), txt, col, font)
    base.save(os.path.join(S.OUT_DIR, key + "_annot.png"))


def main(argv):
    keys = argv[1:] or ALL_KEYS
    S.ensure_dir(S.OUT_DIR)
    all_ids = {}
    for k in keys:
        res = analyze(k)
        t = res["tong_ket"]
        print("%-14s labels=%d (nhom %d) groups=%d cells=%d no_gray=%d non_rect=%d glued=%d"
              % (k, t["so_nhan"], t["so_nhan_nhom"], t["so_nhom"], t["so_o"], t["so_o_khong_xam"],
                 t["so_o_khong_dac"], t["so_o_dinh_lien"]))
        for c in res["cells"]:
            if c["id"] in all_ids:
                raise SystemExit("LOI: id %s trung giua %s va %s" % (c["id"], all_ids[c["id"]], k))
            all_ids[c["id"]] = k
    print("OK: %d id duy nhat" % len(all_ids))


if __name__ == "__main__":
    main(sys.argv)
