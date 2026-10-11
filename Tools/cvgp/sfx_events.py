"""sfx_events.py - làm giàu theo SỰ KIỆN: SFX nào khớp âm video đúng lúc một loại sự kiện (nhặt tim, roi, ném dao, ...).

Chạy (từ gốc repo):  python -P -E Tools/cvgp/sfx_events.py [--lo 0.2 --hi 0.3] [--offset 0.0]
Đầu vào : ClaudeNotes/cvgp/_cache/02/map1.wav (sfx_match.py) + sự kiện từ ClaudeNotes/cvgp/_cache/video/work/
          (map1_events.txt: HUD đổi theo frame; whip_attacks.txt: cú roi; do planner đo từ video)
Đầu ra  : _cache/02/events_enrich.txt - với mỗi (SFX, loại sự kiện): số sự kiện, điểm phổ cực đại trong cửa sổ
          [t-lo, t+hi], phân vị so với cực đại cửa sổ cùng độ rộng trên toàn video (trung bình + số sự kiện > p95).
Tất định, chỉ numpy.
"""
import argparse
import glob
import os
import re
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sfx_match as M  # noqa: E402

WORK = os.path.join(M.ROOT, "ClaudeNotes", "cvgp", "_cache", "video", "work")


def parse_events():
    ev = {}

    def add(k, t):
        ev.setdefault(k, []).append(t)

    tally_from = 3 * 60 + 39.6  # từ đây là bảng cộng điểm (đếm TIME/tim) — tách riêng
    for line in open(os.path.join(WORK, "map1_events.txt"), encoding="utf-8"):
        m = re.match(r"f\s*(\d+)\s+(\d+):([\d.]+)\s+TIME\s+\d+\s+\|\s+(.*)", line.strip())
        if not m:
            continue
        t = int(m.group(2)) * 60 + float(m.group(3))
        txt = m.group(4)
        if t >= tally_from:
            if "hearts:" in txt:
                add("TALLY_HEART", t)
            continue
        if t >= 3 * 60 + 24.9 and "ebar" in txt:
            add("BOSS_HIT", t)
        h = re.search(r"hearts:(\d+)->(\d+)", txt)
        if h:
            d = int(h.group(2)) - int(h.group(1))
            if d == 1:
                add("HEART_SMALL", t)
            elif d == 5:
                add("HEART_BIG", t)
            elif d == -1:
                add("THROW", t)
        s = re.search(r"score:(\d+)->(\d+)", txt)
        if s:
            add("SCORE_%d" % (int(s.group(2)) - int(s.group(1))), t)
        if txt.startswith("sub:none->") or "sub:dagger->axe" in txt:
            add("SUB_PICKUP", t)
        if txt.startswith("stage:"):
            add("STAGE_CHANGE", t)
        if "mult:" in txt and "->II" in txt and "II->" not in txt:
            add("DOUBLE_ICON", t)
    prev = -9
    for line in open(os.path.join(WORK, "whip_attacks.txt"), encoding="utf-8"):
        m = re.match(r"\s+f(\d+)-(\d+)\s+(\d+):([\d.]+)", line)
        if m:
            t = int(m.group(3)) * 60 + float(m.group(4))
            if t - prev > 0.6:  # gộp khung 1-2-3 của cùng một cú roi
                add("WHIP", t)
            prev = t
    return ev


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--lo", type=float, default=0.2)
    ap.add_argument("--hi", type=float, default=0.3)
    ap.add_argument("--offset", type=float, default=0.0)
    ap.add_argument("--out", default=os.path.join(M.ROOT, "ClaudeNotes", "cvgp", "_cache", "02"))
    a = ap.parse_args()
    ev = parse_events()
    x, sr = M.read_wav(os.path.join(a.out, "map1.wav"))
    x = M.resample(x, sr, M.SR)
    xb = M.bandpass(x, 500.0, 8000.0)
    S = M.stft_mag(xb, 500.0, 8000.0)
    step = M.HOP / M.SR
    wlen = int((a.lo + a.hi) / step)
    rows = []
    for path in sorted(glob.glob(os.path.join(M.SFX_DIR, "[0-9][0-9].wav"))):
        num = os.path.basename(path)[:2]
        s, sr2 = M.read_wav(path)
        s = M.resample(s, sr2, M.SR)
        T = M.stft_mag(M.bandpass(s - s.mean(), 500.0, 8000.0), 500.0, 8000.0)
        sc = M.ncc_spec(S, T)
        # cực đại cuộn độ rộng wlen (mọi vị trí) làm phân phối nền
        pad = np.concatenate([sc, np.full(wlen, sc.min())])
        win = np.lib.stride_tricks.sliding_window_view(pad, wlen).max(axis=1)[: len(sc)]
        srt = np.sort(win)
        for k, ts in sorted(ev.items()):
            vals = []
            for t in ts:
                i0 = int((t + a.offset - a.lo) / step)
                i0 = max(0, min(len(sc) - 1, i0))
                vals.append(win[i0])
            vals = np.array(vals)
            pct = np.searchsorted(srt, vals) / float(len(srt))
            rows.append((num, k, len(ts), float(pct.mean()), int((pct > 0.95).sum()), float(vals.mean())))
    outp = os.path.join(a.out, "events_enrich.txt")
    with open(outp, "w") as f:
        f.write("sfx event n mean_pct n>p95 mean_score\n")
        for r in rows:
            f.write("%s %-14s %3d %.2f %3d %.2f\n" % r)
    # tóm tắt: mỗi loại sự kiện, 4 SFX tốt nhất
    kinds = sorted(ev.keys())
    for k in kinds:
        best = sorted([r for r in rows if r[1] == k], key=lambda r: -r[3])[:5]
        print("%-13s n=%-3d " % (k, len(ev[k])) + "  ".join("%s(%.2f,%d/%d,s%.2f)" % (r[0], r[3], r[4], r[2], r[5]) for r in best))


if __name__ == "__main__":
    main()
