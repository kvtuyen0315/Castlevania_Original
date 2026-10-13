using UnityEngine;

/// <summary>Ngồi khựng sau khi rơi cao (video Map_1 1:02.8: rơi 64 px ⇒ ngồi ~20 tick rồi mới đi): khung ngồi, bỏ qua input.</summary>
public class SimonLandState : SimonState
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
    }

    public override void CheckNextState()
    {
        if (StateFrame >= owner.landCrouchTicks)
            owner.ToGroundState();
    }

    public override void OnExitState()
    {
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
    }
}
