"""sfx_match.py - tìm 37 SFX (Art_Source/sounds-resource/393996/NN.wav) trong âm thanh của video tham chiếu.

Chạy (từ gốc repo):
    python -P -E Tools/cvgp/sfx_match.py [--top 8] [--band 500 8000] [--out ClaudeNotes/cvgp/_cache/02]
Đầu vào : <out>/map1.wav, <out>/boss.wav (ffmpeg -vn -ac 1 -ar 22050 từ 2 video; xem CVGP_MEMORY §7 routine 02)
Đầu ra  : <out>/matches.json  { "NN": { "map1": [{t, spec, wave}], "boss": [...] } }
          <out>/matches.txt   bảng đọc nhanh (đỉnh, z-score so với median/MAD của đường điểm)
Phương pháp:
  spec = NCC phổ: STFT biên độ (cửa sổ 512 mẫu ≈ 23 ms @22050, hop 128 ≈ 5.8 ms), dải 500-8000 Hz,
         nén log1p(|X|/C), mẫu SFX trừ trung bình, NCC phẳng qua FFT theo từng bin (không phụ thuộc pha).
  wave = NCC dạng sóng trên tín hiệu lọc thông dải (500-8000 Hz) qua FFT (nhạy pha, chỉ để tham khảo).
  Đỉnh: lấy cực đại cục bộ, cách nhau >= max(0.25 s, độ dài SFX); z = (điểm - median)/(1.4826*MAD).
Tất định: không có RNG. Chỉ numpy (không scipy).
"""
import argparse
import glob
import json
import os
import sys
import wave

import numpy as np

SR = 22050
WIN = 512
HOP = 128
LOGC = 0.003  # hằng nén log1p(|X|/C)

# đoạn không phải game (YouTube + quảng cáo, MAP1_TIMELINE §2 dòng 24): loại khỏi mọi đỉnh
NON_GAME = {"map1": [(107.7, 114.5)], "boss": []}

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SFX_DIR = os.path.join(ROOT, "Art_Source", "sounds-resource", "393996")


def read_wav(path):
    with wave.open(path, "rb") as w:
        n, ch, sw, sr = w.getnframes(), w.getnchannels(), w.getsampwidth(), w.getframerate()
        raw = w.readframes(n)
    if sw == 1:
        x = (np.frombuffer(raw, dtype=np.uint8).astype(np.float64) - 128.0) / 128.0
    elif sw == 2:
        x = np.frombuffer(raw, dtype="<i2").astype(np.float64) / 32768.0
    else:
        raise ValueError("sample width %d" % sw)
    if ch > 1:
        x = x.reshape(-1, ch).mean(axis=1)
    return x, sr


def resample(x, sr, target):
    if sr == target:
        return x.copy()
    n_out = int(round(len(x) * target / sr))
    t_out = np.arange(n_out) * (sr / target)
    if sr > target:  # lọc thấp thô bằng FFT trước khi giảm mẫu
        X = np.fft.rfft(x)
        f = np.fft.rfftfreq(len(x), 1.0 / sr)
        X[f > 0.45 * target] = 0
        x = np.fft.irfft(X, len(x))
    return np.interp(t_out, np.arange(len(x)), x)


def bandpass(x, lo, hi):
    X = np.fft.rfft(x)
    f = np.fft.rfftfreq(len(x), 1.0 / SR)
    X[(f < lo) | (f > hi)] = 0
    return np.fft.irfft(X, len(x))


def stft_mag(x, lo, hi):
    win = np.hanning(WIN)
    n = 1 + max(0, (len(x) - WIN) // HOP)
    idx = np.arange(WIN)[None, :] + HOP * np.arange(n)[:, None]
    fr = x[idx] * win[None, :]
    mag = np.abs(np.fft.rfft(fr, axis=1)) / (WIN / 4.0)
    f = np.fft.rfftfreq(WIN, 1.0 / SR)
    sel = (f >= lo) & (f <= hi)
    return np.log1p(mag[:, sel] / LOGC)  # (frames, bins)


def fft_xcorr_valid(sig, tmpl):
    """Tương quan chéo 1-D chế độ valid: r[k] = sum_j sig[k+j]*tmpl[j]."""
    n = len(sig) + len(tmpl)
    nf = 1 << (n - 1).bit_length()
    r = np.fft.irfft(np.fft.rfft(sig, nf) * np.conj(np.fft.rfft(tmpl, nf)), nf)
    return r[: len(sig) - len(tmpl) + 1]


def ncc_spec(S, T):
    """S: (frames_sig, bins), T: (frames_tmpl, bins). Trả điểm NCC phẳng theo từng bước hop."""
    nt = T.shape[0]
    Tz = T - T.mean()
    tn = np.sqrt((Tz ** 2).sum())
    if tn < 1e-9 or S.shape[0] < nt:
        return np.zeros(max(0, S.shape[0] - nt + 1))
    num = np.zeros(S.shape[0] - nt + 1)
    for b in range(S.shape[1]):
        num += fft_xcorr_valid(S[:, b], Tz[:, b])
    cs = np.concatenate([[0.0], np.cumsum(S.sum(axis=1))])
    cs2 = np.concatenate([[0.0], np.cumsum((S ** 2).sum(axis=1))])
    cnt = nt * S.shape[1]
    s1 = cs[nt:] - cs[:-nt]
    s2 = cs2[nt:] - cs2[:-nt]
    var = np.maximum(s2 - s1 * s1 / cnt, 1e-12)
    return num / (tn * np.sqrt(var))


def ncc_wave(x, t):
    tz = t - t.mean()
    tn = np.sqrt((tz ** 2).sum())
    num = fft_xcorr_valid(x, tz)
    cs = np.concatenate([[0.0], np.cumsum(x)])
    cs2 = np.concatenate([[0.0], np.cumsum(x * x)])
    n = len(t)
    s1 = cs[n:] - cs[:-n]
    s2 = cs2[n:] - cs2[:-n]
    var = np.maximum(s2 - s1 * s1 / n, 1e-12)
    return num / (tn * np.sqrt(var))


def peaks(score, step_s, min_sep_s, top):
    """Cực đại cục bộ cách nhau >= min_sep_s; trả [(index, score)] giảm dần."""
    order = np.argsort(-score)
    sep = max(1, int(min_sep_s / step_s))
    taken = []
    for i in order:
        if len(taken) >= top:
            break
        if all(abs(int(i) - j) >= sep for j, _ in taken):
            taken.append((int(i), float(score[i])))
    return taken


def mask_non_game(score, name, step_s):
    sc = score.copy()
    lo_v = float(np.min(sc))
    for a, b in NON_GAME.get(name, []):
        sc[max(0, int(a / step_s)): int(b / step_s) + 1] = lo_v
    return sc


def zscore(score, v):
    med = np.median(score)
    mad = np.median(np.abs(score - med)) * 1.4826 + 1e-9
    return (v - med) / mad


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--top", type=int, default=8)
    ap.add_argument("--band", type=float, nargs=2, default=[500.0, 8000.0])
    ap.add_argument("--out", default=os.path.join(ROOT, "ClaudeNotes", "cvgp", "_cache", "02"))
    a = ap.parse_args()
    lo, hi = a.band

    vids = {}
    for name in ("map1", "boss"):
        x, sr = read_wav(os.path.join(a.out, name + ".wav"))
        x = resample(x, sr, SR)
        vids[name] = (x, stft_mag(bandpass(x, lo, hi), lo, hi), bandpass(x, lo, hi))

    result = {}
    lines = []
    for path in sorted(glob.glob(os.path.join(SFX_DIR, "[0-9][0-9].wav"))):
        num = os.path.basename(path)[:2]
        s, sr = read_wav(path)
        s = resample(s, sr, SR)
        s = s - s.mean()
        dur = len(s) / SR
        T = stft_mag(bandpass(s, lo, hi), lo, hi)
        sb = bandpass(s, lo, hi)
        result[num] = {"dur": round(dur, 3)}
        for name, (x, S, xb) in vids.items():
            sc = mask_non_game(ncc_spec(S, T), name, HOP / SR)
            scw = ncc_wave(xb, sb) if len(xb) > len(sb) else np.zeros(1)
            for a_, b_ in NON_GAME.get(name, []):
                scw = scw.copy()
                scw[int(a_ * SR): int(b_ * SR)] = 0.0
            pk = peaks(sc, HOP / SR, max(0.25, dur), a.top)
            out = []
            for i, v in pk:
                w0 = int(i * HOP)  # mẫu bắt đầu SFX trong video
                wmax = float(np.max(np.abs(scw[max(0, w0 - 220): w0 + 220]))) if len(scw) > w0 else 0.0
                out.append({"t": round(i * HOP / SR, 3), "spec": round(v, 4),
                            "z": round(float(zscore(sc, v)), 1), "wave": round(wmax, 3)})
            result[num][name] = out
            # đỉnh theo NCC dạng sóng (khớp chính xác từng mẫu ⇒ điểm cao gần 1 khi SFX nằm sạch trong video)
            wp = peaks(np.abs(scw), 1.0 / SR, max(0.25, dur), a.top)
            result[num][name + "_wave"] = [{"t": round(i / SR, 3), "wave": round(v, 3)} for i, v in wp]
            lines.append("%s %-4s wave: " % (num, name) + "  ".join(
                "%.2fs(%.2f)" % (i / SR, v) for i, v in wp[:5]))
            lines.append("%s %-4s dur=%.2f  " % (num, name, dur) + "  ".join(
                "%.2fs(s=%.2f z=%.0f w=%.2f)" % (o["t"], o["spec"], o["z"], o["wave"]) for o in out[:5]))
    with open(os.path.join(a.out, "matches.json"), "w") as f:
        json.dump(result, f, indent=1, sort_keys=True)
    with open(os.path.join(a.out, "matches.txt"), "w") as f:
        f.write("\n".join(lines) + "\n")
    print("\n".join(lines))


if __name__ == "__main__":
    sys.exit(main())
