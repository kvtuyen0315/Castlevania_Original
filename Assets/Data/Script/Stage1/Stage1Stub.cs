using UnityEngine;

/// <summary>
/// Stage 1 stub — điểm cuối của cụm CVTI (CVTI_MEMORY §4.6): HUD + Simon đứng quay phải tại góc (42,161) như video 16.4 s.
/// Chưa có sheet Stage 1 ⇒ nền đen; gameplay (đếm giờ, điều khiển, roi…) là cụm sau. Nhạc Vampire Killer (loop) khi có file.
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
