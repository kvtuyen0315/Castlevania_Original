using UnityEngine;

/// <summary>State Igor (đi cùng Frankenstein's Monster).</summary>
public enum eIgorState
{
    Ride,    // ngồi trên vai Monster
    Ground,  // nghỉ giữa 2 cú nhảy
    Hop,     // trên không
}

/// <summary>
/// Igor (CVGP 19, theo wiki — GẦN ĐÚNG, không có video Block 4). Wiki Bosses#Frankenstein & Igor + Stages 10-12#Stage 12: "trông và cư xử y
/// như Fleaman nhưng BẤT TỬ và ném cầu lửa"; "nhảy hết sang một phía phòng rồi nhảy ngược lại"; "thường nhảy tiến khi ở giữa màn và nhảy
/// CAO khi tới các bục ở góc"; "nhảy khỏi vai Monster"; "quất Igor 1 lần sẽ đóng băng nó" (Axe làm nó choáng giữa không trung); Stopwatch vô
/// dụng (boss). ConfigEnemy.Igor không có máu/điểm ⇒ không bao giờ chết (trừ khi Monster chết ⇒ biến mất trong lửa, 0 điểm). Ước lượng
/// (ConfigActorParam Enemy.Igor — ghi §2p):
///   Ride    vị trí = Monster (X, Y − rideAbovePx) mỗi tick; Monster thức + startDelayTicks ⇒ nhảy về phía mép bên Simon.
///   Ground  nghỉ pauseMin..+span tick ⇒ cú nhảy: đích = mép màn [edgeMinScreenX | edgeMaxScreenX] của phía đang đi; cách đích ≤ edgeReachPx
///           ⇒ đổi phía; ≤ cornerZonePx ⇒ nhảy CAO (longVx/longVy0 — lên được bục góc 32 px), còn lại nhảy TIẾN (shortVx/shortVy0); gravity.
///   Hop     như Fleaman (đụng tường mất đà ngang); đỉnh cú nhảy (vy đổi dấu) của mỗi fireEveryHops cú ⇒ 1 cầu lửa nhắm tâm thân Simon (chân −
///           aimAbovePx) fireballSpeed (lớp BoneTowerFireball — quất vỡ được +100, SUY LUẬN như cầu lửa 17/18).
/// Trúng đòn (mọi vũ khí): KHÔNG mất máu, đứng hình stunTicks tick (giữa không trung cũng đứng — wiki Axe), tia lửa; bất tử hitInvincibleTicks.
/// </summary>
public class Igor : EnemyBase
{
    [SerializeField] private BoneTowerFireball fireballPrefab;

    public StateMachine<Igor, eIgorState> Fsm { get; private set; }
    private AnimSet.Anim aHop;
    [System.NonSerialized] public Frankenstein monster;
    [System.NonSerialized] public int rideAbove, startDelay, edgeMin, edgeMax, edgeReach, cornerZone, shortVx, shortVy0, longVx, longVy0, gravity;
    [System.NonSerialized] public int pauseMin, pauseSpan, fireEvery, fireSpeed, aimAbove, stunLen;
    [System.NonSerialized] public int side, pauseLen, hopVx, hopIndex, stunLeft, awakeAge;
    /// <summary>Đếm cú nhảy tiến / cao, cầu lửa, số lần bị đóng băng, đổi phía (harness).</summary>
    [System.NonSerialized] public int shortHops, longHops, shots, stuns, sideFlips;
    [System.NonSerialized] public bool lastHopLong;
    private bool fireThisHop;

    public override bool IsBoss => true;
    public override string DebugState => Fsm != null && Fsm.HasState ? (stunLeft > 0 ? "Stun" : Fsm.CurState + (Fsm.Is(eIgorState.Hop) ? (lastHopLong ? "L" : "S") : "")) : "-";

    abstract class IgorState : BaseState<Igor>
    {
        protected void SetState(eIgorState s) { owner.Fsm.SetState(s); }
    }

    class RideState : IgorState
    {
        public override void OnEnterState() { owner.Pose(0); }

        public override void OnExecuteState() { owner.FollowMonster(); }

        public override void CheckNextState()
        {
            if (owner.awakeAge < 0 || owner.ageTicks - owner.awakeAge < owner.startDelay)
                return;
            var p = owner.World.Player;
            owner.side = p != null && p.X < owner.X ? -1 : 1;
            owner.pauseLen = 0;
            owner.StartHop();
        }
    }

    class GroundState : IgorState
    {
        public override void OnEnterState() { owner.Pose(0); }

        public override void CheckNextState()
        {
            if (!owner.Grounded())
            {
                owner.hopVx = 0;
                owner.vy = 0;
                owner.fireThisHop = false;
                SetState(eIgorState.Hop);
            }
            else if (StateFrame > owner.pauseLen)
                owner.StartHop();
        }
    }

    class HopState : IgorState
    {
        public override void OnEnterState() { owner.Pose(1); }

        public override void OnExecuteState()
        {
            if (owner.hopVx != 0 && owner.StepX(owner.Facing * owner.hopVx))
                owner.hopVx = 0;
            int before = owner.vy;
            owner.vy += owner.gravity;
            if (owner.fireThisHop && before < 0 && owner.vy >= 0)
            {
                owner.fireThisHop = false;
                owner.Fire();
            }
            if (owner.StepFall(owner.vy))
            {
                owner.vy = 0;
                owner.pauseLen = owner.Roll(owner.pauseMin, owner.pauseSpan);
                SetState(eIgorState.Ground);
            }
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Igor, eIgorState>(this, "Igor");
        Fsm.Add(eIgorState.Ride, new RideState());
        Fsm.Add(eIgorState.Ground, new GroundState());
        Fsm.Add(eIgorState.Hop, new HopState());
        if (animSet != null)
            aHop = animSet.Get("hop");
    }

    /// <summary>Bất tử: ConfigEnemy.Igor không có máu/điểm (wiki) ⇒ máu tượng trưng, không bao giờ trừ.</summary>
    protected override void ResetHealth(GameConfig cfg)
    {
        Health.Reset(1, 1);
        Score = 0;
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        rideAbove = EnemyParams.Int(cfg, id, "rideAbovePx", 40);
        startDelay = EnemyParams.Int(cfg, id, "startDelayTicks", 96);
        edgeMin = EnemyParams.Int(cfg, id, "edgeMinScreenX", 24);
        edgeMax = EnemyParams.Int(cfg, id, "edgeMaxScreenX", 232);
        edgeReach = EnemyParams.Int(cfg, id, "edgeReachPx", 24);
        cornerZone = EnemyParams.Int(cfg, id, "cornerZonePx", 48);
        shortVx = EnemyParams.Fixed256(cfg, id, "shortVx", 1.25f);
        shortVy0 = EnemyParams.Fixed256(cfg, id, "shortVy0", -3f);
        longVx = EnemyParams.Fixed256(cfg, id, "longVx", 1.5f);
        longVy0 = EnemyParams.Fixed256(cfg, id, "longVy0", -5f);
        gravity = EnemyParams.Fixed256(cfg, id, "gravity", 0.25f);
        pauseMin = EnemyParams.Int(cfg, id, "pauseMinTicks", 8);
        pauseSpan = EnemyParams.Int(cfg, id, "pauseSpanTicks", 16);
        fireEvery = Mathf.Max(1, EnemyParams.Int(cfg, id, "fireEveryHops", 2));
        fireSpeed = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 1.5f);
        aimAbove = EnemyParams.Int(cfg, id, "aimAbovePx", 16);
        stunLen = EnemyParams.Int(cfg, id, "stunTicks", 120);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        checkOffscreen = false;
        monster = null;
        side = -1;
        pauseLen = hopVx = hopIndex = stunLeft = 0;
        awakeAge = -1;
        shortHops = longHops = shots = stuns = sideFlips = 0;
        lastHopLong = fireThisHop = false;
        vy = 0;
        Fsm.SetState(eIgorState.Ground);
    }

    /// <summary>Frankenstein sinh Igor ⇒ ngồi trên vai.</summary>
    public void RideOn(Frankenstein m)
    {
        monster = m;
        Fsm.SetState(eIgorState.Ride);
        FollowMonster();
    }

    /// <summary>Monster vừa thức ⇒ đếm startDelay rồi nhảy khỏi vai.</summary>
    public void MonsterAwake()
    {
        awakeAge = ageTicks;
    }

    private void FollowMonster()
    {
        if (monster == null)
            return;
        fx = monster.fx;
        fy = monster.fy - Fixed.FromPx(rideAbove);
        Facing = monster.Facing;
    }

    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;   // Stopwatch vô dụng với boss Block 4 (wiki)
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }

    public override void OnTick()
    {
        if (stunLeft > 0)
        {
            ageTicks++;
            stunLeft--;
            ApplyVisibility();
            return;   // đứng hình (cả giữa không trung): không di chuyển, không chạy anim
        }
        base.OnTick();
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (hidden || !Alive)
            return false;
        stunLeft = stunLen;
        stuns++;
        var cfg = World.Config;
        FxSprite.Spawn(World, "hit", X, CenterY, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] Igor trúng " + hit.strength + " ⇒ đứng hình " + stunLen + " tick (bất tử)");
        return true;
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    private void StartHop()
    {
        int edgeX = CamX + (side > 0 ? edgeMax : edgeMin);
        if (Mathf.Abs(edgeX - X) <= edgeReach)
        {
            side = -side;
            sideFlips++;
            edgeX = CamX + (side > 0 ? edgeMax : edgeMin);
        }
        int d = Mathf.Abs(edgeX - X);
        Facing = edgeX >= X ? 1 : -1;
        bool high = d <= cornerZone;
        lastHopLong = high;
        if (high) longHops++; else shortHops++;
        hopVx = high ? longVx : shortVx;
        vy = high ? longVy0 : shortVy0;
        hopIndex++;
        fireThisHop = hopIndex % fireEvery == 0;
        Fsm.SetState(eIgorState.Hop);
    }

    private void Fire()
    {
        if (fireballPrefab == null)
            return;
        shots++;
        var p = World.Player;
        long dx = p != null ? p.X - X : Facing * 64, dy = p != null ? (p.Y - aimAbove) - (Y - 4) : 0;
        long len = ISqrt((dx * dx + dy * dy) << 16);   // độ dài ×256 (căn nguyên — H7)
        if (len == 0)
        {
            dx = Facing;
            len = 256;
        }
        var f = World.Spawn(fireballPrefab, X, Y - 4, dx >= 0 ? 1 : -1) as BoneTowerFireball;   // quả cầu ở nửa trên ô 8×16 ⇒ tâm ≈ Y − 8
        if (f != null)
        {
            f.vx = (int)(fireSpeed * dx * 256 / len);
            f.vy = (int)(fireSpeed * dy * 256 / len);
        }
    }

    private static long ISqrt(long v)
    {
        if (v <= 0)
            return 0;
        long r = (long)System.Math.Sqrt(v);
        while (r * r > v) r--;
        while ((r + 1) * (r + 1) <= v) r++;
        return r;
    }

    private void Pose(int frame)
    {
        if (Anim != null && aHop != null)
            Anim.SetFrame(aHop, frame);
    }

    /// <summary>Monster bị hạ ⇒ Igor biến mất trong lửa (0 điểm).</summary>
    public void Vanish()
    {
        if (!Alive)
            return;
        defeated = true;
        FxSprite.Spawn(World, "death", X, CenterY);
        Kill();
    }
}
