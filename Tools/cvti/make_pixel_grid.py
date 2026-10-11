"""Sinh ảnh lưới kiểm pixel-perfect 256x240 (chỉ dùng stdlib).

Mẫu: caro 8x8 (xám tối/xám vừa), viền đỏ 1 px sát mép, ô caro 1 px đen/trắng 64x64 ở giữa
(NES 96,88), 4 ô góc 8x8 (TL xanh lá, TR xanh dương, BL vàng, BR tím) nằm trong viền.
Dùng: python -I make_pixel_grid.py <out.png>
"""
import struct
import sys
import zlib

W, H = 256, 240


def pixel(x, y):
    if x in (0, W - 1) or y in (0, H - 1):
        return (255, 0, 0)
    if 1 <= x <= 8 and 1 <= y <= 8:
        return (0, 255, 0)
    if W - 9 <= x <= W - 2 and 1 <= y <= 8:
        return (0, 0, 255)
    if 1 <= x <= 8 and H - 9 <= y <= H - 2:
        return (255, 255, 0)
    if W - 9 <= x <= W - 2 and H - 9 <= y <= H - 2:
        return (255, 0, 255)
    if 96 <= x < 160 and 88 <= y < 152:
        return (255, 255, 255) if (x + y) % 2 == 0 else (0, 0, 0)
    return (40, 40, 40) if ((x // 8) + (y // 8)) % 2 == 0 else (90, 90, 90)


def write_png(path):
    raw = b"".join(b"\x00" + bytes(c for x in range(W) for c in pixel(x, y)) for y in range(H))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", W, H, 8, 2, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")
    with open(path, "wb") as f:
        f.write(png)


if __name__ == "__main__":
    write_png(sys.argv[1])
