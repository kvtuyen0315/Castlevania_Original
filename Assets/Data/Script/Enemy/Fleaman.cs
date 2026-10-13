using UnityEngine;

/// <summary>State Fleaman (Hunchback).</summary>
public enum eFleamanState
{
    Wait,    // đứng chờ sẵn (wiki Stage 13)
    Ground,  // nghỉ giữa 2 cú nhảy
    Hop,     // trên không (cả rơi khỏi mép)
    Carried, // Eagle đang mang (CVGP 18 — wiki Stage 11/17)
}

/// <summary>
/// Fleaman — "Hunchback/Fleaman" trên sheet (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 3). Wiki Enemies#Fleamen: nhảy như
/// ngẫu nhiên, kiểu thường gặp 2 bước nhảy ngắn rồi 1 cú nhảy vọt lớn; va chạm khi nhảy tới; "chạy chui dưới cú nhảy cao"; 1/1 đòn,
/// 500 đ; Stage 13 đứng chờ sẵn, Stage 11/17 do Eagle thả (routine 18). Ước lượng (Enemy.Fleaman): chờ tới khi |dx| &lt;
/// triggerDistancePx ⇒ chu kỳ longEvery cú: (longEvery − 1) cú ngắn (shortVx/shortVy0) + 1 cú vọt (longVx/longVy0), gravity chung,
/// mỗi cú hướng về Simon lúc bật; đáp ⇒ nghỉ pauseMin..+span tick (ngẫu nhiên — "nhìn như ngẫu nhiên"). Khung: fleaman_0 trên đất,
/// fleaman_1 trên không (SUY LUẬN theo hình). Đụng tường giữa không trung ⇒ mất đà ngang. Đặt bằng SpawnPoint (hoặc Eagle — 18).
/// CVGP 18: Carried — Eagle đặt vị trí mỗi tick (dưới chân chim), không tự gỡ ở mép; Release() ⇒ rơi thẳng (vx 0, gravity) rồi chạy
/// chu kỳ nhảy như thường (không chờ trigger).
/// </summary>
public class Fleaman : EnemyBase
{
    public StateMachine<Fleaman, eFleamanState> Fsm { get; private set; }
    [System.NonSerialized] public int trigger, shortVx, shortVy0, longVx, longVy0, gravity, longEvery, pauseMin, pauseSpan;
    [System.NonSerialized] public int hopIndex, pauseLen, hopVx;
    /// <summary>Số cú nhảy ngắn / vọt + tuổi lúc bắt đầu (harness).</summary>
    [System.NonSerialized] public int shortHops, longHops, triggerAge;
    private AnimSet.Anim aHop;
    /// <summary>Eagle đang mang (CVGP 18) — null khi tự do.</summary>
    [System.NonSerialized] public Eagle carrier;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState + (Fsm.Is(eFleamanState.Hop) ? (hopVx == longVx ? "L" : "S") : "") : "-";

    abstract class FleaState : BaseState<Fleaman>
    {
        protected void SetState(eFleamanState s) { owner.Fsm.SetState(s); }
    }

    class WaitState : FleaState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            owner.Pose(0);
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) < owner.trigger)
            {
                owner.triggerAge = owner.ageTicks;
                owner.checkOffscreen = true;
                owner.pauseLen = 0;
                SetState(eFleamanState.Ground);
            }
        }
    }

    class GroundState : FleaState
    {
        public override void OnEnterState() { owner.Pose(0); }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
            {
                owner.hopVx = 0;
                owner.vy = 0;
                SetState(eFleamanState.Hop);
            }
            else if (StateFrame > owner.pauseLen)   // nghỉ đúng pauseLen tick trên đất
                owner.StartHop();
        }
    }

    class HopState : FleaState
    {
        public override void OnEnterState() { owner.Pose(1); }

        public override void OnExecuteState()
        {
            if (owner.hopVx != 0 && owner.StepX(owner.Facing * owner.hopVx))
                owner.hopVx = 0;
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                owner.pauseLen = owner.Roll(owner.pauseMin, owner.pauseSpan);
                SetState(eFleamanState.Ground);
            }
        }
    }

    class CarriedState : FleaState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;   // Eagle lo (bay ra khỏi màn còn mang ⇒ gỡ cả hai)
            owner.Pose(1);
        }
    }

    /// <summary>Eagle bắt đầu mang (gọi ngay sau Spawn).</summary>
    public void CarryBy(Eagle e)
    {
        carrier = e;
        Facing = e.Facing;
        Fsm.SetState(eFleamanState.Carried);
    }

    /// <summary>Eagle thả: rơi thẳng (vx 0, vy 0, gravity) ⇒ đáp ⇒ nghỉ ⇒ nhảy về phía Simon.</summary>
    public void Release()
    {
        carrier = null;
        triggerAge = ageTicks;
        checkOffscreen = true;
        hopVx = 0;
        vy = 0;
        Fsm.SetState(eFleamanState.Hop);
    }

    private void StartHop()
    {
        Facing = FacingToPlayer();
        bool big = longEvery > 0 && hopIndex % longEvery == longEvery - 1;
        hopIndex++;
        if (big) longHops++; else shortHops++;
        hopVx = big ? longVx : shortVx;
        vy = big ? longVy0 : shortVy0;
        Fsm.SetState(eFleamanState.Hop);
    }

    private void Pose(int frame)
    {
        if (Anim != null && aHop != null)
            Anim.SetFrame(aHop, frame);
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Fleaman, eFleamanState>(this, "Fleaman");
        Fsm.Add(eFleamanState.Wait, new WaitState());
        Fsm.Add(eFleamanState.Ground, new GroundState());
        Fsm.Add(eFleamanState.Hop, new HopState());
        Fsm.Add(eFleamanState.Carried, new CarriedState());
        if (animSet != null)
            aHop = animSet.Get("hop");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        shortVx = EnemyParams.Fixed256(cfg, id, "shortVx", 1f);
        shortVy0 = EnemyParams.Fixed256(cfg, id, "shortVy0", -2f);
        longVx = EnemyParams.Fixed256(cfg, id, "longVx", 2f);
        longVy0 = EnemyParams.Fixed256(cfg, id, "longVy0", -4.5f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.25f);
        longEvery = EnemyParams.Int(cfg, id, "longEvery", 3);
        trigger = EnemyParams.Int(cfg, id, "triggerDistancePx", 112);
        pauseMin = EnemyParams.Int(cfg, id, "pauseMinTicks", 4);
        pauseSpan = EnemyParams.Int(cfg, id, "pauseSpanTicks", 12);
        hopIndex = shortHops = longHops = 0;
        triggerAge = -1;
        hopVx = 0;
        vy = 0;
        carrier = null;
        Facing = FacingToPlayer();
        Fsm.SetState(eFleamanState.Wait);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
