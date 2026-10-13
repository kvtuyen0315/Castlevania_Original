using System;
using UnityEngine;

/// <summary>
/// Simon (CVGP 08) — di chuyển đo từ video Map_1 / Boss (CVGP_MEMORY §2 "Simon"), số liệu ở ConfigSimon (H6), chạy theo tick
/// StageWorld (H4: không Update/Physics). Mỗi state 1 lớp trong Simon/States (kế thừa SimonState : BaseState&lt;SimonController&gt;).
/// Toạ độ = pivot (x tâm thân, y MẶT SÀN) px section; hộp địa hình body 16×32. Vẽ: viewOffset (0,1) + DrawOffset của tư thế:
///   co chân khi nhảy +JumpTuckDrawY, ngồi/đáp khựng +CrouchDrawY, walk_1 khi lên thang +StairWalkPoseDrawY, xuống thang lùi
///   StairDownDrawBack px. Sheet vẽ quay TRÁI ⇒ quay phải = flipX quanh pivot (AnimPlayer.Apply).
/// Nhảy = bảng độ cao (JumpRise01..16 đi lên, đỉnh JumpApexTicks tick, rơi đối xứng; 40 tick trên sàn phẳng), hướng ngang chốt
/// lúc bấm A (1 px/tick). Hết bảng mà chưa chạm đất ⇒ Fall (FallSpeed hằng, giữ vận tốc ngang). Rơi ≥ LandCrouchMinFallTicks
/// tick ⇒ Land (ngồi khựng LandCrouchTicks). Cầu thang: đường StairPath (CVGP 07) + lệch x theo hướng thang; 0.5 px/tick; thả nút
/// ⇒ đi nốt tới bội StairStepPx. Trúng đòn: mất máu (GameSession.playerHp, ConfigBlock theo stage), knockback cung parabol lùi
/// ngược hướng mặt (trên thang: không văng — wiki), bất tử nháy InvincibleTicks từ lúc đáp. Hết máu ⇒ Dead; nước/hố ⇒ FallDeath.
/// Routine 09 thêm roi/vũ khí phụ: state mới + gọi lại AirStep/StairMove/GroundTransitions (không sửa state có sẵn).
/// </summary>
public class SimonController : Entity
{
    public const int JUMP_RISE_TICKS = 16;

    [Tooltip("Scene thử: chết xong (DeathTicks) tự hồi sinh ở điểm xuất phát + đầy máu. Màn chơi thật (routine 12) để tắt và nghe Died.")]
    [SerializeField] private bool autoRespawn;

    public StateMachine<SimonController, eSimonState> Fsm { get; private set; }
    public Hurtbox Hurt { get; private set; }

    // ---------------------------------------------------------------- số liệu (ConfigSimon — đọc 1 lần ở OnSpawned)
    [NonSerialized] public int walkSpeed;            // fixed/tick
    [NonSerialized] public int stairSpeed;           // fixed/tick
    [NonSerialized] public int fallSpeed;            // fixed/tick
    /// <summary>dy (px, dương = xuống) ở tick nhảy 1..jumpTicks.</summary>
    [NonSerialized] public int[] jumpDy = new int[0];
    [NonSerialized] public int jumpTicks;
    [NonSerialized] public int tuckStart, untuck, tuckDrawY, crouchDrawY;
    [NonSerialized] public int landCrouchMinFall, landCrouchTicks;
    [NonSerialized] public int stairStep, stairEnterRange, stairOffUpRight, stairOffUpLeft, stairWalkDrawY, stairDownBack, stairFrameTicks;
    [NonSerialized] public int kbVy0, kbGravity, kbVx, kbDxTicks;
    [NonSerialized] public int blinkTicks, blinkPeriod, deathKneelTicks, deathTicks;
    [NonSerialized] public bool stairKnockOff;
    [NonSerialized] public RectInt standHurt, tuckHurt, crouchHurt;

    // ---------------------------------------------------------------- trạng thái chuyển động (state đọc/ghi)
    [NonSerialized] public int jumpDir;          // hướng chốt lúc bấm A (−1/0/+1)
    [NonSerialized] public int jumpTick;         // tick đã bay (1 = tick đầu)
    [NonSerialized] public bool jumpLanded;
    /// <summary>Routine 09: sau đòn trên không quay về khung co chân dù đã qua JumpUntuckTick (video Map_1 2:14.6).</summary>
    [NonSerialized] public bool jumpTuckAfterAttack;
    [NonSerialized] public int airVx;            // fixed/tick (nhảy, rơi)
    [NonSerialized] public int fallTicks;
    [NonSerialized] public bool fallLanded;
    [NonSerialized] public StairPath stair;
    [NonSerialized] public int stairT;           // fixed, px đã lên tính từ chân thang
    [NonSerialized] public int stairMoveDir;     // +1 lên / −1 xuống (lần đi gần nhất)
    [NonSerialized] public int approachX;        // StairApproach: x đích
    [NonSerialized] public eSimonState approachNext;
    [NonSerialized] public int kbDir, kbVy;
    [NonSerialized] public bool kbLanded;
    [NonSerialized] public int autoWalkDir, autoWalkTarget, autoWalkSpeed;
    [NonSerialized] public Vector2Int home;

    /// <summary>Phần vẽ thêm (px NES) của tư thế hiện tại — state đặt mỗi tick; SyncView cộng vào viewOffset.</summary>
    [NonSerialized] public Vector2Int drawOffset;

    // anim (cache ở OnBuild — không tra chuỗi mỗi tick)
    public AnimSet.Anim aIdle, aWalk, aTuck, aDuck, aStairsUp, aStairsDown, aHurt, aDeath;

    // ---------------------------------------------------------------- sự kiện / đếm (HUD, luật chơi routine 12, harness)
    /// <summary>Hết máu (sau DeathTicks) hoặc rơi hố/nước (sau DeathTicks). Tham số: loại ô (Empty = hết máu).</summary>
    public event Action<SimonController, eTileCollision> Died;
    public event Action<SimonController> AutoWalkDone;
    [NonSerialized] public int hitsTaken, deathCount, landCount, jumpCount;
    [NonSerialized] public eTileCollision lastDeath;

    public override string DebugState
    {
        get
        {
            if (Fsm == null || !Fsm.HasState)
                return "";
            switch (Fsm.CurState)
            {
                case eSimonState.Jump: return "Jump t" + jumpTick;
                case eSimonState.StairIdle:
                case eSimonState.StairUp:
                case eSimonState.StairDown: return Fsm.CurState + " t" + Fixed.ToPx(stairT) + "/" + stair.Length;
                default: return Fsm.CurState.ToString();
            }
        }
    }

    public bool AutoRespawn { get => autoRespawn; set => autoRespawn = value; }
    public bool IsDying => Fsm.Is(eSimonState.Dead) || Fsm.Is(eSimonState.FallDeath);
    public bool OnStairs => Fsm.Is(eSimonState.StairIdle) || Fsm.Is(eSimonState.StairUp) || Fsm.Is(eSimonState.StairDown);
    public int StairTPx => Fixed.ToPx(stairT);

    // ---------------------------------------------------------------- input (chỉ đọc NesPad — H7)
    public int InputX => NesPad.I != null ? NesPad.I.Direction.x : 0;
    public bool Held(eNesButton b) => NesPad.I != null && NesPad.I.Held(b);
    public bool Pressed(eNesButton b) => NesPad.I != null && NesPad.I.Pressed(b);

    // ---------------------------------------------------------------- vòng đời

    protected override void OnBuild()
    {
        Fsm = new StateMachine<SimonController, eSimonState>(this, "Simon");
        Fsm.Add(eSimonState.Idle, new SimonIdleState());
        Fsm.Add(eSimonState.Walk, new SimonWalkState());
        Fsm.Add(eSimonState.Crouch, new SimonCrouchState());
        Fsm.Add(eSimonState.Jump, new SimonJumpState());
        Fsm.Add(eSimonState.Fall, new SimonFallState());
        Fsm.Add(eSimonState.Land, new SimonLandState());
        Fsm.Add(eSimonState.StairApproach, new SimonStairApproachState());
        Fsm.Add(eSimonState.StairIdle, new SimonStairIdleState());
        Fsm.Add(eSimonState.StairUp, new SimonStairUpState());
        Fsm.Add(eSimonState.StairDown, new SimonStairDownState());
        Fsm.Add(eSimonState.Hurt, new SimonHurtState());
        Fsm.Add(eSimonState.Dead, new SimonDeadState());
        Fsm.Add(eSimonState.FallDeath, new SimonFallDeathState());
        Fsm.Add(eSimonState.AutoWalk, new SimonAutoWalkState());
        Hurt = AddHurtbox(eHurtLayer.Player, new RectInt(-8, -30, 16, 30));
        if (animSet != null)
        {
            aIdle = animSet.Get("idle");
            aWalk = animSet.Get("walk");
            aTuck = animSet.Get("jump");
            aDuck = animSet.Get("duck");
            aStairsUp = animSet.Get("stairs_up");
            aStairsDown = animSet.Get("stairs_down");
            aHurt = animSet.Get("hurt");
            aDeath = animSet.Get("death");
        }
    }

    protected override void OnSpawned()
    {
        LoadTuning(World.Config);
        if (fx == 0 && fy == 0 && World.Section != null && World.Section.data.hasSimonStart)
        {
            SetPositionPx(World.Section.data.simonStart.x, World.Section.data.simonStart.y);
            Facing = 1;
        }
        home = new Vector2Int(X, Y);
        ResetMotion();
        Fsm.SetState(eSimonState.Idle);
    }

    /// <summary>Đọc ConfigSimon (fixed-point) — chỉ lúc spawn/teleport (GetRow tạo chuỗi).</summary>
    public void LoadTuning(GameConfig cfg)
    {
        var s = cfg != null ? cfg.simon : null;
        if (s == null)
        {
            Debug.LogError("[Simon] thiếu GameConfig/ConfigSimon");
            return;
        }
        walkSpeed = Fixed.FromFloat(s.GetFloat(eSimonParam.WalkSpeed));
        stairSpeed = Fixed.FromFloat(s.GetFloat(eSimonParam.StairSpeedX));
        fallSpeed = Fixed.FromFloat(s.GetFloat(eSimonParam.FallSpeed));
        // bảng nhảy: đi lên JumpRise01..16, giữ đỉnh JumpApexTicks tick, rơi = gương pha lên
        int apex = s.GetInt(eSimonParam.JumpApexTicks);
        jumpTicks = JUMP_RISE_TICKS * 2 + apex;
        jumpDy = new int[jumpTicks + 1];
        for (int i = 1; i <= JUMP_RISE_TICKS; i++)
        {
            int rise = s.GetInt((eSimonParam)((int)eSimonParam.JumpRise01 + i - 1));
            jumpDy[i] = -rise;
            jumpDy[jumpTicks + 1 - i] = rise;
        }
        tuckStart = s.GetInt(eSimonParam.JumpTuckStartTick);
        untuck = s.GetInt(eSimonParam.JumpUntuckTick);
        tuckDrawY = s.GetInt(eSimonParam.JumpTuckDrawY);
        crouchDrawY = s.GetInt(eSimonParam.CrouchDrawY);
        landCrouchMinFall = s.GetInt(eSimonParam.LandCrouchMinFallTicks);
        landCrouchTicks = s.GetInt(eSimonParam.LandCrouchTicks);
        stairStep = s.GetInt(eSimonParam.StairStepPx);
        stairEnterRange = s.GetInt(eSimonParam.StairEnterRange);
        stairOffUpRight = s.GetInt(eSimonParam.StairLineOffsetUpRight);
        stairOffUpLeft = s.GetInt(eSimonParam.StairLineOffsetUpLeft);
        stairWalkDrawY = s.GetInt(eSimonParam.StairWalkPoseDrawY);
        stairDownBack = s.GetInt(eSimonParam.StairDownDrawBack);
        stairFrameTicks = s.GetInt(eSimonParam.StairFrameTicks);
        kbVy0 = Fixed.FromFloat(s.GetFloat(eSimonParam.KnockbackVy0));
        kbGravity = Fixed.FromFloat(s.GetFloat(eSimonParam.KnockbackGravity));
        kbDxTicks = s.GetInt(eSimonParam.KnockbackDxTicks);
        kbVx = kbDxTicks > 0 ? Fixed.FromPx(s.GetInt(eSimonParam.KnockbackDx)) / kbDxTicks : 0;
        blinkTicks = s.GetInt(eSimonParam.InvincibleTicks);
        blinkPeriod = Mathf.Max(1, s.GetInt(eSimonParam.InvincibleBlinkPeriod));
        deathKneelTicks = s.GetInt(eSimonParam.DeathKneelTicks);
        deathTicks = s.GetInt(eSimonParam.DeathTicks);
        stairKnockOff = s.GetInt(eSimonParam.StairKnockOff) != 0;
        int w = s.GetInt(eSimonParam.HurtboxWidth), h = s.GetInt(eSimonParam.HurtboxHeight), hc = s.GetInt(eSimonParam.CrouchHurtboxHeight);
        // hàng đục trên cùng của khung = Y − h + drawY (idle_0 hàng 1–30; jumpduck_0 hàng 1–23 vẽ lệch +drawY)
        standHurt = new RectInt(-w / 2, -h, w, h);
        tuckHurt = new RectInt(-w / 2, tuckDrawY - h, w, hc);
        crouchHurt = new RectInt(-w / 2, crouchDrawY - h, w, hc);
        InvincibleOnHit = blinkTicks;
    }

    private void ResetMotion()
    {
        vx = vy = 0;
        airVx = 0;
        jumpTick = 0;
        jumpTuckAfterAttack = false;
        fallTicks = 0;
        stairT = 0;
        stairMoveDir = 1;
        drawOffset = Vector2Int.zero;
        movedXTick = movedYTick = -1;
        InvincibleTicks = 0;
        Hurt.active = true;
        Hurt.local = standHurt;
        if (Anim != null)
            Anim.Visible = true;
    }

    /// <summary>Đặt lại vị trí/hướng/trạng thái (harness, hồi sinh). Đọc lại Config.</summary>
    public void Teleport(int x, int y, int facing)
    {
        LoadTuning(World.Config);
        SetPositionPx(x, y);
        Facing = facing >= 0 ? 1 : -1;
        home = new Vector2Int(x, y);
        ResetMotion();
        Fsm.SetState(eSimonState.Idle);
    }

    public void ResetStats()
    {
        hitsTaken = deathCount = landCount = jumpCount = 0;
        lastDeath = eTileCollision.Empty;
    }

    public override void OnTick()
    {
        Fsm.Tick();
        base.OnTick();
    }

    // ---------------------------------------------------------------- combat

    public override bool OnHurt(ref HitInfo hit)
    {
        if (IsDying || Fsm.Is(eSimonState.Hurt))
            return false;
        hitsTaken++;
        var session = GameSession.Current;
        session.playerHp = Mathf.Max(0, session.playerHp - Mathf.Max(0, hit.damage));
        session.NotifyChanged();
        if (OnStairs && !stairKnockOff)
        {
            // wiki Stage 17: trên thang không bao giờ bị đánh văng — chỉ mất máu + nháy (resolver đặt InvincibleTicks)
            if (session.playerHp <= 0)
                Fsm.SetState(eSimonState.Dead);
            return true;
        }
        // knockback: lùi NGƯỢC hướng mặt (video boss f363: quay trái ⇒ bật sang phải; MAP1_TIMELINE §4)
        kbDir = -Facing;
        Fsm.SetState(eSimonState.Hurt);
        return true;
    }

    public override void OnDeadlyTile(eTileCollision kind)
    {
        if (IsDying)
            return;
        lastDeath = kind;
        Fsm.SetState(eSimonState.FallDeath);
    }

    internal void RaiseDied(eTileCollision kind)
    {
        deathCount++;
        lastDeath = kind;
        if (Died != null)
            Died(this, kind);
        if (autoRespawn)
        {
            GameSession.Current.playerHp = GameSession.MAX_HP;
            GameSession.Current.NotifyChanged();
            Teleport(home.x, home.y, 1);
            if (World.CameraCtl != null)
                World.CameraCtl.SnapToTarget();
        }
    }

    internal void RaiseAutoWalkDone()
    {
        if (AutoWalkDone != null)
            AutoWalkDone(this);
    }

    /// <summary>Kịch bản (cửa/cổng — routine 15): tự đi tới targetX với tốc độ speed (fixed/tick; 0 = WalkSpeed), bỏ qua input.</summary>
    public void StartAutoWalk(int targetX, int speed = 0)
    {
        autoWalkTarget = targetX;
        autoWalkDir = targetX >= X ? 1 : -1;
        autoWalkSpeed = speed > 0 ? speed : walkSpeed;
        Fsm.SetState(eSimonState.AutoWalk);
    }

    // ---------------------------------------------------------------- vật lý dùng chung (state + routine 09 gọi)

    // StateMachine chạy NỐI state mới trong cùng tick (CVGP 07) ⇒ mỗi trục chỉ di chuyển 1 lần/tick (vd Walk vừa đi 1 px rồi bấm A:
    // Jump tick đầu chỉ đi dọc; đáp đất rồi sang Walk: không đi thêm). Tick đã dùng trục: movedXTick / movedYTick.
    private int movedXTick = -1, movedYTick = -1;

    public bool OnGround() => IsOnGround();
    public bool CanStepX => World.TickCount != movedXTick;
    public bool CanStepY => World.TickCount != movedYTick;

    /// <summary>Đi ngang (fixed) tối đa 1 lần/tick; trả true nếu đụng tường.</summary>
    public bool StepX(int dxFixed)
    {
        if (!CanStepX)
            return false;
        movedXTick = World.TickCount;
        return MoveX(dxFixed);
    }

    /// <summary>Đi dọc (fixed, dương = xuống) tối đa 1 lần/tick; trả true nếu chạm sàn (đi xuống — kể cả vừa tới đúng mặt sàn) hoặc trần.</summary>
    public bool StepY(int dyFixed, bool ceiling = true)
    {
        if (!CanStepY)
            return false;
        movedYTick = World.TickCount;
        bool hit = MoveY(dyFixed, ceiling);
        return hit || (dyFixed > 0 && IsOnGround());
    }

    /// <summary>Chuyển về state mặt đất theo input (sau khi đáp/đứng dậy): Walk nếu giữ ←/→, Crouch nếu giữ ↓, không thì Idle.</summary>
    public void ToGroundState()
    {
        if (Held(eNesButton.Down) && InputX == 0)
            Fsm.SetState(eSimonState.Crouch);
        else if (InputX != 0)
            Fsm.SetState(eSimonState.Walk);
        else
            Fsm.SetState(eSimonState.Idle);
    }

    /// <summary>Chuyển state chung khi đứng trên đất (Idle/Walk/Crouch; routine 09 gọi khi đòn đất kết thúc). Trả true nếu đã đổi.</summary>
    public bool GroundTransitions(int walkVx)
    {
        if (!IsOnGround())
        {
            StartFall(walkVx);
            return true;
        }
        if (Pressed(eNesButton.A))
        {
            StartJump(InputX);
            return true;
        }
        if (Held(eNesButton.Up) && TryEnterStair(1))
            return true;
        if (Held(eNesButton.Down) && TryEnterStair(-1))
            return true;
        return false;
    }

    public void StartJump(int dir)
    {
        jumpDir = dir;
        jumpCount++;
        Fsm.SetState(eSimonState.Jump);
    }

    public void StartFall(int vxFixed)
    {
        airVx = vxFixed;
        Fsm.SetState(eSimonState.Fall);
    }

    /// <summary>1 tick bay theo bảng nhảy: ngang airVx (đụng tường thì dừng), dọc jumpDy[jumpTick] (chặn trần). Trả true khi chạm đất.</summary>
    public bool AirStep()
    {
        jumpTick++;
        if (airVx != 0)
            StepX(airVx);
        int dy = jumpTick <= jumpTicks ? jumpDy[jumpTick] : 0;
        if (dy != 0 && StepY(Fixed.FromPx(dy), true) && dy > 0)
        {
            jumpLanded = true;
            landCount++;
        }
        return jumpLanded;
    }

    /// <summary>Tư thế khi nhảy: đứng ở tick &lt; JumpTuckStartTick và ≥ JumpUntuckTick (trừ sau đòn trên không), còn lại co chân (+4).</summary>
    public void ApplyJumpPose()
    {
        bool tuck = jumpTick >= tuckStart && (jumpTick < untuck || jumpTuckAfterAttack);
        if (tuck)
        {
            PlayStatic(aTuck);
            drawOffset = new Vector2Int(0, tuckDrawY);
            Hurt.local = tuckHurt;
        }
        else
        {
            PlayStatic(aIdle);
            drawOffset = Vector2Int.zero;
            Hurt.local = standHurt;
        }
    }

    public void PlayStatic(AnimSet.Anim a)
    {
        if (Anim != null && a != null && !Anim.IsPlaying(a))
            Anim.SetFrame(a, 0);
    }

    // ---------------------------------------------------------------- cầu thang

    /// <summary>Lệch x của đường đi Simon so với StairPath (tâm bậc), dương = về phía thang đi LÊN: ↗ +3, ↖ 0 (đo video — ConfigSimon).</summary>
    public int StairOffset(StairPath p)
    {
        return p.upDx > 0 ? stairOffUpRight : -stairOffUpLeft;
    }

    /// <summary>
    /// Bước lên (dir &gt; 0, ở chân) / xuống (dir &lt; 0, ở đỉnh) thang gần nhất trong StairEnterRange px. Đứng phía "trong" đường thang
    /// (gầm thang khi lên, trên thang khi xuống) ⇒ kẹp lên đường thang tại x hiện tại (video: bấm Lên khi cách chân 3–4 px là đã ở bậc
    /// t = 3–4); phía còn lại ⇒ StairApproach tự đi tới chân/đỉnh rồi leo. Trả true nếu đã vào thang.
    /// </summary>
    public bool TryEnterStair(int dir)
    {
        var tiles = World.Tiles;
        int best = int.MaxValue, bestI = -1, bestD = 0;
        for (int i = 0; i < tiles.StairCount; i++)
        {
            var p = tiles.GetStair(i);
            var end = dir > 0 ? p.bottom : p.top;
            if (end.y != Y)
                continue;
            int ex = end.x + StairOffset(p);
            int d = (X - ex) * p.upDx;   // > 0: phía thang đi LÊN
            if (Mathf.Abs(d) > stairEnterRange || Mathf.Abs(d) >= best)
                continue;
            best = Mathf.Abs(d);
            bestI = i;
            bestD = d;
        }
        if (bestI < 0)
            return false;
        stair = tiles.GetStair(bestI);
        int off = StairOffset(stair);
        if (dir > 0)
        {
            if (bestD > 0)
            {
                SetStairT(Fixed.FromPx(bestD));
                stairMoveDir = 1;
                Fsm.SetState(eSimonState.StairUp);
            }
            else
            {
                approachX = stair.bottom.x + off;
                approachNext = eSimonState.StairUp;
                stairMoveDir = 1;
                if (bestD == 0)
                    SetStairT(0);
                Fsm.SetState(bestD == 0 ? eSimonState.StairUp : eSimonState.StairApproach);
            }
        }
        else
        {
            if (bestD < 0)
            {
                SetStairT(Fixed.FromPx(stair.Length + bestD));
                stairMoveDir = -1;
                Fsm.SetState(eSimonState.StairDown);
            }
            else
            {
                approachX = stair.top.x + off;
                approachNext = eSimonState.StairDown;
                stairMoveDir = -1;
                if (bestD == 0)
                    SetStairT(Fixed.FromPx(stair.Length));
                Fsm.SetState(bestD == 0 ? eSimonState.StairDown : eSimonState.StairApproach);
            }
        }
        return true;
    }

    /// <summary>Đặt vị trí theo tham số thang t (fixed px đã lên tính từ chân).</summary>
    public void SetStairT(int t)
    {
        stairT = Mathf.Clamp(t, 0, Fixed.FromPx(stair.Length));
        int tp = Fixed.ToPx(stairT);
        var q = stair.PointAt(tp);
        SetPositionPx(q.x + StairOffset(stair), q.y);
    }

    /// <summary>1 tick đi thang (dir +1 lên / −1 xuống) 0.5 px/tick; quay mặt theo hướng đi; đặt khung theo vị trí.</summary>
    public void StairMove(int dir)
    {
        stairMoveDir = dir;
        Facing = dir * stair.upDx;
        SetStairT(stairT + dir * stairSpeed);
        ApplyStairPose();
    }

    public bool AtStairStep => Fixed.Frac(stairT) == 0 && Fixed.ToPx(stairT) % Mathf.Max(1, stairStep) == 0;
    public bool AtStairTop => stairT >= Fixed.FromPx(stair.Length);
    public bool AtStairBottom => stairT <= 0;

    /// <summary>Input dọc theo thang: +1 lên (↑ hoặc hướng ngang của thang), −1 xuống, 0 không. ↑/↓ ưu tiên.</summary>
    public int StairInput()
    {
        if (Held(eNesButton.Up))
            return 1;
        if (Held(eNesButton.Down))
            return -1;
        int x = InputX;
        return x == 0 ? 0 : x == stair.upDx ? 1 : -1;
    }

    /// <summary>Khung trên thang theo VỊ TRÍ t (đo video): lên — stairs_up_0 khi (t−1) mod 8 &lt; 4, walk_1 vẽ thấp 2 px;
    /// xuống — stairs_down_0 khi t mod 8 &lt; 4, vẽ lùi StairDownDrawBack px ngược hướng đi ngang.</summary>
    public void ApplyStairPose()
    {
        int t = Fixed.ToPx(stairT);
        int half = Mathf.Max(1, stairFrameTicks * stairSpeed / Fixed.ONE);   // 8 tick × 0.5 = 4 px mỗi khung
        int cycle = half * 2;
        Hurt.local = standHurt;
        if (stairMoveDir >= 0)
        {
            bool a = Mod(t - 1, cycle) < half;
            if (Anim != null && aStairsUp != null)
                Anim.SetFrame(aStairsUp, a ? 0 : 1);
            drawOffset = new Vector2Int(0, a ? 0 : stairWalkDrawY);
        }
        else
        {
            bool a = Mod(t, cycle) < half;
            if (Anim != null && aStairsDown != null)
                Anim.SetFrame(aStairsDown, a ? 0 : 1);
            drawOffset = new Vector2Int(stairDownBack * stair.upDx, 0);
        }
    }

    /// <summary>Ra khỏi thang ở đầu đang đứng: đặt lên mặt sàn rồi về state đất (không có sàn — thang cắt mép section ⇒ StairIdle).</summary>
    public void ExitStair()
    {
        drawOffset = Vector2Int.zero;
        if (IsOnGround())
            ToGroundState();
        else
            Fsm.SetState(eSimonState.StairIdle);
    }

    public static int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }

    // ---------------------------------------------------------------- vẽ

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x + drawOffset.x, Y + viewOffset.y + drawOffset.y);
        if (Anim == null)
            return;
        Anim.Apply(Facing);
        // nháy bất tử: 1 tick hiện / 1 tick ẩn (InvincibleBlinkPeriod) — không nháy khi đang bay knockback; chết ⇒ hiện
        bool visible = InvincibleTicks <= 0 || Fsm.Is(eSimonState.Hurt) || IsDying || InvincibleTicks % blinkPeriod == 0;
        if (Fsm.Is(eSimonState.FallDeath) && Y - 32 >= (World != null && World.Tiles != null ? World.Tiles.Height : 192))
            visible = false;
        Anim.Visible = visible;
    }
}
