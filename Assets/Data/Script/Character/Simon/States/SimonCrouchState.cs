using UnityEngine;

/// <summary>Ngồi (giữ ↓): khung jumpduck_0 vẽ thấp CrouchDrawY (video Map_1 1:02.8), hurtbox thấp CrouchHurtboxHeight;
/// không đi được, ←/→ chỉ quay mặt; A ⇒ nhảy; thả ↓ ⇒ đứng.</summary>
public class SimonCrouchState : SimonState
{
    public override void OnEnterState()
    {
        owner.vx = 0;
        owner.PlayStatic(owner.aDuck);
        owner.drawOffset = new Vector2Int(0, owner.crouchDrawY);
        owner.Hurt.local = owner.crouchHurt;
    }

    public override void OnExecuteState()
    {
        if (owner.InputX != 0)
            owner.Facing = owner.InputX;
    }

    public override void CheckNextState()
    {
        if (!owner.OnGround())
        {
            owner.StartFall(0);
            return;
        }
        if (owner.TryAttack())
            return;
        if (owner.Pressed(eNesButton.A))
        {
            owner.StartJump(owner.InputX);
            return;
        }
        if (!owner.Held(eNesButton.Down))
            owner.ToGroundState();
    }

    public override void OnExitState()
    {
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
    }
}
