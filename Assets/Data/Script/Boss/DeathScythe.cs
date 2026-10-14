using UnityEngine;

/// <summary>State lưỡi hái của Death.</summary>
public enum eScytheState
{
    Idle,  // vừa sinh: đứng yên (vẫn xoay) scytheIdleTicks
    Home,  // đuổi Simon, nhanh dần
}

/// <summary>
/// Lưỡi hái của Death (CVGP 20, theo wiki — GẦN ĐÚNG, không có video Block 5). Wiki Bosses#Death: "lưỡi hái đứng yên khoảng một giây rồi
/// chậm rãi đuổi theo Simon", "lúc đầu chậm, rồi nhanh dần"; "phá lưỡi hái ngay khi vừa tạo" (Stages 13-15: "quất lưỡi hái trước");
/// "Death chết rồi lưỡi hái VẪN CÒN — có thể chết khi đi nhặt quả cầu". Nhóm EnemyShot, hộp EnemyAttack (sát thương = mức stage, đánh lại
/// khi Simon hết bất tử — không tự huỷ khi trúng: SUY LUẬN) + Hurtbox Enemy (mọi đòn của Simon phá, +100 như đạn phá được 17 — SUY LUẬN).
/// Ước lượng (ConfigActorParam Boss.Death.scythe* — ghi §2q):
///   Idle  scytheIdleTicks (60 = "một giây"), anim xoay.
///   Home  mỗi trục tăng tốc scytheAccel (1/64 px/tick² — như Ghost 17) về tâm thân Simon (chân − scytheAimAbovePx), trần tốc độ mỗi trục
///         tăng dần scytheCapStart → scytheCapMax (+scytheCapGrow mỗi tick) ⇒ "chậm rồi nhanh dần", lao quá rồi vòng lại; xuyên địa hình.
/// Stopwatch vô dụng (wiki: mọi boss sau Medusa miễn nhiễm — áp cả lưỡi hái, SUY LUẬN). Rosary gỡ (không phải boss). Nhặt quả cầu ⇒
/// lưỡi hái còn lại biến mất (tally — SUY LUẬN, wiki không nói).
/// </summary>
public class DeathScythe : Entity
{
    public static readonly RectInt BOX = new RectInt(-8, -8, 16, 16);   // bbox pixel đục scythe_0..3 (16×16 đục kín bbox, pivot tâm)

    public StateMachine<DeathScythe, eScytheState> Fsm { get; private set; }
    public Hitbox Hit { get; private set; }
    public Hurtbox Hurt { get; private set; }
    /// <summary>Bị Simon phá (harness).</summary>
    [System.NonSerialized] public bool broken;
    [System.NonSerialized] public int idleTicks, accel, capStart, capGrow, capMax, aimAbove, cap, age;

    public override string DebugState => Fsm != null && Fsm.HasState ? Fsm.CurState.ToString() : "-";

    abstract class ScytheState : BaseState<DeathScythe>
    {
        protected void SetState(eScytheState s) { owner.Fsm.SetState(s); }
    }

    class IdleState : ScytheState
    {
        public override void CheckNextState()
        {
            if (StateFrame >= owner.idleTicks)
                SetState(eScytheState.Home);
        }
    }

    class HomeState : ScytheState
    {
        public override void OnEnterState() { owner.cap = owner.capStart; }

        public override void OnExecuteState()
        {
            var p = owner.World.Player;
            int tx = p != null ? Fixed.FromPx(p.X) : owner.fx, ty = p != null ? Fixed.FromPx(p.Y - owner.aimAbove) : owner.fy;
            owner.vx = Mathf.Clamp(owner.vx + (tx > owner.fx ? owner.accel : tx < owner.fx ? -owner.accel : 0), -owner.cap, owner.cap);
            owner.vy = Mathf.Clamp(owner.vy + (ty > owner.fy ? owner.accel : ty < owner.fy ? -owner.accel : 0), -owner.cap, owner.cap);
            owner.fx += owner.vx;
            owner.fy += owner.vy;
            owner.cap = Mathf.Min(owner.capMax, owner.cap + owner.capGrow);
        }
    }

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
        Hit.singleHit = false;
        Hurt = AddHurtbox(eHurtLayer.Enemy, BOX);
        Fsm = new StateMachine<DeathScythe, eScytheState>(this, "DeathScythe");
        Fsm.Add(eScytheState.Idle, new IdleState());
        Fsm.Add(eScytheState.Home, new HomeState());
    }

    protected override void OnSpawned()
    {
        var cfg = World.Config;
        idleTicks = BossParams.Int(cfg, eBossId.Death, "scytheIdleTicks", 60);
        accel = BossParams.Fixed256(cfg, eBossId.Death, "scytheAccel", 1f / 64f);
        capStart = BossParams.Fixed256(cfg, eBossId.Death, "scytheCapStart", 0.5f);
        capGrow = BossParams.Fixed256(cfg, eBossId.Death, "scytheCapGrow", 1f / 256f);
        capMax = BossParams.Fixed256(cfg, eBossId.Death, "scytheCapMax", 1.5f);
        aimAbove = BossParams.Int(cfg, eBossId.Death, "scytheAimAbovePx", 16);
        cap = capStart;
        age = 0;
        vx = vy = 0;
        broken = false;
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        Hurt.active = true;
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("scythe"), true);
        }
        Fsm.SetState(eScytheState.Idle);
        ItemEffects.CrystalPicked -= OnCrystalPicked;   // thực thể tái dùng (pool) — không đăng ký 2 lần
        ItemEffects.CrystalPicked += OnCrystalPicked;
    }

    protected override void OnRemoved()
    {
        ItemEffects.CrystalPicked -= OnCrystalPicked;
        base.OnRemoved();
    }

    private void OnDestroy()
    {
        ItemEffects.CrystalPicked -= OnCrystalPicked;
    }

    private void OnCrystalPicked(StageWorld w)
    {
        if (w == World && Alive)
            Kill();
    }

    /// <summary>Stopwatch vô dụng (boss Block 5) — chỉ đóng băng thế giới (nhặt roi).</summary>
    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;
    }

    public override void OnTick()
    {
        age++;
        Fsm.Tick();
        base.OnTick();
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (!Alive)
            return false;
        broken = true;
        var cfg = World.Config;
        FxSprite.Spawn(World, "hit", X, Y, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
        int pts = cfg != null && cfg.score != null ? cfg.score.GetInt(eScoreParam.EnemyProjectilePoints) : 0;
        if (pts > 0)
        {
            GameRules.AddScore(pts);
            GameSession.Current.NotifyChanged();
        }
        Kill();
        return true;
    }
}
