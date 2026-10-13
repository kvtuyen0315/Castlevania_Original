using UnityEngine;

/// <summary>
/// STAGE 04 stub (CVGP 15) — sau bản đồ lâu đài: HUD (STAGE 04, TIME 0400 — giờ Block 2, ENEMY đầy — video Map_1 3:59.2 f7177),
/// nền đen. Map Block 2 (Stage 04–06) là cụm sau (CVGP_MEMORY §6 "chờ cụm Map 2"). Nhạc Stalker chưa import ⇒ im lặng + log.
/// </summary>
public class Stage4Stub : GameScreen
{
    [Tooltip("04. Stalker (Armory) — chưa import, để trống ⇒ im lặng.")]
    [SerializeField] private AudioClip music;

    public override eGameScreen Id => eGameScreen.Stage4Stub;

    public override void OnEnter()
    {
        GameRules.EnterStage(4, true);
        CvtiDebug.Log("Stage4Stub: STAGE 04 (chờ cụm Map 2) — nhạc Stalker" + (music == null ? " chưa import" : ""));
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(music, true);
    }
}
