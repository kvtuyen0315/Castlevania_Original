using UnityEngine;

/// <summary>
/// Đồng hồ (CVGP 09; CVM3 03 đo video): không bay, không gây sát thương. Sinh NGAY tick bấm (SimonController.StartAttack — Simon
/// vẫn vung roi) rồi gọi StageWorld.FreezeEnemies(freezeTicks) (ConfigSubWeapon.Stopwatch.freezeTicks = 186 — video Map_2, CVM3_MEMORY §2d);
/// tồn tại (vô hình) đúng thời gian đóng băng. Dùng lại khi còn hiệu lực (video f6928) ⇒ đồng hồ cũ bị gỡ, đóng băng tính lại từ tick bấm
/// (FreezeEnemies đặt lại). Trong lúc đóng băng TIME không giảm
/// (StageRules: nhịp TIME vẫn chạy, mốc rơi vào lúc băng bị bỏ) và bẫy ép chạy lại ở mốc lưới kế tiếp (Crusher).
/// Boss miễn nhiễm trừ Phantom Bat/Queen Medusa = override Entity.IsFrozenBy ở lớp boss (routine 14/19/20). Không bị multiplier ảnh hưởng (wiki Items#Stopwatch).
/// </summary>
public class Stopwatch : SubWeaponProjectile
{
    private int life;

    protected override void OnLaunch(GameConfig cfg)
    {
        Hit.Deactivate();
        // dùng lại khi đang hiệu lực ⇒ gỡ đồng hồ cũ (1 đồng hồ trên thế giới)
        var list = World.GroupList(eEntityGroup.PlayerWeapon);
        for (int i = 0; i < list.Count; i++)
            if (list[i] != this && list[i].Alive && list[i] is Stopwatch)
                list[i].Kill();
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
