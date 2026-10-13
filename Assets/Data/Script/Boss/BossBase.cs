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
///     tick). Chạm nước thánh (lọ hoặc lửa) ⇒ khựng suốt lúc chạm + hitStunTicks (video boss f342–352: lọ bay xuyên dơi ⇒ dơi đứng;
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

    protected int hitStunTicks, invincibleTicks, deathFxTicks, deathFxUp, crystalDelay, crystalScreenX, crystalY;
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
        hitStunTicks = BossParams.Int(cfg, bossId, "hitStunTicks", 10);
        invincibleTicks = BossParams.Int(cfg, bossId, "invincibleTicks", 16);
        deathFxTicks = BossParams.Int(cfg, bossId, "deathFxTicks", 62);
        deathFxUp = BossParams.Int(cfg, bossId, "deathFxUpPx", 8);
        crystalDelay = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnDelayTicks", 152));
        crystalScreenX = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnScreenX", 128));
        crystalY = Mathf.RoundToInt(ItemPickup.Param(cfg, eItemId.MagicCrystal, "spawnY", 104));
        InvincibleOnHit = invincibleTicks;
        if (animSet != null && animSet.Has("hurt"))
            aHurt = animSet.Get("hurt");
        SetEnemyBar(Health.Bars(GameSession.MAX_HP));
        LoadBoss(cfg);
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
        SetEnemyBar(Health.Bars(GameSession.MAX_HP));
    }

    /// <summary>Nhạc boss: chưa import nhạc (CVGP_MEMORY §6 nợ 02) ⇒ PlayMusic(null) = im lặng + log (H2 — không thay bài khác).</summary>
    protected void StartBossMusic()
    {
        if (AudioManager.I != null)
            AudioManager.I.PlayMusic(null, true);
        if (CvgpDebug.LogState)
            CvgpDebug.Log("[Boss] " + bossId + ": nhạc boss (ConfigBlock.bossMusic) — chưa có clip");
    }

    protected sealed override void TickStates()
    {
        if (dying)
        {
            if (++deathAge >= crystalDelay)
                SpawnCrystal();
            return;
        }
        if (!dormant && TouchingHolyWater())
            stunTicks = Mathf.Max(stunTicks, hitStunTicks);
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

    /// <summary>Lọ / lửa nước thánh đang chạm hộp nhận đòn (video boss: dơi đứng yên khi chạm — kể cả khi không mất máu).</summary>
    private bool TouchingHolyWater()
    {
        var me = Hurt.SectionRect;
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
        SetEnemyBar(Health.Bars(GameSession.MAX_HP));
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
        if (CrystalSpawned != null)
            CrystalSpawned(this, it);
        Kill();
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
        SetEnemyBar(Health.Bars(GameSession.MAX_HP));
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
