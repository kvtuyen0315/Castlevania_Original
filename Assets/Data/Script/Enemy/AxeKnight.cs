using UnityEngine;

/// <summary>State AxeKnight (Axe-Man).</summary>
public enum eAxeKnightState
{
    Walk,    // giữ khoảng cách (tiến / lùi / đứng)
    Throw,   // tư thế ném
    Fall,
}

/// <summary>
/// AxeKnight — "Axe-Man" trên sheet (CVGP 18, theo wiki — GẦN ĐÚNG, không có video Block 5). Wiki Enemies#Axe Knights + Stages 13-15:
/// ném rìu CAO hoặc THẤP (phải quất đúng độ cao), rìu bay đi rồi quay về như Boomerang; hay LÙI khi Simon tiến, "không bao giờ cố tới
/// quá gần"; bị đẩy khỏi mép màn thì không quay lại; 10 đòn yếu / 9 đòn mạnh, 1400 đ.
/// Ước lượng (Enemy.AxeKnight): luôn quay mặt về Simon; mỗi decideTicks chọn: xa hơn keepMaxPx ⇒ tiến, gần hơn keepMinPx ⇒ lùi, trong
/// dải ⇒ đứng; đi ConfigEnemy.moveSpeed; hết sàn cách edgeProbePx / đụng tường ⇒ đứng (không tự rơi); ném: firstThrow rồi mỗi
/// throwMin..+span tick (chỉ khi rìu trước đã hết và Knight đã lên màn) — tư thế ném throwPoseTicks tick (khung axeman_1), rìu sinh ở tick đầu, cao/thấp
/// ngẫu nhiên 50 % (tâm rìu = chân − axeHigh/LowAbovePx). Đã lên màn ⇒ ra khỏi màn thì gỡ (không quay lại). Mỗi đòn chưa hạ: tia lửa
/// + bất tử hitInvincibleTicks. Đặt bằng SpawnPoint.
/// </summary>
public class AxeKnight : EnemyBase
{
    [SerializeField] private AxeKnightAxe axePrefab;

    public StateMachine<AxeKnight, eAxeKnightState> Fsm { get; private set; }
    [System.NonSerialized] public int walkVx, keepMin, keepMax, decide, edgeProbe, gravity;
    [System.NonSerialized] public int throwFirst, throwMin, throwSpan, throwPose, axeSpeed, axeAccel, axeHigh, axeLow, axeFront;
    [System.NonSerialized] public int moveDir, decideTimer, throwTimer;
    [System.NonSerialized] public AxeKnightAxe lastAxe;
    private int lastAxeSpawnTick;
    private bool seenOnScreen;
    /// <summary>Số rìu cao / thấp đã ném (harness).</summary>
    [System.NonSerialized] public int highThrows, lowThrows;
    private AnimSet.Anim aWalk;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class KnightState : BaseState<AxeKnight>
    {
        protected void SetState(eAxeKnightState s) { owner.Fsm.SetState(s); }
    }

    class WalkState : KnightState
    {
        public override void OnEnterState() { owner.UpdatePose(); }

        public override void OnExecuteState()
        {
            owner.Facing = owner.FacingToPlayer();
            if (--owner.decideTimer <= 0)
            {
                owner.decideTimer = owner.decide;
                owner.ChooseDir();
            }
            if (owner.moveDir != 0)
            {
                int x = owner.X + owner.moveDir * owner.edgeProbe;
                bool floor = owner.World.Tiles == null || owner.World.Tiles.RowLandable(owner.Y, x, x);
                if (!floor || owner.StepX(owner.moveDir * owner.walkVx))
                    owner.moveDir = 0;   // mép / tường ⇒ đứng tới lần chọn sau
            }
            owner.UpdatePose();
        }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
            {
                SetState(eAxeKnightState.Fall);
                return;
            }
            if (--owner.throwTimer <= 0 && owner.seenOnScreen && !owner.AxeInFlight())   // chỉ ném khi đã lên màn
                SetState(eAxeKnightState.Throw);
        }
    }

    class ThrowState : KnightState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
            owner.Pose(1);
            owner.ThrowAxe();
        }

        public override void CheckNextState()
        {
            if (StateFrame > owner.throwPose)   // đúng throwPose tick tư thế ném
            {
                owner.throwTimer = owner.Roll(owner.throwMin, owner.throwSpan);
                SetState(eAxeKnightState.Walk);
            }
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
                SetState(eAxeKnightState.Walk);
            }
        }
    }

    private void ChooseDir()
    {
        var p = World.Player;
        if (p == null)
        {
            moveDir = 0;
            return;
        }
        int d = Mathf.Abs(p.X - X), toward = p.X >= X ? 1 : -1;
        moveDir = d > keepMax ? toward : d < keepMin ? -toward : 0;
    }

    private bool AxeInFlight()
    {
        return lastAxe != null && lastAxe.Alive && lastAxe.SpawnTick == lastAxeSpawnTick;
    }

    private void ThrowAxe()
    {
        bool high = Roll(0, 1) == 0;
        if (high) highThrows++; else lowThrows++;
        if (axePrefab == null)
            return;
        var a = World.Spawn(axePrefab, X + Facing * axeFront, Y - (high ? axeHigh : axeLow), Facing) as AxeKnightAxe;
        if (a != null)
        {
            a.Launch(this, Facing, axeSpeed, axeAccel);
            lastAxe = a;
            lastAxeSpawnTick = a.SpawnTick;
        }
    }

    private void UpdatePose()
    {
        if (Anim == null || aWalk == null)
            return;
        if (moveDir != 0)
        {
            if (!Anim.IsPlaying(aWalk))
                Anim.Play(aWalk);
        }
        else
            Pose(0);
    }

    private void Pose(int frame)
    {
        if (Anim != null && aWalk != null)
            Anim.SetFrame(aWalk, frame);
    }

    public override void OnTick()
    {
        // "đẩy khỏi mép màn thì không quay lại": chỉ gỡ ở mép sau khi đã lên màn (SpawnPoint sinh ở lề ngoài màn)
        if (!seenOnScreen && Alive && !IsOutsideScreen())
        {
            seenOnScreen = true;
            checkOffscreen = true;
        }
        base.OnTick();
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<AxeKnight, eAxeKnightState>(this, "AxeKnight");
        Fsm.Add(eAxeKnightState.Walk, new WalkState());
        Fsm.Add(eAxeKnightState.Throw, new ThrowState());
        Fsm.Add(eAxeKnightState.Fall, new FallState());
        if (animSet != null)
            aWalk = animSet.Get("walk");
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        walkVx = EnemyParams.MoveSpeed(cfg, id, 0.5f);
        keepMin = EnemyParams.Int(cfg, id, "keepMinPx", 64);
        keepMax = EnemyParams.Int(cfg, id, "keepMaxPx", 112);
        decide = Mathf.Max(1, EnemyParams.Int(cfg, id, "decideTicks", 32));
        edgeProbe = EnemyParams.Int(cfg, id, "edgeProbePx", 8);
        gravity = EnemyParams.Fixed256(cfg, id, "fallGravity", 0.125f);
        throwFirst = EnemyParams.Int(cfg, id, "firstThrowTicks", 48);
        throwMin = EnemyParams.Int(cfg, id, "throwMinTicks", 96);
        throwSpan = EnemyParams.Int(cfg, id, "throwSpanTicks", 64);
        throwPose = Mathf.Max(1, EnemyParams.Int(cfg, id, "throwPoseTicks", 16));
        axeSpeed = EnemyParams.Fixed256(cfg, id, "axeSpeed", 2f);
        axeAccel = EnemyParams.Fixed256(cfg, id, "axeTurnAccel", 0.0234375f);
        axeHigh = EnemyParams.Int(cfg, id, "axeHighAbovePx", 20);
        axeLow = EnemyParams.Int(cfg, id, "axeLowAbovePx", 10);
        axeFront = EnemyParams.Int(cfg, id, "axeFrontPx", 12);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        multiHit = true;
        checkOffscreen = false;
        seenOnScreen = false;
        highThrows = lowThrows = 0;
        lastAxe = null;
        moveDir = 0;
        decideTimer = 1;
        throwTimer = throwFirst;
        vy = 0;
        Facing = FacingToPlayer();
        Fsm.SetState(Grounded() ? eAxeKnightState.Walk : eAxeKnightState.Fall);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
