/// <summary>
/// Hằng luồng Map 2 = Block 2 (CVM2 11 — H6 "lớp hằng có tên"): cửa TRÁI, đổi tầng qua thang, tally, bản đồ lâu đài Block 2. Đo trên
/// video Map_2 bằng khớp mẫu từng frame (ClaudeNotes/cvm2/_cache/11/tools/door11b.py, cmap11b.py + ref/data/map2_camera.csv,
/// map2_simon.csv, map2_hud.csv — CVM2_MEMORY §2m). 1 frame video = 2.0033 tick; mốc đơn ±1 tick. Số trùng Map 1 trong ±2 tick ⇒ DÙNG
/// CHUNG Map1FlowTiming (cuộn cửa 128 px @ 1 px/tick, tim tally 8 tick, chờ sau tim 120, bản đồ trượt 1.5 px/tick + Simon 16→240 @ 1 px/tick,
/// đen bản đồ → stage 27); khác ⇒ hằng ở đây, chỉ áp cho Block 2 (Block 3–6 chưa có video ⇒ giữ số Map 1).
/// </summary>
public static class Map2FlowTiming
{
    /// <summary>Block áp dụng các hằng này (luồng đọc theo GameRules.BlockOfStage(stage)).</summary>
    public const int BLOCK = 2;

    public static bool AppliesTo(int stage)
    {
        return GameRules.BlockOfStage(stage) == BLOCK;
    }

    // ---------------------------------------------------------------- cửa TRÁI (LevelSectionData.Connection.dir −1)
    /// <summary>
    /// Mốc 1 cửa, tính từ tick camera xong cuộn lần 1 (như Map1FlowTiming.DOOR_*): cánh hẹp door_0 hiện, cửa mở door_1 (+SFX), Simon tự đi
    /// (1 px/tick tới entry.x của section đích), cánh hẹp lại, cột cửa trở lại (+SFX), cuộn lần 2.
    /// </summary>
    public sealed class DoorTiming
    {
        public readonly int leaf, open, walk, closing, closed, scroll2;
        public readonly string src;

        public DoorTiming(int leaf, int open, int walk, int closing, int closed, int scroll2, string src)
        {
            this.leaf = leaf;
            this.open = open;
            this.walk = walk;
            this.closing = closing;
            this.closed = closed;
            this.scroll2 = scroll2;
            this.src = src;
        }
    }

    /// <summary>Cửa #1 TEB → Hall A (STAGE 05): cuộn 1 xong f1616.5 (cx ghép 641 @ f1616 = tick 127); door_0 hiện f1618, door_1 f1621,
    /// Simon 791 → 790 f1629 (đi 71 px tới 720 = f1665), door_0 f1669, cột cửa f1672, cuộn 2 bắt đầu f1696.5 (cx 639 @ f1697).</summary>
    public static readonly DoorTiming DOOR_TO_HALL_A = new DoorTiming(2, 8, 24, 104, 110, 160,
        "video:Map_2 0:53.9–0:56.6 f1616.5→f1618/1621/1628.5/1669/1672/1696.5 (door11b.py, khớp mẫu door_0/door_1 lật ngang)");

    /// <summary>Cửa #2 Hall B → Hall C (STAGE 06): cuộn 1 xong f4332.0; door_0 f4334, door_1 f4340, Simon 794 → 793 f4349 (74 px tới 720
    /// = f4384), door_0 f4388, cột cửa f4392, cuộn 2 bắt đầu f4414.8.</summary>
    public static readonly DoorTiming DOOR_TO_HALL_C = new DoorTiming(3, 15, 33, 111, 119, 166,
        "video:Map_2 2:24.4–2:27.2 f4332.0→f4334/4340/4348.5/4388/4392/4414.8 (door11b.py)");

    /// <summary>
    /// 2 cửa trái của video lệch nhau 7–9 tick từ bước "cửa mở" (+8 / +15) — Map 1 2 cửa trùng nhau (+12): NES có lẽ bước theo bộ đếm khung
    /// chung (chưa rõ luật, CVM2_MEMORY §6) ⇒ mỗi cửa dùng mốc đo của chính nó; cửa trái khác (chưa đo) dùng cửa #1.
    /// </summary>
    public static DoorTiming Door(string connectionId)
    {
        return connectionId == "door_to_TowerHallC" ? DOOR_TO_HALL_C : DOOR_TO_HALL_A;
    }

    /// <summary>Cánh cửa cửa trái: door_0 và door_1 LẬT NGANG, cùng góc trái = x cột cửa − 16 (khớp mẫu x màn 120 khi cột ở 136, cả 2 cửa:
    /// f1618/1621/1669, f4334/4340/4388 — gương của Map 1: door_1 ở x cột, door_0 ở x cột + 8), đỉnh = đỉnh cột + 1 (NES y 65 / 97).</summary>
    public const int DOOR_LEFT_DX = -16;
    public const int DOOR_DY = 1;

    // ---------------------------------------------------------------- đổi tầng qua thang cắt mép (TEA→TEB, Hall A→B, Hall C→Medusa)
    /// <summary>Đen khi đổi tầng: 11 / 11 / 10 frame (f855–865, f2790–2800, f5914–5923) = 22.0 / 22.0 / 20.0 tick ⇒ 21 (cả 3 trong ±1; Map 1 18).</summary>
    public const int STAIR_BLACK_TICKS = 21;
    /// <summary>Lên: Simon hiện ở thang cắt đáy section trên tại Y 189 (f866, f2801, f5924: walk_1 NES y 221 — Map 1 188). Rời Y 31 như Map 1.</summary>
    public const int STAIR_UP_ARRIVE_Y = 189;

    // ---------------------------------------------------------------- tally sau Queen Medusa
    /// <summary>Nhặt quả cầu (nhạc Stage Clear 3:58.83 = f7164.9) → TIME đếm đầu f7277 = 224.6 tick (Map_1 260; video boss Map_1 220 —
    /// luật NES chưa rõ, CVM2_MEMORY §6).</summary>
    public const int TALLY_WAIT_TICKS = 224;
    /// <summary>TIME 0000 (f7380) → tim giảm đầu (f7409) = 58.1 tick (Map_1 54).</summary>
    public const int TALLY_GAP_TICKS = 58;
    /// <summary>Tally xong (sân chơi đen f7584, cả màn f7585) → bản đồ B2 bật (dx 256 hồi quy f7594.7) = 20.4–21.4 tick ⇒ 21 (Map 1 23).</summary>
    public const int MAP_BLACK_TICKS = 21;

    // ---------------------------------------------------------------- bản đồ lâu đài sau Block 2 (video Map_2 4:13.2–4:19.7)
    /// <summary>Dơi tím ở ô Block 2 (góc castlemap_bat_0/1) (164, 99) — khớp mẫu f7681–7700 (sai 19); chấm tím (castlemap_marker) ô kế
    /// (76, 111) cùng pha bat_1 (f7682–7685, 7690–7693). Nhịp 8 tick, trượt bản đồ 1.4975 px/tick (f7610 dx 210 → f7679 dx 3), Simon
    /// hiện f7681 x 18 rồi đi tới 241 @ 1.004 px/tick = như Block 1 (Map1FlowTiming.MAP_*).</summary>
    public const int MAP_BAT_X = 164, MAP_BAT_Y = 99, MAP_MARKER_X = 76, MAP_MARKER_Y = 111;
}
