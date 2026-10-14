using UnityEngine;

/// <summary>State Dracula dạng 2 (quỷ).</summary>
public enum eDemonState
{
    Emerge,  // vừa biến hình: đứng, chưa va chạm
    Stand,   // đứng quay về Simon
    Breath,  // há miệng phun lửa
    Crouch,  // ngồi lấy đà
    Jump,    // trên không (cao / thấp)
    Land,    // vừa đáp
}

/// <summary>
/// Dracula dạng 2 — quỷ (CVGP 20, theo wiki — GẦN ĐÚNG, không có video Block 6). Wiki Bosses#The Count + Stages 16-18#Stage 18: "không còn
/// dịch chuyển nên LUÔN đánh được"; "NHẢY quanh phòng (CAO hoặc THẤP), THỈNH THOẢNG PHUN LỬA"; "để khỏi bị dồn vào góc hãy chạy CHUI DƯỚI
/// khi nó nhảy cao"; "phải đánh vào ĐẦU", 32 đòn (gán đòn MẠNH; đòn yếu 48 = tỉ lệ 3:2 mọi boss — §4); "Holy Water KHÔNG gây sát thương
/// (chân không nhận đòn) nhưng làm nó CHOÁNG HOÀN TOÀN khi lửa còn cháy — trong lúc đó ĐI XUYÊN qua nó không bị thương"; Boomerang hất
/// được cầu lửa; Stopwatch vô dụng; 50 000 đ (Bosses — mâu thuẫn Gameplay, §4); "tan rã rồi nhặt quả cầu cuối và kết thúc game".
/// Sprite: 7 khung 48 rộng = thân trên A (đứng: 0, 1, 2, 5) / B (giơ tay: 3, 4, 6) + chân (0 đứng, 2 bước, 5 ngồi, 3/6 buông, 4 co) —
/// gán khung theo hình (SUY LUẬN): đứng 0, phun 1, ngồi 5, lên 3, xuống 6, đáp 4. Ước lượng (ConfigActorParam Boss.Dracula.p2* — §2q):
///   Emerge  p2EmergeTicks (lửa biến hình còn cháy), không va chạm ⇒ Activate.
///   Stand   p2StandMin..+Span quay về Simon ⇒ p2FirePct % Breath, còn lại Crouch.
///   Breath  khung 1; tick p2FireWindupTicks bắn fireballsPerVolley cầu lửa từ miệng (chân − p2FireAbovePx, trước p2MouthDx) như dạng 1;
///           giữ tới p2FireHoldTicks ⇒ Stand.
///   Crouch  khung 5, p2CrouchTicks ⇒ Jump: p2HighPct % nhảy CAO (p2HighVy0, p2HighVx) — còn lại THẤP (p2LowVy0, p2LowVx); trọng lực
///           p2Gravity; hướng về Simon (sát mép màn quay ra giữa); ngang dừng ở [cx + p2MinScreenX, cx + p2MaxScreenX]; đáp sàn cột pivot.
///   Land    khung 4, p2LandTicks ⇒ Stand.
/// Hộp nhận đòn = MẶT (khung A: cột 0–23 hàng 0–31; khung B: cột 4–31 hàng 16–47 — theo hình) đổi theo khung mỗi tick; hộp chạm = cả
/// sprite. Vùng dò nước thánh = cả sprite (lửa dưới chân); lửa/lọ KHÔNG trừ máu; đang bị lửa giữ ⇒ tắt hộp chạm.
/// Chết: chuỗi chết boss + thêm lửa phủ thân; quả cầu đánh dấu CUỐI ⇒ nhặt ⇒ GameRules.OnGameCleared.
/// </summary>
public class DraculaPhase2 : BossBase
{
    [SerializeField] private DraculaFireball fireballPrefab;

    public StateMachine<DraculaPhase2, eDemonState> Fsm { get; private set; }

    // khung p2_all (thứ tự sheet dracula2_0..6) + chiều cao sprite
    public const int F_STAND = 0, F_BREATH = 1, F_STEP = 2, F_RISE = 3, F_LAND = 4, F_CROUCH = 5, F_FALL = 6;
    private static readonly int[] FRAME_H = { 64, 64, 64, 80, 64, 48, 80 };
    private static readonly bool[] UPPER_B = { false, false, false, true, true, false, true };

    private AnimSet.Anim aAll;
    [System.NonSerialized] public int emergeTicks, standMin, standSpan, firePct, fireWindup, fireHold, fireAbove, mouthDx, aimAbove, volley, fireSpeed;
    [System.NonSerialized] public int spreadCos, spreadSin, crouchTicks, landTicks, highPct, highVy0, highVx, lowVy0, lowVx, gravity, minSX, maxSX;
    [System.NonSerialized] public int standLen, jumpVx, frame, holyStunLeft;
    [System.NonSerialized] public bool jumpHigh;
    /// <summary>Harness: số cú nhảy cao / thấp, loạt phun, cầu lửa, đỉnh cú nhảy gần nhất (y nhỏ nhất), lần bị lửa giữ (tick).</summary>
    [System.NonSerialized] public int highJumps, lowJumps, volleys, shots, jumpTopY, holyTicks;

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState + (Fsm.Is(eDemonState.Jump) ? (jumpHigh ? "H" : "L") : "")) : "-";

    abstract class DemonState : BaseState<DraculaPhase2>
    {
        protected void SetState(eDemonState s) { owner.Fsm.SetState(s); }
    }

    class EmergeState : DemonState
    {
        public override void OnEnterState() { owner.Pose(F_STAND); }

        public override void CheckNextState()
        {
            if (StateFrame < owner.emergeTicks)
                return;
            owner.Activate();
            SetState(eDemonState.Stand);
        }
    }

    class StandState : DemonState
    {
        public override void OnEnterState()
        {
            owner.Pose(F_STAND);
            owner.standLen = owner.Roll(owner.standMin, owner.standSpan);
        }

        public override void OnExecuteState() { owner.Facing = owner.FacingToPlayer(); }

        public override void CheckNextState()
        {
            if (StateFrame < owner.standLen)
                return;
            SetState(owner.World.Rng.Next(100) < owner.firePct ? eDemonState.Breath : eDemonState.Crouch);
        }
    }

    class BreathState : DemonState
    {
        public override void OnEnterState() { owner.Pose(F_BREATH); }

        public override void OnExecuteState()
        {
            if (StateFrame == owner.fireWindup)
                owner.Fire();
        }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.fireHold)
                SetState(eDemonState.Stand);
        }
    }

    class CrouchState : DemonState
    {
        public override void OnEnterState() { owner.Pose(F_CROUCH); }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.crouchTicks)
                owner.StartJump();
        }
    }

    class JumpState : DemonState
    {
        public override void OnExecuteState()
        {
            int nx = owner.fx + owner.Facing * owner.jumpVx;
            int lo = Fixed.FromPx(owner.CamX + owner.minSX), hi = Fixed.FromPx(owner.CamX + owner.maxSX);
            nx = Mathf.Clamp(nx, lo, hi);
            // chân (cột pivot, 8 px trên chân) đụng khối đặc (bậc) ⇒ mất đà ngang (như Igor đụng tường) — không lọt vào trong khối
            var tiles = owner.World.Tiles;
            if (tiles == null || !tiles.ColumnHitsSolid(Fixed.ToPx(nx), owner.Y - 8, owner.Y - 1))
                owner.fx = nx;
            owner.vy += owner.gravity;
            bool landed = owner.StepFall(owner.vy);   // lên: xuyên mọi thứ; xuống: đáp mặt sàn cột pivot (như Igor 19)
            owner.jumpTopY = Mathf.Min(owner.jumpTopY, owner.Y);
            owner.Pose(owner.vy < 0 ? F_RISE : F_FALL);
            if (landed)
            {
                owner.vy = 0;
                SetState(eDemonState.Land);
            }
        }
    }

    class LandState : DemonState
    {
        public override void OnEnterState() { owner.Pose(F_LAND); }

        public override void CheckNextState()
        {
            if (StateFrame >= owner.landTicks)
                SetState(eDemonState.Stand);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<DraculaPhase2, eDemonState>(this, "DraculaPhase2");
        Fsm.Add(eDemonState.Emerge, new EmergeState());
        Fsm.Add(eDemonState.Stand, new StandState());
        Fsm.Add(eDemonState.Breath, new BreathState());
        Fsm.Add(eDemonState.Crouch, new CrouchState());
        Fsm.Add(eDemonState.Jump, new JumpState());
        Fsm.Add(eDemonState.Land, new LandState());
        if (animSet != null)
            aAll = animSet.Get("p2_all");
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        emergeTicks = PInt(cfg, "p2EmergeTicks", 48);
        standMin = PInt(cfg, "p2StandMinTicks", 32);
        standSpan = PInt(cfg, "p2StandSpanTicks", 32);
        firePct = PInt(cfg, "p2FirePct", 33);
        fireWindup = PInt(cfg, "p2FireWindupTicks", 16);
        fireHold = PInt(cfg, "p2FireHoldTicks", 40);
        fireAbove = PInt(cfg, "p2FireAbovePx", 39);
        mouthDx = PInt(cfg, "p2MouthDx", 8);
        aimAbove = PInt(cfg, "aimAbovePx", 16);
        volley = PInt(cfg, "fireballsPerVolley", 3);
        fireSpeed = PFixed(cfg, "fireballSpeed", 1.5f);
        float deg = PInt(cfg, "fireSpreadDeg", 14);
        spreadCos = Mathf.RoundToInt(Mathf.Cos(deg * Mathf.Deg2Rad) * 256f);
        spreadSin = Mathf.RoundToInt(Mathf.Sin(deg * Mathf.Deg2Rad) * 256f);
        crouchTicks = PInt(cfg, "p2CrouchTicks", 12);
        landTicks = PInt(cfg, "p2LandTicks", 12);
        highPct = PInt(cfg, "p2HighPct", 50);
        highVy0 = PFixed(cfg, "p2HighVy0", -5.5f);
        highVx = PFixed(cfg, "p2HighVx", 2f);
        lowVy0 = PFixed(cfg, "p2LowVy0", -3f);
        lowVx = PFixed(cfg, "p2LowVx", 1.5f);
        gravity = PFixed(cfg, "p2Gravity", 0.25f);
        minSX = PInt(cfg, "p2MinScreenX", 32);
        maxSX = PInt(cfg, "p2MaxScreenX", 224);
        standLen = jumpVx = holyStunLeft = 0;
        jumpHigh = false;
        highJumps = lowJumps = volleys = shots = holyTicks = 0;
        jumpTopY = Y;
        vy = 0;
        frame = F_STAND;
        Fsm.SetState(eDemonState.Emerge);
        UpdateBoxes();
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    public int CamX => World.CameraCtl != null ? World.CameraCtl.X : 0;

    private void StartJump()
    {
        var p = World.Player;
        int dir = p != null && p.X != X ? (p.X > X ? 1 : -1) : Facing;
        // sát mép màn mà Simon ở phía mép ⇒ nhảy ra giữa (không đâm vào tường)
        if ((dir < 0 && X <= CamX + minSX + 8) || (dir > 0 && X >= CamX + maxSX - 8))
            dir = -dir;
        Facing = dir;
        jumpHigh = World.Rng.Next(100) < highPct;
        if (jumpHigh) highJumps++; else lowJumps++;
        vy = jumpHigh ? highVy0 : lowVy0;
        jumpVx = jumpHigh ? highVx : lowVx;
        jumpTopY = Y;
        Fsm.SetState(eDemonState.Jump);
    }

    private void Fire()
    {
        if (fireballPrefab == null)
            return;
        volleys++;
        var p = World.Player;
        int ox = X + Facing * mouthDx, oy = Y - fireAbove;
        long dx = p != null ? p.X - ox : Facing * 64, dy = p != null ? (p.Y - aimAbove) - oy : 0;
        long len = DraculaPhase1.ISqrt((dx * dx + dy * dy) << 16);
        if (len == 0)
        {
            dx = Facing;
            dy = 0;
            len = 256;
        }
        long cvx = fireSpeed * dx * 256 / len, cvy = fireSpeed * dy * 256 / len;
        for (int i = 0; i < volley; i++)
        {
            int k = i - volley / 2;
            long vx1 = cvx, vy1 = cvy;
            for (int r = 0; r < Mathf.Abs(k); r++)
            {
                long s = k > 0 ? spreadSin : -spreadSin;
                long nx = (vx1 * spreadCos - vy1 * s) / 256, ny = (vx1 * s + vy1 * spreadCos) / 256;
                vx1 = nx;
                vy1 = ny;
            }
            var f = World.Spawn(fireballPrefab, ox, oy, vx1 >= 0 ? 1 : -1) as DraculaFireball;
            if (f == null)
                continue;
            f.vx = (int)vx1;
            f.vy = (int)vy1;
            shots++;
        }
    }

    private void Pose(int f)
    {
        frame = f;
        if (Anim != null && aAll != null)
            Anim.SetFrame(aAll, f);
        UpdateBoxes();
    }

    /// <summary>Hộp mặt (nhận đòn) + hộp thân (chạm) theo khung đang vẽ — quy ước quay TRÁI, pivot đáy-giữa.</summary>
    private void UpdateBoxes()
    {
        int h = FRAME_H[frame];
        Hurt.local = UPPER_B[frame] ? new RectInt(-20, 16 - h, 28, 32) : new RectInt(-24, -h, 24, 32);
        Touch.local = new RectInt(-24, -h, 48, h);
    }

    /// <summary>Đầu khung hiện tại (local) — harness kiểm hộp.</summary>
    public RectInt HeadLocal => Hurt.local;

    /// <summary>Lửa dưới CHÂN làm choáng (wiki) ⇒ dò cả thân, không chỉ đầu.</summary>
    protected override RectInt HolyWaterProbe => Touch.SectionRect;

    public override void OnTick()
    {
        base.OnTick();
        if (holyTouch)
        {
            holyStunLeft = hitStunTicks;
            holyTicks++;
        }
        else if (holyStunLeft > 0)
            holyStunLeft--;
        ApplyVisibility();
    }

    protected override void ApplyVisibility()
    {
        base.ApplyVisibility();
        if (holyStunLeft > 0)
            Touch.active = false;   // "đi xuyên qua nó không bị thương khi bị lửa giữ" (wiki)
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        var w = hit.attacker as SubWeaponProjectile;
        if ((w != null && w.Kind == eSubWeapon.HolyWater) || HolyFire.IsHolyFire(hit.hitbox))
            return false;   // "Holy Water không gây sát thương" (wiki) — chỉ làm choáng (BossBase.TickStates)
        return base.OnHurt(ref hit);
    }

    protected override void Die()
    {
        base.Die();
        // "tan rã" — thân 48×64 lớn hơn dơi: thêm 2 cụm lửa phủ nửa dưới (SUY LUẬN, dùng FX bossdeath_m có sẵn)
        FxSprite.Spawn(World, "bossdeath_m", X - 12, Y - 12, deathFxTicks);
        FxSprite.Spawn(World, "bossdeath_m", X + 12, Y - 12, deathFxTicks);
    }

    protected override void OnCrystalSpawned(ItemPickup crystal)
    {
        crystal.finalCrystal = true;
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] Dracula: quả cầu CUỐI — nhặt ⇒ GameRules.OnGameCleared");
    }
}
