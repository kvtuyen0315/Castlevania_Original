using UnityEngine;

public enum eDummyState
{
    Idle,
    Walk,
    Air,
    Stair,
    Dead,
}

/// <summary>
/// Thực thể THỬ của scene Arena (CVGP 07) — kiểm lõi (va chạm, cầu thang, camera, combat, state machine), KHÔNG phải Simon
/// (Simon thật: routine 08). Hộp 16×32, điều khiển bằng NesPad: ←/→ đi (ConfigSimon.WalkSpeed), A nhảy (vật lý thử bên dưới,
/// hướng chốt lúc bật nhảy), ↑/↓ ở chân/đỉnh thang ⇒ đi thang (ConfigSimon.StairSpeedX), B = hộp đánh thử trước mặt 8 tick.
/// Nước/hố ⇒ Dead 60 tick rồi về điểm xuất phát. Trúng đòn ⇒ bất tử ConfigSimon.InvincibleTicks (nháy).
/// Hình: mượn anim Simon (idle/walk/jump/stairs_up/stairs_down) chỉ để nhìn — không phải hành vi Simon.
/// Mẫu viết state theo phong cách code cũ: lớp nền DummyState có SetState(eDummyState), mỗi state 1 lớp lồng.
/// </summary>
public class ArenaDummy : Entity
{
    // Vật lý THỬ (KHÔNG phải số Simon — routine 08 đo video): nhảy vy0 −4.25 px/tick, trọng lực 0.25 px/tick², rơi tối đa
    // 4 px/tick ⇒ cao ≈ 36 px (đủ qua tường 32 px của Arena), bay ≈ 34 tick.
    public const int JUMP_VY0 = -1088;
    public const int GRAVITY = 64;
    public const int MAX_FALL = 1024;
    /// <summary>Khoảng dò điểm chân/đỉnh thang khi bấm ↑/↓ (px) — thử.</summary>
    public const int STAIR_RANGE = 8;
    public const int ATTACK_TICKS = 8;
    public const int DEAD_TICKS = 60;
    /// <summary>Hộp đánh thử: 32×8 ngay trước mặt (quay trái), cao 16–24 px trên chân.</summary>
    public static readonly RectInt ATTACK_BOX = new RectInt(-40, -24, 32, 8);
    public static readonly RectInt HURT_BOX = new RectInt(-8, -32, 16, 32);

    [Tooltip("Ghi đè tốc độ đi px/tick (0 = ConfigSimon.WalkSpeed) — kiểm không xuyên tường ở tốc độ cao.")]
    public float walkSpeedOverride;

    public StateMachine<ArenaDummy, eDummyState> Fsm { get; private set; }
    public Hitbox Attack { get; private set; }
    public Hurtbox Hurt { get; private set; }
    public Vector2Int Home { get; set; }

    // Số đếm cho kiểm thử
    public int HitsTaken;
    public int DeathCount;
    public eTileCollision LastDeath;
    public int LandCount;

    private int walkSpeed;
    private int stairSpeed;
    private int attackTimer;
    private bool jumpRequested;
    private int jumpDir;
    private StairPath stair;
    private int stairT;
    private AnimSet.Anim aIdle, aWalk, aJump, aUp, aDown;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "";
    public StairPath CurrentStair => stair;
    public int StairT => Fixed.ToPx(stairT);

    private NesPad Pad => NesPad.I;
    private int InputX => Pad != null ? Pad.Direction.x : 0;
    private bool Held(eNesButton b) => Pad != null && Pad.Held(b);
    private bool Pressed(eNesButton b) => Pad != null && Pad.Pressed(b);

    protected override void OnBuild()
    {
        Fsm = new StateMachine<ArenaDummy, eDummyState>(this, "Dummy");
        Fsm.Add(eDummyState.Idle, new IdleState());
        Fsm.Add(eDummyState.Walk, new WalkState());
        Fsm.Add(eDummyState.Air, new AirState());
        Fsm.Add(eDummyState.Stair, new StairState());
        Fsm.Add(eDummyState.Dead, new DeadState());
        Attack = AddHitbox(eHitLayer.PlayerAttack, ATTACK_BOX);
        Attack.strength = eHitStrength.Weak;
        Hurt = AddHurtbox(eHurtLayer.Player, HURT_BOX);
        if (animSet != null)
        {
            aIdle = animSet.Get("idle");
            aWalk = animSet.Get("walk");
            aJump = animSet.Get("jump");
            aUp = animSet.Get("stairs_up");
            aDown = animSet.Get("stairs_down");
        }
    }

    protected override void OnSpawned()
    {
        var cfg = World.Config;
        float walk = walkSpeedOverride > 0f ? walkSpeedOverride : cfg != null ? cfg.simon.GetFloat(eSimonParam.WalkSpeed) : 1f;
        walkSpeed = Fixed.FromFloat(walk);
        stairSpeed = Fixed.FromFloat(cfg != null ? cfg.simon.GetFloat(eSimonParam.StairSpeedX) : 0.5f);
        InvincibleOnHit = cfg != null ? cfg.simon.GetInt(eSimonParam.InvincibleTicks) : 0;
        Attack.damage = 1;
        // đặt sẵn trong scene (chưa có vị trí) ⇒ điểm bắt đầu của section, quay phải như Simon đầu stage
        if (fx == 0 && fy == 0 && World.Section != null && World.Section.data.hasSimonStart)
        {
            SetPositionPx(World.Section.data.simonStart.x, World.Section.data.simonStart.y);
            Facing = 1;
        }
        Home = new Vector2Int(X, Y);
        attackTimer = 0;
        Attack.Deactivate();
        Fsm.SetState(eDummyState.Idle);
    }

    /// <summary>Đặt lại vị trí/hướng/trạng thái (harness). Tốc độ đọc lại Config + walkSpeedOverride.</summary>
    public void Teleport(int x, int y, int facing)
    {
        SetPositionPx(x, y);
        Facing = facing;
        vx = vy = 0;
        InvincibleTicks = 0;
        Home = new Vector2Int(x, y);
        float walk = walkSpeedOverride > 0f ? walkSpeedOverride : World.Config != null ? World.Config.simon.GetFloat(eSimonParam.WalkSpeed) : 1f;
        walkSpeed = Fixed.FromFloat(walk);
        attackTimer = 0;
        Attack.Deactivate();
        if (Anim != null)
            Anim.Visible = true;
        Fsm.SetState(eDummyState.Idle);
    }

    public void ResetStats()
    {
        HitsTaken = DeathCount = LandCount = 0;
        LastDeath = eTileCollision.Empty;
    }

    public override void OnTick()
    {
        if (attackTimer > 0 && --attackTimer == 0)
            Attack.Deactivate();
        Fsm.Tick();
        base.OnTick();
        if (Anim != null && !Fsm.Is(eDummyState.Dead))
            Anim.Visible = InvincibleTicks == 0 || (InvincibleTicks & 2) == 0;
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (Fsm.Is(eDummyState.Dead))
            return false;
        HitsTaken++;
        return true;
    }

    public override void OnDeadlyTile(eTileCollision kind)
    {
        if (Fsm.Is(eDummyState.Dead))
            return;
        LastDeath = kind;
        DeathCount++;
        Fsm.SetState(eDummyState.Dead);
    }

    // ---------------------------------------------------------------- dùng chung cho state

    private void TryAttack()
    {
        if (Pressed(eNesButton.B) && attackTimer == 0)
        {
            Attack.Activate(ATTACK_BOX);
            attackTimer = ATTACK_TICKS;
        }
    }

    /// <summary>Chuyển state từ mặt đất (Idle/Walk). Trả true nếu đã đổi.</summary>
    private bool GroundTransitions()
    {
        if (!IsOnGround())
        {
            jumpRequested = false;
            Fsm.SetState(eDummyState.Air);
            return true;
        }
        if (Pressed(eNesButton.A))
        {
            jumpRequested = true;
            jumpDir = InputX;
            Fsm.SetState(eDummyState.Air);
            return true;
        }
        StairPath p;
        if (Held(eNesButton.Up) && World.Tiles.TryGetStair(X, Y, 1, STAIR_RANGE, out p))
        {
            EnterStair(p, 0);
            return true;
        }
        if (Held(eNesButton.Down) && World.Tiles.TryGetStair(X, Y, -1, STAIR_RANGE, out p))
        {
            EnterStair(p, p.Length);
            return true;
        }
        return false;
    }

    private void EnterStair(StairPath p, int t)
    {
        stair = p;
        stairT = Fixed.FromPx(t);
        var q = p.PointAt(t);
        SetPositionPx(q.x, q.y);
        Fsm.SetState(eDummyState.Stair);
    }

    // ---------------------------------------------------------------- state (phong cách BaseState cũ)

    private abstract class DummyState : BaseState<ArenaDummy>
    {
        protected void SetState(eDummyState s)
        {
            owner.Fsm.SetState(s);
        }
    }

    private class IdleState : DummyState
    {
        public override void OnEnterState()
        {
            owner.vx = owner.vy = 0;
            if (owner.Anim != null)
                owner.Anim.Play(owner.aIdle);
        }

        public override void OnExecuteState()
        {
            owner.TryAttack();
        }

        public override void CheckNextState()
        {
            if (owner.GroundTransitions())
                return;
            if (owner.InputX != 0)
                SetState(eDummyState.Walk);
        }
    }

    private class WalkState : DummyState
    {
        public override void OnEnterState()
        {
            if (owner.Anim != null)
                owner.Anim.Play(owner.aWalk);
        }

        public override void OnExecuteState()
        {
            int dir = owner.InputX;
            if (dir != 0)
            {
                owner.Facing = dir;
                owner.MoveX(dir * owner.walkSpeed);
            }
            owner.TryAttack();
        }

        public override void CheckNextState()
        {
            if (owner.GroundTransitions())
                return;
            if (owner.InputX == 0)
                SetState(eDummyState.Idle);
        }
    }

    private class AirState : DummyState
    {
        private bool landed;

        public override void OnEnterState()
        {
            landed = false;
            if (owner.jumpRequested)
            {
                owner.vy = JUMP_VY0;
                owner.vx = owner.jumpDir * owner.walkSpeed;
                if (owner.jumpDir != 0)
                    owner.Facing = owner.jumpDir;
            }
            else
            {
                owner.vy = 0;
                owner.vx = 0;
            }
            owner.jumpRequested = false;
            if (owner.Anim != null)
                owner.Anim.Play(owner.aJump);
        }

        public override void OnExecuteState()
        {
            if (owner.vx != 0)
                owner.MoveX(owner.vx);
            owner.vy = Mathf.Min(owner.vy + GRAVITY, MAX_FALL);
            if (owner.MoveY(owner.vy))
            {
                if (owner.vy > 0)
                {
                    landed = true;
                    owner.LandCount++;
                }
                owner.vy = 0;
            }
            owner.TryAttack();
        }

        public override void CheckNextState()
        {
            if (landed)
                SetState(owner.InputX != 0 ? eDummyState.Walk : eDummyState.Idle);
        }
    }

    private class StairState : DummyState
    {
        private int dir;

        public override void OnEnterState()
        {
            owner.vx = owner.vy = 0;
            dir = 0;
            if (owner.Anim != null)
                owner.Anim.Play(owner.aUp);
        }

        public override void OnExecuteState()
        {
            dir = owner.Held(eNesButton.Up) ? 1 : owner.Held(eNesButton.Down) ? -1 : 0;
            if (dir == 0)
                return;
            var p = owner.stair;
            int len = Fixed.FromPx(p.Length);
            owner.stairT = Mathf.Clamp(owner.stairT + dir * owner.stairSpeed, 0, len);
            var q = p.PointAt(Fixed.ToPx(owner.stairT));
            owner.SetPositionPx(q.x, q.y);
            owner.Facing = dir * p.upDx;
            if (owner.Anim != null)
                owner.Anim.Play(dir > 0 ? owner.aUp : owner.aDown);
            owner.TryAttack();
        }

        public override void CheckNextState()
        {
            if (dir > 0 && owner.stairT >= Fixed.FromPx(owner.stair.Length))
                SetState(eDummyState.Idle);
            else if (dir < 0 && owner.stairT <= 0)
                SetState(eDummyState.Idle);
        }
    }

    private class DeadState : DummyState
    {
        public override void OnEnterState()
        {
            owner.vx = owner.vy = 0;
            owner.Attack.Deactivate();
            if (owner.Anim != null)
                owner.Anim.Visible = false;
        }

        public override void CheckNextState()
        {
            if (StateFrame < DEAD_TICKS)
                return;
            owner.SetPositionPx(owner.Home.x, owner.Home.y);
            if (owner.Anim != null)
                owner.Anim.Visible = true;
            SetState(eDummyState.Idle);
        }
    }
}
