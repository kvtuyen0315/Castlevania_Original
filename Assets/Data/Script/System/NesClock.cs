using System;
using afsdk;
using UnityEngine;

/// <summary>
/// Đồng hồ logic 60 Hz cố định (CVTI_MEMORY §4.3): accumulator trong Update theo Time.unscaledDeltaTime,
/// tối đa MAX_TICKS_PER_UPDATE tick mỗi Update (máy khựng lâu thì bỏ phần dư — game chậm lại chứ không nhảy cóc).
/// Mỗi tick: OnSystemTick (GameManager: input + luồng màn hình) rồi OnTick (FrameAnimator, object trong màn).
/// Event static để component bật trước khi NesClock Awake vẫn đăng ký được.
/// </summary>
[DefaultExecutionOrder(-100)]
public class NesClock : ManualSingletonMono<NesClock>
{
    // Không "snap" deltaTime về 1/60: đã thử, đồng hồ chạy nhanh hơn thời gian thực (lệch nhạc) — CVTI_MEMORY §4.3.
    public const int MAX_TICKS_PER_UPDATE = 4;

    /// <summary>Số tick đã chạy từ lúc vào Play (frame NES toàn cục).</summary>
    public static int Frame { get; private set; }

    public static event Action<int> OnSystemTick;
    public static event Action<int> OnTick;

    private double accumulator;

    protected override void Awake()
    {
        base.Awake();
        Frame = 0;
        accumulator = 0;
    }

    private void Update()
    {
        accumulator += Time.unscaledDeltaTime;

        int ticks = 0;
        while (accumulator >= NesTiming.FRAME_SECONDS && ticks < MAX_TICKS_PER_UPDATE)
        {
            accumulator -= NesTiming.FRAME_SECONDS;
            Step();
            ticks++;
        }
        if (accumulator >= NesTiming.FRAME_SECONDS)
            accumulator = 0;
    }

    private void Step()
    {
        Frame++;
        OnSystemTick?.Invoke(Frame);
        OnTick?.Invoke(Frame);
    }
}
