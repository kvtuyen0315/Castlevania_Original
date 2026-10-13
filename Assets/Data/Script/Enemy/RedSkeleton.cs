using UnityEngine;

/// <summary>State RedSkeleton.</summary>
public enum eRedSkeletonState
{
    Walk,
    Fall,
    Collapse,   // đổ: redskeleton_1 rồi đống xương redskeleton_0
    Pile,       // nằm thành đống xương
    Rise,       // ráp lại: redskeleton_1 rồi đi tiếp
}

/// <summary>
/// RedSkeleton (CVGP 18, theo wiki — GẦN ĐÚNG, không có video Block 5). Wiki Enemies#Red Skeletons + Stages 13-15: bộ xương bị phù
/// phép KHÔNG giết được — trúng đòn thì đổ thành đống xương, "một hai giây sau" đứng dậy tiếp tục chậm rãi đuổi theo Simon; 1/1 đòn,
/// 400 đ. Ước lượng (Enemy.RedSkeleton): đi ConfigEnemy.moveSpeed về phía Simon (quay lại khi Simon ở phía sau quá turnDeadzonePx);
/// hết sàn ⇒ rơi thẳng như Zombie; đụng tường ⇒ đứng dậm chân. Trúng đòn (mọi vũ khí, 1 đòn) ⇒ +điểm wiki MỖI lần đổ (SUY LUẬN —
/// wiki cho điểm mà không có cách hạ) + tia lửa + Collapse collapseTicks (khung _1) ⇒ Pile reviveTicks (đống xương _0; không nhận /
/// không gây đòn — SUY LUẬN) ⇒ Rise riseTicks (khung _1) ⇒ Walk. Rosary/Stopwatch như enemy thường (gỡ / đứng yên).
/// Đặt bằng SpawnPoint (hành lang Stage 13–15).
/// </summary>
public class RedSkeleton : EnemyBase
{
    public StateMachine<RedSkeleton, eRedSkeletonState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, gravity, deadzone, collapseTicks, reviveTicks, riseTicks;
    /// <summary>Số lần bị đánh đổ (harness).</summary>
    [System.NonSerialized] public int collapses;
    private AnimSet.Anim aWalk, aCollapse;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class RedState : BaseState<RedSkeleton>
    {
        protected void SetState(eRedSkeletonState s) { owner.Fsm.SetState(s); }
    }

    class WalkState : RedState
    {
        public override void OnEnterState()
        {
            owner.SetDown(false);
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk);
        }

        public override void OnExecuteState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) > owner.deadzone)
                owner.Facing = p.X > owner.X ? 1 : -1;
            owner.StepX(owner.Facing * owner.walkVx);
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
                SetState(eRedSkeletonState.Fall);
        }
    }

    class FallState : RedState
    {
        public override void OnEnterState() { owner.vy = 0; }

        public override void OnExecuteState()
        {
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                SetState(eRedSkeletonState.Walk);
            }
        }
    }

    class CollapseState : RedState
    {
        public override void OnEnterState()
        {
            owner.SetDown(true);
            owner.Pose(1);
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.collapseTicks)   // vào từ CombatResolver (sau Fsm.Tick) ⇒ tick trúng đòn đã vẽ khung đổ: >= cho đúng N tick
                SetState(eRedSkeletonState.Pile);
        }
    }

    class PileState : RedState
    {
        public override void OnEnterState() { owner.Pose(0); }

        public override void CheckNextState()
        {
            if (StateFrame > owner.reviveTicks)
                SetState(eRedSkeletonState.Rise);
        }
    }

    class RiseState : RedState
    {
        public override void OnEnterState() { owner.Pose(1); }

        public override void CheckNextState()
        {
            if (StateFrame > owner.riseTicks)
                SetState(eRedSkeletonState.Walk);
        }
    }

    public bool IsDown => Fsm != null && Fsm.HasState
        && (Fsm.Is(eRedSkeletonState.Collapse) || Fsm.Is(eRedSkeletonState.Pile) || Fsm.Is(eRedSkeletonState.Rise));

    private bool down;

    /// <summary>Nằm/đang đổ/đang ráp: không nhận, không gây đòn.</summary>
    private void SetDown(bool d)
    {
        down = d;
        ApplyVisibility();
    }

    protected override void ApplyVisibility()
    {
        base.ApplyVisibility();
        if (down)
        {
            Hurt.active = false;
            Touch.active = false;
        }
    }

    private void Pose(int frame)
    {
        if (Anim != null && aCollapse != null)
            Anim.SetFrame(aCollapse, frame);
    }

    /// <summary>Không chết: hết máu ⇒ đổ (điểm mỗi lần đổ) rồi hồi máu.</summary>
    public override bool OnHurt(ref HitInfo hit)
    {
        if (hidden || !Alive || down)
            return false;
        Health.Apply(hit.strength);
        if (Health.IsDead)
            Collapse();
        return true;
    }

    private void Collapse()
    {
        collapses++;
        defeated = false;
        var cfg = World.Config;
        FxSprite.Spawn(World, "hit", X, CenterY, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HitFlame);
        if (Score > 0)
        {
            GameRules.AddScore(Score);
            GameSession.Current.NotifyChanged();
        }
        ResetHealth(cfg);
        vy = 0;
        Fsm.SetState(eRedSkeletonState.Collapse);
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<RedSkeleton, eRedSkeletonState>(this, "RedSkeleton");
        Fsm.Add(eRedSkeletonState.Walk, new WalkState());
        Fsm.Add(eRedSkeletonState.Fall, new FallState());
        Fsm.Add(eRedSkeletonState.Collapse, new CollapseState());
        Fsm.Add(eRedSkeletonState.Pile, new PileState());
        Fsm.Add(eRedSkeletonState.Rise, new RiseState());
        if (animSet != null)
        {
            aWalk = animSet.Get("walk");
            aCollapse = animSet.Get("collapse");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 0.5f);
        gravity = EnemyParams.Fixed256(cfg, id, "fallGravity", 0.125f);
        deadzone = EnemyParams.Int(cfg, id, "turnDeadzonePx", 16);
        collapseTicks = Mathf.Max(1, EnemyParams.Int(cfg, id, "collapseTicks", 8));
        reviveTicks = Mathf.Max(1, EnemyParams.Int(cfg, id, "reviveTicks", 90));
        riseTicks = Mathf.Max(1, EnemyParams.Int(cfg, id, "riseTicks", 8));
        checkOffscreen = false;   // đặt sẵn — SpawnPoint lo
        collapses = 0;
        down = false;
        vy = 0;
        if (Facing == 0)
            Facing = FacingToPlayer();
        Fsm.SetState(Grounded() ? eRedSkeletonState.Walk : eRedSkeletonState.Fall);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
