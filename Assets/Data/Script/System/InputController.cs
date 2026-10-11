using afsdk;
using UnityEngine;

/// <summary>
/// Input Manager cũ (activeInputHandler 0). Start = Return / KeypadEnter / Space / joystick button 7, 9.
/// Đọc theo tick như NES đọc tay cầm mỗi frame: GameManager gọi Tick() đầu mỗi tick, rồi màn hình đọc StartPressedThisFrame.
/// Cạnh nhấn = vừa nhấn trong Update kể từ tick trước (chốt lại để không lọt khi fps cao) hoặc giữ ở tick này mà tick trước chưa giữ.
/// </summary>
[DefaultExecutionOrder(-200)]
public class InputController : ManualSingletonMono<InputController>
{
    static readonly KeyCode[] START_KEYS =
    {
        KeyCode.Return, KeyCode.KeypadEnter, KeyCode.Space, KeyCode.JoystickButton7, KeyCode.JoystickButton9,
    };

    /// <summary>Start vừa được nhấn ở tick hiện tại (chỉ 1 tick, giữ phím không lặp).</summary>
    public bool StartPressedThisFrame { get; private set; }

    public bool StartHeld { get; private set; }

    private bool startDownLatched;

    private void Update()
    {
        if (AnyDown(START_KEYS))
            startDownLatched = true;
    }

    public void Tick()
    {
        bool held = AnyHeld(START_KEYS);
        StartPressedThisFrame = startDownLatched || (held && !StartHeld);
        StartHeld = held;
        startDownLatched = false;
    }

    /// <summary>Giả lập 1 lần nhấn Start (test qua MCP — không bấm phím được).</summary>
    public void InjectStart()
    {
        startDownLatched = true;
    }

    static bool AnyDown(KeyCode[] keys)
    {
        foreach (var k in keys)
            if (Input.GetKeyDown(k))
                return true;
        return false;
    }

    static bool AnyHeld(KeyCode[] keys)
    {
        foreach (var k in keys)
            if (Input.GetKey(k))
                return true;
        return false;
    }
}
