using UnityEngine;

/// <summary>
/// Knockback (video Boss_Map_1 11.9–12.6 s): khung hurt_0, lùi ngược hướng mặt KnockbackDx px trong KnockbackDxTicks tick đầu rồi
/// chỉ còn rơi; cung dọc vy0 KnockbackVy0 + KnockbackGravity (cao ≈ 15 px, ≈ 35 tick), rơi tối đa FallSpeed. Suốt cung không nháy
/// (bất tử giữ đầy); đáp đất ⇒ bắt đầu đếm InvincibleTicks (nháy). Hết máu ⇒ Dead khi đáp.
/// </summary>
public class SimonHurtState : SimonState
{
    public override void OnEnterState()
    {
        owner.kbVy = owner.kbVy0;
        owner.kbLanded = false;
        owner.airVx = owner.kbDir * owner.kbVx;
        owner.PlayStatic(owner.aHurt);
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        owner.InvincibleTicks = owner.blinkTicks;
    }

    public override void OnExecuteState()
    {
        owner.InvincibleTicks = owner.blinkTicks;
        if (StateFrame <= owner.kbDxTicks)
            owner.StepX(owner.airVx);
        int vy = owner.kbVy;
        owner.kbVy = Mathf.Min(owner.kbVy + owner.kbGravity, owner.fallSpeed);
        if (owner.StepY(vy, true) && vy > 0)
        {
            owner.kbLanded = true;
            owner.landCount++;
        }
    }

    public override void CheckNextState()
    {
        if (!owner.kbLanded)
            return;
        if (GameSession.Current.playerHp <= 0)
            SetState(eSimonState.Dead);
        else
            owner.ToGroundState();
    }

    public override void OnExitState()
    {
    }
}
