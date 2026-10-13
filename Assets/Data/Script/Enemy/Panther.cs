using UnityEngine;

/// <summary>State Panther (Black Leopard).</summary>
public enum ePantherState
{
    Sit,
    Run,
    Jump,
}

/// <summary>
/// Panther — "Black Leopard" trên sheet (CVGP 13). Đo video Map_1 (§2k, 3 con EH): nằm chờ (leopard_0) tới khi |Simon.X − X| &lt; 70
/// ⇒ chạy 2 px/tick theo hướng đang nhìn (khung leopard_2→1→3 mỗi 7 tick) ⇒ hết sàn dưới pivot (mép gờ) ⇒ nhảy: vx 2, vy0 −1.5,
/// g 0.125 (đỉnh ~9 px, leopard_1) ⇒ đáp ⇒ quay về phía Simon MỘT lần ⇒ chạy thẳng tới khi ra khỏi màn (video f1799–1860: chạy qua
/// Simon không quay lại). Đặt bằng SpawnPoint (facing 0 = nhìn về Simon lúc sinh). Đụng tường ⇒ quay đầu (quy ước).
/// </summary>
public class Panther : EnemyBase
{
    public static readonly RectInt BOX_SIT = new RectInt(-12, -15, 24, 16);    // bbox leopard_0 (+1 vẽ)
    public static readonly RectInt BOX_MOVE = new RectInt(-16, -15, 32, 16);   // bbox leopard_1..3

    public StateMachine<Panther, ePantherState> Fsm { get; private set; }
    [System.NonSerialized] public int runVx, jumpVx, jumpVy0, gravity, trigger;
    /// <summary>Tick (tuổi) lúc bật dậy / cất cánh / đáp — harness đọc.</summary>
    [System.NonSerialized] public int triggerAge, takeoffAge, landAge;
    private AnimSet.Anim aSit, aRun, aJump;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class PantherState : BaseState<Panther>
    {
        protected void SetState(ePantherState s) { owner.Fsm.SetState(s); }
    }

    class SitState : PantherState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            owner.SetBoxes(BOX_SIT);
            if (owner.Anim != null && owner.aSit != null)
                owner.Anim.Play(owner.aSit, true);
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) < owner.trigger)
            {
                owner.triggerAge = owner.ageTicks;
                SetState(ePantherState.Run);
            }
        }
    }

    class RunState : PantherState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = true;
            owner.SetBoxes(BOX_MOVE);
            if (owner.Anim != null && owner.aRun != null)
                owner.Anim.Play(owner.aRun);
        }

        public override void OnExecuteState()
        {
            if (owner.StepX(owner.Facing * owner.runVx))
                owner.Facing = -owner.Facing;
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(ePantherState.Jump);
        }
    }

    class JumpState : PantherState
    {
        public override void OnEnterState()
        {
            owner.vy = owner.jumpVy0;
            owner.takeoffAge = owner.ageTicks;
            if (owner.Anim != null && owner.aJump != null)
                owner.Anim.Play(owner.aJump, true);
        }

        public override void OnExecuteState()
        {
            if (owner.StepX(owner.Facing * owner.jumpVx))
                owner.Facing = -owner.Facing;
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                owner.landAge = owner.ageTicks;
                owner.Facing = owner.FacingToPlayer();
                SetState(ePantherState.Run);
            }
        }
    }

    private void SetBoxes(RectInt r)
    {
        Hurt.local = r;
        Touch.local = r;
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Panther, ePantherState>(this, "Panther");
        Fsm.Add(ePantherState.Sit, new SitState());
        Fsm.Add(ePantherState.Run, new RunState());
        Fsm.Add(ePantherState.Jump, new JumpState());
        if (animSet != null)
        {
            aSit = animSet.Get("sit");
            aRun = animSet.Get("run");
            aJump = animSet.Get("jump");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        runVx = EnemyParams.MoveSpeed(cfg, enemyId, 2f);
        jumpVx = EnemyParams.Fixed256(cfg, enemyId, "jumpVx", 2f);
        jumpVy0 = EnemyParams.Fixed256(cfg, enemyId, "jumpVy0", -1.5f);
        gravity = EnemyParams.Fixed256(cfg, enemyId, "gravity", 0.125f);
        trigger = EnemyParams.Int(cfg, enemyId, "triggerDistancePx", 70);
        triggerAge = takeoffAge = landAge = -1;
        vy = 0;
        Fsm.SetState(ePantherState.Sit);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
