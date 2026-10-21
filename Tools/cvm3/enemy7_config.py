"""CVM3 07 — thay khối dòng Enemy.Skeleton.* + Enemy.Fleaman.* trong Assets/Data/Config/Csv/ConfigActorParam.csv bằng số ĐO video Map_3
(CVM3_MEMORY §2g) + sửa moveSpeed Skeleton/Fleaman ở ConfigEnemy.csv. Chạy lại ⇒ y hệt (idempotent). Giữ LF như file gốc.
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy7_config.py   rồi menu CVGP/Import Config.
"""
import csv, io, os

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
AP = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigActorParam.csv')
EN = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigEnemy.csv')

W = 'video:Map_3 0:34.5–0:35.5 WEA f1036–1065 + 1:10.8–1:13.0 WEB f2123–2195 + 1:39.6–1:42.3 WalkwayA f2987–3069'
SK = [
    ('legTicks', '16', '14', '18', 'tick', 'đo:cvm3-07 ' + W + ': 11 chặng đi đều 22–23 px, mỗi chặng 14–18 tick (đồng hồ frame) ⇒ 16 tick × 1.4375 = 23 px', 'lượt đi = legTicks đi + pauseTicks đứng'),
    ('pauseTicks', '4', '2', '6', 'tick', 'đo:cvm3-07 cùng cửa sổ: đứng 3 frame (4.6–5 tick) giữa 2 chặng liền nhau (f2133–2135, f2170–2172, f1045–1047, f1055–1057, f2995–2997, f3039–3041) và sau khi đáp (f1034–1036, f3029–3031)', 'cuối lượt đi + lúc mới vào (sinh / đáp)'),
    ('keepPx', '72', '70', '76', 'px', 'đo:cvm3-07 video:Map_3 24 lượt có Simon: |dx| < 32 ⇒ lùi 3/3; 33–70 ⇒ lùi 4 / ném 3; 76–84 ⇒ tiến 5 / ném 1; ≥ 96 ⇒ tiến 7 / ném 3 (lùi ở 70 f3041, tiến ở 76 f2190) ⇒ ngưỡng giữa 70 và 76', '|Simon − X| < keepPx ⇒ lùi, không thì tiến (khi không ném)'),
    ('throwChancePct', '25', '20', '40', '%', 'đo:cvm3-07 video:Map_3: 7 lượt ném / 24 lượt quan sát (29 %) — 6 con (WEA, WEB ×2, WalkwayA, MummyMan ×2); chọn 1/4 (suy luận phân bố)', 'mỗi lượt: Roll(0..99) < pct ⇒ đứng ném 1 xương'),
    ('throwTicks', '18', '16', '20', 'tick', 'đo:cvm3-07 video:Map_3: đứng giữa 2 chặng khi ném 1 xương 20 / 21.4 / 23 tick (WEA f1065–1075.5, WEB f2180–2190.7, WalkwayA f3049–3060.5), 2 xương liền 40 tick (WEB f2142–2162) = pauseTicks 4 + 18 × số xương; xương thứ 2 sinh 26 tick sau khi đứng (4 + 18 + 4)', 'lượt ném: đứng yên, KHÔNG có pauseTicks riêng'),
    ('throwAtTick', '4', '2', '6', 'tick', 'đo:cvm3-07 video:Map_3: xương sinh 6.8–8 tick sau khi đứng lại (f1068.4, f2146.0, f2183.8, f3052.9 — ngoại suy x xương về X Skeleton) = pauseTicks 4 + 3–4; xương thứ 2 liền f2155.0 = +26', 'tick trong lượt ném'),
    ('jumpVy0', '-2.5', '-2.625', '-2.375', 'px/tick', 'đo:cvm3-07 video:Map_3 4 cú nhảy ở mép (WEB 0:50.9 f1527 / 1:13.2 f2196; MummyMan 2:51.2 f5137 / 2:53.0 f5191): đỉnh 23.6–24.1 px, 40–43 tick trên sàn phẳng; khớp lưới (vy0, g, giữ đỉnh) ⇒ −2.5 / 0.125 / 0 rms 1.22 px (tốt nhất −2.625 / 0.1328 rms 1.12)', 'âm = lên; vy += g rồi dời (đỉnh 23.75 px sau 20 tick)'),
    ('jumpVx', '1.25', '1.20', '1.30', 'px/tick', 'đo:cvm3-07 cùng 4 cú + WEA 0:34.0 f1020 / WalkwayA 1:40.0 f3000: hồi quy 1.20–1.30 (TB 1.244); 50–53 px trên 40 tick', 'theo chiều đang đi; trên không KHÔNG chặn tường'),
    ('gravity', '0.125', '0.10', '0.135', 'px/tick²', 'đo:cvm3-07 cùng 4 cú (xem jumpVy0)', ''),
    ('boneVx', '0.75', '0.725', '0.778', 'px/tick', 'đo:cvm3-07 video:Map_3 7 xương (WEA 0:35.7 f1072; WEB 0:50.8 f1523, 1:11.6 f2150 / f2156, 1:12.8 f2185; WalkwayA 1:41.8 f3055; MummyMan 2:51.0 f5127–5168): hồi quy 0.725–0.778 (rms 0.35–0.66 px), mọi xương cùng tốc — về phía Simon', 'theo hướng mặt'),
    ('boneVy0', '-3.5', '-3.6', '-3.3', 'px/tick', 'đo:cvm3-07 cùng 7 xương: 2 ĐỘ CAO — đỉnh 61.5–62.6 px trên pivot vẽ Skeleton (4 xương) và 90.8–93.0 (3 xương); g 0.125 + sinh ở Y − 13 ⇒ vy0 −3.5 / −4.5 (đỉnh 61.25 / 92.75)', 'xương thấp; âm = lên'),
    ('boneVyStep', '1.0', '', '', 'px/tick', 'đo:cvm3-07 cùng 7 xương: xương cao vy0 −4.5 = −3.5 − 1', 'vy0 = boneVy0 − k·step'),
    ('boneKindSpan', '1', '', '', 'loại', 'đo:cvm3-07 cùng 7 xương: chỉ 2 độ cao (4 thấp / 3 cao, không theo khoảng cách — f2146 thấp và f2155 cao cùng |dx| 51) ⇒ k ngẫu nhiên 0..1; wiki "các đường khác nhau"', ''),
    ('boneGravity', '0.125', '0.1216', '0.1283', 'px/tick²', 'đo:cvm3-07 cùng 7 xương: khớp bậc 2 tự do g 0.1216–0.1283 ở 5 vết dài (rms 0.47–1.37 px)', ''),
    ('boneHandAbovePx', '13', '10', '18', 'px', 'đo:cvm3-07 cùng 7 xương: ngoại suy x xương về X Skeleton cho tâm lúc sinh 10.5–18.8 px trên pivot vẽ (TB 15.0 = Y − 14); đỉnh 2 loại cố định ⇒ chọn 13 (đỉnh 61.25 / 92.75 ↔ video 62.0 / 92.0, sai ±0.75 px)', 'tâm xương lúc sinh = (X, Y − 13)'),
]
F3 = 'video:Map_3 0:16.4–0:30.6 WEA (3 con: bục (433,128) f491–543, (305,128) f621–673, (177,96) f825–916)'
FL = [
    ('triggerDistancePx', '128', '', '', 'px', 'đo:cvm3-07 ' + F3 + ': cả 3 bật sau khi |Simon − Fleaman| < 128 (Simon tới từ phải); bật ở |dx| 100 / 100 / 120 khi Simon đứng; không bật ngay khi |dx| 95 (f838) ⇒ không phải ngưỡng tức thì 112', '|Simon.X − X| < ⇒ bắt đầu đếm'),
    ('triggerDelayMinTicks', '62', '', '', 'tick', 'đo:cvm3-07 cùng 3 con: từ tick đầu |dx| < 128 (vết Simon video từng tick — ref/data/enemy7/puppet_fl_wea*.csv) tới lúc ngồi thấp = 61.9 / 62.4 / 135.7 tick (±2) — 2 mẫu đúng 62', 'đếm = Roll(min, min + span)'),
    ('triggerDelaySpanTicks', '74', '', '', 'tick', 'đo:cvm3-07 cùng 3 con: 136 − 62 (suy luận phân bố đều — 3 mẫu; có thể là kiểm định kỳ ~74 tick — §4)', ''),
    ('crouchTicks', '13', '12', '14', 'tick', 'đo:cvm3-07 cùng 3 con: đang đổi khung 0↔1 mỗi 9 tick thì giữ fleaman_0 12.4 / 12.8 / 14.2 tick rồi bật (f518→524.4, f647→654.1, f889→895.2)', 'trước cú đầu'),
    ('shortVx', '2.25', '2.125', '2.375', 'px/tick', 'đo:cvm3-07 cùng 3 con: cú đầu từ bục xuống sàn 81 / 81 / 94 px; khớp lưới (vx, vy0, g) ⇒ 2.25 / −1.0 / 0.125 rms 2.22 px (kế 2.375 / −1 / 0.1406 rms 2.39)', 'về phía Simon lúc bật'),
    ('shortVy0', '-1.0', '-1.25', '-0.75', 'px/tick', 'đo:cvm3-07 cùng 3 con: lên 4 px rồi rơi (đỉnh 3.5 px với g 0.125)', 'âm = lên'),
    ('longVx', '2.25', '', '', 'px/tick', 'suy luận: cùng vx đo của cú ngắn (cvm3-07) — video Map_3 không có cú vọt (cả 3 con bị hạ ngay khi đáp cú đầu); CVGP 17 ước lượng 2.0', ''),
    ('longVy0', '-3.25', '', '', 'px/tick', 'suy luận: giữ đỉnh ≈ 40 px của ước lượng CVGP 17 với g đo 0.125 (cvm3-07) — video Map_3 không có cú vọt', 'âm = lên; đỉnh 40.6 px, 52 tick'),
    ('gravity', '0.125', '0.109', '0.141', 'px/tick²', 'đo:cvm3-07 cùng 3 con (xem shortVx) — rơi 48 / 48 / 80 px không thấy trần tốc', ''),
    ('longEvery', '3', '', '', 'lần', 'wiki:Enemies#Fleamen "hai bước nhảy ngắn rồi một cú nhảy vọt" — video Map_3 chỉ thấy cú đầu (ngắn, khớp)', 'cú thứ 3 của mỗi chu kỳ là nhảy vọt'),
    ('pauseMinTicks', '4', '', '', 'tick', 'suy luận: ước lượng (không video) — routine 17; video Map_3 không có cú thứ 2 (bị hạ ngay khi đáp)', 'ngẫu nhiên đều [4, 16]'),
    ('pauseSpanTicks', '12', '', '', 'tick', 'suy luận: ước lượng (không video) — routine 17; cùng lý do', ''),
]


def rows(actor, spec):
    out = io.StringIO()
    w = csv.writer(out, lineterminator='\n')
    for k, v, mn, mx, unit, src, note in spec:
        w.writerow([actor, k, v, mn, mx, unit, src, note])
    return out.getvalue()


def main():
    s = open(AP, encoding='utf-8', newline='').read()
    lines = s.split('\n')
    keep, block_at = [], None
    for i, l in enumerate(lines):
        if l.startswith('Enemy.Skeleton,') or l.startswith('Enemy.Fleaman,'):
            if block_at is None:
                block_at = len(keep)
            continue
        keep.append(l)
    new = (rows('Enemy.Skeleton', SK) + rows('Enemy.Fleaman', FL)).rstrip('\n').split('\n')
    keep[block_at:block_at] = new
    t = '\n'.join(keep)
    if t != s:
        open(AP, 'w', encoding='utf-8', newline='').write(t)
        print('ghi', AP, len(new), 'dong')
    else:
        print('same', AP)
    e = open(EN, encoding='utf-8', newline='').read()
    out = []
    rd = list(csv.reader(io.StringIO(e)))
    hdr = rd[0]
    i_ms, i_src = hdr.index('moveSpeed'), hdr.index('src_moveSpeed')
    for r in rd:
        if r and r[0] == 'Skeleton':
            r[i_ms] = '1.4375'
            r[i_src] = 'đo:cvm3-07 ' + W + ': 11 chặng 22–23 px, hồi quy 1.38–1.42 px/tick (TB 1.416), đồng hồ Simon (1 px/tick) f2125–2131 / f2174–2178 cho 1.42–1.44 ⇒ 23 px / 16 tick'
        if r and r[0] == 'Fleaman':
            r[i_ms] = '2.25'
            r[i_src] = 'đo:cvm3-07 ' + F3 + ': vx cú nhảy 2.25 px/tick (= Enemy.Fleaman.shortVx; code dùng ConfigActorParam)'
        out.append(r)
    b = io.StringIO()
    csv.writer(b, lineterminator='\n').writerows(out)
    t = b.getvalue()
    if not e.endswith('\n'):
        t = t.rstrip('\n')
    if t != e:
        open(EN, 'w', encoding='utf-8', newline='').write(t)
        print('ghi', EN)
    else:
        print('same', EN)


if __name__ == '__main__':
    main()
