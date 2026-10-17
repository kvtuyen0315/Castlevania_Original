using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item rơi / item bí mật (CVGP 11) — thực thể nhóm Item, KHÔNG Update (StageWorld tick, H4). 1 prefab cho mọi loại: Setup(id)
/// sau World.Spawn. Toạ độ: pivot = đáy ô 16 px của item; vẽ viewOffset (0,1) ⇒ ô vẽ hàng Y−15..Y — đứng yên thì Y = mặt sàn
/// (video Map_1: ô item nằm sàn hàng 177..192 NES khi sàn 192). Tim nhỏ chỉ vẽ 8 hàng trên của ô 8×16 ⇒ hộp riêng để tim chạm sàn.
/// Chuyển động (đo video — ConfigRule/ConfigActorParam, CVGP_MEMORY §2i):
///   rơi thường: vy bắt đầu 0, mỗi tick đi vy rồi vy += ItemGravity (0.25 — y = g·t(t−1)/2), chặn từng px (không xuyên sàn);
///   tim nhỏ: rơi đều 0.5 px/tick + vận tốc ngang đổi đều ±swayAccel giữa ±swayVx0 (lần đầu sang phải); chạm sàn thì đứng;
///   trồi (bí mật): đi lên SecretRiseSpeed tới vị trí đích, phần dưới mặt sàn bị cắt (sprite cắt hàng — bị sàn che như video).
/// Tồn tại ItemLifetimeTicks tick rồi biến mất (không nháy — video f940–967); gỡ khi tâm cách mép màn &lt; ItemDespawnInsetPx.
/// Nhặt = hộp item chồng hộp thân Simon ⇒ ItemEffects.Apply (1 chỗ duy nhất).
/// </summary>
public class ItemPickup : Entity
{
    public enum eMotion
    {
        Fall,
        Sway,
        Rise,
        Rest,
    }

    public static readonly RectInt BOX16 = new RectInt(-8, -16, 16, 16);
    /// <summary>Tim nhỏ: 8 hàng trên của ô 8×16 (đáy hộp = hàng tim cuối) — sprite heart_small hàng 0–7 đục.</summary>
    public static readonly RectInt BOX_SMALL_HEART = new RectInt(-4, -15, 8, 8);

    [NonSerialized] public eItemId id;
    [NonSerialized] public eMotion motion;
    [NonSerialized] public int age, lifetime;
    /// <summary>Đã nhặt (ItemEffects đã chạy) — harness đọc.</summary>
    [NonSerialized] public bool picked;
    /// <summary>Trồi: px còn phải đi lên (fixed).</summary>
    [NonSerialized] public int riseLeft;
    /// <summary>CVGP 20: quả cầu cuối (Dracula) — nhặt ⇒ GameRules.OnGameCleared.</summary>
    [NonSerialized] public bool finalCrystal;

    private int gravity, swayAccel, swayMax, swayDir, fallSmall, riseSpeed;
    private AnimSet.Anim anim;

    public override string DebugState => id + " " + motion + " t" + age;

    protected override void OnSpawned()
    {
        id = eItemId.None;
        motion = eMotion.Rest;
        age = 0;
        picked = false;
        riseLeft = 0;
        finalCrystal = false;
        vx = vy = 0;
        Facing = -1;
        if (Anim != null)
            Anim.Visible = false;
    }

    /// <summary>CVM2 10: tốc độ trồi riêng của 1 bí mật (fixed 1/256 px/tick, gọi ngay sau Setup) — Block 2 đo video 0.25; ≤ 0 ⇒ giữ ConfigRule.</summary>
    public void OverrideRiseSpeed(int fixed256)
    {
        if (fixed256 > 0)
            riseSpeed = fixed256;
    }

    /// <summary>Đặt loại item + chuyển động (gọi ngay sau World.Spawn). rise = trồi từ sau sàn (bí mật).</summary>
    public void Setup(eItemId itemId, bool rise)
    {
        id = itemId;
        var cfg = World.Config;
        var row = cfg != null ? cfg.GetItem(itemId) : null;
        lifetime = row != null && row.lifetimeTicks.has ? row.lifetimeTicks.value : 0;
        if (cfg != null)
        {
            gravity = cfg.rules[eRuleParam.ItemGravity].ToFixed256("ConfigRule.ItemGravity");
            offscreenMargin = -cfg.rules.GetInt(eRuleParam.ItemDespawnInsetPx, 8);
            riseSpeed = cfg.rules[eRuleParam.SecretRiseSpeed].ToFixed256("ConfigRule.SecretRiseSpeed");
            fallSmall = Fixed.FromFloat(Param(cfg, eItemId.SmallHeart, "fallSpeed", 0.5f));
            swayMax = Fixed.FromFloat(Param(cfg, eItemId.SmallHeart, "swayVx0", 1.125f));
            swayAccel = Fixed.FromFloat(Param(cfg, eItemId.SmallHeart, "swayAccel", 0.046875f));
        }
        body = itemId == eItemId.SmallHeart ? BOX_SMALL_HEART : BOX16;
        vx = vy = 0;
        if (rise)
        {
            motion = eMotion.Rise;
            int px = cfg != null ? cfg.rules.GetInt(eRuleParam.SecretRisePx, 16) : 16;
            riseLeft = Fixed.FromPx(px);
            fy += riseLeft;   // bắt đầu dưới đích (sau sàn), đi lên
        }
        else if (itemId == eItemId.SmallHeart)
        {
            motion = eMotion.Sway;
            vx = swayMax;
            swayDir = -1;
        }
        else
            motion = eMotion.Fall;
        string an = ItemCatalog.AnimName(itemId);
        anim = animSet != null && an != null ? animSet.Get(an) : null;
        if (Anim != null)
        {
            Anim.Visible = anim != null;
            if (anim != null)
                Anim.Play(anim, true);
        }
        SyncView();
    }

    public static float Param(GameConfig cfg, eItemId actor, string key, float fallback)
    {
        if (cfg == null || cfg.actorParams == null)
            return fallback;
        var r = cfg.actorParams.Find(ConfigActorParam.ActorKey(actor), key);
        return r != null && r.value.has ? r.value.value : fallback;
    }

    public override void OnTick()
    {
        age++;
        switch (motion)
        {
            case eMotion.Rise:
                {
                    int d = Mathf.Min(riseSpeed, riseLeft);
                    fy -= d;
                    riseLeft -= d;
                    if (riseLeft <= 0)
                        motion = eMotion.Rest;
                }
                break;
            case eMotion.Fall:
                if (MoveY(vy, false))
                {
                    vy = 0;
                    motion = eMotion.Rest;
                }
                else
                    vy += gravity;
                break;
            case eMotion.Sway:
                fx += vx;
                vx += swayDir * swayAccel;
                if (vx <= -swayMax) { vx = -swayMax; swayDir = 1; }
                else if (vx >= swayMax) { vx = swayMax; swayDir = -1; }
                if (MoveY(fallSmall, false))
                    motion = eMotion.Rest;
                break;
        }
        if (Alive && World.Tiles != null && Y - 16 > World.Tiles.Height)
            Kill(eKillReason.Offscreen);
        if (Alive && TryPickup())
            return;
        if (Alive && lifetime > 0 && age >= lifetime)
            Kill();
        base.OnTick();
    }

    /// <summary>Hộp item chồng hộp thân Simon (Player còn sống, không đang chết) ⇒ hiệu ứng + gỡ item.</summary>
    private bool TryPickup()
    {
        var p = World.Player as SimonController;
        if (p == null || !p.Alive || p.IsDying)
            return false;
        if (!CombatResolver.Overlaps(BodyRect, p.BodyRect))
            return false;
        picked = true;
        ItemEffects.Apply(World, this, p);
        Kill();
        return true;
    }

    public override void SyncView()
    {
        // Y đã gồm phần còn phải trồi ⇒ số hàng nằm dưới mặt sàn đích = riseLeft (px)
        int riseOff = motion == eMotion.Rise ? Fixed.ToPx(riseLeft) : 0;
        transform.localPosition = NesCoord.ToWorld(X + viewOffset.x, Y + viewOffset.y);
        if (Anim == null || anim == null)
            return;
        Anim.Apply(Facing);
        if (view == null || view.sprite == null)
            return;
        if (riseOff >= (int)view.sprite.rect.height)
            view.enabled = false;
        else
        {
            view.enabled = true;
            if (riseOff > 0)
                view.sprite = Cropped(view.sprite, riseOff);
        }
    }

    // ---------------------------------------------------------------- sprite cắt hàng dưới (trồi sau sàn)

    private static readonly Dictionary<long, Sprite> crops = new Dictionary<long, Sprite>();

    /// <summary>Bản sprite bỏ <paramref name="rows"/> hàng DƯỚI (giữ nguyên vị trí các hàng còn lại). Cache theo (sprite, rows).</summary>
    public static Sprite Cropped(Sprite s, int rows)
    {
        var r = s.rect;
        if (rows <= 0)
            return s;
        if (rows >= (int)r.height)
            rows = (int)r.height - 1;
        long key = ((long)s.GetInstanceID() << 8) | (uint)rows;
        Sprite c;
        if (crops.TryGetValue(key, out c) && c != null)
            return c;
        var nr = new Rect(r.x, r.y + rows, r.width, r.height - rows);
        var piv = new Vector2(s.pivot.x / nr.width, (s.pivot.y - rows) / nr.height);
        c = Sprite.Create(s.texture, nr, piv, s.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        c.name = s.name + "_crop" + rows;
        crops[key] = c;
        return c;
    }
}
