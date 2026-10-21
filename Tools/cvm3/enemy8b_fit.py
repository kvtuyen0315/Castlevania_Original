"""CVM3 08 — dò Raven (và enemy dùng chung Ghost / Medusa Head / Bone Tower) mỗi frame video Map_3 bằng khớp mẫu sprite trên bộ
đệm lưới NES (_cache/02/work/map3_nes.npy). Bọc nguyên Tools/cvm3/enemy7_fit.py (= thước đo Tools/cvm2/enemy8_fit.py), chỉ đổi thư mục
ra sang _cache/08 để không đè kết quả routine 07.

Chạy (từ gốc repo), tham số y hệt enemy7_fit.py:
  python -P -E Tools/cvm3/enemy8b_fit.py <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax] [pal=auto|file.json] [video=map3] [step=1]
  vd Raven cả video: python -P -E Tools/cvm3/enemy8b_fit.py rv_all Enemy/Raven/raven_0,Enemy/Raven/raven_1,Enemy/Raven/raven_2,Enemy/Raven/raven_3 0 7547 0 48 256 224 0.07 Tools/cvm3/enemy8b_pal_raven_map3.json
  → ClaudeNotes/cvm3/_cache/08/det_<tag>.csv + pal_<tag>.json (cột như enemy7_fit).
Bảng màu Raven Map_3 (enemy8b_pal_raven_map3.json) = ước lượng tự động trên WEB f1380–1740 (40 phát hiện, err tốt nhất 0.027) —
chỉ để dò vị trí (H1).
"""
import os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy7_fit as E7  # noqa: E402

E7.OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')

if __name__ == '__main__':
    E7.main()
