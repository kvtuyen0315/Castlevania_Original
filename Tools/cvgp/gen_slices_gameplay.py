"""SINH 1 LẦN (routine CVGP 05, 13/10/2026) manifest Tools/cvgp/slices_gameplay.json từ kiểm kê
ClaudeNotes/cvgp/inventory/*.json + kết quả tách lớp roi (Tools/cvgp/whip_layers.py).
TỪ NAY SỬA THẲNG slices_gameplay.json (nguồn sự thật) — KHÔNG chạy lại script này (sẽ ghi đè chỉnh tay).
  python -P -E Tools/cvgp/whip_layers.py --out <tmp.json>
  python -P -E Tools/cvgp/gen_slices_gameplay.py <tmp.json> [--force]
Quy ước pivot: [px, py] = pixel tính từ góc TRÊN-TRÁI sprite (NesSpriteImporter đọc và đặt Custom pivot).
"""
import json
import os
import sys

INV = "ClaudeNotes/cvgp/inventory/{}.json"
SRC = "Art_Source/spriters-resource/{}.png"
OUT = "Tools/cvgp/slices_gameplay.json"
ART = "Assets/Data/Art/"
GRAY = [116, 116, 116]
D = 8  # thời lượng mặc định (tick) — chưa đo


def cells(sheet):
    return {c["id"]: c for c in json.load(open(INV.format(sheet), encoding="utf-8"))["cells"]}


SIMON, ENEMY, BOSS, ITEM = cells("simon_belmont"), cells("enemies"), cells("bosses"), cells("items_objects")
slices, anims, skipped = [], [], []


def piv(kind, w, h):
    return {"BC": [w // 2, h], "C": [w // 2, h // 2], "TL": [0, 0]}[kind]


def add(sheet, inv, src, name, out, pv, note="", rect=None, key=GRAY, **extra):
    r = rect or inv[src]["rect"]
    e = {"sheet": SRC.format(sheet), "name": name, "rect": r, "keyColor": key, "outDir": ART + out,
         "pivot": pv if isinstance(pv, list) else piv(pv, r[2], r[3]), "src": src}
    e.update(extra)
    if note:
        e["note"] = note
    slices.append(e)
    return out + "/" + name


def anim(set_, name, frames, loop, note="", durations=None, **extra):
    a = {"set": set_, "name": name, "frames": frames, "durations": durations or [D] * len(frames),
         "loop": loop, "measured": False}
    a.update(extra)
    if note:
        a["note"] = note
    anims.append(a)


def skip(src, why):
    skipped.append({"src": src, "why": why})


# ---------------- Simon (thân) ----------------
SM = "Character/Simon"
OLD = {"simon_idle_0": "idle_0", "simon_walk_0": "walk_0", "simon_walk_1": "walk_1", "simon_walk_2": "walk_2",
       "simon_intro_0": "back_0"}
for k, v in OLD.items():
    skip(k, f"đã cắt ở CVTI (Tools/cvti/slices.json → {SM}/{v}.png) — giữ nguyên byte/tên")
for src, name in [("simon_jumpduck_0", "jumpduck_0"), ("simon_stairs_up_0", "stairs_up_0"),
                  ("simon_stairs_up_1", "stairs_up_1"), ("simon_stairs_down_0", "stairs_down_0"),
                  ("simon_stairs_down_1", "stairs_down_1"), ("simon_hurt_0", "hurt_0"), ("simon_death_0", "death_0")]:
    add("simon_belmont", SIMON, src, name, SM, [8, 32], "pivot = giữa cột chân, đáy slot 32 (baseline hàng)")
add("simon_belmont", SIMON, "simon_death_1", "death_1", SM, [16, 16],
    "nằm ngang 32x16; sheet đặt ở nửa trên hàng ⇒ pivot = đáy-giữa ô (nằm trên sàn) — QUY ƯỚC, chưa đo video")
skip("simon_color_0", "trùng từng pixel simon_idle_0 (đã kiểm) — bảng màu mặc định")
for k in range(1, 5):
    add("simon_belmont", SIMON, f"simon_color_{k}", f"color_{k}", SM, [8, 32],
        "mẫu bảng màu nháy (tư thế đứng) — dùng bảng 'palettes' của SimonAnimSet để đổi màu khung bất kỳ")
POST = ["stand", "duck", "stairs_up", "stairs_down"]
for p in POST:
    for k in range(6):
        c = SIMON[f"simon_use_{p}_{k}"]
        add("simon_belmont", SIMON, c["id"], f"use_{p}_{k}", SM, c["pivot_goi_y"],
            "thân Whip/Use Subweapon; pivot = cột chân 16 px (inventory pivot_goi_y)" +
            ("; tay duỗi + chuôi roi màu" if k >= 3 else ""))

# ---------------- Simon (mảnh roi) ----------------
WP = SM + "/Whip"
for cid, c in SIMON.items():
    if cid.startswith("simon_whippart_"):
        add("simon_belmont", SIMON, cid, "whip_" + cid[len("simon_whippart_"):], WP, [0, 0],
            "mảnh roi rời; pivot góc trên-trái, đặt tại pivot thân + overlayOffset (quay trái như sheet)")
for cid in SIMON:
    if cid.startswith("simon_whip") and not cid.startswith("simon_whippart"):
        if cid.endswith("_4"):
            skip(cid, "mảnh thân 16x32 trùng từng pixel cột chân của simon_use_*_2..5 (đã kiểm)")
        else:
            skip(cid, "khung ghép sẵn thân+roi: dựng lại 100 % từ thân simon_use_* + mảnh roi "
                      "(Tools/cvgp/whip_layers.py + verify, 0 pixel lệch) ⇒ không cắt riêng")

# ---------------- Vũ khí phụ + hit ----------------
SW = "SubWeapon"
add("simon_belmont", SIMON, "sub_dagger_0", "dagger_0", SW, "C")
add("simon_belmont", SIMON, "sub_cross_0", "cross_0", SW, "C")
add("simon_belmont", SIMON, "sub_cross_spin_0", "cross_spin_0", SW, "C")
add("simon_belmont", SIMON, "sub_cross_spin_1", "cross_spin_1", SW, "C")
for k in range(4):
    add("simon_belmont", SIMON, f"sub_axe_{k}", f"axe_{k}", SW, "C")
add("simon_belmont", SIMON, "sub_holywater_bottle", "holywater_bottle", SW, "C")
for k in range(5):
    add("simon_belmont", SIMON, f"sub_holywater_fire_{k}", f"holywater_fire_{k}", SW, "BC",
        "lửa cháy trên sàn ⇒ pivot đáy-giữa")
add("simon_belmont", SIMON, "fx_hit_0", "hit_0", "FX", "C", "tia lóe khi đòn trúng")


# ---------------- Enemy ----------------
def enemy(folder, src, name, pv, note=""):
    return add("enemies", ENEMY, src, name, "Enemy/" + folder, pv, note)


for s in ("enemy_zombie_0", "enemy_zombie_1", "enemy_zombie_2"):
    skip(s, "mảnh 16x16 trùng từng pixel nửa trên/dưới khung Zombie 16x32 (đã kiểm)")
zombie = [enemy("Zombie", "enemy_zombie_3", "zombie_0", "BC"),
          enemy("Zombie", "enemy_zombie_4", "zombie_1", "BC", "16x31: đáy thẳng hàng khung 0")]
leo = [enemy("BlackLeopard", f"enemy_leopard_{k}", f"leopard_{k}", "BC") for k in range(4)]
bat = [enemy("VampireBat", f"enemy_bat_{k}", f"bat_red_{k}", "C") for k in range(3)]
batp = [enemy("VampireBat", f"enemy_bat_{k + 3}", f"bat_purple_{k}", "C", "bảng màu tím (chưa rõ màn dùng)")
        for k in range(4)]
for s in ("enemy_fishman_0", "enemy_fishman_1", "enemy_fishman_2", "enemy_fishman_3"):
    skip(s, "mảnh 16x16 trùng từng pixel nửa khung Fish Man 16x32 (đã kiểm)")
fish = [enemy("FishMan", f"enemy_fishman_{k + 5}", f"fishman_{k}", "BC") for k in range(4)]
fish_top = enemy("FishMan", "enemy_fishman_4", "fishman_piece_0", "BC",
                 "mảnh 16x16 KHÔNG nằm trong khung 16x32 nào (đã kiểm) — chưa rõ dùng khi nào")
fish_fb = enemy("FishMan", "enemy_fishman_fireball", "fishman_fireball", "C")
add("enemies", ENEMY, "enemy_fishman_bubble", "water_bubble", "FX", "C", "bọt nước tím khi Fish Man trồi/rơi")
for s in ("enemy_blackknight_0", "enemy_blackknight_1", "enemy_blackknight_2", "enemy_blackknight_3"):
    skip(s, "mảnh 16x16 trùng từng pixel nửa khung Black Knight (đã kiểm)")
knight = [enemy("BlackKnight", f"enemy_blackknight_{k + 4}", f"blackknight_{k}", "BC") for k in range(3)]
ghost = [enemy("Ghost", f"enemy_ghost_{k}", f"ghost_{k}", "C") for k in range(2)]
skull = []
for k, col in ((0, "purple"), (1, "red")):
    x, y, w, h = ENEMY[f"enemy_dragonskull_{k}"]["rect"]
    for j, part in enumerate(("top", "bottom")):
        skull.append(add("enemies", ENEMY, f"enemy_dragonskull_{k}", f"dragonskull_{col}_{part}",
                         "Enemy/DragonSkullCannon", [8, 16],
                         "ô DÍNH LIỀN 16x32 tách 2 đầu lâu 16x16 (trên/dưới khác nhau); pivot đáy-giữa",
                         rect=[x, y + 16 * j, 16, 16], edgeCheck=False))
skull_fb = enemy("DragonSkullCannon", "enemy_dragonskull_fireball", "dragonskull_fireball", "C")
medhead = [enemy("MedusaHead", f"enemy_medusahead_{k}", f"medusahead_{k}", "C") for k in range(2)]
flea = [enemy("Fleaman", f"enemy_fleaman_{k}", f"fleaman_{k}", "BC") for k in range(2)]
raven = [enemy("Raven", f"enemy_raven_{k}", f"raven_{k}", "C") for k in range(4)]
skel = [enemy("WhiteSkeleton", f"enemy_skeleton_{k}", f"skeleton_{k}", "BC") for k in range(2)]
bone = [enemy("WhiteSkeleton", f"enemy_skeleton_bone_{k}", f"bone_{k}", "C") for k in range(3)]
eagle = [enemy("Eagle", f"enemy_eagle_{k}", f"eagle_{k}", "C") for k in range(2)]
sdr = [enemy("SkeleDragon", f"enemy_skeledragon_{k}", f"skeledragon_head_{k}", "C") for k in range(2)]
sdr_bone = enemy("SkeleDragon", "enemy_skeledragon_bone", "skeledragon_bone", "C")
sdr_fb = enemy("SkeleDragon", "enemy_skeledragon_fireball", "skeledragon_fireball", "C")
rsk = [enemy("RedSkeleton", f"enemy_redskeleton_{k}", f"redskeleton_{k}", "BC") for k in range(4)]
axeman = [enemy("AxeMan", f"enemy_axeman_{k}", f"axeman_{k}", "BC") for k in range(2)]
axe = [enemy("AxeMan", f"enemy_axeman_axe_{k}", f"axeman_axe_{k}", "C") for k in range(4)]
death_fx = [add("enemies", ENEMY, f"fx_death_{k}", f"death_{k}", "FX", "C",
                "hiệu ứng chết enemy (giọt → lửa nhỏ → lửa lớn)") for k in range(3)]


# ---------------- Boss ----------------
def boss(folder, src, name, pv, note="", **kw):
    return add("bosses", BOSS, src, name, "Boss/" + folder, pv, note, **kw)


pb = [boss("PhantomBat", "boss_phantombat_0", "phantombat_0", "C", "gập cánh (ngủ)"),
      boss("PhantomBat", "boss_phantombat_1", "phantombat_1", "C", "dang cánh 48x32 (hợp tile)"),
      boss("PhantomBat", "boss_phantombat_2", "phantombat_2", "C", "cánh nửa 32x32 (hợp tile)")]
med = [boss("Medusa", "boss_medusa_0", "medusa_bust_0", "BC", "NỀN ĐEN THẬT — cắt nguyên chữ nhật, không key", key=None),
       boss("Medusa", "boss_medusa_1", "medusa_bust_1", "BC", "NỀN ĐEN THẬT — cắt nguyên chữ nhật, không key", key=None),
       boss("Medusa", "boss_medusa_2", "medusa_head", "C"), boss("Medusa", "boss_medusa_3", "medusa_stone", "C"),
       boss("Medusa", "boss_medusa_4", "snake_0", "BC"), boss("Medusa", "boss_medusa_5", "snake_1", "BC")]
mum = [boss("Mummy", f"boss_mummy_{k}", f"mummy_{'purple' if k < 3 else 'red'}_{k % 3}", "BC") for k in range(6)]
band = [boss("Mummy", f"boss_mummy_{k}", f"bandage_{k - 6}", "C") for k in (6, 7)]
frank = [boss("Frankenstein", f"boss_frankenstein_{k}", f"frankenstein_{k}", "BC") for k in range(3)]
igor = [boss("Frankenstein", f"boss_frankenstein_{k}", f"igor_{k - 3}", "BC") for k in (3, 4)]
igor_fb = boss("Frankenstein", "boss_frankenstein_5", "igor_fireball", "C")
reaper = boss("Death", "boss_reaper_0", "death_0", "C", "Tử thần bay 40x48 (hợp tile)")
scythe = [boss("Death", f"boss_reaper_{k}", f"scythe_{k - 1}", "C") for k in range(1, 5)]
d1 = [boss("Dracula", "boss_dracula1_0", "dracula1_headless_0", "BC", "thân không đầu (áo khép)"),
      boss("Dracula", "boss_dracula1_1", "dracula1_headless_1", "BC", "thân không đầu (áo mở)"),
      boss("Dracula", "boss_dracula1_2", "dracula1_head", "C", "đầu rời 8x16 (ghép lên thân không đầu)"),
      boss("Dracula", "boss_dracula1_3", "dracula1_fireball", "C"),
      boss("Dracula", "boss_dracula1_4", "dracula1_0", "BC", "thân có đầu, áo khép (hợp tile)"),
      boss("Dracula", "boss_dracula1_5", "dracula1_1", "BC", "thân có đầu, áo mở (hợp tile)")]
d2 = [boss("Dracula", f"boss_dracula2_{k}", f"dracula2_{k}", "BC", "dạng 2 (hợp tile)") for k in range(7)]
d2x = [boss("Dracula", "boss_dracula2_7", "dracula2_spark", "C", "16x16 be-xanh — chưa rõ (vuốt/lửa)"),
       boss("Dracula", "boss_dracula2_8", "dracula2_fireball", "C")]
bdfx = []
for size, ks in (("s", (0, 1, 2)), ("m", (3, 4, 5)), ("l", (6, 7, 8))):
    bdfx.append([add("bosses", BOSS, f"fx_bossdeath_{k}", f"bossdeath_{size}_{i}", "FX", "C",
                     "hiệu ứng chết boss (khối ghép sẵn)") for i, k in enumerate(ks)])
for k in (0, 1):
    skip(f"boss_unused_{k}", "nhãn 'Unused' trên sheet — không dùng trong game")


# ---------------- Item ----------------
def item(src, name, pv="BC", note="", folder="Item", **kw):
    return add("items_objects", ITEM, src, name, folder, pv, note, **kw)


IT = {}
for src, name in [("item_whip", "whip"), ("item_heart_small", "heart_small"), ("item_heart_large", "heart_large"),
                  ("item_moneybag_red", "moneybag_red"), ("item_moneybag_purple", "moneybag_purple"),
                  ("item_moneybag_white", "moneybag_white"), ("item_crystal_0", "crystal_0"),
                  ("item_crystal_1", "crystal_1"), ("item_dagger", "dagger"), ("item_axe", "axe"),
                  ("item_holywater", "holywater"), ("item_cross", "cross"), ("item_cross_spin_0", "cross_spin_0"),
                  ("item_cross_spin_1", "cross_spin_1"), ("item_stopwatch", "stopwatch"), ("item_double", "double"),
                  ("item_triple", "triple"), ("item_roast", "roast"), ("item_jar", "jar"), ("item_rosary", "rosary"),
                  ("item_1up", "1up")]:
    IT[name] = item(src, name)
for k in range(4):
    IT[f"crown_{k}"] = item(f"item_crown_{k}", f"crown_{k}")
for k in range(4):
    IT[f"chest_{k}"] = item(f"item_chest_{k}", f"chest_{k}")
moai = []
for col in ("orange", "purple", "tan", "red"):
    for kind in ("u", "bang"):
        moai.append(item(f"obj_block_{kind}_{col}", f"moai_block_{kind}_{col}", "BC",
                         "khối 8x16 đặc (không xám) — nhận diện Moai (Second Quest, wiki Secrets) là SUY LUẬN",
                         key=None, folder="Item/Moai"))
        moai.append(item(f"obj_lean_{kind}_{col}", f"moai_lean_{kind}_{col}", "BC",
                         "đầu nghiêng 16x16 — nhận diện Moai là SUY LUẬN", folder="Item/Moai"))
for cid, c in ITEM.items():
    if cid.startswith("item_unused"):
        skip(cid, "nhãn 'Unused' trên sheet — không dùng trong game")


# ---------------- Object ----------------
def obj(src, name, pv, note="", **kw):
    return add("items_objects", ITEM, src, name, "Object", pv, note, **kw)


candle = [obj(f"obj_candle_{k}", f"candle_{k}", "BC") for k in range(2)]
torch = [obj(f"obj_torch_{k}", f"torch_{k}", "BC", "chân đèn lửa (brazier)") for k in range(2)]
door = [obj("obj_door_arch", "door_0", "TL", "cửa 16x48"), obj("obj_door_wide", "door_1", "TL", "cửa 24x48")]
score = [obj(f"obj_score_{v}", f"score_{v}", "TL", "mảnh số điểm (chữ ở nửa trên); ghép ngang 10/20/40/70 + 0/00")
         for v in ("10", "20", "40", "70", "0", "00")]
bx, by, _, _ = ITEM["obj_brick_set"]["rect"]
brick = obj("obj_brick_set", "brick_0", "C", "ô DÍNH LIỀN 32x16 = 4 khối 8x16 GIỐNG HỆT (alsoAt kiểm) ⇒ cắt 1; chưa rõ công dụng",
            rect=[bx, by, 8, 16], edgeCheck=False, alsoAt=[[bx + 8, by], [bx + 16, by], [bx + 24, by]])
add("items_objects", ITEM, "obj_purple_orb", "brick_debris", "FX", "C",
    "mảnh vỡ tường tím — xác nhận video Map_1 1:39.6 (đập tường, mảnh tím bay)")

# ---------------- Bản đồ lâu đài (Cutscenes & Credits) ----------------
CS = "cutscenes_credits"
cmap = add(CS, None, "cutscene_map_bg", "castlemap_bg", "CastleMap", "TL",
           "nền bản đồ 384x144 (đục, không key) — đo bằng thành phần liên thông", rect=[262, 808, 384, 144], key=None)
cmark = add(CS, None, "cutscene_map_marker", "castlemap_marker", "CastleMap", "TL", "chấm Simon 8x16",
            rect=[262, 953, 8, 16])
cbat = [add(CS, None, f"cutscene_map_bat_{k}", f"castlemap_bat_{k}", "CastleMap", "TL", "dơi tím 16x16",
            rect=[271 + 17 * k, 953, 16, 16]) for k in range(4)]

# =============== ANIM ===============
def P(n):
    return SM + "/" + n


anim("Simon", "idle", [P("idle_0")], True)
anim("Simon", "walk", [P("walk_0"), P("walk_1"), P("walk_2"), P("walk_1")], True,
     "ping-pong như Intro CVTI (WALK_ORDER 0,1,2,1); nhịp chờ đo routine 08")
anim("Simon", "jump", [P("jumpduck_0")], False, "dùng chung sprite với duck")
anim("Simon", "duck", [P("jumpduck_0")], False)
anim("Simon", "stairs_up", [P("stairs_up_0"), P("stairs_up_1")], True)
anim("Simon", "stairs_down", [P("stairs_down_0"), P("stairs_down_1")], True)
anim("Simon", "hurt", [P("hurt_0")], False)
anim("Simon", "death", [P("death_0"), P("death_1")], False)
anim("Simon", "back", [P("back_0")], False, "quay lưng (Intro)")
for p in POST:
    anim("Simon", f"throw_{p}", [P(f"use_{p}_{k}") for k in range(3)], False,
         "ném vũ khí phụ: 3 thân như vung roi, không lớp roi" + ("; nhảy dùng stand" if p == "stand" else ""))
layers = {s["frame"]: s for s in json.load(open(sys.argv[1], encoding="utf-8"))}
LV = {"whip1": "l1", "whip2": "l2", "whip3c1": "l3c1", "whip3c2": "l3c2", "whip3c3": "l3c3", "whip3c4": "l3c4"}
for lv, tag in LV.items():
    for p in POST:
        fr, ov, off, top = [], [], [], []
        for i in range(3):
            s = layers[f"simon_{lv}_{p}_{i}"]
            fr.append(P(s["body"][len("simon_"):]))
            ov.append(WP + "/whip_" + s["part"][len("simon_whippart_"):])
            off += s["whipOffset"]
            top.append(s["whipOnTop"])
        note = ("thân + roi tách lớp; overlayOffsets = góc trên-trái mảnh roi − pivot thân (px NES, y xuống, quay TRÁI). "
                "Khung ghép sẵn _3 trên sheet = khung 2 với roi vẽ ĐÈ thân (whipOnTop) — routine 09 chọn theo video"
                + ("; nhảy dùng stand" if p == "stand" else ""))
        clip = layers[f"simon_{lv}_{p}_0"]["clippedPx"]
        if clip:
            note += f"; ô ghép sẵn _0 trên sheet bị cụt đáy ({clip} px của mảnh treo nằm ngoài ô)"
        anim("Simon", f"whip_{tag}_{p}", fr, False, note, overlays=ov, overlayOffsets=off, overlayOnTop=top)

anim("SubWeapon", "dagger", [SW + "/dagger_0"], True)
anim("SubWeapon", "cross", [SW + "/cross_0", SW + "/cross_spin_0", SW + "/cross_spin_1"], True, "thập tự xoay")
anim("SubWeapon", "axe", [SW + f"/axe_{k}" for k in range(4)], True)
anim("SubWeapon", "holywater_bottle", [SW + "/holywater_bottle"], True)
anim("SubWeapon", "holywater_fire", [SW + f"/holywater_fire_{k}" for k in range(5)], False, "lửa lớn dần")

anim("Zombie", "walk", zombie, True)
anim("BlackLeopard", "sit", [leo[0]], True)
anim("BlackLeopard", "run", leo[2:4], True, "nhãn suy luận từ hình")
anim("BlackLeopard", "jump", [leo[1]], False, "khung duỗi dài — suy luận")
anim("VampireBat", "perch", [bat[0]], True)
anim("VampireBat", "fly", bat[1:3], True)
anim("VampireBat", "purple_hang", [batp[0]], True, "bảng màu tím — suy luận")
anim("VampireBat", "purple_perch", [batp[1]], True)
anim("VampireBat", "purple_fly", batp[2:4], True)
anim("FishMan", "walk", fish[0:2], True)
anim("FishMan", "shoot", [fish[2]], False, "suy luận từ hình")
anim("FishMan", "jump", [fish[3]], False, "suy luận từ hình")
anim("FishMan", "piece", [fish_top], False, "mảnh lạ — xem ghi chú sprite")
anim("FishMan", "fireball", [fish_fb], True)
anim("BlackKnight", "walk", knight, True)
anim("Ghost", "fly", ghost, True)
anim("DragonSkullCannon", "purple", skull[0:2], False,
     "2 đầu lâu chồng: frames[0]=trên, [1]=dưới (vẽ cùng lúc, không phải chuỗi)")
anim("DragonSkullCannon", "red", skull[2:4], False, "2 đầu lâu chồng: frames[0]=trên, [1]=dưới")
anim("DragonSkullCannon", "fireball", [skull_fb], True)
anim("MedusaHead", "fly", medhead, True)
anim("Fleaman", "hop", flea, True)
anim("Raven", "perch", [raven[0]], True)
anim("Raven", "fly", raven[1:4], True)
anim("WhiteSkeleton", "walk", skel, True)
anim("WhiteSkeleton", "bone", bone, True, "xương xoay; bone_1/2 là lật gương của nhau")
anim("Eagle", "fly", eagle, True)
anim("SkeleDragon", "head", sdr, True)
anim("SkeleDragon", "bone", [sdr_bone], True)
anim("SkeleDragon", "fireball", [sdr_fb], True)
anim("RedSkeleton", "collapse", rsk[0:2], False, "đống xương → đang ráp (chạy ngược để ráp lại)")
anim("RedSkeleton", "walk", rsk[2:4], True)
anim("AxeMan", "walk", axeman, True)
anim("AxeMan", "axe", axe, True)
anim("PhantomBat", "sleep", [pb[0]], True)
anim("PhantomBat", "fly", pb[1:3], True)
anim("Medusa", "bust", med[0:2], False, "tượng/mặt Medusa nền đen")
anim("Medusa", "fly", [med[2]], True)
anim("Medusa", "stone", [med[3]], True)
anim("Medusa", "snake", med[4:6], True)
anim("Mummy", "walk_purple", mum[0:3], True)
anim("Mummy", "walk_red", mum[3:6], True)
anim("Mummy", "bandage", band, True)
anim("Frankenstein", "walk", frank, True)
anim("Igor", "hop", igor, True)
anim("Igor", "fireball", [igor_fb], True)
anim("Death", "fly", [reaper], True)
anim("Death", "scythe", scythe, True)
anim("Dracula", "p1_idle", [d1[4]], True)
anim("Dracula", "p1_cape_open", [d1[5]], False)
anim("Dracula", "p1_headless", [d1[0], d1[1]], False, "thân không đầu (áo khép, mở)")
anim("Dracula", "p1_head", [d1[2]], True)
anim("Dracula", "p1_fireball", [d1[3]], True)
anim("Dracula", "p2_all", d2, False, "7 khung dạng 2 theo thứ tự sheet — routine 20 chia anim")
anim("Dracula", "p2_spark", [d2x[0]], True)
anim("Dracula", "p2_fireball", [d2x[1]], True)

for n in ("whip", "heart_small", "heart_large", "moneybag_red", "moneybag_purple", "moneybag_white", "dagger", "axe",
          "holywater", "cross", "stopwatch", "double", "triple", "roast", "jar", "rosary", "1up"):
    anim("Item", n, [IT[n]], True)
anim("Item", "cross_spin", [IT["cross_spin_0"], IT["cross_spin_1"]], True, "biểu tượng thập tự xoay (sheet Items)")
anim("Item", "moneybag_flash", [IT["moneybag_red"], IT["moneybag_purple"], IT["moneybag_white"]], True,
     "túi tiền nhấp nháy (bonus 1000) — thứ tự suy luận")
anim("Item", "crystal", [IT["crystal_0"], IT["crystal_1"]], True, "Magic Crystal nhấp nháy")
anim("Item", "crown", [IT[f"crown_{k}"] for k in range(4)], True, "4 bảng màu nhấp nháy")
anim("Item", "chest", [IT[f"chest_{k}"] for k in range(4)], True, "4 bảng màu nhấp nháy")
anim("Item", "moai", moai, False, "16 sprite Moai (suy luận) theo thứ tự sheet — không phải chuỗi")
anim("Object", "candle", candle, True)
anim("Object", "torch", torch, True)
anim("Object", "door", door, False, "2 sprite cửa (16x48, 24x48) — vai trò chờ routine 15")
anim("Object", "score_parts", score, False, "mảnh số điểm (không phải chuỗi)")
anim("Object", "brick", [brick], False)
anim("FX", "hit", ["FX/hit_0"], False)
anim("FX", "death", death_fx, False)
anim("FX", "bossdeath_s", bdfx[0], False)
anim("FX", "bossdeath_m", bdfx[1], False)
anim("FX", "bossdeath_l", bdfx[2], False)
anim("FX", "water_bubble", ["FX/water_bubble"], False)
anim("FX", "brick_debris", ["FX/brick_debris"], False)
anim("CastleMap", "bg", [cmap], False)
anim("CastleMap", "marker", [cmark], True)
anim("CastleMap", "bat", cbat, True)

# bảng màu Simon (sheet hàng Colors: ánh xạ màu khung 0 → khung k, 1-1, đã kiểm)
PAL = {"purple": [[124, 8, 0], [0, 0, 0], [252, 216, 168], [252, 196, 252], [252, 152, 56], [204, 136, 252]],
       "blue": [[124, 8, 0], [0, 0, 0], [252, 216, 168], [0, 112, 236], [252, 152, 56], [252, 216, 168]],
       "red": [[124, 8, 0], [0, 0, 0], [252, 216, 168], [252, 116, 180], [252, 152, 56], [216, 40, 0]],
       "white": [[124, 8, 0], [168, 228, 252], [252, 216, 168], [252, 152, 56], [252, 152, 56], [252, 216, 168]]}
palettes = []
for k, (n, v) in enumerate(PAL.items(), 1):
    palettes.append({"set": "Simon", "name": n, "from": sum(v[0::2], []), "to": sum(v[1::2], []),
                     "note": f"hàng Colors khung {k}" +
                             (" — nháy khi nhặt roi (video Map_1 0:06.5–0:07.3: mặc định/đỏ/tím/xanh)" if k <= 3
                              else " — chưa thấy trong video Map_1")})

doc = {
    "_doc": "Manifest cắt sprite gameplay CVGP — NGUỒN SỰ THẬT (sinh 1 lần bằng Tools/cvgp/gen_slices_gameplay.py, từ nay "
            "sửa tay). Cắt: python -P -E Tools/cvti/slice_sprites.py --manifest Tools/cvgp/slices_gameplay.json. "
            "pivot = [px, py] tính từ góc TRÊN-TRÁI sprite (NesSpriteImporter đọc file này). src = id kiểm kê planner. "
            "anims: frames/overlays = đường dẫn dưới Assets/Data/Art (không .png); overlayOffsets = phẳng [x0,y0,x1,y1..] "
            "(góc trên-trái lớp phủ − pivot thân, px NES, y xuống, quay TRÁI như sheet); durations mặc định 8, "
            "measured=false. palettes: from/to phẳng [r,g,b,..]. Dựng AnimSet: menu CVGP/Build AnimSets. "
            "skipped = ô kiểm kê KHÔNG cắt + lý do.",
    "sheetBg": [0, 128, 0],
    "animDir": "Assets/Data/Config/Anim",
    "slices": slices, "anims": anims, "palettes": palettes, "skipped": skipped,
}
if os.path.exists(OUT) and "--force" not in sys.argv:
    sys.exit(f"{OUT} đã có — đây là script SINH 1 LẦN; sửa thẳng manifest (hoặc --force nếu chắc chắn).")
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write("{\n")
    keys = list(doc)
    for i, k in enumerate(keys):
        v = doc[k]
        if isinstance(v, list):
            f.write(f' "{k}": [\n')
            for j, e in enumerate(v):
                f.write("  " + json.dumps(e, ensure_ascii=False) + ("," if j < len(v) - 1 else "") + "\n")
            f.write(" ]")
        else:
            f.write(f' "{k}": {json.dumps(v, ensure_ascii=False)}')
        f.write(",\n" if i < len(keys) - 1 else "\n")
    f.write("}\n")
print(f"slices {len(slices)}, anims {len(anims)}, palettes {len(palettes)}, skipped {len(skipped)}")
