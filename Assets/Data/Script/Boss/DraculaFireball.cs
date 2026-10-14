using UnityEngine;

/// <summary>
/// Cầu lửa Dracula (CVGP 20) — 2 dạng dùng chung lớp (prefab DraculaFireball: sprite dracula1_fireball; DemonFireball: dracula2_fireball).
/// = cầu lửa Bone Tower (17: EnemyShot, EnemyAttack sát thương mức stage, bay thẳng (vx, vy), xuyên địa hình, QUẤT VỠ được +100) thêm:
/// wiki Stages 16-18#Stage 18 "cầu lửa ông ta bắn đôi khi sinh ra TIM khi bị phá" ⇒ phá ⇒ heartPct % rơi tim nhỏ (RNG thế giới — tỉ lệ
/// ƯỚC LƯỢNG, ConfigActorParam Boss.Dracula.fireballHeartPct). Toạ độ = pivot sprite (tâm ô 8×16; quả cầu ở nửa trên như cầu lửa 17).
/// </summary>
public class DraculaFireball : BoneTowerFireball
{
    [Tooltip("Tên anim trong AnimSet Dracula (p1_fireball / p2_fireball).")]
    [SerializeField] private string animName = "p1_fireball";

    [System.NonSerialized] public int heartPct;
    /// <summary>Phá ra tim (harness).</summary>
    [System.NonSerialized] public bool droppedHeart;

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        Hurt.active = true;
        broken = false;
        droppedHeart = false;
        heartPct = BossParams.Int(World.Config, eBossId.Dracula, "fireballHeartPct", 25);
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get(animName), true);
        }
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (!base.OnHurt(ref hit))
            return false;
        if (World.Rng.Next(100) < heartPct)
        {
            droppedHeart = true;
            ItemDrops.Spawn(World, eItemId.SmallHeart, X, Y);
        }
        return true;
    }
}
