using UnityEngine;

/// <summary>State Raven.</summary>
public enum eRavenState
{
    Perch,
    Glide,   // lướt chéo: vx đều, vy giảm dần tới 0
    Hover,
    Dash,    // lao ngang tăng tốc về phía Simon
    Brake,   // vượt qua Simon lần đầu ⇒ phanh gấp ("quay đầu rất nhanh")
}

/// <summary>
/// Raven (CVGP 17 theo wiki; CVM3 08 ĐO LẠI video Map_3 — 12 con WEB / WalkwayB / MummyMan, 9 lần cất cánh, 9 chặng lướt, 2 cú lao ngang
/// — CVM3_MEMORY §2h). Đậu (raven_0) đúng perchTicks sau khi hiện (video 58–62 tick — KHÔNG theo khoảng cách Simon: |dx| lúc cất cánh
/// 55–117) ⇒ LƯỚT: vx = ±moveSpeed về phía Simon, vy = ±glideVy0 (1 trong 3 mức đo được, Roll) về phía TÂM THÂN Simon (chân −
/// targetAboveFeetPx), mỗi tick dời rồi |vy| −= glideDecel tới 0 ⇒ TREO hoverTicks ⇒ nếu cao hơn tâm Simon ⇒ LAO NGANG (vx từ ±dashVx0
/// về phía Simon, mỗi tick |vx| += dashAccel, trần dashMaxVx; y giữ) — vượt qua X Simon lần đầu ⇒ PHANH (|vx| −= brakeDecel) ⇒ treo
/// turnHoverTicks ⇒ lao lại về Simon; lần vượt thứ dashTurns + 1 ⇒ bay tiếp ra khỏi màn (gỡ); nếu thấp hơn tâm Simon ⇒ LƯỚT lần nữa.
/// Hướng mặt luôn về Simon. Xuyên địa hình. Pivot = tâm sprite. Đặt bằng SpawnPoint (khung hiện vật [cx+8, cx+248) ⇒ lúc sinh = lúc hiện).
/// </summary>
public class Raven : EnemyBase
{
    public StateMachine<Raven, eRavenState> Fsm { get; private set; }
    [System.NonSerialized] public int perch, glideVx, glideDecel, targetAbove, hover, turnHover, dashVx0, dashAccel, dashMax, brakeDecel, dashTurns;
    [System.NonSerialized] public int[] glideVy0 = new int[0];
    [System.NonSerialized] public int hoverLen, dir, turnsLeft;
    [System.NonSerialized] public bool afterBrake;
    /// <summary>Số chặng lướt / cú lao ngang / tuổi lúc cất cánh (harness).</summary>
    [System.NonSerialized] public int glides, dashes, takeoffAge;
    /// <summary>Harness (CVM3 08 — so video): vy0 (fixed) từng chặng lướt thay Roll; hết mảng ⇒ phần tử cuối. null = Config.</summary>
    [System.NonSerialized] public int[] forcedVy0;
    private AnimSet.Anim aPerch, aFly;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class RavenState : BaseState<Raven>
    {
        protected void SetState(eRavenState s) { owner.Fsm.SetState(s); }

        /// <summary>Hướng mặt về Simon theo vị trí SAU khi dời (lớp con gọi cuối Execute) — video: lật mặt ngay khi vượt qua Simon (f1436, f1470).</summary>
        public override void OnExecuteState()
        {
            owner.Facing = owner.FacingToPlayer();
        }
    }

    class PerchState : RavenState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            if (owner.Anim != null && owner.aPerch != null)
                owner.Anim.Play(owner.aPerch, true);
        }

        public override void OnExecuteState()
        {
        }

        public override void CheckNextState()
        {
            if (owner.ageTicks >= owner.perch)
            {
                owner.takeoffAge = owner.ageTicks;
                SetState(eRavenState.Glide);
            }
        }
    }

    class GlideState : RavenState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = true;
            if (owner.Anim != null && owner.aFly != null && owner.Anim.Current != owner.aFly)
                owner.Anim.Play(owner.aFly, true);
            owner.StartGlide();
        }

        public override void OnExecuteState()
        {
            owner.fx += owner.vx;
            owner.fy += owner.vy;
            owner.vy = owner.vy > 0 ? Mathf.Max(0, owner.vy - owner.glideDecel) : Mathf.Min(0, owner.vy + owner.glideDecel);
            base.OnExecuteState();
        }

        public override void CheckNextState()
        {
            if (owner.vy == 0)
            {
                owner.hoverLen = owner.hover;
                owner.afterBrake = false;
                SetState(eRavenState.Hover);
            }
        }
    }

    class HoverState : RavenState
    {
        public override void OnEnterState()
        {
            owner.vx = owner.vy = 0;
        }

        public override void CheckNextState()
        {
            if (StateFrame <= owner.hoverLen)   // đứng yên đúng hoverLen tick
                return;
            var p = owner.World.Player;
            bool above = p == null || owner.Y < p.Y - owner.targetAbove;
            SetState(owner.afterBrake || above ? eRavenState.Dash : eRavenState.Glide);
        }
    }

    class DashState : RavenState
    {
        public override void OnEnterState()
        {
            owner.dashes++;
            owner.dir = owner.SideOfPlayer();
            owner.vx = owner.dir * owner.dashVx0;
            owner.vy = 0;
        }

        public override void OnExecuteState()
        {
            owner.vx = owner.dir * Mathf.Min(Mathf.Abs(owner.vx) + owner.dashAccel, owner.dashMax);
            owner.fx += owner.vx;
            base.OnExecuteState();
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (owner.turnsLeft > 0 && p != null && (p.X - owner.X) * owner.dir < 0)
            {
                owner.turnsLeft--;
                SetState(eRavenState.Brake);
            }
        }
    }

    class BrakeState : RavenState
    {
        public override void OnExecuteState()
        {
            int v = Mathf.Max(0, Mathf.Abs(owner.vx) - owner.brakeDecel);
            owner.vx = owner.vx > 0 ? v : -v;
            owner.fx += owner.vx;
            base.OnExecuteState();
        }

        public override void CheckNextState()
        {
            if (owner.vx == 0)
            {
                owner.hoverLen = owner.turnHover;
                owner.afterBrake = true;
                SetState(eRavenState.Hover);
            }
        }
    }

    /// <summary>Phía Simon theo X (−1 / +1); trùng X hoặc không có Player ⇒ hướng đang quay.</summary>
    private int SideOfPlayer()
    {
        var p = World.Player;
        if (p == null || p.X == X)
            return Facing != 0 ? Facing : -1;
        return p.X > X ? 1 : -1;
    }

    /// <summary>Chặng lướt: vx đều về phía Simon, vy0 (Roll 1 trong các mức đo / harness) về phía tâm thân Simon.</summary>
    private void StartGlide()
    {
        var p = World.Player;
        dir = SideOfPlayer();
        vx = dir * glideVx;
        int v0;
        if (forcedVy0 != null && forcedVy0.Length > 0)
            v0 = forcedVy0[Mathf.Min(glides, forcedVy0.Length - 1)];
        else
            v0 = glideVy0.Length > 0 ? glideVy0[glideVy0.Length > 1 ? World.Rng.Next(glideVy0.Length) : 0] : Fixed.ONE;
        int dy = p != null ? (p.Y - targetAbove) - Y : 0;
        vy = dy > 0 ? v0 : -v0;   // trùng tâm ⇒ lên (suy luận — video không có mẫu)
        glides++;
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Raven, eRavenState>(this, "Raven");
        Fsm.Add(eRavenState.Perch, new PerchState());
        Fsm.Add(eRavenState.Glide, new GlideState());
        Fsm.Add(eRavenState.Hover, new HoverState());
        Fsm.Add(eRavenState.Dash, new DashState());
        Fsm.Add(eRavenState.Brake, new BrakeState());
        if (animSet != null)
        {
            aPerch = animSet.Get("perch");
            aFly = animSet.Get("fly");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        glideVx = EnemyParams.MoveSpeed(cfg, id, 0.75f);
        perch = EnemyParams.Int(cfg, id, "perchTicks", 60);
        glideVy0 = new[]
        {
            EnemyParams.Fixed256(cfg, id, "glideVy0Low", 1.25f),
            EnemyParams.Fixed256(cfg, id, "glideVy0Mid", 1.75f),
            EnemyParams.Fixed256(cfg, id, "glideVy0High", 2f),
        };
        glideDecel = EnemyParams.Fixed256(cfg, id, "glideDecel", 0.03515625f);
        targetAbove = EnemyParams.Int(cfg, id, "targetAboveFeetPx", 16);
        hover = EnemyParams.Int(cfg, id, "hoverTicks", 25);
        turnHover = EnemyParams.Int(cfg, id, "turnHoverTicks", 14);
        dashVx0 = EnemyParams.Fixed256(cfg, id, "dashVx0", 0.125f);
        dashAccel = EnemyParams.Fixed256(cfg, id, "dashAccel", 0.03125f);
        dashMax = EnemyParams.Fixed256(cfg, id, "dashMaxVx", 3f);
        brakeDecel = Mathf.Max(1, EnemyParams.Fixed256(cfg, id, "brakeDecel", 0.1875f));
        dashTurns = EnemyParams.Int(cfg, id, "dashTurns", 1);
        turnsLeft = dashTurns;
        glides = dashes = 0;
        takeoffAge = -1;
        afterBrake = false;
        forcedVy0 = null;
        vx = vy = 0;
        Facing = FacingToPlayer();
        Fsm.SetState(eRavenState.Perch);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
