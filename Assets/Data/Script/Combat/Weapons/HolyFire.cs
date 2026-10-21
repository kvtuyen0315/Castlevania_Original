using UnityEngine;

/// <summary>
/// Lửa nước thánh (CVGP 09): đứng yên trên mặt sàn (pivot đáy-giữa = mặt sàn; hàng đáy lửa vẽ ở mặt sàn − 1 — video Boss_Map_1 f483–500), cháy ConfigActorParam
/// HolyWater.fireTicks tick (CVM3 04 đo video Map_3: 76) với anim holywater_fire fire_0→4→0→3 mỗi 9 tick (không lặp), đánh lại mọi mục tiêu chạm lửa mỗi
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
        life = Mathf.Max(1, Mathf.RoundToInt(Param(cfg, "fireTicks", 76)));
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
