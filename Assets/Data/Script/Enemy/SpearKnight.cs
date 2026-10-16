using UnityEngine;

/// <summary>State SpearKnight (Black Knight).</summary>
public enum eSpearKnightState
{
    Walk,
    Fall,
}

/// <summary>
/// SpearKnight — "Black Knight" trên sheet (CVGP 17 theo wiki; CVM2 08 ĐO LẠI video Map_2 — 7 con, ≈ 1 100 frame). Wiki Enemies#Spear
/// Knights: đi tới lui chậm, có thể quay lại bất cứ lúc nào, KHÔNG đâm/ném giáo — chỉ gây hại khi chạm thân; 3 đòn yếu / 2 đòn mạnh, 400 đ.
/// Video (ConfigActorParam Enemy.SpearKnight): đi 0.5 px/tick (1 px / 2 tick); quay đầu NGẪU NHIÊN theo bộ đếm riêng turnMin..+turnSpan
/// tick (chỉ quay ngẫu nhiên mới rút lại bộ đếm — quay ở mép/tường KHÔNG đặt lại: video TEB khe 12 px quay mép đều 12 frame nhưng vẫn
/// có lần quay ngẫu nhiên sau 2–8 frame); hết sàn ở pivot + hướng·edgeProbePx (4) ⇒ quay; ô đặc ở pivot + hướng·wallProbePx (16),
/// cao wallProbeUpPx (8) trên mặt sàn ⇒ quay (TEB khe: quay ở x 336, tường x 352); sinh lơ lửng ⇒ rơi (TEB f866–880 g ≈ 0.12).
/// Mỗi đòn chưa hạ: tia lửa + bất tử hitInvincibleTicks + KHỰNG hitStunTicks (EnemyBase — video đứng ≈ 16 tick, anim dừng).
/// Đặt bằng SpawnPoint (đứng chờ trong màn — wiki Stage 4).
/// </summary>
public class SpearKnight : EnemyBase
{
    public StateMachine<SpearKnight, eSpearKnightState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, gravity, turnMin, turnSpan, edgeProbe, wallProbe, wallProbeUp, legTimer;
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
                owner.Turn(true);
            else if (!owner.FloorAhead() || owner.WallAhead())
                owner.Turn(false);
            if (owner.StepX(owner.Facing * owner.walkVx))
                owner.Turn(false);
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(eSpearKnightState.Fall);
        }
    }

    class FallState : KnightState
    {
        public override void OnEnterState()
        {
            owner.vy = 0;
            // CVM2 08: rơi vẫn chạy khung walk (video TEB f866–880 đổi khung 0→1 giữa lúc rơi); trước đây sinh lơ lửng ⇒ chưa phát anim ⇒ vô hình
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk);
        }

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

    /// <summary>Quay đầu; random = hết bộ đếm ngẫu nhiên ⇒ rút lại bộ đếm (quay ở mép/tường giữ nguyên bộ đếm — video TEB).</summary>
    private void Turn(bool random)
    {
        Facing = -Facing;
        if (random)
            legTimer = Roll(turnMin, turnSpan);
        turns++;
    }

    /// <summary>Sàn ở cột pivot + hướng·edgeProbe (cùng hàng Y).</summary>
    private bool FloorAhead()
    {
        int x = X + Facing * edgeProbe;
        return World.Tiles == null || World.Tiles.RowLandable(Y, x, x);
    }

    /// <summary>Ô đặc ở pivot + hướng·wallProbe, cao wallProbeUp trên mặt sàn (CVM2 08 — video TEB: quay ở x 336 trước tường x 352).</summary>
    private bool WallAhead()
    {
        return wallProbe > 0 && World.Tiles != null && World.Tiles.IsSolid(X + Facing * wallProbe, Y - wallProbeUp);
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
        turnMin = EnemyParams.Int(cfg, id, "turnMinTicks", 16);
        turnSpan = EnemyParams.Int(cfg, id, "turnSpanTicks", 176);
        edgeProbe = EnemyParams.Int(cfg, id, "edgeProbePx", 4);
        wallProbe = EnemyParams.Int(cfg, id, "wallProbePx", 16);
        wallProbeUp = EnemyParams.Int(cfg, id, "wallProbeUpPx", 8);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        hurtStunTicks = EnemyParams.Int(cfg, id, "hitStunTicks", 16);
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
