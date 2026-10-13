using UnityEngine;

/// <summary>
/// Rìu Axe Knight (CVGP 18) — nhóm EnemyShot, hộp EnemyAttack (sát thương = DamageToPlayer), bay NGANG như Boomerang (wiki Enemies#Axe
/// Knights: "bay đi một đoạn rồi quay về, y hệt Boomerang của Simon"): vx đầu = hướng·speed, giảm đều turnAccel/tick tới 0 rồi tăng
/// ngược lại tới speed, giữ độ cao ném (cao/thấp). Quay về chạm lại cột của Knight (Knight còn sống) ⇒ Knight bắt lại (gỡ — SUY LUẬN);
/// không thì bay tiếp tới khi ra khỏi màn. Xuyên địa hình. QUẤT VỠ ĐƯỢC (wiki "quất rìu để nó không quay về"): Hurtbox lớp Enemy ⇒
/// tia lửa + ConfigScore.EnemyProjectilePoints (100 — SUY LUẬN như cầu lửa Bone Tower 17). Toạ độ = tâm sprite 16×16; anim "axe" xoay.
/// </summary>
public class AxeKnightAxe : Entity
{
    public static readonly RectInt BOX = new RectInt(-7, -7, 14, 14);   // hợp bbox pixel đục axeman_axe_0..3 (XẤP XỈ — rìu xoay)

    public Hitbox Hit { get; private set; }
    public Hurtbox Hurt { get; private set; }
    [System.NonSerialized] public int speed, turnAccel, dir;
    [System.NonSerialized] public AxeKnight owner;
    private int ownerSpawnTick;
    /// <summary>Bị Simon phá / đã quay đầu / được Knight bắt lại (harness).</summary>
    [System.NonSerialized] public bool broken, returning, caught;

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
        Hurt = AddHurtbox(eHurtLayer.Enemy, BOX);
    }

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        Hurt.active = true;
        broken = returning = caught = false;
        owner = null;
        speed = turnAccel = 0;
        dir = Facing;
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("axe"), true);
        }
    }

    /// <summary>Knight ném: bay theo hướng d (−1/+1).</summary>
    public void Launch(AxeKnight k, int d, int spd, int accel)
    {
        owner = k;
        ownerSpawnTick = k != null ? k.SpawnTick : -1;
        dir = d;
        speed = spd;
        turnAccel = Mathf.Max(1, accel);
        vx = d * spd;
    }

    public bool OwnerAlive => owner != null && owner.Alive && owner.SpawnTick == ownerSpawnTick;

    public override void OnTick()
    {
        int before = X;
        // giảm tốc theo hướng ném, qua 0 thì tăng tốc ngược lại tới speed
        vx -= dir * turnAccel;
        if (vx * dir <= 0)
            returning = true;
        if (Mathf.Abs(vx) > speed)
            vx = (vx > 0 ? 1 : -1) * speed;
        fx += vx;
        if (returning && OwnerAlive)
        {
            int ox = owner.X;
            if ((before - ox) * (X - ox) <= 0)
            {
                caught = true;
                Kill();
            }
        }
        base.OnTick();
    }

    public override void OnHitLanded(ref HitInfo hit)
    {
        Kill();
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
