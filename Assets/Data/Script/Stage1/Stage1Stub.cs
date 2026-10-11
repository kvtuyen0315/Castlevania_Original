using UnityEngine;

/// <summary>
/// Stage 1 stub — PLACEHOLDER của routine 04 (routine 07 nối nền + HUD + nhạc). Điểm cuối của cụm CVTI.
/// </summary>
public class Stage1Stub : GameScreen
{
    [Tooltip("Nhạc Vampire Killer (loop) — chưa có file (CVTI_MEMORY §5 #3), để trống thì im lặng.")]
    [SerializeField] private AudioClip music;

    public override eGameScreen Id => eGameScreen.Stage1;

    public override void OnEnter()
    {
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(music, true);
    }
}
