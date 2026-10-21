using UnityEngine;

/// <summary>State Mummy Man (boss Block 3 — 2 thực thể).</summary>
public enum eMummyState
{
    Dormant, // đứng chờ khung 0 quay trái (không va chạm) tới khi camera khoá + wakeDelayTicks
    Walk,    // đi theo hướng đang chọn (kể cả đi tại chỗ ở mép màn); anim chạy liên tục
    Throw,   // đứng quay về Simon, nháy đỏ/tím rồi ném 1 băng gạc
}

/// <summary>
/// Mummy Men (CVGP 19 theo wiki; CVM3 09 đo lại 2 video — Map_3 3:06–3:54 bảng màu chuẩn, Boss_Map_3 0–42.7 s chỉ chuyển động; số đo
/// CVM3_MEMORY §2i, Tools/cvm3/boss9m_*.py, ClaudeNotes/cvm3/ref/data/boss9m/model.txt). Cùng 1 lớp cho 2 con: BossArena sinh con ĐẦU (phải,
/// video x 1503) ở bossPosition, con đầu sinh con THỨ HAI cách partnerDxPx về trái (video x 1312). Cả hai TÍM (sprite đỏ = khung nháy lúc ném).
///   Dormant  đứng khung 0 quay trái, thấy được, không va chạm. Con đầu đếm tick từ khi camera KHOÁ: musicDelayTicks ⇒ nhạc boss;
///            wakeDelayTicks ⇒ CẢ HAI thức (video: khoá + 120 ở cả 2 video, không theo khoảng cách Simon), quay + đi VỀ PHÍA Simon.
///   Walk     0.75 px/tick (ConfigBoss.moveSpeed); quyết định mỗi decideTicks tick (lần đầu decideFirstTicks sau khi thức — lưới 16 tick
///            tính từ lúc khoá; lúc đang ném thì bỏ lượt): Simon QUAY LƯNG về Mummy ⇒ đi TỚI; Simon nhìn về Mummy ⇒ theo |dx|: &lt; nearPx ⇒ đi XA,
///            ≥ farPx ⇒ đi TỚI, giữa ⇒ giữ hướng (video Map_3 Simon đứng yên quay phải trên khối: quay xa ở |dx| 33–47, quay tới ở 55–68; con
///            sau lưng Simon f6027 quay TỚI ở |dx| 13; video boss: con kẹt mép phải ~140 tick ở |dx| 48–53 không quay). Mép màn [cx+screenMinX,
///            cx+screenMaxX] chặn x nhưng vẫn "đi tại chỗ" (video boss: con thứ 2 kẹt mép phải ~140 tick, anim chạy, quay lưng Simon).
///            Anim walk [0,1,2,1] × animStepTicks chạy liên tục cả lúc đứng ném / đi tại chỗ; dừng khi khựng (lửa nước thánh).
///   Throw    hết throwTimer (chỉ đếm khi Walk: đầu throwFirstTicks / con 2 throwFirstPartnerTicks, sau mỗi lần ném throwEveryTicks; từ lần
///            thứ 2 chỉ ném với xác suất throwChancePct, trượt ⇒ chờ thêm throwRetryTicks — video: khoảng ném ≈ bội của ~113 tick): đứng
///            quay về Simon throwPoseTicks tick, nháy đỏ↔tím mỗi flashTicks tick tới throwReleaseTicks thì ném 1 MummyBandage (x = X Mummy,
///            tâm Y − bandageAboveHighPx hoặc − bandageAboveLowPx ngẫu nhiên), rồi đi tiếp hướng mặt.
/// Máu: mỗi con CombatHealth riêng (ConfigBoss TotalAllEntities / 2); thanh ENEMY = máu 2 con cộng lại (video: 16 vạch chung); bất tử sau đòn
/// (invincibleTicks 32) DÙNG CHUNG cho cả cặp (video: 0 lần −2 vạch cùng lúc dù 2 con cùng đứng trong lửa). Trúng roi
/// không khựng (hitStunTicks 0); lửa/lọ nước thánh ⇒ đứng yên suốt lúc chạm + holyStunTicks (BossBase). MỖI con hết máu ⇒ +3000 + lửa
/// bossdeath (video Map_3 3:42.87 / 3:45.70); con cuối ⇒ chuỗi chết boss (quả cầu sau crystalDelayTicks, đứng crystalHoverTicks rồi rơi).
/// Stopwatch: miễn (ConfigBoss.stopwatchAffects 0 — wiki). Harness: forcedHeights ("H"/"L" theo thứ tự ném), forcedThrowFirst.
/// </summary>
public class MummyMan : BossBase
{
    [SerializeField] private MummyBandage bandagePrefab;

    public StateMachine<MummyMan, eMummyState> Fsm { get; private set; }

    private AnimSet.Anim aPurple, aRed;
    [System.NonSerialized] public MummyMan partner;
    private int partnerSpawnTick;
    /// <summary>Con thứ hai (trái) — do con đầu sinh.</summary>
    [System.NonSerialized] public bool second;
    [System.NonSerialized] public int partnerDx, speed, wakeDelay, musicDelay, decideTicks, decideFirst, nearPx, farPx, animStep;
    [System.NonSerialized] public int throwFirst, throwFirstPartner, throwEvery, throwChance, throwRetry, poseTicks, releaseTick, flashTicks;
    [System.NonSerialized] public int aboveHigh, aboveLow, bandageSpeed, waveAmp, wavePeriod, bandageInset, screenMin, screenMax;
    /// <summary>Trạng thái: tick từ khi camera khoá (con đầu), hướng đi, hồi ném, đồng hồ anim, hồi quyết định, đang nháy đỏ.</summary>
    [System.NonSerialized] public int lockTicks, walkDir, throwTimer, animTick, decideTimer;
    [System.NonSerialized] public bool flashRed;
    /// <summary>Tick thế giới lúc bật nhạc boss (−1 = chưa) — harness.</summary>
    [System.NonSerialized] public int musicAtTick = -1;
    /// <summary>Đếm (harness): số băng đã ném, số lần quyết định, số lần đổi hướng.</summary>
    [System.NonSerialized] public int throws, decisions, turns;
    /// <summary>Harness (CVM3 09 so video): chuỗi độ cao băng "H"/"L" theo thứ tự ném (hết ⇒ RNG); hồi ném đầu ép (−1 ⇒ Config).</summary>
    [System.NonSerialized] public string forcedHeights;
    [System.NonSerialized] public int forcedThrowFirst = -1;
    /// <summary>Harness (CVM3 09 so quỹ đạo video): tắt quyết định hướng + tự ném — probe ép ForceTurn / ForceThrow theo mốc video.</summary>
    [System.NonSerialized] public bool harnessPlan;
    private static bool spawningPartner;

    public override string DebugState => Fsm != null && Fsm.HasState ? (dying ? "Dying" : Stunned ? "Stun" : Fsm.CurState.ToString()) : "-";

    abstract class MummyState : BaseState<MummyMan>
    {
        protected void SetState(eMummyState s) { owner.Fsm.SetState(s); }
    }

    class DormantState : MummyState
    {
        public override void OnEnterState() { owner.flashRed = false; owner.Show(); }

        public override void OnExecuteState()
        {
            if (owner.second)
                return;   // con đầu đếm + đánh thức cả hai
            var cam = owner.World.CameraCtl;
            if (cam == null || !cam.Locked)
                return;
            owner.lockTicks++;
            if (owner.lockTicks == owner.musicDelay)
            {
                owner.musicAtTick = owner.World.TickCount;
                owner.StartBossMusic();
            }
        }

        public override void CheckNextState()
        {
            if (owner.second || owner.lockTicks < owner.wakeDelay)
                return;
            var o = owner.PartnerAlive() ? owner.partner : null;
            owner.Wake();
            if (o != null)
                o.Wake();
        }
    }

    class WalkState : MummyState
    {
        public override void OnExecuteState()
        {
            owner.throwTimer--;
            owner.Facing = owner.walkDir;
            owner.Step();
            owner.Fall();
            owner.Show();
        }

        public override void CheckNextState()
        {
            if (owner.harnessPlan || owner.throwTimer > 0 || !owner.Grounded())
                return;
            if (owner.throws == 0 || owner.World.Rng.Next(100) < owner.throwChance)
                SetState(eMummyState.Throw);
            else
                owner.throwTimer = owner.throwRetry;   // bỏ lượt ném này
        }
    }

    class ThrowState : MummyState
    {
        public override void OnEnterState()
        {
            owner.Facing = owner.FacingToPlayer();
            owner.walkDir = owner.Facing;
            owner.flashRed = false;
        }

        public override void OnExecuteState()
        {
            int t = StateFrame;
            owner.flashRed = t < owner.releaseTick && ((t - 1) / owner.flashTicks) % 2 == 0;
            if (t == owner.releaseTick)
                owner.ThrowBandage();
            owner.Show();
        }

        public override void CheckNextState()
        {
            if (StateFrame < owner.poseTicks)
                return;
            owner.throwTimer = owner.throwEvery;
            owner.flashRed = false;
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

    /// <summary>Máu mỗi con = tổng / số thực thể (ConfigBoss hitsScope TotalAllEntities, entityCount).</summary>
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
        partnerDx = PInt(cfg, "partnerDxPx", 191);
        speed = BossMoveSpeed(cfg, 0.75f);
        wakeDelay = PInt(cfg, "wakeDelayTicks", 120);
        musicDelay = PInt(cfg, "musicDelayTicks", 60);
        decideTicks = Mathf.Max(1, PInt(cfg, "decideTicks", 16));
        decideFirst = Mathf.Max(1, PInt(cfg, "decideFirstTicks", 8));
        nearPx = PInt(cfg, "nearPx", 46);
        farPx = PInt(cfg, "farPx", 56);
        animStep = Mathf.Max(1, PInt(cfg, "animStepTicks", 16));
        throwFirst = PInt(cfg, "throwFirstTicks", 64);
        throwFirstPartner = PInt(cfg, "throwFirstPartnerTicks", 120);
        throwEvery = PInt(cfg, "throwEveryTicks", 66);
        throwChance = PInt(cfg, "throwChancePct", 50);
        throwRetry = PInt(cfg, "throwRetryTicks", 114);
        poseTicks = PInt(cfg, "throwPoseTicks", 48);
        releaseTick = PInt(cfg, "throwReleaseTicks", 40);
        flashTicks = Mathf.Max(1, PInt(cfg, "flashTicks", 2));
        aboveHigh = PInt(cfg, "bandageAboveHighPx", 31);
        aboveLow = PInt(cfg, "bandageAboveLowPx", 15);
        bandageSpeed = PFixed(cfg, "bandageSpeed", 1.5f);
        waveAmp = PInt(cfg, "bandageWaveAmpPx", 24);
        wavePeriod = Mathf.Max(1, PInt(cfg, "bandageWavePeriodTicks", 102));
        bandageInset = PInt(cfg, "bandageScreenInsetPx", 8);
        screenMin = PInt(cfg, "screenMinX", 8);
        screenMax = PInt(cfg, "screenMaxX", 248);
        lockTicks = animTick = decideTimer = 0;
        musicAtTick = -1;
        walkDir = -1;
        flashRed = false;
        throws = decisions = turns = 0;
        forcedHeights = null;
        forcedThrowFirst = -1;
        harnessPlan = false;
        partner = null;
        second = spawningPartner;
        throwTimer = second ? throwFirstPartner : throwFirst;
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
                partner.Show();
            }
        }
        Show();
    }

    /// <summary>Đồng hồ anim + lưới quyết định: đúng 1 lần mỗi tick không khựng sau khi thức (state chạy nối Walk↔Throw cùng tick không
    /// đếm 2 lần); lượt quyết định rơi vào lúc đang ném bị bỏ.</summary>
    protected override void TickBoss()
    {
        if (!Fsm.Is(eMummyState.Dormant))
        {
            animTick++;
            if (--decideTimer <= 0)
            {
                decideTimer = decideTicks;
                if (Fsm.Is(eMummyState.Walk))
                    Decide();
            }
        }
        Fsm.Tick();
    }

    /// <summary>Bất tử DÙNG CHUNG 2 con (video Map_3 f6186–6216: cả 2 đứng trong lửa nhưng ENEMY chỉ −1 mỗi ~32 tick; 2 video 0 lần −2 cùng lúc).</summary>
    public override bool OnHurt(ref HitInfo hit)
    {
        if (!base.OnHurt(ref hit))
            return false;
        if (PartnerAlive())
            partner.InvincibleTicks = Mathf.Max(partner.InvincibleTicks, invincibleTicks);
        return true;
    }

    private bool PartnerAlive()
    {
        return partner != null && partner.Alive && partner.SpawnTick == partnerSpawnTick && !partner.defeated;
    }

    /// <summary>Thức: kích hoạt va chạm, quay + đi về phía Simon (video: cả 2 con bước đầu tiên về Simon).</summary>
    private void Wake()
    {
        if (!dormant)
            return;
        Activate();
        Facing = FacingToPlayer();
        walkDir = Facing;
        animTick = 0;
        decideTimer = decideFirst;
        if (forcedThrowFirst >= 0)
            throwTimer = forcedThrowFirst;
        Fsm.SetState(eMummyState.Walk);
    }

    /// <summary>Khung hiện tại: walk [0,1,2,1] theo đồng hồ anim, bảng màu đỏ khi đang nháy lúc ném.</summary>
    private void Show()
    {
        var a = flashRed ? aRed : aPurple;
        if (Anim == null || a == null || a.frames == null || a.frames.Length == 0)
            return;
        int k = dormant ? 0 : (animTick / animStep) % a.frames.Length;
        Anim.SetFrame(a, k);
    }

    /// <summary>Harness: đổi hướng đi ngay (đang Walk).</summary>
    public void ForceTurn(int dir)
    {
        if (dir != walkDir)
        {
            walkDir = dir;
            turns++;
        }
    }

    /// <summary>Harness: bắt đầu tư thế ném ngay (đang Walk, không khựng).</summary>
    public void ForceThrow()
    {
        if (Fsm.Is(eMummyState.Walk) && !Stunned)
            Fsm.SetState(eMummyState.Throw);
    }

    /// <summary>Quyết định hướng: Simon quay lưng ⇒ tới; Simon nhìn về Mummy ⇒ dải |dx| nearPx / farPx.</summary>
    private void Decide()
    {
        decisions++;
        if (harnessPlan)
            return;
        var p = World.Player;
        if (p == null || !p.Alive)
            return;
        int dx = p.X - X;
        if (dx == 0)
            return;
        int toward = dx > 0 ? 1 : -1, ad = Mathf.Abs(dx), d = walkDir;
        bool behind = p.Facing == toward;   // Simon nhìn cùng chiều Mummy→Simon ⇒ Mummy ở sau lưng
        if (behind || ad >= farPx)
            d = toward;
        else if (ad < nearPx)
            d = -toward;
        if (d != walkDir)
        {
            walkDir = d;
            turns++;
        }
    }

    /// <summary>Đi 1 tick; ra ngoài [cx + screenMin, cx + screenMax] thì đứng tại chỗ (vẫn "đi" — anim chạy).</summary>
    private void Step()
    {
        int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
        int nx = Fixed.ToPx(fx + walkDir * speed);
        if (nx < cx + screenMin || nx > cx + screenMax)
            return;
        StepX(walkDir * speed);
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
        bool high;
        if (!string.IsNullOrEmpty(forcedHeights) && throws < forcedHeights.Length)
            high = forcedHeights[throws] != 'L';
        else
            high = World.Rng.Next(2) == 0;
        throws++;
        var b = World.Spawn(bandagePrefab, X, Y - (high ? aboveHigh : aboveLow), Facing) as MummyBandage;
        if (b != null)
            b.Launch(Facing * bandageSpeed, waveAmp, wavePeriod, bandageInset);
    }

    /// <summary>Thanh ENEMY = máu còn của cả hai con (hết máu tính 0).</summary>
    protected override int BarValue()
    {
        int cur = Mathf.Max(0, Health.Current), max = Health.Max;
        if (partner != null)
        {
            if (partner.SpawnTick == partnerSpawnTick)
                cur += Mathf.Max(0, partner.Health.Current);
            max += Health.Max;   // 2 con cùng máu tối đa
        }
        return max > 0 ? (cur * GameSession.MAX_HP + max - 1) / max : 0;
    }

    /// <summary>Con đầu bị hạ khi con kia còn ⇒ +điểm (video: MỖI con +3000) + lửa + biến mất; con cuối ⇒ chuỗi chết boss.</summary>
    protected override void Die()
    {
        if (PartnerAlive())
        {
            defeated = true;
            hidden = true;
            stunTicks = 0;
            if (Score > 0)
                GameRules.AddScore(Score);
            FxSprite.Spawn(World, "bossdeath_m", X, Y - deathFxUp, deathFxTicks);
            if (CvgpDebug.LogState)
                CvgpDebug.Log("[Boss] MummyMan " + (second ? "trái" : "phải") + " hạ ⇒ +" + Score + " — còn 1 con");
            partner.RefreshBar();
            GameSession.Current.NotifyChanged();
            Kill();
            RaiseDefeated();
            return;
        }
        base.Die();
    }
}
