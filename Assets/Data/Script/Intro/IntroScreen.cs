using UnityEngine;

/// <summary>
/// Cảnh Intro (CVTI_MEMORY §2.4, §4.6). Cắt cứng từ đen: HUD + nền cổng lâu đài (intro_bg tại NES (0,48)).
/// Simon đi từ mép phải vào giữa cổng 1 px / 2 f rồi quay lưng; mây trôi trái qua mặt trăng; 2 dơi nhỏ bay theo bảng
/// điểm đo từ video (xấp xỉ). Nhạc Prologue (không loop) bắt đầu cùng tick bật màn. Hết INTRO_TOTAL ⇒ đen 32 f ⇒ Stage 1.
/// Mọi vị trí tính lại từ localFrame mỗi tick (không cộng dồn) ⇒ tất định, vào lại màn là chạy lại từ đầu. Start bị bỏ qua.
/// </summary>
public class IntroScreen : GameScreen
{
    // Toạ độ NES đo từ video (§2.4). Simon: mép trái sprite 16×32, đỉnh y 178 ⇒ chân (pivot Bottom-Center) ở y 210.
    const int SIMON_START_X = 243;
    const int SIMON_TOP_Y = 178;
    const int SIMON_W = 16;
    const int SIMON_H = 32;
    const int CLOUD_START_X = 222;
    const int CLOUD_Y = 73;

    static readonly int[] WALK_ORDER = { 0, 1, 2, 1 };

    // Dơi nhỏ: (frame, tâm x, tâm y) đo từ video, nội suy tuyến tính giữa các mốc (XẤP XỈ — capture 30 fps, dơi 8×7 px).
    static readonly int[] BAT_A_PATH =
    {
        0, 145, 72,
        60, 145, 72,
        110, 136, 69,
        200, 129, 76,
        246, 125, 75,
        270, 121, 72,
        330, 117, 69,
        390, 115, 66,
        442, 117, 63,
    };
    static readonly int[] BAT_B_PATH =
    {
        NesTiming.INTRO_BAT_B_APPEAR, 31, 121,
        90, 41, 116,
        150, 43, 111,
        166, 49, 108,
        194, 54, 100,
        240, 65, 94,
        270, 70, 89,
        330, 83, 79,
        390, 96, 70,
        430, 107, 63,
        442, 109, 62,
    };
    // Hình dơi chiếm 8×7 trên cùng của sprite 8×16 (pivot Top-Left) ⇒ góc = tâm − (4,3).
    const int BAT_HALF_W = 4;
    const int BAT_HALF_H = 3;

    [Tooltip("Nhạc Prologue — chưa có file (CVTI_MEMORY §5 #3), để trống thì im lặng.")]
    [SerializeField] private AudioClip music;
    [SerializeField] private SpriteRenderer simon;
    [Tooltip("walk_0, walk_1, walk_2 (sheet Simon, quay trái — không flipX).")]
    [SerializeField] private Sprite[] simonWalk = new Sprite[0];
    [SerializeField] private Sprite simonBack;
    [SerializeField] private Transform cloud;
    [SerializeField] private SpriteRenderer batA;
    [SerializeField] private SpriteRenderer batB;
    [Tooltip("intro_bat_0 (cánh chữ v), intro_bat_1 (cánh ngang).")]
    [SerializeField] private Sprite[] batFrames = new Sprite[0];

    public override eGameScreen Id => eGameScreen.Intro;

    public override void OnEnter()
    {
        ShowFrame(0);
        CvtiDebug.Log("Intro: Simon bắt đầu đi");
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(music, false);
    }

    public override void OnTick(int localFrame)
    {
        if (localFrame >= NesTiming.INTRO_TOTAL)
        {
            GameManager.I.GoTo(eGameScreen.Stage1, NesTiming.BLACK_AFTER_INTRO);
            return;
        }
        if (localFrame == NesTiming.INTRO_SIMON_WALK)
            CvtiDebug.Log("Intro: Simon dừng, quay lưng");
        ShowFrame(localFrame);
    }

    void ShowFrame(int f)
    {
        UpdateSimon(f);
        if (cloud != null)
        {
            int x = CLOUD_START_X - (f + NesTiming.INTRO_CLOUD_PHASE) / NesTiming.INTRO_CLOUD_STEP;
            cloud.localPosition = NesCoord.ToWorld(x, CLOUD_Y);
        }
        UpdateBat(batA, BAT_A_PATH, f, 0);
        UpdateBat(batB, BAT_B_PATH, f, NesTiming.INTRO_BAT_FLAP);
    }

    void UpdateSimon(int f)
    {
        if (simon == null)
            return;
        int x;
        if (f < NesTiming.INTRO_SIMON_WALK)
        {
            x = SIMON_START_X - f / NesTiming.INTRO_SIMON_STEP;
            int pose = WALK_ORDER[(f + NesTiming.INTRO_SIMON_POSE_PHASE) / NesTiming.INTRO_SIMON_POSE % WALK_ORDER.Length];
            if (pose < simonWalk.Length)
                simon.sprite = simonWalk[pose];
        }
        else
        {
            x = SIMON_START_X - NesTiming.INTRO_SIMON_WALK / NesTiming.INTRO_SIMON_STEP;
            simon.sprite = simonBack;
        }
        simon.transform.localPosition = NesCoord.ToWorld(x + SIMON_W / 2, SIMON_TOP_Y + SIMON_H);
    }

    // flapPhase lệch nhịp vỗ cánh giữa 2 dơi để không đập đồng loạt.
    void UpdateBat(SpriteRenderer bat, int[] path, int f, int flapPhase)
    {
        if (bat == null)
            return;
        bool visible = f >= path[0];
        bat.enabled = visible;
        if (!visible)
            return;

        int i = 0;
        while (i + 3 < path.Length && f >= path[i + 3])
            i += 3;
        int cx = path[i + 1], cy = path[i + 2];
        if (i + 3 < path.Length)
        {
            float t = (float)(f - path[i]) / (path[i + 3] - path[i]);
            cx = Mathf.RoundToInt(Mathf.Lerp(path[i + 1], path[i + 4], t));
            cy = Mathf.RoundToInt(Mathf.Lerp(path[i + 2], path[i + 5], t));
        }
        bat.transform.localPosition = NesCoord.ToWorld(cx - BAT_HALF_W, cy - BAT_HALF_H);
        if (batFrames.Length > 0)
            bat.sprite = batFrames[(f + flapPhase) / NesTiming.INTRO_BAT_FLAP % batFrames.Length];
    }
}
