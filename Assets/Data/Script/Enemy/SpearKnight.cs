using UnityEngine;

/// <summary>State SpearKnight (Black Knight).</summary>
public enum eSpearKnightState
{
    Walk,
    Fall,
}

/// <summary>
/// SpearKnight — "Black Knight" trên sheet (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 2). Wiki Enemies#Spear Knights:
/// đi tới lui chậm, có thể quay lại bất cứ lúc nào, KHÔNG đâm/ném giáo — chỉ gây hại khi chạm thân; 3 đòn yếu / 2 đòn mạnh, 400 đ.
/// Ước lượng (ConfigActorParam Enemy.SpearKnight, nguồn "suy luận: ước lượng (không video) — routine 17"): đi 0.5 px/tick; đổi hướng
/// ngẫu nhiên sau mỗi đoạn turnMin..turnMin+turnSpan tick (RNG thế giới); đụng tường hoặc hết sàn cách pivot edgeProbePx theo hướng
/// đi ⇒ quay đầu (không tự rơi khỏi bục); sinh lơ lửng ⇒ rơi thẳng như Zombie. Mỗi đòn chưa hạ: tia lửa + bất tử hitInvincibleTicks.
/// Đặt bằng SpawnPoint (đứng chờ trong màn — wiki Stage 4).
/// </summary>
public class SpearKnight : EnemyBase
{
    public StateMachine<SpearKnight, eSpearKnightState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, gravity, turnMin, turnSpan, edgeProbe, legTimer;
    /// <summary>Số lần quay đầu (harness).</summary>
    [System.NonSerialized] public int turns;
    private AnimSet.Anim aWalk;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class KnightState : BaseState<SpearKnight>
    {
        protected void SetState(eSpearKnightState s) { owner.Fsm.SetState(s); }
    }

    class WalkState : KnightState
    {
        public override void OnEnterState()
        {
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk);
        }

        public override void OnExecuteState()
        {
            if (--owner.legTimer <= 0)
                owner.Turn();
            else if (!owner.FloorAhead())
                owner.Turn();
            if (owner.StepX(owner.Facing * owner.walkVx))
                owner.Turn();
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(eSpearKnightState.Fall);
        }
    }

    class FallState : KnightState
    {
        public override void OnEnterState() { owner.vy = 0; }

        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                SetState(eSpearKnightState.Walk);
            }
        }
    }

    private void Turn()
    {
        Facing = -Facing;
        legTimer = Roll(turnMin, turnSpan);
        turns++;
    }

    /// <summary>Sàn ở cột pivot + hướng·edgeProbe (cùng hàng Y).</summary>
    private bool FloorAhead()
    {
        int x = X + Facing * edgeProbe;
        return World.Tiles == null || World.Tiles.RowLandable(Y, x, x);
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<SpearKnight, eSpearKnightState>(this, "SpearKnight");
        Fsm.Add(eSpearKnightState.Walk, new WalkState());
        Fsm.Add(eSpearKnightState.Fall, new FallState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 0.5f);
        gravity = EnemyParams.Fixed256(cfg, id, "fallGravity", 0.125f);
        turnMin = EnemyParams.Int(cfg, id, "turnMinTicks", 64);
        turnSpan = EnemyParams.Int(cfg, id, "turnSpanTicks", 128);
        edgeProbe = EnemyParams.Int(cfg, id, "edgeProbePx", 8);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        multiHit = true;
        checkOffscreen = false;   // đặt sẵn — SpawnPoint lo gỡ khi ra khỏi lề
        turns = 0;
        vy = 0;
        if (Facing == 0)
            Facing = FacingToPlayer();
        legTimer = Roll(turnMin, turnSpan);
        Fsm.SetState(Grounded() ? eSpearKnightState.Walk : eSpearKnightState.Fall);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
