using UnityEngine;

/// <summary>
/// Thập tự / Boomerang (CVGP 09 — CHỈ WIKI, không có trong video): bay ngang ConfigSubWeapon.Cross.speedX px/tick (quy ước),
/// đi đủ turnDistancePx hoặc tới gần mép màn (turnScreenMarginPx) thì quay đầu cùng tốc độ; xuyên mọi mục tiêu, lượt về đánh
/// lại được (Hitbox.Activate mới khi quay đầu — wiki Items#Cross "trúng cả lượt về"); chạm thân Simon lúc về ⇒ bắt được (biến
/// mất); Simon nhảy qua thì bay tiếp ra sau lưng và biến mất ngoài màn (wiki). Xuyên địa hình (suy luận).
/// </summary>
public class Cross : SubWeaponProjectile
{
    private int turnDist, screenMargin, travelled, speed;
    private bool turned;

    public bool Turned => turned;

    protected override void OnLaunch(GameConfig cfg)
    {
        speed = row != null ? Speed(row.speedX, "Cross.speedX") : 0;
        vx = Facing * speed;
        turnDist = Fixed.FromPx(Mathf.RoundToInt(Param(cfg, "turnDistancePx", 112)));
        screenMargin = Mathf.RoundToInt(Param(cfg, "turnScreenMarginPx", 8));
        travelled = 0;
        turned = false;
    }

    protected override void Move()
    {
        fx += vx;
        if (!turned)
        {
            travelled += speed;
            int cx = World.CameraCtl != null ? World.CameraCtl.X : 0;
            bool edge = vx > 0 ? X >= cx + NesCoord.SCREEN_W - screenMargin : X < cx + screenMargin;
            if (travelled >= turnDist || edge)
            {
                turned = true;
                vx = -vx;
                Hit.Activate(hitRect);
            }
            return;
        }
        // bắt lại: hộp đánh chạm thân người ném
        if (Thrower != null && Thrower.Alive && Overlaps(Hit.SectionRect, Thrower.BodyRect))
            Kill();
    }

    private static bool Overlaps(RectInt a, RectInt b)
    {
        return a.xMin < b.xMax && b.xMin < a.xMax && a.yMin < b.yMax && b.yMin < a.yMax;
    }
}
