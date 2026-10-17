"""CVM2 09 — ghép cặp ảnh video (lưới NES, bộ đệm map2_nes.npy / boss2_nes.npy) ↔ ảnh Play Unity (Map2BossProbe, Game view 1920×1080:
NES (x, y) ↔ màn (448 + 4x, 60 + 4y)) — vùng phòng boss NES x 0–255, y 96–207, phóng 3×.

Chạy (từ gốc repo): python -P -E Tools/cvm2/boss9_pairs.py [shotDir] [outDir]
  → <outDir>/pairs_<n>.png (4 cặp/ảnh) + pairs.txt
"""
import os, sys
import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
SHOT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '09', 'unity')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', 'shots', '09')
Y0, Y1, S = 96, 208, 3

PAIRS = [
    ('map2', 6824, 'wake_m2_r001', 'tượng nguyên vẹn (trước thức)'),
    ('map2', 6826, 'wake_m2_r003', 'thức: đầu tượng xoá, Medusa xanh ở đỉnh tượng'),
    ('map2', 6830, 'wake_m2_r010', 'nháy tím (medusa_stone)'),
    ('map2', 6836, 'wake_m2_r022', 'xanh, rắn 1 sắp chạm sàn'),
    ('boss2', 74, 'wake_b2_r013', 'bay nhào xuống (chuyển động)'),
    ('boss2', 86, 'wake_b2_r037', 'đáy sóng nhào'),
    ('boss2', 99, 'wake_b2_r062', 'tới chỗ dừng 1 (57,145)'),
    ('boss2', 110, 'wake_b2_r084', 'đang dừng, rắn 1 trườn trái'),
    ('boss2', 132, 'wake_b2_r128', 'rời dừng 1: rắn 2 thả'),
    ('boss2', 141, 'wake_b2_r146', 'rắn 2 chạm sàn'),
    ('boss2', 331, 'seg_de_r016', 'đoạn D 16 tick → dừng 3'),
    ('boss2', 363, 'seg_de_r080', 'rời dừng 3: rắn 4 thả, rắn 3 trườn'),
    ('boss2', 378, 'seg_de_r110', 'đoạn E đáy (sàn − 8)'),
    ('boss2', 386, 'seg_de_r130', 'đoạn E đi lên (video sắp vào lửa)'),
    ('map2', 7072, 'death_r007', 'lửa chết (mảnh)'),
    ('map2', 7080, 'death_r023', 'lửa chết (lùn)'),
    ('map2', 7089, 'death_r041', 'lửa chết (to)'),
    ('map2', 7150, 'death_r162', 'quả cầu rơi'),
    ('map2', 7160, 'death_r190', 'quả cầu nằm sàn'),
]


def vid(video, f, cache={}):
    if video not in cache:
        cache[video] = np.load(os.path.join(WORK, video + '_nes.npy'), mmap_mode='r')
    a = np.asarray(cache[video][f]).astype(np.uint8)        # hàng NES 8..231
    full = np.zeros((240, 256, 3), np.uint8)
    full[8:232] = a
    return full


def uni(name, shot):
    im = np.asarray(Image.open(os.path.join(shot, name + '.png')).convert('RGB'))
    ys = 60 + 4 * np.arange(240) + 2
    xs = 448 + 4 * np.arange(256) + 2
    return im[ys][:, xs]


def main():
    shot = sys.argv[1] if len(sys.argv) > 1 else SHOT
    out = sys.argv[2] if len(sys.argv) > 2 else OUT
    os.makedirs(out, exist_ok=True)
    W, H = 256 * S, (Y1 - Y0) * S
    txt = []
    for g in range(0, len(PAIRS), 4):
        grp = PAIRS[g:g + 4]
        img = Image.new('RGB', (2 * W + 12, len(grp) * (H + 18)), (40, 40, 40))
        d = ImageDraw.Draw(img)
        for i, (video, f, name, note) in enumerate(grp):
            p = os.path.join(shot, name + '.png')
            if not os.path.exists(p):
                txt.append('THIẾU ' + name)
                continue
            a = Image.fromarray(vid(video, f)[Y0:Y1]).resize((W, H), Image.NEAREST)
            b = Image.fromarray(uni(name, shot)[Y0:Y1]).resize((W, H), Image.NEAREST)
            y = i * (H + 18)
            img.paste(a, (0, y + 16))
            img.paste(b, (W + 12, y + 16))
            lab = '%s f%d  <->  Unity %s : %s' % ('Map_2' if video == 'map2' else 'Boss_Map_2', f, name, note)
            d.text((4, y + 2), lab.encode('ascii', 'replace').decode(), fill=(255, 255, 0))
            txt.append('pairs_%d #%d: %s' % (g // 4, i, lab))
        img.save(os.path.join(out, 'pairs_%d.png' % (g // 4)))
    open(os.path.join(out, 'pairs.txt'), 'w', encoding='utf-8').write('\n'.join(txt) + '\n')
    print('ghi', (len(PAIRS) + 3) // 4, 'anh')


if __name__ == '__main__':
    main()
