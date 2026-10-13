using UnityEngine;

/// <summary>
/// Màn chơi Map 1 (CVGP 15) — thay Stage1Stub (giữ id eGameScreen.Stage1: Intro → đen 32 → đây; GAME OVER → CONTINUE → đây).
/// Prefab Assets/Data/Prefabs/Screens/StageScreen.prefab (menu CVGP/Build Map1 Screens): HUD (follower camera) + StageWorld
/// (StageWorld + NesCameraController + StageRules + StageFlow + CvgpDebugOverlay; con: 5 section Block 1, mỗi section có lớp vật thể
/// &lt;Section&gt;_Objects (CVGP/Build Map1 Objects), Entities/Simon + cánh cửa).
/// OnEnter: stage = GameSession.stage (1–3) ⇒ section + điểm đầu stage (StageRules.stageStarts — đo video), STAGE/ENEMY
/// (GameRules.EnterStage), khởi tạo thế giới ngay để Simon đứng đúng chỗ ở khung hình đầu (H8: Stage 01 ⇒ góc sprite (42,161) quay phải),
/// nhạc stage. Mọi logic gameplay chạy trong StageWorld (NesClock.OnTick) — màn này không làm gì mỗi tick.
/// </summary>
public class StageScreen : GameScreen
{
    [SerializeField] private StageWorld world;
    [SerializeField] private StageRules rules;
    [SerializeField] private StageFlow flow;

    public override eGameScreen Id => eGameScreen.Stage1;
    public StageWorld World => world;
    public StageRules Rules => rules;
    public StageFlow Flow => flow;

    public override void OnEnter()
    {
        var s = GameSession.Current;
        int stage = Mathf.Clamp(s.stage, 1, 3);
        LevelSection sec;
        Vector2Int pos;
        rules.GetStageStart(stage, out sec, out pos);
        world.Stage = stage;
        if (sec != null)
            world.StartSection = sec;
        GameRules.EnterStage(stage, false);
        world.EnsureInit();
        var simon = world.Player as SimonController;
        if (simon != null)
        {
            simon.Teleport(pos.x, pos.y, 1);
            if (world.CameraCtl != null)
            {
                world.CameraCtl.Target = simon;
                world.CameraCtl.SnapToTarget();
            }
        }
        rules.ResetClockPhase();
        flow.PlayStageMusic();
        CvtiDebug.Log("StageScreen: STAGE " + stage.ToString("00") + " — " + (sec != null ? sec.name : "?") + " (" + pos.x + "," + pos.y + ")");
    }
}
