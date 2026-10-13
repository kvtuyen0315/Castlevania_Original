using UnityEngine;

/// <summary>State Skeleton (White Skeleton).</summary>
public enum eSkeletonState
{
    Walk,
    Jump,   // cả rơi khỏi mép (vy từ 0)
}

/// <summary>
/// Skeleton — "White Skeleton" trên sheet (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 3). Wiki Enemies#Skeletons: nhanh, nhảy
/// nhiều, ném xương theo vòng cung — hay 2–3 xương cùng lúc theo các đường khác nhau; 1/1 đòn, 300 đ; Stages 13-15: "chọn khoảng
/// cách thuận lợi", có thể "đẩy" chúng đi. Ước lượng (Enemy.Skeleton): luôn quay mặt về Simon; mỗi decideTicks chọn hướng đi —
/// xa hơn keepMaxPx ⇒ tiến, gần hơn keepMinPx ⇒ lùi, trong dải ⇒ ngẫu nhiên — đi ConfigEnemy.moveSpeed; jumpChancePct % nhảy
/// (jumpVy0/jumpVx/gravity), đụng tường ⇒ nhảy (lên bục ≤ 42 px), hết sàn ⇒ rơi; ném: firstThrow rồi mỗi throwMin..+span tick 1 loạt
/// burstMin..+span xương cách burstGapTicks, xương k: vx = boneVx + k·step, vy0 = boneVy0 − k·step (SkeletonBone).
/// Đặt bằng SpawnPoint.
/// </summary>
public class Skeleton : EnemyBase
{
    [SerializeField] private SkeletonBone bonePrefab;

    public StateMachine<Skeleton, eSkeletonState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, keepMin, keepMax, decide, jumpChance, jumpVy0, jumpVx, gravity;
    [System.NonSerialized] public int throwFirst, throwMin, throwSpan, burstMin, burstSpan, burstGap, boneVx, boneVxStep, boneVy0, boneVyStep, boneG, boneHand;
    [System.NonSerialized] public int moveDir, decideTimer, throwTimer, burstLeft, burstIndex, burstTimer;
    private bool movedX;   // đã dời ngang trong tick này (đổi state nối cùng tick — Walk⇄Jump — không dời lần 2)
    /// <summary>Số xương đã ném / số loạt / số cú nhảy (harness).</summary>
    [System.NonSerialized] public int bones, bursts, jumps;
    private AnimSet.Anim aWalk;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class SkelState : BaseState<Skeleton>
    {
        protected void SetState(eSkeletonState s) { owner.Fsm.SetState(s); }
    }

    class WalkState : SkelState
    {
        public override void OnEnterState()
        {
            if (owner.Anim != null && owner.aWalk != null)
                owner.Anim.Play(owner.aWalk);
        }

        public override void OnExecuteState()
        {
            if (--owner.decideTimer <= 0)
            {
                owner.decideTimer = owner.decide;
                owner.ChooseDir();
                if (owner.Roll(0, 99) < owner.jumpChance)
                {
                    owner.StartJump(owner.jumpVy0);
                    return;
                }
            }
            if (owner.MoveOnce(owner.walkVx))
                owner.StartJump(owner.jumpVy0);
        }

        public override void CheckNextState()
        {
            if (!owner.Fsm.Is(eSkeletonState.Jump) && !owner.Grounded())
                owner.StartJump(0);
        }
    }

    class JumpState : SkelState
    {
        public override void OnExecuteState()
        {
            owner.MoveOnce(owner.jumpVx);
            owner.vy += owner.gravity;
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                SetState(eSkeletonState.Walk);
            }
        }
    }

    private void StartJump(int vy0)
    {
        vy = vy0;
        if (vy0 != 0)
            jumps++;
        Fsm.SetState(eSkeletonState.Jump);
    }

    /// <summary>Dời ngang theo moveDir tối đa 1 lần/tick; trả true nếu đụng tường.</summary>
    private bool MoveOnce(int speed)
    {
        if (movedX)
            return false;
        movedX = true;
        return StepX(moveDir * speed);
    }

    private void ChooseDir()
    {
        var p = World.Player;
        if (p == null)
            return;
        int d = Mathf.Abs(p.X - X), toward = p.X >= X ? 1 : -1;
        if (d > keepMax) moveDir = toward;
        else if (d < keepMin) moveDir = -toward;
        else moveDir = Roll(0, 1) == 0 ? toward : -toward;
    }

    /// <summary>Đồng hồ ném chạy cả khi đi lẫn nhảy.</summary>
    private void TickThrow()
    {
        if (burstLeft > 0)
        {
            if (--burstTimer > 0)
                return;
            ThrowBone(burstIndex++);
            burstTimer = burstGap;
            if (--burstLeft == 0)
                throwTimer = Roll(throwMin, throwSpan);
            return;
        }
        if (--throwTimer > 0)
            return;
        bursts++;
        burstLeft = Roll(burstMin, burstSpan);
        burstIndex = 0;
        burstTimer = 1;
        TickThrow();
    }

    private void ThrowBone(int k)
    {
        bones++;
        if (bonePrefab == null)
            return;
        var b = World.Spawn(bonePrefab, X, Y - boneHand, Facing) as SkeletonBone;
        if (b != null)
        {
            b.vx = Facing * (boneVx + k * boneVxStep);
            b.vy = boneVy0 - k * boneVyStep;
            b.gravity = boneG;
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Skeleton, eSkeletonState>(this, "Skeleton");
        Fsm.Add(eSkeletonState.Walk, new WalkState());
        Fsm.Add(eSkeletonState.Jump, new JumpState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 1f);
        keepMin = EnemyParams.Int(cfg, id, "keepMinPx", 48);
        keepMax = EnemyParams.Int(cfg, id, "keepMaxPx", 96);
        decide = Mathf.Max(1, EnemyParams.Int(cfg, id, "decideTicks", 24));
        jumpChance = EnemyParams.Int(cfg, id, "jumpChancePct", 20);
        jumpVy0 = EnemyParams.Fixed256(cfg, id, "jumpVy0", -3.25f);
        jumpVx = EnemyParams.Fixed256(cfg, id, "jumpVx", 1f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.125f);
        throwFirst = EnemyParams.Int(cfg, id, "firstThrowTicks", 48);
        throwMin = EnemyParams.Int(cfg, id, "throwMinTicks", 80);
        throwSpan = EnemyParams.Int(cfg, id, "throwSpanTicks", 64);
        burstMin = Mathf.Max(1, EnemyParams.Int(cfg, id, "burstMin", 2));
        burstSpan = EnemyParams.Int(cfg, id, "burstSpan", 1);
        burstGap = Mathf.Max(1, EnemyParams.Int(cfg, id, "burstGapTicks", 10));
        boneVx = EnemyParams.Fixed256(cfg, id, "boneVx", 1f);
        boneVxStep = EnemyParams.Fixed256(cfg, id, "boneVxStep", 0.375f);
        boneVy0 = EnemyParams.Fixed256(cfg, id, "boneVy0", -3.5f);
        boneVyStep = EnemyParams.Fixed256(cfg, id, "boneVyStep", 0.5f);
        boneG = EnemyParams.Fixed256(cfg, id, "boneGravity", 0.125f);
        boneHand = EnemyParams.Int(cfg, id, "boneHandAbovePx", 24);
        bones = bursts = jumps = 0;
        checkOffscreen = false;   // đặt sẵn — SpawnPoint lo gỡ (lùi/nhảy giữ khoảng cách có thể ra sát mép)
        burstLeft = 0;
        throwTimer = throwFirst;
        decideTimer = 1;
        vy = 0;
        Facing = FacingToPlayer();
        moveDir = Facing;
        Fsm.SetState(Grounded() ? eSkeletonState.Walk : eSkeletonState.Jump);
    }

    protected override void TickStates()
    {
        movedX = false;
        Fsm.Tick();
        Facing = FacingToPlayer();
        TickThrow();
    }
}
