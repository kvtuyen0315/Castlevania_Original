using UnityEngine;

/// <summary>State Frankenstein's Monster (boss Block 4).</summary>
public enum eFrankensteinState
{
    Dormant, // đứng chờ (không va chạm) — Igor ngồi trên vai
    Walk,    // lang thang nửa phải màn (gồm đứng giữa 2 lần quyết định)
}

/// <summary>
/// Frankenstein's Monster (CVGP 19, theo wiki — GẦN ĐÚNG, không có video Block 4). Wiki Bosses#Frankenstein & Igor + Stages 10-12#Stage 12:
/// Monster "chỉ đi lang thang trong phòng", "thường ở nửa phải màn hình", có thể chắn đường; 24/16 đòn (chỉ Monster — Igor bất tử), 5000 đ;
/// Stopwatch vô dụng; "lúc mới vào phòng chạy nhảy lên mép bục dưới bên phải đánh vài đòn trước khi Igor bắt đầu di chuyển" ⇒ Monster nhận
/// đòn trước Igor; Igor "nhảy khỏi vai Monster". Ước lượng (ConfigActorParam Boss.Frankenstein — ghi §2p):
///   Dormant  sinh ⇒ Igor (igorPrefab) ngồi trên vai; hết wakeDelayTicks ⇒ Activate (Igor tự bắt đầu sau Enemy.Igor.startDelayTicks).
///   Walk     mỗi decideMin..+span tick: chasePct % nhắm x Simon (kẹp trong vùng [cx + zoneMinScreenX, cx + zoneMaxScreenX] = nửa phải),
///            còn lại 1 điểm ngẫu nhiên trong vùng; đi moveSpeed (ConfigBoss — chậm hơn Mummy) tới nơi ⇒ đứng quay về Simon. Tường/khối chặn
///            ⇒ đứng tới lần quyết định sau.
/// Chết: chuỗi chết boss (5000 đ, quả cầu) + Igor biến mất trong lửa (không điểm).
/// </summary>
public class Frankenstein : BossBase
{
    [SerializeField] private Igor igorPrefab;

    public StateMachine<Frankenstein, eFrankensteinState> Fsm { get; private set; }

    private AnimSet.Anim aWalk;
    [System.NonSerialized] public Igor igor;
    private int igorSpawnTick;
    [System.NonSerialized] public int wakeDelay, speed, zoneMin, zoneMax, decideMin, decideSpan, chasePct, standDx;
    [System.NonSerialized] public int decideTimer, walkDir, targetX;
    /// <summary>Số lần quyết định (harness).</summary>
    [System.NonSerialized] public int decisions;

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) : "-";

    abstract class FrankState : BaseState<Frankenstein>
    {
        protected void SetState(eFrankensteinState s) { owner.Fsm.SetState(s); }
    }

    class DormantState : FrankState
    {
        public override void OnEnterState() { owner.PlayWalk(false); }

        public override void CheckNextState()
        {
            if (owner.ageTicks < owner.wakeDelay)
                return;
            owner.Activate();
            owner.StartBossMusic();
            if (owner.IgorAlive())
                owner.igor.MonsterAwake();
            SetState(eFrankensteinState.Walk);
        }
    }

    class WalkState : FrankState
    {
        public override void OnEnterState() { owner.walkDir = 0; owner.decideTimer = 0; }

        public override void OnExecuteState()
        {
            if (--owner.decideTimer <= 0)
                owner.Decide();
            if (owner.walkDir != 0)
            {
                owner.Facing = owner.walkDir;
                bool blocked = owner.StepX(owner.walkDir * owner.speed);
                if (blocked || Mathf.Abs(owner.X - owner.targetX) <= owner.standDx)
                    owner.walkDir = 0;
            }
            if (owner.walkDir == 0)
                owner.Facing = owner.FacingToPlayer();
            owner.PlayWalk(owner.walkDir != 0);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Frankenstein, eFrankensteinState>(this, "Frankenstein");
        Fsm.Add(eFrankensteinState.Dormant, new DormantState());
        Fsm.Add(eFrankensteinState.Walk, new WalkState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        wakeDelay = PInt(cfg, "wakeDelayTicks", 16);
        speed = BossMoveSpeed(cfg, 0.25f);
        zoneMin = PInt(cfg, "zoneMinScreenX", 128);
        zoneMax = PInt(cfg, "zoneMaxScreenX", 216);
        decideMin = PInt(cfg, "decideMinTicks", 64);
        decideSpan = PInt(cfg, "decideSpanTicks", 64);
        chasePct = PInt(cfg, "chasePct", 75);
        standDx = PInt(cfg, "standDxPx", 4);
        decideTimer = 0;
        walkDir = 0;
        targetX = X;
        decisions = 0;
        Facing = -1;
        igor = null;
        Fsm.SetState(eFrankensteinState.Dormant);
        if (igorPrefab != null)
        {
            igor = World.Spawn(igorPrefab, X, Y, -1) as Igor;
            if (igor != null)
            {
                igorSpawnTick = igor.SpawnTick;
                igor.RideOn(this);
            }
        }
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    protected override void OnStunEnd()
    {
        PlayWalk(Fsm.Is(eFrankensteinState.Walk) && walkDir != 0);
    }

    public bool IgorAlive()
    {
        return igor != null && igor.Alive && igor.SpawnTick == igorSpawnTick;
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    private void Decide()
    {
        decisions++;
        decideTimer = Roll(decideMin, decideSpan);
        int lo = CamX + zoneMin, hi = CamX + zoneMax;
        var p = World.Player;
        if (p != null && World.Rng.Next(100) < chasePct)
            targetX = Mathf.Clamp(p.X, lo, hi);
        else
            targetX = Roll(lo, hi - lo);
        int d = targetX - X;
        walkDir = Mathf.Abs(d) <= standDx ? 0 : (d > 0 ? 1 : -1);
    }

    private void PlayWalk(bool moving)
    {
        if (Anim == null || aWalk == null)
            return;
        if (moving)
        {
            if (!Anim.IsPlaying(aWalk) || Anim.Done)
                Anim.Play(aWalk, true);
        }
        else if (!Anim.IsPlaying(aWalk) || !Anim.Done || Anim.FrameIndex != 0)
            Anim.SetFrame(aWalk, 0);
    }

    protected override void Die()
    {
        base.Die();
        if (IgorAlive())
            igor.Vanish();
    }

    protected override void OnRemoved()
    {
        // gỡ phòng (Warp/ClearNonPlayer) ⇒ Igor đã bị gỡ cùng; chết thật ⇒ Igor đã Vanish
        igor = null;
        base.OnRemoved();
    }
}
