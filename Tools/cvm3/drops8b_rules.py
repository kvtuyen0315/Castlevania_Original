"""CVM3 08 — bảng "enemy rơi vật" 3 video (Map_1 → Map_2 → Map_3 = MỘT playthrough liền: SCORE đầu Map_2 023370 = cuối Map_1,
đầu Map_3 044670 = cuối Map_2) + thử các họ luật TẤT ĐỊNH.
Danh sách lần hạ = phân loại bằng mắt dải ảnh Tools/cvm3/drops8b_flame.py / drops8b_strip.py (ClaudeNotes/cvm3/_cache/08/drops/
z1_*, z2_*, z3_*, m1_*, m3_* — đã xem): mỗi lần HUD cộng điểm hạ enemy, lửa chết ở đâu, 8–30 frame sau có vật hiện TẠI CHỖ lửa không
(vật từ nến/khối cùng nhát roi được loại theo ô nến MAP1_PLACEMENT / MAP2_PLACEMENT + vị trí). Trạng thái Simon (tim, vũ khí phụ, II/III,
TIME, SCORE) đọc từ HUD từng frame (map1_hud.csv CVGP, map2_hud.csv CVM2 02, map3_hud.csv CVM3 02) ở frame k − 1.
Chạy (từ gốc repo): python -P -E Tools/cvm3/drops8b_rules.py
  → ClaudeNotes/cvm3/ref/data/drops3.tsv (bảng) + drops3_rules.txt (kết quả từng luật: giải thích n/N).
"""
import csv, os
from collections import defaultdict

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data')
HUD = {
    'Map_1': os.path.join(ROOT, 'ClaudeNotes', 'cvgp', '_cache', 'video', 'work', 'map1_hud.csv'),
    'Map_2': os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'data', 'map2_hud.csv'),
    'Map_3': os.path.join(ROOT, 'ClaudeNotes', 'cvm3', 'ref', 'data', 'map3_hud.csv'),
}

# (video, frame HUD cộng điểm, số con hạ cùng frame, loại, vũ khí hạ, vật rơi, ghi chú/nguồn ảnh)
# vật rơi: '-' = không; '?' = không xem được (bị che); tên item = rơi TẠI CHỖ lửa chết
KILLS = [
    # ---- Map_1 (Stage 01–03) — CVGP 15 chỉ ghi 2 lần rơi (đồng hồ 0:29.5, rosary 1:15.03); CVM3 08 thấy thêm 5
    ('Map_1', 690, 2, 'Zombie', 'roi', '-', 'z1_a: 2 lửa, không vật'),
    ('Map_1', 708, 1, 'Zombie', 'roi', '-', 'z1_a; tim k+30 = nến'),
    ('Map_1', 762, 1, 'Zombie', 'roi', 'SmallHeart', 'z1_f: cùng nhát roi vỡ nến EH_C03 ⇒ 2 tim nhỏ k+10 (1 ở lửa Zombie) — tim +1 f788 (nến) + f800 (Zombie, CVGP không gán nguồn)'),
    ('Map_1', 847, 1, 'Zombie', 'roi', '-', 'z1_a: túi đỏ EH_C04 trên sàn từ trước, nhặt f885 (CVGP ghi +100 f847 = nhầm Zombie)'),
    ('Map_1', 865, 2, 'Zombie', 'roi', 'Stopwatch', 'z1_a/m1_f: 2 lửa, 1 con rơi ĐỒNG HỒ k+10 (CVGP 13: f864 ⇒ f874)'),
    ('Map_1', 1012, 1, 'Zombie', 'roi', '-', 'z1_g; tim k+12 = nến EH_C07 cùng nhát'),
    ('Map_1', 1032, 1, 'Zombie', 'roi', 'RedMoneyBag', 'z1_g: túi đỏ ở lửa k+8, Simon nhặt ngay +100 f1042 (CVGP không gán nguồn)'),
    ('Map_1', 1051, 1, 'Zombie', 'roi', '-', 'z1_b'),
    ('Map_1', 1295, 1, 'BlackLeopard', 'roi', '-', 'z1_b'),
    ('Map_1', 1647, 1, 'BlackLeopard', 'roi', '-', 'z1_b: tim lớn k+12 = nến EH_C14 (vỡ f1641)'),
    ('Map_1', 2091, 1, 'Zombie', 'roi', '-', 'z1_b; tim sàn = nến EH_C19'),
    ('Map_1', 2107, 1, 'Zombie', 'roi', '-', 'z1_b'),
    ('Map_1', 2122, 1, 'Zombie', 'roi', '-', 'z1_b'),
    ('Map_1', 2222, 1, 'Zombie', 'roi', 'Rosary', 'z1_b: ROSARY ở lửa k+10 (CVGP 15 rosary1.png)'),
    ('Map_1', 2247, 1, 'Zombie', 'roi', '-', 'z1_b'),
    ('Map_1', 2311, 1, 'Zombie', 'roi', '-', 'z1_c; tim nhặt cùng frame = nến EH_C22'),
    ('Map_1', 2806, 1, 'VampireBat', 'dao', '-', 'z1_c/z1_f'),
    ('Map_1', 2992, 1, 'VampireBat', 'roi', 'SmallHeart', 'z1_f: tim nhỏ ở lửa k+8, nhặt f3010 (CVGP không gán nguồn); túi đỏ cạnh = nến CA_C4'),
    ('Map_1', 3076, 1, 'VampireBat', 'roi', '-', 'z1_c'),
    ('Map_1', 3222, 1, 'VampireBat', 'dao', '?', 'z1_c: k+12.. màn đen đổi tầng + lớp phủ YouTube — không xem được'),
    ('Map_1', 3549, 1, 'FishMan', 'roi', 'LargeHeart', 'z1_c: tim LỚN ở lửa k+10 (nến CB_C1 phía trên vẫn nguyên), +5 f3597 (CVGP không gán nguồn)'),
    ('Map_1', 3576, 1, 'FishMan', 'roi', '-', 'z1_c'),
    ('Map_1', 3632, 1, 'FishMan', 'roi', '-', 'z1_d'),
    ('Map_1', 3827, 1, 'FishMan', 'roi', '-', 'z1_d'),
    ('Map_1', 3974, 1, 'FishMan', 'roi', '-', 'z1_d: tim k+10 ở độ cao nến CB_C6 (cùng nhát roi)'),
    ('Map_1', 4140, 1, 'FishMan', 'roi', '-', 'z1_d'),
    ('Map_1', 4282, 1, 'FishMan', 'roi', '-', 'z1_d; túi nháy bên trái = bí mật CB'),
    ('Map_1', 4491, 1, 'VampireBat', 'roi', 'SmallHeart', 'z1_d: tim nhỏ ở lửa k+10, +1 f4509 (CVGP không gán nguồn); túi đỏ = nến CA_C6'),
    ('Map_1', 4674, 1, 'VampireBat', 'roi', '-', 'z1_d'),
    ('Map_1', 5292, 1, 'Zombie', 'roi', '-', 'z1_e'),
    ('Map_1', 5492, 2, 'Zombie', 'roi', '-', 'z1_e: 2 con'),
    ('Map_1', 5509, 1, 'Zombie', 'roi', '-', 'z1_e'),
    ('Map_1', 5643, 1, 'Zombie', 'roi', '-', 'z1_e'),
    ('Map_1', 5686, 2, 'Zombie', 'roi', '-', 'z1_e: 2 con; rìu sàn = nến PB_C06'),
    # ---- Map_2 (Stage 04–06) — CVM2 10 ghi 3 lần rơi; CVM3 08 xem lại cả 18: đúng 3
    ('Map_2', 251, 1, 'SpearKnight', 'rìu', '-', 'z2_a'),
    ('Map_2', 778, 1, 'BatHanging', 'thập tự', '-', 'z2_a'),
    ('Map_2', 1040, 1, 'BatHanging', 'thập tự', '-', 'z2_a; tim k+30 = nến cùng lượt thập tự'),
    ('Map_2', 1331, 1, 'SpearKnight', 'roi', '-', 'z2_a'),
    ('Map_2', 1872, 1, 'MedusaHead', 'thập tự', '-', 'z2_a'),
    ('Map_2', 2056, 1, 'MedusaHead', 'roi', 'SmallHeart', 'z2_a (CVM2 10 ha528.png)'),
    ('Map_2', 2934, 1, 'MedusaHead', 'roi', '-', 'z2_a'),
    ('Map_2', 2983, 1, 'MedusaHead', 'thập tự', '-', 'z2_a; túi đỏ k+30 = nến thập tự phá lúc quay về'),
    ('Map_2', 3235, 1, 'SpearKnight', 'roi', '-', 'z2_a'),
    ('Map_2', 3507, 1, 'SpearKnight', 'roi', '-', 'z2_b'),
    ('Map_2', 3615, 1, 'SpearKnight', 'roi', '-', 'z2_b'),
    ('Map_2', 4188, 1, 'MedusaHead', 'roi', '-', 'z2_b; tim nhỏ đang rơi từ nến trên'),
    ('Map_2', 5022, 1, 'Ghost', 'roi', '-', 'z2_b'),
    ('Map_2', 5501, 1, 'BoneTower', 'roi', '-', 'z2_b'),
    ('Map_2', 5653, 1, 'BoneTower', 'roi', 'Rosary', 'z2_b (CVM2 10 hc_rosary.png)'),
    ('Map_2', 5865, 1, 'Ghost', 'roi', '-', 'z2_b'),
    ('Map_2', 6227, 1, 'MedusaHead', 'roi', 'SmallHeart', 'z2_b (CVM2 10 md_heart.png)'),
    ('Map_2', 6846, 1, 'Snake', 'roi', '-', 'z2_b: rắn của Medusa, trong lúc đồng hồ'),
    # ---- Map_3 (Stage 07–09)
    ('Map_3', 453, 1, 'Ghost', 'roi', '-', 'z3_a'),
    ('Map_3', 545, 1, 'Fleaman', 'roi', '-', 'z3_a; tim trái = nến (CVM3 07)'),
    ('Map_3', 675, 1, 'Fleaman', 'roi', '-', 'z3_a; tim = nến cùng nhát'),
    ('Map_3', 918, 1, 'Fleaman', 'roi', '-', 'z3_a'),
    ('Map_3', 1078, 1, 'Skeleton', 'roi', '-', 'z3_a'),
    ('Map_3', 1553, 1, 'Skeleton', 'roi', '-', 'z3_a; túi đỏ có từ trước'),
    ('Map_3', 1626, 1, 'Raven', 'roi', '-', 'z3_a'),
    ('Map_3', 1682, 1, 'Raven', 'roi', '-', 'z3_a'),
    ('Map_3', 1732, 1, 'Raven', 'roi', 'SmallHeart', 'z3_a: tim nhỏ đung đưa ở lửa k+10'),
    ('Map_3', 3073, 1, 'Skeleton', 'roi', 'SmallHeart', 'z3_b/m3_f: tim ở lửa k+10, tim +1 f3089'),
    ('Map_3', 3081, 1, 'Skeleton', 'nước thánh', '-', 'm3_f: chết trong lửa nước thánh, CHỮ 1000 tại chỗ (+1100)'),
    ('Map_3', 3644, 1, 'Raven', 'roi', '-', 'z3_b'),
    ('Map_3', 3837, 1, 'BoneTower', 'roi', '-', 'z3_b; tim cạnh tháp = cầu lửa f3728'),
    ('Map_3', 3917, 1, 'Raven', 'roi', '-', 'z3_b'),
    ('Map_3', 3957, 1, 'Raven', 'roi', '-', 'z3_b'),
    ('Map_3', 4096, 1, 'BoneTower', 'roi', '-', 'z3_b'),
    ('Map_3', 4700, 1, 'Raven', 'roi', '-', 'z3_b'),
    ('Map_3', 4745, 1, 'Raven', 'roi', '-', 'z3_b'),
    ('Map_3', 4854, 1, 'BoneTower', 'roi', '-', 'z3_c'),
    ('Map_3', 4904, 1, 'Raven', 'roi', '-', 'z3_c'),
    ('Map_3', 4933, 1, 'Raven', 'roi', '-', 'z3_c'),
    ('Map_3', 5077, 1, 'BoneTower', 'roi', 'LargeHeart', 'z3_c: tim LỚN ở lửa k+10 (tim nhỏ cạnh = cầu lửa f5009)'),
    ('Map_3', 5123, 1, 'Raven', 'roi', '-', 'z3_c'),
    ('Map_3', 5151, 1, 'Skeleton', 'roi', '-', 'z3_c'),
    ('Map_3', 5223, 1, 'Skeleton', 'nước thánh', '-', 'm3_f: chết trong lửa nước thánh, CHỮ 1000 tại chỗ (+1300 sau +100 f5221)'),
    ('Map_3', 5475, 1, 'MedusaHead', 'roi', '-', 'z3_c'),
    ('Map_3', 5511, 1, 'BoneTower', 'roi', '-', 'z3_c: +500 = tháp 400 + cầu lửa 100 cùng nhát'),
    ('Map_3', 5703, 1, 'MedusaHead', 'roi', '-', 'z3_c'),
]
# cầu lửa Bone Tower bị roi phá (+100) — đạn, KHÔNG phải enemy; ghi riêng để thử luật "đếm cả đạn"
SHOTS = [
    ('Map_3', 3728, 'SmallHeart'), ('Map_3', 3746, '-'), ('Map_3', 4028, '-'), ('Map_3', 4042, '-'), ('Map_3', 4778, '-'),
    ('Map_3', 4791, '-'), ('Map_3', 4994, '-'), ('Map_3', 5009, 'SmallHeart'), ('Map_3', 5432, '-'), ('Map_3', 5447, '-'),
    ('Map_3', 5495, '-'),
]


def hud(video):
    rows = {}
    for r in csv.DictReader(open(HUD[video])):
        rows[int(r['frame'])] = r
    return rows


def state(H, f):
    for g in range(f - 1, f - 20, -1):
        r = H.get(g)
        if r and r['hud_ok'] == '1' and r['hearts'].isdigit() and r['time'].isdigit() and r['score'].isdigit():
            return r
    return None


def main():
    huds = {v: hud(v) for v in HUD}
    rows = []
    gidx = 0
    per_stage = defaultdict(int)
    per_type = defaultdict(int)
    events = []
    for v, f, n, typ, wpn, drop, note in KILLS:
        events.append((v, f, 'kill', n, typ, wpn, drop, note))
    for v, f, drop in SHOTS:
        events.append((v, f, 'shot', 1, 'BoneTowerFireball', 'roi', drop, 'z3_d/z3_e'))
    vo = {'Map_1': 0, 'Map_2': 1, 'Map_3': 2}
    events.sort(key=lambda e: (vo[e[0]], e[1]))
    gall = 0
    for v, f, kind, n, typ, wpn, drop, note in events:
        st = state(huds[v], f)
        stage = int(st['stage']) if st else 0
        gall += n
        if kind == 'kill':
            gidx += n
            per_stage[stage] += n
            per_type[typ] += n
        rows.append(dict(video=v, mss='%d:%05.2f' % (f // 1800, (f % 1800) / 30.0), frame=f, stage=stage, kind=kind, n=n, enemy=typ,
                         weapon=wpn, drop=drop, idx_video=gidx if kind == 'kill' else '', idx_stage=per_stage[stage] if kind == 'kill' else '',
                         idx_type=per_type[typ] if kind == 'kill' else '', idx_all=gall,
                         hearts=int(st['hearts']) if st else -1, sub=st['sub'] if st else '', mult=st['mult'] if st else '',
                         time=int(st['time']) if st else -1, score=int(st['score']) if st else -1, note=note))
    os.makedirs(OUT, exist_ok=True)
    keys = ['video', 'mss', 'frame', 'stage', 'kind', 'n', 'enemy', 'weapon', 'drop', 'idx_video', 'idx_stage', 'idx_type', 'idx_all',
            'hearts', 'sub', 'mult', 'time', 'score', 'note']
    with open(os.path.join(OUT, 'drops3.tsv'), 'w', newline='', encoding='utf-8') as fo:
        fo.write('# CVM3 08 — enemy rơi vật, 3 video liền 1 playthrough (Tools/cvm3/drops8b_rules.py). idx_video = thứ tự hạ cộng dồn từ đầu Map_1'
                 ' (n con cùng frame tính n); idx_all = cả cầu lửa bị phá; hearts/TIME/SCORE = HUD frame k−1\n')
        w = csv.DictWriter(fo, fieldnames=keys, delimiter='\t', lineterminator='\n')
        w.writeheader()
        w.writerows(rows)
    kills = [r for r in rows if r['kind'] == 'kill' and r['drop'] != '?']
    out = []
    pos = [r for r in kills if r['drop'] != '-']
    out.append('Mẫu: %d lần hạ có xem được (%d rơi, %d không), + %d cầu lửa (%d rơi); 1 lần không xem được (Map_1 f3222)' % (
        len(kills), len(pos), len(kills) - len(pos), len(SHOTS), sum(1 for s in SHOTS if s[2] != '-')))
    by = defaultdict(lambda: [0, 0])
    for r in kills:
        by[r['enemy']][0] += 1
        by[r['enemy']][1] += r['drop'] != '-'
    out.append('Theo loại (hạ / rơi): ' + ', '.join('%s %d/%d' % (k, b, a) for k, (a, b) in sorted(by.items())))
    items = defaultdict(int)
    for r in pos:
        items[r['drop']] += 1
    out.append('Vật rơi: ' + ', '.join('%s ×%d' % kv for kv in sorted(items.items())))

    def test(name, val, data, nmax=40):
        """luật 'rơi ⇔ val mod N == r' (một residue) và 'rơi ⇔ val mod N ∈ tập residue của mẫu dương' (tập R) — báo N nhỏ nhất đạt 100 %."""
        best = None
        first100 = None
        for N in range(2, nmax + 1):
            for r0 in range(N):
                ok = sum(1 for d in data if ((val(d) % N) == r0) == (d['drop'] != '-'))
                if best is None or ok > best[0]:
                    best = (ok, N, r0)
            R = set(val(d) % N for d in data if d['drop'] != '-')
            okR = sum(1 for d in data if ((val(d) % N) in R) == (d['drop'] != '-'))
            if okR == len(data) and first100 is None:
                first100 = (N, sorted(R))
        out.append('  %-34s 1 residue: tốt nhất %d/%d (N %d r %d) | tập residue 100 %%: %s' % (
            name, best[0], len(data), best[1], best[2],
            ('N %d với %d/%d residue (chọn từ chính mẫu — quá khớp nếu tập ≈ số mẫu dương)' % (first100[0], len(first100[1]), first100[0])) if first100 else 'không có N ≤ %d' % nmax))

    out.append('Luật "rơi ⇔ đại lượng mod N" (mẫu = lần hạ có xem được, n con cùng frame tính 1 dòng):')
    test('thứ tự hạ cộng dồn (idx_video)', lambda d: d['idx_video'], kills)
    test('thứ tự hạ trong stage', lambda d: d['idx_stage'], kills)
    test('thứ tự hạ cùng loại', lambda d: d['idx_type'], kills)
    allr = [r for r in rows if r['drop'] != '?']
    test('thứ tự cả enemy + cầu lửa (idx_all)', lambda d: d['idx_all'], allr)
    test('tim lúc hạ', lambda d: d['hearts'], kills)
    test('TIME lúc hạ', lambda d: d['time'], kills)
    test('SCORE/100 lúc hạ', lambda d: d['score'] // 100, kills)
    test('SCORE/100 + điểm enemy', lambda d: d['score'] // 100 + 1, kills)
    test('frame video (≈ tick/2)', lambda d: d['frame'], kills)
    # luật đơn giản khác
    out.append('Luật khác:')
    same_type_diff = [k for k, (a, b) in by.items() if 0 < b < a]
    out.append('  "theo loại enemy" (luôn/không bao giờ): SAI — loại có cả rơi lẫn không: ' + ', '.join(same_type_diff))
    wp = defaultdict(lambda: [0, 0])
    for r in kills:
        wp[r['weapon']][0] += 1
        wp[r['weapon']][1] += r['drop'] != '-'
    out.append('  "theo vũ khí hạ": ' + ', '.join('%s %d/%d' % (k, b, a) for k, (a, b) in wp.items()) + ' — roi có cả rơi lẫn không ⇒ không đủ')
    sb = defaultdict(lambda: [0, 0])
    for r in kills:
        sb[r['sub'] + (r['mult'] or '')][0] += 1
        sb[r['sub'] + (r['mult'] or '')][1] += r['drop'] != '-'
    out.append('  "theo vũ khí phụ đang cầm": ' + ', '.join('%s %d/%d' % (k, b, a) for k, (a, b) in sb.items()))
    gaps = []
    last = None
    for r in kills:
        if r['drop'] != '-':
            if last is not None:
                gaps.append(r['idx_video'] - last)
            last = r['idx_video']
    out.append('  khoảng cách (số lần hạ) giữa 2 lần rơi liên tiếp: %s — có khoảng 1 (2 lần hạ liền đều rơi: Map_3 f1732 Raven → f3073 Skeleton) và khoảng tới 14 ⇒ không phải "mỗi N lần hạ"' % gaps)
    txt = '\n'.join(out) + '\n'
    open(os.path.join(OUT, 'drops3_rules.txt'), 'w', encoding='utf-8').write(txt)
    import sys
    sys.stdout.buffer.write(txt.encode('utf-8'))


if __name__ == '__main__':
    main()
