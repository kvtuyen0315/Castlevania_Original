# events_annot2.py (CVM2 02) - ghi chú nghĩa từng sự kiện HUD Map_2 (xem bằng mắt từng bộ ba frame, shots/ev/ev_*.png) -> work/map2_events.txt
# Chạy: python -P -E Tools/cvm2/events_annot2.py   (cần map2_events_raw.txt + map2_pickups.txt)
import os, re, sys
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
TK = 60.0988 / 30
# nghĩa theo frame (đã xem bằng mắt triplet 3 khung quanh mỗi sự kiện)
MEAN = {
 251: 'Spear Knight (TEA, tầng trên) chết vì RÌU ném (hearts 05->04 f207, 04->03 f239 = 2 rìu)',
 593: 'nhặt VƯƠNG MIỆN (crown) dưới chân thang TEA (mọc từ sàn từ 0:17.0 sau khi phá tường phải TEA)',
 734: 'túi tiền ĐỎ (rơi từ nến trên cao) +100',
 778: 'THẬP TỰ (boomerang) ném trúng DƠI TREO (oval hồng, TEA/TEB tường trái) +200',
 1040: 'THẬP TỰ trúng DƠI TREO #2 (TEB) +200',
 1082: 'túi tiền ĐỎ +100',
 1331: 'Spear Knight (TEB) chết vì ROI +400',
 1375: 'túi tiền TRẮNG +700 (rơi từ nến trên cao)',
 1506: 'túi tiền ĐỎ +100 (cạnh bệ di động TEB)',
 1872: 'Medusa Head (Hall A) chết vì thập tự +300',
 2056: 'Medusa Head chết vì ROI +300',
 2082: 'túi tiền ĐỎ +100',
 2283: 'túi tiền ĐỎ +100',
 2441: 'RƯƠNG BÁU (treasure chest) +2000, đồng thời P 03->04 (qua 30000 điểm = +1 mạng)',
 2934: 'Medusa Head (Hall B) chết vì roi +300',
 2983: 'Medusa Head chết vì thập tự +300',
 3235: 'Spear Knight (Hall B) chết vì roi +400',
 3303: 'túi tiền ĐỎ +100',
 3420: 'túi tiền TRẮNG +700',
 3507: 'Spear Knight chết vì roi +400',
 3615: 'Spear Knight chết vì roi +400',
 3728: 'túi tiền ĐỎ +100',
 3830: 'túi tiền ĐỎ +100',
 4188: 'Medusa Head (Hall B) chết vì roi +300',
 4853: 'túi tiền TÍM +400 (nến Hall C)',
 4922: 'túi tiền ĐỎ +100',
 4965: 'túi tiền ĐỎ +100',
 5022: 'Ghost (Hall C) chết vì roi +300',
 5148: 'túi tiền TRẮNG +700',
 5501: 'Bone Tower chết vì roi +400',
 5653: 'Bone Tower #2 chết vì roi +400',
 5865: 'Ghost (Hall C tầng trên) chết vì roi +300',
 6227: 'Medusa Head (sảnh Medusa) chết vì roi +300',
 6846: 'rắn của Medusa (xanh) chết vì roi +100 (trong lúc đồng hồ đang dừng)',
 7069: 'Queen Medusa chết +3000 (thanh E về 0 ở f7073)',
}
def meaning(f, ch):
    if f in MEAN: return MEAN[f]
    if 'stage:06->07' in ch: return 'HUD STAGE 07 + tim 00→05 (bản đồ lâu đài Block 2 hiện; tim đặt lại 5)'
    m = re.search(r'hearts:(\d+)->(\d+)', ch)
    if m:
        d = int(m.group(2)) - int(m.group(1))
        return {1: 'nhặt tim NHỎ +1', 5: 'nhặt tim LỚN +5', -1: 'dùng vũ khí phụ -1 tim', -5: 'dùng ĐỒNG HỒ (stopwatch) -5 tim'}.get(d, 'tim %+d' % d)
    if 'sub:axe->cross' in ch: return 'nhặt THẬP TỰ (boomerang), mất II (nhân đôi); thập tự rơi từ nến trong TEA'
    if 'sub:cross->watch' in ch: return 'nhặt ĐỒNG HỒ (stopwatch) — KHÔNG phải nước thánh (planner ghi nhầm)'
    if 'ebar:0->16' in ch: return 'bắt đầu Stage 04 (sau màn đen): ENEMY 16 vạch; TIME 0400'
    if 'ebar:16->' in ch or 'ebar:' in ch: return 'thanh ENEMY (Medusa) -1 vạch'
    if 'stage:' in ch: return 'HUD đổi STAGE (cuối đoạn cuộn thứ 2 của cửa trái, ~1.5 frame sau khi camera dừng)'
    if 'p:' in ch: return 'đổi số mạng P'
    if 'sub:cross->occ' in ch or 'sub:occ->cross' in ch: return '(nhiễu) sprite thập tự đang bay đè lên ô vũ khí phụ — KHÔNG đổi vũ khí'
    return ''
out = ['# map2_events — mỗi lần HUD đổi (trừ TIME) + nghĩa. f = frame video (30 fps), tick ≈ f × %.4f (sai số mốc đơn ±2–3 tick: nhịp capture)' % TK, '# cột: frame | m:ss.ss | tick | TIME | thay đổi | vị trí Simon lúc đó | nghĩa']
simon = {}
for l in open(os.path.join(WORK, 'map2_pickups.txt'), encoding='utf-8'):
    m = re.match(r'f(\d+) .*?\| Simon (.*?)( \|\||$)', l.strip())
    if m: simon[int(m.group(1))] = m.group(2)
for l in open(os.path.join(WORK, 'map2_events_raw.txt'), encoding='utf-8'):
    m = re.match(r'f\s*(\d+) (\S+) TIME (\S+) \| (.*)', l.strip())
    if not m: continue
    f = int(m.group(1)); ch = m.group(4)
    if f >= 7380 and f < 7530 and 'score' in ch and 'hearts' in ch:   # tally tim: gom lại ở dưới
        continue
    out.append('f%-5d %s tick %-6d TIME %s | %s | Simon %s | %s' % (f, m.group(2), round(f * TK), m.group(3), ch, simon.get(f, '-'), meaning(f, ch)))
out.append('# tally (f7380–7526): mỗi 4 frame video ≈ 8 tick: tim -1, SCORE +100 (30 tim → +3000); xem map2_events_raw.txt')
open(os.path.join(WORK, 'map2_events.txt'), 'w', encoding='utf-8').write('\n'.join(out))
print(len(out), 'dong')
