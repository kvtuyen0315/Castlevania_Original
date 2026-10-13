using UnityEngine;

/// <summary>Rơi FallSpeed px/tick (hằng — video Map_1 2:05.3 / 3:24.1), giữ vận tốc ngang lúc rời mép/hết bảng nhảy, khung đứng.
/// Đáp: rơi ≥ LandCrouchMinFallTicks tick ⇒ Land (ngồi khựng — video 1:02.8), không thì state đất.</summary>
public class SimonFallState : SimonState
{
    public override void OnEnterState()
    {
        owner.fallTicks = 0;
        owner.fallLanded = false;
        owner.PlayStatic(owner.aIdle);
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
    }

    public override void OnExecuteState()
    {
        if (owner.airVx != 0)
            owner.StepX(owner.airVx);
        if (!owner.CanStepY)
            return;
        owner.fallTicks++;
        if (owner.StepY(owner.fallSpeed))
        {
            owner.fallLanded = true;
            owner.landCount++;
        }
    }

    public override void CheckNextState()
    {
        if (!owner.fallLanded && owner.TryAttack())
            return;
        if (!owner.fallLanded)
            return;
        if (owner.fallTicks >= owner.landCrouchMinFall)
            SetState(eSimonState.Land);
        else
            owner.ToGroundState();
    }

    public override void OnExitState()
    {
    }
}
