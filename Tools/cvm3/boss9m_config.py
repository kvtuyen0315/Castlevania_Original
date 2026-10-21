"""CVM3 09 — thay khối dòng Boss.MummyMan.* trong Assets/Data/Config/Csv/ConfigActorParam.csv bằng số ĐO 2 video (CVM3_MEMORY §2i,
Tools/cvm3/boss9m_model.py ⇒ ClaudeNotes/cvm3/ref/data/boss9m/model.txt) + sửa dòng MummyMan của ConfigBoss.csv (đòn, điểm, tốc độ) + thêm nội
dung khối Pot Roast phòng thật (ConfigDrop MummyMan_X83_9). Chạy lại ⇒ y hệt (idempotent). Giữ LF như file gốc.
Chạy (từ gốc repo): python -P -E Tools/cvm3/boss9m_config.py   rồi menu CVGP/Import Config.
"""
import csv, io, os

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
CSV = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv')
AP = os.path.join(CSV, 'ConfigActorParam.csv')
BO = os.path.join(CSV, 'ConfigBoss.csv')
DR = os.path.join(CSV, 'ConfigDrop.csv')

M3 = 'video:Map_3 3:17.7–3:50.7 (f5931–6920)'
B3 = 'video:Boss_Map_3 12.5–40.8 s (f374–1223, chỉ chuyển động — khác bảng màu)'
MM = [
    ('hitStunTicks', '0', '0', '2', 'tick', 'đo:cvm3-09 ' + B3 + ': ENEMY −1 lúc Mummy đang đi f690 (x 1447→1449→1452) và f840 (1496→1497→1500) — không đứng lại sau đòn roi (Phantom Bat 10 / Medusa 10 thì có)', 'trúng roi/dao không khựng'),
    ('invincibleTicks', '32', '30', '34', 'tick', 'đo:cvm3-09 ' + M3 + ': trong 1 lửa nước thánh 2–3 lần ENEMY −1 cách 30 / 32 / 34 tick (f6183→6200→6215, f6270→6286, f6435→6450); ' + B3 + ' 2 nhát roi liền nhất cách 34 tick (f823→840)', 'DÙNG CHUNG cả cặp (MummyMan.OnHurt): video Map_3 f6186–6216 cả 2 đứng trong lửa nhưng chỉ −1 mỗi ~32 tick, 2 video 0 lần −2 cùng lúc; lửa đánh lại mỗi 8 tick ⇒ trúng mỗi 32'),
    ('holyStunTicks', '12', '11', '13', 'tick', 'đo:cvm3-09 ' + M3 + ': Mummy đứng yên (khung anim đứng) suốt lúc chạm lọ/lửa; đi lại 13 tick sau khi lửa lọ 1 tắt (f6101–6149, lửa f6104.5–6142.5) và 11 tick sau lửa lọ 5 (f6422–6468, lửa tới f6462.5) — wiki "hoá đá"', 'khựng thêm sau khi rời nước thánh'),
    ('deathFxTicks', '62', '', '', 'tick', 'đo:cvm3-09 ' + M3 + ' Mummy 2 f6770–6799: bossdeath_m 0,1,2,0,1,2,0 mỗi ~9 tick (Tools/cvm3/boss9m_model.py §7) = Phantom Bat (đo 14)', ''),
    ('deathFxUpPx', '24', '', '', 'px', 'đo:cvm3-09 ' + M3 + ' f6686 / f6770: tâm cụm lửa bossdeath_m (24×32) ysec 153 khi chân Mummy ysec 177 ⇒ tâm = chân − 24, x = X Mummy (1360 / 1366)', 'tâm lửa so với chân'),
    ('partnerDxPx', '191', '', '', 'px', 'đo:cvm3-09 ' + M3 + ' f5583–5990 + ' + B3 + ' f37–433: 2 Mummy đứng chờ ở x 1312 (Boss 1311) và 1503, khung 0 quay trái, trước khi khoá', 'con 2 sinh cách con đầu về trái'),
    ('musicDelayTicks', '60', '50', '72', 'tick', 'đo:cvm3-02 MAP3_TIMELINE §3d/§7: Poison Mind thay Wicked Child ≈ 1.2 s sau khoá (Map_3 198.9 s, cx 1280 f5931) và ≈ 0.83 s (Boss_Map_3 13.3 s, khoá f374) ⇒ TB 60 (±0.3 s cửa sổ 0.5 s)', 'sau khi camera KHOÁ (trước khi thức)'),
    ('wakeDelayTicks', '120', '119', '121', 'tick', 'đo:cvm3-09 ' + M3 + ' khoá f5931 → bước đầu f5992 (119.5–120.9) + ' + B3 + ' khoá f374 → bước đầu f434/435 (119.5–120.2) — cả 2 con cùng lúc, Simon ở x khác nhau ⇒ theo THỜI GIAN không theo khoảng cách (thay wakeDistancePx 128 ước lượng CVGP 19)', 'đếm từ khi camera khoá'),
    ('decideTicks', '16', '', '', 'tick', 'đo:cvm3-09 ' + M3 + ': lúc Simon đứng yên quay phải trên khối (f6023–6763) Mummy đổi hướng xa ở |dx| 33–47 và tới ở 55–68 — mỗi dải rộng ≈ 12 px = 16 tick × 0.75 ⇒ quyết định mỗi 16 tick (suy luận từ độ rộng dải)', 'nhịp quyết định hướng'),
    ('decideFirstTicks', '8', '', '', 'tick', 'đo:cvm3-09 ' + M3 + ' con trái quay xa ở f5996 (thức + 9 tick, |dx| 10) + ' + B3 + ' con phải quay xa ở f446–448 (thức + 24–28 = lượt thứ 2 khi lượt 1 ở thức + 8 có |dx| 46) ⇒ lưới 16 tick tính từ lúc khoá (khoá + 128 = thức + 8)', 'lượt quyết định đầu sau khi thức'),
    ('nearPx', '46', '44', '48', 'px', 'đo:cvm3-09 ' + M3 + ' 10 lần quay XA khi đang tới (con phải f6235–6731): |dx| 33–47 (TB 39) = (near − 12, near] ⇒ 46', 'Simon nhìn về Mummy và |dx| < near ⇒ đi xa'),
    ('farPx', '56', '54', '58', 'px', 'đo:cvm3-09 ' + M3 + ' 12 lần quay TỚI khi đang lùi (không kể lúc ném): |dx| 55–68 (TB 61) = [far, far + 12) ⇒ 56; ' + B3 + ' con 2 kẹt mép phải f927–1002 ở |dx| 48–53 không quay (giữa near/far)', '|dx| ≥ far ⇒ đi tới'),
    ('animStepTicks', '16', '15', '18', 'tick', 'đo:cvm3-09 ' + M3 + ' f5992–6100 (con phải 8 + 8 + 8 frame) + ' + B3 + ' f435–494: khung 0,1,2,1 mỗi 8 frame = 16 tick, chạy liên tục (cả lúc ném, đi tại chỗ)', 'tick mỗi khung walk'),
    ('throwFirstTicks', '64', '62', '66', 'tick', 'đo:cvm3-09 ' + M3 + ': con phải dừng ném ở f6023.5 = thức + 63 tick đi; ' + B3 + ' con phải dừng f477 = thức + 86 (lệch +22 — f448–454 đứng 14 tick không rõ lý do, §6)', 'con đầu (phải): tick đi trước lần ném đầu'),
    ('throwFirstPartnerTicks', '120', '118', '122', 'tick', 'đo:cvm3-09 ' + M3 + ' con trái dừng ném f6051 (thức + 118) + ' + B3 + ' con trái f494 (thức + 120) — lệch nửa chu kỳ (56) với con phải', 'con 2 (trái)'),
    ('throwEveryTicks', '66', '64', '68', 'tick', 'đo:cvm3-09 2 lần ném liền cùng con không bị khựng: ' + M3 + ' con trái dừng f6051 → f6108.5 (115 tick) + ' + B3 + ' con trái f494 → f551 (114 tick) — trừ tư thế 48 ⇒ 66–67', 'tick đi giữa 2 lần ném (đếm khi Walk)'),
    ('throwChancePct', '50', '', '', '%', 'suy luận: (đo:cvm3-09 khoảng giữa 2 lần ném cùng con) Boss_Map_3 con trái 114 / 111 / 108 tick rồi 226 (≈ 2 × 113) ; con phải 378 (≈ 3 × 126); Map_3 con trái 115 — sau lần ném đầu không phải lượt nào cũng ném (2 video: 9 + 11 băng, ít hơn ~1/3 so với ném mọi lượt) ⇒ mỗi lượt 50 % (phân bố thật chưa rõ — §6); lần ném ĐẦU luôn ném (4/4 đúng hẹn)', 'từ lần ném thứ 2'),
    ('throwRetryTicks', '114', '111', '115', 'tick', 'đo:cvm3-09 bỏ lượt ⇒ lượt kế sau đúng 1 chu kỳ đi + ném (66 + 48 = 114; Boss_Map_3 con trái 226 ≈ 2 × 113)', 'chờ thêm khi bỏ lượt'),
    ('throwPoseTicks', '48', '46', '51', 'tick', 'đo:cvm3-09 5 lần ném (Map_3 f6023.5–6047 / f6051–6076.5 / f6108.5–6133, Boss f477–501 / f494–518): đứng 46–51 tick (TB 49) rồi đi tiếp hướng mặt (về Simon)', 'đứng ném'),
    ('throwReleaseTicks', '40', '38', '43', 'tick', 'đo:cvm3-09 cùng 5 lần: băng sinh 38–43 tick sau khi dừng (khớp sóng: f6043.0 / 6072.6 / 6128.3 / ~496.3 / 514.5) — đúng lúc hết nháy đỏ', 'tick ném trong tư thế'),
    ('flashTicks', '2', '', '', 'tick', 'đo:cvm3-09 màu thân từng frame 5 lần ném (boss9m_model.py §4): đỏ / tím xen kẽ gần như mỗi frame video (2 tick) suốt từ lúc dừng tới lúc ném (32–36 tick) ⇒ đổi mỗi 2 tick (bắt đầu đỏ) — sprite mummy_red_* (CVM3 01: cùng hình, bảng màu đỏ)', 'nhịp nháy đỏ/tím lúc ném'),
    ('bandageAboveHighPx', '31', '30', '32', 'px', 'đo:cvm3-09 12 băng CAO (Map_3 7, Boss 5): tâm sóng y 144.1–146.2 (TB 145.2) khi chân Mummy 176 — sinh đúng ở tâm, đi xuống trước', 'tâm băng lúc sinh = chân − giá trị (cao)'),
    ('bandageAboveLowPx', '15', '13', '16', 'px', 'đo:cvm3-09 6 băng THẤP (Map_3 B2 / B5 / B9 / B10, Boss B2 / B8): tâm 159.9 / 161.3 / 163.0 / 160.9 (TB 161) ⇒ chân − 15; tỉ lệ cao/thấp 12/6 — chọn đều 50 % (suy luận; giả thuyết xen kẽ con trái C-T-C ở 2 video §6)', 'thấp'),
    ('bandageSpeed', '1.5', '1.43', '1.55', 'px/tick', 'đo:cvm3-09 17 băng hồi quy vx 1.43–1.55 (băng dài nhất Map_3 B0 44 điểm −1.474, Boss B1 28 điểm +1.488) — về phía Simon', 'theo hướng mặt lúc ném'),
    ('bandageWaveAmpPx', '24', '23', '25', 'px', 'đo:cvm3-09 sóng chung 17 băng y = c + A·sin(2π t/P): A 24 rms TB 0.645 (23: 0.755, 25: 0.669); Map_3 B0 169 → 121 → 169', 'biên sóng dọc'),
    ('bandageWavePeriodTicks', '102', '100', '103', 'tick', 'đo:cvm3-09 cùng 17 băng: P 100 / 101 / 102 / 103 / 104 rms 0.648 / 0.637 / 0.645 / 0.672 / 0.713; B0 đáy f6056 → đỉnh f6081.5 = 51 tick (nửa chu kỳ)', 'chu kỳ sóng'),
    ('bandageScreenInsetPx', '8', '7', '9', 'px', 'đo:cvm3-09 băng mất ở x 1288–1291 (cx + 8, Map_3 9 băng bay trái) và 1526–1528 (cx + 248, Boss 4 băng bay phải)', 'gỡ khi tâm cách mép màn < giá trị'),
    ('screenMinX', '8', '', '', 'px màn', 'đo:cvm3-09 ' + M3 + ' con trái f6013–6026 đứng ở x 1288 (= cx + 8) khi đang đi trái', 'X Mummy ≥ cx + giá trị'),
    ('screenMaxX', '248', '', '', 'px màn', 'đo:cvm3-09 ' + B3 + ' con 2 f933–1002 kẹt x 1528 (= cx + 248) quay phải, khung anim vẫn chạy', 'X Mummy ≤ cx + giá trị'),
    ('crystalDelayTicks', '117', '116', '119', 'tick', 'đo:cvm3-09 Mummy 2 chết (lửa + 3000) → quả cầu hiện ở (cx + 128, 104): ' + M3 + ' f6770.5 → f6829.5 (118.2) + ' + B3 + ' f1004.5 → ~f1062.5 (116.2 ±3)', 'Mummy Men: cầu hiện sớm hơn Phantom Bat / Medusa (152)'),
    ('crystalHoverTicks', '34', '32', '36', 'tick', 'đo:cvm3-09 quả cầu đứng yên ở y 104 rồi mới rơi (g 0.25): Map_3 f6829.5 → ~f6847 (35) + Boss ~f1062.5 → ~f1079.5 (34) ⇒ bắt đầu rơi ≈ chết + 151 (≈ 152 của Phantom Bat)', 'đứng trước khi rơi'),
]


def rows(actor, spec):
    out = io.StringIO()
    w = csv.writer(out, lineterminator='\n')
    for k, v, mn, mx, unit, src, note in spec:
        w.writerow([actor, k, v, mn, mx, unit, src, note])
    return out.getvalue()


def write_if(p, t, s):
    if t != s:
        open(p, 'w', encoding='utf-8', newline='').write(t)
        print('ghi', p)
    else:
        print('same', p)


def main():
    s = open(AP, encoding='utf-8', newline='').read()
    keep, at = [], None
    for l in s.split('\n'):
        if l.startswith('Boss.MummyMan,'):
            if at is None:
                at = len(keep)
            continue
        keep.append(l)
    keep[at:at] = rows('Boss.MummyMan', MM).rstrip('\n').split('\n')
    write_if(AP, '\n'.join(keep), s)

    e = open(BO, encoding='utf-8', newline='').read()
    rd = list(csv.reader(io.StringIO(e)))
    h = rd[0]
    for r in rd:
        if r and r[0] == 'MummyMan':
            r[h.index('hitsWeak')] = '16'
            r[h.index('hitsStrong')] = '16'
            r[h.index('src_hits')] = ('đo:cvm3-09 ' + B3 + ': roi cấp 3 (dài — Strong) 16 lần ENEMY −1 f452–1005 (16 vạch chung 2 con), con 1 chết sau 9 vạch '
                                      '(f848: 8 đòn + 1 đòn con 2), con 2 sau 7 nữa ⇒ mỗi con 8 đòn; ' + M3 + ' chỉ nước thánh (Weak): 16 lần −1 vạch qua 9 lọ ⇒ lọ/lửa '
                                      'cũng 1 vạch/đòn (như Medusa CVM2 09); wiki 48/32 sai so video (H1)')
            r[h.index('score')] = '3000'
            r[h.index('src_score')] = 'video:Map_3 3:42.87 (f6686) + 3:45.70 (f6771): MỖI con +3000; video:Boss_Map_3 f848 +3100 (= 3000 + 100 băng vỡ cùng nhát) / f1005 +3000 — wiki 3000 không nói rõ'
            r[h.index('moveSpeed')] = '0.75'
            r[h.index('src_moveSpeed')] = 'đo:cvm3-09 4 đoạn đi sạch (Map_3 f5992–6023 / f6027–6051 / f6047–6100, Boss f435–494): hồi quy 0.741–0.767 (TB 0.752) px/tick'
            r[h.index('attacks')] = 'Đứng chờ tới khoá + 120 tick; đi 0.75 giữ cự ly 46–56 px trước mặt Simon (sau lưng thì áp sát); mỗi ~114 tick đứng nháy đỏ 40 tick rồi ném băng gạc sóng (video Map_3 / Boss_Map_3)'
            r[h.index('itemsInRoom')] = 'Pot Roast (khối thấp nhất cầu thang trái — ô (83,9)); nến dao găm (x 1311); nến phải (x 1503 — tim nhỏ ⇒ II khi vũ khí phụ đã trúng ≥ 10 lần)'
    b = io.StringIO()
    csv.writer(b, lineterminator='\n').writerows(rd)
    t = b.getvalue()
    if not e.endswith('\n'):
        t = t.rstrip('\n')
    write_if(BO, t, e)

    d = open(DR, encoding='utf-8', newline='').read()
    o = io.StringIO()
    csv.writer(o, lineterminator='\n').writerow(['MummyMan_X83_9', '9', 'BreakableBlock', 'PotRoast',
                                                 'phòng Mummy THẬT (section MummyMan): khối thấp nhất cầu thang trái — ô (83,9) (CVM3 05 Breakable)',
                                                 'video:Boss_Map_3 7.60 s f228 (khối thành đen, thịt nướng rơi — Simon ăn +6 vạch f302–322); wiki:Stages 7-9#Stage 9 (Pot Roast khối thấp nhất phòng boss)'])
    row = o.getvalue().rstrip('\n')
    lines = d.split('\n')
    lines = [l for l in lines if not l.startswith('MummyMan_X83_9,')]
    at = next(i for i, l in enumerate(lines) if l.startswith('BossMummy_X6_10,')) + 1
    lines[at:at] = [row]
    write_if(DR, '\n'.join(lines), d)


if __name__ == '__main__':
    main()
