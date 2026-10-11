"""sfx_describe.py - mô tả âm học 37 SFX (thời lượng, md5, tần số chính, độ phẳng phổ -> tonal/noise, số 'nốt' đổi cao độ).

Chạy (từ gốc repo):  python -P -E Tools/cvgp/sfx_describe.py   -> in bảng markdown một dòng/SFX
"""
import glob
import hashlib
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import sfx_match as M  # noqa: E402

for path in sorted(glob.glob(os.path.join(M.SFX_DIR, "[0-9][0-9].wav"))):
    num = os.path.basename(path)[:2]
    x, sr = M.read_wav(path)
    x = x - x.mean()
    md5 = hashlib.md5(open(path, "rb").read()).hexdigest()
    n = 2048
    fr = [x[i:i + n] * np.hanning(n) for i in range(0, max(1, len(x) - n), n // 2)]
    mags = np.array([np.abs(np.fft.rfft(f)) for f in fr])
    f = np.fft.rfftfreq(n, 1.0 / sr)
    mags[:, f < 100] = 0
    peak = f[mags.argmax(axis=1)]
    flat = np.exp(np.mean(np.log(mags + 1e-9), axis=1)) / (np.mean(mags, axis=1) + 1e-9)
    pk = np.round(peak[mags.max(axis=1) > 0.2 * mags.max()] / 10) * 10
    steps = int((np.abs(np.diff(pk)) > 40).sum()) if len(pk) > 1 else 0
    kind = "noise" if np.median(flat) > 0.25 else "tonal"
    print("| %s | %.2f s | %s | %s | median peak %d Hz, flat %.2f, %d steps |" % (
        num, len(x) / sr, md5, kind, int(np.median(peak)), float(np.median(flat)), steps))
