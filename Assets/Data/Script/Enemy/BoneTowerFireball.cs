using UnityEngine;

/// <summary>
/// Cầu lửa Bone Tower (CVGP 17) — nhóm EnemyShot, hộp EnemyAttack (sát thương = DamageToPlayer), bay ngang vx (ước lượng), xuyên địa
/// hình, gỡ khi ra khỏi màn / trúng Simon. QUẤT VỠ ĐƯỢC (wiki Enemies#Bone Towers): có Hurtbox lớp Enemy ⇒ mọi đòn của Simon (roi, vũ
/// khí phụ) phá ⇒ tia lửa "hit" + ConfigScore.EnemyProjectilePoints (wiki Gameplay#Points "một số đạn đáng 100 điểm" — đạn nào wiki
/// không nói; gán cho đạn phá được là SUY LUẬN). Toạ độ = pivot sprite (tâm ô 8×16; quả cầu ở nửa trên).
/// </summary>
public class BoneTowerFireball : Entity
{
    public static readonly RectInt BOX = new RectInt(-4, -7, 7, 6);   // bbox pixel đục dragonskull_fireball (pivot tâm ô)

    public Hitbox Hit { get; private set; }
    public Hurtbox Hurt { get; private set; }
    /// <summary>Bị Simon phá (harness).</summary>
    [System.NonSerialized] public bool broken;

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
        broken = false;
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("fireball"), true);
        }
    }

    public override void OnTick()
    {
        fx += vx;
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
        FxSprite.Spawn(World, "hit", X, Y - 4, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 16) : 16);
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
