using System;
using UnityEngine;

/// <summary>
/// Phát chuỗi sprite theo số frame NES mỗi khung, chạy bằng NesClock.OnTick (CVTI_MEMORY §4.3).
/// durations[i] = số tick hiện khung i; mảng rỗng ⇒ mọi khung dùng frameDuration. Loop hoặc one-shot (dừng ở khung cuối, gọi OnComplete).
/// Khung 0 hiện ngay từ lúc Play và giữ đúng durations[0] frame (tick cùng frame với Play bị bỏ qua).
/// </summary>
public class FrameAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private Sprite[] frames = new Sprite[0];
    [SerializeField] private int[] durations = new int[0];
    [SerializeField, Min(1)] private int frameDuration = 8;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnEnable = true;

    public event Action OnComplete;

    private int index;
    private int counter;
    private int startFrame;

    public bool IsPlaying { get; private set; }
    public int CurrentIndex => index;

    private void Reset()
    {
        target = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        NesClock.OnTick += HandleTick;
        if (playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        NesClock.OnTick -= HandleTick;
    }

    /// <summary>Phát lại từ khung 0 với dữ liệu hiện tại.</summary>
    public void Play()
    {
        index = 0;
        counter = 0;
        startFrame = NesClock.Frame;
        IsPlaying = frames != null && frames.Length > 0;
        Apply();
    }

    /// <summary>Đổi dữ liệu rồi phát từ khung 0. durationsPerFrame null/rỗng ⇒ mọi khung dùng frameDuration hiện có.</summary>
    public void Play(Sprite[] newFrames, int[] durationsPerFrame, bool isLoop, Action onComplete = null)
    {
        frames = newFrames ?? new Sprite[0];
        durations = durationsPerFrame ?? new int[0];
        loop = isLoop;
        OnComplete = onComplete;
        Play();
    }

    public void Play(Sprite[] newFrames, int durationEach, bool isLoop, Action onComplete = null)
    {
        frameDuration = Mathf.Max(1, durationEach);
        Play(newFrames, null, isLoop, onComplete);
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    /// <summary>Hiện khung i và đứng yên (không phát).</summary>
    public void SetFrame(int i)
    {
        IsPlaying = false;
        index = Mathf.Clamp(i, 0, Mathf.Max(0, frames.Length - 1));
        Apply();
    }

    int DurationOf(int i)
    {
        return durations != null && i < durations.Length ? Mathf.Max(1, durations[i]) : frameDuration;
    }

    void HandleTick(int frame)
    {
        if (!IsPlaying || frame <= startFrame || !isActiveAndEnabled)
            return;

        if (++counter < DurationOf(index))
            return;
        counter = 0;

        if (index + 1 < frames.Length)
            index++;
        else if (loop)
            index = 0;
        else
        {
            IsPlaying = false;
            OnComplete?.Invoke();
            return;
        }
        Apply();
    }

    void Apply()
    {
        if (target != null && frames != null && index < frames.Length)
            target.sprite = frames[index];
    }
}
