using UnityEngine;

/// <summary>State BoneTower (Dragon Skull Cannon).</summary>
public enum eBoneTowerState
{
    Idle,
    Warn,   // nhấp nháy trước khi bắn
    Fire,   // 2 cầu lửa: đầu trên rồi đầu dưới
}

/// <summary>
/// BoneTower — "Dragon Skull Cannon" trên sheet (CVGP 17 theo wiki; CVM2 08 ĐO LẠI video Map_2 — 2 tháp Hall C, 7 lượt bắn, 15 cầu lửa).
/// Wiki Enemies#Bone Towers: bất động; theo chu kỳ phun 2 cầu lửa, nhấp nháy ngay trước khi bắn; cầu lửa quất vỡ được; 8/6 đòn, 400 đ.
/// Hình: 2 đầu lâu 16×16 chồng nhau (AnimSet: thân = đầu dưới, lớp phủ = đầu trên; 2 đầu là ảnh gương). Video (Enemy.BoneTower):
/// tháp LUÔN quay về phía Simon (đổi hướng ngay khi Simon đi ngang qua x tháp — f4982/4992/5034); trước lượt bắn đổi bảng ĐỎ
/// warnTicks (8–14 tick ở 4/7 lượt; 3/7 lượt có thêm 1 nháy đỏ ngắn sớm hơn ~18 tick — chưa dựng); 2 cầu lửa CÙNG từ miệng đầu TRÊN
/// (chân tháp − 15, sinh ở x tháp), cách nhau fireGapTicks 40, bay ngang 2.0 px/tick về hướng tháp đang quay; lượt đầu firstIdle sau khi sinh/hết đóng
/// băng; giữa 2 lượt nghỉ idleMin..+idleSpan (khoảng phát 1 → phát 1: 185–314 tick, 6 mẫu). Đặt bằng SpawnPoint. Mỗi đòn chưa hạ:
/// tia lửa + bất tử hitInvincibleTicks (khựng: không thấy trong video — tháp bất động).
/// </summary>
public class BoneTower : EnemyBase
{
    [SerializeField] private BoneTowerFireball fireballPrefab;

    public StateMachine<BoneTower, eBoneTowerState> Fsm { get; private set; }
    [System.NonSerialized] public int firstIdle, idleMin, idleSpan, warnTicks, warnBlink, fireGap, fireballVx, fireballFront, topAbove, bottomAbove;
    [System.NonSerialized] public int idleLen;
    /// <summary>Số cầu lửa đã bắn / số lượt (harness).</summary>
    [System.NonSerialized] public int shots, volleys;
    private AnimSet.Anim aIdle, aFlash;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class TowerState : BaseState<BoneTower>
    {
        protected void SetState(eBoneTowerState s) { owner.Fsm.SetState(s); }
    }

    class IdleState : TowerState
    {
        public override void OnEnterState() { owner.Pose(false); }

        public override void CheckNextState()
        {
            if (StateFrame > owner.idleLen)   // đứng đúng idleLen tick rồi mới nháy
                SetState(eBoneTowerState.Warn);
        }
    }

    class WarnState : TowerState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
            owner.Pose(true);
        }

        public override void OnExecuteState()
        {
            owner.Pose((StateFrame / owner.warnBlink) % 2 == 0);
        }

        public override void CheckNextState()
        {
            if (StateFrame > owner.warnTicks)   // nháy đủ warnTicks tick
                SetState(eBoneTowerState.Fire);
        }
    }

    class FireState : TowerState
    {
        public override void OnEnterState()
        {
            owner.Pose(false);
            owner.volleys++;
        }

        public override void OnExecuteState()
        {
            if (StateFrame == 1)
                owner.Shoot(owner.topAbove);
            else if (StateFrame == 1 + owner.fireGap)
                owner.Shoot(owner.bottomAbove);
        }

        public override void CheckNextState()
        {
            if (StateFrame >= 1 + owner.fireGap)
            {
                owner.idleLen = owner.Roll(owner.idleMin, owner.idleSpan);
                SetState(eBoneTowerState.Idle);
            }
        }
    }

    private void Pose(bool flash)
    {
        var a = flash ? aFlash : aIdle;
        if (Anim != null && a != null)
            Anim.SetFrame(a, 0);
    }

    private void Shoot(int above)
    {
        shots++;
        if (fireballPrefab == null)
            return;
        var f = World.Spawn(fireballPrefab, X + Facing * fireballFront, Y - above, Facing) as BoneTowerFireball;
        if (f != null)
            f.vx = Facing * fireballVx;
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<BoneTower, eBoneTowerState>(this, "BoneTower");
        Fsm.Add(eBoneTowerState.Idle, new IdleState());
        Fsm.Add(eBoneTowerState.Warn, new WarnState());
        Fsm.Add(eBoneTowerState.Fire, new FireState());
        if (animSet != null)
        {
            aIdle = animSet.Get("purple");
            aFlash = animSet.Get("red");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        firstIdle = EnemyParams.Int(cfg, id, "firstIdleTicks", 28);
        idleMin = EnemyParams.Int(cfg, id, "idleMinTicks", 133);
        idleSpan = EnemyParams.Int(cfg, id, "idleSpanTicks", 129);
        warnTicks = EnemyParams.Int(cfg, id, "warnTicks", 12);
        warnBlink = Mathf.Max(1, EnemyParams.Int(cfg, id, "warnBlinkTicks", 16));
        fireGap = Mathf.Max(1, EnemyParams.Int(cfg, id, "fireGapTicks", 40));
        fireballVx = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 2f);
        fireballFront = EnemyParams.Int(cfg, id, "fireballFrontPx", 0);
        topAbove = EnemyParams.Int(cfg, id, "fireballTopAbovePx", 15);
        bottomAbove = EnemyParams.Int(cfg, id, "fireballBottomAbovePx", 15);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        multiHit = true;
        checkOffscreen = false;   // bất động — SpawnPoint lo
        shots = volleys = 0;
        idleLen = firstIdle;
        Facing = FacingToPlayer();
        Fsm.SetState(eBoneTowerState.Idle);
    }

    protected override void TickStates()
    {
        Facing = FacingToPlayer();   // CVM2 08: luôn quay về Simon (video f4982/4992/5034), không chỉ lúc nháy
        Fsm.Tick();
    }
}
