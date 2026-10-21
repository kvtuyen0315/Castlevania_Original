"""CVM3 08 — so quỹ đạo Raven / Ghost / Medusa Head / cầu lửa Bone Tower: video Map_3 (vết _cache/08/trk_<tag>.csv của enemy8b_track.py)
↔ Unity (Map3EnemyProbe ⇒ <dir>/<đoạn>.csv). Dùng lại khung so của Tools/cvm3/enemy7_compare.py (ghép vết ↔ thực thể cùng loại có rms nhỏ
nhất, thử lệch −1/0/+1 tick, báo thêm căn mốc ±20) — chỉ khác danh sách đoạn, thư mục vết và WalkwayB: x video (gốc sheet) + 112 = x Unity.
Toạ độ so = pivot: Raven / Ghost / Medusa Head / cầu lửa đều pivot TÂM sprite, Unity Y = tâm (viewOffset 0) ⇒ video (worldx, pivy − 32).
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_compare.py <unity_dir> [out=ClaudeNotes/cvm3/ref/data/enemy8b/compare.txt]
"""
import csv, os, sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'Tools', 'cvm3'))
import enemy7_compare as C7  # noqa: E402

C7.D = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', '_cache', '08')
C7.VOY = {}
XOFF = {'WalkwayB': 112}
# đoạn: f0, f1, section, [(tag, loại Unity, errmax, lọc)] — lọc theo section để không lẫn vết khác cửa sổ
SEGS = {
    'rv_web0': (1338, 1492, 'WalkwayEntranceB', [('rv_all', 'Raven', 0.07, None)]),
    'rv_web1': (1572, 1732, 'WalkwayEntranceB', [('rv_all', 'Raven', 0.07, None)]),
    'rv_wb0': (3592, 3644, 'WalkwayB', [('rv_all', 'Raven', 0.07, None)]),
    'rv_wb1': (3863, 3957, 'WalkwayB', [('rv_all', 'Raven', 0.07, None)]),
    'rv_mm': (5084, 5122, 'MummyMan', [('rv_all', 'Raven', 0.07, None)]),
    'gh_wea': (414, 437, 'WalkwayEntranceA', [('gh_wea', 'Ghost', 0.10, None)]),   # đến trước đòn roi đầu (f438 khựng — Simon rối không quất)
    'mh_wa': (2788, 3060, 'WalkwayA', [('mh_all', 'MedusaHead', 0.10, None)]),
    'mh_mm': (5368, 5590, 'MummyMan', [('mh_all', 'MedusaHead', 0.10, None)]),
    'bt_mm': (5392, 5512, 'MummyMan', [('fb_all', 'BoneTowerFireball', 0.075, None)]),
}


def main():
    udir = sys.argv[1]
    out = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'enemy8b', 'compare.txt')
    orig = C7.video_tracks

    def vt_shift(tag, f0, f1, errmax, filt, sec=None):
        tr = orig(tag, f0, f1, errmax, filt)
        off = XOFF.get(sec, 0)
        return {k: [(t, x + off, y, fl, sp) for (t, x, y, fl, sp) in v] for k, v in tr.items()}

    C7.SEGS = {}
    for seg, (f0, f1, sec, specs) in SEGS.items():
        C7.SEGS[seg] = (f0, f1, [(tag, typ, em, (lambda r, s=sec: r['section'] == s)) for tag, typ, em, _ in specs])
    secs = {seg: v[2] for seg, v in SEGS.items()}
    cur = {'seg': None}

    def video_tracks(tag, f0, f1, errmax, filt):
        sec = None
        for seg, (a, b, s, specs) in SEGS.items():
            if a == f0 and b == f1:
                sec = s
        return vt_shift(tag, f0, f1, errmax, filt, sec)

    C7.video_tracks = video_tracks
    sys.argv = [sys.argv[0], udir, out]
    C7.main()


if __name__ == '__main__':
    main()
