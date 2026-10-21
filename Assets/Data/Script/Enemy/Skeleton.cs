using UnityEngine;

/// <summary>State Skeleton (White Skeleton).</summary>
public enum eSkeletonState
{
    Act,    // trên đất: chuỗi lượt (nghỉ → đi tới / đi lùi / ném xương)
    Jump,   // trên không: nhảy ở mép (cả rơi khi mất sàn — vx 0, vy từ 0)
}

/// <summary>
/// Skeleton — "White Skeleton" trên sheet. CVGP 17 dựng theo wiki; CVM3 07 đo lại theo video Map_3 (6 con, 7 xương, 6 cú nhảy — CVM3_MEMORY
/// §2g). Mô hình video: chuỗi LƯỢT, mỗi lượt chọn 1 việc — throwChancePct % ném 1 xương (đứng yên throwTicks 18 tick, xương sinh ở tick
/// throwAtTick 4), còn lại đi moveSpeed legTicks tick (1.4375 × 16 = 23 px/chặng) VỀ PHÍA Simon nếu |dx| ≥ keepPx, LÙI nếu gần hơn, rồi
/// đứng pauseTicks (4) — lượt đi 20 tick; mới vào (sinh / đáp) đứng pauseTicks rồi mới chọn. Luôn quay mặt về Simon. Nhảy CHỈ khi sắp bước khỏi mép sàn
/// (6/6 cú ở video) hoặc đụng tường (suy luận CVGP 17): vx jumpVx theo chiều đang đi, vy0 jumpVy0, gravity (đỉnh 23.75 px, 40 tick trên
/// sàn phẳng) — trên không đi ngang KHÔNG chặn tường, rơi xuống mà chân lọt vào ô đặc (mép bục cao) ⇒ đặt lên mặt ô (video WEA 0:34.4).
/// Gờ cao hơn tầm nhảy / khe rộng hơn ⇒ rơi hố (wiki St7, video WEB 1:13.2). Xương (SkeletonBone): sinh ở (X, Y − boneHandAbovePx), vx
/// boneVx về hướng mặt, vy0 = boneVy0 − k·boneVyStep với k ngẫu nhiên 0..boneKindSpan (video: 2 độ cao), boneGravity. Đặt bằng SpawnPoint.
/// </summary>
public class Skeleton : EnemyBase
{
    [SerializeField] private SkeletonBone bonePrefab;

    /// <summary>Việc của lượt (harness ép theo video: forcedPlan).</summary>
    public const int PLAN_AUTO = 0, PLAN_TOWARD = 1, PLAN_AWAY = 2, PLAN_THROW = 3, PLAN_THROW_LOW = 4, PLAN_THROW_HIGH = 5, PLAN_STAND = 6;

    public StateMachine<Skeleton, eSkeletonState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, legTicks, pauseTicks, throwTicks, keepPx, throwChance, throwAt, jumpVy0, jumpVx, gravity;
    [System.NonSerialized] public int boneVx, boneVy0, boneVyStep, boneKindSpan, boneG, boneHand;
    [System.NonSerialized] public int moveDir, actTick, actLen, plan, jumpVxCur;
    /// <summary>Số xương / số lượt ném / số cú nhảy / số lượt (harness).</summary>
    [System.NonSerialized] public int bones, throws, jumps, units;
    /// <summary>Harness: việc từng lượt theo thứ tự (PLAN_*; hết mảng ⇒ tự chọn bằng RNG).</summary>
    [System.NonSerialized] public int[] forcedPlan;
    [System.NonSerialized] public int planIndex;
    private AnimSet.Anim aWalk;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState + (Fsm.Is(eSkeletonState.Act) ? PlanName() : "") : "-";

    private string PlanName()
    {
        switch (plan)
        {
            case PLAN_TOWARD: case PLAN_AWAY: return actTick <= legTicks ? (moveDir > 0 ? "R" : "L") : "";   // actTick đã +1 sau tick đi
            case PLAN_THROW: case PLAN_THROW_LOW: case PLAN_THROW_HIGH: return "T";
            case PLAN_STAND: return "S";
            default: return "";
        }
    }

    abstract class SkelState : BaseState<Skeleton>
    {
        protected void SetState(eSkeletonState s) { owner.Fsm.SetState(s); }
    }

    class ActState : SkelState
    {
        public override void OnEnterState()
        {
            owner.plan = PLAN_AUTO;            // mới vào (sinh / đáp): đứng pauseTicks rồi mới chọn việc
            owner.actTick = 0;
            owner.actLen = owner.pauseTicks;
        }

        public override void OnExecuteState()
        {
            if (owner.actTick >= owner.actLen)
                owner.Decide();
            int t = owner.actTick++;
            if (owner.plan == PLAN_TOWARD || owner.plan == PLAN_AWAY)
            {
                if (t < owner.legTicks)
                    owner.WalkStep();
            }
            else if (owner.plan >= PLAN_THROW && owner.plan <= PLAN_THROW_HIGH && t == owner.throwAt)
                owner.ThrowBone();
        }

        public override void CheckNextState()
        {
            if (!owner.Fsm.Is(eSkeletonState.Jump) && !owner.Grounded())
                owner.StartJump(0, 0);
        }
    }

    class JumpState : SkelState
    {
        public override void OnExecuteState()
        {
            owner.fx += owner.jumpVxCur;   // trên không: không chặn tường (video WEA 0:34.4 — lọt vào mép bục rồi đứng lên mặt bục)
            owner.vy += owner.gravity;
            if (owner.StepFallSnap(owner.vy))
            {
                owner.vy = 0;
                SetState(eSkeletonState.Act);
            }
        }
    }

    /// <summary>Đi 1 tick theo moveDir; bước tới mà hết sàn (mép) hoặc đụng tường ⇒ nhảy theo chiều đi.</summary>
    private void WalkStep()
    {
        int nx = Fixed.ToPx(fx + moveDir * walkVx);
        if (!GroundedAt(nx) || StepX(moveDir * walkVx))
            StartJump(jumpVy0, moveDir * jumpVx);
    }

    private void StartJump(int vy0, int vxj)
    {
        vy = vy0;
        jumpVxCur = vxj;
        if (vy0 != 0)
            jumps++;
        Fsm.SetState(eSkeletonState.Jump);
    }

    private void Decide()
    {
        units++;
        int p = forcedPlan != null && planIndex < forcedPlan.Length ? forcedPlan[planIndex++] : PLAN_AUTO;
        var pl = World.Player;
        int toward = pl != null && pl.X != X ? (pl.X > X ? 1 : -1) : Facing;
        int d = pl != null ? Mathf.Abs(pl.X - X) : 9999;
        if (p == PLAN_AUTO)
        {
            if (Roll(0, 99) < throwChance)
                p = PLAN_THROW;
            else
                p = d < keepPx ? PLAN_AWAY : PLAN_TOWARD;
        }
        plan = p;
        actTick = 0;
        bool thr = p >= PLAN_THROW && p <= PLAN_THROW_HIGH;
        if (p == PLAN_TOWARD) moveDir = toward;
        else if (p == PLAN_AWAY) moveDir = -toward;
        else if (thr) throws++;
        actLen = thr ? throwTicks : legTicks + pauseTicks;   // video: đi 16 + đứng 4; ném đứng 18 (1 xương ⇒ đứng 22, 2 xương ⇒ 40)
    }

    private void ThrowBone()
    {
        bones++;
        int k = plan == PLAN_THROW_LOW ? 0 : plan == PLAN_THROW_HIGH ? 1 : Roll(0, boneKindSpan);
        if (bonePrefab == null)
            return;
        var b = World.Spawn(bonePrefab, X, Y - boneHand, Facing) as SkeletonBone;
        if (b != null)
        {
            b.vx = Facing * boneVx;
            b.vy = boneVy0 - k * boneVyStep;
            b.gravity = boneG;
        }
    }

    /// <summary>Có sàn ở cột x (mặt trên ô đặc ở hàng Y).</summary>
    private bool GroundedAt(int x)
    {
        return World.Tiles != null && World.Tiles.RowLandable(Y, x, x);
    }

    /// <summary>
    /// Như StepFall (đi lên xuyên mọi thứ, đi xuống đáp ở mặt ô đặc đầu tiên tại cột X) + video WEA 0:34.4: đang rơi mà hàng chân (Y − 1)
    /// lọt trong ô đặc có ô trên trống (vào từ cạnh bên) ⇒ đặt lên mặt ô. Trả true nếu đáp.
    /// </summary>
    private bool StepFallSnap(int dy)
    {
        if (StepFall(dy))
            return true;
        var tc = World.Tiles;
        if (dy <= 0 || tc == null || !tc.IsSolid(X, Y - 1))
            return false;
        int top = TileCollision.FloorDiv(Y - 1, TileCollision.CELL) * TileCollision.CELL;
        if (tc.IsSolid(X, top - 1))
            return false;
        fy = Fixed.FromPx(top);
        return true;
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Skeleton, eSkeletonState>(this, "Skeleton");
        Fsm.Add(eSkeletonState.Act, new ActState());
        Fsm.Add(eSkeletonState.Jump, new JumpState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 1.4375f);
        legTicks = Mathf.Max(1, EnemyParams.Int(cfg, id, "legTicks", 16));
        pauseTicks = Mathf.Max(0, EnemyParams.Int(cfg, id, "pauseTicks", 4));
        throwTicks = Mathf.Max(1, EnemyParams.Int(cfg, id, "throwTicks", 18));
        keepPx = EnemyParams.Int(cfg, id, "keepPx", 72);
        throwChance = EnemyParams.Int(cfg, id, "throwChancePct", 25);
        throwAt = EnemyParams.Int(cfg, id, "throwAtTick", 4);
        jumpVy0 = EnemyParams.Fixed256(cfg, id, "jumpVy0", -2.5f);
        jumpVx = EnemyParams.Fixed256(cfg, id, "jumpVx", 1.25f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.125f);
        boneVx = EnemyParams.Fixed256(cfg, id, "boneVx", 0.75f);
        boneVy0 = EnemyParams.Fixed256(cfg, id, "boneVy0", -3.5f);
        boneVyStep = EnemyParams.Fixed256(cfg, id, "boneVyStep", 1f);
        boneKindSpan = EnemyParams.Int(cfg, id, "boneKindSpan", 1);
        boneG = EnemyParams.Fixed256(cfg, id, "boneGravity", 0.125f);
        boneHand = EnemyParams.Int(cfg, id, "boneHandAbovePx", 13);
        bones = throws = jumps = units = 0;
        forcedPlan = null;
        planIndex = 0;
        checkOffscreen = false;   // đặt sẵn — SpawnPoint lo gỡ (lùi/nhảy giữ khoảng cách có thể ra sát mép)
        vy = 0;
        jumpVxCur = 0;
        Facing = FacingToPlayer();
        moveDir = Facing;
        if (Anim != null && aWalk != null)
            Anim.Play(aWalk, true);   // khung đi chạy liên tục (cả đứng, ném, nhảy — video 31 lần đổi khung TB 17 tick)
        Fsm.SetState(Grounded() ? eSkeletonState.Act : eSkeletonState.Jump);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
        Facing = FacingToPlayer();
    }
}
