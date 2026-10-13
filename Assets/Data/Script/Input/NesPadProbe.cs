#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dụng cụ kiểm thử NesPad (chỉ Editor, harness MCP thêm vào lúc Play): phát lịch "phần cứng" raw theo từng Update
/// (SetTestRaw — Update của NesPad vẫn chốt như thật) và ghi trạng thái từng tick để đếm Pressed/Held/Released.
/// Chạy trước NesPad (-300) để raw của Update này có hiệu lực ngay trong Update đó.
/// </summary>
[DefaultExecutionOrder(-300)]
public class NesPadProbe : MonoBehaviour
{
    public struct TickRow
    {
        public int tick;
        public int unityFrame;
        public int held;
        public int pressed;
        public int released;
    }

    public readonly List<TickRow> rows = new List<TickRow>();
    public int UpdatesRun { get; private set; }
    public bool Done { get; private set; }

    private int[] schedule;
    private int index;
    private int firstUpdateFrame = -1;
    private int lastUpdateFrame = -1;

    /// <summary>schedule[i] = bitmask raw ở Update thứ i; hết lịch ⇒ ClearTestRaw, Done sau thêm `tail` Update.</summary>
    public void Begin(int[] rawPerUpdate)
    {
        schedule = rawPerUpdate;
        index = 0;
        rows.Clear();
        UpdatesRun = 0;
        Done = false;
        firstUpdateFrame = -1;
    }

    private void OnEnable()
    {
        NesClock.OnTick += OnTick;
    }

    private void OnDisable()
    {
        NesClock.OnTick -= OnTick;
        if (NesPad.I != null)
            NesPad.I.ClearTestRaw();
    }

    private void Update()
    {
        if (schedule == null || Done || NesPad.I == null)
            return;
        if (firstUpdateFrame < 0)
            firstUpdateFrame = Time.frameCount;
        if (index < schedule.Length)
        {
            NesPad.I.SetTestRaw(schedule[index++]);
            UpdatesRun++;
            lastUpdateFrame = Time.frameCount;
        }
        else
        {
            NesPad.I.ClearTestRaw();
            Done = true;
        }
    }

    private void OnTick(int frame)
    {
        if (schedule == null || firstUpdateFrame < 0 || NesPad.I == null || (Done && Time.frameCount > lastUpdateFrame + 1))
            return;
        rows.Add(new TickRow
        {
            tick = frame,
            unityFrame = Time.frameCount,
            held = NesPad.I.HeldMask,
            pressed = NesPad.I.PressedMask,
            released = NesPad.I.ReleasedMask,
        });
    }

    public int Count(eNesButton b, int which)
    {
        int bit = 1 << (int)b;
        int n = 0;
        foreach (var r in rows)
        {
            int m = which == 0 ? r.held : which == 1 ? r.pressed : r.released;
            if ((m & bit) != 0)
                n++;
        }
        return n;
    }
}
#endif
