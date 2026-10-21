using UnityEngine;

/// <summary>State Fleaman (Hunchback).</summary>
public enum eFleamanState
{
    Wait,    // ngồi chờ sẵn (wiki Stage 13; video Map_3 trên bục), khung 0↔1
    Ground,  // nghỉ giữa 2 cú nhảy
    Hop,     // trên không (cả rơi khỏi mép)
    Carried, // Eagle đang mang (CVGP 18 — wiki Stage 11/17)
}

/// <summary>
/// Fleaman — "Hunchback/Fleaman" trên sheet. CVGP 17 dựng theo wiki; CVM3 07 đo lại theo video Map_3 (3 con WEA 0:16–0:31 — CVM3_MEMORY
/// §2g). Wiki Enemies#Fleamen: nhảy như ngẫu nhiên, kiểu thường gặp 2 bước nhảy ngắn rồi 1 cú nhảy vọt lớn; 1/1 đòn, 500 đ; Stage 13 đứng
/// chờ sẵn, Stage 11/17 do Eagle thả (routine 18). Video: ngồi chờ trên bục, khung fleaman_0↔1 đổi mỗi waitFrameTicks (9) tick; khi
/// |Simon − X| &lt; triggerDistancePx thì đếm thêm triggerDelayMin..+span tick (video 62 / 62 / 136) rồi NGỒI THẤP fleaman_0 crouchTicks (13)
/// tick ⇒ cú ngắn (shortVx 2.25 / shortVy0 −1 / gravity 0.125 — đỉnh 3.5 px; từ bục cao rơi xuống trước mặt Simon). Sau đó chu kỳ
/// longEvery cú: (longEvery − 1) cú ngắn + 1 cú vọt (longVx/longVy0 — video không có, suy luận), mỗi cú hướng về Simon lúc bật; đáp ⇒
/// nghỉ pauseMin..+span tick (suy luận CVGP 17). Khung: fleaman_0 trên đất, fleaman_1 trên không (video 3/3). Đụng tường giữa không trung
/// ⇒ mất đà ngang. Đặt bằng SpawnPoint (hoặc Eagle — 18).
/// CVGP 18: Carried — Eagle đặt vị trí mỗi tick (dưới chân chim), không tự gỡ ở mép; Release() ⇒ rơi thẳng (vx 0, gravity) rồi chạy
/// chu kỳ nhảy như thường (không chờ trigger).
/// </summary>
public class Fleaman : EnemyBase
{
    public StateMachine<Fleaman, eFleamanState> Fsm { get; private set; }
    [System.NonSerialized] public int trigger, shortVx, shortVy0, longVx, longVy0, gravity, longEvery, pauseMin, pauseSpan;
    [System.NonSerialized] public int triggerDelayMin, triggerDelaySpan, crouchTicks;
    [System.NonSerialized] public int hopIndex, pauseLen, hopVx, triggerLeft;
    [System.NonSerialized] public bool hopBig;
    /// <summary>Harness: số tick đếm sau khi Simon vào tầm (≥ 0 ⇒ thay Roll — ép theo video).</summary>
    [System.NonSerialized] public int forceTriggerDelay = -1;
    /// <summary>Số cú nhảy ngắn / vọt + tuổi lúc bắt đầu (harness).</summary>
    [System.NonSerialized] public int shortHops, longHops, triggerAge;
    private AnimSet.Anim aHop;
    /// <summary>Eagle đang mang (CVGP 18) — null khi tự do.</summary>
    [System.NonSerialized] public Eagle carrier;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState + (Fsm.Is(eFleamanState.Hop) ? (hopBig ? "L" : "S") : "") : "-";

    abstract class FleaState : BaseState<Fleaman>
    {
        protected void SetState(eFleamanState s) { owner.Fsm.SetState(s); }
    }

    class WaitState : FleaState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            owner.triggerLeft = -1;
            if (owner.Anim != null && owner.aHop != null)
                owner.Anim.Play(owner.aHop, true);   // video: khung 0↔1 mỗi 9 tick khi ngồi chờ
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (owner.triggerLeft < 0)
            {
                if (p == null || Mathf.Abs(p.X - owner.X) >= owner.trigger)
                    return;
                owner.triggerAge = owner.ageTicks;
                owner.triggerLeft = owner.forceTriggerDelay >= 0 ? owner.forceTriggerDelay : owner.Roll(owner.triggerDelayMin, owner.triggerDelaySpan);
            }
            if (owner.triggerLeft-- > 0)
                return;
            owner.checkOffscreen = true;
            owner.pauseLen = owner.crouchTicks;
            SetState(eFleamanState.Ground);
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
                owner.hopBig = false;
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
        hopBig = false;
        vy = 0;
        Fsm.SetState(eFleamanState.Hop);
    }

    private void StartHop()
    {
        Facing = FacingToPlayer();
        bool big = longEvery > 0 && hopIndex % longEvery == longEvery - 1;
        hopIndex++;
        if (big) longHops++; else shortHops++;
        hopBig = big;
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
        shortVx = EnemyParams.Fixed256(cfg, id, "shortVx", 2.25f);
        shortVy0 = EnemyParams.Fixed256(cfg, id, "shortVy0", -1f);
        longVx = EnemyParams.Fixed256(cfg, id, "longVx", 2.25f);
        longVy0 = EnemyParams.Fixed256(cfg, id, "longVy0", -3.25f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.125f);
        longEvery = EnemyParams.Int(cfg, id, "longEvery", 3);
        trigger = EnemyParams.Int(cfg, id, "triggerDistancePx", 128);
        triggerDelayMin = EnemyParams.Int(cfg, id, "triggerDelayMinTicks", 62);
        triggerDelaySpan = EnemyParams.Int(cfg, id, "triggerDelaySpanTicks", 74);
        crouchTicks = EnemyParams.Int(cfg, id, "crouchTicks", 13);
        pauseMin = EnemyParams.Int(cfg, id, "pauseMinTicks", 4);
        pauseSpan = EnemyParams.Int(cfg, id, "pauseSpanTicks", 12);
        hopIndex = shortHops = longHops = 0;
        triggerAge = -1;
        triggerLeft = -1;
        forceTriggerDelay = -1;
        hopVx = 0;
        hopBig = false;
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
