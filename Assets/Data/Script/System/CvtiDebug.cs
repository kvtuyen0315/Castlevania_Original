using UnityEngine;

/// <summary>
/// Cờ log mốc frame (routine 08 dùng đo timing). Bật bằng GameManager.logFlow trên prefab
/// hoặc menu CVTI/Log Flow Frames (EditorPrefs, không đổi asset). Tắt thì không log gì.
/// </summary>
public static class CvtiDebug
{
    public const string PREF_KEY = "CVTI.LogFlow";

    public static bool LogFlow { get; set; }

    public static void Log(string message)
    {
        if (LogFlow)
            Debug.Log("[CVTI] frame " + NesClock.Frame + " (t=" + Time.unscaledTime.ToString("0.000") + "s): " + message);
    }
}
