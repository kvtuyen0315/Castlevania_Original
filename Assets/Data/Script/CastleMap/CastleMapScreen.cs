using UnityEngine;

/// <summary>
/// Bản đồ lâu đài sau Block 1 (CVGP 15 — video Map_1 3:52.3–3:58.7, f6956–7177; số đo Map1FlowTiming):
///   OnEnter: GameRules.StartNextBlock (STAGE 04, tim 05; TIME 0000 + ENEMY rỗng giữ như video). Bản đồ (castlemap_bg 384×144, góc
///   trên-trái y 48) trượt vào từ x 256 sang trái 1.5 px/tick tới x 0 ⇒ dơi tím (boss Block 1 đã hạ) ở ô trái nháy bat_0/bat_1 mỗi
///   8 tick, chấm tím ô kế hiện cùng pha bat_1; Simon (sprite thường, quay phải) hiện ở x 16, đi 1 px/tick (khung walk_0,1,2,1 × 7 tick)
///   tới x 240 (SFX MapStep mỗi 16 tick) ⇒ đen 27 tick ⇒ STAGE 04 stub. Không nhạc (video chỉ có tiếng bước).
///   Vị trí tính lại từ localFrame mỗi tick (như IntroScreen) ⇒ tất định, vào lại là chạy lại từ đầu.
/// </summary>
public class CastleMapScreen : GameScreen
{
    static readonly int[] WALK_ORDER = { 0, 1, 2, 1 };

    [SerializeField] private SpriteRenderer map;
    [SerializeField] private SpriteRenderer bat;
    [SerializeField] private SpriteRenderer marker;
    [SerializeField] private SpriteRenderer simon;
    [Tooltip("castlemap_bat_0, castlemap_bat_1")]
    [SerializeField] private Sprite[] batFrames = new Sprite[0];
    [Tooltip("walk_0, walk_1, walk_2 (Character/Simon — quay trái, lật khi vẽ)")]
    [SerializeField] private Sprite[] simonWalk = new Sprite[0];

    private int stopFrame;
    private bool left;

    public override eGameScreen Id => eGameScreen.CastleMap;

    /// <summary>Tick (localFrame) bản đồ dừng ở x 0.</summary>
    public int StopFrame => stopFrame;

    public override void OnEnter()
    {
        GameRules.StartNextBlock();
        if (AudioManager.I != null)
            AudioManager.I.StopMusic();
        // bản đồ chạm x 0 ở tick đầu tiên có 256·256 ≤ 384·t
        stopFrame = (NesCoord.SCREEN_W * Fixed.ONE + Map1FlowTiming.MAP_SLIDE_SPEED_FIXED - 1) / Map1FlowTiming.MAP_SLIDE_SPEED_FIXED;
        left = false;
        CvtiDebug.Log("CastleMap: bản đồ trượt vào (dừng sau " + stopFrame + " tick)");
        ShowFrame(0);
    }

    public override void OnTick(int localFrame)
    {
        if (left)
            return;
        ShowFrame(localFrame);
        int t = localFrame - stopFrame;
        if (t >= Map1FlowTiming.MAP_STEP_SFX_FIRST && (t - Map1FlowTiming.MAP_STEP_SFX_FIRST) % Map1FlowTiming.MAP_STEP_SFX_EVERY == 0
            && SimonX(t) < Map1FlowTiming.MAP_SIMON_END_X && AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.MapStep);
        if (t >= 0 && SimonX(t) >= Map1FlowTiming.MAP_SIMON_END_X)
        {
            left = true;
            CvtiDebug.Log("CastleMap: Simon tới x " + Map1FlowTiming.MAP_SIMON_END_X + " ⇒ STAGE 04");
            GameManager.I.GoTo(eGameScreen.Stage4Stub, Map1FlowTiming.MAP_TO_STAGE4_BLACK_TICKS);
        }
    }

    static int SimonX(int t)
    {
        return Map1FlowTiming.MAP_SIMON_START_X + t;
    }

    void ShowFrame(int f)
    {
        int dx = Mathf.Max(0, NesCoord.SCREEN_W - Fixed.ToPx(f * Map1FlowTiming.MAP_SLIDE_SPEED_FIXED));
        if (map != null)
            map.transform.localPosition = NesCoord.ToWorld(dx, Map1FlowTiming.MAP_Y);
        int t = f - stopFrame;
        bool stopped = t >= 0;
        bool phase1 = stopped && (t / Map1FlowTiming.MAP_BLINK_HALF_TICKS) % 2 == 1;
        if (bat != null)
        {
            bat.enabled = stopped && batFrames.Length >= 2;
            if (bat.enabled)
                bat.sprite = batFrames[phase1 ? 1 : 0];
        }
        if (marker != null)
            marker.enabled = phase1;
        if (simon != null)
        {
            simon.enabled = stopped;
            if (stopped && simonWalk.Length >= 3)
            {
                int x = Mathf.Min(SimonX(t), Map1FlowTiming.MAP_SIMON_END_X);
                simon.sprite = simonWalk[WALK_ORDER[(t / Map1FlowTiming.MAP_SIMON_FRAME_TICKS) % WALK_ORDER.Length]];
                simon.flipX = true;
                // pivot đáy-giữa vẽ ở Y + 1 như Simon trong màn chơi (hàng đáy sprite trong suốt — CVTI §4.6)
                simon.transform.localPosition = NesCoord.ToWorld(x, Map1FlowTiming.MAP_SIMON_Y + 1);
            }
        }
    }
}
