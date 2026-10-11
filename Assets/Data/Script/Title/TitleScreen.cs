using UnityEngine;

/// <summary>
/// Màn Title (CVTI_MEMORY §2, §4.4). Cắt cứng từ đen, im lặng. Nền title_base + 3 dòng copyright (NesText) đứng yên;
/// tháp + dơi = 40 khung sheet ghép sẵn tại NES (184,104): 37 khung bay one-shot rồi lơ lửng 37-38-39-38 lặp.
/// PUSH START KEY đứng yên tới khi bấm Start ⇒ nháy 8 f tắt / 8 f bật × 5 ⇒ đen 16 f ⇒ Intro. Dơi vẫn vỗ cánh khi nháy.
/// </summary>
public class TitleScreen : GameScreen
{
    // Thứ tự tư thế lơ lửng đo từ video (§2.3): 37 → 38 → 39 → 38 → lặp.
    static readonly int[] HOVER_ORDER = { 37, 38, 39, 38 };

    [SerializeField] private SpriteRenderer pushStart;
    [SerializeField] private FrameAnimator bat;
    [Tooltip("title_bat_00..39 theo thứ tự sheet.")]
    [SerializeField] private Sprite[] batFrames = new Sprite[0];

    private Sprite[] flight;
    private int[] flightDurations;
    private Sprite[] hover;
    private int blinkFrame = -1;

    public override eGameScreen Id => eGameScreen.Title;

    private void Awake()
    {
        int n = Mathf.Min(NesTiming.TITLE_BAT_FLIGHT_FRAMES, batFrames.Length);
        flight = new Sprite[n];
        flightDurations = new int[n];
        for (int i = 0; i < n; i++)
        {
            flight[i] = batFrames[i];
            flightDurations[i] = i % 2 == 0 ? NesTiming.TITLE_BAT_FLIGHT_SHORT : NesTiming.TITLE_BAT_FLIGHT_LONG;
        }

        hover = new Sprite[HOVER_ORDER.Length];
        for (int i = 0; i < hover.Length; i++)
            hover[i] = HOVER_ORDER[i] < batFrames.Length ? batFrames[HOVER_ORDER[i]] : null;
    }

    public override void OnEnter()
    {
        blinkFrame = -1;
        if (pushStart != null)
            pushStart.enabled = true;
        if (bat != null)
            bat.Play(flight, flightDurations, false, StartHover);
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

    // Gọi trong tick khung bay cuối hết thời lượng ⇒ tư thế lơ lửng đầu hiện ngay tick đó.
    void StartHover()
    {
        CvtiDebug.Log("Title: dơi lơ lửng");
        bat.Play(hover, NesTiming.TITLE_BAT_HOVER_POSE, true);
    }

    // Pha chẵn (0–7, 16–23, …) tắt, pha lẻ bật — nháy bắt đầu bằng tắt ngay tick bấm Start.
    void SetBlinkVisible()
    {
        if (pushStart != null)
            pushStart.enabled = (blinkFrame / NesTiming.TITLE_BLINK_HALF) % 2 == 1;
    }
}
