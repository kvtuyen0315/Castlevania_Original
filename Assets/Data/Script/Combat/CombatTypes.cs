using System.Collections.Generic;
using UnityEngine;

/// <summary>Lớp hộp GÂY sát thương.</summary>
public enum eHitLayer
{
    PlayerAttack = 0,  // roi, vũ khí phụ, đạn của Simon
    EnemyAttack = 1,   // đạn/vũ khí của enemy, boss
    EnemyBody = 2,     // chạm thân enemy/boss
}

/// <summary>Lớp hộp NHẬN đòn.</summary>
public enum eHurtLayer
{
    Player = 0,
    Enemy = 1,
    Breakable = 2,     // nến, đuốc, khối gạch phá được
}

/// <summary>
/// Hộp gây sát thương (AABB px quanh pivot chủ, quay TRÁI như sheet — lật theo Facing). Bật/tắt theo khung animation
/// (state code gọi Activate ở khung đánh, Deactivate khi hết). Mỗi lần Activate = 1 "đòn": trúng mỗi mục tiêu tối đa 1 lần
/// (singleHit = true — roi, vũ khí). Hộp thân enemy (singleHit = false) đánh lại được khi mục tiêu hết bất tử.
/// </summary>
public class Hitbox
{
    public readonly Entity owner;
    public readonly eHitLayer layer;
    public RectInt local;
    public bool active;
    /// <summary>Sát thương dạng số (vạch máu Simon — ConfigBlock.damageToSimon) — bên nhận tự diễn giải.</summary>
    public int damage;
    /// <summary>Đòn yếu/mạnh (wiki Enemies) — enemy/boss trừ máu theo CombatHealth.</summary>
    public eHitStrength strength = eHitStrength.Weak;
    public bool singleHit = true;
    /// <summary>Số lần Activate (mã đòn).</summary>
    public int Activation { get; private set; }

    private readonly List<Entity> victims = new List<Entity>(4);

    public Hitbox(Entity owner, eHitLayer layer, RectInt local)
    {
        this.owner = owner;
        this.layer = layer;
        this.local = local;
    }

    /// <summary>Bắt đầu 1 đòn mới với hộp local (xoá danh sách đã trúng).</summary>
    public void Activate(RectInt rect)
    {
        local = rect;
        active = true;
        Activation++;
        victims.Clear();
    }

    public void Deactivate()
    {
        active = false;
    }

    public void ResetHits()
    {
        victims.Clear();
    }

    public bool HasHit(Entity e)
    {
        if (!singleHit)
            return false;
        for (int i = 0; i < victims.Count; i++)
            if (victims[i] == e)
                return true;
        return false;
    }

    internal void MarkHit(Entity e)
    {
        if (singleHit)
            victims.Add(e);
    }

    public RectInt SectionRect => owner.ToSection(local);
}

/// <summary>Hộp nhận đòn (AABB px quanh pivot chủ, quay TRÁI — lật theo Facing).</summary>
public class Hurtbox
{
    public readonly Entity owner;
    public readonly eHurtLayer layer;
    public RectInt local;
    public bool active = true;

    public Hurtbox(Entity owner, eHurtLayer layer, RectInt local)
    {
        this.owner = owner;
        this.layer = layer;
        this.local = local;
    }

    public RectInt SectionRect => owner.ToSection(local);
}

/// <summary>1 đòn đã được giải (CombatResolver → victim.OnHurt → attacker.OnHitLanded → StageWorld.Hit).</summary>
public struct HitInfo
{
    public Entity attacker;
    public Entity victim;
    public Hitbox hitbox;
    public Hurtbox hurtbox;
    public int damage;
    public eHitStrength strength;
    /// <summary>Hướng đẩy lùi victim: +1 sang phải / −1 sang trái = phía rời xa attacker (cùng x ⇒ ngược hướng mặt victim).
    /// Simon gốc lùi NGƯỢC HƯỚNG MẶT (MAP1_TIMELINE §4) — routine 08 chọn dùng cái nào.</summary>
    public int knockbackDir;
    public int tick;
}

/// <summary>
/// Máu enemy/boss theo 2 số wiki (đòn yếu W / mạnh S để hạ — CVGP 04 §4 "routine 07 chốt"):
/// HP = W × S; đòn yếu −S, đòn mạnh −W ⇒ đúng W đòn yếu hoặc S đòn mạnh, trộn thì cộng dồn tỉ lệ.
/// Kiểm Phantom Bat 12/8: HP 96, rìu (mạnh) −12 = 1/8 ⇒ thanh ENEMY 16 vạch mất 2 vạch/rìu — khớp video Map_1 3:24–3:30.
/// </summary>
public class CombatHealth
{
    public int Max { get; private set; }
    public int Current { get; private set; }
    public int HitsWeak { get; private set; }
    public int HitsStrong { get; private set; }

    public bool IsDead => Current <= 0;

    public void Reset(int hitsWeak, int hitsStrong)
    {
        HitsWeak = Mathf.Max(1, hitsWeak);
        HitsStrong = Mathf.Max(1, hitsStrong);
        Max = HitsWeak * HitsStrong;
        Current = Max;
    }

    public static int DamageFor(eHitStrength s, int hitsWeak, int hitsStrong)
    {
        return s == eHitStrength.Strong ? Mathf.Max(1, hitsWeak) : s == eHitStrength.Weak ? Mathf.Max(1, hitsStrong) : 0;
    }

    /// <summary>Trừ máu theo loại đòn; trả lượng đã trừ.</summary>
    public int Apply(eHitStrength s)
    {
        int d = Mathf.Min(Current, DamageFor(s, HitsWeak, HitsStrong));
        Current -= d;
        return d;
    }

    /// <summary>Đặt máu hiện tại (cheat "Boss HP 1" — CVGP 14), kẹp [0, Max].</summary>
    public void Set(int hp)
    {
        Current = Mathf.Clamp(hp, 0, Max);
    }

    /// <summary>Số vạch hiển thị (thanh ENEMY 16 vạch) = làm tròn LÊN.</summary>
    public int Bars(int segments)
    {
        if (Max <= 0)
            return 0;
        return (Current * segments + Max - 1) / Max;
    }
}
