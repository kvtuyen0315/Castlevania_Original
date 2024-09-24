using UnityEngine;

public class JumpState : BaseState
{
    bool _isAttack { get; set; }

    public override void OnEnterState()
    {
        base.OnEnterState();
        character.PlayAnim(config);
        character.Jump();
        character.Move(character.curDirectionH);
    }

    public override void OnExecuteState()
    {
        base.OnExecuteState();

        // AAA - check isGround to stand.
        if (character.isGround)
        {
            if (_isAttack)
            {
                Debug.LogError($"is on Attack when jump");
                return;
            }

            if (character.controller.moveX != 0)
                SetState(eStateType.walk);
            else
                SetState(eStateType.idle);
        }
    }

    public override void CheckNextState()
    {
        base.CheckNextState();

        if (character.controller.isAttack)
        {
            _isAttack = true;
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        character.StopMove();
        _isAttack = false;
    }
}
