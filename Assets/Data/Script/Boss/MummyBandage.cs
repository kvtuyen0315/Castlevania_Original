using UnityEngine;

/// <summary>
/// Băng vải Mummy Man ném (CVGP 19, theo wiki — GẦN ĐÚNG). Wiki Bosses#Mummy Men + Stages 7-9: "thỉnh thoảng ném băng gạc vào Simon —
/// băng gây sát thương". Nhóm EnemyShot, hộp EnemyAttack (sát thương = DamageToPlayer), bay NGANG vx do Mummy đặt (ước lượng), xuyên địa
/// hình, gỡ khi ra khỏi màn / trúng Simon. Wiki không nói phá được ⇒ KHÔNG có hộp nhận đòn (như xương Skeleton 17). Stopwatch: Mummy Men
/// miễn nhiễm ("Stopwatch vô dụng") ⇒ băng cũng không bị giữ (SUY LUẬN — cả đòn tấn công của boss). Toạ độ = tâm sprite (bandage 16×16).
/// </summary>
public class MummyBandage : Entity
{
    public static readonly RectInt BOX = new RectInt(-8, -3, 16, 9);   // bbox pixel đục bandage_0/1 (hàng 5–13, rộng 16)

    public Hitbox Hit { get; private set; }

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.EnemyAttack, BOX);
    }

    protected override void OnSpawned()
    {
        Hit.damage = World.DamageToPlayer();
        Hit.Activate(BOX);
        if (Anim != null && animSet != null)
        {
            Anim.Visible = true;
            Anim.Play(animSet.Get("bandage"), true);
        }
    }

    public override bool IsFrozenBy(bool worldFrozen, bool enemyFrozen)
    {
        return worldFrozen;
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
