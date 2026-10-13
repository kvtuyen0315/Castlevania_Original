using UnityEngine;

/// <summary>
/// Xương Skeleton ném (CVGP 17) — nhóm EnemyShot, hộp EnemyAttack (sát thương = DamageToPlayer), bay vòng cung vx/vy/gravity do
/// Skeleton đặt (ước lượng — wiki chỉ nói "vòng cung, các đường khác nhau"), xuyên địa hình, xoay (anim bone), gỡ khi ra khỏi màn /
/// rơi quá đáy section / trúng Simon. Wiki không nói xương phá được ⇒ KHÔNG có hộp nhận đòn. Toạ độ = tâm sprite.
/// </summary>
public class SkeletonBone : Entity
{
    public static readonly RectInt BOX = new RectInt(-7, -7, 14, 14);   // hợp bbox pixel đục bone_0..2 (XẤP XỈ — xương xoay)

    public Hitbox Hit { get; private set; }
    [System.NonSerialized] public int gravity;

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
    }

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        gravity = 0;
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("bone"), true);
        }
    }

    public override void OnTick()
    {
        fx += vx;
        vy += gravity;
        fy += vy;
        if (World.Tiles != null && Y > World.Tiles.Height + 16)
            Kill(eKillReason.Offscreen);
        base.OnTick();
    }

    public override void OnHitLanded(ref HitInfo hit)
    {
        Kill();
    }
}
