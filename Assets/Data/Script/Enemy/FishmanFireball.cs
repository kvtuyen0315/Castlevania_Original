using UnityEngine;

/// <summary>
/// Cầu lửa của Fish Man (CVGP 13) — nhóm EnemyShot, hộp EnemyAttack (sát thương = DamageToPlayer), bay ngang vx (QUY ƯỚC — video
/// không có cảnh bắn), xuyên địa hình, gỡ khi ra khỏi màn (despawnOffscreen) hoặc khi trúng Simon. Toạ độ = tâm sprite.
/// </summary>
public class FishmanFireball : Entity
{
    public static readonly RectInt BOX = new RectInt(-4, -7, 7, 6);   // bbox pixel đục fishman_fireball (pivot tâm)

    public Hitbox Hit { get; private set; }

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
        if (Anim != null && animSet != null)
            Anim.Play(animSet.Get("fireball"), true);
    }

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
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
}
