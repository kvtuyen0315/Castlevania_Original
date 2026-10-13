using UnityEngine;

/// <summary>
/// Đồng hồ (CVGP 09 — CHỈ WIKI): không bay, không gây sát thương. Lúc sinh gọi StageWorld.FreezeEnemies(freezeTicks)
/// (ConfigSubWeapon.Stopwatch.freezeTicks = 300 — wiki 5 giây); tồn tại (vô hình) đúng thời gian đóng băng để Simon
/// không dùng chồng (SimonController.CanThrow). Boss miễn nhiễm trừ Phantom Bat/Queen Medusa = override Entity.IsFrozenBy
/// ở lớp boss (routine 14/19/20). Không bị multiplier ảnh hưởng (wiki Items#Stopwatch).
/// </summary>
public class Stopwatch : SubWeaponProjectile
{
    private int life;

    protected override void OnLaunch(GameConfig cfg)
    {
        Hit.Deactivate();
        life = row != null && row.freezeTicks.has ? row.freezeTicks.value : 0;
        if (life > 0)
            World.FreezeEnemies(life);
        if (Anim != null)
            Anim.Visible = false;
    }

    protected override void Move()
    {
        if (age >= life)
            Kill();
    }

    public override void SyncView()
    {
        if (Anim != null)
            Anim.Visible = false;
    }
}
