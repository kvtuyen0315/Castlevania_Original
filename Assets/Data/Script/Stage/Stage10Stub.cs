using UnityEngine;

/// <summary>
/// STAGE 10 stub (CVM3 11 — thay Stage7Stub cuối luồng): sau bản đồ lâu đài Block 3 (đen 27 tick — dùng chung Map 1/2, SUY LUẬN: video Map_3
/// hết giữa lúc bản đồ B3 trượt). HUD STAGE 10, tim 05 (GameRules.StartNextBlock — video Map_3 f7464), ENEMY đầy, nền đen. Map Block 4
/// (Stage 10–12) là cụm sau (CVM3_MEMORY §6 "chờ cụm Map 4"). TIME = ConfigBlock[4].timeLimit — CHƯA CÓ SỐ (wiki không nêu, chưa có video
/// Map_4) ⇒ giữ TIME đang hiện (0000 sau tally — như HUD bản đồ B3 video) và ghi log, KHÔNG gọi CfgInt.Get (tránh lỗi console). Nhạc =
/// ConfigStage[10].music (Walking on the Edge — suy luận tên file).
/// </summary>
public class Stage10Stub : GameScreen
{
    public const int STAGE = 10;

    public override eGameScreen Id => eGameScreen.Stage10Stub;

    public override void OnEnter()
    {
        var cfg = GameConfig.Current;
        var block = cfg != null ? cfg.GetBlockOfStage(STAGE) : null;
        bool hasTime = block != null && block.timeLimit.has;
        if (hasTime)
            GameRules.EnterStage(STAGE, true);
        else
        {
            // như GameRules.EnterStage nhưng giữ TIME (giới hạn Block 4 chưa biết)
            var s = GameSession.Current;
            s.stage = STAGE;
            s.block = GameRules.BlockOfStage(STAGE);
            s.enemyHp = s.shownEnemyHp = GameSession.MAX_HP;
            s.NotifyChanged();
        }
        var row = cfg != null ? cfg.GetStage(STAGE) : null;
        var id = row != null ? row.music : eMusic.WalkingOnTheEdge;
        CvtiDebug.Log("Stage10Stub: STAGE 10 (chờ cụm Map 4) — TIME " + GameSession.Current.time.ToString("0000")
                      + (hasTime ? "" : " (ConfigBlock[4].timeLimit chưa có số — giữ)") + ", nhạc " + id);
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(id);
    }
}
