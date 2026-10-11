using UnityEngine;

/// <summary>
/// Cảnh Intro — PLACEHOLDER của routine 04 (routine 07 dựng thật trong chính class/prefab này).
/// Nhạc Prologue (không loop) bắt đầu cùng tick bật màn; hết INTRO_TOTAL frame ⇒ đen 32 f ⇒ Stage 1. Start bị bỏ qua.
/// </summary>
public class IntroScreen : GameScreen
{
    [Tooltip("Nhạc Prologue — chưa có file (CVTI_MEMORY §5 #3), để trống thì im lặng.")]
    [SerializeField] private AudioClip music;

    public override eGameScreen Id => eGameScreen.Intro;

    public override void OnEnter()
    {
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(music, false);
    }

    public override void OnTick(int localFrame)
    {
        if (localFrame >= NesTiming.INTRO_TOTAL)
            GameManager.I.GoTo(eGameScreen.Stage1, NesTiming.BLACK_AFTER_INTRO);
    }
}
