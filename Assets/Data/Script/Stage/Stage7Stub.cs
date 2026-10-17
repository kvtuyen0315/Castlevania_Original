using UnityEngine;

/// <summary>
/// STAGE 07 stub (CVM2 11 — thay Stage4Stub cuối luồng): sau bản đồ lâu đài Block 2 (video Map_2 4:19.73–4:20.17 đen 14 frame ⇒ 27 tick dùng
/// chung Map 1) — HUD STAGE 07, TIME 0500 (ConfigBlock[3].timeLimit — video Map_2 4:20.20), tim 05, ENEMY đầy, nền đen. Map Block 3
/// (Stage 07–09) là cụm sau (CVM2_MEMORY §6 "chờ cụm Map 3"). Nhạc = ConfigStage[7].music (Wicked Child — video Map_2 4:20.15, bằng chứng yếu).
/// </summary>
public class Stage7Stub : GameScreen
{
    public const int STAGE = 7;

    public override eGameScreen Id => eGameScreen.Stage7Stub;

    public override void OnEnter()
    {
        GameRules.EnterStage(STAGE, true);
        var cfg = GameConfig.Current;
        var row = cfg != null ? cfg.GetStage(STAGE) : null;
        var id = row != null ? row.music : eMusic.WickedChild;
        CvtiDebug.Log("Stage7Stub: STAGE 07 (chờ cụm Map 3) — TIME " + GameSession.Current.time.ToString("0000") + ", nhạc " + id);
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(id);
    }
}
