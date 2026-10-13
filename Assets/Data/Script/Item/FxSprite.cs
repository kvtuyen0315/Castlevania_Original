using System;
using UnityEngine;

/// <summary>
/// Hiệu ứng ngắn (CVGP 11) — nhóm Effect, không va chạm: phát 1 anim của FXAnimSet rồi tự gỡ (one-shot hết khung cuối),
/// hoặc có vận tốc + trọng lực, xuyên địa hình, gỡ khi ra khỏi màn (mảnh gạch vỡ). Toạ độ = TÂM sprite (FX pivot tâm — CVGP 05).
/// Dùng: lửa vỡ nến/chậu (anim "death" — video Map_1 5 chậu: 8/9/3 tick), tia lửa trúng đòn ("hit"), mảnh gạch ("brick_debris").
/// Routine 13 dùng lại cho enemy chết.
/// </summary>
public class FxSprite : Entity
{
    [NonSerialized] public int age, life, gravity;
    private AnimSet.Anim anim;

    public override string DebugState => (anim != null ? anim.name : "-") + " t" + age;

    protected override void OnSpawned()
    {
        age = 0;
        life = 0;
        gravity = 0;
        vx = vy = 0;
        anim = null;
    }

    /// <summary>Sinh FX tại tâm (x, y). life &gt; 0 ⇒ gỡ sau life tick (thay cho hết anim); vx/vy/gravity fixed (0 = đứng yên).</summary>
    public static FxSprite Spawn(StageWorld world, string animName, int x, int y, int life = 0, int vx = 0, int vy = 0, int gravity = 0)
    {
        var cat = world.Items;
        if (cat == null || cat.fxPrefab == null)
            return null;
        var fx = world.Spawn(cat.fxPrefab, x, y, -1) as FxSprite;
        if (fx == null)
            return null;
        fx.life = life;
        fx.vx = vx;
        fx.vy = vy;
        fx.gravity = gravity;
        fx.anim = fx.animSet != null ? fx.animSet.Get(animName) : null;
        if (fx.Anim != null && fx.anim != null)
        {
            fx.Anim.Visible = true;
            fx.Anim.Play(fx.anim, true);
        }
        fx.SyncView();
        return fx;
    }

    public override void OnTick()
    {
        age++;
        bool moving = vx != 0 || vy != 0 || gravity != 0;
        if (moving)
        {
            fx += vx;
            fy += vy;
            vy += gravity;
            var t = World.Tiles;
            int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
            if ((t != null && Y - 8 > t.Height) || X < cx - 8 || X > cx + NesCoord.SCREEN_W + 8)
            {
                Kill(eKillReason.Offscreen);
                return;
            }
        }
        base.OnTick();
        // vật bay (mảnh gạch) sống tới khi ra khỏi màn; còn lại: hết life hoặc hết anim one-shot
        if (life > 0 ? age >= life : (!moving && (Anim == null || Anim.Done)))
            Kill();
    }
}
