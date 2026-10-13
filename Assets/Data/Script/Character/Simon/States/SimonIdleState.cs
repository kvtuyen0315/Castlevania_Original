using UnityEngine;

/// <summary>Đứng yên trên đất. ↑/↓ gần chân/đỉnh thang ⇒ thang; ↓ ⇒ ngồi; ←/→ ⇒ đi; A ⇒ nhảy; mất sàn ⇒ rơi thẳng.</summary>
public class SimonIdleState : SimonState
{
    public override void OnEnterState()
    {
        owner.vx = 0;
        owner.drawOffset = Vector2Int.zero;
        owner.Hurt.local = owner.standHurt;
        if (owner.Anim != null)
            owner.Anim.Play(owner.aIdle);
    }

    public override void OnExecuteState()
    {
    }

    public override void CheckNextState()
    {
        if (owner.TryAttack())
            return;
        if (owner.GroundTransitions(0))
            return;
        if (owner.Held(eNesButton.Down))
            SetState(eSimonState.Crouch);
        else if (owner.InputX != 0)
            SetState(eSimonState.Walk);
    }

    public override void OnExitState()
    {
    }
}
