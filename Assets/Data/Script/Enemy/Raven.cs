using UnityEngine;

/// <summary>State Raven.</summary>
public enum eRavenState
{
    Perch,
    Rise,
    Hover,
    Dive,
}

/// <summary>
/// Raven (CVGP 17, theo wiki — GẦN ĐÚNG, không có video Block 3). Wiki Enemies#Ravens: đậu chờ; cất cánh khi Simon lại gần, treo một
/// thoáng rồi bổ nhào về phía Simon; trượt thì quay đầu rất nhanh để đánh từ phía sau, có thể lao lần nữa trước khi rời màn; 1/1 đòn,
/// 200 đ. Ước lượng (Enemy.Raven): đậu (raven_0) tới khi |dx| &lt; triggerDistancePx ⇒ bay thẳng lên riseVy × riseTicks ⇒ khựng
/// firstHoverTicks (quay về Simon) ⇒ lao THẲNG tới tâm thân Simon lúc bắt đầu lao (chân − targetAboveFeetPx) với tốc độ
/// ConfigEnemy.moveSpeed, đi quá điểm nhắm diveOvershootPx (tối đa maxDiveTicks) ⇒ khựng hoverTicks (ngắn — "quay đầu rất nhanh") ⇒
/// lao tiếp … tới khi ra khỏi màn/bị hạ; đang lao xuống mà tâm chạm chân Simon − diveFloorAboveFeetPx ⇒ dừng lao (không xuyên sàn). Xuyên địa hình. Pivot = tâm sprite. Đặt bằng SpawnPoint.
/// </summary>
public class Raven : EnemyBase
{
    public StateMachine<Raven, eRavenState> Fsm { get; private set; }
    [System.NonSerialized] public int diveSpeed, trigger, riseVy, riseTicks, firstHover, hover, overshoot, maxDive, targetAbove, floorAbove;
    [System.NonSerialized] public int hoverLen, diveLen, diveFloorY;
    /// <summary>Số cú lao / tuổi lúc cất cánh (harness).</summary>
    [System.NonSerialized] public int dives, takeoffAge;
    private AnimSet.Anim aPerch, aFly;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class RavenState : BaseState<Raven>
    {
        protected void SetState(eRavenState s) { owner.Fsm.SetState(s); }
    }

    class PerchState : RavenState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = false;
            if (owner.Anim != null && owner.aPerch != null)
                owner.Anim.Play(owner.aPerch, true);
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (p != null && Mathf.Abs(p.X - owner.X) < owner.trigger)
            {
                owner.takeoffAge = owner.ageTicks;
                SetState(eRavenState.Rise);
            }
        }
    }

    class RiseState : RavenState
    {
        public override void OnEnterState()
        {
            owner.checkOffscreen = true;
            owner.Facing = owner.FacingToPlayer();
            if (owner.Anim != null && owner.aFly != null)
                owner.Anim.Play(owner.aFly, true);
        }

        public override void OnExecuteState()
        {
            owner.fy += owner.riseVy;
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.riseTicks)
            {
                owner.hoverLen = owner.firstHover;
                SetState(eRavenState.Hover);
            }
        }
    }

    class HoverState : RavenState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
        }

        public override void CheckNextState()
        {
            if (StateFrame > owner.hoverLen)   // đứng yên đúng hoverLen tick
                SetState(eRavenState.Dive);
        }
    }

    class DiveState : RavenState
    {
        public override void OnEnterState()
        {
            owner.dives++;
            owner.AimDive();
        }

        public override void OnExecuteState()
        {
            owner.fx += owner.vx;
            owner.fy += owner.vy;
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.diveLen || (owner.vy > 0 && owner.Y >= owner.diveFloorY))
            {
                owner.hoverLen = owner.hover;
                SetState(eRavenState.Hover);
            }
        }
    }

    /// <summary>Vận tốc thẳng tới tâm thân Simon (tính nguyên — H7) + số tick lao = (khoảng cách + overshoot)/tốc độ.</summary>
    private void AimDive()
    {
        var p = World.Player;
        long dx = p != null ? p.X - X : Facing * 64, dy = p != null ? (p.Y - targetAbove) - Y : 0;
        long len = ISqrt(dx * dx + dy * dy);
        if (len == 0)
        {
            dx = Facing;
            len = 1;
        }
        diveFloorY = (p != null ? p.Y : Y + 64) - floorAbove;   // không lao xuyên xuống dưới chân Simon (ảnh routine 17 r160)
        vx = (int)(diveSpeed * dx / len);
        vy = (int)(diveSpeed * dy / len);
        if (vx != 0)
            Facing = vx > 0 ? 1 : -1;
        diveLen = Mathf.Clamp((int)((len + overshoot) * Fixed.ONE / Mathf.Max(1, diveSpeed)), 1, maxDive);
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

    protected override void BuildStates()
    {
        Fsm = new StateMachine<Raven, eRavenState>(this, "Raven");
        Fsm.Add(eRavenState.Perch, new PerchState());
        Fsm.Add(eRavenState.Rise, new RiseState());
        Fsm.Add(eRavenState.Hover, new HoverState());
        Fsm.Add(eRavenState.Dive, new DiveState());
        if (animSet != null)
        {
            aPerch = animSet.Get("perch");
            aFly = animSet.Get("fly");
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        diveSpeed = EnemyParams.MoveSpeed(cfg, id, 2f);
        trigger = EnemyParams.Int(cfg, id, "triggerDistancePx", 96);
        riseVy = EnemyParams.Fixed256(cfg, id, "riseVy", -1f);
        riseTicks = EnemyParams.Int(cfg, id, "riseTicks", 16);
        firstHover = EnemyParams.Int(cfg, id, "firstHoverTicks", 24);
        hover = EnemyParams.Int(cfg, id, "hoverTicks", 12);
        overshoot = EnemyParams.Int(cfg, id, "diveOvershootPx", 32);
        maxDive = EnemyParams.Int(cfg, id, "maxDiveTicks", 96);
        targetAbove = EnemyParams.Int(cfg, id, "targetAboveFeetPx", 16);
        floorAbove = EnemyParams.Int(cfg, id, "diveFloorAboveFeetPx", 8);
        dives = 0;
        takeoffAge = -1;
        vx = vy = 0;
        Facing = FacingToPlayer();
        Fsm.SetState(eRavenState.Perch);
    }

    protected override void TickStates()
    {
        Fsm.Tick();
    }
}
