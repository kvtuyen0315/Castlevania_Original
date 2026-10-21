"""CVM3 08 — thay khối dòng Enemy.Raven.* trong Assets/Data/Config/Csv/ConfigActorParam.csv bằng số ĐO video Map_3 (CVM3_MEMORY §2h)
+ sửa moveSpeed Raven ở ConfigEnemy.csv (= vx lướt). Chạy lại ⇒ y hệt (idempotent). Giữ LF như file gốc.
Chạy (từ gốc repo): python -P -E Tools/cvm3/enemy8b_config.py   rồi menu CVGP/Import Config.
Số được chốt bằng mô phỏng đúng logic Unity Tools/cvm3/enemy8b_raven_model.py 9 8 32 48 25 14 60 (rms 1.70 px / 412 điểm, 8 Raven).
"""
import csv, io, os

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
AP = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigActorParam.csv')
EN = os.path.join(ROOT, 'Assets', 'Data', 'Config', 'Csv', 'ConfigEnemy.csv')

V = 'video:Map_3 12 Raven (WEB 0:44.7–0:57.7 f1340–1732 ×4, WalkwayB 1:59.8–2:11.8 f3594–3955 ×3, MummyMan 2:35.6–2:50.7 f4668–5121 ×5)'
L = 'video:Map_3 9 chặng lướt (f1370–1394, f1604–1622, f1664–1680, f1683–1701, f1713–1728, f3624–3642, f3896–3914, f3928–3955, f5117–5121)'
D = 'video:Map_3 Raven #0 WEB f1407–1490 (2 cú lao ngang, 1 lần phanh quay đầu)'
RV = [
    ('perchTicks', '60', '58', '62', 'tick', 'đo:cvm3-08 ' + V + ': cất cánh 29–31 frame (58–62 tick, TB 60.3) sau khi HIỆN ở mép phải màn (x màn 246–248 = khung hiện vật cx+248) — 9/9 lần, kể cả khi camera đứng (#3 cx 280–296, #6 cx 303–304); |dx| Simon lúc cất cánh 55–117 ⇒ KHÔNG theo khoảng cách (thay triggerDistancePx 96 ước lượng CVGP 17)', 'đậu tới ageTicks ≥ perchTicks (tính từ lúc sinh)'),
    ('glideVy0Low', '1.25', '1.18', '1.30', 'px/tick', 'đo:cvm3-08 ' + L + ': vy0 khớp parabol 1.18–2.10 rơi vào 3 mức — mô phỏng chọn từng chặng: 1.25 ×3, 1.75 ×3, 2.0 ×3 (1.5 không thấy lần nào)', 'mức thấp'),
    ('glideVy0Mid', '1.75', '1.61', '1.76', 'px/tick', 'đo:cvm3-08 cùng 9 chặng (xem glideVy0Low)', 'mức giữa'),
    ('glideVy0High', '2.0', '1.95', '2.10', 'px/tick', 'đo:cvm3-08 cùng 9 chặng (xem glideVy0Low); chọn 1 trong 3 mức bằng Roll đều (suy luận phân bố — 3/3/3)', 'mức cao'),
    ('glideDecel', '0.03515625', '0.030', '0.047', 'px/tick²', 'đo:cvm3-08 cùng 9 chặng: |vy| giảm đều tới 0 rồi đứng — 2 chặng trọn (#0 f1370–1394, #3 f1683–1701, đồng hồ x 0.75 px/tick) cho 0.0345 / 0.0340; khớp tự do 0.030–0.047 ⇒ 9/256', 'mỗi tick sau khi dời: |vy| −= decel (không đổi dấu); vy = 0 ⇒ treo'),
    ('targetAboveFeetPx', '16', '7', '39', 'px', 'đo:cvm3-08 cùng 9 chặng: chiều vy0 = về phía TÂM THÂN Simon 9/9 (Raven tâm y 121 với chân Simon 128 bay LÊN; y 89 với chân 128 bay XUỐNG ⇒ ngưỡng giữa chân − 7 và chân − 39 ⇒ tâm thân 16); sau treo: cao hơn tâm ⇒ lao ngang (#0), thấp hơn ⇒ lướt tiếp (#3)', 'tâm thân Simon = Y − 16'),
    ('hoverTicks', '25', '24', '26', 'tick', 'đo:cvm3-08 video:Map_3 2 lần treo sau chặng lướt: #0 f1394–1407 (26 tick), #3 f1701–1713 (24 tick); x và y đứng yên, cánh vẫn vỗ', 'treo sau chặng lướt'),
    ('turnHoverTicks', '14', '12', '16', 'tick', 'đo:cvm3-08 ' + D + ': phanh xong đứng f1440–1447 (14 tick) rồi lao lại — wiki "quay đầu rất nhanh"', 'treo sau khi phanh'),
    ('dashVx0', '0.125', '0', '0.25', 'px/tick', 'đo:cvm3-08 ' + D + ': khớp mô phỏng (v0, gia tốc) 2 cú lao ⇒ v0 0.125–0.25 (cú 2 0.125)', 'đầu cú lao ngang'),
    ('dashAccel', '0.03125', '0.029', '0.032', 'px/tick²', 'đo:cvm3-08 ' + D + ': x bậc 2 cú lao 1 0.0293 / cú 2 0.0321; mô phỏng tốt nhất 0.032 ⇒ 8/256; y giữ nguyên (83) suốt 2 cú', 'mỗi tick |vx| += accel'),
    ('dashMaxVx', '3.0', '2.5', '', 'px/tick', 'suy luận: video cú lao 2 tới 2.5–3 px/tick lúc ra mép màn (f1478–1490) không thấy trần ⇒ trần 3.0 chỉ để chặn', 'trần |vx| khi lao'),
    ('brakeDecel', '0.1875', '0.125', '0.25', 'px/tick²', 'đo:cvm3-08 ' + D + ': vượt X Simon (~180) ở f1435 với ~2 px/tick ⇒ dừng sau ~9 px (f1436–1440); mô phỏng phanh 48/256 rms 2.63 px (32: 3.68, 64: 3.42)', 'mỗi tick |vx| −= brake tới 0'),
    ('dashTurns', '1', '', '', 'lần', 'đo:cvm3-08 ' + D + ': vượt Simon lần 1 (f1435) ⇒ phanh + lao lại; vượt lần 2 (f1469, Simon x ~214) ⇒ KHÔNG quay, tăng tốc ra khỏi màn f1490 (wiki "có thể lao lần nữa trước khi bay đi") — 1 mẫu', 'số lần quay đầu tối đa'),
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
        if l.startswith('Enemy.Raven,'):
            if block_at is None:
                block_at = len(keep)
            continue
        keep.append(l)
    new = rows('Enemy.Raven', RV).rstrip('\n').split('\n')
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
    i_beh = hdr.index('behaviorShort')
    for r in rd:
        if r and r[0] == 'Raven':
            r[i_ms] = '0.75'
            r[i_src] = 'đo:cvm3-08 ' + L + ': vx đều 0.716–0.792 (TB 0.746) suốt chặng lướt, không gia tốc ⇒ vx lướt (code dùng làm glideVx)'
            r[i_beh] = 'Đậu 60 tick sau khi hiện rồi lướt chéo về phía Simon (vy giảm dần), treo, lao ngang tăng tốc; vượt qua thì phanh quay lại 1 lần rồi bay đi (video Map_3)'
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
