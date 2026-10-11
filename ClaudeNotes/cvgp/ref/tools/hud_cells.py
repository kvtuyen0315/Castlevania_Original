# hud_cells.py - dinh nghia o HUD (toa do NES) + ham trich o 8x8; Dung: import (khong chay truc tiep)
# memmap work/<video>_nes.npy: hang 0 = NES y Y0 (8 voi map1)
import numpy as np
Y0 = 8
# (ten, x_nes, y_nes) cua tung o chu so
CELLS = {}
for i in range(6):
    CELLS[f'score{i}'] = (48 + 8 * i, 16)
for i in range(4):
    CELLS[f'time{i}'] = (144 + 8 * i, 16)
for i in range(2):
    CELLS[f'stage{i}'] = (232 + 8 * i, 16)
    CELLS[f'heart{i}'] = (184 + 8 * i, 24)
    CELLS[f'p{i}'] = (184 + 8 * i, 32)

def cell(fr, x, y, y0=Y0):
    return fr[y - y0: y - y0 + 8, x: x + 8]

def binar(c, thr=128):
    return (c.min(axis=2) > thr)

def bars(fr, y0=Y0):
    """So vach mau day (PLAYER, ENEMY) + mau tam tung vach."""
    out = []
    for yy in (27, 35):   # giua vach PLAYER (25..30) va ENEMY (33..38)
        cols = []
        for i in range(16):
            px = fr[yy - y0, 57 + 4 * i].astype(int)
            cols.append(px)
        out.append(cols)
    return out
