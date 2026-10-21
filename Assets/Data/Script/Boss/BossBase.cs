using System;
using UnityEngine;

/// <summary>
/// Khung boss (CVGP 14 — routine 19/20 dùng lại). Boss = EnemyBase (nhóm Enemy, Hurtbox Enemy + Hitbox thân EnemyBody sát thương
/// theo STAGE) nhưng:
///   • Máu = CombatHealth(hitsWeak, hitsStrong) của ConfigBoss (bossId, phase) — mô hình HP = W × S chốt ở routine 07 (Phantom Bat 12/8:
///     HP 96, đòn mạnh −12 = 2 vạch, yếu −8); thanh ENEMY 16 vạch = Health.Bars(16) ghi vào GameSession.enemyHp mỗi lần trúng (StageRules
///     cho thanh HIỆN chạy dần 8 tick/vạch — video Map_1 3:24.9–3:30.4).
///   • Ngủ (dormant): không nhận/gây đòn tới khi lớp con gọi Activate() (Phantom Bat thức khi Simon tới gần).
///   • Trúng đòn: bất tử invincibleTicks + khựng hitStunTicks (đứng yên, khung "hurt") — đo video Map_1 (3 lần rìu: dơi đứng 10 ± 2
///     tick). Chạm nước thánh (lọ hoặc lửa) ⇒ khựng suốt lúc chạm + hitStunTicks (video boss f467–482: lọ bay xuyên dơi ⇒ dơi đứng — CVM2 04;
///     f469–533: dơi kẹt trong lửa tới chết). Stopwatch: theo ConfigBoss.stopwatchAffects — đứng yên nhưng bất tử vẫn đếm (OnTick).
///   • Hết máu (chuỗi chết): +điểm (GameRules.AddScore) NGAY, ENEMY 0, ẩn boss, FX "bossdeath_m" tại tâm (X, Y − deathFxUp) — video:
///     6 ngọn 2×3, 7 khung × ~9 tick (62); sau crystalDelayTicks tick sinh Magic Crystal (ItemPickup, rơi trọng lực item) ở
///     (cx + crystalScreenX, crystalY) rồi Kill(). Nhặt cầu ⇒ ItemEffects.CrystalPicked ⇒ StageRules (tally — routine 12).
///   • Rosary không diệt (IsBoss). Sự kiện tĩnh BossDefeated / CrystalSpawned (harness, luồng stage 15).
/// Lớp con: BuildStates (StateMachine), LoadBoss(cfg) (đọc ConfigActorParam "Boss.&lt;id&gt;" qua BossParams), TickBoss() (Fsm.Tick()).
/// Đặt vào màn: BossArena (con section) sinh boss + tường mép màn khi camera khoá.
/// </summary>
public abstract class BossBase : EnemyBase
{
    [SerializeField] protected eBossId bossId;
    [SerializeField] protected int phase = 1;

    /// <summary>Boss đang ngủ (chưa kích hoạt): không va chạm.</summary>
    [NonSerialized] public bool dormant;
    /// <summary>Số tick còn khựng (trúng đòn / chạm nước thánh).</summary>
    [NonSerialized] public int stunTicks;
    /// <summary>Đang chuỗi chết (ẩn, đếm tới lúc sinh quả cầu).</summary>
    [NonSerialized] public bool dying;
    [NonSerialized] public int deathAge;
    /// <summary>Số đòn đã nhận (harness).</summary>
    [NonSerialized] public int hitsTaken;
    [NonSerialized] public BossArena arena;
    /// <summary>Tick này lọ/lửa nước thánh đang chạm vùng dò (CVGP 20: Dracula dạng 2 tắt hộp chạm khi bị lửa giữ — wiki).</summary>
    [NonSerialized] protected bool holyTouch;

    protected int hitStunTicks, invincibleTicks, deathFxTicks, deathFxUp, crystalDelay, crystalScreenX, crystalY;
    /// <summary>CVM3 09: khựng sau khi rời nước thánh (mặc định = hitStunTicks) + quả cầu đứng yên trước khi rơi (mặc định 0) —
    /// khoá tuỳ chọn Boss.&lt;id&gt;.holyStunTicks / crystalDelayTicks / crystalHoverTicks (Mummy Men đo video; boss khác không có khoá ⇒ y nguyên).</summary>
    protected int holyStunTicks, crystalHoverTicks;
    /// <summary>ConfigBoss.stopwatchAffects (wiki: Phantom Bat, Queen Medusa).</summary>
    protected bool stopwatchAffects;
    protected AnimSet.Anim aHurt;
    private string deathFxAnim = "bossdeath_m";

    public eBossId BossId => bossId;
    public override bool IsBoss => true;
    public bool Stunned => stunTicks > 0;

    public static event Action<BossBase> BossDefeated;
    public static event Action<BossBase, ItemPickup> CrystalSpawned;

    protected override void ResetHealth(GameConfig cfg)
    {
        var row = cfg != null ? cfg.GetBoss(bossId, phase) : null;
        string ctx = "ConfigBoss." + bossId + "." + phase;
        Health.Reset(row != null ? row.hitsWeak.Get(ctx + ".hitsWeak", 1) : 1, row != null ? row.hitsStrong.Get(ctx + ".hitsStrong", 1) : 1);
        Score = row != null && row.score.has ? row.score.value : 0;
        stopwatchAffects = row != null && row.stopwatchAffects;
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        dormant = true;
        stunTicks = 0;
        dying = false;
        deathAge = 0;
        hitsTaken = 0;
        checkOffscreen = false;
        hitStunTicks = PInt(cfg, "hitStunTicks", 10);
        invincibleTicks = PInt(cfg, "invincibleTicks", 16);
        deathFxTicks = PInt(cfg, "deathFxTicks", 62);
        deathFxUp = PInt(cfg, "deathFxUpPx", 8);
        crystalDelay = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnDelayTicks", 152));
        crystalScreenX = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnScreenX", 128));
        crystalY = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnY", 104));
        holyStunTicks = HasParam(cfg, "holyStunTicks") ? PInt(cfg, "holyStunTicks", hitStunTicks) : hitStunTicks;
        if (HasParam(cfg, "crystalDelayTicks"))
            crystalDelay = PInt(cfg, "crystalDelayTicks", crystalDelay);
        crystalHoverTicks = HasParam(cfg, "crystalHoverTicks") ? PInt(cfg, "crystalHoverTicks", 0) : 0;
        InvincibleOnHit = invincibleTicks;
        if (animSet != null && animSet.Has("hurt"))
            aHurt = animSet.Get("hurt");
        Bar(BarValue());
        LoadBoss(cfg);
    }

    /// <summary>Tham số hành vi (mặc định ConfigActorParam "Boss.&lt;bossId&gt;"; CVGP 18: Phantom Bat bản thường đọc "Enemy.PhantomBatMinor").</summary>
    protected virtual int PInt(GameConfig cfg, string key, int fallback) { return BossParams.Int(cfg, bossId, key, fallback); }
    protected virtual int PFixed(GameConfig cfg, string key, float fallback) { return BossParams.Fixed256(cfg, bossId, key, fallback); }
    /// <summary>Có khoá tuỳ chọn ConfigActorParam "Boss.&lt;bossId&gt;.key" (không log lỗi khi thiếu — CVM3 09).</summary>
    protected bool HasParam(GameConfig cfg, string key) { return cfg != null && cfg.actorParams != null && cfg.GetParam(bossId, key) != null; }
    /// <summary>Dùng thanh ENEMY của HUD (CVGP 18: Phantom Bat bản thường là enemy thường ⇒ false).</summary>
    protected virtual bool UsesEnemyBar => true;
    /// <summary>Đổi nhạc sang nhạc boss khi bắt đầu đánh (CVGP 18: bản thường ⇒ false).</summary>
    protected virtual bool PlaysBossMusic => true;

    /// <summary>Số vạch thanh ENEMY (mặc định máu thực thể này — CVGP 19: Mummy Men cộng máu 2 con).</summary>
    protected virtual int BarValue() { return Health.Bars(GameSession.MAX_HP); }

    /// <summary>ConfigBoss.moveSpeed (px/tick) → fixed (CVGP 19).</summary>
    protected int BossMoveSpeed(GameConfig cfg, float fallback)
    {
        var row = cfg != null ? cfg.GetBoss(bossId, phase) : null;
        return Fixed.FromFloat(row != null ? row.moveSpeed.Get("ConfigBoss." + bossId + "." + phase + ".moveSpeed", fallback) : fallback);
    }

    /// <summary>Cập nhật thanh ENEMY ngay (CVGP 19: thực thể phụ đổi máu chung).</summary>
    protected void RefreshBar() { Bar(BarValue()); }

    private void Bar(int bars)
    {
        if (UsesEnemyBar)
            SetEnemyBar(bars);
    }

    protected abstract void LoadBoss(GameConfig cfg);
    protected abstract void TickBoss();
    /// <summary>Đang khựng: lớp con giữ khung (mặc định khung "hurt" nếu có).</summary>
    protected virtual void OnStunTick()
    {
        if (Anim != null && aHurt != null && !Anim.IsPlaying(aHurt))
            Anim.Play(aHurt, true);
    }
    /// <summary>Hết khựng: lớp con phát lại anim của state hiện tại.</summary>
    protected virtual void OnStunEnd()
    {
    }

    /// <summary>Kích hoạt (thức): bắt đầu nhận/gây đòn.</summary>
    public void Activate()
    {
        dormant = false;
        ApplyVisibility();   // bật hộp ngay (boss đang bị Stopwatch giữ thì OnTick không chạy)
        Bar(BarValue());
    }

    /// <summary>Nhạc boss = ConfigBlock.bossMusic của block đang chơi (CVM2 03: Poison Mind — video Map_1/Boss_Map_1/Map_2/Boss_Map_2).</summary>
    protected void StartBossMusic()
    {
        if (!PlaysBossMusic)
            return;
        var cfg = GameConfig.Current;
        var block = cfg != null && GameSession.Current != null ? cfg.GetBlockOfStage(GameSession.Current.stage) : null;
        var id = block != null ? block.bossMusic : eMusic.PoisonMind;
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(id);
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] " + bossId + ": nhạc boss " + id);
    }

    protected sealed override void TickStates()
    {
        if (dying)
        {
            if (++deathAge >= crystalDelay)
                SpawnCrystal();
            return;
        }
        holyTouch = !dormant && TouchingHolyWater();
        if (holyTouch)
            stunTicks = Mathf.Max(stunTicks, holyStunTicks);
        if (stunTicks > 0)
        {
            stunTicks--;
            OnStunTick();
            if (stunTicks == 0)
                OnStunEnd();
            return;
        }
        TickBoss();
    }

    protected override void ApplyVisibility()
    {
        bool on = !hidden && Alive && !dormant && !dying;
        Hurt.active = on;
        Touch.active = on;
        if (Anim != null)
            Anim.Visible = !hidden;
    }

    /// <summary>Vùng dò nước thánh (mặc định hộp nhận đòn; CVGP 20: Dracula — đầu mới nhận đòn nhưng lửa dưới CHÂN làm choáng (wiki)).</summary>
    protected virtual RectInt HolyWaterProbe => Hurt.SectionRect;

    /// <summary>Lọ / lửa nước thánh đang chạm hộp nhận đòn (video boss: dơi đứng yên khi chạm — kể cả khi không mất máu).</summary>
    private bool TouchingHolyWater()
    {
        var me = HolyWaterProbe;
        var list = World.GroupList(eEntityGroup.PlayerWeapon);
        for (int i = 0; i < list.Count; i++)
        {
            var w = list[i] as SubWeaponProjectile;
            if (w == null || !w.Alive || w.Kind != eSubWeapon.HolyWater || w.Hit == null || !w.Hit.active)
                continue;
            if (CombatResolver.Overlaps(w.Hit.SectionRect, me))
                return true;
        }
        return false;
    }

    /// <summary>Boss tự xử lý Stopwatch (OnTick) để bộ đếm bất tử vẫn chạy khi bị đóng băng — wiki: "dùng Stopwatch rồi quất tới khi
    /// hết đóng băng" (StageWorld chỉ giảm InvincibleTicks của thực thể không bị đóng băng). Đóng băng thế giới (nhặt roi) như cũ.</summary>
    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;
    }

    public override void OnTick()
    {
        if (!dying && stopwatchAffects && World.IsEnemyFrozen)
        {
            ApplyVisibility();   // đứng yên, anim dừng (không Anim.Tick); vẫn nhận đòn
            return;
        }
        base.OnTick();
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (dormant || dying || hidden || !Alive || Health.IsDead)
            return false;
        Health.Apply(hit.strength);
        hitsTaken++;
        Bar(BarValue());
        var cfg = World.Config;
        // tia lửa tại chỗ trúng (video boss f492–530: tia hồng ở mép dơi chỗ roi chạm)
        var a = hit.hitbox != null ? hit.hitbox.SectionRect : new RectInt(X, Y, 0, 0);
        var b = Hurt.SectionRect;
        int x0 = Mathf.Max(a.xMin, b.xMin), x1 = Mathf.Min(a.xMax, b.xMax), y0 = Mathf.Max(a.yMin, b.yMin), y1 = Mathf.Min(a.yMax, b.yMax);
        FxSprite.Spawn(World, "hit", (x0 + x1) / 2, (y0 + y1) / 2, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        if (Health.IsDead)
            Die();
        else
            stunTicks = Mathf.Max(stunTicks, hitStunTicks);
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] " + bossId + " trúng " + hit.strength + " ⇒ HP " + Health.Current + "/" + Health.Max + " (" + Health.Bars(16) + " vạch)");
        return true;
    }

    /// <summary>Chuỗi chết: điểm ngay, ENEMY 0, ẩn boss, FX lửa; quả cầu sau crystalDelay tick (TickStates).</summary>
    protected override void Die()
    {
        defeated = true;
        dying = true;
        deathAge = 0;
        stunTicks = 0;
        hidden = true;
        if (Score > 0)
            GameRules.AddScore(Score);
        SetEnemyBar(0);
        GameSession.Current.NotifyChanged();
        FxSprite.Spawn(World, deathFxAnim, X, Y - deathFxUp, deathFxTicks);
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] " + bossId + " hạ ⇒ +" + Score + ", quả cầu sau " + crystalDelay + " tick");
        if (BossDefeated != null)
            BossDefeated(this);
        RaiseDefeated();
    }

    private void SpawnCrystal()
    {
        int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
        var it = ItemDrops.Spawn(World, eItemId.MagicCrystal, cx + crystalScreenX, crystalY);
        if (it != null)
        {
            it.holdTicks = crystalHoverTicks;
            OnCrystalSpawned(it);
        }
        if (CrystalSpawned != null)
            CrystalSpawned(this, it);
        Kill();
    }

    /// <summary>Quả cầu vừa sinh (CVGP 20: Dracula đánh dấu quả cầu cuối ⇒ nhặt ⇒ GameRules.OnGameCleared).</summary>
    protected virtual void OnCrystalSpawned(ItemPickup crystal)
    {
    }

    protected static void SetEnemyBar(int bars)
    {
        var s = GameSession.Current;
        if (s.enemyHp == bars)
            return;
        s.enemyHp = bars;
        s.NotifyChanged();
    }

    /// <summary>Cheat / harness: đặt máu còn hp (≥ 1).</summary>
    public void DebugSetHp(int hp)
    {
        Health.Set(Mathf.Max(1, hp));
        Bar(BarValue());
    }
}

/// <summary>Đọc tham số hành vi boss (ConfigActorParam "Boss.&lt;id&gt;") — thiếu ⇒ log lỗi + fallback (như EnemyParams).</summary>
public static class BossParams
{
    public static float Get(GameConfig cfg, eBossId id, string key, float fallback)
    {
        var r = cfg != null && cfg.actorParams != null ? cfg.GetParam(id, key) : null;
        if (r == null)
        {
            if (cfg != null)
                Debug.LogError("[Config] thiếu ConfigActorParam Boss." + id + "." + key);
            return fallback;
        }
        return r.value.Get("Boss." + id + "." + key, fallback);
    }

    public static int Fixed256(GameConfig cfg, eBossId id, string key, float fallback)
    {
        return Fixed.FromFloat(Get(cfg, id, key, fallback));
    }

    public static int Int(GameConfig cfg, eBossId id, string key, int fallback)
    {
        return Mathf.RoundToInt(Get(cfg, id, key, fallback));
    }
}
