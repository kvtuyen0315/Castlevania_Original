using UnityEngine;

/// <summary>Đi WalkSpeed px/tick (đo video: 1.00, không gia tốc, dừng ngay khi thả); anim walk 7 tick/khung (AnimSet).
/// Bước khỏi mép ⇒ Fall giữ vận tốc ngang (video Map_1 2:05.3).</summary>
public class SimonWalkState : SimonState
{
    public override void OnEnterState()
    {
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        if (owner.Anim != null)
            owner.Anim.Play(owner.aWalk);
    }

    public override void OnExecuteState()
    {
        int dir = owner.InputX;
        if (dir == 0)
            return;
        owner.Facing = dir;
        owner.StepX(dir * owner.walkSpeed);
    }

    public override void CheckNextState()
    {
        int dir = owner.InputX;
        if (owner.GroundTransitions(dir * owner.walkSpeed))
            return;
        if (owner.Held(eNesButton.Down))
            SetState(eSimonState.Crouch);
        else if (dir == 0)
            SetState(eSimonState.Idle);
    }

    public override void OnExitState()
    {
    }
}
