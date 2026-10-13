/// <summary>
/// Giải va chạm chiến đấu tập trung 1 chỗ/tick (bước 6 của StageWorld — CVGP_PLAN §5.5). Duyệt hitbox theo thứ tự nhóm
/// rồi thứ tự đăng ký (tất định), so với mọi hurtbox được phép theo ma trận:
///   PlayerAttack → Enemy, Breakable      EnemyAttack → Player      EnemyBody → Player
/// Điều kiện trúng: cả 2 còn sống, khác chủ, hurtbox bật, victim hết bất tử, hitbox chưa trúng victim trong đòn này,
/// AABB px chồng nhau. Trúng ⇒ victim.OnHurt (true = nhận) ⇒ bất tử InvincibleOnHit + attacker.OnHitLanded + StageWorld.Hit.
/// Mỗi hitbox trúng mỗi victim tối đa 1 hurtbox/tick. Không cấp phát.
/// </summary>
public class CombatResolver
{
    // [hitLayer, hurtLayer]
    private static readonly bool[,] MATRIX =
    {
        //            Player Enemy  Breakable
        /* PlayerAttack */ { false, true, true },
        /* EnemyAttack  */ { true, false, false },
        /* EnemyBody    */ { true, false, false },
    };

    public static bool CanHit(eHitLayer hit, eHurtLayer hurt)
    {
        return MATRIX[(int)hit, (int)hurt];
    }

    /// <summary>Số đòn đã giải từ lúc tạo (debug/kiểm thử).</summary>
    public int TotalHits { get; private set; }

    public void Resolve(StageWorld world)
    {
        for (int ga = 0; ga < Entity.GROUP_COUNT; ga++)
        {
            var attackers = world.GroupList(ga);
            for (int ia = 0; ia < attackers.Count; ia++)
            {
                var a = attackers[ia];
                if (!a.Alive)
                    continue;
                var hbs = a.Hitboxes;
                for (int h = 0; h < hbs.Count; h++)
                {
                    var hb = hbs[h];
                    if (hb.active)
                        ResolveHitbox(world, a, hb);
                }
            }
        }
    }

    private void ResolveHitbox(StageWorld world, Entity a, Hitbox hb)
    {
        var hr = a.ToSection(hb.local);
        for (int gv = 0; gv < Entity.GROUP_COUNT; gv++)
        {
            var victims = world.GroupList(gv);
            for (int iv = 0; iv < victims.Count; iv++)
            {
                var v = victims[iv];
                if (v == a || !v.Alive || v.InvincibleTicks > 0 || hb.HasHit(v))
                    continue;
                var hus = v.Hurtboxes;
                for (int u = 0; u < hus.Count; u++)
                {
                    var hu = hus[u];
                    if (!hu.active || !CanHit(hb.layer, hu.layer))
                        continue;
                    if (hu.layer == eHurtLayer.Player && CvgpDebug.CheatInvincible)
                        continue;
                    var vr = v.ToSection(hu.local);
                    if (!Overlaps(hr, vr))
                        continue;

                    hb.MarkHit(v);
                    var info = new HitInfo
                    {
                        attacker = a,
                        victim = v,
                        hitbox = hb,
                        hurtbox = hu,
                        damage = hb.damage,
                        strength = hb.strength,
                        knockbackDir = v.X > a.X ? 1 : v.X < a.X ? -1 : -v.Facing,
                        tick = world.TickCount,
                    };
                    if (v.OnHurt(ref info))
                    {
                        if (v.InvincibleOnHit > 0)
                            v.InvincibleTicks = v.InvincibleOnHit;
                        TotalHits++;
                        a.OnHitLanded(ref info);
                        world.RaiseHit(ref info);
                    }
                    break;
                }
                if (!a.Alive || !hb.active)
                    return;
            }
        }
    }

    /// <summary>AABB px nửa mở [min, max) — chạm cạnh (không chung pixel) KHÔNG tính.</summary>
    public static bool Overlaps(UnityEngine.RectInt a, UnityEngine.RectInt b)
    {
        return a.x < b.x + b.width && b.x < a.x + a.width && a.y < b.y + b.height && b.y < a.y + a.height;
    }
}
