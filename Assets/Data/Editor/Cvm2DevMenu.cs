using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM2 11 — cheat dev (chỉ Editor, đang Play Main.unity; không có trong bản build): bắt đầu THẲNG đầu Stage 04 / 05 / 06 bằng luồng thật
/// (ván mới GameRules.NewGame ⇒ GameSession.stage = N ⇒ GameManager.GoTo(Stage1, 0) ⇒ StageScreen.OnEnter như khi qua bản đồ lâu đài / cửa:
/// TIME 0400 Block 2, điểm đầu stage + hướng đo video). Không bấm menu thì không đổi gì (tắt mặc định).
/// </summary>
public static class Cvm2DevMenu
{
    [MenuItem("CVM2/Start Stage 4")]
    static void Stage4() { Debug.Log(StartStage(4)); }

    [MenuItem("CVM2/Start Stage 5")]
    static void Stage5() { Debug.Log(StartStage(5)); }

    [MenuItem("CVM2/Start Stage 6")]
    static void Stage6() { Debug.Log(StartStage(6)); }

    public static string StartStage(int stage)
    {
        if (!EditorApplication.isPlaying || GameManager.I == null)
            return "[CVM2] cần Play Main.unity";
        if (GameManager.I.IsTransitioning)
            return "[CVM2] đang chuyển cảnh — thử lại";
        GameRules.NewGame();
        var s = GameSession.Current;
        s.stage = Mathf.Clamp(stage, 1, StageScreen.LAST_PLAYABLE_STAGE);
        s.NotifyChanged();
        return GameManager.I.GoTo(eGameScreen.Stage1, 0) ? "[CVM2] bắt đầu STAGE " + s.stage.ToString("00") : "[CVM2] GoTo lỗi";
    }
}
