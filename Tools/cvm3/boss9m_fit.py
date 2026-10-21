"""CVM3 09 — dò Mummy Men / băng gạc / quả cầu / dao găm / lửa chết mỗi frame video Map_3 hoặc Boss_Map_3 bằng khớp mẫu sprite trên
bộ đệm lưới NES (CVM3 02: _cache/02/work/<video>_nes.npy). Bọc nguyên Tools/cvm3/enemy7_fit.py (= thước đo Tools/cvm2/enemy8_fit.py:
sai khác màu cắt CLIP, gồm pixel ĐEN đục, bảng màu VIDEO tự ước lượng hoặc file — H1: video boss khác bảng màu ⇒ chỉ dò vị trí/khung,
KHÔNG lấy màu làm sprite), chỉ đổi thư mục ra sang _cache/09.

Chạy (từ gốc repo), tham số y hệt enemy7_fit.py:
  python -P -E Tools/cvm3/boss9m_fit.py <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax] [pal=auto|file.json] [video=map3|boss3] [step=1]
  vd Mummy Map_3: python -P -E Tools/cvm3/boss9m_fit.py mm_m3 Boss/Mummy/mummy_purple_0,Boss/Mummy/mummy_purple_1,Boss/Mummy/mummy_purple_2 5560 6800 0 140 256 216 0.16 auto map3
  → ClaudeNotes/cvm3/_cache/09/det_<tag>.csv + pal_<tag>.json (cột như enemy7_fit: frame, dup, sprite, flip, x, y, pivx, pivy, cx, section,
    worldx = cx + pivx, ysec = pivy − 32, err).
Trình tự dựng lại toàn bộ số đo routine 09: xem docstring Tools/cvm3/boss9m_track.py.
"""
import os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy7_fit as E7  # noqa: E402

E7.OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '09')

if __name__ == '__main__':
    E7.main()
