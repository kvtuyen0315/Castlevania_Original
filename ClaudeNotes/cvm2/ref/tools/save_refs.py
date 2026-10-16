# save_refs.py - luu frame tham chieu: full frame goc (_cache/video/ref/) + lay mau luoi NES 256x240 (ref/nes/, hang ngoai khung = den)
# Chay tu ClaudeNotes/cvm2: python -P -E _cache/tools/save_refs.py
import sys, os
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__))))
from nes2 import *
CAL = {'map2': dict(ox=268.932, oy=-20.244, sx=3.623, sy=3.104), 'boss2': dict(ox=274.328, oy=-22.952, sx=3.609, sy=3.157)}
REFS = [
    ('map2', 2.0, 'ban_do_lau_dai_vao_stage04'), ('map2', 6.0, 'stage04_bat_dau_tower_entrance_a'),
    ('map2', 8.0, 'nen_dau_tien_thap_tu'), ('map2', 20.0, 'leo_thang_tea_vuong_mien'),
    ('map2', 30.0, 'tower_entrance_b_cx256'), ('map2', 46.0, 'be_di_dong_teb'),
    ('map2', 52.2, 'cua_trai_bat_dau_cuon'), ('map2', 54.2, 'cua_trai_mo'),
    ('map2', 58.0, 'stage05_tower_hall_a_cx512'), ('map2', 62.0, 'medusa_head_hall_a'),
    ('map2', 80.0, 'ruong_bau_vat_bi_mat'), ('map2', 92.0, 'thang_len_hall_a_sang_b'),
    ('map2', 104.0, 'hall_b_be_di_dong'), ('map2', 116.0, 'hall_b_thit_nuong'),
    ('map2', 144.2, 'cua_trai_stage05_sang_06'), ('map2', 150.0, 'stage06_tower_hall_c_cx512'),
    ('map2', 158.0, 'bay_ep_gai_hall_c'), ('map2', 164.0, 'bone_tower_hall_c'),
    ('map2', 166.0, 'ghost_hall_c'), ('map2', 170.0, 'vat_pham_iii_hall_c'),
    ('map2', 196.0, 'thang_len_sanh_medusa'), ('map2', 198.0, 'sanh_medusa_cx512'),
    ('map2', 222.0, 'phong_boss_tuong_medusa'), ('map2', 228.0, 'medusa_tran_dau'),
    ('map2', 236.0, 'medusa_chet_3000'), ('map2', 238.0, 'qua_cau_do'),
    ('map2', 242.0, 'tally_cong_diem'), ('map2', 256.0, 'ban_do_lau_dai_stage07'),
    ('boss2', 0.0, 'tuong_medusa_simon_vao'), ('boss2', 2.5, 'medusa_thoat_tuong'),
    ('boss2', 3.5, 'ran_tren_san'), ('boss2', 6.0, 'lua_nuoc_thanh'),
    ('boss2', 12.5, 'ran_va_medusa_bay'), ('boss2', 16.5, 'medusa_chet_lua'),
    ('boss2', 19.0, 'qua_cau_roi'), ('boss2', 20.5, 'nhat_qua_cau'),
]
for v, t, name in REFS:
    img = next(read_frames(v, ss=t))
    tag = ('m2_%02dm%04.1fs' % (t // 60, t % 60)).replace('.', '_') if v == 'map2' else ('b2_%04.1fs' % t).replace('.', '_')
    Image.fromarray(img).save('_cache/video/ref/%s_%s.png' % (tag, name))
    a = nes_sample(img, CAL[v], 0, 240).clip(0, 255).astype('uint8')
    # hang NES nam ngoai khung video => den (nes_sample da de 0)
    Image.fromarray(a).save('ref/nes/%s_%s_nes.png' % (tag, name))
    print(tag, name)
