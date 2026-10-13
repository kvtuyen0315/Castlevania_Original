"""Sinh sprite HUD TẠM (thanh máu, tim, khung vũ khí phụ) — vẽ lại theo số đo trên video gốc vì chưa có sheet HUD.

KHÔNG phải pixel cắt từ sheet (CVTI_MEMORY §4.5, §5 #17): người dùng đồng ý dựng tạm ngày 11/10/2026, đặt trong
Assets/Data/Art/Intro/. Khi có sheet HUD bản US: cắt bằng Tools/cvti/slices.json rồi XOÁ các file *_temp.png này.
Số đo (toạ độ NES, video đổi bằng x=(xv-11)/1.969, y=(yv+7.4)/1.712, trung bình 12 khung 8.7–14.2 s):
  - vạch máu: khối đặc 3×6, bước 4 px, 16 vạch từ x=56; PLAYER y=25 màu (216,40,0), ENEMY y=33 màu (252,116,96)
  - khung vũ khí phụ: 32×22 tại (128,24), viền 2 px màu (216,40,0), trong rỗng
  - tim: 8×8 tại (168,24), đỏ (216,40,0) + vệt sáng trắng (252,252,252) ở cột 1, dòng 1–3
  - (CVGP 12) vạch đã mất: khung rỗng trắng (252,252,252) 3×6 cùng chỗ vạch (video Map_1 3:30 ENEMY 0 vạch, video boss 12.2 s PLAYER)
Màu chọn theo palette NES mà các sheet rip đang dùng (0x16 = (216,40,0), 0x26 = (252,116,96), 0x20 = (252,252,252)).
Chạy từ gốc repo:  python -P -E Tools/cvti/make_hud_temp.py   (cần Pillow ở user site — KHÔNG dùng -I)
"""
import hashlib
import io
import os

from PIL import Image

OUT = "Assets/Data/Art/Intro"
RED = (216, 40, 0, 255)
PINK = (252, 116, 96, 255)
WHITE = (252, 252, 252, 255)
CLEAR = (0, 0, 0, 0)

HEART = [
    ".##..##.",
    "#W######",
    "#W######",
    "#W######",
    "########",
    ".######.",
    "..####..",
    "...##...",
]


def solid(w, h, color):
    return Image.new("RGBA", (w, h), color)


def frame(w, h, border, color):
    img = Image.new("RGBA", (w, h), CLEAR)
    px = img.load()
    for y in range(h):
        for x in range(w):
            if x < border or y < border or x >= w - border or y >= h - border:
                px[x, y] = color
    return img


def from_rows(rows, colors):
    img = Image.new("RGBA", (len(rows[0]), len(rows)), CLEAR)
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch in colors:
                px[x, y] = colors[ch]
    return img


def save(img, name):
    buf = io.BytesIO()
    img.save(buf, format="PNG", optimize=False, compress_level=9)
    data = buf.getvalue()
    path = f"{OUT}/{name}.png"
    os.makedirs(OUT, exist_ok=True)
    with open(path, "wb") as f:
        f.write(data)
    print(f"{hashlib.md5(data).hexdigest()}  {path}  {img.width}x{img.height}")


def main():
    save(solid(3, 6, RED), "hud_tick_player_temp")
    save(solid(3, 6, PINK), "hud_tick_enemy_temp")
    save(frame(32, 22, 2, RED), "hud_subweapon_frame_temp")
    save(from_rows(HEART, {"#": RED, "W": WHITE}), "hud_heart_temp")
    save(frame(3, 6, 1, WHITE), "hud_tick_empty_temp")


if __name__ == "__main__":
    main()
