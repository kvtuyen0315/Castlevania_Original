using System;
using UnityEngine;

/// <summary>
/// Nến tường / chậu lửa (CVGP 11) — nhóm Prop, Hurtbox Breakable (roi + vũ khí phụ — CombatResolver PlayerAttack → Breakable).
/// Pivot = đáy-giữa sprite, vẽ viewOffset (0,1) như Simon ⇒ chậu lửa đặt Y = mặt sàn (video: chậu CE hàng 161–192 NES khi sàn 192).
/// Lửa 2 khung đổi mỗi 9 tick, MỌI vật cùng pha (đồng hồ thế giới — video Map_1 f68–166: 2 chậu đổi cùng frame, TB 8.9 tick/khung).
/// Trúng đòn: tắt hộp + ẩn, sinh tia lửa "hit" (HitSparkTicks) + lửa "death" (FX, tâm vật) + SFX HitFlame; sau BreakItemDelayTicks
/// (≈ lúc lửa tắt — 5 chậu video) sinh item: ô item căn giữa tâm vật (chậu: đỉnh ô = đỉnh chậu + 8; nến: trùng ô nến).
/// Nội dung: ConfigDrop[objectId] hoặc <see cref="content"/> (mặc định nến tường = tim nhỏ) → ItemDrops.Resolve (Morning Star / multiplier).
/// </summary>
public class CandleProp : Entity
{
    public const int FLAME_FRAME_TICKS = 9;   // video Map_1 f68–166: 11 chu kỳ / 98 frame = 17.8 tick/chu kỳ (đo routine 11)

    [Tooltip("Khoá ConfigDrop (routine 15 đặt, vd CE_T2). Trống ⇒ dùng content.")]
    public string objectId;
    [Tooltip("Nội dung khi ConfigDrop không có dòng objectId.")]
    public eItemId content = eItemId.SmallHeart;
    [SerializeField] private string animName = "candle";
    [Tooltip("Cao sprite (px): nến 16, chậu 32 — tâm vật = Y − H/2 + 1.")]
    [SerializeField] private int height = 16;

    public Hurtbox Hurt { get; private set; }
    [NonSerialized] public bool broken;
    [NonSerialized] public int breakAge, breakDelay;
    [NonSerialized] public eItemId dropped;
    /// <summary>Item đã sinh (harness đọc).</summary>
    [NonSerialized] public ItemPickup spawnedItem;
    private AnimSet.Anim anim;

    public override string DebugState => (broken ? "Broken t" + breakAge : "Idle") + " " + objectId;
    public int Height => height;

    protected override void OnBuild()
    {
        Hurt = AddHurtbox(eHurtLayer.Breakable, new RectInt(-8, -height, 16, height));
        if (animSet != null)
            anim = animSet.Get(animName);
    }

    protected override void OnSpawned()
    {
        broken = false;
        breakAge = 0;
        dropped = eItemId.None;
        spawnedItem = null;
        Hurt.active = true;
        Hurt.local = new RectInt(-body.width / 2, -height, body.width, height);
        var cfg = World.Config;
        breakDelay = cfg != null ? cfg.rules.GetInt(eRuleParam.BreakItemDelayTicks, 20) : 20;
        if (Anim != null)
            Anim.Visible = true;
    }

    public override bool OnHurt(ref HitInfo hit)
    {
        if (broken)
            return false;
        broken = true;
        breakAge = 0;
        Hurt.active = false;
        if (Anim != null)
            Anim.Visible = false;
        var cfg = World.Config;
        int cy = Y - height / 2 + 1;
        FxSprite.Spawn(World, "hit", X, cy, cfg != null ? cfg.rules.GetInt(eRuleParam.HitSparkTicks, 4) : 4);
        FxSprite.Spawn(World, "death", X, cy);
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HitFlame);
        return true;
    }

    public override void OnTick()
    {
        if (!broken)
            return;
        breakAge++;
        if (breakAge < breakDelay)
            return;
        dropped = ItemDrops.Resolve(World, ItemDrops.ContentOf(World, objectId, content));
        spawnedItem = ItemDrops.Spawn(World, dropped, X, Y - (height - 16) / 2);
        Kill();
    }

    public override void SyncView()
    {
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
        if (Anim == null || anim == null || broken)
            return;
        int f = World != null ? (World.TickCount / FLAME_FRAME_TICKS) % 2 : 0;
        Anim.SetFrame(anim, f);
        Anim.Apply(-1);
    }
}
