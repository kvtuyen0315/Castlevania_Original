using System;
using UnityEngine;

/// <summary>
/// Lớp nền enemy thường (CVGP 13 — khuôn cho routine 17/18). Nhóm Enemy; Hurtbox Enemy + Hitbox thân EnemyBody (singleHit = false,
/// sát thương = StageWorld.DamageToPlayer() — wiki: theo STAGE, không theo loại enemy). Máu = CombatHealth(W, S) của ConfigEnemy.
/// Chết (OnHurt hết máu): tia lửa "hit" + lửa "death" (FX 8/9/3 tick — video Map_1 f690–699 Zombie, f1296–1305 báo, f3576–3585
/// Fish Man: CÙNG nhịp lửa vỡ nến routine 11) tại TÂM thân + SFX HitFlame + điểm (GameRules.AddScore — không hiện chữ điểm: video
/// không có) + (tuỳ) item rơi khi lửa tắt (20 tick, như nến) rồi Kill() NGAY (vũ khí phụ đếm kills). Rosary: StageWorld.RosaryClear
/// gỡ thẳng (0 điểm). Stopwatch: Entity.IsFrozenBy (nhóm Enemy). Ra khỏi màn: gỡ khi pivot ngoài [cx + inset, cx + 256 − inset]
/// (video f2436–2447: inset 4) nếu checkOffscreen (enemy đặt sẵn đang chờ để SpawnPoint lo).
/// Lớp con: BuildStates() (StateMachine + state lồng), LoadTuning(cfg) (đọc Config 1 lần), TickStates() (Fsm.Tick()).
/// </summary>
public abstract class EnemyBase : Entity
{
    [SerializeField] protected eEnemyId enemyId;
    [Tooltip("Hộp nhận đòn (px quanh pivot, quay TRÁI) — bbox pixel đục sprite (XẤP XỈ: hộp ROM không đo được, như Simon 07).")]
    [SerializeField] protected RectInt hurtBox = new RectInt(-8, -31, 16, 32);
    [Tooltip("Hộp thân gây sát thương khi chạm Simon (cùng bbox).")]
    [SerializeField] protected RectInt touchBox = new RectInt(-8, -31, 16, 32);
    [Tooltip("Tâm thân so với pivot (y âm = lên) — chỗ sinh lửa chết (video: tâm sprite).")]
    [SerializeField] protected int centerDy = -15;

    public readonly CombatHealth Health = new CombatHealth();
    public Hurtbox Hurt { get; private set; }
    public Hitbox Touch { get; private set; }
    public eEnemyId EnemyId => enemyId;
    public int Score { get; protected set; }
    public int CenterY => Y + centerDy;

    /// <summary>Item rơi khi chết (mặc định None — wiki/video không cho bảng rơi; nơi đặt enemy (15) gán nếu video có).</summary>
    [NonSerialized] public eItemId dropItem;
    /// <summary>Số tick đã chạy (không tính tick đóng băng).</summary>
    [NonSerialized] public int ageTicks;
    /// <summary>Ẩn (Fish Man dưới nước): không vẽ, không va chạm.</summary>
    [NonSerialized] public bool hidden;
    /// <summary>Gỡ khi ra khỏi màn (tắt khi đang chờ tại chỗ — SpawnPoint lo).</summary>
    [NonSerialized] public bool checkOffscreen;
    /// <summary>Đã bị hạ bằng đòn (khác bị gỡ vì ra khỏi màn / Rosary).</summary>
    [NonSerialized] public bool defeated;
    protected int despawnInset = 4;

    /// <summary>Enemy vừa bị hạ (harness + luật chơi).</summary>
    public static event Action<EnemyBase> Defeated;

    protected override void OnBuild()
    {
        Hurt = AddHurtbox(eHurtLayer.Enemy, hurtBox);
        Touch = AddHitbox(eHitLayer.EnemyBody, touchBox);
        Touch.singleHit = false;
        BuildStates();
    }

    /// <summary>Máu + điểm từ ConfigEnemy (CVGP 14: boss đọc ConfigBoss — BossBase override).</summary>
    protected virtual void ResetHealth(GameConfig cfg)
    {
        var row = cfg != null ? cfg.GetEnemy(enemyId) : null;
        string ctx = "ConfigEnemy." + enemyId;
        Health.Reset(row != null ? row.hitsWeak.Get(ctx + ".hitsWeak", 1) : 1, row != null ? row.hitsStrong.Get(ctx + ".hitsStrong", 1) : 1);
        Score = row != null && row.score.has ? row.score.value : 0;
    }

    protected abstract void BuildStates();
    protected abstract void LoadTuning(GameConfig cfg);
    protected abstract void TickStates();

    protected override void OnSpawned()
    {
        var cfg = World.Config;
        ResetHealth(cfg);
        Touch.damage = World.DamageToPlayer();
        Hurt.local = hurtBox;
        Touch.local = touchBox;
        dropItem = eItemId.None;
        ageTicks = 0;
        hidden = false;
        defeated = false;
        checkOffscreen = true;
        despawnInset = Mathf.RoundToInt(EnemyParams.Get(cfg, eEnemyId.Ghoul, "despawnInsetPx", 4f));
        if (Anim != null)
            Anim.Visible = true;
        LoadTuning(cfg);
        ApplyVisibility();
    }

    public override void OnTick()
    {
        ageTicks++;
        TickStates();
        if (Alive && checkOffscreen && IsOutsideScreen())
            Kill(eKillReason.Offscreen);
        ApplyVisibility();
        base.OnTick();
    }

    protected virtual void ApplyVisibility()
    {
        bool on = !hidden && Alive;
        Hurt.active = on;
        Touch.active = on;
        if (Anim != null)
            Anim.Visible = !hidden;
    }

    /// <summary>Pivot ra ngoài [cx + inset, cx + 256 − inset] (video Zombie ra mép trái f2436–2447).</summary>
    public bool IsOutsideScreen()
    {
        int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
        return X < cx + despawnInset || X > cx + NesCoord.SCREEN_W - despawnInset;
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (hidden || !Alive || Health.IsDead)
            return false;
        Health.Apply(hit.strength);
        if (Health.IsDead)
            Die();
        return true;
    }

    /// <summary>Hạ: FX + SFX + điểm + (rơi item khi lửa tắt) + Kill ngay.</summary>
    protected virtual void Die()
    {
        defeated = true;
        var cfg = World.Config;
        int cy = CenterY;
        FxSprite.Spawn(World, "hit", X, cy, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        var flame = FxSprite.Spawn(World, "death", X, cy);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HitFlame);
        if (Score > 0)
        {
            GameRules.AddScore(Score);
            GameSession.Current.NotifyChanged();
        }
        if (dropItem != eItemId.None && flame != null)
        {
            // item hiện khi lửa tắt (= BreakItemDelayTicks 20 của nến — video zombie f864 ⇒ đồng hồ f874), ô 16 căn giữa tâm thân
            var item = dropItem;
            int x = X, y = cy + 8;
            flame.onDone = f => ItemDrops.Spawn(f.World, ItemDrops.Resolve(f.World, item), x, y);
        }
        Kill();
        if (Defeated != null)
            Defeated(this);
    }

    /// <summary>Bắn sự kiện Defeated (lớp con tự làm chuỗi chết — BossBase).</summary>
    protected void RaiseDefeated()
    {
        if (Defeated != null)
            Defeated(this);
    }

    /// <summary>Hướng về phía Player (−1 / +1); không có Player ⇒ giữ hướng.</summary>
    protected int FacingToPlayer()
    {
        var p = World.Player;
        if (p == null || p.X == X)
            return Facing;
        return p.X > X ? 1 : -1;
    }

    /// <summary>Hạ ngay như trúng đòn chí mạng (harness / cheat).</summary>
    public void Defeat()
    {
        if (Alive && !hidden)
            Die();
    }

    /// <summary>Đi ngang có chặn tường (body enemy = vùng CHÂN 2×2 ⇒ chỉ tường ngang chân — video: báo #3 nhảy qua dưới bục y 80–95,
    /// Fish Man đi dưới khối CB (6–7,4) mà không vướng).</summary>
    protected internal bool StepX(int dx) { return MoveX(dx); }

    /// <summary>Có sàn ĐÚNG cột pivot X (mặt trên ô đặc ở hàng Y) — video: Zombie rơi khỏi gờ PB ở x 127 (mép 128), Fish Man trồi ở
    /// x 224 / 288 (mép khe lối đi CB) rơi lại xuống nước dù quay trái hay phải ⇒ dò 1 cột, không theo hướng.</summary>
    protected internal bool Grounded()
    {
        return World.Tiles != null && World.Tiles.RowLandable(Y, X, X);
    }

    /// <summary>Dọc dy (fixed): đi lên xuyên mọi thứ; đi xuống đáp ở mặt trên ô đặc đầu tiên tại cột X (như TileCollision.MoveY
    /// nhưng 1 cột). Trả true nếu đáp.</summary>
    protected internal bool StepFall(int dy)
    {
        int old = Y;
        int nf = fy + dy;
        int np = Fixed.ToPx(nf);
        if (dy > 0 && np > old && World.Tiles != null)
            for (int y = old; y < np; y++)
                if (World.Tiles.RowLandable(y, X, X))
                {
                    fy = Fixed.FromPx(y);
                    return true;
                }
        fy = nf;
        return false;
    }
}

/// <summary>Đọc tham số hành vi enemy (ConfigActorParam "Enemy.&lt;id&gt;") — thiếu ⇒ log lỗi + fallback (không âm thầm).</summary>
public static class EnemyParams
{
    public static float Get(GameConfig cfg, eEnemyId id, string key, float fallback)
    {
        var r = cfg != null && cfg.actorParams != null ? cfg.GetParam(id, key) : null;
        if (r == null)
        {
            if (cfg != null)
                Debug.LogError("[Config] thiếu ConfigActorParam Enemy." + id + "." + key);
            return fallback;
        }
        return r.value.Get("Enemy." + id + "." + key, fallback);
    }

    public static int Fixed256(GameConfig cfg, eEnemyId id, string key, float fallback)
    {
        return Fixed.FromFloat(Get(cfg, id, key, fallback));
    }

    public static int Int(GameConfig cfg, eEnemyId id, string key, int fallback)
    {
        return Mathf.RoundToInt(Get(cfg, id, key, fallback));
    }

    /// <summary>Tham số tuỳ chọn: không có dòng ⇒ fallback, KHÔNG log.</summary>
    public static int IntOpt(GameConfig cfg, eEnemyId id, string key, int fallback)
    {
        var r = cfg != null && cfg.actorParams != null ? cfg.GetParam(id, key) : null;
        return r != null && r.value.has ? Mathf.RoundToInt(r.value.value) : fallback;
    }

    /// <summary>Tốc độ di chuyển ConfigEnemy.moveSpeed (px/tick) → fixed.</summary>
    public static int MoveSpeed(GameConfig cfg, eEnemyId id, float fallback)
    {
        var row = cfg != null ? cfg.GetEnemy(id) : null;
        return Fixed.FromFloat(row != null ? row.moveSpeed.Get("ConfigEnemy." + id + ".moveSpeed", fallback) : fallback);
    }
}
