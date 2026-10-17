using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Lý do TIME đứng (cờ bit, CVGP 12). Đứng khi có ít nhất 1 cờ.</summary>
[Flags]
public enum eClockHold
{
    None = 0,
    /// <summary>Kịch bản: Simon tự đi vào cổng / qua cửa cuối stage (video 0:16.2–0:20.0, 1:24.8–1:31.9) — routine 15 bật/tắt.</summary>
    Script = 1,
    /// <summary>Quả cầu (Magic Crystal): từ lúc nhặt (hoặc lúc cầu hiện — routine 14 bật) tới hết tally.</summary>
    StageClear = 2,
}

/// <summary>
/// Luật chơi trong màn chơi (CVGP 12) — component cạnh StageWorld (màn gameplay routine 15; harness thêm vào scene Arena khi kiểm).
/// Không tự đăng ký NesClock (H7): nghe StageWorld.TickBegan (đầu tick, cả khi pause) và TickEnded (cuối tick).
///   • Đồng hồ: TIME −1 mỗi TimeUnitTicks (64 — đo 165 đv) tick thế giới; pha đặt lại khi stage/section bắt đầu (ResetClockPhase —
///     video: đơn vị đầu giảm 60–64 tick sau mỗi lần vào section); đứng khi pause, có eClockHold, Simon đang chết, tally. KHÔNG đứng khi
///     nhặt roi (đóng băng thế giới — video 0:06.5). TIME 0 ⇒ Simon.KillByTimeout.
///   • Pause: Start (NesPad) ⇒ StageWorld.Paused + tạm dừng nhạc; không pause khi đang đen / tally / chết / kịch bản / GameManager chuyển cảnh.
///   • Chết (Simon.Died): GameRules.LoseLife ⇒ còn mạng: đen RespawnBlackTicks rồi RestartStage (đầu stage — wiki: không checkpoint);
///     hết mạng: GameRules.EnterGameOver (GameManager ⇒ màn GAME OVER); scene thử không có GameManager ⇒ đen rồi tự Continue.
///   • HUD: thanh máu HIỆN chạy dần (DamageDisplayTicksPerBar / HealDisplayTicksPerBar = 8 — đo), icon II/III nháy ShotIconBlink*.
///   • Tally: ItemEffects.CrystalPicked ⇒ khoá input Simon + StageClearTally; xong ⇒ sự kiện TallyDone (routine 15 nối bản đồ lâu đài).
/// Màn đen nội bộ = camera gameplay cullingMask 0 (camera xoá nền đen như GameManager.GoTo — không cần art).
/// </summary>
[DisallowMultipleComponent]
public class StageRules : MonoBehaviour
{
    [Serializable]
    public class StageStart
    {
        public int stage = 1;
        public LevelSection section;
        [Tooltip("true ⇒ dùng LevelSectionData.simonStart của section")]
        public bool useSectionStart = true;
        public Vector2Int position;
    }

    enum ePending { None, Respawn, LocalContinue }

    public static StageRules Current { get; private set; }

    [SerializeField] private StageWorld world;
    [Tooltip("Điểm hồi sinh = đầu mỗi stage (routine 15 điền theo section đầu stage). Trống ⇒ section đang chơi + simonStart.")]
    [SerializeField] private List<StageStart> stageStarts = new List<StageStart>();

    /// <summary>Tally xong (đúng tick) — luồng stage (routine 15) nối đen → bản đồ lâu đài.</summary>
    public event Action<StageRules> TallyDone;
    /// <summary>Mất mạng: nguyên nhân, còn mạng không.</summary>
    public event Action<eDeathCause, bool> LifeLost;
    /// <summary>Đã hồi sinh ở đầu stage.</summary>
    public event Action<StageRules> Respawned;
    public event Action<bool> PauseChanged;

    private readonly StageClearTally tally = new StageClearTally();
    private SimonController simon;
    private eClockHold holds;
    private int clockPhase;
    private bool paused;
    private int blackLeft;
    private ePending pending;
    private bool unpauseNext;
    private bool deathPending, timeUpPending;
    private eDeathCause deathCause;
    private int cullingSaved;
    private bool blackOn;
    private Camera blackCam;
    private int playerWait, enemyWait;
    private int shotBlinkLeft, prevShot;
    // số liệu (đọc lúc bật)
    private int timeUnit = 64, dmgTicks = 8, healTicks = 8, respawnBlack = 32, gameOverBlack = 32, blinkTicks = 120, blinkHalf = 4;

    public StageWorld World => world;
    public StageClearTally Tally => tally;
    public SimonController Simon => simon;
    public eClockHold Holds => holds;
    public int ClockPhase => clockPhase;
    public bool IsPaused => paused;
    public bool IsBlack => blackLeft > 0 || unpauseNext;
    public int DeathCount { get; private set; }
    public int RespawnCount { get; private set; }
    public eDeathCause LastDeathCause { get; private set; }
    public List<StageStart> StageStarts => stageStarts;

    public bool CanPause => blackLeft == 0 && !unpauseNext && !tally.Active && !tally.IsDone && simon != null && !simon.IsDying
                            && (holds & eClockHold.Script) == 0 && !deathPending
                            && (GameManager.I == null || !GameManager.I.IsTransitioning);

    private void OnEnable()
    {
        if (world == null)
            world = GetComponent<StageWorld>();
        Current = this;
        LoadTuning();
        world.TickBegan += PreTick;
        world.TickEnded += PostTick;
        ItemEffects.CrystalPicked += OnCrystal;
        var s = GameSession.Current;
        s.animateBars = true;
        s.shownPlayerHp = s.playerHp;
        s.shownEnemyHp = s.enemyHp;
        s.shotIconVisible = true;
        prevShot = s.shotMultiplier;
        clockPhase = 0;
        holds = eClockHold.None;
        paused = false;
        blackLeft = 0;
        pending = ePending.None;
        unpauseNext = deathPending = timeUpPending = false;
        tally.Reset();
        s.NotifyChanged();
    }

    private void OnDisable()
    {
        if (world != null)
        {
            world.TickBegan -= PreTick;
            world.TickEnded -= PostTick;
            if (paused || IsBlack)
                world.Paused = false;
        }
        ItemEffects.CrystalPicked -= OnCrystal;
        BindSimon(null);
        if (paused && AudioManager.I != null)
            AudioManager.I.PauseMusic(false);
        paused = false;
        SetBlack(false);
        GameSession.Current.animateBars = false;
        GameSession.Current.shotIconVisible = true;
        GameSession.Current.NotifyChanged();
        if (Current == this)
            Current = null;
    }

    private void LoadTuning()
    {
        var cfg = GameConfig.Current;
        if (cfg == null)
            return;
        timeUnit = Mathf.Max(1, cfg.rules.GetInt(eRuleParam.TimeUnitTicks, 64));
        dmgTicks = Mathf.Max(1, cfg.rules.GetInt(eRuleParam.DamageDisplayTicksPerBar, 8));
        healTicks = Mathf.Max(1, cfg.rules.GetInt(eRuleParam.HealDisplayTicksPerBar, 8));
        respawnBlack = cfg.rules.GetInt(eRuleParam.RespawnBlackTicks, 32);
        gameOverBlack = cfg.rules.GetInt(eRuleParam.GameOverBlackTicks, 32);
        blinkTicks = cfg.rules.GetInt(eRuleParam.ShotIconBlinkTicks, 120);
        blinkHalf = Mathf.Max(1, cfg.rules.GetInt(eRuleParam.ShotIconBlinkHalfTicks, 4));
    }

    // ---------------------------------------------------------------- API (routine 14/15 + harness)

    public void SetClockHold(eClockHold h, bool on)
    {
        var before = holds;
        holds = on ? holds | h : holds & ~h;
        if (holds != before)
            Log("TIME " + (holds == eClockHold.None ? "chạy" : "đứng (" + holds + ")"));
    }

    /// <summary>Đơn vị TIME kế tiếp giảm sau đủ TimeUnitTicks tick (gọi khi stage/section mới hiện — routine 15 sau mỗi màn đen).</summary>
    public void ResetClockPhase()
    {
        clockPhase = 0;
    }

    /// <summary>Bật/tắt pause (Start — cũng gọi được từ harness). Trả false nếu đang không được pause.</summary>
    public bool TogglePause()
    {
        if (!paused && !CanPause)
            return false;
        paused = !paused;
        world.Paused = paused;
        if (AudioManager.I != null)
            AudioManager.I.PauseMusic(paused);
        Log(paused ? "PAUSE" : "hết PAUSE");
        if (PauseChanged != null)
            PauseChanged(paused);
        return true;
    }

    /// <summary>Đầu stage đang chơi: section + vị trí (StageStart của GameSession.stage, không có ⇒ section hiện tại + simonStart).</summary>
    public void GetStageStart(int stage, out LevelSection section, out Vector2Int pos)
    {
        int facing;
        GetStageStart(stage, out section, out pos, out facing);
    }

    /// <summary>CVM2 11: + hướng Simon — dùng simonStart của section ⇒ LevelSectionData.SimonStartFacing (Map 2 Hall A/C quay TRÁI sau cửa
    /// trái — video 0:58.70 / 2:29.30); vị trí đặt tay (Block 1 Stage 02/03) ⇒ phải như cũ.</summary>
    public void GetStageStart(int stage, out LevelSection section, out Vector2Int pos, out int facing)
    {
        section = null;
        pos = Vector2Int.zero;
        facing = 1;
        for (int i = 0; i < stageStarts.Count; i++)
        {
            var st = stageStarts[i];
            if (st.stage == stage && st.section != null)
            {
                section = st.section;
                bool fromSection = st.useSectionStart && st.section.data.hasSimonStart;
                pos = fromSection ? st.section.data.simonStart : st.position;
                facing = fromSection ? st.section.data.SimonStartFacing : 1;
                return;
            }
        }
        section = world.Section;
        if (section != null && section.data.hasSimonStart)
        {
            pos = section.data.simonStart;
            facing = section.data.SimonStartFacing;
        }
        else if (simon != null)
            pos = simon.home;
    }

    /// <summary>Hồi sinh ở đầu stage: đặt lại trạng thái (GameRules.ApplyDeathReset), dọn thế giới, Simon về điểm đầu, RNG đầu stage.</summary>
    public void RestartStage()
    {
        var s = GameSession.Current;
        LevelSection sec;
        Vector2Int pos;
        int facing;
        GetStageStart(s.stage, out sec, out pos, out facing);
        GameRules.ApplyDeathReset();
        world.Stage = s.stage;
        if (sec != null)
            world.Warp(sec, pos.x, pos.y);
        if (simon != null)
        {
            simon.Teleport(pos.x, pos.y, facing);   // CVM2 11: Block 2 Hall A/C quay trái (Block 1 luôn phải)
            if (world.CameraCtl != null)
                world.CameraCtl.SnapToTarget();
        }
        world.ResetRng();
        clockPhase = 0;
        holds = eClockHold.None;
        tally.Reset();
        playerWait = enemyWait = 0;
        shotBlinkLeft = 0;
        prevShot = s.shotMultiplier;
        s.shownPlayerHp = s.playerHp;
        s.shownEnemyHp = s.enemyHp;
        s.shotIconVisible = true;
        s.NotifyChanged();
        RespawnCount++;
        Log("hồi sinh đầu STAGE " + s.stage.ToString("00") + " tại (" + pos.x + "," + pos.y + ") — TIME " + s.time.ToString("0000") + ", tim " + s.hearts
            + ", roi " + s.whipLevel + ", vũ khí " + s.subWeapon + ", P-" + s.lives.ToString("00"));
        if (Respawned != null)
            Respawned(this);
    }

    // ---------------------------------------------------------------- tick

    private void BindSimon(SimonController next)
    {
        if (next == simon)
            return;
        if (simon != null)
            simon.Died -= OnSimonDied;
        simon = next;
        if (simon != null)
        {
            simon.Died += OnSimonDied;
            simon.AutoRespawn = false;   // luật hồi sinh ở đây (autoRespawn chỉ cho scene thử không có StageRules)
        }
    }

    private void PreTick(StageWorld w)
    {
        BindSimon(w.Player as SimonController);
        if (unpauseNext)
        {
            unpauseNext = false;
            w.Paused = paused;
        }
        if (blackLeft > 0)
        {
            w.Paused = true;
            if (--blackLeft == 0)
                FinishBlack();
            return;
        }
        if (NesPad.I != null && NesPad.I.Pressed(eNesButton.Start))
            TogglePause();
    }

    private void PostTick(StageWorld w)
    {
        if (w.Paused)
            return;
        var s = GameSession.Current;
        bool changed = false;

        if (deathPending)
        {
            deathPending = false;
            HandleDeath();
            return;
        }

        // đồng hồ
        if (s.time > 0 && holds == eClockHold.None && !tally.Active && !tally.IsDone && simon != null && !simon.IsDying)
        {
            if (++clockPhase >= timeUnit)
            {
                clockPhase = 0;
                s.time--;
                changed = true;
                if (s.time == 0)
                {
                    Log("TIME 0000 ⇒ Simon chết (hết giờ)");
                    timeUpPending = true;
                    simon.KillByTimeout();
                }
            }
        }

        // tally
        if (tally.Active && tally.Tick())
        {
            Log("tally xong: TIME " + tally.TimeCounted + " đv + tim " + tally.HeartsCounted + " = +" + tally.PointsAdded + " điểm (SCORE " + s.score + ")");
            if (TallyDone != null)
                TallyDone(this);
        }

        // thanh máu hiển thị chạy dần
        changed |= StepBar(ref s.shownPlayerHp, s.playerHp, ref playerWait);
        changed |= StepBar(ref s.shownEnemyHp, s.enemyHp, ref enemyWait);

        // icon II/III nháy khi vừa có multiplier
        if (s.shotMultiplier > prevShot && s.shotMultiplier > 1)
            shotBlinkLeft = blinkTicks;
        prevShot = s.shotMultiplier;
        bool vis = true;
        if (shotBlinkLeft > 0)
        {
            vis = ((blinkTicks - shotBlinkLeft) / blinkHalf) % 2 == 0;
            shotBlinkLeft--;
        }
        if (vis != s.shotIconVisible)
        {
            s.shotIconVisible = vis;
            changed = true;
        }

        if (changed)
            s.NotifyChanged();
    }

    private bool StepBar(ref int shown, int actual, ref int wait)
    {
        if (shown == actual)
        {
            wait = 0;
            return false;
        }
        if (wait > 0)
        {
            wait--;
            return false;
        }
        bool down = shown > actual;
        shown += down ? -1 : 1;
        wait = (down ? dmgTicks : healTicks) - 1;
        return true;
    }

    private void OnSimonDied(SimonController who, eTileCollision kind)
    {
        deathCause = kind == eTileCollision.Hazard ? eDeathCause.Hazard
                   : kind != eTileCollision.Empty ? eDeathCause.Fall : timeUpPending ? eDeathCause.TimeUp : eDeathCause.Damage;
        timeUpPending = false;
        deathPending = true;
    }

    private void HandleDeath()
    {
        DeathCount++;
        LastDeathCause = deathCause;
        bool alive = GameRules.LoseLife(deathCause);
        if (LifeLost != null)
            LifeLost(deathCause, alive);
        if (alive)
        {
            StartBlack(respawnBlack, ePending.Respawn);
            return;
        }
        if (GameRules.EnterGameOver(gameOverBlack))
            return;   // GameManager tắt màn chơi (StageWorld OnDisable) ⇒ màn GAME OVER
        Log("scene thử không có GameManager ⇒ đen " + gameOverBlack + " tick rồi tự CONTINUE");
        StartBlack(gameOverBlack, ePending.LocalContinue);
    }

    private void StartBlack(int ticks, ePending what)
    {
        pending = what;
        blackLeft = Mathf.Max(1, ticks);
        world.Paused = true;
        SetBlack(true);
        Log("màn đen " + blackLeft + " tick ⇒ " + what);
    }

    private void FinishBlack()
    {
        SetBlack(false);
        var p = pending;
        pending = ePending.None;
        if (p == ePending.LocalContinue)
            GameRules.Continue();
        RestartStage();
        unpauseNext = true;   // stage mới hiện ở tick này, thực thể chạy từ tick sau (như GameManager: OnEnter rồi OnTick)
    }

    /// <summary>CVGP 15: màn đen của luồng stage (cổng, chuyển tầng) — cùng cơ chế camera cullingMask 0 với màn đen hồi sinh.</summary>
    public void SetBlackScreen(bool on)
    {
        SetBlack(on);
    }

    private void SetBlack(bool on)
    {
        if (on)
        {
            blackCam = world != null && world.CameraCtl != null ? world.CameraCtl.Cam : null;
            if (blackCam != null && !blackOn)
            {
                cullingSaved = blackCam.cullingMask;
                blackCam.cullingMask = 0;
                blackOn = true;
            }
        }
        else if (blackCam != null && blackOn)
        {
            blackCam.cullingMask = cullingSaved;
            blackOn = false;
        }
    }

    private void OnCrystal(StageWorld w)
    {
        if (w != world || tally.Active)
            return;
        SetClockHold(eClockHold.StageClear, true);
        if (simon != null)
            simon.InputLocked = true;
        // CVM2 11: Block 2 chờ/nghỉ đo video Map_2 (Map2FlowTiming.TALLY_*); block khác giữ Config (video Map_1)
        bool b2 = Map2FlowTiming.AppliesTo(world.Stage);
        tally.Start(GameConfig.Current, b2 ? Map2FlowTiming.TALLY_WAIT_TICKS : -1, b2 ? Map2FlowTiming.TALLY_GAP_TICKS : -1);
        Log("nhặt quả cầu ⇒ TIME đứng " + GameSession.Current.time.ToString("0000") + ", tally sau "
            + (b2 ? Map2FlowTiming.TALLY_WAIT_TICKS : GameConfig.Current.rules.GetInt(eRuleParam.StageClearToTallyTicks, 260)) + " tick");
    }

    private static void Log(string msg)
    {
        if (CvgpDebug.LogRules)
            CvgpDebug.Log("[Rules] " + msg);
    }
}
