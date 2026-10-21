using UnityEngine;

/// <summary>
/// Màn chơi Map 1 + Map 2 + Map 3 (CVGP 15; CVM2 11 mở rộng Block 2; CVM3 11 mở rộng Block 3) — thay Stage1Stub (giữ id eGameScreen.Stage1:
/// Intro → đen 32 → đây; GAME OVER → CONTINUE → đây; bản đồ lâu đài sau Block 1 / Block 2 → đen 27 → đây với GameSession.stage = 4 / 7).
/// Prefab Assets/Data/Prefabs/Screens/StageScreen.prefab (menu CVGP/Build Map1 Screens = CVM2/Build Map2 Screens): HUD (follower camera) +
/// StageWorld (StageWorld + NesCameraController + StageRules + StageFlow + CvgpDebugOverlay; con: 5 section Block 1 + 6 section Block 2
/// + 5 section Block 3, mỗi section có lớp vật thể &lt;Section&gt;_Objects (CVGP/Build Map1 Objects, CVM2/Build Map2 Objects, CVM3/Build Map3
/// Objects), Entities/Simon + cánh cửa).
/// OnEnter: stage = GameSession.stage (1–9) ⇒ section + điểm đầu stage + hướng (StageRules.stageStarts — đo video; Block 2 Hall A/C quay
/// TRÁI; Block 3 WEA (720,160) quay TRÁI, WalkwayA (48,128) / MummyMan (48,96) phải), STAGE/ENEMY (GameRules.EnterStage — sang Block 2 ⇒
/// TIME 0400, Block 3 ⇒ 0500), khởi tạo thế giới ngay để Simon đứng đúng chỗ ở khung hình đầu (H8: Stage 01 ⇒ góc sprite (42,161) quay phải;
/// Map_2 0:06.23: TEA cx 0, Simon (64,176) quay phải; Map_3 0:06.90: WEA cx 512, Simon (720,160) quay trái), nhạc stage (Stalker Block 2,
/// Wicked Child Block 3), pha TIME (Block 3: giảm đầu sau 60 tick — Map3FlowTiming.TIME_FIRST_STAGE; Block 1/2 luật 64).
/// Mọi logic gameplay chạy trong StageWorld (NesClock.OnTick) — màn này không làm gì mỗi tick.
/// </summary>
public class StageScreen : GameScreen
{
    /// <summary>CVM2 11: stage cuối có màn chơi thật (Block 1–2: 6); CVM3 11: Block 1–3 = 9 — sau đó bản đồ lâu đài ⇒ Stage10Stub.</summary>
    public const int LAST_PLAYABLE_STAGE = 9;

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
        int stage = Mathf.Clamp(s.stage, 1, LAST_PLAYABLE_STAGE);
        LevelSection sec;
        Vector2Int pos;
        int facing;
        rules.GetStageStart(stage, out sec, out pos, out facing);
        world.Stage = stage;
        if (sec != null)
            world.StartSection = sec;
        GameRules.EnterStage(stage, false);
        world.EnsureInit();
        var simon = world.Player as SimonController;
        if (simon != null)
        {
            simon.Teleport(pos.x, pos.y, facing);
            if (world.CameraCtl != null)
            {
                world.CameraCtl.Target = simon;
                world.CameraCtl.SnapToTarget();
            }
        }
        if (Map3FlowTiming.AppliesTo(stage))
            rules.ResetClockPhase(Map3FlowTiming.TIME_FIRST_STAGE);
        else
            rules.ResetClockPhase();
        flow.PlayStageMusic();
        CvtiDebug.Log("StageScreen: STAGE " + stage.ToString("00") + " — " + (sec != null ? sec.name : "?") + " (" + pos.x + "," + pos.y
                      + (facing < 0 ? ", quay trái" : "") + ")");
    }
}
