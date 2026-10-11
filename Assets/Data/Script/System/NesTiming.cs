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

    // Intro (§2.1): 8.50→15.87 s = 7.37 s ≈ 442 f; Simon đi từ đầu cảnh tới 12.70 s (4.2 s = 252 f) rồi quay lưng.
    public const int INTRO_TOTAL = 442;
    public const int INTRO_SIMON_WALK = 252;

    public static float ToSeconds(int frames)
    {
        return (float)(frames * FRAME_SECONDS);
    }
}
