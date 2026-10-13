using System;
using UnityEngine;

/// <summary>
/// Bảng phím của NesPad (asset Assets/Data/Config/InputBindings.asset — nguồn DUY NHẤT của phím, CVGP_PLAN §6).
/// Đổi phím = sửa asset, không sửa code. Gamepad đọc bằng KeyCode.JoystickButtonN (mọi tay) + 4 trục CV_* trong InputManager.
/// </summary>
[CreateAssetMenu(fileName = "InputBindings", menuName = "CVGP/InputBindings")]
public class InputBindings : ScriptableObject
{
    [Serializable]
    public struct ButtonKeys
    {
        public eNesButton button;
        public KeyCode[] keys;
    }

    [Tooltip("Phím bàn phím + JoystickButtonN cho từng nút.")]
    public ButtonKeys[] buttons = new ButtonKeys[0];

    [Header("Trục (tên trong ProjectSettings/InputManager.asset, dương = Phải/Lên)")]
    public string dpadAxisX;
    public string dpadAxisY;
    public string stickAxisX;
    public string stickAxisY;

    [Tooltip("|giá trị trục| >= ngưỡng ⇒ coi như giữ hướng (D-pad ra -1/0/+1).")]
    [Range(0.1f, 0.95f)] public float dpadThreshold = 0.5f;
    [Range(0.1f, 0.95f)] public float stickThreshold = 0.5f;

    public KeyCode[] GetKeys(eNesButton b)
    {
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i].button == b)
                return buttons[i].keys ?? new KeyCode[0];
        return new KeyCode[0];
    }
}
