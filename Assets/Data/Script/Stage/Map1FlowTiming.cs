/// <summary>
/// Hằng thời lượng / vị trí của luồng Map 1 (CVGP 15 — H6 "lớp hằng có tên"): cổng lâu đài, cửa cuối stage, đổi tầng qua cầu thang,
/// bản đồ lâu đài, Stage 04 stub. Mọi số đo trên video Map_1 bằng khớp mẫu sprite (Tools/cvgp/simon_fit.py, enemy_fit.py, map1_grab.py)
/// — chi tiết + frame ở ClaudeNotes/cvgp/MAP1_PLACEMENT.md §3 và CVGP_MEMORY §2m. 1 frame video = 2.003 tick; mốc đơn ±1 frame.
/// </summary>
public static class Map1FlowTiming
{
    // ---------------------------------------------------------------- cổng lâu đài (Castle Entrance → Entrance Hall)
    /// <summary>Simon (trên đất) tới x ≥ này ⇒ tự đi vào cổng: hồi quy gãy khúc f485–546 — gãy ở x 678.1 (rms 0.80 px).</summary>
    public const int GATE_TRIGGER_X = 678;
    /// <summary>Hết cửa sổ kích hoạt (mép lớp FG vòm cổng x 704 — CVGP 06): đáp xuống đất ở x ≥ này (nhảy VƯỢT cổng) không tự đi
    /// (wiki Secrets "phải canh nhảy chuẩn, nếu không sẽ tự đi vào cửa") — SUY LUẬN vị trí.</summary>
    public const int GATE_WINDOW_MAX_X = 704;
    /// <summary>Tốc độ tự đi trong cổng: 0.305 px/tick (hồi quy f505–546) ⇒ fixed 78/256 = 0.3047.</summary>
    public const int GATE_WALK_SPEED_FIXED = 78;
    /// <summary>Tự đi tới x này rồi cắt đen: gãy f505.25 → đen f591 = 171.8 tick × 0.305 = 52 px ⇒ 678 + 52.</summary>
    public const int GATE_END_X = 730;
    /// <summary>Màn đen giữa cổng và Entrance Hall: đen f591–599, EH hiện f600 (9 frame ≈ 18 tick).</summary>
    public const int GATE_BLACK_TICKS = 18;

    // ---------------------------------------------------------------- cửa cuối stage (EH → CA, CA → PB) — 2 lần đo y hệt nhau
    /// <summary>Camera cuộn 128 px @ 1.00 px/tick (hồi quy 4 đoạn: 0.999 / 1.000 / 1.002 / 0.995 px/tick, rms ≤ 1 px) — planner ghi
    /// 0.90–0.91 là SAI (lấy nhầm mốc đầu/cuối).</summary>
    public const int DOOR_SCROLL_PX = 128;
    public const int DOOR_SCROLL_SPEED_FIXED = 256;
    /// <summary>Mốc tính từ lúc cuộn lần 1 xong (tick): cánh cửa hẹp door_0 hiện (+0..+4 ⇒ 2), cửa mở door_1 + SFX Door (+12, 2 lần đo
    /// [11.6, 13.6] ∩ [10.2, 12.2]), Simon tự đi (+27 / +26 ⇒ 26), cánh hẹp lại (+97), cột cửa trở lại + SFX Door (+106), cuộn lần 2 (+152 / +151.9).</summary>
    public const int DOOR_LEAF_TICK = 2;
    public const int DOOR_OPEN_TICK = 12;
    public const int DOOR_WALK_TICK = 26;
    /// <summary>Simon tự đi 68 px @ 1 px/tick (x màn 108 → 176, 2 lần đo 68.3 / 67.4 tick).</summary>
    public const int DOOR_WALK_PX = 68;
    public const int DOOR_CLOSING_TICK = 97;
    public const int DOOR_CLOSED_TICK = 106;
    public const int DOOR_SCROLL2_TICK = 152;
    /// <summary>Cánh cửa (sprite Object/door_0 hẹp 16, door_1 rộng 24 — khớp mẫu y 65 NES = section y 33): door_0 ở x cột cửa + 8,
    /// door_1 ở x cột cửa (EH: 1536 / 1528 — f2616/f2622 x màn 128/120 @ cx 1408). Simon vẽ SAU cánh cửa (f2633–2644 bị che).</summary>
    public const int DOOR_SPRITE_Y = 33;
    public const int DOOR_LEAF_DX = 8;

    // ---------------------------------------------------------------- đổi tầng qua cầu thang cắt mép (Crypt B ↔ Crypt A)
    /// <summary>Lên: Y (mặt sàn logic) ≤ 31 ở thang cắt đỉnh ⇒ đen (CB f4431 còn thấy ở Y 32 — khung walk_1 vẽ thấp 2).</summary>
    public const int STAIR_UP_EXIT_Y = 31;
    /// <summary>Lên: hiện ở thang cắt đáy section trên tại Y 188 (CA f4442 Y 187 sau ~2 tick leo tiếp).</summary>
    public const int STAIR_UP_ARRIVE_Y = 188;
    /// <summary>Xuống: Y ≥ 192 ở thang cắt đáy ⇒ đen (CA f3233 còn thấy ở Y 191).</summary>
    public const int STAIR_DOWN_EXIT_Y = 192;
    /// <summary>Xuống: hiện ở Y 35 (CB f3431 — frame game đầu sau đoạn quảng cáo; đối xứng 192 − 157 với lần lên 31 ↔ 188 — SUY LUẬN).</summary>
    public const int STAIR_DOWN_ARRIVE_Y = 35;
    /// <summary>Đen khi đổi tầng: f4432–4440 (9 frame ≈ 18 tick; planner 0.33 s đếm cả frame cuối còn Simon).</summary>
    public const int STAIR_BLACK_TICKS = 18;

    // ---------------------------------------------------------------- hết Block 1 → bản đồ lâu đài → Stage 04
    /// <summary>Tally xong (StageRules.TallyDone, f6956 đen) → bản đồ bắt đầu trượt từ x 256 ở f6967.5 (hồi quy) ⇒ đen ≈ 23 tick.</summary>
    public const int MAP_BLACK_TICKS = 23;
    /// <summary>Bản đồ trượt vào từ phải 1.5 px/tick (hồi quy 68 frame: 1.515, rms 1.35 px) ⇒ fixed 384/256; dừng ở x 0 (f7051.8).</summary>
    public const int MAP_SLIDE_SPEED_FIXED = 384;
    /// <summary>Bản đồ (castlemap_bg 384×144) đặt góc trên-trái ở NES y 48 (khớp ZNCC dy 48).</summary>
    public const int MAP_Y = 48;
    /// <summary>Dơi tím (castlemap_bat_0/1) góc (84,115), chấm tím (castlemap_marker) góc (148,141) — khớp mẫu f7054–7163.</summary>
    public const int MAP_BAT_X = 84, MAP_BAT_Y = 115, MAP_MARKER_X = 148, MAP_MARKER_Y = 141;
    /// <summary>Dơi đổi khung bat_0 ↔ bat_1 mỗi 8 tick (chu kỳ 8 frame video); chấm tím hiện cùng pha bat_1; bắt đầu bat_0 lúc bản đồ dừng.</summary>
    public const int MAP_BLINK_HALF_TICKS = 8;
    /// <summary>Simon hiện lúc bản đồ dừng ở x 16, đi 1 px/tick sang phải tới x 240 (f7053 x 18 … f7163 x 238), Y mặt sàn 192 NES.</summary>
    public const int MAP_SIMON_START_X = 16, MAP_SIMON_END_X = 240, MAP_SIMON_Y = 192;
    /// <summary>Khung đi: walk_0, walk_1, walk_2, walk_1 mỗi 7 tick, bắt đầu walk_0 (f7053–7055 idle_0 ≡ walk_0).</summary>
    public const int MAP_SIMON_FRAME_TICKS = 7;
    /// <summary>SFX MapStep (07) mỗi 16 tick, lần đầu ~14 tick sau khi dừng (3:55.3 = f7059 ⇐ dừng f7051.8) — 14 lần như video.</summary>
    public const int MAP_STEP_SFX_FIRST = 14, MAP_STEP_SFX_EVERY = 16;
    /// <summary>Simon tới x 240 ⇒ đen ≈ 27 tick (f7163.6 → Stage 04 f7177).</summary>
    public const int MAP_TO_STAGE4_BLACK_TICKS = 27;

    // ---------------------------------------------------------------- điểm vào section (khớp mẫu — 06 dùng x_med lệch +1 khi quay phải)
    /// <summary>Entrance Hall sau cổng: f600 khớp mẫu idle_0 pivot x 48, Y 176.</summary>
    public const int EH_ENTRY_X = 48, EH_ENTRY_Y = 176;
    /// <summary>Đầu Stage 02 (Crypt A) / Stage 03 (Phantom Bat): f2756 / f5126 pivot x 48, Y 80 (= cửa: 1516 + 68 − 1536).</summary>
    public const int STAGE_ENTRY_X = 48, STAGE_ENTRY_Y = 80;
}
