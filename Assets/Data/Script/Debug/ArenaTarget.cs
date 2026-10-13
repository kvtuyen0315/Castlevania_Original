using UnityEngine;

/// <summary>
/// Bia thử của scene Arena (CVGP 07) — kiểm combat + spawn, KHÔNG phải enemy thật (Zombie thật: routine 13).
/// Hurtbox nhóm Enemy (16×32), hitbox thân EnemyBody (đánh lại được khi Simon hết bất tử) gây DamageToPlayer() vạch.
/// Máu theo mô hình CombatHealth (HP = W × S) với W/S đặt ở Inspector (mặc định 12/8 = Phantom Bat wiki) ⇒ thanh 16 vạch.
/// Hình: khung 0 anim "walk" của ZombieAnimSet chỉ để nhìn.
/// </summary>
public class ArenaTarget : Entity
{
    public static readonly RectInt HURT_BOX = new RectInt(-8, -32, 16, 32);
    public static readonly RectInt TOUCH_BOX = new RectInt(-6, -30, 12, 30);

    [SerializeField] private int hitsWeak = 12;
    [SerializeField] private int hitsStrong = 8;
    [Tooltip("Bật hitbox thân (chạm Simon mất máu).")]
    public bool touchDamage = true;

    public readonly CombatHealth Health = new CombatHealth();
    public Hurtbox Hurt { get; private set; }
    public Hitbox Touch { get; private set; }
    public int HitsTaken;

    public override string DebugState => "HP " + Health.Current + "/" + Health.Max + " (" + Health.Bars(GameSession.MAX_HP) + " vạch)";

    protected override void OnBuild()
    {
        Hurt = AddHurtbox(eHurtLayer.Enemy, HURT_BOX);
        Touch = AddHitbox(eHitLayer.EnemyBody, TOUCH_BOX);
        Touch.singleHit = false;
    }

    protected override void OnSpawned()
    {
        Health.Reset(hitsWeak, hitsStrong);
        HitsTaken = 0;
        Touch.damage = World.DamageToPlayer();
        Touch.active = touchDamage;
        if (Anim != null && animSet != null)
            Anim.SetFrame(animSet.Get("walk"), 0);
    }

    public override void OnTick()
    {
        Touch.active = touchDamage;
        base.OnTick();
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        Health.Apply(hit.strength);
        HitsTaken++;
        if (Health.IsDead)
            Kill();
        return true;
    }
}
