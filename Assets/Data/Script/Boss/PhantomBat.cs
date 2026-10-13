using UnityEngine;

/// <summary>State Phantom Bat (boss Block 1).</summary>
public enum ePhantomBatState
{
    Sleep,
    Wake,
    Swoop,
    Sweep,
    Rise,
    Glide,
    Hover,
}

/// <summary>
/// Phantom Bat — "Giant Bat" (CVGP 14). Đo 2 lần đánh (video Boss_Map_1_Behaviour + Map_1 3:17–3:33; Tools/cvgp/boss_fit.py khớp mẫu
/// 3 khung sprite — CVGP_MEMORY §2l). Pivot = tâm khung bay (viewOffset 0). Toạ độ px section; mép màn = camera X.
///   Sleep  treo trần tại (sleepX, sleepY) — khung ngủ 16×16 vẽ cao hơn pivot sleepDrawUpPx (đỉnh khung ngủ = đỉnh khung bay);
///          Simon.X ≥ wakeSimonX ⇒ thức (Activate).
///   Wake   vỗ cánh tại chỗ wakeHoverTicks tick (2 video 31/35) ⇒ nhạc boss ⇒ Swoop.
///   Swoop  bổ nhào: ngang vx = (Simon.X − x0)/T (|vx| kẹp [swoopVxMin, swoopVxMax], hướng về Simon); dọc y = y0 + d(2s − s²), s = t/T,
///          d = min(swoopDepthPx, swoopMaxY − y0), T = 2d/swoopVy0 ⇒ vy bắt đầu swoopVy0 giảm đều về 0 ở đáy (khớp 5 cú lao rms y ≤ 1.1 px).
///          Hết T: đáy cao hơn riseTopY (cú lao đầu từ trần — 2 video) ⇒ Glide ngay; không thì Sweep.
///   Sweep  bay ngang tiếp vx sweepTicks tick ⇒ Rise.
///   Rise   vx giữ, vy giảm riseAccel mỗi tick (vọt lên nhanh dần) tới y ≤ riseTopY ⇒ Glide. Chạm mép màn (screenMinX/MaxX) ⇒ đổi
///          chiều ngang (video boss: quay đầu ở x màn 237–238).
///   Glide  bay thẳng tới điểm (x ngẫu nhiên trong [glideMinScreenX, glideMaxScreenX] cách Simon ≥ glideMinSimonDx; y = glideLowY sau
///          cú lao đầu / glideHighY sau khi vọt lên — 5/5 lần video), trục trội glideSpeed px/tick.
///   Hover  lượn tại chỗ hoverMin..hoverMax tick (ngẫu nhiên) ⇒ Swoop (video 4/4 lần; diveChancePct cho phép đổi).
/// Khựng khi trúng đòn / chạm nước thánh: BossBase (khung hurt = phantombat_2 — video 13 frame khựng đều là khung này).
/// Không bắn đạn (cả 2 video). Ngẫu nhiên = StageWorld.Rng (seed đầu stage — H7).
/// </summary>
public class PhantomBat : BossBase
{
    public StateMachine<PhantomBat, ePhantomBatState> Fsm { get; private set; }

    private AnimSet.Anim aSleep, aFly;
    // tham số (fixed 1/256 px trừ tick/px nguyên)
    private int wakeX, wakeHover, sleepUp, vy0, depth, vxMin, vxMax, sweepTicks, riseAccel, scrMinX, scrMaxX;
    private int glideSpeed, glideMinSX, glideMaxSX, glideMinDx, hoverMin, hoverMax, diveChance;
    /// <summary>Độ cao tuyệt đối (px section) — CVGP 18: bản thường đặt lại theo chân Simon lúc thức.</summary>
    protected int maxY, riseTopY, glideLowY, glideHighY;
    // trạng thái chuyển động
    [System.NonSerialized] public int t, T, x0f, y0f, dxf, dyf, glideLow, hoverLen;
    /// <summary>Số cú lao / lượn đã bắt đầu (harness).</summary>
    [System.NonSerialized] public int swoops, glides;
    /// <summary>Đích lượn hiện tại / đáy cú lao gần nhất (px section) — harness.</summary>
    [System.NonSerialized] public Vector2Int glideTarget, swoopEnd;

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) + " t" + t : "-";

    abstract class BatState : BaseState<PhantomBat>
    {
        protected void SetState(ePhantomBatState s) { owner.Fsm.SetState(s); }
    }

    class SleepState : BatState
    {
        public override void OnEnterState() { owner.PlayAnim(owner.aSleep); }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && p.Alive && owner.ShouldWake(p))
            {
                owner.Activate();
                SetState(ePhantomBatState.Wake);
            }
        }
    }

    class WakeState : BatState
    {
        public override void OnEnterState() { owner.t = 0; owner.PlayAnim(owner.aFly); }
        public override void OnExecuteState() { owner.t++; }

        public override void CheckNextState()
        {
            // đứng yên đúng wakeHover tick sau tick thức; tick kế tiếp đã lao
            if (owner.t <= owner.wakeHover)
                return;
            owner.StartBossMusic();
            SetState(ePhantomBatState.Swoop);
        }
    }

    class SwoopState : BatState
    {
        public override void OnEnterState() { owner.BeginSwoop(); }

        public override void OnExecuteState()
        {
            owner.t++;
            long tt = owner.t, TT = owner.T;
            owner.fy = owner.y0f + (int)(owner.dyf * (2 * tt * TT - tt * tt) / (TT * TT));
            owner.StepVx();
        }

        public override void CheckNextState()
        {
            if (owner.t < owner.T)
                return;
            owner.swoopEnd = new Vector2Int(owner.X, owner.Y);
            SetState(owner.Y <= owner.riseTopY ? ePhantomBatState.Glide : ePhantomBatState.Sweep);
        }
    }

    class SweepState : BatState
    {
        public override void OnEnterState() { owner.t = 0; }
        public override void OnExecuteState() { owner.t++; owner.StepVx(); }

        public override void CheckNextState()
        {
            if (owner.t >= owner.sweepTicks)
                SetState(ePhantomBatState.Rise);
        }
    }

    class RiseState : BatState
    {
        public override void OnEnterState() { owner.t = 0; owner.vy = 0; owner.glideLow = 0; }

        public override void OnExecuteState()
        {
            owner.t++;
            owner.vy -= owner.riseAccel;
            owner.fy += owner.vy;
            owner.StepVx();
        }

        public override void CheckNextState()
        {
            if (owner.Y <= owner.riseTopY)
                SetState(ePhantomBatState.Glide);
        }
    }

    class GlideState : BatState
    {
        public override void OnEnterState() { owner.BeginGlide(); }

        public override void OnExecuteState()
        {
            owner.t++;
            long tt = owner.t, TT = owner.T;
            owner.fx = owner.x0f + (int)(owner.dxf * tt / TT);
            owner.fy = owner.y0f + (int)(owner.dyf * tt / TT);
        }

        public override void CheckNextState()
        {
            if (owner.t >= owner.T)
                SetState(ePhantomBatState.Hover);
        }
    }

    class HoverState : BatState
    {
        public override void OnEnterState()
        {
            owner.t = 0;
            owner.hoverLen = owner.hoverMin + owner.World.Rng.Next(owner.hoverMax - owner.hoverMin + 1);
        }

        public override void OnExecuteState() { owner.t++; owner.OnHoverTick(owner.t); }

        public override void CheckNextState()
        {
            if (owner.t <= owner.hoverLen)
                return;
            bool dive = owner.diveChance >= 100 || owner.World.Rng.Next(100) < owner.diveChance;
            SetState(dive ? ePhantomBatState.Swoop : ePhantomBatState.Glide);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<PhantomBat, ePhantomBatState>(this, "PhantomBat");
        Fsm.Add(ePhantomBatState.Sleep, new SleepState());
        Fsm.Add(ePhantomBatState.Wake, new WakeState());
        Fsm.Add(ePhantomBatState.Swoop, new SwoopState());
        Fsm.Add(ePhantomBatState.Sweep, new SweepState());
        Fsm.Add(ePhantomBatState.Rise, new RiseState());
        Fsm.Add(ePhantomBatState.Glide, new GlideState());
        Fsm.Add(ePhantomBatState.Hover, new HoverState());
        if (animSet != null)
        {
            aSleep = animSet.Get("sleep");
            aFly = animSet.Get("fly");
        }
    }

    /// <summary>Điều kiện thức (boss: Simon.X ≥ wakeSimonX — CVGP 18 bản thường override theo khoảng cách).</summary>
    protected virtual bool ShouldWake(Entity p) { return p.X >= wakeX; }
    /// <summary>Mỗi tick lơ lửng (CVGP 18: bản thường phun cầu lửa ở đây; boss không bắn — 2 video).</summary>
    protected virtual void OnHoverTick(int tick) { }

    protected override void LoadBoss(GameConfig cfg)
    {
        wakeX = PInt(cfg, "wakeSimonX", 693);
        wakeHover = PInt(cfg, "wakeHoverTicks", 33);
        sleepUp = PInt(cfg, "sleepDrawUpPx", 8);
        vy0 = Mathf.Max(1, PFixed(cfg, "swoopVy0", 1.46f));
        depth = PInt(cfg, "swoopDepthPx", 41);
        maxY = PInt(cfg, "swoopMaxY", 156);
        vxMin = PFixed(cfg, "swoopVxMin", 1.22f);
        vxMax = PFixed(cfg, "swoopVxMax", 2.1f);
        sweepTicks = PInt(cfg, "sweepTicks", 12);
        riseAccel = PFixed(cfg, "riseAccel", 0.039f);
        riseTopY = PInt(cfg, "riseTopY", 88);
        scrMinX = PInt(cfg, "screenMinX", 18);
        scrMaxX = PInt(cfg, "screenMaxX", 238);
        glideSpeed = Mathf.Max(1, PFixed(cfg, "glideSpeed", 0.75f));
        glideMinSX = PInt(cfg, "glideMinScreenX", 24);
        glideMaxSX = PInt(cfg, "glideMaxScreenX", 232);
        glideLowY = PInt(cfg, "glideLowY", 92);
        glideHighY = PInt(cfg, "glideHighY", 123);
        glideMinDx = PInt(cfg, "glideMinSimonDx", 64);
        hoverMin = PInt(cfg, "hoverMinTicks", 32);
        hoverMax = Mathf.Max(hoverMin, PInt(cfg, "hoverMaxTicks", 124));
        diveChance = PInt(cfg, "diveChancePct", 100);
        t = T = 0;
        vx = vy = 0;
        glideLow = 1;
        swoops = glides = 0;
        glideTarget = new Vector2Int(X, Y);
        Facing = -1;
        Fsm.SetState(ePhantomBatState.Sleep);
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    protected override void OnStunEnd()
    {
        PlayAnim(Fsm.Is(ePhantomBatState.Sleep) ? aSleep : aFly);
    }

    private void PlayAnim(AnimSet.Anim a)
    {
        if (Anim != null && a != null && !Anim.IsPlaying(a))
            Anim.Play(a, true);
    }

    private int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    /// <summary>Đi ngang vx; ra ngoài [cx + screenMinX, cx + screenMaxX] theo chiều đang đi ⇒ đổi chiều (video boss x màn 237–238).</summary>
    private void StepVx()
    {
        fx += vx;
        int sx = X - CamX;
        if ((vx > 0 && sx >= scrMaxX) || (vx < 0 && sx <= scrMinX))
            vx = -vx;
        if (vx != 0)
            Facing = vx > 0 ? 1 : -1;
    }

    private void BeginSwoop()
    {
        swoops++;
        t = 0;
        x0f = fx;
        y0f = fy;
        int d = Mathf.Max(1, Mathf.Min(depth, maxY - Y));
        dyf = Fixed.FromPx(d);
        T = Mathf.Max(1, (2 * dyf + vy0 / 2) / vy0);
        var p = World.Player;
        int dir = p == null || p.X == X ? Facing : (p.X > X ? 1 : -1);
        int want = p != null ? Mathf.Abs(Fixed.FromPx(p.X - X)) / T : vxMin;
        vx = dir * Mathf.Clamp(want, vxMin, vxMax);
        Facing = dir;
        PlayAnim(aFly);
    }

    private void BeginGlide()
    {
        glides++;
        t = 0;
        x0f = fx;
        y0f = fy;
        int cx = CamX;
        var p = World.Player;
        int px = p != null ? p.X : X;
        int lo = cx + glideMinSX, hi = cx + glideMaxSX, x = lo;
        // x ngẫu nhiên đều trong [lo, hi] cách Simon ≥ glideMinDx (video 5/5 lần ≥ 68): lấy mẫu trên phần hợp lệ (không lặp lại)
        int leftHi = Mathf.Min(hi, px - glideMinDx), rightLo = Mathf.Max(lo, px + glideMinDx);
        int nLeft = Mathf.Max(0, leftHi - lo + 1), nRight = Mathf.Max(0, hi - rightLo + 1);
        if (nLeft + nRight > 0)
        {
            int k = World.Rng.Next(nLeft + nRight);
            x = k < nLeft ? lo + k : rightLo + (k - nLeft);
        }
        else
            x = px - lo > hi - px ? lo : hi;
        int y = glideLow != 0 ? glideLowY : glideHighY;
        glideTarget = new Vector2Int(x, y);
        dxf = Fixed.FromPx(x) - fx;
        dyf = Fixed.FromPx(y) - fy;
        int n = Mathf.Max(Mathf.Abs(dxf), Mathf.Abs(dyf));
        T = Mathf.Max(1, (n + glideSpeed - 1) / glideSpeed);
        vx = 0;
        if (dxf != 0)
            Facing = dxf > 0 ? 1 : -1;
        PlayAnim(aFly);
    }

    /// <summary>Harness: kéo dài lơ lửng hiện tại / bay ngang (Sweep) đúng n tick.</summary>
    public void DebugHoverTicks(int n) { hoverLen = n; }
    public void DebugSweepTicks(int n) { sweepTicks = n; }

    public override void SyncView()
    {
        // khung ngủ 16×16: đỉnh trùng đỉnh khung bay ⇒ tâm cao hơn pivot sleepDrawUpPx (video f0–65 tâm (111,65) ↔ bay (111,73))
        int up = Fsm != null && Fsm.HasState && Fsm.Is(ePhantomBatState.Sleep) ? sleepUp : 0;
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y - up);
        if (Anim != null)
            Anim.Apply(-1);
    }
}
