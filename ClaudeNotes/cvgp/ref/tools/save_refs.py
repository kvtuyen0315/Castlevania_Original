# save_refs.py - luu cac frame tham chieu (FULL FRAME GOC, khong scale) vao ref/
# Chay: python -P -E tools/save_refs.py
# He so hieu chuan ap dung truc tiep cho anh (xem nes_common.CALIB / MAP1_TIMELINE.md §1).
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nes_common import read_frames
from PIL import Image
OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'ref')
os.makedirs(OUT, exist_ok=True)
M1 = {
    40: 'intro_cong_lau_dai', 75: 'stage01_castle_entrance_bat_dau', 99: 'roi_cap1_danh_chau_lua',
    200: 'nhat_roi_nhay_mau_dong_bang', 270: 'roi_cap2_duoi', 455: 'dao_gam_roi_tren_san',
    500: 'cong_lau_dai_camera_khoa_cx512', 620: 'entrance_hall_zombie', 878: 'roi_cap3_duoi_zombie',
    909: 'nhay_kem_di_chuyen_qua_dong_ho', 1278: 'black_leopard_nhay', 1430: 'len_cau_thang',
    1846: 'black_leopard_chay_san', 2253: 'rosary_man_nhay_trang', 2650: 'cua_cuoi_stage1_mo_simon_di_qua',
    2775: 'stage02_crypt_a_vampire_bat', 3004: 'tuong_vo_thit_nuong_diem100', 3216: 'nem_dao_gam_bay',
    3240: 'NONGAME_trang_youtube', 3440: 'crypt_b_tro_lai_con_thanh_youtube', 3525: 'fishman_nhay_khoi_nuoc',
    3560: 'fishman_di_tren_loi_di', 4426: 'simon_de_len_hud_cuoi_cau_thang', 5160: 'stage03_hanh_lang_phantom_bat',
    5400: 'simon_vo_hinh_binh_thuoc', 5930: 'phong_boss_doi_dang_ngu', 5990: 'doi_thuc_day_nhat_II',
    6150: 'riu_trung_doi_E_giam', 6315: 'doi_chet_khoi_lua', 6386: 'qua_cau_do_roi', 6550: 'tally_dem_time',
    6750: 'tally_dem_tim', 7000: 'ban_do_lau_dai_truot_vao', 7180: 'stage04_bat_dau',
}
BOSS = {
    30: 'simon_vao_phong_doi_ngu', 66: 'doi_thuc_day', 120: 'doi_bay_cham_sang_trai', 225: 'doi_lao_xuong_simon',
    270: 'doi_bay_len_lui', 365: 'simon_trung_don_knockback', 495: 'nuoc_thanh_chay_doi_trung',
    545: 'doi_chet_luoi_lua', 615: 'qua_cau_do_roi', 645: 'nhat_cau_hoi_mau',
}
def name_m1(i, d):
    t = i / 30.0
    return f'm1_{int(t//60):02d}m{int(t%60):02d}s_{d}.png'
def name_boss(i, d):
    t = i / 30.0
    return f'boss_{t:04.1f}s_{d}.png'
for video, table, fn in (('map1', M1, name_m1), ('boss', BOSS, name_boss)):
    want = dict(table); last = max(want)
    for i, fr in enumerate(read_frames(video)):
        if i in want:
            p = os.path.join(OUT, fn(i, want[i]))
            Image.fromarray(fr).save(p, optimize=True)
            print('saved', os.path.basename(p))
        if i >= last: break
