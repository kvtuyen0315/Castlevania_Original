# -*- coding: utf-8 -*-
"""cvm2-03 - do nhac: diem lap tung bai + nhan dang bai/vi tri trong video bang tuong quan cheo pho.

Chay tu goc repo (python -P -E). Dau vao: wav do ffmpeg giai ma (ClaudeNotes/cvm2/_cache/03/audio):
  mNN.wav  = Art_Source/music/NN*.mp3 mono 22050 Hz
  sNN.wav  = Art_Source/music/NN*.mp3 stereo 44100 Hz (tinh mau chinh xac cho diem lap)
  v_<video>.wav = am thanh video mono 22050 Hz
Lenh:
  loop  [NN ...]           -> diem lap (mau 44.1 kHz) + do khop, ghi _cache/03/loops.json
  ident <video> [win step] -> moi cua so: bai tot nhat + songpos + ncc (CSV _cache/03/ident_<video>.csv)
  seg   <video>            -> gop cua so thanh doan phat lien tuc (offset = t - songpos khong doi)
  pos   <video> NN t0 t1 [win step] -> songpos cua 1 bai trong khoang (kiem tung bai)
"""
import json, os, sys, wave
import numpy as np

AUD2 = 'ClaudeNotes/cvm2/_cache/03/audio'          # bai nhac (cvm2 da giai ma)
AUD = 'ClaudeNotes/cvm3/_cache/03'
OUT = 'ClaudeNotes/cvm3/_cache/03'
SR = 22050
HOP = 256            # 11.6 ms
NFFT = 2048
SONGS = ['%02d' % i for i in range(1, 17)]


def rd(p):
    w = wave.open(p)
    n, sr, ch = w.getnframes(), w.getframerate(), w.getnchannels()
    d = np.frombuffer(w.readframes(n), dtype=np.int16).astype(np.float32) / 32768.0
    if ch > 1:
        d = d.reshape(-1, ch)
    return d, sr


_bands = None


def bands():
    global _bands
    if _bands is None:
        f = np.fft.rfftfreq(NFFT, 1.0 / SR)
        edges = np.geomspace(90, 6000, 65)
        _bands = [(np.searchsorted(f, edges[i]), max(np.searchsorted(f, edges[i + 1]), np.searchsorted(f, edges[i]) + 1)) for i in range(64)]
    return _bands


def feat(x):
    """log pho 64 bang (90 Hz - 6 kHz), tru trung binh + chuan hoa tung khung."""
    n = 1 + max(0, (len(x) - NFFT) // HOP)
    idx = np.arange(NFFT)[None, :] + HOP * np.arange(n)[:, None]
    fr = x[idx] * np.hanning(NFFT)[None, :]
    S = np.abs(np.fft.rfft(fr, axis=1))
    B = np.stack([S[:, a:b].mean(1) for a, b in bands()], 1)
    B = np.log1p(B * 100.0)
    e = B.mean(1)
    B = B - B.mean(1, keepdims=True)
    B /= np.linalg.norm(B, axis=1, keepdims=True) + 1e-9
    return B.astype(np.float32), e


_cache = {}


def song_feat(nn):
    if nn not in _cache:
        x, _ = rd(os.path.join(AUD2, 'm%s.wav' % nn))
        _cache[nn] = feat(x)
    return _cache[nn]


def xcorr(A, B):
    """score[k] = mean_i <A[i], B[k+i]>, k = 0..len(B)-len(A)."""
    L, N = len(A), len(B)
    if N < L:
        return np.zeros(0)
    n = 1 << int(np.ceil(np.log2(N + L)))
    FA = np.fft.rfft(A, n, axis=0)
    FB = np.fft.rfft(B, n, axis=0)
    c = np.fft.irfft((FB * np.conj(FA)).sum(1), n)
    return c[:N - L + 1] / L


# ------------------------------------------------------------------ loop
def find_loop(nn):
    F, e = song_feat(nn)
    N = len(F)
    dur = N * HOP / SR
    # do tuong tu theo do tre: duong cheo lag cua ma tran G = F F^T; lag co tong thoi gian khop (> 0.9) lon nhat = do dai vong
    G = F @ F.T
    minlag = int(1.5 * SR / HOP)
    best = (0, 0)
    for lag in range(minlag, N - minlag):
        c = int((np.diagonal(G, lag) > 0.9).sum())
        if c > best[0]:
            best = (c, lag)
    cnt, lag = best
    if lag == 0:
        return dict(frames=N, dur=dur, lag_f=0, run_f=0, start_f=0)
    s = np.convolve(np.diagonal(G, lag), np.ones(9) / 9.0, 'same')
    ok = s > 0.85
    d = np.diff(np.concatenate([[0], ok.astype(np.int8), [0]]))
    st, en = np.flatnonzero(d == 1), np.flatnonzero(d == -1)
    if len(st) == 0:
        return dict(frames=N, dur=dur, lag_f=lag, run_f=0, start_f=0, cnt_f=cnt)
    j = int((en - st).argmax())
    return dict(frames=N, dur=dur, lag_f=lag, run_f=int(en[j] - st[j]), start_f=int(st[j]), cnt_f=cnt)


def refine_loop(nn, lag_f, start_f, run_f):
    """tinh mau o 44.1 kHz: lag chinh xac bang tuong quan dang song, start = mau dau tien x[s] ~ x[s+L]."""
    x, sr = rd(os.path.join(AUD2, 's%s.wav' % nn))
    m = x.mean(1)
    k = sr / SR
    L0 = int(lag_f * HOP * k)
    # doan tham chieu 1 s giua vung khop
    mid = int((start_f + run_f * 0.5) * HOP * k)
    seg = m[mid:mid + sr]
    best = (-2, L0)
    for L in range(L0 - 1500, L0 + 1501):
        b = m[mid + L:mid + L + sr]
        if len(b) < len(seg):
            break
        c = float((seg * b).sum() / (np.sqrt((seg * seg).sum() * (b * b).sum()) + 1e-12))
        if c > best[0]:
            best = (c, L)
    ncc, L = best
    # residual theo cua so 10 ms
    w = int(0.010 * sr)
    n = len(m) - L
    d = m[:n] - m[L:L + n]
    nw = n // w
    rr = np.sqrt((d[:nw * w].reshape(nw, w) ** 2).mean(1))
    ra = np.sqrt((m[:nw * w].reshape(nw, w) ** 2).mean(1)) + 1e-6
    rel = rr / ra
    good = rel < 0.25
    s0 = int(start_f * HOP * k) // w
    # lui ve truoc tu s0 toi khi het khop
    i = min(s0 + 5, nw - 1)
    while i > 0 and (good[i - 1] or (i > 2 and good[i - 2] and good[i - 3])):
        i -= 1
    st = i
    # tien ve sau tu vung khop toi khi het khop (bat dau fade / het file)
    j = min(int((start_f + run_f * 0.5) * HOP * k) // w, nw - 1)
    while j < nw - 1 and (good[j + 1] or (j + 3 < nw and good[j + 2] and good[j + 3])):
        j += 1
    en = j + 1
    # tinh mau cho start: trong cua so st, tim mau dau tien |d| nho lien tiep
    s_samp = st * w
    seg_rel = float(np.median(rel[st:en]))
    return dict(sr=sr, total=len(m), loop_len=L, ncc=round(ncc, 5),
                match_start=s_samp, match_end=en * w, median_rel=round(seg_rel, 4))


def cmd_loop(args):
    out = {}
    p = os.path.join(OUT, 'loops.json')
    if os.path.exists(p):
        out = json.load(open(p))
    for nn in (args or SONGS):
        a = find_loop(nn)
        r = dict(a)
        if a['run_f'] * HOP / SR >= 4.0 and a.get('cnt_f', 0) * HOP / SR >= 6.0:
            r.update(refine_loop(nn, a['lag_f'], a['start_f'], a['run_f']))
        out[nn] = r
        sr = r.get('sr', 44100)
        print(nn, 'dur %.2f' % a['dur'], 'lag %.3f s' % (a['lag_f'] * HOP / SR), 'run %.2f s cnt %.2f s' % (a['run_f'] * HOP / SR, a.get('cnt_f', 0) * HOP / SR),
              'start %.3f' % (a['start_f'] * HOP / SR),
              ('L %d (%.4f s) ncc %.4f match %.3f-%.3f s rel %.3f' % (r['loop_len'], r['loop_len'] / sr, r['ncc'],
                                                                 r['match_start'] / sr, r['match_end'] / sr, r['median_rel'])) if 'loop_len' in r else 'KHONG LAP')
        sys.stdout.flush()
    json.dump(out, open(p, 'w'), indent=1)


# ------------------------------------------------------------------ ident
def vid_feat(v):
    x, _ = rd(os.path.join(AUD, 'v_%s.wav' % v))
    return feat(x)


def cmd_ident(args):
    v = args[0]
    win = float(args[1]) if len(args) > 1 else 3.0
    step = float(args[2]) if len(args) > 2 else 0.5
    F, e = vid_feat(v)
    W = int(win * SR / HOP)
    S = int(step * SR / HOP)
    songs = {nn: song_feat(nn)[0] for nn in SONGS}
    rows = []
    for i in range(0, len(F) - W, S):
        A = F[i:i + W]
        t = i * HOP / SR
        res = []
        for nn, B in songs.items():
            c = xcorr(A, B)
            if len(c) == 0:
                continue
            k = int(c.argmax())
            res.append((float(c[k]), nn, k * HOP / SR))
        res.sort(reverse=True)
        lvl = float(e[i:i + W].mean())
        b, s2 = res[0], res[1]
        rows.append((t, b[1], b[2], b[0], s2[1], s2[2], s2[0], lvl))
    p = os.path.join(OUT, 'ident_%s.csv' % v)
    with open(p, 'w') as f:
        f.write('t,song,songpos,ncc,song2,songpos2,ncc2,level\n')
        for r in rows:
            f.write('%.3f,%s,%.3f,%.4f,%s,%.3f,%.4f,%.3f\n' % r)
    print('ghi', p, len(rows))


def canon(nn, pos, loops):
    """dua songpos ve vong dau (vung lap 2 = vung lap 1)."""
    L = loops.get(nn, {})
    if not L.get('loop'):
        return pos
    ls = L['loopStart'] / L['sr']
    ll = L['loopLen'] / L['sr']
    while pos >= ls + ll - 0.05:
        pos -= ll
    return pos


def cmd_seg(args):
    v = args[0]
    thr = float(args[1]) if len(args) > 1 else 0.45
    loops = json.load(open(os.path.join(OUT, 'loops2.json')))
    rows = [l.strip().split(',') for l in open(os.path.join(OUT, 'ident_%s.csv' % v)).readlines()[1:]]
    segs = []
    for r in rows:
        t, nn, pos, ncc = float(r[0]), r[1], float(r[2]), float(r[3])
        if ncc < thr:
            continue
        cp = canon(nn, pos, loops)
        off = t - cp
        if segs and segs[-1]['song'] == nn and abs(segs[-1]['off_last'] - off) < 0.08 and t - segs[-1]['t1'] < 3.0:
            s = segs[-1]
            s['t1'] = t
            s['n'] += 1
            s['ncc'].append(ncc)
            s['offs'].append(off)
            s['off_last'] = off
            s['p1'] = cp
        else:
            segs.append(dict(song=nn, t0=t, t1=t, p0=cp, p1=cp, n=1, ncc=[ncc], offs=[off], off_last=off))
    for s in segs:
        print('%7.2f-%7.2f  bai %s  songpos %6.2f-%6.2f  t0-songpos(=goc bai) %8.3f (sd %.3f)  ncc tb %.3f  n %d' % (
            s['t0'], s['t1'], s['song'], s['p0'], s['p1'], float(np.median(s['offs'])), float(np.std(s['offs'])),
            float(np.mean(s['ncc'])), s['n']))


def cmd_pos(args):
    v, nn, t0, t1 = args[0], args[1], float(args[2]), float(args[3])
    win = float(args[4]) if len(args) > 4 else 2.0
    step = float(args[5]) if len(args) > 5 else 0.5
    F, e = vid_feat(v)
    B = song_feat(nn)[0]
    W = int(win * SR / HOP)
    t = t0
    while t < t1:
        i = int(t * SR / HOP)
        A = F[i:i + W]
        c = xcorr(A, B)
        k = int(c.argmax())
        print('%7.2f  songpos %7.3f  ncc %.3f  goc %8.3f' % (t, k * HOP / SR, c[k], t - k * HOP / SR))
        t += step


# ------------------------------------------------------------------ loop2 (chot)
# Ban rip KHONG lap dung tung mau: do tre giua 2 vong dao dong +-12 ms (jitter khung ghi am) => diem lap chon theo khop CUC BO
# quanh diem nhay. Bang tho (lag, vung khop rs..re) lay tu duong cheo ma tran tu tuong dong (lenh loop + kiem tay, CVM2_MEMORY §2d).
COARSE = {
    '01': (9.590, 0.5, 19.0),
    '03': (29.82, 0.05, 37.0),
    '04': (29.826, 4.0, 42.0),
    '05': (45.25, 22.6, 30.6),
    '06': (35.15, 6.0, 46.0),
    '07': (55.92, 0.0, 15.0),
    '08': (31.951, 0.0, 37.0),
    '09': (35.143, 0.0, 4.7),
    '10': (7.43, 0.03, 30.0),
    '11': (15.975, 1.0, 25.0),
}
NOLOOP = ['02', '12', '13', '14', '15', '16']


def local_lag(m, sr, p, Lc, w, rng):
    """lag tot nhat (mau) cho cua so m[p:p+w] trong [Lc-rng, Lc+rng] (FFT) -> (lag, ncc)."""
    a = m[p:p + w]
    b0 = p + Lc - rng
    b = m[b0:b0 + w + 2 * rng]
    if len(b) < w + 2 * rng:
        return 0, -1.0
    n = 1 << int(np.ceil(np.log2(len(b) + w)))
    c = np.fft.irfft(np.fft.rfft(b, n) * np.conj(np.fft.rfft(a, n)), n)[:2 * rng + 1]
    e = np.cumsum(np.concatenate([[0.0], b.astype(np.float64) ** 2]))
    eb = e[w:w + 2 * rng + 1] - e[0:2 * rng + 1]
    ea = float((a.astype(np.float64) ** 2).sum())
    if ea < w * 1e-4:            # cua so qua nho (im lang) -> khong dung lam diem nhay
        return 0, -1.0
    nc = c / (np.sqrt(np.maximum(eb, w * 1e-4) * ea) + 1e-12)
    k = int(nc.argmax())
    return Lc - rng + k, float(nc[k])


def cmd_loop2(args):
    out = {}
    for nn in SONGS:
        x, sr = rd(os.path.join(AUD2, 's%s.wav' % nn))
        m = x.mean(1)
        r = dict(total=len(m), sr=sr, dur=round(len(m) / sr, 4))
        if nn not in COARSE:
            r['loop'] = False
            out[nn] = r
            print(nn, 'khong lap, %d mau' % len(m))
            continue
        Lc, rs, re_ = COARSE[nn]
        Lc = int(Lc * sr)
        w = int(0.08 * sr)    # 80 ms moi phia diem nhay
        rng = int(0.05 * sr)
        # bat dau fade: diem cuoi rms 0.5 s >= 0.7 x trung vi than bai
        hw = sr // 2
        rr = np.array([np.sqrt((m[i:i + hw] ** 2).mean()) for i in range(0, len(m) - hw, hw)])
        med = float(np.median(rr[rr > 0.02]))
        fade = (int(np.flatnonzero(rr >= 0.7 * med)[-1]) + 1) * 0.5
        r['fadeStart_s'] = fade
        best = None
        p = max(0.05, rs)
        pend = min(re_ - 0.1, rs + 10.0, fade - Lc / sr - 0.2)
        if pend <= p:            # 1 vong + fade: diem nhay buoc phai o dau vung khop
            p, pend = rs, rs + 1.5
        while p < pend:
            P = int(p * sr)
            L, c = local_lag(m, sr, P, Lc, w, rng)
            # can them: cua so 0.3 s TRUOC diem nhay cung phai khop (noi lien 2 phia)
            L2, c2 = local_lag(m, sr, P - w, Lc, w, rng)
            sc = min(c, c2) if abs(L2 - L) <= 3 else min(c, c2) - 0.5
            if best is None or sc > best[0] + 1e-4:
                best = (sc, P, L, c, c2, L2)
            p += 0.05
        sc, P, L, c, c2, L2 = best
        # kiem 1 s sau diem nhay voi dung lag L
        n1 = min(sr, len(m) - (P + L))
        a = m[P:P + n1]
        b = m[P + L:P + L + n1]
        c1s = float((a * b).sum() / np.sqrt((a * a).sum() * (b * b).sum() + 1e-12))
        # do to: rms 2 s quanh loopEnd so voi quanh loopStart (fade => < 1)
        ra = np.sqrt((m[max(0, P - sr):P + sr] ** 2).mean())
        rb = np.sqrt((m[max(0, P + L - sr):P + L + sr] ** 2).mean())
        r['rmsRatioEnd'] = round(float(rb / (ra + 1e-9)), 3)
        q = int(0.25 * sr)
        r['gainAtEnd'] = round(float(np.sqrt((m[P + L:P + L + q] ** 2).mean()) / (np.sqrt((m[P:P + q] ** 2).mean()) + 1e-9)), 3)
        r.update(loop=True, loopStart=P, loopEnd=P + L, loopLen=L, nccAfter=round(c, 4), nccBefore=round(c2, 4),
                 lagBefore=L2, ncc1s=round(c1s, 4), margin_s=round((len(m) - (P + L)) / sr, 2))
        out[nn] = r
        print(nn, 'loopStart %d (%.4f s) loopEnd %d (%.4f s) L %.4f s ncc sau %.3f truoc %.3f (lag truoc lech %d mau) 1s %.3f con %.1f s rms cuoi/dau %.2f gain %.2f' % (
            P, P / sr, P + L, (P + L) / sr, L / sr, c, c2, L2 - L, c1s, r['margin_s'], r['rmsRatioEnd'], r['gainAtEnd']))
        sys.stdout.flush()
    json.dump(out, open(os.path.join(OUT, 'loops2.json'), 'w'), indent=1)


# ------------------------------------------------------------------ export (cho Unity MusicLibraryBuilder)
UNITY_OFFSET = 2257   # mau Unity = mau ffmpeg + 2257 (Unity giu do tre bo ma MP3 1105 + 1 khung 1152; do 5/5 jingle NCC >= 0.999)
NAMES = {'01': ('Underground', 'm01_underground'), '02': ('Prologue', 'm02_prologue'), '03': ('VampireKiller', 'm03_vampire_killer'),
         '04': ('Stalker', 'm04_stalker'), '05': ('WickedChild', 'm05_wicked_child'), '06': ('WalkingOnTheEdge', 'm06_walking_on_the_edge'),
         '07': ('HeartOfFire', 'm07_heart_of_fire'), '08': ('OutOfTime', 'm08_out_of_time'), '09': ('NothingToLose', 'm09_nothing_to_lose'),
         '10': ('PoisonMind', 'm10_poison_mind'), '11': ('BlackNight', 'm11_black_night'), '12': ('StageClear', 'm12_stage_clear'),
         '13': ('GameComplete', 'm13_game_complete'), '14': ('Voyager', 'm14_voyager'), '15': ('Death', 'm15_death'), '16': ('GameOver', 'm16_game_over')}
APPROX = {'05': 'xap xi: ban rip chi 1 vong roi fade - diem nhay o dau fade (gain 0.49)',
          '09': 'xap xi: ban rip chi 1 vong roi fade - diem nhay o dau fade (gain 0.71)'}


def cmd_export(args):
    L = json.load(open(os.path.join(OUT, 'loops2.json')))
    rows = []
    for nn in SONGS:
        r = L[nn]
        name, file = NAMES[nn]
        e = dict(id=name, num=int(nn), file=file + '.mp3', startSample=UNITY_OFFSET, loop=bool(r.get('loop')))
        if e['loop']:
            e.update(loopStart=r['loopStart'] + UNITY_OFFSET, loopEnd=r['loopEnd'] + UNITY_OFFSET,
                     loopSeconds=round(r['loopLen'] / r['sr'], 4), nccBefore=r['nccBefore'], nccAfter=r['nccAfter'],
                     gainAtEnd=r['gainAtEnd'], note=APPROX.get(nn, ''))
            e['src'] = 'do:cvm2-03 Tools/cvm2/music3.py loop2 (khop dang song cuc bo 80 ms 2 phia, ncc %.3f/%.3f)' % (r['nccBefore'], r['nccAfter'])
        else:
            e['src'] = 'do:cvm2-03 khong lap (jingle / ket game; ma tran tu tuong dong khong co vong)'
        rows.append(e)
    json.dump(rows, open(args[0] if args else 'Tools/cvm2/music_loops.json', 'w', encoding='utf-8'), indent=1)
    print('ghi', len(rows))


if __name__ == '__main__':
    cmd = sys.argv[1]
    dict(loop=cmd_loop, loop2=cmd_loop2, export=cmd_export, ident=cmd_ident, seg=cmd_seg, pos=cmd_pos)[cmd](sys.argv[2:])
