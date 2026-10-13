using System;
using UnityEngine;

/// <summary>
/// Phát AnimSet (CVGP 05) theo tick của StageWorld (CVGP 07) — khác FrameAnimator (tự nghe NesClock.OnTick) ở chỗ
/// thực thể tự gọi Tick() trong lượt của nó ⇒ đóng băng/pause dừng luôn animation, thứ tự tất định.
/// Nhịp giống FrameAnimator: Play ở tick T ⇒ khung 0 hiện từ T và giữ đúng durations[0] tick (tick T không đếm).
/// One-shot hết khung cuối ⇒ Done = true (giữ khung cuối). FrameChanged = khung vừa đổi trong tick này (hoặc vừa Play) —
/// state code dùng để bật hitbox/sinh đạn đúng khung (hoặc nghe OnFrame).
/// Apply(facing): sprite thân + flipX (sheet quay trái); lớp phủ (roi) đặt tại pivot + overlayOffsets (quay phải ⇒ −x, flipX)
/// trên renderer con "Overlay" tự tạo, thứ tự vẽ ±1 quanh thân theo overlayOnTop.
/// </summary>
public class AnimPlayer
{
    /// <summary>Khi anim thiếu durations (AnimSetBuilder vốn chặn) — bằng mặc định FrameAnimator.frameDuration (CVTI).</summary>
    public const int FALLBACK_DURATION = 8;

    private readonly Entity owner;
    private readonly SpriteRenderer body;
    private SpriteRenderer overlay;
    private int counter;
    private int playTick = int.MinValue;

    public AnimSet Set { get; set; }
    /// <summary>Renderer lớp phủ (roi) — null tới lần đầu có lớp phủ (CVGP 11 đổi bảng màu cả roi).</summary>
    public SpriteRenderer Overlay => overlay;
    public AnimSet.Anim Current { get; private set; }
    public int FrameIndex { get; private set; }
    public bool FrameChanged { get; private set; }
    public bool Done { get; private set; }
    /// <summary>Số tick đã chạy từ lần Play (không gồm tick Play).</summary>
    public int TicksInAnim { get; private set; }
    /// <summary>Hệ số chậm: mỗi khung giữ durations × slow (mặc định 1). Không đổi giữa chừng trừ khi cần.</summary>
    public int Slow { get; set; } = 1;

    /// <summary>Khung vừa hiện (index). Gán 1 lần (Awake) — không tạo delegate mỗi tick.</summary>
    public event Action<int> OnFrame;

    public AnimPlayer(Entity owner, SpriteRenderer body)
    {
        this.owner = owner;
        this.body = body;
    }

    public bool Visible
    {
        get => body != null && body.enabled;
        set
        {
            if (body != null)
                body.enabled = value;
            if (overlay != null && !value)
                overlay.enabled = false;
        }
    }

    public bool IsPlaying(AnimSet.Anim a)
    {
        return Current == a && a != null;
    }

    /// <summary>Phát anim theo tên trong Set (cùng anim đang chạy và !restart ⇒ không làm gì). Hot path: cache Anim rồi gọi Play(Anim).</summary>
    public void Play(string animName, bool restart = false)
    {
        if (Set == null)
        {
            Debug.LogError("[AnimPlayer] " + owner.name + ": chưa gán AnimSet");
            return;
        }
        if (!restart && Current != null && Current.name == animName)
            return;
        Play(Set.Get(animName), true);
    }

    public void Play(AnimSet.Anim a, bool restart = false)
    {
        if (a == null || (!restart && a == Current))
            return;
        Current = a;
        FrameIndex = 0;
        counter = 0;
        Done = false;
        TicksInAnim = 0;
        FrameChanged = true;
        playTick = owner.World != null ? owner.World.TickCount : int.MinValue;
        if (OnFrame != null)
            OnFrame(0);
    }

    /// <summary>Giữ nguyên khung i (không chạy) — vd tư thế cố định.</summary>
    public void SetFrame(AnimSet.Anim a, int i)
    {
        Current = a;
        FrameIndex = a != null ? Mathf.Clamp(i, 0, Mathf.Max(0, a.FrameCount - 1)) : 0;
        counter = 0;
        Done = true;
        FrameChanged = true;
    }

    public void Tick()
    {
        FrameChanged = false;
        if (Current == null || Done || Current.FrameCount == 0)
            return;
        if (owner.World != null && owner.World.TickCount == playTick)
            return;
        TicksInAnim++;
        if (++counter < Duration(FrameIndex))
            return;
        counter = 0;
        if (FrameIndex + 1 < Current.FrameCount)
            FrameIndex++;
        else if (Current.loop)
            FrameIndex = 0;
        else
        {
            Done = true;
            return;
        }
        FrameChanged = true;
        if (OnFrame != null)
            OnFrame(FrameIndex);
    }

    private int Duration(int i)
    {
        var d = Current.durations;
        int v = d != null && i < d.Length ? d[i] : FALLBACK_DURATION;
        return Mathf.Max(1, v) * Mathf.Max(1, Slow);
    }

    public void Apply(int facing)
    {
        if (body == null || Current == null || Current.FrameCount == 0)
            return;
        bool right = facing > 0;
        body.sprite = Current.frames[FrameIndex];
        body.flipX = right;
        var ov = Current.overlays;
        if (Current.HasOverlay && FrameIndex < ov.Length && ov[FrameIndex] != null)
        {
            EnsureOverlay();
            overlay.enabled = body.enabled;
            overlay.sprite = ov[FrameIndex];
            var off = FrameIndex < Current.overlayOffsets.Length ? Current.overlayOffsets[FrameIndex] : Vector2Int.zero;
            overlay.transform.localPosition = NesCoord.ToWorld(right ? -off.x : off.x, off.y);
            overlay.flipX = right;
            bool onTop = FrameIndex < Current.overlayOnTop.Length && Current.overlayOnTop[FrameIndex];
            overlay.sortingLayerID = body.sortingLayerID;
            overlay.sortingOrder = body.sortingOrder + (onTop ? 1 : -1);
        }
        else if (overlay != null)
            overlay.enabled = false;
    }

    private void EnsureOverlay()
    {
        if (overlay != null)
            return;
        var go = new GameObject("Overlay");
        go.transform.SetParent(body.transform, false);
        overlay = go.AddComponent<SpriteRenderer>();
        overlay.sharedMaterial = body.sharedMaterial;
    }
}
