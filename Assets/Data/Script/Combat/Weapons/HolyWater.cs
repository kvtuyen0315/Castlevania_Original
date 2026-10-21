using UnityEngine;

/// <summary>
/// Lọ nước thánh (CVGP 09; CVM3 04 đo lại video Map_3 trận Mummy 9 lọ + 1 lọ nhảy-ném): cung thấp — vy += g, x += vx, y += vy
/// (ConfigSubWeapon.HolyWater: vx 1.5, vy0 −1.5, g 33/256; sinh tại X Simon, Y chân − 19). Điểm dò sàn (tâm lọ + floorProbePx) vào ô đặc
/// khi đang rơi ⇒ vỡ thành HolyFire trên mặt sàn đó
/// (+ SFX HolyWater — routine 02 "khả năng"). Trúng trực tiếp mục tiêu ⇒ 1 đòn (yếu) rồi BAY XUYÊN tiếp, chỉ vỡ khi chạm sàn
/// (ConfigSubWeapon.HolyWater.pierces = 1 ⇒ OnSpent không được gọi; CVM2 04 xác nhận bằng video Boss_Map_1 f467–483 + Boss_Map_2 f134–149,
/// f383–402: lọ đi qua thân Phantom Bat/Medusa, ENEMY −1 lúc lọ chạm, vỡ ở sàn phía sau — kiểm: HolyPierceProbe). Boss đứng khựng suốt lúc
/// lọ chạm (BossBase.TouchingHolyWater). Không va tường (suy luận — video không có). Lửa tính vào giới hạn số vũ khí trên màn.
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
        floorProbe = Mathf.RoundToInt(Param(cfg, "floorProbePx", bottomOffset));
    }

    // CVM3 04: điểm dò sàn so với tâm lọ (px) — video Map_3 9 lọ trận Mummy: lọ còn vẽ ở Y 204 (đáy 211, lấn sàn 208 3 px) rồi vỡ tick sau ⇒ dò ở tâm (0)
    private int floorProbe;

    protected override void Move()
    {
        // CVM3 04: vy += g TRƯỚC rồi mới dời (khớp 46 điểm quỹ đạo video Map_3 dư 0 px với vy0 −1.5, g 33/256 — Tools/cvm3/kb4_holy_fit.py)
        int oldBottom = Y + floorProbe;
        vy += gravity;
        fx += vx;
        fy += vy;
        if (vy <= 0)
            return;
        int newBottom = Y + floorProbe;
        // quét từng hàng đáy đã đi qua ⇒ không xuyên sàn ở mọi tốc độ
        for (int y = oldBottom + 1; y <= newBottom; y++)
            if (World.Tiles.IsSolid(X, y))
            {
                Break(y);
                return;
            }
    }

    /// <summary>Chỉ khi Config đặt pierces = 0 (không dùng — video: lọ xuyên địch): vỡ xuống sàn ngay dưới chỗ trúng.</summary>
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
