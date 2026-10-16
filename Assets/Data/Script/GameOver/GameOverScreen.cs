using UnityEngine;

/// <summary>
/// Màn GAME OVER (CVGP 12). Video/wiki không có cảnh Game Over (wiki chỉ nói hết mạng thì được Continue) ⇒ THIẾT KẾ TỐI THIỂU (quy ước,
/// CVGP_MEMORY §4): chữ "GAME OVER" + 2 lựa chọn "CONTINUE" / "END" (NesText, layer HUD), con trỏ = sprite tim nhỏ (Item/heart_small —
/// sheet gốc). ↑/↓ hoặc Select đổi lựa chọn, Start chọn: CONTINUE ⇒ GameRules.Continue rồi về màn chơi (đầu stage đang chơi);
/// END ⇒ GameRules.NewGame rồi về Title. Không giới hạn số lần Continue (wiki không nêu). Nhạc Game Over (CVM2 03 — suy luận: không có video Game Over).
/// </summary>
public class GameOverScreen : GameScreen
{
    [SerializeField] private SpriteRenderer cursor;
    [Tooltip("NES y (góc trên) của từng lựa chọn — con trỏ đặt tại (cursorX, y).")]
    [SerializeField] private int[] optionY = { 128, 144 };
    [SerializeField] private int cursorX = 84;
    [SerializeField] private eMusic musicId = eMusic.GameOver;

    public int Selection { get; private set; }
    public bool Chosen { get; private set; }

    public override eGameScreen Id => eGameScreen.GameOver;

    public override void OnEnter()
    {
        Selection = 0;
        Chosen = false;
        PlaceCursor();
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(musicId);
        CvtiDebug.Log("GameOver: hiện (P-" + GameSession.Current.lives.ToString("00") + ", SCORE " + GameSession.Current.score + ")");
    }

    public override void OnTick(int localFrame)
    {
        var pad = NesPad.I;
        if (pad == null || Chosen)
            return;
        if (pad.Pressed(eNesButton.Up) || pad.Pressed(eNesButton.Down) || pad.Pressed(eNesButton.Select))
        {
            Selection = 1 - Selection;
            PlaceCursor();
        }
        if (!pad.Pressed(eNesButton.Start))
            return;
        Chosen = true;
        var cfg = GameConfig.Current;
        int black = cfg != null ? cfg.rules.GetInt(eRuleParam.GameOverBlackTicks, 32) : 32;
        if (Selection == 0)
        {
            GameRules.Continue();
            CvtiDebug.Log("GameOver: CONTINUE ⇒ STAGE " + GameSession.Current.stage.ToString("00"));
            GameManager.I.GoTo(eGameScreen.Stage1, black);
        }
        else
        {
            GameRules.NewGame();
            CvtiDebug.Log("GameOver: END ⇒ Title");
            GameManager.I.GoTo(eGameScreen.Title, black);
        }
    }

    void PlaceCursor()
    {
        if (cursor == null || optionY.Length == 0)
            return;
        int y = optionY[Mathf.Clamp(Selection, 0, optionY.Length - 1)];
        // heart_small 8×16, pivot đáy-giữa (4,16), hình ở 8 hàng trên ⇒ góc trên-trái (cursorX, y)
        cursor.transform.localPosition = NesCoord.ToWorld(cursorX + 4, y + 16);
    }
}
