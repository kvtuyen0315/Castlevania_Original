/// <summary>
/// Hằng luồng Map 3 = Block 3 (CVM3 11 — H6 "lớp hằng có tên"; khuôn Map2FlowTiming): cửa PHẢI (2 cửa đo riêng), đổi tầng thang LÊN-TRÁI cắt
/// mép trên, pha TIME khi vào section, tally sau Mummy Men, bản đồ lâu đài sau Block 3. Đo trên video Map_3 bằng khớp mẫu từng frame
/// (Tools/cvm3/flow11_door3.py — door_0/door_1 + camera = đồng hồ khi cuộn 1 px/tick; ref/data/map3_camera.csv, map3_hud.csv, map3_simon.csv;
/// bản đồ: ClaudeNotes/cvm3/_cache/11/work/cmap3.py) — CVM3_MEMORY §2k. 1 frame video = 2.0033 tick (đều 32 frame / đơn vị TIME = 2.000);
/// video ≈ 22.7 khung mới/s ⇒ mốc đơn ±2 tick. Số trùng Map 1/2 trong ±2 tick ⇒ DÙNG CHUNG (cuộn cửa 128 px @ 1 px/tick, thang lên hiện ở
/// Y 189 = Map 2, tim tally 8, chờ sau tim 120, bản đồ trượt 1.5 px/tick + Simon 16→240, đen bản đồ → stage 27); khác ⇒ hằng ở đây, CHỈ
/// áp cho Block 3 (Map 1/2 giữ nguyên — hash PRE7).
/// </summary>
public static class Map3FlowTiming
{
    /// <summary>Block áp dụng các hằng này (luồng đọc theo GameRules.BlockOfStage(stage)).</summary>
    public const int BLOCK = 3;

    public static bool AppliesTo(int stage)
    {
        return GameRules.BlockOfStage(stage) == BLOCK;
    }

    // ---------------------------------------------------------------- cửa PHẢI (LevelSectionData.Connection.dir +1)
    /// <summary>
    /// Mốc 1 cửa phải Block 3, tính từ tick camera xong cuộn lần 1 (như Map1FlowTiming.DOOR_*): cánh hẹp door_0 hiện, cửa mở door_1 (+SFX),
    /// Simon tự đi (1 px/tick tới entry.x của section đích), cánh hẹp lại, cột cửa trở lại (+SFX), cuộn lần 2. openDx = x góc trái door_1 so với
    /// x cột cửa (trigger.x); door_0 ở openDx + 8 (như Map 1 — Map 1 openDx 0).
    /// </summary>
    public sealed class DoorTiming
    {
        public readonly int leaf, open, walk, closing, closed, scroll2, openDx;
        public readonly string src;

        public DoorTiming(int leaf, int open, int walk, int closing, int closed, int scroll2, int openDx, string src)
        {
            this.leaf = leaf;
            this.open = open;
            this.walk = walk;
            this.closing = closing;
            this.closed = closed;
            this.scroll2 = scroll2;
            this.openDx = openDx;
            this.src = src;
        }
    }

    /// <summary>Cửa #1 WEB → WalkwayA (STAGE 08): cuộn 1 bắt đầu f2438.1, xong f2501.9 (0.991 px/tick); door_0 hiện ∈ (+0.1, +4.1] (f2504),
    /// door_1 ∈ (+4.1, +8.1] (f2506), Simon đi x(t) = 742 + (t − 23.2) (f2514–2519 ⇒ bắt đầu +23), door_0 lại ∈ (+100.3, +104.3] (f2554),
    /// cột cửa ∈ (+104.3, +110.3] (f2557), cuộn 2 bắt đầu +158.5 (f2581.1, 1.012 px/tick) ⇒ STAGE 08 f2645 (cuộn 2 xong f2644.9).
    /// door_1 ở x màn 108 / door_0 116 @ cxw 640 = cột 752 − 4 / + 4 (dò vị trí tự do f2504/f2510/f2540, sai số khớp 0.02).</summary>
    public static readonly DoorTiming DOOR_TO_WALKWAY_A = new DoorTiming(2, 6, 23, 102, 107, 158, -4,
        "video:Map_3 1:21.3–1:28.2 f2438.1→f2504/2506/2514/2554/2557/2581.1 (flow11_door3.py 2425 2650 752 80 -4)");

    /// <summary>Cửa #2 WalkwayB → MummyMan (STAGE 09): cuộn 1 bắt đầu f4336.5, xong f4400.4; door_0 ∈ (+1.2, +3.2] (f4402), door_1 ∈ (+3.2,
    /// +11.2] (f4406 — 4 frame lặp trước đó ⇒ lấy giữa), Simon đi x(t) = 740 + (t − 26.4) (f4415–4426), door_0 lại ∈ (+103.3, +107.3] (f4454),
    /// cột cửa ∈ (+107.3, +113.3] (f4457), cuộn 2 bắt đầu +160.6 (f4480.6) ⇒ STAGE 09 f4546. door_1 x màn 120 / door_0 128 @ cxw 640 =
    /// cột (sheet 752) + 8 / + 16 (f4402/4404/4406/4410/4450 — cột đỏ cùng ở x màn 115–116 như cửa #1 ⇒ 2 cửa lệch nhau 12 px).</summary>
    public static readonly DoorTiming DOOR_TO_MUMMY_MAN = new DoorTiming(2, 7, 26, 105, 110, 161, 8,
        "video:Map_3 2:24.6–2:31.5 f4336.5→f4402/4406/4415/4454/4457/4480.6 (flow11_door3.py 4320 4550 752 48 8)");

    /// <summary>Cửa phải Block 3 theo id kết nối (2 cửa của video lệch nhau 2–4 tick mỗi bước — như Map 2, mỗi cửa dùng mốc đo của chính nó).</summary>
    public static DoorTiming Door(string connectionId)
    {
        return connectionId == "door_to_MummyMan" ? DOOR_TO_MUMMY_MAN : DOOR_TO_WALKWAY_A;
    }

    // ---------------------------------------------------------------- đổi tầng qua thang LÊN-TRÁI cắt mép trên (WEA→WEB, WalkwayA→WalkwayB)
    /// <summary>Đen khi đổi tầng: f1187–1197 (11 frame = 22.0 tick), f3263–3274 (12 frame = 24.0 tick) ⇒ 23 (cả 2 trong ±1; Map 1 18, Map 2 21).</summary>
    public const int STAIR_BLACK_TICKS = 23;
    /// <summary>Lên: Simon hiện ở thang cắt đáy section trên tại Y 189 (WEB 0:39.93 Simon (96,189) — CVM3 05; WalkwayB cx video −33 = Unity 79
    /// ⇔ Simon x ≈ 207 trên thang) = Map 2.</summary>
    public const int STAIR_UP_ARRIVE_Y = 189;

    // ---------------------------------------------------------------- pha TIME khi vào section (Block 3 — Map 1/2 giữ luật 64 của CVGP 12)
    /// <summary>Đầu stage (STAGE 07 hiện f207 TIME 0500 → 0499 f237): 30 frame = 60 tick tới lần giảm đầu. Hồi sinh/Continue dùng cùng số
    /// (suy luận — không có video Simon chết ở Block 3).</summary>
    public const int TIME_FIRST_STAGE = 60;
    /// <summary>Sau đổi tầng: lần giảm đầu 82.1 / 82.1 tick sau ĐẦU màn đen (WEB f1187 → f1228; WalkwayB f3263 → f3304) — cố định theo đầu đen,
    /// không theo lúc hiện (đen 22 / 24) ⇒ với đen 23: 59 tick sau khi section hiện. (CVM2 §5 #16: Map 2 ~80 sau đầu đen — Map 1/2 không đổi.)</summary>
    public const int TIME_FIRST_STAIR = 59;
    /// <summary>Sau cửa: lần giảm đầu 186.1 / 185.1 tick sau khi cuộn 2 bắt đầu (f2581.1 → f2674; f4480.6 → f4573) ⇒ 58 tick sau khi section
    /// đích hiện (cuộn 2 = 128).</summary>
    public const int TIME_FIRST_DOOR = 58;

    // ---------------------------------------------------------------- tally sau Mummy Men
    /// <summary>Nhặt quả cầu f6920 (Stage Clear 230.4–230.7 s) → TIME đếm đầu f7033 = 113 frame = 226.4 tick (Map_2 224, Map_1 260).</summary>
    public const int TALLY_WAIT_TICKS = 226;
    /// <summary>TIME 0000 f7186 → tim giảm đầu f7213 = 27 frame = 54.1 tick (= Map 1; Map 2 58). Đếm TIME 1 tick/đv (304 đv f7033–7186), tim
    /// 8 tick (45 khoảng f7213–7392 = 358.6 tick = 7.97) — dùng chung Config.</summary>
    public const int TALLY_GAP_TICKS = 54;
    /// <summary>Tim cuối f7392 → bản đồ B3 bắt đầu trượt (dx 256 hồi quy f7462.1) = 140.4 tick = chờ sau tim 120 (Config) + đen 20.4 ⇒ 20
    /// (Map 1 23, Map 2 21); đen thấy f7453–7463.</summary>
    public const int MAP_BLACK_TICKS = 20;

    // ---------------------------------------------------------------- bản đồ lâu đài sau Block 3 (video Map_3 4:08.8–4:11.6 — CHỈ có 2.4 s đầu)
    /// <summary>Bản đồ trượt 1.510 px/tick (hồi quy f7469–7546, rms 0.89 px) = Block 1/2 (1.5) — và KHÔNG dừng ở x 0: f7546 dx +1, f7547 dx −2
    /// (khớp nền lệch âm) ⇒ bản đồ 384 px cuộn tiếp để lộ phần phải (ô Block 4–6 ở x nền 261–380). Video hết ở f7547 ⇒ điểm dừng SUY LUẬN:
    /// mép phải bản đồ khớp mép phải màn (x = 256 − 384 = −128).</summary>
    public const int MAP_STOP_X = -128;
    /// <summary>Ô Block 3 / Block 4 trên nền bản đồ (ô đỏ 14×11 — thành phần liên thông của castlemap_bg): Block 1 (133,149), Block 2 (85,117),
    /// Block 3 (165,101) (= dơi B1 (84,115) / B2 (164,99) + (1,2) — dơi ở ô block KẾ, chấm ở ô block VỪA XONG: chấm B1 (148,141) cạnh ô 1, chấm B2
    /// (76,111) cạnh ô 2). Block 4 = ô (285,133) — SUY LUẬN (3 ô còn lại (170,61), (261,85), (285,133); bản đồ cuộn quá x 0 ⇒ ô Block 4 ở phần
    /// phải; chọn ô thấp nhất = tầng hầm nước Stage 10 theo wiki). Toạ độ dưới = toạ độ MÀN khi bản đồ dừng ở MAP_STOP_X.</summary>
    public const int MAP_BAT_X = 285 - 1 + MAP_STOP_X, MAP_BAT_Y = 133 - 2;
    /// <summary>Chấm tím ở ô Block 3 (165,101) phía TRÁI như chấm B2 so với ô 2 (−9, −6) — SUY LUẬN (B1 chấm ở phía phải ô 1: phía đối diện ô kế).</summary>
    public const int MAP_MARKER_X = 165 - 9 + MAP_STOP_X, MAP_MARKER_Y = 101 - 6;
}
