using UnityEngine;

/// <summary>
/// Lọ nước thánh (CVGP 09): cung thấp — x += vx, y += vy, vy += g (ConfigSubWeapon.HolyWater: vx 1.5, vy0 −1.3, g 0.125 —
/// khớp 2 quỹ đạo video Boss 13.0–13.9 s). Đáy lọ chạm mặt ô đặc khi đang rơi ⇒ vỡ thành HolyFire trên mặt sàn đó
/// (+ SFX HolyWater — routine 02 "khả năng"). Trúng trực tiếp mục tiêu ⇒ vỡ, lửa rơi xuống sàn ngay dưới (wiki: "có thể trúng
/// trực tiếp" — cách vỡ là SUY LUẬN). Không va tường (suy luận — video không có). Lửa tính vào giới hạn số vũ khí trên màn.
/// </summary>
public class HolyWater : SubWeaponProjectile
{
    [SerializeField] private HolyFire firePrefab;
    [Tooltip("Hàng đáy của lọ so với tâm (px) — builder đo từ bbox pixel đục (holywater_bottle hàng 8–15 ⇒ +8).")]
    [SerializeField] private int bottomOffset = 8;

    private int gravity;

    public HolyFire FirePrefab { get => firePrefab; set => firePrefab = value; }
    public int BottomOffset { get => bottomOffset; set => bottomOffset = value; }

    protected override void OnLaunch(GameConfig cfg)
    {
        vx = row != null ? Facing * Speed(row.speedX, "HolyWater.speedX") : 0;
        vy = row != null ? Speed(row.speedY0, "HolyWater.speedY0") : 0;
        gravity = row != null ? Speed(row.gravity, "HolyWater.gravity") : 0;
    }

    protected override void Move()
    {
        int oldBottom = Y + bottomOffset;
        fx += vx;
        fy += vy;
        vy += gravity;
        if (vy <= 0)
            return;
        int newBottom = Y + bottomOffset;
        // quét từng hàng đáy đã đi qua ⇒ không xuyên sàn ở mọi tốc độ
        for (int y = oldBottom + 1; y <= newBottom; y++)
            if (World.Tiles.IsSolid(X, y))
            {
                Break(y);
                return;
            }
    }

    protected override void OnSpent()
    {
        int floor = FloorBelow(X, Y + bottomOffset, 64);
        Break(floor >= 0 ? floor : Y + bottomOffset);
    }

    /// <summary>Vỡ ở mặt sàn floorY: sinh lửa (đáy-giữa) cùng hướng, tự biến mất.</summary>
    private void Break(int floorY)
    {
        if (!Alive)
            return;
        if (firePrefab != null)
        {
            var f = World.Spawn(firePrefab, X, floorY, Facing) as HolyFire;
            if (f != null)
                f.Thrower = Thrower;
        }
        if (AudioManager.I != null)
            AudioManager.I.PlaySfx(eSfx.HolyWater);
        Kill();
    }
}
