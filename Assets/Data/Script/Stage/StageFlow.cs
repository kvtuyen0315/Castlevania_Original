using System;
using UnityEngine;

/// <summary>
/// Luồng giữa các section/stage của Map 1 (CVGP 15) — component cạnh StageWorld + StageRules trong màn chơi (StageScreen).
/// Không tự đăng ký NesClock (H7): nghe StageWorld.TickBegan (màn đen — cả khi Paused) và TickEnded (dò điểm kích hoạt + chạy kịch bản
/// theo tick thế giới). Số liệu: Map1FlowTiming (đo video Map_1, MAP1_PLACEMENT §3).
///   • Cổng lâu đài (LevelSectionData.connections kind Gate): Simon trên đất tới x ∈ [GATE_TRIGGER_X, GATE_WINDOW_MAX_X) ⇒ TIME đứng,
///     tự đi 0.305 px/tick tới GATE_END_X (bị lớp FG che từ x 704) ⇒ đen GATE_BLACK_TICKS ⇒ section đích tại điểm vào đo video.
///   • Cửa cuối stage (kind Door, có target): Simon trên đất tới x ≥ triggerSimonX ⇒ TIME đứng, enemy/đạn bị gỡ, bộ sinh dừng; camera cuộn
///     128 px @ 1 px/tick (section đích hiện liền phải ở targetOrigin) ⇒ cánh cửa door_0 → door_1 (+SFX) ⇒ Simon tự đi 68 px xuyên cửa
///     ⇒ door_0 → cột cửa (+SFX) ⇒ cuộn 128 px ⇒ nạp section đích (cùng khung hình), STAGE +1 (GameRules.EnterStage), TIME chạy pha mới.
///   • Cầu thang cắt mép (kind Stair): đi lên quá STAIR_UP_EXIT_Y / xuống quá STAIR_DOWN_EXIT_Y ⇒ đen STAIR_BLACK_TICKS ⇒ section đích,
///     Simon trên thang đích ở Y đến đo video (x theo đường thang), TIME pha mới.
///   • Hết Block 1: StageRules.TallyDone ⇒ GameManager.GoTo(CastleMap, MAP_BLACK_TICKS).
///   • Nhạc (chưa import — §6 nợ 02 ⇒ clip null = im lặng + log): Vampire Killer từ đầu stage (video 2.205 s, chạy liền qua cổng/cửa/
///     Stage 02/03 — songpos), Stage Clear lúc nhặt quả cầu (12), Death khi mất mạng; nhạc boss do BossBase (14).
/// </summary>
[DisallowMultipleComponent]
public class StageFlow : MonoBehaviour
{
    public enum ePhase
    {
        None,
        Gate,
        Door,
        Black,
    }

    enum ePending
    {
        None,
        GateArrive,
        StairArrive,
    }

    [SerializeField] private StageWorld world;
    [SerializeField] private StageRules rules;
    [Tooltip("Cánh cửa cuối stage (SpriteRenderer con của gốc thực thể — px section, layer Foreground: vẽ ĐÈ Simon).")]
    [SerializeField] private SpriteRenderer doorView;
    [Tooltip("Object/door_0 — cánh hẹp 16×48 (đang mở / đang đóng).")]
    [SerializeField] private Sprite doorLeaf;
    [Tooltip("Object/door_1 — cửa mở 24×48.")]
    [SerializeField] private Sprite doorOpen;
    [Header("Nhạc — chưa import (Art_Source/music): để trống ⇒ PlayMusic(null) im lặng + log")]
    [Tooltip("03. Vampire Killer (Courtyard) — loop, Stage 01–03.")]
    [SerializeField] private AudioClip stageMusic;
    [Tooltip("12. Stage Clear — nhặt quả cầu.")]
    [SerializeField] private AudioClip stageClearMusic;
    [Tooltip("15. Death — mất mạng.")]
    [SerializeField] private AudioClip deathMusic;

    /// <summary>Mốc luồng (harness Map1RouteProbe ghi log): "gate", "eh", "door", "door_open", "stage2", "stair_down", …</summary>
    public static event Action<StageFlow, string> Milestone;

    private ePhase phase;
    private ePending pending;
    private int blackLeft;
    private int seqTick;                  // tick thế giới lúc bắt đầu bước hiện tại
    private bool gateSfxDone;
    private LevelSectionData.Connection conn;
    private LevelSection target;
    private int cx0, doorX, doorY, walkFromX;
    private int doorStep;                 // 0 cuộn 1, 1 dừng (cửa), 2 cuộn 2
    private int stairDir, stairArriveY;

    public ePhase Phase => phase;
    public StageWorld World => world;
    public bool Busy => phase != ePhase.None;

    private SimonController Simon => world != null ? world.Player as SimonController : null;
    private NesCameraController Cam => world != null ? world.CameraCtl : null;

    private void OnEnable()
    {
        if (world == null)
            world = GetComponent<StageWorld>();
        if (rules == null)
            rules = GetComponent<StageRules>();
        world.TickBegan += PreTick;
        world.TickEnded += PostTick;
        if (rules != null)
        {
            rules.TallyDone += OnTallyDone;
            rules.Respawned += OnRespawned;
            rules.LifeLost += OnLifeLost;
        }
        ItemEffects.CrystalPicked += OnCrystal;
        ResetFlow();
    }

    private void OnDisable()
    {
        if (world != null)
        {
            world.TickBegan -= PreTick;
            world.TickEnded -= PostTick;
        }
        if (rules != null)
        {
            rules.TallyDone -= OnTallyDone;
            rules.Respawned -= OnRespawned;
            rules.LifeLost -= OnLifeLost;
        }
        ItemEffects.CrystalPicked -= OnCrystal;
        ResetFlow();
    }

    /// <summary>Huỷ mọi kịch bản (hồi sinh / tắt màn): trả cột cửa, ẩn cánh cửa, camera bám Simon, mở khoá input.</summary>
    public void ResetFlow()
    {
        phase = ePhase.None;
        pending = ePending.None;
        blackLeft = 0;
        conn = null;
        target = null;
        if (doorView != null)
            doorView.enabled = false;
        if (world != null)
            world.SpawnersPaused = false;
    }

    /// <summary>Đầu stage (StageScreen.OnEnter, hồi sinh): nhạc stage — clip null ⇒ im lặng + log (nhạc chưa import).</summary>
    public void PlayStageMusic()
    {
        Log("nhạc Vampire Killer (loop" + (stageMusic == null ? " — chưa import, im lặng" : "") + ")");
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(stageMusic, true);
    }

    // ---------------------------------------------------------------- tick

    private void PreTick(StageWorld w)
    {
        if (blackLeft <= 0)
            return;
        w.Paused = true;
        if (--blackLeft > 0)
            return;
        var p = pending;
        pending = ePending.None;
        if (rules != null)
            rules.SetBlackScreen(false);
        w.Paused = false;
        phase = ePhase.None;
        if (p == ePending.GateArrive)
            ArriveSection(target, Map1FlowTiming.EH_ENTRY_X, Map1FlowTiming.EH_ENTRY_Y, "eh");
        else if (p == ePending.StairArrive)
            ArriveStair();
    }

    private void PostTick(StageWorld w)
    {
        var simon = Simon;
        if (simon == null || w.Section == null || w.Paused)
            return;
        switch (phase)
        {
            case ePhase.None:
                if (!simon.IsDying && (rules == null || !rules.Tally.Active))
                    Detect(simon);
                break;
            case ePhase.Gate:
                TickGate(simon);
                break;
            case ePhase.Door:
                TickDoor(simon);
                break;
        }
    }

    private void Detect(SimonController simon)
    {
        var data = world.Section.data;
        if (data.connections == null)
            return;
        bool onGround = simon.Fsm.Is(eSimonState.Idle) || simon.Fsm.Is(eSimonState.Walk);
        bool onStairMoving = simon.Fsm.Is(eSimonState.StairUp) || simon.Fsm.Is(eSimonState.StairDown);
        for (int i = 0; i < data.connections.Length; i++)
        {
            var c = data.connections[i];
            if (string.IsNullOrEmpty(c.target))
                continue;
            switch (c.kind)
            {
                case eLevelLink.Gate:
                    if (onGround && simon.X >= Map1FlowTiming.GATE_TRIGGER_X && simon.X < Map1FlowTiming.GATE_WINDOW_MAX_X)
                    {
                        StartGate(simon, c);
                        return;
                    }
                    break;
                case eLevelLink.Door:
                    if (onGround && c.triggerSimonX >= 0 && simon.X >= c.triggerSimonX && simon.Y == c.trigger.y + c.trigger.height)
                    {
                        StartDoor(simon, c);
                        return;
                    }
                    break;
                case eLevelLink.Stair:
                    if (onStairMoving && simon.stair.stair != null && simon.stair.stair.id == c.stair)
                    {
                        var st = simon.stair.stair;
                        if (st.cutTop && simon.stairMoveDir > 0 && simon.Y <= Map1FlowTiming.STAIR_UP_EXIT_Y)
                        {
                            StartStair(c, 1);
                            return;
                        }
                        if (st.cutBottom && simon.stairMoveDir < 0 && simon.Y >= Map1FlowTiming.STAIR_DOWN_EXIT_Y)
                        {
                            StartStair(c, -1);
                            return;
                        }
                    }
                    break;
            }
        }
    }

    // ---------------------------------------------------------------- cổng

    private void StartGate(SimonController simon, LevelSectionData.Connection c)
    {
        conn = c;
        target = FindSection(c.target);
        phase = ePhase.Gate;
        gateSfxDone = false;
        Hold(true);
        world.SpawnersPaused = true;
        simon.StartAutoWalk(Map1FlowTiming.GATE_END_X, Map1FlowTiming.GATE_WALK_SPEED_FIXED);
        Mark("gate");
    }

    private void TickGate(SimonController simon)
    {
        // SFX 25 "vào cổng" (CVGP 02 — khả năng, 1 mẫu 0:17.82 ⇐ Simon x 696)
        if (!gateSfxDone && simon.X >= 696)
        {
            gateSfxDone = true;
            if (AudioManager.I != null)
                AudioManager.I.PlaySfx(eSfx.EnterGate);
        }
        if (simon.Fsm.Is(eSimonState.AutoWalk))
            return;
        StartBlack(Map1FlowTiming.GATE_BLACK_TICKS, ePending.GateArrive);
        Mark("gate_black");
    }

    // ---------------------------------------------------------------- cửa cuối stage

    private void StartDoor(SimonController simon, LevelSectionData.Connection c)
    {
        conn = c;
        target = FindSection(c.target);
        phase = ePhase.Door;
        doorStep = 0;
        seqTick = world.TickCount;
        walkFromX = simon.X;
        doorX = c.trigger.x;
        doorY = Map1FlowTiming.DOOR_SPRITE_Y;
        Hold(true);
        world.SpawnersPaused = true;
        world.KillGroup(eEntityGroup.Enemy, eKillReason.Cleared);
        world.KillGroup(eEntityGroup.EnemyShot, eKillReason.Cleared);
        simon.InputLocked = true;
        simon.Fsm.SetState(eSimonState.Idle);
        // section đích hiện liền phải (gốc đích = targetOrigin trong toạ độ section nguồn — CVGP 06)
        if (target != null)
        {
            target.gameObject.SetActive(true);
            target.PlaceForCamera(-c.targetOrigin.x);
        }
        var cam = Cam;
        if (cam != null)
        {
            cx0 = cam.X;
            cam.Target = null;
            cam.ScrollTo(cx0 + Map1FlowTiming.DOOR_SCROLL_PX, Map1FlowTiming.DOOR_SCROLL_SPEED_FIXED);
        }
        Mark("door");
    }

    private void TickDoor(SimonController simon)
    {
        var cam = Cam;
        int k = world.TickCount - seqTick;
        if (doorStep == 0)
        {
            if (cam != null && cam.IsScrolling)
                return;
            doorStep = 1;
            seqTick = world.TickCount;   // cuộn lần 1 xong ⇒ mốc 0 của các bước cửa
            Mark("door_stop");
            return;
        }
        if (doorStep == 1)
        {
            if (k == Map1FlowTiming.DOOR_LEAF_TICK)
            {
                world.Section.ClearTileRect(conn.trigger.x, conn.trigger.y, conn.trigger.width, conn.trigger.height);
                ShowDoor(doorLeaf, doorX + Map1FlowTiming.DOOR_LEAF_DX);
            }
            else if (k == Map1FlowTiming.DOOR_OPEN_TICK)
            {
                ShowDoor(doorOpen, doorX);
                if (AudioManager.I != null)
                    AudioManager.I.PlaySfx(eSfx.Door);
                Mark("door_open");
            }
            else if (k == Map1FlowTiming.DOOR_WALK_TICK)
                simon.StartAutoWalk(walkFromX + Map1FlowTiming.DOOR_WALK_PX, 0, true);
            else if (k == Map1FlowTiming.DOOR_CLOSING_TICK)
                ShowDoor(doorLeaf, doorX + Map1FlowTiming.DOOR_LEAF_DX);
            else if (k == Map1FlowTiming.DOOR_CLOSED_TICK)
            {
                if (doorView != null)
                    doorView.enabled = false;
                world.Section.RestoreCells();
                if (AudioManager.I != null)
                    AudioManager.I.PlaySfx(eSfx.Door);
                Mark("door_closed");
            }
            else if (k == Map1FlowTiming.DOOR_SCROLL2_TICK)
            {
                doorStep = 2;
                if (cam != null)
                    cam.ScrollTo(cx0 + 2 * Map1FlowTiming.DOOR_SCROLL_PX, Map1FlowTiming.DOOR_SCROLL_SPEED_FIXED);
            }
            return;
        }
        if (cam != null && cam.IsScrolling)
            return;
        // cuộn lần 2 xong: nạp section đích cùng khung hình (camera đích = cx0 + 256 − gốc đích)
        phase = ePhase.None;
        var c = conn;
        simon.InputLocked = false;
        GameRules.EnterStage(c.targetStage, false);
        world.Stage = c.targetStage;
        ArriveSection(target, simon.X - c.targetOrigin.x, simon.Y - c.targetOrigin.y, "stage" + c.targetStage);
    }

    private void ShowDoor(Sprite s, int x)
    {
        if (doorView == null)
            return;
        doorView.sprite = s;
        doorView.enabled = s != null;
        doorView.transform.localPosition = NesCoord.ToWorld(x, doorY);
    }

    // ---------------------------------------------------------------- cầu thang đổi tầng

    private void StartStair(LevelSectionData.Connection c, int dir)
    {
        conn = c;
        target = FindSection(c.target);
        stairDir = dir;
        stairArriveY = dir > 0 ? Map1FlowTiming.STAIR_UP_ARRIVE_Y : Map1FlowTiming.STAIR_DOWN_ARRIVE_Y;
        Hold(true);
        StartBlack(Map1FlowTiming.STAIR_BLACK_TICKS, ePending.StairArrive);
        Mark(dir > 0 ? "stair_up" : "stair_down");
    }

    private void ArriveStair()
    {
        var simon = Simon;
        var c = conn;
        if (target == null || simon == null)
            return;
        // đích(x, y) = nguồn(x, y) − targetOrigin; vị trí cuối do thang đích quyết định (Y đến đo video)
        world.Warp(target, simon.X - c.targetOrigin.x, simon.Y - c.targetOrigin.y);
        var tiles = world.Tiles;
        bool placed = false;
        for (int i = 0; i < tiles.StairCount; i++)
        {
            var p = tiles.GetStair(i);
            if (p.stair == null || p.stair.id != c.targetStair)
                continue;
            int t = Mathf.Clamp(p.bottom.y - stairArriveY, 0, p.Length);
            simon.PlaceOnStair(p, t, stairDir);
            placed = true;
            break;
        }
        if (!placed)
            Debug.LogError("[Flow] không thấy thang đích " + c.targetStair + " trong " + target.name);
        AfterArrive(stairDir > 0 ? "ca_from_cb" : "cb_from_ca");
    }

    // ---------------------------------------------------------------- chung

    private void ArriveSection(LevelSection s, int x, int y, string mark)
    {
        var simon = Simon;
        if (s == null || simon == null)
            return;
        world.Warp(s, x, y);
        simon.Teleport(x, y, 1);
        AfterArrive(mark);
    }

    private void AfterArrive(string mark)
    {
        var cam = Cam;
        if (cam != null)
        {
            cam.Target = world.Player;
            cam.SnapToTarget();
        }
        if (doorView != null)
            doorView.enabled = false;
        world.SpawnersPaused = false;
        Hold(false);
        if (rules != null)
            rules.ResetClockPhase();
        world.RefreshSpawnPoints();   // nến/chậu của section mới thấy ngay khung hình đầu (video f600, f2756, f4441, f5126)
        conn = null;
        target = null;
        Mark(mark);
    }

    private void StartBlack(int ticks, ePending what)
    {
        phase = ePhase.Black;
        pending = what;
        blackLeft = Mathf.Max(1, ticks);
        if (rules != null)
            rules.SetBlackScreen(true);
    }

    private void Hold(bool on)
    {
        if (rules != null)
            rules.SetClockHold(eClockHold.Script, on);
    }

    private LevelSection FindSection(string id)
    {
        var list = world.Sections;
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null && list[i].data != null && list[i].data.id == id)
                return list[i];
        Debug.LogError("[Flow] không có section " + id + " dưới StageWorld");
        return null;
    }

    // ---------------------------------------------------------------- sự kiện luật chơi

    private void OnTallyDone(StageRules r)
    {
        Mark("tally_done");
        if (GameManager.I != null)
            GameManager.I.GoTo(eGameScreen.CastleMap, Map1FlowTiming.MAP_BLACK_TICKS);
    }

    private void OnRespawned(StageRules r)
    {
        ResetFlow();
        if (Cam != null)
        {
            Cam.Target = world.Player;
            Cam.SnapToTarget();
        }
        PlayStageMusic();
        Mark("respawn");
    }

    private void OnLifeLost(eDeathCause cause, bool alive)
    {
        Log("nhạc Death (" + (deathMusic == null ? "chưa import — im lặng" : deathMusic.name) + ")");
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(deathMusic, false);
    }

    private void OnCrystal(StageWorld w)
    {
        if (w != world)
            return;
        Log("nhạc Stage Clear (" + (stageClearMusic == null ? "chưa import — im lặng" : stageClearMusic.name) + ")");
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(stageClearMusic, false);
        Mark("crystal");
    }

    private void Mark(string m)
    {
        Log(m + " (section " + (world.Section != null ? world.Section.name : "?") + ", cx " + (Cam != null ? Cam.X : 0)
            + ", Simon " + (Simon != null ? Simon.X + "," + Simon.Y : "?") + ", tick " + world.TickCount + ")");
        if (Milestone != null)
            Milestone(this, m);
    }

    private static void Log(string msg)
    {
        if (CvgpDebug.LogRules)
            CvgpDebug.Log("[Flow] " + msg);
    }
}
