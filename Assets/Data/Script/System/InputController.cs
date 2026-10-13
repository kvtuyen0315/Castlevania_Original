using afsdk;
using UnityEngine;

/// <summary>
/// Lớp mỏng cho màn Title/Intro (CVTI) — đọc Start từ NesPad (bảng phím InputBindings: B, Enter, KeypadEnter, Space,
/// joystick button 7, 9). Không đọc Input.* nữa (CVGP 03); NesPad.Tick() do GameManager gọi đầu mỗi tick, trước luồng màn hình.
/// </summary>
public class InputController : ManualSingletonMono<InputController>
{
    /// <summary>Start vừa được nhấn ở tick hiện tại (chỉ 1 tick, giữ phím không lặp).</summary>
    public bool StartPressedThisFrame => NesPad.I != null && NesPad.I.Pressed(eNesButton.Start);

    public bool StartHeld => NesPad.I != null && NesPad.I.Held(eNesButton.Start);

    /// <summary>Giả lập 1 lần nhấn Start ở tick kế tiếp (harness CVTI qua MCP — không bấm phím được).</summary>
    public void InjectStart()
    {
        if (NesPad.I != null)
            NesPad.I.InjectPress(eNesButton.Start);
        else
            Debug.LogError("[InputController] Không có NesPad trong scene");
    }
}
