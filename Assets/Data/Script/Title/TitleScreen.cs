using UnityEngine;

/// <summary>
/// Màn Title — PLACEHOLDER của routine 04 (routine 05 dựng thật trong chính class/prefab này).
/// Im lặng; bấm Start ⇒ PUSH START KEY nháy 8 f tắt / 8 f bật × 5 ⇒ đen 16 f ⇒ Intro.
/// </summary>
public class TitleScreen : GameScreen
{
    [SerializeField] private NesText pushStart;

    private int blinkFrame = -1;

    public override eGameScreen Id => eGameScreen.Title;

    public override void OnEnter()
    {
        blinkFrame = -1;
        if (pushStart != null)
            pushStart.SetVisible(true);
        if (AudioManager.I != null)
            AudioManager.I.StopMusic();
    }

    public override void OnTick(int localFrame)
    {
        if (blinkFrame < 0)
        {
            if (InputController.I != null && InputController.I.StartPressedThisFrame)
            {
                CvtiDebug.Log("Title: Start");
                blinkFrame = 0;
                SetBlinkVisible();
            }
            return;
        }

        if (++blinkFrame >= NesTiming.TITLE_BLINK_TOTAL)
        {
            GameManager.I.GoTo(eGameScreen.Intro, NesTiming.BLACK_AFTER_TITLE);
            return;
        }
        SetBlinkVisible();
    }

    // Pha chẵn (0–7, 16–23, …) tắt, pha lẻ bật — nháy bắt đầu bằng tắt ngay tick bấm Start.
    void SetBlinkVisible()
    {
        if (pushStart != null)
            pushStart.SetVisible((blinkFrame / NesTiming.TITLE_BLINK_HALF) % 2 == 1);
    }
}
