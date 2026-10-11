"""Kiểm ảnh chụp Game view có đúng là PNG gốc phóng nguyên lần (Point) hay không.

Mỗi pixel NES (nx, ny) phải ra khối S×S giống hệt tại (X + nx·S, Y + ny·S); ngoài vùng game phải đen ĐẶC (alpha 255 —
vùng không camera nào xoá ra alpha 0, trình xem ảnh hiện thành trắng).
Cần ffmpeg trong PATH. Dùng:
  python -I check_pixel_exact.py <shot.png> <src.png> <S> <X> <Y> <shotW> <shotH> [<srcW> <srcH>]
vd 1920×1080 scale 4×: ... shot.png PixelGrid.png 4 448 60 1920 1080
"""
import subprocess
import sys


def raw(path, w, h):
    d = subprocess.run(["ffmpeg", "-v", "error", "-i", path, "-f", "rawvideo", "-pix_fmt", "rgba", "-"],
                       capture_output=True).stdout
    if len(d) != w * h * 4:
        sys.exit(f"{path}: kích thước không phải {w}x{h}")
    return d


def main():
    shot, src = sys.argv[1], sys.argv[2]
    S, X, Y, W, H = map(int, sys.argv[3:8])
    sw, sh = (int(sys.argv[8]), int(sys.argv[9])) if len(sys.argv) > 9 else (256, 240)
    a, b = raw(shot, W, H), raw(src, sw, sh)

    def px(d, w, x, y):
        i = (y * w + x) * 4
        return d[i:i + 4]

    opaque = bytes([255])
    inside = sum(px(a, W, X + nx * S + dx, Y + ny * S + dy) != px(b, sw, nx, ny)[:3] + opaque
                 for ny in range(sh) for nx in range(sw) for dy in range(S) for dx in range(S))
    outside = sum(px(a, W, x, y) != bytes([0, 0, 0, 255])
                  for y in range(H) for x in range(W)
                  if not (X <= x < X + sw * S and Y <= y < Y + sh * S))
    print(f"mismatch inside: {inside}/{sw * sh * S * S}  non-black outside: {outside}")
    sys.exit(1 if inside or outside else 0)


if __name__ == "__main__":
    main()
