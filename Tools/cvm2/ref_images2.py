# ref_images2.py (CVM2 02) - xuất ảnh tham chiếu lưới NES 256x240 (hàng NES 8..231 từ bộ đệm hiệu chuẩn mới; hàng ngoài khung = đen) vào ClaudeNotes/cvm2/ref/nes/
# Chạy: python -P -E Tools/cvm2/ref_images2.py   (cần _cache/02/work/<video>_nes.npy)
import os, sys
import numpy as np
from PIL import Image
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
WORK = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', '_cache', '02', 'work')
OUT = os.path.join(ROOT, 'ClaudeNotes', 'cvm2', 'ref', 'nes')
REFS = [  # (video, frame, tên)
    ('map2', 192, 'm2_00m06_4s_stage04_bat_dau_tea_cx0'),
    ('map2', 520, 'm2_00m17_3s_vuong_mien_moc_tu_san_tea'),
    ('map2', 1620, 'm2_00m54_0s_cua_trai_cua_hien_ra_simon_di_vao'),
    ('map2', 2434, 'm2_01m21_1s_ruong_bau_moc_len_hall_a'),
    ('map2', 5200, 'm2_02m53_3s_vat_pham_iii_hall_c'),
    ('map2', 5676, 'm2_03m09_2s_rosary_nhay_man_hall_c'),
    ('map2', 6780, 'm2_03m46_0s_phong_boss_khoa_cx0'),
    ('map2', 6855, 'm2_03m48_5s_medusa_dong_bang_dong_ho'),
    ('map2', 7641, 'm2_04m14_7s_ban_do_lau_dai_block2_truot_vao'),
    ('map2', 7806, 'm2_04m20_2s_stage07_time0500_frame_dau'),
    ('boss2', 68, 'b2_02_3s_medusa_thuc_dau_xanh_tren_tuong'),
    ('boss2', 120, 'b2_04_0s_simon_trung_don_hurt'),
]
for v, f, name in REFS:
    a = np.asarray(np.load(os.path.join(WORK, v + '_nes.npy'), mmap_mode='r')[f])
    canvas = np.zeros((240, 256, 3), np.uint8); canvas[8:232] = a
    Image.fromarray(canvas).save(os.path.join(OUT, name + '_nes.png'))
    print(name)
