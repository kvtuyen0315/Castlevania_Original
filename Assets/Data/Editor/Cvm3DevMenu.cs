using UnityEditor;
using UnityEngine;

/// <summary>
/// CVM3 11 — cheat dev (chỉ Editor, đang Play Main.unity; không có trong bản build): bắt đầu THẲNG đầu Stage 07 / 08 / 09 bằng luồng thật
/// (ván mới GameRules.NewGame ⇒ GameSession.stage = N ⇒ GameManager.GoTo(Stage1, 0) ⇒ StageScreen.OnEnter như khi qua bản đồ lâu đài / cửa:
/// TIME 0500 Block 3, điểm đầu stage + hướng đo video — WEA (720,160) quay trái, WalkwayA (48,128) / MummyMan (48,96) quay phải).
/// Không bấm menu thì không đổi gì (tắt mặc định). Dùng chung Cvm2DevMenu.StartStage.
/// </summary>
public static class Cvm3DevMenu
{
    [MenuItem("CVM3/Start Stage 7")]
    static void Stage7() { Debug.Log(Cvm2DevMenu.StartStage(7).Replace("[CVM2]", "[CVM3]")); }

    [MenuItem("CVM3/Start Stage 8")]
    static void Stage8() { Debug.Log(Cvm2DevMenu.StartStage(8).Replace("[CVM2]", "[CVM3]")); }

    [MenuItem("CVM3/Start Stage 9")]
    static void Stage9() { Debug.Log(Cvm2DevMenu.StartStage(9).Replace("[CVM2]", "[CVM3]")); }
}
