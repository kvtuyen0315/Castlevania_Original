# CV_WIKI_DATA — Bảng chỉ số Castlevania (NES, bản US) trích từ StrategyWiki

Ngày soạn: 2026-10-11. Mục đích: bảng dữ liệu có cấu trúc để các routine lập trình dùng làm "stats" cho nhân vật, item, luật chơi. Chỉ ghi số có trong nguồn; chỗ nào wiki không có thì ghi "wiki không nêu" (không tự điền).

**Quy ước đọc file**
- Nhãn nguồn: [GP] Gameplay · [IT] Items · [EN] Enemies · [BO] Bosses · [WT] Walkthrough · [S1-3] Stages 1 - 3 · [S4-6] · [S7-9] · [S10-12] · [S13-15] · [S16-18] · [SE] Secrets.
- [IMG] = đọc/đo từ ảnh nằm trong trang wiki (bản đồ stage, ảnh chụp màn hình, icon), KHÔNG phải chữ của wiki. Các con số đo từ ảnh đều là xấp xỉ (xem §6.0).
- "(suy luận)" = do người soạn suy ra, không có nguyên văn trong nguồn.
- "Stage" = màn 1-18. "Block" = nhóm 3 stage (wiki gọi là "leg"): Block 1 = Stage 1-3, Block 2 = 4-6, Block 3 = 7-9, Block 4 = 10-12, Block 5 = 13-15, Block 6 = 16-18.
- "Hits (Weak/Strong)": số đòn để diệt khi dùng vũ khí yếu (Leather Whip, Holy Water, Dagger) / mạnh (Chain Whip ngắn và dài, Boomerang, Axe) [EN].

**Tóm tắt 12 hằng số quan trọng nhất (chi tiết ở các mục sau)**
1. Simon có 16 vạch máu. Sát thương mỗi lần trúng: Stage 1-6 = 2 (8 đòn chết), 7-12 = 3 (6 đòn chết), 13-18 = 4 (4 đòn chết); mọi loại đòn trong cùng một stage gây cùng mức [GP].
2. Rơi hố / rơi nước / chạm gai = chết ngay ở mọi stage, bất kể máu [GP].
3. Thêm mạng ở 30,000 điểm, 80,000 điểm, rồi mỗi 50,000 điểm [GP].
4. Heart: Small = 1, Large = 5. Vũ khí phụ tốn 1 tim/lần, riêng Stopwatch 5 tim/lần [IT][GP].
5. Pot Roast hồi 6 vạch máu; chỉ nằm trong khối gạch phải phá bằng roi [IT].
6. Chain Whip gây sát thương gấp 1.5 lần Leather Whip; roi lên cấp 2 lần, về cấp đầu khi chết [IT].
7. Sau mỗi boss: Magic Crystal hồi đầy máu; cộng 10 điểm/giây còn lại và 100 điểm/tim còn lại [GP][WT].
8. Giết nhiều kẻ địch bằng một lần ném vũ khí phụ (không phải roi): 2 con = 1000, 3 con = 2000, 4 con = 4000 điểm [GP].
9. Stopwatch đóng băng 5 giây; Invisibility Potion khoảng 5 giây; Rosary diệt mọi kẻ địch trên màn trừ boss [IT].
10. Không có checkpoint giữa stage: hết mạng → Continue quay về ĐẦU stage đang chơi [GP].
11. Boss: số đòn Weak/Strong = Phantom Bat 12/8, Queen Medusa 12/8, Mummy Men 48/32, Frankenstein 24/16, Death 24/16, Dracula 24/16 (dạng 2: 32 đòn theo [S16-18]) [BO].
12. Wiki KHÔNG nêu: thời gian đếm ngược mỗi stage, số mạng khởi đầu, giới hạn tim, sát thương từng vũ khí phụ, tốc độ/knockback/bất tử tạm (xem §7).

---

## §0 Nguồn

Cách lấy dữ liệu (quan trọng): trang gốc `strategywiki.org` trả HTTP 403 kèm màn chắn Cloudflare "Just a moment..." cho cả `curl` (đủ 2 dạng URL `?action=raw` và `/w/index.php?...&action=raw`) lẫn công cụ WebFetch. Tôi KHÔNG vượt bot-check/CAPTCHA. Thay vào đó lấy bản lưu công khai (Internet Archive / Wayback Machine, dạng `id_` = HTML gốc không bị chèn lại) của chính các URL đó, rồi chuyển HTML thành văn bản bằng script Python chuẩn. Đã kiểm tra độ trung thực: so sánh từng từ giữa HTML gốc và văn bản xuất → không rơi chữ nào (chỉ khác mục lục, nút "edit", chân trang). Không lấy được wikitext thô (`action=raw`): trang gốc bị chặn và Wayback không có bản lưu loại đó, nên KHÔNG có file `raw_<Trang>.txt`; thay bằng `wayback_<Trang>.html` (HTML nguồn) và `text_<Trang>.txt` (văn bản đã chuyển).

Thư mục lưu (bản planner chép vào repo 11/10/2026): HTML nguồn `wayback_<Trang>.html`, văn bản `text_<Trang>.txt` và ảnh bản đồ/icon `img/` nằm ở `ClaudeNotes/cvgp/_cache/wiki_raw/` (KHÔNG commit — nội dung wiki nguyên trang; chỉ để kiểm chứng tại máy). Ảnh bản đồ/icon tải từ CDN ảnh của wiki `cdn.wikimg.net` (không bị chặn). Script phân tích của planner không giữ lại.

URL bản lưu đầy đủ (kèm timestamp) của từng trang nằm trong `wiki\wayback_manifest.txt`. Ngày lấy dữ liệu: 2026-10-11 (tất cả các dòng dưới). "Snapshot" = thời điểm bản lưu Wayback; "Revision" = số `oldid` của wiki ghi trong chân trang bản lưu, kèm ngày sửa cuối của trang.

| Trang | URL gốc | Đọc được? | Cách lấy | Snapshot Wayback | Revision (oldid) / sửa cuối |
|---|---|---|---|---|---|
| Gameplay | https://strategywiki.org/wiki/Castlevania/Gameplay | Có (bản lưu) | Wayback id_ + parse; live bị 403 | 2026-01-15 03:15 | 1003717 / 9 Oct 2022 |
| Items | https://strategywiki.org/wiki/Castlevania/Items | Có (bản lưu) | như trên | 2026-03-10 08:56 | 998260 / 12 Sep 2022 |
| Enemies | https://strategywiki.org/wiki/Castlevania/Enemies | Có (bản lưu) | như trên | 2026-04-16 20:15 (bản 200 mới nhất; bản 2026-05-14 là trang chắn Cloudflare) | 1130580 / 14 Jul 2025 |
| Bosses | https://strategywiki.org/wiki/Castlevania/Bosses | Có (bản lưu) | như trên | 2026-03-05 06:31 | 1142691 / 29 Oct 2025 |
| Walkthrough | https://strategywiki.org/wiki/Castlevania/Walkthrough | Có (bản lưu) | như trên | 2026-02-28 02:58 | 260388 / 13 Apr 2008 |
| Stages 1 - 3 | https://strategywiki.org/wiki/Castlevania/Stages_1_-_3 | Có (bản lưu) | như trên | 2026-10-10 07:00 | 1129575 / 7 Jul 2025 |
| Secrets (trang con) | https://strategywiki.org/wiki/Castlevania/Secrets | Có (bản lưu) | như trên | 2026-10-10 06:59 | 1055846 / 14 Dec 2023 |
| Stages 4 - 6 (trang con) | https://strategywiki.org/wiki/Castlevania/Stages_4_-_6 | Có (bản lưu) | như trên | 2026-03-08 10:02 | 834658 / 21 Jun 2019 |
| Stages 7 - 9 (trang con) | https://strategywiki.org/wiki/Castlevania/Stages_7_-_9 | Có (bản lưu) | như trên | 2026-03-05 12:22 | 748600 / 23 Jun 2015 |
| Stages 10 - 12 (trang con) | https://strategywiki.org/wiki/Castlevania/Stages_10_-_12 | Có (bản lưu) | như trên | 2026-03-10 09:42 | 734073 / 30 Jan 2015 |
| Stages 13 - 15 (trang con) | https://strategywiki.org/wiki/Castlevania/Stages_13_-_15 | Có (bản lưu) | như trên | 2026-03-12 15:02 | 1129572 / 7 Jul 2025 |
| Stages 16 - 18 (trang con) | https://strategywiki.org/wiki/Castlevania/Stages_16_-_18 | Có (bản lưu) | như trên | 2026-03-05 13:49 | 1129573 / 7 Jul 2025 |
| Table of Contents | https://strategywiki.org/wiki/Castlevania/Table_of_Contents | Có (bản lưu) | như trên | 2026-02-08 07:37 | 934020 / 6 Dec 2021 |
| Trophies | https://strategywiki.org/wiki/Castlevania/Trophies | Có (bản lưu) | như trên | 2026-05-10 17:17 | 934021 / 6 Dec 2021 (chỉ là danh sách trophy PS4 Arcade Archives, không có dữ liệu game) |

Lưu ý độ cập nhật: bản lưu có thể cũ hơn trang đang chạy; sửa đổi sau ngày snapshot (đặc biệt Enemies sau 2026-04-16) sẽ không có mặt. Ngày sửa cuối của mọi trang đều trước ngày snapshot.

**Kiểm lại 13/10/2026 (routine CVGP 04)** với `text_<Trang>.txt`: số đòn yếu/mạnh + điểm 15 enemy (bảng từng loại và bảng Comparison), 6 boss (số đòn, điểm, Stopwatch), giá tim (1 / Stopwatch 5), tim nhỏ/lớn 1/5, túi 100/400/700, bonus 1000/2000, Moai 4000, multiplier 700, Pot Roast 6 vạch, sát thương theo stage 2/3/4, 1UP 30 000 / 80 000 / mỗi 50 000, tally 10/giây + 100/tim, giết nhiều 1000/2000/4000, Dracula dạng 2 "32 lần", Death tối đa 3 lưỡi hái — **0 sai**, không sửa số nào. Ghi thêm: trang Gameplay mục Points viết lại luật 1UP là "30,000 rồi mỗi 50,000" (cùng nghĩa với 30 000 → 80 000 → +50 000). Số liệu này đã vào `Assets/Data/Config/Csv/*.csv` (báo cáo `ClaudeNotes/cvgp/CHARACTER_STATS.md`).

Ảnh đã đọc (nội dung của trang, không phải chữ): bản đồ Stage 1 (Part A, Part B), Stage 2, Stage 3 có chấm số cyan khớp với các ghi chú đánh số; 3 ảnh chụp màn hình Stage 1/2/3 (cho thấy HUD thật); 19 icon vật phẩm trên trang Items; ảnh bản đồ lâu đài của Walkthrough (6 ô đỏ = vị trí 6 boss, không có chữ). Ảnh bản đồ Stage 4-18 chưa phân tích (không thuộc yêu cầu Map 1).

---

## §1 Điều khiển & Simon Belmont

### 1.1 Nút NES [GP]

| Nút | Hành động | Chi tiết theo wiki |
|---|---|---|
| D-pad Trái / Phải | Đi bộ sang trái / phải | — |
| D-pad Lên / Xuống khi đứng trước hoặc đang ở trên cầu thang | Leo lên / leo xuống bậc | — |
| D-pad Xuống khi đứng trên đất | Ngồi (crouch) | Ngồi để đánh roi thấp |
| A | Nhảy | Nhảy thẳng đứng, hoặc nhảy chéo sang trái/phải nếu GIỮ hướng lúc bấm A |
| B | Tấn công | Roi cao khi đứng, roi thấp khi ngồi |
| Lên + B | Dùng vũ khí phụ | Chỉ dùng được khi đủ tim cho vũ khí đó |
| Start | Bắt đầu game mới; giữa trận = Pause | — |
| Select | Không dùng | — |

Dùng vũ khí (roi) khi đang ở giữa không trung: được [GP]. Giữ một vũ khí phụ tại một thời điểm [GP].

### 1.2 Cơ chế di chuyển, bị đánh, chết

| Cơ chế | Wiki nói gì | Nguồn |
|---|---|---|
| Đi bộ | Có; tốc độ wiki không nêu | [GP] |
| Nhảy | Nhảy qua khe nhỏ dễ dàng; hướng nhảy (đứng thẳng / trái / phải) chốt lúc bấm A. Đổi hướng giữa không trung: wiki không nêu. Độ cao/đường cong nhảy: wiki không nêu | [GP] |
| Rơi | Rơi xuống hố, nước hoặc gai = chết ngay ở mọi stage | [GP] |
| Ngồi | Ngồi để dùng roi thấp; ngồi tại điểm đặc biệt kích hoạt kho báu ẩn (xem §2.2) | [GP][SE] |
| Cầu thang | Lên/xuống bằng D-pad Lên/Xuống. Có nhảy khỏi cầu thang được không: wiki không nêu. Trên cầu thang Simon không thể bị đánh văng khỏi bậc trong bất cứ trường hợp nào (Stage 17) | [GP][S16-18 Stage 17] |
| Bị đánh / knockback | Wiki không có mục riêng. Các ghi chú rời: (a) Fishman/Bat có thể hất Simon khỏi bệ nổi rơi xuống nước (Stage 10); (b) Stage 10 phải ngồi dưới trần thấp, nếu không bị đẩy khỏi bệ nổi; (c) Stage 5: cố ý để Medusa Head húc khi nhảy khỏi sàn thứ 3 sẽ bị hất lên sàn phía trên (mẹo, rủi ro rơi hố) | [EN][S10-12][S4-6] |
| Bất tử tạm sau khi bị đánh (i-frames) | Wiki không nêu | — |
| Chạm vào kẻ địch bị đóng băng/gai bị đóng băng | Vẫn nhận sát thương | [IT] |
| Chết | 3 nguyên nhân: (1) thanh máu về 0; (2) đồng hồ stage về 0; (3) rơi hố/nước/gai. Stopwatch và Invisibility KHÔNG ngăn chết do rơi hố | [GP][IT] |

### 1.3 Máu và sát thương nhận vào

Simon có **16 vạch máu** [GP] (HUD ở ảnh chụp: thanh PLAYER 16 đoạn, thanh ENEMY 16 đoạn — đếm từ điểm ảnh [IMG]).

| Stage | Sát thương mỗi lần trúng (vạch) | "Hits per Life" (số đòn đủ để chết) |
|---|---|---|
| 1 – 6 | 2 | 8 |
| 7 – 12 | 3 | 6 |
| 13 – 18 | 4 | 4 |

- Mọi đòn trong một stage (kẻ địch thường, boss, đạn) gây CÙNG một mức sát thương [GP]. Rơi hố/nước/gai giết ngay ở mọi level [GP].
- Khớp chéo: Stage 7-9 mất 3 vạch thay vì 2 [S7-9]; Stage 13-15 mất 4 vạch mỗi va chạm, chỉ chịu được 4 đòn [S13-15]; trận Dracula chỉ sống được 3 đòn, đòn thứ 4 chết [S16-18]. "Hits per Life" hiểu là đòn thứ N làm chết: 8×2 = 16; 6×3 = 18 ≥ 16 (đòn thứ 6); 4×4 = 16 (suy luận).
- Hồi máu: Pot Roast +6 vạch [IT]; Magic Crystal sau boss hồi ĐẦY máu [WT].
- Second Quest (Stage 19+): mọi đòn gây 4 vạch ngay từ đầu [SE][S16-18].

### 1.4 Mạng, 1UP, Continue

- Thêm 1 mạng khi điểm chạm 30,000, rồi 80,000, rồi cứ mỗi 50,000 điểm sau đó [GP] → các mốc tiếp theo 130,000, 180,000, … (suy luận từ "mỗi 50,000").
- Số mạng khởi đầu: wiki không nêu. Ảnh chụp HUD Stage 1/2/3 đều hiển thị "P-03" khi điểm < 30,000 [IMG] → (suy luận) bắt đầu 3 mạng.
- Mất 1 mạng khi: máu về 0, hết giờ, rơi hố/nước/gai [GP].
- Hết mạng: được Continue nhưng quay lại ĐẦU stage đã chết, dù đã đi xa cỡ nào [GP]. Số lần Continue tối đa / màn Game Over: wiki không nêu.
- Khi chết: roi về dạng ban đầu [IT]. Vũ khí phụ cũng mất (suy luận từ [S13-15]: muốn còn Holy Water ở cuối stage thì không được chết; và [BO]: sau khi chết ở Stage 12 chỉ còn Dagger ngoài phòng boss).
- 1UP dạng vật phẩm: chỉ có thể nhặt khi kích hoạt bí mật (xem §2).
- Second Quest: bắt đầu lại ở Stage 1 với vũ khí phụ đang có [SE].

### 1.5 Tim (heart)

- Tim = "đạn" cho vũ khí phụ [IT][GP]. Small Heart = 1 tim; Large Heart = 5 tim [IT][GP]. Small Heart rơi chậm, đung đưa qua lại; Large Heart nặng nên rơi nhanh xuống sàn [IT].
- Mỗi lần ném vũ khí phụ tốn 1 tim, riêng Stopwatch tốn 5 tim [IT][GP].
- Điểm: nhặt tim giữa stage KHÔNG cho điểm; tim còn lại sau boss được 100 điểm/tim [GP][IT].
- Giới hạn tim tối đa: wiki không nêu (ảnh Stage 3 hiển thị ♥-70 nên giới hạn ≥ 70 [IMG]; ảnh Stage 1 ngay trước cổng hiển thị ♥-15, khớp với 5 tim ban đầu + 2 Large Heart ở Stage 1A (suy luận)).

### 1.6 Roi (Vampire Killer): 3 cấp

| Cấp | Tên | Hình dạng / độ dài | Sát thương | Phân loại | Cách có |
|---|---|---|---|---|---|
| 1 | Leather Whip | roi da, ngắn; độ dài px: wiki không nêu | mức cơ sở (1×) | weak [EN] | mặc định; quay về cấp này khi chết [IT] |
| 2 | Chain Whip (ngắn) | đổi từ da sang xích | 1.5× Leather Whip [IT] | strong [EN] | nhặt Morning Star lần 1 [IT] |
| 3 | Chain Whip (dài) | kéo dài chiều dài roi so với cấp 2 | wiki không nêu riêng; xếp chung nhóm strong với chain [EN] | strong | nhặt Morning Star lần 2 [IT] |

- Quy tắc nguồn rơi (nguyên ý wiki): Morning Star xuất hiện trong các nến vốn chứa small heart, nếu bạn đã thu đủ tim (4 cho lần nâng 1, 8 cho lần nâng 2) [IT]. Cơ chế chi tiết wiki không giải thích thêm (xem §7).
- Chuỗi nâng cấp bảo đảm ở Stage 1: 2 đuốc trước cổng lâu đài chứa Morning Star (đủ để lên cấp cuối) [S1-3][IMG].
- Mô tả minh họa của [GP]: Vampire Killer là roi gia truyền Belmont, có thể dài ra, đổi từ da sang xích, và gắn morning star.

### 1.7 Vũ khí phụ (secondary weapon)

Dùng bằng Lên + B; cần đủ tim [GP]. Mặc định chỉ 1 vũ khí phụ trên màn hình cùng lúc [GP][IT].

| Vũ khí | Tim/lần | Nhóm | Đặc tính (theo wiki) | Điểm cần lưu ý |
|---|---|---|---|---|
| Dagger | 1 | weak | Bay nhanh và thẳng về phía trước; sức mạnh xấp xỉ Leather Whip; chỉ trúng 1 mục tiêu [IT]. Là vũ khí phụ cơ bản nhất | Ở Stage 12, Dagger nằm ngay ngoài phòng boss và là vũ khí phụ duy nhất bạn có nếu chết [BO] |
| Axe | 1 | strong | Bay lên cao rồi vòng xuống sàn (đường vòng cung); không hợp với kẻ địch ngay trước mặt; rất tốt với kẻ địch ở trên cao, kẻ địch bay, Knight trên bệ [IT]. Mạnh ngang Cross [IT] | Là vũ khí duy nhất chạm được vài nến cao ở Stage 13 và 15 (chỉ chứa Small Heart) [SE] |
| Holy Water (Fire Bomb) | 1 | weak | Lọ vỡ khi chạm sàn, bùng thành ngọn lửa nhỏ như bom; có thể trúng trực tiếp nhưng mấu chốt là nhốt kẻ địch trong lửa trên sàn; rất hiệu quả với kẻ địch chậm (nhiều boss, Axe Knight) [IT] | Làm Dracula dạng 2 choáng khi còn cháy [BO]; không hiệu quả với Raven [S7-9] |
| Cross (Boomerang) | 1 | strong | Mạnh ngang Axe; có thể trúng nhiều mục tiêu một lần ném và quay về trúng thêm lượt về; nếu nhảy qua thì nó tiếp tục bay trúng kẻ địch phía sau lưng [IT] | Dễ có bonus nhiều-kẻ-địch [IT]. Với Triple Shot khi đánh Death có thể làm game treo [BO] (lỗi của bản gốc, không cần tái hiện) |
| Stopwatch | 5 | không gây sát thương | Đóng băng kẻ địch và bẫy gai di động trong 5 giây [IT] | 4 giới hạn: hầu hết boss miễn nhiễm (Queen Medusa là boss cuối còn bị ảnh hưởng [BO]); item vẫn biến mất khi đóng băng; chạm kẻ địch/gai đóng băng vẫn mất máu; rơi hố vẫn chết [IT]. Multiplier không ảnh hưởng Stopwatch [IT] |

Sát thương số của từng vũ khí phụ: wiki không nêu; chỉ có phân nhóm weak/strong và các so sánh trên.

### 1.8 Double Shot / Triple Shot (shot multiplier)

- Mặc định 1 vũ khí phụ trên màn; Double Shot cho 2, Triple Shot cho 3 vũ khí cùng lúc [IT][GP].
- Nguồn có multiplier: khối ẩn, nến, kẻ địch rơi ra [IT]; xuất hiện khi trúng kẻ địch 10 lần bằng cùng một loại vũ khí phụ [IT]; Bone Dragon rơi multiplier nếu bị đánh bằng Holy Water [EN]; Stage 18: dùng Boomerang phá nến lặp lại ở tầng dưới sẽ lần lượt ra Double rồi Triple [S16-18]; Stage 15: có thể nhặt multiplier ngay trước Death nếu dùng vũ khí phụ hiệu quả [S13-15].
- Mất multiplier khi nhặt vũ khí phụ khác [IT].
- Nhặt multiplier khi đang cầm Stopwatch (hoặc không có vũ khí phụ nào, theo [GP]) được 700 điểm [IT][GP].
- Với Boomerang + Multiplier, ném liên tiếp nhanh gây sát thương lớn và hất đạn của Dracula [S16-18].

### 1.9 Bảng điểm (score) [GP]

| Nguồn điểm | Giá trị | Ghi chú |
|---|---|---|
| Money Bag (Red / Purple / White) | 100 / 400 / 700 | Nến, kẻ địch rơi ngẫu nhiên [GP][IT] |
| Bonus bí mật | 1000 (Flashing Money Bag) hoặc 2000 (Crown, Treasure Chest/Box) | Phải đứng đúng chỗ [GP][IT][SE] |
| Moai figure | 4000 | Chỉ có ở Second Quest [SE] |
| Double/Triple Shot | 700 | Chỉ khi đang cầm Stopwatch (hoặc không có vũ khí phụ) [GP][IT] |
| Tim còn lại | 100/tim | Cộng sau boss; nhặt tim giữa stage không cho điểm [GP] |
| Thời gian còn lại | 10 điểm/giây | Cộng sau boss, sau khi chạm Crystal Ball [GP] |
| "Special" (giết nhiều địch trong một lần ném vũ khí phụ, không phải roi) | 2 con = 1000; 3 con = 2000; 4 con = 4000 | [GP] |
| Kẻ địch thường | 100 – 1400 | Mỗi loại một giá (xem §3); một số đạn đáng 100 điểm; giết bằng "cross" không có điểm [GP] ([IT] xác nhận cho Rosary; (suy luận) "cross" ở [GP] chính là Rosary) |
| Boss | 3000 – 7000 | [GP] nói mọi boss trừ Dracula có điểm; [BO] ghi Dracula 50,000 (mâu thuẫn, xem §7) |

### 1.10 Thời gian stage và thưởng cuối

- Có đồng hồ đếm ngược: về 0 thì mất 1 mạng [GP]. Giá trị khởi đầu của từng stage, tốc độ đếm: wiki không nêu. Ảnh chụp HUD: TIME 0282 (Stage 1, sát cổng), TIME 0182 (Stage 2), TIME 0131 (Stage 3) [IMG] (giá trị giữa chừng, không phải mốc khởi đầu).
- Sau mỗi boss: xuất hiện Magic Crystal → chạm vào thì hồi đầy máu và tính thưởng: thời gian còn lại × 10, tim còn lại × 100 [GP][WT].
- Thưởng ở cuối stage không có boss: wiki không nêu.

### 1.11 HUD (Status) [GP]

A điểm hiện tại · B thời gian còn lại · C stage hiện tại · D máu Simon · E máu boss · F vũ khí phụ · G số tim · H số mạng còn lại · I hệ số nhân (shot multiplier). Ảnh chụp thật [IMG]: hàng trên "SCORE-000000 TIME 0282 STAGE 01", dưới là "PLAYER" + 16 đoạn, "ENEMY" + 16 đoạn, ô khung chứa icon vũ khí phụ, "♥-15", "P-03".

Định dạng chữ HUD đo từ ảnh [IMG] (suy luận cho phần số chữ số): `SCORE-dddddd` (6 chữ số, có gạch nối), `TIME dddd` (4 chữ số, không gạch nối), `STAGE dd` (2 chữ số); hàng dưới: `PLAYER` + thanh 16 đoạn cam, ô vuông viền cam chứa icon vũ khí phụ, `♥-dd`; hàng cuối: `ENEMY` + thanh 16 đoạn cam, `P-dd` (số mạng). Ví dụ ba ảnh: `SCORE-000000 TIME 0282 STAGE 01 / ♥-15 / P-03` (dao găm), `SCORE-010200 TIME 0182 STAGE 02 / ♥-46 / P-03` (Stopwatch), `SCORE-012800 TIME 0131 STAGE 03 / ♥-70 / P-03` (Axe).

### 1.12 Pause, Game Over

Pause: nhấn Start giữa trận [GP]. Game Over: wiki chỉ nói hết mạng thì được Continue (xem 1.4).

### 1.13 Second Quest (sau khi hạ Dracula lần đầu) [SE][S16-18]

- Credits chạy xong thì game bắt đầu lại từ Stage 1 (hiển thị là Stage 19) với vũ khí phụ đang có; mọi đòn gây 4 vạch; kẻ địch đông hơn và nhanh hơn rõ rệt nhưng KHÔNG cần thêm đòn để diệt [SE].
- Bộ đếm stage không reset: Stage 19 = khu ngoài lâu đài, Stage 20 = khu bên trong (cả hai = Stage 1 của lượt đầu). Các stage sau đi theo cùng thứ tự (suy luận: Stage k của lượt đầu = k + 19 từ Stage 2 trở đi, vì Stage 25 trong [SE] có bẫy gai giống Stage 6).
- Sau khi hạ Dracula lần 2, vòng lặp tiếp tục và độ khó KHÔNG tăng thêm; tới Stage 99 bộ đếm ngừng tăng nhưng game vẫn chạy [SE].
- Thay đổi thêm: Stage 1-3 có Bat bay liên tục, Fishman và Ghoul đông hơn; Stage 4-6 đầy Medusa Head (gai Stage 6 trở nên cực khó); Stage 10-11 nhiều Bat hơn nhiều, Eagle/Fleaman nhiều hơn, Fishman đông và có thể nhảy lên ngay dưới chân [SE].
- Vật phẩm riêng Second Quest: Moai 4000 điểm (Stage 24, 26, 27), 1UP (Stage 21, 25), Pot Roast (Stage 23), Shot Multiplier (Stage 26, 34), money bag nhấp nháy (Stage 19, 20) — xem §2.2.

### 1.14 Bối cảnh

Dracula thức dậy năm 1691; người dân Transylvania nhờ Simon Belmont; Simon dùng roi gia truyền Vampire Killer và 5 loại vũ khí phụ để vượt lâu đài, hạ 5 boss trung gian (kể cả Death) rồi mới tới phòng Dracula [GP].

---

## §2 Bảng ITEM

### 2.1 Bảng vật phẩm

Tên EN theo wiki; "Tên khác" là tên gọi khác xuất hiện trong các trang. Điểm của việc NHẶT item: wiki chỉ nêu điểm cho money bag/bonus/multiplier; các item khác không nêu điểm (ảnh chụp Stage 1 sát cổng: SCORE vẫn 000000 dù đã có ♥-15 và Dagger [IMG]).

| ID | Tên (EN) | Tên khác | Hiệu ứng chính xác | Điểm | Nguồn rơi | Thời gian tồn tại / biến mất | Ghi chú |
|---|---|---|---|---|---|---|---|
| I01 | Small Heart | Heart | +1 tim (đạn vũ khí phụ). Rơi chậm, đung đưa qua lại [IT] | 0 (nhặt tim không cho điểm [GP]) | Phần lớn các nến; kẻ địch hay rơi [IT]. Nến thường chứa small heart có thể đổi thành Morning Star theo quy tắc ở I10 [IT] | Wiki không nêu | Stage 13 và 15 có vài nến rất khó với tới chỉ chứa Small Heart, chỉ Axe mới đánh tới [SE] |
| I02 | Large Heart | Heart (lớn) | +5 tim [IT][GP]. Nặng nên rơi nhanh xuống sàn [IT] | 0 | 2 đuốc ở Stage 1A [IMG]; khối của Bone Dragon thứ 2 ở Stage 12 [SE][S10-12]; Bone Dragon hay để lại 5-8 money bag hoặc small heart khi chết [EN] | Wiki không nêu | — |
| I03 | Red Money Bag | Moneybag (đỏ) | Cộng điểm | 100 | Nến; kẻ địch rơi ngẫu nhiên [IT] | Wiki không nêu | Không nên mạo hiểm mạng để lấy [IT] |
| I04 | Purple Money Bag | Moneybag (tím) | Cộng điểm | 400 | Như trên; cố định: khối phải nhất của bục có Panther thứ 3, Stage 1B [SE] | Wiki không nêu | Wiki Stage 1 gọi mục này là "bonus treasure bag" [S1-3] (xem §7) |
| I05 | White Money Bag | Moneybag (trắng) | Cộng điểm | 700 | Như trên | Wiki không nêu | — |
| I06 | Flashing Money Bag | Bonus; hidden money bag | Cộng điểm | 1000 | Bí mật: đứng/ngồi đúng chỗ (xem 2.2) [IT][SE] | Có thể biến mất sau thời gian ngắn (Stage 7: chạy tới nhặt trước khi nó biến mất [S7-9]); thời lượng wiki không nêu | Có thể hiện ngoài màn hình (Stage 8) [SE] |
| I07 | Crown | Bonus | Cộng điểm | 2000 | Stage 4 (phá khối tường phải rồi bước vào khe) [SE][S4-6] | Wiki không nêu | Hiện nhấp nháy dưới cầu thang [S4-6] |
| I08 | Treasure Chest / Treasure Box | Treasure (kho báu) | Cộng điểm | 2000 | Stage 5 (đứng yên trên 2 khối nhô cao) [SE][S4-6] | Wiki không nêu | Hiện từ các khối bên phải [S4-6] |
| I09 | Moai figure | Tượng Moai (tham chiếu Gradius) | Cộng điểm | 4000 | Chỉ Second Quest: Stage 24, 26, 27 [SE] | Wiki không nêu | — |
| I10 | Morning Star | Whip upgrade; Chain Whip upgrade | Lần 1: roi da → roi xích, sát thương 1.5×. Lần 2: roi dài hơn. Tối đa 2 lần nâng. Mất (về roi da) khi chết [IT] | Wiki không nêu | Nến vốn chứa small heart, nếu đã thu đủ tim: 4 cho lần 1, 8 cho lần 2 [IT]. Cố định: 2 đuốc trước cổng Stage 1 [S1-3][IMG] | Wiki không nêu | Xem §7 về quy tắc "4 / 8 tim" |
| I11 | Dagger | — | Vũ khí phụ, 1 tim/lần, weak. Bay nhanh và thẳng; xấp xỉ sức mạnh Leather Whip; chỉ trúng 1 mục tiêu [IT] | 0 | Stage 1 đuốc thứ 5 [IMG]; Stage 5 nến gần giữa tầng dưới; Stage 9 nến trong phòng boss; Stage 12 ngay ngoài phòng boss [S4-6][S7-9][BO] | Wiki không nêu | Stage 5: chỉ lấy nếu chưa có vũ khí phụ khác [S4-6] |
| I12 | Axe | Throwing Axe | Vũ khí phụ, 1 tim/lần, strong. Bay lên cao rồi vòng xuống sàn; tốt với kẻ địch cao/bay/Knight trên bệ [IT] | 0 | Stage 3 nến giữa cầu thang cuối; Stage 7 nến thấp nhất (rơi nếu không nhặt ngay); Stage 14 nến gần Axe Knight đầu; Stage 16 ngay đầu stage [S1-3][S7-9][S13-15][S16-18] | Stage 7: rơi mất nếu không nhặt ngay [S7-9]; thời lượng wiki không nêu | Stage 14: wiki khuyên đừng lấy Axe trong nến đó trừ khi thật sự muốn [S13-15] |
| I13 | Holy Water | Fire Bomb | Vũ khí phụ, 1 tim/lần, weak. Lọ vỡ tạo ngọn lửa nhỏ trên sàn; nhốt/choáng kẻ địch chậm [IT] | 0 | Stage 1 nến đỉnh cầu thang [S1-3]; Stage 3 nến chân cầu thang cạnh Axe (ngẫu nhiên, nếu may) [S1-3]; Stage 7 một trong các nến đầu [S7-9]; Stage 10 đỉnh gờ [S10-12]; Stage 13 đầu stage (duy nhất) [S13-15]; Stage 18 nến trái cùng phòng Dracula [BO] | Thời gian lửa cháy/choáng: wiki không nêu | — |
| I14 | Cross | Boomerang | Vũ khí phụ, 1 tim/lần, strong. Mạnh ngang Axe; trúng nhiều mục tiêu, quay về, trúng cả lượt về [IT] | 0 | Stage 4 nến đầu tiên; Stage 8 nến dưới cầu thang lên tầng trên; Stage 15 đầu stage [BO]; Stage 18 đầu stage [S4-6][S7-9][BO][S16-18] | Wiki không nêu | Tên "Cross" dễ nhầm với Rosary (I18) |
| I15 | Stopwatch | Stop Watch | Vũ khí phụ, 5 tim/lần. Đóng băng kẻ địch và gai di động 5 giây; không phải vũ khí gây sát thương [IT] | 0 | Stage 1 nến thứ 2 [IMG]; Stage 2 lối ngầm [IMG]; Stage 5 nến gần Pot Roast; Stage 8 nến bên phải cầu thang; Stage 10 trước bục đầu; Stage 16 gần đoạn có con Phantom Bat thứ 2 (chú thích 2) [S4-6][S7-9][S10-12][S16-18] | 5 giây (hiệu lực) [IT] | Boss miễn nhiễm trừ Phantom Bat, Queen Medusa [IT][BO] |
| I16 | Double Shot | Shot Multiplier ×2 | Cho phép 2 vũ khí phụ cùng lúc trên màn; mất khi nhặt vũ khí phụ khác [IT] | 700 nếu nhặt khi đang cầm Stopwatch (hoặc không có vũ khí phụ) [IT][GP] | Khối ẩn (xem 2.2), nến, kẻ địch rơi; xuất hiện khi trúng kẻ địch 10 lần bằng cùng vũ khí phụ [IT] | Wiki không nêu | Stopwatch không bị ảnh hưởng |
| I17 | Triple Shot | Shot Multiplier ×3 | 3 vũ khí phụ cùng lúc | như I16 | Như I16 (Stage 18: phá nến bằng Boomerang lặp lại sẽ ra Double rồi Triple [S16-18]) | Wiki không nêu | — |
| I18 | Rosary | Cross (tên trong bảng Items: "Rosary (Cross)") | Mọi kẻ địch trên màn hình (trừ boss) bị diệt ngay, bất kể máu còn lại; boss không bị ảnh hưởng nhưng đạn của boss bị phá [IT] | 0 (kẻ địch bị Rosary giết không cho điểm [IT]) | Một số nến cố định; hiếm khi kẻ địch rơi [IT]. Stage 1: đỉnh cầu thang cuối [S1-3][IMG]; Stage 2: nến phía trên bên phải [IMG] | Wiki không nêu | — |
| I19 | Invisibility Potion | — | Simon tàng hình ~5 giây (hiện dạng lấp lánh), đi qua kẻ địch không bị thương; vẫn chết nếu rơi hố [IT] | 0 | Stage 3: giấu giữa cầu thang [S1-3] | ~5 giây [IT] | — |
| I20 | Pot Roast | Pork Chop (tên trong bảng Items) | Hồi 6 đơn vị (vạch) máu [IT][SE] | 0 | CHỈ trong khối gạch trông bình thường phải phá bằng roi (liệt kê ở 2.2) [IT] | Wiki không nêu | Một số khối là bậc đứng hữu ích, nên để dành (Stage 9) [SE] |
| I21 | 1UP | — | +1 mạng [IT] | 0 | Rất hiếm; chỉ kích hoạt bằng bí mật. Lượt đầu chỉ có 1 cái: Stage 13 [SE]. Second Quest: Stage 21, 25 [SE] | Stage 25: biến mất sau thời gian ngắn [SE] | — |
| I22 | Magic Crystal | Crystal Ball (glowing orb) | Xuất hiện khi hạ mỗi boss; chạm vào hồi ĐẦY máu và tính thưởng thời gian ×10, tim ×100; báo hiệu 1 trong 6 leg hoàn thành (wiki viết "6 stages" theo nghĩa leg); cần 5 Magic Crystal (5 boss trung gian) để vào đấu Dracula; crystal cuối sau Dracula kết thúc game [IT][WT][S1-3][S16-18][GP] | 10/giây còn lại + 100/tim còn lại | Rơi ra khi boss chết | Wiki không nêu | Sau khi hạ Death, lưỡi hái còn trên màn nên có thể chết khi đi nhặt [BO] |

Vật chứa (không phải item):

| ID | Đối tượng | Mô tả |
|---|---|---|
| C01 | Đuốc / lò lửa (brazier) | Wiki gọi chung là "torches"/"candles". Ở Stage 1A: 5 đuốc đặt trên bệ đá ngoài trời (số liệu đo ở §6.1). Nội dung đọc từ icon trên bản đồ [IMG] |
| C02 | Nến tường (wall candle) | Nến nhỏ trên tường trong lâu đài. Tất cả đều phá được; nội dung chỉ có ở những nến wiki liệt kê hoặc có icon trên bản đồ. Vị trí đo từ ảnh ở §6 |
| C03 | Khối gạch phá được | Khối/bức tường nhỏ bằng gạch giấu Pot Roast, Shot Multiplier, money bag, Large Heart… phải phá bằng roi; khác khối thường (xem 2.2) [IT][SE] |

### 2.2 Vị trí vật phẩm ẩn toàn game [SE]

(Diễn đạt lại, giữ đủ điều kiện kích hoạt. Số stage theo lượt chơi đầu trừ khi ghi Second Quest.)

**Bonus (money bag 1000 / Crown, Treasure 2000 / Moai 4000) — lượt đầu**

| Stage | Điều kiện kích hoạt | Vật phẩm |
|---|---|---|
| 1 | Phần đầu (ngoài trời): nhảy VƯỢT qua cửa lâu đài thì hiện money bag nhấp nháy; nhảy lùi lại nhặt rồi mới đi vào cửa. Phải canh nhảy cực chuẩn, sai sẽ tự đi vào cửa | Money bag 1000 |
| 2 | Bên phải xa của lối ngầm (có Fishman): 2 khối không với tới dưới cầu thang; phá khối trên-phải-nhất để thả xuống, ngồi trên các khối thấp nhất | Money bag 1000 |
| 4 | Phòng đầu: leo bộ cầu thang đầu, phá khối trên tường bên phải, bước vào khe | Crown 2000 |
| 5 | Tầng dưới: chỗ phải nhảy lên 2 khối nhô cao, đứng yên vài giây | Treasure Box 2000 |
| 6 | Sảnh có vòm và Medusa Head: đứng yên vài giây ở vòm thứ 4 | Money bag |
| 7 | Giữa tầng trên, đoạn băng qua khe rộng bằng bục (ngay sau bộ cầu thang lơ lửng không với tới). Phía dưới có bục cụt; ngồi ở mút cuối | Money bag |
| 8 | Tầng trên, 2 khối lơ lửng bên dưới ngay dưới Stop Watch không với tới; quỳ ngay phía trên chúng. Có thể hiện ngoài màn hình, quay lại sau 1-2 giây | Money bag |
| 9 | Phòng boss, trước khi khóa màn: đứng trên khối nằm trên khối chứa pot roast, đi sang trái vài giây; thường không với tới | Money bag |
| 13 | Đi trên bục cao gần Shot Multiplier, ép vào tường phải vài giây; hiện dưới cầu thang | 1UP (duy nhất lượt đầu) |
| 14 (a) | Hốc có bình lọ nền sau: đứng yên vài giây | Money bag |
| 14 (b) | Gần cuối, sau Red Skeleton, leo cầu thang có Axe Knight trên đỉnh: nhảy lên bục phía trên-phải, thả xuống bên phải lên 2 khối nhô cao, ngồi → money bag hiện ngay bên dưới | Money bag |
| 14 (c) | Sau Red Skeleton, đi dưới cầu thang tới vùng có lưới sắt nền sau; đứng yên ở bên trái vài giây | Money bag |
| 17 | Ngồi ở bên phải của bánh răng thấp nhất của tháp (cẩn thận Eagle và Fleaman) | Money bag |

**Bonus — Second Quest**

| Stage | Điều kiện kích hoạt | Vật phẩm |
|---|---|---|
| 19 | Quỳ ở tượng thứ 2 | Money bag nhấp nháy |
| 20 | Đứng giữa bục có Panther thứ 2, chờ vài giây | Money bag nhấp nháy |
| 21 | Xuống tầng dưới ngay trước bộ cầu thang đầu; đi sang trái vào phần sàn nhô cao vài giây | 1UP |
| 24 | Đi qua một điểm trên không gần bục di động đầu tiên (nhảy lên chính bục, hoặc nhảy xuống đường dưới đúng chỗ bục sẽ ở nếu nó ở xa nhất bên phải) | Moai 4000 |
| 25 | Đứng giữa bẫy gai thứ 1 và thứ 2 vài giây; phải nhanh vì 1UP biến mất sau chốc lát, lại phải né Medusa Head và gai | 1UP |
| 26 | Tầng trên: quỳ trên bục gần nến đầu tiên | Moai 4000 |
| 27 | Chạm vào tường phải | Moai 4000 |

**Pot Roast (+6 vạch máu) — lượt đầu**

| Stage | Vị trí |
|---|---|
| 2 | Phần đầu stage (trước khi xuống cầu thang vào lối ngầm có nước): bức tường gồm khối nhỏ bên phải; phá các khối thấp nhất; sẽ có 1 con Bat xuất hiện |
| 5 | Tầng trên, phía phải xa, nhảy lên gờ trên cùng để quay ngược lại. Ngay trên khe nhảy có 2 khối nhô cao; phá khối bên phải |
| 6 | Khối trái nhất của bục xuất phát |
| 7 | Khối bên trái của bộ khối nhô thứ 2; dọn hết Fleaman gần đó trước khi lấy |
| 9 | Khối thấp nhất trong phòng boss (Mummy Men); nên để dành đến khi thật cần vì khối là bậc đứng đánh tốt |
| 12 | Khối mà Bone Dragon đầu tiên bám vào |
| 13 | Gần cửa ra, ngay sau Red Skeleton: chồng khối; phá khối trên-trái |
| 15 | Phá bức tường phải xa, ngay sau cầu thang lên tầng trên |
| 17 | Tường phải ở cùng tầng với bánh răng ngang thứ 3 từ trên xuống; nhiều rủi ro vì Eagle/Fleaman; cũng có thể phá từ phía bên kia |

Second Quest: Stage 23 — lên cầu thang tới tầng trên, phá tường phải.

**Shot Multiplier — lượt đầu**

| Stage | Vị trí |
|---|---|
| 3 | Phòng boss: phá khối phải ở chân cầu thang |
| 4 | Tầng trên, gặp hố hẹp có Spear Knight: khối trên cùng ở tường phải của hố |
| 6 | Sau 3 bẫy gai, tới bộ cầu thang: phá khối trái ở chân cầu thang |
| 13 | Tầng trên, lên tới đỉnh có bộ cầu thang xuống lại: xuống rồi phá khối ở góc dưới-phải của trần thấp |
| 14 | Trên cùng, đi qua 3 quan tài ở nền sau; sang trái, xuống cầu thang rồi lên bộ tiếp theo; nhảy lên gờ bên phải và phá tường phải |
| 17 | Trước khi leo lên lối ra, sau bộ cầu thang gần cuối: khối phá được ở góc trần bên trái |

Second Quest: Stage 26 (phá bục phía trên nến thứ 2, tầng trên), Stage 34 (phá tường trái sau bộ cầu thang đầu).

**Khác**

| Stage | Vị trí / vật phẩm |
|---|---|
| 1 | Phần 2, bục có Panther thứ 3: phá khối phải nhất → Purple money bag 400 |
| 12 | Khối mà Bone Dragon thứ 2 bám vào → Large Heart |
| 13, 15 | Vài nến rất khó với tới, chỉ Throwing Axe (mang từ stage trước) đánh tới được; chỉ chứa Small Heart |

### 2.3 Vũ khí/item đặt sẵn trong nến theo văn bản từng stage (không phải bí mật)

| Stage | Nội dung (theo wiki) |
|---|---|
| 1 | 2 đuốc Morning Star trước cổng; Holy Water trong nến đỉnh cầu thang; Rosary ở đỉnh cầu thang cuối [S1-3]. Đọc từ ảnh bản đồ [IMG]: đuốc 1-5 = Large Heart, Morning Star, Morning Star, Large Heart, Dagger; nến thứ 2 phần trong = Stopwatch |
| 2 | Pot Roast ở tường khối (2.2); Stopwatch và Rosary chỉ thấy trên ảnh bản đồ [IMG] |
| 3 | Invisibility Potion giữa cầu thang; Axe trong nến giữa cầu thang cuối; nến chân cầu thang cạnh đó có thể chứa Holy Water (ngẫu nhiên); khối chân cầu thang ngoài cùng bên phải chứa Shot Multiplier (nhiều khả năng là Double Shot) [S1-3] |
| 4 | Boomerang trong nến đầu tiên; Crown (2.2); Shot Multiplier ở góc bục [S4-6] |
| 5 | Dagger trong nến gần giữa tầng dưới; Pot Roast ở góc trên-phải; Stop Watch trong nến gần đó [S4-6] |
| 6 | Pot Roast trong khối ở đầu stage; Shot Multiplier trong khối chân cầu thang [S4-6] |
| 7 | Holy Water ở một trong các nến đầu; Axe trong nến thấp nhất [S7-9] |
| 8 | Boomerang trong nến dưới cầu thang lên tầng trên (khó với tới nếu không có Axe/Dagger); Stop Watch trong nến bên phải cầu thang [S7-9] |
| 9 | Pot Roast trong khối thấp nhất phòng Mummy Men; Dagger trong nến phòng boss [S7-9] |
| 10 | Stop Watch trước bục đầu tiên; Holy Water trên đỉnh gờ [S10-12] |
| 12 | Pot Roast (khối của Bone Dragon 1); Large Heart (khối của Bone Dragon 2); Dagger ngoài phòng boss [S10-12][BO] |
| 13 | Holy Water ngay đầu stage; Pot Roast, Shot Multiplier (2.2) [S13-15] |
| 14 | Axe trong nến gần Axe Knight đầu; Shot Multiplier ở tường trái của tường trên [S13-15] |
| 15 | Pot Roast trong tường sau bộ cầu thang cuối lên đoạn tiếp theo; Boomerang ở đầu stage [S13-15][BO] |
| 16 | Axe ngay đầu stage; Stop Watch gần đoạn con Phantom Bat thứ 2 (chú thích 2) [S16-18] |
| 17 | Pot Roast trong tường (2.2) [S16-18] |
| 18 | Boomerang ở đầu stage; Holy Water trong nến trái cùng phòng Dracula; có thể "farm" Double/Triple Shot bằng cách phá nến tầng dưới nhiều lần bằng Boomerang [S16-18][BO] |

---

## §3 Bảng ENEMY (kẻ địch thường)

[EN] mở đầu: ngoài 6 boss, game có **15 kẻ địch thường**. Một số chết bằng 1 đòn với mọi vũ khí, nhiều loại cần nhiều đòn và số đòn phụ thuộc sức mạnh vũ khí. Vũ khí yếu (weak): Leather Whip, Holy Water, Dagger. Vũ khí mạnh (strong): Chain Whip (ngắn và dài), Boomerang, Axe [EN].

### 3.1 Bảng tổng hợp (sắp theo điểm tăng dần, như bảng "Comparison" của [EN])

Ghi chú cột: "Hits" = số đòn để diệt, cột "HP" và "HP/2" của bảng Comparison [EN] trùng với Hits Weak và Hits Strong. Tên JP/US: wiki không nêu (chỉ có tên Anh dưới đây). "Sát thương" = theo bảng stage ở §1.3, suy ra từ stage xuất hiện (suy luận): Stage 1-6 = 2, 7-12 = 3, 13-18 = 4.

| ID | Tên (EN) | Tên khác (trong wiki) | Xuất hiện ở Stage | Hits Weak / Strong | Sát thương gây cho Simon | Điểm | Hành vi / kiểu tấn công | Đạn / vật phóng | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|
| E01 | Ghoul | "lost souls" | 1, 3 | 1 / 1 | St1: 2; St3: 2 | 100 | Đi thẳng một hướng, băng ngang màn hình rồi rời đi; hay đi nhóm 2-3 từ cả hai phía | Không | Gần như vô hại, chủ yếu gây xao nhãng |
| E02 | Bat | Black Bat, Red Bat | 2, 4, 10 | 1 / 1 | St2: 2; St4: 2; St10: 3 | 200 | Black Bat treo trần rồi lao xuống khi Simon lại gần; Red Bat bay sóng lên xuống | Không | Có thể hất Simon khỏi bệ nổi (Stage 10) |
| E03 | Panther | — | 1 | 1 / 1 | 2 | 200 | Nằm chờ; khi Simon vào tầm thì nhảy vọt tới; rơi xuống tầng thấp nếu cần và luôn quay về phía Simon | Không | Có ít nhất 3 con ở Stage 1 [SE] |
| E04 | Raven | — | 7-9 | 1 / 1 | 3 | 200 | Đậu chờ; cất cánh khi Simon lại gần, treo giây lát rồi bổ nhào; nếu trượt thì quay đầu rất nhanh đánh từ sau | Không | Holy Water không hiệu quả [S7-9] |
| E05 | Eagle | giant Eagle | 11, 17 | 1 / 1 | St11: 3; St17: 4 | 300 | Bay ngang mang theo Fleaman, thả xuống gần Simon rồi bay đi | Không (mang Fleaman) | Giết Eagle khi còn mang Fleaman thì cả hai chết cùng lúc |
| E06 | Fishman | — | 2, 10 | 1 / 1 | St2: 2; St10: 3 | 300 | Nhảy lên từ nước; nếu chạm đất thì đi bộ và bắn cầu lửa | Cầu lửa (khi ở trên đất) | Có thể hất Simon khỏi bệ nổi khi vừa nhảy lên |
| E07 | Medusa Head | gorgon heads | 5, 6, 8, 9, 15 | 1 / 1 | St5-6: 2; St8-9: 3; St15: 4 | 300 | Xuất hiện từ hướng Simon đang quay mặt, bay ngang theo đường vòng cung/sóng; quay lưng thì đến từ cả hai phía | Không | Có thể húc Simon vào hố (Stage 5) |
| E08 | Skeleton | White Skeleton (alt ảnh) | 7-9, 13, 17 | 1 / 1 | St7-9: 3; St13, 17: 4 | 300 | Nhanh, nhảy nhiều; ném xương theo vòng cung, hay 2-3 xương cùng lúc theo các đường khác nhau | Xương (bone) | Nguy hiểm nhất khi đứng trên gờ cao không với tới |
| E09 | Ghost | "souls of lost men" | 6, 7, 15 | 3 / 2 | St6: 2; St7: 3; St15: 4 | 300 | Xuất hiện ở các điểm cố định, chậm nhưng bám theo vị trí Simon và tiếp cận từ góc lạ | Không | Cần ≥ 2 đòn |
| E10 | Red Skeleton | "enchanted skeleton" | 13-15 | 1 / 1 | 4 | 400 | Đi chậm đuổi theo; bị đánh thì đổ thành đống xương rồi đứng dậy sau 1-2 giây | Không | KHÔNG thể giết (hồi sinh) |
| E11 | Spear Knight | — | 4, 5 | 3 / 2 | 2 | 400 | Đi tới lui; không tấn công bằng đâm, gây hại khi va chạm; có thể quay lại bất cứ lúc nào | Không | Kẻ địch thường đầu tiên không chết bằng 1 đòn |
| E12 | Bone Tower | tower of skulls | 6, 8, 9, 15 | 8 / 6 | St6: 2; St8-9: 3; St15: 4 | 400 | Bất động; theo chu kỳ phun 2 cầu lửa; nhấp nháy trước khi bắn | 2 cầu lửa mỗi lượt | Cầu lửa quất vỡ được |
| E13 | Fleaman | Hunchback | 7, 11*, 13, 14, 17* | 1 / 1 | St7, 11: 3; St13, 14, 17: 4 | 500 | Ngẫu nhiên; kiểu thường gặp: 2 bước nhảy ngắn rồi 1 cú nhảy vọt lớn | Không | * = do Eagle mang tới. Không cần ngồi để đánh |
| E14 | Bone Dragon | skeletal serpentine dragon | 11, 12 | 8 / 7 | 3 | 1000 | Gắn vào tường, thân bất động, cổ ngọ nguậy lên xuống; bắn cầu lửa từ miệng theo các góc | Cầu lửa | Chỉ 3 con trong cả game (1 ở Stage 11, 2 ở Stage 12) |
| E15 | Axe Knight | — | 14, 15 | 10 / 9 | 4 | 1400 | Ném rìu cao hoặc thấp; rìu bay đi rồi quay về như Boomerang; hay lùi khi Simon tiến | Rìu (quay về) | Khó hơn vài boss đầu |

Kẻ địch/thực thể phụ ngoài 15 loại trên: Phantom Bat (phiên bản kẻ địch thường ở Stage 16, 5 con, có thể phun cầu lửa [BO][S16-18]) · Igor (bất tử, ném cầu lửa, đi cùng Frankenstein's Monster [BO][S10-12]) · Snake (rắn của Queen Medusa, chết bằng 1 đòn roi [BO]). Chi tiết ở §4.

Số liệu thiếu cho cả 15 loại: tốc độ di chuyển, tầm kích hoạt, tần suất/giới hạn spawn, tỉ lệ rơi item, thời gian chờ giữa các đòn.

### 3.2 Kẻ địch theo stage (tổng hợp từ cột Stage của [EN] và văn bản từng stage)

| Stage | Kẻ địch thường | Ghi chú |
|---|---|---|
| 1 | Ghoul, Panther | Không có kẻ địch ở phần ngoài trời; chỉ xuất hiện khi vào lâu đài [S1-3] |
| 2 | Fishman, Bat | |
| 3 | Ghoul (sinh ra từ phía trên) | Boss Phantom Bat |
| 4 | Spear Knight, Bat | Chỉ có 2 loại này theo [S4-6] |
| 5 | Medusa Head, Spear Knight | Văn bản Stage 5 chỉ nhắc Medusa Head; [EN] liệt kê thêm Spear Knight |
| 6 | Bone Tower, Ghost, Medusa Head | Bẫy gai; boss Queen Medusa |
| 7 | Ghost, Fleaman, Skeleton, Raven | [S7-9] nói có 4 loại |
| 8 | Medusa Head, Skeleton (tầng dưới); Bone Tower, Raven (tầng trên) | |
| 9 | Raven, Skeleton, Bone Tower, Medusa Head | Văn bản Stage 9 chỉ nhắc Raven, Skeleton; boss Mummy Men |
| 10 | Fishman, Bat | Bệ nổi trên nước |
| 11 | Eagle (mang Fleaman), 1 Bone Dragon | Eagle không ngừng xuất hiện tới cuối stage |
| 12 | 2 Bone Dragon | Boss Frankenstein's Monster + Igor |
| 13 | Fleaman, Skeleton, Red Skeleton | 2 Fleaman chờ sẵn ngay đầu stage |
| 14 | Axe Knight (2 con ở đầu stage), Red Skeleton, Fleaman | [S13-15] chỉ nhắc Axe Knight |
| 15 | Bone Tower, Axe Knight, Medusa Head, Ghost, Red Skeleton | Boss Death |
| 16 | Phantom Bat × 5 (bắn cầu lửa) | |
| 17 | Skeleton, Eagle + Fleaman | |
| 18 | — | Dracula |

### 3.3 Đạn / vật phóng của kẻ địch thường

| Nguồn | Vật phóng | Chi tiết (theo wiki) | Phá được bằng roi? |
|---|---|---|---|
| Axe Knight | Rìu | Ném cao hoặc thấp; bay đi một đoạn rồi quay về chỗ Knight | Có (nên quất để rìu không quay về) [EN] |
| Bone Tower | 2 cầu lửa | Theo chu kỳ; tháp nhấp nháy trước khi bắn | Có [EN] |
| Bone Dragon | Cầu lửa | Phun từ miệng theo các góc khác nhau | Có (đừng để lọt) [S10-12] |
| Fishman | Cầu lửa | Chỉ khi nó đã chạm đất và đi bộ | Wiki không nêu |
| Skeleton | Xương | Ném vòng cung, 2-3 xương cùng lúc theo các đường khác nhau | Wiki không nêu |
| Phantom Bat (Stage 16) | Cầu lửa | | Wiki không nêu |
| Ghi chú chung | | Một số đạn đáng 100 điểm [GP] | |

### 3.4 Đặc tả hành vi từng loại (diễn đạt lại; dùng để dựng state machine)

Định dạng: Spawn · Di chuyển · Điều kiện tấn công · Né/diệt · Dữ liệu thiếu.

**E01 Ghoul.** Spawn: nhóm 2-3 con, xuất hiện từ cả hai mép màn hình [EN]; ở Stage 3 chúng sinh ra từ vùng phía trên rồi rơi xuống bậc thấp [S1-3]. Di chuyển: bước thẳng theo một hướng, chậm, như bị thôi miên; băng qua màn hình rồi rời đi [EN]. Tấn công: không có đòn riêng; chỉ cản trở khi bạn đang đánh kẻ khác (suy luận: sát thương khi va chạm). Né/diệt: vì chậm nên một cú quất có thể diệt nhiều con; khi cả nhóm kéo đến hãy quất liên tục không ngừng [S1-3]. Thiếu: tốc độ, tần suất spawn, số lượng đồng thời.

**E02 Bat.** Spawn: Black Bat treo ở trần; Red Bat bay vào màn hình [EN]. Stage 2: Pot Roast ở tường khối sẽ kích hoạt 1 con Bat khi phá [SE]. Stage 10: dòng Bat liên tục [S10-12]. Di chuyển: Black Bat đậu yên rồi lao xuống khi Simon lại gần; Red Bat bay chậm theo đường sóng lên xuống [EN]; độ cao bay khác nhau nên phải chọn độ cao roi, có thể ngồi quất [S1-3]. Tấn công: va chạm; có thể hất Simon khỏi bệ nổi [S10-12]. Né/diệt: 1 đòn; ném Axe trúng Black Bat trước khi nó bay [EN]; trên bệ nổi hãy ngồi thấp và quất ngay khi thấy chúng [S10-12]. Thiếu: tốc độ, biên độ sóng, khoảng cách kích hoạt.

**E03 Panther.** Spawn: nằm chờ ở chỗ cố định; có ≥ 3 con ở Stage 1, con thứ 3 ở bục có khối giấu money bag 400 [SE]. Di chuyển: khi Simon vào tầm thì nhảy vọt tới; nếu cần thì rơi xuống tầng thấp hơn và luôn quay mặt về Simon [EN]. Tấn công: va chạm; đòn nhanh hơn người chơi dự đoán [S1-3]. Né/diệt: quất ngay trước khi nó nhảy [S1-3]; Dagger đánh từ xa trước khi nó di chuyển; Holy Water hiệu quả [EN][S1-3]. Thiếu: tầm kích hoạt, tốc độ nhảy.

**E04 Raven.** Spawn: đậu ở chỗ cố định [EN]. Di chuyển: khi Simon lại gần thì cất cánh, treo (stall) một thoáng rồi bổ nhào hướng Simon; nếu trượt thì quay đầu rất nhanh để đánh từ phía sau; có thể chưa rời hẳn màn hình và chuẩn bị lao lần nữa [EN]. Né/diệt: giết từ xa bằng Axe/Dagger trước khi nó rời chỗ đậu; nếu có Dagger thì nhảy lên và ném từ xa trước khi nó bay [S7-9]; nhảy rồi quất ngay khi nó bắt đầu lao [S7-9]; Holy Water không hiệu quả [S7-9]. Thiếu: tầm kích hoạt, thời gian treo.

**E05 Eagle.** Spawn: chỉ ở vài vị trí; luôn mang theo Fleaman [EN]. Stage 11: Eagle xuất hiện liên tục cho tới hết stage [S10-12]. Di chuyển: bay ngang qua, thả Fleaman xuống gần Simon rồi bay đi [EN][S10-12]. Né/diệt: thường nên bỏ qua con chim; giết Eagle khi nó còn mang Fleaman thì cả hai chết cùng lúc [EN]. Stage 17: dùng Stopwatch (có 10 tim) ngay khi chúng xuất hiện, sang màn hình kế dùng tiếp [S16-18]. Thiếu: tốc độ bay, tần suất.

**E06 Fishman.** Spawn: nhảy lên từ mặt nước [EN]. Di chuyển: nếu chạm đất thì đi bộ quanh [EN]. Tấn công: bắn cầu lửa khi đã trên đất [EN]; va chạm; nguy hiểm vì có thể hất Simon khỏi bệ nổi khi vừa xuất hiện [EN]. Né/diệt: 1 đòn; quất ngay khi vừa lên khỏi nước, trước khi nó kịp tấn công [S1-3]. Thiếu: vị trí/tần suất nhảy.

**E07 Medusa Head.** Spawn: xuất hiện từ hướng Simon đang quay mặt [EN]; nếu quay lưng thì sinh ra từ cả hai phía [EN]. Di chuyển: bay ngang màn hình theo đường vòng cung/sóng [EN]. Né/diệt: nhảy qua hoặc đi/ngồi dưới; muốn đánh thì phải canh cho nó ngang tầm roi [EN]; quất khi ngồi (Stage 5) [S4-6]; có thể bị húc rơi hố ở Stage 5; Boomerang cũng hiệu quả [S13-15]. Thiếu: biên độ sóng, tốc độ, tần suất.

**E08 Skeleton.** Spawn: đứng chờ ở vị trí cố định (vd góc dưới-trái Stage 7) [S7-9]. Di chuyển: nhanh, nhảy tới lui nhiều [EN][S7-9]; chọn khoảng cách thuận lợi để tấn công [S13-15]. Tấn công: ném xương theo vòng cung, hay 2-3 xương cùng lúc theo các đường khác nhau [EN]. Né/diệt: quất khi nó đến gần; Axe/Dagger diệt dễ; Holy Water thả đúng chỗ nó sắp đáp thì cháy khi nó chạm lửa [S7-9]; Stage 7: một con có thể bị dụ nhảy rồi rơi xuống hố vì gờ quá cao [S7-9]; Stage 9: hai con nhảy qua đoạn cầu hỏng, diệt con đầu khi nó tới, con thứ hai quất khi nhảy giữa các bục [S7-9]; Stage 13: có thể "đẩy" chúng đi bằng cách điều khiển khoảng cách [S13-15]. Thiếu: tốc độ, số xương, tầm ném.

**E09 Ghost.** Spawn: ở các điểm cố định trong vài stage [EN]. Di chuyển: chậm, bám theo vị trí Simon, đến từ các góc lạ [EN]. Né/diệt: cần ít nhất 2 đòn (weak 3 / strong 2); quất nhanh hoặc tránh khỏi đường bay [EN]. Thiếu: tốc độ.

**E10 Red Skeleton.** Spawn: ở vị trí cố định, hành lang Stage 13-15 [EN][S13-15]. Di chuyển: đi chậm đuổi theo (bị phù phép) [S13-15]. Tấn công: va chạm. Né/diệt: không thể giết — bị đánh thì đổ thành đống xương rồi 1-2 giây sau đứng dậy tiếp tục đuổi; quất rồi đi tiếp; sau khi vượt qua chúng bạn có thể bị bao vây [EN][S13-15]. Thiếu: thời gian hồi sinh chính xác (wiki chỉ nói khoảng một hai giây).

**E11 Spear Knight.** Spawn: đứng/đi sẵn trong stage (vd một con đứng dưới hố Stage 4, một con chắn cửa cuối Stage 4) [S4-6]. Di chuyển: đi tới lui; có thể quay lại bất cứ lúc nào; chậm [EN]. Tấn công: không có đòn đâm; chỉ va chạm [EN]. Né/diệt: weak 3 / strong 2; đừng tiếp cận gần khi nó đang quay lưng đi, nhất là sau khi nhảy/thả xuống từ bục; Holy Water và Boomerang hiệu quả; vũ khí bay ném từ xa [EN][S4-6]. Thiếu: tốc độ, phạm vi tuần tra.

**E12 Bone Tower.** Spawn: bất động tại chỗ [EN]. Di chuyển: không. Tấn công: theo chu kỳ phun 2 cầu lửa; tháp nhấp nháy ngay trước khi bắn [EN]. Né/diệt: weak 8 / strong 6; quất trong khoảng nghỉ giữa các lần phun và quất vỡ cả cầu lửa; nhảy qua thì bị bắn từ phía sau; Boomerang và Holy Water hiệu quả; Axe nếu tháp ở bệ phía trên [EN][S7-9]. Thiếu: chu kỳ phun (giây), tốc độ cầu lửa.

**E13 Fleaman (Hunchback).** Spawn: đứng chờ sẵn, hoặc được Eagle thả xuống (đánh dấu * ở Stage 11, 17) [EN]; Stage 13: 2 con chờ ngay đầu stage [S13-15]. Di chuyển: nhìn như ngẫu nhiên; kiểu điển hình 2 bước nhảy ngắn rồi 1 cú nhảy vọt [EN]. Tấn công: va chạm khi nhảy tới. Né/diệt: chạy chui dưới cú nhảy cao rồi đánh ngay khi nó tiếp đất; không cần ngồi; đừng để chúng đến gần/ bao vây/ tụ thành nhóm [EN]; nếu chúng áp sát, để chúng nhảy qua đầu rồi quất từ phía sau [S13-15]; Holy Water/Boomerang ném ngay từ xa [S13-15]. Stage 11: Fleaman gần như luôn rơi đúng tầm roi — đứng yên và quất ngay trước khi nó chạm đất; không để nhóm lớn hình thành [S10-12]. Thiếu: độ cao nhảy, thời gian giữa các lần nhảy.

**E14 Bone Dragon.** Spawn: gắn cố định vào tường/khối; chỉ 3 con trong game [EN]. Di chuyển: thân bất động, đầu và cổ ngọ nguậy lên xuống [EN][S10-12]. Tấn công: phun cầu lửa theo các góc; chỉ gây hại nếu bạn lại quá gần hoặc bị cầu lửa [S10-12]. Né/diệt: weak 8 / strong 7; đứng xa nhất có thể mà vẫn trong tầm; Boomerang hiệu quả nhất, Axe cũng tốt; với multiplier ném liên tiếp có thể giết trước khi nó kịp bắn; chờ đầu hạ thấp rồi quất, hoặc nhảy quất; luôn sẵn sàng quất cầu lửa [EN][S10-12]. Stage 11: nếu cứ đi thẳng khi rồng sắp xuất hiện thì chui qua dưới nó tới cửa mà không cần đánh [S10-12]. Rơi 5-8 money bag hoặc small heart khi chết; nếu bị Holy Water thì rơi shot multiplier [EN]. Stage 12: quất khối mà rồng 1 bám để lấy Pot Roast; sau khi diệt rồng 2 phá khối sau nó lấy Large Heart [S10-12][SE]. Thiếu: chu kỳ bắn, góc bắn.

**E15 Axe Knight.** Spawn: đứng sẵn trên sàn hoặc bệ cao (Stage 14: 2 con ngay đầu stage, 1 con trên gờ cao trước lối ra; Stage 15: một loạt Axe Knight tuần tra sàn) [S13-15]. Di chuyển: hay lùi khi Simon tiến, giữ khoảng cách, không bao giờ cố tiến quá gần; bị đẩy khỏi mép màn hình thì không quay lại [EN][S13-15]. Tấn công: ném rìu cao hoặc thấp (phải quất đúng độ cao); rìu bay đi một đoạn rồi quay về như Boomerang [EN]. Né/diệt: weak 10 / strong 9; Holy Water hoặc Axe (ném từ bên dưới); quất rìu để nó không quay về [EN]. Stage 14: Knight thấp ở đầu stage có thể "đẩy" ra khỏi màn bằng cách cứ tiến tới cho đến khi sát mép phải màn hình thì nó không quay lại; Knight cao: thả Holy Water lên mép bệ nó đứng rồi đi sang phải để nó bước qua lửa, chết sau 2-3 lần; hoặc đẩy khỏi màn rồi quất liên tục [S13-15]. Stage 15: không nên đẩy chúng suốt tới phòng Death vì bị rìu áp đảo, hãy dùng Holy Water/Boomerang [S13-15]. Thiếu: tần suất ném, tốc độ rìu, tầm lùi.

**Phantom Bat thường (Stage 16).** 5 con; bay đến theo vị trí khởi đầu đánh số trên bản đồ; có thể phun cầu lửa; nên tránh hơn là đánh [BO][S16-18]. Con khởi đầu phía trên cầu: nhảy lên quất 1 phát làm chậm rồi cứ chạy tiếp, cuộn chúng ra khỏi màn; con khởi đầu phía dưới cầu: thường xuất hiện lúc bạn nhảy qua khe → vừa tiếp đất phải nhảy ngay để vượt lên trên con dơi đang bay qua; con cuối: quất giữa không trung để làm chậm hoặc chạy dưới nó tới lối ra [S16-18]. Thiếu: HP của phiên bản thường.

---

## §4 Bảng BOSS

6 boss (5 boss trung gian + Dracula) [GP][EN]. Sát thương mà MỌI đòn của boss gây ra = mức của stage đang chơi (§1.3): Stage 3 và 6 = 2; Stage 9 và 12 = 3; Stage 15 và 18 = 4 [GP]. Thanh máu boss hiển thị 16 đoạn (đếm từ HUD ảnh chụp [IMG]); wiki chỉ nêu số đòn, không nêu "vạch" cho từng boss. Số đòn Weak/Strong = số đòn đủ để hạ khi dùng vũ khí yếu/mạnh (định nghĩa ở §3). Tỉ lệ Weak:Strong của mọi boss đều đúng 3:2 (12/8, 48/32, 24/16), khớp với "chain whip gấp 1.5 lần" [IT] (suy luận).

### 4.1 Bảng tổng hợp [BO]

| ID | Tên | Stage (Block) | HP: số đòn Weak / Strong | Sát thương gây ra | Điểm | Giai đoạn / pha | Đòn tấn công | Điểm yếu / vũ khí khuyến nghị | Ghi chú |
|---|---|---|---|---|---|---|---|---|---|
| B1 | Phantom Bat (giant black Phantom Bat; Vampire Bat) | 3 (Block 1) | 12 / 8 | 2 | 3000 | 1 pha | Bay tới các vị trí ngẫu nhiên, thỉnh thoảng bổ nhào xuống tấn công | Axe, Stop Watch, Holy Water | Item trong phòng: Double Shot (khối ở chân cầu thang). Stopwatch có tác dụng. Dễ nhất trong các boss [S1-3] |
| B2 | Queen Medusa | 6 (Block 2) | 12 / 8 | 2 | 3000 | 1 pha | Lao thẳng vào Simon; thả rắn bò trên sàn về phía Simon | Boomerang, Holy Water, Stop Watch | Item trong phòng: không có. Boss CUỐI CÙNG mà Stopwatch còn tác dụng |
| B3 | Mummy Men (2 Mummy) | 9 (Block 3) | 48 / 32 (wiki ghi theo "Mummies", không nói là tổng hay mỗi con; (suy luận) tổng 2 con) | 3 | 3000 | 1 pha, 2 thực thể | Đi lang thang trong phòng, thỉnh thoảng ném băng gạc; cố kẹp Simon từ hai phía | Boomerang, Holy Water, Whip | Item trong phòng: Pot Roast (khối thấp nhất của cầu thang trái). Stopwatch vô dụng |
| B4 | Frankenstein's Monster & Igor | 12 (Block 4) | 24 / 16 (chỉ Monster; Igor bất tử) | 3 | 5000 | 1 pha, 2 thực thể | Monster đi lang thang; Igor nhảy từ phía này sang phía kia và ném cầu lửa | Axe, Boomerang, Holy Water | Item trong phòng: không có (Dagger nằm ngoài phòng) |
| B5 | Death (Grim Reaper) | 15 (Block 5) | 24 / 16 | 4 | 7000 | 1 pha | Bay lơ lửng ngẫu nhiên (ưu tiên các góc), tạo lưỡi hái tự đuổi theo Simon; không tấn công trực tiếp | Boomerang, Holy Water, Axe | Item trong phòng: không có. Boss khó nhất theo wiki ("arguably the toughest") |
| B6 | Count Dracula | 18 (Block 6) | 24 / 16 (bảng [BO], không nói rõ dạng); dạng 2: 32 đòn [S16-18] | 4 | 50,000 [BO] ([GP] nói Dracula không có điểm — mâu thuẫn) | 2 dạng: Count Dracula (teleport) → demon form | Dạng 1: dịch chuyển, mở áo choàng, bắn 3 cầu lửa rồi biến mất. Dạng 2: nhảy quanh phòng, phun lửa | Whip (đầu), Holy Water, Boomerang | Item trong phòng: Holy Water (nến trái cùng). Chỉ đầu bị thương |

### 4.2 Quy tắc chung của trận boss

- Khi vào phòng boss màn hình bị khóa (suy luận từ [SE] Stage 9: "trước khi màn hình bị khóa") [SE].
- Hạ boss → Magic Crystal → chạm vào: hồi đầy máu, cộng thưởng thời gian ×10 và tim ×100 (§1.10) [WT][GP].
- Phantom Bat, Queen Medusa: Stopwatch có tác dụng; các boss còn lại miễn nhiễm [IT][BO].
- Glitch của bản gốc (không cần tái hiện): khi Simon và kẻ địch trúng nhau ở cùng một khung hình, kẻ địch nhận sát thương rất lớn (Phantom Bat chết ngay; boss khác bị thương nặng); vũ khí gây sát thương MỖI khung hình cho kẻ địch chồng lên nhau (Mummy Men chồng nhau + nhiều Boomerang với multiplier → chết gần như tức thì); Boomerang × Triple Shot khi đánh Death có thể làm treo game [BO][S7-9].

### 4.3 Đặc tả hành vi từng boss theo pha (diễn đạt lại)

**B1 Phantom Bat (Stage 3).** Vào phòng, con dơi đen khổng lồ lao xuống từ vòm cao [S1-3]; (suy luận từ ảnh bản đồ: nó treo ở trần như một khối tròn tối có hai mắt, khoảng native x≈622 [IMG]). Hành vi: bay tới các điểm ngẫu nhiên và thỉnh thoảng bổ nhào về phía Simon [BO]. Né: đứng bên trái cầu thang (nhiều chỗ để đánh và né hơn); khi nó bổ nhào thì ngồi xuống hoặc né sang bên [BO]. Cách đánh: (1) Axe lấy từ nến vài màn hình phía sau (nến cạnh cầu thang Stage 3): ném thẳng vào dơi; (2) Double Shot nằm ở khối phải nhất của bục dưới cầu thang → hạ nhanh hơn; (3) Stopwatch: chờ dơi sà xuống trong tầm roi, dùng Stopwatch rồi quất tới khi hết đóng băng; hoặc đóng băng lại ngay / né vì nó tiếp tục tấn công ngay khi cử động được; (4) Holy Water: đứng góc dưới-phải, ném vào bục khi dơi bổ nhào thì nó bị kẹt trong lửa [BO]. Có thể dùng roi cùng Stopwatch [S1-3]. Thiếu: tốc độ bay, chu kỳ bổ nhào, vị trí hạ cánh.

**B2 Queen Medusa (Stage 6).** Xuất hiện: vỡ ra từ bức tượng ở giữa phòng, không có báo trước; đứng bên phải tượng chờ [BO]. Hành vi: lao thẳng vào Simon, hầu như luôn bay thấp sát đất [BO][S4-6]; khi bay thả ra rắn trườn dưới sàn về phía Simon; rắn chết bằng 1 đòn roi [BO]. Cách đánh: Holy Water (dễ nhất vì nàng bay thấp) hoặc Boomerang, đặc biệt với Double/Triple Shot (nàng bị thương nhanh đến mức hầu như không kịp di chuyển); Stopwatch đóng băng cả nàng lẫn đám rắn rồi quất liên tục; roi/Boomerang đánh liên tục cũng hiệu quả [BO][S4-6]. Item để chuẩn bị: Holy Water hoặc Boomerang lấy gần đầu Stage 4 (Boomerang ở nến đầu tiên) [BO][S4-6]. Thiếu: tốc độ, chu kỳ thả rắn.

**B3 Mummy Men (Stage 9).** Hai Mummy lang thang trong phòng, thỉnh thoảng ném băng gạc (gây sát thương) và cố kẹp Simon từ hai phía [BO]. Địa hình: có cầu thang trái; khối thấp nhất của cầu thang trái chứa Pot Roast — wiki khuyên để dành tới khi máu rất thấp, vì đứng lên khối đó là bục đánh tốt và cửa phòng là chỗ lui tương đối an toàn [BO][S7-9]. Cách đánh: dụ chúng lại gần rồi đánh. Holy Water là lựa chọn hàng đầu: đóng băng chúng tại chỗ (wiki gọi là hóa đá) [BO][S7-9]; Boomerang tốt ngang, tầm xa hơn Holy Water/roi, nhảy qua Boomerang khi nó quay về thì nó trúng cả Mummy kia [BO]. Không có vũ khí phụ: ngồi và quất khi chúng áp sát; lui lên khối cao khi chúng quá gần; giữ cả hai ở bên phải để không bị bao vây; nếu chúng cùng đi vào góc trái trong khi bạn ngồi trên khối Pot Roast còn nguyên thì chúng không đánh trúng bạn được nữa [BO]. Nếu đang cầm Stopwatch (vô dụng với Mummy Men) thì nhặt Dagger trong nến phòng boss [S7-9]. Thiếu: tốc độ, chu kỳ ném băng.

**B4 Frankenstein's Monster & Igor (Stage 12).** Monster đi lang thang, thường ở nửa phải màn hình; Igor (giống Fleaman nhưng BẤT TỬ) nhảy hết sang một phía phòng rồi nhảy ngược lại, ném cầu lửa; Igor thường nhảy tiến lên khi ở giữa màn hình và nhảy cao khi tới bục ở các góc [BO]. Né: nấp ở mép trong của các bục; Igor không đi hoàn toàn theo quy luật; ở gần bục để nhảy qua Monster/Igor khi bị kẹp; lúc mới vào phòng, chạy nhảy lên mép bục dưới bên phải để đánh nhanh vài đòn trước khi Igor bắt đầu di chuyển [BO]. Không đánh được Igor để tiêu diệt, nhưng quất Igor một lần sẽ đóng băng nó; khi đó dồn đòn vào Monster, theo dõi Igor và quất lại ngay khi nó hết đóng băng [S10-12]. Với Holy Water hoặc Boomerang có multiplier: gây sát thương lớn, thậm chí hạ Monster trước khi Igor đánh trúng; không multiplier thì Monster/Igor thoát choáng gần như ngay, vẫn lấy được khoảng 1/4 đến gần 1/3 máu; sau đó đánh tiếp mỗi khi có cơ hội. Dagger (nằm ngoài phòng, cũng là vũ khí duy nhất nếu bạn chết) dùng cách tương tự Boomerang nhưng cần nhiều đòn; Axe tốt vì dễ làm Igor choáng giữa không trung và có thể trúng Monster lúc rơi xuống [BO]. Thiếu: tốc độ, tầm ném lửa, tần suất.

**B5 Death (Stage 15).** Bay lơ lửng ngẫu nhiên quanh phòng, ưu tiên các góc [BO]. Không tấn công trực tiếp; tạo ra tối đa 3 lưỡi hái, mỗi cái đứng yên khoảng một giây rồi chậm rãi lao về phía Simon, tăng dần tốc độ [BO]. Né: phá lưỡi hái ngay khi vừa tạo ra nếu được; lùi bớt khi chúng áp sát; nấp góc nhưng đừng ở lâu vì Death có thể bay tới [BO]. Cách đánh: Holy Water ném lên bục trên-phải để giết Death trước khi nó kịp tấn công (đứng sát mép; ném khi nó chưa vào vị trí; nếu nó thoát choáng và bay lên trên bục thì Holy Water vô dụng vì không thể làm kẹt lần nữa); hoặc ném liên tục từ bục giữa để nó không kịp vào vị trí tạo lưỡi hái [S13-15][BO]. Axe dễ trúng cả Death lẫn lưỡi hái ở trên cao, đánh từ xa; Boomerang (nhặt ở đầu stage) tầm xa hơn Axe nhưng phải nhảy rồi ném; roi khó dùng vì phải sát Death và lưỡi hái [BO]. Sau khi Death chết, lưỡi hái vẫn còn nên có thể chết khi đi nhặt Crystal Ball [BO]. Thiếu: tốc độ lưỡi hái theo thời gian, chu kỳ sinh lưỡi hái, tầm đổi chỗ.

**B6 Count Dracula (Stage 18).** Vào phòng thì nhặt hết nội dung nến, kể cả nến trái cùng chứa Holy Water; để dành Holy Water cho dạng 2 [BO]. Dạng 1 (Count Dracula): bắt đầu bằng việc dịch chuyển đến một vị trí ngẫu nhiên, mở áo choàng rồi bắn 3 cầu lửa, sau đó biến mất; CHỈ đầu bị thương [BO]. Mẹo: chờ ông ta hiện, đứng cách đúng tầm roi khi ông ta hiện hình hoàn toàn, nhảy và quất vào mặt trước khi ông ta lại biến mất; nếu nhảy đúng lúc thì 3 cầu lửa hoặc bay lên cao qua đầu bạn khi rơi, hoặc bắn quá thấp để nhảy qua; nhảy quá sớm thì cầu lửa bắn chếch lên, hãy đợi cầu lửa bắn ra rồi mới nhảy; cầu lửa bị phá đôi khi sinh ra tim [BO][S16-18]. Vũ khí phụ ít hiệu quả vì ông ta không ở lâu (trừ Boomerang + Double/Triple Shot); nên dùng roi và để dành tim cho dạng 2 [S16-18]. Chuyển dạng: khi hết vạch máu cuối, đầu bay ra và thân biến thành quỷ [BO]. Dạng 2 (demon): không còn dịch chuyển nên luôn có thể bị đánh; nhảy quanh phòng (cao hoặc thấp), thỉnh thoảng phun lửa; phải đánh vào ĐẦU, cần 32 đòn [S16-18]; để khỏi bị dồn vào góc hãy chạy chui dưới thân khi nó nhảy cao; Holy Water không gây sát thương (chân không nhận sát thương) nhưng làm nó choáng hoàn toàn khi lửa còn cháy, trong lúc đó có thể đi xuyên qua nó mà không bị thương để đổi bên; Boomerang + Triple Shot ném nhanh gây khá nhiều sát thương, hất cầu lửa và giữ nó đứng yên [BO][S16-18]. Mẹo: hung hăng tấn công liên tục [S16-18]. Ở Stage 18 trước trận: nhặt Boomerang ngay đầu stage; có thể xuống tầng dưới phá nến lặp lại bằng Boomerang để ra Double rồi Triple Shot [S16-18]. Thiếu: tốc độ, vị trí dịch chuyển (ngẫu nhiên), chu kỳ nhảy/phun lửa.

---

## §5 Walkthrough tổng: 18 Stage / 6 Block

Cấu trúc [WT]: lâu đài chia 6 "leg" (= Block), mỗi leg 3 stage; game hoàn toàn tuyến tính, không thể bỏ qua stage nào. Cuối mỗi leg là một boss. Sau mỗi boss Simon nhận 1 Magic Crystal (có 5 crystal trước Dracula; crystal cuối sau Dracula kết thúc game [S16-18]): hồi đầy máu và cộng thưởng thời gian còn lại + tim còn lại. Khi Simon đi từ leg này sang leg khác, bản đồ lâu đài cuộn vào màn hình khi Simon bước ngang qua nó, cho thấy vị trí hiện tại và vị trí boss kế tiếp [WT]. (Ảnh bản đồ lâu đài của trang có 6 ô đỏ ứng với 6 boss; wiki không ghi ô nào là boss nào [IMG].)

Thời gian mỗi stage: wiki không nêu (xem §1.10). Checkpoint giữa stage: wiki không nêu; luật duy nhất là hết mạng thì Continue từ ĐẦU stage đang chơi [GP].

| Block | Stage | Khu vực / mô tả (theo wiki) | Kẻ địch chính | Boss | Ghi chú nổi bật (item, bí mật, mẹo cấu trúc) |
|---|---|---|---|---|---|
| 1 | 1 | Phần A: sân ngoài lâu đài tới cổng; Phần B: sảnh vào lâu đài. Wiki xếp Block 1 là các stage luyện tập | Ghoul, Panther (không có kẻ địch ở phần ngoài trời) | — | 2 đuốc Morning Star trước cổng; Holy Water ở nến đỉnh cầu thang; Rosary ở đỉnh cầu thang cuối (hết stage khi qua cầu thang); bí mật: money bag 1000 (nhảy qua cửa), purple bag 400 (khối trên bục Panther thứ 3). Chi tiết §6 |
| 1 | 2 | Ngắn; phải đi qua lối ngầm có nước (underground path); mất mạng nếu rơi nước | Fishman, Bat | — | Pot Roast trong tường khối (phá khối thấp nhất, sẽ có 1 Bat); bonus money bag ở cuối đoạn nước. Chi tiết §6 |
| 1 | 3 | Cầu thang đi xuống tới phòng boss | Ghoul (spawn từ trên) | Phantom Bat | Axe trong nến giữa cầu thang cuối; Invisibility Potion giữa cầu thang; Shot Multiplier ở chân cầu thang cạnh phòng boss. Chi tiết §6, §4 |
| 2 | 4 | Ngắn, tương đối dễ; số kẻ địch và độ khó tăng dần từ leg này [S4-6] | Spear Knight, Bat | — | Boomerang trong nến đầu tiên; Crown 2000 (phá khối tường phải sau cầu thang đầu); Shot Multiplier ở góc bục có Spear Knight; 1 Spear Knight chắn cửa cuối stage |
| 2 | 5 | Có nhiều hố; Medusa Head xuất hiện ngay từ đầu | Medusa Head, Spear Knight | — | Dagger trong nến giữa tầng dưới; Treasure Box 2000 (đứng yên trên 2 khối nhô cao); Pot Roast ở bậc cao nhất góc trên-phải; Stop Watch ở nến gần đó; mẹo: cố ý bị Medusa Head húc để bay lên tầng trên (rủi ro hố) |
| 2 | 6 | Bẫy gai (3 cái; cái thứ 2 hạ xuống một đoạn ngắn hơn nhiều nên khó nhất), Bone Tower, Ghost; sảnh có vòm | Bone Tower, Ghost, Medusa Head | Queen Medusa | Pot Roast ở khối đầu stage; Shot Multiplier ở khối chân cầu thang; mẹo gai: đi ngay khi gai bắt đầu hạ; Stopwatch dùng khi gai ở đỉnh |
| 3 | 7 | Kẻ địch có chuyển động phức tạp hơn; sát thương 3 vạch/đòn từ leg này [S7-9] | Ghost, Fleaman, Skeleton, Raven | — | Axe hiệu quả nhất; Holy Water ở một trong các nến đầu; Pot Roast ở bộ khối nhô thứ 2; bonus money bag (ngồi ở mút bục cụt); một Skeleton có thể dụ rơi hố |
| 3 | 8 | Tầng dưới và tầng trên | Medusa Head, Skeleton (dưới); Bone Tower, Raven (trên) | — | Boomerang trong nến dưới cầu thang lên tầng trên; Stop Watch trong nến bên phải cầu thang; bonus money bag (quỳ phía trên 2 khối lơ lửng) |
| 3 | 9 | Cầu có đoạn hỏng; phòng Mummy Men | Raven, Skeleton, Bone Tower, Medusa Head | Mummy Men | Pot Roast ở khối thấp nhất phòng boss; Dagger trong nến phòng boss |
| 4 | 10 | Hầm ngầm có nước; bệ nổi cách mặt nước vài inch; có đoạn trần thấp phải ngồi | Fishman, Bat (liên tục) | — | Stop Watch trước bục đầu tiên; Holy Water trên đỉnh gờ (hữu ích cho boss và vài Bone Dragon) |
| 4 | 11 | Trên mặt đất; ban đầu tưởng không có kẻ địch rồi Eagle xuất hiện liên tục, bầu trời đầy Eagle | Eagle mang Fleaman; Bone Dragon ×1 (cuối stage) | — | Có thể chui dưới Bone Dragon để tới cửa nếu cứ đi thẳng |
| 4 | 12 | Cống ngầm trông hoang vắng nhưng có đúng 4 cư dân | 2 Bone Dragon | Frankenstein's Monster & Igor | Pot Roast (khối của rồng 1); Large Heart (khối của rồng 2); Dagger ngay ngoài phòng boss |
| 5 | 13 | Sảnh không có bước nhảy/hố khó; sát thương 4 vạch/đòn (chỉ chịu 4 đòn) [S13-15] | Fleaman (2 con chờ ở đầu stage), Skeleton, Red Skeleton | — | Holy Water ngay đầu stage (duy nhất); Shot Multiplier (khối góc dưới-phải trần thấp); Pot Roast gần cuối; 1UP bí mật |
| 5 | 14 | Axe Knight xuất hiện nhiều (2 con ngay đầu stage) | Axe Knight, Red Skeleton, Fleaman | — | Axe trong nến gần Knight đầu; Shot Multiplier ở tường trái của tường trên; 3 chỗ money bag bí mật |
| 5 | 15 | Leo bậc thang qua nhiều Bone Tower rồi hành lang cuối | Bone Tower, Axe Knight, Medusa Head, Ghost, Red Skeleton | Death | Pot Roast trong tường sau cầu thang cuối lên đoạn tiếp theo; Boomerang ở đầu stage; có thể nhặt multiplier trước Death nếu dùng vũ khí phụ tốt |
| 6 | 16 | Cầu dài dẫn tới tháp chuông | Phantom Bat × 5 (có cầu lửa) | — | Axe ngay đầu; Stop Watch ở khoảng chú thích 2; mẹo: đánh dơi ở trên cao 1 lần giữa không trung rồi chạy tiếp |
| 6 | 17 | Tháp đồng hồ; bánh răng; bắt đầu chỉ có Skeleton | Skeleton, Eagle + Fleaman | — | Trên cầu thang không thể bị đánh văng; Pot Roast trong tường (rủi ro); money bag bí mật ở bánh răng thấp nhất; Stopwatch + 10 tim dùng ngay khi bầy Eagle xuất hiện |
| 6 | 18 | Phòng Dracula ở điểm cao nhất lâu đài | — | Count Dracula (2 dạng) | Boomerang ở đầu stage; Holy Water ở nến trái cùng phòng boss; có thể "farm" Double/Triple Shot ở tầng dưới |

Bảng boss theo Block: 1 Phantom Bat · 2 Queen Medusa · 3 Mummy Men · 4 Frankenstein's Monster & Igor · 5 Death · 6 Count Dracula.

Sát thương theo Block: 1-2 = 2 vạch; 3-4 = 3; 5-6 = 4 (xem §1.3). Mô tả mở đầu từng leg của wiki: Block 1 là các stage luyện tập, dùng để tập dùng item, nhịp roi, nhảy [S1-3]; Block 2 nhiều kẻ địch hơn, cần đoán chuyển động và nhảy qua chướng ngại/hố [S4-6]; Block 3 kẻ địch phức tạp và mạnh hơn, boss chậm nhưng khỏe [S7-9]; Block 4 xuống lòng đất rồi lên mặt đất, Bone Dragon trước Frankenstein [S10-12]; Block 5 có mối đe dọa lớn nhất, chỉ chịu được 4 đòn [S13-15]; Block 6 trận cuối [S16-18].

Second Quest (Stage 19+) xem §1.13.

---

## §6 Stage 1-3 CHI TIẾT (dùng để dựng Map 1)

### 6.0 Nguồn và quy ước đo

- Văn bản: [S1-3] (+ [SE], [BO], [GP], [EN] khi liên quan). Wiki chỉ mô tả mỗi stage bằng một ảnh bản đồ có chấm số cyan và 4-5 ghi chú đánh số, không có danh sách nến đầy đủ. Vì vậy các tọa độ/nội dung nến dưới đây ĐỌC TỪ ẢNH [IMG] và là xấp xỉ; mọi thứ ảnh không cho thấy được ghi "wiki không nêu".
- Ảnh bản đồ (từ trang Stages 1 - 3): Stage 1 Part A = 384×88 px; Part B = 768×88 px; Stage 2 = 264×176 px; Stage 3 = 384×88 px; ngoài ra 3 ảnh chụp màn hình 256×240 (Stage 1, 2, 3).
- Tỉ lệ: ảnh bản đồ là NỬA tỉ lệ NES (gạch 8×8 px trong ảnh = ô 16×16 NES; sprite nến 4×8 px trong ảnh = 8×16 NES) (suy luận, đã kiểm bằng kích thước gạch và sprite). Mọi tọa độ dưới đây là "native" = NES px = giá trị ảnh × 2.
- Gốc tọa độ: mép trái và mép TRÊN của chính ảnh đó (Part A, Part B, Stage 2, Stage 3 là các ảnh riêng; wiki không nói chúng nối với nhau thế nào). y = 0 là mép trên khung chơi; khung chơi cao 176 px (11 hàng ô 16 px); sàn chính nằm ở y = 160 (hàng ô thứ 11 tính từ trên). Tile col = x_native / 16.
- Độ chính xác: nến ±2 px (đo bằng điểm ảnh); icon, cầu thang, bục ±8 px (đọc bằng mắt/đếm bậc).
- Số cyan trên ảnh = số thứ tự ghi chú trong văn bản wiki. Đã đo vị trí các chấm.
- Nhận diện icon: so khớp với icon của trang Items (Large Heart có tỉ lệ rộng 13:11, Morning Star hình roi đỏ, Dagger lưỡi xanh…). Hai đuốc có tim là Large Heart chứ không phải Small Heart (khớp kích thước/tỉ lệ; và khớp phép cộng tim 5 + 2×5 = 15 trên HUD ảnh chụp).
- Quy ước tầng nến (y_top = đỉnh sprite nến 16 px cao): y_top 128 = "thấp" (đáy nến cách sàn 16 px); 96 = "giữa"; 32 = "cao"; 0 = "đỉnh".

Tổng số vật chứa đo được: Stage 1A 5 đuốc; Stage 1B 24 nến; Stage 2 16 nến; Stage 3 12 nến (57 tổng).

---

### 6.1 STAGE 1

**Tổng quan [S1-3].** Kẻ địch chỉ có Ghoul và Panther. Kẻ địch KHÔNG xuất hiện cho tới khi vào lâu đài (nghĩa là Part A không có kẻ địch). Qua màn khi vượt hết bộ cầu thang cuối cùng của sảnh trong.

#### Part A — sân ngoài lâu đài (ảnh 384×88 → 768×176 px NES, bằng 3 màn hình 256 px) 

- Sàn: y = 160, liền suốt. Cổng lâu đài là tháp gạch ở cuối bên phải: tháp chiếm khoảng x 656-768; vòm cửa khoảng x 684-710 (đã đối chiếu với ảnh chụp màn hình). Chấm #1 tại native (729, 127).

5 đuốc (brazier) đặt trên bệ đá, tâm cách đều 128 px (xấp xỉ x = 95 + 128k, k = 0..4; tile col 6, 14, 22, 30, 38):

| Đuốc | x_native (tâm) | tile col | Nội dung (icon trên ảnh [IMG]) | Hiệu ứng |
|---|---|---|---|---|
| B1 | ≈ 95 | 6 | Large Heart | +5 tim |
| B2 | ≈ 223 | 14 | Morning Star (whip upgrade) | roi da → roi xích (1.5×) |
| B3 | ≈ 351 | 22 | Morning Star | roi xích dài hơn |
| B4 | ≈ 479 | 30 | Large Heart | +5 tim |
| B5 | ≈ 607 | 38 | Dagger | vũ khí phụ |

Văn bản xác nhận phần nâng roi: nâng đủ roi bằng các đuốc dẫn tới cổng [S1-3 note 1]. Ảnh chụp HUD ngay trước cổng: ♥-15, ô vũ khí phụ = Dagger, SCORE 000000, P-03, TIME 0282 [IMG] → 15 = 5 (khởi đầu, suy luận) + 5 + 5.

- Bí mật ở cửa (money bag 1000): nhảy VƯỢT qua cửa lâu đài thì túi tiền nhấp nháy hiện ra ở phía sau; nhảy lùi lại nhặt rồi mới đi vào cửa. Phải canh nhảy rất chuẩn, nếu không sẽ tự đi vào cửa [SE][S1-3 note 1]. Ảnh chụp [IMG]: túi tiền nằm bên TRÁI cửa, khoảng 50-70 px NES trước vòm (khoảng x≈632, ngay cạnh bệ tượng sát đuốc B5), Simon đứng ở/ sau cửa. (Văn bản [S1-3] viết "back on the right side" — xem §7.)
- Ngoài ra: các bệ tượng đá/ hàng rào/ cây chỉ là nền (suy luận).

#### Part B — sảnh trong lâu đài (ảnh 768×88 → 1536×176 px NES, bằng 6 màn hình)

Kẻ địch: Ghoul (đi thẳng, hay theo nhóm 2-3, từ cả hai phía) và Panther (nằm chờ đến khi Simon vào tầm rồi nhảy) [S1-3][EN]. Mẹo: quất liên tục khi Ghoul đến theo nhóm; quất ngay trước khi Panther nhảy; Holy Water hiệu quả với Panther; có ít nhất 3 Panther, con thứ 3 ở bục chứa khối bí mật [S1-3][SE]. Vị trí spawn từng con: wiki không nêu.

Hình học (native px, từ ảnh [IMG]):

| Đối tượng | Vị trí | Ghi chú |
|---|---|---|
| Sàn chính | y = 160, x 0-1536 (liền suốt) | |
| Cầu thang 1 (đi lên sang phải, 8 bậc) | từ (624, 152) tới (688, 96) | tới Bục A |
| Bục A | y = 96, x 688-738 (3 gạch) | |
| Cầu thang 2 (đi lên sang phải, 4 bậc) | từ (720, 88) tới (752, 64) | tới Bục B |
| Bục B (bục cao) | y = 64, x 752-914 (9 gạch; đoạn ≈ x 866-880 bị icon Holy Water che trên ảnh) | có nến #14 (Holy Water) |
| Cầu thang 3 (đi xuống sang phải, 4 bậc) | từ (912, 64) tới (944, 88) | |
| Bục C | y = 96, x 928-1026 (6 gạch); khối cuối ở mép phải (chấm #4 tại (1041, 103)) | khối bí mật (xem dưới) |
| Cầu thang 4 (đi lên sang phải, ≈ 12 bậc) | từ (1296, 152) tới (1392, 64) | "cầu thang cuối" |
| Bục D (bục cuối) | y = 64, x 1392-1538 (8 gạch; icon Rosary che đoạn ≈ x 1442-1456) | có nến #23 (Rosary); cửa/trụ cuối ở mép phải ≈ x 1530 |

Các chấm số trên ảnh (khớp ghi chú của wiki):

| Chấm | Vị trí native | Ghi chú wiki | 
|---|---|---|
| #2 | (172, 100) | Cách đánh Ghoul: dùng roi, khi đi nhóm thì quất liên tục (luyện nhịp) |
| #3 | (783, 100) | Holy Water hiệu quả với Panther; nằm trong nến ở đỉnh cầu thang (Bục B) |
| #4 | (1041, 103) | Đánh khối ở cuối cầu thang bằng roi rồi thả xuống ledge thấp nhất → hiện "bonus treasure bag". Đối chiếu [SE] (suy luận): khối phải nhất của bục có Panther thứ 3 chứa Purple money bag 400 điểm |
| #5 | (1464, 40) | Qua màn khi đi hết cầu thang; một Rosary giấu ở đỉnh cầu thang cuối (diệt mọi kẻ địch trên màn) |

Nến tường Part B (24 cái, lưới đều 64 px NES; x = 32 + 64·(n − 1); sai số đo ≤ 2 px):

| # | x_native | tile col | y_top | Tầng | Nội dung |
|---|---|---|---|---|---|
| 1 | 32 | 2 | 128 | thấp | wiki không nêu |
| 2 | 96 | 6 | 96 | giữa | Stopwatch (icon ngay trên nến [IMG]; văn bản wiki không nhắc) |
| 3 | 160 | 10 | 128 | thấp | wiki không nêu |
| 4 | 224 | 14 | 96 | giữa | wiki không nêu |
| 5 | 288 | 18 | 128 | thấp | wiki không nêu |
| 6 | 352 | 22 | 96 | giữa | wiki không nêu |
| 7 | 416 | 26 | 128 | thấp | wiki không nêu |
| 8 | 480 | 30 | 96 | giữa | wiki không nêu |
| 9 | 544 | 34 | 128 | thấp | wiki không nêu |
| 10 | 608 | 38 | 96 | giữa | wiki không nêu |
| 11 | 672 | 42 | 32 | cao | wiki không nêu |
| 12 | 736 | 46 | 128 | thấp | wiki không nêu |
| 13 | 800 | 50 | 0 | đỉnh | wiki không nêu |
| 14 | 864 | 54 | 32 | cao (rơi xuống Bục B) | Holy Water [S1-3 note 3][IMG] |
| 15 | 928 | 58 | 128 | thấp | wiki không nêu |
| 16 | 992 | 62 | 32 | cao | wiki không nêu |
| 17 | 1056 | 66 | 128 | thấp | wiki không nêu |
| 18 | 1120 | 70 | 128 | thấp | wiki không nêu |
| 19 | 1184 | 74 | 128 | thấp | wiki không nêu |
| 20 | 1248 | 78 | 128 | thấp | wiki không nêu |
| 21 | 1312 | 82 | 128 | thấp | wiki không nêu |
| 22 | 1376 | 86 | 96 | giữa | wiki không nêu |
| 23 | 1440 | 90 | 32 | cao (rơi xuống Bục D) | Rosary [S1-3 note 5][IMG] |
| 24 | 1504 | 94 | 0 | đỉnh | wiki không nêu |

Nội dung thật của các nến "wiki không nêu": nhiều khả năng là tim/money bag ngẫu nhiên theo quy tắc rơi item của game, nhưng wiki không cung cấp bảng rơi (không tự điền).

---

### 6.2 STAGE 2

**Tổng quan [S1-3].** Stage ngắn nhưng KHÔNG thể hoàn thành nếu không đi qua lối ngầm (underground path). Kẻ địch mới: Fishman và Bat. Rơi xuống nước = mất mạng, nhịp nhảy rất quan trọng. Điểm xuất phát: wiki không nêu.

Ảnh bản đồ 264×176 → 528×352 px NES, gồm tầng trên (y < 176) và tầng dưới / lối ngầm (y ≥ 176) nối bằng cầu thang.

Hình học (native px, từ ảnh [IMG]):

| Đối tượng | Vị trí | Ghi chú |
|---|---|---|
| Sàn tầng trên (y = 160) | x 0-50, 64-258, 320-370, 384-530 | Khe x 50-64 là miệng cầu thang S2-A; x 258-320 là bức tường khối nhỏ (xem Pot Roast); x 370-384 là miệng cầu thang S2-C |
| Bục tầng trên | y = 64: x 0-130 và x 416-514 · y = 80: x 320-354 · y = 96: x 128-178 và x 352-402 · y = 0: x 480-514 | |
| Cầu thang S2-A (xuống sang phải, 8 bậc) | từ (48, 160) tới (112, 216) | đi từ tầng trên xuống lối ngầm (suy luận: "cầu thang xuống lối ngầm có nước" của [SE]) |
| Cầu thang S2-B (xuống sang phải, 4 bậc) | từ (176, 96) tới (208, 120) | |
| Cầu thang S2-C (xuống sang phải, 12 bậc) | từ (368, 160) tới (464, 248) | nối sàn tầng trên bên phải với phần phải của lối ngầm |
| Cầu thang S2-D (xuống sang phải, 8 bậc) | từ (400, 96) tới (464, 152) | |
| Bục nhỏ ở chân S2-A | y = 224, x 112-146 | |
| Sàn lối ngầm (y = 256) | x 16-242, 272-306 (2 gạch lơ lửng), 336-498 | Khe x 242-272 và 306-336 là hố nước |
| Bậc phải xa (hai khối "không với tới") | y = 288: x 496-530 · y = 320: x 464-498 | chấm #2 tại (477, 300) |
| Nước | từ y ≈ 320 tới đáy ảnh (352) | chết ngay khi rơi |
| Cửa/trụ cuối ở góc trên-phải | khoảng x 508, y 24-56 (trên bục y = 64) | (suy luận: lối ra) |

Chấm số:

| Chấm | Vị trí | Ghi chú wiki |
|---|---|---|
| #1 | (225, 135) | Bat bay theo sóng; chú ý độ cao roi (quất khi ngồi nếu cần). Phá khối ở đáy cầu thang trái → Pot Roast (+6 máu). Icon Pot Roast tại ≈ (259, 149) |
| #2 | (477, 300) | Fishman: quất ngay khi vừa lên khỏi nước. Cuối đoạn nước: phá khối bên phải, ngồi xuống chờ bonus money bag, cẩn thận Fishman |

Pot Roast [SE] (tóm tắt): ở phần đầu stage, trước khi đi xuống cầu thang vào lối ngầm có nước, bên phải có bức tường gồm khối nhỏ; phá các khối thấp nhất và sẵn sàng giết con Bat xuất hiện. Trên ảnh, tường khối nằm ở x ≈ 256-320, y ≈ 80-160 (nơi icon Pot Roast).

Bonus [SE] (tóm tắt): bên phải xa của lối ngầm (có Fishman) có 2 khối không với tới dưới cầu thang; phá khối trên-phải-nhất để thả xuống; ngồi trên các khối thấp nhất thì hiện money bag. Ảnh chụp [IMG] cho thấy túi tiền hiện ở bục gạch phía trái, cao hơn chỗ Simon ngồi (cách ≈ 230 px sang trái, ≈ 50 px lên).

Nến tường Stage 2 (16 cái):

| # | x_native | tile col | y_top | Khu | Nội dung |
|---|---|---|---|---|---|
| 1 | 32 | 2 | 128 | trên | wiki không nêu |
| 2 | 48 | 3 | 192 | dưới (lối ngầm) | wiki không nêu |
| 3 | 96 | 6 | 32 | trên | wiki không nêu |
| 4 | 112 | 7 | 192 | dưới | wiki không nêu |
| 5 | 160 | 10 | 16 | trên | wiki không nêu |
| 6 | 176 | 11 | 192 | dưới | wiki không nêu |
| 7 | 224 | 14 | 96 | trên | wiki không nêu |
| 8 | 240 | 15 | 192 | dưới | wiki không nêu |
| 9 | 304 | 19 | 16 | trên | wiki không nêu |
| 10 | 304 | 19 | 192 | dưới (rơi xuống bục lơ lửng x 272-306) | Stopwatch (icon [IMG]; văn bản wiki không nhắc) |
| 11 | 352 | 22 | 128 | trên | wiki không nêu |
| 12 | 368 | 23 | 192 | dưới | wiki không nêu |
| 13 | 416 | 26 | 0 | trên (rơi xuống bục y = 64) | Rosary (icon [IMG]; văn bản wiki không nhắc) |
| 14 | 432 | 27 | 192 | dưới | wiki không nêu |
| 15 | 480 | 30 | 32 | trên | wiki không nêu |
| 16 | 512 | 32 | 192 | dưới | wiki không nêu |

Kẻ địch: Fishman nhảy lên từ nước, nếu chạm đất thì đi bộ và bắn cầu lửa; Bat theo sóng; 1 con Bat xuất hiện khi phá tường Pot Roast [EN][S1-3][SE]. Vị trí/thời điểm spawn: wiki không nêu. Ảnh chụp [IMG]: TIME 0182, ♥-46, vũ khí phụ = Stopwatch, P-03, SCORE 010200.

---

### 6.3 STAGE 3

**Tổng quan [S1-3].** Stage cuối của Block 1, có Phantom Bat. Đi xuống cầu thang; Axe giấu trong nến bên phải trên đường xuống.

Ảnh bản đồ 384×88 → 768×176 px NES (3 màn hình). Hình học [IMG]:

| Đối tượng | Vị trí | Ghi chú |
|---|---|---|
| Sàn chính | y = 160, x 0-770 | |
| Bục T1 (bục xuất phát phía trên) | y = 64, x 0-114 (7 gạch) | (suy luận: nơi bắt đầu) |
| Cầu thang 3-A (xuống sang phải, 4 bậc) | từ (112, 64) tới (144, 88) | |
| Bục M | y = 96, x 128-306 (11 gạch) | |
| Cầu thang 3-B "cầu thang cuối" (xuống sang phải, 8 bậc) | từ (304, 96) tới (368, 152) | chứa nến #6 (Axe) |
| Bục phải thấp | y = 128, x 672-706 (2 gạch) | chân cầu thang nhỏ bên phải |
| Cầu thang nhỏ bên phải | khoảng x 690-740, y 128 → 96 | (chỉ thấy 1 bậc vì bị icon che) |
| Bục phải cao | y = 96, x 720-738 và 752-770 | |
| Khối chứa Shot Multiplier | ≈ (698, 110), chân cầu thang phải | icon Double Shot ("II") trên ảnh |
| Phantom Bat (vị trí nghỉ) | ≈ (622, 16) | (suy luận từ ảnh: khối tròn tối có 2 mắt đỏ ở trần) |
| Cửa/trụ cuối | mép phải, ≈ (763, 56-88) | (suy luận: lối ra sau boss) |

Chấm số:

| Chấm | Vị trí | Ghi chú wiki |
|---|---|---|
| #1 | (186, 41) | Invisibility Potion giấu giữa cầu thang (đi xuyên kẻ địch tới khi hết hiệu lực). Icon Potion KHÔNG vẽ trên ảnh. Gần nhất: nến #3 (160, 64) và #4 (224, 32); không xác định chắc nến nào |
| #2 | (385, 105) | Axe xuất hiện khi phá nến giữa cầu thang cuối → nến #6 (352, 96) [icon Axe tại ≈ (352, 84)] |
| #3 | (728, 135) | Khối ở chân cầu thang ngoài cùng bên phải chứa Shot Multiplier ("rất có thể Double Shot") [S1-3][SE][BO] |

Nến tường Stage 3 (12 cái; cùng lưới 64 px):

| # | x_native | tile col | y_top | Nội dung |
|---|---|---|---|---|
| 1 | 32 | 2 | 96 | wiki không nêu |
| 2 | 96 | 6 | 32 | wiki không nêu |
| 3 | 160 | 10 | 64 | wiki không nêu (ứng viên chứa Invisibility Potion, suy luận) |
| 4 | 224 | 14 | 32 | wiki không nêu (ứng viên chứa Invisibility Potion, suy luận) |
| 5 | 288 | 18 | 128 | wiki không nêu |
| 6 | 352 | 22 | 96 | Axe [S1-3][IMG] |
| 7 | 416 | 26 | 128 | wiki không nêu; nến gần chỗ Axe, ngay chân cầu thang, có thể chứa Holy Water ngẫu nhiên (nếu may) [S1-3] (suy luận: chính là nến này) |
| 8 | 480 | 30 | 128 | wiki không nêu |
| 9 | 544 | 34 | 96 | wiki không nêu |
| 10 | 608 | 38 | 128 | wiki không nêu |
| 11 | 672 | 42 | 96 | wiki không nêu |
| 12 | 736 | 46 | 96 | wiki không nêu |

Kẻ địch: Ghoul sinh ra từ phía trên rồi rơi xuống; không có loại khác ngoài boss [S1-3][EN]. Mẹo "infinite lives" của wiki [S1-3]: nếu nến chân cầu thang cạnh Axe chứa Holy Water, đứng góc dưới-trái và ném sao cho Ghoul sinh ra phía trên rơi xuống đúng ngọn lửa; vì giết nhiều kẻ địch bằng một vũ khí phụ cho điểm lớn (1000/2000/4000) nên nhanh chóng lên mạng; chú ý giới hạn thời gian; có thể làm tương tự bằng Axe nhưng chậm và nhờ may hơn. Ảnh chụp [IMG]: TIME 0131, ♥-70, vũ khí phụ = Axe, P-03, SCORE 012800.

#### Boss Phantom Bat (xem §4.3 B1 cho đặc tả hành vi)

- Khối Double Shot: phá khối phải nhất của bục ở chân cầu thang trong phòng boss [BO][SE].
- Axe có thể ném thẳng vào dơi; hoặc nhảy lên khối rồi dùng roi; roi + Stopwatch cũng được [S1-3][BO].
- Hạ boss: Magic Crystal (quả cầu phát sáng) hồi đầy máu và cho phép sang stage kế [S1-3].

---

### 6.4 Danh sách đối tượng tương tác Map 1 theo thứ tự xuất hiện

(Dùng để cross-check với dữ liệu map đã có. Tọa độ native xem 6.0. Nguồn nội dung trong ngoặc.)

| # | Stage / khu | Đối tượng | x_native | y | Nội dung / hiệu ứng | Điều kiện |
|---|---|---|---|---|---|---|
| 1 | 1A | Đuốc B1 | ≈95 | sàn | Large Heart [IMG] | phá bằng roi |
| 2 | 1A | Đuốc B2 | ≈223 | sàn | Morning Star [IMG][S1-3] | |
| 3 | 1A | Đuốc B3 | ≈351 | sàn | Morning Star [IMG][S1-3] | |
| 4 | 1A | Đuốc B4 | ≈479 | sàn | Large Heart [IMG] | |
| 5 | 1A | Đuốc B5 | ≈607 | sàn | Dagger [IMG] | |
| 6 | 1A | Bí mật cửa | túi tiền hiện ở ≈632 | sàn | Money bag nhấp nháy 1000 [SE] | nhảy vượt cửa (≈ x 684-710), nhảy lùi nhặt |
| 7 | 1B | Nến #2 | 96 | y_top 96 | Stopwatch [IMG] | |
| 8 | 1B | Nến #14 | 864 | y_top 32 | Holy Water [S1-3][IMG] | lên Bục B qua cầu thang 1-2 |
| 9 | 1B | Khối cuối Bục C | ≈1018-1034 | y ≈ 96 | Purple money bag 400 (suy luận khớp [SE]) / bonus treasure bag [S1-3] | phá khối rồi thả xuống ledge thấp nhất |
| 10 | 1B | Nến #23 | 1440 | y_top 32 | Rosary [S1-3][IMG] | lên cầu thang 4 tới Bục D |
| 11 | 1B | Cửa ra | ≈1530 | | kết thúc Stage 1 | đi hết cầu thang cuối [S1-3] |
| 12 | 2 | Tường khối (Pot Roast) | ≈256-320 | y 80-160 | Pot Roast (+6 máu) + 1 Bat | phá khối thấp nhất [SE] |
| 13 | 2 | Nến #10 | 304 | y_top 192 | Stopwatch [IMG] | tầng dưới |
| 14 | 2 | Nến #13 | 416 | y_top 0 | Rosary [IMG] | tầng trên-phải |
| 15 | 2 | Hai khối phải xa | 464-530 | y 288-320 | Money bag 1000 [SE] | phá khối trên-phải-nhất, thả xuống, ngồi trên khối thấp nhất |
| 16 | 3 | Invisibility Potion | quanh ≈186 | y ≈ 41 | tàng hình ≈5 giây [S1-3][IT] | giấu giữa cầu thang |
| 17 | 3 | Nến #6 | 352 | y_top 96 | Axe [S1-3][IMG] | nến giữa cầu thang cuối |
| 18 | 3 | Nến #7 (suy luận) | 416 | y_top 128 | Holy Water ngẫu nhiên [S1-3] | nếu may |
| 19 | 3 | Khối chân cầu thang phải | ≈698 | y ≈ 110 | Shot Multiplier (Double Shot) [S1-3][BO][SE] | phá khối |
| 20 | 3 | Boss Phantom Bat | ≈622 (vị trí nghỉ) | y ≈ 16 | 3000 điểm; Magic Crystal | xem §4 |

Phần còn lại (46 trong 57 vật chứa đo được): wiki không nêu nội dung.

---

## §7 Mâu thuẫn & thiếu

### 7.1 Chỗ các trang nói khác nhau / không nhất quán

| # | Vấn đề | Chi tiết | Nguồn |
|---|---|---|---|
| 1 | Điểm của Dracula | [GP]: bảng điểm ghi boss 3000-7000 và nói mọi boss TRỪ Dracula đều có điểm. [BO]: Dracula "Points 50,000". Không thể suy ra cái nào đúng (khuyến nghị: xem 50,000 là con số của trang Bosses, đánh dấu cần kiểm với game gốc) | [GP] vs [BO] |
| 2 | Số đòn hạ Dracula | [BO]: 24/16 (không ghi dạng nào). [S16-18]: dạng demon cần 32 lần đánh vào đầu. 32 > 24 không khớp tỉ lệ 3:2 của mọi boss khác; wiki không nói 32 tính theo vũ khí nào | [BO] vs [S16-18] |
| 3 | Điều kiện nhận 700 điểm khi nhặt Double/Triple Shot | [GP]: chỉ khi đang cầm Stopwatch HOẶC không có vũ khí phụ. [IT]: nhận 700 khi nhặt multiplier lúc đang cầm Stopwatch (không nhắc "không có vũ khí phụ") | [GP] vs [IT] |
| 4 | Fishman và "stage 4" | [EN]: nguy hiểm khi đứng trên bệ nổi ở "stage 4", nhưng Fishman chỉ có ở Stage 2 và 10, bệ nổi trên nước là Stage 10 [S10-12]. (suy luận) "stage 4" ở đây là Block 4 (Stage 10-12) | [EN] vs [S10-12] |
| 5 | Quy tắc nâng cấp roi | [IT]: Morning Star có trong nến vốn chứa small heart nếu bạn đã thu đủ tim (4 / 8). [S1-3]/[IMG]: 2 đuốc Stage 1 cố định chứa Morning Star. Không mâu thuẫn trực tiếp (tại đuốc 2 và 3 Simon có ≥ 10 tim) nhưng cơ chế điều kiện theo tim không được giải thích | [IT] vs [S1-3] |
| 6 | Hits Strong không nhất quán với "chain 1.5×" | Nếu strong = ceil(HP / 1.5): HP 1→1, HP 3→2, HP 8→6 khớp (Bone Tower 6) nhưng Bone Dragon (HP 8) ghi 7 và Axe Knight (HP 10) ghi 9 (công thức cho 7). Cột "HP/2" của bảng Comparison [EN] không phải một nửa HP (3→2, 8→6/7, 10→9); wiki không giải thích cột này | [EN] vs [IT] |
| 7 | "Hits per Life" | [GP]: Stage 7-12 "6 đòn / mạng" trong khi 16 / 3 = 5.33; hiểu là đòn thứ 6 làm chết. Không mâu thuẫn nhưng cần quy ước "chết khi máu ≤ 0" | [GP] |
| 8 | Stage 1 note 1 về vị trí money bag | [S1-3]: money bag hiện ra "back on the right side". [SE]: nhảy lùi lại, nhặt rồi mới vào cửa. Ảnh chụp [IMG] cho thấy túi tiền bên TRÁI cửa | [S1-3] vs [SE] vs [IMG] |
| 9 | Stage 1 note 4 vs Secrets | [S1-3]: bonus treasure bag khi phá khối cuối cầu thang rồi thả xuống ledge thấp nhất. [SE] Bonuses chỉ liệt kê 1 bonus Stage 1 (money bag ở cửa); [SE] Miscellaneous có Purple money bag 400 ở khối phải nhất của bục Panther thứ 3. (suy luận) cùng một bí mật | [S1-3] vs [SE] |
| 10 | Shot Multiplier Stage 14 | [S13-15]: quất tường trái của tường trên. [SE]: nhảy lên gờ bên phải rồi phá tường phải | [S13-15] vs [SE] |
| 11 | Đặt tên: Pot Roast / Pork Chop; Cross | [IT] gọi item hồi máu là Pork Chop (tên file ảnh Pot Roast), các trang khác gọi Pot Roast. Cross vừa là tên vũ khí phụ (Boomerang) vừa là tên Rosary; câu của [GP] về không có điểm khi giết bằng cross mơ hồ (suy luận: Rosary) | [IT][GP] |
| 12 | Số Magic Crystal | [WT]/[IT]: 5 crystal; [S16-18]: nhặt crystal ball cuối sau Dracula (tổng 6 nếu tính cả) | [WT] vs [S16-18] |
| 13 | Kẻ địch có trong [EN] nhưng văn bản stage không nhắc | Spear Knight ở Stage 5; Bone Tower và Medusa Head ở Stage 9; Fleaman ở Stage 14; Ghost và Red Skeleton ở Stage 15 (văn bản stage chỉ nêu các loại chính). Coi [EN] là danh sách chuẩn | [EN] vs [S4-6][S7-9][S13-15] |
| 14 | Mummy Men: số đòn và điểm | Bảng ghi Mummies 48/32 và 3000 điểm; không nói là tổng hay mỗi con. (suy luận) 48 = 2 × 24 là tổng | [BO] |
| 15 | Vị trí Axe ở Stage 3 | [BO]: nến cách phòng boss vài màn hình về phía sau. [S1-3] phần boss: Axe nằm phía trên bậc thang bên trái. Ảnh bản đồ cho thấy nến Axe ở cầu thang cuối, có thể trùng khớp | [BO] vs [S1-3] |

### 7.2 Chỗ thiếu số liệu: "wiki không nêu"

**Luật chơi và Simon**
- Thời gian đếm ngược khởi đầu của từng stage; tốc độ đếm (1 giây NES = bao nhiêu đơn vị).
- Số mạng khởi đầu (chỉ có ảnh HUD P-03); số lần Continue; màn Game Over; mốc 1UP có thể có giới hạn.
- Giới hạn tối đa của tim (chỉ biết ≥ 70 qua ảnh).
- Tốc độ đi, vận tốc/độ cao nhảy, có đổi hướng giữa không trung không, có nhảy khỏi cầu thang không, tốc độ lên/xuống thang, gia tốc rơi, hitbox, kích thước sprite.
- Bị đánh: khoảng cách/góc knockback, thời gian bất tử tạm, có bị knockback khi đang trên cầu thang không (chỉ biết không bị đánh văng khỏi bậc).
- Mất vũ khí phụ khi chết (chỉ suy luận), tim còn lại sau khi chết.
- Thưởng điểm cuối stage không có boss.

**Vũ khí**
- Sát thương số của từng vũ khí phụ và từng cấp roi (chỉ biết weak/strong, chain 1.5×, Dagger ≈ roi da, Cross ≈ Axe); độ dài px của 3 cấp roi; hitbox roi (cao/thấp).
- Quỹ đạo Axe/Boomerang (tốc độ, tầm xa, độ cao), thời gian cháy của Holy Water, thời lượng choáng.
- Thời lượng đóng băng của Stopwatch chỉ "5 giây"; Invisibility chỉ "khoảng 5 giây".

**Item**
- Bảng rơi (drop table) của kẻ địch và nến "ngẫu nhiên"; tần suất; thời gian tồn tại và nhấp nháy trước khi biến mất; giá trị điểm của Morning Star/Dagger… (không nêu).
- Nội dung của hầu hết nến tường (chỉ Stage 1-3 có một phần qua ảnh [IMG]).

**Kẻ địch**
- Tốc độ, tầm phát hiện, chu kỳ tấn công, giới hạn số lượng đồng thời, vị trí và thời điểm spawn, hành vi respawn khi cuộn lại, tên JP/US (chỉ có tên Anh).
- HP theo "vạch" (chỉ biết số đòn weak/strong).

**Boss**
- HP theo vạch (chỉ có số đòn); chu kỳ/tần suất tấn công; vị trí dịch chuyển của Dracula; thời gian giữa các lần tạo lưỡi hái của Death; tốc độ lưỡi hái; quỹ đạo Medusa.
- Số đòn của từng dạng Dracula (mâu thuẫn mục 2).

**Map**
- Danh sách đầy đủ nội dung mọi nến/đuốc/khối từng stage; vị trí kẻ địch; điểm xuất phát; độ dài stage; cách camera/khóa màn hình ở phòng boss; vị trí cửa chuyển cảnh.
- Ảnh bản đồ Stage 4-18 chưa phân tích trong đợt này.
- Checkpoint giữa stage: wiki không nêu (quy tắc duy nhất: Continue về đầu stage).

### 7.3 Danh sách các suy luận đã dùng trong file (để kiểm toán)

1. "Hits per Life" = đòn thứ N làm chết (§1.3).
2. Mạng khởi đầu = 3 (ảnh HUD P-03) và tim khởi đầu = 5 (ảnh HUD ♥-15 sau 2 Large Heart) (§1.4, §1.5, §6.1).
3. Mốc 1UP sau 80,000 là 130,000, 180,000, … (§1.4).
4. Mất vũ khí phụ khi chết (§1.4).
5. "Cross" trong [GP] bảng điểm = Rosary (§1.9, §7.1 mục 11).
6. Tỉ lệ boss Weak:Strong = 3:2 tương ứng "chain 1.5×" (§4).
7. Thanh máu boss = 16 vạch (đếm điểm ảnh trên HUD ảnh chụp) (§4).
8. Chuỗi Chain Whip (ngắn) → (dài) theo thứ tự nâng (§1.6).
9. Ảnh bản đồ là nửa tỉ lệ NES; mọi tọa độ native = ảnh × 2; y tính từ mép trên khung chơi 176 px; sàn y = 160 (§6.0).
10. Nội dung nến theo icon: Stopwatch ở nến #2 (Stage 1B), Stopwatch ở nến #10 và Rosary ở nến #13 (Stage 2); gán icon ↔ nến theo khoảng cách x ≤ 2 px (§6).
11. Khối cuối Bục C (Stage 1B) = khối Purple money bag 400 (§6.1, §7.1 mục 9).
12. Phantom Bat treo ở trần tại ≈ (622, 16) trong Stage 3 (§4.3 B1, §6.3).
13. Quy ước Second Quest: Stage k (≥ 2) = k + 19 (§1.13).
14. Điểm bắt đầu: Stage 3 bắt đầu ở bục T1 phía trên-trái; Stage 2 lối ra ở góc trên-phải (§6).
15. Mỗi thông tin "sát thương gây cho Simon" trong bảng §3 được suy ra từ bảng stage ở §1.3 (không có số liệu riêng cho từng loại).
