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
/// ⇒ đi nốt tới bội StairStepPx. Trúng đòn: mất máu (GameSession.playerHp, ConfigBlock theo stage), knockback = bảng độ cao theo tick
/// (CVM3 04: KnockbackRise01..10 + giữ đỉnh KnockbackApexTicks + xuống đối xứng) lùi ngược hướng mặt 1 px/tick suốt cung (trên thang:
/// không văng — wiki), bất tử nháy InvincibleTicks từ lúc đáp. Hết máu ⇒ Dead; nước/hố ⇒ FallDeath. Dò sàn (đứng/đáp) = cột X±FootProbeHalfWidth.
/// Tấn công (CVGP 09): 1 state Attack cho 4 tư thế (đứng/trên không, ngồi, lên thang, xuống thang) — roi 3 cấp (thân use_* +
/// roi lớp phủ, cấp 3 đổi 4 màu mỗi WhipColorTicks tick) hoặc ném vũ khí phụ (Up+B / phím tắt SubWeapon) cùng nhịp 3 khung
/// (WhipWindupTicks/RaiseTicks/ExtendTicks — đo video). Hitbox roi = bbox mảnh roi duỗi (ConfigWhip.hit*) đặt theo
/// overlayOffsets khung duỗi, bật suốt khung duỗi. Vũ khí phụ sinh ở tick SubWeaponSpawnTick của khung duỗi; tim trừ lúc bấm.
/// </summary>
public class SimonController : Entity
{
    public const int JUMP_RISE_TICKS = 16;
    /// <summary>CVM3 04: số tick pha lên của knockback (KnockbackRise01..10).</summary>
    public const int KB_RISE_TICKS = 10;

    [Tooltip("Scene thử: chết xong (DeathTicks) tự hồi sinh ở điểm xuất phát + đầy máu. Màn chơi thật (routine 12) để tắt và nghe Died.")]
    [SerializeField] private bool autoRespawn;

    [Header("CVGP 09 — vũ khí phụ (prefab dựng bằng menu CVGP/Build Weapon Prefabs)")]
    [SerializeField] private SubWeaponProjectile daggerPrefab;
    [SerializeField] private SubWeaponProjectile axePrefab;
    [SerializeField] private SubWeaponProjectile holyWaterPrefab;
    [SerializeField] private SubWeaponProjectile crossPrefab;
    [SerializeField] private SubWeaponProjectile stopwatchPrefab;

    public StateMachine<SimonController, eSimonState> Fsm { get; private set; }
    public Hurtbox Hurt { get; private set; }
    /// <summary>Hộp đánh của roi (PlayerAttack) — chỉ bật ở khung duỗi.</summary>
    public Hitbox WhipHit { get; private set; }

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
    [NonSerialized] public int kbVx;                 // fixed/tick (lùi suốt cung)
    /// <summary>CVM3 04: dy (px, dương = xuống) ở tick knockback 1..kbTicks; sau đó rơi FallSpeed.</summary>
    [NonSerialized] public int[] kbDy = new int[0];
    [NonSerialized] public int kbTicks;
    /// <summary>CVM3 04: nửa bề rộng đầu dò sàn (px; 0 = cả thân).</summary>
    [NonSerialized] public int footHalf;
    [NonSerialized] public int blinkTicks, blinkPeriod, deathKneelTicks, deathTicks;
    [NonSerialized] public bool stairKnockOff;
    [NonSerialized] public RectInt standHurt, tuckHurt, crouchHurt;
    // CVGP 09 — tấn công
    [NonSerialized] public int atkWindup, atkRaise, atkExtend, atkTotal, whipColorTicks, subSpawnTick;
    /// <summary>Bbox mảnh roi duỗi theo cấp (index 1..3), tính từ góc trên-trái mảnh (ConfigWhip.hit*).</summary>
    [NonSerialized] public RectInt[] whipPiece = new RectInt[4];
    [NonSerialized] public eHitStrength[] whipStrength = new eHitStrength[4];
    [NonSerialized] public int[] subCost = new int[6];
    [NonSerialized] public int[] subSpawnHeight = new int[6];
    [NonSerialized] public int shotCapBase, shotCapDouble, shotCapTriple;

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
    [NonSerialized] public int kbDir, kbTick;
    [NonSerialized] public bool kbLanded;
    [NonSerialized] public int autoWalkDir, autoWalkTarget, autoWalkSpeed;
    [NonSerialized] public bool autoWalkNoClip;      // CVGP 15: đi xuyên địa hình (qua cửa sang section kế)
    [NonSerialized] public Vector2Int home;
    // đòn đang đánh (SimonAttackState đọc/ghi)
    [NonSerialized] public eAttackPose atkPose;
    [NonSerialized] public bool atkThrow, atkAir, atkFalling, atkLanded, atkSpawned;
    [NonSerialized] public eSubWeapon atkSub;
    [NonSerialized] public int atkLevel, atkColor, atkPhase, atkFallTicks;

    /// <summary>Phần vẽ thêm (px NES) của tư thế hiện tại — state đặt mỗi tick; SyncView cộng vào viewOffset.</summary>
    [NonSerialized] public Vector2Int drawOffset;

    // CVGP 11 — nháy màu khi nhặt roi, tàng hình
    private int flashFrom = 1, flashTo, flashStep = 2;
    private int invisibleLeft, invisibleAge, invisVisibleTicks, invisGlintFrom, invisGlintPeriod = 1, invisGlintOn;
    private Material defaultMaterial;

    // anim (cache ở OnBuild — không tra chuỗi mỗi tick)
    public AnimSet.Anim aIdle, aWalk, aTuck, aDuck, aStairsUp, aStairsDown, aHurt, aDeath;
    /// <summary>Roi [cấp 1..3, màu 0..3 (chỉ cấp 3 dùng 1..3), tư thế eAttackPose]; ném [tư thế].</summary>
    [NonSerialized] public AnimSet.Anim[,,] aWhip = new AnimSet.Anim[4, 4, 4];
    [NonSerialized] public AnimSet.Anim[] aThrow = new AnimSet.Anim[4];
    public static readonly string[] POSE_NAMES = { "stand", "duck", "stairs_up", "stairs_down" };

    // ---------------------------------------------------------------- sự kiện / đếm (HUD, luật chơi routine 12, harness)
    /// <summary>Hết máu (sau DeathTicks) hoặc rơi hố/nước (sau DeathTicks). Tham số: loại ô (Empty = hết máu).</summary>
    public event Action<SimonController, eTileCollision> Died;
    /// <summary>CVM2 03: BẮT ĐẦU chết (vào Dead / FallDeath) — chỉ để phát nhạc Death đúng lúc (không đổi trạng thái gameplay).</summary>
    public static event Action<SimonController> DyingStarted;
    public event Action<SimonController> AutoWalkDone;
    [NonSerialized] public int hitsTaken, deathCount, landCount, jumpCount, whipCount, throwCount, throwFailCount;
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
                case eSimonState.Attack: return (atkThrow ? "Throw " + atkSub : "Whip L" + atkLevel + (atkLevel == 3 ? "c" + (atkColor + 1) : ""))
                                                + " " + atkPose + " k" + atkPhase + (atkAir ? " air" : "");
                default: return Fsm.CurState.ToString();
            }
        }
    }

    public bool AutoRespawn { get => autoRespawn; set => autoRespawn = value; }
    public bool IsDying => Fsm.Is(eSimonState.Dead) || Fsm.Is(eSimonState.FallDeath);
    public bool OnStairs => Fsm.Is(eSimonState.StairIdle) || Fsm.Is(eSimonState.StairUp) || Fsm.Is(eSimonState.StairDown)
                            || (Fsm.Is(eSimonState.Attack) && atkPose >= eAttackPose.StairsUp);
    public int StairTPx => Fixed.ToPx(stairT);

    // ---------------------------------------------------------------- input (chỉ đọc NesPad — H7)
    /// <summary>CVGP 12: khoá điều khiển (tally sau quả cầu — video Map_1 3:33–3:52 Simon đứng yên) — mọi nút đọc như thả.</summary>
    [NonSerialized] public bool InputLocked;
    public int InputX => NesPad.I != null && !InputLocked ? NesPad.I.Direction.x : 0;
    public bool Held(eNesButton b) => NesPad.I != null && !InputLocked && NesPad.I.Held(b);
    public bool Pressed(eNesButton b) => NesPad.I != null && !InputLocked && NesPad.I.Pressed(b);

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
        Fsm.Add(eSimonState.Attack, new SimonAttackState());
        Hurt = AddHurtbox(eHurtLayer.Player, new RectInt(-8, -30, 16, 30));
        WhipHit = AddHitbox(eHitLayer.PlayerAttack, new RectInt(0, 0, 0, 0));
        WhipHit.singleHit = true;
        if (view != null)
            defaultMaterial = view.sharedMaterial;
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
            for (int p = 0; p < 4; p++)
            {
                aThrow[p] = animSet.Get("throw_" + POSE_NAMES[p]);
                aWhip[1, 0, p] = animSet.Get("whip_l1_" + POSE_NAMES[p]);
                aWhip[2, 0, p] = animSet.Get("whip_l2_" + POSE_NAMES[p]);
                for (int c = 0; c < 4; c++)
                    aWhip[3, c, p] = animSet.Get("whip_l3c" + (c + 1) + "_" + POSE_NAMES[p]);
            }
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
        // CVM3 04: knockback = bảng như nhảy — lên KnockbackRise01..10, giữ đỉnh KnockbackApexTicks, xuống = gương (video Boss_Map_1/2/3)
        kbVx = Fixed.FromFloat(s.GetFloat(eSimonParam.KnockbackSpeedX));
        kbTicks = KB_RISE_TICKS * 2 + s.GetInt(eSimonParam.KnockbackApexTicks);
        kbDy = new int[kbTicks + 1];
        for (int i = 1; i <= KB_RISE_TICKS; i++)
        {
            int rise = s.GetInt((eSimonParam)((int)eSimonParam.KnockbackRise01 + i - 1));
            kbDy[i] = -rise;
            kbDy[kbTicks + 1 - i] = rise;
        }
        footHalf = s.GetInt(eSimonParam.FootProbeHalfWidth);
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
        LoadAttackTuning(cfg);
    }

    /// <summary>Số liệu tấn công (CVGP 09): nhịp 3 khung, màu roi cấp 3, hộp roi theo cấp, giá tim, độ cao sinh, giới hạn trên màn.</summary>
    private void LoadAttackTuning(GameConfig cfg)
    {
        var s = cfg.simon;
        atkWindup = s.GetInt(eSimonParam.WhipWindupTicks);
        atkRaise = s.GetInt(eSimonParam.WhipRaiseTicks);
        atkExtend = s.GetInt(eSimonParam.WhipExtendTicks);
        atkTotal = atkWindup + atkRaise + atkExtend;
        whipColorTicks = Mathf.Max(1, s.GetInt(eSimonParam.WhipColorTicks));
        subSpawnTick = s.GetInt(eSimonParam.SubWeaponSpawnTick);
        for (int lv = 1; lv <= 3; lv++)
        {
            var w = cfg.GetWhip(lv);
            if (w == null)
                continue;
            whipPiece[lv] = new RectInt(w.hitX.Get("Whip.hitX"), w.hitY.Get("Whip.hitY"), w.hitW.Get("Whip.hitW"), w.hitH.Get("Whip.hitH"));
            whipStrength[lv] = w.strength;
        }
        for (int i = 1; i < subCost.Length; i++)
        {
            var id = (eSubWeapon)i;
            var r = cfg.GetSubWeapon(id);
            subCost[i] = r != null ? r.heartCost : 99;
            subSpawnHeight[i] = Mathf.RoundToInt(SubWeaponProjectile.Param(cfg, ConfigActorParam.ActorKey(id), "spawnHeightAboveFeet", 24));
        }
        shotCapBase = cfg.rules.GetInt(eRuleParam.SubWeaponOnScreenBase, 1);
        shotCapDouble = cfg.rules.GetInt(eRuleParam.DoubleShotOnScreen, 2);
        shotCapTriple = cfg.rules.GetInt(eRuleParam.TripleShotOnScreen, 3);
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
        hazardDeath = false;
        invisibleLeft = invisibleAge = 0;
        InputLocked = false;
        flashFrom = 1;
        flashTo = 0;
        WhipHit.Deactivate();
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
        if (invisibleLeft > 0)
        {
            invisibleAge++;
            if (--invisibleLeft == 0 && AudioManager.I != null)
                AudioManager.I.PlaySfx(eSfx.InvisibleEnd);   // video Map_1 3:02.11 (SFX 28) đúng lúc Simon hiện lại
        }
    }

    // ---------------------------------------------------------------- combat

    public override bool OnHurt(ref HitInfo hit)
    {
        // tàng hình (CVGP 11, wiki Items#Invisibility Potion): đi xuyên enemy/đạn không bị thương (nước/hố vẫn chết — OnDeadlyTile)
        if (IsDying || Fsm.Is(eSimonState.Hurt) || invisibleLeft > 0)
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

    /// <summary>CVGP 12: hết giờ (TIME 0 — wiki Gameplay: mất 1 mạng) ⇒ Dead ngay (không knockback); Died(Empty). Giữa không trung: chết tại chỗ (§6).</summary>
    public void KillByTimeout()
    {
        if (IsDying)
            return;
        lastDeath = eTileCollision.Empty;
        Fsm.SetState(eSimonState.Dead);
    }

    /// <summary>
    /// CVM2 06: chạm gai bẫy ép (Crusher) — wiki Gameplay "hitting spikes will instantly kill you" ⇒ chết ngay tại chỗ (Dead, khung chết như
    /// hết máu — suy luận: video không có lần chạm), Died(Hazard). Như nước/hố: bỏ qua bất tử nháy + tàng hình (bên gọi kiểm cheat).
    /// </summary>
    public void KillByHazard()
    {
        if (IsDying)
            return;
        lastDeath = eTileCollision.Hazard;
        hazardDeath = true;
        Fsm.SetState(eSimonState.Dead);
    }

    /// <summary>CVM2 06: Dead hiện tại do gai (Died báo Hazard thay vì Empty).</summary>
    [NonSerialized] public bool hazardDeath;

    /// <summary>CVM2 06: đứng được trên bệ di động khi không ở thang / không đang chết / không đi xuyên địa hình qua cửa.</summary>
    public override bool RidesPlatforms => Fsm != null && !OnStairs && !IsDying && !(Fsm.Is(eSimonState.AutoWalk) && autoWalkNoClip);

    /// <summary>
    /// CVM3 04: dò sàn (đứng + đáp) = cột X−FootProbeHalfWidth..X+FootProbeHalfWidth — video 9 lần đi khỏi mép (Map_1 ×2, Map_2 ×7, cả 2 hướng):
    /// còn đứng khi thân qua mép 5 px, rơi ở 6 px (dò cả thân X−8..X+7 như trước thì rơi trễ 2 px). Va tường ngang vẫn dùng thân 16 px.
    /// </summary>
    public override void FootSpan(out int x0, out int x1)
    {
        if (footHalf <= 0)
        {
            base.FootSpan(out x0, out x1);
            return;
        }
        x0 = X - footHalf;
        x1 = X + footHalf;
    }

    internal void RaiseDyingStarted()
    {
        if (DyingStarted != null)
            DyingStarted(this);
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

    /// <summary>Kịch bản (cửa/cổng — routine 15): tự đi tới targetX với tốc độ speed (fixed/tick; 0 = WalkSpeed), bỏ qua input.
    /// noClip ⇒ không va địa hình (qua cửa: Simon đi sang phần section kế đang hiện cạnh — CVGP 15).</summary>
    public void StartAutoWalk(int targetX, int speed = 0, bool noClip = false)
    {
        autoWalkTarget = targetX;
        autoWalkDir = targetX >= X ? 1 : -1;
        autoWalkSpeed = speed > 0 ? speed : walkSpeed;
        autoWalkNoClip = noClip;
        Fsm.SetState(eSimonState.AutoWalk);
    }

    /// <summary>
    /// CVGP 15 — đổi tầng qua cầu thang cắt mép section (Crypt A ↔ B): đặt Simon lên thang <paramref name="p"/> tại tham số t px
    /// (đã nạp section đích + đọc lại Config như Teleport), giữ hướng đi (dir +1 lên / −1 xuống) ⇒ StairIdle (giữ nút thì đi tiếp ngay).
    /// </summary>
    public void PlaceOnStair(StairPath p, int tPx, int dir)
    {
        LoadTuning(World.Config);
        ResetMotion();
        stair = p;
        SetStairT(Fixed.FromPx(tPx));
        home = new Vector2Int(X, Y);
        stairMoveDir = dir >= 0 ? 1 : -1;
        Facing = stairMoveDir * p.upDx;
        ApplyStairPose();
        Fsm.SetState(eSimonState.StairIdle);
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

    /// <summary>
    /// CVM2 11: nhảy KHÔNG va trần — video Map_2 f3359–3366 (Hall B x 567–574): Simon nhảy từ dưới mái (ô đặc x 480–575, y 80–95) lên khối
    /// y 112, chân lên tới y 112 trong khi thân chồng mái (NES không chặn đầu); CVGP 08 chặn trần vì "không có video phản chứng".
    /// </summary>
    public const bool JUMP_HITS_CEILING = false;

    /// <summary>1 tick bay theo bảng nhảy: ngang airVx (đụng tường thì dừng), dọc jumpDy[jumpTick] (không chặn trần — JUMP_HITS_CEILING). Trả true khi chạm đất.</summary>
    public bool AirStep()
    {
        jumpTick++;
        if (airVx != 0)
        {
            if (jumpTick >= tuckStart && (jumpTick < untuck || jumpTuckAfterAttack))
            {
                // CVM2 11: đang co chân ⇒ va tường ngang theo hộp co chân (đỉnh = tuckHurt, đáy = chân) — video Map_2 f2945–2966 Hall B:
                // nhảy dưới mái (ô đặc y 80–95) cao đủ 36, thân đứng 32 px sẽ quét mái nhưng Simon vẫn đi ngang qua cột 9→10 không bị chặn
                var saved = body;
                body = new RectInt(body.x, tuckHurt.y, body.width, -tuckHurt.y);
                StepX(airVx);
                body = saved;
            }
            else
                StepX(airVx);
        }
        int dy = jumpTick <= jumpTicks ? jumpDy[jumpTick] : 0;
        if (dy != 0 && StepY(Fixed.FromPx(dy), JUMP_HITS_CEILING) && dy > 0)
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

    // ---------------------------------------------------------------- tấn công (CVGP 09)

    public SubWeaponProjectile PrefabOf(eSubWeapon w)
    {
        switch (w)
        {
            case eSubWeapon.Dagger: return daggerPrefab;
            case eSubWeapon.Axe: return axePrefab;
            case eSubWeapon.HolyWater: return holyWaterPrefab;
            case eSubWeapon.Cross: return crossPrefab;
            case eSubWeapon.Stopwatch: return stopwatchPrefab;
        }
        return null;
    }

#if UNITY_EDITOR
    public void SetWeaponPrefabs(SubWeaponProjectile dagger, SubWeaponProjectile axe, SubWeaponProjectile holyWater,
                                 SubWeaponProjectile cross, SubWeaponProjectile stopwatch)
    {
        daggerPrefab = dagger;
        axePrefab = axe;
        holyWaterPrefab = holyWater;
        crossPrefab = cross;
        stopwatchPrefab = stopwatch;
    }
#endif

    /// <summary>Số vũ khí phụ của Simon đang trên màn (tính vào giới hạn). stopwatch: đếm đồng hồ, không thì đếm vật ném.</summary>
    public int ActiveShots(bool stopwatch)
    {
        int n = 0;
        var list = World.GroupList(eEntityGroup.PlayerWeapon);
        for (int i = 0; i < list.Count; i++)
        {
            var p = list[i] as SubWeaponProjectile;
            if (p != null && p.Alive && p.CountsAsShot && p.Thrower == this && (p.Kind == eSubWeapon.Stopwatch) == stopwatch)
                n++;
        }
        return n;
    }

    /// <summary>Giới hạn trên màn: 1 / Double 2 / Triple 3 (ConfigRule); đồng hồ luôn 1 (wiki: multiplier không ảnh hưởng).</summary>
    public int ShotCap(eSubWeapon w)
    {
        if (w == eSubWeapon.Stopwatch)
            return shotCapBase;
        int m = GameSession.Current.shotMultiplier;
        return m >= 3 ? shotCapTriple : m == 2 ? shotCapDouble : shotCapBase;
    }

    /// <summary>Ném được không: có vũ khí phụ, đủ tim, chưa đủ số trên màn (wiki Gameplay: "chỉ dùng được khi đủ tim").</summary>
    public bool CanThrow()
    {
        var session = GameSession.Current;
        var w = session.subWeapon;
        if (w == eSubWeapon.None || PrefabOf(w) == null)
            return false;
        if (session.hearts < subCost[(int)w])
            return false;
        // CVM3 03: đồng hồ dùng lại được khi còn hiệu lực (−5 tim, tính lại từ đầu — video Map_2 f6928: 176–184 tick sau lần trước, Medusa vẫn đứng)
        if (w == eSubWeapon.Stopwatch)
            return true;
        return ActiveShots(false) < ShotCap(w);
    }

    /// <summary>
    /// Gọi từ CheckNextState của Idle/Walk/Crouch/Jump/Fall/StairIdle/StairUp/StairDown: B ⇒ roi; Up+B hoặc phím tắt SubWeapon
    /// ⇒ ném (không ném được — hết tim / đủ số trên màn / chưa có vũ khí ⇒ vung roi thay: SUY LUẬN, §5). Trả true nếu đã vào Attack.
    /// </summary>
    public bool TryAttack()
    {
        bool sub = Pressed(eNesButton.SubWeapon) || (Pressed(eNesButton.B) && Held(eNesButton.Up));
        if (!sub && !Pressed(eNesButton.B))
            return false;
        bool doThrow = sub && CanThrow();
        if (sub && !doThrow)
            throwFailCount++;
        StartAttack(doThrow);
        return true;
    }

    private void StartAttack(bool doThrow)
    {
        bool air = Fsm.Is(eSimonState.Jump) || Fsm.Is(eSimonState.Fall);
        bool stairs = OnStairs;
        atkAir = air;
        atkFalling = Fsm.Is(eSimonState.Fall) || (Fsm.Is(eSimonState.Jump) && jumpTick >= jumpTicks);
        atkFallTicks = Fsm.Is(eSimonState.Fall) ? fallTicks : 0;
        atkLanded = false;
        atkSpawned = false;
        if (stairs)
            atkPose = stairMoveDir >= 0 ? eAttackPose.StairsUp : eAttackPose.StairsDown;
        else if (!air && (Fsm.Is(eSimonState.Crouch) || Held(eNesButton.Down)))
            atkPose = eAttackPose.Duck;
        else
            atkPose = eAttackPose.Stand;
        // trên đất: bấm hướng cùng lúc ⇒ quay mặt theo hướng (suy luận)
        if (!air && !stairs && InputX != 0)
            Facing = InputX;
        atkThrow = doThrow;
        atkSub = GameSession.Current.subWeapon;
        atkLevel = Mathf.Clamp(GameSession.Current.whipLevel, 1, 3);
        if (doThrow)
        {
            var session = GameSession.Current;
            session.hearts -= subCost[(int)atkSub];
            session.NotifyChanged();
            throwCount++;
            // CVM3 03: đồng hồ có hiệu lực NGAY tick bấm (cùng tick trừ tim) và Simon vẫn VUNG ROI (video Map_2 f4829/f6837/f6928:
            // tim −5 cùng frame khung chuẩn bị roi, roi duỗi 6 frame sau; Medusa đứng từ f6837) ⇒ sinh đồng hồ ở đây, đòn = roi
            if (atkSub == eSubWeapon.Stopwatch)
            {
                atkSpawned = true;
                atkThrow = false;
                SpawnSubWeapon();
            }
        }
        else
            whipCount++;
        Fsm.SetState(eSimonState.Attack);
    }

    /// <summary>Hộp roi (quay TRÁI quanh pivot) của khung duỗi tư thế hiện tại: overlayOffsets[2] + bbox mảnh + phần vẽ thêm.</summary>
    public RectInt WhipRect(AnimSet.Anim a)
    {
        var piece = whipPiece[atkLevel];
        var off = a != null && a.overlayOffsets.Length > 2 ? a.overlayOffsets[2] : Vector2Int.zero;
        int dx = viewOffset.x + drawOffset.x;
        int x = off.x + piece.x + (Facing < 0 ? dx : -dx);
        int y = viewOffset.y + drawOffset.y + off.y + piece.y;
        return new RectInt(x, y, piece.width, piece.height);
    }

    /// <summary>Sinh vũ khí phụ tại pivot (tâm x), độ cao ConfigActorParam spawnHeightAboveFeet trên chân (+ phần vẽ thêm của tư thế).</summary>
    public SubWeaponProjectile SpawnSubWeapon()
    {
        var prefab = PrefabOf(atkSub);
        if (prefab == null)
            return null;
        int x = X + drawOffset.x;
        int y = Y + drawOffset.y - subSpawnHeight[(int)atkSub];
        var p = World.Spawn(prefab, x, y, Facing) as SubWeaponProjectile;
        if (p != null)
            p.Thrower = this;
        if (atkSub == eSubWeapon.Dagger && AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.ThrowDagger);
        return p;
    }

    /// <summary>Nhặt Morning Star (routine 11 gọi): roi lên 1 cấp (tối đa 3) + đóng băng cả thế giới WhipPickupFreezeTicks tick.</summary>
    public void UpgradeWhip()
    {
        var session = GameSession.Current;
        session.whipLevel = Mathf.Min(3, Mathf.Max(1, session.whipLevel) + 1);
        session.NotifyChanged();
        var s = World.Config != null ? World.Config.simon : null;
        if (s != null)
            World.FreezeWorld(s.GetInt(eSimonParam.WhipPickupFreezeTicks));
    }

    // ---------------------------------------------------------------- item (CVGP 11)

    /// <summary>Nháy 4 bảng màu khi nhặt Morning Star: tick fromTick..fromTick+ticks−1 (trùng đóng băng thế giới), mỗi
    /// ConfigRule.WhipPickupPaletteTicks tick đổi 1 bảng theo thứ tự thường→đỏ→tím→xanh (video Map_1 f193–217; thứ tự SUY LUẬN).</summary>
    public void StartPickupFlash(int fromTick, int ticks)
    {
        flashFrom = fromTick;
        flashTo = fromTick + ticks - 1;
        var cfg = World.Config;
        flashStep = cfg != null ? Mathf.Max(1, cfg.rules.GetInt(eRuleParam.WhipPickupPaletteTicks, 2)) : 2;
    }

    /// <summary>Uống bình tàng hình (ConfigItem.effectTicks): pha 1 vẽ thường (visibleTicks), pha 2 ẩn + lóe bảng màu white
    /// (glint*) — video Map_1 2:58.2–3:02.1; suốt thời gian không nhận đòn.</summary>
    public void StartInvisible(int ticks)
    {
        invisibleLeft = ticks;
        invisibleAge = 0;
        var cfg = World.Config;
        invisVisibleTicks = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.InvisibilityPotion, "visibleTicks", 84));
        invisGlintFrom = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.InvisibilityPotion, "glintFromTick", 160));
        invisGlintPeriod = Mathf.Max(1, Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.InvisibilityPotion, "glintPeriodTicks", 10)));
        invisGlintOn = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.InvisibilityPotion, "glintOnTicks", 3));
    }

    public bool IsInvisible => invisibleLeft > 0;
    public int InvisibleLeft => invisibleLeft;
    public bool IsPickupFlashing => World != null && World.TickCount >= flashFrom && World.TickCount <= flashTo;

    /// <summary>Bảng màu đang vẽ (null = gốc) — harness đọc.</summary>
    public AnimSet.Palette CurrentPalette { get; private set; }

    private AnimSet.Palette PaletteNow(out bool hide)
    {
        hide = false;
        if (IsPickupFlashing)
        {
            int k = ((World.TickCount - flashFrom) / flashStep) % 4;   // 0 thường, 1 đỏ, 2 tím, 3 xanh
            return k == 0 ? null : animSet.GetPalette(k == 1 ? "red" : k == 2 ? "purple" : "blue");
        }
        if (invisibleLeft > 0 && invisibleAge >= invisVisibleTicks)
        {
            bool glint = invisibleAge >= invisGlintFrom && (invisibleAge - invisGlintFrom) % invisGlintPeriod < invisGlintOn;
            hide = !glint;
            return glint ? animSet.GetPalette("white") : null;
        }
        return null;
    }

    /// <summary>Nhặt vũ khí phụ (routine 11 gọi): thay vũ khí; khác loại ⇒ mất multiplier (GameSession.SetSubWeapon).</summary>
    public void PickupSubWeapon(eSubWeapon w)
    {
        GameSession.Current.SetSubWeapon(w);
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
        bool hide;
        var pal = PaletteNow(out hide);
        if (hide)
            visible = false;
        Anim.Visible = visible;
        if (pal != CurrentPalette)
        {
            CurrentPalette = pal;
            var mat = World != null && World.Items != null ? World.Items.paletteMaterial : null;
            SpritePalette.Apply(view, pal, mat, defaultMaterial);
            SpritePalette.Apply(Anim.Overlay, pal, mat, defaultMaterial);
        }
    }
}
