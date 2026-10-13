using System;
using UnityEngine;

/// <summary>
/// Vũ khí phụ đã ném của Simon (CVGP 09) — thực thể nhóm PlayerWeapon, KHÔNG Update (StageWorld tick, H4).
/// Toạ độ = TÂM vật (px section, y xuống) — khác pivot "mặt sàn" của nhân vật; lửa nước thánh = đáy-giữa (mặt sàn).
/// Hitbox PlayerAttack = bbox pixel đục của sprite (builder đo từ PNG — sheet là chuẩn pixel, H1), quy ước quay TRÁI
/// như mọi hộp (Entity); sprite vũ khí trên sheet vẽ hướng PHẢI (khớp mẫu video: ném phải = sprite không lật) ⇒ spriteFacesRight.
/// Số liệu: ConfigSubWeapon (giá tim, mạnh/yếu, xuyên, tốc độ, trọng lực) + ConfigActorParam "SubWeapon.X" (độ cao sinh…).
/// Đếm cho luật chơi: GameSession.subWeaponHits (wiki: 10 lần trúng ⇒ rơi multiplier — routine 11), hạ ≥ 2 enemy trong
/// 1 lần ném ⇒ ConfigScore.MultiKill2/3/4 (wiki Gameplay#Points) khi vật rời thế giới.
/// </summary>
public abstract class SubWeaponProjectile : Entity
{

    [SerializeField] protected eSubWeapon kind;
    [SerializeField] protected string animName;
    [Tooltip("Hộp đánh quanh pivot (quay TRÁI) — CVGP/Build Weapon Prefabs đo từ bbox pixel đục của sprite.")]
    [SerializeField] protected RectInt hitRect = new RectInt(-8, -8, 16, 16);
    [SerializeField] protected bool spriteFacesRight = true;
    [Tooltip("Tính vào giới hạn số vũ khí trên màn (1 / Double 2 / Triple 3).")]
    [SerializeField] protected bool countsAsShot = true;

    public Hitbox Hit { get; private set; }
    public eSubWeapon Kind => kind;
    public bool CountsAsShot => countsAsShot;
    public RectInt HitRect => hitRect;
    /// <summary>Simon đã ném (đặt ngay sau World.Spawn).</summary>
    [NonSerialized] public SimonController Thrower;
    [NonSerialized] public int kills, hits, age;

    protected ConfigSubWeaponRow row;
    protected bool pierces;
    protected AnimSet.Anim aMain;

    public override string DebugState => kind + " t" + age;

    protected override void OnBuild()
    {
        Hit = AddHitbox(eHitLayer.PlayerAttack, hitRect);
        Hit.singleHit = true;
        if (animSet != null && !string.IsNullOrEmpty(animName))
            aMain = animSet.Get(animName);
    }

    protected override void OnSpawned()
    {
        kills = hits = age = 0;
        Thrower = null;
        var cfg = World.Config;
        row = cfg != null ? cfg.GetSubWeapon(kind) : null;
        pierces = row != null && row.pierces.has && row.pierces.value != 0;
        Hit.strength = row != null ? row.strength : eHitStrength.Weak;
        Hit.damage = 0;
        Hit.Activate(hitRect);
        offscreenMargin = Mathf.RoundToInt(Param(cfg, "offscreenMarginPx", 8));
        if (Anim != null && aMain != null)
        {
            Anim.Visible = true;
            Anim.Play(aMain, true);
        }
        OnLaunch(cfg);
    }

    /// <summary>Đọc số liệu + vận tốc đầu (vx theo Facing).</summary>
    protected abstract void OnLaunch(GameConfig cfg);

    /// <summary>Di chuyển 1 tick.</summary>
    protected abstract void Move();

    public override void OnTick()
    {
        age++;
        Move();
        // rơi khỏi đáy section (rìu, lọ) ⇒ biến mất (despawnOffscreen chỉ xét x)
        if (Alive && World.Tiles != null && Y - 16 > World.Tiles.Height)
            Kill(eKillReason.Offscreen);
        base.OnTick();
    }

    public override void OnHitLanded(ref HitInfo hit)
    {
        hits++;
        if (hit.victim != null && hit.victim.Group == eEntityGroup.Enemy)
        {
            GameSession.Current.subWeaponHits++;
            if (!hit.victim.Alive)
                kills++;
        }
        if (!pierces)
            OnSpent();
    }

    /// <summary>Trúng mục tiêu mà không xuyên (dao): mặc định biến mất.</summary>
    protected virtual void OnSpent()
    {
        Kill();
    }

    protected override void OnRemoved()
    {
        // wiki Gameplay#Points: 1 lần ném hạ 2/3/4 enemy ⇒ 1000/2000/4000 (không tính roi)
        if (kills >= 2 && World != null && World.Config != null && World.Config.score != null)
        {
            var k = kills >= 4 ? eScoreParam.MultiKill4 : kills == 3 ? eScoreParam.MultiKill3 : eScoreParam.MultiKill2;
            GameSession.Current.score += World.Config.score.GetInt(k);
            GameSession.Current.NotifyChanged();
        }
        Hit.Deactivate();
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
        if (Anim != null)
            Anim.Apply(spriteFacesRight ? -Facing : Facing);
    }

    // ---------------------------------------------------------------- tiện ích Config

    public static float Param(GameConfig cfg, string actor, string key, float fallback)
    {
        if (cfg == null || cfg.actorParams == null)
            return fallback;
        var r = cfg.actorParams.Find(actor, key);
        return r != null && r.value.has ? r.value.value : fallback;
    }

    protected float Param(GameConfig cfg, string key, float fallback)
    {
        return Param(cfg, ConfigActorParam.ActorKey(kind), key, fallback);
    }

    protected int Speed(CfgFloat v, string what)
    {
        return v.has ? Fixed.FromFloat(v.value) : 0;
    }

    /// <summary>Mặt trên ô đặc đầu tiên ở cột x, từ y xuống tối đa maxDown px (không có ⇒ −1).</summary>
    protected int FloorBelow(int x, int y, int maxDown)
    {
        var t = World.Tiles;
        for (int yy = Mathf.Max(0, y); yy <= y + maxDown && yy < t.Height; yy++)
            if (t.IsSolid(x, yy))
                return yy;
        return -1;
    }
}
