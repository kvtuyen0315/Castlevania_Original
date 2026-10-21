# save_refs3.py - luu frame tham chieu: full frame goc (_cache/video/ref/) + lay mau luoi NES 256x240 (ref/nes/, hang ngoai khung = den)
# Chay tu ClaudeNotes/cvm3: python -P -E _cache/tools/save_refs3.py
import sys, os
sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__))))
from nes3 import *
REFS = [
    ('map3', 3.0, 'ban_do_lau_dai_b2_simon_di'), ('map3', 6.8, 'stage07_bat_dau_wea_cx512'),
    ('map3', 8.0, 'vat_trang_duoi_chan_simon'), ('map3', 14.0, 'nuoc_thanh_roi_va_enemy_hong'),
    ('map3', 20.0, 'raven_dau_wea'), ('map3', 26.0, 'skeleton_goc_duoi_trai'),
    ('map3', 34.0, 'skeleton_tren_thang_wea'), ('map3', 39.6, 'den_doi_tang_wea_web'),
    ('map3', 40.0, 'web_cx0_chan_thang'), ('map3', 50.0, 'skeleton_va_raven_web'),
    ('map3', 66.0, 'tui_tien_bi_mat_st07'), ('map3', 72.0, 'skeleton_nem_xuong_web'),
    ('map3', 82.0, 'cua_phai_1_cuon'), ('map3', 84.0, 'cua_phai_1_mo'),
    ('map3', 88.0, 'stage08_walkway_a_cx0'), ('map3', 96.0, 'medusa_head_walkway_a'),
    ('map3', 104.0, 'medusa_head_thang_walkway_a'), ('map3', 108.8, 'den_doi_tang_a_b'),
    ('map3', 109.4, 'walkway_b_cx_am_troi_trai'), ('map3', 115.0, 'tui_tien_bi_mat_st08'),
    ('map3', 124.0, 'bone_tower_walkway_b'), ('map3', 146.0, 'cua_phai_2_mo'),
    ('map3', 151.6, 'stage09_mummyman_cx0'), ('map3', 158.0, 'bone_tower_st09'),
    ('map3', 170.0, 'skeleton_cau_hong_st09'), ('map3', 178.0, 'thap_vo_cau_hong'),
    ('map3', 188.0, 'mummy_dung_truoc_khi_khoa'), ('map3', 198.0, 'khoa_cx1280_mummy'),
    ('map3', 206.0, 'nuoc_thanh_mummy_do'), ('map3', 223.0, 'mummy_1_chet_3000'),
    ('map3', 228.0, 'qua_cau_va_vat_iii'), ('map3', 236.0, 'tally_block3'),
    ('map3', 249.8, 'ban_do_lau_dai_b3_stage10'),
    ('boss3', 2.0, 'vao_phong_mummy'), ('boss3', 8.0, 'mummy_simon_thit_nuong'),
    ('boss3', 12.0, 'mummy_nem_bang'), ('boss3', 20.0, 'simon_trung_don'),
    ('boss3', 26.0, 'mummy_chet'), ('boss3', 34.0, 'qua_cau'), ('boss3', 41.0, 'nhat_cau'),
]
for v, t, name in REFS:
    img = next(read_frames(v, ss=t))
    tag = ('m3_%dm%04.1fs' % (t // 60, t % 60)).replace('.', '_') if v == 'map3' else ('b3_%04.1fs' % t).replace('.', '_')
    Image.fromarray(img).save('_cache/video/ref/%s_%s.png' % (tag, name))
    a = nes_sample(img, CALIB[v], 0, 240).clip(0, 255).astype('uint8')
    Image.fromarray(a).save('ref/nes/%s_%s_nes.png' % (tag, name))
    print(tag, name)
