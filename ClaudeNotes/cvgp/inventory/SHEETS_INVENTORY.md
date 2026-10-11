# Kiểm kê 5 sprite sheet Castlevania (NES) — rip bởi Mister Mike

> **Ghi chú planner (11/10/2026):** bản này được chép từ thư mục làm việc tạm của planner vào `ClaudeNotes/cvgp/inventory/`. Script ở `tools/` đang đọc sheet từ thư mục Downloads (`SRC_DIR` trong `tools/sheetlib.py`) — routine **01** trỏ lại vào `Art_Source/spriters-resource/` (tên file đổi snake_case), chạy lại để xác nhận JSON y hệt, rồi chuyển script dùng lại được sang `Tools/cvgp/`.

Tạo ngày 2026-10-11 bằng `tools\run_all.py` (chạy lại ra y hệt). Không sửa file nào trong dự án Unity; mọi thứ nằm trong thư mục này.

Nguồn (chỉ đọc): `C:\Users\kvtuy\Downloads\NES - Castlevania - ....png` (5 file; hai sheet Title Screen và Cutscenes & Credits đã cắt trước đó, không làm lại).

| File | Nội dung |
|---|---|
| `simon_belmont.json`, `simon_belmont_annot.png` | Simon Belmont: 181 ô / 43 nhóm (JSON: nhãn, nhóm, hàng, ô, bất thường; ảnh: khung + số thứ tự `stt`) |
| `enemies.json`, `enemies_annot.png` | Enemies: 71 ô / 16 nhóm (JSON: nhãn, nhóm, hàng, ô, bất thường; ảnh: khung + số thứ tự `stt`) |
| `bosses.json`, `bosses_annot.png` | Bosses: 54 ô / 9 nhóm (JSON: nhãn, nhóm, hàng, ô, bất thường; ảnh: khung + số thứ tự `stt`) |
| `items_objects.json`, `items_objects_annot.png` | Items & Objects: 65 ô / 10 nhóm (JSON: nhãn, nhóm, hàng, ô, bất thường; ảnh: khung + số thứ tự `stt`) |
| `level1_entrance.json`, `level1_entrance_annot.png`, `level1_entrance_annot_1..4.png` | Bản đồ Level 1: vùng, lưới, thống kê tile, loại khối, cầu thang, nước, cửa, bản đồ ký tự |
| `tools\` | `sheetlib.py`, `sheet_specs.py` (gán nhãn thủ công), `inv_sprites.py`, `inv_level1.py`, `make_inventory_md.py`, `run_all.py` |

Tổng cộng sheet 1-4: **371 ô** sprite (+ 5 vùng bản đồ ở sheet 5).

## 0. Quy ước chung (đã đo trên cả 5 sheet)

- Cả 5 sheet đều đục hoàn toàn (alpha=255), chỉ 11-14 màu NES. Nền sheet = **(0,128,0)**; mỗi sprite nằm trên ô nền xám **(116,116,116)** (màu key). Chữ nhãn = **(255,255,255)** ở sheet 1-4 (font 8 px, cao 7-8 px) và (252,252,252) ở sheet 5.
- **Ô sprite** = thành phần liên thông (8 láng giềng) của pixel khác nền sheet và khác chữ trắng. Đã kiểm liên thông 4 và 8 cho cùng kết quả (ngoài logo) => không có ô nào chỉ dính nhau ở góc chéo; các ô cùng nhóm thường cách nhau đúng 1 px xanh (bước 17 px với ô 16 px; riêng hai bảng màu Vampire Bat cách 3 px).
- `rect` = [x, y, w, h] theo pixel gốc, gốc ở góc trên-trái sheet. Ô Simon/boss là **hợp các tile 8x16** nên nhiều ô không phải chữ nhật đặc (`fill` < 1): bbox vẫn chính xác, phần trống trong bbox là nền xanh.
- Quy ước khi cắt: pixel xám (116,116,116) và pixel xanh nền (0,128,0) trong bbox = trong suốt; **đen (0,0,0) là màu thật**. `co_den_that` = ô có pixel đen thật. Ô không có xám (Medusa nền đen, 8 khối 8x16 ở sheet items) là chữ nhật đặc, không key-out.
- Logo credit "Sprites ripped by Mister Mike" (khung 224x32 + bóng chữ (0,64,0) = 516 px) có ở mọi sheet: bỏ qua khi cắt.
- Id: `<nhóm>_<chỉ số>` (chỉ số theo thứ tự trái->phải, trên->dưới trong nhóm) hoặc tên riêng khi đã nhận diện được (vd `item_heart_small`). Id duy nhất trên cả 4 sheet (đã kiểm).
- `loai`: frame = khung hoạt hình, manh = mảnh/biến thể cần ghép, dan = đạn/vũ khí ném, hieu_ung, item, vat_the. Phân loại chỉ dựa vào nhìn ảnh nên mục "manh" và vài tên item có ghi chú "chưa chắc".

## 1. Simon Belmont — `simon_belmont` (799×529)

File nguồn: `NES - Castlevania - Playable Characters - Simon Belmont.png`

| Mục | Giá trị |
|---|---|
| Màu nền sheet | (0,128,0) (xanh lá) |
| Màu key (nền ô) | (116,116,116) (xám) |
| Màu chữ nhãn | (255,255,255) |
| Số màu khác nhau | 14 |
| Số nhãn chữ | 52 (43 nhãn nhóm + 9 nhãn tiêu đề) |
| Số nhóm | 43 |
| **Số ô** | **181** (loại: khung 138, mảnh 28, đạn/vũ khí ném 14, hiệu ứng 1) |
| Ô không đặc (hợp tile, fill<1) | 125 |
| Ô không có pixel xám | 0 |
| Ô dính liền (nhiều khung 1 ô) | 0 |
| Logo credit (bỏ qua) | rect [574,496,224,32] + 6 mảnh chữ bên trong |

**Histogram màu** (số pixel toàn sheet; vai trò): `#008000` (0,128,0) 279761 = nền sheet; `#747474` (116,116,116) 71818 = màu key (nền ô xám); `#7c0800` (124,8,0) 26804 = sprite; `#ffffff` (255,255,255) 13077 = chữ nhãn; `#fc9838` (252,152,56) 10189 = sprite; `#fcd8a8` (252,216,168) 6702 = sprite; `#000000` (0,0,0) 6381 = sprite; `#d82800` (216,40,0) 2358 = sprite; `#fc74b4` (252,116,180) 1608 = sprite; `#cc88fc` (204,136,252) 1475 = sprite; `#fcc4fc` (252,196,252) 1233 = sprite; `#0070ec` (0,112,236) 559 = sprite; `#004000` (0,64,0) 516 = chỉ có trong logo credit; `#a8e4fc` (168,228,252) 190 = sprite.

**Bố cục**: 9 hàng ô. Hàng 0 (y 21-52): Idle, Walk, Jump/Duck, Climb Stairs, Hurt, Death, Intro, Colors. Hàng 1 (y 78-109): Whip/Use Subweapon. Hàng 2-7: Whip Level 1, Level 2, Level 3 màu 1-4 (mỗi hàng 4 nhóm con Standing/Jumping, Ducking, Ascending Stairs, Descending Stairs). Hàng 8 (y 477+): roi rời, Subweapons, Hit Effect.

**Bảng nhóm** (nhóm roi và Use Subweapon được gộp dòng; JSON có đủ 43 nhóm):

| Nhóm (id) | Nhãn chữ | stt | Số khung | Kích thước điển hình | Ghi chú |
|---|---|---|---|---|---|
| `simon_idle` | Idle | 1-1 | 1 | 16x32 | 1 khung đứng. Simon quay mặt TRÁI trong sheet (lật ngang khi quay phải). |
| `simon_walk` | Walk | 2-4 | 3 | 16x32 x3 | 3 khung đi bộ (chu kỳ), cách nhau 1 px xanh. Simon quay mặt TRÁI. |
| `simon_jumpduck` | Jump/Duck | 5-5 | 1 | 16x32 | 1 khung cúi/cuộn người (dùng chung cho nhảy và cúi). Tile dưới trống: pixel sprite chỉ tới y+24, 8 px cuối ô là xám trống. |
| `simon_stairs_up` | Climb Stairs (Ascend) | 6-7 | 2 | 16x32 x2 | 2 khung leo cầu thang đi lên. |
| `simon_stairs_down` | Climb Stairs (Descend) | 8-9 | 2 | 16x32 x2 | 2 khung leo cầu thang đi xuống. |
| `simon_hurt` | Hurt | 10-10 | 1 | 16x32 | 1 khung bị thương/bật lùi. |
| `simon_death` | Death | 11-12 | 2 | 16x32, 32x16 | 2 ô: [0] 16x32 (gục/cúi, 8 px cuối ô trống), [1] 32x16 nằm ngang đặt ở nửa TRÊN của hàng (đáy thân tại y+16, không phải baseline hàng). |
| `simon_intro` | Intro | 13-13 | 1 | 16x32 | 1 khung nhìn từ phía sau (cảnh intro đi vào cổng). |
| `simon_color` | Colors | 14-18 | 5 | 16x32 x5 | 5 khung cùng tư thế, 5 bảng màu: [0] mặc định (đỏ-nâu), [1] tím/đen, [2] xanh dương/đen, [3] đỏ-hồng/đen, [4] trắng-xanh nhạt. |
| `simon_use_*` | Whip/Use Subweapon (4 nhóm con: Standing, Ducking, Ascending Stairs, Descending Stairs) | 19-42 | 6+6+6+6=24 | 24x32 x20, 16x32 x4 | mỗi nhóm con 6 ô: [0] chuẩn bị, [1] biến thể thu hẹp (16 rộng), [2..5] tay duỗi với 4 màu vật cầm (không/tím/xanh/hồng-đỏ); ô là hợp tile 8x16 (fill 0.62-0.83); nhóm Ducking: 8 px cuối ô trống |
| `simon_whip1_*` | Whip (Level 1) (4 nhóm con) | 43-60 | 5+5+4+4=18 | 32x32 x7, 44x33 x6, 16x32 x2, 44x32 x2, ... | 4 nhóm con: Standing/Jumping (5 ô), Ducking (5 ô), Ascending Stairs (4 ô), Descending Stairs (4 ô). Ô: [0] chuẩn bị (roi rủ), [1] roi vắt qua đầu, [2][3] roi duỗi (rộng 44 px, thân Simon ở 16 px bên PHẢI, roi bên trái), [4] thân Simon 16x32 lẻ (chỉ ở Standing và Ducking, loại manh). Ducking: 8 px cuối ô trống. Simon quay mặt TRÁI. Roi hồng/trắng viền đen. |
| `simon_whip2_*` | Whip (Level 2) (4 nhóm con) | 61-78 | 5+5+4+4=18 | 44x32 x8, 32x32 x7, 16x32 x2, 32x34 | Như `simon_whip1_*` (ô roi duỗi rộng 44 px). Roi tím (chuỗi). |
| `simon_whip3c1_*` | Whip (Level 3, Color 1) (4 nhóm con) | 79-96 | 5+5+4+4=18 | 60x32 x8, 32x32 x7, 16x32 x2, 32x34 | Như `simon_whip1_*` nhưng roi dài hơn: ô roi duỗi rộng 60 px. Roi cam. |
| `simon_whip3c2_*` | Whip (Level 3, Color 2) (4 nhóm con) | 97-114 | 5+5+4+4=18 | 60x32 x8, 32x32 x7, 16x32 x2, 32x34 | Như `simon_whip1_*` nhưng roi dài hơn: ô roi duỗi rộng 60 px. Roi tím. |
| `simon_whip3c3_*` | Whip (Level 3, Color 3) (4 nhóm con) | 115-132 | 5+5+4+4=18 | 60x32 x8, 32x32 x7, 16x32 x2, 32x34 | Như `simon_whip1_*` nhưng roi dài hơn: ô roi duỗi rộng 60 px. Roi xanh dương. |
| `simon_whip3c4_*` | Whip (Level 3, Color 4) (4 nhóm con) | 133-150 | 5+5+4+4=18 | 60x32 x8, 32x32 x7, 16x32 x2, 32x33 | Như `simon_whip1_*` nhưng roi dài hơn: ô roi duỗi rộng 60 px. Roi đỏ. |
| `simon_whippart` | Whip | 151-166 | 16 | 16x24 x5, 8x32 x5, 40x16 x4, 24x16 x2 | Mảnh roi RỜI (không có thân Simon), tổng 16 ô: L1 (hồng/trắng, viền đen): treo 8x32, vắt 16x24, duỗi 24x16; L3 màu 1..4 (cam/tím/xanh/đỏ, dạng xích): treo 8x32 x4, vắt 16x24 x4, duỗi 40x16 x4; L2 chỉ có mảnh duỗi 24x16 (tím). Đã khớp từng pixel: mảnh treo (8x32) nằm nguyên vẹn tại x=24 của ô [0] và mảnh vắt (16x24) tại x=16 của ô [1] của nhóm Standing cùng màu; L2 dùng chung mảnh treo/vắt với L3 màu 2; mảnh duỗi 24x16 tím của L2 nằm ở x=0 trong 2 ô roi duỗi của L3 màu 2. |
| `sub_dagger` | Subweapons Dagger | 167-167 | 1 | 16x16 | 1 ô 16x16: dao găm nằm ngang. |
| `sub_cross` | Subweapons Cross/Boomerang | 168-170 | 3 | 16x16 x3 | 3 ô 16x16: [0] thập tự (+), [1][2] hai khung xoay (dạng ×). |
| `sub_axe` | Subweapons Axe | 171-174 | 4 | 16x16 x4 | 4 ô 16x16: rìu xoay 4 khung. |
| `sub_holywater` | Subweapons Holy Water/Fire Bomb | 175-180 | 6 | 8x16 x4, 16x16 x2 | 6 ô: [0] chai (8x16), [1..5] lửa cháy lớn dần (8x16 x3, 16x16 x2). |
| `fx_hit` | Hit Effect | 181-181 | 1 | 8x16 | 1 ô 8x16: tia lóe khi đòn trúng. |

**Hàng và baseline y** (slot Simon cao 32 px; baseline_y = đáy slot, dùng làm pivot chân):

| Hàng | y0..y1 | Số ô | baseline_y | Đỉnh slot | Ô lệch đáy |
|---|---|---|---|---|---|
| 0 | 21..53 | 18 | **53** | 21 | `simon_death_1` |
| 1 | 78..110 | 24 | **110** | 78 | - |
| 2 | 134..169 | 18 | **167** | 135 | `simon_whip1_stairs_up_0` |
| 3 | 192..226 | 18 | **224** | 192 | `simon_whip2_stairs_up_0` |
| 4 | 249..283 | 18 | **281** | 249 | `simon_whip3c1_stairs_up_0` |
| 5 | 306..340 | 18 | **338** | 306 | `simon_whip3c2_stairs_up_0` |
| 6 | 363..397 | 18 | **395** | 363 | `simon_whip3c3_stairs_up_0` |
| 7 | 420..453 | 18 | **452** | 420 | `simon_whip3c4_stairs_up_0` |
| 8 | 477..509 | 31 | (không áp dụng: roi rời + vũ khí phụ) | - | - |

- Mọi ô Simon trong hàng 0-7 có đáy slot trùng baseline_y, trừ các ô thò roi thấp hơn chân (+1..2 px, nhóm Ascending Stairs [0]) và ô nằm ngang `simon_death_1` (đáy ở y+16).
- Chân (pixel sprite thấp nhất) cách đáy ô: **0-1 px** khi đứng/đi/leo cầu thang; **8 px** khi cúi (Jump/Duck, Ducking, Death[0]) vì tile dưới trống. => pivot nên lấy theo baseline_y của hàng (đáy slot 32 px), không lấy theo pixel thấp nhất, để các khung khớp nhau như trong game.
- Trường `chan_x` = [x0, x0+16] (tương đối trong ô) là **cột chân**: đoạn cột rộng đúng 16 px mà ô phủ đủ 16 hàng cuối của slot (2 tile chân 8x16 của Simon), và `pivot_goi_y` = [x0+8, baseline_rel] (baseline_rel = baseline_y - y của ô). Quy tắc này cho đáp án DUY NHẤT với 149/150 ô Simon hàng 0-7 (trừ ô nằm ngang `simon_death_1`). Ví dụ pivot X: ô 16 rộng = 8; chuẩn bị/vắt roi 32 rộng = 8; roi duỗi 44 rộng = 36, 60 rộng = 52; nhóm Use Subweapon 24 rộng = 8 (khung [0]) hoặc 16 (khung [2..5]).
- Với ô rộng hơn 16 px, trường `phan` gợi ý tách thành `phu_trai` / `cot_chan` / `phu_phai` (phần phụ = roi, tay, vũ khí); chúng dính liền không có biên xanh nên vẫn giữ 1 ô.
- Simon quay mặt TRÁI trong sheet (roi duỗi sang trái); cần lật ngang khi quay phải.

**Ô / hiện tượng bất thường**

- Kiểm tra: pixel chữ trắng chạm pixel sprite: 0 (0 = nhãn không dính ô nào)
- Màu lạ (0,64,0): 516 px — chỉ nằm trong logo credit (bóng chữ), không phải màu sprite
- 125 ô không đặc (fill<1) trong các nhóm: `simon_use_*` 24, `simon_whip1_*` 16, `simon_whip2_*` 16, `simon_whip3c1_*` 16, `simon_whip3c2_*` 16, `simon_whip3c3_*` 16, `simon_whip3c4_*` 16, `simon_whippart` 5. Ô là hợp tile 8x16: bbox đúng nhưng có góc trống màu xanh; khi cắt giữ nguyên bbox, coi pixel xanh/xám là trong suốt.
- 32 ô có vùng xám key KHÉP KÍN nhỏ (tổng 350 px, ví dụ `simon_stairs_up_0`, `simon_intro_0`, `simon_use_stairs_up_0`): thường là lỗ trong sprite (vòng roi, quai bình...); key-out theo màu vẫn đúng. Chi tiết ở trường `key_khep_kin` của từng ô.
- 12 ô cao hơn slot 32 px (roi thò ra): `simon_whip1_stand_2` 44x33, `simon_whip1_stand_3` 44x33, `simon_whip1_duck_2` 44x33, `simon_whip1_duck_3` 44x33, `simon_whip1_stairs_up_0` 32x34, `simon_whip1_stairs_down_2` 44x33, `simon_whip1_stairs_down_3` 44x33, `simon_whip2_stairs_up_0` 32x34, `simon_whip3c1_stairs_up_0` 32x34, `simon_whip3c2_stairs_up_0` 32x34, `simon_whip3c3_stairs_up_0` 32x34, `simon_whip3c4_stairs_up_0` 32x33
- Không có ô dính liền thật: mọi ô cách nhau >= 1 px xanh. Các ô roi dài (44/60 rộng) là 1 ô ghép thân + roi, không phải 2 khung.
- Ô 16x32 lẻ ở cuối nhóm Standing/Ducking (trùng từng pixel với thân trong ô roi duỗi, đã kiểm) là mảnh thân Simon (loại `manh`), không phải khung riêng.
- Hàng 8: mảnh roi rời, vũ khí phụ, hit effect không nằm trên slot 32 px (không có baseline).

## 2. Enemies — `enemies` (474×190)

File nguồn: `NES - Castlevania - Enemies & Bosses - Enemies.png`

| Mục | Giá trị |
|---|---|
| Màu nền sheet | (0,128,0) (xanh lá) |
| Màu key (nền ô) | (116,116,116) (xám) |
| Màu chữ nhãn | (255,255,255) |
| Số màu khác nhau | 11 |
| Số nhãn chữ | 16 (16 nhãn nhóm + 0 nhãn tiêu đề) |
| Số nhóm | 16 |
| **Số ô** | **71** (loại: khung 44, mảnh 13, đạn/vũ khí ném 11, hiệu ứng 3) |
| Ô không đặc (hợp tile, fill<1) | 0 |
| Ô không có pixel xám | 0 |
| Ô dính liền (nhiều khung 1 ô) | 2 |
| Logo credit (bỏ qua) | rect [249,157,224,32] + 6 mảnh chữ bên trong |

**Histogram màu** (số pixel toàn sheet; vai trò): `#008000` (0,128,0) 58758 = nền sheet; `#747474` (116,116,116) 8781 = màu key (nền ô xám); `#000000` (0,0,0) 8681 = sprite; `#ffffff` (255,255,255) 3557 = chữ nhãn; `#d82800` (216,40,0) 3257 = sprite; `#fcc4fc` (252,196,252) 2491 = sprite; `#fc74b4` (252,116,180) 2171 = sprite; `#cc88fc` (204,136,252) 1722 = sprite; `#004000` (0,64,0) 516 = chỉ có trong logo credit; `#0070ec` (0,112,236) 74 = sprite; `#fcd8a8` (252,216,168) 52 = sprite.

**Bố cục**: 4 hàng nhãn/ô; mỗi nhãn nằm ngay trên nhóm của nó. Zombie, Fish Man, Black Knight xếp 2 tầng (tầng trên 16x16, tầng dưới 16x32).

**Bảng nhóm**:

| Nhóm (id) | Nhãn chữ | stt | Số khung | Kích thước điển hình | Ghi chú |
|---|---|---|---|---|---|
| `enemy_zombie` | Zombie | 1-5 | 5 | 16x16 x3, 16x31, 16x32 | 5 ô: hàng trên 3 ô 16x16 (mảnh/biến thể thân trên và thân dưới), hàng dưới 2 khung 16x32 đầy đủ (có vẻ ghép từ các mảnh hàng trên). |
| `enemy_leopard` | Black Leopard | 6-9 | 4 | 32x16 x3, 24x16 | 4 khung báo đen: [0] 24x16 (nằm/ngồi), [1][2][3] 32x16 (chạy/nhảy). |
| `enemy_bat` | Vampire Bat | 10-16 | 7 | 16x16 x7 | 7 ô 16x16: [0..2] 3 khung (viền hồng, thân đỏ-đen), [3..6] 4 khung (bảng màu tím); khoảng cách 3 px giữa 2 bảng màu. |
| `enemy_fishman` | Fish Man | 17-27 | 11 | 16x16 x5, 16x32 x4, 8x16 x2 | 11 ô: hàng trên 5 ô 16x16 (mảnh/biến thể), hàng dưới 4 khung 16x32 và 2 đạn 8x16 (cầu lửa đỏ, bong bóng tím). |
| `enemy_blackknight` | Black Knight | 28-34 | 7 | 16x16 x4, 16x31 x2, 16x32 | 7 ô: hàng trên 4 ô 16x16 (mảnh/biến thể: có thể là phần thân/vũ khí), hàng dưới 3 khung 16x32. |
| `enemy_ghost` | Ghost | 35-36 | 2 | 16x16 x2 | 2 khung 16x16. |
| `enemy_dragonskull` | Dragon Skull Cannon | 37-39 | 3 | 16x32 x2, 8x16 | 3 ô: [0] tím, [1] đỏ - mỗi ô 16x32 DÍNH LIỀN 2 khung 16x16 xếp dọc (không có biên xanh; khung trên miệng mở, khung dưới khép/khác), [2] cầu lửa 8x16. |
| `enemy_medusahead` | Medusa/Medusa Head | 40-41 | 2 | 16x16 x2 | 2 khung 16x16 (đầu Medusa xanh). |
| `enemy_fleaman` | Hunchback/Fleaman | 42-43 | 2 | 16x16 x2 | 2 khung 16x16 (người lưng gù, tông đỏ). |
| `enemy_raven` | Raven | 44-47 | 4 | 16x16 x4 | 4 khung 16x16 (quạ). |
| `enemy_skeleton` | White Skeleton | 48-52 | 5 | 16x16 x2, 16x32 x2, 8x16 | 5 ô: [0][1] 2 khung xương 16x32, [2] xương dọc 8x16, [3][4] xương xoay chéo 16x16 (đạn). |
| `enemy_eagle` | Eagle | 53-54 | 2 | 32x32 x2 | 2 khung 32x32 (đại bàng vỗ cánh). |
| `enemy_skeledragon` | Skele-Dragon | 55-58 | 4 | 16x16 x2, 8x16 x2 | 4 ô: [0][1] 2 khung đầu rồng 16x16, [2] đốt xương 8x16, [3] cầu lửa đỏ 8x16. |
| `enemy_redskeleton` | Red Skeleton | 59-62 | 4 | 16x16 x2, 16x32 x2 | 4 ô: [0][1] 16x16 (xương tan/đang ráp lại), [2][3] 2 khung xương 16x32 (đi). |
| `enemy_axeman` | Axe-Man | 63-68 | 6 | 16x16 x4, 24x32 x2 | 6 ô: [0][1] 2 khung hiệp sĩ 24x32 (kèm khiên), [2..5] rìu xoay 16x16 x4. |
| `fx_death` | Death Effect | 69-71 | 3 | 8x16 x3 | 3 ô 8x16: giọt -> lửa nhỏ -> lửa lớn (dùng xếp thành khối theo cỡ enemy). |

**Ô / hiện tượng bất thường**

- Ô DÍNH LIỀN: `enemy_dragonskull_0` rect [40, 75, 16, 32]; gợi ý tách (toạ độ tương đối trong ô): [[0, 0, 16, 16], [0, 16, 16, 16]]
- Ô DÍNH LIỀN: `enemy_dragonskull_1` rect [57, 75, 16, 32]; gợi ý tách (toạ độ tương đối trong ô): [[0, 0, 16, 16], [0, 16, 16, 16]]
- Kiểm tra: pixel chữ trắng chạm pixel sprite: 0 (0 = nhãn không dính ô nào)
- Màu lạ (0,64,0): 516 px — chỉ nằm trong logo credit (bóng chữ), không phải màu sprite
- 27 ô có vùng xám key KHÉP KÍN nhỏ (tổng 128 px, ví dụ `enemy_zombie_0`, `enemy_zombie_3`, `enemy_zombie_4`): thường là lỗ trong sprite (vòng roi, quai bình...); key-out theo màu vẫn đúng. Chi tiết ở trường `key_khep_kin` của từng ô.
- Các ô 16x31 (`enemy_zombie_4`, `enemy_blackknight_4`, `enemy_blackknight_6`) có đỉnh thấp hơn 1 px nhưng đáy y+h = 60 thẳng hàng với ô 16x32 cùng hàng (không phải ô khác loại).
- Mọi ô là chữ nhật đặc (fill=1) — không có ô hợp tile như Simon.
- Nhóm có mảnh 16x16 ở tầng trên (Zombie, Fish Man, Black Knight): nhìn ảnh có vẻ là nửa thân/biến thể ghép thành khung 16x32 tầng dưới — ghi `loai=manh`, chưa chắc.

## 3. Bosses — `bosses` (415×292)

File nguồn: `NES - Castlevania - Enemies & Bosses - Bosses.png`

| Mục | Giá trị |
|---|---|
| Màu nền sheet | (0,128,0) (xanh lá) |
| Màu key (nền ô) | (116,116,116) (xám) |
| Màu chữ nhãn | (255,255,255) |
| Số màu khác nhau | 12 |
| Số nhãn chữ | 9 (9 nhãn nhóm + 0 nhãn tiêu đề) |
| Số nhóm | 9 |
| **Số ô** | **54** (loại: khung 30, đạn/vũ khí ném 12, hiệu ứng 9, mảnh 3) |
| Ô không đặc (hợp tile, fill<1) | 12 |
| Ô không có pixel xám | 2 |
| Ô dính liền (nhiều khung 1 ô) | 0 |
| Logo credit (bỏ qua) | rect [190,259,224,32] + 6 mảnh chữ bên trong |

**Histogram màu** (số pixel toàn sheet; vai trò): `#008000` (0,128,0) 66273 = nền sheet; `#000000` (0,0,0) 15611 = sprite; `#747474` (116,116,116) 15131 = màu key (nền ô xám); `#d82800` (216,40,0) 6975 = sprite; `#0070ec` (0,112,236) 5454 = sprite; `#fcd8a8` (252,216,168) 3984 = sprite; `#ffffff` (255,255,255) 2490 = chữ nhãn; `#fc74b4` (252,116,180) 2009 = sprite; `#cc88fc` (204,136,252) 1565 = sprite; `#fcc4fc` (252,196,252) 862 = sprite; `#004000` (0,64,0) 516 = chỉ có trong logo credit; `#fcfcfc` (252,252,252) 310 = sprite.

**Bố cục**: 5 hàng ô (Phantom Bat/Medusa/Mummy Man; Frankenstein & Igor/Grim Reaper/Dracula P1; Dracula P2 hai tầng; Death Effect + Unused).

**Bảng nhóm**:

| Nhóm (id) | Nhãn chữ | stt | Số khung | Kích thước điển hình | Ghi chú |
|---|---|---|---|---|---|
| `boss_phantombat` | Phantom Bat | 1-3 | 3 | 16x16, 32x32, 48x32 | 3 ô: [0] 16x16 (dơi gập, đen), [1] 48x32 dang cánh (không đặc, fill 0.67), [2] 32x32 (không đặc, fill 0.75). |
| `boss_medusa` | Medusa | 4-9 | 6 | 16x16 x2, 32x32 x2, 32x48 x2 | 6 ô: [0][1] 32x48 NỀN ĐEN (không có xám): 2 khung Medusa (mặt+thân), [2] 32x32 đầu tóc rắn xanh, [3] 32x32 đầu đá (xám-hồng), [4][5] 16x16 rắn xanh. |
| `boss_mummy` | Mummy Man | 10-17 | 8 | 16x48 x6, 16x16 x2 | 8 ô: [0..5] 6 khung 16x48 (3 khung tím rồi 3 khung đỏ), [6][7] 16x16 (dải băng quấn bay ra). |
| `boss_frankenstein` | Frankenstein & Igor | 18-23 | 6 | 16x48 x3, 16x16 x2, 8x16 | 6 ô: [0..2] Frankenstein 16x48 (3 khung đi), [3][4] Igor 16x16 (tông đỏ), [5] đạn 8x16 (cầu đỏ). |
| `boss_reaper` | Grim Reaper/Death | 24-28 | 5 | 16x16 x4, 40x48 | 5 ô: [0] 40x48 Tử thần (không đặc, fill 0.73), [1..4] 16x16 lưỡi hái xoay 4 khung. |
| `boss_dracula1` | Dracula (Phase 1) | 29-34 | 6 | 8x16 x2, 24x48, 24x59, 32x48, ... | 6 ô: [0] 24x48 và [1] 32x48 thân + áo choàng đỏ KHÔNG có đầu, [2] 8x16 đầu Dracula (mảnh để ghép lên thân), [3] 8x16 cầu lửa đỏ, [4] 24x59 và [5] 32x59 thân có đầu (không đặc, fill 0.86-0.88). |
| `boss_dracula2` | Dracula (Phase 2)/Cookie Monster | 35-43 | 9 | 48x64 x4, 48x80 x2, 16x16, 48x48, ... | 9 ô: [0..6] 7 khung thân quái vật (rộng 48, cao 48-80, không đặc fill 0.73-0.89, ghép nhiều tile), [7] 16x16 (bóng vuốt/lửa be-xanh, chưa chắc) và [8] 8x16 (cầu lửa đỏ). |
| `fx_bossdeath` | Death Effect | 44-52 | 9 | 24x32 x3, 24x48 x3, 8x16 x3 | 9 ô gồm 3 cỡ khối hiệu ứng: 3 ô 8x16 (đơn), 3 ô 24x32 (2 hàng tile), 3 ô 24x48 (3 hàng tile) - mỗi cỡ 3 khung giọt -> lửa nhỏ -> lửa lớn (khối ghép sẵn nhiều tile, mỗi ô là 1 khung). |
| `boss_unused` | Unused | 53-54 | 2 | 16x16 x2 | 2 ô 16x16 không dùng trong game: bàn tay/cẳng tay be-xanh (chưa rõ thuộc nhân vật nào). |

**Ô / hiện tượng bất thường**

- 2 ô KHÔNG có pixel xám: `boss_medusa_0` [104, 11, 32, 48] viền #000000; `boss_medusa_1` [137, 11, 32, 48] viền #000000. Toàn bộ chữ nhật là pixel đặc: cắt nguyên chữ nhật, không key-out.
- Kiểm tra: pixel chữ trắng chạm pixel sprite: 0 (0 = nhãn không dính ô nào)
- Màu lạ (0,64,0): 516 px — chỉ nằm trong logo credit (bóng chữ), không phải màu sprite
- Màu lạ (252,252,252): 310 px — trắng của sprite (252,252,252) KHÁC trắng chữ nhãn (255,255,255): không bị tách nhầm vào nhãn (đã kiểm: chữ nhãn không chạm sprite)
- 12 ô không đặc (fill<1) trong các nhóm: `boss_phantombat` 2, `boss_reaper` 1, `boss_dracula1` 2, `boss_dracula2` 7. Ô là hợp tile 8x16: bbox đúng nhưng có góc trống màu xanh; khi cắt giữ nguyên bbox, coi pixel xanh/xám là trong suốt.
- 23 ô có vùng xám key KHÉP KÍN nhỏ (tổng 196 px, ví dụ `boss_phantombat_0`, `boss_phantombat_1`, `boss_phantombat_2`): thường là lỗ trong sprite (vòng roi, quai bình...); key-out theo màu vẫn đúng. Chi tiết ở trường `key_khep_kin` của từng ô.
- 2 ô Medusa `boss_medusa_0/1` (32x48) có NỀN ĐEN thật (đen là pixel đặc, không phải key).
- Dracula (Phase 2) và Phantom Bat là ô hợp nhiều tile (fill 0.67-0.89, cao tới 80 px): giữ nguyên bbox.
- Death Effect có 3 cỡ khối ghép sẵn (8x16, 24x32, 24x48): mỗi ô là 1 khung của cỡ đó, không phải nhiều khung.

## 4. Items & Objects — `items_objects` (251×135)

File nguồn: `NES - Castlevania - Miscellaneous - Items & Objects.png`

| Mục | Giá trị |
|---|---|
| Màu nền sheet | (0,128,0) (xanh lá) |
| Màu key (nền ô) | (116,116,116) (xám) |
| Màu chữ nhãn | (255,255,255) |
| Số màu khác nhau | 13 |
| Số nhãn chữ | 1 (1 nhãn nhóm + 0 nhãn tiêu đề) |
| Số nhóm | 10 |
| **Số ô** | **65** (loại: vật phẩm 33, vật thể 30, đạn/vũ khí ném 2) |
| Ô không đặc (hợp tile, fill<1) | 0 |
| Ô không có pixel xám | 8 |
| Ô dính liền (nhiều khung 1 ô) | 1 |
| Logo credit (bỏ qua) | rect [26,102,224,32] + 6 mảnh chữ bên trong |

**Histogram màu** (số pixel toàn sheet; vai trò): `#008000` (0,128,0) 13895 = nền sheet; `#747474` (116,116,116) 5972 = màu key (nền ô xám); `#000000` (0,0,0) 4652 = sprite; `#d82800` (216,40,0) 3120 = sprite; `#fc74b4` (252,116,180) 1526 = sprite; `#fcd8a8` (252,216,168) 1373 = sprite; `#7c0800` (124,8,0) 842 = sprite; `#fc9838` (252,152,56) 628 = sprite; `#cc88fc` (204,136,252) 527 = sprite; `#004000` (0,64,0) 516 = chỉ có trong logo credit; `#0070ec` (0,112,236) 496 = sprite; `#fcc4fc` (252,196,252) 205 = sprite; `#ffffff` (255,255,255) 133 = chữ nhãn.

**Bố cục**: lưới 17 px (ô 16 px + 1 px khe), 6 hàng ô: hàng y=1 (item cơ bản, nến, đuốc, 2 cửa), y=18 (vũ khí phụ + II/III), y=35 (đồ ăn, vương miện, rương), y=52 (8 cặp khối), y=69 (mảnh số điểm + gạch), y=83 (Unused). Sheet **chỉ có một nhãn chữ** ("Unused"): nhóm/tên của các ô còn lại do nhìn ảnh gán thủ công trong `sheet_specs.py`.

**Bảng nhóm**:

| Nhóm (id) | Nhãn chữ | stt | Số khung | Kích thước điển hình | Ghi chú |
|---|---|---|---|---|---|
| `item_misc` | (gán tay theo toạ độ) | 1-8 | 8 | 16x16 x7, 8x16 | Vật phẩm cơ bản (roi, tim, túi tiền, pha lê): hàng y=1, x<128. |
| `obj_candle` | (gán tay theo toạ độ) | 9-10 | 2 | 8x16 x2 | Nến đôi 2 khung (8x16). |
| `obj_torch` | (gán tay theo toạ độ) | 11-12 | 2 | 16x32 x2 | Chân đèn lửa 2 khung (16x32). |
| `obj_door` | (gán tay theo toạ độ) | 13-14 | 2 | 16x48, 24x48 | 2 ô cửa/cổng cao 48 px (một ô vòm 16 rộng, một ô 24 rộng). |
| `item_weapon` | (gán tay theo toạ độ) | 15-23 | 9 | 16x16 x9 | Vũ khí phụ dạng vật phẩm (dao, rìu, nước thánh, thập tự, đồng hồ) + II/III. |
| `item_treasure` | (gán tay theo toạ độ) | 24-35 | 12 | 16x16 x12 | Đồ ăn/bình/rosary/1UP + vương miện (4 bảng màu) + rương (4 bảng màu). |
| `obj_block` | (gán tay theo toạ độ) | 36-51 | 16 | 16x16 x8, 8x16 x8 | 8 cặp [khối 8x16 'U' hoặc '!'][hình nghiêng 16x16] x 4 bảng màu; chưa rõ tên vật thể (sheet không có nhãn). |
| `obj_score` | (gán tay theo toạ độ) | 52-58 | 7 | 8x16 x7 | 7 mảnh 8x16 (chữ số điểm + 1 viên tím); chữ chỉ ở nửa trên ô. |
| `obj_brick` | (gán tay theo toạ độ) | 59-59 | 1 | 32x16 | 1 ô 32x16 chứa 4 khối gạch nâu dính liền. |
| `item_unused` | Unused | 60-65 | 6 | 16x16 x5, 8x16 | Nhãn 'Unused' trong sheet: 6 vật phẩm không dùng. |

**Ô / hiện tượng bất thường**

- 8 ô KHÔNG có pixel xám: `obj_block_u_orange` [1, 52, 8, 16] viền #7c0800; `obj_block_bang_orange` [27, 52, 8, 16] viền #7c0800; `obj_block_u_purple` [53, 52, 8, 16] viền #000000; `obj_block_bang_purple` [79, 52, 8, 16] viền #000000; `obj_block_u_tan` [105, 52, 8, 16] viền #000000; `obj_block_bang_tan` [131, 52, 8, 16] viền #000000; `obj_block_u_red` [157, 52, 8, 16] viền #000000; `obj_block_bang_red` [183, 52, 8, 16] viền #000000. Toàn bộ chữ nhật là pixel đặc: cắt nguyên chữ nhật, không key-out.
- Ô DÍNH LIỀN: `obj_brick_set` rect [64, 69, 32, 16]; gợi ý tách (toạ độ tương đối trong ô): [[0, 0, 8, 16], [8, 0, 8, 16], [16, 0, 8, 16], [24, 0, 8, 16]]
- Kiểm tra: pixel chữ trắng chạm pixel sprite: 0 (0 = nhãn không dính ô nào)
- Màu lạ (0,64,0): 516 px — chỉ nằm trong logo credit (bóng chữ), không phải màu sprite
- 18 ô có vùng xám key KHÉP KÍN nhỏ (tổng 123 px, ví dụ `item_whip`, `obj_candle_0`, `obj_candle_1`): thường là lỗ trong sprite (vòng roi, quai bình...); key-out theo màu vẫn đúng. Chi tiết ở trường `key_khep_kin` của từng ô.
- 8 ô không xám là các khối 8x16 `obj_block_u_*` / `obj_block_bang_*` (nhóm `obj_block`): sprite phủ kín ô, viền đỏ sẫm hoặc đen.
- `obj_brick_set` (64,69,32,16): 4 khối gạch 8x16 dính liền không có biên xanh; giữ 1 ô, gợi ý tách 4 ô 8x16 theo x+0/8/16/24.
- Tên chưa chắc: `item_stopwatch` (hình giống đồng hồ, đoán), 16 ô `obj_block_*`/`obj_lean_*` (khối 'U'/'!' + hình nghiêng, chưa rõ vật thể), `obj_purple_orb`, `item_unused_0`, các `obj_score_*` (chữ số điểm chỉ là mảnh: 10/20/40/70 ghép với 0 hoặc 00).
- `obj_door_arch` (16x48) và `obj_door_wide` (24x48) là sprite cửa/cổng cao 48 px; có xám ở góc trên nên vẫn là ô thường.

## 5. Level 1 — `level1_entrance` (3601×411)

File nguồn: `NES - Castlevania - Stages - Level 1_ Entrance.png`

### 5.1 Vùng bản đồ

**Histogram màu** (số pixel toàn sheet; vai trò): `#008000` (0,128,0) 689108 = nền sheet (ngoài vùng bản đồ); `#000000` (0,0,0) 226452 = đen: nền tối/ngoài trời, hàng đáy sân, bóng, vân tường; `#bcbcbc` (188,188,188) 202590 = xám sáng: tường đá Entrance Hall/Phantom Bat, thân trụ; `#747474` (116,116,116) 177399 = xám giữa: gạch tường Crypt, vân đá, bóng trụ; `#c84c0c` (200,76,12) 57535 = cam gạch: khối đứng được, bậc thang, tường gạch cam; `#24188c` (36,24,140) 54160 = xanh dương đậm: trời sân, kính cửa sổ, nước Crypt B; `#004400` (0,68,0) 31445 = xanh lá đậm: tán cây, khung cửa sổ, rêu; `#009400` (0,148,0) 22446 = xanh lá nhạt: tán cây, khung cửa sổ, rêu; `#d82800` (216,40,0) 12553 = đỏ: rèm, cột cửa, logo; `#fc74b4` (252,116,180) 4551 = hồng: bóng sáng rèm, logo; `#fcfcfc` (252,252,252) 1256 = trắng: chữ nhãn (ngoài vùng) và viền cột cửa (trong vùng); `#004000` (0,64,0) 516 = bóng chữ logo credit.

Sheet gồm **5 vùng chữ nhật ĐẶC** (diện tích = w×h, không có pixel nền xanh bên trong) + logo credit [3376, 378, 224, 32]. Không có mảnh rời nào khác: hai "mảnh nhỏ" nhìn thấy ở góc dưới-trái của ảnh crop vùng Phantom Bat chính là mép phải của Crypt A và Crypt B.

| Vùng (id) | Nhãn chữ | rect [x,y,w,h] | Khối 16 (cột×hàng) | Màn 256 px | Hàng sàn chính | Tile 8×8 duy nhất | Metatile 16×16 duy nhất | Ô 16×16 đồng màu |
|---|---|---|---|---|---|---|---|---|
| `castle_entrance` | Castle Entrance | [1, 11, 768, 192] | 48×12 | 3 | 10 (y_local 160) | 49 | 50 | 176 |
| `entrance_hall` | Entrance Hall | [774, 11, 1536, 192] | 96×12 | 6 | 11 (y_local 176) | 57 | 71 | 0 |
| `crypt_a` | Crypt A | [2315, 11, 512, 192] | 32×12 | 2 | 11 (y_local 176) | 34 | 36 | 11 |
| `crypt_b` | Crypt B | [2315, 218, 512, 192] | 32×12 | 2 | 6 (y_local 96) | 36 | 27 | 86 |
| `phantom_bat` | Phantom Bat | [2832, 11, 768, 192] | 48×12 | 3 | 11 (y_local 176) | 68 | 78 | 0 |

- Chiều cao mọi vùng = **192 px = 12 khối 16 px** (không phải 176); chiều rộng 768/1536/512/512/768 đều là **bội của 16 (và của 256)**: tổng 256 khối = 16 màn.
- Hàng sàn chính: hàng 11 (y_local 176) ở Entrance Hall, Crypt A, Phantom Bat; hàng 10 (y_local 160) ở Castle Entrance (hàng 11 là nền đen); hàng 6 (y_local 96) ở Crypt B (nước ở hàng 10-11).
- Nhãn chữ: "Castle Entrance" [1, 1, 100, 7]; "Entrance Hall" [774, 1, 83, 7]; "Crypt A" [2315, 1, 47, 8]; "Phantom Bat" [2832, 1, 77, 7]; "Crypt B" [2315, 208, 47, 8].

### 5.2 Lưới và thống kê tile

Thử mọi offset (0..15, 0..15) cho khối 16×16 và (0..7, 0..7) cho tile 8×8, đếm số khối duy nhất; offset đúng là offset cho ÍT khối duy nhất nhất.

| Vùng | Offset đúng 16×16 | Duy nhất ở offset đó | Min ở offset khác | Offset đúng 8×8 | Duy nhất ở offset đó | Min ở offset khác |
|---|---|---|---|---|---|---|
| `castle_entrance` | (0, 0) | 50 / 576 | 71 | (0, 0) | 49 / 2304 | 107 |
| `entrance_hall` | (0, 0) | 71 / 1152 | 127 | (0, 0) | 57 / 4608 | 198 |
| `crypt_a` | (0, 0) | 36 / 384 | 57 | (0, 0) | 34 / 1536 | 78 |
| `crypt_b` | (0, 0) | 27 / 384 | 48 | (0, 0) | 36 / 1536 | 80 |
| `phantom_bat` | (0, 0) | 78 / 576 | 136 | (0, 0) | 68 / 2304 | 218 |

**Kết luận: cả 5 vùng căn đúng lưới 16×16 và 8×8 ở offset (0,0) so với góc trên-trái vùng** (offset khác cho nhiều khối duy nhất hơn rõ rệt).

**Toàn Level 1** (chỉ 5 vùng, đã bỏ nền xanh; khớp từng pixel, gồm cả màu):

| Đại lượng | Giá trị |
|---|---|
| Tổng tile 8×8 / metatile 16×16 | 12288 / 3072 |
| **Tile 8×8 duy nhất** | **118** (còn 113 sau khi gộp lật ngang/dọc) |
| **Metatile 16×16 duy nhất** | **164** (gộp lật vẫn 164) |

Theo vùng (cột "mới" = chưa xuất hiện ở các vùng đứng trước theo thứ tự sheet):

| Vùng | Tổng 8×8 | Duy nhất 8×8 | Mới 8×8 | Tổng 16×16 | Duy nhất 16×16 | Mới 16×16 |
|---|---|---|---|---|---|---|
| `castle_entrance` | 2304 | 49 | 49 | 576 | 50 | 50 |
| `entrance_hall` | 4608 | 57 | 43 | 1152 | 71 | 67 |
| `crypt_a` | 1536 | 34 | 8 | 384 | 36 | 22 |
| `crypt_b` | 1536 | 36 | 7 | 384 | 27 | 16 |
| `phantom_bat` | 2304 | 68 | 11 | 576 | 78 | 9 |

### 5.3 Liên kết giữa các vùng (theo bố cục sheet + hình vẽ)

Bố cục sheet: Castle Entrance → Entrance Hall → Crypt A → Phantom Bat đặt kề nhau theo hàng ngang (khe xanh 5 px), Crypt B nằm ngay dưới Crypt A (cùng x, khe 15 px).

| Mã | Từ | Tới | Nối qua | Độ tin cậy | Ghi chú |
|---|---|---|---|---|---|
| c1 | `castle_entrance` (mép phải) | `entrance_hall` (mép trái) | vòm cổng trong tháp ở cuối sân (cột 43, hàng 4-9) rồi tới mép phải sân (x=768) | trung bình (suy luận từ bố cục + hình vẽ) | 2 vùng đặt kề nhau trên sheet (khe xanh 5 px); không có cột cửa ở mép phải sân. |
| c2 | `entrance_hall` (mép phải) | `crypt_a` (mép trái) | cột cửa đỏ-trắng ở mép phải Hall (cột 95, hàng 2-4) đứng trên bục hàng 5 (cột 87-95, y_local 80) <-> khoảng đen 16x48 ở mép trái Crypt A (cột 0, hàng 2-4) trên bục hàng 5 (cột 0-7) | cao (cùng độ cao y sheet 43..90 ở 2 bên, đo bằng pixel) | Hall: bục hàng 5 cuối bản đồ dẫn tới cửa. |
| c3 | `crypt_a` (mép phải) | `phantom_bat` (mép trái) | cột cửa đỏ-trắng ở mép phải Crypt A (cột 31, hàng 2-4) đứng trên bục hàng 5 (cột 26-31) -> bục hàng 5 (cột 0-6) ở mép trái Phantom Bat (cùng y sheet 91) | trung bình (mép trái Phantom Bat không có khoảng đen, chỉ khớp độ cao bục) |  |
| c4 | `crypt_a` (mép dưới) | `crypt_b` (mép trên) | 2 giếng cầu thang ở sàn Crypt A (cột 3 và 23, hàng 11) <-> 2 cầu thang có đỉnh sát mép trên Crypt B (cột 2 và 22) | cao (bậc thang khớp sau khi dịch ngang) | bậc thang kéo dài từ A xuống B với độ lệch ngang x_B = x_A + shift (cùng giá trị cho cả 2 cầu thang). Dịch ngang đo được: crypt_a_stair_0->crypt_b_stair_0 = -32 px, crypt_a_stair_2->crypt_b_stair_1 = -32 px |
| c5 | `phantom_bat` (mép phải) | (không có vùng nối tiếp) | cột cửa đỏ-trắng ở mép phải (cột 47, hàng 4-6) đứng trên bục hàng 7 (cột 45-47) | trung bình | cuối Level 1 (phòng boss Phantom Bat); chưa thấy vùng nào nối tiếp trên sheet. |
| c6 | `crypt_b` (mép phải) | (không có vùng nối tiếp) | dải khung cửa (gạch nhỏ cam) ở mép phải, KHÔNG có cột đỏ-trắng | thấp | Crypt B chỉ thấy lối vào bằng 2 cầu thang từ Crypt A; mép phải không có vùng nối tiếp trên sheet. |

**Cột cửa đỏ-trắng** (viền trắng (252,252,252), cùng một hình ở 3 vùng; đứng trên một bục gạch cam):

| Vùng | rect sheet [x,y,w,h] | Khối (cột; hàng) | Đứng trên bục |
|---|---|---|---|
| `entrance_hall` | [2302, 43, 8, 48] | 95; 2-4 | hàng 5, cột 87-95 |
| `crypt_a` | [2819, 43, 8, 48] | 31; 2-4 | hàng 5, cột 26-31 |
| `phantom_bat` | [3592, 75, 8, 48] | 47; 4-6 | hàng 7, cột 45-47 |

- Vòm cổng lâu đài trong tháp (Castle Entrance): rect sheet [689, 75, 16, 96] (cột 43, hàng 4-9) — vòm cổng lâu đài: ô đen rộng 16 px trong tháp (cột 43, hàng 4-9, đỉnh vòm ở hàng 4); nhìn ảnh, nhiều khả năng trang trí.
- Khoảng đen ở mép trái Crypt A: rect sheet [2315, 43, 16, 48] — khoảng đen 16x48 ở mép trái Crypt A (cột 0, hàng 2-4), cùng độ cao với cột cửa ở mép phải Entrance Hall.
- Dải khung cửa (chuỗi gạch cam nhỏ 6×5 xếp dọc ở sát mép): `entrance_hall` mép phải x_local 1529 y_local [1, 30]; `entrance_hall` mép phải x_local 1529 y_local [97, 174]; `crypt_a` mép trái x_local 1 y_local [1, 30]; `crypt_a` mép trái x_local 1 y_local [97, 174]; `crypt_a` mép phải x_local 505 y_local [97, 174]; `crypt_b` mép phải x_local 505 y_local [1, 126]; `phantom_bat` mép phải x_local 761 y_local [1, 62]; `phantom_bat` mép phải x_local 761 y_local [129, 174].

### 5.4 Loại khối (cho planner làm va chạm; không tuyệt đối)

Bản đồ ký tự theo metatile 16×16 (hàng = y/16, cột = x/16 trong vùng):

| Ký hiệu | Ý nghĩa | `castle_entrance` | `entrance_hall` | `crypt_a` | `crypt_b` | `phantom_bat` |
|---|---|---|---|---|---|---|
| `B` | gạch cam 16x16 (đứng được) | 0 | 124 | 50 | 40 | 71 |
| `F` | sàn sân Castle Entrance (hàng 10) | 48 | 0 | 0 | 0 | 0 |
| `U` | ô có bậc thang đi lên sang phải (↗) | 0 | 12 | 0 | 0 | 2 |
| `L` | ô có bậc thang đi lên sang trái (↖) | 0 | 2 | 10 | 10 | 6 |
| `W` | nước (Crypt B, chết) | 0 | 0 | 0 | 60 | 0 |
| `K` | tường gạch cam dạng lưới (có thể phá) | 0 | 0 | 24 | 0 | 0 |
| `G` | song/lưới sắt (trang trí) | 0 | 0 | 0 | 8 | 0 |
| `D` | cột cửa đỏ-trắng | 0 | 3 | 3 | 0 | 3 |
| `R` | lan can/rào (trang trí) | 14 | 8 | 4 | 0 | 10 |
| `.` | nền/trang trí khác | 514 | 1003 | 293 | 266 | 484 |

**Gạch cam (B/F, đứng được)** — khối 16×16 màu cam (200,76,12); các đoạn liền nhau theo hàng:

- `castle_entrance`: hàng 10 cột 0-47 (48 khối) [sàn chính]
- `entrance_hall`: hàng 5 cột 47-56 (10 khối); hàng 5 cột 87-95 (9 khối); hàng 7 cột 43-45 (3 khối); hàng 7 cột 58-63 (6 khối); hàng 11 cột 0-95 (96 khối) [sàn chính]
- `crypt_a`: hàng 1 cột 30-31 (2 khối); hàng 5 cột 0-7 (8 khối); hàng 5 cột 26-31 (6 khối); hàng 6 cột 20-21 (2 khối); hàng 7 cột 8-10 (3 khối); hàng 7 cột 22-24 (3 khối); hàng 11 cột 0-2 (3 khối) [sàn chính]; hàng 11 cột 4-15 (12 khối) [sàn chính]; hàng 11 cột 20-22 (3 khối) [sàn chính]; hàng 11 cột 24-31 (8 khối) [sàn chính]
- `crypt_b`: hàng 0 cột 0-1 (2 khối); hàng 0 cột 6-7 (2 khối); hàng 0 cột 12-13 (2 khối); hàng 0 cột 16-17 (2 khối); hàng 4 cột 6-7 (2 khối); hàng 6 cột 0-13 (14 khối) [sàn chính]; hàng 6 cột 16-17 (2 khối) [sàn chính]; hàng 6 cột 20-29 (10 khối) [sàn chính]; hàng 8 cột 30-31 (2 khối); hàng 10 cột 28-29 (2 khối)
- `phantom_bat`: hàng 5 cột 0-6 (7 khối); hàng 7 cột 8-18 (11 khối); hàng 7 cột 45-47 (3 khối); hàng 9 cột 42-43 (2 khối); hàng 11 cột 0-47 (48 khối) [sàn chính]

**Cầu thang** — nhận ra bằng vệt cam 8×2 lặp mỗi 8 px theo đường chéo (mỗi bậc 8×8 px; `chân`/`đỉnh` là toạ độ góc trên-trái vạch cam của bậc thấp nhất/cao nhất, theo toạ độ SHEET). ↗ = đi lên sang phải (chân ở trái-dưới); ↖ = đi lên sang trái (chân ở phải-dưới):

| Id | Hướng | Số bậc | Chân (x,y) | Đỉnh (x,y) | rect sheet [x,y,w,h] | Cao (px) | Ghi chú |
|---|---|---|---|---|---|---|---|
| `entrance_hall_stair_0` | ↗ | 8 | (1398,179) | (1454,123) | [1398, 123, 64, 64] | 64 |  |
| `entrance_hall_stair_1` | ↗ | 4 | (1494,115) | (1518,91) | [1494, 91, 32, 32] | 32 |  |
| `entrance_hall_stair_2` | ↖ | 4 | (1710,115) | (1686,91) | [1686, 91, 32, 32] | 32 |  |
| `entrance_hall_stair_3` | ↗ | 12 | (2070,179) | (2158,91) | [2070, 91, 96, 96] | 96 |  |
| `crypt_a_stair_0` | ↖ | 2 | (2371,195) | (2363,187) | [2363, 187, 16, 16] | 16 | chân chạm mép DƯỚI vùng (bậc thang bị cắt, nối xuống vùng phía dưới) |
| `crypt_a_stair_1` | ↖ | 8 | (2547,179) | (2491,123) | [2491, 123, 64, 64] | 64 |  |
| `crypt_a_stair_2` | ↖ | 2 | (2691,195) | (2683,187) | [2683, 187, 16, 16] | 16 | chân chạm mép DƯỚI vùng (bậc thang bị cắt, nối xuống vùng phía dưới) |
| `crypt_a_stair_3` | ↖ | 8 | (2771,179) | (2715,123) | [2715, 123, 64, 64] | 64 |  |
| `crypt_b_stair_0` | ↖ | 8 | (2403,274) | (2347,218) | [2347, 218, 64, 64] | 64 | đỉnh chạm mép TRÊN vùng (nối lên vùng phía trên) |
| `crypt_b_stair_1` | ↖ | 12 | (2755,306) | (2667,218) | [2667, 218, 96, 96] | 96 | đỉnh chạm mép TRÊN vùng (nối lên vùng phía trên) |
| `phantom_bat_stair_0` | ↖ | 4 | (2968,115) | (2944,91) | [2944, 91, 32, 32] | 32 |  |
| `phantom_bat_stair_1` | ↖ | 8 | (3192,179) | (3136,123) | [3136, 123, 64, 64] | 64 |  |
| `phantom_bat_stair_2` | ↗ | 4 | (3520,147) | (3544,123) | [3520, 123, 32, 32] | 32 |  |

**Nước (chết)** — Crypt B hàng 10-11 (mặt nước từ y_local 160): rect sheet [2315, 378, 448, 32] (56 khối); rect sheet [2795, 378, 32, 32] (4 khối).

**Mép vực** (khoảng trống trên hàng sàn chính):

- `crypt_b` hàng 6 cột 14-15 (rộng 32 px, mép trái x_local 224, mép phải x_local 256): vực: khe trống 32 px giữa 2 đoạn sàn hàng 6, bên dưới là nước (rơi xuống là chết)
- `crypt_b` hàng 6 cột 18-19 (rộng 32 px, mép trái x_local 288, mép phải x_local 320): vực: khe trống 32 px (có bục nổi 2 khối ở cột 16-17 bên trái), bên dưới là nước (rơi xuống là chết)
- `crypt_b` hàng 6 cột 30-31 (rộng 32 px, mép trái x_local 480, mép phải x_local 512): KHÔNG phải vực tự do: cột 30-31 là trụ đá + dải khung cửa (mép phải vùng), không có sàn ở hàng 6

**Trang trí / khác**:

- Tường gạch cam dạng lưới (K, có thể là tường phá được; sheet không có nhãn): `crypt_a` rect sheet [2571, 107, 64, 96] (24 khối, có 3 khe đen).
- Song/lưới sắt (G, trang trí, ở sau nước): rect sheet [2475, 346, 32, 32]; rect sheet [2667, 346, 32, 32].
- Lan can/rào lưới (R, trang trí): 16 cụm, nhận bằng so khớp khối mẫu (rào sân ở hàng 9 Castle Entrance; lan can trên bục/cầu thang ở Hall, Crypt A, Phantom Bat). Lan can chạy dọc cầu thang nằm trong các khối U/L.
- Không thấy gai/hố lửa; rêu/cỏ gai xanh ở Crypt B (hàng 7 và 9) là trang trí; trụ đá, rèm đỏ, cửa sổ xanh, mảng gạch xám là nền.

**Bản đồ ký tự** (mỗi dòng = 1 hàng khối; B=gạch cam, F=sàn sân, U/L=bậc thang, W=nước, K=tường gạch, G=song sắt, D=cột cửa, R=lan can, .=nền):

`castle_entrance` (48 cột):

```
 0 ................................................
 1 ................................................
 2 ................................................
 3 ................................................
 4 ................................................
 5 ................................................
 6 ................................................
 7 ................................................
 8 ................................................
 9 ....RR....RR..RR....RR....RR..RRRR..............
10 FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
11 ................................................
```

`entrance_hall` (96 cột):

```
 0 ................................................................................................
 1 ................................................................................................
 2 ...............................................................................................D
 3 ...............................................................................................D
 4 ....................................................RRRR................................RR.....D
 5 ..............................................UBBBBBBBBBBL............................UBBBBBBBBB
 6 .............................................U............L...RR.....................U..........
 7 ..........................................UBBB............BBBBBB....................U...........
 8 .........................................U.........................................U............
 9 ........................................U.........................................U.............
10 .......................................U.........................................U..............
11 BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
```

`crypt_a` (32 cột):

```
 0 ................................
 1 ..............................BB
 2 ...............................D
 3 ...............................D
 4 ...............................D
 5 BBBBBBBB..................BBBBBB
 6 ..........R.....KKKKBB..R.......
 7 ........BBBL....KKKK..BBBL......
 8 ............L...KKKK......L.....
 9 .............L..KKKK.......L....
10 ..R...........L.KKKK..R.....L...
11 BBBLBBBBBBBBBBBBKKKKBBBLBBBBBBBB
```

`crypt_b` (32 cột):

```
 0 BBL...BB....BB..BB....L.........
 1 ...L...................L........
 2 ....L...................L.......
 3 .....L...................L......
 4 ......BB..................L.....
 5 ...........................L....
 6 BBBBBBBBBBBBBB..BB..BBBBBBBBBB..
 7 ................................
 8 ..........GG..........GG......BB
 9 ..........GG..........GG........
10 WWWWWWWWWWWWWWWWWWWWWWWWWWWWBBWW
11 WWWWWWWWWWWWWWWWWWWWWWWWWWWW..WW
```

`phantom_bat` (48 cột):

```
 0 ................................................
 1 ................................................
 2 ................................................
 3 ................................................
 4 ..RRRR.........................................D
 5 BBBBBBBL.......................................D
 6 ........L.RR..RRRR.............................D
 7 ........BBBBBBBBBBBL........................UBBB
 8 ....................L......................U....
 9 .....................L....................BB....
10 ......................L.........................
11 BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
```

Ảnh `level1_entrance_annot.png` (1×) tô loại khối; `level1_entrance_annot_1..4.png` (2×) có lưới 16 px, chỉ số cột/hàng, ký tự loại khối, đường nối các bậc thang và khung nhãn.

## 6. Điểm cần lưu ý khi cắt (tóm tắt)

1. Cắt theo `rect` trong JSON; xoá màu key xám (116,116,116) và xanh nền (0,128,0) trong bbox thành trong suốt; **giữ pixel đen**. Chỉ 2 ô Medusa (nền đen) và 8 khối 8x16 ở sheet items không có xám.
2. Bỏ logo credit (224x32) và mọi chữ nhãn; không ô nào chạm chữ nhãn (đã kiểm 0 pixel chạm).
3. Simon: ô roi ghép thân + roi (rộng 44/60 cho roi duỗi); pivot chân = [`pivot_goi_y`] (x = giữa cột chân, y = đáy slot 32 px = `baseline_y` của hàng), không phải pixel thấp nhất; Simon quay TRÁI; các ô thò roi 33-34 px cao hơn slot.
4. Hai ô `enemy_dragonskull_0/1` và `obj_brick_set` là ô DÍNH LIỀN nhiều khung (có `goi_y_tach`); mọi ô khác đã tách đúng bằng khe 1 px.
5. Nhiều ô Simon/boss không phải chữ nhật đặc (hợp tile 8x16): đừng cắt theo lưới đều; dùng đúng bbox từng ô.
6. Một số ô có vùng xám khép kín nhỏ (<= 37 px): là lỗ trong sprite (vòng roi, quai bình...), key-out bằng màu vẫn đúng.
7. Level 1: 5 vùng chữ nhật đặc, cao 192 (12 khối 16 px), rộng bội của 256; lưới 16x16/8x8 căn đúng offset (0,0) từ góc trên-trái từng vùng; khe xanh giữa vùng 5 px (ngang) / 15 px (dọc); cắt theo `rect` của `regions`.

## 7. Nhóm / tên không chắc (cần người xem lại nếu quan trọng)

| Nội dung | Mức chắc | Lý do |
|---|---|---|
| `enemy_zombie_0..2`, `enemy_fishman_0..4`, `enemy_blackknight_0..3` (tầng trên 16x16) | trung bình | nhìn giống nửa thân/biến thể; có thể là khung độc lập |
| `item_stopwatch` | thấp | hình khác đồng hồ NES quen thuộc; là ô duy nhất còn lại trong hàng vũ khí |
| `obj_block_*` / `obj_lean_*` (16 ô hàng y=52) | thấp | không có nhãn, chưa rõ vật thể (4 bảng màu x 2 kiểu) |
| `obj_purple_orb`, `item_unused_0`, `boss_unused_*`, `boss_dracula2_7` | thấp | hình lạ, không có nhãn rõ |
| `obj_score_*` | trung bình | mảnh chữ số; cách ghép thành 100/200/.../2000 là suy luận |
| `simon_use_*` ô [2..5] (4 màu vật cầm) | trung bình | chưa đối chiếu với từng cấp roi trong game |
| `simon_whippart_*`: gán L2 / L3 màu | cao | đã khớp pixel với các ô roi (mảnh treo/vắt); mảnh duỗi gán theo màu và độ dài |
| Liên kết vùng Level 1 (c1, c3, c5, c6) | trung bình/thấp | suy luận từ bố cục và hình vẽ; xem `connections` |

