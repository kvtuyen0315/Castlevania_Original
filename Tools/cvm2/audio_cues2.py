# audio_cues2.py (CVM2 02) - đường bao RMS + khoảng lặng + điểm đổi nhạc (khoảng cách phổ giữa 2 cửa sổ liền kề) của audio Map_2 / Boss_Map_2
# Chạy: python -P -E Tools/cvm2/audio_cues2.py <wav> <out.txt>   (wav 16 kHz mono từ ffmpeg -vn -ac 1 -ar 16000)
import sys, wave
import numpy as np
w = wave.open(sys.argv[1]); sr = w.getframerate(); x = np.frombuffer(w.readframes(w.getnframes()), np.int16).astype(float) / 32768
hop = int(sr * 0.05)
n = len(x) // hop
rms = np.sqrt((x[:n * hop].reshape(n, hop) ** 2).mean(axis=1))
out = []
def ts(t): return '%d:%05.2f' % (t // 60, t % 60)
out.append('duration %.2f s, sr %d, hop 0.05 s; rms median %.4f p10 %.4f p90 %.4f max %.4f' % (len(x) / sr, sr, np.median(rms), np.percentile(rms, 10), np.percentile(rms, 90), rms.max()))
thr = 0.004
sil = rms < thr
out.append('--- khoang lang (rms < %.3f) >= 0.25 s' % thr)
i = 0
while i < n:
    if sil[i]:
        j = i
        while j < n and sil[j]: j += 1
        if (j - i) * 0.05 >= 0.25: out.append('lang %s - %s (%.2f s)' % (ts(i * 0.05), ts(j * 0.05), (j - i) * 0.05))
        i = j
    else: i += 1
# phổ log theo cửa sổ 1 s, hop 0.5 s, 24 băng
win = sr; hp = sr // 2
bands = np.geomspace(100, 7500, 25)
specs = []; ts_ = []
for s in range(0, len(x) - win, hp):
    seg = x[s:s + win] * np.hanning(win)
    sp = np.abs(np.fft.rfft(seg)); fr = np.fft.rfftfreq(win, 1 / sr)
    v = np.array([sp[(fr >= bands[k]) & (fr < bands[k + 1])].sum() for k in range(24)])
    specs.append(np.log(v + 1e-3)); ts_.append(s / sr)
specs = np.array(specs)
d = []
for k in range(2, len(specs) - 2):
    a = specs[k - 2:k].mean(axis=0); b = specs[k:k + 2].mean(axis=0)
    d.append(np.linalg.norm(a - b) / np.sqrt(24))
d = np.array(d)
out.append('--- diem doi pho (khoang cach pho 1s truoc/sau), dinh cuc bo > p90 va > 1.2 ; thoi diem = giua 2 cua so')
th = max(np.percentile(d, 90), 1.2)
for k in range(1, len(d) - 1):
    if d[k] > th and d[k] >= d[k - 1] and d[k] >= d[k + 1]:
        out.append('doi %s  dist %.2f' % (ts(ts_[k + 2] + 0.5 * 0), d[k]))
out.append('--- rms moi 0.5 s (x1000)')
step = 10
line = []
for i in range(0, n - step + 1, step):
    line.append('%s:%d' % (ts(i * 0.05), int(rms[i:i + step].mean() * 1000)))
for i in range(0, len(line), 8): out.append('  '.join(line[i:i + 8]))
open(sys.argv[2], 'w', encoding='utf-8').write('\n'.join(out))
print('\n'.join(out[:60]))
