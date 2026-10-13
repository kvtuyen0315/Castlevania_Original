using UnityEngine;

/// <summary>State BoneTower (Dragon Skull Cannon).</summary>
public enum eBoneTowerState
{
    Idle,
    Warn,   // nhấp nháy trước khi bắn
    Fire,   // 2 cầu lửa: đầu trên rồi đầu dưới
}

/// <summary>
/// BoneTower — "Dragon Skull Cannon" trên sheet (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 2–3). Wiki Enemies#Bone Towers:
/// bất động; theo chu kỳ phun 2 cầu lửa, nhấp nháy ngay trước khi bắn; cầu lửa quất vỡ được; 8 đòn yếu / 6 đòn mạnh, 400 đ.
/// Hình: 2 đầu lâu 16×16 chồng nhau (AnimSet: thân = đầu dưới, lớp phủ = đầu trên). Nhấp nháy = đổi bảng tím ↔ đỏ (sheet có 2 bộ
/// màu — SUY LUẬN). Ước lượng (Enemy.BoneTower): nghỉ firstIdle rồi idleMin..+span tick ⇒ nháy warnTicks (đổi màu mỗi warnBlinkTicks)
/// ⇒ quay về phía Simon, bắn cầu lửa đầu trên, fireGapTicks sau bắn đầu dưới (BoneTowerFireball bay ngang fireballSpeed) ⇒ nghỉ.
/// Đặt bằng SpawnPoint. Mỗi đòn chưa hạ: tia lửa + bất tử hitInvincibleTicks.
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
        firstIdle = EnemyParams.Int(cfg, id, "firstIdleTicks", 48);
        idleMin = EnemyParams.Int(cfg, id, "idleMinTicks", 96);
        idleSpan = EnemyParams.Int(cfg, id, "idleSpanTicks", 64);
        warnTicks = EnemyParams.Int(cfg, id, "warnTicks", 32);
        warnBlink = Mathf.Max(1, EnemyParams.Int(cfg, id, "warnBlinkTicks", 4));
        fireGap = Mathf.Max(1, EnemyParams.Int(cfg, id, "fireGapTicks", 16));
        fireballVx = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 1.5f);
        fireballFront = EnemyParams.Int(cfg, id, "fireballFrontPx", 8);
        topAbove = EnemyParams.Int(cfg, id, "fireballTopAbovePx", 16);
        bottomAbove = EnemyParams.Int(cfg, id, "fireballBottomAbovePx", 0);
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
        Fsm.Tick();
    }
}
