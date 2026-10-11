# _inbox — 3 thứ còn thiếu cho cụm CVTI (người dùng tải tay, bỏ vào đây)

Trang resource bị Cloudflare chặn truy cập tự động nên cần người dùng tải.

1. **Sheet HUD + font** (chữ số 0–9, chữ cái, `-`, thanh máu PLAYER/ENEMY, khung vũ khí phụ, icon tim) — từ https://www.spriters-resource.com/nes/cv/ (mục Miscellaneous/HUD hoặc tương đương). Gợi ý tên: `hud_font.png`. Chặn routine 06.
2. **Sheet Stage 1** (sân trước lâu đài) — cùng trang. Gợi ý tên: `stage1.png`. Routine 07 dùng nền đen nếu chưa có.
3. **Nhạc Prologue + Vampire Killer** — pack 393996 chỉ có SFX; nguồn do người dùng chọn. Gợi ý tên: `prologue.(wav|ogg|mp3)`, `vampire_killer.(wav|ogg|mp3)`. Routine 07 không phát nhạc nếu chưa có.

Sau khi bỏ file vào, chạy lại nhập (chép nguyên byte + sha256 + thêm dòng vào `../SOURCES.md`).

**Cập nhật 11/10/2026 chiều:** `Text Font.gif` đã chuyển sang `../spriters-resource/text_font.gif` và dùng tạm cho chữ số HUD. Vẫn cần: (1) sheet HUD bản US (chữ số, thanh máu, tim, khung vũ khí phụ) để thay đồ TẠM; (2) sheet Stage 1; (3) nhạc Prologue + Vampire Killer.
