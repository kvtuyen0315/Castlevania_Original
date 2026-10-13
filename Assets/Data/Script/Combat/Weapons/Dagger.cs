/// <summary>Dao găm (CVGP 09): bay thẳng ngang ConfigSubWeapon.Dagger.speedX px/tick (đo video Map_1 1:33.0 / 1:47.1: 4.0),
/// xuyên tường (wiki không nói chặn; video không có tường), trúng 1 mục tiêu rồi biến mất (wiki Items#Dagger), ra khỏi màn ⇒ biến mất.</summary>
public class Dagger : SubWeaponProjectile
{
    private int speed;

    protected override void OnLaunch(GameConfig cfg)
    {
        speed = row != null ? Speed(row.speedX, "Dagger.speedX") : 0;
        vx = Facing * speed;
    }

    protected override void Move()
    {
        fx += vx;
    }
}
