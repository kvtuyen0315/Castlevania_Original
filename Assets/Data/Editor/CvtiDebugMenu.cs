using UnityEditor;

/// <summary>Bật/tắt log mốc frame [CVTI] (EditorPrefs, áp dụng từ lần vào Play kế tiếp).</summary>
public static class CvtiDebugMenu
{
    const string MENU = "CVTI/Log Flow Frames";

    [MenuItem(MENU)]
    static void Toggle()
    {
        EditorPrefs.SetBool(CvtiDebug.PREF_KEY, !EditorPrefs.GetBool(CvtiDebug.PREF_KEY, false));
    }

    [MenuItem(MENU, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MENU, EditorPrefs.GetBool(CvtiDebug.PREF_KEY, false));
        return true;
    }
}
