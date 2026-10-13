using UnityEngine;

/// <summary>State BoneDragon (Skele-Dragon).</summary>
public enum eBoneDragonState
{
    Idle,   // cổ ngọ nguậy, miệng đóng
    Warn,   // há miệng trước khi phun
    Fire,   // phun loạt cầu lửa theo góc tới Simon
}

/// <summary>
/// BoneDragon — "Skele-Dragon" trên sheet (CVGP 18, theo wiki — GẦN ĐÚNG, không có video Block 4). Wiki Enemies#Bone Dragons +
/// Stages 10-12: rồng xương gắn vào tường/khối, thân bất động, đầu trên cổ dài ngọ nguậy lên xuống; phun cầu lửa từ miệng "theo các
/// góc"; chỉ gây hại khi lại quá gần hoặc trúng cầu lửa; cầu lửa quất được ("đừng để lọt qua roi"); "chờ đầu hạ thấp rồi quất, hoặc
/// nhảy quất"; chết rơi 5–8 túi tiền / tim nhỏ; bị Holy Water khi đánh ⇒ rơi shot multiplier; 8 đòn yếu / 7 đòn mạnh, 1000 đ; cả game
/// 3 con (St 11: 1, St 12: 2).
/// Hình (H2 — chỉ sprite sheet): pivot (X, Y) = chân cổ ở tường; cổ = `segments` đốt skeledragon_bone (8×16) nối ra phía mặt
/// (`Facing`), cách `segmentSpacingPx`; đầu skeledragon_head_0 (miệng đóng) / _1 (há — SUY LUẬN theo hình) ở cuối cổ. Ngọ nguậy
/// (ước lượng): đốt k (1..N) lệch dọc A·k/(N+1)·sin(2π(t − k·lag)/P), đầu lệch A·sin(2π(t − (N+1)·lag)/P) — sóng lan từ tường ra đầu.
/// Nhận đòn: CHỈ đầu (wiki "chờ đầu hạ thấp"); gây đòn khi chạm: đầu + mọi đốt cổ. Phun: nghỉ firstIdle rồi idleMin..+span ⇒ há
/// miệng warnTicks ⇒ `burst` quả cách burstGapTicks, mỗi quả nhắm tâm thân Simon (chân − aimAbovePx) lúc phun, tốc fireballSpeed
/// (vx, vy nguyên — H7), từ miệng (đầu + mouthFrontPx theo hướng mặt) ⇒ nghỉ. Đạn = BoneTowerFireball (đi thẳng vx/vy, phá được +100).
/// Chết: lửa "death" ở đầu + mỗi đốt; khi lửa tắt rơi dropMin..+span vật: túi tiền đỏ (dropMoneyPct %) / tim nhỏ tại các đốt cổ; đã trúng
/// nước thánh ⇒ thêm Double/Triple Shot ở đầu. Mỗi đòn chưa hạ: tia lửa + bất tử hitInvincibleTicks. Đặt bằng SpawnPoint (tường).
/// </summary>
public class BoneDragon : EnemyBase
{
    [SerializeField] private BoneTowerFireball fireballPrefab;
    [Tooltip("Renderer các đốt cổ (con, builder tạo) — từ tường ra đầu.")]
    [SerializeField] private SpriteRenderer[] neckViews = new SpriteRenderer[0];

    public StateMachine<BoneDragon, eBoneDragonState> Fsm { get; private set; }
    [System.NonSerialized] public int segments, spacing, amplitude, period, lag, headGap;
    [System.NonSerialized] public int firstIdle, idleMin, idleSpan, warnTicks, burst, burstGap, fireSpeed, aimAbove, mouthFront;
    [System.NonSerialized] public int dropMin, dropSpan, dropMoneyPct;
    [System.NonSerialized] public int waveT, idleLen, burstLeft;
    /// <summary>Đã bị nước thánh đánh (wiki ⇒ rơi shot multiplier).</summary>
    [System.NonSerialized] public bool holyHit;
    /// <summary>Số cầu lửa / số loạt (harness).</summary>
    [System.NonSerialized] public int shots, volleys;
    private Vector2Int[] seg = new Vector2Int[0];   // tâm đốt k (local quay TRÁI, px so với pivot)
    private Vector2Int head;
    private Hitbox[] neckHits = new Hitbox[0];
    private AnimSet.Anim aHead;
    private Sprite boneSprite;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";
    /// <summary>Tâm đầu (px section).</summary>
    public Vector2Int HeadPos => new Vector2Int(X + (Facing > 0 ? -head.x : head.x), Y + head.y);

    abstract class DragonState : BaseState<BoneDragon>
    {
        protected void SetState(eBoneDragonState s) { owner.Fsm.SetState(s); }
    }

    class IdleState : DragonState
    {
        public override void OnEnterState() { owner.Mouth(false); }

        public override void CheckNextState()
        {
            if (StateFrame > owner.idleLen)
                SetState(eBoneDragonState.Warn);
        }
    }

    class WarnState : DragonState
    {
        public override void OnEnterState() { owner.Mouth(true); }

        public override void CheckNextState()
        {
            if (StateFrame > owner.warnTicks)
                SetState(eBoneDragonState.Fire);
        }
    }

    class FireState : DragonState
    {
        public override void OnEnterState()
        {
            owner.volleys++;
            owner.burstLeft = owner.burst;
        }

        public override void OnExecuteState()
        {
            // quả k ở StateFrame 1 + k·gap
            if (owner.burstLeft > 0 && (StateFrame - 1) % owner.burstGap == 0)
            {
                owner.Shoot();
                owner.burstLeft--;
            }
        }

        public override void CheckNextState()
        {
            if (owner.burstLeft <= 0)
            {
                owner.idleLen = owner.Roll(owner.idleMin, owner.idleSpan);
                SetState(eBoneDragonState.Idle);
            }
        }
    }

    protected override void OnBuild()
    {
        base.OnBuild();
        neckHits = new Hitbox[neckViews.Length];
        for (int i = 0; i < neckHits.Length; i++)
        {
            neckHits[i] = AddHitbox(eHitLayer.EnemyBody, new RectInt(-4, -8, 8, 16));
            neckHits[i].singleHit = false;
        }
    }

    private void Mouth(bool open)
    {
        if (Anim != null && aHead != null)
            Anim.SetFrame(aHead, open ? 1 : 0);
    }

    /// <summary>Tính tâm đốt cổ + đầu theo waveT (local quay trái: ra xa tường = x âm).</summary>
    private void Shape()
    {
        int n = segments;
        if (seg.Length != n)
            seg = new Vector2Int[n];
        for (int k = 1; k <= n; k++)
        {
            float w = Mathf.Sin(2f * Mathf.PI * (waveT - k * lag) / period);
            seg[k - 1] = new Vector2Int(-k * spacing, Mathf.RoundToInt(amplitude * k / (float)(n + 1) * w));
        }
        float wh = Mathf.Sin(2f * Mathf.PI * (waveT - (n + 1) * lag) / period);
        head = new Vector2Int(-(n * spacing + headGap), Mathf.RoundToInt(amplitude * wh));
        Hurt.local = new RectInt(head.x - 8, head.y - 8, 16, 16);
        Touch.local = Hurt.local;
        for (int i = 0; i < neckHits.Length; i++)
        {
            var s = i < n ? seg[i] : seg[n - 1];
            neckHits[i].local = new RectInt(s.x - 4, s.y - 8, 8, 16);
        }
    }

    private void Shoot()
    {
        shots++;
        if (fireballPrefab == null)
            return;
        var h = HeadPos;
        int mx = h.x + Facing * mouthFront, my = h.y;
        var p = World.Player;
        long dx = p != null ? p.X - mx : Facing * 64, dy = p != null ? (p.Y - aimAbove) - my : 0;
        long len = ISqrt((dx * dx + dy * dy) << 16);   // độ dài ×256 (căn nguyên — H7) ⇒ |v| sai < 1/256
        if (len == 0)
        {
            dx = Facing;
            len = 256;
        }
        // pivot đạn = tâm ô 8×16, quả cầu ở nửa trên (tâm ≈ pivot − 4) ⇒ đặt pivot thấp hơn miệng 4 px
        var f = World.Spawn(fireballPrefab, mx, my + 4, dx >= 0 ? 1 : -1) as BoneTowerFireball;
        if (f != null)
        {
            f.vx = (int)(fireSpeed * dx * 256 / len);
            f.vy = (int)(fireSpeed * dy * 256 / len);
        }
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

    public override bool OnHurt(ref HitInfo hit)
    {
        var w = hit.attacker as SubWeaponProjectile;
        bool holy = (w != null && w.Kind == eSubWeapon.HolyWater) || HolyFire.IsHolyFire(hit.hitbox);
        bool took = base.OnHurt(ref hit);
        if (took && holy)
            holyHit = true;
        return took;
    }

    /// <summary>Hạ: lửa ở đầu + từng đốt, điểm, rơi 5–8 vật khi lửa tắt (+ multiplier nếu trúng nước thánh) rồi Kill.</summary>
    protected override void Die()
    {
        defeated = true;
        var cfg = World.Config;
        var h = HeadPos;
        FxSprite.Spawn(World, "hit", h.x, h.y, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        var flame = FxSprite.Spawn(World, "death", h.x, h.y);
        for (int i = 0; i < seg.Length; i++)
            FxSprite.Spawn(World, "death", X + (Facing > 0 ? -seg[i].x : seg[i].x), Y + seg[i].y);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HitFlame);
        if (Score > 0)
        {
            GameRules.AddScore(Score);
            GameSession.Current.NotifyChanged();
        }
        int n = Roll(dropMin, dropSpan);
        var items = new eItemId[n + 1];
        for (int i = 0; i < n; i++)
            items[i] = Roll(0, 99) < dropMoneyPct ? eItemId.RedMoneyBag : eItemId.SmallHeart;
        var s = GameSession.Current;
        items[n] = holyHit && s.shotMultiplier < 3 ? (s.shotMultiplier >= 2 ? eItemId.TripleShot : eItemId.DoubleShot) : eItemId.None;
        // rải dọc cổ: vật i ở tâm đốt (i mod N) — ô 16 căn giữa tâm đốt (pivot = tâm + 8), rơi xuống sàn (ItemGravity)
        var px = new int[n];
        var py = new int[n];
        for (int i = 0; i < n; i++)
        {
            var sg = seg.Length > 0 ? seg[i % seg.Length] : Vector2Int.zero;
            px[i] = X + (Facing > 0 ? -sg.x : sg.x);
            py[i] = Y + sg.y + 8;
        }
        int hx = h.x, hy = h.y + 8;
        if (flame != null)
            flame.onDone = f =>
            {
                for (int i = 0; i < n; i++)
                    ItemDrops.Spawn(f.World, items[i], px[i], py[i]);
                if (items[n] != eItemId.None)
                    ItemDrops.Spawn(f.World, items[n], hx, hy);
            };
        Kill();
        RaiseDefeated();
    }

    protected override void ApplyVisibility()
    {
        base.ApplyVisibility();
        bool on = Touch.active;
        for (int i = 0; i < neckHits.Length; i++)
            neckHits[i].active = on && i < segments;
    }

    public override void SyncView()
    {
        base.SyncView();
        if (view != null && view.transform != transform)
            view.transform.localPosition = Local(head);
        for (int i = 0; i < neckViews.Length; i++)
        {
            var r = neckViews[i];
            if (r == null)
                continue;
            bool on = i < seg.Length && Alive;
            r.enabled = on;
            if (!on)
                continue;
            if (boneSprite != null)
                r.sprite = boneSprite;
            r.flipX = Facing > 0;
            r.transform.localPosition = Local(seg[i]);
        }
    }

    /// <summary>Lệch local (px, quay trái) → transform con (lật theo hướng, pixel nguyên).</summary>
    private Vector3 Local(Vector2Int v)
    {
        int x = Facing > 0 ? -v.x : v.x;
        return new Vector3(x / (float)NesCoord.PPU, -v.y / (float)NesCoord.PPU, 0f);
    }

    protected override void BuildStates()
    {
        Fsm = new StateMachine<BoneDragon, eBoneDragonState>(this, "BoneDragon");
        Fsm.Add(eBoneDragonState.Idle, new IdleState());
        Fsm.Add(eBoneDragonState.Warn, new WarnState());
        Fsm.Add(eBoneDragonState.Fire, new FireState());
        if (animSet != null)
        {
            aHead = animSet.Get("head");
            var b = animSet.Get("bone");
            boneSprite = b != null && b.frames != null && b.frames.Length > 0 ? b.frames[0] : null;
        }
    }

    protected override void LoadTuning(GameConfig cfg)
    {
        var id = enemyId;
        segments = Mathf.Clamp(EnemyParams.Int(cfg, id, "segments", 6), 1, Mathf.Max(1, neckViews.Length));
        spacing = EnemyParams.Int(cfg, id, "segmentSpacingPx", 8);
        headGap = EnemyParams.Int(cfg, id, "headGapPx", 10);
        amplitude = EnemyParams.Int(cfg, id, "headAmplitudePx", 24);
        period = Mathf.Max(1, EnemyParams.Int(cfg, id, "wavePeriodTicks", 128));
        lag = EnemyParams.Int(cfg, id, "waveLagTicks", 6);
        firstIdle = EnemyParams.Int(cfg, id, "firstIdleTicks", 64);
        idleMin = EnemyParams.Int(cfg, id, "idleMinTicks", 96);
        idleSpan = EnemyParams.Int(cfg, id, "idleSpanTicks", 64);
        warnTicks = EnemyParams.Int(cfg, id, "warnTicks", 16);
        burst = Mathf.Max(1, EnemyParams.Int(cfg, id, "burstCount", 2));
        burstGap = Mathf.Max(1, EnemyParams.Int(cfg, id, "burstGapTicks", 20));
        fireSpeed = EnemyParams.Fixed256(cfg, id, "fireballSpeed", 1.5f);
        aimAbove = EnemyParams.Int(cfg, id, "aimAbovePx", 16);
        mouthFront = EnemyParams.Int(cfg, id, "mouthFrontPx", 6);
        dropMin = EnemyParams.Int(cfg, id, "dropMinCount", 5);
        dropSpan = EnemyParams.Int(cfg, id, "dropSpanCount", 3);
        dropMoneyPct = EnemyParams.Int(cfg, id, "dropMoneyPct", 50);
        InvincibleOnHit = EnemyParams.Int(cfg, id, "hitInvincibleTicks", 16);
        multiHit = true;
        checkOffscreen = false;   // gắn tường — SpawnPoint lo
        holyHit = false;
        shots = volleys = burstLeft = 0;
        waveT = 0;
        idleLen = firstIdle;
        Shape();
        Fsm.SetState(eBoneDragonState.Idle);
    }

    protected override void TickStates()
    {
        waveT++;
        Shape();
        Fsm.Tick();
    }
}
