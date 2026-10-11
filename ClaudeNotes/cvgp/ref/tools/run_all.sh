#!/bin/sh
# run_all.sh - chay lai toan bo chuoi do (Git Bash). Chay: sh tools/run_all.sh   (tu thu muc video/)
# Thoi gian ~10-15 phut. Ket qua trong work/ (CSV) + ref/.
set -e
P="python -P -E"
$P tools/extract_nes.py map1          # ~90 s: work/map1_nes.npy (NES 256x224/frame) + meta (diff)
$P tools/extract_nes.py boss
$P tools/hud_glyphs.py map1 frames/glyphs_map1.png   # gom mau chu so (nhan da dan trong hud_read.py)
$P tools/hud_read.py map1 && $P tools/hud_read.py boss   # work/*_hud.csv
$P tools/hud_events.py map1 > work/map1_events.txt
$P tools/dup_frames.py map1 && $P tools/dup_frames.py boss
$P tools/camera_track.py map1         # work/map1_camera.csv (vung + camera x)
$P tools/simon_track.py map1          # work/map1_simon.csv
$P tools/whip_track.py map1 > work/whip_attacks.txt
$P tools/simon_states.py map1
$P tools/measure_walk.py map1
$P tools/measure_jump.py map1
$P tools/measure_stairs.py map1 1400 1466 2357 2438 4372 4428 4569 4629 3431 3459
$P tools/time_rate.py map1 655 2240 3502 4420 5170 5900 2560 3230 4441 4880
$P tools/boss_track.py boss 0 760 work/boss_bat.csv
$P tools/boss_track.py map1 5935 6330 work/map1_bat.csv
$P tools/simon_boss.py 0 760 work/boss_simon2.csv
$P tools/boss_table.py boss 0 25.0 > work/boss_table.md
$P tools/boss_table.py map1 197.5 211.0 > work/map1_boss_table.md
$P tools/boss_speed.py work/boss_bat.csv 2.75 3.6 3.65 5.2 7.05 8.5 9.15 10.5 11.05 12.25 12.3 13.25 13.3 14.2 15.3 15.6
$P tools/boss_speed.py work/map1_bat.csv 199.95 200.9 200.95 204.15 206.75 207.6
$P tools/overlay_scan.py map1
$P tools/save_refs.py
