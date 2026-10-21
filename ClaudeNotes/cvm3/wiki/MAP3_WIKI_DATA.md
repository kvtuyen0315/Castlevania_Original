# MAP3_WIKI_DATA — Wiki cho Map 3 (Block 3: Stage 7–9)

> Planner CVM3, 17/10/2026. **Không lặp lại** dữ liệu đã có trong `ClaudeNotes/cvgp/wiki/CV_WIKI_DATA.md` — file này là chỉ mục + phần
> chi tiết riêng Stage 7–9. Nguồn: StrategyWiki bị Cloudflare chặn ⇒ bản Wayback `id_` (snapshot **2026-03-05**, revision 748600, trang sửa lần cuối
> 23/06/2015) — văn bản thô `ClaudeNotes/cvgp/_cache/wiki_raw/text_Stages_7_-_9.txt` (+ `text_Walkthrough.txt`, `text_Enemies.txt`, `text_Bosses.txt`,
> `text_Items.txt`, `text_Secrets.txt`), KHÔNG commit (bản quyền). Ảnh bản đồ nửa tỉ lệ + ảnh màn (cdn.wikimg.net tải thẳng được, planner tải 17/10):
> `ClaudeNotes/cvm3/_cache/wiki_img/Castlevania_Stage_{7,8,9}.png` (384×176, 632×176, 768×88 = **nửa tỉ lệ**: 1 px ảnh = 2 px NES) +
> `Castlevania_Stage_{7,8,9}_screen.png` (256×240 nguyên tỉ lệ, có HUD); ghép xem nhanh `_cache/stages789_x2.png` (×2 = đúng tỉ lệ NES), `_cache/screens789.png`.
> **Ưu tiên (H1):** video `Map_3.mp4` là chuẩn vị trí/thời gian; wiki chỉ để đối chiếu nội dung (vật phẩm, bí mật, loại enemy) và chỉ số video không đo được.

## §1 Chỉ mục vào CV_WIKI_DATA (đã có — đọc ở đó)
| Cần | Mục CV_WIKI_DATA |
|---|---|
| Sát thương Simon Stage 7–12 = **3 vạch/đòn**, "6 đòn/mạng" (đòn thứ 6 chết: 6×3 = 18 ≥ 16) — Unity đã có `ConfigBlock` block 3 `damageToSimon` 3 | §1.3 |
| Enemy Block 3: **Raven** (E04, 1/1, 200), **Skeleton trắng** (E08, 1/1, 300, ném xương vòng cung 2–3 cái), **Ghost** (E09, 3/2, 300), **Fleaman** (E13, 1/1, **500**), Medusa Head (E07, 1/1, 300), Bone Tower (E12, 8/6, 400) | §3.1 bảng, §3.3 đạn, §3.4 hành vi |
| Enemy theo stage: **St7** Ghost, Fleaman, Skeleton, Raven · **St8** Medusa Head + Skeleton (tầng dưới), Bone Tower + Raven (tầng trên) · **St9** Raven, Skeleton, Bone Tower, Medusa Head | §3.2 |
| **Mummy Men** (B3): 2 Mummy, **48/32 đòn** (wiki không nói tổng hay mỗi con — suy luận tổng), 3000 đ, đi lang thang + **ném băng gạc**, kẹp Simon 2 phía; Boomerang/Holy Water/roi hiệu quả; **Stopwatch VÔ DỤNG**; Holy Water "hoá đá" chúng | §4.1 B3, §4.3 B3 |
| Vật phẩm Block 3: Holy Water (St7 một trong các nến đầu), **Axe (St7 nến thấp nhất — RƠI MẤT nếu không nhặt ngay)**, Boomerang (St8 nến dưới cầu thang lên tầng trên), Stop Watch (St8 nến bên phải cầu thang), **Dagger (St9 nến trong phòng boss)**, Pot Roast (St7 khối trái của bộ khối nhô thứ 2; **St9 khối thấp nhất phòng Mummy**) | §2.1 (I11–I15, I20), §2.3 bảng nến theo stage |
| Bí mật Block 3 (money bag 1000) | §2.2 (St7: ngồi ở mút bục cụt giữa tầng trên; St8: quỳ ngay trên 2 khối lơ lửng không với tới dưới Stop Watch — **có thể hiện NGOÀI màn hình**, quay lại sau 1–2 s; **St9: phòng boss, TRƯỚC khi khoá màn — đứng trên khối nằm trên khối chứa pot roast, đi sang trái vài giây**) |
| Money bag nhấp nháy **có thể biến mất sau một lúc** (St7: "chạy tới nhặt trước khi nó biến mất") — thời lượng wiki không nêu | §2.1 I06 |
| Glitch gốc (không cần dựng): Mummy chồng nhau + Boomerang × multiplier ⇒ sát thương mỗi khung | §4.2 |
| Walkthrough tổng Block 3 | §5 (dòng Stage 7, 8, 9) |

## §2 Stage 7–9 chi tiết (diễn đạt lại từ `text_Stages_7_-_9.txt`)
Mở đầu leg: kẻ địch chuyển động phức tạp hơn, khó né hơn (nhất là **Fleaman**), mạnh hơn — **mất 3 vạch/đòn** thay vì 2; boss chậm nhưng khoẻ.

### Stage 7 (ảnh 384×176 ⇒ 2 tầng × 768: dưới = **WalkwayEntranceA**, trên = **WalkwayEntranceB**)
- 4 loại địch: **Ghost, Fleaman, Skeleton, Raven**. Axe là vũ khí hiệu quả nhất (mọi loại đều bay hoặc nhảy).
- (1) Holy Water trong **một trong các nến đầu** stage. Pot Roast xuất hiện một chút sau (trong khối — §2.2 CV_WIKI_DATA: khối trái của bộ khối nhô thứ 2); lấy nếu Fleaman gây khó.
- (2) **Skeleton ở góc dưới-trái**: nhảy tới lui + ném xương; quất khi nó lại gần; Axe/Dagger diệt dễ.
- (3) Tầng trên: Holy Water hiệu quả với Skeleton (thả đúng chỗ nó sắp đáp), không hiệu quả với Raven. **Ngồi ở mép phải bục THẤP NHẤT ⇒ money bag hiện trên bục cao bên trái — chạy tới nhặt trước khi nó biến mất.** Một Skeleton nữa: đứng trên gờ nhìn nó — nó cố nhảy tới nhưng gờ quá cao, **nó rơi xuống hố**. Sau đó đi xuống cầu thang lấy **Axe ở nến thấp nhất (rơi mất nếu không nhặt ngay)**.
- Ảnh bản đồ wiki: ① (tầng dưới, gần thịt nướng), ② (góc trái tầng dưới), ③ (tầng trên giữa — icon rìu), icon nước thánh tầng dưới bên phải (gần điểm vào), 2 tim lớn ở góc trái trên/dưới.

### Stage 8 (ảnh 632×176 ⇒ tầng dưới **WalkwayA** [0..768], tầng trên **WalkwayB** lệch phải; vùng đen = không vẽ)
- Tầng dưới: **Medusa Head + Skeleton**; tầng trên: **Bone Tower + Raven**. Raven lao vào ⇒ quất; Bone Tower: quất liên tục giữa 2 lượt phun; Boomerang rất hiệu quả.
- (1) Medusa Head đầu tiên — tốt nhất là né. **Boomerang trong nến DƯỚI cầu thang lên tầng trên** (khó với tới nếu không có Axe/Dagger).
- (2) **Quỳ ngay phía trên 2 khối không với tới bên phải cầu thang ⇒ money bag hiện bên trái** (video Map_3 1:53.5–1:56: ngồi → túi hiện bên trái → +1000). **Stop Watch trong nến bên phải cầu thang** — có lợi với Bone Tower phía phải, vô dụng với boss.
- (3) Chỗ như thế này: sẵn sàng cho Raven — nhảy và quất khi nó bắt đầu lao.

### Stage 9 (ảnh 768×88 ⇒ 1 tầng × 1536 = **MummyMan**, phòng boss 256 px cuối)
- Mấu chốt: quất và nhảy đúng lúc. Stop Watch dùng được với địch thường nhưng **vô dụng với Mummy Men** ⇒ lấy **Dagger trong phòng boss** nếu cần.
- (1) **Raven cất cánh ngay khi Simon lại gần** — nhảy lên quất ngay lúc chúng cất cánh; có Dagger thì nhảy ném từ xa trước khi chúng bay.
- (2) **Skeleton nhảy qua đoạn cầu hỏng**: diệt con đầu khi nó tới, con thứ hai quất khi nhảy giữa các bục.
- (3) **Pot Roast trong khối dưới cùng phòng Mummy Men** — ăn ngay trước trận hoặc để dành, dùng khối làm gờ đứng.
- **Mummy Madness**: băng gạc gây sát thương; Stopwatch vô dụng; Dagger/Holy Water/Boomerang hiệu quả với "cặp hoá đá", nhất là có Shot Multiplier. Phải vừa né vừa đánh.

## §3 Đối chiếu wiki ↔ video Map_3 (planner — routine 02/10 kiểm)
| Wiki | Video Map_3 (planner) | |
|---|---|---|
| St7 Holy Water ở nến đầu | 0:14 lọ nước thánh trên sàn WEA, 0:16 HUD đổi nước thánh | khớp (vị trí nến: routine 10) |
| St7 Skeleton góc dưới-trái | 0:26–0:36 Skeleton ở WEA cx 0 | khớp |
| St7 money bag (ngồi mút bục thấp nhất) | 1:06 túi trắng bục cao bên trái WEB ⇒ +1000 @1:08 | khớp |
| St7 Skeleton rơi hố vì gờ cao | 1:10–1:12 Skeleton ném xương WEB — rơi hố? | routine 07 xem |
| St7 Axe nến thấp nhất (rơi mất) | ? | routine 10 xem (video có phá nến đó không) |
| St8 Medusa Head + Skeleton tầng dưới | 1:34–1:44 Medusa Head, 1:40 Skeleton | khớp |
| St8 money bag quỳ trên 2 khối | 1:53.5 ngồi ⇒ 1:54.5 túi hiện trái ⇒ 1:56 +1000 | khớp |
| St8 Bone Tower + Raven tầng trên | 2:04–2:14 Bone Tower trên cầu lan can | khớp |
| St9 Skeleton qua cầu hỏng | 2:50–2:52 | khớp |
| St9 Bone Tower, Medusa Head | Bone Tower 2:38–2:48, 3:00–3:02; Medusa Head? | routine 02 xem |
| St9 Pot Roast khối thấp nhất phòng boss | video boss 0:06–0:08 Simon phá ⇒ thịt nướng | khớp |
| St9 Dagger nến phòng boss | video boss Simon cầm dao găm từ trước | routine 09/10 xem nến |
| Mummy Men 3000 đ | **mỗi con +3000** (Map_3 3:43 và 3:46; video boss +3000 / +3200?) | **lệch wiki — theo video** (H1) |
| Mummy 48/32 đòn | Map_3: chỉ nước thánh (không đếm được đòn); video boss: dao + roi | routine 09 đo từ ENEMY bar |
| Stopwatch vô dụng với Mummy | Map_3 không dùng | giữ wiki |
| St9 money bag trong phòng boss trước khi khoá | video không làm | chỉ wiki |
| Mummy "hoá đá" bởi Holy Water | Map_3: Mummy đỏ khi đứng trong lửa | routine 09 đo |
