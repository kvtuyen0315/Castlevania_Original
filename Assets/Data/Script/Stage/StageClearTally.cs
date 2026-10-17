using UnityEngine;

public enum eTallyPhase
{
    Idle,
    WaitStart,
    CountTime,
    Gap,
    CountHearts,
    EndWait,
    Done,
}

/// <summary>
/// Bảng cộng điểm sau boss (CVGP 12; wiki Gameplay#Points: giờ còn lại × 10 + tim × 100). Nhịp đo video Map_1 3:33–3:52
/// (Tools/cvgp/hud_rules.py, CVGP_MEMORY §2j): nhặt quả cầu (tick P) → chờ StageClearToTallyTicks (260) → TIME về 0, 1 đơn vị /
/// TallyTicksPerTimeUnit tick (+TallyPointsPerTimeUnit) → nghỉ TallyGapTicks (54) → tim về 0, 1 tim / TallyTicksPerHeart tick
/// (+TallyPointsPerHeart) → chờ TallyEndWaitTicks (120) ⇒ Done (luồng stage — routine 15: đen 28 tick → bản đồ lâu đài).
/// SFX kênh đơn (routine 02): TallyTime ở đơn vị đầu, mỗi TallySfxEveryTicks tick và đơn vị cuối; TallyHeart mỗi tim.
/// StageRules gọi Tick() 1 lần/tick (cuối tick, bỏ qua khi pause). Điểm cộng qua GameRules.AddScore (1UP vẫn tính).
/// </summary>
public class StageClearTally
{
    public eTallyPhase Phase { get; private set; } = eTallyPhase.Idle;
    public bool Active => Phase != eTallyPhase.Idle && Phase != eTallyPhase.Done;
    public bool IsDone => Phase == eTallyPhase.Done;
    /// <summary>Số tick đã chạy từ lúc Start (tick nhặt cầu = 1).</summary>
    public int Ticks { get; private set; }
    public int TimeCounted { get; private set; }
    public int HeartsCounted { get; private set; }
    public int PointsAdded { get; private set; }
    /// <summary>Tick (Ticks) bắt đầu từng pha — harness/log.</summary>
    public int TimeStartTick { get; private set; }
    public int TimeEndTick { get; private set; }
    public int HeartStartTick { get; private set; }
    public int HeartEndTick { get; private set; }
    public int DoneTick { get; private set; }

    int t, waitStart, perUnit, perHeart, gap, endWait, sfxEvery, ptsUnit, ptsHeart;

    public void Reset()
    {
        Phase = eTallyPhase.Idle;
        Ticks = t = 0;
        TimeCounted = HeartsCounted = PointsAdded = 0;
        TimeStartTick = TimeEndTick = HeartStartTick = HeartEndTick = DoneTick = 0;
    }

    /// <summary>waitOverride / gapOverride ≥ 0 ⇒ thay StageClearToTallyTicks / TallyGapTicks (CVM2 11: Block 2 đo video Map_2 —
    /// Map2FlowTiming.TALLY_*; Block 1 giữ Config = video Map_1).</summary>
    public void Start(GameConfig cfg, int waitOverride = -1, int gapOverride = -1)
    {
        Reset();
        waitStart = waitOverride >= 0 ? waitOverride : cfg.rules.GetInt(eRuleParam.StageClearToTallyTicks, 260);
        perUnit = Mathf.Max(1, cfg.score.GetInt(eScoreParam.TallyTicksPerTimeUnit, 1));
        perHeart = Mathf.Max(1, cfg.score.GetInt(eScoreParam.TallyTicksPerHeart, 8));
        gap = gapOverride >= 0 ? gapOverride : cfg.score.GetInt(eScoreParam.TallyGapTicks, 54);
        endWait = cfg.score.GetInt(eScoreParam.TallyEndWaitTicks, 120);
        sfxEvery = Mathf.Max(1, cfg.score.GetInt(eScoreParam.TallySfxEveryTicks, 8));
        ptsUnit = cfg.score.GetInt(eScoreParam.TallyPointsPerTimeUnit, 10);
        ptsHeart = cfg.score.GetInt(eScoreParam.TallyPointsPerHeart, 100);
        Phase = eTallyPhase.WaitStart;
    }

    /// <summary>1 tick. Trả true ở đúng tick tally xong (Done).</summary>
    public bool Tick()
    {
        if (!Active)
            return false;
        Ticks++;
        var s = GameSession.Current;
        switch (Phase)
        {
            case eTallyPhase.WaitStart:
                if (++t <= waitStart)
                    return false;
                Enter(eTallyPhase.CountTime);
                TimeStartTick = Ticks;
                goto case eTallyPhase.CountTime;
            case eTallyPhase.CountTime:
                if (s.time > 0 && t % perUnit == 0)
                {
                    s.time--;
                    int k = TimeCounted++;
                    PointsAdded += ptsUnit;
                    GameRules.AddScore(ptsUnit);
                    s.NotifyChanged();
                    if (k % sfxEvery == 0 || s.time == 0)
                        Sfx(eSfx.TallyTime);
                }
                t++;
                if (s.time <= 0)
                {
                    TimeEndTick = Ticks;
                    Enter(eTallyPhase.Gap);
                }
                return false;
            case eTallyPhase.Gap:
                if (++t < gap)
                    return false;
                Enter(eTallyPhase.CountHearts);
                HeartStartTick = Ticks;
                goto case eTallyPhase.CountHearts;
            case eTallyPhase.CountHearts:
                if (s.hearts > 0 && t % perHeart == 0)
                {
                    s.hearts--;
                    HeartsCounted++;
                    PointsAdded += ptsHeart;
                    GameRules.AddScore(ptsHeart);
                    s.NotifyChanged();
                    Sfx(eSfx.TallyHeart);
                }
                t++;
                if (s.hearts <= 0)
                {
                    HeartEndTick = Ticks;
                    Enter(eTallyPhase.EndWait);
                }
                return false;
            case eTallyPhase.EndWait:
                if (++t < endWait)
                    return false;
                DoneTick = Ticks;
                Enter(eTallyPhase.Done);
                return true;
        }
        return false;
    }

    void Enter(eTallyPhase p)
    {
        Phase = p;
        t = 0;
    }

    static void Sfx(eSfx id)
    {
        if (AudioManager.I != null)
            AudioManager.I.PlaySfxRestart(id);
    }
}
