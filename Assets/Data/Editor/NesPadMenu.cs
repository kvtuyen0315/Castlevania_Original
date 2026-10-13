using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menu CVGP/Input/*: ghi replay trong Play (Record → Stop & Save vào Assets/Data/Tests/Replays/*.txt)
/// và bật/tắt Debug Overlay (EditorPrefs, có hiệu lực ngay — để người dùng tự kiểm từng nút gamepad).
/// </summary>
public static class NesPadMenu
{
    const string MENU_RECORD = "CVGP/Input/Record";
    const string MENU_STOP = "CVGP/Input/Stop & Save";
    const string MENU_OVERLAY = "CVGP/Input/Debug Overlay";
    public const string REPLAY_DIR = "Assets/Data/Tests/Replays";

    [MenuItem(MENU_RECORD)]
    static void Record()
    {
        NesPad.I.StartRecord();
        Debug.Log("[NesPad] Bắt đầu ghi input từ tick " + (NesClock.Frame + 1));
    }

    [MenuItem(MENU_RECORD, true)]
    static bool RecordValidate()
    {
        return Application.isPlaying && NesPad.I != null && !NesPad.I.IsRecording;
    }

    [MenuItem(MENU_STOP)]
    static void StopAndSave()
    {
        var ticks = NesPad.I.StopRecord();
        var path = SaveReplay(ticks, "rec_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Debug.Log("[NesPad] Đã lưu " + ticks.Count + " tick vào " + path);
    }

    [MenuItem(MENU_STOP, true)]
    static bool StopValidate()
    {
        return Application.isPlaying && NesPad.I != null && NesPad.I.IsRecording;
    }

    /// <summary>Ghi replay ra Assets/Data/Tests/Replays/name.txt (TextAsset), trả đường dẫn asset.</summary>
    public static string SaveReplay(System.Collections.Generic.IList<int> ticks, string name)
    {
        Directory.CreateDirectory(REPLAY_DIR);
        var path = REPLAY_DIR + "/" + name + ".txt";
        var text = "# NesPad replay (định dạng: NesReplay.cs) — " + ticks.Count + " tick, hash "
                   + NesReplay.Hash(ticks).ToString("x8") + "\n" + NesReplay.Encode(ticks) + "\n";
        File.WriteAllText(path, text);
        AssetDatabase.ImportAsset(path);
        return path;
    }

    [MenuItem(MENU_OVERLAY)]
    static void ToggleOverlay()
    {
        bool on = !EditorPrefs.GetBool(NesPad.PREF_OVERLAY, false);
        EditorPrefs.SetBool(NesPad.PREF_OVERLAY, on);
        NesPad.ShowOverlay = on;
    }

    [MenuItem(MENU_OVERLAY, true)]
    static bool ToggleOverlayValidate()
    {
        Menu.SetChecked(MENU_OVERLAY, EditorPrefs.GetBool(NesPad.PREF_OVERLAY, false));
        return true;
    }
}
