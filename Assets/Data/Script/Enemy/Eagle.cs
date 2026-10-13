using UnityEngine;

/// <summary>State Eagle.</summary>
public enum eEagleState
{
    Carry,   // bay ngang mang Fleaman
    Fly,     // đã thả (hoặc Fleaman bị hạ) — bay tiếp ra khỏi màn
}

/// <summary>
/// Eagle (CVGP 18, theo wiki — GẦN ĐÚNG, không có video Block 4/6). Wiki Enemies#Eagles: chỉ ở vài nơi, LUÔN mang Fleaman; Fleaman rơi
/// xuống gần Simon rồi đánh như thường; hạ Eagle khi còn mang ⇒ cả hai cùng chết; 1/1 đòn, 300 đ. Stages 10-12#Stage 11: chim bay tới,
/// thả Fleaman gần Simon rồi bay đi, con này nối con kia tới cuối stage; Fleaman "hầu như luôn rơi đúng tầm roi".
/// Ước lượng (Enemy.Eagle): EdgeSpawner FrontOfPlayer + playerRelativeY (tâm = chân Simon − spawnAboveFeetPx); bay ngang thẳng
/// ConfigEnemy.moveSpeed về phía trong màn; Fleaman treo dưới chân (pivot Fleaman = tâm chim + carryBelowPx); khi |Simon − Eagle| ≤
/// dropDistancePx (lần đầu, đang bay TỚI Simon) ⇒ thả (Fleaman.Release: rơi thẳng) ⇒ bay tiếp tới khi ra khỏi màn (gỡ, mang theo
/// Fleaman nếu chưa thả). Fleaman bị hạ khi đang treo ⇒ chim bay tiếp. Khung eagle_0/1 vỗ cánh 8 tick (ước lượng).
/// </summary>
public class Eagle : EnemyBase
{
    [SerializeField] private Fleaman fleamanPrefab;

    public StateMachine<Eagle, eEagleState> Fsm { get; private set; }
    [System.NonSerialized] public int flyVx, carryBelow, dropDist;
    [System.NonSerialized] public Fleaman carried;
    private int carriedSpawnTick;
    /// <summary>Tuổi lúc thả / X lúc thả (harness; −1 = chưa thả).</summary>
    [System.NonSerialized] public int dropAge, dropX;
    private AnimSet.Anim aFly;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class EagleState : BaseState<Eagle>
    {
        protected void SetState(eEagleState s) { owner.Fsm.SetState(s); }
    }

    class CarryState : EagleState
    {
        public override void OnExecuteState()
        {
            owner.HoldCarried();
        }

        public override void CheckNextState()
        {
            if (!owner.CarriedAlive())
            {
                owner.carried = null;
                SetState(eEagleState.Fly);
                return;
            }
            var p = owner.World.Player;
            if (p == null)
                return;
            int dx = p.X - owner.X;
            // chỉ thả khi đang bay TỚI Simon (đã qua đầu thì không thả ngược)
            if (Mathf.Abs(dx) <= owner.dropDist && (dx == 0 || (dx > 0) == (owner.Facing > 0)))
            {
                owner.dropAge = owner.ageTicks;
                owner.dropX = owner.X;
                var f = owner.carried;
                owner.carried = null;
                f.Release();
                SetState(eEagleState.Fly);
            }
        }
    }

    class FlyState : EagleState
    {
    }

    private bool CarriedAlive()
    {
        return carried != null && carried.Alive && carried.SpawnTick == carriedSpawnTick && carried.carrier == this;
    }

    /// <summary>Đặt Fleaman dưới chân chim (cùng tick, sau khi chim dời).</summary>
    private void HoldCarried()
    {
        if (!CarriedAlive())
            return;
        carried.fx = fx;
        carried.fy = fy + Fixed.FromPx(carryBelow);
        carried.Facing = Facing;
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Eagle, eEagleState>(this, "Eagle");
        Fsm.Add(eEagleState.Carry, new CarryState());
        Fsm.Add(eEagleState.Fly, new FlyState());
        if (animSet != null)
            aFly = animSet.Get("fly");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        flyVx = EnemyParams.MoveSpeed(cfg, id, 1.5f);
        carryBelow = EnemyParams.Int(cfg, id, "carryBelowPx", 28);
        dropDist = EnemyParams.Int(cfg, id, "dropDistancePx", 28);
        dropAge = dropX = -1;
        carried = null;
        if (Anim != null && aFly != null)
            Anim.Play(aFly, true);
        // Fleaman sinh cùng tick, ngay sau chim (thứ tự nhóm Enemy: chim tick trước ⇒ đặt vị trí Fleaman trước khi Fleaman tick)
        if (fleamanPrefab != null)
        {
            var f = World.Spawn(fleamanPrefab, X, Y + carryBelow, Facing) as Fleaman;
            if (f != null)
            {
                carried = f;
                carriedSpawnTick = f.SpawnTick;
                f.CarryBy(this);
            }
        }
        Fsm.SetState(carried != null ? eEagleState.Carry : eEagleState.Fly);
    }

    protected override void TickStates()
    {
        fx += Facing * flyVx;   // dời 1 lần/tick ở đây (Carry → Fly chạy nối cùng tick không dời lần 2)
        Fsm.Tick();
    }

    /// <summary>Hạ chim khi còn mang ⇒ Fleaman chết cùng lúc (wiki) — tính điểm cả hai (SUY LUẬN: wiki "cả hai bị giết").</summary>
    protected override void Die()
    {
        var f = CarriedAlive() ? carried : null;
        carried = null;
        base.Die();
        if (f != null)
        {
            f.carrier = null;
            f.Defeat();
        }
    }

    protected override void OnRemoved()
    {
        // chim rời thế giới (ra khỏi màn / Rosary) khi còn mang ⇒ gỡ Fleaman theo (không điểm)
        if (CarriedAlive())
            carried.Kill(KillReason == eKillReason.Offscreen ? eKillReason.Offscreen : eKillReason.Killed);
        carried = null;
        base.OnRemoved();
    }
}
