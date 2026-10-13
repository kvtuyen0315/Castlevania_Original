using UnityEngine;

/// <summary>
/// Lửa nước thánh (CVGP 09): đứng yên trên mặt sàn (pivot đáy-giữa = mặt sàn), cháy ConfigActorParam HolyWater.fireTicks
/// tick (đo video Boss ≈ 80) với anim holywater_fire lặp (fire_1..4, nhịp đo video), đánh lại mọi mục tiêu chạm lửa mỗi
/// fireRehitTicks tick (quy ước — Hitbox.Activate lại xoá danh sách đã trúng; mục tiêu đang bất tử vẫn bỏ qua).
/// Giữ chân enemy/boss trong lửa là hành vi của bên nhận (routine 13/14 xét IsHolyFire).
/// </summary>
public class HolyFire : SubWeaponProjectile
{
    private int life, rehit;

    public static bool IsHolyFire(Hitbox h)
    {
        return h != null && h.owner is HolyFire;
    }

    protected override void OnLaunch(GameConfig cfg)
    {
        life = Mathf.Max(1, Mathf.RoundToInt(Param(cfg, "fireTicks", 80)));
        rehit = Mathf.Max(1, Mathf.RoundToInt(Param(cfg, "fireRehitTicks", 8)));
        vx = vy = 0;
    }

    protected override void Move()
    {
        if (age >= life)
        {
            Kill();
            return;
        }
        if (age % rehit == 0)
            Hit.Activate(hitRect);
    }
}
