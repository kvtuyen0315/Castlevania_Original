using UnityEngine;

/// <summary>State Mummy Man (boss Block 3 — 2 thực thể).</summary>
public enum eMummyState
{
    Dormant, // đứng chờ (không va chạm) tới khi Simon lại gần
    Walk,    // lang thang / kẹp Simon (gồm đứng yên giữa 2 lần quyết định)
    Throw,   // dừng lại ném băng vải
}

/// <summary>
/// Mummy Men (CVGP 19, theo wiki — GẦN ĐÚNG, không có video Block 3). Wiki Bosses#Mummy Men + Stages 7-9#Stage 9: 2 Mummy "lang thang trong
/// phòng, thỉnh thoảng ném băng gạc vào Simon, cố KẸP Simon từ hai phía"; Stopwatch vô dụng; Holy Water "hoá đá" chúng tại chỗ; "đánh tới
/// khi cả hai bị tiêu diệt"; 48/32 đòn (wiki không nói tổng hay mỗi con — ConfigBoss.hitsScope TotalAllEntities ⇒ mỗi con 24/16, CVGP
/// 04 §4), 3000 đ khi hạ con cuối; Block 3 "boss chậm nhưng khoẻ". Cùng 1 lớp cho 2 con: BossArena sinh con ĐẦU (tím) tại bossPosition,
/// con đầu sinh con THỨ HAI (đỏ — sheet có 2 bộ màu, gán mỗi con 1 màu là SUY LUẬN) cách partnerDxPx về phía trái. Ước lượng (ConfigActorParam
/// Boss.MummyMan — ghi §2p):
///   Dormant  khung đứng; |Simon − con bất kỳ| &lt; wakeDistancePx ⇒ CẢ HAI Activate.
///   Walk     mỗi decideMin..+span tick quyết định: con đang ở BÊN PHẢI con kia nhắm điểm Simon + keepDxPx, con bên trái nhắm Simon −
///            keepDxPx (kẹp 2 phía); wanderPct % chọn hướng ngẫu nhiên thay vì nhắm (lang thang); tới đích (≤ standDxPx) ⇒ đứng. Đi
///            moveSpeed (ConfigBoss) trên sàn (dò sàn/tường như enemy 13 — khối/tường chặn ⇒ đứng tới lần quyết định sau).
///   Throw    hết hồi ném (đầu throwFirstTicks, sau throwMin..+span) và |dx| ≤ throwRangePx ⇒ đứng quay về Simon throwPoseTicks tick rồi
///            ném 1 MummyBandage ngang bandageSpeed ở chân − bandageAbovePx (trên đầu Simon NGỒI — wiki "ngồi quất khi chúng áp sát").
/// Máu: mỗi con CombatHealth riêng (24/16); thanh ENEMY = máu 2 con cộng lại. Con đầu hết máu (con kia còn) ⇒ lửa bossdeath + biến mất (không
/// điểm); con cuối ⇒ chuỗi chết boss (3000 đ, quả cầu). Khựng khi trúng / chạm nước thánh: BossBase.
/// </summary>
public class MummyMan : BossBase
{
    [SerializeField] private MummyBandage bandagePrefab;

    public StateMachine<MummyMan, eMummyState> Fsm { get; private set; }

    private AnimSet.Anim aPurple, aRed;
    [System.NonSerialized] public MummyMan partner;
    private int partnerSpawnTick;
    /// <summary>Con thứ hai (đỏ) — do con đầu sinh.</summary>
    [System.NonSerialized] public bool second;
    [System.NonSerialized] public int wakeDist, partnerDx, speed, decideMin, decideSpan, keepDx, wanderPct, standDx;
    [System.NonSerialized] public int throwFirst, throwMin, throwSpan, throwRange, poseTicks, bandageAbove, bandageSpeed, bandageFront;
    [System.NonSerialized] public int decideTimer, throwTimer, walkDir, targetX;
    /// <summary>Số băng đã ném / số lần quyết định (harness).</summary>
    [System.NonSerialized] public int throws, decisions;
    private static bool spawningPartner;

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) : "-";

    abstract class MummyState : BaseState<MummyMan>
    {
        protected void SetState(eMummyState s) { owner.Fsm.SetState(s); }
    }

    class DormantState : MummyState
    {
        public override void OnEnterState() { owner.Pose(0); }

        public override void CheckNextState()
        {
            if (owner.second)
                return;   // con đầu đánh thức cả hai
            var p = owner.World.Player;
            if (p == null || !p.Alive)
                return;
            var o = owner.PartnerAlive() ? owner.partner : null;
            if (Mathf.Abs(p.X - owner.X) < owner.wakeDist || (o != null && Mathf.Abs(p.X - o.X) < owner.wakeDist))
            {
                owner.Wake();
                if (o != null)
                    o.Wake();
            }
        }
    }

    class WalkState : MummyState
    {
        public override void OnEnterState() { owner.walkDir = 0; owner.PlayWalk(false); }

        public override void OnExecuteState()
        {
            if (--owner.decideTimer <= 0)
                owner.Decide();
            if (owner.walkDir != 0)
            {
                owner.Facing = owner.walkDir;
                bool blocked = owner.StepX(owner.walkDir * owner.speed);
                bool arrived = owner.targetX != int.MinValue && Mathf.Abs(owner.X - owner.targetX) <= owner.standDx;
                if (blocked || arrived)
                    owner.walkDir = 0;
            }
            if (owner.walkDir == 0)
                owner.Facing = owner.FacingToPlayer();
            owner.PlayWalk(owner.walkDir != 0);
            owner.Fall();
            owner.throwTimer--;
        }

        public override void CheckNextState()
        {
            var p = owner.World.Player;
            if (owner.throwTimer <= 0 && p != null && Mathf.Abs(p.X - owner.X) <= owner.throwRange && owner.Grounded())
                SetState(eMummyState.Throw);
        }
    }

    class ThrowState : MummyState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
            owner.Pose(0);
        }

        public override void CheckNextState()
        {
            if (StateFrame < owner.poseTicks)
                return;
            owner.ThrowBandage();
            owner.throwTimer = owner.Roll(owner.throwMin, owner.throwSpan);
            owner.decideTimer = 0;
            SetState(eMummyState.Walk);
        }
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<MummyMan, eMummyState>(this, "MummyMan");
        Fsm.Add(eMummyState.Dormant, new DormantState());
        Fsm.Add(eMummyState.Walk, new WalkState());
        Fsm.Add(eMummyState.Throw, new ThrowState());
        if (animSet != null)
        {
            aPurple = animSet.Get("walk_purple");
            aRed = animSet.Get("walk_red");
        }
    }

    /// <summary>Máu mỗi con = tổng wiki / số thực thể (ConfigBoss hitsScope TotalAllEntities, entityCount).</summary>
    protected override void ResetHealth(GameConfig cfg)
    {
        base.ResetHealth(cfg);
        partner = null;   // thực thể tái dùng từ pool: BossBase.LoadTuning vẽ thanh ENEMY trước LoadBoss
        var row = cfg != null ? cfg.GetBoss(bossId, phase) : null;
        int n = row != null && row.hitsScope == eHitsScope.TotalAllEntities ? Mathf.Max(1, row.entityCount) : 1;
        Health.Reset(Health.HitsWeak / n, Health.HitsStrong / n);
    }

    protected override void LoadBoss(GameConfig cfg)
    {
        wakeDist = PInt(cfg, "wakeDistancePx", 128);
        partnerDx = PInt(cfg, "partnerDxPx", 40);
        speed = BossMoveSpeed(cfg, 0.375f);
        decideMin = PInt(cfg, "decideMinTicks", 48);
        decideSpan = PInt(cfg, "decideSpanTicks", 64);
        keepDx = PInt(cfg, "keepDxPx", 40);
        wanderPct = PInt(cfg, "wanderPct", 25);
        standDx = PInt(cfg, "standDxPx", 4);
        throwFirst = PInt(cfg, "throwFirstTicks", 64);
        throwMin = PInt(cfg, "throwMinTicks", 128);
        throwSpan = PInt(cfg, "throwSpanTicks", 96);
        throwRange = PInt(cfg, "throwRangePx", 160);
        poseTicks = PInt(cfg, "throwPoseTicks", 16);
        bandageAbove = PInt(cfg, "bandageAbovePx", 28);
        bandageSpeed = PFixed(cfg, "bandageSpeed", 1f);
        bandageFront = PInt(cfg, "bandageFrontPx", 8);
        decideTimer = 0;
        throwTimer = throwFirst;   // con thứ hai: + throwMin/2 (đặt sau khi biết second) — 2 con không ném cùng tick
        walkDir = 0;
        targetX = int.MinValue;
        throws = decisions = 0;
        partner = null;
        second = spawningPartner;
        if (second)
            throwTimer = throwFirst + throwMin / 2;
        Facing = -1;
        Fsm.SetState(eMummyState.Dormant);
        if (!second)
        {
            // con thứ hai: cùng prefab, cùng hàng sàn, cách partnerDx về bên trái (con đầu ở bossPosition)
            spawningPartner = true;
            try
            {
                var src = SourcePrefab != null ? SourcePrefab : this;
                partner = World.Spawn(src, X - partnerDx, Y, -1) as MummyMan;
            }
            finally
            {
                spawningPartner = false;
            }
            if (partner != null)
            {
                partner.partner = this;
                partner.partnerSpawnTick = SpawnTick;
                partnerSpawnTick = partner.SpawnTick;
                partner.arena = arena;
                partner.Pose(0);
            }
        }
        Pose(0);
    }

    protected override void TickBoss()
    {
        Fsm.Tick();
    }

    protected override void OnStunEnd()
    {
        PlayWalk(Fsm.Is(eMummyState.Walk) && walkDir != 0);
    }

    private bool PartnerAlive()
    {
        return partner != null && partner.Alive && partner.SpawnTick == partnerSpawnTick && !partner.defeated;
    }

    private void Wake()
    {
        if (!dormant)
            return;
        Activate();
        decideTimer = 0;
        if (!second)
            StartBossMusic();
        Fsm.SetState(eMummyState.Walk);
    }

    private AnimSet.Anim WalkAnim => second ? aRed : aPurple;

    private void Pose(int frame)
    {
        if (Anim != null && WalkAnim != null)
            Anim.SetFrame(WalkAnim, frame);
    }

    /// <summary>Đi ⇒ chạy anim walk (khung 8 tick — ước lượng 05); đứng ⇒ giữ khung 0.</summary>
    private void PlayWalk(bool moving)
    {
        if (Anim == null || WalkAnim == null)
            return;
        if (moving)
        {
            if (!Anim.IsPlaying(WalkAnim) || Anim.Done)
                Anim.Play(WalkAnim, true);
        }
        else if (!Anim.IsPlaying(WalkAnim) || !Anim.Done || Anim.FrameIndex != 0)
            Anim.SetFrame(WalkAnim, 0);
    }

    /// <summary>Quyết định hướng đi (kẹp 2 phía / lang thang).</summary>
    private void Decide()
    {
        decisions++;
        decideTimer = Roll(decideMin, decideSpan);
        var p = World.Player;
        if (p == null)
        {
            walkDir = 0;
            return;
        }
        if (World.Rng.Next(100) < wanderPct)
        {
            walkDir = World.Rng.Next(2) == 0 ? -1 : 1;
            targetX = int.MinValue;
            return;
        }
        // bên của mình: con đang ở bên PHẢI con kia giữ phía phải Simon (một mình ⇒ phía mình đang đứng so với Simon)
        int side;
        if (PartnerAlive() && partner.X != X)
            side = X > partner.X ? 1 : -1;
        else
            side = X >= p.X ? 1 : -1;
        targetX = p.X + side * keepDx;
        int d = targetX - X;
        walkDir = Mathf.Abs(d) <= standDx ? 0 : (d > 0 ? 1 : -1);
    }

    private void Fall()
    {
        if (Grounded())
        {
            vy = 0;
            return;
        }
        vy += Fixed.FromFloat(0.25f);
        if (StepFall(vy))
            vy = 0;
    }

    private void ThrowBandage()
    {
        if (bandagePrefab == null)
            return;
        throws++;
        var b = World.Spawn(bandagePrefab, X + Facing * bandageFront, Y - bandageAbove, Facing);
        if (b != null)
            b.vx = Facing * bandageSpeed;
    }

    /// <summary>Thanh ENEMY = máu còn của cả hai con (hết máu tính 0).</summary>
    protected override int BarValue()
    {
        int cur = Health.Current, max = Health.Max;
        if (partner != null)
        {
            cur += partner.Health.Current;
            max += partner.Health.Max;
        }
        return max > 0 ? (Mathf.Max(0, cur) * GameSession.MAX_HP + max - 1) / max : 0;
    }

    /// <summary>Con đầu bị hạ khi con kia còn ⇒ lửa + biến mất (không điểm); con cuối ⇒ chuỗi chết boss.</summary>
    protected override void Die()
    {
        if (PartnerAlive())
        {
            defeated = true;
            hidden = true;
            stunTicks = 0;
            FxSprite.Spawn(World, "bossdeath_m", X, Y - deathFxUp, deathFxTicks);
            if (CvgpDebug.LogState)
                CvgpDebug.Log("[Boss] MummyMan " + (second ? "đỏ" : "tím") + " hạ — còn 1 con");
            partner.RefreshBar();
            Kill();
            RaiseDefeated();
            return;
        }
        base.Die();
    }
}
