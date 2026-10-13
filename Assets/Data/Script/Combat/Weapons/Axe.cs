/// <summary>
/// Rìu (CVGP 09): cung parabol — mỗi tick x += vx, y += vy rồi vy += g (ConfigSubWeapon.Axe: vx 2.0, vy0 −5.0, g 0.25 —
/// khớp 2 quỹ đạo video Map_1 3:27.9 / 3:29.3). Xuyên mục tiêu (video: trúng Phantom Bat rồi bay tiếp), xuyên địa hình
/// (video: rơi qua sàn phòng boss), ra khỏi màn/đáy section ⇒ biến mất. Anim xoay axe_0..3 mỗi khung 4 tick (đo video).
/// </summary>
public class Axe : SubWeaponProjectile
{
    private int gravity;

    protected override void OnLaunch(GameConfig cfg)
    {
        vx = row != null ? Facing * Speed(row.speedX, "Axe.speedX") : 0;
        vy = row != null ? Speed(row.speedY0, "Axe.speedY0") : 0;
        gravity = row != null ? Speed(row.gravity, "Axe.gravity") : 0;
    }

    protected override void Move()
    {
        fx += vx;
        fy += vy;
        vy += gravity;
    }
}
