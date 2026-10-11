# -*- coding: utf-8 -*-
"""
sheet_specs.py - DỮ LIỆU gán nhãn thủ công (đã NHÌN ẢNH kiểm lại) cho 4 sprite sheet:
  simon_belmont, enemies, bosses, items_objects.

Không chạy trực tiếp; inv_sprites.py import module này.

Cấu trúc mỗi spec (dict):
  labels      : list nhãn chữ. Mỗi nhãn = dict(text, parts=[(x,y,n_từ),...], kind, stem, parent)
                  - (x,y) là góc trên-trái CHÍNH XÁC của từ đầu tiên (đo tự động bằng gộp ký tự, khoảng cách <= 3 px)
                  - n_từ = số từ nối tiếp bên phải trên cùng dòng thuộc nhãn đó (nhãn nhiều dòng = nhiều part)
                  - kind: "H" = nhãn tiêu đề (không gắn ô trực tiếp), "G" = nhãn nhóm (gắn ô)
                  - stem: tiền tố id của nhóm (chỉ với kind "G")
                  - parent: text của nhãn tiêu đề H đứng trên nó (để ghép nhãn đầy đủ)
  names       : {stem: [tên từng ô theo thứ tự đọc]}  - ghi đè hậu tố số _0,_1,...
  group_note  : {stem: ghi chú nhóm}                    - chép vào ghi_chu của mọi ô trong nhóm
  group_kind  : {stem: loại}                            - loại mặc định của ô (frame/manh/dan/hieu_ung/item/vat_the)
  cell_group  : {(x,y): stem}                           - gán nhóm thủ công (sheet không có nhãn: items)
  cell_names  : {(x,y): id}                             - đặt id thủ công theo toạ độ góc trên-trái
  cell_note   : {(x,y): ghi chú riêng}
  cell_kind   : {(x,y): loại riêng}
  glued       : {(x,y): [[dx,dy,w,h],...]}              - ô DÍNH LIỀN (nhiều khung không có biên xanh): gợi ý tách

Mọi toạ độ đều theo pixel gốc của sheet (không phóng to).
"""

LOAI_GHI_CHU = {
    "frame": "khung hoạt hình đầy đủ",
    "manh": "mảnh/biến thể cần ghép (không phải khung độc lập)",
    "dan": "đạn / vũ khí ném / vật phóng ra",
    "hieu_ung": "hiệu ứng",
    "item": "vật phẩm nhặt",
    "vat_the": "vật thể trong màn (nến, đuốc, cửa, gạch ...)",
}


def _mk():
    L = []

    def H(text, *parts):
        L.append(dict(text=text, parts=list(parts), kind="H", stem=None, parent=None))

    def G(text, stem, *parts, parent=None):
        L.append(dict(text=text, parts=list(parts), kind="G", stem=stem, parent=parent))

    return L, H, G


# ======================================================================================
# 1) SIMON BELMONT  (799 x 529)
# ======================================================================================
_L, H, G = _mk()
# --- hàng 0 -------------------------------------------------------------------------
H("Climb Stairs", (154, 1, 2))
G("Idle", "simon_idle", (1, 11, 1))
G("Walk", "simon_walk", (29, 11, 1))
G("Jump/Duck", "simon_jumpduck", (84, 11, 1))
G("(Ascend)", "simon_stairs_up", (154, 11, 1), parent="Climb Stairs")
G("(Descend)", "simon_stairs_down", (211, 11, 1), parent="Climb Stairs")
G("Hurt", "simon_hurt", (275, 11, 1))
G("Death", "simon_death", (308, 11, 1))
G("Intro", "simon_intro", (362, 11, 1))
G("Colors", "simon_color", (406, 11, 1))
# --- hàng 1: Whip/Use Subweapon ------------------------------------------------------
H("Whip/Use Subweapon", (1, 58, 2))
G("(Standing)", "simon_use_stand", (1, 68, 1), parent="Whip/Use Subweapon")
G("(Ducking)", "simon_use_duck", (147, 68, 1), parent="Whip/Use Subweapon")
G("(Ascending Stairs)", "simon_use_stairs_up", (293, 68, 2), parent="Whip/Use Subweapon")
G("(Descending Stairs)", "simon_use_stairs_down", (439, 68, 2), parent="Whip/Use Subweapon")
# --- hàng 2..7: Whip Level 1, Level 2, Level 3 màu 1..4 --------------------------------
for _text, _stem, _yh, _ys, _nh, _xs in [
    ("Whip (Level 1)", "simon_whip1", 115, 125, 3, (1, 178, 355, 515)),
    ("Whip (Level 2)", "simon_whip2", 172, 182, 3, (1, 178, 355, 515)),
    ("Whip (Level 3, Color 1)", "simon_whip3c1", 229, 239, 5, (1, 210, 419, 611)),
    ("Whip (Level 3, Color 2)", "simon_whip3c2", 286, 296, 5, (1, 210, 419, 611)),
    ("Whip (Level 3, Color 3)", "simon_whip3c3", 343, 353, 5, (1, 210, 419, 611)),
    ("Whip (Level 3, Color 4)", "simon_whip3c4", 400, 410, 5, (1, 210, 419, 611)),
]:
    H(_text, (1, _yh, _nh))
    G("(Standing/Jumping)", _stem + "_stand", (_xs[0], _ys, 1), parent=_text)
    G("(Ducking)", _stem + "_duck", (_xs[1], _ys, 1), parent=_text)
    G("(Ascending Stairs)", _stem + "_stairs_up", (_xs[2], _ys, 2), parent=_text)
    G("(Descending Stairs)", _stem + "_stairs_down", (_xs[3], _ys, 2), parent=_text)
# --- hàng 8: roi rời, vũ khí phụ, hit effect -------------------------------------------
G("Whip", "simon_whippart", (1, 467, 1))
H("Subweapons", (354, 457, 1))
G("Dagger", "sub_dagger", (354, 467, 1), parent="Subweapons")
G("Cross/Boomerang", "sub_cross", (401, 467, 1), parent="Subweapons")
G("Axe", "sub_axe", (515, 467, 1), parent="Subweapons")
G("Holy Water/Fire Bomb", "sub_holywater", (587, 467, 3), parent="Subweapons")
G("Hit Effect", "fx_hit", (727, 467, 2))

_WHIP_NOTE_STAND = (
    "5 ô: [0] chuẩn bị (roi rủ xuống bên phải thân), [1] roi vắt qua đầu, [2][3] roi duỗi ngang "
    "(roi ở cột trái, thân Simon chiếm 16 px phải của ô; 2 ô chỉ khác phần roi), [4] thân Simon 16x32 lẻ "
    "(trùng pixel với thân trong [2],[3], đã kiểm). Simon quay mặt TRÁI."
)
_WHIP_NOTE_STAIRS = (
    "4 ô: [0] chuẩn bị, [1] roi vắt qua đầu, [2][3] roi duỗi (thân Simon ở 16 px phải); không có ô thân lẻ. "
    "Simon quay mặt TRÁI."
)

SIMON = dict(
    labels=_L,
    names={
        # hàng roi rời: theo thứ tự trái->phải
        "simon_whippart": [
            "simon_whippart_l1_hang", "simon_whippart_l1_over", "simon_whippart_l1_ext",
            "simon_whippart_l3c1_hang", "simon_whippart_l3c2_hang", "simon_whippart_l3c3_hang", "simon_whippart_l3c4_hang",
            "simon_whippart_l3c1_over", "simon_whippart_l3c2_over", "simon_whippart_l3c3_over", "simon_whippart_l3c4_over",
            "simon_whippart_l2_ext",
            "simon_whippart_l3c1_ext", "simon_whippart_l3c2_ext", "simon_whippart_l3c3_ext", "simon_whippart_l3c4_ext",
        ],
        "sub_cross": ["sub_cross_0", "sub_cross_spin_0", "sub_cross_spin_1"],
        "sub_holywater": ["sub_holywater_bottle", "sub_holywater_fire_0", "sub_holywater_fire_1",
                          "sub_holywater_fire_2", "sub_holywater_fire_3", "sub_holywater_fire_4"],
    },
    group_note={
        "simon_idle": "1 khung đứng. Simon quay mặt TRÁI trong sheet (lật ngang khi quay phải).",
        "simon_walk": "3 khung đi bộ (chu kỳ), cách nhau 1 px xanh. Simon quay mặt TRÁI.",
        "simon_jumpduck": "1 khung cúi/cuộn người (dùng chung cho nhảy và cúi). Tile dưới trống: pixel sprite chỉ tới y+24, "
                          "8 px cuối ô là xám trống.",
        "simon_stairs_up": "2 khung leo cầu thang đi lên.",
        "simon_stairs_down": "2 khung leo cầu thang đi xuống.",
        "simon_hurt": "1 khung bị thương/bật lùi.",
        "simon_death": "2 ô: [0] 16x32 (gục/cúi, 8 px cuối ô trống), [1] 32x16 nằm ngang đặt ở nửa TRÊN của hàng "
                       "(đáy thân tại y+16, không phải baseline hàng).",
        "simon_intro": "1 khung nhìn từ phía sau (cảnh intro đi vào cổng).",
        "simon_color": "5 khung cùng tư thế, 5 bảng màu: [0] mặc định (đỏ-nâu), [1] tím/đen, [2] xanh dương/đen, "
                       "[3] đỏ-hồng/đen, [4] trắng-xanh nhạt.",
        "simon_use_stand": "6 ô (hợp tile 8x16, fill 0.62-0.83): [0] chuẩn bị, [1] biến thể thu hẹp 16 rộng, "
                           "[2..5] tay duỗi với 4 biến thể màu vật cầm (không/tím/xanh dương/hồng-đỏ).",
        "simon_use_duck": "6 ô như nhóm Standing nhưng tư thế cúi (8 px cuối ô trống).",
        "simon_use_stairs_up": "6 ô như Standing, tư thế trên cầu thang đi lên.",
        "simon_use_stairs_down": "6 ô như Standing, tư thế trên cầu thang đi xuống.",
        "simon_whippart": "Mảnh roi RỜI (không có thân Simon), tổng 16 ô: L1 (hồng/trắng, viền đen): treo 8x32, vắt 16x24, duỗi 24x16; "
                          "L3 màu 1..4 (cam/tím/xanh/đỏ, dạng xích): treo 8x32 x4, vắt 16x24 x4, duỗi 40x16 x4; "
                          "L2 chỉ có mảnh duỗi 24x16 (tím). Đã khớp từng pixel: mảnh treo (8x32) nằm nguyên vẹn tại x=24 của ô [0] và "
                          "mảnh vắt (16x24) tại x=16 của ô [1] của nhóm Standing cùng màu; L2 dùng chung mảnh treo/vắt với L3 màu 2; "
                          "mảnh duỗi 24x16 tím của L2 nằm ở x=0 trong 2 ô roi duỗi của L3 màu 2.",
        "sub_dagger": "1 ô 16x16: dao găm nằm ngang.",
        "sub_cross": "3 ô 16x16: [0] thập tự (+), [1][2] hai khung xoay (dạng ×).",
        "sub_axe": "4 ô 16x16: rìu xoay 4 khung.",
        "sub_holywater": "6 ô: [0] chai (8x16), [1..5] lửa cháy lớn dần (8x16 x3, 16x16 x2).",
        "fx_hit": "1 ô 8x16: tia lóe khi đòn trúng.",
    },
    group_kind={
        "simon_whippart": "manh", "sub_dagger": "dan", "sub_cross": "dan", "sub_axe": "dan",
        "sub_holywater": "dan", "fx_hit": "hieu_ung",
    },
    cell_group={}, cell_names={}, cell_note={}, cell_kind={}, glued={},
)
# ô thân Simon 16x32 lẻ ở cuối nhóm Standing/Jumping và Ducking của 6 hàng roi (đã khớp từng pixel với thân trong ô roi duỗi)
for _xy in [(157, 135), (334, 135), (157, 192), (334, 192),
            (189, 249), (398, 249), (189, 306), (398, 306), (189, 363), (398, 363), (189, 420), (398, 420)]:
    SIMON["cell_kind"][_xy] = "manh"
    SIMON["cell_note"][_xy] = "thân Simon 16x32 lẻ, trùng pixel với thân trong 2 ô roi duỗi cùng nhóm (không kèm roi)"
# gắn ghi chú nhóm cho 6 bảng roi
for _stem in ["simon_whip1", "simon_whip2", "simon_whip3c1", "simon_whip3c2", "simon_whip3c3", "simon_whip3c4"]:
    SIMON["group_note"][_stem + "_stand"] = _WHIP_NOTE_STAND
    SIMON["group_note"][_stem + "_duck"] = _WHIP_NOTE_STAND + " Tư thế cúi: 8 px cuối ô trống."
    SIMON["group_note"][_stem + "_stairs_up"] = _WHIP_NOTE_STAIRS + " Trên cầu thang đi lên."
    SIMON["group_note"][_stem + "_stairs_down"] = _WHIP_NOTE_STAIRS + " Trên cầu thang đi xuống."
for _stem in ["simon_whip3c1", "simon_whip3c2", "simon_whip3c3", "simon_whip3c4"]:
    for _suf in ["_stand", "_duck", "_stairs_up", "_stairs_down"]:
        SIMON["group_note"][_stem + _suf] += " Level 3: roi dài hơn, ô duỗi rộng 60 px."

# ======================================================================================
# 2) ENEMIES  (474 x 190)
# ======================================================================================
_L, H, G = _mk()
G("Zombie", "enemy_zombie", (1, 1, 1))
G("Black Leopard", "enemy_leopard", (56, 1, 2))
G("Vampire Bat", "enemy_bat", (184, 1, 2))
G("Fish Man", "enemy_fishman", (309, 1, 2))
G("Black Knight", "enemy_blackknight", (399, 1, 2))
G("Ghost", "enemy_ghost", (1, 65, 1))
G("Dragon Skull Cannon", "enemy_dragonskull", (40, 55, 2), (40, 65, 1))
G("Medusa/Medusa Head", "enemy_medusahead", (119, 55, 1), (119, 65, 2))
G("Hunchback/Fleaman", "enemy_fleaman", (200, 55, 1), (200, 65, 1))
G("Raven", "enemy_raven", (277, 65, 1))
G("White Skeleton", "enemy_skeleton", (349, 65, 2))
G("Eagle", "enemy_eagle", (1, 112, 1))
G("Skele-Dragon", "enemy_skeledragon", (71, 112, 1))
G("Red Skeleton", "enemy_redskeleton", (157, 112, 2))
G("Axe-Man", "enemy_axeman", (240, 112, 1))
G("Death Effect", "fx_death", (362, 112, 2))

ENEMIES = dict(
    labels=_L,
    names={
        "enemy_fishman": ["enemy_fishman_0", "enemy_fishman_1", "enemy_fishman_2", "enemy_fishman_3", "enemy_fishman_4",
                          "enemy_fishman_5", "enemy_fishman_6", "enemy_fishman_7", "enemy_fishman_8",
                          "enemy_fishman_fireball", "enemy_fishman_bubble"],
        "enemy_dragonskull": ["enemy_dragonskull_0", "enemy_dragonskull_1", "enemy_dragonskull_fireball"],
        "enemy_skeledragon": ["enemy_skeledragon_0", "enemy_skeledragon_1", "enemy_skeledragon_bone",
                              "enemy_skeledragon_fireball"],
        "enemy_skeleton": ["enemy_skeleton_0", "enemy_skeleton_1", "enemy_skeleton_bone_0",
                           "enemy_skeleton_bone_1", "enemy_skeleton_bone_2"],
        "enemy_axeman": ["enemy_axeman_0", "enemy_axeman_1", "enemy_axeman_axe_0", "enemy_axeman_axe_1",
                         "enemy_axeman_axe_2", "enemy_axeman_axe_3"],
    },
    group_note={
        "enemy_zombie": "5 ô: hàng trên 3 ô 16x16 (mảnh/biến thể thân trên và thân dưới), hàng dưới 2 khung 16x32 đầy đủ "
                        "(có vẻ ghép từ các mảnh hàng trên).",
        "enemy_leopard": "4 khung báo đen: [0] 24x16 (nằm/ngồi), [1][2][3] 32x16 (chạy/nhảy).",
        "enemy_bat": "7 ô 16x16: [0..2] 3 khung (viền hồng, thân đỏ-đen), [3..6] 4 khung (bảng màu tím); khoảng cách 3 px giữa 2 bảng màu.",
        "enemy_fishman": "11 ô: hàng trên 5 ô 16x16 (mảnh/biến thể), hàng dưới 4 khung 16x32 và 2 đạn 8x16 "
                         "(cầu lửa đỏ, bong bóng tím).",
        "enemy_blackknight": "7 ô: hàng trên 4 ô 16x16 (mảnh/biến thể: có thể là phần thân/vũ khí), hàng dưới 3 khung 16x32.",
        "enemy_ghost": "2 khung 16x16.",
        "enemy_dragonskull": "3 ô: [0] tím, [1] đỏ - mỗi ô 16x32 DÍNH LIỀN 2 khung 16x16 xếp dọc (không có biên xanh; "
                             "khung trên miệng mở, khung dưới khép/khác), [2] cầu lửa 8x16.",
        "enemy_medusahead": "2 khung 16x16 (đầu Medusa xanh).",
        "enemy_fleaman": "2 khung 16x16 (người lưng gù, tông đỏ).",
        "enemy_raven": "4 khung 16x16 (quạ).",
        "enemy_skeleton": "5 ô: [0][1] 2 khung xương 16x32, [2] xương dọc 8x16, [3][4] xương xoay chéo 16x16 (đạn).",
        "enemy_eagle": "2 khung 32x32 (đại bàng vỗ cánh).",
        "enemy_skeledragon": "4 ô: [0][1] 2 khung đầu rồng 16x16, [2] đốt xương 8x16, [3] cầu lửa đỏ 8x16.",
        "enemy_redskeleton": "4 ô: [0][1] 16x16 (xương tan/đang ráp lại), [2][3] 2 khung xương 16x32 (đi).",
        "enemy_axeman": "6 ô: [0][1] 2 khung hiệp sĩ 24x32 (kèm khiên), [2..5] rìu xoay 16x16 x4.",
        "fx_death": "3 ô 8x16: giọt -> lửa nhỏ -> lửa lớn (dùng xếp thành khối theo cỡ enemy).",
    },
    group_kind={"fx_death": "hieu_ung"},
    cell_group={}, cell_names={},
    cell_note={},
    cell_kind={
        (377, 28): "dan", (386, 28): "dan", (74, 75): "dan", (114, 122): "dan", (105, 122): "manh",
        (383, 75): "dan", (392, 75): "dan", (409, 75): "dan",
        (290, 122): "dan", (307, 122): "dan", (324, 122): "dan", (341, 122): "dan",
        (1, 11): "manh", (18, 11): "manh", (35, 11): "manh",
        (309, 11): "manh", (326, 11): "manh", (343, 11): "manh", (360, 11): "manh", (377, 11): "manh",
        (399, 11): "manh", (416, 11): "manh", (433, 11): "manh", (450, 11): "manh",
    },
    glued={
        (40, 75): [[0, 0, 16, 16], [0, 16, 16, 16]],
        (57, 75): [[0, 0, 16, 16], [0, 16, 16, 16]],
    },
)

# ======================================================================================
# 3) BOSSES  (415 x 292)
# ======================================================================================
_L, H, G = _mk()
G("Phantom Bat", "boss_phantombat", (1, 1, 2))
G("Medusa", "boss_medusa", (104, 1, 1))
G("Mummy Man", "boss_mummy", (274, 1, 2))
G("Frankenstein & Igor", "boss_frankenstein", (1, 64, 3))
G("Grim Reaper/Death", "boss_reaper", (128, 64, 2))
G("Dracula (Phase 1)", "boss_dracula1", (249, 64, 3))
G("Dracula (Phase 2)/Cookie Monster", "boss_dracula2", (1, 138, 4))
G("Death Effect", "fx_bossdeath", (1, 233, 2))
G("Unused", "boss_unused", (372, 228, 1))

BOSSES = dict(
    labels=_L,
    names={},
    group_note={
        "boss_phantombat": "3 ô: [0] 16x16 (dơi gập, đen), [1] 48x32 dang cánh (không đặc, fill 0.67), [2] 32x32 (không đặc, fill 0.75).",
        "boss_medusa": "6 ô: [0][1] 32x48 NỀN ĐEN (không có xám): 2 khung Medusa (mặt+thân), [2] 32x32 đầu tóc rắn xanh, "
                       "[3] 32x32 đầu đá (xám-hồng), [4][5] 16x16 rắn xanh.",
        "boss_mummy": "8 ô: [0..5] 6 khung 16x48 (3 khung tím rồi 3 khung đỏ), [6][7] 16x16 (dải băng quấn bay ra).",
        "boss_frankenstein": "6 ô: [0..2] Frankenstein 16x48 (3 khung đi), [3][4] Igor 16x16 (tông đỏ), [5] đạn 8x16 (cầu đỏ).",
        "boss_reaper": "5 ô: [0] 40x48 Tử thần (không đặc, fill 0.73), [1..4] 16x16 lưỡi hái xoay 4 khung.",
        "boss_dracula1": "6 ô: [0] 24x48 và [1] 32x48 thân + áo choàng đỏ KHÔNG có đầu, [2] 8x16 đầu Dracula (mảnh để ghép lên thân), "
                         "[3] 8x16 cầu lửa đỏ, [4] 24x59 và [5] 32x59 thân có đầu (không đặc, fill 0.86-0.88).",
        "boss_dracula2": "9 ô: [0..6] 7 khung thân quái vật (rộng 48, cao 48-80, không đặc fill 0.73-0.89, ghép nhiều tile), "
                         "[7] 16x16 (bóng vuốt/lửa be-xanh, chưa chắc) và [8] 8x16 (cầu lửa đỏ).",
        "fx_bossdeath": "9 ô gồm 3 cỡ khối hiệu ứng: 3 ô 8x16 (đơn), 3 ô 24x32 (2 hàng tile), 3 ô 24x48 (3 hàng tile) - "
                        "mỗi cỡ 3 khung giọt -> lửa nhỏ -> lửa lớn (khối ghép sẵn nhiều tile, mỗi ô là 1 khung).",
        "boss_unused": "2 ô 16x16 không dùng trong game: bàn tay/cẳng tay be-xanh (chưa rõ thuộc nhân vật nào).",
    },
    group_kind={"fx_bossdeath": "hieu_ung", "boss_unused": "manh"},
    cell_group={}, cell_names={},
    cell_note={
        (32, 243): "khối hiệu ứng ghép 3 cột x 2 hàng tile 8x16 (có khe xám nội bộ giữa các tile); 1 ô = 1 giai đoạn (giọt)",
        (57, 243): "khối hiệu ứng ghép 3 cột x 2 hàng tile 8x16; 1 ô = 1 giai đoạn (lửa nhỏ)",
        (82, 243): "khối hiệu ứng ghép 3 cột x 2 hàng tile 8x16; 1 ô = 1 giai đoạn (lửa lớn)",
        (111, 243): "khối hiệu ứng ghép 3 cột x 3 hàng tile 8x16 (có khe xám nội bộ giữa các tile); 1 ô = 1 giai đoạn (giọt)",
        (136, 243): "khối hiệu ứng ghép 3 cột x 3 hàng tile 8x16; 1 ô = 1 giai đoạn (lửa nhỏ)",
        (161, 243): "khối hiệu ứng ghép 3 cột x 3 hàng tile 8x16; 1 ô = 1 giai đoạn (lửa lớn)",
    },
    cell_kind={
        (1, 11): "frame",
        (376, 11): "dan", (393, 11): "dan",
        (86, 74): "dan", (169, 74): "dan", (186, 74): "dan", (203, 74): "dan", (220, 74): "dan",
        (307, 74): "manh", (316, 74): "dan", (344, 148): "dan", (344, 165): "dan",
        (236, 11): "dan", (253, 11): "dan",
        (52, 74): "frame", (69, 74): "frame",
    },
    glued={},
)

# ======================================================================================
# 4) ITEMS & OBJECTS  (251 x 135) - sheet KHÔNG có nhãn nhóm (chỉ có "Unused") -> gán thủ công theo toạ độ
# ======================================================================================
_L, H, G = _mk()
G("Unused", "item_unused", (115, 73, 1))

_ITEM_CELLS = [
    # (x, y, stem, id, loai, ghi_chu)
    # --- hàng y=1: vật phẩm cơ bản ----------------------------------------------------
    (1, 1, "item_misc", "item_whip", "item", "roi (vật phẩm nâng cấp roi, hình xích chéo)"),
    (18, 1, "item_misc", "item_heart_small", "item", "tim nhỏ (8x16)"),
    (27, 1, "item_misc", "item_heart_large", "item", "tim lớn"),
    (44, 1, "item_misc", "item_moneybag_red", "item", "túi tiền đỏ"),
    (61, 1, "item_misc", "item_moneybag_purple", "item", "túi tiền tím"),
    (78, 1, "item_misc", "item_moneybag_white", "item", "túi tiền trắng/kem"),
    (95, 1, "item_misc", "item_crystal_0", "item", "quả cầu pha lê đỏ (khung 0)"),
    (112, 1, "item_misc", "item_crystal_1", "item", "quả cầu pha lê đỏ có viền hồng (khung 1)"),
    # --- nến / đuốc / cửa ------------------------------------------------------------
    (157, 1, "obj_candle", "obj_candle_0", "vat_the", "nến đôi (8x16), khung 0"),
    (166, 1, "obj_candle", "obj_candle_1", "vat_the", "nến đôi (8x16), khung 1 (lửa nhấp nháy)"),
    (175, 1, "obj_torch", "obj_torch_0", "vat_the", "chân đèn lửa (đuốc trên bệ) 16x32, khung 0"),
    (192, 1, "obj_torch", "obj_torch_1", "vat_the", "chân đèn lửa 16x32, khung 1"),
    (209, 1, "obj_door", "obj_door_arch", "vat_the", "cửa/cổng vòm 16x48 (mảng đen+đỏ, có xám ở góc trên)"),
    (226, 1, "obj_door", "obj_door_wide", "vat_the", "cửa/cổng rộng 24x48"),
    # --- hàng y=18: vũ khí phụ ------------------------------------------------------
    (1, 18, "item_weapon", "item_dagger", "item", "dao găm"),
    (18, 18, "item_weapon", "item_axe", "item", "rìu"),
    (35, 18, "item_weapon", "item_holywater", "item", "nước thánh (chai)"),
    (52, 18, "item_weapon", "item_cross", "item", "thập tự (boomerang)"),
    (69, 18, "item_weapon", "item_cross_spin_0", "dan", "thập tự đang xoay, khung 0 (hình sao/×)"),
    (86, 18, "item_weapon", "item_cross_spin_1", "dan", "thập tự đang xoay, khung 1"),
    (103, 18, "item_weapon", "item_stopwatch", "item", "đồng hồ bấm giờ (nhận diện bằng mắt, chưa chắc)"),
    (120, 18, "item_weapon", "item_double", "item", "II - bắn đôi (Double Shot)"),
    (137, 18, "item_weapon", "item_triple", "item", "III - bắn ba (Triple Shot)"),
    # --- hàng y=35: đồ ăn / báu vật -------------------------------------------------
    (1, 35, "item_treasure", "item_roast", "item", "gà nướng (hồi máu)"),
    (18, 35, "item_treasure", "item_jar", "item", "bình/lọ cam"),
    (35, 35, "item_treasure", "item_rosary", "item", "rosary (chuỗi thập tự xanh)"),
    (52, 35, "item_treasure", "item_1up", "item", "1UP"),
    (69, 35, "item_treasure", "item_crown_0", "item", "vương miện bảng màu cam"),
    (86, 35, "item_treasure", "item_crown_1", "item", "vương miện bảng màu tím"),
    (103, 35, "item_treasure", "item_crown_2", "item", "vương miện bảng màu trắng-xanh"),
    (120, 35, "item_treasure", "item_crown_3", "item", "vương miện bảng màu đỏ"),
    (137, 35, "item_treasure", "item_chest_0", "item", "rương bảng màu cam"),
    (154, 35, "item_treasure", "item_chest_1", "item", "rương bảng màu tím"),
    (171, 35, "item_treasure", "item_chest_2", "item", "rương bảng màu xanh"),
    (188, 35, "item_treasure", "item_chest_3", "item", "rương bảng màu hồng-đỏ"),
]
# --- hàng y=52: 8 cặp [khối 8x16][hình nghiêng 16x16] = 4 bảng màu x 2 kiểu ------------------
_PAL = ["cam", "tim", "kem-xanh", "do"]
_PAL_ID = ["orange", "purple", "tan", "red"]
for _p in range(4):
    for _k, _kind in enumerate(["u", "bang"]):
        _x8 = 1 + 26 * (_p * 2 + _k)
        _x16 = _x8 + 9
        _ITEM_CELLS.append((_x8, 52, "obj_block", "obj_block_%s_%s" % (_kind, _PAL_ID[_p]), "vat_the",
                            "khối đứng 8x16 (kiểu %s), bảng màu %s; ô kín không còn xám (viền sprite phủ hết ô)"
                            % ("'U'" if _kind == "u" else "'!'", _PAL[_p])))
        _ITEM_CELLS.append((_x16, 52, "obj_block", "obj_lean_%s_%s" % (_kind, _PAL_ID[_p]), "vat_the",
                            "hình nghiêng 16x16 đi cùng khối %s, bảng màu %s (chưa rõ tên vật thể)"
                            % ("'U'" if _kind == "u" else "'!'", _PAL[_p])))
# --- hàng y=69: chữ số điểm + gạch -------------------------------------------------------
_ITEM_CELLS += [
    (1, 69, "obj_score", "obj_score_10", "vat_the", "mảnh số điểm '10' (8x16, chỉ nửa trên có chữ; ghép với '0' => 100)"),
    (10, 69, "obj_score", "obj_score_20", "vat_the", "mảnh số điểm '20' (ghép => 200)"),
    (19, 69, "obj_score", "obj_score_40", "vat_the", "mảnh số điểm '40' (ghép => 400)"),
    (28, 69, "obj_score", "obj_score_70", "vat_the", "mảnh số điểm '70' (ghép => 700)"),
    (37, 69, "obj_score", "obj_score_0", "vat_the", "mảnh '0' đơn (đuôi của 100/200/400/700)"),
    (46, 69, "obj_score", "obj_score_00", "vat_the", "mảnh '00' (đuôi của 1000/2000)"),
    (55, 69, "obj_score", "obj_purple_orb", "vat_the", "viên/bóng tím viền đen 8x16 (chưa rõ; nằm cạnh hàng số điểm)"),
    (64, 69, "obj_brick", "obj_brick_set", "vat_the", "4 khối gạch nâu 8x16 DÍNH LIỀN trong 1 ô 32x16 (không có biên xanh)"),
]
# --- hàng y=83: Unused -------------------------------------------------------------------
_ITEM_CELLS += [
    (115, 83, "item_unused", "item_unused_0", "item", "không dùng: vật dài chéo màu tím (giống súng/gậy, chưa rõ)"),
    (132, 83, "item_unused", "item_unused_book", "item", "không dùng: cuốn sách có dấu thập"),
    (141, 83, "item_unused", "item_unused_boots", "item", "không dùng: đôi ủng/giày đỏ"),
    (158, 83, "item_unused", "item_unused_cup", "item", "không dùng: tách + đĩa"),
    (175, 83, "item_unused", "item_unused_basket", "item", "không dùng: giỏ"),
    (192, 83, "item_unused", "item_unused_envelope", "item", "không dùng: phong bì hồng"),
]

ITEMS = dict(
    labels=_L,
    names={},
    group_note={
        "item_misc": "Vật phẩm cơ bản (roi, tim, túi tiền, pha lê): hàng y=1, x<128.",
        "obj_candle": "Nến đôi 2 khung (8x16).",
        "obj_torch": "Chân đèn lửa 2 khung (16x32).",
        "obj_door": "2 ô cửa/cổng cao 48 px (một ô vòm 16 rộng, một ô 24 rộng).",
        "item_weapon": "Vũ khí phụ dạng vật phẩm (dao, rìu, nước thánh, thập tự, đồng hồ) + II/III.",
        "item_treasure": "Đồ ăn/bình/rosary/1UP + vương miện (4 bảng màu) + rương (4 bảng màu).",
        "obj_block": "8 cặp [khối 8x16 'U' hoặc '!'][hình nghiêng 16x16] x 4 bảng màu; chưa rõ tên vật thể (sheet không có nhãn).",
        "obj_score": "7 mảnh 8x16 (chữ số điểm + 1 viên tím); chữ chỉ ở nửa trên ô.",
        "obj_brick": "1 ô 32x16 chứa 4 khối gạch nâu dính liền.",
        "item_unused": "Nhãn 'Unused' trong sheet: 6 vật phẩm không dùng.",
    },
    group_kind={},
    cell_group={(c[0], c[1]): c[2] for c in _ITEM_CELLS},
    cell_names={(c[0], c[1]): c[3] for c in _ITEM_CELLS},
    cell_note={(c[0], c[1]): c[5] for c in _ITEM_CELLS},
    cell_kind={(c[0], c[1]): c[4] for c in _ITEM_CELLS},
    glued={(64, 69): [[0, 0, 8, 16], [8, 0, 8, 16], [16, 0, 8, 16], [24, 0, 8, 16]]},
)

SPECS = {
    "simon_belmont": SIMON,
    "enemies": ENEMIES,
    "bosses": BOSSES,
    "items_objects": ITEMS,
}
