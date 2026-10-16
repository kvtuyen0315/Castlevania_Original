# -*- coding: utf-8 -*-
"""cvm2-03 - kiem diem lap nhac tren AM THANH DAU RA THAT cua Unity (MusicProbeTap ghi WAV).
python -P -E Tools/cvm2/music3_tapcheck.py <tap.wav> NN
Moi cua so 50 ms cua ban ghi: tim vi tri tot nhat trong bai goc (ffmpeg sNN.wav, mau 44.1 kHz) -> songpos + ncc;
in bang + ket luan: vi tri truoc/sau cu nhay, buoc nhay = loopLen?, ncc thap nhat, rms 10 ms thap nhat quanh cu nhay (khoang lang?)."""
import sys, os, json
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
import music3 as M

tap, nn = sys.argv[1], sys.argv[2]
x, sr = M.rd(tap)
x = x.mean(1) if x.ndim > 1 else x
# bo phan dau (truoc khi co tieng) va doi ve 44.1 kHz
ref, rsr = M.rd(os.path.join(M.AUD, 's%s.wav' % nn))
ref = ref.mean(1)
t_new = np.arange(0, len(x) / sr, 1.0 / rsr)
y = np.interp(t_new, np.arange(len(x)) / sr, x).astype(np.float32)
L = json.load(open(os.path.join(M.OUT, 'loops2.json')))[nn]
ls, le = L['loopStart'], L['loopEnd']
w = int(0.05 * rsr)
nz = np.flatnonzero(np.abs(y) > 1e-3)
i0 = int(nz[0]) if len(nz) else 0
rows = []
for i in range(i0, len(y) - w, w):
    a = y[i:i + w]
    if np.sqrt((a * a).mean()) < 1e-3:
        rows.append((i, -1, 0.0))
        continue
    # vung tim: quanh loopEnd (truoc cu nhay) va quanh loopStart (sau)
    best = (-2, 0)
    for c0 in (le - int(1.5 * rsr), ls - int(0.2 * rsr)):
        seg = ref[max(0, c0):c0 + int(2.0 * rsr)]
        n = 1 << int(np.ceil(np.log2(len(seg) + w)))
        cc = np.fft.irfft(np.fft.rfft(seg, n) * np.conj(np.fft.rfft(a, n)), n)[:len(seg) - w + 1]
        e = np.cumsum(np.concatenate([[0.0], seg.astype(np.float64) ** 2]))
        eb = e[w:] - e[:-w]
        nc = cc / (np.sqrt(np.maximum(eb[:len(cc)], 1e-9) * (a.astype(np.float64) ** 2).sum()) + 1e-12)
        k = int(nc.argmax())
        if nc[k] > best[0]:
            best = (float(nc[k]), max(0, c0) + k)
    rows.append((i, best[1], best[0]))
print('tap %s bai %s: loopStart %d loopEnd %d (len %d) | sr tap %d' % (os.path.basename(tap), nn, ls, le, le - ls, sr))
prev = None
jump = None
for i, p, c in rows:
    flag = ''
    if prev is not None and p >= 0 and prev >= 0:
        d = p - prev
        if abs(d - w) > 200:
            flag = '  <-- NHAY %+d mau (loopLen %d, lech %+d)' % (d, le - ls, d + (le - ls) - w)
            if jump is None and d < -100000:   # cu nhay lap that (lui ~1 vong) = lan dau
                jump = (i, prev, p, d)
    print('  tap %6.3f s  songpos %9d (%.4f s)  ncc %.3f%s' % (i / rsr, p, p / rsr, c, flag))
    prev = p
# rms 10 ms quanh cu nhay
if jump:
    j = jump[0]
    q = int(0.01 * rsr)
    r = [float(np.sqrt((y[k:k + q] ** 2).mean())) for k in range(max(i0, j - 20 * q), min(len(y) - q, j + 20 * q), q)]
    print('rms 10 ms quanh cu nhay (x1000): ' + ' '.join('%d' % (v * 1000) for v in r))
    # so voi bai goc phat lien (khong nhay): vi tri goc tuong ung cung doan
    k0 = max(i0, j - 20 * q)
    rr = [float(np.sqrt((ref[jump[1] + (k - j + w):jump[1] + (k - j + w) + q] ** 2).mean())) for k in range(k0, j, q)] +          [float(np.sqrt((ref[jump[2] - w + (k - j) + w:jump[2] + (k - j) + q] ** 2).mean())) for k in range(j, min(len(y) - q, j + 20 * q), q)]
    print('rms bai goc cung vi tri (x1000):   ' + ' '.join('%d' % (v * 1000) for v in rr))
    print('rms toan ban ghi trung vi %.0f, min quanh cu nhay %.0f' % (1000 * float(np.median([np.sqrt((y[k:k+q]**2).mean()) for k in range(i0, len(y) - q, q)])), 1000 * min(r)))
cs = [c for _, p, c in rows if p >= 0]
print('ncc min %.3f, trung vi %.3f, so cua so %d' % (min(cs), float(np.median(cs)), len(cs)))
