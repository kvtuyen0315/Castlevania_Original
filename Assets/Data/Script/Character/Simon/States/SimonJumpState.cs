using UnityEngine;

/// <summary>
/// Nhảy theo bảng độ cao (ConfigSimon.JumpRise01..16 + đỉnh JumpApexTicks + rơi đối xứng = 40 tick trên sàn phẳng, cao 36 px —
/// đồng hồ x video Map_1 2:03.7/3:23.6 dư 0 px). Ngang 1 px/tick theo hướng chốt lúc bấm A (không đổi giữa không trung),
/// đụng tường thì chỉ dừng ngang. Đáp sàn (kể cả bục cao hơn) ⇒ state đất; hết bảng chưa chạm đất ⇒ Fall (giữ vận tốc ngang).
/// </summary>
public class SimonJumpState : SimonState
{
    public override void OnEnterState()
    {
        owner.jumpTick = 0;
        owner.jumpLanded = false;
        owner.jumpTuckAfterAttack = false;
        owner.airVx = owner.jumpDir * owner.walkSpeed;
        if (owner.jumpDir != 0)
            owner.Facing = owner.jumpDir;
        owner.ApplyJumpPose();
    }

    public override void OnExecuteState()
    {
        owner.AirStep();
        owner.ApplyJumpPose();
    }

    public override void CheckNextState()
    {
        if (!owner.jumpLanded && owner.TryAttack())
            return;
        if (owner.jumpLanded)
            owner.ToGroundState();
        else if (owner.jumpTick >= owner.jumpTicks)
            owner.StartFall(owner.airVx);
    }

    public override void OnExitState()
    {
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
    }
}
