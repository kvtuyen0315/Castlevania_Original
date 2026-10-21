"""CVM3 10 — dò vật thể (nến, item rơi, túi tiền bí mật, enemy ở điểm sinh) mỗi frame video Map_3 bằng khớp mẫu sprite trên bộ đệm
lưới NES (_cache/02/work/map3_nes.npy). Bọc nguyên Tools/cvm3/enemy7_fit.py (= thước đo Tools/cvm2/enemy8_fit.py — H1: chỉ để dò VỊ TRÍ,
không lấy màu làm sprite), chỉ đổi thư mục ra sang _cache/10 để không đè kết quả routine 07/08/09.

Chạy (từ gốc repo), tham số y hệt enemy7_fit.py:
  python -P -E Tools/cvm3/fit10.py <tag> <sprites> <f0> <f1> [x0 y0 x1 y1] [errmax] [pal=auto|file.json] [video=map3] [step=1]
  vd nến cả video (bảng màu nến video CVM3 09 Tools/cvm3/candle10_pal_map3.json):
    python -P -E Tools/cvm3/fit10.py cd_all Object/candle_0,Object/candle_1 207 7451 0 48 256 224 0.09 Tools/cvm3/candle10_pal_map3.json
  → ClaudeNotes/cvm3/_cache/10/det_<tag>.csv + pal_<tag>.json (cột như enemy7_fit: worldx = gốc SHEET; WalkwayB Unity x = worldx + 112).
"""
import os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy7_fit as E7  # noqa: E402

E7.OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '10')

if __name__ == '__main__':
    E7.main()
