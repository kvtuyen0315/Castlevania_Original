# Ghep cap anh video (luoi NES) <-> Unity (Game view 1920x1080, NES 4x tai (448,60)) - cvm2-04
import sys
sys.path.insert(0, 'Tools/cvm2')
from holy4_sheets import frames, nes
from PIL import Image, ImageDraw
SH = 'ClaudeNotes/cvm2/_cache/shots/04/'
PAIRS = [
 ('boss1', 471, 'bat_a_vao_r027.png', 'lo cham than doi - ENEMY 16->15'),
 ('boss1', 476, 'bat_b_giua_r033.png', 'lo trong than doi (bi che) - doi dung yen'),
 ('boss1', 481, 'enemy_c_ra_r041.png', 'lo ra khoi than van bay (chua vo)'),
 ('boss1', 484, 'bat_d_lua_r049.png', 'lo vo thanh lua o san phia sau'),
 ('boss2', 390, 'medusa_a_vao_r155.png', 'lo vao than Medusa'),
 ('boss2', 393, 'medusa_b_giua_r161.png', 'lo trong mat Medusa - ENEMY -1'),
 ('boss2', 402, 'medusa_d_lua_r177.png', 'lo vo thanh lua o san phia sau Medusa'),
]
S = 3
rows = []
for v, f, u, txt in PAIRS:
    a = Image.fromarray(nes(frames(v, f, 1)[0], v)).resize((256 * S, 240 * S), Image.NEAREST)
    b = Image.open(SH + u).convert('RGB').crop((448, 60, 1472, 1020)).resize((256 * S, 240 * S), Image.NEAREST)
    row = Image.new('RGB', (256 * S * 2 + 8, 240 * S + 22), (40, 40, 40))
    row.paste(a, (0, 22)); row.paste(b, (256 * S + 8, 22))
    d = ImageDraw.Draw(row)
    d.text((4, 4), 'VIDEO %s f%d' % (v, f), fill=(255, 255, 0)); d.text((256 * S + 12, 4), 'UNITY ' + u + '  | ' + txt, fill=(0, 255, 255))
    rows.append(row)
for k in range(0, len(rows), 2):
    part = rows[k:k + 2]
    W = part[0].width; H = sum(r.height for r in part)
    out = Image.new('RGB', (W, H)); y = 0
    for r in part:
        out.paste(r, (0, y)); y += r.height
    out.save(SH + 'pairs_%d.png' % (k // 2)); print(SH + 'pairs_%d.png' % (k // 2))
