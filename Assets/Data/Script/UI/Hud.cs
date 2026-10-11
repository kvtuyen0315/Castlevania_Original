using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD dùng chung Intro + Stage 1 (CVTI_MEMORY §4.5). Canvas World Space 256×48, scale 1/16 ⇒ 1 đơn vị canvas = 1 px NES,
/// chữ TextMeshProUGUI font "NesFont TMP" cỡ 8. Là con của prefab màn nên tắt cùng màn khi cắt đen.
/// Bố cục đo từ video: dòng trạng thái (0,16); PLAYER (0,24) + 16 vạch từ x 56 bước 4; ENEMY (0,32);
/// khung vũ khí phụ (128,24); tim (168,24) + "-05" (176,24); "P-03" (168,32).
/// </summary>
public class Hud : MonoBehaviour
{
    [SerializeField] private TMP_Text statusLine;
    [SerializeField] private TMP_Text heartsText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private Image[] playerTicks = new Image[0];
    [SerializeField] private Image[] enemyTicks = new Image[0];

    private void OnEnable()
    {
        GameSession.Current.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameSession.Current.Changed -= Refresh;
    }

    public void Refresh()
    {
        var s = GameSession.Current;
        if (statusLine != null)
            statusLine.SetText("SCORE-" + s.score.ToString("000000") + " TIME " + s.time.ToString("0000") + " STAGE " + s.stage.ToString("00"));
        if (heartsText != null)
            heartsText.SetText("-" + s.hearts.ToString("00"));
        if (livesText != null)
            livesText.SetText("P-" + s.lives.ToString("00"));
        SetTicks(playerTicks, s.playerHp);
        SetTicks(enemyTicks, s.enemyHp);
    }

    // Vạch đã mất = không vẽ (nền HUD đen).
    static void SetTicks(Image[] ticks, int hp)
    {
        for (int i = 0; i < ticks.Length; i++)
            if (ticks[i] != null)
                ticks[i].enabled = i < hp;
    }
}
