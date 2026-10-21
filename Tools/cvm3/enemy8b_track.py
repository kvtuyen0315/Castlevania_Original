"""CVM3 08 — ghép phát hiện của enemy8b_fit.py thành VẾT (bọc nguyên Tools/cvm3/enemy7_track.py, thư mục _cache/08).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_track.py <tag> [errmax] [maxgap] [maxd] [minlen] [key=worldx|pivx] [dump=0]
  → ClaudeNotes/cvm3/_cache/08/trk_<tag>.csv + tóm tắt từng vết (như enemy7_track.py).
"""
import os, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy7_track as T7  # noqa: E402

T7.D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')

if __name__ == '__main__':
    T7.main()
