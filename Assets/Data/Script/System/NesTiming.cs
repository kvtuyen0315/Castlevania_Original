/// <summary>
/// Hằng thời lượng tính bằng frame NES 60 fps (H4) — đo từ video gốc, nguồn: CVTI_MEMORY §2.
/// Mọi logic theo thời gian đếm tick của NesClock, KHÔNG dùng Time.deltaTime.
/// </summary>
public static class NesTiming
{
    public const int FPS = 60;
    public const double FRAME_SECONDS = 1.0 / FPS;

    // Title (§2.1): PUSH START KEY nháy 8 f tắt / 8 f bật × 5 lần sau khi bấm Start = 80 f, rồi cắt đen.
    public const int TITLE_BLINK_HALF = 8;
    public const int TITLE_BLINK_COUNT = 5;
    public const int TITLE_BLINK_TOTAL = TITLE_BLINK_HALF * 2 * TITLE_BLINK_COUNT;

    // Title (§2.3): 37 khung bay (title_bat_00..36) chạy ngay khi Title hiện, xen kẽ 3 f / 4 f (trung bình 3.5 f đo được,
    // từng khung 3 hay 4 không phân giải được — xấp xỉ) ⇒ 129 f; rồi lơ lửng 37-38-39-38 lặp, mỗi tư thế 4 f (chu kỳ 16 f).
    public const int TITLE_BAT_FLIGHT_FRAMES = 37;
    public const int TITLE_BAT_FLIGHT_SHORT = 3;
    public const int TITLE_BAT_FLIGHT_LONG = 4;
    public const int TITLE_BAT_HOVER_POSE = 4;

    // Màn đen giữa các cảnh (§2.1): 8.23→8.50 s và 15.87→16.40 s.
    public const int BLACK_AFTER_TITLE = 16;
    public const int BLACK_AFTER_INTRO = 32;

    // Intro (§2.1): 8.50→15.87 s = 7.37 s ≈ 442 f.
    public const int INTRO_TOTAL = 442;

    // Intro — Simon (§2.4, khớp từng khung video 30 fps với sprite): đi 1 px mỗi 2 f từ x 243 tới 119 = 124 px ⇒ 248 f,
    // rồi quay lưng (video 12.67 s). Chu kỳ đi walk_0→1→2→1, mỗi tư thế 7 f (28 f/chu kỳ), lệch pha 4 f lúc cảnh bật.
    public const int INTRO_SIMON_WALK = 248;
    public const int INTRO_SIMON_STEP = 2;
    public const int INTRO_SIMON_POSE = 7;
    public const int INTRO_SIMON_POSE_PHASE = 4;

    // Intro — mây (§2.4): trôi sang trái 1 px mỗi 8 f (video 7.6 px/s), lệch pha 4 f.
    public const int INTRO_CLOUD_STEP = 8;
    public const int INTRO_CLOUD_PHASE = 4;

    // Intro — 2 dơi nhỏ (§2.4, XẤP XỈ): vỗ cánh đổi khung mỗi 8 f; dơi B hiện từ frame 66 (video 9.6 s).
    public const int INTRO_BAT_FLAP = 8;
    public const int INTRO_BAT_B_APPEAR = 66;

    public static float ToSeconds(int frames)
    {
        return (float)(frames * FRAME_SECONDS);
    }
}
